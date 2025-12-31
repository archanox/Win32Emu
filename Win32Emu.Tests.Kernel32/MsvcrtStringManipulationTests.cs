using Win32Emu.Win32.Modules;
using Win32Emu.Tests.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for MSVCRT string manipulation functions (_strlwr, _strupr, _strset, etc.)
/// </summary>
[Trait("Category", "DllModuleTests")]
public sealed class MsvcrtStringManipulationTests : IDisposable
{
	private readonly TestEnvironment _testEnv;
	private readonly MsvcrtModule _msvcrt;

	public MsvcrtStringManipulationTests()
	{
		_testEnv = new TestEnvironment();
		_msvcrt = new MsvcrtModule(_testEnv.ProcessEnv, 0x00400000, _testEnv.PeLoader, NullLogger.Instance);
	}

	public void Dispose()
	{
		_testEnv?.Dispose();
	}

	[Fact]
	public void Strlwr_ConvertsToLowercase()
	{
		// Arrange
		var testStr = "Hello WORLD 123";
		var addr = 0x00100000u;
		var bytes = System.Text.Encoding.ASCII.GetBytes(testStr + "\0");
		_testEnv.Memory.WriteBytes(addr, bytes);
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, addr);
		
		// Act
		var success = _msvcrt.TryInvokeUnsafe("_STRLWR", _testEnv.Cpu, _testEnv.Memory, out var result);
		
