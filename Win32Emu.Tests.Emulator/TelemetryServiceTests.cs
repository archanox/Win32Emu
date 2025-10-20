using Win32Emu.Telemetry;
using Xunit;

namespace Win32Emu.Tests.Emulator;

public class TelemetryServiceTests
{
	[Fact]
	public void TelemetryService_Constructor_ShouldInitializeWithoutErrors()
	{
		// Arrange
		var config = new TelemetryConfig
		{
			EnableTracing = true,
			EnableMetrics = true,
			UseConsoleExporter = false,
			UseOtlpExporter = false
		};

		// Act
		using var telemetryService = new TelemetryService(config);

		// Assert
		Assert.NotNull(telemetryService);
		Assert.NotNull(telemetryService.ActivitySource);
		Assert.NotNull(telemetryService.Meter);
	}

	[Fact]
	public void TelemetryService_StartActivity_ShouldCreateActivity()
	{
		// Arrange
		var config = new TelemetryConfig
		{
			EnableTracing = true,
			EnableMetrics = true,
			UseConsoleExporter = false,
			UseOtlpExporter = false
		};

		using var telemetryService = new TelemetryService(config);

		// Act
		using var activity = telemetryService.StartActivity("TestActivity");

		// Assert
		Assert.NotNull(activity);
		Assert.Equal("TestActivity", activity.DisplayName);
	}

	[Fact]
	public void EmulatorMetrics_Constructor_ShouldInitializeWithoutErrors()
	{
		// Arrange
		var config = new TelemetryConfig
		{
			EnableTracing = true,
			EnableMetrics = true,
			UseConsoleExporter = false,
			UseOtlpExporter = false
		};

		using var telemetryService = new TelemetryService(config);

		// Act
		var metrics = new EmulatorMetrics(telemetryService.Meter);

		// Assert
		Assert.NotNull(metrics);
	}

	[Fact]
	public void EmulatorMetrics_RecordInstructionsExecuted_ShouldNotThrow()
	{
		// Arrange
		var config = new TelemetryConfig
		{
			EnableTracing = true,
			EnableMetrics = true,
			UseConsoleExporter = false,
			UseOtlpExporter = false
		};

		using var telemetryService = new TelemetryService(config);
		var metrics = new EmulatorMetrics(telemetryService.Meter);

		// Act & Assert - should not throw
		metrics.RecordInstructionsExecuted(100);
	}

	[Fact]
	public void EmulatorMetrics_RecordApiCall_ShouldNotThrow()
	{
		// Arrange
		var config = new TelemetryConfig
		{
			EnableTracing = true,
			EnableMetrics = true,
			UseConsoleExporter = false,
			UseOtlpExporter = false
		};

		using var telemetryService = new TelemetryService(config);
		var metrics = new EmulatorMetrics(telemetryService.Meter);

		// Act & Assert - should not throw
		metrics.RecordApiCall("KERNEL32", "GetCurrentProcess");
	}

	[Fact]
	public void EmulatorMetrics_RecordMemoryAllocation_ShouldNotThrow()
	{
		// Arrange
		var config = new TelemetryConfig
		{
			EnableTracing = true,
			EnableMetrics = true,
			UseConsoleExporter = false,
			UseOtlpExporter = false
		};

		using var telemetryService = new TelemetryService(config);
		var metrics = new EmulatorMetrics(telemetryService.Meter);

		// Act & Assert - should not throw
		metrics.RecordMemoryAllocation(1024);
		metrics.RecordMemoryDeallocation(512);
	}

	[Fact]
	public void EmulatorMetrics_RecordApiDuration_ShouldNotThrow()
	{
		// Arrange
		var config = new TelemetryConfig
		{
			EnableTracing = true,
			EnableMetrics = true,
			UseConsoleExporter = false,
			UseOtlpExporter = false
		};

		using var telemetryService = new TelemetryService(config);
		var metrics = new EmulatorMetrics(telemetryService.Meter);

		// Act & Assert - should not throw
		metrics.RecordApiDuration("KERNEL32", "Sleep", 10.5);
	}

	[Fact]
	public void EmulatorMetrics_RecordException_ShouldNotThrow()
	{
		// Arrange
		var config = new TelemetryConfig
		{
			EnableTracing = true,
			EnableMetrics = true,
			UseConsoleExporter = false,
			UseOtlpExporter = false
		};

		using var telemetryService = new TelemetryService(config);
		var metrics = new EmulatorMetrics(telemetryService.Meter);

		// Act & Assert - should not throw
		metrics.RecordException("AccessViolation");
	}

	[Fact]
	public void TelemetryConfig_DefaultValues_ShouldBeCorrect()
	{
		// Arrange & Act
		var config = new TelemetryConfig();

		// Assert
		Assert.True(config.EnableTracing);
		Assert.True(config.EnableMetrics);
		Assert.False(config.UseConsoleExporter);
		Assert.False(config.UseOtlpExporter);
		Assert.Equal("http://localhost:4317", config.OtlpEndpoint);
	}

	[Fact]
	public void TelemetryConfig_FromEnvironment_ShouldUseEnvironmentVariable()
	{
		// Arrange
		var testEndpoint = "http://test-endpoint:4317";
		Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT", testEndpoint);

		try
		{
			// Act
			var config = TelemetryConfig.FromEnvironment();

			// Assert
			Assert.True(config.UseOtlpExporter);
			Assert.Equal(testEndpoint, config.OtlpEndpoint);
		}
		finally
		{
			// Cleanup
			Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT", null);
		}
	}

	[Fact]
	public void TelemetryConfig_FromEnvironment_ShouldReturnDefaultWhenEnvVarNotSet()
	{
		// Arrange - ensure the environment variable is not set
		Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT", null);

		// Act
		var config = TelemetryConfig.FromEnvironment();

		// Assert
		Assert.False(config.UseOtlpExporter);
		Assert.Equal("http://localhost:4317", config.OtlpEndpoint);
	}

	[Fact]
	public void TelemetryService_ShouldInitializeWithEnvironmentConfig()
	{
		// Arrange
		var testEndpoint = "http://rider-endpoint:4317";
		Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT", testEndpoint);

		try
		{
			var config = TelemetryConfig.FromEnvironment();

			// Act
			using var telemetryService = new TelemetryService(config);

			// Assert
			Assert.NotNull(telemetryService);
			Assert.NotNull(telemetryService.ActivitySource);
			Assert.NotNull(telemetryService.Meter);
		}
		finally
		{
			// Cleanup
			Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT", null);
		}
	}
}
