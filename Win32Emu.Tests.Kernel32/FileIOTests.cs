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
        Assert.Equal(0x00000001u, handle); // Default stdin handle
    }

    [Fact]
    public void GetStdHandle_StdOutput_ShouldReturnOutputHandle()
    {
        // Arrange
        const uint stdOutputHandle = 0xFFFFFFF5; // STD_OUTPUT_HANDLE

        // Act
        var handle = _testEnv.CallKernel32Api("GETSTDHANDLE", stdOutputHandle);

        // Assert
        Assert.Equal(0x00000002u, handle); // Default stdout handle
    }

    [Fact]
    public void GetStdHandle_StdError_ShouldReturnErrorHandle()
    {
        // Arrange
        const uint stdErrorHandle = 0xFFFFFFF4; // STD_ERROR_HANDLE

        // Act
        var handle = _testEnv.CallKernel32Api("GETSTDHANDLE", stdErrorHandle);

        // Assert
        Assert.Equal(0x00000003u, handle); // Default stderr handle
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
    public void GetStartupInfoA_ShouldReturnPseudoHandlesInStartupInfo()
    {
        // Arrange
        // Allocate memory for STARTUPINFO structure (68 bytes)
        var startupInfoPtr = _testEnv.AllocateMemory(68);

        // Act
        _testEnv.CallKernel32Api("GETSTARTUPINFOA", startupInfoPtr);

        // Assert
        // STARTUPINFO structure offsets:
        // +0: cb (size) - should be 68
        // +56: hStdInput - should be STD_INPUT_HANDLE pseudo-handle (0xFFFFFFF6)
        // +60: hStdOutput - should be STD_OUTPUT_HANDLE pseudo-handle (0xFFFFFFF5)
        // +64: hStdError - should be STD_ERROR_HANDLE pseudo-handle (0xFFFFFFF4)
        
        var cb = _testEnv.Memory.Read32(startupInfoPtr);
        var hStdInput = _testEnv.Memory.Read32(startupInfoPtr + 56);
        var hStdOutput = _testEnv.Memory.Read32(startupInfoPtr + 60);
        var hStdError = _testEnv.Memory.Read32(startupInfoPtr + 64);

        Assert.Equal(68u, cb);
        Assert.Equal(0xFFFFFFF6u, hStdInput); // STD_INPUT_HANDLE
        Assert.Equal(0xFFFFFFF5u, hStdOutput); // STD_OUTPUT_HANDLE
        Assert.Equal(0xFFFFFFF4u, hStdError); // STD_ERROR_HANDLE
    }

    [Fact]
    public void GetStartupInfoA_ThenGetStdHandle_ShouldWorkCorrectly()
    {
        // This test simulates the correct program behavior:
        // 1. Call GetStartupInfoA to get startup info
        // 2. Read the hStdOutput field (which contains a pseudo-handle)
        // 3. Call GetStdHandle with the pseudo-handle to get the real handle
        // 4. Use the real handle with WriteFile
        
        // Arrange
        var startupInfoPtr = _testEnv.AllocateMemory(68);
        
        // Act
        // Step 1: Get startup info
        _testEnv.CallKernel32Api("GETSTARTUPINFOA", startupInfoPtr);
        
        // Step 2: Read the hStdOutput field (offset 60)
        var pseudoHandle = _testEnv.Memory.Read32(startupInfoPtr + 60);
        
        // Verify it's the pseudo-handle constant
        Assert.Equal(0xFFFFFFF5u, pseudoHandle);
        
        // Step 3: Call GetStdHandle to get the real handle
        var realHandle = _testEnv.CallKernel32Api("GETSTDHANDLE", pseudoHandle);
        
        // Verify we got the real stdout handle
        Assert.Equal(0x00000002u, realHandle);
        
        // Step 4: Verify the real handle can be used with WriteFile
        var buffer = _testEnv.WriteString("test");
        var bytesWrittenPtr = _testEnv.AllocateMemory(4);
        
        var result = _testEnv.CallKernel32Api("WRITEFILE", realHandle, buffer, 4u, bytesWrittenPtr, 0u);
        
        // WriteFile should succeed
        Assert.Equal(1u, result);
    }

    #endregion

    #region WriteFile Tests

    [Fact]
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
    
    [Fact]
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
    
    [Fact]
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

    [Fact]
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

    public void Dispose()
    {
        _testEnv?.Dispose();
    }
}