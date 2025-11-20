using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Win32Emu.Win32;

namespace Win32Emu.Gui.Views;

/// <summary>
/// Avalonia window that displays Win32 dialog boxes by converting dialog templates
/// into Avalonia controls.
/// </summary>
public partial class DialogWindow : Window
{
	private readonly DialogTemplate _template;
	private readonly Dictionary<ushort, Control> _controlsById = new();
	private readonly Dictionary<int, uint> _controlHandles;
	private readonly TaskCompletionSource<int> _resultTcs = new();
	private readonly Action<uint, uint, uint, uint>? _messageCallback;
	private readonly Action<string, DebugLevel>? _debugCallback;
	private readonly uint _dialogHandle;

	public int DialogResult { get; private set; }

	public DialogWindow(DialogTemplate template, uint dialogHandle = 0, Dictionary<int, uint>? controlHandles = null, Action<uint, uint, uint, uint>? messageCallback = null, Action<string, DebugLevel>? debugCallback = null)
	{
		_template = template ?? throw new ArgumentNullException(nameof(template));
		_dialogHandle = dialogHandle;
		_controlHandles = controlHandles ?? new Dictionary<int, uint>();
		_messageCallback = messageCallback;
		_debugCallback = debugCallback;
		
		// Log initialization for debugging
		_debugCallback?.Invoke($"[DialogWindow] Constructor: dialogHandle=0x{dialogHandle:X8}, controlHandles count={_controlHandles.Count}, messageCallback={(messageCallback != null ? "set" : "NULL")}, debugCallback={(debugCallback != null ? "set" : "NULL")}", DebugLevel.Info);
		
		InitializeComponent();
		BuildDialogContent();
	}

	/// <summary>
	/// Shows the dialog modally and returns the dialog result.
	/// </summary>
	public new async Task<int> ShowDialog(Window? owner)
	{
		if (owner != null)
		{
			await Dispatcher.UIThread.InvokeAsync(async () =>
			{
				await ShowDialog<int>(owner);
			});
		}
		else
		{
			await Dispatcher.UIThread.InvokeAsync(Show);
		}

		return await _resultTcs.Task;
	}

	/// <summary>
	/// Ends the dialog with the specified result.
	/// </summary>
	public void EndDialog(int result)
	{
		DialogResult = result;
		_resultTcs.TrySetResult(result);
		
		Dispatcher.UIThread.Post(() =>
		{
			Close();
		});
	}

	private void BuildDialogContent()
	{
		// Set window title
		Title = _template.Title;

		// Convert dialog units to pixels (approximate: 1 DLU ≈ 2 pixels for width, 1.5 for height)
		// Add some padding for the window chrome and margins
		var contentWidth = _template.Width * 2;
		var contentHeight = _template.Height * 1.5;
		Width = Math.Max(200, contentWidth + 20);  // Add 20px for window padding
		Height = Math.Max(150, contentHeight + 20);  // Add 20px for window padding

		// Get the content panel
		var contentPanel = this.FindControl<Panel>("DialogContentPanel");
		if (contentPanel == null)
		{
			return;
		}

		// Create a canvas for absolute positioning of controls with margins
		var canvas = new Canvas
		{
			Width = contentWidth,
			Height = contentHeight,
			Margin = new Thickness(10)  // Add 10px margin around the canvas
		};

		// Create controls from template items
		foreach (var item in _template.Items)
		{
			var control = CreateControlFromItem(item);
			if (control != null)
			{
				// Position the control
				Canvas.SetLeft(control, item.X * 2);
				Canvas.SetTop(control, item.Y * 1.5);
				control.Width = item.Width * 2;
				
				// For TextBlock controls with text wrapping, use MinHeight instead of fixed Height
				// to allow the control to expand for multi-line text
				if (control is TextBlock textBlock && textBlock.TextWrapping == TextWrapping.Wrap)
				{
					control.MinHeight = item.Height * 1.5;
				}
				else
				{
					control.Height = item.Height * 1.5;
				}

				canvas.Children.Add(control);
				_controlsById[item.Id] = control;
			}
		}

		contentPanel.Children.Add(canvas);
	}

