namespace Win32Emu.Win32.Messaging;

/// <summary>
/// Factory for creating typed message instances from raw Win32 message data
/// </summary>
public static class MessageFactory
{
	/// <summary>
	/// Create a typed message from raw Win32 message data
	/// </summary>
	/// <param name="hwnd">Window handle</param>
	/// <param name="message">Message identifier</param>
	/// <param name="wParam">Additional message info</param>
	/// <param name="lParam">Additional message info</param>
	/// <returns>Typed message instance</returns>
	public static IMessage CreateMessage(uint hwnd, uint message, uint wParam, uint lParam)
	{
		return message switch
		{
			(uint)WM.CREATE => new CreateMessage(hwnd, wParam, lParam),
			(uint)WM.DESTROY => new DestroyMessage(hwnd),
			(uint)WM.PAINT => new PaintMessage(hwnd),
			(uint)WM.CLOSE => new CloseMessage(hwnd),
			(uint)WM.COMMAND => new CommandMessage(hwnd, wParam, lParam),
			(uint)WM.LBUTTONDOWN => new LButtonDownMessage(hwnd, wParam, lParam),
			(uint)WM.LBUTTONUP => new LButtonUpMessage(hwnd, wParam, lParam),
			(uint)WM.RBUTTONDOWN => new RButtonDownMessage(hwnd, wParam, lParam),
			(uint)WM.RBUTTONUP => new RButtonUpMessage(hwnd, wParam, lParam),
			(uint)WM.KEYDOWN => new KeyDownMessage(hwnd, wParam, lParam),
			(uint)WM.KEYUP => new KeyUpMessage(hwnd, wParam, lParam),
			(uint)WM.CHAR => new CharMessage(hwnd, wParam, lParam),
			(uint)WM.MOVE => new MoveMessage(hwnd, lParam),
			(uint)WM.SIZE => new SizeMessage(hwnd, wParam, lParam),
			(uint)WM.ACTIVATE => new ActivateMessage(hwnd, wParam, lParam),
			(uint)WM.MOUSEMOVE => new MouseMoveMessage(hwnd, wParam, lParam),
			(uint)WM.TIMER => new TimerMessage(hwnd, wParam, lParam),
			(uint)WM.ERASEBKGND => new EraseBackgroundMessage(hwnd, wParam),
			(uint)WM.QUIT => new QuitMessage(wParam),
			_ => new Win32Message(hwnd, message, wParam, lParam)
		};
	}

	/// <summary>
	/// Create a message from a QueuedMessage
	/// </summary>
	/// <param name="queuedMessage">The queued message</param>
	/// <returns>Typed message instance</returns>
	public static IMessage CreateMessage(ProcessEnvironment.QueuedMessage queuedMessage)
	{
		return CreateMessage(queuedMessage.Hwnd, queuedMessage.Message, queuedMessage.WParam, queuedMessage.LParam);
	}
}
