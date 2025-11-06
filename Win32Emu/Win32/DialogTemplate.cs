namespace Win32Emu.Win32
{
	/// <summary>
	/// Represents a parsed dialog template.
	/// </summary>
	public class DialogTemplate
	{
		public bool IsExtended { get; set; }
		public uint HelpId { get; set; }
		public uint Style { get; set; }
		public uint ExtendedStyle { get; set; }
		public ushort ItemCount { get; set; }
		public short X { get; set; }
		public short Y { get; set; }
		public short Width { get; set; }
		public short Height { get; set; }
		public string Menu { get; set; } = string.Empty;
		public string WindowClass { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public ushort FontSize { get; set; }
		public ushort FontWeight { get; set; }
		public byte FontItalic { get; set; }
		public byte FontCharset { get; set; }
		public string FontName { get; set; } = string.Empty;
		public List<DialogItem> Items { get; set; } = [];
	}
}