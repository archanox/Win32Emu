using Win32Emu.Cpu;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for SIMD intrinsics helper functions
/// </summary>
public class SimdIntrinsicsHelperTests
{
    [Fact]
    public void AddPackedSingle_ShouldAddFourFloats()
    {
        // Arrange
        var a = new byte[16];
        var b = new byte[16];
        
        // Set a = [1.0f, 2.0f, 3.0f, 4.0f]
        BitConverter.GetBytes(1.0f).CopyTo(a, 0);
        BitConverter.GetBytes(2.0f).CopyTo(a, 4);
        BitConverter.GetBytes(3.0f).CopyTo(a, 8);
        BitConverter.GetBytes(4.0f).CopyTo(a, 12);
        
        // Set b = [5.0f, 6.0f, 7.0f, 8.0f]
        BitConverter.GetBytes(5.0f).CopyTo(b, 0);
        BitConverter.GetBytes(6.0f).CopyTo(b, 4);
        BitConverter.GetBytes(7.0f).CopyTo(b, 8);
        BitConverter.GetBytes(8.0f).CopyTo(b, 12);

        // Act
        var result = SimdIntrinsicsHelper.AddPackedSingle(a, b);

        // Assert - result should be [6.0f, 8.0f, 10.0f, 12.0f]
        Assert.Equal(6.0f, BitConverter.ToSingle(result, 0), precision: 5);
        Assert.Equal(8.0f, BitConverter.ToSingle(result, 4), precision: 5);
        Assert.Equal(10.0f, BitConverter.ToSingle(result, 8), precision: 5);
        Assert.Equal(12.0f, BitConverter.ToSingle(result, 12), precision: 5);
    }

    [Fact]
    public void MultiplyPackedSingle_ShouldMultiplyFourFloats()
    {
        // Arrange
        var a = new byte[16];
        var b = new byte[16];
        
        // Set a = [2.0f, 3.0f, 4.0f, 5.0f]
        BitConverter.GetBytes(2.0f).CopyTo(a, 0);
        BitConverter.GetBytes(3.0f).CopyTo(a, 4);
        BitConverter.GetBytes(4.0f).CopyTo(a, 8);
        BitConverter.GetBytes(5.0f).CopyTo(a, 12);
        
        // Set b = [1.5f, 2.0f, 2.5f, 3.0f]
        BitConverter.GetBytes(1.5f).CopyTo(b, 0);
        BitConverter.GetBytes(2.0f).CopyTo(b, 4);
        BitConverter.GetBytes(2.5f).CopyTo(b, 8);
        BitConverter.GetBytes(3.0f).CopyTo(b, 12);

        // Act
        var result = SimdIntrinsicsHelper.MultiplyPackedSingle(a, b);

        // Assert - result should be [3.0f, 6.0f, 10.0f, 15.0f]
        Assert.Equal(3.0f, BitConverter.ToSingle(result, 0), precision: 5);
        Assert.Equal(6.0f, BitConverter.ToSingle(result, 4), precision: 5);
        Assert.Equal(10.0f, BitConverter.ToSingle(result, 8), precision: 5);
        Assert.Equal(15.0f, BitConverter.ToSingle(result, 12), precision: 5);
    }

    [Fact]
    public void AddPackedDouble_ShouldAddTwoDoubles()
    {
        // Arrange
        var a = new byte[16];
        var b = new byte[16];
        
        // Set a = [1.5, 2.5]
        BitConverter.GetBytes(1.5).CopyTo(a, 0);
        BitConverter.GetBytes(2.5).CopyTo(a, 8);
        
        // Set b = [3.5, 4.5]
        BitConverter.GetBytes(3.5).CopyTo(b, 0);
        BitConverter.GetBytes(4.5).CopyTo(b, 8);

        // Act
        var result = SimdIntrinsicsHelper.AddPackedDouble(a, b);

        // Assert - result should be [5.0, 7.0]
        Assert.Equal(5.0, BitConverter.ToDouble(result, 0), precision: 10);
        Assert.Equal(7.0, BitConverter.ToDouble(result, 8), precision: 10);
    }

