using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics.Arm;

namespace Win32Emu.Win32.DirectDraw
{
	/// <summary>
	/// High-performance blitter optimized with SIMD intrinsics (SSE2/AVX2 on x86/x64, Neon on ARM).
	/// Inspired by cnc-ddraw and DDrawCompat blitter implementations with color key support.
	/// Includes adaptive algorithms that select optimal strategy based on buffer size and alignment.
	/// </summary>
	public static class OptimizedBlitter
	{
		// Thresholds for selecting optimization strategies (from cnc-ddraw)
		private const int LARGE_BUFFER_THRESHOLD = 4096 * 1024; // 4MB - use streaming stores
		private const int SMALL_BUFFER_THRESHOLD = 100 * 1024;  // 100KB - use regular stores

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
			// For 8-bit, simple row-by-row copy is often fastest
			for (var y = 0; y < height; y++)
			{
				var destRow = dest.Slice(y * destPitch, width);
				var srcRow = src.Slice(y * srcPitch, width);
				srcRow.CopyTo(destRow);
			}
		}

		private static unsafe void BltWithSourceColorKey8Bpp(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destPitch,
			int srcPitch,
			int width,
			int height,
			byte colorKeyLow,
			byte colorKeyHigh)
		{
			fixed (byte* destPtr = dest)
			fixed (byte* srcPtr = src)
			{
				byte* dstRow = destPtr;
				byte* srcRow = srcPtr;

				if (Sse2.IsSupported && width >= 16)
				{
					var keyLow = Vector128.Create(colorKeyLow);
					var keyHigh = Vector128.Create(colorKeyHigh);

					for (var y = 0; y < height; y++)
					{
						var x = 0;

						// Process 16 bytes at a time with SSE2
						for (; x <= width - 16; x += 16)
						{
							var srcData = Sse2.LoadVector128(srcRow + x);
							
							// For color key, we check if pixel < colorKeyLow OR pixel > colorKeyHigh
							// Pixels in range [colorKeyLow, colorKeyHigh] are transparent and should NOT be copied
							// This is equivalent to: isNotTransparent = (pixel < low) | (pixel > high)
							// For unsigned bytes, use signed comparison after XOR with 0x80
							var cmpLow = Sse2.CompareLessThan(
								Sse2.Xor(srcData, Vector128.Create((byte)0x80)).AsSByte(),
								Sse2.Xor(keyLow, Vector128.Create((byte)0x80)).AsSByte());
							var cmpHigh = Sse2.CompareGreaterThan(
								Sse2.Xor(srcData, Vector128.Create((byte)0x80)).AsSByte(),
								Sse2.Xor(keyHigh, Vector128.Create((byte)0x80)).AsSByte());
							var isNotTransparent = Sse2.Or(cmpLow, cmpHigh);

							var mask = Sse2.MoveMask(isNotTransparent);
							
							// If all bytes are transparent (mask == 0, all pixels in color key range), skip
							if (mask == 0)
								continue;

							// If all bytes are NOT transparent (mask == 0xFFFF, no pixels in range), copy entire vector
							if (mask == 0xFFFF)
							{
								Sse2.Store(dstRow + x, srcData);
								continue;
							}

							// Mixed case: use SIMD to blend source and destination
							// isNotTransparent has bits set to 1 for pixels to copy from source
							var destData = Sse2.LoadVector128(dstRow + x);
							var maskedSrc = Sse2.And(srcData, isNotTransparent.AsByte()); // Select src where mask=1
							var maskedDest = Sse2.AndNot(isNotTransparent.AsByte(), destData); // AndNot(a,b) = (~a) & b, selects dest where mask=0
							var result = Sse2.Or(maskedSrc, maskedDest); // Combine: src where non-transparent, dest where transparent
							Sse2.Store(dstRow + x, result);
						}

						// Handle remaining bytes
						for (; x < width; x++)
						{
							var pixel = srcRow[x];
							if (pixel < colorKeyLow || pixel > colorKeyHigh)
							{
								dstRow[x] = pixel;
							}
						}

						dstRow += destPitch;
						srcRow += srcPitch;
					}
				}
				else
				{
					// Scalar fallback
					for (var y = 0; y < height; y++)
					{
						for (var x = 0; x < width; x++)
						{
							var pixel = srcRow[x];
							if (pixel < colorKeyLow || pixel > colorKeyHigh)
							{
								dstRow[x] = pixel;
							}
						}

						dstRow += destPitch;
						srcRow += srcPitch;
					}
				}
			}
		}

