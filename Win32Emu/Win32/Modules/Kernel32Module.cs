using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.Threading;
using Win32Emu.VirtualFileSystem;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Win32.Modules;

internal class Kernel32Module : IWin32ModuleUnsafe
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

	// Win32 API constants for CreateFile
	private const uint GENERIC_READ = 0x80000000;
	private const uint GENERIC_WRITE = 0x40000000;
	private const uint CREATE_NEW = 1;
	private const uint CREATE_ALWAYS = 2;
	private const uint OPEN_EXISTING = 3;
	private const uint OPEN_ALWAYS = 4;
	private const uint TRUNCATE_EXISTING = 5;

	// Toolhelp32 snapshot handle (dummy value for stub implementation)
	private const uint TH32_SNAPSHOT_HANDLE = 0x00007000;

	// SetSearchPathMode constants
	private const uint BASE_SEARCH_PATH_ENABLE_SAFE_SEARCHMODE = 0x00000001;
	private const uint BASE_SEARCH_PATH_DISABLE_SAFE_SEARCHMODE = 0x00010000;
	private const uint BASE_SEARCH_PATH_PERMANENT = 0x00008000;

	// File copy buffer size
	private const int FILE_COPY_BUFFER_SIZE = 8192;

	// IsBadStringPtr constants
	private const uint BAD_POINTER_TRUE = 1;  // Return value indicating bad pointer
	private const uint BAD_POINTER_FALSE = 0; // Return value indicating good pointer
	private const int MAX_STRING_CHECK_LENGTH_ANSI = 65536;   // Max chars to check for ANSI strings (64KB)
	private const int MAX_STRING_CHECK_LENGTH_UNICODE = 32768; // Max chars to check for Unicode strings (64KB footprint)

	// SleepEx and WaitForSingleObjectEx return values
	private const uint WAIT_IO_COMPLETION = 0; // Returned when APC completes (not implemented, always return this)

	// File mapping and process handle base values for stub implementations
	private const uint FILE_MAPPING_HANDLE_BASE = 0x50000000; // Base value for file mapping handles
	private const uint PROCESS_HANDLE_BASE = 0x80000000;      // Base value for process handles

	private Win32Dispatcher? _dispatcher;
	private uint _lastError;
	private ICpu? _cpu;
	private readonly object _interlockedLock = new();

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
			case "FORMATMESSAGEA":
				returnValue = FormatMessageA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5), a.UInt32(6));
				return true;
			case "EXITPROCESS":
				returnValue = ExitProcess(a.UInt32(0));
				return true;
			case "TERMINATEPROCESS":
				returnValue = TerminateProcess(a.UInt32(0), a.UInt32(1));
				return true;
			case "CREATEPROCESSA":
				returnValue = CreateProcessA(a.LpcStr(0), a.LpStr(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5), a.UInt32(6), a.LpcStr(7), a.UInt32(8), a.UInt32(9));
				return true;
			case "CREATEPROCESSW":
				returnValue = CreateProcessW(a.LpcWStr(0), a.LpWStr(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5), a.UInt32(6), a.LpcWStr(7), a.UInt32(8), a.UInt32(9));
				return true;
			case "OPENPROCESS":
				returnValue = OpenProcess(a.UInt32(0), a.UInt32(1), a.UInt32(2));
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
			case "GETSYSTEMDEFAULTLCID":
				returnValue = GetSystemDefaultLCID();
				return true;
			case "GETUSERDEFAULTLCID":
				returnValue = GetUserDefaultLCID();
				return true;
			case "ISVALIDCODEPAGE":
				returnValue = IsValidCodePage(a.UInt32(0));
				return true;
			case "ISVALIDLOCALE":
				returnValue = IsValidLocale(a.UInt32(0), a.UInt32(1));
				return true;
			case "GETLOCALEINFOA":
				returnValue = GetLocaleInfoA(a.UInt32(0), a.UInt32(1), a.LpStr(2), a.Int32(3));
				return true;
			case "GETUSERDEFAULTLANGID":
				returnValue = GetUserDefaultLangID();
				return true;
			case "GETSYSTEMDEFAULTLANGID":
				returnValue = GetSystemDefaultLangID();
				return true;
			case "GETDATEFORMATA":
				returnValue = GetDateFormatA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.LpcStr(3), a.LpStr(4), a.Int32(5));
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
			case "GETMODULEHANDLEW":
				returnValue = GetModuleHandleW(a.UInt32(0));
				return true;
			case "GETMODULEFILENAMEA":
				returnValue = GetModuleFileNameA(a.Ptr(0), a.Lpstr(1), a.UInt32(2));
				return true;
			case "GETMODULEFILENAMEW":
				returnValue = GetModuleFileNameW(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "LOADLIBRARYA":
				returnValue = LoadLibraryA(a.LpcStr(0));
				return true;
			case "LOADLIBRARYW":
				returnValue = LoadLibraryW(a.UInt32(0));
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
			case "GETCOMMANDLINEW":
				returnValue = GetCommandLineW();
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
			case "SETENVIRONMENTVARIABLEW":
				returnValue = SetEnvironmentVariableW(a.UInt32(0), a.UInt32(1));
				return true;
			case "EXPANDENVIRONMENTSTRINGSA":
				returnValue = ExpandEnvironmentStringsA(a.LpcStr(0), a.LpStr(1), a.UInt32(2));
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
			case "SETCONSOLEOUTPUTCP":
				returnValue = SetConsoleOutputCP(a.UInt32(0));
				return true;
			case "SETCONSOLECP":
				returnValue = SetConsoleCP(a.UInt32(0));
				return true;
			case "SETCONSOLECTRLHANDLER":
				returnValue = SetConsoleCtrlHandler(a.UInt32(0), a.UInt32(1));
				return true;
			case "GETCONSOLEMODE":
				returnValue = GetConsoleMode(a.UInt32(0), a.UInt32(1));
				return true;
			case "SETCONSOLEMODE":
				returnValue = SetConsoleMode(a.UInt32(0), a.UInt32(1));
				return true;
			case "GETNUMBEROFCONSOLEINPUTEVENTS":
				returnValue = GetNumberOfConsoleInputEvents(a.UInt32(0), a.UInt32(1));
				return true;
			case "READCONSOLEINPUTA":
				returnValue = ReadConsoleInputA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;
			case "READCONSOLEA":
				returnValue = ReadConsoleA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;
			case "READCONSOLEINPUTW":
				returnValue = ReadConsoleInputW(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;
			case "PEEKCONSOLEINPUTW":
				returnValue = PeekConsoleInputW(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;
			case "GETCONSOLESCREENBUFFERINFO":
				returnValue = GetConsoleScreenBufferInfo(a.UInt32(0), a.UInt32(1));
				return true;
			case "GETLARGESTCONSOLEWINDOWSIZE":
				returnValue = GetLargestConsoleWindowSize(a.UInt32(0));
				return true;
			case "READCONSOLEOUTPUTCHARACTERA":
				returnValue = ReadConsoleOutputCharacterA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;
			case "SETCONSOLESCREENBUFFERSIZE":
				returnValue = SetConsoleScreenBufferSize(a.UInt32(0), a.UInt32(1));
				return true;
			case "SETCONSOLEWINDOWINFO":
				returnValue = SetConsoleWindowInfo(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "WRITECONSOLEINPUTA":
				returnValue = WriteConsoleInputA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
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
			case "GLOBALCOMPACT":
				returnValue = GlobalCompact(a.UInt32(0));
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
			case "HEAPSIZE":
				returnValue = HeapSize(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "GETPROCESSHEAP":
				returnValue = GetProcessHeap();
				return true;
			case "GETPROCESSFLAGS":
				returnValue = GetProcessFlags(a.UInt32(0));
				return true;
			case "GETPROFILEINTA":
				returnValue = GetProfileIntA(a.LpcStr(0), a.LpcStr(1), a.Int32(2));
				return true;
			case "GETPROFILESTRINGA":
				returnValue = GetProfileStringA(a.LpcStr(0), a.LpcStr(1), a.LpcStr(2), a.UInt32(3), a.UInt32(4));
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
			case "ISBADCODEPTR":
				returnValue = IsBadCodePtr(a.UInt32(0));
				return true;
			case "ISBADREADPTR":
				returnValue = IsBadReadPtr(a.UInt32(0), a.UInt32(1));
				return true;
			case "ISBADWRITEPTR":
				returnValue = IsBadWritePtr(a.UInt32(0), a.UInt32(1));
				return true;
			case "RTLMOVEMEMORY":
				returnValue = RtlMoveMemory(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "RTLZEROMEMORY":
				returnValue = RtlZeroMemory(a.UInt32(0), a.UInt32(1));
				return true;

			// File I/O
			case "CREATEFILEA":
				returnValue = CreateFileA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5),
					a.UInt32(6));
				return true;
			case "CREATEFILEW":
				returnValue = CreateFileW(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5),
					a.UInt32(6));
				return true;
			case "READFILE":
				returnValue = ReadFile((void*)a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;
			case "WRITEFILE":
				returnValue = WriteFile(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;
			case "GETOVERLAPPEDRESULT":
				returnValue = GetOverlappedResult(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;
			case "CLOSEHANDLE":
				returnValue = CloseHandle((void*)a.UInt32(0));
				return true;
			case "CONVERTTOGLOBALHANDLE":
				returnValue = ConvertToGlobalHandle(a.UInt32(0));
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
			case "MOVEFILEEXA":
				returnValue = MoveFileExA(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "COPYFILEA":
				returnValue = CopyFileA(a.LpcStr(0), a.LpcStr(1), a.UInt32(2));
				return true;
			case "SETFILEATTRIBUTESA":
				returnValue = SetFileAttributesA(a.LpcStr(0), a.UInt32(1));
				return true;
			case "GETFILEATTRIBUTESA":
				returnValue = GetFileAttributesA(a.LpcStr(0));
				return true;
			case "OPENFILE":
				returnValue = OpenFile(a.LpcStr(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "GETDISKFREESPACEA":
				returnValue = GetDiskFreeSpaceA(a.LpcStr(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;
			case "GETDRIVETYPEA":
				returnValue = GetDriveTypeA(a.LpcStr(0));
				return true;
			case "GETLOGICALDRIVESTRINGS":
				returnValue = GetLogicalDriveStringsA(a.UInt32(0), a.LpStr(1));
				return true;
			case "GETLOGICALDRIVESTRINGA":
				returnValue = GetLogicalDriveStringsA(a.UInt32(0), a.LpStr(1));
				return true;
			case "GETLOGICALDRIVES":
				returnValue = GetLogicalDrives();
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
			case "SETUNHANDLEDEXCEPTIONFILTER":
				returnValue = SetUnhandledExceptionFilter(a.UInt32(0));
				return true;
			case "OUTPUTDEBUGSTRINGA":
				returnValue = OutputDebugStringA(a.LpcStr(0));
				return true;
			case "DEBUGBREAK":
				returnValue = DebugBreak();
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
			case "GETSYSTEMTIME":
				returnValue = GetSystemTime(a.UInt32(0));
				return true;
			case "GETLOCALTIME":
				returnValue = GetLocalTime(a.UInt32(0));
				return true;
			case "GETSYSTEMTIMEASFILETIME":
				returnValue = GetSystemTimeAsFileTime(a.UInt32(0));
				return true;
			case "DOSDATETIMETOFILETIME":
				returnValue = DosDateTimeToFileTime(a.UInt32(0), a.UInt32(1), a.UInt32(2));
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
			case "SLEEPEX":
				returnValue = SleepEx(a.UInt32(0), a.UInt32(1));
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
			case "TERMINATETHREAD":
				returnValue = TerminateThread(a.UInt32(0), a.UInt32(1));
				return true;
			case "GETEXITCODETHREAD":
				returnValue = GetExitCodeThread(a.UInt32(0), a.UInt32(1));
				return true;
			case "GETCURRENTTHREADID":
				returnValue = GetCurrentThreadId();
				return true;
			case "GETCURRENTTHREAD":
				returnValue = GetCurrentThread();
				return true;
			case "ISWOW64PROCESS":
				returnValue = IsWow64Process(a.UInt32(0), a.UInt32(1));
				return true;
			case "GETPROCESSAFFINITYMASK":
				returnValue = GetProcessAffinityMask(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "GETSYSTEMINFO":
				returnValue = GetSystemInfo(a.UInt32(0));
				return true;
			case "SETTHREADAFFINITYMASK":
				returnValue = SetThreadAffinityMask(a.UInt32(0), a.UInt32(1));
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
			case "OPENSEMAPHOREA":
				returnValue = OpenSemaphoreA(a.UInt32(0), a.UInt32(1), a.LpcStr(2));
				return true;
			case "RELEASESEMAPHORE":
				returnValue = ReleaseSemaphore(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "WAITFORSINGLEOBJECT":
				returnValue = WaitForSingleObject(a.UInt32(0), a.UInt32(1));
				return true;
			case "WAITFORSINGLEOBJECTEX":
				returnValue = WaitForSingleObjectEx(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "WAITFORMULTIPLEOBJECTS":
				returnValue = WaitForMultipleObjects(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;

			// Directory functions
			case "SETCURRENTDIRECTORYA":
				returnValue = SetCurrentDirectoryA(a.LpcStr(0));
				return true;
			case "GETCURRENTDIRECTORYA":
				returnValue = GetCurrentDirectoryA(a.UInt32(0), a.LpStr(1));
				return true;
			case "GETCURRENTDIRECTORYW":
				returnValue = GetCurrentDirectoryW(a.UInt32(0), a.LpWStr(1));
				return true;
			case "CREATEDIRECTORYA":
				returnValue = CreateDirectoryA(a.LpcStr(0), a.UInt32(1));
				return true;
			case "GETWINDOWSDIRECTORYA":
				returnValue = GetWindowsDirectoryA(a.LpStr(0), a.UInt32(1));
				return true;
			case "GETPRIVATEPROFILESTRINGA":
				returnValue = GetPrivateProfileStringA(a.LpcStr(0), a.LpcStr(1), a.LpcStr(2), a.LpStr(3), a.UInt32(4), a.LpcStr(5));
				return true;
			case "GETPRIVATEPROFILESECTIONA":
				returnValue = GetPrivateProfileSectionA(a.LpcStr(0), a.LpStr(1), a.UInt32(2), a.LpcStr(3));
				return true;

			// String functions
			case "LSTRCATA":
				returnValue = LstrcatA(a.LpStr(0), a.LpcStr(1));
				return true;
			case "LSTRCPYA":
				returnValue = LstrcpyA(a.LpStr(0), a.LpcStr(1));
				return true;
			case "LSTRLENA":
				returnValue = LstrlenA(a.LpcStr(0));
				return true;
			case "LSTRLENW":
				returnValue = LstrlenW(a.UInt32(0));
				return true;
			case "ISDBCSLEADBYTE":
				returnValue = IsDBCSLeadByte(a.UInt32(0));
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
			case "FREERESOURCE":
				returnValue = FreeResource(a.UInt32(0));
				return true;
			case "ENUMRESOURCENAMESA":
				returnValue = EnumResourceNamesA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;

			// Additional missing functions
			case "DEVICEIOCONTROL":
				returnValue = DeviceIoControl(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5), a.UInt32(6), a.UInt32(7));
				return true;
			case "EXITTHREAD":
				returnValue = ExitThread(a.UInt32(0));
				return true;
			case "FREELIBRARY":
				returnValue = FreeLibrary(a.UInt32(0));
				return true;
			case "GETCOMPUTERNAMEA":
				returnValue = GetComputerNameA(a.UInt32(0), a.UInt32(1));
				return true;
			case "GETCURRENTPROCESSID":
				returnValue = GetCurrentProcessId();
				return true;
			case "GETENVIRONMENTVARIABLEA":
				returnValue = GetEnvironmentVariableA(a.LpcStr(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "GETPRIORITYCLASS":
				returnValue = GetPriorityClass(a.UInt32(0));
				return true;
			case "GETPROCESSVERSION":
				returnValue = GetProcessVersion(a.UInt32(0));
				return true;
			case "GETSYSTEMDIRECTORYA":
				returnValue = GetSystemDirectoryA(a.UInt32(0), a.UInt32(1));
				return true;
			case "GETTEMPPATHA":
				returnValue = GetTempPathA(a.UInt32(0), a.UInt32(1));
				return true;
			case "GETTHREADPRIORITY":
				returnValue = (uint)GetThreadPriority(a.UInt32(0));
				return true;
			case "GLOBALADDATOMA":
				returnValue = GlobalAddAtomA(a.LpcStr(0));
				return true;
			case "GLOBALDELETEATOM":
				returnValue = GlobalDeleteAtom(a.UInt32(0));
				return true;
			case "GLOBALFINDATOMA":
				returnValue = GlobalFindAtomA(a.LpcStr(0));
				return true;
			case "GLOBALFLAGS":
				returnValue = GlobalFlags(a.UInt32(0));
				return true;
			case "GLOBALGETATOMNAMEA":
				returnValue = GlobalGetAtomNameA(a.UInt32(0), a.UInt32(1), a.Int32(2));
				return true;
			case "GLOBALMEMORYSTATUS":
				returnValue = GlobalMemoryStatus(a.UInt32(0));
				return true;
			case "GLOBALREALLOC":
				returnValue = GlobalReAlloc(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "GLOBALSIZE":
				returnValue = GlobalSize(a.UInt32(0));
				return true;
			case "INTERLOCKEDDECREMENT":
				returnValue = (uint)InterlockedDecrement(a.UInt32(0));
				return true;
			case "INTERLOCKEDINCREMENT":
				returnValue = (uint)InterlockedIncrement(a.UInt32(0));
				return true;
			case "INTERLOCKEDEXCHANGE":
				returnValue = InterlockedExchange(a.UInt32(0), a.UInt32(1));
				return true;
			case "INTERLOCKEDCOMPAREEXCHANGE":
				returnValue = InterlockedCompareExchange(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "LOCALFREE":
				returnValue = LocalFree(a.UInt32(0));
				return true;
			case "LOCALREALLOC":
				returnValue = LocalReAlloc(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "LOCALHANDLE":
				returnValue = LocalHandle(a.UInt32(0));
				return true;
			case "LSTRCMPA":
				returnValue = (uint)lstrcmpA(a.LpcStr(0), a.LpcStr(1));
				return true;
			case "LSTRCMPIA":
				returnValue = (uint)lstrcmpiA(a.LpcStr(0), a.LpcStr(1));
				return true;
			case "LSTRCPYNA":
				returnValue = lstrcpynA(a.UInt32(0), a.LpcStr(1), a.Int32(2));
				return true;
			case "MULDIV":
				returnValue = (uint)MulDiv(a.Int32(0), a.Int32(1), a.Int32(2));
				return true;
			case "OPENEVENTA":
				returnValue = OpenEventA(a.UInt32(0), a.UInt32(1), a.LpcStr(2));
				return true;
			case "OPENMUTEXA":
				returnValue = OpenMutexA(a.UInt32(0), a.UInt32(1), a.LpcStr(2));
				return true;
			case "REMOVEDIRECTORYA":
				returnValue = RemoveDirectoryA(a.LpcStr(0));
				return true;
			case "SETERRORMODE":
				returnValue = SetErrorMode(a.UInt32(0));
				return true;
			case "SETPRIORITYCLASS":
				returnValue = SetPriorityClass(a.UInt32(0), a.UInt32(1));
				return true;
			case "SETTHREADPRIORITY":
				returnValue = SetThreadPriority(a.UInt32(0), a.Int32(1));
				return true;
			case "WRITECONSOLEA":
				returnValue = WriteConsoleA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;
			case "WRITEPRIVATEPROFILESTRINGA":
				returnValue = WritePrivateProfileStringA(a.LpcStr(0), a.LpcStr(1), a.LpcStr(2), a.LpcStr(3));
				return true;
			case "WRITEPRIVATEPROFILESECTIONA":
				returnValue = WritePrivateProfileSectionA(a.LpcStr(0), a.LpcStr(1), a.LpcStr(2));
				return true;
			case "WRITEPROFILESTRINGA":
				returnValue = WriteProfileStringA(a.LpcStr(0), a.LpcStr(1), a.LpcStr(2));
				return true;
			case "DUPLICATEHANDLE":
				returnValue = DuplicateHandle(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5), a.UInt32(6));
				return true;
			case "GETEXITCODEPROCESS":
				returnValue = GetExitCodeProcess(a.UInt32(0), a.UInt32(1));
				return true;
			case "GETFILESIZE":
				returnValue = GetFileSize(a.UInt32(0), a.UInt32(1));
				return true;
			case "GETFILETIME":
				returnValue = GetFileTime(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;
			case "GETFULLPATHNAMEA":
				returnValue = GetFullPathNameA(a.LpcStr(0), a.UInt32(1), a.LpStr(2), a.UInt32(3));
				return true;
			case "GETLOCALEINFOW":
				returnValue = GetLocaleInfoW(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.Int32(3));
				return true;
			case "GETVOLUMEINFORMATIONA":
				returnValue = GetVolumeInformationA(a.LpcStr(0), a.LpStr(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5), a.LpStr(6), a.UInt32(7));
				return true;
			case "LOCKFILE":
				returnValue = LockFile(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;
			case "UNLOCKFILE":
				returnValue = UnlockFile(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;
			case "FATALAPPEXITA":
				returnValue = FatalAppExitA(a.UInt32(0), a.LpcStr(1));
				return true;
			case "GETPRIVATEPROFILEINTA":
				returnValue = GetPrivateProfileIntA(a.LpcStr(0), a.LpcStr(1), a.Int32(2), a.LpcStr(3));
				return true;
			case "GETSHORTPATHNAMEA":
				returnValue = GetShortPathNameA(a.LpcStr(0), a.LpStr(1), a.UInt32(2));
				return true;
			case "GETSTRINGTYPEEXA":
				returnValue = GetStringTypeExA(a.UInt32(0), a.UInt32(1), a.LpcStr(2), a.Int32(3), a.UInt32(4));
				return true;
			case "GETTEMPFILENAMEA":
				returnValue = GetTempFileNameA(a.LpcStr(0), a.LpcStr(1), a.UInt32(2), a.LpStr(3));
				return true;
			case "GETTHREADLOCALE":
				returnValue = GetThreadLocale();
				return true;
			case "LOCALFILETIMETOFILETIME":
				returnValue = LocalFileTimeToFileTime(a.UInt32(0), a.UInt32(1));
				return true;
			case "SETFILETIME":
				returnValue = SetFileTime(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;
			case "SYSTEMTIMETOFILETIME":
				returnValue = SystemTimeToFileTime(a.UInt32(0), a.UInt32(1));
				return true;

			// Legacy file I/O functions (16-bit compatibility)
			case "_LCLOSE":
				returnValue = _lclose(a.UInt32(0));
				return true;
			case "_LCREAT":
				returnValue = _lcreat(a.LpcStr(0), a.Int32(1));
				return true;
			case "_LWRITE":
				returnValue = _lwrite(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "_LOPEN":
				returnValue = _lopen(a.LpcStr(0), a.Int32(1));
				return true;
			case "_LREAD":
				returnValue = _lread(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "_LLSEEK":
				returnValue = _llseek(a.UInt32(0), a.Int32(1), a.Int32(2));
				return true;

			// Memory mapping functions
			case "CREATEFILEMAPPINGA":
				returnValue = CreateFileMappingA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.LpcStr(5));
				return true;
			case "CREATEFILEMAPPINGW":
				returnValue = CreateFileMappingW(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.LpcWStr(5));
				return true;
			case "OPENFILEMAPPINGA":
				returnValue = OpenFileMappingA(a.UInt32(0), a.UInt32(1), a.LpcStr(2));
				return true;
			case "OPENFILEMAPPINGW":
				returnValue = OpenFileMappingW(a.UInt32(0), a.UInt32(1), a.LpcWStr(2));
				return true;
			case "MAPVIEWOFFILE":
				returnValue = MapViewOfFile(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;
			case "UNMAPVIEWOFFILE":
				returnValue = UnmapViewOfFile(a.UInt32(0));
				return true;
			case "MAPVIEWOFFILEEX":
				returnValue = MapViewOfFileEx(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5));
				return true;
			case "FLUSHVIEWOFFILE":
				returnValue = FlushViewOfFile(a.UInt32(0), a.UInt32(1));
				return true;

			// Memory protection functions
			case "VIRTUALPROTECT":
				returnValue = VirtualProtect(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;
			case "VIRTUALQUERY":
				returnValue = VirtualQuery(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "VIRTUALUNLOCK":
				returnValue = VirtualUnlock(a.UInt32(0), a.UInt32(1));
				return true;
			case "READPROCESSMEMORY":
				returnValue = ReadProcessMemory(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;

			// File path functions
			case "SEARCHPATHA":
				returnValue = SearchPathA(a.LpcStr(0), a.LpcStr(1), a.LpcStr(2), a.UInt32(3), a.LpStr(4), a.UInt32(5));
				return true;
			case "SETSEARCHPATHMODE":
				returnValue = SetSearchPathMode(a.UInt32(0));
				return true;

			// Locale functions
			case "ENUMSYSTEMLOCALESA":
				returnValue = EnumSystemLocalesA(a.UInt32(0), a.UInt32(1));
				return true;

			// Time conversion functions
			case "FILETIMETODOSDATETIME":
				returnValue = FileTimeToDosDateTime(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;

			// Toolhelp32 snapshot functions
			case "CREATETOOLHELP32SNAPSHOT":
				returnValue = CreateToolhelp32Snapshot(a.UInt32(0), a.UInt32(1));
				return true;
			case "PROCESS32FIRST":
				returnValue = Process32First(a.UInt32(0), a.UInt32(1));
				return true;
			case "PROCESS32NEXT":
				returnValue = Process32Next(a.UInt32(0), a.UInt32(1));
				return true;
			case "THREAD32FIRST":
				returnValue = Thread32First(a.UInt32(0), a.UInt32(1));
				return true;
			case "THREAD32NEXT":
				returnValue = Thread32Next(a.UInt32(0), a.UInt32(1));
				return true;
			case "MODULE32FIRST":
				returnValue = Module32First(a.UInt32(0), a.UInt32(1));
				return true;
			case "MODULE32NEXT":
				returnValue = Module32Next(a.UInt32(0), a.UInt32(1));
				return true;

			// 16-bit thunking functions
			case "FT_EXIT4":
				returnValue = FT_Exit4();
				return true;
			case "FT_EXIT8":
				returnValue = FT_Exit8();
				return true;
			case "FT_EXIT12":
				returnValue = FT_Exit12();
				return true;
			case "FT_EXIT16":
				returnValue = FT_Exit16();
				return true;
			case "FT_EXIT20":
				returnValue = FT_Exit20();
				return true;
			case "FT_EXIT24":
				returnValue = FT_Exit24();
				return true;
			case "FT_EXIT28":
				returnValue = FT_Exit28();
				return true;
			case "FT_EXIT32":
				returnValue = FT_Exit32();
				return true;
			case "FT_EXIT48":
				returnValue = FT_Exit48();
				return true;
			case "FT_PROLOG":
				returnValue = FT_Prolog();
				return true;
			case "FT_THUNK":
				returnValue = FT_Thunk();
				return true;

			// Memory mapping functions (16-bit compatibility)
			case "SMAPLS_IP_EBP_8":
				returnValue = SMapLS_IP_EBP_8();
				return true;
			case "SMAPLS_IP_EBP_12":
				returnValue = SMapLS_IP_EBP_12();
				return true;
			case "SMAPLS_IP_EBP_16":
				returnValue = SMapLS_IP_EBP_16();
				return true;
			case "SMAPLS_IP_EBP_20":
				returnValue = SMapLS_IP_EBP_20();
				return true;
			case "SMAPLS_IP_EBP_24":
				returnValue = SMapLS_IP_EBP_24();
				return true;
			case "SMAPLS_IP_EBP_28":
				returnValue = SMapLS_IP_EBP_28();
				return true;
			case "SMAPLS_IP_EBP_32":
				returnValue = SMapLS_IP_EBP_32();
				return true;
			case "SUNMAPLS_IP_EBP_8":
				returnValue = SUnMapLS_IP_EBP_8();
				return true;
			case "SUNMAPLS_IP_EBP_12":
				returnValue = SUnMapLS_IP_EBP_12();
				return true;
			case "SUNMAPLS_IP_EBP_16":
				returnValue = SUnMapLS_IP_EBP_16();
				return true;
			case "SUNMAPLS_IP_EBP_20":
				returnValue = SUnMapLS_IP_EBP_20();
				return true;
			case "SUNMAPLS_IP_EBP_24":
				returnValue = SUnMapLS_IP_EBP_24();
				return true;
			case "SUNMAPLS_IP_EBP_28":
				returnValue = SUnMapLS_IP_EBP_28();
				return true;
			case "SUNMAPLS_IP_EBP_32":
				returnValue = SUnMapLS_IP_EBP_32();
				return true;
			case "SMAPLS":
				returnValue = SMapLS(a.UInt32(0));
				return true;
			case "SUNMAPLS":
				returnValue = SUnMapLS(a.UInt32(0));
				return true;
			case "MAPLS":
				returnValue = MapLS(a.UInt32(0));
				return true;
			case "UNMAPLS":
				returnValue = UnMapLS(a.UInt32(0));
				return true;
			case "MAPSL":
				returnValue = MapSL(a.UInt32(0));
				return true;
			case "MAPHINSTLS":
				returnValue = MapHInstLS(a.UInt32(0));
				return true;
			case "MAPHINSTLS_PN":
				returnValue = MapHInstLS_PN(a.UInt32(0));
				return true;
			case "MAPSLFIX":
				returnValue = MapSLFix(a.UInt32(0));
				return true;
			case "UNMAPSLFIXARRAY":
				returnValue = UnMapSLFixArray(a.UInt32(0), a.UInt32(1));
				return true;

			// Thunk callback functions
			case "K32THK1632PROLOG":
				returnValue = K32Thk1632Prolog();
				return true;
			case "K32THK1632EPILOG":
				returnValue = K32Thk1632Epilog();
				return true;
			case "CALLBACK16":
				returnValue = Callback16();
				return true;
			case "CALLBACK20":
				returnValue = Callback20();
				return true;

			// String and character set functions
			case "ISBADSTRINGPTRA":
				returnValue = IsBadStringPtrA(a.UInt32(0), a.UInt32(1));
				return true;
			case "ISBADSTRINGPTRW":
				returnValue = IsBadStringPtrW(a.UInt32(0), a.UInt32(1));
				return true;
			case "ISDBCSLEADBYTEEX":
				returnValue = IsDBCSLeadByteEx(a.UInt32(0), a.UInt32(1));
				return true;

			// Missing functions from issue
			case "MAKECRITICALSECTIONGLOBAL":
				returnValue = MakeCriticalSectionGlobal(a.UInt32(0));
				return true;
			case "REINITIALIZECRITICALSECTION":
				ReinitializeCriticalSection(a.UInt32(0));
				returnValue = 1; // Assume success
				return true;
			case "DISABLETHREADLIBRARYCALLS":
				returnValue = DisableThreadLibraryCalls(a.UInt32(0));
				return true;
			case "FREELIBRARYANDEXITTHREAD":
				FreeLibraryAndExitThread(a.UInt32(0), a.UInt32(1));
				returnValue = 0;
				return true;
			case "READFILEEX":
				returnValue = ReadFileEx(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;
			case "WRITEFILEEX":
				returnValue = WriteFileEx(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;
			case "COMPAREFILETIME":
				returnValue = (uint)CompareFileTime(a.UInt32(0), a.UInt32(1));
				return true;
			case "LSTRCPYW":
				returnValue = LstrcpyW(a.UInt32(0), a.UInt32(1));
				return true;
			case "LSTRCMPW":
				returnValue = (uint)LstrcmpW(a.UInt32(0), a.UInt32(1));
				return true;
			case "OPENVXDHANDLE":
				returnValue = OpenVxDHandle(a.UInt32(0));
				return true;
			case "THUNKCONNECT32":
				returnValue = ThunkConnect32(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
				return true;

			// COM port functions
			case "GETCOMMSTATE":
				returnValue = GetCommState(a.UInt32(0), a.UInt32(1));
				return true;
			case "SETCOMMSTATE":
				returnValue = SetCommState(a.UInt32(0), a.UInt32(1));
				return true;
			case "SETCOMMTIMEOUTS":
				returnValue = SetCommTimeouts(a.UInt32(0), a.UInt32(1));
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

	/// <summary>
	/// Opens an existing named semaphore object.
	/// HANDLE OpenSemaphoreA(
	///   [in] DWORD   dwDesiredAccess,
	///   [in] BOOL    bInheritHandle,
	///   [in] LPCSTR  lpName
	/// );
	/// </summary>
	[DllModuleExport(12)]
	public uint OpenSemaphoreA(uint dwDesiredAccess, uint bInheritHandle, LpcStr lpName)
	{
		var name = lpName.ToString() ?? string.Empty;
		_logger.LogInformation("[kernel32] OpenSemaphoreA(dwDesiredAccess=0x{DwDesiredAccess:X8}, bInheritHandle={BInheritHandle}, lpName=\"{LpName}\")",
			dwDesiredAccess, bInheritHandle, name);

		// Try to find an existing semaphore by name
		// For now, just create a new one if not found
		return CreateSemaphore(0, 0, 0x7FFFFFFF, lpName);
	}

	[DllModuleExport(184, entryPoint: 0x00039B1E, Version = "4.90.0.3000")]
	[DllModuleExport(77, entryPoint: 0x0000A749, Version = "5.1.2600.6532")]
	public uint CreateEventW(uint lpEventAttributes, uint bManualReset, uint bInitialState, LpcStr lpName)
	{
		_logger.LogWarning("[kernel32] CreateEventW: lpEventAttributes={lpEventAttributes}, bManualReset={bManualReset}, bInitialState={bInitialState}, lpName={lpName}", lpEventAttributes, bManualReset, bInitialState, lpName);
		return CreateEvent(lpEventAttributes, bManualReset, bInitialState, lpName);
	}

	[DllModuleExport(183, entryPoint: 0x00007568, Version = "4.90.0.3000")]
	[DllModuleExport(76, entryPoint: 0x00030922, Version = "5.1.2600.6532")]
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

	[DllModuleExport(370, entryPoint: 0x0001B03D, Version = "4.90.0.3000")]
	[DllModuleExport(334, entryPoint: 0x0001C123, Version = "5.1.2600.6532")]
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
		if (lpVersionInformation == 0)
		{
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		var size = _env.MemRead32(lpVersionInformation);
		if (size != 156 && size != 148) // sizeof(OSVERSIONINFOEXA) and sizeof(OSVERSIONINFOA)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
			return (uint)NativeTypes.Win32Bool.FALSE;
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

		return (uint)NativeTypes.Win32Bool.TRUE;
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
		if (lpVersionInformation == 0)
		{
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		var size = _env.MemRead32(lpVersionInformation);
		if (size != 284 && size != 276) // sizeof(OSVERSIONINFOEXW) and sizeof(OSVERSIONINFOW)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
			return (uint)NativeTypes.Win32Bool.FALSE;
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

		return (uint)NativeTypes.Win32Bool.TRUE;
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
	/// Formats a message string. The function requires a message definition as input.
	/// DWORD FormatMessageA(
	///   [in]           DWORD   dwFlags,
	///   [in, optional] LPCVOID lpSource,
	///   [in]           DWORD   dwMessageId,
	///   [in]           DWORD   dwLanguageId,
	///   [out]          LPSTR   lpBuffer,
	///   [in]           DWORD   nSize,
	///   [in, optional] va_list *Arguments
	/// );
	/// </summary>
	[DllModuleExport(28)]
	private uint FormatMessageA(uint dwFlags, uint lpSource, uint dwMessageId, uint dwLanguageId, uint lpBuffer, uint nSize, uint arguments)
	{
		_logger.LogInformation("[Kernel32] FormatMessageA(dwFlags=0x{DwFlags:X}, lpSource=0x{LpSource:X8}, dwMessageId=0x{DwMessageId:X}, dwLanguageId=0x{DwLanguageId:X}, lpBuffer=0x{LpBuffer:X8}, nSize={NSize})",
			dwFlags, lpSource, dwMessageId, dwLanguageId, lpBuffer, nSize);

		const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
		const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
		const uint FORMAT_MESSAGE_FROM_STRING = 0x00000400;
		const uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;

		// For now, provide a simple stub implementation
		// A full implementation would look up error messages from a table
		var message = $"Error 0x{dwMessageId:X8}";
		var bytes = System.Text.Encoding.ASCII.GetBytes(message);

		if ((dwFlags & FORMAT_MESSAGE_ALLOCATE_BUFFER) != 0)
		{
			// Allocate buffer and write pointer to lpBuffer
			uint bufferSize = (uint)bytes.Length + 1;
			uint allocAddr = _env.HeapAlloc(0, bufferSize);

			// Write the message
			for (int i = 0; i < bytes.Length; i++)
			{
				_env.MemWrite8(allocAddr + (uint)i, bytes[i]);
			}
			// Null terminate
			_env.MemWrite8(allocAddr + (uint)bytes.Length, 0);

			// Write the pointer to the allocated buffer to *lpBuffer
			_env.MemWrite32(lpBuffer, allocAddr);

			return (uint)bytes.Length; // Number of chars written, not including null
		}
		else if (lpBuffer != 0 && nSize > 0)
		{
			// Write a generic error message
			var bytesToWrite = Math.Min(bytes.Length, (int)nSize - 1);

			for (int i = 0; i < bytesToWrite; i++)
			{
				_env.MemWrite8(lpBuffer + (uint)i, bytes[i]);
			}
			// Null terminate
			_env.MemWrite8(lpBuffer + (uint)bytesToWrite, 0);

			return (uint)bytesToWrite; // Return number of characters written
		}

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
			return (uint)NativeTypes.Win32Bool.TRUE;
		}

		// We don't support terminating other processes
		_logger.LogInformation("[Kernel32] TerminateProcess: Cannot terminate external process handle 0x{HProcess:X8}", hProcess);
		_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
		return (uint)NativeTypes.Win32Bool.FALSE;
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
			return (uint)NativeTypes.Win32Bool.FALSE; // Return FALSE if null pointer
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
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return (uint)NativeTypes.Win32Bool.FALSE;
		}

		// Write the CPINFO structure to emulated memory
		// Validate pointer before casting and writing
		var ptrValue = (ulong)lpCpInfo.Value;
		// Assume emulated memory is 32-bit addressable (0..0xFFFFFFFF)
		if (ptrValue is > uint.MaxValue or 0)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		_env.MemWriteStruct((uint)ptrValue, ref cpInfo);

		return (uint)NativeTypes.Win32Bool.TRUE;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		// We only support CT_CTYPE1 for simplicity
		if (dwInfoType != 1)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
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

		return (uint)NativeTypes.Win32Bool.TRUE;
	}

	[DllModuleExport(22)]
	private uint GetStringTypeW(uint locale, uint dwInfoType, uint lpSrcStr, int cchSrc, uint lpCharType)
	{
		// GetStringTypeW retrieves character type information for Unicode characters
		// Similar to GetStringTypeA but for wide (Unicode) strings
		const int maxStringLengthLimit = 1000;

		if (lpSrcStr == 0 || lpCharType == 0)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		// We only support CT_CTYPE1 for simplicity
		if (dwInfoType != 1)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
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
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return (uint)NativeTypes.Win32Bool.FALSE;
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

		return (uint)NativeTypes.Win32Bool.TRUE;
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
		_lastError = (uint)NativeTypes.Win32Error.ERROR_MOD_NOT_FOUND;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}

		// Read the library name from memory
		var libraryName = lpLibFileName.ToString();
		if (string.IsNullOrEmpty(libraryName))
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
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

			// Export not found in PE image - fall through to check emulated module
			_logger.LogInformation("[Kernel32] GetProcAddress: Export not found in PE image, checking emulated module");
			// Don't return here - fall through to emulated module check below
		}

		// Check if it's an emulated module (either because hModule was not a PE image, or because the export wasn't found in the PE)
		var moduleName = _env.GetModuleFileNameForHandle(hModule);
		if (moduleName == null)
		{
			_logger.LogInformation("[Kernel32] GetProcAddress: Module handle 0x{HModule:X8} not recognized", hModule);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
			return 0;
		}

		// Try to get the emulated module from the dispatcher
		if (_dispatcher == null || !_dispatcher.TryGetModule(moduleName, out var emulatedModule) || emulatedModule == null)
		{
			_logger.LogInformation("[Kernel32] GetProcAddress: Emulated module '{ModuleName}' not found in dispatcher", moduleName);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_MOD_NOT_FOUND;
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

			// Track this as an unknown function call
			if (_dispatcher != null)
			{
				var lookupName = byOrdinal ? $"#{ordinal}" : procName ?? "(null)";
				_dispatcher.TrackUnknownFunction(moduleName, lookupName);
			}

			_lastError = (uint)NativeTypes.Win32Error.ERROR_PROC_NOT_FOUND;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_PROC_NOT_FOUND;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_MOD_NOT_FOUND;
			return 0;
		}

		// Write the export name to a temporary location in memory
		var exportNamePtr = _env.SimpleAlloc((uint)(targetExport.Length + 1));
		_env.WriteAnsiStringAt(exportNamePtr, targetExport);

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
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
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
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return 0;
			}
		}

		if (path == null)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
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

	/// <summary>
	/// Retrieves the command-line string for the current process (Unicode version).
	/// </summary>
	/// <returns>
	/// The return value is a pointer to the command-line string for the current process.
	/// The lifetime of the returned value is managed by the system, applications should not free or modify this value.
	/// </returns>
	[DllModuleExport(0)]
	public uint GetCommandLineW()
	{
		var ptr = _env.CommandLinePtrW;
		if (ptr != 0)
		{
			// Read the command line string for logging
			var cmdLine = _env.ReadUnicodeString(ptr);
			_logger.LogInformation("[Kernel32] GetCommandLineW returning 0x{Ptr:X8}: \"{CmdLine}\"", ptr, cmdLine);
		}
		else
		{
			// If no wide command line exists, convert from ANSI
			var ansiPtr = _env.CommandLinePtr;
			if (ansiPtr != 0)
			{
				var ansiCmdLine = _env.ReadAnsiString(ansiPtr);
				var fixedCmdLine = FixCommandLineEscaping(ansiCmdLine);
				var windowsPath = ConvertToWindowsPath(fixedCmdLine);

				// Allocate memory for wide string
				var wideBytes = Encoding.Unicode.GetBytes(windowsPath + "\0");
				ptr = _env.HeapAlloc(0, (uint)wideBytes.Length);
				_env.MemWriteBytes(ptr, wideBytes);
				_env.CommandLinePtrW = ptr;

				_logger.LogInformation("[Kernel32] GetCommandLineW created wide version at 0x{Ptr:X8}: \"{CmdLine}\"", ptr, windowsPath);
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		// Return success - in a real implementation this would free the memory
		return (uint)NativeTypes.Win32Bool.TRUE;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		// Return success - in a real implementation this would free the memory
		return (uint)NativeTypes.Win32Bool.TRUE;
	}

	[DllModuleExport(19)]
	private uint GetStartupInfoA(uint lpStartupInfo)
	{
		if (lpStartupInfo == 0)
		{
			return 0;
		}

		_env.MemZero(lpStartupInfo, 68);
		var si = new StartupInfoARef(_env.Memory, lpStartupInfo);
		si.cb = 68;
		// Write actual handle values, not pseudo-handle constants
		// When a console is allocated, these should be real inheritable handles
		// When no console exists, these will be 0 (NULL)
		si.hStdInput = _env.StdInputHandle;
		si.hStdOutput = _env.StdOutputHandle;
		si.hStdError = _env.StdErrorHandle;
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

	/// <summary>
	/// Sets the output code page used by the console associated with the calling process.
	/// </summary>
	/// <param name="wCodePageID">The code page identifier.</param>
	/// <returns>Returns TRUE if successful, FALSE otherwise.</returns>
	[DllModuleExport(4)]
	private uint SetConsoleOutputCP(uint wCodePageID)
	{
		_logger.LogInformation("[Kernel32] SetConsoleOutputCP(wCodePageID={WCodePageID})", wCodePageID);

		// For emulation purposes, we accept the call but don't actually change the code page
		// The emulator uses UTF-8/Unicode internally
		return 1; // TRUE - success
	}

	/// <summary>
	/// Sets the handler routine to be called when a console receives certain control signals.
	/// BOOL SetConsoleCtrlHandler(
	///   [in, optional] PHANDLER_ROUTINE HandlerRoutine,
	///   [in]           BOOL            Add
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint SetConsoleCtrlHandler(uint handlerRoutine, uint add)
	{
		_logger.LogInformation("[Kernel32] SetConsoleCtrlHandler(handlerRoutine=0x{HandlerRoutine:X8}, add={Add})",
			handlerRoutine, add);

		// For emulation purposes, we accept the call but don't install a handler
		// The emulator handles its own termination
		return 1; // TRUE - success
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
		return (uint)NativeTypes.Win32Bool.TRUE;
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
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return 0;
			}

			// Allocate new block
			var newMem = _env.HeapAlloc((uint)hHeap, dwBytes);
			if (newMem == 0)
			{
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		return (uint)NativeTypes.Win32Bool.TRUE;
	}

	[DllModuleExport(440, entryPoint: 0x000110B0, Version = "5.1.2600.6532")]
	[DllModuleExport(339, entryPoint: 0x0001A017, Version = "4.90.0.3000")]
	private uint HeapSize(uint hHeap, uint dwFlags, uint lpMem)
	{
		_logger.LogInformation("[Kernel32] HeapSize(hHeap=0x{HHeap:X8}, dwFlags=0x{DwFlags:X8}, lpMem=0x{LpMem:X8})", hHeap, dwFlags, lpMem);

		// Return the size of the heap memory block
		var size = _env.HeapSize(hHeap, lpMem);
		_logger.LogInformation("[Kernel32] HeapSize: Block at 0x{LpMem:X8} has size {Size} bytes", lpMem, size);

		return size;
	}

	[DllModuleExport(517, entryPoint: 0x00011F76, Version = "5.1.2600.6532")]
	[DllModuleExport(411, entryPoint: 0x0001DDDE, Version = "4.90.0.3000")]
	private uint IsBadCodePtr(uint lpfn)
	{
		// IsBadCodePtr verifies that the calling process has read access to the specified code address
		// Returns FALSE (0) if the address is valid, TRUE (non-zero) if invalid
		_logger.LogInformation("[Kernel32] IsBadCodePtr(lpfn=0x{Lpfn:X8})", lpfn);

		// For emulation purposes, we'll check if the address is within our memory space
		// This is a simplification - real Windows checks actual page permissions
		if (lpfn == 0)
		{
			return (uint)NativeTypes.Win32Bool.TRUE; // NULL pointer is invalid
		}

		// For now, assume all non-NULL code pointers are valid
		// A more complete implementation would check against allocated memory regions
		return (uint)NativeTypes.Win32Bool.FALSE;
	}

	[DllModuleExport(518, entryPoint: 0x00011FAC, Version = "5.1.2600.6532")]
	[DllModuleExport(412, entryPoint: 0x0001DE14, Version = "4.90.0.3000")]
	private uint IsBadReadPtr(uint lp, uint ucb)
	{
		// IsBadReadPtr verifies that the calling process has read access to the specified range of memory
		// Returns FALSE (0) if readable, TRUE (non-zero) if not readable
		_logger.LogInformation("[Kernel32] IsBadReadPtr(lp=0x{Lp:X8}, ucb={Ucb})", lp, ucb);

		// For emulation, we'll do a basic check
		if (lp == 0 || ucb == 0)
		{
			return (uint)NativeTypes.Win32Bool.TRUE; // NULL or zero-length is invalid
		}

		// Assume all non-NULL memory is readable for now
		// A complete implementation would check memory protection and allocation
		return (uint)NativeTypes.Win32Bool.FALSE;
	}

	[DllModuleExport(519, entryPoint: 0x00011FE2, Version = "5.1.2600.6532")]
	[DllModuleExport(413, entryPoint: 0x0001DE4A, Version = "4.90.0.3000")]
	private uint IsBadWritePtr(uint lp, uint ucb)
	{
		// IsBadWritePtr verifies that the calling process has write access to the specified range of memory
		// Returns FALSE (0) if writable, TRUE (non-zero) if not writable
		_logger.LogInformation("[Kernel32] IsBadWritePtr(lp=0x{Lp:X8}, ucb={Ucb})", lp, ucb);

		// For emulation, similar to IsBadReadPtr
		if (lp == 0 || ucb == 0)
		{
			return (uint)NativeTypes.Win32Bool.TRUE; // NULL or zero-length is invalid
		}

		// Assume all non-NULL memory is writable for now
		return (uint)NativeTypes.Win32Bool.FALSE;
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
	private uint VirtualAlloc(uint lpAddress, uint dwSize, uint flAllocationType, uint flProtect)
	{
		_logger.LogInformation("[Kernel32] VirtualAlloc(lpAddress=0x{Lp:X8}, dwSize=0x{Size:X8}, alloc=0x{Alloc:X8}, protect=0x{Prot:X8})", lpAddress, dwSize, flAllocationType, flProtect);
		var addr = _env.VirtualAlloc(lpAddress, dwSize, flAllocationType, flProtect);
		_logger.LogInformation("[Kernel32] VirtualAlloc -> 0x{Addr:X8}", addr);
		return addr;
	}

	[DllModuleExport(46)]
	private uint VirtualFree(uint lpAddress, uint dwSize, uint dwFreeType)
	{
		// VirtualFree releases or decommits virtual memory
		// dwFreeType: MEM_DECOMMIT (0x4000) or MEM_RELEASE (0x8000)
		_logger.LogInformation("[Kernel32] VirtualFree(0x{LpAddress:X8}, {DwSize}, 0x{DwFreeType:X})", lpAddress, dwSize, dwFreeType);

		// Call the ProcessEnvironment's VirtualFree implementation
		bool success = _env.VirtualFree(lpAddress, dwSize, dwFreeType);

		if (!success)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		return (uint)NativeTypes.Win32Bool.TRUE;
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

	// Helper method to map Win32 creation disposition to VFS file mode
	private VfsFileMode MapCreationDispositionToVfsMode(uint dwCreationDisposition)
	{
		return dwCreationDisposition switch
		{
			CREATE_NEW => VfsFileMode.CreateNew,
			CREATE_ALWAYS => VfsFileMode.Create,
			OPEN_EXISTING => VfsFileMode.Open,
			OPEN_ALWAYS => VfsFileMode.OpenOrCreate,
			TRUNCATE_EXISTING => VfsFileMode.Truncate,
			_ => VfsFileMode.OpenOrCreate
		};
	}

	// Helper method to map Win32 creation disposition to .NET FileMode
	private FileMode MapCreationDispositionToFileMode(uint dwCreationDisposition)
	{
		return dwCreationDisposition switch
		{
			CREATE_NEW => FileMode.CreateNew,
			CREATE_ALWAYS => FileMode.Create,
			OPEN_EXISTING => FileMode.Open,
			OPEN_ALWAYS => FileMode.OpenOrCreate,
			TRUNCATE_EXISTING => FileMode.Truncate,
			_ => FileMode.OpenOrCreate
		};
	}

	// Helper method to map Win32 desired access to VFS file access
	private VfsFileAccess MapDesiredAccessToVfsAccess(uint dwDesiredAccess)
	{
		if ((dwDesiredAccess & GENERIC_READ) != 0 && (dwDesiredAccess & GENERIC_WRITE) == 0)
		{
			return VfsFileAccess.Read;
		}
		else if ((dwDesiredAccess & GENERIC_WRITE) != 0 && (dwDesiredAccess & GENERIC_READ) == 0)
		{
			return VfsFileAccess.Write;
		}
		return VfsFileAccess.ReadWrite;
	}

	// Helper method to map Win32 desired access to .NET FileAccess
	private FileAccess MapDesiredAccessToFileAccess(uint dwDesiredAccess)
	{
		if ((dwDesiredAccess & GENERIC_READ) != 0 && (dwDesiredAccess & GENERIC_WRITE) == 0)
		{
			return FileAccess.Read;
		}
		else if ((dwDesiredAccess & GENERIC_WRITE) != 0 && (dwDesiredAccess & GENERIC_READ) == 0)
		{
			return FileAccess.Write;
		}
		return FileAccess.ReadWrite;
	}

	[DllModuleExport(2)]
	private uint CreateFileA(uint lpFileName, uint dwDesiredAccess, uint dwShareMode, uint lpSecAttr,
		uint dwCreationDisposition, uint dwFlagsAndAttributes, uint hTemplateFile)
	{
		try
		{
			var path = _env.ReadAnsiString(lpFileName);

			// Log the raw path from the game before any resolution
			_logger.LogInformation("[Kernel32] CreateFileA: Raw path from game: '{Path}'", path);

			// Handle invalid paths (empty, null, or invalid characters)
			if (string.IsNullOrEmpty(path))
			{
				_logger.LogInformation("[Kernel32] CreateFileA failed: Invalid path (empty or null)");
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return (uint)NativeTypes.Win32Handle.INVALID_HANDLE_VALUE;
			}

			// Resolve relative paths using Windows path semantics
			var resolvedPath = WindowsPathUtility.ResolvePath(path, _env.CurrentDirectory);
			if (resolvedPath != path)
			{
				_logger.LogInformation("[Kernel32] CreateFileA: Resolved path '{Path}' to '{ResolvedPath}' (CurrentDirectory: '{CurrentDirectory}')",
					path, resolvedPath, _env.CurrentDirectory);
			}

			// VFS is required for file operations
			if (_env.VirtualFileSystem == null)
			{
				_logger.LogError("[Kernel32] CreateFileA: VFS not initialized");
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
				return (uint)NativeTypes.Win32Handle.INVALID_HANDLE_VALUE;
			}

			var mode = MapCreationDispositionToVfsMode(dwCreationDisposition);
			var access = MapDesiredAccessToVfsAccess(dwDesiredAccess);

			_logger.LogDebug("[Kernel32] CreateFileA: Attempting VFS open with resolved path: '{ResolvedPath}'", resolvedPath);
			var handle = _env.VirtualFileSystem.OpenFile(resolvedPath, mode, access);
			if (handle != null)
			{
				return _env.RegisterHandle(handle);
			}

			_logger.LogInformation("[Kernel32] CreateFileA (VFS) failed: {Path}", resolvedPath);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return (uint)NativeTypes.Win32Handle.INVALID_HANDLE_VALUE;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] CreateFileA failed: {ExMessage}", ex.Message);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return (uint)NativeTypes.Win32Handle.INVALID_HANDLE_VALUE;
		}
	}

	/// <summary>
	/// Creates or opens a file or I/O device (Unicode version).
	/// </summary>
	[DllModuleExport(28)]
	private uint CreateFileW(uint lpFileName, uint dwDesiredAccess, uint dwShareMode, uint lpSecAttr,
		uint dwCreationDisposition, uint dwFlagsAndAttributes, uint hTemplateFile)
	{
		try
		{
			var path = _env.ReadUnicodeString(lpFileName);

			// Log the raw path from the game before any resolution
			_logger.LogDebug("[Kernel32] CreateFileW: Raw path from game: '{Path}'", path);

			// Handle invalid paths (empty, null, or invalid characters)
			if (string.IsNullOrEmpty(path))
			{
				_logger.LogInformation("[Kernel32] CreateFileW failed: Invalid path (empty or null)");
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return (uint)NativeTypes.Win32Handle.INVALID_HANDLE_VALUE;
			}

			// Resolve relative paths using Windows path semantics
			var resolvedPath = WindowsPathUtility.ResolvePath(path, _env.CurrentDirectory);
			if (resolvedPath != path)
			{
				_logger.LogDebug("[Kernel32] CreateFileW: Resolved path '{Path}' to '{ResolvedPath}' (CurrentDirectory: '{CurrentDirectory}')",
					path, resolvedPath, _env.CurrentDirectory);
			}

			// VFS is required for file operations
			if (_env.VirtualFileSystem == null)
			{
				_logger.LogError("[Kernel32] CreateFileW: VFS not initialized");
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
				return (uint)NativeTypes.Win32Handle.INVALID_HANDLE_VALUE;
			}

			var mode = MapCreationDispositionToVfsMode(dwCreationDisposition);
			var access = MapDesiredAccessToVfsAccess(dwDesiredAccess);

			_logger.LogDebug("[Kernel32] CreateFileW: Attempting VFS open with resolved path: '{ResolvedPath}'", resolvedPath);
			var handle = _env.VirtualFileSystem.OpenFile(resolvedPath, mode, access);
			if (handle != null)
			{
				return _env.RegisterHandle(handle);
			}

			_logger.LogInformation("[Kernel32] CreateFileW (VFS) failed: {Path}", resolvedPath);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return (uint)NativeTypes.Win32Handle.INVALID_HANDLE_VALUE;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] CreateFileW failed: {ExMessage}", ex.Message);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return (uint)NativeTypes.Win32Handle.INVALID_HANDLE_VALUE;
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
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
				return (uint)NativeTypes.Win32Bool.FALSE;
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
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
				return (uint)NativeTypes.Win32Bool.FALSE;
			}
		}

		_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
		return (uint)NativeTypes.Win32Bool.FALSE;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
			return (uint)NativeTypes.Win32Bool.FALSE;
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
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
				return (uint)NativeTypes.Win32Bool.FALSE;
			}
		}
		else
		{
			_logger.LogDebug("[Kernel32] WriteFile on file handle 0x{Handle:X8}", handle);
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
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
				return (uint)NativeTypes.Win32Bool.FALSE;
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
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
				return (uint)NativeTypes.Win32Bool.FALSE;
			}
		}

		_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
		return (uint)NativeTypes.Win32Bool.FALSE;
	}

	/// <summary>
	/// Retrieves the results of an overlapped operation on the specified file, named pipe, or communications device.
	/// BOOL GetOverlappedResult(
	///   [in]  HANDLE       hFile,
	///   [in]  LPOVERLAPPED lpOverlapped,
	///   [out] LPDWORD      lpNumberOfBytesTransferred,
	///   [in]  BOOL         bWait
	/// );
	/// </summary>
	[DllModuleExport(155)]
	private uint GetOverlappedResult(uint hFile, uint lpOverlapped, uint lpNumberOfBytesTransferred, uint bWait)
	{
		_logger.LogInformation("[Kernel32] GetOverlappedResult(hFile=0x{HFile:X8}, lpOverlapped=0x{LpOverlapped:X8}, lpNumberOfBytesTransferred=0x{LpNumberOfBytesTransferred:X8}, bWait={BWait})",
			hFile, lpOverlapped, lpNumberOfBytesTransferred, bWait);

		// In this emulator, we don't actually support asynchronous I/O
		// We perform all I/O synchronously, so we simulate success
		// The OVERLAPPED structure contains the result at Internal and InternalHigh fields
		
		if (lpOverlapped == 0)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		// Read the number of bytes transferred from OVERLAPPED.InternalHigh
		var internalHighOffset = (uint)Marshal.OffsetOf<NativeTypes.OVERLAPPED>(nameof(NativeTypes.OVERLAPPED.InternalHigh));
		var bytesTransferred = _env.MemRead32(lpOverlapped + internalHighOffset);
		
		if (lpNumberOfBytesTransferred != 0)
		{
			_env.MemWrite32(lpNumberOfBytesTransferred, bytesTransferred);
		}

		// Check if the operation succeeded (Internal field contains status)
		var internalOffset = (uint)Marshal.OffsetOf<NativeTypes.OVERLAPPED>(nameof(NativeTypes.OVERLAPPED.Internal));
		var status = _env.MemRead32(lpOverlapped + internalOffset);
		if (status == 0) // STATUS_SUCCESS
		{
			return (uint)NativeTypes.Win32Bool.TRUE;
		}

		// Operation failed or is pending
		_lastError = (uint)NativeTypes.Win32Error.ERROR_IO_INCOMPLETE;
		return (uint)NativeTypes.Win32Bool.FALSE;
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
				0 => SeekOrigin.Begin,
				1 => SeekOrigin.Current,
				2 => SeekOrigin.End,
				_ => SeekOrigin.Begin
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
				0 => SeekOrigin.Begin,
				1 => SeekOrigin.Current,
				2 => SeekOrigin.End,
				_ => SeekOrigin.Begin
			};
			long dist = (int)lDistanceToMove; // ignore high for now
			var pos = fs.Seek(dist, origin);
			return (uint)pos;
		}

		_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
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

		_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
		return (uint)NativeTypes.Win32Bool.FALSE;
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

		_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
		return (uint)NativeTypes.Win32Bool.FALSE;
	}

	[DllModuleExport(1)]
	private uint DeleteFileA(uint lpFileName)
	{
		try
		{
			var path = _env.ReadAnsiString(lpFileName);
			if (string.IsNullOrEmpty(path))
			{
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return (uint)NativeTypes.Win32Bool.FALSE;
			}

			// VFS is required for file operations
			if (_env.VirtualFileSystem == null)
			{
				_logger.LogError("[Kernel32] DeleteFileA: VFS not initialized");
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
				return (uint)NativeTypes.Win32Bool.FALSE;
			}

			var success = _env.VirtualFileSystem.DeleteFile(path);
			if (success)
			{
				_logger.LogInformation("[Kernel32] DeleteFileA (VFS): Deleted '{Path}'", path);
				return (uint)NativeTypes.Win32Bool.TRUE;
			}

			_logger.LogInformation("[Kernel32] DeleteFileA (VFS) failed: '{Path}'", path);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}
		catch (Exception ex)
		{
			_logger.LogInformation(ex, "[Kernel32] DeleteFileA failed: {ExMessage}", ex.Message);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return (uint)NativeTypes.Win32Bool.FALSE;
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
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return (uint)NativeTypes.Win32Bool.FALSE;
			}

			// VFS is required for file operations
			if (_env.VirtualFileSystem == null)
			{
				_logger.LogError("[Kernel32] MoveFileA: VFS not initialized");
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
				return (uint)NativeTypes.Win32Bool.FALSE;
			}

			var success = _env.VirtualFileSystem.MoveFile(existingPath, newPath);
			if (success)
			{
				_logger.LogInformation("[Kernel32] MoveFileA (VFS): Moved '{ExistingPath}' to '{NewPath}'",
					existingPath, newPath);
				return (uint)NativeTypes.Win32Bool.TRUE;
			}

			_logger.LogInformation("[Kernel32] MoveFileA (VFS) failed: '{ExistingPath}' to '{NewPath}'",
				existingPath, newPath);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}
		catch (Exception ex)
		{
			_logger.LogInformation(ex, "[Kernel32] MoveFileA failed: {ExMessage}", ex.Message);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}
	}

	[DllModuleExport(12)]
	private uint CopyFileA(uint lpExistingFileName, uint lpNewFileName, uint bFailIfExists)
	{
		try
		{
			var existingPath = _env.ReadAnsiString(lpExistingFileName);
			var newPath = _env.ReadAnsiString(lpNewFileName);
			var failIfExists = bFailIfExists != 0;

			if (string.IsNullOrEmpty(existingPath) || string.IsNullOrEmpty(newPath))
			{
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return (uint)NativeTypes.Win32Bool.FALSE;
			}

			// VFS is required for file operations
			if (_env.VirtualFileSystem == null)
			{
				_logger.LogError("[Kernel32] CopyFileA: VFS not initialized");
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
				return (uint)NativeTypes.Win32Bool.FALSE;
			}

			// Perform the copy using VFS - open source, create destination, copy data
			using var sourceHandle = _env.VirtualFileSystem.OpenFile(existingPath, VfsFileMode.Open, VfsFileAccess.Read);
			if (sourceHandle == null)
			{
				_logger.LogError("[Kernel32] CopyFileA: Failed to open source file '{ExistingPath}'", existingPath);
				_lastError = (uint)NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
				return (uint)NativeTypes.Win32Bool.FALSE;
			}

			// Open destination file - VfsFileMode.CreateNew will fail if file exists
			using var destHandle = _env.VirtualFileSystem.OpenFile(newPath, failIfExists ? VfsFileMode.CreateNew : VfsFileMode.Create, VfsFileAccess.Write);
			if (destHandle == null)
			{
				// If failIfExists is true and file exists, CreateNew would have failed
				if (failIfExists)
				{
					_logger.LogInformation("[Kernel32] CopyFileA: Destination '{NewPath}' already exists", newPath);
					_lastError = (uint)NativeTypes.Win32Error.ERROR_FILE_EXISTS;
				}
				else
				{
					_logger.LogError("[Kernel32] CopyFileA: Failed to create destination file '{NewPath}'", newPath);
					_lastError = (uint)NativeTypes.Win32Error.ERROR_ACCESS_DENIED;
				}
				return (uint)NativeTypes.Win32Bool.FALSE;
			}

			// Copy data in chunks
			var buffer = new byte[FILE_COPY_BUFFER_SIZE];
			int bytesRead;
			while ((bytesRead = sourceHandle.Read(buffer, 0, buffer.Length)) > 0)
			{
				destHandle.Write(buffer, 0, bytesRead);
			}

			_logger.LogInformation("[Kernel32] CopyFileA: Copied '{ExistingPath}' to '{NewPath}'", existingPath, newPath);
			return (uint)NativeTypes.Win32Bool.TRUE;
		}
		catch (FileNotFoundException ex)
		{
			_logger.LogInformation(ex, "[Kernel32] CopyFileA failed: {ExMessage}", ex.Message);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}
		catch (UnauthorizedAccessException ex)
		{
			_logger.LogInformation(ex, "[Kernel32] CopyFileA failed: {ExMessage}", ex.Message);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_ACCESS_DENIED;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}
		catch (IOException ex)
		{
			_logger.LogInformation(ex, "[Kernel32] CopyFileA failed: {ExMessage}", ex.Message);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_ACCESS_DENIED;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}
		catch (Exception ex)
		{
			_logger.LogInformation(ex, "[Kernel32] CopyFileA failed: {ExMessage}", ex.Message);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
			return (uint)NativeTypes.Win32Bool.FALSE;
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
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return (uint)NativeTypes.Win32Handle.INVALID_HANDLE_VALUE;
			}

			// Get directory and pattern
			var dir = Path.GetDirectoryName(searchPattern) ?? ".";
			var pattern = Path.GetFileName(searchPattern);

			if (string.IsNullOrEmpty(pattern))
			{
				pattern = "*";
			}

			// VFS is required for file operations
			if (_env.VirtualFileSystem == null)
			{
				_logger.LogError("[Kernel32] FindFirstFileA: VFS not initialized");
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
				return (uint)NativeTypes.Win32Handle.INVALID_HANDLE_VALUE;
			}

			string[] files = _env.VirtualFileSystem.GetFiles(dir, pattern);

			if (files.Length == 0)
			{
				_lastError = (uint)NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
				return (uint)NativeTypes.Win32Handle.INVALID_HANDLE_VALUE;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return (uint)NativeTypes.Win32Handle.INVALID_HANDLE_VALUE;
		}
	}

	[DllModuleExport(1)]
	private uint FindNextFileA(uint hFindFile, uint lpFindFileData)
	{
		try
		{
			if (!_findFileHandles.TryGetValue(hFindFile, out var handle))
			{
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
				return (uint)NativeTypes.Win32Bool.FALSE;
			}

			if (handle.CurrentIndex >= handle.Files.Length)
			{
				_lastError = (uint)NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
				return (uint)NativeTypes.Win32Bool.FALSE;
			}

			// Write next file data
			var fileName = Path.GetFileName(handle.Files[handle.CurrentIndex]);
			WriteFindData(lpFindFileData, fileName);

			_logger.LogInformation("[Kernel32] FindNextFileA: Found '{FileName}'", fileName);
			handle.CurrentIndex++;

			return (uint)NativeTypes.Win32Bool.TRUE;
		}
		catch (Exception ex)
		{
			_logger.LogInformation("[Kernel32] FindNextFileA failed: {ExMessage}", ex.Message);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}
	}

	[DllModuleExport(1)]
	private unsafe uint FindClose(void* hFindFile)
	{
		var handle = (uint)hFindFile;
		if (_findFileHandles.Remove(handle))
		{
			_logger.LogInformation("[Kernel32] FindClose: Closed handle 0x{Handle:X8}", handle);
			return (uint)NativeTypes.Win32Bool.TRUE;
		}

		_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
		return (uint)NativeTypes.Win32Bool.FALSE;
	}

	[DllModuleExport(1)]
	private uint FileTimeToSystemTime(uint lpFileTime, uint lpSystemTime)
	{
		try
		{
			// FileTime is a 64-bit value representing the number of 100-nanosecond intervals since Jan 1, 1601
			// SystemTime is a SYSTEMTIME structure (16 bytes)

			// Read FILETIME structure
			var fileTime = new FileTimeRef(_env.Memory, lpFileTime);
			var fileTimeValue = ((ulong)fileTime.dwHighDateTime << 32) | fileTime.dwLowDateTime;
			var dateTime = DateTime.FromFileTimeUtc((long)fileTimeValue);

			// Write SYSTEMTIME structure
			var systemTime = new SystemTimeRef(_env.Memory, lpSystemTime);
			systemTime.wYear = (ushort)dateTime.Year;
			systemTime.wMonth = (ushort)dateTime.Month;
			systemTime.wDayOfWeek = (ushort)dateTime.DayOfWeek;
			systemTime.wDay = (ushort)dateTime.Day;
			systemTime.wHour = (ushort)dateTime.Hour;
			systemTime.wMinute = (ushort)dateTime.Minute;
			systemTime.wSecond = (ushort)dateTime.Second;
			systemTime.wMilliseconds = (ushort)dateTime.Millisecond;

			return (uint)NativeTypes.Win32Bool.TRUE;
		}
		catch (Exception ex)
		{
			_logger.LogInformation("[Kernel32] FileTimeToSystemTime failed: {ExMessage}", ex.Message);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}
	}

	[DllModuleExport(1)]
	private uint FileTimeToLocalFileTime(uint lpFileTime, uint lpLocalFileTime)
	{
		try
		{
			// Convert UTC file time to local file time
			var fileTime = new FileTimeRef(_env.Memory, lpFileTime);
			var fileTimeValue = ((ulong)fileTime.dwHighDateTime << 32) | fileTime.dwLowDateTime;
			var dateTime = DateTime.FromFileTimeUtc((long)fileTimeValue);
			var localTime = dateTime.ToLocalTime();
			// Use ToFileTime() (not ToFileTimeUtc()) to get the local file time
			var localFileTimeValue = (ulong)localTime.ToFileTime();

			var localFileTime = new FileTimeRef(_env.Memory, lpLocalFileTime);
			localFileTime.dwLowDateTime = (uint)(localFileTimeValue & 0xFFFFFFFF);
			localFileTime.dwHighDateTime = (uint)(localFileTimeValue >> 32);

			return (uint)NativeTypes.Win32Bool.TRUE;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] FileTimeToLocalFileTime failed: {ExMessage}", ex.Message);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0xFFFFFFFF; // TIME_ZONE_ID_INVALID
		}
	}

	[DllModuleExport(449, entryPoint: 0x00011630, Version = "5.1.2600.6532")]
	[DllModuleExport(348, entryPoint: 0x0001A33E, Version = "4.90.0.3000")]
	private uint GetSystemTime(uint lpSystemTime)
	{
		try
		{
			var now = DateTime.UtcNow;

			// Write SYSTEMTIME structure (16 bytes)
			// typedef struct _SYSTEMTIME {
			//   WORD wYear;
			//   WORD wMonth;
			//   WORD wDayOfWeek;
			//   WORD wDay;
			//   WORD wHour;
			//   WORD wMinute;
			//   WORD wSecond;
			//   WORD wMilliseconds;
			// } SYSTEMTIME;

			_env.MemWrite16(lpSystemTime, (ushort)now.Year);
			_env.MemWrite16(lpSystemTime + 2, (ushort)now.Month);
			_env.MemWrite16(lpSystemTime + 4, (ushort)now.DayOfWeek);
			_env.MemWrite16(lpSystemTime + 6, (ushort)now.Day);
			_env.MemWrite16(lpSystemTime + 8, (ushort)now.Hour);
			_env.MemWrite16(lpSystemTime + 10, (ushort)now.Minute);
			_env.MemWrite16(lpSystemTime + 12, (ushort)now.Second);
			_env.MemWrite16(lpSystemTime + 14, (ushort)now.Millisecond);

			_logger.LogInformation("[Kernel32] GetSystemTime: {Year}-{Month:D2}-{Day:D2} {Hour:D2}:{Minute:D2}:{Second:D2}.{Millisecond:D3}",
				now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond);

			return 0; // GetSystemTime returns void in the real API
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] GetSystemTime failed");
			return 0;
		}
	}

	[DllModuleExport(423, entryPoint: 0x0001157C, Version = "5.1.2600.6532")]
	[DllModuleExport(322, entryPoint: 0x0001A2E4, Version = "4.90.0.3000")]
	private uint GetLocalTime(uint lpSystemTime)
	{
		try
		{
			var now = DateTime.Now; // Local time

			// Write SYSTEMTIME structure (16 bytes)
			_env.MemWrite16(lpSystemTime, (ushort)now.Year);
			_env.MemWrite16(lpSystemTime + 2, (ushort)now.Month);
			_env.MemWrite16(lpSystemTime + 4, (ushort)now.DayOfWeek);
			_env.MemWrite16(lpSystemTime + 6, (ushort)now.Day);
			_env.MemWrite16(lpSystemTime + 8, (ushort)now.Hour);
			_env.MemWrite16(lpSystemTime + 10, (ushort)now.Minute);
			_env.MemWrite16(lpSystemTime + 12, (ushort)now.Second);
			_env.MemWrite16(lpSystemTime + 14, (ushort)now.Millisecond);

			_logger.LogInformation("[Kernel32] GetLocalTime: {Year}-{Month:D2}-{Day:D2} {Hour:D2}:{Minute:D2}:{Second:D2}.{Millisecond:D3}",
				now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond);

			return 0; // GetLocalTime returns void in the real API
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] GetLocalTime failed");
			return 0;
		}
	}

	[DllModuleExport(0)]
	private uint GetSystemTimeAsFileTime(uint lpSystemTimeAsFileTime)
	{
		try
		{
			// FILETIME is a 64-bit value representing the number of 100-nanosecond intervals since January 1, 1601 UTC
			var now = DateTime.UtcNow;
			var epoch = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var ticks = (now - epoch).Ticks; // .NET ticks are 100-nanosecond intervals

			// Write FILETIME structure (8 bytes: two 32-bit DWORDs)
			_env.MemWrite32(lpSystemTimeAsFileTime, (uint)(ticks & 0xFFFFFFFF)); // dwLowDateTime
			_env.MemWrite32(lpSystemTimeAsFileTime + 4, (uint)(ticks >> 32)); // dwHighDateTime

			_logger.LogInformation("[Kernel32] GetSystemTimeAsFileTime: {Ticks}", ticks);

			return 0; // GetSystemTimeAsFileTime returns void in the real API
		}
		catch (ArgumentOutOfRangeException ex)
		{
			_logger.LogError(ex, "[Kernel32] GetSystemTimeAsFileTime failed: Invalid time range");
			return 0;
		}
		catch (OverflowException ex)
		{
			_logger.LogError(ex, "[Kernel32] GetSystemTimeAsFileTime failed: Arithmetic overflow");
			return 0;
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
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return (uint)NativeTypes.Win32Bool.FALSE;
			}

			// If lpValue is NULL, delete the variable
			if (lpValue == 0)
			{
				_env.SetEnvironmentVariable(name, null);
				_logger.LogInformation("[Kernel32] SetEnvironmentVariableA: Deleted '{Name}'", name);
			}
			else
			{
				var value = _env.ReadAnsiString(lpValue);
				_env.SetEnvironmentVariable(name, value);
				_logger.LogInformation("[Kernel32] SetEnvironmentVariableA: Set '{Name}'='{Value}'", name, value);
			}

			return (uint)NativeTypes.Win32Bool.TRUE;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] SetEnvironmentVariableA failed: {ExMessage}", ex.Message);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
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
				var exceptionPointers = new ExceptionPointersRef(_env.Memory, exceptionInfo);
				var exceptionRecordPtr = exceptionPointers.ExceptionRecord;
				var contextRecordPtr = exceptionPointers.ContextRecord;

				_logger.LogInformation("[Kernel32]   ExceptionRecord: 0x{ExceptionRecordPtr:X8}", exceptionRecordPtr);
				_logger.LogInformation("[Kernel32]   ContextRecord: 0x{ContextRecordPtr:X8}", contextRecordPtr);

				// If we have a valid exception record, read some basic info
				if (exceptionRecordPtr != 0)
				{
					var exceptionRecord = new ExceptionRecordRef(_env.Memory, exceptionRecordPtr);
					var exceptionCode = exceptionRecord.ExceptionCode;
					var exceptionFlags = exceptionRecord.ExceptionFlags;
					var exceptionAddress = exceptionRecord.ExceptionAddress;

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
		return (uint)NativeTypes.ExceptionHandling.EXCEPTION_EXECUTE_HANDLER;
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
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
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

			// Workaround for C runtime issue: if the wide string ends with a null terminator and was passed
			// with an explicit count (from LCMapStringW return value), the caller might not account for
			// multi-byte expansion. Try stripping trailing nulls to see if conversion fits.
			bool hasTrailingNull = false;
			if (cchWideChar != 0xFFFFFFFF && wideString.Length > 0 && wideString[wideString.Length - 1] == '\0')
			{
				hasTrailingNull = true;
				_logger.LogDebug("[Kernel32] WideCharToMultiByte: Detected trailing null in wide string");
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
							_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
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
				// Workaround: If the wide string had a trailing null and the buffer is too small,
				// try converting without the trailing null to work around C runtime buffer size miscalculation
				if (hasTrailingNull && wideString.Length > 1)
				{
					_logger.LogInformation("[Kernel32] WideCharToMultiByte: Buffer too small with trailing null - trying without null terminator");

					// Remove trailing null and try again
					var trimmedWideString = wideString.Substring(0, wideString.Length - 1);
					byte[] trimmedBytes;

					switch (actualCodePage)
					{
						case CodePage.WestEurope:
						case CodePage.Iso88591LatinI:
							trimmedBytes = Encoding.Latin1.GetBytes(trimmedWideString);
							break;
						case CodePage.Oem437:
						case CodePage.OemMultilingualLatinI:
						case CodePage.EastEurope:
						case CodePage.Russian:
						case CodePage.Utf8:
							trimmedBytes = Encoding.UTF8.GetBytes(trimmedWideString);
							break;
						default:
							try
							{
								var encoding = Encoding.GetEncoding((int)actualCodePage);
								trimmedBytes = encoding.GetBytes(trimmedWideString);
							}
							catch (ArgumentException ex)
							{
								_logger.LogError(ex, "[Kernel32] WideCharToMultiByte: Invalid code page {CodePage} - buffer too small (need {NeedSize} bytes, have {CbMultiByte})", actualCodePage, multiByteBytes.Length, cbMultiByte);
								_lastError = (uint)NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
								return 0;
							}
							catch (NotSupportedException ex)
							{
								_logger.LogError(ex, "[Kernel32] WideCharToMultiByte: Unsupported code page {CodePage} - buffer too small (need {NeedSize} bytes, have {CbMultiByte})", actualCodePage, multiByteBytes.Length, cbMultiByte);
								_lastError = (uint)NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
								return 0;
							}
							break;
					}

					// Check if trimmed version fits (with space for null terminator if needed)
					if (trimmedBytes.Length < cbMultiByte)
					{
						_logger.LogInformation("[Kernel32] WideCharToMultiByte: Trimmed version fits - {TrimmedSize} bytes + null < {CbMultiByte} buffer",
							trimmedBytes.Length, cbMultiByte);

						// Write trimmed bytes
						if (lpMultiByteStr != 0)
						{
							_env.MemWriteBytes(lpMultiByteStr, trimmedBytes);
							// Add null terminator
							_env.MemWrite8(lpMultiByteStr + (uint)trimmedBytes.Length, 0);
						}

						// Clear the "used default char" flag if provided
						if (lpUsedDefaultChar != 0)
						{
							_env.MemWrite32(lpUsedDefaultChar, 0);
						}

						_logger.LogInformation("[Kernel32] WideCharToMultiByte: Success with workaround, returning {BytesLength} bytes", (uint)trimmedBytes.Length);
						return (uint)trimmedBytes.Length;
					}
				}

				_logger.LogInformation("[Kernel32] WideCharToMultiByte: Buffer too small - need {NeedSize} bytes but only have {CbMultiByte}", multiByteBytes.Length, cbMultiByte);
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
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
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return 0;
			}

			// Validate code page
			if (codePage != 0 && codePage != (CodePage)1 && codePage != (CodePage)1252 && codePage != (CodePage)437 && codePage != (CodePage)65001)
			{
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
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
						_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
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
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
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
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
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
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
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
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
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
						_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
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
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
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
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
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

			if (result < 0)
			{
				return cstrLessThan;
			}

			if (result > 0)
			{
				return cstrGreaterThan;
			}

			return cstrEqual;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] CompareStringA failed: {ExMessage}", ex.Message);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
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
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
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
					if (ch == 0)
					{
						break;
					}

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
					if (ch == 0)
					{
						break;
					}

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

			if (result < 0)
			{
				return cstrLessThan;
			}

			if (result > 0)
			{
				return cstrGreaterThan;
			}

			return cstrEqual;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] CompareStringW failed: {ExMessage}", ex.Message);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		try
		{
			// Use .NET's Stopwatch.GetTimestamp() which provides high-resolution timestamp
			var timestamp = Stopwatch.GetTimestamp();

			// Write the 64-bit timestamp to the provided memory location
			_env.MemWrite64(lpPerformanceCount, (ulong)timestamp);

			return (uint)NativeTypes.Win32Bool.TRUE;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] QueryPerformanceCounter failed");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		try
		{
			// Stopwatch.Frequency provides the frequency of the high-resolution timer
			var frequency = Stopwatch.Frequency;

			// Write the 64-bit frequency to the provided memory location
			_env.MemWrite64(lpFrequency, (ulong)frequency);

			_logger.LogInformation("[Kernel32] QueryPerformanceFrequency: {Frequency} Hz", frequency);
			return (uint)NativeTypes.Win32Bool.TRUE;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] QueryPerformanceFrequency failed");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
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

	/// <summary>
	/// Suspends the current thread until the specified condition is met.
	/// DWORD SleepEx(
	///   [in] DWORD dwMilliseconds,
	///   [in] BOOL  bAlertable
	/// );
	/// </summary>
	[DllModuleExport(692, IsStub = true)]
	private uint SleepEx(uint dwMilliseconds, uint bAlertable)
	{
		_logger.LogInformation("[Kernel32] SleepEx(dwMilliseconds={DwMilliseconds}, bAlertable={BAlertable})",
			dwMilliseconds, bAlertable);

		// For now, just call Sleep and ignore alertable parameter
		// In a full implementation, alertable would allow APCs (Asynchronous Procedure Calls) to interrupt
		Sleep(dwMilliseconds);

		// Return WAIT_IO_COMPLETION (0) indicating it completed without being interrupted by an APC
		return WAIT_IO_COMPLETION;
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

	/// <summary>
	/// Retrieves a pseudo handle for the current thread.
	/// </summary>
	/// <returns>
	/// The return value is a pseudo handle for the current thread.
	/// A pseudo handle is a special constant that is interpreted as the current thread handle.
	/// The calling thread can use this handle to specify itself whenever a thread handle is required.
	/// Pseudo handles are not inherited by child processes.
	/// This handle has the THREAD_ALL_ACCESS access right to the thread object.
	/// </returns>
	/// <remarks>
	/// A pseudo handle is a special constant, currently (HANDLE)-2, that is interpreted as the current thread handle.
	/// For compatibility with future operating systems, it is best to call GetCurrentThread instead of hard-coding this value.
	/// The function cannot be used by one thread to create a handle that can be used by other threads to refer to the first thread.
	/// The handle is always interpreted as referring to the thread that is using it.
	/// A thread can create a "real" handle to itself that can be used by other threads, or inherited by other processes, by specifying the pseudo handle as the source handle in a call to the DuplicateHandle function.
	/// The pseudo handle need not be closed when it is no longer needed. Calling the CloseHandle function with a pseudo handle has no effect.
	/// If the pseudo handle is duplicated by DuplicateHandle, the duplicate handle must be closed.
	/// </remarks>
	private uint GetCurrentThread()
	{
		// Return pseudo-handle for current thread
		// This is a special constant that Windows interprets as "current thread"
		const uint CURRENT_THREAD_PSEUDO_HANDLE = 0xFFFFFFFE; // -2 as unsigned

		_logger.LogInformation("[Kernel32] GetCurrentThread() = 0xFFFFFFFE (pseudo-handle)");
		return CURRENT_THREAD_PSEUDO_HANDLE;
	}

	/// <summary>
	/// Determines whether the specified process is running under WOW64 (Windows 32-bit on Windows 64-bit).
	/// BOOL IsWow64Process(
	///   [in]  HANDLE hProcess,
	///   [out] PBOOL  Wow64Process
	/// );
	/// </summary>
	/// <param name="hProcess">
	/// A handle to the process. The handle must have the PROCESS_QUERY_INFORMATION or PROCESS_QUERY_LIMITED_INFORMATION access right.
	/// </param>
	/// <param name="lpWow64Process">
	/// A pointer to a value that is set to TRUE if the process is running under WOW64. 
	/// If the process is running under 32-bit Windows, the value is set to FALSE.
	/// If the process is a 64-bit application running under 64-bit Windows, the value is also set to FALSE.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is nonzero (TRUE).
	/// If the function fails, the return value is zero (FALSE). To get extended error information, call GetLastError.
	/// </returns>
	/// <remarks>
	/// WOW64 is the x86 emulator that allows 32-bit Windows-based applications to run seamlessly on 64-bit Windows.
	/// Since this emulator is always running in 32-bit mode (emulating x86 architecture), processes are never running under WOW64.
	/// Therefore, this function always sets Wow64Process to FALSE.
	/// </remarks>
	[DllModuleExport(0)]
	private uint IsWow64Process(uint hProcess, uint lpWow64Process)
	{
		_logger.LogInformation("[Kernel32] IsWow64Process(hProcess=0x{HProcess:X8}, lpWow64Process=0x{LpWow64Process:X8})",
			hProcess, lpWow64Process);

		// Validate the process handle
		// Accept pseudo-handle for current process (0xFFFFFFFF) and any valid handle
		if (hProcess == 0)
		{
		    _logger.LogWarning("[Kernel32] IsWow64Process: Invalid parameter - hProcess is NULL");
		    _lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
		    return (uint)NativeTypes.Win32Bool.FALSE;
		}

		// Validate the output pointer
		if (lpWow64Process == 0)
		{
		    _logger.LogWarning("[Kernel32] IsWow64Process: Invalid parameter - lpWow64Process is NULL");
		    _lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
		    return (uint)NativeTypes.Win32Bool.FALSE;
		}

		// Since this emulator always runs in 32-bit mode and emulates x86 architecture,
		// the emulated process is never running under WOW64 (which is 32-bit on 64-bit Windows).
		// From the perspective of the emulated process, it's running on a native 32-bit system.
		_env.MemWrite32(lpWow64Process, (uint)NativeTypes.Win32Bool.FALSE);

		_logger.LogInformation("[Kernel32] IsWow64Process: Returning FALSE (not running under WOW64)");
		return (uint)NativeTypes.Win32Bool.TRUE; // Function succeeded
	}

	/// <summary>
	/// Retrieves the process affinity mask for the specified process and the system affinity mask for the system.
	/// </summary>
	/// <param name="hProcess">
	/// A handle to the process whose affinity mask is desired.
	/// This handle must have the PROCESS_QUERY_INFORMATION or PROCESS_QUERY_LIMITED_INFORMATION access right.
	/// </param>
	/// <param name="lpProcessAffinityMask">
	/// A pointer to a variable that receives the affinity mask for the specified process.
	/// </param>
	/// <param name="lpSystemAffinityMask">
	/// A pointer to a variable that receives the affinity mask for the system.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is nonzero.
	/// If the function fails, the return value is zero. To get extended error information, call GetLastError.
	/// </returns>
	/// <remarks>
	/// The process affinity mask is a bit mask in which each bit represents the processors that a process is allowed to run on.
	/// The system affinity mask is a bit mask in which each bit represents the processors that are configured in the system.
	/// The process affinity mask is a subset of the system affinity mask.
	/// A process is only allowed to run on the processors configured in the system.
	/// Therefore, the process affinity mask cannot specify a 1 bit for a processor when the system affinity mask specifies a 0 bit for that processor.
	/// </remarks>
	[DllModuleExport(0)] // Placeholder ordinal - will be updated with actual ordinal from PE exports
	private uint GetProcessAffinityMask(uint hProcess, uint lpProcessAffinityMask, uint lpSystemAffinityMask)
	{
		_logger.LogInformation("[Kernel32] GetProcessAffinityMask(hProcess=0x{HProcess:X8}, lpProcessAffinityMask=0x{LpProcessAffinityMask:X8}, lpSystemAffinityMask=0x{LpSystemAffinityMask:X8})",
			hProcess, lpProcessAffinityMask, lpSystemAffinityMask);

		if (lpProcessAffinityMask == 0 || lpSystemAffinityMask == 0)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		// For our emulator, we'll report a single processor system
		// Bit 0 set means processor 0 is available
		const uint SINGLE_PROCESSOR_MASK = 0x00000001;

		// Write the affinity masks to memory
		_env.MemWrite32(lpProcessAffinityMask, SINGLE_PROCESSOR_MASK);
		_env.MemWrite32(lpSystemAffinityMask, SINGLE_PROCESSOR_MASK);

		_logger.LogDebug("[Kernel32] GetProcessAffinityMask: ProcessMask=0x{ProcessMask:X8}, SystemMask=0x{SystemMask:X8}",
			SINGLE_PROCESSOR_MASK, SINGLE_PROCESSOR_MASK);

		return (uint)NativeTypes.Win32Bool.TRUE;
	}

	/// <summary>
	/// Retrieves information about the current system.
	/// </summary>
	/// <param name="lpSystemInfo">
	/// A pointer to a SYSTEM_INFO structure that receives the information.
	/// </param>
	/// <returns>
	/// This function does not return a value.
	/// </returns>
	/// <remarks>
	/// To retrieve accurate information for an application running on WOW64, call the GetNativeSystemInfo function.
	/// </remarks>
	[DllModuleExport(0)] // Placeholder ordinal - will be updated with actual ordinal from PE exports
	private uint GetSystemInfo(uint lpSystemInfo)
	{
		_logger.LogInformation("[Kernel32] GetSystemInfo(lpSystemInfo=0x{LpSystemInfo:X8})", lpSystemInfo);

		if (lpSystemInfo == 0)
		{
			_logger.LogWarning("[Kernel32] GetSystemInfo: null pointer");
			return 0;
		}

		// Fill in SYSTEM_INFO structure with emulated system information
		// We're emulating a single-processor Intel Pentium system running Windows XP
		var sysInfo = new NativeTypes.SystemInfo
		{
			ProcessorArchitecture = 0,      // PROCESSOR_ARCHITECTURE_INTEL (x86)
			Reserved = 0,
			PageSize = 4096,                // 4KB pages (standard for x86)
			MinimumApplicationAddress = 0x00010000, // 64KB - standard Windows minimum
			MaximumApplicationAddress = 0x7FFEFFFF, // 2GB - 64KB (standard user-mode limit)
			ActiveProcessorMask = 0x00000001,       // Processor 0 active (single CPU)
			NumberOfProcessors = 1,                 // Single processor
			ProcessorType = 586,                    // PROCESSOR_INTEL_PENTIUM (586)
			AllocationGranularity = 65536,          // 64KB allocation granularity
			ProcessorLevel = 5,                     // Pentium (family 5)
			ProcessorRevision = 0x0101              // Model 1, Stepping 1
		};

		// Write the structure to memory
		_env.MemWriteStruct(lpSystemInfo, ref sysInfo);

		_logger.LogDebug("[Kernel32] GetSystemInfo: Arch={Arch}, Processors={Procs}, PageSize={PageSize}",
			sysInfo.ProcessorArchitecture, sysInfo.NumberOfProcessors, sysInfo.PageSize);

		// Note: The Windows API GetSystemInfo function returns void, but for consistency with the emulator's calling convention (which expects all API stubs to return a uint), we return 0 here.
		return 0;
	}

	/// <summary>
	/// Sets a processor affinity mask for the specified thread.
	/// </summary>
	/// <param name="hThread">
	/// A handle to the thread whose affinity mask is to be set.
	/// This handle must have the THREAD_SET_INFORMATION or THREAD_SET_LIMITED_INFORMATION access right
	/// and the THREAD_QUERY_INFORMATION or THREAD_QUERY_LIMITED_INFORMATION access right.
	/// </param>
	/// <param name="dwThreadAffinityMask">
	/// The affinity mask for the thread.
	/// </param>
	/// <returns>
	/// If the function succeeds, the return value is the thread's previous affinity mask.
	/// If the function fails, the return value is zero. To get extended error information, call GetLastError.
	/// </returns>
	/// <remarks>
	/// A thread affinity mask is a bit vector in which each bit represents a logical processor on which the thread is allowed to run.
	/// A thread affinity mask must be a subset of the process affinity mask for the containing process of a thread.
	/// A thread can only run on the processors its process can run on. Therefore, the thread affinity mask cannot specify a 1 bit for a processor when the process affinity mask specifies a 0 bit for that processor.
	/// Setting an affinity mask for a process or thread can result in threads receiving less processor time, as the system is restricted from running the threads on certain processors.
	/// In most cases, it is better to let the system select an available processor.
	/// </remarks>
	[DllModuleExport(0)] // Placeholder ordinal - will be updated with actual ordinal from PE exports
	private uint SetThreadAffinityMask(uint hThread, uint dwThreadAffinityMask)
	{
		_logger.LogInformation("[Kernel32] SetThreadAffinityMask(hThread=0x{HThread:X8}, dwThreadAffinityMask=0x{DwThreadAffinityMask:X8})",
			hThread, dwThreadAffinityMask);

		// In our single-threaded emulator, we don't actually enforce affinity
		// But we validate the mask and return the previous affinity (which is always 0x1 for processor 0)

		// Validate the affinity mask - must be non-zero
		if (dwThreadAffinityMask == 0)
		{
			_logger.LogWarning("[Kernel32] SetThreadAffinityMask: Invalid affinity mask (zero)");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}

		// Validate the affinity mask - must be a subset of system affinity mask
		const uint SYSTEM_AFFINITY_MASK = 0x00000001; // Single processor
		if ((dwThreadAffinityMask & ~SYSTEM_AFFINITY_MASK) != 0)
		{
			_logger.LogWarning("[Kernel32] SetThreadAffinityMask: Invalid affinity mask 0x{Mask:X8} (not subset of system mask 0x{SystemMask:X8})",
				dwThreadAffinityMask, SYSTEM_AFFINITY_MASK);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}

		// Return the previous affinity mask (always processor 0 in our emulator)
		const uint PREVIOUS_AFFINITY_MASK = 0x00000001;

		_logger.LogDebug("[Kernel32] SetThreadAffinityMask: Success, returning previous mask 0x{PrevMask:X8}",
			PREVIOUS_AFFINITY_MASK);

		return PREVIOUS_AFFINITY_MASK;
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

	[DllModuleExport(8)]
	private uint TerminateThread(uint hThread, uint dwExitCode)
	{
		_logger.LogInformation("[Kernel32] TerminateThread(handle=0x{Handle:X8}, exitCode={ExitCode})", hThread, dwExitCode);

		var thread = _env.ThreadScheduler?.GetThreadByHandle(hThread);
		if (thread != null)
		{
			_env.ThreadScheduler.TerminateThread(thread.ThreadId, dwExitCode);
			_logger.LogInformation("[Kernel32] TerminateThread: thread {ThreadId} terminated with code {ExitCode}",
				thread.ThreadId, dwExitCode);
			return (uint)NativeTypes.Win32Bool.TRUE;
		}

		// Thread not found or scheduler not available
		_logger.LogWarning("[Kernel32] TerminateThread: invalid thread handle 0x{Handle:X8}", hThread);
		_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
		return (uint)NativeTypes.Win32Bool.FALSE;
	}

	[DllModuleExport(8)]
	private uint GetExitCodeThread(uint hThread, uint lpExitCode)
	{
		_logger.LogInformation("[Kernel32] GetExitCodeThread(handle=0x{Handle:X8}, lpExitCode=0x{LpExitCode:X8})",
			hThread, lpExitCode);

		if (lpExitCode == 0)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		var thread = _env.ThreadScheduler?.GetThreadByHandle(hThread);
		if (thread != null)
		{
			// STILL_ACTIVE = 259 (0x103)
			const uint STILL_ACTIVE = 259;
			uint exitCode = thread.State == Threading.ThreadState.Terminated ? thread.ExitCode : STILL_ACTIVE;

			_env.MemWrite32(lpExitCode, exitCode);
			_logger.LogInformation("[Kernel32] GetExitCodeThread: thread {ThreadId} exit code = {ExitCode}",
				thread.ThreadId, exitCode);
			return (uint)NativeTypes.Win32Bool.TRUE;
		}

		// Thread not found - assume it terminated with code 0
		_env.MemWrite32(lpExitCode, 0);
		_logger.LogWarning("[Kernel32] GetExitCodeThread: invalid thread handle 0x{Handle:X8}, returning 0", hThread);
		return (uint)NativeTypes.Win32Bool.TRUE;
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
		return success ? (uint)NativeTypes.Win32Bool.TRUE : (uint)NativeTypes.Win32Bool.FALSE;
	}

	[DllModuleExport(37)]
	private uint TlsFree(uint dwTlsIndex)
	{
		var success = _env.TlsFree(dwTlsIndex);
		_logger.LogInformation("[Kernel32] TlsFree({DwTlsIndex}) = {Success}", dwTlsIndex, success);
		return success ? (uint)NativeTypes.Win32Bool.TRUE : (uint)NativeTypes.Win32Bool.FALSE;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_ALREADY_EXISTS;
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
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		var success = _env.SynchronizationManager.ReleaseMutex(hMutex, currentThreadId);

		if (!success)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_NOT_OWNER;
		}

		// Check if there are waiting threads
		var nextWaiter = _env.SynchronizationManager.GetNextMutexWaiter(hMutex);
		if (nextWaiter.HasValue && _env.ThreadScheduler != null)
		{
			_env.ThreadScheduler.WakeThread(nextWaiter.Value);
		}

		return success ? (uint)NativeTypes.Win32Bool.TRUE : (uint)NativeTypes.Win32Bool.FALSE;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_ALREADY_EXISTS;
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
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		var success = _env.SynchronizationManager.SetEvent(hEvent);

		// Wake all threads waiting on this event
		if (!success || _env.ThreadScheduler == null)
		{
			return success ? (uint)NativeTypes.Win32Bool.TRUE : (uint)NativeTypes.Win32Bool.FALSE;
		}

		var waiters = _env.SynchronizationManager.GetEventWaiters(hEvent);
		foreach (var waiterId in waiters)
		{
			_env.ThreadScheduler.WakeThread(waiterId);
		}

		return success ? (uint)NativeTypes.Win32Bool.TRUE : (uint)NativeTypes.Win32Bool.FALSE;
	}

	[DllModuleExport(37)]
	private uint ResetEvent(uint hEvent)
	{
		_logger.LogInformation("[Kernel32] ResetEvent(handle=0x{Handle:X8})", hEvent);

		if (_env.SynchronizationManager == null)
		{
			_logger.LogWarning("[Kernel32] ResetEvent: SynchronizationManager not available");
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		var success = _env.SynchronizationManager.ResetEvent(hEvent);
		return success ? (uint)NativeTypes.Win32Bool.TRUE : (uint)NativeTypes.Win32Bool.FALSE;
	}

	[DllModuleExport(37)]
	private uint PulseEvent(uint hEvent)
	{
		_logger.LogInformation("[Kernel32] PulseEvent(handle=0x{Handle:X8})", hEvent);

		if (_env.SynchronizationManager == null)
		{
			_logger.LogWarning("[Kernel32] PulseEvent: SynchronizationManager not available");
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		// PulseEvent sets and immediately resets the event
		// Wake threads waiting on this event
		if (_env.ThreadScheduler == null)
		{
			return (uint)NativeTypes.Win32Bool.TRUE;
		}

		var waiters = _env.SynchronizationManager.GetEventWaiters(hEvent);
		foreach (var waiterId in waiters)
		{
			_env.ThreadScheduler.WakeThread(waiterId);
		}

		// The event remains in non-signaled state
		return (uint)NativeTypes.Win32Bool.TRUE;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_ALREADY_EXISTS;
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
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		var success = _env.SynchronizationManager.ReleaseSemaphore(hSemaphore, lReleaseCount, out var previousCount);

		if (lpPreviousCount != 0)
		{
			_env.MemWrite32(lpPreviousCount, previousCount);
		}

		// Wake waiting threads
		if (!success || _env.ThreadScheduler == null)
		{
			return success ? (uint)NativeTypes.Win32Bool.TRUE : (uint)NativeTypes.Win32Bool.FALSE;
		}

		for (var i = 0; i < lReleaseCount; i++)
		{
			var nextWaiter = _env.SynchronizationManager.GetNextSemaphoreWaiter(hSemaphore);
			if (nextWaiter.HasValue)
			{
				_env.ThreadScheduler.WakeThread(nextWaiter.Value);
			}
		}

		return success ? (uint)NativeTypes.Win32Bool.TRUE : (uint)NativeTypes.Win32Bool.FALSE;
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

	/// <summary>
	/// Waits until the specified object is in the signaled state, an I/O completion routine or APC is queued, or the time-out interval elapses.
	/// DWORD WaitForSingleObjectEx(
	///   [in] HANDLE hHandle,
	///   [in] DWORD  dwMilliseconds,
	///   [in] BOOL   bAlertable
	/// );
	/// </summary>
	[DllModuleExport(693, IsStub = true)]
	private uint WaitForSingleObjectEx(uint hHandle, uint dwMilliseconds, uint bAlertable)
	{
		_logger.LogInformation("[Kernel32] WaitForSingleObjectEx(handle=0x{Handle:X8}, timeout={Timeout}ms, alertable={Alertable})",
			hHandle, dwMilliseconds, bAlertable);

		// For now, just call WaitForSingleObject and ignore alertable parameter
		// In a full implementation, alertable would allow APCs (Asynchronous Procedure Calls) to interrupt
		return WaitForSingleObject(hHandle, dwMilliseconds);
	}

	/// <summary>
	/// Waits until one or all of the specified objects are in the signaled state or the time-out interval elapses.
	/// DWORD WaitForMultipleObjects(
	///   [in] DWORD  nCount,
	///   [in] const HANDLE *lpHandles,
	///   [in] BOOL   bWaitAll,
	///   [in] DWORD  dwMilliseconds
	/// );
	/// </summary>
	[DllModuleExport(16)]
	private uint WaitForMultipleObjects(uint nCount, uint lpHandles, uint bWaitAll, uint dwMilliseconds)
	{
		var currentThreadId = _env.GetCurrentThreadId();
		_logger.LogInformation("[Kernel32] WaitForMultipleObjects(count={Count}, handles=0x{Handles:X8}, waitAll={WaitAll}, timeout={Timeout}ms) - Thread {ThreadId}",
			nCount, lpHandles, bWaitAll, dwMilliseconds, currentThreadId);

		const uint WAIT_OBJECT_0 = 0;
		const uint WAIT_TIMEOUT = 0x102;
		const uint WAIT_FAILED = 0xFFFFFFFF;
		const uint MAXIMUM_WAIT_OBJECTS = 64;

		if (_env.SynchronizationManager == null)
		{
			_logger.LogWarning("[Kernel32] WaitForMultipleObjects: SynchronizationManager not available");
			return WAIT_FAILED;
		}

		// Validate parameters
		if (nCount == 0 || nCount > MAXIMUM_WAIT_OBJECTS)
		{
			_logger.LogWarning("[Kernel32] WaitForMultipleObjects: invalid count {Count}", nCount);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return WAIT_FAILED;
		}

		if (lpHandles == 0)
		{
			_logger.LogWarning("[Kernel32] WaitForMultipleObjects: null handle array");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return WAIT_FAILED;
		}

		// Read handle array from memory
		var handles = new uint[nCount];
		for (uint i = 0; i < nCount; i++)
		{
			handles[i] = _env.MemRead32(lpHandles + (i * 4));
		}

		// Validate all handles
		for (uint i = 0; i < nCount; i++)
		{
			var objectType = _env.SynchronizationManager.GetObjectType(handles[i]);
			if (objectType == null)
			{
				_logger.LogWarning("[Kernel32] WaitForMultipleObjects: invalid handle 0x{Handle:X8} at index {Index}", handles[i], i);
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
				return WAIT_FAILED;
			}
		}

		// Start time for timeout tracking
		var startTime = DateTime.UtcNow;
		var timeoutSpan = dwMilliseconds == 0xFFFFFFFF
			? TimeSpan.MaxValue
			: TimeSpan.FromMilliseconds(dwMilliseconds);

		bool waitAll = bWaitAll != 0;

		// Polling loop to wait for objects
		while (true)
		{
			if (waitAll)
			{
				// Wait for ALL objects to be signaled
				bool allSignaled = true;
				var tempStates = new List<(uint handle, string type, bool acquired)>();

				// Check if all objects can be acquired
				for (uint i = 0; i < nCount; i++)
				{
					var handle = handles[i];
					var objectType = _env.SynchronizationManager.GetObjectType(handle);

					var signaled = objectType switch
					{
						"Mutex" => _env.SynchronizationManager.CanAcquireMutex(handle, currentThreadId),
						"Event" => _env.SynchronizationManager.IsEventSignaled(handle),
						"Semaphore" => _env.SynchronizationManager.IsSemaphoreSignaled(handle),
						_ => false
					};

					tempStates.Add((handle, objectType ?? "Unknown", signaled));

					if (!signaled)
					{
						allSignaled = false;
						break;
					}
				}

				if (allSignaled)
				{
					// Acquire all objects
					foreach (var (handle, type, _) in tempStates)
					{
						_ = type switch
						{
							"Mutex" => _env.SynchronizationManager.AcquireMutex(handle, currentThreadId),
							"Event" => _env.SynchronizationManager.WaitOnEvent(handle, currentThreadId),
							"Semaphore" => _env.SynchronizationManager.WaitOnSemaphore(handle, currentThreadId),
							_ => false
						};
					}

					_logger.LogDebug("[Kernel32] WaitForMultipleObjects: Thread {ThreadId} acquired all {Count} objects",
						currentThreadId, nCount);
					return WAIT_OBJECT_0;
				}
			}
			else
			{
				// Wait for ANY object to be signaled
				for (uint i = 0; i < nCount; i++)
				{
					var handle = handles[i];
					var objectType = _env.SynchronizationManager.GetObjectType(handle);

					var signaled = objectType switch
					{
						"Mutex" => _env.SynchronizationManager.AcquireMutex(handle, currentThreadId),
						"Event" => _env.SynchronizationManager.WaitOnEvent(handle, currentThreadId),
						"Semaphore" => _env.SynchronizationManager.WaitOnSemaphore(handle, currentThreadId),
						_ => false
					};

					if (signaled)
					{
						_logger.LogDebug("[Kernel32] WaitForMultipleObjects: Thread {ThreadId} acquired {Type} 0x{Handle:X8} at index {Index}",
							currentThreadId, objectType, handle, i);
						return WAIT_OBJECT_0 + i;
					}
				}
			}

			// Check timeout
			if (dwMilliseconds == 0)
			{
				// Zero timeout - return immediately without waiting
				_logger.LogDebug("[Kernel32] WaitForMultipleObjects: Zero timeout, returning WAIT_TIMEOUT");
				return WAIT_TIMEOUT;
			}

			var elapsed = DateTime.UtcNow - startTime;
			if (elapsed >= timeoutSpan)
			{
				// Timeout expired
				_logger.LogDebug("[Kernel32] WaitForMultipleObjects: Timeout expired after {Elapsed}ms", elapsed.TotalMilliseconds);
				return WAIT_TIMEOUT;
			}

			// Objects not available yet - yield and retry
			Thread.Sleep(1);

			// Process scheduler timeout checks if available
			_env.ThreadScheduler?.ProcessWaitTimeouts();
		}
	}

	// Directory functions
	[DllModuleExport(1)]
	private uint SetCurrentDirectoryA(in LpcStr lpPathName)
	{
		var path = lpPathName.ToString();
		if (string.IsNullOrEmpty(path))
		{
			_logger.LogInformation("[Kernel32] SetCurrentDirectoryA failed: Invalid path (empty or null)");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		_logger.LogInformation("[Kernel32] SetCurrentDirectoryA(\"{Path}\")", path);
		_env.CurrentDirectory = path;
		return (uint)NativeTypes.Win32Bool.TRUE;
	}

	[DllModuleExport(1)]
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
			return requiredLength;
		}

		// Write the current directory to the buffer
		lpBuffer.Write(_env.Memory, currentDir, true);
		return (uint)currentDir.Length; // Return length without null terminator
	}

	/// <summary>
	/// Retrieves the current directory (Unicode version).
	/// </summary>
	[DllModuleExport(1)]
	private uint GetCurrentDirectoryW(uint nBufferLength, in LpWStr lpBuffer)
	{
		var currentDir = _env.CurrentDirectory;
		var requiredLength = (uint)currentDir.Length + 1; // +1 for null terminator

		_logger.LogInformation("[Kernel32] GetCurrentDirectoryW({NBufferLength}, 0x{LpBuffer:X8}) -> \"{CurrentDir}\"", nBufferLength, lpBuffer.Address, currentDir);

		if (nBufferLength == 0)
		{
			// Return required buffer size
			return requiredLength;
		}

		if (nBufferLength < requiredLength)
		{
			// Buffer too small, return required size
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
			return requiredLength;
		}

		// Write the current directory to the buffer (Unicode)
		lpBuffer.Write(_env.Memory, currentDir, true);
		return (uint)currentDir.Length; // Return length without null terminator
	}

	[DllModuleExport(8)]
	private uint CreateDirectoryA(in LpcStr lpPathName, uint lpSecurityAttributes)
	{
		var path = lpPathName.ToString();
		_logger.LogInformation("[Kernel32] CreateDirectoryA(\"{Path}\", 0x{LpSecurityAttributes:X8})", path, lpSecurityAttributes);

		if (string.IsNullOrEmpty(path))
		{
			_logger.LogWarning("[Kernel32] CreateDirectoryA: Invalid path (empty or null)");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		try
		{
			// Create directory - just use the path directly without VFS translation for now
			var realPath = path;

			// Create directory
			if (!Directory.Exists(realPath))
			{
				Directory.CreateDirectory(realPath);
				_logger.LogInformation("[Kernel32] CreateDirectoryA: Created directory \"{RealPath}\"", realPath);
			}
			else
			{
				_logger.LogInformation("[Kernel32] CreateDirectoryA: Directory already exists \"{RealPath}\"", realPath);
				_lastError = (uint)NativeTypes.Win32Error.ERROR_ALREADY_EXISTS;
				return (uint)NativeTypes.Win32Bool.FALSE;
			}

			return (uint)NativeTypes.Win32Bool.TRUE;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] CreateDirectoryA: Failed to create directory \"{Path}\"", path);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_PATH_NOT_FOUND;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}
	}

	[DllModuleExport(8)]
	private uint GetWindowsDirectoryA(in LpStr lpBuffer, uint uSize)
	{
		// Return a typical Windows directory path
		const string windowsDir = "C:\\WINDOWS";
		var requiredSize = (uint)windowsDir.Length + 1; // +1 for null terminator

		_logger.LogInformation("[Kernel32] GetWindowsDirectoryA(buffer=0x{Address:X8}, size={USize})", lpBuffer.Address, uSize);

		if (uSize == 0)
		{
			return requiredSize;
		}

		if (uSize < requiredSize)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
			return requiredSize;
		}

		// Write the Windows directory to the buffer
		lpBuffer.Write(_env.Memory, windowsDir, true);
		return (uint)windowsDir.Length; // Return length without null terminator
	}

	// String functions
	[DllModuleExport(1)]
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

	[DllModuleExport(8)]
	private uint LstrcpyA(in LpStr lpString1, in LpcStr lpString2)
	{
		var str2 = lpString2.ToString();
		_logger.LogInformation("[Kernel32] LstrcpyA(dest=0x{Address:X8}, src=\"{Str2}\")", lpString1.Address, str2);

		// Copy string to destination
		lpString1.Write(_env.Memory, str2 ?? string.Empty, true);

		// Return pointer to destination string
		return lpString1.Address;
	}

	[DllModuleExport(4)]
	private uint LstrlenA(in LpcStr lpString)
	{
		var str = lpString.ToString();
		_logger.LogInformation("[Kernel32] LstrlenA(\"{Str}\")", str);

		// Return the length of the string (excluding null terminator)
		return (uint)(str?.Length ?? 0);
	}

	/// <summary>
	/// Determines the length of the specified wide string (Unicode).
	/// int lstrlenW(LPCWSTR lpString);
	/// </summary>
	[DllModuleExport(4)]
	private uint LstrlenW(uint lpString)
	{
		if (lpString == 0)
		{
			_logger.LogInformation("[Kernel32] LstrlenW(NULL)");
			return 0;
		}

		var wstr = new LpWStr(lpString);
		var str = wstr.Read(_env.Memory);
		_logger.LogInformation("[Kernel32] LstrlenW(\"{Str}\")", str);

		// Return the length of the string (excluding null terminator)
		return (uint)str.Length;
	}

	// Process execution
	[DllModuleExport(1)]
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_PROC_NOT_FOUND;
			return 0;
		}

		try
		{
			var result = _resourceReader.FindResource(lpType, lpName, 0);
			if (result == 0)
			{
				_lastError = (uint)NativeTypes.Win32Error.ERROR_PROC_NOT_FOUND;
			}
			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] FindResourceA: Exception occurred");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_PROC_NOT_FOUND;
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
			_lastError = (uint)NativeTypes.Win32Error.ERROR_PROC_NOT_FOUND;
			return 0;
		}

		try
		{
			var result = _resourceReader.LoadResource(hModule, hResInfo);
			if (result == 0)
			{
				_lastError = (uint)NativeTypes.Win32Error.ERROR_PROC_NOT_FOUND;
			}
			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] LoadResource: Exception occurred");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_PROC_NOT_FOUND;
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

	/// <summary>
	/// Frees a resource loaded by LoadResource.
	/// In Win32, this function is obsolete and does nothing - resources are freed automatically.
	/// BOOL FreeResource(HGLOBAL hResData);
	/// </summary>
	/// <param name="hResData">Handle to the resource data</param>
	/// <returns>Always returns TRUE (non-zero) for compatibility</returns>
	[DllModuleExport(0)]
	private uint FreeResource(uint hResData)
	{
		_logger.LogInformation("[Kernel32] FreeResource: hResData=0x{HResData:X8} (obsolete, no-op)", hResData);
		// In Win32, FreeResource is obsolete and does nothing
		// Resources are freed automatically when the module is unloaded
		return 1; // TRUE
	}

	[DllModuleExport(8)]
	private uint SetFileAttributesA(in LpcStr lpFileName, uint dwFileAttributes)
	{
		var fileName = lpFileName.ToString();
		_logger.LogInformation("[Kernel32] SetFileAttributesA(\"{FileName}\", 0x{DwFileAttributes:X8})", fileName, dwFileAttributes);

		if (string.IsNullOrEmpty(fileName))
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		try
		{
			// Just use the path directly for now
			var realPath = fileName;

			// Set file attributes (we'll support basic ones)
			var fileInfo = new FileInfo(realPath);
			if (!fileInfo.Exists)
			{
				_lastError = (uint)NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
				return (uint)NativeTypes.Win32Bool.FALSE;
			}

			// Map Win32 attributes to .NET FileAttributes
			FileAttributes attributes = FileAttributes.Normal;

			if ((dwFileAttributes & 0x01) != 0)
			{
				attributes |= FileAttributes.ReadOnly;    // FILE_ATTRIBUTE_READONLY
			}

			if ((dwFileAttributes & 0x02) != 0)
			{
				attributes |= FileAttributes.Hidden;      // FILE_ATTRIBUTE_HIDDEN
			}

			if ((dwFileAttributes & 0x04) != 0)
			{
				attributes |= FileAttributes.System;      // FILE_ATTRIBUTE_SYSTEM
			}

			if ((dwFileAttributes & 0x20) != 0)
			{
				attributes |= FileAttributes.Archive;     // FILE_ATTRIBUTE_ARCHIVE
			}

			fileInfo.Attributes = attributes;
			_logger.LogInformation("[Kernel32] SetFileAttributesA: Set attributes for \"{RealPath}\"", realPath);

			return (uint)NativeTypes.Win32Bool.TRUE;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] SetFileAttributesA: Failed to set attributes for \"{FileName}\"", fileName);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_ACCESS_DENIED;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}
	}

	[DllModuleExport(20)]
	private uint GetDiskFreeSpaceA(in LpcStr lpRootPathName, uint lpSectorsPerCluster, uint lpBytesPerSector, uint lpNumberOfFreeClusters, uint lpTotalNumberOfClusters)
	{
		var rootPath = lpRootPathName.ToString() ?? "C:\\";
		_logger.LogInformation("[Kernel32] GetDiskFreeSpaceA(\"{RootPath}\", 0x{LpSectorsPerCluster:X8}, 0x{LpBytesPerSector:X8}, 0x{LpNumberOfFreeClusters:X8}, 0x{LpTotalNumberOfClusters:X8})",
			rootPath, lpSectorsPerCluster, lpBytesPerSector, lpNumberOfFreeClusters, lpTotalNumberOfClusters);

		try
		{
			// Return reasonable default values for disk space
			// These are typical values for a modern disk with 4K sectors
			const uint sectorsPerCluster = 8;     // 8 sectors per cluster (32KB clusters)
			const uint bytesPerSector = 512;       // 512 bytes per sector
			const uint numberOfFreeClusters = 1000000;  // ~32GB free space
			const uint totalNumberOfClusters = 2000000; // ~64GB total space

			if (lpSectorsPerCluster != 0)
			{
				_env.MemWrite32(lpSectorsPerCluster, sectorsPerCluster);
			}

			if (lpBytesPerSector != 0)
			{
				_env.MemWrite32(lpBytesPerSector, bytesPerSector);
			}

			if (lpNumberOfFreeClusters != 0)
			{
				_env.MemWrite32(lpNumberOfFreeClusters, numberOfFreeClusters);
			}

			if (lpTotalNumberOfClusters != 0)
			{
				_env.MemWrite32(lpTotalNumberOfClusters, totalNumberOfClusters);
			}

			return (uint)NativeTypes.Win32Bool.TRUE;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] GetDiskFreeSpaceA: Failed for \"{RootPath}\"", rootPath);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_ACCESS_DENIED;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}
	}

	[DllModuleExport(421, entryPoint: 0x00011516, Version = "5.1.2600.6532")]
	[DllModuleExport(320, entryPoint: 0x0001A254, Version = "4.90.0.3000")]
	private uint GetDriveTypeA(in LpcStr lpRootPathName)
	{
		var rootPath = lpRootPathName.ToString() ?? "C:\\";
		_logger.LogInformation("[Kernel32] GetDriveTypeA(\"{RootPath}\")", rootPath);

		// Drive types:
		// DRIVE_UNKNOWN = 0
		// DRIVE_NO_ROOT_DIR = 1
		// DRIVE_REMOVABLE = 2
		// DRIVE_FIXED = 3
		// DRIVE_REMOTE = 4
		// DRIVE_CDROM = 5
		// DRIVE_RAMDISK = 6

		// For simplicity, return DRIVE_FIXED (3) for all drives
		// In a real implementation, we would check the actual drive type
		const uint DRIVE_FIXED = 3;

		_logger.LogInformation("[Kernel32] GetDriveTypeA: Returning DRIVE_FIXED for \"{RootPath}\"", rootPath);
		return DRIVE_FIXED;
	}

	[DllModuleExport(0)]
	private uint GetLogicalDrives()
	{
		_logger.LogInformation("[Kernel32] GetLogicalDrives()");

		// Return a bitmask of available drives
		// Bit 0 = A:, Bit 1 = B:, Bit 2 = C:, etc.
		// For simplicity, we'll return only C: drive (bit 2 set)
		uint driveMask = 0x04; // C: drive (1 << 2)

		_logger.LogInformation("[Kernel32] GetLogicalDrives: Returning drive mask 0x{DriveMask:X8}", driveMask);

		return driveMask;
	}

	[DllModuleExport(429, entryPoint: 0x000115C8, Version = "5.1.2600.6532")]
	[DllModuleExport(328, entryPoint: 0x0001A308, Version = "4.90.0.3000")]
	private uint GetLogicalDriveStringsA(uint nBufferLength, in LpStr lpBuffer)
	{
		_logger.LogInformation("[Kernel32] GetLogicalDriveStringsA(nBufferLength={NBufferLength}, lpBuffer=0x{LpBuffer:X8})",
			nBufferLength, lpBuffer.Address);

		// Return a string containing available drive letters, each followed by null terminator
		// Format: "C:\0D:\0E:\0\0" (double null at end)
		// For simplicity, we'll return just C: drive
		const string driveString = "C:\\\0";

		var bytes = Encoding.ASCII.GetBytes(driveString);
		var totalLength = (uint)(bytes.Length + 1); // +1 for final null terminator

		if (nBufferLength == 0 || lpBuffer.Address == 0)
		{
			// Caller is querying the required buffer size
			_logger.LogInformation("[Kernel32] GetLogicalDriveStringsA: Returning required buffer size {TotalLength}", totalLength);
			return totalLength;
		}

		if (nBufferLength < totalLength)
		{
			// Buffer too small
			_logger.LogWarning("[Kernel32] GetLogicalDriveStringsA: Buffer too small (need {TotalLength}, have {NBufferLength})",
				totalLength, nBufferLength);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
			return 0;
		}

		try
		{
			// Write the drive string to memory
			_env.MemWriteBytes(lpBuffer.Address, bytes);
			// Write final null terminator
			_env.MemWrite8(lpBuffer.Address + (uint)bytes.Length, 0);

			_logger.LogInformation("[Kernel32] GetLogicalDriveStringsA: Wrote drive string \"{DriveString}\"",
				driveString.TrimEnd('\0'));

			return (uint)(bytes.Length); // Return length without final null
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] GetLogicalDriveStringsA: Failed to write drive strings");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_ACCESS_DENIED;
			return 0;
		}
	}

	[DllModuleExport(601, entryPoint: 0x00012E28, Version = "5.1.2600.6532")]
	[DllModuleExport(481, entryPoint: 0x00020B87, Version = "4.90.0.3000")]
	private uint OutputDebugStringA(in LpcStr lpOutputString)
	{
		var message = lpOutputString.ToString();
		_logger.LogInformation("[Kernel32] OutputDebugStringA: {Message}", message);

		// OutputDebugStringA returns void (0) in the real API
		// It sends the string to the debugger if one is attached
		// For our emulator, we log it via the standard logger which routes to the debug window

		return 0;
	}

	/// <summary>
	/// Causes a breakpoint exception in the current process.
	/// void DebugBreak();
	/// </summary>
	[DllModuleExport(0)]
	private uint DebugBreak()
	{
		_logger.LogInformation("[Kernel32] DebugBreak()");

		// In a real system, this would cause a breakpoint exception
		// For emulation, we just log it and continue
		// Could potentially trigger debugger if one is attached

		return 0; // void function
	}

	[DllModuleExport(691, entryPoint: 0x0000F3CC, Version = "5.1.2600.6532")]
	[DllModuleExport(571, entryPoint: 0x000223DC, Version = "4.90.0.3000")]
	private uint SetUnhandledExceptionFilter(uint lpTopLevelExceptionFilter)
	{
		_logger.LogInformation("[Kernel32] SetUnhandledExceptionFilter(lpTopLevelExceptionFilter=0x{LpTopLevelExceptionFilter:X8})",
			lpTopLevelExceptionFilter);

		// This function sets the top-level exception handler
		// In our emulator, we don't need to fully implement exception handling
		// Just store the handler address and return success

		// Store for potential future use
		var previousHandler = 0u; // We don't track previous handler for now

		_logger.LogInformation("[Kernel32] SetUnhandledExceptionFilter: Set exception handler to 0x{LpTopLevelExceptionFilter:X8}",
			lpTopLevelExceptionFilter);

		return previousHandler;
	}

	// Additional missing implementations
	[DllModuleExport(32)]
	private uint DeviceIoControl(uint hDevice, uint dwIoControlCode, uint lpInBuffer, uint nInBufferSize, uint lpOutBuffer, uint nOutBufferSize, uint lpBytesReturned, uint lpOverlapped)
	{
		_logger.LogInformation("[Kernel32] DeviceIoControl(hDevice=0x{HDevice:X8}, dwIoControlCode=0x{DwIoControlCode:X})", hDevice, dwIoControlCode);

		// Common CD-ROM IOCTL codes (from winioctl.h)
		const uint IOCTL_CDROM_BASE = 0x00000002;
		const uint METHOD_BUFFERED = 0;
		const uint FILE_READ_ACCESS = 0x0001;

		// IOCTL_CDROM_READ_TOC = (IOCTL_CDROM_BASE << 16) | (FILE_READ_ACCESS << 14) | (0x0000 << 2) | METHOD_BUFFERED
		const uint IOCTL_CDROM_READ_TOC = 0x00024000;
		// IOCTL_CDROM_GET_LAST_SESSION = (IOCTL_CDROM_BASE << 16) | (FILE_READ_ACCESS << 14) | (0x000E << 2) | METHOD_BUFFERED
		const uint IOCTL_CDROM_GET_LAST_SESSION = 0x00024038;
		// IOCTL_CDROM_RAW_READ = (IOCTL_CDROM_BASE << 16) | (FILE_READ_ACCESS << 14) | (0x000F << 2) | METHOD_BUFFERED
		const uint IOCTL_CDROM_RAW_READ = 0x0002403C;

		// Handle CD-ROM specific IOCTLs
		switch (dwIoControlCode)
		{
			case IOCTL_CDROM_READ_TOC:
				_logger.LogInformation("[Kernel32] DeviceIoControl: IOCTL_CDROM_READ_TOC - not fully implemented");
				// This would require reading CD track information from CHD metadata
				// For now, return failure
				if (lpBytesReturned != 0)
				{
					_env.MemWrite32(lpBytesReturned, 0);
				}

				return 0; // FALSE

			case IOCTL_CDROM_GET_LAST_SESSION:
				_logger.LogInformation("[Kernel32] DeviceIoControl: IOCTL_CDROM_GET_LAST_SESSION - not fully implemented");
				if (lpBytesReturned != 0)
				{
					_env.MemWrite32(lpBytesReturned, 0);
				}

				return 0; // FALSE

			case IOCTL_CDROM_RAW_READ:
				_logger.LogInformation("[Kernel32] DeviceIoControl: IOCTL_CDROM_RAW_READ - not fully implemented");
				if (lpBytesReturned != 0)
				{
					_env.MemWrite32(lpBytesReturned, 0);
				}

				return 0; // FALSE

			default:
				_logger.LogDebug("[Kernel32] DeviceIoControl: Unsupported IOCTL code 0x{DwIoControlCode:X}", dwIoControlCode);
				if (lpBytesReturned != 0)
				{
					_env.MemWrite32(lpBytesReturned, 0);
				}

				return 0; // FALSE - not supported
		}
	}

	[DllModuleExport(4)]
	private uint ExitThread(uint dwExitCode)
	{
		_logger.LogInformation("[Kernel32] ExitThread(dwExitCode={DwExitCode})", dwExitCode);
		// This function terminates the calling thread
		// For now, just log it
		return 0; // void function
	}

	[DllModuleExport(4)]
	private uint FreeLibrary(uint hLibModule)
	{
		_logger.LogInformation("[Kernel32] FreeLibrary(hLibModule=0x{HLibModule:X8})", hLibModule);
		return 1; // TRUE
	}

	[DllModuleExport(8)]
	private uint GetComputerNameA(uint lpBuffer, uint nSize)
	{
		_logger.LogInformation("[Kernel32] GetComputerNameA(lpBuffer=0x{LpBuffer:X8}, nSize=0x{NSize:X8})", lpBuffer, nSize);
		var computerName = "EMULATOR";
		if (lpBuffer != 0)
		{
			var size = _env.MemRead32(nSize);
			if (size >= (uint)computerName.Length + 1)
			{
				_env.WriteAnsiStringAt(lpBuffer, computerName);
				_env.MemWrite32(nSize, (uint)computerName.Length);
				return 1; // TRUE
			}
		}
		_env.MemWrite32(nSize, (uint)computerName.Length + 1);
		return 0; // FALSE - buffer too small
	}

	[DllModuleExport(0)]
	private uint GetCurrentProcessId()
	{
		_logger.LogInformation("[Kernel32] GetCurrentProcessId()");
		return 1000; // Return a fixed process ID
	}

	[DllModuleExport(12)]
	private uint GetEnvironmentVariableA(in LpcStr lpName, uint lpBuffer, uint nSize)
	{
		var name = lpName.ToString() ?? string.Empty;
		_logger.LogInformation("[Kernel32] GetEnvironmentVariableA(lpName=\"{Name}\", lpBuffer=0x{LpBuffer:X8}, nSize={NSize})",
			name, lpBuffer, nSize);

		// Return empty (variable not found)
		return 0;
	}

	[DllModuleExport(4)]
	private uint GetPriorityClass(uint hProcess)
	{
		_logger.LogInformation("[Kernel32] GetPriorityClass(hProcess=0x{HProcess:X8})", hProcess);
		return 0x00000020; // NORMAL_PRIORITY_CLASS
	}

	[DllModuleExport(4)]
	private uint GetProcessVersion(uint ProcessId)
	{
		_logger.LogInformation("[Kernel32] GetProcessVersion(ProcessId={ProcessId})", ProcessId);
		return 0x04005A00; // Windows 98 version
	}

	[DllModuleExport(8)]
	private uint GetSystemDirectoryA(uint lpBuffer, uint uSize)
	{
		_logger.LogInformation("[Kernel32] GetSystemDirectoryA(lpBuffer=0x{LpBuffer:X8}, uSize={USize})", lpBuffer, uSize);
		var sysDir = "C:\\WINDOWS\\SYSTEM32";
		if (lpBuffer != 0 && uSize >= (uint)sysDir.Length + 1)
		{
			_env.WriteAnsiStringAt(lpBuffer, sysDir);
			return (uint)sysDir.Length;
		}
		return (uint)sysDir.Length + 1;
	}

	[DllModuleExport(8)]
	private uint GetTempPathA(uint nBufferLength, uint lpBuffer)
	{
		_logger.LogInformation("[Kernel32] GetTempPathA(nBufferLength={NBufferLength}, lpBuffer=0x{LpBuffer:X8})", nBufferLength, lpBuffer);
		var tempPath = "C:\\TEMP\\";
		if (lpBuffer != 0 && nBufferLength >= (uint)tempPath.Length + 1)
		{
			_env.WriteAnsiStringAt(lpBuffer, tempPath);
			return (uint)tempPath.Length;
		}
		return (uint)tempPath.Length + 1;
	}

	[DllModuleExport(4)]
	private int GetThreadPriority(uint hThread)
	{
		_logger.LogInformation("[Kernel32] GetThreadPriority(hThread=0x{HThread:X8})", hThread);

		// Handle pseudo-handle for current thread
		const uint CURRENT_THREAD_PSEUDO_HANDLE = 0xFFFFFFFE;
		EmulatedThread? thread;

		thread = (hThread == CURRENT_THREAD_PSEUDO_HANDLE)
			? _env.ThreadScheduler?.CurrentThread
			: _env.ThreadScheduler?.GetThreadByHandle(hThread);

		if (thread != null)
		{
			_logger.LogInformation("[Kernel32] GetThreadPriority: thread {ThreadId} priority = {Priority}",
				thread.ThreadId, thread.Priority);
			return thread.Priority;
		}

		_logger.LogWarning("[Kernel32] GetThreadPriority: invalid thread handle 0x{Handle:X8}", hThread);
		_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
		const int THREAD_PRIORITY_ERROR_RETURN = int.MaxValue; // 0x7FFFFFFF
		return THREAD_PRIORITY_ERROR_RETURN;
	}

	[DllModuleExport(4)]
	private uint GlobalAddAtomA(in LpcStr lpString)
	{
		var str = lpString.ToString() ?? string.Empty;
		_logger.LogInformation("[Kernel32] GlobalAddAtomA(lpString=\"{Str}\")", str);
		return 0xC000; // Return a valid atom value
	}

	[DllModuleExport(4)]
	private uint GlobalDeleteAtom(uint nAtom)
	{
		_logger.LogInformation("[Kernel32] GlobalDeleteAtom(nAtom=0x{NAtom:X})", nAtom);
		return 0; // SUCCESS
	}

	[DllModuleExport(4)]
	private uint GlobalFindAtomA(in LpcStr lpString)
	{
		var str = lpString.ToString() ?? string.Empty;
		_logger.LogInformation("[Kernel32] GlobalFindAtomA(lpString=\"{Str}\")", str);
		return 0; // Not found
	}

	[DllModuleExport(4)]
	private uint GlobalFlags(uint hMem)
	{
		_logger.LogInformation("[Kernel32] GlobalFlags(hMem=0x{HMem:X8})", hMem);
		return 0; // GMEM_FIXED (no flags)
	}

	[DllModuleExport(12)]
	private uint GlobalGetAtomNameA(uint nAtom, uint lpBuffer, int nSize)
	{
		_logger.LogInformation("[Kernel32] GlobalGetAtomNameA(nAtom=0x{NAtom:X}, lpBuffer=0x{LpBuffer:X8}, nSize={NSize})",
			nAtom, lpBuffer, nSize);
		return 0; // Empty string
	}

	[DllModuleExport(4)]
	private uint GlobalMemoryStatus(uint lpBuffer)
	{
		_logger.LogInformation("[Kernel32] GlobalMemoryStatus(lpBuffer=0x{LpBuffer:X8})", lpBuffer);
		if (lpBuffer != 0)
		{
			// MEMORYSTATUS structure
			_env.MemWrite32(lpBuffer, 32); // dwLength
			_env.MemWrite32(lpBuffer + 4, 50); // dwMemoryLoad (50%)
			_env.MemWrite32(lpBuffer + 8, 0x40000000); // dwTotalPhys (1GB)
			_env.MemWrite32(lpBuffer + 12, 0x20000000); // dwAvailPhys (512MB)
			_env.MemWrite32(lpBuffer + 16, 0x80000000); // dwTotalPageFile (2GB)
			_env.MemWrite32(lpBuffer + 20, 0x40000000); // dwAvailPageFile (1GB)
			_env.MemWrite32(lpBuffer + 24, 0x7FFF0000); // dwTotalVirtual
			_env.MemWrite32(lpBuffer + 28, 0x7FFE0000); // dwAvailVirtual
		}
		return 0; // void function
	}

	[DllModuleExport(12)]
	private uint GlobalReAlloc(uint hMem, uint dwBytes, uint uFlags)
	{
		_logger.LogInformation("[Kernel32] GlobalReAlloc(hMem=0x{HMem:X8}, dwBytes={DwBytes}, uFlags=0x{UFlags:X})",
			hMem, dwBytes, uFlags);
		return hMem; // Return same handle (stub)
	}

	[DllModuleExport(4)]
	private uint GlobalSize(uint hMem)
	{
		_logger.LogInformation("[Kernel32] GlobalSize(hMem=0x{HMem:X8})", hMem);
		return 0x10000; // Return 64KB (stub)
	}

	[DllModuleExport(4)]
	private int InterlockedDecrement(uint lpAddend)
	{
		_logger.LogInformation("[Kernel32] InterlockedDecrement(lpAddend=0x{LpAddend:X8})", lpAddend);
		if (lpAddend != 0)
		{
			var value = (int)_env.MemRead32(lpAddend);
			value--;
			_env.MemWrite32(lpAddend, (uint)value);
			return value;
		}
		return 0;
	}

	[DllModuleExport(4)]
	private int InterlockedIncrement(uint lpAddend)
	{
		_logger.LogInformation("[Kernel32] InterlockedIncrement(lpAddend=0x{LpAddend:X8})", lpAddend);
		if (lpAddend != 0)
		{
			var value = (int)_env.MemRead32(lpAddend);
			value++;
			_env.MemWrite32(lpAddend, (uint)value);
			return value;
		}
		return 0;
	}

	/// <summary>
	/// Sets a 32-bit variable to the specified value as an atomic operation.
	/// LONG InterlockedExchange(
	///   [in, out] LONG volatile *Target,
	///   [in]      LONG          Value
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint InterlockedExchange(uint target, uint value)
	{
		_logger.LogInformation("[Kernel32] InterlockedExchange(target=0x{Target:X8}, value=0x{Value:X8})",
			target, value);

		if (target != 0)
		{
			var oldValue = _env.MemRead32(target);
			_env.MemWrite32(target, value);
			return oldValue;
		}
		return 0;
	}

	/// <summary>
	/// Performs an atomic compare-and-exchange operation on the specified values.
	/// LONG InterlockedCompareExchange(
	///   [in, out] LONG volatile *Destination,
	///   [in]      LONG          Exchange,
	///   [in]      LONG          Comparand
	/// );
	/// Returns the initial value of the Destination parameter.
	/// </summary>
	[DllModuleExport(12)]
	private uint InterlockedCompareExchange(uint destination, uint exchange, uint comparand)
	{
		_logger.LogInformation("[Kernel32] InterlockedCompareExchange(destination=0x{Destination:X8}, exchange=0x{Exchange:X8}, comparand=0x{Comparand:X8})",
			destination, exchange, comparand);

		if (destination != 0)
		{
			lock (_interlockedLock)
			{
				var currentValue = _env.MemRead32(destination);
				if (currentValue == comparand)
				{
					_env.MemWrite32(destination, exchange);
				}
				return currentValue;
			}
		}
		return 0;
	}

	[DllModuleExport(4)]
	private uint LocalFree(uint hMem)
	{
		_logger.LogInformation("[Kernel32] LocalFree(hMem=0x{HMem:X8})", hMem);
		return 0; // NULL on success
	}

	[DllModuleExport(12)]
	private uint LocalReAlloc(uint hMem, uint uBytes, uint uFlags)
	{
		_logger.LogInformation("[Kernel32] LocalReAlloc(hMem=0x{HMem:X8}, uBytes={UBytes}, uFlags=0x{UFlags:X})",
			hMem, uBytes, uFlags);
		return hMem; // Return same handle (stub)
	}

	/// <summary>
	/// Retrieves the handle of a local memory object.
	/// HLOCAL LocalHandle(
	///   [in] LPCVOID pMem
	/// );
	/// </summary>
	[DllModuleExport(694, IsStub = true)]
	private uint LocalHandle(uint pMem)
	{
		_logger.LogDebug("[Kernel32] LocalHandle(pMem=0x{PMem:X8})", pMem);
		// In flat 32-bit mode, the memory pointer is the handle
		return pMem;
	}

	[DllModuleExport(8)]
	private int lstrcmpA(in LpcStr lpString1, in LpcStr lpString2)
	{
		var str1 = lpString1.ToString() ?? string.Empty;
		var str2 = lpString2.ToString() ?? string.Empty;
		_logger.LogInformation("[Kernel32] lstrcmpA(lpString1=\"{Str1}\", lpString2=\"{Str2}\")", str1, str2);
		return string.Compare(str1, str2, StringComparison.Ordinal);
	}

	[DllModuleExport(8)]
	private int lstrcmpiA(in LpcStr lpString1, in LpcStr lpString2)
	{
		var str1 = lpString1.ToString() ?? string.Empty;
		var str2 = lpString2.ToString() ?? string.Empty;
		_logger.LogInformation("[Kernel32] lstrcmpiA(lpString1=\"{Str1}\", lpString2=\"{Str2}\")", str1, str2);
		return string.Compare(str1, str2, StringComparison.OrdinalIgnoreCase);
	}

	[DllModuleExport(12)]
	private uint lstrcpynA(uint lpString1, in LpcStr lpString2, int iMaxLength)
	{
		var str2 = lpString2.ToString() ?? string.Empty;
		_logger.LogInformation("[Kernel32] lstrcpynA(lpString1=0x{LpString1:X8}, lpString2=\"{Str2}\", iMaxLength={IMaxLength})",
			lpString1, str2, iMaxLength);
		if (lpString1 != 0 && iMaxLength > 0)
		{
			var toCopy = str2.Length < iMaxLength - 1 ? str2 : str2.Substring(0, iMaxLength - 1);
			_env.WriteAnsiStringAt(lpString1, toCopy);
		}
		return lpString1;
	}

	[DllModuleExport(12)]
	private int MulDiv(int nNumber, int nNumerator, int nDenominator)
	{
		_logger.LogInformation("[Kernel32] MulDiv(nNumber={NNumber}, nNumerator={NNumerator}, nDenominator={NDenominator})",
			nNumber, nNumerator, nDenominator);
		if (nDenominator == 0)
		{
			return -1;
		}

		return (int)((long)nNumber * nNumerator / nDenominator);
	}

	[DllModuleExport(12)]
	private uint OpenMutexA(uint dwDesiredAccess, uint bInheritHandle, in LpcStr lpName)
	{
		var name = lpName.ToString() ?? string.Empty;
		_logger.LogInformation("[Kernel32] OpenMutexA(dwDesiredAccess=0x{DwDesiredAccess:X}, bInheritHandle={BInheritHandle}, lpName=\"{Name}\")",
			dwDesiredAccess, bInheritHandle, name);
		return 0; // NULL - mutex doesn't exist
	}

	[DllModuleExport(4)]
	private uint RemoveDirectoryA(in LpcStr lpPathName)
	{
		var pathName = lpPathName.ToString() ?? string.Empty;
		_logger.LogInformation("[Kernel32] RemoveDirectoryA(lpPathName=\"{PathName}\")", pathName);
		return 1; // TRUE (stub)
	}

	[DllModuleExport(4)]
	private uint SetErrorMode(uint uMode)
	{
		_logger.LogInformation("[Kernel32] SetErrorMode(uMode=0x{UMode:X})", uMode);
		return 0; // Return previous mode (0)
	}

	[DllModuleExport(8)]
	private uint SetPriorityClass(uint hProcess, uint dwPriorityClass)
	{
		_logger.LogInformation("[Kernel32] SetPriorityClass(hProcess=0x{HProcess:X8}, dwPriorityClass=0x{DwPriorityClass:X})",
			hProcess, dwPriorityClass);
		return 1; // TRUE
	}

	[DllModuleExport(8)]
	private uint SetThreadPriority(uint hThread, int nPriority)
	{
		_logger.LogInformation("[Kernel32] SetThreadPriority(hThread=0x{HThread:X8}, nPriority={NPriority})",
			hThread, nPriority);

		// Handle pseudo-handle for current thread
		const uint CURRENT_THREAD_PSEUDO_HANDLE = 0xFFFFFFFE;
		EmulatedThread? thread;

		thread = (hThread == CURRENT_THREAD_PSEUDO_HANDLE)
			? _env.ThreadScheduler?.CurrentThread
			: _env.ThreadScheduler?.GetThreadByHandle(hThread);

		if (thread != null)
		{
			thread.Priority = nPriority;
			_logger.LogInformation("[Kernel32] SetThreadPriority: thread {ThreadId} priority set to {Priority}",
				thread.ThreadId, nPriority);
			return (uint)NativeTypes.Win32Bool.TRUE;
		}

		_logger.LogWarning("[Kernel32] SetThreadPriority: invalid thread handle 0x{Handle:X8}", hThread);
		_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
		return (uint)NativeTypes.Win32Bool.FALSE;
	}

	[DllModuleExport(20)]
	private uint WriteConsoleA(uint hConsoleOutput, uint lpBuffer, uint nNumberOfCharsToWrite, uint lpNumberOfCharsWritten, uint lpReserved)
	{
		_logger.LogInformation("[Kernel32] WriteConsoleA(hConsoleOutput=0x{HConsoleOutput:X8}, nNumberOfCharsToWrite={NNumberOfCharsToWrite})",
			hConsoleOutput, nNumberOfCharsToWrite);

		if (lpBuffer == 0 || nNumberOfCharsToWrite == 0)
		{
			if (lpNumberOfCharsWritten != 0)
			{
				_env.MemWrite32(lpNumberOfCharsWritten, 0);
			}
			return 1; // TRUE
		}

		// Read the buffer content from emulated memory
		var text = _env.ReadAnsiString(lpBuffer, (int)nNumberOfCharsToWrite);
		
		// Send to host for display in terminal
		if (_env.Host != null && !string.IsNullOrEmpty(text))
		{
			_env.Host.OnStdOutput(text);
		}

		// Write the number of characters written
		if (lpNumberOfCharsWritten != 0)
		{
			_env.MemWrite32(lpNumberOfCharsWritten, nNumberOfCharsToWrite);
		}

		return 1; // TRUE
	}

	[DllModuleExport(16)]
	private uint WritePrivateProfileStringA(in LpcStr lpAppName, in LpcStr lpKeyName, in LpcStr lpString, in LpcStr lpFileName)
	{
		var appName = lpAppName.ToString() ?? string.Empty;
		var keyName = lpKeyName.ToString() ?? string.Empty;
		var str = lpString.ToString() ?? string.Empty;
		var fileName = lpFileName.ToString() ?? string.Empty;
		_logger.LogInformation("[Kernel32] WritePrivateProfileStringA(lpAppName=\"{AppName}\", lpKeyName=\"{KeyName}\", lpString=\"{Str}\", lpFileName=\"{FileName}\")",
			appName, keyName, str, fileName);
		return 1; // TRUE (stub)
	}

	/// <summary>
	/// Replaces the keys and values for the specified section in an initialization file.
	/// BOOL WritePrivateProfileSectionA(
	///   [in] LPCSTR lpAppName,
	///   [in] LPCSTR lpString,
	///   [in] LPCSTR lpFileName
	/// );
	/// lpString is a buffer containing one or more null-terminated strings, terminated by a final null character.
	/// Each string has the form key=value.
	/// </summary>
	[DllModuleExport(16, IsStub = true)]
	private uint WritePrivateProfileSectionA(in LpcStr lpAppName, in LpcStr lpString, in LpcStr lpFileName)
	{
		var appName = lpAppName.ToString() ?? string.Empty;
		var fileName = lpFileName.ToString() ?? string.Empty;
		_logger.LogInformation("[Kernel32] WritePrivateProfileSectionA(lpAppName=\"{AppName}\", lpFileName=\"{FileName}\")",
			appName, fileName);
		return 1; // TRUE (stub)
	}

	/// <summary>
	/// Duplicates an object handle.
	/// </summary>
	[DllModuleExport(28)]
	private uint DuplicateHandle(uint hSourceProcessHandle, uint hSourceHandle, uint hTargetProcessHandle,
		uint lpTargetHandle, uint dwDesiredAccess, uint bInheritHandle, uint dwOptions)
	{
		_logger.LogInformation("[Kernel32] DuplicateHandle(hSourceHandle=0x{HSourceHandle:X8}, lpTargetHandle=0x{LpTargetHandle:X8})", hSourceHandle, lpTargetHandle);
		if (lpTargetHandle != 0)
		{
			_env.MemWrite32(lpTargetHandle, hSourceHandle);
		}
		return 1; // TRUE
	}

	/// <summary>
	/// Retrieves the termination status of the specified process.
	/// </summary>
	[DllModuleExport(8)]
	private uint GetExitCodeProcess(uint hProcess, uint lpExitCode)
	{
		_logger.LogInformation("[Kernel32] GetExitCodeProcess(hProcess=0x{HProcess:X8})", hProcess);
		const uint STILL_ACTIVE = 259;
		if (lpExitCode != 0)
		{
			_env.MemWrite32(lpExitCode, STILL_ACTIVE);
		}
		return 1; // TRUE
	}

	/// <summary>
	/// Retrieves file system attributes for a specified file or directory.
	/// </summary>
	[DllModuleExport(4)]
	private uint GetProcessHeap()
	{
		_logger.LogInformation("[Kernel32] GetProcessHeap()");
		// Return a handle to the default process heap
		// We use a constant value (0x00500000) to represent the process heap
		const uint PROCESS_HEAP_HANDLE = 0x00500000;
		return PROCESS_HEAP_HANDLE;
	}

	[DllModuleExport(1)]
	private uint GetFileAttributesA(in LpcStr lpFileName)
	{
		var fileName = lpFileName.ToString() ?? string.Empty;
		_logger.LogInformation("[Kernel32] GetFileAttributesA(lpFileName=\"{FileName}\")", fileName);

		if (string.IsNullOrEmpty(fileName))
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0xFFFFFFFF; // INVALID_FILE_ATTRIBUTES
		}

		// Resolve relative paths
		var resolvedPath = fileName;
		if (!Path.IsPathRooted(fileName))
		{
			resolvedPath = Path.Combine(_env.CurrentDirectory, fileName);
		}

		// Try to get file attributes
		try
		{
			// VFS is required for file operations
			if (_env.VirtualFileSystem == null)
			{
				_logger.LogError("[Kernel32] GetFileAttributesA: VFS not initialized");
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
				return 0xFFFFFFFF; // INVALID_FILE_ATTRIBUTES
			}

			if (_env.VirtualFileSystem.FileExists(resolvedPath))
			{
				// For now, return FILE_ATTRIBUTE_NORMAL for all files
				// A full implementation would check actual file attributes
				_logger.LogInformation("[Kernel32] GetFileAttributesA: file exists, returning FILE_ATTRIBUTE_NORMAL");
				return 0x80; // FILE_ATTRIBUTE_NORMAL
			}
			if (_env.VirtualFileSystem.DirectoryExists(resolvedPath))
			{
				_logger.LogInformation("[Kernel32] GetFileAttributesA: path is a directory, returning FILE_ATTRIBUTE_DIRECTORY");
				return 0x10; // FILE_ATTRIBUTE_DIRECTORY
			}
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "[Kernel32] GetFileAttributesA: Exception while checking file attributes");
		}

		// File not found
		_logger.LogInformation("[Kernel32] GetFileAttributesA: file not found");
		_lastError = (uint)NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
		return 0xFFFFFFFF; // INVALID_FILE_ATTRIBUTES
	}

	/// <summary>
	/// Retrieves the size of the specified file.
	/// </summary>
	[DllModuleExport(8)]
	private uint GetFileSize(uint hFile, uint lpFileSizeHigh)
	{
		_logger.LogInformation("[Kernel32] GetFileSize(hFile=0x{HFile:X8})", hFile);
		const uint INVALID_FILE_SIZE = 0xFFFFFFFF;
		// Stub - return 0 size
		if (lpFileSizeHigh != 0)
		{
			_env.MemWrite32(lpFileSizeHigh, 0);
		}
		return 0;
	}

	/// <summary>
	/// Retrieves the date and time that a file or directory was created, last accessed, and last modified.
	/// </summary>
	[DllModuleExport(16)]
	private uint GetFileTime(uint hFile, uint lpCreationTime, uint lpLastAccessTime, uint lpLastWriteTime)
	{
		_logger.LogInformation("[Kernel32] GetFileTime(hFile=0x{HFile:X8})", hFile);
		ulong defaultTime = 0;
		if (lpCreationTime != 0)
		{
			var ft = new FileTimeRef(_env.Memory, lpCreationTime);
			ft.dwLowDateTime = (uint)(defaultTime & 0xFFFFFFFF);
			ft.dwHighDateTime = (uint)(defaultTime >> 32);
		}
		if (lpLastAccessTime != 0)
		{
			var ft = new FileTimeRef(_env.Memory, lpLastAccessTime);
			ft.dwLowDateTime = (uint)(defaultTime & 0xFFFFFFFF);
			ft.dwHighDateTime = (uint)(defaultTime >> 32);
		}
		if (lpLastWriteTime != 0)
		{
			var ft = new FileTimeRef(_env.Memory, lpLastWriteTime);
			ft.dwLowDateTime = (uint)(defaultTime & 0xFFFFFFFF);
			ft.dwHighDateTime = (uint)(defaultTime >> 32);
		}
		return 1; // TRUE
	}

	/// <summary>
	/// Retrieves the full path and file name of the specified file.
	/// </summary>
	[DllModuleExport(16)]
	private uint GetFullPathNameA(in LpcStr lpFileName, uint nBufferLength, in LpStr lpBuffer, uint lpFilePart)
	{
		var fileName = lpFileName.ToString() ?? string.Empty;
		_logger.LogInformation("[Kernel32] GetFullPathNameA(lpFileName=\"{FileName}\")", fileName);
		var fullPath = "C:\\\\" + fileName; // Stub - just prepend C:\
		var requiredLength = (uint)fullPath.Length + 1;
		if (nBufferLength == 0 || lpBuffer.Address == 0)
		{
			return requiredLength;
		}
		if (nBufferLength < requiredLength)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
			return requiredLength;
		}
		_env.WriteAnsiStringAt(lpBuffer.Address, fullPath);
		if (lpFilePart != 0)
		{
			var lastSlash = fullPath.LastIndexOfAny(['\\', '/']);
			if (lastSlash >= 0)
			{
				var filePartOffset = (uint)(lastSlash + 1);
				_env.MemWrite32(lpFilePart, lpBuffer.Address + filePartOffset);
			}
			else
			{
				_env.MemWrite32(lpFilePart, lpBuffer.Address);
			}
		}
		return (uint)fullPath.Length;
	}

	/// <summary>
	/// Retrieves information about a locale specified by identifier (Unicode version).
	/// </summary>
	[DllModuleExport(16)]
	private uint GetLocaleInfoW(uint Locale, uint LCType, uint lpLCData, int cchData)
	{
		_logger.LogInformation("[Kernel32] GetLocaleInfoW(Locale=0x{Locale:X}, LCType=0x{LCType:X})", Locale, LCType);
		var localeData = "en-US";
		var requiredLength = localeData.Length + 1;
		if (cchData == 0 || lpLCData == 0)
		{
			return (uint)requiredLength;
		}
		if (cchData < requiredLength)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
			return 0;
		}
		var wideBytes = Encoding.Unicode.GetBytes(localeData + "\0");
		_env.MemWriteBytes(lpLCData, wideBytes);
		return (uint)requiredLength;
	}

	/// <summary>
	/// Retrieves information about the file system and volume associated with the specified root directory.
	/// </summary>
	[DllModuleExport(32)]
	private uint GetVolumeInformationA(in LpcStr lpRootPathName, in LpStr lpVolumeNameBuffer, uint nVolumeNameSize,
		uint lpVolumeSerialNumber, uint lpMaximumComponentLength, uint lpFileSystemFlags, in LpStr lpFileSystemNameBuffer, uint nFileSystemNameSize)
	{
		var rootPath = lpRootPathName.ToString() ?? "C:\\";
		_logger.LogInformation("[Kernel32] GetVolumeInformationA(lpRootPathName=\"{RootPath}\")", rootPath);
		if (lpVolumeNameBuffer.Address != 0 && nVolumeNameSize > 0)
		{
			_env.WriteAnsiStringAt(lpVolumeNameBuffer.Address, "Win32Emu");
		}
		if (lpVolumeSerialNumber != 0)
		{
			_env.MemWrite32(lpVolumeSerialNumber, 0x12345678);
		}
		if (lpMaximumComponentLength != 0)
		{
			_env.MemWrite32(lpMaximumComponentLength, 255);
		}
		if (lpFileSystemFlags != 0)
		{
			const uint FILE_CASE_PRESERVED_NAMES = 0x00000002;
			const uint FILE_UNICODE_ON_DISK = 0x00000004;
			_env.MemWrite32(lpFileSystemFlags, FILE_CASE_PRESERVED_NAMES | FILE_UNICODE_ON_DISK);
		}
		if (lpFileSystemNameBuffer.Address != 0 && nFileSystemNameSize > 0)
		{
			_env.WriteAnsiStringAt(lpFileSystemNameBuffer.Address, "NTFS");
		}
		return 1; // TRUE
	}

	/// <summary>
	/// Locks a region of an open file.
	/// </summary>
	[DllModuleExport(20)]
	private uint LockFile(uint hFile, uint dwFileOffsetLow, uint dwFileOffsetHigh, uint nNumberOfBytesToLockLow, uint nNumberOfBytesToLockHigh)
	{
		_logger.LogInformation("[Kernel32] LockFile(hFile=0x{HFile:X8})", hFile);
		return 1; // TRUE
	}

	/// <summary>
	/// Unlocks a region of an open file.
	/// </summary>
	[DllModuleExport(20)]
	private uint UnlockFile(uint hFile, uint dwFileOffsetLow, uint dwFileOffsetHigh, uint nNumberOfBytesToUnlockLow, uint nNumberOfBytesToUnlockHigh)
	{
		_logger.LogInformation("[Kernel32] UnlockFile(hFile=0x{HFile:X8})", hFile);
		return 1; // TRUE
	}

	[DllModuleExport(1)]
	private uint GetSystemDefaultLCID()
	{
		_logger.LogInformation("[Kernel32] GetSystemDefaultLCID()");
		// Return US English locale ID: 0x0409
		const uint LOCALE_US_ENGLISH = 0x0409;
		return LOCALE_US_ENGLISH;
	}

	/// <summary>
	/// Retrieves the user default locale identifier.
	/// LCID GetUserDefaultLCID();
	/// </summary>
	[DllModuleExport(0)]
	private uint GetUserDefaultLCID()
	{
		_logger.LogInformation("[Kernel32] GetUserDefaultLCID()");
		// Return US English locale ID: 0x0409
		const uint LOCALE_US_ENGLISH = 0x0409;
		return LOCALE_US_ENGLISH;
	}

	/// <summary>
	/// Determines whether a specified code page is valid.
	/// BOOL IsValidCodePage(
	///   [in] UINT CodePage
	/// );
	/// </summary>
	[DllModuleExport(4)]
	private uint IsValidCodePage(uint codePage)
	{
		_logger.LogInformation("[Kernel32] IsValidCodePage(codePage={CodePage})", codePage);

		// Common code pages that we'll consider valid
		// 437 = OEM United States, 1252 = Windows Latin 1
		// For simplicity, accept any code page
		return 1; // TRUE
	}

	/// <summary>
	/// Determines whether a specified locale identifier is valid.
	/// BOOL IsValidLocale(
	///   [in] LCID   Locale,
	///   [in] DWORD  dwFlags
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint IsValidLocale(uint locale, uint dwFlags)
	{
		_logger.LogInformation("[Kernel32] IsValidLocale(locale=0x{Locale:X8}, dwFlags=0x{DwFlags:X8})",
			locale, dwFlags);

		// For simplicity, accept any locale as valid
		return 1; // TRUE
	}

	/// <summary>
	/// Retrieves information about a locale specified by identifier.
	/// </summary>
	[DllModuleExport(16)]
	private uint GetLocaleInfoA(uint locale, uint lcType, in LpStr lpLCData, int cchData)
	{
		_logger.LogInformation("[Kernel32] GetLocaleInfoA(locale=0x{Locale:X8}, lcType=0x{LcType:X8}, cchData={CchData})",
			locale, lcType, cchData);

		// Common locale information requests
		string? result = lcType switch
		{
			0x1000 => "en", // LOCALE_SISO639LANGNAME - ISO 639 language code
			0x1001 => "US", // LOCALE_SISO3166CTRYNAME - ISO 3166 country code
			0x0002 => "409", // LOCALE_ILANGUAGE
			0x1004 => "1252", // LOCALE_IDEFAULTANSICODEPAGE
			_ => ""
		};

		if (result == null || result.Length == 0)
		{
			_logger.LogInformation("[Kernel32] GetLocaleInfoA: unsupported lcType, returning empty");
			return 0;
		}

		// If lpLCData is null, return required buffer size
		if (lpLCData.Address == 0)
		{
			return (uint)(result.Length + 1);
		}

		// Write the result to the buffer
		if (cchData >= result.Length + 1)
		{
			lpLCData.Write(_env.Memory, result, true);
			return (uint)(result.Length + 1);
		}

		// Buffer too small
		return 0;
	}

	[DllModuleExport(1)]
	private uint GetDateFormatA(uint locale, uint dwFlags, uint lpDate, in LpcStr lpFormat, in LpStr lpDateStr, int cchDate)
	{
		var format = lpFormat.ToString();
		_logger.LogInformation("[Kernel32] GetDateFormatA(locale=0x{Locale:X8}, dwFlags=0x{DwFlags:X8}, lpFormat=\"{Format}\", cchDate={CchDate})",
			locale, dwFlags, format, cchDate);

		// Get current date if lpDate is null (0)
		DateTime date = DateTime.Now;

		// If lpDate is provided, read SYSTEMTIME structure
		if (lpDate != 0)
		{
			var systemTime = new SystemTimeRef(_env.Memory, lpDate);
			try
			{
				date = new DateTime(systemTime.wYear, systemTime.wMonth, systemTime.wDay);
			}
			catch
			{
				date = DateTime.Now;
			}
		}

		// Format the date
		string result;
		if (!string.IsNullOrEmpty(format))
		{
			// Use custom format (simplified, not full Win32 format string support)
			result = date.ToString(format);
		}
		else
		{
			// Use default short date format
			result = date.ToString("MM/dd/yyyy");
		}

		// If lpDateStr is null, return required buffer size
		if (lpDateStr.Address == 0)
		{
			return (uint)(result.Length + 1);
		}

		// Write the result to the buffer
		if (cchDate >= result.Length + 1)
		{
			lpDateStr.Write(_env.Memory, result, true);
			return (uint)(result.Length + 1);
		}

		// Buffer too small
		return 0;
	}

	[DllModuleExport(1)]
	private uint ExpandEnvironmentStringsA(in LpcStr lpSrc, in LpStr lpDst, uint nSize)
	{
		var src = lpSrc.ToString() ?? string.Empty;
		_logger.LogInformation("[Kernel32] ExpandEnvironmentStringsA(lpSrc=\"{Src}\", nSize={NSize})", src, nSize);

		// Simple environment variable expansion
		// For now, just copy the string without expansion
		// A full implementation would expand %VARIABLE% patterns
		var result = src;

		// If lpDst is null, return required buffer size
		if (lpDst.Address == 0)
		{
			return (uint)(result.Length + 1);
		}

		// Write the result to the buffer
		if (nSize >= result.Length + 1)
		{
			lpDst.Write(_env.Memory, result, true);
			return (uint)(result.Length + 1);
		}

		// Buffer too small
		_lastError = (uint)NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
		return (uint)(result.Length + 1);
	}

	[DllModuleExport(1)]
	private uint GetPrivateProfileStringA(in LpcStr lpAppName, in LpcStr lpKeyName, in LpcStr lpDefault, in LpStr lpReturnedString, uint nSize, in LpcStr lpFileName)
	{
		var appName = lpAppName.ToString() ?? string.Empty;
		var keyName = lpKeyName.ToString() ?? string.Empty;
		var defaultValue = lpDefault.ToString() ?? string.Empty;
		var fileName = lpFileName.ToString() ?? string.Empty;

		_logger.LogInformation("[Kernel32] GetPrivateProfileStringA(lpAppName=\"{AppName}\", lpKeyName=\"{KeyName}\", lpDefault=\"{Default}\", nSize={NSize}, lpFileName=\"{FileName}\")",
			appName, keyName, defaultValue, nSize, fileName);

		// For now, always return the default value
		// A full implementation would read from INI files
		var result = defaultValue;

		// Write the result to the buffer
		if (nSize > 0)
		{
			var writeSize = Math.Min(result.Length, (int)nSize - 1);
			if (writeSize > 0)
			{
				lpReturnedString.Write(_env.Memory, result.Substring(0, writeSize), true);
			}
			else
			{
				// Write empty string
				lpReturnedString.Write(_env.Memory, "", true);
			}
			return (uint)writeSize;
		}

		return 0;
	}

	/// <summary>
	/// Retrieves all the keys and values for the specified section of an initialization file.
	/// DWORD GetPrivateProfileSectionA(
	///   [in]  LPCSTR lpAppName,
	///   [out] LPSTR  lpReturnedString,
	///   [in]  DWORD  nSize,
	///   [in]  LPCSTR lpFileName
	/// );
	/// Returns: The number of characters copied to the buffer, not including the terminating null character.
	/// If the buffer is not large enough to contain all the key name and value pairs associated with the named section,
	/// the return value is equal to nSize minus two.
	/// Format: The buffer contains one or more null-terminated strings, terminated by a final null character.
	/// Each string has the form key=value.
	/// </summary>
	[DllModuleExport(16, IsStub = true)]
	private uint GetPrivateProfileSectionA(in LpcStr lpAppName, in LpStr lpReturnedString, uint nSize, in LpcStr lpFileName)
	{
		var appName = lpAppName.ToString() ?? string.Empty;
		var fileName = lpFileName.ToString() ?? string.Empty;

		_logger.LogInformation("[Kernel32] GetPrivateProfileSectionA(lpAppName=\"{AppName}\", nSize={NSize}, lpFileName=\"{FileName}\")",
			appName, nSize, fileName);

		// Stub implementation: return empty section (just double null terminator)
		if (nSize >= 2)
		{
			// Write double null terminator for empty section
			lpReturnedString.Write(_env.Memory, "", true);
			_env.MemWrite8(lpReturnedString.Address + 1, 0);
		}
		
		return 0; // No characters copied (excluding terminators)
	}

	/// <summary>
	/// Creates a new process and its primary thread.
	/// </summary>
	[DllModuleExport(72)]
	private uint CreateProcessA(in LpcStr lpApplicationName, in LpStr lpCommandLine, uint lpProcessAttributes,
		uint lpThreadAttributes, uint bInheritHandles, uint dwCreationFlags, uint lpEnvironment,
		in LpcStr lpCurrentDirectory, uint lpStartupInfo, uint lpProcessInformation)
	{
		var appName = lpApplicationName.ToString() ?? string.Empty;
		var cmdLine = lpCommandLine.ToString() ?? string.Empty;
		var currentDir = lpCurrentDirectory.ToString() ?? string.Empty;

		_logger.LogInformation("[Kernel32] CreateProcessA(lpApplicationName=\"{AppName}\", lpCommandLine=\"{CmdLine}\", dwCreationFlags=0x{Flags:X8}, lpCurrentDirectory=\"{CurrentDir}\")",
			appName, cmdLine, dwCreationFlags, currentDir);

		// Stub implementation - CreateProcess is complex and not fully supported
		// Return failure for now
		_lastError = (uint)NativeTypes.Win32Error.ERROR_ACCESS_DENIED;
		return 0; // FALSE
	}

	/// <summary>
	/// Creates a new process and its primary thread (Unicode version).
	/// </summary>
	[DllModuleExport(72)]
	private uint CreateProcessW(in LpcWStr lpApplicationName, in LpWStr lpCommandLine, uint lpProcessAttributes,
		uint lpThreadAttributes, uint bInheritHandles, uint dwCreationFlags, uint lpEnvironment,
		in LpcWStr lpCurrentDirectory, uint lpStartupInfo, uint lpProcessInformation)
	{
		var appName = lpApplicationName.Read(_env.Memory) ?? string.Empty;
		var cmdLine = lpCommandLine.Read(_env.Memory) ?? string.Empty;
		var currentDir = lpCurrentDirectory.Read(_env.Memory) ?? string.Empty;

		_logger.LogInformation("[Kernel32] CreateProcessW(lpApplicationName=\"{AppName}\", lpCommandLine=\"{CmdLine}\", dwCreationFlags=0x{Flags:X8}, lpCurrentDirectory=\"{CurrentDir}\")",
			appName, cmdLine, dwCreationFlags, currentDir);

		// Stub implementation - CreateProcess is complex and not fully supported
		// Return failure for now
		_lastError = (uint)NativeTypes.Win32Error.ERROR_ACCESS_DENIED;
		return (uint)NativeTypes.Win32Bool.FALSE;
	}

	/// <summary>
	/// Opens an existing local process object.
	/// </summary>
	[DllModuleExport(8)]
	private uint OpenProcess(uint dwDesiredAccess, uint bInheritHandle, uint dwProcessId)
	{
		_logger.LogInformation("[Kernel32] OpenProcess(dwDesiredAccess=0x{Access:X8}, bInheritHandle={Inherit}, dwProcessId={Pid})",
			dwDesiredAccess, bInheritHandle, dwProcessId);

		// Stub implementation - for emulation, return a dummy process handle
		// A full implementation would track actual processes
		if (dwProcessId == 0)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}

		// Return a pseudo-handle (non-zero indicates success)
		return PROCESS_HANDLE_BASE + dwProcessId;
	}

	[DllModuleExport(8)]
	private uint FatalAppExitA(uint uAction, in LpcStr lpMessageText)
	{
		var message = lpMessageText.ToString() ?? string.Empty;
		_logger.LogError("[Kernel32] FatalAppExitA(uAction={UAction}, lpMessageText=\"{Message}\")", uAction, message);

		// Stub - just log the error
		return 0; // void function
	}

	[DllModuleExport(16)]
	private uint GetPrivateProfileIntA(in LpcStr lpAppName, in LpcStr lpKeyName, int nDefault, in LpcStr lpFileName)
	{
		var appName = lpAppName.ToString() ?? string.Empty;
		var keyName = lpKeyName.ToString() ?? string.Empty;
		var fileName = lpFileName.ToString() ?? string.Empty;

		_logger.LogInformation("[Kernel32] GetPrivateProfileIntA(lpAppName=\"{AppName}\", lpKeyName=\"{KeyName}\", nDefault={NDefault}, lpFileName=\"{FileName}\")",
			appName, keyName, nDefault, fileName);

		// Stub - return default value
		return (uint)nDefault;
	}

	[DllModuleExport(12)]
	private uint GetShortPathNameA(in LpcStr lpszLongPath, in LpStr lpszShortPath, uint cchBuffer)
	{
		var longPath = lpszLongPath.ToString() ?? string.Empty;
		_logger.LogInformation("[Kernel32] GetShortPathNameA(lpszLongPath=\"{LongPath}\", cchBuffer={CchBuffer})", longPath, cchBuffer);

		// Stub - just copy the long path as the short path
		if (cchBuffer > 0 && lpszShortPath.Address != 0)
		{
			string toCopy;
			if (cchBuffer > 1)
			{
				toCopy = longPath.Length < cchBuffer - 1 ? longPath : longPath.Substring(0, (int)cchBuffer - 1);
			}
			else
			{
				toCopy = string.Empty;
			}
			lpszShortPath.Write(_env.Memory, toCopy, true);
			return (uint)toCopy.Length + 1;
		}

		return (uint)longPath.Length + 1;
	}

	[DllModuleExport(20)]
	private uint GetStringTypeExA(uint Locale, uint dwInfoType, in LpcStr lpSrcStr, int cchSrc, uint lpCharType)
	{
		var srcStr = lpSrcStr.ToString() ?? string.Empty;
		_logger.LogInformation("[Kernel32] GetStringTypeExA(Locale=0x{Locale:X}, dwInfoType=0x{DwInfoType:X}, cchSrc={CchSrc})",
			Locale, dwInfoType, cchSrc);

		// Stub - just set all character types to 0
		if (lpCharType != 0)
		{
			int count = cchSrc == -1 ? srcStr.Length : cchSrc;
			for (int i = 0; i < count; i++)
			{
				_env.MemWrite16(lpCharType + (uint)(i * 2), 0);
			}
		}

		return 1; // TRUE
	}

	[DllModuleExport(16)]
	private uint GetTempFileNameA(in LpcStr lpPathName, in LpcStr lpPrefixString, uint uUnique, in LpStr lpTempFileName)
	{
		var pathName = lpPathName.ToString() ?? string.Empty;
		var prefix = lpPrefixString.ToString() ?? string.Empty;

		_logger.LogInformation("[Kernel32] GetTempFileNameA(lpPathName=\"{PathName}\", lpPrefixString=\"{Prefix}\", uUnique={UUnique})",
			pathName, prefix, uUnique);

		// Generate a temporary file name
		var uniqueNum = uUnique != 0 ? uUnique : (uint)Random.Shared.Next(0x1, 0xFFFF);
		var tempFileName = $"{prefix}{uniqueNum:X4}.TMP";

		if (lpTempFileName.Address != 0)
		{
			lpTempFileName.Write(_env.Memory, tempFileName, true);
		}

		return uniqueNum;
	}

	[DllModuleExport(0)]
	private uint GetThreadLocale()
	{
		_logger.LogInformation("[Kernel32] GetThreadLocale()");
		// Return English (United States) locale
		return 0x0409; // MAKELCID(MAKELANGID(LANG_ENGLISH, SUBLANG_ENGLISH_US), SORT_DEFAULT)
	}

	[DllModuleExport(0)]
	private uint GetUserDefaultLangID()
	{
		_logger.LogInformation("[Kernel32] GetUserDefaultLangID()");
		// Return English (United States) language ID
		// LANGID is a 16-bit value: MAKELANGID(LANG_ENGLISH, SUBLANG_ENGLISH_US)
		return 0x0409; // English (United States)
	}

	/// <summary>
	/// Returns the language identifier for the system locale.
	/// LANGID GetSystemDefaultLangID(void);
	/// </summary>
	[DllModuleExport(0)]
	private uint GetSystemDefaultLangID()
	{
		_logger.LogInformation("[Kernel32] GetSystemDefaultLangID()");
		// Return English (United States) language ID
		// LANGID is a 16-bit value: MAKELANGID(LANG_ENGLISH, SUBLANG_ENGLISH_US)
		return 0x0409; // English (United States)
	}

	[DllModuleExport(8)]
	private uint LocalFileTimeToFileTime(uint lpLocalFileTime, uint lpFileTime)
	{
		_logger.LogInformation("[Kernel32] LocalFileTimeToFileTime(lpLocalFileTime=0x{LpLocalFileTime:X8}, lpFileTime=0x{LpFileTime:X8})",
			lpLocalFileTime, lpFileTime);

		// Stub - just copy the time
		if (lpLocalFileTime != 0 && lpFileTime != 0)
		{
			var localTime = new FileTimeRef(_env.Memory, lpLocalFileTime);
			var fileTime = new FileTimeRef(_env.Memory, lpFileTime);
			fileTime.dwLowDateTime = localTime.dwLowDateTime;
			fileTime.dwHighDateTime = localTime.dwHighDateTime;
		}

		return 1; // TRUE
	}

	[DllModuleExport(16)]
	private uint SetFileTime(uint hFile, uint lpCreationTime, uint lpLastAccessTime, uint lpLastWriteTime)
	{
		_logger.LogInformation("[Kernel32] SetFileTime(hFile=0x{HFile:X8}, lpCreationTime=0x{LpCreationTime:X8}, lpLastAccessTime=0x{LpLastAccessTime:X8}, lpLastWriteTime=0x{LpLastWriteTime:X8})",
			hFile, lpCreationTime, lpLastAccessTime, lpLastWriteTime);

		// Stub - return success
		return 1; // TRUE
	}

	[DllModuleExport(8)]
	private uint SystemTimeToFileTime(uint lpSystemTime, uint lpFileTime)
	{
		_logger.LogInformation("[Kernel32] SystemTimeToFileTime(lpSystemTime=0x{LpSystemTime:X8}, lpFileTime=0x{LpFileTime:X8})",
			lpSystemTime, lpFileTime);

		// Stub - write dummy FILETIME value
		if (lpFileTime != 0)
		{
			_env.MemWrite32(lpFileTime, 0);     // dwLowDateTime
			_env.MemWrite32(lpFileTime + 4, 0); // dwHighDateTime
		}

		return 1; // TRUE
	}

	[DllModuleExport(4)]
	private uint GlobalCompact(uint dwMinFree)
	{
		_logger.LogInformation("[Kernel32] GlobalCompact(dwMinFree={DwMinFree})", dwMinFree);
		// Return a reasonable amount of free memory (stub)
		return 0x10000000; // 256 MB
	}

	[DllModuleExport(12)]
	private uint GetProfileIntA(in LpcStr lpAppName, in LpcStr lpKeyName, int nDefault)
	{
		var appName = lpAppName.ToString() ?? string.Empty;
		var keyName = lpKeyName.ToString() ?? string.Empty;
		_logger.LogInformation("[Kernel32] GetProfileIntA(lpAppName=\"{AppName}\", lpKeyName=\"{KeyName}\", nDefault={NDefault})",
			appName, keyName, nDefault);
		// Return default value (stub - no Win.ini support)
		return (uint)nDefault;
	}

	[DllModuleExport(20)]
	private uint GetProfileStringA(in LpcStr lpAppName, in LpcStr lpKeyName, in LpcStr lpDefault, uint lpReturnedString, uint nSize)
	{
		var appName = lpAppName.ToString() ?? string.Empty;
		var keyName = lpKeyName.ToString() ?? string.Empty;
		var defaultStr = lpDefault.ToString() ?? string.Empty;
		_logger.LogInformation("[Kernel32] GetProfileStringA(lpAppName=\"{AppName}\", lpKeyName=\"{KeyName}\", lpDefault=\"{Default}\", lpReturnedString=0x{LpReturnedString:X8}, nSize={NSize})",
			appName, keyName, defaultStr, lpReturnedString, nSize);

		// Return default string (stub - no Win.ini support)
		if (lpReturnedString != 0 && nSize > 0)
		{
			int charsToWrite = Math.Min(defaultStr.Length, (int)nSize - 1);
			string truncated = charsToWrite > 0 ? defaultStr.Substring(0, charsToWrite) : string.Empty;
			_env.WriteAnsiStringAt(lpReturnedString, truncated);
			return (uint)charsToWrite;
		}
		return 0;
	}

	/// <summary>
	/// Closes a file opened with _lopen or _lcreat (legacy 16-bit API).
	/// HFILE _lclose(
	///   [in] HFILE hFile
	/// );
	/// </summary>
	[DllModuleExport(4)]
	private unsafe uint _lclose(uint hFile)
	{
		_logger.LogInformation("[Kernel32] _lclose(hFile=0x{HFile:X8})", hFile);

		// _lclose is a legacy 16-bit API for closing files
		// It maps to CloseHandle in Win32
		// Return 0 for success, -1 (0xFFFFFFFF) for error

		return CloseHandle((void*)hFile) != 0 ? 0u : 0xFFFFFFFF;
	}

	/// <summary>
	/// Creates a new file or rewrites an existing file (legacy 16-bit API).
	/// HFILE _lcreat(
	///   [in] LPCSTR lpPathName,
	///   [in] int    iAttribute
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint _lcreat(in LpcStr lpPathName, int iAttribute)
	{
		var pathName = lpPathName.ToString() ?? string.Empty;
		_logger.LogInformation("[Kernel32] _lcreat(lpPathName=\"{PathName}\", iAttribute={IAttribute})",
			pathName, iAttribute);

		// _lcreat is a legacy 16-bit API for creating/rewriting files
		// iAttribute can be 0 (normal), 1 (read-only), 2 (hidden), 4 (system)
		// Maps to CreateFile with CREATE_ALWAYS disposition

		// For simplicity, use the VFS or return a dummy handle
		// In a real implementation, this would create the file

		if (string.IsNullOrEmpty(pathName))
		{
			_logger.LogWarning("[Kernel32] _lcreat: NULL or empty path");
			return 0xFFFFFFFF; // HFILE_ERROR
		}

		// Return a dummy file handle
		// A full implementation would actually create the file
		var handle = 0x4000u + ((uint)pathName.GetHashCode() & 0xFFF);
		_logger.LogInformation("[Kernel32] _lcreat: Created file handle 0x{Handle:X8}", handle);
		return handle;
	}

	/// <summary>
	/// Writes to a file (legacy 16-bit API).
	/// UINT _lwrite(
	///   [in] HFILE  hFile,
	///   [in] LPCSTR lpBuffer,
	///   [in] UINT   uBytes
	/// );
	/// </summary>
	[DllModuleExport(12)]
	private uint _lwrite(uint hFile, uint lpBuffer, uint uBytes)
	{
		_logger.LogInformation("[Kernel32] _lwrite(hFile=0x{HFile:X8}, lpBuffer=0x{LpBuffer:X8}, uBytes={UBytes})",
			hFile, lpBuffer, uBytes);

		// _lwrite is a legacy 16-bit API for writing to files
		// Returns the number of bytes written, or 0xFFFFFFFF on error

		if (lpBuffer == 0)
		{
			_logger.LogWarning("[Kernel32] _lwrite: NULL buffer");
			return 0xFFFFFFFF; // HFILE_ERROR
		}

		// For stub implementation, just return the number of bytes written
		// A full implementation would actually write the data
		_logger.LogInformation("[Kernel32] _lwrite: Stub returning {UBytes} bytes written", uBytes);
		return uBytes;
	}

	/// <summary>
	/// Opens an existing file (legacy 16-bit API).
	/// HFILE _lopen(
	///   [in] LPCSTR lpPathName,
	///   [in] int    iReadWrite
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint _lopen(in LpcStr lpPathName, int iReadWrite)
	{
		var pathName = lpPathName.ToString() ?? string.Empty;
		_logger.LogInformation("[Kernel32] _lopen(lpPathName=\"{PathName}\", iReadWrite={IReadWrite})",
			pathName, iReadWrite);

		// _lopen is a legacy 16-bit API for opening files
		// iReadWrite can be:
		// OF_READ (0x0000) - Open for reading
		// OF_WRITE (0x0001) - Open for writing
		// OF_READWRITE (0x0002) - Open for reading and writing
		// Plus various flags like OF_SHARE_DENY_NONE, etc.

		if (string.IsNullOrEmpty(pathName))
		{
			_logger.LogWarning("[Kernel32] _lopen: NULL or empty path");
			return 0xFFFFFFFF; // HFILE_ERROR
		}

		// Try to use VirtualFileSystem if available
		if (_env.VirtualFileSystem != null)
		{
			try
			{
				if (_env.VirtualFileSystem.FileExists(pathName))
				{
					// Return a dummy file handle
					var handle = 0x4000u + ((uint)pathName.GetHashCode() & 0xFFF);
					_logger.LogInformation("[Kernel32] _lopen: Opened file handle 0x{Handle:X8}", handle);
					return handle;
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "[Kernel32] _lopen: VFS error accessing \"{PathName}\"", pathName);
			}
		}

		// File not found or VFS not available - return a dummy handle for compatibility
		// This allows legacy apps to continue even if the file doesn't exist
		var dummyHandle = 0x4000u + ((uint)pathName.GetHashCode() & 0xFFF);
		_logger.LogInformation("[Kernel32] _lopen: Created dummy file handle 0x{Handle:X8}", dummyHandle);
		return dummyHandle;
	}

	/// <summary>
	/// Reads from a file (legacy 16-bit API).
	/// UINT _lread(
	///   [in]  HFILE  hFile,
	///   [out] LPVOID lpBuffer,
	///   [in]  UINT   uBytes
	/// );
	/// </summary>
	[DllModuleExport(12)]
	private uint _lread(uint hFile, uint lpBuffer, uint uBytes)
	{
		_logger.LogInformation("[Kernel32] _lread(hFile=0x{HFile:X8}, lpBuffer=0x{LpBuffer:X8}, uBytes={UBytes})",
			hFile, lpBuffer, uBytes);

		// _lread is a legacy 16-bit API for reading from files
		// Returns the number of bytes read, or 0xFFFFFFFF on error

		if (lpBuffer == 0)
		{
			_logger.LogWarning("[Kernel32] _lread: NULL buffer");
			return 0xFFFFFFFF; // HFILE_ERROR
		}

		// For stub implementation, zero out the buffer and return 0 bytes read
		// A full implementation would actually read from the file
		for (uint i = 0; i < uBytes; i++)
		{
			_env.MemWrite8(lpBuffer + i, 0);
		}

		_logger.LogInformation("[Kernel32] _lread: Stub returning 0 bytes read (EOF)");
		return 0; // EOF
	}

	/// <summary>
	/// Repositions the file pointer in a file (legacy 16-bit API).
	/// LONG _llseek(
	///   [in] HFILE hFile,
	///   [in] LONG  lOffset,
	///   [in] int   iOrigin
	/// );
	/// </summary>
	[DllModuleExport(12)]
	private uint _llseek(uint hFile, int lOffset, int iOrigin)
	{
		_logger.LogInformation("[Kernel32] _llseek(hFile=0x{HFile:X8}, lOffset={LOffset}, iOrigin={IOrigin})",
			hFile, lOffset, iOrigin);

		// _llseek is a legacy 16-bit API for seeking in files
		// iOrigin can be:
		// FILE_BEGIN (0) - Seek from beginning
		// FILE_CURRENT (1) - Seek from current position
		// FILE_END (2) - Seek from end
		// Returns the new file pointer position, or 0xFFFFFFFF on error

		// For stub implementation, calculate position based on origin
		// A full implementation would track actual file positions and sizes
		uint newPosition;
		switch (iOrigin)
		{
			case 0: // FILE_BEGIN
				newPosition = (uint)Math.Max(0, lOffset);
				break;
			case 1: // FILE_CURRENT
					// Would need to track current position - for now assume position 0
				newPosition = (uint)Math.Max(0, lOffset);
				break;
			case 2: // FILE_END
					// Would need to know file size - for now just use offset
				newPosition = (uint)Math.Max(0, lOffset);
				break;
			default:
				_logger.LogWarning("[Kernel32] _llseek: Invalid origin {IOrigin}", iOrigin);
				return 0xFFFFFFFF; // Error
		}

		_logger.LogInformation("[Kernel32] _llseek: Stub returning position 0x{NewPosition:X8}", newPosition);
		return newPosition;
	}

	[DllModuleExport(12)]
	private uint WriteProfileStringA(in LpcStr lpAppName, in LpcStr lpKeyName, in LpcStr lpString)
	{
		var appName = lpAppName.ToString() ?? string.Empty;
		var keyName = lpKeyName.ToString() ?? string.Empty;
		var str = lpString.ToString() ?? string.Empty;
		_logger.LogInformation("[Kernel32] WriteProfileStringA(lpAppName=\"{AppName}\", lpKeyName=\"{KeyName}\", lpString=\"{Str}\")",
			appName, keyName, str);
		// Return success (stub - no Win.ini support)
		return 1; // TRUE
	}

	/// <summary>
	/// Creates or opens a named or unnamed file mapping object for a specified file.
	/// HANDLE CreateFileMappingA(
	///   [in]           HANDLE                hFile,
	///   [in, optional] LPSECURITY_ATTRIBUTES lpFileMappingAttributes,
	///   [in]           DWORD                 flProtect,
	///   [in]           DWORD                 dwMaximumSizeHigh,
	///   [in]           DWORD                 dwMaximumSizeLow,
	///   [in, optional] LPCSTR                lpName
	/// );
	/// </summary>
	[DllModuleExport(24)]
	private uint CreateFileMappingA(uint hFile, uint lpFileMappingAttributes, uint flProtect,
		uint dwMaximumSizeHigh, uint dwMaximumSizeLow, in LpcStr lpName)
	{
		var name = lpName.ToString() ?? string.Empty;
		_logger.LogInformation("[Kernel32] CreateFileMappingA(hFile=0x{HFile:X8}, flProtect=0x{FlProtect:X}, dwMaximumSizeHigh={DwMaximumSizeHigh}, dwMaximumSizeLow={DwMaximumSizeLow}, lpName=\"{Name}\")",
			hFile, flProtect, dwMaximumSizeHigh, dwMaximumSizeLow, name);

		// File mapping objects allow multiple processes to access the same file or memory
		// For now, return a dummy handle
		// A full implementation would:
		// 1. Track the mapping in memory management
		// 2. Associate it with the file handle
		// 3. Support both file-backed and page file-backed mappings

		// Return a dummy file mapping handle (non-zero for success)
		return FILE_MAPPING_HANDLE_BASE + ((uint)name.GetHashCode() & 0x7FFFFFFF);
	}

	/// <summary>
	/// Creates or opens a named or unnamed file mapping object for a specified file (Unicode version).
	/// </summary>
	[DllModuleExport(24)]
	private uint CreateFileMappingW(uint hFile, uint lpFileMappingAttributes, uint flProtect,
		uint dwMaximumSizeHigh, uint dwMaximumSizeLow, in LpcWStr lpName)
	{
		var name = lpName.Read(_env.Memory) ?? string.Empty;
		_logger.LogInformation("[Kernel32] CreateFileMappingW(hFile=0x{HFile:X8}, flProtect=0x{FlProtect:X}, dwMaximumSizeHigh={DwMaximumSizeHigh}, dwMaximumSizeLow={DwMaximumSizeLow}, lpName=\"{Name}\")",
			hFile, flProtect, dwMaximumSizeHigh, dwMaximumSizeLow, name);

		// Return a dummy file mapping handle (non-zero for success)
		return FILE_MAPPING_HANDLE_BASE + ((uint)name.GetHashCode() & 0x7FFFFFFF);
	}

	/// <summary>
	/// Opens a named file mapping object (ANSI version).
	/// </summary>
	[DllModuleExport(20, IsStub = true)]
	private uint OpenFileMappingA(uint dwDesiredAccess, uint bInheritHandle, in LpcStr lpName)
	{
		var name = lpName.ToString() ?? string.Empty;
		_logger.LogInformation("[Kernel32] OpenFileMappingA(dwDesiredAccess=0x{Access:X8}, bInheritHandle={Inherit}, lpName=\"{Name}\")",
			dwDesiredAccess, bInheritHandle, name);

		// Stub implementation - return a dummy handle if name is not empty
		if (string.IsNullOrEmpty(name))
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}

		// Return a dummy file mapping handle (non-zero for success)
		return FILE_MAPPING_HANDLE_BASE + ((uint)name.GetHashCode() & 0x7FFFFFFF);
	}

	/// <summary>
	/// Opens a named file mapping object (Unicode version).
	/// </summary>
	[DllModuleExport(20, IsStub = true)]
	private uint OpenFileMappingW(uint dwDesiredAccess, uint bInheritHandle, in LpcWStr lpName)
	{
		var name = lpName.Read(_env.Memory) ?? string.Empty;
		_logger.LogInformation("[Kernel32] OpenFileMappingW(dwDesiredAccess=0x{Access:X8}, bInheritHandle={Inherit}, lpName=\"{Name}\")",
			dwDesiredAccess, bInheritHandle, name);

		// Stub implementation - return a dummy handle if name is not empty
		if (string.IsNullOrEmpty(name))
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}

		// Return a dummy file mapping handle (non-zero for success)
		return FILE_MAPPING_HANDLE_BASE + ((uint)name.GetHashCode() & 0x7FFFFFFF);
	}

	/// <summary>
	/// Maps a view of a file mapping into the address space of a calling process.
	/// LPVOID MapViewOfFile(
	///   [in] HANDLE hFileMappingObject,
	///   [in] DWORD  dwDesiredAccess,
	///   [in] DWORD  dwFileOffsetHigh,
	///   [in] DWORD  dwFileOffsetLow,
	///   [in] SIZE_T dwNumberOfBytesToMap
	/// );
	/// </summary>
	[DllModuleExport(20)]
	private uint MapViewOfFile(uint hFileMappingObject, uint dwDesiredAccess,
		uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap)
	{
		_logger.LogInformation("[Kernel32] MapViewOfFile(hFileMappingObject=0x{HFileMappingObject:X8}, dwDesiredAccess=0x{DwDesiredAccess:X}, dwFileOffsetHigh={DwFileOffsetHigh}, dwFileOffsetLow={DwFileOffsetLow}, dwNumberOfBytesToMap={DwNumberOfBytesToMap})",
			hFileMappingObject, dwDesiredAccess, dwFileOffsetHigh, dwFileOffsetLow, dwNumberOfBytesToMap);

		// MapViewOfFile creates a view of the file mapping in the process's address space
		// For a stub implementation:
		// 1. Allocate memory for the view
		// 2. Return the address

		if (dwNumberOfBytesToMap == 0)
		{
			// If size is 0, map the entire file (we'll use a default size)
			dwNumberOfBytesToMap = 0x10000; // 64KB default
		}

		// Allocate memory for the view
		var baseAddress = VirtualAlloc(0, dwNumberOfBytesToMap, 0x1000 | 0x2000, 0x04); // MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE

		_logger.LogInformation("[Kernel32] MapViewOfFile: Allocated view at 0x{BaseAddress:X8}", baseAddress);
		return baseAddress;
	}

	/// <summary>
	/// Unmaps a mapped view of a file from the calling process's address space.
	/// BOOL UnmapViewOfFile(
	///   [in] LPCVOID lpBaseAddress
	/// );
	/// </summary>
	[DllModuleExport(4)]
	private uint UnmapViewOfFile(uint lpBaseAddress)
	{
		_logger.LogInformation("[Kernel32] UnmapViewOfFile(lpBaseAddress=0x{LpBaseAddress:X8})", lpBaseAddress);

		// UnmapViewOfFile unmaps a view created by MapViewOfFile
		// For a stub implementation, we just log it
		// A full implementation would:
		// 1. Free the memory allocated for the view
		// 2. Update internal tracking structures

		return 1; // TRUE (success)
	}

	/// <summary>
	/// Changes the protection on a region of committed pages in the virtual address space.
	/// BOOL VirtualProtect(
	///   [in]  LPVOID lpAddress,
	///   [in]  SIZE_T dwSize,
	///   [in]  DWORD  flNewProtect,
	///   [out] PDWORD lpflOldProtect
	/// );
	/// </summary>
	[DllModuleExport(16)]
	private uint VirtualProtect(uint lpAddress, uint dwSize, uint flNewProtect, uint lpflOldProtect)
	{
		_logger.LogInformation("[Kernel32] VirtualProtect(lpAddress=0x{LpAddress:X8}, dwSize={DwSize}, flNewProtect=0x{FlNewProtect:X}, lpflOldProtect=0x{LpflOldProtect:X8})",
			lpAddress, dwSize, flNewProtect, lpflOldProtect);

		// VirtualProtect changes memory protection attributes
		// Common protection values:
		// PAGE_NOACCESS = 0x01, PAGE_READONLY = 0x02, PAGE_READWRITE = 0x04
		// PAGE_EXECUTE = 0x10, PAGE_EXECUTE_READ = 0x20, PAGE_EXECUTE_READWRITE = 0x40

		if (lpflOldProtect != 0)
		{
			// Return the old protection (assume it was PAGE_READWRITE)
			_env.MemWrite32(lpflOldProtect, 0x04); // PAGE_READWRITE
		}

		// For emulation, we don't enforce memory protection
		// But we log the request for debugging purposes
		_logger.LogInformation("[Kernel32] VirtualProtect: Changed protection at 0x{LpAddress:X8} to 0x{FlNewProtect:X}",
			lpAddress, flNewProtect);

		return 1; // TRUE (success)
	}

	/// <summary>
	/// Provides information about a range of pages in the virtual address space.
	/// SIZE_T VirtualQuery(
	///   [in, optional] LPCVOID                   lpAddress,
	///   [out]          PMEMORY_BASIC_INFORMATION lpBuffer,
	///   [in]           SIZE_T                    dwLength
	/// );
	/// </summary>
	[DllModuleExport(12)]
	private uint VirtualQuery(uint lpAddress, uint lpBuffer, uint dwLength)
	{
		_logger.LogInformation("[Kernel32] VirtualQuery(lpAddress=0x{LpAddress:X8}, lpBuffer=0x{LpBuffer:X8}, dwLength={DwLength})",
			lpAddress, lpBuffer, dwLength);

		// VirtualQuery returns information about a memory region
		// MEMORY_BASIC_INFORMATION structure (32-bit):
		// PVOID  BaseAddress;        // 0
		// PVOID  AllocationBase;     // 4
		// DWORD  AllocationProtect;  // 8
		// SIZE_T RegionSize;         // 12
		// DWORD  State;              // 16
		// DWORD  Protect;            // 20
		// DWORD  Type;               // 24
		// Total size: 28 bytes

		if (lpBuffer == 0 || dwLength < 28)
		{
			_logger.LogWarning("[Kernel32] VirtualQuery: Invalid buffer or size");
			return 0; // Return 0 on error
		}

		// Fill in dummy memory information
		_env.MemWrite32(lpBuffer + 0, lpAddress);      // BaseAddress
		_env.MemWrite32(lpBuffer + 4, lpAddress);      // AllocationBase
		_env.MemWrite32(lpBuffer + 8, 0x04);           // AllocationProtect (PAGE_READWRITE)
		_env.MemWrite32(lpBuffer + 12, 0x10000);       // RegionSize (64KB)
		_env.MemWrite32(lpBuffer + 16, 0x1000);        // State (MEM_COMMIT)
		_env.MemWrite32(lpBuffer + 20, 0x04);          // Protect (PAGE_READWRITE)
		_env.MemWrite32(lpBuffer + 24, 0x20000);       // Type (MEM_PRIVATE)

		return 28; // Return size of structure written
	}

	/// <summary>
	/// Enumerates the locales that are either installed on or supported by an operating system.
	/// BOOL EnumSystemLocalesA(
	///   [in] LOCALE_ENUMPROCA lpLocaleEnumProc,
	///   [in] DWORD            dwFlags
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint EnumSystemLocalesA(uint lpLocaleEnumProc, uint dwFlags)
	{
		_logger.LogInformation("[Kernel32] EnumSystemLocalesA(lpLocaleEnumProc=0x{LpLocaleEnumProc:X8}, dwFlags=0x{DwFlags:X})",
			lpLocaleEnumProc, dwFlags);

		// EnumSystemLocalesA enumerates system locales by calling a callback for each
		// dwFlags can be LCID_INSTALLED (0x01) or LCID_SUPPORTED (0x02)

		if (lpLocaleEnumProc == 0)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0; // FALSE
		}

		// For a stub implementation, we'll enumerate a few common locales
		// Real implementation would call the callback for each locale
		// Callback signature: BOOL CALLBACK EnumLocalesProc(LPSTR lpLocaleString)

		string[] locales =
		[
			"00000409", // English (United States)
			"00000809", // English (United Kingdom)
			"0000040c", // French (France)
			"00000407", // German (Germany)
		];

		foreach (var locale in locales)
		{
			// Allocate memory for locale string
			var localeAddr = _env.HeapAlloc(0, (uint)(locale.Length + 1));
			_env.WriteAnsiStringAt(localeAddr, locale);

			// Call the callback
			// Callback returns TRUE to continue enumeration, FALSE to stop
			// For stub, we just log it instead of calling
			_logger.LogInformation("[Kernel32] EnumSystemLocalesA: Would enumerate locale \"{Locale}\"", locale);

			// Free the allocated memory
			// _env.HeapFree(0, 0, localeAddr);
		}

		return 1; // TRUE (success)
	}

	/// <summary>
	/// Converts a file time to MS-DOS date and time values.
	/// BOOL FileTimeToDosDateTime(
	///   [in]  const FILETIME *lpFileTime,
	///   [out] LPWORD         lpFatDate,
	///   [out] LPWORD         lpFatTime
	/// );
	/// </summary>
	[DllModuleExport(12)]
	private uint FileTimeToDosDateTime(uint lpFileTime, uint lpFatDate, uint lpFatTime)
	{
		_logger.LogInformation("[Kernel32] FileTimeToDosDateTime(lpFileTime=0x{LpFileTime:X8}, lpFatDate=0x{LpFatDate:X8}, lpFatTime=0x{LpFatTime:X8})",
			lpFileTime, lpFatDate, lpFatTime);

		if (lpFileTime == 0 || lpFatDate == 0 || lpFatTime == 0)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0; // FALSE
		}

		// Read FILETIME structure
		var fileTime = new FileTimeRef(_env.Memory, lpFileTime);

		// For stub, convert to a default DOS date/time
		// DOS date format (16-bit):
		// Bits 0-4: Day (1-31)
		// Bits 5-8: Month (1-12)
		// Bits 9-15: Year (0 = 1980, 127 = 2107)
		// DOS time format (16-bit):
		// Bits 0-4: Seconds/2 (0-29)
		// Bits 5-10: Minutes (0-59)
		// Bits 11-15: Hours (0-23)

		// Use current time as default (January 1, 2000, 12:00:00)
		ushort dosDate = (ushort)((20 << 9) | (1 << 5) | 1);  // Year 2000, Month 1, Day 1
		ushort dosTime = (ushort)((12 << 11) | (0 << 5) | 0); // 12:00:00

		_env.MemWrite16(lpFatDate, dosDate);
		_env.MemWrite16(lpFatTime, dosTime);

		return 1; // TRUE (success)
	}


	/// <summary>
	/// Reads memory from a specified process.
	/// </summary>
	[DllModuleExport(0)]
	private uint ReadProcessMemory(uint hProcess, uint lpBaseAddress, uint lpBuffer, uint nSize, uint lpNumberOfBytesRead)
	{
		_logger.LogInformation("[Kernel32] ReadProcessMemory(hProcess=0x{HProcess:X8}, lpBaseAddress=0x{LpBaseAddress:X8}, lpBuffer=0x{LpBuffer:X8}, nSize={NSize}, lpNumberOfBytesRead=0x{LpNumberOfBytesRead:X8})",
		hProcess, lpBaseAddress, lpBuffer, nSize, lpNumberOfBytesRead);

		// For self-process (pseudo-handle or current process), just copy memory
		if (hProcess == 0xFFFFFFFF || hProcess == GetCurrentProcess())
		{
			try
			{
				// Copy memory from source to destination
				for (uint i = 0; i < nSize; i++)
				{
					var value = _env.MemRead8(lpBaseAddress + i);
					_env.MemWrite8(lpBuffer + i, value);
				}

				// Write the number of bytes read
				if (lpNumberOfBytesRead != 0)
				{
					_env.MemWrite32(lpNumberOfBytesRead, nSize);
				}

				return 1; // TRUE
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Kernel32] ReadProcessMemory: Exception occurred");
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
				if (lpNumberOfBytesRead != 0)
				{
					_env.MemWrite32(lpNumberOfBytesRead, 0);
				}
				return 0; // FALSE
			}
		}

		// For other processes, we don't support cross-process memory access in the emulator
		_logger.LogWarning("[Kernel32] ReadProcessMemory: Cross-process memory access not supported");
		_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
		if (lpNumberOfBytesRead != 0)
		{
			_env.MemWrite32(lpNumberOfBytesRead, 0);
		}
		return 0; // FALSE
	}

	/// <summary>
	/// Searches for a file in a specified path.
	/// </summary>
	[DllModuleExport(0)]
	private uint SearchPathA(in LpcStr lpPath, in LpcStr lpFileName, in LpcStr lpExtension, uint nBufferLength, in LpStr lpBuffer, uint lpFilePart)
	{
		var path = lpPath.ToString();
		var fileName = lpFileName.ToString();
		var extension = lpExtension.ToString();

		_logger.LogInformation("[Kernel32] SearchPathA(lpPath=\"{Path}\", lpFileName=\"{FileName}\", lpExtension=\"{Extension}\", nBufferLength={NBufferLength})",
		path ?? "(null)", fileName ?? "(null)", extension ?? "(null)", nBufferLength);

		if (string.IsNullOrEmpty(fileName))
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}

		// Build the complete filename with extension if provided
		var fullFileName = fileName;
		if (!string.IsNullOrEmpty(extension) && !fileName.Contains('.'))
		{
			fullFileName = fileName + extension;
		}

		// For stub, just return the filename in current directory
		var resultPath = System.IO.Path.Combine(_env.CurrentDirectory, fullFileName);
		var resultPathLength = (uint)(resultPath.Length + 1); // +1 for null terminator

		if (nBufferLength < resultPathLength)
		{
			_logger.LogInformation("[Kernel32] SearchPathA: Buffer too small, need {ResultPathLength} bytes", resultPathLength);
			return resultPathLength; // Return required size
		}

		// Write the path to the buffer
		lpBuffer.Write(_env.Memory, resultPath, true);

		// Write the file part pointer if requested
		if (lpFilePart != 0)
		{
			var lastSlash = resultPath.LastIndexOfAny(['\\', '/']);
			if (lastSlash >= 0)
			{
				var filePartOffset = (uint)(lastSlash + 1);
				_env.MemWrite32(lpFilePart, lpBuffer.Address + filePartOffset);
			}
			else
			{
				_env.MemWrite32(lpFilePart, lpBuffer.Address);
			}
		}

		_logger.LogInformation("[Kernel32] SearchPathA: Returning \"{ResultPath}\"", resultPath);
		return (uint)resultPath.Length; // Return length without null terminator
	}

	/// <summary>
	/// Sets the per-process mode that the SearchPath function uses when locating files.
	/// This affects the order in which directories are searched when resolving file paths.
	/// Safe search mode (enabled) searches system directories before the current directory,
	/// while disabled mode searches the current directory first (less secure).
	/// BOOL SetSearchPathMode(
	///   [in] DWORD Flags
	/// );
	/// </summary>
	[DllModuleExport(0)]
	private uint SetSearchPathMode(uint flags)
	{
		_logger.LogInformation("[Kernel32] SetSearchPathMode(flags=0x{Flags:X8})", flags);

		// Check if search path mode is already set to permanent
		if (_env.SearchPathModePermanent)
		{
			_logger.LogWarning("[Kernel32] SetSearchPathMode: Cannot change mode - already set to permanent");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_ACCESS_DENIED;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		// Extract the mode flags
		bool enableSafeMode = (flags & BASE_SEARCH_PATH_ENABLE_SAFE_SEARCHMODE) != 0;
		bool disableSafeMode = (flags & BASE_SEARCH_PATH_DISABLE_SAFE_SEARCHMODE) != 0;
		bool permanent = (flags & BASE_SEARCH_PATH_PERMANENT) != 0;

		// Validate flag combinations
		if (enableSafeMode && disableSafeMode)
		{
			_logger.LogWarning("[Kernel32] SetSearchPathMode: Invalid parameter - both enable and disable flags set");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		// Cannot set permanent with disable flag
		if (permanent && disableSafeMode)
		{
			_logger.LogWarning("[Kernel32] SetSearchPathMode: Invalid parameter - permanent flag cannot be combined with disable flag");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		// Must specify either enable or disable
		if (!enableSafeMode && !disableSafeMode)
		{
			_logger.LogWarning("[Kernel32] SetSearchPathMode: Invalid parameter - must specify enable or disable flag");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		// Set the search path mode
		if (enableSafeMode)
		{
			_env.SearchPathMode = BASE_SEARCH_PATH_ENABLE_SAFE_SEARCHMODE;
			_logger.LogInformation("[Kernel32] SetSearchPathMode: Safe search mode enabled");
		}
		else // disableSafeMode
		{
			_env.SearchPathMode = BASE_SEARCH_PATH_DISABLE_SAFE_SEARCHMODE;
			_logger.LogInformation("[Kernel32] SetSearchPathMode: Safe search mode disabled");
		}

		// Set permanent flag if specified
		if (permanent)
		{
			_env.SearchPathModePermanent = true;
			_logger.LogInformation("[Kernel32] SetSearchPathMode: Mode set to permanent");
		}

		return (uint)NativeTypes.Win32Bool.TRUE;
	}

	/// <summary>
	/// Takes a snapshot of the specified processes, as well as the heaps, modules, and threads used by these processes.
	/// HANDLE CreateToolhelp32Snapshot(
	///   [in] DWORD dwFlags,
	///   [in] DWORD th32ProcessID
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID)
	{
		_logger.LogInformation("[Kernel32] CreateToolhelp32Snapshot(dwFlags=0x{DwFlags:X8}, th32ProcessID={Th32ProcessID})",
			dwFlags, th32ProcessID);

		// Parse the snapshot flags
		var flags = (NativeTypes.Th32SnapshotFlags)dwFlags;
		_logger.LogDebug("[Kernel32] Snapshot flags: {Flags}", flags);

		// For stub implementation, return a dummy snapshot handle
		// A full implementation would:
		// 1. Capture the current system state based on dwFlags
		// 2. Store the snapshot data for later retrieval
		// 3. Return a handle to the snapshot

		// Return a dummy snapshot handle (non-zero for success)
		return TH32_SNAPSHOT_HANDLE;
	}

	/// <summary>
	/// Retrieves information about the first process encountered in a system snapshot.
	/// BOOL Process32First(
	///   [in]      HANDLE           hSnapshot,
	///   [in, out] LPPROCESSENTRY32 lppe
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint Process32First(uint hSnapshot, uint lppe)
	{
		_logger.LogInformation("[Kernel32] Process32First(hSnapshot=0x{HSnapshot:X8}, lppe=0x{Lppe:X8})",
			hSnapshot, lppe);

		if (lppe == 0)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0; // FALSE
		}

		var dwSize = _env.MemRead32(lppe);
		if (dwSize < NativeTypes.PROCESSENTRY32.Size)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
			return 0; // FALSE
		}

		// Create ref wrapper to read/write the structure
		var entry = new ProcessEntry32Ref(_env.Memory, lppe);

		// For stub, return information about the current process
		entry.cntUsage = 0;
		entry.th32ProcessID = GetCurrentProcessId();
		entry.th32DefaultHeapID = 0;
		entry.th32ModuleID = 0;
		entry.cntThreads = 1;
		entry.th32ParentProcessID = 0;
		entry.pcPriClassBase = 8; // NORMAL_PRIORITY_CLASS
		entry.dwFlags = 0;
		entry.szExeFile = System.IO.Path.GetFileName(_env.ExecutablePath);

		// Log the filled structure
		_logger.LogDebug("[Kernel32] Process32First: Returned {Entry}", entry.ToString());

		// No more processes after this one (stub)
		return 1; // TRUE
	}

	/// <summary>
	/// Retrieves information about the next process recorded in a system snapshot.
	/// BOOL Process32Next(
	///   [in]  HANDLE           hSnapshot,
	///   [out] LPPROCESSENTRY32 lppe
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint Process32Next(uint hSnapshot, uint lppe)
	{
		_logger.LogInformation("[Kernel32] Process32Next(hSnapshot=0x{HSnapshot:X8}, lppe=0x{Lppe:X8})",
			hSnapshot, lppe);

		// For stub implementation, there are no more processes
		// Return FALSE to indicate end of snapshot
		_lastError = (uint)NativeTypes.Win32Error.ERROR_NO_MORE_FILES;
		return 0; // FALSE
	}

	/// <summary>
	/// Retrieves information about the first thread of any process encountered in a system snapshot.
	/// BOOL Thread32First(
	///   [in]  HANDLE          hSnapshot,
	///   [out] LPTHREADENTRY32 lpte
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint Thread32First(uint hSnapshot, uint lpte)
	{
		_logger.LogInformation("[Kernel32] Thread32First(hSnapshot=0x{HSnapshot:X8}, lpte=0x{Lpte:X8})",
			hSnapshot, lpte);

		if (lpte == 0)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0; // FALSE
		}

		var dwSize = _env.MemRead32(lpte);
		if (dwSize < NativeTypes.THREADENTRY32.Size)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
			return 0; // FALSE
		}

		// Create ref wrapper to read/write the structure
		var entry = new ThreadEntry32Ref(_env.Memory, lpte);

		// For stub, return information about the main thread
		entry.cntUsage = 0;
		entry.th32ThreadID = GetCurrentThreadId();
		entry.th32OwnerProcessID = GetCurrentProcessId();
		entry.tpBasePri = 8; // THREAD_PRIORITY_NORMAL
		entry.tpDeltaPri = 0;
		entry.dwFlags = 0;

		// Log the filled structure
		_logger.LogDebug("[Kernel32] Thread32First: Returned {Entry}", entry.ToString());

		return 1; // TRUE
	}

	/// <summary>
	/// Retrieves information about the next thread of any process encountered in the system memory snapshot.
	/// BOOL Thread32Next(
	///   [in]  HANDLE          hSnapshot,
	///   [out] LPTHREADENTRY32 lpte
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint Thread32Next(uint hSnapshot, uint lpte)
	{
		_logger.LogInformation("[Kernel32] Thread32Next(hSnapshot=0x{HSnapshot:X8}, lpte=0x{Lpte:X8})",
			hSnapshot, lpte);

		// For stub implementation, there are no more threads
		_lastError = (uint)NativeTypes.Win32Error.ERROR_NO_MORE_FILES;
		return 0; // FALSE
	}

	/// <summary>
	/// Retrieves information about the first module associated with a process.
	/// BOOL Module32First(
	///   [in]  HANDLE           hSnapshot,
	///   [out] LPMODULEENTRY32 lpme
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint Module32First(uint hSnapshot, uint lpme)
	{
		_logger.LogInformation("[Kernel32] Module32First(hSnapshot=0x{HSnapshot:X8}, lpme=0x{Lpme:X8})",
			hSnapshot, lpme);

		if (lpme == 0)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0; // FALSE
		}

		var dwSize = _env.MemRead32(lpme);
		if (dwSize < NativeTypes.MODULEENTRY32.Size)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
			return 0; // FALSE
		}

		// Create ref wrapper to read/write the structure
		var entry = new ModuleEntry32Ref(_env.Memory, lpme);

		// For stub, return information about the main module
		entry.th32ModuleID = 1;
		entry.th32ProcessID = GetCurrentProcessId();
		entry.GlblcntUsage = 0;
		entry.ProccntUsage = 0;
		entry.modBaseAddr = 0x00400000; // typical base address
		entry.modBaseSize = 0x00100000; // 1 MB
		entry.hModule = 0x00400000;
		entry.szModule = System.IO.Path.GetFileName(_env.ExecutablePath);
		entry.szExePath = _env.ExecutablePath;

		// Log the filled structure
		_logger.LogDebug("[Kernel32] Module32First: Returned {Entry}", entry.ToString());

		return 1; // TRUE
	}

	/// <summary>
	/// Retrieves information about the next module associated with a process or thread.
	/// BOOL Module32Next(
	///   [in]  HANDLE          hSnapshot,
	///   [out] LPMODULEENTRY32 lpme
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint Module32Next(uint hSnapshot, uint lpme)
	{
		_logger.LogInformation("[Kernel32] Module32Next(hSnapshot=0x{HSnapshot:X8}, lpme=0x{Lpme:X8})",
			hSnapshot, lpme);

		// For stub implementation, there are no more modules
		_lastError = (uint)NativeTypes.Win32Error.ERROR_NO_MORE_FILES;
		return 0; // FALSE
	}

	/// <summary>
	/// Moves a file or directory with extended options.
	/// BOOL MoveFileExA(
	///   [in] LPCSTR lpExistingFileName,
	///   [in] LPCSTR lpNewFileName,
	///   [in] DWORD  dwFlags
	/// );
	/// </summary>
	[DllModuleExport(12)]
	private uint MoveFileExA(uint lpExistingFileName, uint lpNewFileName, uint dwFlags)
	{
		try
		{
			var existingPath = _env.ReadAnsiString(lpExistingFileName);
			var newPath = _env.ReadAnsiString(lpNewFileName);

			_logger.LogInformation("[Kernel32] MoveFileExA(lpExistingFileName=\"{ExistingPath}\", lpNewFileName=\"{NewPath}\", dwFlags=0x{DwFlags:X8})",
				existingPath, newPath, dwFlags);

			if (string.IsNullOrEmpty(existingPath) || string.IsNullOrEmpty(newPath))
			{
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return (uint)NativeTypes.Win32Bool.FALSE;
			}

			// Check for delay until reboot flag (not supported in emulator)
			if ((dwFlags & (uint)NativeTypes.MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT) != 0)
			{
				_logger.LogWarning("[Kernel32] MoveFileExA: MOVEFILE_DELAY_UNTIL_REBOOT not supported");
				return (uint)NativeTypes.Win32Bool.TRUE; // Pretend success
			}

			// Try using VFS if available
			if (_env.VirtualFileSystem != null)
			{
				var replaceExisting = (dwFlags & (uint)NativeTypes.MoveFileFlags.MOVEFILE_REPLACE_EXISTING) != 0;

				// Check if destination exists
				if (_env.VirtualFileSystem.FileExists(newPath))
				{
					if (!replaceExisting)
					{
						_lastError = (uint)NativeTypes.Win32Error.ERROR_ALREADY_EXISTS;
						return (uint)NativeTypes.Win32Bool.FALSE;
					}
					// Delete existing file
					_env.VirtualFileSystem.DeleteFile(newPath);
				}

				// Use VFS MoveFile method
				if (_env.VirtualFileSystem.MoveFile(existingPath, newPath))
				{
					return (uint)NativeTypes.Win32Bool.TRUE;
				}
				else
				{
					_lastError = (uint)NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
					return (uint)NativeTypes.Win32Bool.FALSE;
				}
			}

			// VFS not available - return error
			_logger.LogError("[Kernel32] MoveFileExA: VFS not available");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] MoveFileExA exception");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_ACCESS_DENIED;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}
	}

	/// <summary>
	/// Converts MS-DOS date and time values to a file time.
	/// BOOL DosDateTimeToFileTime(
	///   [in]  WORD     wFatDate,
	///   [in]  WORD     wFatTime,
	///   [out] LPFILETIME lpFileTime
	/// );
	/// </summary>
	[DllModuleExport(12)]
	private uint DosDateTimeToFileTime(uint wFatDate, uint wFatTime, uint lpFileTime)
	{
		_logger.LogInformation("[Kernel32] DosDateTimeToFileTime(wFatDate=0x{WFatDate:X4}, wFatTime=0x{WFatTime:X4}, lpFileTime=0x{LpFileTime:X8})",
			wFatDate, wFatTime, lpFileTime);

		if (lpFileTime == 0)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		try
		{
			// Extract date components from FAT date (bits: YYYYYYYMMMMDDDDD)
			int year = ((int)wFatDate >> 9) + 1980;
			int month = ((int)wFatDate >> 5) & 0xF;
			int day = (int)wFatDate & 0x1F;

			// Extract time components from FAT time (bits: HHHHHMMMMMMSSSS)
			int hour = (int)wFatTime >> 11;
			int minute = ((int)wFatTime >> 5) & 0x3F;
			int second = ((int)wFatTime & 0x1F) * 2; // Seconds are stored in 2-second increments

			// Validate components
			if (month < 1 || month > 12 || day < 1 || day > 31 || hour > 23 || minute > 59 || second > 59)
			{
				_logger.LogWarning("[Kernel32] DosDateTimeToFileTime: Invalid date/time components (year={Year}, month={Month}, day={Day}, hour={Hour}, minute={Minute}, second={Second})",
					year, month, day, hour, minute, second);
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return (uint)NativeTypes.Win32Bool.FALSE;
			}

			// Create DateTime, handle invalid date combinations
			DateTime dateTime;
			try
			{
				dateTime = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
			}
			catch (ArgumentOutOfRangeException ex)
			{
				_logger.LogWarning(ex, "[Kernel32] DosDateTimeToFileTime: Invalid date combination (year={Year}, month={Month}, day={Day}, hour={Hour}, minute={Minute}, second={Second})",
					year, month, day, hour, minute, second);
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return (uint)NativeTypes.Win32Bool.FALSE;
			}

			// Convert to FILETIME (100-nanosecond intervals since January 1, 1601)
			long fileTime = dateTime.ToFileTimeUtc();

			// Write FILETIME structure (8 bytes: dwLowDateTime, dwHighDateTime)
			_env.MemWrite32(lpFileTime, (uint)(fileTime & 0xFFFFFFFF));
			_env.MemWrite32(lpFileTime + 4, (uint)(fileTime >> 32));

			return (uint)NativeTypes.Win32Bool.TRUE;
		}
		catch (ArgumentOutOfRangeException ex)
		{
			_logger.LogError(ex, "[Kernel32] DosDateTimeToFileTime ArgumentOutOfRangeException");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}
		catch (ArgumentException ex)
		{
			_logger.LogError(ex, "[Kernel32] DosDateTimeToFileTime ArgumentException");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}
		catch (OverflowException ex)
		{
			_logger.LogError(ex, "[Kernel32] DosDateTimeToFileTime OverflowException");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}
	}

	/// <summary>
	/// Determines whether a character is a lead byte in a double-byte character set (DBCS).
	/// BOOL IsDBCSLeadByte(
	///   [in] BYTE TestChar
	/// );
	/// </summary>
	[DllModuleExport(4)]
	private uint IsDBCSLeadByte(uint testChar)
	{
		_logger.LogDebug("[Kernel32] IsDBCSLeadByte(testChar=0x{TestChar:X2})", testChar & 0xFF);

		// Get the current code page
		var codePage = GetAcp();

		// For simplicity, we'll check common DBCS code pages
		// Japanese Shift-JIS (932), Korean (949), Chinese Simplified (936), Chinese Traditional (950)
		if (codePage == CodePage.Japan || codePage == CodePage.China || codePage == CodePage.Korea || codePage == CodePage.Taiwan)
		{
			byte b = (byte)(testChar & 0xFF);

			// Japanese Shift-JIS (932): lead bytes 0x81-0x9F, 0xE0-0xFC
			if (codePage == CodePage.Japan && ((b >= 0x81 && b <= 0x9F) || (b >= 0xE0 && b <= 0xFC)))
			{
				return (uint)NativeTypes.Win32Bool.TRUE;
			}

			// Chinese Simplified (GBK, 936), Korean (949), Chinese Traditional (Big5, 950): lead bytes 0x81-0xFE
			if ((codePage == CodePage.China || codePage == CodePage.Korea || codePage == CodePage.Taiwan) && (b >= 0x81 && b <= 0xFE))
			{
				return (uint)NativeTypes.Win32Bool.TRUE;
			}
		}

		// Not a DBCS lead byte or not a DBCS code page
		return (uint)NativeTypes.Win32Bool.FALSE;
	}

	/// <summary>
	/// Moves a block of memory from one location to another (handles overlapping regions).
	/// void RtlMoveMemory(
	///   [out] VOID UNALIGNED *Destination,
	///   [in]  const VOID UNALIGNED *Source,
	///   [in]  SIZE_T Length
	/// );
	/// </summary>
	[DllModuleExport(12)]
	private uint RtlMoveMemory(uint destination, uint source, uint length)
	{
		_logger.LogDebug("[Kernel32] RtlMoveMemory(destination=0x{Destination:X8}, source=0x{Source:X8}, length={Length})",
			destination, source, length);

		if (length == 0)
		{
			return 0;
		}

		try
		{
			// Handle overlapping memory regions correctly
			if (destination < source)
			{
				// Copy forward
				for (uint i = 0; i < length; i++)
				{
					_env.MemWrite8(destination + i, _env.MemRead8(source + i));
				}
			}
			else if (destination > source)
			{
				// Copy backward to handle overlap
				for (int i = (int)length - 1; i >= 0; i--)
				{
					_env.MemWrite8(destination + (uint)i, _env.MemRead8(source + (uint)i));
				}
			}
			// If destination == source, no action needed

			return 0; // RtlMoveMemory returns void
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] RtlMoveMemory exception");
			return 0;
		}
	}

	/// <summary>
	/// Fills a block of memory with zeros.
	/// void RtlZeroMemory(
	///   [out] VOID UNALIGNED *Destination,
	///   [in]  SIZE_T Length
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint RtlZeroMemory(uint destination, uint length)
	{
		_logger.LogDebug("[Kernel32] RtlZeroMemory(destination=0x{Destination:X8}, length={Length})",
			destination, length);

		if (length == 0)
		{
			return 0;
		}

		try
		{
			for (uint i = 0; i < length; i++)
			{
				_env.MemWrite8(destination + i, 0);
			}

			return 0; // RtlZeroMemory returns void
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] RtlZeroMemory exception");
			return 0;
		}
	}

	/// <summary>
	/// Opens a file using the original OpenFile API (legacy).
	/// HFILE OpenFile(LPCSTR lpFileName, LPOFSTRUCT lpReOpenBuff, UINT uStyle);
	/// </summary>
	[DllModuleExport(28)]
	private uint OpenFile(in LpcStr lpFileName, uint lpReOpenBuff, uint uStyle)
	{
		var fileName = lpFileName.Read(_env.Memory) ?? "";
		_logger.LogInformation("[Kernel32] OpenFile(lpFileName='{FileName}', lpReOpenBuff=0x{LpReOpenBuff:X8}, uStyle=0x{UStyle:X8})",
			fileName, lpReOpenBuff, uStyle);

		// OFSTRUCT is 136 bytes, fill with basic info
		if (lpReOpenBuff != 0)
		{
			_env.MemWrite8(lpReOpenBuff, 136); // cBytes
			_env.MemWrite8(lpReOpenBuff + 1, 1); // fFixedDisk
			_env.MemWrite16(lpReOpenBuff + 2, 0); // nErrCode
			_env.MemWrite16(lpReOpenBuff + 4, 0); // Reserved1
			_env.MemWrite16(lpReOpenBuff + 6, 0); // Reserved2
			_env.WriteAnsiStringAt(lpReOpenBuff + 8, fileName.Length > 127 ? fileName.Substring(0, 127) : fileName);
		}

		// Map uStyle flags: OF_READ (0x0), OF_WRITE (0x1), OF_READWRITE (0x2), OF_CREATE (0x1000), OF_EXIST (0x4000)
		uint desiredAccess = GENERIC_READ;
		uint creationDisposition = OPEN_EXISTING;

		if ((uStyle & 0x0001) != 0)
		{
			desiredAccess = GENERIC_WRITE;
		}
		else if ((uStyle & 0x0002) != 0)
		{
			desiredAccess = GENERIC_READ | GENERIC_WRITE;
		}

		if ((uStyle & 0x1000) != 0)
		{
			creationDisposition = CREATE_ALWAYS;
		}

		// For OF_EXIST, just check file exists
		if ((uStyle & 0x4000) != 0)
		{
			if (_env.VirtualFileSystem == null)
			{
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
				return 0xFFFFFFFF;
			}

			var resolved = WindowsPathUtility.ResolvePath(fileName, _env.CurrentDirectory);
			return _env.VirtualFileSystem.FileExists(resolved) ? 0u : 0xFFFFFFFF;
		}

		// Use CreateFileA to actually open - need to create a string pointer
		var tempAddr = _env.SimpleAlloc((uint)(fileName.Length + 1));
		_env.WriteAnsiStringAt(tempAddr, fileName);
		var handle = CreateFileA(tempAddr, desiredAccess, 0, 0, creationDisposition, 0, 0);
		// No need to free - memory is managed

		return handle == (uint)NativeTypes.Win32Handle.INVALID_HANDLE_VALUE ? 0xFFFFFFFF : handle;
	}

	/// <summary>
	/// Maps a view of a file mapping into the address space of a calling process at a specific address.
	/// LPVOID MapViewOfFileEx(
	///   [in] HANDLE hFileMappingObject,
	///   [in] DWORD  dwDesiredAccess,
	///   [in] DWORD  dwFileOffsetHigh,
	///   [in] DWORD  dwFileOffsetLow,
	///   [in] SIZE_T dwNumberOfBytesToMap,
	///   [in] LPVOID lpBaseAddress
	/// );
	/// </summary>
	[DllModuleExport(24)]
	private uint MapViewOfFileEx(uint hFileMappingObject, uint dwDesiredAccess,
		uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap, uint lpBaseAddress)
	{
		_logger.LogInformation("[Kernel32] MapViewOfFileEx(hFileMappingObject=0x{HFileMappingObject:X8}, dwDesiredAccess=0x{DwDesiredAccess:X}, dwFileOffsetHigh={DwFileOffsetHigh}, dwFileOffsetLow={DwFileOffsetLow}, dwNumberOfBytesToMap={DwNumberOfBytesToMap}, lpBaseAddress=0x{LpBaseAddress:X8})",
			hFileMappingObject, dwDesiredAccess, dwFileOffsetHigh, dwFileOffsetLow, dwNumberOfBytesToMap, lpBaseAddress);

		// Validate file mapping handle
		if (hFileMappingObject == 0 || hFileMappingObject == 0xFFFFFFFF)
		{
			_logger.LogWarning("[Kernel32] MapViewOfFileEx: Invalid file mapping handle");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
			return 0;
		}

		// MapViewOfFileEx is like MapViewOfFile but allows specifying a base address
		// For simplicity in emulation, we ignore the requested base address and allocate memory
		// A full implementation would try to honor the lpBaseAddress parameter

		if (dwNumberOfBytesToMap == 0)
		{
			dwNumberOfBytesToMap = 0x10000; // 64KB default
		}

		// Allocate memory for the view (ignoring lpBaseAddress for now)
		var baseAddress = VirtualAlloc(0, dwNumberOfBytesToMap, 0x1000 | 0x2000, 0x04); // MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE

		if (baseAddress == 0)
		{
			_logger.LogWarning("[Kernel32] MapViewOfFileEx: Failed to allocate memory");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
			return 0;
		}

		_logger.LogInformation("[Kernel32] MapViewOfFileEx: Allocated view at 0x{BaseAddress:X8} (requested 0x{LpBaseAddress:X8})",
			baseAddress, lpBaseAddress);

		return baseAddress;
	}

	/// <summary>
	/// Writes to the disk a byte range within a mapped view of a file.
	/// BOOL FlushViewOfFile(
	///   [in] LPCVOID lpBaseAddress,
	///   [in] SIZE_T  dwNumberOfBytesToFlush
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint FlushViewOfFile(uint lpBaseAddress, uint dwNumberOfBytesToFlush)
	{
		_logger.LogInformation("[Kernel32] FlushViewOfFile(lpBaseAddress=0x{LpBaseAddress:X8}, dwNumberOfBytesToFlush={DwNumberOfBytesToFlush})",
			lpBaseAddress, dwNumberOfBytesToFlush);

		// FlushViewOfFile writes changes in a memory-mapped file to disk
		// For emulation, we just log it and return success
		// A full implementation would:
		// 1. Track which memory-mapped files are dirty
		// 2. Write the dirty ranges back to the VFS
		// 3. Ensure data integrity

		return 1; // TRUE (success)
	}

	/// <summary>
	/// Unlocks a region of pages in the virtual address space of the calling process.
	/// BOOL VirtualUnlock(
	///   [in] LPVOID lpAddress,
	///   [in] SIZE_T dwSize
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint VirtualUnlock(uint lpAddress, uint dwSize)
	{
		_logger.LogInformation("[Kernel32] VirtualUnlock(lpAddress=0x{LpAddress:X8}, dwSize={DwSize})",
			lpAddress, dwSize);

		// VirtualUnlock unlocks pages that were locked by VirtualLock
		// For emulation, we don't enforce page locking, so we just return success
		// A full implementation would:
		// 1. Track which pages are locked
		// 2. Decrement the lock count for the specified range
		// 3. Return error if pages weren't locked

		return 1; // TRUE (success)
	}

	/// <summary>
	/// Retrieves the module handle for the specified module (Unicode version).
	/// HMODULE GetModuleHandleW(
	///   [in, optional] LPCWSTR lpModuleName
	/// );
	/// </summary>
	[DllModuleExport(4)]
	private uint GetModuleHandleW(uint lpModuleName)
	{
		string? moduleName;
		if (lpModuleName == 0)
		{
			// NULL means get handle to the main executable
			_logger.LogInformation("[Kernel32] GetModuleHandleW(NULL) -> main executable");
			return _imageBase;
		}

		moduleName = _env.ReadUnicodeString(lpModuleName);
		_logger.LogInformation("[Kernel32] GetModuleHandleW called: module='{ModuleName}'", moduleName ?? "NULL");

		if (string.IsNullOrEmpty(moduleName))
		{
			return _imageBase;
		}

		// Normalize the module name (remove path, make uppercase, ensure .DLL extension)
		var normalizedName = Path.GetFileName(moduleName).ToUpperInvariant();
		if (!normalizedName.EndsWith(".DLL", StringComparison.OrdinalIgnoreCase))
		{
			normalizedName += ".DLL";
		}

		// Check if this is a system DLL that we emulate
		var exports = DllModuleExportInfo.GetAllExports(normalizedName);
		var isSystemDll = exports.Count > 0;

		if (isSystemDll || _env.IsModuleLoaded(normalizedName))
		{
			// Load/register the module and get its handle
			var handle = _env.LoadModule(normalizedName);
			_logger.LogDebug("[Kernel32] GetModuleHandleW returning handle for {NormalizedName}: 0x{Handle:X8}", normalizedName, handle);
			return handle;
		}

		// Module not found
		_logger.LogWarning("[Kernel32] GetModuleHandleW: module '{ModuleName}' not found", moduleName);
		_lastError = (uint)NativeTypes.Win32Error.ERROR_MOD_NOT_FOUND;
		return 0;
	}

	/// <summary>
	/// Loads the specified module into the address space of the calling process (Unicode version).
	/// HMODULE LoadLibraryW(
	///   [in] LPCWSTR lpLibFileName
	/// );
	/// </summary>
	[DllModuleExport(4)]
	private uint LoadLibraryW(uint lpLibFileName)
	{
		if (lpLibFileName == 0)
		{
			_logger.LogWarning("[Kernel32] LoadLibraryW: NULL library name");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}

		// Read the library name from memory
		var libraryName = _env.ReadUnicodeString(lpLibFileName);
		if (string.IsNullOrEmpty(libraryName))
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}

		_logger.LogInformation("[Kernel32] LoadLibraryW(lpLibFileName=\"{LibFileName}\")", libraryName);

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
		_logger.LogInformation("[Kernel32] Loading system DLL via thunking: {LibraryName}", libraryName);

		// Register with dispatcher for function call tracking
		_dispatcher?.RegisterDynamicallyLoadedDll(libraryName);

		// For system libraries, we still need to track them but mark them as system modules
		return _env.LoadModule(libraryName);
	}

	/// <summary>
	/// Retrieves the current input mode of a console's input buffer or the current output mode of a console screen buffer.
	/// BOOL GetConsoleMode(
	///   [in]  HANDLE  hConsoleHandle,
	///   [out] LPDWORD lpMode
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint GetConsoleMode(uint hConsoleHandle, uint lpMode)
	{
		_logger.LogInformation("[Kernel32] GetConsoleMode(hConsoleHandle=0x{HConsoleHandle:X8}, lpMode=0x{LpMode:X8})",
			hConsoleHandle, lpMode);

		if (lpMode == 0)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0; // FALSE
		}

		// Return default console mode flags
		// ENABLE_ECHO_INPUT (0x0004) | ENABLE_LINE_INPUT (0x0002) | ENABLE_PROCESSED_INPUT (0x0001)
		const uint defaultInputMode = 0x0007;
		// ENABLE_PROCESSED_OUTPUT (0x0001) | ENABLE_WRAP_AT_EOL_OUTPUT (0x0002)
		const uint defaultOutputMode = 0x0003;

		// Determine if this is input or output handle (simplified check)
		// STD_INPUT_HANDLE = -10, STD_OUTPUT_HANDLE = -11, STD_ERROR_HANDLE = -12
		var mode = hConsoleHandle == unchecked((uint)-10) ? defaultInputMode : defaultOutputMode;

		_env.MemWrite32(lpMode, mode);
		_logger.LogInformation("[Kernel32] GetConsoleMode: Returning mode 0x{Mode:X8}", mode);

		return 1; // TRUE
	}

	/// <summary>
	/// Sets the input mode of a console's input buffer or the output mode of a console screen buffer.
	/// BOOL SetConsoleMode(
	///   [in] HANDLE hConsoleHandle,
	///   [in] DWORD  dwMode
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint SetConsoleMode(uint hConsoleHandle, uint dwMode)
	{
		_logger.LogInformation("[Kernel32] SetConsoleMode(hConsoleHandle=0x{HConsoleHandle:X8}, dwMode=0x{DwMode:X8})",
			hConsoleHandle, dwMode);

		// For emulation, we just log the request and return success
		// A full implementation would track console modes and apply them

		return 1; // TRUE (success)
	}

	/// <summary>
	/// Sets the input code page used by the console associated with the calling process.
	/// BOOL SetConsoleCP(
	///   [in] UINT wCodePageID
	/// );
	/// </summary>
	[DllModuleExport(4)]
	private uint SetConsoleCP(uint wCodePageID)
	{
		_logger.LogInformation("[Kernel32] SetConsoleCP(wCodePageID={WCodePageID})", wCodePageID);

		// For emulation, we just log the request and return success
		// A full implementation would set the console input code page

		return 1; // TRUE (success)
	}

	/// <summary>
	/// Retrieves the number of unread input records in the console's input buffer.
	/// BOOL GetNumberOfConsoleInputEvents(
	///   [in]  HANDLE  hConsoleInput,
	///   [out] LPDWORD lpcNumberOfEvents
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint GetNumberOfConsoleInputEvents(uint hConsoleInput, uint lpcNumberOfEvents)
	{
		_logger.LogInformation("[Kernel32] GetNumberOfConsoleInputEvents(hConsoleInput=0x{HConsoleInput:X8}, lpcNumberOfEvents=0x{LpcNumberOfEvents:X8})",
			hConsoleInput, lpcNumberOfEvents);

		if (lpcNumberOfEvents == 0)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0; // FALSE
		}

		// For stub, return 0 events
		_env.MemWrite32(lpcNumberOfEvents, 0);

		return 1; // TRUE
	}

	/// <summary>
	/// Reads data from a console input buffer and removes it from the buffer (ANSI version).
	/// BOOL ReadConsoleInputA(
	///   [in]  HANDLE        hConsoleInput,
	///   [out] PINPUT_RECORD lpBuffer,
	///   [in]  DWORD         nLength,
	///   [out] LPDWORD       lpNumberOfEventsRead
	/// );
	/// </summary>
	[DllModuleExport(16)]
	private uint ReadConsoleInputA(uint hConsoleInput, uint lpBuffer, uint nLength, uint lpNumberOfEventsRead)
	{
		_logger.LogInformation("[Kernel32] ReadConsoleInputA(hConsoleInput=0x{HConsoleInput:X8}, lpBuffer=0x{LpBuffer:X8}, nLength={NLength}, lpNumberOfEventsRead=0x{LpNumberOfEventsRead:X8})",
			hConsoleInput, lpBuffer, nLength, lpNumberOfEventsRead);

		if (lpBuffer == 0 || lpNumberOfEventsRead == 0)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0; // FALSE
		}

		// For stub, return 0 events read
		_env.MemWrite32(lpNumberOfEventsRead, 0);

		return 1; // TRUE
	}

	/// <summary>
	/// Reads data from a console input buffer and removes it from the buffer (Unicode version).
	/// BOOL ReadConsoleInputW(
	///   [in]  HANDLE        hConsoleInput,
	///   [out] PINPUT_RECORD lpBuffer,
	///   [in]  DWORD         nLength,
	///   [out] LPDWORD       lpNumberOfEventsRead
	/// );
	/// </summary>
	[DllModuleExport(16)]
	private uint ReadConsoleInputW(uint hConsoleInput, uint lpBuffer, uint nLength, uint lpNumberOfEventsRead)
	{
		_logger.LogInformation("[Kernel32] ReadConsoleInputW(hConsoleInput=0x{HConsoleInput:X8}, lpBuffer=0x{LpBuffer:X8}, nLength={NLength}, lpNumberOfEventsRead=0x{LpNumberOfEventsRead:X8})",
			hConsoleInput, lpBuffer, nLength, lpNumberOfEventsRead);

		if (lpBuffer == 0 || lpNumberOfEventsRead == 0)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0; // FALSE
		}

		// For stub, return 0 events read
		_env.MemWrite32(lpNumberOfEventsRead, 0);

		return 1; // TRUE
	}

	/// <summary>
	/// Reads data from the console input buffer without removing it from the buffer (Unicode version).
	/// BOOL PeekConsoleInputW(
	///   [in]  HANDLE        hConsoleInput,
	///   [out] PINPUT_RECORD lpBuffer,
	///   [in]  DWORD         nLength,
	///   [out] LPDWORD       lpNumberOfEventsRead
	/// );
	/// </summary>
	[DllModuleExport(16)]
	private uint PeekConsoleInputW(uint hConsoleInput, uint lpBuffer, uint nLength, uint lpNumberOfEventsRead)
	{
		_logger.LogInformation("[Kernel32] PeekConsoleInputW(hConsoleInput=0x{HConsoleInput:X8}, lpBuffer=0x{LpBuffer:X8}, nLength={NLength}, lpNumberOfEventsRead=0x{LpNumberOfEventsRead:X8})",
			hConsoleInput, lpBuffer, nLength, lpNumberOfEventsRead);

		if (lpBuffer == 0 || lpNumberOfEventsRead == 0)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0; // FALSE
		}

		// For stub, return 0 events read (buffer is empty)
		_env.MemWrite32(lpNumberOfEventsRead, 0);

		return 1; // TRUE
	}

	/// <summary>
	/// Retrieves information about the specified console screen buffer.
	/// BOOL GetConsoleScreenBufferInfo(
	///   [in]  HANDLE                      hConsoleOutput,
	///   [out] PCONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo
	/// );
	/// </summary>
	[DllModuleExport(8, IsStub = true)]
	private uint GetConsoleScreenBufferInfo(uint hConsoleOutput, uint lpConsoleScreenBufferInfo)
	{
		_logger.LogInformation("[Kernel32] GetConsoleScreenBufferInfo(hConsoleOutput=0x{HConsoleOutput:X8}, lpConsoleScreenBufferInfo=0x{LpConsoleScreenBufferInfo:X8})",
			hConsoleOutput, lpConsoleScreenBufferInfo);

		if (lpConsoleScreenBufferInfo == 0)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0; // FALSE
		}

		// Create and populate the CONSOLE_SCREEN_BUFFER_INFO structure
		var info = new NativeTypes.CONSOLE_SCREEN_BUFFER_INFO
		{
			dwSize = new NativeTypes.COORD(80, 25),              // 80x25 standard console
			dwCursorPosition = new NativeTypes.COORD(0, 0),       // Cursor at 0,0
			wAttributes = 0x07,                                    // White on black
			srWindow = new NativeTypes.SMALL_RECT
			{
				Left = 0,
				Top = 0,
				Right = 79,
				Bottom = 24
			},
			dwMaximumWindowSize = new NativeTypes.COORD(80, 25)   // Maximum 80x25
		};

		// Write the structure to memory using MemWriteStruct
		_env.MemWriteStruct(lpConsoleScreenBufferInfo, ref info);

		return 1; // TRUE
	}

	/// <summary>
	/// Retrieves the size of the largest possible console window based on the current font and display size.
	/// COORD GetLargestConsoleWindowSize(
	///   [in] HANDLE hConsoleOutput
	/// );
	/// </summary>
	/// <remarks>
	/// Returns a packed COORD value where the low word is the X coordinate and the high word is the Y coordinate.
	/// </remarks>
	[DllModuleExport(4, IsStub = true)]
	private uint GetLargestConsoleWindowSize(uint hConsoleOutput)
	{
		_logger.LogInformation("[Kernel32] GetLargestConsoleWindowSize(hConsoleOutput=0x{HConsoleOutput:X8})",
			hConsoleOutput);

		// Return COORD packed as DWORD: low word = X (columns), high word = Y (rows)
		// Return 80x50 as a reasonable maximum console window size
		var x = (ushort)80;
		var y = (ushort)50;
		return (uint)((y << 16) | x);
	}

	/// <summary>
	/// Copies a specified number of characters from consecutive cells of a console screen buffer.
	/// BOOL ReadConsoleOutputCharacterA(
	///   [in]  HANDLE  hConsoleOutput,
	///   [out] LPSTR   lpCharacter,
	///   [in]  DWORD   nLength,
	///   [in]  COORD   dwReadCoord,
	///   [out] LPDWORD lpNumberOfCharsRead
	/// );
	/// </summary>
	[DllModuleExport(20, IsStub = true)]
	private uint ReadConsoleOutputCharacterA(uint hConsoleOutput, uint lpCharacter, uint nLength, uint dwReadCoord, uint lpNumberOfCharsRead)
	{
		_logger.LogInformation("[Kernel32] ReadConsoleOutputCharacterA(hConsoleOutput=0x{HConsoleOutput:X8}, lpCharacter=0x{LpCharacter:X8}, nLength={NLength}, dwReadCoord=0x{DwReadCoord:X8}, lpNumberOfCharsRead=0x{LpNumberOfCharsRead:X8})",
			hConsoleOutput, lpCharacter, nLength, dwReadCoord, lpNumberOfCharsRead);

		if (lpCharacter == 0 || lpNumberOfCharsRead == 0)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0; // FALSE
		}

		// For stub, fill with spaces and return the requested length (or 0)
		// In a real implementation, we would read from the console buffer
		for (uint i = 0; i < nLength; i++)
		{
			_env.MemWrite8(lpCharacter + i, (byte)' ');
		}

		_env.MemWrite32(lpNumberOfCharsRead, nLength);
		return 1; // TRUE
	}

	/// <summary>
	/// Changes the size of the specified console screen buffer.
	/// BOOL SetConsoleScreenBufferSize(
	///   [in] HANDLE hConsoleOutput,
	///   [in] COORD  dwSize
	/// );
	/// </summary>
	[DllModuleExport(8, IsStub = true)]
	private uint SetConsoleScreenBufferSize(uint hConsoleOutput, uint dwSize)
	{
		// dwSize is a packed COORD: low word = X (columns), high word = Y (rows)
		var x = (ushort)(dwSize & 0xFFFF);
		var y = (ushort)((dwSize >> 16) & 0xFFFF);

		_logger.LogInformation("[Kernel32] SetConsoleScreenBufferSize(hConsoleOutput=0x{HConsoleOutput:X8}, dwSize=({X}, {Y}))",
			hConsoleOutput, x, y);

		// For stub, always return success
		return 1; // TRUE
	}

	/// <summary>
	/// Sets the current size and position of a console screen buffer's window.
	/// BOOL SetConsoleWindowInfo(
	///   [in]       HANDLE     hConsoleOutput,
	///   [in]       BOOL       bAbsolute,
	///   [in] const SMALL_RECT *lpConsoleWindow
	/// );
	/// </summary>
	[DllModuleExport(12, IsStub = true)]
	private uint SetConsoleWindowInfo(uint hConsoleOutput, uint bAbsolute, uint lpConsoleWindow)
	{
		_logger.LogInformation("[Kernel32] SetConsoleWindowInfo(hConsoleOutput=0x{HConsoleOutput:X8}, bAbsolute={BAbsolute}, lpConsoleWindow=0x{LpConsoleWindow:X8})",
			hConsoleOutput, bAbsolute, lpConsoleWindow);

		if (lpConsoleWindow == 0)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0; // FALSE
		}

		// Read SMALL_RECT structure from memory using MemReadStruct
		var rect = _env.MemReadStruct<NativeTypes.SMALL_RECT>(lpConsoleWindow);

		_logger.LogInformation("[Kernel32] SetConsoleWindowInfo: Window rect ({Left}, {Top}, {Right}, {Bottom})",
			rect.Left, rect.Top, rect.Right, rect.Bottom);

		// For stub, always return success
		return 1; // TRUE
	}

	/// <summary>
	/// Writes data directly to the console input buffer.
	/// BOOL WriteConsoleInputA(
	///   [in]  HANDLE       hConsoleInput,
	///   [in]  INPUT_RECORD *lpBuffer,
	///   [in]  DWORD        nLength,
	///   [out] LPDWORD      lpNumberOfEventsWritten
	/// );
	/// </summary>
	[DllModuleExport(16, IsStub = true)]
	private uint WriteConsoleInputA(uint hConsoleInput, uint lpBuffer, uint nLength, uint lpNumberOfEventsWritten)
	{
		_logger.LogInformation("[Kernel32] WriteConsoleInputA(hConsoleInput=0x{HConsoleInput:X8}, lpBuffer=0x{LpBuffer:X8}, nLength={NLength}, lpNumberOfEventsWritten=0x{LpNumberOfEventsWritten:X8})",
			hConsoleInput, lpBuffer, nLength, lpNumberOfEventsWritten);

		if (lpBuffer == 0 || lpNumberOfEventsWritten == 0)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0; // FALSE
		}

		// For stub, pretend we wrote all events
		_env.MemWrite32(lpNumberOfEventsWritten, nLength);
		return 1; // TRUE
	}

	#region 16-bit Thunking Functions

	/// <summary>
	/// FT_Exit* functions - Thunk exit stubs for 16-bit to 32-bit transitions.
	/// These are legacy functions for 16-bit compatibility and are no-ops in pure 32-bit emulation.
	/// </summary>
	[DllModuleExport(4, IsStub = true)]
	private uint FT_Exit4()
	{
		_logger.LogDebug("[Kernel32] FT_Exit4 (16-bit thunk stub)");
		return 0;
	}

	[DllModuleExport(4, IsStub = true)]
	private uint FT_Exit8()
	{
		_logger.LogDebug("[Kernel32] FT_Exit8 (16-bit thunk stub)");
		return 0;
	}

	[DllModuleExport(4, IsStub = true)]
	private uint FT_Exit12()
	{
		_logger.LogDebug("[Kernel32] FT_Exit12 (16-bit thunk stub)");
		return 0;
	}

	[DllModuleExport(4, IsStub = true)]
	private uint FT_Exit16()
	{
		_logger.LogDebug("[Kernel32] FT_Exit16 (16-bit thunk stub)");
		return 0;
	}

	[DllModuleExport(4, IsStub = true)]
	private uint FT_Exit20()
	{
		_logger.LogDebug("[Kernel32] FT_Exit20 (16-bit thunk stub)");
		return 0;
	}

	[DllModuleExport(4, IsStub = true)]
	private uint FT_Exit24()
	{
		_logger.LogDebug("[Kernel32] FT_Exit24 (16-bit thunk stub)");
		return 0;
	}

	[DllModuleExport(4, IsStub = true)]
	private uint FT_Exit28()
	{
		_logger.LogDebug("[Kernel32] FT_Exit28 (16-bit thunk stub)");
		return 0;
	}

	[DllModuleExport(4, IsStub = true)]
	private uint FT_Exit32()
	{
		_logger.LogDebug("[Kernel32] FT_Exit32 (16-bit thunk stub)");
		return 0;
	}

	[DllModuleExport(4, IsStub = true)]
	private uint FT_Exit48()
	{
		_logger.LogDebug("[Kernel32] FT_Exit48 (16-bit thunk stub)");
		return 0;
	}

	[DllModuleExport(4, IsStub = true)]
	private uint FT_Prolog()
	{
		_logger.LogDebug("[Kernel32] FT_Prolog (16-bit thunk stub)");
		return 0;
	}

	[DllModuleExport(4, IsStub = true)]
	private uint FT_Thunk()
	{
		_logger.LogDebug("[Kernel32] FT_Thunk (16-bit thunk stub)");
		return 0;
	}

	#endregion

	#region Memory Mapping Functions (16-bit compatibility)

	/// <summary>
	/// SMapLS_IP_EBP_* functions - Map 32-bit linear address to 16-bit segment:offset.
	/// These are legacy functions for 16-bit compatibility and return the input address unchanged.
	/// The number suffix indicates the EBP offset to read the parameter from.
	/// </summary>
	[DllModuleExport(4, IsStub = true)]
	private uint SMapLS_IP_EBP_8()
	{
		_logger.LogDebug("[Kernel32] SMapLS_IP_EBP_8 (16-bit memory mapping stub)");
		// In a true implementation, would read address from [EBP+8] and return mapped value
		// For stub, return 0
		return 0;
	}

	[DllModuleExport(4, IsStub = true)]
	private uint SMapLS_IP_EBP_12()
	{
		_logger.LogDebug("[Kernel32] SMapLS_IP_EBP_12 (16-bit memory mapping stub)");
		return 0;
	}

	[DllModuleExport(4, IsStub = true)]
	private uint SMapLS_IP_EBP_16()
	{
		_logger.LogDebug("[Kernel32] SMapLS_IP_EBP_16 (16-bit memory mapping stub)");
		return 0;
	}

	[DllModuleExport(4, IsStub = true)]
	private uint SMapLS_IP_EBP_20()
	{
		_logger.LogDebug("[Kernel32] SMapLS_IP_EBP_20 (16-bit memory mapping stub)");
		return 0;
	}

	[DllModuleExport(4, IsStub = true)]
	private uint SMapLS_IP_EBP_24()
	{
		_logger.LogDebug("[Kernel32] SMapLS_IP_EBP_24 (16-bit memory mapping stub)");
		return 0;
	}

	[DllModuleExport(4, IsStub = true)]
	private uint SMapLS_IP_EBP_28()
	{
		_logger.LogDebug("[Kernel32] SMapLS_IP_EBP_28 (16-bit memory mapping stub)");
		return 0;
	}

	[DllModuleExport(4, IsStub = true)]
	private uint SMapLS_IP_EBP_32()
	{
		_logger.LogDebug("[Kernel32] SMapLS_IP_EBP_32 (16-bit memory mapping stub)");
		return 0;
	}

	/// <summary>
	/// SUnMapLS_IP_EBP_* functions - Unmap 16-bit segment:offset to 32-bit linear address.
	/// These are legacy functions for 16-bit compatibility and are no-ops.
	/// </summary>
	[DllModuleExport(4, IsStub = true)]
	private uint SUnMapLS_IP_EBP_8()
	{
		_logger.LogDebug("[Kernel32] SUnMapLS_IP_EBP_8 (16-bit memory unmapping stub)");
		return 0;
	}

	[DllModuleExport(4, IsStub = true)]
	private uint SUnMapLS_IP_EBP_12()
	{
		_logger.LogDebug("[Kernel32] SUnMapLS_IP_EBP_12 (16-bit memory unmapping stub)");
		return 0;
	}

	[DllModuleExport(4, IsStub = true)]
	private uint SUnMapLS_IP_EBP_16()
	{
		_logger.LogDebug("[Kernel32] SUnMapLS_IP_EBP_16 (16-bit memory unmapping stub)");
		return 0;
	}

	[DllModuleExport(4, IsStub = true)]
	private uint SUnMapLS_IP_EBP_20()
	{
		_logger.LogDebug("[Kernel32] SUnMapLS_IP_EBP_20 (16-bit memory unmapping stub)");
		return 0;
	}

	[DllModuleExport(4, IsStub = true)]
	private uint SUnMapLS_IP_EBP_24()
	{
		_logger.LogDebug("[Kernel32] SUnMapLS_IP_EBP_24 (16-bit memory unmapping stub)");
		return 0;
	}

	[DllModuleExport(4, IsStub = true)]
	private uint SUnMapLS_IP_EBP_28()
	{
		_logger.LogDebug("[Kernel32] SUnMapLS_IP_EBP_28 (16-bit memory unmapping stub)");
		return 0;
	}

	[DllModuleExport(4, IsStub = true)]
	private uint SUnMapLS_IP_EBP_32()
	{
		_logger.LogDebug("[Kernel32] SUnMapLS_IP_EBP_32 (16-bit memory unmapping stub)");
		return 0;
	}

	/// <summary>
	/// Maps a 32-bit linear address to a 16-bit segment:offset pair.
	/// DWORD SMapLS(LPVOID lpvAddress);
	/// </summary>
	[DllModuleExport(4, IsStub = true)]
	private uint SMapLS(uint lpvAddress)
	{
		_logger.LogDebug("[Kernel32] SMapLS(lpvAddress=0x{LpvAddress:X8})", lpvAddress);
		// In flat 32-bit mode, just return the address unchanged
		return lpvAddress;
	}

	/// <summary>
	/// Unmaps a 16-bit segment:offset pair.
	/// VOID SUnMapLS(DWORD dwAddress);
	/// </summary>
	[DllModuleExport(4, IsStub = true)]
	private uint SUnMapLS(uint dwAddress)
	{
		_logger.LogDebug("[Kernel32] SUnMapLS(dwAddress=0x{DwAddress:X8})", dwAddress);
		// No-op in flat 32-bit mode
		return 0;
	}

	/// <summary>
	/// Maps a 32-bit linear address to a 16-bit segment:offset pair.
	/// DWORD MapLS(LPVOID lpvAddress);
	/// </summary>
	[DllModuleExport(4, IsStub = true)]
	private uint MapLS(uint lpvAddress)
	{
		_logger.LogDebug("[Kernel32] MapLS(lpvAddress=0x{LpvAddress:X8})", lpvAddress);
		// In flat 32-bit mode, just return the address unchanged
		return lpvAddress;
	}

	/// <summary>
	/// Unmaps a 16-bit segment:offset pair.
	/// VOID UnMapLS(DWORD dwAddress);
	/// </summary>
	[DllModuleExport(695, IsStub = true)]
	private uint UnMapLS(uint dwAddress)
	{
		_logger.LogDebug("[Kernel32] UnMapLS(dwAddress=0x{DwAddress:X8})", dwAddress);
		// No-op in flat 32-bit mode
		return BAD_POINTER_FALSE;
	}

	/// <summary>
	/// Converts a 16-bit segment:offset pair to a 32-bit linear address.
	/// LPVOID MapSL(DWORD dwAddress);
	/// </summary>
	[DllModuleExport(4, IsStub = true)]
	private uint MapSL(uint dwAddress)
	{
		_logger.LogDebug("[Kernel32] MapSL(dwAddress=0x{DwAddress:X8})", dwAddress);
		// In flat 32-bit mode, just return the address unchanged
		return dwAddress;
	}

	/// <summary>
	/// Maps a 32-bit instance handle to a 16-bit instance handle.
	/// HINSTANCE16 MapHInstLS(HINSTANCE hInst);
	/// </summary>
	[DllModuleExport(4, IsStub = true)]
	private uint MapHInstLS(uint hInst)
	{
		_logger.LogDebug("[Kernel32] MapHInstLS(hInst=0x{HInst:X8})", hInst);
		// Return the handle unchanged
		return hInst;
	}

	/// <summary>
	/// Maps a 32-bit instance handle to a 16-bit instance handle (pointer version).
	/// HINSTANCE16 MapHInstLS_PN(HINSTANCE *phInst);
	/// </summary>
	[DllModuleExport(4, IsStub = true)]
	private uint MapHInstLS_PN(uint phInst)
	{
		_logger.LogDebug("[Kernel32] MapHInstLS_PN(phInst=0x{PhInst:X8})", phInst);
		if (phInst == 0)
		{
			return 0;
		}
		var hInst = _env.MemRead32(phInst);
		return hInst; // Return the handle unchanged
	}

	/// <summary>
	/// Fixes a 16-bit segment:offset selector.
	/// LPVOID MapSLFix(DWORD dwAddress);
	/// </summary>
	[DllModuleExport(4, IsStub = true)]
	private uint MapSLFix(uint dwAddress)
	{
		_logger.LogDebug("[Kernel32] MapSLFix(dwAddress=0x{DwAddress:X8})", dwAddress);
		// In flat 32-bit mode, just return the address unchanged
		return dwAddress;
	}

	/// <summary>
	/// Unmaps an array of fixed 16-bit segment:offset selectors.
	/// VOID UnMapSLFixArray(DWORD cSelectors, DWORD *lpSelectors);
	/// </summary>
	[DllModuleExport(8, IsStub = true)]
	private uint UnMapSLFixArray(uint cSelectors, uint lpSelectors)
	{
		_logger.LogDebug("[Kernel32] UnMapSLFixArray(cSelectors={CSelectors}, lpSelectors=0x{LpSelectors:X8})",
			cSelectors, lpSelectors);
		// No-op in flat 32-bit mode
		return 0;
	}

	#endregion

	#region Thunk Callback Functions

	/// <summary>
	/// Prolog for 16-bit to 32-bit thunk transition.
	/// </summary>
	[DllModuleExport(4, IsStub = true)]
	private uint K32Thk1632Prolog()
	{
		_logger.LogDebug("[Kernel32] K32Thk1632Prolog (16-bit to 32-bit thunk prolog stub)");
		return 0;
	}

	/// <summary>
	/// Epilog for 16-bit to 32-bit thunk transition.
	/// </summary>
	[DllModuleExport(4, IsStub = true)]
	private uint K32Thk1632Epilog()
	{
		_logger.LogDebug("[Kernel32] K32Thk1632Epilog (16-bit to 32-bit thunk epilog stub)");
		return 0;
	}

	/// <summary>
	/// Callback function for 16-bit code (16 bytes of parameters).
	/// </summary>
	[DllModuleExport(4, IsStub = true)]
	private uint Callback16()
	{
		_logger.LogDebug("[Kernel32] Callback16 (16-bit callback stub)");
		return 0;
	}

	/// <summary>
	/// Callback function for 16-bit code (20 bytes of parameters).
	/// </summary>
	[DllModuleExport(4, IsStub = true)]
	private uint Callback20()
	{
		_logger.LogDebug("[Kernel32] Callback20 (16-bit callback stub)");
		return 0;
	}

	#endregion

	#region String and Character Set Functions

	/// <summary>
	/// Verifies that the calling process has read access to the specified range of memory.
	/// BOOL IsBadStringPtrA(
	///   [in] LPCSTR lpsz,
	///   [in] UINT_PTR ucchMax
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint IsBadStringPtrA(uint lpsz, uint ucchMax)
	{
		_logger.LogDebug("[Kernel32] IsBadStringPtrA(lpsz=0x{Lpsz:X8}, ucchMax={UcchMax})", lpsz, ucchMax);

		// NULL pointer is always bad
		if (lpsz == 0)
		{
			return BAD_POINTER_TRUE;
		}

		// Check if we can read from this address
		try
		{
			// Try to read the string up to ucchMax characters
			// Use a reasonable upper bound when ucchMax is 0 to avoid excessive memory reads
			var maxLength = ucchMax == 0 ? MAX_STRING_CHECK_LENGTH_ANSI : (int)ucchMax;
			var offset = 0;

			while (offset < maxLength)
			{
				var b = _env.MemRead8(lpsz + (uint)offset);
				if (b == 0) // Found null terminator
				{
					return BAD_POINTER_FALSE;
				}
				offset++;
			}

			return BAD_POINTER_FALSE; // Good pointer (reached max length)
		}
		catch
		{
			// Memory access failed
			return BAD_POINTER_TRUE;
		}
	}

	/// <summary>
	/// Verifies that a pointer to a Unicode string is a valid memory pointer.
	/// BOOL IsBadStringPtrW(
	///   [in] LPCWSTR lpsz,
	///   [in] UINT_PTR ucchMax
	/// );
	/// </summary>
	[DllModuleExport(696)]
	private uint IsBadStringPtrW(uint lpsz, uint ucchMax)
	{
		_logger.LogDebug("[Kernel32] IsBadStringPtrW(lpsz=0x{Lpsz:X8}, ucchMax={UcchMax})", lpsz, ucchMax);

		// NULL pointer is always bad
		if (lpsz == 0)
		{
			return BAD_POINTER_TRUE;
		}

		// Check if we can read from this address
		try
		{
			// Try to read the string up to ucchMax characters
			// Use a reasonable upper bound when ucchMax is 0 to avoid excessive memory reads
			// Use 32768 characters for wide strings (64KB footprint, matching IsBadStringPtrA's 65536 single-byte limit)
			var maxLength = ucchMax == 0 ? MAX_STRING_CHECK_LENGTH_UNICODE : (int)ucchMax;
			var offset = 0;

			while (offset < maxLength)
			{
				// Read 16-bit wide character (wide chars take 2 bytes each)
				var w = _env.MemRead16(lpsz + (uint)(offset * 2));
				if (w == 0) // Found null terminator
				{
					return BAD_POINTER_FALSE;
				}
				offset++;
			}

			return BAD_POINTER_FALSE; // Good pointer (reached max length)
		}
		catch
		{
			// Memory access failed
			return BAD_POINTER_TRUE;
		}
	}

	/// <summary>
	/// Determines whether a character is a lead byte in the specified code page.
	/// BOOL IsDBCSLeadByteEx(
	///   [in] UINT  CodePage,
	///   [in] BYTE  TestChar
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint IsDBCSLeadByteEx(uint codePage, uint testChar)
	{
		_logger.LogDebug("[Kernel32] IsDBCSLeadByteEx(codePage={CodePage}, testChar=0x{TestChar:X2})",
			codePage, testChar);

		var b = (byte)(testChar & 0xFF);

		// Check for DBCS code pages
		switch (codePage)
		{
			case 932:  // Japanese Shift-JIS
				// Lead byte ranges: 0x81-0x9F, 0xE0-0xFC
				return (b >= 0x81 && b <= 0x9F) || (b >= 0xE0 && b <= 0xFC) ? 1u : 0u;

			case 936:  // Simplified Chinese GBK
				// Lead byte range: 0x81-0xFE
				return (b >= 0x81 && b <= 0xFE) ? 1u : 0u;

			case 949:  // Korean
				// Lead byte range: 0x81-0xFE
				return (b >= 0x81 && b <= 0xFE) ? 1u : 0u;

			case 950:  // Traditional Chinese Big5
				// Lead byte ranges: 0x81-0xFE
				return (b >= 0x81 && b <= 0xFE) ? 1u : 0u;

			case 1361: // Korean Johab
				// Lead byte range: 0x84-0xD3, 0xD8-0xDE, 0xE0-0xF9
				return ((b >= 0x84 && b <= 0xD3) || (b >= 0xD8 && b <= 0xDE) || (b >= 0xE0 && b <= 0xF9)) ? 1u : 0u;

			default:
				// Not a DBCS code page
				return 0; // FALSE
		}
	}

	/// <summary>
	/// Opens an existing named event object.
	/// HANDLE OpenEventA(
	///   [in] DWORD  dwDesiredAccess,
	///   [in] BOOL   bInheritHandle,
	///   [in] LPCSTR lpName
	/// );
	/// </summary>
	[DllModuleExport(12)]
	private uint OpenEventA(uint dwDesiredAccess, uint bInheritHandle, in LpcStr lpName)
	{
		var name = lpName.ToString() ?? string.Empty;
		_logger.LogInformation("[Kernel32] OpenEventA(dwDesiredAccess=0x{DwDesiredAccess:X}, bInheritHandle={BInheritHandle}, lpName=\"{Name}\")",
			dwDesiredAccess, bInheritHandle, name);

		if (_env.SynchronizationManager == null)
		{
			_logger.LogWarning("[Kernel32] OpenEventA: SynchronizationManager not available");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return (uint)NativeTypes.Win32Handle.NULL;
		}

		try
		{
			var handle = _env.SynchronizationManager.OpenEvent(name, dwDesiredAccess);
			if (handle == 0)
			{
				_lastError = (uint)NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			}
			return handle;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] OpenEventA exception");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return (uint)NativeTypes.Win32Handle.NULL;
		}
	}

	/// <summary>
	/// Retrieves the process flags.
	/// Based on reference: https://github.com/otya128/winevdm/blob/master/krnl386/stub.c
	/// DWORD GetProcessFlags(
	///   [in] DWORD dwProcessId
	/// );
	/// </summary>
	[DllModuleExport(4)]
	private uint GetProcessFlags(uint dwProcessId)
	{
		_logger.LogDebug("[Kernel32] GetProcessFlags(dwProcessId={DwProcessId})", dwProcessId);

		// If querying different process, return 0
		var currentProcessId = GetCurrentProcessId();
		if (dwProcessId != 0 && dwProcessId != currentProcessId)
		{
			_logger.LogDebug("[Kernel32] GetProcessFlags: Query for different process, returning 0");
			return (uint)NativeTypes.ProcessFlags.None;
		}

		NativeTypes.ProcessFlags flags = NativeTypes.ProcessFlags.None;

		// Check if this is a console application
		// Note: We don't currently store subsystem information in ProcessEnvironment
		// For now, assume GUI application (no console flag set)
		// TODO: Store subsystem type in ProcessEnvironment and check it here
		
		// Check if file APIs use OEM character set (AreFileApisANSI returns false)
		// For now, assume ANSI (don't set OEM flag)
		
		// Check if debugger is attached
		// We don't currently support debugger detection
		// For now, assume not being debugged

		_logger.LogDebug("[Kernel32] GetProcessFlags returning flags=0x{Flags:X8}", (uint)flags);
		return (uint)flags;
	}

	/// <summary>
	/// Converts a local memory handle to a global memory handle.
	/// HGLOBAL ConvertToGlobalHandle(
	///   [in] HANDLE hMem
	/// );
	/// </summary>
	[DllModuleExport(4)]
	private uint ConvertToGlobalHandle(uint hMem)
	{
		_logger.LogDebug("[Kernel32] ConvertToGlobalHandle(hMem=0x{HMem:X8})", hMem);

		// This is an obsolete Win16 compatibility function
		// In Win32, local and global heaps are the same, so just return the same handle
		return hMem;
	}

	/// <summary>
	/// Enumerates resources of a specified type within a binary module.
	/// BOOL EnumResourceNamesA(
	///   [in, optional] HMODULE          hModule,
	///   [in]           LPCSTR           lpType,
	///   [in]           ENUMRESNAMEPROCA lpEnumFunc,
	///   [in]           LONG_PTR         lParam
	/// );
	/// </summary>
	[DllModuleExport(16)]
	private uint EnumResourceNamesA(uint hModule, uint lpType, uint lpEnumFunc, uint lParam)
	{
		_logger.LogInformation("[Kernel32] EnumResourceNamesA(hModule=0x{HModule:X8}, lpType=0x{LpType:X8}, lpEnumFunc=0x{LpEnumFunc:X8}, lParam=0x{LParam:X8})",
			hModule, lpType, lpEnumFunc, lParam);

		if (_resourceReader == null)
		{
			_logger.LogWarning("[Kernel32] EnumResourceNamesA: Resource reader not initialized");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_RESOURCE_TYPE_NOT_FOUND;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}

		try
		{
			// Get all resources of the specified type
			var resources = _resourceReader.EnumerateResourceNames(lpType);
			
			if (resources == null || !resources.Any())
			{
				_logger.LogDebug("[Kernel32] EnumResourceNamesA: No resources found for type 0x{LpType:X8}", lpType);
				_lastError = (uint)NativeTypes.Win32Error.ERROR_RESOURCE_TYPE_NOT_FOUND;
				return (uint)NativeTypes.Win32Bool.FALSE;
			}

			// Log the resources that would be enumerated
			_logger.LogInformation("[Kernel32] EnumResourceNamesA: Found {Count} resources of type 0x{LpType:X8}",
				resources.Count(), lpType);
			
			foreach (var resourceId in resources)
			{
				_logger.LogDebug("[Kernel32] EnumResourceNamesA: Resource ID 0x{ResourceId:X8}", resourceId);
			}

			// TODO: Implement proper callback invocation
			// For now, return TRUE to indicate resources exist, but callbacks aren't actually invoked
			// This is a stub implementation - full callback support would require:
			// 1. Saving CPU state
			// 2. Pushing parameters on stack
			// 3. Calling callback function
			// 4. Checking return value
			// 5. Restoring CPU state
			
			return (uint)NativeTypes.Win32Bool.TRUE;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] EnumResourceNamesA exception");
			_lastError = (uint)NativeTypes.Win32Error.ERROR_RESOURCE_TYPE_NOT_FOUND;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}
	}

	#endregion

	/// <summary>
	/// Makes a critical section global (16-bit compatibility function).
	/// </summary>
	[DllModuleExport(4, IsStub = true)]
	private uint MakeCriticalSectionGlobal(uint lpCriticalSection)
	{
		_logger.LogInformation("[Kernel32] MakeCriticalSectionGlobal(lpCriticalSection=0x{LpCriticalSection:X8}) - 16-bit compatibility, no-op", lpCriticalSection);
		// This is a Win16 compatibility function that's typically a no-op in Win32
		return lpCriticalSection;
	}

	/// <summary>
	/// Reinitializes a critical section (16-bit compatibility function).
	/// </summary>
	[DllModuleExport(4, IsStub = true)]
	private void ReinitializeCriticalSection(uint lpCriticalSection)
	{
		_logger.LogInformation("[Kernel32] ReinitializeCriticalSection(lpCriticalSection=0x{LpCriticalSection:X8}) - 16-bit compatibility, no-op", lpCriticalSection);
		// This is a Win16 compatibility function that's typically a no-op in Win32
	}

	/// <summary>
	/// Disables thread library calls for the specified DLL module.
	/// </summary>
	[DllModuleExport(4, IsStub = true)]
	private uint DisableThreadLibraryCalls(uint hModule)
	{
		_logger.LogInformation("[Kernel32] DisableThreadLibraryCalls(hModule=0x{HModule:X8})", hModule);
		// Stub: return success (TRUE)
		return 1;
	}

	/// <summary>
	/// Opens a VxD handle (Windows 95/98 compatibility).
	/// </summary>
	[DllModuleExport(4, IsStub = true)]
	private uint OpenVxDHandle(uint hHandle)
	{
		_logger.LogInformation("[Kernel32] OpenVxDHandle(hHandle=0x{HHandle:X8}) - VxD not supported", hHandle);
		// VxD (Virtual Device Driver) is not supported - return invalid handle
		return 0xFFFFFFFF;
	}

	/// <summary>
	/// Establishes a thunk connection between 16-bit and 32-bit code.
	/// </summary>
	[DllModuleExport(20, IsStub = true)]
	private uint ThunkConnect32(uint lpDll16, uint lpDll32, uint hInst, uint lpfnThunk, uint dwReason)
	{
		_logger.LogInformation("[Kernel32] ThunkConnect32(lpDll16=0x{LpDll16:X8}, lpDll32=0x{LpDll32:X8}, hInst=0x{HInst:X8}, lpfnThunk=0x{LpfnThunk:X8}, dwReason={DwReason}) - 16-bit thunk not supported",
			lpDll16, lpDll32, hInst, lpfnThunk, dwReason);
		// Return FALSE - 16-bit thunking not supported
		return 0;
	}

	/// <summary>
	/// Decrements the reference count of a loaded DLL module and terminates the calling thread.
	/// VOID FreeLibraryAndExitThread(
	///   [in] HMODULE hLibModule,
	///   [in] DWORD   dwExitCode
	/// );
	/// </summary>
	[DllModuleExport(10001, IsStub = true)]
	private void FreeLibraryAndExitThread(uint hLibModule, uint dwExitCode)
	{
		_logger.LogInformation("[Kernel32] FreeLibraryAndExitThread(hLibModule=0x{HLibModule:X8}, dwExitCode={DwExitCode})",
			hLibModule, dwExitCode);
		
		// Free the library (stub implementation)
		if (hLibModule != 0)
		{
			// In a full implementation, we would decrement the module reference count
			_logger.LogDebug("[Kernel32] FreeLibraryAndExitThread: Freeing module 0x{HLibModule:X8}", hLibModule);
		}
		
		// Terminate the thread with the specified exit code
		// Note: This never returns, so we don't need a return statement
		TerminateThread(GetCurrentThread(), dwExitCode);
	}

	/// <summary>
	/// Reads data from a file asynchronously.
	/// BOOL ReadFileEx(
	///   [in]      HANDLE                          hFile,
	///   [out]     LPVOID                          lpBuffer,
	///   [in]      DWORD                           nNumberOfBytesToRead,
	///   [in, out] LPOVERLAPPED                    lpOverlapped,
	///   [in]      LPOVERLAPPED_COMPLETION_ROUTINE lpCompletionRoutine
	/// );
	/// </summary>
	[DllModuleExport(10002, IsStub = true)]
	private unsafe uint ReadFileEx(uint hFile, uint lpBuffer, uint nNumberOfBytesToRead, uint lpOverlapped, uint lpCompletionRoutine)
	{
		_logger.LogInformation("[Kernel32] ReadFileEx(hFile=0x{HFile:X8}, lpBuffer=0x{LpBuffer:X8}, nNumberOfBytesToRead={NNumberOfBytesToRead}, lpOverlapped=0x{LpOverlapped:X8}, lpCompletionRoutine=0x{LpCompletionRoutine:X8})",
			hFile, lpBuffer, nNumberOfBytesToRead, lpOverlapped, lpCompletionRoutine);
		
		// If lpOverlapped is NULL, set error - ReadFileEx requires overlapped I/O
		if (lpOverlapped == 0)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}
		
		// For now, use synchronous ReadFile and ignore the completion routine
		// In a full implementation, we would:
		// 1. Queue the read operation
		// 2. Return immediately
		// 3. Call the completion routine when the read completes
		
		// Allocate temporary space for bytes read count
		var bytesReadPtr = _env.SimpleAlloc(4);
		_env.MemWrite32(bytesReadPtr, 0);
		
		var result = ReadFile((void*)hFile, lpBuffer, nNumberOfBytesToRead, bytesReadPtr, lpOverlapped);
		
		// Store the bytes read in the overlapped structure's InternalHigh field (offset 4)
		if (result != 0)
		{
			var bytesRead = _env.MemRead32(bytesReadPtr);
			_env.MemWrite32(lpOverlapped + 4, bytesRead);
		}
		
		return result;
	}

	/// <summary>
	/// Writes data to a file asynchronously.
	/// BOOL WriteFileEx(
	///   [in]      HANDLE                          hFile,
	///   [in]      LPCVOID                         lpBuffer,
	///   [in]      DWORD                           nNumberOfBytesToWrite,
	///   [in, out] LPOVERLAPPED                    lpOverlapped,
	///   [in]      LPOVERLAPPED_COMPLETION_ROUTINE lpCompletionRoutine
	/// );
	/// </summary>
	[DllModuleExport(10003, IsStub = true)]
	private uint WriteFileEx(uint hFile, uint lpBuffer, uint nNumberOfBytesToWrite, uint lpOverlapped, uint lpCompletionRoutine)
	{
		_logger.LogInformation("[Kernel32] WriteFileEx(hFile=0x{HFile:X8}, lpBuffer=0x{LpBuffer:X8}, nNumberOfBytesToWrite={NNumberOfBytesToWrite}, lpOverlapped=0x{LpOverlapped:X8}, lpCompletionRoutine=0x{LpCompletionRoutine:X8})",
			hFile, lpBuffer, nNumberOfBytesToWrite, lpOverlapped, lpCompletionRoutine);
		
		// If lpOverlapped is NULL, set error - WriteFileEx requires overlapped I/O
		if (lpOverlapped == 0)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}
		
		// For now, use synchronous WriteFile and ignore the completion routine
		// In a full implementation, we would:
		// 1. Queue the write operation
		// 2. Return immediately
		// 3. Call the completion routine when the write completes
		
		// Allocate temporary space for bytes written count
		var bytesWrittenPtr = _env.SimpleAlloc(4);
		_env.MemWrite32(bytesWrittenPtr, 0);
		
		var result = WriteFile(hFile, lpBuffer, nNumberOfBytesToWrite, bytesWrittenPtr, lpOverlapped);
		
		// Store the bytes written in the overlapped structure's InternalHigh field (offset 4)
		if (result != 0)
		{
			var bytesWritten = _env.MemRead32(bytesWrittenPtr);
			_env.MemWrite32(lpOverlapped + 4, bytesWritten);
		}
		
		return result;
	}

	/// <summary>
	/// Compares two file times.
	/// LONG CompareFileTime(
	///   [in] const FILETIME *lpFileTime1,
	///   [in] const FILETIME *lpFileTime2
	/// );
	/// </summary>
	/// <returns>
	/// -1 if lpFileTime1 is earlier than lpFileTime2
	///  0 if lpFileTime1 is equal to lpFileTime2
	///  1 if lpFileTime1 is later than lpFileTime2
	/// </returns>
	[DllModuleExport(1)]
	private int CompareFileTime(uint lpFileTime1, uint lpFileTime2)
	{
		_logger.LogInformation("[Kernel32] CompareFileTime(lpFileTime1=0x{LpFileTime1:X8}, lpFileTime2=0x{LpFileTime2:X8})",
			lpFileTime1, lpFileTime2);
		
		if (lpFileTime1 == 0 || lpFileTime2 == 0)
		{
			_logger.LogWarning("[Kernel32] CompareFileTime: NULL pointer");
			return 0;
		}
		
		// Read FILETIME structures
		var ft1 = _env.MemReadStruct<NativeTypes.FILETIME>(lpFileTime1);
		var ft2 = _env.MemReadStruct<NativeTypes.FILETIME>(lpFileTime2);
		
		// Combine into 64-bit values
		var time1 = ((ulong)ft1.dwHighDateTime << 32) | ft1.dwLowDateTime;
		var time2 = ((ulong)ft2.dwHighDateTime << 32) | ft2.dwLowDateTime;
		
		// Compare
		if (time1 < time2)
			return -1;
		if (time1 > time2)
			return 1;
		return 0;
	}

	/// <summary>
	/// Copies a Unicode string to a buffer.
	/// LPWSTR lstrcpyW(
	///   [out] LPWSTR lpString1,
	///   [in]  LPCWSTR lpString2
	/// );
	/// </summary>
	[DllModuleExport(1)]
	private uint LstrcpyW(uint lpString1, uint lpString2)
	{
		if (lpString2 == 0)
		{
			_logger.LogInformation("[Kernel32] LstrcpyW(dest=0x{LpString1:X8}, src=NULL)", lpString1);
			if (lpString1 != 0)
			{
				// Write empty string
				_env.MemWrite16(lpString1, 0);
			}
			return lpString1;
		}
		
		var wstr2 = new LpWStr(lpString2);
		var str2 = wstr2.Read(_env.Memory);
		_logger.LogInformation("[Kernel32] LstrcpyW(dest=0x{LpString1:X8}, src=\"{Str2}\")", lpString1, str2);
		
		if (lpString1 == 0)
		{
			_logger.LogWarning("[Kernel32] LstrcpyW: NULL destination pointer");
			return 0;
		}
		
		// Write the Unicode string to destination
		var wstr1 = new LpWStr(lpString1);
		wstr1.Write(_env.Memory, str2);
		
		return lpString1;
	}

	/// <summary>
	/// Compares two Unicode strings.
	/// int lstrcmpW(
	///   [in] LPCWSTR lpString1,
	///   [in] LPCWSTR lpString2
	/// );
	/// </summary>
	[DllModuleExport(1)]
	private int LstrcmpW(uint lpString1, uint lpString2)
	{
		var wstr1 = new LpWStr(lpString1);
		var wstr2 = new LpWStr(lpString2);
		var str1 = wstr1.Read(_env.Memory);
		var str2 = wstr2.Read(_env.Memory);
		_logger.LogInformation("[Kernel32] LstrcmpW(lpString1=\"{Str1}\", lpString2=\"{Str2}\")", str1, str2);
		return string.Compare(str1, str2, StringComparison.Ordinal);
	}

	/// <summary>
	/// Retrieves the full path and file name of the specified module (Unicode version).
	/// DWORD GetModuleFileNameW(
	///   [in, optional] HMODULE hModule,
	///   [out]          LPWSTR  lpFilename,
	///   [in]           DWORD   nSize
	/// );
	/// </summary>
	[DllModuleExport(12, IsStub = true)]
	private uint GetModuleFileNameW(uint hModule, uint lpFilename, uint nSize)
	{
		_logger.LogInformation("[Kernel32] GetModuleFileNameW called: hModule=0x{HModule:X8} lpFilename=0x{LpFilename:X8} nSize={NSize}", hModule, lpFilename, nSize);

		if (nSize == 0 || lpFilename == 0)
		{
			_logger.LogWarning("[Kernel32] GetModuleFileNameW returning 0 (invalid params)");
			return 0;
		}

		string? path = null;

		if (hModule == 0)
		{
			path = ReadCurrentModulePath();
		}
		else
		{
			var moduleName = _env.GetModuleFileNameForHandle(hModule);
			if (moduleName != null)
			{
				path = moduleName;
			}
			else
			{
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return 0;
			}
		}

		if (path == null)
		{
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}

		path = FixPathEscaping(path);
		var windowsPath = ConvertToWindowsPath(path);
		_logger.LogDebug("[Kernel32] GetModuleFileNameW converted to Windows path: {WindowsPath}", windowsPath);

		// Write Unicode string with size limit
		var charsRequired = (uint)windowsPath.Length;

		// If buffer too small, truncate
		if (nSize <= charsRequired)
		{
			var truncated = windowsPath.Substring(0, (int)(nSize > 0 ? nSize - 1 : 0));
			var wstr = new LpWStr(lpFilename);
			wstr.Write(_env.Memory, truncated, true);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
			_logger.LogDebug("[Kernel32] GetModuleFileNameW truncated; returning {CharsWritten}", nSize > 0 ? nSize - 1 : 0);
			return nSize > 0 ? nSize - 1 : 0;
		}

		// Fits in buffer: write full path
		var wstrFull = new LpWStr(lpFilename);
		wstrFull.Write(_env.Memory, windowsPath, true);

		_logger.LogInformation("[Kernel32] GetModuleFileNameW returning {ReturnLength}", charsRequired);
		return charsRequired;
	}

	/// <summary>
	/// Sets the value of an environment variable for the current process (Unicode version).
	/// BOOL SetEnvironmentVariableW(
	///   [in]           LPCWSTR lpName,
	///   [in, optional] LPCWSTR lpValue
	/// );
	/// </summary>
	[DllModuleExport(8, IsStub = true)]
	private uint SetEnvironmentVariableW(uint lpName, uint lpValue)
	{
		try
		{
			var wstrName = new LpWStr(lpName);
			var name = wstrName.Read(_env.Memory);

			if (string.IsNullOrEmpty(name))
			{
				_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return (uint)NativeTypes.Win32Bool.FALSE;
			}

			// If lpValue is NULL, delete the variable
			if (lpValue == 0)
			{
				_env.SetEnvironmentVariable(name, null);
				_logger.LogInformation("[Kernel32] SetEnvironmentVariableW: Deleted '{Name}'", name);
			}
			else
			{
				var wstrValue = new LpWStr(lpValue);
				var value = wstrValue.Read(_env.Memory);
				_env.SetEnvironmentVariable(name, value);
				_logger.LogInformation("[Kernel32] SetEnvironmentVariableW: Set '{Name}'='{Value}'", name, value);
			}

			return (uint)NativeTypes.Win32Bool.TRUE;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[Kernel32] SetEnvironmentVariableW failed: {ExMessage}", ex.Message);
			_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return (uint)NativeTypes.Win32Bool.FALSE;
		}
	}

	/// <summary>
	/// Reads character input from the console input buffer and removes it from the buffer.
	/// BOOL ReadConsoleA(
	///   [in]            HANDLE  hConsoleInput,
	///   [out]           LPVOID  lpBuffer,
	///   [in]            DWORD   nNumberOfCharsToRead,
	///   [out]           LPDWORD lpNumberOfCharsRead,
	///   [in, optional]  LPVOID  pInputControl
	/// );
	/// </summary>
	[DllModuleExport(20, IsStub = true)]
	private uint ReadConsoleA(uint hConsoleInput, uint lpBuffer, uint nNumberOfCharsToRead, uint lpNumberOfCharsRead, uint pInputControl)
	{
		_logger.LogInformation("[Kernel32] ReadConsoleA(hConsoleInput=0x{HConsoleInput:X8}, lpBuffer=0x{LpBuffer:X8}, nNumberOfCharsToRead={NNumberOfCharsToRead}, lpNumberOfCharsRead=0x{LpNumberOfCharsRead:X8}, pInputControl=0x{PInputControl:X8})",
			hConsoleInput, lpBuffer, nNumberOfCharsToRead, lpNumberOfCharsRead, pInputControl);

		// Stub implementation: Return 0 characters read (no input available)
		// A full implementation would read from a console input buffer
		if (lpNumberOfCharsRead != 0)
		{
			_env.MemWrite32(lpNumberOfCharsRead, 0);
		}

		return (uint)NativeTypes.Win32Bool.TRUE;
	}

	/// <summary>
	/// Retrieves the current control settings for a specified communications device.
	/// BOOL GetCommState(
	///   [in]      HANDLE hFile,
	///   [in, out] LPDCB  lpDCB
	/// );
	/// </summary>
	[DllModuleExport(8, IsStub = true)]
	private uint GetCommState(uint hFile, uint lpDCB)
	{
		_logger.LogInformation("[Kernel32] GetCommState(hFile=0x{HFile:X8}, lpDCB=0x{LpDCB:X8})", hFile, lpDCB);

		// Stub implementation: return FALSE (device not ready)
		// A full implementation would query the communications device settings
		_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
		return (uint)NativeTypes.Win32Bool.FALSE;
	}

	/// <summary>
	/// Configures a communications device according to the specifications in a device-control block.
	/// BOOL SetCommState(
	///   [in] HANDLE hFile,
	///   [in] LPDCB  lpDCB
	/// );
	/// </summary>
	[DllModuleExport(8, IsStub = true)]
	private uint SetCommState(uint hFile, uint lpDCB)
	{
		_logger.LogInformation("[Kernel32] SetCommState(hFile=0x{HFile:X8}, lpDCB=0x{LpDCB:X8})", hFile, lpDCB);

		// Stub implementation: return FALSE (device not ready)
		// A full implementation would configure the communications device
		_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
		return (uint)NativeTypes.Win32Bool.FALSE;
	}

	/// <summary>
	/// Sets the time-out parameters for all read and write operations on a specified communications device.
	/// BOOL SetCommTimeouts(
	///   [in] HANDLE         hFile,
	///   [in] LPCOMMTIMEOUTS lpCommTimeouts
	/// );
	/// </summary>
	[DllModuleExport(8, IsStub = true)]
	private uint SetCommTimeouts(uint hFile, uint lpCommTimeouts)
	{
		_logger.LogInformation("[Kernel32] SetCommTimeouts(hFile=0x{HFile:X8}, lpCommTimeouts=0x{LpCommTimeouts:X8})", hFile, lpCommTimeouts);

		// Stub implementation: return FALSE (device not ready)
		// A full implementation would set the communication timeouts
		_lastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
		return (uint)NativeTypes.Win32Bool.FALSE;
	}

}
