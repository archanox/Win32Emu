# MCP (Model Context Protocol) Debugging Guide

## Overview

Win32Emu now supports the Model Context Protocol (MCP), enabling AI assistants like GitHub Copilot to directly inspect and control the emulator during debugging sessions. This provides a powerful new way to debug emulated programs with AI assistance.

## What is MCP?

The Model Context Protocol is an open standard that allows AI applications to interact with external tools and data sources in a structured way. For Win32Emu, this means AI assistants can:

- Inspect CPU registers and memory in real-time
- Set breakpoints and control execution
- Analyze execution history and call stacks
- Search memory for patterns
- Disassemble code at any address
- Review Win32 API call traces

## Benefits

1. **Natural Language Debugging**: Ask questions like "Why is the program stuck at 0x401000?" and the AI can investigate
2. **Automated Analysis**: AI can detect common issues like infinite loops, memory corruption, and incorrect API calls
3. **Real-time Insights**: Get immediate answers about emulator state without manual inspection
4. **Enhanced Productivity**: Faster debugging cycles with AI assistance
5. **Learning Tool**: Understand emulator internals and Win32 programming through AI explanations

## Requirements

- Win32Emu.Gui with MCP support
- AI assistant with MCP client support (e.g., GitHub Copilot, Claude Desktop)
- .NET 10.0 SDK

## Enabling MCP Server

### Transport Types

Win32Emu MCP server supports two transport types:

1. **HTTP Transport** (default, recommended for Visual Studio):
   - URL: `http://127.0.0.1:5111` (configurable port)
   - Best for Visual Studio, IDEs, and web-based AI tools
   - Supports remote connections
   - Easy to test with curl or browser

2. **STDIO Transport** (for command-line tools):
   - Uses standard input/output streams
   - Best for CLI-based AI assistants
   - Local-only, more secure
   - No network configuration needed

### Method 1: Configuration File

Edit your `appsettings.json` or GUI settings:

```json
{
  "EmulatorSettings": {
    "EnableMcpServer": true,
    "AutoStartMcpServer": true,
    "McpUseHttpTransport": true,
    "McpHttpPort": 5111
  }
}
```

**Configuration Options**:
- `EnableMcpServer`: Enable MCP server functionality
- `AutoStartMcpServer`: Start MCP server when application launches
- `McpUseHttpTransport`: Use HTTP transport (true) or STDIO (false). Default: true
- `McpHttpPort`: Port for HTTP server. Default: 5111

The MCP server will start when you launch Win32Emu.Gui, before any emulation session begins. This allows AI assistants to:
- Connect immediately when the application starts
- Start emulation sessions using MCP tools
- Monitor the entire lifecycle from application startup to shutdown
- Debug issues that occur during emulation startup

### Method 2: UI Settings

1. Open Win32Emu.Gui
2. Go to **Settings** → **Debugging**
3. Check **Enable MCP Server**
4. Check **Auto-start MCP server** (optional)
5. Select **HTTP Transport** for Visual Studio (or **STDIO** for CLI tools)
6. Set **HTTP Port** (default: 5111)
7. Click **Save**
8. Restart the application for changes to take effect

### Method 3: Command Line

Start the emulator with MCP enabled:

```bash
Win32Emu.Gui.exe game.exe --mcp-server
```

**Note**: The `--mcp-server` CLI flag is parsed but server startup is controlled by configuration settings. Set `EnableMcpServer` or `AutoStartMcpServer` to `true` in your configuration.

## Available MCP Tools

The MCP server exposes the following debugging tools:

### 1. GetEmulatorState

Get current CPU state including all registers and flags.

**Example AI Query**: "What are the current register values?"

**Response**:
```json
{
  "Registers": {
    "EAX": 0,
    "EBX": 0,
    "ECX": 0,
    "EDX": 0,
    "ESI": 0,
    "EDI": 0,
    "EBP": 2097152,
    "ESP": 2097152,
    "EIP": 4198400,
    "EFLAGS": 0
  },
  "Memory": {
    "Size": 268435456,
    "StackBase": 2097152,
    "StackLimit": 2064384,
    "HeapBase": 5242880
  },
  "State": {
    "IsRunning": true,
    "IsPaused": false,
    "DebugMode": false,
    "InteractiveDebugMode": false
  }
}
```

### 2. ReadMemory

