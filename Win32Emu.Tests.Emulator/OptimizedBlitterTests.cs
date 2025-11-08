using System;
using Xunit;
using Win32Emu.Win32.DirectDraw;

namespace Win32Emu.Tests.Emulator;

public class OptimizedBlitterTests
{
	[Theory]
	[InlineData(1)] // 8-bit
	[InlineData(2)] // 16-bit
	[InlineData(3)] // 24-bit
	[InlineData(4)] // 32-bit
	public void BltFast_CopiesDataCorrectly_ForVariousBitDepths(int bytesPerPixel)
	{
		// Arrange
		const int width = 8;
		const int height = 4;
		var srcPitch = width * bytesPerPixel;
		var destPitch = width * bytesPerPixel;

		var src = new byte[srcPitch * height];
		var dest = new byte[destPitch * height];

		// Fill source with a pattern
		for (var i = 0; i < src.Length; i++)
		{
			src[i] = (byte)(i % 256);
		}

		// Act
		OptimizedBlitter.BltFast(
			dest.AsSpan(),
			src.AsSpan(),
			destPitch,
			srcPitch,
			width,
			height,
			bytesPerPixel);

		// Assert
		Assert.Equal(src, dest);
	}

	[Theory]
	[InlineData(8, 4)]    // Small
	[InlineData(64, 32)]  // Medium
	[InlineData(256, 128)] // Large
	public void BltFast_HandlesVariousSizes(int width, int height)
	{
		// Arrange
		const int bytesPerPixel = 4;
		var srcPitch = width * bytesPerPixel;
		var destPitch = width * bytesPerPixel;

		var src = new byte[srcPitch * height];
		var dest = new byte[destPitch * height];

		// Fill with pattern
		for (var i = 0; i < src.Length; i++)
		{
			src[i] = (byte)((i * 7) % 256);
		}

		// Act
		OptimizedBlitter.BltFast(
			dest.AsSpan(),
			src.AsSpan(),
			destPitch,
			srcPitch,
			width,
			height,
			bytesPerPixel);

		// Assert
		Assert.Equal(src, dest);
	}

