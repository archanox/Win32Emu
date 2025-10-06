using Win32Emu.Loader;

namespace Win32Emu.Tests.Gui;

public class PeIconExtractorTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _outputIconPath;

    public PeIconExtractorTests()
    {
        // Create temporary directory for test files
        _tempDir = Path.Combine(Path.GetTempPath(), "Win32EmuIconTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _outputIconPath = Path.Combine(_tempDir, "extracted_icon.ico");
    }

    public void Dispose()
    {
        // Clean up temporary directory
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void TryExtractIcon_WithValidPE_ReturnsTrue()
    {
        // Arrange
        var testExePath = GetTestExecutablePath();
        
        // Skip test if test executable doesn't exist
        if (!File.Exists(testExePath))
        {
            return; // Skip test
        }

        // Act
        var result = PeIconExtractor.TryExtractIcon(testExePath, _outputIconPath);

        // Assert
        Assert.True(result, "Icon extraction should succeed for valid PE file");
        Assert.True(File.Exists(_outputIconPath), "Icon file should be created");
        
        // Verify the icon file has content
        var fileInfo = new FileInfo(_outputIconPath);
        Assert.True(fileInfo.Length > 0, "Icon file should not be empty");
    }

    [Fact]
    public void TryExtractIcon_WithNonExistentFile_ReturnsFalse()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_tempDir, "nonexistent.exe");

        // Act
        var result = PeIconExtractor.TryExtractIcon(nonExistentPath, _outputIconPath);

        // Assert
        Assert.False(result, "Icon extraction should fail for non-existent file");
        Assert.False(File.Exists(_outputIconPath), "No icon file should be created");
    }

    [Fact]
    public void ExtractIconToTemp_WithValidPE_ReturnsPath()
    {
        // Arrange
        var testExePath = GetTestExecutablePath();
        
        // Skip test if test executable doesn't exist
        if (!File.Exists(testExePath))
        {
            return; // Skip test
        }

        // Act
        var iconPath = PeIconExtractor.ExtractIconToTemp(testExePath);

        // Assert
        Assert.NotNull(iconPath);
        Assert.True(File.Exists(iconPath), "Icon file should exist at returned path");
        
        // Clean up the temp icon
        if (iconPath != null && File.Exists(iconPath))
        {
            File.Delete(iconPath);
        }
    }

    [Fact]
    public void ExtractIconToTemp_WithNonExistentFile_ReturnsNull()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_tempDir, "nonexistent.exe");

        // Act
        var iconPath = PeIconExtractor.ExtractIconToTemp(nonExistentPath);

        // Assert
        Assert.Null(iconPath);
    }

    private static string GetTestExecutablePath()
    {
        // Try to find a test executable
        // Look for the ign_win.exe in the test data
        var possiblePaths = new[]
        {
            "../../../EXEs/ign_win/Ign_win.exe",
            "../../../../EXEs/ign_win/Ign_win.exe",
            "/home/runner/work/Win32Emu/Win32Emu/EXEs/ign_win/Ign_win.exe"
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        // Return the first path (test will be skipped if it doesn't exist)
        return possiblePaths[0];
    }
}
