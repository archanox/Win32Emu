using System;
using System.Runtime.CompilerServices;
using Win32Emu.Memory;

namespace Win32Emu;

public static class Diagnostics
{
	public enum Level { Error, Warn, Info, Debug }

	private static readonly object _sync = new();
	private static CpuContext? _currentContext;

	public static void Log(Level level, string message)
	{
		lock (_sync)
		{
			Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [{level}] {message}");
		}
	}

	public static void LogDebug(string message) => Log(Level.Debug, message);
	public static void LogInfo(string message) => Log(Level.Info, message);
	public static void LogWarn(string message) => Log(Level.Warn, message);
	public static void LogError(string message) => Log(Level.Error, message);

	public static void SetCpuContext(CpuContext ctx)
	{
		_currentContext = ctx;
	}

	public static void ClearCpuContext() => _currentContext = null;

	public static void LogMemoryEnsureFailure(ulong addr, ulong length, ulong size)
	{
		var ctx = _currentContext;
		var sb = new System.Text.StringBuilder();
		sb.Append($"Memory access out of range: addr=0x{addr:X}, len={length}, vm.size=0x{size:X}");
		if (ctx.HasValue)
		{
			var c = ctx.Value;
			sb.Append($"; EIP=0x{c.EIP:X8} ESP=0x{c.ESP:X8} EBP=0x{c.EBP:X8} EAX=0x{c.EAX:X8}");
		}
		LogError(sb.ToString());
		if (ctx.HasValue && ctx.Value.InstructionBytes != null)
		{
			try
			{
				LogError($"Instruction bytes at EIP: {BitConverter.ToString(ctx.Value.InstructionBytes).Replace('-', ' ')}");
			}
			catch { }
		}
	}

	public static void LogCalcMemAddressFailure(uint addr, ulong vmSize, uint eip, uint esp, uint ebp, uint eax, uint ecx, uint edx, byte[]? instrBytes = null)
	{
		var sb = new System.Text.StringBuilder();
		sb.Append($"Calculated memory address out of range: 0x{addr:X} (EIP=0x{eip:X8}) size=0x{vmSize:X}");
		sb.Append($"; ESP=0x{esp:X8} EBP=0x{ebp:X8} EAX=0x{eax:X8} ECX=0x{ecx:X8} EDX=0x{edx:X8}");
		LogError(sb.ToString());
		if (instrBytes != null) LogError($"Instruction bytes at EIP: {BitConverter.ToString(instrBytes).Replace('-', ' ')}");
	}

	public static void LogMemWrite(uint addr, int length, byte[] data)
	{
		LogDebug($"MemWrite addr=0x{addr:X8} len={length} data={Preview(data)}");
	}

	public static void LogMemRead(uint addr, int length)
	{
		LogDebug($"MemRead addr=0x{addr:X8} len={length}");
	}

	private static string Preview(byte[] data, int max = 16)
	{
		if (data == null || data.Length == 0) return "<empty>";
		var len = Math.Min(max, data.Length);
		return BitConverter.ToString(data, 0, len).Replace('-', ' ') + (data.Length > len ? " .." : "");
	}

	public readonly struct CpuContext
	{
		public readonly uint EIP, ESP, EBP, EAX, ECX, EDX;
		public readonly byte[]? InstructionBytes;
		public CpuContext(uint eip, uint esp, uint ebp, uint eax, uint ecx, uint edx, byte[]? instr)
		{
			EIP = eip; ESP = esp; EBP = ebp; EAX = eax; ECX = ecx; EDX = edx; InstructionBytes = instr;
		}
	}
}
