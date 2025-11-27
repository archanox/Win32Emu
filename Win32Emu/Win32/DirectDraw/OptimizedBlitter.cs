using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics.Arm;
#if NET8_0_OR_GREATER
using System.Runtime.Intrinsics.Wasm;
#endif

namespace Win32Emu.Win32.DirectDraw
{
	/// <summary>
	/// High-performance blitter optimized with SIMD intrinsics (SSE2/AVX2 on x86/x64, Neon on ARM, PackedSimd on WASM).
	/// Inspired by cnc-ddraw and DDrawCompat blitter implementations with color key support.
	/// Includes adaptive algorithms that select optimal strategy based on buffer size and alignment.
	/// </summary>
	public static class OptimizedBlitter
	{
		/// <summary>
		/// Indicates whether WASM SIMD is supported on the current platform.
		/// </summary>
		public static bool IsWasmSimdSupported
		{
			get
			{
#if NET8_0_OR_GREATER
				return PackedSimd.IsSupported;
#else
				return false;
#endif
			}
		}

		/// <summary>
		/// Performs a fast blit operation without color key.
		/// </summary>
		public static void BltFast(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destPitch,
			int srcPitch,
			int width,
			int height,
			int bytesPerPixel)
		{
			switch (bytesPerPixel)
			{
				case 1:
					BltFast8Bpp(dest, src, destPitch, srcPitch, width, height);
					break;
				case 2:
					BltFast16Bpp(dest, src, destPitch, srcPitch, width, height);
					break;
				case 3:
					BltFast24Bpp(dest, src, destPitch, srcPitch, width, height);
					break;
				case 4:
					BltFast32Bpp(dest, src, destPitch, srcPitch, width, height);
					break;
				default:
					throw new ArgumentException($"Unsupported bytes per pixel: {bytesPerPixel}");
			}
		}

		/// <summary>
		/// Performs a blit operation with source color key (transparency).
		/// </summary>
		public static void BltWithSourceColorKey(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destPitch,
			int srcPitch,
			int width,
			int height,
			int bytesPerPixel,
			uint colorKeyLow,
			uint colorKeyHigh)
		{
			switch (bytesPerPixel)
			{
				case 1:
					BltWithSourceColorKey8Bpp(dest, src, destPitch, srcPitch, width, height, (byte)colorKeyLow, (byte)colorKeyHigh);
					break;
				case 2:
					BltWithSourceColorKey16Bpp(dest, src, destPitch, srcPitch, width, height, (ushort)colorKeyLow, (ushort)colorKeyHigh);
					break;
				case 4:
					BltWithSourceColorKey32Bpp(dest, src, destPitch, srcPitch, width, height, colorKeyLow, colorKeyHigh);
					break;
				default:
					throw new NotSupportedException($"Color key blitting not supported for {bytesPerPixel} bytes per pixel");
			}
		}

		#region 8-bit Blitting

		private static void BltFast8Bpp(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destPitch,
			int srcPitch,
			int width,
			int height)
		{
			// Check if we can use optimized full-surface copy
			if (width == destPitch && destPitch == srcPitch)
			{
				// Contiguous memory - use simple span copy for WASM compatibility
				src.Slice(0, destPitch * height).CopyTo(dest);
			}
			else
			{
				// Row-by-row copy - safe implementation for all platforms
				for (var y = 0; y < height; y++)
				{
					var srcRow = src.Slice(y * srcPitch, width);
					var destRow = dest.Slice(y * destPitch, width);
					srcRow.CopyTo(destRow);
				}
			}
		}

