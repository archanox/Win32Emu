using System.Diagnostics;
using System.Text;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.VirtualFileSystem;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Win32.Modules;

public class Kernel32Module : IWin32ModuleUnsafe
{
	private readonly ProcessEnvironment _env;
	private readonly uint _imageBase;
	private readonly PeImageLoader? _peLoader;
	private readonly ILogger _logger;
	private PeResourceReader? _resourceReader;

	public Kernel32Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
	{
		_env = env;
		_imageBase = imageBase;
		_peLoader = peLoader;
		_logger = logger ?? NullLogger.Instance;
	}

	public string Name => "KERNEL32.DLL";

	private Win32Dispatcher? _dispatcher;
	private uint _lastError;
	private ICpu? _cpu;

	public void SetResourceReader(PeResourceReader resourceReader)
	{
		_resourceReader = resourceReader;
	}

	public void SetDispatcher(Win32Dispatcher dispatcher)
	{
		_dispatcher = dispatcher;
	}

	public unsafe bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		_cpu = cpu;
		returnValue = 0;
		var a = new StackArgs(cpu, memory);
		switch (export.ToUpperInvariant())
		{
			// Process / version / module
			case "GETVERSION":
				returnValue = GetVersion();
				return true;
			case "ISPROCESSORFEATUREPRESENT":
				returnValue = IsProcessorFeaturePresent(a.UInt32(0));
				return true;
			case "GETVERSIONEXA":
				returnValue = GetVersionExA(a.UInt32(0));
				return true;
			case "GETVERSIONEXW":
				returnValue = GetVersionExW(a.UInt32(0));
				return true;
			case "GETLASTERROR":
				returnValue = GetLastError();
				return true;
			case "SETLASTERROR":
				returnValue = SetLastError(a.UInt32(0));
				return true;
			case "EXITPROCESS":
				returnValue = ExitProcess(a.UInt32(0));
				return true;
			case "TERMINATEPROCESS":
				returnValue = TerminateProcess(a.UInt32(0), a.UInt32(1));
				return true;
			case "GETCURRENTPROCESS":
				returnValue = GetCurrentProcess();
				return true;
			case "GETACP":
				returnValue = (uint)GetAcp();
				return true;
			case "GETCPINFO":
				returnValue = GetCpInfo((CodePage)a.UInt32(0), a.Lpcpinfo(1));
				return true;
			case "GETOEMCP":
				returnValue = (uint)GetOemCp();
				return true;
			case "GETSTRINGTYPEA":
				returnValue = GetStringTypeA(a.UInt32(0), a.UInt32(1), a.Lpstr(2), a.Int32(3), a.UInt32(4));
				return true;
			case "GETSTRINGTYPEW":
				returnValue = GetStringTypeW(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.Int32(3), a.UInt32(4));
				return true;
			case "GETMODULEHANDLEA":
				returnValue = GetModuleHandleA(a.LpcStr(0));
				return true;
			case "GETMODULEFILENAMEA":
				returnValue = GetModuleFileNameA(a.Ptr(0), a.Lpstr(1), a.UInt32(2));
				return true;
			case "LOADLIBRARYA":
				returnValue = LoadLibraryA(a.LpcStr(0));
				return true;
			case "GETPROCADDRESS":
				returnValue = GetProcAddress(a.UInt32(0), a.LpcStr(1));
				return true;
			case "GETSTARTUPINFOA":
				returnValue = GetStartupInfoA(a.UInt32(0));
				return true;
			case "GETCOMMANDLINEA":
				returnValue = GetCommandLineA();
				return true;
			case "GETENVIRONMENTSTRINGSW":
				returnValue = GetEnvironmentStringsW();
				return true;
			case "GETENVIRONMENTSTRINGS":
				returnValue = GetEnvironmentStrings();
				return true;
			case "GETENVIRONMENTSTRINGSA":
				returnValue = GetEnvironmentStringsA();
				return true;
			case "SETENVIRONMENTVARIABLEA":
				returnValue = SetEnvironmentVariableA(a.UInt32(0), a.UInt32(1));
				return true;
			case "FREEENVIRONMENTSTRINGSW":
				returnValue = FreeEnvironmentStringsW(a.UInt32(0));
				return true;
			case "FREEENVIRONMENTSTRINGSA":
				returnValue = FreeEnvironmentStringsA(a.UInt32(0));
				return true;

			// Std handles
			case "GETSTDHANDLE":
				returnValue = GetStdHandle(a.UInt32(0));
				return true;
			case "SETSTDHANDLE":
				returnValue = SetStdHandle(a.UInt32(0), a.UInt32(1));
				return true;
			case "ALLOCCONSOLE":
				returnValue = AllocConsole();
				return true;
			case "FREECONSOLE":
				returnValue = FreeConsole();
				return true;
			case "ATTACHCONSOLE":
				returnValue = AttachConsole(a.UInt32(0));
				return true;

			// Memory/heap
			case "GLOBALALLOC":
				returnValue = GlobalAlloc(a.UInt32(0), a.UInt32(1));
				return true;
			case "GLOBALFREE":
				returnValue = GlobalFree((void*)a.UInt32(0));
				return true;
			case "GLOBALLOCK":
				returnValue = GlobalLock((void*)a.UInt32(0));
				return true;
			case "GLOBALUNLOCK":
				returnValue = GlobalUnlock((void*)a.UInt32(0));
				return true;
			case "GLOBALHANDLE":
				returnValue = GlobalHandle((void*)a.UInt32(0));
				return true;
			case "HEAPCREATE":
				returnValue = HeapCreate(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "HEAPALLOC":
				returnValue = HeapAlloc((void*)a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "HEAPFREE":
				returnValue = HeapFree((void*)a.UInt32(0), a.UInt32(1), (void*)a.UInt32(2));
				return true;
			case "HEAPREALLOC":
				returnValue = HeapReAlloc((void*)a.UInt32(0), a.UInt32(1), (void*)a.UInt32(2), a.UInt32(3));
				return true;
			case "HEAPDESTROY":
				returnValue = HeapDestroy((void*)a.UInt32(0));
				return true;
			case "LOCALALLOC":
				returnValue = LocalAlloc(a.UInt32(0), a.UInt32(1));
				return true;
			case "VIRTUALALLOC":
				returnValue = VirtualAlloc(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;
			case "VIRTUALFREE":
				returnValue = VirtualFree(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;

			// File I/O
			case "CREATEFILEA":
				returnValue = CreateFileA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5),
					a.UInt32(6));
				return true;
			case "READFILE":
				returnValue = ReadFile((void*)a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;
			case "WRITEFILE":
				returnValue = WriteFile(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;
			case "CLOSEHANDLE":
				returnValue = CloseHandle((void*)a.UInt32(0));
				return true;
			case "GETFILETYPE":
				returnValue = GetFileType((void*)a.UInt32(0));
				return true;
			case "SETFILEPOINTER":
				returnValue = SetFilePointer((void*)a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;
			case "FLUSHFILEBUFFERS":
				returnValue = FlushFileBuffers((void*)a.UInt32(0));
				return true;
			case "SETENDOFFILE":
				returnValue = SetEndOfFile((void*)a.UInt32(0));
				return true;
			case "DELETEFILEA":
				returnValue = DeleteFileA(a.UInt32(0));
				return true;
			case "MOVEFILEA":
				returnValue = MoveFileA(a.UInt32(0), a.UInt32(1));
				return true;
			case "FINDFIRSTFILEA":
				returnValue = FindFirstFileA(a.UInt32(0), a.UInt32(1));
				return true;
			case "FINDNEXTFILEA":
				returnValue = FindNextFileA(a.UInt32(0), a.UInt32(1));
				return true;
			case "FINDCLOSE":
				returnValue = FindClose((void*)a.UInt32(0));
				return true;
			case "FILETIMETOSYSTEMTIME":
				returnValue = FileTimeToSystemTime(a.UInt32(0), a.UInt32(1));
				return true;
			case "FILETIMETOLOCALFILETIME":
				returnValue = FileTimeToLocalFileTime(a.UInt32(0), a.UInt32(1));
				return true;
			case "GETTIMEZONEINFORMATION":
				returnValue = GetTimeZoneInformation(a.UInt32(0));
				return true;
			case "SETHANDLECOUNT":
				returnValue = SetHandleCount(a.UInt32(0));
				return true;
			case "UNHANDLEDEXCEPTIONFILTER":
				returnValue = UnhandledExceptionFilter(a.UInt32(0));
				return true;
			case "RTLUNWIND":
				returnValue = RtlUnwind(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;
			case "WIDECHARTOMULTIBYTE":
				returnValue = WideCharToMultiByte((CodePage)a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5), a.UInt32(6), a.UInt32(7));
				return true;
			case "MULTIBYTETOWIDECHAR":
				returnValue = MultiByteToWideChar((CodePage)a.UInt32(0), a.UInt32(1), a.UInt32(2), a.Int32(3), a.UInt32(4), a.UInt32(5));
				return true;
			case "LCMAPSTRINGA":
				returnValue = LcMapStringA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.Int32(3), a.UInt32(4), a.Int32(5));
				return true;
			case "LCMAPSTRINGW":
				returnValue = LcMapStringW(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.Int32(3), a.UInt32(4), a.Int32(5));
				return true;
			case "COMPARESTRINGA":
				returnValue = CompareStringA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.Int32(3), a.UInt32(4), a.Int32(5));
				return true;
			case "COMPARESTRINGW":
				returnValue = CompareStringW(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.Int32(3), a.UInt32(4), a.Int32(5));
				return true;
			case "RAISEEXCEPTION":
				returnValue = RaiseException(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;

			// Performance/timing functions
			case "QUERYPERFORMANCECOUNTER":
				returnValue = QueryPerformanceCounter(a.UInt32(0));
				return true;
			case "QUERYPERFORMANCEFREQUENCY":
				returnValue = QueryPerformanceFrequency(a.UInt32(0));
				return true;
			case "GETTICKCOUNT":
				returnValue = GetTickCount();
				return true;
			case "GETTICKCOUNT64":
				returnValue = GetTickCount64(a.UInt32(0));
				return true;
			case "SLEEP":
				returnValue = Sleep(a.UInt32(0));
				return true;

			// Thread management and TLS functions
			case "CREATETHREAD":
				returnValue = CreateThread(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5));
				return true;
			case "RESUMETHREAD":
				returnValue = ResumeThread(a.UInt32(0));
				return true;
			case "SUSPENDTHREAD":
				returnValue = SuspendThread(a.UInt32(0));
				return true;
			case "GETCURRENTTHREADID":
				returnValue = GetCurrentThreadId();
				return true;
			case "TLSALLOC":
				returnValue = TlsAlloc();
				return true;
			case "TLSGETVALUE":
				returnValue = TlsGetValue(a.UInt32(0));
				return true;
			case "TLSSETVALUE":
				returnValue = TlsSetValue(a.UInt32(0), a.UInt32(1));
				return true;
			case "TLSFREE":
				returnValue = TlsFree(a.UInt32(0));
				return true;

			// Synchronization primitives
			case "CREATEMUTEXW":
				returnValue = CreateMutexW(a.UInt32(0), a.UInt32(1), a.LpcStr(2));
				return true;
			case "CREATEMUTEXA":
				returnValue = CreateMutexA(a.UInt32(0), a.UInt32(1), a.LpcStr(2));
				return true;
			case "RELEASEMUTEX":
				returnValue = ReleaseMutex(a.UInt32(0));
				return true;
			case "CREATEEVENTW":
				returnValue = CreateEventW(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.LpcStr(3));
				return true;
			case "CREATEEVENTA":
				returnValue = CreateEventA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.LpcStr(3));
				return true;
			case "SETEVENT":
				returnValue = SetEvent(a.UInt32(0));
				return true;
			case "RESETEVENT":
				returnValue = ResetEvent(a.UInt32(0));
				return true;
			case "PULSEEVENT":
				returnValue = PulseEvent(a.UInt32(0));
				return true;
			case "CREATESEMAPHOREW":
				returnValue = CreateSemaphoreW(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.LpcStr(3));
				return true;
			case "CREATESEMAPHOREA":
				returnValue = CreateSemaphoreA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.LpcStr(3));
				return true;
			case "RELEASESEMAPHORE":
				returnValue = ReleaseSemaphore(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "WAITFORSINGLEOBJECT":
				returnValue = WaitForSingleObject(a.UInt32(0), a.UInt32(1));
				return true;

			// Directory functions
			case "SETCURRENTDIRECTORYA":
				returnValue = SetCurrentDirectoryA(a.LpcStr(0));
				return true;
			case "GETCURRENTDIRECTORYA":
				returnValue = GetCurrentDirectoryA(a.UInt32(0), a.LpStr(1));
				return true;

			// String functions
			case "LSTRCATA":
				returnValue = LstrcatA(a.LpStr(0), a.LpcStr(1));
				return true;

			// Process execution
			case "WINEXEC":
				returnValue = WinExec(a.LpcStr(0), a.UInt32(1));
				return true;

			// Critical section synchronization
			case "INITIALIZECRITICALSECTION":
				returnValue = InitializeCriticalSection(a.UInt32(0));
				return true;
			case "DELETECRITICALSECTION":
				returnValue = DeleteCriticalSection(a.UInt32(0));
				return true;
			case "ENTERCRITICALSECTION":
				returnValue = EnterCriticalSection(a.UInt32(0));
				return true;
			case "LEAVECRITICALSECTION":
				returnValue = LeaveCriticalSection(a.UInt32(0));
				return true;

			// Resource functions
			case "FINDRESOURCEA":
				returnValue = FindResourceA(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "LOADRESOURCE":
				returnValue = LoadResource(a.UInt32(0), a.UInt32(1));
				return true;
			case "SIZEOFRESOURCE":
				returnValue = SizeofResource(a.UInt32(0), a.UInt32(1));
				return true;
			case "LOCKRESOURCE":
				returnValue = LockResource(a.UInt32(0));
				return true;

			default:
				_logger.LogInformation("[Kernel32] Unimplemented export: {Export}", export);
				return false;
		}
	}
	
	[DllModuleExport(203, entryPoint: 0x00039B1E, Version = "4.90.0.3000")]
	[DllModuleExport(106, entryPoint: 0x00010156, Version = "5.1.2600.6532")]
	public uint CreateSemaphoreW(uint lpSemaphoreAttributes, uint lInitialCount, uint lMaximumCount, LpcStr lpName)
	{
		_logger.LogInformation("[kernel32] CreateSemaphoreW: lpSemaphoreAttributes={lpSemaphoreAttributes}, lInitialCount={lInitialCount}, lMaximumCount={lMaximumCount}, lpName={lpName}", lpSemaphoreAttributes, lInitialCount, lMaximumCount, lpName);
		return CreateSemaphore(lpSemaphoreAttributes, lInitialCount, lMaximumCount, lpName);
	}
	
	[DllModuleExport(202, entryPoint: 0x000075C5, Version = "4.90.0.3000")]
	[DllModuleExport(105, entryPoint: 0x00010B6D, Version = "5.1.2600.6532")]
	public uint CreateSemaphoreA(uint lpSemaphoreAttributes, uint lInitialCount, uint lMaximumCount, LpcStr lpName)
	{
		_logger.LogInformation("[kernel32] CreateSemaphoreA: lpSemaphoreAttributes={lpSemaphoreAttributes}, lInitialCount={lInitialCount}, lMaximumCount={lMaximumCount}, lpName={lpName}", lpSemaphoreAttributes, lInitialCount, lMaximumCount, lpName);
		return CreateSemaphore(lpSemaphoreAttributes, lInitialCount, lMaximumCount, lpName);
	}
	
	[DllModuleExport(184, entryPoint: 0x00039B1E, Version = "4.90.0.3000")]
	[DllModuleExport(77, entryPoint: 0x0000A749, Version = "5.1.2600.6532")]
	public uint CreateEventW(uint lpEventAttributes, uint bManualReset, uint bInitialState, LpcStr lpName)
	{
		_logger.LogWarning("[kernel32] CreateEventW: lpEventAttributes={lpEventAttributes}, bManualReset={bManualReset}, bInitialState={bInitialState}, lpName={lpName}", lpEventAttributes, bManualReset, bInitialState, lpName);
		return CreateEvent(lpEventAttributes, bManualReset, bInitialState, lpName);
	}
	
	[DllModuleExport(183, entryPoint: 0x00007568, Version = "4.90.0.3000", IsStub = true)]
	[DllModuleExport(76, entryPoint: 0x00030922, Version = "5.1.2600.6532", IsStub = true)]
	public uint CreateEventA(uint lpEventAttributes, uint bManualReset, uint bInitialState, LpcStr lpName)
	{
		_logger.LogWarning("[kernel32] CreateEventA: lpEventAttributes={lpEventAttributes}, bManualReset={bManualReset}, bInitialState={bInitialState}, lpName={lpName}", lpEventAttributes, bManualReset, bInitialState, lpName);
		return CreateEvent(lpEventAttributes, bManualReset, bInitialState, lpName);
	}
	
	[DllModuleExport(194, entryPoint: 0x00007532, Version = "4.90.0.3000", IsStub = true)]
	[DllModuleExport(93, entryPoint: 0x0000E9DF, Version = "5.1.2600.6532", IsStub = true)]
	private uint CreateMutexA(uint lpMutexAttributes, uint bInitialOwner, LpcStr lpName)
	{
		_logger.LogInformation("[kernel32] CreateMutexA: lpMutexAttributes={lpMutexAttributes}, bInitialOwner={bInitialOwner}, lpName={lpName}", lpMutexAttributes, bInitialOwner, lpName);
		return CreateMutex(lpMutexAttributes, bInitialOwner, lpName);
	}

	[DllModuleExport(195, entryPoint: 0x00039B03, Version = "4.90.0.3000", IsStub = true)]
	[DllModuleExport(94, entryPoint: 0x0000E957, Version = "5.1.2600.6532", IsStub = true)]
	public uint CreateMutexW(uint lpMutexAttributes, uint bInitialOwner, LpcStr lpName)
	{
		_logger.LogInformation("[kernel32] CreateMutexW: lpMutexAttributes={lpMutexAttributes}, bInitialOwner={bInitialOwner}, lpName={lpName}", lpMutexAttributes, bInitialOwner, lpName);
		return CreateMutex(lpMutexAttributes, bInitialOwner, lpName);
	}

	[DllModuleExport(370, entryPoint: 0x0001B03D, Version = "4.90.0.3000", IsStub = true)]
	[DllModuleExport(334, entryPoint: 0x0001C123, Version = "5.1.2600.6532", IsStub = true)]
	private uint GetEnvironmentStrings() => GetEnvironmentStringsA();

	/// <summary>
	/// Retrieves the version number of the operating system.
	/// With the release of Windows 8.1, the behavior of this API has changed. The value returned now depends on how the application is manifested.
	/// </summary>
	/// <returns>
	/// If the function succeeds, the return value includes the major and minor version numbers of the operating system in the low-order word,
	/// and information about the operating system platform in the high-order word.
	/// The low-order byte specifies the major version number in hexadecimal notation. 
	/// The high-order byte specifies the minor version number in hexadecimal notation.
	/// </returns>
	/// <remarks>
	/// This function has been deprecated. Applications not manifested for Windows 8.1 or Windows 10 will return the Windows 8 OS version value (6.2).
	/// It is recommended to use the Version Helper functions instead for version detection.
	/// </remarks>
	[DllModuleExport(489, entryPoint: 0x000233FD, Version = "4.90.0.3000")]
	[DllModuleExport(478, entryPoint: 0x00011752, Version = "5.1.2600.6532")]
	private uint GetVersion()
	{
		const ushort build = 950;
		const byte major = 4;
		const byte minor = 0;
		return (major << 8 | minor) << 16 | build;
	}

	[DllModuleExport(85)]
	private uint IsProcessorFeaturePresent(uint processorFeature)
	{
		// Return features that would be present on an Intel Pentium 1 processor
		// Pentium 1 (P5) was introduced in 1993 and had the following features:
		// - FPU (Floating Point Unit) - built-in, not emulated
		// - TSC (Time Stamp Counter)
		// - MSR (Model Specific Registers)
		// - CX8 (CMPXCHG8B instruction)
		// - MMX was added in Pentium MMX (P55C) in 1997, not in original P5

		const uint PF_FLOATING_POINT_PRECISION_ERRATA = 0;
		const uint PF_FLOATING_POINT_EMULATED = 1;
		const uint PF_COMPARE_EXCHANGE_DOUBLE = 2;
		const uint PF_MMX_INSTRUCTIONS_AVAILABLE = 3;
		const uint PF_RDTSC_INSTRUCTION_AVAILABLE = 8;
		const uint PF_3DNOW_INSTRUCTIONS_AVAILABLE = 7;

		var isPresent = processorFeature switch
		{
			PF_FLOATING_POINT_PRECISION_ERRATA => false, // No known FPU precision bug
			PF_FLOATING_POINT_EMULATED => false, // FPU is built-in, not emulated
			PF_COMPARE_EXCHANGE_DOUBLE => true, // Pentium has CMPXCHG8B
			PF_MMX_INSTRUCTIONS_AVAILABLE => false, // Original Pentium doesn't have MMX (added in P55C)
			PF_RDTSC_INSTRUCTION_AVAILABLE => true, // Pentium has RDTSC
			PF_3DNOW_INSTRUCTIONS_AVAILABLE => false, // 3DNow! is AMD K6-2 feature
			_ => false // Other features not present
		};

		_logger.LogDebug("[Kernel32] IsProcessorFeaturePresent({ProcessorFeature}) -> {Result}", processorFeature, isPresent);

		return isPresent ? 1u : 0u; // TRUE or FALSE
	}

	[DllModuleExport(48, ForwardedTo = "KERNELBASE.GetVersionEx")]
	private uint GetVersionEx()
	{
		// This is a forwarded export - the actual implementation is in KERNELBASE.DLL
		// This method will never be called; GetProcAddress will resolve to KERNELBASE
		throw new NotImplementedException("This export is forwarded to KERNELBASE.GetVersionEx");
	}

	/// <summary>
	/// Retrieves information about the current operating system (ANSI version).
	/// With the release of Windows 8.1, the behavior of this API has changed. The value returned now depends on how the application is manifested.
	/// </summary>
	/// <param name="lpVersionInformation">
	/// An OSVERSIONINFOA or OSVERSIONINFOEXA structure that receives the operating system information.
	/// Before calling the GetVersionEx function, set the dwOSVersionInfoSize member of the structure as appropriate to indicate which data structure is being passed.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is a nonzero value.
	/// If the function fails, the return value is zero. The function fails if an invalid value is specified for the dwOSVersionInfoSize member.
	/// </returns>
	/// <remarks>
	/// This function has been deprecated. Applications not manifested for Windows 8.1 or Windows 10 will return the Windows 8 OS version value (6.2).
	/// Identifying the current operating system is usually not the best way to determine whether a particular operating system feature is present.
	/// Instead, test for the presence of the feature itself.
	/// </remarks>
	[DllModuleExport(490, Version = "4.90.0.3000")]
	[DllModuleExport(479, entryPoint: 0x00010830, Version = "5.1.2600.6532")]
	public uint GetVersionExA(uint lpVersionInformation)
	{
		if (lpVersionInformation == 0) return NativeTypes.Win32Bool.FALSE;

		var size = _env.MemRead32(lpVersionInformation);
		if (size != 156 && size != 148) // sizeof(OSVERSIONINFOEXA) and sizeof(OSVERSIONINFOA)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
			return NativeTypes.Win32Bool.FALSE;
		}

		_env.MemWrite32(lpVersionInformation + 4, 5); // dwMajorVersion = 5 (Windows XP)
		_env.MemWrite32(lpVersionInformation + 8, 1); // dwMinorVersion = 1
		_env.MemWrite32(lpVersionInformation + 12, 2600); // dwBuildNumber = 2600
		_env.MemWrite32(lpVersionInformation + 16, 2); // dwPlatformId = VER_PLATFORM_WIN32_NT
		_env.MemWriteBytes(lpVersionInformation + 20, Encoding.ASCII.GetBytes("Service Pack 3\0".PadRight(128, '\0')));
		if (size == 156)
		{
			_env.MemWrite16(lpVersionInformation + 148, 3); // wServicePackMajor = 3
			_env.MemWrite16(lpVersionInformation + 150, 0); // wServicePackMinor = 0
			_env.MemWrite16(lpVersionInformation + 152, 0x0100); // wSuiteMask = VER_SUITE_SINGLEUSERUI
			_env.MemWrite8(lpVersionInformation + 154, 1); // wProductType = VER_NT_WORKSTATION
			_env.MemWrite8(lpVersionInformation + 155, 0); // wReserved = 0
		}

		return NativeTypes.Win32Bool.TRUE;
	}

	/// <summary>
	/// Retrieves information about the current operating system (Unicode version).
	/// With the release of Windows 8.1, the behavior of this API has changed. The value returned now depends on how the application is manifested.
	/// </summary>
	/// <param name="lpVersionInformation">
	/// An OSVERSIONINFOW or OSVERSIONINFOEXW structure that receives the operating system information.
	/// Before calling the GetVersionEx function, set the dwOSVersionInfoSize member of the structure as appropriate to indicate which data structure is being passed.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is a nonzero value.
	/// If the function fails, the return value is zero. The function fails if an invalid value is specified for the dwOSVersionInfoSize member.
	/// </returns>
	/// <remarks>
	/// This function has been deprecated. Applications not manifested for Windows 8.1 or Windows 10 will return the Windows 8 OS version value (6.2).
	/// Identifying the current operating system is usually not the best way to determine whether a particular operating system feature is present.
	/// Instead, test for the presence of the feature itself.
	/// </remarks>
	[DllModuleExport(491, Version = "4.90.0.3000")]
	[DllModuleExport(480, entryPoint: 0x0000AF05, Version = "5.1.2600.6532")]
	public uint GetVersionExW(uint lpVersionInformation)
	{
		if (lpVersionInformation == 0) return NativeTypes.Win32Bool.FALSE;

		var size = _env.MemRead32(lpVersionInformation);
		if (size != 284 && size != 276) // sizeof(OSVERSIONINFOEXW) and sizeof(OSVERSIONINFOW)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
			return NativeTypes.Win32Bool.FALSE;
		}

		_env.MemWrite32(lpVersionInformation + 4, 5); // dwMajorVersion = 5 (Windows XP)
		_env.MemWrite32(lpVersionInformation + 8, 1); // dwMinorVersion = 1
		_env.MemWrite32(lpVersionInformation + 12, 2600); // dwBuildNumber = 2600
		_env.MemWrite32(lpVersionInformation + 16, 2); // dwPlatformId = VER_PLATFORM_WIN32_NT

		var sp = "Service Pack 3\0".ToCharArray();
		var bytes = new byte[sp.Length * 2];
		Buffer.BlockCopy(sp, 0, bytes, 0, bytes.Length);
		_env.MemWriteBytes(lpVersionInformation + 20, bytes.AsSpan()[..256]);

		if (size == 284)
		{
			_env.MemWrite16(lpVersionInformation + 276, 3); // wServicePackMajor = 3
			_env.MemWrite16(lpVersionInformation + 278, 0); // wServicePackMinor = 0
			_env.MemWrite16(lpVersionInformation + 280, 0x0100); // wSuiteMask = VER_SUITE_SINGLEUSERUI
			_env.MemWrite8(lpVersionInformation + 282, 1); // wProductType = VER_NT_WORKSTATION
			_env.MemWrite8(lpVersionInformation + 283, 0); // wReserved = 0
		}

		return NativeTypes.Win32Bool.TRUE;
	}

