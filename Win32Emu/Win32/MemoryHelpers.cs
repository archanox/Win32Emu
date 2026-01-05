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
}
