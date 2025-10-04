using System.Collections.ObjectModel;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Win32Emu.Gui.Configuration;
using Win32Emu.Gui.Models;
using Win32Emu.Gui.Services;
using Win32Emu.Gui.Views;
using Win32Emu.Loader;

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
    private readonly ConfigurationService _configService;

    public GameLibraryViewModel(EmulatorConfiguration configuration, ConfigurationService configService)
    {
        _configuration = configuration;
        _configService = configService;
        
        // Load games and watched folders from persistent storage
        LoadFromConfiguration();
    }

    public void SetStorageProvider(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    private void LoadFromConfiguration()
    {
        // Load games from configuration
        var savedGames = _configService.GetGames();
        foreach (var game in savedGames)
        {
            Games.Add(game);
        }

        // Load watched folders from configuration
        var savedFolders = _configService.GetWatchedFolders();
        foreach (var folder in savedFolders)
        {
            WatchedFolders.Add(folder);
        }
    }

    private void SaveToConfiguration()
    {
        _configService.SaveGames(Games);
        _configService.SaveWatchedFolders(WatchedFolders);
    }

    [RelayCommand]
    private async Task AddGame()
    {
        if (_storageProvider == null)
        {
	        return;
        }

        var files = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Game Executable",
            AllowMultiple = true,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Windows Executable")
                {
                    Patterns = new[] { "*.exe" }
                }
            }
        });

        foreach (var file in files)
        {
            var fileName = file.Name;
            var filePath = file.Path.LocalPath;

            // Validate that the file is a valid PE32 executable
            if (!PeImageLoader.IsPE32(filePath))
            {
                Console.WriteLine($"Skipping non-PE32 executable: {filePath}");
                continue;
            }

            Games.Add(new Game
            {
                Title = Path.GetFileNameWithoutExtension(fileName),
                ExecutablePath = filePath,
                Description = "Added from file picker",
                TimesPlayed = 0
            });
        }

        // Save the updated games list
        SaveToConfiguration();
    }

    [RelayCommand]
    private async Task AddFolder()
    {
        if (_storageProvider == null)
        {
	        return;
        }

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

            // Save the updated watched folders and games
            SaveToConfiguration();
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
                    // Validate that the file is a valid PE32 executable
                    if (!PeImageLoader.IsPE32(exeFile))
                    {
                        Console.WriteLine($"Skipping non-PE32 executable: {exeFile}");
                        continue;
                    }

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
        if (game == null)
        {
	        return;
        }

        try
        {
            // Update play count and last played time before launching
            game.TimesPlayed++;
            game.LastPlayed = DateTime.UtcNow;
            
            // Save the updated game stats
            SaveToConfiguration();
            
            // Create the EmulatorWindow with its ViewModel that implements IGuiEmulatorHost
            var emulatorWindow = new EmulatorWindow();
            var viewModel = new EmulatorWindowViewModel();
            emulatorWindow.DataContext = viewModel;
            
            // Set the owner window reference so created windows can use it as parent
            viewModel.SetOwnerWindow(emulatorWindow);
            
            // Show the emulator window
            emulatorWindow.Show();
            
            // Launch the game with the view model as the host
            var service = new EmulatorService(_configuration, viewModel);
            await service.LaunchGame(game);
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
            
            // Save the updated games list
            SaveToConfiguration();
        }
    }
}
