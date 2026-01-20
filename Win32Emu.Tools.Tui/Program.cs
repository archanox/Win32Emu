using Microsoft.Extensions.Logging;
using Spectre.Console;
using Win32Emu.Tools.Tui.Models;
using Win32Emu.Tools.Tui.Services;
using Win32Emu.Tools.Tui.UI;

namespace Win32Emu.Tools.Tui;

/// <summary>
/// TUI application entry point using Spectre.Console framework
/// Provides terminal-based interface for Win32Emu with 80-column mode support
/// </summary>
internal class Program
{
	private static async Task<int> Main(string[] args)
	{
		// Create logger factory
		using var loggerFactory = LoggerFactory.Create(builder =>
		{
			builder
				.AddConsole()
				.SetMinimumLevel(LogLevel.Information);
		});

		var logger = loggerFactory.CreateLogger<Program>();

		try
		{
			// Initialize services
			var gameLibrary = new GameLibraryService(logger);
			await gameLibrary.LoadLibraryAsync();

			var configService = new ConfigurationService(logger);
			
			// Create app state
			var appState = new AppState(gameLibrary, configService, logger);

			// Create and run the main UI
			var mainMenu = new MainMenuScreen(appState);
			await mainMenu.RunAsync();
			
			return 0;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Fatal error in TUI application");
			AnsiConsole.MarkupLine("[red]Error: {0}[/]", ex.Message);
			return 1;
		}
	}
}
