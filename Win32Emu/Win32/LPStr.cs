using System.Text;
using Win32Emu.Memory;

namespace Win32Emu.Win32;

public readonly struct LpStr(uint address)
{
	public readonly uint Address = address;

	public void Write(VirtualMemory mem, string s, bool nullTerminate = true)
	{
		var bytes = Encoding.ASCII.GetBytes(nullTerminate ? s + "\0" : s);
		mem.WriteBytes(Address, bytes);
	}

	public string Read(VirtualMemory mem, int max = int.MaxValue)
	{
		// Use stackalloc for small strings (most common case)
		const int stackAllocThreshold = 256;
		
		if (max <= stackAllocThreshold)
		{
			Span<byte> buffer = stackalloc byte[stackAllocThreshold];
			var length = 0;
			var a = Address;
			
			for (var i = 0; i < max; i++)
			{
				var b = mem.Read8(a++);
				if (b == 0)
				{
					break;
				}
				buffer[length++] = b;
			}
			
			return Encoding.ASCII.GetString(buffer[..length]);
		}
		else
		{
			// For large max values, use array pool
			var rentedArray = System.Buffers.ArrayPool<byte>.Shared.Rent(Math.Min(max, 4096));
			try
			{
				var length = 0;
				var a = Address;
				
				for (var i = 0; i < max; i++)
				{
					var b = mem.Read8(a++);
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
	}
}