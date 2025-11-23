using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Iced.Intel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Memory;

namespace Win32Emu.Cpu.Iced;

public class IcedCpu : IAsyncCpu
{
	private readonly VirtualMemory _mem;
	private readonly ILogger _logger;

	private uint _eax, _ebx, _ecx, _edx, _esi, _edi, _ebp, _esp, _eip, _eflags;
	
	// Segment registers (stored as 16-bit values in lower 16 bits)
	private ushort _cs, _ds, _es, _fs, _gs, _ss;

	private readonly Decoder _decoder;
	private readonly SimpleMemoryCodeReader _reader;
	private readonly InstructionAnalyzer? _analyzer;

	// EFLAGS bit positions
	private const int Cf = 0, Pf = 2, Af = 4, Zf = 6, Sf = 7, Tf = 8, If = 9, Df = 10, Of = 11;

	// Default image base if not specified (typical default for Win32 executables)
	private const uint DEFAULT_IMAGE_BASE = 0x00400000;
	
	// Default stack region bounds if not specified (typical range for Windows applications)
	private const uint DEFAULT_STACK_LIMIT = 0x00100000;  // 1 MB (bottom of stack)
	private const uint DEFAULT_STACK_BASE = 0x01000000;   // 16 MB (top of stack)
	
	// Image base from PE header (used for validation of indirect calls/jumps)
	private readonly uint _imageBase;
	
	// Stack bounds from PE header (used for validation of indirect calls/jumps)
	// Stack grows downward from _stackBase to _stackLimit
	private readonly uint _stackLimit;
	private readonly uint _stackBase;
	
	// CPU bitness mode (16 for real mode, 32 for protected mode)
	private readonly int _bitness;

	// x87 FPU state (8 registers in a stack, ST(0) to ST(7))
	private readonly double[] _fpu = new double[8];
	private int _fpuTop = 0; // Index of ST(0) in the circular stack
	private ushort _fpuControlWord = 0x037F; // Default FPU control word
	private ushort _fpuStatusWord = 0x0000; // FPU status word
	private ushort _fpuTagWord = 0xFFFF; // FPU tag word (all tags set to 11b = empty)

	// Control Registers (CR0-CR4) for protected mode and system control
	// CR0: System control flags (PE, MP, EM, TS, ET, NE, WP, AM, NW, CD, PG)
	// CR1: Reserved
	// CR2: Page fault linear address
	// CR3: Page directory base register (PDBR)
	// CR4: Extended system control flags (VME, PVI, TSD, DE, PSE, PAE, MCE, PGE, PCE, etc.)
	private uint _cr0 = 0x00000010; // ET flag set by default (387 present)
	private uint _cr2 = 0;
	private uint _cr3 = 0;
	private uint _cr4 = 0;

	// Debug Registers (DR0-DR7) for hardware breakpoints
	// DR0-DR3: Linear addresses of breakpoints
	// DR4-DR5: Reserved (aliased to DR6-DR7 on older CPUs)
	// DR6: Debug status register
	// DR7: Debug control register
	private uint _dr0 = 0;
	private uint _dr1 = 0;
	private uint _dr2 = 0;
	private uint _dr3 = 0;
	private uint _dr6 = 0xFFFF0FF0; // Initial value per Intel specification
	private uint _dr7 = 0x00000400; // Initial value per Intel specification

	// RDTSC support - use Stopwatch for high-resolution timing
	private static readonly Stopwatch RdtscStopwatch = Stopwatch.StartNew();
	private static readonly bool RdtscIsHighResolution = Stopwatch.IsHighResolution;
	private static readonly long RdtscFrequency = Stopwatch.Frequency;

