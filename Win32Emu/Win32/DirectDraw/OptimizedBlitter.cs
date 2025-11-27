using System;

namespace Win32Emu.Win32.DirectDraw
{
	/// <summary>
	/// Static facade for blitter operations - uses SafeBlitter by default.
	/// This maintains backwards compatibility with existing code that uses OptimizedBlitter.
	/// </summary>
	public static class OptimizedBlitter
	{
		/// <summary>
		/// The current blitter implementation.
		/// Defaults to SafeBlitter but can be replaced with an optimized implementation.
		/// </summary>
		public static IBlitter Current { get; set; } = SafeBlitter.Instance;

		/// <summary>
		/// Indicates whether WASM SIMD is supported on the current platform.
		/// </summary>
		public static bool IsWasmSimdSupported => SafeBlitter.IsWasmSimdSupported;

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
			=> Current.BltFast(dest, src, destPitch, srcPitch, width, height, bytesPerPixel);

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
			=> Current.BltWithSourceColorKey(dest, src, destPitch, srcPitch, width, height, bytesPerPixel, colorKeyLow, colorKeyHigh);

		/// <summary>
		/// Performs a stretch blit with optional mirroring and color key support.
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
			=> Current.BltStretchWithColorKey(dest, src, destX, destY, destWidth, destHeight, destPitch,
				srcX, srcY, srcWidth, srcHeight, srcPitch, bytesPerPixel, colorKeyLow, colorKeyHigh, mirrorUpDown, mirrorLeftRight);

		/// <summary>
		/// Clears a buffer with a specific value.
		/// </summary>
		public static void Clear(Span<byte> buffer, byte value)
			=> Current.Clear(buffer, value);

		/// <summary>
		/// Performs a blit operation where source and destination may overlap.
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
			=> Current.BltOverlapping(buffer, destX, destY, destWidth, destHeight, destPitch, srcX, srcY, srcPitch, bytesPerPixel);

		/// <summary>
		/// Gets a string describing the available SIMD capabilities.
		/// </summary>
		public static string GetSimdCapabilities()
			=> Current.GetSimdCapabilities();
	}
}