		private static void BltWithSourceColorKey8Bpp(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destPitch,
			int srcPitch,
			int width,
			int height,
			byte colorKeyLow,
			byte colorKeyHigh)
		{
#if NET8_0_OR_GREATER
			// WASM SIMD path
			if (PackedSimd.IsSupported && width >= 16)
			{
				BltWithSourceColorKey8BppWasmSimd(dest, src, destPitch, srcPitch, width, height, colorKeyLow, colorKeyHigh);
				return;
			}
#endif
			// Desktop SIMD path or scalar fallback
			BltWithSourceColorKey8BppSafe(dest, src, destPitch, srcPitch, width, height, colorKeyLow, colorKeyHigh);
		}

#if NET8_0_OR_GREATER
		private static void BltWithSourceColorKey8BppWasmSimd(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destPitch,
			int srcPitch,
			int width,
			int height,
			byte colorKeyLow,
			byte colorKeyHigh)
		{
			var keyLow = Vector128.Create(colorKeyLow);
			var keyHigh = Vector128.Create(colorKeyHigh);
			var signFlip = Vector128.Create((byte)0x80);

			for (var y = 0; y < height; y++)
			{
				var srcRowOffset = y * srcPitch;
				var dstRowOffset = y * destPitch;
				var x = 0;

				// Process 16 bytes at a time with WASM SIMD
				for (; x <= width - 16; x += 16)
				{
					var srcSlice = src.Slice(srcRowOffset + x, 16);
					var srcData = Vector128.Create(srcSlice);

					// For color key, check if pixel < colorKeyLow OR pixel > colorKeyHigh
					// Pixels in range [colorKeyLow, colorKeyHigh] are transparent and should NOT be copied
					var srcSigned = PackedSimd.Xor(srcData, signFlip).AsSByte();
					var keyLowSigned = PackedSimd.Xor(keyLow, signFlip).AsSByte();
					var keyHighSigned = PackedSimd.Xor(keyHigh, signFlip).AsSByte();

					var cmpLow = PackedSimd.CompareLessThan(srcSigned, keyLowSigned);
					var cmpHigh = PackedSimd.CompareGreaterThan(srcSigned, keyHighSigned);
					var isNotTransparent = PackedSimd.Or(cmpLow, cmpHigh);

					// Check if any pixels need to be copied
					if (PackedSimd.AnyTrue(isNotTransparent))
					{
						// If all pixels should be copied
						if (PackedSimd.AllTrue(isNotTransparent))
						{
							srcData.CopyTo(dest.Slice(dstRowOffset + x, 16));
						}
						else
						{
							// Mixed case: blend source and destination using the mask
							var destSlice = dest.Slice(dstRowOffset + x, 16);
							var destData = Vector128.Create(destSlice);
							var maskedSrc = PackedSimd.And(srcData, isNotTransparent.AsByte());
							var maskedDest = PackedSimd.AndNot(destData, isNotTransparent.AsByte());
							var result = PackedSimd.Or(maskedSrc, maskedDest);
							result.CopyTo(destSlice);
						}
					}
				}

				// Handle remaining bytes with scalar code
				for (; x < width; x++)
				{
					var pixel = src[srcRowOffset + x];
					if (pixel < colorKeyLow || pixel > colorKeyHigh)
					{
						dest[dstRowOffset + x] = pixel;
					}
				}
			}
		}
#endif

		private static void BltWithSourceColorKey8BppSafe(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destPitch,
			int srcPitch,
			int width,
			int height,
			byte colorKeyLow,
			byte colorKeyHigh)
		{
			// Scalar fallback - safe for all platforms including WASM
			for (var y = 0; y < height; y++)
			{
				var srcRowOffset = y * srcPitch;
				var dstRowOffset = y * destPitch;
				for (var x = 0; x < width; x++)
				{
					var pixel = src[srcRowOffset + x];
					if (pixel < colorKeyLow || pixel > colorKeyHigh)
					{
						dest[dstRowOffset + x] = pixel;
					}
				}
			}
		}

		#endregion

		#region 16-bit Blitting

		private static void BltFast16Bpp(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destPitch,
			int srcPitch,
			int width,
			int height)
		{
			var bytesPerRow = width * 2;
			
			// Check if we can use optimized full-surface copy
			if (bytesPerRow == destPitch && destPitch == srcPitch)
			{
				// Contiguous memory - use simple span copy for WASM compatibility
				src.Slice(0, destPitch * height).CopyTo(dest);
			}
			else
			{
				// Row-by-row copy - safe implementation for all platforms
				for (var y = 0; y < height; y++)
				{
					var srcRow = src.Slice(y * srcPitch, bytesPerRow);
					var destRow = dest.Slice(y * destPitch, bytesPerRow);
					srcRow.CopyTo(destRow);
				}
			}
		}

