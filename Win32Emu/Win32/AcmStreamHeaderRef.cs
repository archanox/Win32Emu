using Win32Emu.Memory;

namespace Win32Emu.Win32
{
	/// <summary>
	/// Ref struct wrapper for ACMSTREAMHEADER with automatic memory read/write.
	/// Used for ACM audio conversion stream headers.
	/// </summary>
	public readonly ref struct AcmStreamHeaderRef
	{
		private readonly VirtualMemory _memory;
		private readonly uint _address;

		public AcmStreamHeaderRef(VirtualMemory memory, uint address)
		{
			_memory = memory;
			_address = address;
		}

		public uint cbStruct
		{
			get => _memory.Read32(_address + 0);
			set => _memory.Write32(_address + 0, value);
		}

		public uint fdwStatus
		{
			get => _memory.Read32(_address + 4);
			set => _memory.Write32(_address + 4, value);
		}

		public uint dwUser
		{
			get => _memory.Read32(_address + 8);
			set => _memory.Write32(_address + 8, value);
		}

		public uint pbSrc
		{
			get => _memory.Read32(_address + 12);
			set => _memory.Write32(_address + 12, value);
		}

		public uint cbSrcLength
		{
			get => _memory.Read32(_address + 16);
			set => _memory.Write32(_address + 16, value);
		}

		public uint cbSrcLengthUsed
		{
			get => _memory.Read32(_address + 20);
			set => _memory.Write32(_address + 20, value);
		}

		public uint dwSrcUser
		{
			get => _memory.Read32(_address + 24);
			set => _memory.Write32(_address + 24, value);
		}

		public uint pbDst
		{
			get => _memory.Read32(_address + 28);
			set => _memory.Write32(_address + 28, value);
		}

		public uint cbDstLength
		{
			get => _memory.Read32(_address + 32);
			set => _memory.Write32(_address + 32, value);
		}

		public uint cbDstLengthUsed
		{
			get => _memory.Read32(_address + 36);
			set => _memory.Write32(_address + 36, value);
		}

		public uint dwDstUser
		{
			get => _memory.Read32(_address + 40);
			set => _memory.Write32(_address + 40, value);
		}

		/// <summary>
		/// Converts this ref struct to a value struct snapshot.
		/// </summary>
		public NativeTypes.ACMSTREAMHEADER ToStruct()
		{
			return new NativeTypes.ACMSTREAMHEADER
			{
				cbStruct = cbStruct,
				fdwStatus = fdwStatus,
				dwUser = dwUser,
				pbSrc = pbSrc,
				cbSrcLength = cbSrcLength,
				cbSrcLengthUsed = cbSrcLengthUsed,
				dwSrcUser = dwSrcUser,
				pbDst = pbDst,
				cbDstLength = cbDstLength,
				cbDstLengthUsed = cbDstLengthUsed,
				dwDstUser = dwDstUser
			};
		}

		/// <summary>
		/// Implicit conversion to the underlying value struct.
		/// </summary>
		public static implicit operator NativeTypes.ACMSTREAMHEADER(AcmStreamHeaderRef refStruct) => refStruct.ToStruct();
	}
}