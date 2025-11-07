using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules
{
	public class MsvcrtModule : IWin32ModuleUnsafe
	{
		private readonly ProcessEnvironment _env;
		private readonly uint _imageBase;
		private readonly PeImageLoader? _peLoader;
		private readonly ILogger _logger;
		private uint _cachedAcmdlnPtr = 0;

		public MsvcrtModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
		{
			_env = env;
			_imageBase = imageBase;
			_peLoader = peLoader;
			_logger = logger ?? NullLogger.Instance;
		}

		public string Name => "MSVCRT.DLL";

		public unsafe bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			returnValue = 0;
			var a = new StackArgs(cpu, memory);

			switch (export.ToUpperInvariant())
			{
				case "__CXXFRAMEHANDLER":
					returnValue = __CxxFrameHandler(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;
				case "__FTOL":
				case "__FTOL2":
				case "__FTOL2_SSE":
					returnValue = (uint)__ftol(cpu);
					return true;
				case "__GETMAINARGS":
					returnValue = (uint)__getmainargs(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.Int32(3), a.UInt32(4));
					return true;
				case "__P___INITENV":
					returnValue = __p___initenv();
					return true;
				case "__P__ACMDLN":
					returnValue = __p__acmdln();
					return true;
				case "__P__COMMODE":
					returnValue = __p__commode();
					return true;
				case "__P__FMODE":
					returnValue = __p__fmode();
					return true;
				case "__P__IOB":
					returnValue = __p__iob();
					return true;
				case "__SET_APP_TYPE":
					returnValue = __set_app_type(a.UInt32(0));
					return true;
				case "__SETUSERMATHERR":
					returnValue = __setusermatherr(a.UInt32(0));
					return true;
				case "_ACMDLN":
					returnValue = _acmdln();
					return true;
				case "_ADJUST_FDIV":
					returnValue = _adjust_fdiv();
					return true;
				case "_AMSG_EXIT":
					_amsg_exit(a.Int32(0));
					returnValue = 0;
					return true;
				case "_CEXIT":
					_cexit();
					returnValue = 0;
					return true;
				case "_CONTROLFP":
					returnValue = _controlfp(a.UInt32(0), a.UInt32(1));
					return true;
				case "_CXXTHROWEXCEPTION":
					_CxxThrowException(a.UInt32(0), a.UInt32(1));
					returnValue = 0;
					return true;
				case "_EH_PROLOG":
					returnValue = _EH_prolog();
					return true;
				case "_EXCEPT_HANDLER3":
					returnValue = _except_handler3(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;
				case "_EXIT":
					_exit(a.Int32(0));
					returnValue = 0;
					return true;
				case "_INITTERM":
					_initterm(a.UInt32(0), a.UInt32(1));
					returnValue = 0;
					return true;
				case "_ITOA":
					returnValue = _itoa(a.Int32(0), a.UInt32(1), a.Int32(2));
					return true;
				case "_XCPTFILTER":
					returnValue = (uint)_XcptFilter(a.UInt32(0));
					return true;
				case "ABORT":
					abort();
					returnValue = 0;
					return true;
				case "ATEXIT":
					returnValue = atexit(a.UInt32(0));
					return true;
				case "CALLOC":
					returnValue = calloc(a.UInt32(0), a.UInt32(1));
					return true;
				case "EXIT":
					exit(a.Int32(0));
					returnValue = 0;
					return true;
				case "FPRINTF":
					returnValue = (uint)fprintf(a.UInt32(0), a.LpcStr(1));
					return true;
				case "FPUTS":
					returnValue = (uint)fputs(a.LpcStr(0), a.UInt32(1));
					return true;
				case "FREE":
					free((void*)a.UInt32(0));
					returnValue = 0;
					return true;
				case "GETENV":
					returnValue = getenv(a.LpcStr(0));
					return true;
				case "MALLOC":
					returnValue = malloc(a.UInt32(0));
					return true;
				case "MEMCMP":
					returnValue = (uint)memcmp(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "MEMCPY":
					returnValue = memcpy(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "MEMMOVE":
					returnValue = memmove(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "MEMSET":
					returnValue = memset(a.UInt32(0), a.Int32(1), a.UInt32(2));
					return true;
				case "REALLOC":
					returnValue = realloc((void*)a.UInt32(0), a.UInt32(1));
					return true;
				case "SIGNAL":
					returnValue = signal(a.Int32(0), a.UInt32(1));
					return true;
				case "STRLEN":
					returnValue = (uint)strlen(a.LpcStr(0));
					return true;
				case "STRNCMP":
					returnValue = (uint)strncmp(a.LpcStr(0), a.LpcStr(1), a.UInt32(2));
					return true;
				case "VFPRINTF":
					returnValue = (uint)vfprintf(a.UInt32(0), a.LpcStr(1), a.UInt32(2));
					return true;
				case "_SPAWNV":
					returnValue = (uint)_spawnv(a.Int32(0), a.LpcStr(1), a.UInt32(2));
					return true;
				case "_STRICMP":
					returnValue = (uint)_stricmp(a.LpcStr(0), a.LpcStr(1));
					return true;
				case "_STRREV":
					returnValue = _strrev(a.UInt32(0));
					return true;
				case "FCLOSE":
					returnValue = (uint)fclose(a.UInt32(0));
					return true;
				case "FGETS":
					returnValue = fgets(a.UInt32(0), a.Int32(1), a.UInt32(2));
					return true;
				case "FOPEN":
					returnValue = fopen(a.LpcStr(0), a.LpcStr(1));
					return true;
				case "PRINTF":
					returnValue = (uint)printf(a.LpcStr(0), a.UInt32(1));
					return true;
				case "SPRINTF":
					returnValue = (uint)sprintf(a.UInt32(0), a.LpcStr(1), a.UInt32(2));
					return true;
				case "STRCHR":
					returnValue = strchr(a.LpcStr(0), a.Int32(1));
					return true;
				case "STRRCHR":
					returnValue = strrchr(a.LpcStr(0), a.Int32(1));
					return true;
				case "TOUPPER":
					returnValue = (uint)toupper(a.Int32(0));
					return true;

				default:
					_logger.LogInformation("[msvcrt] Unimplemented export: {Export}", export);
					return false;
			}
		}

		[DllModuleExport(0)]
		private uint __p___initenv()
		{
			_logger.LogInformation("[msvcrt] __p___initenv()");
			// Return pointer to environment variables
			return _env.GetEnvironmentStringsA();
		}

		[DllModuleExport(0)]
		private uint __p__commode()
		{
			_logger.LogInformation("[msvcrt] __p__commode()");
			// Return pointer to commode (console/file mode)
			return _env.HeapAlloc(0, 4); // Allocate 4 bytes for mode
		}

		[DllModuleExport(0)]
		private uint __p__fmode()
		{
			_logger.LogInformation("[msvcrt] __p__fmode()");
			// Return pointer to file mode (text/binary)
			return _env.HeapAlloc(0, 4); // Allocate 4 bytes for mode
		}

		[DllModuleExport(0)]
		private uint __p__iob()
		{
			_logger.LogInformation("[msvcrt] __p__iob()");
			// Return pointer to IO buffer array (stdin, stdout, stderr)
			return _env.HeapAlloc(0, 96); // Simplified stub
		}

		[DllModuleExport(4)]
		private uint __set_app_type(uint appType)
		{
			_logger.LogInformation("[msvcrt] __set_app_type(appType={AppType})", appType);
			return 0;
		}

		[DllModuleExport(4)]
		private uint __setusermatherr(uint handler)
		{
			_logger.LogInformation("[msvcrt] __setusermatherr(handler=0x{Handler:X8})", handler);
			return 0;
		}

		[DllModuleExport(20)]
		private int __getmainargs(uint pargc, uint pargv, uint penv, int doWildcard, uint startupInfo)
		{
			_logger.LogInformation("[msvcrt] __getmainargs(pargc=0x{Pargc:X8}, pargv=0x{Pargv:X8}, penv=0x{Penv:X8}, doWildcard={DoWildcard}, startupInfo=0x{StartupInfo:X8})", 
				pargc, pargv, penv, doWildcard, startupInfo);

			// Parse command line into argc/argv
			var cmdLinePtr = _env.CommandLinePtr;
			var cmdLine = cmdLinePtr != 0 ? _env.ReadAnsiString(cmdLinePtr) : "";
			
			// Simple command line parsing - split on spaces, respecting quotes
			var args = new List<string>();
			var inQuote = false;
			var current = new System.Text.StringBuilder();
			
			foreach (var ch in cmdLine)
			{
				if (ch == '"')
				{
					inQuote = !inQuote;
				}
				else if (ch == ' ' && !inQuote)
				{
					if (current.Length > 0)
					{
						args.Add(current.ToString());
						current.Clear();
					}
				}
				else
				{
					current.Append(ch);
				}
			}
			
			if (current.Length > 0)
			{
				args.Add(current.ToString());
			}
			
			// Ensure we have at least one argument (program name)
			if (args.Count == 0)
			{
				args.Add("msconfig.exe");
			}
			
			// Allocate argv array (need argc+1 for NULL terminator)
			var argc = args.Count;
			var argvArray = _env.HeapAlloc(0, (uint)((argc + 1) * 4)); // Array of pointers + NULL
			
			// Write each argument string and store pointer
			for (var i = 0; i < argc; i++)
			{
				var argPtr = _env.WriteAnsiString(args[i] + '\0');
				_env.MemWrite32(argvArray + (uint)(i * 4), argPtr);
			}
			
			// Add NULL terminator to argv array
			_env.MemWrite32(argvArray + (uint)(argc * 4), 0);
			
			// Write argc
			_env.MemWrite32(pargc, (uint)argc);
			
			// Write argv pointer
			_env.MemWrite32(pargv, argvArray);
			
			// Write environment pointer
			var envPtr = _env.GetEnvironmentStringsA();
			_env.MemWrite32(penv, envPtr);
			
			_logger.LogInformation("[msvcrt] __getmainargs: argc={Argc}, argv=0x{Argv:X8}, env=0x{Env:X8}", argc, argvArray, envPtr);
			
			return 0; // Success
		}

		[DllModuleExport(0)]
		private uint __p__acmdln()
		{
			_logger.LogInformation("[msvcrt] __p__acmdln()");
			// Return pointer to command line string pointer
			// Cache to avoid memory leak from repeated allocations
			if (_cachedAcmdlnPtr == 0)
			{
				_cachedAcmdlnPtr = _env.HeapAlloc(0, 4);
				_env.MemWrite32(_cachedAcmdlnPtr, _env.CommandLinePtr);
			}
			return _cachedAcmdlnPtr;
		}

		[DllModuleExport(0)]
		private uint _acmdln()
		{
			_logger.LogInformation("[msvcrt] _acmdln()");
			// Return pointer to command line string
			return _env.CommandLinePtr;
		}

		[DllModuleExport(0)]
		private uint _adjust_fdiv()
		{
			_logger.LogInformation("[msvcrt] _adjust_fdiv()");
			// Pentium FDIV bug adjustment - not needed on modern CPUs
			return 0;
		}

		[DllModuleExport(4)]
		private void _amsg_exit(int code)
		{
			_logger.LogInformation("[msvcrt] _amsg_exit(code={Code})", code);
			// Exit with error message (stub)
		}

		[DllModuleExport(0)]
		private void _cexit()
		{
			_logger.LogInformation("[msvcrt] _cexit()");
			// Clean up without terminating (stub)
		}

		[DllModuleExport(8)]
		private void _initterm(uint start, uint end)
		{
			_logger.LogInformation("[msvcrt] _initterm(start=0x{Start:X8}, end=0x{End:X8})", start, end);
			// Call initializers between start and end (stub)
		}

		[DllModuleExport(8)]
		private uint _controlfp(uint newControl, uint mask)
		{
			_logger.LogInformation("[msvcrt] _controlfp(newControl=0x{NewControl:X8}, mask=0x{Mask:X8})", newControl, mask);
			// Control floating point behavior
			// Return current control word (stub - return default x87 control word)
			return 0x0001003F; // Default FPU control word
		}

		[DllModuleExport(16)]
		private uint _except_handler3(uint pRecord, uint pFrame, uint pContext, uint pDispatcher)
		{
			_logger.LogInformation("[msvcrt] _except_handler3(pRecord=0x{PRecord:X8}, pFrame=0x{PFrame:X8}, pContext=0x{PContext:X8}, pDispatcher=0x{PDispatcher:X8})", 
				pRecord, pFrame, pContext, pDispatcher);
			// Exception handler - return ExceptionContinueSearch
			return 1; // EXCEPTION_CONTINUE_SEARCH
		}

		[DllModuleExport(4)]
		private void _exit(int code)
		{
			_logger.LogInformation("[msvcrt] _exit(code={Code})", code);
			// Exit without cleanup (stub - should exit)
		}

		[DllModuleExport(12)]
		private uint _itoa(int value, uint buffer, int radix)
		{
			_logger.LogInformation("[msvcrt] _itoa(value={Value}, buffer=0x{Buffer:X8}, radix={Radix})", value, buffer, radix);
			
			// Convert integer to string
			string result;
			if (radix == 10)
			{
				result = value.ToString();
			}
			else if (radix == 16)
			{
				// Use unsigned conversion to avoid overflow with int.MinValue
				if (value < 0)
				{
					result = "-" + ((uint)-value).ToString("X");
				}
				else
				{
					result = value.ToString("X");
				}
			}
			else if (radix == 8)
			{
				// Use two's complement for negative values, as in C's _itoa
				result = Convert.ToString(unchecked((uint)value), 8);
			}
			else if (radix == 2)
			{
				// Use two's complement for negative values, as in C's _itoa
				result = Convert.ToString(unchecked((uint)value), 2);
			}
			else
			{
				// Unsupported radix, use decimal
				result = value.ToString();
			}
			
			// Write string to buffer
			var bytes = System.Text.Encoding.ASCII.GetBytes(result);
			_env.MemWriteBytes(buffer, bytes);
			_env.MemWrite8(buffer + (uint)bytes.Length, 0); // Null terminator
			
			return buffer;
		}

		[DllModuleExport(4)]
		private int _XcptFilter(uint exceptionCode)
		{
			_logger.LogInformation("[msvcrt] _XcptFilter(exceptionCode=0x{ExceptionCode:X8})", exceptionCode);
			// Exception filter - return EXCEPTION_CONTINUE_SEARCH
			return 0; // EXCEPTION_CONTINUE_SEARCH
		}

		[DllModuleExport(0)]
		private void abort()
		{
			_logger.LogInformation("[msvcrt] abort()");
			// Abnormal termination (stub - should exit)
		}

		[DllModuleExport(4)]
		private uint atexit(uint func)
		{
			_logger.LogInformation("[msvcrt] atexit(func=0x{Func:X8})", func);
			// Register function to be called at exit
			return 0; // Success
		}

		[DllModuleExport(8)]
		private uint calloc(uint num, uint size)
		{
			_logger.LogInformation("[msvcrt] calloc(num={Num}, size={Size})", num, size);
			var totalSize = num * size;
			var ptr = _env.HeapAlloc(0, totalSize);
			// Zero-initialize the memory
			if (ptr != 0)
			{
				for (uint i = 0; i < totalSize; i++)
				{
					_env.MemWrite8(ptr + i, 0);
				}
			}
			return ptr;
		}

		[DllModuleExport(4)]
		private void exit(int code)
		{
			_logger.LogInformation("[msvcrt] exit(code={Code})", code);
			// Exit process (stub - should exit)
		}

		[DllModuleExport(8)]
		private int fprintf(uint stream, in LpcStr format)
		{
			var fmt = format.ToString() ?? string.Empty;
			_logger.LogInformation("[msvcrt] fprintf(stream=0x{Stream:X8}, format=\"{Fmt}\")", stream, fmt);
			// Print formatted string (stub)
			return fmt.Length;
		}

		[DllModuleExport(8)]
		private int fputs(in LpcStr str, uint stream)
		{
			var s = str.ToString() ?? string.Empty;
			_logger.LogInformation("[msvcrt] fputs(str=\"{S}\", stream=0x{Stream:X8})", s, stream);
			// Write string to stream (stub)
			return 0; // Success
		}

		[DllModuleExport(4)]
		private unsafe void free(void* ptr)
		{
			_logger.LogInformation("[msvcrt] free(ptr=0x{Ptr:X8})", (uint)ptr);
			if ((uint)ptr != 0)
			{
				_env.HeapFree(0, (uint)ptr);
			}
		}

		[DllModuleExport(4)]
		private uint getenv(in LpcStr name)
		{
			var varName = name.ToString() ?? string.Empty;
			_logger.LogInformation("[msvcrt] getenv(name=\"{VarName}\")", varName);
			// Get environment variable (stub - return NULL)
			return 0;
		}

		[DllModuleExport(4)]
		private uint malloc(uint size)
		{
			_logger.LogInformation("[msvcrt] malloc(size={Size})", size);
			return _env.HeapAlloc(0, size);
		}

		[DllModuleExport(12)]
		private int memcmp(uint ptr1, uint ptr2, uint num)
		{
			_logger.LogInformation("[msvcrt] memcmp(ptr1=0x{Ptr1:X8}, ptr2=0x{Ptr2:X8}, num={Num})", ptr1, ptr2, num);
			// Compare memory (stub - return 0 for equal)
			for (uint i = 0; i < num; i++)
			{
				var b1 = _env.MemRead8(ptr1 + i);
				var b2 = _env.MemRead8(ptr2 + i);
				if (b1 < b2) return -1;
				if (b1 > b2) return 1;
			}
			return 0;
		}

		[DllModuleExport(12)]
		private uint memcpy(uint dest, uint src, uint count)
		{
			_logger.LogInformation("[msvcrt] memcpy(dest=0x{Dest:X8}, src=0x{Src:X8}, count={Count})", dest, src, count);
			// Copy memory
			for (uint i = 0; i < count; i++)
			{
				_env.MemWrite8(dest + i, _env.MemRead8(src + i));
			}
			return dest;
		}

		[DllModuleExport(12)]
		private uint memmove(uint dest, uint src, uint count)
		{
			_logger.LogInformation("[msvcrt] memmove(dest=0x{Dest:X8}, src=0x{Src:X8}, count={Count})", dest, src, count);
			// Move memory (handle overlapping regions)
			if (dest < src || dest >= src + count)
			{
				// Non-overlapping or dest before src - copy forward
				for (uint i = 0; i < count; i++)
				{
					_env.MemWrite8(dest + i, _env.MemRead8(src + i));
				}
			}
			else
			{
				// Overlapping with dest after src - copy backward
				for (uint i = count; i > 0; i--)
				{
					_env.MemWrite8(dest + i - 1, _env.MemRead8(src + i - 1));
				}
			}
			return dest;
		}

		[DllModuleExport(12)]
		private uint memset(uint ptr, int value, uint num)
		{
			_logger.LogInformation("[msvcrt] memset(ptr=0x{Ptr:X8}, value={Value}, num={Num})", ptr, value, num);
			// Set memory
			var byteValue = (byte)(value & 0xFF);
			for (uint i = 0; i < num; i++)
			{
				_env.MemWrite8(ptr + i, byteValue);
			}
			return ptr;
		}

		[DllModuleExport(8)]
		private unsafe uint realloc(void* ptr, uint size)
		{
			_logger.LogInformation("[msvcrt] realloc(ptr=0x{Ptr:X8}, size={Size})", (uint)ptr, size);
			// Reallocate memory
			if ((uint)ptr == 0)
			{
				return malloc(size);
			}
			if (size == 0)
			{
				free(ptr);
				return 0;
			}
			// Allocate new block and copy (simplified implementation)
			var newPtr = malloc(size);
			if (newPtr != 0 && (uint)ptr != 0)
			{
				// Copy old data (we don't know the old size, so copy up to new size)
				for (uint i = 0; i < size; i++)
				{
					_env.MemWrite8(newPtr + i, _env.MemRead8((uint)ptr + i));
				}
				free(ptr);
			}
			return newPtr;
		}

		[DllModuleExport(8)]
		private uint signal(int sig, uint func)
		{
			_logger.LogInformation("[msvcrt] signal(sig={Sig}, func=0x{Func:X8})", sig, func);
			// Set signal handler (stub)
			return 0; // Success
		}

		[DllModuleExport(4)]
		private int strlen(in LpcStr str)
		{
			var s = str.ToString() ?? string.Empty;
			_logger.LogInformation("[msvcrt] strlen(str=\"{S}\")", s);
			return s.Length;
		}

		[DllModuleExport(12)]
		private int strncmp(in LpcStr str1, in LpcStr str2, uint num)
		{
			var s1 = str1.ToString() ?? string.Empty;
			var s2 = str2.ToString() ?? string.Empty;
			_logger.LogInformation("[msvcrt] strncmp(str1=\"{S1}\", str2=\"{S2}\", num={Num})", s1, s2, num);
			// Compare strings up to num characters
			var compareLength = (int)Math.Min(num, Math.Min(s1.Length, s2.Length));
			var result = string.Compare(s1, 0, s2, 0, compareLength, StringComparison.Ordinal);
			return result;
		}

		[DllModuleExport(12)]
		private int vfprintf(uint stream, in LpcStr format, uint args)
		{
			var fmt = format.ToString() ?? string.Empty;
			_logger.LogInformation("[msvcrt] vfprintf(stream=0x{Stream:X8}, format=\"{Fmt}\", args=0x{Args:X8})", stream, fmt, args);
			// Print formatted string with varargs (stub)
			return fmt.Length;
		}

	[DllModuleExport(12)]
	private int _spawnv(int mode, in LpcStr path, uint argv)
	{
		var pathStr = path.ToString() ?? string.Empty;
		_logger.LogInformation("[msvcrt] _spawnv(mode={Mode}, path=\"{Path}\", argv=0x{Argv:X8})", mode, pathStr, argv);
		// Stub - return -1 (error)
		return -1;
	}

	[DllModuleExport(8)]
	private int _stricmp(in LpcStr str1, in LpcStr str2)
	{
		var s1 = str1.ToString() ?? string.Empty;
		var s2 = str2.ToString() ?? string.Empty;
		_logger.LogInformation("[msvcrt] _stricmp(\"{S1}\", \"{S2}\")", s1, s2);
		return string.Compare(s1, s2, StringComparison.OrdinalIgnoreCase);
	}

	[DllModuleExport(4)]
	private int fclose(uint stream)
	{
		_logger.LogInformation("[msvcrt] fclose(stream=0x{Stream:X8})", stream);
		// Stub - return 0 (success)
		return 0;
	}

	[DllModuleExport(12)]
	private uint fgets(uint str, int n, uint stream)
	{
		_logger.LogInformation("[msvcrt] fgets(str=0x{Str:X8}, n={N}, stream=0x{Stream:X8})", str, n, stream);
		// Stub - return NULL
		return 0;
	}

	[DllModuleExport(8)]
	private uint fopen(in LpcStr filename, in LpcStr mode)
	{
		var fname = filename.ToString() ?? string.Empty;
		var modeStr = mode.ToString() ?? string.Empty;
		_logger.LogInformation("[msvcrt] fopen(\"{Fname}\", \"{Mode}\")", fname, modeStr);
		// Stub - return NULL
		return 0;
	}

	[DllModuleExport(8)]
	private int printf(in LpcStr format, uint args)
	{
		var fmt = format.ToString() ?? string.Empty;
		_logger.LogInformation("[msvcrt] printf(\"{Fmt}\", args=0x{Args:X8})", fmt, args);
		// Stub - return length
		return fmt.Length;
	}

	[DllModuleExport(12)]
	private int sprintf(uint buffer, in LpcStr format, uint args)
	{
		var fmt = format.ToString() ?? string.Empty;
		_logger.LogInformation("[msvcrt] sprintf(buffer=0x{Buffer:X8}, format=\"{Fmt}\", args=0x{Args:X8})", buffer, fmt, args);
		// Stub - write format string to buffer and return length
		if (buffer != 0)
		{
			_env.WriteAnsiStringAt(buffer, fmt);
		}
		return fmt.Length;
	}

	[DllModuleExport(8)]
	private uint strchr(in LpcStr str, int c)
	{
		var s = str.ToString() ?? string.Empty;
		_logger.LogInformation("[msvcrt] strchr(\"{S}\", {C})", s, c);
		var ch = (char)c;
		var index = s.IndexOf(ch);
		if (index >= 0)
		{
			// Return pointer to the character
			return str.Address + (uint)index;
		}
		return 0; // NULL
	}

	[DllModuleExport(8)]
	private uint strrchr(in LpcStr str, int c)
	{
		var s = str.ToString() ?? string.Empty;
		_logger.LogInformation("[msvcrt] strrchr(\"{S}\", {C})", s, c);
		var ch = (char)c;
		var index = s.LastIndexOf(ch);
		if (index >= 0)
		{
			// Return pointer to the character
			return str.Address + (uint)index;
		}
		return 0; // NULL
	}

	[DllModuleExport(16)]
	private uint __CxxFrameHandler(uint pExcept, uint pRN, uint pContext, uint pDC)
	{
		_logger.LogInformation("[msvcrt] __CxxFrameHandler(pExcept=0x{PExcept:X8}, pRN=0x{PRN:X8}, pContext=0x{PContext:X8}, pDC=0x{PDC:X8})", 
			pExcept, pRN, pContext, pDC);
		// Stub for C++ exception handler - return 0 (exception not handled)
		return 0;
	}

	[DllModuleExport(8)]
	private void _CxxThrowException(uint pExceptionObject, uint pThrowInfo)
	{
		_logger.LogInformation("[msvcrt] _CxxThrowException(pExceptionObject=0x{PExceptionObject:X8}, pThrowInfo=0x{PThrowInfo:X8})", 
			pExceptionObject, pThrowInfo);
		// Stub for C++ throw exception - just log it
		_logger.LogWarning("[msvcrt] C++ exception thrown but not handled in emulator");
	}

	[DllModuleExport(0)]
	private uint _EH_prolog()
	{
		_logger.LogInformation("[msvcrt] _EH_prolog()");
		// Stub for exception handler prolog - return 0
		return 0;
	}

	[DllModuleExport(4)]
	private uint _strrev(uint str)
	{
		_logger.LogInformation("[msvcrt] _strrev(str=0x{Str:X8})", str);
		if (str != 0)
		{
			var s = _env.ReadAnsiString(str) ?? string.Empty;
			var charArray = s.ToCharArray();
			Array.Reverse(charArray);
			var reversed = new string(charArray);
			_env.WriteAnsiStringAt(str, reversed);
		}
		return str;
	}

	[DllModuleExport(4)]
	private int toupper(int c)
	{
		_logger.LogInformation("[msvcrt] toupper({C})", c);
		if (c >= 'a' && c <= 'z')
		{
			return c - ('a' - 'A');
		}
		return c;
	}

	/// <summary>
	/// __ftol - Convert floating point value in ST(0) to signed long integer
	/// This is a special CRT function that reads from the x87 FPU stack
	/// Returns 64-bit result in EDX:EAX (high:low 32 bits)
	/// </summary>
	[DllModuleExport(0)]
	private long __ftol(ICpu cpu)
	{
		_logger.LogInformation("[msvcrt] __ftol()");
		
		// __ftol reads the floating point value from ST(0) and converts to long
		// The result is returned in EDX:EAX (high:low 32 bits) per x86 convention
		// ST(0) is popped from the FPU stack
		
		long result = AccessFpuAndConvert(cpu);
		
		// Set both EAX (low 32 bits) and EDX (high 32 bits)
		cpu.SetRegister("EAX", (uint)(result & 0xFFFFFFFF));
		cpu.SetRegister("EDX", (uint)((result >> 32) & 0xFFFFFFFF));
		
		_logger.LogDebug("[msvcrt] __ftol: result={Result:X16} (EDX:EAX = {Edx:X8}:{Eax:X8})", 
			result, (uint)((result >> 32) & 0xFFFFFFFF), (uint)(result & 0xFFFFFFFF));
		
		return result;
	}
	
	/// <summary>
	/// Helper method to access FPU stack and convert ST(0) to long integer
	/// Uses reflection to access private FPU methods on concrete CPU implementations
	/// </summary>
	private long AccessFpuAndConvert(ICpu cpu)
	{
		// Try to access FPU state through concrete CPU implementations
		// Note: This uses reflection which is brittle but necessary since ICpu doesn't expose FPU methods
		// TODO: Consider adding FPU methods to ICpu interface or creating IFpuCpu interface
		
		if (cpu is not (Cpu.Iced.IcedCpu or Cpu.Jit.JitCpu))
		{
			_logger.LogWarning("[msvcrt] __ftol: Unsupported CPU type {CpuType}, returning 0", 
				cpu.GetType().Name);
			return 0;
		}
		
		var cpuType = cpu.GetType();
		var fpuGetStMethod = cpuType.GetMethod("FpuGetSt", 
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		var fpuPopMethod = cpuType.GetMethod("FpuPop",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		
		if (fpuGetStMethod == null || fpuPopMethod == null)
		{
			_logger.LogWarning("[msvcrt] __ftol: Could not access FPU methods on {CpuType}, returning 0", 
				cpu.GetType().Name);
			return 0;
		}
		
		var st0 = (double)fpuGetStMethod.Invoke(cpu, new object[] { 0 })!;
		fpuPopMethod.Invoke(cpu, null);
		var result = (long)st0;
		
		_logger.LogDebug("[msvcrt] __ftol: ST(0)={St0} -> {Result}", st0, result);
		return result;
	}
}
}
