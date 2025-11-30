using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules;

/// <summary>
/// COMDLG32.DLL module - provides common dialog box functionality.
/// </summary>
public class Comdlg32Module : IWin32ModuleAsync
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
	/// Async implementation for Win32 APIs that may require host interaction.
	/// Routes file dialog APIs through async paths to avoid blocking calls that fail on WASM.
	/// </summary>
	public async Task<(bool success, uint returnValue)> TryInvokeAsync(
		string export,
		ICpu cpu,
		VirtualMemory memory,
		CancellationToken cancellationToken = default)
	{
		var a = new StackArgs(cpu, memory);

		// Route file dialog APIs through async paths for WASM compatibility
		switch (export.ToUpperInvariant())
		{
			case "GETOPENFILENAMEA":
				return (true, await GetOpenFileNameAAsync(a.UInt32(0), cancellationToken).ConfigureAwait(false));
			case "GETSAVEFILENAMEA":
				return (true, await GetSaveFileNameAAsync(a.UInt32(0), cancellationToken).ConfigureAwait(false));
		}

		// For all other APIs, use synchronous implementation
		if (TryInvokeUnsafe(export, cpu, memory, out var syncReturnValue))
		{
			return (true, syncReturnValue);
		}

		// No async work performed; return failure immediately
		return (false, 0);
	}

	/// <summary>
	/// Displays an Open dialog box that lets the user specify the drive, directory, and the name of a file or set of files to be opened.
	/// BOOL GetOpenFileNameA(
	///   LPOPENFILENAMEA lpofn
	/// );
	/// </summary>
	[DllModuleExport(110, Version = "5.50.4134.100", IsStub = true)]
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
	[DllModuleExport(112, Version = "5.50.4134.100", IsStub = true)]
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

	#region Async File Dialog Implementations

	/// <summary>
	/// Async implementation of GetOpenFileNameA that uses host dialog if available.
	/// </summary>
	private async Task<uint> GetOpenFileNameAAsync(uint lpofn, CancellationToken cancellationToken = default)
	{
		_logger.LogInformation("[Comdlg32] GetOpenFileNameAAsync(lpofn=0x{Lpofn:X8})", lpofn);

		if (lpofn == 0)
		{
			return 0; // FALSE - dialog cancelled
		}

		// Read OPENFILENAME structure fields we need
		// Offset 0: lStructSize (DWORD)
		// Offset 4: hwndOwner (HWND)
		// Offset 8: hInstance (HINSTANCE)
		// Offset 12: lpstrFilter (LPCSTR)
		// Offset 16: lpstrCustomFilter (LPSTR)
		// Offset 20: nMaxCustFilter (DWORD)
		// Offset 24: nFilterIndex (DWORD)
		// Offset 28: lpstrFile (LPSTR)
		// Offset 32: nMaxFile (DWORD)
		// Offset 36: lpstrFileTitle (LPSTR)
		// Offset 40: nMaxFileTitle (WORD)
		// Offset 44: lpstrInitialDir (LPCSTR)
		// Offset 48: lpstrTitle (LPCSTR)
		// Offset 52: Flags (DWORD)

		var lpstrFile = _env.MemRead32(lpofn + 28);
		var nMaxFile = _env.MemRead32(lpofn + 32);
		var lpstrTitle = _env.MemRead32(lpofn + 48);

		// Try to read the title for the dialog
		string? dialogTitle = null;
		if (lpstrTitle != 0)
		{
			dialogTitle = _env.ReadAnsiString(lpstrTitle);
		}
		dialogTitle ??= "Open";

		// Try to read the filter if available
		// For now, we'll just use a basic filter

		// Use host's file dialog if available
		if (_env.Host != null)
		{
			try
			{
				var selectedPath = await _env.Host.OnOpenFileDialog(dialogTitle, null, null).ConfigureAwait(false);
				
				if (!string.IsNullOrEmpty(selectedPath))
				{
					// Write the selected path to lpstrFile buffer
					if (lpstrFile != 0 && nMaxFile > 0)
					{
						// Truncate path if needed to fit in the buffer
						var pathToWrite = selectedPath.Length < nMaxFile ? selectedPath : selectedPath.Substring(0, (int)nMaxFile - 1);
						_env.WriteAnsiStringAt(lpstrFile, pathToWrite);
					}
					
					_logger.LogInformation("[Comdlg32] GetOpenFileNameAAsync: Selected '{Path}'", selectedPath);
					return 1; // TRUE - success
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Comdlg32] GetOpenFileNameAAsync: Error showing dialog");
			}
		}

		_logger.LogInformation("[Comdlg32] GetOpenFileNameAAsync: Dialog cancelled");
		return 0; // FALSE - cancelled or error
	}

	/// <summary>
	/// Async implementation of GetSaveFileNameA that uses host dialog if available.
	/// </summary>
	private async Task<uint> GetSaveFileNameAAsync(uint lpofn, CancellationToken cancellationToken = default)
	{
		_logger.LogInformation("[Comdlg32] GetSaveFileNameAAsync(lpofn=0x{Lpofn:X8})", lpofn);

		if (lpofn == 0)
		{
			return 0; // FALSE - dialog cancelled
		}

		// Read OPENFILENAME structure fields we need
		var lpstrFile = _env.MemRead32(lpofn + 28);
		var nMaxFile = _env.MemRead32(lpofn + 32);
		var lpstrTitle = _env.MemRead32(lpofn + 48);

		// Try to read the title for the dialog
		string? dialogTitle = null;
		if (lpstrTitle != 0)
		{
			dialogTitle = _env.ReadAnsiString(lpstrTitle);
		}
		dialogTitle ??= "Save As";

		// Use host's file dialog if available
		if (_env.Host != null)
		{
			try
			{
				var selectedPath = await _env.Host.OnSaveFileDialog(dialogTitle, null, null).ConfigureAwait(false);
				
				if (!string.IsNullOrEmpty(selectedPath))
				{
					// Write the selected path to lpstrFile buffer
					if (lpstrFile != 0 && nMaxFile > 0)
					{
						// Truncate path if needed to fit in the buffer
						var pathToWrite = selectedPath.Length < nMaxFile ? selectedPath : selectedPath.Substring(0, (int)nMaxFile - 1);
						_env.WriteAnsiStringAt(lpstrFile, pathToWrite);
					}
					
					_logger.LogInformation("[Comdlg32] GetSaveFileNameAAsync: Selected '{Path}'", selectedPath);
					return 1; // TRUE - success
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Comdlg32] GetSaveFileNameAAsync: Error showing dialog");
			}
		}

		_logger.LogInformation("[Comdlg32] GetSaveFileNameAAsync: Dialog cancelled");
		return 0; // FALSE - cancelled or error
	}

	#endregion

	/// <summary>
	/// Retrieves the name of the specified file.
	/// short GetFileTitleA(LPCSTR lpszFile, LPSTR lpszTitle, WORD cchSize);
	/// </summary>
	[DllModuleExport(108, Version = "5.50.4134.100")]
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
	[DllModuleExport(115, Version = "5.50.4134.100", IsStub = true)]
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
	[DllModuleExport(117, Version = "5.50.4134.100", IsStub = true)]
	private uint PrintDlgA(uint lppd)
	{
		_logger.LogInformation("[Comdlg32] PrintDlgA(lppd=0x{Lppd:X8})", lppd);
		return 0; // User cancelled
	}

}
