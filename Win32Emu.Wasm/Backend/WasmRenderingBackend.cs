using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using System.Numerics;
using Win32Emu.Rendering;

namespace Win32Emu.Wasm.Backend;

/// <summary>
/// WASM-compatible rendering backend using HTML5 Canvas and JavaScript interop.
/// </summary>
/// <remarks>
/// <para>
/// <b>WASM-Specific Behavior:</b> Due to WebAssembly's single-threaded nature, blocking calls
/// like <c>.Wait()</c> or <c>.Result</c> on async operations are not supported and will throw
/// <see cref="PlatformNotSupportedException"/>.
/// </para>
/// <para>
/// Use <see cref="InitializeAsync"/> for proper async initialization.
/// </para>
/// </remarks>
public class WasmRenderingBackend : IRenderingBackend
{
	private const int BytesPerPixelRgba = 4; // RGBA format
	
	private readonly IJSRuntime _jsRuntime;
	private readonly ILogger<WasmRenderingBackend> _logger;
	private bool _initialized;
	private int _width;
	private int _height;
	private string _canvasId = "emulatorCanvas";
	private byte[]? _frameBuffer;
	private volatile bool _renderingErrorOccurred = false;
	
	public event EventHandler<UIEventArgs>? UIEvent;
	
	public bool IsInitialized => _initialized;
	public int Width => _width;
	public int Height => _height;

	public WasmRenderingBackend(IJSRuntime jsRuntime, ILogger<WasmRenderingBackend> logger)
	{
		_jsRuntime = jsRuntime;
		_logger = logger;
	}

	/// <summary>
	/// Async initialization that properly awaits the JavaScript call.
	/// </summary>
	public async Task<bool> InitializeAsync(int width, int height, string title = "Win32Emu Display")
	{
		if (_initialized)
		{
			_logger.LogInformation("[WASM] InitializeAsync called but already initialized");
			return true;
		}

		try
		{
			_width = width;
			_height = height;
			_frameBuffer = new byte[width * height * BytesPerPixelRgba]; // RGBA format
			
			_logger.LogInformation("[WASM] InitializeAsync starting: ({Width}x{Height})", width, height);
			_logger.LogInformation("[WASM] Calling JavaScript initializeEmulator with canvasId: {CanvasId}", _canvasId);
			
			await _jsRuntime.InvokeVoidAsync("initializeEmulator", _canvasId);
			
			_initialized = true;
			_logger.LogInformation("[WASM] Rendering backend initialized successfully - canvas ready for updates");
			_logger.LogInformation("[WASM] Frame buffer allocated: {Size} bytes ({Width}x{Height}x{BytesPerPixel})", 
				_frameBuffer.Length, width, height, BytesPerPixelRgba);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[WASM] Failed to initialize rendering backend");
			return false;
		}
	}

