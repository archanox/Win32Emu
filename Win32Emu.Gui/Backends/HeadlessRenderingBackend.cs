using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Win32Emu.Gui.Backends;
using Win32Emu.Rendering;

/// <summary>
/// Headless (no display) rendering backend for running emulator without any GUI dependencies.
/// This backend performs all rendering operations in memory without requiring SDL, GLFW, or any windowing system.
/// Suitable for CI/CD environments, headless servers, and automated testing.
/// </summary>
public class HeadlessRenderingBackend : IRenderingBackend
{
	private readonly ILogger _logger;
	private int _width;
	private int _height;
	private bool _initialized;
	private readonly Lock _lock = new();
	private bool _disposed;
	private byte[]? _frameBuffer;
	private readonly string? _frameDumpPath;
	private int _frameCounter;
	private readonly bool _enableFrameDumping;

	/// <summary>
	/// Event fired when a UI event occurs (never fired in headless mode)
	/// </summary>
	public event EventHandler<UIEventArgs>? UIEvent;

	public HeadlessRenderingBackend(ILogger logger)
	{
		_logger = logger;
		
		// Check for frame dumping configuration from environment variable
		_frameDumpPath = Environment.GetEnvironmentVariable("WIN32EMU_FRAME_DUMP_PATH");
		_enableFrameDumping = !string.IsNullOrEmpty(_frameDumpPath);
		_frameCounter = 0;
		
		if (_enableFrameDumping)
		{
			_logger.LogInformation("[Headless] Frame dumping enabled. Frames will be saved to: {Path}", _frameDumpPath);
			
			// Create directory if it doesn't exist
			try
			{
				if (!Directory.Exists(_frameDumpPath!))
				{
					Directory.CreateDirectory(_frameDumpPath!);
					_logger.LogInformation("[Headless] Created frame dump directory: {Path}", _frameDumpPath);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Headless] Failed to create frame dump directory: {Path}", _frameDumpPath);
				_enableFrameDumping = false;
			}
		}
	}