    [Fact]
    public void AddPackedBytes_ShouldAddSixteenBytes()
    {
        // Arrange
        var a = new byte[16];
        var b = new byte[16];
        
        for (var i = 0; i < 16; i++)
        {
            a[i] = (byte)(i + 1);      // [1, 2, 3, ..., 16]
            b[i] = (byte)(16 - i);     // [16, 15, 14, ..., 1]
        }

        // Act
        var result = SimdIntrinsicsHelper.AddPackedBytes(a, b);

        // Assert - each result should be 17
        for (var i = 0; i < 16; i++)
        {
            Assert.Equal(17, result[i]);
        }
    }

    [Fact]
    public void AddPackedBytes_WithOverflow_ShouldWrap()
    {
        // Arrange
        var a = new byte[] { 250, 251, 252, 253, 254, 255, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var b = new byte[] { 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 };

        // Act
        var result = SimdIntrinsicsHelper.AddPackedBytes(a, b);

        // Assert - should wrap around (unsigned byte arithmetic)
        Assert.Equal((byte)4, result[0]);   // 250 + 10 = 260 -> 4
        Assert.Equal((byte)5, result[1]);   // 251 + 10 = 261 -> 5
        Assert.Equal((byte)6, result[2]);   // 252 + 10 = 262 -> 6
        Assert.Equal((byte)7, result[3]);   // 253 + 10 = 263 -> 7
        Assert.Equal((byte)8, result[4]);   // 254 + 10 = 264 -> 8
        Assert.Equal((byte)9, result[5]);   // 255 + 10 = 265 -> 9
    }

    [Fact]
    public void PopCount_ShouldCountSetBits()
    {
        // Arrange & Act & Assert
        Assert.Equal(0u, SimdIntrinsicsHelper.PopCount(0x00000000));
        Assert.Equal(1u, SimdIntrinsicsHelper.PopCount(0x00000001));
        Assert.Equal(8u, SimdIntrinsicsHelper.PopCount(0x000000FF));
        Assert.Equal(16u, SimdIntrinsicsHelper.PopCount(0x0000FFFF));
        Assert.Equal(32u, SimdIntrinsicsHelper.PopCount(0xFFFFFFFF));
        Assert.Equal(4u, SimdIntrinsicsHelper.PopCount(0x0F000000));
        Assert.Equal(3u, SimdIntrinsicsHelper.PopCount(0x00000007));
    }

    [Fact]
    public void LeadingZeroCount_ShouldCountLeadingZeros()
    {
        // Arrange & Act & Assert
        Assert.Equal(32u, SimdIntrinsicsHelper.LeadingZeroCount(0x00000000));
        Assert.Equal(31u, SimdIntrinsicsHelper.LeadingZeroCount(0x00000001));
        Assert.Equal(24u, SimdIntrinsicsHelper.LeadingZeroCount(0x000000FF));
        Assert.Equal(16u, SimdIntrinsicsHelper.LeadingZeroCount(0x0000FFFF));
        Assert.Equal(0u, SimdIntrinsicsHelper.LeadingZeroCount(0xFFFFFFFF));
        Assert.Equal(0u, SimdIntrinsicsHelper.LeadingZeroCount(0x80000000));
        Assert.Equal(1u, SimdIntrinsicsHelper.LeadingZeroCount(0x40000000));
    }

    [Fact]
    public void Crc32_ShouldCalculateCrc()
    {
        // Arrange
        var data = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // "Hello"

        // Act - Calculate CRC32 of "Hello"
        uint crc = 0;
        foreach (var b in data)
        {
            crc = SimdIntrinsicsHelper.Crc32C(crc, b);
        }

        // Assert - CRC32 should be consistent across runs
        // We just verify it produces a non-zero result and is deterministic
        Assert.NotEqual(0u, crc);
        
        // Calculate again to verify determinism
        uint crc2 = 0;
        foreach (var b in data)
        {
            crc2 = SimdIntrinsicsHelper.Crc32C(crc2, b);
        }
        Assert.Equal(crc, crc2);
    }
}