	/// <summary>
	/// Retrieves the calling thread's last-error code value. The last-error code is maintained on a per-thread basis.
	/// Multiple threads do not overwrite each other's last-error code.
	/// </summary>
	/// <returns>
	/// The return value is the calling thread's last-error code.
	/// </returns>
	/// <remarks>
	/// Functions set this value by calling the SetLastError function if they fail.
	/// You should call the GetLastError function immediately when a function's return value indicates that such a call will return useful data.
	/// </remarks>
	[DllModuleExport(361, entryPoint: 0x000090DB, Version = "5.1.2600.6532")]
	private uint GetLastError() => _lastError;

	/// <summary>
	/// Sets the last-error code for the calling thread.
	/// </summary>
	/// <param name="e">
	/// The last-error code for the thread.
	/// </param>
	/// <returns>
	/// This function does not return a value.
	/// </returns>
	/// <remarks>
	/// The last-error code is maintained on a per-thread basis. Multiple threads do not overwrite each other's last-error code.
	/// Error codes are 32-bit values (bit 31 is the most significant bit). Bit 29 is reserved for application-defined error codes; no system error code has this bit set.
	/// </remarks>
	[DllModuleExport(41)]
	private uint SetLastError(uint e)
	{
		_lastError = e;
		return 0;
	}

	/// <summary>
	/// Ends the calling process and all its threads.
	/// </summary>
	/// <param name="code">
	/// The exit code for the process and all threads.
	/// </param>
	/// <returns>
	/// This function does not return a value.
	/// </returns>
	/// <remarks>
	/// Use the ExitProcess function to end a process. This function provides a clean process shutdown.
	/// ExitProcess is the preferred method of ending a process.
	/// Exiting a process causes the following:
	/// All of the object handles opened by the process are closed.
	/// All of the threads in the process terminate their execution.
	/// The state of the process object becomes signaled, satisfying any threads that had been waiting for the process to terminate.
	/// The process's termination status changes from STILL_ACTIVE to the exit code of the process.
	/// </remarks>
	[DllModuleExport(3)]
	private uint ExitProcess(uint code)
	{
		_logger.LogInformation("[Kernel32] ExitProcess({Code})", code);
		_env.RequestExit();
		return 0;
	}

	[DllModuleExport(43)]
	private uint TerminateProcess(uint hProcess, uint uExitCode)
	{
		// TerminateProcess terminates the specified process
		// hProcess: handle to the process (0xFFFFFFFF for current process)
		// uExitCode: exit code for the process

		_logger.LogInformation("[Kernel32] TerminateProcess(0x{HProcess:X8}, {UExitCode})", hProcess, uExitCode);

		// In our emulator, we only support terminating the current process
		if (hProcess is 0xFFFFFFFF or 0)
		{
			_env.RequestExit();
			return NativeTypes.Win32Bool.TRUE;
		}

		// We don't support terminating other processes
		_logger.LogInformation("[Kernel32] TerminateProcess: Cannot terminate external process handle 0x{HProcess:X8}", hProcess);
		_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
		return NativeTypes.Win32Bool.FALSE;
	}

	[DllModuleExport(35)]
	private uint RaiseException(uint dwExceptionCode, uint dwExceptionFlags, uint nNumberOfArguments, uint lpArguments)
	{
		// RaiseException raises a software exception
		// For now, we just log and continue - proper implementation would need exception handling
		_logger.LogInformation("[Kernel32] RaiseException(code=0x{DwExceptionCode:X8}, flags=0x{DwExceptionFlags:X}, nArgs={NNumberOfArguments}, args=0x{LpArguments:X8})", dwExceptionCode, dwExceptionFlags, nNumberOfArguments, lpArguments);

		// In a real implementation, this would:
		// 1. Create an EXCEPTION_RECORD
		// 2. Search for exception handlers
		// 3. Unwind the stack if no handler found
		// For our emulator, we'll just log and return (doesn't actually return in real Win32)

		// This function doesn't return in normal Windows - it transfers control to exception handler
		// But for our simple emulator, we'll just return 0
		return 0;
	}

	[DllModuleExport(10)]
	private uint GetCurrentProcess() => 0xFFFFFFFF; // pseudo-handle

	[DllModuleExport(7)]
	public CodePage GetAcp() => CodePage.Utf8;

	[DllModuleExport(9)]
	public unsafe uint GetCpInfo(CodePage codePage, NativeTypes.Lpcpinfo lpCpInfo)
	{
		_logger.LogInformation("[Kernel32] GetCPInfo called: codePage={CodePage} lpCpInfo=0x{LpCpInfo:X8}", codePage, (nint)lpCpInfo.Value);

		if (lpCpInfo.Value == null)
		{
			_logger.LogWarning("[Kernel32] GetCPInfo: null pointer");
			return NativeTypes.Win32Bool.FALSE; // Return FALSE if null pointer
		}

		// Handle special code page values
		var actualCodePage = codePage switch
		{
			CodePage.Acp => GetAcp(), // CP_ACP - system default Windows ANSI code page
			CodePage.OemCp => GetOemCp(), // CP_OEMCP - system default OEM code page (we'll use same as ACP)
			_ => codePage
		};

		_logger.LogInformation("[Kernel32] GetCPInfo: actualCodePage={ActualCodePage}", actualCodePage);
		NativeTypes.Cpinfo cpInfo;

		// We'll support common Western code pages
		switch (actualCodePage)
		{
			case CodePage.WestEurope: // Windows-1252 (Western European)
			case CodePage.Oem437: // OEM United States
			case CodePage.OemMultilingualLatinI: // OEM Multilingual Latin I
			case CodePage.EastEurope: // Windows Central Europe
			case CodePage.Russian: // Windows Cyrillic
			case CodePage.Iso88591LatinI: // ISO 8859-1 Latin I
				// Single-byte code page setup
				cpInfo.MaxCharSize = 1;
				cpInfo.DefaultChar[0] = 0x3F; // '?' character
				cpInfo.DefaultChar[1] = 0x00; // Null terminator
				// LeadByte array - all zeros for single-byte code page
				for (var i = 0; i < 12; i++)
				{
					cpInfo.LeadByte[i] = 0;
				}

				break;

			case CodePage.Utf8: // UTF-8
				// UTF-8 is a multi-byte encoding with variable length (1-4 bytes per character)
				cpInfo.MaxCharSize = 4;
				cpInfo.DefaultChar[0] = 0x3F; // '?' character
				cpInfo.DefaultChar[1] = 0x00; // Null terminator
				// LeadByte array - all zeros for UTF-8 (no traditional lead bytes like DBCS)
				for (var i = 0; i < 12; i++)
				{
					cpInfo.LeadByte[i] = 0;
				}

				break;

			default:
				// Unsupported code page
				_logger.LogWarning("[Kernel32] GetCPInfo: unsupported code page {ActualCodePage}", actualCodePage);
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return NativeTypes.Win32Bool.FALSE;
		}

		// Write the CPINFO structure to emulated memory
		// Validate pointer before casting and writing
		var ptrValue = (ulong)lpCpInfo.Value;
		// Assume emulated memory is 32-bit addressable (0..0xFFFFFFFF)
		if (ptrValue is > uint.MaxValue or 0)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}

		_env.MemWriteStruct((uint)ptrValue, ref cpInfo);

