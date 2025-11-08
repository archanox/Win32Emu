using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules;

/// <summary>
/// VCRUNTIME140.DLL module - provides Visual C++ runtime functions.
/// This is the Visual C++ 2015-2022 runtime library.
/// </summary>
public partial class Vcruntime140Module : IWin32ModuleUnsafe
{
	private readonly ProcessEnvironment _env;
	private readonly uint _imageBase;
	private readonly PeImageLoader? _peLoader;
	private readonly ILogger _logger;

	public Vcruntime140Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
	{
		_env = env;
		_imageBase = imageBase;
		_peLoader = peLoader;
		_logger = logger ?? NullLogger.Instance;
	}

	public string Name => "VCRUNTIME140.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "MEMCPY":
				returnValue = Memcpy(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;

			case "MEMSET":
				returnValue = Memset(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;

			case "MEMCMP":
				returnValue = Memcmp(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;

			case "_CXXTHROWEXCEPTION":
				CxxThrowException(a.UInt32(0), a.UInt32(1));
				// Never returns - throws exception
				return true;

			default:
				LogUnimplementedExport(export);
				return false;
		}
	}

	/// <summary>
	/// Copies a block of memory from source to destination.
	/// void* memcpy(void* dest, const void* src, size_t count);
	/// </summary>
	[DllModuleExport(12)]
	private uint Memcpy(uint dest, uint src, uint count)
	{
		if (dest == 0 || src == 0 || count == 0)
		{
			LogMemcpy(dest, src, count);
			return dest;
		}

		// Handle overlapping regions safely (memmove semantics)
		if (dest > src && dest < src + count)
		{
			// Overlap with destination ahead of source: copy backwards
			for (uint i = count; i > 0; i--)
			{
				_env.MemWrite8(dest + i - 1, _env.MemRead8(src + i - 1));
			}
		}
		else
		{
			// No overlap or safe to copy forwards - use bulk operation
			var buffer = _env.Memory.GetSpan(src, (int)count);
			_env.Memory.WriteBytes(dest, buffer);
		}

		LogMemcpy(dest, src, count);
		return dest;
	}

	/// <summary>
	/// Sets a block of memory to a specified value.
	/// void* memset(void* dst, int val, size_t len);
	/// </summary>
	[DllModuleExport(12)]
	private uint Memset(uint dst, uint val, uint len)
	{
		if (dst == 0 || len == 0)
		{
			LogMemset(dst, val, len);
			return dst;
		}

		var byteVal = (byte)(val & 0xFF);
		
		// Use bulk operation for better performance
		var buffer = new byte[len];
		Array.Fill(buffer, byteVal);
		_env.Memory.WriteBytes(dst, buffer);

		LogMemset(dst, val, len);
		return dst;
	}

	/// <summary>
	/// Compares two blocks of memory.
	/// int memcmp(const void* lhs, const void* rhs, size_t len);
	/// </summary>
	/// <returns>0 if equal, &lt;0 if lhs &lt; rhs, &gt;0 if lhs &gt; rhs</returns>
	[DllModuleExport(12)]
	private uint Memcmp(uint lhs, uint rhs, uint len)
	{
		if (len == 0)
		{
			LogMemcmp(lhs, rhs, len, 0);
			return 0;
		}

		if (lhs == 0 || rhs == 0)
		{
			LogMemcmp(lhs, rhs, len, 0);
			return 0;
		}

		for (uint i = 0; i < len; i++)
		{
			byte left = _env.MemRead8(lhs + i);
			byte right = _env.MemRead8(rhs + i);

			if (left < right)
			{
				LogMemcmp(lhs, rhs, len, unchecked((uint)(-1)));
				return unchecked((uint)(-1)); // -1 as unsigned
			}
			if (left > right)
			{
				LogMemcmp(lhs, rhs, len, 1);
				return 1;
			}
		}

		LogMemcmp(lhs, rhs, len, 0);
		return 0; // Equal
	}

	/// <summary>
	/// Throws a C++ exception.
	/// This is a special function used by C++ code to throw exceptions.
	/// For now, we just log and terminate.
	/// </summary>
	[DllModuleExport(8)]
	private void CxxThrowException(uint pExceptionObject, uint pThrowInfo)
	{
		LogCxxThrowException(pExceptionObject, pThrowInfo);

		// In a real implementation, this would unwind the stack and search for exception handlers.
		// For now, just terminate the process as we don't support C++ exceptions.
		_logger.LogError("[Vcruntime140] C++ exception thrown - not supported. Terminating process.");
		_env.RequestExit();
	}

	// High-performance logging using source generators
	[LoggerMessage(Level = LogLevel.Information, Message = "[Vcruntime140] Unimplemented export: {Export}")]
	partial void LogUnimplementedExport(string export);

	[LoggerMessage(Level = LogLevel.Debug, Message = "[Vcruntime140] memcpy(0x{Dest:X8}, 0x{Src:X8}, {Count})")]
	partial void LogMemcpy(uint dest, uint src, uint count);

	[LoggerMessage(Level = LogLevel.Debug, Message = "[Vcruntime140] memset(0x{Dst:X8}, {Val}, {Len})")]
	partial void LogMemset(uint dst, uint val, uint len);

	[LoggerMessage(Level = LogLevel.Debug, Message = "[Vcruntime140] memcmp(0x{Lhs:X8}, 0x{Rhs:X8}, {Len}) -> {Result}")]
	partial void LogMemcmp(uint lhs, uint rhs, uint len, uint result);

	[LoggerMessage(Level = LogLevel.Error, Message = "[Vcruntime140] _CxxThrowException(0x{ExceptionObject:X8}, 0x{ThrowInfo:X8})")]
	partial void LogCxxThrowException(uint exceptionObject, uint throwInfo);
}
