using System.Text;
using Win32Emu.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Win32;

/// <summary>
/// Helper methods for common memory operations and Win32 API patterns.
/// </summary>
public static class MemoryHelpers
{
	/// <summary>
	/// Maximum length for null-terminated strings to prevent infinite loops.
	/// </summary>
	private const uint MAX_STRING_LENGTH = 4096;

	/// <summary>
	/// Reads a null-terminated ASCII string from memory.
	/// </summary>
	/// <param name="memory">The virtual memory to read from.</param>
	/// <param name="address">The address to start reading from.</param>
	/// <param name="logger">Optional logger for diagnostics.</param>
	/// <returns>The read string, or empty string if an error occurs.</returns>
	public static string ReadNullTerminatedString(VirtualMemory memory, uint address, ILogger? logger = null)
	{
		var bytes = new List<byte>();
		uint offset = 0;

		try
		{
			while (offset < MAX_STRING_LENGTH)
			{
				var b = memory.Read8(address + offset);
				if (b == 0)
				{
					break;
				}

				bytes.Add(b);
				offset++;
			}

			return Encoding.ASCII.GetString(bytes.ToArray());
		}
		catch (Exception ex)
		{
			logger?.LogWarning(ex, "Error reading null-terminated string from memory at 0x{Address:X8} (offset {Offset}). Returning partial string.", address, offset);
			return bytes.Count > 0 ? Encoding.ASCII.GetString(bytes.ToArray()) : string.Empty;
		}
	}

	/// <summary>
	/// Reads a null-terminated ASCII string from memory using the ProcessEnvironment's memory accessor.
	/// </summary>
	/// <param name="env">The process environment.</param>
	/// <param name="address">The address to start reading from.</param>
	/// <param name="logger">Optional logger for diagnostics.</param>
	/// <returns>The read string, or empty string if an error occurs.</returns>
	public static string ReadNullTerminatedString(ProcessEnvironment env, uint address, ILogger? logger = null)
	{
		var bytes = new List<byte>();
		uint offset = 0;

		try
		{
			while (offset < MAX_STRING_LENGTH)
			{
				var b = env.MemRead8(address + offset);
				if (b == 0)
				{
					break;
				}

				bytes.Add(b);
				offset++;
			}

			return Encoding.ASCII.GetString(bytes.ToArray());
		}
		catch (Exception ex)
		{
			logger?.LogWarning(ex, "Error reading null-terminated string from memory at 0x{Address:X8} (offset {Offset}). Returning partial string.", address, offset);
			return bytes.Count > 0 ? Encoding.ASCII.GetString(bytes.ToArray()) : string.Empty;
		}
	}

	/// <summary>
	/// Validates that a pointer is not null (0).
	/// Useful for Win32 API parameter validation.
	/// </summary>
	/// <param name="pointer">The pointer value to validate.</param>
	/// <returns>True if the pointer is valid (non-zero), false otherwise.</returns>
	public static bool IsValidPointer(uint pointer)
	{
		return pointer != 0;
	}

	/// <summary>
	/// Validates that a pointer is not null (0) and sets ERROR_INVALID_PARAMETER if invalid.
	/// </summary>
	/// <param name="env">The process environment.</param>
	/// <param name="pointer">The pointer value to validate.</param>
	/// <param name="logger">Optional logger for diagnostics.</param>
	/// <param name="parameterName">Optional parameter name for logging.</param>
	/// <returns>True if the pointer is valid (non-zero), false otherwise.</returns>
	public static bool ValidatePointer(ProcessEnvironment env, uint pointer, ILogger? logger = null, string? parameterName = null)
	{
		if (pointer == 0)
		{
			env.LastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			if (logger != null && parameterName != null)
			{
				logger.LogWarning("Invalid parameter: {ParameterName} is null", parameterName);
			}
			return false;
		}
		return true;
	}

	/// <summary>
	/// Sets the ERROR_INVALID_PARAMETER error code in the process environment.
	/// </summary>
	/// <param name="env">The process environment.</param>
	/// <param name="logger">Optional logger for diagnostics.</param>
	/// <param name="message">Optional message to log.</param>
	public static void SetInvalidParameterError(ProcessEnvironment env, ILogger? logger = null, string? message = null)
	{
		env.LastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
		if (logger != null && message != null)
		{
			logger.LogWarning("Invalid parameter: {Message}", message);
		}
	}

	/// <summary>
	/// Validates a handle by attempting to retrieve it from a dictionary.
	/// This is a common pattern in Win32 module implementations.
	/// </summary>
	/// <typeparam name="T">The type of the handle value.</typeparam>
	/// <param name="handleDict">The dictionary containing handles.</param>
	/// <param name="handle">The handle to validate.</param>
	/// <param name="value">The retrieved value if the handle is valid.</param>
	/// <returns>True if the handle is valid and found in the dictionary, false otherwise.</returns>
	public static bool TryGetHandle<T>(Dictionary<uint, T> handleDict, uint handle, out T? value)
	{
		return handleDict.TryGetValue(handle, out value);
	}
}
