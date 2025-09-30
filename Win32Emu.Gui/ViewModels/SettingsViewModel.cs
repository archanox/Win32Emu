using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Win32Emu.Gui.Models;

namespace Win32Emu.Gui.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _renderingBackend = "Software";

    [ObservableProperty]
    private int _resolutionScaleFactor = 1;

    [ObservableProperty]
    private int _reservedMemoryMB = 256;

    [ObservableProperty]
    private string _windowsVersion = "Windows 95";

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
}
