using Microsoft.Extensions.Logging;
using Win32Emu;

namespace Win32Emu.Wasm.Services;

/// <summary>
/// WASM-specific implementation of IEmulatorHost that forwards emulator output
/// to the EmulatorService's events for display in the Blazor UI.
/// </summary>
public class WasmEmulatorHost : IEmulatorHost
{
	private readonly ILogger<WasmEmulatorHost> _logger;
	
	/// <summary>
	/// Event fired when debug output is received from the emulator.
	/// </summary>
	public event EventHandler<string>? DebugOutputReceived;
	
	/// <summary>
	/// Event fired when stdout output is received from the emulator.
	/// </summary>
	public event EventHandler<string>? StdOutputReceived;
	
	/// <summary>
	/// Event fired when a window is created.
	/// </summary>
	public event EventHandler<WindowCreateInfo>? WindowCreated;
	
	/// <summary>
	/// Event fired when a dialog needs to be displayed and awaits user interaction.
	/// </summary>
	public event EventHandler<DialogCreateEventArgs>? DialogCreateRequested;
	
	/// <summary>
	/// Event fired when a message box needs to be displayed and awaits user response.
	/// </summary>
	public event EventHandler<MessageBoxEventArgs>? MessageBoxRequested;
	
	/// <summary>
	/// Event fired when a dialog ends.
	/// </summary>
	public event EventHandler<DialogEndEventArgs>? DialogEnded;
	
	public WasmEmulatorHost(ILogger<WasmEmulatorHost> logger)
	{
		_logger = logger;
	}
	
	/// <summary>
	/// Event arguments for dialog creation requests.
	/// </summary>
	public class DialogCreateEventArgs : EventArgs
	{
		public required DialogCreateInfo Info { get; init; }
		public TaskCompletionSource<int> CompletionSource { get; } = new();
	}
	
	/// <summary>
	/// Event arguments for message box requests.
	/// </summary>
	public class MessageBoxEventArgs : EventArgs
	{
		public required MessageBoxInfo Info { get; init; }
		public TaskCompletionSource<int> CompletionSource { get; } = new();
	}
	
	/// <summary>
	/// Event arguments for dialog end notifications.
	/// </summary>
	public class DialogEndEventArgs : EventArgs
	{
		public uint DialogHandle { get; init; }
		public int Result { get; init; }
	}
	
	public void OnDebugOutput(string message, DebugLevel level)
	{
		// Log to console for debugging
		_logger.LogDebug("[EmulatorHost] Debug ({Level}): {Message}", level, message);
		
		// Forward to UI via event
		DebugOutputReceived?.Invoke(this, message);
	}
	
	public void OnStdOutput(string output)
	{
		// Log to console for debugging
		_logger.LogInformation("[EmulatorHost] StdOut: {Output}", output);
		
		// Forward to UI via event
		StdOutputReceived?.Invoke(this, output);
	}
	
	public void OnWindowCreate(WindowCreateInfo info)
	{
		_logger.LogInformation("[EmulatorHost] Window created: {Title}", info.Title);
		// Forward to UI for display
		WindowCreated?.Invoke(this, info);
	}
	
	public Task<int> OnDialogCreate(DialogCreateInfo info)
	{
		_logger.LogInformation("[EmulatorHost] Dialog created: {Title}", info.Template.Title);
		
		// Create event args with completion source
		var eventArgs = new DialogCreateEventArgs { Info = info };
		
		// Raise event to UI (this is async-friendly)
		DialogCreateRequested?.Invoke(this, eventArgs);
		
		// Return the task that will be completed when the user interacts with the dialog
		return eventArgs.CompletionSource.Task;
	}
	
	public void OnDialogEnd(uint dialogHandle, int result)
	{
		_logger.LogInformation("[EmulatorHost] Dialog ended: Handle=0x{Handle:X8}, Result={Result}", dialogHandle, result);
		DialogEnded?.Invoke(this, new DialogEndEventArgs { DialogHandle = dialogHandle, Result = result });
	}
	
