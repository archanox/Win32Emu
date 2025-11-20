namespace Win32Emu
{
	public interface IEmulatorHost
	{
		void OnDebugOutput(string message, DebugLevel level);
		void OnStdOutput(string output);
		void OnWindowCreate(WindowCreateInfo info);
		Task<int> OnDialogCreate(DialogCreateInfo info);
		void OnDialogEnd(uint dialogHandle, int result);
		int OnMessageBox(MessageBoxInfo info);
		void OnDialogControlTextChanged(uint dialogHandle, int controlId, string text);
		void OnDialogControlBitmapChanged(uint dialogHandle, int controlId, byte[] bitmapData);
		void OnDialogControlEnabledChanged(uint dialogHandle, int controlId, bool enabled);
		void OnDisplayUpdate(DisplayUpdateInfo info);
		
		/// <summary>
		/// Shows a folder browser dialog for VFS navigation.
		/// Returns the selected folder path, or null if cancelled.
		/// </summary>
		Task<string?> OnBrowseForFolder(string? title, string? rootPath);
		
		/// <summary>
		/// Notifies the host that a window's title/text has changed.
		/// </summary>
		/// <param name="windowHandle">The window handle</param>
		/// <param name="title">The new window title</param>
		void OnWindowTitleChanged(uint windowHandle, string title);
		
		/// <summary>
		/// Notifies the host that a dialog control's visibility has changed.
		/// </summary>
		/// <param name="dialogHandle">The parent dialog handle</param>
		/// <param name="controlId">The control ID</param>
		/// <param name="visible">True if visible, false if hidden</param>
		void OnControlVisibilityChanged(uint dialogHandle, int controlId, bool visible);
	}
}