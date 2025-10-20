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
    /// Priority: 1. Explicitly set value, 2. WIN32EMU_BACKEND environment variable, 3. Default to GLFW
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

            // Default to GLFW
            return BackendType.GLFW;
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
            BackendType.GLFW => new SilkGlfwRenderingBackend(logger),
            BackendType.Vulkan => new SilkVulkanRenderingBackend(logger),
            _ => new SilkGlfwRenderingBackend(logger)
        };
    }

    /// <summary>
    /// Create an audio backend instance
    /// </summary>
    public static IAudioBackend CreateAudioBackend(ILogger logger)
    {
        // Always use OpenAL for audio
        return new SilkOpenAlAudioBackend(logger);
    }

    /// <summary>
    /// Create an input backend instance
    /// </summary>
    public static IInputBackend CreateInputBackend(ILogger logger)
    {
        // Use Silk.NET input abstraction
        return new SilkInputBackend(logger);
    }
}
