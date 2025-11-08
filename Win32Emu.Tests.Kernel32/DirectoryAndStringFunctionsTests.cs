using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Win32Emu.Win32;
using Win32Emu.Win32.Modules;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for directory functions (SetCurrentDirectoryA, GetCurrentDirectoryA)
/// and string functions (LstrcatA), and process execution (WinExec)
/// </summary>
public sealed class DirectoryAndStringFunctionsTests : IDisposable
{
    private readonly TestEnvironment _testEnv;
    private readonly Advapi32Module _advapi32;

    public DirectoryAndStringFunctionsTests()
    {
        _testEnv = new TestEnvironment();
        
        // Register Advapi32 module for registry tests
        _advapi32 = new Advapi32Module(_testEnv.ProcessEnv, 0x00400000);
        _testEnv.Dispatcher.RegisterModule(_advapi32);
    }

    [Fact]
    public void SetCurrentDirectoryA_ShouldSetCurrentDirectory()
    {
        // Arrange
        var newDir = @"C:\Windows\System32";
        var dirAddr = _testEnv.WriteString(newDir);

        // Act
        var result = _testEnv.CallKernel32Api("SETCURRENTDIRECTORYA", dirAddr);

        // Assert
        Assert.Equal(1u, result); // TRUE
        Assert.Equal(newDir, _testEnv.ProcessEnv.CurrentDirectory);
    }

    [Fact]
    public void SetCurrentDirectoryA_ShouldReturnFalseForNullPath()
    {
        // Arrange - null pointer (address 0)
        var dirAddr = 0u;

        // Act
        var result = _testEnv.CallKernel32Api("SETCURRENTDIRECTORYA", dirAddr);

        // Assert
        Assert.Equal(0u, result); // FALSE
    }

    [Fact]
    public void GetCurrentDirectoryA_ShouldReturnCurrentDirectory()
    {
        // Arrange
        var expectedDir = @"C:\Test\Directory";
        _testEnv.ProcessEnv.CurrentDirectory = expectedDir;
        
        var bufferSize = 260u; // MAX_PATH
        var buffer = _testEnv.ProcessEnv.SimpleAlloc(bufferSize);

        // Act
        var result = _testEnv.CallKernel32Api("GETCURRENTDIRECTORYA", bufferSize, buffer);

        // Assert
        Assert.Equal((uint)expectedDir.Length, result); // Should return length without null terminator
        var actualDir = _testEnv.ReadString(buffer);
        Assert.Equal(expectedDir, actualDir);
    }

    [Fact]
    public void GetCurrentDirectoryA_ShouldReturnRequiredSizeWhenBufferTooSmall()
    {
        // Arrange
        var expectedDir = @"C:\This\Is\A\Very\Long\Directory\Path";
        _testEnv.ProcessEnv.CurrentDirectory = expectedDir;
        
        var bufferSize = 10u; // Too small
        var buffer = _testEnv.ProcessEnv.SimpleAlloc(bufferSize);

        // Act
        var result = _testEnv.CallKernel32Api("GETCURRENTDIRECTORYA", bufferSize, buffer);

        // Assert
        Assert.Equal((uint)expectedDir.Length + 1, result); // Should return required size
    }

    [Fact]
    public void LstrcatA_ShouldConcatenateTwoStrings()
    {
        // Arrange
        var str1 = "Hello, ";
        var str2 = "World!";
        var expected = str1 + str2;
        
        // Allocate buffer for str1 with enough space for concatenation
        var buffer = _testEnv.ProcessEnv.SimpleAlloc(100);
        _testEnv.Memory.WriteBytes(buffer, System.Text.Encoding.ASCII.GetBytes(str1 + "\0"));
        
        var str2Addr = _testEnv.WriteString(str2);

        // Act
        var result = _testEnv.CallKernel32Api("LSTRCATA", buffer, str2Addr);

        // Assert
        Assert.Equal(buffer, result); // Should return pointer to destination
        var actualResult = _testEnv.ReadString(buffer);
        Assert.Equal(expected, actualResult);
    }

    [Fact]
    public void WinExec_ShouldReturnSuccessCode()
    {
        // Arrange
        var cmdLine = "notepad.exe";
        var cmdLineAddr = _testEnv.WriteString(cmdLine);
        var showCmd = 1u; // SW_SHOWNORMAL

        // Act
        var result = _testEnv.CallKernel32Api("WINEXEC", cmdLineAddr, showCmd);

        // Assert
        Assert.True(result > 31); // Values > 31 indicate success
        Assert.Equal(33u, result); // SE_ERR_SUCCESS
    }

