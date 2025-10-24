using Win32Emu.Gui.Configuration;
using Win32Emu.Gui.Models;
using Win32Emu.Gui.ViewModels;

namespace Win32Emu.Tests.Gui;

public class CpuBackendSettingsTests
{
    [Fact]
    public void EmulatorSettings_DefaultCpuBackend_IsIcedCPU()
    {
        // Arrange & Act
        var settings = new EmulatorSettings();

        // Assert
        Assert.Equal("IcedCPU", settings.CpuBackend);
    }

    [Fact]
    public void EmulatorConfiguration_DefaultCpuBackend_IsIcedCPU()
    {
        // Arrange & Act
        var config = new EmulatorConfiguration();

        // Assert
        Assert.Equal("IcedCPU", config.CpuBackend);
    }

    [Fact]
    public void ConfigurationService_GetEmulatorConfiguration_IncludesCpuBackend()
    {
        // Arrange
        var configService = new ConfigurationService();

        // Act
        var config = configService.GetEmulatorConfiguration();

        // Assert
        Assert.NotNull(config);
        Assert.NotNull(config.CpuBackend);
        Assert.Contains(config.CpuBackend, new[] { "IcedCPU", "JitCPU" });
    }

    [Fact]
    public void ConfigurationService_SaveAndLoadCpuBackend_Persists()
    {
        // Arrange
        var configService = new ConfigurationService();
        var config = configService.GetEmulatorConfiguration();
        
        // Store original value to restore later
        var originalCpuBackend = config.CpuBackend;
        
        // Modify CPU backend setting
        config.CpuBackend = "JitCPU";

        // Act
        configService.SaveEmulatorConfiguration(config);
        var loadedConfig = configService.GetEmulatorConfiguration();

        // Assert
        Assert.Equal("JitCPU", loadedConfig.CpuBackend);
        
        // Cleanup - restore original value
        config.CpuBackend = originalCpuBackend;
        configService.SaveEmulatorConfiguration(config);
    }

    [Fact]
    public void SettingsViewModel_CpuBackend_IsObservable()
    {
        // Arrange
        var config = new EmulatorConfiguration();
        var configService = new ConfigurationService();
        var viewModel = new SettingsViewModel(config, configService);

        // Assert
        Assert.Equal("IcedCPU", viewModel.CpuBackend);
        Assert.NotEmpty(viewModel.CpuBackends);
        Assert.Contains("IcedCPU", viewModel.CpuBackends);
        Assert.Contains("JitCPU", viewModel.CpuBackends);
        Assert.Contains("Unicorn", viewModel.CpuBackends);
    }

    [Fact]
    public void SettingsViewModel_ChangingCpuBackend_UpdatesConfiguration()
    {
        // Arrange
        var config = new EmulatorConfiguration();
        var configService = new ConfigurationService();
        var viewModel = new SettingsViewModel(config, configService);

        // Act
        viewModel.CpuBackend = "JitCPU";

        // Assert
        Assert.Equal("JitCPU", config.CpuBackend);
    }

    [Fact]
    public void SettingsViewModel_InitializesCpuBackendFromConfiguration()
    {
        // Arrange
        var config = new EmulatorConfiguration
        {
            CpuBackend = "JitCPU"
        };
        var configService = new ConfigurationService();

        // Act
        var viewModel = new SettingsViewModel(config, configService);

        // Assert
        Assert.Equal("JitCPU", viewModel.CpuBackend);
    }

    [Theory]
    [InlineData("IcedCPU")]
    [InlineData("JitCPU")]
    [InlineData("Unicorn")]
    public void SettingsViewModel_SupportsAllCpuBackends(string cpuBackend)
    {
        // Arrange
        var config = new EmulatorConfiguration { CpuBackend = cpuBackend };
        var configService = new ConfigurationService();

        // Act
        var viewModel = new SettingsViewModel(config, configService);

        // Assert
        Assert.Equal(cpuBackend, viewModel.CpuBackend);
        Assert.Contains(cpuBackend, viewModel.CpuBackends);
    }
}
