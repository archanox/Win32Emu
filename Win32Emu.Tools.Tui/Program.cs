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
			// AppState prepared for future interactive navigation
			_ = new AppState(gameLibrary, configuration);

			logger.LogInformation("Win32Emu TUI - Backend services ready!");
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
								menu.Text("  BACKEND SERVICES READY (Interactive UI in development):"),
								menu.Text(""),
								menu.Text("  📚 Game Library Service"),
								menu.Text($"    - {gameLibrary.Games.Count} games in library"),
								menu.Text("    - JSON persistence implemented"),
								menu.Text("    - Play statistics tracking ready"),
								menu.Text(""),
								menu.Text("  ⚙️  Configuration Service"),
								menu.Text($"    - Backend: {configuration.DefaultBackend}"),
								menu.Text($"    - Debug Mode: {(configuration.EnableDebugMode ? "ON" : "OFF")}"),
								menu.Text($"    - Interactive Debugger: {(configuration.EnableInteractiveDebugger ? "ON" : "OFF")}"),
								menu.Text($"    - GDB Server: {(configuration.EnableGdbServer ? "ON (port " + configuration.GdbServerPort + ")" : "OFF")}"),
								menu.Text($"    - File Logging: {(configuration.EnableFileLogging ? "ON" : "OFF")}"),
								menu.Text(""),
								menu.Text("  🎮 EmulatorLauncher Integration"),
								menu.Text("    - Launch argument building implemented"),
								menu.Text("    - Configuration injection ready"),
								menu.Text("    - Stats tracking on launch prepared"),
								menu.Text(""),
								menu.Text("  ⏳ Coming Next: Interactive navigation UI"),
								menu.Text("     Keyboard controls, menu selection, game launching interface"),
								menu.Text("")
							]),
							title: "Backend Status"
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

