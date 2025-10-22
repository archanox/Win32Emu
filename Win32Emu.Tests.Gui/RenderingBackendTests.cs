using Win32Emu.Gui.Configuration;
using Win32Emu.Gui.Models;
using Win32Emu.Gui.ViewModels;

namespace Win32Emu.Tests.Gui;

public class RenderingBackendTests
{
    [Fact]
    public void SettingsViewModel_RenderingBackends_ContainsAllBackends()
    {
        // Arrange
        var config = new EmulatorConfiguration();
        var configService = new ConfigurationService();
        var viewModel = new SettingsViewModel(config, configService);

        // Act
        var backends = viewModel.RenderingBackends;

        // Assert
        Assert.Contains("SDL", backends);
        Assert.Contains("GLFW", backends);
        Assert.Contains("Vulkan", backends);
        Assert.Contains("Metal", backends);
        Assert.Equal(4, backends.Count);
    }

    [Fact]
    public void EmulatorConfiguration_DefaultRenderingBackend_IsSDL()
    {
        // Arrange & Act
        var config = new EmulatorConfiguration();

        // Assert
        Assert.Equal("SDL", config.RenderingBackend);
    }

    [Fact]
    public void SettingsViewModel_CanSetRenderingBackend_ToSDL()
    {
        // Arrange
        var config = new EmulatorConfiguration();
        var configService = new ConfigurationService();
        var viewModel = new SettingsViewModel(config, configService);

        // Act
        viewModel.RenderingBackend = "SDL";

        // Assert
        Assert.Equal("SDL", viewModel.RenderingBackend);
        Assert.Equal("SDL", config.RenderingBackend);
    }

    [Fact]
    public void SettingsViewModel_CanSetRenderingBackend_ToGLFW()
    {
        // Arrange
        var config = new EmulatorConfiguration();
        var configService = new ConfigurationService();
        var viewModel = new SettingsViewModel(config, configService);

        // Act
        viewModel.RenderingBackend = "GLFW";

        // Assert
        Assert.Equal("GLFW", viewModel.RenderingBackend);
        Assert.Equal("GLFW", config.RenderingBackend);
    }

    [Fact]
    public void SettingsViewModel_CanSetRenderingBackend_ToVulkan()
    {
        // Arrange
        var config = new EmulatorConfiguration();
        var configService = new ConfigurationService();
        var viewModel = new SettingsViewModel(config, configService);

        // Act
        viewModel.RenderingBackend = "Vulkan";

        // Assert
        Assert.Equal("Vulkan", viewModel.RenderingBackend);
        Assert.Equal("Vulkan", config.RenderingBackend);
    }

    [Fact]
    public void SettingsViewModel_CanSetRenderingBackend_ToMetal()
    {
        // Arrange
        var config = new EmulatorConfiguration();
        var configService = new ConfigurationService();
        var viewModel = new SettingsViewModel(config, configService);

        // Act
        viewModel.RenderingBackend = "Metal";

        // Assert
        Assert.Equal("Metal", viewModel.RenderingBackend);
        Assert.Equal("Metal", config.RenderingBackend);
    }

    [Fact]
    public void ConfigurationService_SaveAndLoadRenderingBackend_Persists()
    {
        // Arrange
        var configService = new ConfigurationService();
        var config = configService.GetEmulatorConfiguration();

        // Act - Save each backend
        var backends = new[] { "SDL", "GLFW", "Vulkan", "Metal" };
        foreach (var backend in backends)
        {
            config.RenderingBackend = backend;
            configService.SaveEmulatorConfiguration(config);
            var loadedConfig = configService.GetEmulatorConfiguration();

            // Assert
            Assert.Equal(backend, loadedConfig.RenderingBackend);
        }
    }
}