	private Control? CreateControlFromItem(DialogItem item)
	{
		// Determine control type from window class
		var className = item.WindowClass.ToUpperInvariant();
		
		// Standard Win32 control classes (ordinals are in decimal)
		// #128 = 0x80 = BUTTON
		// #129 = 0x81 = EDIT
		// #130 = 0x82 = STATIC
		// #131 = 0x83 = LISTBOX
		// #132 = 0x84 = SCROLLBAR
		// #133 = 0x85 = COMBOBOX
		if (className == "BUTTON" || className == "#128")
		{
			return CreateButton(item);
		}
		else if (className == "STATIC" || className == "#130")
		{
			return CreateStatic(item);
		}
		else if (className == "EDIT" || className == "#129")
		{
			return CreateEdit(item);
		}
		else if (className == "LISTBOX" || className == "#131")
		{
			return CreateListBox(item);
		}
		else if (className == "COMBOBOX" || className == "#133")
		{
			return CreateComboBox(item);
		}
		else if (className == "SCROLLBAR" || className == "#132")
		{
			return CreateScrollBar(item);
		}
		else
		{
			// Unknown control type - create a placeholder
			return CreatePlaceholder(item);
		}
	}

	private Control CreateButton(DialogItem item)
	{
		const uint BS_PUSHBUTTON = 0x00000000;
		const uint BS_DEFPUSHBUTTON = 0x00000001;
		const uint BS_CHECKBOX = 0x00000002;
		const uint BS_AUTOCHECKBOX = 0x00000003;
		const uint BS_RADIOBUTTON = 0x00000004;
		const uint BS_AUTORADIOBUTTON = 0x00000009;
		const uint BS_GROUPBOX = 0x00000007;
		const uint WS_DISABLED = 0x08000000;

		var buttonStyle = item.Style & 0x0F;
		var isDisabled = (item.Style & WS_DISABLED) != 0;

		if (buttonStyle == BS_CHECKBOX || buttonStyle == BS_AUTOCHECKBOX)
		{
			var checkbox = new CheckBox
			{
				Content = ProcessAccessKeys(item.Title),
				Tag = item.Id,
				IsEnabled = !isDisabled
			};
			checkbox.Click += OnControlClick;
			return checkbox;
		}
		else if (buttonStyle == BS_RADIOBUTTON || buttonStyle == BS_AUTORADIOBUTTON)
		{
			var radio = new RadioButton
			{
				Content = ProcessAccessKeys(item.Title),
				Tag = item.Id,
				IsEnabled = !isDisabled
			};
			radio.Click += OnControlClick;
			return radio;
		}
		else if (buttonStyle == BS_GROUPBOX)
		{
			var groupBox = new Border
			{
				BorderBrush = Brushes.Gray,
				BorderThickness = new Thickness(1),
				Child = new TextBlock
				{
					Text = ProcessAccessKeys(item.Title),
					Margin = new Thickness(5)
				}
			};
			return groupBox;
		}
		else
		{
			// Push button or default push button
			var button = new Button
			{
				Content = ProcessAccessKeys(item.Title),
				Tag = item.Id,
				IsEnabled = !isDisabled
			};

			if (buttonStyle == BS_DEFPUSHBUTTON)
			{
				button.IsDefault = true;
			}

			button.Click += OnControlClick;
			return button;
		}
	}

	private Control CreateStatic(DialogItem item)
	{
		const uint SS_LEFT = 0x00000000;
		const uint SS_CENTER = 0x00000001;
		const uint SS_RIGHT = 0x00000002;
		const uint SS_ICON = 0x00000003;
		const uint SS_BLACKRECT = 0x00000004;
		const uint SS_GRAYRECT = 0x00000005;
		const uint SS_WHITERECT = 0x00000006;
		const uint SS_BITMAP = 0x0000000E;

		var staticStyle = item.Style & 0x1F;

		if (staticStyle == SS_ICON || staticStyle == SS_BITMAP)
		{
			// Try to load the icon/bitmap resource
			// For now, display the resource name if available, otherwise show placeholder
			if (!string.IsNullOrEmpty(item.Title))
			{
				return new Border
				{
					Background = Brushes.LightGray,
					Child = new TextBlock
					{
						Text = item.Title,
						HorizontalAlignment = HorizontalAlignment.Center,
						VerticalAlignment = VerticalAlignment.Center,
						FontSize = 10,
						Foreground = Brushes.DarkGray
					}
				};
			}
			else
			{
				// Icon placeholder
				return new Border
				{
					Background = Brushes.LightGray,
					Child = new TextBlock
					{
						Text = "🖼",
						HorizontalAlignment = HorizontalAlignment.Center,
						VerticalAlignment = VerticalAlignment.Center
					}
				};
			}
		}
		else if (staticStyle >= SS_BLACKRECT && staticStyle <= SS_WHITERECT)
		{
			// Rectangle
			var brush = staticStyle == SS_BLACKRECT ? Brushes.Black :
			            staticStyle == SS_GRAYRECT ? Brushes.Gray : Brushes.White;
			return new Border { Background = brush };
		}
		else
		{
			// Text label
			var alignment = staticStyle == SS_CENTER ? TextAlignment.Center :
			               staticStyle == SS_RIGHT ? TextAlignment.Right : TextAlignment.Left;

			return new TextBlock
			{
				Text = ProcessAccessKeys(item.Title),
				TextAlignment = alignment,
				VerticalAlignment = VerticalAlignment.Center,
				TextWrapping = TextWrapping.Wrap  // Enable text wrapping and newline handling
			};
		}
	}

