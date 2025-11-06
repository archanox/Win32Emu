namespace Win32Emu.Win32.Messaging
{
	/// <summary>
	/// Base class for Win32 messages
	/// </summary>
	public record Win32Message(uint Hwnd, uint Message, uint WParam, uint LParam) : IMessage;
}