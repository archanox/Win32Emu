using Xunit;
using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Win32Emu.Win32.Modules;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for SHLWAPI.DLL functions
/// </summary>
[Trait("Category", "DllModuleTests")]
public sealed class ShlwapiTests : IDisposable
{
	private readonly TestEnvironment _testEnv;
	private readonly ShlwapiModule _shlwapi;

	public ShlwapiTests()
	{
		_testEnv = new TestEnvironment();
		_shlwapi = new ShlwapiModule(_testEnv.ProcessEnv, 0x00400000, _testEnv.PeLoader, NullLogger.Instance);
		_testEnv.Dispatcher.RegisterModule(_shlwapi);
	}

	[Fact]
	public void PathRemoveFileSpecA_WithValidPath_ShouldRemoveFilename()
	{
		// Arrange
		const string testPath = "C:\\Windows\\System32\\notepad.exe";
		var pathAddr = WriteAnsiString(testPath);

		// Act
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, pathAddr);
		var result = _shlwapi.TryInvokeUnsafe("PathRemoveFileSpecA", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(result);
		Assert.Equal(1u, returnValue); // TRUE - something was removed
		
		// Verify the path was modified correctly
		var modifiedPath = ReadAnsiString(pathAddr);
		Assert.Equal("C:\\Windows\\System32", modifiedPath);
	}

	[Fact]
	public void PathRemoveFileSpecA_WithPathEndingInBackslash_ShouldRemoveBackslash()
	{
		// Arrange
		const string testPath = "C:\\Windows\\System32\\";
		var pathAddr = WriteAnsiString(testPath);

		// Act
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, pathAddr);
		var result = _shlwapi.TryInvokeUnsafe("PathRemoveFileSpecA", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(result);
		Assert.Equal(1u, returnValue); // TRUE - something was removed
		
		// The function removes the trailing backslash
		var modifiedPath = ReadAnsiString(pathAddr);
		Assert.Equal("C:\\Windows\\System32", modifiedPath);
	}

	[Fact]
	public void PathRemoveFileSpecA_WithNoBackslash_ShouldNotModify()
	{
		// Arrange
		const string testPath = "notepad.exe";
		var pathAddr = WriteAnsiString(testPath);

		// Act
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, pathAddr);
		var result = _shlwapi.TryInvokeUnsafe("PathRemoveFileSpecA", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(result);
		Assert.Equal(0u, returnValue); // FALSE - nothing was removed
		
		var modifiedPath = ReadAnsiString(pathAddr);
		Assert.Equal("notepad.exe", modifiedPath);
	}

	[Fact]
	public void PathRemoveFileSpecA_WithNullPointer_ShouldReturnFalse()
	{
		// Act
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, 0u); // NULL pointer
		var result = _shlwapi.TryInvokeUnsafe("PathRemoveFileSpecA", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(result);
		Assert.Equal(0u, returnValue); // FALSE
	}

	private uint WriteAnsiString(string str)
	{
		var bytes = Encoding.ASCII.GetBytes(str + "\0");
		var addr = _testEnv.ProcessEnv.SimpleAlloc((uint)bytes.Length);
		for (int i = 0; i < bytes.Length; i++)
		{
			_testEnv.Memory.Write8(addr + (uint)i, bytes[i]);
		}
		return addr;
	}

	private string ReadAnsiString(uint addr)
	{
		var bytes = new List<byte>();
		uint offset = 0;
		byte b;
		while ((b = _testEnv.Memory.Read8(addr + offset)) != 0)
		{
			bytes.Add(b);
			offset++;
		}
		return Encoding.ASCII.GetString(bytes.ToArray());
	}

	public void Dispose()
	{
		_testEnv.Dispose();
	}
}
