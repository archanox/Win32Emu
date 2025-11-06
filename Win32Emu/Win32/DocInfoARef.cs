using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	/// <summary>
	/// Ref struct wrapper for DOCINFOA that provides direct memory access via properties.
	/// Properties automatically read from and write to the underlying memory address.
	/// </summary>
	public readonly ref struct DocInfoARef
	{
		private readonly VirtualMemory _memory;
		private readonly uint _address;

		public DocInfoARef(VirtualMemory memory, uint address)
		{
			_memory = memory;
			_address = address;
		}

		public uint Address => _address;

		public int cbSize
		{
			get => (int)_memory.Read32(_address + 0);
			set => _memory.Write32(_address + 0, (uint)value);
		}

		public uint lpszDocName
		{
			get => _memory.Read32(_address + 4);
			set => _memory.Write32(_address + 4, value);
		}

		public uint lpszOutput
		{
			get => _memory.Read32(_address + 8);
			set => _memory.Write32(_address + 8, value);
		}

		public uint lpszDatatype
		{
			get => _memory.Read32(_address + 12);
			set => _memory.Write32(_address + 12, value);
		}

		public uint fwType
		{
			get => _memory.Read32(_address + 16);
			set => _memory.Write32(_address + 16, value);
		}

		/// <summary>
		/// Converts this ref struct to a value struct snapshot.
		/// </summary>
		public NativeTypes.DOCINFOA ToStruct()
		{
			return new NativeTypes.DOCINFOA
			{
				cbSize = cbSize,
				lpszDocName = lpszDocName,
				lpszOutput = lpszOutput,
				lpszDatatype = lpszDatatype,
				fwType = fwType
			};
		}

		/// <summary>
		/// Implicit conversion to the underlying value struct.
		/// </summary>
		public static implicit operator NativeTypes.DOCINFOA(DocInfoARef refStruct) => refStruct.ToStruct();
	}
}