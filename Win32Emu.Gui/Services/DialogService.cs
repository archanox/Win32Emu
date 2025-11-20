using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Threading;
using Win32Emu.Gui.Views;
using Win32Emu.Win32;
using Win32Emu.VirtualFileSystem;

namespace Win32Emu.Gui.Services;

/// <summary>
/// Service that coordinates Win32 dialogs with Avalonia UI.
/// Handles dialog creation, message routing, and async coordination.
/// </summary>
public class DialogService
{
	private readonly Dictionary<uint, DialogWindow> _activeDialogs = new();
	private readonly Dictionary<uint, Queue<DialogMessage>> _messageQueues = new();
	private readonly object _lock = new();

	/// <summary>
	/// Creates and shows a dialog from a Win32 dialog template.
	/// </summary>
	/// <param name="dialogHandle">Handle for the dialog (from Win32 emulator)</param>
	/// <param name="template">Parsed dialog template</param>
	/// <param name="parentWindow">Parent window for modal display</param>
	/// <param name="controlHandles">Optional dictionary mapping control IDs to window handles</param>
	/// <returns>Task that completes when dialog is closed, with the dialog result</returns>
	public async Task<int> ShowDialogAsync(uint dialogHandle, DialogTemplate template, Avalonia.Controls.Window? parentWindow = null, Dictionary<int, uint>? controlHandles = null)
	{
		DialogWindow? dialog = null;

		// Create dialog on UI thread
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			// Create message callback that queues messages for processing by emulator
			Action<uint, uint, uint, uint> messageCallback = (hwnd, msg, wParam, lParam) =>
			{
				// Queue message for processing by emulator
				EnqueueMessage(dialogHandle, new DialogMessage
				{
					Hwnd = hwnd,
					Message = msg,
					WParam = wParam,
					LParam = lParam
				});
			};
			
			// Create debug callback that logs to System.Diagnostics
			Action<string, DebugLevel> debugCallback = (message, level) =>
			{
				System.Diagnostics.Debug.WriteLine(message);
			};
			
			dialog = new DialogWindow(template, dialogHandle, controlHandles, messageCallback, debugCallback);

			lock (_lock)
			{
				_activeDialogs[dialogHandle] = dialog;
				_messageQueues[dialogHandle] = new Queue<DialogMessage>();
			}
		});

		if (dialog == null)
		{
			return 0;
		}

		// Show dialog and wait for result
		var result = await dialog.ShowDialog(parentWindow);

		// Clean up
		lock (_lock)
		{
			_activeDialogs.Remove(dialogHandle);
			_messageQueues.Remove(dialogHandle);
		}

		return result;
	}

	/// <summary>
	/// Ends a dialog with the specified result.
	/// </summary>
	public void EndDialog(uint dialogHandle, int result)
	{
		DialogWindow? dialog;
		lock (_lock)
		{
			if (!_activeDialogs.TryGetValue(dialogHandle, out dialog))
			{
				return;
			}
		}

		Dispatcher.UIThread.Post(() =>
		{
			dialog.EndDialog(result);
		});
	}

	/// <summary>
	/// Posts a message to a dialog.
	/// </summary>
	public void PostMessage(uint dialogHandle, uint message, uint wParam, uint lParam)
	{
		EnqueueMessage(dialogHandle, new DialogMessage
		{
			Hwnd = dialogHandle,
			Message = message,
			WParam = wParam,
			LParam = lParam
		});
	}

	/// <summary>
	/// Tries to get the next message from a dialog's message queue.
	/// </summary>
	public bool TryGetMessage(uint dialogHandle, out DialogMessage message)
	{
		lock (_lock)
		{
			if (_messageQueues.TryGetValue(dialogHandle, out var queue) && queue.Count > 0)
			{
				message = queue.Dequeue();
				return true;
			}
		}

		message = default;
		return false;
	}

	/// <summary>
	/// Gets a dialog window by its handle.
	/// </summary>
	public DialogWindow? GetDialog(uint dialogHandle)
	{
		lock (_lock)
		{
			_activeDialogs.TryGetValue(dialogHandle, out var dialog);
			return dialog;
		}
	}

	/// <summary>
	/// Checks if a dialog is active.
	/// </summary>
	public bool IsDialogActive(uint dialogHandle)
	{
		lock (_lock)
		{
			return _activeDialogs.ContainsKey(dialogHandle);
		}
	}

	/// <summary>
	/// Sends a message to a dialog control and waits for the result.
	/// </summary>
	public async Task<uint> SendMessageAsync(uint dialogHandle, ushort controlId, uint message, uint wParam, uint lParam)
	{
		DialogWindow? dialog;
		lock (_lock)
		{
			if (!_activeDialogs.TryGetValue(dialogHandle, out dialog))
			{
				return 0;
			}
		}

		// Execute on UI thread and wait for result
		return await Dispatcher.UIThread.InvokeAsync(() =>
		{
			var control = dialog.GetControlById(controlId);
			if (control == null)
			{
				return 0u;
			}

			// Handle common control messages
			const uint WM_GETTEXT = 0x000D;
			const uint WM_SETTEXT = 0x000C;
			const uint WM_ENABLE = 0x000A;

			if (message == WM_GETTEXT)
			{
				// Get text from control (simplified)
				return 0u;
			}
			else if (message == WM_SETTEXT)
			{
				// Set text on control (simplified)
				return 1u;
			}
			else if (message == WM_ENABLE)
			{
				control.IsEnabled = wParam != 0;
				return 1u;
			}

			return 0u;
		});
	}

	private void EnqueueMessage(uint dialogHandle, DialogMessage message)
	{
		lock (_lock)
		{
			if (_messageQueues.TryGetValue(dialogHandle, out var queue))
			{
				queue.Enqueue(message);
			}
		}
	}

	/// <summary>
	/// Shows a folder browser dialog for VFS navigation.
	/// </summary>
	/// <param name="vfs">Virtual file system to browse</param>
	/// <param name="title">Optional title for the dialog</param>
	/// <param name="rootPath">Optional root path to start browsing from</param>
	/// <param name="parentWindow">Parent window for modal display</param>
	/// <returns>Selected folder path, or null if cancelled</returns>
	public async Task<string?> ShowFolderBrowserAsync(IVirtualFileSystem? vfs, string? title = null, string? rootPath = null, Avalonia.Controls.Window? parentWindow = null)
	{
		FolderBrowserDialog? dialog = null;

		// Create dialog on UI thread
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			dialog = new FolderBrowserDialog(vfs, title, rootPath);
		});

		if (dialog == null)
		{
			return null;
		}

		// Show dialog and wait for result
		return await dialog.ShowDialogAsync(parentWindow);
	}
}

/// <summary>
/// Represents a Win32 window message for a dialog.
/// </summary>
public struct DialogMessage
{
	public uint Hwnd;
	public uint Message;
	public uint WParam;
	public uint LParam;
}
