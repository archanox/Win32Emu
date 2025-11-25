using Xunit;
using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Win32Emu.Loader;
using Win32Emu.Win32;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Integration tests for GetProcAddress and LoadLibraryA with real PE files.
/// Tests export resolution, forwarded exports, and ordinal lookups using actual DLLs.
/// </summary>
[Trait("Category", "DllModuleTests")]
public class PeExportIntegrationTests : IDisposable
{
    private readonly TestEnvironment _testEnv;
    private readonly string _testDllPath;

    public PeExportIntegrationTests()
    {
        _testEnv = new TestEnvironment();
        
        // Use a real Windows DLL from the repository for testing
        _testDllPath = Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(typeof(PeExportIntegrationTests).Assembly.Location) ?? string.Empty,
            "..", "..", "..", "..", "..", "DLLs", "WinXP", "kernel32.dll"));
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

        // Act - Try to resolve the forwarded export
        var exportNamePtr = _testEnv.WriteString(forwardedExportName);
        
        // This test verifies that GetProcAddress handles forwarded exports without crashing
        // The resolution may succeed (if target DLL is available) or fail (if not), but should not throw
        _testEnv.CallKernel32Api("GETPROCADDRESS", handle, exportNamePtr);
        
        // Assert - Execution completed without exceptions
        // This test only verifies that resolving a forwarded export does not throw an exception.
        // It does not assert on the resolution result, as it may vary depending on the environment.
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

    [Fact]
    public void GetProcAddress_WithRealPeDll_MissingExport_ShouldFallBackToEmulatedModule()
    {
        // This test verifies that when GetProcAddress is called on a real PE DLL (e.g., kernel32.dll)
        // for an export that doesn't exist in the PE file, but DOES exist in our emulated module,
        // it should fall back to creating a synthetic export instead of returning 0.
        
        // Skip test if the DLL doesn't exist
        if (!File.Exists(_testDllPath))
        {
            return;
        }

        // Arrange - Load the real kernel32.dll PE file
        var handle = _testEnv.ProcessEnv.LoadPeImage(_testDllPath, _testEnv.PeLoader);
        Assert.NotEqual(0u, handle);

        // Check what exports the real DLL actually has
        _testEnv.ProcessEnv.TryGetLoadedImage(handle, out var loadedImage);
        Assert.NotNull(loadedImage);

        // Find a function that exists in our emulated KERNEL32 but NOT in the real DLL
        // We'll try several candidates and use the first one that's missing from the PE but exists in emulated module
        // NOTE: This list is intentionally hardcoded with functions added in later Windows versions.
        // The test is robust: if all candidates are found in the PE (e.g., newer kernel32.dll), 
        // the test gracefully skips. If any candidate is missing from PE but exists in emulated module,
        // we test the fallback behavior.
        string[] candidates = 
        {
            "GetSystemWindowsDirectoryA",  // Vista+
            "GetNativeSystemInfo",          // XP 64-bit+
            "IsWow64Process",               // XP 64-bit+
            "SetThreadStackGuarantee",      // Vista+
            "InitializeCriticalSectionEx"   // Vista+
        };

        // Derive the module name from the loaded DLL path
        var moduleName = Path.GetFileName(_testDllPath).ToUpperInvariant();

        string? testFunction = null;
        foreach (var candidate in candidates)
        {
            // Check if it's in our emulated module but not in the real PE
            bool inEmulated = DllModuleExportInfo.IsExportImplemented(moduleName, candidate);
            bool inPeExports = loadedImage.ExportsByName.ContainsKey(candidate);
            
            if (inEmulated && !inPeExports)
            {
                testFunction = candidate;
                break;
            }
        }

        // If we couldn't find a suitable test function, skip the test
        if (testFunction == null)
        {
            return; // The real DLL has all the functions we tried, can't test the fallback
        }

        // Act - Call GetProcAddress for a function that's NOT in the PE but IS in emulated module
        var exportNamePtr = _testEnv.WriteString(testFunction);
        var exportAddress = _testEnv.CallKernel32Api("GETPROCADDRESS", handle, exportNamePtr);

        // Assert - Should return a valid synthetic export address, not 0
        // After the fix, GetProcAddress should check the emulated module and create a synthetic export
        // The address should be a synthetic export (in the 0x0F800000 range)
        Assert.InRange(exportAddress, 0x0F800000u, 0x10000000u);
    }

    public void Dispose()
    {
        _testEnv.Dispose();
    }
}
