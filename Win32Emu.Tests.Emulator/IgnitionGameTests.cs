using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Integration tests for running various Ignition game executables with the emulator.
/// These tests document the behavior and issues encountered when running actual Win32 executables.
/// 
/// Test Overview:
/// - IgnitionDemo_ShouldLoadAndRun: Runs IGN_DEMO.EXE and captures execution details
/// - IgnitionWin_ShouldLoadAndRun: Runs Ign_win.exe and captures execution details
/// - IgnitionWinFix_ShouldLoadAndRun: Runs Ign_win_fix.exe and captures execution details
/// - Ignition3dfx_ShouldLoadAndRun: Runs ign_3dfx.exe and captures execution details
/// 
/// Purpose:
/// These tests help identify what needs to be fixed in the emulator by documenting:
/// 1. Which Win32 APIs are called during execution
/// 2. What errors or issues occur
/// 3. How far each executable progresses before encountering problems
/// 4. Missing or incorrectly implemented functionality
/// </summary>
public class IgnitionGameTests
{
    private readonly ITestOutputHelper _output;

    public IgnitionGameTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void IgnitionDemo_ShouldLoadAndRun()
    {
        var exePath = FindExecutable("IGN_DEMO.EXE");
        RunExecutableTest(exePath, "IGN_DEMO.EXE");
    }

    [Fact]
    public void IgnitionWin_ShouldLoadAndRun()
    {
        var exePath = FindExecutable("Ign_win.exe");
        RunExecutableTest(exePath, "Ign_win.exe");
    }

    [Fact]
    public void IgnitionWinFix_ShouldLoadAndRun()
    {
        var exePath = FindExecutable("Ign_win_fix.exe");
        RunExecutableTest(exePath, "Ign_win_fix.exe");
    }

    [Fact]
    public void Ignition3dfx_ShouldLoadAndRun()
    {
        var exePath = FindExecutable("ign_3dfx.exe");
        RunExecutableTest(exePath, "ign_3dfx.exe");
    }
    
    [Fact]
    public void IgnitionAutorun_ShouldLoadAndRun()
    {
	    var exePath = FindExecutable("AUTORUN.EXE");
	    RunExecutableTest(exePath, "AUTORUN.EXE");
    }
    
    [Fact]
    public void IgnitionSetup_ShouldLoadAndRun()
    {
	    var exePath = FindExecutable("SETUP.EXE");
	    RunExecutableTest(exePath, "SETUP.EXE");
    }
    
    [Fact]
    public void CHKCPU32_ShouldLoadAndRun()
    {
	    var exePath = FindExecutable("CHKCPU32.exe");
	    RunExecutableTest(exePath, "CHKCPU32.exe");
    }

