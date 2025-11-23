using Avalonia.Headless.XUnit;
using Win32Emu.Gui.Controls;

namespace Win32Emu.Tests.Gui;

/// <summary>
/// Tests for TerminalControl
/// </summary>
public class TerminalControlTests
{
	[AvaloniaFact]
	public void TerminalControl_CanBeCreated()
	{
		// Act
		var terminal = new TerminalControl();

		// Assert
		Assert.NotNull(terminal);
	}

	[AvaloniaFact]
	public void Write_WithSimpleText_DoesNotThrow()
	{
		// Arrange
		var terminal = new TerminalControl();

		// Act & Assert
		terminal.Write("Hello");
	}

	[AvaloniaFact]
	public void Write_WithNewline_DoesNotThrow()
	{
		// Arrange
		var terminal = new TerminalControl();

		// Act & Assert
		terminal.Write("Hello\n");
		terminal.Write("World\n");
	}

	[AvaloniaFact]
	public void Write_WithCarriageReturnNewline_DoesNotThrow()
	{
		// Arrange
		var terminal = new TerminalControl();

		// Act & Assert
		terminal.Write("Hello\r\n");
		terminal.Write("World\r\n");
	}

	[AvaloniaFact]
	public void WriteLine_WithText_DoesNotThrow()
	{
		// Arrange
		var terminal = new TerminalControl();

		// Act & Assert
		terminal.WriteLine("Hello");
		terminal.WriteLine("World");
	}

	[AvaloniaFact]
	public void Write_WithMultipleLines_DoesNotThrow()
	{
		// Arrange
		var terminal = new TerminalControl();

		// Act & Assert
		terminal.Write("Line 1\nLine 2\nLine 3\n");
	}

	[AvaloniaFact]
	public void Write_WithMultipleLinesWindowsNewlines_DoesNotThrow()
	{
		// Arrange
		var terminal = new TerminalControl();

		// Act & Assert
		terminal.Write("Line 1\r\nLine 2\r\nLine 3\r\n");
	}

	[AvaloniaFact]
	public void Clear_AfterWrite_DoesNotThrow()
	{
		// Arrange
		var terminal = new TerminalControl();
		terminal.Write("Hello\n");

		// Act & Assert
		terminal.Clear();
	}

	[AvaloniaFact]
	public void SetCursorPosition_WithValidPosition_DoesNotThrow()
	{
		// Arrange
		var terminal = new TerminalControl();

		// Act & Assert
		terminal.SetCursorPosition(10, 5);
	}

	[AvaloniaFact]
	public void Write_WithTab_DoesNotThrow()
	{
		// Arrange
		var terminal = new TerminalControl();

		// Act & Assert
		terminal.Write("Hello\tWorld\n");
	}

	[AvaloniaFact]
	public void Write_WithBackspace_DoesNotThrow()
	{
		// Arrange
		var terminal = new TerminalControl();

		// Act & Assert
		terminal.Write("Hello\b\b");
	}

	[AvaloniaFact]
	public void Write_WithMixedControlCharacters_DoesNotThrow()
	{
		// Arrange
		var terminal = new TerminalControl();

		// Act & Assert
		terminal.Write("Test\r\nWith\tTabs\nAnd\rCarriage\nReturns");
	}
}
