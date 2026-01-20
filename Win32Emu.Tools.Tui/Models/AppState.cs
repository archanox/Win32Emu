using Win32Emu.Tools.Tui.Services;

namespace Win32Emu.Tools.Tui.Models;

public enum ViewMode
{
	MainMenu,
	GameLibrary,
	AddGame,
	Settings,
	GameDetails,
	Help
}

public class AppState
{
	public ViewMode CurrentView { get; set; } = ViewMode.MainMenu;
	public int SelectedIndex { get; set; } = 0;
	public GameEntry? SelectedGame { get; set; }
	public GameLibraryService GameLibrary { get; }
	public ConfigurationService Configuration { get; }
	
	// Form state for adding games
	public GameEntry NewGameEntry { get; set; } = new();
	public int AddGameFieldIndex { get; set; } = 0;

	public AppState(GameLibraryService gameLibrary, ConfigurationService configuration)
	{
		GameLibrary = gameLibrary;
		Configuration = configuration;
	}

	public void ResetNewGameEntry()
	{
		NewGameEntry = new GameEntry();
		AddGameFieldIndex = 0;
	}
}
