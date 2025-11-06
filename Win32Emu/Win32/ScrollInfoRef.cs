using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	/// <summary>
	/// Ref struct wrapper for SCROLLINFO that provides direct memory access via properties.
	/// Properties automatically read from and write to the underlying memory address.
	/// </summary>
	public readonly ref struct ScrollInfoRef
	{
		private readonly VirtualMemory _memory;
		private readonly uint _address;

		public ScrollInfoRef(VirtualMemory memory, uint address)
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

		public uint fMask
		{
			get => _memory.Read32(_address + 4);
			set => _memory.Write32(_address + 4, value);
		}

		public int nMin
		{
			get => (int)_memory.Read32(_address + 8);
			set => _memory.Write32(_address + 8, unchecked((uint)value));
		}

		public int nMax
		{
			get => (int)_memory.Read32(_address + 12);
			set => _memory.Write32(_address + 12, unchecked((uint)value));
		}

		public uint nPage
		{
			get => _memory.Read32(_address + 16);
			set => _memory.Write32(_address + 16, value);
		}

		public int nPos
		{
			get => (int)_memory.Read32(_address + 20);
			set => _memory.Write32(_address + 20, unchecked((uint)value));
		}

		public int nTrackPos
		{
			get => (int)_memory.Read32(_address + 24);
			set => _memory.Write32(_address + 24, unchecked((uint)value));
		}

		/// <summary>
		/// Converts this ref struct to a value struct snapshot.
		/// </summary>
		public NativeTypes.SCROLLINFO ToStruct()
		{
			return new NativeTypes.SCROLLINFO
			{
				cbSize = cbSize,
				fMask = fMask,
				nMin = nMin,
				nMax = nMax,
				nPage = nPage,
				nPos = nPos,
				nTrackPos = nTrackPos
			};
		}

		/// <summary>
		/// Implicit conversion to the underlying value struct.
		/// </summary>
		public static implicit operator NativeTypes.SCROLLINFO(ScrollInfoRef refStruct) => refStruct.ToStruct();
	}
}