	[Fact]
	public void BltFast_HandlesDifferentPitches()
	{
		// Arrange
		const int width = 10;
		const int height = 5;
		const int bytesPerPixel = 4;
		const int srcPitch = 64;  // Wider pitch than needed
		const int destPitch = 48; // Different pitch

		var src = new byte[srcPitch * height];
		var dest = new byte[destPitch * height];

		// Fill source with pattern in used area only
		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				var offset = y * srcPitch + x * bytesPerPixel;
				src[offset] = (byte)((x + y * 10) % 256);
				src[offset + 1] = (byte)((x + y * 10 + 1) % 256);
				src[offset + 2] = (byte)((x + y * 10 + 2) % 256);
				src[offset + 3] = (byte)((x + y * 10 + 3) % 256);
			}
		}

		// Act
		OptimizedBlitter.BltFast(
			dest.AsSpan(),
			src.AsSpan(),
			destPitch,
			srcPitch,
			width,
			height,
			bytesPerPixel);

		// Assert - Check that the copied data matches
		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				var srcOffset = y * srcPitch + x * bytesPerPixel;
				var destOffset = y * destPitch + x * bytesPerPixel;
				
				Assert.Equal(src[srcOffset], dest[destOffset]);
				Assert.Equal(src[srcOffset + 1], dest[destOffset + 1]);
				Assert.Equal(src[srcOffset + 2], dest[destOffset + 2]);
				Assert.Equal(src[srcOffset + 3], dest[destOffset + 3]);
			}
		}
	}

	[Theory]
	[InlineData(1)]  // 8-bit
	[InlineData(2)]  // 16-bit
	[InlineData(4)]  // 32-bit
	public void BltWithSourceColorKey_SkipsTransparentPixels(int bytesPerPixel)
	{
		// Arrange
		const int width = 8;
		const int height = 4;
		var srcPitch = width * bytesPerPixel;
		var destPitch = width * bytesPerPixel;

		var src = new byte[srcPitch * height];
		var dest = new byte[destPitch * height];

		// Fill dest with a known pattern
		for (var i = 0; i < dest.Length; i++)
		{
			dest[i] = 0xFF;
		}

		// Fill source with alternating transparent and opaque pixels
		// For simplicity, use color key value of 0 (transparent)
		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				var offset = y * srcPitch + x * bytesPerPixel;
				if ((x + y) % 2 == 0)
				{
					// Transparent pixel (all zeros)
					for (var b = 0; b < bytesPerPixel; b++)
					{
						src[offset + b] = 0;
					}
				}
				else
				{
					// Opaque pixel
					for (var b = 0; b < bytesPerPixel; b++)
					{
						src[offset + b] = (byte)(42 + b);
					}
				}
			}
		}

		// Act
		uint colorKeyLow = 0;
		uint colorKeyHigh = 0;
		OptimizedBlitter.BltWithSourceColorKey(
			dest.AsSpan(),
			src.AsSpan(),
			destPitch,
			srcPitch,
			width,
			height,
			bytesPerPixel,
			colorKeyLow,
			colorKeyHigh);

		// Assert - Check that transparent pixels weren't copied
		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				var offset = y * destPitch + x * bytesPerPixel;
				if ((x + y) % 2 == 0)
				{
					// Should still be 0xFF (not overwritten)
					Assert.Equal(0xFF, dest[offset]);
				}
				else
				{
					// Should be the source value
					Assert.Equal(42, dest[offset]);
				}
			}
		}
	}

	[Theory]
	[InlineData(1)]  // 8-bit
	[InlineData(2)]  // 16-bit
	[InlineData(4)]  // 32-bit
	public void BltWithSourceColorKey_HandlesColorKeyRange(int bytesPerPixel)
	{
		// Arrange
		const int width = 16;
		const int height = 4;
		var srcPitch = width * bytesPerPixel;
		var destPitch = width * bytesPerPixel;

		var src = new byte[srcPitch * height];
		var dest = new byte[destPitch * height];

		// Fill dest with 0xFF
		for (var i = 0; i < dest.Length; i++)
		{
			dest[i] = 0xFF;
		}

		// Fill source with pixel values 0-15 repeated
		// For multi-byte pixels, all bytes of the pixel have the same value
		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				var offset = y * srcPitch + x * bytesPerPixel;
				var pixelValue = (byte)(x % 16);
				
				// Write the pixel value to all bytes of the pixel
				// This ensures the pixel as a whole has this value
				for (var b = 0; b < bytesPerPixel; b++)
				{
					src[offset + b] = pixelValue;
				}
			}
		}

		// Act - Color key range 5-10 (these should be transparent)
		uint colorKeyLow = bytesPerPixel == 1 ? 5u : 0x05050505u;  // For 16/32-bit, replicate across bytes
		uint colorKeyHigh = bytesPerPixel == 1 ? 10u : 0x0A0A0A0Au;
		
		// For 2-byte pixels, use proper 16-bit values
		if (bytesPerPixel == 2)
		{
			colorKeyLow = 0x0505;
			colorKeyHigh = 0x0A0A;
		}
		
		OptimizedBlitter.BltWithSourceColorKey(
			dest.AsSpan(),
			src.AsSpan(),
			destPitch,
			srcPitch,
			width,
			height,
			bytesPerPixel,
			colorKeyLow,
			colorKeyHigh);

		// Assert
		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				var offset = y * destPitch + x * bytesPerPixel;
				var pixelValue = (byte)(x % 16);
				
				// Check if this pixel value is in the transparent range
				var isTransparent = false;
				if (bytesPerPixel == 1)
				{
					isTransparent = pixelValue >= 5 && pixelValue <= 10;
				}
				else if (bytesPerPixel == 2)
				{
					var pixelVal16 = (ushort)((pixelValue << 8) | pixelValue);
					isTransparent = pixelVal16 >= 0x0505 && pixelVal16 <= 0x0A0A;
				}
				else if (bytesPerPixel == 4)
				{
					var pixelVal32 = (uint)((pixelValue << 24) | (pixelValue << 16) | (pixelValue << 8) | pixelValue);
					isTransparent = pixelVal32 >= 0x05050505u && pixelVal32 <= 0x0A0A0A0Au;
				}
				
				if (isTransparent)
				{
					// Transparent - should not be copied
					Assert.Equal(0xFF, dest[offset]);
				}
				else
				{
					// Opaque - should be copied
					Assert.Equal(pixelValue, dest[offset]);
				}
			}
		}
	}

	[Fact]
	public void BltWithSourceColorKey_AllTransparent_DoesNotModifyDestination()
	{
		// Arrange
		const int width = 8;
		const int height = 4;
		const int bytesPerPixel = 2;
		var srcPitch = width * bytesPerPixel;
		var destPitch = width * bytesPerPixel;

		var src = new byte[srcPitch * height];
		var dest = new byte[destPitch * height];

		// Fill dest with a pattern
		for (var i = 0; i < dest.Length; i++)
		{
			dest[i] = (byte)(i % 256);
		}

		// Keep a copy of original dest
		var originalDest = new byte[destPitch * height];
		Array.Copy(dest, originalDest, dest.Length);

		// Fill source with all transparent values
		// For 16-bit pixels, we need to set both bytes to form the 16-bit value
		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				var offset = y * srcPitch + x * bytesPerPixel;
				src[offset] = 50;      // Low byte
				src[offset + 1] = 0;   // High byte, forming value 50 (0x0032)
			}
		}

		// Act - All pixels should be transparent (value 50 = 0x0032 for 16-bit)
		uint colorKeyLow = 50;
		uint colorKeyHigh = 50;
		OptimizedBlitter.BltWithSourceColorKey(
			dest.AsSpan(),
			src.AsSpan(),
			destPitch,
			srcPitch,
			width,
			height,
			bytesPerPixel,
			colorKeyLow,
			colorKeyHigh);

		// Assert - Destination should be unchanged
		Assert.Equal(originalDest, dest);
	}

	[Fact]
	public void BltWithSourceColorKey_AllOpaque_CopiesEverything()
	{
		// Arrange
		const int width = 8;
		const int height = 4;
		const int bytesPerPixel = 2;
		var srcPitch = width * bytesPerPixel;
		var destPitch = width * bytesPerPixel;

		var src = new byte[srcPitch * height];
		var dest = new byte[destPitch * height];

		// Fill source with pattern (values 100-115)
		for (var i = 0; i < src.Length; i++)
		{
			src[i] = (byte)(100 + (i % 16));
		}

		// Act - No pixels in color key range (50-50)
		uint colorKeyLow = 50;
		uint colorKeyHigh = 50;
		OptimizedBlitter.BltWithSourceColorKey(
			dest.AsSpan(),
			src.AsSpan(),
			destPitch,
			srcPitch,
			width,
			height,
			bytesPerPixel,
			colorKeyLow,
			colorKeyHigh);

		// Assert - All pixels should be copied
		Assert.Equal(src, dest);
	}

	[Fact]
	public void GetSimdCapabilities_ReturnsValidString()
	{
		// Act
		var capabilities = OptimizedBlitter.GetSimdCapabilities();

		// Assert
		Assert.NotNull(capabilities);
		Assert.NotEmpty(capabilities);
		
		// Should contain at least one of these
		var hasCapability = 
			capabilities.Contains("SSE2") ||
			capabilities.Contains("AVX2") ||
			capabilities.Contains("NEON") ||
			capabilities.Contains("Scalar");
		
		Assert.True(hasCapability, $"Capabilities string '{capabilities}' doesn't contain expected SIMD type");
	}

	[Theory]
	[InlineData(1, 8, 4)]   // 8-bit, small
	[InlineData(2, 16, 8)]  // 16-bit, medium
	[InlineData(4, 32, 16)] // 32-bit, larger
	public void BltFast_PerformanceBenchmark_LogsInfo(int bytesPerPixel, int width, int height)
	{
		// This test documents that OptimizedBlitter uses SIMD when available
		// Actual performance testing should be done with BenchmarkDotNet
		
		// Arrange
		var srcPitch = width * bytesPerPixel;
		var destPitch = width * bytesPerPixel;
		var src = new byte[srcPitch * height];
		var dest = new byte[destPitch * height];

		// Fill with pattern
		for (var i = 0; i < src.Length; i++)
		{
			src[i] = (byte)(i % 256);
		}

		// Act
		OptimizedBlitter.BltFast(
			dest.AsSpan(),
			src.AsSpan(),
			destPitch,
			srcPitch,
			width,
			height,
			bytesPerPixel);

		// Assert - Verify correctness
		Assert.Equal(src, dest);
	}

	#region Tests for New cnc-ddraw Inspired Features

	[Theory]
	[InlineData(1)]  // 8-bit
	[InlineData(2)]  // 16-bit
	[InlineData(4)]  // 32-bit
	public void BltStretchWithColorKey_ScalesAndFiltersByColorKey(int bytesPerPixel)
	{
		// Arrange
		const int srcWidth = 4;
		const int srcHeight = 4;
		const int destWidth = 8;
		const int destHeight = 8;
		var srcPitch = srcWidth * bytesPerPixel;
		var destPitch = destWidth * bytesPerPixel;

		var src = new byte[srcPitch * srcHeight];
		var dest = new byte[destPitch * destHeight];

		// Fill dest with 0xFF (so we can detect unchanged pixels)
		for (var i = 0; i < dest.Length; i++)
		{
			dest[i] = 0xFF;
		}

		// Create a simple pattern in source: alternating transparent and opaque
		for (var y = 0; y < srcHeight; y++)
		{
			for (var x = 0; x < srcWidth; x++)
			{
				var offset = y * srcPitch + x * bytesPerPixel;
				var isTransparent = (x + y) % 2 == 0;
				
				for (var b = 0; b < bytesPerPixel; b++)
				{
					src[offset + b] = isTransparent ? (byte)0 : (byte)(42 + b);
				}
			}
		}

		// Act
		OptimizedBlitter.BltStretchWithColorKey(
			dest.AsSpan(),
			src.AsSpan(),
			0, 0, destWidth, destHeight, destPitch,
			0, 0, srcWidth, srcHeight, srcPitch,
			bytesPerPixel,
			0, 0); // Color key 0 = transparent

		// Assert - Verify that some pixels were copied and some were not
		var copiedPixels = 0;
		var unchangedPixels = 0;
		
		for (var y = 0; y < destHeight; y++)
		{
			for (var x = 0; x < destWidth; x++)
			{
				var offset = y * destPitch + x * bytesPerPixel;
				if (dest[offset] == 0xFF)
					unchangedPixels++;
				else if (dest[offset] == 42)
					copiedPixels++;
			}
		}

		// We should have both copied and unchanged pixels due to color keying
		Assert.True(copiedPixels > 0, "Expected some pixels to be copied");
		Assert.True(unchangedPixels > 0, "Expected some pixels to remain unchanged due to color key");
	}

	[Theory]
	[InlineData(true, false)]   // Mirror up-down
	[InlineData(false, true)]   // Mirror left-right
	[InlineData(true, true)]    // Mirror both
	public void BltStretchWithColorKey_SupportsMirroring(bool mirrorUpDown, bool mirrorLeftRight)
	{
		// Arrange
		const int srcWidth = 4;
		const int srcHeight = 4;
		const int destWidth = 4;
		const int destHeight = 4;
		const int bytesPerPixel = 1;
		var srcPitch = srcWidth * bytesPerPixel;
		var destPitch = destWidth * bytesPerPixel;

		var src = new byte[srcPitch * srcHeight];
		var dest = new byte[destPitch * destHeight];

		// Create a unique pattern in source (top-left = 1, increases to the right and down)
		for (var y = 0; y < srcHeight; y++)
		{
			for (var x = 0; x < srcWidth; x++)
			{
				src[y * srcPitch + x] = (byte)(1 + x + y * srcWidth);
			}
		}

		// Act
		OptimizedBlitter.BltStretchWithColorKey(
			dest.AsSpan(),
			src.AsSpan(),
			0, 0, destWidth, destHeight, destPitch,
			0, 0, srcWidth, srcHeight, srcPitch,
			bytesPerPixel,
			255, 255, // No transparent pixels
			mirrorUpDown,
			mirrorLeftRight);

		// Assert - Check that mirroring occurred correctly
		for (var y = 0; y < destHeight; y++)
		{
			for (var x = 0; x < destWidth; x++)
			{
				var srcX = mirrorLeftRight ? (srcWidth - 1 - x) : x;
				var srcY = mirrorUpDown ? (srcHeight - 1 - y) : y;
				var expectedValue = (byte)(1 + srcX + srcY * srcWidth);
				var actualValue = dest[y * destPitch + x];
				
				Assert.Equal(expectedValue, actualValue);
			}
		}
	}

	[Fact]
	public void Clear_FillsBufferWithValue()
	{
		// Arrange
		var buffer = new byte[1024];
		for (var i = 0; i < buffer.Length; i++)
		{
			buffer[i] = 0xFF; // Fill with non-zero value
		}

		// Act
		OptimizedBlitter.Clear(buffer.AsSpan(), 42);

		// Assert
		for (var i = 0; i < buffer.Length; i++)
		{
			Assert.Equal(42, buffer[i]);
		}
	}

	[Theory]
	[InlineData(0)]
	[InlineData(16)]
	[InlineData(64)]
	[InlineData(256)]
	[InlineData(1024)]
	[InlineData(4096)]
	public void Clear_HandlesVariousSizes(int size)
	{
		// Arrange
		var buffer = new byte[size];

		// Act
		OptimizedBlitter.Clear(buffer.AsSpan(), 123);

		// Assert
		for (var i = 0; i < buffer.Length; i++)
		{
			Assert.Equal(123, buffer[i]);
		}
	}

	[Fact]
	public void BltOverlapping_CopiesCorrectly_WhenDestinationIsBelow()
	{
		// Arrange
		const int width = 4;
		const int height = 2;
		const int bytesPerPixel = 1;
		const int pitch = 16; // Wide enough for non-overlapping rows
		var buffer = new byte[pitch * (height + 2)]; // Extra space below

		// Fill source area with pattern
		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				buffer[y * pitch + x] = (byte)(1 + x + y * width);
			}
		}

		// Store expected values before overlap
		var expected = new byte[width * height];
		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				expected[y * width + x] = buffer[y * pitch + x];
			}
		}

		// Act - Copy to destination 1 row below
		OptimizedBlitter.BltOverlapping(
			buffer.AsSpan(),
			0, 1, // destX, destY (1 row down)
			width, height, pitch,
			0, 0, // srcX, srcY
			pitch,
			bytesPerPixel);

		// Assert - Check destination area has correct values
		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				var destValue = buffer[(y + 1) * pitch + x];
				var expectedValue = expected[y * width + x];
				Assert.Equal(expectedValue, destValue);
			}
		}
	}

	[Fact]
	public void BltOverlapping_CopiesCorrectly_WhenNotOverlapping()
	{
		// Arrange
		const int width = 4;
		const int height = 2;
		const int bytesPerPixel = 1;
		const int pitch = 16;
		var buffer = new byte[pitch * height * 2]; // Enough for source and dest

		// Fill source area
		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				buffer[y * pitch + x] = (byte)(1 + x + y * width);
			}
		}

		// Act - Copy to non-overlapping destination (far to the right)
		OptimizedBlitter.BltOverlapping(
			buffer.AsSpan(),
			8, 0, // destX, destY (to the right, no overlap)
			width, height, pitch,
			0, 0, // srcX, srcY
			pitch,
			bytesPerPixel);

		// Assert - Check both source and destination
		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				var expectedValue = (byte)(1 + x + y * width);
				var srcValue = buffer[y * pitch + x];
				var destValue = buffer[y * pitch + 8 + x];
				
				Assert.Equal(expectedValue, srcValue); // Source unchanged
				Assert.Equal(expectedValue, destValue); // Destination correct
			}
		}
	}

	#endregion
}
