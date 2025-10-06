using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Win32Emu.Gui.Models;
using Win32Emu.Gui.Services;

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
    private Action<Game>? _onGameUpdated;

    public GameInfoViewModel(Game game, IGameDbService? gameDbService = null)
    {
        _game = game;
        _gameDbService = gameDbService;
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
        // Common DLLs and their typical implementation status
        // This is a heuristic - for a real implementation, you'd query the emulator's module registry
        var commonImplementedDlls = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "KERNEL32.DLL",
            "USER32.DLL",
            "GDI32.DLL",
            "ADVAPI32.DLL"
        };

        // For now, mark common DLL imports as potentially implemented
        // In a real implementation, you'd check against the actual emulator exports
        return commonImplementedDlls.Contains(dllName);
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
            Console.WriteLine($"Error creating GameDB stub: {ex.Message}");
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
                Console.WriteLine($"Error opening VirusTotal URL: {ex.Message}");
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
