using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu.Iced;
using Win32Emu.Debugging;
using Win32Emu.Memory;
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
}
