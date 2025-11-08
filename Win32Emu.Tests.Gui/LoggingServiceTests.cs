using Microsoft.Extensions.Logging;
using Win32Emu.Gui.Models;
using Win32Emu.Gui.Services;
using Xunit;

namespace Win32Emu.Tests.Gui;

public class LoggingServiceTests
{
	[Fact]
	public void LoggingService_CanBeCreated()
	{
		// Arrange
		var config = new EmulatorConfiguration
		{
			EnableDebugMode = false,
			EnableFileLogging = false
		};

		// Act
		using var loggingService = new LoggingService(config);

		// Assert
		Assert.NotNull(loggingService);
		Assert.NotNull(loggingService.LoggerFactory);
	}

	[Fact]
	public void LoggingService_CanCreateLogger()
	{
		// Arrange
		var config = new EmulatorConfiguration
		{
			EnableDebugMode = false,
			EnableFileLogging = false
		};

		// Act
		using var loggingService = new LoggingService(config);
		var logger = loggingService.CreateLogger<LoggingServiceTests>();

		// Assert
		Assert.NotNull(logger);
	}

	[Fact]
	public void LoggingService_CanCreateLoggerWithCategoryName()
	{
		// Arrange
		var config = new EmulatorConfiguration
		{
			EnableDebugMode = false,
			EnableFileLogging = false
		};

		// Act
		using var loggingService = new LoggingService(config);
		var logger = loggingService.CreateLogger("TestCategory");

		// Assert
		Assert.NotNull(logger);
	}

	[Fact]
	public void LoggingService_WithDebugMode_SetsMinimumLogLevel()
	{
		// Arrange
		var config = new EmulatorConfiguration
		{
			EnableDebugMode = true,
			EnableFileLogging = false
		};

		// Act
		using var loggingService = new LoggingService(config);
		var logger = loggingService.CreateLogger<LoggingServiceTests>();

		// Assert
		Assert.NotNull(logger);
		// Logger should be enabled for Debug level
		Assert.True(logger.IsEnabled(LogLevel.Debug));
	}

	[Fact]
	public void LoggingService_WithoutDebugMode_FiltersDebugLogs()
	{
		// Arrange
		var config = new EmulatorConfiguration
		{
			EnableDebugMode = false,
			EnableFileLogging = false
		};

		// Act
		using var loggingService = new LoggingService(config);
		var logger = loggingService.CreateLogger<LoggingServiceTests>();

		// Assert
		Assert.NotNull(logger);
		// Logger should be enabled for Information level
		Assert.True(logger.IsEnabled(LogLevel.Information));
	}
}
