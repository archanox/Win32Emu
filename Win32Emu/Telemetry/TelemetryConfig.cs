namespace Win32Emu.Telemetry
{
	/// <summary>
	/// Configuration for OpenTelemetry
	/// </summary>
	public class TelemetryConfig
	{
		/// <summary>
		/// Enable distributed tracing
		/// </summary>
		public bool EnableTracing { get; set; } = true;
	
		/// <summary>
		/// Enable metrics collection
		/// </summary>
		public bool EnableMetrics { get; set; } = true;
	
		/// <summary>
		/// Export telemetry to console
		/// </summary>
		public bool UseConsoleExporter { get; set; }
	
		/// <summary>
		/// Export telemetry via OTLP (OpenTelemetry Protocol)
		/// </summary>
		public bool UseOtlpExporter { get; set; }
	
		/// <summary>
		/// OTLP endpoint URL (e.g., "http://localhost:4317")
		/// </summary>
		public string OtlpEndpoint { get; set; } = "http://localhost:4317";
	
		/// <summary>
		/// Creates a TelemetryConfig by reading from environment variables.
		/// Respects standard OpenTelemetry environment variables like OTEL_EXPORTER_OTLP_ENDPOINT.
		/// </summary>
		public static TelemetryConfig FromEnvironment()
		{
			var config = new TelemetryConfig();
		
			// Check for OTEL_EXPORTER_OTLP_ENDPOINT environment variable
			var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
			if (!string.IsNullOrEmpty(otlpEndpoint))
			{
				config.UseOtlpExporter = true;
				config.OtlpEndpoint = otlpEndpoint;
			}
		
			return config;
		}
	}
}