using Hex1b;
using Microsoft.Extensions.Logging;
using Win32Emu.Tools.Tui.Services;

namespace Win32Emu.Tools.Tui;

/// <summary>
/// TUI application entry point using Hex1b framework
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

			logger.LogInformation("Starting Win32Emu TUI - Press Ctrl+C to exit");
			
			// Create and run the Hex1b terminal with app
			await using var terminal = Hex1bTerminal.CreateBuilder()
				.WithHex1bApp((app, options) => ctx => ScreenBuilder.BuildScreen(ctx, appState))
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
