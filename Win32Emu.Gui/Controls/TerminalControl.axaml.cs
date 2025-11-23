using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;

namespace Win32Emu.Gui.Controls;

/// <summary>
/// Custom terminal control for console/terminal emulation.
/// Provides character-based rendering with attributes (color, bold, underline).
/// </summary>
public partial class TerminalControl : UserControl
{
	private const int DefaultColumns = 80;
	private const int DefaultRows = 25;
	private const double CharWidth = 8.0;
	private const double CharHeight = 16.0;

	private readonly Canvas _canvas;
	private readonly char[,] _buffer;
	private readonly TerminalCharAttributes[,] _attributes;
	private readonly Dictionary<int, TextBlock> _textBlocks = new();
	
	private int _cursorX;
	private int _cursorY;
	private readonly int _columns;
	private readonly int _rows;
	private readonly bool _cursorVisible = true;
	private TextBlock? _cursorBlock;

	public TerminalControl()
	{
		InitializeComponent();
		
		_canvas = this.FindControl<Canvas>("TerminalCanvas") ?? throw new InvalidOperationException("TerminalCanvas not found");
		_columns = DefaultColumns;
		_rows = DefaultRows;
		_buffer = new char[_rows, _columns];
		_attributes = new TerminalCharAttributes[_rows, _columns];
		
		// Initialize buffer with spaces
		for (int row = 0; row < _rows; row++)
		{
			for (int col = 0; col < _columns; col++)
			{
				_buffer[row, col] = ' ';
				_attributes[row, col] = new TerminalCharAttributes
				{
					Foreground = Colors.LightGray,
					Background = Colors.Black
				};
			}
		}

		// Set up event handlers
		this.KeyDown += OnKeyDown;
		this.GotFocus += (s, e) => ShowCursor();
		this.LostFocus += (s, e) => HideCursor();

		// Initial render
		RenderAll();
		ShowCursor();
	}

	/// <summary>
	/// Event fired when a key is pressed in the terminal
	/// </summary>
	public event EventHandler<TerminalKeyEventArgs>? KeyPressed;

	/// <summary>
	/// Write text to the terminal at the current cursor position
	/// </summary>
	public void Write(string text)
	{
		Dispatcher.UIThread.Post(() =>
		{
			foreach (char c in text)
			{
				WriteChar(c);
			}
			RenderFromCursor();
		});
	}

	/// <summary>
	/// Write text with a newline
	/// </summary>
	public void WriteLine(string text)
	{
		Write(text + "\n");
	}

	/// <summary>
	/// Clear the terminal screen
	/// </summary>
	public void Clear()
	{
		Dispatcher.UIThread.Post(() =>
		{
			for (int row = 0; row < _rows; row++)
			{
				for (int col = 0; col < _columns; col++)
				{
					_buffer[row, col] = ' ';
					_attributes[row, col] = new TerminalCharAttributes
					{
						Foreground = Colors.LightGray,
						Background = Colors.Black
					};
				}
			}
			_cursorX = 0;
			_cursorY = 0;
			RenderAll();
		});
	}

	/// <summary>
	/// Set cursor position (0-based)
	/// </summary>
	public void SetCursorPosition(int x, int y)
	{
		Dispatcher.UIThread.Post(() =>
		{
			HideCursor();
			_cursorX = Math.Clamp(x, 0, _columns - 1);
			_cursorY = Math.Clamp(y, 0, _rows - 1);
			ShowCursor();
		});
	}

	private void WriteChar(char c)
	{
		switch (c)
		{
			case '\r':
				_cursorX = 0;
				break;
			case '\n':
				_cursorX = 0;
				_cursorY++;
				if (_cursorY >= _rows)
				{
					ScrollUp();
				}
				break;
			case '\b':
				if (_cursorX > 0)
				{
					_cursorX--;
					_buffer[_cursorY, _cursorX] = ' ';
				}
				break;
			case '\t':
				// Tab to next 8-column boundary
				_cursorX = (_cursorX + 8) & ~7;
				if (_cursorX >= _columns)
				{
					_cursorX = 0;
					_cursorY++;
					if (_cursorY >= _rows)
					{
						ScrollUp();
					}
				}
				break;
			default:
				if (c >= 32) // Printable character
				{
					_buffer[_cursorY, _cursorX] = c;
					_attributes[_cursorY, _cursorX] = new TerminalCharAttributes
					{
						Foreground = Colors.LightGray,
						Background = Colors.Black
					};
					_cursorX++;
					if (_cursorX >= _columns)
					{
						_cursorX = 0;
						_cursorY++;
						if (_cursorY >= _rows)
						{
							ScrollUp();
						}
					}
				}
				break;
		}
	}

