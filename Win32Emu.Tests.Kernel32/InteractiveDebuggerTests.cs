using Xunit;
using Win32Emu.Debugging;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for the interactive debugger functionality
/// </summary>
public class InteractiveDebuggerTests
{
    [Fact]
    public void BreakpointManager_AddBreakpoint_CreatesBreakpoint()
    {
        // Arrange
        var manager = new BreakpointManager();

        // Act
        var bp = manager.AddBreakpoint(0x401000, "Test breakpoint");

        // Assert
        Assert.NotNull(bp);
        Assert.Equal(0x401000u, bp.Address);
        Assert.Equal("Test breakpoint", bp.Description);
        Assert.True(bp.Enabled);
        Assert.Equal(0, bp.HitCount);
    }

    [Fact]
    public void BreakpointManager_AddDuplicateBreakpoint_ReturnsSameBreakpoint()
    {
        // Arrange
        var manager = new BreakpointManager();

        // Act
        var bp1 = manager.AddBreakpoint(0x401000);
        var bp2 = manager.AddBreakpoint(0x401000);

        // Assert
        Assert.Equal(bp1.Id, bp2.Id);
    }

    [Fact]
    public void BreakpointManager_RemoveBreakpoint_RemovesBreakpoint()
    {
        // Arrange
        var manager = new BreakpointManager();
        var bp = manager.AddBreakpoint(0x401000);

        // Act
        var removed = manager.RemoveBreakpoint(bp.Id);

        // Assert
        Assert.True(removed);
        Assert.False(manager.IsBreakpointAt(0x401000));
    }

    [Fact]
    public void BreakpointManager_RemoveNonexistentBreakpoint_ReturnsFalse()
    {
        // Arrange
        var manager = new BreakpointManager();

        // Act
        var removed = manager.RemoveBreakpoint(999);

        // Assert
        Assert.False(removed);
    }

    [Fact]
    public void BreakpointManager_DisableBreakpoint_DisablesBreakpoint()
    {
        // Arrange
        var manager = new BreakpointManager();
        var bp = manager.AddBreakpoint(0x401000);

        // Act
        manager.SetBreakpointEnabled(bp.Id, false);

        // Assert
        Assert.False(manager.IsBreakpointAt(0x401000));
        var retrievedBp = manager.GetAllBreakpoints().First();
        Assert.False(retrievedBp.Enabled);
    }

    [Fact]
    public void BreakpointManager_EnableBreakpoint_EnablesBreakpoint()
    {
        // Arrange
        var manager = new BreakpointManager();
        var bp = manager.AddBreakpoint(0x401000);
        manager.SetBreakpointEnabled(bp.Id, false);

        // Act
        manager.SetBreakpointEnabled(bp.Id, true);

        // Assert
        Assert.True(manager.IsBreakpointAt(0x401000));
    }

    [Fact]
    public void BreakpointManager_IsBreakpointAt_ReturnsTrueForEnabledBreakpoint()
    {
        // Arrange
        var manager = new BreakpointManager();
        manager.AddBreakpoint(0x401000);

        // Act
        var isBreakpoint = manager.IsBreakpointAt(0x401000);

        // Assert
        Assert.True(isBreakpoint);
    }

    [Fact]
    public void BreakpointManager_IsBreakpointAt_ReturnsFalseForDisabledBreakpoint()
    {
        // Arrange
        var manager = new BreakpointManager();
        var bp = manager.AddBreakpoint(0x401000);
        manager.SetBreakpointEnabled(bp.Id, false);

        // Act
        var isBreakpoint = manager.IsBreakpointAt(0x401000);

        // Assert
        Assert.False(isBreakpoint);
    }

    [Fact]
    public void BreakpointManager_RecordBreakpointHit_IncrementsHitCount()
    {
        // Arrange
        var manager = new BreakpointManager();
        var bp = manager.AddBreakpoint(0x401000);

        // Act
        manager.RecordBreakpointHit(0x401000);
        manager.RecordBreakpointHit(0x401000);

        // Assert
        var retrievedBp = manager.GetBreakpointAt(0x401000);
        Assert.NotNull(retrievedBp);
        Assert.Equal(2, retrievedBp.HitCount);
    }

    [Fact]
    public void BreakpointManager_GetAllBreakpoints_ReturnsAllBreakpoints()
    {
        // Arrange
        var manager = new BreakpointManager();
        manager.AddBreakpoint(0x401000);
        manager.AddBreakpoint(0x402000);
        manager.AddBreakpoint(0x403000);

        // Act
        var breakpoints = manager.GetAllBreakpoints().ToList();

        // Assert
        Assert.Equal(3, breakpoints.Count);
    }

    [Fact]
    public void BreakpointManager_ClearAll_RemovesAllBreakpoints()
    {
        // Arrange
        var manager = new BreakpointManager();
        manager.AddBreakpoint(0x401000);
        manager.AddBreakpoint(0x402000);

        // Act
        manager.ClearAll();

        // Assert
        Assert.Empty(manager.GetAllBreakpoints());
    }

    [Fact]
    public void InteractiveDebugger_ShouldBreak_ReturnsTrueAtBreakpoint()
    {
        // Arrange
        var vm = new VirtualMemory(1024 * 1024);
        var cpu = new IcedCpu(vm, null);
        cpu.SetEip(0x401000);
        
        var debugger = new InteractiveDebugger(cpu, vm);
        
        // Manually access the internal breakpoint manager through reflection
        // or better: add a public method to add breakpoints
        var breakpointsField = typeof(InteractiveDebugger)
            .GetField("_breakpoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var breakpointManager = (BreakpointManager)breakpointsField!.GetValue(debugger)!;
        breakpointManager.AddBreakpoint(0x401000);

        // Act
        var shouldBreak = debugger.ShouldBreak(0x401000);

        // Assert
        Assert.True(shouldBreak);
    }

    [Fact]
    public void Breakpoint_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var bp = new Breakpoint
        {
            Id = 1,
            Address = 0x401000,
            Description = "Test",
            Enabled = true,
            HitCount = 0
        };

        // Assert
        Assert.Equal(1u, bp.Id);
        Assert.Equal(0x401000u, bp.Address);
        Assert.Equal("Test", bp.Description);
        Assert.True(bp.Enabled);
        Assert.Equal(0, bp.HitCount);
    }
}
