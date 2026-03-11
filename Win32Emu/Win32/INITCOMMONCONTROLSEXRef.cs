using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	/// <summary>
	/// Ref struct wrapper for INITCOMMONCONTROLSEX that provides direct memory access via properties.
	/// </summary>
	public readonly ref struct INITCOMMONCONTROLSEXRef
	{
		private readonly VirtualMemory _memory;
		private readonly uint _address;

		public INITCOMMONCONTROLSEXRef(VirtualMemory memory, uint address)
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

		public uint dwICC
		{
			get => _memory.Read32(_address + 4);
			set => _memory.Write32(_address + 4, value);
		}

		public NativeTypes.INITCOMMONCONTROLSEX ToStruct()
		{
			return new NativeTypes.INITCOMMONCONTROLSEX
			{
				dwSize = dwSize,
				dwICC = dwICC
			};
		}

		public static implicit operator NativeTypes.INITCOMMONCONTROLSEX(INITCOMMONCONTROLSEXRef refStruct) => refStruct.ToStruct();
	}
}
