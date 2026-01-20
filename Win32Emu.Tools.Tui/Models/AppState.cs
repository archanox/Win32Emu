using Microsoft.Extensions.Logging;
using Win32Emu.Tools.Tui.Services;

namespace Win32Emu.Tools.Tui.Models;

/// <summary>
/// Application state for the TUI
/// </summary>
public class AppState
{
	public GameLibraryService GameLibrary { get; }
	public ConfigurationService Configuration { get; }
	public ILogger Logger { get; }
	
	// Current view state
	public ViewMode CurrentView { get; set; } = ViewMode.MainMenu;
	public int SelectedGameIndex { get; set; } = 0;
	public int SelectedMenuIndex { get; set; } = 0;
	
	// Debug state
	public bool IsInDebugMode { get; set; } = false;
	public string? CurrentExecutablePath { get; set; }

	public AppState(GameLibraryService gameLibrary, ConfigurationService configuration, ILogger logger)
	{
		GameLibrary = gameLibrary;
		Configuration = configuration;
		Logger = logger;
	}
}

/// <summary>
/// Different views/screens in the TUI
/// </summary>
public enum ViewMode
{
	MainMenu,
	GameLibrary,
	GameDetails,
	Settings,
	AddGame,
	Help,
	Debugger
}
