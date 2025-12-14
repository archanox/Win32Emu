using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.Win32;
using Win32Emu.Win32.Modules;

namespace Win32Emu.Tests.User32.TestInfrastructure;

/// <summary>
/// Test environment that provides a complete setup for testing Win32 API calls
/// </summary>
public class TestEnvironment : IDisposable
{
    public VirtualMemory Memory { get; }
    public MockCpu Cpu { get; }
    public ProcessEnvironment ProcessEnv { get; }
    internal User32Module User32 { get; }
    internal Gdi32Module Gdi32 { get; }
    public DDrawModule DDraw { get; }
    public DSoundModule DSound { get; }
    public DInputModule DInput { get; }
    internal WinMmModule WinMm { get; }
    public PeImageLoader PeLoader { get; }

    public TestEnvironment()
    {
        Memory = new VirtualMemory();
        Cpu = new MockCpu();
        ProcessEnv = new ProcessEnvironment(Memory, logger: NullLogger.Instance);
        PeLoader = new PeImageLoader(Memory, NullLogger.Instance);
        User32 = new User32Module(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);
        Gdi32 = new Gdi32Module(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);
        DDraw = new DDrawModule(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);
        DSound = new DSoundModule(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);
        DInput = new DInputModule(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);
        WinMm = new WinMmModule(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);

        // Initialize process environment with test data
        ProcessEnv.InitializeStrings("test.exe", ["test.exe"]);
    }

    public TestEnvironment(IEmulatorHost host)
    {
        Memory = new VirtualMemory();
        Cpu = new MockCpu();
        ProcessEnv = new ProcessEnvironment(Memory, host: host, logger: NullLogger.Instance);
        PeLoader = new PeImageLoader(Memory, NullLogger.Instance);
        User32 = new User32Module(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);
        Gdi32 = new Gdi32Module(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);
        DDraw = new DDrawModule(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);
        DSound = new DSoundModule(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);
        DInput = new DInputModule(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);
        WinMm = new WinMmModule(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);

        // Initialize process environment with test data
        ProcessEnv.InitializeStrings("test.exe", ["test.exe"]);
    }

    /// <summary>
    /// Call a User32 API function with the given arguments
    /// </summary>
    public uint CallUser32Api(string functionName, params uint[] args)
    {
        // Set up stack arguments
        Cpu.SetupStackArgs(Memory, args);

        // Call the API
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
    public uint CallGdi32Api(string functionName, params uint[] args)
    {
        // Set up stack arguments
        Cpu.SetupStackArgs(Memory, args);

        // Call the API
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
    /// Write a null-terminated string to memory and return its address
    /// </summary>
    public uint WriteString(string str)
    {
        var bytes = Encoding.ASCII.GetBytes(str + "\0");
        var addr = ProcessEnv.SimpleAlloc((uint)bytes.Length);
        Memory.WriteBytes(addr, bytes);
        return addr;
    }

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
    /// Read a null-terminated string from memory
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
        var addr = AllocateMemory(40); // Size of WNDCLASSA
        
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
    /// Call a Kernel32 API function with the given arguments
    /// </summary>
    public uint CallKernel32Api(string functionName, params uint[] args)
    {
        // Setup mock CPU with args
        Cpu.SetupStackArgs(Memory, args);
        
        // For test purposes, we'll create a temporary Kernel32 module
        var kernel32 = new Win32.Modules.Kernel32Module(ProcessEnv, 0x00400000, PeLoader, Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);
        
        var success = kernel32.TryInvokeUnsafe(functionName, Cpu, Memory, out var returnValue);
        if (!success)
        {
            throw new InvalidOperationException($"Failed to invoke {functionName}");
        }
        
        return returnValue;
    }

    public void Dispose()
    {
        // Nothing to dispose currently, but good practice for future cleanup
    }
}
