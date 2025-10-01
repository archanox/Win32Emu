using Win32Emu;

namespace Win32Emu.ConsoleApp
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				System.Console.WriteLine("Usage: Win32Emu <path-to-pe> [--debug]");
				System.Console.WriteLine("  --debug: Enable enhanced debugging to catch 0xFFFFFFFD memory access errors");
				return;
			}

			// Parse command line arguments
			var debugMode = args.Contains("--debug");
			var path = args[0];

			try
			{
				var emulator = new Emulator();
				emulator.LoadExecutable(path, debugMode);
				emulator.Run();
			}
			catch (FileNotFoundException ex)
			{
				System.Console.WriteLine($"Error: {ex.Message}");
			}
			catch (Exception ex)
			{
				System.Console.WriteLine($"Emulator error: {ex.Message}");
				if (debugMode)
				{
					System.Console.WriteLine($"Stack trace: {ex.StackTrace}");
				}
			}
		}
	}
}