Read memory at a specified address.

**Parameters**:
- `address`: Memory address in hex (e.g., "0x00401000")
- `length`: Number of bytes to read (default: 16)

**Example AI Query**: "Show me the memory at 0x00401000"

**Response**:
```
Address: 00401000
Hex: 55 8B EC 83 EC 40 53 56 57 8B 7D 08 8B F7 8D 45
ASCII: U.....@SVW.}..E
```

### 3. SetBreakpoint

Set a breakpoint at a specified address.

**Parameters**:
- `address`: Memory address in hex (e.g., "0x00401234")

**Example AI Query**: "Set a breakpoint at 0x00401234"

### 4. ContinueExecution

Resume emulator execution until the next breakpoint.

**Example AI Query**: "Continue execution"

### 5. StepInstruction

Execute a single instruction and pause.

**Example AI Query**: "Step one instruction"

### 6. GetExecutionHistory

Get the last N executed instructions.

**Parameters**:
- `count`: Number of instructions to retrieve (default: 10)

**Example AI Query**: "Show me the last 10 instructions executed"

### 7. GetCallStack

Get the current call stack.

**Example AI Query**: "What's the current call stack?"

**Response**:
```json
{
  "ESP": 2088960,
  "EBP": 2097152,
  "Message": "Call stack reconstruction not yet fully implemented"
}
```

### 8. GetLoadedModules

Get a list of loaded DLL modules.

**Example AI Query**: "What DLLs are loaded?"

**Response**:
```json
[
  {
    "Name": "game.exe",
    "BaseAddress": 4194304
  },
  {
    "Message": "DLL enumeration not yet fully implemented"
  }
]
```

### 9. SearchMemory

Search for a byte pattern in memory.

**Parameters**:
- `pattern`: Hex byte pattern (e.g., "4D 5A" for PE header)
- `startAddress`: Starting address (optional)
- `maxResults`: Maximum results (default: 10)

**Example AI Query**: "Search for the pattern '4D 5A' in memory"

**Response**:
```json
{
  "Results": [4194304],
  "Count": 1
}
```

### 10. GetWin32ApiTrace

Get recent Win32 API calls with parameters and return values.

**Parameters**:
- `count`: Number of recent calls to retrieve (default: 20)

**Example AI Query**: "Show me the last 20 Win32 API calls"

### 11. DisassembleAt

Disassemble instructions at a specified address.

**Parameters**:
- `address`: Memory address in hex
- `count`: Number of instructions to disassemble (default: 10)

**Example AI Query**: "Disassemble 10 instructions at 0x00401000"

## Usage Examples

### MCP Server Lifecycle

The MCP server lifecycle is designed to give AI assistants full control over the debugging experience:

1. **Application Startup**: When you launch Win32Emu.Gui with MCP enabled, the server starts immediately
2. **AI Connection**: AI assistants can connect before any emulation begins
3. **Pre-Emulation**: AI can inspect configuration, start emulation sessions, or wait for user to start a game
4. **During Emulation**: Full access to all debugging tools while emulation is running
5. **Post-Emulation**: AI can analyze results after emulation stops
6. **Application Shutdown**: Server shuts down gracefully when the application exits

This design allows AI to:
- Monitor the complete application lifecycle
- Start and control emulation sessions programmatically
- Catch issues that occur during startup
- Provide proactive assistance throughout the debugging workflow

### Example 1: Debugging an Infinite Loop

**You**: "The program seems stuck. Can you check what's happening?"

**AI** (using MCP tools):
1. Calls `GetEmulatorState` to see current EIP
2. Calls `DisassembleAt` at current EIP
3. Calls `GetExecutionHistory` to see recent instructions
4. Analyzes the pattern and responds: "The program is in an infinite loop at 0x00401234. It's repeatedly executing a `JMP` instruction that jumps to itself. The condition flag isn't being set correctly."

### Example 2: Analyzing a Crash

**You**: "The program crashed. What went wrong?"

**AI** (using MCP tools):
1. Calls `GetEmulatorState` to check registers
2. Notices ESP/EBP are corrupted
3. Calls `GetCallStack` to see the stack state
4. Calls `ReadMemory` at the crash address
5. Responds: "The program crashed due to stack corruption. EBP was set to an invalid value (0x00000002), causing memory access at 0xFFFFFFFD. This is likely caused by a buffer overflow in the previous function."

