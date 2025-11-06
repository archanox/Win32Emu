using System;
using System.Collections.Generic;
using System.Text;
using Win32Emu.Memory;

namespace Win32Emu.Win32;

/// <summary>
/// Parses Win32 dialog templates (DLGTEMPLATE and DLGTEMPLATE_EX structures).
/// Handles both standard and extended dialog templates with proper alignment.
/// </summary>
public class DialogTemplateParser
{
	private readonly VirtualMemory _memory;

	public DialogTemplateParser(VirtualMemory memory)
	{
		_memory = memory ?? throw new ArgumentNullException(nameof(memory));
	}

	/// <summary>
	/// Parses a dialog template from memory.
	/// </summary>
	/// <param name="templateAddress">Address of the dialog template in memory</param>
	/// <returns>Parsed dialog template information</returns>
	public DialogTemplate Parse(uint templateAddress)
	{
		var offset = 0u;

		// Read first two WORDs to determine if this is extended or standard
		var word1 = _memory.Read16(templateAddress);
		offset += 2;
		var word2 = _memory.Read16(templateAddress + offset);
		offset += 2;

		var isExtended = (word1 == 0xFFFF && word2 == 0x0001);

		if (isExtended)
		{
			return ParseExtendedTemplate(templateAddress, ref offset);
		}

		// Reset offset since these were style fields, not signature
		offset = 0;
		return ParseStandardTemplate(templateAddress, ref offset);
	}

	private DialogTemplate ParseStandardTemplate(uint templateAddress, ref uint offset)
	{
		var template = new DialogTemplate
		{
			IsExtended = false,
			// DLGTEMPLATE structure:
			// DWORD style;
			// DWORD dwExtendedStyle;
			// WORD cdit;
			// short x;
			// short y;
			// short cx;
			// short cy;
			Style = _memory.Read32(templateAddress + offset)
		};

		offset += 4;
		template.ExtendedStyle = _memory.Read32(templateAddress + offset);
		offset += 4;
		template.ItemCount = _memory.Read16(templateAddress + offset);
		offset += 2;
		template.X = (short)_memory.Read16(templateAddress + offset);
		offset += 2;
		template.Y = (short)_memory.Read16(templateAddress + offset);
		offset += 2;
		template.Width = (short)_memory.Read16(templateAddress + offset);
		offset += 2;
		template.Height = (short)_memory.Read16(templateAddress + offset);
		offset += 2;

		// Variable length fields: menu, windowClass, title
		template.Menu = ReadNameOrOrdinal(templateAddress, ref offset);
		template.WindowClass = ReadNameOrOrdinal(templateAddress, ref offset);
		template.Title = ReadString(templateAddress, ref offset);

		// If DS_SETFONT is set, read font info
		const uint DS_SETFONT = 0x40;
		if ((template.Style & DS_SETFONT) != 0)
		{
			template.FontSize = _memory.Read16(templateAddress + offset);
			offset += 2;
			template.FontName = ReadString(templateAddress, ref offset);
		}

		// Align to DWORD boundary before reading items
		offset = AlignToDword(offset);

		// Parse dialog items
		template.Items = new List<DialogItem>();
		for (var i = 0; i < template.ItemCount; i++)
		{
			var item = ParseStandardItem(templateAddress, ref offset);
			template.Items.Add(item);
			// Align to DWORD boundary before next item
			offset = AlignToDword(offset);
		}

		return template;
	}

	private DialogTemplate ParseExtendedTemplate(uint templateAddress, ref uint offset)
	{
		var template = new DialogTemplate
		{
			IsExtended = true,
			// DLGTEMPLATEEX structure (after signature):
			// WORD dlgVer; (already read as word2 = 0x0001)
			// WORD signature; (already read as word1 = 0xFFFF)
			// DWORD helpID;
			// DWORD exStyle;
			// DWORD style;
			// WORD cDlgItems;
			// short x;
			// short y;
			// short cx;
			// short cy;
			HelpId = _memory.Read32(templateAddress + offset)
		};

		offset += 4;
		template.ExtendedStyle = _memory.Read32(templateAddress + offset);
		offset += 4;
		template.Style = _memory.Read32(templateAddress + offset);
		offset += 4;
		template.ItemCount = _memory.Read16(templateAddress + offset);
		offset += 2;
		template.X = (short)_memory.Read16(templateAddress + offset);
		offset += 2;
		template.Y = (short)_memory.Read16(templateAddress + offset);
		offset += 2;
		template.Width = (short)_memory.Read16(templateAddress + offset);
		offset += 2;
		template.Height = (short)_memory.Read16(templateAddress + offset);
		offset += 2;

		// Variable length fields
		template.Menu = ReadNameOrOrdinal(templateAddress, ref offset);
		template.WindowClass = ReadNameOrOrdinal(templateAddress, ref offset);
		template.Title = ReadString(templateAddress, ref offset);

		// If DS_SETFONT is set, read extended font info
		const uint DS_SETFONT = 0x40;
		if ((template.Style & DS_SETFONT) != 0)
		{
			template.FontSize = _memory.Read16(templateAddress + offset);
			offset += 2;
			template.FontWeight = _memory.Read16(templateAddress + offset);
			offset += 2;
			template.FontItalic = _memory.Read8(templateAddress + offset);
			offset += 1;
			template.FontCharset = _memory.Read8(templateAddress + offset);
			offset += 1;
			template.FontName = ReadString(templateAddress, ref offset);
		}

		// Align to DWORD boundary before reading items
		offset = AlignToDword(offset);

		// Parse dialog items
		template.Items = new List<DialogItem>();
		for (var i = 0; i < template.ItemCount; i++)
		{
			var item = ParseExtendedItem(templateAddress, ref offset);
			template.Items.Add(item);
			// Align to DWORD boundary before next item
			offset = AlignToDword(offset);
		}

		return template;
	}

