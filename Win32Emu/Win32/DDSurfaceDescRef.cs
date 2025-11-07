using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	/// <summary>
	/// Ref struct wrapper for DDSURFACEDESC that provides direct memory access via properties.
	/// Properties automatically read from and write to the underlying memory address.
	/// </summary>
	public readonly ref struct DDSurfaceDescRef
	{
		private readonly VirtualMemory _memory;
		private readonly uint _address;

		public DDSurfaceDescRef(VirtualMemory memory, uint address)
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

		public NativeTypes.DDSD dwFlags
		{
			get => (NativeTypes.DDSD)_memory.Read32(_address + 4);
			set => _memory.Write32(_address + 4, (uint)value);
		}

		public uint dwHeight
		{
			get => _memory.Read32(_address + 8);
			set => _memory.Write32(_address + 8, value);
		}

		public uint dwWidth
		{
			get => _memory.Read32(_address + 12);
			set => _memory.Write32(_address + 12, value);
		}

		public uint lPitch
		{
			get => _memory.Read32(_address + 16);
			set => _memory.Write32(_address + 16, value);
		}

		public uint dwBackBufferCount
		{
			get => _memory.Read32(_address + 20);
			set => _memory.Write32(_address + 20, value);
		}

		public uint dwSurfaceCaps
		{
			get => _memory.Read32(_address + 104);
			set => _memory.Write32(_address + 104, value);
		}

		/// <summary>
		/// Converts this ref struct to a value struct snapshot.
		/// </summary>
		public NativeTypes.DDSURFACEDESC ToStruct()
		{
			return new NativeTypes.DDSURFACEDESC
			{
				dwSize = dwSize,
				dwFlags = dwFlags,
				dwWidth = dwWidth,
				dwHeight = dwHeight,
				lPitch = lPitch,
				dwBackBufferCount = dwBackBufferCount
			};
		}

		/// <summary>
		/// Implicit conversion to the underlying value struct.
		/// </summary>
		public static implicit operator NativeTypes.DDSURFACEDESC(DDSurfaceDescRef refStruct) => refStruct.ToStruct();
	}
}