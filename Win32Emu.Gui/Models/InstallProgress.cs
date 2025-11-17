namespace Win32Emu.Gui.Models;

/// <summary>
/// Progress information for game installation to VHD
/// </summary>
public class InstallProgress
{
	/// <summary>
	/// Current file being copied
	/// </summary>
	public string CurrentFile { get; set; } = string.Empty;
	
	/// <summary>
	/// Number of files copied so far
	/// </summary>
	public int FilesCopied { get; set; }
	
	/// <summary>
	/// Total number of files to copy
	/// </summary>
	public int TotalFiles { get; set; }
	
	/// <summary>
	/// Total bytes copied so far
	/// </summary>
	public long BytesCopied { get; set; }
	
	/// <summary>
	/// Total bytes to copy
	/// </summary>
	public long TotalBytes { get; set; }
	
	/// <summary>
	/// Percentage complete (0-100)
	/// </summary>
	public double PercentComplete => TotalFiles > 0 ? (FilesCopied * 100.0 / TotalFiles) : 0;
	
	/// <summary>
	/// Indicates if the operation is complete
	/// </summary>
	public bool IsComplete { get; set; }
	
	/// <summary>
	/// Error message if the operation failed
	/// </summary>
	public string? ErrorMessage { get; set; }
}