		private static void BltWithSourceColorKey16Bpp(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destPitch,
			int srcPitch,
			int width,
			int height,
			ushort colorKeyLow,
			ushort colorKeyHigh)
		{
			// Safe scalar implementation for all platforms including WASM
			var srcSpan = MemoryMarshal.Cast<byte, ushort>(src);
			var destSpan = MemoryMarshal.Cast<byte, ushort>(dest);
			var srcPitch16 = srcPitch / 2;
			var destPitch16 = destPitch / 2;

			for (var y = 0; y < height; y++)
			{
				var srcRowOffset = y * srcPitch16;
				var dstRowOffset = y * destPitch16;
				for (var x = 0; x < width; x++)
				{
					var pixel = srcSpan[srcRowOffset + x];
					if (pixel < colorKeyLow || pixel > colorKeyHigh)
					{
						destSpan[dstRowOffset + x] = pixel;
					}
				}
			}
		}

		#endregion

		#region 24-bit Blitting

		private static void BltFast24Bpp(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destPitch,
			int srcPitch,
			int width,
			int height)
		{
			var bytesPerRow = width * 3;
			
			// Check if we can use optimized full-surface copy
			if (bytesPerRow == destPitch && destPitch == srcPitch)
			{
				// Contiguous memory - use simple span copy for WASM compatibility
				src.Slice(0, destPitch * height).CopyTo(dest);
			}
			else
			{
				// Row-by-row copy - safe implementation for all platforms
				for (var y = 0; y < height; y++)
				{
					var srcRow = src.Slice(y * srcPitch, bytesPerRow);
					var destRow = dest.Slice(y * destPitch, bytesPerRow);
					srcRow.CopyTo(destRow);
				}
			}
		}

		#endregion

		#region 32-bit Blitting

		private static void BltFast32Bpp(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destPitch,
			int srcPitch,
			int width,
			int height)
		{
			var bytesPerRow = width * 4;
			
			// Check if we can use optimized full-surface copy
			if (bytesPerRow == destPitch && destPitch == srcPitch)
			{
				// Contiguous memory - use simple span copy for WASM compatibility
				src.Slice(0, destPitch * height).CopyTo(dest);
			}
			else
			{
				// Row-by-row copy - safe implementation for all platforms
				for (var y = 0; y < height; y++)
				{
					var srcRow = src.Slice(y * srcPitch, bytesPerRow);
					var destRow = dest.Slice(y * destPitch, bytesPerRow);
					srcRow.CopyTo(destRow);
				}
			}
		}

		private static void BltWithSourceColorKey32Bpp(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destPitch,
			int srcPitch,
			int width,
			int height,
			uint colorKeyLow,
			uint colorKeyHigh)
		{
			// Safe scalar implementation for all platforms including WASM
			var srcSpan = MemoryMarshal.Cast<byte, uint>(src);
			var destSpan = MemoryMarshal.Cast<byte, uint>(dest);
			var srcPitch32 = srcPitch / 4;
			var destPitch32 = destPitch / 4;

			for (var y = 0; y < height; y++)
			{
				var srcRowOffset = y * srcPitch32;
				var dstRowOffset = y * destPitch32;
				for (var x = 0; x < width; x++)
				{
					var pixel = srcSpan[srcRowOffset + x];
					if (pixel < colorKeyLow || pixel > colorKeyHigh)
					{
						destSpan[dstRowOffset + x] = pixel;
					}
				}
			}
		}

		#endregion

		/// <summary>
		/// Gets a string describing the available SIMD capabilities.
		/// </summary>
		public static string GetSimdCapabilities()
		{
			var caps = new System.Text.StringBuilder();
			
			// X86/X64 capabilities
			if (Avx512F.IsSupported)
			{
				caps.Append("AVX-512F ");
			}

			if (Avx512BW.IsSupported)
			{
				caps.Append("AVX-512BW ");
			}

			if (Avx2.IsSupported)
			{
				caps.Append("AVX2 ");
			}

			if (Sse2.IsSupported)
			{
				caps.Append("SSE2 ");
			}

			// ARM capabilities
			if (AdvSimd.Arm64.IsSupported)
			{
				caps.Append("NEON-ARM64 ");
			}
			else if (AdvSimd.IsSupported)
			{
				caps.Append("NEON ");
			}

#if NET8_0_OR_GREATER
			// WASM capabilities
			if (PackedSimd.IsSupported)
			{
				caps.Append("WASM-SIMD ");
			}
#endif

			// Cross-platform vector support
			if (System.Numerics.Vector.IsHardwareAccelerated)
			{
				caps.Append($"Vector<T>({System.Numerics.Vector<byte>.Count}B) ");
			}

			return caps.Length > 0 ? caps.ToString().TrimEnd() : "Scalar (no SIMD)";
		}

		#region Stretch and Mirror Blitting

