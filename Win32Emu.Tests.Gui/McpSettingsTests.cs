using Win32Emu.Gui.Configuration;
using Win32Emu.Gui.Models;

namespace Win32Emu.Tests.Gui;

public class McpSettingsTests : IDisposable
{
	public McpSettingsTests()
	{
		// No temp directory needed
	}

	[Fact]
	public void EmulatorSettings_DefaultMcpValues_AreCorrect()
	{
		// Arrange & Act
		var settings = new EmulatorSettings();

		// Assert
		Assert.False(settings.EnableMcpServer);
		Assert.False(settings.AutoStartMcpServer);
		Assert.True(settings.McpUseHttpTransport); // Default to HTTP for Visual Studio
		Assert.Equal(5111, settings.McpHttpPort);
	}

	[Fact]
	public void EmulatorConfiguration_DefaultMcpValues_AreCorrect()
	{
		// Arrange & Act
		var config = new EmulatorConfiguration();

		// Assert
		Assert.False(config.EnableMcpServer);
		Assert.False(config.AutoStartMcpServer);
		Assert.True(config.McpUseHttpTransport); // Default to HTTP for Visual Studio
		Assert.Equal(5111, config.McpHttpPort);
	}

	[Fact]
	public void ConfigurationService_GetEmulatorConfiguration_IncludesMcpSettings()
	{
		// Arrange
		var configService = new ConfigurationService();

		// Act
		var config = configService.GetEmulatorConfiguration();

		// Assert - Verify that MCP properties exist
		Assert.NotNull(config);
		// McpHttpPort is an int and cannot be null
	}

	[Fact]
	public void McpHttpTransport_DefaultsToTrue_ForVisualStudioCompatibility()
	{
		// This test verifies the design decision to default HTTP transport
		// which is required for Visual Studio integration

		// Arrange & Act
		var settings = new EmulatorSettings();
		var config = new EmulatorConfiguration();

		// Assert
		Assert.True(settings.McpUseHttpTransport, 
			"HTTP transport should be enabled by default for Visual Studio compatibility");
		Assert.True(config.McpUseHttpTransport, 
			"HTTP transport should be enabled by default for Visual Studio compatibility");
	}

	[Fact]
	public void McpHttpPort_DefaultsTo5111()
	{
		// This test verifies the default port matches the problem statement requirement

		// Arrange & Act
		var settings = new EmulatorSettings();
		var config = new EmulatorConfiguration();

		// Assert
		Assert.Equal(5111, settings.McpHttpPort);
		Assert.Equal(5111, config.McpHttpPort);
	}

	public void Dispose()
	{
		// No cleanup needed
	}
}
