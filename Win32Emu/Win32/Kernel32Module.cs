using Win32Emu.Cpu;
using Win32Emu.Memory;
using Win32Emu.Loader;

namespace Win32Emu.Win32;

public class Kernel32Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null) : IWin32ModuleUnsafe
{
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
				returnValue = GetACP();
				return true;
			case "GETCPINFO":
				returnValue = GetCPInfo(a.UInt32(0), a.UInt32(1));
				return true;
			case "GETOEMCP":
				returnValue = GetOEMCP();
				return true;
			case "GETSTRINGTYPEA":
				returnValue = GetStringTypeA(a.UInt32(0), a.UInt32(1), a.Lpstr(2), a.Int32(3), a.UInt32(4));
				return true;
			case "GETSTRINGTYPEW":
				returnValue = GetStringTypeW(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.Int32(3), a.UInt32(4));
				return true;
			case "GETMODULEHANDLEA":
				returnValue = GetModuleHandleA(a.Lpstr(0));
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
			case "HEAPCREATE":
				returnValue = HeapCreate(a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "HEAPALLOC":
				returnValue = HeapAlloc((void*)a.UInt32(0), a.UInt32(1), a.UInt32(2));
				return true;
			case "HEAPFREE":
				returnValue = HeapFree((void*)a.UInt32(0), a.UInt32(1), (void*)a.UInt32(2));
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
				returnValue = LCMapStringA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.Int32(3), a.UInt32(4), a.Int32(5));
				return true;
			case "LCMAPSTRINGW":
				returnValue = LCMapStringW(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.Int32(3), a.UInt32(4), a.Int32(5));
				return true;
			case "RAISEEXCEPTION":
				returnValue = RaiseException(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
				return true;

			// Performance/timing functions
			case "QUERYPERFORMANCECOUNTER":
				returnValue = QueryPerformanceCounter(a.UInt32(0));
				return true;

			default:
				Console.WriteLine($"[Kernel32] Unimplemented export: {export}");
				return false;
		}
	}

	private static unsafe uint GetVersion()
	{
		const ushort build = 950;
		const byte major = 4;
		const byte minor = 0;
		return (uint)((major << 8 | minor) << 16 | build);
	}

	private unsafe uint GetLastError() => _lastError;

	private unsafe uint SetLastError(uint e)
	{
		_lastError = e;
		return 0;
	}

	private unsafe uint ExitProcess(uint code)
	{
		Console.WriteLine($"[Kernel32] ExitProcess({code})");
		env.RequestExit();
		return 0;
	}

	private unsafe uint TerminateProcess(uint hProcess, uint uExitCode)
	{
		// TerminateProcess terminates the specified process
		// hProcess: handle to the process (0xFFFFFFFF for current process)
		// uExitCode: exit code for the process
		
		Console.WriteLine($"[Kernel32] TerminateProcess(0x{hProcess:X8}, {uExitCode})");
		
		// In our emulator, we only support terminating the current process
		if (hProcess == 0xFFFFFFFF || hProcess == 0)
		{
			env.RequestExit();
			return NativeTypes.Win32Bool.TRUE;
		}
		
		// We don't support terminating other processes
		Console.WriteLine($"[Kernel32] TerminateProcess: Cannot terminate external process handle 0x{hProcess:X8}");
		_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
		return NativeTypes.Win32Bool.FALSE;
	}

	private unsafe uint RaiseException(uint dwExceptionCode, uint dwExceptionFlags, uint nNumberOfArguments, uint lpArguments)
	{
		// RaiseException raises a software exception
		// For now, we just log and continue - proper implementation would need exception handling
		Console.WriteLine($"[Kernel32] RaiseException(code=0x{dwExceptionCode:X8}, flags=0x{dwExceptionFlags:X}, nArgs={nNumberOfArguments}, args=0x{lpArguments:X8})");
		
		// In a real implementation, this would:
		// 1. Create an EXCEPTION_RECORD
		// 2. Search for exception handlers
		// 3. Unwind the stack if no handler found
		// For our emulator, we'll just log and return (doesn't actually return in real Win32)
		
		// This function doesn't return in normal Windows - it transfers control to exception handler
		// But for our simple emulator, we'll just return 0
		return 0;
	}

	private unsafe uint GetCurrentProcess() => 0xFFFFFFFF; // pseudo-handle

	private static unsafe uint GetACP() => 1252; // Windows-1252 (Western European)

	private unsafe uint GetCPInfo(uint codePage, uint lpCPInfo)
	{
		if (lpCPInfo == 0) return NativeTypes.Win32Bool.FALSE; // Return FALSE if null pointer

		// Handle special code page values
		uint actualCodePage = codePage switch
		{
			0 => GetACP(),        // CP_ACP - system default Windows ANSI code page
			1 => GetACP(),        // CP_OEMCP - system default OEM code page (we'll use same as ACP)
			_ => codePage
		};

		// We'll support common Western code pages
		switch (actualCodePage)
		{
			case 1252: // Windows-1252 (Western European)
				// Fill CPINFO structure
				env.MemWrite32(lpCPInfo + 0, 1);  // MaxCharSize = 1 (single-byte)
				// Write DefaultChar as bytes - using MemWriteBytes for byte array
				env.MemWriteBytes(lpCPInfo + 4, new byte[] { 0x3F, 0x00 }); // DefaultChar[0] = '?' (0x3F), DefaultChar[1] = 0
				// LeadByte array - all zeros for single-byte code page (12 bytes)
				env.MemWriteBytes(lpCPInfo + 6, new byte[12]); // All zeros
				return 1; // TRUE

			case 437:  // OEM United States
			case 850:  // OEM Multilingual Latin I
			case 1250: // Windows Central Europe
			case 1251: // Windows Cyrillic
			case 28591: // ISO 8859-1 Latin I
				// Similar single-byte code page setup
				env.MemWrite32(lpCPInfo + 0, 1);  // MaxCharSize = 1
				env.MemWriteBytes(lpCPInfo + 4, new byte[] { 0x3F, 0x00 }); // DefaultChar = '?', 0
				env.MemWriteBytes(lpCPInfo + 6, new byte[12]); // LeadByte array all zeros
				return NativeTypes.Win32Bool.TRUE;

			default:
				// Unsupported code page
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return NativeTypes.Win32Bool.FALSE;
		}
	}
  
	private static unsafe uint GetOEMCP() => 437; // IBM PC US (OEM code page)

	private unsafe uint GetStringTypeA(uint locale, uint dwInfoType, sbyte* lpSrcStr, int cchSrc, uint lpCharType)
	{
		// Maximum string length limit to prevent excessive memory usage and infinite loops
		const int MAX_STRING_LENGTH_LIMIT = 1000;
		
		uint srcStrAddr = (uint)(nint)lpSrcStr;
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
		int length = cchSrc;
		if (cchSrc == -1)
		{
			length = 0;
			// Safely calculate string length with bounds check
			while (length < MAX_STRING_LENGTH_LIMIT)
			{
				byte ch = env.MemRead8(srcStrAddr + (uint)length);
				if (ch == 0) break;
				length++;
			}
		}

		// Validate length
		if (length <= 0 || length > MAX_STRING_LENGTH_LIMIT)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}

		// Character type constants from Windows API
		const ushort CT_CTYPE1_UPPER = 0x0001;    // uppercase
		const ushort CT_CTYPE1_LOWER = 0x0002;    // lowercase
		const ushort CT_CTYPE1_DIGIT = 0x0004;    // decimal digit
		const ushort CT_CTYPE1_SPACE = 0x0008;    // space characters
		const ushort CT_CTYPE1_PUNCT = 0x0010;    // punctuation
		const ushort CT_CTYPE1_CNTRL = 0x0020;    // control characters
		const ushort CT_CTYPE1_BLANK = 0x0040;    // blank characters
		const ushort CT_CTYPE1_XDIGIT = 0x0080;   // hexadecimal digits
		const ushort CT_CTYPE1_ALPHA = 0x0100;    // any letter

		// Process each character
		for (int i = 0; i < length; i++)
		{
			byte ch = env.MemRead8(srcStrAddr + (uint)i);
			ushort charType = 0;

			// ASCII punctuation ranges:
			// '!'..'/'  (33-47): !"#$%&'()*+,-./
			// ':'..'@'  (58-64): :;<=>?@
			// '['..'`'  (91-96): [\]^_`
			// '{'..'~'  (123-126): {|}~
			const byte PUNCT_RANGE1_START = (byte)'!';
			const byte PUNCT_RANGE1_END   = (byte)'/';
			const byte PUNCT_RANGE2_START = (byte)':';
			const byte PUNCT_RANGE2_END   = (byte)'@';
			const byte PUNCT_RANGE3_START = (byte)'[';
			const byte PUNCT_RANGE3_END   = (byte)'`';
			const byte PUNCT_RANGE4_START = (byte)'{';
			const byte PUNCT_RANGE4_END   = (byte)'~';

			// Basic ASCII character classification
			if (ch >= 'A' && ch <= 'Z')
			{
				charType |= CT_CTYPE1_UPPER | CT_CTYPE1_ALPHA;
				if ((ch >= 'A' && ch <= 'F'))
					charType |= CT_CTYPE1_XDIGIT;
			}
			else if (ch >= 'a' && ch <= 'z')
			{
				charType |= CT_CTYPE1_LOWER | CT_CTYPE1_ALPHA;
				if ((ch >= 'a' && ch <= 'f'))
					charType |= CT_CTYPE1_XDIGIT;
			}
			else if (ch >= '0' && ch <= '9')
			{
				charType |= CT_CTYPE1_DIGIT | CT_CTYPE1_XDIGIT;
			}
			else if (ch == ' ' || ch == '\t')
			{
				charType |= CT_CTYPE1_SPACE | CT_CTYPE1_BLANK;
			}
			else if (ch == '\n' || ch == '\r' || ch == '\f' || ch == '\v')
			{
				charType |= CT_CTYPE1_SPACE;
			}
			else if (ch <= 0x1F || ch == 0x7F)
			{
				charType |= CT_CTYPE1_CNTRL;
			}
			else if ((ch >= PUNCT_RANGE1_START && ch <= PUNCT_RANGE1_END) ||
			         (ch >= PUNCT_RANGE2_START && ch <= PUNCT_RANGE2_END) ||
			         (ch >= PUNCT_RANGE3_START && ch <= PUNCT_RANGE3_END) ||
			         (ch >= PUNCT_RANGE4_START && ch <= PUNCT_RANGE4_END))
			{
				charType |= CT_CTYPE1_PUNCT;
			}

			// Write the character type to the output array (each entry is 2 bytes)
			env.MemWrite16(lpCharType + (uint)(i * 2), charType);
		}

		return NativeTypes.Win32Bool.TRUE;
	}

	private unsafe uint GetStringTypeW(uint locale, uint dwInfoType, uint lpSrcStr, int cchSrc, uint lpCharType)
	{
		// GetStringTypeW retrieves character type information for Unicode characters
		// Similar to GetStringTypeA but for wide (Unicode) strings
		const int MAX_STRING_LENGTH_LIMIT = 1000;
		
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
		int length = cchSrc;
		if (cchSrc == -1)
		{
			// Count characters until null terminator (wide char = 2 bytes)
			length = 0;
			uint currentAddr = lpSrcStr;
			while (length < MAX_STRING_LENGTH_LIMIT)
			{
				ushort wchar = env.MemRead16(currentAddr);
				if (wchar == 0) break;
				length++;
				currentAddr += 2;
			}

			if (length >= MAX_STRING_LENGTH_LIMIT)
			{
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return NativeTypes.Win32Bool.FALSE;
			}
		}

		// Use same character type constants as GetStringTypeA
		const ushort CT_CTYPE1_UPPER = 0x0001;
		const ushort CT_CTYPE1_LOWER = 0x0002;
		const ushort CT_CTYPE1_DIGIT = 0x0004;
		const ushort CT_CTYPE1_SPACE = 0x0008;
		const ushort CT_CTYPE1_ALPHA = 0x0100;

		// Write character type information for each character
		for (int i = 0; i < length; i++)
		{
			ushort wchar = env.MemRead16(lpSrcStr + (uint)(i * 2));
			ushort charType = 0;

			if (wchar >= 'A' && wchar <= 'Z')
				charType = (ushort)(CT_CTYPE1_UPPER | CT_CTYPE1_ALPHA);
			else if (wchar >= 'a' && wchar <= 'z')
				charType = (ushort)(CT_CTYPE1_LOWER | CT_CTYPE1_ALPHA);
			else if (wchar >= '0' && wchar <= '9')
				charType = CT_CTYPE1_DIGIT;
			else if (wchar == ' ' || wchar == '\t' || wchar == '\n' || wchar == '\r')
				charType = CT_CTYPE1_SPACE;
			else
				charType = CT_CTYPE1_ALPHA; // Default for other characters

			env.MemWrite16(lpCharType + (uint)(i * 2), charType);
		}

		return NativeTypes.Win32Bool.TRUE;
	}

	private unsafe uint GetModuleHandleA(sbyte* name)
	{
		return imageBase;
	}

	private unsafe uint LoadLibraryA(sbyte* lpLibFileName)
	{
		if (lpLibFileName == null)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}

		// Read the library name from memory
		var libraryName = env.ReadAnsiString((uint)lpLibFileName);
		if (string.IsNullOrEmpty(libraryName))
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}

		// Get the directory of the current executable
		var executablePath = env.ExecutablePath;
		var executableDir = Path.GetDirectoryName(executablePath) ?? string.Empty;
		
		// Check if the library is local to the executable path
		var localLibraryPath = Path.Combine(executableDir, libraryName);
		bool isLocalDll = File.Exists(localLibraryPath);

		if (isLocalDll)
		{
			// DLL is local to executable path - load it using PeImageLoader for proper emulation
			Console.WriteLine($"[Kernel32] Loading local DLL for emulation: {libraryName}");
			
			// Register with dispatcher for function call tracking
			_dispatcher?.RegisterDynamicallyLoadedDll(libraryName);
			
			if (peLoader != null)
			{
				return env.LoadPeImage(localLibraryPath, peLoader);
			}
			else
			{
				Console.WriteLine($"[Kernel32] Warning: PeImageLoader not available, falling back to module tracking for {libraryName}");
				return env.LoadModule(libraryName);
			}
		}
		else
		{
			// DLL is not local - thunk to emulator's win32 syscall implementation
			// For system DLLs like kernel32.dll, user32.dll, etc., we return a fake handle
			// but the actual implementation will be handled by the dispatcher
			Console.WriteLine($"[Kernel32] Loading system DLL via thunking: {libraryName}");
			
			// Register with dispatcher for function call tracking
			_dispatcher?.RegisterDynamicallyLoadedDll(libraryName);
			
			// For system libraries, we still need to track them but mark them as system modules
			return env.LoadModule(libraryName);
		}
	}

	private unsafe uint GetProcAddress(uint hModule, uint lpProcName)
	{
		// GetProcAddress retrieves the address of an exported function from a DLL
		// hModule: module handle from LoadLibraryA or GetModuleHandleA
		// lpProcName: either a string pointer (name) or an ordinal value (LOWORD)
		
		Console.WriteLine($"[Kernel32] GetProcAddress(0x{hModule:X8}, 0x{lpProcName:X8})");
		
		if (hModule == 0)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}
		
		string? procName = null;
		ushort ordinal = 0;
		
		// Check if lpProcName is an ordinal (high word is 0)
		if ((lpProcName & 0xFFFF0000) == 0)
		{
			ordinal = (ushort)(lpProcName & 0xFFFF);
			Console.WriteLine($"[Kernel32] GetProcAddress: Looking up by ordinal {ordinal}");
		}
		else
		{
			// It's a string pointer
			procName = env.ReadAnsiString(lpProcName);
			Console.WriteLine($"[Kernel32] GetProcAddress: Looking up '{procName}'");
		}
		
		// For now, we don't actually resolve exports from loaded modules
		// Real implementation would need to:
		// 1. Find the module by handle
		// 2. Parse its PE export table
		// 3. Return the RVA of the exported function
		// For emulation purposes, we return a stub address or 0 for not found
		
		Console.WriteLine($"[Kernel32] GetProcAddress: Export resolution not fully implemented, returning 0");
		_lastError = NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
		return 0;
	}

	private unsafe uint GetModuleFileNameA(void* h, sbyte* lp, uint n)
	{
		Diagnostics.LogDebug($"GetModuleFileNameA called: h=0x{(uint)(nint)h:X8} lp=0x{(uint)(nint)lp:X8} n={n}");
		// Use guest memory helpers instead of dereferencing raw pointers to avoid AccessViolation
		if (n == 0 || lp == null) return 0;

		// Convert lp to guest address
		uint lpAddr = (uint)(nint)lp;
		if (lpAddr == 0) return 0;

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
			var moduleName = env.GetModuleFileNameForHandle(numericHandle);
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

		Diagnostics.LogDebug($"GetModuleFileNameA resolved path: {path}");

		var bytes = System.Text.Encoding.ASCII.GetBytes(path);
		var required = (uint)bytes.Length; // number of chars without null

		// If buffer too small, copy up to n-1 and null terminate
		if (n <= required)
		{
			var copyLen = n > 0 ? n - 1u : 0u;
			if (copyLen > 0)
			{
				env.MemWriteBytes(lpAddr, bytes.AsSpan(0, (int)copyLen));
				Diagnostics.LogMemWrite(lpAddr, (int)copyLen, bytes.AsSpan(0, (int)copyLen).ToArray());
			}
			// write null terminator
			env.MemWriteBytes(lpAddr + copyLen, new byte[] { 0 });
			_lastError = NativeTypes.Win32Error.ERROR_INSUFFICIENT_BUFFER;
			Diagnostics.LogDebug($"GetModuleFileNameA truncated; copyLen={copyLen} returned");
			return copyLen;
		}

		// Fits in buffer: write full path and null terminator
		env.MemWriteBytes(lpAddr, bytes);
		env.MemWriteBytes(lpAddr + (uint)bytes.Length, new byte[] { 0 });
		Diagnostics.LogMemWrite(lpAddr, bytes.Length + 1, bytes.AsSpan(0, bytes.Length).ToArray());
		return (uint)bytes.Length;
	}

	private unsafe uint GetCommandLineA() => env.CommandLinePtr;

	private unsafe uint GetEnvironmentStringsW()
	{
		// Return pointer to Unicode environment strings block
		// This will be obtained from emulated environment variables, not system ones
		return env.GetEnvironmentStringsW();
	}

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
  
	private unsafe uint GetEnvironmentStringsA()
	{
		// Return pointer to ANSI environment strings block
		// This will be obtained from emulated environment variables, not system ones
		return env.GetEnvironmentStringsA();
	}



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

	
	
	private unsafe uint GetStartupInfoA(uint lpStartupInfo)
	{
		if (lpStartupInfo == 0) return 0;
		env.MemZero(lpStartupInfo, 68);
		env.MemWrite32(lpStartupInfo + 0, 68);
		env.MemWrite32(lpStartupInfo + 56, env.StdInputHandle);
		env.MemWrite32(lpStartupInfo + 60, env.StdOutputHandle);
		env.MemWrite32(lpStartupInfo + 64, env.StdErrorHandle);
		return 0;
	}

	private unsafe uint GetStdHandle(uint nStdHandle)
	{
		return nStdHandle switch
			{
				0xFFFFFFF6 => env.StdInputHandle, 0xFFFFFFF5 => env.StdOutputHandle,
				0xFFFFFFF4 => env.StdErrorHandle, _ => 0
			};
	}

	private unsafe uint SetStdHandle(uint nStdHandle, uint hHandle)
	{
		switch (nStdHandle)
		{
			case 0xFFFFFFF6: env.StdInputHandle = hHandle; break;
			case 0xFFFFFFF5: env.StdOutputHandle = hHandle; break;
			case 0xFFFFFFF4: env.StdErrorHandle = hHandle; break;
		}

		return 1;
	}

	private unsafe uint GlobalAlloc(uint flags, uint bytes) => env.SimpleAlloc(bytes == 0 ? 1u : bytes);
	private static unsafe uint GlobalFree(void* h) => 0;

	private unsafe uint HeapCreate(uint flOptions, uint dwInitialSize, uint dwMaximumSize) =>
		env.HeapCreate(flOptions, dwInitialSize, dwMaximumSize);

	private unsafe uint HeapAlloc(void* hHeap, uint dwFlags, uint dwBytes) => env.HeapAlloc((uint)hHeap, dwBytes);
	private static unsafe uint HeapFree(void* hHeap, uint dwFlags, void* lpMem) => 1;

	private unsafe uint HeapDestroy(void* hHeap)
	{
		// HeapDestroy destroys a heap created with HeapCreate
		// In our simple allocator, we don't actually manage individual heaps
		// Just return success for API compatibility
		Console.WriteLine($"[Kernel32] HeapDestroy(0x{(uint)(nint)hHeap:X8})");
		
		if (hHeap == null)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}
		
		return NativeTypes.Win32Bool.TRUE;
	}

	private unsafe uint VirtualAlloc(uint lpAddress, uint dwSize, uint flAllocationType, uint flProtect) =>
		env.VirtualAlloc(lpAddress, dwSize, flAllocationType, flProtect);

	private unsafe uint VirtualFree(uint lpAddress, uint dwSize, uint dwFreeType)
	{
		// VirtualFree releases or decommits virtual memory
		// dwFreeType: MEM_DECOMMIT (0x4000) or MEM_RELEASE (0x8000)
		// For simplicity in our emulator, we accept the call but don't actually free memory
		// The bump allocator doesn't support freeing
		Console.WriteLine($"[Kernel32] VirtualFree(0x{lpAddress:X8}, {dwSize}, 0x{dwFreeType:X})");
		
		const uint MEM_DECOMMIT = 0x4000;
		const uint MEM_RELEASE = 0x8000;
		
		// Validate parameters
		if (lpAddress == 0)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}
		
		// When using MEM_RELEASE, dwSize must be 0
		if ((dwFreeType & MEM_RELEASE) != 0 && dwSize != 0)
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}
		
		// Return success - memory will be cleaned up when process terminates
		return NativeTypes.Win32Bool.TRUE;
	}

	// File I/O implementations
	private unsafe uint CreateFileA(uint lpFileName, uint dwDesiredAccess, uint dwShareMode, uint lpSecAttr,
		uint dwCreationDisposition, uint dwFlagsAndAttributes, uint hTemplateFile)
	{
		try
		{
			var path = env.ReadAnsiString(lpFileName);
			
			// Handle invalid paths (empty, null, or invalid characters)
			if (string.IsNullOrEmpty(path))
			{
				Console.WriteLine("[Kernel32] CreateFileA failed: Invalid path (empty or null)");
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
				access = FileAccess.Read; // GENERIC_READ
			if ((dwDesiredAccess & 0x80000000) != 0 && (dwDesiredAccess & 0x40000000) == 0)
				access = FileAccess.Write; // GENERIC_WRITE
			var fs = new FileStream(path, mode, access, FileShare.ReadWrite);
			return env.RegisterHandle(fs);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[Kernel32] CreateFileA failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_FILE_NOT_FOUND;
			return NativeTypes.Win32Handle.INVALID_HANDLE_VALUE;
		}
	}

	private unsafe uint ReadFile(void* hFile, uint lpBuffer, uint nNumberOfBytesToRead, uint lpNumberOfBytesRead,
		uint lpOverlapped)
	{
		if (!env.TryGetHandle<FileStream>((uint)hFile, out var fs) || fs is null) return 0;
		try
		{
			var buf = new byte[nNumberOfBytesToRead];
			var read = fs.Read(buf, 0, buf.Length);
			if (lpBuffer != 0 && read > 0) env.MemWriteBytes(lpBuffer, buf.AsSpan(0, read));
			if (lpNumberOfBytesRead != 0) env.MemWrite32(lpNumberOfBytesRead, (uint)read);
			return 1;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[Kernel32] ReadFile failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
			return NativeTypes.Win32Bool.FALSE;
		}
	}

	private unsafe uint WriteFile(void* hFile, uint lpBuffer, uint nNumberOfBytesToWrite, uint lpNumberOfBytesWritten,
		uint lpOverlapped)
	{
		if (!env.TryGetHandle<FileStream>((uint)hFile, out var fs) || fs is null) return 0;
		try
		{
			var buf = env.MemReadBytes(lpBuffer, (int)nNumberOfBytesToWrite);
			fs.Write(buf, 0, buf.Length);
			if (lpNumberOfBytesWritten != 0) env.MemWrite32(lpNumberOfBytesWritten, (uint)buf.Length);
			return 1;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[Kernel32] WriteFile failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_FUNCTION;
			return NativeTypes.Win32Bool.FALSE;
		}
	}

	private unsafe uint CloseHandle(void* hObject)
	{
		var h = (uint)hObject;
		if (env.TryGetHandle<FileStream>(h, out var fs) && fs is not null)
		{
			fs.Dispose();
			env.CloseHandle(h);
			return 1;
		}

		return env.CloseHandle(h) ? 1u : 0u;
	}

	private unsafe uint GetFileType(void* hFile)
	{
		if (env.TryGetHandle<FileStream>((uint)hFile, out var fs) && fs is not null) return 0x0001; // FILE_TYPE_DISK
		return 0; // FILE_TYPE_UNKNOWN
	}

	private unsafe uint SetFilePointer(void* hFile, uint lDistanceToMove, uint lpDistanceToMoveHigh, uint dwMoveMethod)
	{
		if (!env.TryGetHandle<FileStream>((uint)hFile, out var fs) || fs is null) return 0xFFFFFFFF;
		var origin = dwMoveMethod switch
		{
			0 => SeekOrigin.Begin, 1 => SeekOrigin.Current, 2 => SeekOrigin.End, _ => SeekOrigin.Begin
		};
		long dist = (int)lDistanceToMove; // ignore high for now
		var pos = fs.Seek(dist, origin);
		return (uint)pos;
	}

	private unsafe uint FlushFileBuffers(void* hFile)
	{
		if (env.TryGetHandle<FileStream>((uint)hFile, out var fs) && fs is not null)
		{
			fs.Flush(true);
			return 1;
		}

		return 0;
	}

	private unsafe uint SetEndOfFile(void* hFile)
	{
		if (env.TryGetHandle<FileStream>((uint)hFile, out var fs) && fs is not null)
		{
			fs.SetLength(fs.Position);
			return 1;
		}

		return 0;
	}

	private unsafe uint SetHandleCount(uint uNumber)
	{
		// SetHandleCount is a legacy function from 16-bit Windows
		// In Win32, it's essentially a no-op that returns the requested count
		// Modern systems ignore this and have much higher handle limits
		return uNumber; // Return the requested number as if it was successfully set
	}
  
	private unsafe uint UnhandledExceptionFilter(uint exceptionInfo)
	{
		// UnhandledExceptionFilter processes unhandled exceptions
		// exceptionInfo is a pointer to an EXCEPTION_POINTERS structure
		Console.WriteLine($"[Kernel32] UnhandledExceptionFilter called with exceptionInfo=0x{exceptionInfo:X8}");
		
		if (exceptionInfo != 0)
		{
			try
			{
				// EXCEPTION_POINTERS structure:
				// typedef struct _EXCEPTION_POINTERS {
				//   PEXCEPTION_RECORD ExceptionRecord;    // offset 0, 4 bytes
				//   PCONTEXT          ContextRecord;       // offset 4, 4 bytes  
				// } EXCEPTION_POINTERS;
				
				var exceptionRecordPtr = env.MemRead32(exceptionInfo);
				var contextRecordPtr = env.MemRead32(exceptionInfo + 4);
				
				Console.WriteLine($"[Kernel32]   ExceptionRecord: 0x{exceptionRecordPtr:X8}");
				Console.WriteLine($"[Kernel32]   ContextRecord: 0x{contextRecordPtr:X8}");
				
				// If we have a valid exception record, read some basic info
				if (exceptionRecordPtr != 0)
				{
					// EXCEPTION_RECORD structure (first few fields):
					//   DWORD ExceptionCode;        // offset 0
					//   DWORD ExceptionFlags;       // offset 4
					//   PEXCEPTION_RECORD ExceptionRecord; // offset 8
					//   PVOID ExceptionAddress;     // offset 12
					var exceptionCode = env.MemRead32(exceptionRecordPtr);
					var exceptionFlags = env.MemRead32(exceptionRecordPtr + 4);
					var exceptionAddress = env.MemRead32(exceptionRecordPtr + 12);
					
					Console.WriteLine($"[Kernel32]     ExceptionCode: 0x{exceptionCode:X8}");
					Console.WriteLine($"[Kernel32]     ExceptionFlags: 0x{exceptionFlags:X8}");
					Console.WriteLine($"[Kernel32]     ExceptionAddress: 0x{exceptionAddress:X8}");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[Kernel32] Error reading exception info: {ex.Message}");
			}
		}
		
		// For the emulator, we'll return EXCEPTION_EXECUTE_HANDLER to terminate the process
		// This is the safest default behavior for unhandled exceptions in an emulated environment
		return NativeTypes.ExceptionHandling.EXCEPTION_EXECUTE_HANDLER;
  }
  
	private unsafe uint WideCharToMultiByte(uint codePage, uint dwFlags, uint lpWideCharStr, uint cchWideChar, uint lpMultiByteStr, uint cbMultiByte, uint lpDefaultChar, uint lpUsedDefaultChar)
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
			uint actualCodePage = codePage switch
			{
				0 => GetACP(),        // CP_ACP - system default Windows ANSI code page
				1 => GetOEMCP(),      // CP_OEMCP - system default OEM code page
				_ => codePage
			};

			// Read the wide character string from memory
			string wideString;
			if (cchWideChar == 0xFFFFFFFF) // -1 indicates null-terminated string
			{
				// Read null-terminated wide string
				var wideChars = new List<char>();
				uint addr = lpWideCharStr;
				while (true)
				{
					ushort wideChar = env.MemRead16(addr);
					if (wideChar == 0) break;
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
					wideChars[i] = (char)env.MemRead16(lpWideCharStr + i * 2);
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
					multiByteBytes = System.Text.Encoding.Latin1.GetBytes(wideString);
					break;
				case 437:  // OEM US
				case 850:  // OEM Latin-1  
				case 1250: // Windows Central Europe
				case 1251: // Windows Cyrillic
					// For other single-byte code pages, fallback to UTF-8 since Latin1 may not cover all characters
					// This provides better Unicode support even if not 100% code page accurate
					multiByteBytes = System.Text.Encoding.UTF8.GetBytes(wideString);
					break;
				case 65001: // UTF-8
					multiByteBytes = System.Text.Encoding.UTF8.GetBytes(wideString);
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
				env.MemWriteBytes(lpMultiByteStr, multiByteBytes);
			}

			// Clear the "used default char" flag if provided
			if (lpUsedDefaultChar != 0)
			{
				env.MemWrite32(lpUsedDefaultChar, 0); // FALSE - no default char used (simplified)
			}

			return (uint)multiByteBytes.Length;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[Kernel32] WideCharToMultiByte failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}
	}
  
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
				codePage = 1252;

			// Determine string length if cbMultiByte is -1
			byte[] multiByteBytes;
			if (cbMultiByte == -1)
			{
				// Null-terminated string - read until null
				var byteList = new List<byte>();
				uint currentAddr = lpMultiByteStr;
				while (true)
				{
					byte b = env.MemRead8(currentAddr);
					if (b == 0) break;
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
				for (int i = 0; i < cbMultiByte; i++)
				{
					multiByteBytes[i] = env.MemRead8(lpMultiByteStr + (uint)i);
				}
			}

			// Convert to string using appropriate encoding
			// For simplicity, use ASCII for code pages 1252/437, UTF-8 for 65001
			System.Text.Encoding encoding = codePage switch
			{
				65001 => System.Text.Encoding.UTF8,             // UTF-8
				_ => System.Text.Encoding.ASCII                  // ASCII for Western code pages
			};

			string str = encoding.GetString(multiByteBytes);

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
			for (int i = 0; i < str.Length; i++)
			{
				env.MemWrite16(lpWideCharStr + (uint)(i * 2), (ushort)str[i]);
			}

			// Add null terminator if there's room and input was null-terminated
			if (cbMultiByte == -1 && str.Length < cchWideChar)
			{
				env.MemWrite16(lpWideCharStr + (uint)(str.Length * 2), 0);
			}

			return (uint)str.Length;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[Kernel32] MultiByteToWideChar failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}
	}

	private unsafe uint LCMapStringA(uint locale, uint dwMapFlags, uint lpSrcStr, int cchSrc, uint lpDestStr, int cchDest)
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

			const uint LCMAP_LOWERCASE = 0x00000100;
			const uint LCMAP_UPPERCASE = 0x00000200;

			// Read source string
			string srcStr;
			if (cchSrc == -1)
			{
				srcStr = env.ReadAnsiString(lpSrcStr);
			}
			else
			{
				var bytes = new byte[cchSrc];
				for (int i = 0; i < cchSrc; i++)
				{
					bytes[i] = env.MemRead8(lpSrcStr + (uint)i);
				}
				srcStr = System.Text.Encoding.ASCII.GetString(bytes);
			}

			// Apply mapping
			string destStr = srcStr;
			if ((dwMapFlags & LCMAP_LOWERCASE) != 0)
				destStr = srcStr.ToLowerInvariant();
			else if ((dwMapFlags & LCMAP_UPPERCASE) != 0)
				destStr = srcStr.ToUpperInvariant();

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
			var destBytes = System.Text.Encoding.ASCII.GetBytes(destStr);
			env.MemWriteBytes(lpDestStr, destBytes);
			env.MemWriteBytes(lpDestStr + (uint)destBytes.Length, new byte[] { 0 }); // Null terminator

			return (uint)destStr.Length + 1;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[Kernel32] LCMapStringA failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}
	}

	private unsafe uint LCMapStringW(uint locale, uint dwMapFlags, uint lpSrcStr, int cchSrc, uint lpDestStr, int cchDest)
	{
		// LCMapStringW performs locale-dependent string mapping for Unicode strings
		
		try
		{
			if (lpSrcStr == 0)
			{
				_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
				return 0;
			}

			const uint LCMAP_LOWERCASE = 0x00000100;
			const uint LCMAP_UPPERCASE = 0x00000200;

			// Read source string (wide chars)
			string srcStr;
			if (cchSrc == -1)
			{
				// Null-terminated
				var chars = new List<char>();
				uint currentAddr = lpSrcStr;
				while (true)
				{
					ushort wchar = env.MemRead16(currentAddr);
					if (wchar == 0) break;
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
				for (int i = 0; i < cchSrc; i++)
				{
					chars[i] = (char)env.MemRead16(lpSrcStr + (uint)(i * 2));
				}
				srcStr = new string(chars);
			}

			// Apply mapping
			string destStr = srcStr;
			if ((dwMapFlags & LCMAP_LOWERCASE) != 0)
				destStr = srcStr.ToLowerInvariant();
			else if ((dwMapFlags & LCMAP_UPPERCASE) != 0)
				destStr = srcStr.ToUpperInvariant();

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
			for (int i = 0; i < destStr.Length; i++)
			{
				env.MemWrite16(lpDestStr + (uint)(i * 2), (ushort)destStr[i]);
			}
			env.MemWrite16(lpDestStr + (uint)(destStr.Length * 2), 0); // Null terminator

			return (uint)destStr.Length + 1;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[Kernel32] LCMapStringW failed: {ex.Message}");
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return 0;
		}
	}
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
			var timestamp = System.Diagnostics.Stopwatch.GetTimestamp();
			
			// Write the 64-bit timestamp to the provided memory location
			env.MemWrite64(lpPerformanceCount, (ulong)timestamp);
			
			return NativeTypes.Win32Bool.TRUE;
		}
		catch
		{
			_lastError = NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return NativeTypes.Win32Bool.FALSE;
		}
	}

	private unsafe string ReadCurrentModulePath()
	{
		// Prefer the initialized executable path from the process environment
		if (!string.IsNullOrEmpty(env.ExecutablePath))
		{
			return env.ExecutablePath;
		}

		// Fall back to the module filename pointer if available
		try
		{
			if (env.ModuleFileNamePtr != 0)
			{
				var s = env.ReadAnsiString(env.ModuleFileNamePtr);
				if (!string.IsNullOrEmpty(s)) return s;
			}
		}
		catch
		{
			// ignore and fall through to default
		}

		// Final fallback for legacy behavior
		return "game.exe";
	}

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
		
		Console.WriteLine($"[Kernel32] RtlUnwind called: targetFrame=0x{targetFrame:X8}, targetIp=0x{targetIp:X8}, exceptionRecord=0x{exceptionRecord:X8}, returnValue=0x{returnValue:X8}");
		
		// If a target IP is specified and it's not null, we would typically:
		// - Unwind the stack to the target frame
		// - Set EIP to targetIp
		// - Set EAX to returnValue
		// However, in this emulator context, we'll leave the actual stack unwinding
		// to be handled by the calling code/exception handling mechanism
		
		if (targetIp != 0)
		{
			Console.WriteLine($"[Kernel32] RtlUnwind: Would jump to 0x{targetIp:X8} with return value 0x{returnValue:X8}");
			// In a full implementation, we would modify the CPU state here
			// For now, we just log the intended operation
		}
		
		// RtlUnwind doesn't return a value in the traditional sense - it either succeeds
		// or raises an exception. We'll return 0 to indicate success.
		return 0;
	}
}