	private Control CreateEdit(DialogItem item)
	{
		const uint ES_MULTILINE = 0x0004;
		const uint ES_PASSWORD = 0x0020;
		const uint ES_READONLY = 0x0800;
		const uint WS_DISABLED = 0x08000000;

		var textBox = new TextBox
		{
			Text = item.Title,
			Tag = item.Id,
			IsEnabled = (item.Style & WS_DISABLED) == 0
		};

		if ((item.Style & ES_MULTILINE) != 0)
		{
			textBox.AcceptsReturn = true;
			textBox.TextWrapping = TextWrapping.Wrap;
		}

		if ((item.Style & ES_PASSWORD) != 0)
		{
			textBox.PasswordChar = '*';
		}

		if ((item.Style & ES_READONLY) != 0)
		{
			textBox.IsReadOnly = true;
		}

		return textBox;
	}

	private Control CreateListBox(DialogItem item)
	{
		var listBox = new ListBox
		{
			Tag = item.Id
		};
		listBox.SelectionChanged += OnControlSelectionChanged;
		return listBox;
	}

	private Control CreateComboBox(DialogItem item)
	{
		var comboBox = new ComboBox
		{
			Tag = item.Id
		};
		comboBox.SelectionChanged += OnControlSelectionChanged;
		return comboBox;
	}

	private Control CreateScrollBar(DialogItem item)
	{
		// Create a scrollbar placeholder
		return new Border
		{
			Background = Brushes.LightGray,
			BorderBrush = Brushes.Gray,
			BorderThickness = new Thickness(1)
		};
	}

	private Control CreatePlaceholder(DialogItem item)
	{
		return new Border
		{
			Background = Brushes.LightGray,
			BorderBrush = Brushes.Red,
			BorderThickness = new Thickness(1),
			Child = new TextBlock
			{
				Text = $"[{item.WindowClass}]",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				FontSize = 10
			}
		};
	}

	private void OnControlClick(object? sender, RoutedEventArgs e)
	{
		_debugCallback?.Invoke($"[DialogWindow] OnControlClick triggered, sender type: {sender?.GetType().Name}", DebugLevel.Debug);
		System.Diagnostics.Debug.WriteLine($"[DialogWindow] OnControlClick triggered, sender type: {sender?.GetType().Name}");
		
		if (sender is Control control)
		{
			_debugCallback?.Invoke($"[DialogWindow] Sender is Control, Tag type: {control.Tag?.GetType().Name ?? "null"}", DebugLevel.Debug);
			_debugCallback?.Invoke($"[DialogWindow] Tag value: {control.Tag}", DebugLevel.Debug);
			
			// Try to get the ID from the Tag
			if (control.Tag != null && control.Tag is ushort id)
			{
				_debugCallback?.Invoke($"[DialogWindow] Button clicked: ID={id}, DialogHandle=0x{_dialogHandle:X8}", DebugLevel.Info);
				System.Diagnostics.Debug.WriteLine($"[DialogWindow] Button clicked: ID={id}, DialogHandle=0x{_dialogHandle:X8}");
				
				// Send WM_COMMAND message to the dialog procedure for all button clicks
				// The dialog procedure will decide whether to close the dialog via EndDialog
				const uint WM_COMMAND = 0x0111;
				const uint BN_CLICKED = 0;
				var wParam = (uint)(BN_CLICKED << 16) | id;
				
				// Get the control's window handle if available
				var controlHandle = _controlHandles.TryGetValue(id, out var handle) ? handle : 0u;
				_debugCallback?.Invoke($"[DialogWindow] Sending WM_COMMAND: wParam=0x{wParam:X8}, controlHandle=0x{controlHandle:X8}", DebugLevel.Info);
				System.Diagnostics.Debug.WriteLine($"[DialogWindow] Sending WM_COMMAND: wParam=0x{wParam:X8}, controlHandle=0x{controlHandle:X8}");
				
				if (_messageCallback != null)
				{
					_messageCallback.Invoke(_dialogHandle, WM_COMMAND, wParam, controlHandle);
					_debugCallback?.Invoke($"[DialogWindow] Message callback invoked successfully", DebugLevel.Debug);
					System.Diagnostics.Debug.WriteLine($"[DialogWindow] Message callback invoked successfully");
				}
				else
				{
					_debugCallback?.Invoke($"[DialogWindow] WARNING: Message callback is null! Button click will not be processed.", DebugLevel.Warning);
					System.Diagnostics.Debug.WriteLine($"[DialogWindow] WARNING: Message callback is null! Button click will not be processed.");
				}

				// Note: We no longer automatically close the dialog for IDOK/IDCANCEL
				// The dialog procedure should call EndDialog when appropriate
			}
			else
			{
				_debugCallback?.Invoke($"[DialogWindow] Tag is null or not ushort", DebugLevel.Debug);
			}
		}
		else
		{
			_debugCallback?.Invoke($"[DialogWindow] Sender is not a Control", DebugLevel.Debug);
		    System.Diagnostics.Debug.WriteLine($"[DialogWindow] OnControlClick: Pattern match failed. Sender type: {sender?.GetType().Name}, Control.Tag type: {(sender as Control)?.Tag?.GetType().Name ?? "null"}");
		}
	}

