using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Win32Emu.Rendering;

namespace Win32Emu.Wasm.Backend;

/// <summary>
/// WASM-compatible backend factory that creates browser-based rendering, audio, and input backends
/// </summary>
public class WasmBackendFactory : IBackendFactory
{
	private readonly IJSRuntime _jsRuntime;
	private readonly ILoggerFactory _loggerFactory;

	public WasmBackendFactory(IJSRuntime jsRuntime, ILoggerFactory loggerFactory)
	{
		_jsRuntime = jsRuntime;
		_loggerFactory = loggerFactory;
	}

	/// <summary>
	/// Creates a WASM rendering backend.
	/// Note: The logger parameter is intentionally ignored because WASM backends require
	/// strongly-typed ILogger&lt;T&gt; instances for proper category names in browser console.
	/// </summary>
	public IRenderingBackend CreateRenderingBackend(ILogger logger)
	{
		return new WasmRenderingBackend(_jsRuntime, _loggerFactory.CreateLogger<WasmRenderingBackend>());
	}

	public IRenderingBackend CreateRenderingBackendWithHost(ILogger logger, IEmulatorHost? host)
	{
		// In WASM, we ignore the host parameter since rendering goes through JS interop
		return CreateRenderingBackend(logger);
	}

	/// <summary>
	/// Creates a WASM audio backend.
	/// Note: The logger parameter is intentionally ignored because WASM backends require
	/// strongly-typed ILogger&lt;T&gt; instances for proper category names in browser console.
	/// </summary>
	public IAudioBackend CreateAudioBackend(ILogger logger)
	{
		return new WasmAudioBackend(_jsRuntime, _loggerFactory.CreateLogger<WasmAudioBackend>());
	}

	/// <summary>
	/// Creates a WASM input backend.
	/// Note: The logger parameter is intentionally ignored because WASM backends require
	/// strongly-typed ILogger&lt;T&gt; instances for proper category names in browser console.
	/// </summary>
	public IInputBackend CreateInputBackend(ILogger logger)
	{
		return new WasmInputBackend(_jsRuntime, _loggerFactory.CreateLogger<WasmInputBackend>());
	}
}
