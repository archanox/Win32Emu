using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Win32Emu.Gui.Configuration;
using Win32Emu.Gui.Models;
using Win32Emu.Cpu.Jit;

namespace Win32Emu.Gui.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly EmulatorConfiguration _configuration;
    private readonly ConfigurationService _configService;

    [ObservableProperty]
    private string _renderingBackend;

    [ObservableProperty]
    private string _inputBackend;

    [ObservableProperty]
    private string _cpuBackend;

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
    
    [ObservableProperty]
    private string _cacheStatusMessage = string.Empty;
    
    // Virtual Disk Settings
    [ObservableProperty]
    private bool _useVirtualDiskByDefault;
    
    [ObservableProperty]
    private int _defaultVirtualDiskSizeMb;
    
    [ObservableProperty]
    private string _virtualDiskFormat;
    
    [ObservableProperty]
    private string? _virtualDisksDirectory;

    public ObservableCollection<string> RenderingBackends { get; } = new()
    {
        "SDL",
        "GLFW",
        "Vulkan",
        "Metal",
        "Software"
    };

    public ObservableCollection<string> InputBackends { get; } = new()
    {
        "SDL",
        "GLFW"
    };

    public ObservableCollection<string> CpuBackends { get; } = new()
    {
        "IcedCPU",
        "JitCPU",
        "Unicorn"
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
    
    public ObservableCollection<string> VirtualDiskFormats { get; } = new()
    {
        "VHD",
        "VHDX"
    };

    public SettingsViewModel(EmulatorConfiguration configuration, ConfigurationService configService)
    {
        _configuration = configuration;
        _configService = configService;
        
        // Initialize properties from configuration
        _renderingBackend = configuration.RenderingBackend;
        _inputBackend = configuration.InputBackend;
        _cpuBackend = configuration.CpuBackend;
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
        _useVirtualDiskByDefault = configuration.UseVirtualDiskByDefault;
        _defaultVirtualDiskSizeMb = configuration.DefaultVirtualDiskSizeMb;
        _virtualDiskFormat = configuration.VirtualDiskFormat;
        _virtualDisksDirectory = configuration.VirtualDisksDirectory;
    }

    partial void OnRenderingBackendChanged(string value)
    {
        _configuration.RenderingBackend = value;
        _configService.SaveEmulatorConfiguration(_configuration);
    }

    partial void OnInputBackendChanged(string value)
    {
        _configuration.InputBackend = value;
        _configService.SaveEmulatorConfiguration(_configuration);
    }

    partial void OnCpuBackendChanged(string value)
    {
        _configuration.CpuBackend = value;
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
    
    partial void OnUseVirtualDiskByDefaultChanged(bool value)
    {
        _configuration.UseVirtualDiskByDefault = value;
        _configService.SaveEmulatorConfiguration(_configuration);
    }
    
    partial void OnDefaultVirtualDiskSizeMbChanged(int value)
    {
        _configuration.DefaultVirtualDiskSizeMb = value;
        _configService.SaveEmulatorConfiguration(_configuration);
    }
    
    partial void OnVirtualDiskFormatChanged(string value)
    {
        _configuration.VirtualDiskFormat = value;
        _configService.SaveEmulatorConfiguration(_configuration);
    }
    
    partial void OnVirtualDisksDirectoryChanged(string? value)
    {
        _configuration.VirtualDisksDirectory = value;
        _configService.SaveEmulatorConfiguration(_configuration);
    }
    
    /// <summary>
    /// Command to purge the JIT cache
    /// </summary>
    [RelayCommand]
    private void PurgeJitCache()
    {
        // Purge the JIT cache using the static method to avoid unnecessary instantiation
        // This will delete all cache files from disk, regardless of which
        // JitCache instance created them
        var success = JitCache.PurgeCache(JitCache.DefaultCacheDirectory);
        
        if (success)
        {
            CacheStatusMessage = "✓ JIT cache purged successfully";
        }
        else
        {
            CacheStatusMessage = "✗ Failed to purge JIT cache - check logs for details";
        }
        
        // Clear the message after 5 seconds
        Task.Delay(5000).ContinueWith(_ => CacheStatusMessage = string.Empty);
    }
}
