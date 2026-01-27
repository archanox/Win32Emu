using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Avalonia.Threading;
using Win32Emu.Gui.Configuration;
using Win32Emu.Gui.Models;
using Win32Emu.Gui.ViewModels;
using Win32Emu.Gui.Views;
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
	private readonly EmulatorRuntimeService _emulatorRuntime;
	private readonly ILogger _logger;
	private readonly ConfigurationService _configService;
	private readonly object _launchGate = new();
	private DateTimeOffset? _lastLaunchRequestedUtc;
	private string? _lastLaunchTitle;
	private string? _lastLaunchError;

	public McpDebugTools(EmulatorRuntimeService emulatorRuntime, ILogger logger)
	{
		_emulatorRuntime = emulatorRuntime;
		_logger = logger;
		_configService = new ConfigurationService();
	}

	[McpServerTool]
	[Description("List games from the saved game library")]
	public string ListLibraryGames()
	{
		try
		{
			var games = _configService.GetGames()
				.Select(g => new
				{
					g.Title,
					g.ExecutablePath,
					g.VhdExecutablePath,
					g.VirtualDiskPath,
					g.LastPlayed,
					g.TimesPlayed
				})
				.ToArray();

			return System.Text.Json.JsonSerializer.Serialize(games, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to list library games");
			return $"Error: {ex.Message}";
		}
	}

	[McpServerTool]
	[Description("Launch a game from the saved library by title (case-insensitive). Creates an emulator window and starts the shared runtime.")]
	public string LaunchLibraryGame(
		[System.ComponentModel.Description("Game title from library (case-insensitive)")] string title)
	{
		if (string.IsNullOrWhiteSpace(title))
		{
			return "Error: title is required";
		}

		try
		{
			var games = _configService.GetGames();
			var game = games.FirstOrDefault(g => string.Equals(g.Title, title, StringComparison.OrdinalIgnoreCase));
			if (game == null)
			{
				return $"Error: Game not found in library: {title}";
			}

			_ = Dispatcher.UIThread.InvokeAsync(async () =>
			{
				try
				{
					var window = new EmulatorWindow();
					var logger = _logger;
					var viewModel = new EmulatorWindowViewModel(logger: logger);
					window.DataContext = viewModel;
					viewModel.SetOwnerWindow(window);
					window.Show();

					var gameSettings = _configService.GetGameSettings(game.ExecutablePath);
					string[]? programArgs = null;
					if (gameSettings?.ProgramArguments != null && !string.IsNullOrWhiteSpace(gameSettings.ProgramArguments))
					{
						programArgs = gameSettings.ProgramArguments.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
					}

					// Keep a per-window EmulatorService for dialog callbacks and message dispatcher integration.
					var config = _configService.GetEmulatorConfiguration(game.ExecutablePath);
					var serviceForDialogs = new EmulatorService(config, viewModel, logger);
					viewModel.SetEmulatorService(serviceForDialogs);
					viewModel.InitializeMessageDispatcher();

					await _emulatorRuntime.LaunchGameAsync(game, viewModel, programArgs);
				}
				catch (Exception ex) when (ex is not OutOfMemoryException && ex is not StackOverflowException)
				{
					_logger.LogError(ex, "Failed to launch game via MCP");
				}
			});

			return $"Launch requested for: {game.Title}";
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to launch library game {Title}", title);
			return $"Error: {ex.Message}";
		}
	}

	[McpServerTool]
	[Description("Get MCP server and emulator runtime status (running state, run id)")]
	public string GetServerStatus()
	{
		try
		{
			string? lastTitle;
			DateTimeOffset? lastRequested;
			string? lastError;
			lock (_launchGate)
			{
				lastTitle = _lastLaunchTitle;
				lastRequested = _lastLaunchRequestedUtc;
				lastError = _lastLaunchError;
			}

			var status = new
			{
				RuntimeInitialized = true,
				IsRunning = _emulatorRuntime.IsRunning,
				RunId = _emulatorRuntime.CurrentRunId,
				HasEmulatorInstance = _emulatorRuntime.CurrentEmulator != null,
				LastException = _emulatorRuntime.CurrentEmulator?.LastException?.Message,
				LastLaunchRequestedUtc = lastRequested,
				LastLaunchTitle = lastTitle,
				LastLaunchError = lastError
			};

			return System.Text.Json.JsonSerializer.Serialize(status, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to get server status");
			return $"Error: {ex.Message}";
		}
	}

	[McpServerTool]
	[Description("Get the current state of the emulator including CPU registers and flags")]
	public string GetEmulatorState()
	{
		var emulator = _emulatorRuntime.CurrentEmulator;
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
	[Description("Read memory contents at a specified address with hex and ASCII output")]
	public string ReadMemory(
		[System.ComponentModel.Description("Memory address in hexadecimal (e.g., '0x00401000')")] string address,
		[System.ComponentModel.Description("Number of bytes to read")] int length = 16)
	{
		var emulator = _emulatorRuntime.CurrentEmulator;
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
	[Description("Set a breakpoint at a specified memory address")]
	public string SetBreakpoint(
		[System.ComponentModel.Description("Memory address in hexadecimal (e.g., '0x00401000')")] string address)
	{
		var emulator = _emulatorRuntime.CurrentEmulator;
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
	[Description("Resume emulator execution until next breakpoint or halt")]
	public string ContinueExecution()
	{
		var emulator = _emulatorRuntime.CurrentEmulator;
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
	[Description("Execute a single CPU instruction and return the result")]
	public string StepInstruction()
	{
		var emulator = _emulatorRuntime.CurrentEmulator;
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
	[Description("Get the history of recently executed instructions")]
	public string GetExecutionHistory(
		[System.ComponentModel.Description("Number of instructions to retrieve")] int count = 10)
	{
		var emulator = _emulatorRuntime.CurrentEmulator;
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
	[Description("Get the current call stack with return addresses and frame information")]
	public string GetCallStack()
	{
		var emulator = _emulatorRuntime.CurrentEmulator;
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
	[Description("Get a list of all loaded DLL modules with their base addresses")]
	public string GetLoadedModules()
	{
		var emulator = _emulatorRuntime.CurrentEmulator;
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
	[Description("Search for a byte pattern in emulator memory")]
	public string SearchMemory(
		[System.ComponentModel.Description("Hex byte pattern to search for (e.g., '4D 5A' for PE header)")] string pattern,
		[System.ComponentModel.Description("Starting address in hexadecimal")] string? startAddress = null,
		[System.ComponentModel.Description("Maximum number of results to return")] int maxResults = 10)
	{
		var emulator = _emulatorRuntime.CurrentEmulator;
		if (emulator == null)
		{
			return "Emulator is not running";
		}

		try
		{
			// Parse pattern - filter out empty tokens and validate hex format
			var tokens = pattern.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			var patternBytes = new List<byte>();
			
			foreach (var token in tokens)
			{
				if (!byte.TryParse(token, System.Globalization.NumberStyles.HexNumber, null, out var b))
				{
					return $"Error: Invalid hex byte '{token}' in pattern. Expected hex bytes like '4D 5A'.";
				}
				patternBytes.Add(b);
			}
			
			if (patternBytes.Count == 0)
			{
				return "Error: Empty pattern provided";
			}

			uint startAddr = 0;
			if (startAddress != null && !TryParseAddress(startAddress, out startAddr))
			{
				return $"Invalid start address format: {startAddress}";
			}

			var results = emulator.SearchMemory(patternBytes.ToArray(), startAddr, maxResults);
			return System.Text.Json.JsonSerializer.Serialize(results, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to search memory");
			return $"Error: {ex.Message}";
		}
	}

	[McpServerTool]
	[Description("Get a trace of recent Win32 API calls with parameters and return values")]
	public string GetWin32ApiTrace(
		[System.ComponentModel.Description("Number of recent API calls to retrieve")] int count = 20)
	{
		var emulator = _emulatorRuntime.CurrentEmulator;
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
	[Description("Disassemble x86 instructions at a specified memory address")]
	public string DisassembleAt(
		[System.ComponentModel.Description("Memory address in hexadecimal")] string address,
		[System.ComponentModel.Description("Number of instructions to disassemble")] int count = 10)
	{
		var emulator = _emulatorRuntime.CurrentEmulator;
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
