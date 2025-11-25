using Xunit;
using Win32Emu.Tests.User32.TestInfrastructure;
using Win32Emu.Win32;

namespace Win32Emu.Tests.User32;

/// <summary>
/// Tests for newly implemented User32 functions: GetWindowWord, SetWindowWord, OemToCharA, OemToCharBuffA
/// </summary>
[Trait("Category", "DllModuleTests")]
public class NewUser32FunctionsTests : IDisposable
{
	private readonly TestEnvironment _testEnv;

	public NewUser32FunctionsTests()
	{
		_testEnv = new TestEnvironment();
	}

	#region GetWindowWord / SetWindowWord Tests

	[Fact]
	public void SetWindowWord_ShouldStoreValue()
	{
		// Arrange - Create a window
		var wndClassAddr = _testEnv.WriteWndClassA(
			className: "TestClass",
			wndProc: 0x00401000
		);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

		var classNamePtr = _testEnv.WriteString("TestClass");
		var titlePtr = _testEnv.WriteString("Test Window");
		
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
			0, classNamePtr, titlePtr, (uint)NativeTypes.WindowStyle.WS_OVERLAPPED,
			100, 100, 640, 480, 0, 0, 0, 0
		);

		// Act - Set user data (GWL_USERDATA = -21) with a 16-bit value
		var previousValue = _testEnv.CallUser32Api("SETWINDOWWORD", hwnd, unchecked((uint)-21), 0x5678u);

