namespace Win32Emu
{
	public class WindowCreateInfo
	{
		public required uint Handle { get; init; }
		public required string Title { get; init; }
		public int Width { get; init; }
		public int Height { get; init; }
		public int X { get; init; }
		public int Y { get; init; }
		public required string ClassName { get; init; }
		public uint Style { get; init; }
		public uint ExStyle { get; init; }
		public uint Parent { get; init; }
		public uint Menu { get; init; }
	}
}