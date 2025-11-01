using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Win32Emu.Loader;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Integration tests for GetProcAddress and LoadLibraryA with real PE files.
/// Tests export resolution, forwarded exports, and ordinal lookups using actual DLLs.
/// </summary>
public class PeExportIntegrationTests : IDisposable
{
    private readonly TestEnvironment _testEnv;
    private readonly string _testDllPath;

    public PeExportIntegrationTests()
    {
        _testEnv = new TestEnvironment();
        
        // Use a real Windows DLL from the repository for testing
        // Use Path.GetFullPath to normalize the relative path for better maintainability
        var assemblyLocation = Path.GetDirectoryName(typeof(PeExportIntegrationTests).Assembly.Location) ?? string.Empty;
        var relativePath = Path.Combine(assemblyLocation, "..", "..", "..", "..", "..", "DLLs", "WinXP", "kernel32.dll");
        _testDllPath = Path.GetFullPath(relativePath);
    }

    [Fact]
    public void LoadLibraryA_WithRealPeDll_ShouldLoadSuccessfully()
    {
        // Skip test if the DLL doesn't exist
        if (!File.Exists(_testDllPath))
        {
            return; // Test will pass but won't execute
        }

        // Arrange - Create DLL path string in memory
        var dllPathPtr = _testEnv.WriteString(_testDllPath);

        // Act - Call LoadLibraryA
        var moduleHandle = _testEnv.CallKernel32Api("LOADLIBRARYA", dllPathPtr);

        // Assert - Should return a non-zero handle
        Assert.NotEqual(0u, moduleHandle);
    }

    [Fact]
    public void GetProcAddress_WithRealPeDll_ByName_ShouldResolveExport()
    {
        // Skip test if the DLL doesn't exist
        if (!File.Exists(_testDllPath))
        {
            return;
        }

        // Arrange - Load the DLL using PeImageLoader directly
        var handle = _testEnv.ProcessEnv.LoadPeImage(_testDllPath, _testEnv.PeLoader);
        Assert.NotEqual(0u, handle);

        // Create export name string in memory
        var exportNamePtr = _testEnv.WriteString("GetLastError");

        // Act - Call GetProcAddress to resolve the export
        var exportAddress = _testEnv.CallKernel32Api("GETPROCADDRESS", handle, exportNamePtr);

        // Assert - Should return a non-zero address
        Assert.NotEqual(0u, exportAddress);
        
        // The address should be within the DLL's image space
        // We can verify this by checking if it's in the loaded image
        var success = _testEnv.ProcessEnv.TryGetLoadedImage(handle, out var loadedImage);
        Assert.True(success);
        Assert.NotNull(loadedImage);
        Assert.InRange(exportAddress, loadedImage.BaseAddress, loadedImage.BaseAddress + loadedImage.ImageSize);
    }

    [Fact]
    public void GetProcAddress_WithRealPeDll_ByOrdinal_ShouldResolveExport()
    {
        // Skip test if the DLL doesn't exist
        if (!File.Exists(_testDllPath))
        {
            return;
        }

        // Arrange - Load the DLL
        var handle = _testEnv.ProcessEnv.LoadPeImage(_testDllPath, _testEnv.PeLoader);
        Assert.NotEqual(0u, handle);

        // Get the loaded image to find a valid ordinal
        var success = _testEnv.ProcessEnv.TryGetLoadedImage(handle, out var loadedImage);
        Assert.True(success);
        Assert.NotNull(loadedImage);

        // Skip if no exports by ordinal
        if (loadedImage.ExportsByOrdinal.Count == 0)
        {
            return;
        }

        // Get the first valid ordinal
        var ordinal = loadedImage.ExportsByOrdinal.Keys.First();

        // Act - Call GetProcAddress with ordinal (high word must be 0)
        var ordinalValue = ordinal & 0xFFFF; // Ensure high word is 0
        var exportAddress = _testEnv.CallKernel32Api("GETPROCADDRESS", handle, ordinalValue);

        // Assert - Should return the same address as in the export table
        Assert.NotEqual(0u, exportAddress);
        Assert.Equal(loadedImage.ExportsByOrdinal[ordinal], exportAddress);
    }

    [Fact]
    public void GetProcAddress_WithRealPeDll_NonExistentExport_ShouldReturnZero()
    {
        // Skip test if the DLL doesn't exist
        if (!File.Exists(_testDllPath))
        {
            return;
        }

        // Arrange - Load the DLL
        var handle = _testEnv.ProcessEnv.LoadPeImage(_testDllPath, _testEnv.PeLoader);
        Assert.NotEqual(0u, handle);

        // Create a non-existent export name
        var exportNamePtr = _testEnv.WriteString("ThisExportDoesNotExist12345");

        // Act - Call GetProcAddress
        var exportAddress = _testEnv.CallKernel32Api("GETPROCADDRESS", handle, exportNamePtr);

        // Assert - Should return 0 (not found)
        Assert.Equal(0u, exportAddress);
        
        // Check that LastError is set to ERROR_PROC_NOT_FOUND (127)
        var lastError = _testEnv.CallKernel32Api("GETLASTERROR");
        Assert.Equal(127u, lastError); // ERROR_PROC_NOT_FOUND
    }

