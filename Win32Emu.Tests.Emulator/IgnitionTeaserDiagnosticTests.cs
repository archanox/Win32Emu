using Microsoft.Extensions.Logging;
using Win32Emu.Cpu;
using Win32Emu.Memory;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Diagnostic tests to understand why IGN_TEAS execution stalls
/// </summary>
public class IgnitionTeaserDiagnosticTests
{
    private readonly ITestOutputHelper _output;

    public IgnitionTeaserDiagnosticTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Diagnostic_LogExecutionAfterLastApiCall()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var repoRoot = currentDir;
        
        while (repoRoot != null && !File.Exists(Path.Combine(repoRoot, "Win32Emu.sln")))
        {
            var parent = Directory.GetParent(repoRoot);
            if (parent == null) break;
            repoRoot = parent.FullName;
        }
        
        var exePath = Path.Combine(repoRoot!, "EXEs", "ign_teas", "IGN_TEAS.EXE");
        
        if (!File.Exists(exePath))
        {
            throw new FileNotFoundException($"Test executable not found: {exePath}");
        }

        var testHost = new DiagnosticEmulatorHost(_output);
        var logger = new XunitLogger(_output, LogLevel.Information);
        
        using var emulator = new Win32Emu.Emulator(testHost, logger);
        
        _output.WriteLine("Loading executable for diagnostic run...");
        emulator.LoadExecutable(exePath, debugMode: false, reservedMemoryMb: 256);
        
        _output.WriteLine("Starting emulation with custom diagnostic logging...");
        
        var timeout = TimeSpan.FromSeconds(2);
        var runTask = Task.Run(() => emulator.Run());
        var completedTask = Task.WhenAny(runTask, Task.Delay(timeout)).Result;
        
        if (completedTask != runTask)
        {
            _output.WriteLine("\nTest timed out - stopping emulator");
            emulator.Stop();
            runTask.Wait(TimeSpan.FromSeconds(2));
        }
        
        _output.WriteLine($"\nDiagnostic Summary:");
        _output.WriteLine($"API calls intercepted: {testHost.ApiCallCount}");
        _output.WriteLine($"Instructions executed after last API: {testHost.InstructionsSinceLastApi}");
        _output.WriteLine($"Unique EIP values seen: {testHost.UniqueEips.Count}");
        
        if (testHost.UniqueEips.Count > 0 && testHost.UniqueEips.Count < 20)
        {
            _output.WriteLine($"\nEIP values in execution loop:");
            foreach (var eip in testHost.UniqueEips.OrderBy(x => x))
            {
                _output.WriteLine($"  0x{eip:X8}");
            }
        }
        
        _output.WriteLine($"\nLast 20 EIP values:");
        foreach (var eip in testHost.LastEips.TakeLast(20))
        {
            _output.WriteLine($"  0x{eip:X8}");
        }
    }

    private class DiagnosticEmulatorHost : IEmulatorHost
    {
        private readonly ITestOutputHelper _output;
        public int ApiCallCount = 0;
        public int InstructionsSinceLastApi = 0;
        public HashSet<uint> UniqueEips = new();
        public List<uint> LastEips = new();
        private const int MaxEipsToKeep = 1000;

        public DiagnosticEmulatorHost(ITestOutputHelper output)
        {
            _output = output;
        }

        public void OnDebugOutput(string message, DebugLevel level)
        {
            // Track API calls
            if (message.Contains("[Import]") || message.Contains("Dispatching"))
            {
                ApiCallCount++;
                InstructionsSinceLastApi = 0;
                UniqueEips.Clear();
                LastEips.Clear();
            }
            
            // Only log errors and warnings to keep output manageable
            if (level == DebugLevel.Error || level == DebugLevel.Warning)
            {
                _output.WriteLine($"[{level}] {message}");
            }
        }

        public void OnStdOutput(string output)
        {
            _output.WriteLine($"[STDOUT] {output}");
        }

        public void OnWindowCreate(WindowCreateInfo windowInfo)
        {
            _output.WriteLine($"[WINDOW] {windowInfo.Title}");
        }
        
        public void TrackInstruction(uint eip)
        {
            InstructionsSinceLastApi++;
            
            if (InstructionsSinceLastApi <= MaxEipsToKeep)
            {
                UniqueEips.Add(eip);
                LastEips.Add(eip);
            }
        }
    }

    private class XunitLogger : ILogger
    {
        private readonly ITestOutputHelper _output;
        private readonly LogLevel _minLevel;

        public XunitLogger(ITestOutputHelper output, LogLevel minLevel = LogLevel.Information)
        {
            _output = output;
            _minLevel = minLevel;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            
            var message = formatter(state, exception);
            _output.WriteLine($"[{logLevel}] {message}");
        }
    }
}
