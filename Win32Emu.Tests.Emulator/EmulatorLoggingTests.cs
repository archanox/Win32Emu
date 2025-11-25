using Microsoft.Extensions.Logging;
using System.Text;
using Win32Emu.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests to verify that system information is logged correctly during emulator initialization
/// </summary>
public class EmulatorLoggingTests
{
    private readonly ITestOutputHelper _output;

    public EmulatorLoggingTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Helper method to find the repository root by looking for Win32Emu.slnx
    /// </summary>
    private static string? FindRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var repoRoot = currentDir;
        
        // Navigate up until we find the .slnx file
        while (repoRoot != null && !File.Exists(Path.Combine(repoRoot, "Win32Emu.slnx")))
        {
            var parent = Directory.GetParent(repoRoot);
            if (parent == null)
            {
                break;
            }
            repoRoot = parent.FullName;
        }
        
        return repoRoot;
    }

    [Fact]
    public void LoadExecutable_ShouldLogSystemInformation()
    {
        // Arrange
        var repoRoot = FindRepositoryRoot();
        
        var exePath = Path.Combine(repoRoot!, "EXEs", "ign_teas", "IGN_TEAS.EXE");
        
        if (!File.Exists(exePath))
        {
            _output.WriteLine($"Test executable not found at: {exePath}");
            return; // Skip test if executable is not available
        }

        var logMessages = new List<string>();
        
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new TestLoggerProvider(logMessages));
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
        var logger = loggerFactory.CreateLogger<Win32Emu.Emulator>();

        // Act
        using var emulator = new Win32Emu.Emulator(logger: logger);
        emulator.LoadExecutable(exePath, debugMode: false, reservedMemoryMb: 256);

        // Assert
        _output.WriteLine("=== Captured Log Messages ===");
        foreach (var message in logMessages)
        {
            _output.WriteLine(message);
        }

        // Verify that system information was logged
        Assert.Contains(logMessages, msg => msg.Contains("[Loader] Host OS:"));
        Assert.Contains(logMessages, msg => msg.Contains("[Loader] Host OS Architecture:") || msg.Contains("[Loader] Host Architecture:"));
        Assert.Contains(logMessages, msg => msg.Contains("[Loader] Selected CPU Emulator:"));
        
        // Verify default CPU emulator is logged (IcedCpu is the default)
        Assert.Contains(logMessages, msg => msg.Contains("[Loader] Selected CPU Emulator: IcedCpu"));
    }

    [Fact]
    public void LoadExecutable_WithJitCpu_ShouldLogJitCpuBackend()
    {
        // Arrange
        var repoRoot = FindRepositoryRoot();
        
        var exePath = Path.Combine(repoRoot!, "EXEs", "ign_teas", "IGN_TEAS.EXE");
        
        if (!File.Exists(exePath))
        {
            _output.WriteLine($"Test executable not found at: {exePath}");
            return; // Skip test if executable is not available
        }

        var logMessages = new List<string>();
        
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new TestLoggerProvider(logMessages));
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
        var logger = loggerFactory.CreateLogger<Win32Emu.Emulator>();

        // Act
        using var emulator = new Win32Emu.Emulator(logger: logger);
        emulator.LoadExecutable(exePath, debugMode: false, reservedMemoryMb: 256, useJitCpu: true);

        // Assert
        _output.WriteLine("=== Captured Log Messages ===");
        foreach (var message in logMessages)
        {
            _output.WriteLine(message);
        }

        // Verify that JitCpu backend is logged
        Assert.Contains(logMessages, msg => msg.Contains("[Loader] Selected CPU Emulator: JitCpu"));
    }

    [Fact]
    public void FileLoggingHelper_GenerateLogFilePath_ShouldIncludeMd5Hash()
    {
        // Arrange
        var repoRoot = FindRepositoryRoot();
        var exePath = Path.Combine(repoRoot!, "EXEs", "ign_teas", "IGN_TEAS.EXE");
        
        if (!File.Exists(exePath))
        {
            _output.WriteLine($"Test executable not found at: {exePath}");
            return; // Skip test if executable is not available
        }

        // Act
        var logFilePath = FileLoggingHelper.GenerateLogFilePath(exePath);

        // Assert
        _output.WriteLine($"Generated log file path: {logFilePath}");
        Assert.NotNull(logFilePath);
        Assert.Contains("IGN_TEAS", logFilePath);
        Assert.Contains(".log", logFilePath);
        
        // Verify MD5 hash is included (should be 32 hex chars)
        // Filename format: <name>_<hash>_<timestamp>.log
        // We need to find the 32-char hex string
        var fileName = Path.GetFileNameWithoutExtension(logFilePath);
        
        // Look for 32 consecutive hex characters
        var match = System.Text.RegularExpressions.Regex.Match(fileName, @"[0-9a-f]{32}");
        Assert.True(match.Success, "Should contain a 32-character MD5 hash");
        Assert.Equal(32, match.Value.Length);
    }

    [Fact]
    public void FileLoggingHelper_AddFileLogging_ShouldWriteToFile()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"win32emu_test_{Guid.NewGuid()}.log");
        
        try
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddFileLogging(tempFile);
                builder.SetMinimumLevel(LogLevel.Information);
            });
            
            var logger = loggerFactory.CreateLogger("TestLogger");

            // Act
            logger.LogInformation("Test message 1");
            logger.LogWarning("Test warning");
            logger.LogError("Test error");

            // Force flush by disposing the factory
            loggerFactory.Dispose();

            // Assert
            Assert.True(File.Exists(tempFile), "Log file should be created");
            
            var logContent = File.ReadAllText(tempFile);
            _output.WriteLine("=== Log File Content ===");
            _output.WriteLine(logContent);
            
            Assert.Contains("Test message 1", logContent);
            Assert.Contains("Test warning", logContent);
            Assert.Contains("Test error", logContent);
            Assert.Contains("[INFO]", logContent);
            Assert.Contains("[WARN]", logContent);
            Assert.Contains("[ERROR]", logContent);
        }
        finally
        {
            // Clean up
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    /// <summary>
    /// Test logger provider that captures log messages to a list
    /// </summary>
    private class TestLoggerProvider : ILoggerProvider
    {
        private readonly List<string> _logMessages;

        public TestLoggerProvider(List<string> logMessages)
        {
            _logMessages = logMessages;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TestLogger(_logMessages);
        }

        public void Dispose()
        {
        }
    }

    /// <summary>
    /// Test logger that captures log messages to a list
    /// </summary>
    private class TestLogger : ILogger
    {
        private readonly List<string> _logMessages;

        public TestLogger(List<string> logMessages)
        {
            _logMessages = logMessages;
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
            _logMessages.Add(message);
        }
    }
}