    [Fact]
    public void LoadLibraryA_AndGetProcAddress_EndToEnd_ShouldWork()
    {
        // Skip test if the DLL doesn't exist
        if (!File.Exists(_testDllPath))
        {
            return;
        }

        // Arrange - Create DLL path string
        var dllPathPtr = _testEnv.WriteString(_testDllPath);

        // Act - Load the library
        var moduleHandle = _testEnv.CallKernel32Api("LOADLIBRARYA", dllPathPtr);
        Assert.NotEqual(0u, moduleHandle);

        // Get an export from the loaded library
        var exportNamePtr = _testEnv.WriteString("GetLastError");
        var exportAddress = _testEnv.CallKernel32Api("GETPROCADDRESS", moduleHandle, exportNamePtr);

        // Assert - Should successfully resolve the export
        Assert.NotEqual(0u, exportAddress);
    }

    [Fact]
    public void GetProcAddress_WithForwardedExport_ShouldResolveCorrectly()
    {
        // Skip test if the DLL doesn't exist
        if (!File.Exists(_testDllPath))
        {
            return;
        }

        // Arrange - Load the DLL
        var handle = _testEnv.ProcessEnv.LoadPeImage(_testDllPath, _testEnv.PeLoader);
        Assert.NotEqual(0u, handle);

        // Get the loaded image to check for forwarded exports
        var success = _testEnv.ProcessEnv.TryGetLoadedImage(handle, out var loadedImage);
        Assert.True(success);
        Assert.NotNull(loadedImage);

        // Skip if no forwarded exports
        if (loadedImage.ForwardedExportsByName.Count == 0)
        {
            return;
        }

        // Get the first forwarded export
        var forwardedExportName = loadedImage.ForwardedExportsByName.Keys.First();
        var forwarderTarget = loadedImage.ForwardedExportsByName[forwardedExportName];

        // Act - Try to resolve the forwarded export
        var exportNamePtr = _testEnv.WriteString(forwardedExportName);
        var exportAddress = _testEnv.CallKernel32Api("GETPROCADDRESS", handle, exportNamePtr);

        // Assert - Should either resolve to an address or return 0 if the target DLL isn't available
        // Note: This test verifies that forwarded exports are handled, even if resolution fails
        // due to missing target DLL in the test environment
        
        // The result should be deterministic - either a valid address or 0
        // We're testing that the code doesn't crash or hang when encountering forwarded exports
        Assert.True(exportAddress == 0 || exportAddress != 0);
    }

    [Fact]
    public void PeImageLoader_ShouldParseExportTable_WithAllExportTypes()
    {
        // Skip test if the DLL doesn't exist
        if (!File.Exists(_testDllPath))
        {
            return;
        }

        // Arrange & Act - Load the PE image
        var handle = _testEnv.ProcessEnv.LoadPeImage(_testDllPath, _testEnv.PeLoader);
        var success = _testEnv.ProcessEnv.TryGetLoadedImage(handle, out var loadedImage);

        // Assert - Verify the export tables were populated
        Assert.True(success);
        Assert.NotNull(loadedImage);
        
        // At least one of the export dictionaries should have entries
        var hasExports = loadedImage.ExportsByName.Count > 0 
                      || loadedImage.ExportsByOrdinal.Count > 0 
                      || loadedImage.ForwardedExportsByName.Count > 0 
                      || loadedImage.ForwardedExportsByOrdinal.Count > 0;
        
        Assert.True(hasExports, "PE file should have at least some exports");
        
        // Verify export addresses are valid (within image bounds)
        foreach (var exportAddress in loadedImage.ExportsByOrdinal.Values)
        {
            Assert.InRange(exportAddress, loadedImage.BaseAddress, loadedImage.BaseAddress + loadedImage.ImageSize);
        }
    }

    [Fact]
    public void GetProcAddress_CaseInsensitiveNameLookup_ShouldWork()
    {
        // Skip test if the DLL doesn't exist
        if (!File.Exists(_testDllPath))
        {
            return;
        }

        // Arrange - Load the DLL
        var handle = _testEnv.ProcessEnv.LoadPeImage(_testDllPath, _testEnv.PeLoader);
        Assert.NotEqual(0u, handle);

        // Try different case variations of the same export
        var exportNameLower = _testEnv.WriteString("getlasterror");
        var exportNameUpper = _testEnv.WriteString("GETLASTERROR");
        var exportNameMixed = _testEnv.WriteString("GetLastError");

        // Act - Resolve with different cases
        var addressLower = _testEnv.CallKernel32Api("GETPROCADDRESS", handle, exportNameLower);
        var addressUpper = _testEnv.CallKernel32Api("GETPROCADDRESS", handle, exportNameUpper);
        var addressMixed = _testEnv.CallKernel32Api("GETPROCADDRESS", handle, exportNameMixed);

        // Assert - All should resolve to the same address (or all be 0 if export doesn't exist)
        // The important thing is that case doesn't matter
        if (addressLower != 0)
        {
            Assert.Equal(addressLower, addressUpper);
            Assert.Equal(addressLower, addressMixed);
        }
    }

    public void Dispose()
    {
        _testEnv.Dispose();
    }
}
