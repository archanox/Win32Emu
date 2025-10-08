using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;

namespace Win32Emu.Debugging;

/// <summary>
/// Interactive debugger that provides step-through debugging capabilities
/// Similar to GDB but integrated directly into the emulator
/// </summary>
public class InteractiveDebugger
{
    private readonly IcedCpu _cpu;
    private readonly VirtualMemory _memory;
    private readonly BreakpointManager _breakpoints;
    private readonly EnhancedCpuDebugger _enhancedDebugger;
    
    private bool _isPaused = true;
    private bool _stepMode = false;
    private bool _shouldStop = false;
    private uint _lastStoppedEip = 0;
    
    private Queue<string>? _scriptCommands = null;
    private bool _scriptMode = false;

    public InteractiveDebugger(IcedCpu cpu, VirtualMemory memory, IEnumerable<string>? scriptCommands = null)
    {
        _cpu = cpu;
        _memory = memory;
        _breakpoints = new BreakpointManager();
        _enhancedDebugger = new EnhancedCpuDebugger(cpu, memory)
        {
            EnableSuspiciousRegisterDetection = true,
            LogToConsole = false,
            LogAllInstructions = false
        };
        
        if (scriptCommands != null)
        {
            _scriptCommands = new Queue<string>(scriptCommands);
            _scriptMode = true;
        }
    }

