using Win32Emu.Gui.Services;
using Xunit;

namespace Win32Emu.Tests.Gui;

public class PeMetadataServiceTests
{
    [Fact]
    public void GetMetadata_WithValidPeFile_ReturnsMetadata()
    {
        // Arrange
        var testExePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "EXEs", "ign_win", "Ign_win.exe");
        
        // Skip test if the file doesn't exist
        if (!File.Exists(testExePath))
        {
            return;
        }

        // Act
        var metadata = PeMetadataService.GetMetadata(testExePath);

        // Assert
        Assert.NotNull(metadata);
        Assert.NotEmpty(metadata.FileName);
        Assert.True(metadata.FileSize > 0);
        Assert.NotEmpty(metadata.MachineType);
        Assert.NotEmpty(metadata.MinimumOs);
        Assert.NotEmpty(metadata.MinimumOsVersion);
    }

    [Fact]
    public void GetMetadata_WithValidPeFile_ReturnsImports()
    {
        // Arrange
        var testExePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "EXEs", "ign_win", "Ign_win.exe");
        
        // Skip test if the file doesn't exist
        if (!File.Exists(testExePath))
        {
            return;
        }

        // Act
        var metadata = PeMetadataService.GetMetadata(testExePath);

        // Assert
        Assert.NotNull(metadata);
        Assert.NotEmpty(metadata.Imports);
        
        // Check that we have some common Windows DLL imports
        Assert.Contains(metadata.Imports, i => i.DllName.Contains("KERNEL32", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetMetadata_WithNonExistentFile_ReturnsNull()
    {
        // Arrange
        var nonExistentPath = "nonexistent.exe";

        // Act
        var metadata = PeMetadataService.GetMetadata(nonExistentPath);

        // Assert
        Assert.Null(metadata);
    }

    [Fact]
    public void GetMetadata_WithInvalidFile_ReturnsNull()
    {
        // Arrange - create a temporary invalid file
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "Not a valid PE file");

        try
        {
            // Act
            var metadata = PeMetadataService.GetMetadata(tempFile);

            // Assert
            Assert.Null(metadata);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
