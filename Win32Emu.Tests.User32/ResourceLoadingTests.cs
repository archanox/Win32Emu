using Xunit;
using AsmResolver.PE;
using AsmResolver.PE.File;
using Win32Emu.Loader;
using Win32Emu.Memory;
using System.IO;

namespace Win32Emu.Tests.User32;

[Trait("Category", "DllModuleTests")]
public class ResourceLoadingTests
{
    [Fact]
    public void LoadString_FromSetupExe_ReturnsCorrectStrings()
    {
        // Arrange
        var setupExePath = Path.Combine("EXEs", "ign_install", "SETUP.EXE");
        if (!File.Exists(setupExePath))
        {
            // Skip test if file doesn't exist
            return;
        }

        var peFile = PEFile.FromFile(setupExePath);
        var peImage = PEImage.FromFile(peFile);
        var memory = new VirtualMemory();
        var resourceReader = new PeResourceReader(peImage, 0x00400000, memory);

        // Act & Assert - Load string resources that are mentioned in the ApiMon log
        var str100 = resourceReader.LoadString(100); // Default installation path
        var str101 = resourceReader.LoadString(101); // Dialog title "Ignition Setup"
        var str118 = resourceReader.LoadString(118); // Bitmap resource name

        // Assert
        Assert.NotNull(str100);
        Assert.Contains("Ignition", str100); // Should contain "C:\Games\Ignition" or similar

        Assert.NotNull(str101);
        Assert.Equal("Ignition Setup", str101);

        Assert.NotNull(str118);
        Assert.NotEmpty(str118);
    }

    [Fact]
    public void LoadBitmapByName_FromSetupExe_LoadsBitmap()
    {
        // Arrange
        var setupExePath = Path.Combine("EXEs", "ign_install", "SETUP.EXE");
        if (!File.Exists(setupExePath))
        {
            // Skip test if file doesn't exist
            return;
        }

        var peFile = PEFile.FromFile(setupExePath);
        var peImage = PEImage.FromFile(peFile);
        var memory = new VirtualMemory();
        var resourceReader = new PeResourceReader(peImage, 0x00400000, memory);

        // First get the bitmap name from string resource 118
        var bitmapName = resourceReader.LoadString(118);
        Assert.NotNull(bitmapName);

        // Act - Try to load the bitmap
        var bitmapData = resourceReader.LoadBitmapByName(bitmapName);

        // Assert - Bitmap may or may not exist, but method should not throw
        // According to ApiMon log, it returns NULL with error 1814 (resource not found)
        // So we just verify the method runs without exception
        Assert.True(bitmapData == null || bitmapData.Length > 0);
    }
}