    private void RunExecutableTest(string exePath, string exeName)
    {
        _output.WriteLine($"=== {exeName} Test ===");
        _output.WriteLine($"Testing executable: {exePath}");
        _output.WriteLine($"File exists: {File.Exists(exePath)}");
        _output.WriteLine("");

        if (!File.Exists(exePath))
        {
            throw new FileNotFoundException($"Test executable not found: {exePath}");
        }

        var testHost = new TestEmulatorHost(_output);
        var logger = new XunitLogger(_output, LogLevel.Trace);

        Exception? caughtException = null;
        var startTime = DateTime.UtcNow;
        try
        {
            using var emulator = new Win32Emu.Emulator(testHost, logger);

            _output.WriteLine("Loading executable...");
            emulator.LoadExecutable(exePath, debugMode: false, reservedMemoryMb: 256);

            _output.WriteLine("Starting emulation...");
            _output.WriteLine("");

            // Set a timeout for the test run
            var timeout = TimeSpan.FromSeconds(10);

            var runTask = Task.Run(() => emulator.Run());
            var completedTask = Task.WhenAny(runTask, Task.Delay(timeout)).Result;

            if (completedTask != runTask)
            {
                _output.WriteLine("Test timed out after 10 seconds - stopping emulator");
                emulator.Stop();
                // Give the emulator up to 2 seconds to shut down gracefully
                if (!runTask.Wait(TimeSpan.FromSeconds(5)))
                {
                    _output.WriteLine("Emulator did not stop within 5 seconds after timeout.");
                }
            }
            else
            {
                _output.WriteLine("Emulation completed");
            }
        }
        catch (Exception ex)
        {
            caughtException = ex;
            _output.WriteLine($"\nException caught: {ex.GetType().Name}");
            _output.WriteLine($"Message: {ex.Message}");
            if (ex.StackTrace != null)
            {
                _output.WriteLine($"Stack trace:\n{ex.StackTrace}");
            }
        }

        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;

        // Report summary
        _output.WriteLine("\n=== Test Summary ===");
        _output.WriteLine($"Execution time: {duration.TotalSeconds:F2} seconds");
        _output.WriteLine($"Debug messages captured: {testHost.DebugMessages.Count}");
        _output.WriteLine($"Error messages captured: {testHost.ErrorMessages.Count}");
        _output.WriteLine($"Warning messages captured: {testHost.WarningMessages.Count}");
        _output.WriteLine($"Stdout messages captured: {testHost.StdOutputMessages.Count}");
        _output.WriteLine($"Windows created: {testHost.WindowsCreated.Count}");

        if (testHost.ErrorMessages.Count > 0)
        {
            _output.WriteLine("\n=== Error Messages ===");
            foreach (var error in testHost.ErrorMessages.Take(20))
            {
                _output.WriteLine($"  - {error}");
            }
            if (testHost.ErrorMessages.Count > 20)
            {
                _output.WriteLine($"  ... and {testHost.ErrorMessages.Count - 20} more errors");
            }
        }

        if (testHost.WarningMessages.Count > 0)
        {
            _output.WriteLine("\n=== Warning Messages ===");
            foreach (var warning in testHost.WarningMessages.Take(20))
            {
                _output.WriteLine($"  - {warning}");
            }
            if (testHost.WarningMessages.Count > 20)
            {
                _output.WriteLine($"  ... and {testHost.WarningMessages.Count - 20} more warnings");
            }
        }

        if (testHost.StdOutputMessages.Count > 0)
        {
            _output.WriteLine("\n=== Stdout Messages ===");
            foreach (var stdout in testHost.StdOutputMessages.Take(20))
            {
                _output.WriteLine($"  - {stdout}");
            }
            if (testHost.StdOutputMessages.Count > 20)
            {
                _output.WriteLine($"  ... and {testHost.StdOutputMessages.Count - 20} more stdout messages");
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

        _output.WriteLine("\n=== Test Result ===");
        if (caughtException == null)
        {
            _output.WriteLine($"✓ {exeName} loaded and ran successfully (with timeout)");
            _output.WriteLine("  No exceptions were thrown during execution");
        }
        else
        {
            _output.WriteLine($"✗ {exeName} encountered an exception during execution");
            _output.WriteLine($"  Exception type: {caughtException.GetType().Name}");
        }

        _output.WriteLine("\nNote: This test documents the behavior and issues when running the game.");
        _output.WriteLine("Check the output above for details on what was encountered.");
    }

    private string FindExecutable(string exeName)
    {
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

        // Search in multiple possible locations
        var possiblePaths = new[]
        {
            Path.Combine(repoRoot!, "EXEs", "ign_demo", exeName),
            Path.Combine(repoRoot!, "EXEs", "ign_win", exeName),
            Path.Combine(repoRoot!, "EXEs", "ign_teas", exeName),
            Path.Combine(repoRoot!, "EXEs", "ign_install", exeName),
            Path.Combine(repoRoot!, "EXEs", exeName),
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        // Return the most likely path even if it doesn't exist (will throw FileNotFoundException later)
        return possiblePaths[0];
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
                DebugLevel.Trace => "[TRACE] ",
                DebugLevel.Debug => "[DEBUG] ",
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
        private readonly LogLevel _minLevel;

        public XunitLogger(ITestOutputHelper output, LogLevel minLevel = LogLevel.Information)
        {
            _output = output;
            _minLevel = minLevel;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _minLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

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
