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
			WM.CREATE => new CreateMessage(hwnd, wParam, lParam),
			WM.DESTROY => new DestroyMessage(hwnd),
			WM.PAINT => new PaintMessage(hwnd),
			WM.CLOSE => new CloseMessage(hwnd),
			WM.COMMAND => new CommandMessage(hwnd, wParam, lParam),
			WM.LBUTTONDOWN => new LButtonDownMessage(hwnd, wParam, lParam),
			WM.LBUTTONUP => new LButtonUpMessage(hwnd, wParam, lParam),
			WM.RBUTTONDOWN => new RButtonDownMessage(hwnd, wParam, lParam),
			WM.RBUTTONUP => new RButtonUpMessage(hwnd, wParam, lParam),
			WM.KEYDOWN => new KeyDownMessage(hwnd, wParam, lParam),
			WM.KEYUP => new KeyUpMessage(hwnd, wParam, lParam),
			WM.CHAR => new CharMessage(hwnd, wParam, lParam),
			WM.MOVE => new MoveMessage(hwnd, lParam),
			WM.SIZE => new SizeMessage(hwnd, wParam, lParam),
			WM.ACTIVATE => new ActivateMessage(hwnd, wParam, lParam),
			WM.MOUSEMOVE => new MouseMoveMessage(hwnd, wParam, lParam),
			WM.TIMER => new TimerMessage(hwnd, wParam, lParam),
			WM.ERASEBKGND => new EraseBackgroundMessage(hwnd, wParam),
			WM.QUIT => new QuitMessage(wParam),
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
