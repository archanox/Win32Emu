using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Memory;
using Iced.Intel;

namespace Win32Emu.Cpu.Jit;

/// <summary>
/// JIT-based x86 CPU emulator that compiles x86 code to .NET CIL for improved performance
/// and native async/await support
/// </summary>
public class JitCpu : IAsyncCpu
{
	private readonly VirtualMemory _mem;
	private readonly ILogger _logger;
	
	// CPU state - same as IcedCpu
	private uint _eax, _ebx, _ecx, _edx, _esi, _edi, _ebp, _esp, _eip, _eflags;
	
	// FPU state
	private readonly double[] _fpu = new double[8];
	private int _fpuTop = 0;
	private ushort _fpuControlWord = 0x037F;
	private ushort _fpuStatusWord = 0x0000;
	
	// JIT compilation infrastructure
	private readonly Dictionary<uint, CompiledBlock> _compiledBlocks = new();
	private readonly AssemblyBuilder _assemblyBuilder;
	private readonly ModuleBuilder _moduleBuilder;
	private int _blockCounter = 0;
	
	// JIT cache for persistent storage
	private readonly JitCache _jitCache;
	private string? _currentExecutablePath;
	
	// Decoder for analyzing x86 instructions before compilation
	private readonly Decoder _decoder;
	private readonly SimpleMemoryCodeReader _reader;

	public JitCpu(VirtualMemory mem, ILogger? logger = null)
	{
		_mem = mem;
		_logger = logger ?? NullLogger.Instance;
		_reader = new SimpleMemoryCodeReader(this);
		_decoder = Decoder.Create(32, _reader, DecoderOptions.None);
		
		// Create a dynamic assembly for JIT compilation
		var assemblyName = new AssemblyName("Win32Emu.Jit.Dynamic");
		_assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
		_moduleBuilder = _assemblyBuilder.DefineDynamicModule("JitModule");
		
		// Initialize JIT cache with default directory
		_jitCache = new JitCache(null, logger);
		
		_logger.LogInformation("[JitCpu] Initialized JIT CPU backend with caching");
	}
	
	/// <summary>
	/// Creates a new JitCpu instance with a custom cache directory
	/// </summary>
	public JitCpu(VirtualMemory mem, ILogger? logger, string cacheDirectory) : this(mem, logger)
	{
		// Replace the default cache with one using the custom directory
		_jitCache = new JitCache(cacheDirectory, logger);
	}

	public void SetEip(uint eip) => _eip = eip;
	public uint GetEip() => _eip;

	public uint GetRegister(string name) => name.ToUpperInvariant() switch
	{
		"EAX" => _eax, "EBX" => _ebx, "ECX" => _ecx, "EDX" => _edx, 
		"ESI" => _esi, "EDI" => _edi, "EBP" => _ebp, "ESP" => _esp, 
		"EIP" => _eip, "EFLAGS" => _eflags,
		_ => 0
	};

	public void SetRegister(string name, uint value)
	{
		switch (name.ToUpperInvariant())
		{
			case "EAX": _eax = value; break;
			case "EBX": _ebx = value; break;
			case "ECX": _ecx = value; break;
			case "EDX": _edx = value; break;
			case "ESI": _esi = value; break;
			case "EDI": _edi = value; break;
			case "EBP": _ebp = value; break;
			case "ESP": _esp = value; break;
			case "EIP": _eip = value; break;
			case "EFLAGS": _eflags = value; break;
		}
	}

	public CpuStepResult SingleStep(VirtualMemory mem)
	{
		return InterpretSingleInstruction(mem);
	}

	public Task<CpuStepResult> SingleStepAsync(VirtualMemory mem)
	{
		var result = InterpretSingleInstruction(mem);
		return Task.FromResult(result);
	}

	public async Task<CpuStepResult> ExecuteBlockAsync(VirtualMemory mem)
	{
		var blockStart = _eip;
		
		if (!_compiledBlocks.TryGetValue(blockStart, out var compiledBlock))
		{
			compiledBlock = CompileBlock(blockStart, mem);
			_compiledBlocks[blockStart] = compiledBlock;
		}
		
		var result = await compiledBlock.ExecuteAsync(this, mem);
		return result;
	}

	public bool SupportsJit => true;

	/// <summary>
	/// Sets the current executable path for cache management
	/// </summary>
	public void SetExecutablePath(string executablePath)
	{
		_currentExecutablePath = executablePath;
		_logger.LogInformation("[JitCpu] Set executable path: {ExecutablePath}", executablePath);
	}
	
	/// <summary>
	/// Loads JIT cache from disk for the current executable
	/// </summary>
	public async Task LoadCacheAsync()
	{
		if (string.IsNullOrEmpty(_currentExecutablePath))
		{
			_logger.LogWarning("[JitCpu] Cannot load cache: executable path not set");
			return;
		}
		
		await _jitCache.LoadCacheAsync(_currentExecutablePath);
		var stats = _jitCache.GetStatistics();
		_logger.LogInformation("[JitCpu] Cache loaded: {TotalBlocks} blocks, {TotalInstructions} instructions",
			stats.TotalBlocks, stats.TotalInstructions);
	}
	
