using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Platform.Storage;
using Win32Emu.Gui.Models;

namespace Win32Emu.Gui.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentPage;

    public GameLibraryViewModel GameLibraryViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }
    public ControllerMappingViewModel ControllerMappingViewModel { get; }
    public EmulatorConfiguration Configuration { get; } = new();

    public MainWindowViewModel()
    {
        GameLibraryViewModel = new GameLibraryViewModel(Configuration);
        SettingsViewModel = new SettingsViewModel(Configuration);
        ControllerMappingViewModel = new ControllerMappingViewModel();
        _currentPage = GameLibraryViewModel;
    }

    public void SetStorageProvider(IStorageProvider storageProvider)
    {
        GameLibraryViewModel.SetStorageProvider(storageProvider);
    }

    [RelayCommand]
    private void NavigateToLibrary()
    {
        CurrentPage = GameLibraryViewModel;
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        CurrentPage = SettingsViewModel;
    }

    [RelayCommand]
    private void NavigateToControllers()
    {
        CurrentPage = ControllerMappingViewModel;
    }
}
