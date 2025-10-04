using System.Collections.ObjectModel;

namespace Win32Emu.Gui.Models
{
	public class GameLibrary
	{
		public ObservableCollection<Game> Games { get; } = [];
		public EmulatorConfiguration Configuration { get; } = new();
	}
}
