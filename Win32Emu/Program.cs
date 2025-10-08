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
				Console.WriteLine();
				Console.WriteLine("Examples:");
				Console.WriteLine("  Win32Emu game.exe");
				Console.WriteLine("  Win32Emu game.exe --debug");
				Console.WriteLine("  Win32Emu game.exe --interactive-debug");
				return;
			}

			// Parse command line arguments
			var debugMode = args.Contains("--debug");
			var interactiveDebugMode = args.Contains("--interactive-debug");
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
				emulator.LoadExecutable(path, debugMode, interactiveDebugMode);
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