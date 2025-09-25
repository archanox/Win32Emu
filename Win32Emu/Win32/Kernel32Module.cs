using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Win32;

public class Kernel32Module : IWin32ModuleUnsafe
{
	public string Name => "KERNEL32.DLL";

	private readonly ProcessEnvironment _env;
	private readonly uint _imageBase;

	public Kernel32Module(ProcessEnvironment env, uint imageBase)
	{
		_env = env;
		_imageBase = imageBase;
	}

	[ThreadStatic] private static uint _lastError;

	public bool TryInvoke(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		return false;
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
			case "GETCURRENTPROCESS":
				returnValue = GetCurrentProcess();
				return true;
			case "GETMODULEHANDLEA":
				returnValue = GetModuleHandleA(a.LPSTR(0));
				return true;
			case "GETMODULEFILENAMEA":
				returnValue = GetModuleFileNameA(a.Ptr(0), a.LPSTR(1), a.UInt32(2));
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
		ushort build = 950;
		byte major = 4, minor = 0;
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
		_env.RequestExit();
		return 0;
	}

	private unsafe uint GetCurrentProcess() => 0xFFFFFFFF; // pseudo-handle

	private unsafe uint GetModuleHandleA(sbyte* name)
	{
		return _imageBase;
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

	private unsafe uint GetCommandLineA() => _env.CommandLinePtr;

	private unsafe uint GetStartupInfoA(uint lpStartupInfo)
	{
		if (lpStartupInfo == 0) return 0;
		_env.MemZero(lpStartupInfo, 68);
		_env.MemWrite32(lpStartupInfo + 0, 68);
		_env.MemWrite32(lpStartupInfo + 56, _env.StdInputHandle);
		_env.MemWrite32(lpStartupInfo + 60, _env.StdOutputHandle);
		_env.MemWrite32(lpStartupInfo + 64, _env.StdErrorHandle);
		return 0;
	}

	private unsafe uint GetStdHandle(uint nStdHandle)
	{
		return nStdHandle switch
		{
			0xFFFFFFF6 => _env.StdInputHandle, 0xFFFFFFF5 => _env.StdOutputHandle,
			0xFFFFFFF4 => _env.StdErrorHandle, _ => 0
		};
	}

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

	private unsafe uint GlobalAlloc(uint flags, uint bytes) => _env.SimpleAlloc(bytes == 0 ? 1u : bytes);
	private static unsafe uint GlobalFree(void* h) => 0;

	private unsafe uint HeapCreate(uint flOptions, uint dwInitialSize, uint dwMaximumSize) =>
		_env.HeapCreate(flOptions, dwInitialSize, dwMaximumSize);

	private unsafe uint HeapAlloc(void* hHeap, uint dwFlags, uint dwBytes) => _env.HeapAlloc((uint)hHeap, dwBytes);
	private static unsafe uint HeapFree(void* hHeap, uint dwFlags, void* lpMem) => 1;

	private unsafe uint VirtualAlloc(uint lpAddress, uint dwSize, uint flAllocationType, uint flProtect) =>
		_env.VirtualAlloc(lpAddress, dwSize, flAllocationType, flProtect);

	// File I/O implementations
	private uint CreateFileA(uint lpFileName, uint dwDesiredAccess, uint dwShareMode, uint lpSecAttr,
		uint dwCreationDisposition, uint dwFlagsAndAttributes, uint hTemplateFile)
	{
		try
		{
			var path = _env.ReadAnsiString(lpFileName);
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
			return _env.RegisterHandle(fs);
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
		if (!_env.TryGetHandle<FileStream>((uint)hFile, out var fs) || fs is null) return 0;
		try
		{
			var buf = new byte[nNumberOfBytesToRead];
			var read = fs.Read(buf, 0, buf.Length);
			if (lpBuffer != 0 && read > 0) _env.MemWriteBytes(lpBuffer, buf.AsSpan(0, read));
			if (lpNumberOfBytesRead != 0) _env.MemWrite32(lpNumberOfBytesRead, (uint)read);
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
		if (!_env.TryGetHandle<FileStream>((uint)hFile, out var fs) || fs is null) return 0;
		try
		{
			var buf = _env.MemReadBytes(lpBuffer, (int)nNumberOfBytesToWrite);
			fs.Write(buf, 0, buf.Length);
			if (lpNumberOfBytesWritten != 0) _env.MemWrite32(lpNumberOfBytesWritten, (uint)buf.Length);
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
		if (_env.TryGetHandle<FileStream>(h, out var fs) && fs is not null)
		{
			fs.Dispose();
			_env.CloseHandle(h);
			return 1;
		}

		return _env.CloseHandle(h) ? 1u : 0u;
	}

	private unsafe uint GetFileType(void* hFile)
	{
		if (_env.TryGetHandle<FileStream>((uint)hFile, out var fs) && fs is not null) return 0x0001; // FILE_TYPE_DISK
		return 0; // FILE_TYPE_UNKNOWN
	}

	private unsafe uint SetFilePointer(void* hFile, uint lDistanceToMove, uint lpDistanceToMoveHigh, uint dwMoveMethod)
	{
		if (!_env.TryGetHandle<FileStream>((uint)hFile, out var fs) || fs is null) return 0xFFFFFFFF;
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
		if (_env.TryGetHandle<FileStream>((uint)hFile, out var fs) && fs is not null)
		{
			fs.Flush(true);
			return 1;
		}

		return 0;
	}

	private unsafe uint SetEndOfFile(void* hFile)
	{
		if (_env.TryGetHandle<FileStream>((uint)hFile, out var fs) && fs is not null)
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