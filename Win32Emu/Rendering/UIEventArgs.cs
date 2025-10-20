namespace Win32Emu.Rendering;

/// <summary>
/// Event arguments for UI input events (mouse, keyboard, window events)
/// </summary>
public class UIEventArgs : EventArgs
{
    public UIEventType EventType { get; set; }
    public uint WindowHandle { get; set; }
    public uint WParam { get; set; }
    public uint LParam { get; set; }
    public int MouseX { get; set; }
    public int MouseY { get; set; }
    public int KeyCode { get; set; }
    public bool IsPressed { get; set; }
}

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
