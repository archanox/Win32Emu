using Microsoft.Extensions.Logging;

namespace Win32Emu;

/// <summary>
/// Public API for launching the emulator from command-line arguments.
/// This class provides the main entry point for running the emulator,
/// designed to be called from the main thread to ensure proper backend initialization.
/// </summary>
public static class EmulatorLauncher
{
	/// <summary>
	/// Launch the emulator with the given command-line arguments.
	/// This method should be called on the main thread to ensure graphics backends work correctly on all platforms.
	/// </summary>
	/// <param name="args">Command-line arguments (first argument should be the path to the PE executable)</param>
	/// <param name="loggerFactory">Optional logger factory. If null, a default console logger will be created.</param>
	/// <returns>Exit code (0 for success, non-zero for error)</returns>
	public static int Launch(string[] args, ILoggerFactory? loggerFactory = null)
	{
		if (args.Length == 0)
		{
			PrintUsage();
			return 1;
		}

		// Parse command line arguments
		var debugMode = args.Contains("--debug");
		var interactiveDebugMode = args.Contains("--interactive-debug");
		var gdbServerMode = args.Contains("--gdb-server");
		var gdbServerPort = 1234; // Default port
		
		// Check for custom GDB server port
		if (gdbServerMode)
		{
			var gdbServerIndex = Array.IndexOf(args, "--gdb-server");
			if (gdbServerIndex >= 0 && gdbServerIndex + 1 < args.Length && 
			    int.TryParse(args[gdbServerIndex + 1], out var customPort))
			{
				gdbServerPort = customPort;
			}
		}
		
		// Parse API tracing options
		var enableApiTrace = args.Contains("--trace-api");
		string? apiTraceOutputPath = null;
		if (enableApiTrace)
		{
			var traceIndex = Array.IndexOf(args, "--trace-api");
			if (traceIndex >= 0 && traceIndex + 1 < args.Length && 
			    !args[traceIndex + 1].StartsWith("--"))
			{
				apiTraceOutputPath = args[traceIndex + 1];
			}
		}
		
		var compareApiMonLog = args.Contains("--compare-apimon");
		string? apiMonLogPath = null;
		if (compareApiMonLog)
		{
			var compareIndex = Array.IndexOf(args, "--compare-apimon");
			if (compareIndex >= 0 && compareIndex + 1 < args.Length && 
			    !args[compareIndex + 1].StartsWith("--"))
			{
				apiMonLogPath = args[compareIndex + 1];
			}
		}
		
		// Parse OpenTelemetry options
		var telemetryConsoleMode = args.Contains("--telemetry-console");
		var telemetryOtlpMode = args.Contains("--telemetry-otlp");
		var telemetryOtlpEndpoint = "http://localhost:4317"; // Default endpoint
		
		if (telemetryOtlpMode)
		{
			var otlpIndex = Array.IndexOf(args, "--telemetry-otlp");
			if (otlpIndex >= 0 && otlpIndex + 1 < args.Length && 
			    !args[otlpIndex + 1].StartsWith("--"))
			{
				telemetryOtlpEndpoint = args[otlpIndex + 1];
			}
		}

		// Check for backend selection
		var backendIndex = Array.IndexOf(args, "--backend");
		if (backendIndex >= 0 && backendIndex + 1 < args.Length &&
		    Enum.TryParse<Rendering.BackendType>(args[backendIndex + 1], ignoreCase: true, out var backendType))
		{
			Rendering.BackendFactory.CurrentBackendType = backendType;
		}
		
		// Find the first non-flag argument as the path
		var path = args.FirstOrDefault(arg => !arg.StartsWith("--"));
		if (string.IsNullOrEmpty(path))
		{
			PrintUsage();
			return 1;
		}

		// Set up logging - use provided factory or create default
		var shouldDisposeLoggerFactory = false;
		if (loggerFactory == null)
		{
			loggerFactory = LoggerFactory.Create(builder =>
			{
				builder
					.AddConsole()
					.SetMinimumLevel(debugMode ? LogLevel.Debug : LogLevel.Information);
			});
			shouldDisposeLoggerFactory = true;
		}

		try
		{
			var logger = loggerFactory.CreateLogger<Emulator>();

			// Initialize OpenTelemetry if enabled
			Telemetry.TelemetryService? telemetryService = null;
			
			// First check for environment variables
			var envConfig = Telemetry.TelemetryConfig.FromEnvironment();
			
			// Command-line arguments override environment variables
			if (telemetryConsoleMode || telemetryOtlpMode || envConfig.UseOtlpExporter)
			{
				var telemetryConfig = new Telemetry.TelemetryConfig
				{
					EnableTracing = true,
					EnableMetrics = true,
					UseConsoleExporter = telemetryConsoleMode,
					UseOtlpExporter = telemetryOtlpMode || envConfig.UseOtlpExporter,
					OtlpEndpoint = telemetryOtlpMode ? telemetryOtlpEndpoint : envConfig.OtlpEndpoint
				};
				
				telemetryService = new Telemetry.TelemetryService(telemetryConfig);
				logger.LogInformation("OpenTelemetry initialized - Console: {Console}, OTLP: {Otlp} ({Endpoint})", 
					telemetryConfig.UseConsoleExporter, telemetryConfig.UseOtlpExporter, telemetryConfig.OtlpEndpoint);
			}

			try
			{
				using var emulator = new Emulator(null, logger, telemetryService);
				emulator.LoadExecutable(path, null, debugMode, interactiveDebugMode, 256, gdbServerMode, gdbServerPort);
				
				// Enable API tracing if requested
				if (enableApiTrace && emulator.Environment != null)
				{
					emulator.Environment.EnableApiTracing(apiTraceOutputPath, enableDetailedParameters: true);
					logger.LogInformation("API call tracing enabled - output: {Output}", apiTraceOutputPath ?? "console");
					
					// Set the tracer on the dispatcher if available
					if (emulator.Win32Dispatcher != null && emulator.Environment.ApiCallTracer != null)
					{
						emulator.Win32Dispatcher.SetApiCallTracer(emulator.Environment.ApiCallTracer);
					}
					
					// Load API Monitor comparison data if requested
					// TODO(enhancement): Implement real-time comparison during execution
					// Currently comparison is manual via ApiMonComparator.GenerateComparisonReport()
					// Could be enhanced to show divergence in real-time during emulation.
					// See issue: (create issue to track this enhancement)
					if (compareApiMonLog && !string.IsNullOrEmpty(apiMonLogPath))
					{
						logger.LogInformation("API Monitor comparison enabled - log: {ApiMonLog}", apiMonLogPath);
						logger.LogInformation("Note: Comparison report can be generated manually using ApiMonComparator");
					}
				}
				
				emulator.Run();
				
				// Generate diagnostic report if tracing was enabled
				if (enableApiTrace && emulator.Environment != null)
				{
					var report = emulator.Environment.DisableApiTracing();
					if (!string.IsNullOrEmpty(report))
					{
						logger.LogInformation("API Call Diagnostic Report:\n{Report}", report);
					}
				}
				
				return 0;
			}
			catch (FileNotFoundException ex)
			{
				logger.LogError("Error: {Message}", ex.Message);
				return 2;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Emulator error: {Message}", ex.Message);
				if (debugMode)
				{
					logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
				}
				return 1;
			}
			finally
			{
				telemetryService?.Dispose();
			}
		}
		finally
		{
			if (shouldDisposeLoggerFactory)
			{
				loggerFactory.Dispose();
			}
		}
	}

