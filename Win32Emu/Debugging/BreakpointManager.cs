using System.Collections.Generic;
using System.Linq;

namespace Win32Emu.Debugging;

/// <summary>
/// Manages breakpoints for the debugger
/// </summary>
public class BreakpointManager
{
    private readonly Dictionary<uint, Breakpoint> _breakpoints = new();
    private uint _nextBreakpointId = 1;

    /// <summary>
    /// Add a breakpoint at the specified address
    /// </summary>
    public Breakpoint AddBreakpoint(uint address, string? description = null)
    {
        // Check if breakpoint already exists at this address
        var existing = _breakpoints.Values.FirstOrDefault(b => b.Address == address);
        if (existing != null)
        {
            return existing;
        }

        var breakpoint = new Breakpoint
        {
            Id = _nextBreakpointId++,
            Address = address,
            Description = description ?? $"Breakpoint at 0x{address:X8}",
            Enabled = true,
            HitCount = 0
        };

        _breakpoints[breakpoint.Id] = breakpoint;
        return breakpoint;
    }

    /// <summary>
    /// Remove a breakpoint by ID
    /// </summary>
    public bool RemoveBreakpoint(uint id)
    {
        return _breakpoints.Remove(id);
    }

    /// <summary>
    /// Remove a breakpoint by address
    /// </summary>
    public bool RemoveBreakpointAtAddress(uint address)
    {
        var breakpoint = _breakpoints.Values.FirstOrDefault(b => b.Address == address);
        if (breakpoint != null)
        {
            return _breakpoints.Remove(breakpoint.Id);
        }
        return false;
    }

    /// <summary>
    /// Enable or disable a breakpoint
    /// </summary>
    public bool SetBreakpointEnabled(uint id, bool enabled)
    {
        if (_breakpoints.TryGetValue(id, out var breakpoint))
        {
            breakpoint.Enabled = enabled;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Check if there's an enabled breakpoint at the specified address
    /// </summary>
    public bool IsBreakpointAt(uint address)
    {
        return _breakpoints.Values.Any(b => b.Address == address && b.Enabled);
    }

    /// <summary>
    /// Get the breakpoint at the specified address (if any)
    /// </summary>
    public Breakpoint? GetBreakpointAt(uint address)
    {
        return _breakpoints.Values.FirstOrDefault(b => b.Address == address && b.Enabled);
    }

    /// <summary>
    /// Record that a breakpoint was hit
    /// </summary>
    public void RecordBreakpointHit(uint address)
    {
        var breakpoint = GetBreakpointAt(address);
        if (breakpoint != null)
        {
            breakpoint.HitCount++;
        }
    }

    /// <summary>
    /// Get all breakpoints
    /// </summary>
    public IEnumerable<Breakpoint> GetAllBreakpoints()
    {
        return _breakpoints.Values;
    }

    /// <summary>
    /// Clear all breakpoints
    /// </summary>
    public void ClearAll()
    {
        _breakpoints.Clear();
    }
}

/// <summary>
/// Represents a breakpoint in the debugger
/// </summary>
public class Breakpoint
{
    public uint Id { get; init; }
    public uint Address { get; init; }
    public string Description { get; init; } = "";
    public bool Enabled { get; set; }
    public int HitCount { get; set; }
}