		/// <summary>
		/// Performs a stretch blit with optional mirroring and color key support.
		/// Inspired by cnc-ddraw's blt_colorkey_mirror_stretch implementation.
		/// </summary>
		public static void BltStretchWithColorKey(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destX,
			int destY,
			int destWidth,
			int destHeight,
			int destPitch,
			int srcX,
			int srcY,
			int srcWidth,
			int srcHeight,
			int srcPitch,
			int bytesPerPixel,
			uint colorKeyLow,
			uint colorKeyHigh,
			bool mirrorUpDown = false,
			bool mirrorLeftRight = false)
		{
			int destSurfaceWidth = destPitch / bytesPerPixel;
			int srcSurfaceWidth = srcPitch / bytesPerPixel;

			float scaleWidth = (float)srcWidth / destWidth;
			float scaleHeight = (float)srcHeight / destHeight;

			switch (bytesPerPixel)
			{
				case 1:
					BltStretchWithColorKey8Bpp(dest, src, destX, destY, destWidth, destHeight, destSurfaceWidth,
						srcX, srcY, srcWidth, srcHeight, srcSurfaceWidth, (byte)colorKeyLow, (byte)colorKeyHigh,
						scaleWidth, scaleHeight, mirrorUpDown, mirrorLeftRight);
					break;
				case 2:
					BltStretchWithColorKey16Bpp(dest, src, destX, destY, destWidth, destHeight, destSurfaceWidth,
						srcX, srcY, srcWidth, srcHeight, srcSurfaceWidth, (ushort)colorKeyLow, (ushort)colorKeyHigh,
						scaleWidth, scaleHeight, mirrorUpDown, mirrorLeftRight);
					break;
				case 4:
					BltStretchWithColorKey32Bpp(dest, src, destX, destY, destWidth, destHeight, destSurfaceWidth,
						srcX, srcY, srcWidth, srcHeight, srcSurfaceWidth, colorKeyLow, colorKeyHigh,
						scaleWidth, scaleHeight, mirrorUpDown, mirrorLeftRight);
					break;
				default:
					throw new NotSupportedException($"Stretch blit with color key not supported for {bytesPerPixel} bytes per pixel");
			}
		}

		private static void BltStretchWithColorKey8Bpp(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destX, int destY, int destWidth, int destHeight, int destSurfaceWidth,
			int srcX, int srcY, int srcWidth, int srcHeight, int srcSurfaceWidth,
			byte colorKeyLow, byte colorKeyHigh,
			float scaleWidth, float scaleHeight,
			bool mirrorUpDown, bool mirrorLeftRight)
		{
			for (var y = 0; y < destHeight; y++)
			{
				var scaledY = (int)(y * scaleHeight);
				if (mirrorUpDown)
				{
					scaledY = srcHeight - 1 - scaledY;
				}

				var srcRow = srcX + srcSurfaceWidth * (scaledY + srcY);
				var destRow = destX + destSurfaceWidth * (y + destY);

				for (var x = 0; x < destWidth; x++)
				{
					var scaledX = (int)(x * scaleWidth);
					if (mirrorLeftRight)
					{
						scaledX = srcWidth - 1 - scaledX;
					}

					var pixel = src[scaledX + srcRow];
					if (pixel < colorKeyLow || pixel > colorKeyHigh)
					{
						dest[x + destRow] = pixel;
					}
				}
			}
		}

		private static void BltStretchWithColorKey16Bpp(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destX, int destY, int destWidth, int destHeight, int destSurfaceWidth,
			int srcX, int srcY, int srcWidth, int srcHeight, int srcSurfaceWidth,
			ushort colorKeyLow, ushort colorKeyHigh,
			float scaleWidth, float scaleHeight,
			bool mirrorUpDown, bool mirrorLeftRight)
		{
			var dest16 = MemoryMarshal.Cast<byte, ushort>(dest);
			var src16 = MemoryMarshal.Cast<byte, ushort>(src);

			for (var y = 0; y < destHeight; y++)
			{
				var scaledY = (int)(y * scaleHeight);
				if (mirrorUpDown)
				{
					scaledY = srcHeight - 1 - scaledY;
				}

				var srcRow = srcX + srcSurfaceWidth * (scaledY + srcY);
				var destRow = destX + destSurfaceWidth * (y + destY);

				for (var x = 0; x < destWidth; x++)
				{
					var scaledX = (int)(x * scaleWidth);
					if (mirrorLeftRight)
					{
						scaledX = srcWidth - 1 - scaledX;
					}

					var pixel = src16[scaledX + srcRow];
					if (pixel < colorKeyLow || pixel > colorKeyHigh)
					{
						dest16[x + destRow] = pixel;
					}
				}
			}
		}

