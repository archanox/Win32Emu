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

    [ObservableProperty]
    private bool _enableGdbServer;

    [ObservableProperty]
    private int _gdbServerPort;

    [ObservableProperty]
    private bool _gdbPauseOnStart;

    [ObservableProperty]
    private bool _enableInstructionAnalyzer;

    [ObservableProperty]
    private bool _enableLegacyInstructionDecoding;
    
    [ObservableProperty]
    private bool _enableOpenTelemetry;
    
    [ObservableProperty]
    private bool _useConsoleExporter;
    
    [ObservableProperty]
    private bool _useOtlpExporter;
    
    [ObservableProperty]
    private string _otlpEndpoint;

    public ObservableCollection<string> RenderingBackends { get; } = new()
    {
        "SDL",
        "GLFW",
        "Vulkan",
        "Metal"
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
        _enableGdbServer = configuration.EnableGdbServer;
        _gdbServerPort = configuration.GdbServerPort;
        _gdbPauseOnStart = configuration.GdbPauseOnStart;
        _enableInstructionAnalyzer = configuration.EnableInstructionAnalyzer;
        _enableLegacyInstructionDecoding = configuration.EnableLegacyInstructionDecoding;
        _enableOpenTelemetry = configuration.EnableOpenTelemetry;
        _useConsoleExporter = configuration.UseConsoleExporter;
        _useOtlpExporter = configuration.UseOtlpExporter;
        _otlpEndpoint = configuration.OtlpEndpoint;
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

    partial void OnEnableGdbServerChanged(bool value)
    {
        _configuration.EnableGdbServer = value;
        _configService.SaveEmulatorConfiguration(_configuration);
    }

    partial void OnGdbServerPortChanged(int value)
    {
        _configuration.GdbServerPort = value;
        _configService.SaveEmulatorConfiguration(_configuration);
    }

    partial void OnGdbPauseOnStartChanged(bool value)
    {
        _configuration.GdbPauseOnStart = value;
        _configService.SaveEmulatorConfiguration(_configuration);
    }

    partial void OnEnableInstructionAnalyzerChanged(bool value)
    {
        _configuration.EnableInstructionAnalyzer = value;
        _configService.SaveEmulatorConfiguration(_configuration);
    }

    partial void OnEnableLegacyInstructionDecodingChanged(bool value)
    {
        _configuration.EnableLegacyInstructionDecoding = value;
        _configService.SaveEmulatorConfiguration(_configuration);
    }
    
    partial void OnEnableOpenTelemetryChanged(bool value)
    {
        _configuration.EnableOpenTelemetry = value;
        _configService.SaveEmulatorConfiguration(_configuration);
    }
    
    partial void OnUseConsoleExporterChanged(bool value)
    {
        _configuration.UseConsoleExporter = value;
        _configService.SaveEmulatorConfiguration(_configuration);
    }
    
    partial void OnUseOtlpExporterChanged(bool value)
    {
        _configuration.UseOtlpExporter = value;
        _configService.SaveEmulatorConfiguration(_configuration);
    }
    
    partial void OnOtlpEndpointChanged(string value)
    {
        _configuration.OtlpEndpoint = value;
        _configService.SaveEmulatorConfiguration(_configuration);
    }
}