	public Task<bool> InitializeAsync(int width, int height, string title = "Win32Emu Display")
	{
		lock (_lock)
		{
			if (_initialized)
			{
				return Task.FromResult(true);
			}

			_width = width;
			_height = height;

			try
			{
				_logger.LogInformation("[Headless] Initializing headless rendering backend ({Width}x{Height})...", width, height);

				// Allocate frame buffer (RGBA format)
				var bufferSize = width * height * 4; // 4 bytes per pixel (RGBA)
				_frameBuffer = new byte[bufferSize];

				_initialized = true;
				_logger.LogInformation("[Headless] Headless rendering backend initialized successfully (no display output)");
				return Task.FromResult(true);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Headless] Failed to initialize headless rendering backend");
				Cleanup();
				return Task.FromResult(false);
			}
		}
	}

	public byte[] ConvertPalettizedToRGBA(byte[] indexedData, uint[] palette, int width, int height, int pitch)
	{
		if (indexedData == null)
		{
			throw new ArgumentNullException(nameof(indexedData));
		}

		if (palette == null)
		{
			throw new ArgumentNullException(nameof(palette));
		}

		_logger.LogDebug("[Headless] Converting palettized data to RGBA: {Width}x{Height}, pitch={Pitch}", width, height, pitch);

		var rgbaData = new byte[width * height * 4];
		var rgbaIndex = 0;

		for (var y = 0; y < height; y++)
		{
			var rowOffset = y * pitch;
			for (var x = 0; x < width; x++)
			{
				var paletteIndex = indexedData[rowOffset + x];
				var color = palette[paletteIndex];

				// Extract RGBA components from palette entry (format: 0xAABBGGRR or 0x00BBGGRR)
				var r = (byte)(color & 0xFF);
				var g = (byte)((color >> 8) & 0xFF);
				var b = (byte)((color >> 16) & 0xFF);
				var a = (byte)((color >> 24) & 0xFF);

				// If alpha is 0, assume fully opaque
				if (a == 0)
				{
					a = 0xFF;
				}

				rgbaData[rgbaIndex++] = r;
				rgbaData[rgbaIndex++] = g;
				rgbaData[rgbaIndex++] = b;
				rgbaData[rgbaIndex++] = a;
			}
		}

		return rgbaData;
	}

	public byte[] Convert16BitToRGBA(byte[] rgb565Data, int width, int height, int pitch)
	{
		if (rgb565Data == null)
		{
			throw new ArgumentNullException(nameof(rgb565Data));
		}

		_logger.LogDebug("[Headless] Converting 16-bit RGB565 to RGBA: {Width}x{Height}, pitch={Pitch}", width, height, pitch);

		var rgbaData = new byte[width * height * 4];
		var rgbaIndex = 0;

		for (var y = 0; y < height; y++)
		{
			var rowOffset = y * pitch;
			for (var x = 0; x < width; x++)
			{
				var pixelOffset = rowOffset + (x * 2);
				var pixel = (ushort)(rgb565Data[pixelOffset] | (rgb565Data[pixelOffset + 1] << 8));

				// Extract RGB565 components
				var r5 = (byte)((pixel >> 11) & 0x1F);
				var g6 = (byte)((pixel >> 5) & 0x3F);
				var b5 = (byte)(pixel & 0x1F);

				// Convert to 8-bit values
				var r = (byte)((r5 << 3) | (r5 >> 2));
				var g = (byte)((g6 << 2) | (g6 >> 4));
				var b = (byte)((b5 << 3) | (b5 >> 2));

				rgbaData[rgbaIndex++] = r;
				rgbaData[rgbaIndex++] = g;
				rgbaData[rgbaIndex++] = b;
				rgbaData[rgbaIndex++] = 0xFF; // Fully opaque
			}
		}

		return rgbaData;
	}

	public byte[] Convert24BitToRGBA(byte[] rgb24Data, int width, int height, int pitch)
	{
		if (rgb24Data == null)
		{
			throw new ArgumentNullException(nameof(rgb24Data));
		}

		_logger.LogDebug("[Headless] Converting 24-bit RGB to RGBA: {Width}x{Height}, pitch={Pitch}", width, height, pitch);

		var rgbaData = new byte[width * height * 4];
		var rgbaIndex = 0;

		for (var y = 0; y < height; y++)
		{
			var rowOffset = y * pitch;
			for (var x = 0; x < width; x++)
			{
				var pixelOffset = rowOffset + (x * 3);

				// 24-bit is typically BGR format in Windows
				var b = rgb24Data[pixelOffset];
				var g = rgb24Data[pixelOffset + 1];
				var r = rgb24Data[pixelOffset + 2];

				rgbaData[rgbaIndex++] = r;
				rgbaData[rgbaIndex++] = g;
				rgbaData[rgbaIndex++] = b;
				rgbaData[rgbaIndex++] = 0xFF; // Fully opaque
			}
		}

		return rgbaData;
	}

	public bool UpdateFrameBuffer(byte[] data, int pitch, IntPtr targetWindowHandle = default)
	{
		lock (_lock)
		{
			if (!_initialized)
			{
				_logger.LogWarning("[Headless] Cannot update frame buffer: backend not initialized");
				return false;
			}

			if (_frameBuffer == null)
			{
				_logger.LogWarning("[Headless] Cannot update frame buffer: frame buffer is null");
				return false;
			}

			if (data == null)
			{
				_logger.LogWarning("[Headless] Cannot update frame buffer: data is null");
				return false;
			}

			try
			{
				// Calculate expected data size
				var expectedSize = _width * _height * 4; // RGBA format

				if (data.Length < expectedSize)
				{
					_logger.LogWarning("[Headless] Data size ({DataSize}) is less than expected size ({ExpectedSize})", 
						data.Length, expectedSize);
				}

				// Copy data to frame buffer (stored in memory only)
				var copySize = Math.Min(data.Length, _frameBuffer.Length);
				Array.Copy(data, 0, _frameBuffer, 0, copySize);

				// Save frame to disk if frame dumping is enabled
				if (_enableFrameDumping && _frameDumpPath != null)
				{
					try
					{
						SaveFrameToDisk(_frameBuffer, _width, _height);
					}
					catch (Exception dumpEx)
					{
						_logger.LogError(dumpEx, "[Headless] Failed to dump frame {FrameNumber}", _frameCounter);
					}
				}

				_logger.LogDebug("[Headless] Frame buffer updated: {Size} bytes copied (no display output)", copySize);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Headless] Failed to update frame buffer");
				return false;
			}
		}
	}

	public void Clear(byte r, byte g, byte b, byte a = 255)
	{
		lock (_lock)
		{
			if (!_initialized || _frameBuffer == null)
			{
				return;
			}

			_logger.LogDebug("[Headless] Clearing frame buffer with color ({R}, {G}, {B}, {A})", r, g, b, a);

			// Fill frame buffer with specified color
			for (var i = 0; i < _frameBuffer.Length; i += 4)
			{
				_frameBuffer[i] = r;
				_frameBuffer[i + 1] = g;
				_frameBuffer[i + 2] = b;
				_frameBuffer[i + 3] = a;
			}
		}
	}

	public void ProcessEvents()
	{
		// Headless mode: No events to process
		// This is intentionally a no-op
	}

	public bool IsInitialized => _initialized;

	public int Width => _width;

	public int Height => _height;

	// Hardware-accelerated rendering methods (not supported in headless backend)
	
	public void BeginFrame()
	{
		// Headless backend doesn't need explicit frame begin
		_logger.LogDebug("[Headless] BeginFrame called (no-op for headless backend)");
	}

	public void EndFrame()
	{
		// Headless backend doesn't need explicit frame end
		_logger.LogDebug("[Headless] EndFrame called (no-op for headless backend)");
	}

	public void DrawTriangles(Span<Vertex> vertices, Span<ushort> indices)
	{
		_logger.LogWarning("[Headless] DrawTriangles not supported in headless backend (use UpdateFrameBuffer)");
		// Headless backend uses CPU rasterization via UpdateFrameBuffer
		// Hardware acceleration is not available
	}

	public void SetTexture(uint textureId, byte[] data, int width, int height, TextureFormat format)
	{
		_logger.LogWarning("[Headless] SetTexture not supported in headless backend");
		// Textures are handled via frame buffer updates in headless backend
	}

	public void BindTexture(uint textureId)
	{
		_logger.LogWarning("[Headless] BindTexture not supported in headless backend");
	}

	public void SetRenderState(BlendMode blend, DepthTest depth, CullMode cull)
	{
		_logger.LogWarning("[Headless] SetRenderState not supported in headless backend");
		// Render state is handled by CPU rasterizer in calling code
	}

	public void DeleteTexture(uint textureId)
	{
		_logger.LogWarning("[Headless] DeleteTexture not supported in headless backend");
	}

	/// <summary>
	/// Save current frame buffer to disk as PNG file
	/// </summary>
	private void SaveFrameToDisk(byte[] frameBuffer, int width, int height)
	{
		if (_frameDumpPath == null)
		{
			return;
		}

		// Increment counter first to ensure unique filenames even if save fails
		var currentFrame = _frameCounter++;
		
		// Save every frame (can be optimized to save every N frames if needed)
		var fileName = Path.Combine(_frameDumpPath, $"frame_{currentFrame:D6}.png");
		
		// Convert RGBA byte array to ImageSharp Image
		using (var image = SixLabors.ImageSharp.Image.LoadPixelData<Rgba32>(frameBuffer, width, height))
		{
			image.SaveAsPng(fileName);
		}
		
		// Log every 100th frame to avoid log spam
		if (currentFrame % 100 == 0)
		{
			_logger.LogInformation("[Headless] Saved frame {FrameNumber} to {FileName}", currentFrame, fileName);
		}
	}

	public void Dispose()
	{
		lock (_lock)
		{
			if (_disposed)
			{
				return;
			}

			_logger.LogInformation("[Headless] Disposing headless rendering backend");

			Cleanup();
			_disposed = true;

			GC.SuppressFinalize(this);
		}
	}

	private void Cleanup()
	{
		_frameBuffer = null;
		_initialized = false;
	}
}