	/// <summary>
	/// Saves JIT cache to disk for the current executable
	/// </summary>
	public async Task SaveCacheAsync()
	{
		if (string.IsNullOrEmpty(_currentExecutablePath))
		{
			_logger.LogWarning("[JitCpu] Cannot save cache: executable path not set");
			return;
		}
		
		await _jitCache.SaveCacheAsync(_currentExecutablePath);
	}
	
	/// <summary>
	/// Precompiles common code blocks to warm up the JIT cache.
	/// This compiles all blocks found in the cache for the current executable.
	/// </summary>
	public async Task<int> PrecompileFromCacheAsync(VirtualMemory mem)
	{
		var cachedAddresses = _jitCache.GetCachedBlockAddresses().OrderBy(a => a).ToList();
		
		if (cachedAddresses.Count == 0)
		{
			_logger.LogInformation("[JitCpu] No cached blocks to precompile");
			return 0;
		}
		
		_logger.LogInformation("[JitCpu] Starting precompilation of {TotalBlocks} cached blocks", cachedAddresses.Count);
		
		var compiled = 0;
		foreach (var address in cachedAddresses)
		{
			try
			{
				// Skip if already compiled
				if (_compiledBlocks.ContainsKey(address))
				{
					continue;
				}
				
				// Compile the block
				var block = CompileBlock(address, mem);
				_compiledBlocks[address] = block;
				compiled++;
			}
			catch (Exception ex)
			{
				_logger.LogDebug(ex, "[JitCpu] Failed to precompile cached block at 0x{Address:X8}", address);
			}
		}
		
		_logger.LogInformation("[JitCpu] Precompilation complete: {Compiled} blocks compiled from cache", compiled);
		
		return await Task.FromResult(compiled);
	}
	
	/// <summary>
	/// Precompiles a specific address range to warm up the JIT cache
	/// </summary>
	public async Task<int> PrecompileRangeAsync(VirtualMemory mem, uint startAddress, uint endAddress)
	{
		if (startAddress >= endAddress)
		{
			_logger.LogWarning("[JitCpu] Invalid address range for precompilation: 0x{Start:X8} - 0x{End:X8}",
				startAddress, endAddress);
			return 0;
		}
		
		_logger.LogInformation("[JitCpu] Precompiling address range: 0x{Start:X8} - 0x{End:X8}",
			startAddress, endAddress);
		
		var compiled = 0;
		var currentAddress = startAddress;
		
		// Analyze and compile blocks in the range
		while (currentAddress < endAddress)
		{
			try
			{
				// Check if block is already compiled
				if (_compiledBlocks.ContainsKey(currentAddress))
				{
					// Skip this block - get its length to advance properly
					if (_jitCache.TryGetBlockMetadata(currentAddress, out var existingMeta) && existingMeta != null)
					{
						currentAddress += (uint)existingMeta.ByteLength;
					}
					else
					{
						// If we don't know the block length, advance by a minimum instruction size
						currentAddress += 1;
					}
					continue;
				}
				
				// Compile the block
				var block = CompileBlock(currentAddress, mem);
				_compiledBlocks[currentAddress] = block;
				compiled++;
				
				// Advance by the block's actual byte length
				if (_jitCache.TryGetBlockMetadata(currentAddress, out var metadata) && metadata != null)
				{
					currentAddress += (uint)metadata.ByteLength;
				}
				else
				{
					// Fallback: advance by minimum instruction size if metadata is unavailable
					currentAddress += 1;
				}
			}
			catch (Exception ex)
			{
				_logger.LogDebug(ex, "[JitCpu] Failed to precompile block at 0x{Address:X8}", currentAddress);
				// On error, advance by minimum instruction size to avoid infinite loop
				currentAddress += 1;
			}
		}
		
		_logger.LogInformation("[JitCpu] Precompilation complete: {Compiled} blocks compiled in range", compiled);
		
		return await Task.FromResult(compiled);
	}
	
	/// <summary>
	/// Gets statistics about the JIT cache
	/// </summary>
	public CacheStatistics GetCacheStatistics()
	{
		return _jitCache.GetStatistics();
	}

	public CpuState SaveState()
	{
		return new CpuState
		{
			Eax = _eax, Ebx = _ebx, Ecx = _ecx, Edx = _edx,
			Esi = _esi, Edi = _edi, Ebp = _ebp, Esp = _esp,
			Eip = _eip, Eflags = _eflags,
			FpuStack = (double[])_fpu.Clone(),
			FpuTop = _fpuTop,
			FpuControlWord = _fpuControlWord,
			FpuStatusWord = _fpuStatusWord
		};
	}

	public void RestoreState(CpuState state)
	{
		_eax = state.Eax; _ebx = state.Ebx; _ecx = state.Ecx; _edx = state.Edx;
		_esi = state.Esi; _edi = state.Edi; _ebp = state.Ebp; _esp = state.Esp;
		_eip = state.Eip; _eflags = state.Eflags;
		
		if (state.FpuStack != null)
		{
			Array.Copy(state.FpuStack, _fpu, 8);
			_fpuTop = state.FpuTop;
			_fpuControlWord = state.FpuControlWord;
			_fpuStatusWord = state.FpuStatusWord;
		}
	}