    [Fact]
    public void WinExec_WithQuotedPath_ShouldReturnSuccessCode()
    {
        // Arrange
        var cmdLine = "\"C:\\Program Files\\App.exe\" /arg1 /arg2";
        var cmdLineAddr = _testEnv.WriteString(cmdLine);
        var showCmd = 5u; // SW_SHOW

        // Act
        var result = _testEnv.CallKernel32Api("WINEXEC", cmdLineAddr, showCmd);

        // Assert
        Assert.True(result > 31); // Values > 31 indicate success
    }

    [Fact]
    public void RegOpenKeyExA_ShouldOpenRegistryKey()
    {
        // Arrange
        const uint HKEY_LOCAL_MACHINE = 0x80000002;
        var subKey = @"SOFTWARE\Microsoft\Windows";
        var subKeyAddr = _testEnv.WriteString(subKey);
        var handlePtr = _testEnv.ProcessEnv.SimpleAlloc(4);

        // Act
        _testEnv.Cpu.SetupStackArgs(_testEnv.Memory, 
            HKEY_LOCAL_MACHINE, 
            subKeyAddr, 
            0u, // ulOptions
            0u, // samDesired
            handlePtr);
        var result = _advapi32.TryInvokeUnsafe("REGOPENKEYEXA", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

        // Assert
        Assert.True(result);
        Assert.Equal(0u, returnValue); // ERROR_SUCCESS
        
        var handle = _testEnv.Memory.Read32(handlePtr);
        Assert.NotEqual(0u, handle); // Should have a valid handle
    }

    [Fact]
    public void RegQueryValueExA_ShouldReturnErrorForNonexistentValue()
    {
        // Arrange
        var handle = _testEnv.ProcessEnv.RegOpenKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Test");
        var valueName = "NonexistentValue";
        var valueNameAddr = _testEnv.WriteString(valueName);
        var typePtr = _testEnv.ProcessEnv.SimpleAlloc(4);
        var dataPtr = _testEnv.ProcessEnv.SimpleAlloc(100);
        var dataSizePtr = _testEnv.ProcessEnv.SimpleAlloc(4);
        _testEnv.Memory.Write32(dataSizePtr, 100);

        // Act
        _testEnv.Cpu.SetupStackArgs(_testEnv.Memory, 
            handle, 
            valueNameAddr, 
            0u, // lpReserved
            typePtr,
            dataPtr,
            dataSizePtr);
        var result = _advapi32.TryInvokeUnsafe("REGQUERYVALUEEXA", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

        // Assert
        Assert.True(result);
        Assert.Equal(2u, returnValue); // ERROR_FILE_NOT_FOUND
    }

    [Fact]
    public void RegCloseKey_ShouldCloseRegistryHandle()
    {
        // Arrange
        var handle = _testEnv.ProcessEnv.RegOpenKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Test");

        // Act
        _testEnv.Cpu.SetupStackArgs(_testEnv.Memory, handle);
        var result = _advapi32.TryInvokeUnsafe("REGCLOSEKEY", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

        // Assert
        Assert.True(result);
        Assert.Equal(0u, returnValue); // ERROR_SUCCESS
    }

    [Fact]
    public void SetSearchPathMode_EnableSafeMode_ShouldSucceed()
    {
        // Arrange
        const uint BASE_SEARCH_PATH_ENABLE_SAFE_SEARCHMODE = 0x00000001;

        // Act
        var result = _testEnv.CallKernel32Api("SETSEARCHPATHMODE", BASE_SEARCH_PATH_ENABLE_SAFE_SEARCHMODE);

        // Assert
        Assert.Equal(1u, result); // TRUE
        Assert.Equal(BASE_SEARCH_PATH_ENABLE_SAFE_SEARCHMODE, _testEnv.ProcessEnv.SearchPathMode);
        Assert.False(_testEnv.ProcessEnv.SearchPathModePermanent);
    }

    [Fact]
    public void SetSearchPathMode_DisableSafeMode_ShouldSucceed()
    {
        // Arrange
        const uint BASE_SEARCH_PATH_DISABLE_SAFE_SEARCHMODE = 0x00010000;

        // Act
        var result = _testEnv.CallKernel32Api("SETSEARCHPATHMODE", BASE_SEARCH_PATH_DISABLE_SAFE_SEARCHMODE);

        // Assert
        Assert.Equal(1u, result); // TRUE
        Assert.Equal(BASE_SEARCH_PATH_DISABLE_SAFE_SEARCHMODE, _testEnv.ProcessEnv.SearchPathMode);
        Assert.False(_testEnv.ProcessEnv.SearchPathModePermanent);
    }

    [Fact]
    public void SetSearchPathMode_EnableWithPermanent_ShouldSucceed()
    {
        // Arrange
        const uint BASE_SEARCH_PATH_ENABLE_SAFE_SEARCHMODE = 0x00000001;
        const uint BASE_SEARCH_PATH_PERMANENT = 0x00008000;
        var flags = BASE_SEARCH_PATH_ENABLE_SAFE_SEARCHMODE | BASE_SEARCH_PATH_PERMANENT;

        // Act
        var result = _testEnv.CallKernel32Api("SETSEARCHPATHMODE", flags);

        // Assert
        Assert.Equal(1u, result); // TRUE
        Assert.Equal(BASE_SEARCH_PATH_ENABLE_SAFE_SEARCHMODE, _testEnv.ProcessEnv.SearchPathMode);
        Assert.True(_testEnv.ProcessEnv.SearchPathModePermanent);
    }

    [Fact]
    public void SetSearchPathMode_BothEnableAndDisable_ShouldFail()
    {
        // Arrange
        const uint BASE_SEARCH_PATH_ENABLE_SAFE_SEARCHMODE = 0x00000001;
        const uint BASE_SEARCH_PATH_DISABLE_SAFE_SEARCHMODE = 0x00010000;
        var flags = BASE_SEARCH_PATH_ENABLE_SAFE_SEARCHMODE | BASE_SEARCH_PATH_DISABLE_SAFE_SEARCHMODE;

        // Act
        var result = _testEnv.CallKernel32Api("SETSEARCHPATHMODE", flags);

        // Assert
        Assert.Equal(0u, result); // FALSE
    }

    [Fact]
    public void SetSearchPathMode_DisableWithPermanent_ShouldFail()
    {
        // Arrange
        const uint BASE_SEARCH_PATH_DISABLE_SAFE_SEARCHMODE = 0x00010000;
        const uint BASE_SEARCH_PATH_PERMANENT = 0x00008000;
        var flags = BASE_SEARCH_PATH_DISABLE_SAFE_SEARCHMODE | BASE_SEARCH_PATH_PERMANENT;

        // Act
        var result = _testEnv.CallKernel32Api("SETSEARCHPATHMODE", flags);

        // Assert
        Assert.Equal(0u, result); // FALSE
    }

    [Fact]
    public void SetSearchPathMode_NoFlags_ShouldFail()
    {
        // Arrange
        const uint flags = 0;

        // Act
        var result = _testEnv.CallKernel32Api("SETSEARCHPATHMODE", flags);

        // Assert
        Assert.Equal(0u, result); // FALSE
    }

    [Fact]
    public void SetSearchPathMode_AfterPermanent_ShouldFail()
    {
        // Arrange
        const uint BASE_SEARCH_PATH_ENABLE_SAFE_SEARCHMODE = 0x00000001;
        const uint BASE_SEARCH_PATH_DISABLE_SAFE_SEARCHMODE = 0x00010000;
        const uint BASE_SEARCH_PATH_PERMANENT = 0x00008000;

        // Set to permanent first
        var flags = BASE_SEARCH_PATH_ENABLE_SAFE_SEARCHMODE | BASE_SEARCH_PATH_PERMANENT;
        var firstResult = _testEnv.CallKernel32Api("SETSEARCHPATHMODE", flags);
        Assert.Equal(1u, firstResult); // Should succeed

        // Act - Try to change it
        var secondResult = _testEnv.CallKernel32Api("SETSEARCHPATHMODE", BASE_SEARCH_PATH_DISABLE_SAFE_SEARCHMODE);

        // Assert
        Assert.Equal(0u, secondResult); // Should fail
        Assert.Equal(BASE_SEARCH_PATH_ENABLE_SAFE_SEARCHMODE, _testEnv.ProcessEnv.SearchPathMode); // Mode should not have changed
    }

    public void Dispose()
    {
        _testEnv?.Dispose();
    }
}
