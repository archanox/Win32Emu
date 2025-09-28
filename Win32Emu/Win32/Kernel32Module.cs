using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Win32;

public class Kernel32Module(ProcessEnvironment env, uint imageBase) : IWin32ModuleUnsafe
{
	public string Name => "KERNEL32.DLL";

	[ThreadStatic] private static uint _lastError;

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
			case "GETMODULEHANDLEA":
				returnValue = GetModuleHandleA(a.Lpstr(0));
				return true;
			case "GETMODULEFILENAMEA":
				returnValue = GetModuleFileNameA(a.Ptr(0), a.Lpstr(1), a.UInt32(2));
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
			case "VIRTUALALLOC":
				returnValue = VirtualAlloc(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
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
			case "RTLUNWIND":
				returnValue = RtlUnwind(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
        return true;
			case "WIDECHARTOMULTIBYTE":
				returnValue = WideCharToMultiByte(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5), a.UInt32(6), a.UInt32(7));
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

	private static unsafe uint GetLastError() => _lastError;

	private static unsafe uint SetLastError(uint e)
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

	private unsafe uint GetModuleHandleA(sbyte* name)
	{
		return imageBase;
	}

	private unsafe uint GetModuleFileNameA(void* h, sbyte* lp, uint n)
	{
		if (n == 0 || lp == null) return 0;
		var p = ReadCurrentModulePath();
		var b = System.Text.Encoding.ASCII.GetBytes(p);
		var len = Math.Min((uint)b.Length, n - 1);
		for (uint i = 0; i < len; i++) lp[i] = (sbyte)b[i];
		lp[len] = 0;
		return len;
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

	private unsafe uint FreeEnvironmentStringsW(uint lpszEnvironmentBlock)
	{
		// Free Unicode environment strings block
		return env.FreeEnvironmentStringsW(lpszEnvironmentBlock);
	}

	private unsafe uint FreeEnvironmentStringsA(uint lpszEnvironmentBlock)
	{
		// Free ANSI environment strings block
		return env.FreeEnvironmentStringsA(lpszEnvironmentBlock);
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

	private unsafe uint VirtualAlloc(uint lpAddress, uint dwSize, uint flAllocationType, uint flProtect) =>
		env.VirtualAlloc(lpAddress, dwSize, flAllocationType, flProtect);

	// File I/O implementations
	private unsafe uint CreateFileA(uint lpFileName, uint dwDesiredAccess, uint dwShareMode, uint lpSecAttr,
		uint dwCreationDisposition, uint dwFlagsAndAttributes, uint hTemplateFile)
	{
		try
		{
			var path = env.ReadAnsiString(lpFileName);
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
			return NativeTypes.Win32Bool.FALSE;
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
				case 1252: // Windows-1252
				case 437:  // OEM US
				case 850:  // OEM Latin-1
				case 1250: // Windows Central Europe
				case 1251: // Windows Cyrillic
				case 28591: // ISO 8859-1
					// Use the correct code page encoding for these single-byte code pages
					multiByteBytes = System.Text.Encoding.GetEncoding((int)actualCodePage).GetBytes(wideString);
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

	private unsafe string ReadCurrentModulePath() => "game.exe";

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