	private CpuStepResult InterpretSingleInstruction(VirtualMemory mem)
	{
		_reader.Reset(_eip);
		_decoder.IP = _eip;
		var insn = _decoder.Decode();
		
		_eip = (uint)_decoder.IP;
		
		var isCall = insn.Mnemonic == Mnemonic.Call;
		uint callTarget = 0;
		
		if (isCall && insn.Op0Kind == OpKind.NearBranch32)
		{
			callTarget = (uint)insn.NearBranch32;
		}
		
		switch (insn.Mnemonic)
		{
			// === Basic instructions (already implemented) ===
			case Mnemonic.Nop:
				break;
			case Mnemonic.Int3:
				break;
			case Mnemonic.Call:
				_esp -= 4;
				mem.Write32(_esp, _eip);
				if (insn.Op0Kind == OpKind.NearBranch32)
				{
					_eip = callTarget;
				}
				else
				{
					_logger.LogWarning("[JitCpu] Unimplemented CALL type: {Op0Kind} at EIP=0x{OldEip:X8}", insn.Op0Kind, _eip - (uint)insn.Length);
				}
				break;
			case Mnemonic.Ret:
				_eip = mem.Read32(_esp);
				_esp += 4;
				if (insn.OpCount > 0 && insn.Op0Kind == OpKind.Immediate16)
				{
					_esp += (uint)insn.Immediate16;
				}
				break;
				
			// === Pentium CPU Instructions (Stubbed) ===
			// These are recognized but not yet fully implemented in JIT mode
			// They will be properly compiled when JIT compilation is complete
			
			// Integer arithmetic
			case Mnemonic.Aaa:
			case Mnemonic.Aas:
			case Mnemonic.Cbw:
			case Mnemonic.Cwde:
				ExecBcdArithmetic(insn);
				break;
			
			// Bit manipulation
			case Mnemonic.Bsf:
			case Mnemonic.Bsr:
			case Mnemonic.Btc:
			case Mnemonic.Btr:
			case Mnemonic.Bts:
				ExecBitManipulation(insn);
				break;
			
			// Conditional jumps
			case Mnemonic.Je:
			case Mnemonic.Jne:
			case Mnemonic.Ja:
			case Mnemonic.Jae:
			case Mnemonic.Jb:
			case Mnemonic.Jbe:
			case Mnemonic.Jg:
			case Mnemonic.Jge:
			case Mnemonic.Jl:
			case Mnemonic.Jle:
			case Mnemonic.Jo:
			case Mnemonic.Jno:
			case Mnemonic.Js:
			case Mnemonic.Jns:
			case Mnemonic.Jp:
			case Mnemonic.Jnp:
			case Mnemonic.Jcxz:
			case Mnemonic.Jecxz:
				ExecConditionalJump(insn);
				break;
			
			// Conditional moves
			case Mnemonic.Cmovae:
			case Mnemonic.Cmovle:
			case Mnemonic.Cmovno:
			case Mnemonic.Cmovnp:
			case Mnemonic.Cmovns:
			case Mnemonic.Cmovo:
			case Mnemonic.Cmovp:
			case Mnemonic.Cmovs:
				_logger.LogDebug("[JitCpu] Stubbed conditional move: {Mnemonic}", insn.Mnemonic);
				break;
			
			// Control flow
			case Mnemonic.Retf:
			case Mnemonic.Into:
				_logger.LogDebug("[JitCpu] Stubbed control flow instruction: {Mnemonic}", insn.Mnemonic);
				break;
			
			// System instructions
			case Mnemonic.Hlt:
			case Mnemonic.Bound:
			case Mnemonic.Enter:
			case Mnemonic.Clts:
				_logger.LogDebug("[JitCpu] Stubbed system instruction: {Mnemonic}", insn.Mnemonic);
				break;
			
			// Segment operations
			case Mnemonic.Lds:
			case Mnemonic.Les:
			case Mnemonic.Lfs:
			case Mnemonic.Lgs:
			case Mnemonic.Lss:
			case Mnemonic.Lar:
			case Mnemonic.Lsl:
			case Mnemonic.Lgdt:
			case Mnemonic.Sgdt:
			case Mnemonic.Lidt:
			case Mnemonic.Sidt:
			case Mnemonic.Lldt:
			case Mnemonic.Ltr:
			case Mnemonic.Str:
			case Mnemonic.Verr:
			case Mnemonic.Verw:
				_logger.LogDebug("[JitCpu] Stubbed segment/descriptor instruction: {Mnemonic}", insn.Mnemonic);
				break;
			
			// Shift double
			case Mnemonic.Shld:
			case Mnemonic.Shrd:
				ExecDoubleShift(insn);
				break;
			
			// String operations
			case Mnemonic.Lodsw:
				_logger.LogDebug("[JitCpu] Stubbed string instruction: {Mnemonic}", insn.Mnemonic);
				break;
			
			// I/O operations
			case Mnemonic.Out:
				_logger.LogDebug("[JitCpu] Stubbed I/O instruction: {Mnemonic}", insn.Mnemonic);
				break;
			
			// FPU instructions (x87)
			case Mnemonic.Fclex:
			case Mnemonic.Fcmovb:
			case Mnemonic.Fcmovbe:
			case Mnemonic.Fcmove:
			case Mnemonic.Fcmovnb:
			case Mnemonic.Fcmovne:
			case Mnemonic.Fcmovnu:
			case Mnemonic.Fcmovu:
			case Mnemonic.Fcomi:
			case Mnemonic.Fcomip:
			case Mnemonic.Fdecstp:
			case Mnemonic.Ffree:
			case Mnemonic.Ffreep:
			case Mnemonic.Ficom:
			case Mnemonic.Ficomp:
			case Mnemonic.Fincstp:
			case Mnemonic.Finit:
			case Mnemonic.Fisubr:
			case Mnemonic.Fldenv:
			case Mnemonic.Fldl2t:
			case Mnemonic.Fldlg2:
			case Mnemonic.Fldln2:
			case Mnemonic.Fnop:
			case Mnemonic.Fprem:
			case Mnemonic.Fprem1:
			case Mnemonic.Fptan:
			case Mnemonic.Frndint:
			case Mnemonic.Frstor:
			case Mnemonic.Fsave:
			case Mnemonic.Fstcw:
			case Mnemonic.Fstenv:
			case Mnemonic.Fstsw:
			case Mnemonic.Ftst:
			case Mnemonic.Fucom:
			case Mnemonic.Fucomp:
			case Mnemonic.Fucompp:
			case Mnemonic.Fxtract:
			case Mnemonic.Fyl2x:
			case Mnemonic.Fyl2xp1:
				_logger.LogDebug("[JitCpu] Stubbed FPU instruction: {Mnemonic}", insn.Mnemonic);
				break;
			
			// MMX instructions
			case Mnemonic.Emms:
			case Mnemonic.Movd:
			case Mnemonic.Movq:
			case Mnemonic.Packssdw:
			case Mnemonic.Packsswb:
			case Mnemonic.Packuswb:
			case Mnemonic.Paddb:
			case Mnemonic.Paddd:
			case Mnemonic.Paddsb:
			case Mnemonic.Paddsw:
			case Mnemonic.Paddusb:
			case Mnemonic.Paddusw:
			case Mnemonic.Paddw:
			case Mnemonic.Pand:
			case Mnemonic.Pandn:
			case Mnemonic.Pcmpeqb:
			case Mnemonic.Pcmpeqd:
			case Mnemonic.Pcmpeqw:
			case Mnemonic.Pcmpgtb:
			case Mnemonic.Pcmpgtd:
			case Mnemonic.Pcmpgtw:
			case Mnemonic.Pmaddwd:
			case Mnemonic.Pmulhw:
			case Mnemonic.Pmullw:
			case Mnemonic.Por:
			case Mnemonic.Pslld:
			case Mnemonic.Psllq:
			case Mnemonic.Psllw:
			case Mnemonic.Psrad:
			case Mnemonic.Psraw:
			case Mnemonic.Psrld:
			case Mnemonic.Psrlq:
			case Mnemonic.Psrlw:
			case Mnemonic.Psubb:
			case Mnemonic.Psubd:
			case Mnemonic.Psubsb:
			case Mnemonic.Psubsw:
			case Mnemonic.Psubusb:
			case Mnemonic.Psubusw:
			case Mnemonic.Psubw:
			case Mnemonic.Punpckhbw:
			case Mnemonic.Punpckhdq:
			case Mnemonic.Punpckhwd:
			case Mnemonic.Punpcklbw:
			case Mnemonic.Punpckldq:
			case Mnemonic.Punpcklwd:
			case Mnemonic.Pxor:
				_logger.LogDebug("[JitCpu] Stubbed MMX instruction: {Mnemonic}", insn.Mnemonic);
				break;
			
			default:
				_logger.LogWarning("[JitCpu] Unimplemented instruction: {Mnemonic}", insn.Mnemonic);
				break;
		}
		
		return new CpuStepResult(isCall, callTarget);
	}

