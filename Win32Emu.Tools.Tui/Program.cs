using Hex1b;
using Microsoft.Extensions.Logging;
using Win32Emu.Tools.Tui.Models;
using Win32Emu.Tools.Tui.Services;

namespace Win32Emu.Tools.Tui;

internal class Program
{
	private static AppState? _state;

	private static async Task<int> Main(string[] args)
	{
		using var loggerFactory = LoggerFactory.Create(builder =>
		{
			builder.AddConsole().SetMinimumLevel(LogLevel.Warning);
		});

		var logger = loggerFactory.CreateLogger<Program>();
		
		try
		{
			var gameLibrary = new GameLibraryService(logger);
			await gameLibrary.LoadLibraryAsync();

			var configuration = new ConfigurationService(logger);
			_state = new AppState(gameLibrary, configuration);
			
			await using var terminal = Hex1bTerminal.CreateBuilder()
				.WithHex1bApp((app, options) => ctx =>
				{
					// Main Menu
					if (_state.CurrentView == ViewMode.MainMenu)
					{
						var menuItems = new[] { "📚 Game Library", "⚙️  Settings", "❓ Help", "🚪 Exit" };
						return ctx.VStack(main => [
							main.Border(
								main.VStack(header => [
									header.Text(""),
									header.Text("  ╔════════════════════════════════════════════════════╗"),
									header.Text("  ║   Win32Emu - Terminal User Interface              ║"),
									header.Text("  ║   Windows 32-bit PE Emulator                      ║"),
									header.Text("  ╚════════════════════════════════════════════════════╝"),
									header.Text("")
								]),
								title: "Welcome"
							).FixedHeight(8),
							main.Border(
								main.VStack(menu => [
									menu.Text(""),
									menu.Text("  Select an option:"),
									menu.Text(""),
									menu.List(menuItems)
										.OnItemActivated(e => {
											_state.CurrentView = e.ActivatedIndex switch {
												0 => ViewMode.GameLibrary,
												1 => ViewMode.Settings,
												2 => ViewMode.Help,
												3 => ViewMode.MainMenu,
												_ => ViewMode.MainMenu
											};
											if (e.ActivatedIndex == 3) Environment.Exit(0);
										})
										.FixedHeight(10),
									menu.Text("")
								]),
								title: "Main Menu"
							).Fill(),
							main.InfoBar(["↑↓", "Navigate", "Enter", "Select", "Ctrl+C", "Exit"])
						]);
					}
					// Game Library
					else if (_state.CurrentView == ViewMode.GameLibrary)
					{
						var games = _state.GameLibrary.Games;
						if (games.Count == 0)
						{
							return ctx.VStack(main => [
								main.Border(
									main.VStack(header => [
										header.Text(""),
										header.Text("  ╔════════════════════════════════════════════════════╗"),
										header.Text("  ║   Win32Emu - Terminal User Interface              ║"),
										header.Text("  ╚════════════════════════════════════════════════════╝"),
										header.Text("")
									]),
									title: "Welcome"
								).FixedHeight(7),
								main.Border(
									main.VStack(content => [
										content.Text(""),
										content.Text("  No games in library yet."),
										content.Text(""),
										content.Text("  Press ESC to return to main menu."),
										content.Text("")
									]),
									title: "Game Library"
								).Fill(),
								main.InfoBar(["ESC", "Back"])
							]);
						}
						
						var gameDisplayNames = games.Select(g => 
							$"{g.Title} ({g.ReleaseYear?.ToString() ?? "Unknown"})"
						).ToArray();
						
						return ctx.VStack(main => [
							main.Border(
								main.VStack(header => [
									header.Text(""),
									header.Text("  ╔════════════════════════════════════════════════════╗"),
									header.Text("  ║   Win32Emu - Terminal User Interface              ║"),
									header.Text("  ╚════════════════════════════════════════════════════╝"),
									header.Text("")
								]),
								title: "Welcome"
							).FixedHeight(7),
							main.Border(
								main.VStack(content => [
									content.Text(""),
									content.Text($"  Total games: {games.Count}"),
									content.Text(""),
									content.List(gameDisplayNames)
										.OnItemActivated(e => {
											_state.SelectedGame = games[e.ActivatedIndex];
											_state.CurrentView = ViewMode.GameDetails;
										})
										.FixedHeight(12),
									content.Text("")
								]),
								title: "Game Library"
							).Fill(),
							main.InfoBar(["↑↓", "Navigate", "Enter", "Details", "ESC", "Back"])
						]);
					}
					// Game Details
					else if (_state.CurrentView == ViewMode.GameDetails)
					{
						var game = _state.SelectedGame;
						if (game == null)
						{
							_state.CurrentView = ViewMode.GameLibrary;
							return ctx.Text("Redirecting...");
						}
						
						return ctx.VStack(main => [
							main.Border(
								main.VStack(header => [
									header.Text(""),
									header.Text("  ╔════════════════════════════════════════════════════╗"),
									header.Text("  ║   Win32Emu - Terminal User Interface              ║"),
									header.Text("  ╚════════════════════════════════════════════════════╝"),
									header.Text("")
								]),
								title: "Welcome"
							).FixedHeight(7),
							main.Border(
								main.VStack(content => [
									content.Text(""),
									content.Text($"  Title: {game.Title}"),
									content.Text($"  Developer: {game.Developer ?? "Unknown"}"),
									content.Text($"  Publisher: {game.Publisher ?? "Unknown"}"),
									content.Text($"  Genre: {game.Genre ?? "Unknown"}"),
									content.Text($"  Year: {game.ReleaseYear?.ToString() ?? "Unknown"}"),
									content.Text(""),
									content.Text($"  Path: {game.ExecutablePath}"),
									content.Text($"  Added: {game.AddedDate:yyyy-MM-dd}"),
									content.Text($"  Last Played: {game.LastPlayed?.ToString("yyyy-MM-dd") ?? "Never"}"),
									content.Text($"  Times Played: {game.PlayCount}"),
									content.Text(""),
									content.Text($"  {game.Description ?? "No description."}"),
									content.Text("")
								]),
								title: $"Game: {game.Title}"
							).Fill(),
							main.InfoBar(["ESC", "Back"])
						]);
					}
					// Settings
					else if (_state.CurrentView == ViewMode.Settings)
					{
						var config = _state.Configuration;
						var backends = Enum.GetNames(typeof(Win32Emu.Rendering.BackendType));
						
						return ctx.VStack(main => [
							main.Border(
								main.VStack(header => [
									header.Text(""),
									header.Text("  ╔════════════════════════════════════════════════════╗"),
									header.Text("  ║   Win32Emu - Terminal User Interface              ║"),
									header.Text("  ╚════════════════════════════════════════════════════╝"),
									header.Text("")
								]),
								title: "Welcome"
							).FixedHeight(7),
							main.Border(
								main.VStack(content => [
									content.Text(""),
									content.Text("  Backend Settings:"),
									content.Text(""),
									content.Text($"  Current Backend: {config.DefaultBackend}"),
									content.List(backends)
										.OnItemActivated(e => {
											var backend = Enum.Parse<Win32Emu.Rendering.BackendType>(backends[e.ActivatedIndex]);
											config.DefaultBackend = backend;
										})
										.FixedHeight(8),
									content.Text(""),
									content.Text($"  Debug Mode: {(config.EnableDebugMode ? "ON" : "OFF")}"),
									content.Text($"  Interactive Debugger: {(config.EnableInteractiveDebugger ? "ON" : "OFF")}"),
									content.Text($"  GDB Server: {(config.EnableGdbServer ? "ON" : "OFF")} (Port: {config.GdbServerPort})"),
									content.Text($"  File Logging: {(config.EnableFileLogging ? "ON" : "OFF")}"),
									content.Text("")
								]),
								title: "Settings"
							).Fill(),
							main.InfoBar(["↑↓", "Navigate", "Enter", "Select", "ESC", "Back"])
						]);
					}
					// Help
					else if (_state.CurrentView == ViewMode.Help)
					{
						return ctx.VStack(main => [
							main.Border(
								main.VStack(header => [
									header.Text(""),
									header.Text("  ╔════════════════════════════════════════════════════╗"),
									header.Text("  ║   Win32Emu - Terminal User Interface              ║"),
									header.Text("  ╚════════════════════════════════════════════════════╝"),
									header.Text("")
								]),
								title: "Welcome"
							).FixedHeight(7),
							main.Border(
								main.VStack(content => [
									content.Text(""),
									content.Text("  Keyboard Shortcuts:"),
									content.Text(""),
									content.Text("  Navigation:"),
									content.Text("    ↑/↓         Navigate lists"),
									content.Text("    Enter       Select item"),
									content.Text("    ESC         Go back"),
									content.Text(""),
									content.Text("  Main Menu:"),
									content.Text("    Use arrow keys to navigate"),
									content.Text("    Press Enter to select"),
									content.Text(""),
									content.Text("  Game Library:"),
									content.Text("    Browse your game collection"),
									content.Text("    Press Enter for game details"),
									content.Text(""),
									content.Text("  General:"),
									content.Text("    Ctrl+C      Exit application"),
									content.Text("")
								]),
								title: "Help"
							).Fill(),
							main.InfoBar(["ESC", "Back"])
						]);
					}
					else
					{
						// Default to Main Menu
						_state.CurrentView = ViewMode.MainMenu;
						return ctx.Text("Loading...");
					}
				})
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
