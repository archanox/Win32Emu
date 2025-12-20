using System.Runtime.InteropServices;
using Xunit;
using Win32Emu.Tests.Kernel32.TestInfrastructure;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests ported from ReactOS Kernel32 API test suite
/// Source: https://github.com/reactos/reactos/tree/master/modules/rostests/apitests/kernel32
/// Focus: String functions, file operations, memory management
/// </summary>
[Trait("Category", "DllModuleTests")]
[Trait("Source", "ReactOS")]
public class ReactOSPortedTests_Kernel32 : IDisposable
{
	private readonly TestEnvironment _testEnv;

	// Error constants
	private const uint ERROR_INVALID_PARAMETER = 87;
	private const uint ERROR_INSUFFICIENT_BUFFER = 122;
	private const uint ERROR_FILE_NOT_FOUND = 2;
	private const uint ERROR_PATH_NOT_FOUND = 3;
	private const uint ERROR_ACCESS_DENIED = 5;
	
	// Special value for null-terminated strings
	private const uint NULL_TERMINATED = unchecked((uint)-1);
	
	// Memory protection constants
	private const uint PAGE_NOACCESS = 0x01;
	private const uint PAGE_READONLY = 0x02;
	private const uint PAGE_READWRITE = 0x04;
	private const uint PAGE_EXECUTE = 0x10;
	private const uint PAGE_EXECUTE_READ = 0x20;
	private const uint PAGE_EXECUTE_READWRITE = 0x40;
	
	// Memory allocation constants
	private const uint MEM_COMMIT = 0x1000;
	private const uint MEM_RESERVE = 0x2000;
	private const uint MEM_RELEASE = 0x8000;
	private const uint MEM_FREE = 0x10000;

	public ReactOSPortedTests_Kernel32()
	{
		_testEnv = new TestEnvironment();
	}

	public void Dispose()
	{
		_testEnv.Dispose();
		GC.SuppressFinalize(this);
	}

	#region GetModuleFileName Tests
	// Ported from: rostests/apitests/kernel32/GetModuleFileName.c

	[Fact]
	public void GetModuleFileNameA_WithNullModule_ShouldReturnExePath()
	{
		// Arrange
		var bufferPtr = _testEnv.AllocateMemory(260); // MAX_PATH

		// Act - NULL module = current executable
		var result = _testEnv.CallKernel32Api("GETMODULEFILENAMEA", 0u, bufferPtr, 260u);

		// Assert
		Assert.True(result > 0, "Should return length of path");
		Assert.True(result < 260, "Should fit in MAX_PATH");

		// Read the path
		var path = _testEnv.ReadString(bufferPtr);
		Assert.NotNull(path);
		Assert.NotEmpty(path);
	}

	[Fact]
	public void GetModuleFileNameA_WithTooSmallBuffer_ShouldTruncate()
	{
		// Arrange
		var bufferPtr = _testEnv.AllocateMemory(10);

		// Act
		var result = _testEnv.CallKernel32Api("GETMODULEFILENAMEA", 0u, bufferPtr, 10u);

		// Assert
		Assert.True(result > 0, "Should return truncated length");
		Assert.True(result <= 10, "Should not exceed buffer size");
	}

	#endregion

	#region GetCommandLine Tests
	// Ported from: rostests/apitests/kernel32/GetCommandLine.c

	[Fact]
	public void GetCommandLineA_ShouldReturnValidPointer()
	{
		// Act
		var cmdLinePtr = _testEnv.CallKernel32Api("GETCOMMANDLINEA");

		// Assert
		Assert.NotEqual(0u, cmdLinePtr);

		// Read command line
		var cmdLine = _testEnv.ReadString(cmdLinePtr);
		Assert.NotNull(cmdLine);
	}

	[Fact]
	public void GetCommandLineW_ShouldReturnValidPointer()
	{
		// Act
		var cmdLinePtr = _testEnv.CallKernel32Api("GETCOMMANDLINEW");

		// Assert
		Assert.NotEqual(0u, cmdLinePtr);

		// Read Unicode command line
		var cmdLine = _testEnv.ReadString(cmdLinePtr);
		Assert.NotNull(cmdLine);
	}

