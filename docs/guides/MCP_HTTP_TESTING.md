# MCP HTTP Server Manual Testing Guide

This document provides step-by-step instructions for manually testing the Win32Emu MCP HTTP server integration.

## Prerequisites

- Win32Emu.Gui built with HTTP transport support
- `curl` command-line tool (or any HTTP client)
- Visual Studio 18.2+ (for full integration testing)

## Test Configuration

Create or update your emulator configuration with the following MCP settings:

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

## Test 1: Start the MCP Server

1. Launch Win32Emu.Gui with the configuration above
2. Check the application logs for MCP server startup messages:
   ```
   [MCP] Initializing server at application startup for AI-assisted debugging
   [MCP] Configuring HTTP transport at http://127.0.0.1:5111
   [MCP] Server initialized and ready
   [MCP] Server started successfully using HTTP transport at http://127.0.0.1:5111
   ```

**Expected Result**: Server starts without errors and logs show HTTP transport at port 5111.

## Test 2: Verify Server is Listening

Check that the server is listening on port 5111:

**Windows:**
```cmd
netstat -an | findstr :5111
```

**Linux/macOS:**
```bash
lsof -i :5111
```

**Expected Result**: Shows the Win32Emu.Gui process listening on port 5111.

## Test 3: Test MCP Initialize Endpoint

Test the MCP protocol initialize handshake:

```bash
curl -X POST http://127.0.0.1:5111/initialize \
  -H "Content-Type: application/json" \
  -d '{
    "protocolVersion": "2024-11-05",
    "capabilities": {},
    "clientInfo": {
      "name": "test-client",
      "version": "1.0.0"
    }
  }'
```

**Expected Result**: JSON response with server capabilities and info:
```json
{
  "protocolVersion": "2024-11-05",
  "capabilities": {
    "tools": {}
  },
  "serverInfo": {
    "name": "win32emu-debugger",
    "version": "1.0.0"
  }
}
```

## Test 4: Test Tools List Endpoint

Retrieve the list of available MCP tools:

```bash
curl -X POST http://127.0.0.1:5111/tools/list \
  -H "Content-Type: application/json" \
  -d '{}'
```

**Expected Result**: JSON response with all available debugging tools:
```json
{
  "tools": [
    {
      "name": "GetEmulatorState",
      "description": "Get the current state of the emulator including CPU registers and flags",
      "inputSchema": {
        "type": "object",
        "properties": {}
      }
    },
    {
      "name": "ReadMemory",
      "description": "Read memory contents at a specified address with hex and ASCII output",
      "inputSchema": {
        "type": "object",
        "properties": {
          "address": {
            "type": "string",
            "description": "Memory address in hexadecimal (e.g., '0x00401000')"
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

## Test 5: Test a Tool Call

Call a specific MCP tool (requires emulator to be running):

```bash
curl -X POST http://127.0.0.1:5111/tools/call \
  -H "Content-Type: application/json" \
  -d '{
    "name": "GetEmulatorState",
    "arguments": {}
  }'
```

**Expected Result**: Either JSON response with emulator state (if running) or error message indicating emulator is not running.

## Test 6: Visual Studio Integration

### Create `.mcp.json` Configuration

Create a `.mcp.json` file in your workspace or user profile directory:

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

### Verify in Visual Studio

1. Open Visual Studio 18.2+
2. Ensure Win32Emu MCP server is running
3. Open a chat window with GitHub Copilot or AI assistant
4. Check available tools/resources

**Expected Result**: MCP tools from Win32Emu appear in the tools list, including:
- GetEmulatorState
- ReadMemory
- SetBreakpoint
- ContinueExecution
- StepInstruction
- GetExecutionHistory
- GetCallStack
- GetLoadedModules
- SearchMemory
- GetWin32ApiTrace
- DisassembleAt

## Troubleshooting

### Server Won't Start

**Issue**: MCP server fails to initialize

**Check**:
1. Verify ModelContextProtocol.AspNetCore package is installed
2. Check for port conflicts (port 5111 already in use)
3. Review application logs for detailed error messages

**Solution**: Change `McpHttpPort` to a different port if 5111 is already in use.

### Connection Refused

**Issue**: curl returns "Connection refused"

**Check**:
1. Verify server is running and logs show successful startup
2. Check firewall isn't blocking port 5111
3. Verify correct port number in curl command

**Solution**: Ensure server started successfully and firewall allows connections.

### Empty Tools List

**Issue**: `/tools/list` returns empty array

**Check**:
1. Verify `WithTools<McpDebugTools>()` (or the appropriate `WithTools<T>()` call) is used in McpServerHost to register tools
2. Check McpDebugTools class has `[McpServerToolType]` attribute
3. Verify tool methods have `[McpServerTool]` attributes

**Solution**: This should work out of the box; if not, rebuild the project.

### Visual Studio Doesn't Show Tools

**Issue**: VS connects but doesn't display tools

**Check**:
1. Verify HTTP transport is enabled (`McpUseHttpTransport: true`)
2. Check `.mcp.json` file location and format
3. Verify server logs show "HTTP transport"
4. Test endpoints with curl to confirm they work

**Solution**: 
1. Restart Visual Studio
2. Verify `.mcp.json` URL matches server endpoint
3. Check Visual Studio logs for MCP connection errors

## Success Criteria

All tests pass when:
- ✅ Server starts and logs show HTTP transport at port 5111
- ✅ Port 5111 is listening and accessible
- ✅ `/initialize` endpoint returns valid MCP protocol response
- ✅ `/tools/list` endpoint returns all debugging tools
- ✅ Tool calls work (when emulator is running)
- ✅ Visual Studio displays MCP tools from Win32Emu

## Next Steps

Once manual testing confirms HTTP transport is working:
1. Document any Visual Studio-specific configuration requirements
2. Consider adding automated integration tests
3. Update user documentation with troubleshooting tips
4. Consider adding health check endpoint for monitoring
