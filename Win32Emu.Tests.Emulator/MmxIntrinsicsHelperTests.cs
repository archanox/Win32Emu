using Win32Emu.Cpu;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for MMX intrinsics helper functions
/// </summary>
public class MmxIntrinsicsHelperTests
{
	[Fact]
	public void AddPackedBytes_ShouldAddEightBytes()
	{
		// Arrange
		ulong a = 0x0102030405060708; // 8, 7, 6, 5, 4, 3, 2, 1 (little-endian)
		ulong b = 0x0807060504030201; // 1, 2, 3, 4, 5, 6, 7, 8

		// Act
		ulong result = MmxIntrinsicsHelper.AddPackedBytes(a, b);

		// Assert - each byte should be 9
		for (int i = 0; i < 8; i++)
		{
			byte value = (byte)((result >> (i * 8)) & 0xFF);
			Assert.Equal(9, value);
		}
	}

	[Fact]
	public void AddPackedWords_ShouldAddFourWords()
	{
		// Arrange
		ulong a = 0x0001000200030004; // 4, 3, 2, 1 (little-endian words)
		ulong b = 0x0004000300020001; // 1, 2, 3, 4

		// Act
		ulong result = MmxIntrinsicsHelper.AddPackedWords(a, b);

		// Assert - each word should be 5
		for (int i = 0; i < 4; i++)
		{
			ushort value = (ushort)((result >> (i * 16)) & 0xFFFF);
			Assert.Equal(5, value);
		}
	}

	[Fact]
	public void AddPackedDwords_ShouldAddTwoDwords()
	{
		// Arrange
		ulong a = 0x0000000200000001; // 1, 2 (little-endian dwords)
		ulong b = 0x0000000400000003; // 3, 4

		// Act
		ulong result = MmxIntrinsicsHelper.AddPackedDwords(a, b);

		// Assert
		uint lo = (uint)(result & 0xFFFFFFFF);
		uint hi = (uint)((result >> 32) & 0xFFFFFFFF);
		Assert.Equal(4u, lo);
		Assert.Equal(6u, hi);
	}

	[Fact]
	public void SubtractPackedBytes_ShouldSubtractEightBytes()
	{
		// Arrange
		ulong a = 0x0F0E0D0C0B0A0908; // 8, 9, 10, 11, 12, 13, 14, 15
		ulong b = 0x0102030405060708; // 8, 7, 6, 5, 4, 3, 2, 1

		// Act
		ulong result = MmxIntrinsicsHelper.SubtractPackedBytes(a, b);

		// Assert
		Assert.Equal((byte)0, (byte)((result >> 0) & 0xFF));  // 8 - 8 = 0
		Assert.Equal((byte)2, (byte)((result >> 8) & 0xFF));  // 9 - 7 = 2
		Assert.Equal((byte)4, (byte)((result >> 16) & 0xFF)); // 10 - 6 = 4
		Assert.Equal((byte)6, (byte)((result >> 24) & 0xFF)); // 11 - 5 = 6
	}

	[Fact]
	public void AddPackedSignedBytesWithSaturation_ShouldSaturateOnOverflow()
	{
		// Arrange - values that will overflow
		ulong a = 0x7F7F7F7F7F7F7F7F; // All bytes are 127 (max signed byte)
		ulong b = 0x0101010101010101; // All bytes are 1

		// Act
		ulong result = MmxIntrinsicsHelper.AddPackedSignedBytesWithSaturation(a, b);

		// Assert - should saturate to 127 (0x7F)
		for (int i = 0; i < 8; i++)
		{
			byte value = (byte)((result >> (i * 8)) & 0xFF);
			Assert.Equal(0x7F, value);
		}
	}

	[Fact]
	public void AddPackedUnsignedBytesWithSaturation_ShouldSaturateOnOverflow()
	{
		// Arrange - values that will overflow
		ulong a = 0xFFFFFFFFFFFFFFFF; // All bytes are 255 (max unsigned byte)
		ulong b = 0x0101010101010101; // All bytes are 1

		// Act
		ulong result = MmxIntrinsicsHelper.AddPackedUnsignedBytesWithSaturation(a, b);

		// Assert - should saturate to 255 (0xFF)
		for (int i = 0; i < 8; i++)
		{
			byte value = (byte)((result >> (i * 8)) & 0xFF);
			Assert.Equal(0xFF, value);
		}
	}

	[Fact]
	public void SubtractPackedUnsignedBytesWithSaturation_ShouldSaturateOnUnderflow()
	{
		// Arrange - values that will underflow
		ulong a = 0x0101010101010101; // All bytes are 1
		ulong b = 0x0202020202020202; // All bytes are 2

		// Act
		ulong result = MmxIntrinsicsHelper.SubtractPackedUnsignedBytesWithSaturation(a, b);

		// Assert - should saturate to 0
		for (int i = 0; i < 8; i++)
		{
			byte value = (byte)((result >> (i * 8)) & 0xFF);
			Assert.Equal(0, value);
		}
	}

	[Fact]
	public void And_ShouldPerformBitwiseAnd()
	{
		// Arrange
		ulong a = 0xFF00FF00FF00FF00;
		ulong b = 0xF0F0F0F0F0F0F0F0;

		// Act
		ulong result = MmxIntrinsicsHelper.And(a, b);

		// Assert
		Assert.Equal(0xF000F000F000F000UL, result);
	}

	[Fact]
	public void Or_ShouldPerformBitwiseOr()
	{
		// Arrange
		ulong a = 0xFF00FF00FF00FF00;
		ulong b = 0xF0F0F0F0F0F0F0F0;

		// Act
		ulong result = MmxIntrinsicsHelper.Or(a, b);

		// Assert
		Assert.Equal(0xFFF0FFF0FFF0FFF0UL, result);
	}

	[Fact]
	public void Xor_ShouldPerformBitwiseXor()
	{
		// Arrange
		ulong a = 0xFF00FF00FF00FF00;
		ulong b = 0xF0F0F0F0F0F0F0F0;

		// Act
		ulong result = MmxIntrinsicsHelper.Xor(a, b);

		// Assert
		Assert.Equal(0x0FF00FF00FF00FF0UL, result);
	}

	[Fact]
	public void AndNot_ShouldPerformBitwiseAndNot()
	{
		// Arrange
		ulong a = 0xFF00FF00FF00FF00;
		ulong b = 0xF0F0F0F0F0F0F0F0;

		// Act
		ulong result = MmxIntrinsicsHelper.AndNot(a, b);

		// Assert - (~a) & b
		Assert.Equal(0x00F000F000F000F0UL, result);
	}
}