	private void ScrollUp()
	{
		// Move all lines up by one
		for (int row = 0; row < _rows - 1; row++)
		{
			for (int col = 0; col < _columns; col++)
			{
				_buffer[row, col] = _buffer[row + 1, col];
				_attributes[row, col] = _attributes[row + 1, col];
			}
		}
		
		// Clear the last line
		for (int col = 0; col < _columns; col++)
		{
			_buffer[_rows - 1, col] = ' ';
			_attributes[_rows - 1, col] = new TerminalCharAttributes
			{
				Foreground = Colors.LightGray,
				Background = Colors.Black
			};
		}
		
		_cursorY = _rows - 1;
		RenderAll();
	}

	private void RenderAll()
	{
		_canvas.Children.Clear();
		_textBlocks.Clear();

		// Update canvas size to match terminal buffer dimensions
		_canvas.Width = _columns * CharWidth;
		_canvas.Height = _rows * CharHeight;

		var sb = new StringBuilder();
		for (int row = 0; row < _rows; row++)
		{
			sb.Clear();
			var lastAttr = _attributes[row, 0];
			int startCol = 0;

			for (int col = 0; col < _columns; col++)
			{
				var currentAttr = _attributes[row, col];
				
				// If attributes changed, render the accumulated text
				if (!currentAttr.Equals(lastAttr) || col == _columns - 1)
				{
					if (col == _columns - 1 && currentAttr.Equals(lastAttr))
					{
						sb.Append(_buffer[row, col]);
					}

					if (sb.Length > 0)
					{
						var textBlock = CreateTextBlock(sb.ToString(), row, startCol, lastAttr);
						_canvas.Children.Add(textBlock);
						int key = row * _columns + startCol;
						_textBlocks[key] = textBlock;
					}

					sb.Clear();
					startCol = col;
					lastAttr = currentAttr;
				}

				if (col != _columns - 1 || !currentAttr.Equals(lastAttr))
				{
					sb.Append(_buffer[row, col]);
				}
			}
		}

		ShowCursor();
	}

	private void RenderFromCursor()
	{
		// For simplicity, just re-render the current line and cursor
		int row = _cursorY;
		
		// Remove existing text blocks for this row
		var toRemove = _canvas.Children
			.OfType<TextBlock>()
			.Where(tb => Math.Abs(Canvas.GetTop(tb) - row * CharHeight) < 0.1)
			.ToList();
		
		foreach (var item in toRemove)
		{
			_canvas.Children.Remove(item);
		}

		// Render the line
		var sb = new StringBuilder();
		for (int col = 0; col < _columns; col++)
		{
			sb.Append(_buffer[row, col]);
		}

		var textBlock = CreateTextBlock(sb.ToString(), row, 0, _attributes[row, 0]);
		_canvas.Children.Add(textBlock);

		ShowCursor();
	}

	private TextBlock CreateTextBlock(string text, int row, int col, TerminalCharAttributes attr)
	{
		var textBlock = new TextBlock
		{
			Text = text,
			FontFamily = new FontFamily("Consolas,Courier New,monospace"),
			FontSize = 12,
			Foreground = new SolidColorBrush(attr.Foreground),
			Background = new SolidColorBrush(attr.Background)
		};

		Canvas.SetLeft(textBlock, col * CharWidth);
		Canvas.SetTop(textBlock, row * CharHeight);

		return textBlock;
	}

	private void ShowCursor()
	{
		if (!_cursorVisible) return;

		HideCursor();

		_cursorBlock = new TextBlock
		{
			Text = "█",
			FontFamily = new FontFamily("Consolas,Courier New,monospace"),
			FontSize = 12,
			Foreground = new SolidColorBrush(Colors.White),
			Opacity = 0.7
		};

		Canvas.SetLeft(_cursorBlock, _cursorX * CharWidth);
		Canvas.SetTop(_cursorBlock, _cursorY * CharHeight);
		_canvas.Children.Add(_cursorBlock);
	}

	private void HideCursor()
	{
		if (_cursorBlock != null)
		{
			_canvas.Children.Remove(_cursorBlock);
			_cursorBlock = null;
		}
	}

	private void OnKeyDown(object? sender, KeyEventArgs e)
	{
		var args = new TerminalKeyEventArgs
		{
			Key = e.Key,
			Modifiers = e.KeyModifiers
		};

		KeyPressed?.Invoke(this, args);
		e.Handled = true;
	}
}

/// <summary>
/// Character attributes for terminal display
/// </summary>
public struct TerminalCharAttributes
{
	public Color Foreground { get; set; }
	public Color Background { get; set; }
	public bool Bold { get; set; }
	public bool Underline { get; set; }

	public bool Equals(TerminalCharAttributes other)
	{
		return Foreground == other.Foreground &&
		       Background == other.Background &&
		       Bold == other.Bold &&
		       Underline == other.Underline;
	}

	public override bool Equals(object? obj)
	{
		return obj is TerminalCharAttributes other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Foreground, Background, Bold, Underline);
	}
}

/// <summary>
/// Event args for terminal key events
/// </summary>
public class TerminalKeyEventArgs : EventArgs
{
	public Key Key { get; set; }
	public KeyModifiers Modifiers { get; set; }
}
