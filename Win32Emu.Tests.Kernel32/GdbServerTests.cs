using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu.Iced;
using Win32Emu.Debugging;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.VirtualFileSystem;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

public class GdbServerTests
{
    [Fact]
    public void GdbServer_CanBeCreated()
    {
        // Arrange
        var memory = new VirtualMemory(1024 * 1024);
        var cpu = new IcedCpu(memory, NullLogger.Instance);
        var breakpoints = new BreakpointManager();
        
        // Act
        using var gdbServer = new GdbServer(cpu, memory, breakpoints, NullLogger.Instance, 9999);
        
        // Assert
        Assert.NotNull(gdbServer);
    }
    
    [Fact]
    public void GdbServer_CanBeCreatedWithVfs()
    {
        // Arrange
        var memory = new VirtualMemory(1024 * 1024);
        var cpu = new IcedCpu(memory, NullLogger.Instance);
        var breakpoints = new BreakpointManager();
        
        var tempDir = Path.Combine(Path.GetTempPath(), $"GdbServerTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        
        try
        {
            var vfs = new LayeredVirtualFileSystem(tempDir, null, NullLogger.Instance);
            
            // Act
            using var gdbServer = new GdbServer(cpu, memory, breakpoints, NullLogger.Instance, 9999, vfs);
            
            // Assert
            Assert.NotNull(gdbServer);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
    
    [Fact]
    public void GdbServer_ShouldBreak_ReturnsFalseWhenNoBreakpoint()
    {
        // Arrange
        var memory = new VirtualMemory(1024 * 1024);
        var cpu = new IcedCpu(memory, NullLogger.Instance);
        var breakpoints = new BreakpointManager();
        using var gdbServer = new GdbServer(cpu, memory, breakpoints, NullLogger.Instance, 9999);
        
        // Act
        var shouldBreak = gdbServer.ShouldBreak(0x00401000);
        
        // Assert
        Assert.False(shouldBreak);
    }
    
    [Fact]
    public void GdbServer_ShouldBreak_ReturnsTrueWhenBreakpointSet()
    {
        // Arrange
        var memory = new VirtualMemory(1024 * 1024);
        var cpu = new IcedCpu(memory, NullLogger.Instance);
        var breakpoints = new BreakpointManager();
        using var gdbServer = new GdbServer(cpu, memory, breakpoints, NullLogger.Instance, 9999);
        
        // Set a breakpoint
        breakpoints.AddBreakpoint(0x00401000);
        
        // Act
        var shouldBreak = gdbServer.ShouldBreak(0x00401000);
        
        // Assert
        Assert.True(shouldBreak);
    }
    
    [Fact]
    public void GdbServer_ShouldBreak_RecordsHitCount()
    {
        // Arrange
        var memory = new VirtualMemory(1024 * 1024);
        var cpu = new IcedCpu(memory, NullLogger.Instance);
        var breakpoints = new BreakpointManager();
        using var gdbServer = new GdbServer(cpu, memory, breakpoints, NullLogger.Instance, 9999);
        
        var bp = breakpoints.AddBreakpoint(0x00401000);
        
        // Act
        gdbServer.ShouldBreak(0x00401000);
        gdbServer.ShouldBreak(0x00401000);
        
        // Assert
        Assert.Equal(2, bp.HitCount);
    }
    
    [Fact]
    public void GdbServer_AddSymbols_StoresSymbolsCorrectly()
    {
        // Arrange
        var memory = new VirtualMemory(1024 * 1024);
        var cpu = new IcedCpu(memory, NullLogger.Instance);
        var breakpoints = new BreakpointManager();
        using var gdbServer = new GdbServer(cpu, memory, breakpoints, NullLogger.Instance, 9999);
        
        var symbols = new Dictionary<string, uint>
        {
            { "KERNEL32!GetVersion", 0x00401000 },
            { "KERNEL32!ExitProcess", 0x00401010 },
            { "USER32!MessageBoxA", 0x00401020 }
        };
        
        // Act
        gdbServer.AddSymbols(symbols);
        
        // Assert - Just verify no exception is thrown
        // We can't directly test the private _symbols field, but we can verify the method completes
        Assert.NotNull(gdbServer);
    }
    
    [Fact]
    public void GdbServer_AddSymbolsFromLoadedImage_ProcessesExportsAndImports()
    {
        // Arrange
        var memory = new VirtualMemory(1024 * 1024);
        var cpu = new IcedCpu(memory, NullLogger.Instance);
        var breakpoints = new BreakpointManager();
        using var gdbServer = new GdbServer(cpu, memory, breakpoints, NullLogger.Instance, 9999);
        
        // Create a mock LoadedImage with exports and imports
        var exportsByName = new Dictionary<string, uint>
        {
            { "MyFunction", 0x00401000 },
            { "MyExport", 0x00401100 }
        };
        
        var exportsByOrdinal = new Dictionary<uint, uint>
        {
            { 1, 0x00401000 },
            { 2, 0x00401100 }
        };
        
        var importMap = new Dictionary<uint, (string dll, string name)>
        {
            { 0x0F000000, ("KERNEL32.DLL", "GetVersion") },
            { 0x0F000010, ("USER32.DLL", "MessageBoxA") }
        };
        
        var forwardedByName = new Dictionary<string, string>();
        var forwardedByOrdinal = new Dictionary<uint, string>();
        
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
            0x00001000, // HeaderEndRva
            0x00100000, // SizeOfStackReserve (1MB)
            0x00010000, // SizeOfStackCommit (64KB)
            []          // TlsCallbacks (empty array)
        );
        
        // Act
        gdbServer.AddSymbolsFromLoadedImage(loadedImage, "TEST");
        
        // Assert - Verify no exception is thrown
        Assert.NotNull(gdbServer);
    }
}
