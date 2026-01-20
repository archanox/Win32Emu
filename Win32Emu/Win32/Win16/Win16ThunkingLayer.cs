using Microsoft.Extensions.Logging;
using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Win16;

/// <summary>
/// Base class for Win16 to Win32 thunking layers.
/// Handles calling convention conversion (PASCAL to STDCALL) and parameter size conversion (16-bit to 32-bit).
/// </summary>
/// <remarks>
/// Win16 PASCAL calling convention:
/// - Arguments pushed left-to-right (opposite of STDCALL)
/// - Callee cleans the stack (same as STDCALL)
/// - Many parameters are 16-bit (WORD, HWND16, etc.)
/// 
/// This thunking layer provides:
/// - Calling convention conversion
/// - Parameter size conversion where needed
/// - Forwarding to Win32 module implementations
/// </remarks>
internal abstract class Win16ThunkingLayer
{
	protected readonly IWin32ModuleUnsafe Win32Module;
	protected readonly ILogger Logger;

	protected Win16ThunkingLayer(IWin32ModuleUnsafe win32Module, ILogger logger)
	{
		Win32Module = win32Module;
		Logger = logger;
	}

	/// <summary>
	/// Try to invoke a Win16 API function, converting calling convention and parameters as needed.
	/// </summary>
	public abstract bool TryInvokeWin16(string export, ICpu cpu, VirtualMemory memory, out uint returnValue);

	/// <summary>
	/// Read a 16-bit value from stack at given offset (in bytes).
	/// </summary>
	protected ushort Read16FromStack(ICpu cpu, VirtualMemory memory, int offset)
	{
		var esp = cpu.GetRegister("ESP");
		return memory.Read16(esp + (uint)offset);
	}

	/// <summary>
	/// Read a 32-bit value from stack at given offset (in bytes).
	/// </summary>
	protected uint Read32FromStack(ICpu cpu, VirtualMemory memory, int offset)
	{
		var esp = cpu.GetRegister("ESP");
		return memory.Read32(esp + (uint)offset);
	}

	/// <summary>
	/// Convert a Win16 handle (16-bit) to Win32 handle (32-bit).
	/// For many handles, we just zero-extend. Specific handle types may need special handling.
	/// </summary>
	protected uint ConvertHandle16To32(ushort handle16)
	{
		// Simple zero-extension for most handles
		// Special handle types (like window handles, DC handles) may need mapping tables
		return handle16;
	}

	/// <summary>
	/// Convert a Win32 handle (32-bit) to Win16 handle (16-bit).
	/// For many handles, we just truncate. Specific handle types may need special handling.
	/// </summary>
	protected ushort ConvertHandle32To16(uint handle32)
	{
		// Simple truncation for most handles
		// Special handle types may need reverse mapping
		return (ushort)(handle32 & 0xFFFF);
	}

	/// <summary>
	/// Log Win16 API call for debugging.
	/// </summary>
	protected void LogWin16Call(string export, string details = "")
	{
		Logger.LogDebug("[Win16 Thunk] {Export}{Details}", export, string.IsNullOrEmpty(details) ? "" : $" - {details}");
	}

	/// <summary>
	/// Try to resolve an ordinal export to a function name.
	/// Subclasses should override this to provide module-specific ordinal mappings.
	/// </summary>
	/// <param name="ordinal">The ordinal number as a string</param>
	/// <param name="functionName">The resolved function name, if found</param>
	/// <returns>True if the ordinal was resolved, false otherwise</returns>
	protected virtual bool TryResolveOrdinal(string ordinal, out string functionName)
	{
		functionName = ordinal;
		return false;
	}

	/// <summary>
	/// Normalize the export name for lookup.
	/// If the export is an ordinal, try to resolve it to a function name.
	/// </summary>
	protected string NormalizeExport(string export)
	{
		// Check if the export is an ordinal (numeric string)
		if (uint.TryParse(export, out _))
		{
			if (TryResolveOrdinal(export, out var functionName))
			{
				Logger.LogDebug("[Win16 Thunk] Resolved ordinal {Ordinal} to {FunctionName}", export, functionName);
				return functionName.ToUpperInvariant();
			}
			Logger.LogDebug("[Win16 Thunk] Unknown ordinal: {Ordinal}", export);
		}
		return export.ToUpperInvariant();
	}
}
