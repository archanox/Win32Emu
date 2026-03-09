namespace Win32Emu.Rendering
{
	/// <summary>
	/// Helpers for converting packed pixel formats into RGBA byte output.
	/// </summary>
	public static class PixelConversion
	{
		/// <summary>
		/// Expands a single RGB565 pixel into four RGBA bytes using bit replication to restore
		/// full 8-bit channel intensity.
		/// </summary>
		/// <param name="pixel">The packed RGB565 pixel value.</param>
		/// <param name="rgbaData">The destination RGBA byte buffer.</param>
		/// <param name="destinationOffset">
		/// The starting index in <paramref name="rgbaData"/> where four bytes can be written.
		/// </param>
		public static void WriteRgb565ToRgba(ushort pixel, byte[] rgbaData, int destinationOffset)
		{
			ArgumentNullException.ThrowIfNull(rgbaData);
			ArgumentOutOfRangeException.ThrowIfNegative(destinationOffset);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(destinationOffset, rgbaData.Length - 4, nameof(destinationOffset));

			var r5 = (byte)((pixel >> 11) & 0x1F);
			var g6 = (byte)((pixel >> 5) & 0x3F);
			var b5 = (byte)(pixel & 0x1F);

			rgbaData[destinationOffset + 0] = (byte)((r5 << 3) | (r5 >> 2));
			rgbaData[destinationOffset + 1] = (byte)((g6 << 2) | (g6 >> 4));
			rgbaData[destinationOffset + 2] = (byte)((b5 << 3) | (b5 >> 2));
			rgbaData[destinationOffset + 3] = 255;
		}
	}
}
