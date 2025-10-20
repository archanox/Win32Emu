namespace Win32Emu.Rendering;

/// <summary>
/// Enumeration of available backend types
/// </summary>
public enum BackendType
{
    /// <summary>
    /// GLFW backend using Silk.NET.GLFW
    /// </summary>
    GLFW,

    /// <summary>
    /// Vulkan backend using Silk.NET.Vulkan (MoltenVK on macOS)
    /// </summary>
    Vulkan
}
