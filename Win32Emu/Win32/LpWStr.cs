using System.Text;
using Win32Emu.Memory;

namespace Win32Emu.Win32;

public readonly struct LpWStr(uint address)
{
	public readonly uint Address = address;

	public void Write(VirtualMemory mem, string s, bool nullTerminate = true)
	{
		var bytes = Encoding.Unicode.GetBytes(nullTerminate ? s + "\0" : s);
		mem.WriteBytes(Address, bytes);
	}

	public string Read(VirtualMemory mem, int maxChars = int.MaxValue)
	{
		var buf = new List<char>();
		var addr = Address;
		for (var i = 0; i < maxChars; i++)
		{
			var wchar = mem.Read16(addr);
			if (wchar == 0)
			{
				break;
			}

			buf.Add((char)wchar);
			addr += 2;
		}
		return new string(buf.ToArray());
	}
}