	private CompiledBlock CompileBlock(uint startEip, VirtualMemory mem)
	{
		// Check if we have cached metadata for this block
		BlockMetadata? cachedMetadata = null;
		var hasCachedMetadata = _jitCache.TryGetBlockMetadata(startEip, out cachedMetadata);
		
		if (hasCachedMetadata && cachedMetadata != null)
		{
			_logger.LogDebug("[JitCpu] Found cached metadata for block at EIP=0x{Eip:X8}", startEip);
			
			// Verify the code hasn't changed by comparing hashes
			try
			{
				var verifyBytes = new byte[cachedMetadata.ByteLength];
				var verifySpan = mem.GetSpan(startEip, cachedMetadata.ByteLength);
				verifySpan.CopyTo(verifyBytes.AsSpan());
				var currentHash = JitCache.ComputeCodeHash(verifyBytes);
				
				if (currentHash != cachedMetadata.CodeHash)
				{
					_logger.LogWarning("[JitCpu] Code hash mismatch at EIP=0x{Eip:X8} - code may have been modified", startEip);
					// Continue with fresh compilation
					hasCachedMetadata = false;
				}
				else
				{
					_logger.LogInformation("[JitCpu] Compiling block at EIP=0x{Eip:X8} (using cached metadata, {InstructionCount} instructions)", 
						startEip, cachedMetadata.InstructionCount);
				}
			}
			catch
			{
				// If we can't read memory or verify hash, proceed with fresh compilation
				hasCachedMetadata = false;
			}
		}
		
		if (!hasCachedMetadata)
		{
			_logger.LogInformation("[JitCpu] Compiling block at EIP=0x{Eip:X8}", startEip);
		}
		
		var blockId = _blockCounter++;
		var typeName = $"Block_{blockId:X8}";
		var typeBuilder = _moduleBuilder.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Sealed);
		
