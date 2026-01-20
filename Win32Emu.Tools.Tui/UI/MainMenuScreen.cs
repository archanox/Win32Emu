using Spectre.Console;
using Win32Emu.Tools.Tui.Models;

namespace Win32Emu.Tools.Tui.UI;

/// <summary>
/// Main menu screen - entry point for the TUI
/// Provides navigation to all major features
/// </summary>
public class MainMenuScreen
{
	private readonly AppState _state;

	public MainMenuScreen(AppState state)
	{
		_state = state;
	}

	public async Task RunAsync()
	{
		while (true)
		{
			Console.Clear();
			
			// Display header
			var header = new Panel(new Markup("[yellow]Win32Emu - TUI Edition[/]"))
				.Border(BoxBorder.Double)
				.BorderColor(Color.Yellow);
			AnsiConsole.Write(header);
			
			AnsiConsole.WriteLine();
			
			// Show menu
			var choice = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("[cyan]Main Menu[/]")
					.PageSize(10)
					.AddChoices(new[]
					{
						"Game Library",
						"Add Game",
						"Settings",
						"Interactive Debugger",
						"Help",
						"Exit"
					}));

			switch (choice)
			{
				case "Game Library":
					await new GameLibraryScreen(_state).RunAsync();
					break;
				case "Add Game":
					await new AddGameScreen(_state).RunAsync();
					break;
				case "Settings":
					new SettingsScreen(_state).Run();
					break;
				case "Interactive Debugger":
					new DebuggerScreen(_state).Run();
					break;
				case "Help":
					new HelpScreen(_state).Run();
					break;
				case "Exit":
					AnsiConsole.MarkupLine("[yellow]Thank you for using Win32Emu TUI![/]");
					return;
			}
		}
	}
}
