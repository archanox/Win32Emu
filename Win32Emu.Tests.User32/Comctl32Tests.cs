using Xunit;
using Win32Emu.Tests.Infrastructure;
using Win32Emu.Win32;

namespace Win32Emu.Tests.User32;

/// <summary>
/// Tests for COMCTL32.DLL functions
/// </summary>
[Trait("Category", "DllModuleTests")]
public sealed class Comctl32Tests : IDisposable
{
	private readonly TestEnvironment _testEnv;

	public Comctl32Tests()
	{
		_testEnv = new TestEnvironment();
	}

	[Fact]
	public void InitCommonControlsEx_WithValidStructure_ReturnsSuccess()
	{
		// Arrange
		var structAddr = 0x1000u;
		var dwSize = 8u;
		var dwICC = (uint)NativeTypes.IccFlags.ICC_WIN95_CLASSES;

		// Write INITCOMMONCONTROLSEX structure to memory
		_testEnv.Memory.Write32(structAddr + 0, dwSize);       // dwSize
		_testEnv.Memory.Write32(structAddr + 4, dwICC);        // dwICC

		// Act
		var result = _testEnv.CallComctl32Api("INITCOMMONCONTROLSEX", structAddr);

		// Assert
		Assert.Equal(1u, result); // TRUE - success
	}

	[Fact]
	public void InitCommonControlsEx_WithInvalidSize_ReturnsFalse()
	{
		// Arrange
		var structAddr = 0x1000u;
		var dwSize = 4u; // Invalid size - should be 8
		var dwICC = (uint)NativeTypes.IccFlags.ICC_LISTVIEW_CLASSES;

		// Write INITCOMMONCONTROLSEX structure to memory
		_testEnv.Memory.Write32(structAddr + 0, dwSize);       // dwSize (invalid)
		_testEnv.Memory.Write32(structAddr + 4, dwICC);        // dwICC

		// Act
		var result = _testEnv.CallComctl32Api("INITCOMMONCONTROLSEX", structAddr);

		// Assert
		Assert.Equal(0u, result); // FALSE - failure
		Assert.Equal((uint)NativeTypes.Win32Error.ERROR_INVALID_PARAMETER, _testEnv.ProcessEnv.LastError);
	}

	[Fact]
	public void InitCommonControlsEx_WithListViewClasses_ReturnsSuccess()
	{
		// Arrange
		var structAddr = 0x1000u;
		var dwSize = 8u;
		var dwICC = (uint)NativeTypes.IccFlags.ICC_LISTVIEW_CLASSES;

		// Write INITCOMMONCONTROLSEX structure to memory
		_testEnv.Memory.Write32(structAddr + 0, dwSize);
		_testEnv.Memory.Write32(structAddr + 4, dwICC);

		// Act
		var result = _testEnv.CallComctl32Api("INITCOMMONCONTROLSEX", structAddr);

		// Assert
		Assert.Equal(1u, result); // TRUE - success
	}

	[Fact]
	public void InitCommonControlsEx_WithMultipleClasses_ReturnsSuccess()
	{
		// Arrange
		var structAddr = 0x1000u;
		var dwSize = 8u;
		var dwICC = (uint)(NativeTypes.IccFlags.ICC_LISTVIEW_CLASSES | 
		                   NativeTypes.IccFlags.ICC_TREEVIEW_CLASSES | 
		                   NativeTypes.IccFlags.ICC_BAR_CLASSES);

		// Write INITCOMMONCONTROLSEX structure to memory
		_testEnv.Memory.Write32(structAddr + 0, dwSize);
		_testEnv.Memory.Write32(structAddr + 4, dwICC);

		// Act
		var result = _testEnv.CallComctl32Api("INITCOMMONCONTROLSEX", structAddr);

		// Assert
		Assert.Equal(1u, result); // TRUE - success
	}

	public void Dispose()
	{
		_testEnv.Dispose();
	}
}
