using Microsoft.Extensions.Logging;
using Spectre.Console;
using Win32Emu.Tools.Tui.Models;

namespace Win32Emu.Tools.Tui.UI;

/// <summary>
/// Game library screen showing list of games
/// Supports 80-column mode for mobile SSH access
/// </summary>
public class GameLibraryScreen
{
	private readonly AppState _state;

	public GameLibraryScreen(AppState state)
	{
		_state = state;
	}

	public async Task RunAsync()
	{
		while (true)
		{
			Console.Clear();
			
			var games = _state.GameLibrary.Games;
			
			if (games.Count == 0)
			{
				AnsiConsole.MarkupLine("[yellow]No games in library.[/]");
				AnsiConsole.MarkupLine("Press [cyan]Enter[/] to return to main menu.");
				Console.ReadKey();
				return;
			}

			// Create table for 80-column display
			var table = new Table();
			table.BorderColor(Color.Blue);
			table.AddColumn(new TableColumn("#").RightAligned());
			table.AddColumn(new TableColumn("Title").Width(40));
			table.AddColumn(new TableColumn("Developer").Width(20));
			table.AddColumn(new TableColumn("Plays").RightAligned());

			for (var i = 0; i < games.Count; i++)
			{
				var game = games[i];
				var title = game.Title.Length > 38 ? game.Title[..35] + "..." : game.Title;
				var dev = (game.Developer ?? "Unknown").Length > 18 ? game.Developer![..15] + "..." : game.Developer ?? "Unknown";
				
				table.AddRow(
					(i + 1).ToString(),
					title,
					dev,
					game.PlayCount.ToString());
			}

			AnsiConsole.Write(table);
			AnsiConsole.WriteLine();

			var choices = games.Select((g, i) => $"{i + 1}. {g.Title}").ToList();
			choices.Add("Add New Game");
			choices.Add("Back to Main Menu");

			var choice = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("[cyan]Select a game or action:[/]")
					.PageSize(15)
					.AddChoices(choices));

			if (choice == "Back to Main Menu")
			{
				return;
			}
			else if (choice == "Add New Game")
			{
				await new AddGameScreen(_state).RunAsync();
			}
			else
			{
				// Extract game index from choice
				var index = int.Parse(choice.Split('.')[0]) - 1;
				await ShowGameDetailsAsync(games[index]);
			}
		}
	}

	private async Task ShowGameDetailsAsync(GameEntry game)
	{
		Console.Clear();
		
		var panel = new Panel(new Markup($"[yellow]{game.Title}[/]"))
			.Border(BoxBorder.Double)
			.BorderColor(Color.Yellow);
		AnsiConsole.Write(panel);
		AnsiConsole.WriteLine();

		var grid = new Grid();
		grid.AddColumn();
		grid.AddColumn();

		grid.AddRow("[cyan]Executable:[/]", game.ExecutablePath);
		if (game.Developer != null)
			grid.AddRow("[cyan]Developer:[/]", game.Developer);
		if (game.Publisher != null)
			grid.AddRow("[cyan]Publisher:[/]", game.Publisher);
		if (game.Genre != null)
			grid.AddRow("[cyan]Genre:[/]", game.Genre);
		if (game.ReleaseYear.HasValue)
			grid.AddRow("[cyan]Year:[/]", game.ReleaseYear.ToString()!);
		
		grid.AddRow("[cyan]Added:[/]", game.AddedDate.ToString("yyyy-MM-dd"));
		if (game.LastPlayed.HasValue)
			grid.AddRow("[cyan]Last Played:[/]", game.LastPlayed?.ToString("yyyy-MM-dd HH:mm") ?? "Never");
		grid.AddRow("[cyan]Play Count:[/]", game.PlayCount.ToString());

		AnsiConsole.Write(grid);
		AnsiConsole.WriteLine();

		var action = AnsiConsole.Prompt(
			new SelectionPrompt<string>()
				.Title("[cyan]What would you like to do?[/]")
				.AddChoices(new[] { "Launch Game", "Delete Game", "Back" }));

		switch (action)
		{
			case "Launch Game":
				await LaunchGameAsync(game);
				break;
			case "Delete Game":
				if (AnsiConsole.Confirm($"Are you sure you want to delete '{game.Title}'?"))
				{
					_state.GameLibrary.RemoveGame(game.Id);
					await _state.GameLibrary.SaveLibraryAsync();
					AnsiConsole.MarkupLine("[green]Game deleted successfully![/]");
					await Task.Delay(1500);
				}
				break;
		}
	}

	private async Task LaunchGameAsync(GameEntry game)
	{
		try
		{
			AnsiConsole.Status()
				.Start("Launching game...", ctx =>
				{
					ctx.Spinner(Spinner.Known.Dots);
					ctx.Status($"Starting {game.Title}...");
					
					// Update play stats
					_state.GameLibrary.UpdateGameStats(game.Id);
					_ = _state.GameLibrary.SaveLibraryAsync();
					
					// Build emulator arguments
					var args = _state.Configuration.BuildEmulatorArgs(game.ExecutablePath);
					
					// Launch the emulator (this will take over the terminal)
					var exitCode = EmulatorLauncher.Launch(args);
					
					_state.Logger.LogInformation("Game exited with code: {ExitCode}", exitCode);
				});

			AnsiConsole.MarkupLine("[green]Game session completed![/]");
			await Task.Delay(2000);
		}
		catch (Exception ex)
		{
			_state.Logger.LogError(ex, "Failed to launch game");
			AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
			Console.ReadKey();
		}
	}
}
