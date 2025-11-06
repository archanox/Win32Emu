using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	/// <summary>
	/// Ref struct wrapper for PAINTSTRUCT that provides direct memory access via properties.
	/// Properties automatically read from and write to the underlying memory address.
	/// Note: rcPaint is represented as a RectRef at offset 8.
	/// </summary>
	public readonly ref struct PaintStructRef
	{
		private readonly VirtualMemory _memory;
		private readonly uint _address;

		public PaintStructRef(VirtualMemory memory, uint address)
		{
			_memory = memory;
			_address = address;
		}

		public uint Address => _address;

		public uint hdc
		{
			get => _memory.Read32(_address + 0);
			set => _memory.Write32(_address + 0, value);
		}

		public uint fErase
		{
			get => _memory.Read32(_address + 4);
			set => _memory.Write32(_address + 4, value);
		}

		/// <summary>
		/// Gets a RectRef for the rcPaint field (at offset 8).
		/// </summary>
		public RectRef rcPaint => new RectRef(_memory, _address + 8);

		public uint fRestore
		{
			get => _memory.Read32(_address + 24);
			set => _memory.Write32(_address + 24, value);
		}

		public uint fIncUpdate
		{
			get => _memory.Read32(_address + 28);
			set => _memory.Write32(_address + 28, value);
		}

		/// <summary>
		/// Converts this ref struct to a value struct snapshot.
		/// </summary>
		public NativeTypes.PAINTSTRUCT ToStruct()
		{
			var rect = rcPaint;
			return new NativeTypes.PAINTSTRUCT
			{
				hdc = hdc,
				fErase = fErase,
				rcPaintLeft = rect.left,
				rcPaintTop = rect.top,
				rcPaintRight = rect.right,
				rcPaintBottom = rect.bottom,
				fRestore = fRestore,
				fIncUpdate = fIncUpdate
			};
		}

		/// <summary>
		/// Implicit conversion to the underlying value struct.
		/// </summary>
		public static implicit operator NativeTypes.PAINTSTRUCT(PaintStructRef refStruct) => refStruct.ToStruct();
	}
}