	/// <summary>
	/// Initializes a new instance of the <see cref="IcedCpu"/> class.
	/// </summary>
	/// <param name="mem">The virtual memory instance used by the CPU.</param>
	/// <param name="logger">Optional logger for CPU events and diagnostics.</param>
	/// <param name="decoderOptions">Options for the instruction decoder.</param>
	/// <param name="enableInstructionAnalyzer">Whether to enable instruction analysis for debugging.</param>
	/// <param name="imageBase">The image base address for the emulated executable.</param>
	/// <param name="stackLimit">The lower bound of the stack region.</param>
	/// <param name="stackBase">The upper bound of the stack region.</param>
	/// <param name="bitness">
	/// The CPU bitness mode (16 for real mode, 32 for protected mode). Defaults to 32-bit.
	/// Use 16 for legacy DOS/real mode code, and 32 for Win32 protected mode applications.
	/// </param>
	public IcedCpu(VirtualMemory mem, ILogger? logger = null, DecoderOptions decoderOptions = DecoderOptions.None, bool enableInstructionAnalyzer = false, uint imageBase = DEFAULT_IMAGE_BASE, uint stackLimit = DEFAULT_STACK_LIMIT, uint stackBase = DEFAULT_STACK_BASE, int bitness = 32)
	{
		_mem = mem;
		_logger = logger ?? NullLogger.Instance;
		_imageBase = imageBase;
		_stackLimit = stackLimit;
		_stackBase = stackBase;
		_bitness = bitness;
		_reader = new SimpleMemoryCodeReader(this);
		_decoder = Decoder.Create(bitness, _reader, decoderOptions);
		
		if (enableInstructionAnalyzer)
		{
			_analyzer = new InstructionAnalyzer(logger);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void SetEip(uint eip) => _eip = eip;
	
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public uint GetEip() => _eip;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public uint GetRegister(string name) => name.ToUpperInvariant() switch
	{
		"EAX" => _eax, "EBX" => _ebx, "ECX" => _ecx, "EDX" => _edx, "ESI" => _esi, "EDI" => _edi, "EBP" => _ebp,
		"ESP" => _esp, "EIP" => _eip, "EFLAGS" => _eflags,
		"CS" => _cs, "DS" => _ds, "ES" => _es, "FS" => _fs, "GS" => _gs, "SS" => _ss,
		"CR0" => _cr0, "CR2" => _cr2, "CR3" => _cr3, "CR4" => _cr4,
		"DR0" => _dr0, "DR1" => _dr1, "DR2" => _dr2, "DR3" => _dr3, "DR6" => _dr6, "DR7" => _dr7,
		_ => 0
	};

	/// <summary>
	/// Gets the instruction analyzer if it was enabled during construction.
	/// </summary>
	public InstructionAnalyzer? GetInstructionAnalyzer() => _analyzer;

	/// <summary>
	/// Decodes and formats the instruction at the current EIP for debugging purposes.
	/// </summary>
	public string FormatCurrentInstruction()
	{
		if (_analyzer == null)
		{
			return "Instruction analyzer not enabled";
		}

		var insn = DecodeCurrentInstruction();
		return _analyzer.FormatInstructionWithAddress(insn);
	}

	/// <summary>
	/// Decodes and analyzes the instruction at the current EIP.
	/// </summary>
	public InstructionAnalysis? AnalyzeCurrentInstruction()
	{
		if (_analyzer == null)
		{
			return null;
		}

		var insn = DecodeCurrentInstruction();
		return _analyzer.AnalyzeInstruction(insn);
	}

	/// <summary>
	/// Decodes the instruction at the current EIP.
	/// </summary>
	private Instruction DecodeCurrentInstruction()
	{
		_reader.Reset(_eip);
		_decoder.IP = _eip;
		return _decoder.Decode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void SetRegister(string name, uint value, [CallerMemberName] string callerName = "")
	{
		switch (name.ToUpperInvariant())
		{
			case "EAX": _eax = value; break;
			case "EBX": _ebx = value; break;
			case "ECX": _ecx = value; break;
			case "EDX": _edx = value; break;
			case "ESI": _esi = value; break;
			case "EDI": _edi = value; break;
			case "EBP":
				_ebp = value;
				break;
			case "ESP": _esp = value; break;
			case "EIP": _eip = value; break;
			case "EFLAGS": _eflags = value; break;
			case "CS": _cs = (ushort)value; break;
			case "DS": _ds = (ushort)value; break;
			case "ES": _es = (ushort)value; break;
			case "FS": _fs = (ushort)value; break;
			case "GS": _gs = (ushort)value; break;
			case "SS": _ss = (ushort)value; break;
			case "CR0": _cr0 = value; break;
			case "CR2": _cr2 = value; break;
			case "CR3": _cr3 = value; break;
			case "CR4": _cr4 = value; break;
			case "DR0": _dr0 = value; break;
			case "DR1": _dr1 = value; break;
			case "DR2": _dr2 = value; break;
			case "DR3": _dr3 = value; break;
			case "DR6": _dr6 = value; break;
			case "DR7": _dr7 = value; break;
		}
	}

	public CpuStepResult SingleStep(VirtualMemory mem)
	{
		// Set diagnostics context for memory errors (without instruction bytes for performance)
		// Instruction bytes are omitted to avoid expensive memory access on every instruction
		Diagnostics.Diagnostics.SetCpuContext(new Diagnostics.Diagnostics.CpuContext(_eip, _esp, _ebp, _eax, _ecx, _edx, null));

		var oldEip = _eip; // Capture instruction address BEFORE any decoder operations
		_reader.Reset(_eip);
		_decoder.IP = _eip;
		var insn = _decoder.Decode();
		//_logger.LogInformation("Instruction: {Insn}", insn.ToString());
		
		// Validate LOCK prefix usage according to x86 specification
		// LOCK can only be used with specific instructions and only when destination is memory
		if (insn.HasLockPrefix)
		{
			// Check if instruction is in the allowed list
			var isLockAllowed = insn.Mnemonic switch
			{
				Mnemonic.Add or Mnemonic.Adc or Mnemonic.And or
				Mnemonic.Btc or Mnemonic.Btr or Mnemonic.Bts or
				Mnemonic.Cmpxchg or Mnemonic.Cmpxchg8b or Mnemonic.Cmpxchg16b or
				Mnemonic.Dec or Mnemonic.Inc or
				Mnemonic.Neg or Mnemonic.Not or
				Mnemonic.Or or Mnemonic.Sbb or Mnemonic.Sub or
				Mnemonic.Xor or Mnemonic.Xadd or Mnemonic.Xchg => true,
				_ => false
			};
			
			// Check if destination operand is memory (LOCK requires memory destination)
			var hasMemoryDestination = insn.Op0Kind == OpKind.Memory;
			
			// Generate #UD (Invalid Opcode) exception if LOCK is invalid
			if (!isLockAllowed || !hasMemoryDestination)
			{
				// Invalid LOCK prefix - generate #UD exception (vector 6)
				// This matches real 80386 hardware behavior
				GenerateException(6, oldEip, mem);
				
				// Clear diagnostics and return
				Diagnostics.Diagnostics.ClearCpuContext();
				return new CpuStepResult(false, 0, false);
			}
		}
		
		// Log instructions in the problematic range (after LoadCursorA returns)
		if (oldEip >= 0x00403160 && oldEip <= 0x004031A0)
		{
			var bytes = mem.GetSpan(oldEip, 16).ToArray();
			var byteString = string.Join(" ", bytes.Select(b => b.ToString("X2")));
			_logger.LogInformation("[IcedCpu] Executing at 0x{Eip:X8}: {Insn} (Bytes: {Bytes})", oldEip, insn.ToString(), byteString);
		}
		
		var isCall = false;
		var isSyscall = false;
		uint callTarget = 0;
		try
		{
			switch (insn.Mnemonic)
			{
				case Mnemonic.Mov: ExecMov(insn); break;
				case Mnemonic.Lea: ExecLea(insn); break;
				case Mnemonic.Movzx: ExecMovx(insn, false); break;
				case Mnemonic.Movsx: ExecMovx(insn, true); break;
				case Mnemonic.Push: ExecPush(insn); break;
				case Mnemonic.Pop: ExecPop(insn); break;
				case Mnemonic.Pushad: ExecPushad(); break;
				case Mnemonic.Popad: ExecPopad(); break;
				case Mnemonic.Add: ExecAdd(insn); break;
				case Mnemonic.Adc: ExecAdc(insn); break;
				case Mnemonic.Sub: ExecSub(insn); break;
				case Mnemonic.Sbb: ExecSbb(insn); break;
				case Mnemonic.Xor: ExecXor(insn); break;
				case Mnemonic.And: ExecLogic(insn, LogicOp.And); break;
				case Mnemonic.Or: ExecLogic(insn, LogicOp.Or); break;
				case Mnemonic.Test: ExecTest(insn); break;
				case Mnemonic.Cmp: ExecCmp(insn); break;
				case Mnemonic.Inc: ExecInc(insn); break;
				case Mnemonic.Dec: ExecDec(insn); break;
				case Mnemonic.Mul: ExecMul(insn); break;
				case Mnemonic.Imul: ExecImul(insn); break;
				case Mnemonic.Div: ExecDiv(insn); break;
				case Mnemonic.Idiv: ExecIdiv(insn); break;
				case Mnemonic.Shl:
				case Mnemonic.Sal: ExecShiftLeft(insn); break;
				case Mnemonic.Shr: ExecShiftRight(insn, false); break;
				case Mnemonic.Sar: ExecShiftRight(insn, true); break;
				case Mnemonic.Rol: ExecRotate(insn, RotateKind.Rol); break;
				case Mnemonic.Ror: ExecRotate(insn, RotateKind.Ror); break;
				case Mnemonic.Rcl: ExecRotate(insn, RotateKind.Rcl); break;
				case Mnemonic.Rcr: ExecRotate(insn, RotateKind.Rcr); break;
				case Mnemonic.Not: ExecNot(insn); break;
				case Mnemonic.Neg: ExecNeg(insn); break;
				case Mnemonic.Bswap: ExecBswap(insn); break;
				case Mnemonic.Cbw: ExecCbw(); break;
				case Mnemonic.Cwd: ExecCwd(); break;
				case Mnemonic.Cwde: ExecCwde(); break;
				case Mnemonic.Cdq: ExecCdq(); break;
				case Mnemonic.Xchg: ExecXchg(insn); break;
				case Mnemonic.Xlatb: ExecXlatb(); break;
				case Mnemonic.Cmpxchg: ExecCmpxchg(insn); break;
				case Mnemonic.Xadd: ExecXadd(insn); break;
				case Mnemonic.Cmpxchg8b: ExecCmpxchg8B(insn); break;
				case Mnemonic.Rdtsc: ExecRdtsc(); break;
				case Mnemonic.Cpuid: ExecCpuid(); break;
				case Mnemonic.Rdmsr: ExecRdmsr(); break;
				case Mnemonic.Wrmsr: ExecWrmsr(); break;
				case Mnemonic.Invd: ExecInvd(); break;
				case Mnemonic.Wbinvd: ExecWbinvd(); break;
				case Mnemonic.Invlpg: ExecInvlpg(insn); break;
				case Mnemonic.Rsm: ExecRsm(); break;
				// SETcc family
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
					ExecSetcc(insn); break;
				// CMOVcc family
				case Mnemonic.Cmove:
				case Mnemonic.Cmovne:
				case Mnemonic.Cmovb:
				case Mnemonic.Cmovbe:
				case Mnemonic.Cmova:
				case Mnemonic.Cmovae:
				case Mnemonic.Cmovge:
				case Mnemonic.Cmovg:
				case Mnemonic.Cmovl:
				case Mnemonic.Cmovo:
				case Mnemonic.Cmovno:
				case Mnemonic.Cmovs:
				case Mnemonic.Cmovns:
				case Mnemonic.Cmovp:
				case Mnemonic.Cmovnp:
					ExecCmovcc(insn); break;
				// FPU operations
				case Mnemonic.Fld: ExecFld(insn); break;
				case Mnemonic.Fst: ExecFst(insn, false); break;
				case Mnemonic.Fstp: ExecFst(insn, true); break;
				case Mnemonic.Fild: ExecFild(insn); break;
				case Mnemonic.Fistp: ExecFistp(insn); break;
				case Mnemonic.Fadd: ExecFadd(insn); break;
				case Mnemonic.Faddp: ExecFaddp(insn); break;
				case Mnemonic.Fsub: ExecFsub(insn); break;
				case Mnemonic.Fsubp: ExecFsubp(insn); break;
				case Mnemonic.Fsubr: ExecFsubr(insn); break;
				case Mnemonic.Fsubrp: ExecFsubrp(insn); break;
				case Mnemonic.Fmul: ExecFmul(insn); break;
				case Mnemonic.Fmulp: ExecFmulp(insn); break;
				case Mnemonic.Fdiv: ExecFdiv(insn); break;
				case Mnemonic.Fdivp: ExecFdivp(insn); break;
				case Mnemonic.Fdivr: ExecFdivr(insn); break;
				case Mnemonic.Fdivrp: ExecFdivrp(insn); break;
				case Mnemonic.Fsqrt: ExecFsqrt(); break;
				case Mnemonic.Fist: ExecFist(insn); break;
				case Mnemonic.Fiadd: ExecFiadd(insn); break;
				case Mnemonic.Fimul: ExecFimul(insn); break;
				case Mnemonic.Fisub: ExecFisub(insn); break;
				case Mnemonic.Fidiv: ExecFidiv(insn); break;
				case Mnemonic.Fidivr: ExecFidivr(insn); break;
				case Mnemonic.Fxch: ExecFxch(insn); break;
				case Mnemonic.Fchs: ExecFchs(); break;
				case Mnemonic.Fabs: ExecFabs(); break;
				case Mnemonic.Fldz: ExecFldz(); break;
				case Mnemonic.Fld1: ExecFld1(); break;
				case Mnemonic.Fldpi: ExecFldpi(); break;
				case Mnemonic.Fldl2e: ExecFldl2e(); break;
				case Mnemonic.Fsin: ExecFsin(); break;
				case Mnemonic.Fcos: ExecFcos(); break;
				case Mnemonic.Fsincos: ExecFsincos(); break;
				case Mnemonic.Fpatan: ExecFpatan(); break;
				case Mnemonic.F2xm1: ExecF2xm1(); break;
				case Mnemonic.Fscale: ExecFscale(); break;
				case Mnemonic.Fcom: ExecFcom(insn); break;
				case Mnemonic.Fcomp: ExecFcomp(insn); break;
				case Mnemonic.Fcompp: ExecFcompp(); break;
				case Mnemonic.Fucomi: ExecFucomi(insn); break;
				case Mnemonic.Fucomip: ExecFucomip(insn); break;
				case Mnemonic.Fcmovnbe: ExecFcmovnbe(insn); break;
				case Mnemonic.Fnstcw: ExecFnstcw(insn); break;
				case Mnemonic.Fnstsw: ExecFnstsw(insn); break;
				case Mnemonic.Fldcw: ExecFldcw(insn); break;
				case Mnemonic.Fninit: ExecFninit(); break;
				case Mnemonic.Fnclex: ExecFnclex(); break;
				case Mnemonic.Fxam: ExecFxam(); break;
				case Mnemonic.Wait: break; // FWAIT - no-op for now
				// Bit operations
				case Mnemonic.Bt: ExecBt(insn); break;
				case Mnemonic.Bts: ExecBts(insn); break;
				case Mnemonic.Btr: ExecBtr(insn); break;
				case Mnemonic.Btc: ExecBtc(insn); break;
				case Mnemonic.Bsf: ExecBsf(insn); break;
				case Mnemonic.Bsr: ExecBsr(insn); break;
				// Double shift operations
				case Mnemonic.Shld: ExecShld(insn); break;
				case Mnemonic.Shrd: ExecShrd(insn); break;
				// String ops (byte/word/dword variants)
				case Mnemonic.Movsb: ExecMovs(1, insn.HasRepPrefix); break;
				case Mnemonic.Movsw: ExecMovs(2, insn.HasRepPrefix); break;
				case Mnemonic.Movsd: ExecMovs(4, insn.HasRepPrefix); break;
				case Mnemonic.Stosb: ExecStos(1, insn.HasRepPrefix); break;
				case Mnemonic.Stosw: ExecStos(2, insn.HasRepPrefix); break;
				case Mnemonic.Stosd: ExecStos(4, insn.HasRepPrefix); break;
				case Mnemonic.Lodsb: ExecLods(1, insn.HasRepPrefix); break;
				case Mnemonic.Lodsw: ExecLods(2, insn.HasRepPrefix); break;
				case Mnemonic.Lodsd: ExecLods(4, insn.HasRepPrefix); break;
				case Mnemonic.Insb: ExecIns(1, insn.HasRepPrefix); break;
				case Mnemonic.Insw: ExecIns(2, insn.HasRepPrefix); break;
				case Mnemonic.Insd: ExecIns(4, insn.HasRepPrefix); break;
				case Mnemonic.Outsb: ExecOuts(1, insn.HasRepPrefix); break;
				case Mnemonic.Outsw: ExecOuts(2, insn.HasRepPrefix); break;
				case Mnemonic.Outsd: ExecOuts(4, insn.HasRepPrefix); break;
				case Mnemonic.Cmpsb: ExecCmps(1, insn.HasRepePrefix, insn.HasRepnePrefix); break;
				case Mnemonic.Cmpsw: ExecCmps(2, insn.HasRepePrefix, insn.HasRepnePrefix); break;
				case Mnemonic.Cmpsd: ExecCmps(4, insn.HasRepePrefix, insn.HasRepnePrefix); break;
				case Mnemonic.Scasb: ExecScas(1, insn.HasRepePrefix, insn.HasRepnePrefix); break;
				case Mnemonic.Scasw: ExecScas(2, insn.HasRepePrefix, insn.HasRepnePrefix); break;
				case Mnemonic.Scasd: ExecScas(4, insn.HasRepePrefix, insn.HasRepnePrefix); break;
				case Mnemonic.Jmp:
					if (insn.GetOpKind(0) == OpKind.Register)
					{
						var jmpTarget = GetReg32(insn.GetOpRegister(0));
						ValidateIndirectTarget(jmpTarget, oldEip, "JMP", insn.GetOpRegister(0));
						_eip = jmpTarget;
					}
					else if (insn.GetOpKind(0) == OpKind.Memory)
					{
						var jmpTarget = Read32(CalcMemAddress(insn));
						ValidateIndirectTarget(jmpTarget, oldEip, "JMP");
						_eip = jmpTarget;
					}
					else
					{
						_eip = (uint)insn.NearBranchTarget;
					}

					break;
				case Mnemonic.Call:
					// Determine operand size based on instruction Code enum
					// The decoder selects the appropriate Code based on bitness and operand-size prefix (66h)
					bool use32BitCall = insn.Code == Code.Call_rel32_32 || insn.Code == Code.Call_rel32_64 ||
					                    insn.Code == Code.Call_rm32 || insn.Code == Code.Call_rm64;
					
					// Push return address onto stack
					if (use32BitCall)
					{
						_esp -= 4;
						Write32(_esp, oldEip + (uint)insn.Length);
					}
					else
					{
						_esp -= 2;
						Write16(_esp, (ushort)((oldEip + (uint)insn.Length) & 0xFFFF));
					}
					
					// Determine call target
					if (insn.GetOpKind(0) == OpKind.Register)
					{
						var targetReg = insn.GetOpRegister(0);
						var callTargetAddr = GetReg32(targetReg);
						
						// Debug logging for register-based CALL in problematic range
						if (oldEip >= 0x00403180 && oldEip <= 0x004031A0)
						{
							_logger.LogWarning("[IcedCpu] CALL {Reg} at EIP=0x{OldEip:X8}: reg={Reg}, value=0x{Value:X8}, EBP=0x{Ebp:X8}, ESP=0x{Esp:X8}",
								targetReg, oldEip, targetReg, callTargetAddr, _ebp, _esp);
						}
						
						ValidateIndirectTarget(callTargetAddr, oldEip, "CALL", targetReg);
						_eip = callTargetAddr;
						callTarget = callTargetAddr;
						isCall = true;
					}
					else if (insn.GetOpKind(0) == OpKind.Memory)
					{
						var callTargetAddr = Read32(CalcMemAddress(insn));
						ValidateIndirectTarget(callTargetAddr, oldEip, "CALL");
						_eip = callTargetAddr;
						callTarget = callTargetAddr;
						isCall = true;
					}
					else
					{
						_eip = (uint)insn.NearBranchTarget;
						callTarget = _eip;
						isCall = true;
					}

					break;
				case Mnemonic.Ret:
					uint ret;
					uint oldEsp;
					
					// Determine operand size based on instruction Code enum
					// The decoder selects the appropriate Code based on bitness and operand-size prefix (66h)
					// In 16-bit mode: Retnw (default), Retnd (with 66h prefix)
					// In 32-bit mode: Retnd (default), Retnw (with 66h prefix)
					bool use32BitOperand = insn.Code == Code.Retnd || insn.Code == Code.Retnd_imm16;
					
					if (use32BitOperand)
					{
						// 32-bit mode: pop 4 bytes
						ret = Read32(_esp);
						oldEsp = _esp;
						_esp += 4;
						_eip = ret;
					}
					else
					{
						// 16-bit mode: pop 2 bytes as IP
						// Store the full value without wrapping - consistent with non-control-flow EIP handling
						// The 16-bit value popped from stack is zero-extended to 32 bits
						ret = Read16(_esp);
						oldEsp = _esp;
						_esp += 2;
						_eip = ret;  // Store as-is (16-bit value zero-extended to 32-bit)
					}
					
					// Handle immediate (cleanup bytes)
					if (insn.Immediate16 != 0)
					{
						_esp += insn.Immediate16;
					}
					
					// Log detailed RET information when in import stub or syscall dispatcher range
					if (MemoryRegions.IsInSyscallRange(oldEip) || MemoryRegions.IsInImportHookRange(oldEip))
					{
						_logger.LogDebug("[IcedCpu] RET at 0x{OldEip:X8}: popped 0x{RetAddr:X8} from ESP=0x{OldEsp:X8}, cleanup={Cleanup} bytes, new ESP=0x{NewEsp:X8}", 
							oldEip, ret, oldEsp, insn.Immediate16, _esp);
						
						// Verify EIP was actually set correctly
						_logger.LogDebug("[IcedCpu] RET: After setting _eip, current _eip value is 0x{Eip:X8}", _eip);
						
						// Warn if return address looks suspicious
						if (ret < 0x00400000 && ret != 0xFFFFFFFF)
						{
							_logger.LogWarning("[IcedCpu] RET at 0x{OldEip:X8}: return address 0x{RetAddr:X8} is suspiciously low (< 0x00400000). Possible stack corruption.", 
								oldEip, ret);
							
							// Dump stack contents for debugging
							try
							{
								var stackDump = new System.Text.StringBuilder();
								for (int i = -4; i <= 16; i += 4)
								{
									var addr = (uint)(oldEsp + i);
									var val = Read32(addr);
									stackDump.Append($"  [ESP{i:+0;-#}]=0x{addr:X8}: 0x{val:X8}");
								}
								_logger.LogWarning("[IcedCpu] Stack dump around ESP=0x{Esp:X8}:{StackDump}", oldEsp, stackDump.ToString());
							}
							catch
							{
								// Ignore errors reading stack
							}
						}
					}

					break;
				case Mnemonic.Leave: ExecLeave(); break;
				case Mnemonic.Nop: break;
				case Mnemonic.Cld: ClearFlag(Df); break;
				case Mnemonic.Std: SetFlag(Df); break;
				case Mnemonic.Clc: ClearFlag(Cf); break;
				case Mnemonic.Stc: SetFlag(Cf); break;
				case Mnemonic.Cli: ClearFlag(If); break;
				case Mnemonic.Sti: SetFlag(If); break;
				case Mnemonic.Cmc: SetFlagVal(Cf, !GetFlag(Cf)); break;
				case Mnemonic.Pushf:
					_esp -= 2;
					Write16(_esp, (ushort)_eflags);
					break;
				case Mnemonic.Popf:
					_eflags = (_eflags & 0xFFFF0000) | Read16(_esp);
					_esp += 2;
					break;
				case Mnemonic.Pushfd:
					_esp -= 4;
					Write32(_esp, _eflags);
					break;
				case Mnemonic.Popfd:
					_eflags = Read32(_esp);
					_esp += 4;
					break;
				case Mnemonic.Iret:
				case Mnemonic.Iretd:
					// IRET/IRETD - Interrupt Return
					// Pops EIP, CS (ignored in flat memory model), and EFLAGS from stack
					_eip = Read32(_esp);
					_esp += 4;
					// Skip CS (we don't use segmentation)
					_esp += 4;
					_eflags = Read32(_esp);
					_esp += 4;
					break;
				case Mnemonic.Lahf:
				{
					byte ah = 0;
					if (GetFlag(Sf))
					{
						ah |= 0x80;
					}

					if (GetFlag(Zf))
					{
						ah |= 0x40;
					}

					if (GetFlag(Af))
					{
						ah |= 0x10;
					}

					if (GetFlag(Pf))
					{
						ah |= 0x04;
					}

					ah |= 0x02;
					if (GetFlag(Cf))
					{
						ah |= 0x01;
					}

					_eax = (_eax & 0xFFFF00FF) | (uint)(ah << 8);
					break;
				}
				case Mnemonic.Sahf:
				{
					var sahf = (byte)((_eax >> 8) & 0xFF);
					SetFlagVal(Sf, (sahf & 0x80) != 0);
					SetFlagVal(Zf, (sahf & 0x40) != 0);
					SetFlagVal(Af, (sahf & 0x10) != 0);
					SetFlagVal(Pf, (sahf & 0x04) != 0);
					SetFlagVal(Cf, (sahf & 0x01) != 0);
					break;
				}
				// Legacy BCD (Binary Coded Decimal) instructions
				case Mnemonic.Aad: ExecAad(insn); break;
				case Mnemonic.Aam: ExecAam(insn); break;
				case Mnemonic.Aas: ExecAas(); break;
				case Mnemonic.Das: ExecDas(); break;
				case Mnemonic.Daa: ExecDaa(); break;
				// Protected mode / privileged instructions - no-op in flat memory model
				case Mnemonic.Sldt: ExecSldt(insn); break;
				case Mnemonic.Arpl: ExecArpl(insn); break;
				case Mnemonic.In: ExecIn(insn); break;
				case Mnemonic.Out: ExecOut(insn); break;
				case Mnemonic.Int:
					// Handle INT instruction with immediate
					if (insn.Immediate8 == 3)
					{
						// INT3 breakpoint - check if it's at a COM vtable address
						// Note: Import stubs and synthetic exports now use CALL/RET and syscall mechanism
						if (MemoryRegions.IsInComVtableRange(oldEip))
						{
							// This is a COM vtable method stub - signal this as a call
							isCall = true;
							callTarget = oldEip;
							_logger.LogInformation("[IcedCpu] INT 3 hooking COM vtable stub at address 0x{OldEip:X8}", oldEip);

							// Don't actually execute the INT3, just treat it as a call
							// The main loop will handle the COM method invocation
						}
						else
						{
							// Regular INT3 - for now, just print a message and continue
							_logger.LogWarning("[IcedCpu] INT3 breakpoint at 0x{OldEip:X8}", oldEip);
						}
					}
					else if (insn.Immediate8 == 0x80)
					{
						// INT 0x80 - Syscall dispatcher (retrowin32-style)
						// This is triggered when import stubs call the syscall dispatcher
						// We signal this as a syscall and let the emulator handle it
						isSyscall = true;
						_logger.LogDebug("[IcedCpu] INT 0x80 syscall at 0x{OldEip:X8}", oldEip);
						// Advance EIP past the INT instruction so the next instruction (RET) can execute
						// INT 0x80 is 2 bytes (CD 80), so advance by instruction length
						_eip = oldEip + (uint)insn.Length;
					}
					else
					{
						_logger.LogWarning("[IcedCpu] Unhandled interrupt INT {InsnImmediate8:X2} at 0x{OldEip:X8}", insn.Immediate8, oldEip);
					}

					break;
				case Mnemonic.Int3:
					// Handle INT3 (0xCC) instruction used for COM vtable methods
					// Note: Import stubs and synthetic exports now use CALL/RET and syscall mechanism
					if (MemoryRegions.IsInComVtableRange(oldEip))
					{
						// This is a COM vtable method stub - signal this as a call
						isCall = true;
						callTarget = oldEip;
						_logger.LogInformation("[IcedCpu] INT3 (0xCC) hooking COM vtable stub at address 0x{OldEip:X8}", oldEip);

						// Don't actually execute the INT3, just treat it as a call
						// The main loop will handle the COM method invocation
					}
					else
					{
						// Regular INT3 - for now, just print a message and continue
						_logger.LogWarning("[IcedCpu] INT3 breakpoint at 0x{OldEip:X8}", oldEip);
					}

					break;
				case Mnemonic.Into:
					// INTO - Interrupt if overflow flag is set
					if (GetFlag(Of))
					{
						// Overflow interrupt - for now, log and continue
						_logger.LogWarning("[IcedCpu] INTO overflow interrupt at 0x{OldEip:X8}", oldEip);
						// In a real implementation, this would trigger interrupt 4
						// For now, just advance EIP
						_eip = oldEip + (uint)insn.Length;
					}
					else
					{
						// No overflow, just advance
						_eip = oldEip + (uint)insn.Length;
					}
					break;
				case Mnemonic.Int1:
					// INT1 (0xF1) - Single-step interrupt / ICEBP
					_logger.LogWarning("[IcedCpu] INT1 single-step interrupt at 0x{OldEip:X8}", oldEip);
					// For now, just advance EIP
					_eip = oldEip + (uint)insn.Length;
					break;
				case Mnemonic.Jecxz:
					// JECXZ - Jump if ECX is zero
					if (_ecx == 0)
					{
						_eip = (uint)insn.NearBranchTarget;
					}
					else
					{
						_eip = oldEip + (uint)insn.Length;
					}
					break;
				case Mnemonic.Jcxz:
					// JCXZ - Jump if CX is zero
					if ((_ecx & 0xFFFF) == 0)
					{
						_eip = (uint)insn.NearBranchTarget;
					}
					else
					{
						_eip = oldEip + (uint)insn.Length;
					}
					break;
				case Mnemonic.Loop:
					// LOOP - Decrement ECX and jump if ECX != 0
					_ecx--;
					if (_ecx != 0)
					{
						_eip = (uint)insn.NearBranchTarget;
					}
					else
					{
						_eip = oldEip + (uint)insn.Length;
					}
					break;
				case Mnemonic.Loope:
					// LOOPE/LOOPZ - Decrement ECX and jump if ECX != 0 and ZF = 1
					_ecx--;
					if (_ecx != 0 && GetFlag(Zf))
					{
						_eip = (uint)insn.NearBranchTarget;
					}
					else
					{
						_eip = oldEip + (uint)insn.Length;
					}
					break;
				case Mnemonic.Loopne:
					// LOOPNE/LOOPNZ - Decrement ECX and jump if ECX != 0 and ZF = 0
					_ecx--;
					if (_ecx != 0 && !GetFlag(Zf))
					{
						_eip = (uint)insn.NearBranchTarget;
					}
					else
					{
						_eip = oldEip + (uint)insn.Length;
					}
					break;
				case Mnemonic.Hlt:
					// HLT - Halt instruction
					// Used in SingleStepTests to mark end of test sequence
					// EIP advancement is handled by the general logic below
					break;
				default:
					if (insn.Mnemonic.ToString().StartsWith('J'))
					{
						if (IsBranchTaken(insn.ConditionCode))
						{
							_eip = (uint)insn.NearBranchTarget;
						}
						else
						{
							_eip = oldEip + (uint)insn.Length;
						}
					}
					else
					{
						// Log extensive diagnostics for unhandled/invalid instructions
						// This helps identify code/data confusion and control flow issues
						_logger.LogError("[IcedCpu] Unhandled mnemonic {InsnMnemonic} at 0x{OldEip:X8}, ESP=0x{Esp:X8}, EBP=0x{Ebp:X8}, EAX=0x{Eax:X8}. Likely executing data as code or invalid jump target.", 
							insn.Mnemonic, oldEip, _esp, _ebp, _eax);
						
						// If this is INVALID mnemonic, it's particularly bad - halt execution
						if (insn.Mnemonic == Mnemonic.INVALID)
						{
							_logger.LogError("[IcedCpu] INVALID instruction encountered - definitely not valid code. Check for corrupted return addresses or bad jumps.");
							
							// Provide additional context about the address range to help debugging
							string addressInfo;
							if (oldEip == 0)
							{
								addressInfo = "NULL pointer - likely corrupted function pointer";
							}
							else if (oldEip < 0x00010000)
							{
								addressInfo = "NULL page/guard pages - likely NULL pointer dereference";
							}
							else if (oldEip >= 0x00400000 && oldEip < 0x00401000)
							{
								addressInfo = "PE header region - code should start later (e.g., 0x00401000+). This suggests a corrupted return address on the stack, a bad function pointer in a jump table, or a buffer overflow.";
							}
							else if (oldEip < 0x00400000)
							{
								addressInfo = $"below typical image base (0x00400000) - likely stack/heap/data being executed as code";
							}
							else
							{
								addressInfo = "unknown region - could be data section, uninitialized memory, or beyond loaded code";
							}
							
							// Dump stack for debugging before throwing exception
							try
							{
								var stackDump = new System.Text.StringBuilder();
								stackDump.AppendLine($"[IcedCpu] Stack dump at INVALID instruction (ESP=0x{_esp:X8}):");
								for (int i = -2; i <= 10; i++)
								{
									var addr = _esp + (uint)(i * 4);
									if (addr < 0x10000 || addr >= mem.Size) continue;
									try
									{
										var val = mem.Read32(addr);
										var label = i == 0 ? " (ESP)" : i < 0 ? $" (ESP{i * 4:+0;-0})" : $" (ESP+{i * 4})";
										stackDump.AppendLine($"  [0x{addr:X8}] = 0x{val:X8}{label}");
									}
									catch (Exception ex)
									{
										_logger.LogDebug(ex, "[IcedCpu] Failed to read stack value at 0x{Addr:X8} during stack dump", addr);
									}
								}
								_logger.LogError(stackDump.ToString());
							}
							catch (Exception ex)
							{
								_logger.LogError(ex, "[IcedCpu] Exception occurred while dumping stack for INVALID instruction at 0x{OldEip:X8}", oldEip);
							}
							
							// Throw exception to halt execution and prevent further corruption
							// This prevents the CPU from continuing to execute random data as code,
							// which would lead to cascading errors and stack corruption
							throw new InvalidOperationException($"INVALID instruction at 0x{oldEip:X8} ({addressInfo}). This indicates execution has jumped to invalid memory. Common causes: (1) corrupted return address on stack (check for stack overflow/underflow), (2) uninitialized or corrupted function pointer, (3) bad indirect jump/call, (4) buffer overflow corrupting code pointers. ESP=0x{_esp:X8}, EBP=0x{_ebp:X8}");
						}
					}

					break;
			}
		}
		finally
		{
			Diagnostics.Diagnostics.ClearCpuContext();
		}

		// Advance EIP for non-control-flow instructions
		// Control-flow instructions (JMP, CALL, RET, Jcc, etc.) set _eip explicitly during execution
		// For all other instructions, advance EIP by the instruction length
		var isControlFlowInstruction = insn.Mnemonic switch
		{
			Mnemonic.Jmp => true,
			Mnemonic.Call => true,
			Mnemonic.Ret => true,
			Mnemonic.Retf => true,
			Mnemonic.Iret => true,
			Mnemonic.Iretd => true,
			Mnemonic.Iretq => true,
			Mnemonic.Loop => true,
			Mnemonic.Loope => true,
			Mnemonic.Loopne => true,
			// Jump if ECX/CX is zero
			Mnemonic.Jecxz => true,
			Mnemonic.Jcxz => true,
			// All conditional jumps
			Mnemonic.Ja => true,
			Mnemonic.Jae => true,
			Mnemonic.Jb => true,
			Mnemonic.Jbe => true,
			Mnemonic.Je => true,
			Mnemonic.Jg => true,
			Mnemonic.Jge => true,
			Mnemonic.Jl => true,
			Mnemonic.Jle => true,
			Mnemonic.Jne => true,
			Mnemonic.Jno => true,
			Mnemonic.Jnp => true,
			Mnemonic.Jns => true,
			Mnemonic.Jo => true,
			Mnemonic.Jp => true,
			Mnemonic.Js => true,
			// INT and INT3 can transfer control in some cases
			Mnemonic.Int => true,
			Mnemonic.Int3 => true,
			Mnemonic.Int1 => true,
			Mnemonic.Into => true,
			_ => false
		};

		// For non-control-flow instructions, advance EIP by instruction length
		if (!isControlFlowInstruction)
		{
			// Use decoder IP directly as it's authoritative for instruction length
			// Store the full 32-bit value of EIP even in 16-bit mode
			// The wrapping to 16-bit happens during instruction fetch (via segment:offset addressing),
			// not when storing EIP. This matches real 386 hardware behavior in real mode.
			_eip = (uint)_decoder.IP;
		}

		return new CpuStepResult(isCall, callTarget, isSyscall);
	}

	/// <summary>
	/// Generates a CPU exception by simulating hardware exception behavior.
	/// Supports real mode (16-bit) exception handling with proper IVT lookup.
	/// Protected mode implementation is simplified for testing purposes.
	/// </summary>
	/// <param name="vector">Interrupt vector number (0-255). Currently supports #UD (vector 6) and other basic exceptions.</param>
	/// <param name="faultingEip">The EIP of the instruction that caused the exception</param>
	/// <param name="mem">Memory interface for stack operations</param>
	/// <remarks>
	/// Real mode: Pushes FLAGS, CS, IP to stack, clears IF/TF, jumps to IVT entry.
	/// Protected mode: Simplified implementation - does not fully parse IDT descriptors.
	/// Error codes are not pushed (correct for #UD, but may need adjustment for other exceptions).
	/// </remarks>
	private void GenerateException(int vector, uint faultingEip, VirtualMemory mem)
	{
		// Exception handling in real mode (16-bit):
		// 1. Push FLAGS (2 bytes)
		// 2. Push CS (2 bytes)
		// 3. Push IP (2 bytes)
		// 4. Clear IF and TF flags
		// 5. Load CS:IP from interrupt vector table at address (vector * 4)
		
		if (_bitness == 16)
		{
			// Validate vector is in valid range
			if (vector < 0 || vector > 255)
			{
				_logger.LogError("Invalid exception vector {Vector}", vector);
				return;
			}
			
			// Real mode exception handling with bounds checking
			// Push FLAGS (16-bit)
			var newSp = (ushort)((_esp & 0xFFFF) - 2);
			_esp = (_esp & 0xFFFF0000) | newSp;
			var flagsAddr = (uint)((_ss << 4) + newSp);
			
			// Validate stack address is within memory bounds
			if (flagsAddr >= mem.Size || flagsAddr + 1 >= mem.Size)
			{
				_logger.LogError("Stack overflow during exception handling at FLAGS push: addr=0x{Address:X8}", flagsAddr);
				return;
			}
			mem.Write16(flagsAddr, (ushort)_eflags);
			
			// Push CS (16-bit)
			newSp = (ushort)(newSp - 2);
			_esp = (_esp & 0xFFFF0000) | newSp;
			var csAddr = (uint)((_ss << 4) + newSp);
			
			if (csAddr >= mem.Size || csAddr + 1 >= mem.Size)
			{
				_logger.LogError("Stack overflow during exception handling at CS push: addr=0x{Address:X8}", csAddr);
				return;
			}
			mem.Write16(csAddr, _cs);
			
			// Push IP (16-bit) - the address of the faulting instruction
			newSp = (ushort)(newSp - 2);
			_esp = (_esp & 0xFFFF0000) | newSp;
			var ipAddr = (uint)((_ss << 4) + newSp);
			
			if (ipAddr >= mem.Size || ipAddr + 1 >= mem.Size)
			{
				_logger.LogError("Stack overflow during exception handling at IP push: addr=0x{Address:X8}", ipAddr);
				return;
			}
			mem.Write16(ipAddr, (ushort)(faultingEip & 0xFFFF));
			
			// Clear IF (bit 9) and TF (bit 8) in EFLAGS
			_eflags &= ~(1u << 9); // Clear IF
			_eflags &= ~(1u << 8); // Clear TF
			
			// Load new CS:IP from interrupt vector table
			// IVT entry is at address (vector * 4): [IP:2bytes][CS:2bytes]
			// IVT is in the first 1KB of memory (vectors 0-255, 4 bytes each)
			var ivtAddr = (uint)(vector * 4);
			
			// Validate IVT address is within bounds (should be 0-1023)
			if (ivtAddr + 3 >= mem.Size)
			{
				_logger.LogError("IVT address out of bounds: vector={Vector}, addr=0x{Address:X8}", vector, ivtAddr);
				return;
			}
			
			var newIp = mem.Read16(ivtAddr);
			var newCs = mem.Read16(ivtAddr + 2);
			
			_eip = newIp;
			_cs = newCs;
		}
		else
		{
			// 32-bit protected mode exception handling
			// NOTE: This is a SIMPLIFIED implementation for testing purposes.
			// Real protected mode uses IDT (Interrupt Descriptor Table) with 8-byte descriptors
			// containing segment selectors, offsets, and access rights that must be properly parsed.
			// This implementation does not handle:
			// - IDT descriptor parsing
			// - Task gates
			// - Privilege level transitions
			// - Error code pushing for some exceptions (though #UD doesn't push error codes)
			
			// Push EFLAGS (32-bit)
			_esp -= 4;
			if (_esp >= mem.Size || _esp + 3 >= mem.Size)
			{
				_logger.LogError("Stack overflow during protected mode exception at EFLAGS push");
				return;
			}
			mem.Write32(_esp, _eflags);
			
			// Push CS (32-bit)
			_esp -= 4;
			if (_esp >= mem.Size || _esp + 3 >= mem.Size)
			{
				_logger.LogError("Stack overflow during protected mode exception at CS push");
				return;
			}
			mem.Write32(_esp, _cs);
			
			// Push EIP (32-bit) - the address of the faulting instruction
			_esp -= 4;
			if (_esp >= mem.Size || _esp + 3 >= mem.Size)
			{
				_logger.LogError("Stack overflow during protected mode exception at EIP push");
				return;
			}
			mem.Write32(_esp, faultingEip);
			
			// Clear IF (bit 9) and TF (bit 8) in EFLAGS
			_eflags &= ~(1u << 9); // Clear IF
			_eflags &= ~(1u << 8); // Clear TF
			
			// Simplified: Load from IVT-style table (not real IDT parsing)
			// In a full implementation, this would parse the IDT descriptor
			var ivtAddr = (uint)(vector * 4);
			if (ivtAddr + 3 >= mem.Size)
			{
				_logger.LogError("Exception vector address out of bounds in protected mode");
				return;
			}
			_eip = mem.Read32(ivtAddr);
		}
	}

	/// <summary>
	/// Validates an indirect jump/call target and throws an exception if it points to an invalid region.
	/// Addresses below the image base (from PE header, typically 0x00400000) are considered invalid,
	/// except for NULL and special emulator ranges. This indicates possible invalid function pointers,
	/// corrupted registers, or uninitialized memory being executed as code.
	/// Stack and low heap addresses are especially problematic as they indicate function pointers
	/// pointing to data rather than code.
	/// </summary>
	/// <param name="target">The target address to validate</param>
	/// <param name="sourceEip">The EIP of the instruction performing the jump/call</param>
	/// <param name="operation">The operation type ("JMP" or "CALL")</param>
	/// <param name="sourceRegister">Optional source register for better diagnostics</param>
	/// <exception cref="InvalidOperationException">Thrown when target points to an invalid memory region</exception>
	private void ValidateIndirectTarget(uint target, uint sourceEip, string operation, Register? sourceRegister = null)
	{
		// Allow NULL (0x00000000) to avoid false positives
		if (target == 0x00000000)
		{
			return;
		}
		
		// Allow special emulator ranges: COM vtables, syscalls, and import hooks
		if (MemoryRegions.IsInSpecialRange(target))
		{
			// Valid special range - no validation needed
			return;
		}
		
		// Check if target is suspiciously low (< image base from PE header)
		// The image base can vary based on the PE header (typically 0x00400000 for executables,
		// but can be different for DLLs or executables with custom image bases)
		if (target < _imageBase)
		{
			// Determine the type of invalid address for better error messaging
			string addressType;
			string diagnosticInfo;
			
			// Check if target is within the stack region (from PE header)
			// Stack grows downward from _stackBase to _stackLimit
			var isStackAddress = target >= _stackLimit && target < _stackBase;
			if (isStackAddress)
			{
				// Stack region
				addressType = "stack";
				diagnosticInfo = GetStackAddressDiagnostic();
			}
			else
			{
				// Other low address (< stack region or >= stack region but < image base)
				addressType = "low memory";
				diagnosticInfo = $"This indicates an invalid or uninitialized function pointer. " +
				                $"The address is below the image base (0x{_imageBase:X8} from PE header).";
			}
			
			string errorMessage;
			if (sourceRegister.HasValue)
			{
				var regName = sourceRegister.Value.ToString();
				
				// Try to provide additional context by checking if the source might be an IAT entry
				var debugHint = string.Empty;
				
				// Check if this might be loading from an IAT entry (common pattern: mov reg,[iat_addr]; call reg)
				if (isStackAddress)
				{
					debugHint = " DEBUGGING: Check if the IAT entry that loaded this register was properly initialized. " +
					           $"Register {regName} was loaded from memory before this CALL instruction.";
				}
				
				errorMessage = $"Invalid indirect {operation} at 0x{sourceEip:X8}: " +
				              $"Target address 0x{target:X8} (from register {regName}) points to {addressType} instead of code. " +
				              $"{diagnosticInfo}{debugHint}";
				
				_logger.LogError("[IcedCpu] {ErrorMessage}", errorMessage);
			}
			else
			{
				errorMessage = $"Invalid indirect {operation} at 0x{sourceEip:X8}: " +
				              $"Target address 0x{target:X8} (from memory) points to {addressType} instead of code. " +
				              $"{diagnosticInfo}";
				
				_logger.LogError("[IcedCpu] {ErrorMessage}", errorMessage);
			}
			
			throw new InvalidOperationException(errorMessage);
		}
	}
	
	/// <summary>
	/// Gets diagnostic information for stack address validation failures.
	/// </summary>
	private static string GetStackAddressDiagnostic()
	{
		return "This indicates a function pointer was loaded with a stack address instead of a code address. " +
		       "Common causes: (1) Uninitialized function pointer in .data/.bss section, " +
		       "(2) Corruption of Import Address Table (IAT) entry, " +
		       "(3) Missing C runtime initialization, " +
		       "(4) Buffer overflow corrupting function pointers.";
	}

	#region Exec helpers

	private void ExecMov(Instruction insn)
	{
		if (insn.OpCount < 2)
		{
			return;
		}

		// Determine the operand size
		var opSize = GetOpSizeBits(insn, 0);

		switch (opSize)
		{
			case 8:
			{
				// 8-bit MOV
				byte value;
				if (insn.GetOpKind(1) == OpKind.Register)
				{
					value = GetReg8(insn.GetOpRegister(1));
				}
				else if (insn.GetOpKind(1) == OpKind.Memory)
				{
					value = _mem.Read8(CalcMemAddress(insn));
				}
				else if (insn.GetOpKind(1) == OpKind.Immediate8)
				{
					value = insn.Immediate8;
				}
				else
				{
					value = (byte)ReadOp(insn, 1);
				}

				// Write the 8-bit value
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					SetReg8(insn.GetOpRegister(0), value);
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					_mem.Write8(CalcMemAddress(insn), value);
				}
				break;
			}
			case 16:
			{
				// 16-bit MOV
				ushort value;
				if (insn.GetOpKind(1) == OpKind.Register)
				{
					value = GetReg16(insn.GetOpRegister(1));
				}
				else if (insn.GetOpKind(1) == OpKind.Memory)
				{
					value = _mem.Read16(CalcMemAddress(insn));
				}
				else
				{
					value = (ushort)ReadOp(insn, 1);
				}

				// Write the 16-bit value
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					SetReg16(insn.GetOpRegister(0), value);
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					_mem.Write16(CalcMemAddress(insn), value);
				}
				break;
			}
			default:
			{
				// 32-bit MOV (default behavior)
				var src = ReadOp(insn, 1);
				WriteOp(insn, 0, src);
				break;
			}
		}
	}

	private void ExecLea(Instruction insn)
	{
		if (insn.OpCount < 2)
		{
			return;
		}

		var addr = CalcLeaAddress(insn);
		if (insn.GetOpKind(0) == OpKind.Register)
		{
			SetReg32(insn.GetOpRegister(0), addr);
		}
		else
		{
			WriteOp(insn, 0, addr);
		}
	}

	private void ExecMovx(Instruction insn, bool signExtend)
	{
		uint value;
		var srcBits = GetSourceSizeBits(insn);
		if (insn.GetOpKind(1) == OpKind.Memory)
		{
			var a = CalcMemAddress(insn);
			value = srcBits == 8 ? _mem.Read8(a) : _mem.Read16(a);
		}
		else
		{
			var r = insn.GetOpRegister(1);
			value = srcBits == 8 ? GetReg8(r) : GetReg16(r);
		}

		uint result;
		if (signExtend)
		{
			result = srcBits == 8 ? (uint)(sbyte)(byte)value : (uint)(short)(ushort)value;
		}
		else
		{
			result = srcBits == 8 ? (byte)value : (uint)(ushort)value;
		}

		WriteOp(insn, 0, result);
	}

	private void ExecPush(Instruction insn)
	{
		// Determine operand size - in 16-bit mode, default is 16-bit
		// with o32 prefix making it 32-bit. In 32-bit mode, vice versa.
		// Note: PUSH never operates on 8-bit values (even PUSH imm8 sign-extends to 16/32 bits)
		var opSize = GetOpSizeBits(insn, 0);
		var val = ReadOp(insn, 0);
		
		switch (opSize)
		{
			case 16:
				AdjustStackPointer(-2);
				var stackAddr16 = GetStackAddress();
				_mem.Write16(stackAddr16, (ushort)val);
				break;
			case 32:
				Push32(val);
				break;
			default:
				throw new InvalidOperationException($"PUSH instruction does not support {opSize}-bit operand size at EIP=0x{_eip:X8}");
		}
	}

	private void ExecPop(Instruction insn)
	{
		// Note: POP never operates on 8-bit values
		var opSize = GetOpSizeBits(insn, 0);
		uint v;
		
		switch (opSize)
		{
			case 16:
				var stackAddr16 = GetStackAddress();
				v = _mem.Read16(stackAddr16);
				AdjustStackPointer(2);
				break;
			case 32:
				v = Pop32();
				break;
			default:
				throw new InvalidOperationException($"POP instruction does not support {opSize}-bit operand size at EIP=0x{_eip:X8}");
		}
		
		WriteOp(insn, 0, v);
	}

	private void ExecPushad()
	{
		var oldEsp = _esp;
		Push32(_eax);
		Push32(_ecx);
		Push32(_edx);
		Push32(_ebx);
		Push32(oldEsp);
		Push32(_ebp);
		Push32(_esi);
		Push32(_edi);
	}

	private void ExecPopad()
	{
		_edi = Pop32();
		_esi = Pop32();
		var ebpValue = Pop32();
		if (_eip >= 0x00403180 && _eip <= 0x004031A0)
		{
			_logger.LogWarning("[IcedCpu] ExecPopad: EBP=0x{Value:X8}, EIP=0x{Eip:X8}, ESP=0x{Esp:X8}",
				ebpValue, _eip, _esp);
		}
		_ebp = ebpValue;
		_ = Pop32();
		_ebx = Pop32();
		_edx = Pop32();
		_ecx = Pop32();
		_eax = Pop32();
	}

	private void ExecAdd(Instruction insn)
	{
		var opSize = GetOpSizeBits(insn, 0);
		
		switch (opSize)
		{
			case 8:
			{
				// 8-bit ADD
				byte a, b;
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					a = GetReg8(insn.GetOpRegister(0));
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					a = _mem.Read8(CalcMemAddress(insn));
				}
				else
				{
					a = (byte)ReadOp(insn, 0);
				}
				
				if (insn.GetOpKind(1) == OpKind.Register)
				{
					b = GetReg8(insn.GetOpRegister(1));
				}
				else if (insn.GetOpKind(1) == OpKind.Memory)
				{
					b = _mem.Read8(CalcMemAddress(insn));
				}
				else if (insn.GetOpKind(1) == OpKind.Immediate8)
				{
					b = insn.Immediate8;
				}
				else if (insn.GetOpKind(1) == OpKind.Immediate16)
				{
					b = (byte)insn.Immediate16;
				}
				else
				{
					b = (byte)ReadOp(insn, 1);
				}
				
				var r = (byte)(a + b);
				
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					SetReg8(insn.GetOpRegister(0), r);
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					_mem.Write8(CalcMemAddress(insn), r);
				}
				
				SetFlagsAdd(a, b, r, 0x80); // 8-bit sign bit
				break;
			}
			case 16:
			{
				// 16-bit ADD
				ushort a, b;
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					a = GetReg16(insn.GetOpRegister(0));
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					a = SafeRead16(insn, CalcMemAddress(insn));
				}
				else
				{
					a = (ushort)ReadOp(insn, 0);
				}
				
				if (insn.GetOpKind(1) == OpKind.Register)
				{
					b = GetReg16(insn.GetOpRegister(1));
				}
				else if (insn.GetOpKind(1) == OpKind.Memory)
				{
					b = SafeRead16(insn, CalcMemAddress(insn));
				}
				else
				{
					b = (ushort)ReadOp(insn, 1);
				}
				
				var r = (ushort)(a + b);
				
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					SetReg16(insn.GetOpRegister(0), r);
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					SafeWrite16(insn, CalcMemAddress(insn), r);
				}
				
