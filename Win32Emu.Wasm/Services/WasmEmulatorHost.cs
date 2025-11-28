using Microsoft.Extensions.Logging;

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
	
	public WasmEmulatorHost(ILogger<WasmEmulatorHost> logger)
	{
		_logger = logger;
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
		// WASM doesn't support native window creation - handled by canvas
	}
	
	public Task<int> OnDialogCreate(DialogCreateInfo info)
	{
		_logger.LogInformation("[EmulatorHost] Dialog created: {Title}", info.Template.Title);
		// Return 0 to indicate dialog was "cancelled" (not supported in WASM POC)
		return Task.FromResult(0);
	}
	
	public void OnDialogEnd(uint dialogHandle, int result)
	{
		_logger.LogInformation("[EmulatorHost] Dialog ended: Handle=0x{Handle:X8}, Result={Result}", dialogHandle, result);
	}
	
	public int OnMessageBox(MessageBoxInfo info)
	{
		_logger.LogInformation("[EmulatorHost] MessageBox: {Title} - {Text}", info.Caption, info.Text);
		// Forward to stdout so user can see it in the WASM UI
		StdOutputReceived?.Invoke(this, $"[MessageBox] {info.Caption}: {info.Text}\n");
		// Return IDOK (1) as default response
		return 1;
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
