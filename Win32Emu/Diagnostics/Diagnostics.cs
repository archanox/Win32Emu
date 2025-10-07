using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Diagnostics;

public static class Diagnostics
{
	private static readonly object Sync = new();
	private static CpuContext? _currentContext;
	private static ILogger _logger = NullLogger.Instance;

	/// <summary>
	/// Set the logger instance to use for diagnostics output
	/// </summary>
	public static void SetLogger(ILogger logger)
	{
		_logger = logger ?? NullLogger.Instance;
	}
	
	public static void SetCpuContext(CpuContext ctx)
	{
		_currentContext = ctx;
	}

	public static void ClearCpuContext() => _currentContext = null;

	public static void LogMemoryEnsureFailure(ulong addr, ulong length, ulong size)
	{
		var ctx = _currentContext;
		var sb = new StringBuilder();
		sb.Append($"Memory access out of range: addr=0x{addr:X}, len={length}, vm.size=0x{size:X}");
		if (ctx.HasValue)
		{
			var c = ctx.Value;
			sb.Append($"; EIP=0x{c.Eip:X8} ESP=0x{c.Esp:X8} EBP=0x{c.Ebp:X8} EAX=0x{c.Eax:X8}");
		}
		_logger.LogError(sb.ToString());
		if (ctx is { InstructionBytes: not null })
		{
			try
			{
				_logger.LogError($"Instruction bytes at EIP: {BitConverter.ToString(ctx.Value.InstructionBytes).Replace('-', ' ')}");
			}
			catch { }
		}
	}

	public static void LogCalcMemAddressFailure(uint addr, ulong vmSize, uint eip, uint esp, uint ebp, uint eax, uint ecx, uint edx, byte[]? instrBytes = null)
	{
		var sb = new StringBuilder();
		sb.Append($"Calculated memory address out of range: 0x{addr:X} (EIP=0x{eip:X8}) size=0x{vmSize:X}");
		sb.Append($"; ESP=0x{esp:X8} EBP=0x{ebp:X8} EAX=0x{eax:X8} ECX=0x{ecx:X8} EDX=0x{edx:X8}");
		_logger.LogError(sb.ToString());
		if (instrBytes != null)
		{
			_logger.LogError($"Instruction bytes at EIP: {BitConverter.ToString(instrBytes).Replace('-', ' ')}");
		}
	}

	public static void LogMemWrite(uint addr, int length, byte[] data)
	{
		_logger.LogError($"MemWrite addr=0x{addr:X8} len={length} data={Preview(data)}");
	}

	public static void LogMemRead(uint addr, int length)
	{
		_logger.LogError($"MemRead addr=0x{addr:X8} len={length}");
	}

	private static string Preview(byte[] data, int max = 32)
	{
		if (data == null || data.Length == 0)
		{
			return "<empty>";
		}

		var len = Math.Min(max, data.Length);
		return BitConverter.ToString(data, 0, len).Replace('-', ' ') + (data.Length > len ? " .." : "");
	}

	public readonly struct CpuContext
	{
		public readonly uint Eip, Esp, Ebp, Eax, Ecx, Edx;
		public readonly byte[]? InstructionBytes;
		public CpuContext(uint eip, uint esp, uint ebp, uint eax, uint ecx, uint edx, byte[]? instr)
		{
			Eip = eip; Esp = esp; Ebp = ebp; Eax = eax; Ecx = ecx; Edx = edx; InstructionBytes = instr;
		}
	}
}
