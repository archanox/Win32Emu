using System;
using System.Text;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.Win32;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for dialog template parsing and resource loading functionality.
/// </summary>
public sealed class DialogTemplateTests : IDisposable
{
	private readonly VirtualMemory _memory;

	public DialogTemplateTests()
	{
		_memory = new VirtualMemory();
	}

	[Fact]
	public void DialogTemplateParser_ParseStandardTemplate_ShouldParseBasicDialog()
	{
		// Arrange - Create a minimal standard DLGTEMPLATE in memory
		var address = 0x10000u;
		var offset = 0u;

		// DLGTEMPLATE structure
		_memory.Write32(address + offset, 0x80C80000); // style = WS_POPUP | WS_CAPTION | WS_SYSMENU
		offset += 4;
		_memory.Write32(address + offset, 0); // dwExtendedStyle
		offset += 4;
		_memory.Write16(address + offset, 2); // cdit = 2 items
		offset += 2;
		_memory.Write16(address + offset, 10); // x = 10
		offset += 2;
		_memory.Write16(address + offset, 10); // y = 10
		offset += 2;
		_memory.Write16(address + offset, 200); // cx = 200
		offset += 2;
		_memory.Write16(address + offset, 100); // cy = 100
		offset += 2;

		// Variable length fields (menu, class, title)
		_memory.Write16(address + offset, 0); // no menu
		offset += 2;
		_memory.Write16(address + offset, 0); // no custom class
		offset += 2;

		// Title string (Unicode)
		var title = "Test Dialog\0";
		foreach (var ch in title)
		{
			_memory.Write16(address + offset, ch);
			offset += 2;
		}

		// No font (DS_SETFONT not set)

		var parser = new DialogTemplateParser(_memory);

		// Act
		var template = parser.Parse(address);

		// Assert
		Assert.NotNull(template);
		Assert.False(template.IsExtended);
		Assert.Equal(2, template.ItemCount);
		Assert.Equal(10, template.X);
		Assert.Equal(10, template.Y);
		Assert.Equal(200, template.Width);
		Assert.Equal(100, template.Height);
		Assert.Equal("Test Dialog", template.Title);
	}

	[Fact]
	public void DialogTemplateParser_ParseExtendedTemplate_ShouldDetectExtendedSignature()
	{
		// Arrange - Create a minimal extended DLGTEMPLATEEX in memory
		var address = 0x10000u;
		var offset = 0u;

		// Extended signature
		_memory.Write16(address + offset, 0xFFFF); // signature
		offset += 2;
		_memory.Write16(address + offset, 0x0001); // version
		offset += 2;

		// DLGTEMPLATEEX structure
		_memory.Write32(address + offset, 0); // helpID
		offset += 4;
		_memory.Write32(address + offset, 0); // exStyle
		offset += 4;
		_memory.Write32(address + offset, 0x80C80000); // style
		offset += 4;
		_memory.Write16(address + offset, 0); // cDlgItems = 0
		offset += 2;
		_memory.Write16(address + offset, 0); // x = 0
		offset += 2;
		_memory.Write16(address + offset, 0); // y = 0
		offset += 2;
		_memory.Write16(address + offset, 100); // cx = 100
		offset += 2;
		_memory.Write16(address + offset, 50); // cy = 50
		offset += 2;

		// Variable length fields
		_memory.Write16(address + offset, 0); // no menu
		offset += 2;
		_memory.Write16(address + offset, 0); // no custom class
		offset += 2;
		_memory.Write16(address + offset, 0); // no title
		offset += 2;

		var parser = new DialogTemplateParser(_memory);

		// Act
		var template = parser.Parse(address);

		// Assert
		Assert.NotNull(template);
		Assert.True(template.IsExtended);
		Assert.Equal(0, template.ItemCount);
		Assert.Equal(100, template.Width);
		Assert.Equal(50, template.Height);
	}

