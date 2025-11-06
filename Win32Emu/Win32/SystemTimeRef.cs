using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	/// <summary>
	/// Ref struct wrapper for SYSTEMTIME with automatic memory read/write.
	/// Specifies a date and time using individual members.
	/// </summary>
	public readonly ref struct SystemTimeRef
	{
		private readonly VirtualMemory _memory;
		private readonly uint _address;

		public SystemTimeRef(VirtualMemory memory, uint address)
		{
			_memory = memory;
			_address = address;
		}

		public ushort wYear
		{
			get => _memory.Read16(_address + 0);
			set => _memory.Write16(_address + 0, value);
		}

		public ushort wMonth
		{
			get => _memory.Read16(_address + 2);
			set => _memory.Write16(_address + 2, value);
		}

		public ushort wDayOfWeek
		{
			get => _memory.Read16(_address + 4);
			set => _memory.Write16(_address + 4, value);
		}

		public ushort wDay
		{
			get => _memory.Read16(_address + 6);
			set => _memory.Write16(_address + 6, value);
		}

		public ushort wHour
		{
			get => _memory.Read16(_address + 8);
			set => _memory.Write16(_address + 8, value);
		}

		public ushort wMinute
		{
			get => _memory.Read16(_address + 10);
			set => _memory.Write16(_address + 10, value);
		}

		public ushort wSecond
		{
			get => _memory.Read16(_address + 12);
			set => _memory.Write16(_address + 12, value);
		}

		public ushort wMilliseconds
		{
			get => _memory.Read16(_address + 14);
			set => _memory.Write16(_address + 14, value);
		}

		/// <summary>
		/// Converts this ref struct to a value struct snapshot.
		/// </summary>
		public NativeTypes.SYSTEMTIME ToStruct()
		{
			return new NativeTypes.SYSTEMTIME
			{
				wYear = wYear,
				wMonth = wMonth,
				wDayOfWeek = wDayOfWeek,
				wDay = wDay,
				wHour = wHour,
				wMinute = wMinute,
				wSecond = wSecond,
				wMilliseconds = wMilliseconds
			};
		}

		/// <summary>
		/// Implicit conversion to the underlying value struct.
		/// </summary>
		public static implicit operator NativeTypes.SYSTEMTIME(SystemTimeRef refStruct) => refStruct.ToStruct();
	}
}