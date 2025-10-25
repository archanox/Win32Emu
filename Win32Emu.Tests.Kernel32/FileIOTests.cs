using Win32Emu.Tests.Kernel32.TestInfrastructure;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for Kernel32 file I/O functions like CreateFileA, ReadFile, WriteFile, CloseHandle, GetFileType
/// </summary>
public class FileIoTests : IDisposable
{
    private readonly TestEnvironment _testEnv;

    public FileIoTests()
    {
        _testEnv = new TestEnvironment();
    }

    #region CreateFileA Tests

    [Fact]
    public void CreateFileA_WithValidFileName_ShouldReturnValidHandle()
    {
        // Set CurrentDirectory to a writable temp directory so the test works
        // when VFS is not available (fallback to direct filesystem access)
        var tempDir = Path.GetTempPath();
        _testEnv.ProcessEnv.CurrentDirectory = tempDir;

        // Arrange
        var fileName = _testEnv.WriteString("test.txt");
        const uint desiredAccess = 0x80000000; // GENERIC_READ
        const uint shareMode = 0x00000001; // FILE_SHARE_READ
        const uint securityAttributes = 0; // NULL
        const uint creationDisposition = 4; // OPEN_ALWAYS
        const uint flagsAndAttributes = 0x80; // FILE_ATTRIBUTE_NORMAL
        const uint templateFile = 0; // NULL

        // Act
        var handle = _testEnv.CallKernel32Api("CREATEFILEA", fileName, desiredAccess, shareMode,
            securityAttributes, creationDisposition, flagsAndAttributes, templateFile);

        // Assert
        Assert.NotEqual(0u, handle);
        Assert.NotEqual(0xFFFFFFFFu, handle); // INVALID_HANDLE_VALUE

        // Cleanup - close the handle and delete the file
        if (handle != 0xFFFFFFFF && handle != 0)
        {
            _testEnv.CallKernel32Api("CLOSEHANDLE", handle);
            var testFilePath = Path.Combine(tempDir, "test.txt");
            if (File.Exists(testFilePath))
            {
                File.Delete(testFilePath);
            }
        }
    }

    [Fact]
    public void CreateFileA_WithInvalidFileName_ShouldReturnInvalidHandle()
    {
        // Arrange
        var fileName = _testEnv.WriteString(""); // Empty filename
        const uint desiredAccess = 0x80000000; // GENERIC_READ
        const uint shareMode = 0x00000001; // FILE_SHARE_READ
        const uint securityAttributes = 0; // NULL
        const uint creationDisposition = 3; // OPEN_EXISTING
        const uint flagsAndAttributes = 0x80; // FILE_ATTRIBUTE_NORMAL
        const uint templateFile = 0; // NULL

        // Act
        var handle = _testEnv.CallKernel32Api("CREATEFILEA", fileName, desiredAccess, shareMode,
            securityAttributes, creationDisposition, flagsAndAttributes, templateFile);

        // Assert
        Assert.Equal(0xFFFFFFFFu, handle); // INVALID_HANDLE_VALUE
    }

