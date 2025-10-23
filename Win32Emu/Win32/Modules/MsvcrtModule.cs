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

		public MsvcrtModule(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
		{
			_env = env;
			_imageBase = imageBase;
			_peLoader = peLoader;
			_logger = logger ?? NullLogger.Instance;
		}

		public string Name => "msvcrt.dll";

		public unsafe bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			returnValue = 0;
			var a = new StackArgs(cpu, memory);

			switch (export.ToUpperInvariant())
			{
				case "__P___INITENV":
					returnValue = __p___initenv();
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
				case "_AMSG_EXIT":
					_amsg_exit(a.Int32(0));
					returnValue = 0;
					return true;
				case "_CEXIT":
					_cexit();
					returnValue = 0;
					return true;
				case "_INITTERM":
					_initterm(a.UInt32(0), a.UInt32(1));
					returnValue = 0;
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
	}
}