		var methodBuilder = typeBuilder.DefineMethod(
			"Execute",
			MethodAttributes.Public | MethodAttributes.Static,
			typeof(Task<CpuStepResult>),
			new[] { typeof(JitCpu), typeof(VirtualMemory) });
		
		var il = methodBuilder.GetILGenerator();
		var instructions = AnalyzeBlock(startEip, mem);
		
		// Calculate block metadata (or use cached if available and verified)
		var blockLength = instructions.Sum(i => i.Length);
		var codeBytes = new byte[blockLength];
		try
		{
			var span = mem.GetSpan(startEip, blockLength);
			span.CopyTo(codeBytes.AsSpan());
		}
		catch
		{
			// If we can't read the memory, use zeros
			Array.Fill(codeBytes, (byte)0);
		}
		
		var codeHash = JitCache.ComputeCodeHash(codeBytes);
		var endsWithCall = instructions.Count > 0 && instructions[^1].Mnemonic == Mnemonic.Call;
		var endsWithReturn = instructions.Count > 0 && instructions[^1].Mnemonic == Mnemonic.Ret;
		
		// Update or save metadata to cache
		var metadata = new BlockMetadata
		{
			StartAddress = startEip,
			InstructionCount = instructions.Count,
			ByteLength = blockLength,
			CodeHash = codeHash,
			FirstCompiled = cachedMetadata?.FirstCompiled ?? DateTime.UtcNow,
			ExecutionCount = cachedMetadata?.ExecutionCount ?? 0,
			EndsWithCall = endsWithCall,
			EndsWithReturn = endsWithReturn
		};
		
		_jitCache.AddBlockMetadata(startEip, metadata);
		
		// For now, just return a default result (placeholder for actual JIT compilation)
		il.Emit(OpCodes.Ldc_I4_0); // isCall = false
		il.Emit(OpCodes.Ldc_I4_0); // callTarget = 0
		var constructor = typeof(CpuStepResult).GetConstructor(new[] { typeof(bool), typeof(uint) });
		if (constructor == null)
		{
			throw new InvalidOperationException("CpuStepResult does not have a constructor with signature (bool, uint).");
		}
		il.Emit(OpCodes.Newobj, constructor);
		var fromResultMethod = typeof(Task).GetMethod(nameof(Task.FromResult))?.MakeGenericMethod(typeof(CpuStepResult));
		if (fromResultMethod == null)
		{
			throw new InvalidOperationException("Task.FromResult method not found.");
		}
		il.Emit(OpCodes.Call, fromResultMethod);
		il.Emit(OpCodes.Ret);
		
		var type = typeBuilder.CreateType();
		if (type == null)
		{
			throw new InvalidOperationException($"Failed to create type '{typeName}' for compiled block at EIP=0x{startEip:X8}.");
		}
		var method = type.GetMethod("Execute");
		if (method == null)
		{
			throw new InvalidOperationException($"Failed to find 'Execute' method on type '{typeName}' for compiled block at EIP=0x{startEip:X8}.");
		}
		
