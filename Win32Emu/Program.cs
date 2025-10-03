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

			try
			{
				using var emulator = new Emulator();
				emulator.LoadExecutable(path, debugMode);
				emulator.Run();
			}
			catch (FileNotFoundException ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Emulator error: {ex.Message}");
				if (debugMode)
				{
					Console.WriteLine($"Stack trace: {ex.StackTrace}");
				}
			}
		}
	}
}