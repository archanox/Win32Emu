using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Memory;
using Win32Emu.Rtl;
using Iced.Intel;

namespace Win32Emu.Cpu.Jit;

/// <summary>
/// JIT-based x86 CPU emulator that compiles x86 code to .NET CIL for improved performance
/// and native async/await support.
/// Now uses RTL-based JIT pipeline for readable C# code generation and optimization.
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
	private ushort _fpuTagWord = 0xFFFF; // All tags set to 11b (empty)
	
	// MMX state - shares physical registers with FPU (MM0-MM7 alias to ST(0)-ST(7))
	// Each MMX register is 64 bits
	// NOTE: In real hardware, MMX and FPU share the same physical registers. This implementation
	// maintains separate arrays for simplicity and to avoid complex conversion between FPU double
	// format and MMX integer format. The tag word management ensures proper state transitions.
	private readonly ulong[] _mmx = new ulong[8];
	
	// JIT compilation infrastructure - now using RTL pipeline
	private readonly Dictionary<uint, RtlCompiledBlock> _compiledBlocks = new();
	
	// RTL-based JIT cache for persistent storage with readable C# output
	private readonly RtlJitCache _rtlJitCache;
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
		
		// Initialize RTL-based JIT cache
		_rtlJitCache = new RtlJitCache(null, logger);
		
		_logger.LogInformation("[JitCpu] Initialized RTL-based JIT CPU backend with readable C# code generation");
	}
	
	/// <summary>
	/// Creates a new JitCpu instance with a custom cache directory
	/// </summary>
	public JitCpu(VirtualMemory mem, ILogger? logger, string cacheDirectory) : this(mem, logger)
	{
		// Replace the default cache with one using the custom directory
		_rtlJitCache = new RtlJitCache(cacheDirectory, logger);
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
		
		// Execute the compiled block using dynamic invocation
		var result = await ExecuteRtlBlock(compiledBlock, this, mem);
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
			_logger.LogError("[JitCpu] Cannot load cache: executable path not set");
			return;
		}
		
		_rtlJitCache.LoadCachedAssemblies(_currentExecutablePath);
		var stats = _rtlJitCache.GetStatistics();
		_logger.LogInformation("[JitCpu] RTL cache loaded: {TotalBlocks} blocks from {SourceDir}",
			stats.TotalBlocks, stats.SourceDirectory);
		
		await Task.CompletedTask;
	}
	
	/// <summary>
	/// Saves JIT cache to disk for the current executable
	/// </summary>
	public async Task SaveCacheAsync()
	{
		if (string.IsNullOrEmpty(_currentExecutablePath))
		{
			_logger.LogError("[JitCpu] Cannot save cache: executable path not set");
			return;
		}
		
		_rtlJitCache.SaveCacheMetadata(_currentExecutablePath);
		_logger.LogInformation("[JitCpu] RTL cache saved with C# source files");
		
		await Task.CompletedTask;
	}
	
	/// <summary>
	/// Precompiles common code blocks to warm up the JIT cache.
	/// This compiles all blocks found in the cache for the current executable.
	/// </summary>
	public async Task<int> PrecompileFromCacheAsync(VirtualMemory mem)
	{
		_logger.LogInformation("[JitCpu] RTL-based precompilation - blocks are loaded on demand");
		// With RTL cache, blocks are already compiled and saved as assemblies
		// They will be loaded from disk when needed
		return await Task.FromResult(0);
	}
	
	/// <summary>
	/// Precompiles a specific address range to warm up the JIT cache
	/// </summary>
	public async Task<int> PrecompileRangeAsync(VirtualMemory mem, uint startAddress, uint endAddress)
	{
		_logger.LogInformation("[JitCpu] RTL-based JIT compiles blocks on demand - precompilation not needed");
		// With RTL cache, blocks are compiled and saved as they're encountered
		return await Task.FromResult(0);
	}
	
	/// <summary>
	/// Gets statistics about the JIT cache
	/// </summary>
	public RtlCacheStatistics GetCacheStatistics()
	{
		return _rtlJitCache.GetStatistics();
	}
	
	/// <summary>
	/// Purges all JIT cache files from disk and clears in-memory cache
	/// </summary>
	public void PurgeCache()
	{
		_rtlJitCache.PurgeCache();
		_compiledBlocks.Clear();
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
			FpuStatusWord = _fpuStatusWord,
			FpuTagWord = _fpuTagWord
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
			_fpuTagWord = state.FpuTagWord;
		}
	}

	private CpuStepResult InterpretSingleInstruction(VirtualMemory mem)
	{
		var oldEip = _eip; // Capture instruction address BEFORE any decoder operations
		_reader.Reset(_eip);
		_decoder.IP = _eip;
		var insn = _decoder.Decode();
		
		_eip = (uint)_decoder.IP;
		
		var isCall = insn.Mnemonic == Mnemonic.Call;
		var isSyscall = false;
		uint callTarget = 0;
		
		if (isCall)
		{
			if (insn.Op0Kind == OpKind.NearBranch32)
			{
				callTarget = (uint)insn.NearBranch32;
			}
			else if (insn.Op0Kind == OpKind.Register)
			{
				callTarget = GetRegisterValue(insn, 0);
			}
			else if (insn.Op0Kind == OpKind.Memory)
			{
				callTarget = mem.Read32(CalcMemAddress(insn, 0));
			}
		}
		
		switch (insn.Mnemonic)
		{
			// === Basic instructions (already implemented) ===
			case Mnemonic.Nop:
				break;
			case Mnemonic.Int3:
				break;
			case Mnemonic.Int:
				// Handle INT instruction with immediate
				if (insn.Immediate8 == 3)
				{
					// INT3 breakpoint - check if it's at a COM vtable address
					if (oldEip is >= 0x0D000000 and < 0x0E000000)
					{
						// This is a COM vtable method stub - signal this as a call
						isCall = true;
						callTarget = oldEip;
						_logger.LogInformation("[JitCpu] INT 3 hooking COM vtable stub at address 0x{OldEip:X8}", oldEip);
					}
					else
					{
						// Regular INT3 - for now, just print a message and continue
						_logger.LogWarning("[JitCpu] INT3 breakpoint at 0x{OldEip:X8}", oldEip);
					}
				}
				else if (insn.Immediate8 == 0x80)
				{
					// INT 0x80 - Syscall dispatcher
					isSyscall = true;
					_logger.LogDebug("[JitCpu] INT 0x80 syscall at 0x{OldEip:X8}", oldEip);
				}
				else
				{
					_logger.LogWarning("[JitCpu] Unhandled interrupt INT {InsnImmediate8:X2} at 0x{OldEip:X8}", insn.Immediate8, oldEip);
				}
				break;
			case Mnemonic.Call:
				_esp -= 4;
				mem.Write32(_esp, _eip);
				if (insn.Op0Kind == OpKind.NearBranch32 || insn.Op0Kind == OpKind.Register || insn.Op0Kind == OpKind.Memory)
				{
					_eip = callTarget;
				}
				else
				{
					throw new NotImplementedException($"[JitCpu] Unimplemented CALL type: {insn.Op0Kind} at EIP=0x{_eip - (uint)insn.Length:X8}");
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
			case Mnemonic.Jmp:
				// Unconditional jump - set EIP to target address
				if (insn.Op0Kind == OpKind.NearBranch32 || insn.Op0Kind == OpKind.NearBranch16)
				{
					_eip = (uint)insn.NearBranchTarget;
				}
				else if (insn.Op0Kind == OpKind.Register)
				{
					_eip = GetRegisterValue(insn, 0);
				}
				else if (insn.Op0Kind == OpKind.Memory)
				{
					_eip = mem.Read32(CalcMemAddress(insn, 0));
				}
				else
				{
					throw new NotImplementedException($"[JitCpu] Unimplemented JMP type: {insn.Op0Kind} at EIP=0x{_eip - (uint)insn.Length:X8}");
				}
				break;
			
			// === Core Arithmetic Instructions ===
			case Mnemonic.Add:
				ExecAdd(insn, mem);
				break;
			case Mnemonic.Sub:
				ExecSub(insn, mem);
				break;
			case Mnemonic.Adc:
				ExecAdc(insn, mem);
				break;
			case Mnemonic.Sbb:
				ExecSbb(insn, mem);
				break;
			case Mnemonic.Inc:
				ExecInc(insn, mem);
				break;
			case Mnemonic.Dec:
				ExecDec(insn, mem);
				break;
			case Mnemonic.Neg:
				ExecNeg(insn, mem);
				break;
			case Mnemonic.Cmp:
				ExecCmp(insn, mem);
				break;
			
			// === Logic Instructions ===
			case Mnemonic.And:
				ExecAnd(insn, mem);
				break;
			case Mnemonic.Or:
				ExecOr(insn, mem);
				break;
			case Mnemonic.Xor:
				ExecXor(insn, mem);
				break;
			case Mnemonic.Test:
				ExecTest(insn, mem);
				break;
			case Mnemonic.Not:
				ExecNot(insn, mem);
				break;
			
			// === Shift/Rotate Instructions ===
			case Mnemonic.Shl:
			case Mnemonic.Sal:
				ExecShl(insn, mem);
				break;
			case Mnemonic.Shr:
				ExecShr(insn, mem);
				break;
			case Mnemonic.Sar:
				ExecSar(insn, mem);
				break;
			case Mnemonic.Rol:
				ExecRol(insn, mem);
				break;
			case Mnemonic.Ror:
				ExecRor(insn, mem);
				break;
			case Mnemonic.Rcl:
				ExecRcl(insn, mem);
				break;
			case Mnemonic.Rcr:
				ExecRcr(insn, mem);
				break;
			
			// === Data Movement ===
			case Mnemonic.Mov:
				ExecMov(insn, mem);
				break;
			case Mnemonic.Movzx:
				ExecMovzx(insn, mem);
				break;
			case Mnemonic.Movsx:
				ExecMovsx(insn, mem);
				break;
			case Mnemonic.Xchg:
				ExecXchg(insn, mem);
				break;
			case Mnemonic.Push:
				ExecPush(insn, mem);
				break;
			case Mnemonic.Pop:
				ExecPop(insn, mem);
				break;
			case Mnemonic.Lea:
				ExecLea(insn, mem);
				break;
			
			// === Multiply/Divide ===
			case Mnemonic.Mul:
				ExecMul(insn, mem);
				break;
			case Mnemonic.Imul:
				ExecImul(insn, mem);
				break;
			case Mnemonic.Div:
				ExecDiv(insn, mem);
				break;
			case Mnemonic.Idiv:
				ExecIdiv(insn, mem);
				break;
			
			// === Additional Instructions ===
			case Mnemonic.Pushad:
				ExecPushad(mem);
				break;
			case Mnemonic.Popad:
				ExecPopad(mem);
				break;
			case Mnemonic.Cdq:
				ExecCdq();
				break;
			case Mnemonic.Bswap:
				ExecBswap(insn);
				break;
			case Mnemonic.Xlatb:
				ExecXlatb(mem);
				break;
			case Mnemonic.Leave:
				ExecLeave(mem);
				break;
			case Mnemonic.Cmpxchg:
				ExecCmpxchg(insn, mem);
				break;
			case Mnemonic.Xadd:
				ExecXadd(insn, mem);
				break;
			case Mnemonic.Cmpxchg8b:
				ExecCmpxchg8b(insn, mem);
				break;
			
			// Flag manipulation
			case Mnemonic.Clc:
				ClearFlag(Cf);
				break;
			case Mnemonic.Stc:
				SetFlag(Cf);
				break;
			case Mnemonic.Cmc:
				SetFlagVal(Cf, !GetFlag(Cf));
				break;
			case Mnemonic.Cld:
				ClearFlag(Df);
				break;
			case Mnemonic.Std:
				SetFlag(Df);
				break;
			case Mnemonic.Cli:
				ClearFlag(If);
				break;
			case Mnemonic.Sti:
				SetFlag(If);
				break;
			case Mnemonic.Pushf:
				_esp -= 2;
				_mem.Write16(_esp, (ushort)_eflags);
				break;
			case Mnemonic.Popf:
				_eflags = (_eflags & 0xFFFF0000) | _mem.Read16(_esp);
				_esp += 2;
				break;
			case Mnemonic.Pushfd:
				_esp -= 4;
				_mem.Write32(_esp, _eflags);
				break;
			case Mnemonic.Popfd:
				_eflags = _mem.Read32(_esp);
				_esp += 4;
				break;
			case Mnemonic.Lahf:
				ExecLahf();
				break;
			case Mnemonic.Sahf:
				ExecSahf();
				break;
			case Mnemonic.Iret:
			case Mnemonic.Iretd:
				ExecIret(mem);
				break;
			case Mnemonic.Seto:
			case Mnemonic.Setno:
			case Mnemonic.Setb:
			case Mnemonic.Setae:
			case Mnemonic.Sete:
			case Mnemonic.Setne:
			case Mnemonic.Setbe:
			case Mnemonic.Seta:
			case Mnemonic.Sets:
			case Mnemonic.Setns:
			case Mnemonic.Setp:
			case Mnemonic.Setnp:
			case Mnemonic.Setl:
			case Mnemonic.Setge:
			case Mnemonic.Setle:
			case Mnemonic.Setg:
				ExecSetcc(insn);
				break;
				
			// === Pentium CPU Instructions (Stubbed) ===
			// These are recognized but not yet fully implemented in JIT mode
			// They will be properly compiled when JIT compilation is complete
			
			// Integer arithmetic - BCD instructions
			case Mnemonic.Aaa:
			case Mnemonic.Aas:
			case Mnemonic.Cbw:
			case Mnemonic.Cwde:
			case Mnemonic.Daa:
			case Mnemonic.Das:
			case Mnemonic.Aam:
			case Mnemonic.Aad:
				ExecBcdArithmetic(insn);
				break;
			
			// Bit manipulation
			case Mnemonic.Bt:
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
			
			// Loop instructions
			case Mnemonic.Loop:
				ExecLoop(insn);
				break;
			case Mnemonic.Loope:
				ExecLoope(insn);
				break;
			case Mnemonic.Loopne:
				ExecLoopne(insn);
				break;
			
			// Conditional moves
			case Mnemonic.Cmove:
			case Mnemonic.Cmovne:
			case Mnemonic.Cmovae:
			case Mnemonic.Cmovb:
			case Mnemonic.Cmovbe:
			case Mnemonic.Cmova:
			case Mnemonic.Cmovle:
			case Mnemonic.Cmovl:
			case Mnemonic.Cmovge:
			case Mnemonic.Cmovg:
			case Mnemonic.Cmovno:
			case Mnemonic.Cmovnp:
			case Mnemonic.Cmovns:
			case Mnemonic.Cmovo:
			case Mnemonic.Cmovp:
			case Mnemonic.Cmovs:
				ExecConditionalMove(insn);
				break;
			
			// Control flow
			case Mnemonic.Retf:
				ExecRetf(insn);
				break;
			case Mnemonic.Into:
				ExecInto();
				break;
			
			// System instructions
			case Mnemonic.Hlt:
				ExecHlt();
				break;
			case Mnemonic.Bound:
				ExecBound(insn);
				break;
			case Mnemonic.Enter:
				ExecEnter(insn);
				break;
			case Mnemonic.Clts:
				ExecClts();
				break;
			
			// Segment operations
			case Mnemonic.Lds:
			case Mnemonic.Les:
			case Mnemonic.Lfs:
			case Mnemonic.Lgs:
			case Mnemonic.Lss:
				ExecLoadSegment(insn);
				break;
			case Mnemonic.Lar:
			case Mnemonic.Lsl:
			case Mnemonic.Verr:
			case Mnemonic.Verw:
				ExecSegmentCheck(insn);
				break;
			case Mnemonic.Lgdt:
			case Mnemonic.Sgdt:
			case Mnemonic.Lidt:
			case Mnemonic.Sidt:
			case Mnemonic.Lldt:
			case Mnemonic.Ltr:
			case Mnemonic.Str:
				ExecDescriptorTable(insn);
				break;
			
			// Shift double
			case Mnemonic.Shld:
			case Mnemonic.Shrd:
				ExecDoubleShift(insn);
				break;
			
			// String operations
			case Mnemonic.Movsb:
				ExecMovsb();
				break;
			case Mnemonic.Movsw:
				ExecMovsw();
				break;
			case Mnemonic.Movsd:
				ExecMovsd();
				break;
			case Mnemonic.Stosb:
				ExecStosb();
				break;
			case Mnemonic.Stosw:
				ExecStosw();
				break;
			case Mnemonic.Stosd:
				ExecStosd();
				break;
			case Mnemonic.Lodsb:
				ExecLodsb();
				break;
			case Mnemonic.Lodsw:
				ExecLodsw();
				break;
			case Mnemonic.Lodsd:
				ExecLodsd();
				break;
			case Mnemonic.Scasb:
				ExecScasb();
				break;
			case Mnemonic.Scasw:
				ExecScasw();
				break;
			case Mnemonic.Scasd:
				ExecScasd();
				break;
			case Mnemonic.Cmpsb:
				ExecCmpsb();
				break;
			case Mnemonic.Cmpsw:
				ExecCmpsw();
				break;
			case Mnemonic.Cmpsd:
				ExecCmpsd();
				break;
			case Mnemonic.Insb:
				ExecInsb();
				break;
			case Mnemonic.Insw:
				ExecInsw();
				break;
			case Mnemonic.Insd:
				ExecInsd();
				break;
			case Mnemonic.Outsb:
				ExecOutsb();
				break;
			case Mnemonic.Outsw:
				ExecOutsw();
				break;
			case Mnemonic.Outsd:
				ExecOutsd();
				break;
			
			// System/Privileged instructions
			case Mnemonic.Rdtsc:
				ExecRdtsc();
				break;
			case Mnemonic.Cpuid:
				ExecCpuid();
				break;
			case Mnemonic.Rdmsr:
				ExecRdmsr();
				break;
			case Mnemonic.Wrmsr:
				ExecWrmsr();
				break;
			case Mnemonic.Invd:
				ExecInvd();
				break;
			case Mnemonic.Wbinvd:
				ExecWbinvd();
				break;
			case Mnemonic.Invlpg:
				ExecInvlpg(insn);
				break;
			case Mnemonic.Rsm:
				ExecRsm();
				break;
			case Mnemonic.Sldt:
				ExecSldt(insn);
				break;
			case Mnemonic.Arpl:
				ExecArpl(insn);
				break;
			case Mnemonic.Wait:
				// WAIT/FWAIT - no-op for now
				break;
			
			// I/O operations
			case Mnemonic.In:
				ExecIn(insn);
				break;
			case Mnemonic.Out:
				ExecOut(insn);
				break;
			
			// FPU instructions (x87)
			case Mnemonic.Fld:
				ExecFld(insn, mem);
				break;
			case Mnemonic.Fst:
				ExecFst(insn, mem, false);
				break;
			case Mnemonic.Fstp:
				ExecFst(insn, mem, true);
				break;
			case Mnemonic.Fild:
				ExecFild(insn, mem);
				break;
			case Mnemonic.Fistp:
				ExecFistp(insn, mem);
				break;
			case Mnemonic.Fist:
				ExecFist(insn, mem);
				break;
			case Mnemonic.Fadd:
				ExecFadd(insn, mem);
				break;
			case Mnemonic.Faddp:
				ExecFaddp(insn);
				break;
			case Mnemonic.Fsub:
				ExecFsub(insn, mem);
				break;
			case Mnemonic.Fsubp:
				ExecFsubp(insn);
				break;
			case Mnemonic.Fsubr:
				ExecFsubr(insn, mem);
				break;
			case Mnemonic.Fsubrp:
				ExecFsubrp(insn);
				break;
			case Mnemonic.Fmul:
				ExecFmul(insn, mem);
				break;
			case Mnemonic.Fmulp:
				ExecFmulp(insn);
				break;
			case Mnemonic.Fdiv:
				ExecFdiv(insn, mem);
				break;
			case Mnemonic.Fdivp:
				ExecFdivp(insn);
				break;
			case Mnemonic.Fdivr:
				ExecFdivr(insn, mem);
				break;
			case Mnemonic.Fdivrp:
				ExecFdivrp(insn);
				break;
			case Mnemonic.Fiadd:
				ExecFiadd(insn, mem);
				break;
			case Mnemonic.Fimul:
				ExecFimul(insn, mem);
				break;
			case Mnemonic.Fisub:
				ExecFisub(insn, mem);
				break;
			case Mnemonic.Fisubr:
				ExecFisubr(insn, mem);
				break;
			case Mnemonic.Fidiv:
				ExecFidiv(insn, mem);
				break;
			case Mnemonic.Fidivr:
				ExecFidivr(insn, mem);
				break;
			case Mnemonic.Fsqrt:
				ExecFsqrt();
				break;
			case Mnemonic.Fxch:
				ExecFxch(insn);
				break;
			case Mnemonic.Fchs:
				ExecFchs();
				break;
			case Mnemonic.Fabs:
				ExecFabs();
				break;
			case Mnemonic.Fldz:
				ExecFldz();
				break;
			case Mnemonic.Fld1:
				ExecFld1();
				break;
			case Mnemonic.Fldpi:
				ExecFldpi();
				break;
			case Mnemonic.Fldl2e:
				ExecFldl2e();
				break;
			case Mnemonic.Fsin:
				ExecFsin();
				break;
			case Mnemonic.Fcos:
				ExecFcos();
				break;
			case Mnemonic.Fsincos:
				ExecFsincos();
				break;
			case Mnemonic.Fpatan:
				ExecFpatan();
				break;
			case Mnemonic.F2xm1:
				ExecF2xm1();
				break;
			case Mnemonic.Fscale:
				ExecFscale();
				break;
			case Mnemonic.Fcom:
				ExecFcom(insn, mem);
				break;
			case Mnemonic.Fcomp:
				ExecFcomp(insn, mem);
				break;
			case Mnemonic.Fcompp:
				ExecFcompp();
				break;
			case Mnemonic.Fucomi:
				ExecFucomi(insn);
				break;
			case Mnemonic.Fucomip:
				ExecFucomip(insn);
				break;
			case Mnemonic.Fcmovnbe:
				ExecFcmovnbe(insn);
				break;
			case Mnemonic.Fnstcw:
				ExecFnstcw(insn, mem);
				break;
			case Mnemonic.Fldcw:
				ExecFldcw(insn, mem);
				break;
			case Mnemonic.Fnstsw:
				ExecFnstsw(insn, mem);
				break;
			case Mnemonic.Fxam:
				ExecFxam();
				break;
			
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
			case Mnemonic.Ftst:
			case Mnemonic.Fucom:
			case Mnemonic.Fucomp:
			case Mnemonic.Fucompp:
			case Mnemonic.Fxtract:
			case Mnemonic.Fyl2x:
			case Mnemonic.Fyl2xp1:
				throw new NotImplementedException($"[JitCpu] Stubbed FPU instruction: {insn.Mnemonic}");
			
			case Mnemonic.Fninit:
				ExecFninit();
				break;
			case Mnemonic.Fnclex:
				ExecFnclex();
				break;
			case Mnemonic.Fstsw:
				ExecFstsw(insn);
				break;
			
			// MMX instructions
			case Mnemonic.Emms:
				ExecEmms();
				break;
			case Mnemonic.Movd:
				ExecMmxMovd(insn);
				break;
			case Mnemonic.Movq:
				ExecMmxMovq(insn);
				break;
			
			// MMX arithmetic and logical operations
			case Mnemonic.Paddb:
			case Mnemonic.Paddw:
			case Mnemonic.Paddd:
			case Mnemonic.Paddsb:
			case Mnemonic.Paddsw:
			case Mnemonic.Paddusb:
			case Mnemonic.Paddusw:
			case Mnemonic.Psubb:
			case Mnemonic.Psubw:
			case Mnemonic.Psubd:
			case Mnemonic.Psubsb:
			case Mnemonic.Psubsw:
			case Mnemonic.Psubusb:
			case Mnemonic.Psubusw:
			case Mnemonic.Pmullw:
			case Mnemonic.Pmulhw:
			case Mnemonic.Pmaddwd:
			case Mnemonic.Pand:
			case Mnemonic.Pandn:
			case Mnemonic.Por:
			case Mnemonic.Pxor:
			case Mnemonic.Pcmpeqb:
			case Mnemonic.Pcmpeqw:
			case Mnemonic.Pcmpeqd:
			case Mnemonic.Pcmpgtb:
			case Mnemonic.Pcmpgtw:
			case Mnemonic.Pcmpgtd:
				ExecMmxArithmetic(insn);
				break;
			
			// MMX shift operations
			case Mnemonic.Psllw:
			case Mnemonic.Pslld:
			case Mnemonic.Psllq:
			case Mnemonic.Psrlw:
			case Mnemonic.Psrld:
			case Mnemonic.Psrlq:
			case Mnemonic.Psraw:
			case Mnemonic.Psrad:
				ExecMmxShift(insn);
				break;
			
			// MMX packing/unpacking operations
			case Mnemonic.Packsswb:
			case Mnemonic.Packssdw:
			case Mnemonic.Packuswb:
			case Mnemonic.Punpckhbw:
			case Mnemonic.Punpckhwd:
			case Mnemonic.Punpckhdq:
			case Mnemonic.Punpcklbw:
			case Mnemonic.Punpcklwd:
			case Mnemonic.Punpckldq:
				ExecMmxPack(insn);
				break;
			
			default:
				throw new NotImplementedException($"[JitCpu] Unimplemented instruction: {insn.Mnemonic}");
				break;
		}
		
		return new CpuStepResult(isCall, callTarget, isSyscall);
	}

	private RtlCompiledBlock CompileBlock(uint startEip, VirtualMemory mem)
	{
		_logger.LogInformation("[JitCpu] Compiling block at EIP=0x{Eip:X8} using RTL pipeline", startEip);
		
		// Analyze the block to get x86 instructions
		var instructions = AnalyzeBlock(startEip, mem);
		
		// Use RTL pipeline to compile the block
		var rtlBlock = _rtlJitCache.CompileBlock(startEip, instructions);
		
		_logger.LogInformation("[JitCpu] Block at EIP=0x{Eip:X8} compiled successfully. C# source saved to {SourceDir}",
			startEip, _rtlJitCache.GetStatistics().SourceDirectory);
		
		return rtlBlock;
	}
	
	/// <summary>
	/// Execute a compiled RTL block by invoking the generated method
	/// </summary>
	private async Task<CpuStepResult> ExecuteRtlBlock(RtlCompiledBlock block, JitCpu cpu, VirtualMemory mem)
	{
		try
		{
			// Get the compiled type from the assembly
			if (block.Assembly == null)
			{
				_logger.LogError("[JitCpu] RTL block assembly is null at 0x{Address:X8}", block.StartAddress);
				return new CpuStepResult { IsCall = false, CallTarget = 0 };
			}
			
			var fullTypeName = $"Win32Emu.Jit.Generated.{block.ClassName}";
			var type = block.Assembly.GetType(fullTypeName);
			
			if (type == null)
			{
				_logger.LogError("[JitCpu] Could not find type {TypeName} in assembly", fullTypeName);
				return new CpuStepResult { IsCall = false, CallTarget = 0 };
			}
			
			var method = type.GetMethod(block.MethodName);
			if (method == null)
			{
				_logger.LogError("[JitCpu] Could not find method {MethodName} in type {TypeName}", 
					block.MethodName, fullTypeName);
				return new CpuStepResult { IsCall = false, CallTarget = 0 };
			}
			
			// Invoke the generated method
			var result = method.Invoke(null, new object[] { cpu, mem });
			
			if (result is Task<CpuStepResult> task)
			{
				return await task;
			}
			
			_logger.LogError("[JitCpu] Method returned unexpected type");
			return new CpuStepResult { IsCall = false, CallTarget = 0 };
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[JitCpu] Failed to execute RTL block at 0x{Address:X8}", block.StartAddress);
			return new CpuStepResult { IsCall = false, CallTarget = 0 };
		}
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
	private const int Cf = 0, Pf = 2, Af = 4, Zf = 6, Sf = 7, Df = 10, Of = 11;

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

	// Loop instructions
	private void ExecLoop(Instruction insn)
	{
		// LOOP - Decrement ECX and jump if ECX != 0
		_ecx--;
		if (_ecx != 0)
		{
			_eip = (uint)insn.NearBranchTarget;
		}
	}

	private void ExecLoope(Instruction insn)
	{
		// LOOPE/LOOPZ - Decrement ECX and jump if ECX != 0 and ZF = 1
		_ecx--;
		if (_ecx != 0 && GetFlag(Zf))
		{
			_eip = (uint)insn.NearBranchTarget;
		}
	}

	private void ExecLoopne(Instruction insn)
	{
		// LOOPNE/LOOPNZ - Decrement ECX and jump if ECX != 0 and ZF = 0
		_ecx--;
		if (_ecx != 0 && !GetFlag(Zf))
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
			case Mnemonic.Bt: // Bit Test
			{
				uint baseVal = GetOperandValue(insn, 0);
				uint bitIndex = GetOperandValue(insn, 1) & 31;
				uint mask = 1u << (int)bitIndex;
				
				// Set CF to the value of the tested bit
				SetFlagVal(Cf, (baseVal & mask) != 0);
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
					// Combine operations: set AL and increment AH in single assignment
					_eax = (uint)((_eax & 0xFFFF0000) | (((_eax + 0x106) & 0xFF00)) | (al & 0x0F));
					SetFlag(Af);
					SetFlag(Cf);
				}
				else
				{
					ClearFlag(Af);
					ClearFlag(Cf);
					_eax = (uint)((_eax & 0xFFFFFF0F) | (al & 0x0F));
				}
				break;
			}
			case Mnemonic.Aas: // ASCII Adjust After Subtraction
			{
				byte al = (byte)(_eax & 0xFF);
				if ((al & 0x0F) > 9 || GetFlag(Af))
				{
					al = (byte)(al - 6);
					// Combine operations: set AL and decrement AH in single assignment
					_eax = (uint)((_eax & 0xFFFF0000) | (((_eax - 0x100) & 0xFF00)) | (al & 0x0F));
					SetFlag(Af);
					SetFlag(Cf);
				}
				else
				{
					ClearFlag(Af);
					ClearFlag(Cf);
					_eax = (uint)((_eax & 0xFFFFFF0F) | (al & 0x0F));
				}
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
			// CF is set to the last bit shifted out (MSB of original dest)
			bool carryOut = ((dest >> (32 - count)) & 1) != 0;
			
			ulong combined = ((ulong)dest << 32) | src;
			combined <<= count;
			dest = (uint)(combined >> 32);
			
			// Set flags
			SetFlagVal(Cf, carryOut);
			SetFlagVal(Sf, (dest & 0x80000000) != 0);
			SetFlagVal(Zf, dest == 0);
			// OF is set only if count == 1
			if (count == 1)
				SetFlagVal(Of, ((dest ^ (dest << 1)) & 0x80000000) != 0);
		}
		else // SHRD - Shift Right Double
		{
			// Shift dest right by count, filling with low bits of src
			// CF is set to the last bit shifted out
			ulong combined = ((ulong)src << 32) | dest;
			bool carryOut = ((combined >> (count - 1)) & 1) != 0;
			combined >>= count;
			dest = (uint)combined;
			
			// Set flags
			SetFlagVal(Cf, carryOut);
			SetFlagVal(Sf, (dest & 0x80000000) != 0);
			SetFlagVal(Zf, dest == 0);
			// OF is set only if count == 1
			if (count == 1)
				SetFlagVal(Of, ((dest ^ (dest >> 1)) & 0x80000000) != 0);
		}
		
		SetOperandValue(insn, 0, dest);
	}

	// Conditional move implementation
	private void ExecConditionalMove(Instruction insn)
	{
		bool condition = insn.Mnemonic switch
		{
			Mnemonic.Cmove => GetFlag(Zf),                                 // Equal (ZF=1)
			Mnemonic.Cmovne => !GetFlag(Zf),                               // Not Equal (ZF=0)
			Mnemonic.Cmovae => !GetFlag(Cf),                               // Above or Equal (CF=0)
			Mnemonic.Cmovb => GetFlag(Cf),                                 // Below (CF=1)
			Mnemonic.Cmovbe => GetFlag(Cf) || GetFlag(Zf),                // Below or Equal (CF=1 or ZF=1)
			Mnemonic.Cmova => !GetFlag(Cf) && !GetFlag(Zf),               // Above (CF=0 and ZF=0)
			Mnemonic.Cmovle => GetFlag(Zf) || GetFlag(Sf) != GetFlag(Of), // Less or Equal (ZF=1 or SF!=OF)
			Mnemonic.Cmovl => GetFlag(Sf) != GetFlag(Of),                 // Less (SF!=OF)
			Mnemonic.Cmovge => GetFlag(Sf) == GetFlag(Of),                // Greater or Equal (SF=OF)
			Mnemonic.Cmovg => !GetFlag(Zf) && GetFlag(Sf) == GetFlag(Of), // Greater (ZF=0 and SF=OF)
			Mnemonic.Cmovno => !GetFlag(Of),                               // Not Overflow (OF=0)
			Mnemonic.Cmovnp => !GetFlag(Pf),                               // Not Parity (PF=0)
			Mnemonic.Cmovns => !GetFlag(Sf),                               // Not Sign (SF=0)
			Mnemonic.Cmovo => GetFlag(Of),                                 // Overflow (OF=1)
			Mnemonic.Cmovp => GetFlag(Pf),                                 // Parity (PF=1)
			Mnemonic.Cmovs => GetFlag(Sf),                                 // Sign (SF=1)
			_ => false
		};

		if (condition)
		{
			uint src = GetOperandValue(insn, 1);
			SetOperandValue(insn, 0, src);
		}
	}

	// Control flow instructions
	private void ExecRetf(Instruction insn)
	{
		// Far return - pop IP and CS from stack
		// In 32-bit protected mode with a flat memory model, the segment selector (CS) is ignored on far returns,
		// so we only update EIP and skip the CS value on the stack. This differs from real-mode far returns,
		// where both CS and IP are restored from the stack.
		_eip = _mem.Read32(_esp);
		_esp += 4;
		_esp += 4; // Skip CS value on stack
		
		// Handle stack cleanup parameter if present
		if (insn.OpCount > 0 && insn.Op0Kind == OpKind.Immediate16)
		{
			_esp += (uint)insn.Immediate16;
		}
	}

	private void ExecInto()
	{
		// INTO - Call interrupt 4 if overflow flag is set
		if (GetFlag(Of))
		{
			// In a full implementation, this would trigger interrupt 4
			// For now, we just log it
			_logger.LogDebug("[JitCpu] INTO triggered with OF=1");
		}
	}

	// System instructions
	private void ExecHlt()
	{
		// HLT - Halt processor
		// In emulation, this typically means we should stop or wait
		_logger.LogDebug("[JitCpu] HLT instruction executed");
		// In a real implementation, this might set a halted state
	}

	private void ExecBound(Instruction insn)
	{
		// BOUND - Check array bounds
		// This instruction checks if a signed index is within bounds
		// For now, we implement a simplified version that doesn't throw exceptions
		int index = (int)GetOperandValue(insn, 0);
		uint boundsAddr = CalcMemAddress(insn, 1);
		int lowerBound = (int)_mem.Read32(boundsAddr);
		int upperBound = (int)_mem.Read32(boundsAddr + 4);
		
		if (index < lowerBound || index > upperBound)
		{
			_logger.LogDebug("[JitCpu] BOUND check failed: index={0}, bounds=[{1}, {2}]", index, lowerBound, upperBound);
			// In a real implementation, this would generate interrupt 5
		}
	}

	private void ExecEnter(Instruction insn)
	{
		// ENTER - Make stack frame for procedure parameters
		ushort allocSize = insn.Immediate16;
		byte nestingLevel = insn.Immediate8_2nd;
		
		// Push EBP
		_esp -= 4;
		_mem.Write32(_esp, _ebp);
		
		uint frameTemp = _esp;
		
		// Create nested stack frames if nesting level > 0
		if (nestingLevel > 0)
		{
			for (int i = 1; i < nestingLevel; i++)
			{
				_ebp -= 4;
				uint temp = _mem.Read32(_ebp);
				_esp -= 4;
				_mem.Write32(_esp, temp);
			}
			_esp -= 4;
			_mem.Write32(_esp, frameTemp);
		}
		
		_ebp = frameTemp;
		_esp -= allocSize;
	}

	private void ExecClts()
	{
		// CLTS - Clear Task-Switched Flag in CR0
		// This is a privileged instruction used by operating systems
		// In flat memory emulation, this is typically a no-op
		_logger.LogDebug("[JitCpu] CLTS instruction executed (no-op in flat memory)");
	}

	// Segment operations
	private void ExecLoadSegment(Instruction insn)
	{
		// LDS, LES, LFS, LGS, LSS - Load far pointer
		// In 32-bit protected mode flat memory, segment registers are typically not used
		// We load the offset but ignore the segment selector
		uint addr = CalcMemAddress(insn, 1);
		uint offset = _mem.Read32(addr);
		// uint segment = _mem.Read16(addr + 4); // Ignored in flat memory
		
		SetOperandValue(insn, 0, offset);
		_logger.LogDebug("[JitCpu] Load segment instruction: {0}", insn.Mnemonic);
	}

	private void ExecSegmentCheck(Instruction insn)
	{
		// LAR, LSL, VERR, VERW - Segment descriptor checks
		// In flat memory model, these are simplified
		switch (insn.Mnemonic)
		{
			case Mnemonic.Lar: // Load Access Rights
			case Mnemonic.Lsl: // Load Segment Limit
				// Return success values for flat memory model
				SetOperandValue(insn, 0, 0xFFFFFFFF);
				SetFlag(Zf); // ZF=1 indicates success
				break;
			case Mnemonic.Verr: // Verify Read
			case Mnemonic.Verw: // Verify Write
				// In flat memory, all segments are readable/writable
				SetFlag(Zf); // ZF=1 indicates segment is accessible
				break;
		}
	}

	private void ExecDescriptorTable(Instruction insn)
	{
		// LGDT, SGDT, LIDT, SIDT, LLDT, LTR, STR - Descriptor table operations
		// These are privileged instructions for protected mode
		// In flat memory emulation, these are typically no-ops or simplified
		_logger.LogDebug("[JitCpu] Descriptor table instruction: {0} (simplified in flat memory)", insn.Mnemonic);
		
		// For store operations (SGDT, SIDT, STR), we could write dummy values
		// For load operations (LGDT, LIDT, LLDT, LTR), we accept but ignore
	}

	// String operations
	private void ExecLodsb()
	{
		// LODSB - Load byte from [ESI] into AL
		byte value = _mem.Read8(_esi);
		_eax = (_eax & 0xFFFFFF00) | value;
		
		// Update ESI based on direction flag
		if (GetFlag(Df))
			_esi -= 1; // Decrement for backward
		else
			_esi += 1; // Increment for forward
	}
	
	private void ExecLodsw()
	{
		// LODSW - Load word from [ESI] into AX
		ushort value = _mem.Read16(_esi);
		_eax = (_eax & 0xFFFF0000) | value;
		
		// Update ESI based on direction flag
		if (GetFlag(Df))
			_esi -= 2; // Decrement for backward
		else
			_esi += 2; // Increment for forward
	}
	
	private void ExecLodsd()
	{
		// LODSD - Load dword from [ESI] into EAX
		_eax = _mem.Read32(_esi);
		
		// Update ESI based on direction flag
		if (GetFlag(Df))
			_esi -= 4; // Decrement for backward
		else
			_esi += 4; // Increment for forward
	}
	
	private void ExecMovsb()
	{
		// MOVSB - Move byte from [ESI] to [EDI]
		byte value = _mem.Read8(_esi);
		_mem.Write8(_edi, value);
		
		// Update ESI and EDI based on direction flag
		if (GetFlag(Df))
		{
			_esi -= 1; // Decrement for backward
			_edi -= 1;
		}
		else
		{
			_esi += 1; // Increment for forward
			_edi += 1;
		}
	}
	
	private void ExecMovsw()
	{
		// MOVSW - Move word from [ESI] to [EDI]
		ushort value = _mem.Read16(_esi);
		_mem.Write16(_edi, value);
		
		// Update ESI and EDI based on direction flag
		if (GetFlag(Df))
		{
			_esi -= 2; // Decrement for backward
			_edi -= 2;
		}
		else
		{
			_esi += 2; // Increment for forward
			_edi += 2;
		}
	}
	
	private void ExecMovsd()
	{
		// MOVSD - Move dword from [ESI] to [EDI]
		uint value = _mem.Read32(_esi);
		_mem.Write32(_edi, value);
		
		// Update ESI and EDI based on direction flag
		if (GetFlag(Df))
		{
			_esi -= 4; // Decrement for backward
			_edi -= 4;
		}
		else
		{
			_esi += 4; // Increment for forward
			_edi += 4;
		}
	}
	
	private void ExecStosb()
	{
		// STOSB - Store AL to [EDI]
		_mem.Write8(_edi, (byte)_eax);
		
		// Update EDI based on direction flag
		if (GetFlag(Df))
			_edi -= 1; // Decrement for backward
		else
			_edi += 1; // Increment for forward
	}
	
	private void ExecStosw()
	{
		// STOSW - Store AX to [EDI]
		_mem.Write16(_edi, (ushort)_eax);
		
		// Update EDI based on direction flag
		if (GetFlag(Df))
			_edi -= 2; // Decrement for backward
		else
			_edi += 2; // Increment for forward
	}
	
	private void ExecStosd()
	{
		// STOSD - Store EAX to [EDI]
		_mem.Write32(_edi, _eax);
		
		// Update EDI based on direction flag
		if (GetFlag(Df))
			_edi -= 4; // Decrement for backward (4 bytes for doubleword)
		else
			_edi += 4; // Increment for forward
	}
	
	private void ExecScasb()
	{
		// SCASB - Scan byte: compare AL with [EDI]
		byte al = (byte)_eax;
		byte value = _mem.Read8(_edi);
		uint result = (uint)(al - value);
		
		// Set flags based on comparison
		SetFlagVal(Cf, al < value);
		SetFlagVal(Of, ((al ^ value) & (al ^ result) & 0x80) != 0);
		SetFlagVal(Af, ((al ^ value ^ result) & 0x10) != 0);
		SetFlagVal(Sf, (result & 0x80) != 0);
		SetFlagVal(Zf, (byte)result == 0);
		
		// Calculate parity
		byte lo = (byte)result;
		int bits = lo ^ (lo >> 4);
		bits &= 0xF;
		bool even = (((0x6996 >> bits) & 1) == 0);
		SetFlagVal(Pf, even);
		
		// Update EDI based on direction flag
		if (GetFlag(Df))
			_edi -= 1;
		else
			_edi += 1;
	}
	
	private void ExecScasw()
	{
		// SCASW - Scan word: compare AX with [EDI]
		ushort ax = (ushort)_eax;
		ushort value = _mem.Read16(_edi);
		uint result = (uint)(ax - value);
		
		// Set flags based on comparison
		SetFlagVal(Cf, ax < value);
		SetFlagVal(Of, ((ax ^ value) & (ax ^ result) & 0x8000) != 0);
		SetFlagVal(Af, ((ax ^ value ^ result) & 0x10) != 0);
		SetFlagVal(Sf, (result & 0x8000) != 0);
		SetFlagVal(Zf, (ushort)result == 0);
		
		// Calculate parity
		byte lo = (byte)result;
		int bits = lo ^ (lo >> 4);
		bits &= 0xF;
		bool even = (((0x6996 >> bits) & 1) == 0);
		SetFlagVal(Pf, even);
		
		// Update EDI based on direction flag
		if (GetFlag(Df))
			_edi -= 2;
		else
			_edi += 2;
	}
	
	private void ExecScasd()
	{
		// SCASD - Scan dword: compare EAX with [EDI]
		uint eax = _eax;
		uint value = _mem.Read32(_edi);
		uint result = eax - value;
		
		// Set flags based on comparison (same as SUB)
		SetFlagVal(Cf, eax < value);
		SetFlagVal(Of, ((eax ^ value) & (eax ^ result) & 0x80000000) != 0);
		SetFlagVal(Af, ((eax ^ value ^ result) & 0x10) != 0);
		SetFlagVal(Sf, (result & 0x80000000) != 0);
		SetFlagVal(Zf, result == 0);
		
		// Calculate parity
		byte lo = (byte)result;
		int bits = lo ^ (lo >> 4);
		bits &= 0xF;
		bool even = (((0x6996 >> bits) & 1) == 0);
		SetFlagVal(Pf, even);
		
		// Update EDI based on direction flag
		if (GetFlag(Df))
			_edi -= 4;
		else
			_edi += 4;
	}
	
	private void ExecCmpsb()
	{
		// CMPSB - Compare byte [ESI] with [EDI]
		byte a = _mem.Read8(_esi);
		byte b = _mem.Read8(_edi);
		uint result = (uint)(a - b);
		
		// Set flags based on comparison
		SetFlagVal(Cf, a < b);
		SetFlagVal(Of, ((a ^ b) & (a ^ result) & 0x80) != 0);
		SetFlagVal(Af, ((a ^ b ^ result) & 0x10) != 0);
		SetFlagVal(Sf, (result & 0x80) != 0);
		SetFlagVal(Zf, (byte)result == 0);
		
		// Calculate parity
		byte lo = (byte)result;
		int bits = lo ^ (lo >> 4);
		bits &= 0xF;
		bool even = (((0x6996 >> bits) & 1) == 0);
		SetFlagVal(Pf, even);
		
		// Update ESI and EDI based on direction flag
		if (GetFlag(Df))
		{
			_esi -= 1;
			_edi -= 1;
		}
		else
		{
			_esi += 1;
			_edi += 1;
		}
	}
	
	private void ExecCmpsw()
	{
		// CMPSW - Compare word [ESI] with [EDI]
		ushort a = _mem.Read16(_esi);
		ushort b = _mem.Read16(_edi);
		uint result = (uint)(a - b);
		
		// Set flags based on comparison
		SetFlagVal(Cf, a < b);
		SetFlagVal(Of, ((a ^ b) & (a ^ result) & 0x8000) != 0);
		SetFlagVal(Af, ((a ^ b ^ result) & 0x10) != 0);
		SetFlagVal(Sf, (result & 0x8000) != 0);
		SetFlagVal(Zf, (ushort)result == 0);
		
		// Calculate parity
		byte lo = (byte)result;
		int bits = lo ^ (lo >> 4);
		bits &= 0xF;
		bool even = (((0x6996 >> bits) & 1) == 0);
		SetFlagVal(Pf, even);
		
		// Update ESI and EDI based on direction flag
		if (GetFlag(Df))
		{
			_esi -= 2;
			_edi -= 2;
		}
		else
		{
			_esi += 2;
			_edi += 2;
		}
	}
	
	private void ExecCmpsd()
	{
		// CMPSD - Compare dword [ESI] with [EDI]
		uint a = _mem.Read32(_esi);
		uint b = _mem.Read32(_edi);
		uint result = a - b;
		
		// Set flags based on comparison (same as SUB)
		SetFlagVal(Cf, a < b);
		SetFlagVal(Of, ((a ^ b) & (a ^ result) & 0x80000000) != 0);
		SetFlagVal(Af, ((a ^ b ^ result) & 0x10) != 0);
		SetFlagVal(Sf, (result & 0x80000000) != 0);
		SetFlagVal(Zf, result == 0);
		
		// Calculate parity
		byte lo = (byte)result;
		int bits = lo ^ (lo >> 4);
		bits &= 0xF;
		bool even = (((0x6996 >> bits) & 1) == 0);
		SetFlagVal(Pf, even);
		
		// Update ESI and EDI based on direction flag
		if (GetFlag(Df))
		{
			_esi -= 4;
			_edi -= 4;
		}
		else
		{
			_esi += 4;
			_edi += 4;
		}
	}
	
	private void ExecInsb()
	{
		// INSB - Input byte from port DX to [EDI]
		// In emulation, I/O ports are not directly accessed, so we write 0
		_mem.Write8(_edi, 0);
		
		// Update EDI based on direction flag
		if (GetFlag(Df))
			_edi -= 1;
		else
			_edi += 1;
	}
	
	private void ExecInsw()
	{
		// INSW - Input word from port DX to [EDI]
		// In emulation, I/O ports are not directly accessed, so we write 0
		_mem.Write16(_edi, 0);
		
		// Update EDI based on direction flag
		if (GetFlag(Df))
			_edi -= 2;
		else
			_edi += 2;
	}
	
	private void ExecInsd()
	{
		// INSD - Input dword from port DX to [EDI]
		// In emulation, I/O ports are not directly accessed, so we write 0
		_mem.Write32(_edi, 0);
		
		// Update EDI based on direction flag
		if (GetFlag(Df))
			_edi -= 4;
		else
			_edi += 4;
	}
	
	private void ExecOutsb()
	{
		// OUTSB - Output byte from [ESI] to port DX
		// In emulation, I/O ports are not directly accessed
		// We just read the value (required for proper ESI advancement)
		_ = _mem.Read8(_esi);
		
		// Update ESI based on direction flag
		if (GetFlag(Df))
			_esi -= 1;
		else
			_esi += 1;
	}
	
	private void ExecOutsw()
	{
		// OUTSW - Output word from [ESI] to port DX
		// In emulation, I/O ports are not directly accessed
		// We just read the value (required for proper ESI advancement)
		_ = _mem.Read16(_esi);
		
		// Update ESI based on direction flag
		if (GetFlag(Df))
			_esi -= 2;
		else
			_esi += 2;
	}
	
	private void ExecOutsd()
	{
		// OUTSD - Output dword from [ESI] to port DX
		// In emulation, I/O ports are not directly accessed
		// We just read the value (required for proper ESI advancement)
		_ = _mem.Read32(_esi);
		
		// Update ESI based on direction flag
		if (GetFlag(Df))
			_esi -= 4;
		else
			_esi += 4;
	}

	// I/O operations
	private void ExecIn(Instruction insn)
	{
		// IN accumulator, port
		// In emulation, I/O ports are typically not directly accessed
		// We return 0 to prevent crashes, but this may not be functionally correct for all programs
		uint port;
		
		if (insn.Op1Kind == OpKind.Immediate8)
		{
			port = insn.Immediate8;
		}
		else
		{
			port = _edx & 0xFFFF;
		}
		
		_logger.LogDebug("[JitCpu] IN from port 0x{0:X} (returning 0)", port);
		
		// Set the accumulator to 0 using SetOperandValue which properly handles register sizes
		SetOperandValue(insn, 0, 0);
	}
	
	private void ExecOut(Instruction insn)
	{
		// OUT - Output to port
		// In emulation, I/O ports are typically not directly accessed
		uint port;
		uint value;
		
		if (insn.Op0Kind == OpKind.Immediate8)
		{
			port = insn.Immediate8;
		}
		else
		{
			port = _edx & 0xFFFF;
		}
		
		// Value is always from AL, AX, or EAX depending on size
		if (insn.Op1Kind == OpKind.Register)
		{
			var reg = insn.Op1Register;
			if (reg == Register.AL)
				value = _eax & 0xFF;
			else if (reg == Register.AX)
				value = _eax & 0xFFFF;
			else
				value = _eax;
		}
		else
		{
			value = _eax & 0xFF; // Default to AL
		}
		
		_logger.LogDebug("[JitCpu] OUT port 0x{0:X}, value 0x{1:X}", port, value);
		// In a full emulator, this would call an I/O handler
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

		uint result = opKind switch
		{
			OpKind.Register => GetRegisterValue(insn, operandIndex),
			// For Immediate8, always use Immediate8 (the Iced library sets the correct one)
			OpKind.Immediate8 => (uint)insn.Immediate8,
			OpKind.Immediate8to16 => (uint)(short)(sbyte)insn.Immediate8,  // Sign-extend 8->16->32
			OpKind.Immediate8to32 => (uint)(sbyte)insn.Immediate8,          // Sign-extend 8->32
			OpKind.Immediate16 => (uint)insn.Immediate16,
			OpKind.Immediate32 => insn.Immediate32,
			OpKind.Memory => _mem.Read32(CalcMemAddress(insn, operandIndex)),
			_ => 0u
		};
		
		return result;
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
		// Full SIB (Scale-Index-Base) memory address calculation
		// The Iced library parses the SIB byte and provides displacement, base, index, and scale
		uint addr = insn.MemoryDisplacement32;
		
		// Add base register if present
		var baseReg = insn.MemoryBase;
		if (baseReg != Register.None)
		{
			addr += GetRegisterByEnum(baseReg);
		}
		
		// Add (index * scale) if index register is present
		var indexReg = insn.MemoryIndex;
		if (indexReg != Register.None)
		{
			uint indexVal = GetRegisterByEnum(indexReg);
			addr += indexVal * (uint)insn.MemoryIndexScale;
		}
		
		// Check if address is within valid memory range
		// Convert to ulong to avoid overflow issues when comparing with memory size
		if (addr >= _mem.Size)
		{
			byte[]? instrBytes = null;
			try
			{
				instrBytes = _mem.GetSpan(_eip, 8);
			}
			catch
			{
			}

			Diagnostics.Diagnostics.LogCalcMemAddressFailure(addr, _mem.Size, _eip, _esp, _ebp, _eax, _ebx, _ecx, _edx, _esi, _edi, instrBytes);
			throw new IndexOutOfRangeException($"Calculated memory address out of range: 0x{addr:X} (EIP=0x{_eip:X8})");
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

	// === Core Arithmetic Implementations ===
	
	private void ExecAdd(Instruction insn, VirtualMemory mem)
	{
		uint a = GetOperandValue(insn, 0);
		uint b = GetOperandValue(insn, 1);
		uint r = a + b;
		SetOperandValue(insn, 0, r);
		
		// Get sign bit mask based on operand size
		int opSize = GetOpSizeBits(insn, 0);
		uint signBitMask = opSize switch
		{
			8 => 0x80,
			16 => 0x8000,
			_ => 0x80000000
		};
		SetFlagsAdd(a, b, r, signBitMask);
	}
	
	private void ExecSub(Instruction insn, VirtualMemory mem)
	{
		uint a = GetOperandValue(insn, 0);
		uint b = GetOperandValue(insn, 1);
		uint r = a - b;
		SetOperandValue(insn, 0, r);
		
		// Get sign bit mask based on operand size
		int opSize = GetOpSizeBits(insn, 0);
		uint signBitMask = opSize switch
		{
			8 => 0x80,
			16 => 0x8000,
			_ => 0x80000000
		};
		SetFlagsSub(a, b, r, signBitMask);
	}
	
	private void ExecAdc(Instruction insn, VirtualMemory mem)
	{
		uint a = GetOperandValue(insn, 0);
		uint b = GetOperandValue(insn, 1);
		uint cf = GetFlag(Cf) ? 1u : 0u;
		ulong sum = (ulong)a + b + cf;
		uint r = (uint)sum;
		SetOperandValue(insn, 0, r);
		SetFlagVal(Cf, (sum >> 32) != 0);
		SetFlagVal(Of, (~(a ^ b) & (a ^ r) & 0x80000000) != 0);
		SetFlagVal(Af, ((a ^ b ^ r) & 0x10) != 0);
		UpdateLogicResultFlags(r);
	}
	
	private void ExecSbb(Instruction insn, VirtualMemory mem)
	{
		uint a = GetOperandValue(insn, 0);
		uint b = GetOperandValue(insn, 1);
		uint cf = GetFlag(Cf) ? 1u : 0u;
		ulong diff = (ulong)a - (b + cf);
		uint r = (uint)diff;
		SetOperandValue(insn, 0, r);
		SetFlagVal(Cf, a < b + cf);
		SetFlagVal(Of, ((a ^ b) & (a ^ r) & 0x80000000) != 0);
		SetFlagVal(Af, ((a ^ b ^ r) & 0x10) != 0);
		UpdateLogicResultFlags(r);
	}
	
	private void ExecInc(Instruction insn, VirtualMemory mem)
	{
		uint a = GetOperandValue(insn, 0);
		uint r = a + 1;
		SetOperandValue(insn, 0, r);
		SetFlagVal(Of, (~(a ^ 1u) & (a ^ r) & 0x80000000) != 0);
		SetFlagVal(Af, ((a ^ 1u ^ r) & 0x10) != 0);
		UpdateLogicResultFlags(r);
	}
	
	private void ExecDec(Instruction insn, VirtualMemory mem)
	{
		uint a = GetOperandValue(insn, 0);
		uint r = a - 1;
		SetOperandValue(insn, 0, r);
		SetFlagVal(Of, (((a ^ 0xFFFFFFFFu) & (a ^ r) & 0x80000000) != 0));
		SetFlagVal(Af, ((a ^ 1u ^ r) & 0x10) != 0);
		UpdateLogicResultFlags(r);
	}
	
	private void ExecNeg(Instruction insn, VirtualMemory mem)
	{
		uint a = GetOperandValue(insn, 0);
		uint r = (uint)(-(int)a);
		SetOperandValue(insn, 0, r);
		SetFlagVal(Cf, a != 0);
		SetFlagVal(Of, a == 0x80000000);
		SetFlagVal(Af, (a & 0x0F) != 0);
		UpdateLogicResultFlags(r);
	}
	
	private void ExecCmp(Instruction insn, VirtualMemory mem)
	{
		uint a = GetOperandValue(insn, 0);
		uint b = GetOperandValue(insn, 1);
		uint r = a - b;
		
		// Get sign bit mask based on operand size
		int opSize = GetOpSizeBits(insn, 0);
		uint signBitMask = opSize switch
		{
			8 => 0x80,
			16 => 0x8000,
			_ => 0x80000000
		};
		SetFlagsSub(a, b, r, signBitMask);
	}
	
	// === Logic Implementations ===
	
	private void ExecAnd(Instruction insn, VirtualMemory mem)
	{
		uint a = GetOperandValue(insn, 0);
		uint b = GetOperandValue(insn, 1);
		uint r = a & b;
		SetOperandValue(insn, 0, r);
		ClearFlag(Cf);
		ClearFlag(Of);
		UpdateLogicResultFlags(r);
	}
	
	private void ExecOr(Instruction insn, VirtualMemory mem)
	{
		uint a = GetOperandValue(insn, 0);
		uint b = GetOperandValue(insn, 1);
		uint r = a | b;
		SetOperandValue(insn, 0, r);
		ClearFlag(Cf);
		ClearFlag(Of);
		UpdateLogicResultFlags(r);
	}
	
	private void ExecXor(Instruction insn, VirtualMemory mem)
	{
		uint a = GetOperandValue(insn, 0);
		uint b = GetOperandValue(insn, 1);
		uint r = a ^ b;
		SetOperandValue(insn, 0, r);
		ClearFlag(Cf);
		ClearFlag(Of);
		UpdateLogicResultFlags(r);
	}
	
	private void ExecTest(Instruction insn, VirtualMemory mem)
	{
		uint a = GetOperandValue(insn, 0);
		uint b = GetOperandValue(insn, 1);
		uint r = a & b;
		ClearFlag(Cf);
		ClearFlag(Of);
		UpdateLogicResultFlags(r);
	}
	
	private void ExecNot(Instruction insn, VirtualMemory mem)
	{
		uint a = GetOperandValue(insn, 0);
		uint r = ~a;
		SetOperandValue(insn, 0, r);
	}
	
	// === Shift/Rotate Implementations ===
	
	private void ExecShl(Instruction insn, VirtualMemory mem)
	{
		uint a = GetOperandValue(insn, 0);
		uint count = GetOperandValue(insn, 1) & 0x1F;
		if (count == 0) return;
		
		uint r = a << (int)count;
		SetOperandValue(insn, 0, r);
		
		SetFlagVal(Cf, ((a >> (32 - (int)count)) & 1) != 0);
		UpdateLogicResultFlags(r);
		
		if (count == 1)
			SetFlagVal(Of, ((r ^ a) & 0x80000000) != 0);
	}
	
	private void ExecShr(Instruction insn, VirtualMemory mem)
	{
		uint a = GetOperandValue(insn, 0);
		uint count = GetOperandValue(insn, 1) & 0x1F;
		if (count == 0) return;
		
		uint r = a >> (int)count;
		SetOperandValue(insn, 0, r);
		
		SetFlagVal(Cf, ((a >> ((int)count - 1)) & 1) != 0);
		UpdateLogicResultFlags(r);
		
		if (count == 1)
			SetFlagVal(Of, (a & 0x80000000) != 0);
	}
	
	private void ExecSar(Instruction insn, VirtualMemory mem)
	{
		uint a = GetOperandValue(insn, 0);
		uint count = GetOperandValue(insn, 1) & 0x1F;
		if (count == 0) return;
		
		int signedA = (int)a;
		int r = signedA >> (int)count;
		SetOperandValue(insn, 0, (uint)r);
		
		SetFlagVal(Cf, ((a >> ((int)count - 1)) & 1) != 0);
		UpdateLogicResultFlags((uint)r);
		
		if (count == 1)
			ClearFlag(Of);
	}
	
	private void ExecRol(Instruction insn, VirtualMemory mem)
	{
		uint a = GetOperandValue(insn, 0);
		uint count = GetOperandValue(insn, 1) & 0x1F;
		if (count == 0) return;
		
		uint r = (a << (int)count) | (a >> (32 - (int)count));
		SetOperandValue(insn, 0, r);
		
		SetFlagVal(Cf, (r & 1) != 0);
		
		if (count == 1)
			SetFlagVal(Of, ((r ^ (r >> 31)) & 1) != 0);
	}
	
	private void ExecRor(Instruction insn, VirtualMemory mem)
	{
		uint a = GetOperandValue(insn, 0);
		uint count = GetOperandValue(insn, 1) & 0x1F;
		if (count == 0) return;
		
		uint r = (a >> (int)count) | (a << (32 - (int)count));
		SetOperandValue(insn, 0, r);
		
		SetFlagVal(Cf, (r & 0x80000000) != 0);
		
		if (count == 1)
			SetFlagVal(Of, ((r ^ (r << 1)) & 0x80000000) != 0);
	}
	
	private void ExecRcl(Instruction insn, VirtualMemory mem)
	{
		uint a = GetOperandValue(insn, 0);
		uint count = GetOperandValue(insn, 1) & 0x1F;
		if (count == 0) return;
		
		for (int i = 0; i < count; i++)
		{
			bool oldCf = GetFlag(Cf);
			SetFlagVal(Cf, (a & 0x80000000) != 0);
			a = (a << 1) | (oldCf ? 1u : 0u);
		}
		SetOperandValue(insn, 0, a);
		
		if (count == 1)
			SetFlagVal(Of, ((a ^ (a >> 31)) & 0x80000000) != 0);
	}
	
	private void ExecRcr(Instruction insn, VirtualMemory mem)
	{
		uint a = GetOperandValue(insn, 0);
		uint count = GetOperandValue(insn, 1) & 0x1F;
		if (count == 0) return;
		
		for (int i = 0; i < count; i++)
		{
			bool oldCf = GetFlag(Cf);
			SetFlagVal(Cf, (a & 1) != 0);
			a = (a >> 1) | (oldCf ? 0x80000000u : 0u);
		}
		SetOperandValue(insn, 0, a);
		
		if (count == 1)
			SetFlagVal(Of, ((a ^ (a << 1)) & 0x80000000) != 0);
	}
	
	// === Data Movement Implementations ===
	
	private void ExecMov(Instruction insn, VirtualMemory mem)
	{
		uint value = GetOperandValue(insn, 1);
		SetOperandValue(insn, 0, value);
	}
	
	private void ExecMovzx(Instruction insn, VirtualMemory mem)
	{
		uint value = GetOperandValue(insn, 1);
		SetOperandValue(insn, 0, value);
	}
	
	private void ExecMovsx(Instruction insn, VirtualMemory mem)
	{
		uint value = GetOperandValue(insn, 1);
		
		var srcOpKind = insn.Op1Kind;
		if (srcOpKind == OpKind.Register)
		{
			var srcReg = insn.Op1Register;
			if (srcReg >= Register.AL && srcReg <= Register.BH)
			{
				value = (uint)(sbyte)value;
			}
			else if (srcReg >= Register.AX && srcReg <= Register.DI)
			{
				value = (uint)(short)value;
			}
		}
		else if (insn.MemorySize == MemorySize.UInt8 || insn.MemorySize == MemorySize.Int8)
		{
			value = (uint)(sbyte)value;
		}
		else if (insn.MemorySize == MemorySize.UInt16 || insn.MemorySize == MemorySize.Int16)
		{
			value = (uint)(short)value;
		}
		
		SetOperandValue(insn, 0, value);
	}
	
	private void ExecXchg(Instruction insn, VirtualMemory mem)
	{
		uint a = GetOperandValue(insn, 0);
		uint b = GetOperandValue(insn, 1);
		SetOperandValue(insn, 0, b);
		SetOperandValue(insn, 1, a);
	}
	
	private void ExecPush(Instruction insn, VirtualMemory mem)
	{
		uint value = GetOperandValue(insn, 0);
		_esp -= 4;
		mem.Write32(_esp, value);
	}
	
	private void ExecPop(Instruction insn, VirtualMemory mem)
	{
		uint value = mem.Read32(_esp);
		_esp += 4;
		SetOperandValue(insn, 0, value);
	}
	
	private void ExecLea(Instruction insn, VirtualMemory mem)
	{
		uint address = CalcMemAddress(insn, 1);
		SetOperandValue(insn, 0, address);
	}
	
	// === Additional instruction implementations ===
	
	private void ExecLeave(VirtualMemory mem)
	{
		// LEAVE - Set ESP to EBP, then pop EBP
		_esp = _ebp;
		_ebp = mem.Read32(_esp);
		_esp += 4;
	}
	
	private void ExecCmpxchg(Instruction insn, VirtualMemory mem)
	{
		// CMPXCHG - Compare and exchange
		// Compares AL/AX/EAX with destination. If equal, source is loaded into destination.
		// Otherwise, destination is loaded into AL/AX/EAX.
		uint dest = GetOperandValue(insn, 0);
		uint src = GetOperandValue(insn, 1);
		uint accum = _eax;
		
		// Compare accumulator with destination (sets flags like CMP)
		uint result = accum - dest;
		SetFlagsSub(accum, dest, result);
		
		if (accum == dest)
		{
			// Equal: write source to destination
			SetOperandValue(insn, 0, src);
		}
		else
		{
			// Not equal: write destination to accumulator
			_eax = dest;
		}
	}
	
	private void ExecXadd(Instruction insn, VirtualMemory mem)
	{
		// XADD - Exchange and add
		// Exchanges dest and src, then stores the sum in dest
		uint dest = GetOperandValue(insn, 0);
		uint src = GetOperandValue(insn, 1);
		
		// Exchange
		SetOperandValue(insn, 1, dest);
		
		// Add and store in dest
		uint sum = dest + src;
		SetOperandValue(insn, 0, sum);
		
		// Set flags like ADD
		SetFlagsAdd(dest, src, sum);
	}
	
	private void ExecCmpxchg8b(Instruction insn, VirtualMemory mem)
	{
		// CMPXCHG8B - Compare and exchange 8 bytes
		// Compares EDX:EAX with destination. If equal, ECX:EBX is loaded into destination.
		// Otherwise, destination is loaded into EDX:EAX.
		uint addr = CalcMemAddress(insn, 0);
		ulong dest = ((ulong)mem.Read32(addr + 4) << 32) | mem.Read32(addr);
		ulong accum = ((ulong)_edx << 32) | _eax;
		
		if (accum == dest)
		{
			// Equal: write ECX:EBX to destination
			ulong src = ((ulong)_ecx << 32) | _ebx;
			mem.Write32(addr, (uint)src);
			mem.Write32(addr + 4, (uint)(src >> 32));
			SetFlag(Zf);
		}
		else
		{
			// Not equal: write destination to EDX:EAX
			_eax = (uint)dest;
			_edx = (uint)(dest >> 32);
			ClearFlag(Zf);
		}
	}
	
	private void ExecLahf()
	{
		// LAHF - Load AH from flags
		byte ah = 0;
		if (GetFlag(Sf)) ah |= 0x80;
		if (GetFlag(Zf)) ah |= 0x40;
		if (GetFlag(Af)) ah |= 0x10;
		if (GetFlag(Pf)) ah |= 0x04;
		ah |= 0x02; // Bit 1 is always set
		if (GetFlag(Cf)) ah |= 0x01;
		
		_eax = (_eax & 0xFFFF00FF) | (uint)(ah << 8);
	}
	
	private void ExecSahf()
	{
		// SAHF - Store AH into flags
		byte ah = (byte)((_eax >> 8) & 0xFF);
		SetFlagVal(Sf, (ah & 0x80) != 0);
		SetFlagVal(Zf, (ah & 0x40) != 0);
		SetFlagVal(Af, (ah & 0x10) != 0);
		SetFlagVal(Pf, (ah & 0x04) != 0);
		SetFlagVal(Cf, (ah & 0x01) != 0);
	}
	
	private void ExecIret(VirtualMemory mem)
	{
		// IRET/IRETD - Interrupt return
		// Pops EIP, CS (ignored in flat memory), and EFLAGS from stack
		_eip = mem.Read32(_esp);
		_esp += 4;
		_esp += 4; // Skip CS (we don't use segmentation)
		_eflags = mem.Read32(_esp);
		_esp += 4;
	}
	
	// === BCD Arithmetic Extensions ===
	
	private void ExecDaa()
	{
		// DAA - Decimal Adjust AL After Addition
		byte al = (byte)(_eax & 0xFF);
		byte oldAl = al;
		bool oldCf = GetFlag(Cf);
		
		ClearFlag(Cf);
		
		// Check low nibble
		if (((al & 0x0F) > 9) || GetFlag(Af))
		{
			al += 6;
			SetFlagVal(Cf, oldCf || (al < oldAl));
			SetFlag(Af);
		}
		else
		{
			ClearFlag(Af);
		}
		
		// Check high nibble
		if ((oldAl > 0x99) || oldCf)
		{
			al += 0x60;
			SetFlag(Cf);
		}
		
		_eax = (_eax & 0xFFFFFF00) | al;
		UpdateLogicResultFlags(al);
	}
	
	private void ExecDas()
	{
		// DAS - Decimal Adjust AL After Subtraction
		byte al = (byte)(_eax & 0xFF);
		byte oldAl = al;
		bool oldCf = GetFlag(Cf);
		
		ClearFlag(Cf);
		
		// Check low nibble
		if (((al & 0x0F) > 9) || GetFlag(Af))
		{
			al -= 6;
			SetFlagVal(Cf, oldCf || (al > oldAl));
			SetFlag(Af);
		}
		else
		{
			ClearFlag(Af);
		}
		
		// Check high nibble
		if ((oldAl > 0x99) || oldCf)
		{
			al -= 0x60;
			SetFlag(Cf);
		}
		
		_eax = (_eax & 0xFFFFFF00) | al;
		UpdateLogicResultFlags(al);
	}
	
	private void ExecAam(Instruction insn)
	{
		// AAM - ASCII Adjust AX After Multiply
		byte base_ = insn.OpCount > 0 ? insn.Immediate8 : (byte)10;
		if (base_ == 0) base_ = 10;
		
		byte al = (byte)(_eax & 0xFF);
		byte ah = (byte)(al / base_);
		al = (byte)(al % base_);
		
		_eax = (_eax & 0xFFFF0000) | ((uint)ah << 8) | al;
		UpdateLogicResultFlags(al);
	}
	
	private void ExecAad(Instruction insn)
	{
		// AAD - ASCII Adjust AX Before Division
		byte base_ = insn.OpCount > 0 ? insn.Immediate8 : (byte)10;
		if (base_ == 0) base_ = 10;
		
		byte al = (byte)(_eax & 0xFF);
		byte ah = (byte)((_eax >> 8) & 0xFF);
		
		al = (byte)(ah * base_ + al);
		ah = 0;
		
		_eax = (_eax & 0xFFFF0000) | ((uint)ah << 8) | al;
		UpdateLogicResultFlags(al);
	}
	
	// === System/Privileged Instructions ===
	
	private void ExecRdtsc()
	{
		// RDTSC - Read Time-Stamp Counter
		// Returns a 64-bit value in EDX:EAX
		// Use a simple counter based on Environment.TickCount for deterministic behavior
		long tsc = Environment.TickCount64 * 1000000; // Convert to approximate CPU cycles
		_eax = (uint)tsc;
		_edx = (uint)(tsc >> 32);
	}
	
	private void ExecCpuid()
	{
		// CPUID - CPU Identification
		// Returns CPU information based on EAX input
		uint function = _eax;
		
		switch (function)
		{
			case 0: // Get vendor string
				_eax = 1; // Maximum supported function
				_ebx = 0x756E6547; // "Genu"
				_edx = 0x49656E69; // "ineI"
				_ecx = 0x6C65746E; // "ntel"
				break;
			case 1: // Get processor info and feature bits
				_eax = 0x00000F00; // Family 15, Model 0, Stepping 0
				_ebx = 0x00000800; // Brand index, CLFLUSH size, etc.
				_ecx = 0x00000001; // Feature flags (SSE3, etc.)
				_edx = 0x078BFBFF; // Feature flags (FPU, TSC, MSR, etc.)
				break;
			default:
				_eax = _ebx = _ecx = _edx = 0;
				break;
		}
	}
	
	private void ExecRdmsr()
	{
		// RDMSR - Read Model-Specific Register
		// In emulation, return 0 for all MSRs
		_logger.LogDebug("[JitCpu] RDMSR ECX=0x{0:X} (returning 0)", _ecx);
		_eax = 0;
		_edx = 0;
	}
	
	private void ExecWrmsr()
	{
		// WRMSR - Write Model-Specific Register
		// In emulation, this is a no-op
		_logger.LogDebug("[JitCpu] WRMSR ECX=0x{0:X}, EDX:EAX=0x{1:X}:{2:X}", _ecx, _edx, _eax);
	}
	
	private void ExecInvd()
	{
		// INVD - Invalidate Cache (no-op in emulation)
		_logger.LogDebug("[JitCpu] INVD executed (no-op)");
	}
	
	private void ExecWbinvd()
	{
		// WBINVD - Write Back and Invalidate Cache (no-op in emulation)
		_logger.LogDebug("[JitCpu] WBINVD executed (no-op)");
	}
	
	private void ExecInvlpg(Instruction insn)
	{
		// INVLPG - Invalidate TLB Entry (no-op in emulation)
		_logger.LogDebug("[JitCpu] INVLPG executed (no-op)");
	}
	
	private void ExecRsm()
	{
		// RSM - Resume from System Management Mode (no-op in emulation)
		_logger.LogDebug("[JitCpu] RSM executed (no-op)");
	}
	
	private void ExecSldt(Instruction insn)
	{
		// SLDT - Store Local Descriptor Table Register
		// In flat memory model, store 0
		SetOperandValue(insn, 0, 0);
	}
	
	private void ExecArpl(Instruction insn)
	{
		// ARPL - Adjust RPL Field of Segment Selector
		// In flat memory model, always report no adjustment
		ClearFlag(Zf);
	}
	
	// === FPU Helper Methods ===
	
	// FPU stack constants (same position as IcedCpu)
	private const int If = 9; // Interrupt flag position
	
	// Get ST(i) - ST(0) is the top of stack
	private double FpuGetSt(int i)
	{
		int idx = (_fpuTop + i) & 7;
		return _fpu[idx];
	}
	
	// Set ST(i)
	private void FpuSetSt(int i, double val)
	{
		int idx = (_fpuTop + i) & 7;
		_fpu[idx] = val;
	}
	
	// Push a value onto the FPU stack
	private void FpuPush(double val)
	{
		_fpuTop = (_fpuTop - 1) & 7;
		_fpu[_fpuTop] = val;
	}
	
	// Pop a value from the FPU stack
	private double FpuPop()
	{
		double val = _fpu[_fpuTop];
		_fpuTop = (_fpuTop + 1) & 7;
		return val;
	}
	
	// === FPU Instructions ===
	
	private void ExecFld(Instruction insn, VirtualMemory mem)
	{
		// FLD - Load floating point value
		double val;
		
		if (insn.Op0Kind == OpKind.Memory)
		{
			uint addr = CalcMemAddress(insn, 0);
			if (insn.MemorySize == MemorySize.Float32)
			{
				val = BitConverter.Int32BitsToSingle((int)mem.Read32(addr));
			}
			else if (insn.MemorySize == MemorySize.Float64)
			{
				ulong bits = mem.Read64(addr);
				val = BitConverter.Int64BitsToDouble((long)bits);
			}
			else
			{
				// Default to 64-bit
				ulong bits = mem.Read64(addr);
				val = BitConverter.Int64BitsToDouble((long)bits);
			}
		}
		else
		{
			// FLD ST(i) - duplicate ST(i) to ST(0)
			var reg = insn.Op0Register;
			int i = reg - Register.ST0;
			val = FpuGetSt(i);
		}
		
		FpuPush(val);
	}
	
	private void ExecFst(Instruction insn, VirtualMemory mem, bool pop)
	{
		// FST/FSTP - Store floating point value
		double st0 = FpuGetSt(0);
		
		if (insn.Op0Kind == OpKind.Memory)
		{
			uint addr = CalcMemAddress(insn, 0);
			if (insn.MemorySize == MemorySize.Float32)
			{
				float f = (float)st0;
				mem.Write32(addr, (uint)BitConverter.SingleToInt32Bits(f));
			}
			else
			{
				// Default to 64-bit
				mem.Write64(addr, (ulong)BitConverter.DoubleToInt64Bits(st0));
			}
		}
		else
		{
			// FST ST(i) - copy ST(0) to ST(i)
			var reg = insn.Op0Register;
			int i = reg - Register.ST0;
			FpuSetSt(i, st0);
		}
		
		if (pop)
		{
			FpuPop();
		}
	}
	
	private void ExecFild(Instruction insn, VirtualMemory mem)
	{
		// FILD - Load integer and convert to float
		uint addr = CalcMemAddress(insn, 0);
		double val;
		
		if (insn.MemorySize == MemorySize.Int16)
		{
			val = (short)mem.Read16(addr);
		}
		else if (insn.MemorySize == MemorySize.Int32)
		{
			val = (int)mem.Read32(addr);
		}
		else if (insn.MemorySize == MemorySize.Int64)
		{
			val = (long)mem.Read64(addr);
		}
		else
		{
			// Default to 32-bit
			val = (int)mem.Read32(addr);
		}
		
		FpuPush(val);
	}
	
	private void ExecFistp(Instruction insn, VirtualMemory mem)
	{
		// FISTP - Store integer and pop
		double val = FpuGetSt(0);
		uint addr = CalcMemAddress(insn, 0);
		
		// Round to nearest integer
		long rounded = (long)Math.Round(val);
		
		if (insn.MemorySize == MemorySize.Int16)
		{
			mem.Write16(addr, unchecked((ushort)(short)rounded));
		}
		else if (insn.MemorySize == MemorySize.Int32)
		{
			mem.Write32(addr, unchecked((uint)(int)rounded));
		}
		else if (insn.MemorySize == MemorySize.Int64)
		{
			mem.Write64(addr, unchecked((ulong)rounded));
		}
		else
		{
			// Default to 32-bit
			mem.Write32(addr, unchecked((uint)(int)rounded));
		}
		
		FpuPop();
	}
	
	private void ExecFist(Instruction insn, VirtualMemory mem)
	{
		// FIST - Store integer (no pop)
		double val = FpuGetSt(0);
		uint addr = CalcMemAddress(insn, 0);
		
		long rounded = (long)Math.Round(val);
		
		if (insn.MemorySize == MemorySize.Int16)
		{
			mem.Write16(addr, unchecked((ushort)(short)rounded));
		}
		else
		{
			mem.Write32(addr, unchecked((uint)(int)rounded));
		}
	}
	
	private void ExecFadd(Instruction insn, VirtualMemory mem)
	{
		// FADD - Add
		if (insn.OpCount == 0)
		{
			// FADD - Add ST(1) to ST(0)
			FpuSetSt(0, FpuGetSt(0) + FpuGetSt(1));
		}
		else if (insn.OpCount == 1)
		{
			if (insn.Op0Kind == OpKind.Memory)
			{
				// FADD m32/m64 - Add memory to ST(0)
				uint addr = CalcMemAddress(insn, 0);
				double val;
				if (insn.MemorySize == MemorySize.Float32)
				{
					val = BitConverter.Int32BitsToSingle((int)mem.Read32(addr));
				}
				else
				{
					ulong bits = mem.Read64(addr);
					val = BitConverter.Int64BitsToDouble((long)bits);
				}
				FpuSetSt(0, FpuGetSt(0) + val);
			}
			else
			{
				// FADD ST(i) - Add ST(i) to ST(0)
				var reg = insn.Op0Register;
				int i = reg - Register.ST0;
				FpuSetSt(0, FpuGetSt(0) + FpuGetSt(i));
			}
		}
		else
		{
			// FADD ST(i), ST(0) - Add ST(0) to ST(i)
			var reg = insn.Op0Register;
			int i = reg - Register.ST0;
			FpuSetSt(i, FpuGetSt(i) + FpuGetSt(0));
		}
	}
	
	private void ExecFaddp(Instruction insn)
	{
		// FADDP - Add and pop
		if (insn.OpCount == 0)
		{
			// FADDP - Add ST(0) to ST(1) and pop
			double st0 = FpuGetSt(0);
			double st1 = FpuGetSt(1);
			FpuPop();
			FpuSetSt(0, st0 + st1);
		}
		else
		{
			// FADDP ST(i), ST(0) - Add ST(0) to ST(i) and pop
			var reg = insn.Op0Register;
			int i = reg - Register.ST0;
			FpuSetSt(i, FpuGetSt(i) + FpuGetSt(0));
			FpuPop();
		}
	}
	
	private void ExecFsub(Instruction insn, VirtualMemory mem)
	{
		// FSUB - Subtract
		if (insn.OpCount == 0)
		{
			// FSUB - Subtract ST(1) from ST(0)
			FpuSetSt(0, FpuGetSt(0) - FpuGetSt(1));
		}
		else if (insn.OpCount == 1)
		{
			if (insn.Op0Kind == OpKind.Memory)
			{
				// FSUB m32/m64 - Subtract memory from ST(0)
				uint addr = CalcMemAddress(insn, 0);
				double val;
				if (insn.MemorySize == MemorySize.Float32)
				{
					val = BitConverter.Int32BitsToSingle((int)mem.Read32(addr));
				}
				else
				{
					ulong bits = mem.Read64(addr);
					val = BitConverter.Int64BitsToDouble((long)bits);
				}
				FpuSetSt(0, FpuGetSt(0) - val);
			}
			else
			{
				// FSUB ST(i) - Subtract ST(i) from ST(0)
				var reg = insn.Op0Register;
				int i = reg - Register.ST0;
				FpuSetSt(0, FpuGetSt(0) - FpuGetSt(i));
			}
		}
		else
		{
			// FSUB ST(i), ST(0) - Subtract ST(0) from ST(i)
			var reg = insn.Op0Register;
			int i = reg - Register.ST0;
			FpuSetSt(i, FpuGetSt(i) - FpuGetSt(0));
		}
	}
	
	private void ExecFsubp(Instruction insn)
	{
		// FSUBP - Subtract and pop
		if (insn.OpCount == 0)
		{
			// FSUBP - Subtract ST(0) from ST(1) and pop
			double st0 = FpuGetSt(0);
			double st1 = FpuGetSt(1);
			FpuPop();
			FpuSetSt(0, st1 - st0);
		}
		else
		{
			// FSUBP ST(i), ST(0) - Subtract ST(0) from ST(i) and pop
			var reg = insn.Op0Register;
			int i = reg - Register.ST0;
			FpuSetSt(i, FpuGetSt(i) - FpuGetSt(0));
			FpuPop();
		}
	}
	
	private void ExecFsubr(Instruction insn, VirtualMemory mem)
	{
		// FSUBR - Reverse subtract
		if (insn.OpCount == 0)
		{
			// FSUBR - Subtract ST(0) from ST(1), store in ST(0)
			FpuSetSt(0, FpuGetSt(1) - FpuGetSt(0));
		}
		else if (insn.OpCount == 1)
		{
			if (insn.Op0Kind == OpKind.Memory)
			{
				// FSUBR m32/m64 - Subtract ST(0) from memory
				uint addr = CalcMemAddress(insn, 0);
				double val;
				if (insn.MemorySize == MemorySize.Float32)
				{
					val = BitConverter.Int32BitsToSingle((int)mem.Read32(addr));
				}
				else
				{
					ulong bits = mem.Read64(addr);
					val = BitConverter.Int64BitsToDouble((long)bits);
				}
				FpuSetSt(0, val - FpuGetSt(0));
			}
			else
			{
				// FSUBR ST(i) - Subtract ST(0) from ST(i), store in ST(0)
				var reg = insn.Op0Register;
				int i = reg - Register.ST0;
				FpuSetSt(0, FpuGetSt(i) - FpuGetSt(0));
			}
		}
		else
		{
			// FSUBR ST(i), ST(0) - Subtract ST(0) from ST(i)
			var reg = insn.Op0Register;
			int i = reg - Register.ST0;
			FpuSetSt(i, FpuGetSt(i) - FpuGetSt(0));
		}
	}
	
	private void ExecFsubrp(Instruction insn)
	{
		// FSUBRP - Reverse subtract and pop
		if (insn.OpCount == 0)
		{
			// FSUBRP - Subtract ST(0) from ST(1) and pop
			double st0 = FpuGetSt(0);
			double st1 = FpuGetSt(1);
			FpuPop();
			FpuSetSt(0, st1 - st0);
		}
		else
		{
			// FSUBRP ST(i), ST(0) - Subtract ST(0) from ST(i) and pop
			var reg = insn.Op0Register;
			int i = reg - Register.ST0;
			FpuSetSt(i, FpuGetSt(i) - FpuGetSt(0));
			FpuPop();
		}
	}
	
	private void ExecFmul(Instruction insn, VirtualMemory mem)
	{
		// FMUL - Multiply
		if (insn.OpCount == 0)
		{
			// FMUL - Multiply ST(0) by ST(1)
			FpuSetSt(0, FpuGetSt(0) * FpuGetSt(1));
		}
		else if (insn.OpCount == 1)
		{
			if (insn.Op0Kind == OpKind.Memory)
			{
				// FMUL m32/m64 - Multiply ST(0) by memory
				uint addr = CalcMemAddress(insn, 0);
				double val;
				if (insn.MemorySize == MemorySize.Float32)
				{
					val = BitConverter.Int32BitsToSingle((int)mem.Read32(addr));
				}
				else
				{
					ulong bits = mem.Read64(addr);
					val = BitConverter.Int64BitsToDouble((long)bits);
				}
				FpuSetSt(0, FpuGetSt(0) * val);
			}
			else
			{
				// FMUL ST(i) - Multiply ST(0) by ST(i)
				var reg = insn.Op0Register;
				int i = reg - Register.ST0;
				FpuSetSt(0, FpuGetSt(0) * FpuGetSt(i));
			}
		}
		else
		{
			// FMUL ST(i), ST(0) - Multiply ST(i) by ST(0)
			var reg = insn.Op0Register;
			int i = reg - Register.ST0;
			FpuSetSt(i, FpuGetSt(i) * FpuGetSt(0));
		}
	}
	
	private void ExecFmulp(Instruction insn)
	{
		// FMULP - Multiply and pop
		if (insn.OpCount == 0)
		{
			// FMULP - Multiply ST(0) by ST(1) and pop
			double st0 = FpuGetSt(0);
			double st1 = FpuGetSt(1);
			FpuPop();
			FpuSetSt(0, st0 * st1);
		}
		else
		{
			// FMULP ST(i), ST(0) - Multiply ST(i) by ST(0) and pop
			var reg = insn.Op0Register;
			int i = reg - Register.ST0;
			FpuSetSt(i, FpuGetSt(i) * FpuGetSt(0));
			FpuPop();
		}
	}
	
	private void ExecFdiv(Instruction insn, VirtualMemory mem)
	{
		// FDIV - Divide
		if (insn.OpCount == 0)
		{
			// FDIV - Divide ST(0) by ST(1)
			FpuSetSt(0, FpuGetSt(0) / FpuGetSt(1));
		}
		else if (insn.OpCount == 1)
		{
			if (insn.Op0Kind == OpKind.Memory)
			{
				// FDIV m32/m64 - Divide ST(0) by memory
				uint addr = CalcMemAddress(insn, 0);
				double val;
				if (insn.MemorySize == MemorySize.Float32)
				{
					val = BitConverter.Int32BitsToSingle((int)mem.Read32(addr));
				}
				else
				{
					ulong bits = mem.Read64(addr);
					val = BitConverter.Int64BitsToDouble((long)bits);
				}
				FpuSetSt(0, FpuGetSt(0) / val);
			}
			else
			{
				// FDIV ST(i) - Divide ST(0) by ST(i)
				var reg = insn.Op0Register;
				int i = reg - Register.ST0;
				FpuSetSt(0, FpuGetSt(0) / FpuGetSt(i));
			}
		}
		else
		{
			// FDIV ST(i), ST(0) - Divide ST(i) by ST(0)
			var reg = insn.Op0Register;
			int i = reg - Register.ST0;
			FpuSetSt(i, FpuGetSt(i) / FpuGetSt(0));
		}
	}
	
	private void ExecFdivp(Instruction insn)
	{
		// FDIVP - Divide and pop
		if (insn.OpCount == 0)
		{
			// FDIVP - Divide ST(1) by ST(0) and pop
			double st0 = FpuGetSt(0);
			double st1 = FpuGetSt(1);
			FpuPop();
			FpuSetSt(0, st1 / st0);
		}
		else
		{
			// FDIVP ST(i), ST(0) - Divide ST(i) by ST(0) and pop
			var reg = insn.Op0Register;
			int i = reg - Register.ST0;
			double st0 = FpuGetSt(0);
			double sti = FpuGetSt(i);
			FpuPop();
			FpuSetSt(i - 1, sti / st0);
		}
	}
	
	private void ExecFdivr(Instruction insn, VirtualMemory mem)
	{
		// FDIVR - Reverse divide
		if (insn.OpCount == 0)
		{
			// FDIVR - Divide ST(1) by ST(0), store in ST(0)
			FpuSetSt(0, FpuGetSt(1) / FpuGetSt(0));
		}
		else if (insn.OpCount == 1)
		{
			if (insn.Op0Kind == OpKind.Memory)
			{
				// FDIVR m32/m64 - Divide memory by ST(0)
				uint addr = CalcMemAddress(insn, 0);
				double val;
				if (insn.MemorySize == MemorySize.Float32)
				{
					val = BitConverter.Int32BitsToSingle((int)mem.Read32(addr));
				}
				else
				{
					ulong bits = mem.Read64(addr);
					val = BitConverter.Int64BitsToDouble((long)bits);
				}
				FpuSetSt(0, val / FpuGetSt(0));
			}
			else
			{
				// FDIVR ST(i) - Divide ST(i) by ST(0), store in ST(0)
				var reg = insn.Op0Register;
				int i = reg - Register.ST0;
				FpuSetSt(0, FpuGetSt(i) / FpuGetSt(0));
			}
		}
		else
		{
			// FDIVR ST(i), ST(0) - Divide ST(0) by ST(i)
			var reg = insn.Op0Register;
			int i = reg - Register.ST0;
			FpuSetSt(i, FpuGetSt(0) / FpuGetSt(i));
		}
	}
	
	private void ExecFdivrp(Instruction insn)
	{
		// FDIVRP - Reverse divide and pop
		if (insn.OpCount == 0)
		{
			// FDIVRP - Divide ST(1) by ST(0) and pop
			double st0 = FpuGetSt(0);
			double st1 = FpuGetSt(1);
			FpuPop();
			FpuSetSt(0, st1 / st0);
		}
		else
		{
			// FDIVRP ST(i), ST(0) - Divide ST(0) by ST(i) and pop
			var reg = insn.Op0Register;
			int i = reg - Register.ST0;
			double st0 = FpuGetSt(0);
			double sti = FpuGetSt(i);
			FpuPop();
			FpuSetSt(i - 1, st0 / sti);
		}
	}
	
	private void ExecFiadd(Instruction insn, VirtualMemory mem)
	{
		// FIADD - Integer add
		uint addr = CalcMemAddress(insn, 0);
		int ival;
		if (insn.MemorySize == MemorySize.Int16)
		{
			ival = (short)mem.Read16(addr);
		}
		else
		{
			ival = (int)mem.Read32(addr);
		}
		FpuSetSt(0, FpuGetSt(0) + ival);
	}
	
	private void ExecFimul(Instruction insn, VirtualMemory mem)
	{
		// FIMUL - Integer multiply
		uint addr = CalcMemAddress(insn, 0);
		int ival;
		if (insn.MemorySize == MemorySize.Int16)
		{
			ival = (short)mem.Read16(addr);
		}
		else
		{
			ival = (int)mem.Read32(addr);
		}
		FpuSetSt(0, FpuGetSt(0) * ival);
	}
	
	private void ExecFisub(Instruction insn, VirtualMemory mem)
	{
		// FISUB - Integer subtract
		uint addr = CalcMemAddress(insn, 0);
		int ival;
		if (insn.MemorySize == MemorySize.Int16)
		{
			ival = (short)mem.Read16(addr);
		}
		else
		{
			ival = (int)mem.Read32(addr);
		}
		FpuSetSt(0, FpuGetSt(0) - ival);
	}
	
	private void ExecFisubr(Instruction insn, VirtualMemory mem)
	{
		// FISUBR - Integer reverse subtract
		uint addr = CalcMemAddress(insn, 0);
		int ival;
		if (insn.MemorySize == MemorySize.Int16)
		{
			ival = (short)mem.Read16(addr);
		}
		else
		{
			ival = (int)mem.Read32(addr);
		}
		FpuSetSt(0, ival - FpuGetSt(0));
	}
	
	private void ExecFidiv(Instruction insn, VirtualMemory mem)
	{
		// FIDIV - Integer divide
		uint addr = CalcMemAddress(insn, 0);
		int ival;
		if (insn.MemorySize == MemorySize.Int16)
		{
			ival = (short)mem.Read16(addr);
		}
		else
		{
			ival = (int)mem.Read32(addr);
		}
		FpuSetSt(0, FpuGetSt(0) / ival);
	}
	
	private void ExecFidivr(Instruction insn, VirtualMemory mem)
	{
		// FIDIVR - Integer reverse divide
		uint addr = CalcMemAddress(insn, 0);
		int ival;
		if (insn.MemorySize == MemorySize.Int16)
		{
			ival = (short)mem.Read16(addr);
		}
		else
		{
			ival = (int)mem.Read32(addr);
		}
		FpuSetSt(0, ival / FpuGetSt(0));
	}
	
	private void ExecFsqrt()
	{
		// FSQRT - Square root
		FpuSetSt(0, Math.Sqrt(FpuGetSt(0)));
	}
	
	private void ExecFxch(Instruction insn)
	{
		// FXCH - Exchange registers
		int i = insn.OpCount > 0 ? insn.Op0Register - Register.ST0 : 1;
		double st0 = FpuGetSt(0);
		double sti = FpuGetSt(i);
		FpuSetSt(0, sti);
		FpuSetSt(i, st0);
	}
	
	private void ExecFchs()
	{
		// FCHS - Change sign
		FpuSetSt(0, -FpuGetSt(0));
	}
	
	private void ExecFabs()
	{
		// FABS - Absolute value
		FpuSetSt(0, Math.Abs(FpuGetSt(0)));
	}
	
	private void ExecFldz()
	{
		// FLDZ - Load +0.0
		FpuPush(0.0);
	}
	
	private void ExecFld1()
	{
		// FLD1 - Load +1.0
		FpuPush(1.0);
	}
	
	private void ExecFldpi()
	{
		// FLDPI - Load π
		FpuPush(Math.PI);
	}
	
	private void ExecFldl2e()
	{
		// FLDL2E - Load log2(e)
		FpuPush(Math.Log2(Math.E));
	}
	
	private void ExecFsin()
	{
		// FSIN - Sine
		FpuSetSt(0, Math.Sin(FpuGetSt(0)));
	}
	
	private void ExecFcos()
	{
		// FCOS - Cosine
		FpuSetSt(0, Math.Cos(FpuGetSt(0)));
	}
	
	private void ExecFsincos()
	{
		// FSINCOS - Sine and cosine
		double angle = FpuGetSt(0);
		FpuSetSt(0, Math.Sin(angle));
		FpuPush(Math.Cos(angle));
	}
	
	private void ExecFpatan()
	{
		// FPATAN - Partial arctangent
		double x = FpuGetSt(1);
		double y = FpuGetSt(0);
		FpuPop();
		FpuSetSt(0, Math.Atan2(y, x));
	}
	
	private void ExecF2xm1()
	{
		// F2XM1 - 2^x - 1
		FpuSetSt(0, Math.Pow(2, FpuGetSt(0)) - 1);
	}
	
	private void ExecFscale()
	{
		// FSCALE - Scale
		FpuSetSt(0, FpuGetSt(0) * Math.Pow(2, Math.Truncate(FpuGetSt(1))));
	}
	
	private void ExecFcom(Instruction insn, VirtualMemory mem)
	{
		// FCOM - Compare (no pop)
		double st0 = FpuGetSt(0);
		double source;
		
		if (insn.OpCount == 0)
		{
			source = FpuGetSt(1);
		}
		else if (insn.Op0Kind == OpKind.Memory)
		{
			uint addr = CalcMemAddress(insn, 0);
			if (insn.MemorySize == MemorySize.Float32)
			{
				source = BitConverter.Int32BitsToSingle((int)mem.Read32(addr));
			}
			else
			{
				ulong bits = mem.Read64(addr);
				source = BitConverter.Int64BitsToDouble((long)bits);
			}
		}
		else
		{
			var reg = insn.Op0Register;
			int i = reg - Register.ST0;
			source = FpuGetSt(i);
		}
		
		// Set EFLAGS based on comparison
		if (double.IsNaN(st0) || double.IsNaN(source))
		{
			SetFlag(Zf); SetFlag(Pf); SetFlag(Cf);
		}
		else if (st0 > source)
		{
			ClearFlag(Zf); ClearFlag(Pf); ClearFlag(Cf);
		}
		else if (st0 < source)
		{
			ClearFlag(Zf); ClearFlag(Pf); SetFlag(Cf);
		}
		else
		{
			SetFlag(Zf); ClearFlag(Pf); ClearFlag(Cf);
		}
	}
	
	private void ExecFcomp(Instruction insn, VirtualMemory mem)
	{
		// FCOMP - Compare and pop
		ExecFcom(insn, mem);
		FpuPop();
	}
	
	private void ExecFcompp()
	{
		// FCOMPP - Compare and pop twice
		double st0 = FpuGetSt(0);
		double st1 = FpuGetSt(1);
		
		if (double.IsNaN(st0) || double.IsNaN(st1))
		{
			SetFlag(Zf); SetFlag(Pf); SetFlag(Cf);
		}
		else if (st0 > st1)
		{
			ClearFlag(Zf); ClearFlag(Pf); ClearFlag(Cf);
		}
		else if (st0 < st1)
		{
			ClearFlag(Zf); ClearFlag(Pf); SetFlag(Cf);
		}
		else
		{
			SetFlag(Zf); ClearFlag(Pf); ClearFlag(Cf);
		}
		
		FpuPop();
		FpuPop();
	}
	
	private void ExecFucomi(Instruction insn)
	{
		// FUCOMI - Unordered compare and set EFLAGS
		double st0 = FpuGetSt(0);
		int i = insn.OpCount > 0 ? insn.Op0Register - Register.ST0 : 1;
		double sti = FpuGetSt(i);
		
		if (double.IsNaN(st0) || double.IsNaN(sti))
		{
			SetFlag(Zf); SetFlag(Pf); SetFlag(Cf);
		}
		else if (st0 > sti)
		{
			ClearFlag(Zf); ClearFlag(Pf); ClearFlag(Cf);
		}
		else if (st0 < sti)
		{
			ClearFlag(Zf); ClearFlag(Pf); SetFlag(Cf);
		}
		else
		{
			SetFlag(Zf); ClearFlag(Pf); ClearFlag(Cf);
		}
	}
	
	private void ExecFucomip(Instruction insn)
	{
		// FUCOMIP - Unordered compare, set EFLAGS, and pop
		ExecFucomi(insn);
		FpuPop();
	}
	
	private void ExecFcmovnbe(Instruction insn)
	{
		// FCMOVNBE - Conditional move if not below or equal
		if (!GetFlag(Cf) && !GetFlag(Zf))
		{
			var reg = insn.Op1Register;
			int i = reg - Register.ST0;
			FpuSetSt(0, FpuGetSt(i));
		}
	}
	
	private void ExecFnstcw(Instruction insn, VirtualMemory mem)
	{
		// FNSTCW - Store FPU control word
		uint addr = CalcMemAddress(insn, 0);
		mem.Write16(addr, _fpuControlWord);
	}
	
	private void ExecFldcw(Instruction insn, VirtualMemory mem)
	{
		// FLDCW - Load FPU control word
		uint addr = CalcMemAddress(insn, 0);
		_fpuControlWord = mem.Read16(addr);
	}
	
	private void ExecFnstsw(Instruction insn, VirtualMemory mem)
	{
		// FNSTSW - Store FPU status word
		if (insn.OpCount == 0 || insn.Op0Kind == OpKind.Register)
		{
			// FNSTSW AX - Store to AX register
			_eax = (_eax & 0xFFFF0000) | _fpuStatusWord;
		}
		else
		{
			// FNSTSW mem16 - Store to memory
			uint addr = CalcMemAddress(insn, 0);
			mem.Write16(addr, _fpuStatusWord);
		}
	}
	
	private void ExecFxam()
	{
		// FXAM - Examine ST(0)
		double st0 = FpuGetSt(0);
		
		// Clear C0, C2, C3 bits
		_fpuStatusWord &= 0xB8FF;
		
		// Set condition codes based on ST(0) value
		if (double.IsNaN(st0))
		{
			// NaN: C0=0, C2=0, C3=0
		}
		else if (double.IsInfinity(st0))
		{
			// Infinity: C0=1, C2=1, C3=0
			_fpuStatusWord |= 0x0500;
		}
		else if (st0 == 0.0)
		{
			// Zero: C0=0, C2=0, C3=1
			_fpuStatusWord |= 0x4000;
		}
		else
		{
			// Normal: C0=1, C2=0, C3=0
			_fpuStatusWord |= 0x0100;
		}
		
		// Set sign bit (C1, bit 9)
		if ((BitConverter.DoubleToInt64Bits(st0) & (1L << 63)) != 0)
		{
			_fpuStatusWord |= 0x0200;
		}
	}
	
	// === Flag Helper Methods ===
	
	private void SetFlagsAdd(uint a, uint b, uint r)
	{
		SetFlagsAdd(a, b, r, 0x80000000);
	}

	private void SetFlagsAdd(uint a, uint b, uint r, uint signBitMask)
	{
		SetFlagVal(Cf, r < a);
		SetFlagVal(Of, (~(a ^ b) & (a ^ r) & signBitMask) != 0);
		SetFlagVal(Af, ((a ^ b ^ r) & 0x10) != 0);
		UpdateLogicResultFlags(r, signBitMask);
	}
	
	private void SetFlagsSub(uint a, uint b, uint r)
	{
		SetFlagsSub(a, b, r, 0x80000000);
	}

	private void SetFlagsSub(uint a, uint b, uint r, uint signBitMask)
	{
		SetFlagVal(Cf, a < b);
		SetFlagVal(Of, ((a ^ b) & (a ^ r) & signBitMask) != 0);
		SetFlagVal(Af, ((a ^ b ^ r) & 0x10) != 0);
		UpdateLogicResultFlags(r, signBitMask);
	}
	
	private void UpdateLogicResultFlags(uint r)
	{
		UpdateLogicResultFlags(r, 0x80000000);
	}

	private void UpdateLogicResultFlags(uint r, uint signBitMask)
	{
		SetFlagVal(Zf, r == 0);
		SetFlagVal(Sf, (r & signBitMask) != 0);
		
		byte lo = (byte)r;
		int bits = lo ^ (lo >> 4);
		bits &= 0xF;
		bool even = (((0x6996 >> bits) & 1) == 0);
		SetFlagVal(Pf, even);
	}
	
	private int GetOpSizeBits(Instruction insn, int opIndex)
	{
		if (insn.GetOpKind(opIndex) == OpKind.Memory)
		{
			return insn.MemorySize switch
			{
				MemorySize.UInt8 or MemorySize.Int8 => 8,
				MemorySize.UInt16 or MemorySize.Int16 => 16,
				_ => 32
			};
		}

		if (insn.GetOpKind(opIndex) == OpKind.Register)
		{
			var r = insn.GetOpRegister(opIndex);
			if (r is Register.AL or Register.CL or Register.DL or Register.BL or Register.AH or Register.CH or Register.DH or Register.BH)
			{
				return 8;
			}

			if (r is Register.AX or Register.CX or Register.DX or Register.BX or Register.SI or Register.DI or Register.SP or Register.BP)
			{
				return 16;
			}

			return 32;
		}

		// For immediates, default to 32
		return 32;
	}
	
	// === Multiply/Divide Implementations ===
	
	private void ExecMul(Instruction insn, VirtualMemory mem)
	{
		uint src = GetOperandValue(insn, 0);
		ulong result = (ulong)_eax * src;
		_eax = (uint)result;
		_edx = (uint)(result >> 32);
		
		SetFlagVal(Cf, _edx != 0);
		SetFlagVal(Of, _edx != 0);
	}
	
	private void ExecImul(Instruction insn, VirtualMemory mem)
	{
		if (insn.OpCount == 1)
		{
			int src = (int)GetOperandValue(insn, 0);
			long result = (long)(int)_eax * src;
			_eax = (uint)result;
			_edx = (uint)(result >> 32);
			
			bool overflow = (result < int.MinValue || result > int.MaxValue);
			SetFlagVal(Cf, overflow);
			SetFlagVal(Of, overflow);
		}
		else if (insn.OpCount == 2)
		{
			int src1 = (int)GetOperandValue(insn, 0);
			int src2 = (int)GetOperandValue(insn, 1);
			long result = (long)src1 * src2;
			SetOperandValue(insn, 0, (uint)result);
			
			bool overflow = (result < int.MinValue || result > int.MaxValue);
			SetFlagVal(Cf, overflow);
			SetFlagVal(Of, overflow);
		}
		else if (insn.OpCount == 3)
		{
			int src1 = (int)GetOperandValue(insn, 1);
			int src2 = (int)GetOperandValue(insn, 2);
			long result = (long)src1 * src2;
			SetOperandValue(insn, 0, (uint)result);
			
			bool overflow = (result < int.MinValue || result > int.MaxValue);
			SetFlagVal(Cf, overflow);
			SetFlagVal(Of, overflow);
		}
	}
	
	private void ExecDiv(Instruction insn, VirtualMemory mem)
	{
		uint divisor = GetOperandValue(insn, 0);
		if (divisor == 0)
		{
			_logger.LogError("[JitCpu] Division by zero");
			return;
		}
		
		ulong dividend = ((ulong)_edx << 32) | _eax;
		_eax = (uint)(dividend / divisor);
		_edx = (uint)(dividend % divisor);
	}
	
	private void ExecIdiv(Instruction insn, VirtualMemory mem)
	{
		int divisor = (int)GetOperandValue(insn, 0);
		if (divisor == 0)
		{
			_logger.LogError("[JitCpu] Division by zero");
			return;
		}
		
		long dividend = ((long)(int)_edx << 32) | _eax;
		_eax = (uint)(dividend / divisor);
		_edx = (uint)(dividend % divisor);
	}
	
	// === Additional Implementations ===
	
	private void ExecPushad(VirtualMemory mem)
	{
		uint temp = _esp;
		mem.Write32(_esp -= 4, _eax);
		mem.Write32(_esp -= 4, _ecx);
		mem.Write32(_esp -= 4, _edx);
		mem.Write32(_esp -= 4, _ebx);
		mem.Write32(_esp -= 4, temp);
		mem.Write32(_esp -= 4, _ebp);
		mem.Write32(_esp -= 4, _esi);
		mem.Write32(_esp -= 4, _edi);
	}
	
	private void ExecPopad(VirtualMemory mem)
	{
		_edi = mem.Read32(_esp); _esp += 4;
		_esi = mem.Read32(_esp); _esp += 4;
		_ebp = mem.Read32(_esp); _esp += 4;
		_esp += 4; // Skip ESP
		_ebx = mem.Read32(_esp); _esp += 4;
		_edx = mem.Read32(_esp); _esp += 4;
		_ecx = mem.Read32(_esp); _esp += 4;
		_eax = mem.Read32(_esp); _esp += 4;
	}
	
	private void ExecCdq()
	{
		_edx = (_eax & 0x80000000) != 0 ? 0xFFFFFFFF : 0;
	}
	
	private void ExecBswap(Instruction insn)
	{
		uint value = GetOperandValue(insn, 0);
		uint result = ((value & 0xFF) << 24) |
		             ((value & 0xFF00) << 8) |
		             ((value & 0xFF0000) >> 8) |
		             ((value & 0xFF000000) >> 24);
		SetOperandValue(insn, 0, result);
	}
	
	private void ExecXlatb(VirtualMemory mem)
	{
		uint address = _ebx + (_eax & 0xFF);
		byte value = mem.Read8(address);
		_eax = (_eax & 0xFFFFFF00) | value;
	}
	
	private void ExecSetcc(Instruction insn)
	{
		bool condition = insn.Mnemonic switch
		{
			Mnemonic.Seto => GetFlag(Of),
			Mnemonic.Setno => !GetFlag(Of),
			Mnemonic.Setb => GetFlag(Cf),
			Mnemonic.Setae => !GetFlag(Cf),
			Mnemonic.Sete => GetFlag(Zf),
			Mnemonic.Setne => !GetFlag(Zf),
			Mnemonic.Setbe => GetFlag(Cf) || GetFlag(Zf),
			Mnemonic.Seta => !GetFlag(Cf) && !GetFlag(Zf),
			Mnemonic.Sets => GetFlag(Sf),
			Mnemonic.Setns => !GetFlag(Sf),
			Mnemonic.Setp => GetFlag(Pf),
			Mnemonic.Setnp => !GetFlag(Pf),
			Mnemonic.Setl => GetFlag(Sf) != GetFlag(Of),
			Mnemonic.Setge => GetFlag(Sf) == GetFlag(Of),
			Mnemonic.Setle => GetFlag(Zf) || (GetFlag(Sf) != GetFlag(Of)),
			Mnemonic.Setg => !GetFlag(Zf) && (GetFlag(Sf) == GetFlag(Of)),
			_ => false
		};
		
		SetOperandValue(insn, 0, condition ? 1u : 0u);
	}

	private void ExecFninit()
	{
		// FNINIT - Initialize FPU (no wait)
		// Reset FPU to default state
		_fpuControlWord = 0x037F;
		_fpuStatusWord = 0x0000;
		_fpuTagWord = 0xFFFF; // All tags set to 11b (empty)
		_fpuTop = 0;
		Array.Clear(_fpu, 0, _fpu.Length);
	}

	private void ExecFnclex()
	{
	    // FNCLEX - Clear FPU Exceptions (no wait)
	    // Clears the exception flags (bits 0-5), stack fault (bit 6),
	    // error summary (bit 7), and busy (bit 15) flags in the FPU status word.
	    // Preserves condition codes (bits 8-10, 14) and TOP (bits 11-13).
	    _fpuStatusWord &= 0x7F00;
	}

	private void ExecFstsw(Instruction insn)
	{
		// FSTSW - Store FPU Status Word
		// Stores the FPU status word to AX register or memory (16-bit)
		if (insn.Op0Kind == OpKind.Register && insn.Op0Register == Register.AX)
		{
			// FSTSW AX - Store to AX register
			_eax = (_eax & 0xFFFF0000) | _fpuStatusWord;
		}
		else if (insn.Op0Kind == OpKind.Memory)
		{
			// FSTSW m16 - Store to memory
			uint addr = CalcMemAddress(insn, 0);
			_mem.Write16(addr, _fpuStatusWord);
		}
		else
		{
			throw new NotImplementedException($"[JitCpu] FSTSW with unsupported operand type: {insn.Op0Kind}");
		}
	}

	private void ExecEmms()
	{
		// EMMS - Empty MMX State
		// This instruction sets the x87 FPU tag word to empty (all tags = 11b)
		// This is required after using MMX instructions before using x87 FPU instructions
		// Each of the 8 FPU registers uses 2 bits in the tag word:
		//   00b = Valid, 01b = Zero, 10b = Special, 11b = Empty
		_fpuTagWord = 0xFFFF; // Set all 8 tags to 11b (empty)
		
		// Clear MMX register state to prevent data leakage
		Array.Clear(_mmx, 0, _mmx.Length);
	}

	// MMX instruction implementations
	private void ExecMmxMovd(Instruction insn)
	{
		// MOVD - Move Doubleword between MMX register and memory/GPR
		if (insn.Op0Kind == OpKind.Register && insn.Op0Register >= Register.MM0 && insn.Op0Register <= Register.MM7)
		{
			// Destination is MMX register
			int mmReg = insn.Op0Register - Register.MM0;
			uint value = GetOperandValue(insn, 1);
			_mmx[mmReg] = value; // Zero-extend to 64 bits
		}
		else if (insn.Op1Kind == OpKind.Register && insn.Op1Register >= Register.MM0 && insn.Op1Register <= Register.MM7)
		{
			// Source is MMX register
			int mmReg = insn.Op1Register - Register.MM0;
			uint value = (uint)_mmx[mmReg];
			SetOperandValue(insn, 0, value);
		}
		else
		{
			throw new NotImplementedException($"[JitCpu] MOVD with unsupported operand types: {insn.Op0Kind}, {insn.Op1Kind}");
		}
	}

	private void ExecMmxMovq(Instruction insn)
	{
		// MOVQ - Move Quadword between MMX registers or memory
		if (insn.Op0Kind == OpKind.Register && insn.Op0Register >= Register.MM0 && insn.Op0Register <= Register.MM7)
		{
			// Destination is MMX register
			int mmRegDst = insn.Op0Register - Register.MM0;
			
			if (insn.Op1Kind == OpKind.Register && insn.Op1Register >= Register.MM0 && insn.Op1Register <= Register.MM7)
			{
				// Source is also MMX register
				int mmRegSrc = insn.Op1Register - Register.MM0;
				_mmx[mmRegDst] = _mmx[mmRegSrc];
			}
			else if (insn.Op1Kind == OpKind.Memory)
			{
				// Source is memory
				uint addr = CalcMemAddress(insn, 1);
				_mmx[mmRegDst] = _mem.Read64(addr);
			}
			else
			{
				throw new NotImplementedException($"[JitCpu] MOVQ with unsupported source: {insn.Op1Kind}");
			}
		}
		else if (insn.Op0Kind == OpKind.Memory && insn.Op1Kind == OpKind.Register && insn.Op1Register >= Register.MM0 && insn.Op1Register <= Register.MM7)
		{
			// Destination is memory, source is MMX register
			int mmRegSrc = insn.Op1Register - Register.MM0;
			uint addr = CalcMemAddress(insn, 0);
			_mem.Write64(addr, _mmx[mmRegSrc]);
		}
		else
		{
			throw new NotImplementedException($"[JitCpu] MOVQ with unsupported operand types: {insn.Op0Kind}, {insn.Op1Kind}");
		}
	}

	private void ExecMmxArithmetic(Instruction insn)
	{
		// Get destination and source MMX registers
		if (insn.Op0Kind != OpKind.Register || insn.Op0Register < Register.MM0 || insn.Op0Register > Register.MM7)
		{
			throw new NotImplementedException($"[JitCpu] MMX arithmetic with non-MMX destination: {insn.Op0Kind}");
		}

		int mmRegDst = insn.Op0Register - Register.MM0;
		ulong src;

		if (insn.Op1Kind == OpKind.Register && insn.Op1Register >= Register.MM0 && insn.Op1Register <= Register.MM7)
		{
			int mmRegSrc = insn.Op1Register - Register.MM0;
			src = _mmx[mmRegSrc];
		}
		else if (insn.Op1Kind == OpKind.Memory)
		{
			uint addr = CalcMemAddress(insn, 1);
			src = _mem.Read64(addr);
		}
		else
		{
			throw new NotImplementedException($"[JitCpu] MMX arithmetic with unsupported source: {insn.Op1Kind}");
		}

		ulong dst = _mmx[mmRegDst];

		// Perform the operation based on mnemonic
		switch (insn.Mnemonic)
		{
			// Packed Add
			case Mnemonic.Paddb:
				_mmx[mmRegDst] = MmxPaddB(dst, src);
				break;
			case Mnemonic.Paddw:
				_mmx[mmRegDst] = MmxPaddW(dst, src);
				break;
			case Mnemonic.Paddd:
				_mmx[mmRegDst] = MmxPaddD(dst, src);
				break;
			case Mnemonic.Paddsb:
				_mmx[mmRegDst] = MmxPaddSB(dst, src);
				break;
			case Mnemonic.Paddsw:
				_mmx[mmRegDst] = MmxPaddSW(dst, src);
				break;
			case Mnemonic.Paddusb:
				_mmx[mmRegDst] = MmxPaddUSB(dst, src);
				break;
			case Mnemonic.Paddusw:
				_mmx[mmRegDst] = MmxPaddUSW(dst, src);
				break;
			
			// Packed Subtract
			case Mnemonic.Psubb:
				_mmx[mmRegDst] = MmxPsubB(dst, src);
				break;
			case Mnemonic.Psubw:
				_mmx[mmRegDst] = MmxPsubW(dst, src);
				break;
			case Mnemonic.Psubd:
				_mmx[mmRegDst] = MmxPsubD(dst, src);
				break;
			case Mnemonic.Psubsb:
				_mmx[mmRegDst] = MmxPsubSB(dst, src);
				break;
			case Mnemonic.Psubsw:
				_mmx[mmRegDst] = MmxPsubSW(dst, src);
				break;
			case Mnemonic.Psubusb:
				_mmx[mmRegDst] = MmxPsubUSB(dst, src);
				break;
			case Mnemonic.Psubusw:
				_mmx[mmRegDst] = MmxPsubUSW(dst, src);
				break;
			
			// Packed Multiply
			case Mnemonic.Pmullw:
				_mmx[mmRegDst] = MmxPmullW(dst, src);
				break;
			case Mnemonic.Pmulhw:
				_mmx[mmRegDst] = MmxPmulhW(dst, src);
				break;
			case Mnemonic.Pmaddwd:
				_mmx[mmRegDst] = MmxPmaddWD(dst, src);
				break;
			
			// Logical Operations
			case Mnemonic.Pand:
				_mmx[mmRegDst] = dst & src;
				break;
			case Mnemonic.Pandn:
				_mmx[mmRegDst] = (~dst) & src;
				break;
			case Mnemonic.Por:
				_mmx[mmRegDst] = dst | src;
				break;
			case Mnemonic.Pxor:
				_mmx[mmRegDst] = dst ^ src;
				break;
			
			// Comparison
			case Mnemonic.Pcmpeqb:
				_mmx[mmRegDst] = MmxPcmpeqB(dst, src);
				break;
			case Mnemonic.Pcmpeqw:
				_mmx[mmRegDst] = MmxPcmpeqW(dst, src);
				break;
			case Mnemonic.Pcmpeqd:
				_mmx[mmRegDst] = MmxPcmpeqD(dst, src);
				break;
			case Mnemonic.Pcmpgtb:
				_mmx[mmRegDst] = MmxPcmpgtB(dst, src);
				break;
			case Mnemonic.Pcmpgtw:
				_mmx[mmRegDst] = MmxPcmpgtW(dst, src);
				break;
			case Mnemonic.Pcmpgtd:
				_mmx[mmRegDst] = MmxPcmpgtD(dst, src);
				break;
			
			default:
				throw new NotImplementedException($"[JitCpu] Unhandled MMX arithmetic: {insn.Mnemonic}");
		}
	}

	private void ExecMmxShift(Instruction insn)
	{
		if (insn.Op0Kind != OpKind.Register || insn.Op0Register < Register.MM0 || insn.Op0Register > Register.MM7)
		{
			throw new NotImplementedException($"[JitCpu] MMX shift with non-MMX destination: {insn.Op0Kind}");
		}

		int mmRegDst = insn.Op0Register - Register.MM0;
		ulong dst = _mmx[mmRegDst];
		
		// Get shift count
		int count;
		if (insn.Op1Kind == OpKind.Immediate8)
		{
			count = (int)insn.Immediate8;
		}
		else if (insn.Op1Kind == OpKind.Register && insn.Op1Register >= Register.MM0 && insn.Op1Register <= Register.MM7)
		{
			// Shift count is in lower 64 bits of source MMX register
			int mmRegSrc = insn.Op1Register - Register.MM0;
			count = (int)(_mmx[mmRegSrc] & 0xFF);
		}
		else if (insn.Op1Kind == OpKind.Memory)
		{
			uint addr = CalcMemAddress(insn, 1);
			count = (int)(_mem.Read64(addr) & 0xFF);
		}
		else
		{
			throw new NotImplementedException($"[JitCpu] MMX shift with unsupported count source: {insn.Op1Kind}");
		}

		// Perform the shift operation
		switch (insn.Mnemonic)
		{
			case Mnemonic.Psllw:
				_mmx[mmRegDst] = MmxPsllW(dst, count);
				break;
			case Mnemonic.Pslld:
				_mmx[mmRegDst] = MmxPsllD(dst, count);
				break;
			case Mnemonic.Psllq:
				_mmx[mmRegDst] = MmxPsllQ(dst, count);
				break;
			case Mnemonic.Psrlw:
				_mmx[mmRegDst] = MmxPsrlW(dst, count);
				break;
			case Mnemonic.Psrld:
				_mmx[mmRegDst] = MmxPsrlD(dst, count);
				break;
			case Mnemonic.Psrlq:
				_mmx[mmRegDst] = MmxPsrlQ(dst, count);
				break;
			case Mnemonic.Psraw:
				_mmx[mmRegDst] = MmxPsraW(dst, count);
				break;
			case Mnemonic.Psrad:
				_mmx[mmRegDst] = MmxPsraD(dst, count);
				break;
			default:
				throw new NotImplementedException($"[JitCpu] Unhandled MMX shift: {insn.Mnemonic}");
		}
	}

	private void ExecMmxPack(Instruction insn)
	{
		if (insn.Op0Kind != OpKind.Register || insn.Op0Register < Register.MM0 || insn.Op0Register > Register.MM7)
		{
			throw new NotImplementedException($"[JitCpu] MMX pack with non-MMX destination: {insn.Op0Kind}");
		}

		int mmRegDst = insn.Op0Register - Register.MM0;
		ulong dst = _mmx[mmRegDst];
		ulong src;

		if (insn.Op1Kind == OpKind.Register && insn.Op1Register >= Register.MM0 && insn.Op1Register <= Register.MM7)
		{
			int mmRegSrc = insn.Op1Register - Register.MM0;
			src = _mmx[mmRegSrc];
		}
		else if (insn.Op1Kind == OpKind.Memory)
		{
			uint addr = CalcMemAddress(insn, 1);
			src = _mem.Read64(addr);
		}
		else
		{
			throw new NotImplementedException($"[JitCpu] MMX pack with unsupported source: {insn.Op1Kind}");
		}

		switch (insn.Mnemonic)
		{
			case Mnemonic.Packsswb:
				_mmx[mmRegDst] = MmxPacksswb(dst, src);
				break;
			case Mnemonic.Packssdw:
				_mmx[mmRegDst] = MmxPackssdw(dst, src);
				break;
			case Mnemonic.Packuswb:
				_mmx[mmRegDst] = MmxPackuswb(dst, src);
				break;
			case Mnemonic.Punpckhbw:
				_mmx[mmRegDst] = MmxPunpckhbw(dst, src);
				break;
			case Mnemonic.Punpckhwd:
				_mmx[mmRegDst] = MmxPunpckhwd(dst, src);
				break;
			case Mnemonic.Punpckhdq:
				_mmx[mmRegDst] = MmxPunpckhdq(dst, src);
				break;
			case Mnemonic.Punpcklbw:
				_mmx[mmRegDst] = MmxPunpcklbw(dst, src);
				break;
			case Mnemonic.Punpcklwd:
				_mmx[mmRegDst] = MmxPunpcklwd(dst, src);
				break;
			case Mnemonic.Punpckldq:
				_mmx[mmRegDst] = MmxPunpckldq(dst, src);
				break;
			default:
				throw new NotImplementedException($"[JitCpu] Unhandled MMX pack: {insn.Mnemonic}");
		}
	}

	// MMX helper methods for packed operations
	private ulong MmxPaddB(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 8; i++)
		{
			byte va = (byte)((a >> (i * 8)) & 0xFF);
			byte vb = (byte)((b >> (i * 8)) & 0xFF);
			result |= (ulong)((byte)(va + vb)) << (i * 8);
		}
		return result;
	}

	private ulong MmxPaddW(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 4; i++)
		{
			ushort va = (ushort)((a >> (i * 16)) & 0xFFFF);
			ushort vb = (ushort)((b >> (i * 16)) & 0xFFFF);
			result |= (ulong)((ushort)(va + vb)) << (i * 16);
		}
		return result;
	}

	private ulong MmxPaddD(ulong a, ulong b)
	{
		uint lo_a = (uint)(a & 0xFFFFFFFF);
		uint lo_b = (uint)(b & 0xFFFFFFFF);
		uint hi_a = (uint)((a >> 32) & 0xFFFFFFFF);
		uint hi_b = (uint)((b >> 32) & 0xFFFFFFFF);
		return ((ulong)(hi_a + hi_b) << 32) | (lo_a + lo_b);
	}

	private ulong MmxPaddSB(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 8; i++)
		{
			sbyte va = (sbyte)((a >> (i * 8)) & 0xFF);
			sbyte vb = (sbyte)((b >> (i * 8)) & 0xFF);
			int sum = va + vb;
			if (sum > 127) sum = 127;
			if (sum < -128) sum = -128;
			result |= (ulong)((byte)sum) << (i * 8);
		}
		return result;
	}

	private ulong MmxPaddSW(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 4; i++)
		{
			short va = (short)((a >> (i * 16)) & 0xFFFF);
			short vb = (short)((b >> (i * 16)) & 0xFFFF);
			int sum = va + vb;
			if (sum > 32767) sum = 32767;
			if (sum < -32768) sum = -32768;
			result |= (ulong)((ushort)sum) << (i * 16);
		}
		return result;
	}

	private ulong MmxPaddUSB(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 8; i++)
		{
			byte va = (byte)((a >> (i * 8)) & 0xFF);
			byte vb = (byte)((b >> (i * 8)) & 0xFF);
			int sum = va + vb;
			if (sum > 255) sum = 255;
			result |= (ulong)((byte)sum) << (i * 8);
		}
		return result;
	}

	private ulong MmxPaddUSW(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 4; i++)
		{
			ushort va = (ushort)((a >> (i * 16)) & 0xFFFF);
			ushort vb = (ushort)((b >> (i * 16)) & 0xFFFF);
			int sum = va + vb;
			if (sum > 65535) sum = 65535;
			result |= (ulong)((ushort)sum) << (i * 16);
		}
		return result;
	}

	private ulong MmxPsubB(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 8; i++)
		{
			byte va = (byte)((a >> (i * 8)) & 0xFF);
			byte vb = (byte)((b >> (i * 8)) & 0xFF);
			result |= (ulong)((byte)(va - vb)) << (i * 8);
		}
		return result;
	}

	private ulong MmxPsubW(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 4; i++)
		{
			ushort va = (ushort)((a >> (i * 16)) & 0xFFFF);
			ushort vb = (ushort)((b >> (i * 16)) & 0xFFFF);
			result |= (ulong)((ushort)(va - vb)) << (i * 16);
		}
		return result;
	}

	private ulong MmxPsubD(ulong a, ulong b)
	{
		uint lo_a = (uint)(a & 0xFFFFFFFF);
		uint lo_b = (uint)(b & 0xFFFFFFFF);
		uint hi_a = (uint)((a >> 32) & 0xFFFFFFFF);
		uint hi_b = (uint)((b >> 32) & 0xFFFFFFFF);
		return ((ulong)(hi_a - hi_b) << 32) | (lo_a - lo_b);
	}

	private ulong MmxPsubSB(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 8; i++)
		{
			sbyte va = (sbyte)((a >> (i * 8)) & 0xFF);
			sbyte vb = (sbyte)((b >> (i * 8)) & 0xFF);
			int diff = va - vb;
			if (diff > 127) diff = 127;
			if (diff < -128) diff = -128;
			result |= (ulong)((byte)diff) << (i * 8);
		}
		return result;
	}

	private ulong MmxPsubSW(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 4; i++)
		{
			short va = (short)((a >> (i * 16)) & 0xFFFF);
			short vb = (short)((b >> (i * 16)) & 0xFFFF);
			int diff = va - vb;
			if (diff > 32767) diff = 32767;
			if (diff < -32768) diff = -32768;
			result |= (ulong)((ushort)diff) << (i * 16);
		}
		return result;
	}

	private ulong MmxPsubUSB(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 8; i++)
		{
			byte va = (byte)((a >> (i * 8)) & 0xFF);
			byte vb = (byte)((b >> (i * 8)) & 0xFF);
			int diff = va - vb;
			if (diff < 0) diff = 0;
			result |= (ulong)((byte)diff) << (i * 8);
		}
		return result;
	}

	private ulong MmxPsubUSW(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 4; i++)
		{
			ushort va = (ushort)((a >> (i * 16)) & 0xFFFF);
			ushort vb = (ushort)((b >> (i * 16)) & 0xFFFF);
			int diff = va - vb;
			if (diff < 0) diff = 0;
			result |= (ulong)((ushort)diff) << (i * 16);
		}
		return result;
	}

	private ulong MmxPmullW(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 4; i++)
		{
			short va = (short)((a >> (i * 16)) & 0xFFFF);
			short vb = (short)((b >> (i * 16)) & 0xFFFF);
			int product = va * vb;
			result |= (ulong)((ushort)product) << (i * 16);
		}
		return result;
	}

	private ulong MmxPmulhW(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 4; i++)
		{
			short va = (short)((a >> (i * 16)) & 0xFFFF);
			short vb = (short)((b >> (i * 16)) & 0xFFFF);
			int product = va * vb;
			result |= (ulong)((ushort)(product >> 16)) << (i * 16);
		}
		return result;
	}

	private ulong MmxPmaddWD(ulong a, ulong b)
	{
		short a0 = (short)(a & 0xFFFF);
		short a1 = (short)((a >> 16) & 0xFFFF);
		short a2 = (short)((a >> 32) & 0xFFFF);
		short a3 = (short)((a >> 48) & 0xFFFF);
		short b0 = (short)(b & 0xFFFF);
		short b1 = (short)((b >> 16) & 0xFFFF);
		short b2 = (short)((b >> 32) & 0xFFFF);
		short b3 = (short)((b >> 48) & 0xFFFF);
		
		int lo = (a0 * b0) + (a1 * b1);
		int hi = (a2 * b2) + (a3 * b3);
		
		return ((ulong)(uint)hi << 32) | (uint)lo;
	}

	private ulong MmxPcmpeqB(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 8; i++)
		{
			byte va = (byte)((a >> (i * 8)) & 0xFF);
			byte vb = (byte)((b >> (i * 8)) & 0xFF);
			result |= (ulong)(va == vb ? 0xFF : 0x00) << (i * 8);
		}
		return result;
	}

	private ulong MmxPcmpeqW(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 4; i++)
		{
			ushort va = (ushort)((a >> (i * 16)) & 0xFFFF);
			ushort vb = (ushort)((b >> (i * 16)) & 0xFFFF);
			result |= (ulong)(va == vb ? 0xFFFF : 0x0000) << (i * 16);
		}
		return result;
	}

	private ulong MmxPcmpeqD(ulong a, ulong b)
	{
		uint lo_a = (uint)(a & 0xFFFFFFFF);
		uint lo_b = (uint)(b & 0xFFFFFFFF);
		uint hi_a = (uint)((a >> 32) & 0xFFFFFFFF);
		uint hi_b = (uint)((b >> 32) & 0xFFFFFFFF);
		return ((ulong)(hi_a == hi_b ? 0xFFFFFFFF : 0x00000000) << 32) | (lo_a == lo_b ? 0xFFFFFFFF : 0x00000000);
	}

	private ulong MmxPcmpgtB(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 8; i++)
		{
			sbyte va = (sbyte)((a >> (i * 8)) & 0xFF);
			sbyte vb = (sbyte)((b >> (i * 8)) & 0xFF);
			result |= (ulong)(va > vb ? 0xFF : 0x00) << (i * 8);
		}
		return result;
	}

	private ulong MmxPcmpgtW(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 4; i++)
		{
			short va = (short)((a >> (i * 16)) & 0xFFFF);
			short vb = (short)((b >> (i * 16)) & 0xFFFF);
			result |= (ulong)(va > vb ? 0xFFFF : 0x0000) << (i * 16);
		}
		return result;
	}

	private ulong MmxPcmpgtD(ulong a, ulong b)
	{
		int lo_a = (int)(a & 0xFFFFFFFF);
		int lo_b = (int)(b & 0xFFFFFFFF);
		int hi_a = (int)((a >> 32) & 0xFFFFFFFF);
		int hi_b = (int)((b >> 32) & 0xFFFFFFFF);
		return ((ulong)(hi_a > hi_b ? 0xFFFFFFFF : 0x00000000) << 32) | (uint)(lo_a > lo_b ? 0xFFFFFFFF : 0x00000000);
	}

	private ulong MmxPsllW(ulong value, int count)
	{
		if (count > 15) return 0;
		ulong result = 0;
		for (int i = 0; i < 4; i++)
		{
			ushort v = (ushort)((value >> (i * 16)) & 0xFFFF);
			result |= (ulong)((ushort)(v << count)) << (i * 16);
		}
		return result;
	}

	private ulong MmxPsllD(ulong value, int count)
	{
		if (count > 31) return 0;
		uint lo = (uint)(value & 0xFFFFFFFF);
		uint hi = (uint)((value >> 32) & 0xFFFFFFFF);
		return ((ulong)(hi << count) << 32) | (lo << count);
	}

	private ulong MmxPsllQ(ulong value, int count)
	{
		if (count > 63) return 0;
		return value << count;
	}

	private ulong MmxPsrlW(ulong value, int count)
	{
		if (count > 15) return 0;
		ulong result = 0;
		for (int i = 0; i < 4; i++)
		{
			ushort v = (ushort)((value >> (i * 16)) & 0xFFFF);
			result |= (ulong)((ushort)(v >> count)) << (i * 16);
		}
		return result;
	}

	private ulong MmxPsrlD(ulong value, int count)
	{
		if (count > 31) return 0;
		uint lo = (uint)(value & 0xFFFFFFFF);
		uint hi = (uint)((value >> 32) & 0xFFFFFFFF);
		return ((ulong)(hi >> count) << 32) | (lo >> count);
	}

	private ulong MmxPsrlQ(ulong value, int count)
	{
		if (count > 63) return 0;
		return value >> count;
	}

	private ulong MmxPsraW(ulong value, int count)
	{
		if (count > 15) count = 15;
		ulong result = 0;
		for (int i = 0; i < 4; i++)
		{
			short v = (short)((value >> (i * 16)) & 0xFFFF);
			result |= (ulong)((ushort)(v >> count)) << (i * 16);
		}
		return result;
	}

	private ulong MmxPsraD(ulong value, int count)
	{
		if (count > 31) count = 31;
		int lo = (int)(value & 0xFFFFFFFF);
		int hi = (int)((value >> 32) & 0xFFFFFFFF);
		return ((ulong)(uint)(hi >> count) << 32) | (uint)(lo >> count);
	}

	private ulong MmxPacksswb(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 4; i++)
		{
			short v = (short)((a >> (i * 16)) & 0xFFFF);
			if (v > 127) v = 127;
			if (v < -128) v = -128;
			result |= (ulong)((byte)v) << (i * 8);
		}
		for (int i = 0; i < 4; i++)
		{
			short v = (short)((b >> (i * 16)) & 0xFFFF);
			if (v > 127) v = 127;
			if (v < -128) v = -128;
			result |= (ulong)((byte)v) << ((i + 4) * 8);
		}
		return result;
	}

	private ulong MmxPackssdw(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 2; i++)
		{
			int v = (int)((a >> (i * 32)) & 0xFFFFFFFF);
			if (v > 32767) v = 32767;
			if (v < -32768) v = -32768;
			result |= (ulong)((ushort)v) << (i * 16);
		}
		for (int i = 0; i < 2; i++)
		{
			int v = (int)((b >> (i * 32)) & 0xFFFFFFFF);
			if (v > 32767) v = 32767;
			if (v < -32768) v = -32768;
			result |= (ulong)((ushort)v) << ((i + 2) * 16);
		}
		return result;
	}

	private ulong MmxPackuswb(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 4; i++)
		{
			short v = (short)((a >> (i * 16)) & 0xFFFF);
			if (v > 255) v = 255;
			if (v < 0) v = 0;
			result |= (ulong)((byte)v) << (i * 8);
		}
		for (int i = 0; i < 4; i++)
		{
			short v = (short)((b >> (i * 16)) & 0xFFFF);
			if (v > 255) v = 255;
			if (v < 0) v = 0;
			result |= (ulong)((byte)v) << ((i + 4) * 8);
		}
		return result;
	}

	private ulong MmxPunpckhbw(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 4; i++)
		{
			byte va = (byte)((a >> ((i + 4) * 8)) & 0xFF);
			byte vb = (byte)((b >> ((i + 4) * 8)) & 0xFF);
			result |= (ulong)va << (i * 16);
			result |= (ulong)vb << (i * 16 + 8);
		}
		return result;
	}

	private ulong MmxPunpckhwd(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 2; i++)
		{
			ushort va = (ushort)((a >> ((i + 2) * 16)) & 0xFFFF);
			ushort vb = (ushort)((b >> ((i + 2) * 16)) & 0xFFFF);
			result |= (ulong)va << (i * 32);
			result |= (ulong)vb << (i * 32 + 16);
		}
		return result;
	}

	private ulong MmxPunpckhdq(ulong a, ulong b)
	{
		uint va = (uint)((a >> 32) & 0xFFFFFFFF);
		uint vb = (uint)((b >> 32) & 0xFFFFFFFF);
		return ((ulong)vb << 32) | va;
	}

	private ulong MmxPunpcklbw(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 4; i++)
		{
			byte va = (byte)((a >> (i * 8)) & 0xFF);
			byte vb = (byte)((b >> (i * 8)) & 0xFF);
			result |= (ulong)va << (i * 16);
			result |= (ulong)vb << (i * 16 + 8);
		}
		return result;
	}

	private ulong MmxPunpcklwd(ulong a, ulong b)
	{
		ulong result = 0;
		for (int i = 0; i < 2; i++)
		{
			ushort va = (ushort)((a >> (i * 16)) & 0xFFFF);
			ushort vb = (ushort)((b >> (i * 16)) & 0xFFFF);
			result |= (ulong)va << (i * 32);
			result |= (ulong)vb << (i * 32 + 16);
		}
		return result;
	}

	private ulong MmxPunpckldq(ulong a, ulong b)
	{
		uint va = (uint)(a & 0xFFFFFFFF);
		uint vb = (uint)(b & 0xFFFFFFFF);
		return ((ulong)vb << 32) | va;
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
