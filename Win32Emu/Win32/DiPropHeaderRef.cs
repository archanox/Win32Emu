using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	/// <summary>
	/// Ref struct wrapper for DIPROPHEADER that provides direct memory access via properties.
	/// Properties automatically read from and write to the underlying memory address.
	/// </summary>
	public readonly ref struct DiPropHeaderRef
	{
		private readonly VirtualMemory _memory;
		private readonly uint _address;

		public DiPropHeaderRef(VirtualMemory memory, uint address)
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

		public uint dwHeaderSize
		{
			get => _memory.Read32(_address + 4);
			set => _memory.Write32(_address + 4, value);
		}

		public uint dwObj
		{
			get => _memory.Read32(_address + 8);
			set => _memory.Write32(_address + 8, value);
		}

		public uint dwHow
		{
			get => _memory.Read32(_address + 12);
			set => _memory.Write32(_address + 12, value);
		}

		/// <summary>
		/// Converts this ref struct to a value struct snapshot.
		/// </summary>
		public NativeTypes.DIPROPHEADER ToStruct()
		{
			return new NativeTypes.DIPROPHEADER
			{
				dwSize = dwSize,
				dwHeaderSize = dwHeaderSize,
				dwObj = dwObj,
				dwHow = dwHow
			};
		}

		/// <summary>
		/// Implicit conversion to the underlying value struct.
		/// </summary>
		public static implicit operator NativeTypes.DIPROPHEADER(DiPropHeaderRef refStruct) => refStruct.ToStruct();
	}
}