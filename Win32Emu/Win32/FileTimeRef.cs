using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	/// <summary>
	/// Ref struct wrapper for FILETIME with automatic memory read/write.
	/// 64-bit value representing the number of 100-nanosecond intervals since January 1, 1601 (UTC).
	/// </summary>
	public readonly ref struct FileTimeRef
	{
		private readonly VirtualMemory _memory;
		private readonly uint _address;

		public FileTimeRef(VirtualMemory memory, uint address)
		{
			_memory = memory;
			_address = address;
		}

		public uint dwLowDateTime
		{
			get => _memory.Read32(_address + 0);
			set => _memory.Write32(_address + 0, value);
		}

		public uint dwHighDateTime
		{
			get => _memory.Read32(_address + 4);
			set => _memory.Write32(_address + 4, value);
		}

		/// <summary>
		/// Converts this ref struct to a value struct snapshot.
		/// </summary>
		public NativeTypes.FILETIME ToStruct()
		{
			return new NativeTypes.FILETIME
			{
				dwLowDateTime = dwLowDateTime,
				dwHighDateTime = dwHighDateTime
			};
		}

		/// <summary>
		/// Implicit conversion to the underlying value struct.
		/// </summary>
		public static implicit operator NativeTypes.FILETIME(FileTimeRef refStruct) => refStruct.ToStruct();
	}
}