using Microsoft.Extensions.Logging;
using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Win16;

/// <summary>
/// Win16 KEYBOARD module thunking layer - maps to USER32.DLL.
/// Provides 16-bit to 32-bit thunking for keyboard-related functions.
/// </summary>
internal class Win16KeyboardModule : Win16ThunkingLayer, IWin32ModuleUnsafe
{
	public Win16KeyboardModule(IWin32ModuleUnsafe user32Module, ILogger logger)
		: base(user32Module, logger)
	{
	}

	public string Name => "KEYBOARD";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		return TryInvokeWin16(export, cpu, memory, out returnValue);
	}

	public override bool TryInvokeWin16(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var exportUpper = export.ToUpperInvariant();

		switch (exportUpper)
		{
			// Keyboard functions - forward to USER32
			case "GETKEYSTATE":
			case "GETASYNCKEYSTATE":
			case "GETKEYBOARDSTATE":
			case "SETKEYBOARDSTATE":
			case "GETKEYBOARDTYPE":
			case "MAPVIRTUALKEY":
			case "OEMKEYSCAN":
			case "VKKEYSCAN":
			case "ENABLEKEYBOARD":
				LogWin16Call(export, "forwarding to USER32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			default:
				Logger.LogWarning("[Win16 Thunk] Unknown Win16 KEYBOARD function: {Export}", export);
				return false;
		}
	}
}

/// <summary>
/// Win16 SYSTEM module thunking layer - maps to KERNEL32.DLL.
/// Provides 16-bit to 32-bit thunking for system-related functions.
/// </summary>
internal class Win16SystemModule : Win16ThunkingLayer, IWin32ModuleUnsafe
{
	public Win16SystemModule(IWin32ModuleUnsafe kernel32Module, ILogger logger)
		: base(kernel32Module, logger)
	{
	}

	public string Name => "SYSTEM";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		return TryInvokeWin16(export, cpu, memory, out returnValue);
	}

	public override bool TryInvokeWin16(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var exportUpper = export.ToUpperInvariant();

		switch (exportUpper)
		{
			// System timer and configuration functions - forward to KERNEL32
			case "GETTICKCOUNT":
			case "GETFREESPACE":
			case "GETSYSTEMTIME":
			case "SETSYSTEMTIME":
			case "GETLOCALTIME":
			case "SETLOCALTIME":
			case "GETCURRENTTIME":
			case "GETTIMERDESCRIPTION":
				LogWin16Call(export, "forwarding to KERNEL32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			default:
				Logger.LogWarning("[Win16 Thunk] Unknown Win16 SYSTEM function: {Export}", export);
				return false;
		}
	}
}

/// <summary>
/// Win16 SOUND module thunking layer - maps to WINMM.DLL.
/// Provides 16-bit to 32-bit thunking for sound/multimedia functions.
/// </summary>
internal class Win16SoundModule : Win16ThunkingLayer, IWin32ModuleUnsafe
{
	public Win16SoundModule(IWin32ModuleUnsafe winmmModule, ILogger logger)
		: base(winmmModule, logger)
	{
	}

	public string Name => "SOUND";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		return TryInvokeWin16(export, cpu, memory, out returnValue);
	}

	public override bool TryInvokeWin16(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var exportUpper = export.ToUpperInvariant();

		switch (exportUpper)
		{
			// Sound functions - forward to WINMM
			case "SNDPLAYSOUND":
			case "SNDPLAYSONDA":
			case "MESSAGEBEEP":
			case "OPENDRIVER":
			case "CLOSEDRIVER":
			case "SENDDRIVER":
			case "GETDRIVERNAME":
			case "GETDRIVERNAMEA":
			case "GETDRIVERINFO":
			case "GETDRIVERMODULEHANDLE":
				LogWin16Call(export, "forwarding to WINMM");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			default:
				Logger.LogWarning("[Win16 Thunk] Unknown Win16 SOUND function: {Export}", export);
				return false;
		}
	}
}
