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

### Method 1: Configuration File

Edit your `appsettings.json` or GUI settings:

```json
{
  "EmulatorSettings": {
    "EnableMcpServer": true,
    "AutoStartMcpServer": true
  }
}
```

### Method 2: UI Settings

1. Open Win32Emu.Gui
2. Go to **Settings** → **Debugging**
3. Check **Enable MCP Server**
4. Check **Auto-start MCP server** (optional)
5. Click **Save**

### Method 3: Command Line

Start the emulator with MCP enabled:

```bash
Win32Emu.Gui.exe game.exe --mcp-server
```

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

## Integrating with GitHub Copilot

To use MCP with GitHub Copilot:

1. Ensure you have the latest version of VS Code with GitHub Copilot
2. Configure MCP in your VS Code settings
3. Start Win32Emu with MCP server enabled
4. Open a chat with Copilot
5. Ask debugging questions about your emulated program

### Example `mcp.json` Configuration

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

## Troubleshooting

### MCP Server Won't Start

**Problem**: MCP server fails to initialize

**Solutions**:
1. Check that ModelContextProtocol package is installed: `dotnet list package | grep ModelContextProtocol`
2. Verify .NET 10.0 SDK is installed: `dotnet --version`
3. Check logs for detailed error messages
4. Ensure no firewall is blocking STDIO communication

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