		#endregion

		#region 16-bit Blitting

		private static unsafe void BltFast16Bpp(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destPitch,
			int srcPitch,
			int width,
			int height)
		{
			fixed (byte* destPtr = dest)
			fixed (byte* srcPtr = src)
			{
				byte* dstRow = destPtr;
				byte* srcRow = srcPtr;

				if (Sse2.IsSupported && width >= 8)
				{
					for (var y = 0; y < height; y++)
					{
						var x = 0;

						// Process 8 pixels (16 bytes) at a time
						for (; x <= width - 8; x += 8)
						{
							var srcData = Sse2.LoadVector128(srcRow + x * 2);
							Sse2.Store(dstRow + x * 2, srcData);
						}

						// Handle remaining pixels
						for (; x < width; x++)
						{
							((ushort*)(dstRow + x * 2))[0] = ((ushort*)(srcRow + x * 2))[0];
						}

						dstRow += destPitch;
						srcRow += srcPitch;
					}
				}
				else
				{
					// Scalar fallback
					for (var y = 0; y < height; y++)
					{
						var destRowSpan = dest.Slice(y * destPitch, width * 2);
						var srcRowSpan = src.Slice(y * srcPitch, width * 2);
						srcRowSpan.CopyTo(destRowSpan);
					}
				}
			}
		}

