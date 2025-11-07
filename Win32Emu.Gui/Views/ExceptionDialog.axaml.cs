using System;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Win32Emu.Gui.Views;

/// <summary>
/// Dialog window that displays exception details with copy-to-clipboard functionality
/// </summary>
public partial class ExceptionDialog : Window
{
	private readonly Exception _exception;
	private readonly string _context;

	public ExceptionDialog()
	{
		InitializeComponent();
		_exception = new Exception("No exception provided");
		_context = "Unknown";
	}

	public ExceptionDialog(Exception exception, string context = "Emulation")
	{
		InitializeComponent();
		_exception = exception ?? throw new ArgumentNullException(nameof(exception));
		_context = context;

		InitializeControls();
	}

	private void InitializeControls()
	{
		// Set exception type
		var exceptionTypeText = this.FindControl<TextBlock>("ExceptionTypeText");
		if (exceptionTypeText != null)
		{
			exceptionTypeText.Text = $"Type: {_exception.GetType().FullName}";
		}

		// Set exception message
		var exceptionMessageText = this.FindControl<TextBlock>("ExceptionMessageText");
		if (exceptionMessageText != null)
		{
			exceptionMessageText.Text = _exception.Message ?? "(No message provided)";
		}

		// Set stack trace
		var stackTraceText = this.FindControl<TextBlock>("StackTraceText");
		if (stackTraceText != null)
		{
			stackTraceText.Text = _exception.StackTrace ?? "(No stack trace available)";
		}

		// Wire up button events
		var copyButton = this.FindControl<Button>("CopyButton");
		if (copyButton != null)
		{
			copyButton.Click += CopyButton_Click;
		}

		var closeButton = this.FindControl<Button>("CloseButton");
		if (closeButton != null)
		{
			closeButton.Click += CloseButton_Click;
		}
	}

	private async void CopyButton_Click(object? sender, RoutedEventArgs e)
	{
		try
		{
			var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
			if (clipboard != null)
			{
				var details = GetExceptionDetails();
				await clipboard.SetTextAsync(details);

				// Show temporary feedback
				var copyButton = this.FindControl<Button>("CopyButton");
				if (copyButton != null)
				{
					var originalContent = copyButton.Content;
					copyButton.Content = "✓ Copied!";
					copyButton.IsEnabled = false;

					// Reset button after 2 seconds
					await Task.Delay(2000);
					copyButton.Content = originalContent;
					copyButton.IsEnabled = true;
				}
			}
		}
		catch (Exception ex) when (ex is not OutOfMemoryException && ex is not StackOverflowException)
		{
			// Fallback if clipboard access fails - log to debug output only
			// Cannot use ILogger here as this is a UI component without DI
			System.Diagnostics.Debug.WriteLine($"Failed to copy to clipboard: {ex.Message}");
		}
	}

	private void CloseButton_Click(object? sender, RoutedEventArgs e)
	{
		Close();
	}

	private string GetExceptionDetails()
	{
		var sb = new StringBuilder();
		sb.AppendLine("=== Win32Emu Exception Report ===");
		sb.AppendLine($"Context: {_context}");
		sb.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
		sb.AppendLine();
		sb.AppendLine($"Exception Type: {_exception.GetType().FullName}");
		sb.AppendLine($"Message: {_exception.Message}");
		sb.AppendLine();
		sb.AppendLine("Stack Trace:");
		sb.AppendLine(_exception.StackTrace ?? "(No stack trace available)");

		// Include inner exceptions if any
		var innerException = _exception.InnerException;
		var level = 1;
		while (innerException != null)
		{
			sb.AppendLine();
			sb.AppendLine($"--- Inner Exception {level} ---");
			sb.AppendLine($"Type: {innerException.GetType().FullName}");
			sb.AppendLine($"Message: {innerException.Message}");
			sb.AppendLine($"Stack Trace: {innerException.StackTrace ?? "(No stack trace available)"}");
			innerException = innerException.InnerException;
			level++;
		}

		return sb.ToString();
	}

	/// <summary>
	/// Shows the exception dialog modally
	/// </summary>
	public static async Task ShowExceptionDialogAsync(Window? owner, Exception exception, string context = "Emulation")
	{
		var dialog = new ExceptionDialog(exception, context);
		
		if (owner != null)
		{
			await dialog.ShowDialog(owner);
		}
		else
		{
			dialog.Show();
		}
	}
}