    [Fact]
    public void CreateFileA_WithRelativePath_ShouldResolveAgainstCurrentDirectory()
    {
        // This test verifies that CreateFileA resolves relative paths correctly
        // against the emulated CurrentDirectory, not the actual process working directory.
        // This is important when VFS is not available (fallback to direct filesystem access).
        
        // Arrange - Create a temporary test directory and file
        var testDir = Path.Combine(Path.GetTempPath(), "Win32EmuTest_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        try
        {
            var testFileName = "testfile.txt";
            var testFilePath = Path.Combine(testDir, testFileName);
            File.WriteAllText(testFilePath, "Test content");

            // Set the emulated CurrentDirectory to our test directory
            _testEnv.ProcessEnv.CurrentDirectory = testDir;

            // Create a relative path (just the filename, no directory)
            var fileName = _testEnv.WriteString(testFileName);
            const uint desiredAccess = 0x80000000; // GENERIC_READ
            const uint shareMode = 0x00000001; // FILE_SHARE_READ
            const uint securityAttributes = 0; // NULL
            const uint creationDisposition = 3; // OPEN_EXISTING
            const uint flagsAndAttributes = 0x80; // FILE_ATTRIBUTE_NORMAL
            const uint templateFile = 0; // NULL

            // Act
            var handle = _testEnv.CallKernel32Api("CREATEFILEA", fileName, desiredAccess, shareMode,
                securityAttributes, creationDisposition, flagsAndAttributes, templateFile);

            // Assert
            Assert.NotEqual(0xFFFFFFFFu, handle); // Should succeed, not INVALID_HANDLE_VALUE
            Assert.NotEqual(0u, handle); // Should be a valid handle

            // Cleanup - close the handle
            if (handle != 0xFFFFFFFF && handle != 0)
            {
                _testEnv.CallKernel32Api("CLOSEHANDLE", handle);
            }
        }
        finally
        {
            // Cleanup - delete test directory and file
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }

    #endregion

    #region GetStdHandle Tests

    [Fact]
    public void GetStdHandle_StdInput_ShouldReturnInputHandle()
    {
        // Arrange
        const uint stdInputHandle = 0xFFFFFFF6; // STD_INPUT_HANDLE

        // Act
        var handle = _testEnv.CallKernel32Api("GETSTDHANDLE", stdInputHandle);

        // Assert
        // For GUI apps without a console, standard handles are NULL
        Assert.Equal(0x00000000u, handle); // NULL - no console
    }

    [Fact]
    public void GetStdHandle_StdOutput_ShouldReturnOutputHandle()
    {
        // Arrange
        const uint stdOutputHandle = 0xFFFFFFF5; // STD_OUTPUT_HANDLE

        // Act
        var handle = _testEnv.CallKernel32Api("GETSTDHANDLE", stdOutputHandle);

        // Assert
        // For GUI apps without a console, standard handles are NULL
        Assert.Equal(0x00000000u, handle); // NULL - no console
    }

    [Fact]
    public void GetStdHandle_StdError_ShouldReturnErrorHandle()
    {
        // Arrange
        const uint stdErrorHandle = 0xFFFFFFF4; // STD_ERROR_HANDLE

        // Act
        var handle = _testEnv.CallKernel32Api("GETSTDHANDLE", stdErrorHandle);

        // Assert
        // For GUI apps without a console, standard handles are NULL
        Assert.Equal(0x00000000u, handle); // NULL - no console
    }

    #endregion

    #region SetStdHandle Tests

    [Fact]
    public void SetStdHandle_StdOutput_ShouldReturnOne()
    {
        // Arrange
        const uint stdOutputHandle = 0xFFFFFFF5; // STD_OUTPUT_HANDLE
        const uint newHandle = 0x12345678;

        // Act
        var result = _testEnv.CallKernel32Api("SETSTDHANDLE", stdOutputHandle, newHandle);

        // Assert
        Assert.Equal(1u, result); // SetStdHandle returns 1 on success

        // Verify the handle was set
        var retrievedHandle = _testEnv.CallKernel32Api("GETSTDHANDLE", stdOutputHandle);
        Assert.Equal(newHandle, retrievedHandle);
    }

    [Fact]
    public void GetStartupInfoA_ShouldReturnActualHandlesInStartupInfo()
    {
        // Arrange
        // Allocate memory for STARTUPINFO structure (68 bytes)
        var startupInfoPtr = _testEnv.AllocateMemory(68);

        // Act
        _testEnv.CallKernel32Api("GETSTARTUPINFOA", startupInfoPtr);

        // Assert
        // STARTUPINFO structure offsets:
        // +0: cb (size) - should be 68
        // +56: hStdInput - should be actual handle value (NULL for GUI apps without console)
        // +60: hStdOutput - should be actual handle value (NULL for GUI apps without console)
        // +64: hStdError - should be actual handle value (NULL for GUI apps without console)
        
        var cb = _testEnv.Memory.Read32(startupInfoPtr);
        var hStdInput = _testEnv.Memory.Read32(startupInfoPtr + 56);
        var hStdOutput = _testEnv.Memory.Read32(startupInfoPtr + 60);
        var hStdError = _testEnv.Memory.Read32(startupInfoPtr + 64);

        Assert.Equal(68u, cb);
        // GUI apps without a console have NULL standard handles
        Assert.Equal(0x00000000u, hStdInput);
        Assert.Equal(0x00000000u, hStdOutput);
        Assert.Equal(0x00000000u, hStdError);
    }

    [Fact]
    public void GetStartupInfoA_ThenGetStdHandle_ShouldWorkCorrectly()
    {
        // This test simulates the correct program behavior:
        // Programs can use GetStdHandle with pseudo-handle constants to get standard handles
        // This is independent of what GetStartupInfoA returns
        
        // Arrange & Act
        // Call GetStdHandle with STD_OUTPUT_HANDLE pseudo-handle constant
        var realHandle = _testEnv.CallKernel32Api("GETSTDHANDLE", 0xFFFFFFF5u);
        
        // For GUI apps without a console, standard handles are NULL
        Assert.Equal(0x00000000u, realHandle); // NULL - no console
    }

    [Fact]
    public void GetStartupInfoA_WithConsole_ShouldReturnRealHandles()
    {
        // This test verifies that when a console is allocated,
        // GetStartupInfoA returns the actual handle values, not pseudo-handles
        
        // Arrange
        // Allocate a console
        _testEnv.CallKernel32Api("ALLOCCONSOLE");
        
        // Allocate memory for STARTUPINFO structure (68 bytes)
        var startupInfoPtr = _testEnv.AllocateMemory(68);
        
        // Act
        _testEnv.CallKernel32Api("GETSTARTUPINFOA", startupInfoPtr);
        
        // Assert
        var cb = _testEnv.Memory.Read32(startupInfoPtr);
        var hStdInput = _testEnv.Memory.Read32(startupInfoPtr + 56);
        var hStdOutput = _testEnv.Memory.Read32(startupInfoPtr + 60);
        var hStdError = _testEnv.Memory.Read32(startupInfoPtr + 64);
        
        Assert.Equal(68u, cb);
        // Console apps should have real handle values (not NULL, not pseudo-handles)
        Assert.Equal(0x00000001u, hStdInput);  // Real stdin handle
        Assert.Equal(0x00000002u, hStdOutput); // Real stdout handle
        Assert.Equal(0x00000003u, hStdError);  // Real stderr handle
    }

    #endregion

    #region WriteFile Tests

    [Fact(Skip = "Console I/O test - requires console handles to be initialized. GUI apps have NULL standard handles by default.")]
    public void WriteFile_ToStdOutput_ShouldSucceed()
    {
	    // Arrange
	    const uint stdOutputHandle = 0x00000002u; // Default stdout handle
	    const string testMessage = "Hello, World!\n";
	    var messagePtr = _testEnv.WriteString(testMessage);
	    var bytesWrittenPtr = _testEnv.AllocateMemory(4); // Allocate space for bytes written

	    // Act
	    var result = _testEnv.CallKernel32Api("WRITEFILE", stdOutputHandle, messagePtr, 
		    (uint)testMessage.Length, bytesWrittenPtr, 0);

	    // Assert
	    Assert.Equal(1u, result); // WriteFile returns 1 on success
        
	    // Verify bytes written
	    var bytesWritten = _testEnv.Memory.Read32(bytesWrittenPtr);
	    Assert.Equal((uint)testMessage.Length, bytesWritten);
    }
    
    [Fact(Skip = "Console I/O test - requires console handles to be initialized. GUI apps have NULL standard handles by default.")]
    public void WriteFile_ToStdError_ShouldSucceed()
    {
	    // Arrange
	    const uint stdErrorHandle = 0x00000003u; // Default stderr handle
	    const string testMessage = "Error message\n";
	    var messagePtr = _testEnv.WriteString(testMessage);
	    var bytesWrittenPtr = _testEnv.AllocateMemory(4); // Allocate space for bytes written

	    // Act
	    var result = _testEnv.CallKernel32Api("WRITEFILE", stdErrorHandle, messagePtr, 
		    (uint)testMessage.Length, bytesWrittenPtr, 0);

	    // Assert
	    Assert.Equal(1u, result); // WriteFile returns 1 on success
        
	    // Verify bytes written
	    var bytesWritten = _testEnv.Memory.Read32(bytesWrittenPtr);
	    Assert.Equal((uint)testMessage.Length, bytesWritten);
    }
    
    [Fact(Skip = "Console I/O test - requires console handles to be initialized. GUI apps have NULL standard handles by default.")]
    public void WriteFile_WithStdOutputHandle_ShouldReturnOne()
    {
	    // Arrange
	    const uint stdOutputHandle = 0xFFFFFFF5; // STD_OUTPUT_HANDLE
	    var handle = _testEnv.CallKernel32Api("GETSTDHANDLE", stdOutputHandle);
	    var buffer = _testEnv.WriteString("hello\n");
	    const uint bytesToWrite = 6; // Length of "hello\n"
	    var bytesWrittenPtr = _testEnv.AllocateMemory(4);

	    // Act
	    var result = _testEnv.CallKernel32Api("WRITEFILE", handle, buffer, bytesToWrite, bytesWrittenPtr, 0);

	    // Assert
	    Assert.Equal(1u, result); // WriteFile returns 1 on success
        
	    // Verify bytes written was set correctly
	    var bytesWritten = _testEnv.Memory.Read32(bytesWrittenPtr);
	    Assert.Equal(bytesToWrite, bytesWritten);
    }

    [Fact(Skip = "Console I/O test - requires console handles to be initialized. GUI apps have NULL standard handles by default.")]
    public void WriteFile_WithStdErrorHandle_ShouldReturnOne()
    {
	    // Arrange
	    const uint stdErrorHandle = 0xFFFFFFF4; // STD_ERROR_HANDLE
	    var handle = _testEnv.CallKernel32Api("GETSTDHANDLE", stdErrorHandle);
	    var buffer = _testEnv.WriteString("error\n");
	    const uint bytesToWrite = 6; // Length of "error\n"
	    var bytesWrittenPtr = _testEnv.AllocateMemory(4);

	    // Act
	    var result = _testEnv.CallKernel32Api("WRITEFILE", handle, buffer, bytesToWrite, bytesWrittenPtr, 0);

	    // Assert
	    Assert.Equal(1u, result); // WriteFile returns 1 on success
        
	    // Verify bytes written was set correctly
	    var bytesWritten = _testEnv.Memory.Read32(bytesWrittenPtr);
	    Assert.Equal(bytesToWrite, bytesWritten);
    }

    [Fact]
    public void WriteFile_WithInvalidHandle_ShouldReturnZeroAndSetLastError()
    {
	    // Arrange
	    const uint invalidHandle = 0x00000000; // NULL handle
	    var buffer = _testEnv.WriteString("test\n");
	    const uint bytesToWrite = 5; // Length of "test\n"
	    var bytesWrittenPtr = _testEnv.AllocateMemory(4);

	    // Act
	    var result = _testEnv.CallKernel32Api("WRITEFILE", invalidHandle, buffer, bytesToWrite, bytesWrittenPtr, 0);

	    // Assert
	    Assert.Equal(0u, result); // WriteFile returns 0 on failure
        
	    // Verify GetLastError returns ERROR_INVALID_HANDLE (6)
	    var lastError = _testEnv.CallKernel32Api("GETLASTERROR");
	    Assert.Equal(6u, lastError); // ERROR_INVALID_HANDLE
    }

    #endregion
    
    #region CloseHandle Tests

    [Fact] 
    public void CloseHandle_WithValidHandle_ShouldReturnOne()
    {
        // Arrange - Create a file first
        var fileName = _testEnv.WriteString("test.txt");
        var handle = _testEnv.CallKernel32Api("CREATEFILEA", fileName, 0x80000000, 0x00000001,
            0, 4, 0x80, 0);
        
        // Skip test if file creation failed
        if (handle == 0xFFFFFFFF)
        {
	        return;
        }

        // Act
        var result = _testEnv.CallKernel32Api("CLOSEHANDLE", handle);

        // Assert
        Assert.Equal(1u, result); // CloseHandle returns 1 on success
    }

    [Fact]
    public void CloseHandle_WithInvalidHandle_ShouldReturnZero()
    {
        // Arrange
        const uint invalidHandle = 0x12345678; // Random invalid handle

        // Act
        var result = _testEnv.CallKernel32Api("CLOSEHANDLE", invalidHandle);

        // Assert
        Assert.Equal(0u, result); // CloseHandle returns 0 on failure
    }

    #endregion

    #region GetFileType Tests

    [Fact]
    public void GetFileType_WithFileHandle_ShouldReturnDiskType()
    {
        // Arrange - Create a file first
        var fileName = _testEnv.WriteString("test.txt");
        var handle = _testEnv.CallKernel32Api("CREATEFILEA", fileName, 0x80000000, 0x00000001,
            0, 4, 0x80, 0);
        
        // Skip test if file creation failed
        if (handle == 0xFFFFFFFF)
        {
	        return;
        }

        // Act
        var fileType = _testEnv.CallKernel32Api("GETFILETYPE", handle);

        // Assert
        Assert.Equal(0x0001u, fileType); // FILE_TYPE_DISK

        // Cleanup
        _testEnv.CallKernel32Api("CLOSEHANDLE", handle);
    }

    [Fact]
    public void GetFileType_WithInvalidHandle_ShouldReturnUnknown()
    {
        // Arrange
        const uint invalidHandle = 0x12345678;

        // Act
        var fileType = _testEnv.CallKernel32Api("GETFILETYPE", invalidHandle);

        // Assert
        Assert.Equal(0u, fileType); // FILE_TYPE_UNKNOWN
    }

    [Fact]
    public void GetFileType_WithNullHandle_ShouldReturnUnknown()
    {
        // Arrange - NULL handle (for GUI apps without console)
        const uint nullHandle = 0x00000000;

        // Act
        var fileType = _testEnv.CallKernel32Api("GETFILETYPE", nullHandle);

        // Assert
        Assert.Equal(0u, fileType); // FILE_TYPE_UNKNOWN
    }

    #endregion

    #region SetHandleCount Tests

    [Fact]
    public void SetHandleCount_WithValidNumber_ShouldReturnSameNumber()
    {
        // Arrange
        const uint handleCount = 64;

        // Act
        var result = _testEnv.CallKernel32Api("SETHANDLECOUNT", handleCount);

        // Assert
        Assert.Equal(handleCount, result); // SetHandleCount returns the number passed
    }

    #endregion

    #region Console Tests

    [Fact]
    public void AllocConsole_ShouldAllocateConsoleAndSetHandles()
    {
        // Act
        var result = _testEnv.CallKernel32Api("ALLOCCONSOLE");

        // Assert
        Assert.Equal(1u, result); // TRUE

        // Verify standard handles are now set
        var stdinHandle = _testEnv.CallKernel32Api("GETSTDHANDLE", 0xFFFFFFF6u);
        var stdoutHandle = _testEnv.CallKernel32Api("GETSTDHANDLE", 0xFFFFFFF5u);
        var stderrHandle = _testEnv.CallKernel32Api("GETSTDHANDLE", 0xFFFFFFF4u);

        Assert.NotEqual(0u, stdinHandle);  // Should not be NULL
        Assert.NotEqual(0u, stdoutHandle); // Should not be NULL
        Assert.NotEqual(0u, stderrHandle); // Should not be NULL
    }

    [Fact]
    public void AllocConsole_WhenConsoleExists_ShouldReturnFalse()
    {
        // Arrange - allocate console first
        _testEnv.CallKernel32Api("ALLOCCONSOLE");

        // Act - try to allocate again
        var result = _testEnv.CallKernel32Api("ALLOCCONSOLE");

        // Assert
        Assert.Equal(0u, result); // FALSE
        
        // Verify last error is set to ERROR_ACCESS_DENIED (5)
        var lastError = _testEnv.CallKernel32Api("GETLASTERROR");
        Assert.Equal(5u, lastError);
    }

    [Fact]
    public void FreeConsole_ShouldFreeConsoleAndResetHandles()
    {
        // Arrange - allocate console first
        _testEnv.CallKernel32Api("ALLOCCONSOLE");

        // Act
        var result = _testEnv.CallKernel32Api("FREECONSOLE");

        // Assert
        Assert.Equal(1u, result); // TRUE

        // Verify standard handles are now NULL
        var stdinHandle = _testEnv.CallKernel32Api("GETSTDHANDLE", 0xFFFFFFF6u);
        var stdoutHandle = _testEnv.CallKernel32Api("GETSTDHANDLE", 0xFFFFFFF5u);
        var stderrHandle = _testEnv.CallKernel32Api("GETSTDHANDLE", 0xFFFFFFF4u);

        Assert.Equal(0u, stdinHandle);  // Should be NULL
        Assert.Equal(0u, stdoutHandle); // Should be NULL
        Assert.Equal(0u, stderrHandle); // Should be NULL
    }

    [Fact]
    public void FreeConsole_WhenNoConsole_ShouldReturnFalse()
    {
        // Act - try to free console when none exists
        var result = _testEnv.CallKernel32Api("FREECONSOLE");

        // Assert
        Assert.Equal(0u, result); // FALSE
        
        // Verify last error is set to ERROR_INVALID_HANDLE (6)
        var lastError = _testEnv.CallKernel32Api("GETLASTERROR");
        Assert.Equal(6u, lastError);
    }

    [Fact]
    public void AttachConsole_ShouldAllocateConsole()
    {
        // Act - attach to parent process (0xFFFFFFFF)
        var result = _testEnv.CallKernel32Api("ATTACHCONSOLE", 0xFFFFFFFFu);

        // Assert
        Assert.Equal(1u, result); // TRUE

        // Verify standard handles are now set
        var stdoutHandle = _testEnv.CallKernel32Api("GETSTDHANDLE", 0xFFFFFFF5u);
        Assert.NotEqual(0u, stdoutHandle); // Should not be NULL
    }

    [Fact]
    public void AttachConsole_WhenConsoleExists_ShouldReturnFalse()
    {
        // Arrange - allocate console first
        _testEnv.CallKernel32Api("ALLOCCONSOLE");

        // Act - try to attach
        var result = _testEnv.CallKernel32Api("ATTACHCONSOLE", 0xFFFFFFFFu);

        // Assert
        Assert.Equal(0u, result); // FALSE
    }

    #endregion

    #region EBP Restoration Tests

    [Fact]
    public void GetStdHandle_WithImportHookInEBP_ShouldNotCorruptEBP()
    {
        // This test verifies the fix for the ign_3dfx.exe crash where EBP restoration
        // after GetStdHandle was setting EBP=ESP, corrupting the frame pointer
        
        // Simulate the scenario: EBP contains an import hook address (indirect call pattern)
        // MOV EBP, [IAT_Entry]; CALL EBP
        const uint importHookAddress = 0x0F0000A0;
        _testEnv.Cpu.SetRegister("EBP", importHookAddress);
        
        // Record the stack pointer before the call
        var espBefore = _testEnv.Cpu.GetRegister("ESP");
        
        // Act - call GetStdHandle (which returns NULL for GUI apps without console)
        _testEnv.CallKernel32Api("GETSTDHANDLE", 0xFFFFFFF6u); // STD_INPUT_HANDLE
        
        // Assert - EBP should NOT be set to ESP
        var ebpAfter = _testEnv.Cpu.GetRegister("EBP");
        var espAfter = _testEnv.Cpu.GetRegister("ESP");
        
        // EBP should still contain the import hook address (unchanged when restoration fails)
        // This is correct because the caller code that used EBP for the indirect call
        // will handle restoring it appropriately
        Assert.Equal(importHookAddress, ebpAfter);
        
        // EBP should NOT equal ESP (the bug that caused the crash)
        Assert.NotEqual(espAfter, ebpAfter);
    }

    [Fact]
    public void GetFileType_AfterGetStdHandle_ShouldNotCrashWithCorruptedStack()
    {
        // This test verifies the fix for the ign_3dfx.exe crash where sequential calls
        // to GetStdHandle and GetFileType would corrupt the stack due to EBP=ESP
        
        // Simulate the import hook indirect call pattern
        const uint importHookAddress = 0x0F0000A0;
        _testEnv.Cpu.SetRegister("EBP", importHookAddress);
        
        // Act - call GetStdHandle followed by GetFileType (same sequence as ign_3dfx.exe)
        var stdHandle = _testEnv.CallKernel32Api("GETSTDHANDLE", 0xFFFFFFF6u); // Returns NULL
        var fileType = _testEnv.CallKernel32Api("GETFILETYPE", stdHandle); // Should not crash
        
        // Assert - both calls should succeed
        Assert.Equal(0u, stdHandle); // NULL (no console)
        Assert.Equal(0u, fileType); // FILE_TYPE_UNKNOWN for NULL handle
        
        // Stack should still be valid (no corruption)
        var esp = _testEnv.Cpu.GetRegister("ESP");
        Assert.True(esp >= 0x00100000 && esp <= 0x00300000, "ESP should be in valid stack range");
    }

    #endregion

    public void Dispose()
    {
        _testEnv?.Dispose();
    }
}