		// Assert
		Assert.Equal(0u, previousValue); // Should return 0 (no previous value)
	}

	[Fact]
	public void GetWindowWord_ShouldRetrieveValue()
	{
		// Arrange - Create a window and set a 16-bit value
		var wndClassAddr = _testEnv.WriteWndClassA(
			className: "TestClass",
			wndProc: 0x00401000
		);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

		var classNamePtr = _testEnv.WriteString("TestClass");
		var titlePtr = _testEnv.WriteString("Test Window");
		
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
			0, classNamePtr, titlePtr, (uint)NativeTypes.WindowStyle.WS_OVERLAPPED,
			100, 100, 640, 480, 0, 0, 0, 0
		);

		_testEnv.CallUser32Api("SETWINDOWWORD", hwnd, unchecked((uint)-21), 0x5678u);

		// Act - Get user data (GWL_USERDATA = -21)
		var value = _testEnv.CallUser32Api("GETWINDOWWORD", hwnd, unchecked((uint)-21));

		// Assert
		Assert.Equal(0x5678u, value);
	}

	[Fact]
	public void SetWindowWord_ShouldReturnPreviousValue()
	{
		// Arrange
		var wndClassAddr = _testEnv.WriteWndClassA(
			className: "TestClass",
			wndProc: 0x00401000
		);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

		var classNamePtr = _testEnv.WriteString("TestClass");
		var titlePtr = _testEnv.WriteString("Test Window");
		
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
			0, classNamePtr, titlePtr, (uint)NativeTypes.WindowStyle.WS_OVERLAPPED,
			100, 100, 640, 480, 0, 0, 0, 0
		);

		// Set initial value
		_testEnv.CallUser32Api("SETWINDOWWORD", hwnd, unchecked((uint)-21), 0x1111u);

		// Act - Set a new value
		var previousValue = _testEnv.CallUser32Api("SETWINDOWWORD", hwnd, unchecked((uint)-21), 0x2222u);

		// Assert
		Assert.Equal(0x1111u, previousValue);
		
		// Verify new value is set
		var currentValue = _testEnv.CallUser32Api("GETWINDOWWORD", hwnd, unchecked((uint)-21));
		Assert.Equal(0x2222u, currentValue);
	}

	[Fact]
	public void GetWindowWord_ShouldReturnOnly16Bits()
	{
		// Arrange - Create a window and set a 32-bit value using SetWindowLongA
		var wndClassAddr = _testEnv.WriteWndClassA(
			className: "TestClass",
			wndProc: 0x00401000
		);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

		var classNamePtr = _testEnv.WriteString("TestClass");
		var titlePtr = _testEnv.WriteString("Test Window");
		
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
			0, classNamePtr, titlePtr, (uint)NativeTypes.WindowStyle.WS_OVERLAPPED,
			100, 100, 640, 480, 0, 0, 0, 0
		);

		// Set a 32-bit value using SetWindowLongA
		_testEnv.CallUser32Api("SETWINDOWLONGA", hwnd, unchecked((uint)-21), 0x12345678u);

		// Act - Get using GetWindowWord (should return only lower 16 bits)
		var value = _testEnv.CallUser32Api("GETWINDOWWORD", hwnd, unchecked((uint)-21));

		// Assert - Should return only lower 16 bits (0x5678)
		Assert.Equal(0x5678u, value);
	}

	[Fact]
	public void SetWindowWord_ShouldPreserveUpper16Bits()
	{
		// Arrange - Create a window and set a 32-bit value
		var wndClassAddr = _testEnv.WriteWndClassA(
			className: "TestClass",
			wndProc: 0x00401000
		);
		_testEnv.CallUser32Api("REGISTERCLASSA", wndClassAddr);

		var classNamePtr = _testEnv.WriteString("TestClass");
		var titlePtr = _testEnv.WriteString("Test Window");
		
		var hwnd = _testEnv.CallUser32Api("CREATEWINDOWEXA",
			0, classNamePtr, titlePtr, (uint)NativeTypes.WindowStyle.WS_OVERLAPPED,
			100, 100, 640, 480, 0, 0, 0, 0
		);

		// Set a 32-bit value using SetWindowLongA
		_testEnv.CallUser32Api("SETWINDOWLONGA", hwnd, unchecked((uint)-21), 0x12345678u);

		// Act - Set lower 16 bits using SetWindowWord
		_testEnv.CallUser32Api("SETWINDOWWORD", hwnd, unchecked((uint)-21), 0xABCDu);

		// Assert - Upper 16 bits should be preserved
		var fullValue = _testEnv.CallUser32Api("GETWINDOWLONGA", hwnd, unchecked((uint)-21));
		Assert.Equal(0x1234ABCDu, fullValue);
	}

	#endregion

	#region OemToCharA / OemToCharBuffA Tests

	[Fact]
	public void OemToCharA_WithValidStrings_ShouldCopyString()
	{
		// Arrange
		var sourcePtr = _testEnv.WriteString("Hello World");
		var destPtr = _testEnv.AllocateMemory(50);

		// Act
		var result = _testEnv.CallUser32Api("OEMTOCHARA", sourcePtr, destPtr);

		// Assert
		Assert.Equal(1u, result); // TRUE
		var destString = _testEnv.ReadString(destPtr);
		Assert.Equal("Hello World", destString);
	}

	[Fact]
	public void OemToCharA_WithEmptyString_ShouldSucceed()
	{
		// Arrange
		var sourcePtr = _testEnv.WriteString("");
		var destPtr = _testEnv.AllocateMemory(50);

		// Act
		var result = _testEnv.CallUser32Api("OEMTOCHARA", sourcePtr, destPtr);

		// Assert
		Assert.Equal(1u, result); // TRUE
	}

	[Fact]
	public void OemToCharA_WithSpecialCharacters_ShouldCopyAsIs()
	{
		// Arrange
		var sourcePtr = _testEnv.WriteString("Test@#$%123");
		var destPtr = _testEnv.AllocateMemory(50);

		// Act
		var result = _testEnv.CallUser32Api("OEMTOCHARA", sourcePtr, destPtr);

		// Assert
		Assert.Equal(1u, result); // TRUE
		var destString = _testEnv.ReadString(destPtr);
		Assert.Equal("Test@#$%123", destString);
	}

	[Fact]
	public void OemToCharBuffA_WithValidBuffer_ShouldCopyString()
	{
		// Arrange
		var sourcePtr = _testEnv.WriteString("Hello World");
		var destPtr = _testEnv.AllocateMemory(50);
		var bufferSize = 12u; // Length of "Hello World" + null terminator

		// Act
		var result = _testEnv.CallUser32Api("OEMTOCHARBUFFA", sourcePtr, destPtr, bufferSize);

		// Assert
		Assert.Equal(1u, result); // TRUE
		var destString = _testEnv.ReadString(destPtr);
		Assert.Equal("Hello World", destString);
	}

	[Fact]
	public void OemToCharBuffA_WithZeroLength_ShouldSucceed()
	{
		// Arrange
		var sourcePtr = _testEnv.WriteString("Hello");
		var destPtr = _testEnv.AllocateMemory(50);

		// Act
		var result = _testEnv.CallUser32Api("OEMTOCHARBUFFA", sourcePtr, destPtr, 0u);

		// Assert
		Assert.Equal(1u, result); // TRUE - should succeed but not copy anything
	}

	[Fact]
	public void OemToCharBuffA_WithLimitedLength_ShouldTruncate()
	{
		// Arrange
		var sourcePtr = _testEnv.WriteString("Hello World");
		var destPtr = _testEnv.AllocateMemory(50);
		var bufferSize = 5u; // Copy only "Hello" (5 chars, no null from source)

		// Act
		var result = _testEnv.CallUser32Api("OEMTOCHARBUFFA", sourcePtr, destPtr, bufferSize);

		// Assert
		Assert.Equal(1u, result); // TRUE
		var destString = _testEnv.ReadString(destPtr);
		Assert.Equal("Hello", destString);
	}

	[Fact]
	public void OemToCharBuffA_WithExactLength_ShouldCopyWithoutNull()
	{
		// Arrange - String is "Hello" (5 chars)
		var sourcePtr = _testEnv.WriteString("Hello");
		var destPtr = _testEnv.AllocateMemory(50);
		var bufferSize = 5u; // Exact length without null terminator

		// Act
		var result = _testEnv.CallUser32Api("OEMTOCHARBUFFA", sourcePtr, destPtr, bufferSize);

		// Assert
		Assert.Equal(1u, result); // TRUE
		// Note: When buffer size equals string length, no null terminator is added
		// So we read the exact bytes that were written
		var bytes = new byte[5];
		_testEnv.Memory.ReadBytes(destPtr, bytes);
		var destString = System.Text.Encoding.ASCII.GetString(bytes);
		Assert.Equal("Hello", destString);
	}

	#endregion

	public void Dispose()
	{
		_testEnv.Dispose();
	}
}
