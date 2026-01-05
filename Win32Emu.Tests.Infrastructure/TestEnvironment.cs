using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.Win32;
using Win32Emu.Win32.Modules;

namespace Win32Emu.Tests.Infrastructure;

/// <summary>
/// Test environment that provides a complete setup for testing Win32 API calls.
/// This is a unified test infrastructure that supports all Win32 module testing.
/// </summary>
public class TestEnvironment : IDisposable
{
	public VirtualMemory Memory { get; }
	public MockCpu Cpu { get; }
	public ProcessEnvironment ProcessEnv { get; }
	public PeImageLoader PeLoader { get; }
	public Win32Dispatcher? Dispatcher { get; private set; }

	// Win32 Modules - lazy loaded as needed
	private Kernel32Module? _kernel32;
	private User32Module? _user32;
	private Gdi32Module? _gdi32;
	private DDrawModule? _ddraw;
	private DSoundModule? _dsound;
	private DInputModule? _dinput;
	private WinMmModule? _winmm;
	private Comctl32Module? _comctl32;

	internal Kernel32Module Kernel32 => _kernel32 ??= CreateKernel32Module();
	internal User32Module User32 => _user32 ??= CreateUser32Module();
	internal Gdi32Module Gdi32 => _gdi32 ??= CreateGdi32Module();
	public DDrawModule DDraw => _ddraw ??= CreateDDrawModule();
	public DSoundModule DSound => _dsound ??= CreateDSoundModule();
	public DInputModule DInput => _dinput ??= CreateDInputModule();
	internal WinMmModule WinMm => _winmm ??= CreateWinMmModule();
	internal Comctl32Module Comctl32 => _comctl32 ??= CreateComctl32Module();

	/// <summary>
	/// Create a test environment with optional custom host
	/// </summary>
	public TestEnvironment(IEmulatorHost? host = null, bool initializeDispatcher = false)
	{
		Memory = new VirtualMemory();
		Cpu = new MockCpu();
		ProcessEnv = new ProcessEnvironment(Memory, host: host, logger: NullLogger.Instance);
		PeLoader = new PeImageLoader(Memory, NullLogger.Instance);

		if (initializeDispatcher)
		{
			InitializeDispatcher();
		}

		// Initialize process environment with test data
		ProcessEnv.InitializeStrings("test.exe", ["test.exe"]);
	}

	/// <summary>
	/// Initialize the Win32 dispatcher and register modules
	/// </summary>
	public void InitializeDispatcher()
	{
		if (Dispatcher != null)
		{
			return;
		}

		Dispatcher = new Win32Dispatcher(NullLogger.Instance);
		ProcessEnv.InitializeMainThread(Cpu);
	}

	private Kernel32Module CreateKernel32Module()
	{
		var module = new Kernel32Module(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);
		if (Dispatcher != null)
		{
			module.SetDispatcher(Dispatcher);
			Dispatcher.RegisterModule(module);
		}
		return module;
	}

	private User32Module CreateUser32Module()
	{
		var module = new User32Module(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);
		if (Dispatcher != null)
		{
			module.SetDispatcher(Dispatcher);
			Dispatcher.RegisterModule(module);
		}
		return module;
	}

	private Gdi32Module CreateGdi32Module()
	{
		var module = new Gdi32Module(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);
		if (Dispatcher != null)
		{
			Dispatcher.RegisterModule(module);
		}
		return module;
	}

	private DDrawModule CreateDDrawModule()
	{
		return new DDrawModule(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);
	}

	private DSoundModule CreateDSoundModule()
	{
		return new DSoundModule(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);
	}

	private DInputModule CreateDInputModule()
	{
		return new DInputModule(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);
	}

	private WinMmModule CreateWinMmModule()
	{
		return new WinMmModule(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);
	}

	private Comctl32Module CreateComctl32Module()
	{
		return new Comctl32Module(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);
	}

