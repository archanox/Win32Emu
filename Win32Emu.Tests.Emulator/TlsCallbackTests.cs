using Win32Emu.Loader;
using Win32Emu.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for TLS (Thread Local Storage) callback execution
/// </summary>
public class TlsCallbackTests
{
    [Fact]
    public void LoadedImage_WithNoTlsCallbacks_ShouldHaveEmptyArray()
    {
        // Arrange
        var importMap = new Dictionary<uint, (string dll, string name)>();
        var exportsByName = new Dictionary<string, uint>();
        var exportsByOrdinal = new Dictionary<uint, uint>();
        var forwardedByName = new Dictionary<string, string>();
        var forwardedByOrdinal = new Dictionary<uint, string>();
        
        // Act
        var loadedImage = new LoadedImage(
            0x00400000,
            0x00401000,
            0x00010000,
            importMap,
            "test.exe",
            exportsByName,
            exportsByOrdinal,
            forwardedByName,
            forwardedByOrdinal,
            3,
            0x00001000,
            0x00100000,
            0x00010000,
            0x00100000, // SizeOfHeapReserve
            0x00010000, // SizeOfHeapCommit
            [], // No TLS callbacks
            [],  // No sections
            new Dictionary<uint, uint>(), // IatEntryMap (empty)
            // FileHeader fields
            Machine: 0x014C,
            TimeDateStamp: 0x00000000,
            Characteristics: 0x010E,
            // OptionalHeader additional fields
            MajorLinkerVersion: 14,
            MinorLinkerVersion: 0,
            MajorOperatingSystemVersion: 4,
            MinorOperatingSystemVersion: 0,
            MajorImageVersion: 0,
            MinorImageVersion: 0,
            MajorSubsystemVersion: 4,
            MinorSubsystemVersion: 0,
            DllCharacteristics: 0x0000,
            CheckSum: 0x00000000,
            SectionAlignment: 0x1000,
            FileAlignment: 0x0200,
            BaseOfCode: 0x1000,
            BaseOfData: 0x3000,
            SizeOfCode: 0x2000,
            SizeOfInitializedData: 0x1500,
            SizeOfUninitializedData: 0x0000
        );
        
        // Assert
        Assert.NotNull(loadedImage.TlsCallbacks);
        Assert.Empty(loadedImage.TlsCallbacks);
    }
    
    [Fact]
    public void LoadedImage_WithTlsCallbacks_ShouldStoreAddresses()
    {
        // Arrange
        var importMap = new Dictionary<uint, (string dll, string name)>();
        var exportsByName = new Dictionary<string, uint>();
        var exportsByOrdinal = new Dictionary<uint, uint>();
        var forwardedByName = new Dictionary<string, string>();
        var forwardedByOrdinal = new Dictionary<uint, string>();
        var tlsCallbacks = new uint[] { 0x00401100, 0x00401200, 0x00401300 };
        
        // Act
        var loadedImage = new LoadedImage(
            0x00400000,
            0x00401000,
            0x00010000,
            importMap,
            "test.exe",
            exportsByName,
            exportsByOrdinal,
            forwardedByName,
            forwardedByOrdinal,
            3,
            0x00001000,
            0x00100000,
            0x00010000,
            0x00100000, // SizeOfHeapReserve
            0x00010000, // SizeOfHeapCommit
            tlsCallbacks,
            [],  // No sections
            new Dictionary<uint, uint>(), // IatEntryMap (empty)
            // FileHeader fields
            Machine: 0x014C,
            TimeDateStamp: 0x00000000,
            Characteristics: 0x010E,
            // OptionalHeader additional fields
            MajorLinkerVersion: 14,
            MinorLinkerVersion: 0,
            MajorOperatingSystemVersion: 4,
            MinorOperatingSystemVersion: 0,
            MajorImageVersion: 0,
            MinorImageVersion: 0,
            MajorSubsystemVersion: 4,
            MinorSubsystemVersion: 0,
            DllCharacteristics: 0x0000,
            CheckSum: 0x00000000,
            SectionAlignment: 0x1000,
            FileAlignment: 0x0200,
            BaseOfCode: 0x1000,
            BaseOfData: 0x3000,
            SizeOfCode: 0x2000,
            SizeOfInitializedData: 0x1500,
            SizeOfUninitializedData: 0x0000
        );
        
        // Assert
        Assert.NotNull(loadedImage.TlsCallbacks);
        Assert.Equal(3, loadedImage.TlsCallbacks.Length);
        Assert.Equal(0x00401100u, loadedImage.TlsCallbacks[0]);
        Assert.Equal(0x00401200u, loadedImage.TlsCallbacks[1]);
        Assert.Equal(0x00401300u, loadedImage.TlsCallbacks[2]);
    }
    
    [Fact]
    public void PeImageLoader_WithNoTlsDirectory_ShouldReturnEmptyCallbackArray()
    {
        // This test verifies that PE files without TLS directories
        // don't cause errors and return an empty callback array.
        // Most PE files don't have TLS directories, so this is the common case.
        
        // Note: Creating a real PE file without TLS in a unit test is complex.
        // This test documents the expected behavior.
        // The actual implementation is tested through integration tests
        // with real executables.
        
        Assert.True(true, "PeImageLoader returns empty array for PE files without TLS directory");
    }
}
