using CommunityToolkit.Mvvm.ComponentModel;

namespace Win32Emu.Gui.Models;

public partial class Game : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;
    
    [ObservableProperty]
    private string _executablePath = string.Empty;
    
    [ObservableProperty]
    private string? _thumbnailPath;
    
    [ObservableProperty]
    private string? _description;
    
    [ObservableProperty]
    private DateTime? _lastPlayed;
    
    [ObservableProperty]
    private int _timesPlayed;
    
    /// <summary>
    /// Optional reference to game database entry ID
    /// </summary>
    [ObservableProperty]
    private Guid? _gameDbId;
    
    /// <summary>
    /// Path to the game's virtual disk (VHD/VHDX)
    /// </summary>
    [ObservableProperty]
    private string? _virtualDiskPath;
    
    /// <summary>
    /// Path to the executable within the VHD (e.g., "C:\ignition\ign_teas.exe")
    /// </summary>
    [ObservableProperty]
    private string? _vhdExecutablePath;
    
    /// <summary>
    /// Indicates if the game is currently being installed to VHD
    /// </summary>
    [ObservableProperty]
    private bool _isInstalling;
    
    /// <summary>
    /// Current installation progress (0-100)
    /// </summary>
    [ObservableProperty]
    private double _installProgress;
    
    /// <summary>
    /// Status message for the installation
    /// </summary>
    [ObservableProperty]
    private string? _installStatusMessage;
}
