using Win32Emu.Memory;
using Win32Emu.Win32;
using Win32Emu.Logging;

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

    public TestEnvironment()
    {
        Memory = new VirtualMemory();
        Cpu = new MockCpu();
        ProcessEnv = new ProcessEnvironment(Memory);
        var mockLogger = new MockLogger();
        Kernel32 = new Kernel32Module(ProcessEnv, 0x00400000, mockLogger);

        // Initialize process environment with test data
        ProcessEnv.InitializeStrings("test.exe", ["test.exe"]);
    }

    /// <summary>
    /// Call a Kernel32 API function with the given arguments
    /// </summary>
    public uint CallKernel32Api(string functionName, params uint[] args)
    {
        // Set up stack arguments
        Cpu.SetupStackArgs(Memory, args);

        // Call the API
        var success = Kernel32.TryInvokeUnsafe(functionName, Cpu, Memory, out uint returnValue);
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
        var bytes = System.Text.Encoding.ASCII.GetBytes(str + "\0");
        var addr = ProcessEnv.SimpleAlloc((uint)bytes.Length);
        Memory.WriteBytes(addr, bytes);
        return addr;
    }

    /// <summary>
    /// Read a null-terminated string from memory
    /// </summary>
    public string ReadString(uint addr)
    {
        if (addr == 0) return string.Empty;

        var result = new List<byte>();
        uint currentAddr = addr;
        
        while (true)
        {
            var b = Memory.Read8(currentAddr);
            if (b == 0) break;
            result.Add(b);
            currentAddr++;
        }

        return System.Text.Encoding.ASCII.GetString(result.ToArray());
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

/// <summary>
/// Simple mock logger for testing that doesn't output anything
/// </summary>
public class MockLogger : IScopedLogger
{
    public string ComponentName => "MockLogger";

    public IDisposable BeginScope(string scopeName) => new MockScope();
    
    public IDisposable BeginScope<T>(T state) => new MockScope();

    public void LogDebug(string message, params object[] args) { }
    
    public void LogError(Exception ex, string message, params object[] args) { }
    
    public void LogError(string message, params object[] args) { }
    
    public void LogInformation(string message, params object[] args) { }
    
    public void LogWarning(string message, params object[] args) { }

    private class MockScope : IDisposable
    {
        public void Dispose() { }
    }
}