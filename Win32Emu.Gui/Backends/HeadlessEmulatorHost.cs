using Microsoft.Extensions.Logging;

namespace Win32Emu.Gui.Backends;

/// <summary>
/// Minimal IEmulatorHost implementation for headless mode.
/// Provides basic window creation notifications and stub implementations for other callbacks.
/// </summary>
public class HeadlessEmulatorHost : IEmulatorHost
{
	private readonly ILogger _logger;

	public HeadlessEmulatorHost(ILogger logger)
	{
		_logger = logger;
	}

	public void OnDebugOutput(string message, DebugLevel level)
	{
		// Debug output is already logged through the standard logging system
		// No need to duplicate it in headless mode
	}

	public void OnStdOutput(string output)
	{
		// Standard output is handled by console in headless mode
		// Already logged through the standard logging system
	}

	public void OnWindowCreate(WindowCreateInfo info)
	{
		_logger.LogDebug("[HeadlessHost] Window created: HWND=0x{Handle:X8} Class='{ClassName}' Title='{Title}' Size={Width}x{Height}",
			info.Handle, info.ClassName, info.Title, info.Width, info.Height);
		
		// In headless mode, we don't create actual UI windows
		// But we acknowledge the window creation for proper application flow
	}

	public Task<int> OnDialogCreate(DialogCreateInfo info)
	{
		_logger.LogWarning("[HeadlessHost] Dialog creation requested in headless mode: Handle=0x{Handle:X8}, returning error", info.Handle);
		// Return error code indicating dialogs are not supported in headless mode
		return Task.FromResult(-1);
	}

	public void OnDialogEnd(uint dialogHandle, int result)
	{
		_logger.LogDebug("[HeadlessHost] Dialog ended: Handle=0x{Handle:X8} Result={Result}", dialogHandle, result);
	}

	public int OnMessageBox(MessageBoxInfo info)
	{
		_logger.LogWarning("[HeadlessHost] MessageBox in headless mode: '{Text}' - '{Caption}', returning IDOK", info.Text, info.Caption);
		// Return IDOK (1) to automatically dismiss message boxes in headless mode
		return 1; // IDOK
	}

	public void OnDialogControlTextChanged(uint dialogHandle, int controlId, string text)
	{
		_logger.LogDebug("[HeadlessHost] Dialog control text changed: Dialog=0x{Handle:X8} Control={Id} Text='{Text}'",
			dialogHandle, controlId, text);
	}

	public void OnDialogControlBitmapChanged(uint dialogHandle, int controlId, byte[] bitmapData)
	{
		_logger.LogDebug("[HeadlessHost] Dialog control bitmap changed: Dialog=0x{Handle:X8} Control={Id} Size={Size} bytes",
			dialogHandle, controlId, bitmapData.Length);
	}

	public void OnDialogControlEnabledChanged(uint dialogHandle, int controlId, bool enabled)
	{
		_logger.LogDebug("[HeadlessHost] Dialog control enabled changed: Dialog=0x{Handle:X8} Control={Id} Enabled={Enabled}",
			dialogHandle, controlId, enabled);
	}

	public void OnDisplayUpdate(DisplayUpdateInfo info)
	{
		// Display updates are handled by the rendering backend
		// No need for additional logging in headless mode
	}

	public Task<string?> OnBrowseForFolder(string? title, string? rootPath)
	{
		_logger.LogWarning("[HeadlessHost] Folder browser requested in headless mode: '{Title}', returning null", title);
		// Return null to indicate the operation was cancelled
		return Task.FromResult<string?>(null);
	}

	public Task<string?> OnOpenFileDialog(string? title, string? filter, string? initialDirectory)
	{
		_logger.LogWarning("[HeadlessHost] Open file dialog requested in headless mode: '{Title}', returning null", title);
		// Return null to indicate the operation was cancelled
		return Task.FromResult<string?>(null);
	}

	public Task<string?> OnSaveFileDialog(string? title, string? filter, string? initialDirectory)
	{
		_logger.LogWarning("[HeadlessHost] Save file dialog requested in headless mode: '{Title}', returning null", title);
		// Return null to indicate the operation was cancelled
		return Task.FromResult<string?>(null);
	}

	public void OnWindowTitleChanged(uint windowHandle, string title)
	{
		_logger.LogDebug("[HeadlessHost] Window title changed: HWND=0x{Handle:X8} Title='{Title}'", windowHandle, title);
	}

	public void OnControlVisibilityChanged(uint dialogHandle, int controlId, bool visible)
	{
		_logger.LogDebug("[HeadlessHost] Control visibility changed: Dialog=0x{Handle:X8} Control={Id} Visible={Visible}",
			dialogHandle, controlId, visible);
	}
}
