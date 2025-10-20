using Win32Emu.Gui.Configuration;
using Win32Emu.Gui.Models;
using Win32Emu.Gui.ViewModels;

namespace Win32Emu.Tests.Gui;

public class TelemetrySettingsTests : IDisposable
{
    private readonly string _tempDir;

    public TelemetrySettingsTests()
    {
        // Create temporary directory for test files
        _tempDir = Path.Combine(Path.GetTempPath(), "Win32EmuTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public void EmulatorSettings_DefaultTelemetryValues_AreCorrect()
    {
        // Arrange & Act
        var settings = new EmulatorSettings();

        // Assert
        Assert.False(settings.EnableOpenTelemetry);
        Assert.False(settings.UseConsoleExporter);
        Assert.False(settings.UseOtlpExporter);
        Assert.Equal("http://localhost:4317", settings.OtlpEndpoint);
    }

    [Fact]
    public void EmulatorConfiguration_DefaultTelemetryValues_AreCorrect()
    {
        // Arrange & Act
        var config = new EmulatorConfiguration();

        // Assert
        Assert.False(config.EnableOpenTelemetry);
        Assert.False(config.UseConsoleExporter);
        Assert.False(config.UseOtlpExporter);
        Assert.Equal("http://localhost:4317", config.OtlpEndpoint);
    }

    [Fact]
    public void ConfigurationService_GetEmulatorConfiguration_IncludesTelemetrySettings()
    {
        // Arrange
        var configService = new ConfigurationService();

        // Act
        var config = configService.GetEmulatorConfiguration();

        // Assert
        Assert.NotNull(config);
        Assert.False(config.EnableOpenTelemetry);
        Assert.False(config.UseConsoleExporter);
        Assert.False(config.UseOtlpExporter);
        Assert.Equal("http://localhost:4317", config.OtlpEndpoint);
    }

    [Fact]
    public void ConfigurationService_SaveAndLoadTelemetrySettings_Persists()
    {
        // Arrange
        var configService = new ConfigurationService();
        var config = configService.GetEmulatorConfiguration();
        
        // Modify telemetry settings
        config.EnableOpenTelemetry = true;
        config.UseConsoleExporter = true;
        config.UseOtlpExporter = true;
        config.OtlpEndpoint = "http://custom-endpoint:4317";

        // Act
        configService.SaveEmulatorConfiguration(config);
        var loadedConfig = configService.GetEmulatorConfiguration();

        // Assert
        Assert.True(loadedConfig.EnableOpenTelemetry);
        Assert.True(loadedConfig.UseConsoleExporter);
        Assert.True(loadedConfig.UseOtlpExporter);
        Assert.Equal("http://custom-endpoint:4317", loadedConfig.OtlpEndpoint);
    }

    [Fact]
    public void SettingsViewModel_TelemetryProperties_AreObservable()
    {
        // Arrange
        var config = new EmulatorConfiguration();
        var configService = new ConfigurationService();
        var viewModel = new SettingsViewModel(config, configService);

        // Assert - Check that properties exist and have default values
        Assert.False(viewModel.EnableOpenTelemetry);
        Assert.False(viewModel.UseConsoleExporter);
        Assert.False(viewModel.UseOtlpExporter);
        Assert.Equal("http://localhost:4317", viewModel.OtlpEndpoint);
    }

    [Fact]
    public void SettingsViewModel_ChangingTelemetrySettings_UpdatesConfiguration()
    {
        // Arrange
        var config = new EmulatorConfiguration();
        var configService = new ConfigurationService();
        var viewModel = new SettingsViewModel(config, configService);

        // Act
        viewModel.EnableOpenTelemetry = true;
        viewModel.UseConsoleExporter = true;
        viewModel.UseOtlpExporter = true;
        viewModel.OtlpEndpoint = "http://test:4317";

        // Assert
        Assert.True(config.EnableOpenTelemetry);
        Assert.True(config.UseConsoleExporter);
        Assert.True(config.UseOtlpExporter);
        Assert.Equal("http://test:4317", config.OtlpEndpoint);
    }

    [Fact]
    public void SettingsViewModel_InitializesFromConfiguration()
    {
        // Arrange
        var config = new EmulatorConfiguration
        {
            EnableOpenTelemetry = true,
            UseConsoleExporter = true,
            UseOtlpExporter = false,
            OtlpEndpoint = "http://initial:4317"
        };
        var configService = new ConfigurationService();

        // Act
        var viewModel = new SettingsViewModel(config, configService);

        // Assert
        Assert.True(viewModel.EnableOpenTelemetry);
        Assert.True(viewModel.UseConsoleExporter);
        Assert.False(viewModel.UseOtlpExporter);
        Assert.Equal("http://initial:4317", viewModel.OtlpEndpoint);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            try
            {
                Directory.Delete(_tempDir, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
