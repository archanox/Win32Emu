using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Gui.Configuration;
using Win32Emu.Gui.Models;
using Win32Emu.Gui.Services;
using Win32Emu.Win32;

namespace Win32Emu.Gui.ViewModels;

public partial class GameInfoViewModel : ViewModelBase
{
    [ObservableProperty]
    private Game _game;

    [ObservableProperty]
    private GameDbEntry? _gameDbEntry;

    [ObservableProperty]
    private PeMetadata? _peMetadata;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _fileSize = string.Empty;

    [ObservableProperty]
    private string _dateTimeCompiled = string.Empty;

    [ObservableProperty]
    private string _machineType = string.Empty;

    [ObservableProperty]
    private string _minimumOs = string.Empty;

    [ObservableProperty]
    private string _minimumOsVersion = string.Empty;

    [ObservableProperty]
    private string _virusTotalUrl = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ImportInfo> _imports = new();

    [ObservableProperty]
    private string _editableTitle;

    [ObservableProperty]
    private string _environmentVariables = string.Empty;

    [ObservableProperty]
    private string _programArguments = string.Empty;

    [ObservableProperty]
    private string _gameDbStubJson = string.Empty;

    private readonly IGameDbService? _gameDbService;
    private readonly ConfigurationService? _configService;
    private readonly ILogger _logger;
    private Action<Game>? _onGameUpdated;

    public GameInfoViewModel(Game game, IGameDbService? gameDbService = null, ConfigurationService? configService = null, ILogger? logger = null)
    {
        _game = game;
        _gameDbService = gameDbService;
        _configService = configService;
        _logger = logger ?? NullLogger.Instance;
        _editableTitle = game.Title;

        LoadGameInfo();
    }

    /// <summary>
    /// Set a callback to be invoked when the game is updated
    /// </summary>
    public void SetGameUpdatedCallback(Action<Game> callback)
    {
        _onGameUpdated = callback;
    }

    private void LoadGameInfo()
    {
        // Load PE metadata
        if (!string.IsNullOrEmpty(Game.ExecutablePath) && File.Exists(Game.ExecutablePath))
        {
            PeMetadata = PeMetadataService.GetMetadata(Game.ExecutablePath);
            
            if (PeMetadata != null)
            {
                FileName = PeMetadata.FileName;
                FileSize = FormatFileSize(PeMetadata.FileSize);
                DateTimeCompiled = PeMetadata.DateTimeCompiled?.ToString("yyyy-MM-dd HH:mm:ss UTC") ?? "Unknown";
                MachineType = PeMetadata.MachineType;
                MinimumOs = PeMetadata.MinimumOs;
                MinimumOsVersion = PeMetadata.MinimumOsVersion;

                // Load imports with implementation status
                LoadImportsWithStatus(PeMetadata.Imports);
            }

            // Load GameDB entry
            if (_gameDbService != null)
            {
                GameDbEntry = _gameDbService.FindGameByExecutable(Game.ExecutablePath);
            }

            // Generate VirusTotal URL
            var hashes = HashUtility.ComputeAllHashes(Game.ExecutablePath);
            VirusTotalUrl = $"https://www.virustotal.com/gui/file/{hashes.Sha256}";
            
            // Load game settings (environment variables and program arguments)
            if (_configService != null)
            {
                var gameSettings = _configService.GetGameSettings(Game.ExecutablePath);
                if (gameSettings != null)
                {
                    // Load environment variables
                    if (gameSettings.EnvironmentVariables != null && gameSettings.EnvironmentVariables.Count > 0)
                    {
                        EnvironmentVariables = string.Join(Environment.NewLine, 
                            gameSettings.EnvironmentVariables.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                    }
                    
                    // Load program arguments
                    ProgramArguments = gameSettings.ProgramArguments ?? string.Empty;
                }
            }
        }
    }

    private void LoadImportsWithStatus(List<PeImport> peImports)
    {
        Imports.Clear();

        // Group imports by DLL
        var importsByDll = peImports.GroupBy(i => i.DllName.ToUpperInvariant());

        foreach (var dllGroup in importsByDll)
        {
            foreach (var import in dllGroup)
            {
                // Check if the import is implemented
                // This is a simplified check - you may want to enhance this
                var isImplemented = CheckIfImplemented(import.DllName, import.FunctionName);
                
                Imports.Add(new ImportInfo
                {
                    DllName = import.DllName,
                    FunctionName = import.FunctionName,
                    IsImplemented = isImplemented
                });
            }
        }
    }

    private static bool CheckIfImplemented(string dllName, string functionName)
    {
        // Use DllModuleExportInfo to check if the export is actually implemented
        return DllModuleExportInfo.IsExportImplemented(dllName, functionName);
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    [RelayCommand]
    private void SaveChanges()
    {
        // Update the game with edited values
        Game.Title = EditableTitle;
        
        // Save environment variables and program arguments to GameSettings
        if (_configService != null && !string.IsNullOrEmpty(Game.ExecutablePath))
        {
            var gameSettings = _configService.GetGameSettings(Game.ExecutablePath) ?? new GameSettings();
            
            // Parse environment variables from the text box
            var envVars = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(EnvironmentVariables))
            {
                var lines = EnvironmentVariables.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var line in lines)
                {
                    var parts = line.Split('=', 2);
                    if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[0]))
                    {
                        envVars[parts[0].Trim()] = parts[1].Trim();
                    }
                }
            }
            
            gameSettings.EnvironmentVariables = envVars.Count > 0 ? envVars : null;
            gameSettings.ProgramArguments = string.IsNullOrWhiteSpace(ProgramArguments) ? null : ProgramArguments;
            
            _configService.SaveGameSettings(Game.ExecutablePath, gameSettings);
        }
        
        // Notify that the game was updated
        _onGameUpdated?.Invoke(Game);
    }

    [RelayCommand]
    private void CopyGameDbStub()
    {
        if (PeMetadata == null || string.IsNullOrEmpty(Game.ExecutablePath))
        {
            return;
        }

        try
        {
            var hashes = HashUtility.ComputeAllHashes(Game.ExecutablePath);
            
            // Create a stub GameDbEntry
            var stub = new GameDbEntry
            {
                Id = Guid.NewGuid(),
                Title = EditableTitle,
                Description = Game.Description ?? "",
                Executables = new List<GameExecutable>
                {
                    new GameExecutable
                    {
                        Name = PeMetadata.FileName,
                        Md5 = hashes.Md5,
                        Sha1 = hashes.Sha1,
                        Sha256 = hashes.Sha256
                    }
                }
            };

            // Serialize to JSON with formatting
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            GameDbStubJson = JsonSerializer.Serialize(stub, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating GameDB stub for game: {GameTitle}", Game.Title);
        }
    }

    [RelayCommand]
    private void OpenVirusTotal()
    {
        if (!string.IsNullOrEmpty(VirusTotalUrl))
        {
            try
            {
                // Open URL in default browser
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = VirusTotalUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening VirusTotal URL: {Url}", VirusTotalUrl);
            }
        }
    }
}

/// <summary>
/// Information about an imported function
/// </summary>
public class ImportInfo
{
    public string DllName { get; set; } = string.Empty;
    public string FunctionName { get; set; } = string.Empty;
    public bool IsImplemented { get; set; }
}
