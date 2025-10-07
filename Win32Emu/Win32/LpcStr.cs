using System.Text;
using Win32Emu.Memory;

namespace Win32Emu.Win32;

/// <summary>
/// Represents LPCSTR (Long Pointer to Const String) - a read-only ANSI string pointer.
/// This type wraps a memory address and provides string reading capabilities.
/// Corresponds to: typedef const char* LPCSTR;
/// </summary>
public readonly struct LpcStr
{
	public readonly uint Address;

	public LpcStr(uint address)
	{
		Address = address;
	}

	/// <summary>
	/// Checks if this pointer is null (address is 0).
	/// </summary>
	public bool IsNull => Address == 0;

	/// <summary>
	/// Reads the ANSI string from virtual memory at this address.
	/// Returns null if the address is 0 (null pointer).
	/// </summary>
	/// <param name="mem">Virtual memory to read from</param>
	/// <param name="max">Maximum number of characters to read (default: int.MaxValue)</param>
	/// <returns>The string read from memory, or null if address is 0</returns>
	public string? Read(VirtualMemory mem, int max = int.MaxValue)
	{
		if (IsNull)
		{
			return null;
		}

		var buf = new List<byte>();
		var a = Address;
		for (var i = 0; i < max; i++)
		{
			var b = mem.Read8(a++);
			if (b == 0)
			{
				break;
			}

			buf.Add(b);
		}
		return Encoding.ASCII.GetString(buf.ToArray());
	}

	/// <summary>
	/// Implicit conversion from uint address to LpcStr.
	/// </summary>
	public static implicit operator LpcStr(uint address) => new(address);

	/// <summary>
	/// Implicit conversion from LpcStr to uint address.
	/// </summary>
	public static implicit operator uint(LpcStr lpcStr) => lpcStr.Address;
}
