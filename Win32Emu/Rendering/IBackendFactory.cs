using Microsoft.Extensions.Logging;

namespace Win32Emu.Rendering;

/// <summary>
/// Factory interface for creating rendering, audio, and input backends
/// </summary>
public interface IBackendFactory
{
	/// <summary>
	/// Create a rendering backend instance
	/// </summary>
	IRenderingBackend CreateRenderingBackend(ILogger logger);

	/// <summary>
	/// Create a rendering backend with automatic host integration.
	/// If a host is provided, creates an integrated backend for GUI.
	/// Otherwise, creates a platform-specific backend.
	/// </summary>
	/// <param name="logger">Logger instance</param>
	/// <param name="host">Optional emulator host for GUI integration</param>
	/// <returns>Rendering backend instance</returns>
	IRenderingBackend CreateRenderingBackendWithHost(ILogger logger, IEmulatorHost? host);

	/// <summary>
	/// Create an audio backend instance
	/// </summary>
	IAudioBackend CreateAudioBackend(ILogger logger);

	/// <summary>
	/// Create an input backend instance
	/// </summary>
	IInputBackend CreateInputBackend(ILogger logger);
}
