using Microsoft.Extensions.Logging;

namespace Win32Emu
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: Win32Emu <path-to-pe> [--debug]");
				Console.WriteLine("  --debug: Enable enhanced debugging to catch 0xFFFFFFFD memory access errors");
				return;
			}

			// Parse command line arguments
			var debugMode = args.Contains("--debug");
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
				emulator.LoadExecutable(path, debugMode);
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