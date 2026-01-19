using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Win32Emu;
using Win32Emu.Loader;
using Xunit;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Test to verify sol.exe (Win16 Solitaire) can be loaded without crashing during Win16 module registration.
/// This test validates the fix for the truncated "[Emulato..." error.
/// </summary>
public class SolExeLoadTest
{
    private readonly ITestOutputHelper _output;
    
    public SolExeLoadTest(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void SolExe_LoadsSuccessfully_WithWin16ModuleRegistration()
    {
        // Arrange
        var solPath = "/home/runner/work/Win32Emu/Win32Emu/EXEs/WinME/sol.exe";
        if (!File.Exists(solPath))
        {
            // Fallback to relative path for local development
            solPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "EXEs", "WinME", "sol.exe");
        }
        
        _output.WriteLine($"Loading sol.exe from: {solPath}");
        
        // Verify file exists
        if (!File.Exists(solPath))
        {
            _output.WriteLine($"sol.exe not found at {solPath}, skipping test");
            return; // Skip test if file not found (e.g., in CI without test files)
        }
        
        var solBytes = File.ReadAllBytes(solPath);
        _output.WriteLine($"sol.exe size: {solBytes.Length} bytes");
        
        // Verify it's an NE executable
        Assert.True(NeImageLoader.IsNE(solBytes), "sol.exe should be a valid NE (Win16) executable");
        
        // Create a test logger that captures log output
        var logMessages = new ConcurrentBag<string>();
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new SimpleTestLoggerProvider((category, level, message) =>
            {
                var logEntry = $"[{level}] [{category}] {message}";
                logMessages.Add(logEntry);
                _output.WriteLine(logEntry);
            }));
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        var logger = loggerFactory.CreateLogger<Win32Emu.Emulator>();
        
        // Act & Assert
        Exception? caughtException = null;
        try
        {
            var emulator = new Win32Emu.Emulator(logger: logger);
            
            // This should not throw during Win16 module registration
            emulator.LoadExecutableFromBytes(
                solBytes,
                "sol.exe",
                programArgs: Array.Empty<string>(),
                debugMode: false,
                reservedMemoryMb: 256,
                virtualFileSystem: null,
                force32BitStackOps: false,
                forceInterpreterMode: true
            );
            
            _output.WriteLine("✓ sol.exe loaded successfully!");
            _output.WriteLine($"Entry point: 0x{emulator.LoadedImage?.EntryPointAddress:X8}");
            _output.WriteLine($"Image base: 0x{emulator.LoadedImage?.BaseAddress:X8}");
        }
        catch (Exception ex)
        {
            caughtException = ex;
            _output.WriteLine($"✗ Exception caught: {ex.GetType().Name}");
            _output.WriteLine($"Message: {ex.Message}");
            if (ex.StackTrace != null)
            {
                _output.WriteLine($"Stack trace:\n{ex.StackTrace}");
            }
        }
        
        // Verify we got past Win16 module registration
        var hasWin16RegLog = logMessages.Any(m => m.Contains("Registering Win16 thunking modules"));
        var hasWin16SuccessLog = logMessages.Any(m => m.Contains("Win16 thunking modules registered successfully"));
        
        _output.WriteLine($"\nLog analysis:");
        _output.WriteLine($"  - Found 'Registering Win16' log: {hasWin16RegLog}");
        _output.WriteLine($"  - Found 'registered successfully' log: {hasWin16SuccessLog}");
        
        // Check for our new debug logs
        var hasKernelLookup = logMessages.Any(m => m.Contains("Looking up KERNEL32.DLL"));
        var hasUserLookup = logMessages.Any(m => m.Contains("Looking up USER32.DLL"));
        var hasGdiLookup = logMessages.Any(m => m.Contains("Looking up GDI32.DLL"));
        var hasWinmmLookup = logMessages.Any(m => m.Contains("Looking up WINMM.DLL"));
        
        _output.WriteLine($"  - Found KERNEL32 lookup log: {hasKernelLookup}");
        _output.WriteLine($"  - Found USER32 lookup log: {hasUserLookup}");
        _output.WriteLine($"  - Found GDI32 lookup log: {hasGdiLookup}");
        _output.WriteLine($"  - Found WINMM lookup log: {hasWinmmLookup}");
        
        // If an exception was thrown, fail the test with details
        if (caughtException != null)
        {
            Assert.Fail($"sol.exe loading failed with {caughtException.GetType().Name}: {caughtException.Message}");
        }
        
        // Verify we completed Win16 registration
        Assert.True(hasWin16RegLog, "Should have logged 'Registering Win16 thunking modules'");
        Assert.True(hasWin16SuccessLog, "Should have logged 'Win16 thunking modules registered successfully'");
    }
}

/// <summary>
/// Simple test logger provider for capturing log output - uses different name to avoid conflicts
/// </summary>
internal class SimpleTestLoggerProvider : ILoggerProvider
{
    private readonly Action<string, LogLevel, string> _logAction;
    
    public SimpleTestLoggerProvider(Action<string, LogLevel, string> logAction)
    {
        _logAction = logAction;
    }
    
    public ILogger CreateLogger(string categoryName) => new SimpleTestLogger(categoryName, _logAction);
    
    public void Dispose() { }
}

/// <summary>
/// Simple test logger for capturing log output - uses different name to avoid conflicts
/// </summary>
internal class SimpleTestLogger : ILogger
{
    private readonly string _categoryName;
    private readonly Action<string, LogLevel, string> _logAction;
    
    public SimpleTestLogger(string categoryName, Action<string, LogLevel, string> logAction)
    {
        _categoryName = categoryName;
        _logAction = logAction;
    }
    
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    
    public bool IsEnabled(LogLevel logLevel) => true;
    
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        _logAction(_categoryName, logLevel, message);
    }
}
