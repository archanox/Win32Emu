using Microsoft.Extensions.Logging;
using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Win16;

/// <summary>
/// Win16 USER module thunking layer - maps to USER32.DLL.
/// Provides 16-bit to 32-bit thunking for common USER functions.
/// </summary>
/// <remarks>
/// This is a simplified thunking layer that handles common Win16 USER functions
/// where parameter sizes and semantics are compatible with Win32 equivalents.
/// Window handles (HWND) and device context handles (HDC) are often 16-bit in Win16
/// but are extended to 32-bit in Win32.
/// </remarks>
internal class Win16UserModule : Win16ThunkingLayer, IWin32ModuleUnsafe
{
	public Win16UserModule(IWin32ModuleUnsafe user32Module, ILogger logger)
		: base(user32Module, logger)
	{
	}

	public string Name => "USER";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		return TryInvokeWin16(export, cpu, memory, out returnValue);
	}

	public override bool TryInvokeWin16(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var exportUpper = export.ToUpperInvariant();

		// For most Win16 USER functions, we can forward directly to the Win32 equivalent
		// Handle parameters are typically compatible (16-bit handles work as 32-bit)
		switch (exportUpper)
		{
			// Window management functions
			case "CREATEWINDOW":
			case "CREATEWINDOWA":
			case "CREATEWINDOWEX":
			case "CREATEWINDOWEXA":
			case "DESTROYWINDOW":
			case "SHOWWINDOW":
			case "UPDATEWINDOW":
			case "SETWINDOWPOS":
			case "MOVEWINDOW":
			case "GETWINDOWRECT":
			case "GETCLIENTRECT":
			case "ADJUSTWINDOWRECT":
			case "ADJUSTWINDOWRECTEX":
			case "SETWINDOWTEXT":
			case "SETWINDOWTEXTA":
			case "GETWINDOWTEXT":
			case "GETWINDOWTEXTA":
			case "GETWINDOWTEXTLENGTH":
			case "GETWINDOWTEXTLENGTHA":
			case "GETWINDOWLONG":
			case "SETWINDOWLONG":
			case "GETCLASSNAME":
			case "GETCLASSNAMEA":
			case "GETCLASSINFO":
			case "GETCLASSINFOA":
				LogWin16Call(export, "forwarding to USER32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			// Message functions
			case "GETMESSAGE":
			case "GETMESSAGEA":
			case "PEEKMESSAGE":
			case "PEEKMESSAGEA":
			case "TRANSLATEMESSAGE":
			case "DISPATCHMESSAGE":
			case "DISPATCHMESSAGEA":
			case "POSTMESSAGE":
			case "POSTMESSAGEA":
			case "SENDMESSAGE":
			case "SENDMESSAGEA":
			case "POSTQUITMESSAGE":
			case "WAITMESSAGE":
				LogWin16Call(export, "forwarding to USER32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			// Dialog functions
			case "DIALOGBOX":
			case "DIALOGBOXA":
			case "DIALOGBOXPARAM":
			case "DIALOGBOXPARAMA":
			case "DIALOGBOXINDIRECT":
			case "DIALOGBOXINDIRECTA":
			case "DIALOGBOXINDIRECTPARAM":
			case "DIALOGBOXINDIRECTPARAMA":
			case "ENDDIALOG":
			case "GETDLGITEM":
			case "GETDLGITEMTEXT":
			case "GETDLGITEMTEXTA":
			case "SETDLGITEMTEXT":
			case "SETDLGITEMTEXTA":
			case "GETDLGITEMINT":
			case "SETDLGITEMINT":
			case "CHECKDLGBUTTON":
			case "ISDLGBUTTONCHECKED":
				LogWin16Call(export, "forwarding to USER32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			// Menu functions
			case "CREATEMENU":
			case "CREATEPOPUPMENU":
			case "DESTROYMENU":
			case "APPENDMENU":
			case "APPENDMENUA":
			case "INSERTMENU":
			case "INSERTMENUA":
			case "DELETEMENU":
			case "MODIFYMENU":
			case "MODIFYMENUA":
			case "CHECKMENUITEM":
			case "ENABLEMENUITEM":
			case "SETMENU":
			case "GETMENU":
			case "DRAWMENUBAR":
				LogWin16Call(export, "forwarding to USER32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			// GDI/DC functions (in USER for Win16)
			case "GETDC":
			case "RELEASEDC":
			case "GETWINDOWDC":
			case "BEGINPAINT":
			case "ENDPAINT":
			case "INVALIDATERECT":
			case "VALIDATERECT":
			case "INVALIDATERGN":
			case "VALIDATERGN":
				LogWin16Call(export, "forwarding to USER32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			// Resource functions
			case "LOADSTRING":
			case "LOADSTRINGA":
			case "LOADICON":
			case "LOADICONA":
			case "LOADCURSOR":
			case "LOADCURSORA":
			case "LOADBITMAP":
			case "LOADBITMAPA":
			case "LOADMENU":
			case "LOADMENUA":
			case "LOADDIALOG":
			case "LOADACCELERATORS":
			case "LOADACCELERATORSA":
				LogWin16Call(export, "forwarding to USER32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			// Keyboard/mouse input functions
			case "GETASYNCKEYSTATE":
			case "GETKEYSTATE":
			case "GETKEYBOARDSTATE":
			case "SETKEYBOARDSTATE":
			case "GETCURSORPOS":
			case "SETCURSORPOS":
			case "SHOWCURSOR":
			case "SETCURSOR":
			case "GETCURSOR":
			case "CLIPCURSOR":
			case "GETCAPTURE":
			case "SETCAPTURE":
			case "RELEASECAPTURE":
				LogWin16Call(export, "forwarding to USER32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			// Timer functions
			case "SETTIMER":
			case "KILLTIMER":
				LogWin16Call(export, "forwarding to USER32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			// Miscellaneous functions
			case "MESSAGEBOX":
			case "MESSAGEBOXA":
			case "MESSAGEBEEP":
			case "GETDESKTOPWINDOW":
			case "GETACTIVEWINDOW":
			case "SETACTIVEWINDOW":
			case "GETFOCUS":
			case "SETFOCUS":
			case "GETSYSTEMMETRICS":
			case "ISWINDOW":
			case "ISWINDOWVISIBLE":
			case "ISWINDOWENABLED":
			case "ENABLEWINDOW":
			case "GETPARENT":
			case "SETPARENT":
				LogWin16Call(export, "forwarding to USER32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			default:
				// Unknown Win16 USER function
				Logger.LogWarning("[Win16 Thunk] Unknown Win16 USER function: {Export}", export);
				return false;
		}
	}
}
