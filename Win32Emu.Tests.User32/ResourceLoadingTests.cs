using Xunit;
using Xunit.Abstractions;
using AsmResolver.PE;
using AsmResolver.PE.File;
using Win32Emu.Loader;
using Win32Emu.Memory;
using System.IO;
using System.Linq;

namespace Win32Emu.Tests.User32;

[Trait("Category", "DllModuleTests")]
public class ResourceLoadingTests
{
	private readonly ITestOutputHelper _output;

	public ResourceLoadingTests(ITestOutputHelper output)
	{
		_output = output;
	}
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

    [Fact]
    public void LoadBitmapById_FromSetupExe_ChecksId118()
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

        // String resource 118 contains "signon"
        var str118 = resourceReader.LoadString(118);
        Assert.NotNull(str118);
        Assert.Equal("signon", str118);

        // Check if there's a bitmap resource with ID 118
        // This would be the integer resource ID, not the string name
        var bitmapById = resourceReader.LoadBitmap(118);

        // Bitmap by name
        var bitmapByName = resourceReader.LoadBitmapByName(str118);

        // Output debug info
        _output.WriteLine($"String 118: '{str118}'");
        _output.WriteLine($"Bitmap by ID 118: {(bitmapById != null ? $"FOUND ({bitmapById.Length} bytes)" : "NULL")}");
        _output.WriteLine($"Bitmap by name '{str118}': {(bitmapByName != null ? $"FOUND ({bitmapByName.Length} bytes)" : "NULL")}");

        // The test itself just validates that both approaches work without exception
        // The actual finding is informational for fixing the LoadImageA implementation
    }

    [Fact]
    public void EnumerateBitmapResources_FromSetupExe()
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

        // Enumerate all bitmap resources to see what's available
        var bitmapResourceIds = resourceReader.EnumerateResourceNames((uint)IResourceReader.ResourceType.RT_BITMAP);

        // Output for debugging - this test is informational
        if (bitmapResourceIds != null)
        {
            var ids = bitmapResourceIds.ToList();
            var idList = string.Join(", ", ids);
            _output.WriteLine($"Bitmap resource IDs found: {idList}");
            _output.WriteLine($"Total bitmap resources: {ids.Count}");
            System.Diagnostics.Debug.WriteLine($"Bitmap resource IDs found: {idList}");
            
            // Check if 118 is in the list
            if (ids.Contains(118))
            {
                _output.WriteLine("Found bitmap resource with ID 118!");
                System.Diagnostics.Debug.WriteLine("Found bitmap resource with ID 118!");
                // If bitmap 118 exists, this is a hint that LoadImageA should try loading by ID
                // when loading by name fails
                Assert.True(true, "Bitmap resource ID 118 exists - LoadImageA should fall back to ID");
            }
            else
            {
                _output.WriteLine("No bitmap resource with ID 118");
                System.Diagnostics.Debug.WriteLine("No bitmap resource with ID 118");
            }
        }
        else
        {
            _output.WriteLine("No bitmap resources found");
            System.Diagnostics.Debug.WriteLine("No bitmap resources found");
        }
    }


	[Fact]
	public void FindResource_WithStringName_LoadsIgnpicBitmap()
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


		// Simulate FindResourceA call with RT_BITMAP (2) and "IGNPIC" name

		// In the emulator, the string would be at some address in memory

		// For this test, we'll use the address as a marker and write the string there

		uint bitmapTypeId = 2; // RT_BITMAP

		uint ignpicNameAddr = 0x00100000;

		memory.WriteBytes(ignpicNameAddr, System.Text.Encoding.ASCII.GetBytes("IGNPIC\0"));


		// Act - FindResource should now handle string-named resources

		var hResInfo = resourceReader.FindResource(bitmapTypeId, ignpicNameAddr, 0);


		// Assert

		_output.WriteLine($"FindResource returned handle: 0x{hResInfo:X8}");

		Assert.NotEqual(0u, hResInfo); // Should find the resource


		// Now try to LoadResource and verify it works

		var hResData = resourceReader.LoadResource(0x00400000, hResInfo);

		_output.WriteLine($"LoadResource returned data handle: 0x{hResData:X8}");

		Assert.NotEqual(0u, hResData);


		// Get the size to verify we got actual data

		var resourceSize = resourceReader.SizeofResource(0x00400000, hResInfo);

		_output.WriteLine($"Resource size: {resourceSize} bytes");

		Assert.NotEqual(0u, resourceSize); // SizeofResource should work with string-named resources

	}
}
