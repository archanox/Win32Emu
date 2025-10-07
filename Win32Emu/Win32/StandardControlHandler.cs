using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Win32;

/// <summary>
/// Handles window messages for standard Win32 controls (BUTTON, EDIT, etc.)
/// Routes messages to the Avalonia frontend via the host
/// </summary>
public class StandardControlHandler
{
	private readonly ProcessEnvironment _env;
	private readonly IEmulatorHost? _host;
	private readonly ILogger _logger;

	public StandardControlHandler(ProcessEnvironment env, IEmulatorHost? host = null, ILogger? logger = null)
	{
		_env = env;
		_host = host;
		_logger = logger ?? NullLogger.Instance;
	}

	/// <summary>
	/// Handle a message for a standard control
	/// </summary>
	public uint HandleMessage(uint hwnd, uint msg, uint wParam, uint lParam, string className)
	{
		_logger.LogDebug("[StandardControlHandler] HWND=0x{Hwnd:X8} Class='{ClassName}' MSG=0x{Msg:X4}", hwnd, msg, className);

		return className.ToUpperInvariant() switch
		{
			"BUTTON" => HandleButtonMessage(hwnd, msg, wParam, lParam),
			"EDIT" => HandleEditMessage(hwnd, msg, wParam, lParam),
			"STATIC" => HandleStaticMessage(hwnd, msg, wParam, lParam),
			"LISTBOX" => HandleListBoxMessage(hwnd, msg, wParam, lParam),
			"COMBOBOX" => HandleComboBoxMessage(hwnd, msg, wParam, lParam),
			"SCROLLBAR" => HandleScrollBarMessage(hwnd, msg, wParam, lParam),
			_ => 0 // Unknown control type
		};
	}

	private uint HandleButtonMessage(uint hwnd, uint msg, uint wParam, uint lParam)
	{
		switch (msg)
		{
			case 0x0001: // WM_CREATE
				_logger.LogDebug("[Button] WM_CREATE");
				return 0;

			case 0x000F: // WM_PAINT
				_logger.LogDebug("[Button] WM_PAINT");
				// Let Avalonia handle the painting
				return 0;

			case 0x0014: // WM_ERASEBKGND
				return 1; // Background erased

			case 0x00F5: // WM_NCPAINT
				return 0; // Non-client area painted

			case 0x0085: // WM_NCPAINT
				return 0;

			default:
				_logger.LogDebug("[Button] Unhandled message 0x{Msg:X4}", msg);
				return 0;
		}
	}

	private uint HandleEditMessage(uint hwnd, uint msg, uint wParam, uint lParam)
	{
		switch (msg)
		{
			case 0x0001: // WM_CREATE
				_logger.LogDebug("[Edit] WM_CREATE");
				return 0;

			case 0x000C: // WM_SETTEXT
				_logger.LogDebug("[Edit] WM_SETTEXT");
				// TODO: Notify Avalonia to update text
				return 1; // TRUE

			case 0x000D: // WM_GETTEXT
				_logger.LogDebug("[Edit] WM_GETTEXT");
				// TODO: Get text from Avalonia control
				return 0;

			case 0x000E: // WM_GETTEXTLENGTH
				_logger.LogDebug("[Edit] WM_GETTEXTLENGTH");
				// TODO: Get text length from Avalonia control
				return 0;

			case 0x000F: // WM_PAINT
				_logger.LogDebug("[Edit] WM_PAINT");
				return 0;

			default:
				_logger.LogDebug("[Edit] Unhandled message 0x{Msg:X4}", msg);
				return 0;
		}
	}

	private uint HandleStaticMessage(uint hwnd, uint msg, uint wParam, uint lParam)
	{
		switch (msg)
		{
			case 0x0001: // WM_CREATE
				_logger.LogDebug("[Static] WM_CREATE");
				return 0;

			case 0x000C: // WM_SETTEXT
				_logger.LogDebug("[Static] WM_SETTEXT");
				// TODO: Notify Avalonia to update text
				return 1; // TRUE

			case 0x000F: // WM_PAINT
				_logger.LogDebug("[Static] WM_PAINT");
				return 0;

			default:
				_logger.LogDebug("[Static] Unhandled message 0x{Msg:X4}", msg);
				return 0;
		}
	}

	private uint HandleListBoxMessage(uint hwnd, uint msg, uint wParam, uint lParam)
	{
		_logger.LogDebug("[ListBox] Message 0x{Msg:X4}", msg);
		return 0;
	}

	private uint HandleComboBoxMessage(uint hwnd, uint msg, uint wParam, uint lParam)
	{
		_logger.LogDebug("[ComboBox] Message 0x{Msg:X4}", msg);
		return 0;
	}

	private uint HandleScrollBarMessage(uint hwnd, uint msg, uint wParam, uint lParam)
	{
		_logger.LogDebug("[ScrollBar] Message 0x{Msg:X4}", msg);
		return 0;
	}

	/// <summary>
	/// Check if a class name is a standard control
	/// </summary>
	public static bool IsStandardControl(string className)
	{
		return className.ToUpperInvariant() switch
		{
			"BUTTON" or "EDIT" or "STATIC" or "LISTBOX" or "COMBOBOX" or "SCROLLBAR" => true,
			_ => false
		};
	}
}
