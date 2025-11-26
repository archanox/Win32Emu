using Microsoft.Extensions.Logging;
using Win32Emu.Logging;

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

		// Parse file logging options
		var enableFileLogging = args.Contains("--log-file");
		string? logFilePath = null;
		if (enableFileLogging)
		{
			var logFileIndex = Array.IndexOf(args, "--log-file");
			if (logFileIndex >= 0 && logFileIndex + 1 < args.Length && 
			    !args[logFileIndex + 1].StartsWith("--"))
			{
				logFilePath = args[logFileIndex + 1];
			}
		}

		// Check for backend selection
		// Note: Backend selection is now handled by the GUI layer through IBackendFactory
		var backendIndex = Array.IndexOf(args, "--backend");
		if (backendIndex >= 0 && backendIndex + 1 < args.Length &&
		    Enum.TryParse<Rendering.BackendType>(args[backendIndex + 1], ignoreCase: true, out var backendType))
		{
			// BackendFactory is no longer static - handled by GUI layer
			// The backendType value is parsed but not used here since backends are managed in GUI
			_ = backendType; // Acknowledge the parsed value
		}
		
		// Build list of flag argument indices (those that start with -- and their values)
		var flagIndices = new HashSet<int>();
		for (var i = 0; i < args.Length; i++)
		{
			if (args[i].StartsWith("--"))
			{
				flagIndices.Add(i);
				// Add the next index if this flag takes a value, but only if the value is valid for the flag
				if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
				{
					if (args[i] == "--backend")
					{
						// For --backend, only exclude the next arg if it's a valid backend type
						if (Enum.TryParse<Rendering.BackendType>(args[i + 1], ignoreCase: true, out var _))
						{
							flagIndices.Add(i + 1);
						}
					}
					else if (args[i] == "--trace-api" || args[i] == "--compare-apimon" ||
					         args[i] == "--log-file" || args[i] == "--telemetry-otlp" || args[i] == "--gdb-server")
					{
						// For these flags, the value is optional and can be any string except another flag.
						// We assume that if the next argument is not another flag, it is intended as the value.
						flagIndices.Add(i + 1);
					}
				}
			}
		}
		
		// Find the first non-flag argument as the path
		var path = args.Where((arg, index) => !flagIndices.Contains(index)).FirstOrDefault();
		if (string.IsNullOrEmpty(path))
		{
			PrintUsage();
			return 1;
		}

		// Generate log file path based on MD5 hash if file logging is enabled
		if (enableFileLogging && string.IsNullOrEmpty(logFilePath))
		{
			try
			{
				logFilePath = FileLoggingHelper.GenerateLogFilePath(path);
			}
			catch (IOException ex)
			{
				Console.WriteLine($"Warning: Could not generate log file path for {path}: {ex.Message}");
				enableFileLogging = false;
			}
			catch (UnauthorizedAccessException ex)
			{
				Console.WriteLine($"Warning: Could not generate log file path for {path}: {ex.Message}");
				enableFileLogging = false;
			}
			catch (ArgumentException ex)
			{
				Console.WriteLine($"Warning: Could not generate log file path for {path}: {ex.Message}");
				enableFileLogging = false;
			}
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
				
				// Add file logging if enabled
				if (enableFileLogging && !string.IsNullOrEmpty(logFilePath))
				{
					builder.AddFileLogging(logFilePath);
				}
			});
			shouldDisposeLoggerFactory = true;
		}

		try
		{
			var logger = loggerFactory.CreateLogger<Emulator>();

			// Log file path if file logging is enabled
			if (enableFileLogging && !string.IsNullOrEmpty(logFilePath))
			{
				logger.LogInformation("Logging to file: {LogFilePath}", logFilePath);
			}

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
				// Create or get virtual disk for the executable
				string virtualDiskPath;
				string vfsExecutablePath;
				
				try
				{
					(virtualDiskPath, vfsExecutablePath) = CreateOrGetVirtualDisk(path, logger);
					logger.LogInformation("Using virtual disk: {VirtualDiskPath}", virtualDiskPath);
					logger.LogInformation("Executable VFS path: {VfsPath}", vfsExecutablePath);
				}
				catch (System.IO.IOException ex)
				{
					logger.LogError(ex, "IO error while creating or accessing virtual disk for {Path}", path);
					return 1;
				}
				catch (System.UnauthorizedAccessException ex)
				{
					logger.LogError(ex, "Access denied while creating or accessing virtual disk for {Path}", path);
					return 1;
				}
				catch (Exception ex) when (
					ex.GetType() != typeof(System.StackOverflowException) &&
					ex.GetType() != typeof(System.OutOfMemoryException) &&
					ex.GetType() != typeof(System.Threading.ThreadAbortException)
				)
				{
					logger.LogError(ex, "Unexpected error while creating or accessing virtual disk for {Path}", path);
					return 1;
				}
				// Let critical exceptions propagate
				
				using var emulator = new Emulator(null, logger, telemetryService);
				emulator.LoadExecutable(vfsExecutablePath, null, debugMode, interactiveDebugMode, 256, gdbServerMode, gdbServerPort, false, false, false, virtualDiskPath);
				
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
		Console.WriteLine("  --log-file [path]    Enable logging to file (auto-generates MD5-based filename if path not provided)");
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
		Console.WriteLine("  Win32Emu game.exe --log-file");
		Console.WriteLine("  Win32Emu game.exe --log-file custom.log");
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

	/// <summary>
	/// Creates or gets a virtual disk for the specified executable.
	/// The virtual disk contains the executable and all files from its directory.
	/// </summary>
	/// <param name="executablePath">Path to the executable</param>
	/// <param name="logger">Logger instance</param>
	/// <returns>Tuple of (virtualDiskPath, vfsExecutablePath)</returns>
	private static (string virtualDiskPath, string vfsExecutablePath) CreateOrGetVirtualDisk(string executablePath, ILogger logger)
	{
		// Get the directory and filename
		var fullPath = Path.GetFullPath(executablePath);
		var directory = Path.GetDirectoryName(fullPath);
		var filename = Path.GetFileName(fullPath);
		
		if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(filename))
		{
			throw new ArgumentException($"Invalid executable path: {executablePath}");
		}

		// Create a virtual disk path based on the executable name
		var vhdDirectory = Path.Combine(Path.GetTempPath(), "Win32Emu_VHDs");
		Directory.CreateDirectory(vhdDirectory);
		
		// Use a hash of the directory path to create a unique VHD name
		var directoryHash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(directory));
		var hashString = Convert.ToHexString(directoryHash)[..16].ToLowerInvariant();
		var vhdPath = Path.Combine(vhdDirectory, $"{Path.GetFileNameWithoutExtension(filename)}_{hashString}.vhd");

		// Determine the virtual directory name (use last component of the directory path)
		var virtualDirName = Path.GetFileName(directory);
		if (string.IsNullOrEmpty(virtualDirName))
		{
			// Fallback to using the filename without extension as the directory name
			virtualDirName = Path.GetFileNameWithoutExtension(filename);
		}
		var vfsExecutablePath = $"C:\\{virtualDirName}\\{filename}";

		// Check if VHD already exists
		if (File.Exists(vhdPath))
		{
			logger.LogInformation("Using existing virtual disk: {VhdPath}", vhdPath);
			return (vhdPath, vfsExecutablePath);
		}

		// Create new VHD and populate it with files from the directory
		logger.LogInformation("Creating virtual disk: {VhdPath}", vhdPath);
		
		const long vhdSizeBytes = 512L * 1024 * 1024; // 512 MB
		
		int successCount = 0;
		int failureCount = 0;
		int totalFiles = 0;
		
		using (var vfs = VirtualFileSystem.DiskVirtualFileSystem.Create(vhdPath, VirtualFileSystem.DiskFormat.Vhd, vhdSizeBytes, logger))
		{
			// Create the root directory for the virtual disk
			try
			{
				vfs.CreateDirectory($"\\{virtualDirName}");
				logger.LogInformation("Created root directory: \\{VirtualDirName}", virtualDirName);
			}
			catch (Exception ex)
			{
				logger.LogWarning(ex, "Failed to create root directory (may already exist): \\{VirtualDirName}", virtualDirName);
			}
			
			// Copy all files from the directory to the VHD
			var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
			totalFiles = files.Length;
			logger.LogInformation("[VHD Creation] Starting copy of {FileCount} files to virtual disk", totalFiles);
			
			foreach (var file in files)
			{
				var relativePath = Path.GetRelativePath(directory, file);
				var vfsPath = $"\\{virtualDirName}\\{relativePath.Replace('/', '\\')}";
				
				try
				{
					// Ensure all parent directories exist in VFS
					// On Linux, Path.GetDirectoryName doesn't recognize backslashes, so we need to extract manually
					var lastBackslash = vfsPath.LastIndexOf('\\');
					string? vfsDir = null;
					if (lastBackslash >= 0)
					{
						vfsDir = vfsPath.Substring(0, lastBackslash);
					}
					
					if (!string.IsNullOrEmpty(vfsDir))
					{
						// Create all parent directories recursively
						EnsureDirectoryExists(vfs, vfsDir, logger);
					}
					
					// Copy file to VFS
					var fileName = Path.GetFileName(file);
					logger.LogInformation("[VHD Creation] Copying file {FilesCopied}/{TotalFiles}: {FileName}", successCount + 1, totalFiles, fileName);
					var fileBytes = File.ReadAllBytes(file);
					var handle = vfs.OpenFile(vfsPath, VirtualFileSystem.VfsFileMode.Create, VirtualFileSystem.VfsFileAccess.Write);
					if (handle != null)
					{
						using (handle)
						{
							handle.Write(fileBytes, 0, fileBytes.Length);
						}
						logger.LogInformation("[VHD Creation] Successfully copied {FileName} ({Size} bytes) [{FilesCopied}/{TotalFiles}]", 
							fileName, fileBytes.Length, successCount + 1, totalFiles);
						successCount++;
					}
					else
					{
						logger.LogWarning("[VHD Creation] Failed to open file for writing: {VfsPath}", vfsPath);
						failureCount++;
					}
				}
				catch (Exception ex)
				{
					logger.LogWarning(ex, "[VHD Creation] Failed to copy file to VHD: {File} -> {VfsPath}", file, vfsPath);
					failureCount++;
				}
			}
			
			if (failureCount > 0)
			{
				logger.LogWarning("[VHD Creation] Virtual disk created with {FailureCount} file copy failures out of {TotalCount} files", failureCount, totalFiles);
			}
		}

		if (failureCount == totalFiles && totalFiles > 0)
		{
			throw new InvalidOperationException($"Failed to copy any files to virtual disk. All {totalFiles} file operations failed.");
		}

		logger.LogInformation("[VHD Creation] Virtual disk created successfully: {VhdPath} ({SuccessCount}/{TotalCount} files copied)", vhdPath, successCount, totalFiles);
		return (vhdPath, vfsExecutablePath);
	}

	/// <summary>
	/// Ensures all parent directories exist for the given path
	/// </summary>
	private static void EnsureDirectoryExists(VirtualFileSystem.DiskVirtualFileSystem vfs, string directoryPath, ILogger logger)
	{
		if (string.IsNullOrEmpty(directoryPath) || directoryPath == "\\" || directoryPath == "/")
		{
			return;
		}

		// Split path into components
		var parts = directoryPath.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
		var currentPath = "";
		
		foreach (var part in parts)
		{
			currentPath += "\\" + part;
			
			try
			{
				vfs.CreateDirectory(currentPath);
				logger.LogDebug("Created directory: {Directory}", currentPath);
			}
			catch (Exception ex)
			{
				logger.LogDebug(ex, "CreateDirectory failed for {Directory} (may already exist)", currentPath);
			}
		}
	}
}
