using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	/// <summary>
	/// Ref struct wrapper for DIDATAFORMAT that provides direct memory access via properties.
	/// Properties automatically read from and write to the underlying memory address.
	/// </summary>
	public readonly ref struct DiDataFormatRef
	{
		private readonly VirtualMemory _memory;
		private readonly uint _address;

		public DiDataFormatRef(VirtualMemory memory, uint address)
		{
			_memory = memory;
			_address = address;
		}

		public uint Address => _address;

		public uint dwSize
		{
			get => _memory.Read32(_address + 0);
			set => _memory.Write32(_address + 0, value);
		}

		public uint dwObjSize
		{
			get => _memory.Read32(_address + 4);
			set => _memory.Write32(_address + 4, value);
		}

		public uint dwFlags
		{
			get => _memory.Read32(_address + 8);
			set => _memory.Write32(_address + 8, value);
		}

		public uint dwDataSize
		{
			get => _memory.Read32(_address + 12);
			set => _memory.Write32(_address + 12, value);
		}

		public uint dwNumObjs
		{
			get => _memory.Read32(_address + 16);
			set => _memory.Write32(_address + 16, value);
		}

		public uint rgodf
		{
			get => _memory.Read32(_address + 20);
			set => _memory.Write32(_address + 20, value);
		}

		/// <summary>
		/// Converts this ref struct to a value struct snapshot.
		/// </summary>
		public NativeTypes.DIDATAFORMAT ToStruct()
		{
			return new NativeTypes.DIDATAFORMAT
			{
				dwSize = dwSize,
				dwObjSize = dwObjSize,
				dwFlags = dwFlags,
				dwDataSize = dwDataSize,
				dwNumObjs = dwNumObjs,
				rgodf = rgodf
			};
		}

		/// <summary>
		/// Implicit conversion to the underlying value struct.
		/// </summary>
		public static implicit operator NativeTypes.DIDATAFORMAT(DiDataFormatRef refStruct) => refStruct.ToStruct();
	}
}