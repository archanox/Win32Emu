using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	/// <summary>
	/// Ref struct wrapper for EXCEPTION_RECORD that provides direct memory access via properties.
	/// Properties automatically read from and write to the underlying memory address.
	/// </summary>
	public readonly ref struct ExceptionRecordRef
	{
		private readonly VirtualMemory _memory;
		private readonly uint _address;

		public ExceptionRecordRef(VirtualMemory memory, uint address)
		{
			_memory = memory;
			_address = address;
		}

		public uint Address => _address;

		public uint ExceptionCode
		{
			get => _memory.Read32(_address + 0);
			set => _memory.Write32(_address + 0, value);
		}

		public uint ExceptionFlags
		{
			get => _memory.Read32(_address + 4);
			set => _memory.Write32(_address + 4, value);
		}

		public uint ExceptionRecord
		{
			get => _memory.Read32(_address + 8);
			set => _memory.Write32(_address + 8, value);
		}

		public uint ExceptionAddress
		{
			get => _memory.Read32(_address + 12);
			set => _memory.Write32(_address + 12, value);
		}

		public uint NumberParameters
		{
			get => _memory.Read32(_address + 16);
			set => _memory.Write32(_address + 16, value);
		}

		/// <summary>
		/// Converts this ref struct to a value struct snapshot.
		/// </summary>
		public NativeTypes.EXCEPTION_RECORD ToStruct()
		{
			return new NativeTypes.EXCEPTION_RECORD
			{
				ExceptionCode = ExceptionCode,
				ExceptionFlags = ExceptionFlags,
				ExceptionRecord = ExceptionRecord,
				ExceptionAddress = ExceptionAddress,
				NumberParameters = NumberParameters
			};
		}

		/// <summary>
		/// Implicit conversion to the underlying value struct.
		/// </summary>
		public static implicit operator NativeTypes.EXCEPTION_RECORD(ExceptionRecordRef refStruct) => refStruct.ToStruct();
	}
}