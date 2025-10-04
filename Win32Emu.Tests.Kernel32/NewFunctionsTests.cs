using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Win32Emu.Win32;

namespace Win32Emu.Tests.Kernel32
{
	/// <summary>
	/// Tests for newly implemented Kernel32 functions: VirtualFree, HeapDestroy, TerminateProcess, 
	/// GetProcAddress, MultiByteToWideChar, LCMapStringA, LCMapStringW, GetStringTypeW, RaiseException
	/// </summary>
	public class NewFunctionsTests : IDisposable
	{
		private readonly TestEnvironment _testEnv;

		public NewFunctionsTests()
		{
			_testEnv = new TestEnvironment();
		}

		#region VirtualFree Tests

		[Fact]
		public void VirtualFree_WithValidAddress_ShouldReturnTrue()
		{
			// Arrange - Allocate memory first
			const uint size = 4096;
			var address = _testEnv.CallKernel32Api("VIRTUALALLOC", 0, size, 0x1000, 0x04);
			Assert.NotEqual(0u, address);

			// Act - Free the memory with MEM_RELEASE
			var result = _testEnv.CallKernel32Api("VIRTUALFREE", address, 0, 0x8000);

			// Assert
			Assert.Equal(NativeTypes.Win32Bool.TRUE, result);
		}

		[Fact]
		public void VirtualFree_WithNullAddress_ShouldReturnFalse()
		{
			// Act
			var result = _testEnv.CallKernel32Api("VIRTUALFREE", 0, 0, 0x8000);

			// Assert
			Assert.Equal(NativeTypes.Win32Bool.FALSE, result);
		}

		#endregion

		#region HeapDestroy Tests

		[Fact]
		public void HeapDestroy_WithValidHeap_ShouldReturnTrue()
		{
			// Arrange - Create a heap first
			var heapHandle = _testEnv.CallKernel32Api("HEAPCREATE", 0, 4096, 0);
			Assert.NotEqual(0u, heapHandle);

			// Act
			var result = _testEnv.CallKernel32Api("HEAPDESTROY", heapHandle);

			// Assert
			Assert.Equal(NativeTypes.Win32Bool.TRUE, result);
		}

		[Fact]
		public void HeapDestroy_WithNullHandle_ShouldReturnFalse()
		{
			// Act
			var result = _testEnv.CallKernel32Api("HEAPDESTROY", 0);

			// Assert
			Assert.Equal(NativeTypes.Win32Bool.FALSE, result);
		}

		#endregion

		#region TerminateProcess Tests

		[Fact]
		public void TerminateProcess_WithCurrentProcessHandle_ShouldReturnTrue()
		{
			// Act - Use pseudo-handle for current process
			var result = _testEnv.CallKernel32Api("TERMINATEPROCESS", 0xFFFFFFFF, 0);

			// Assert - Should return TRUE (though process would exit in real scenario)
			Assert.Equal(NativeTypes.Win32Bool.TRUE, result);
		}

		#endregion

		#region GetProcAddress Tests

		[Fact]
		public void GetProcAddress_WithNonLoadedModule_ShouldReturnZero()
		{
			// Arrange - Get a module handle
			// Note: GetModuleHandleA returns imageBase, which is not a loaded PE image with exports
			var moduleHandle = _testEnv.CallKernel32Api("GETMODULEHANDLEA", 0);
			var procNamePtr = _testEnv.WriteString("GetVersion");

			// Act
			var result = _testEnv.CallKernel32Api("GETPROCADDRESS", moduleHandle, procNamePtr);

			// Assert - Returns 0 because the module handle doesn't correspond to a loaded PE image
			Assert.Equal(0u, result);
		}

		[Fact]
		public void GetProcAddress_WithNullModule_ShouldReturnZero()
		{
			// Arrange
			var procNamePtr = _testEnv.WriteString("SomeFunction");

			// Act
			var result = _testEnv.CallKernel32Api("GETPROCADDRESS", 0, procNamePtr);

			// Assert
			Assert.Equal(0u, result);
		}

		[Fact]
		public void GetProcAddress_ByOrdinal_WithNonLoadedModule_ShouldReturnZero()
		{
			// Arrange - Get a module handle  
			var moduleHandle = _testEnv.CallKernel32Api("GETMODULEHANDLEA", 0);
        
			// Act - Look up by ordinal (ordinal 1)
			var result = _testEnv.CallKernel32Api("GETPROCADDRESS", moduleHandle, 1);

			// Assert - Returns 0 because the module handle doesn't correspond to a loaded PE image
			Assert.Equal(0u, result);
		}

		#endregion

		#region MultiByteToWideChar Tests

		[Fact]
		public void MultiByteToWideChar_WithBufferSizeQuery_ShouldReturnRequiredSize()
		{
			// Arrange
			const string testString = "Hello";
			var multiBytePtr = _testEnv.WriteString(testString);
			const uint codePage = 1252;

			// Act - Query buffer size
			var result = _testEnv.CallKernel32Api("MULTIBYTETOWIDECHAR", 
				codePage, 0, multiBytePtr, (uint)testString.Length, 0, 0);

			// Assert
			Assert.Equal((uint)testString.Length, result);
		}

