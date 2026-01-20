using Hex1b;
using Hex1b.Widgets;
using Microsoft.Extensions.Logging;
using Win32Emu.Tools.Tui.Models;
using Win32Emu.Tools.Tui.Screens;
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
		// Create cancellation token for clean shutdown
		using var cts = new CancellationTokenSource();
		Console.CancelKeyPress += (_, e) =>
		{
			e.Cancel = true;
			cts.Cancel();
		};

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

			// Create and run the Hex1b app
			using var app = new Hex1bApp(async ctx =>
			{
				return await Task.FromResult(ScreenBuilder.BuildScreen(ctx, appState, cts));
			});
			
			logger.LogInformation("Starting Win32Emu TUI - Press Ctrl+C to exit");
			await app.RunAsync(cts.Token);
			
			return 0;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Fatal error in TUI application");
			return 1;
		}
	}
}
