using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;
using System.Collections.Concurrent;
using System.Text;
using System.Linq;

namespace Win32Emu.Win32.Modules
{
	public class MsvcrtModule : IWin32ModuleUnsafe
	{
		private readonly ProcessEnvironment _env;
		private readonly uint _imageBase;
		private readonly PeImageLoader? _peLoader;
		private readonly ILogger _logger;
		private uint _cachedAcmdlnPtr = 0;
		private uint _cachedWcmdlnPtr = 0;
		
		// CPU instance is set by TryInvokeUnsafe before calling exported methods
		// Cannot be passed as parameter to [DllModuleExport] methods as it breaks source generation
		private ICpu? _cpu;
		
		// Invalid parameter handler tracking
		private uint _invalidParameterHandler = 0;
		
		// File handle tracking for fflush and setvbuf
		private readonly Dictionary<uint, FileStreamInfo> _fileStreams = new();
		
		// atexit/onexit function tracking (using ConcurrentBag for thread safety)
		private readonly ConcurrentBag<uint> _exitFunctions = new();
		
		// Thread lock tracking (using ConcurrentDictionary for thread safety)
		// Uses a shared object since we're just tracking lock acquisition, not implementing real locks
		private readonly ConcurrentDictionary<int, object> _locks = new();
		private static readonly object _sharedLockObject = new();
		
		/// <summary>
		/// Stream buffering mode
		/// </summary>
		private enum StreamBufferMode
		{
			IOFBF = 0,  // Full buffering
			IOLBF = 1,  // Line buffering
			IONBF = 2   // No buffering
		}
		
		/// <summary>
		/// Information about an open file stream
		/// </summary>
		private class FileStreamInfo
		{
			public uint BufferPtr { get; set; }
			public StreamBufferMode Mode { get; set; }
			public uint BufferSize { get; set; }
			public bool NeedsFlush { get; set; }
		}

		public MsvcrtModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
		{
			_env = env;
			_imageBase = imageBase;
			_peLoader = peLoader;
			_logger = logger ?? NullLogger.Instance;
		}

		public string Name => "MSVCRT.DLL";

		public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			returnValue = 0;
			_cpu = cpu;
			var a = new StackArgs(cpu, memory);

