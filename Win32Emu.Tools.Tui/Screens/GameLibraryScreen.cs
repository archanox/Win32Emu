using Hex1b;
using Hex1b.Widgets;
using Microsoft.Extensions.Logging;
using Win32Emu.Tools.Tui.Models;

namespace Win32Emu.Tools.Tui.Screens;

/// <summary>
/// Game library screen showing list of games
/// Supports 80-column mode with truncation
/// </summary>
public static class GameLibraryScreen
{
	public static Hex1bWidget Build(IHex1bContext ctx, AppState state)
	{
		var games = state.GameLibrary.Games;
		
		if (games.Count == 0)
		{
			return ctx.VStack([
				ctx.Border().WithTitle("Game Library")
					.Child(ctx.Text("No games in library. Press 'A' to add games.")),
				new SeparatorWidget(),
				ctx.InfoBar("ESC: Back | A: Add Game")
			]);
		}

		// Create game list items with 80-column truncation
		var gameItems = games.Select((g, i) =>
		{
			var title = g.Title.Length > 50 ? g.Title[..47] + "..." : g.Title;
			var dev = g.Developer ?? "Unknown";
			if (dev.Length > 20) dev = dev[..17] + "...";
			
			return $"{i + 1,3}. {title,-50} | {dev,-20} | Plays: {g.PlayCount,3}";
		}).ToArray();

		return ctx.VStack([
			// Header
			ctx.Border().WithTitle($"Game Library ({games.Count} games)")
				.Child(ctx.Text("Select a game to view details and launch")),
			
			new SeparatorWidget(),
			
			// Game list
			ctx.List(gameItems)
				.Fill()
				.OnSelectionChanged(index => state.SelectedGameIndex = index)
				.OnItemActivated(index => ShowGameDetails(state, games[index])),
			
			// Footer
			ctx.InfoBar("Arrow keys: Navigate | Enter: Details | D: Delete | ESC: Back | A: Add")
		]);
	}

	private static void ShowGameDetails(AppState state, GameEntry game)
	{
		// For now, just try to launch the game
		// In a real implementation, this would show a details screen
		try
		{
			state.Logger.LogInformation("Launching game: {Title}", game.Title);
			
			// Update play stats
			state.GameLibrary.UpdateGameStats(game.Id);
			_ = state.GameLibrary.SaveLibraryAsync();
			
			// Build emulator arguments
			var args = state.Configuration.BuildEmulatorArgs(game.ExecutablePath);
			
			// Launch the emulator (this will take over the terminal)
			var exitCode = EmulatorLauncher.Launch(args);
			
			state.Logger.LogInformation("Game exited with code: {ExitCode}", exitCode);
		}
		catch (Exception ex)
		{
			state.Logger.LogError(ex, "Failed to launch game: {Title}", game.Title);
		}
	}
}
