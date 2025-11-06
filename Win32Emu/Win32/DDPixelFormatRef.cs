using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	/// <summary>
	/// Ref struct wrapper for DDPIXELFORMAT that provides direct memory access via properties.
	/// Properties automatically read from and write to the underlying memory address.
	/// </summary>
	public readonly ref struct DDPixelFormatRef
	{
		private readonly VirtualMemory _memory;
		private readonly uint _address;

		public DDPixelFormatRef(VirtualMemory memory, uint address)
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

		public uint dwFlags
		{
			get => _memory.Read32(_address + 4);
			set => _memory.Write32(_address + 4, value);
		}

		public uint dwFourCC
		{
			get => _memory.Read32(_address + 8);
			set => _memory.Write32(_address + 8, value);
		}

		public uint dwRGBBitCount
		{
			get => _memory.Read32(_address + 12);
			set => _memory.Write32(_address + 12, value);
		}

		public uint dwRBitMask
		{
			get => _memory.Read32(_address + 16);
			set => _memory.Write32(_address + 16, value);
		}

		public uint dwGBitMask
		{
			get => _memory.Read32(_address + 20);
			set => _memory.Write32(_address + 20, value);
		}

		public uint dwBBitMask
		{
			get => _memory.Read32(_address + 24);
			set => _memory.Write32(_address + 24, value);
		}

		public uint dwRGBAlphaBitMask
		{
			get => _memory.Read32(_address + 28);
			set => _memory.Write32(_address + 28, value);
		}

		/// <summary>
		/// Converts this ref struct to a value struct snapshot.
		/// </summary>
		public NativeTypes.DDPIXELFORMAT ToStruct()
		{
			return new NativeTypes.DDPIXELFORMAT
			{
				dwSize = dwSize,
				dwFlags = dwFlags,
				dwFourCC = dwFourCC,
				dwRGBBitCount = dwRGBBitCount,
				dwRBitMask = dwRBitMask,
				dwGBitMask = dwGBitMask,
				dwBBitMask = dwBBitMask,
				dwRGBAlphaBitMask = dwRGBAlphaBitMask
			};
		}

		/// <summary>
		/// Implicit conversion to the underlying value struct.
		/// </summary>
		public static implicit operator NativeTypes.DDPIXELFORMAT(DDPixelFormatRef refStruct) => refStruct.ToStruct();
	}
}