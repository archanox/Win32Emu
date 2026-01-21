using Hex1b;
using Hex1b.Input;
using Microsoft.Extensions.Logging;
using Win32Emu.Tools.Tui.Models;
using Win32Emu.Tools.Tui.Services;
using WidgetArray = Hex1b.Widgets.Hex1bWidget[];

namespace Win32Emu.Tools.Tui;

internal class Program
{
	// UI Layout Constants
	private const int HEADER_HEIGHT = 7;
	private const int HEADER_HEIGHT_EXTENDED = 8;
	private const int MAIN_MENU_HEIGHT = 11;
	private const int ADD_GAME_FORM_HEIGHT = 12;
	private const int GAME_LIBRARY_EMPTY_HEIGHT = 4;
	private const int GAME_LIBRARY_LIST_HEIGHT = 13;
	private const int SETTINGS_LIST_HEIGHT = 10;
	
	// Menu Item Indices
	private const int MENU_GAME_LIBRARY = 0;
	private const int MENU_ADD_GAME = 1;
	private const int MENU_SETTINGS = 2;
	private const int MENU_HELP = 3;
	private const int MENU_EXIT = 4;
	
	// Add Game Form Indices
	private const int FORM_FIELD_TITLE = 0;
	private const int FORM_FIELD_PATH = 1;
	private const int FORM_FIELD_DEVELOPER = 2;
	private const int FORM_FIELD_PUBLISHER = 3;
	private const int FORM_FIELD_GENRE = 4;
	private const int FORM_FIELD_YEAR = 5;
	private const int FORM_FIELD_DESCRIPTION = 6;
	private const int FORM_BUTTON_SAVE = 7;
	private const int FORM_BUTTON_CANCEL = 8;
	
	// Settings Menu Indices
	private const int SETTING_BACKEND = 0;
	private const int SETTING_DEBUG_MODE = 1;
	private const int SETTING_INTERACTIVE_DEBUGGER = 2;
	private const int SETTING_GDB_SERVER = 3;
	private const int SETTING_GDB_PORT = 4;
	private const int SETTING_FILE_LOGGING = 5;
	
	// Configuration Values
	private static readonly Win32Emu.Rendering.BackendType[] AllowedBackends = [
		Win32Emu.Rendering.BackendType.SDL,
		Win32Emu.Rendering.BackendType.GLFW,
		Win32Emu.Rendering.BackendType.Vulkan,
		Win32Emu.Rendering.BackendType.Metal,
		Win32Emu.Rendering.BackendType.Software
	];
	
	private static readonly int[] GdbServerPorts = [1234, 2345, 3456, 4567, 5678];
	
	private static AppState? _state;
	private static string? _errorMessage;