				SetFlagsAdd(a, b, r, 0x8000); // 16-bit sign bit
				break;
			}
			default:
			{
				// 32-bit ADD (default behavior)
				uint a = ReadOp(insn, 0), b = ReadOp(insn, 1), r = a + b;
				WriteOp(insn, 0, r);
				SetFlagsAdd(a, b, r);
				break;
			}
		}
	}

	private void ExecAdc(Instruction insn)
	{
		var opSize = GetOpSizeBits(insn, 0);
		
		switch (opSize)
		{
			case 8:
			{
				byte a = (insn.GetOpKind(0) == OpKind.Register) ? GetReg8(insn.GetOpRegister(0)) :
				         (insn.GetOpKind(0) == OpKind.Memory) ? _mem.Read8(CalcMemAddress(insn)) : (byte)ReadOp(insn, 0);
				byte b = (insn.GetOpKind(1) == OpKind.Register) ? GetReg8(insn.GetOpRegister(1)) :
				         (insn.GetOpKind(1) == OpKind.Memory) ? _mem.Read8(CalcMemAddress(insn)) :
				         (insn.GetOpKind(1) == OpKind.Immediate8) ? insn.Immediate8 : (byte)ReadOp(insn, 1);
				var cf = GetFlag(Cf) ? (byte)1 : (byte)0;
				var sum = (ushort)(a + b + cf);
				byte r = (byte)sum;
				
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					SetReg8(insn.GetOpRegister(0), r);
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					_mem.Write8(CalcMemAddress(insn), r);
				}

				SetFlagVal(Cf, (sum >> 8) != 0);
				SetFlagVal(Of, (~(a ^ b) & (a ^ r) & 0x80) != 0);
				SetFlagVal(Af, ((a ^ b ^ r) & 0x10) != 0);
				UpdateLogicResultFlags(r, 0x80);
				break;
			}
			case 16:
			{
				ushort a = (insn.GetOpKind(0) == OpKind.Register) ? GetReg16(insn.GetOpRegister(0)) :
				           (insn.GetOpKind(0) == OpKind.Memory) ? _mem.Read16(CalcMemAddress(insn)) : (ushort)ReadOp(insn, 0);
				ushort b = (insn.GetOpKind(1) == OpKind.Register) ? GetReg16(insn.GetOpRegister(1)) :
				           (insn.GetOpKind(1) == OpKind.Memory) ? _mem.Read16(CalcMemAddress(insn)) : (ushort)ReadOp(insn, 1);
				var cf = GetFlag(Cf) ? (ushort)1 : (ushort)0;
				var sum = (uint)(a + b + cf);
				ushort r = (ushort)sum;
				
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					SetReg16(insn.GetOpRegister(0), r);
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					_mem.Write16(CalcMemAddress(insn), r);
				}

				SetFlagVal(Cf, (sum >> 16) != 0);
				SetFlagVal(Of, (~(a ^ b) & (a ^ r) & 0x8000) != 0);
				SetFlagVal(Af, ((a ^ b ^ r) & 0x10) != 0);
				UpdateLogicResultFlags(r, 0x8000);
				break;
			}
			default:
			{
				uint a = ReadOp(insn, 0), b = ReadOp(insn, 1);
				var cf = GetFlag(Cf) ? 1u : 0u;
				var sum = (ulong)a + b + cf;
				var r = (uint)sum;
				WriteOp(insn, 0, r);
				SetFlagVal(Cf, (sum >> 32) != 0);
				SetFlagVal(Of, (~(a ^ b) & (a ^ r) & 0x80000000) != 0);
				SetFlagVal(Af, (((a ^ b ^ r) & 0x10) != 0));
				UpdateLogicResultFlags(r);
				break;
			}
		}
	}

	private void ExecSub(Instruction insn)
	{
		var opSize = GetOpSizeBits(insn, 0);
		
		switch (opSize)
		{
			case 8:
			{
				byte a = (insn.GetOpKind(0) == OpKind.Register) ? GetReg8(insn.GetOpRegister(0)) :
				         (insn.GetOpKind(0) == OpKind.Memory) ? _mem.Read8(CalcMemAddress(insn)) : (byte)ReadOp(insn, 0);
				byte b = (insn.GetOpKind(1) == OpKind.Register) ? GetReg8(insn.GetOpRegister(1)) :
				         (insn.GetOpKind(1) == OpKind.Memory) ? _mem.Read8(CalcMemAddress(insn)) :
				         (insn.GetOpKind(1) == OpKind.Immediate8) ? insn.Immediate8 : (byte)ReadOp(insn, 1);
				byte r = (byte)(a - b);
				
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					SetReg8(insn.GetOpRegister(0), r);
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					_mem.Write8(CalcMemAddress(insn), r);
				}

				SetFlagsSub(a, b, r, 0x80);
				break;
			}
			case 16:
			{
				ushort a = (insn.GetOpKind(0) == OpKind.Register) ? GetReg16(insn.GetOpRegister(0)) :
				           (insn.GetOpKind(0) == OpKind.Memory) ? _mem.Read16(CalcMemAddress(insn)) : (ushort)ReadOp(insn, 0);
				ushort b = (insn.GetOpKind(1) == OpKind.Register) ? GetReg16(insn.GetOpRegister(1)) :
				           (insn.GetOpKind(1) == OpKind.Memory) ? _mem.Read16(CalcMemAddress(insn)) : (ushort)ReadOp(insn, 1);
				ushort r = (ushort)(a - b);
				
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					SetReg16(insn.GetOpRegister(0), r);
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					_mem.Write16(CalcMemAddress(insn), r);
				}

				SetFlagsSub(a, b, r, 0x8000);
				break;
			}
			default:
			{
				uint a = ReadOp(insn, 0), b = ReadOp(insn, 1), r = a - b;
				WriteOp(insn, 0, r);
				SetFlagsSub(a, b, r);
				break;
			}
		}
	}

	private void ExecSbb(Instruction insn)
	{
		var opSize = GetOpSizeBits(insn, 0);
		
		switch (opSize)
		{
			case 8:
			{
				// 8-bit SBB
				byte a, b;
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					a = GetReg8(insn.GetOpRegister(0));
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					a = _mem.Read8(CalcMemAddress(insn));
				}
				else
				{
					a = (byte)ReadOp(insn, 0);
				}
				
				if (insn.GetOpKind(1) == OpKind.Register)
				{
					b = GetReg8(insn.GetOpRegister(1));
				}
				else if (insn.GetOpKind(1) == OpKind.Memory)
				{
					b = _mem.Read8(CalcMemAddress(insn));
				}
				else if (insn.GetOpKind(1) == OpKind.Immediate8)
				{
					b = insn.Immediate8;
				}
				else if (insn.GetOpKind(1) == OpKind.Immediate16)
				{
					b = (byte)insn.Immediate16;
				}
				else
				{
					b = (byte)ReadOp(insn, 1);
				}
				
				var cf = GetFlag(Cf) ? (byte)1 : (byte)0;
				var r = (byte)(a - b - cf);
				
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					SetReg8(insn.GetOpRegister(0), r);
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					_mem.Write8(CalcMemAddress(insn), r);
				}
				
				SetFlagsSbb(a, b, r, 0x80, cf != 0); // 8-bit sign bit; pass cf separately to avoid overflow
				break;
			}
			case 16:
			{
				// 16-bit SBB
				ushort a, b;
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					a = GetReg16(insn.GetOpRegister(0));
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					a = _mem.Read16(CalcMemAddress(insn));
				}
				else
				{
					a = (ushort)ReadOp(insn, 0);
				}
				
				if (insn.GetOpKind(1) == OpKind.Register)
				{
					b = GetReg16(insn.GetOpRegister(1));
				}
				else if (insn.GetOpKind(1) == OpKind.Memory)
				{
					b = _mem.Read16(CalcMemAddress(insn));
				}
				else
				{
					b = (ushort)ReadOp(insn, 1);
				}
				
				var cf = GetFlag(Cf) ? (ushort)1 : (ushort)0;
				var r = (ushort)(a - b - cf);
				
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					SetReg16(insn.GetOpRegister(0), r);
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					_mem.Write16(CalcMemAddress(insn), r);
				}
				
				SetFlagsSbb(a, b, r, 0x8000, cf != 0); // 16-bit sign bit; pass cf separately to avoid overflow
				break;
			}
			default:
			{
				// 32-bit SBB (default behavior)
				uint a = ReadOp(insn, 0), b = ReadOp(insn, 1);
				var cf = GetFlag(Cf) ? 1u : 0u;
				var r = a - b - cf;
				WriteOp(insn, 0, r);
				SetFlagsSbb(a, b + cf, r);
				break;
			}
		}
	}
	
	private void SetFlagsSbb(uint a, uint b, uint r)
	{
		SetFlagsSbb(a, b, r, 0x80000000);
	}
	
	private void SetFlagsSbb(uint a, uint b, uint r, uint signBitMask)
	{
		SetFlagVal(Cf, a < b);
		SetFlagVal(Of, ((a ^ b) & (a ^ r) & signBitMask) != 0);
		SetFlagVal(Af, ((a ^ b ^ r) & 0x10) != 0);
		UpdateLogicResultFlags(r, signBitMask);
	}
	
	private void SetFlagsSbb(uint a, uint b, uint r, uint signBitMask, bool cfIn)
	{
		SetFlagVal(Cf, cfIn ? a <= b : a < b);
		SetFlagVal(Of, ((a ^ b) & (a ^ r) & signBitMask) != 0);
		SetFlagVal(Af, ((a ^ b ^ r) & 0x10) != 0);
		UpdateLogicResultFlags(r, signBitMask);
	}

	private void ExecXor(Instruction insn)
	{
		var opSize = GetOpSizeBits(insn, 0);
		
		switch (opSize)
		{
			case 8:
			{
				byte a = (insn.GetOpKind(0) == OpKind.Register) ? GetReg8(insn.GetOpRegister(0)) :
				         (insn.GetOpKind(0) == OpKind.Memory) ? _mem.Read8(CalcMemAddress(insn)) : (byte)ReadOp(insn, 0);
				byte b = (insn.GetOpKind(1) == OpKind.Register) ? GetReg8(insn.GetOpRegister(1)) :
				         (insn.GetOpKind(1) == OpKind.Memory) ? _mem.Read8(CalcMemAddress(insn)) :
				         (insn.GetOpKind(1) == OpKind.Immediate8) ? insn.Immediate8 : (byte)ReadOp(insn, 1);
				byte r = (byte)(a ^ b);
				
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					SetReg8(insn.GetOpRegister(0), r);
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					_mem.Write8(CalcMemAddress(insn), r);
				}

				ClearFlag(Cf);
				ClearFlag(Of);
				ClearFlag(Af);
				UpdateLogicResultFlags(r, 0x80);
				break;
			}
			case 16:
			{
				ushort a = (insn.GetOpKind(0) == OpKind.Register) ? GetReg16(insn.GetOpRegister(0)) :
				           (insn.GetOpKind(0) == OpKind.Memory) ? _mem.Read16(CalcMemAddress(insn)) : (ushort)ReadOp(insn, 0);
				ushort b = (insn.GetOpKind(1) == OpKind.Register) ? GetReg16(insn.GetOpRegister(1)) :
				           (insn.GetOpKind(1) == OpKind.Memory) ? _mem.Read16(CalcMemAddress(insn)) : (ushort)ReadOp(insn, 1);
				ushort r = (ushort)(a ^ b);
				
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					SetReg16(insn.GetOpRegister(0), r);
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					_mem.Write16(CalcMemAddress(insn), r);
				}

				ClearFlag(Cf);
				ClearFlag(Of);
				ClearFlag(Af);
				UpdateLogicResultFlags(r, 0x8000);
				break;
			}
			default:
			{
				var r = ReadOp(insn, 0) ^ ReadOp(insn, 1);
				WriteOp(insn, 0, r);
				ClearFlag(Cf);
				ClearFlag(Of);
				ClearFlag(Af);
				UpdateLogicResultFlags(r);
				break;
			}
		}
	}

	private void ExecLogic(Instruction insn, LogicOp op)
	{
		// Determine the operand size
		var opSize = GetOpSizeBits(insn, 0);

		switch (opSize)
		{
			case 8:
			{
				// 8-bit logic operation
				byte a, b;
				
				// Read first operand
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					a = GetReg8(insn.GetOpRegister(0));
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					a = _mem.Read8(CalcMemAddress(insn));
				}
				else
				{
					a = (byte)ReadOp(insn, 0);
				}
				
				// Read second operand
				if (insn.GetOpKind(1) == OpKind.Register)
				{
					b = GetReg8(insn.GetOpRegister(1));
				}
				else if (insn.GetOpKind(1) == OpKind.Memory)
				{
					b = _mem.Read8(CalcMemAddress(insn));
				}
				else
				{
					b = (byte)ReadOp(insn, 1);
				}
				
				var r = op == LogicOp.And ? (byte)(a & b) : (byte)(a | b);

				// Write the 8-bit result
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					SetReg8(insn.GetOpRegister(0), r);
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					_mem.Write8(CalcMemAddress(insn), r);
				}

				ClearFlag(Cf);
				ClearFlag(Of);
				ClearFlag(Af);
				UpdateLogicResultFlags(r, 0x80);
				break;
			}
			case 16:
			{
				// 16-bit logic operation
				ushort a, b;
				
				// Read first operand
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					a = GetReg16(insn.GetOpRegister(0));
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					a = _mem.Read16(CalcMemAddress(insn));
				}
				else
				{
					a = (ushort)ReadOp(insn, 0);
				}
				
				// Read second operand
				if (insn.GetOpKind(1) == OpKind.Register)
				{
					b = GetReg16(insn.GetOpRegister(1));
				}
				else if (insn.GetOpKind(1) == OpKind.Memory)
				{
					b = _mem.Read16(CalcMemAddress(insn));
				}
				else
				{
					b = (ushort)ReadOp(insn, 1);
				}
				
				var r = op == LogicOp.And ? (ushort)(a & b) : (ushort)(a | b);

				// Write the 16-bit result
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					SetReg16(insn.GetOpRegister(0), r);
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					_mem.Write16(CalcMemAddress(insn), r);
				}

				ClearFlag(Cf);
				ClearFlag(Of);
				ClearFlag(Af);
				UpdateLogicResultFlags(r, 0x8000);
				break;
			}
			default:
			{
				// 32-bit logic operation (default)
				uint a = ReadOp(insn, 0), b = ReadOp(insn, 1), r = op == LogicOp.And ? a & b : a | b;
				WriteOp(insn, 0, r);
				ClearFlag(Cf);
				ClearFlag(Of);
				ClearFlag(Af);
				UpdateLogicResultFlags(r);
				break;
			}
		}
	}

	private void ExecTest(Instruction insn)
	{
		var opSize = GetOpSizeBits(insn, 0);
		
		switch (opSize)
		{
			case 8:
			{
				// 8-bit TEST
				byte a, b;
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					a = GetReg8(insn.GetOpRegister(0));
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					a = _mem.Read8(CalcMemAddress(insn));
				}
				else
				{
					a = (byte)ReadOp(insn, 0);
				}
				
				if (insn.GetOpKind(1) == OpKind.Register)
				{
					b = GetReg8(insn.GetOpRegister(1));
				}
				else if (insn.GetOpKind(1) == OpKind.Memory)
				{
					b = _mem.Read8(CalcMemAddress(insn));
				}
				else if (insn.GetOpKind(1) == OpKind.Immediate8)
				{
					b = insn.Immediate8;
				}
				else
				{
					b = (byte)ReadOp(insn, 1);
				}
				
				var r = (byte)(a & b);
				
				ClearFlag(Cf);
				ClearFlag(Of);
				ClearFlag(Af);
				
				// Update flags based on 8-bit result
				SetFlagVal(Sf, (r & 0x80) != 0);
				SetFlagVal(Zf, r == 0);
				
				// Calculate parity using the same method as UpdateLogicResultFlags
				var bits = r ^ (r >> 4);
				bits &= 0xF;
				var even = (((0x6996 >> bits) & 1) == 0);
				SetFlagVal(Pf, even);
				break;
			}
			case 16:
			{
				// 16-bit TEST
				ushort a, b;
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					a = GetReg16(insn.GetOpRegister(0));
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					a = _mem.Read16(CalcMemAddress(insn));
				}
				else
				{
					a = (ushort)ReadOp(insn, 0);
				}
				
				if (insn.GetOpKind(1) == OpKind.Register)
				{
					b = GetReg16(insn.GetOpRegister(1));
				}
				else if (insn.GetOpKind(1) == OpKind.Memory)
				{
					b = _mem.Read16(CalcMemAddress(insn));
				}
				else
				{
					b = (ushort)ReadOp(insn, 1);
				}
				
				var r = (ushort)(a & b);
				
				ClearFlag(Cf);
				ClearFlag(Of);
				ClearFlag(Af);
				
				// Update flags based on 16-bit result
				SetFlagVal(Sf, (r & 0x8000) != 0);
				SetFlagVal(Zf, r == 0);
				
				// Calculate parity using the same method as UpdateLogicResultFlags
				var lo = (byte)r;
				var bits = lo ^ (lo >> 4);
				bits &= 0xF;
				var even = (((0x6996 >> bits) & 1) == 0);
				SetFlagVal(Pf, even);
				break;
			}
			default:
			{
				// 32-bit TEST (default behavior)
				var r = ReadOp(insn, 0) & ReadOp(insn, 1);
				ClearFlag(Cf);
				ClearFlag(Of);
				ClearFlag(Af);
				UpdateLogicResultFlags(r);
				break;
			}
		}
	}

	private void ExecCmp(Instruction insn)
	{
		var opSize = GetOpSizeBits(insn, 0);
		
		switch (opSize)
		{
			case 8:
			{
				// 8-bit CMP
				byte a, b;
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					a = GetReg8(insn.GetOpRegister(0));
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					a = _mem.Read8(CalcMemAddress(insn));
				}
				else
				{
					a = (byte)ReadOp(insn, 0);
				}
				
				if (insn.GetOpKind(1) == OpKind.Register)
				{
					b = GetReg8(insn.GetOpRegister(1));
				}
				else if (insn.GetOpKind(1) == OpKind.Memory)
				{
					b = _mem.Read8(CalcMemAddress(insn));
				}
				else if (insn.GetOpKind(1) == OpKind.Immediate8)
				{
					b = insn.Immediate8;
				}
				else
				{
					b = (byte)ReadOp(insn, 1);
				}
				
				var r = (byte)(a - b);
				
				// Set flags for 8-bit comparison
				SetFlagVal(Cf, a < b);
				SetFlagVal(Of, ((a ^ b) & (a ^ r) & 0x80) != 0);
				SetFlagVal(Af, ((a ^ b ^ r) & 0x10) != 0);
				SetFlagVal(Sf, (r & 0x80) != 0);
				SetFlagVal(Zf, r == 0);
				
				// Calculate parity using the same method as UpdateLogicResultFlags
				var bits = r ^ (r >> 4);
				bits &= 0xF;
				var even = (((0x6996 >> bits) & 1) == 0);
				SetFlagVal(Pf, even);
				break;
			}
			case 16:
			{
				// 16-bit CMP
				ushort a, b;
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					a = GetReg16(insn.GetOpRegister(0));
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					a = _mem.Read16(CalcMemAddress(insn));
				}
				else
				{
					a = (ushort)ReadOp(insn, 0);
				}
				
				if (insn.GetOpKind(1) == OpKind.Register)
				{
					b = GetReg16(insn.GetOpRegister(1));
				}
				else if (insn.GetOpKind(1) == OpKind.Memory)
				{
					b = _mem.Read16(CalcMemAddress(insn));
				}
				else
				{
					b = (ushort)ReadOp(insn, 1);
				}
				
				var r = (ushort)(a - b);
				
				// Set flags for 16-bit comparison
				SetFlagVal(Cf, a < b);
				SetFlagVal(Of, ((a ^ b) & (a ^ r) & 0x8000) != 0);
				SetFlagVal(Af, ((a ^ b ^ r) & 0x10) != 0);
				SetFlagVal(Sf, (r & 0x8000) != 0);
				SetFlagVal(Zf, r == 0);
				
				// Calculate parity using the same method as UpdateLogicResultFlags
				var lo = (byte)r;
				var bits = lo ^ (lo >> 4);
				bits &= 0xF;
				var even = (((0x6996 >> bits) & 1) == 0);
				SetFlagVal(Pf, even);
				break;
			}
			default:
			{
				// 32-bit CMP (default behavior)
				uint a = ReadOp(insn, 0), b = ReadOp(insn, 1), r = a - b;
				SetFlagsSub(a, b, r);
				break;
			}
		}
	}

	private void ExecInc(Instruction insn)
	{
		var opSize = GetOpSizeBits(insn, 0);
		
		switch (opSize)
		{
			case 8:
			{
				byte a = (insn.GetOpKind(0) == OpKind.Register) ? GetReg8(insn.GetOpRegister(0)) :
				         (insn.GetOpKind(0) == OpKind.Memory) ? _mem.Read8(CalcMemAddress(insn)) : (byte)ReadOp(insn, 0);
				byte r = (byte)(a + 1);
				
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					SetReg8(insn.GetOpRegister(0), r);
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					_mem.Write8(CalcMemAddress(insn), r);
				}

				// Overflow only occurs when incrementing max positive (0x7F) to min negative (0x80)
				SetFlagVal(Of, a == 0x7F); // Overflow from 0x7F to 0x80
				SetFlagVal(Af, ((a ^ 1 ^ r) & 0x10) != 0);
				UpdateLogicResultFlags(r, 0x80);
				break;
			}
			case 16:
			{
				ushort a = (insn.GetOpKind(0) == OpKind.Register) ? GetReg16(insn.GetOpRegister(0)) :
				           (insn.GetOpKind(0) == OpKind.Memory) ? _mem.Read16(CalcMemAddress(insn)) : (ushort)ReadOp(insn, 0);
				ushort r = (ushort)(a + 1);
				
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					SetReg16(insn.GetOpRegister(0), r);
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					_mem.Write16(CalcMemAddress(insn), r);
				}

				SetFlagVal(Of, a == 0x7FFF); // Overflow from 0x7FFF to 0x8000
				SetFlagVal(Af, ((a ^ 1 ^ r) & 0x10) != 0);
				UpdateLogicResultFlags(r, 0x8000);
				break;
			}
			default:
			{
				uint a = ReadOp(insn, 0), r = a + 1;
				WriteOp(insn, 0, r);
				// Overflow only occurs when incrementing max positive (0x7FFFFFFF) to min negative (0x80000000)
				SetFlagVal(Of, a == 0x7FFFFFFF);
				SetFlagVal(Af, ((a ^ 1u ^ r) & 0x10) != 0);
				UpdateLogicResultFlags(r);
				break;
			}
		}
	}

	private void ExecDec(Instruction insn)
	{
		var opSize = GetOpSizeBits(insn, 0);
		
		switch (opSize)
		{
			case 8:
			{
				byte a = (insn.GetOpKind(0) == OpKind.Register) ? GetReg8(insn.GetOpRegister(0)) :
				         (insn.GetOpKind(0) == OpKind.Memory) ? _mem.Read8(CalcMemAddress(insn)) : (byte)ReadOp(insn, 0);
				byte r = (byte)(a - 1);
				
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					SetReg8(insn.GetOpRegister(0), r);
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					_mem.Write8(CalcMemAddress(insn), r);
				}

				// Overflow only occurs when decrementing min negative (0x80) to max positive (0x7F)
				SetFlagVal(Of, a == 0x80); // Overflow from 0x80 to 0x7F
				SetFlagVal(Af, ((a ^ 1 ^ r) & 0x10) != 0);
				UpdateLogicResultFlags(r, 0x80);
				break;
			}
			case 16:
			{
				ushort a = (insn.GetOpKind(0) == OpKind.Register) ? GetReg16(insn.GetOpRegister(0)) :
				           (insn.GetOpKind(0) == OpKind.Memory) ? _mem.Read16(CalcMemAddress(insn)) : (ushort)ReadOp(insn, 0);
				ushort r = (ushort)(a - 1);
				
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					SetReg16(insn.GetOpRegister(0), r);
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					_mem.Write16(CalcMemAddress(insn), r);
				}

				SetFlagVal(Of, a == 0x8000); // Overflow from 0x8000 to 0x7FFF
				SetFlagVal(Af, ((a ^ 1 ^ r) & 0x10) != 0);
				UpdateLogicResultFlags(r, 0x8000);
				break;
			}
			default:
			{
				uint a = ReadOp(insn, 0), r = a - 1;
				WriteOp(insn, 0, r);
				// Overflow only occurs when decrementing min negative (0x80000000) to max positive (0x7FFFFFFF)
				SetFlagVal(Of, a == 0x80000000);
				SetFlagVal(Af, ((a ^ 1u ^ r) & 0x10) != 0);
				UpdateLogicResultFlags(r);
				break;
			}
		}
	}

	private void ExecShiftLeft(Instruction insn)
	{
		var a = ReadOp(insn, 0);
		var c = GetShiftCount(insn);
		if (c == 0)
		{
			return;
		}

		c &= 0x1F;
		if (c == 0)
		{
			return;
		}

		// Get operand size to determine bit width
		var opSize = GetOpSizeBits(insn, 0);
		var bitWidth = opSize; // 8, 16, or 32
		var signBit = 1u << (bitWidth - 1); // Bit 7 for 8-bit, bit 15 for 16-bit, bit 31 for 32-bit

		var r = a << c;
		
		// Per https://github.com/SingleStepTests/80386/issues/4:
		// For 8-bit SHL when count > 8:
		// OF and CF set to 1 if ((count == 16 OR count == 24) AND (src & 1)) else 0
		if (opSize == 8 && c > 8)
		{
			bool specialCase = (c == 16 || c == 24) && ((a & 1) != 0);
			SetFlagVal(Of, specialCase);
			SetFlagVal(Cf, specialCase);
		}
		else if (c > bitWidth)
		{
			// For counts greater than bit width (16-bit/32-bit), all bits shift out
			// CF and OF are undefined, but real 80386 hardware clears them
			SetFlagVal(Cf, false);
			SetFlagVal(Of, false);
		}
		else
		{
			// Normal case
			var lastOut = (a >> (bitWidth - c)) & 1u;
			SetFlagVal(Cf, lastOut != 0);
			if (c == 1)
			{
				// OF is set if sign bit changed (XOR of MSB before and after)
				bool before = (a & signBit) != 0, after = (r & signBit) != 0;
				SetFlagVal(Of, before ^ after);
			}
			else
			{
				// OF is undefined when count > 1, but real 80386 hardware clears it
				SetFlagVal(Of, false);
			}
		}
		
		// AF is undefined for shift operations, but on real 80386 hardware it gets set
		// Based on empirical evidence from SingleStepTests, AF is set when count > 0
		SetFlagVal(Af, true);

		WriteOp(insn, 0, r);
		UpdateLogicResultFlags(r, signBit);
	}

	private void ExecShiftRight(Instruction insn, bool arithmetic)
	{
		var a = ReadOp(insn, 0);
		var c = GetShiftCount(insn);
		if (c == 0)
		{
			return;
		}

		c &= 0x1F;
		if (c == 0)
		{
			return;
		}

		// Get operand size to determine bit width
		var opSize = GetOpSizeBits(insn, 0);
		var bitWidth = opSize; // 8, 16, or 32
		var signBit = 1u << (bitWidth - 1); // Bit 7 for 8-bit, bit 15 for 16-bit, bit 31 for 32-bit

		uint r;
		if (arithmetic)
		{
			// For SAR, need to sign-extend based on operand size
			if (opSize == 8)
			{
				var s = (sbyte)(byte)a;
				r = (uint)(byte)(s >> c);
			}
			else if (opSize == 16)
			{
				var s = (short)(ushort)a;
				r = (uint)(ushort)(s >> c);
			}
			else
			{
				var s = (int)a;
				r = (uint)(s >> c);
			}
			
			if (c == 1)
			{
				// For SAR with count=1, OF is always cleared
				SetFlagVal(Of, false);
			}
			else
			{
				// OF is undefined when count > 1, but real 80386 hardware clears it
				SetFlagVal(Of, false);
			}
			
			var lastOut = (a >> (c - 1)) & 1u;
			SetFlagVal(Cf, lastOut != 0);
		}
		else
		{
			r = a >> c;
			
			// Per https://github.com/SingleStepTests/80386/issues/4:
			// For 8-bit SHR when count > 8:
			// OF set to 0, CF set if ((count == 16 OR count == 24) AND (src & 0x80)) else 0
			if (opSize == 8 && c > 8)
			{
				SetFlagVal(Of, false);
				bool specialCase = (c == 16 || c == 24) && ((a & 0x80) != 0);
				SetFlagVal(Cf, specialCase);
			}
			else if (c > bitWidth)
			{
				// For counts greater than bit width (16-bit/32-bit), all bits shift out
				// The result is 0 for logical shift
				// CF and OF behavior: CF would be 0 (no bits left), OF is cleared
				SetFlagVal(Cf, false);
				SetFlagVal(Of, false);
			}
			else
			{
				if (c == 1)
				{
					// OF is set to MSB of original operand
					SetFlagVal(Of, (a & signBit) != 0);
				}
				else
				{
					// OF is undefined when count > 1, but real 80386 hardware clears it
					SetFlagVal(Of, false);
				}
				
				var lastOut = (a >> (c - 1)) & 1u;
				SetFlagVal(Cf, lastOut != 0);
			}
		}

		// AF is undefined for shift operations, but on real 80386 hardware it gets set
		// Based on empirical evidence from SingleStepTests, AF is set when count > 0
		SetFlagVal(Af, true);
		WriteOp(insn, 0, r);
		UpdateLogicResultFlags(r, signBit);
	}

	private void ExecRotate(Instruction insn, RotateKind kind)
	{
		var a = ReadOp(insn, 0);
		var c = GetShiftCount(insn);
		if (c == 0)
		{
			return;
		}

		// Get operand size to determine bit width and mask
		var opSize = GetOpSizeBits(insn, 0);
		var bitWidth = (uint)opSize; // 8, 16, or 32
		var mask = opSize switch
		{
			8 => 0xFFu,
			16 => 0xFFFFu,
			_ => 0xFFFFFFFFu
		};
		var msbMask = 1u << ((int)bitWidth - 1); // Bit 7 for 8-bit, bit 15 for 16-bit, bit 31 for 32-bit
		var msb1Mask = 1u << ((int)bitWidth - 2); // Bit 6 for 8-bit, bit 14 for 16-bit, bit 30 for 32-bit

		if (kind is RotateKind.Rol or RotateKind.Ror)
		{
			c &= 0x1F;
			// For 8/16-bit operands, further mask count to operand size
			if (opSize < 32)
			{
				c = (int)(c % bitWidth);
			}
		}
		else
		{
			// RCL/RCR: count is masked to operand size + 1 (for carry bit)
			c = (int)(c % (bitWidth + 1));
		}

		if (c == 0)
		{
			return;
		}

		var r = a;
		switch (kind)
		{
			case RotateKind.Rol:
				// Rotate left: shift left by c, fill with bits shifted out from top
				r = ((a << (int)c) | (a >> ((int)bitWidth - (int)c))) & mask;
				SetFlagVal(Cf, (r & 1) != 0);
				if (c == 1)
				{
					var msb = (r & msbMask) != 0;
					var cf = GetFlag(Cf);
					SetFlagVal(Of, msb ^ cf);
				}
				// OF is undefined when count > 1, so don't modify it

				break;
			case RotateKind.Ror:
				// Rotate right: shift right by c, fill with bits shifted out from bottom
				r = ((a >> (int)c) | (a << ((int)bitWidth - (int)c))) & mask;
				SetFlagVal(Cf, ((r >> ((int)bitWidth - 1)) & 1) != 0);
				if (c == 1)
				{
					var bitN1 = (r & msbMask) != 0;   // MSB
					var bitN2 = (r & msb1Mask) != 0;  // MSB-1
					SetFlagVal(Of, bitN1 ^ bitN2);
				}
				// OF is undefined when count > 1, so don't modify it

				break;
			case RotateKind.Rcl:
				// Rotate left through carry
				for (var i = 0; i < c; i++)
				{
					var carry = GetFlag(Cf) ? 1u : 0u;
					var newCarry = (a >> ((int)bitWidth - 1)) & 1u;
					r = ((a << 1) | carry) & mask;
					SetFlagVal(Cf, newCarry != 0);
					a = r;
				}

				if (c == 1)
				{
					// OF = MSB XOR CF after rotation
					var msb = (r & msbMask) != 0;
					var cf = GetFlag(Cf);
					SetFlagVal(Of, msb != cf);
				}
				// OF is undefined when count > 1, so don't modify it

				break;
			case RotateKind.Rcr:
				// Rotate right through carry
				for (var i = 0; i < c; i++)
				{
					var carry = GetFlag(Cf) ? 1u : 0u;
					var newCarry = a & 1u;
					r = ((a >> 1) | (carry << ((int)bitWidth - 1))) & mask;
					SetFlagVal(Cf, newCarry != 0);
					a = r;
				}

				if (c == 1)
				{
					// OF = MSB XOR (MSB-1) after rotation
					var bitN1 = (r & msbMask) != 0;
					var bitN2 = (r & msb1Mask) != 0;
					SetFlagVal(Of, bitN1 ^ bitN2);
				}
				// OF is undefined when count > 1, so don't modify it

				break;
		}

		WriteOp(insn, 0, r);
		// Note: Rotate instructions only affect CF and OF flags, not ZF, SF, PF
		// AF is undefined for rotate operations
	}

	private void ExecNot(Instruction insn)
	{
		var opSize = GetOpSizeBits(insn, 0);
		
		switch (opSize)
		{
			case 8:
			{
				byte a = (insn.GetOpKind(0) == OpKind.Register) ? GetReg8(insn.GetOpRegister(0)) : _mem.Read8(CalcMemAddress(insn));
				byte r = (byte)~a;
				
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					SetReg8(insn.GetOpRegister(0), r);
				}
				else
				{
					_mem.Write8(CalcMemAddress(insn), r);
				}

				break;
			}
			case 16:
			{
				ushort a = (insn.GetOpKind(0) == OpKind.Register) ? GetReg16(insn.GetOpRegister(0)) : _mem.Read16(CalcMemAddress(insn));
				ushort r = (ushort)~a;
				
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					SetReg16(insn.GetOpRegister(0), r);
				}
				else
				{
					_mem.Write16(CalcMemAddress(insn), r);
				}

				break;
			}
			default:
			{
				var a = ReadOp(insn, 0);
				var r = ~a;
				WriteOp(insn, 0, r);
				break;
			}
		}
	}

	private void ExecNeg(Instruction insn)
	{
		var opSize = GetOpSizeBits(insn, 0);
		
		switch (opSize)
		{
			case 8:
			{
				byte a = insn.GetOpKind(0) == OpKind.Register ? GetReg8(insn.GetOpRegister(0)) : _mem.Read8(CalcMemAddress(insn));
				byte r = (byte)(0 - a);
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					SetReg8(insn.GetOpRegister(0), r);
				}
				else
				{
					_mem.Write8(CalcMemAddress(insn), r);
				}

				SetFlagsSub(0, a, r, 0x80);
				break;
			}
			case 16:
			{
				ushort a = insn.GetOpKind(0) == OpKind.Register ? GetReg16(insn.GetOpRegister(0)) : _mem.Read16(CalcMemAddress(insn));
				ushort r = (ushort)(0 - a);
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					SetReg16(insn.GetOpRegister(0), r);
				}
				else
				{
					_mem.Write16(CalcMemAddress(insn), r);
				}

				SetFlagsSub(0, a, r, 0x8000);
				break;
			}
			default:
			{
				var a = ReadOp(insn, 0);
				var r = 0u - a;
				WriteOp(insn, 0, r);
				SetFlagsSub(0, a, r);
				break;
			}
		}
	}

	private void ExecBswap(Instruction insn)
	{
		if (insn.GetOpKind(0) == OpKind.Register)
		{
			var r = insn.GetOpRegister(0);
			var v = GetReg32(r);
			v = (v >> 24) | ((v >> 8) & 0x0000FF00) | ((v << 8) & 0x00FF0000) | (v << 24);
			SetReg32(r, v);
		}
	}

	private void ExecCbw()
	{
		// CBW: Convert Byte to Word
		// Sign-extend AL into AX
		// If bit 7 of AL is 0 (positive), AH = 0x00
		// If bit 7 of AL is 1 (negative), AH = 0xFF
		var al = (byte)(_eax & 0xFF);
		var sign = (al & 0x80) != 0;
		var ah = sign ? (byte)0xFF : (byte)0x00;
		_eax = (_eax & 0xFFFF0000) | ((uint)ah << 8) | al;
	}

	private void ExecCwd()
	{
		// CWD: Convert Word to Doubleword (16-bit mode)
		// Sign-extend AX into DX:AX
		// If bit 15 of AX is 0 (positive), DX = 0x0000
		// If bit 15 of AX is 1 (negative), DX = 0xFFFF
		// Preserves the high 16 bits of EDX
		var ax = (ushort)(_eax & 0xFFFF);
		var sign = (ax & 0x8000) != 0;
		var dx = sign ? (ushort)0xFFFF : (ushort)0x0000;
		_edx = (_edx & 0xFFFF0000) | dx;
	}

	private void ExecCwde()
	{
		// CWDE: Convert Word to Doubleword Extended
		// Sign-extend AX into EAX
		// If bit 15 of AX is 0 (positive), high word = 0x0000
		// If bit 15 of AX is 1 (negative), high word = 0xFFFF
		var ax = (ushort)(_eax & 0xFFFF);
		var sign = (ax & 0x8000) != 0;
		_eax = sign ? (0xFFFF0000 | ax) : ax;
	}

	private void ExecCdq()
	{
		// CDQ: Sign-extend EAX into EDX:EAX
		// If bit 31 of EAX is 0 (positive), EDX = 0x00000000
		// If bit 31 of EAX is 1 (negative), EDX = 0xFFFFFFFF
		_edx = (_eax & 0x80000000) != 0 ? 0xFFFFFFFF : 0x00000000;
	}

	private void ExecXchg(Instruction insn)
	{
		var a = ReadOp(insn, 0);
		var b = ReadOp(insn, 1);
		WriteOp(insn, 0, b);
		WriteOp(insn, 1, a);
	}

	private void ExecXlatb()
	{
		// XLATB - Table lookup translation
		// AL = [EBX + AL]
		var al = (byte)(_eax & 0xFF);
		var addr = _ebx + al;
		var value = _mem.Read8(addr);
		_eax = (_eax & 0xFFFFFF00) | value;
	}

	private void ExecCmpxchg(Instruction insn)
	{
		// CMPXCHG dest, src
		// Compare AL/AX/EAX with dest. If equal, ZF=1 and dest=src. If not equal, ZF=0 and AL/AX/EAX=dest.
		var dest = ReadOp(insn, 0);
		var src = ReadOp(insn, 1);
		var accumulator = _eax;

		// Compare accumulator with destination
		var result = accumulator - dest;
		SetFlagsSub(accumulator, dest, result);

		if (GetFlag(Zf))
		{
			// Equal: write src to dest
			WriteOp(insn, 0, src);
		}
		else
		{
			// Not equal: write dest to accumulator
			_eax = dest;
		}
	}

	private void ExecXadd(Instruction insn)
	{
		// XADD dest, src
		// temp = dest; dest = dest + src; src = temp
		var dest = ReadOp(insn, 0);
		var src = ReadOp(insn, 1);

		var result = dest + src;
		WriteOp(insn, 0, result);
		WriteOp(insn, 1, dest);

		SetFlagsAdd(dest, src, result);
	}

	private void ExecCmpxchg8B(Instruction insn)
	{
		// CMPXCHG8B m64
		// Compare EDX:EAX with m64. If equal, ZF=1 and m64=ECX:EBX. If not equal, ZF=0 and EDX:EAX=m64.
		var addr = CalcMemAddress(insn);

		// Read 64-bit value from memory
		var memLow = Read32(addr);
		var memHigh = Read32(addr + 4);

		// Compare with EDX:EAX
		if (_eax == memLow && _edx == memHigh)
		{
			// Equal: write ECX:EBX to memory
			Write32(addr, _ebx);
			Write32(addr + 4, _ecx);
			SetFlag(Zf);
		}
		else
		{
			// Not equal: load memory into EDX:EAX
			_eax = memLow;
			_edx = memHigh;
			ClearFlag(Zf);
		}
	}

	private void ExecRdtsc()
	{
		// RDTSC - Read Time-Stamp Counter
		// Returns timestamp in EDX:EAX
		// Use Stopwatch for high-resolution timing when available
		ulong ticks;
		if (RdtscIsHighResolution)
		{
			// Use high-resolution Stopwatch
			// Scale the ticks to approximate CPU cycle count (assuming ~1 GHz for compatibility)
			var elapsed = RdtscStopwatch.ElapsedTicks;
			// Convert to approximate "CPU cycles" by scaling based on frequency
			// Real CPUs run at GHz speeds, so we scale the Stopwatch frequency to approximate that
			ticks = (ulong)((double)elapsed / RdtscFrequency * 1_000_000_000.0);
		}
		else
		{
			// Fall back to TickCount64 if high-resolution timer is not available
			ticks = (ulong)Environment.TickCount64;
		}

		_eax = (uint)(ticks & 0xFFFFFFFF);
		_edx = (uint)(ticks >> 32);
	}

	private void ExecCpuid()
	{
		// CPUID - CPU Identification
		// Input: EAX = function number, ECX = sub-function (for some functions)
		// Output: EAX, EBX, ECX, EDX contain CPU info
		switch (_eax)
		{
			case 0: // Get vendor string and max function
				_eax = 7; // Max supported standard function (extended to support function 7)
				_ebx = 0x756E6547; // "Genu"
				_edx = 0x49656E69; // "ineI"
				_ecx = 0x6C65746E; // "ntel"
				break;

			case 1: // Get feature flags
				// Pentium III (Family 6, Model 7, Stepping 3)
				// A very common CPU that CHKCPU32 should recognize well
				_eax = 0x00000673; // Family 6, Model 7, Stepping 3
				_ebx = 0x00000001; // Brand index = 1 (Intel Celeron processor)
				_ecx = CpuIntrinsics.GetCpuidEcxFeatures(); // Feature flags based on host CPU
				_edx = CpuIntrinsics.GetCpuidEdxFeatures(); // Feature flags based on host CPU
				break;

			case 2: // Cache and TLB Descriptor (old style)
				// Returns cache descriptors in EAX, EBX, ECX, EDX
				// Each descriptor is a byte value identifying cache type/size
				// AL[7:0] = number of times CPUID must be executed to get all descriptors
				_eax = 0x00000001; // Query once (bits 7-0), descriptor 0x00 (no cache) in bytes 3-1
				_ebx = 0x00000000; // No descriptors
				_ecx = 0x00000000; // No descriptors
				_edx = 0x00000000; // No descriptors
				break;

			case 4: // Deterministic Cache Parameters (sub-function in ECX)
				// This function reports cache hierarchy information
				// Input: ECX = sub-function index (0 = L1D, 1 = L1I, 2 = L2, ...)
				// Output: EAX, EBX, ECX, EDX contain cache information
				// When EAX[4:0] = 0, there are no more caches
				switch (_ecx)
				{
					case 0: // L1 Data Cache
						// EAX bits:
						//   [4:0] = Cache Type (1 = Data Cache)
						//   [7:5] = Cache Level (1 = L1)
						//   [8] = Self Initializing
						//   [9] = Fully Associative (0 = not fully associative)
						//   [14:10] = Reserved (0)
						//   [25:14] = Max logical processors sharing cache - 1 (0 = 1 processor)
						//   [31:26] = Max cores in package - 1 (0 = 1 core)
						_eax = 0x00000121; // Data cache (1), L1 (1 << 5), Self-init (1 << 8)
						_ebx = 0x0700003F; // Line size, partitions, associativity (64-byte lines, 8-way)
						_ecx = 0x0000003F; // Number of sets - 1 (64 sets = 32KB cache)
						_edx = 0x00000000; // Write-back invalidate, not inclusive, no complex indexing
						break;

					case 1: // L1 Instruction Cache
						_eax = 0x00000122; // Instruction cache (2), L1 (1 << 5), Self-init (1 << 8)
						_ebx = 0x0700003F; // Line size, partitions, associativity (64-byte lines, 8-way)
						_ecx = 0x0000003F; // Number of sets - 1 (64 sets = 32KB cache)
						_edx = 0x00000000;
						break;

					case 2: // L2 Unified Cache
						_eax = 0x00000143; // Unified cache (3), L2 (2 << 5), Self-init (1 << 8)
						_ebx = 0x01C0003F; // Line size, partitions, associativity (64-byte lines, 8-way)
						_ecx = 0x000001FF; // Number of sets - 1 (512 sets = 256KB cache)
						_edx = 0x00000000;
						break;

					default: // No more caches
						_eax = 0x00000000; // Cache type = 0 (no more caches)
						_ebx = 0x00000000;
						_ecx = 0x00000000;
						_edx = 0x00000000;
						break;
				}
				break;

			case 7: // Extended features (sub-function in ECX)
				if (_ecx == 0)
				{
					_eax = 0; // Max sub-function
					_ebx = CpuIntrinsics.GetCpuidExtendedEbxFeatures(); // Extended feature flags
					_ecx = 0;
					_edx = 0;
				}
				else
				{
					// Unsupported sub-function
					_eax = 0;
					_ebx = 0;
					_ecx = 0;
					_edx = 0;
				}

				break;

			case 0x80000000: // Get maximum extended function
				_eax = 0x80000001; // Max supported extended function
				_ebx = 0;
				_ecx = 0;
				_edx = 0;
				break;

			case 0x80000001: // Extended processor info and feature bits
				_eax = 0x00000673; // Extended processor signature (same as function 1)
				_ebx = 0;
				_ecx = CpuIntrinsics.GetCpuid80000001EcxFeatures(); // Extended feature flags (includes LZCNT)
				_edx = 0; // Extended feature flags in EDX
				break;

			default:
				// Unsupported function - return zeros
				_eax = 0;
				_ebx = 0;
				_ecx = 0;
				_edx = 0;
				break;
		}
	}

	private void ExecRdmsr()
	{
		// RDMSR - Read Model Specific Register (privileged)
		// Input: ECX = MSR address
		// Output: EDX:EAX = MSR value
		// For user-mode emulation, return dummy values
		_eax = 0;
		_edx = 0;
	}

	private void ExecWrmsr()
	{
		// WRMSR - Write Model Specific Register (privileged)
		// Input: ECX = MSR address, EDX:EAX = value
		// For user-mode emulation, this is a no-op
	}

	private void ExecInvd()
	{
		// INVD - Invalidate Cache (privileged)
		// For user-mode emulation, this is a no-op
	}

	private void ExecWbinvd()
	{
		// WBINVD - Write-Back and Invalidate Cache (privileged)
		// For user-mode emulation, this is a no-op
	}

	private void ExecInvlpg(Instruction insn)
	{
		// INVLPG - Invalidate TLB Entry (privileged)
		// For user-mode emulation, this is a no-op
	}

	private void ExecRsm()
	{
		// RSM - Resume from System Management Mode (privileged)
		// For user-mode emulation, this is a no-op
	}

	private void ExecSetcc(Instruction insn)
	{
		var v = (byte)(IsSetccTrue(insn.Mnemonic) ? 1 : 0);
		if (insn.GetOpKind(0) == OpKind.Memory)
		{
			_mem.Write8(CalcMemAddress(insn), v);
		}
		else
		{
			SetReg8(insn.GetOpRegister(0), v);
		}
	}

	private void ExecCmovcc(Instruction insn)
	{
		if (IsCmovccTrue(insn.Mnemonic))
		{
			var src = ReadOp(insn, 1);
			WriteOp(insn, 0, src);
		}
	}

	#region REP Prefix Handlers

	/// <summary>
	/// Executes a string instruction with REP prefix (unconditional repeat).
	/// Repeats the operation while ECX != 0, decrementing ECX after each iteration.
	/// </summary>
	/// <param name="operation">The string operation to execute each iteration</param>
	private void Rep(Action operation)
	{
		while (_ecx != 0)
		{
			operation();
			_ecx--;
		}
	}

	/// <summary>
	/// Executes a string instruction with REPE/REPZ prefix (repeat while equal/zero).
	/// Repeats while ECX != 0 AND ZF = 1, decrementing ECX after each iteration.
	/// Stops early if ZF becomes 0 (values not equal).
	/// </summary>
	/// <param name="operation">The string operation to execute each iteration</param>
	private void Repe(Action operation)
	{
		while (_ecx != 0)
		{
			operation();
			_ecx--;
			if (!GetFlag(Zf))
			{
				break; // Stop when not equal
			}
		}
	}

	/// <summary>
	/// Executes a string instruction with REPNE/REPNZ prefix (repeat while not equal/not zero).
	/// Repeats while ECX != 0 AND ZF = 0, decrementing ECX after each iteration.
	/// Stops early if ZF becomes 1 (values equal).
	/// </summary>
	/// <param name="operation">The string operation to execute each iteration</param>
	private void Repne(Action operation)
	{
		while (_ecx != 0)
		{
			operation();
			_ecx--;
			if (GetFlag(Zf))
			{
				break; // Stop when equal
			}
		}
	}

	#endregion

	private void ExecMovs(int size, bool rep)
	{
		var delta = GetFlag(Df) ? -size : size;
		
		// Define the single-iteration operation
		void MovsOperation()
		{
			var v = size switch
			{
				1 => _mem.Read8(_esi),
				2 => _mem.Read16(_esi),
				_ => _mem.Read32(_esi)
			};
			if (size == 1)
			{
				_mem.Write8(_edi, (byte)v);
			}
			else if (size == 2)
			{
				_mem.Write16(_edi, (ushort)v);
			}
			else
			{
				_mem.Write32(_edi, v);
			}
			_esi = (uint)(_esi + delta);
			_edi = (uint)(_edi + delta);
		}
		
		// Execute with or without REP prefix
		if (rep)
		{
			Rep(MovsOperation);
		}
		else
		{
			MovsOperation();
		}
	}

	private void ExecStos(int size, bool rep)
	{
		var delta = GetFlag(Df) ? -size : size;
		var src = size switch
		{
			1 => (byte)_eax,
			2 => (ushort)_eax,
			_ => _eax
		};
		
		// Define the single-iteration operation
		void StosOperation()
		{
			if (size == 1)
			{
				_mem.Write8(_edi, (byte)src);
			}
			else if (size == 2)
			{
				_mem.Write16(_edi, (ushort)src);
			}
			else
			{
				_mem.Write32(_edi, src);
			}
			_edi = (uint)(_edi + delta);
		}
		
		// Execute with or without REP prefix
		if (rep)
		{
			Rep(StosOperation);
		}
		else
		{
			StosOperation();
		}
	}

	private void ExecLods(int size, bool rep)
	{
		var delta = GetFlag(Df) ? -size : size;
		
		// Define the single-iteration operation
		void LodsOperation()
		{
			var v = size switch
			{
				1 => _mem.Read8(_esi),
				2 => _mem.Read16(_esi),
				_ => _mem.Read32(_esi)
			};
			if (size == 1)
			{
				_eax = (_eax & 0xFFFFFF00) | (v & 0xFF);
			}
			else if (size == 2)
			{
				_eax = (_eax & 0xFFFF0000) | (v & 0xFFFF);
			}
			else
			{
				_eax = v;
			}
			_esi = (uint)(_esi + delta);
		}
		
		// Execute with or without REP prefix
		if (rep)
		{
			Rep(LodsOperation);
		}
		else
		{
			LodsOperation();
		}
	}

	private void ExecIns(int size, bool rep)
	{
		// INS reads from I/O port DX and writes to [EDI]
		// Since I/O ports are not fully emulated, we write 0 (similar to IN instruction handling)
		var delta = GetFlag(Df) ? -size : size;
		
		// Define the single-iteration operation
		void InsOperation()
		{
			// I/O port read would go here, but we stub it to return 0
			uint value = 0;
			
			if (size == 1)
			{
				_mem.Write8(_edi, (byte)value);
			}
			else if (size == 2)
			{
				_mem.Write16(_edi, (ushort)value);
			}
			else
			{
				_mem.Write32(_edi, value);
			}
			_edi = (uint)(_edi + delta);
		}
		
		// Execute with or without REP prefix
		if (rep)
		{
			Rep(InsOperation);
		}
		else
		{
			InsOperation();
		}
	}

	private void ExecOuts(int size, bool rep)
	{
		// OUTS reads from [ESI] and writes to I/O port DX
		// Since I/O ports are not fully emulated, we just read and discard (similar to OUT instruction handling)
		var delta = GetFlag(Df) ? -size : size;
		
		// Define the single-iteration operation
		void OutsOperation()
		{
			// Read from memory (required for proper ESI advancement)
			if (size == 1)
			{
				_ = _mem.Read8(_esi);
			}
			else if (size == 2)
			{
				_ = _mem.Read16(_esi);
			}
			else
			{
				_ = _mem.Read32(_esi);
			}
			// I/O port write would go here, but we stub it as a no-op
			_esi = (uint)(_esi + delta);
		}
		
		// Execute with or without REP prefix
		if (rep)
		{
			Rep(OutsOperation);
		}
		else
		{
			OutsOperation();
		}
	}

	private void ExecCmps(int size, bool repe, bool repne)
	{
		var delta = GetFlag(Df) ? -size : size;
		
		// Define the single-iteration operation
		void CmpsOperation()
		{
			var a = size switch
			{
				1 => _mem.Read8(_esi),
				2 => _mem.Read16(_esi),
				_ => _mem.Read32(_esi)
			};
			var b = size switch
			{
				1 => _mem.Read8(_edi),
				2 => _mem.Read16(_edi),
				_ => _mem.Read32(_edi)
			};
			var r = a - b;
			SetFlagsSub(a, b, r);
			_esi = (uint)(_esi + delta);
			_edi = (uint)(_edi + delta);
		}
		
		// Execute with appropriate REP prefix
		if (repe)
		{
			Repe(CmpsOperation);
		}
		else if (repne)
		{
			Repne(CmpsOperation);
		}
		else
		{
			CmpsOperation();
		}
	}

	private void ExecScas(int size, bool repe, bool repne)
	{
		var delta = GetFlag(Df) ? -size : size;
		var a = size switch
		{
			1 => (byte)_eax,
			2 => (ushort)_eax,
			_ => _eax
		};
		
		// Define the single-iteration operation
		void ScasOperation()
		{
			var b = size switch
			{
				1 => _mem.Read8(_edi),
				2 => _mem.Read16(_edi),
				_ => _mem.Read32(_edi)
			};
			var r = a - b;
			SetFlagsSub(a, b, r);
			_edi = (uint)(_edi + delta);
		}
		
		// Execute with appropriate REP prefix
		if (repe)
		{
			Repe(ScasOperation);
		}
		else if (repne)
		{
			Repne(ScasOperation);
		}
		else
		{
			ScasOperation();
		}
	}

	private void ExecMul(Instruction insn)
	{
		// MUL performs unsigned multiplication
		// 8-bit form: AX = AL * r/m8
		// 16-bit form: DX:AX = AX * r/m16
		// 32-bit form: EDX:EAX = EAX * r/m32
		// Flags: CF and OF are set if high-order bits contain significant digits; otherwise cleared
		// AF, PF, SF, ZF are undefined (not modified)
		
		var opSize = GetOpSizeBits(insn, 0);
		
		switch (opSize)
		{
			case 8:
			{
				// 8-bit: AX = AL * r/m8
				var src = (byte)ReadOp(insn, 0);
				var prod = (uint)((byte)(_eax & 0xFF) * src);
				_eax = (_eax & 0xFFFF0000) | (prod & 0xFFFF);
				var carry = (prod & 0xFF00) != 0;
				SetFlagVal(Cf, carry);
				SetFlagVal(Of, carry);
				// AF, PF, SF, ZF are undefined, don't modify them
				break;
			}
			case 16:
			{
				// 16-bit: DX:AX = AX * r/m16
				var src = (ushort)ReadOp(insn, 0);
				var prod = (ushort)(_eax & 0xFFFF) * (uint)src;
				_eax = (_eax & 0xFFFF0000) | (prod & 0xFFFF);
				_edx = (_edx & 0xFFFF0000) | ((prod >> 16) & 0xFFFF);
				var carry = (prod & 0xFFFF0000) != 0;
				SetFlagVal(Cf, carry);
				SetFlagVal(Of, carry);
				// AF, PF, SF, ZF are undefined, don't modify them
				break;
			}
			default:
			{
				// 32-bit: EDX:EAX = EAX * r/m32
				var src = ReadOp(insn, 0);
				var prod = _eax * (ulong)src;
				_eax = (uint)prod;
				_edx = (uint)(prod >> 32);
				var carry = _edx != 0;
				SetFlagVal(Cf, carry);
				SetFlagVal(Of, carry);
				// AF, PF, SF, ZF are undefined, don't modify them
				break;
			}
		}
	}

	private void ExecImul(Instruction insn)
	{
		// IMUL performs signed multiplication
		// Flags: CF and OF are set if result doesn't fit in destination size; otherwise cleared
		// AF, PF, SF, ZF are undefined (not modified)
		
		if (insn.OpCount == 1)
		{
			// 8-bit form: AX = AL * r/m8
			// 16-bit form: DX:AX = AX * r/m16
			// 32-bit form: EDX:EAX = EAX * r/m32
			
			var opSize = GetOpSizeBits(insn, 0);
			
			switch (opSize)
			{
				case 8:
				{
					// 8-bit: AX = AL * r/m8 (signed)
					var src = (sbyte)(byte)ReadOp(insn, 0);
					var al = (sbyte)(byte)(_eax & 0xFF);
					var prod = (short)(al * src);
					_eax = (_eax & 0xFFFF0000) | (ushort)prod;
					// Overflow if result doesn't fit in 8 bits (sign-extended)
					var overflow = prod != (sbyte)prod;
					SetFlagVal(Cf, overflow);
					SetFlagVal(Of, overflow);
					// AF, PF, SF, ZF are undefined, don't modify them
					break;
				}
				case 16:
				{
					// 16-bit: DX:AX = AX * r/m16 (signed)
					var src = (short)(ushort)ReadOp(insn, 0);
					var ax = (short)(ushort)(_eax & 0xFFFF);
					var prod = (int)(ax * src);
					_eax = (_eax & 0xFFFF0000) | ((uint)prod & 0xFFFF);
				    _edx = (_edx & 0xFFFF0000) | (((uint)prod >> 16) & 0xFFFF);
				    // Overflow if upper 16 bits are not sign extension of lower 16 bits
				    var upper = (prod >> 16) & 0xFFFF;
				    var overflow = upper != 0 && upper != 0xFFFF;
				    SetFlagVal(Cf, overflow);
					SetFlagVal(Of, overflow);
					// AF, PF, SF, ZF are undefined, don't modify them
					break;
				}
				default:
				{
					// 32-bit: EDX:EAX = EAX * r/m32 (signed)
					var prod = (int)_eax * (long)(int)ReadOp(insn, 0);
					_eax = (uint)prod;
					_edx = (uint)(prod >> 32);
					// Overflow if result doesn't fit in 32 bits (sign-extended)
					var overflow = prod != (int)prod;
					SetFlagVal(Cf, overflow);
					SetFlagVal(Of, overflow);
					// AF, PF, SF, ZF are undefined, don't modify them
					break;
				}
			}
		}
		else
		{
			// Two or three operand form: dest = src1 * src2
			var prod = (int)ReadOp(insn, 1) *
			           (long)(insn.OpCount >= 3 ? (int)ReadOp(insn, 2) : (int)ReadOp(insn, 1));
			var r = (uint)prod;
			WriteOp(insn, 0, r);
			var overflow = prod is > int.MaxValue or < int.MinValue;
			SetFlagVal(Cf, overflow);
			SetFlagVal(Of, overflow);
			// AF, PF, SF, ZF are undefined, don't modify them
		}
	}

	private void ExecDiv(Instruction insn)
	{
		// DIV performs unsigned division
		// 8-bit form: AL = AX / r/m8, AH = AX % r/m8
		// 16-bit form: AX = DX:AX / r/m16, DX = DX:AX % r/m16
		// 32-bit form: EAX = EDX:EAX / r/m32, EDX = EDX:EAX % r/m32
		
		var opSize = GetOpSizeBits(insn, 0);
		
		switch (opSize)
		{
			case 8:
			{
				// 8-bit: AL = AX / r/m8, AH = AX % r/m8
				var divisor = (byte)ReadOp(insn, 0);
				if (divisor == 0)
				{
					_logger.LogWarning("[IcedCpu] Division by zero at EIP=0x{Eip:X8}", _eip);
					return;
				}
				
				var dividend = (ushort)(_eax & 0xFFFF);
				var q = dividend / divisor;
				if (q > 0xFF)
				{
					_logger.LogWarning("[IcedCpu] DIV overflow at EIP=0x{Eip:X8}", _eip);
					return;
				}
				
				var r = dividend % divisor;
				_eax = (_eax & 0xFFFF0000) | ((uint)r << 8) | (uint)q;
				break;
			}
			case 16:
			{
				// 16-bit: AX = DX:AX / r/m16, DX = DX:AX % r/m16
				var divisor = (ushort)ReadOp(insn, 0);
				if (divisor == 0)
				{
					_logger.LogWarning("[IcedCpu] Division by zero at EIP=0x{Eip:X8}", _eip);
					return;
				}
				
				var dividend = ((uint)(_edx & 0xFFFF) << 16) | (_eax & 0xFFFF);
				var q = dividend / divisor;
				if (q > 0xFFFF)
				{
					_logger.LogWarning("[IcedCpu] DIV overflow at EIP=0x{Eip:X8}", _eip);
					return;
				}
				
				var r = dividend % divisor;
				_eax = (_eax & 0xFFFF0000) | q;
				_edx = (_edx & 0xFFFF0000) | r;
				break;
			}
			default:
			{
				// 32-bit: EAX = EDX:EAX / r/m32, EDX = EDX:EAX % r/m32
				var divisor = ReadOp(insn, 0);
				if (divisor == 0)
				{
					_logger.LogWarning("[IcedCpu] Division by zero at EIP=0x{Eip:X8}", _eip);
					return;
				}

				var dividend = ((ulong)_edx << 32) | _eax;
				var q = dividend / divisor;
				if (q > 0xFFFFFFFFu)
				{
					_logger.LogWarning("[IcedCpu] DIV overflow at EIP=0x{Eip:X8}", _eip);
					return;
				}

				var r = (uint)(dividend % divisor);
				_eax = (uint)q;
				_edx = r;
				break;
			}
		}
	}

	private void ExecIdiv(Instruction insn)
	{
		// IDIV performs signed division
		// 8-bit form: AL = AX / r/m8, AH = AX % r/m8
		// 16-bit form: AX = DX:AX / r/m16, DX = DX:AX % r/m16
		// 32-bit form: EAX = EDX:EAX / r/m32, EDX = EDX:EAX % r/m32
		
		var opSize = GetOpSizeBits(insn, 0);
		
		switch (opSize)
		{
			case 8:
			{
				// 8-bit: AL = AX / r/m8, AH = AX % r/m8 (signed)
				var divisor = (sbyte)(byte)ReadOp(insn, 0);
				if (divisor == 0)
				{
					_logger.LogWarning("[IcedCpu] Division by zero at EIP=0x{Eip:X8}", _eip);
					return;
				}
				
				var dividend = (short)(ushort)(_eax & 0xFFFF);
				var q = dividend / divisor;
				if (q is > sbyte.MaxValue or < sbyte.MinValue)
				{
					_logger.LogWarning("[IcedCpu] IDIV overflow at EIP=0x{Eip:X8}", _eip);
					return;
				}
				
				var r = dividend % divisor;
				_eax = (_eax & 0xFFFF0000) | ((uint)(byte)r << 8) | (uint)(byte)q;
				break;
			}
			case 16:
			{
				// 16-bit: AX = DX:AX / r/m16, DX = DX:AX % r/m16 (signed)
				var divisor = (short)(ushort)ReadOp(insn, 0);
				if (divisor == 0)
				{
					_logger.LogWarning("[IcedCpu] Division by zero at EIP=0x{Eip:X8}", _eip);
					return;
				}
				
				var dividend = ((int)(short)(_edx & 0xFFFF) << 16) | (int)(_eax & 0xFFFF);
				var q = dividend / divisor;
				if (q is > short.MaxValue or < short.MinValue)
				{
					_logger.LogWarning("[IcedCpu] IDIV overflow at EIP=0x{Eip:X8}", _eip);
					return;
				}
				
				var r = dividend % divisor;
				_eax = (_eax & 0xFFFF0000) | (ushort)q;
				_edx = (_edx & 0xFFFF0000) | (ushort)r;
				break;
			}
			default:
			{
				// 32-bit: EAX = EDX:EAX / r/m32, EDX = EDX:EAX % r/m32 (signed)
				var divisor = (int)ReadOp(insn, 0);
				if (divisor == 0)
				{
					_logger.LogWarning("[IcedCpu] Division by zero at EIP=0x{Eip:X8}", _eip);
					return;
				}

				var dividend = ((long)_edx << 32) | _eax;
				var q = dividend / divisor;
				if (q is > int.MaxValue or < int.MinValue)
				{
					_logger.LogWarning("[IcedCpu] IDIV overflow at EIP=0x{Eip:X8}", _eip);
					return;
				}

				var r = (int)(dividend % divisor);
				_eax = (uint)(int)q;
				_edx = (uint)r;
				break;
			}
		}
	}

	private void ExecLeave()
	{
		_esp = _ebp;
		var ebpValue = Pop32();
		if (_eip >= 0x00403180 && _eip <= 0x004031A0)
		{
			_logger.LogWarning("[IcedCpu] ExecLeave: EBP=0x{Value:X8}, EIP=0x{Eip:X8}, ESP=0x{Esp:X8}",
				ebpValue, _eip, _esp);
		}
		_ebp = ebpValue;
	}

	private void ExecIn(Instruction insn)
	{
		// IN accumulator, port
		// We don't emulate I/O ports, so we'll just return 0.
		// This prevents crashes but may not be functionally correct for all programs.
		var opSize = GetOpSizeBits(insn, 0);
		switch (opSize)
		{
			case 8:
				SetReg8(Register.AL, 0);
				break;
			case 16:
				SetReg16(Register.AX, 0);
				break;
			default:
				_eax = 0;
				break;
		}
	}

	private void ExecOut(Instruction insn)
	{
		// OUT port, accumulator
		// We don't emulate I/O ports, so we'll just discard the value.
		// This prevents crashes but may not be functionally correct for all programs.
		// The instruction reads from AL/AX/EAX but we don't need to do anything with the value.
		// Just let the instruction complete successfully as a no-op.
		_logger.LogDebug("[IcedCpu] OUT instruction at EIP=0x{Eip:X8} - I/O port operations not emulated", _eip);
	}

	private void ExecFld(Instruction insn)
	{
		// FLD - Load floating point value
		if (insn.GetOpKind(0) == OpKind.Memory)
		{
			var addr = CalcMemAddress(insn);
			double val;
			if (insn.MemorySize == MemorySize.Float32)
			{
				val = BitConverter.Int32BitsToSingle(unchecked((int)_mem.Read32(addr)));
			}
			else if (insn.MemorySize == MemorySize.Float64)
			{
				var bits = _mem.Read64(addr);
				val = BitConverter.Int64BitsToDouble((long)bits);
			}
			else
			{
				// Assume 64-bit double
				var bits = _mem.Read64(addr);
				val = BitConverter.Int64BitsToDouble((long)bits);
			}
			FpuPush(val);
		}
		else if (insn.GetOpKind(0) == OpKind.Register)
		{
			// FLD ST(i)
			var reg = insn.GetOpRegister(0);
			var i = reg - Register.ST0;
			FpuPush(FpuGetSt(i));
		}
	}

	private void ExecFst(Instruction insn, bool pop)
	{
		// FST/FSTP - Store floating point value
		var val = FpuGetSt(0);
		
		if (insn.GetOpKind(0) == OpKind.Memory)
		{
			var addr = CalcMemAddress(insn);
			if (insn.MemorySize == MemorySize.Float32)
			{
				var bits = unchecked((uint)BitConverter.SingleToInt32Bits((float)val));
				_mem.Write32(addr, bits);
			}
			else
			{
				// Assume 64-bit double
				var bits = unchecked((ulong)BitConverter.DoubleToInt64Bits(val));
				_mem.Write64(addr, bits);
			}
		}
		else if (insn.GetOpKind(0) == OpKind.Register)
		{
			// FST/FSTP ST(i)
			var reg = insn.GetOpRegister(0);
			var i = reg - Register.ST0;
			FpuSetSt(i, val);
		}

		if (pop)
		{
			FpuPop();
		}
	}

	private void ExecFild(Instruction insn)
	{
		// FILD - Load integer to FPU stack
		var addr = CalcMemAddress(insn);
		double val;
		
		if (insn.MemorySize == MemorySize.Int16)
		{
			val = (short)_mem.Read16(addr);
		}
		else if (insn.MemorySize == MemorySize.Int32)
		{
			val = (int)_mem.Read32(addr);
		}
		else if (insn.MemorySize == MemorySize.Int64)
		{
			val = (long)_mem.Read64(addr);
		}
		else
		{
			// Default to 32-bit
			val = (int)_mem.Read32(addr);
		}
		
		FpuPush(val);
	}

	private void ExecFistp(Instruction insn)
	{
		// FISTP - Store integer and pop
		var val = FpuGetSt(0);
		var addr = CalcMemAddress(insn);
		
		// Get rounding mode from control word (bits 10-11)
		// For simplicity, we'll use standard rounding
		var rounded = Math.Round(val);
		
		if (insn.MemorySize == MemorySize.Int16)
		{
			// Store the signed 16-bit integer bit pattern as unsigned for memory representation
			_mem.Write16(addr, unchecked((ushort)(short)rounded));
		}
		else if (insn.MemorySize == MemorySize.Int32)
		{
			_mem.Write32(addr, unchecked((uint)(int)rounded));
		}
		else if (insn.MemorySize == MemorySize.Int64)
		{
			// Cast double -> long (truncate/round), then to ulong (bit pattern preserved, unchecked)
			_mem.Write64(addr, unchecked((ulong)(long)rounded));
		}
		else
		{
			// Default to 32-bit
			_mem.Write32(addr, unchecked((uint)(int)rounded));
		}
		
		FpuPop();
	}

	private void ExecFadd(Instruction insn)
	{
		// FADD - Add
		if (insn.OpCount == 0)
		{
			// FADD - Add ST(1) to ST(0)
			var st0 = FpuGetSt(0);
			var st1 = FpuGetSt(1);
			FpuSetSt(0, st0 + st1);
		}
		else if (insn.OpCount == 1)
		{
			if (insn.GetOpKind(0) == OpKind.Memory)
			{
				// FADD m32/m64 - Add memory to ST(0)
				var addr = CalcMemAddress(insn);
				double val;
				if (insn.MemorySize == MemorySize.Float32)
				{
					val = BitConverter.Int32BitsToSingle((int)_mem.Read32(addr));
				}
				else
				{
					var bits = _mem.Read64(addr);
					val = BitConverter.Int64BitsToDouble((long)bits);
				}
				FpuSetSt(0, FpuGetSt(0) + val);
			}
			else
			{
				// FADD ST(i) - Add ST(i) to ST(0)
				var reg = insn.GetOpRegister(0);
				var i = reg - Register.ST0;
				FpuSetSt(0, FpuGetSt(0) + FpuGetSt(i));
			}
		}
		else
		{
			// FADD ST(i), ST(0) - Add ST(0) to ST(i)
			var reg = insn.GetOpRegister(0);
			var i = reg - Register.ST0;
			FpuSetSt(i, FpuGetSt(i) + FpuGetSt(0));
		}
	}

	private void ExecFaddp(Instruction insn)
	{
		// FADDP - Add and pop
		if (insn.OpCount == 0)
		{
			// FADDP - Add ST(0) to ST(1) and pop
			var st0 = FpuGetSt(0);
			var st1 = FpuGetSt(1);
			FpuPop();
			FpuSetSt(0, st0 + st1);
		}
		else
		{
			// FADDP ST(i), ST(0) - Add ST(0) to ST(i) and pop
			var reg = insn.GetOpRegister(0);
			var i = reg - Register.ST0;
			FpuSetSt(i, FpuGetSt(i) + FpuGetSt(0));
			FpuPop();
		}
	}

	private void ExecFsub(Instruction insn)
	{
		// FSUB - Subtract
		if (insn.OpCount == 0)
		{
			// FSUB - Subtract ST(0) from ST(1)
			var st0 = FpuGetSt(0);
			var st1 = FpuGetSt(1);
			FpuSetSt(0, st1 - st0);
		}
		else if (insn.OpCount == 1)
		{
			if (insn.GetOpKind(0) == OpKind.Memory)
			{
				// FSUB m32/m64 - Subtract memory from ST(0)
				var addr = CalcMemAddress(insn);
				double val;
				if (insn.MemorySize == MemorySize.Float32)
				{
					val = BitConverter.Int32BitsToSingle((int)_mem.Read32(addr));
				}
				else
				{
					var bits = _mem.Read64(addr);
					val = BitConverter.Int64BitsToDouble((long)bits);
				}
				FpuSetSt(0, FpuGetSt(0) - val);
			}
			else
			{
				// FSUB ST(i) - Subtract ST(i) from ST(0)
				var reg = insn.GetOpRegister(0);
				var i = reg - Register.ST0;
				FpuSetSt(0, FpuGetSt(0) - FpuGetSt(i));
			}
		}
		else
		{
			// FSUB ST(i), ST(0) - Subtract ST(0) from ST(i)
			var reg = insn.GetOpRegister(0);
			var i = reg - Register.ST0;
			FpuSetSt(i, FpuGetSt(i) - FpuGetSt(0));
		}
	}

	private void ExecFmul(Instruction insn)
	{
		// FMUL - Multiply
		if (insn.OpCount == 0)
		{
			// FMUL - Multiply ST(0) by ST(1)
			var st0 = FpuGetSt(0);
			var st1 = FpuGetSt(1);
			FpuSetSt(0, st0 * st1);
		}
		else if (insn.OpCount == 1)
		{
			if (insn.GetOpKind(0) == OpKind.Memory)
			{
				// FMUL m32/m64 - Multiply ST(0) by memory
				var addr = CalcMemAddress(insn);
				double val;
				if (insn.MemorySize == MemorySize.Float32)
				{
					val = BitConverter.Int32BitsToSingle((int)_mem.Read32(addr));
				}
				else
				{
					var bits = _mem.Read64(addr);
					val = BitConverter.Int64BitsToDouble((long)bits);
				}
				FpuSetSt(0, FpuGetSt(0) * val);
			}
			else
			{
				// FMUL ST(i) - Multiply ST(0) by ST(i)
				var reg = insn.GetOpRegister(0);
				var i = reg - Register.ST0;
				FpuSetSt(0, FpuGetSt(0) * FpuGetSt(i));
			}
		}
		else
		{
			// FMUL ST(i), ST(0) - Multiply ST(i) by ST(0)
			var reg = insn.GetOpRegister(0);
			var i = reg - Register.ST0;
			FpuSetSt(i, FpuGetSt(i) * FpuGetSt(0));
		}
	}

	private void ExecFiadd(Instruction insn)
	{
		// FIADD - Add integer to ST(0)
		var addr = CalcMemAddress(insn);
		double val;
		
		if (insn.MemorySize == MemorySize.Int16)
		{
			val = (short)_mem.Read16(addr);
		}
		else
		{
			val = (int)_mem.Read32(addr);
		}
		
		FpuSetSt(0, FpuGetSt(0) + val);
	}

	private void ExecFimul(Instruction insn)
	{
		// FIMUL - Multiply ST(0) by integer
		var addr = CalcMemAddress(insn);
		double val;
		
		if (insn.MemorySize == MemorySize.Int16)
		{
			val = (short)_mem.Read16(addr);
		}
		else
		{
			val = (int)_mem.Read32(addr);
		}
		
		FpuSetSt(0, FpuGetSt(0) * val);
	}

	private void ExecFisub(Instruction insn)
	{
		// FISUB - Subtract integer from ST(0)
		var addr = CalcMemAddress(insn);
		double val;
		
		if (insn.MemorySize == MemorySize.Int16)
		{
			val = (short)_mem.Read16(addr);
		}
		else
		{
			val = (int)_mem.Read32(addr);
		}
		
		FpuSetSt(0, FpuGetSt(0) - val);
	}

	private void ExecFidiv(Instruction insn)
	{
		// FIDIV - Divide ST(0) by integer
		var addr = CalcMemAddress(insn);
		double val;
		
		if (insn.MemorySize == MemorySize.Int16)
		{
			val = (short)_mem.Read16(addr);
		}
		else
		{
			val = (int)_mem.Read32(addr);
		}
		
		FpuSetSt(0, FpuGetSt(0) / val);
	}

	private void ExecFidivr(Instruction insn)
	{
		// FIDIVR - Divide integer by ST(0) (reversed)
		var addr = CalcMemAddress(insn);
		double val;
		
		if (insn.MemorySize == MemorySize.Int16)
		{
			val = (short)_mem.Read16(addr);
		}
		else
		{
			val = (int)_mem.Read32(addr);
		}
		
		FpuSetSt(0, val / FpuGetSt(0));
	}

	private void ExecFxch(Instruction insn)
	{
		// FXCH - Exchange ST(0) with ST(i)
		var i = 1; // Default to ST(1)
		if (insn.OpCount > 0)
		{
			var reg = insn.GetOpRegister(0);
			i = reg - Register.ST0;
		}
		
		var st0 = FpuGetSt(0);
		var sti = FpuGetSt(i);
		FpuSetSt(0, sti);
		FpuSetSt(i, st0);
	}

	private void ExecFchs()
	{
		// FCHS - Change sign of ST(0)
		FpuSetSt(0, -FpuGetSt(0));
	}

	private void ExecFabs()
	{
		// FABS - Absolute value of ST(0)
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
		// FSIN - Sine of ST(0)
		FpuSetSt(0, Math.Sin(FpuGetSt(0)));
	}

	private void ExecFcos()
	{
		// FCOS - Cosine of ST(0)
		FpuSetSt(0, Math.Cos(FpuGetSt(0)));
	}

	private void ExecFsincos()
	{
		// FSINCOS - Sine and cosine of ST(0)
		var st0 = FpuGetSt(0);
		FpuSetSt(0, Math.Sin(st0));
		FpuPush(Math.Cos(st0));
	}

	private void ExecFpatan()
	{
		// FPATAN - Partial arctangent: ST(1) = atan2(ST(1), ST(0)), then pop
		var st0 = FpuGetSt(0);
		var st1 = FpuGetSt(1);
		FpuPop();
		FpuSetSt(0, Math.Atan2(st1, st0));
	}

	private void ExecF2xm1()
	{
		// F2XM1 - Compute 2^x - 1 where x is ST(0)
		var st0 = FpuGetSt(0);
		FpuSetSt(0, Math.Pow(2, st0) - 1);
	}

	private void ExecFscale()
	{
		// FSCALE - Scale ST(0) by powers of 2: ST(0) = ST(0) * 2^floor(ST(1))
		var st0 = FpuGetSt(0);
		var st1 = FpuGetSt(1);
		FpuSetSt(0, st0 * Math.Pow(2, Math.Floor(st1)));
	}

	private void ExecFucomi(Instruction insn)
	{
		// FUCOMI - Compare ST(0) with ST(i) and set EFLAGS
		var i = 1; // Default to ST(1)
		if (insn.OpCount > 0)
		{
			var reg = insn.GetOpRegister(0);
			i = reg - Register.ST0;
		}
		
		var st0 = FpuGetSt(0);
		var sti = FpuGetSt(i);
		
		// Set EFLAGS based on comparison
		if (double.IsNaN(st0) || double.IsNaN(sti))
		{
			SetFlag(Zf);
			SetFlag(Pf);
			SetFlag(Cf);
		}
		else if (st0 > sti)
		{
			ClearFlag(Zf);
			ClearFlag(Pf);
			ClearFlag(Cf);
		}
		else if (st0 < sti)
		{
			ClearFlag(Zf);
			ClearFlag(Pf);
			SetFlag(Cf);
		}
		else // st0 == sti
		{
			SetFlag(Zf);
			ClearFlag(Pf);
			ClearFlag(Cf);
		}
	}

	private void ExecFucomip(Instruction insn)
	{
		// FUCOMIP - Compare ST(0) with ST(i), set EFLAGS, and pop
		ExecFucomi(insn);
		FpuPop();
	}

	private void ExecFcomp(Instruction insn)
	{
		// FCOMP - Compare ST(0) with source and pop
		// The source can be a memory operand (float32/float64) or ST(i)
		// Note: FCOMP is an ordered comparison (unlike FUCOMP which is unordered)
		// In a full x87 implementation, this would set C0, C2, C3 in the FPU status word
		// For simplicity in this emulator, we set EFLAGS like FUCOMIP does
		double st0 = FpuGetSt(0);
		double source;
		
		if (insn.OpCount == 0)
		{
			// FCOMP with no operand defaults to ST(1)
			source = FpuGetSt(1);
		}
		else if (insn.GetOpKind(0) == OpKind.Memory)
		{
			// FCOMP m32/m64 - Compare with memory
			var addr = CalcMemAddress(insn);
			if (insn.MemorySize == MemorySize.Float32)
			{
				source = BitConverter.Int32BitsToSingle((int)_mem.Read32(addr));
			}
			else
			{
				// Handle Float64 and unspecified memory sizes (default to 64-bit double)
				var bits = _mem.Read64(addr);
				source = BitConverter.Int64BitsToDouble((long)bits);
			}
		}
		else
		{
			// FCOMP ST(i) - Compare with ST(i)
			var reg = insn.GetOpRegister(0);
			var i = reg - Register.ST0;
			source = FpuGetSt(i);
		}
		
		// Set EFLAGS based on comparison (similar to FUCOMIP)
		if (double.IsNaN(st0) || double.IsNaN(source))
		{
			SetFlag(Zf);
			SetFlag(Pf);
			SetFlag(Cf);
		}
		else if (st0 > source)
		{
			ClearFlag(Zf);
			ClearFlag(Pf);
			ClearFlag(Cf);
		}
		else if (st0 < source)
		{
			ClearFlag(Zf);
			ClearFlag(Pf);
			SetFlag(Cf);
		}
		else // st0 == source
		{
			SetFlag(Zf);
			ClearFlag(Pf);
			ClearFlag(Cf);
		}
		
		// Pop the stack
		FpuPop();
	}

	private void ExecFcom(Instruction insn)
	{
		// FCOM - Compare ST(0) with source (no pop)
		// Similar to FCOMP but doesn't pop the stack
		double st0 = FpuGetSt(0);
		double source;
		
		if (insn.OpCount == 0)
		{
			// FCOM with no operand defaults to ST(1)
			source = FpuGetSt(1);
		}
		else if (insn.GetOpKind(0) == OpKind.Memory)
		{
			// FCOM m32/m64 - Compare with memory
			var addr = CalcMemAddress(insn);
			if (insn.MemorySize == MemorySize.Float32)
			{
				source = BitConverter.Int32BitsToSingle((int)_mem.Read32(addr));
			}
			else
			{
				// Handle Float64 and unspecified memory sizes (default to 64-bit double)
				var bits = _mem.Read64(addr);
				source = BitConverter.Int64BitsToDouble((long)bits);
			}
		}
		else
		{
			// FCOM ST(i) - Compare with ST(i)
			var reg = insn.GetOpRegister(0);
			var i = reg - Register.ST0;
			source = FpuGetSt(i);
		}
		
		// Set EFLAGS based on comparison
		if (double.IsNaN(st0) || double.IsNaN(source))
		{
			SetFlag(Zf);
			SetFlag(Pf);
			SetFlag(Cf);
		}
		else if (st0 > source)
		{
			ClearFlag(Zf);
			ClearFlag(Pf);
			ClearFlag(Cf);
		}
		else if (st0 < source)
		{
			ClearFlag(Zf);
			ClearFlag(Pf);
			SetFlag(Cf);
		}
		else // st0 == source
		{
			SetFlag(Zf);
			ClearFlag(Pf);
			ClearFlag(Cf);
		}
		// No pop for FCOM
	}

	private void ExecFcompp()
	{
		// FCOMPP - Compare ST(0) with ST(1) and pop twice
		double st0 = FpuGetSt(0);
		double st1 = FpuGetSt(1);
		
		// Set EFLAGS based on comparison
		if (double.IsNaN(st0) || double.IsNaN(st1))
		{
			SetFlag(Zf);
			SetFlag(Pf);
			SetFlag(Cf);
		}
		else if (st0 > st1)
		{
			ClearFlag(Zf);
			ClearFlag(Pf);
			ClearFlag(Cf);
		}
		else if (st0 < st1)
		{
			ClearFlag(Zf);
			ClearFlag(Pf);
			SetFlag(Cf);
		}
		else // st0 == st1
		{
			SetFlag(Zf);
			ClearFlag(Pf);
			ClearFlag(Cf);
		}
		
		// Pop twice
		FpuPop();
		FpuPop();
	}

	private void ExecFsubp(Instruction insn)
	{
		// FSUBP - Subtract and pop
		if (insn.OpCount == 0)
		{
			// FSUBP - Subtract ST(0) from ST(1) and pop
			var st0 = FpuGetSt(0);
			var st1 = FpuGetSt(1);
			FpuPop();
			FpuSetSt(0, st1 - st0);
		}
		else
		{
			// FSUBP ST(i), ST(0) - Subtract ST(0) from ST(i) and pop
			var reg = insn.GetOpRegister(0);
			var i = reg - Register.ST0;
			FpuSetSt(i, FpuGetSt(i) - FpuGetSt(0));
			FpuPop();
		}
	}

	private void ExecFsubr(Instruction insn)
	{
		// FSUBR - Reverse subtract (subtract ST(0) from source)
		if (insn.OpCount == 0)
		{
			// FSUBR - Subtract ST(0) from ST(1), store in ST(0)
			var st0 = FpuGetSt(0);
			var st1 = FpuGetSt(1);
			FpuSetSt(0, st1 - st0);
		}
		else if (insn.OpCount == 1)
		{
			if (insn.GetOpKind(0) == OpKind.Memory)
			{
				// FSUBR m32/m64 - Subtract ST(0) from memory
				var addr = CalcMemAddress(insn);
				double val;
				if (insn.MemorySize == MemorySize.Float32)
				{
					val = BitConverter.Int32BitsToSingle((int)_mem.Read32(addr));
				}
				else
				{
					var bits = _mem.Read64(addr);
					val = BitConverter.Int64BitsToDouble((long)bits);
				}
				FpuSetSt(0, val - FpuGetSt(0));
			}
			else
			{
				// FSUBR ST(i) - Subtract ST(0) from ST(i), store in ST(0)
				var reg = insn.GetOpRegister(0);
				var i = reg - Register.ST0;
				FpuSetSt(0, FpuGetSt(i) - FpuGetSt(0));
			}
		}
		else
		{
			// FSUBR ST(i), ST(0) - Subtract ST(0) from ST(i)
			var reg = insn.GetOpRegister(0);
			var i = reg - Register.ST0;
			FpuSetSt(i, FpuGetSt(i) - FpuGetSt(0));
		}
	}

	private void ExecFsubrp(Instruction insn)
	{
		// FSUBRP - Reverse subtract and pop
		if (insn.OpCount == 0)
		{
			// FSUBRP - Subtract ST(0) from ST(1) and pop
			var st0 = FpuGetSt(0);
			var st1 = FpuGetSt(1);
			FpuPop();
			FpuSetSt(0, st1 - st0);
		}
		else
		{
			// FSUBRP ST(i), ST(0) - Subtract ST(0) from ST(i) and pop
			var reg = insn.GetOpRegister(0);
			var i = reg - Register.ST0;
			FpuSetSt(i, FpuGetSt(i) - FpuGetSt(0));
			FpuPop();
		}
	}

	private void ExecFmulp(Instruction insn)
	{
		// FMULP - Multiply and pop
		if (insn.OpCount == 0)
		{
			// FMULP - Multiply ST(0) by ST(1) and pop
			var st0 = FpuGetSt(0);
			var st1 = FpuGetSt(1);
			FpuPop();
			FpuSetSt(0, st0 * st1);
		}
		else
		{
			// FMULP ST(i), ST(0) - Multiply ST(i) by ST(0) and pop
			var reg = insn.GetOpRegister(0);
			var i = reg - Register.ST0;
			FpuSetSt(i, FpuGetSt(i) * FpuGetSt(0));
			FpuPop();
		}
	}

	private void ExecFdiv(Instruction insn)
	{
		// FDIV - Divide
		if (insn.OpCount == 0)
		{
			// FDIV - Divide ST(0) by ST(1)
			var st0 = FpuGetSt(0);
			var st1 = FpuGetSt(1);
			FpuSetSt(0, st0 / st1);
		}
		else if (insn.OpCount == 1)
		{
			if (insn.GetOpKind(0) == OpKind.Memory)
			{
				// FDIV m32/m64 - Divide ST(0) by memory
				var addr = CalcMemAddress(insn);
				double val;
				if (insn.MemorySize == MemorySize.Float32)
				{
					val = BitConverter.Int32BitsToSingle((int)_mem.Read32(addr));
				}
				else
				{
					var bits = _mem.Read64(addr);
					val = BitConverter.Int64BitsToDouble((long)bits);
				}
				FpuSetSt(0, FpuGetSt(0) / val);
			}
			else
			{
				// FDIV ST(i) - Divide ST(0) by ST(i)
				var reg = insn.GetOpRegister(0);
				var i = reg - Register.ST0;
				FpuSetSt(0, FpuGetSt(0) / FpuGetSt(i));
			}
		}
		else
		{
			// FDIV ST(i), ST(0) - Divide ST(i) by ST(0)
			var reg = insn.GetOpRegister(0);
			var i = reg - Register.ST0;
			FpuSetSt(i, FpuGetSt(i) / FpuGetSt(0));
		}
	}

	private void ExecFdivp(Instruction insn)
	{
		// FDIVP - Divide and pop
		if (insn.OpCount == 0)
		{
			// FDIVP - Divide ST(1) by ST(0) and pop
			var st0 = FpuGetSt(0);
			var st1 = FpuGetSt(1);
			FpuPop();
			FpuSetSt(0, st1 / st0);
		}
		else
		{
			// FDIVP ST(i), ST(0) - Divide ST(i) by ST(0) and pop
			var reg = insn.GetOpRegister(0);
			var i = reg - Register.ST0;
			var st0 = FpuGetSt(0);
			var sti = FpuGetSt(i);
			FpuPop();
			FpuSetSt(i - 1, sti / st0);
		}
	}

	private void ExecFdivr(Instruction insn)
	{
		// FDIVR - Reverse divide (divide source by ST(0))
		if (insn.OpCount == 0)
		{
			// FDIVR - Divide ST(1) by ST(0), store in ST(0)
			var st0 = FpuGetSt(0);
			var st1 = FpuGetSt(1);
			FpuSetSt(0, st1 / st0);
		}
		else if (insn.OpCount == 1)
		{
			if (insn.GetOpKind(0) == OpKind.Memory)
			{
				// FDIVR m32/m64 - Divide memory by ST(0)
				var addr = CalcMemAddress(insn);
				double val;
				if (insn.MemorySize == MemorySize.Float32)
				{
					val = BitConverter.Int32BitsToSingle((int)_mem.Read32(addr));
				}
				else
				{
					var bits = _mem.Read64(addr);
					val = BitConverter.Int64BitsToDouble((long)bits);
				}
				FpuSetSt(0, val / FpuGetSt(0));
			}
			else
			{
				// FDIVR ST(i) - Divide ST(i) by ST(0), store in ST(0)
				var reg = insn.GetOpRegister(0);
				var i = reg - Register.ST0;
				FpuSetSt(0, FpuGetSt(i) / FpuGetSt(0));
			}
		}
		else
		{
			// FDIVR ST(i), ST(0) - Divide ST(0) by ST(i)
			var reg = insn.GetOpRegister(0);
			var i = reg - Register.ST0;
			FpuSetSt(i, FpuGetSt(0) / FpuGetSt(i));
		}
	}

	private void ExecFdivrp(Instruction insn)
	{
		// FDIVRP - Reverse divide and pop
		if (insn.OpCount == 0)
		{
			// FDIVRP - Divide ST(1) by ST(0) and pop
			var st0 = FpuGetSt(0);
			var st1 = FpuGetSt(1);
			FpuPop();
			FpuSetSt(0, st1 / st0);
		}
		else
		{
			// FDIVRP ST(i), ST(0) - Divide ST(0) by ST(i) and pop
			var reg = insn.GetOpRegister(0);
			var i = reg - Register.ST0;
			var st0 = FpuGetSt(0);
			var sti = FpuGetSt(i);
			FpuPop();
			FpuSetSt(i - 1, st0 / sti);
		}
	}

	private void ExecFsqrt()
	{
		// FSQRT - Square root of ST(0)
		FpuSetSt(0, Math.Sqrt(FpuGetSt(0)));
	}

	private void ExecFist(Instruction insn)
	{
		// FIST - Store integer (no pop)
		var val = FpuGetSt(0);
		var addr = CalcMemAddress(insn);
		
		// Get rounding mode from control word (for simplicity, use standard rounding)
		var rounded = Math.Round(val);
		
		if (insn.MemorySize == MemorySize.Int16)
		{
			_mem.Write16(addr, unchecked((ushort)(short)rounded));
		}
		else if (insn.MemorySize == MemorySize.Int32)
		{
			_mem.Write32(addr, unchecked((uint)(int)rounded));
		}
		else
		{
			// Default to 32-bit
			_mem.Write32(addr, unchecked((uint)(int)rounded));
		}
		// No pop for FIST
	}

	private void ExecFcmovnbe(Instruction insn)
	{
		// FCMOVNBE - Conditional move if not below or equal (CF=0 and ZF=0)
		if (!GetFlag(Cf) && !GetFlag(Zf))
		{
			var reg = insn.GetOpRegister(1);
			var i = reg - Register.ST0;
			FpuSetSt(0, FpuGetSt(i));
		}
	}

	private void ExecFnstcw(Instruction insn)
	{
		// FNSTCW - Store FPU control word
		var addr = CalcMemAddress(insn);
		_mem.Write16(addr, _fpuControlWord);
	}

	private void ExecFldcw(Instruction insn)
	{
		// FLDCW - Load FPU control word
		var addr = CalcMemAddress(insn);
		_fpuControlWord = _mem.Read16(addr);
	}

	private void ExecFnstsw(Instruction insn)
	{
		// FNSTSW - Store FPU status word (no wait)
		// Can store to memory or to AX register
		if (insn.OpCount == 0 || insn.GetOpKind(0) == OpKind.Register)
		{
			// FNSTSW AX - Store to AX register
			_eax = (_eax & 0xFFFF0000) | _fpuStatusWord;
		}
		else
		{
			// FNSTSW mem16 - Store to memory
			var addr = CalcMemAddress(insn);
			_mem.Write16(addr, _fpuStatusWord);
		}
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
		// FNCLEX/FCLEX - Clear FPU exceptions (no wait)
		// Clear all exception flags in status word
		_fpuStatusWord &= 0xFF00; // Clear the lower 8 bits which contain exception flags
	}

	private void ExecFxam()
	{
		// FXAM - Examine ST(0) and set condition codes in status word
		var st0 = FpuGetSt(0);
		
		// Clear C0, C2, C3 bits (bits 8, 10, 14)
		_fpuStatusWord &= 0xB8FF;
		
		// Set condition codes based on ST(0) value
		if (double.IsNaN(st0))
		{
			// NaN: C0=0, C2=0, C3=0
			// Already cleared above
		}
		else if (double.IsInfinity(st0))
		{
			// Infinity: C0=1, C2=1, C3=0
			_fpuStatusWord |= 0x0500; // Set C0 and C2
		}
		else if (st0 == 0.0)
		{
			// Zero: C0=0, C2=0, C3=1
			_fpuStatusWord |= 0x4000; // Set C3
		}
		else
		{
			// Normal: C0=1, C2=0, C3=0
			_fpuStatusWord |= 0x0100; // Set C0
		}
		
		// Set sign bit (C1, bit 9) based on sign of ST(0)
		if ((BitConverter.DoubleToInt64Bits(st0) & (1L << 63)) != 0) // Handle negative zero and negative numbers
		{
			_fpuStatusWord |= 0x0200; // Set C1 (sign bit)
		}
	}

	/// <summary>
	/// Sets OF flag for BT family instructions based on hardware behavior.
	/// Per https://github.com/SingleStepTests/80386/issues/4:
	/// Rotate the source value right by bit index, then set OF to XOR of top two bits.
	/// </summary>
	private void SetBtFamilyOfFlag(uint bitBase, int bitPos, int opSize)
	{
		var rotated = ((bitBase >> bitPos) | (bitBase << (opSize - bitPos))) & ((1u << opSize) - 1);
		var topBit = (rotated >> (opSize - 1)) & 1;
		var secondBit = (rotated >> (opSize - 2)) & 1;
		SetFlagVal(Of, (topBit ^ secondBit) != 0);
	}

	private void ExecBt(Instruction insn)
	{
		// BT - Bit test
		var bitBase = ReadOp(insn, 0);
		var bitOffset = ReadOp(insn, 1);
		int opSize = GetOpSizeBits(insn, 0); // Get destination operand size
		uint mask = opSize == 16 ? 0xFu : 0x1Fu;
		var bitPos = (int)(bitOffset & mask);
		var bitValue = (bitBase >> bitPos) & 1;
		SetFlagVal(Cf, bitValue != 0);
		SetBtFamilyOfFlag(bitBase, bitPos, opSize);
	}

	private void ExecBts(Instruction insn)
	{
		// BTS - Bit test and set
		var bitBase = ReadOp(insn, 0);
		var bitOffset = ReadOp(insn, 1);
		int opSize = GetOpSizeBits(insn, 0); // Get destination operand size
		uint mask = opSize == 16 ? 0xFu : 0x1Fu;
		var bitPos = (int)(bitOffset & mask);
		var bitValue = (bitBase >> bitPos) & 1;
		SetFlagVal(Cf, bitValue != 0);
		SetBtFamilyOfFlag(bitBase, bitPos, opSize);
		
		// Set the bit
		bitBase |= (1u << bitPos);
		WriteOp(insn, 0, bitBase);
	}

	private void ExecBtr(Instruction insn)
	{
		// BTR - Bit test and reset
		var bitBase = ReadOp(insn, 0);
		var bitOffset = ReadOp(insn, 1);
		int opSize = GetOpSizeBits(insn, 0); // Get destination operand size
		uint mask = opSize == 16 ? 0xFu : 0x1Fu;
		var bitPos = (int)(bitOffset & mask);
		var bitValue = (bitBase >> bitPos) & 1;
		SetFlagVal(Cf, bitValue != 0);
		SetBtFamilyOfFlag(bitBase, bitPos, opSize);
		
		// Reset (clear) the bit
		bitBase &= ~(1u << bitPos);
		WriteOp(insn, 0, bitBase);
	}

	private void ExecBtc(Instruction insn)
	{
		// BTC - Bit test and complement
		var bitBase = ReadOp(insn, 0);
		var bitOffset = ReadOp(insn, 1);
		int opSize = GetOpSizeBits(insn, 0); // Get destination operand size
		uint mask = opSize == 16 ? 0xFu : 0x1Fu;
		var bitPos = (int)(bitOffset & mask);
		var bitValue = (bitBase >> bitPos) & 1;
		SetFlagVal(Cf, bitValue != 0);
		SetBtFamilyOfFlag(bitBase, bitPos, opSize);
		
		// Complement (toggle) the bit
		bitBase ^= (1u << bitPos);
		WriteOp(insn, 0, bitBase);
	}

	private void ExecBsf(Instruction insn)
	{
		// BSF - Bit Scan Forward
		// Scans source operand for first set bit (starting from bit 0)
		// If found, stores bit index in destination and clears ZF
		// If not found (source is 0), sets ZF and destination is undefined
		var src = ReadOp(insn, 1);
		int opSize = GetSourceSizeBits(insn);
		
		// Mask source to operand size
		if (opSize == 16)
		{
			src &= 0xFFFF;
		}
		else if (opSize == 8)
		{
			src &= 0xFF;
		}

		if (src == 0)
		{
			// No bits set - set ZF, destination is undefined (we'll leave it unchanged)
			SetFlagVal(Zf, true);
		}
		else
		{
			// Find first set bit using hardware intrinsic
			uint bitIndex = (uint)BitOperations.TrailingZeroCount(src);
			WriteOp(insn, 0, bitIndex);
			SetFlagVal(Zf, false);
		}
	}

	private void ExecBsr(Instruction insn)
	{
		// BSR - Bit Scan Reverse
		// Scans source operand for last set bit (starting from MSB down to bit 0)
		// If found, stores bit index in destination and clears ZF
		// If not found (source is 0), sets ZF and destination is undefined
		var src = ReadOp(insn, 1);
		int opSize = GetSourceSizeBits(insn);
		
		// Mask source to operand size
		if (opSize == 16)
		{
			src &= 0xFFFF;
		}
		else if (opSize == 8)
		{
			src &= 0xFF;
		}

		if (src == 0)
		{
			// No bits set - set ZF, destination is undefined (we'll leave it unchanged)
			SetFlagVal(Zf, true);
		}
		else
		{
			// Find last set bit using hardware intrinsic
			// LeadingZeroCount returns count from MSB, we need position from LSB
			uint bitIndex;
			if (opSize == 16)
			{
				// For 16-bit: count leading zeros in a 16-bit value
				bitIndex = (uint)(15 - (BitOperations.LeadingZeroCount(src) - 16));
			}
			else if (opSize == 8)
			{
				// For 8-bit: count leading zeros in an 8-bit value
				bitIndex = (uint)(7 - (BitOperations.LeadingZeroCount(src) - 24));
			}
			else
			{
				// For 32-bit
				bitIndex = (uint)(31 - BitOperations.LeadingZeroCount(src));
			}
			
			WriteOp(insn, 0, bitIndex);
			SetFlagVal(Zf, false);
		}
	}

	private void ExecShld(Instruction insn)
	{
		// SHLD - Double precision shift left
		var dest = ReadOp(insn, 0);
		var src = ReadOp(insn, 1);
		var count = (byte)(insn.Op2Kind == OpKind.Immediate8 ? insn.Immediate8 : (_ecx & 0x1F));
		
		if (count == 0)
		{
			return;
		}

		count &= 0x1F; // Modulo 32
		
		// Get operand size - SHLD can be 16-bit or 32-bit
		var opSize = GetOpSizeBits(insn, 0);
		var signBit = 1u << (opSize - 1);
		
		// Save original MSB for OF calculation
		var originalMsb = (dest & signBit) != 0;
		
		// Calculate mask - avoid overflow for 32-bit case
		var mask = opSize == 32 ? 0xFFFFFFFFu : (1u << opSize) - 1;
		
		// Mask operands to size
		dest &= mask;
		src &= mask;
		
		// CF is set to the last bit shifted out of dest
		bool carryOut;
		if (count < opSize)
		{
			// Normal case: last bit shifted out from dest
			carryOut = ((dest >> (opSize - count)) & 1) != 0;
		}
		else
		{
			// When count >= opSize, the last bit shifted out wraps around
			var rotateAmount = count % opSize;
			if (rotateAmount == 0)
			{
				// Full rotation, CF comes from LSB of src
				carryOut = (src & 1) != 0;
			}
			else
			{
				// Use rotation to find the carry bit
				var rotatedSrc = RotateLeft(src, rotateAmount, opSize);
				carryOut = (rotatedSrc & 1) != 0;
			}
		}
		
		// For SHLD, shift dest left and bring in bits from src
		// Per https://github.com/SingleStepTests/80386/issues/4:
		// When count >= opSize, the "inBits" are rotated (ROL style) from src
		if (count >= opSize)
		{
			// When count >= opSize, rotate src left by (count mod opSize)
			var rotateAmount = count % opSize;
			if (rotateAmount == 0)
			{
				// Full rotation by opSize returns src unchanged
				dest = src;
			}
			else
			{
				dest = RotateLeft(src, rotateAmount, opSize);
			}
		}
		else
		{
			// Normal case: shift dest left by count and bring in high bits from src
			// Take high 'count' bits from src and place them in low positions of result
			uint inBits = src >> (opSize - count);
			dest = (dest << count) | inBits;
			dest &= mask;
		}
		
		// Set flags
		SetFlagVal(Cf, carryOut);
		// OF is calculated based on whether the two MSB bits differ after the shift
		// This matches MAME's implementation for consistency
		var msb = (dest >> (opSize - 1)) & 1;
		var nextMsb = (dest >> (opSize - 2)) & 1;
		SetFlagVal(Of, msb != nextMsb);
		// AF is undefined for SHLD
		
		WriteOp(insn, 0, dest);
		// Update SF, ZF, and PF based on result
		UpdateLogicResultFlags(dest, signBit);
	}

	private void ExecShrd(Instruction insn)
	{
		// SHRD - Double precision shift right
		var dest = ReadOp(insn, 0);
		var src = ReadOp(insn, 1);
		var count = (byte)(insn.Op2Kind == OpKind.Immediate8 ? insn.Immediate8 : (_ecx & 0x1F));
		
		if (count == 0)
		{
			return;
		}

		count &= 0x1F; // Modulo 32
		
		// Get operand size - SHRD can be 16-bit or 32-bit
		var opSize = GetOpSizeBits(insn, 0);
		var signBit = 1u << (opSize - 1);
		
		// Save original MSB for OF calculation
		var originalMsb = (dest & signBit) != 0;
		
		// Calculate mask - avoid overflow for 32-bit case
		var mask = opSize == 32 ? 0xFFFFFFFFu : (1u << opSize) - 1;
		
		// Mask operands to size
		dest &= mask;
		src &= mask;
		
		// CF is set to the last bit shifted out of dest
		bool carryOut;
		if (count <= opSize)
		{
			// Normal case: last bit shifted out from dest
			carryOut = ((dest >> (count - 1)) & 1) != 0;
		}
		else
		{
			// When count > opSize, need to account for wrap-around
			var rotateAmount = count % opSize;
			if (rotateAmount == 0)
			{
				// Full rotation, CF comes from MSB of src
				carryOut = (src & signBit) != 0;
			}
			else
			{
				// Use rotation to find the carry bit
				var rotatedSrc = RotateRight(src, rotateAmount, opSize);
				carryOut = ((rotatedSrc >> (opSize - 1)) & 1) != 0;
			}
		}
		
		// For SHRD, shift dest right and bring in bits from src
		// Per https://github.com/SingleStepTests/80386/issues/4:
		// When count >= opSize, the "inBits" are rotated (ROR style) from src
		if (count >= opSize)
		{
			// When count >= opSize, rotate src right by (count mod opSize)
			var rotateAmount = count % opSize;
			if (rotateAmount == 0)
			{
				// Full rotation by opSize returns src unchanged
				dest = src;
			}
			else
			{
				dest = RotateRight(src, rotateAmount, opSize);
			}
		}
		else
		{
			// Normal case: shift dest right by count and bring in high bits from src
			// Take low 'count' bits from src and place them in high positions of result
			// Create mask for low 'count' bits
			// count is already masked to 0x1F and > 0, so (1u << count) - 1 is safe
			var lowBitsMask = (1u << count) - 1;
			uint inBits = (src & lowBitsMask) << (opSize - count);
			dest = (dest >> count) | inBits;
			dest &= mask;
		}
		
		// Set flags
		SetFlagVal(Cf, carryOut);
		// OF is calculated based on whether the two MSB bits differ after the shift
		// This matches MAME's implementation: m_OF = ((dst >> 31) ^ (dst >> 30)) & 1;
		// For 16-bit operations, use bits 15 and 14
		var msb = (dest >> (opSize - 1)) & 1;
		var nextMsb = (dest >> (opSize - 2)) & 1;
		SetFlagVal(Of, msb != nextMsb);
		// AF is undefined for SHRD
		
		WriteOp(insn, 0, dest);
		// Update SF, ZF, and PF based on result
		UpdateLogicResultFlags(dest, signBit);
	}

	private void ExecAad(Instruction insn)
	{
		// AAD - ASCII Adjust AX Before Division
		// Converts unpacked BCD in AX to binary
		// Formula: AL = AH * base + AL, AH = 0
		// The base is typically 10 (0x0A) but can be specified in the instruction
		byte base_;
		// If the instruction has no immediate operand, use default base 10.
		// If immediate is present (even if 0), use it as the base.
		if (insn.OpCount == 0)
		{
			base_ = 10;
		}
		else
		{
			base_ = insn.Immediate8;
		}

		var al = (byte)(_eax & 0xFF);
		var ah = (byte)((_eax >> 8) & 0xFF);
		
		al = (byte)(ah * base_ + al);
		ah = 0;
		
		_eax = (_eax & 0xFFFF0000) | ((uint)ah << 8) | al;
		
		// Update flags: SF, ZF, PF (OF, AF, CF are undefined)
		UpdateLogicResultFlags(al, 0x80);  // Use 0x80 sign bit mask for byte operations
	}

	private void ExecAam(Instruction insn)
	{
		// AAM - ASCII Adjust AX After Multiply
		// Converts binary in AL to unpacked BCD in AX
		// Formula: AH = AL / base, AL = AL % base
		// The base is typically 10 (0x0A) but can be specified in the instruction
		var base_ = insn.Immediate8;
		if (base_ == 0)
		{
			base_ = 10; // Default base is 10
		}

		var al = (byte)(_eax & 0xFF);
		
		var ah = (byte)(al / base_);
		al = (byte)(al % base_);
		
		_eax = (_eax & 0xFFFF0000) | ((uint)ah << 8) | al;
		
		// Update flags: SF, ZF, PF (OF, AF, CF are undefined)
		UpdateLogicResultFlags(al, 0x80);  // Use 0x80 sign bit mask for byte operations
	}

	private void ExecAas()
	{
		// AAS - ASCII Adjust AL After Subtraction
		// Adjusts result of unpacked BCD subtraction
		var al = (byte)(_eax & 0xFF);
		var ah = (byte)((_eax >> 8) & 0xFF);
		
		// Check if adjustment is needed
		if (((al & 0x0F) > 9) || GetFlag(Af))
		{
			// Adjust AL and AH
			al -= 6;
			ah -= 1;
			SetFlag(Af);
			SetFlag(Cf);
		}
		else
		{
			ClearFlag(Af);
			ClearFlag(Cf);
		}
		
		// Clear high nibble of AL
		al &= 0x0F;
		
		_eax = (_eax & 0xFFFF0000) | ((uint)ah << 8) | al;
		
		// Update flags: SF, ZF, PF (technically undefined per Intel spec, but set for consistency)
		// This matches the behavior of other BCD instructions (AAD, AAM, DAS, DAA) in this emulator
		UpdateLogicResultFlags(al, 0x80);  // Use 0x80 sign bit mask for byte operations
	}

	private void ExecDas()
	{
		// DAS - Decimal Adjust AL After Subtraction
		// Adjusts AL after packed BCD subtraction
		// Reference: Intel® 64 and IA-32 Architectures Software Developer's Manual
		//
		// Pseudocode from Intel manual:
		// OLD_AL ← AL;
		// OLD_CF ← CF;
		// CF ← 0;
		// IF (AL AND 0FH) > 9 OR AF = 1 THEN
		//     AL ← AL - 6;
		//     CF ← OLD_CF OR (Borrow from AL ← AL - 6);
		//     AF ← 1;
		// ELSE
		//     AF ← 0;
		// FI;
		// IF OLD_AL > 99H OR OLD_CF = 1 THEN
		//     AL ← AL - 60H;
		//     CF ← 1;
		// FI;
		
		var oldAl = (byte)(_eax & 0xFF);
		var oldCf = GetFlag(Cf);
		var oldAf = GetFlag(Af);
		var al = oldAl;
		
		// CF is initially cleared per Intel spec
		ClearFlag(Cf);
		
		// Step 1: Check low nibble
		if (((al & 0x0F) > 9) || oldAf)
		{
			// Perform subtraction and check for borrow (underflow)
			var newAl = (byte)(al - 6);
			var borrow = (newAl > al);  // Underflow: result wrapped around
			al = newAl;
			
			// CF = OLD_CF OR Borrow
			SetFlagVal(Cf, oldCf || borrow);
			SetFlag(Af);
		}
		else
		{
			ClearFlag(Af);
		}
		
		// Step 2: Check high nibble
		if ((oldAl > 0x99) || oldCf)
		{
			al -= 0x60;
			SetFlag(Cf);
		}
		
		_eax = (_eax & 0xFFFFFF00) | al;
		
		// Update flags: SF, ZF, PF
		UpdateLogicResultFlags(al, 0x80);  // Use 0x80 sign bit mask for byte operations
		
		// OF is officially "undefined" per Intel docs, but real 80386 hardware sets it
		// when the adjustment causes a transition from negative to positive (signed overflow).
		// This matches the behavior observed in hardware-generated SingleStepTests.
		// For DAS (subtraction), OF = 1 when: old AL was negative AND new AL is positive
		SetFlagVal(Of, ((oldAl & 0x80) != 0) && ((al & 0x80) == 0));
	}

	private void ExecDaa()
	{
		// DAA - Decimal Adjust AL After Addition
		// Adjusts AL after packed BCD addition
		// Reference: Intel® 64 and IA-32 Architectures Software Developer's Manual
		//
		// Pseudocode from Intel manual:
		// OLD_AL ← AL;
		// OLD_CF ← CF;
		// CF ← 0;
		// IF (AL AND 0FH) > 9 OR AF = 1 THEN
		//     AL ← AL + 6;
		//     CF ← OLD_CF OR (Carry from AL ← AL + 6);
		//     AF ← 1;
		// ELSE
		//     AF ← 0;
		// FI;
		// IF OLD_AL > 99H OR OLD_CF = 1 THEN
		//     AL ← AL + 60H;
		//     CF ← 1;
		// FI;
		
		var oldAl = (byte)(_eax & 0xFF);
		var oldCf = GetFlag(Cf);
		var oldAf = GetFlag(Af);
		var al = oldAl;
		
		// CF is initially cleared per Intel spec
		ClearFlag(Cf);
		
		// Step 1: Check low nibble
		if (((al & 0x0F) > 9) || oldAf)
		{
			// Perform addition and check for carry (overflow)
			var newAl = (byte)(al + 6);
			var carry = (newAl < al);  // Overflow: result wrapped around
			al = newAl;
			
			// CF = OLD_CF OR Carry
			SetFlagVal(Cf, oldCf || carry);
			SetFlag(Af);
		}
		else
		{
			ClearFlag(Af);
		}
		
		// Step 2: Check high nibble
		if ((oldAl > 0x99) || oldCf)
		{
			al += 0x60;
			SetFlag(Cf);
		}
		
		_eax = (_eax & 0xFFFFFF00) | al;
		
		// Update flags: SF, ZF, PF
		UpdateLogicResultFlags(al, 0x80);  // Use 0x80 sign bit mask for byte operations
		
		// OF is officially "undefined" per Intel docs, but real 80386 hardware sets it
		// when the adjustment causes a transition from positive to negative (signed overflow).
		// This matches the behavior observed in hardware-generated SingleStepTests.
		// For DAA (addition), OF = 1 when: old AL was positive AND new AL is negative
		SetFlagVal(Of, ((oldAl & 0x80) == 0) && ((al & 0x80) != 0));
	}

	private void ExecSldt(Instruction insn)
	{
		// SLDT - Store Local Descriptor Table Register
		// This is a privileged instruction for protected mode
		// In a flat memory model emulation, we don't use segmentation
		// Store a dummy value of 0 to indicate no LDT
		if (insn.GetOpKind(0) == OpKind.Memory)
		{
			var addr = CalcMemAddress(insn);
			_mem.Write16(addr, 0);
		}
		else if (insn.GetOpKind(0) == OpKind.Register)
		{
			var reg = insn.GetOpRegister(0);
			if (reg >= Register.EAX && reg <= Register.EDI)
			{
				SetReg32(reg, 0);
			}
			else if (reg >= Register.AX && reg <= Register.DI)
			{
				SetReg16(reg, 0);
			}
		}
	}

	private void ExecArpl(Instruction insn)
	{
		// ARPL - Adjust RPL Field of Segment Selector
		// This is a protected mode instruction for adjusting privilege levels
		// In a flat memory model emulation, we don't use segmentation or privilege levels
		// The instruction should just set ZF based on whether an adjustment was made
		// Since we don't track segment selectors, we always report no adjustment (ZF=0)
		ClearFlag(Zf);
	}

	#endregion

	#region Flags

	/// <summary>
	/// Sets CPU flags after an ADD operation (32-bit operands).
	/// </summary>
	/// <param name="a">First operand</param>
	/// <param name="b">Second operand</param>
	/// <param name="r">Result of a + b</param>
	private void SetFlagsAdd(uint a, uint b, uint r)
	{
		SetFlagsAdd(a, b, r, 0x80000000);
	}

	/// <summary>
	/// Sets CPU flags after an ADD operation with custom sign bit mask for different operand sizes.
	/// 
	/// Flag calculations:
	/// - CF (Carry): Set if unsigned overflow occurred (result wrapped around)
	/// - OF (Overflow): Set if signed overflow occurred using XOR-based detection:
	///   (~(a ^ b) & (a ^ r)) checks if operands had same sign but result has different sign
	/// - AF (Auxiliary): Set if carry occurred from bit 3 to bit 4 (BCD arithmetic)
	/// - ZF, SF, PF: Set by UpdateLogicResultFlags based on result value
	/// 
	/// Reference: Intel SDM Vol 1, Section 3.4.3.1 (Status Flags)
	/// </summary>
	/// <param name="a">First operand</param>
	/// <param name="b">Second operand</param>
	/// <param name="r">Result of a + b</param>
	/// <param name="signBitMask">Mask for sign bit (0x80 for 8-bit, 0x8000 for 16-bit, 0x80000000 for 32-bit)</param>
	private void SetFlagsAdd(uint a, uint b, uint r, uint signBitMask)
	{
		SetFlagVal(Cf, r < a);
		SetFlagVal(Of, (~(a ^ b) & (a ^ r) & signBitMask) != 0);
		SetFlagVal(Af, ((a ^ b ^ r) & 0x10) != 0);
		UpdateLogicResultFlags(r, signBitMask);
	}

	/// <summary>
	/// Sets CPU flags after a SUB operation (32-bit operands).
	/// </summary>
	/// <param name="a">Minuend (value being subtracted from)</param>
	/// <param name="b">Subtrahend (value being subtracted)</param>
	/// <param name="r">Result of a - b</param>
	private void SetFlagsSub(uint a, uint b, uint r)
	{
		SetFlagsSub(a, b, r, 0x80000000);
	}

	/// <summary>
	/// Sets CPU flags after a SUB operation with custom sign bit mask for different operand sizes.
	/// 
	/// Flag calculations:
	/// - CF (Carry/Borrow): Set if unsigned underflow occurred (a &lt; b)
	/// - OF (Overflow): Set if signed overflow occurred using XOR-based detection:
	///   ((a ^ b) & (a ^ r)) checks if operands had different signs and result sign differs from minuend
	/// - AF (Auxiliary): Set if borrow occurred from bit 4 to bit 3 (BCD arithmetic)
	/// - ZF, SF, PF: Set by UpdateLogicResultFlags based on result value
	/// 
	/// Note: SUB is implemented as a + (~b + 1), hence the different XOR pattern for OF
	/// Reference: Intel SDM Vol 1, Section 3.4.3.1 (Status Flags)
	/// </summary>
	/// <param name="a">Minuend (value being subtracted from)</param>
	/// <param name="b">Subtrahend (value being subtracted)</param>
	/// <param name="r">Result of a - b</param>
	/// <param name="signBitMask">Mask for sign bit (0x80 for 8-bit, 0x8000 for 16-bit, 0x80000000 for 32-bit)</param>
	private void SetFlagsSub(uint a, uint b, uint r, uint signBitMask)
	{
		SetFlagVal(Cf, a < b);
		SetFlagVal(Of, ((a ^ b) & (a ^ r) & signBitMask) != 0);
		SetFlagVal(Af, ((a ^ b ^ r) & 0x10) != 0);
		UpdateLogicResultFlags(r, signBitMask);
	}

	/// <summary>
	/// Sets CPU flags after an INC operation.
	/// Note: INC does not affect the Carry Flag (CF), unlike ADD.
	/// </summary>
	/// <param name="a">Original value</param>
	/// <param name="r">Result after incrementing (a + 1)</param>
	private void SetFlagsIncDecAdd(uint a, uint r)
	{
		SetFlagVal(Of, ((~(a ^ 1u) & (a ^ r) & 0x80000000) != 0));
		SetFlagVal(Af, ((a ^ 1u ^ r) & 0x10) != 0);
		UpdateLogicResultFlags(r);
	}

	/// <summary>
	/// Sets CPU flags after a DEC operation.
	/// Note: DEC does not affect the Carry Flag (CF), unlike SUB.
	/// </summary>
	/// <param name="a">Original value</param>
	/// <param name="r">Result after decrementing (a - 1)</param>
	private void SetFlagsIncDecSub(uint a, uint r)
	{
		SetFlagVal(Of, (((a ^ 0xFFFFFFFFu) & (a ^ r) & 0x80000000) != 0));
		SetFlagVal(Af, ((a ^ 1u ^ r) & 0x10) != 0); // Set if borrow from bit 4 (auxiliary carry) occurred
		UpdateLogicResultFlags(r);
	}

	/// <summary>
	/// Updates ZF, SF, and PF flags based on operation result (32-bit).
	/// </summary>
	/// <param name="r">Result value</param>
	private void UpdateLogicResultFlags(uint r)
	{
		UpdateLogicResultFlags(r, 0x80000000);
	}

	/// <summary>
	/// Updates Zero Flag (ZF), Sign Flag (SF), and Parity Flag (PF) based on operation result.
	/// 
	/// Flag calculations:
	/// - ZF: Set if result is zero
	/// - SF: Set if sign bit (MSB) of result is set
	/// - PF: Set if low byte of result has even parity (even number of 1 bits)
	/// 
	/// Parity calculation uses a lookup table approach with magic constant 0x6996:
	/// This 16-bit constant encodes parity for all 4-bit values (0-15).
	/// The algorithm XORs high and low nibbles to reduce 8 bits to 4 bits,
	/// then uses bit position in 0x6996 to determine parity.
	/// 
	/// Reference: Intel SDM Vol 1, Section 3.4.3.1 (Status Flags)
	/// </summary>
	/// <param name="r">Result value</param>
	/// <param name="signBitMask">Mask for sign bit (0x80 for 8-bit, 0x8000 for 16-bit, 0x80000000 for 32-bit)</param>
	private void UpdateLogicResultFlags(uint r, uint signBitMask)
	{
		SetFlagVal(Zf, r == 0);
		SetFlagVal(Sf, (r & signBitMask) != 0);
		var lo = (byte)r;
		var bits = lo ^ (lo >> 4);
		bits &= 0xF;
		var even = (((0x6996 >> bits) & 1) == 0); // Inverted: 0x6996 returns 1 for odd parity
		SetFlagVal(Pf, even);
	}

	private bool IsBranchTaken(ConditionCode cc)
	{
		bool cf = GetFlag(Cf), zf = GetFlag(Zf), sf = GetFlag(Sf), of = GetFlag(Of), pf = GetFlag(Pf);
		return cc switch
		{
			ConditionCode.o => of, ConditionCode.no => !of, ConditionCode.b => cf, ConditionCode.ae => !cf,
			ConditionCode.e => zf, ConditionCode.ne => !zf, ConditionCode.be => cf || zf,
			ConditionCode.a => !cf && !zf, ConditionCode.s => sf, ConditionCode.ns => !sf, ConditionCode.p => pf,
			ConditionCode.np => !pf, ConditionCode.l => sf != of, ConditionCode.ge => sf == of,
			ConditionCode.le => zf || (sf != of), ConditionCode.g => !zf && (sf == of), _ => false
		};
	}

	private bool IsSetccTrue(Mnemonic m) => m switch
	{
		Mnemonic.Seto => GetFlag(Of), Mnemonic.Setno => !GetFlag(Of), Mnemonic.Setb => GetFlag(Cf),
		Mnemonic.Setae => !GetFlag(Cf), Mnemonic.Sete => GetFlag(Zf), Mnemonic.Setne => !GetFlag(Zf),
		Mnemonic.Setbe => GetFlag(Cf) || GetFlag(Zf), Mnemonic.Seta => !GetFlag(Cf) && !GetFlag(Zf),
		Mnemonic.Sets => GetFlag(Sf), Mnemonic.Setns => !GetFlag(Sf), Mnemonic.Setp => GetFlag(Pf),
		Mnemonic.Setnp => !GetFlag(Pf), Mnemonic.Setl => GetFlag(Sf) != GetFlag(Of),
		Mnemonic.Setge => GetFlag(Sf) == GetFlag(Of), Mnemonic.Setle => GetFlag(Zf) || (GetFlag(Sf) != GetFlag(Of)),
		Mnemonic.Setg => !GetFlag(Zf) && (GetFlag(Sf) == GetFlag(Of)), _ => false
	};

	private bool IsCmovccTrue(Mnemonic m) => m switch
	{
		Mnemonic.Cmove => GetFlag(Zf),
		Mnemonic.Cmovne => !GetFlag(Zf),
		Mnemonic.Cmovb => GetFlag(Cf),
		Mnemonic.Cmovae => !GetFlag(Cf),
		Mnemonic.Cmovbe => GetFlag(Cf) || GetFlag(Zf),
		Mnemonic.Cmova => !GetFlag(Cf) && !GetFlag(Zf),
		Mnemonic.Cmovge => GetFlag(Sf) == GetFlag(Of),
		Mnemonic.Cmovg => !GetFlag(Zf) && (GetFlag(Sf) == GetFlag(Of)),
		Mnemonic.Cmovl => GetFlag(Sf) != GetFlag(Of),
		Mnemonic.Cmovo => GetFlag(Of),
		Mnemonic.Cmovno => !GetFlag(Of),
		Mnemonic.Cmovs => GetFlag(Sf),
		Mnemonic.Cmovns => !GetFlag(Sf),
		Mnemonic.Cmovp => GetFlag(Pf),
		Mnemonic.Cmovnp => !GetFlag(Pf),
		_ => false
	};

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private bool GetFlag(int bit) => (_eflags & (1u << bit)) != 0;
	
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private void SetFlag(int bit) => _eflags |= (1u << bit);
	
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private void ClearFlag(int bit) => _eflags &= ~(1u << bit);

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private void SetFlagVal(int bit, bool val)
	{
		if (val)
		{
			SetFlag(bit);
		}
		else
		{
			ClearFlag(bit);
		}
	}

	#endregion

	private uint ReadOp(Instruction insn, int index)
	{
		return insn.GetOpKind(index) switch
		{
			OpKind.Register => ReadRegister(insn.GetOpRegister(index)),
			OpKind.Memory => insn.MemorySize switch
			{
				MemorySize.UInt8 or MemorySize.Int8 => _mem.Read8(CalcMemAddress(insn)),
				MemorySize.UInt16 or MemorySize.Int16 => Read16(CalcMemAddress(insn)),
				_ => Read32(CalcMemAddress(insn))
			},
			OpKind.Immediate8 => insn.Immediate8,
			OpKind.Immediate8to32 => (uint)(sbyte)insn.Immediate8,
			OpKind.Immediate16 => insn.Immediate16,
			OpKind.Immediate32 => insn.Immediate32,
			_ => 0u
		};
	}
	
	private uint ReadRegister(Register reg)
	{
		// Try 8-bit registers first
		if (reg is Register.AL or Register.CL or Register.DL or Register.BL or 
		    Register.AH or Register.CH or Register.DH or Register.BH)
		{
			return GetReg8(reg);
		}
		
		// Try 16-bit registers (including segment registers)
		if (reg is Register.AX or Register.CX or Register.DX or Register.BX or
		    Register.SI or Register.DI or Register.SP or Register.BP or
		    Register.CS or Register.DS or Register.ES or Register.FS or Register.GS or Register.SS)
		{
			return GetReg16(reg);
		}
		
		// Default to 32-bit
		return GetReg32(reg);
	}

	private void WriteOp(Instruction insn, int index, uint value)
	{
		switch (insn.GetOpKind(index))
		{
			case OpKind.Register:
				WriteRegister(insn.GetOpRegister(index), value);
				break;
			case OpKind.Memory:
				{
					var addr = CalcMemAddress(insn);
					switch (insn.MemorySize)
					{
						case MemorySize.UInt8:
						case MemorySize.Int8:
							_mem.Write8(addr, (byte)value);
							break;
						case MemorySize.UInt16:
						case MemorySize.Int16:
							Write16(addr, (ushort)value);
							break;
						default:
							Write32(addr, value);
							break;
					}
					break;
				}
			default:
				_logger.LogWarning("[IcedCpu] WriteOp unsupported {GetOpKind}", insn.GetOpKind(index));
				break;
		}
	}
	
	private void WriteRegister(Register reg, uint value)
	{
		// Try 8-bit registers first
		if (reg is Register.AL or Register.CL or Register.DL or Register.BL or 
		    Register.AH or Register.CH or Register.DH or Register.BH)
		{
			SetReg8(reg, (byte)value);
			return;
		}
		
		// Try 16-bit registers (including segment registers)
		if (reg is Register.AX or Register.CX or Register.DX or Register.BX or
		    Register.SI or Register.DI or Register.SP or Register.BP or
		    Register.CS or Register.DS or Register.ES or Register.FS or Register.GS or Register.SS)
		{
			SetReg16(reg, (ushort)value);
			return;
		}
		
		// Default to 32-bit
		SetReg32(reg, value);
	}

	private int GetShiftCount(Instruction insn)
	{
		if (insn.OpCount < 2)
		{
			return 1;
		}

		var kind = insn.GetOpKind(1);
		if (kind == OpKind.Immediate8)
		{
			return insn.Immediate8 & 0x1F;
		}

		if (kind == OpKind.Register && insn.GetOpRegister(1) == Register.CL)
		{
			return (int)(_ecx & 0xFF) & 0x1F;
		}

		return 1;
	}

	private int GetSourceSizeBits(Instruction insn)
	{
		if (insn.GetOpKind(1) == OpKind.Memory)
		{
			return insn.MemorySize switch
			{
				MemorySize.UInt8 or MemorySize.Int8 => 8, MemorySize.UInt16 or MemorySize.Int16 => 16, _ => 32
			};
		}

		var r = insn.GetOpRegister(1);
		if (r is Register.AL or Register.CL or Register.DL or Register.BL or Register.AH or Register.CH or Register.DH
		    or Register.BH)
		{
			return 8;
		}

		if (r is Register.AX or Register.CX or Register.DX or Register.BX or Register.SI or Register.DI or Register.SP
		    or Register.BP)
		{
			return 16;
		}

		return 32;
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

			if (r is Register.AX or Register.CX or Register.DX or Register.BX or Register.SI or Register.DI or Register.SP or Register.BP or
			    Register.CS or Register.DS or Register.ES or Register.FS or Register.GS or Register.SS)
			{
				return 16;
			}

			return 32;
		}

		// For immediates, default to 32
		return 32;
	}

	/// <summary>
	/// Rotate a value right by the specified number of bits, respecting the operand size.
	/// Handles the shift overflow issue for 32-bit operands.
	/// </summary>
	private uint RotateRight(uint value, int count, int opSize)
	{
		// Normalize count to operand size
		count %= opSize;
		if (count == 0)
			return value;
		
		// Get mask for operand size
		var mask = opSize == 32 ? 0xFFFFFFFFu : (1u << opSize) - 1;
		value &= mask;
		
		// Perform rotation: (value >> count) | (value << (opSize - count))
		var rightPart = value >> count;
		var leftPart = (value << (opSize - count)) & mask;
		return (rightPart | leftPart) & mask;
	}

	/// <summary>
	/// Rotate a value left by the specified number of bits, respecting the operand size.
	/// Handles the shift overflow issue for 32-bit operands.
	/// </summary>
	private uint RotateLeft(uint value, int count, int opSize)
	{
		// Normalize count to operand size
		count %= opSize;
		if (count == 0)
			return value;
		
		// Get mask for operand size
		var mask = opSize == 32 ? 0xFFFFFFFFu : (1u << opSize) - 1;
		value &= mask;
		
		// Perform rotation: (value << count) | (value >> (opSize - count))
		var leftPart = (value << count) & mask;
		var rightPart = value >> (opSize - count);
		return (leftPart | rightPart) & mask;
	}

	// replace CalcMemAddress to report via Diagnostics on failure
	private uint CalcMemAddress(Instruction insn)
	{
		// Calculate offset part (base + index*scale + displacement)
		var offset = insn.MemoryDisplacement32;
		if (insn.MemoryBase != Register.None)
		{
			var baseReg = insn.MemoryBase;
			// Use appropriate register size - 16-bit registers should only use lower 16 bits
			if (Is16BitRegister(baseReg))
			{
				offset += GetReg16(baseReg);
			}
			else
			{
				offset += GetReg32(baseReg);
			}
		}

		if (insn.MemoryIndex != Register.None)
		{
			var indexReg = insn.MemoryIndex;
			var scale = insn.MemoryIndexScale;
			// Use appropriate register size - 16-bit registers should only use lower 16 bits
			if (Is16BitRegister(indexReg))
			{
				offset += (uint)(GetReg16(indexReg) * scale);
			}
			else
			{
				offset += (uint)(GetReg32(indexReg) * scale);
			}
		}

		// Determine which segment register to use
		// Priority: explicit segment override > default for register
		ushort segmentValue = 0;
		Register segmentReg = insn.SegmentPrefix;
		
		// If no explicit segment override, use default for the base register
		if (segmentReg == Register.None)
		{
			// Default segment rules for x86:
			// BP/EBP defaults to SS, all other registers default to DS
			if (insn.MemoryBase == Register.BP || insn.MemoryBase == Register.EBP)
			{
				segmentReg = Register.SS;
			}
			else
			{
				segmentReg = Register.DS;
			}
		}
		
		// Get the segment register value
		segmentValue = segmentReg switch
		{
			Register.CS => _cs,
			Register.DS => _ds,
			Register.ES => _es,
			Register.FS => _fs,
			Register.GS => _gs,
			Register.SS => _ss,
			_ => 0
		};
		
		// In 16-bit real mode, physical address = (segment << 4) + offset
		// For 32-bit protected mode, segment registers are selectors (not used for address calculation)
		uint addr;
		if (_bitness == 16)
		{
			// 16-bit real mode addressing: Calculate linear address from segment:offset
			// First, mask offset to 16 bits (wrap at 64KB boundary)
			var offset16 = offset & 0xFFFF;
			
			// Then convert to linear address: (segment << 4) + offset
			// This is the proper 8086/80286/80386 real mode addressing formula
			addr = (uint)((segmentValue << 4) + offset16);
		}
		else
		{
			// 32-bit protected mode: segments are selectors, not used for linear address
			// (In proper protected mode emulation, we'd use descriptor tables, but for now just use offset)
			addr = offset;
		}

		// Debug logging for IAT address calculations (displacement in IAT range 0x004552E0-0x00455360)
		// This will catch the problematic LoadIconA read from 0x004552F8
		if (insn.MemoryDisplacement32 >= 0x004552E0 && insn.MemoryDisplacement32 <= 0x00455360)
		{
			_logger.LogWarning("[IcedCpu] CalcMemAddress for IAT: EIP=0x{Eip:X8}, disp=0x{Disp:X8}, base={Base}, baseVal=0x{BaseVal:X8}, index={Index}, indexVal=0x{IndexVal:X8}, scale={Scale}, seg={Seg}, segVal=0x{SegVal:X4}, finalAddr=0x{Addr:X8}",
				_eip, insn.MemoryDisplacement32, insn.MemoryBase,
				insn.MemoryBase != Register.None ? GetReg32(insn.MemoryBase) : 0,
				insn.MemoryIndex,
				insn.MemoryIndex != Register.None ? GetReg32(insn.MemoryIndex) : 0,
				insn.MemoryIndexScale,
				segmentReg, segmentValue,
				addr);
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

	/// <summary>
	/// Calculates the offset within a segment for a given memory instruction.
	/// </summary>
	private uint CalculateSegmentOffset(Instruction insn)
	{
		var offset = insn.MemoryDisplacement32;
		if (insn.MemoryBase != Register.None)
		{
			var baseReg = insn.MemoryBase;
			if (Is16BitRegister(baseReg))
			{
				offset += GetReg16(baseReg);
			}
			else
			{
				offset += GetReg32(baseReg);
			}
		}
		if (insn.MemoryIndex != Register.None)
		{
			var indexReg = insn.MemoryIndex;
			var scale = insn.MemoryIndexScale;
			if (Is16BitRegister(indexReg))
			{
				offset += (uint)(GetReg16(indexReg) * scale);
			}
			else
			{
				offset += (uint)(GetReg32(indexReg) * scale);
			}
		}
		return offset;
	}

	/// <summary>
	/// Determines which segment register to use for a memory instruction.
	/// </summary>
	private Register GetSegmentRegister(Instruction insn)
	{
		Register segmentReg = insn.SegmentPrefix;
		if (segmentReg == Register.None)
		{
			// Default segment rules for x86: BP/EBP defaults to SS, all other registers default to DS
			segmentReg = (insn.MemoryBase == Register.BP || insn.MemoryBase == Register.EBP) 
				? Register.SS 
				: Register.DS;
		}
		return segmentReg;
	}

	/// <summary>
	/// Retrieves the value of a segment register.
	/// </summary>
	private ushort GetSegmentValue(Register segmentReg)
	{
		return segmentReg switch
		{
			Register.CS => _cs,
			Register.DS => _ds,
			Register.ES => _es,
			Register.FS => _fs,
			Register.GS => _gs,
			Register.SS => _ss,
			_ => 0
		};
	}

	/// <summary>
	/// Safe memory read that handles segment wrapping in 16-bit real mode.
	/// In real mode, when a 16-bit access starts at offset 0xFFFF, it wraps to offset 0x0000.
	/// </summary>
	private ushort SafeRead16(Instruction insn, uint addr)
	{
		// In 16-bit real mode, handle segment wrapping
		if (_bitness == 16)
		{
			// Calculate the offset within the segment
			var offset16 = CalculateSegmentOffset(insn) & 0xFFFF;
			
			// Check if the 16-bit read would wrap at the segment boundary
			if (offset16 == 0xFFFF)
			{
				// Read crosses segment boundary: read byte at 0xFFFF and byte at 0x0000 (wrapped)
				var segmentReg = GetSegmentRegister(insn);
				var segmentValue = GetSegmentValue(segmentReg);

				// Read low byte from offset 0xFFFF
				var addrHigh = (uint)((segmentValue << 4) + 0xFFFF);
				var lowByte = _mem.Read8(addrHigh);
				
				// Read high byte from offset 0x0000 (wrapped)
				var addrLow = (uint)((segmentValue << 4) + 0x0000);
				var highByte = _mem.Read8(addrLow);
				
				// Combine bytes (little-endian)
				return (ushort)(lowByte | (highByte << 8));
			}
		}

		// Normal read (no segment wrap)
		return _mem.Read16(addr);
	}

	/// <summary>
	/// Safe memory read that handles segment wrapping in 16-bit real mode.
	/// In real mode, 32-bit accesses that cross segment boundaries wrap around.
	/// </summary>
	private uint SafeRead32(Instruction insn, uint addr)
	{
		// In 16-bit real mode, handle segment wrapping for 32-bit reads
		if (_bitness == 16)
		{
			// Calculate the offset within the segment
			var offset16 = CalculateSegmentOffset(insn) & 0xFFFF;
			
			// Check if the 32-bit read would wrap at the segment boundary
			if (offset16 >= 0xFFFD) // 0xFFFD, 0xFFFE, or 0xFFFF would cause wrap
			{
				// Read crosses segment boundary - read byte by byte with wrapping
				var segmentReg = GetSegmentRegister(insn);
				var segmentValue = GetSegmentValue(segmentReg);

				uint result = 0;
				for (int i = 0; i < 4; i++)
				{
					var byteOffset = (ushort)((offset16 + i) & 0xFFFF); // Wrap at 64KB
					var physAddr = (uint)((segmentValue << 4) + byteOffset);
					var b = _mem.Read8(physAddr);
					result |= ((uint)b << (i * 8));
				}
				return result;
			}
		}

		// Normal read (no segment wrap)
		return _mem.Read32(addr);
	}

	/// <summary>
	/// Safe memory write that handles segment wrapping in 16-bit real mode.
	/// In real mode, when a 16-bit write starts at offset 0xFFFF, it wraps to offset 0x0000.
	/// </summary>
	private void SafeWrite16(Instruction insn, uint addr, ushort value)
	{
		// In 16-bit real mode, handle segment wrapping
		if (_bitness == 16)
		{
			// Calculate the offset within the segment
			var offset16 = CalculateSegmentOffset(insn) & 0xFFFF;
			
			// Check if the 16-bit write would wrap at the segment boundary
			if (offset16 == 0xFFFF)
			{
				// Write crosses segment boundary: write byte at 0xFFFF and byte at 0x0000 (wrapped)
				var segmentReg = GetSegmentRegister(insn);
				var segmentValue = GetSegmentValue(segmentReg);

				// Write low byte to offset 0xFFFF
				var addrHigh = (uint)((segmentValue << 4) + 0xFFFF);
				_mem.Write8(addrHigh, (byte)(value & 0xFF));
				
				// Write high byte to offset 0x0000 (wrapped)
				var addrLow = (uint)((segmentValue << 4) + 0x0000);
				_mem.Write8(addrLow, (byte)((value >> 8) & 0xFF));
				
				return;
			}
		}

		// Normal write (no segment wrap)
		_mem.Write16(addr, value);
	}

	/// <summary>
	/// Safe memory write that handles segment wrapping in 16-bit real mode.
	/// In real mode, 32-bit writes that cross segment boundaries wrap around.
	/// </summary>
	private void SafeWrite32(Instruction insn, uint addr, uint value)
	{
		// In 16-bit real mode, handle segment wrapping for 32-bit writes
		if (_bitness == 16)
		{
			// Calculate the offset within the segment
			var offset16 = CalculateSegmentOffset(insn) & 0xFFFF;
			
			// Check if the 32-bit write would wrap at the segment boundary
			if (offset16 >= 0xFFFD) // 0xFFFD, 0xFFFE, or 0xFFFF would cause wrap
			{
				// Write crosses segment boundary - write byte by byte with wrapping
				var segmentReg = GetSegmentRegister(insn);
				var segmentValue = GetSegmentValue(segmentReg);

				for (int i = 0; i < 4; i++)
				{
					var byteOffset = (ushort)((offset16 + i) & 0xFFFF); // Wrap at 64KB
					var physAddr = (uint)((segmentValue << 4) + byteOffset);
					var b = (byte)((value >> (i * 8)) & 0xFF);
					_mem.Write8(physAddr, b);
				}
				return;
			}
		}

		// Normal write (no segment wrap)
		_mem.Write32(addr, value);
	}

	// CalcLeaAddress calculates an effective address without validating memory bounds
	// LEA (Load Effective Address) doesn't actually access memory, so out-of-bounds addresses are valid
	private uint CalcLeaAddress(Instruction insn)
	{
		var addr = insn.MemoryDisplacement32;
		if (insn.MemoryBase != Register.None)
		{
			addr += GetReg32(insn.MemoryBase);
		}

		if (insn.MemoryIndex != Register.None)
		{
			var scale = insn.MemoryIndexScale;
			addr += (uint)(GetReg32(insn.MemoryIndex) * scale);
		}

		return addr;
	}

	private bool Is16BitRegister(Register reg) => 
		reg is Register.AX or Register.CX or Register.DX or Register.BX or 
		      Register.SI or Register.DI or Register.SP or Register.BP;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private uint GetReg32(Register reg) => reg switch
	{
		Register.EAX => _eax, Register.EBX => _ebx, Register.ECX => _ecx, Register.EDX => _edx,
		Register.ESI => _esi, Register.EDI => _edi, Register.EBP => _ebp, Register.ESP => _esp,
		// Segment registers are 16-bit but zero-extended to 32-bit
		Register.CS => _cs, Register.DS => _ds, Register.ES => _es,
		Register.FS => _fs, Register.GS => _gs, Register.SS => _ss,
		// Control registers (CR0-CR4)
		Register.CR0 => _cr0, Register.CR2 => _cr2, Register.CR3 => _cr3, Register.CR4 => _cr4,
		// Debug registers (DR0-DR7)
		Register.DR0 => _dr0, Register.DR1 => _dr1, Register.DR2 => _dr2, Register.DR3 => _dr3,
		Register.DR6 => _dr6, Register.DR7 => _dr7,
		_ => 0
	};

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private ushort GetReg16(Register reg) => reg switch
	{
		Register.AX => (ushort)_eax, Register.BX => (ushort)_ebx, Register.CX => (ushort)_ecx,
		Register.DX => (ushort)_edx, Register.SI => (ushort)_esi, Register.DI => (ushort)_edi,
		Register.BP => (ushort)_ebp, Register.SP => (ushort)_esp,
		// Segment registers
		Register.CS => _cs, Register.DS => _ds, Register.ES => _es,
		Register.FS => _fs, Register.GS => _gs, Register.SS => _ss,
		_ => 0
	};

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private byte GetReg8(Register reg) => reg switch
	{
		Register.AL => (byte)(_eax & 0xFF), Register.CL => (byte)(_ecx & 0xFF), Register.DL => (byte)(_edx & 0xFF),
		Register.BL => (byte)(_ebx & 0xFF), Register.AH => (byte)((_eax >> 8) & 0xFF),
		Register.CH => (byte)((_ecx >> 8) & 0xFF), Register.DH => (byte)((_edx >> 8) & 0xFF),
		Register.BH => (byte)((_ebx >> 8) & 0xFF), _ => 0
	};

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private void SetReg8(Register reg, byte v)
	{
		switch (reg)
		{
			case Register.AL: _eax = (_eax & 0xFFFFFF00) | v; break;
			case Register.CL: _ecx = (_ecx & 0xFFFFFF00) | v; break;
			case Register.DL: _edx = (_edx & 0xFFFFFF00) | v; break;
			case Register.BL: _ebx = (_ebx & 0xFFFFFF00) | v; break;
			case Register.AH: _eax = (_eax & 0xFFFF00FF) | ((uint)v << 8); break;
			case Register.CH: _ecx = (_ecx & 0xFFFF00FF) | ((uint)v << 8); break;
			case Register.DH: _edx = (_edx & 0xFFFF00FF) | ((uint)v << 8); break;
			case Register.BH: _ebx = (_ebx & 0xFFFF00FF) | ((uint)v << 8); break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private void SetReg16(Register reg, ushort v)
	{
		switch (reg)
		{
			case Register.AX: _eax = (_eax & 0xFFFF0000) | v; break;
			case Register.BX: _ebx = (_ebx & 0xFFFF0000) | v; break;
			case Register.CX: _ecx = (_ecx & 0xFFFF0000) | v; break;
			case Register.DX: _edx = (_edx & 0xFFFF0000) | v; break;
			case Register.SI: _esi = (_esi & 0xFFFF0000) | v; break;
			case Register.DI: _edi = (_edi & 0xFFFF0000) | v; break;
			case Register.BP:
				if (_eip >= 0x00403180 && _eip <= 0x004031A0)
				{
					_logger.LogWarning("[IcedCpu] SetReg16(BP): value=0x{Value:X4}, newEBP=0x{NewEbp:X8}, EIP=0x{Eip:X8}, ESP=0x{Esp:X8}",
						v, (_ebp & 0xFFFF0000) | v, _eip, _esp);
				}
				_ebp = (_ebp & 0xFFFF0000) | v;
				break;
			case Register.SP: _esp = (_esp & 0xFFFF0000) | v; break;
			// Segment registers
			case Register.CS: _cs = v; break;
			case Register.DS: _ds = v; break;
			case Register.ES: _es = v; break;
			case Register.FS: _fs = v; break;
			case Register.GS: _gs = v; break;
			case Register.SS: _ss = v; break;
			default:
				throw new ArgumentOutOfRangeException(nameof(reg), reg, "Invalid 16-bit register specified in SetReg16.");
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private void SetReg32(Register reg, uint v)
	{
		// Debug logging for EBP changes around the problematic instruction
		if (reg == Register.EBP && _eip >= 0x00403180 && _eip <= 0x004031A0)
		{
			_logger.LogWarning("[IcedCpu] SetReg32: EBP=0x{Value:X8}, EIP=0x{Eip:X8}, ESP=0x{Esp:X8}",
				v, _eip, _esp);
		}
		
		switch (reg)
		{
			case Register.EAX: _eax = v; break;
			case Register.EBX: _ebx = v; break;
			case Register.ECX: _ecx = v; break;
			case Register.EDX: _edx = v; break;
			case Register.ESI: _esi = v; break;
			case Register.EDI: _edi = v; break;
			case Register.EBP: _ebp = v; break;
			case Register.ESP: _esp = v; break;
			// Segment registers - truncate to 16-bit
			case Register.CS: _cs = (ushort)v; break;
			case Register.DS: _ds = (ushort)v; break;
			case Register.ES: _es = (ushort)v; break;
			case Register.FS: _fs = (ushort)v; break;
			case Register.GS: _gs = (ushort)v; break;
			case Register.SS: _ss = (ushort)v; break;
			// Control registers (CR0-CR4)
			// Note: In real hardware, writing to CR0/CR3/CR4 would have side effects
			// For user-mode emulation, we just store the values
			case Register.CR0: _cr0 = v; break;
			case Register.CR2: _cr2 = v; break;
			case Register.CR3: _cr3 = v; break;
			case Register.CR4: _cr4 = v; break;
			// Debug registers (DR0-DR7)
			case Register.DR0: _dr0 = v; break;
			case Register.DR1: _dr1 = v; break;
			case Register.DR2: _dr2 = v; break;
			case Register.DR3: _dr3 = v; break;
			case Register.DR6: _dr6 = v; break;
			case Register.DR7: _dr7 = v; break;
		}
	}

	private uint Read32(uint addr)
	{
		var value = _mem.Read32(addr);
		
		// Debug logging for IAT reads
		if (addr >= 0x004552E0 && addr <= 0x00455360)
		{
			_logger.LogWarning("[IcedCpu] Read32 from IAT: addr=0x{Addr:X8}, value=0x{Value:X8}, EIP=0x{Eip:X8}",
				addr, value, _eip);
		}
		
		return value;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private void Write32(uint addr, uint v)
	{
		_mem.Write32(addr, v);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private ushort Read16(uint addr) => _mem.Read16(addr);
	
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private void Write16(uint addr, ushort v) => _mem.Write16(addr, v);

	/// <summary>
	/// Calculate physical stack address from current stack pointer.
	/// In 16-bit mode, uses SS:SP segment:offset addressing.
	/// In 32-bit mode, returns ESP directly.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private uint GetStackAddress()
	{
		if (_bitness == 16)
		{
			var sp = (ushort)(_esp & 0xFFFF);
			return (uint)((_ss << 4) + sp);
		}
		return _esp;
	}

	/// <summary>
	/// Adjust stack pointer by the specified number of bytes.
	/// In 16-bit mode, only modifies SP (low 16 bits of ESP).
	/// In 32-bit mode, modifies full ESP.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private void AdjustStackPointer(int bytes)
	{
		if (_bitness == 16)
		{
			var sp = (ushort)((_esp & 0xFFFF) + bytes);
			_esp = (_esp & 0xFFFF0000) | sp;
		}
		else
		{
			_esp = (uint)((int)_esp + bytes);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private void Push32(uint v)
	{
		AdjustStackPointer(-4);
		var stackAddr = GetStackAddress();
		_mem.Write32(stackAddr, v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private uint Pop32()
	{
		var stackAddr = GetStackAddress();
		var v = _mem.Read32(stackAddr);
		AdjustStackPointer(4);
		return v;
	}

	#region FPU Helpers

	// Get ST(i) - ST(0) is the top of stack
	public double FpuGetSt(int i)
	{
		var idx = (_fpuTop + i) & 7;
		return _fpu[idx];
	}

	// Set ST(i)
	private void FpuSetSt(int i, double val)
	{
		var idx = (_fpuTop + i) & 7;
		_fpu[idx] = val;
	}

	// Push a value onto the FPU stack
	private void FpuPush(double val)
	{
		_fpuTop = (_fpuTop - 1) & 7;
		_fpu[_fpuTop] = val;
	}

	// Pop a value from the FPU stack
	public double FpuPop()
	{
		var val = _fpu[_fpuTop];
		_fpuTop = (_fpuTop + 1) & 7;
		return val;
	}

	#endregion

	#region IAsyncCpu Implementation

	/// <summary>
	/// Execute a single instruction asynchronously (wrapper around synchronous SingleStep for compatibility)
	/// </summary>
	public Task<CpuStepResult> SingleStepAsync(VirtualMemory mem)
	{
		// For the interpreter-based IcedCpu, we simply wrap the synchronous call
		// A true JIT implementation would use this for async operations
		var result = SingleStep(mem);
		return Task.FromResult(result);
	}

	/// <summary>
	/// Execute multiple instructions asynchronously until a breakpoint or call
	/// </summary>
	public Task<CpuStepResult> ExecuteBlockAsync(VirtualMemory mem)
	{
		// For the interpreter, execute instructions one at a time until we hit a call
		// A true JIT would compile the block and execute it as a single unit
		CpuStepResult result;
		
		do
		{
			result = SingleStep(mem);
			
			// Stop if we hit a call
			if (result.IsCall)
			{
				break;
			}
		} while (true);
		
		return Task.FromResult(result);
	}

	/// <summary>
	/// Interpreter-based IcedCpu does not support JIT compilation
	/// </summary>
	public bool SupportsJit => false;

	/// <summary>
	/// Save complete CPU state for async suspension
	/// </summary>
	public CpuState SaveState()
	{
		return new CpuState
		{
			Eax = _eax,
			Ebx = _ebx,
			Ecx = _ecx,
			Edx = _edx,
			Esi = _esi,
			Edi = _edi,
			Ebp = _ebp,
			Esp = _esp,
			Eip = _eip,
			Eflags = _eflags,
			FpuStack = (double[])_fpu.Clone(),
			FpuTop = _fpuTop,
			FpuControlWord = _fpuControlWord,
			FpuStatusWord = _fpuStatusWord,
			FpuTagWord = _fpuTagWord,
			Cr0 = _cr0,
			Cr2 = _cr2,
			Cr3 = _cr3,
			Cr4 = _cr4,
			Dr0 = _dr0,
			Dr1 = _dr1,
			Dr2 = _dr2,
			Dr3 = _dr3,
			Dr6 = _dr6,
			Dr7 = _dr7
		};
	}

	/// <summary>
	/// Restore CPU state after async resumption
	/// </summary>
	public void RestoreState(CpuState state)
	{
		_eax = state.Eax;
		_ebx = state.Ebx;
		_ecx = state.Ecx;
		_edx = state.Edx;
		_esi = state.Esi;
		_edi = state.Edi;
		if (_eip >= 0x00403180 && _eip <= 0x004031A0)
		{
			_logger.LogWarning("[IcedCpu] RestoreState: EBP=0x{Value:X8}, EIP=0x{Eip:X8}, ESP=0x{Esp:X8}",
				state.Ebp, _eip, _esp);
		}
		_ebp = state.Ebp;
		_esp = state.Esp;
		_eip = state.Eip;
		_eflags = state.Eflags;
		
		if (state.FpuStack != null)
		{
			Array.Copy(state.FpuStack, _fpu, 8);
			_fpuTop = state.FpuTop;
			_fpuControlWord = state.FpuControlWord;
			_fpuStatusWord = state.FpuStatusWord;
			_fpuTagWord = state.FpuTagWord;
		}

		// Restore control and debug registers
		_cr0 = state.Cr0;
		_cr2 = state.Cr2;
		_cr3 = state.Cr3;
		_cr4 = state.Cr4;
		_dr0 = state.Dr0;
		_dr1 = state.Dr1;
		_dr2 = state.Dr2;
		_dr3 = state.Dr3;
		_dr6 = state.Dr6;
		_dr7 = state.Dr7;
	}

	#endregion

	private sealed class SimpleMemoryCodeReader(IcedCpu cpu) : CodeReader
	{
		private uint _ip;
		
		public void Reset(uint ip) => _ip = ip;
		
		public override int ReadByte()
		{
			uint physicalAddress;
			
			// In 16-bit real mode, convert IP to physical address using CS:IP
			if (cpu._bitness == 16)
			{
				// Calculate physical address: CS * 16 + IP
				// IP should be masked to 16 bits
				var ip16 = _ip & 0xFFFF;
				physicalAddress = (uint)((cpu._cs << 4) + ip16);
			}
			else
			{
				// In 32-bit protected mode, use IP directly
				physicalAddress = _ip;
			}
			
			// Increment IP (wrap to 16 bits in real mode)
			if (cpu._bitness == 16)
			{
				_ip = (_ip + 1) & 0xFFFF;
			}
			else
			{
				_ip++;
			}
			
			return cpu._mem.Read8(physicalAddress);
		}
	}

	private enum LogicOp
	{
		And,
		Or
	}

	private enum RotateKind
	{
		Rol,
		Ror,
		Rcl,
		Rcr
	}
}