	private DialogItem ParseStandardItem(uint templateAddress, ref uint offset)
	{
		var item = new DialogItem
		{
			IsExtended = false,
			// DLGITEMTEMPLATE structure:
			// DWORD style;
			// DWORD dwExtendedStyle;
			// short x;
			// short y;
			// short cx;
			// short cy;
			// WORD id;
			Style = _memory.Read32(templateAddress + offset)
		};

		offset += 4;
		item.ExtendedStyle = _memory.Read32(templateAddress + offset);
		offset += 4;
		item.X = (short)_memory.Read16(templateAddress + offset);
		offset += 2;
		item.Y = (short)_memory.Read16(templateAddress + offset);
		offset += 2;
		item.Width = (short)_memory.Read16(templateAddress + offset);
		offset += 2;
		item.Height = (short)_memory.Read16(templateAddress + offset);
		offset += 2;
		item.Id = _memory.Read16(templateAddress + offset);
		offset += 2;

		// Variable length fields: windowClass, title
		item.WindowClass = ReadNameOrOrdinal(templateAddress, ref offset);
		item.Title = ReadNameOrOrdinal(templateAddress, ref offset);

		// Creation data
		var dataSize = _memory.Read16(templateAddress + offset);
		offset += 2;
		if (dataSize > 0)
		{
			item.CreationData = new byte[dataSize];
			for (var i = 0; i < dataSize; i++)
			{
				item.CreationData[i] = _memory.Read8(templateAddress + offset);
				offset++;
			}
		}

		return item;
	}

	private DialogItem ParseExtendedItem(uint templateAddress, ref uint offset)
	{
		var item = new DialogItem
		{
			IsExtended = true,
			// DLGITEMTEMPLATEEX structure:
			// DWORD helpID;
			// DWORD exStyle;
			// DWORD style;
			// short x;
			// short y;
			// short cx;
			// short cy;
			// DWORD id;
			HelpId = _memory.Read32(templateAddress + offset)
		};

		offset += 4;
		item.ExtendedStyle = _memory.Read32(templateAddress + offset);
		offset += 4;
		item.Style = _memory.Read32(templateAddress + offset);
		offset += 4;
		item.X = (short)_memory.Read16(templateAddress + offset);
		offset += 2;
		item.Y = (short)_memory.Read16(templateAddress + offset);
		offset += 2;
		item.Width = (short)_memory.Read16(templateAddress + offset);
		offset += 2;
		item.Height = (short)_memory.Read16(templateAddress + offset);
		offset += 2;
		item.Id = (ushort)_memory.Read32(templateAddress + offset);
		offset += 4;

		// Variable length fields: windowClass, title
		item.WindowClass = ReadNameOrOrdinal(templateAddress, ref offset);
		item.Title = ReadNameOrOrdinal(templateAddress, ref offset);

		// Creation data
		var dataSize = _memory.Read16(templateAddress + offset);
		offset += 2;
		if (dataSize > 0)
		{
			item.CreationData = new byte[dataSize];
			for (var i = 0; i < dataSize; i++)
			{
				item.CreationData[i] = _memory.Read8(templateAddress + offset);
				offset++;
			}
		}

		return item;
	}

	private string ReadNameOrOrdinal(uint templateAddress, ref uint offset)
	{
		var firstWord = _memory.Read16(templateAddress + offset);

		switch (firstWord)
		{
			case 0x0000:
				// Empty string
				offset += 2;
				return string.Empty;
			case 0xFFFF:
			{
				// Ordinal value follows
				offset += 2;
				var ordinal = _memory.Read16(templateAddress + offset);
				offset += 2;
				return $"#{ordinal}";
			}
			default:
				// Unicode string (null-terminated)
				return ReadString(templateAddress, ref offset);
		}
	}

	private string ReadString(uint templateAddress, ref uint offset)
	{
		var sb = new StringBuilder();
		while (true)
		{
			var wchar = _memory.Read16(templateAddress + offset);
			offset += 2;
			if (wchar == 0)
			{
				break;
			}
			sb.Append((char)wchar);
		}
		return sb.ToString();
	}

	private uint AlignToDword(uint offset)
	{
		return (offset + 3) & ~3u;
	}
}