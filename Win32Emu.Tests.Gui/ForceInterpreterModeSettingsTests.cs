using Win32Emu.Gui.Configuration;
using Win32Emu.Gui.Models;
using Win32Emu.Gui.ViewModels;

namespace Win32Emu.Tests.Gui;

public class ForceInterpreterModeSettingsTests
{
	[Fact]
	public void EmulatorSettings_DefaultForceInterpreterMode_IsFalse()
	{
		// Arrange & Act
		var settings = new EmulatorSettings();

		// Assert
		Assert.False(settings.ForceInterpreterMode);
	}

	[Fact]
	public void EmulatorConfiguration_DefaultForceInterpreterMode_IsFalse()
	{
		// Arrange & Act
		var config = new EmulatorConfiguration();

		// Assert
		Assert.False(config.ForceInterpreterMode);
	}

	[Fact]
	public void ConfigurationService_GetEmulatorConfiguration_IncludesForceInterpreterMode()
	{
		// Arrange
		var configService = new ConfigurationService();

		// Act
		var config = configService.GetEmulatorConfiguration();

		// Assert - Verify that the property exists (value may vary if config file exists)
		Assert.NotNull(config);
		// Verify the property has a boolean value (not checking specific value since config file may exist)
		_ = config.ForceInterpreterMode; // This will throw if property doesn't exist
	}

	[Fact]
	public void ConfigurationService_SaveAndLoadForceInterpreterMode_Persists()
	{
		// Arrange
		var configService = new ConfigurationService();
		var config = configService.GetEmulatorConfiguration();
		
		// Store original value to restore later
		var originalValue = config.ForceInterpreterMode;
		
		// Modify ForceInterpreterMode setting to opposite of original
		config.ForceInterpreterMode = !originalValue;

		// Act
		configService.SaveEmulatorConfiguration(config);
		var loadedConfig = configService.GetEmulatorConfiguration();

		// Assert
		Assert.Equal(!originalValue, loadedConfig.ForceInterpreterMode);
		
		// Cleanup - restore original value
		loadedConfig.ForceInterpreterMode = originalValue;
		configService.SaveEmulatorConfiguration(loadedConfig);
	}

	[Fact]
	public void SettingsViewModel_ForceInterpreterMode_IsObservable()
	{
		// Arrange
		var config = new EmulatorConfiguration();
		var configService = new ConfigurationService();
		var viewModel = new SettingsViewModel(config, configService);

		// Assert - Check that property exists and has default value of false
		Assert.False(viewModel.ForceInterpreterMode);
	}

	[Fact]
	public void SettingsViewModel_ChangingForceInterpreterMode_UpdatesConfiguration()
	{
		// Arrange
		var config = new EmulatorConfiguration();
		var configService = new ConfigurationService();
		var viewModel = new SettingsViewModel(config, configService);

		// Act
		viewModel.ForceInterpreterMode = true;

		// Assert
		Assert.True(config.ForceInterpreterMode);
	}

	[Fact]
	public void SettingsViewModel_InitializesForceInterpreterModeFromConfiguration()
	{
		// Arrange
		var config = new EmulatorConfiguration
		{
			ForceInterpreterMode = true
		};
		var configService = new ConfigurationService();

		// Act
		var viewModel = new SettingsViewModel(config, configService);

		// Assert
		Assert.True(viewModel.ForceInterpreterMode);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void SettingsViewModel_SupportsBothForceInterpreterModeValues(bool value)
	{
		// Arrange
		var config = new EmulatorConfiguration { ForceInterpreterMode = value };
		var configService = new ConfigurationService();

		// Act
		var viewModel = new SettingsViewModel(config, configService);

		// Assert
		Assert.Equal(value, viewModel.ForceInterpreterMode);
	}
}
