using System.Text;
using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	/// <summary>
	/// Ref struct wrapper for MODULEENTRY32 with automatic memory read/write.
	/// Describes an entry from a list of the modules belonging to a process.
	/// </summary>
	public readonly ref struct ModuleEntry32Ref
	{
		private readonly VirtualMemory _memory;
		private readonly uint _address;

		public ModuleEntry32Ref(VirtualMemory memory, uint address)
		{
			_memory = memory;
			_address = address;
		}

		public uint dwSize
		{
			get => _memory.Read32(_address + 0);
			set => _memory.Write32(_address + 0, value);
		}

		public uint th32ModuleID
		{
			get => _memory.Read32(_address + 4);
			set => _memory.Write32(_address + 4, value);
		}

		public uint th32ProcessID
		{
			get => _memory.Read32(_address + 8);
			set => _memory.Write32(_address + 8, value);
		}

		public uint GlblcntUsage
		{
			get => _memory.Read32(_address + 12);
			set => _memory.Write32(_address + 12, value);
		}

		public uint ProccntUsage
		{
			get => _memory.Read32(_address + 16);
			set => _memory.Write32(_address + 16, value);
		}

		public uint modBaseAddr
		{
			get => _memory.Read32(_address + 20);
			set => _memory.Write32(_address + 20, value);
		}

		public uint modBaseSize
		{
			get => _memory.Read32(_address + 24);
			set => _memory.Write32(_address + 24, value);
		}

		public uint hModule
		{
			get => _memory.Read32(_address + 28);
			set => _memory.Write32(_address + 28, value);
		}

		/// <summary>
		/// Gets the module name (MAX_MODULE_NAME32 + 1 = 256 bytes).
		/// </summary>
		public string szModule
		{
			get
			{
				var buf = new List<byte>();
				for (uint i = 0; i < 256; i++)
				{
					var b = _memory.Read8(_address + 32 + i);
					if (b == 0)
					{
						break;
					}

					buf.Add(b);
				}
				return Encoding.ASCII.GetString(buf.ToArray());
			}
			set
			{
				var bytes = Encoding.ASCII.GetBytes(value);
				var length = Math.Min(bytes.Length, 255); // Leave room for null terminator
				_memory.WriteBytes(_address + 32, bytes.AsSpan(0, length));
				_memory.Write8(_address + 32 + (uint)length, 0); // Null terminator
			}
		}

		/// <summary>
		/// Gets the module path (MAX_PATH = 260 bytes).
		/// </summary>
		public string szExePath
		{
			get
			{
				var buf = new List<byte>();
				for (uint i = 0; i < 260; i++)
				{
					var b = _memory.Read8(_address + 288 + i);
					if (b == 0)
					{
						break;
					}

					buf.Add(b);
				}
				return Encoding.ASCII.GetString(buf.ToArray());
			}
			set
			{
				var bytes = Encoding.ASCII.GetBytes(value);
				var length = Math.Min(bytes.Length, 259); // Leave room for null terminator
				_memory.WriteBytes(_address + 288, bytes.AsSpan(0, length));
				_memory.Write8(_address + 288 + (uint)length, 0); // Null terminator
			}
		}

		public override string ToString()
		{
			return $"MODULEENTRY32 {{ dwSize={dwSize}, th32ModuleID={th32ModuleID}, th32ProcessID={th32ProcessID}, modBaseAddr=0x{modBaseAddr:X8}, modBaseSize=0x{modBaseSize:X8}, hModule=0x{hModule:X8}, szModule=\"{szModule}\", szExePath=\"{szExePath}\" }}";
		}
	}
}