### Example 3: Finding a Memory Leak

**You**: "Is there a pattern '48 65 6C 6C 6F' (\"Hello\") in memory?"

**AI** (using MCP tools):
1. Calls `SearchMemory` with pattern "48 65 6C 6C 6F"
2. Responds: "Yes, found 15 instances of 'Hello' in memory at addresses: 0x00410000, 0x00420000, ..."

## Integrating with Visual Studio

Visual Studio (version 18.2+) has built-in MCP support. To connect Win32Emu MCP server to Visual Studio:

### Configuration

1. **Enable HTTP Transport**: In Win32Emu settings, ensure `McpUseHttpTransport` is `true` and `McpHttpPort` is set to `5111` (or your preferred port)

2. **Create `.mcp.json` Configuration**: Create a `.mcp.json` file in your workspace or user profile directory:

```json
{
  "mcpServers": {
    "win32emu-debugger": {
      "url": "http://127.0.0.1:5111",
      "description": "Win32Emu Debugging Server - Inspect and control x86 emulation"
    }
  }
}
```

3. **Start Win32Emu**: Launch Win32Emu.Gui with MCP server enabled. The server will start at `http://127.0.0.1:5111`

4. **Verify Connection**: In Visual Studio:
   - Open a chat window with GitHub Copilot or another AI assistant
   - The MCP tools from Win32Emu should appear in the available tools list
   - Look for tools like `GetEmulatorState`, `ReadMemory`, `SetBreakpoint`, etc.

### Testing the Connection

You can test the MCP HTTP endpoint with curl:

```bash
# Test server health
curl http://127.0.0.1:5111/health

# Test initialize endpoint
curl -X POST http://127.0.0.1:5111/initialize \
  -H "Content-Type: application/json" \
  -d '{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}'

# Test tools/list endpoint
curl -X POST http://127.0.0.1:5111/tools/list \
  -H "Content-Type: application/json" \
  -d '{}'
```

### Expected Response

The `/tools/list` endpoint should return all available MCP tools:

```json
{
  "tools": [
    {
      "name": "GetEmulatorState",
      "description": "Get current CPU state including registers and flags"
    },
    {
      "name": "ReadMemory",
      "description": "Read memory at a specified address",
      "inputSchema": {
        "type": "object",
        "properties": {
          "address": {
            "type": "string",
            "description": "Memory address in hexadecimal"
          },
          "length": {
            "type": "integer",
            "description": "Number of bytes to read"
          }
        }
      }
    }
    // ... more tools
  ]
}
```

## Integrating with GitHub Copilot (VS Code)

To use MCP with GitHub Copilot in VS Code:

1. Ensure you have the latest version of VS Code with GitHub Copilot
2. Configure MCP in your VS Code settings
3. Start Win32Emu with MCP server enabled
4. Open a chat with Copilot
5. Ask debugging questions about your emulated program

### Example `mcp.json` Configuration for STDIO

For command-line tools that use STDIO transport:

```json
{
  "mcpServers": {
    "win32emu-debugger": {
      "command": "Win32Emu.Gui.exe",
      "args": ["--mcp-server"],
      "env": {}
    }
  }
}
```

**Note**: Ensure `McpUseHttpTransport` is set to `false` when using STDIO transport.

## Troubleshooting

### Visual Studio Not Showing MCP Tools

**Problem**: Visual Studio connects to MCP server but tools don't appear in the UI

**Solutions**:
1. **Verify HTTP Transport is Enabled**: Check that `McpUseHttpTransport` is `true` in your configuration
2. **Verify Port Configuration**: Ensure `McpHttpPort` matches the port in your `.mcp.json` (default: 5111)
3. **Test the Endpoint**: Use curl to verify the server is responding:
   ```bash
   curl http://127.0.0.1:5111/tools/list -X POST -H "Content-Type: application/json" -d '{}'
   ```
4. **Check Server Logs**: Look for "[MCP] Server started successfully using HTTP transport" in Win32Emu logs
5. **Restart Visual Studio**: Sometimes VS needs to be restarted after the MCP server starts
6. **Check .mcp.json Location**: Ensure your `.mcp.json` file is in the correct location (workspace root or user profile)

### MCP Server Won't Start

**Problem**: MCP server fails to initialize