	/// <summary>
	/// Call a Kernel32 API function with the given arguments
	/// </summary>
	public uint CallKernel32Api(string functionName, params uint[] args)
	{
		Cpu.SetupStackArgs(Memory, args);
		var success = Kernel32.TryInvokeUnsafe(functionName, Cpu, Memory, out var returnValue);
		if (!success)
		{
			throw new InvalidOperationException($"Failed to invoke {functionName}");
		}
		return returnValue;
	}

	/// <summary>
	/// Call a User32 API function with the given arguments
	/// </summary>
	public uint CallUser32Api(string functionName, params object[] args)
	{
		var uintArgs = ConvertArgsToUInt(args);
		Cpu.SetupStackArgs(Memory, uintArgs);
		var success = User32.TryInvokeUnsafe(functionName, Cpu, Memory, out var returnValue);
		if (!success)
		{
			throw new InvalidOperationException($"Failed to invoke {functionName}");
		}
		return returnValue;
	}

	/// <summary>
	/// Call a GDI32 API function with the given arguments
	/// </summary>
	public uint CallGdi32Api(string functionName, params object[] args)
	{
		var uintArgs = ConvertArgsToUInt(args);
		Cpu.SetupStackArgs(Memory, uintArgs);
		var success = Gdi32.TryInvokeUnsafe(functionName, Cpu, Memory, out var returnValue);
		if (!success)
		{
			throw new InvalidOperationException($"Failed to invoke {functionName}");
		}
		return returnValue;
	}

	/// <summary>
	/// Call a DirectDraw API function with the given arguments
	/// </summary>
	public uint CallDDrawApi(string functionName, params uint[] args)
	{
		Cpu.SetupStackArgs(Memory, args);
		var success = DDraw.TryInvokeUnsafe(functionName, Cpu, Memory, out var returnValue);
		if (!success)
		{
			throw new InvalidOperationException($"Failed to invoke {functionName}");
		}
		return returnValue;
	}

	/// <summary>
	/// Call a DirectSound API function with the given arguments
	/// </summary>
	public uint CallDSoundApi(string functionName, params uint[] args)
	{
		Cpu.SetupStackArgs(Memory, args);
		var success = DSound.TryInvokeUnsafe(functionName, Cpu, Memory, out var returnValue);
		if (!success)
		{
			throw new InvalidOperationException($"Failed to invoke {functionName}");
		}
		return returnValue;
	}

	/// <summary>
	/// Call a DirectInput API function with the given arguments
	/// </summary>
	public uint CallDInputApi(string functionName, params uint[] args)
	{
		Cpu.SetupStackArgs(Memory, args);
		var success = DInput.TryInvokeUnsafe(functionName, Cpu, Memory, out var returnValue);
		if (!success)
		{
			throw new InvalidOperationException($"Failed to invoke {functionName}");
		}
		return returnValue;
	}

	/// <summary>
	/// Call a WinMM API function with the given arguments
	/// </summary>
	public uint CallWinMmApi(string functionName, params uint[] args)
	{
		Cpu.SetupStackArgs(Memory, args);
		var success = WinMm.TryInvokeUnsafe(functionName, Cpu, Memory, out var returnValue);
		if (!success)
		{
			throw new InvalidOperationException($"Failed to invoke {functionName}");
		}
		return returnValue;
	}

	/// <summary>
	/// Call a Comctl32 API function with the given arguments
	/// </summary>
	public uint CallComctl32Api(string functionName, params uint[] args)
	{
		Cpu.SetupStackArgs(Memory, args);
		var success = Comctl32.TryInvokeUnsafe(functionName, Cpu, Memory, out var returnValue);
		if (!success)
		{
			throw new InvalidOperationException($"Failed to invoke {functionName}");
		}
		return returnValue;
	}

