namespace Win32Emu.Win32.Messaging
{
	/// <summary>
	/// Represents a message type that can be handled
	/// </summary>
	public interface IMessage
	{
		/// <summary>
		/// The window handle this message is for
		/// </summary>
		uint Hwnd { get; }
	
		/// <summary>
		/// The message identifier
		/// </summary>
		uint Message { get; }
	
		/// <summary>
		/// Additional message-specific information
		/// </summary>
		uint WParam { get; }
	
		/// <summary>
		/// Additional message-specific information
		/// </summary>
		uint LParam { get; }
	}
}