**Solutions**:
1. Check that ModelContextProtocol packages are installed:
   ```bash
   # Linux/macOS
   dotnet list package | grep ModelContextProtocol
   
   # Windows PowerShell
   dotnet list package | Select-String ModelContextProtocol
   
   # Windows Command Prompt
   dotnet list package | findstr ModelContextProtocol
   ```
   Should show both `ModelContextProtocol` and `ModelContextProtocol.AspNetCore`
2. Verify .NET 10.0 SDK is installed: `dotnet --version`
3. Check logs for detailed error messages
4. For HTTP transport: Ensure port 5111 (or configured port) is not already in use
5. For STDIO transport: Ensure no firewall is blocking communication
6. Try switching between HTTP and STDIO transport to isolate the issue

### Port Already in Use

**Problem**: HTTP MCP server can't start because port is in use

**Solutions**:
1. Change `McpHttpPort` to a different port (e.g., 5112, 5113)
2. Find and stop the process using port 5111:
   ```bash
   # Windows
   netstat -ano | findstr :5111
   taskkill /PID <process_id> /F
   
   # Linux/macOS
   lsof -i :5111
   kill -9 <process_id>
   ```
3. Update your `.mcp.json` configuration to match the new port

### AI Can't Connect to Emulator

**Problem**: AI assistant can't see MCP tools

**Solutions**:
1. Verify MCP server is running (check logs for "[MCP] Server started")
2. Ensure AI assistant supports MCP protocol
3. Check MCP client configuration in your AI tool
4. Try restarting both the emulator and AI assistant

### Tools Return "Not Yet Implemented"

**Problem**: Some MCP tools show "not yet implemented" messages

**Explanation**: Some advanced features like full call stack reconstruction and API tracing are still being developed. The basic debugging tools (GetEmulatorState, ReadMemory, etc.) are fully functional.

## Best Practices

1. **Start with Simple Queries**: Begin with basic state inspection before diving into complex analysis
2. **Use Breakpoints Wisely**: Set breakpoints at key locations to pause execution for inspection
3. **Combine Tools**: Use multiple tools together (e.g., GetEmulatorState + DisassembleAt + ReadMemory) for comprehensive analysis
4. **Iterate**: If the first analysis doesn't reveal the issue, try different approaches
5. **Learn from AI**: Pay attention to how the AI uses MCP tools to understand debugging patterns

## Security Considerations

- MCP server only runs when explicitly enabled
- All debugging operations are read-only by default (except SetBreakpoint and execution control)
- No sensitive data is exposed beyond what's already in the emulator's memory
- STDIO transport means only local AI assistants can connect (no network exposure)

## Limitations

- Execution history requires history tracking to be enabled (future feature)
- Full call stack reconstruction is not yet implemented
- API tracing needs to be enabled separately
- Some tools return placeholder data until fully implemented

## Future Enhancements

- [ ] Real-time execution history tracking
- [ ] Complete call stack reconstruction with symbol resolution
- [ ] Integrated API call tracing with parameter values
- [ ] Memory watch points and conditional breakpoints
- [ ] Source-level debugging with symbol files
- [ ] Performance profiling tools
- [ ] HTTP transport for remote debugging

## Contributing

Want to improve MCP support? Check out:
- `Win32Emu.Gui/Services/McpDebugServer.cs` - MCP tool implementations
- `Win32Emu.Gui/Services/McpServerHost.cs` - MCP server hosting
- `Win32Emu/Emulator.cs` - Debugging methods in the MCP Debugging API region

## Additional Resources

- [Model Context Protocol Specification](https://modelcontextprotocol.io/specification/)
- [MCP C# SDK Documentation](https://modelcontextprotocol.github.io/csharp-sdk/)
- [Win32Emu Debugging Guide](DEBUGGING_GUIDE.md)
- [Interactive Debugger Guide](INTERACTIVE_DEBUGGER_GUIDE.md)
- [GDB Server Guide](GDB_SERVER_GUIDE.md)

## Feedback

Have questions or suggestions about MCP support? Please:
- Open an issue on GitHub
- Join the discussion in the Win32Emu community
- Submit a pull request with improvements

---

**Note**: MCP support is a cutting-edge feature that brings AI-assisted debugging to emulator development. As the technology evolves, we'll continue enhancing the capabilities and expanding the available tools.
