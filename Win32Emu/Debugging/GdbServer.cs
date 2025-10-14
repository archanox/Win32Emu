using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;

namespace Win32Emu.Debugging;

/// <summary>
/// GDB Remote Serial Protocol server for integration with Ghidra, IDA, and other debuggers
/// Implements a subset of the GDB Remote Serial Protocol sufficient for step-through debugging
/// </summary>
public class GdbServer : IDisposable
{
    private readonly IcedCpu _cpu;
    private readonly VirtualMemory _memory;
    private readonly BreakpointManager _breakpoints;
    private readonly ILogger _logger;
    private readonly int _port;
    
    private TcpListener? _listener;
    private TcpClient? _client;
    private NetworkStream? _stream;
    private bool _isRunning;
    private bool _shouldStop;
    private bool _noAckMode;
    
    // Execution state
    private bool _continueExecution;
    private bool _singleStep;
    private uint _lastStoppedEip;
    
    public GdbServer(IcedCpu cpu, VirtualMemory memory, BreakpointManager breakpoints, ILogger logger, int port = 1234)
    {
        _cpu = cpu;
        _memory = memory;
        _breakpoints = breakpoints;
        _logger = logger;
        _port = port;
    }
    
    /// <summary>
    /// Start the GDB server and wait for a connection
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _listener = new TcpListener(IPAddress.Loopback, _port);
        _listener.Start();
        _isRunning = true;
        
        _logger.LogInformation("GDB server listening on port {Port}", _port);
        _logger.LogInformation("Connect with: target remote localhost:{Port}", _port);
        
        _client = await _listener.AcceptTcpClientAsync(cancellationToken);
        _stream = _client.GetStream();
        
