using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Win32Emu.Gui.Configuration;
using Win32Emu.Gui.Models;
using Win32Emu.Gui.Services;

namespace Win32Emu.Gui.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentPage;

    public GameLibraryViewModel GameLibraryViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }
    public ControllerMappingViewModel ControllerMappingViewModel { get; }
    public EmulatorConfiguration Configuration { get; }
    private readonly ConfigurationService _configService;
    private readonly IGameDbService _gameDbService;

    public MainWindowViewModel()
    {
        _configService = new ConfigurationService();
        _gameDbService = new GameDbService();
        Configuration = _configService.GetEmulatorConfiguration();
        GameLibraryViewModel = new GameLibraryViewModel(Configuration, _configService, _gameDbService);
        SettingsViewModel = new SettingsViewModel(Configuration, _configService);
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
