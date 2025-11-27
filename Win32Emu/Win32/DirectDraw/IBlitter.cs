using System;

namespace Win32Emu.Win32.DirectDraw
{
	/// <summary>
	/// Interface for blitter implementations.
	/// Allows platform-specific optimizations (e.g., SIMD on desktop, scalar on WASM).
	/// </summary>
	public interface IBlitter
	{
		/// <summary>
		/// Performs a fast blit operation without color key.
		/// </summary>
		void BltFast(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destPitch,
			int srcPitch,
			int width,
			int height,
			int bytesPerPixel);

		/// <summary>
		/// Performs a blit operation with source color key (transparency).
		/// </summary>
		void BltWithSourceColorKey(
			Span<byte> dest,
			ReadOnlySpan<byte> src,
			int destPitch,
			int srcPitch,
			int width,
			int height,
			int bytesPerPixel,
			uint colorKeyLow,
			uint colorKeyHigh);

		/// <summary>
		/// Performs a stretch blit with optional mirroring and color key support.
		/// </summary>
		void BltStretchWithColorKey(
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
			bool mirrorLeftRight = false);

		/// <summary>
		/// Clears a buffer with a specific value.
		/// </summary>
		void Clear(Span<byte> buffer, byte value);

		/// <summary>
		/// Performs a blit operation where source and destination may overlap.
		/// </summary>
		void BltOverlapping(
			Span<byte> buffer,
			int destX,
			int destY,
			int destWidth,
			int destHeight,
			int destPitch,
			int srcX,
			int srcY,
			int srcPitch,
			int bytesPerPixel);

		/// <summary>
		/// Gets a string describing the available SIMD capabilities.
		/// </summary>
		string GetSimdCapabilities();
	}
}
