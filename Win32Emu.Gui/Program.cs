using Avalonia;
using Avalonia.Logging;
using Microsoft.Extensions.Logging;
using Win32Emu.Logging;

namespace Win32Emu.Gui;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static int Main(string[] args)
    {
        // Check for --nogui flag to run in CLI mode
        if (args.Contains("--nogui"))
        {
            // Remove --nogui from args and pass the rest to the emulator launcher
            var emuArgs = args.Where(arg => arg != "--nogui").ToArray();
            
            // Create a logger factory for CLI mode
            // This ensures consistent logging between GUI and CLI modes
            var enableDebugMode = args.Contains("--debug");
            var enableFileLogging = args.Contains("--log-file");
            
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddConsole()
                    .SetMinimumLevel(enableDebugMode ? LogLevel.Debug : LogLevel.Information);
                
                // Add file logging if requested
                if (enableFileLogging)
                {
                    try
                    {
                        // Find the executable path from args
                        var exePath = emuArgs.FirstOrDefault(arg => !arg.StartsWith("--"));
                        if (!string.IsNullOrEmpty(exePath))
                        {
                            var logFilePath = FileLoggingHelper.GenerateLogFilePath(exePath);
                            builder.AddFileLogging(logFilePath);
                            Console.WriteLine($"Logging to: {logFilePath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Could not enable file logging: {ex.Message}");
                    }
                }
            });
            
            // Run the emulator directly on the main thread with the logger factory (no GUI)
            return EmulatorLauncher.Launch(emuArgs, loggerFactory);
        }
        
        // Otherwise, start the Avalonia GUI application
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        return 0;
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace(level: LogEventLevel.Debug);
}
