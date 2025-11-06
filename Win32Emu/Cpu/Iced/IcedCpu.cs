using System.Diagnostics;
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

	// x87 FPU state (8 registers in a stack, ST(0) to ST(7))
	private readonly double[] _fpu = new double[8];
	private int _fpuTop = 0; // Index of ST(0) in the circular stack
	private ushort _fpuControlWord = 0x037F; // Default FPU control word
	private ushort _fpuStatusWord = 0x0000; // FPU status word
	private ushort _fpuTagWord = 0xFFFF; // FPU tag word (all tags set to 11b = empty)

	// RDTSC support - use Stopwatch for high-resolution timing
	private static readonly Stopwatch RdtscStopwatch = Stopwatch.StartNew();
	private static readonly bool RdtscIsHighResolution = Stopwatch.IsHighResolution;
	private static readonly long RdtscFrequency = Stopwatch.Frequency;

	public IcedCpu(VirtualMemory mem, ILogger? logger = null, DecoderOptions decoderOptions = DecoderOptions.None, bool enableInstructionAnalyzer = false, uint imageBase = DEFAULT_IMAGE_BASE, uint stackLimit = DEFAULT_STACK_LIMIT, uint stackBase = DEFAULT_STACK_BASE)
	{
		_mem = mem;
		_logger = logger ?? NullLogger.Instance;
		_imageBase = imageBase;
		_stackLimit = stackLimit;
		_stackBase = stackBase;
		_reader = new SimpleMemoryCodeReader(this);
		_decoder = Decoder.Create(32, _reader, decoderOptions);
		
		if (enableInstructionAnalyzer)
		{
			_analyzer = new InstructionAnalyzer(logger);
		}
	}

	public void SetEip(uint eip) => _eip = eip;
	public uint GetEip() => _eip;

	public uint GetRegister(string name) => name.ToUpperInvariant() switch
	{
		"EAX" => _eax, "EBX" => _ebx, "ECX" => _ecx, "EDX" => _edx, "ESI" => _esi, "EDI" => _edi, "EBP" => _ebp,
		"ESP" => _esp, "EIP" => _eip, "EFLAGS" => _eflags, _ => 0
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
				if (_eip >= 0x00403180 && _eip <= 0x004031A0)
				{
					_logger.LogWarning("[IcedCpu] SetRegister(EBP): value=0x{Value:X8}, EIP=0x{Eip:X8}, ESP=0x{Esp:X8}, Caller={CallerName}",
						value, _eip, _esp, callerName);
				}
				_ebp = value;
				break;
			case "ESP": _esp = value; break;
			case "EIP": _eip = value; break;
			case "EFLAGS": _eflags = value; break;
		}
	}

	public CpuStepResult SingleStep(VirtualMemory mem)
	{
		// Set diagnostics context for memory errors
		var instrBytes = new byte[8];
		try
		{
			instrBytes = mem.GetSpan(_eip, 8);
		}
		catch
		{
			instrBytes = null;
		}

		Diagnostics.Diagnostics.SetCpuContext(new Diagnostics.Diagnostics.CpuContext(_eip, _esp, _ebp, _eax, _ecx, _edx, instrBytes));

		var oldEip = _eip; // Capture instruction address BEFORE any decoder operations
		_reader.Reset(_eip);
		_decoder.IP = _eip;
		var insn = _decoder.Decode();
		//_logger.LogInformation("Instruction: {Insn}", insn.ToString());
		
		// Log instructions in the problematic range (after LoadCursorA returns)
		if (oldEip >= 0x00403160 && oldEip <= 0x004031A0)
		{
			var bytes = mem.GetSpan(oldEip, 16).ToArray();
			var byteString = string.Join(" ", bytes.Select(b => b.ToString("X2")));
			_logger.LogInformation("[IcedCpu] Executing at 0x{Eip:X8}: {Insn} (Bytes: {Bytes})", oldEip, insn.ToString(), byteString);
		}
		
		_eip = (uint)_decoder.IP;
		
		// Detect if decoder advanced EIP incorrectly (sanity check)
		if (_eip < oldEip || _eip > oldEip + 15)
		{
			_logger.LogWarning("[IcedCpu] Decoder set suspicious EIP: oldEip=0x{OldEip:X8}, new EIP=0x{NewEip:X8}, instruction={Insn}", 
				oldEip, _eip, insn.ToString());
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
					_esp -= 4;
					Write32(_esp, _eip);
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
					var ret = Read32(_esp);
					var oldEsp = _esp;
					_esp += 4;
					_eip = ret;
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
						// Signal syscall to emulator; no actual interrupt handling will occur
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
				case Mnemonic.Loop:
					// LOOP - Decrement ECX and jump if ECX != 0
					_ecx--;
					if (_ecx != 0)
					{
						_eip = (uint)insn.NearBranchTarget;
					}
					break;
				case Mnemonic.Loope:
					// LOOPE/LOOPZ - Decrement ECX and jump if ECX != 0 and ZF = 1
					_ecx--;
					if (_ecx != 0 && GetFlag(Zf))
					{
						_eip = (uint)insn.NearBranchTarget;
					}
					break;
				case Mnemonic.Loopne:
					// LOOPNE/LOOPNZ - Decrement ECX and jump if ECX != 0 and ZF = 0
					_ecx--;
					if (_ecx != 0 && !GetFlag(Zf))
					{
						_eip = (uint)insn.NearBranchTarget;
					}
					break;
				default:
					if (insn.Mnemonic.ToString().StartsWith('J'))
					{
						if (IsBranchTaken(insn.ConditionCode))
						{
							_eip = (uint)insn.NearBranchTarget;
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

		// Sanity check: verify EIP is still reasonable after instruction execution
		// For non-control-flow instructions, EIP should be the value set by the decoder
		// For control-flow instructions (JMP, CALL, RET), EIP will be different
		var eipChanged = (_eip != (uint)_decoder.IP);
		if (eipChanged)
		{
			// EIP was modified by the instruction - this is expected for JMP, CALL, RET, conditional jumps
			var isControlFlow = insn.Mnemonic switch
			{
				Mnemonic.Jmp => true,
				Mnemonic.Call => true,
				Mnemonic.Ret => true,
				Mnemonic.Iretd => true,
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
				Mnemonic.Loop => true,
				Mnemonic.Loope => true,
				Mnemonic.Loopne => true,
				_ => false
			};
			
			if (!isControlFlow)
			{
				_logger.LogError("[IcedCpu] EIP corrupted! At 0x{OldEip:X8}, instruction '{Insn}' changed EIP to 0x{NewEip:X8} (decoder expected 0x{DecoderIP:X8})",
					oldEip, insn.ToString(), _eip, _decoder.IP);
			}
		}

		return new CpuStepResult(isCall, callTarget, isSyscall);
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
		var val = ReadOp(insn, 0);
		Push32(val);
	}

	private void ExecPop(Instruction insn)
	{
		var v = Pop32();
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
				
				var r = (ushort)(a + b);
				
				if (insn.GetOpKind(0) == OpKind.Register)
				{
					SetReg16(insn.GetOpRegister(0), r);
				}
				else if (insn.GetOpKind(0) == OpKind.Memory)
				{
					_mem.Write16(CalcMemAddress(insn), r);
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
		uint a = ReadOp(insn, 0), b = ReadOp(insn, 1);
		var cf = GetFlag(Cf) ? 1u : 0u;
		var sum = (ulong)a + b + cf;
		var r = (uint)sum;
		WriteOp(insn, 0, r);
		SetFlagVal(Cf, (sum >> 32) != 0);
		SetFlagVal(Of, (~(a ^ b) & (a ^ r) & 0x80000000) != 0);
		SetFlagVal(Af, (((a ^ b ^ r) & 0x10) != 0));
		UpdateLogicResultFlags(r);
	}

	private void ExecSub(Instruction insn)
	{
		uint a = ReadOp(insn, 0), b = ReadOp(insn, 1), r = a - b;
		WriteOp(insn, 0, r);
		SetFlagsSub(a, b, r);
	}

	private void ExecSbb(Instruction insn)
	{
		uint a = ReadOp(insn, 0), b = ReadOp(insn, 1);
		var cf = GetFlag(Cf) ? 1u : 0u;
		var diff = (ulong)a - (b + cf);
		var r = (uint)diff;
		WriteOp(insn, 0, r);
		SetFlagVal(Cf, a < b + cf);
		SetFlagVal(Of, ((a ^ b) & (a ^ r) & 0x80000000) != 0);
		SetFlagVal(Af, (((a ^ b ^ r) & 0x10) != 0));
		UpdateLogicResultFlags(r);
	}

	private void ExecXor(Instruction insn)
	{
		var r = ReadOp(insn, 0) ^ ReadOp(insn, 1);
		WriteOp(insn, 0, r);
		ClearFlag(Cf);
		ClearFlag(Of);
		ClearFlag(Af);
		UpdateLogicResultFlags(r);
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
				UpdateLogicResultFlags(r);
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
				UpdateLogicResultFlags(r);
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
		uint a = ReadOp(insn, 0), r = a + 1;
		WriteOp(insn, 0, r);
		SetFlagsIncDecAdd(a, r);
	}

	private void ExecDec(Instruction insn)
	{
		uint a = ReadOp(insn, 0), r = a - 1;
		WriteOp(insn, 0, r);
		SetFlagsIncDecSub(a, r);
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

		var r = a << c;
		var lastOut = (a >> (32 - c)) & 1u;
		SetFlagVal(Cf, lastOut != 0);
		if (c == 1)
		{
			bool before = (a & 0x80000000) != 0, after = (r & 0x80000000) != 0;
			SetFlagVal(Of, before ^ after);
		}
		else
		{
			ClearFlag(Of);
		}

		ClearFlag(Af);
		WriteOp(insn, 0, r);
		UpdateLogicResultFlags(r);
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

		uint r;
		if (arithmetic)
		{
			var s = (int)a;
			r = (uint)(s >> c);
			SetFlagVal(Of, false);
		}
		else
		{
			r = a >> c;
			if (c == 1)
			{
				SetFlagVal(Of, (a & 0x80000000) != 0);
			}
			else
			{
				ClearFlag(Of);
			}
		}

		var lastOut = (a >> (c - 1)) & 1u;
		SetFlagVal(Cf, lastOut != 0);
		ClearFlag(Af);
		WriteOp(insn, 0, r);
		UpdateLogicResultFlags(r);
	}

	private void ExecRotate(Instruction insn, RotateKind kind)
	{
		var a = ReadOp(insn, 0);
		var c = GetShiftCount(insn);
		if (c == 0)
		{
			return;
		}

		if (kind is RotateKind.Rol or RotateKind.Ror)
		{
			c &= 0x1F;
		}
		else
		{
			c %= 33;
		}

		if (c == 0)
		{
			return;
		}

		var r = a;
		switch (kind)
		{
			case RotateKind.Rol:
				r = (a << c) | (a >> (32 - c));
				SetFlagVal(Cf, (r & 1) != 0);
				if (c == 1)
				{
					var msb = (r & 0x80000000) != 0;
					var cf = GetFlag(Cf);
					SetFlagVal(Of, msb ^ cf);
				}
				else
				{
					ClearFlag(Of);
				}

				break;
			case RotateKind.Ror:
				r = (a >> c) | (a << (32 - c));
				SetFlagVal(Cf, ((r >> 31) & 1) != 0);
				if (c == 1)
				{
					var bit31 = (r & 0x80000000) != 0;
					var bit30 = (r & 0x40000000) != 0;
					SetFlagVal(Of, bit31 ^ bit30);
				}
				else
				{
					ClearFlag(Of);
				}

				break;
			case RotateKind.Rcl:
				for (var i = 0; i < c; i++)
				{
					var carry = GetFlag(Cf) ? 1u : 0u;
					var newCarry = (a >> 31) & 1u;
					r = (a << 1) | carry;
					SetFlagVal(Cf, newCarry != 0);
					a = r;
				}

				if (c == 1)
				{
					SetFlagVal(Of, ((a ^ r) & 0x80000000) != 0);
				}
				else
				{
					ClearFlag(Of);
				}

				break;
			case RotateKind.Rcr:
				for (var i = 0; i < c; i++)
				{
					var carry = GetFlag(Cf) ? 1u : 0u;
					var newCarry = a & 1u;
					r = (a >> 1) | (carry << 31);
					SetFlagVal(Cf, newCarry != 0);
					a = r;
				}

				if (c == 1)
				{
					SetFlagVal(Of, ((a ^ r) & 0x80000000) != 0);
				}
				else
				{
					ClearFlag(Of);
				}

				break;
		}

		WriteOp(insn, 0, r);
		// Note: Rotate instructions only affect CF and OF flags, not ZF, SF, PF
	}

	private void ExecNot(Instruction insn)
	{
		var a = ReadOp(insn, 0);
		var r = ~a;
		WriteOp(insn, 0, r);
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
					SetReg8(insn.GetOpRegister(0), r);
				else
					_mem.Write8(CalcMemAddress(insn), r);
				SetFlagsSub(0, a, r, 0x80);
				break;
			}
			case 16:
			{
				ushort a = insn.GetOpKind(0) == OpKind.Register ? GetReg16(insn.GetOpRegister(0)) : _mem.Read16(CalcMemAddress(insn));
				ushort r = (ushort)(0 - a);
				if (insn.GetOpKind(0) == OpKind.Register)
					SetReg16(insn.GetOpRegister(0), r);
				else
					_mem.Write16(CalcMemAddress(insn), r);
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
		// Input: EAX = function number
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
				_eax = 0x00000600; // Family 6, Model 0, Stepping 0
				_ebx = 0x00000000; // Brand index, CLFLUSH line size, etc.
				_ecx = CpuIntrinsics.GetCpuidEcxFeatures(); // Feature flags based on host CPU
				_edx = CpuIntrinsics.GetCpuidEdxFeatures(); // Feature flags based on host CPU
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
				_eax = 0x00000600; // Extended processor signature (same as function 1)
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

	private void ExecMovs(int size, bool rep)
	{
		var count = rep ? _ecx : 1u;
		var delta = GetFlag(Df) ? -size : size;
		for (uint i = 0; i < count; i++)
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

		if (rep)
		{
			_ecx = 0;
		}
	}

	private void ExecStos(int size, bool rep)
	{
		var count = rep ? _ecx : 1u;
		var delta = GetFlag(Df) ? -size : size;
		var src = size switch
		{
			1 => (byte)_eax,
			2 => (ushort)_eax,
			_ => _eax
		};
		for (uint i = 0; i < count; i++)
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

		if (rep)
		{
			_ecx = 0;
		}
	}

	private void ExecLods(int size, bool rep)
	{
		var count = rep ? _ecx : 1u;
		var delta = GetFlag(Df) ? -size : size;
		for (uint i = 0; i < count; i++)
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

		if (rep)
		{
			_ecx = 0;
		}
	}

	private void ExecIns(int size, bool rep)
	{
		// INS reads from I/O port DX and writes to [EDI]
		// Since I/O ports are not fully emulated, we write 0 (similar to IN instruction handling)
		var count = rep ? _ecx : 1u;
		var delta = GetFlag(Df) ? -size : size;
		for (uint i = 0; i < count; i++)
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

		if (rep)
		{
			_ecx = 0;
		}
	}

	private void ExecOuts(int size, bool rep)
	{
		// OUTS reads from [ESI] and writes to I/O port DX
		// Since I/O ports are not fully emulated, we just read and discard (similar to OUT instruction handling)
		var count = rep ? _ecx : 1u;
		var delta = GetFlag(Df) ? -size : size;
		for (uint i = 0; i < count; i++)
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

		if (rep)
		{
			_ecx = 0;
		}
	}

	private void ExecCmps(int size, bool repe, bool repne)
	{
		var count = (repe || repne) ? _ecx : 1u;
		var delta = GetFlag(Df) ? -size : size;
		for (uint i = 0; i < count; i++)
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
			_ecx--;
			if (repe && !GetFlag(Zf))
			{
				break; // stop when not equal
			}

			if (repne && GetFlag(Zf))
			{
				break; // stop when equal
			}
		}
	}

	private void ExecScas(int size, bool repe, bool repne)
	{
		var count = (repe || repne) ? _ecx : 1u;
		var delta = GetFlag(Df) ? -size : size;
		var a = size switch
		{
			1 => (byte)_eax,
			2 => (ushort)_eax,
			_ => _eax
		};
		for (uint i = 0; i < count; i++)
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
			_ecx--;
			if (repe && !GetFlag(Zf))
			{
				break;
			}

			if (repne && GetFlag(Zf))
			{
				break;
			}
		}
	}

	private void ExecMul(Instruction insn)
	{
		// Only 32-bit form: EDX:EAX = EAX * r/m32 (unsigned)
		var src = ReadOp(insn, 0);
		var prod = _eax * (ulong)src;
		_eax = (uint)prod;
		_edx = (uint)(prod >> 32);
		var carry = _edx != 0;
		SetFlagVal(Cf, carry);
		SetFlagVal(Of, carry);
		// Other flags undefined; leave as-is except clear AF.
		ClearFlag(Af);
	}

	private void ExecImul(Instruction insn)
	{
		if (insn.OpCount == 1)
		{
			var prod = (int)_eax * (long)(int)ReadOp(insn, 0);
			_eax = (uint)prod;
			_edx = (uint)(prod >> 32);
			var overflow = (_edx != 0 && _edx != 0xFFFFFFFFu) || (((prod >> 31) & 1) != ((prod >> 32) & 1));
			SetFlagVal(Cf, overflow);
			SetFlagVal(Of, overflow);
			ClearFlag(Af);
		}
		else
		{
			var prod = (int)ReadOp(insn, 1) *
			           (long)(insn.OpCount >= 3 ? (int)ReadOp(insn, 2) : (int)ReadOp(insn, 1));
			var r = (uint)prod;
			WriteOp(insn, 0, r);
			var overflow = prod is > int.MaxValue or < int.MinValue;
			SetFlagVal(Cf, overflow);
			SetFlagVal(Of, overflow);
			ClearFlag(Af);
		}
	}

	private void ExecDiv(Instruction insn)
	{
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
	}

	private void ExecIdiv(Instruction insn)
	{
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

	private void ExecBt(Instruction insn)
	{
		// BT - Bit test
		var bitBase = ReadOp(insn, 0);
		var bitOffset = ReadOp(insn, 1);
		var bitPos = (int)(bitOffset & 0x1F); // Modulo 32 for 32-bit operands
		var bitValue = (bitBase >> bitPos) & 1;
		SetFlagVal(Cf, bitValue != 0);
	}

	private void ExecBts(Instruction insn)
	{
		// BTS - Bit test and set
		var bitBase = ReadOp(insn, 0);
		var bitOffset = ReadOp(insn, 1);
		var bitPos = (int)(bitOffset & 0x1F); // Modulo 32 for 32-bit operands
		var bitValue = (bitBase >> bitPos) & 1;
		SetFlagVal(Cf, bitValue != 0);
		// Set the bit
		bitBase |= (1u << bitPos);
		WriteOp(insn, 0, bitBase);
	}

	private void ExecBtr(Instruction insn)
	{
		// BTR - Bit test and reset
		var bitBase = ReadOp(insn, 0);
		var bitOffset = ReadOp(insn, 1);
		var bitPos = (int)(bitOffset & 0x1F); // Modulo 32 for 32-bit operands
		var bitValue = (bitBase >> bitPos) & 1;
		SetFlagVal(Cf, bitValue != 0);
		// Reset (clear) the bit
		bitBase &= ~(1u << bitPos);
		WriteOp(insn, 0, bitBase);
	}

	private void ExecBtc(Instruction insn)
	{
		// BTC - Bit test and complement
		var bitBase = ReadOp(insn, 0);
		var bitOffset = ReadOp(insn, 1);
		var bitPos = (int)(bitOffset & 0x1F); // Modulo 32 for 32-bit operands
		var bitValue = (bitBase >> bitPos) & 1;
		SetFlagVal(Cf, bitValue != 0);
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
		
		if (src == 0)
		{
			// No bits set - set ZF, destination is undefined (we'll leave it unchanged)
			SetFlagVal(Zf, true);
		}
		else
		{
			// Find first set bit
			uint bitIndex = 0;
			while ((src & (1u << (int)bitIndex)) == 0)
			{
				bitIndex++;
			}
			WriteOp(insn, 0, bitIndex);
			SetFlagVal(Zf, false);
		}
	}

	private void ExecBsr(Instruction insn)
	{
		// BSR - Bit Scan Reverse
		// Scans source operand for last set bit (starting from bit 31 down to 0)
		// If found, stores bit index in destination and clears ZF
		// If not found (source is 0), sets ZF and destination is undefined
		var src = ReadOp(insn, 1);
		
		if (src == 0)
		{
			// No bits set - set ZF, destination is undefined (we'll leave it unchanged)
			SetFlagVal(Zf, true);
		}
		else
		{
			// Find last set bit (scan from high to low)
			uint bitIndex = 31;
			while ((src & (1u << (int)bitIndex)) == 0)
			{
				bitIndex--;
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
			return;
		
		count &= 0x1F; // Modulo 32
		
		// Shift dest left by count, filling with high bits of src
		// CF is set to the last bit shifted out
		var carryOut = ((dest >> (32 - count)) & 1) != 0;
		
		ulong combined = ((ulong)dest << 32) | src;
		combined <<= count;
		dest = (uint)(combined >> 32);
		
		// Set flags
		SetFlagVal(Cf, carryOut);
		SetFlagVal(Sf, (dest & 0x80000000) != 0);
		SetFlagVal(Zf, dest == 0);
		// OF is set only if count == 1 - check if sign bit changed (top two bits differ)
		if (count == 1)
			SetFlagVal(Of, ((dest >> 31) ^ ((dest >> 30) & 1)) != 0);
		
		WriteOp(insn, 0, dest);
	}

	private void ExecShrd(Instruction insn)
	{
		// SHRD - Double precision shift right
		var dest = ReadOp(insn, 0);
		var src = ReadOp(insn, 1);
		var count = (byte)(insn.Op2Kind == OpKind.Immediate8 ? insn.Immediate8 : (_ecx & 0x1F));
		
		if (count == 0)
			return;
		
		count &= 0x1F; // Modulo 32
		
		// Shift dest right by count, filling with low bits of src
		// CF is set to the last bit shifted out
		ulong combined = ((ulong)src << 32) | dest;
		var carryOut = ((combined >> (count - 1)) & 1) != 0;
		combined >>= count;
		dest = (uint)combined;
		
		// Set flags
		SetFlagVal(Cf, carryOut);
		SetFlagVal(Sf, (dest & 0x80000000) != 0);
		SetFlagVal(Zf, dest == 0);
		// OF is set only if count == 1
		if (count == 1)
			SetFlagVal(Of, (((dest >> 31) ^ ((dest >> 30) & 1)) != 0));
		
		WriteOp(insn, 0, dest);
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
		UpdateLogicResultFlags(al);
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
		UpdateLogicResultFlags(al);
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
		UpdateLogicResultFlags(al);
	}

	private void ExecDas()
	{
		// DAS - Decimal Adjust AL After Subtraction
		// Adjusts AL after packed BCD subtraction
		var al = (byte)(_eax & 0xFF);
		var oldAl = al;
		var oldCf = GetFlag(Cf);
		
		ClearFlag(Cf);
		
		// Step 1: Check low nibble
		if (((al & 0x0F) > 9) || GetFlag(Af))
		{
			al -= 6;
			SetFlagVal(Cf, oldCf || (al < oldAl)); // Set CF if borrow occurred
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
		UpdateLogicResultFlags(al);
	}

	private void ExecDaa()
	{
		// DAA - Decimal Adjust AL After Addition
		// Adjusts AL after packed BCD addition
		var al = (byte)(_eax & 0xFF);
		var oldAl = al;
		var oldCf = GetFlag(Cf);
		
		ClearFlag(Cf);
		
		// Step 1: Check low nibble
		if (((al & 0x0F) > 9) || GetFlag(Af))
		{
			al += 6;
			SetFlagVal(Cf, oldCf || (al < oldAl)); // Set CF if carry occurred
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
		UpdateLogicResultFlags(al);
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

	private void SetFlagsIncDecAdd(uint a, uint r)
	{
		SetFlagVal(Of, ((~(a ^ 1u) & (a ^ r) & 0x80000000) != 0));
		SetFlagVal(Af, ((a ^ 1u ^ r) & 0x10) != 0);
		UpdateLogicResultFlags(r);
	}

	private void SetFlagsIncDecSub(uint a, uint r)
	{
		SetFlagVal(Of, (((a ^ 0xFFFFFFFFu) & (a ^ r) & 0x80000000) != 0));
		SetFlagVal(Af, ((a ^ 1u ^ r) & 0x10) != 0); // Set if borrow from bit 4 (auxiliary carry) occurred
		UpdateLogicResultFlags(r);
	}

	private void UpdateLogicResultFlags(uint r)
	{
		UpdateLogicResultFlags(r, 0x80000000);
	}

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

	private bool GetFlag(int bit) => (_eflags & (1u << bit)) != 0;
	private void SetFlag(int bit) => _eflags |= (1u << bit);
	private void ClearFlag(int bit) => _eflags &= ~(1u << bit);

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

	private uint ReadOp(Instruction insn, int index) => insn.GetOpKind(index) switch
	{
		OpKind.Register => GetReg32(insn.GetOpRegister(index)), OpKind.Memory => Read32(CalcMemAddress(insn)),
		OpKind.Immediate8 => insn.Immediate8, OpKind.Immediate8to32 => (uint)(sbyte)insn.Immediate8,
		OpKind.Immediate32 => insn.Immediate32, _ => 0u
	};

	private void WriteOp(Instruction insn, int index, uint value)
	{
		switch (insn.GetOpKind(index))
		{
			case OpKind.Register: SetReg32(insn.GetOpRegister(index), value); break;
			case OpKind.Memory: Write32(CalcMemAddress(insn), value); break;
			default: _logger.LogWarning("[IcedCpu] WriteOp unsupported {GetOpKind}", insn.GetOpKind(index)); break;
		}
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

			if (r is Register.AX or Register.CX or Register.DX or Register.BX or Register.SI or Register.DI or Register.SP or Register.BP)
			{
				return 16;
			}

			return 32;
		}

		// For immediates, default to 32
		return 32;
	}

	// replace CalcMemAddress to report via Diagnostics on failure
	private uint CalcMemAddress(Instruction insn)
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

		// Debug logging for IAT address calculations (displacement in IAT range 0x004552E0-0x00455360)
		// This will catch the problematic LoadIconA read from 0x004552F8
		if (insn.MemoryDisplacement32 >= 0x004552E0 && insn.MemoryDisplacement32 <= 0x00455360)
		{
			_logger.LogWarning("[IcedCpu] CalcMemAddress for IAT: EIP=0x{Eip:X8}, disp=0x{Disp:X8}, base={Base}, baseVal=0x{BaseVal:X8}, index={Index}, indexVal=0x{IndexVal:X8}, scale={Scale}, finalAddr=0x{Addr:X8}",
				_eip, insn.MemoryDisplacement32, insn.MemoryBase,
				insn.MemoryBase != Register.None ? GetReg32(insn.MemoryBase) : 0,
				insn.MemoryIndex,
				insn.MemoryIndex != Register.None ? GetReg32(insn.MemoryIndex) : 0,
				insn.MemoryIndexScale,
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

	private uint GetReg32(Register reg) => reg switch
	{
		Register.EAX => _eax, Register.EBX => _ebx, Register.ECX => _ecx, Register.EDX => _edx,
		Register.ESI => _esi, Register.EDI => _edi, Register.EBP => _ebp, Register.ESP => _esp, _ => 0
	};

	private ushort GetReg16(Register reg) => reg switch
	{
		Register.AX => (ushort)_eax, Register.BX => (ushort)_ebx, Register.CX => (ushort)_ecx,
		Register.DX => (ushort)_edx, Register.SI => (ushort)_esi, Register.DI => (ushort)_edi,
		Register.BP => (ushort)_ebp, Register.SP => (ushort)_esp, _ => 0
	};

	private byte GetReg8(Register reg) => reg switch
	{
		Register.AL => (byte)(_eax & 0xFF), Register.CL => (byte)(_ecx & 0xFF), Register.DL => (byte)(_edx & 0xFF),
		Register.BL => (byte)(_ebx & 0xFF), Register.AH => (byte)((_eax >> 8) & 0xFF),
		Register.CH => (byte)((_ecx >> 8) & 0xFF), Register.DH => (byte)((_edx >> 8) & 0xFF),
		Register.BH => (byte)((_ebx >> 8) & 0xFF), _ => 0
	};

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
			default:
				throw new ArgumentOutOfRangeException(nameof(reg), reg, "Invalid 16-bit register specified in SetReg16.");
		}
	}

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
	private void Write32(uint addr, uint v)
	{
		_mem.Write32(addr, v);
	}
	private ushort Read16(uint addr) => _mem.Read16(addr);
	private void Write16(uint addr, ushort v) => _mem.Write16(addr, v);

	private void Push32(uint v)
	{
		_esp -= 4;
		Write32(_esp, v);
	}

	private uint Pop32()
	{
		var v = Read32(_esp);
		_esp += 4;
		return v;
	}

	#region FPU Helpers

	// Get ST(i) - ST(0) is the top of stack
	private double FpuGetSt(int i)
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
	private double FpuPop()
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
			FpuTagWord = _fpuTagWord
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
	}

	#endregion

	private sealed class SimpleMemoryCodeReader(IcedCpu cpu) : CodeReader
	{
		private uint _ptr;
		public void Reset(uint ip) => _ptr = ip;
		public override int ReadByte() => cpu._mem.Read8(_ptr++);
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
