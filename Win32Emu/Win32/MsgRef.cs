using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	/// <summary>
	/// Ref struct wrapper for MSG that provides direct memory access via properties.
	/// Properties automatically read from and write to the underlying memory address.
	/// </summary>
	public readonly ref struct MsgRef
	{
		private readonly VirtualMemory _memory;
		private readonly uint _address;

		public MsgRef(VirtualMemory memory, uint address)
		{
			_memory = memory;
			_address = address;
		}

		public uint Address => _address;

		public uint hwnd
		{
			get => _memory.Read32(_address + 0);
			set => _memory.Write32(_address + 0, value);
		}

		public uint message
		{
			get => _memory.Read32(_address + 4);
			set => _memory.Write32(_address + 4, value);
		}

		public uint wParam
		{
			get => _memory.Read32(_address + 8);
			set => _memory.Write32(_address + 8, value);
		}

		public uint lParam
		{
			get => _memory.Read32(_address + 12);
			set => _memory.Write32(_address + 12, value);
		}

		public uint time
		{
			get => _memory.Read32(_address + 16);
			set => _memory.Write32(_address + 16, value);
		}

		public int ptX
		{
			get => (int)_memory.Read32(_address + 20);
			set => _memory.Write32(_address + 20, unchecked((uint)value));
		}

		public int ptY
		{
			get => (int)_memory.Read32(_address + 24);
			set => _memory.Write32(_address + 24, unchecked((uint)value));
		}

		/// <summary>
		/// Converts this ref struct to a value struct snapshot.
		/// </summary>
		public NativeTypes.MSG ToStruct()
		{
			return new NativeTypes.MSG
			{
				hwnd = hwnd,
				message = message,
				wParam = wParam,
				lParam = lParam,
				time = time,
				ptX = ptX,
				ptY = ptY
			};
		}

		/// <summary>
		/// Implicit conversion to the underlying value struct.
		/// </summary>
		public static implicit operator NativeTypes.MSG(MsgRef refStruct) => refStruct.ToStruct();
	}
}