	/// <summary>
	/// Write a null-terminated ANSI string to memory and return its address
	/// </summary>
	public uint WriteString(string str)
	{
		var bytes = Encoding.ASCII.GetBytes(str + "\0");
		var addr = ProcessEnv.SimpleAlloc((uint)bytes.Length);
		Memory.WriteBytes(addr, bytes);
		return addr;
	}

	/// <summary>
	/// Create a null-terminated ANSI string in memory (alias for WriteString)
	/// </summary>
	public uint CreateAnsiString(string str) => WriteString(str);

	/// <summary>
	/// Write a null-terminated Unicode string to memory and return its address
	/// </summary>
	public uint WriteStringW(string str)
	{
		var bytes = Encoding.Unicode.GetBytes(str + "\0");
		var addr = ProcessEnv.SimpleAlloc((uint)bytes.Length);
		Memory.WriteBytes(addr, bytes);
		return addr;
	}

	/// <summary>
	/// Read a null-terminated ANSI string from memory
	/// </summary>
	public string ReadString(uint addr)
	{
		if (addr == 0)
		{
			return string.Empty;
		}

		var result = new List<byte>();
		var currentAddr = addr;
		
		while (true)
		{
			var b = Memory.Read8(currentAddr);
			if (b == 0)
			{
				break;
			}

			result.Add(b);
			currentAddr++;
		}

		return Encoding.ASCII.GetString(result.ToArray());
	}

	/// <summary>
	/// Read a null-terminated Unicode string from memory
	/// </summary>
	public string ReadWideString(uint addr)
	{
		if (addr == 0)
		{
			return string.Empty;
		}

		var result = new List<char>();
		var currentAddr = addr;
		
		while (true)
		{
			var wideChar = Memory.Read16(currentAddr);
			if (wideChar == 0)
			{
				break;
			}

			result.Add((char)wideChar);
			currentAddr += 2;
		}

		return new string(result.ToArray());
	}

	/// <summary>
	/// Allocate memory and return its address
	/// </summary>
	public uint AllocateMemory(uint size)
	{
		return ProcessEnv.SimpleAlloc(size);
	}

	/// <summary>
	/// Write a WNDCLASSA structure to memory
	/// </summary>
	public uint WriteWndClassA(
		string? className = null,
		uint style = 0,
		uint wndProc = 0x00401000,
		int cbClsExtra = 0,
		int cbWndExtra = 0,
		uint hInstance = 0,
		uint hIcon = 0,
		uint hCursor = 0,
		uint hbrBackground = 0,
		string? menuName = null)
	{
		var addr = AllocateMemory((uint)System.Runtime.InteropServices.Marshal.SizeOf<NativeTypes.WNDCLASSA>());
		
		Memory.Write32(addr + 0, style);
		Memory.Write32(addr + 4, wndProc);
		Memory.Write32(addr + 8, (uint)cbClsExtra);
		Memory.Write32(addr + 12, (uint)cbWndExtra);
		Memory.Write32(addr + 16, hInstance == 0 ? 0x00400000 : hInstance); // Use default module instance
		Memory.Write32(addr + 20, hIcon);
		Memory.Write32(addr + 24, hCursor);
		Memory.Write32(addr + 28, hbrBackground);
		Memory.Write32(addr + 32, menuName != null ? WriteString(menuName) : 0);
		Memory.Write32(addr + 36, className != null ? WriteString(className) : 0);
		
		return addr;
	}

	/// <summary>
	/// Convert object arguments to uint array, handling int values
	/// </summary>
	private static uint[] ConvertArgsToUInt(object[] args)
	{
		var uintArgs = new uint[args.Length];
		for (int i = 0; i < args.Length; i++)
		{
			uintArgs[i] = args[i] switch
			{
				uint u => u,
				int n => unchecked((uint)n),
				_ => throw new ArgumentException($"Unsupported argument type: {args[i].GetType()}")
			};
		}
		return uintArgs;
	}

	public void Dispose()
	{
		// Nothing to dispose currently, but good practice for future cleanup
	}
}