	[Fact]
	public void DialogTemplateParser_ParseItemTemplate_ShouldParseButtonControl()
	{
		// Arrange - Create a dialog with one button item
		var address = 0x10000u;
		var offset = 0u;

		// Standard DLGTEMPLATE header
		_memory.Write32(address + offset, 0x80C80000); // style
		offset += 4;
		_memory.Write32(address + offset, 0); // dwExtendedStyle
		offset += 4;
		_memory.Write16(address + offset, 1); // cdit = 1 item
		offset += 2;
		_memory.Write16(address + offset, 0); // x
		offset += 2;
		_memory.Write16(address + offset, 0); // y
		offset += 2;
		_memory.Write16(address + offset, 100); // cx
		offset += 2;
		_memory.Write16(address + offset, 50); // cy
		offset += 2;

		// Variable fields
		_memory.Write16(address + offset, 0); // no menu
		offset += 2;
		_memory.Write16(address + offset, 0); // no custom class
		offset += 2;
		_memory.Write16(address + offset, 0); // no title
		offset += 2;

		// Align to DWORD
		offset = (offset + 3) & ~3u;

		// DLGITEMTEMPLATE for button
		_memory.Write32(address + offset, 0x50010000); // style = WS_VISIBLE | WS_CHILD | BS_PUSHBUTTON
		offset += 4;
		_memory.Write32(address + offset, 0); // dwExtendedStyle
		offset += 4;
		_memory.Write16(address + offset, 10); // x = 10
		offset += 2;
		_memory.Write16(address + offset, 10); // y = 10
		offset += 2;
		_memory.Write16(address + offset, 50); // cx = 50
		offset += 2;
		_memory.Write16(address + offset, 14); // cy = 14
		offset += 2;
		_memory.Write16(address + offset, 1); // id = IDOK
		offset += 2;

		// Window class - predefined button class (#80 = 0xFFFF, 0x0080)
		_memory.Write16(address + offset, 0xFFFF);
		offset += 2;
		_memory.Write16(address + offset, 0x0080);
		offset += 2;

		// Title string
		var title = "OK\0";
		foreach (var ch in title)
		{
			_memory.Write16(address + offset, ch);
			offset += 2;
		}

		// Creation data
		_memory.Write16(address + offset, 0); // no creation data
		offset += 2;

		var parser = new DialogTemplateParser(_memory);

		// Act
		var template = parser.Parse(address);

		// Assert
		Assert.NotNull(template);
		Assert.Equal(1, template.Items.Count);
		
		var item = template.Items[0];
		Assert.Equal(1, item.Id);
		Assert.Equal(10, item.X);
		Assert.Equal(10, item.Y);
		Assert.Equal(50, item.Width);
		Assert.Equal(14, item.Height);
		Assert.Equal("OK", item.Title);
		Assert.Equal("#128", item.WindowClass); // Ordinal 0x80 = 128
	}

	[Fact]
	public void DialogTemplateParser_AlignmentHandling_ShouldAlignToDword()
	{
		// This test verifies that the parser correctly handles DWORD alignment
		// between dialog template sections, which is crucial for proper parsing

		// Arrange - Create a template with non-aligned title
		var address = 0x10000u;
		var offset = 0u;

		// Header
		_memory.Write32(address + offset, 0x80C80000);
		offset += 4;
		_memory.Write32(address + offset, 0);
		offset += 4;
		_memory.Write16(address + offset, 0); // no items
		offset += 2;
		_memory.Write16(address + offset, 0);
		offset += 2;
		_memory.Write16(address + offset, 0);
		offset += 2;
		_memory.Write16(address + offset, 100);
		offset += 2;
		_memory.Write16(address + offset, 50);
		offset += 2;

		// Variable fields
		_memory.Write16(address + offset, 0);
		offset += 2;
		_memory.Write16(address + offset, 0);
		offset += 2;

		// Title with odd number of characters (forces alignment)
		var title = "Odd\0"; // 3 chars + null = 8 bytes (4 WCHARs), aligned
		foreach (var ch in title)
		{
			_memory.Write16(address + offset, ch);
			offset += 2;
		}

		var parser = new DialogTemplateParser(_memory);

		// Act - Should not throw
		var template = parser.Parse(address);

		// Assert
		Assert.NotNull(template);
		Assert.Equal("Odd", template.Title);
	}

	public void Dispose()
	{
		// Cleanup if needed
	}
}
