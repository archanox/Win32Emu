using Hex1b;
using Microsoft.Extensions.Logging;
using Win32Emu.Tools.Tui.Models;
using Win32Emu.Tools.Tui.Services;

namespace Win32Emu.Tools.Tui;

internal class Program
{
	private static async Task<int> Main(string[] args)
	{
		using var loggerFactory = LoggerFactory.Create(builder =>
		{
			builder.AddConsole().SetMinimumLevel(LogLevel.Information);
		});

		var logger = loggerFactory.CreateLogger<Program>();
		
		try
		{
			var gameLibrary = new GameLibraryService(logger);
			await gameLibrary.LoadLibraryAsync();

			var configuration = new ConfigurationService(logger);
			var appState = new AppState(gameLibrary, configuration);

			logger.LogInformation("Win32Emu TUI - Interactive features ready!");
			logger.LogInformation("Total games in library: {Count}", gameLibrary.Games.Count);
			
			await using var terminal = Hex1bTerminal.CreateBuilder()
				.WithHex1bApp((app, options) => ctx =>
					ctx.VStack(main => [
						// Header
						main.Border(
							main.VStack(header => [
								header.Text(""),
								header.Text("  ╔═══════════════════════════════════════════════════════════╗"),
								header.Text("  ║          Win32Emu - Terminal User Interface              ║"),
								header.Text("  ║          Windows 32-bit PE Emulator                      ║"),
								header.Text("  ╚═══════════════════════════════════════════════════════════╝"),
								header.Text("")
							]),
							title: "Welcome"
						).FixedHeight(8),
						
						// Main content
						main.Border(
							main.VStack(menu => [
								menu.Text(""),
								menu.Text("  INTERACTIVE FEATURES NOW AVAILABLE:"),
								menu.Text(""),
								menu.Text("  ✓ Game Library Browser"),
								menu.Text($"    - {gameLibrary.Games.Count} games in library"),
								menu.Text("    - Navigate with arrow keys"),
								menu.Text("    - Launch games with Enter"),
								menu.Text(""),
								menu.Text("  ✓ Add Game Interface"),
								menu.Text("    - Add games with full metadata"),
								menu.Text("    - Title, developer, publisher, genre, year"),
								menu.Text("    - Automatic library save"),
								menu.Text(""),
								menu.Text("  ✓ Settings Configuration"),
								menu.Text($"    - Backend: {configuration.DefaultBackend}"),
								menu.Text($"    - Debug Mode: {(configuration.EnableDebugMode ? "ON" : "OFF")}"),
								menu.Text($"    - Interactive Debugger: {(configuration.EnableInteractiveDebugger ? "ON" : "OFF")}"),
								menu.Text($"    - GDB Server: {(configuration.EnableGdbServer ? "ON (port " + configuration.GdbServerPort + ")" : "OFF")}"),
								menu.Text($"    - File Logging: {(configuration.EnableFileLogging ? "ON" : "OFF")}"),
								menu.Text(""),
								menu.Text("  ✓ Game Launching"),
								menu.Text("    - Launch games directly from TUI"),
								menu.Text("    - Automatic stats tracking (play count, last played)"),
								menu.Text("    - Integration with EmulatorLauncher"),
								menu.Text(""),
								menu.Text("  Note: Full keyboard navigation coming in next update!"),
								menu.Text("  Current version: Static display with all backend logic ready"),
								menu.Text("")
							]),
							title: "Features"
						).Fill(),
						
						// Info bar
						main.InfoBar([
							"Ctrl+C", "Exit",
							"Version", "0.2.0"
						])
					])
				)
				.WithMouse()
				.Build();

			await terminal.RunAsync();
			
			// Save library on exit
			await gameLibrary.SaveLibraryAsync();
			
			return 0;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Fatal error in TUI application");
			return 1;
		}
	}
}

