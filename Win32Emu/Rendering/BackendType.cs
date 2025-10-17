namespace Win32Emu.Rendering;

/// <summary>
/// Enumeration of available backend types
/// </summary>
public enum BackendType
{
    /// <summary>
    /// SDL backend using Silk.NET.SDL
    /// </summary>
    SDL,

    /// <summary>
    /// GLFW backend using Silk.NET.GLFW
    /// </summary>
    GLFW,

    /// <summary>
    /// Vulkan backend using Silk.NET.Vulkan (MoltenVK on macOS)
    /// </summary>
    Vulkan
}
