using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Win32Emu.Telemetry;

/// <summary>
/// Provides OpenTelemetry support for logging, metrics, and tracing
/// </summary>
public sealed class TelemetryService : IDisposable
{
	private readonly TracerProvider? _tracerProvider;
	private readonly MeterProvider? _meterProvider;
	private readonly TelemetryConfig _config;
	
	public ActivitySource ActivitySource { get; }
	public Meter Meter { get; }

	public TelemetryService(TelemetryConfig config)
	{
		_config = config ?? throw new ArgumentNullException(nameof(config));
		
		// Create ActivitySource for distributed tracing
		ActivitySource = new ActivitySource("Win32Emu", "1.0.0");
		
		// Create Meter for metrics
		Meter = new Meter("Win32Emu", "1.0.0");
		
		// Configure tracer provider
		if (config.EnableTracing)
		{
			var tracerBuilder = Sdk.CreateTracerProviderBuilder()
				.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Win32Emu", serviceVersion: "1.0.0"))
				.AddSource("Win32Emu");
			
			if (config.UseConsoleExporter)
			{
				tracerBuilder.AddConsoleExporter();
			}
			
			if (config.UseOtlpExporter && !string.IsNullOrEmpty(config.OtlpEndpoint))
			{
				tracerBuilder.AddOtlpExporter(options =>
				{
					options.Endpoint = new Uri(config.OtlpEndpoint);
				});
			}
			
			_tracerProvider = tracerBuilder.Build();
		}
		
		// Configure meter provider
		if (config.EnableMetrics)
		{
			var meterBuilder = Sdk.CreateMeterProviderBuilder()
				.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Win32Emu", serviceVersion: "1.0.0"))
				.AddMeter("Win32Emu")
				.AddRuntimeInstrumentation();
			
			if (config.UseConsoleExporter)
			{
				meterBuilder.AddConsoleExporter();
			}
			
			if (config.UseOtlpExporter && !string.IsNullOrEmpty(config.OtlpEndpoint))
			{
				meterBuilder.AddOtlpExporter(options =>
				{
					options.Endpoint = new Uri(config.OtlpEndpoint);
				});
			}
			
			_meterProvider = meterBuilder.Build();
		}
	}
	
	/// <summary>
	/// Start an activity for tracing/profiling
	/// </summary>
	public Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
	{
		return ActivitySource.StartActivity(name, kind);
	}
	
	public void Dispose()
	{
		_tracerProvider?.Dispose();
		_meterProvider?.Dispose();
		ActivitySource.Dispose();
		Meter.Dispose();
	}
}