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
			case "GETOEMCP":
				returnValue = GetOEMCP();
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
		if (lpCPInfo == 0) return 0; // Return FALSE if null pointer

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
				return 1; // TRUE

			default:
				// Unsupported code page
				_lastError = 87; // ERROR_INVALID_PARAMETER
				return 0; // FALSE
		}
	}
  
	private static unsafe uint GetOEMCP() => 437; // IBM PC US (OEM code page)

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
	private uint CreateFileA(uint lpFileName, uint dwDesiredAccess, uint dwShareMode, uint lpSecAttr,
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
			_lastError = 2; // ERROR_FILE_NOT_FOUND or generic error
			return 0;
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
			_lastError = 1; // generic
			return 0;
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
			_lastError = 1;
			return 0;
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

	private string ReadCurrentModulePath() => "game.exe";
}