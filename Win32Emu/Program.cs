using Microsoft.Extensions.Logging;

namespace Win32Emu
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: Win32Emu <path-to-pe> [options]");
				Console.WriteLine();
				Console.WriteLine("Options:");
				Console.WriteLine("  --debug              Enable enhanced debugging to catch memory access errors");
				Console.WriteLine("  --interactive-debug  Enable interactive step-through debugger (GDB-like)");
				Console.WriteLine("  --gdb-server [port]  Start GDB server for remote debugging (default port: 1234)");
				Console.WriteLine("  --backend <SDL|GLFW> Select rendering backend (default: SDL)");
				Console.WriteLine("  --telemetry-console  Enable OpenTelemetry with console exporter");
				Console.WriteLine("  --telemetry-otlp [endpoint] Enable OpenTelemetry with OTLP exporter (default: http://localhost:4317)");
				Console.WriteLine();
				Console.WriteLine("Environment Variables:");
				Console.WriteLine("  WIN32EMU_BACKEND     Set backend type (SDL or GLFW)");
				Console.WriteLine();
				Console.WriteLine("Examples:");
				Console.WriteLine("  Win32Emu game.exe");
				Console.WriteLine("  Win32Emu game.exe --debug");
				Console.WriteLine("  Win32Emu game.exe --backend GLFW");
				Console.WriteLine("  Win32Emu game.exe --interactive-debug");
				Console.WriteLine("  Win32Emu game.exe --gdb-server");
				Console.WriteLine("  Win32Emu game.exe --gdb-server 5678");
				return;
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
			if (backendIndex >= 0 && backendIndex + 1 < args.Length)
			{
				if (Enum.TryParse<Rendering.BackendType>(args[backendIndex + 1], ignoreCase: true, out var backendType))
				{
					Rendering.BackendFactory.CurrentBackendType = backendType;
				}
			}
			
			var path = args[0];

			// Set up logging
			using var loggerFactory = LoggerFactory.Create(builder =>
			{
				builder
					.AddConsole()
					.SetMinimumLevel(debugMode ? LogLevel.Debug : LogLevel.Information);
			});

			var logger = loggerFactory.CreateLogger<Emulator>();

			// Initialize OpenTelemetry if enabled
			Telemetry.TelemetryService? telemetryService = null;
			if (telemetryConsoleMode || telemetryOtlpMode)
			{
				var telemetryConfig = new Telemetry.TelemetryConfig
				{
					EnableTracing = true,
					EnableMetrics = true,
					UseConsoleExporter = telemetryConsoleMode,
					UseOtlpExporter = telemetryOtlpMode,
					OtlpEndpoint = telemetryOtlpEndpoint
				};
				
				telemetryService = new Telemetry.TelemetryService(telemetryConfig);
				logger.LogInformation("OpenTelemetry initialized - Console: {Console}, OTLP: {Otlp} ({Endpoint})", 
					telemetryConsoleMode, telemetryOtlpMode, telemetryOtlpEndpoint);
			}

			try
			{
				using var emulator = new Emulator(null, logger, telemetryService);
				emulator.LoadExecutable(path, null, debugMode, interactiveDebugMode, 256, gdbServerMode, gdbServerPort);
				emulator.Run();
			}
			catch (FileNotFoundException ex)
			{
				logger.LogError("Error: {Message}", ex.Message);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Emulator error: {Message}", ex.Message);
				if (debugMode)
				{
					logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
				}
			}
			finally
			{
				telemetryService?.Dispose();
			}
		}
	}
}