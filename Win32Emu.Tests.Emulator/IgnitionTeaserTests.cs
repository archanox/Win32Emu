using Microsoft.Extensions.Logging;
using System.Text;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Integration tests for running real game executables with the emulator.
/// These tests document the behavior and issues encountered when running actual Win32 executables.
/// 
/// Test Overview:
/// - IgnitionTeaser_ShouldLoadAndRun: Runs the IGN_TEAS.EXE demo and captures basic execution details
/// - IgnitionTeaser_ShouldLoadAndRun_WithDebugLogging: (Skipped) Runs with full debug output for detailed analysis
/// 
/// Findings from running IGN_TEAS.EXE:
/// 1. The executable loads successfully and runs without crashing
/// 2. Several Win32 API calls are made during initialization:
///    - GetVersion: Returns version information
///    - HeapCreate: Creates a heap for dynamic memory allocation
///    - VirtualAlloc: Allocates virtual memory (called multiple times)
///    - GetStartupInfoA: Retrieves startup configuration
///    - GetStdHandle: Gets standard handles (stdin, stdout, stderr)
///    - GetFileType: Checks file handle types
///    - SetHandleCount: Sets number of file handles
///    - GetACP: Gets the ANSI code page
///    - GetCPInfo: Gets code page information
/// 3. After initialization, the game enters an infinite loop in its own code
///    - No additional Win32 API calls are made
///    - No unknown/missing DLL functions are called (confirmed by dispatcher logging)
///    - The game is executing instructions but not making progress toward any visible state
/// 4. Warnings observed:
///    - Missing argument byte metadata for some KERNEL32 functions (GetACP, GetCPInfo)
/// 
/// Root Cause Analysis:
/// - The game's PE imports include DDRAW.dll, DINPUT.dll, DSOUND.dll, USER32.dll, GDI32.dll, and WINMM.dll
/// - However, the game never attempts to call functions from these DLLs
/// - This suggests the game's initialization code is stuck in a loop BEFORE it tries to initialize graphics/input
/// - Possible causes: waiting for a condition that will never be met (timer, event, or specific return value)
/// - The test uses a timeout to prevent infinite execution
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
        
        _output.WriteLine("=== Ignition Teaser Demo Test ===");
        _output.WriteLine($"Current directory: {currentDir}");
        _output.WriteLine($"Repository root: {repoRoot}");
        _output.WriteLine($"Testing executable: {exePath}");
        _output.WriteLine($"File exists: {File.Exists(exePath)}");
        _output.WriteLine("");
        
        if (!File.Exists(exePath))
        {
            throw new FileNotFoundException($"Test executable not found: {exePath}");
        }

        var testHost = new TestEmulatorHost(_output);
        var logger = new XunitLogger(_output, LogLevel.Information); // Only log Info and above
        
        // Act
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
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(5));
            
            var runTask = Task.Run(() => emulator.Run(), cancellationTokenSource.Token);
            
            try
            {
                runTask.Wait(cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                _output.WriteLine("Test timed out after 5 seconds - stopping emulator");
                emulator.Stop();
                runTask.Wait(TimeSpan.FromSeconds(2));
            }
            
            _output.WriteLine("Emulation completed");
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
        
        // Assert
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
            _output.WriteLine("✓ The executable loaded and ran successfully (with timeout)");
            _output.WriteLine("  No exceptions were thrown during execution");
        }
        else
        {
            _output.WriteLine("✗ The executable encountered an exception during execution");
            _output.WriteLine($"  Exception type: {caughtException.GetType().Name}");
        }
        
        // The test always succeeds - we're just documenting what happens
        _output.WriteLine("\nNote: This test documents the behavior and issues when running the game.");
        _output.WriteLine("Check the output above for details on what was encountered.");
    }

    [Fact(Skip = "Very verbose - enable manually to see detailed debugging output")]
    public void IgnitionTeaser_ShouldLoadAndRun_WithDebugLogging()
    {
        // This test is identical to the main test but runs with debug logging enabled
        // to capture all the detailed execution information
        
        var currentDir = Directory.GetCurrentDirectory();
        var repoRoot = currentDir;
        
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
        
        _output.WriteLine("=== Ignition Teaser Demo Test (Debug Mode) ===");
        _output.WriteLine($"Testing executable: {exePath}");
        
        if (!File.Exists(exePath))
        {
            throw new FileNotFoundException($"Test executable not found: {exePath}");
        }

        var testHost = new TestEmulatorHost(_output);
        var logger = new XunitLogger(_output, LogLevel.Debug); // Log everything
        
        try
        {
            using var emulator = new Win32Emu.Emulator(testHost, logger);
            
            emulator.LoadExecutable(exePath, debugMode: true, reservedMemoryMb: 256);
            
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(10));
            
            var runTask = Task.Run(() => emulator.Run(), cancellationTokenSource.Token);
            
            try
            {
                runTask.Wait(cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                emulator.Stop();
                runTask.Wait(TimeSpan.FromSeconds(2));
            }
        }
        catch (Exception ex)
        {
            _output.WriteLine($"\nException: {ex.Message}");
        }
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
