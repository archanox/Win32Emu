namespace Win32Emu.Rendering
{
	/// <summary>
	/// Types of UI events that can be generated
	/// </summary>
	public enum UIEventType
	{
		MouseMove,
		MouseButtonDown,
		MouseButtonUp,
		KeyDown,
		KeyUp,
		WindowResize,
		WindowClose,
		WindowActivate,
		WindowDeactivate
	}
}