	/// <summary>
	/// Print usage information to the console
	/// </summary>
	public static void PrintUsage()
	{
		Console.WriteLine("Usage: Win32Emu <path-to-pe> [options]");
		Console.WriteLine();
		Console.WriteLine("Options:");
		Console.WriteLine("  --debug              Enable enhanced debugging to catch memory access errors");
		Console.WriteLine("  --interactive-debug  Enable interactive step-through debugger (GDB-like)");
		Console.WriteLine("  --gdb-server [port]  Start GDB server for remote debugging (default port: 1234)");
		Console.WriteLine("  --backend <SDL|GLFW|Vulkan|Metal|Software> Select rendering backend (default: SDL)");
		Console.WriteLine("  --trace-api [file]   Enable comprehensive API call tracing (optional output file)");
		Console.WriteLine("  --compare-apimon <csv> Compare behavior against API Monitor CSV log");
		Console.WriteLine("  --telemetry-console  Enable OpenTelemetry with console exporter");
		Console.WriteLine("  --telemetry-otlp [endpoint] Enable OpenTelemetry with OTLP exporter (default: http://localhost:4317)");
		Console.WriteLine();
		Console.WriteLine("Environment Variables:");
		Console.WriteLine("  WIN32EMU_BACKEND             Set backend type (SDL, GLFW, Vulkan, Metal, or Software)");
		Console.WriteLine("  OTEL_EXPORTER_OTLP_ENDPOINT  OpenTelemetry OTLP endpoint (e.g., http://localhost:4317)");
		Console.WriteLine();
		Console.WriteLine("Examples:");
		Console.WriteLine("  Win32Emu game.exe");
		Console.WriteLine("  Win32Emu game.exe --debug");
		Console.WriteLine("  Win32Emu game.exe --trace-api trace.log");
		Console.WriteLine("  Win32Emu game.exe --trace-api --compare-apimon ApiMonLogs/game.csv");
		Console.WriteLine("  Win32Emu game.exe --backend SDL");
		Console.WriteLine("  Win32Emu game.exe --backend GLFW");
		Console.WriteLine("  Win32Emu game.exe --backend Vulkan");
		Console.WriteLine("  Win32Emu game.exe --backend Metal");
		Console.WriteLine("  Win32Emu game.exe --backend Software");
		Console.WriteLine("  Win32Emu game.exe --interactive-debug");
		Console.WriteLine("  Win32Emu game.exe --gdb-server");
		Console.WriteLine("  Win32Emu game.exe --gdb-server 5678");
	}
}