		return new CompiledBlock(
			startEip,
			instructions.Count,
			(cpu, memory) => (Task<CpuStepResult>)method.Invoke(null, new object[] { cpu, memory })!
		);
	}

	private List<Instruction> AnalyzeBlock(uint startEip, VirtualMemory mem)
	{
		var instructions = new List<Instruction>();
		
		_reader.Reset(startEip);
		_decoder.IP = startEip;
		
		while (true)
		{
			var insn = _decoder.Decode();
			instructions.Add(insn);
			
			// Stop at control flow instructions
			if (insn.Mnemonic == Mnemonic.Call ||
			    insn.Mnemonic == Mnemonic.Ret ||
			    insn.Mnemonic == Mnemonic.Jmp ||
			    IsConditionalJump(insn.Mnemonic))
			{
				break;
			}
		}
		
		return instructions;
	}

	private static bool IsConditionalJump(Mnemonic mnemonic)
	{
		return mnemonic is Mnemonic.Je or Mnemonic.Jne or Mnemonic.Jl or Mnemonic.Jle or 
		       Mnemonic.Jg or Mnemonic.Jge or Mnemonic.Ja or Mnemonic.Jae or Mnemonic.Jb or 
		       Mnemonic.Jbe or Mnemonic.Jo or Mnemonic.Jno or Mnemonic.Js or Mnemonic.Jns;
	}

	// Flag bit positions (same as IcedCpu)
	private const int Cf = 0, Pf = 2, Af = 4, Zf = 6, Sf = 7, Of = 11;

	// Flag helper methods
	private bool GetFlag(int bit) => (_eflags & (1u << bit)) != 0;
	private void SetFlag(int bit) => _eflags |= (1u << bit);
	private void ClearFlag(int bit) => _eflags &= ~(1u << bit);
	private void SetFlagVal(int bit, bool val)
	{
		if (val)
			SetFlag(bit);
		else
			ClearFlag(bit);
	}

	// Conditional jump implementation
	private void ExecConditionalJump(Instruction insn)
	{
		bool condition = insn.Mnemonic switch
		{
			Mnemonic.Je => GetFlag(Zf),                                    // Jump if Equal (ZF=1)
			Mnemonic.Jne => !GetFlag(Zf),                                  // Jump if Not Equal (ZF=0)
			Mnemonic.Ja => !GetFlag(Cf) && !GetFlag(Zf),                  // Jump if Above (CF=0 and ZF=0)
			Mnemonic.Jae => !GetFlag(Cf),                                 // Jump if Above or Equal (CF=0)
			Mnemonic.Jb => GetFlag(Cf),                                   // Jump if Below (CF=1)
			Mnemonic.Jbe => GetFlag(Cf) || GetFlag(Zf),                   // Jump if Below or Equal (CF=1 or ZF=1)
			Mnemonic.Jg => !GetFlag(Zf) && GetFlag(Sf) == GetFlag(Of),    // Jump if Greater (ZF=0 and SF=OF)
			Mnemonic.Jge => GetFlag(Sf) == GetFlag(Of),                   // Jump if Greater or Equal (SF=OF)
			Mnemonic.Jl => GetFlag(Sf) != GetFlag(Of),                    // Jump if Less (SF!=OF)
			Mnemonic.Jle => GetFlag(Zf) || GetFlag(Sf) != GetFlag(Of),    // Jump if Less or Equal (ZF=1 or SF!=OF)
			Mnemonic.Jo => GetFlag(Of),                                   // Jump if Overflow (OF=1)
			Mnemonic.Jno => !GetFlag(Of),                                 // Jump if Not Overflow (OF=0)
			Mnemonic.Js => GetFlag(Sf),                                   // Jump if Sign (SF=1)
			Mnemonic.Jns => !GetFlag(Sf),                                 // Jump if Not Sign (SF=0)
			Mnemonic.Jp => GetFlag(Pf),                                   // Jump if Parity (PF=1)
			Mnemonic.Jnp => !GetFlag(Pf),                                 // Jump if Not Parity (PF=0)
			Mnemonic.Jcxz => (_ecx & 0xFFFF) == 0,                         // Jump if CX is Zero
			Mnemonic.Jecxz => _ecx == 0,                                   // Jump if ECX is Zero
			_ => false
		};

		if (condition && insn.Op0Kind == OpKind.NearBranch32)
		{
			_eip = (uint)insn.NearBranchTarget;
		}
		else if (condition && insn.Op0Kind == OpKind.NearBranch16)
		{
			_eip = (uint)insn.NearBranchTarget;
		}
	}

	// Bit manipulation implementation
	private void ExecBitManipulation(Instruction insn)
	{
		switch (insn.Mnemonic)
		{
			case Mnemonic.Bsf: // Bit Scan Forward
			case Mnemonic.Bsr: // Bit Scan Reverse
			{
				uint src = GetOperandValue(insn, 1);
				if (src == 0)
				{
					SetFlag(Zf);
				}
				else
				{
					ClearFlag(Zf);
					int bitPos = 0;
					if (insn.Mnemonic == Mnemonic.Bsf)
					{
						// Find first set bit from LSB
						for (int i = 0; i < 32; i++)
						{
							if ((src & (1u << i)) != 0)
							{
								bitPos = i;
								break;
							}
						}
					}
					else // BSR
					{
						// Find first set bit from MSB
						for (int i = 31; i >= 0; i--)
						{
							if ((src & (1u << i)) != 0)
							{
								bitPos = i;
								break;
							}
						}
					}
					SetOperandValue(insn, 0, (uint)bitPos);
				}
				break;
			}
			case Mnemonic.Btc: // Bit Test and Complement
			case Mnemonic.Btr: // Bit Test and Reset
			case Mnemonic.Bts: // Bit Test and Set
			{
				// For register-to-register, we need special handling
				if (insn.Op0Kind == OpKind.Register && insn.Op1Kind == OpKind.Register)
				{
					uint baseVal = GetRegisterValue(insn, 0);
					uint bitIndex = GetRegisterValue(insn, 1) & 31;
					uint mask = 1u << (int)bitIndex;
					
					// Set CF to the value of the tested bit
					SetFlagVal(Cf, (baseVal & mask) != 0);
					
					// Modify the bit based on instruction
					if (insn.Mnemonic == Mnemonic.Btc)
						baseVal ^= mask; // Complement
					else if (insn.Mnemonic == Mnemonic.Btr)
						baseVal &= ~mask; // Reset
					else // BTS
						baseVal |= mask; // Set
					
					SetRegisterValue(insn, 0, baseVal);
				}
				else
				{
					uint baseVal = GetOperandValue(insn, 0);
					uint bitIndex = GetOperandValue(insn, 1) & 31;
					uint mask = 1u << (int)bitIndex;
					
					// Set CF to the value of the tested bit
					SetFlagVal(Cf, (baseVal & mask) != 0);
					
					// Modify the bit based on instruction
					if (insn.Mnemonic == Mnemonic.Btc)
						baseVal ^= mask; // Complement
					else if (insn.Mnemonic == Mnemonic.Btr)
						baseVal &= ~mask; // Reset
					else // BTS
						baseVal |= mask; // Set
					
					SetOperandValue(insn, 0, baseVal);
				}
				break;
			}
		}
	}

	// BCD/ASCII arithmetic implementation
	private void ExecBcdArithmetic(Instruction insn)
	{
		switch (insn.Mnemonic)
		{
			case Mnemonic.Aaa: // ASCII Adjust After Addition
			{
				byte al = (byte)(_eax & 0xFF);
				if ((al & 0x0F) > 9 || GetFlag(Af))
				{
					al = (byte)(al + 6);
					_eax = (_eax & 0xFFFFFF00) | al;
					_eax = (uint)((_eax & 0xFFFF00FF) | (((_eax + 0x100) & 0xFF00)));
					SetFlag(Af);
					SetFlag(Cf);
				}
				else
				{
					ClearFlag(Af);
					ClearFlag(Cf);
				}
				_eax = (uint)((_eax & 0xFFFFFF0F) | (al & 0x0F));
				break;
			}
			case Mnemonic.Aas: // ASCII Adjust After Subtraction
			{
				byte al = (byte)(_eax & 0xFF);
				if ((al & 0x0F) > 9 || GetFlag(Af))
				{
					al = (byte)(al - 6);
					_eax = (_eax & 0xFFFFFF00) | al;
					_eax = (uint)((_eax & 0xFFFF00FF) | (((_eax - 0x100) & 0xFF00)));
					SetFlag(Af);
					SetFlag(Cf);
				}
				else
				{
					ClearFlag(Af);
					ClearFlag(Cf);
				}
				_eax = (uint)((_eax & 0xFFFFFF0F) | (al & 0x0F));
				break;
			}
			case Mnemonic.Cbw: // Convert Byte to Word
			{
				short ax = (sbyte)(_eax & 0xFF);
				_eax = (_eax & 0xFFFF0000) | (ushort)ax;
				break;
			}
			case Mnemonic.Cwde: // Convert Word to Doubleword Extended
			{
				int eax = (short)(_eax & 0xFFFF);
				_eax = (uint)eax;
				break;
			}
		}
	}

	// Double shift implementation
	private void ExecDoubleShift(Instruction insn)
	{
		uint dest = GetOperandValue(insn, 0);
		uint src = GetOperandValue(insn, 1);
		byte count;
		
		if (insn.Op2Kind == OpKind.Immediate8)
			count = (byte)(insn.Immediate8 & 0x1F);
		else
			count = (byte)(_ecx & 0x1F);
		
		if (count == 0)
			return;
		
		if (insn.Mnemonic == Mnemonic.Shld) // Shift Left Double
		{
			// Shift dest left by count, filling with high bits of src
			ulong combined = ((ulong)dest << 32) | src;
			combined <<= count;
			dest = (uint)(combined >> 32);
			
			// Set flags
			SetFlagVal(Cf, (combined & 0x100000000UL) != 0);
			SetFlagVal(Sf, (dest & 0x80000000) != 0);
			SetFlagVal(Zf, dest == 0);
			// OF is set only if count == 1
			if (count == 1)
				SetFlagVal(Of, ((dest ^ (dest << 1)) & 0x80000000) != 0);
		}
		else // SHRD - Shift Right Double
		{
			// Shift dest right by count, filling with low bits of src
			ulong combined = ((ulong)src << 32) | dest;
			combined >>= count;
			dest = (uint)combined;
			
			// Set flags
			SetFlagVal(Cf, ((combined >> (count - 1)) & 1) != 0);
			SetFlagVal(Sf, (dest & 0x80000000) != 0);
			SetFlagVal(Zf, dest == 0);
			// OF is set only if count == 1
			if (count == 1)
				SetFlagVal(Of, ((dest ^ (dest >> 1)) & 0x80000000) != 0);
		}
		
		SetOperandValue(insn, 0, dest);
	}

	// Helper methods for operand access
	private uint GetOperandValue(Instruction insn, int operandIndex)
	{
		var opKind = operandIndex switch
		{
			0 => insn.Op0Kind,
			1 => insn.Op1Kind,
			2 => insn.Op2Kind,
			_ => OpKind.Register
		};

		return opKind switch
		{
			OpKind.Register => GetRegisterValue(insn, operandIndex),
			OpKind.Immediate8 => operandIndex == 0 ? insn.Immediate8 :
			                     operandIndex == 1 ? insn.Immediate8_2nd : insn.Immediate8,
			OpKind.Immediate16 => insn.Immediate16,
			OpKind.Immediate32 => insn.Immediate32,
			OpKind.Memory => _mem.Read32(CalcMemAddress(insn, operandIndex)),
			_ => 0
		};
	}

	private void SetOperandValue(Instruction insn, int operandIndex, uint value)
	{
		var opKind = operandIndex switch
		{
			0 => insn.Op0Kind,
			1 => insn.Op1Kind,
			2 => insn.Op2Kind,
			_ => OpKind.Register
		};

		if (opKind == OpKind.Register)
		{
			SetRegisterValue(insn, operandIndex, value);
		}
		else if (opKind == OpKind.Memory)
		{
			_mem.Write32(CalcMemAddress(insn, operandIndex), value);
		}
	}

	private uint GetRegisterValue(Instruction insn, int operandIndex)
	{
		var reg = operandIndex switch
		{
			0 => insn.Op0Register,
			1 => insn.Op1Register,
			2 => insn.Op2Register,
			_ => Register.None
		};

		return reg switch
		{
			Register.EAX => _eax,
			Register.EBX => _ebx,
			Register.ECX => _ecx,
			Register.EDX => _edx,
			Register.ESI => _esi,
			Register.EDI => _edi,
			Register.EBP => _ebp,
			Register.ESP => _esp,
			Register.AX => _eax & 0xFFFF,
			Register.BX => _ebx & 0xFFFF,
			Register.CX => _ecx & 0xFFFF,
			Register.DX => _edx & 0xFFFF,
			Register.AL => _eax & 0xFF,
			Register.BL => _ebx & 0xFF,
			Register.CL => _ecx & 0xFF,
			Register.DL => _edx & 0xFF,
			Register.AH => (_eax >> 8) & 0xFF,
			Register.BH => (_ebx >> 8) & 0xFF,
			Register.CH => (_ecx >> 8) & 0xFF,
			Register.DH => (_edx >> 8) & 0xFF,
			_ => 0
		};
	}

	private void SetRegisterValue(Instruction insn, int operandIndex, uint value)
	{
		var reg = operandIndex switch
		{
			0 => insn.Op0Register,
			1 => insn.Op1Register,
			2 => insn.Op2Register,
			_ => Register.None
		};

		switch (reg)
		{
			case Register.EAX: _eax = value; break;
			case Register.EBX: _ebx = value; break;
			case Register.ECX: _ecx = value; break;
			case Register.EDX: _edx = value; break;
			case Register.ESI: _esi = value; break;
			case Register.EDI: _edi = value; break;
			case Register.EBP: _ebp = value; break;
			case Register.ESP: _esp = value; break;
			case Register.AX: _eax = (_eax & 0xFFFF0000) | (value & 0xFFFF); break;
			case Register.BX: _ebx = (_ebx & 0xFFFF0000) | (value & 0xFFFF); break;
			case Register.CX: _ecx = (_ecx & 0xFFFF0000) | (value & 0xFFFF); break;
			case Register.DX: _edx = (_edx & 0xFFFF0000) | (value & 0xFFFF); break;
			case Register.AL: _eax = (_eax & 0xFFFFFF00) | (value & 0xFF); break;
			case Register.BL: _ebx = (_ebx & 0xFFFFFF00) | (value & 0xFF); break;
			case Register.CL: _ecx = (_ecx & 0xFFFFFF00) | (value & 0xFF); break;
			case Register.DL: _edx = (_edx & 0xFFFFFF00) | (value & 0xFF); break;
			case Register.AH: _eax = (_eax & 0xFFFF00FF) | ((value & 0xFF) << 8); break;
			case Register.BH: _ebx = (_ebx & 0xFFFF00FF) | ((value & 0xFF) << 8); break;
			case Register.CH: _ecx = (_ecx & 0xFFFF00FF) | ((value & 0xFF) << 8); break;
			case Register.DH: _edx = (_edx & 0xFFFF00FF) | ((value & 0xFF) << 8); break;
		}
	}

	private uint CalcMemAddress(Instruction insn, int operandIndex)
	{
		// Simplified memory address calculation
		// For now, just return a basic address - full SIB decoding would be more complex
		uint addr = 0;
		
		if (insn.MemoryDisplSize > 0)
		{
			addr = insn.MemoryDisplacement32;
		}
		
		var baseReg = insn.MemoryBase;
		if (baseReg != Register.None)
		{
			addr += GetRegisterByEnum(baseReg);
		}
		
		var indexReg = insn.MemoryIndex;
		if (indexReg != Register.None)
		{
			uint indexVal = GetRegisterByEnum(indexReg);
			addr += indexVal * (uint)insn.MemoryIndexScale;
		}
		
		return addr;
	}

	private uint GetRegisterByEnum(Register reg)
	{
		return reg switch
		{
			Register.EAX => _eax,
			Register.EBX => _ebx,
			Register.ECX => _ecx,
			Register.EDX => _edx,
			Register.ESI => _esi,
			Register.EDI => _edi,
			Register.EBP => _ebp,
			Register.ESP => _esp,
			_ => 0
		};
	}


	private sealed class SimpleMemoryCodeReader : CodeReader
	{
		private readonly JitCpu _cpu;
		private uint _ptr;
		
		public SimpleMemoryCodeReader(JitCpu cpu)
		{
			_cpu = cpu;
		}
		
		public void Reset(uint ip) => _ptr = ip;
		public override int ReadByte() => _cpu._mem.Read8(_ptr++);
	}
}

internal class CompiledBlock
{
	private readonly Func<JitCpu, VirtualMemory, Task<CpuStepResult>> _execute;
	
	public CompiledBlock(uint startEip, int instructionCount, Func<JitCpu, VirtualMemory, Task<CpuStepResult>> execute)
	{
		StartEip = startEip;
		InstructionCount = instructionCount;
		_execute = execute;
	}
	
	public uint StartEip { get; }
	public int InstructionCount { get; }
	
	public Task<CpuStepResult> ExecuteAsync(JitCpu cpu, VirtualMemory memory)
	{
		return _execute(cpu, memory);
	}
}
