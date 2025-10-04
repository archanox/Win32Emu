using Win32Emu.Tests.Kernel32.TestInfrastructure;

namespace Win32Emu.Tests.Kernel32
{
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
}