	public byte[] ConvertPalettizedToRGBA(byte[] indexedData, uint[] palette, int width, int height, int pitch)
	{
		var rgbaData = new byte[width * height * 4];
		
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				var srcOffset = y * pitch + x;
				var dstOffset = (y * width + x) * 4;
				
				if (srcOffset < indexedData.Length)
				{
					var paletteIndex = indexedData[srcOffset];
					if (paletteIndex < palette.Length)
					{
						var color = palette[paletteIndex];
						rgbaData[dstOffset + 0] = (byte)((color >> 16) & 0xFF); // R
						rgbaData[dstOffset + 1] = (byte)((color >> 8) & 0xFF);  // G
						rgbaData[dstOffset + 2] = (byte)(color & 0xFF);         // B
						rgbaData[dstOffset + 3] = (byte)((color >> 24) & 0xFF); // A
					}
				}
			}
		}
		
		return rgbaData;
	}

	public byte[] Convert16BitToRGBA(byte[] rgb565Data, int width, int height, int pitch)
	{
		var rgbaData = new byte[width * height * 4];
		
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				var srcOffset = y * pitch + x * 2;
				var dstOffset = (y * width + x) * 4;
				
				if (srcOffset + 1 < rgb565Data.Length)
				{
					ushort pixel = (ushort)(rgb565Data[srcOffset] | (rgb565Data[srcOffset + 1] << 8));
					
					rgbaData[dstOffset + 0] = (byte)(((pixel >> 11) & 0x1F) << 3); // R
					rgbaData[dstOffset + 1] = (byte)(((pixel >> 5) & 0x3F) << 2);  // G
					rgbaData[dstOffset + 2] = (byte)((pixel & 0x1F) << 3);         // B
					rgbaData[dstOffset + 3] = 255;                                  // A
				}
			}
		}
		
		return rgbaData;
	}

	public byte[] Convert24BitToRGBA(byte[] rgb24Data, int width, int height, int pitch)
	{
		var rgbaData = new byte[width * height * 4];
		
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				var srcOffset = y * pitch + x * 3;
				var dstOffset = (y * width + x) * 4;
				
				if (srcOffset + 2 < rgb24Data.Length)
				{
					rgbaData[dstOffset + 0] = rgb24Data[srcOffset + 2]; // R (BGR to RGB)
					rgbaData[dstOffset + 1] = rgb24Data[srcOffset + 1]; // G
					rgbaData[dstOffset + 2] = rgb24Data[srcOffset + 0]; // B
					rgbaData[dstOffset + 3] = 255;                       // A
				}
			}
		}
		
		return rgbaData;
	}

	public bool UpdateFrameBuffer(byte[] data, int pitch)
	{
		// Stop rendering if a previous error occurred
		if (_renderingErrorOccurred)
		{
			_logger.LogError("[WASM] Rendering stopped due to previous error. Emulator execution halted.");
			return false;
		}

		if (!_initialized || _frameBuffer == null)
		{
			_logger.LogWarning("[WASM] UpdateFrameBuffer called but backend not initialized (_initialized={Initialized}, _frameBuffer={FrameBufferNull})", 
				_initialized, _frameBuffer == null ? "null" : "not null");
			return false;
		}

		try
		{
			_logger.LogDebug("[WASM] UpdateFrameBuffer called: width={Width}, height={Height}, pitch={Pitch}, dataLength={DataLength}", 
				_width, _height, pitch, data.Length);
			
			// Copy data to internal frame buffer
			if (pitch == _width * BytesPerPixelRgba)
			{
				// Direct copy if pitch matches
				Array.Copy(data, _frameBuffer, Math.Min(data.Length, _frameBuffer.Length));
			}
			else
			{
				// Line-by-line copy if pitch doesn't match
				for (int y = 0; y < _height; y++)
				{
					var srcOffset = y * pitch;
					var dstOffset = y * _width * BytesPerPixelRgba;
					var lineLength = Math.Min(_width * BytesPerPixelRgba, pitch);
					
					if (srcOffset + lineLength <= data.Length && dstOffset + lineLength <= _frameBuffer.Length)
					{
						Array.Copy(data, srcOffset, _frameBuffer, dstOffset, lineLength);
					}
				}
			}
			
			// Update canvas through JavaScript with better error handling
			var base64Data = Convert.ToBase64String(_frameBuffer);
			
			// Log the call for debugging
			_logger.LogDebug("[WASM] Calling updateCanvasWithErrorHandling: canvasId={CanvasId}, width={Width}, height={Height}, base64Length={Base64Length}",
				_canvasId, _width, _height, base64Data.Length);
			
			// Use fire-and-forget pattern but with proper error tracking
			// We return success immediately for performance, but track errors for future calls
			_jsRuntime.InvokeVoidAsync("updateCanvasWithErrorHandling", _canvasId, base64Data, _width, _height)
				.AsTask()
				.ContinueWith(t =>
				{
					if (t.IsFaulted)
					{
						_renderingErrorOccurred = true;
						var exception = t.Exception?.GetBaseException();
						_logger.LogCritical(exception, "[WASM] CRITICAL ERROR in updateCanvas - Stopping emulator execution!");
						_logger.LogError("[WASM] Error details: {Message}", exception?.Message);
						
						// Try to notify JavaScript to stop the emulator
						try
						{
							_jsRuntime.InvokeVoidAsync("stopEmulatorOnError", exception?.Message ?? "Unknown error");
						}
						catch (Exception notifyEx)
						{
							_logger.LogWarning(notifyEx, "[WASM] Failed to notify JavaScript to stop emulator after rendering error");
						}
					}
					else
					{
						_logger.LogDebug("[WASM] Canvas update completed successfully");
					}
				});
			
			// Return true immediately (fire-and-forget), errors will be caught on next call
			return true;
		}
		catch (Exception ex)
		{
			_renderingErrorOccurred = true;
			_logger.LogCritical(ex, "[WASM] CRITICAL ERROR in UpdateFrameBuffer - Stopping emulator execution!");
			return false;
		}
	}

	public void Clear(byte r, byte g, byte b, byte a = 255)
	{
		if (!_initialized || _frameBuffer == null)
		{
			return;
		}

		for (int i = 0; i < _frameBuffer.Length; i += 4)
		{
			_frameBuffer[i + 0] = r;
			_frameBuffer[i + 1] = g;
			_frameBuffer[i + 2] = b;
			_frameBuffer[i + 3] = a;
		}
	}

	public void ProcessEvents()
	{
		// Event processing handled by Blazor UI
	}

	// Hardware acceleration methods (not implemented for WASM proof-of-concept)
	
	public void BeginFrame()
	{
		// No-op for software rendering
	}

	public void EndFrame()
	{
		// No-op for software rendering
	}

	public void DrawTriangles(Span<Vertex> vertices, Span<ushort> indices)
	{
		// Not implemented for WASM proof-of-concept
		_logger.LogWarning("[WASM] DrawTriangles not implemented");
	}

	public void SetTexture(uint textureId, byte[] data, int width, int height, TextureFormat format)
	{
		// Not implemented for WASM proof-of-concept
		_logger.LogWarning("[WASM] SetTexture not implemented");
	}

	public void BindTexture(uint textureId)
	{
		// Not implemented for WASM proof-of-concept
	}

	public void SetRenderState(BlendMode blend, DepthTest depth, CullMode cull)
	{
		// Not implemented for WASM proof-of-concept
	}

	public void DeleteTexture(uint textureId)
	{
		// Not implemented for WASM proof-of-concept
	}

	public void Dispose()
	{
		if (_initialized)
		{
			_logger.LogInformation("[WASM] Disposing rendering backend");
			_initialized = false;
			_frameBuffer = null;
		}
	}
}