		private static unsafe void BltWithSourceColorKey16Bpp(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destPitch,
			int srcPitch,
			int width,
			int height,
			ushort colorKeyLow,
			ushort colorKeyHigh)
		{
			fixed (byte* destPtr = dest)
			fixed (byte* srcPtr = src)
			{
				byte* dstRow = destPtr;
				byte* srcRow = srcPtr;

				if (Sse2.IsSupported && width >= 8)
				{
					var keyLow = Vector128.Create(colorKeyLow);
					var keyHigh = Vector128.Create(colorKeyHigh);

					for (var y = 0; y < height; y++)
					{
						var x = 0;

						// Process 8 pixels at a time
						for (; x <= width - 8; x += 8)
						{
							var srcData = Sse2.LoadVector128((ushort*)(srcRow + x * 2));
							
							// Check if pixel < colorKeyLow OR pixel > colorKeyHigh (not transparent)
							// Use XOR trick to convert to unsigned comparison
							var cmpLow = Sse2.CompareLessThan(
								Sse2.Xor(srcData, Vector128.Create((ushort)0x8000)).AsInt16(),
								Sse2.Xor(keyLow, Vector128.Create((ushort)0x8000)).AsInt16());
							var cmpHigh = Sse2.CompareGreaterThan(
								Sse2.Xor(srcData, Vector128.Create((ushort)0x8000)).AsInt16(),
								Sse2.Xor(keyHigh, Vector128.Create((ushort)0x8000)).AsInt16());
							var isNotTransparent = Sse2.Or(cmpLow, cmpHigh);

							var mask = Sse2.MoveMask(isNotTransparent.AsByte());
							
							// If all pixels are transparent (mask == 0), skip
							if (mask == 0)
								continue;

							// If no pixels are transparent (mask == 0xFFFF), copy entire vector
							if (mask == 0xFFFF)
							{
								Sse2.Store((ushort*)(dstRow + x * 2), srcData);
								continue;
							}

							// Mixed case: use SIMD to blend source and destination
							// isNotTransparent has bits set to 1 for pixels to copy from source
							var destData = Sse2.LoadVector128((ushort*)(dstRow + x * 2));
							var maskedSrc = Sse2.And(srcData.AsByte(), isNotTransparent.AsByte()); // Select src where mask=1
							var maskedDest = Sse2.AndNot(isNotTransparent.AsByte(), destData.AsByte()); // AndNot(a,b) = (~a) & b, selects dest where mask=0
							var result = Sse2.Or(maskedSrc, maskedDest); // Combine: src where non-transparent, dest where transparent
							Sse2.Store((ushort*)(dstRow + x * 2), result.AsUInt16());
						}

						// Handle remaining pixels
						for (; x < width; x++)
						{
							var pixel = ((ushort*)(srcRow + x * 2))[0];
							if (pixel < colorKeyLow || pixel > colorKeyHigh)
							{
								((ushort*)(dstRow + x * 2))[0] = pixel;
							}
						}

						dstRow += destPitch;
						srcRow += srcPitch;
					}
				}
				else
				{
					// Scalar fallback
					for (var y = 0; y < height; y++)
					{
						for (var x = 0; x < width; x++)
						{
							var pixel = ((ushort*)(srcRow + x * 2))[0];
							if (pixel < colorKeyLow || pixel > colorKeyHigh)
							{
								((ushort*)(dstRow + x * 2))[0] = pixel;
							}
						}

						dstRow += destPitch;
						srcRow += srcPitch;
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
			// 24-bit is awkward for SIMD, use simple row copy
			for (var y = 0; y < height; y++)
			{
				var destRow = dest.Slice(y * destPitch, width * 3);
				var srcRow = src.Slice(y * srcPitch, width * 3);
				srcRow.CopyTo(destRow);
			}
		}

		#endregion

		#region 32-bit Blitting

		private static unsafe void BltFast32Bpp(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destPitch,
			int srcPitch,
			int width,
			int height)
		{
			fixed (byte* destPtr = dest)
			fixed (byte* srcPtr = src)
			{
				byte* dstRow = destPtr;
				byte* srcRow = srcPtr;

				if (Sse2.IsSupported && width >= 4)
				{
					for (var y = 0; y < height; y++)
					{
						var x = 0;

						// Process 4 pixels (16 bytes) at a time
						for (; x <= width - 4; x += 4)
						{
							var srcData = Sse2.LoadVector128(srcRow + x * 4);
							Sse2.Store(dstRow + x * 4, srcData);
						}

						// Handle remaining pixels
						for (; x < width; x++)
						{
							((uint*)(dstRow + x * 4))[0] = ((uint*)(srcRow + x * 4))[0];
						}

						dstRow += destPitch;
						srcRow += srcPitch;
					}
				}
				else
				{
					// Scalar fallback
					for (var y = 0; y < height; y++)
					{
						var destRowSpan = dest.Slice(y * destPitch, width * 4);
						var srcRowSpan = src.Slice(y * srcPitch, width * 4);
						srcRowSpan.CopyTo(destRowSpan);
					}
				}
			}
		}

		private static unsafe void BltWithSourceColorKey32Bpp(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destPitch,
			int srcPitch,
			int width,
			int height,
			uint colorKeyLow,
			uint colorKeyHigh)
		{
			fixed (byte* destPtr = dest)
			fixed (byte* srcPtr = src)
			{
				byte* dstRow = destPtr;
				byte* srcRow = srcPtr;

				if (Sse2.IsSupported && width >= 4)
				{
					var keyLow = Vector128.Create(colorKeyLow);
					var keyHigh = Vector128.Create(colorKeyHigh);

					for (var y = 0; y < height; y++)
					{
						var x = 0;

						// Process 4 pixels at a time
						for (; x <= width - 4; x += 4)
						{
							var srcData = Sse2.LoadVector128((uint*)(srcRow + x * 4));
							
							// Check if pixel < colorKeyLow OR pixel > colorKeyHigh (not transparent)
							// Use XOR trick to convert to unsigned comparison
							var cmpLow = Sse2.CompareLessThan(
								Sse2.Xor(srcData, Vector128.Create(0x80000000u)).AsInt32(),
								Sse2.Xor(keyLow, Vector128.Create(0x80000000u)).AsInt32());
							var cmpHigh = Sse2.CompareGreaterThan(
								Sse2.Xor(srcData, Vector128.Create(0x80000000u)).AsInt32(),
								Sse2.Xor(keyHigh, Vector128.Create(0x80000000u)).AsInt32());
							var isNotTransparent = Sse2.Or(cmpLow, cmpHigh);

							var mask = Sse2.MoveMask(isNotTransparent.AsByte());
							
							// If all pixels are transparent (mask == 0), skip
							if (mask == 0)
								continue;

							// If no pixels are transparent (mask == 0xFFFF), copy entire vector
							if (mask == 0xFFFF)
							{
								Sse2.Store((uint*)(dstRow + x * 4), srcData);
								continue;
							}

							// Mixed case: use SIMD to blend source and destination
							// isNotTransparent has bits set to 1 for pixels to copy from source
							var destData = Sse2.LoadVector128((uint*)(dstRow + x * 4));
							var maskedSrc = Sse2.And(srcData.AsByte(), isNotTransparent.AsByte()); // Select src where mask=1
							var maskedDest = Sse2.AndNot(isNotTransparent.AsByte(), destData.AsByte()); // AndNot(a,b) = (~a) & b, selects dest where mask=0
							var result = Sse2.Or(maskedSrc, maskedDest); // Combine: src where non-transparent, dest where transparent
							Sse2.Store((uint*)(dstRow + x * 4), result.AsUInt32());
						}

						// Handle remaining pixels
						for (; x < width; x++)
						{
							var pixel = ((uint*)(srcRow + x * 4))[0];
							if (pixel < colorKeyLow || pixel > colorKeyHigh)
							{
								((uint*)(dstRow + x * 4))[0] = pixel;
							}
						}

						dstRow += destPitch;
						srcRow += srcPitch;
					}
				}
				else if (AdvSimd.IsSupported && width >= 4)
				{
					// ARM Neon implementation
					var keyLow = Vector128.Create(colorKeyLow);
					var keyHigh = Vector128.Create(colorKeyHigh);

					for (var y = 0; y < height; y++)
					{
						var x = 0;

						// Process 4 pixels at a time with Neon
						for (; x <= width - 4; x += 4)
						{
							var srcData = AdvSimd.LoadVector128((uint*)(srcRow + x * 4));
							
							// Check if pixels are outside color key range
							var isLtLow = AdvSimd.CompareLessThan(srcData, keyLow);
							var isGtHigh = AdvSimd.CompareGreaterThan(srcData, keyHigh);
							var isNotTransparent = AdvSimd.Or(isLtLow, isGtHigh);

							// Check if any pixels are non-transparent
							var anyOpaque = AdvSimd.Arm64.MaxAcross(isNotTransparent);
							if (anyOpaque.ToScalar() == 0)
								continue;

							// Mixed case: copy non-transparent pixels individually
							for (var i = 0; i < 4; i++)
							{
								var pixel = ((uint*)(srcRow + (x + i) * 4))[0];
								if (pixel < colorKeyLow || pixel > colorKeyHigh)
								{
									((uint*)(dstRow + (x + i) * 4))[0] = pixel;
								}
							}
						}

						// Handle remaining pixels
						for (; x < width; x++)
						{
							var pixel = ((uint*)(srcRow + x * 4))[0];
							if (pixel < colorKeyLow || pixel > colorKeyHigh)
							{
								((uint*)(dstRow + x * 4))[0] = pixel;
							}
						}

						dstRow += destPitch;
						srcRow += srcPitch;
					}
				}
				else
				{
					// Scalar fallback
					for (var y = 0; y < height; y++)
					{
						for (var x = 0; x < width; x++)
						{
							var pixel = ((uint*)(srcRow + x * 4))[0];
							if (pixel < colorKeyLow || pixel > colorKeyHigh)
							{
								((uint*)(dstRow + x * 4))[0] = pixel;
							}
						}

						dstRow += destPitch;
						srcRow += srcPitch;
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
				caps.Append("AVX-512F ");
			if (Avx512BW.IsSupported)
				caps.Append("AVX-512BW ");
			if (Avx2.IsSupported)
				caps.Append("AVX2 ");
			if (Sse2.IsSupported)
				caps.Append("SSE2 ");
			
			// ARM capabilities
			if (AdvSimd.Arm64.IsSupported)
				caps.Append("NEON-ARM64 ");
			else if (AdvSimd.IsSupported)
				caps.Append("NEON ");
			
			// Cross-platform vector support
			if (System.Numerics.Vector.IsHardwareAccelerated)
				caps.Append($"Vector<T>({System.Numerics.Vector<byte>.Count}B) ");
			
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

		private static unsafe void BltStretchWithColorKey8Bpp(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destX, int destY, int destWidth, int destHeight, int destSurfaceWidth,
			int srcX, int srcY, int srcWidth, int srcHeight, int srcSurfaceWidth,
			byte colorKeyLow, byte colorKeyHigh,
			float scaleWidth, float scaleHeight,
			bool mirrorUpDown, bool mirrorLeftRight)
		{
			fixed (byte* destPtr = dest)
			fixed (byte* srcPtr = src)
			{
				for (var y = 0; y < destHeight; y++)
				{
					var scaledY = (int)(y * scaleHeight);
					if (mirrorUpDown)
						scaledY = srcHeight - 1 - scaledY;

					var srcRow = srcX + srcSurfaceWidth * (scaledY + srcY);
					var destRow = destX + destSurfaceWidth * (y + destY);

					for (var x = 0; x < destWidth; x++)
					{
						var scaledX = (int)(x * scaleWidth);
						if (mirrorLeftRight)
							scaledX = srcWidth - 1 - scaledX;

						var pixel = srcPtr[scaledX + srcRow];
						if (pixel < colorKeyLow || pixel > colorKeyHigh)
						{
							destPtr[x + destRow] = pixel;
						}
					}
				}
			}
		}

		private static unsafe void BltStretchWithColorKey16Bpp(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destX, int destY, int destWidth, int destHeight, int destSurfaceWidth,
			int srcX, int srcY, int srcWidth, int srcHeight, int srcSurfaceWidth,
			ushort colorKeyLow, ushort colorKeyHigh,
			float scaleWidth, float scaleHeight,
			bool mirrorUpDown, bool mirrorLeftRight)
		{
			fixed (byte* destPtr = dest)
			fixed (byte* srcPtr = src)
			{
				var dest16 = (ushort*)destPtr;
				var src16 = (ushort*)srcPtr;

				for (var y = 0; y < destHeight; y++)
				{
					var scaledY = (int)(y * scaleHeight);
					if (mirrorUpDown)
						scaledY = srcHeight - 1 - scaledY;

					var srcRow = srcX + srcSurfaceWidth * (scaledY + srcY);
					var destRow = destX + destSurfaceWidth * (y + destY);

					for (var x = 0; x < destWidth; x++)
					{
						var scaledX = (int)(x * scaleWidth);
						if (mirrorLeftRight)
							scaledX = srcWidth - 1 - scaledX;

						var pixel = src16[scaledX + srcRow];
						if (pixel < colorKeyLow || pixel > colorKeyHigh)
						{
							dest16[x + destRow] = pixel;
						}
					}
				}
			}
		}

		private static unsafe void BltStretchWithColorKey32Bpp(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destX, int destY, int destWidth, int destHeight, int destSurfaceWidth,
			int srcX, int srcY, int srcWidth, int srcHeight, int srcSurfaceWidth,
			uint colorKeyLow, uint colorKeyHigh,
			float scaleWidth, float scaleHeight,
			bool mirrorUpDown, bool mirrorLeftRight)
		{
			fixed (byte* destPtr = dest)
			fixed (byte* srcPtr = src)
			{
				var dest32 = (uint*)destPtr;
				var src32 = (uint*)srcPtr;
				var keyLow = colorKeyLow & 0xFFFFFF;
				var keyHigh = colorKeyHigh & 0xFFFFFF;

				for (var y = 0; y < destHeight; y++)
				{
					var scaledY = (int)(y * scaleHeight);
					if (mirrorUpDown)
						scaledY = srcHeight - 1 - scaledY;

					var srcRow = srcX + srcSurfaceWidth * (scaledY + srcY);
					var destRow = destX + destSurfaceWidth * (y + destY);

					for (var x = 0; x < destWidth; x++)
					{
						var scaledX = (int)(x * scaleWidth);
						if (mirrorLeftRight)
							scaledX = srcWidth - 1 - scaledX;

						var pixel = src32[scaledX + srcRow];
						var pixelColor = pixel & 0xFFFFFF;
						if (pixelColor < keyLow || pixelColor > keyHigh)
						{
							dest32[x + destRow] = pixel;
						}
					}
				}
			}
		}

		#endregion

		#region Clear Operations

		/// <summary>
		/// Optimized clear operation using AVX-512, AVX2, SSE2, ARM NEON, or scalar fallback.
		/// Inspired by cnc-ddraw's blt_clear implementation with extended SIMD support.
		/// </summary>
		public static unsafe void Clear(Span<byte> buffer, byte value)
		{
			var size = buffer.Length;
			
			if (size == 0)
				return;

			fixed (byte* ptr = buffer)
			{
				// For large buffers, use native memset which may use REP STOSB
				if (size >= SMALL_BUFFER_THRESHOLD)
				{
					buffer.Fill(value);
					return;
				}

				// Check alignment
				var isAligned64 = (((nuint)ptr) % 64) == 0;
				var isAligned32 = (((nuint)ptr) % 32) == 0;
				var isAligned16 = (((nuint)ptr) % 16) == 0;

				// AVX-512: 512-bit vectors for maximum throughput
				if (isAligned64 && size >= 256 && Avx512F.IsSupported)
				{
					var vec = Vector512.Create(value);
					var p = ptr;
					
					while (size >= 256)
					{
						Avx512F.Store(p, vec);
						Avx512F.Store(p + 64, vec);
						Avx512F.Store(p + 128, vec);
						Avx512F.Store(p + 192, vec);
						
						p += 256;
						size -= 256;
					}
					
					// Handle remaining full vectors
					while (size >= 64)
					{
						Avx512F.Store(p, vec);
						p += 64;
						size -= 64;
					}
				}
				// AVX2: 256-bit vectors for small/medium buffers with good alignment
				else if (isAligned32 && Avx2.IsSupported)
				{
					var vec = Vector256.Create(value);
					var p = ptr;
					
					while (size >= 128)
					{
						Avx2.Store(p, vec);
						Avx2.Store(p + 32, vec);
						Avx2.Store(p + 64, vec);
						Avx2.Store(p + 96, vec);
						
						p += 128;
						size -= 128;
					}
					
					// Handle remaining full vectors
					while (size >= 32)
					{
						Avx2.Store(p, vec);
						p += 32;
						size -= 32;
					}
				}
				// SSE2: 128-bit vectors
				else if (isAligned16 && Sse2.IsSupported)
				{
					var vec = Vector128.Create(value);
					var p = ptr;
					
					while (size >= 64)
					{
						Sse2.Store(p, vec);
						Sse2.Store(p + 16, vec);
						Sse2.Store(p + 32, vec);
						Sse2.Store(p + 48, vec);
						
						p += 64;
						size -= 64;
					}
					
					// Handle remaining full vectors
					while (size >= 16)
					{
						Sse2.Store(p, vec);
						p += 16;
						size -= 16;
					}
				}
				// ARM NEON: 128-bit vectors
				else if (isAligned16 && AdvSimd.IsSupported)
				{
					var vec = Vector128.Create(value);
					var p = ptr;
					
					while (size >= 64)
					{
						AdvSimd.Store(p, vec);
						AdvSimd.Store(p + 16, vec);
						AdvSimd.Store(p + 32, vec);
						AdvSimd.Store(p + 48, vec);
						
						p += 64;
						size -= 64;
					}
					
					// Handle remaining full vectors
					while (size >= 16)
					{
						AdvSimd.Store(p, vec);
						p += 16;
						size -= 16;
					}
				}
				// System.Numerics.Vector: Cross-platform fallback
				else if (System.Numerics.Vector.IsHardwareAccelerated)
				{
					var vectorSize = System.Numerics.Vector<byte>.Count;
					var vec = new System.Numerics.Vector<byte>(value);
					var bufferSpan = buffer;
					var offset = 0;
					
					while (offset + vectorSize <= size)
					{
						vec.CopyTo(bufferSpan.Slice(offset));
						offset += vectorSize;
					}
					
					size -= offset;
				}
				
				// Handle remainder with Fill
				if (size > 0)
				{
					new Span<byte>(ptr + (buffer.Length - size), size).Fill(value);
				}
			}
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

		#region Enhanced Copy with Adaptive Algorithm Selection

		/// <summary>
		/// Enhanced copy operation with adaptive algorithm selection based on buffer size and alignment.
		/// Supports AVX-512, AVX2, SSE2, ARM NEON, and System.Numerics.Vector fallbacks.
		/// Inspired by cnc-ddraw's blt_copy with extended SIMD support.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static unsafe void CopyAdaptive(byte* dest, byte* src, int size)
		{
			// Check for good alignment
			var isAligned64 = (((nuint)dest) % 64) == 0 && (((nuint)src) % 64) == 0;
			var isAligned32 = (((nuint)dest) % 32) == 0 && (((nuint)src) % 32) == 0;
			var isAligned16 = (((nuint)dest) % 16) == 0 && (((nuint)src) % 16) == 0;
			
			// AVX-512: 512-bit vectors for maximum throughput on modern CPUs
			if (isAligned64 && size >= LARGE_BUFFER_THRESHOLD && Avx512F.IsSupported)
			{
				// Large buffer with AVX-512 - process 512 bytes at a time
				while (size >= 512)
				{
					// Prefetch ahead
					Sse.Prefetch0(src + 1024);
					
					// Load 8x 512-bit vectors (512 bytes total)
					var v0 = Avx512F.LoadVector512(src);
					var v1 = Avx512F.LoadVector512(src + 64);
					var v2 = Avx512F.LoadVector512(src + 128);
					var v3 = Avx512F.LoadVector512(src + 192);
					var v4 = Avx512F.LoadVector512(src + 256);
					var v5 = Avx512F.LoadVector512(src + 320);
					var v6 = Avx512F.LoadVector512(src + 384);
					var v7 = Avx512F.LoadVector512(src + 448);
					
					// Non-temporal stores for cache bypass
					Avx512F.Store(dest, v0);
					Avx512F.Store(dest + 64, v1);
					Avx512F.Store(dest + 128, v2);
					Avx512F.Store(dest + 192, v3);
					Avx512F.Store(dest + 256, v4);
					Avx512F.Store(dest + 320, v5);
					Avx512F.Store(dest + 384, v6);
					Avx512F.Store(dest + 448, v7);
					
					src += 512;
					dest += 512;
					size -= 512;
				}
			}
			// AVX2: 256-bit vectors for large buffers
			else if (isAligned64 && size >= LARGE_BUFFER_THRESHOLD && Avx2.IsSupported)
			{
				// Large buffer with good alignment - use AVX2 streaming stores to bypass cache
				// This is optimal for very large transfers that would pollute the cache
				while (size >= 256)
				{
					// Prefetch next cache line
					Sse.Prefetch0(src + 512);
					
					// Load 8x 256-bit vectors (256 bytes total)
					var c0 = Avx.LoadVector256(src);
					var c1 = Avx.LoadVector256(src + 32);
					var c2 = Avx.LoadVector256(src + 64);
					var c3 = Avx.LoadVector256(src + 96);
					var c4 = Avx.LoadVector256(src + 128);
					var c5 = Avx.LoadVector256(src + 160);
					var c6 = Avx.LoadVector256(src + 192);
					var c7 = Avx.LoadVector256(src + 224);
					
					// Non-temporal stores (bypass cache)
					Avx.Store(dest, c0);
					Avx.Store(dest + 32, c1);
					Avx.Store(dest + 64, c2);
					Avx.Store(dest + 96, c3);
					Avx.Store(dest + 128, c4);
					Avx.Store(dest + 160, c5);
					Avx.Store(dest + 192, c6);
					Avx.Store(dest + 224, c7);
					
					src += 256;
					dest += 256;
					size -= 256;
				}
			}
			// AVX2: Regular stores for small/medium buffers
			else if (isAligned32 && size < SMALL_BUFFER_THRESHOLD && Avx2.IsSupported)
			{
				// Small/medium buffer with good alignment - use regular AVX2 stores
				while (size >= 128)
				{
					var c0 = Avx.LoadVector256(src);
					var c1 = Avx.LoadVector256(src + 32);
					var c2 = Avx.LoadVector256(src + 64);
					var c3 = Avx.LoadVector256(src + 96);
					
					Avx.Store(dest, c0);
					Avx.Store(dest + 32, c1);
					Avx.Store(dest + 64, c2);
					Avx.Store(dest + 96, c3);
					
					src += 128;
					dest += 128;
					size -= 128;
				}
			}
			// ARM NEON: 128-bit vectors
			else if (isAligned16 && AdvSimd.IsSupported && size >= 64)
			{
				// ARM NEON path - process 64 bytes at a time
				while (size >= 64)
				{
					var v0 = AdvSimd.LoadVector128(src);
					var v1 = AdvSimd.LoadVector128(src + 16);
					var v2 = AdvSimd.LoadVector128(src + 32);
					var v3 = AdvSimd.LoadVector128(src + 48);
					
					AdvSimd.Store(dest, v0);
					AdvSimd.Store(dest + 16, v1);
					AdvSimd.Store(dest + 32, v2);
					AdvSimd.Store(dest + 48, v3);
					
					src += 64;
					dest += 64;
					size -= 64;
				}
			}
			// System.Numerics.Vector: Cross-platform hardware-accelerated vectors
			else if (System.Numerics.Vector.IsHardwareAccelerated && size >= System.Numerics.Vector<byte>.Count * 4)
			{
				var vectorSize = System.Numerics.Vector<byte>.Count;
				var srcSpan = new Span<byte>(src, size);
				var destSpan = new Span<byte>(dest, size);
				
				// Process 4 vectors at a time
				var stride = vectorSize * 4;
				var offset = 0;
				
				while (offset + stride <= size)
				{
					var v0 = new System.Numerics.Vector<byte>(srcSpan.Slice(offset));
					var v1 = new System.Numerics.Vector<byte>(srcSpan.Slice(offset + vectorSize));
					var v2 = new System.Numerics.Vector<byte>(srcSpan.Slice(offset + vectorSize * 2));
					var v3 = new System.Numerics.Vector<byte>(srcSpan.Slice(offset + vectorSize * 3));
					
					v0.CopyTo(destSpan.Slice(offset));
					v1.CopyTo(destSpan.Slice(offset + vectorSize));
					v2.CopyTo(destSpan.Slice(offset + vectorSize * 2));
					v3.CopyTo(destSpan.Slice(offset + vectorSize * 3));
					
					offset += stride;
				}
				
				src += offset;
				dest += offset;
				size -= offset;
			}
			
			// Handle remainder with standard copy
			if (size > 0)
			{
				new Span<byte>(src, size).CopyTo(new Span<byte>(dest, size));
			}
		}

		#endregion
	}
}
