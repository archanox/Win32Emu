using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	/// <summary>
	/// Ref struct wrapper for WNDCLASSEXA that provides direct memory access via properties.
	/// Properties automatically read from and write to the underlying memory address.
	/// </summary>
	public readonly ref struct WndClassExARef
	{
		private readonly VirtualMemory _memory;
		private readonly uint _address;

		public WndClassExARef(VirtualMemory memory, uint address)
		{
			_memory = memory;
			_address = address;
		}

		public uint Address => _address;

		public uint cbSize
		{
			get => _memory.Read32(_address + 0);
			set => _memory.Write32(_address + 0, value);
		}

		public uint style
		{
			get => _memory.Read32(_address + 4);
			set => _memory.Write32(_address + 4, value);
		}

		public uint lpfnWndProc
		{
			get => _memory.Read32(_address + 8);
			set => _memory.Write32(_address + 8, value);
		}

		public int cbClsExtra
		{
			get => (int)_memory.Read32(_address + 12);
			set => _memory.Write32(_address + 12, (uint)value);
		}

		public int cbWndExtra
		{
			get => (int)_memory.Read32(_address + 16);
			set => _memory.Write32(_address + 16, (uint)value);
		}

		public uint hInstance
		{
			get => _memory.Read32(_address + 20);
			set => _memory.Write32(_address + 20, value);
		}

		public uint hIcon
		{
			get => _memory.Read32(_address + 24);
			set => _memory.Write32(_address + 24, value);
		}

		public uint hCursor
		{
			get => _memory.Read32(_address + 28);
			set => _memory.Write32(_address + 28, value);
		}

		public uint hbrBackground
		{
			get => _memory.Read32(_address + 32);
			set => _memory.Write32(_address + 32, value);
		}

		public uint lpszMenuName
		{
			get => _memory.Read32(_address + 36);
			set => _memory.Write32(_address + 36, value);
		}

		public uint lpszClassName
		{
			get => _memory.Read32(_address + 40);
			set => _memory.Write32(_address + 40, value);
		}

		public uint hIconSm
		{
			get => _memory.Read32(_address + 44);
			set => _memory.Write32(_address + 44, value);
		}

		/// <summary>
		/// Converts this ref struct to a value struct snapshot.
		/// </summary>
		public NativeTypes.WNDCLASSEXA ToStruct()
		{
			return new NativeTypes.WNDCLASSEXA
			{
				cbSize = cbSize,
				style = style,
				lpfnWndProc = lpfnWndProc,
				cbClsExtra = cbClsExtra,
				cbWndExtra = cbWndExtra,
				hInstance = hInstance,
				hIcon = hIcon,
				hCursor = hCursor,
				hbrBackground = hbrBackground,
				lpszMenuName = lpszMenuName,
				lpszClassName = lpszClassName,
				hIconSm = hIconSm
			};
		}

		/// <summary>
		/// Implicit conversion to the underlying value struct.
		/// </summary>
		public static implicit operator NativeTypes.WNDCLASSEXA(WndClassExARef refStruct) => refStruct.ToStruct();
	}
}