using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Win32Emu.Gui.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentPage;

    public GameLibraryViewModel GameLibraryViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }

    public MainWindowViewModel()
    {
        GameLibraryViewModel = new GameLibraryViewModel();
        SettingsViewModel = new SettingsViewModel();
        _currentPage = GameLibraryViewModel;
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
}
