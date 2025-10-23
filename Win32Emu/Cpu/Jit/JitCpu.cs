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
		
		_logger.LogInformation("[JitCpu] Initialized JIT CPU backend");
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
			default:
				_logger.LogWarning("[JitCpu] Unimplemented instruction: {Mnemonic}", insn.Mnemonic);
				break;
		}
		
		return new CpuStepResult(isCall, callTarget);
	}

	private CompiledBlock CompileBlock(uint startEip, VirtualMemory mem)
	{
		_logger.LogInformation("[JitCpu] Compiling block at EIP=0x{Eip:X8}", startEip);
		
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
