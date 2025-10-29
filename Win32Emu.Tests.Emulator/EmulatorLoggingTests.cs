using Microsoft.Extensions.Logging;
using System.Text;
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
        Assert.Contains(logMessages, msg => msg.Contains("[Loader] Host Architecture:"));
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