	private static Hex1b.Widgets.BorderWidget CreateHeaderBorder(Hex1b.RootContext ctx, string title, bool extended = false)
	{
		return ctx.Border(
			ctx.VStack(header => {
				WidgetArray widgets = [
					header.Text(""),
					header.Text("  ╔════════════════════════════════════════════════════╗"),
					header.Text("  ║   Win32Emu - Terminal User Interface              ║"),
					extended ? header.Text("  ║   Windows 32-bit PE Emulator                      ║") : null!,
					header.Text("  ╚════════════════════════════════════════════════════╝"),
					header.Text("")
				];
				return widgets.Where(w => w != null!).ToArray();
			}),
			title: title
		).FixedHeight(extended ? HEADER_HEIGHT_EXTENDED : HEADER_HEIGHT);
	}

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
							CreateHeaderBorder(ctx, "Welcome", extended: true),
							main.Border(
								main.VStack(menu => [
									menu.Text(""),
									menu.Text("  Select an option:"),
									menu.Text(""),
									menu.List(menuItems)
										.OnItemActivated(e => {
											_state.CurrentView = e.ActivatedIndex switch {
												MENU_GAME_LIBRARY => ViewMode.GameLibrary,
												MENU_ADD_GAME => ViewMode.AddGame,
												MENU_SETTINGS => ViewMode.Settings,
												MENU_HELP => ViewMode.Help,
												MENU_EXIT => ViewMode.MainMenu,
												_ => ViewMode.MainMenu
											};
											if (e.ActivatedIndex == MENU_EXIT) Environment.Exit(0);
											if (e.ActivatedIndex == MENU_ADD_GAME) _state.ResetNewGameEntry();
										})
										.FixedHeight(MAIN_MENU_HEIGHT),
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
							CreateHeaderBorder(ctx, "Welcome"),
							main.Border(
								main.VStack(content => {
									WidgetArray widgets = [
										content.Text(""),
										content.Text("  Add New Game"),
										content.Text("  Select a field to edit, then type and press Enter:"),
										content.Text(""),
										_errorMessage != null ? content.Text($"  ⚠️  {_errorMessage}") : null!,
										_errorMessage != null ? content.Text("") : null!,
										content.List(fields)
											.OnItemActivated(e => {
												_errorMessage = null; // Clear error on new action
												if (e.ActivatedIndex == FORM_BUTTON_SAVE)
												{
													if (!string.IsNullOrWhiteSpace(_state.NewGameEntry.Title) && 
													    !string.IsNullOrWhiteSpace(_state.NewGameEntry.ExecutablePath))
													{
														_state.GameLibrary.AddGame(_state.NewGameEntry);
														_state.GameLibrary.SaveLibraryAsync().GetAwaiter().GetResult();
														_state.ResetNewGameEntry();
														_state.CurrentView = ViewMode.GameLibrary;
													}
													else
													{
														_errorMessage = "Title and Executable Path are required";
													}
												}
												else if (e.ActivatedIndex == FORM_BUTTON_CANCEL)
												{
													_state.ResetNewGameEntry();
													_state.CurrentView = ViewMode.MainMenu;
												}
												else
												{
													_state.AddGameFieldIndex = e.ActivatedIndex;
												}
											})
											.WithInputBindings(bindings => {
												bindings.Key(Hex1bKey.Escape).Action(() => {
													_errorMessage = null;
													_state.ResetNewGameEntry();
													_state.CurrentView = ViewMode.MainMenu;
												});
											})
											.FixedHeight(ADD_GAME_FORM_HEIGHT),
										content.Text("")
									];
									return widgets.Where(w => w != null!).ToArray();
								}),
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
								CreateHeaderBorder(ctx, "Welcome"),
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
											.WithInputBindings(bindings => {
												bindings.Key(Hex1bKey.Escape).Action(() => {
													_state.CurrentView = ViewMode.MainMenu;
												});
											})
											.FixedHeight(GAME_LIBRARY_EMPTY_HEIGHT),
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
							CreateHeaderBorder(ctx, "Welcome"),
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
										.WithInputBindings(bindings => {
											bindings.Key(Hex1bKey.Escape).Action(() => {
												_state.CurrentView = ViewMode.MainMenu;
											});
										})
										.FixedHeight(GAME_LIBRARY_LIST_HEIGHT),
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
							CreateHeaderBorder(ctx, "Welcome"),
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
								])
								.WithInputBindings(bindings => {
									bindings.Key(Hex1bKey.Escape).Action(() => {
										_state.CurrentView = ViewMode.GameLibrary;
									});
								}),
								title: $"Game: {game.Title}"
							).Fill(),
							main.InfoBar(["ESC", "Back"])
						]);
					}
					// Settings
					else if (_state.CurrentView == ViewMode.Settings)
					{
						var config = _state.Configuration;
						
						var settingsItems = new[] {
							$"Backend: {config.DefaultBackend}",
							$"Debug Mode: {(config.EnableDebugMode ? "ON" : "OFF")}",
							$"Interactive Debugger: {(config.EnableInteractiveDebugger ? "ON" : "OFF")}",
							$"GDB Server: {(config.EnableGdbServer ? "ON" : "OFF")}",
							$"GDB Server Port: {config.GdbServerPort}",
							$"File Logging: {(config.EnableFileLogging ? "ON" : "OFF")}"
						};
						
						return ctx.VStack(main => [
							CreateHeaderBorder(ctx, "Welcome"),
							main.Border(
								main.VStack(content => [
									content.Text(""),
									content.Text("  Settings - Press Enter to toggle or change"),
									content.Text(""),
									content.List(settingsItems)
										.OnItemActivated(e => {
											switch (e.ActivatedIndex)
											{
												case SETTING_BACKEND:
													var currentBackendIndex = Array.IndexOf(AllowedBackends, config.DefaultBackend);
													if (currentBackendIndex == -1)
													{
														currentBackendIndex = 0;
													}
													var nextBackendIndex = (currentBackendIndex + 1) % AllowedBackends.Length;
													config.DefaultBackend = AllowedBackends[nextBackendIndex];
													break;
												case SETTING_DEBUG_MODE:
													config.EnableDebugMode = !config.EnableDebugMode;
													break;
												case SETTING_INTERACTIVE_DEBUGGER:
													config.EnableInteractiveDebugger = !config.EnableInteractiveDebugger;
													break;
												case SETTING_GDB_SERVER:
													config.EnableGdbServer = !config.EnableGdbServer;
													break;
												case SETTING_GDB_PORT:
													var currentPortIndex = Array.IndexOf(GdbServerPorts, config.GdbServerPort);
													if (currentPortIndex == -1) currentPortIndex = 0;
													config.GdbServerPort = GdbServerPorts[(currentPortIndex + 1) % GdbServerPorts.Length];
													break;
												case SETTING_FILE_LOGGING:
													config.EnableFileLogging = !config.EnableFileLogging;
													break;
											}
										})
										.WithInputBindings(bindings => {
											bindings.Key(Hex1bKey.Escape).Action(() => {
												_state.CurrentView = ViewMode.MainMenu;
											});
										})
										.FixedHeight(SETTINGS_LIST_HEIGHT),
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
							CreateHeaderBorder(ctx, "Welcome"),
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
								])
								.WithInputBindings(bindings => {
									bindings.Key(Hex1bKey.Escape).Action(() => {
										_state.CurrentView = ViewMode.MainMenu;
									});
								}),
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
