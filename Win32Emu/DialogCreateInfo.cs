namespace Win32Emu
{
	public class DialogCreateInfo
	{
		public required uint Handle { get; init; }
		public required Win32.DialogTemplate Template { get; init; }
		public uint ParentHandle { get; init; }
		public uint DialogProcAddress { get; init; }
		public uint InitParam { get; init; }
		public Dictionary<int, uint> ControlHandles { get; init; } = new();
	}
}