	#endregion

	#region GetVersion Tests
	// Ported from: rostests/apitests/kernel32/GetVersion.c

	[Fact]
	public void GetVersion_ShouldReturnValidWindowsVersion()
	{
		// Act
		var version = _testEnv.CallKernel32Api("GETVERSION");

		// Assert
		var major = (version >> 24) & 0xFF;
		var minor = (version >> 16) & 0xFF;
		var build = version & 0xFFFF;

		Assert.True(major >= 4, $"Major version should be >= 4 (Win95/NT4), got {major}");
		Assert.True(major <= 10, $"Major version should be <= 10, got {major}");
		Assert.True(build > 0, "Build number should be positive");
	}

	#endregion

	#region LoadLibrary Tests
	// Ported from: rostests/apitests/kernel32/LoadLibrary.c

	[Fact]
	public void LoadLibraryA_WithKernel32_ShouldReturnHandle()
	{
		// Arrange
		var libNamePtr = _testEnv.WriteString("KERNEL32.DLL");

		// Act
		var hModule = _testEnv.CallKernel32Api("LOADLIBRARYA", libNamePtr);

		// Assert
		Assert.NotEqual(0u, hModule);
	}

	[Fact]
	public void LoadLibraryA_WithNonExistentDll_ShouldReturnNull()
	{
		// Arrange
		var libNamePtr = _testEnv.WriteString("NonExistentDll12345.dll");

		// Act
		_testEnv.CallKernel32Api("SETLASTERROR", 0);
		var hModule = _testEnv.CallKernel32Api("LOADLIBRARYA", libNamePtr);
		var lastError = _testEnv.CallKernel32Api("GETLASTERROR");

		// Assert
		Assert.Equal(0u, hModule);
		Assert.NotEqual(0u, lastError);
	}

	[Fact]
	public void GetProcAddress_WithValidFunction_ShouldReturnAddress()
	{
		// Arrange
		var libNamePtr = _testEnv.WriteString("KERNEL32.DLL");
		var hModule = _testEnv.CallKernel32Api("LOADLIBRARYA", libNamePtr);
		var funcNamePtr = _testEnv.WriteString("GetVersion");

		// Act
		var funcAddr = _testEnv.CallKernel32Api("GETPROCADDRESS", hModule, funcNamePtr);

		// Assert
		Assert.NotEqual(0u, funcAddr);
	}

	[Fact]
	public void GetProcAddress_WithInvalidFunction_ShouldReturnNull()
	{
		// Arrange
		var libNamePtr = _testEnv.WriteString("KERNEL32.DLL");
		var hModule = _testEnv.CallKernel32Api("LOADLIBRARYA", libNamePtr);
		var funcNamePtr = _testEnv.WriteString("NonExistentFunction12345");

		// Act
		_testEnv.CallKernel32Api("SETLASTERROR", 0);
		var funcAddr = _testEnv.CallKernel32Api("GETPROCADDRESS", hModule, funcNamePtr);
		var lastError = _testEnv.CallKernel32Api("GETLASTERROR");

		// Assert
		Assert.Equal(0u, funcAddr);
		Assert.NotEqual(0u, lastError);
	}

	#endregion

	#region VirtualAlloc Tests
	// Ported from: rostests/apitests/kernel32/VirtualAlloc.c

	[Fact]
	public void VirtualAlloc_WithCommit_ShouldReturnValidAddress()
	{
		// Act
		var addr = _testEnv.CallKernel32Api("VIRTUALALLOC", 0u, 0x1000u, MEM_COMMIT, PAGE_READWRITE);

		// Assert
		Assert.NotEqual(0u, addr);
		Assert.True((addr & 0xFFF) == 0, "Address should be page-aligned");

		// Cleanup
		_testEnv.CallKernel32Api("VIRTUALFREE", addr, 0u, MEM_RELEASE);
	}

