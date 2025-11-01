namespace Win32Emu.Rendering;

/// <summary>
/// Enumeration of available backend types
/// </summary>
public enum BackendType
{
    /// <summary>
    /// SDL3 backend using SDL3-CS (Metal on macOS, Vulkan on Linux, DirectX 12 on Windows)
    /// </summary>
    SDL,

    /// <summary>
    /// GLFW backend using Silk.NET.GLFW
    /// </summary>
    GLFW,

    /// <summary>
    /// Vulkan backend using Silk.NET.Vulkan (MoltenVK on macOS)
    /// </summary>
    Vulkan,

    /// <summary>
    /// Metal backend using SharpMetal (macOS only)
    /// </summary>
    Metal,

    /// <summary>
    /// Software (CPU-based) rendering backend - no GPU acceleration required
    /// </summary>
    Software,

    /// <summary>
    /// Avalonia UI integration backend - renders to Avalonia WriteableBitmap instead of separate window
    /// </summary>
    Avalonia
}
