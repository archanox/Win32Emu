using SDL3;

namespace Win32Emu.Gui.Backends;
using Win32Emu.Rendering;
/// <summary>
/// Helper class to ensure SDL app metadata is set before any SDL initialization.
/// This is critical on macOS where SetAppMetadata must be called before ANY SDL.Init() call.
/// </summary>
internal static class Sdl3Initializer
{
    private static bool _appMetadataSet;
    private static bool _headlessModeConfigured;
    private static readonly object _lock = new();

    /// <summary>
    /// Ensures SDL app metadata is set exactly once, before any SDL initialization.
    /// Must be called before any SDL.Init() call on all platforms, especially macOS.
    /// </summary>
    public static void EnsureAppMetadataSet()
    {
        lock (_lock)
        {
            if (_appMetadataSet)
            {
                return;
            }

            // Check for headless environment and configure SDL BEFORE SetAppMetadata
            // This is critical because SetAppMetadata may trigger SDL initialization internally
            EnsureHeadlessModeConfigured();

            SDL.SetAppMetadata("Win32Emu", "1.0.0", "com.archanox.win32emu");
            _appMetadataSet = true;
        }
    }

    /// <summary>
    /// Detects headless environment and sets SDL_VIDEODRIVER=dummy if needed.
    /// This is a defensive/best-effort measure. SDL may read the environment variable during
    /// native library load, which can happen before this code runs. For reliable headless
    /// operation, users should use run-headless.sh or set SDL_VIDEODRIVER before process start.
    /// </summary>
    private static void EnsureHeadlessModeConfigured()
    {
        if (_headlessModeConfigured)
        {
            return;
        }

        // Check if we're in a headless environment (no DISPLAY variable on Linux/Unix)
        // Also check if SDL_VIDEODRIVER is already set (user preference takes priority)
        var display = Environment.GetEnvironmentVariable("DISPLAY");
        var sdlDriver = Environment.GetEnvironmentVariable("SDL_VIDEODRIVER");
        var isWindows = OperatingSystem.IsWindows();
        var isMacOS = OperatingSystem.IsMacOS();
        
        var isHeadless = string.IsNullOrEmpty(display) &&
                         string.IsNullOrEmpty(sdlDriver) &&
                         !isWindows && !isMacOS;

        if (isHeadless)
        {
            // Set dummy video driver for headless operation (best-effort)
            // NOTE: Headless mode configuration happens silently here.
            // Logging is not performed because this static class does not have access to an ILogger instance.
            // The Program.cs ModuleInitializer provides console output for diagnostic purposes.
            Environment.SetEnvironmentVariable("SDL_VIDEODRIVER", "dummy");
        }

        _headlessModeConfigured = true;
    }
}
