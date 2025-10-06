using System.Collections.ObjectModel;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
    private readonly IGameDbService? _gameDbService;
    private readonly ILogger _logger;

    public GameLibraryViewModel(EmulatorConfiguration configuration, ConfigurationService configService, IGameDbService? gameDbService = null, ILogger? logger = null)
    {
        _configuration = configuration;
        _configService = configService;
        _gameDbService = gameDbService;
        _logger = logger ?? NullLogger.Instance;
        
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

    /// <summary>
    /// Enrich a game with metadata from the GameDB if available, and extract icon if needed
    /// </summary>
    private void EnrichGameFromDb(Game game)
    {
        if (string.IsNullOrEmpty(game.ExecutablePath))
        {
            return;
        }

        try
        {
            // Try to enrich from GameDB first
            var hasLogo = false;
            if (_gameDbService != null)
            {
                var dbEntry = _gameDbService.FindGameByExecutable(game.ExecutablePath);
                if (dbEntry != null)
                {
                    // Update game with metadata from database
                    game.GameDbId = dbEntry.Id;
                    game.Title = dbEntry.Title;
                    game.Description = dbEntry.Description ?? game.Description;
                    
                    // If there's a logo URL, we could download it and set ThumbnailPath
                    // For now, just use the URL as a reference
                    if (!string.IsNullOrEmpty(dbEntry.LogoUrl))
                    {
                        // Future: Download logo and set ThumbnailPath
                        // For now, we'll leave ThumbnailPath as is
                        hasLogo = true;
                    }
                }
            }

            // If no logo from GameDB, try to extract icon from PE
            if (!hasLogo && string.IsNullOrEmpty(game.ThumbnailPath))
            {
                ExtractGameIcon(game);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enrich game from DB for executable path: {ExecutablePath}", game.ExecutablePath);
            // If enrichment fails, just continue with the basic game info
        }
    }

    /// <summary>
    /// Extract icon from PE executable and save it for the game
    /// </summary>
    private void ExtractGameIcon(Game game)
    {
        if (string.IsNullOrEmpty(game.ExecutablePath) || !File.Exists(game.ExecutablePath))
        {
            return;
        }

        try
        {
            // Create icons directory in AppData
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var iconsDirectory = Path.Combine(appDataDir, "Win32Emu", "GameIcons");
            
            if (!Directory.Exists(iconsDirectory))
            {
                Directory.CreateDirectory(iconsDirectory);
            }

            // Generate icon filename based on executable hash
            var exeFileName = Path.GetFileNameWithoutExtension(game.ExecutablePath);
            var iconFileName = $"{exeFileName}_{GetFileHash(game.ExecutablePath)}.ico";
            var iconPath = Path.Combine(iconsDirectory, iconFileName);

            // Check if icon already exists
            if (File.Exists(iconPath))
            {
                game.ThumbnailPath = iconPath;
                return;
            }

            // Try to extract icon from PE
            if (PeIconExtractor.TryExtractIcon(game.ExecutablePath, iconPath))
            {
                game.ThumbnailPath = iconPath;
                _logger.LogInformation("Extracted icon for {GameTitle} to {IconPath}", game.Title, iconPath);
            }
            else
            {
                // Fall back to default icon
                var defaultIconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "default-game-icon.ico");
                if (File.Exists(defaultIconPath))
                {
                    // Copy default icon to the icons directory
                    var fallbackIconPath = Path.Combine(iconsDirectory, $"{exeFileName}_default.ico");
                    if (!File.Exists(fallbackIconPath))
                    {
                        File.Copy(defaultIconPath, fallbackIconPath);
                    }
                    game.ThumbnailPath = fallbackIconPath;
                    _logger.LogInformation("Using default icon for {GameTitle}", game.Title);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract icon for {GameTitle}", game.Title);
        }
    }

    /// <summary>
    /// Get a simple hash of the file for unique identification
    /// </summary>
    private static string GetFileHash(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            using var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant().Substring(0, 16);
        }
        catch
        {
            // If hashing fails, use file size and name as fallback
            var info = new FileInfo(filePath);
            return $"{info.Length}_{info.Name.GetHashCode():X8}";
        }
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

            var game = new Game
            {
                Title = Path.GetFileNameWithoutExtension(fileName),
                ExecutablePath = filePath,
                Description = "Added from file picker",
                TimesPlayed = 0
            };
            
            // Enrich with GameDB metadata if available
            EnrichGameFromDb(game);
            
            Games.Add(game);
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
                        var game = new Game
                        {
                            Title = fileName,
                            ExecutablePath = exeFile,
                            Description = $"Found in {Path.GetFileName(folderPath)}",
                            TimesPlayed = 0
                        };
                        
                        // Enrich with GameDB metadata if available
                        EnrichGameFromDb(game);
                        
                        Games.Add(game);
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
            
            // Create logger factory with console and Avalonia providers
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddConsole()
                    .AddProvider(new AvaloniaLoggerProvider(viewModel))
                    .SetMinimumLevel(_configuration.EnableDebugMode ? LogLevel.Debug : LogLevel.Information);
            });
            
            var logger = loggerFactory.CreateLogger<Emulator>();
            
            // Launch the game with the view model as the host
            var service = new EmulatorService(_configuration, viewModel, logger);
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

    [RelayCommand]
    private void ShowGameInfo(Game? game)
    {
        if (game == null)
        {
            return;
        }

        var viewModel = new GameInfoViewModel(game, _gameDbService);
        var window = new GameInfoWindow
        {
            DataContext = viewModel
        };
        
        window.Show();
    }
}
