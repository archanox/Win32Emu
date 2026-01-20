using Microsoft.Extensions.Logging;
using Spectre.Console;
using Win32Emu.Tools.Tui.Models;

namespace Win32Emu.Tools.Tui.UI;

public class AddGameScreen
{
	private readonly AppState _state;

	public AddGameScreen(AppState state)
	{
		_state = state;
	}

	public async Task RunAsync()
	{
		Console.Clear();
		AnsiConsole.Write(new Rule("[yellow]Add New Game[/]"));
		AnsiConsole.WriteLine();

		var title = AnsiConsole.Ask<string>("Game [cyan]title[/]:");
		var path = AnsiConsole.Ask<string>("Executable [cyan]path[/]:");

		if (!File.Exists(path))
		{
			AnsiConsole.MarkupLine("[red]Error: Executable not found![/]");
			Console.ReadKey();
			return;
		}

		var developer = AnsiConsole.Ask<string>("Developer (optional, press Enter to skip):", string.Empty);
		var publisher = AnsiConsole.Ask<string>("Publisher (optional, press Enter to skip):", string.Empty);
		var genre = AnsiConsole.Ask<string>("Genre (optional, press Enter to skip):", string.Empty);
		var yearStr = AnsiConsole.Ask<string>("Release Year (optional, press Enter to skip):", string.Empty);

		int? year = null;
		if (!string.IsNullOrWhiteSpace(yearStr) && int.TryParse(yearStr, out var y))
		{
			year = y;
		}

		var game = new GameEntry
		{
			Title = title,
			ExecutablePath = path,
			Developer = string.IsNullOrWhiteSpace(developer) ? null : developer,
			Publisher = string.IsNullOrWhiteSpace(publisher) ? null : publisher,
			Genre = string.IsNullOrWhiteSpace(genre) ? null : genre,
			ReleaseYear = year
		};

		_state.GameLibrary.AddGame(game);
		await _state.GameLibrary.SaveLibraryAsync();

		AnsiConsole.MarkupLine("[green]Game added successfully![/]");
		await Task.Delay(1500);
	}
}

public class SettingsScreen
{
	private readonly AppState _state;

	public SettingsScreen(AppState state)
	{
		_state = state;
	}

	public void Run()
	{
		while (true)
		{
			Console.Clear();
			AnsiConsole.Write(new Rule("[yellow]Settings[/]"));
			AnsiConsole.WriteLine();

			var config = _state.Configuration;
			
			AnsiConsole.MarkupLine($"[cyan]Default Backend:[/] {config.DefaultBackend}");
			AnsiConsole.MarkupLine($"[cyan]Debug Mode:[/] {(config.EnableDebugMode ? "Enabled" : "Disabled")}");
			AnsiConsole.MarkupLine($"[cyan]Interactive Debugger:[/] {(config.EnableInteractiveDebugger ? "Enabled" : "Disabled")}");
			AnsiConsole.MarkupLine($"[cyan]GDB Server:[/] {(config.EnableGdbServer ? "Enabled" : "Disabled")}");
			AnsiConsole.MarkupLine($"[cyan]GDB Port:[/] {config.GdbServerPort}");
			AnsiConsole.MarkupLine($"[cyan]File Logging:[/] {(config.EnableFileLogging ? "Enabled" : "Disabled")}");
			AnsiConsole.WriteLine();

			var choice = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("[cyan]Select setting to change:[/]")
					.AddChoices(new[]
					{
						"Default Backend",
						"Toggle Debug Mode",
						"Toggle Interactive Debugger",
						"Toggle GDB Server",
						"Change GDB Port",
						"Toggle File Logging",
						"Back"
					}));

			if (choice == "Back")
				return;

			switch (choice)
			{
				case "Default Backend":
					var backends = Enum.GetNames(typeof(Rendering.BackendType));
					var backend = AnsiConsole.Prompt(
						new SelectionPrompt<string>()
							.Title("Select backend:")
							.AddChoices(backends));
					config.DefaultBackend = Enum.Parse<Rendering.BackendType>(backend);
					break;
				case "Toggle Debug Mode":
					config.EnableDebugMode = !config.EnableDebugMode;
					break;
				case "Toggle Interactive Debugger":
					config.EnableInteractiveDebugger = !config.EnableInteractiveDebugger;
					break;
				case "Toggle GDB Server":
					config.EnableGdbServer = !config.EnableGdbServer;
					break;
				case "Change GDB Port":
					config.GdbServerPort = AnsiConsole.Ask<int>("Enter GDB server port:");
					break;
				case "Toggle File Logging":
					config.EnableFileLogging = !config.EnableFileLogging;
					break;
			}
		}
	}
}

public class DebuggerScreen
{
	private readonly AppState _state;

	public DebuggerScreen(AppState state)
	{
		_state = state;
	}

	public void Run()
	{
		Console.Clear();
		AnsiConsole.Write(new Rule("[yellow]Interactive Debugger[/]"));
		AnsiConsole.WriteLine();

		AnsiConsole.MarkupLine("The interactive debugger provides step-through debugging capabilities.");
		AnsiConsole.MarkupLine("You can set breakpoints, inspect registers, and step through instructions.");
		AnsiConsole.WriteLine();

		var path = AnsiConsole.Ask<string>("Enter executable path to debug:");

		if (!File.Exists(path))
		{
			AnsiConsole.MarkupLine("[red]Error: Executable not found![/]");
			Console.ReadKey();
			return;
		}

		try
		{
			var args = new List<string> { path, "--nogui", "--interactive-debug" };
			var exitCode = EmulatorLauncher.Launch(args.ToArray());
			AnsiConsole.MarkupLine($"[green]Debugger session ended (exit code: {exitCode})[/]");
		}
		catch (Exception ex)
		{
			_state.Logger.LogError(ex, "Failed to launch debugger");
			AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
		}

		Console.ReadKey();
	}
}

public class HelpScreen
{
	private readonly AppState _state;

	public HelpScreen(AppState state)
	{
		_state = state;
	}

	public void Run()
	{
		Console.Clear();
		AnsiConsole.Write(new Rule("[yellow]Win32Emu TUI - Help[/]"));
		AnsiConsole.WriteLine();

		var panel = new Panel(
			"Win32Emu TUI provides a terminal-based interface for managing and running\n" +
			"classic Windows games. It's optimized for 80-column mode, making it perfect\n" +
			"for SSH access from mobile devices.");
		panel.Header = new PanelHeader("[cyan]Overview[/]");
		panel.Border = BoxBorder.Rounded;
		AnsiConsole.Write(panel);
		AnsiConsole.WriteLine();

		AnsiConsole.MarkupLine("[cyan]FEATURES:[/]");
		AnsiConsole.MarkupLine("  • Game library management");
		AnsiConsole.MarkupLine("  • Interactive debugger integration");
		AnsiConsole.MarkupLine("  • Multiple rendering backends");
		AnsiConsole.MarkupLine("  • 80-column display for mobile SSH");
		AnsiConsole.WriteLine();

		AnsiConsole.MarkupLine("[cyan]NAVIGATION:[/]");
		AnsiConsole.MarkupLine("  • Use arrow keys to navigate");
		AnsiConsole.MarkupLine("  • Press Enter to select");
		AnsiConsole.MarkupLine("  • Follow on-screen prompts");
		AnsiConsole.WriteLine();

		AnsiConsole.MarkupLine("Press any key to return to main menu...");
		Console.ReadKey();
	}
}
