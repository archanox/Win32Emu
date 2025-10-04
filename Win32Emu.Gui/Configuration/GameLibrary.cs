using Win32Emu.Gui.Models;

namespace Win32Emu.Gui.Configuration
{
	/// <summary>
	/// Game library - machine-specific game library and watched folders
	/// </summary>
	public class GameLibrary
	{
		public List<Game> Games { get; set; } = new();
		public List<string> WatchedFolders { get; set; } = new();
	}
}
