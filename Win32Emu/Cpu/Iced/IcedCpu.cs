using System.Diagnostics;
using Iced.Intel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Memory;

namespace Win32Emu.Cpu.Iced;

public class IcedCpu : ICpu
{
	private readonly VirtualMemory _mem;
	private readonly ILogger _logger;

	private uint _eax, _ebx, _ecx, _edx, _esi, _edi, _ebp, _esp, _eip, _eflags;

	private readonly Decoder _decoder;
	private readonly SimpleMemoryCodeReader _reader;

	// EFLAGS bit positions
	private const int Cf = 0, Pf = 2, Af = 4, Zf = 6, Sf = 7, Tf = 8, If = 9, Df = 10, Of = 11;

	// RDTSC support - use Stopwatch for high-resolution timing
	private static readonly Stopwatch RdtscStopwatch = Stopwatch.StartNew();
	private static readonly bool RdtscIsHighResolution = Stopwatch.IsHighResolution;
	private static readonly long RdtscFrequency = Stopwatch.Frequency;

	public IcedCpu(VirtualMemory mem, ILogger? logger = null)
	{
		_mem = mem;
		_logger = logger ?? NullLogger.Instance;
		_reader = new SimpleMemoryCodeReader(this);
		_decoder = Decoder.Create(32, _reader);
	}

	public void SetEip(uint eip) => _eip = eip;
	public uint GetEip() => _eip;

