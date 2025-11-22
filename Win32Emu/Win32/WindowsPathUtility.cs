namespace Win32Emu.Win32;

/// <summary>
/// Utility class for handling Windows path operations in a cross-platform manner.
/// Provides consistent path resolution logic that emulates Windows path semantics
/// even when running on Unix-like systems.
/// </summary>
public static class WindowsPathUtility
{
	/// <summary>
	/// Checks if a path is rooted in the Windows sense.
	/// In Windows, a path is rooted if it has a drive letter (C:\, D:/) or is a UNC path (\\server\share).
	/// Paths like /data or \data are NOT rooted - they're relative to the current drive.
	/// This is important on Unix systems where Path.IsPathRooted treats /path as rooted.
	/// </summary>
	public static bool IsWindowsRootedPath(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}

		// Check for drive letter (C:\ or C:/)
		if (path.Length >= 2 && path[1] == ':' && char.IsLetter(path[0]))
		{
			return true;
		}

		// Check for UNC path (\\server\share or //server/share)
		if (path.Length >= 2 && (path[0] == '\\' || path[0] == '/') && (path[1] == '\\' || path[1] == '/'))
		{
			return true;
		}

		return false;
	}

	/// <summary>
	/// Extracts the directory path from a Windows-style path.
	/// Works correctly on Unix systems where Path.GetDirectoryName doesn't recognize backslashes.
	/// </summary>
	/// <param name="path">The Windows-style path (e.g., C:\ign_teas\file.exe)</param>
	/// <returns>The directory path, or null if it cannot be determined</returns>
	public static string? GetWindowsDirectory(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return null;
		}

		// Check if this is a Windows-style path (starts with drive letter and contains path separators)
		if (path.Length >= 3 &&
			path[1] == ':' &&
			char.IsLetter(path[0]) &&
			(path.Contains('\\') || path.Contains('/')))
		{
			// Find the last backslash or forward slash
			var lastBackslash = path.LastIndexOf('\\');
			var lastForwardSlash = path.LastIndexOf('/');
			var lastSeparator = Math.Max(lastBackslash, lastForwardSlash);
			
			if (lastSeparator > 0)
			{
				return path.Substring(0, lastSeparator);
			}
		}
		
		// Fallback to Path.GetDirectoryName for native paths
		return Path.GetDirectoryName(path);
	}

	/// <summary>
	/// Resolves a path relative to a current directory, handling Windows path semantics.
	/// </summary>
	/// <param name="path">The path to resolve</param>
	/// <param name="currentDirectory">The current directory</param>
	/// <returns>The resolved absolute path</returns>
	public static string ResolvePath(string path, string currentDirectory)
	{
		if (string.IsNullOrEmpty(path))
		{
			return currentDirectory;
		}

		// If already rooted with drive letter or UNC, return as-is
		if (IsWindowsRootedPath(path))
		{
			return path;
		}

		// If path starts with \ (backslash), it's a root-relative path - prepend the drive letter
		// Forward slashes / are treated as regular path separators, not root indicators
		if (path.Length > 0 && path[0] == '\\')
		{
			// Extract drive letter from CurrentDirectory (e.g., "C:" from "C:\ign_teas")
			if (currentDirectory.Length >= 2 && currentDirectory[1] == ':')
			{
				var drive = currentDirectory.Substring(0, 2); // e.g., "C:"
				return drive + path; // e.g., "C:\data\file.txt"
			}
			
			// Defensive fallback: CurrentDirectory doesn't have a valid drive letter, use "C:" as default
			return "C:" + path;
		}

		// Path is relative (including paths starting with /), resolve it relative to current directory
		// Use custom Windows-style path combining to avoid platform-specific path separators
		var baseDir = currentDirectory.TrimEnd('\\', '/');
		var relativePath = path.TrimStart('\\', '/');
		return baseDir + "\\" + relativePath;
	}

	/// <summary>
	/// Extracts the drive letter from a Windows path.
	/// </summary>
	/// <param name="path">The Windows path</param>
	/// <returns>The drive letter with colon (e.g., "C:"), or null if no drive letter is present</returns>
	public static string? ExtractDrive(string path)
	{
		if (string.IsNullOrEmpty(path) || path.Length < 2)
		{
			return null;
		}

		if (path[1] == ':' && char.IsLetter(path[0]))
		{
			return path.Substring(0, 2);
		}

		return null;
	}
}
