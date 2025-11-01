using Win32Emu.Win32;
using Win32Emu.Win32.Messaging;

namespace Win32Emu.Gui.Services;

/// <summary>
/// Integrates MessageDispatcher with GUI for event-driven message handling
/// </summary>
public class GuiMessageDispatcherIntegration
{
	private readonly ProcessEnvironment _env;
	private readonly IGuiEmulatorHost _guiHost;

	public GuiMessageDispatcherIntegration(ProcessEnvironment env, IGuiEmulatorHost guiHost)
	{
		_env = env;
		_guiHost = guiHost;
	}

	/// <summary>
	/// Register default async handlers for GUI events
	/// </summary>
	public void RegisterDefaultHandlers()
	{
		// Register async handler for WM_PAINT messages
		_env.MessageDispatcher.RegisterHandler((uint)WM.PAINT, async (msg, ct) =>
		{
			await Task.Run(() =>
			{
				LogDebug($"Async paint handler for window 0x{msg.Hwnd:X8}");
			}, ct);
			return 0;
		});

		// Register async handler for WM_CLOSE messages
		_env.MessageDispatcher.RegisterHandler((uint)WM.CLOSE, async (msg, ct) =>
		{
			await Task.Run(() =>
			{
				LogDebug($"Async close handler for window 0x{msg.Hwnd:X8}");
				_env.DestroyWindow(msg.Hwnd);
			}, ct);
			return 0;
		});

		// Register async handler for WM_COMMAND messages
		_env.MessageDispatcher.RegisterHandler((uint)WM.COMMAND, async (msg, ct) =>
		{
			if (msg is CommandMessage cmdMsg)
			{
				await Task.Run(() =>
				{
					LogDebug($"Async command handler: ControlId={cmdMsg.ControlId}, NotificationCode={cmdMsg.NotificationCode}");
				}, ct);
			}
			return 0;
		});

		// Register async handlers for mouse events
		_env.MessageDispatcher.RegisterHandler((uint)WM.LBUTTONDOWN, async (msg, ct) =>
		{
			if (msg is LButtonDownMessage mouseMsg)
			{
				await Task.Run(() =>
				{
					LogDebug($"Async left mouse down at ({mouseMsg.X}, {mouseMsg.Y}) on window 0x{msg.Hwnd:X8}");
				}, ct);
			}
			return 0;
		});

		_env.MessageDispatcher.RegisterHandler((uint)WM.LBUTTONUP, async (msg, ct) =>
		{
			if (msg is LButtonUpMessage mouseMsg)
			{
				await Task.Run(() =>
				{
					LogDebug($"Async left mouse up at ({mouseMsg.X}, {mouseMsg.Y}) on window 0x{msg.Hwnd:X8}");
				}, ct);
			}
			return 0;
		});

		_env.MessageDispatcher.RegisterHandler((uint)WM.RBUTTONDOWN, async (msg, ct) =>
		{
			if (msg is RButtonDownMessage mouseMsg)
			{
				await Task.Run(() =>
				{
					LogDebug($"Async right mouse down at ({mouseMsg.X}, {mouseMsg.Y}) on window 0x{msg.Hwnd:X8}");
				}, ct);
			}
			return 0;
		});

		_env.MessageDispatcher.RegisterHandler((uint)WM.RBUTTONUP, async (msg, ct) =>
		{
			if (msg is RButtonUpMessage mouseMsg)
			{
				await Task.Run(() =>
				{
					LogDebug($"Async right mouse up at ({mouseMsg.X}, {mouseMsg.Y}) on window 0x{msg.Hwnd:X8}");
				}, ct);
			}
			return 0;
		});

		_env.MessageDispatcher.RegisterHandler((uint)WM.MOUSEMOVE, async (msg, ct) =>
		{
			if (msg is MouseMoveMessage mouseMsg)
			{
				await Task.Run(() =>
				{
					LogDebug($"Async mouse move at ({mouseMsg.X}, {mouseMsg.Y}) on window 0x{msg.Hwnd:X8}");
				}, ct);
			}
			return 0;
		});

		// Register async handlers for keyboard events
		_env.MessageDispatcher.RegisterHandler((uint)WM.KEYDOWN, async (msg, ct) =>
		{
			if (msg is KeyDownMessage keyMsg)
			{
				await Task.Run(() =>
				{
					LogDebug($"Async key down: VK={keyMsg.VirtualKeyCode} on window 0x{msg.Hwnd:X8}");
				}, ct);
			}
			return 0;
		});

		_env.MessageDispatcher.RegisterHandler((uint)WM.KEYUP, async (msg, ct) =>
		{
			if (msg is KeyUpMessage keyMsg)
			{
				await Task.Run(() =>
				{
					LogDebug($"Async key up: VK={keyMsg.VirtualKeyCode} on window 0x{msg.Hwnd:X8}");
				}, ct);
			}
			return 0;
		});

		_env.MessageDispatcher.RegisterHandler((uint)WM.CHAR, async (msg, ct) =>
		{
			if (msg is CharMessage charMsg)
			{
				await Task.Run(() =>
				{
					LogDebug($"Async char: code={charMsg.CharCode} on window 0x{msg.Hwnd:X8}");
				}, ct);
			}
			return 0;
		});

		// Register async handlers for window state changes
		_env.MessageDispatcher.RegisterHandler((uint)WM.MOVE, async (msg, ct) =>
		{
			if (msg is MoveMessage moveMsg)
			{
				await Task.Run(() =>
				{
					LogDebug($"Async window move to ({moveMsg.X}, {moveMsg.Y}) for window 0x{msg.Hwnd:X8}");
				}, ct);
			}
			return 0;
		});

		_env.MessageDispatcher.RegisterHandler((uint)WM.SIZE, async (msg, ct) =>
		{
			if (msg is SizeMessage sizeMsg)
			{
				await Task.Run(() =>
				{
					LogDebug($"Async window resize to {sizeMsg.Width}x{sizeMsg.Height} for window 0x{msg.Hwnd:X8}");
				}, ct);
			}
			return 0;
		});

		_env.MessageDispatcher.RegisterHandler((uint)WM.ACTIVATE, async (msg, ct) =>
		{
			if (msg is ActivateMessage activateMsg)
			{
				await Task.Run(() =>
				{
					LogDebug($"Async window activate: flag={activateMsg.ActiveFlag} for window 0x{msg.Hwnd:X8}");
				}, ct);
			}
			return 0;
		});

		LogInfo("Registered default async message handlers for GUI integration (PAINT, CLOSE, COMMAND, MOUSE, KEYBOARD, WINDOW STATE)");
	}

