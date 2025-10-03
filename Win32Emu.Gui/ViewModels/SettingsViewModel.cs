using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Win32Emu.Gui.Configuration;
using Win32Emu.Gui.Models;

namespace Win32Emu.Gui.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly EmulatorConfiguration _configuration;
    private readonly ConfigurationService _configService;

    [ObservableProperty]
    private string _renderingBackend;

    [ObservableProperty]
    private int _resolutionScaleFactor;

    [ObservableProperty]
    private int _reservedMemoryMb;

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

    public SettingsViewModel(EmulatorConfiguration configuration, ConfigurationService configService)
    {
        _configuration = configuration;
        _configService = configService;
        
        // Initialize properties from configuration
        _renderingBackend = configuration.RenderingBackend;
        _resolutionScaleFactor = configuration.ResolutionScaleFactor;
        _reservedMemoryMb = configuration.ReservedMemoryMb;
        _windowsVersion = configuration.WindowsVersion;
        _enableDebugMode = configuration.EnableDebugMode;
    }

    partial void OnRenderingBackendChanged(string value)
    {
        _configuration.RenderingBackend = value;
        _configService.SaveEmulatorConfiguration(_configuration);
    }

    partial void OnResolutionScaleFactorChanged(int value)
    {
        _configuration.ResolutionScaleFactor = value;
        _configService.SaveEmulatorConfiguration(_configuration);
    }

    partial void OnReservedMemoryMbChanged(int value)
    {
        _configuration.ReservedMemoryMb = value;
        _configService.SaveEmulatorConfiguration(_configuration);
    }

    partial void OnWindowsVersionChanged(string value)
    {
        _configuration.WindowsVersion = value;
        _configService.SaveEmulatorConfiguration(_configuration);
    }

    partial void OnEnableDebugModeChanged(bool value)
    {
        _configuration.EnableDebugMode = value;
        _configService.SaveEmulatorConfiguration(_configuration);
    }
}