	[Fact]
	public void VirtualAlloc_WithReserve_ShouldReturnValidAddress()
	{
		// Act
		var addr = _testEnv.CallKernel32Api("VIRTUALALLOC", 0u, 0x10000u, MEM_RESERVE, PAGE_NOACCESS);

		// Assert
		Assert.NotEqual(0u, addr);
		Assert.True((addr & 0xFFFF) == 0, "Address should be 64KB-aligned for reserve");

		// Cleanup
		_testEnv.CallKernel32Api("VIRTUALFREE", addr, 0u, MEM_RELEASE);
	}

	[Fact]
	public void VirtualFree_WithValidAddress_ShouldReturnTrue()
	{
		// Arrange
		var addr = _testEnv.CallKernel32Api("VIRTUALALLOC", 0u, 0x1000u, MEM_COMMIT, PAGE_READWRITE);

		// Act
		var result = _testEnv.CallKernel32Api("VIRTUALFREE", addr, 0u, MEM_RELEASE);

		// Assert
		Assert.NotEqual(0u, result); // TRUE
	}

	#endregion

	#region HeapAlloc Tests
	// Ported from: rostests/apitests/kernel32/Heap.c

	[Fact]
	public void HeapCreate_ShouldReturnValidHandle()
	{
		// Act
		var hHeap = _testEnv.CallKernel32Api("HEAPCREATE", 0u, 0x1000u, 0u);

		// Assert
		Assert.NotEqual(0u, hHeap);

		// Cleanup
		_testEnv.CallKernel32Api("HEAPDESTROY", hHeap);
	}

	[Fact]
	public void HeapAlloc_WithValidHeap_ShouldReturnValidPointer()
	{
		// Arrange
		var hHeap = _testEnv.CallKernel32Api("HEAPCREATE", 0u, 0x1000u, 0u);

		// Act
		var ptr = _testEnv.CallKernel32Api("HEAPALLOC", hHeap, 0u, 256u);

		// Assert
		Assert.NotEqual(0u, ptr);

		// Cleanup
		_testEnv.CallKernel32Api("HEAPFREE", hHeap, 0u, ptr);
		_testEnv.CallKernel32Api("HEAPDESTROY", hHeap);
	}

	[Fact]
	public void HeapFree_WithValidPointer_ShouldReturnTrue()
	{
		// Arrange
		var hHeap = _testEnv.CallKernel32Api("HEAPCREATE", 0u, 0x1000u, 0u);
		var ptr = _testEnv.CallKernel32Api("HEAPALLOC", hHeap, 0u, 256u);

		// Act
		var result = _testEnv.CallKernel32Api("HEAPFREE", hHeap, 0u, ptr);

		// Assert
		Assert.NotEqual(0u, result); // TRUE

		// Cleanup
		_testEnv.CallKernel32Api("HEAPDESTROY", hHeap);
	}

	#endregion

	#region WideCharToMultiByte Tests
	// Ported from: rostests/apitests/kernel32/WideCharToMultiByte.c

	[Fact]
	public void WideCharToMultiByte_WithValidString_ShouldConvert()
	{
		// Arrange - Write a Unicode string "Hello"
		var wideStrPtr = _testEnv.AllocateMemory(12);
		_testEnv.Memory.Write16(wideStrPtr + 0, (ushort)'H');
		_testEnv.Memory.Write16(wideStrPtr + 2, (ushort)'e');
		_testEnv.Memory.Write16(wideStrPtr + 4, (ushort)'l');
		_testEnv.Memory.Write16(wideStrPtr + 6, (ushort)'l');
		_testEnv.Memory.Write16(wideStrPtr + 8, (ushort)'o');
		_testEnv.Memory.Write16(wideStrPtr + 10, 0);

		var multiBytePtr = _testEnv.AllocateMemory(10);

		// Act - CP_ACP = 0
		var result = _testEnv.CallKernel32Api("WIDECHARTOMULTIBYTE",
			0u,              // CodePage (CP_ACP)
			0u,              // dwFlags
			wideStrPtr,      // lpWideCharStr
			NULL_TERMINATED, // cchWideChar (-1 = null-terminated)
			multiBytePtr,    // lpMultiByteStr
			10u,             // cbMultiByte
			0u,              // lpDefaultChar
			0u               // lpUsedDefaultChar
		);

		// Assert
		Assert.True(result > 0, "Should return number of bytes written");
		Assert.Equal(6u, result); // "Hello" + null terminator

		// Verify the converted string
		var converted = _testEnv.ReadString(multiBytePtr);
		Assert.Equal("Hello", converted);
	}