	public int OnMessageBox(MessageBoxInfo info)
	{
		_logger.LogInformation("[EmulatorHost] MessageBox: {Title} - {Text}", info.Caption, info.Text);
		
		// Forward to stdout so user can see it in the WASM UI
		StdOutputReceived?.Invoke(this, $"[MessageBox] {info.Caption}: {info.Text}\n");
		
		// Create event args with completion source
		var eventArgs = new MessageBoxEventArgs { Info = info };
		
		// Raise event to UI
		MessageBoxRequested?.Invoke(this, eventArgs);
		
		// Wait synchronously for the result (MessageBox is a blocking API in Win32)
		// Note: This blocks the emulator thread, but that's consistent with Win32 behavior
		// Using GetAwaiter().GetResult() instead of .Result to avoid potential deadlocks
		return eventArgs.CompletionSource.Task.GetAwaiter().GetResult();
	}
	
	public void OnDialogControlTextChanged(uint dialogHandle, int controlId, string text)
	{
		_logger.LogDebug("[EmulatorHost] Dialog control text changed: Handle=0x{Handle:X8}, ControlId={ControlId}, Text={Text}", 
			dialogHandle, controlId, text);
	}
	
	public void OnDialogControlBitmapChanged(uint dialogHandle, int controlId, byte[] bitmapData)
	{
		_logger.LogDebug("[EmulatorHost] Dialog control bitmap changed: Handle=0x{Handle:X8}, ControlId={ControlId}, Size={Size}", 
			dialogHandle, controlId, bitmapData.Length);
	}
	
	public void OnDialogControlEnabledChanged(uint dialogHandle, int controlId, bool enabled)
	{
		_logger.LogDebug("[EmulatorHost] Dialog control enabled changed: Handle=0x{Handle:X8}, ControlId={ControlId}, Enabled={Enabled}", 
			dialogHandle, controlId, enabled);
	}
	
	public void OnDisplayUpdate(DisplayUpdateInfo info)
	{
		// Display updates are handled by the WasmRenderingBackend
		_logger.LogTrace("[EmulatorHost] Display update: {Width}x{Height}", info.Width, info.Height);
	}
	
	public Task<string?> OnBrowseForFolder(string? title, string? rootPath)
	{
		_logger.LogInformation("[EmulatorHost] Browse for folder requested: {Title}, {RootPath}", title, rootPath);
		// Not supported in WASM - return null to indicate cancelled
		return Task.FromResult<string?>(null);
	}
	
	public Task<string?> OnOpenFileDialog(string? title, string? filter, string? initialDirectory)
	{
		_logger.LogInformation("[EmulatorHost] Open file dialog requested: {Title}", title);
		// Not supported in WASM - return null to indicate cancelled
		// Future: could implement using browser's file picker API via JS interop
		return Task.FromResult<string?>(null);
	}
	
	public Task<string?> OnSaveFileDialog(string? title, string? filter, string? initialDirectory)
	{
		_logger.LogInformation("[EmulatorHost] Save file dialog requested: {Title}", title);
		// Not supported in WASM - return null to indicate cancelled
		// Future: could implement using browser's file saver API via JS interop
		return Task.FromResult<string?>(null);
	}
	
	public void OnWindowTitleChanged(uint windowHandle, string title)
	{
		_logger.LogDebug("[EmulatorHost] Window title changed: Handle=0x{Handle:X8}, Title={Title}", windowHandle, title);
	}
	
	public void OnControlVisibilityChanged(uint dialogHandle, int controlId, bool visible)
	{
		_logger.LogDebug("[EmulatorHost] Control visibility changed: Handle=0x{Handle:X8}, ControlId={ControlId}, Visible={Visible}", 
			dialogHandle, controlId, visible);
	}
}
