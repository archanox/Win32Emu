using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Win32Emu.Gui.Models;

namespace Win32Emu.Gui.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly EmulatorConfiguration _configuration;

    [ObservableProperty]
    private string _renderingBackend;

    [ObservableProperty]
    private int _resolutionScaleFactor;

    [ObservableProperty]
    private int _reservedMemoryMB;

    [ObservableProperty]
    private string _windowsVersion;

    [ObservableProperty]
    private bool _enableDebugMode;

    public ObservableCollection<string> RenderingBackends { get; } = new()
    {
        "Software",
        "DirectDraw",
        "Glide"
    };

    public ObservableCollection<string> WindowsVersions { get; } = new()
    {
        "Windows 95",
        "Windows 98",
        "Windows ME",
        "Windows NT 4.0",
        "Windows 2000",
        "Windows XP"
    };

    public ObservableCollection<int> ScaleFactors { get; } = new()
    {
        1, 2, 3, 4
    };

    public SettingsViewModel(EmulatorConfiguration configuration)
    {
        _configuration = configuration;
        
        // Initialize properties from configuration
        _renderingBackend = configuration.RenderingBackend;
        _resolutionScaleFactor = configuration.ResolutionScaleFactor;
        _reservedMemoryMB = configuration.ReservedMemoryMB;
        _windowsVersion = configuration.WindowsVersion;
        _enableDebugMode = configuration.EnableDebugMode;
    }

    partial void OnRenderingBackendChanged(string value)
    {
        _configuration.RenderingBackend = value;
    }

    partial void OnResolutionScaleFactorChanged(int value)
    {
        _configuration.ResolutionScaleFactor = value;
    }

    partial void OnReservedMemoryMBChanged(int value)
    {
        _configuration.ReservedMemoryMB = value;
    }

    partial void OnWindowsVersionChanged(string value)
    {
        _configuration.WindowsVersion = value;
    }

    partial void OnEnableDebugModeChanged(bool value)
    {
        _configuration.EnableDebugMode = value;
    }
}
