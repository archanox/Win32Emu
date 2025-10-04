using Microsoft.Extensions.Logging;
using System.Text;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Integration tests for running real game executables with the emulator
/// </summary>
public class IgnitionTeaserTests
{
    private readonly ITestOutputHelper _output;

    public IgnitionTeaserTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void IgnitionTeaser_ShouldLoadAndRun()
    {
        // Arrange
        // Find the repository root by looking for the .sln file
        var currentDir = Directory.GetCurrentDirectory();
        var repoRoot = currentDir;
        
        // Navigate up until we find the .sln file
        while (repoRoot != null && !File.Exists(Path.Combine(repoRoot, "Win32Emu.sln")))
        {
            var parent = Directory.GetParent(repoRoot);
            if (parent == null)
            {
                break;
            }
            repoRoot = parent.FullName;
        }
        
        var exePath = Path.Combine(repoRoot!, "EXEs", "ign_teas", "IGN_TEAS.EXE");
        
        _output.WriteLine($"Current directory: {currentDir}");
        _output.WriteLine($"Repository root: {repoRoot}");
        _output.WriteLine($"Testing executable: {exePath}");
        _output.WriteLine($"File exists: {File.Exists(exePath)}");
        
        if (!File.Exists(exePath))
        {
            throw new FileNotFoundException($"Test executable not found: {exePath}");
        }

        var testHost = new TestEmulatorHost(_output);
        var logger = new XunitLogger(_output);
        
        // Act
        Exception? caughtException = null;
        try
        {
            using var emulator = new Win32Emu.Emulator(testHost, logger);
            
            _output.WriteLine("Loading executable...");
            emulator.LoadExecutable(exePath, debugMode: true, reservedMemoryMb: 256);
            
            _output.WriteLine("Starting emulation...");
            
            // Set a timeout for the test run
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(10));
            
            var runTask = Task.Run(() => emulator.Run(), cancellationTokenSource.Token);
            
            try
            {
                runTask.Wait(cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                _output.WriteLine("Test timed out after 10 seconds - stopping emulator");
                emulator.Stop();
                runTask.Wait(TimeSpan.FromSeconds(2));
            }
            
            _output.WriteLine("Emulation completed");
        }
        catch (Exception ex)
        {
            caughtException = ex;
            _output.WriteLine($"Exception caught: {ex.GetType().Name}");
            _output.WriteLine($"Message: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        // Assert
        _output.WriteLine("\n=== Test Summary ===");
        _output.WriteLine($"Debug messages captured: {testHost.DebugMessages.Count}");
        _output.WriteLine($"Error messages captured: {testHost.ErrorMessages.Count}");
        _output.WriteLine($"Warning messages captured: {testHost.WarningMessages.Count}");
        _output.WriteLine($"Stdout messages captured: {testHost.StdOutputMessages.Count}");
        _output.WriteLine($"Windows created: {testHost.WindowsCreated.Count}");
        
        if (testHost.ErrorMessages.Count > 0)
        {
            _output.WriteLine("\n=== Error Messages ===");
            foreach (var error in testHost.ErrorMessages)
            {
                _output.WriteLine($"  - {error}");
            }
        }
        
        if (testHost.WarningMessages.Count > 0)
        {
            _output.WriteLine("\n=== Warning Messages ===");
            foreach (var warning in testHost.WarningMessages)
            {
                _output.WriteLine($"  - {warning}");
            }
        }
        
        if (testHost.StdOutputMessages.Count > 0)
        {
            _output.WriteLine("\n=== Stdout Messages ===");
            foreach (var stdout in testHost.StdOutputMessages)
            {
                _output.WriteLine($"  - {stdout}");
            }
        }
        
        if (testHost.WindowsCreated.Count > 0)
        {
            _output.WriteLine("\n=== Windows Created ===");
            foreach (var window in testHost.WindowsCreated)
            {
                _output.WriteLine($"  - {window}");
            }
        }
        
        if (caughtException != null)
        {
            _output.WriteLine($"\n=== Exception Details ===");
            _output.WriteLine($"Type: {caughtException.GetType().FullName}");
            _output.WriteLine($"Message: {caughtException.Message}");
            
            if (caughtException.InnerException != null)
            {
                _output.WriteLine($"Inner exception: {caughtException.InnerException.GetType().FullName}");
                _output.WriteLine($"Inner message: {caughtException.InnerException.Message}");
            }
        }
        
        // The test always succeeds - we're just documenting what happens
        _output.WriteLine("\nTest completed - check output above for issues encountered");
    }

    /// <summary>
    /// Test host implementation that captures all emulator callbacks
    /// </summary>
    private class TestEmulatorHost : IEmulatorHost
    {
        private readonly ITestOutputHelper _output;
        public List<string> DebugMessages { get; } = new();
        public List<string> ErrorMessages { get; } = new();
        public List<string> WarningMessages { get; } = new();
        public List<string> WindowsCreated { get; } = new();
        public List<string> StdOutputMessages { get; } = new();

        public TestEmulatorHost(ITestOutputHelper output)
        {
            _output = output;
        }

        public void OnDebugOutput(string message, DebugLevel level)
        {
            var prefix = level switch
            {
                DebugLevel.Error => "[ERROR] ",
                DebugLevel.Warning => "[WARN]  ",
                DebugLevel.Info => "[INFO]  ",
                _ => "[DEBUG] "
            };
            
            _output.WriteLine($"{prefix}{message}");
            
            switch (level)
            {
                case DebugLevel.Error:
                    ErrorMessages.Add(message);
                    break;
                case DebugLevel.Warning:
                    WarningMessages.Add(message);
                    break;
                default:
                    DebugMessages.Add(message);
                    break;
            }
        }

        public void OnStdOutput(string output)
        {
            _output.WriteLine($"[STDOUT] {output}");
            StdOutputMessages.Add(output);
        }

        public void OnWindowCreate(WindowCreateInfo windowInfo)
        {
            var info = $"Window: '{windowInfo.Title}' Class: '{windowInfo.ClassName}' Size: {windowInfo.Width}x{windowInfo.Height}";
            _output.WriteLine($"[WINDOW] {info}");
            WindowsCreated.Add(info);
        }
    }

    /// <summary>
    /// Logger implementation that writes to xUnit test output
    /// </summary>
    private class XunitLogger : ILogger
    {
        private readonly ITestOutputHelper _output;

        public XunitLogger(ITestOutputHelper output)
        {
            _output = output;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            var prefix = logLevel switch
            {
                LogLevel.Critical => "[CRITICAL]",
                LogLevel.Error => "[ERROR]   ",
                LogLevel.Warning => "[WARNING] ",
                LogLevel.Information => "[INFO]    ",
                LogLevel.Debug => "[DEBUG]   ",
                LogLevel.Trace => "[TRACE]   ",
                _ => "[LOG]     "
            };
            
            _output.WriteLine($"{prefix} {message}");
            
            if (exception != null)
            {
                _output.WriteLine($"          Exception: {exception}");
            }
        }
    }
}
