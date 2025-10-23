using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules;

/// <summary>
/// WINSPOOL.DRV module - provides printer spooler functionality.
/// </summary>
public class WinspoolModule : IWin32ModuleUnsafe
{
	private readonly ProcessEnvironment _env;
	private readonly uint _imageBase;
	private readonly PeImageLoader? _peLoader;
	private readonly ILogger _logger;
	private uint _nextPrinterHandle = 0xA0000000;
	private readonly Dictionary<uint, PrinterData> _printers = new();

	public WinspoolModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
	{
		_env = env;
		_imageBase = imageBase;
		_peLoader = peLoader;
		_logger = logger ?? NullLogger.Instance;
	}

	public string Name => "WINSPOOL.DRV";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "OPENPRINTERA":
				returnValue = OpenPrinterA(a.LpcStr(0), a.UInt32(1), a.UInt32(2));
				return true;

			case "CLOSEPRINTER":
				returnValue = ClosePrinter(a.UInt32(0));
				return true;

			case "DOCUMENTPROPERTIESA":
				returnValue = DocumentPropertiesA(a.UInt32(0), a.UInt32(1), a.LpcStr(2), a.UInt32(3), a.UInt32(4), a.UInt32(5));
				return true;

			default:
				_logger.LogInformation("[Winspool] Unimplemented export: {Export}", export);
				return false;
		}
	}

	/// <summary>
	/// Retrieves a handle to the specified printer or print server or other types of handles in the print subsystem.
	/// BOOL OpenPrinterA(
	///   LPSTR   pPrinterName,
	///   LPHANDLE phPrinter,
	///   LPPRINTER_DEFAULTSA pDefault
	/// );
	/// </summary>
	[DllModuleExport(12)]
	private uint OpenPrinterA(in LpcStr pPrinterName, uint phPrinter, uint pDefault)
	{
		var printerName = pPrinterName.ToString() ?? "Default Printer";
		_logger.LogInformation("[Winspool] OpenPrinterA(pPrinterName=\"{PrinterName}\", phPrinter=0x{PhPrinter:X8}, pDefault=0x{PDefault:X8})",
			printerName, phPrinter, pDefault);

		// Create a pseudo-handle for the printer
		var handle = _nextPrinterHandle++;
		_printers[handle] = new PrinterData
		{
			Name = printerName,
			Handle = handle
		};

		// Write the handle to the output parameter
		if (phPrinter != 0)
		{
			_env.MemWrite32(phPrinter, handle);
		}

		return 1; // TRUE - success
	}

	/// <summary>
	/// Closes the specified printer handle.
	/// BOOL ClosePrinter(
	///   HANDLE hPrinter
	/// );
	/// </summary>
	[DllModuleExport(4)]
	private uint ClosePrinter(uint hPrinter)
	{
		_logger.LogInformation("[Winspool] ClosePrinter(hPrinter=0x{HPrinter:X8})", hPrinter);

		if (_printers.Remove(hPrinter))
		{
			return 1; // TRUE - success
		}

		return 0; // FALSE - invalid handle
	}

	/// <summary>
	/// Retrieves or modifies printer configuration information or displays a printer-configuration property sheet for the specified printer.
	/// LONG DocumentPropertiesA(
	///   HWND     hwnd,
	///   HANDLE   hPrinter,
	///   LPSTR    pDeviceName,
	///   PDEVMODE pDevModeOutput,
	///   PDEVMODE pDevModeInput,
	///   DWORD    fMode
	/// );
	/// </summary>
	[DllModuleExport(24)]
	private uint DocumentPropertiesA(uint hwnd, uint hPrinter, in LpcStr pDeviceName, uint pDevModeOutput, uint pDevModeInput, uint fMode)
	{
		var deviceName = pDeviceName.ToString() ?? string.Empty;
		_logger.LogInformation("[Winspool] DocumentPropertiesA(hwnd=0x{Hwnd:X8}, hPrinter=0x{HPrinter:X8}, pDeviceName=\"{DeviceName}\", pDevModeOutput=0x{PDevModeOutput:X8}, pDevModeInput=0x{PDevModeInput:X8}, fMode=0x{FMode:X})",
			hwnd, hPrinter, deviceName, pDevModeOutput, pDevModeInput, fMode);

		// Constants for fMode
		const uint DM_IN_BUFFER = 8;
		const uint DM_OUT_BUFFER = 2;

		// If fMode is 0, return size of DEVMODE structure
		if (fMode == 0)
		{
			// Return size of DEVMODEA structure (156 bytes for basic structure)
			return 156;
		}

		// For other modes, return success
		return 1; // IDOK
	}

	private class PrinterData
	{
		public string Name { get; set; } = string.Empty;
		public uint Handle { get; set; }
	}
}
