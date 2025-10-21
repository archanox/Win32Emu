using Microsoft.Extensions.Logging;

namespace Win32Emu.Rendering;

/// <summary>
/// Factory for creating rendering, audio, and input backends
/// </summary>
public static class BackendFactory
{
    private static BackendType? _currentBackendType;

    /// <summary>
    /// Current backend type setting
    /// Priority: 1. Explicitly set value, 2. WIN32EMU_BACKEND environment variable, 3. Default to SDL
    /// </summary>
    public static BackendType CurrentBackendType
    {
        get
        {
            if (_currentBackendType.HasValue)
            {
                return _currentBackendType.Value;
            }

            // Check environment variable
            var envBackend = Environment.GetEnvironmentVariable("WIN32EMU_BACKEND");
            if (!string.IsNullOrEmpty(envBackend))
            {
                if (Enum.TryParse<BackendType>(envBackend, ignoreCase: true, out var backendType))
                {
                    return backendType;
                }
            }

            // Default to SDL (Metal on macOS, Vulkan on Linux, DirectX 12 on Windows)
            return BackendType.SDL;
        }
        set => _currentBackendType = value;
    }

    /// <summary>
    /// Create a rendering backend instance
    /// </summary>
    public static IRenderingBackend CreateRenderingBackend(ILogger logger)
    {
        return CurrentBackendType switch
        {
            BackendType.SDL => new Sdl3RenderingBackend(logger),
            BackendType.GLFW => new SilkGlfwRenderingBackend(logger),
            BackendType.Vulkan => new SilkVulkanRenderingBackend(logger),
            _ => new Sdl3RenderingBackend(logger)
        };
    }

    /// <summary>
    /// Create an audio backend instance
    /// </summary>
    public static IAudioBackend CreateAudioBackend(ILogger logger)
    {
        // Use SDL3 audio when SDL backend is selected, otherwise use OpenAL
        return CurrentBackendType switch
        {
            BackendType.SDL => new Sdl3AudioBackend(logger),
            _ => new SilkOpenAlAudioBackend(logger)
        };
    }

    /// <summary>
    /// Create an input backend instance
    /// </summary>
    public static IInputBackend CreateInputBackend(ILogger logger)
    {
        // Use SDL3 input when SDL backend is selected, otherwise use Silk.NET
        return CurrentBackendType switch
        {
            BackendType.SDL => new Sdl3InputBackend(logger),
            _ => new SilkInputBackend(logger)
        };
    }
}