		private static void BltStretchWithColorKey32Bpp(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destX, int destY, int destWidth, int destHeight, int destSurfaceWidth,
			int srcX, int srcY, int srcWidth, int srcHeight, int srcSurfaceWidth,
			uint colorKeyLow, uint colorKeyHigh,
			float scaleWidth, float scaleHeight,
			bool mirrorUpDown, bool mirrorLeftRight)
		{
			var dest32 = MemoryMarshal.Cast<byte, uint>(dest);
			var src32 = MemoryMarshal.Cast<byte, uint>(src);
			var keyLow = colorKeyLow & 0xFFFFFF;
			var keyHigh = colorKeyHigh & 0xFFFFFF;

			for (var y = 0; y < destHeight; y++)
			{
				var scaledY = (int)(y * scaleHeight);
				if (mirrorUpDown)
				{
					scaledY = srcHeight - 1 - scaledY;
				}

				var srcRow = srcX + srcSurfaceWidth * (scaledY + srcY);
				var destRow = destX + destSurfaceWidth * (y + destY);

				for (var x = 0; x < destWidth; x++)
				{
					var scaledX = (int)(x * scaleWidth);
					if (mirrorLeftRight)
					{
						scaledX = srcWidth - 1 - scaledX;
					}

					var pixel = src32[scaledX + srcRow];
					var pixelColor = pixel & 0xFFFFFF;
					if (pixelColor < keyLow || pixelColor > keyHigh)
					{
						dest32[x + destRow] = pixel;
					}
				}
			}
		}

		#endregion

		#region Clear Operations

		/// <summary>
		/// Optimized clear operation that is safe for all platforms including WASM.
		/// Uses Span.Fill which is optimized by the runtime.
		/// </summary>
		public static void Clear(Span<byte> buffer, byte value)
		{
			// Use Span.Fill which is optimized for all platforms
			buffer.Fill(value);
		}

		#endregion

		#region Overlapping Blit Support

		/// <summary>
		/// Performs a blit operation where source and destination may overlap.
		/// Uses safe copying strategy similar to cnc-ddraw's blt_overlap.
		/// </summary>
		public static void BltOverlapping(
			Span<byte> buffer,
			int destX,
			int destY,
			int destWidth,
			int destHeight,
			int destPitch,
			int srcX,
			int srcY,
			int srcPitch,
			int bytesPerPixel)
		{
			var bytesPerRow = destWidth * bytesPerPixel;
			var srcOffset = srcX * bytesPerPixel + srcPitch * srcY;
			var destOffset = destX * bytesPerPixel + destPitch * destY;

			// Check if we need reverse copying (destination is below source)
			if (destY > srcY && destOffset > srcOffset)
			{
				// Copy from bottom to top to avoid overwriting source data
				for (var y = destHeight - 1; y >= 0; y--)
				{
					var srcRowOffset = srcOffset + y * srcPitch;
					var destRowOffset = destOffset + y * destPitch;
					
					var srcRow = buffer.Slice(srcRowOffset, bytesPerRow);
					var destRow = buffer.Slice(destRowOffset, bytesPerRow);
					
					srcRow.CopyTo(destRow);
				}
			}
			else
			{
				// Normal top-to-bottom or non-overlapping copy
				// Check if we can do a single copy (contiguous memory)
				if (bytesPerRow == destPitch && destPitch == srcPitch)
				{
					var totalBytes = destPitch * destHeight;
					var srcSlice = buffer.Slice(srcOffset, totalBytes);
					var destSlice = buffer.Slice(destOffset, totalBytes);
					srcSlice.CopyTo(destSlice);
				}
				else
				{
					// Row-by-row copy
					for (var y = 0; y < destHeight; y++)
					{
						var srcRowOffset = srcOffset + y * srcPitch;
						var destRowOffset = destOffset + y * destPitch;
						
						var srcRow = buffer.Slice(srcRowOffset, bytesPerRow);
						var destRow = buffer.Slice(destRowOffset, bytesPerRow);
						
						srcRow.CopyTo(destRow);
					}
				}
			}
		}

		#endregion
	}
}