    /// <summary>
    /// Check if the debugger should break (either at a breakpoint or in step mode)
    /// </summary>
    public bool ShouldBreak(uint currentEip)
    {
        // Break if we're in step mode
        if (_stepMode && currentEip != _lastStoppedEip)
        {
            _stepMode = false;
            return true;
        }

        // Break if we hit a breakpoint
        if (_breakpoints.IsBreakpointAt(currentEip))
        {
            _breakpoints.RecordBreakpointHit(currentEip);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Enter the interactive debugger command loop
    /// </summary>
    public bool HandleBreak(uint eip, string? reason = null)
    {
        _isPaused = true;
        _lastStoppedEip = eip;

        if (reason != null)
        {
            Console.WriteLine($"\n{reason}");
        }

        var breakpoint = _breakpoints.GetBreakpointAt(eip);
        if (breakpoint != null)
        {
            Console.WriteLine($"Breakpoint {breakpoint.Id} hit at 0x{eip:X8} (hit count: {breakpoint.HitCount})");
        }
        else
        {
            Console.WriteLine($"Stopped at 0x{eip:X8}");
        }

        ShowRegisters();
        ShowCurrentInstruction(eip);

        while (_isPaused)
        {
            string? input;
            
            if (_scriptMode && _scriptCommands != null && _scriptCommands.Count > 0)
            {
                // Script mode: get next command from queue
                input = _scriptCommands.Dequeue();
                Console.WriteLine($"(dbg) {input}");
            }
            else if (_scriptMode)
            {
                // Script exhausted, quit
                input = "quit";
                Console.WriteLine($"(dbg) {input}");
            }
            else
            {
                // Interactive mode: read from user
                Console.Write("(dbg) ");
                input = Console.ReadLine()?.Trim();
            }
            
            if (string.IsNullOrEmpty(input))
            {
                input = "help";
            }

            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0].ToLowerInvariant();

            try
            {
                switch (command)
                {
                    case "c":
                    case "continue":
                        _isPaused = false;
                        _stepMode = false;
                        Console.WriteLine("Continuing...");
                        break;

                    case "s":
                    case "step":
                    case "stepi":
                        _isPaused = false;
                        _stepMode = true;
                        Console.WriteLine("Stepping one instruction...");
                        break;

                    case "n":
                    case "next":
                        // For now, next is the same as step (we don't step over calls)
                        _isPaused = false;
                        _stepMode = true;
                        Console.WriteLine("Stepping one instruction...");
                        break;

                    case "b":
                    case "break":
                    case "breakpoint":
                        HandleBreakpointCommand(parts);
                        break;

                    case "d":
                    case "delete":
                        HandleDeleteCommand(parts);
                        break;

                    case "disable":
                        HandleDisableCommand(parts);
                        break;

                    case "enable":
                        HandleEnableCommand(parts);
                        break;

                    case "i":
                    case "info":
                        HandleInfoCommand(parts);
                        break;

                    case "x":
                    case "examine":
                        HandleExamineCommand(parts);
                        break;

                    case "r":
                    case "registers":
                        ShowRegisters();
                        break;

                    case "disas":
                    case "disassemble":
                        HandleDisassembleCommand(parts);
                        break;

                    case "q":
                    case "quit":
                    case "exit":
                        Console.WriteLine("Quitting debugger...");
                        _shouldStop = true;
                        _isPaused = false;
                        break;

                    case "h":
                    case "help":
                    case "?":
                        ShowHelp();
                        break;

                    default:
                        Console.WriteLine($"Unknown command: {command}. Type 'help' for available commands.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing command: {ex.Message}");
            }
        }

        return !_shouldStop;
    }

    private void HandleBreakpointCommand(string[] parts)
    {
        if (parts.Length < 2)
        {
            Console.WriteLine("Usage: break <address>");
            Console.WriteLine("Example: break 0x401000");
            return;
        }

        var addressStr = parts[1];
        if (TryParseAddress(addressStr, out var address))
        {
            var bp = _breakpoints.AddBreakpoint(address);
            Console.WriteLine($"Breakpoint {bp.Id} set at 0x{address:X8}");
        }
        else
        {
            Console.WriteLine($"Invalid address: {addressStr}");
        }
    }

    private void HandleDeleteCommand(string[] parts)
    {
        if (parts.Length < 2)
        {
            Console.WriteLine("Usage: delete <breakpoint-id>");
            return;
        }

        if (uint.TryParse(parts[1], out var id))
        {
            if (_breakpoints.RemoveBreakpoint(id))
            {
                Console.WriteLine($"Breakpoint {id} deleted");
            }
            else
            {
                Console.WriteLine($"Breakpoint {id} not found");
            }
        }
        else
        {
            Console.WriteLine($"Invalid breakpoint ID: {parts[1]}");
        }
    }

    private void HandleDisableCommand(string[] parts)
    {
        if (parts.Length < 2)
        {
            Console.WriteLine("Usage: disable <breakpoint-id>");
            return;
        }

        if (uint.TryParse(parts[1], out var id))
        {
            if (_breakpoints.SetBreakpointEnabled(id, false))
            {
                Console.WriteLine($"Breakpoint {id} disabled");
            }
            else
            {
                Console.WriteLine($"Breakpoint {id} not found");
            }
        }
        else
        {
            Console.WriteLine($"Invalid breakpoint ID: {parts[1]}");
        }
    }

    private void HandleEnableCommand(string[] parts)
    {
        if (parts.Length < 2)
        {
            Console.WriteLine("Usage: enable <breakpoint-id>");
            return;
        }

        if (uint.TryParse(parts[1], out var id))
        {
            if (_breakpoints.SetBreakpointEnabled(id, true))
            {
                Console.WriteLine($"Breakpoint {id} enabled");
            }
            else
            {
                Console.WriteLine($"Breakpoint {id} not found");
            }
        }
        else
        {
            Console.WriteLine($"Invalid breakpoint ID: {parts[1]}");
        }
    }

    private void HandleInfoCommand(string[] parts)
    {
        if (parts.Length < 2)
        {
            Console.WriteLine("Usage: info <breakpoints|registers|stack>");
            return;
        }

        var subcommand = parts[1].ToLowerInvariant();
        switch (subcommand)
        {
            case "b":
            case "breakpoints":
                ShowBreakpoints();
                break;

            case "r":
            case "registers":
                ShowRegisters();
                break;

            case "s":
            case "stack":
                ShowStack();
                break;

            default:
                Console.WriteLine($"Unknown info subcommand: {subcommand}");
                break;
        }
    }

    private void HandleExamineCommand(string[] parts)
    {
        if (parts.Length < 2)
        {
            Console.WriteLine("Usage: x <address> [count]");
            Console.WriteLine("Example: x 0x401000 16");
            return;
        }

        if (!TryParseAddress(parts[1], out var address))
        {
            Console.WriteLine($"Invalid address: {parts[1]}");
            return;
        }

        var count = 16u;
        if (parts.Length > 2 && uint.TryParse(parts[2], out var parsedCount))
        {
            count = parsedCount;
        }

        try
        {
            Console.WriteLine($"Memory at 0x{address:X8}:");
            for (var i = 0u; i < count; i += 16)
            {
                var lineAddr = address + i;
                var bytesToRead = Math.Min(16u, count - i);
                var bytes = _memory.GetSpan(lineAddr, (int)bytesToRead);
                
                Console.Write($"0x{lineAddr:X8}: ");
                
                // Hex dump
                for (var j = 0; j < bytesToRead; j++)
                {
                    Console.Write($"{bytes[j]:X2} ");
                }
                
                // Padding
                for (var j = bytesToRead; j < 16; j++)
                {
                    Console.Write("   ");
                }
                
                Console.Write(" | ");
                
                // ASCII dump
                for (var j = 0; j < bytesToRead; j++)
                {
                    var c = (char)bytes[j];
                    Console.Write(char.IsControl(c) ? '.' : c);
                }
                
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cannot read memory at 0x{address:X8}: {ex.Message}");
        }
    }

    private void HandleDisassembleCommand(string[] parts)
    {
        var address = _cpu.GetEip();
        var count = 10u;

        if (parts.Length > 1)
        {
            if (!TryParseAddress(parts[1], out address))
            {
                Console.WriteLine($"Invalid address: {parts[1]}");
                return;
            }
        }

        if (parts.Length > 2 && uint.TryParse(parts[2], out var parsedCount))
        {
            count = parsedCount;
        }

        Console.WriteLine($"Disassembly at 0x{address:X8}:");
        try
        {
            for (var i = 0u; i < count; i++)
            {
                var bytes = _memory.GetSpan(address, 15);
                var hexStr = Convert.ToHexString(bytes);
                var displayLen = Math.Min(24, hexStr.Length);
                Console.WriteLine($"0x{address:X8}: {hexStr.AsSpan(0, displayLen)}");
                
                // Simple heuristic: most instructions are 1-7 bytes
                // For better disassembly, we'd need to properly decode
                address += 3; // Simple increment for demo
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cannot disassemble at 0x{address:X8}: {ex.Message}");
        }
    }

    private void ShowRegisters()
    {
        var eip = _cpu.GetEip();
        var eax = _cpu.GetRegister("EAX");
        var ebx = _cpu.GetRegister("EBX");
        var ecx = _cpu.GetRegister("ECX");
        var edx = _cpu.GetRegister("EDX");
        var esi = _cpu.GetRegister("ESI");
        var edi = _cpu.GetRegister("EDI");
        var ebp = _cpu.GetRegister("EBP");
        var esp = _cpu.GetRegister("ESP");
        var eflags = _cpu.GetRegister("EFLAGS");

        Console.WriteLine("Registers:");
        Console.WriteLine($"  EIP: 0x{eip:X8}");
        Console.WriteLine($"  EAX: 0x{eax:X8}  EBX: 0x{ebx:X8}  ECX: 0x{ecx:X8}  EDX: 0x{edx:X8}");
        Console.WriteLine($"  ESI: 0x{esi:X8}  EDI: 0x{edi:X8}  EBP: 0x{ebp:X8}  ESP: 0x{esp:X8}");
        Console.WriteLine($"  EFLAGS: 0x{eflags:X8}");
    }

    private void ShowStack()
    {
        var esp = _cpu.GetRegister("ESP");
        Console.WriteLine($"Stack (ESP=0x{esp:X8}):");
        
        try
        {
            for (var i = 0; i < 8; i++)
            {
                var addr = esp + (uint)(i * 4);
                var value = _memory.Read32(addr);
                Console.WriteLine($"  0x{addr:X8}: 0x{value:X8}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cannot read stack: {ex.Message}");
        }
    }

    private void ShowBreakpoints()
    {
        var breakpoints = _breakpoints.GetAllBreakpoints().ToList();
        
        if (breakpoints.Count == 0)
        {
            Console.WriteLine("No breakpoints set");
            return;
        }

        Console.WriteLine("Breakpoints:");
        foreach (var bp in breakpoints)
        {
            var status = bp.Enabled ? "enabled" : "disabled";
            Console.WriteLine($"  {bp.Id}: 0x{bp.Address:X8} ({status}, hit {bp.HitCount} time(s))");
        }
    }

    private void ShowCurrentInstruction(uint eip)
    {
        try
        {
            var bytes = _memory.GetSpan(eip, 15);
            var len = Math.Min(8, bytes.Length);
            var hexStr = Convert.ToHexString(bytes.ToArray(), 0, len);
            Console.WriteLine($"Next instruction at 0x{eip:X8}: {hexStr}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cannot read instruction at 0x{eip:X8}: {ex.Message}");
        }
    }

    private void ShowHelp()
    {
        Console.WriteLine("\nAvailable commands:");
        Console.WriteLine("  continue (c)           - Continue execution");
        Console.WriteLine("  step (s, stepi)        - Execute one instruction");
        Console.WriteLine("  next (n)               - Execute one instruction (same as step for now)");
        Console.WriteLine("  break (b) <addr>       - Set breakpoint at address");
        Console.WriteLine("  delete (d) <id>        - Delete breakpoint");
        Console.WriteLine("  disable <id>           - Disable breakpoint");
        Console.WriteLine("  enable <id>            - Enable breakpoint");
        Console.WriteLine("  info breakpoints       - List all breakpoints");
        Console.WriteLine("  info registers         - Show CPU registers");
        Console.WriteLine("  info stack             - Show stack contents");
        Console.WriteLine("  registers (r)          - Show CPU registers");
        Console.WriteLine("  examine (x) <addr> [n] - Examine memory at address");
        Console.WriteLine("  disassemble <addr> [n] - Disassemble instructions");
        Console.WriteLine("  quit (q, exit)         - Quit debugger");
        Console.WriteLine("  help (h, ?)            - Show this help");
        Console.WriteLine();
    }

    private static bool TryParseAddress(string addressStr, out uint address)
    {
        // Try to parse with 0x prefix
        if (addressStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return uint.TryParse(addressStr.Substring(2), NumberStyles.HexNumber, null, out address);
        }
        
        // Try as decimal
        if (uint.TryParse(addressStr, out address))
        {
            return true;
        }

        // Try as hex without prefix
        return uint.TryParse(addressStr, NumberStyles.HexNumber, null, out address);
    }

    public bool ShouldStop => _shouldStop;
}
