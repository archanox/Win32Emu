using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	/// <summary>
	/// Ref struct wrapper for EXCEPTION_POINTERS that provides direct memory access via properties.
	/// Properties automatically read from and write to the underlying memory address.
	/// </summary>
	public readonly ref struct ExceptionPointersRef
	{
		private readonly VirtualMemory _memory;
		private readonly uint _address;

		public ExceptionPointersRef(VirtualMemory memory, uint address)
		{
			_memory = memory;
			_address = address;
		}

		public uint Address => _address;

		public uint ExceptionRecord
		{
			get => _memory.Read32(_address + 0);
			set => _memory.Write32(_address + 0, value);
		}

		public uint ContextRecord
		{
			get => _memory.Read32(_address + 4);
			set => _memory.Write32(_address + 4, value);
		}

		/// <summary>
		/// Converts this ref struct to a value struct snapshot.
		/// </summary>
		public NativeTypes.EXCEPTION_POINTERS ToStruct()
		{
			return new NativeTypes.EXCEPTION_POINTERS
			{
				ExceptionRecord = ExceptionRecord,
				ContextRecord = ContextRecord
			};
		}

		/// <summary>
		/// Implicit conversion to the underlying value struct.
		/// </summary>
		public static implicit operator NativeTypes.EXCEPTION_POINTERS(ExceptionPointersRef refStruct) => refStruct.ToStruct();
	}
}