	[Fact]
	public void WideCharToMultiByte_WithNullBuffer_ShouldReturnRequiredSize()
	{
		// Arrange
		var wideStrPtr = _testEnv.AllocateMemory(12);
		_testEnv.Memory.Write16(wideStrPtr + 0, (ushort)'H');
		_testEnv.Memory.Write16(wideStrPtr + 2, (ushort)'i');
		_testEnv.Memory.Write16(wideStrPtr + 4, 0);

		// Act - Query required size with null buffer
		var result = _testEnv.CallKernel32Api("WIDECHARTOMULTIBYTE",
			0u,              // CP_ACP
			0u,              // dwFlags
			wideStrPtr,      // lpWideCharStr
			NULL_TERMINATED, // cchWideChar
			0u,              // lpMultiByteStr (NULL)
			0u,              // cbMultiByte (0)
			0u,              // lpDefaultChar
			0u               // lpUsedDefaultChar
		);

		// Assert
		Assert.Equal(3u, result); // "Hi" + null terminator
	}

	#endregion

	#region MultiByteToWideChar Tests
	// Ported from: rostests/apitests/kernel32/MultiByteToWideChar.c

	[Fact]
	public void MultiByteToWideChar_WithValidString_ShouldConvert()
	{
		// Arrange
		var multiBytePtr = _testEnv.WriteString("Hello");
		var wideStrPtr = _testEnv.AllocateMemory(20);

		// Act - CP_ACP = 0
		var result = _testEnv.CallKernel32Api("MULTIBYTETOWIDECHAR",
			0u,              // CodePage (CP_ACP)
			0u,              // dwFlags
			multiBytePtr,    // lpMultiByteStr
			NULL_TERMINATED, // cbMultiByte (-1 = null-terminated)
			wideStrPtr,      // lpWideCharStr
			10u              // cchWideChar
		);

		// Assert
		Assert.True(result > 0, "Should return number of wide characters written");
		Assert.Equal(6u, result); // "Hello" + null terminator

		// Verify the converted string
		var converted = _testEnv.ReadWideString(wideStrPtr);
		Assert.Equal("Hello", converted);
	}

	[Fact]
	public void MultiByteToWideChar_WithNullBuffer_ShouldReturnRequiredSize()
	{
		// Arrange
		var multiBytePtr = _testEnv.WriteString("Test");

		// Act - Query required size with null buffer
		var result = _testEnv.CallKernel32Api("MULTIBYTETOWIDECHAR",
			0u,              // CP_ACP
			0u,              // dwFlags
			multiBytePtr,    // lpMultiByteStr
			NULL_TERMINATED, // cbMultiByte
			0u,              // lpWideCharStr (NULL)
			0u               // cchWideChar (0)
		);

		// Assert
		Assert.Equal(5u, result); // "Test" + null terminator (in wide chars)
	}

	#endregion

	#region LCMapString Tests
	// Ported from: rostests/apitests/kernel32/LCMapString.c

	[Fact]
	public void LCMapStringA_Uppercase_ShouldConvertToUppercase()
	{
		// Arrange
		const uint LCMAP_UPPERCASE = 0x00000200;
		var sourcePtr = _testEnv.WriteString("hello");
		var destPtr = _testEnv.AllocateMemory(10);

		// Act - LOCALE_USER_DEFAULT = 0x0400
		var result = _testEnv.CallKernel32Api("LCMAPSTRINGA",
			0x0400u,         // Locale
			LCMAP_UPPERCASE, // dwMapFlags
			sourcePtr,       // lpSrcStr
			NULL_TERMINATED, // cchSrc (-1 = null-terminated)
			destPtr,         // lpDestStr
			10u              // cchDest
		);

		// Assert
		Assert.True(result > 0, "Should return number of characters written");

		var converted = _testEnv.ReadString(destPtr);
		Assert.Equal("HELLO", converted);
	}

