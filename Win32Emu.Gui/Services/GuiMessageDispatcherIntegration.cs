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
		_env.MessageDispatcher.RegisterAsyncHandler(WM.PAINT, async (msg, ct) =>
		{
			await Task.Run(() =>
			{
				// Paint handling can be done in background
				LogDebug($"Async paint handler for window 0x{msg.Hwnd:X8}");
			}, ct);
			return 0;
		});

		// Register async handler for WM_CLOSE messages
		_env.MessageDispatcher.RegisterAsyncHandler(WM.CLOSE, async (msg, ct) =>
		{
			await Task.Run(() =>
			{
				LogDebug($"Async close handler for window 0x{msg.Hwnd:X8}");
				_env.DestroyWindow(msg.Hwnd);
			}, ct);
			return 0;
		});

		// Register async handler for WM_COMMAND messages
		_env.MessageDispatcher.RegisterAsyncHandler(WM.COMMAND, async (msg, ct) =>
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

		// Register async handler for mouse events
		_env.MessageDispatcher.RegisterAsyncHandler(WM.LBUTTONDOWN, async (msg, ct) =>
		{
			if (msg is LButtonDownMessage mouseMsg)
			{
				await Task.Run(() =>
				{
					LogDebug($"Async mouse down at ({mouseMsg.X}, {mouseMsg.Y}) on window 0x{msg.Hwnd:X8}");
				}, ct);
			}
			return 0;
		});

		_env.MessageDispatcher.RegisterAsyncHandler(WM.LBUTTONUP, async (msg, ct) =>
		{
			if (msg is LButtonUpMessage mouseMsg)
			{
				await Task.Run(() =>
				{
					LogDebug($"Async mouse up at ({mouseMsg.X}, {mouseMsg.Y}) on window 0x{msg.Hwnd:X8}");
				}, ct);
			}
			return 0;
		});

		// Register async handler for keyboard events
		_env.MessageDispatcher.RegisterAsyncHandler(WM.KEYDOWN, async (msg, ct) =>
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

		LogInfo("Registered default async message handlers for GUI integration");
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