        _logger.LogInformation("GDB client connected from {RemoteEndPoint}", _client.Client.RemoteEndPoint);
    }
    
    /// <summary>
    /// Check if the debugger should break
    /// </summary>
    public bool ShouldBreak(uint currentEip)
    {
        // Break if single stepping
        if (_singleStep && currentEip != _lastStoppedEip)
        {
            _singleStep = false;
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
    /// Handle a break event - process GDB commands until continue/step
    /// </summary>
    public async Task<bool> HandleBreakAsync(uint eip, string? reason = null)
    {
        if (_stream == null || _client == null)
        {
            return false;
        }
        
        _lastStoppedEip = eip;
        _continueExecution = false;
        
        // Send stop reply
        var signal = 5; // SIGTRAP
        await SendPacketAsync($"S{signal:X2}");
        
        // Process commands until continue/step
        while (!_continueExecution && !_shouldStop && _client.Connected)
        {
            var packet = await ReceivePacketAsync();
            if (packet == null)
            {
                break;
            }
            
            await HandlePacketAsync(packet);
        }
        
        return !_shouldStop;
    }
    
    /// <summary>
    /// Receive a GDB protocol packet
    /// </summary>
    private async Task<string?> ReceivePacketAsync()
    {
        if (_stream == null)
        {
            return null;
        }
        
        var buffer = new byte[4096];
        var packetData = new StringBuilder();
        var inPacket = false;
        
        while (true)
        {
            var bytesRead = await _stream.ReadAsync(buffer);
            if (bytesRead == 0)
            {
                return null; // Connection closed
            }
            
            for (var i = 0; i < bytesRead; i++)
            {
                var ch = (char)buffer[i];
                
                if (ch == '$')
                {
                    inPacket = true;
                    packetData.Clear();
                }
                else if (ch == '#' && inPacket)
                {
                    // Read checksum (2 hex digits)
                    if (i + 2 <= bytesRead)
                    {
                        var checksumStr = Encoding.ASCII.GetString(buffer, i + 1, 2);
                        var packet = packetData.ToString();
                        
                        // Send ACK if not in no-ack mode
                        if (!_noAckMode)
                        {
                            await _stream.WriteAsync(new byte[] { (byte)'+' });
                            await _stream.FlushAsync();
                        }
                        
                        return packet;
                    }
                }
                else if (inPacket)
                {
                    packetData.Append(ch);
                }
                else if (ch is '+' or '-')
                {
                    // ACK/NACK from client - ignore
                }
                else if (ch == '\x03')
                {
                    // Ctrl+C interrupt
                    return "?";
                }
            }
        }
    }
    
    /// <summary>
    /// Send a GDB protocol packet
    /// </summary>
    private async Task SendPacketAsync(string data)
    {
        if (_stream == null)
        {
            return;
        }
        
        // Calculate checksum
        var checksum = 0;
        foreach (var ch in data)
        {
            checksum += ch;
        }
        checksum &= 0xFF;
        
        var packet = $"${data}#{checksum:X2}";
        var bytes = Encoding.ASCII.GetBytes(packet);
        
        await _stream.WriteAsync(bytes);
        await _stream.FlushAsync();
        
        // Wait for ACK if not in no-ack mode
        if (!_noAckMode)
        {
            var ack = new byte[1];
            await _stream.ReadAsync(ack);
            // Could check for '+' here, but we'll be lenient
        }
    }
    
    /// <summary>
    /// Handle a GDB protocol packet
    /// </summary>
    private async Task HandlePacketAsync(string packet)
    {
        if (string.IsNullOrEmpty(packet))
        {
            return;
        }
        
        // Remove sequence number if present (e.g., "qSupported" might be sent as "100:qSupported")
        var colonIndex = packet.IndexOf(':');
        if (colonIndex > 0 && int.TryParse(packet[..colonIndex], out _))
        {
            packet = packet[(colonIndex + 1)..];
        }
        
        var command = packet[0];
        var args = packet.Length > 1 ? packet[1..] : "";
        
        switch (command)
        {
            case '?':
                // Report why the target halted
                await SendPacketAsync("S05"); // SIGTRAP
                break;
                
            case 'g':
                // Read general registers
                await HandleReadRegistersAsync();
                break;
                
            case 'G':
                // Write general registers
                await HandleWriteRegistersAsync(args);
                break;
                
            case 'p':
                // Read single register
                await HandleReadRegisterAsync(args);
                break;
                
            case 'P':
                // Write single register
                await HandleWriteRegisterAsync(args);
                break;
                
            case 'm':
                // Read memory
                await HandleReadMemoryAsync(args);
                break;
                
            case 'M':
                // Write memory
                await HandleWriteMemoryAsync(args);
                break;
                
            case 'c':
                // Continue
                _continueExecution = true;
                _singleStep = false;
                break;
                
            case 's':
                // Single step
                _continueExecution = true;
                _singleStep = true;
                break;
                
            case 'Z':
                // Insert breakpoint
                await HandleInsertBreakpointAsync(args);
                break;
                
            case 'z':
                // Remove breakpoint
                await HandleRemoveBreakpointAsync(args);
                break;
                
            case 'q':
                // Query
                await HandleQueryAsync(args);
                break;
                
            case 'Q':
                // Set
                await HandleSetAsync(args);
                break;
                
            case 'v':
                // v packets
                await HandleVPacketAsync(args);
                break;
                
            case 'H':
                // Set thread for subsequent operations
                await SendPacketAsync("OK");
                break;
                
            case 'k':
                // Kill
                _shouldStop = true;
                _continueExecution = true;
                break;
                
            case 'D':
                // Detach
                await SendPacketAsync("OK");
                _shouldStop = true;
                _continueExecution = true;
                break;
                
            default:
                // Unsupported command
                await SendPacketAsync("");
                break;
        }
    }
    
    /// <summary>
    /// Read all general purpose registers
    /// Format: EAX, ECX, EDX, EBX, ESP, EBP, ESI, EDI, EIP, EFLAGS, CS, SS, DS, ES, FS, GS
    /// </summary>
    private async Task HandleReadRegistersAsync()
    {
        var regs = new StringBuilder();
        
        // General purpose registers
        regs.Append(ToHex32(_cpu.GetRegister("EAX")));
        regs.Append(ToHex32(_cpu.GetRegister("ECX")));
        regs.Append(ToHex32(_cpu.GetRegister("EDX")));
        regs.Append(ToHex32(_cpu.GetRegister("EBX")));
        regs.Append(ToHex32(_cpu.GetRegister("ESP")));
        regs.Append(ToHex32(_cpu.GetRegister("EBP")));
        regs.Append(ToHex32(_cpu.GetRegister("ESI")));
        regs.Append(ToHex32(_cpu.GetRegister("EDI")));
        
        // EIP
        regs.Append(ToHex32(_cpu.GetEip()));
        
        // EFLAGS
        regs.Append(ToHex32(_cpu.GetRegister("EFLAGS")));
        
        // Segment registers (stub values)
        for (var i = 0; i < 6; i++)
        {
            regs.Append("00000000");
        }
        
        await SendPacketAsync(regs.ToString());
    }
    
    /// <summary>
    /// Read a single register by index
    /// </summary>
    private async Task HandleReadRegisterAsync(string args)
    {
        if (!int.TryParse(args, NumberStyles.HexNumber, null, out var regNum))
        {
            await SendPacketAsync("E01");
            return;
        }
        
        var value = regNum switch
        {
            0 => _cpu.GetRegister("EAX"),
            1 => _cpu.GetRegister("ECX"),
            2 => _cpu.GetRegister("EDX"),
            3 => _cpu.GetRegister("EBX"),
            4 => _cpu.GetRegister("ESP"),
            5 => _cpu.GetRegister("EBP"),
            6 => _cpu.GetRegister("ESI"),
            7 => _cpu.GetRegister("EDI"),
            8 => _cpu.GetEip(),
            9 => _cpu.GetRegister("EFLAGS"),
            _ => 0u
        };
        
        await SendPacketAsync(ToHex32(value));
    }
    
    /// <summary>
    /// Read memory: m addr,length
    /// </summary>
    private async Task HandleReadMemoryAsync(string args)
    {
        var parts = args.Split(',');
        if (parts.Length != 2)
        {
            await SendPacketAsync("E01");
            return;
        }
        
        if (!uint.TryParse(parts[0], NumberStyles.HexNumber, null, out var addr) ||
            !uint.TryParse(parts[1], NumberStyles.HexNumber, null, out var length))
        {
            await SendPacketAsync("E01");
            return;
        }
        
        try
        {
            var data = new StringBuilder();
            for (uint i = 0; i < length; i++)
            {
                var b = _memory.Read8(addr + i);
                data.Append($"{b:X2}");
            }
            await SendPacketAsync(data.ToString());
        }
        catch (IndexOutOfRangeException ex)
        {
            // Memory access out of bounds
            _logger.LogWarning("GDB memory read failed: {Message} (addr=0x{Address:X8}, length={Length})", 
                ex.Message, addr, length);
            await SendPacketAsync("E01");
        }
        catch
        {
            await SendPacketAsync("E01");
        }
    }
    
    /// <summary>
    /// Write all general purpose registers: G XX...
    /// Format: EAX, ECX, EDX, EBX, ESP, EBP, ESI, EDI, EIP, EFLAGS, CS, SS, DS, ES, FS, GS
    /// </summary>
    private async Task HandleWriteRegistersAsync(string args)
    {
        // Each register is 4 bytes (8 hex digits) in little-endian format
        // We expect 16 registers: 10 GPRs + 6 segment registers
        if (args.Length < 80) // 10 registers * 8 hex chars = 80 minimum
        {
            await SendPacketAsync("E01");
            return;
        }
        
        try
        {
            var offset = 0;
            _cpu.SetRegister("EAX", ParseHex32(args, ref offset));
            _cpu.SetRegister("ECX", ParseHex32(args, ref offset));
            _cpu.SetRegister("EDX", ParseHex32(args, ref offset));
            _cpu.SetRegister("EBX", ParseHex32(args, ref offset));
            _cpu.SetRegister("ESP", ParseHex32(args, ref offset));
            _cpu.SetRegister("EBP", ParseHex32(args, ref offset));
            _cpu.SetRegister("ESI", ParseHex32(args, ref offset));
            _cpu.SetRegister("EDI", ParseHex32(args, ref offset));
            _cpu.SetEip(ParseHex32(args, ref offset));
            _cpu.SetRegister("EFLAGS", ParseHex32(args, ref offset));
            
            // Segment registers are ignored (we don't emulate them)
            
            _logger.LogDebug("GDB: Registers updated via G command");
            await SendPacketAsync("OK");
        }
        catch
        {
            await SendPacketAsync("E01");
        }
    }
    
    /// <summary>
    /// Write a single register: P n=XX...
    /// </summary>
    private async Task HandleWriteRegisterAsync(string args)
    {
        var parts = args.Split('=');
        if (parts.Length != 2)
        {
            await SendPacketAsync("E01");
            return;
        }
        
        if (!int.TryParse(parts[0], NumberStyles.HexNumber, null, out var regNum))
        {
            await SendPacketAsync("E01");
            return;
        }
        
        try
        {
            var offset = 0;
            var value = ParseHex32(parts[1], ref offset);
            
            switch (regNum)
            {
                case 0: _cpu.SetRegister("EAX", value); break;
                case 1: _cpu.SetRegister("ECX", value); break;
                case 2: _cpu.SetRegister("EDX", value); break;
                case 3: _cpu.SetRegister("EBX", value); break;
                case 4: _cpu.SetRegister("ESP", value); break;
                case 5: _cpu.SetRegister("EBP", value); break;
                case 6: _cpu.SetRegister("ESI", value); break;
                case 7: _cpu.SetRegister("EDI", value); break;
                case 8: _cpu.SetEip(value); break;
                case 9: _cpu.SetRegister("EFLAGS", value); break;
                default:
                    // Unsupported register (segment registers, etc.)
                    await SendPacketAsync("E01");
                    return;
            }
            
            _logger.LogDebug("GDB: Register {RegNum} updated to 0x{Value:X8}", regNum, value);
            await SendPacketAsync("OK");
        }
        catch
        {
            await SendPacketAsync("E01");
        }
    }
    
    /// <summary>
    /// Write memory: M addr,length:XX...
    /// </summary>
    private async Task HandleWriteMemoryAsync(string args)
    {
        var parts = args.Split(':');
        if (parts.Length != 2)
        {
            await SendPacketAsync("E01");
            return;
        }
        
        var addrLen = parts[0].Split(',');
        if (addrLen.Length != 2)
        {
            await SendPacketAsync("E01");
            return;
        }
        
        if (!uint.TryParse(addrLen[0], NumberStyles.HexNumber, null, out var addr) ||
            !uint.TryParse(addrLen[1], NumberStyles.HexNumber, null, out var length))
        {
            await SendPacketAsync("E01");
            return;
        }
        
        var data = parts[1];
        if (data.Length != length * 2)
        {
            await SendPacketAsync("E01");
            return;
        }
        
        try
        {
            for (uint i = 0; i < length; i++)
            {
                var byteStr = data.Substring((int)(i * 2), 2);
                var byteVal = byte.Parse(byteStr, NumberStyles.HexNumber);
                _memory.Write8(addr + i, byteVal);
            }
            
            _logger.LogDebug("GDB: Memory written at 0x{Address:X8}, {Length} bytes", addr, length);
            await SendPacketAsync("OK");
        }
        catch (IndexOutOfRangeException ex)
        {
            _logger.LogWarning("GDB memory write failed: {Message} (addr=0x{Address:X8}, length={Length})", 
                ex.Message, addr, length);
            await SendPacketAsync("E01");
        }
        catch
        {
            await SendPacketAsync("E01");
        }
    }
    
    /// <summary>
    /// Insert breakpoint: Z type,addr,kind
    /// Types: 0=software, 1=hardware, 2=write watchpoint, 3=read watchpoint, 4=access watchpoint
    /// </summary>
    private async Task HandleInsertBreakpointAsync(string args)
    {
        var parts = args.Split(',');
        if (parts.Length < 3)
        {
            await SendPacketAsync("E01");
            return;
        }
        
        if (!int.TryParse(parts[0], out var type) ||
            !uint.TryParse(parts[1], NumberStyles.HexNumber, null, out var addr) ||
            !uint.TryParse(parts[2], NumberStyles.HexNumber, null, out var kind))
        {
            await SendPacketAsync("E01");
            return;
        }
        
        switch (type)
        {
            case 0: // Software breakpoint
            case 1: // Hardware breakpoint (treat same as software)
                _breakpoints.AddBreakpoint(addr, $"GDB breakpoint at 0x{addr:X8}");
                _logger.LogDebug("GDB: Breakpoint added at 0x{Address:X8}", addr);
                await SendPacketAsync("OK");
                break;
                
            case 2: // Write watchpoint
                _breakpoints.AddWatchpoint(addr, WatchpointType.Write, kind, $"GDB write watchpoint at 0x{addr:X8}");
                _logger.LogDebug("GDB: Write watchpoint added at 0x{Address:X8}, size={Size}", addr, kind);
                await SendPacketAsync("OK");
                break;
                
            case 3: // Read watchpoint
                _breakpoints.AddWatchpoint(addr, WatchpointType.Read, kind, $"GDB read watchpoint at 0x{addr:X8}");
                _logger.LogDebug("GDB: Read watchpoint added at 0x{Address:X8}, size={Size}", addr, kind);
                await SendPacketAsync("OK");
                break;
                
            case 4: // Access watchpoint (read or write)
                _breakpoints.AddWatchpoint(addr, WatchpointType.Access, kind, $"GDB access watchpoint at 0x{addr:X8}");
                _logger.LogDebug("GDB: Access watchpoint added at 0x{Address:X8}, size={Size}", addr, kind);
                await SendPacketAsync("OK");
                break;
                
            default:
                // Unsupported breakpoint type
                await SendPacketAsync("");
                break;
        }
    }
    
    /// <summary>
    /// Remove breakpoint: z type,addr,kind
    /// </summary>
    private async Task HandleRemoveBreakpointAsync(string args)
    {
        var parts = args.Split(',');
        if (parts.Length < 2)
        {
            await SendPacketAsync("E01");
            return;
        }
        
        if (!int.TryParse(parts[0], out var type) ||
            !uint.TryParse(parts[1], NumberStyles.HexNumber, null, out var addr))
        {
            await SendPacketAsync("E01");
            return;
        }
        
        bool removed;
        switch (type)
        {
            case 0: // Software breakpoint
            case 1: // Hardware breakpoint
                removed = _breakpoints.RemoveBreakpointAtAddress(addr);
                _logger.LogDebug("GDB: Breakpoint removed at 0x{Address:X8}", addr);
                break;
                
            case 2: // Write watchpoint
                removed = _breakpoints.RemoveWatchpointAtAddress(addr, WatchpointType.Write);
                _logger.LogDebug("GDB: Write watchpoint removed at 0x{Address:X8}", addr);
                break;
                
            case 3: // Read watchpoint
                removed = _breakpoints.RemoveWatchpointAtAddress(addr, WatchpointType.Read);
                _logger.LogDebug("GDB: Read watchpoint removed at 0x{Address:X8}", addr);
                break;
                
            case 4: // Access watchpoint
                removed = _breakpoints.RemoveWatchpointAtAddress(addr, WatchpointType.Access);
                _logger.LogDebug("GDB: Access watchpoint removed at 0x{Address:X8}", addr);
                break;
                
            default:
                await SendPacketAsync("");
                return;
        }
        
        await SendPacketAsync(removed ? "OK" : "E01");
    }
    
    /// <summary>
    /// Handle query packets
    /// </summary>
    private async Task HandleQueryAsync(string args)
    {
        if (args.StartsWith("Supported", StringComparison.Ordinal))
        {
            // Advertise our capabilities
            await SendPacketAsync("PacketSize=4096;qXfer:features:read+;QStartNoAckMode+");
        }
        else if (args.StartsWith("Attached", StringComparison.Ordinal))
        {
            await SendPacketAsync("1"); // We're attached to an existing process
        }
        else if (args.StartsWith("C", StringComparison.Ordinal))
        {
            await SendPacketAsync(""); // Current thread (not implemented)
        }
        else if (args.StartsWith("Offsets", StringComparison.Ordinal))
        {
            await SendPacketAsync(""); // Text/Data/BSS offsets
        }
        else if (args.StartsWith("Symbol", StringComparison.Ordinal))
        {
            await SendPacketAsync("OK"); // No symbols
        }
        else if (args.StartsWith("TStatus", StringComparison.Ordinal))
        {
            await SendPacketAsync(""); // Tracepoint status
        }
        else if (args.StartsWith("Xfer:features:read", StringComparison.Ordinal))
        {
            // Send target description XML
            await HandleTargetDescriptionAsync(args);
        }
        else
        {
            await SendPacketAsync("");
        }
    }
    
    /// <summary>
    /// Handle set packets (Q commands)
    /// </summary>
    private async Task HandleSetAsync(string args)
    {
        if (args.StartsWith("StartNoAckMode", StringComparison.Ordinal))
        {
            // Enable no-ack mode
            _noAckMode = true;
            await SendPacketAsync("OK");
        }
        else
        {
            // Unsupported set command
            await SendPacketAsync("");
        }
    }
    
    /// <summary>
    /// Handle v packets (extended commands)
    /// </summary>
    private async Task HandleVPacketAsync(string args)
    {
        if (args.StartsWith("Cont?", StringComparison.Ordinal))
        {
            // Advertise vCont support
            await SendPacketAsync("vCont;c;C;s;S");
        }
        else if (args.StartsWith("Cont;", StringComparison.Ordinal))
        {
            // Handle vCont commands
            var action = args[5..];
            if (action.StartsWith('c'))
            {
                _continueExecution = true;
                _singleStep = false;
            }
            else if (action.StartsWith('s'))
            {
                _continueExecution = true;
                _singleStep = true;
            }
        }
        else
        {
            await SendPacketAsync("");
        }
    }
    
    /// <summary>
    /// Send target description XML (i386 architecture)
    /// </summary>
    private async Task HandleTargetDescriptionAsync(string args)
    {
        const string targetXml = """
            <?xml version="1.0"?>
            <!DOCTYPE target SYSTEM "gdb-target.dtd">
            <target version="1.0">
              <architecture>i386</architecture>
              <feature name="org.gnu.gdb.i386.core">
                <reg name="eax" bitsize="32" type="int32"/>
                <reg name="ecx" bitsize="32" type="int32"/>
                <reg name="edx" bitsize="32" type="int32"/>
                <reg name="ebx" bitsize="32" type="int32"/>
                <reg name="esp" bitsize="32" type="data_ptr"/>
                <reg name="ebp" bitsize="32" type="data_ptr"/>
                <reg name="esi" bitsize="32" type="int32"/>
                <reg name="edi" bitsize="32" type="int32"/>
                <reg name="eip" bitsize="32" type="code_ptr"/>
                <reg name="eflags" bitsize="32" type="i386_eflags"/>
              </feature>
            </target>
            """;
        
        var xmlBytes = Encoding.ASCII.GetBytes(targetXml);
        var response = $"l{Convert.ToHexString(xmlBytes).ToLowerInvariant()}";
        await SendPacketAsync(response);
    }
    
    /// <summary>
    /// Convert 32-bit value to little-endian hex string
    /// </summary>
    private static string ToHex32(uint value)
    {
        var bytes = BitConverter.GetBytes(value);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
    
    /// <summary>
    /// Parse a 32-bit value from little-endian hex string
    /// </summary>
    private static uint ParseHex32(string hex, ref int offset)
    {
        if (offset + 8 > hex.Length)
        {
            throw new ArgumentException("Insufficient data for 32-bit value");
        }
        
        var bytes = new byte[4];
        for (var i = 0; i < 4; i++)
        {
            bytes[i] = byte.Parse(hex.Substring(offset + i * 2, 2), NumberStyles.HexNumber);
        }
        offset += 8;
        
        return BitConverter.ToUInt32(bytes, 0);
    }
    
    public void Dispose()
    {
        _stream?.Dispose();
        _client?.Dispose();
        _listener?.Stop();
    }
}
