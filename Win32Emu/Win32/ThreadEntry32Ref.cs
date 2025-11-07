using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	/// <summary>
	/// Ref struct wrapper for THREADENTRY32 with automatic memory read/write.
	/// Describes an entry from a list of the threads executing in the system.
	/// </summary>
	public readonly ref struct ThreadEntry32Ref
	{
		private readonly VirtualMemory _memory;
		private readonly uint _address;

		public ThreadEntry32Ref(VirtualMemory memory, uint address)
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

		public uint th32ThreadID
		{
			get => _memory.Read32(_address + 8);
			set => _memory.Write32(_address + 8, value);
		}

		public uint th32OwnerProcessID
		{
			get => _memory.Read32(_address + 12);
			set => _memory.Write32(_address + 12, value);
		}

		public int tpBasePri
		{
			get => (int)_memory.Read32(_address + 16);
			set => _memory.Write32(_address + 16, (uint)value);
		}

		public int tpDeltaPri
		{
			get => (int)_memory.Read32(_address + 20);
			set => _memory.Write32(_address + 20, (uint)value);
		}

		public uint dwFlags
		{
			get => _memory.Read32(_address + 24);
			set => _memory.Write32(_address + 24, value);
		}

		public override string ToString()
		{
			return $"THREADENTRY32 {{ dwSize={dwSize}, th32ThreadID={th32ThreadID}, th32OwnerProcessID={th32OwnerProcessID}, tpBasePri={tpBasePri}, tpDeltaPri={tpDeltaPri} }}";
		}
	}
}
