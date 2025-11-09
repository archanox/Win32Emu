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

    [ObservableProperty]
    private string _compatibilityRating = "Unknown";

    [ObservableProperty]
    private string _unimplementedList = string.Empty;

    [ObservableProperty]
    private string _partiallyImplementedList = string.Empty;

    private readonly IGameDbService? _gameDbService;
    private readonly ConfigurationService? _configService;
    private readonly GameRegistryService _gameRegistryService;
    private readonly VirtualDiskService _virtualDiskService;
    private readonly ILogger _logger;
    private Action<Game>? _onGameUpdated;
    private Func<string, Task>? _clipboardSetter;

    public GameInfoViewModel(Game game, IGameDbService? gameDbService = null, ConfigurationService? configService = null, ILogger? logger = null)
    {
        _game = game;
        _gameDbService = gameDbService;
        _configService = configService;
        _gameRegistryService = new GameRegistryService(logger);
        _logger = logger ?? NullLogger.Instance;
        _editableTitle = game.Title;

        // Initialize VirtualDiskService to get or create virtual disk
        var emulatorConfig = configService?.GetEmulatorConfiguration() ?? new EmulatorConfiguration();
        _virtualDiskService = new VirtualDiskService(emulatorConfig, logger);

        LoadGameInfo();
    }

    /// <summary>
    /// Set a callback to be invoked when the game is updated
    /// </summary>
    public void SetGameUpdatedCallback(Action<Game> callback)
    {
        _onGameUpdated = callback;
    }

    /// <summary>
    /// Set a clipboard setter function for copying text to clipboard
    /// </summary>
    public void SetClipboardSetter(Func<string, Task> clipboardSetter)
    {
        _clipboardSetter = clipboardSetter;
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
            
            // Load environment variables from game registry (if virtual disk exists)
            var virtualDiskPath = GetVirtualDiskPath();
            if (!string.IsNullOrEmpty(virtualDiskPath) && File.Exists(virtualDiskPath))
            {
                try
                {
                    var envVars = _gameRegistryService.GetEnvironmentVariables(virtualDiskPath);
                    if (envVars.Count > 0)
                    {
                        EnvironmentVariables = string.Join(Environment.NewLine, 
                            envVars.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load environment variables from virtual disk");
                }
            }
            
            // Load program arguments from game settings (still using JSON for non-env settings)
            if (_configService != null)
            {
                var gameSettings = _configService.GetGameSettings(Game.ExecutablePath);
                if (gameSettings != null)
                {
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
                // Check implementation status
                var status = GetImplementationStatus(import.DllName, import.FunctionName);
                
                Imports.Add(new ImportInfo
                {
                    DllName = import.DllName,
                    FunctionName = import.FunctionName,
                    Status = status
                });
            }
        }
        
        // Calculate compatibility rating after loading imports
        CalculateCompatibilityRating();
        
        // Generate lists for clipboard
        GenerateClipboardLists();
    }

    private static ImplementationStatus GetImplementationStatus(string dllName, string functionName)
    {
        // Check if the import is implemented
        bool isImplemented = DllModuleExportInfo.IsExportImplemented(dllName, functionName);
        
        if (!isImplemented)
        {
            return ImplementationStatus.NotImplemented;
        }
        
        // Check if it's a stub (partial implementation)
        bool isStub = DllModuleExportInfo.IsExportStub(dllName, functionName);
        
        return isStub ? ImplementationStatus.Partial : ImplementationStatus.Implemented;
    }

    private void CalculateCompatibilityRating()
    {
        if (Imports.Count == 0)
        {
            CompatibilityRating = "Unknown";
            return;
        }

        var totalImports = Imports.Count;
        var implementedCount = Imports.Count(i => i.Status == ImplementationStatus.Implemented);
        var partialCount = Imports.Count(i => i.Status == ImplementationStatus.Partial);
        
        // Calculate a weighted score: Implemented = 1.0, Partial = 0.5, Not Implemented = 0.0
        var score = (implementedCount + (partialCount * 0.5)) / totalImports;
        var percentage = (int)(score * 100);
        
        // Determine rating based on percentage
        string rating;
        if (percentage >= 90)
            rating = "Excellent";
        else if (percentage >= 75)
            rating = "Good";
        else if (percentage >= 50)
            rating = "Fair";
        else if (percentage >= 25)
            rating = "Poor";
        else
            rating = "Very Poor";
        
        CompatibilityRating = $"{rating} ({percentage}%)";
    }

    private void GenerateClipboardLists()
    {
        var unimplemented = Imports
            .Where(i => i.Status == ImplementationStatus.NotImplemented)
            .GroupBy(i => i.DllName)
            .OrderBy(g => g.Key);
        
        var partial = Imports
            .Where(i => i.Status == ImplementationStatus.Partial)
            .GroupBy(i => i.DllName)
            .OrderBy(g => g.Key);
        
        // Generate unimplemented list
        var unimplementedSb = new StringBuilder();
        unimplementedSb.AppendLine("## Unimplemented Functions");
        unimplementedSb.AppendLine();
        foreach (var dllGroup in unimplemented)
        {
            unimplementedSb.AppendLine($"### {dllGroup.Key}");
            foreach (var import in dllGroup.OrderBy(i => i.FunctionName))
            {
                unimplementedSb.AppendLine($"- [ ] {import.FunctionName}");
            }
            unimplementedSb.AppendLine();
        }
        UnimplementedList = unimplementedSb.ToString();
        
        // Generate partially implemented list
        var partialSb = new StringBuilder();
        partialSb.AppendLine("## Partially Implemented Functions (Stubs)");
        partialSb.AppendLine();
        foreach (var dllGroup in partial)
        {
            partialSb.AppendLine($"### {dllGroup.Key}");
            foreach (var import in dllGroup.OrderBy(i => i.FunctionName))
            {
                partialSb.AppendLine($"- [ ] {import.FunctionName}");
            }
            partialSb.AppendLine();
        }
        PartiallyImplementedList = partialSb.ToString();
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
        
        // Save environment variables to virtual disk registry
        var virtualDiskPath = GetVirtualDiskPath();
        if (!string.IsNullOrEmpty(virtualDiskPath))
        {
            try
            {
                _gameRegistryService.SetEnvironmentVariables(virtualDiskPath, envVars);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save environment variables to virtual disk");
            }
        }
        
        // Save program arguments to GameSettings (keeping other settings in JSON)
        if (_configService != null && !string.IsNullOrEmpty(Game.ExecutablePath))
        {
            var gameSettings = _configService.GetGameSettings(Game.ExecutablePath) ?? new GameSettings();
            
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

    [RelayCommand]
    private async Task CopyUnimplementedToClipboard()
    {
        if (_clipboardSetter == null || string.IsNullOrEmpty(UnimplementedList))
        {
            return;
        }

        try
        {
            await _clipboardSetter(UnimplementedList);
            _logger.LogInformation("Copied unimplemented functions list to clipboard");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying unimplemented list to clipboard");
        }
    }

    [RelayCommand]
    private async Task CopyPartiallyImplementedToClipboard()
    {
        if (_clipboardSetter == null || string.IsNullOrEmpty(PartiallyImplementedList))
        {
            if (_clipboardSetter == null)
                _logger.LogWarning("Clipboard setter is unavailable. Cannot copy partially implemented list to clipboard.");
            else
                _logger.LogWarning("PartiallyImplementedList is empty. Nothing to copy to clipboard.");
            return;
        }

        try
        {
            await _clipboardSetter(PartiallyImplementedList);
            _logger.LogInformation("Copied partially implemented functions list to clipboard");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying partially implemented list to clipboard");
        }
    }

    [RelayCommand]
    private void OpenRegistryEditor()
    {
        var virtualDiskPath = GetVirtualDiskPath();
        if (string.IsNullOrEmpty(virtualDiskPath))
        {
            _logger.LogWarning("Cannot open registry editor: no virtual disk path available");
            return;
        }

        try
        {
            // Open the registry editor for this game's virtual disk
            var hive = _gameRegistryService.GetOrCreateGameRegistry(virtualDiskPath);
            var registryWindow = new Views.RegistryViewerWindow
            {
                DataContext = new RegistryViewerViewModel(hive, _logger)
            };
            registryWindow.Show();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open registry editor");
        }
    }

    /// <summary>
    /// Gets the virtual disk path for the current game.
    /// Returns the explicit path from Game.VirtualDiskPath, or gets/creates it from VirtualDiskService.
    /// </summary>
    private string? GetVirtualDiskPath()
    {
        // If game has explicit virtual disk path, use it
        if (!string.IsNullOrEmpty(Game.VirtualDiskPath))
        {
            return Game.VirtualDiskPath;
        }

        // Otherwise, try to get or create via VirtualDiskService
        if (_configService != null)
        {
            try
            {
                var gameSettings = _configService.GetGameSettings(Game.ExecutablePath);
                var virtualDiskPath = _virtualDiskService.GetOrCreateVirtualDisk(Game, gameSettings);
                
                // Update the game model with the disk path
                Game.VirtualDiskPath = virtualDiskPath;
                
                return virtualDiskPath;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get or create virtual disk for game");
                return null;
            }
        }

        return null;
    }
}

/// <summary>
/// Implementation status of an imported function
/// </summary>
public enum ImplementationStatus
{
    NotImplemented,
    Partial,
    Implemented
}

/// <summary>
/// Information about an imported function
/// </summary>
public class ImportInfo
{
    public string DllName { get; set; } = string.Empty;
    public string FunctionName { get; set; } = string.Empty;
    public ImplementationStatus Status { get; set; }
}