	public uint GetRegister(string name) => name.ToUpperInvariant() switch
	{
		"EAX" => _eax, "EBX" => _ebx, "ECX" => _ecx, "EDX" => _edx, "ESI" => _esi, "EDI" => _edi, "EBP" => _ebp,
		"ESP" => _esp, "EIP" => _eip, "EFLAGS" => _eflags, _ => 0
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

		_reader.Reset(_eip);
		_decoder.IP = _eip;
		var insn = _decoder.Decode();
		var oldEip = _eip;
		_eip = (uint)_decoder.IP;
		var isCall = false;
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
				case Mnemonic.Mul: ExecMul(insn); break;
				case Mnemonic.Imul: ExecImul(insn); break;
				case Mnemonic.Div: ExecDiv(insn); break;
				case Mnemonic.Idiv: ExecIdiv(insn); break;
				case Mnemonic.Bswap: ExecBswap(insn); break;
				case Mnemonic.Xchg: ExecXchg(insn); break;
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
				case Mnemonic.Cmovge:
				case Mnemonic.Cmovg:
				case Mnemonic.Cmovl:
					ExecCmovcc(insn); break;
				// String ops (byte/dword variants)
				case Mnemonic.Movsb: ExecMovs(1, insn.HasRepPrefix); break;
				case Mnemonic.Movsd: ExecMovs(4, insn.HasRepPrefix); break;
				case Mnemonic.Stosb: ExecStos(1, insn.HasRepPrefix); break;
				case Mnemonic.Stosd: ExecStos(4, insn.HasRepPrefix); break;
				case Mnemonic.Lodsb: ExecLods(1, insn.HasRepPrefix); break;
				case Mnemonic.Lodsd: ExecLods(4, insn.HasRepPrefix); break;
				case Mnemonic.Jmp:
					if (insn.GetOpKind(0) == OpKind.Register)
					{
						_eip = GetReg32(insn.GetOpRegister(0));
					}
					else if (insn.GetOpKind(0) == OpKind.Memory)
					{
						_eip = Read32(CalcMemAddress(insn));
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
						_eip = GetReg32(insn.GetOpRegister(0));
						callTarget = _eip;
						isCall = true;
					}
					else if (insn.GetOpKind(0) == OpKind.Memory)
					{
						_eip = Read32(CalcMemAddress(insn));
						callTarget = _eip;
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
					_esp += 4;
					_eip = ret;
					if (insn.Immediate16 != 0)
					{
						_esp += insn.Immediate16;
					}

					break;
				case Mnemonic.Nop: break;
				case Mnemonic.Cld: ClearFlag(Df); break;
				case Mnemonic.Std: SetFlag(Df); break;
				case Mnemonic.Clc: ClearFlag(Cf); break;
				case Mnemonic.Stc: SetFlag(Cf); break;
				case Mnemonic.Cli: ClearFlag(If); break;
				case Mnemonic.Sti: SetFlag(If); break;
				case Mnemonic.Cmc: SetFlagVal(Cf, !GetFlag(Cf)); break;
				case Mnemonic.Pushfd:
					_esp -= 4;
					Write32(_esp, _eflags);
					break;
				case Mnemonic.Popfd:
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
				case Mnemonic.Int:
					// Handle INT instruction with immediate
					if (insn.Immediate8 == 3)
					{
						// This is an INT3 breakpoint - check if it's at a synthetic import address
						if (oldEip is >= 0x0F000000 and < 0x10000000)
						{
							// This is a synthetic import stub - signal this as a call
							isCall = true;
							callTarget = oldEip;

							// Don't actually execute the INT3, just treat it as a call
							// The main loop will handle the import invocation
						}
						else
						{
							// Regular INT3 - for now, just print a message and continue
							_logger.LogWarning($"[IcedCpu] INT3 breakpoint at 0x{oldEip:X8}");
						}
					}
					else
					{
						_logger.LogWarning($"[IcedCpu] Unhandled interrupt INT {insn.Immediate8:X2} at 0x{oldEip:X8}");
					}

					break;
				case Mnemonic.Int3:
					// Handle INT3 (0xCC) instruction used for import stubs
					if (oldEip is >= 0x0F000000 and < 0x10000000)
					{
						// This is a synthetic import stub - signal this as a call
						isCall = true;
						callTarget = oldEip;
						_logger.LogWarning($"[IcedCpu] Handling INT3 import stub at 0x{oldEip:X8}");

						// Don't actually execute the INT3, just treat it as a call
						// The main loop will handle the import invocation
					}
					else
					{
						// Regular INT3 - for now, just print a message and continue
						_logger.LogWarning($"[IcedCpu] INT3 breakpoint at 0x{oldEip:X8}");
					}

					break;
				case Mnemonic.In:
					//TODO: this is getting spammed all the time? regression?
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
						_logger.LogWarning($"[IcedCpu] Unhandled mnemonic {insn.Mnemonic} at 0x{oldEip:X8}");
					}

					break;
			}
		}
		finally
		{
			Diagnostics.Diagnostics.ClearCpuContext();
		}

		return new CpuStepResult(isCall, callTarget);
	}

	#region Exec helpers

	private void ExecMov(Instruction insn)
	{
		if (insn.OpCount < 2)
		{
			return;
		}

		var src = ReadOp(insn, 1);
		WriteOp(insn, 0, src);
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
		_ebp = Pop32();
		_ = Pop32();
		_ebx = Pop32();
		_edx = Pop32();
		_ecx = Pop32();
		_eax = Pop32();
	}

	private void ExecAdd(Instruction insn)
	{
		uint a = ReadOp(insn, 0), b = ReadOp(insn, 1), r = a + b;
		WriteOp(insn, 0, r);
		SetFlagsAdd(a, b, r);
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
		uint a = ReadOp(insn, 0), b = ReadOp(insn, 1), r = op == LogicOp.And ? a & b : a | b;
		WriteOp(insn, 0, r);
		ClearFlag(Cf);
		ClearFlag(Of);
		ClearFlag(Af);
		UpdateLogicResultFlags(r);
	}

	private void ExecTest(Instruction insn)
	{
		var r = ReadOp(insn, 0) & ReadOp(insn, 1);
		ClearFlag(Cf);
		ClearFlag(Of);
		ClearFlag(Af);
		UpdateLogicResultFlags(r);
	}

	private void ExecCmp(Instruction insn)
	{
		uint a = ReadOp(insn, 0), b = ReadOp(insn, 1), r = a - b;
		SetFlagsSub(a, b, r);
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
					var msb = (r & 0x80000000) != 0;
					var cf = GetFlag(Cf);
					SetFlagVal(Of, msb ^ cf);
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
		UpdateLogicResultFlags(r);
	}

	private void ExecNot(Instruction insn)
	{
		var a = ReadOp(insn, 0);
		var r = ~a;
		WriteOp(insn, 0, r);
	}

	private void ExecNeg(Instruction insn)
	{
		var a = ReadOp(insn, 0);
		var r = 0u - a;
		WriteOp(insn, 0, r);
		SetFlagsSub(0, a, r);
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

	private void ExecXchg(Instruction insn)
	{
		var a = ReadOp(insn, 0);
		var b = ReadOp(insn, 1);
		WriteOp(insn, 0, b);
		WriteOp(insn, 1, a);
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
			throw new DivideByZeroException();
		}

		var dividend = ((ulong)_edx << 32) | _eax;
		var q = dividend / divisor;
		if (q > 0xFFFFFFFFu)
		{
			throw new OverflowException("DIV overflow");
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
			throw new DivideByZeroException();
		}

		var dividend = ((long)_edx << 32) | _eax;
		var q = dividend / divisor;
		if (q is > int.MaxValue or < int.MinValue)
		{
			throw new OverflowException("IDIV overflow");
		}

		var r = (int)(dividend % divisor);
		_eax = (uint)(int)q;
		_edx = (uint)r;
	}

	#endregion

	#region Flags

	private void SetFlagsAdd(uint a, uint b, uint r)
	{
		SetFlagVal(Cf, r < a);
		SetFlagVal(Of, (~(a ^ b) & (a ^ r) & 0x80000000) != 0);
		SetFlagVal(Af, ((a ^ b ^ r) & 0x10) != 0);
		UpdateLogicResultFlags(r);
	}

	private void SetFlagsSub(uint a, uint b, uint r)
	{
		SetFlagVal(Cf, a < b);
		SetFlagVal(Of, ((a ^ b) & (a ^ r) & 0x80000000) != 0);
		SetFlagVal(Af, ((a ^ b ^ r) & 0x10) != 0);
		UpdateLogicResultFlags(r);
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
		SetFlagVal(Af, ((a ^ 0xFFFFFFFFu ^ r) & 0x10) != 0);
		UpdateLogicResultFlags(r);
	}

	private void UpdateLogicResultFlags(uint r)
	{
		SetFlagVal(Zf, r == 0);
		SetFlagVal(Sf, (r & 0x80000000) != 0);
		var lo = (byte)r;
		var bits = lo ^ (lo >> 4);
		bits &= 0xF;
		var even = (((0x6996 >> bits) & 1) == 1);
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
		Mnemonic.Cmove => GetFlag(Zf), Mnemonic.Cmovne => !GetFlag(Zf), Mnemonic.Cmovb => GetFlag(Cf),
		Mnemonic.Cmovbe => GetFlag(Cf) || GetFlag(Zf), Mnemonic.Cmova => !GetFlag(Cf) && !GetFlag(Zf),
		Mnemonic.Cmovge => GetFlag(Sf) == GetFlag(Of),
		Mnemonic.Cmovg => !GetFlag(Zf) && (GetFlag(Sf) == GetFlag(Of)),
		Mnemonic.Cmovl => GetFlag(Sf) != GetFlag(Of), _ => false
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
			default: _logger.LogWarning($"[IcedCpu] WriteOp unsupported {insn.GetOpKind(index)}"); break;
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

			Diagnostics.Diagnostics.LogCalcMemAddressFailure(addr, _mem.Size, _eip, _esp, _ebp, _eax, _ecx, _edx, instrBytes);
			throw new IndexOutOfRangeException($"Calculated memory address out of range: 0x{addr:X} (EIP=0x{_eip:X8})");
		}

		return addr;
	}

	private uint CalcLeaAddress(Instruction insn) => CalcMemAddress(insn);

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

	private void SetReg32(Register reg, uint v)
	{
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

	private uint Read32(uint addr) => _mem.Read32(addr);
	private void Write32(uint addr, uint v) => _mem.Write32(addr, v);

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