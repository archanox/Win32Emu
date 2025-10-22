using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Win32.Modules;

/// <summary>
/// OLE32.DLL module - provides COM initialization and related functions.
/// </summary>
public class Ole32Module : IWin32ModuleUnsafe
{
	private readonly ProcessEnvironment _env;
	private readonly uint _imageBase;
	private readonly PeImageLoader? _peLoader;
	private readonly ILogger _logger;
	private bool _comInitialized;

	public Ole32Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
	{
		_env = env;
		_imageBase = imageBase;
		_peLoader = peLoader;
		_logger = logger ?? NullLogger.Instance;
		_comInitialized = false;
	}

	public string Name => "OLE32.DLL";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);

		switch (export.ToUpperInvariant())
		{
			case "COINITIALIZE":
				returnValue = CoInitialize(a.UInt32(0));
				return true;

			case "COUNINITIALIZE":
				returnValue = CoUninitialize();
				return true;

			case "COCREATEINSTANCE":
				returnValue = CoCreateInstance(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;

			default:
				_logger.LogInformation("[Ole32] Unimplemented export: {Export}", export);
				return false;
		}
	}

	/// <summary>
	/// Initializes the COM library on the current thread.
	/// HRESULT CoInitialize([in, optional] LPVOID pvReserved);
	/// Note: This implementation is synchronous but designed to be non-blocking.
	/// COM initialization in Win32 can involve thread-specific setup, which we handle
	/// cooperatively with the emulator's threading system.
	/// </summary>
	[DllModuleExport(4)]
	private uint CoInitialize(uint pvReserved)
	{
		_logger.LogInformation("[Ole32] CoInitialize called with pvReserved=0x{PvReserved:X8}", pvReserved);

		if (_comInitialized)
		{
			// S_FALSE - COM library is already initialized on this thread
			_logger.LogInformation("[Ole32] CoInitialize: COM already initialized, returning S_FALSE");
			return 0x00000001;
		}

		// Mark COM as initialized for this process/thread context
		// In a real Windows system, this would set up the COM runtime for the calling thread.
		// In our emulator, we simply track the initialization state.
		_comInitialized = true;
		_logger.LogInformation("[Ole32] CoInitialize: COM initialized successfully, returning S_OK");
		
		// S_OK - success
		return 0x00000000;
	}

	/// <summary>
	/// Closes the COM library on the current thread, unloads all DLLs loaded by the thread,
	/// frees any other resources that the thread maintains, and forces all RPC connections on the thread to close.
	/// void CoUninitialize();
	/// </summary>
	[DllModuleExport(0)]
	private uint CoUninitialize()
	{
		_logger.LogInformation("[Ole32] CoUninitialize called");

		if (!_comInitialized)
		{
			_logger.LogWarning("[Ole32] CoUninitialize: COM was not initialized on this thread");
		}
		else
		{
			_comInitialized = false;
			_logger.LogInformation("[Ole32] CoUninitialize: COM uninitialized successfully");
		}

		// CoUninitialize returns void (no return value in COM)
		return 0;
	}
	/// <summary>
	/// Creates a single uninitialized object of the class associated with a specified CLSID.
	/// HRESULT CoCreateInstance(
	///   [in]  REFCLSID  rclsid,
	///   [in]  LPUNKNOWN pUnkOuter,
	///   [in]  DWORD     dwClsContext,
	///   [in]  REFIID    riid,
	///   [out] LPVOID    *ppv
	/// );
	/// </summary>
	[DllModuleExport(20)]
	private uint CoCreateInstance(uint rclsid, uint pUnkOuter, uint dwClsContext, uint riid, uint ppv)
	{
		_logger.LogInformation("[Ole32] CoCreateInstance(rclsid=0x{Rclsid:X8}, pUnkOuter=0x{PUnkOuter:X8}, dwClsContext=0x{DwClsContext:X}, riid=0x{Riid:X8}, ppv=0x{Ppv:X8})",
			rclsid, pUnkOuter, dwClsContext, riid, ppv);

		// Stub implementation - COM object creation not fully supported
		// Return E_NOTIMPL (0x80004001) - Not implemented
		_logger.LogInformation("[Ole32] CoCreateInstance: COM object creation not implemented (stub)");
		
		// Write NULL to output pointer
		if (ppv != 0)
		{
			_env.MemWrite32(ppv, 0);
		}

		return 0x80004001; // E_NOTIMPL
	}

}
