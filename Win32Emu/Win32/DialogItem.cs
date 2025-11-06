namespace Win32Emu.Win32
{
	/// <summary>
	/// Represents a dialog item (control) in a dialog template.
	/// </summary>
	public class DialogItem
	{
		public bool IsExtended { get; set; }
		public uint HelpId { get; set; }
		public uint Style { get; set; }
		public uint ExtendedStyle { get; set; }
		public short X { get; set; }
		public short Y { get; set; }
		public short Width { get; set; }
		public short Height { get; set; }
		public ushort Id { get; set; }
		public string WindowClass { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public byte[]? CreationData { get; set; }
	}
}