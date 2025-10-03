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
}