using Win32Emu.Gui.Configuration;
using Win32Emu.Gui.Models;
using Win32Emu.Gui.ViewModels;

namespace Win32Emu.Tests.Gui;

public class Force32BitStackOpsSettingsTests
{
	[Fact]
	public void EmulatorSettings_DefaultForce32BitStackOps_IsTrue()
	{
		// Arrange & Act
		var settings = new EmulatorSettings();

		// Assert
		Assert.True(settings.Force32BitStackOps);
	}

	[Fact]
	public void EmulatorConfiguration_DefaultForce32BitStackOps_IsTrue()
	{
		// Arrange & Act
		var config = new EmulatorConfiguration();

		// Assert
		Assert.True(config.Force32BitStackOps);
	}

	[Fact]
	public void ConfigurationService_GetEmulatorConfiguration_IncludesForce32BitStackOps()
	{
		// Arrange
		var configService = new ConfigurationService();

		// Act
		var config = configService.GetEmulatorConfiguration();

		// Assert - Verify that the property exists (value may vary if config file exists)
		Assert.NotNull(config);
		// Verify the property has a boolean value (not checking specific value since config file may exist)
		_ = config.Force32BitStackOps; // This will throw if property doesn't exist
	}

	[Fact]
	public void ConfigurationService_SaveAndLoadForce32BitStackOps_Persists()
	{
		// Arrange
		var configService = new ConfigurationService();
		var config = configService.GetEmulatorConfiguration();
		
		// Store original value to restore later
		var originalValue = config.Force32BitStackOps;
		
		// Modify Force32BitStackOps setting to opposite of original
		config.Force32BitStackOps = !originalValue;

		// Act
		configService.SaveEmulatorConfiguration(config);
		var loadedConfig = configService.GetEmulatorConfiguration();

		// Assert
		Assert.Equal(!originalValue, loadedConfig.Force32BitStackOps);
		
		// Cleanup - restore original value
		loadedConfig.Force32BitStackOps = originalValue;
		configService.SaveEmulatorConfiguration(loadedConfig);
	}

	[Fact]
	public void SettingsViewModel_Force32BitStackOps_IsObservable()
	{
		// Arrange
		var config = new EmulatorConfiguration();
		var configService = new ConfigurationService();
		var viewModel = new SettingsViewModel(config, configService);

		// Assert - Check that property exists and has default value of true
		Assert.True(viewModel.Force32BitStackOps);
	}

	[Fact]
	public void SettingsViewModel_ChangingForce32BitStackOps_UpdatesConfiguration()
	{
		// Arrange
		var config = new EmulatorConfiguration();
		var configService = new ConfigurationService();
		var viewModel = new SettingsViewModel(config, configService);

		// Act
		viewModel.Force32BitStackOps = false;

		// Assert
		Assert.False(config.Force32BitStackOps);
	}

	[Fact]
	public void SettingsViewModel_InitializesForce32BitStackOpsFromConfiguration()
	{
		// Arrange
		var config = new EmulatorConfiguration
		{
			Force32BitStackOps = false
		};
		var configService = new ConfigurationService();

		// Act
		var viewModel = new SettingsViewModel(config, configService);

		// Assert
		Assert.False(viewModel.Force32BitStackOps);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void SettingsViewModel_SupportsBothForce32BitStackOpsValues(bool value)
	{
		// Arrange
		var config = new EmulatorConfiguration { Force32BitStackOps = value };
		var configService = new ConfigurationService();

		// Act
		var viewModel = new SettingsViewModel(config, configService);

		// Assert
		Assert.Equal(value, viewModel.Force32BitStackOps);
	}
}
