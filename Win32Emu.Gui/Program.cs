using Avalonia;
using Avalonia.Logging;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using Win32Emu.Logging;

namespace Win32Emu.Gui;

sealed class Program
{
    /// <summary>
    /// Module initializer that runs before any other code in the assembly.
    /// Used to configure headless mode BEFORE SDL native library is loaded.
    /// NOTE: This is a best-effort approach. SDL may still load its native library before this runs.
    /// For reliable headless operation, use the run-headless.sh launcher script or set SDL_VIDEODRIVER
    /// before starting the process.
    /// </summary>
    [ModuleInitializer]
    internal static void ModuleInit()
    {
        // Configure headless mode at module load time
        // This is a defensive measure that may not always work due to SDL native library load timing
        ConfigureHeadlessMode();
    }

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

    /// <summary>
    /// Configures headless mode by detecting headless environment and setting SDL_VIDEODRIVER=dummy.
    /// This is a defense-in-depth measure that may not always work due to SDL native library load timing.
    /// For reliable headless operation, users should use run-headless.sh or set the environment variable
    /// before starting the process.
    /// </summary>
    private static void ConfigureHeadlessMode()
    {
        // Check if we're in a headless environment (no DISPLAY variable on Linux/Unix)
        // Also respect user-set SDL_VIDEODRIVER (don't override if already set)
        var display = Environment.GetEnvironmentVariable("DISPLAY");
        var sdlDriver = Environment.GetEnvironmentVariable("SDL_VIDEODRIVER");
        
        var isHeadless = string.IsNullOrEmpty(display) &&
                         string.IsNullOrEmpty(sdlDriver) &&
                         !OperatingSystem.IsWindows() && !OperatingSystem.IsMacOS();
        
        if (isHeadless)
        {
            // Set dummy video driver for headless operation (best-effort)
            Environment.SetEnvironmentVariable("SDL_VIDEODRIVER", "dummy");
            // NOTE: Console.WriteLine is used here because this runs in a ModuleInitializer,
            // before any ILogger instances are available. This is an acceptable exception to
            // the project's logging guidelines for early diagnostic output.
            Console.WriteLine("[Win32Emu] Headless environment detected - configured SDL dummy video driver");
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace(level: LogEventLevel.Debug);
}
