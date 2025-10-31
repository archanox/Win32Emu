using Avalonia.Input;
using Win32Emu.Gui.Utilities;

namespace Win32Emu.Tests.Gui;

/// <summary>
/// Tests for Win32InputMapper utility class
/// </summary>
public class Win32InputMapperTests
{
	[Theory]
	[InlineData(Key.A, 0x41)]
	[InlineData(Key.B, 0x42)]
	[InlineData(Key.Z, 0x5A)]
	[InlineData(Key.D0, 0x30)]
	[InlineData(Key.D9, 0x39)]
	public void MapKeyToVirtualKeyCode_WithLettersAndNumbers_ReturnsCorrectCode(Key key, byte expectedVK)
	{
		// Act
		var result = Win32InputMapper.MapKeyToVirtualKeyCode(key);

		// Assert
		Assert.Equal(expectedVK, result);
	}

	[Theory]
	[InlineData(Key.F1, 0x70)]
	[InlineData(Key.F12, 0x7B)]
	[InlineData(Key.Enter, 0x0D)]
	[InlineData(Key.Escape, 0x1B)]
	[InlineData(Key.Space, 0x20)]
	[InlineData(Key.Back, 0x08)]
	[InlineData(Key.Tab, 0x09)]
	public void MapKeyToVirtualKeyCode_WithSpecialKeys_ReturnsCorrectCode(Key key, byte expectedVK)
	{
		// Act
		var result = Win32InputMapper.MapKeyToVirtualKeyCode(key);

		// Assert
		Assert.Equal(expectedVK, result);
	}

	[Theory]
	[InlineData(Key.Left, 0x25)]
	[InlineData(Key.Up, 0x26)]
	[InlineData(Key.Right, 0x27)]
	[InlineData(Key.Down, 0x28)]
	public void MapKeyToVirtualKeyCode_WithArrowKeys_ReturnsCorrectCode(Key key, byte expectedVK)
	{
		// Act
		var result = Win32InputMapper.MapKeyToVirtualKeyCode(key);

		// Assert
		Assert.Equal(expectedVK, result);
	}

	[Theory]
	[InlineData(Key.LeftShift, 0xA0)]
	[InlineData(Key.RightShift, 0xA1)]
	[InlineData(Key.LeftCtrl, 0xA2)]
	[InlineData(Key.RightCtrl, 0xA3)]
	[InlineData(Key.LeftAlt, 0xA4)]
	[InlineData(Key.RightAlt, 0xA5)]
	public void MapKeyToVirtualKeyCode_WithModifierKeys_ReturnsCorrectCode(Key key, byte expectedVK)
	{
		// Act
		var result = Win32InputMapper.MapKeyToVirtualKeyCode(key);

		// Assert
		Assert.Equal(expectedVK, result);
	}

	[Theory]
	[InlineData(Key.NumPad0, 0x60)]
	[InlineData(Key.NumPad9, 0x69)]
	[InlineData(Key.Multiply, 0x6A)]
	[InlineData(Key.Add, 0x6B)]
	[InlineData(Key.Subtract, 0x6D)]
	[InlineData(Key.Divide, 0x6F)]
	public void MapKeyToVirtualKeyCode_WithNumpadKeys_ReturnsCorrectCode(Key key, byte expectedVK)
	{
		// Act
		var result = Win32InputMapper.MapKeyToVirtualKeyCode(key);

		// Assert
		Assert.Equal(expectedVK, result);
	}

	[Fact]
	public void GetKeyModifiers_WithNoModifiers_ReturnsZero()
	{
		// Act
		var result = Win32InputMapper.GetKeyModifiers(KeyModifiers.None);

		// Assert
		Assert.Equal(0u, result);
	}

	[Fact]
	public void GetKeyModifiers_WithShift_ReturnsShiftFlag()
	{
		// Act
		var result = Win32InputMapper.GetKeyModifiers(KeyModifiers.Shift);

		// Assert
		Assert.Equal(0x0004u, result); // MK_SHIFT
	}

	[Fact]
	public void GetKeyModifiers_WithControl_ReturnsControlFlag()
	{
		// Act
		var result = Win32InputMapper.GetKeyModifiers(KeyModifiers.Control);

		// Assert
		Assert.Equal(0x0008u, result); // MK_CONTROL
	}

	[Fact]
	public void GetKeyModifiers_WithShiftAndControl_ReturnsCombinedFlags()
	{
		// Act
		var result = Win32InputMapper.GetKeyModifiers(KeyModifiers.Shift | KeyModifiers.Control);

		// Assert
		Assert.Equal(0x000Cu, result); // MK_SHIFT | MK_CONTROL
	}

	[Theory]
	[InlineData(0, 0, 0x00000000u)]
	[InlineData(100, 200, 0x00C80064u)] // HIWORD=200, LOWORD=100
	[InlineData(50, 75, 0x004B0032u)]   // HIWORD=75, LOWORD=50
	[InlineData(-10, -20, 0xFFECFFF6u)] // Test negative coordinates
	public void MakeMouseLParam_WithCoordinates_ReturnsCorrectLParam(double x, double y, uint expectedLParam)
	{
		// Act
		var result = Win32InputMapper.MakeMouseLParam(x, y);

		// Assert
		Assert.Equal(expectedLParam, result);
	}

	[Fact]
	public void MakeMouseLParam_WithLargeCoordinates_ClampsToValidRange()
	{
		// Arrange - coordinates beyond short.MaxValue
		double x = 50000;
		double y = 60000;

		// Act
		var result = Win32InputMapper.MakeMouseLParam(x, y);

		// Assert - should be clamped to short.MaxValue
		short xPos = (short)(result & 0xFFFF);
		short yPos = (short)((result >> 16) & 0xFFFF);
		
		Assert.Equal(short.MaxValue, xPos);
		Assert.Equal(short.MaxValue, yPos);
	}
}
