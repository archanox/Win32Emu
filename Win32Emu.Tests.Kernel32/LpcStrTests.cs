using Win32Emu.Memory;
using Win32Emu.Win32;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for LpcStr (Long Pointer to Const String) type
/// </summary>
public sealed class LpcStrTests : IDisposable
{
	private readonly VirtualMemory _memory;

	public LpcStrTests()
	{
		_memory = new VirtualMemory(1024 * 1024); // 1MB
	}

	[Fact]
	public void LpcStr_WithNullAddress_IsNull()
	{
		// Arrange
		var lpcStr = new LpcStr(0, _memory);

		// Assert
		Assert.True(lpcStr.IsNull);
		Assert.Equal(0u, lpcStr.Address);
	}

	[Fact]
	public void LpcStr_WithNonZeroAddress_IsNotNull()
	{
		// Arrange
		var lpcStr = new LpcStr(0x1000, _memory);

		// Assert
		Assert.False(lpcStr.IsNull);
		Assert.Equal(0x1000u, lpcStr.Address);
	}

	[Fact]
	public void LpcStr_Read_WithNullPointer_ReturnsNull()
	{
		// Arrange
		var lpcStr = new LpcStr(0, _memory);

		// Act
		var result = lpcStr.Read();

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void LpcStr_Read_WithValidString_ReturnsCorrectString()
	{
		// Arrange
		const uint address = 0x1000;
		const string expectedString = "Hello, World!";
		var bytes = System.Text.Encoding.ASCII.GetBytes(expectedString + "\0");
		_memory.WriteBytes(address, bytes);

		var lpcStr = new LpcStr(address, _memory);

		// Act
		var result = lpcStr.Read();

		// Assert
		Assert.NotNull(result);
		Assert.Equal(expectedString, result);
	}

	[Fact]
	public void LpcStr_Read_WithEmptyString_ReturnsEmptyString()
	{
		// Arrange
		const uint address = 0x1000;
		var bytes = new byte[] { 0 }; // Just null terminator
		_memory.WriteBytes(address, bytes);

		var lpcStr = new LpcStr(address, _memory);

		// Act
		var result = lpcStr.Read();

		// Assert
		Assert.NotNull(result);
		Assert.Equal(string.Empty, result);
	}

	[Fact]
	public void LpcStr_ToString_ReturnsStringValue()
	{
		// Arrange
		const uint address = 0x1000;
		const string expectedString = "Test String";
		var bytes = System.Text.Encoding.ASCII.GetBytes(expectedString + "\0");
		_memory.WriteBytes(address, bytes);

		var lpcStr = new LpcStr(address, _memory);

		// Act
		var result = lpcStr.ToString();

		// Assert
		Assert.NotNull(result);
		Assert.Equal(expectedString, result);
	}

	[Fact]
	public void LpcStr_ImplicitConversion_FromUint()
	{
		// Arrange
		const uint address = 0x2000;

		// Act
		LpcStr lpcStr = address;

		// Assert
		Assert.Equal(address, lpcStr.Address);
	}

	[Fact]
	public void LpcStr_ImplicitConversion_ToUint()
	{
		// Arrange
		var lpcStr = new LpcStr(0x3000);

		// Act
		uint address = lpcStr;

		// Assert
		Assert.Equal(0x3000u, address);
	}

	[Fact]
	public void LpcStr_Read_WithSpecialCharacters_HandlesCorrectly()
	{
		// Arrange
		const uint address = 0x1000;
		const string specialString = "Test\r\n\t!@#$%";
		var bytes = System.Text.Encoding.ASCII.GetBytes(specialString + "\0");
		_memory.WriteBytes(address, bytes);

		var lpcStr = new LpcStr(address, _memory);

		// Act
		var result = lpcStr.Read();

		// Assert
		Assert.NotNull(result);
		Assert.Equal(specialString, result);
	}

	[Fact]
	public void LpcStr_Read_StopsAtNullTerminator()
	{
		// Arrange
		const uint address = 0x1000;
		var bytes = System.Text.Encoding.ASCII.GetBytes("Hello\0World\0");
		_memory.WriteBytes(address, bytes);

		var lpcStr = new LpcStr(address, _memory);

		// Act
		var result = lpcStr.Read();

		// Assert
		Assert.NotNull(result);
		Assert.Equal("Hello", result);
	}

	[Fact]
	public void LpcStr_ToString_WithNullPointer_ReturnsNull()
	{
		// Arrange
		var lpcStr = new LpcStr(0, _memory);

		// Act
		var result = lpcStr.ToString();

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void LpcStr_WithoutMemory_ToString_ReturnsNull()
	{
		// Arrange
		const uint address = 0x1000;
		var lpcStr = new LpcStr(address); // No memory provided

		// Act
		var result = lpcStr.ToString();

		// Assert
		Assert.Null(result);
	}

	public void Dispose()
	{
		// Cleanup if needed
	}
}
