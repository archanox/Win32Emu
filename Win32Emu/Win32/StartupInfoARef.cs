using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	/// <summary>
	/// Ref struct wrapper for STARTUPINFOA that provides direct memory access via properties.
	/// Properties automatically read from and write to the underlying memory address.
	/// </summary>
	public readonly ref struct StartupInfoARef
	{
		private readonly VirtualMemory _memory;
		private readonly uint _address;

		public StartupInfoARef(VirtualMemory memory, uint address)
		{
			_memory = memory;
			_address = address;
		}

		public uint Address => _address;

		public uint cb
		{
			get => _memory.Read32(_address + 0);
			set => _memory.Write32(_address + 0, value);
		}

		public uint lpReserved
		{
			get => _memory.Read32(_address + 4);
			set => _memory.Write32(_address + 4, value);
		}

		public uint lpDesktop
		{
			get => _memory.Read32(_address + 8);
			set => _memory.Write32(_address + 8, value);
		}

		public uint lpTitle
		{
			get => _memory.Read32(_address + 12);
			set => _memory.Write32(_address + 12, value);
		}

		public uint dwX
		{
			get => _memory.Read32(_address + 16);
			set => _memory.Write32(_address + 16, value);
		}

		public uint dwY
		{
			get => _memory.Read32(_address + 20);
			set => _memory.Write32(_address + 20, value);
		}

		public uint dwXSize
		{
			get => _memory.Read32(_address + 24);
			set => _memory.Write32(_address + 24, value);
		}

		public uint dwYSize
		{
			get => _memory.Read32(_address + 28);
			set => _memory.Write32(_address + 28, value);
		}

		public uint dwXCountChars
		{
			get => _memory.Read32(_address + 32);
			set => _memory.Write32(_address + 32, value);
		}

		public uint dwYCountChars
		{
			get => _memory.Read32(_address + 36);
			set => _memory.Write32(_address + 36, value);
		}

		public uint dwFillAttribute
		{
			get => _memory.Read32(_address + 40);
			set => _memory.Write32(_address + 40, value);
		}

		public uint dwFlags
		{
			get => _memory.Read32(_address + 44);
			set => _memory.Write32(_address + 44, value);
		}

		public ushort wShowWindow
		{
			get => _memory.Read16(_address + 48);
			set => _memory.Write16(_address + 48, value);
		}

		public ushort cbReserved2
		{
			get => _memory.Read16(_address + 50);
			set => _memory.Write16(_address + 50, value);
		}

		public uint lpReserved2
		{
			get => _memory.Read32(_address + 52);
			set => _memory.Write32(_address + 52, value);
		}

		public uint hStdInput
		{
			get => _memory.Read32(_address + 56);
			set => _memory.Write32(_address + 56, value);
		}

		public uint hStdOutput
		{
			get => _memory.Read32(_address + 60);
			set => _memory.Write32(_address + 60, value);
		}

		public uint hStdError
		{
			get => _memory.Read32(_address + 64);
			set => _memory.Write32(_address + 64, value);
		}

		/// <summary>
		/// Converts this ref struct to a value struct snapshot.
		/// </summary>
		public NativeTypes.STARTUPINFOA ToStruct()
		{
			return new NativeTypes.STARTUPINFOA
			{
				cb = cb,
				lpReserved = lpReserved,
				lpDesktop = lpDesktop,
				lpTitle = lpTitle,
				dwX = dwX,
				dwY = dwY,
				dwXSize = dwXSize,
				dwYSize = dwYSize,
				dwXCountChars = dwXCountChars,
				dwYCountChars = dwYCountChars,
				dwFillAttribute = dwFillAttribute,
				dwFlags = dwFlags,
				wShowWindow = wShowWindow,
				cbReserved2 = cbReserved2,
				lpReserved2 = lpReserved2,
				hStdInput = hStdInput,
				hStdOutput = hStdOutput,
				hStdError = hStdError
			};
		}

		/// <summary>
		/// Implicit conversion to the underlying value struct.
		/// </summary>
		public static implicit operator NativeTypes.STARTUPINFOA(StartupInfoARef refStruct) => refStruct.ToStruct();
	}
}