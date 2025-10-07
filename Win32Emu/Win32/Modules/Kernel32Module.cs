using System.Diagnostics;
using System.Text;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
namespace Win32Emu.Win32.Modules;

public class Kernel32Module : IWin32ModuleUnsafe
	{
		private readonly ProcessEnvironment _env;
		private readonly uint _imageBase;
		private readonly PeImageLoader? _peLoader;
		private readonly ILogger _logger;

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

	public void SetDispatcher(Win32Dispatcher dispatcher)
	{
		_dispatcher = dispatcher;
	}

	public unsafe bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var a = new StackArgs(cpu, memory);
		switch (export.ToUpperInvariant())
		{
			// Process / version / module
			case "GETVERSION":
				returnValue = GetVersion();
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
				returnValue = GetAcp();
				return true;
			case "GETCPINFO":
				returnValue = GetCpInfo(a.UInt32(0), a.UInt32(1));
				return true;
			case "GETOEMCP":
				returnValue = GetOemcp();
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
				returnValue = LoadLibraryA(a.Lpstr(0));
				return true;
			case "GETPROCADDRESS":
				returnValue = GetProcAddress(a.UInt32(0), a.UInt32(1));
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
				returnValue = WriteFile((void*)a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
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
				returnValue = WideCharToMultiByte(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5), a.UInt32(6), a.UInt32(7));
				return true;
			case "MULTIBYTETOWIDECHAR":
				returnValue = MultiByteToWideChar(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.Int32(3), a.UInt32(4), a.UInt32(5));
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

			default:
				_logger.LogInformation($"[Kernel32] Unimplemented export: {export}");
				return false;
		}
	}

	[DllModuleExport(23)]
	private unsafe uint GetVersion()
	{
		const ushort build = 950;
		const byte major = 4;
		const byte minor = 0;
		return (major << 8 | minor) << 16 | build;
	}

	[DllModuleExport(48, ForwardedTo = "KERNELBASE.GetVersionEx")]
	private unsafe uint GetVersionEx()
	{
		// This is a forwarded export - the actual implementation is in KERNELBASE.DLL
		// This method will never be called; GetProcAddress will resolve to KERNELBASE
		throw new NotImplementedException("This export is forwarded to KERNELBASE.GetVersionEx");
	}

	[DllModuleExport(14)]
	private unsafe uint GetLastError() => _lastError;

	[DllModuleExport(41)]
	private unsafe uint SetLastError(uint e)
	{
		_lastError = e;
		return 0;
	}

	[DllModuleExport(3)]
	private unsafe uint ExitProcess(uint code)
	{
		_logger.LogInformation($"[Kernel32] ExitProcess({code})");
		_env.RequestExit();
		return 0;
	}

	[DllModuleExport(43)]
	private unsafe uint TerminateProcess(uint hProcess, uint uExitCode)
	{
		// TerminateProcess terminates the specified process
		// hProcess: handle to the process (0xFFFFFFFF for current process)
		// uExitCode: exit code for the process

		_logger.LogInformation($"[Kernel32] TerminateProcess(0x{hProcess:X8}, {uExitCode})");

		// In our emulator, we only support terminating the current process
		if (hProcess is 0xFFFFFFFF or 0)
		{
			_env.RequestExit();
			return NativeTypes.Win32Bool.TRUE;
		}

		// We don't support terminating other processes
		_logger.LogInformation($"[Kernel32] TerminateProcess: Cannot terminate external process handle 0x{hProcess:X8}");
		_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
		return NativeTypes.Win32Bool.FALSE;
	}

	[DllModuleExport(35)]
	private unsafe uint RaiseException(uint dwExceptionCode, uint dwExceptionFlags, uint nNumberOfArguments, uint lpArguments)
	{
		// RaiseException raises a software exception
		// For now, we just log and continue - proper implementation would need exception handling
		_logger.LogInformation($"[Kernel32] RaiseException(code=0x{dwExceptionCode:X8}, flags=0x{dwExceptionFlags:X}, nArgs={nNumberOfArguments}, args=0x{lpArguments:X8})");

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
	private unsafe uint GetCurrentProcess() => 0xFFFFFFFF; // pseudo-handle

	[DllModuleExport(7)]
	private unsafe uint GetAcp() => 1252; // Windows-1252 (Western European)

	[DllModuleExport(9)]
	private unsafe uint GetCpInfo(uint codePage, uint lpCpInfo)
	{
		if (lpCpInfo == 0)
		{
			return NativeTypes.Win32Bool.FALSE; // Return FALSE if null pointer
		}

		// Handle special code page values
		var actualCodePage = codePage switch
		{
			0 => GetAcp(), // CP_ACP - system default Windows ANSI code page
			1 => GetAcp(), // CP_OEMCP - system default OEM code page (we'll use same as ACP)
			_ => codePage
		};

		// We'll support common Western code pages
		switch (actualCodePage)
		{
			case 1252: // Windows-1252 (Western European)
				// Fill CPINFO structure
				_env.MemWrite32(lpCpInfo + 0, 1); // MaxCharSize = 1 (single-byte)
				// Write DefaultChar as bytes - using MemWriteBytes for byte array
				_env.MemWriteBytes(lpCpInfo + 4, new byte[] { 0x3F, 0x00 }); // DefaultChar[0] = '?' (0x3F), DefaultChar[1] = 0
				// LeadByte array - all zeros for single-byte code page (12 bytes)
				_env.MemWriteBytes(lpCpInfo + 6, new byte[12]); // All zeros
				return 1; // TRUE

			case 437: // OEM United States
			case 850: // OEM Multilingual Latin I
			case 1250: // Windows Central Europe
			case 1251: // Windows Cyrillic
			case 28591: // ISO 8859-1 Latin I
				// Similar single-byte code page setup
				_env.MemWrite32(lpCpInfo + 0, 1); // MaxCharSize = 1
				_env.MemWriteBytes(lpCpInfo + 4, new byte[] { 0x3F, 0x00 }); // DefaultChar = '?', 0
				_env.MemWriteBytes(lpCpInfo + 6, new byte[12]); // LeadByte array all zeros
				return NativeTypes.Win32Bool.TRUE;

			default:
				// Unsupported code page
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return NativeTypes.Win32Bool.FALSE;
		}
	}

	[DllModuleExport(17)]
	private unsafe uint GetOemcp() => 437; // IBM PC US (OEM code page)

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
		if (length <= 0 || length > maxStringLengthLimit)
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
			else if (ch <= 0x1F || ch == 0x7F)
			{
				charType |= ctCtype1Cntrl;
			}
			else if (ch is >= punctRange1Start and <= punctRange1End ||
			         ch is >= punctRange2Start and <= punctRange2End ||
			         ch is >= punctRange3Start and <= punctRange3End ||
			         ch is >= punctRange4Start and <= punctRange4End)
			{
				charType |= ctCtype1Punct;
			}

			// Write the character type to the output array (each entry is 2 bytes)
			_env.MemWrite16(lpCharType + (uint)(i * 2), charType);
		}

		return NativeTypes.Win32Bool.TRUE;
	}

	[DllModuleExport(22)]
	private unsafe uint GetStringTypeW(uint locale, uint dwInfoType, uint lpSrcStr, int cchSrc, uint lpCharType)
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
			else if (wchar == ' ' || wchar == '\t' || wchar == '\n' || wchar == '\r')
			{
				charType = ctCtype1Space;
			}
			else
			{
				charType = ctCtype1Alpha; // Default for other characters
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
		var moduleName = lpModuleName.IsNull ? null : _env.ReadAnsiString(lpModuleName.Address);
		_logger.LogInformation($"Getting module handle for '{moduleName ?? "NULL (current process)"}'");
		return _imageBase;
	}

	[DllModuleExport(32)]
	private unsafe uint LoadLibraryA(sbyte* lpLibFileName)
	{
		if (lpLibFileName == null)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}

		// Read the library name from memory
		var libraryName = _env.ReadAnsiString((uint)lpLibFileName);
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
			_logger.LogInformation($"[Kernel32] Loading local DLL for emulation: {libraryName}");

			// Register with dispatcher for function call tracking
			_dispatcher?.RegisterDynamicallyLoadedDll(libraryName);

			if (_peLoader != null)
			{
				return _env.LoadPeImage(localLibraryPath, _peLoader);
			}

			_logger.LogInformation($"[Kernel32] Warning: PeImageLoader not available, falling back to module tracking for {libraryName}");
			return _env.LoadModule(libraryName);
		}

		// DLL is not local - thunk to emulator's win32 syscall implementation
		// For system DLLs like kernel32.dll, user32.dll, etc., we return a fake handle
		// but the actual implementation will be handled by the dispatcher
		_logger.LogInformation($"[Kernel32] Loading system DLL via thunking: {libraryName}");

		// Register with dispatcher for function call tracking
		_dispatcher?.RegisterDynamicallyLoadedDll(libraryName);

		// For system libraries, we still need to track them but mark them as system modules
		return _env.LoadModule(libraryName);
	}

	[DllModuleExport(18)]
	private unsafe uint GetProcAddress(uint hModule, uint lpProcName)
	{
		// GetProcAddress retrieves the address of an exported function from a DLL
		// hModule: module handle from LoadLibraryA or GetModuleHandleA
		// lpProcName: either a string pointer (name) or an ordinal value (LOWORD)

		_logger.LogInformation($"[Kernel32] GetProcAddress(0x{hModule:X8}, 0x{lpProcName:X8})");

		if (hModule == 0)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}

		string? procName = null;
		var byOrdinal = false;

		// Check if lpProcName is an ordinal (high word is 0)
		uint ordinal = 0;
		if ((lpProcName & 0xFFFF0000) == 0)
		{
			ordinal = lpProcName & 0xFFFF;
			byOrdinal = true;
			_logger.LogInformation($"[Kernel32] GetProcAddress: Looking up by ordinal {ordinal}");
		}
		else
		{
			// It's a string pointer
			procName = _env.ReadAnsiString(lpProcName);
			_logger.LogInformation($"[Kernel32] GetProcAddress: Looking up '{procName}'");
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
					_logger.LogInformation($"[Kernel32] GetProcAddress: Found export by ordinal {ordinal} at 0x{exportAddress:X8}");
					return exportAddress;
				}
				
				// Check if it's a forwarded export
				if (loadedImage.ForwardedExportsByOrdinal.TryGetValue(ordinal, out forwarderName))
				{
					_logger.LogInformation($"[Kernel32] GetProcAddress: Found forwarded export by ordinal {ordinal} -> {forwarderName}");
					return ResolveForwardedExport(forwarderName);
				}
			}
			else if (procName != null)
			{
				if (loadedImage.ExportsByName.TryGetValue(procName, out exportAddress))
				{
					_logger.LogInformation($"[Kernel32] GetProcAddress: Found export '{procName}' at 0x{exportAddress:X8}");
					return exportAddress;
				}
				
				// Check if it's a forwarded export
				if (loadedImage.ForwardedExportsByName.TryGetValue(procName, out forwarderName))
				{
					_logger.LogInformation($"[Kernel32] GetProcAddress: Found forwarded export '{procName}' -> {forwarderName}");
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
			_logger.LogInformation($"[Kernel32] GetProcAddress: Module handle 0x{hModule:X8} not recognized");
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
			return 0;
		}

		// Try to get the emulated module from the dispatcher
		if (_dispatcher == null || !_dispatcher.TryGetModule(moduleName, out var emulatedModule) || emulatedModule == null)
		{
			_logger.LogInformation($"[Kernel32] GetProcAddress: Emulated module '{moduleName}' not found in dispatcher");
			_lastError = NativeTypes.Win32Error.ERROR_MOD_NOT_FOUND;
			return 0;
		}

		// Use DllModuleExportInfo to check if the export exists before looking up
		string? exportName = null;
		
		if (byOrdinal)
		{
			// Get export ordinals for this emulated module
			var exportOrdinals = emulatedModule.GetExportOrdinals();
			
			// Find export by ordinal
			var exportEntry = exportOrdinals.FirstOrDefault(kvp => kvp.Value == ordinal);
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
			_logger.LogInformation($"[Kernel32] GetProcAddress: Export not found in emulated module '{moduleName}'");
			_lastError = NativeTypes.Win32Error.ERROR_PROC_NOT_FOUND;
			return 0;
		}

		// Check if this export is forwarded to another DLL
		var forwardedTo = DllModuleExportInfo.GetForwardedExport(moduleName, exportName);
		if (forwardedTo != null)
		{
			_logger.LogInformation($"[Kernel32] GetProcAddress: Found forwarded export '{moduleName}!{exportName}' -> {forwardedTo}");
			return ResolveForwardedExport(forwardedTo);
		}

		// Register and return a synthetic export address
		var syntheticAddress = _env.RegisterSyntheticExport(moduleName, exportName);
		_logger.LogInformation($"[Kernel32] GetProcAddress: Registered synthetic export '{moduleName}!{exportName}' at 0x{syntheticAddress:X8}");
		return syntheticAddress;
	}

	/// <summary>
	/// Resolves a forwarded export to its actual address.
	/// Forwarded exports have the format "DLL.ExportName" or "DLL.DLL.ExportName".
	/// </summary>
	private unsafe uint ResolveForwardedExport(string forwarderName)
	{
		// Parse the forwarder string (format: "DLL.ExportName" or "DLL.DLL.ExportName")
		var parts = forwarderName.Split('.');
		if (parts.Length < 2)
		{
			_logger.LogInformation($"[Kernel32] ResolveForwardedExport: Invalid forwarder format '{forwarderName}'");
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

		_logger.LogInformation($"[Kernel32] ResolveForwardedExport: Resolving '{forwarderName}' -> {targetDll}!{targetExport}");

		// Try to get the target module handle
		var targetModuleHandle = _env.LoadModule(targetDll);
		if (targetModuleHandle == 0)
		{
			_logger.LogInformation($"[Kernel32] ResolveForwardedExport: Failed to load target module '{targetDll}'");
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
		Diagnostics.Diagnostics.LogDebug($"GetModuleFileNameA called: h=0x{(uint)(nint)h:X8} lp=0x{(uint)(nint)lp:X8} n={n}");
		// Use guest memory helpers instead of dereferencing raw pointers to avoid AccessViolation
		if (n == 0 || lp == null)
		{
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

		Diagnostics.Diagnostics.LogDebug($"GetModuleFileNameA resolved path: {path}");

		var bytes = Encoding.ASCII.GetBytes(path);
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
			Diagnostics.Diagnostics.LogDebug($"GetModuleFileNameA truncated; copyLen={copyLen} returned");
			return copyLen;
		}

		// Fits in buffer: write full path and null terminator
		_env.MemWriteBytes(lpAddr, bytes);
		_env.MemWriteBytes(lpAddr + (uint)bytes.Length, [0]);
		Diagnostics.Diagnostics.LogMemWrite(lpAddr, bytes.Length + 1, bytes.AsSpan(0, bytes.Length).ToArray());
		return (uint)bytes.Length;
	}

	[DllModuleExport(8)]
	private unsafe uint GetCommandLineA() => _env.CommandLinePtr;

	[DllModuleExport(12)]
	private unsafe uint GetEnvironmentStringsW()
	{
		// Return pointer to Unicode environment strings block
		// This will be obtained from emulated environment variables, not system ones
		return _env.GetEnvironmentStringsW();
	}

	[DllModuleExport(6)]
	private unsafe uint FreeEnvironmentStringsW(uint lpszEnvironmentBlock)
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
	private unsafe uint GetEnvironmentStringsA()
	{
		// Return pointer to ANSI environment strings block
		// This will be obtained from emulated environment variables, not system ones
		return _env.GetEnvironmentStringsA();
	}

	[DllModuleExport(5)]
	private unsafe uint FreeEnvironmentStringsA(uint lpszEnvironmentBlock)
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
	private unsafe uint GetStartupInfoA(uint lpStartupInfo)
	{
		if (lpStartupInfo == 0)
		{
			return 0;
		}

		_env.MemZero(lpStartupInfo, 68);
		_env.MemWrite32(lpStartupInfo + 0, 68);
		_env.MemWrite32(lpStartupInfo + 56, _env.StdInputHandle);
		_env.MemWrite32(lpStartupInfo + 60, _env.StdOutputHandle);
		_env.MemWrite32(lpStartupInfo + 64, _env.StdErrorHandle);
		return 0;
	}

	[DllModuleExport(20)]
	private unsafe uint GetStdHandle(uint nStdHandle)
	{
		return nStdHandle switch
		{
			0xFFFFFFF6 => _env.StdInputHandle,
			0xFFFFFFF5 => _env.StdOutputHandle,
			0xFFFFFFF4 => _env.StdErrorHandle,
			_ => 0
		};
	}

	[DllModuleExport(42)]
	private unsafe uint SetStdHandle(uint nStdHandle, uint hHandle)
	{
		switch (nStdHandle)
		{
			case 0xFFFFFFF6: _env.StdInputHandle = hHandle; break;
			case 0xFFFFFFF5: _env.StdOutputHandle = hHandle; break;
			case 0xFFFFFFF4: _env.StdErrorHandle = hHandle; break;
		}

		return 1;
	}

	[DllModuleExport(24)]
	private unsafe uint GlobalAlloc(uint flags, uint bytes) => _env.SimpleAlloc(bytes == 0 ? 1u : bytes);
	[DllModuleExport(25)]
	private static unsafe uint GlobalFree(void* h) => 0;

	private static unsafe uint GlobalLock(void* hMem)
	{
		// GlobalLock locks a global memory object and returns a pointer to it
		// In our simplified implementation, we just return the handle as a pointer
		// since memory is already accessible
		return (uint)hMem;
	}

	private static unsafe uint GlobalUnlock(void* hMem)
	{
		// GlobalUnlock decrements the lock count
		// Returns TRUE (1) if still locked, FALSE (0) if unlocked
		// In our simplified implementation, always return TRUE
		return NativeTypes.Win32Bool.TRUE;
	}

	private static unsafe uint GlobalHandle(void* pMem)
	{
		// GlobalHandle retrieves the handle associated with a locked memory pointer
		// In our simplified implementation, the handle is the same as the pointer
		return (uint)pMem;
	}

	[DllModuleExport(27)]
	private unsafe uint HeapCreate(uint flOptions, uint dwInitialSize, uint dwMaximumSize) =>
		_env.HeapCreate(flOptions, dwInitialSize, dwMaximumSize);

	[DllModuleExport(26)]
	private unsafe uint HeapAlloc(void* hHeap, uint dwFlags, uint dwBytes) => _env.HeapAlloc((uint)hHeap, dwBytes);
	[DllModuleExport(29)]
	private static unsafe uint HeapFree(void* hHeap, uint dwFlags, void* lpMem) => 1;

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
				_logger.LogInformation($"[Kernel32] HeapReAlloc: lpMem is null, allocated new block at 0x{alloc:X8}, size={dwBytes}");
				return alloc;
			}

			// Get the size of the original allocation
			uint originalSize = _env.HeapSize((uint)hHeap, (uint)lpMem);
			if (originalSize == 0)
			{
				// If we don't have size info, this might be an invalid pointer
				_logger.LogWarning($"[Kernel32] HeapReAlloc: Could not determine size of block at 0x{(uint)lpMem:X8}");
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
			uint bytesToCopy = Math.Min(originalSize, dwBytes);
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

			_logger.LogInformation($"[Kernel32] HeapReAlloc: Reallocated from 0x{(uint)lpMem:X8} (size={originalSize}) to 0x{newMem:X8} (size={dwBytes}), copied {bytesToCopy} bytes");
			return newMem;
		}
		catch (Exception ex)
		{
			_logger.LogError($"[Kernel32] HeapReAlloc failed: {ex.Message}");
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
		_logger.LogInformation($"[Kernel32] HeapDestroy(0x{(uint)(nint)hHeap:X8})");

		if (hHeap == null)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}

		return NativeTypes.Win32Bool.TRUE;
	}

	[DllModuleExport(45)]
	private unsafe uint VirtualAlloc(uint lpAddress, uint dwSize, uint flAllocationType, uint flProtect) =>
		_env.VirtualAlloc(lpAddress, dwSize, flAllocationType, flProtect);

	[DllModuleExport(46)]
	private unsafe uint VirtualFree(uint lpAddress, uint dwSize, uint dwFreeType)
	{
		// VirtualFree releases or decommits virtual memory
		// dwFreeType: MEM_DECOMMIT (0x4000) or MEM_RELEASE (0x8000)
		// For simplicity in our emulator, we accept the call but don't actually free memory
		// The bump allocator doesn't support freeing
		_logger.LogInformation($"[Kernel32] VirtualFree(0x{lpAddress:X8}, {dwSize}, 0x{dwFreeType:X})");

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

	// File I/O implementations
	[DllModuleExport(2)]
	private unsafe uint CreateFileA(uint lpFileName, uint dwDesiredAccess, uint dwShareMode, uint lpSecAttr,
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

			var mode = FileMode.OpenOrCreate;
			switch (dwCreationDisposition)
			{
				case 1: mode = FileMode.CreateNew; break; // CREATE_NEW
				case 2: mode = FileMode.Create; break; // CREATE_ALWAYS
				case 3: mode = FileMode.Open; break; // OPEN_EXISTING
				case 4: mode = FileMode.OpenOrCreate; break; // OPEN_ALWAYS
				case 5: mode = FileMode.Truncate; break; // TRUNCATE_EXISTING
			}

			var access = FileAccess.ReadWrite;
			if ((dwDesiredAccess & 0x40000000) != 0 && (dwDesiredAccess & 0x80000000) == 0)
			{
				access = FileAccess.Read; // GENERIC_READ
			}

			if ((dwDesiredAccess & 0x80000000) != 0 && (dwDesiredAccess & 0x40000000) == 0)
			{
				access = FileAccess.Write; // GENERIC_WRITE
			}

			var fs = new FileStream(path, mode, access, FileShare.ReadWrite);
			return _env.RegisterHandle(fs);
		}
		catch (Exception ex)
		{
			_logger.LogInformation($"[Kernel32] CreateFileA failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return NativeTypes.Win32Handle.INVALID_HANDLE_VALUE;
		}
	}

	[DllModuleExport(36)]
	private unsafe uint ReadFile(void* hFile, uint lpBuffer, uint nNumberOfBytesToRead, uint lpNumberOfBytesRead,
		uint lpOverlapped)
	{
		if (!_env.TryGetHandle<FileStream>((uint)hFile, out var fs) || fs is null)
		{
			return 0;
		}

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
			_logger.LogInformation($"[Kernel32] ReadFile failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
			return NativeTypes.Win32Bool.FALSE;
		}
	}

	[DllModuleExport(48)]
	private unsafe uint WriteFile(void* hFile, uint lpBuffer, uint nNumberOfBytesToWrite, uint lpNumberOfBytesWritten,
		uint lpOverlapped)
	{
		var handle = (uint)hFile;
		
		// Handle standard handles specially
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
				_logger.LogInformation($"[Kernel32] WriteFile to standard handle failed: {ex.Message}");
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
				return NativeTypes.Win32Bool.FALSE;
			}
		}
		
		// Handle regular file handles
		if (!_env.TryGetHandle<FileStream>(handle, out var fs) || fs is null)
		{
			return 0;
		}

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
			_logger.LogInformation($"[Kernel32] WriteFile failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
			return NativeTypes.Win32Bool.FALSE;
		}
	}

	[DllModuleExport(1)]
	private unsafe uint CloseHandle(void* hObject)
	{
		var h = (uint)hObject;
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
		
		// Standard handles are character devices (console)
		if (handle == _env.StdInputHandle || handle == _env.StdOutputHandle || handle == _env.StdErrorHandle)
		{
			return 0x0002; // FILE_TYPE_CHAR (character device like console)
		}
		
		if (_env.TryGetHandle<FileStream>(handle, out var fs) && fs is not null)
		{
			return 0x0001; // FILE_TYPE_DISK
		}

		return 0; // FILE_TYPE_UNKNOWN
	}

	[DllModuleExport(39)]
	private unsafe uint SetFilePointer(void* hFile, uint lDistanceToMove, uint lpDistanceToMoveHigh, uint dwMoveMethod)
	{
		if (!_env.TryGetHandle<FileStream>((uint)hFile, out var fs) || fs is null)
		{
			return 0xFFFFFFFF;
		}

		var origin = dwMoveMethod switch
		{
			0 => SeekOrigin.Begin, 1 => SeekOrigin.Current, 2 => SeekOrigin.End, _ => SeekOrigin.Begin
		};
		long dist = (int)lDistanceToMove; // ignore high for now
		var pos = fs.Seek(dist, origin);
		return (uint)pos;
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
		
		if (_env.TryGetHandle<FileStream>(handle, out var fs) && fs is not null)
		{
			fs.Flush(true);
			return 1;
		}

		return 0;
	}

	[DllModuleExport(38)]
	private unsafe uint SetEndOfFile(void* hFile)
	{
		if (_env.TryGetHandle<FileStream>((uint)hFile, out var fs) && fs is not null)
		{
			fs.SetLength(fs.Position);
			return 1;
		}

		return 0;
	}

	private unsafe uint DeleteFileA(uint lpFileName)
	{
		try
		{
			var path = _env.ReadAnsiString(lpFileName);
			if (string.IsNullOrEmpty(path))
			{
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return NativeTypes.Win32Bool.FALSE;
			}

			File.Delete(path);
			_logger.LogInformation($"[Kernel32] DeleteFileA: Deleted '{path}'");
			return NativeTypes.Win32Bool.TRUE;
		}
		catch (Exception ex)
		{
			_logger.LogInformation($"[Kernel32] DeleteFileA failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return NativeTypes.Win32Bool.FALSE;
		}
	}

	private unsafe uint MoveFileA(uint lpExistingFileName, uint lpNewFileName)
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

			File.Move(existingPath, newPath);
			_logger.LogInformation($"[Kernel32] MoveFileA: Moved '{existingPath}' to '{newPath}'");
			return NativeTypes.Win32Bool.TRUE;
		}
		catch (Exception ex)
		{
			_logger.LogInformation($"[Kernel32] MoveFileA failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return NativeTypes.Win32Bool.FALSE;
		}
	}

	// Simple structure to hold find file data
	private class FindFileHandle
	{
		public string SearchPattern { get; set; } = "";
		public string[] Files { get; set; } = Array.Empty<string>();
		public int CurrentIndex { get; set; } = 0;
	}

	private readonly Dictionary<uint, FindFileHandle> _findFileHandles = new();
	private uint _nextFindFileHandle = 0x1000;

	// Helper method to write WIN32_FIND_DATAA structure
	private unsafe void WriteFindData(uint lpFindFileData, string fileName)
	{
		var fileNameBytes = Encoding.ASCII.GetBytes(fileName);
		
		// Clear the structure
		var zeroBuffer = new byte[320];
		_env.MemWriteBytes(lpFindFileData, zeroBuffer);
		
		// Write filename at offset 44 (cFileName field), ensure null-terminated and max 260 bytes
		var cFileNameBytes = new byte[260];
		int copyLen = Math.Min(fileNameBytes.Length, 259); // leave room for null terminator
		Array.Copy(fileNameBytes, 0, cFileNameBytes, 0, copyLen);
		cFileNameBytes[copyLen] = 0; // explicit null terminator
		_env.MemWriteBytes(lpFindFileData + 44, cFileNameBytes);
	}

	private unsafe uint FindFirstFileA(uint lpFileName, uint lpFindFileData)
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

			var files = Directory.GetFiles(dir, pattern);
			
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
			
			_logger.LogInformation($"[Kernel32] FindFirstFileA: Found '{fileName}' for pattern '{searchPattern}'");
			_findFileHandles[handle].CurrentIndex = 1;
			
			return handle;
		}
		catch (Exception ex)
		{
			_logger.LogInformation($"[Kernel32] FindFirstFileA failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return NativeTypes.Win32Handle.INVALID_HANDLE_VALUE;
		}
	}

	private unsafe uint FindNextFileA(uint hFindFile, uint lpFindFileData)
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
			
			_logger.LogInformation($"[Kernel32] FindNextFileA: Found '{fileName}'");
			handle.CurrentIndex++;
			
			return NativeTypes.Win32Bool.TRUE;
		}
		catch (Exception ex)
		{
			_logger.LogInformation($"[Kernel32] FindNextFileA failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return NativeTypes.Win32Bool.FALSE;
		}
	}

	private unsafe uint FindClose(void* hFindFile)
	{
		var handle = (uint)hFindFile;
		if (_findFileHandles.Remove(handle))
		{
			_logger.LogInformation($"[Kernel32] FindClose: Closed handle 0x{handle:X8}");
			return NativeTypes.Win32Bool.TRUE;
		}

		_lastError = NativeTypes.Win32Error.ERROR_INVALID_HANDLE;
		return NativeTypes.Win32Bool.FALSE;
	}

	private unsafe uint FileTimeToSystemTime(uint lpFileTime, uint lpSystemTime)
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
			_logger.LogInformation($"[Kernel32] FileTimeToSystemTime failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}
	}

	private unsafe uint FileTimeToLocalFileTime(uint lpFileTime, uint lpLocalFileTime)
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
			_logger.LogInformation($"[Kernel32] FileTimeToLocalFileTime failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}
	}

	private unsafe uint GetTimeZoneInformation(uint lpTimeZoneInformation)
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
			
			_logger.LogInformation($"[Kernel32] GetTimeZoneInformation: Bias={bias} minutes");
			
			// Return TIME_ZONE_ID_UNKNOWN (0)
			return 0;
		}
		catch (Exception ex)
		{
			_logger.LogInformation($"[Kernel32] GetTimeZoneInformation failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0xFFFFFFFF; // TIME_ZONE_ID_INVALID
		}
	}

	private unsafe uint SetEnvironmentVariableA(uint lpName, uint lpValue)
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
				_logger.LogInformation($"[Kernel32] SetEnvironmentVariableA: Deleted '{name}'");
			}
			else
			{
				var value = _env.ReadAnsiString(lpValue);
				Environment.SetEnvironmentVariable(name, value);
				_logger.LogInformation($"[Kernel32] SetEnvironmentVariableA: Set '{name}'='{value}'");
			}

			return NativeTypes.Win32Bool.TRUE;
		}
		catch (Exception ex)
		{
			_logger.LogInformation($"[Kernel32] SetEnvironmentVariableA failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}
	}

	[DllModuleExport(40)]
	private unsafe uint SetHandleCount(uint uNumber)
	{
		// SetHandleCount is a legacy function from 16-bit Windows
		// In Win32, it's essentially a no-op that returns the requested count
		// Modern systems ignore this and have much higher handle limits
		return uNumber; // Return the requested number as if it was successfully set
	}

	[DllModuleExport(44)]
	private unsafe uint UnhandledExceptionFilter(uint exceptionInfo)
	{
		// UnhandledExceptionFilter processes unhandled exceptions
		// exceptionInfo is a pointer to an EXCEPTION_POINTERS structure
		_logger.LogInformation($"[Kernel32] UnhandledExceptionFilter called with exceptionInfo=0x{exceptionInfo:X8}");

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

				_logger.LogInformation($"[Kernel32]   ExceptionRecord: 0x{exceptionRecordPtr:X8}");
				_logger.LogInformation($"[Kernel32]   ContextRecord: 0x{contextRecordPtr:X8}");

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

					_logger.LogInformation($"[Kernel32]     ExceptionCode: 0x{exceptionCode:X8}");
					_logger.LogInformation($"[Kernel32]     ExceptionFlags: 0x{exceptionFlags:X8}");
					_logger.LogInformation($"[Kernel32]     ExceptionAddress: 0x{exceptionAddress:X8}");
				}
			}
			catch (Exception ex)
			{
				_logger.LogInformation($"[Kernel32] Error reading exception info: {ex.Message}");
			}
		}

		// For the emulator, we'll return EXCEPTION_EXECUTE_HANDLER to terminate the process
		// This is the safest default behavior for unhandled exceptions in an emulated environment
		return NativeTypes.ExceptionHandling.EXCEPTION_EXECUTE_HANDLER;
	}

	[DllModuleExport(47)]
	private unsafe uint WideCharToMultiByte(
		uint codePage,
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
			// Handle null input string
			if (lpWideCharStr == 0)
			{
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return 0;
			}

			// Handle special code page values
			var actualCodePage = codePage switch
			{
				0 => GetAcp(), // CP_ACP - system default Windows ANSI code page
				1 => GetOemcp(), // CP_OEMCP - system default OEM code page
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
			}

			// Convert to multi-byte string based on code page
			byte[] multiByteBytes;
			switch (actualCodePage)
			{
				case 1252: // Windows-1252 (Western European)
				case 28591: // ISO 8859-1 (Latin-1)
					// Both Windows-1252 and ISO 8859-1 are single-byte encodings
					// For compatibility with InvariantGlobalization, use Latin1 fallback
					multiByteBytes = Encoding.Latin1.GetBytes(wideString);
					break;
				case 437: // OEM US
				case 850: // OEM Latin-1  
				case 1250: // Windows Central Europe
				case 1251: // Windows Cyrillic
					// For other single-byte code pages, fallback to UTF-8 since Latin1 may not cover all characters
					// This provides better Unicode support even if not 100% code page accurate
					multiByteBytes = Encoding.UTF8.GetBytes(wideString);
					break;
				case 65001: // UTF-8
					multiByteBytes = Encoding.UTF8.GetBytes(wideString);
					break;
				default:
					// Unsupported code page
					_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
					return 0;
			}

			// If cbMultiByte is 0, return required buffer size
			if (cbMultiByte == 0)
			{
				// If input is null-terminated, include space for null terminator in required size
				if (cchWideChar == unchecked((uint)-1))
				{
					return (uint)(multiByteBytes.Length + 1);
				}

				return (uint)multiByteBytes.Length;
			}

			// Check if output buffer is large enough
			if (multiByteBytes.Length > cbMultiByte)
			{
				_lastError = NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
				return 0;
			}

			// Copy converted bytes to output buffer
			if (lpMultiByteStr != 0)
			{
				_env.MemWriteBytes(lpMultiByteStr, multiByteBytes);
			}

			// Clear the "used default char" flag if provided
			if (lpUsedDefaultChar != 0)
			{
				_env.MemWrite32(lpUsedDefaultChar, 0); // FALSE - no default char used (simplified)
			}

			return (uint)multiByteBytes.Length;
		}
		catch (Exception ex)
		{
			_logger.LogInformation($"[Kernel32] WideCharToMultiByte failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}
	}

	[DllModuleExport(33)]
	private unsafe uint MultiByteToWideChar(uint codePage, uint dwFlags, uint lpMultiByteStr, int cbMultiByte, uint lpWideCharStr, uint cchWideChar)
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
			if (codePage != 0 && codePage != 1 && codePage != 1252 && codePage != 437 && codePage != 65001)
			{
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return 0;
			}

			// Use CP_ACP (1252) as default
			if (codePage == 0 || codePage == 1)
			{
				codePage = 1252;
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
				65001 => Encoding.UTF8, // UTF-8
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
			_logger.LogInformation($"[Kernel32] MultiByteToWideChar failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}
	}

	[DllModuleExport(30)]
	private unsafe uint LcMapStringA(uint locale, uint dwMapFlags, uint lpSrcStr, int cchSrc, uint lpDestStr, int cchDest)
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
			_logger.LogInformation($"[Kernel32] LCMapStringA failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}
	}

	[DllModuleExport(31)]
	private unsafe uint LcMapStringW(uint locale, uint dwMapFlags, uint lpSrcStr, int cchSrc, uint lpDestStr, int cchDest)
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
			_logger.LogInformation($"[Kernel32] LCMapStringW failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}
	}

	private unsafe uint CompareStringA(uint locale, uint dwCmpFlags, uint lpString1, int cchCount1, uint lpString2, int cchCount2)
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
			
			_logger.LogInformation($"[Kernel32] CompareStringA: '{str1}' vs '{str2}' = {result}");
			
			if (result < 0) return cstrLessThan;
			if (result > 0) return cstrGreaterThan;
			return cstrEqual;
		}
		catch (Exception ex)
		{
			_logger.LogInformation($"[Kernel32] CompareStringA failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}
	}

	private unsafe uint CompareStringW(uint locale, uint dwCmpFlags, uint lpString1, int cchCount1, uint lpString2, int cchCount2)
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
			
			_logger.LogInformation($"[Kernel32] CompareStringW: '{str1}' vs '{str2}' = {result}");
			
			if (result < 0) return cstrLessThan;
			if (result > 0) return cstrGreaterThan;
			return cstrEqual;
		}
		catch (Exception ex)
		{
			_logger.LogInformation($"[Kernel32] CompareStringW failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}
	}

	[DllModuleExport(34)]
	private unsafe uint QueryPerformanceCounter(uint lpPerformanceCount)
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
		catch
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}
	}

	private unsafe uint QueryPerformanceFrequency(uint lpFrequency)
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

			_logger.LogInformation($"[Kernel32] QueryPerformanceFrequency: {frequency} Hz");
			return NativeTypes.Win32Bool.TRUE;
		}
		catch
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}
	}

	private uint GetTickCount()
	{
		// GetTickCount returns the number of milliseconds since system start
		// In an emulator context, we use the time since the emulator started
		// Returns a 32-bit value that wraps around to zero after ~49.7 days
		
		// Use Environment.TickCount which is designed for this exact purpose
		var tickCount = (uint)Environment.TickCount;
		
		_logger.LogInformation($"[Kernel32] GetTickCount: {tickCount} ms");
		return tickCount;
	}

	private unsafe uint GetTickCount64(uint lpTickCount)
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
			
			_logger.LogInformation($"[Kernel32] GetTickCount64: {tickCount64} ms");
			return 1; // Success (non-zero return)
		}
		catch
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}
	}

	private uint Sleep(uint dwMilliseconds)
	{
		// Sleep suspends execution for a specified interval
		// In an emulator, we can either:
		// 1. Actually sleep (blocking) - not ideal for emulation
		// 2. Just acknowledge the call without sleeping
		// 3. Use a minimal sleep for 0 (yield) cases
		
		if (dwMilliseconds == 0)
		{
			// Sleep(0) means "yield to other threads"
			// In our single-threaded emulator, this is effectively a no-op
			_logger.LogInformation("[Kernel32] Sleep(0): yielding");
			Thread.Yield();
		}
		else if (dwMilliseconds == 0xFFFFFFFF) // INFINITE
		{
			_logger.LogWarning("[Kernel32] Sleep(INFINITE): treating as 1ms sleep");
			// Don't actually sleep forever - that would hang the emulator
			// Just sleep for a short time
			Thread.Sleep(1);
		}
		else
		{
			_logger.LogInformation($"[Kernel32] Sleep: {dwMilliseconds} ms");
			// For actual sleep durations, we need to decide:
			// - Sleeping blocks emulation which may not be desired
			// - Not sleeping means game timing could be wrong
			// For now, use a minimal sleep to avoid busy-waiting
			if (dwMilliseconds > 100)
			{
				// Long sleeps - use a fraction of the requested time
				Thread.Sleep((int)(dwMilliseconds / 10));
			}
			else if (dwMilliseconds > 0)
			{
				// Short sleeps - minimal delay
				Thread.Sleep(1);
			}
		}
		
		return 0; // Sleep doesn't return a value (void function)
	}

	private unsafe string ReadCurrentModulePath()
	{
		// Prefer the initialized executable path from the process environment
		if (!string.IsNullOrEmpty(_env.ExecutablePath))
		{
			return _env.ExecutablePath;
		}

		// Fall back to the module filename pointer if available
		try
		{
			if (_env.ModuleFileNamePtr != 0)
			{
				var s = _env.ReadAnsiString(_env.ModuleFileNamePtr);
				if (!string.IsNullOrEmpty(s))
				{
					return s;
				}
			}
		}
		catch
		{
			// ignore and fall through to default
		}

		// Final fallback for legacy behavior
		return "game.exe";
	}

	[DllModuleExport(37)]
	private unsafe uint RtlUnwind(uint targetFrame, uint targetIp, uint exceptionRecord, uint returnValue)
	{
		// RtlUnwind is used for structured exception handling to unwind the stack
		// In a real implementation, this would:
		// 1. Walk the stack from current frame to targetFrame
		// 2. Call exception handlers with EXCEPTION_UNWIND flag
		// 3. Restore processor state
		// 4. Jump to targetIp with returnValue in EAX

		// For the Win32Emu, we implement a minimal version that:
		// - Simply logs the unwind operation
		// - Sets the target IP if provided
		// - Returns success

		_logger.LogInformation($"[Kernel32] RtlUnwind called: targetFrame=0x{targetFrame:X8}, targetIp=0x{targetIp:X8}, exceptionRecord=0x{exceptionRecord:X8}, returnValue=0x{returnValue:X8}");

		// If a target IP is specified and it's not null, we would typically:
		// - Unwind the stack to the target frame
		// - Set EIP to targetIp
		// - Set EAX to returnValue
		// However, in this emulator context, we'll leave the actual stack unwinding
		// to be handled by the calling code/exception handling mechanism

		if (targetIp != 0)
		{
			_logger.LogInformation($"[Kernel32] RtlUnwind: Would jump to 0x{targetIp:X8} with return value 0x{returnValue:X8}");
			// In a full implementation, we would modify the CPU state here
			// For now, we just log the intended operation
		}

		// RtlUnwind doesn't return a value in the traditional sense - it either succeeds
		// or raises an exception. We'll return 0 to indicate success.
		return 0;
	}

	// Thread management and TLS functions
	private unsafe uint CreateThread(uint lpThreadAttributes, uint dwStackSize, uint lpStartAddress, 
		uint lpParameter, uint dwCreationFlags, uint lpThreadId)
	{
		_logger.LogInformation($"[Kernel32] CreateThread(attr=0x{lpThreadAttributes:X8}, stack=0x{dwStackSize:X8}, " +
			$"start=0x{lpStartAddress:X8}, param=0x{lpParameter:X8}, flags=0x{dwCreationFlags:X8}, outId=0x{lpThreadId:X8})");

		// For now, we do simple thread emulation - we don't actually create threads
		// Just allocate a thread ID and return a handle
		var threadId = _env.CreateThread();
		
		// If lpThreadId is not null, write the thread ID to it
		if (lpThreadId != 0)
		{
			_env.MemWrite32(lpThreadId, threadId);
		}
		
		// Return a pseudo thread handle (just use the thread ID as the handle)
		return threadId;
	}

	private unsafe uint GetCurrentThreadId()
	{
		var threadId = _env.GetCurrentThreadId();
		_logger.LogInformation($"[Kernel32] GetCurrentThreadId() = {threadId}");
		return threadId;
	}

	private unsafe uint TlsAlloc()
	{
		var index = _env.TlsAlloc();
		_logger.LogInformation($"[Kernel32] TlsAlloc() = {index}");
		return index;
	}

	private unsafe uint TlsGetValue(uint dwTlsIndex)
	{
		var value = _env.TlsGetValue(dwTlsIndex);
		_logger.LogInformation($"[Kernel32] TlsGetValue({dwTlsIndex}) = 0x{value:X8}");
		return value;
	}

	private unsafe uint TlsSetValue(uint dwTlsIndex, uint lpTlsValue)
	{
		var success = _env.TlsSetValue(dwTlsIndex, lpTlsValue);
		_logger.LogInformation($"[Kernel32] TlsSetValue({dwTlsIndex}, 0x{lpTlsValue:X8}) = {success}");
		return success ? NativeTypes.Win32Bool.TRUE : NativeTypes.Win32Bool.FALSE;
	}

	private unsafe uint TlsFree(uint dwTlsIndex)
	{
		var success = _env.TlsFree(dwTlsIndex);
		_logger.LogInformation($"[Kernel32] TlsFree({dwTlsIndex}) = {success}");
		return success ? NativeTypes.Win32Bool.TRUE : NativeTypes.Win32Bool.FALSE;
	}

	public Dictionary<string, uint> GetExportOrdinals()
	{
		// Auto-generated from [DllModuleExport] attributes
		return DllModuleExportInfo.GetAllExports("KERNEL32.DLL");
	}
}