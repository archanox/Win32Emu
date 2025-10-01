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

    [ObservableProperty]
    private ObservableCollection<string> _watchedFolders = [];

    private IStorageProvider? _storageProvider;
    private readonly EmulatorConfiguration _configuration;

    public GameLibraryViewModel(EmulatorConfiguration configuration)
    {
        _configuration = configuration;
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
    private async Task AddFolder()
    {
        if (_storageProvider == null) return;

        var folders = await _storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Folder to Scan for Games",
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            var folder = folders[0];
            var folderPath = folder.Path.LocalPath;

            // Add to watched folders if not already added
            if (!WatchedFolders.Contains(folderPath))
            {
                WatchedFolders.Add(folderPath);
            }

            // Scan for exe files
            await ScanFolderForGames(folderPath);
        }
    }

    private async Task ScanFolderForGames(string folderPath)
    {
        await Task.Run(() =>
        {
            try
            {
                var exeFiles = Directory.GetFiles(folderPath, "*.exe", SearchOption.TopDirectoryOnly);
                
                foreach (var exeFile in exeFiles)
                {
                    // Check if game already exists
                    if (!Games.Any(g => g.ExecutablePath.Equals(exeFile, StringComparison.OrdinalIgnoreCase)))
                    {
                        var fileName = Path.GetFileNameWithoutExtension(exeFile);
                        Games.Add(new Game
                        {
                            Title = fileName,
                            ExecutablePath = exeFile,
                            Description = $"Found in {Path.GetFileName(folderPath)}",
                            TimesPlayed = 0
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning folder: {ex.Message}");
            }
        });
    }

    [RelayCommand]
    private async Task LaunchGame(Game? game)
    {
        if (game == null) return;
        
        try
        {
            // TODO: For now, use process-based launching
            // In future updates, this will use in-process API with EmulatorWindow
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