	private void OnControlSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (sender is Control control && control.Tag is ushort id)
		{
			// Send WM_COMMAND message for selection change
			const uint WM_COMMAND = 0x0111;
			const uint LBN_SELCHANGE = 1;
			const uint CBN_SELCHANGE = 1;
			var wParam = (uint)(LBN_SELCHANGE << 16) | id;
			
			// Get the control's window handle if available
			var controlHandle = _controlHandles.TryGetValue(id, out var handle) ? handle : 0u;
			
			_messageCallback?.Invoke(_dialogHandle, WM_COMMAND, wParam, controlHandle);
		}
	}

	/// <summary>
	/// Processes Win32 access key markers (&) for Avalonia.
	/// In Win32, & before a character marks it as an access key.
	/// In Avalonia, _ before a character marks it as an access key.
	/// This also handles HTML entity encoding like &amp; -> &
	/// </summary>
	private string ProcessAccessKeys(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}

		// First, decode HTML entities (e.g., &amp; -> &)
		text = System.Net.WebUtility.HtmlDecode(text);

		// Convert Win32 access key format (&) to Avalonia format (_)
		// In Win32: &Cancel means C is the access key
		// In Avalonia: _Cancel means C is the access key
		// && in Win32 means literal &, which becomes & in Avalonia
		
		var result = text.Replace("&&", "\x00"); // Temporarily replace && with null char
		result = result.Replace("&", "_");        // Replace & with _
		result = result.Replace("\x00", "&");     // Replace null char back to &
		
		return result;
	}

	/// <summary>
	/// Gets a control by its dialog item ID.
	/// </summary>
	public Control? GetControlById(ushort id)
	{
		_controlsById.TryGetValue(id, out var control);
		return control;
	}

	/// <summary>
	/// Sets the text of a control by its ID.
	/// </summary>
	public void SetControlText(ushort id, string text)
	{
		Dispatcher.UIThread.Post(() =>
		{
			var control = GetControlById(id);
			if (control is TextBox textBox)
			{
				textBox.Text = text;
			}
			else if (control is Button button)
			{
				button.Content = text;
			}
			else if (control is TextBlock textBlock)
			{
				textBlock.Text = text;
				// Ensure text wrapping is enabled to handle multi-line text
				textBlock.TextWrapping = TextWrapping.Wrap;
			}
		});
	}

	/// <summary>
	/// Sets a bitmap on a static control.
	/// </summary>
	public void SetControlBitmap(ushort id, byte[] bitmapData)
	{
		_debugCallback?.Invoke($"[DialogWindow] SetControlBitmap called for control ID={id}, data length={bitmapData.Length}", DebugLevel.Debug);
		
		Dispatcher.UIThread.Post(() =>
		{
			var control = GetControlById(id);
			_debugCallback?.Invoke($"[DialogWindow] SetControlBitmap: control found = {control != null}, type = {control?.GetType().Name}", DebugLevel.Debug);
			
			if (control is Border border)
			{
				try
				{
					_debugCallback?.Invoke($"[DialogWindow] SetControlBitmap: Converting DIB to bitmap...", DebugLevel.Debug);
					// Convert DIB bitmap data to Avalonia-compatible image
					var bitmap = ConvertDibToBitmap(bitmapData);
					if (bitmap != null)
					{
						_debugCallback?.Invoke($"[DialogWindow] SetControlBitmap: Bitmap converted successfully, size={bitmap.PixelSize}", DebugLevel.Info);
						border.Child = new Avalonia.Controls.Image
						{
							Source = bitmap,
							Stretch = Avalonia.Media.Stretch.Uniform
						};
						_debugCallback?.Invoke($"[DialogWindow] SetControlBitmap: Image control created and set", DebugLevel.Debug);
					}
					else
					{
						_debugCallback?.Invoke($"[DialogWindow] SetControlBitmap: ConvertDibToBitmap returned null", DebugLevel.Warning);
					}
				}
				catch (ArgumentException ex)
				{
					_debugCallback?.Invoke($"[DialogWindow] SetControlBitmap: ArgumentException - {ex.Message}", DebugLevel.Error);
					// If bitmap conversion fails, show error in the border
					border.Child = new TextBlock
					{
						Text = $"Error loading bitmap: {ex.Message}",
						FontSize = 10,
						Foreground = Brushes.Red
					};
				}
				catch (System.IO.IOException ex)
				{
					_debugCallback?.Invoke($"[DialogWindow] SetControlBitmap: IOException - {ex.Message}", DebugLevel.Error);
					// If bitmap I/O fails, show error in the border
					border.Child = new TextBlock
					{
						Text = $"Error loading bitmap: {ex.Message}",
						FontSize = 10,
						Foreground = Brushes.Red
					};
				}
			}
			else
			{
				_debugCallback?.Invoke($"[DialogWindow] SetControlBitmap: Control is not a Border, cannot set bitmap", DebugLevel.Warning);
			}
		});
	}

	/// <summary>
	/// Enables or disables a control by its ID.
	/// </summary>
	public void SetControlEnabled(ushort id, bool enabled)
	{
		Dispatcher.UIThread.Post(() =>
		{
			var control = GetControlById(id);
			if (control != null)
			{
				control.IsEnabled = enabled;
			}
		});
	}

	public void SetControlVisible(ushort id, bool visible)
	{
		Dispatcher.UIThread.Post(() =>
		{
			var control = GetControlById(id);
			if (control != null)
			{
				control.IsVisible = visible;
			}
		});
	}

	/// <summary>
	/// Converts a DIB (Device Independent Bitmap) to an Avalonia Bitmap.
	/// </summary>
	private Avalonia.Media.Imaging.Bitmap? ConvertDibToBitmap(byte[] dibData)
	{
		if (dibData == null || dibData.Length < 40)
		{
			return null;
		}

		// DIB format starts with BITMAPINFOHEADER
		// We need to add a BITMAPFILEHEADER to make it a valid BMP file
		var headerSize = BitConverter.ToInt32(dibData, 0);
		var bitsPerPixel = BitConverter.ToInt16(dibData, 14);
		
		// Calculate the size of the color table (palette)
		var colorTableSize = 0;
		if (bitsPerPixel <= 8)
		{
			var numColors = BitConverter.ToInt32(dibData, 32); // biClrUsed
			if (numColors == 0)
			{
				numColors = 1 << bitsPerPixel; // 2^bitsPerPixel
			}
			colorTableSize = numColors * 4; // Each color is 4 bytes (RGBQUAD)
		}

		// Calculate file header size and offset to pixel data
		var fileHeaderSize = 14;
		var pixelDataOffset = fileHeaderSize + headerSize + colorTableSize;

		// Create BMP file header
		var bmpData = new byte[fileHeaderSize + dibData.Length];
		
		// Validate the final size
		if (bmpData.Length > int.MaxValue)
		{
			// File size too large for BMP format
			return null;
		}
		
		// BITMAPFILEHEADER
		bmpData[0] = (byte)'B';
		bmpData[1] = (byte)'M';
		BitConverter.GetBytes((int)bmpData.Length).CopyTo(bmpData, 2); // File size
		// Reserved fields are 0
		BitConverter.GetBytes((int)pixelDataOffset).CopyTo(bmpData, 10); // Offset to pixel data

		// Copy DIB data (BITMAPINFOHEADER + color table + pixels)
		dibData.CopyTo(bmpData, fileHeaderSize);

		// Create Avalonia bitmap from the BMP data
		// Don't use 'using' - the Bitmap needs the stream to remain open
		var stream = new System.IO.MemoryStream(bmpData);
		return new Avalonia.Media.Imaging.Bitmap(stream);
	}
}
