using Win32Emu.Gui.Configuration;
using Win32Emu.Gui.Models;
using Win32Emu.Gui.ViewModels;

namespace Win32Emu.Tests.Gui;

public class GameLibraryViewModelTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _configPath;

    public GameLibraryViewModelTests()
    {
        // Create temporary directory for test files
        _tempDir = Path.Combine(Path.GetTempPath(), "Win32EmuTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);

        _configPath = Path.Combine(_tempDir, "config.json");
    }

    [Fact]
    public void ShowGameInfoCommand_Exists_AndCanBeInvoked()
    {
        // Arrange
        var config = new EmulatorConfiguration();
        var configService = new ConfigurationService();
        var viewModel = new GameLibraryViewModel(config, configService);

        var testGame = new Game
        {
            Title = "Test Game",
            ExecutablePath = Path.Combine(_tempDir, "test.exe"),
            Description = "Test description"
        };

        // Act & Assert
        Assert.NotNull(viewModel.ShowGameInfoCommand);
        Assert.True(viewModel.ShowGameInfoCommand.CanExecute(testGame));
    }

    [Fact]
    public void LaunchGameCommand_Exists_AndCanBeInvoked()
    {
        // Arrange
        var config = new EmulatorConfiguration();
        var configService = new ConfigurationService();
        var viewModel = new GameLibraryViewModel(config, configService);

        var testGame = new Game
        {
            Title = "Test Game",
            ExecutablePath = Path.Combine(_tempDir, "test.exe"),
            Description = "Test description"
        };

        // Act & Assert
        Assert.NotNull(viewModel.LaunchGameCommand);
        Assert.True(viewModel.LaunchGameCommand.CanExecute(testGame));
    }

    [Fact]
    public void RemoveGameCommand_Exists_AndCanBeInvoked()
    {
        // Arrange
        var config = new EmulatorConfiguration();
        var configService = new ConfigurationService();
        var viewModel = new GameLibraryViewModel(config, configService);

        var testGame = new Game
        {
            Title = "Test Game",
            ExecutablePath = Path.Combine(_tempDir, "test.exe"),
            Description = "Test description"
        };

        // Add the game first
        viewModel.Games.Add(testGame);

        // Act & Assert
        Assert.NotNull(viewModel.RemoveGameCommand);
        Assert.True(viewModel.RemoveGameCommand.CanExecute(testGame));
        
        // Execute the command
        viewModel.RemoveGameCommand.Execute(testGame);
        
        // Verify the game was removed
        Assert.DoesNotContain(testGame, viewModel.Games);
    }

    [Fact]
    public void ContextMenuCommands_HandleNullGameParameter()
    {
        // Arrange
        var config = new EmulatorConfiguration();
        var configService = new ConfigurationService();
        var viewModel = new GameLibraryViewModel(config, configService);

        // Act & Assert - Commands should handle null gracefully
        Assert.NotNull(viewModel.ShowGameInfoCommand);
        Assert.NotNull(viewModel.LaunchGameCommand);
        Assert.NotNull(viewModel.RemoveGameCommand);

        // These should not throw when passed null
        viewModel.ShowGameInfoCommand.Execute(null);
        viewModel.RemoveGameCommand.Execute(null);
    }

    public void Dispose()
    {
        // Clean up temp directory
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }
}
