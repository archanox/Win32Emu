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

			case "CLSIDFROMSTRING":
				returnValue = CLSIDFromString(a.UInt32(0), a.UInt32(1));
				return true;

			case "CLSIDFROMPROGRAM":
				returnValue = CLSIDFromProgID(a.UInt32(0), a.UInt32(1));
				return true;

			case "STRINGFROMCLSID":
				returnValue = StringFromCLSID(a.UInt32(0), a.UInt32(1));
				return true;

			case "COGETCLASSOBJECT":
				returnValue = CoGetClassObject(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;

			case "COREGISTERCLASSOBJECT":
				returnValue = CoRegisterClassObject(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;

			case "COREVOKECLASSOBJECT":
				returnValue = CoRevokeClassObject(a.UInt32(0));
				return true;

			case "CODISCONNECTOBJECT":
				returnValue = CoDisconnectObject(a.UInt32(0), a.UInt32(1));
				return true;

			case "COREGISTERMESSAGEFILTER":
				returnValue = CoRegisterMessageFilter(a.UInt32(0), a.UInt32(1));
				return true;

			case "COFREEUNUSEDLIBRARIES":
				returnValue = CoFreeUnusedLibraries();
				return true;

			case "COTASKMEMALLOC":
				returnValue = CoTaskMemAlloc(a.UInt32(0));
				return true;

			case "COTASKMEMFREE":
				returnValue = CoTaskMemFree(a.UInt32(0));
				return true;

			case "OLEINITIALIZE":
				returnValue = OleInitialize(a.UInt32(0));
				return true;

			case "OLEUNINITIALIZE":
				returnValue = OleUninitialize();
				return true;

			case "OLEFLUSHCLIPBOARD":
				returnValue = OleFlushClipboard();
				return true;

			case "OLEISCURRENTCLIPBOARD":
				returnValue = OleIsCurrentClipboard(a.UInt32(0));
				return true;

			case "CREATEILOCKBYTESONHGLOBAL":
				returnValue = CreateILockBytesOnHGlobal(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;

			case "STGCREATEDOCFILEONILOCKBYTES":
				returnValue = StgCreateDocfileOnILockBytes(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;

			case "STGOPENSTORAGEONILOCKBYTES":
				returnValue = StgOpenStorageOnILockBytes(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5));
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

	/// <summary>
	/// Converts a string to a CLSID.
	/// HRESULT CLSIDFromString(LPCOLESTR lpsz, LPCLSID pclsid);
	/// </summary>
	[DllModuleExport(8)]
	private uint CLSIDFromString(uint lpsz, uint pclsid)
	{
		_logger.LogInformation("[Ole32] CLSIDFromString(lpsz=0x{Lpsz:X8}, pclsid=0x{Pclsid:X8})", lpsz, pclsid);

		// Stub: Write a null CLSID (16 bytes of zeros)
		if (pclsid != 0)
		{
			for (int i = 0; i < 16; i++)
			{
				_env.MemWrite8(pclsid + (uint)i, 0);
			}
		}

		return 0x00000000; // S_OK
	}

	/// <summary>
	/// Converts a programmatic identifier (ProgID) to a CLSID.
	/// HRESULT CLSIDFromProgID(LPCOLESTR lpszProgID, LPCLSID lpclsid);
	/// </summary>
	[DllModuleExport(8)]
	private uint CLSIDFromProgID(uint lpszProgID, uint lpclsid)
	{
		_logger.LogInformation("[Ole32] CLSIDFromProgID(lpszProgID=0x{LpszProgID:X8}, lpclsid=0x{Lpclsid:X8})", 
			lpszProgID, lpclsid);

		// Stub: Write a null CLSID
		if (lpclsid != 0)
		{
			for (int i = 0; i < 16; i++)
			{
				_env.MemWrite8(lpclsid + (uint)i, 0);
			}
		}

		return 0x80040154; // REGDB_E_CLASSNOTREG - Class not registered
	}

	/// <summary>
	/// Converts a CLSID to a string.
	/// HRESULT StringFromCLSID(REFCLSID rclsid, LPOLESTR *lplpsz);
	/// </summary>
	[DllModuleExport(8)]
	private uint StringFromCLSID(uint rclsid, uint lplpsz)
	{
		_logger.LogInformation("[Ole32] StringFromCLSID(rclsid=0x{Rclsid:X8}, lplpsz=0x{Lplpsz:X8})", 
			rclsid, lplpsz);

		// Stub: Return E_NOTIMPL
		if (lplpsz != 0)
		{
			_env.MemWrite32(lplpsz, 0);
		}

		return 0x80004001; // E_NOTIMPL
	}

	/// <summary>
	/// Retrieves a pointer to a class object.
	/// HRESULT CoGetClassObject(REFCLSID rclsid, DWORD dwClsContext, COSERVERINFO *pServerInfo, REFIID riid, LPVOID *ppv);
	/// </summary>
	[DllModuleExport(20)]
	private uint CoGetClassObject(uint rclsid, uint dwClsContext, uint pServerInfo, uint riid, uint ppv)
	{
		_logger.LogInformation("[Ole32] CoGetClassObject(rclsid=0x{Rclsid:X8}, dwClsContext=0x{DwClsContext:X}, riid=0x{Riid:X8}, ppv=0x{Ppv:X8})",
			rclsid, dwClsContext, riid, ppv);

		// Stub: Return E_NOTIMPL
		if (ppv != 0)
		{
			_env.MemWrite32(ppv, 0);
		}

		return 0x80004001; // E_NOTIMPL
	}

	/// <summary>
	/// Registers an EXE class object with COM.
	/// HRESULT CoRegisterClassObject(REFCLSID rclsid, LPUNKNOWN pUnk, DWORD dwClsContext, DWORD flags, LPDWORD lpdwRegister);
	/// </summary>
	[DllModuleExport(20)]
	private uint CoRegisterClassObject(uint rclsid, uint pUnk, uint dwClsContext, uint flags, uint lpdwRegister)
	{
		_logger.LogInformation("[Ole32] CoRegisterClassObject(rclsid=0x{Rclsid:X8}, pUnk=0x{PUnk:X8}, dwClsContext=0x{DwClsContext:X}, flags=0x{Flags:X}, lpdwRegister=0x{LpdwRegister:X8})",
			rclsid, pUnk, dwClsContext, flags, lpdwRegister);

		// Stub: Return a fake registration cookie
		if (lpdwRegister != 0)
		{
			_env.MemWrite32(lpdwRegister, 0x12345678);
		}

		return 0x00000000; // S_OK
	}

	/// <summary>
	/// Revokes a class object registration.
	/// HRESULT CoRevokeClassObject(DWORD dwRegister);
	/// </summary>
	[DllModuleExport(4)]
	private uint CoRevokeClassObject(uint dwRegister)
	{
		_logger.LogInformation("[Ole32] CoRevokeClassObject(dwRegister=0x{DwRegister:X})", dwRegister);
		return 0x00000000; // S_OK
	}

	/// <summary>
	/// Disconnects all remote process connections.
	/// HRESULT CoDisconnectObject(LPUNKNOWN pUnk, DWORD dwReserved);
	/// </summary>
	[DllModuleExport(8)]
	private uint CoDisconnectObject(uint pUnk, uint dwReserved)
	{
		_logger.LogInformation("[Ole32] CoDisconnectObject(pUnk=0x{PUnk:X8}, dwReserved=0x{DwReserved:X})", 
			pUnk, dwReserved);
		return 0x00000000; // S_OK
	}

	/// <summary>
	/// Registers or revokes a message filter.
	/// HRESULT CoRegisterMessageFilter(LPMESSAGEFILTER lpMessageFilter, LPMESSAGEFILTER *lplpMessageFilter);
	/// </summary>
	[DllModuleExport(8)]
	private uint CoRegisterMessageFilter(uint lpMessageFilter, uint lplpMessageFilter)
	{
		_logger.LogInformation("[Ole32] CoRegisterMessageFilter(lpMessageFilter=0x{LpMessageFilter:X8}, lplpMessageFilter=0x{LplpMessageFilter:X8})",
			lpMessageFilter, lplpMessageFilter);

		// Stub: Return the old filter as NULL
		if (lplpMessageFilter != 0)
		{
			_env.MemWrite32(lplpMessageFilter, 0);
		}

		return 0x00000000; // S_OK
	}

	/// <summary>
	/// Frees libraries that are no longer in use.
	/// void CoFreeUnusedLibraries();
	/// </summary>
	[DllModuleExport(0)]
	private uint CoFreeUnusedLibraries()
	{
		_logger.LogInformation("[Ole32] CoFreeUnusedLibraries called");
		return 0; // void function
	}

	/// <summary>
	/// Allocates a block of memory using the COM task allocator.
	/// LPVOID CoTaskMemAlloc(SIZE_T cb);
	/// </summary>
	[DllModuleExport(4)]
	private uint CoTaskMemAlloc(uint cb)
	{
		_logger.LogInformation("[Ole32] CoTaskMemAlloc(cb={Cb})", cb);

		// Allocate memory from the heap
		if (cb == 0)
		{
			return 0;
		}

		try
		{
			var ptr = _env.HeapAlloc(0, cb);
			return ptr;
		}
		catch
		{
			return 0;
		}
	}

	/// <summary>
	/// Frees a block of memory allocated by CoTaskMemAlloc.
	/// void CoTaskMemFree(LPVOID pv);
	/// </summary>
	[DllModuleExport(4)]
	private uint CoTaskMemFree(uint pv)
	{
		_logger.LogInformation("[Ole32] CoTaskMemFree(pv=0x{Pv:X8})", pv);

		// Free memory (stub - we don't track allocations in detail)
		if (pv != 0)
		{
			try
			{
				_env.HeapFree(0, pv);
			}
			catch
			{
				// Ignore errors
			}
		}

		return 0; // void function
	}

	/// <summary>
	/// Initializes the OLE library.
	/// HRESULT OleInitialize(LPVOID pvReserved);
	/// </summary>
	[DllModuleExport(4)]
	private uint OleInitialize(uint pvReserved)
	{
		_logger.LogInformation("[Ole32] OleInitialize(pvReserved=0x{PvReserved:X8})", pvReserved);

		if (_comInitialized)
		{
			return 0x00000001; // S_FALSE - already initialized
		}

		_comInitialized = true;
		return 0x00000000; // S_OK
	}

	/// <summary>
	/// Uninitializes the OLE library.
	/// void OleUninitialize();
	/// </summary>
	[DllModuleExport(0)]
	private uint OleUninitialize()
	{
		_logger.LogInformation("[Ole32] OleUninitialize called");
		_comInitialized = false;
		return 0; // void function
	}

	/// <summary>
	/// Flushes the clipboard.
	/// HRESULT OleFlushClipboard();
	/// </summary>
	[DllModuleExport(0)]
	private uint OleFlushClipboard()
	{
		_logger.LogInformation("[Ole32] OleFlushClipboard called");
		return 0x00000000; // S_OK
	}

	/// <summary>
	/// Determines whether the data object pointer previously placed on the clipboard is still on the clipboard.
	/// HRESULT OleIsCurrentClipboard(LPDATAOBJECT pDataObj);
	/// </summary>
	[DllModuleExport(4)]
	private uint OleIsCurrentClipboard(uint pDataObj)
	{
		_logger.LogInformation("[Ole32] OleIsCurrentClipboard(pDataObj=0x{PDataObj:X8})", pDataObj);
		return 0x00000001; // S_FALSE - not current clipboard
	}

	/// <summary>
	/// Creates a byte array object on global memory.
	/// HRESULT CreateILockBytesOnHGlobal(HGLOBAL hGlobal, BOOL fDeleteOnRelease, LPLOCKBYTES *pplkbyt);
	/// </summary>
	[DllModuleExport(12)]
	private uint CreateILockBytesOnHGlobal(uint hGlobal, uint fDeleteOnRelease, uint pplkbyt)
	{
		_logger.LogInformation("[Ole32] CreateILockBytesOnHGlobal(hGlobal=0x{HGlobal:X8}, fDeleteOnRelease={FDeleteOnRelease}, pplkbyt=0x{Pplkbyt:X8})",
			hGlobal, fDeleteOnRelease, pplkbyt);

		// Stub: Return E_NOTIMPL
		if (pplkbyt != 0)
		{
			_env.MemWrite32(pplkbyt, 0);
		}

		return 0x80004001; // E_NOTIMPL
	}

	/// <summary>
	/// Creates a compound file storage object on a byte array object.
	/// HRESULT StgCreateDocfileOnILockBytes(ILockBytes *plkbyt, DWORD grfMode, DWORD reserved, IStorage **ppstgOpen);
	/// </summary>
	[DllModuleExport(16)]
	private uint StgCreateDocfileOnILockBytes(uint plkbyt, uint grfMode, uint reserved, uint ppstgOpen)
	{
		_logger.LogInformation("[Ole32] StgCreateDocfileOnILockBytes(plkbyt=0x{Plkbyt:X8}, grfMode=0x{GrfMode:X}, reserved={Reserved}, ppstgOpen=0x{PpstgOpen:X8})",
			plkbyt, grfMode, reserved, ppstgOpen);

		// Stub: Return E_NOTIMPL
		if (ppstgOpen != 0)
		{
			_env.MemWrite32(ppstgOpen, 0);
		}

		return 0x80004001; // E_NOTIMPL
	}

	/// <summary>
	/// Opens an existing storage object on a byte array.
	/// HRESULT StgOpenStorageOnILockBytes(ILockBytes *plkbyt, IStorage *pstgPriority, DWORD grfMode, SNB snbExclude, DWORD reserved, IStorage **ppstgOpen);
	/// </summary>
	[DllModuleExport(24)]
	private uint StgOpenStorageOnILockBytes(uint plkbyt, uint pstgPriority, uint grfMode, uint snbExclude, uint reserved, uint ppstgOpen)
	{
		_logger.LogInformation("[Ole32] StgOpenStorageOnILockBytes(plkbyt=0x{Plkbyt:X8}, pstgPriority=0x{PstgPriority:X8}, grfMode=0x{GrfMode:X}, ppstgOpen=0x{PpstgOpen:X8})",
			plkbyt, pstgPriority, grfMode, ppstgOpen);

		// Stub: Return E_NOTIMPL
		if (ppstgOpen != 0)
		{
			_env.MemWrite32(ppstgOpen, 0);
		}

		return 0x80004001; // E_NOTIMPL
	}

}
