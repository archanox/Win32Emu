namespace Win32Emu
{
	public class DisplayUpdateInfo
	{
		public required byte[] FrameBuffer { get; init; }
		public int Width { get; init; }
		public int Height { get; init; }
		public int Stride { get; init; }
		/// <summary>
		/// Optional window handle to target specific window for rendering.
		/// If IntPtr.Zero, renders to the default/main display.
		/// </summary>
		public IntPtr TargetWindowHandle { get; init; }
	}
}