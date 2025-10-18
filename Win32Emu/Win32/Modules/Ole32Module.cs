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
}
