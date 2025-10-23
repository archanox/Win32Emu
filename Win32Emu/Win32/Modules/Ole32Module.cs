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

			case "STRINGFROMGUID2":
				returnValue = StringFromGUID2(a.UInt32(0), a.UInt32(1), a.Int32(2));
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

	/// <summary>
	/// Converts a GUID into a string of printable characters.
	/// int StringFromGUID2(
	///   [in]  REFGUID rguid,
	///   [out] LPOLESTR lpsz,
	///   [in]  int cchMax
	/// );
	/// </summary>
	[DllModuleExport(12)]
	private uint StringFromGUID2(uint rguid, uint lpsz, int cchMax)
	{
		_logger.LogInformation("[Ole32] StringFromGUID2(rguid=0x{Rguid:X8}, lpsz=0x{Lpsz:X8}, cchMax={CchMax})",
			rguid, lpsz, cchMax);

		// Stub implementation - write a placeholder GUID string
		var guidStr = "{00000000-0000-0000-0000-000000000000}";
		if (lpsz != 0 && cchMax >= guidStr.Length + 1)
		{
			// Write as wide string (2 bytes per character)
			for (int i = 0; i < guidStr.Length; i++)
			{
				_env.MemWrite16(lpsz + (uint)(i * 2), (ushort)guidStr[i]);
			}
			_env.MemWrite16(lpsz + (uint)(guidStr.Length * 2), 0); // Null terminator
			return (uint)guidStr.Length + 1;
		}

		return 0; // Buffer too small
	}

}
