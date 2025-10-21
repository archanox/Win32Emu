using SDL3;

namespace Win32Emu.Rendering;

/// <summary>
/// Helper class to ensure SDL app metadata is set before any SDL initialization.
/// This is critical on macOS where SetAppMetadata must be called before ANY SDL.Init() call.
/// </summary>
internal static class Sdl3Initializer
{
    private static bool _appMetadataSet;
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

            SDL.SetAppMetadata("Win32Emu", "1.0.0", "com.archanox.win32emu");
            _appMetadataSet = true;
        }
    }
}
