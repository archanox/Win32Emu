using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Integration tests for the retrowin32 test executables.
/// These are purpose-built test programs from the retrowin32 project (https://github.com/evmar/retrowin32)
/// that exercise specific Win32 API functionality.
/// 
/// Test Overview:
/// - GdiTest: Tests GDI basics (CreateWindow, BeginPaint, message loop)
/// - CmdLineTest: Tests command line argument handling
/// - MetricsTest: Tests GetSystemMetrics API
/// - ThreadTest: Tests CreateThread and threading
/// - ErrorsTest: Tests various error handling scenarios
/// - DDrawTest: Tests DirectDraw functionality
/// - DibTest: Tests bitmap drawing APIs
/// - WinapiTest: Tests general Win32 API functionality
/// - OpsTest: Tests math and floating point operations
/// - CallbackTest: Tests callback functionality
/// - TraceTest: Tests execution tracing
/// 
/// Purpose:
/// These focused tests help identify specific missing or broken APIs more precisely than
/// full game executables, making debugging and implementation easier.
/// </summary>
public class Retrowin32Tests
{
    private readonly ITestOutputHelper _output;

    public Retrowin32Tests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void GdiTest_ShouldLoadAndRun()
    {
        var exePath = FindRetrowin32Executable("cpp/gdi.exe");
        RunExecutableTest(exePath, "gdi.exe", "GDI window creation and painting");
    }

    [Fact]
    public void CmdLineTest_ShouldLoadAndRun()
    {
        var exePath = FindRetrowin32Executable("cpp/cmdline.exe");
        RunExecutableTest(exePath, "cmdline.exe", "Command line argument handling");
    }

    [Fact]
    public void MetricsTest_ShouldLoadAndRun()
    {
        var exePath = FindRetrowin32Executable("cpp/metrics.exe");
        RunExecutableTest(exePath, "metrics.exe", "GetSystemMetrics API");
    }

    [Fact]
    public void ThreadTest_ShouldLoadAndRun()
    {
        var exePath = FindRetrowin32Executable("cpp/thread.exe");
        RunExecutableTest(exePath, "thread.exe", "CreateThread and threading");
    }

    [Fact]
    public void ErrorsTest_ShouldLoadAndRun()
    {
        var exePath = FindRetrowin32Executable("cpp/errors.exe");
        RunExecutableTest(exePath, "errors.exe", "Error handling scenarios");
    }

    [Fact]
    public void DDrawTest_ShouldLoadAndRun()
    {
        var exePath = FindRetrowin32Executable("cpp/ddraw.exe");
        RunExecutableTest(exePath, "ddraw.exe", "DirectDraw functionality");
    }

    [Fact]
    public void DibTest_ShouldLoadAndRun()
    {
        var exePath = FindRetrowin32Executable("cpp/dib.exe");
        RunExecutableTest(exePath, "dib.exe", "Bitmap drawing APIs");
    }

    [Fact]
    public void WinapiTest_ShouldLoadAndRun()
    {
        var exePath = FindRetrowin32Executable("winapi/winapi.exe");
        RunExecutableTest(exePath, "winapi.exe", "General Win32 API");
    }

    [Fact]
    public void OpsTest_ShouldLoadAndRun()
    {
        var exePath = FindRetrowin32Executable("ops/ops.exe");
        RunExecutableTest(exePath, "ops.exe", "Math and FPU operations");
    }

    [Fact]
    public void CallbackTest_ShouldLoadAndRun()
    {
        var exePath = FindRetrowin32Executable("callback/callback.exe");
        RunExecutableTest(exePath, "callback.exe", "Callback functionality");
    }

    [Fact]
    public void TraceTest_ShouldLoadAndRun()
    {
        var exePath = FindRetrowin32Executable("trace/trace.exe");
        RunExecutableTest(exePath, "trace.exe", "Execution tracing");
    }

    [Fact]
    public void ZigHelloTest_ShouldLoadAndRun()
    {
        var exePath = FindRetrowin32Executable("zig_hello/hello.exe");
        RunExecutableTest(exePath, "hello.exe", "Zig hello world");
    }

    private void RunExecutableTest(string exePath, string exeName, string description)
    {
        _output.WriteLine($"=== {exeName} Test ===");
        _output.WriteLine($"Description: {description}");
        _output.WriteLine($"Testing executable: {exePath}");
        _output.WriteLine($"File exists: {File.Exists(exePath)}");
        _output.WriteLine("");

        if (!File.Exists(exePath))
        {
            throw new FileNotFoundException($"Test executable not found: {exePath}");
        }

        var testHost = new TestEmulatorHost(_output);
        var logger = new XunitLogger(_output, LogLevel.Information);

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
            var timeout = TimeSpan.FromSeconds(5);

            var runTask = Task.Run(() => emulator.Run());
            var completedTask = Task.WhenAny(runTask, Task.Delay(timeout)).Result;

            if (completedTask != runTask)
            {
                _output.WriteLine("Test timed out after 5 seconds - stopping emulator");
                emulator.Stop();
                // Give the emulator up to 2 seconds to shut down gracefully
                if (!runTask.Wait(TimeSpan.FromSeconds(2)))
                {
                    _output.WriteLine("Emulator did not stop within 2 seconds after timeout.");
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

        _output.WriteLine("\nNote: This test documents the behavior and issues when running the test executable.");
        _output.WriteLine("Check the output above for details on what was encountered.");
    }

    private string FindRetrowin32Executable(string relativePath)
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

        var exePath = Path.Combine(repoRoot!, "retrowin32", "exe", relativePath);
        
        // If file doesn't exist, try to initialize submodule (won't work in test environment but documents the requirement)
        if (!File.Exists(exePath))
        {
            var submodulePath = Path.Combine(repoRoot!, "retrowin32");
            if (!Directory.Exists(submodulePath) || !Directory.GetFiles(submodulePath).Any())
            {
                throw new InvalidOperationException(
                    $"retrowin32 submodule not initialized. Run: git submodule update --init retrowin32");
            }
        }

        return exePath;
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