		[Fact]
		public void MultiByteToWideChar_WithValidBuffer_ShouldConvertString()
		{
			// Arrange
			const string testString = "Test";
			var multiBytePtr = _testEnv.WriteString(testString);
			var wideCharBuffer = _testEnv.AllocateMemory(20);
			const uint codePage = 1252;

			// Act
			var result = _testEnv.CallKernel32Api("MULTIBYTETOWIDECHAR", 
				codePage, 0, multiBytePtr, (uint)testString.Length, wideCharBuffer, 10);

			// Assert
			Assert.Equal((uint)testString.Length, result);
        
			// Verify the converted string
			for (var i = 0; i < testString.Length; i++)
			{
				var wideChar = _testEnv.Memory.Read16(wideCharBuffer + (uint)(i * 2));
				Assert.Equal(testString[i], wideChar);
			}
		}

		#endregion

		#region LCMapStringA Tests

		[Fact]
		public void LCMapStringA_Uppercase_ShouldConvertToUppercase()
		{
			// Arrange
			const string testString = "hello";
			var srcPtr = _testEnv.WriteString(testString);
			var destBuffer = _testEnv.AllocateMemory(20);
			const uint lcmapUppercase = 0x00000200;

			// Act
			var result = _testEnv.CallKernel32Api("LCMAPSTRINGA", 
				0, lcmapUppercase, srcPtr, unchecked((uint)-1), destBuffer, 20);

			// Assert
			Assert.True(result > 0);
			var resultString = _testEnv.ReadString(destBuffer);
			Assert.Equal("HELLO", resultString);
		}

		[Fact]
		public void LCMapStringA_Lowercase_ShouldConvertToLowercase()
		{
			// Arrange
			const string testString = "WORLD";
			var srcPtr = _testEnv.WriteString(testString);
			var destBuffer = _testEnv.AllocateMemory(20);
			const uint lcmapLowercase = 0x00000100;

			// Act
			var result = _testEnv.CallKernel32Api("LCMAPSTRINGA", 
				0, lcmapLowercase, srcPtr, unchecked((uint)-1), destBuffer, 20);

			// Assert
			Assert.True(result > 0);
			var resultString = _testEnv.ReadString(destBuffer);
			Assert.Equal("world", resultString);
		}

		#endregion

		#region LCMapStringW Tests

		[Fact]
		public void LCMapStringW_Uppercase_ShouldConvertToUppercase()
		{
			// Arrange
			const string testString = "hello";
			var srcPtr = WriteWideString(testString);
			var destBuffer = _testEnv.AllocateMemory(40);
			const uint lcmapUppercase = 0x00000200;

			// Act
			var result = _testEnv.CallKernel32Api("LCMAPSTRINGW", 
				0, lcmapUppercase, srcPtr, unchecked((uint)-1), destBuffer, 20);

			// Assert
			Assert.True(result > 0);
			var resultString = ReadWideString(destBuffer);
			Assert.Equal("HELLO", resultString);
		}

		#endregion

		#region GetStringTypeW Tests

		[Fact]
		public void GetStringTypeW_WithValidString_ShouldReturnTrue()
		{
			// Arrange
			const string testString = "Test123";
			var srcPtr = WriteWideString(testString);
			var charTypeBuffer = _testEnv.AllocateMemory(20);

			// Act - CT_CTYPE1 = 1
			var result = _testEnv.CallKernel32Api("GETSTRINGTYPEW", 
				0, 1, srcPtr, (uint)testString.Length, charTypeBuffer);

			// Assert
			Assert.Equal(NativeTypes.Win32Bool.TRUE, result);
		}

		#endregion

		#region RaiseException Tests

		[Fact]
		public void RaiseException_ShouldReturnZero()
		{
			// Act - Raise a test exception
			var result = _testEnv.CallKernel32Api("RAISEEXCEPTION", 0xE0000001, 0, 0, 0);

			// Assert - In our simple implementation, it returns 0
			Assert.Equal(0u, result);
		}

		#endregion

		public void Dispose()
		{
			_testEnv.Dispose();
		}

		// Helper methods for wide string operations
		private uint WriteWideString(string str)
		{
			var ptr = _testEnv.AllocateMemory((uint)((str.Length + 1) * 2));
			for (var i = 0; i < str.Length; i++)
			{
				_testEnv.Memory.Write16(ptr + (uint)(i * 2), str[i]);
			}
			_testEnv.Memory.Write16(ptr + (uint)(str.Length * 2), 0); // Null terminator
			return ptr;
		}

		private string ReadWideString(uint ptr)
		{
			var chars = new List<char>();
			uint offset = 0;
			while (true)
			{
				var wchar = _testEnv.Memory.Read16(ptr + offset);
				if (wchar == 0)
				{
					break;
				}

				chars.Add((char)wchar);
				offset += 2;
			}
			return new string(chars.ToArray());
		}
	}
}
