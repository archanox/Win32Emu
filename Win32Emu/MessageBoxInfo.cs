namespace Win32Emu
{
	public class MessageBoxInfo
	{
		public uint ParentHandle { get; init; }
		public required string Text { get; init; }
		public required string Caption { get; init; }
		public uint Type { get; init; }
	}
}