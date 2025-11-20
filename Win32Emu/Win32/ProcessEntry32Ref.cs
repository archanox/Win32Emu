using System.Text;
using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	/// <summary>
	/// Ref struct wrapper for PROCESSENTRY32 with automatic memory read/write.
	/// Describes an entry from a list of the processes residing in the system address space.
	/// </summary>
	public readonly ref struct ProcessEntry32Ref
	{
		private readonly VirtualMemory _memory;
		private readonly uint _address;

		public ProcessEntry32Ref(VirtualMemory memory, uint address)
		{
			_memory = memory;
			_address = address;
		}

		public uint dwSize
		{
			get => _memory.Read32(_address + 0);
			set => _memory.Write32(_address + 0, value);
		}

		public uint cntUsage
		{
			get => _memory.Read32(_address + 4);
			set => _memory.Write32(_address + 4, value);
		}

		public uint th32ProcessID
		{
			get => _memory.Read32(_address + 8);
			set => _memory.Write32(_address + 8, value);
		}

		public uint th32DefaultHeapID
		{
			get => _memory.Read32(_address + 12);
			set => _memory.Write32(_address + 12, value);
		}

		public uint th32ModuleID
		{
			get => _memory.Read32(_address + 16);
			set => _memory.Write32(_address + 16, value);
		}

		public uint cntThreads
		{
			get => _memory.Read32(_address + 20);
			set => _memory.Write32(_address + 20, value);
		}

		public uint th32ParentProcessID
		{
			get => _memory.Read32(_address + 24);
			set => _memory.Write32(_address + 24, value);
		}

		public int pcPriClassBase
		{
			get => (int)_memory.Read32(_address + 28);
			set => _memory.Write32(_address + 28, (uint)value);
		}

		public uint dwFlags
		{
			get => _memory.Read32(_address + 32);
			set => _memory.Write32(_address + 32, value);
		}

		/// <summary>
		/// Gets the executable file name (MAX_PATH = 260 bytes).
		/// </summary>
		public string szExeFile
		{
			get
			{
				var buf = new List<byte>();
				for (uint i = 0; i < 260; i++)
				{
					var b = _memory.Read8(_address + 36 + i);
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
				_memory.WriteBytes(_address + 36, bytes.AsSpan(0, length));
				_memory.Write8(_address + 36 + (uint)length, 0); // Null terminator
			}
		}

		public override string ToString()
		{
			return $"PROCESSENTRY32 {{ dwSize={dwSize}, th32ProcessID={th32ProcessID}, cntThreads={cntThreads}, th32ParentProcessID={th32ParentProcessID}, pcPriClassBase={pcPriClassBase}, szExeFile=\"{szExeFile}\" }}";
		}
	}
}
