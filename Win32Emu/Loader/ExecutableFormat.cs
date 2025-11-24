namespace Win32Emu.Loader;

/// <summary>
/// Executable file format types supported by Win32Emu.
/// </summary>
public enum ExecutableFormat
{
	/// <summary>
	/// Unknown or unsupported format.
	/// </summary>
	Unknown = 0,
	
	/// <summary>
	/// PE32 (Portable Executable) format - Win32 32-bit executables.
	/// </summary>
	PE32 = 1,
	
	/// <summary>
	/// NE (New Executable) format - Win16 16-bit executables.
	/// </summary>
	NE = 2
}
