using Avalonia.Controls;
using Avalonia.Layout;

namespace Win32Emu.Gui.Services;

/// <summary>
/// Factory for creating Avalonia controls that correspond to Win32 standard controls
/// </summary>
public static class Win32ControlFactory
{
	/// <summary>
	/// Create an Avalonia control for a Win32 standard control class
	/// </summary>
	public static Control? CreateControl(string className, string windowName, uint style, int width, int height)
	{
		return className.ToUpperInvariant() switch
		{
			"BUTTON" => CreateButton(windowName, style, width, height),
			"EDIT" => CreateEdit(windowName, style, width, height),
			"STATIC" => CreateStatic(windowName, style, width, height),
			"LISTBOX" => CreateListBox(windowName, style, width, height),
			"COMBOBOX" => CreateComboBox(windowName, style, width, height),
			"SCROLLBAR" => CreateScrollBar(windowName, style, width, height),
			_ => null
		};
	}

	private static Button CreateButton(string text, uint style, int width, int height)
	{
		var button = new Button
		{
			Content = text,
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Top
		};

		if (width > 0) button.Width = width;
		if (height > 0) button.Height = height;

		return button;
	}

	private static TextBox CreateEdit(string text, uint style, int width, int height)
	{
		// Check for multiline flag (ES_MULTILINE = 0x0004)
		bool isMultiline = (style & 0x0004) != 0;
		
		var textBox = new TextBox
		{
			Text = text,
			AcceptsReturn = isMultiline,
			TextWrapping = isMultiline ? Avalonia.Media.TextWrapping.Wrap : Avalonia.Media.TextWrapping.NoWrap,
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Top
		};

		if (width > 0) textBox.Width = width;
		if (height > 0) textBox.Height = height;

		return textBox;
	}

	private static TextBlock CreateStatic(string text, uint style, int width, int height)
	{
		var textBlock = new TextBlock
		{
			Text = text,
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Top
		};

		if (width > 0) textBlock.Width = width;
		if (height > 0) textBlock.Height = height;

		return textBlock;
	}

	private static ListBox CreateListBox(string text, uint style, int width, int height)
	{
		var listBox = new ListBox
		{
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Top
		};

		if (width > 0) listBox.Width = width;
		if (height > 0) listBox.Height = height;

		return listBox;
	}

	private static ComboBox CreateComboBox(string text, uint style, int width, int height)
	{
		var comboBox = new ComboBox
		{
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Top
		};

		if (width > 0) comboBox.Width = width;
		if (height > 0) comboBox.Height = height;

		return comboBox;
	}

	private static Slider CreateScrollBar(string text, uint style, int width, int height)
	{
		// Check for horizontal flag (SBS_HORZ = 0x0000, SBS_VERT = 0x0001)
		bool isVertical = (style & 0x0001) != 0;
		
		var slider = new Slider
		{
			Orientation = isVertical ? Orientation.Vertical : Orientation.Horizontal,
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Top,
			Minimum = 0,
			Maximum = 100,
			Value = 0
		};

		if (width > 0) slider.Width = width;
		if (height > 0) slider.Height = height;

		return slider;
	}

	/// <summary>
	/// Check if a class name represents a standard Win32 control
	/// </summary>
	public static bool IsStandardControl(string className)
	{
		return className.ToUpperInvariant() switch
		{
			"BUTTON" or "EDIT" or "STATIC" or "LISTBOX" or "COMBOBOX" or "SCROLLBAR" => true,
			_ => false
		};
	}
}
