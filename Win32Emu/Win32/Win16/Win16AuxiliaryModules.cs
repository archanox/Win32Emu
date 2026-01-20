using Microsoft.Extensions.Logging;
using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Win16;

/// <summary>
/// Win16 KEYBOARD module thunking layer - maps to USER32.DLL.
/// Provides 16-bit to 32-bit thunking for keyboard-related functions.
/// </summary>
internal class Win16KeyboardModule : Win16ThunkingLayer, IWin32ModuleAsync
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

	public async Task<(bool success, uint returnValue)> TryInvokeAsync(string export, ICpu cpu, VirtualMemory memory, CancellationToken cancellationToken = default)
	{
		var success = TryInvokeUnsafe(export, cpu, memory, out var returnValue);
		return await Task.FromResult((success, returnValue));
	}

	public override bool TryInvokeWin16(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var exportUpper = NormalizeExport(export);

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
internal class Win16SystemModule : Win16ThunkingLayer, IWin32ModuleAsync
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

	public async Task<(bool success, uint returnValue)> TryInvokeAsync(string export, ICpu cpu, VirtualMemory memory, CancellationToken cancellationToken = default)
	{
		var success = TryInvokeUnsafe(export, cpu, memory, out var returnValue);
		return await Task.FromResult((success, returnValue));
	}

	public override bool TryInvokeWin16(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var exportUpper = NormalizeExport(export);

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
internal class Win16SoundModule : Win16ThunkingLayer, IWin32ModuleAsync
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

	public async Task<(bool success, uint returnValue)> TryInvokeAsync(string export, ICpu cpu, VirtualMemory memory, CancellationToken cancellationToken = default)
	{
		var success = TryInvokeUnsafe(export, cpu, memory, out var returnValue);
		return await Task.FromResult((success, returnValue));
	}

	public override bool TryInvokeWin16(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var exportUpper = NormalizeExport(export);

		switch (exportUpper)
		{
			// Sound functions - forward to WINMM
			case "SNDPLAYSOUND":
			case "SNDPLAYSONDA":
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

/// <summary>
/// Win16 SHELL module thunking layer - maps to SHELL32.DLL.
/// Provides 16-bit to 32-bit thunking for shell-related functions.
/// </summary>
internal class Win16ShellModule : Win16ThunkingLayer, IWin32ModuleAsync
{
	public Win16ShellModule(IWin32ModuleUnsafe shell32Module, ILogger logger)
		: base(shell32Module, logger)
	{
	}

	public string Name => "SHELL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		return TryInvokeWin16(export, cpu, memory, out returnValue);
	}

	public async Task<(bool success, uint returnValue)> TryInvokeAsync(string export, ICpu cpu, VirtualMemory memory, CancellationToken cancellationToken = default)
	{
		var success = TryInvokeUnsafe(export, cpu, memory, out var returnValue);
		return await Task.FromResult((success, returnValue));
	}

	/// <summary>
	/// Resolve Win16 SHELL ordinals to function names.
	/// Based on Windows 3.1 SHELL.DLL export ordinals.
	/// </summary>
	protected override bool TryResolveOrdinal(string ordinal, out string functionName)
	{
		functionName = ordinal switch
		{
			"1" => "RegOpenKey",
			"2" => "RegCreateKey",
			"3" => "RegCloseKey",
			"4" => "RegDeleteKey",
			"5" => "RegSetValue",
			"6" => "RegQueryValue",
			"7" => "RegEnumKey",
			"8" => "WinHelp",
			"9" => "DoEnvironmentSubst",
			"10" => "FindExecutable",
			"11" => "ShellAbout",
			"12" => "ShellExecute",
			"13" => "ExtractIcon",
			"14" => "DragAcceptFiles",
			"15" => "DragQueryFile",
			"16" => "DragFinish",
			"17" => "DragQueryPoint",
			"18" => "ExtractAssociatedIcon",
			"19" => "ShellHookProc",
			"20" => "ShellExecuteEx",
			"21" => "InternalExtractIconList",
			"22" => "AboutDlgProc",
			_ => ordinal
		};
		return functionName != ordinal;
	}

	public override bool TryInvokeWin16(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var exportUpper = NormalizeExport(export);

		switch (exportUpper)
		{
			// Shell functions - forward to SHELL32 (only ANSI 'A' variants are supported)
			// Win16 SHELL functions typically use ANSI strings, so we map them to the 'A' variants
			case "SHELLABOUT":
			case "SHELLABOUTA":
			case "SHELLEXECUTE":
			case "SHELLEXECUTEA":
			case "SHELLEXECUTEEX":
			case "SHELLEXECUTEEXA":
			case "DRAGFINISH":
			case "DRAGQUERYFILE":
			case "DRAGQUERYFILEA":
			case "EXTRACTICON":
			case "EXTRACTICONA":
			case "SHBROWSEFORFOLDER":
			case "SHBROWSEFORFOLDERA":
			case "SHCHANGENOTIFY":
			case "SHFILEOPERATION":
			case "SHFILEOPERATIONA":
			case "SHGETFILEINFO":
			case "SHGETFILEINFOA":
			case "SHGETMALLOC":
			case "SHGETPATHFROMIDLIST":
			case "SHGETPATHFROMIDLISTA":
			case "SHGETSPECIALFOLDERLOCATION":
			case "SHGETDESKTOPFOLDER":
				// Map generic names to ANSI variants for compatibility
				var mappedExport = exportUpper switch
				{
					"SHELLABOUT" => "SHELLABOUTA",
					"SHELLEXECUTE" => "SHELLEXECUTEA",
					"SHELLEXECUTEEX" => "SHELLEXECUTEEXA",
					"DRAGQUERYFILE" => "DRAGQUERYFILEA",
					"EXTRACTICON" => "EXTRACTICONA",
					"SHBROWSEFORFOLDER" => "SHBROWSEFORFOLDERA",
					"SHFILEOPERATION" => "SHFILEOPERATIONA",
					"SHGETFILEINFO" => "SHGETFILEINFOA",
					"SHGETPATHFROMIDLIST" => "SHGETPATHFROMIDLISTA",
					_ => exportUpper
				};
				LogWin16Call(export, $"forwarding to SHELL32 as {mappedExport}");
				return Win32Module.TryInvokeUnsafe(mappedExport, cpu, memory, out returnValue);
			
			// Win16-specific functions that don't have Win32 equivalents
			case "ABOUTDLGPROC":
			case "REGOPENKEYSTR":
			case "REGCREATEKEYSTR":
			case "REGCLOSEKEYSTR":
			case "REGDELETEKEYSTR":
			case "REGSETVALUESTR":
			case "REGQUERYVALUESTR":
			case "REGENUMKEYSTR":
			case "DOENVIRONMENTSUBST":
			case "FINDEXECUTABLE":
			case "EXTRACTASSOCIATEDICON":
			case "SHELLHOOKPROC":
			case "INTERNALEXTRACTICONLIST":
			case "WINHELP":
				// These are Win16-specific functions that may need special handling
				// For now, log as unimplemented
				Logger.LogWarning("[Win16 Thunk] Unimplemented Win16 SHELL function: {Export}", export);
				return false;

			default:
				Logger.LogWarning("[Win16 Thunk] Unknown Win16 SHELL function: {Export}", export);
				return false;
		}
	}
}
