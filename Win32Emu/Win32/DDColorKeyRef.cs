using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	/// <summary>
	/// Ref struct wrapper for DDCOLORKEY with automatic memory read/write.
	/// Specifies a color key for DirectDraw surfaces.
	/// </summary>
	public readonly ref struct DDColorKeyRef
	{
		private readonly VirtualMemory _memory;
		private readonly uint _address;

		public DDColorKeyRef(VirtualMemory memory, uint address)
		{
			_memory = memory;
			_address = address;
		}

		public uint dwColorSpaceLowValue
		{
			get => _memory.Read32(_address + 0);
			set => _memory.Write32(_address + 0, value);
		}

		public uint dwColorSpaceHighValue
		{
			get => _memory.Read32(_address + 4);
			set => _memory.Write32(_address + 4, value);
		}

		/// <summary>
		/// Converts this ref struct to a value struct snapshot.
		/// </summary>
		public NativeTypes.DDCOLORKEY ToStruct()
		{
			return new NativeTypes.DDCOLORKEY
			{
				dwColorSpaceLowValue = dwColorSpaceLowValue,
				dwColorSpaceHighValue = dwColorSpaceHighValue
			};
		}

		/// <summary>
		/// Implicit conversion to the underlying value struct.
		/// </summary>
		public static implicit operator NativeTypes.DDCOLORKEY(DDColorKeyRef refStruct) => refStruct.ToStruct();
	}
}