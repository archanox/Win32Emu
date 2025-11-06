using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	/// <summary>
	/// Ref struct wrapper for POINT that provides direct memory access via properties.
	/// Properties automatically read from and write to the underlying memory address.
	/// </summary>
	public readonly ref struct PointRef
	{
		private readonly VirtualMemory _memory;
		private readonly uint _address;

		public PointRef(VirtualMemory memory, uint address)
		{
			_memory = memory;
			_address = address;
		}

		public uint Address => _address;

		public int x
		{
			get => (int)_memory.Read32(_address + 0);
			set => _memory.Write32(_address + 0, unchecked((uint)value));
		}

		public int y
		{
			get => (int)_memory.Read32(_address + 4);
			set => _memory.Write32(_address + 4, unchecked((uint)value));
		}

		/// <summary>
		/// Converts this ref struct to a value struct snapshot.
		/// </summary>
		public NativeTypes.POINT ToStruct()
		{
			return new NativeTypes.POINT
			{
				x = x,
				y = y
			};
		}

		/// <summary>
		/// Implicit conversion to the underlying value struct.
		/// </summary>
		public static implicit operator NativeTypes.POINT(PointRef refStruct) => refStruct.ToStruct();
	}
}