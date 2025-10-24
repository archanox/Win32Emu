using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.Win32;
using Win32Emu.Win32.Modules;

namespace Win32Emu.Tests.Kernel32.TestInfrastructure;

/// <summary>
/// Test environment that provides a complete setup for testing Win32 API calls
/// </summary>
public class TestEnvironment : IDisposable
{
    public VirtualMemory Memory { get; }
    public MockCpu Cpu { get; }
    public ProcessEnvironment ProcessEnv { get; }
    public Kernel32Module Kernel32 { get; }
    public User32Module User32 { get; }
    public Gdi32Module Gdi32 { get; }
    public PeImageLoader PeLoader { get; }
    public Win32Dispatcher Dispatcher { get; }

    public TestEnvironment()
    {
        Memory = new VirtualMemory();
        Cpu = new MockCpu();
        ProcessEnv = new ProcessEnvironment(Memory, logger: NullLogger.Instance);
        PeLoader = new PeImageLoader(Memory, NullLogger.Instance);
        
        // Create dispatcher and register modules
        Dispatcher = new Win32Dispatcher(NullLogger.Instance);
        
        // Create and register Kernel32 module
        Kernel32 = new Kernel32Module(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);
        Kernel32.SetDispatcher(Dispatcher);
        Dispatcher.RegisterModule(Kernel32);

        // Create and register User32 module
        User32 = new User32Module(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);
        User32.SetDispatcher(Dispatcher);
        Dispatcher.RegisterModule(User32);

        // Create and register Gdi32 module
        Gdi32 = new Gdi32Module(ProcessEnv, 0x00400000, PeLoader, NullLogger.Instance);
        Dispatcher.RegisterModule(Gdi32);

        // Initialize process environment with test data
        ProcessEnv.InitializeStrings("test.exe", []);
    }

    /// <summary>
    /// Call a Kernel32 API function with the given arguments
    /// </summary>
    public uint CallKernel32Api(string functionName, params uint[] args)
    {
        // Set up stack arguments
        Cpu.SetupStackArgs(Memory, args);

        // Call the API
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
        // Convert args to uint array, handling int values
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

        // Set up stack arguments
        Cpu.SetupStackArgs(Memory, uintArgs);

        // Call the API
        var success = User32.TryInvokeUnsafe(functionName, Cpu, Memory, out var returnValue);
        if (!success)
        {
            throw new InvalidOperationException($"Failed to invoke {functionName}");
        }

        return returnValue;
    }

    /// <summary>
    /// Call a Gdi32 API function with the given arguments
    /// </summary>
    public uint CallGdi32Api(string functionName, params object[] args)
    {
        // Convert args to uint array, handling int values
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

        // Set up stack arguments
        Cpu.SetupStackArgs(Memory, uintArgs);

        // Call the API
        var success = Gdi32.TryInvokeUnsafe(functionName, Cpu, Memory, out var returnValue);
        if (!success)
        {
            throw new InvalidOperationException($"Failed to invoke {functionName}");
        }

        return returnValue;
    }

    /// <summary>
    /// Create a null-terminated ANSI string in memory and return its address
    /// </summary>
    public uint CreateAnsiString(string str)
    {
        return WriteString(str);
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

    public void Dispose()
    {
        // Nothing to dispose currently, but good practice for future cleanup
    }
}