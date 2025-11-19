namespace Win32Emu.Tests.Emulator.SingleStepTests;

/// <summary>
/// Utility class for locating test data files
/// </summary>
public static class TestFileHelper
{
	/// <summary>
	/// Finds a test file by searching common locations
	/// </summary>
	/// <param name="fileName">Name of the test file to find</param>
	/// <returns>Full path to the test file, or null if not found</returns>
	public static string? FindTestFile(string fileName)
	{
		var searchPaths = new[]
		{
			Path.Combine("TestData", "SingleStepTests", fileName),
			Path.Combine("SingleStepTests", fileName),
			Path.Combine("..", "TestData", "SingleStepTests", fileName),
			fileName
		};
		
		return searchPaths.FirstOrDefault(File.Exists);
	}
}
