using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Win32Emu.Gui.Models;

namespace Win32Emu.Gui.ViewModels;

public partial class GameLibraryViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<Game> _games = [];

    [ObservableProperty]
    private Game? _selectedGame;

    public GameLibraryViewModel()
    {
        // Add some sample games for demonstration
        Games.Add(new Game
        {
            Title = "Ignition",
            ExecutablePath = "ignition.exe",
            Description = "Classic racing game from 1997",
            TimesPlayed = 5
        });
    }

    [RelayCommand]
    private async Task AddGame()
    {
        // This will be implemented with a file picker
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task LaunchGame(Game? game)
    {
        if (game == null) return;
        
        // Launch the emulator with the selected game
        await Task.Run(() =>
        {
            // This will launch the Win32Emu with the game executable
            // For now, just a placeholder
        });
    }

    [RelayCommand]
    private void RemoveGame(Game? game)
    {
        if (game != null)
        {
            Games.Remove(game);
        }
    }
}