	/// <summary>
	/// Post a message and dispatch it asynchronously through the MessageDispatcher
	/// </summary>
	public async Task<uint> PostMessageAsync(uint hwnd, uint message, uint wParam, uint lParam, CancellationToken cancellationToken = default)
	{
		// Create typed message
		var msg = MessageFactory.CreateMessage(hwnd, message, wParam, lParam);

		// Post to Win32 message queue
		_env.PostMessage(hwnd, message, wParam, lParam);

		// Also dispatch through MessageDispatcher asynchronously
		if (_env.MessageDispatcher.HasHandlers(message))
		{
			return await _env.MessageDispatcher.DispatchAsync(msg, cancellationToken);
		}

		return 0;
	}

	/// <summary>
	/// Send a message and dispatch it asynchronously through the MessageDispatcher
	/// </summary>
	public async Task<uint> SendMessageAsync(uint hwnd, uint message, uint wParam, uint lParam, CancellationToken cancellationToken = default)
	{
		// Create typed message
		var msg = MessageFactory.CreateMessage(hwnd, message, wParam, lParam);

		// Dispatch through MessageDispatcher asynchronously
		if (_env.MessageDispatcher.HasHandlers(message))
		{
			return await _env.MessageDispatcher.DispatchAsync(msg, cancellationToken);
		}

		return 0;
	}

	private void LogInfo(string message)
	{
		_guiHost.OnDebugOutput($"[GuiMessageDispatcher] {message}", DebugLevel.Info);
	}

	private void LogDebug(string message)
	{
		_guiHost.OnDebugOutput($"[GuiMessageDispatcher] {message}", DebugLevel.Debug);
	}
}
