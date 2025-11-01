using Avalonia;
using Avalonia.Logging;

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
            
            // Run the emulator directly on the main thread (no GUI)
            return EmulatorLauncher.Launch(emuArgs);
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
