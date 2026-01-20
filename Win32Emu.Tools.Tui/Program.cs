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
						var menuItems = new[] { "📚 Game Library", "➕ Add Game", "⚙️  Settings", "❓ Help", "🚪 Exit" };
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
												1 => ViewMode.AddGame,
												2 => ViewMode.Settings,
												3 => ViewMode.Help,
												4 => ViewMode.MainMenu,
												_ => ViewMode.MainMenu
											};
											if (e.ActivatedIndex == 4) Environment.Exit(0);
											if (e.ActivatedIndex == 1) _state.ResetNewGameEntry();
										})
										.FixedHeight(11),
									menu.Text("")
								]),
								title: "Main Menu"
							).Fill(),
							main.InfoBar(["↑↓", "Navigate", "Enter", "Select", "Ctrl+C", "Exit"])
						]);
					}
					// Add Game
					else if (_state.CurrentView == ViewMode.AddGame)
					{
						var fields = new[] {
							$"Title: {_state.NewGameEntry.Title}",
							$"Executable Path: {_state.NewGameEntry.ExecutablePath}",
							$"Developer: {_state.NewGameEntry.Developer ?? "(optional)"}",
							$"Publisher: {_state.NewGameEntry.Publisher ?? "(optional)"}",
							$"Genre: {_state.NewGameEntry.Genre ?? "(optional)"}",
							$"Release Year: {_state.NewGameEntry.ReleaseYear?.ToString() ?? "(optional)"}",
							$"Description: {_state.NewGameEntry.Description ?? "(optional)"}",
							"[Save Game]",
							"[Cancel]"
						};

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
									content.Text("  Add New Game"),
									content.Text("  Select a field to edit, then type and press Enter:"),
									content.Text(""),
									content.List(fields)
										.OnItemActivated(e => {
											if (e.ActivatedIndex == 7) // Save
											{
												if (!string.IsNullOrWhiteSpace(_state.NewGameEntry.Title) && 
												    !string.IsNullOrWhiteSpace(_state.NewGameEntry.ExecutablePath))
												{
													_state.GameLibrary.AddGame(_state.NewGameEntry);
													_state.ResetNewGameEntry();
													_state.CurrentView = ViewMode.GameLibrary;
												}
											}
											else if (e.ActivatedIndex == 8) // Cancel
											{
												_state.ResetNewGameEntry();
												_state.CurrentView = ViewMode.MainMenu;
											}
											else
											{
												_state.AddGameFieldIndex = e.ActivatedIndex;
											}
										})
										.FixedHeight(12),
									content.Text("")
								]),
								title: "Add Game"
							).Fill(),
							main.InfoBar(["↑↓", "Navigate", "Enter", "Edit/Select", "ESC", "Cancel"])
						]);
					}
					// Game Library
					else if (_state.CurrentView == ViewMode.GameLibrary)
					{
						var games = _state.GameLibrary.Games;
						if (games.Count == 0)
						{
							var emptyMenuItems = new[] { "[Add New Game]", "[Back to Main Menu]" };
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
										content.Text("  Add your first game to get started!"),
										content.Text(""),
										content.List(emptyMenuItems)
											.OnItemActivated(e => {
												if (e.ActivatedIndex == 0)
												{
													_state.ResetNewGameEntry();
													_state.CurrentView = ViewMode.AddGame;
												}
												else
												{
													_state.CurrentView = ViewMode.MainMenu;
												}
											})
											.FixedHeight(4),
										content.Text("")
									]),
									title: "Game Library"
								).Fill(),
								main.InfoBar(["↑↓", "Navigate", "Enter", "Select", "ESC", "Back"])
							]);
						}
						
						// Create a list with games + "Add New Game" option
						var allItems = games.Select(g => 
							$"{g.Title} ({g.ReleaseYear?.ToString() ?? "Unknown"})"
						).Append("[Add New Game]").ToArray();
						
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
									content.List(allItems)
										.OnItemActivated(e => {
											if (e.ActivatedIndex == games.Count) // "Add New Game" option
											{
												_state.ResetNewGameEntry();
												_state.CurrentView = ViewMode.AddGame;
											}
											else
											{
												_state.SelectedGame = games[e.ActivatedIndex];
												_state.CurrentView = ViewMode.GameDetails;
											}
										})
										.FixedHeight(13),
									content.Text("")
								]),
								title: "Game Library"
							).Fill(),
							main.InfoBar(["↑↓", "Navigate", "Enter", "Select", "ESC", "Back"])
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
						
						var settingsItems = new[] {
							$"Backend: {config.DefaultBackend}",
							$"Debug Mode: {(config.EnableDebugMode ? "ON" : "OFF")}",
							$"Interactive Debugger: {(config.EnableInteractiveDebugger ? "ON" : "OFF")}",
							$"GDB Server: {(config.EnableGdbServer ? "ON" : "OFF")}",
							$"GDB Server Port: {config.GdbServerPort}",
							$"File Logging: {(config.EnableFileLogging ? "ON" : "OFF")}"
						};
						
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
									content.Text("  Settings - Press Enter to toggle or change"),
									content.Text(""),
									content.List(settingsItems)
										.OnItemActivated(e => {
											switch (e.ActivatedIndex)
											{
												case 0: // Backend - cycle through options
													var currentBackendIndex = Array.IndexOf(backends, config.DefaultBackend.ToString());
													var nextBackendIndex = (currentBackendIndex + 1) % backends.Length;
													config.DefaultBackend = Enum.Parse<Win32Emu.Rendering.BackendType>(backends[nextBackendIndex]);
													break;
												case 1: // Debug Mode
													config.EnableDebugMode = !config.EnableDebugMode;
													break;
												case 2: // Interactive Debugger
													config.EnableInteractiveDebugger = !config.EnableInteractiveDebugger;
													break;
												case 3: // GDB Server
													config.EnableGdbServer = !config.EnableGdbServer;
													break;
												case 4: // GDB Server Port - cycle through common ports
													var ports = new[] { 1234, 2345, 3456, 4567, 5678 };
													var currentPortIndex = Array.IndexOf(ports, config.GdbServerPort);
													if (currentPortIndex == -1) currentPortIndex = 0;
													config.GdbServerPort = ports[(currentPortIndex + 1) % ports.Length];
													break;
												case 5: // File Logging
													config.EnableFileLogging = !config.EnableFileLogging;
													break;
											}
										})
										.FixedHeight(10),
									content.Text(""),
									content.Text("  Use ↑↓ to navigate, Enter to toggle/change values"),
									content.Text("")
								]),
								title: "Settings"
							).Fill(),
							main.InfoBar(["↑↓", "Navigate", "Enter", "Toggle", "ESC", "Back"])
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
									content.Text("    Enter       Select/Edit/Toggle"),
									content.Text("    ESC         Go back"),
									content.Text(""),
									content.Text("  Main Menu:"),
									content.Text("    Navigate and select: Library, Add Game, Settings, Help"),
									content.Text(""),
									content.Text("  Game Library:"),
									content.Text("    Select a game to view details"),
									content.Text("    Select [Add New Game] to add games"),
									content.Text(""),
									content.Text("  Add Game:"),
									content.Text("    Note: Fields are display-only (text input coming soon)"),
									content.Text("    Select [Save Game] when done or [Cancel] to abort"),
									content.Text(""),
									content.Text("  Settings:"),
									content.Text("    Press Enter to toggle ON/OFF or cycle values"),
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
