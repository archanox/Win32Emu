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
	}
}