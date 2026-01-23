using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Gui.Services;

/// <summary>
/// MCP (Model Context Protocol) server for debugging Win32Emu emulator.
/// Provides AI assistants with tools to inspect and control the emulator state.
/// </summary>
[McpServerToolType]
public class McpDebugTools
{
	private readonly EmulatorService _emulatorService;
	private readonly ILogger _logger;

	public McpDebugTools(EmulatorService emulatorService, ILogger logger)
	{
		_emulatorService = emulatorService;
		_logger = logger;
	}

	[McpServerTool]
	public string GetEmulatorState()
	{
		var emulator = _emulatorService.CurrentEmulator;
		if (emulator == null)
		{
			return "Emulator is not running";
		}

		try
		{
			// Get CPU state (this will need to be exposed via a public API)
			var state = emulator.GetDebugState();
			return System.Text.Json.JsonSerializer.Serialize(state, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to get emulator state");
			return $"Error: {ex.Message}";
		}
	}

	[McpServerTool]
	public string ReadMemory(
		[System.ComponentModel.Description("Memory address in hexadecimal (e.g., '0x00401000')")] string address,
		[System.ComponentModel.Description("Number of bytes to read")] int length = 16)
	{
		var emulator = _emulatorService.CurrentEmulator;
		if (emulator == null)
		{
			return "Emulator is not running";
		}

		try
		{
			// Parse address
			if (!TryParseAddress(address, out var addr))
			{
				return $"Invalid address format: {address}";
			}

			// Read memory
			var bytes = emulator.ReadMemory(addr, length);
			var hex = BitConverter.ToString(bytes).Replace("-", " ");
			var ascii = string.Concat(bytes.Select(b => b >= 32 && b < 127 ? (char)b : '.'));
			
			return $"Address: {addr:X8}\nHex: {hex}\nASCII: {ascii}";
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to read memory at {Address}", address);
			return $"Error: {ex.Message}";
		}
	}

	[McpServerTool]
	public string SetBreakpoint(
		[System.ComponentModel.Description("Memory address in hexadecimal (e.g., '0x00401000')")] string address)
	{
		var emulator = _emulatorService.CurrentEmulator;
		if (emulator == null)
		{
			return "Emulator is not running";
		}

		try
		{
			if (!TryParseAddress(address, out var addr))
			{
				return $"Invalid address format: {address}";
			}

			emulator.SetBreakpoint(addr);
			return $"Breakpoint set at {addr:X8}";
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to set breakpoint at {Address}", address);
			return $"Error: {ex.Message}";
		}
	}

	[McpServerTool]
	public string ContinueExecution()
	{
		var emulator = _emulatorService.CurrentEmulator;
		if (emulator == null)
		{
			return "Emulator is not running";
		}

		try
		{
			emulator.Continue();
			return "Execution resumed";
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to continue execution");
			return $"Error: {ex.Message}";
		}
	}

	[McpServerTool]
	public string StepInstruction()
	{
		var emulator = _emulatorService.CurrentEmulator;
		if (emulator == null)
		{
			return "Emulator is not running";
		}

		try
		{
			emulator.Step();
			return "Stepped one instruction";
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to step instruction");
			return $"Error: {ex.Message}";
		}
	}

	[McpServerTool]
	public string GetExecutionHistory(
		[System.ComponentModel.Description("Number of instructions to retrieve")] int count = 10)
	{
		var emulator = _emulatorService.CurrentEmulator;
		if (emulator == null)
		{
			return "Emulator is not running";
		}

		try
		{
			var history = emulator.GetExecutionHistory(count);
			return System.Text.Json.JsonSerializer.Serialize(history, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to get execution history");
			return $"Error: {ex.Message}";
		}
	}

	[McpServerTool]
	public string GetCallStack()
	{
		var emulator = _emulatorService.CurrentEmulator;
		if (emulator == null)
		{
			return "Emulator is not running";
		}

		try
		{
			var callStack = emulator.GetCallStack();
			return System.Text.Json.JsonSerializer.Serialize(callStack, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to get call stack");
			return $"Error: {ex.Message}";
		}
	}

	[McpServerTool]
	public string GetLoadedModules()
	{
		var emulator = _emulatorService.CurrentEmulator;
		if (emulator == null)
		{
			return "Emulator is not running";
		}

		try
		{
			var modules = emulator.GetLoadedModules();
			return System.Text.Json.JsonSerializer.Serialize(modules, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to get loaded modules");
			return $"Error: {ex.Message}";
		}
	}

	[McpServerTool]
	public string SearchMemory(
		[System.ComponentModel.Description("Hex byte pattern to search for (e.g., '4D 5A' for PE header)")] string pattern,
		[System.ComponentModel.Description("Starting address in hexadecimal")] string? startAddress = null,
		[System.ComponentModel.Description("Maximum number of results to return")] int maxResults = 10)
	{
		var emulator = _emulatorService.CurrentEmulator;
		if (emulator == null)
		{
			return "Emulator is not running";
		}

		try
		{
			// Parse pattern
			var patternBytes = pattern.Split(' ')
				.Select(s => Convert.ToByte(s.Trim(), 16))
				.ToArray();

			uint startAddr = 0;
			if (startAddress != null && !TryParseAddress(startAddress, out startAddr))
			{
				return $"Invalid start address format: {startAddress}";
			}

			var results = emulator.SearchMemory(patternBytes, startAddr, maxResults);
			return System.Text.Json.JsonSerializer.Serialize(results, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to search memory");
			return $"Error: {ex.Message}";
		}
	}

	[McpServerTool]
	public string GetWin32ApiTrace(
		[System.ComponentModel.Description("Number of recent API calls to retrieve")] int count = 20)
	{
		var emulator = _emulatorService.CurrentEmulator;
		if (emulator == null)
		{
			return "Emulator is not running";
		}

		try
		{
			var trace = emulator.GetApiTrace(count);
			return System.Text.Json.JsonSerializer.Serialize(trace, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to get API trace");
			return $"Error: {ex.Message}";
		}
	}

	[McpServerTool]
	public string DisassembleAt(
		[System.ComponentModel.Description("Memory address in hexadecimal")] string address,
		[System.ComponentModel.Description("Number of instructions to disassemble")] int count = 10)
	{
		var emulator = _emulatorService.CurrentEmulator;
		if (emulator == null)
		{
			return "Emulator is not running";
		}

		try
		{
			if (!TryParseAddress(address, out var addr))
			{
				return $"Invalid address format: {address}";
			}

			var disassembly = emulator.Disassemble(addr, count);
			return System.Text.Json.JsonSerializer.Serialize(disassembly, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to disassemble at {Address}", address);
			return $"Error: {ex.Message}";
		}
	}

	private static bool TryParseAddress(string address, out uint result)
	{
		// Remove "0x" prefix if present
		if (address.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
		{
			address = address.Substring(2);
		}

		return uint.TryParse(address, System.Globalization.NumberStyles.HexNumber, null, out result);
	}
}
