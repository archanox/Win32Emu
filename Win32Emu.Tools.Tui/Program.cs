using Hex1b;
using Microsoft.Extensions.Logging;
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

			logger.LogInformation("Starting Win32Emu TUI - Press Ctrl+C to exit");
			
			await using var terminal = Hex1bTerminal.CreateBuilder()
				.WithHex1bApp((app, options) => ctx =>
					ctx.VStack(main => [
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
						main.Border(
							main.VStack(menu => [
								menu.Text(""),
								menu.Text($"  Game Library ({gameLibrary.Games.Count} games)"),
								menu.Text(""),
								menu.Text("  Features:"),
								menu.Text("  • Browse and manage game library"),
								menu.Text("  • Add games with metadata (title, developer, year, etc.)"),
								menu.Text("  • Launch games with emulator"),
								menu.Text("  • Interactive debugger integration"),
								menu.Text("  • Configure rendering backends (SDL, GLFW, Vulkan, etc.)"),
								menu.Text(""),
								menu.Text("  Note: Full interactive menu system coming soon!"),
								menu.Text("  For now, press Ctrl+C to exit"),
								menu.Text("")
							]),
							title: "Main Menu"
						).Fill(),
						main.InfoBar([
							"Ctrl+C", "Exit",
							"Version", "0.1.0"
						])
					])
				)
				.WithMouse()
				.Build();

			await terminal.RunAsync();
			
			return 0;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Fatal error in TUI application");
			return 1;
		}
	}
}
