using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	/// <summary>
	/// Ref struct wrapper for RECT that provides direct memory access via properties.
	/// Properties automatically read from and write to the underlying memory address.
	/// </summary>
	public readonly ref struct RectRef
	{
		private readonly VirtualMemory _memory;
		private readonly uint _address;

		public RectRef(VirtualMemory memory, uint address)
		{
			_memory = memory;
			_address = address;
		}

		public uint Address => _address;

		public int left
		{
			get => (int)_memory.Read32(_address + 0);
			set => _memory.Write32(_address + 0, unchecked((uint)value));
		}

		public int top
		{
			get => (int)_memory.Read32(_address + 4);
			set => _memory.Write32(_address + 4, unchecked((uint)value));
		}

		public int right
		{
			get => (int)_memory.Read32(_address + 8);
			set => _memory.Write32(_address + 8, unchecked((uint)value));
		}

		public int bottom
		{
			get => (int)_memory.Read32(_address + 12);
			set => _memory.Write32(_address + 12, unchecked((uint)value));
		}

		/// <summary>
		/// Converts this ref struct to a value struct snapshot.
		/// </summary>
		public NativeTypes.RECT ToStruct()
		{
			return new NativeTypes.RECT
			{
				left = left,
				top = top,
				right = right,
				bottom = bottom
			};
		}

		/// <summary>
		/// Implicit conversion to the underlying value struct.
		/// </summary>
		public static implicit operator NativeTypes.RECT(RectRef refStruct) => refStruct.ToStruct();
	}
}