using SDL3;

namespace Win32Emu.Rendering;

/// <summary>
/// Helper class to ensure SDL3 is properly initialized with app metadata before any subsystem initialization.
/// On macOS, SetAppMetadata must be called before ANY SDL.Init() call to avoid "No available video device" errors.
/// </summary>
internal static class Sdl3Initializer
{
    private static readonly object _lock = new();
    private static bool _metadataSet = false;

    /// <summary>
    /// Ensures SDL3 app metadata is set. This must be called before any SDL.Init() calls.
    /// This is a no-op after the first call, making it safe to call multiple times.
    /// </summary>
    public static void EnsureAppMetadataSet()
    {
        lock (_lock)
        {
            if (_metadataSet)
            {
                return;
            }

            // Set app metadata before any SDL initialization
            // This is critical for macOS to properly initialize video/Metal support
            SDL.SetAppMetadata("Win32Emu", "1.0", "com.win32emu.display");
            _metadataSet = true;
        }
    }
}
