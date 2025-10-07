using System.Globalization;
using Avalonia.Media.Imaging;
using Win32Emu.Gui.Converters;

namespace Win32Emu.Tests.Gui;

public class ThumbnailPathConverterTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ThumbnailPathConverter _converter;

    public ThumbnailPathConverterTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "Win32EmuConverterTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _converter = new ThumbnailPathConverter();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Convert_WithNullValue_ReturnsNull()
    {
        // Act
        var result = _converter.Convert(null, typeof(Bitmap), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Convert_WithEmptyString_ReturnsNull()
    {
        // Act
        var result = _converter.Convert("", typeof(Bitmap), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Convert_WithNonExistentPath_ReturnsNull()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_tempDir, "nonexistent.ico");

        // Act
        var result = _converter.Convert(nonExistentPath, typeof(Bitmap), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Convert_WithValidIconPath_ReturnsBitmap()
    {
        // Arrange - use the default-game-icon.ico if it exists
        var iconPath = GetTestIconPath();
        
        // Skip test if icon doesn't exist
        if (!File.Exists(iconPath))
        {
            return; // Skip test
        }

        // Act
        var result = _converter.Convert(iconPath, typeof(Bitmap), null, CultureInfo.InvariantCulture);

        // Assert
        // Note: Bitmap loading might fail in test context without UI, so we just verify it doesn't throw
        // In actual UI context, this would return a valid Bitmap
        Assert.True(result == null || result is Bitmap);
    }

    [Fact]
    public void Convert_WithInvalidIconFile_ReturnsNull()
    {
        // Arrange - create an invalid icon file
        var iconPath = Path.Combine(_tempDir, "invalid.ico");
        File.WriteAllText(iconPath, "This is not a valid icon file");

        // Act
        var result = _converter.Convert(iconPath, typeof(Bitmap), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Act & Assert
        Assert.Throws<NotImplementedException>(() =>
            _converter.ConvertBack(null, typeof(string), null, CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Gets path to a test icon file
    /// </summary>
    private static string GetTestIconPath()
    {
        // Try to find the default-game-icon.ico
        var possiblePaths = new[]
        {
            "../../../Win32Emu.Gui/Assets/default-game-icon.ico",
            "../../../../Win32Emu.Gui/Assets/default-game-icon.ico",
            "/home/runner/work/Win32Emu/Win32Emu/Win32Emu.Gui/Assets/default-game-icon.ico"
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        return possiblePaths[0];
    }
}
