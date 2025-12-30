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
	private readonly VirtualMemory? _memory;

	public LpcStr(uint address, VirtualMemory? memory = null)
	{
		Address = address;
		_memory = memory;
	}

	/// <summary>
	/// Checks if this pointer is null (address is 0).
	/// </summary>
	public bool IsNull => Address == 0;

	/// <summary>
	/// Reads the ANSI string from virtual memory at this address.
	/// Returns null if the address is 0 (null pointer).
	/// </summary>
	/// <param name="mem">Virtual memory to read from (optional if memory was provided in constructor)</param>
	/// <returns>The string read from memory, or null if address is 0</returns>
	public string? Read(VirtualMemory? mem = null)
	{
		var memory = mem ?? _memory;
		if (IsNull || memory == null)
		{
			return null;
		}

		// Use stackalloc for small strings (most common case)
		const int stackAllocThreshold = 256;
		Span<byte> buffer = stackalloc byte[stackAllocThreshold];
		var length = 0;
		var a = Address;
		
		// First pass: try to fit in stack buffer
		while (length < stackAllocThreshold)
		{
			var b = memory.Read8(a++);
			if (b == 0)
			{
				return Encoding.ASCII.GetString(buffer[..length]);
			}
			buffer[length++] = b;
		}
		
		// String is larger - use array pool
		var rentedArray = System.Buffers.ArrayPool<byte>.Shared.Rent(1024);
		try
		{
			// Copy what we already read
			buffer.CopyTo(rentedArray);
			
			// Continue reading
			while (true)
			{
				var b = memory.Read8(a++);
				if (b == 0)
				{
					break;
				}
				
				// Grow array if needed
				if (length >= rentedArray.Length)
				{
					var newArray = System.Buffers.ArrayPool<byte>.Shared.Rent(rentedArray.Length * 2);
					Array.Copy(rentedArray, newArray, length);
					System.Buffers.ArrayPool<byte>.Shared.Return(rentedArray);
					rentedArray = newArray;
				}
				
				rentedArray[length++] = b;
			}
			
			return Encoding.ASCII.GetString(rentedArray, 0, length);
		}
		finally
		{
			System.Buffers.ArrayPool<byte>.Shared.Return(rentedArray);
		}
	}

	/// <summary>
	/// Returns the string value from memory, or null if the pointer is null.
	/// </summary>
	public override string? ToString()
	{
		return Read();
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
