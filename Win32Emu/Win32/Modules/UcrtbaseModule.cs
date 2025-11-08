using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules;

/// <summary>
/// UCRTBASE.DLL module - provides Universal C Runtime functions.
/// This is the modern C runtime library for Windows (Windows 10+).
/// </summary>
public partial class UcrtbaseModule : IWin32ModuleUnsafe
{
	private readonly ProcessEnvironment _env;
	private readonly uint _imageBase;
	private readonly PeImageLoader? _peLoader;
	private readonly ILogger _logger;

	public UcrtbaseModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
	{
		_env = env;
		_imageBase = imageBase;
		_peLoader = peLoader;
		_logger = logger ?? NullLogger.Instance;
	}

	public string Name => "UCRTBASE.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			// Memory management functions
			case "MALLOC":
				returnValue = Malloc(a.UInt32(0));
				return true;

			case "CALLOC":
				returnValue = Calloc(a.UInt32(0), a.UInt32(1));
				return true;

			case "FREE":
				Free(a.UInt32(0));
				returnValue = 0;
				return true;

			case "MEMSET":
				returnValue = Memset(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;

			case "MEMCPY":
				returnValue = Memcpy(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;

			case "STRLEN":
				returnValue = Strlen(a.UInt32(0));
				return true;

			default:
				LogUnimplementedExport(export);
				return false;
		}
	}

	/// <summary>
	/// Allocates a block of memory from the heap.
	/// </summary>
	[DllModuleExport(4)]
	private uint Malloc(uint size)
	{
		if (size == 0)
		{
			LogMalloc(size, 0);
			return 0;
		}

		// Use HeapAlloc with default heap (0)
		var ptr = _env.HeapAlloc(0, size);
		LogMalloc(size, ptr);
		return ptr;
	}

	/// <summary>
	/// Allocates an array from the heap and initializes to zero.
	/// </summary>
	[DllModuleExport(8)]
	private uint Calloc(uint count, uint size)
	{
		// Check for integer overflow before multiplication
		if (count != 0 && size > uint.MaxValue / count)
		{
			// Overflow would occur
			LogCalloc(count, size, 0);
			return 0;
		}

		var totalSize = count * size;
		if (totalSize == 0)
		{
			LogCalloc(count, size, 0);
			return 0;
		}

		// Allocate and zero-initialize
		var ptr = _env.HeapAlloc(0, totalSize);
		if (ptr != 0)
		{
			// Zero-initialize the memory using bulk operation
			var zeroBuffer = new byte[totalSize];
			_env.Memory.WriteBytes(ptr, zeroBuffer);
		}
		LogCalloc(count, size, ptr);
		return ptr;
	}

	/// <summary>
	/// Frees a block of memory allocated by malloc or calloc.
	/// </summary>
	[DllModuleExport(4)]
	private void Free(uint ptr)
	{
		if (ptr == 0)
		{
			LogFree(ptr, false);
			return;
		}

		var success = _env.HeapFree(0, ptr) != 0;
		LogFree(ptr, success);
	}

	/// <summary>
	/// Sets a block of memory to a specified value.
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
	/// Copies a block of memory from source to destination.
	/// </summary>
	[DllModuleExport(12)]
	private uint Memcpy(uint dest, uint src, uint count)
	{
		if (dest == 0 || src == 0 || count == 0)
		{
			LogMemcpy(dest, src, count);
			return dest;
		}

		// Handle overlapping regions: if dest > src and dest < src + count, copy backwards
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
	/// Returns the length of a null-terminated string.
	/// </summary>
	[DllModuleExport(4)]
	private uint Strlen(uint lpString)
	{
		if (lpString == 0)
		{
			LogStrlen(lpString, 0);
			return 0;
		}

		uint len = 0;
		const uint MAX_STRING_LENGTH = 0x7FFFFFFF; // 2GB, reasonable upper bound for emulated strings
		while (len < MAX_STRING_LENGTH && _env.MemRead8(lpString + len) != 0)
		{
			len++;
		}

		LogStrlen(lpString, len);
		return len;
	}

	// High-performance logging using source generators
	[LoggerMessage(Level = LogLevel.Information, Message = "[Ucrtbase] Unimplemented export: {Export}")]
	partial void LogUnimplementedExport(string export);

	[LoggerMessage(Level = LogLevel.Debug, Message = "[Ucrtbase] malloc({Size}) -> 0x{Result:X8}")]
	partial void LogMalloc(uint size, uint result);

	[LoggerMessage(Level = LogLevel.Debug, Message = "[Ucrtbase] calloc({Count}, {Size}) -> 0x{Result:X8}")]
	partial void LogCalloc(uint count, uint size, uint result);

	[LoggerMessage(Level = LogLevel.Debug, Message = "[Ucrtbase] free(0x{Ptr:X8}) -> {Success}")]
	partial void LogFree(uint ptr, bool success);

	[LoggerMessage(Level = LogLevel.Debug, Message = "[Ucrtbase] memset(0x{Dst:X8}, {Val}, {Len})")]
	partial void LogMemset(uint dst, uint val, uint len);

	[LoggerMessage(Level = LogLevel.Debug, Message = "[Ucrtbase] memcpy(0x{Dest:X8}, 0x{Src:X8}, {Count})")]
	partial void LogMemcpy(uint dest, uint src, uint count);

	[LoggerMessage(Level = LogLevel.Debug, Message = "[Ucrtbase] strlen(0x{Str:X8}) -> {Length}")]
	partial void LogStrlen(uint str, uint length);
}