	[Fact]
	public void LCMapStringA_Lowercase_ShouldConvertToLowercase()
	{
		// Arrange
		const uint LCMAP_LOWERCASE = 0x00000100;
		var sourcePtr = _testEnv.WriteString("WORLD");
		var destPtr = _testEnv.AllocateMemory(10);

		// Act
		var result = _testEnv.CallKernel32Api("LCMAPSTRINGA",
			0x0400u,         // Locale
			LCMAP_LOWERCASE, // dwMapFlags
			sourcePtr,       // lpSrcStr
			NULL_TERMINATED, // cchSrc
			destPtr,         // lpDestStr
			10u              // cchDest
		);

		// Assert
		Assert.True(result > 0);

		var converted = _testEnv.ReadString(destPtr);
		Assert.Equal("world", converted);
	}

	#endregion

	#region GetACP and GetOEMCP Tests
	// Ported from: rostests/apitests/kernel32/GetACP.c

	[Fact]
	public void GetACP_ShouldReturnValidCodePage()
	{
		// Act
		var codePage = _testEnv.CallKernel32Api("GETACP");

		// Assert
		Assert.True(codePage > 0, "Code page should be positive");
		// Common code pages: 1252 (Latin), 932 (Japanese), etc.
		Assert.True(codePage <= 65535, "Code page should be valid");
	}

	[Fact]
	public void GetOEMCP_ShouldReturnValidCodePage()
	{
		// Act
		var codePage = _testEnv.CallKernel32Api("GETOEMCP");

		// Assert
		Assert.True(codePage > 0);
		Assert.True(codePage <= 65535);
	}

	[Fact]
	public void GetCPInfo_WithValidCodePage_ShouldReturnTrue()
	{
		// Arrange
		var cpInfoPtr = _testEnv.AllocateMemory(20); // sizeof(CPINFO)

		// Act - Get info for current ACP
		var acp = _testEnv.CallKernel32Api("GETACP");
		var result = _testEnv.CallKernel32Api("GETCPINFO", acp, cpInfoPtr);

		// Assert
		Assert.NotEqual(0u, result); // TRUE

		// Read MaxCharSize (first field)
		var maxCharSize = _testEnv.Memory.Read32(cpInfoPtr);
		Assert.True(maxCharSize >= 1, "MaxCharSize should be at least 1");
		Assert.True(maxCharSize <= 4, "MaxCharSize should be at most 4");
	}

	#endregion

	#region GetStringType Tests
	// Ported from: rostests/apitests/kernel32/GetStringType.c

	[Fact]
	public void GetStringTypeA_WithDigit_ShouldReturnCType1Digit()
	{
		// Arrange
		const uint CT_CTYPE1 = 0x0001;
		const uint C1_DIGIT = 0x0004;
		var strPtr = _testEnv.WriteString("5");
		var typePtr = _testEnv.AllocateMemory(4);

		// Act
		var result = _testEnv.CallKernel32Api("GETSTRINGTYPEA",
			0x0400u,   // LOCALE_USER_DEFAULT
			CT_CTYPE1,
			strPtr,
			1u,        // cchSrc
			typePtr
		);

		// Assert
		Assert.NotEqual(0u, result); // TRUE

		var charType = _testEnv.Memory.Read16(typePtr);
		Assert.True((charType & C1_DIGIT) != 0, "Should have C1_DIGIT flag");
	}

	[Fact]
	public void GetStringTypeA_WithLetter_ShouldReturnCType1Alpha()
	{
		// Arrange
		const uint CT_CTYPE1 = 0x0001;
		const uint C1_ALPHA = 0x0100;
		var strPtr = _testEnv.WriteString("A");
		var typePtr = _testEnv.AllocateMemory(4);

		// Act
		var result = _testEnv.CallKernel32Api("GETSTRINGTYPEA",
			0x0400u, CT_CTYPE1, strPtr, 1u, typePtr
		);

		// Assert
		Assert.NotEqual(0u, result);

		var charType = _testEnv.Memory.Read16(typePtr);
		Assert.True((charType & C1_ALPHA) != 0, "Should have C1_ALPHA flag");
	}

	#endregion
}