		return NativeTypes.Win32Bool.TRUE;
	}

	[DllModuleExport(17)]
	private CodePage GetOemCp() => CodePage.Oem437; // IBM PC US (OEM code page)

	[DllModuleExport(21)]
	private unsafe uint GetStringTypeA(uint locale, uint dwInfoType, sbyte* lpSrcStr, int cchSrc, uint lpCharType)
	{
		// Maximum string length limit to prevent excessive memory usage and infinite loops
		const int maxStringLengthLimit = 1000;

		var srcStrAddr = (uint)(nint)lpSrcStr;
		if (srcStrAddr == 0 || lpCharType == 0)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}

		// We only support CT_CTYPE1 for simplicity
		if (dwInfoType != 1)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}

		// Determine the length of the string if cchSrc is -1
		var length = cchSrc;
		if (cchSrc == -1)
		{
			length = 0;
			// Safely calculate string length with bounds check
			while (length < maxStringLengthLimit)
			{
				var ch = _env.MemRead8(srcStrAddr + (uint)length);
				if (ch == 0)
				{
					break;
				}

				length++;
			}
		}

		// Validate length
		if (length is <= 0 or > maxStringLengthLimit)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}

		// Character type constants from Windows API
		const ushort ctCtype1Upper = 0x0001; // uppercase
		const ushort ctCtype1Lower = 0x0002; // lowercase
		const ushort ctCtype1Digit = 0x0004; // decimal digit
		const ushort ctCtype1Space = 0x0008; // space characters
		const ushort ctCtype1Punct = 0x0010; // punctuation
		const ushort ctCtype1Cntrl = 0x0020; // control characters
		const ushort ctCtype1Blank = 0x0040; // blank characters
		const ushort ctCtype1Xdigit = 0x0080; // hexadecimal digits
		const ushort ctCtype1Alpha = 0x0100; // any letter

		// Process each character
		for (var i = 0; i < length; i++)
		{
			var ch = _env.MemRead8(srcStrAddr + (uint)i);
			ushort charType = 0;

			// ASCII punctuation ranges:
			// '!'..'/'  (33-47): !"#$%&'()*+,-./
			// ':'..'@'  (58-64): :;<=>?@
			// '['..'`'  (91-96): [\]^_`
			// '{'..'~'  (123-126): {|}~
			const byte punctRange1Start = (byte)'!';
			const byte punctRange1End = (byte)'/';
			const byte punctRange2Start = (byte)':';
			const byte punctRange2End = (byte)'@';
			const byte punctRange3Start = (byte)'[';
			const byte punctRange3End = (byte)'`';
			const byte punctRange4Start = (byte)'{';
			const byte punctRange4End = (byte)'~';

			// Basic ASCII character classification
			if (ch >= 'A' && ch <= 'Z')
			{
				charType |= ctCtype1Upper | ctCtype1Alpha;
				if ((ch >= 'A' && ch <= 'F'))
				{
					charType |= ctCtype1Xdigit;
				}
			}
			else if (ch >= 'a' && ch <= 'z')
			{
				charType |= ctCtype1Lower | ctCtype1Alpha;
				if ((ch >= 'a' && ch <= 'f'))
				{
					charType |= ctCtype1Xdigit;
				}
			}
			else if (ch >= '0' && ch <= '9')
			{
				charType |= ctCtype1Digit | ctCtype1Xdigit;
			}
			else if (ch == ' ' || ch == '\t')
			{
				charType |= ctCtype1Space | ctCtype1Blank;
			}
			else if (ch == '\n' || ch == '\r' || ch == '\f' || ch == '\v')
			{
				charType |= ctCtype1Space;
			}
			else if (ch is <= 0x1F or 0x7F)
			{
				charType |= ctCtype1Cntrl;
			}
			else if (ch is >= punctRange1Start and <= punctRange1End or >= punctRange2Start and <= punctRange2End or >= punctRange3Start and <= punctRange3End or >= punctRange4Start and <= punctRange4End)
			{
				charType |= ctCtype1Punct;
			}

			// Write the character type to the output array (each entry is 2 bytes)
			_env.MemWrite16(lpCharType + (uint)(i * 2), charType);
		}

		return NativeTypes.Win32Bool.TRUE;
	}

	[DllModuleExport(22)]
	private uint GetStringTypeW(uint locale, uint dwInfoType, uint lpSrcStr, int cchSrc, uint lpCharType)
	{
		// GetStringTypeW retrieves character type information for Unicode characters
		// Similar to GetStringTypeA but for wide (Unicode) strings
		const int maxStringLengthLimit = 1000;

		if (lpSrcStr == 0 || lpCharType == 0)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}

		// We only support CT_CTYPE1 for simplicity
		if (dwInfoType != 1)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}

		// Determine the length of the string if cchSrc is -1
		var length = cchSrc;
		if (cchSrc == -1)
		{
			// Count characters until null terminator (wide char = 2 bytes)
			length = 0;
			var currentAddr = lpSrcStr;
			while (length < maxStringLengthLimit)
			{
				var wchar = _env.MemRead16(currentAddr);
				if (wchar == 0)
				{
					break;
				}

				length++;
				currentAddr += 2;
			}

			if (length >= maxStringLengthLimit)
			{
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return NativeTypes.Win32Bool.FALSE;
			}
		}

		// Use same character type constants as GetStringTypeA
		const ushort ctCtype1Upper = 0x0001;
		const ushort ctCtype1Lower = 0x0002;
		const ushort ctCtype1Digit = 0x0004;
		const ushort ctCtype1Space = 0x0008;
		const ushort ctCtype1Punct = 0x0010;
		const ushort ctCtype1Cntrl = 0x0020;
		const ushort ctCtype1Blank = 0x0040;
		const ushort ctCtype1Xdigit = 0x0080;
		const ushort ctCtype1Alpha = 0x0100;

		// Write character type information for each character
		for (var i = 0; i < length; i++)
		{
			var wchar = _env.MemRead16(lpSrcStr + (uint)(i * 2));
			ushort charType = 0;

			if (wchar is >= 'A' and <= 'Z')
			{
				charType = ctCtype1Upper | ctCtype1Alpha;
			}
			else if (wchar is >= 'a' and <= 'z')
			{
				charType = ctCtype1Lower | ctCtype1Alpha;
			}
			else if (wchar is >= '0' and <= '9')
			{
				charType = ctCtype1Digit;
			}
			else if (wchar is ' ' or '\t' or '\n' or '\r')
			{
				charType = ctCtype1Space;
				// Space and tab are also blank characters
				if (wchar is ' ' or '\t')
				{
					charType |= ctCtype1Blank;
				}
			}

			// Control characters (0x00-0x1F, 0x7F)
			if (wchar is >= 0x00 and <= 0x1F or 0x7F)
			{
				charType |= ctCtype1Cntrl;
			}

			// Hex digits
			if (wchar is >= '0' and <= '9' or >= 'A' and <= 'F' or >= 'a' and <= 'f')
			{
				charType |= ctCtype1Xdigit;
			}

			// Punctuation characters - same ranges as GetStringTypeA
			const ushort punctRange1Start = 0x21; // !
			const ushort punctRange1End = 0x2F; // /
			const ushort punctRange2Start = 0x3A; // :
			const ushort punctRange2End = 0x40; // @
			const ushort punctRange3Start = 0x5B; // [
			const ushort punctRange3End = 0x60; // `
			const ushort punctRange4Start = 0x7B; // {
			const ushort punctRange4End = 0x7E; // ~

			if (wchar is >= punctRange1Start and <= punctRange1End or >= punctRange2Start and <= punctRange2End or >= punctRange3Start and <= punctRange3End or >= punctRange4Start and <= punctRange4End)
			{
				charType |= ctCtype1Punct;
			}

			_env.MemWrite16(lpCharType + (uint)(i * 2), charType);
		}

		return NativeTypes.Win32Bool.TRUE;
	}

	/// <summary>
	/// Retrieves a module handle for the specified module. The module must have been loaded by the calling process.
	/// To avoid the race conditions described in the Remarks section, use the GetModuleHandleEx function.
	/// </summary>
	/// <param name="lpModuleName">
	/// The name of the loaded module (either a .dll or .exe file). If the file name extension is omitted, the default library extension .dll is appended. The file name string can include a trailing point character (.) to indicate that the module name has no extension. The string does not have to specify a path. When specifying a path, be sure to use backslashes (\), not forward slashes (/). The name is compared (case independently) to the names of modules currently mapped into the address space of the calling process.
	/// If this parameter is NULL, GetModuleHandle returns a handle to the file used to create the calling process (.exe file).
	/// The GetModuleHandle function does not retrieve handles for modules that were loaded using the LOAD_LIBRARY_AS_DATAFILE flag. For more information, see LoadLibraryEx.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is a handle to the specified module.
	/// If the function fails, the return value is NULL. To get extended error information, call GetLastError.
	/// </returns>
	/// <remarks>
	/// The returned handle is not global or inheritable. It cannot be duplicated or used by another process.
	/// If lpModuleName does not include a path and there is more than one loaded module with the same base name and extension, you cannot predict which module handle will be returned. To work around this problem, you could specify a path, use side-by-side assemblies, or use GetModuleHandleEx to specify a memory location rather than a DLL name.
	/// The GetModuleHandle function returns a handle to a mapped module without incrementing its reference count. However, if this handle is passed to the FreeLibrary function, the reference count of the mapped module will be decremented. Therefore, do not pass a handle returned by GetModuleHandle to the FreeLibrary function. Doing so can cause a DLL module to be unmapped prematurely.
	/// This function must be used carefully in a multithreaded application. There is no guarantee that the module handle remains valid between the time this function returns the handle and the time it is used. For example, suppose that a thread retrieves a module handle, but before it uses the handle, a second thread frees the module. If the system loads another module, it could reuse the module handle that was recently freed. Therefore, the first thread would have a handle to a different module than the one intended.
	/// </remarks>
	[DllModuleExport(16)]
	private uint GetModuleHandleA(in LpcStr lpModuleName)
	{
		var moduleName = lpModuleName.ToString();
		_logger.LogInformation("[Kernel32] GetModuleHandleA called: module='{ModuleName}'", moduleName ?? "NULL (current process)");

		// NULL means get handle to current process executable
		if (string.IsNullOrEmpty(moduleName))
		{
			_logger.LogDebug("[Kernel32] GetModuleHandleA returning current process handle: 0x{ImageBase:X8}", _imageBase);
			return _imageBase;
		}

		// Normalize the module name (remove path, make uppercase, ensure .DLL extension)
		var normalizedName = Path.GetFileName(moduleName).ToUpperInvariant();
		if (!normalizedName.EndsWith(".DLL", StringComparison.OrdinalIgnoreCase))
		{
			normalizedName += ".DLL";
		}

		// Check if this is a system DLL that we emulate by checking if it has any exports
		// registered in the source-generated DllModuleExportInfo
		var exports = DllModuleExportInfo.GetAllExports(normalizedName);
		var isSystemDll = exports.Count > 0;

		if (isSystemDll || _env.IsModuleLoaded(normalizedName))
		{
			// Load/register the module and get its handle
			// LoadModule returns existing handle if already loaded
			var handle = _env.LoadModule(normalizedName);
			_logger.LogDebug("[Kernel32] GetModuleHandleA returning handle for {NormalizedName}: 0x{Handle:X8}", normalizedName, handle);
			return handle;
		}

		// Module not found
		_logger.LogWarning("[Kernel32] GetModuleHandleA: module '{ModuleName}' not found", moduleName);
		_lastError = NativeTypes.Win32Error.ERROR_MOD_NOT_FOUND;
		return 0;
	}

	/// <summary>
	/// Loads the specified module into the address space of the calling process. The specified module may cause other modules to be loaded.
	/// For additional load options, use the LoadLibraryEx function.
	/// </summary>
	/// <param name="lpLibFileName">
	/// The name of the module. This can be either a library module (a .dll file) or an executable module (an .exe file).
	/// If the string specifies a full path, the function searches only that path for the module.
	/// If the string specifies a relative path or a module name without a path, the function uses a standard search strategy to find the module.
	/// If the string specifies a module name without a path and the file name extension is omitted, the function appends the default library extension ".DLL" to the module name.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is a handle to the module.
	/// If the function fails, the return value is NULL. To get extended error information, call GetLastError.
	/// </returns>
	/// <remarks>
	/// LoadLibrary can be used to load a library module into the address space of the process and return a handle that can be used in GetProcAddress to get the address of a DLL function.
	/// If the specified module is a DLL that is not already loaded for the calling process, the system calls the DLL's DllMain function with the DLL_PROCESS_ATTACH value.
	/// The system maintains a per-process reference count on all loaded modules. Calling LoadLibrary increments the reference count.
	/// Module handles are not global or inheritable. A call to LoadLibrary by one process does not produce a handle that another process can use.
	/// </remarks>
	[DllModuleExport(32)]
	private uint LoadLibraryA(in LpcStr lpLibFileName)
	{
		if (lpLibFileName.IsNull)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}

		// Read the library name from memory
		var libraryName = lpLibFileName.ToString();
		if (string.IsNullOrEmpty(libraryName))
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}

		// Get the directory of the current executable
		var executablePath = _env.ExecutablePath;
		var executableDir = Path.GetDirectoryName(executablePath) ?? string.Empty;

		// Check if the library is local to the executable path
		var localLibraryPath = Path.Combine(executableDir, libraryName);
		var isLocalDll = File.Exists(localLibraryPath);

		if (isLocalDll)
		{
			// DLL is local to executable path - load it using PeImageLoader for proper emulation
			_logger.LogInformation("[Kernel32] Loading local DLL for emulation: {LibraryName}", libraryName);

			// Register with dispatcher for function call tracking
			_dispatcher?.RegisterDynamicallyLoadedDll(libraryName);

			if (_peLoader != null)
			{
				return _env.LoadPeImage(localLibraryPath, _peLoader);
			}

			_logger.LogInformation("[Kernel32] Warning: PeImageLoader not available, falling back to module tracking for {LibraryName}", libraryName);
			return _env.LoadModule(libraryName);
		}

		// DLL is not local - thunk to emulator's win32 syscall implementation
		// For system DLLs like kernel32.dll, user32.dll, etc., we return a fake handle
		// but the actual implementation will be handled by the dispatcher
		_logger.LogInformation("[Kernel32] Loading system DLL via thunking: {LibraryName}", libraryName);

		// Register with dispatcher for function call tracking
		_dispatcher?.RegisterDynamicallyLoadedDll(libraryName);

		// For system libraries, we still need to track them but mark them as system modules
		return _env.LoadModule(libraryName);
	}

	/// <summary>
	/// Retrieves the address of an exported function (also known as a procedure) or variable from the specified dynamic-link library (DLL).
	/// </summary>
	/// <param name="hModule">
	/// A handle to the DLL module that contains the function or variable. 
	/// The LoadLibrary, LoadLibraryEx, LoadPackagedLibrary, or GetModuleHandle function returns this handle.
	/// The GetProcAddress function does not retrieve addresses from modules that were loaded using the LOAD_LIBRARY_AS_DATAFILE flag.
	/// </param>
	/// <param name="lpProcName">
	/// The function or variable name, or the function's ordinal value. If this parameter is an ordinal value, it must be in the low-order word; the high-order word must be zero.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is the address of the exported function or variable.
	/// If the function fails, the return value is NULL. To get extended error information, call GetLastError.
	/// </returns>
	/// <remarks>
	/// The spelling and case of a function name pointed to by lpProcName must be identical to that in the EXPORTS statement of the source DLL's module-definition (.def) file.
	/// The lpProcName parameter can identify the DLL function by specifying an ordinal value associated with the function in the EXPORTS statement.
	/// GetProcAddress verifies that the specified ordinal is in the range 1 through the highest ordinal value exported in the .def file.
	/// If the function might not exist in the DLL module, specify the function by name rather than by ordinal value.
	/// </remarks>
	[DllModuleExport(18)]
	private uint GetProcAddress(uint hModule, LpcStr lpProcName)
	{
		// GetProcAddress retrieves the address of an exported function from a DLL
		// hModule: module handle from LoadLibraryA or GetModuleHandleA
		// lpProcName: either a string pointer (name) or an ordinal value (LOWORD)

		_logger.LogInformation("[Kernel32] GetProcAddress(0x{HModule:X8}, 0x{LpProcName:X8})", hModule, lpProcName.Address);

		if (hModule == 0)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}

		string? procName = null;
		var byOrdinal = false;

		// Check if lpProcName is an ordinal (high word is 0)
		uint ordinal = 0;
		if ((lpProcName.Address & 0xFFFF0000) == 0)
		{
			ordinal = lpProcName.Address & 0xFFFF;
			byOrdinal = true;
			_logger.LogInformation("[Kernel32] GetProcAddress: Looking up by ordinal {Ordinal}", ordinal);
		}
		else
		{
			// It's a string pointer
			procName = lpProcName.ToString();
			_logger.LogInformation("[Kernel32] GetProcAddress: Looking up '{ProcName}'", procName);
		}

		// Try to find the module in loaded PE images first
		if (_env.TryGetLoadedImage(hModule, out var loadedImage) && loadedImage != null)
		{
			uint exportAddress = 0;
			string? forwarderName = null;

			// Look up by ordinal or name in the real PE export table
			if (byOrdinal)
			{
				if (loadedImage.ExportsByOrdinal.TryGetValue(ordinal, out exportAddress))
				{
					_logger.LogInformation("[Kernel32] GetProcAddress: Found export by ordinal {Ordinal} at 0x{ExportAddress:X8}", ordinal, exportAddress);
					return exportAddress;
				}

				// Check if it's a forwarded export
				if (loadedImage.ForwardedExportsByOrdinal.TryGetValue(ordinal, out forwarderName))
				{
					_logger.LogInformation("[Kernel32] GetProcAddress: Found forwarded export by ordinal {Ordinal} -> {ForwarderName}", ordinal, forwarderName);
					return ResolveForwardedExport(forwarderName);
				}
			}
			else if (procName != null)
			{
				if (loadedImage.ExportsByName.TryGetValue(procName, out exportAddress))
				{
					_logger.LogInformation("[Kernel32] GetProcAddress: Found export '{ProcName}' at 0x{ExportAddress:X8}", procName, exportAddress);
					return exportAddress;
				}

				// Check if it's a forwarded export
				if (loadedImage.ForwardedExportsByName.TryGetValue(procName, out forwarderName))
				{
					_logger.LogInformation("[Kernel32] GetProcAddress: Found forwarded export '{ProcName}' -> {ForwarderName}", procName, forwarderName);
					return ResolveForwardedExport(forwarderName);
				}
			}

			// Export not found in PE image
			_logger.LogInformation("[Kernel32] GetProcAddress: Export not found in PE image");
			_lastError = NativeTypes.Win32Error.ERROR_PROC_NOT_FOUND;
			return 0;
		}

		// Not in loaded images - check if it's an emulated module
		var moduleName = _env.GetModuleFileNameForHandle(hModule);
		if (moduleName == null)
		{
			_logger.LogInformation("[Kernel32] GetProcAddress: Module handle 0x{HModule:X8} not recognized", hModule);
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
			return 0;
		}

		// Try to get the emulated module from the dispatcher
		if (_dispatcher == null || !_dispatcher.TryGetModule(moduleName, out var emulatedModule) || emulatedModule == null)
		{
			_logger.LogInformation("[Kernel32] GetProcAddress: Emulated module '{ModuleName}' not found in dispatcher", moduleName);
			_lastError = NativeTypes.Win32Error.ERROR_MOD_NOT_FOUND;
			return 0;
		}

		// Use DllModuleExportInfo to check if the export exists before looking up
		string? exportName = null;

		if (byOrdinal)
		{
			// Find export by ordinal
			var exportEntry = DllModuleExportInfo.GetAllExports(emulatedModule.Name).FirstOrDefault(kvp => kvp.Value == ordinal);
			if (exportEntry.Key != null)
			{
				exportName = exportEntry.Key;
			}
		}
		else if (procName != null)
		{
			// Check if export is implemented using DllModuleExportInfo
			if (DllModuleExportInfo.IsExportImplemented(moduleName, procName))
			{
				exportName = procName;
			}
		}

		if (exportName == null)
		{
			_logger.LogInformation("[Kernel32] GetProcAddress: Export not found in emulated module '{ModuleName}'", moduleName);
			_lastError = NativeTypes.Win32Error.ERROR_PROC_NOT_FOUND;
			return 0;
		}

		// Check if this export is forwarded to another DLL
		var forwardedTo = DllModuleExportInfo.GetForwardedExport(moduleName, exportName);
		if (forwardedTo != null)
		{
			_logger.LogInformation("[Kernel32] GetProcAddress: Found forwarded export '{ModuleName}!{ExportName}' -> {ForwardedTo}", moduleName, exportName, forwardedTo);
			return ResolveForwardedExport(forwardedTo);
		}

		// Register and return a synthetic export address
		var syntheticAddress = _env.RegisterSyntheticExport(moduleName, exportName);
		_logger.LogInformation("[Kernel32] GetProcAddress: Registered synthetic export '{ModuleName}!{ExportName}' at 0x{SyntheticAddress:X8}", moduleName, exportName, syntheticAddress);
		return syntheticAddress;
	}

	/// <summary>
	/// Resolves a forwarded export to its actual address.
	/// Forwarded exports have the format "DLL.ExportName" or "DLL.DLL.ExportName".
	/// </summary>
	private uint ResolveForwardedExport(string forwarderName)
	{
		// Parse the forwarder string (format: "DLL.ExportName" or "DLL.DLL.ExportName")
		var parts = forwarderName.Split('.');
		if (parts.Length < 2)
		{
			_logger.LogInformation("[Kernel32] ResolveForwardedExport: Invalid forwarder format '{ForwarderName}'", forwarderName);
			_lastError = NativeTypes.Win32Error.ERROR_PROC_NOT_FOUND;
			return 0;
		}

		// Extract DLL name and export name
		string targetDll;
		string targetExport;

		if (parts.Length == 2)
		{
			// Format: "DLL.ExportName"
			targetDll = parts[0] + ".DLL";
			targetExport = parts[1];
		}
		else
		{
			// Format: "DLL.DLL.ExportName" or assume first part is DLL, rest is export
			// Check if second part is "DLL"
			if (parts[1].Equals("DLL", StringComparison.OrdinalIgnoreCase))
			{
				targetDll = parts[0] + "." + parts[1];
				targetExport = string.Join(".", parts.Skip(2));
			}
			else
			{
				targetDll = parts[0] + ".DLL";
				targetExport = string.Join(".", parts.Skip(1));
			}
		}

		_logger.LogInformation("[Kernel32] ResolveForwardedExport: Resolving '{ForwarderName}' -> {TargetDll}!{TargetExport}", forwarderName, targetDll, targetExport);

		// Try to get the target module handle
		var targetModuleHandle = _env.LoadModule(targetDll);
		if (targetModuleHandle == 0)
		{
			_logger.LogInformation("[Kernel32] ResolveForwardedExport: Failed to load target module '{TargetDll}'", targetDll);
			_lastError = NativeTypes.Win32Error.ERROR_MOD_NOT_FOUND;
			return 0;
		}

		// Write the export name to a temporary location in memory
		var exportNamePtr = _env.WriteAnsiString(targetExport);

		// Recursively call GetProcAddress to resolve the forwarded export
		var result = GetProcAddress(targetModuleHandle, exportNamePtr);

		return result;
	}

	[DllModuleExport(15)]
	private unsafe uint GetModuleFileNameA(void* h, sbyte* lp, uint n)
	{
		_logger.LogInformation("[Kernel32] GetModuleFileNameA called: h=0x{U:X8} lp=0x{Lp:X8} n={U1}", (uint)(nint)h, (uint)(nint)lp, n);

		// Use guest memory helpers instead of dereferencing raw pointers to avoid AccessViolation
		if (n == 0 || lp == null)
		{
			_logger.LogWarning("[Kernel32] GetModuleFileNameA returning 0 (invalid params)");
			return 0;
		}

		// Convert lp to guest address
		var lpAddr = (uint)(nint)lp;
		if (lpAddr == 0)
		{
			return 0;
		}

		string? path = null;

		if (h == null || (IntPtr)h == IntPtr.Zero)
		{
			path = ReadCurrentModulePath();
		}
		else
		{
			if ((ulong)(nint)h == 0xFFFFFFFFul)
			{
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return 0;
			}

			var numericHandle = (uint)(nint)h;
			var moduleName = _env.GetModuleFileNameForHandle(numericHandle);
			if (moduleName != null)
			{
				path = moduleName;
			}
			else
			{
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return 0;
			}
		}

		if (path == null)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}

		_logger.LogDebug("[Kernel32] GetModuleFileNameA resolved path: {Path}", path);

		// Ensure path doesn't have any backslashes before quotes that could cause parsing issues
		path = FixPathEscaping(path);

		// Convert to Windows-style path
		var windowsPath = ConvertToWindowsPath(path);
		_logger.LogDebug("[Kernel32] GetModuleFileNameA converted to Windows path: {WindowsPath}", windowsPath);

		var bytes = Encoding.ASCII.GetBytes(windowsPath);
		var required = (uint)bytes.Length; // number of chars without null

		// If buffer too small, copy up to n-1 and null terminate
		if (n <= required)
		{
			var copyLen = n > 0 ? n - 1u : 0u;
			if (copyLen > 0)
			{
				_env.MemWriteBytes(lpAddr, bytes.AsSpan(0, (int)copyLen));
				Diagnostics.Diagnostics.LogMemWrite(lpAddr, (int)copyLen, bytes.AsSpan(0, (int)copyLen).ToArray());
			}

			// write null terminator
			_env.MemWriteBytes(lpAddr + copyLen, [0]);
			_lastError = NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
			_logger.LogDebug("[Kernel32] GetModuleFileNameA truncated; copyLen={CopyLen} returned", copyLen);
			return copyLen;
		}

		// Fits in buffer: write full path and null terminator
		_env.MemWriteBytes(lpAddr, bytes);
		_env.MemWriteBytes(lpAddr + (uint)bytes.Length, [0]);
		Diagnostics.Diagnostics.LogMemWrite(lpAddr, bytes.Length + 1, bytes.AsSpan(0, bytes.Length).ToArray());

		var returnLength = (uint)bytes.Length;
		_logger.LogInformation("[Kernel32] GetModuleFileNameA returning {ReturnLength}", returnLength);

		return returnLength;
	}

	/// <summary>
	/// Fixes path escaping issues that can cause parsing problems
	/// </summary>
	private string FixPathEscaping(string path)
	{
		// Replace any problematic sequences that might cause parsing issues
		// Specifically, ensure backslashes before quotes are properly escaped

		// First, normalize all backslashes to single backslashes
		var result = new StringBuilder();
		var inQuote = false;

		for (var i = 0; i < path.Length; i++)
		{
			var c = path[i];

			switch (c)
			{
				case '\\':
				{
					// Count consecutive backslashes
					var backslashCount = 1;
					while (i + 1 < path.Length && path[i + 1] == '\\')
					{
						backslashCount++;
						i++;
					}

					// Check if next char is a quote
					if (i + 1 < path.Length && path[i + 1] == '"')
					{
						// For backslashes before quotes, ensure they're properly escaped
						// In Windows, each backslash before a quote needs to be doubled
						for (var j = 0; j < backslashCount; j++)
						{
							result.Append('\\');
						}
					}
					else
					{
						// For regular backslashes, just add them normally
						for (var j = 0; j < backslashCount; j++)
						{
							result.Append('\\');
						}
					}

					break;
				}
				case '"':
				{
					// Toggle quote state and add the quote
					inQuote = !inQuote;

					// Ensure quotes are properly escaped
					if (result.Length > 0 && result[result.Length - 1] != '\\')
					{
						// Add a backslash before the quote if there isn't one already
						result.Append('\\');
					}

					result.Append(c);
					break;
				}
				default:
					// Regular character
					result.Append(c);
					break;
			}
		}

		return result.ToString();
	}

	/// <summary>
	/// Retrieves the command-line string for the current process (ANSI version).
	/// </summary>
	/// <returns>
	/// The return value is a pointer to the command-line string for the current process.
	/// The lifetime of the returned value is managed by the system, applications should not free or modify this value.
	/// </returns>
	/// <remarks>
	/// Console processes can use the argc and argv arguments of the main or wmain functions by implementing those as the program entry point.
	/// GUI processes can use the lpCmdLine argument of the WinMain or wWinMain functions by implementing those as the program entry point.
	/// The name of the executable in the command line that the operating system provides to a process is not necessarily identical to that in the command line that the calling process gives to the CreateProcess function.
	/// </remarks>
	[DllModuleExport(8)]
	public uint GetCommandLineA()
	{
		var ptr = _env.CommandLinePtr;
		if (ptr != 0)
		{
			// Read the command line string for logging
			var cmdLine = _env.ReadAnsiString(ptr);

			// Fix command line escaping issues that can cause infinite loops in function 412440
			var fixedCmdLine = FixCommandLineEscaping(cmdLine);

			// Convert to Windows-style path and update in memory
			var windowsPath = ConvertToWindowsPath(fixedCmdLine);
			if (windowsPath != cmdLine)
			{
				// Write the Windows-style path back to memory
				var bytes = Encoding.ASCII.GetBytes(windowsPath);
				_env.MemWriteBytes(ptr, bytes);
				_env.MemWriteBytes(ptr + (uint)bytes.Length, [0]); // Null terminator

				// Update logging to show the converted path
				_logger.LogInformation("[Kernel32] GetCommandLineA returning 0x{Ptr:X8}: \"{CmdLine}\" (converted from \"{OrigPath}\")",
					ptr, windowsPath, cmdLine);
			}
			else
			{
				_logger.LogInformation("[Kernel32] GetCommandLineA returning 0x{Ptr:X8}: \"{CmdLine}\"", ptr, cmdLine);
			}
		}

		return ptr;
	}

	[DllModuleExport(12)]
	public uint GetEnvironmentStringsW()
	{
		// Return pointer to Unicode environment strings block
		// This will be obtained from emulated environment variables, not system ones
		return _env.GetEnvironmentStringsW();
	}

	[DllModuleExport(6)]
	private uint FreeEnvironmentStringsW(uint lpszEnvironmentBlock)
	{
		// In the Windows API, FreeEnvironmentStringsW frees the memory allocated by GetEnvironmentStringsW
		// However, our emulator uses a simple bump allocator that doesn't support freeing individual blocks
		// For API compatibility, we accept the call and always return success (TRUE)
		// The memory will be cleaned up when the process terminates

		// Validate that the pointer is not null (basic error checking)
		if (lpszEnvironmentBlock == 0)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}

		// Return success - in a real implementation this would free the memory
		return NativeTypes.Win32Bool.TRUE;
	}

	[DllModuleExport(11)]
	private uint GetEnvironmentStringsA()
	{
		// Return pointer to ANSI environment strings block
		// This will be obtained from emulated environment variables, not system ones
		return _env.GetEnvironmentStringsA();
	}

	[DllModuleExport(5)]
	private uint FreeEnvironmentStringsA(uint lpszEnvironmentBlock)
	{
		// In the Windows API, FreeEnvironmentStringsA frees the memory allocated by GetEnvironmentStringsA
		// However, our emulator uses a simple bump allocator that doesn't support freeing individual blocks
		// For API compatibility, we accept the call and always return success (TRUE)
		// The memory will be cleaned up when the process terminates

		// Validate that the pointer is not null (basic error checking)
		if (lpszEnvironmentBlock == 0)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}

		// Return success - in a real implementation this would free the memory
		return NativeTypes.Win32Bool.TRUE;
	}

	[DllModuleExport(19)]
	private uint GetStartupInfoA(uint lpStartupInfo)
	{
		if (lpStartupInfo == 0)
		{
			return 0;
		}

		_env.MemZero(lpStartupInfo, 68);
		_env.MemWrite32(lpStartupInfo + 0, 68);
		// Write actual handle values, not pseudo-handle constants
		// When a console is allocated, these should be real inheritable handles
		// When no console exists, these will be 0 (NULL)
		_env.MemWrite32(lpStartupInfo + 56, _env.StdInputHandle);
		_env.MemWrite32(lpStartupInfo + 60, _env.StdOutputHandle);
		_env.MemWrite32(lpStartupInfo + 64, _env.StdErrorHandle);
		return 0;
	}

	[DllModuleExport(20)]
	private uint GetStdHandle(uint nStdHandle)
	{
		return nStdHandle switch
		{
			0xFFFFFFF6 => _env.StdInputHandle, //STD_INPUT_HANDLE ((DWORD)-10)
			0xFFFFFFF5 => _env.StdOutputHandle, //STD_OUTPUT_HANDLE ((DWORD)-11)
			0xFFFFFFF4 => _env.StdErrorHandle, //STD_ERROR_HANDLE ((DWORD)-12)
			_ => 0
		};
	}

	[DllModuleExport(42)]
	private uint SetStdHandle(uint nStdHandle, uint hHandle)
	{
		switch (nStdHandle)
		{
			case 0xFFFFFFF6: _env.StdInputHandle = hHandle; break;
			case 0xFFFFFFF5: _env.StdOutputHandle = hHandle; break;
			case 0xFFFFFFF4: _env.StdErrorHandle = hHandle; break;
		}

		return 1;
	}

	[DllModuleExport(1)]
	private uint AllocConsole()
	{
		_logger.LogInformation("[Kernel32] AllocConsole()");

		var success = _env.AllocateConsole();
		if (!success)
		{
			// Console already exists
			_lastError = 5; // ERROR_ACCESS_DENIED
			return 0; // FALSE
		}

		return 1; // TRUE
	}

	[DllModuleExport(1)]
	private uint FreeConsole()
	{
		_logger.LogInformation("[Kernel32] FreeConsole()");

		var success = _env.FreeConsole();
		if (!success)
		{
			// No console to free
			_lastError = 6; // ERROR_INVALID_HANDLE
			return 0; // FALSE
		}

		return 1; // TRUE
	}

	[DllModuleExport(1)]
	private uint AttachConsole(uint dwProcessId)
	{
		_logger.LogInformation("[Kernel32] AttachConsole(dwProcessId={DwProcessId})", dwProcessId);

		// dwProcessId == 0xFFFFFFFF means attach to parent process console
		// For emulation, we just allocate a console if one doesn't exist

		if (_env.HasConsole)
		{
			// Already has a console
			_lastError = 5; // ERROR_ACCESS_DENIED
			return 0; // FALSE
		}

		var success = _env.AllocateConsole();
		if (!success)
		{
			_lastError = 5; // ERROR_ACCESS_DENIED
			return 0; // FALSE
		}

		return 1; // TRUE
	}

	[DllModuleExport(24)]
	private uint GlobalAlloc(uint flags, uint bytes) => _env.SimpleAlloc(bytes == 0 ? 1u : bytes);

	[DllModuleExport(25)]
	private static unsafe uint GlobalFree(void* h) => 0;

	[DllModuleExport(1)]
	private static unsafe uint GlobalLock(void* hMem)
	{
		// GlobalLock locks a global memory object and returns a pointer to it
		// In our simplified implementation, we just return the handle as a pointer
		// since memory is already accessible
		return (uint)hMem;
	}

	[DllModuleExport(1)]
	private static unsafe uint GlobalUnlock(void* hMem)
	{
		// GlobalUnlock decrements the lock count
		// Returns TRUE (1) if still locked, FALSE (0) if unlocked
		// In our simplified implementation, always return TRUE
		return NativeTypes.Win32Bool.TRUE;
	}

	[DllModuleExport(1)]
	private static unsafe uint GlobalHandle(void* pMem)
	{
		// GlobalHandle retrieves the handle associated with a locked memory pointer
		// In our simplified implementation, the handle is the same as the pointer
		return (uint)pMem;
	}

	/// <summary>
	/// Creates a private heap object that can be used by the calling process. The function reserves space in the virtual address space of the process and allocates physical storage for a specified initial portion of this block.
	/// </summary>
	/// <param name="flOptions">
	/// The heap allocation options. This parameter can be 0 or one or more of the following values:
	/// HEAP_CREATE_ENABLE_EXECUTE (0x00040000), HEAP_GENERATE_EXCEPTIONS (0x00000004), HEAP_NO_SERIALIZE (0x00000001).
	/// </param>
	/// <param name="dwInitialSize">
	/// The initial size of the heap, in bytes. This value determines the initial amount of memory that is committed for the heap.
	/// The value is rounded up to a multiple of the system page size. If this parameter is 0, the function commits one page.
	/// </param>
	/// <param name="dwMaximumSize">
	/// The maximum size of the heap, in bytes. If dwMaximumSize is not zero, the heap size is fixed and cannot grow beyond the maximum size.
	/// If dwMaximumSize is 0, the heap can grow in size. The heap's size is limited only by the available memory.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is a handle to the newly created heap.
	/// If the function fails, the return value is NULL. To get extended error information, call GetLastError.
	/// </returns>
	/// <remarks>
	/// The HeapCreate function creates a private heap object from which the calling process can allocate memory blocks by using the HeapAlloc function.
	/// The memory of a private heap object is accessible only to the process that created it.
	/// </remarks>
	[DllModuleExport(27)]
	private uint HeapCreate(uint flOptions, uint dwInitialSize, uint dwMaximumSize) =>
		_env.HeapCreate(flOptions, dwInitialSize, dwMaximumSize);

	[DllModuleExport(26)]
	private unsafe uint HeapAlloc(void* hHeap, uint dwFlags, uint dwBytes) => _env.HeapAlloc((uint)hHeap, dwBytes);

	[DllModuleExport(29)]
	private static unsafe uint HeapFree(void* hHeap, uint dwFlags, void* lpMem) => 1;

	[DllModuleExport(1)]
	private unsafe uint HeapReAlloc(void* hHeap, uint dwFlags, void* lpMem, uint dwBytes)
	{
		// HeapReAlloc reallocates a memory block from a heap
		// This implementation properly copies old data and frees the old block

		try
		{
			if (lpMem == null)
			{
				// If lpMem is null, HeapReAlloc acts like HeapAlloc
				var alloc = _env.HeapAlloc((uint)hHeap, dwBytes);
				_logger.LogInformation("[Kernel32] HeapReAlloc: lpMem is null, allocated new block at 0x{Alloc:X8}, size={DwBytes}", alloc, dwBytes);
				return alloc;
			}

			// Get the size of the original allocation
			var originalSize = _env.HeapSize((uint)hHeap, (uint)lpMem);
			if (originalSize == 0)
			{
				// If we don't have size info, this might be an invalid pointer
				_logger.LogWarning("[Kernel32] HeapReAlloc: Could not determine size of block at 0x{LpMem:X8}", (uint)lpMem);
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return 0;
			}

			// Allocate new block
			var newMem = _env.HeapAlloc((uint)hHeap, dwBytes);
			if (newMem == 0)
			{
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return 0;
			}

			// Copy the data from the old block to the new block
			var bytesToCopy = Math.Min(originalSize, dwBytes);
			if (bytesToCopy > 0)
			{
				// Copy using memory operations
				var buffer = new byte[bytesToCopy];
				for (uint i = 0; i < bytesToCopy; i++)
				{
					buffer[i] = _env.MemRead8((uint)lpMem + i);
				}

				_env.MemWriteBytes(newMem, buffer);
			}

			// Free the old block
			_env.HeapFree((uint)hHeap, (uint)lpMem);

			_logger.LogInformation("[Kernel32] HeapReAlloc: Reallocated from 0x{LpMem:X8} (size={OriginalSize}) to 0x{NewMem:X8} (size={DwBytes}), copied {BytesToCopy} bytes", (uint)lpMem, originalSize, newMem, dwBytes, bytesToCopy);
			return newMem;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] HeapReAlloc failed: {ExMessage}", ex.Message);
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}
	}

	[DllModuleExport(28)]
	private unsafe uint HeapDestroy(void* hHeap)
	{
		// HeapDestroy destroys a heap created with HeapCreate
		// In our simple allocator, we don't actually manage individual heaps
		// Just return success for API compatibility
		_logger.LogInformation("[Kernel32] HeapDestroy(0x{HHeap:X8})", (uint)(nint)hHeap);

		if (hHeap == null)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}

		return NativeTypes.Win32Bool.TRUE;
	}

	[DllModuleExport(61)]
	private uint LocalAlloc(uint uFlags, uint uBytes)
	{
		_logger.LogInformation("[Kernel32] LocalAlloc(uFlags=0x{UFlags:X}, uBytes={UBytes})", uFlags, uBytes);
		// In modern Windows, LocalAlloc is implemented by the heap allocator.
		// We can just use our simple allocator.
		return _env.SimpleAlloc(uBytes == 0 ? 1u : uBytes);
	}

	/// <summary>
	/// Reserves, commits, or changes the state of a region of pages in the virtual address space of the calling process.
	/// Memory allocated by this function is automatically initialized to zero.
	/// </summary>
	/// <param name="lpAddress">
	/// The starting address of the region to allocate. If the memory is being reserved, the specified address is rounded down to the nearest multiple of the allocation granularity.
	/// If this parameter is NULL, the system determines where to allocate the region.
	/// </param>
	/// <param name="dwSize">
	/// The size of the region, in bytes. If the lpAddress parameter is NULL, this value is rounded up to the next page boundary.
	/// </param>
	/// <param name="flAllocationType">
	/// The type of memory allocation. This parameter must contain one of the following values:
	/// MEM_COMMIT (0x00001000), MEM_RESERVE (0x00002000), MEM_RESET (0x00080000), MEM_RESET_UNDO (0x1000000).
	/// It can also specify: MEM_LARGE_PAGES (0x20000000), MEM_PHYSICAL (0x00400000), MEM_TOP_DOWN (0x00100000), MEM_WRITE_WATCH (0x00200000).
	/// </param>
	/// <param name="flProtect">
	/// The memory protection for the region of pages to be allocated. If the pages are being committed, you can specify any one of the memory protection constants.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is the base address of the allocated region of pages.
	/// If the function fails, the return value is NULL. To get extended error information, call GetLastError.
	/// </returns>
	/// <remarks>
	/// Each page has an associated page state. The VirtualAlloc function can perform the following operations:
	/// Commit a region of reserved pages, Reserve a region of free pages, or Simultaneously reserve and commit a region of free pages.
	/// VirtualAlloc cannot reserve a reserved page. It can commit a page that is already committed.
	/// </remarks>
	[DllModuleExport(45)]
	private uint VirtualAlloc(uint lpAddress, uint dwSize, uint flAllocationType, uint flProtect) =>
		_env.VirtualAlloc(lpAddress, dwSize, flAllocationType, flProtect);

	[DllModuleExport(46)]
	private uint VirtualFree(uint lpAddress, uint dwSize, uint dwFreeType)
	{
		// VirtualFree releases or decommits virtual memory
		// dwFreeType: MEM_DECOMMIT (0x4000) or MEM_RELEASE (0x8000)
		// For simplicity in our emulator, we accept the call but don't actually free memory
		// The bump allocator doesn't support freeing
		_logger.LogInformation("[Kernel32] VirtualFree(0x{LpAddress:X8}, {DwSize}, 0x{DwFreeType:X})", lpAddress, dwSize, dwFreeType);

		const uint memDecommit = 0x4000;
		const uint memRelease = 0x8000;

		// Validate parameters
		if (lpAddress == 0)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}

		// When using MEM_RELEASE, dwSize must be 0
		if ((dwFreeType & memRelease) != 0 && dwSize != 0)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}

		// Return success - memory will be cleaned up when process terminates
		return NativeTypes.Win32Bool.TRUE;
	}

	/// <summary>
	/// Creates or opens a file or I/O device. The most commonly used I/O devices are: file, file stream, directory, physical disk, volume, console buffer, tape drive, communications resource, mailslot, and pipe.
	/// </summary>
	/// <param name="lpFileName">
	/// The name of the file or device to be created or opened.
	/// </param>
	/// <param name="dwDesiredAccess">
	/// The requested access to the file or device, which can be summarized as read, write, both or 0 to indicate neither.
	/// The most commonly used values are GENERIC_READ, GENERIC_WRITE, or both (GENERIC_READ | GENERIC_WRITE).
	/// </param>
	/// <param name="dwShareMode">
	/// The requested sharing mode of the file or device, which can be read, write, both, delete, all of these, or none.
	/// If this parameter is zero and CreateFile succeeds, the file or device cannot be shared and cannot be opened again until the handle is closed.
	/// </param>
	/// <param name="lpSecAttr">
	/// A pointer to a SECURITY_ATTRIBUTES structure that contains an optional security descriptor and a Boolean value that determines whether the returned handle can be inherited.
	/// This parameter can be NULL.
	/// </param>
	/// <param name="dwCreationDisposition">
	/// An action to take on a file or device that exists or does not exist.
	/// This parameter must be one of the following values: CREATE_NEW (1), CREATE_ALWAYS (2), OPEN_EXISTING (3), OPEN_ALWAYS (4), or TRUNCATE_EXISTING (5).
	/// </param>
	/// <param name="dwFlagsAndAttributes">
	/// The file or device attributes and flags. FILE_ATTRIBUTE_NORMAL being the most common default value for files.
	/// </param>
	/// <param name="hTemplateFile">
	/// A valid handle to a template file with the GENERIC_READ access right. The template file supplies file attributes and extended attributes for the file that is being created.
	/// This parameter can be NULL.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is an open handle to the specified file, device, named pipe, or mail slot.
	/// If the function fails, the return value is INVALID_HANDLE_VALUE. To get extended error information, call GetLastError.
	/// </returns>
	/// <remarks>
	/// CreateFile was originally developed specifically for file interaction but has since been expanded to include most other types of I/O devices and mechanisms available to Windows developers.
	/// When an application is finished using the object handle returned by CreateFile, use the CloseHandle function to close the handle.
	/// </remarks>
	// File I/O implementations
	[DllModuleExport(2)]
	private uint CreateFileA(uint lpFileName, uint dwDesiredAccess, uint dwShareMode, uint lpSecAttr,
		uint dwCreationDisposition, uint dwFlagsAndAttributes, uint hTemplateFile)
	{
		try
		{
			var path = _env.ReadAnsiString(lpFileName);

			// Handle invalid paths (empty, null, or invalid characters)
			if (string.IsNullOrEmpty(path))
			{
				_logger.LogInformation("[Kernel32] CreateFileA failed: Invalid path (empty or null)");
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return NativeTypes.Win32Handle.INVALID_HANDLE_VALUE;
			}

			// Resolve relative paths relative to the current directory.
			// This ensures paths like "data\IGN1.TEX" are resolved relative to the executable's directory.
			// CurrentDirectory is always set, so we always resolve relative paths.
			var resolvedPath = path;
			if (!Path.IsPathRooted(path))
			{
				// Path is relative, resolve it relative to current directory
				resolvedPath = Path.Combine(_env.CurrentDirectory, path);
				_logger.LogDebug("[Kernel32] CreateFileA: Resolved relative path '{Path}' to '{ResolvedPath}' (CurrentDirectory: '{CurrentDirectory}')", 
					path, resolvedPath, _env.CurrentDirectory);
			}

			// If VFS is available, use it for file operations
			if (_env.VirtualFileSystem != null)
			{
				var mode = dwCreationDisposition switch
				{
					1 => VfsFileMode.CreateNew,
					2 => VfsFileMode.Create,
					3 => VfsFileMode.Open,
					4 => VfsFileMode.OpenOrCreate,
					5 => VfsFileMode.Truncate,
					_ => VfsFileMode.OpenOrCreate
				};

				var access = VfsFileAccess.ReadWrite;
				if ((dwDesiredAccess & 0x80000000) != 0 && (dwDesiredAccess & 0x40000000) == 0)
				{
					access = VfsFileAccess.Read; // GENERIC_READ
				}
				else if ((dwDesiredAccess & 0x40000000) != 0 && (dwDesiredAccess & 0x80000000) == 0)
				{
					access = VfsFileAccess.Write; // GENERIC_WRITE
				}

				var handle = _env.VirtualFileSystem.OpenFile(resolvedPath, mode, access);
				if (handle != null)
				{
					return _env.RegisterHandle(handle);
				}

				_logger.LogInformation("[Kernel32] CreateFileA (VFS) failed: {Path}", resolvedPath);
				_lastError = NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
				return NativeTypes.Win32Handle.INVALID_HANDLE_VALUE;
			}

			// Fallback to direct filesystem access if VFS not available
			// Use the original path (not resolved) since we're working with the real filesystem
			var fileMode = dwCreationDisposition switch
			{
				1 => FileMode.CreateNew,
				2 => FileMode.Create,
				3 => FileMode.Open,
				4 => FileMode.OpenOrCreate,
				5 => FileMode.Truncate,
				_ => FileMode.OpenOrCreate
			};

			var fileAccess = FileAccess.ReadWrite;
			if ((dwDesiredAccess & 0x80000000) != 0 && (dwDesiredAccess & 0x40000000) == 0)
			{
				fileAccess = FileAccess.Read; // GENERIC_READ
			}

			if ((dwDesiredAccess & 0x40000000) != 0 && (dwDesiredAccess & 0x80000000) == 0)
			{
				fileAccess = FileAccess.Write; // GENERIC_WRITE
			}

			var fs = new FileStream(path, fileMode, fileAccess, FileShare.ReadWrite);
			return _env.RegisterHandle(fs);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] CreateFileA failed: {ExMessage}", ex.Message);
			_lastError = NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return NativeTypes.Win32Handle.INVALID_HANDLE_VALUE;
		}
	}

	[DllModuleExport(36)]
	private unsafe uint ReadFile(void* hFile, uint lpBuffer, uint nNumberOfBytesToRead, uint lpNumberOfBytesRead,
		uint lpOverlapped)
	{
		var handle = (uint)hFile;

		// Try VFS handle first
		if (_env.TryGetHandle<IVirtualFileHandle>(handle, out var vfsHandle) && vfsHandle is not null)
		{
			try
			{
				var buf = new byte[nNumberOfBytesToRead];
				var read = vfsHandle.Read(buf, 0, buf.Length);
				if (lpBuffer != 0 && read > 0)
				{
					_env.MemWriteBytes(lpBuffer, buf.AsSpan(0, read));
				}

				if (lpNumberOfBytesRead != 0)
				{
					_env.MemWrite32(lpNumberOfBytesRead, (uint)read);
				}

				return 1;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Kernel32] ReadFile (VFS) failed: {ExMessage}", ex.Message);
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
				return NativeTypes.Win32Bool.FALSE;
			}
		}

		// Fallback to FileStream for backwards compatibility
		if (_env.TryGetHandle<FileStream>(handle, out var fs) && fs is not null)
		{
			try
			{
				var buf = new byte[nNumberOfBytesToRead];
				var read = fs.Read(buf, 0, buf.Length);
				if (lpBuffer != 0 && read > 0)
				{
					_env.MemWriteBytes(lpBuffer, buf.AsSpan(0, read));
				}

				if (lpNumberOfBytesRead != 0)
				{
					_env.MemWrite32(lpNumberOfBytesRead, (uint)read);
				}

				return 1;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Kernel32] ReadFile failed: {ExMessage}", ex.Message);
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
				return NativeTypes.Win32Bool.FALSE;
			}
		}

		_lastError = NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
		return NativeTypes.Win32Bool.FALSE;
	}

	/// <summary>
	/// Writes data to the specified file or input/output (I/O) device.
	/// This function is designed for both synchronous and asynchronous operation. For a similar function designed solely for asynchronous operation, see WriteFileEx.
	/// </summary>
	/// <param name="handle">
	/// A handle to the file or I/O device (for example, a file, file stream, physical disk, volume, console buffer, tape drive, socket, communications resource, mailslot, or pipe).
	/// The hFile parameter must have been created with the write access. For more information, see Generic Access Rights and File Security and Access Rights.
	/// For asynchronous write operations, hFile can be any handle opened with the CreateFile function using the FILE_FLAG_OVERLAPPED flag or a socket handle returned by the socket or accept function.
	/// </param>
	/// <param name="lpBuffer">
	/// A pointer to the buffer containing the data to be written to the file or device.
	/// This buffer must remain valid for the duration of the write operation. The caller must not use this buffer until the write operation is completed.
	/// </param>
	/// <param name="nNumberOfBytesToWrite">
	/// The number of bytes to be written to the file or device.
	/// A value of zero specifies a null write operation. The behavior of a null write operation depends on the underlying file system or communications technology.
	/// Windows Server 2003 and Windows XP: Pipe write operations across a network are limited in size per write. The amount varies per platform. For x86 platforms it's 63.97 MB. For x64 platforms it's 31.97 MB. For Itanium it's 63.95 MB. For more information regarding pipes, see the Remarks section.
	/// </param>
	/// <param name="lpNumberOfBytesWritten">
	/// A pointer to the variable that receives the number of bytes written when using a synchronous hFile parameter. WriteFile sets this value to zero before doing any work or error checking. Use NULL for this parameter if this is an asynchronous operation to avoid potentially erroneous results.
	/// This parameter can be NULL only when the lpOverlapped parameter is not NULL.
	/// Windows 7: This parameter can not be NULL.
	/// For more information, see the Remarks section.
	/// </param>
	/// <param name="lpOverlapped">
	/// A pointer to an OVERLAPPED structure is required if the hFile parameter was opened with FILE_FLAG_OVERLAPPED, otherwise this parameter can be NULL.
	/// For an hFile that supports byte offsets, if you use this parameter you must specify a byte offset at which to start writing to the file or device. This offset is specified by setting the Offset and OffsetHigh members of the OVERLAPPED structure. For an hFile that does not support byte offsets, Offset and OffsetHigh are ignored.
	/// To write to the end of file, specify both the Offset and OffsetHigh members of the OVERLAPPED structure as 0xFFFFFFFF. This is functionally equivalent to previously calling the CreateFile function to open hFile using FILE_APPEND_DATA access.
	/// For more information about different combinations of lpOverlapped and FILE_FLAG_OVERLAPPED, see the Remarks section and the Synchronization and File Position section.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is nonzero (TRUE).
	/// If the function fails, or is completing asynchronously, the return value is zero (FALSE). To get extended error information, call the GetLastError function.
	/// </returns>
	/// <remarks>
	/// The WriteFile function returns when one of the following conditions occur:
	/// <ul>
	/// <li>The number of bytes requested is written.</li>
	/// <li>A read operation releases buffer space on the read end of the pipe (if the write was blocked). For more information, see the Pipes section.</li>
	/// <li>An asynchronous handle is being used and the write is occurring asynchronously.</li>
	/// <li>An error occurs.</li>
	/// </ul>
	/// The WriteFile function may fail with ERROR_INVALID_USER_BUFFER or ERROR_NOT_ENOUGH_MEMORY whenever there are too many outstanding asynchronous I/O requests.
	/// </remarks>
	[DllModuleExport(48)]
	private uint WriteFile(uint handle, uint lpBuffer, uint nNumberOfBytesToWrite, uint lpNumberOfBytesWritten,
		uint lpOverlapped)
	{
		_logger.LogInformation("[Kernel32] WriteFile(handle=0x{Handle:X8}, lpBuffer=0x{LpBuffer:X8}, nNumberOfBytesToWrite={NNumberOfBytesToWrite}, lpNumberOfBytesWritten=0x{LpNumberOfBytesWritten:X8}, lpOverlapped=0x{LpOverlapped:X8})", handle, lpBuffer, nNumberOfBytesToWrite, lpNumberOfBytesWritten, lpOverlapped);

		// NULL handle is invalid
		if (handle == 0)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
			return NativeTypes.Win32Bool.FALSE;
		}

		// Handle standard handles specially (only if they're not NULL)
		if (handle == _env.StdOutputHandle || handle == _env.StdErrorHandle || handle == _env.StdInputHandle)
		{
			try
			{
				var buf = _env.MemReadBytes(lpBuffer, (int)nNumberOfBytesToWrite);
				var text = Encoding.ASCII.GetString(buf);

				if (handle == _env.StdOutputHandle)
				{
					_env.WriteToStdOutput(text);
				}
				else if (handle == _env.StdErrorHandle)
				{
					_env.WriteToStdError(text);
				}
				// StdInputHandle is not writable, but we'll just succeed silently

				if (lpNumberOfBytesWritten != 0)
				{
					_env.MemWrite32(lpNumberOfBytesWritten, (uint)buf.Length);
				}

				return 1;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Kernel32] WriteFile to standard handle failed: {ExMessage}", ex.Message);
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
				return NativeTypes.Win32Bool.FALSE;
			}
		}
		else
		{
			_logger.LogWarning("[Kernel32] WriteFile not StdOutput, StdError or StdInput, called on non-standard handle 0x{Handle:X8}", handle);
		}

		// Handle regular file handles
		// Try VFS handle first
		if (_env.TryGetHandle<IVirtualFileHandle>(handle, out var vfsHandle) && vfsHandle is not null)
		{
			try
			{
				var buf = _env.MemReadBytes(lpBuffer, (int)nNumberOfBytesToWrite);
				vfsHandle.Write(buf, 0, buf.Length);
				if (lpNumberOfBytesWritten != 0)
				{
					_env.MemWrite32(lpNumberOfBytesWritten, (uint)buf.Length);
				}

				return 1;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Kernel32] WriteFile (VFS) failed: {ExMessage}", ex.Message);
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
				return NativeTypes.Win32Bool.FALSE;
			}
		}

		// Fallback to FileStream for backwards compatibility
		if (_env.TryGetHandle<FileStream>(handle, out var fs) && fs is not null)
		{
			try
			{
				var buf = _env.MemReadBytes(lpBuffer, (int)nNumberOfBytesToWrite);
				fs.Write(buf, 0, buf.Length);
				if (lpNumberOfBytesWritten != 0)
				{
					_env.MemWrite32(lpNumberOfBytesWritten, (uint)buf.Length);
				}

				return 1;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Kernel32] WriteFile failed: {ExMessage}", ex.Message);
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
				return NativeTypes.Win32Bool.FALSE;
			}
		}

		_lastError = NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
		return NativeTypes.Win32Bool.FALSE;
	}

	[DllModuleExport(1)]
	private unsafe uint CloseHandle(void* hObject)
	{
		var h = (uint)hObject;

		// Try VFS handle first
		if (_env.TryGetHandle<IVirtualFileHandle>(h, out var vfsHandle) && vfsHandle is not null)
		{
			vfsHandle.Dispose();
			_env.CloseHandle(h);
			return 1;
		}

		// Fallback to FileStream for backwards compatibility
		if (_env.TryGetHandle<FileStream>(h, out var fs) && fs is not null)
		{
			fs.Dispose();
			_env.CloseHandle(h);
			return 1;
		}

		return _env.CloseHandle(h) ? 1u : 0u;
	}

	[DllModuleExport(13)]
	private unsafe uint GetFileType(void* hFile)
	{
		var handle = (uint)hFile;

		// NULL handle returns FILE_TYPE_UNKNOWN
		if (handle == 0)
		{
			return 0x0000; // FILE_TYPE_UNKNOWN
		}

		// Standard handles are character devices (console)
		if (handle == _env.StdInputHandle || handle == _env.StdOutputHandle || handle == _env.StdErrorHandle)
		{
			return 0x0002; // FILE_TYPE_CHAR (character device like console)
		}

		// Check VFS handle
		if (_env.TryGetHandle<IVirtualFileHandle>(handle, out var vfsHandle) && vfsHandle is not null)
		{
			return 0x0001; // FILE_TYPE_DISK
		}

		// Check FileStream (backwards compatibility)
		if (_env.TryGetHandle<FileStream>(handle, out var fs) && fs is not null)
		{
			return 0x0001; // FILE_TYPE_DISK
		}

		return 0; // FILE_TYPE_UNKNOWN
	}

	[DllModuleExport(39)]
	private unsafe uint SetFilePointer(void* hFile, uint lDistanceToMove, uint lpDistanceToMoveHigh, uint dwMoveMethod)
	{
		var handle = (uint)hFile;

		// Try VFS handle first
		if (_env.TryGetHandle<IVirtualFileHandle>(handle, out var vfsHandle) && vfsHandle is not null)
		{
			var origin = dwMoveMethod switch
			{
				0 => SeekOrigin.Begin, 1 => SeekOrigin.Current, 2 => SeekOrigin.End, _ => SeekOrigin.Begin
			};
			long dist = (int)lDistanceToMove; // ignore high for now
			var pos = vfsHandle.Seek(dist, origin);
			return (uint)pos;
		}

		// Fallback to FileStream
		if (_env.TryGetHandle<FileStream>(handle, out var fs) && fs is not null)
		{
			var origin = dwMoveMethod switch
			{
				0 => SeekOrigin.Begin, 1 => SeekOrigin.Current, 2 => SeekOrigin.End, _ => SeekOrigin.Begin
			};
			long dist = (int)lDistanceToMove; // ignore high for now
			var pos = fs.Seek(dist, origin);
			return (uint)pos;
		}

		_lastError = NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
		return 0xFFFFFFFF;
	}

	[DllModuleExport(4)]
	private unsafe uint FlushFileBuffers(void* hFile)
	{
		var handle = (uint)hFile;

		// Standard output/error handles don't need flushing in our implementation
		// since WriteToStdOutput already calls the host callback immediately
		if (handle == _env.StdOutputHandle || handle == _env.StdErrorHandle)
		{
			return 1; // Success
		}

		// Try VFS handle first
		if (_env.TryGetHandle<IVirtualFileHandle>(handle, out var vfsHandle) && vfsHandle is not null)
		{
			vfsHandle.Flush();
			return 1;
		}

		// Fallback to FileStream
		if (_env.TryGetHandle<FileStream>(handle, out var fs) && fs is not null)
		{
			fs.Flush(true);
			return 1;
		}

		_lastError = NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
		return NativeTypes.Win32Bool.FALSE;
	}

	[DllModuleExport(38)]
	private unsafe uint SetEndOfFile(void* hFile)
	{
		var handle = (uint)hFile;

		// Try VFS handle first
		if (_env.TryGetHandle<IVirtualFileHandle>(handle, out var vfsHandle) && vfsHandle is not null)
		{
			vfsHandle.SetLength(vfsHandle.Position);
			return 1;
		}

		// Fallback to FileStream
		if (_env.TryGetHandle<FileStream>(handle, out var fs) && fs is not null)
		{
			fs.SetLength(fs.Position);
			return 1;
		}

		_lastError = NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
		return NativeTypes.Win32Bool.FALSE;
	}

	[DllModuleExport(1)]
	private uint DeleteFileA(uint lpFileName)
	{
		try
		{
			var path = _env.ReadAnsiString(lpFileName);
			if (string.IsNullOrEmpty(path))
			{
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return NativeTypes.Win32Bool.FALSE;
			}

			// If VFS is available, use it
			if (_env.VirtualFileSystem != null)
			{
				var success = _env.VirtualFileSystem.DeleteFile(path);
				if (success)
				{
					_logger.LogInformation("[Kernel32] DeleteFileA (VFS): Deleted '{Path}'", path);
					return NativeTypes.Win32Bool.TRUE;
				}

				_logger.LogInformation("[Kernel32] DeleteFileA (VFS) failed: '{Path}'", path);
				_lastError = NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
				return NativeTypes.Win32Bool.FALSE;
			}

			// Fallback to direct filesystem
			File.Delete(path);
			_logger.LogInformation("[Kernel32] DeleteFileA: Deleted '{Path}'", path);
			return NativeTypes.Win32Bool.TRUE;
		}
		catch (Exception ex)
		{
			_logger.LogInformation(ex, "[Kernel32] DeleteFileA failed: {ExMessage}", ex.Message);
			_lastError = NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return NativeTypes.Win32Bool.FALSE;
		}
	}

	[DllModuleExport(1)]
	private uint MoveFileA(uint lpExistingFileName, uint lpNewFileName)
	{
		try
		{
			var existingPath = _env.ReadAnsiString(lpExistingFileName);
			var newPath = _env.ReadAnsiString(lpNewFileName);

			if (string.IsNullOrEmpty(existingPath) || string.IsNullOrEmpty(newPath))
			{
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return NativeTypes.Win32Bool.FALSE;
			}

			// If VFS is available, use it
			if (_env.VirtualFileSystem != null)
			{
				var success = _env.VirtualFileSystem.MoveFile(existingPath, newPath);
				if (success)
				{
					_logger.LogInformation("[Kernel32] MoveFileA (VFS): Moved '{ExistingPath}' to '{NewPath}'",
						existingPath, newPath);
					return NativeTypes.Win32Bool.TRUE;
				}

				_logger.LogInformation("[Kernel32] MoveFileA (VFS) failed: '{ExistingPath}' to '{NewPath}'",
					existingPath, newPath);
				_lastError = NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
				return NativeTypes.Win32Bool.FALSE;
			}

			// Fallback to direct filesystem
			File.Move(existingPath, newPath);
			_logger.LogInformation("[Kernel32] MoveFileA: Moved '{ExistingPath}' to '{NewPath}'", existingPath, newPath);
			return NativeTypes.Win32Bool.TRUE;
		}
		catch (Exception ex)
		{
			_logger.LogInformation(ex, "[Kernel32] MoveFileA failed: {ExMessage}", ex.Message);
			_lastError = NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return NativeTypes.Win32Bool.FALSE;
		}
	}

	// Simple structure to hold find file data
	private class FindFileHandle
	{
		public string SearchPattern { get; set; } = "";
		public string[] Files { get; set; } = [];
		public int CurrentIndex { get; set; } = 0;
	}

	private readonly Dictionary<uint, FindFileHandle> _findFileHandles = new();
	private uint _nextFindFileHandle = 0x1000;

	// Helper method to write WIN32_FIND_DATAA structure
	private void WriteFindData(uint lpFindFileData, string fileName)
	{
		var fileNameBytes = Encoding.ASCII.GetBytes(fileName);

		// Clear the structure
		var zeroBuffer = new byte[320];
		_env.MemWriteBytes(lpFindFileData, zeroBuffer);

		// Write filename at offset 44 (cFileName field), ensure null-terminated and max 260 bytes
		var cFileNameBytes = new byte[260];
		var copyLen = Math.Min(fileNameBytes.Length, 259); // leave room for null terminator
		Array.Copy(fileNameBytes, 0, cFileNameBytes, 0, copyLen);
		cFileNameBytes[copyLen] = 0; // explicit null terminator
		_env.MemWriteBytes(lpFindFileData + 44, cFileNameBytes);
	}

	[DllModuleExport(1)]
	private uint FindFirstFileA(uint lpFileName, uint lpFindFileData)
	{
		try
		{
			var searchPattern = _env.ReadAnsiString(lpFileName);
			if (string.IsNullOrEmpty(searchPattern))
			{
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return NativeTypes.Win32Handle.INVALID_HANDLE_VALUE;
			}

			// Get directory and pattern
			var dir = Path.GetDirectoryName(searchPattern) ?? ".";
			var pattern = Path.GetFileName(searchPattern);

			if (string.IsNullOrEmpty(pattern))
			{
				pattern = "*";
			}

			string[] files;

			// If VFS is available, use it
			if (_env.VirtualFileSystem != null)
			{
				files = _env.VirtualFileSystem.GetFiles(dir, pattern);
			}
			else
			{
				// Fallback to direct filesystem
				files = Directory.GetFiles(dir, pattern);
			}

			if (files.Length == 0)
			{
				_lastError = NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
				return NativeTypes.Win32Handle.INVALID_HANDLE_VALUE;
			}

			// Create handle for this search
			var handle = _nextFindFileHandle++;
			_findFileHandles[handle] = new FindFileHandle
			{
				SearchPattern = searchPattern,
				Files = files,
				CurrentIndex = 0
			};

			// Write first file data (WIN32_FIND_DATAA structure - 320 bytes)
			// We'll write a simplified version with just the filename
			var fileName = Path.GetFileName(files[0]);
			WriteFindData(lpFindFileData, fileName);

			_logger.LogInformation("[Kernel32] FindFirstFileA: Found '{FileName}' for pattern '{SearchPattern}'", fileName, searchPattern);
			_findFileHandles[handle].CurrentIndex = 1;

			return handle;
		}
		catch (Exception ex)
		{
			_logger.LogInformation("[Kernel32] FindFirstFileA failed: {ExMessage}", ex.Message);
			_lastError = NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return NativeTypes.Win32Handle.INVALID_HANDLE_VALUE;
		}
	}

	[DllModuleExport(1)]
	private uint FindNextFileA(uint hFindFile, uint lpFindFileData)
	{
		try
		{
			if (!_findFileHandles.TryGetValue(hFindFile, out var handle))
			{
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
				return NativeTypes.Win32Bool.FALSE;
			}

			if (handle.CurrentIndex >= handle.Files.Length)
			{
				_lastError = NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
				return NativeTypes.Win32Bool.FALSE;
			}

			// Write next file data
			var fileName = Path.GetFileName(handle.Files[handle.CurrentIndex]);
			WriteFindData(lpFindFileData, fileName);

			_logger.LogInformation("[Kernel32] FindNextFileA: Found '{FileName}'", fileName);
			handle.CurrentIndex++;

			return NativeTypes.Win32Bool.TRUE;
		}
		catch (Exception ex)
		{
			_logger.LogInformation("[Kernel32] FindNextFileA failed: {ExMessage}", ex.Message);
			_lastError = NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return NativeTypes.Win32Bool.FALSE;
		}
	}

	[DllModuleExport(1)]
	private unsafe uint FindClose(void* hFindFile)
	{
		var handle = (uint)hFindFile;
		if (_findFileHandles.Remove(handle))
		{
			_logger.LogInformation("[Kernel32] FindClose: Closed handle 0x{Handle:X8}", handle);
			return NativeTypes.Win32Bool.TRUE;
		}

		_lastError = NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
		return NativeTypes.Win32Bool.FALSE;
	}

	[DllModuleExport(1)]
	private uint FileTimeToSystemTime(uint lpFileTime, uint lpSystemTime)
	{
		try
		{
			// FileTime is a 64-bit value representing the number of 100-nanosecond intervals since Jan 1, 1601
			// SystemTime is a SYSTEMTIME structure (16 bytes)

			// Read 64-bit file time as two 32-bit values
			var low = _env.MemRead32(lpFileTime);
			var high = _env.MemRead32(lpFileTime + 4);
			var fileTime = ((ulong)high << 32) | low;
			var dateTime = DateTime.FromFileTimeUtc((long)fileTime);

			// Write SYSTEMTIME structure
			_env.MemWrite16(lpSystemTime, (ushort)dateTime.Year);
			_env.MemWrite16(lpSystemTime + 2, (ushort)dateTime.Month);
			_env.MemWrite16(lpSystemTime + 4, (ushort)dateTime.DayOfWeek);
			_env.MemWrite16(lpSystemTime + 6, (ushort)dateTime.Day);
			_env.MemWrite16(lpSystemTime + 8, (ushort)dateTime.Hour);
			_env.MemWrite16(lpSystemTime + 10, (ushort)dateTime.Minute);
			_env.MemWrite16(lpSystemTime + 12, (ushort)dateTime.Second);
			_env.MemWrite16(lpSystemTime + 14, (ushort)dateTime.Millisecond);

			return NativeTypes.Win32Bool.TRUE;
		}
		catch (Exception ex)
		{
			_logger.LogInformation("[Kernel32] FileTimeToSystemTime failed: {ExMessage}", ex.Message);
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}
	}

	[DllModuleExport(1)]
	private uint FileTimeToLocalFileTime(uint lpFileTime, uint lpLocalFileTime)
	{
		try
		{
			// Convert UTC file time to local file time
			var low = _env.MemRead32(lpFileTime);
			var high = _env.MemRead32(lpFileTime + 4);
			var fileTime = ((ulong)high << 32) | low;
			var dateTime = DateTime.FromFileTimeUtc((long)fileTime);
			var localTime = dateTime.ToLocalTime();
			// Use ToFileTime() (not ToFileTimeUtc()) to get the local file time
			var localFileTime = (ulong)localTime.ToFileTime();

			_env.MemWrite32(lpLocalFileTime, (uint)(localFileTime & 0xFFFFFFFF));
			_env.MemWrite32(lpLocalFileTime + 4, (uint)(localFileTime >> 32));

			return NativeTypes.Win32Bool.TRUE;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] FileTimeToLocalFileTime failed: {ExMessage}", ex.Message);
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}
	}

	[DllModuleExport(1)]
	private uint GetTimeZoneInformation(uint lpTimeZoneInformation)
	{
		try
		{
			// TIME_ZONE_INFORMATION structure is 172 bytes
			// For simplicity, we'll just fill in the bias
			var bias = -(int)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalMinutes;

			_env.MemWrite32(lpTimeZoneInformation, (uint)bias);

			// Fill rest with zeros
			for (uint i = 4; i < 172; i++)
			{
				_env.MemWriteBytes(lpTimeZoneInformation + i, new byte[] { 0 });
			}

			_logger.LogInformation("[Kernel32] GetTimeZoneInformation: Bias={Bias} minutes", bias);

			// Return TIME_ZONE_ID_UNKNOWN (0)
			return 0;
		}
		catch (Exception ex)
		{
			_logger.LogInformation("[Kernel32] GetTimeZoneInformation failed: {ExMessage}", ex.Message);
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0xFFFFFFFF; // TIME_ZONE_ID_INVALID
		}
	}

	[DllModuleExport(1)]
	private uint SetEnvironmentVariableA(uint lpName, uint lpValue)
	{
		try
		{
			var name = _env.ReadAnsiString(lpName);

			if (string.IsNullOrEmpty(name))
			{
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return NativeTypes.Win32Bool.FALSE;
			}

			// If lpValue is NULL, delete the variable
			if (lpValue == 0)
			{
				Environment.SetEnvironmentVariable(name, null);
				_logger.LogInformation("[Kernel32] SetEnvironmentVariableA: Deleted '{Name}'", name);
			}
			else
			{
				var value = _env.ReadAnsiString(lpValue);
				Environment.SetEnvironmentVariable(name, value);
				_logger.LogInformation("[Kernel32] SetEnvironmentVariableA: Set '{Name}'='{Value}'", name, value);
			}

			return NativeTypes.Win32Bool.TRUE;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] SetEnvironmentVariableA failed: {ExMessage}", ex.Message);
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}
	}

	[DllModuleExport(40)]
	public uint SetHandleCount(uint uNumber)
	{
		// SetHandleCount is a legacy function from 16-bit Windows
		// In Win32, it's essentially a no-op that returns the requested count
		// Modern systems ignore this and have much higher handle limits
		return uNumber; // Return the requested number as if it was successfully set
	}

	[DllModuleExport(44)]
	private uint UnhandledExceptionFilter(uint exceptionInfo)
	{
		// UnhandledExceptionFilter processes unhandled exceptions
		// exceptionInfo is a pointer to an EXCEPTION_POINTERS structure
		_logger.LogInformation("[Kernel32] UnhandledExceptionFilter called with exceptionInfo=0x{ExceptionInfo:X8}", exceptionInfo);

		if (exceptionInfo != 0)
		{
			try
			{
				// EXCEPTION_POINTERS structure:
				// typedef struct _EXCEPTION_POINTERS {
				//   PEXCEPTION_RECORD ExceptionRecord;    // offset 0, 4 bytes
				//   PCONTEXT          ContextRecord;       // offset 4, 4 bytes  
				// } EXCEPTION_POINTERS;

				var exceptionRecordPtr = _env.MemRead32(exceptionInfo);
				var contextRecordPtr = _env.MemRead32(exceptionInfo + 4);

				_logger.LogInformation("[Kernel32]   ExceptionRecord: 0x{ExceptionRecordPtr:X8}", exceptionRecordPtr);
				_logger.LogInformation("[Kernel32]   ContextRecord: 0x{ContextRecordPtr:X8}", contextRecordPtr);

				// If we have a valid exception record, read some basic info
				if (exceptionRecordPtr != 0)
				{
					// EXCEPTION_RECORD structure (first few fields):
					//   DWORD ExceptionCode;        // offset 0
					//   DWORD ExceptionFlags;       // offset 4
					//   PEXCEPTION_RECORD ExceptionRecord; // offset 8
					//   PVOID ExceptionAddress;     // offset 12
					var exceptionCode = _env.MemRead32(exceptionRecordPtr);
					var exceptionFlags = _env.MemRead32(exceptionRecordPtr + 4);
					var exceptionAddress = _env.MemRead32(exceptionRecordPtr + 12);

					_logger.LogInformation("[Kernel32]     ExceptionCode: 0x{ExceptionCode:X8}", exceptionCode);
					_logger.LogInformation("[Kernel32]     ExceptionFlags: 0x{ExceptionFlags:X8}", exceptionFlags);
					_logger.LogInformation("[Kernel32]     ExceptionAddress: 0x{ExceptionAddress:X8}", exceptionAddress);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Kernel32] Error reading exception info: {ExMessage}", ex.Message);
			}
		}

		// For the emulator, we'll return EXCEPTION_EXECUTE_HANDLER to terminate the process
		// This is the safest default behavior for unhandled exceptions in an emulated environment
		return NativeTypes.ExceptionHandling.EXCEPTION_EXECUTE_HANDLER;
	}

	[DllModuleExport(47)]
	private uint WideCharToMultiByte(
		CodePage codePage,
		uint dwFlags,
		uint lpWideCharStr,
		uint cchWideChar,
		uint lpMultiByteStr,
		uint cbMultiByte,
		uint lpDefaultChar,
		uint lpUsedDefaultChar)
	{
		try
		{
			// Log the call parameters for debugging
			_logger.LogInformation("[Kernel32] WideCharToMultiByte: CP={CodePage} cchWide={CchWide} lpWide=0x{LpWide:X8} cbMulti={CbMulti} lpMulti=0x{LpMulti:X8}",
				codePage, cchWideChar, lpWideCharStr, cbMultiByte, lpMultiByteStr);

			// Handle null input string
			if (lpWideCharStr == 0)
			{
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return 0;
			}

			// Handle special code page values
			var actualCodePage = codePage switch
			{
				CodePage.Acp => GetAcp(), // CP_ACP - system default Windows ANSI code page
				CodePage.OemCp => GetOemCp(), // CP_OEMCP - system default OEM code page
				_ => codePage
			};

			// Read the wide character string from memory
			string wideString;
			if (cchWideChar == 0xFFFFFFFF) // -1 indicates null-terminated string
			{
				// Read null-terminated wide string
				var wideChars = new List<char>();
				var addr = lpWideCharStr;
				while (true)
				{
					var wideChar = _env.MemRead16(addr);
					if (wideChar == 0)
					{
						break;
					}

					wideChars.Add((char)wideChar);
					addr += 2;
				}

				wideString = new string(wideChars.ToArray());
				_logger.LogInformation("[Kernel32] WideCharToMultiByte: Read {Count} chars (null-terminated)", wideChars.Count);
			}
			else
			{
				// Read specified number of wide characters
				var wideChars = new char[cchWideChar];
				for (uint i = 0; i < cchWideChar; i++)
				{
					wideChars[i] = (char)_env.MemRead16(lpWideCharStr + i * 2);
				}

				wideString = new string(wideChars);
				_logger.LogInformation("[Kernel32] WideCharToMultiByte: Read {Count} chars (specified count)", cchWideChar);
			}

			// Convert to multi-byte string based on code page
			byte[] multiByteBytes;
			_logger.LogDebug("[Kernel32] WideCharToMultiByte: Converting with code page {ActualCodePage}", actualCodePage);
			switch (actualCodePage)
			{
				case CodePage.WestEurope: // Windows-1252 (Western European)
				case CodePage.Iso88591LatinI: // ISO 8859-1 (Latin-1)
					// Both Windows-1252 and ISO 8859-1 are single-byte encodings
					// For compatibility with InvariantGlobalization, use Latin1 fallback
					multiByteBytes = Encoding.Latin1.GetBytes(wideString);
					break;
				case CodePage.Oem437: // OEM US
				case CodePage.OemMultilingualLatinI: // OEM Latin-1  
				case CodePage.EastEurope: // Windows Central Europe
				case CodePage.Russian: // Windows Cyrillic
					// For other single-byte code pages, fallback to UTF-8 since Latin1 may not cover all characters
					// This provides better Unicode support even if not 100% code page accurate
					multiByteBytes = Encoding.UTF8.GetBytes(wideString);
					break;
				case CodePage.Utf8: // UTF-8
					multiByteBytes = Encoding.UTF8.GetBytes(wideString);
					break;
				default:
				{
					try
					{
						var encoding = Encoding.GetEncoding((int)actualCodePage);
						multiByteBytes = encoding.GetBytes(wideString);
						_logger.LogDebug("[Kernel32] Found bonus codepage {EncodingName} {WebName} ({CodePageInt})", encoding.EncodingName, encoding.WebName, (int)actualCodePage);
						break;
					}
					catch (Exception ex)
					{
						// Unsupported code page
						_logger.LogError(ex, "[Kernel32] Unsupported code page {CodePage} ({CodePageInt})", actualCodePage, (int)actualCodePage);
						_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
						return 0;
					}
				}
			}

			_logger.LogDebug("[Kernel32] WideCharToMultiByte: Conversion complete, got {BytesLength} bytes", multiByteBytes.Length);

			// If cbMultiByte is 0, return required buffer size
			if (cbMultiByte == 0)
			{
				// If input is null-terminated, include space for null terminator in required size
				if (cchWideChar == unchecked((uint)-1))
				{
					var result = (uint)(multiByteBytes.Length + 1);
					_logger.LogInformation("[Kernel32] WideCharToMultiByte: Returning size {Size} (including null terminator)", result);
					return result;
				}

				_logger.LogInformation("[Kernel32] WideCharToMultiByte: Returning size {Size}", (uint)multiByteBytes.Length);
				return (uint)multiByteBytes.Length;
			}

			// Check if output buffer is large enough
			if (multiByteBytes.Length > cbMultiByte)
			{
				_logger.LogInformation("[Kernel32] WideCharToMultiByte: Buffer too small - need {NeedSize} bytes but only have {CbMultiByte}", multiByteBytes.Length, cbMultiByte);
				_lastError = NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
				return 0;
			}

			// Copy converted bytes to output buffer
			if (lpMultiByteStr != 0)
			{
				_logger.LogDebug("[Kernel32] WideCharToMultiByte: Writing {BytesLength} bytes to 0x{LpMultiByteStr:X8}", multiByteBytes.Length, lpMultiByteStr);
				_env.MemWriteBytes(lpMultiByteStr, multiByteBytes);
			}

			// Clear the "used default char" flag if provided
			if (lpUsedDefaultChar != 0)
			{
				_env.MemWrite32(lpUsedDefaultChar, 0); // FALSE - no default char used (simplified)
			}

			_logger.LogDebug("[Kernel32] WideCharToMultiByte: Success, returning {BytesLength} bytes", (uint)multiByteBytes.Length);
			return (uint)multiByteBytes.Length;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] WideCharToMultiByte failed: {ExMessage}", ex.Message);
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}
	}

	[DllModuleExport(33)]
	private uint MultiByteToWideChar(CodePage codePage, uint dwFlags, uint lpMultiByteStr, int cbMultiByte, uint lpWideCharStr, uint cchWideChar)
	{
		// MultiByteToWideChar converts a multibyte (ANSI) string to Unicode (wide char) string
		// This is the inverse of WideCharToMultiByte

		try
		{
			if (lpMultiByteStr == 0)
			{
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return 0;
			}

			// Validate code page
			if (codePage != 0 && codePage != (CodePage)1 && codePage != (CodePage)1252 && codePage != (CodePage)437 && codePage != (CodePage)65001)
			{
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return 0;
			}

			// Use CP_ACP (1252) as default
			if (codePage is 0 or (CodePage)1)
			{
				codePage = (CodePage)1252;
			}

			// Determine string length if cbMultiByte is -1
			byte[] multiByteBytes;
			if (cbMultiByte == -1)
			{
				// Null-terminated string - read until null
				var byteList = new List<byte>();
				var currentAddr = lpMultiByteStr;
				while (true)
				{
					var b = _env.MemRead8(currentAddr);
					if (b == 0)
					{
						break;
					}

					byteList.Add(b);
					currentAddr++;
					if (byteList.Count > 10000) // Safety limit
					{
						_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
						return 0;
					}
				}

				multiByteBytes = byteList.ToArray();
			}
			else
			{
				// Read specified number of bytes
				multiByteBytes = new byte[cbMultiByte];
				for (var i = 0; i < cbMultiByte; i++)
				{
					multiByteBytes[i] = _env.MemRead8(lpMultiByteStr + (uint)i);
				}
			}

			// Convert to string using appropriate encoding
			// For simplicity, use ASCII for code pages 1252/437, UTF-8 for 65001
			var encoding = codePage switch
			{
				(CodePage)65001 => Encoding.UTF8, // UTF-8
				_ => Encoding.ASCII // ASCII for Western code pages
			};

			var str = encoding.GetString(multiByteBytes);

			// If lpWideCharStr is 0, just return required buffer size
			if (lpWideCharStr == 0 || cchWideChar == 0)
			{
				return (uint)str.Length; // Not including null terminator
			}

			// Check if output buffer is large enough
			if (str.Length > cchWideChar)
			{
				_lastError = NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
				return 0;
			}

			// Write wide characters to output buffer
			for (var i = 0; i < str.Length; i++)
			{
				_env.MemWrite16(lpWideCharStr + (uint)(i * 2), str[i]);
			}

			// Add null terminator if there's room and input was null-terminated
			if (cbMultiByte == -1 && str.Length < cchWideChar)
			{
				_env.MemWrite16(lpWideCharStr + (uint)(str.Length * 2), 0);
			}

			return (uint)str.Length;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] MultiByteToWideChar failed: {ExMessage}", ex.Message);
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}
	}

	[DllModuleExport(30)]
	private uint LcMapStringA(uint locale, uint dwMapFlags, uint lpSrcStr, int cchSrc, uint lpDestStr, int cchDest)
	{
		// LCMapStringA performs locale-dependent string mapping (e.g., uppercase, lowercase)
		// For simplicity, we'll support only basic case conversion

		try
		{
			if (lpSrcStr == 0)
			{
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return 0;
			}

			const uint lcmapLowercase = 0x00000100;
			const uint lcmapUppercase = 0x00000200;

			// Read source string
			string srcStr;
			if (cchSrc == -1)
			{
				srcStr = _env.ReadAnsiString(lpSrcStr);
			}
			else
			{
				var bytes = new byte[cchSrc];
				for (var i = 0; i < cchSrc; i++)
				{
					bytes[i] = _env.MemRead8(lpSrcStr + (uint)i);
				}

				srcStr = Encoding.ASCII.GetString(bytes);
			}

			// Apply mapping
			var destStr = srcStr;
			if ((dwMapFlags & lcmapLowercase) != 0)
			{
				destStr = srcStr.ToLowerInvariant();
			}
			else if ((dwMapFlags & lcmapUppercase) != 0)
			{
				destStr = srcStr.ToUpperInvariant();
			}

			// If lpDestStr is 0, return required buffer size
			if (lpDestStr == 0 || cchDest == 0)
			{
				return (uint)destStr.Length + 1; // Including null terminator
			}

			// Check buffer size
			if (destStr.Length + 1 > cchDest)
			{
				_lastError = NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
				return 0;
			}

			// Write result
			var destBytes = Encoding.ASCII.GetBytes(destStr);
			_env.MemWriteBytes(lpDestStr, destBytes);
			_env.MemWriteBytes(lpDestStr + (uint)destBytes.Length, new byte[] { 0 }); // Null terminator

			return (uint)destStr.Length + 1;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] LCMapStringA failed: {ExMessage}", ex.Message);
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}
	}

	[DllModuleExport(31)]
	private uint LcMapStringW(uint locale, uint dwMapFlags, uint lpSrcStr, int cchSrc, uint lpDestStr, int cchDest)
	{
		// LCMapStringW performs locale-dependent string mapping for Unicode strings

		try
		{
			if (lpSrcStr == 0)
			{
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return 0;
			}

			const uint lcmapLowercase = 0x00000100;
			const uint lcmapUppercase = 0x00000200;

			// Read source string (wide chars)
			string srcStr;
			if (cchSrc == -1)
			{
				// Null-terminated
				var chars = new List<char>();
				var currentAddr = lpSrcStr;
				while (true)
				{
					var wchar = _env.MemRead16(currentAddr);
					if (wchar == 0)
					{
						break;
					}

					chars.Add((char)wchar);
					currentAddr += 2;
					if (chars.Count > 10000) // Safety limit
					{
						_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
						return 0;
					}
				}

				srcStr = new string(chars.ToArray());
			}
			else
			{
				var chars = new char[cchSrc];
				for (var i = 0; i < cchSrc; i++)
				{
					chars[i] = (char)_env.MemRead16(lpSrcStr + (uint)(i * 2));
				}

				srcStr = new string(chars);
			}

			// Apply mapping
			var destStr = srcStr;
			if ((dwMapFlags & lcmapLowercase) != 0)
			{
				destStr = srcStr.ToLowerInvariant();
			}
			else if ((dwMapFlags & lcmapUppercase) != 0)
			{
				destStr = srcStr.ToUpperInvariant();
			}

			// If lpDestStr is 0, return required buffer size
			if (lpDestStr == 0 || cchDest == 0)
			{
				return (uint)destStr.Length + 1; // Including null terminator
			}

			// Check buffer size
			if (destStr.Length + 1 > cchDest)
			{
				_lastError = NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
				return 0;
			}

			// Write result (wide chars)
			for (var i = 0; i < destStr.Length; i++)
			{
				_env.MemWrite16(lpDestStr + (uint)(i * 2), destStr[i]);
			}

			_env.MemWrite16(lpDestStr + (uint)(destStr.Length * 2), 0); // Null terminator

			return (uint)destStr.Length + 1;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] LCMapStringW failed: {ExMessage}", ex.Message);
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}
	}

	[DllModuleExport(1)]
	private uint CompareStringA(uint locale, uint dwCmpFlags, uint lpString1, int cchCount1, uint lpString2, int cchCount2)
	{
		// CompareStringA compares two ANSI strings
		// Returns: CSTR_LESS_THAN (1), CSTR_EQUAL (2), or CSTR_GREATER_THAN (3)
		const uint cstrLessThan = 1;
		const uint cstrEqual = 2;
		const uint cstrGreaterThan = 3;

		try
		{
			if (lpString1 == 0 || lpString2 == 0)
			{
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return 0;
			}

			// Read strings
			string str1;
			if (cchCount1 == -1)
			{
				str1 = _env.ReadAnsiString(lpString1);
			}
			else
			{
				var bytes = new byte[cchCount1];
				for (var i = 0; i < cchCount1; i++)
				{
					bytes[i] = _env.MemRead8(lpString1 + (uint)i);
				}

				str1 = Encoding.ASCII.GetString(bytes);
			}

			string str2;
			if (cchCount2 == -1)
			{
				str2 = _env.ReadAnsiString(lpString2);
			}
			else
			{
				var bytes = new byte[cchCount2];
				for (var i = 0; i < cchCount2; i++)
				{
					bytes[i] = _env.MemRead8(lpString2 + (uint)i);
				}

				str2 = Encoding.ASCII.GetString(bytes);
			}

			// Perform comparison (ignoring locale and flags for simplicity)
			var result = string.Compare(str1, str2, StringComparison.Ordinal);

			_logger.LogInformation("[Kernel32] CompareStringA: '{Str1}' vs '{Str2}' = {Result}", str1, str2, result);

			if (result < 0) return cstrLessThan;
			if (result > 0) return cstrGreaterThan;
			return cstrEqual;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] CompareStringA failed: {ExMessage}", ex.Message);
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}
	}

	[DllModuleExport(1)]
	private uint CompareStringW(uint locale, uint dwCmpFlags, uint lpString1, int cchCount1, uint lpString2, int cchCount2)
	{
		// CompareStringW compares two Unicode strings
		// Returns: CSTR_LESS_THAN (1), CSTR_EQUAL (2), or CSTR_GREATER_THAN (3)
		const uint cstrLessThan = 1;
		const uint cstrEqual = 2;
		const uint cstrGreaterThan = 3;

		try
		{
			if (lpString1 == 0 || lpString2 == 0)
			{
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return 0;
			}

			// Read wide strings
			string str1;
			if (cchCount1 == -1)
			{
				// Read null-terminated Unicode string
				var sb = new StringBuilder();
				uint offset = 0;
				while (true)
				{
					var ch = (char)_env.MemRead16(lpString1 + offset);
					if (ch == 0) break;
					sb.Append(ch);
					offset += 2;
				}

				str1 = sb.ToString();
			}
			else
			{
				var sb = new StringBuilder();
				for (var i = 0; i < cchCount1; i++)
				{
					var ch = (char)_env.MemRead16(lpString1 + (uint)(i * 2));
					sb.Append(ch);
				}

				str1 = sb.ToString();
			}

			string str2;
			if (cchCount2 == -1)
			{
				// Read null-terminated Unicode string
				var sb = new StringBuilder();
				uint offset = 0;
				while (true)
				{
					var ch = (char)_env.MemRead16(lpString2 + offset);
					if (ch == 0) break;
					sb.Append(ch);
					offset += 2;
				}

				str2 = sb.ToString();
			}
			else
			{
				var sb = new StringBuilder();
				for (var i = 0; i < cchCount2; i++)
				{
					var ch = (char)_env.MemRead16(lpString2 + (uint)(i * 2));
					sb.Append(ch);
				}

				str2 = sb.ToString();
			}

			// Perform comparison (ignoring locale and flags for simplicity)
			var result = string.Compare(str1, str2, StringComparison.Ordinal);

			_logger.LogInformation("[Kernel32] CompareStringW: '{Str1}' vs '{Str2}' = {Result}", str1, str2, result);

			if (result < 0) return cstrLessThan;
			if (result > 0) return cstrGreaterThan;
			return cstrEqual;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] CompareStringW failed: {ExMessage}", ex.Message);
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}
	}

	[DllModuleExport(34)]
	private uint QueryPerformanceCounter(uint lpPerformanceCount)
	{
		// QueryPerformanceCounter retrieves the current value of the performance counter
		// lpPerformanceCount is a pointer to a LARGE_INTEGER (64-bit value)
		if (lpPerformanceCount == 0)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}

		try
		{
			// Use .NET's Stopwatch.GetTimestamp() which provides high-resolution timestamp
			var timestamp = Stopwatch.GetTimestamp();

			// Write the 64-bit timestamp to the provided memory location
			_env.MemWrite64(lpPerformanceCount, (ulong)timestamp);

			return NativeTypes.Win32Bool.TRUE;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] QueryPerformanceCounter failed");
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}
	}

	[DllModuleExport(1)]
	private uint QueryPerformanceFrequency(uint lpFrequency)
	{
		// QueryPerformanceFrequency retrieves the frequency of the performance counter
		// lpFrequency is a pointer to a LARGE_INTEGER (64-bit value)
		// The frequency is in counts per second
		if (lpFrequency == 0)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}

		try
		{
			// Stopwatch.Frequency provides the frequency of the high-resolution timer
			var frequency = Stopwatch.Frequency;

			// Write the 64-bit frequency to the provided memory location
			_env.MemWrite64(lpFrequency, (ulong)frequency);

			_logger.LogInformation("[Kernel32] QueryPerformanceFrequency: {Frequency} Hz", frequency);
			return NativeTypes.Win32Bool.TRUE;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] QueryPerformanceFrequency failed");
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}
	}

	[DllModuleExport(1)]
	private uint GetTickCount()
	{
		// GetTickCount returns the number of milliseconds since system start
		// In an emulator context, we use the time since the emulator started
		// Returns a 32-bit value that wraps around to zero after ~49.7 days

		// Use Environment.TickCount which is designed for this exact purpose
		var tickCount = (uint)Environment.TickCount;

		_logger.LogInformation("[Kernel32] GetTickCount: {TickCount} ms", tickCount);
		return tickCount;
	}

	[DllModuleExport(1)]
	private uint GetTickCount64(uint lpTickCount)
	{
		// GetTickCount64 returns a 64-bit tick count that won't wrap
		// lpTickCount is a pointer to a ULONGLONG (64-bit value)
		// Returns non-zero on success, zero on failure

		if (lpTickCount == 0)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}

		try
		{
			// Use Environment.TickCount64 which provides 64-bit tick count
			// This is available in .NET and won't wrap around
			var tickCount64 = (ulong)Environment.TickCount64;

			// Write the 64-bit tick count to the provided memory location
			_env.MemWrite64(lpTickCount, tickCount64);

			_logger.LogInformation("[Kernel32] GetTickCount64: {TickCount64} ms", tickCount64);
			return 1; // Success (non-zero return)
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] GetTickCount64 failed");
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}
	}

	[DllModuleExport(1)]
	private uint Sleep(uint dwMilliseconds)
	{
		// Sleep suspends execution for a specified interval
		// With cooperative threading support, we integrate with the ThreadScheduler
		// to properly yield control to other emulated threads.

		if (dwMilliseconds == 0)
		{
			// Sleep(0) means "yield to other threads"
			_logger.LogInformation("[Kernel32] Sleep(0): yielding to other threads");
			
			// Cooperate with the thread scheduler if available
			var scheduler = _env.ThreadScheduler;
			if (scheduler != null)
			{
				var currentThreadId = _env.GetCurrentThreadId();
				_logger.LogDebug("[Kernel32] Sleep(0): Current thread {ThreadId} yielding", currentThreadId);
				
				// Mark as yielding but don't actually suspend
				// The main execution loop will handle context switching
				Thread.Yield();
			}
			else
			{
				// No thread scheduler, just yield the native thread
				Thread.Yield();
			}
		}
		else if (dwMilliseconds == 0xFFFFFFFF) // INFINITE
		{
			_logger.LogWarning("[Kernel32] Sleep(INFINITE): suspending thread indefinitely");
			// Don't actually sleep forever - mark thread as waiting
			// The thread scheduler will handle this appropriately
			var scheduler = _env.ThreadScheduler;
			if (scheduler != null)
			{
				var currentThreadId = _env.GetCurrentThreadId();
				// Use a very long timeout to simulate INFINITE
				scheduler.SetThreadWaiting(currentThreadId, new object(), 0xFFFFFFFF);
			}
			else
			{
				// No thread scheduler - do a short sleep to avoid hanging
				Thread.Sleep(1);
			}
		}
		else
		{
			_logger.LogInformation("[Kernel32] Sleep: {DwMilliseconds} ms", dwMilliseconds);
			
			// For timed sleeps, cooperate with the threading system
			var scheduler = _env.ThreadScheduler;
			if (scheduler != null && dwMilliseconds > 10)
			{
				var currentThreadId = _env.GetCurrentThreadId();
				_logger.LogDebug("[Kernel32] Sleep: Thread {ThreadId} sleeping for {Ms}ms", currentThreadId, dwMilliseconds);
				
				// Mark thread as waiting with timeout
				// The scheduler will wake it after the timeout expires
				var sleepToken = new object(); // Unique token for this sleep
				scheduler.SetThreadWaiting(currentThreadId, sleepToken, dwMilliseconds);
				
				// Do a minimal actual sleep to prevent busy-waiting
				Thread.Sleep(1);
			}
			else
			{
				// Short sleeps or no scheduler - use minimal delay
				Thread.Sleep(dwMilliseconds > 0 ? 1 : 0);
			}
		}

		return 0; // Sleep doesn't return a value (void function)
	}

	[DllModuleExport(1)]
	private string ReadCurrentModulePath()
	{
		// Prefer the initialized executable path from the process environment
		if (!string.IsNullOrEmpty(_env.ExecutablePath))
		{
			// Convert to Windows-style path with backslashes
			return ConvertToWindowsPath(_env.ExecutablePath);
		}

		// Fall back to the module filename pointer if available
		try
		{
			if (_env.ModuleFileNamePtr != 0)
			{
				var s = _env.ReadAnsiString(_env.ModuleFileNamePtr);
				if (!string.IsNullOrEmpty(s))
				{
					// Convert to Windows-style path with backslashes
					return ConvertToWindowsPath(s);
				}
			}
		}
		catch
		{
			// ignore and fall through to default
		}

		// Final fallback for legacy behavior
		return "C:\\game.exe";
	}

	/// <summary>
	/// Converts a system path to a Windows-style VFS path with backslashes
	/// </summary>
	private string ConvertToWindowsPath(string path)
	{
		// Special handling for command line strings
		if (path.Contains(' '))
		{
			// This might be a command line with arguments
			// Fix the issue with backslashes before quotes that causes function 412440 to loop infinitely
			// Windows command line parser expects backslashes before quotes to be properly escaped
			var fixedPath = FixCommandLineEscaping(path);
			if (fixedPath != path)
			{
				_logger.LogDebug("[Kernel32] Fixed command line escaping: {OrigPath} -> {FixedPath}", path, fixedPath);
				path = fixedPath;
			}
		}

		// Replace forward slashes with backslashes
		var windowsPath = path.Replace('/', '\\');

		// If the path doesn't start with a drive letter, add C:\ prefix
		if (!windowsPath.Contains(":\\"))
		{
			if (windowsPath.StartsWith(@"""\"))
			{
				windowsPath = windowsPath.TrimStart('\"', '\\');
				windowsPath = $"\"C:\\{windowsPath}";
			}
			else
			{
				// Remove any leading backslashes
				windowsPath = windowsPath.TrimStart('\\');
				windowsPath = $"C:\\{windowsPath}";
			}
		}

		return windowsPath;
	}

	/// <summary>
	/// Fixes command line escaping issues that can cause infinite loops in command line parsing
	/// </summary>
	private string FixCommandLineEscaping(string cmdLine)
	{
		// The issue occurs when there are backslashes before quotes
		// Windows expects backslashes before quotes to be properly escaped

		var result = new StringBuilder(cmdLine.Length);
		var inQuote = false;

		for (var i = 0; i < cmdLine.Length; i++)
		{
			var c = cmdLine[i];

			// Handle backslash sequences
			if (c == '\\')
			{
				var backslashCount = 1;
				while (i + 1 < cmdLine.Length && cmdLine[i + 1] == '\\')
				{
					backslashCount++;
					i++;
				}

				// Check if next char is a quote
				if (i + 1 < cmdLine.Length && cmdLine[i + 1] == '"')
				{
					// For each backslash before a quote, we need two backslashes
					// This ensures the command line parser correctly handles the escaping
					for (var j = 0; j < backslashCount; j++)
					{
						result.Append(@"\\");
					}
				}
				else
				{
					// Regular backslashes not before quotes
					for (var j = 0; j < backslashCount; j++)
					{
						result.Append('\\');
					}
				}
			}
			else if (c == '"')
			{
				// Toggle quote state and add the quote
				inQuote = !inQuote;
				result.Append(c);
			}
			else
			{
				// Regular character
				result.Append(c);
			}
		}

		return result.ToString();
	}

	[DllModuleExport(37)]
	private uint RtlUnwind(uint targetFrame, uint targetIp, uint exceptionRecord, uint returnValue)
	{
		// RtlUnwind is used for structured exception handling to unwind the stack
		// In a real implementation, this would:
		// 1. Walk the stack from current frame to targetFrame
		// 2. Call exception handlers with EXCEPTION_UNWIND flag
		// 3. Restore processor state
		// 4. Jump to targetIp with returnValue in EAX

		// For the Win32Emu, we implement a minimal version that:
		// - Logs the unwind operation
		// - Sets the target IP if provided
		// - Sets EAX to returnValue
		// - Adjusts ESP to targetFrame if provided

		_logger.LogInformation("[Kernel32] RtlUnwind called: targetFrame=0x{TargetFrame:X8}, targetIp=0x{TargetIp:X8}, exceptionRecord=0x{ExceptionRecord:X8}, returnValue=0x{ReturnValue:X8}", targetFrame, targetIp, exceptionRecord, returnValue);

		// Modify CPU state as specified
		if (_cpu != null)
		{
			// Set the return value in EAX
			_cpu.SetRegister("EAX", returnValue);
			_logger.LogInformation("[Kernel32] RtlUnwind: Set EAX to 0x{ReturnValue:X8}", returnValue);

			// If a target frame is specified, set ESP to it
			if (targetFrame != 0)
			{
				_cpu.SetRegister("ESP", targetFrame);
				_logger.LogInformation("[Kernel32] RtlUnwind: Set ESP to target frame 0x{TargetFrame:X8}", targetFrame);
			}

			// If a target IP is specified, set EIP to it
			if (targetIp != 0)
			{
				_cpu.SetEip(targetIp);
				_logger.LogInformation("[Kernel32] RtlUnwind: Set EIP to 0x{TargetIp:X8}", targetIp);
			}
		}
		else
		{
			_logger.LogWarning("[Kernel32] RtlUnwind: CPU not available, cannot modify state");
		}

		// RtlUnwind doesn't return a value in the traditional sense - it either succeeds
		// or raises an exception. We'll return 0 to indicate success.
		return 0;
	}

	// Thread management and TLS functions
	[DllModuleExport(1)]
	private uint CreateThread(uint lpThreadAttributes, uint dwStackSize, uint lpStartAddress, uint lpParameter, uint dwCreationFlags, uint lpThreadId)
	{
		_logger.LogInformation(
			"[Kernel32] CreateThread(attr=0x{LpThreadAttributes:X8}, stack=0x{DwStackSize:X8}, start=0x{LpStartAddress:X8}, param=0x{LpParameter:X8}, flags=0x{DwCreationFlags:X8}, outId=0x{LpThreadId:X8})",
			lpThreadAttributes,
			dwStackSize,
			lpStartAddress,
			lpParameter,
			dwCreationFlags,
			lpThreadId);

		// Use default stack size if not specified
		if (dwStackSize == 0)
		{
			dwStackSize = 0x8000; // 32KB default
		}

		// CREATE_SUSPENDED flag (0x4)
		const uint CREATE_SUSPENDED = 0x4;
		var suspended = (dwCreationFlags & CREATE_SUSPENDED) != 0;

		// Create the thread using the new threading infrastructure
		var handle = _env.CreateThread(lpStartAddress, lpParameter, dwStackSize, suspended);

		// Get the thread ID from the handle (if ThreadScheduler is available)
		var threadId = handle;
		if (_env.ThreadScheduler != null)
		{
			var thread = _env.ThreadScheduler.GetThreadByHandle(handle);
			if (thread != null)
			{
				threadId = thread.ThreadId;
			}
		}

		// If lpThreadId is not null, write the thread ID to it
		if (lpThreadId != 0)
		{
			_env.MemWrite32(lpThreadId, threadId);
		}

		// Return the thread handle
		return handle;
	}

	[DllModuleExport(37)]
	private uint GetCurrentThreadId()
	{
		var threadId = _env.GetCurrentThreadId();
		_logger.LogInformation("[Kernel32] GetCurrentThreadId() = {ThreadId}", threadId);
		return threadId;
	}

	[DllModuleExport(37)]
	private uint ResumeThread(uint hThread)
	{
		_logger.LogInformation("[Kernel32] ResumeThread(handle=0x{Handle:X8})", hThread);

		if (_env.ThreadScheduler != null)
		{
			var thread = _env.ThreadScheduler.GetThreadByHandle(hThread);
			if (thread != null)
			{
				// Get previous suspend count (0 if running, >0 if suspended)
				var previousSuspendCount = thread.State == Threading.ThreadState.Suspended ? 1u : 0u;
				
				_env.ThreadScheduler.ResumeThread(thread.ThreadId);
				_logger.LogInformation("[Kernel32] ResumeThread: thread {ThreadId} resumed", thread.ThreadId);
				
				return previousSuspendCount;
			}
		}

		// Thread not found or scheduler not available
		_logger.LogWarning("[Kernel32] ResumeThread: invalid thread handle 0x{Handle:X8}", hThread);
		return 0xFFFFFFFF; // -1 = error
	}

	[DllModuleExport(37)]
	private uint SuspendThread(uint hThread)
	{
		_logger.LogInformation("[Kernel32] SuspendThread(handle=0x{Handle:X8})", hThread);

		var thread = _env.ThreadScheduler?.GetThreadByHandle(hThread);
		if (thread != null)
		{
			// Get previous suspend count (0 if running, >0 if suspended)
			var previousSuspendCount = thread.State == Threading.ThreadState.Suspended ? 1u : 0u;
				
			_env.ThreadScheduler.SuspendThread(thread.ThreadId);
			_logger.LogInformation("[Kernel32] SuspendThread: thread {ThreadId} suspended", thread.ThreadId);
				
			return previousSuspendCount;
		}

		// Thread not found or scheduler not available
		_logger.LogWarning("[Kernel32] SuspendThread: invalid thread handle 0x{Handle:X8}", hThread);
		return 0xFFFFFFFF; // -1 = error
	}

	[DllModuleExport(37)]
	private uint TlsAlloc()
	{
		var index = _env.TlsAlloc();
		_logger.LogInformation("[Kernel32] TlsAlloc() = {Index}", index);
		return index;
	}

	[DllModuleExport(37)]
	private uint TlsGetValue(uint dwTlsIndex)
	{
		var value = _env.TlsGetValue(dwTlsIndex);
		_logger.LogInformation("[Kernel32] TlsGetValue({DwTlsIndex}) = 0x{Value:X8}", dwTlsIndex, value);
		return value;
	}

	[DllModuleExport(37)]
	private uint TlsSetValue(uint dwTlsIndex, uint lpTlsValue)
	{
		var success = _env.TlsSetValue(dwTlsIndex, lpTlsValue);
		_logger.LogInformation("[Kernel32] TlsSetValue({DwTlsIndex}, 0x{LpTlsValue:X8}) = {Success}", dwTlsIndex, lpTlsValue, success);
		return success ? NativeTypes.Win32Bool.TRUE : NativeTypes.Win32Bool.FALSE;
	}

	[DllModuleExport(37)]
	private uint TlsFree(uint dwTlsIndex)
	{
		var success = _env.TlsFree(dwTlsIndex);
		_logger.LogInformation("[Kernel32] TlsFree({DwTlsIndex}) = {Success}", dwTlsIndex, success);
		return success ? NativeTypes.Win32Bool.TRUE : NativeTypes.Win32Bool.FALSE;
	}

	// Synchronization primitives
	[DllModuleExport(37)]
	private uint CreateMutex(uint lpMutexAttributes, uint bInitialOwner, in LpcStr lpName)
	{
		var name = lpName.ToString();
		var initialOwner = bInitialOwner != 0;
		var currentThreadId = _env.GetCurrentThreadId();

		_logger.LogInformation("[Kernel32] CreateMutex(attr=0x{Attr:X8}, initialOwner={InitialOwner}, name=\"{Name}\")",
			lpMutexAttributes, initialOwner, name ?? "<unnamed>");

		if (_env.SynchronizationManager == null)
		{
			_logger.LogWarning("[Kernel32] CreateMutex: SynchronizationManager not available");
			return 0; // NULL handle
		}

		var handle = _env.SynchronizationManager.CreateMutex(initialOwner, name, currentThreadId, out var alreadyExists);

		if (alreadyExists)
		{
			_lastError = NativeTypes.Win32Error.ERROR_ALREADY_EXISTS;
		}

		return handle;
	}

	[DllModuleExport(37)]
	private uint ReleaseMutex(uint hMutex)
	{
		var currentThreadId = _env.GetCurrentThreadId();
		_logger.LogInformation("[Kernel32] ReleaseMutex(handle=0x{Handle:X8})", hMutex);

		if (_env.SynchronizationManager == null)
		{
			_logger.LogWarning("[Kernel32] ReleaseMutex: SynchronizationManager not available");
			return NativeTypes.Win32Bool.FALSE;
		}

		var success = _env.SynchronizationManager.ReleaseMutex(hMutex, currentThreadId);
		
		if (!success)
		{
			_lastError = NativeTypes.Win32Error.ERROR_NOT_OWNER;
		}

		// Check if there are waiting threads
		var nextWaiter = _env.SynchronizationManager.GetNextMutexWaiter(hMutex);
		if (nextWaiter.HasValue && _env.ThreadScheduler != null)
		{
			_env.ThreadScheduler.WakeThread(nextWaiter.Value);
		}

		return success ? NativeTypes.Win32Bool.TRUE : NativeTypes.Win32Bool.FALSE;
	}

	[DllModuleExport(37)]
	private uint CreateEvent(uint lpEventAttributes, uint bManualReset, uint bInitialState, in LpcStr lpName)
	{
		var name = lpName.ToString();
		var manualReset = bManualReset != 0;
		var initialState = bInitialState != 0;

		_logger.LogInformation("[Kernel32] CreateEvent(attr=0x{Attr:X8}, manual={Manual}, initial={Initial}, name=\"{Name}\")",
			lpEventAttributes, manualReset, initialState, name ?? "<unnamed>");

		if (_env.SynchronizationManager == null)
		{
			_logger.LogWarning("[Kernel32] CreateEvent: SynchronizationManager not available");
			return 0; // NULL handle
		}

		var handle = _env.SynchronizationManager.CreateEvent(manualReset, initialState, name, out var alreadyExists);

		if (alreadyExists)
		{
			_lastError = NativeTypes.Win32Error.ERROR_ALREADY_EXISTS;
		}

		return handle;
	}

	[DllModuleExport(37)]
	private uint SetEvent(uint hEvent)
	{
		_logger.LogInformation("[Kernel32] SetEvent(handle=0x{Handle:X8})", hEvent);

		if (_env.SynchronizationManager == null)
		{
			_logger.LogWarning("[Kernel32] SetEvent: SynchronizationManager not available");
			return NativeTypes.Win32Bool.FALSE;
		}

		var success = _env.SynchronizationManager.SetEvent(hEvent);

		// Wake all threads waiting on this event
		if (!success || _env.ThreadScheduler == null)
		{
			return success ? NativeTypes.Win32Bool.TRUE : NativeTypes.Win32Bool.FALSE;
		}

		var waiters = _env.SynchronizationManager.GetEventWaiters(hEvent);
		foreach (var waiterId in waiters)
		{
			_env.ThreadScheduler.WakeThread(waiterId);
		}

		return success ? NativeTypes.Win32Bool.TRUE : NativeTypes.Win32Bool.FALSE;
	}

	[DllModuleExport(37)]
	private uint ResetEvent(uint hEvent)
	{
		_logger.LogInformation("[Kernel32] ResetEvent(handle=0x{Handle:X8})", hEvent);

		if (_env.SynchronizationManager == null)
		{
			_logger.LogWarning("[Kernel32] ResetEvent: SynchronizationManager not available");
			return NativeTypes.Win32Bool.FALSE;
		}

		var success = _env.SynchronizationManager.ResetEvent(hEvent);
		return success ? NativeTypes.Win32Bool.TRUE : NativeTypes.Win32Bool.FALSE;
	}

	[DllModuleExport(37)]
	private uint PulseEvent(uint hEvent)
	{
		_logger.LogInformation("[Kernel32] PulseEvent(handle=0x{Handle:X8})", hEvent);

		if (_env.SynchronizationManager == null)
		{
			_logger.LogWarning("[Kernel32] PulseEvent: SynchronizationManager not available");
			return NativeTypes.Win32Bool.FALSE;
		}

		// PulseEvent sets and immediately resets the event
		// Wake threads waiting on this event
		if (_env.ThreadScheduler == null)
		{
			return NativeTypes.Win32Bool.TRUE;
		}

		var waiters = _env.SynchronizationManager.GetEventWaiters(hEvent);
		foreach (var waiterId in waiters)
		{
			_env.ThreadScheduler.WakeThread(waiterId);
		}

		// The event remains in non-signaled state
		return NativeTypes.Win32Bool.TRUE;
	}

	[DllModuleExport(37)]
	private uint CreateSemaphore(uint lpSemaphoreAttributes, uint lInitialCount, uint lMaximumCount, in LpcStr lpName)
	{
		var name = lpName.ToString();

		_logger.LogInformation("[Kernel32] CreateSemaphore(attr=0x{Attr:X8}, initial={Initial}, max={Max}, name=\"{Name}\")",
			lpSemaphoreAttributes, lInitialCount, lMaximumCount, name ?? "<unnamed>");

		if (_env.SynchronizationManager == null)
		{
			_logger.LogWarning("[Kernel32] CreateSemaphore: SynchronizationManager not available");
			return 0; // NULL handle
		}

		var handle = _env.SynchronizationManager.CreateSemaphore(lInitialCount, lMaximumCount, name, out var alreadyExists);

		if (alreadyExists)
		{
			_lastError = NativeTypes.Win32Error.ERROR_ALREADY_EXISTS;
		}

		return handle;
	}

	[DllModuleExport(37)]
	private uint ReleaseSemaphore(uint hSemaphore, uint lReleaseCount, uint lpPreviousCount)
	{
		_logger.LogInformation("[Kernel32] ReleaseSemaphore(handle=0x{Handle:X8}, count={Count})", hSemaphore, lReleaseCount);

		if (_env.SynchronizationManager == null)
		{
			_logger.LogWarning("[Kernel32] ReleaseSemaphore: SynchronizationManager not available");
			return NativeTypes.Win32Bool.FALSE;
		}

		var success = _env.SynchronizationManager.ReleaseSemaphore(hSemaphore, lReleaseCount, out var previousCount);

		if (lpPreviousCount != 0)
		{
			_env.MemWrite32(lpPreviousCount, previousCount);
		}

		// Wake waiting threads
		if (!success || _env.ThreadScheduler == null)
		{
			return success ? NativeTypes.Win32Bool.TRUE : NativeTypes.Win32Bool.FALSE;
		}

		for (var i = 0; i < lReleaseCount; i++)
		{
			var nextWaiter = _env.SynchronizationManager.GetNextSemaphoreWaiter(hSemaphore);
			if (nextWaiter.HasValue)
			{
				_env.ThreadScheduler.WakeThread(nextWaiter.Value);
			}
		}

		return success ? NativeTypes.Win32Bool.TRUE : NativeTypes.Win32Bool.FALSE;
	}

	[DllModuleExport(37)]
	private uint WaitForSingleObject(uint hHandle, uint dwMilliseconds)
	{
		var currentThreadId = _env.GetCurrentThreadId();
		_logger.LogInformation("[Kernel32] WaitForSingleObject(handle=0x{Handle:X8}, timeout={Timeout}ms) - Thread {ThreadId}", 
			hHandle, dwMilliseconds, currentThreadId);

		if (_env.SynchronizationManager == null)
		{
			_logger.LogWarning("[Kernel32] WaitForSingleObject: SynchronizationManager not available");
			return 0xFFFFFFFF; // WAIT_FAILED
		}

		const uint WAIT_OBJECT_0 = 0;
		const uint WAIT_TIMEOUT = 0x102;
		const uint WAIT_FAILED = 0xFFFFFFFF;

		// Check what type of object this is
		var objectType = _env.SynchronizationManager.GetObjectType(hHandle);
		
		if (objectType == null)
		{
			_logger.LogWarning("[Kernel32] WaitForSingleObject: invalid handle 0x{Handle:X8}", hHandle);
			return WAIT_FAILED;
		}

		// Start time for timeout tracking
		var startTime = DateTime.UtcNow;
		var timeoutSpan = dwMilliseconds == 0xFFFFFFFF 
			? TimeSpan.MaxValue 
			: TimeSpan.FromMilliseconds(dwMilliseconds);

		// Polling loop to wait for object - implements blocking behavior
		while (true)
		{
			// Try to acquire/wait on the synchronization object
			var signaled = objectType switch
			{
				"Mutex" => _env.SynchronizationManager.AcquireMutex(hHandle, currentThreadId),
				"Event" => _env.SynchronizationManager.WaitOnEvent(hHandle, currentThreadId),
				"Semaphore" => _env.SynchronizationManager.WaitOnSemaphore(hHandle, currentThreadId),
				_ => false
			};

			if (signaled)
			{
				// Object is now available
				_logger.LogDebug("[Kernel32] WaitForSingleObject: Thread {ThreadId} successfully acquired {Type} 0x{Handle:X8}", 
					currentThreadId, objectType, hHandle);
				return WAIT_OBJECT_0;
			}

			// Check timeout
			if (dwMilliseconds == 0)
			{
				// Zero timeout - return immediately without waiting
				_logger.LogDebug("[Kernel32] WaitForSingleObject: Zero timeout, returning WAIT_TIMEOUT");
				return WAIT_TIMEOUT;
			}

			var elapsed = DateTime.UtcNow - startTime;
			if (elapsed >= timeoutSpan)
			{
				// Timeout expired
				_logger.LogDebug("[Kernel32] WaitForSingleObject: Timeout expired after {Elapsed}ms", elapsed.TotalMilliseconds);
				return WAIT_TIMEOUT;
			}

			// Object not available yet - yield and retry
			// Use a small sleep to prevent busy-waiting and allow other threads to run
			Thread.Sleep(1);
			
			// Yield to thread scheduler if available
			_env.ThreadScheduler?.ProcessWaitTimeouts();
		}
	}

	// Directory functions
	private uint SetCurrentDirectoryA(in LpcStr lpPathName)
	{
		var path = lpPathName.ToString();
		if (string.IsNullOrEmpty(path))
		{
			_logger.LogInformation("[Kernel32] SetCurrentDirectoryA failed: Invalid path (empty or null)");
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}

		// Convert to Windows path format if using VFS
		if (_env.VirtualFileSystem != null)
		{
			path = _env.VirtualFileSystem.ToWindowsPath(path);
		}

		_logger.LogInformation("[Kernel32] SetCurrentDirectoryA(\"{Path}\")", path);
		_env.CurrentDirectory = path;
		return NativeTypes.Win32Bool.TRUE;
	}

	private uint GetCurrentDirectoryA(uint nBufferLength, in LpStr lpBuffer)
	{
		var currentDir = _env.CurrentDirectory;
		var requiredLength = (uint)currentDir.Length + 1; // +1 for null terminator

		_logger.LogInformation("[Kernel32] GetCurrentDirectoryA({NBufferLength}, 0x{LpBuffer:X8}) -> \"{CurrentDir}\"", nBufferLength, lpBuffer.Address, currentDir);

		if (nBufferLength == 0)
		{
			// Return required buffer size
			return requiredLength;
		}

		if (nBufferLength < requiredLength)
		{
			// Buffer too small, return required size
			_lastError = NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
			return requiredLength;
		}

		// Write the current directory to the buffer
		lpBuffer.Write(_env.Memory, currentDir, true);
		return (uint)currentDir.Length; // Return length without null terminator
	}

	// String functions
	private uint LstrcatA(in LpStr lpString1, in LpcStr lpString2)
	{
		var str1 = lpString1.Read(_env.Memory);
		var str2 = lpString2.ToString();

		_logger.LogInformation("[Kernel32] LstrcatA(\"{Str1}\", \"{Str2}\")", str1, str2);

		// Concatenate and write back
		var result = str1 + str2;
		lpString1.Write(_env.Memory, result, true);

		// Return pointer to destination string
		return lpString1.Address;
	}

	// Process execution
	private uint WinExec(in LpcStr lpCmdLine, uint uCmdShow)
	{
		var cmdLine = lpCmdLine.ToString();
		_logger.LogInformation("[Kernel32] WinExec(\"{CmdLine}\", {UCmdShow})", cmdLine, uCmdShow);
		
		if (cmdLine == null)
		{
			_logger.LogWarning("[Kernel32] WinExec: null command line");
			return 2; // ERROR_FILE_NOT_FOUND
		}

		// Parse command line to extract executable path
		var executable = cmdLine.Trim();
		if (executable.StartsWith('"'))
		{
			var endQuote = executable.IndexOf('"', 1);
			if (endQuote > 0)
			{
				executable = executable.Substring(1, endQuote - 1);
			}
		}
		else
		{
			var spaceIndex = executable.IndexOf(' ');
			if (spaceIndex > 0)
			{
				executable = executable.Substring(0, spaceIndex);
			}
		}

		_logger.LogInformation("[Kernel32] WinExec: Parsed executable path: \"{Executable}\"", executable);

		// For now, we just log that an attempt was made to execute a program
		// A full implementation would need to support launching child processes
		// Return success (33 or higher indicates success in WinExec)
		return 33; // SE_ERR_SUCCESS (actually any value > 31 indicates success)
	}

	// Critical section synchronization functions
	// In a single-threaded emulator, these are essentially no-ops but we need to initialize the structure properly
	[DllModuleExport(1)]
	private uint InitializeCriticalSection(uint lpCriticalSection)
	{
		_logger.LogInformation("[Kernel32] InitializeCriticalSection(0x{LpCriticalSection:X8})", lpCriticalSection);

		if (lpCriticalSection == 0)
		{
			_logger.LogWarning("[Kernel32] InitializeCriticalSection: null pointer");
			return 0;
		}

		// Initialize the CRITICAL_SECTION structure with default values
		var criticalSection = new NativeTypes.CriticalSection
		{
			DebugInfo = 0,       // NULL (simplified)
			LockCount = -1,      // -1 means unlocked
			RecursionCount = 0,  // Initially 0
			OwningThread = 0,    // NULL initially
			LockSemaphore = 0,   // NULL
			SpinCount = 0        // 0 for single-threaded
		};

		_env.MemWriteStruct(lpCriticalSection, ref criticalSection);

		return 0; // This function returns void, but we return 0 for consistency
	}

	[DllModuleExport(1)]
	private uint DeleteCriticalSection(uint lpCriticalSection)
	{
		_logger.LogInformation("[Kernel32] DeleteCriticalSection(0x{LpCriticalSection:X8})", lpCriticalSection);

		if (lpCriticalSection == 0)
		{
			_logger.LogWarning("[Kernel32] DeleteCriticalSection: null pointer");
			return 0;
		}

		// In our single-threaded emulator, we just need to clear the structure
		// A real implementation would release any associated semaphore and free debug info
		var criticalSection = new NativeTypes.CriticalSection
		{
			DebugInfo = 0,
			LockCount = 0,
			RecursionCount = 0,
			OwningThread = 0,
			LockSemaphore = 0,
			SpinCount = 0
		};
		_env.MemWriteStruct(lpCriticalSection, ref criticalSection);

		return 0; // This function returns void, but we return 0 for consistency
	}

	[DllModuleExport(1)]
	private uint EnterCriticalSection(uint lpCriticalSection)
	{
		_logger.LogInformation("[Kernel32] EnterCriticalSection(0x{LpCriticalSection:X8})", lpCriticalSection);

		if (lpCriticalSection == 0)
		{
			_logger.LogWarning("[Kernel32] EnterCriticalSection: null pointer");
			return 0;
		}

		// In a single-threaded emulator, this is a no-op since there's no contention
		// However, we update the structure to maintain correct state for any code that reads it
		
		// Read current state
		var criticalSection = _env.MemReadStruct<NativeTypes.CriticalSection>(lpCriticalSection);
		var currentThreadId = _env.GetCurrentThreadId();

		if (criticalSection.OwningThread == 0)
		{
			// Critical section is not owned, acquire it
			criticalSection.LockCount = 0;  // 0 means locked once
			criticalSection.RecursionCount = 1;
			criticalSection.OwningThread = currentThreadId;
		}
		else if (criticalSection.OwningThread == currentThreadId)
		{
			// Re-entering from the same thread
			criticalSection.LockCount++;
			criticalSection.RecursionCount++;
		}
		else
		{
			// In a real multi-threaded scenario, this would block
			// For single-threaded emulator, this shouldn't happen
			_logger.LogWarning("[Kernel32] EnterCriticalSection: unexpected thread ownership (owner=0x{OwningThread:X8}, current=0x{CurrentThreadId:X8})", criticalSection.OwningThread, currentThreadId);
		}

		_env.MemWriteStruct(lpCriticalSection, ref criticalSection);

		return 0; // This function returns void, but we return 0 for consistency
	}

	[DllModuleExport(1)]
	private uint LeaveCriticalSection(uint lpCriticalSection)
	{
		_logger.LogInformation("[Kernel32] LeaveCriticalSection(0x{LpCriticalSection:X8})", lpCriticalSection);

		if (lpCriticalSection == 0)
		{
			_logger.LogWarning("[Kernel32] LeaveCriticalSection: null pointer");
			return 0;
		}

		// Read current state
		var criticalSection = _env.MemReadStruct<NativeTypes.CriticalSection>(lpCriticalSection);
		var currentThreadId = _env.GetCurrentThreadId();

		if (criticalSection.OwningThread != currentThreadId)
		{
			_logger.LogWarning("[Kernel32] LeaveCriticalSection: thread 0x{CurrentThreadId:X8} does not own critical section (owner=0x{OwningThread:X8})", currentThreadId, criticalSection.OwningThread);
			return 0;
		}

		// Decrement recursion count
		criticalSection.RecursionCount--;
		
		if (criticalSection.RecursionCount == 0)
		{
			// Fully releasing the critical section
			criticalSection.LockCount = -1;  // -1 means unlocked
			criticalSection.OwningThread = 0;  // NULL
		}
		else
		{
			// Still owned by this thread (recursive lock)
			criticalSection.LockCount--;
		}

		_env.MemWriteStruct(lpCriticalSection, ref criticalSection);

		return 0; // This function returns void, but we return 0 for consistency
	}

	/// <summary>
	/// Finds a resource in the specified module.
	/// </summary>
	/// <param name="hModule">Handle to the module whose executable file contains the resource</param>
	/// <param name="lpName">Resource name (can be integer ID or string pointer)</param>
	/// <param name="lpType">Resource type (can be integer ID or string pointer)</param>
	/// <returns>Handle to the resource information block, or NULL if not found</returns>
	[DllModuleExport(302, entryPoint: 0x00008D8E, Version = "5.1.2600.6532")]
	private uint FindResourceA(uint hModule, uint lpName, uint lpType)
	{
		_logger.LogInformation("[Kernel32] FindResourceA: hModule=0x{HModule:X8} lpName=0x{LpName:X8} lpType=0x{LpType:X8}",
			hModule, lpName, lpType);

		if (_resourceReader == null)
		{
			_logger.LogWarning("[Kernel32] FindResourceA: Resource reader not initialized");
			_lastError = NativeTypes.Win32Error.ERROR_PROC_NOT_FOUND;
			return 0;
		}

		try
		{
			var result = _resourceReader.FindResource(lpType, lpName, 0);
			if (result == 0)
			{
				_lastError = NativeTypes.Win32Error.ERROR_PROC_NOT_FOUND;
			}
			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] FindResourceA: Exception occurred");
			_lastError = NativeTypes.Win32Error.ERROR_PROC_NOT_FOUND;
			return 0;
		}
	}

	/// <summary>
	/// Loads a resource into memory.
	/// </summary>
	/// <param name="hModule">Handle to the module containing the resource</param>
	/// <param name="hResInfo">Handle to the resource (from FindResource)</param>
	/// <returns>Handle to the loaded resource data, or NULL if failed</returns>
	[DllModuleExport(459, entryPoint: 0x0000A6E7, Version = "5.1.2600.6532")]
	private uint LoadResource(uint hModule, uint hResInfo)
	{
		_logger.LogInformation("[Kernel32] LoadResource: hModule=0x{HModule:X8} hResInfo=0x{HResInfo:X8}",
			hModule, hResInfo);

		if (_resourceReader == null)
		{
			_logger.LogWarning("[Kernel32] LoadResource: Resource reader not initialized");
			_lastError = NativeTypes.Win32Error.ERROR_PROC_NOT_FOUND;
			return 0;
		}

		try
		{
			var result = _resourceReader.LoadResource(hModule, hResInfo);
			if (result == 0)
			{
				_lastError = NativeTypes.Win32Error.ERROR_PROC_NOT_FOUND;
			}
			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] LoadResource: Exception occurred");
			_lastError = NativeTypes.Win32Error.ERROR_PROC_NOT_FOUND;
			return 0;
		}
	}

	/// <summary>
	/// Gets the size of a resource.
	/// </summary>
	/// <param name="hModule">Handle to the module containing the resource</param>
	/// <param name="hResInfo">Handle to the resource</param>
	/// <returns>Size of the resource in bytes, or 0 if failed</returns>
	[DllModuleExport(680, entryPoint: 0x0000F25A, Version = "5.1.2600.6532")]
	private uint SizeofResource(uint hModule, uint hResInfo)
	{
		_logger.LogInformation("[Kernel32] SizeofResource: hModule=0x{HModule:X8} hResInfo=0x{HResInfo:X8}",
			hModule, hResInfo);

		if (_resourceReader == null)
		{
			_logger.LogWarning("[Kernel32] SizeofResource: Resource reader not initialized");
			return 0;
		}

		try
		{
			return _resourceReader.SizeofResource(hModule, hResInfo);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] SizeofResource: Exception occurred");
			return 0;
		}
	}

	/// <summary>
	/// Locks a resource into memory.
	/// </summary>
	/// <param name="hResData">Handle to the resource data</param>
	/// <returns>Pointer to the resource data</returns>
	[DllModuleExport(460, entryPoint: 0x0000A6F9, Version = "5.1.2600.6532")]
	private uint LockResource(uint hResData)
	{
		_logger.LogInformation("[Kernel32] LockResource: hResData=0x{HResData:X8}", hResData);

		if (_resourceReader == null)
		{
			_logger.LogWarning("[Kernel32] LockResource: Resource reader not initialized");
			return 0;
		}

		try
		{
			return _resourceReader.LockResource(hResData);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] LockResource: Exception occurred");
			return 0;
		}
	}
}