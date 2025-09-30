using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Win32Emu.Gui.Models;
using Win32Emu.Gui.Services;
using Avalonia.Platform.Storage;

namespace Win32Emu.Gui.ViewModels;

public partial class GameLibraryViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<Game> _games = [];

    [ObservableProperty]
    private Game? _selectedGame;

    private IStorageProvider? _storageProvider;
    private readonly EmulatorConfiguration _configuration;

    public GameLibraryViewModel(EmulatorConfiguration configuration)
    {
        _configuration = configuration;
        
        // Add some sample games for demonstration
        Games.Add(new Game
        {
            Title = "Ignition",
            ExecutablePath = "ignition.exe",
            Description = "Classic racing game from 1997",
            TimesPlayed = 5
        });
    }

    public void SetStorageProvider(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    [RelayCommand]
    private async Task AddGame()
    {
        if (_storageProvider == null) return;

        var files = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Game Executable",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Windows Executable")
                {
                    Patterns = new[] { "*.exe" }
                }
            }
        });

        if (files.Count > 0)
        {
            var file = files[0];
            var fileName = file.Name;
            var filePath = file.Path.LocalPath;

            Games.Add(new Game
            {
                Title = Path.GetFileNameWithoutExtension(fileName),
                ExecutablePath = filePath,
                Description = "Added from file picker",
                TimesPlayed = 0
            });
        }
    }

    [RelayCommand]
    private async Task LaunchGame(Game? game)
    {
        if (game == null) return;
        
        try
        {
            var service = new EmulatorService(_configuration);
            await service.LaunchGame(game);
            
            // Update play count
            game.TimesPlayed++;
            game.LastPlayed = DateTime.Now;
        }
        catch (Exception ex)
        {
            // In a real app, we would show an error dialog here
            Console.WriteLine($"Error launching game: {ex.Message}");
        }
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
