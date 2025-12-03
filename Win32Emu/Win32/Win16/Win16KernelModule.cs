using Microsoft.Extensions.Logging;
using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Win16;

/// <summary>
/// Win16 KERNEL module thunking layer - maps to KERNEL32.DLL.
/// Provides 16-bit to 32-bit thunking for common KERNEL functions.
/// </summary>
/// <remarks>
/// This is a simplified thunking layer that handles common Win16 KERNEL functions
/// where parameter sizes and semantics are compatible with Win32 equivalents.
/// More complex functions may require additional parameter translation logic.
/// </remarks>
internal class Win16KernelModule : Win16ThunkingLayer, IWin32ModuleAsync
{
	public Win16KernelModule(IWin32ModuleUnsafe kernel32Module, ILogger logger)
		: base(kernel32Module, logger)
	{
	}

	public string Name => "KERNEL";

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
		var exportUpper = export.ToUpperInvariant();

		// For most Win16 KERNEL functions, we can forward directly to the Win32 equivalent
		// as parameter sizes are often compatible or handled by the Win32 implementation
		switch (exportUpper)
		{
			// Memory management functions - mostly compatible
			case "GLOBALALLOC":
			case "GLOBALFREE":
			case "GLOBALLOCK":
			case "GLOBALUNLOCK":
			case "GLOBALREALLOC":
			case "GLOBALSIZE":
			case "GLOBALHANDLE":
			case "LOCALALLOC":
			case "LOCALFREE":
			case "LOCALLOCK":
			case "LOCALUNLOCK":
			case "LOCALREALLOC":
			case "LOCALSIZE":
			case "LOCALHANDLE":
				LogWin16Call(export, "forwarding to KERNEL32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			// File I/O functions - mostly compatible
			case "OPENFILE":
			case "LCREAT":
			case "LOPEN":
			case "LCLOSE":
			case "LREAD":
			case "LWRITE":
			case "LSEEK":
			case "_HREAD":
			case "_HWRITE":
			case "_LCREAT":
			case "_LOPEN":
			case "_LCLOSE":
			case "_LREAD":
			case "_LWRITE":
			case "_LLSEEK":
				LogWin16Call(export, "forwarding to KERNEL32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			// String functions - compatible
			case "LSTRCPY":
			case "LSTRCPYA":
			case "LSTRCPYN":
			case "LSTRCPYNA":
			case "LSTRCAT":
			case "LSTRCATA":
			case "LSTRCMP":
			case "LSTRCMPA":
			case "LSTRCMPI":
			case "LSTRCMPIA":
			case "LSTRLEN":
			case "LSTRLENA":
				LogWin16Call(export, "forwarding to KERNEL32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			// Module/task functions - mostly compatible
			case "GETMODULEHANDLE":
			case "GETMODULEFILENAME":
			case "GETMODULEFILENAME16":
			case "GETPROCADDRESS":
			case "LOADLIBRARY":
			case "FREELIBRARY":
			case "GETCURRENTTASK":
			case "GETCURRENTPDB":
				LogWin16Call(export, "forwarding to KERNEL32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			// Version functions - compatible
			case "GETVERSION":
			case "GETVERSIONEX":
				LogWin16Call(export, "forwarding to KERNEL32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			// Miscellaneous functions - compatible
			case "GETWINFLAGS":
			case "GETTICKCOUNT":
			case "GETFREESPACE":
			case "EXITWINDOWS":
			case "EXITWINDOWSEXEC":
			case "OUTPUTDEBUGSTRING":
			case "FATALAPPEXIT":
			case "FATALEXIT":
				LogWin16Call(export, "forwarding to KERNEL32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			default:
				// Unknown Win16 KERNEL function
				Logger.LogWarning("[Win16 Thunk] Unknown Win16 KERNEL function: {Export}", export);
				return false;
		}
	}
}
