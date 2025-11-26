using Microsoft.Extensions.Logging;
using Win32Emu.Rendering;

namespace Win32Emu.Gui.Backends;

/// <summary>
/// Factory for creating rendering, audio, and input backends
/// </summary>
public class BackendFactory : IBackendFactory
{
	private BackendType? _currentBackendType;

	/// <summary>
	/// Current backend type setting
	/// Priority: 1. Explicitly set value, 2. WIN32EMU_BACKEND environment variable, 3. Default to SDL
	/// </summary>
	public BackendType CurrentBackendType
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
	public IRenderingBackend CreateRenderingBackend(ILogger logger)
	{
		return CurrentBackendType switch
		{
			BackendType.SDL => new Sdl3RenderingBackend(logger),
			BackendType.GLFW => new SilkGlfwRenderingBackend(logger),
			BackendType.Vulkan => new SilkVulkanRenderingBackend(logger),
			BackendType.Metal => new SharpMetalRenderingBackend(logger),
			BackendType.Software => new SoftwareRenderingBackend(logger),
			_ => new Sdl3RenderingBackend(logger)
		};
	}

	/// <summary>
	/// Create a rendering backend with automatic Avalonia integration.
	/// If a host is provided, creates an AvaloniaRenderingBackend for GUI integration.
	/// Otherwise, creates a platform-specific backend based on CurrentBackendType.
	/// </summary>
	/// <param name="logger">Logger instance</param>
	/// <param name="host">Optional emulator host for GUI integration</param>
	/// <returns>Rendering backend instance</returns>
	public IRenderingBackend CreateRenderingBackendWithHost(ILogger logger, IEmulatorHost? host)
	{
		// Use Avalonia backend if host is available for GUI integration
		if (host != null)
		{
			return new AvaloniaRenderingBackend(logger, host);
		}
		
		// Otherwise use default platform-specific backend
		return CreateRenderingBackend(logger);
	}

	/// <summary>
	/// Create an audio backend instance
	/// </summary>
	public IAudioBackend CreateAudioBackend(ILogger logger)
	{
		// Software backend uses null audio (no audio output)
		if (CurrentBackendType == BackendType.Software)
		{
			return new NullAudioBackend(logger);
		}

		// Try to create appropriate audio backend, fall back to null audio on failure
		try
		{
			return CurrentBackendType switch
			{
				BackendType.SDL => new Sdl3AudioBackend(logger),
				_ => new SilkOpenAlAudioBackend(logger)
			};
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "[BackendFactory] Failed to create audio backend, falling back to null audio");
			return new NullAudioBackend(logger);
		}
	}

	/// <summary>
	/// Create an input backend instance
	/// </summary>
	public IInputBackend CreateInputBackend(ILogger logger)
	{
		// Use SDL3 input when SDL backend is selected, otherwise use Silk.NET
		return CurrentBackendType switch
		{
			BackendType.SDL => new Sdl3InputBackend(logger),
			_ => new SilkInputBackend(logger)
		};
	}
}