			switch (export.ToUpperInvariant())
			{
				case "__CXXFRAMEHANDLER":
					returnValue = __CxxFrameHandler(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;
				case "__FTOL":
					{
						var result = __ftol();
						returnValue = (uint)result;
						_cpu!.SetRegister("EDX", (uint)((ulong)result >> 32));
						return true;
					}
				case "__FTOL2":
					{
						var result = __ftol2();
						returnValue = (uint)result;
						_cpu!.SetRegister("EDX", (uint)((ulong)result >> 32));
						return true;
					}
				case "__FTOL2_SSE":
					{
						var result = __ftol2_sse();
						returnValue = (uint)result;
						_cpu!.SetRegister("EDX", (uint)((ulong)result >> 32));
						return true;
					}
				case "__GETMAINARGS":
					returnValue = (uint)__getmainargs(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.Int32(3), a.UInt32(4));
					return true;
				case "__WGETMAINARGS":
					returnValue = (uint)__wgetmainargs(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.Int32(3), a.UInt32(4));
					return true;
				case "__P___INITENV":
					returnValue = __p___initenv();
					return true;
				case "__P___WINITENV":
					returnValue = __p___winitenv();
					return true;
				case "__WINITENV":
					returnValue = __winitenv();
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
				case "_WCMDLN":
					returnValue = _wcmdln();
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
					free(a.UInt32(0));
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
					returnValue = realloc(a.UInt32(0), a.UInt32(1));
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
				case "STRCMP":
					returnValue = (uint)strcmp(a.LpcStr(0), a.LpcStr(1));
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
				case "_FPRESET":
					_fpreset();
					returnValue = 0;
					return true;
				case "_SET_INVALID_PARAMETER_HANDLER":
					returnValue = _set_invalid_parameter_handler(a.UInt32(0));
					return true;
				case "FFLUSH":
					returnValue = (uint)fflush(a.UInt32(0));
					return true;
				case "SETVBUF":
					returnValue = (uint)setvbuf(a.UInt32(0), a.UInt32(1), a.Int32(2), a.UInt32(3));
					return true;
				case "__DLLONEXIT":
					returnValue = __dllonexit(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "__INITENV":
					returnValue = __initenv();
					return true;
				case "__LCONV_INIT":
					returnValue = __lconv_init();
					return true;
				case "_IOB":
					returnValue = _iob();
					return true;
				case "_LOCK":
					_lock(a.Int32(0));
					returnValue = 0;
					return true;
				case "_ONEXIT":
					returnValue = _onexit(a.UInt32(0));
					return true;
				case "_STRDUP":
					returnValue = _strdup(a.LpcStr(0));
					return true;
				case "_UNLOCK":
					_unlock(a.Int32(0));
					returnValue = 0;
					return true;
				case "_VSNPRINTF":
					returnValue = (uint)_vsnprintf(a.UInt32(0), a.UInt32(1), a.LpcStr(2), a.UInt32(3));
					return true;
				case "ATOI":
					returnValue = (uint)atoi(a.LpcStr(0));
					return true;
				case "FWRITE":
					returnValue = fwrite(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;
				case "STRCPY":
					returnValue = strcpy(a.UInt32(0), a.LpcStr(1));
					return true;
				case "_ASSERT":
					_assert(a.LpcStr(0), a.LpcStr(1), a.UInt32(2));
					returnValue = 0;
					return true;
				case "PUTCHAR":
					returnValue = (uint)putchar(a.Int32(0));
					return true;
				case "PUTS":
					returnValue = (uint)puts(a.LpcStr(0));
					return true;
				case "SSCANF":
					returnValue = (uint)sscanf(a.UInt32(0), a.LpcStr(1), a.UInt32(2));
					return true;
				case "STRCAT":
					returnValue = strcat(a.UInt32(0), a.LpcStr(1));
					return true;
				case "STRSTR":
					returnValue = strstr(a.LpcStr(0), a.LpcStr(1));
					return true;
				case "??1TYPE_INFO@@UAE@XZ":
					// type_info destructor (C++ mangled name)
					// This is typically a no-op for type_info objects in our emulation
					type_info_destructor(a.UInt32(0));
					returnValue = 0;
					return true;
				case "??3@YAXPAX@Z":
					// operator delete (C++ mangled name)
					operator_delete(a.UInt32(0));
					returnValue = 0;
					return true;
				case "_BEGINTHREADEX":
					returnValue = _beginthreadex(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5));
					return true;
				case "_VSNWPRINTF":
					returnValue = (uint)_vsnwprintf(a.UInt32(0), a.UInt32(1), a.LpcWStr(2), a.UInt32(3));
					return true;
				case "_WFOPEN":
					returnValue = _wfopen(a.LpcWStr(0), a.LpcWStr(1));
					return true;
				case "SWPRINTF":
					returnValue = (uint)swprintf(a.UInt32(0), a.LpcWStr(1), a.UInt32(2));
					return true;
				case "WCSCMP":
					returnValue = (uint)wcscmp(a.LpcWStr(0), a.LpcWStr(1));
					return true;
				case "WCSCPY":
					returnValue = wcscpy(a.UInt32(0), a.LpcWStr(1));
					return true;
				case "WCSLEN":
					returnValue = (uint)wcslen(a.LpcWStr(0));
					return true;
				case "WCSRCHR":
					returnValue = wcsrchr(a.LpcWStr(0), a.Int32(1));
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
		private uint __p___winitenv()
		{
			_logger.LogInformation("[msvcrt] __p___winitenv()");
			// Return pointer to environment variables (Unicode version)
			return _env.GetEnvironmentStringsW();
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

		[DllModuleExport(20)]
		private int __wgetmainargs(uint pargc, uint pargv, uint penv, int doWildcard, uint startupInfo)
		{
			_logger.LogInformation("[msvcrt] __wgetmainargs(pargc=0x{Pargc:X8}, pargv=0x{Pargv:X8}, penv=0x{Penv:X8}, doWildcard={DoWildcard}, startupInfo=0x{StartupInfo:X8})", 
				pargc, pargv, penv, doWildcard, startupInfo);

			// Parse command line into argc/argv (Unicode version)
			var cmdLinePtr = _env.CommandLinePtrW;
			var cmdLine = cmdLinePtr != 0 ? _env.ReadUnicodeString(cmdLinePtr) : "";
			
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
				var argPtr = _env.WriteUnicodeString(args[i] + '\0');
				_env.MemWrite32(argvArray + (uint)(i * 4), argPtr);
			}
			
			// Add NULL terminator to argv array
			_env.MemWrite32(argvArray + (uint)(argc * 4), 0);
			
			// Write argc
			_env.MemWrite32(pargc, (uint)argc);
			
			// Write argv pointer
			_env.MemWrite32(pargv, argvArray);
			
			// Write environment pointer
			var envPtr = _env.GetEnvironmentStringsW();
			_env.MemWrite32(penv, envPtr);
			
			_logger.LogInformation("[msvcrt] __wgetmainargs: argc={Argc}, argv=0x{Argv:X8}, env=0x{Env:X8}", argc, argvArray, envPtr);
			
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
		private uint _wcmdln()
		{
			_logger.LogInformation("[msvcrt] _wcmdln()");
			// Return pointer to command line string (Unicode version)
			// Cache to avoid memory leak from repeated allocations
			if (_cachedWcmdlnPtr == 0)
			{
				_cachedWcmdlnPtr = _env.HeapAlloc(0, 4);
				_env.MemWrite32(_cachedWcmdlnPtr, _env.CommandLinePtrW);
			}
			return _cachedWcmdlnPtr;
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
		private void free(uint ptr)
		{
			_logger.LogInformation("[msvcrt] free(ptr=0x{Ptr:X8})", ptr);
			if (ptr != 0)
			{
				_env.HeapFree(0, ptr);
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
				if (b1 < b2)
				{
					return -1;
				}

				if (b1 > b2)
				{
					return 1;
				}
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
		private uint realloc(uint ptr, uint size)
		{
			_logger.LogInformation("[msvcrt] realloc(ptr=0x{Ptr:X8}, size={Size})", ptr, size);
			// Reallocate memory
			if (ptr == 0)
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
			if (newPtr != 0 && ptr != 0)
			{
				// Copy old data (we don't know the old size, so copy up to new size)
				for (uint i = 0; i < size; i++)
				{
					_env.MemWrite8(newPtr + i, _env.MemRead8(ptr + i));
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

		/// <summary>
		/// strcmp - Compare two strings
		/// Performs lexicographic comparison of two null-terminated strings
		/// </summary>
		[DllModuleExport(8)]
		private int strcmp(in LpcStr str1, in LpcStr str2)
		{
			var s1 = str1.ToString() ?? string.Empty;
			var s2 = str2.ToString() ?? string.Empty;
			_logger.LogInformation("[msvcrt] strcmp(str1=\"{S1}\", str2=\"{S2}\")", s1, s2);
			// Compare strings
			return string.Compare(s1, s2, StringComparison.Ordinal);
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
	private long __ftol()
	{
		_logger.LogInformation("[msvcrt] __ftol()");
		return AccessFpuAndConvert();
	}
	
	/// <summary>
	/// __ftol2 - Convert floating point value in ST(0) to signed long integer
	/// Variant of __ftol for newer compilers
	/// </summary>
	[DllModuleExport(0)]
	private long __ftol2()
	{
		_logger.LogInformation("[msvcrt] __ftol2()");
		return AccessFpuAndConvert();
	}
	
	/// <summary>
	/// __ftol2_sse - Convert floating point value in ST(0) to signed long integer
	/// SSE-optimized variant of __ftol
	/// </summary>
	[DllModuleExport(0)]
	private long __ftol2_sse()
	{
		_logger.LogInformation("[msvcrt] __ftol2_sse()");
		return AccessFpuAndConvert();
	}
	
	/// <summary>
	/// Helper method to access FPU stack and convert ST(0) to long integer
	/// Accesses public FPU methods on concrete CPU implementations
	/// </summary>
	private long AccessFpuAndConvert()
	{
		// _cpu is guaranteed to be set by TryInvokeUnsafe before this method is called
		if (_cpu == null)
		{
			throw new InvalidOperationException("CPU instance is not available - this should never happen");
		}
		
		double st0;
		
		// Access FPU state through concrete CPU implementations
		if (_cpu is Cpu.Iced.IcedCpu icedCpu)
		{
			st0 = icedCpu.FpuGetSt(0);
			icedCpu.FpuPop();
		}
		else if (_cpu is Cpu.Jit.JitCpu jitCpu)
		{
			st0 = jitCpu.FpuGetSt(0);
			jitCpu.FpuPop();
		}
		else
		{
			_logger.LogWarning("[msvcrt] __ftol: Unsupported CPU type {CpuType}, returning 0", 
				_cpu.GetType().Name);
			return 0;
		}
		
		var result = (long)st0;
		
		// Note: EDX:EAX registers are set by the caller in the switch statement
		// to avoid redundant setting (Win32Dispatcher also sets EAX from returnValue)
		
		_logger.LogDebug("[msvcrt] __ftol: ST(0)={St0} -> {Result:X16}", st0, result);
		
		return result;
	}

	/// <summary>
	/// C++ type_info destructor (mangled name: ??1type_info@@UAE@XZ)
	/// Destructor for the std::type_info class
	/// </summary>
	[DllModuleExport(0)]
	private void type_info_destructor(uint thisPtr)
	{
		_logger.LogInformation("[msvcrt] type_info::~type_info(this=0x{This:X8})", thisPtr);
		// type_info destructor is typically a no-op in the MSVC runtime
		// The object is usually statically allocated and doesn't need explicit cleanup
	}

	/// <summary>
	/// C++ operator delete (mangled name: ??3@YAXPAX@Z)
	/// Global operator delete(void*)
	/// </summary>
	[DllModuleExport(0)]
	private void operator_delete(uint ptr)
	{
		_logger.LogInformation("[msvcrt] operator delete(ptr=0x{Ptr:X8})", ptr);
		// operator delete calls free internally
		if (ptr != 0)
		{
			_env.HeapFree(0, ptr);
		}
	}

	/// <summary>
	/// _fpreset - Reset floating-point unit to default state
	/// Resets the FPU control word and status word
	/// </summary>
	[DllModuleExport(0)]
	private void _fpreset()
	{
		_logger.LogInformation("[msvcrt] _fpreset()");
		
		// Reset FPU to default state
		// Default x87 FPU control word is 0x037F
		if (_cpu == null)
		{
			_logger.LogWarning("[msvcrt] _fpreset: CPU instance not available");
			return;
		}
		
		// Access FPU state through concrete CPU implementations
		if (_cpu is Cpu.Iced.IcedCpu icedCpu)
		{
			// Reset FPU by calling the FINIT instruction behavior
			// This sets control word to 0x037F, clears status word, and sets tag word to 0xFFFF
			icedCpu.FpuReset();
		}
		else if (_cpu is Cpu.Jit.JitCpu jitCpu)
		{
			// Reset FPU by calling the FINIT instruction behavior
			jitCpu.FpuReset();
		}
		else
		{
			_logger.LogWarning("[msvcrt] _fpreset: Unsupported CPU type {CpuType}", _cpu.GetType().Name);
		}
	}

	/// <summary>
	/// _set_invalid_parameter_handler - Set handler for invalid parameter errors
	/// Allows applications to handle invalid parameter errors in CRT functions
	/// </summary>
	[DllModuleExport(4)]
	private uint _set_invalid_parameter_handler(uint handler)
	{
		_logger.LogInformation("[msvcrt] _set_invalid_parameter_handler(handler=0x{Handler:X8})", handler);
		// Return the old handler and store the new one
		var oldHandler = _invalidParameterHandler;
		_invalidParameterHandler = handler;
		return oldHandler;
	}

	/// <summary>
	/// fflush - Flush a file stream
	/// Forces any buffered data to be written to the file
	/// </summary>
	[DllModuleExport(4)]
	private int fflush(uint stream)
	{
		_logger.LogInformation("[msvcrt] fflush(stream=0x{Stream:X8})", stream);
		
		// NULL stream means flush all open streams
		if (stream == 0)
		{
			// Flush all tracked streams
			foreach (var kvp in _fileStreams)
			{
				if (kvp.Value.NeedsFlush)
				{
					_logger.LogDebug("[msvcrt] fflush: Flushing stream 0x{Stream:X8}", kvp.Key);
					kvp.Value.NeedsFlush = false;
				}
			}
			return 0; // Success
		}
		
		// Flush specific stream
		if (_fileStreams.TryGetValue(stream, out var fileInfo))
		{
			if (fileInfo.NeedsFlush)
			{
				_logger.LogDebug("[msvcrt] fflush: Flushing stream 0x{Stream:X8}", stream);
				fileInfo.NeedsFlush = false;
			}
			return 0; // Success
		}
		
		// Stream not tracked, assume success (might be stdin/stdout/stderr)
		_logger.LogDebug("[msvcrt] fflush: Stream 0x{Stream:X8} not tracked, assuming success", stream);
		return 0; // Success
	}

	/// <summary>
	/// setvbuf - Set buffer for file stream
	/// Controls buffering mode and buffer size for a stream
	/// </summary>
	[DllModuleExport(16)]
	private int setvbuf(uint stream, uint buffer, int mode, uint size)
	{
		_logger.LogInformation("[msvcrt] setvbuf(stream=0x{Stream:X8}, buffer=0x{Buffer:X8}, mode={Mode}, size={Size})", 
			stream, buffer, mode, size);
		
		// Validate mode
		if (!Enum.IsDefined(typeof(StreamBufferMode), mode))
		{
			_logger.LogWarning("[msvcrt] setvbuf: Invalid mode {Mode}", mode);
			return -1; // Error
		}
		
		var bufferMode = (StreamBufferMode)mode;
		
		// Create or update stream info
		if (!_fileStreams.ContainsKey(stream))
		{
			_fileStreams[stream] = new FileStreamInfo();
		}
		
		var fileInfo = _fileStreams[stream];
		fileInfo.BufferPtr = buffer;
		fileInfo.Mode = bufferMode;
		fileInfo.BufferSize = size;
		fileInfo.NeedsFlush = false;
		
		_logger.LogDebug("[msvcrt] setvbuf: Set stream 0x{Stream:X8} to mode {Mode} with buffer 0x{Buffer:X8} size {Size}", 
			stream, bufferMode, buffer, size);
		
		return 0; // Success
	}

	/// <summary>
	/// __dllonexit - Register function to be called at DLL unload
	/// Takes function pointer, start and end of onexit table
	/// </summary>
	[DllModuleExport(12)]
	private uint __dllonexit(uint func, uint pbegin, uint pend)
	{
		_logger.LogInformation("[msvcrt] __dllonexit(func=0x{Func:X8}, pbegin=0x{Pbegin:X8}, pend=0x{Pend:X8})", 
			func, pbegin, pend);
		
		// Add to exit functions list
		if (func != 0)
		{
			_exitFunctions.Add(func);
		}
		
		// In real implementation, this would update the onexit table
		// For our purposes, just tracking in _exitFunctions is sufficient
		return func; // Return function pointer on success
	}

	/// <summary>
	/// __initenv - Get pointer to environment variables
	/// Returns pointer to array of environment strings
	/// </summary>
	[DllModuleExport(0)]
	private uint __initenv()
	{
		_logger.LogInformation("[msvcrt] __initenv()");
		// Return pointer to environment variables (same as __p___initenv)
		return _env.GetEnvironmentStringsA();
	}

	/// <summary>
	/// __winitenv - Get pointer to environment variables (Unicode version)
	/// Returns pointer to array of Unicode environment strings
	/// </summary>
	[DllModuleExport(0)]
	private uint __winitenv()
	{
		_logger.LogInformation("[msvcrt] __winitenv()");
		// Return pointer to environment variables (same as __p___winitenv)
		return _env.GetEnvironmentStringsW();
	}

	/// <summary>
	/// __lconv_init - Initialize locale conversion structure
	/// Initializes the locale-specific formatting information
	/// </summary>
	[DllModuleExport(0, IsStub = true)]
	private uint __lconv_init()
	{
		_logger.LogInformation("[msvcrt] __lconv_init()");
		// Return 0 for success - this is a stub as we don't implement full locale support
		return 0;
	}

	/// <summary>
	/// _iob - Get pointer to I/O buffer array
	/// Returns pointer to stdin, stdout, stderr buffers
	/// </summary>
	[DllModuleExport(0)]
	private uint _iob()
	{
		_logger.LogInformation("[msvcrt] _iob()");
		// Return pointer to IO buffer array (same as __p__iob)
		return __p__iob();
	}

	/// <summary>
	/// _lock - Acquire a lock for thread synchronization
	/// Used to protect CRT data structures in multi-threaded programs
	/// NOTE: This is a stub implementation that only tracks lock acquisition.
	/// Real implementation would block until lock is available.
	/// </summary>
	[DllModuleExport(4, IsStub = true)]
	private void _lock(int locknum)
	{
		_logger.LogInformation("[msvcrt] _lock(locknum={Locknum})", locknum);
		// In a full implementation, this would acquire a lock
		// For now, just track that we "have" the lock (thread-safe with ConcurrentDictionary)
		// Using a shared object to avoid allocating a new object for each lock
		_locks.TryAdd(locknum, _sharedLockObject);
	}

	/// <summary>
	/// _onexit - Register function to be called at exit
	/// Similar to atexit but returns the function pointer
	/// </summary>
	[DllModuleExport(4)]
	private uint _onexit(uint func)
	{
		_logger.LogInformation("[msvcrt] _onexit(func=0x{Func:X8})", func);
		
		// Add to exit functions list
		if (func != 0)
		{
			_exitFunctions.Add(func);
			return func; // Return function pointer on success
		}
		
		return 0; // NULL on error
	}

	/// <summary>
	/// _strdup - Duplicate a string
	/// Allocates memory and copies the string
	/// </summary>
	[DllModuleExport(4)]
	private uint _strdup(in LpcStr str)
	{
		var s = str.ToString() ?? string.Empty;
		_logger.LogInformation("[msvcrt] _strdup(str=\"{S}\")", s);
		
		// Allocate memory for string + null terminator
		// Check for potential overflow (though extremely unlikely in practice)
		if (s.Length >= int.MaxValue)
		{
			_logger.LogWarning("[msvcrt] _strdup: String too long, returning NULL");
			return 0; // NULL on error
		}
		
		var length = (uint)s.Length + 1;
		var ptr = _env.HeapAlloc(0, length);
		
		if (ptr == 0)
		{
			return 0; // NULL on allocation failure
		}
		
		// Copy string to allocated memory
		_env.WriteAnsiStringAt(ptr, s);
		
		return ptr;
	}

	/// <summary>
	/// _unlock - Release a lock for thread synchronization
	/// Used to release locks acquired with _lock
	/// </summary>
	[DllModuleExport(4, IsStub = true)]
	private void _unlock(int locknum)
	{
		_logger.LogInformation("[msvcrt] _unlock(locknum={Locknum})", locknum);
		// In a full implementation, this would release a lock
		// Thread-safe removal with ConcurrentDictionary
		_locks.TryRemove(locknum, out _);
	}

	/// <summary>
	/// _vsnprintf - Format string with variable arguments and size limit
	/// Similar to sprintf but with a maximum size and va_list
	/// NOTE: This is a simplified stub that does not handle printf-style format string substitution.
	/// It only copies the format string itself to the buffer with size limiting.
	/// Full implementation would require parsing format specifiers and reading varargs.
	/// </summary>
	[DllModuleExport(16, IsStub = true)]
	private int _vsnprintf(uint buffer, uint count, in LpcStr format, uint args)
	{
		var fmt = format.ToString() ?? string.Empty;
		_logger.LogInformation("[msvcrt] _vsnprintf(buffer=0x{Buffer:X8}, count={Count}, format=\"{Fmt}\", args=0x{Args:X8})", 
			buffer, count, fmt, args);
		
		// Simplified implementation - just copy format string to buffer with size limit
		if (buffer != 0 && count > 0)
		{
			// Safely handle count underflow and conversion
			// If count is 1, we can only write the null terminator
			var maxLength = count > 1 ? (int)Math.Min(count - 1, int.MaxValue) : 0;
			var outputLength = Math.Min(fmt.Length, maxLength);
			if (outputLength > 0)
			{
				var bytes = System.Text.Encoding.ASCII.GetBytes(fmt.Substring(0, outputLength));
				_env.MemWriteBytes(buffer, bytes);
			}
			// Always null terminate
			_env.MemWrite8(buffer + (uint)outputLength, 0);
			return outputLength;
		}
		
		return -1; // Error
	}

	/// <summary>
	/// atoi - Convert string to integer
	/// Parses string and returns integer value
	/// </summary>
	[DllModuleExport(4)]
	private int atoi(in LpcStr str)
	{
		var s = str.ToString() ?? string.Empty;
		_logger.LogInformation("[msvcrt] atoi(str=\"{S}\")", s);
		
		// Parse string to integer, return 0 if parsing fails
		if (int.TryParse(s.Trim(), out var result))
		{
			return result;
		}
		
		return 0;
	}

	/// <summary>
	/// fwrite - Write data to stream
	/// Writes count items of size bytes each to stream
	/// </summary>
	[DllModuleExport(16, IsStub = true)]
	private uint fwrite(uint ptr, uint size, uint count, uint stream)
	{
		_logger.LogInformation("[msvcrt] fwrite(ptr=0x{Ptr:X8}, size={Size}, count={Count}, stream=0x{Stream:X8})", 
			ptr, size, count, stream);
		
		// Check for potential overflow in size * count
		// This is a simplified implementation; real fwrite would write bytes to a file
		if (size > 0 && count > uint.MaxValue / size)
		{
			_logger.LogWarning("[msvcrt] fwrite: size * count would overflow, returning 0");
			return 0; // Error - overflow would occur
		}
		
		// In a real implementation, this would write to a file
		// For now, just return the count to indicate success
		// Mark stream as needing flush if tracked
		if (_fileStreams.TryGetValue(stream, out var fileInfo))
		{
			fileInfo.NeedsFlush = true;
		}
		
		return count; // Return number of items written
	}

	/// <summary>
	/// strcpy - Copy string
	/// Copies source string to destination
	/// </summary>
	[DllModuleExport(8)]
	private uint strcpy(uint dest, in LpcStr src)
	{
		var s = src.ToString() ?? string.Empty;
		_logger.LogInformation("[msvcrt] strcpy(dest=0x{Dest:X8}, src=\"{S}\")", dest, s);
		
		if (dest == 0)
		{
			return 0; // NULL destination
		}
		
		// Copy string to destination
		_env.WriteAnsiStringAt(dest, s);
		
		return dest; // Return destination pointer
	}

	/// <summary>
	/// _assert - Handle assertion failures
	/// Reports an assertion failure. Note: Unlike real MSVCRT, this implementation logs the failure but does not terminate the program.
	/// </summary>
	[DllModuleExport(12)]
	private void _assert(in LpcStr expr, in LpcStr file, uint line)
	{
		var expression = expr.ToString() ?? string.Empty;
		var filename = file.ToString() ?? string.Empty;
		_logger.LogError("[msvcrt] Assertion failed: {Expression}, file {File}, line {Line}", 
			expression, filename, line);
		
		// In real MSVCRT, this would show a dialog and abort the program
		// For now, just log the assertion and continue
		// Note: In a real implementation, this might call abort()
	}

	/// <summary>
	/// putchar - Write character to stdout
	/// Writes a character to standard output
	/// </summary>
	[DllModuleExport(4)]
	private int putchar(int c)
	{
		_logger.LogInformation("[msvcrt] putchar(c={C})", (char)c);
		
		// Write to proper stdout using the environment's WriteToStdOutput
		_env.WriteToStdOutput(((char)c).ToString());
		
		return c; // Return the character written
	}

	/// <summary>
	/// puts - Write string to stdout with newline
	/// Writes a string to standard output followed by a newline
	/// </summary>
	[DllModuleExport(4)]
	private int puts(in LpcStr str)
	{
		var s = str.ToString() ?? string.Empty;
		_logger.LogInformation("[msvcrt] puts(str=\"{S}\")", s);
		
		// Write to proper stdout using the environment's WriteToStdOutput
		_env.WriteToStdOutput(s + "\n");
		
		return s.Length + 1; // Return non-negative value on success (number of chars including newline)
	}

	/// <summary>
	/// sscanf - Read formatted data from string
	/// Reads data from string according to format specification
	/// </summary>
	[DllModuleExport(12, IsStub = true)]
	private int sscanf(uint buffer, in LpcStr format, uint varargs)
	{
		var fmt = format.ToString() ?? string.Empty;
		_logger.LogInformation("[msvcrt] sscanf(buffer=0x{Buffer:X8}, format=\"{Fmt}\", varargs=0x{Varargs:X8})", 
			buffer, fmt, varargs);
		
		// This is a complex function that would require parsing format strings
		// For now, return 0 to indicate no items were read
		// A real implementation would parse the format string and write to varargs
		_logger.LogWarning("[msvcrt] sscanf is a stub implementation");
		
		return 0; // Return number of items successfully read (0 for stub)
	}

	/// <summary>
	/// strcat - Concatenate strings
	/// Appends source string to destination string
	/// </summary>
	[DllModuleExport(8)]
	private uint strcat(uint dest, in LpcStr src)
	{
		var s = src.ToString() ?? string.Empty;
		_logger.LogInformation("[msvcrt] strcat(dest=0x{Dest:X8}, src=\"{S}\")", dest, s);
		
		if (dest == 0)
		{
			return 0; // NULL destination
		}
		
		// Read existing string at destination
		var existing = _env.ReadAnsiString(dest);
		
		// Concatenate strings
		var result = existing + s;
		
		// Write back to destination
		_env.WriteAnsiStringAt(dest, result);
		
		return dest; // Return destination pointer
	}

	/// <summary>
	/// strstr - Find substring
	/// Searches for first occurrence of substring in string
	/// </summary>
	[DllModuleExport(8)]
	private uint strstr(in LpcStr str, in LpcStr substr)
	{
		var s = str.ToString() ?? string.Empty;
		var sub = substr.ToString() ?? string.Empty;
		_logger.LogInformation("[msvcrt] strstr(str=\"{S}\", substr=\"{Sub}\")", s, sub);
		
		// Find index of substring
		var index = s.IndexOf(sub, StringComparison.Ordinal);
		
		if (index < 0)
		{
			return 0; // Not found, return NULL
		}
		
		// Calculate pointer to substring in original string
		// We need to return a pointer offset by the index
		var strPtr = str.Address;
		return strPtr + (uint)index;
	}

	/// <summary>
	/// _beginthreadex - Create a new thread with security attributes
	/// uintptr_t _beginthreadex(
	///   void *security,
	///   unsigned stack_size,
	///   unsigned ( __stdcall *start_address )( void * ),
	///   void *arglist,
	///   unsigned initflag,
	///   unsigned *thrdaddr
	/// );
	/// </summary>
	[DllModuleExport(18, IsStub = true)]
	private uint _beginthreadex(uint security, uint stackSize, uint startAddress, uint arglist, uint initflag, uint thrdaddr)
	{
		_logger.LogInformation("[msvcrt] _beginthreadex(security=0x{Security:X8}, stackSize={StackSize}, startAddress=0x{StartAddress:X8}, arglist=0x{Arglist:X8}, initflag={Initflag}, thrdaddr=0x{Thrdaddr:X8})",
			security, stackSize, startAddress, arglist, initflag, thrdaddr);
		
		// Stub implementation - return a fake thread handle
		// In a real implementation, this would create a new thread
		uint fakeThreadHandle = 0x12340000 | (uint)(_env.GetCurrentThreadId() + 1);
		
		// Write thread ID if pointer is provided
		if (thrdaddr != 0)
		{
			_env.MemWrite32(thrdaddr, fakeThreadHandle);
		}
		
		return fakeThreadHandle;
	}

	/// <summary>
	/// _vsnwprintf - Format a wide string with variable arguments
	/// int _vsnwprintf(
	///   wchar_t *buffer,
	///   size_t count,
	///   const wchar_t *format,
	///   va_list argptr
	/// );
	/// </summary>
	[DllModuleExport(12, IsStub = true)]
	private int _vsnwprintf(uint buffer, uint count, in LpcWStr format, uint args)
	{
		var fmt = format.ToString() ?? string.Empty;
		_logger.LogInformation("[msvcrt] _vsnwprintf(buffer=0x{Buffer:X8}, count={Count}, format=\"{Fmt}\", args=0x{Args:X8})", buffer, count, fmt, args);
		
		// Stub implementation - just write the format string
		if (buffer != 0 && count > 0)
		{
			var bytes = Encoding.Unicode.GetBytes(fmt + "\0");
			var maxBytes = Math.Min((uint)bytes.Length, count * 2); // count is in characters, not bytes
			_env.Memory.WriteBytes(buffer, bytes.Take((int)maxBytes).ToArray());
		}
		
		return fmt.Length;
	}

	/// <summary>
	/// _wfopen - Open a file using wide character filename
	/// FILE *_wfopen(
	///   const wchar_t *filename,
	///   const wchar_t *mode
	/// );
	/// </summary>
	[DllModuleExport(8, IsStub = true)]
	private uint _wfopen(in LpcWStr filename, in LpcWStr mode)
	{
		var fname = filename.ToString() ?? string.Empty;
		var m = mode.ToString() ?? string.Empty;
		_logger.LogInformation("[msvcrt] _wfopen(filename=\"{Fname}\", mode=\"{M}\")", fname, m);
		
		// Stub implementation - return a fake FILE pointer
		// In a real implementation, this would open the file
		return _env.HeapAlloc(0, 32); // Allocate fake FILE structure
	}

	/// <summary>
	/// swprintf - Format a wide string
	/// int swprintf(
	///   wchar_t *buffer,
	///   const wchar_t *format,
	///   ...
	/// );
	/// </summary>
	[DllModuleExport(12, IsStub = true)]
	private int swprintf(uint buffer, in LpcWStr format, uint args)
	{
		var fmt = format.ToString() ?? string.Empty;
		_logger.LogInformation("[msvcrt] swprintf(buffer=0x{Buffer:X8}, format=\"{Fmt}\", args=0x{Args:X8})", buffer, fmt, args);
		
		// Stub implementation - just write the format string
		if (buffer != 0)
		{
			var bytes = Encoding.Unicode.GetBytes(fmt + "\0");
			_env.Memory.WriteBytes(buffer, bytes);
		}
		
		return fmt.Length;
	}

	/// <summary>
	/// wcscmp - Compare two wide strings
	/// int wcscmp(
	///   const wchar_t *string1,
	///   const wchar_t *string2
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private int wcscmp(in LpcWStr str1, in LpcWStr str2)
	{
		var s1 = str1.ToString() ?? string.Empty;
		var s2 = str2.ToString() ?? string.Empty;
		_logger.LogInformation("[msvcrt] wcscmp(str1=\"{S1}\", str2=\"{S2}\")", s1, s2);
		
		// Compare wide strings
		return string.Compare(s1, s2, StringComparison.Ordinal);
	}

	/// <summary>
	/// wcscpy - Copy a wide string
	/// wchar_t *wcscpy(
	///   wchar_t *strDestination,
	///   const wchar_t *strSource
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint wcscpy(uint dest, in LpcWStr src)
	{
		var s = src.ToString() ?? string.Empty;
		_logger.LogInformation("[msvcrt] wcscpy(dest=0x{Dest:X8}, src=\"{S}\")", dest, s);
		
		if (dest == 0)
		{
			return 0; // NULL destination
		}
		
		// Copy wide string to destination
		var bytes = Encoding.Unicode.GetBytes(s + "\0");
		_env.Memory.WriteBytes(dest, bytes);
		
		return dest; // Return destination pointer
	}

	/// <summary>
	/// wcslen - Get length of a wide string
	/// size_t wcslen(
	///   const wchar_t *str
	/// );
	/// </summary>
	[DllModuleExport(4)]
	private int wcslen(in LpcWStr str)
	{
		var s = str.ToString() ?? string.Empty;
		_logger.LogInformation("[msvcrt] wcslen(str=\"{S}\")", s);
		
		return s.Length;
	}

	/// <summary>
	/// wcsrchr - Find last occurrence of a character in a wide string
	/// wchar_t *wcsrchr(
	///   const wchar_t *str,
	///   wchar_t c
	/// );
	/// </summary>
	[DllModuleExport(8)]
	private uint wcsrchr(in LpcWStr str, int c)
	{
		var s = str.ToString() ?? string.Empty;
		var ch = (char)c;
		_logger.LogInformation("[msvcrt] wcsrchr(str=\"{S}\", c='{Ch}')", s, ch);
		
		// Find last occurrence of character
		var index = s.LastIndexOf(ch);
		
		if (index < 0)
		{
			return 0; // Not found, return NULL
		}
		
		// Calculate pointer to character in original string
		// Wide chars are 2 bytes each, so multiply index by 2
		var strPtr = str.Address;
		return strPtr + (uint)(index * 2);
	}
}
}
