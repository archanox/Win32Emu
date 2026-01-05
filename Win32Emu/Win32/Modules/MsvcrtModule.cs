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
		
		// CPU instance is set by TryInvokeUnsafe before calling exported methods
		// Cannot be passed as parameter to [DllModuleExport] methods as it breaks source generation
		private ICpu? _cpu;
		
		// Dispatcher for handling nested syscalls in callbacks
		private Win32Dispatcher? _dispatcher;
		
		// Loaded image for import validation
		private LoadedImage? _image;
		
		// Track patched import stubs to avoid redundant memory writes
		private readonly HashSet<uint> _patchedImportStubs = new();
		
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
		
		// Shift-JIS (CP 932) multibyte character lead byte ranges
		private const byte SHIFTJIS_LEAD_BYTE_RANGE1_START = 0x81;
		private const byte SHIFTJIS_LEAD_BYTE_RANGE1_END = 0x9F;
		private const byte SHIFTJIS_LEAD_BYTE_RANGE2_START = 0xE0;
		private const byte SHIFTJIS_LEAD_BYTE_RANGE2_END = 0xFC;
		
		// MSVC PRNG constants
		private const uint MSVC_RAND_MULTIPLIER = 214013;
		private const uint MSVC_RAND_INCREMENT = 2531011;
		private const int MSVC_RAND_MAX = 0x7FFF;
		
		// Error codes
		private const int EINVAL = 22;
		
		// Callback execution constants
		private const uint CALLBACK_RETURN_ADDRESS = 0xDEADBEEF; // Return address marker for callback execution
		private const int MAX_CALLBACK_STEPS = 100000; // Safety limit to prevent infinite loops in callbacks
		private const int MINIMUM_VALID_EIP = 0x10000; // Minimum valid EIP (avoid NULL and low memory)
		
		// Random number generator state (thread-specific in real MSVCRT, but we use a simple global)
		// Initialized with a proper random seed based on current time
		private uint _randomSeed;
		
		// Timezone variables - initialized from .NET environment
		private readonly int _daylight;
		private readonly int _timezone;
		private readonly int _dstbias;
		
		// Cached pointers for timezone variables (to avoid memory leak from repeated allocations)
		private uint _daylightPtr;
		private uint _timezonePtr;
		private uint _dstbiasPtr;
		
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
			
			// Initialize random seed with a proper random value based on current time
			// Use a combination of ticks and process-specific data for better randomness
			_randomSeed = unchecked((uint)(DateTime.Now.Ticks ^ (DateTime.Now.Ticks >> 32)));
			
			// Initialize timezone variables from .NET environment
			var tz = TimeZoneInfo.Local;
			_daylight = tz.SupportsDaylightSavingTime ? 1 : 0;
			_timezone = -(int)tz.BaseUtcOffset.TotalSeconds; // MSVCRT uses seconds west of UTC (negative of UTC offset)
			
			// Get DST bias from adjustment rules if available
			var adjustmentRules = tz.GetAdjustmentRules();
			if (adjustmentRules.Length > 0)
			{
				// Use the most recent adjustment rule
				var latestRule = adjustmentRules[adjustmentRules.Length - 1];
				_dstbias = -(int)latestRule.DaylightDelta.TotalSeconds; // MSVCRT uses negative value
			}
			else
			{
				_dstbias = -3600; // Default to -1 hour if no DST rules available
			}
			
			// Allocate memory for timezone pointers once to avoid memory leaks
			_daylightPtr = _env.HeapAlloc(0, 4);
			_env.MemWrite32(_daylightPtr, (uint)_daylight);
			
			_timezonePtr = _env.HeapAlloc(0, 4);
			_env.MemWrite32(_timezonePtr, (uint)_timezone);
			
			_dstbiasPtr = _env.HeapAlloc(0, 4);
			_env.MemWrite32(_dstbiasPtr, (uint)_dstbias);
		}

		public string Name => "MSVCRT.DLL";

		public void SetDispatcher(Win32Dispatcher dispatcher)
		{
			_dispatcher = dispatcher;
		}

		public void SetLoadedImage(LoadedImage image)
		{
			_image = image;
		}

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
					returnValue = (uint)fprintf(a.UInt32(0), a.LpcStr(1), a.UInt32(2));
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
				case "_STRNICMP":
					returnValue = (uint)_strnicmp(a.LpcStr(0), a.LpcStr(1), a.UInt32(2));
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
				case "_ISMBBLEAD":
					returnValue = (uint)_ismbblead(a.Int32(0));
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
				case "SIN":
					sin();
					returnValue = 0;
					return true;
				case "SQRT":
					sqrt();
					returnValue = 0;
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
				case "RAND":
					returnValue = (uint)rand();
					return true;
				case "SRAND":
					srand(a.UInt32(0));
					returnValue = 0;
					return true;
				case "RAND_S":
					returnValue = (uint)rand_s(a.UInt32(0));
					return true;
				case "SYSTEM":
					returnValue = (uint)system(a.LpcStr(0));
					return true;
				case "_WSYSTEM":
					returnValue = (uint)_wsystem(a.LpcWStr(0));
					return true;
				case "_SLEEP":
					_sleep(a.UInt32(0));
					returnValue = 0;
					return true;
				case "_BEEP":
					_beep(a.UInt32(0), a.UInt32(1));
					returnValue = 0;
					return true;
				case "_LFIND":
					returnValue = _lfind(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;
				case "_LSEARCH":
					returnValue = _lsearch(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;
				case "BSEARCH":
					returnValue = bsearch(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;
				case "_TZSET":
					_tzset();
					returnValue = 0;
					return true;
				case "__P__DAYLIGHT":
					returnValue = __p__daylight();
					return true;
				case "__P__TIMEZONE":
					returnValue = __p__timezone();
					return true;
				case "__P__DSTBIAS":
					returnValue = __p__dstbias();
					return true;
				case "_STRLWR":
					returnValue = _strlwr(a.UInt32(0));
					return true;
				case "_STRUPR":
					returnValue = _strupr(a.UInt32(0));
					return true;
				case "_STRSET":
					returnValue = _strset(a.UInt32(0), a.Int32(1));
					return true;
				case "_STRNSET":
					returnValue = _strnset(a.UInt32(0), a.Int32(1), a.UInt32(2));
					return true;
				case "_LTOA":
					returnValue = _ltoa(a.Int32(0), a.UInt32(1), a.Int32(2));
					return true;
				case "_ULTOA":
					returnValue = _ultoa(a.UInt32(0), a.UInt32(1), a.Int32(2));
					return true;
				case "_I64TOA":
					{
						// Read 64-bit value as two 32-bit values (low, high)
						var low = a.UInt32(0);
						var high = a.UInt32(1);
						var value = ((long)high << 32) | low;
						returnValue = _i64toa(value, a.UInt32(2), a.Int32(3));
					}
					return true;
				case "_UI64TOA":
					{
						// Read 64-bit value as two 32-bit values (low, high)
						var low = a.UInt32(0);
						var high = a.UInt32(1);
						var value = ((ulong)high << 32) | low;
						returnValue = _ui64toa(value, a.UInt32(2), a.Int32(3));
					}
					return true;
				case "_WCSLWR":
					returnValue = _wcslwr(a.UInt32(0));
					return true;
				case "_WCSUPR":
					returnValue = _wcsupr(a.UInt32(0));
					return true;
				case "STRTOK":
					returnValue = strtok(a.UInt32(0), a.LpcStr(1));
					return true;
				case "_SWAB":
					_swab(a.UInt32(0), a.UInt32(1), a.Int32(2));
					returnValue = 0;
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

		// Cache for _iob array pointer
		private uint _iobArrayPtr = 0;
		
		[DllModuleExport(0)]
		private uint __p__iob()
		{
			_logger.LogInformation("[msvcrt] __p__iob()");
			
			// Return consistent pointer to IO buffer array (stdin, stdout, stderr)
			// Each FILE structure is 32 bytes in MSVC runtime
			// We need stdin (offset 0), stdout (offset 32), stderr (offset 64)
			if (_iobArrayPtr == 0)
			{
				_iobArrayPtr = _env.HeapAlloc(0, 96); // 3 FILE structures * 32 bytes
				_logger.LogInformation("[msvcrt] __p__iob() allocated _iob array at 0x{Ptr:X8}", _iobArrayPtr);
			}
			
			return _iobArrayPtr;
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

		/// <summary>
		/// Helper method to parse command line into arguments.
		/// Splits on spaces while respecting quoted sections.
		/// </summary>
		private List<string> ParseCommandLine(string cmdLine)
		{
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
			
			return args;
		}

		[DllModuleExport(20)]
		private int __getmainargs(uint pargc, uint pargv, uint penv, int doWildcard, uint startupInfo)
		{
			_logger.LogInformation("[msvcrt] __getmainargs(pargc=0x{Pargc:X8}, pargv=0x{Pargv:X8}, penv=0x{Penv:X8}, doWildcard={DoWildcard}, startupInfo=0x{StartupInfo:X8})", 
				pargc, pargv, penv, doWildcard, startupInfo);

			// Parse command line into argc/argv
			var cmdLinePtr = _env.CommandLinePtr;
			var cmdLine = cmdLinePtr != 0 ? _env.ReadAnsiString(cmdLinePtr) : "";
			var args = ParseCommandLine(cmdLine);
			
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
			var args = ParseCommandLine(cmdLine);
			
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
			// Return pointer to Unicode command line string
			return _env.CommandLinePtrW;
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
			
			// Call registered exit functions in reverse order (LIFO)
			// This is what _cexit does - call exit handlers without terminating the process
			var handlerCount = _exitFunctions.Count;
			_logger.LogDebug("[msvcrt] _cexit: Found {Count} registered exit handlers", handlerCount);
			
			if (handlerCount == 0)
			{
				// Nothing to do; just return without terminating the process
				return;
			}
			
			// Take a snapshot to ensure a stable iteration order and avoid mutating the collection
			// Handlers must be called in reverse registration order (LIFO)
			var handlers = _exitFunctions.ToArray();
			var executedCount = 0;
			var successCount = 0;
			
			for (var i = handlers.Length - 1; i >= 0; i--)
			{
				var funcPtr = handlers[i];
				
				if (funcPtr == 0)
				{
					_logger.LogDebug("[msvcrt] _cexit: Skipping NULL exit handler at index {Index}", i);
					continue;
				}
				
				executedCount++;
				_logger.LogDebug("[msvcrt] _cexit: Calling exit handler #{Index} at 0x{FuncPtr:X8}", executedCount, funcPtr);
				
				if (ExecuteCallback(funcPtr, "_cexit"))
				{
					successCount++;
					_logger.LogDebug("[msvcrt] _cexit: Exit handler #{Index} completed successfully", executedCount);
				}
				else
				{
					_logger.LogWarning("[msvcrt] _cexit: Exit handler #{Index} at 0x{FuncPtr:X8} failed to execute", executedCount, funcPtr);
				}
			}
			
			_logger.LogInformation("[msvcrt] _cexit: Executed {Success}/{Total} exit handlers successfully", successCount, executedCount);
			
			// Note: _cexit does NOT terminate the process, only calls cleanup handlers
		}

		[DllModuleExport(8)]
		private void _initterm(uint start, uint end)
		{
			_logger.LogInformation("[msvcrt] _initterm(start=0x{Start:X8}, end=0x{End:X8})", start, end);
			
			// Call initializers between start and end
			// The start and end pointers point to an array of function pointers
			// Each non-NULL function pointer should be called with no arguments
			
			if (start == 0 || end == 0 || start >= end)
			{
				_logger.LogWarning("[msvcrt] _initterm: Invalid range");
				return;
			}
			
			// Win32 executables use 32-bit pointers (4 bytes each)
			const int POINTER_SIZE = 4;
			
			var initializerCount = 0;
			var successCount = 0;
			
			// Iterate through the function pointer array
			for (uint addr = start; addr < end; addr += POINTER_SIZE)
			{
				var funcPtr = _env.Memory.Read32(addr);
				
				if (funcPtr != 0)
				{
					initializerCount++;
					_logger.LogDebug("[msvcrt] _initterm: Calling initializer #{Index} at 0x{FuncPtr:X8}", initializerCount, funcPtr);
					
					// Execute the initializer function
					if (ExecuteCallback(funcPtr, "_initterm"))
					{
						successCount++;
						_logger.LogDebug("[msvcrt] _initterm: Initializer #{Index} completed successfully", initializerCount);
					}
					else
					{
						_logger.LogWarning("[msvcrt] _initterm: Initializer #{Index} at 0x{FuncPtr:X8} failed to execute", initializerCount, funcPtr);
					}
				}
			}
			
			_logger.LogInformation("[msvcrt] _initterm: Executed {Success}/{Total} initializers successfully", successCount, initializerCount);
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
			
			// Call _cexit to log exit handlers (actual handler execution not yet implemented)
			_cexit();
			
			// Then terminate the process
			_logger.LogInformation("[msvcrt] Terminating process with exit code {Code}", code);
			_env.RequestExit();
		}

		[DllModuleExport(12)]
		private int fprintf(uint stream, in LpcStr format, uint args)
		{
			var fmt = format.ToString() ?? string.Empty;
			_logger.LogInformation("[msvcrt] fprintf(stream=0x{Stream:X8}, format=\"{Fmt}\", args=0x{Args:X8})", stream, fmt, args);
			
			// Format the string using the va_list
			var formatted = FormatPrintfString(fmt, args);
			
			// Check if stream is stdout or stderr
			// stdout = _iob + 32 (offset for second FILE structure)
			// stderr = _iob + 64 (offset for third FILE structure)
			if (_iobArrayPtr != 0)
			{
				var stdoutPtr = _iobArrayPtr + 32;
				var stderrPtr = _iobArrayPtr + 64;
				
				if (stream == stdoutPtr)
				{
					_logger.LogDebug("[msvcrt] fprintf detected stdout stream, writing to stdout");
					_env.WriteToStdOutput(formatted);
					return formatted.Length;
				}
				else if (stream == stderrPtr)
				{
					_logger.LogDebug("[msvcrt] fprintf detected stderr stream, writing to stderr");
					_env.WriteToStdError(formatted);
					return formatted.Length;
				}
			}
			
			// For unknown streams, just log and return success
			_logger.LogWarning("[msvcrt] fprintf to unknown stream 0x{Stream:X8}, output: {Output}", stream, formatted);
			return formatted.Length;
		}

		[DllModuleExport(8)]
		private int fputs(in LpcStr str, uint stream)
		{
			var s = str.ToString() ?? string.Empty;
			_logger.LogInformation("[msvcrt] fputs(str=\"{S}\", stream=0x{Stream:X8})", s, stream);
			
			// Check if stream is stdout or stderr
			if (_iobArrayPtr != 0)
			{
				var stdoutPtr = _iobArrayPtr + 32;
				var stderrPtr = _iobArrayPtr + 64;
				
				if (stream == stdoutPtr)
				{
					_logger.LogDebug("[msvcrt] fputs detected stdout stream, writing to stdout");
					_env.WriteToStdOutput(s);
					return 0; // Success
				}
				else if (stream == stderrPtr)
				{
					_logger.LogDebug("[msvcrt] fputs detected stderr stream, writing to stderr");
					_env.WriteToStdError(s);
					return 0; // Success
				}
			}
			
			// For unknown streams, just log and return success
			_logger.LogWarning("[msvcrt] fputs to unknown stream 0x{Stream:X8}, output: {Output}", stream, s);
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
			
			// Format the string using the va_list
			var formatted = FormatPrintfString(fmt, args);
			
			// For now, we treat all streams as stdout since we don't have proper FILE* implementation
			_env.WriteToStdOutput(formatted);
			
			return formatted.Length;
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

	/// <summary>
	/// _strnicmp - Compare two strings case-insensitively up to n characters
	/// Performs lexicographic comparison of two null-terminated strings up to a maximum count,
	/// ignoring case differences.
	/// Returns a negative value if str1 is less than str2, 0 if they are equal (within count characters),
	/// and a positive value if str1 is greater than str2.
	/// </summary>
	[DllModuleExport(12)]
	private int _strnicmp(in LpcStr str1, in LpcStr str2, uint count)
	{
		var s1 = str1.ToString() ?? string.Empty;
		var s2 = str2.ToString() ?? string.Empty;
		_logger.LogInformation("[msvcrt] _strnicmp(\"{S1}\", \"{S2}\", count={Count})", s1, s2, count);
		
		// Compare strings up to count characters, case-insensitive
		var compareLength = (int)Math.Min(count, Math.Min(s1.Length, s2.Length));
		var result = string.Compare(s1, 0, s2, 0, compareLength, StringComparison.OrdinalIgnoreCase);
		
		// If strings are equal up to compareLength, check if one is shorter within count
		if (result == 0 && compareLength < count)
		{
			// If one string is shorter, it compares as "less than" the longer string
			if (s1.Length < s2.Length)
			{
				return -1;
			}
			if (s2.Length < s1.Length)
			{
				return 1;
			}
		}
		
		return result;
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
		
		// Format the string using the va_list (args points to the first variadic argument)
		var formatted = FormatPrintfString(fmt, args);
		
		// Write to stdout
		_env.WriteToStdOutput(formatted);
		
		return formatted.Length;
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
	/// _ismbblead - Determines if a byte is a lead byte of a multibyte character
	/// For Japanese Shift-JIS (CP 932): lead bytes are 0x81-0x9F and 0xE0-0xFC
	/// For most Western code pages (like CP 1252): no lead bytes (returns 0)
	/// Returns non-zero if the byte is a lead byte, 0 otherwise
	/// </summary>
	[DllModuleExport(4)]
	private int _ismbblead(int c)
	{
		_logger.LogDebug("[msvcrt] _ismbblead(0x{C:X2})", c);
		
		// Get the current code page from the environment
		// For now, we'll use a simplified implementation that assumes CP 932 (Japanese Shift-JIS)
		// as the most common MBCS code page that uses lead bytes
		
		// Extract the byte value (only care about lower 8 bits)
		var byteVal = (byte)(c & 0xFF);
		
		// Check if byte is a lead byte for Shift-JIS (CP 932)
		// Lead byte ranges: 0x81-0x9F and 0xE0-0xFC
		if ((byteVal >= SHIFTJIS_LEAD_BYTE_RANGE1_START && byteVal <= SHIFTJIS_LEAD_BYTE_RANGE1_END) || 
		    (byteVal >= SHIFTJIS_LEAD_BYTE_RANGE2_START && byteVal <= SHIFTJIS_LEAD_BYTE_RANGE2_END))
		{
			_logger.LogDebug("[msvcrt] _ismbblead: 0x{ByteVal:X2} is a lead byte", byteVal);
			return 1; // Non-zero indicates it's a lead byte
		}
		
		_logger.LogDebug("[msvcrt] _ismbblead: 0x{ByteVal:X2} is not a lead byte", byteVal);
		return 0; // Zero indicates it's not a lead byte
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
	/// Accesses public FPU methods on JitCpu
	/// </summary>
	private long AccessFpuAndConvert()
	{
		// _cpu is guaranteed to be set by TryInvokeUnsafe before this method is called
		if (_cpu == null)
		{
			throw new InvalidOperationException("CPU instance is not available - this should never happen");
		}
		
		// Access FPU state through JitCpu
		if (_cpu is not Cpu.Jit.JitCpu jitCpu)
		{
			_logger.LogWarning("[msvcrt] __ftol: Unsupported CPU type {CpuType}, returning 0", 
				_cpu.GetType().Name);
			return 0;
		}
		
		double st0 = jitCpu.FpuGetSt(0);
		jitCpu.FpuPop();
		
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
		
		// Access FPU state through JitCpu
		if (_cpu is Cpu.Jit.JitCpu jitCpu)
		{
			// Reset FPU by calling the FINIT instruction behavior
			// This sets control word to 0x037F, clears status word, and sets tag word to 0xFFFF
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
	[DllModuleExport(0)]
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
	[DllModuleExport(4)]
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
	[DllModuleExport(4)]
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
	[DllModuleExport(16)]
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
	[DllModuleExport(16)]
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

	/// <summary>
	/// sin - Compute sine of angle
	/// Reads angle in radians from FPU ST(0), computes sine, and replaces ST(0) with result
	/// NOTE: This is a stub that reads and pops the value but doesn't compute or push the result.
	/// Real implementation would require FPU manipulation that isn't accessible.
	/// </summary>
	[DllModuleExport(0, IsStub = true)]
	private void sin()
	{
		_logger.LogInformation("[msvcrt] sin()");
		
		// Read and log the FPU stack value (stub - doesn't compute result)
		if (_cpu is Cpu.Jit.JitCpu jitCpu)
		{
			var angle = jitCpu.FpuGetSt(0);
			_logger.LogInformation("[msvcrt] sin: angle={Angle}", angle);
			// Cannot set ST(0) as FpuSetSt is not public
			// The calling code expects ST(0) to contain sin(angle)
		}
		else
		{
			_logger.LogWarning("[msvcrt] sin: Unsupported CPU type {CpuType}, no-op", _cpu?.GetType().Name ?? "null");
		}
	}

	/// <summary>
	/// sqrt - Compute square root
	/// Reads value from FPU ST(0), computes square root, and replaces ST(0) with result
	/// NOTE: This is a stub that reads the value but doesn't compute or replace the result.
	/// Real implementation would require FPU manipulation that isn't accessible.
	/// </summary>
	[DllModuleExport(0, IsStub = true)]
	private void sqrt()
	{
		_logger.LogInformation("[msvcrt] sqrt()");
		
		// Read and log the FPU stack value (stub - doesn't compute result)
		if (_cpu is Cpu.Jit.JitCpu jitCpu)
		{
			var value = jitCpu.FpuGetSt(0);
			_logger.LogInformation("[msvcrt] sqrt: value={Value}", value);
			// Cannot set ST(0) as FpuSetSt is not public
			// The calling code expects ST(0) to contain sqrt(value)
		}
		else
		{
			_logger.LogWarning("[msvcrt] sqrt: Unsupported CPU type {CpuType}, no-op", _cpu?.GetType().Name ?? "null");
		}
	}

	/// <summary>
	/// Formats a printf-style format string with arguments from a va_list pointer.
	/// Supports common format specifiers: %s (string), %d/%i (int), %u (uint), %x/%X (hex), %c (char), %% (literal %).
	/// This implementation is based on the User32Module's FormatStringFromVaList with proper handling
	/// of variadic arguments.
	/// </summary>
	private string FormatPrintfString(string format, uint vaListPtr)
	{
		var result = new StringBuilder();
		uint currentArgPtr = vaListPtr;
		
		for (int i = 0; i < format.Length; i++)
		{
			if (format[i] == '%')
			{
				// If '%' is the last character, treat it as a literal percent.
				// This matches the existing behavior (it would otherwise fall through
				// to the else-branch) and makes the intent explicit.
				if (i + 1 >= format.Length)
				{
					result.Append('%');
					continue;
				}
				
				i++; // Skip the %
				
				// Handle %% (literal %)
				if (format[i] == '%')
				{
					result.Append('%');
					continue;
				}
				
				// Parse format specifier (simplified - doesn't handle width, precision, etc.)
				char specifier = format[i];
				
				// Validate we can read memory safely
				// Check if currentArgPtr is within reasonable bounds (not null, not at end of 32-bit address space)
				if (currentArgPtr == 0 || currentArgPtr > 0xFFFFFFF0)
				{
					_logger.LogWarning("[msvcrt] FormatPrintfString: Invalid va_list pointer 0x{CurrentArgPtr:X8}", currentArgPtr);
					result.Append("%[invalid]");
					break;
				}
				
				try
				{
					switch (specifier)
					{
						case 's': // String pointer
							var strAddr = _env.Memory.Read32(currentArgPtr);
							currentArgPtr += 4;
							if (strAddr != 0)
							{
								var str = new LpcStr(strAddr, _env.Memory).ToString() ?? string.Empty;
								result.Append(str);
							}
							else
							{
								result.Append("(null)");
							}
							break;
						
						case 'd': // Signed decimal integer
						case 'i':
							var intVal = (int)_env.Memory.Read32(currentArgPtr);
							currentArgPtr += 4;
							result.Append(intVal);
							break;
						
						case 'u': // Unsigned decimal integer
							var uintVal = _env.Memory.Read32(currentArgPtr);
							currentArgPtr += 4;
							result.Append(uintVal);
							break;
						
						case 'x': // Unsigned hexadecimal (lowercase)
							var hexVal = _env.Memory.Read32(currentArgPtr);
							currentArgPtr += 4;
							result.Append(hexVal.ToString("x"));
							break;
						
						case 'X': // Unsigned hexadecimal (uppercase)
							var hexValUpper = _env.Memory.Read32(currentArgPtr);
							currentArgPtr += 4;
							result.Append(hexValUpper.ToString("X"));
							break;
						
						case 'c': // Character
							var charVal = (char)_env.Memory.Read32(currentArgPtr);
							currentArgPtr += 4;
							result.Append(charVal);
							break;
						
						default:
							// Unknown specifier - just append it as-is
							result.Append('%');
							result.Append(specifier);
							currentArgPtr += 4; // Still consume an argument
							break;
					}
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "[msvcrt] FormatPrintfString: Error reading argument at 0x{CurrentArgPtr:X8}", currentArgPtr);
					result.Append("%[error]");
					break;
				}
			}
			else
			{
				result.Append(format[i]);
			}
		}
		
		return result.ToString();
	}

	/// <summary>
	/// rand - Generate pseudo-random number
	/// Returns pseudo-random integer in the range 0 to RAND_MAX (32767)
	/// Uses the algorithm from MSVC runtime
	/// </summary>
	[DllModuleExport(0)]
	private int rand()
	{
		_logger.LogDebug("[msvcrt] rand()");
		
		// MSVC algorithm: seed = seed * 214013 + 2531011; return (seed >> 16) & 0x7FFF;
		_randomSeed = _randomSeed * MSVC_RAND_MULTIPLIER + MSVC_RAND_INCREMENT;
		var result = (int)((_randomSeed >> 16) & MSVC_RAND_MAX);
		
		_logger.LogDebug("[msvcrt] rand: returning {Result}", result);
		return result;
	}

	/// <summary>
	/// srand - Seed pseudo-random number generator
	/// Sets the seed for the random number generator
	/// </summary>
	[DllModuleExport(4)]
	private void srand(uint seed)
	{
		_logger.LogInformation("[msvcrt] srand(seed={Seed})", seed);
		_randomSeed = seed;
	}

	/// <summary>
	/// rand_s - Generate cryptographically secure random number
	/// Returns cryptographically secure random integer
	/// </summary>
	[DllModuleExport(4)]
	private int rand_s(uint pval)
	{
		_logger.LogInformation("[msvcrt] rand_s(pval=0x{Pval:X8})", pval);
		
		if (pval == 0)
		{
			_env.LastError = (uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER;
			return EINVAL;
		}
		
		// Generate cryptographically secure random number using System.Security.Cryptography
		var bytes = new byte[4];
		System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
		var value = BitConverter.ToUInt32(bytes, 0);
		_env.MemWrite32(pval, value);
		
		_logger.LogDebug("[msvcrt] rand_s: generated {Value}", value);
		return 0; // Success
	}

	/// <summary>
	/// system - Execute command via command processor
	/// Executes a command string via the system command processor (cmd.exe on Windows)
	/// </summary>
	[DllModuleExport(4)]
	private int system(in LpcStr command)
	{
		var cmd = command.ToString();
		_logger.LogInformation("[msvcrt] system(command=\"{Cmd}\")", cmd);
		
		// If command is NULL, return non-zero to indicate command processor is available
		if (string.IsNullOrEmpty(cmd))
		{
			return 1; // Command processor is available
		}
		
		// For security and simplicity, we don't actually execute system commands
		// A real implementation would use CreateProcess to launch cmd.exe /c <command>
		// and wait for it to complete, returning the exit code
		_logger.LogWarning("[msvcrt] system: Command execution not implemented for security reasons");
		
		// Return 0 to indicate success (command executed)
		return 0;
	}

	/// <summary>
	/// _wsystem - Execute command via command processor (wide character version)
	/// Executes a command string via the system command processor
	/// </summary>
	[DllModuleExport(4)]
	private int _wsystem(in LpcWStr command)
	{
		var cmd = command.ToString();
		_logger.LogInformation("[msvcrt] _wsystem(command=\"{Cmd}\")", cmd);
		
		// If command is NULL, return non-zero to indicate command processor is available
		if (string.IsNullOrEmpty(cmd))
		{
			return 1; // Command processor is available
		}
		
		// For security and simplicity, we don't actually execute system commands
		_logger.LogWarning("[msvcrt] _wsystem: Command execution not implemented for security reasons");
		
		// Return 0 to indicate success (command executed)
		return 0;
	}

	/// <summary>
	/// _sleep - Sleep for specified milliseconds
	/// Suspends execution of the current thread for the specified duration
	/// </summary>
	[DllModuleExport(4)]
	private void _sleep(uint milliseconds)
	{
		_logger.LogInformation("[msvcrt] _sleep(milliseconds={Milliseconds})", milliseconds);
		
		// Sleep for at least 1ms if 0 is passed (matches Wine behavior)
		var sleepTime = milliseconds == 0 ? 1 : milliseconds;
		
		// In a real emulator, we would pause the emulated thread
		// For now, just log the sleep request
		_logger.LogDebug("[msvcrt] _sleep: would sleep for {SleepTime}ms", sleepTime);
	}

	/// <summary>
	/// _beep - Produce system beep
	/// Generates a tone on the speaker at the specified frequency and duration
	/// </summary>
	[DllModuleExport(8)]
	private void _beep(uint frequency, uint duration)
	{
		_logger.LogInformation("[msvcrt] _beep(frequency={Frequency}, duration={Duration})", frequency, duration);
		
		// In a real implementation, this would call the Win32 Beep function
		// For an emulator, we just log the beep request
		_logger.LogDebug("[msvcrt] _beep: would beep at {Frequency}Hz for {Duration}ms", frequency, duration);
	}

	/// <summary>
	/// _lfind - Linear search for element in array
	/// Performs a linear search for a key in an array
	/// </summary>
	[DllModuleExport(20, IsStub = true)]
	private uint _lfind(uint key, uint base_, uint num, uint width, uint compare)
	{
		_logger.LogInformation("[msvcrt] _lfind(key=0x{Key:X8}, base=0x{Base:X8}, num=0x{Num:X8}, width={Width}, compare=0x{Compare:X8})", 
			key, base_, num, width, compare);
		
		if (base_ == 0 || num == 0 || compare == 0)
		{
			return 0; // NULL
		}
		
		// Read the number of elements
		var count = _env.Memory.Read32(num);
		
		if (count == 0)
		{
			return 0; // Not found
		}
		
		// Linear search through array
		for (uint i = 0; i < count; i++)
		{
			var elementPtr = base_ + (i * width);
			
			// Call comparison function: int compare(const void *key, const void *element)
			// For now, we can't actually call the comparison function as it requires
			// setting up a proper call context. Return NULL to indicate not found.
			_logger.LogDebug("[msvcrt] _lfind: would compare element at 0x{ElementPtr:X8}", elementPtr);
		}
		
		_logger.LogDebug("[msvcrt] _lfind: not found (comparison not implemented)");
		return 0; // Not found
	}

	/// <summary>
	/// _lsearch - Linear search for element in array, add if not found
	/// Performs a linear search for a key in an array, adds it if not found
	/// </summary>
	[DllModuleExport(20, IsStub = true)]
	private uint _lsearch(uint key, uint base_, uint num, uint width, uint compare)
	{
		_logger.LogInformation("[msvcrt] _lsearch(key=0x{Key:X8}, base=0x{Base:X8}, num=0x{Num:X8}, width={Width}, compare=0x{Compare:X8})", 
			key, base_, num, width, compare);
		
		// Try to find the element first
		var found = _lfind(key, base_, num, width, compare);
		
		if (found != 0)
		{
			return found; // Found, return pointer to element
		}
		
		// Not found, add to end of array
		var count = _env.Memory.Read32(num);
		var newElementPtr = base_ + (count * width);
		
		// Copy key to new element (simple memcpy)
		for (uint i = 0; i < width; i++)
		{
			var b = _env.Memory.Read8(key + i);
			_env.Memory.Write8(newElementPtr + i, b);
		}
		
		// Increment count
		_env.MemWrite32(num, count + 1);
		
		_logger.LogDebug("[msvcrt] _lsearch: added element at 0x{NewElementPtr:X8}, new count={NewCount}", newElementPtr, count + 1);
		return newElementPtr;
	}

	/// <summary>
	/// bsearch - Binary search for element in sorted array
	/// Performs a binary search for a key in a sorted array
	/// </summary>
	[DllModuleExport(20, IsStub = true)]
	private uint bsearch(uint key, uint base_, uint nmemb, uint size, uint compar)
	{
		_logger.LogInformation("[msvcrt] bsearch(key=0x{Key:X8}, base=0x{Base:X8}, nmemb={Nmemb}, size={Size}, compar=0x{Compar:X8})", 
			key, base_, nmemb, size, compar);
		
		if (size == 0 || compar == 0 || base_ == 0 || nmemb == 0)
		{
			return 0; // NULL
		}
		
		// Binary search implementation
		// For now, we can't actually call the comparison function as it requires
		// setting up a proper call context. Return NULL to indicate not found.
		_logger.LogDebug("[msvcrt] bsearch: comparison function calls not implemented, returning NULL");
		return 0; // Not found
	}

	/// <summary>
	/// _tzset - Set time zone information
	/// Initializes time zone information from environment variables
	/// </summary>
	[DllModuleExport(0)]
	private void _tzset()
	{
		_logger.LogInformation("[msvcrt] _tzset()");
		
		// In a real implementation, this would parse the TZ environment variable
		// and update _daylight, _timezone, and _dstbias accordingly
		// For now, we keep the default PST timezone settings
		_logger.LogDebug("[msvcrt] _tzset: using default PST timezone (_timezone={Timezone}, _daylight={Daylight}, _dstbias={Dstbias})", 
			_timezone, _daylight, _dstbias);
	}

	/// <summary>
	/// __p__daylight - Get pointer to daylight saving time flag
	/// Returns pointer to the _daylight variable
	/// </summary>
	[DllModuleExport(0)]
	private uint __p__daylight()
	{
		_logger.LogDebug("[msvcrt] __p__daylight()");
		
		// Return cached pointer to avoid memory leaks
		return _daylightPtr;
	}

	/// <summary>
	/// __p__timezone - Get pointer to timezone offset
	/// Returns pointer to the _timezone variable (offset in seconds)
	/// </summary>
	[DllModuleExport(0)]
	private uint __p__timezone()
	{
		_logger.LogDebug("[msvcrt] __p__timezone()");
		
		// Return cached pointer to avoid memory leaks
		return _timezonePtr;
	}

	/// <summary>
	/// __p__dstbias - Get pointer to DST bias
	/// Returns pointer to the _dstbias variable (DST offset in seconds)
	/// </summary>
	[DllModuleExport(0)]
	private uint __p__dstbias()
	{
		_logger.LogDebug("[msvcrt] __p__dstbias()");
		
		// Return cached pointer to avoid memory leaks
		return _dstbiasPtr;
	}

	/// <summary>
	/// _strlwr - Convert string to lowercase
	/// Converts all uppercase characters in a string to lowercase in-place
	/// </summary>
	[DllModuleExport(4)]
	private uint _strlwr(uint str)
	{
		_logger.LogInformation("[msvcrt] _strlwr(str=0x{Str:X8})", str);
		
		if (str == 0)
		{
			return 0; // NULL pointer
		}
		
		var s = _env.ReadAnsiString(str);
		if (string.IsNullOrEmpty(s))
		{
			return str;
		}
		
		var lowered = s.ToLowerInvariant();
		_env.WriteAnsiStringAt(str, lowered);
		
		return str;
	}

	/// <summary>
	/// _strupr - Convert string to uppercase
	/// Converts all lowercase characters in a string to uppercase in-place
	/// </summary>
	[DllModuleExport(4)]
	private uint _strupr(uint str)
	{
		_logger.LogInformation("[msvcrt] _strupr(str=0x{Str:X8})", str);
		
		if (str == 0)
		{
			return 0; // NULL pointer
		}
		
		var s = _env.ReadAnsiString(str);
		if (string.IsNullOrEmpty(s))
		{
			return str;
		}
		
		var uppered = s.ToUpperInvariant();
		_env.WriteAnsiStringAt(str, uppered);
		
		return str;
	}

	/// <summary>
	/// _strset - Set all characters in string to a value
	/// Sets all characters in a string (except the null terminator) to the specified character
	/// </summary>
	[DllModuleExport(8)]
	private uint _strset(uint str, int value)
	{
		_logger.LogInformation("[msvcrt] _strset(str=0x{Str:X8}, value={Value})", str, value);
		
		if (str == 0)
		{
			return 0; // NULL pointer
		}
		
		var s = _env.ReadAnsiString(str);
		if (string.IsNullOrEmpty(s))
		{
			return str;
		}
		
		var ch = (char)(byte)value;
		var result = new string(ch, s.Length);
		_env.WriteAnsiStringAt(str, result);
		
		return str;
	}

	/// <summary>
	/// _strnset - Set first n characters in string to a value
	/// Sets the first n characters in a string to the specified character
	/// </summary>
	[DllModuleExport(12)]
	private uint _strnset(uint str, int value, uint count)
	{
		_logger.LogInformation("[msvcrt] _strnset(str=0x{Str:X8}, value={Value}, count={Count})", str, value, count);
		
		if (str == 0)
		{
			return 0; // NULL pointer
		}
		
		var s = _env.ReadAnsiString(str);
		if (string.IsNullOrEmpty(s))
		{
			return str;
		}
		
		var ch = (char)(byte)value;
		var setCount = (int)Math.Min(count, (uint)s.Length);
		var result = new string(ch, setCount) + s.Substring(setCount);
		_env.WriteAnsiStringAt(str, result);
		
		return str;
	}

	/// <summary>
	/// _ltoa - Convert long integer to string
	/// Converts a long integer value to a string representation in the specified radix
	/// </summary>
	[DllModuleExport(12)]
	private uint _ltoa(int value, uint buffer, int radix)
	{
		_logger.LogInformation("[msvcrt] _ltoa(value={Value}, buffer=0x{Buffer:X8}, radix={Radix})", value, buffer, radix);
		
		if (buffer == 0)
		{
			return 0; // NULL buffer
		}
		
		string result;
		if (radix == 10)
		{
			result = value.ToString();
		}
		else if (radix == 16)
		{
			// For hex, octal, and binary, treat as unsigned for non-decimal bases
			result = ((uint)value).ToString("x");
		}
		else if (radix == 8)
		{
			result = Convert.ToString((uint)value, 8);
		}
		else if (radix == 2)
		{
			result = Convert.ToString((uint)value, 2);
		}
		else
		{
			// Unsupported radix, default to decimal
			result = value.ToString();
		}
		
		_env.WriteAnsiStringAt(buffer, result);
		return buffer;
	}

	/// <summary>
	/// _ultoa - Convert unsigned long integer to string
	/// Converts an unsigned long integer value to a string representation in the specified radix
	/// </summary>
	[DllModuleExport(12)]
	private uint _ultoa(uint value, uint buffer, int radix)
	{
		_logger.LogInformation("[msvcrt] _ultoa(value={Value}, buffer=0x{Buffer:X8}, radix={Radix})", value, buffer, radix);
		
		if (buffer == 0)
		{
			return 0; // NULL buffer
		}
		
		string result;
		if (radix == 10)
		{
			result = value.ToString();
		}
		else if (radix == 16)
		{
			result = value.ToString("x");
		}
		else if (radix == 8)
		{
			result = Convert.ToString(value, 8);
		}
		else if (radix == 2)
		{
			result = Convert.ToString(value, 2);
		}
		else
		{
			// Unsupported radix, default to decimal
			result = value.ToString();
		}
		
		_env.WriteAnsiStringAt(buffer, result);
		return buffer;
	}

	/// <summary>
	/// _i64toa - Convert 64-bit integer to string
	/// Converts a 64-bit integer value to a string representation in the specified radix
	/// </summary>
	[DllModuleExport(16)]
	private uint _i64toa(long value, uint buffer, int radix)
	{
		_logger.LogInformation("[msvcrt] _i64toa(value={Value}, buffer=0x{Buffer:X8}, radix={Radix})", value, buffer, radix);
		
		if (buffer == 0)
		{
			return 0; // NULL buffer
		}
		
		string result;
		if (radix == 10)
		{
			result = value.ToString();
		}
		else if (radix == 16)
		{
			// For hex, octal, and binary, treat as unsigned for non-decimal bases
			result = ((ulong)value).ToString("x");
		}
		else if (radix == 8 || radix == 2)
		{
			// Convert.ToString doesn't support ulong, so handle large values specially
			if (value >= 0)
			{
				result = Convert.ToString(value, radix);
			}
			else
			{
				// For negative values in non-decimal bases, convert as unsigned
				result = ConvertUInt64ToString((ulong)value, radix);
			}
		}
		else
		{
			// Unsupported radix, default to decimal
			result = value.ToString();
		}
		
		_env.WriteAnsiStringAt(buffer, result);
		return buffer;
	}

	/// <summary>
	/// _ui64toa - Convert unsigned 64-bit integer to string
	/// Converts an unsigned 64-bit integer value to a string representation in the specified radix
	/// </summary>
	[DllModuleExport(16)]
	private uint _ui64toa(ulong value, uint buffer, int radix)
	{
		_logger.LogInformation("[msvcrt] _ui64toa(value={Value}, buffer=0x{Buffer:X8}, radix={Radix})", value, buffer, radix);
		
		if (buffer == 0)
		{
			return 0; // NULL buffer
		}
		
		string result;
		if (radix == 10)
		{
			result = value.ToString();
		}
		else if (radix == 16)
		{
			result = value.ToString("x");
		}
		else if (radix == 8 || radix == 2)
		{
			// Convert.ToString doesn't support ulong directly, handle manually
			result = ConvertUInt64ToString(value, radix);
		}
		else
		{
			// Unsupported radix, default to decimal
			result = value.ToString();
		}
		
		_env.WriteAnsiStringAt(buffer, result);
		return buffer;
	}
	
	/// <summary>
	/// Helper method to convert ulong to string in specified radix
	/// </summary>
	private string ConvertUInt64ToString(ulong value, int radix)
	{
		if (value == 0)
			return "0";
			
		var chars = "0123456789abcdefghijklmnopqrstuvwxyz";
		var result = new StringBuilder();
		
		while (value > 0)
		{
			result.Insert(0, chars[(int)(value % (ulong)radix)]);
			value /= (ulong)radix;
		}
		
		return result.ToString();
	}

	/// <summary>
	/// _wcslwr - Convert wide string to lowercase
	/// Converts all uppercase characters in a wide string to lowercase in-place
	/// </summary>
	[DllModuleExport(4)]
	private uint _wcslwr(uint str)
	{
		_logger.LogInformation("[msvcrt] _wcslwr(str=0x{Str:X8})", str);
		
		if (str == 0)
		{
			return 0; // NULL pointer
		}
		
		var s = _env.ReadUnicodeString(str);
		if (string.IsNullOrEmpty(s))
		{
			return str;
		}
		
		var lowered = s.ToLowerInvariant();
		
		// Write wide string back to memory manually
		var bytes = Encoding.Unicode.GetBytes(lowered + '\0');
		_env.Memory.WriteBytes(str, bytes);
		
		return str;
	}

	/// <summary>
	/// _wcsupr - Convert wide string to uppercase
	/// Converts all lowercase characters in a wide string to uppercase in-place
	/// </summary>
	[DllModuleExport(4)]
	private uint _wcsupr(uint str)
	{
		_logger.LogInformation("[msvcrt] _wcsupr(str=0x{Str:X8})", str);
		
		if (str == 0)
		{
			return 0; // NULL pointer
		}
		
		var s = _env.ReadUnicodeString(str);
		if (string.IsNullOrEmpty(s))
		{
			return str;
		}
		
		var uppered = s.ToUpperInvariant();
		
		// Write wide string back to memory manually
		var bytes = Encoding.Unicode.GetBytes(uppered + '\0');
		_env.Memory.WriteBytes(str, bytes);
		
		return str;
	}

	// Thread-local static storage for strtok state (per host thread)
	[System.ThreadStatic]
	private static uint _strtokLastPtr;

	/// <summary>
	/// strtok - Tokenize string using delimiters
	/// Finds the next token in a string, using delimiters to separate tokens
	/// </summary>
	[DllModuleExport(8)]
	private uint strtok(uint str, in LpcStr delim)
	{
		var delimStr = delim.ToString() ?? string.Empty;
		_logger.LogInformation("[msvcrt] strtok(str=0x{Str:X8}, delim=\"{Delim}\")", str, delimStr);
		
		if (delimStr.Length == 0)
		{
			return 0; // No delimiters
		}
		
		// If str is NULL, continue from last position
		var startPtr = str != 0 ? str : _strtokLastPtr;
		
		if (startPtr == 0)
		{
			return 0; // No string to tokenize
		}
		
		// Read string from current position
		var remaining = _env.ReadAnsiString(startPtr);
		if (string.IsNullOrEmpty(remaining))
		{
			_strtokLastPtr = 0;
			return 0; // No more tokens
		}
		
		// Skip leading delimiters
		var startIdx = 0;
		while (startIdx < remaining.Length && delimStr.Contains(remaining[startIdx]))
		{
			startIdx++;
		}
		
		if (startIdx >= remaining.Length)
		{
			_strtokLastPtr = 0;
			return 0; // No token found
		}
		
		// Find end of token
		var endIdx = startIdx;
		while (endIdx < remaining.Length && !delimStr.Contains(remaining[endIdx]))
		{
			endIdx++;
		}
		
		// Calculate token address
		var tokenPtr = startPtr + (uint)startIdx;
		
		// Write null terminator after token
		if (endIdx < remaining.Length)
		{
			_env.Memory.Write8(startPtr + (uint)endIdx, 0);
			_strtokLastPtr = startPtr + (uint)endIdx + 1;
		}
		else
		{
			_strtokLastPtr = 0; // No more tokens
		}
		
		return tokenPtr;
	}

	/// <summary>
	/// _swab - Swap bytes in buffer
	/// Swaps adjacent bytes in a buffer (byte swapping)
	/// </summary>
	[DllModuleExport(12)]
	private void _swab(uint src, uint dst, int len)
	{
		_logger.LogInformation("[msvcrt] _swab(src=0x{Src:X8}, dst=0x{Dst:X8}, len={Len})", src, dst, len);
		
		if (src == 0 || dst == 0 || len <= 0)
		{
			return;
		}
		
		// Swap adjacent bytes
		for (int i = 0; i < len - 1; i += 2)
		{
			var b1 = _env.Memory.Read8(src + (uint)i);
			var b2 = _env.Memory.Read8(src + (uint)(i + 1));
			_env.Memory.Write8(dst + (uint)i, b2);
			_env.Memory.Write8(dst + (uint)(i + 1), b1);
		}
	}

	/// <summary>
	/// Handles nested syscalls (import calls and INT 0x80 syscalls) during callback execution.
	/// Based on User32Module.HandleComAndImportCalls but simplified - does not handle COM vtable calls
	/// since MSVCRT callbacks typically only call standard Win32 APIs.
	/// </summary>
	/// <param name="step">The current CPU step result</param>
	/// <param name="cpu">The CPU instance</param>
	/// <param name="memory">The virtual memory instance</param>
	/// <param name="logContext">Context string for logging</param>
	/// <param name="stepDesc">Output parameter for step description (for debugging)</param>
	/// <param name="shouldBreak">Output parameter indicating if execution should stop</param>
	/// <returns>True if the step was handled (import call or INT 0x80 syscall), false if it should be processed normally</returns>
	private bool HandleNestedSyscalls(CpuStepResult step, ICpu cpu, VirtualMemory memory, string logContext, out string? stepDesc, out bool shouldBreak)
	{
		stepDesc = null;
		shouldBreak = false;

		// Check for INT 0x80 syscalls (import stubs trigger INT 0x80)
		// This is the most common case for nested syscalls in callbacks
		if (step.IsSyscall && _image != null && _dispatcher != null)
		{
			// Read the import stub info from the stack (same as HandleSyscallAsync in Emulator.cs)
			// Stack layout:
			// [ESP+0] = return address to import stub (points to RET after CALL to syscall dispatcher)
			// [ESP+4] = return address to callback code
			// [ESP+8+] = function arguments
			
			var esp = cpu.GetRegister("ESP");
			
			// Validate ESP
			if (esp < MemoryRegions.MinValidUserAddress)
			{
				_logger.LogError("[msvcrt] {Context}: ESP=0x{Esp:X8} is too low during nested syscall", logContext, esp);
				shouldBreak = true;
				return true;
			}
			
			// Read return address to import stub
			var retToStub = memory.Read32(esp);
			
			// Validate that the return address points into the import hook range
			if (!MemoryRegions.IsInImportHookRange(retToStub))
			{
				_logger.LogError(
					"[msvcrt] {Context}: Invalid nested syscall return-to-stub address 0x{RetToStub:X8} (ESP=0x{Esp:X8}) - not in import hook range",
					logContext, retToStub, esp);
				shouldBreak = true;
				return true;
			}
			
			// Calculate import stub address (5 bytes before return address for CALL instruction)
			var importStubAddr = retToStub - 5;
			
			// Look up the import
			if (_image.ImportAddressMap.TryGetValue(importStubAddr, out var imp))
			{
				var dll = imp.dll.ToUpperInvariant();
				var name = imp.name;
				_logger.LogDebug("[msvcrt] {Context}: Nested INT 0x80 syscall {Dll}!{Name} from stub at 0x{Stub:X8}", 
					logContext, dll, name, importStubAddr);
				stepDesc = $"INT 0x80 syscall {dll}!{name}";
				
				// Save callee-saved registers
				var saved = CpuHelpers.SaveCalleeSavedRegisters(cpu);
				
				// Adjust ESP to skip the return-to-stub address
				// This allows the dispatcher to read arguments at correct offsets
				var originalEsp = esp;
				cpu.SetRegister("ESP", esp + 4);
				
				// Dispatch the API call
				if (_dispatcher.TryInvoke(dll, name, cpu, memory, out var ret, out var argBytes))
				{
					_logger.LogDebug("[msvcrt] {Context}: Nested syscall {Dll}!{Name} returned 0x{Ret:X8}, argBytes={ArgBytes}", 
						logContext, dll, name, ret, argBytes);
					
					// Set return value in EAX
					cpu.SetRegister("EAX", ret);
					
					// Restore ESP to original value so CPU can execute RET instructions naturally
					// The CPU will execute RET in syscall dispatcher (pops return-to-stub),
					// then RET in import stub (pops return-to-callback and cleans args)
					cpu.SetRegister("ESP", originalEsp);
					
					// Validate restored ESP to detect potential stack corruption
					if (originalEsp < MemoryRegions.MinValidUserAddress)
					{
						_logger.LogError(
							"[msvcrt] {Context}: Restored ESP 0x{Esp:X8} below MinValidUserAddress 0x{MinEsp:X8} after nested syscall {Dll}!{Name}",
							logContext,
							originalEsp,
							MemoryRegions.MinValidUserAddress,
							dll,
							name);
						shouldBreak = true;
						return true;
					}
					
					// Patch the import stub's RET instruction with argBytes for stdcall cleanup
					// The stub RET is at importStubAddr + 5 (after the 5-byte CALL instruction)
					// Format: RET imm16 = 0xC2 <low_byte> <high_byte>
					// Only patch if not already patched to avoid redundant memory writes
					if (argBytes > 0 && argBytes <= 0xFFFF && !_patchedImportStubs.Contains(importStubAddr))
					{
						var retInstrAddr = importStubAddr + 5;
						var opcode = memory.Read8(retInstrAddr);
						if (opcode == 0xC2)
						{
							// Patch the immediate value
							memory.Write8(retInstrAddr + 1, (byte)(argBytes & 0xFF));
							memory.Write8(retInstrAddr + 2, (byte)((argBytes >> 8) & 0xFF));
							_patchedImportStubs.Add(importStubAddr);
							_logger.LogDebug("[msvcrt] {Context}: Patched RET at 0x{RetAddr:X8} with argBytes={ArgBytes}", 
								logContext, retInstrAddr, argBytes);
						}
						else if (opcode == 0xC3)
						{
							// Found RET (0xC3) but need RET imm16 (0xC2) for stdcall cleanup
							// Overwrite with RET imm16 to handle argument cleanup
							memory.Write8(retInstrAddr, 0xC2); // Change to RET imm16
							memory.Write8(retInstrAddr + 1, (byte)(argBytes & 0xFF));
							memory.Write8(retInstrAddr + 2, (byte)((argBytes >> 8) & 0xFF));
							_patchedImportStubs.Add(importStubAddr);
							_logger.LogDebug("[msvcrt] {Context}: Converted RET to RET imm16 at 0x{RetAddr:X8} with argBytes={ArgBytes}", 
								logContext, retInstrAddr, argBytes);
						}
						else
						{
							_logger.LogWarning("[msvcrt] {Context}: Expected RET (0xC3) or RET imm16 (0xC2) at 0x{RetAddr:X8} but found 0x{Opcode:X2}. Skipping patch.", 
								logContext, retInstrAddr, opcode);
						}
					}
					
					// Restore callee-saved registers
					CpuHelpers.RestoreCalleeSavedRegisters(cpu, saved, skipInvalidEbp: true, memorySize: memory.Size);
					
					return true;
				}
				else
				{
					// Function not implemented - simulate return
					_logger.LogWarning("[msvcrt] {Context}: Nested syscall {Dll}!{Name} not implemented, simulating return with 0", 
						logContext, dll, name);
					
					// Try to get arg bytes from metadata
					var simulatedArgBytes = 0;
					try
					{
						simulatedArgBytes = StdCallMeta.GetArgBytes(dll, name);
					}
					catch (System.ArgumentException ex)
					{
						_logger.LogWarning(ex, "[msvcrt] {Context}: Cannot determine argBytes for {Dll}!{Name}, assuming 0", 
							logContext, dll, name);
					}
					catch (System.Collections.Generic.KeyNotFoundException ex)
					{
						_logger.LogWarning(ex, "[msvcrt] {Context}: Cannot determine argBytes for {Dll}!{Name}, assuming 0", 
							logContext, dll, name);
					}
					
					// Set return value in EAX
					cpu.SetRegister("EAX", 0);
					
					// Restore ESP to original value
					cpu.SetRegister("ESP", originalEsp);
					
					// Validate restored ESP to detect potential stack corruption
					if (originalEsp < MemoryRegions.MinValidUserAddress)
					{
						_logger.LogError(
							"[msvcrt] {Context}: Restored ESP 0x{Esp:X8} below MinValidUserAddress 0x{MinEsp:X8} after unimplemented nested syscall {Dll}!{Name}",
							logContext,
							originalEsp,
							MemoryRegions.MinValidUserAddress,
							dll,
							name);
						shouldBreak = true;
						return true;
					}
					
					// Patch the stub's RET if needed
					// Only patch if not already patched to avoid redundant memory writes
					if (simulatedArgBytes > 0 && simulatedArgBytes <= 0xFFFF && !_patchedImportStubs.Contains(importStubAddr))
					{
						var retInstrAddr = importStubAddr + 5;
						var opcode = memory.Read8(retInstrAddr);
						if (opcode == 0xC2)
						{
							memory.Write8(retInstrAddr + 1, (byte)(simulatedArgBytes & 0xFF));
							memory.Write8(retInstrAddr + 2, (byte)((simulatedArgBytes >> 8) & 0xFF));
							_patchedImportStubs.Add(importStubAddr);
							_logger.LogDebug("[msvcrt] {Context}: Patched RET at 0x{RetAddr:X8} with argBytes={ArgBytes}", 
								logContext, retInstrAddr, simulatedArgBytes);
						}
						else if (opcode == 0xC3)
						{
							// Found RET (0xC3) but need RET imm16 (0xC2) for stdcall cleanup
							// Overwrite with RET imm16 to handle argument cleanup
							memory.Write8(retInstrAddr, 0xC2); // Change to RET imm16
							memory.Write8(retInstrAddr + 1, (byte)(simulatedArgBytes & 0xFF));
							memory.Write8(retInstrAddr + 2, (byte)((simulatedArgBytes >> 8) & 0xFF));
							_patchedImportStubs.Add(importStubAddr);
							_logger.LogDebug("[msvcrt] {Context}: Converted RET to RET imm16 at 0x{RetAddr:X8} with argBytes={ArgBytes}", 
								logContext, retInstrAddr, simulatedArgBytes);
						}
						else
						{
							_logger.LogWarning("[msvcrt] {Context}: Expected RET (0xC3) or RET imm16 (0xC2) at 0x{RetAddr:X8} but found 0x{Opcode:X2}. Skipping patch.", 
								logContext, retInstrAddr, opcode);
						}
					}
					
					// Restore callee-saved registers
					CpuHelpers.RestoreCalleeSavedRegisters(cpu, saved, skipInvalidEbp: true, memorySize: memory.Size);
					
					return true;
				}
			}
			else
			{
				_logger.LogError("[msvcrt] {Context}: Nested syscall at unmapped import stub 0x{Stub:X8}", logContext, importStubAddr);
				shouldBreak = true;
				return true;
			}
		}

		// Check for direct import calls (CALL instructions to imported Win32 API functions)
		// This is less common but still supported
		if (step.IsCall && _image != null && _image.ImportAddressMap.TryGetValue(step.CallTarget, out var imp2))
		{
			var dll = imp2.dll.ToUpperInvariant();
			var name = imp2.name;
			_logger.LogDebug("[msvcrt] {Context}: Nested import call {Dll}!{Name} at 0x{CallTarget:X8}", logContext, dll, name, step.CallTarget);
			stepDesc = $"Import call {dll}!{name}";

			// Use shared import call handler
			var handled = ImportCallHelper.HandleImportCall(
				dll, name, cpu, memory, _dispatcher, _image, _logger,
				"msvcrt:" + logContext, IsValidReturnAddress, out shouldBreak);
			
			return handled;
		}

		return false;
	}

	/// <summary>
	/// Validates that a return address points to valid executable code and not to stack or invalid memory.
	/// Uses the class field _image for validation rather than passing it as a parameter.
	/// </summary>
	/// <param name="address">The return address to validate</param>
	/// <returns>True if the address is valid for execution, false otherwise</returns>
	private bool IsValidReturnAddress(uint address)
	{
		// Reject NULL addresses
		if (address == 0)
		{
			return false;
		}

		// Get actual stack boundaries from process environment
		var stackLimit = _env.StackLimit;
		var stackBase = _env.StackBase;

		// Reject addresses within the stack region
		if (address >= stackLimit && address <= stackBase)
		{
			return false;
		}

		// If we have image info, use IsAddressInCodeSection for proper validation
		if (_image != null)
		{
			var isInCodeSection = _image.IsAddressInCodeSection(address);

			// Also check if it's in imported DLL space
			// Accept any address above the image base if not in code section
			// This handles DLLs that are loaded at different addresses
			if (!isInCodeSection && address >= _image.BaseAddress)
			{
				// Could be in a DLL, allow it
				return true;
			}

			return isInCodeSection;
		}

		// Without image info, use conservative default (typical Win32 image base)
		const uint DEFAULT_MIN_CODE_ADDRESS = 0x00400000;
		return address >= DEFAULT_MIN_CODE_ADDRESS;
	}

	/// <summary>
	/// Execute a callback function in the emulated code.
	/// Similar to User32Module's callback execution but adapted for synchronous context.
	/// This method sets up a call frame, executes the callback, and restores CPU state.
	/// 
	/// Note: The finally block always restores CPU state (EIP, ESP, EBP) to ensure
	/// the caller's state is preserved even if the callback fails or throws an exception.
	/// This is intentional - we return control to the Win32 API function that invoked
	/// the callback, not to the address the callback would have returned to.
	/// </summary>
	/// <param name="funcPtr">Address of the function to call</param>
	/// <param name="logContext">Context for logging (e.g., "_initterm")</param>
	/// <returns>True if execution was successful, false if there was an error</returns>
	private bool ExecuteCallback(uint funcPtr, string logContext)
	{
		if (_cpu == null)
		{
			_logger.LogWarning("[msvcrt] {LogContext}: CPU not available", logContext);
			return false;
		}

		if (funcPtr == 0)
		{
			_logger.LogWarning("[msvcrt] {LogContext}: Function pointer is NULL", logContext);
			return false;
		}

		_logger.LogDebug("[msvcrt] {LogContext}: Executing callback at 0x{FuncPtr:X8}", logContext, funcPtr);

		// Save current CPU state
		var savedEip = _cpu.GetEip();
		var savedEsp = _cpu.GetRegister("ESP");
		var savedEbp = _cpu.GetRegister("EBP");

		try
		{
			// Set up stack for cdecl/stdcall convention
			var esp = savedEsp;

			// Push return address
			esp -= 4;
			_env.Memory.Write32(esp, CALLBACK_RETURN_ADDRESS);

			// Update CPU registers
			_cpu.SetRegister("ESP", esp);
			_cpu.SetEip(funcPtr);

			// Execute callback - keep running until we hit the return address
			var steps = 0;

			while (steps < MAX_CALLBACK_STEPS)
			{
				var eip = _cpu.GetEip();

				// Check if we've returned to our marker address
				if (eip == CALLBACK_RETURN_ADDRESS)
				{
					_logger.LogDebug("[msvcrt] {LogContext}: Callback returned successfully after {Steps} steps", logContext, steps);
					break;
				}

				// Check for invalid EIP (NULL pointer execution)
				if (eip == 0x00000000)
				{
					_logger.LogError("[msvcrt] {LogContext}: Execution jumped to NULL address (0x00000000) after {Steps} steps", logContext, steps);
					return false;
				}

				// Check for other invalid low addresses
				if (eip < MINIMUM_VALID_EIP && eip != CALLBACK_RETURN_ADDRESS)
				{
					_logger.LogError("[msvcrt] {LogContext}: Execution jumped to invalid low address 0x{Eip:X8} after {Steps} steps", logContext, eip, steps);
					return false;
				}

				// Execute one instruction
				var step = _cpu.SingleStep(_env.Memory);
				
				// Handle nested syscalls (import calls) from within callbacks
				// This allows callbacks to call other Win32 API functions
				if (HandleNestedSyscalls(step, _cpu, _env.Memory, logContext, out var stepDesc, out var shouldBreak))
				{
					if (shouldBreak)
					{
						_logger.LogError("[msvcrt] {LogContext}: Nested syscall handler indicated execution should stop", logContext);
						return false;
					}
					// Successfully handled nested syscall, continue to next instruction
				}
				else if (step.IsSyscall)
				{
					// Syscall was not handled (dispatcher or image not available)
					// This maintains backward compatibility with tests that don't set dispatcher
					_logger.LogWarning("[msvcrt] {LogContext}: Callback attempted nested syscall at step {Steps} (EIP=0x{Eip:X8}) but dispatcher not available - aborting callback execution", logContext, steps, _cpu.GetEip());
					return false;
				}
				
				steps++;
			}

			if (steps >= MAX_CALLBACK_STEPS)
			{
				_logger.LogError("[msvcrt] {LogContext}: Callback execution exceeded maximum steps ({MaxSteps}), possible infinite loop at EIP=0x{Eip:X8}", logContext, MAX_CALLBACK_STEPS, _cpu.GetEip());
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[msvcrt] {LogContext}: Exception during callback execution: {Message}", logContext, ex.Message);
			return false;
		}
		finally
		{
			// Always restore CPU state to return control to the API function caller
			// This ensures the Win32 API function's execution context is preserved
			_cpu.SetEip(savedEip);
			_cpu.SetRegister("ESP", savedEsp);
			_cpu.SetRegister("EBP", savedEbp);
		}
	}
}
}
