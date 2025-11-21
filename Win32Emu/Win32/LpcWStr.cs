using System.Text;
using Win32Emu.Memory;

namespace Win32Emu.Win32;

/// <summary>
/// Represents LPCWSTR (Long Pointer to Const Wide String) - a read-only Unicode string pointer.
/// This type wraps a memory address and provides Unicode string reading capabilities.
/// Corresponds to: typedef const wchar_t* LPCWSTR;
/// </summary>
public readonly struct LpcWStr(uint address, VirtualMemory? memory = null)
{
	public readonly uint Address = address;
	private readonly VirtualMemory? _memory = memory;

	/// <summary>
	/// Checks if this pointer is null (address is 0).
	/// </summary>
	public bool IsNull => Address == 0;

	/// <summary>
	/// Reads the Unicode string from virtual memory at this address.
	/// Returns null if the address is 0 (null pointer).
	/// </summary>
	/// <param name="mem">Virtual memory to read from (optional if memory was provided in constructor)</param>
	/// <param name="maxChars">Maximum number of characters to read</param>
	/// <returns>The string read from memory, or null if address is 0</returns>
	public string? Read(VirtualMemory? mem = null, int maxChars = int.MaxValue)
	{
		var memory = mem ?? _memory;
		if (IsNull || memory == null)
		{
			return null;
		}

		var buf = new List<char>();
		var addr = Address;
		for (var i = 0; i < maxChars; i++)
		{
			var wchar = memory.Read16(addr);
			if (wchar == 0)
			{
				break;
			}

			buf.Add((char)wchar);
			addr += 2;
		}
		return new string(buf.ToArray());
	}

	/// <summary>
	/// Returns the string value from memory, or null if the pointer is null.
	/// </summary>
	public override string? ToString()
	{
		return Read();
	}

	/// <summary>
	/// Implicit conversion from uint address to LpcWStr.
	/// </summary>
	public static implicit operator LpcWStr(uint address) => new(address);

	/// <summary>
	/// Implicit conversion from LpcWStr to uint address.
	/// </summary>
	public static implicit operator uint(LpcWStr lpcWStr) => lpcWStr.Address;
}
