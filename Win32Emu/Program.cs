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
				Console.WriteLine();
				Console.WriteLine("Examples:");
				Console.WriteLine("  Win32Emu game.exe");
				Console.WriteLine("  Win32Emu game.exe --debug");
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
			
			var path = args[0];

			// Set up logging
			using var loggerFactory = LoggerFactory.Create(builder =>
			{
				builder
					.AddConsole()
					.SetMinimumLevel(debugMode ? LogLevel.Debug : LogLevel.Information);
			});

			var logger = loggerFactory.CreateLogger<Emulator>();

			try
			{
				using var emulator = new Emulator(null, logger);
				emulator.LoadExecutable(path, debugMode, interactiveDebugMode, 256, gdbServerMode, gdbServerPort);
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
		}
	}
}