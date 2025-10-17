using Microsoft.Extensions.Logging;

namespace Win32Emu.Rendering;

/// <summary>
/// Factory for creating rendering, audio, and input backends
/// </summary>
public static class BackendFactory
{
    /// <summary>
    /// Current backend type setting (defaults to SDL)
    /// </summary>
    public static BackendType CurrentBackendType { get; set; } = BackendType.SDL;

    /// <summary>
    /// Create a rendering backend instance
    /// </summary>
    public static IRenderingBackend CreateRenderingBackend(ILogger logger)
    {
        return CurrentBackendType switch
        {
            BackendType.SDL => new SilkSdlRenderingBackend(logger),
            BackendType.GLFW => new SilkGlfwRenderingBackend(logger),
            _ => new SilkSdlRenderingBackend(logger)
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
