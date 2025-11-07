using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics.Arm;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Win32.DirectDraw
{
	/// <summary>
	/// High-performance blitter optimized with SIMD intrinsics (SSE2 on x86/x64, Neon on ARM).
	/// Inspired by DDrawCompat's blitter implementation with color key support.
	/// </summary>
	public static class OptimizedBlitter
	{
		private static readonly ILogger _logger = NullLogger.Instance;

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

							// Mixed case: copy non-transparent bytes individually
							for (var i = 0; i < 16; i++)
							{
								var pixel = srcRow[x + i];
								if (pixel < colorKeyLow || pixel > colorKeyHigh)
								{
									dstRow[x + i] = pixel;
								}
							}
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

							// Mixed case: copy non-transparent pixels individually
							for (var i = 0; i < 8; i++)
							{
								var pixel = ((ushort*)(srcRow + (x + i) * 2))[0];
								if (pixel < colorKeyLow || pixel > colorKeyHigh)
								{
									((ushort*)(dstRow + (x + i) * 2))[0] = pixel;
								}
							}
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
			
			if (Sse2.IsSupported)
				caps.Append("SSE2 ");
			if (Avx2.IsSupported)
				caps.Append("AVX2 ");
			if (AdvSimd.IsSupported)
				caps.Append("NEON ");
			
			return caps.Length > 0 ? caps.ToString().TrimEnd() : "Scalar (no SIMD)";
		}
	}
}
