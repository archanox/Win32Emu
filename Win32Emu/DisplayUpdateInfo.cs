namespace Win32Emu
{
	public class DisplayUpdateInfo
	{
		public required byte[] FrameBuffer { get; init; }
		public int Width { get; init; }
		public int Height { get; init; }
		public int Stride { get; init; }
	}
}