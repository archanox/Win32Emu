using Microsoft.Extensions.Logging;
using Win32Emu;
using Win32Emu.Loader;
using Win32Emu.Tests.Emulator.TestInfrastructure;
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
        // Arrange - Find sol.exe using multiple possible paths
        var solPath = FindSolExePath();
        if (solPath == null)
        {
            throw new SkipException("sol.exe not found in expected locations (EXEs/WinME/sol.exe). Ensure test data is available.");
        }
        
        _output.WriteLine($"Loading sol.exe from: {solPath}");
        
        var solBytes = File.ReadAllBytes(solPath);
        _output.WriteLine($"sol.exe size: {solBytes.Length} bytes");
        
        // Verify it's an NE executable
        Assert.True(NeImageLoader.IsNE(solBytes), "sol.exe should be a valid NE (Win16) executable");
        
        // Create a test logger that captures log output using shared infrastructure
        var (loggerFactory, logMessages) = TestLoggerHelper.CreateTestLoggerFactory(_output);
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
    
    /// <summary>
    /// Finds sol.exe by checking multiple possible paths:
    /// 1. Repository root relative path (EXEs/WinME/sol.exe)
    /// 2. Environment variable WIN32EMU_TEST_DATA_DIR
    /// 3. Current directory navigation up to repository root
    /// </summary>
    private string? FindSolExePath()
    {
        // Try environment variable first (most flexible for different CI/dev environments)
        var testDataDir = Environment.GetEnvironmentVariable("WIN32EMU_TEST_DATA_DIR");
        if (!string.IsNullOrEmpty(testDataDir))
        {
            var envPath = Path.Combine(testDataDir, "EXEs", "WinME", "sol.exe");
            if (File.Exists(envPath))
            {
                return envPath;
            }
        }
        
        // Try to find repository root by walking up from current directory
        var currentDir = Directory.GetCurrentDirectory();
        var dir = new DirectoryInfo(currentDir);
        
        while (dir != null)
        {
            // Check if this looks like the repository root (has EXEs directory)
            var testPath = Path.Combine(dir.FullName, "EXEs", "WinME", "sol.exe");
            if (File.Exists(testPath))
            {
                return testPath;
            }
            
            dir = dir.Parent;
        }
        
        // Try relative path from test binary location (for dotnet test runs)
        var relativePaths = new[]
        {
            Path.Combine("..", "..", "..", "..", "..", "EXEs", "WinME", "sol.exe"),
            Path.Combine("..", "..", "..", "..", "..", "..", "EXEs", "WinME", "sol.exe"),
            Path.Combine("EXEs", "WinME", "sol.exe")
        };
        
        foreach (var relPath in relativePaths)
        {
            var fullPath = Path.GetFullPath(Path.Combine(currentDir, relPath));
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }
        
        return null;
    }
}

/// <summary>
/// Exception type used to skip xUnit tests with a clear message
/// </summary>
public class SkipException : Exception
{
    public SkipException(string message) : base(message)
    {
    }
}