		// Assert
		Assert.True(success);
		Assert.Equal(addr, result);
		var resultStr = _testEnv.ProcessEnv.ReadAnsiString(addr);
		Assert.Equal("hello world 123", resultStr);
	}

	[Fact]
	public void Strupr_ConvertsToUppercase()
	{
		// Arrange
		var testStr = "Hello world 123";
		var addr = 0x00100000u;
		var bytes = System.Text.Encoding.ASCII.GetBytes(testStr + "\0");
		_testEnv.Memory.WriteBytes(addr, bytes);
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, addr);
		
		// Act
		var success = _msvcrt.TryInvokeUnsafe("_STRUPR", _testEnv.Cpu, _testEnv.Memory, out var result);
		
		// Assert
		Assert.True(success);
		Assert.Equal(addr, result);
		var resultStr = _testEnv.ProcessEnv.ReadAnsiString(addr);
		Assert.Equal("HELLO WORLD 123", resultStr);
	}

	[Fact]
	public void Strset_SetsAllCharacters()
	{
		// Arrange
		var testStr = "Hello";
		var addr = 0x00100000u;
		var bytes = System.Text.Encoding.ASCII.GetBytes(testStr + "\0");
		_testEnv.Memory.WriteBytes(addr, bytes);
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, addr, (int)'X');
		
		// Act
		var success = _msvcrt.TryInvokeUnsafe("_STRSET", _testEnv.Cpu, _testEnv.Memory, out var result);
		
		// Assert
		Assert.True(success);
		Assert.Equal(addr, result);
		var resultStr = _testEnv.ProcessEnv.ReadAnsiString(addr);
		Assert.Equal("XXXXX", resultStr);
	}

	[Fact]
	public void Strnset_SetsFirstNCharacters()
	{
		// Arrange
		var testStr = "HelloWorld";
		var addr = 0x00100000u;
		var bytes = System.Text.Encoding.ASCII.GetBytes(testStr + "\0");
		_testEnv.Memory.WriteBytes(addr, bytes);
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, addr, (int)'X', 5u);
		
		// Act
		var success = _msvcrt.TryInvokeUnsafe("_STRNSET", _testEnv.Cpu, _testEnv.Memory, out var result);
		
		// Assert
		Assert.True(success);
		Assert.Equal(addr, result);
		var resultStr = _testEnv.ProcessEnv.ReadAnsiString(addr);
		Assert.Equal("XXXXXWorld", resultStr);
	}

	[Fact]
	public void Ltoa_ConvertsDecimal()
	{
		// Arrange
		var addr = 0x00100000u;
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, unchecked((uint)-12345), addr, 10u);
		
		// Act
		var success = _msvcrt.TryInvokeUnsafe("_LTOA", _testEnv.Cpu, _testEnv.Memory, out var result);
		
		// Assert
		Assert.True(success);
		Assert.Equal(addr, result);
		var resultStr = _testEnv.ProcessEnv.ReadAnsiString(addr);
		Assert.Equal("-12345", resultStr);
	}

	[Fact]
	public void Ltoa_ConvertsHex()
	{
		// Arrange
		var addr = 0x00100000u;
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, 255u, addr, 16u);
		
		// Act
		var success = _msvcrt.TryInvokeUnsafe("_LTOA", _testEnv.Cpu, _testEnv.Memory, out var result);
		
		// Assert
		Assert.True(success);
		Assert.Equal(addr, result);
		var resultStr = _testEnv.ProcessEnv.ReadAnsiString(addr);
		Assert.Equal("ff", resultStr);
	}

	[Fact]
	public void Ultoa_ConvertsUnsignedDecimal()
	{
		// Arrange
		var addr = 0x00100000u;
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, 4294967295u, addr, 10);
		
		// Act
		var success = _msvcrt.TryInvokeUnsafe("_ULTOA", _testEnv.Cpu, _testEnv.Memory, out var result);
		
		// Assert
		Assert.True(success);
		Assert.Equal(addr, result);
		var resultStr = _testEnv.ProcessEnv.ReadAnsiString(addr);
		Assert.Equal("4294967295", resultStr);
	}

	[Fact]
	public void Wcslwr_ConvertsWideStringToLowercase()
	{
		// Arrange
		var testStr = "Hello WORLD";
		var addr = 0x00100000u;
		var bytes = System.Text.Encoding.Unicode.GetBytes(testStr + "\0");
		_testEnv.Memory.WriteBytes(addr, bytes);
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, addr);
		
		// Act
		var success = _msvcrt.TryInvokeUnsafe("_WCSLWR", _testEnv.Cpu, _testEnv.Memory, out var result);
		
		// Assert
		Assert.True(success);
		Assert.Equal(addr, result);
		var resultStr = _testEnv.ProcessEnv.ReadUnicodeString(addr);
		Assert.Equal("hello world", resultStr);
	}

	[Fact]
	public void Wcsupr_ConvertsWideStringToUppercase()
	{
		// Arrange
		var testStr = "Hello world";
		var addr = 0x00100000u;
		var bytes = System.Text.Encoding.Unicode.GetBytes(testStr + "\0");
		_testEnv.Memory.WriteBytes(addr, bytes);
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, addr);
		
		// Act
		var success = _msvcrt.TryInvokeUnsafe("_WCSUPR", _testEnv.Cpu, _testEnv.Memory, out var result);
		
		// Assert
		Assert.True(success);
		Assert.Equal(addr, result);
		var resultStr = _testEnv.ProcessEnv.ReadUnicodeString(addr);
		Assert.Equal("HELLO WORLD", resultStr);
	}

	[Fact]
	public void Strtok_TokenizesString()
	{
		// Arrange - create a string to tokenize
		var testStr = "Hello,World;Test";
		var addr = 0x00100000u;
		var bytes = System.Text.Encoding.ASCII.GetBytes(testStr + "\0");
		_testEnv.Memory.WriteBytes(addr, bytes);
		
		var delimAddr = 0x00200000u;
		var delimBytes = System.Text.Encoding.ASCII.GetBytes(",;\0");
		_testEnv.Memory.WriteBytes(delimAddr, delimBytes);
		
		// Act - first call with string pointer
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, addr, delimAddr);
		var success1 = _msvcrt.TryInvokeUnsafe("STRTOK", _testEnv.Cpu, _testEnv.Memory, out var token1);
		
		// Act - second call with NULL to continue
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, 0u, delimAddr);
		var success2 = _msvcrt.TryInvokeUnsafe("STRTOK", _testEnv.Cpu, _testEnv.Memory, out var token2);
		
		// Act - third call with NULL to continue
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, 0u, delimAddr);
		var success3 = _msvcrt.TryInvokeUnsafe("STRTOK", _testEnv.Cpu, _testEnv.Memory, out var token3);
		
		// Assert
		Assert.True(success1);
		Assert.NotEqual(0u, token1);
		var str1 = _testEnv.ProcessEnv.ReadAnsiString(token1);
		Assert.Equal("Hello", str1);
		
		Assert.True(success2);
		Assert.NotEqual(0u, token2);
		var str2 = _testEnv.ProcessEnv.ReadAnsiString(token2);
		Assert.Equal("World", str2);
		
		Assert.True(success3);
		Assert.NotEqual(0u, token3);
		var str3 = _testEnv.ProcessEnv.ReadAnsiString(token3);
		Assert.Equal("Test", str3);
	}

	[Fact]
	public void Swab_SwapsBytes()
	{
		// Arrange
		var srcAddr = 0x00100000u;
		var dstAddr = 0x00200000u;
		var bytes = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 };
		_testEnv.Memory.WriteBytes(srcAddr, bytes);
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, srcAddr, dstAddr, 6u);
		
		// Act
		var success = _msvcrt.TryInvokeUnsafe("_SWAB", _testEnv.Cpu, _testEnv.Memory, out var result);
		
		// Assert
		Assert.True(success);
		var resultBytes = new byte[6];
		for (int i = 0; i < 6; i++)
		{
			resultBytes[i] = _testEnv.Memory.Read8(dstAddr + (uint)i);
		}
		Assert.Equal(new byte[] { 0x02, 0x01, 0x04, 0x03, 0x06, 0x05 }, resultBytes);
	}
}
