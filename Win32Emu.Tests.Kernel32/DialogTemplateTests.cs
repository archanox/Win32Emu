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
[Trait("Category", "DllModuleTests")]
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

	[Fact]
	public void DialogTemplateParser_CorruptedStringData_ShouldHandleGracefully()
	{
		// This test verifies that the parser can handle corrupted string data
		// without crashing or hanging in an infinite loop
		
		// Arrange - Create a template with a string that's missing a null terminator
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
		_memory.Write16(address + offset, 0); // no menu
		offset += 2;
		_memory.Write16(address + offset, 0); // no custom class
		offset += 2;

		// Title with many characters but NO null terminator
		// This simulates corrupted data that could cause infinite loop
		for (var i = 0; i < 100; i++)
		{
			_memory.Write16(address + offset, (ushort)('A' + (i % 26)));
			offset += 2;
		}
		// Intentionally NOT writing null terminator

		var parser = new DialogTemplateParser(_memory);

		// Act - Should not hang or crash
		var template = parser.Parse(address);

		// Assert - Should have parsed what it could
		Assert.NotNull(template);
		Assert.NotNull(template.Title);
		// The title should have captured something (either truncated or partial)
		Assert.True(template.Title.Length > 0);
	}

	[Fact]
	public void DialogTemplateParser_CorruptedItemData_ShouldParseAvailableItems()
	{
		// This test verifies that the parser stops gracefully when it encounters
		// corrupted item data instead of crashing
		
		// Arrange - Create a template claiming 3 items but only provide data for 2
		// Then make the 3rd item location point to invalid memory
		var address = 0x10000u;
		var offset = 0u;

		// Standard DLGTEMPLATE header
		_memory.Write32(address + offset, 0x80C80000);
		offset += 4;
		_memory.Write32(address + offset, 0);
		offset += 4;
		_memory.Write16(address + offset, 3); // cdit = 3 items (but we'll make the 3rd one fail)
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
		_memory.Write16(address + offset, 0); // no title
		offset += 2;

		// Align to DWORD
		offset = (offset + 3) & ~3u;

		// Create first item (valid)
		_memory.Write32(address + offset, 0x50010000);
		offset += 4;
		_memory.Write32(address + offset, 0);
		offset += 4;
		_memory.Write16(address + offset, 10);
		offset += 2;
		_memory.Write16(address + offset, 10);
		offset += 2;
		_memory.Write16(address + offset, 50);
		offset += 2;
		_memory.Write16(address + offset, 14);
		offset += 2;
		_memory.Write16(address + offset, 1);
		offset += 2;
		_memory.Write16(address + offset, 0xFFFF);
		offset += 2;
		_memory.Write16(address + offset, 0x0080);
		offset += 2;
		_memory.Write16(address + offset, 0); // empty title
		offset += 2;
		_memory.Write16(address + offset, 0); // no creation data
		offset += 2;

		// Align to DWORD
		offset = (offset + 3) & ~3u;

		// Create second item (valid)
		_memory.Write32(address + offset, 0x50010000);
		offset += 4;
		_memory.Write32(address + offset, 0);
		offset += 4;
		_memory.Write16(address + offset, 10);
		offset += 2;
		_memory.Write16(address + offset, 30);
		offset += 2;
		_memory.Write16(address + offset, 50);
		offset += 2;
		_memory.Write16(address + offset, 14);
		offset += 2;
		_memory.Write16(address + offset, 2);
		offset += 2;
		_memory.Write16(address + offset, 0xFFFF);
		offset += 2;
		_memory.Write16(address + offset, 0x0080);
		offset += 2;
		// For the title, write a string without null terminator and beyond valid memory
		// This will cause ReadString to hit the memory limit
		for (var i = 0; i < 8000; i++)
		{
			// Write non-zero values to prevent it from finding a null terminator
			_memory.Write16(address + offset, (ushort)('X'));
			offset += 2;
		}

		var parser = new DialogTemplateParser(_memory);

		// Act - Should parse 2 items and handle the 3rd gracefully
		var template = parser.Parse(address);

		// Assert
		Assert.NotNull(template);
		// Should have parsed 2 items successfully
		// The 3rd item should have been handled gracefully (either skipped or parsed partially)
		Assert.True(template.Items.Count >= 2, $"Expected at least 2 items, got {template.Items.Count}");
		Assert.Equal(1, template.Items[0].Id);
		Assert.Equal(2, template.Items[1].Id);
	}

	public void Dispose()
	{
		// VirtualMemory doesn't implement IDisposable, no cleanup needed
	}
}
