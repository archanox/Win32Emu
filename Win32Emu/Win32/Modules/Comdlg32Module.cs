using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules;

/// <summary>
/// COMDLG32.DLL module - provides common dialog box functionality.
/// </summary>
public class Comdlg32Module : IWin32ModuleUnsafe
{
	private readonly ProcessEnvironment _env;
	private readonly uint _imageBase;
	private readonly PeImageLoader? _peLoader;
	private readonly ILogger _logger;

	public Comdlg32Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
	{
		_env = env;
		_imageBase = imageBase;
		_peLoader = peLoader;
		_logger = logger ?? NullLogger.Instance;
	}

	public string Name => "COMDLG32.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "GETOPENFILENAMEA":
				returnValue = GetOpenFileNameA(a.UInt32(0));
				return true;

			case "GETSAVEFILENAMEA":
				returnValue = GetSaveFileNameA(a.UInt32(0));
				return true;
			case "GETFILETITLEA":
				returnValue = GetFileTitleA(a.LpcStr(0), a.LpStr(1), a.UInt32(2));
				return true;
			case "PAGESETUPDLGA":
				returnValue = PageSetupDlgA(a.UInt32(0));
				return true;


case "PRINTDLGA":
returnValue = PrintDlgA(a.UInt32(0));
return true;

			default:
				_logger.LogInformation("[Comdlg32] Unimplemented export: {Export}", export);
				return false;
		}
	}

	/// <summary>
	/// Displays an Open dialog box that lets the user specify the drive, directory, and the name of a file or set of files to be opened.
	/// BOOL GetOpenFileNameA(
	///   LPOPENFILENAMEA lpofn
	/// );
	/// </summary>
	[DllModuleExport(4)]
	private uint GetOpenFileNameA(uint lpofn)
	{
		_logger.LogInformation("[Comdlg32] GetOpenFileNameA(lpofn=0x{Lpofn:X8})", lpofn);

		if (lpofn == 0)
		{
			return 0; // FALSE - dialog cancelled
		}

		// OPENFILENAME structure fields (simplified):
		// DWORD lStructSize;
		// HWND hwndOwner;
		// HINSTANCE hInstance;
		// LPCSTR lpstrFilter;
		// LPSTR lpstrCustomFilter;
		// DWORD nMaxCustFilter;
		// DWORD nFilterIndex;
		// LPSTR lpstrFile;
		// DWORD nMaxFile;
		// ... more fields

		// For now, just return FALSE to indicate user cancelled
		// A full implementation would show a file dialog
		_logger.LogInformation("[Comdlg32] GetOpenFileNameA: Dialog cancelled (stub)");
		return 0; // FALSE
	}

	/// <summary>
	/// Displays a Save dialog box that lets the user specify the drive, directory, and name of a file to save.
	/// BOOL GetSaveFileNameA(
	///   LPOPENFILENAMEA lpofn
	/// );
	/// </summary>
	[DllModuleExport(4)]
	private uint GetSaveFileNameA(uint lpofn)
	{
		_logger.LogInformation("[Comdlg32] GetSaveFileNameA(lpofn=0x{Lpofn:X8})", lpofn);

		if (lpofn == 0)
		{
			return 0; // FALSE - dialog cancelled
		}

		// For now, just return FALSE to indicate user cancelled
		// A full implementation would show a save file dialog
		_logger.LogInformation("[Comdlg32] GetSaveFileNameA: Dialog cancelled (stub)");
		return 0; // FALSE
	}

	/// <summary>
	/// Retrieves the name of the specified file.
	/// short GetFileTitleA(LPCSTR lpszFile, LPSTR lpszTitle, WORD cchSize);
	/// </summary>
	[DllModuleExport(12)]
	private uint GetFileTitleA(in LpcStr lpszFile, in LpStr lpszTitle, uint cchSize)
	{
		var file = lpszFile.ToString() ?? string.Empty;
		_logger.LogInformation("[Comdlg32] GetFileTitleA(lpszFile=\"{File}\", cchSize={CchSize})", file, cchSize);
		
		// Extract just the file name without path
		var fileName = Path.GetFileNameWithoutExtension(file);
		
		if (lpszTitle.Address != 0 && cchSize > 0)
		{
			var toCopy = fileName.Length < cchSize ? fileName : fileName.Substring(0, (int)cchSize - 1);
			_env.WriteAnsiStringAt(lpszTitle.Address, toCopy);
			return 0; // Success
		}
		
		return 1; // Error
	}

	/// <summary>
	/// Displays a Page Setup dialog box.
	/// BOOL PageSetupDlgA(
	///   [in, out] LPPAGESETUPDLGA lppsd
	/// );
	/// </summary>
	[DllModuleExport(4)]
	private uint PageSetupDlgA(uint lppsd)
	{
		_logger.LogInformation("[Comdlg32] PageSetupDlgA(lppsd=0x{Lppsd:X8})", lppsd);

		if (lppsd == 0)
		{
			return 0; // FALSE - dialog cancelled
		}

		// PAGESETUPDLG structure fields (simplified):
		// DWORD lStructSize;
		// HWND hwndOwner;
		// HGLOBAL hDevMode;
		// HGLOBAL hDevNames;
		// DWORD Flags;
		// POINT ptPaperSize;
		// RECT rtMinMargin;
		// RECT rtMargin;
		// ... more fields

		// For now, just return FALSE to indicate user cancelled
		// A full implementation would show a page setup dialog
		_logger.LogInformation("[Comdlg32] PageSetupDlgA: Dialog cancelled (stub)");
		return 0; // FALSE
	}

/// <summary>
/// Displays a Print dialog box.
/// </summary>
[DllModuleExport(4, IsStub = true)]
private uint PrintDlgA(uint lppd)
{
_logger.LogInformation("[Comdlg32] PrintDlgA(lppd=0x{Lppd:X8})", lppd);
return 0; // User cancelled
}

}
