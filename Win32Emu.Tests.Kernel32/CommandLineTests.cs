using System.Text;
using Win32Emu.Memory;
using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Win32Emu.Win32;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for command line functions like GetCommandLineA
/// </summary>
public class CommandLineTests : IDisposable
{
    private readonly TestEnvironment _testEnv;

    public CommandLineTests()
    {
        _testEnv = new TestEnvironment();
    }

    [Fact]
    public void GetCommandLineA_WithNoArguments_ShouldReturnQuotedExecutablePath()
    {
        // Arrange
        var exePath = "C:\\test\\app.exe";
        _testEnv.ProcessEnv.InitializeStrings(exePath, []);

        // Act
        var cmdLinePtr = _testEnv.CallKernel32Api("GETCOMMANDLINEA");

        // Assert
        Assert.NotEqual(0u, cmdLinePtr);
        var cmdLine = ReadAnsiString(cmdLinePtr);
        Assert.Equal($"\"{exePath}\"", cmdLine);
    }

    [Fact]
    public void GetCommandLineA_WithSingleArgument_ShouldReturnQuotedExePathAndArgument()
    {
        // Arrange
        var exePath = "C:\\test\\app.exe";
        var args = new[] { "arg1" };
        _testEnv.ProcessEnv.InitializeStrings(exePath, args);

        // Act
        var cmdLinePtr = _testEnv.CallKernel32Api("GETCOMMANDLINEA");

        // Assert
        Assert.NotEqual(0u, cmdLinePtr);
        var cmdLine = ReadAnsiString(cmdLinePtr);
        Assert.Equal($"\"{exePath}\" arg1", cmdLine);
    }

    [Fact]
    public void GetCommandLineA_WithMultipleArguments_ShouldReturnQuotedExePathAndAllArguments()
    {
        // Arrange
        var exePath = "C:\\test\\app.exe";
        var args = new[] { "arg1", "arg2", "testing" };
        _testEnv.ProcessEnv.InitializeStrings(exePath, args);

        // Act
        var cmdLinePtr = _testEnv.CallKernel32Api("GETCOMMANDLINEA");

        // Assert
        Assert.NotEqual(0u, cmdLinePtr);
        var cmdLine = ReadAnsiString(cmdLinePtr);
        Assert.Equal($"\"{exePath}\" arg1 arg2 testing", cmdLine);
    }

    [Fact]
    public void GetCommandLineA_WithSpacesInPath_ShouldReturnQuotedExecutablePath()
    {
        // Arrange
        var exePath = "C:\\Program Files\\My App\\app.exe";
        var args = new[] { "testing" };
        _testEnv.ProcessEnv.InitializeStrings(exePath, args);

        // Act
        var cmdLinePtr = _testEnv.CallKernel32Api("GETCOMMANDLINEA");

        // Assert
        Assert.NotEqual(0u, cmdLinePtr);
        var cmdLine = ReadAnsiString(cmdLinePtr);
        Assert.Equal($"\"{exePath}\" testing", cmdLine);
    }

    /// <summary>
    /// Helper method to read a null-terminated ANSI string from memory
    /// </summary>
    private string ReadAnsiString(uint addr)
    {
        var bytes = new List<byte>();
        var currentAddr = addr;
        
        while (true)
        {
            var b = _testEnv.Memory.Read8(currentAddr);
            if (b == 0)
            {
                break;
            }

            bytes.Add(b);
            currentAddr += 1;
        }
        
        return Encoding.ASCII.GetString(bytes.ToArray());
    }

    public void Dispose()
    {
        _testEnv.Dispose();
    }
}
