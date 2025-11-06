using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	/// <summary>
	/// Ref struct wrapper for WAVEFORMATEX with automatic memory read/write.
	/// Defines the format of waveform-audio data.
	/// </summary>
	public readonly ref struct WaveFormatExRef
	{
		private readonly VirtualMemory _memory;
		private readonly uint _address;

		public WaveFormatExRef(VirtualMemory memory, uint address)
		{
			_memory = memory;
			_address = address;
		}

		public ushort wFormatTag
		{
			get => _memory.Read16(_address + 0);
			set => _memory.Write16(_address + 0, value);
		}

		public ushort nChannels
		{
			get => _memory.Read16(_address + 2);
			set => _memory.Write16(_address + 2, value);
		}

		public uint nSamplesPerSec
		{
			get => _memory.Read32(_address + 4);
			set => _memory.Write32(_address + 4, value);
		}

		public uint nAvgBytesPerSec
		{
			get => _memory.Read32(_address + 8);
			set => _memory.Write32(_address + 8, value);
		}

		public ushort nBlockAlign
		{
			get => _memory.Read16(_address + 12);
			set => _memory.Write16(_address + 12, value);
		}

		public ushort wBitsPerSample
		{
			get => _memory.Read16(_address + 14);
			set => _memory.Write16(_address + 14, value);
		}

		public ushort cbSize
		{
			get => _memory.Read16(_address + 16);
			set => _memory.Write16(_address + 16, value);
		}

		/// <summary>
		/// Converts this ref struct to a value struct snapshot.
		/// </summary>
		public NativeTypes.WAVEFORMATEX ToStruct()
		{
			return new NativeTypes.WAVEFORMATEX
			{
				wFormatTag = wFormatTag,
				nChannels = nChannels,
				nSamplesPerSec = nSamplesPerSec,
				nAvgBytesPerSec = nAvgBytesPerSec,
				nBlockAlign = nBlockAlign,
				wBitsPerSample = wBitsPerSample,
				cbSize = cbSize
			};
		}

		/// <summary>
		/// Implicit conversion to the underlying value struct.
		/// </summary>
		public static implicit operator NativeTypes.WAVEFORMATEX(WaveFormatExRef refStruct) => refStruct.ToStruct();
	}
}