using Microsoft.Extensions.Logging;
using SDL3;
using System.Runtime.InteropServices;

namespace Win32Emu.Gui.Backends;
using Win32Emu.Rendering;
/// <summary>
/// Software (CPU-based) rendering backend for DirectDraw operations.
/// This backend uses SDL3's software renderer for true CPU-only rendering without GPU acceleration.
/// Suitable for macOS and other platforms where GPU acceleration may not be available or desired.
/// </summary>
public unsafe class SoftwareRenderingBackend : IRenderingBackend
{
    private readonly ILogger _logger;
    private IntPtr _window;
    private IntPtr _renderer;
    private IntPtr _texture;
    private int _width;
    private int _height;
    private bool _initialized;
    private readonly Lock _lock = new();
    private bool _disposed;
    private byte[]? _frameBuffer;

    /// <summary>
    /// Event fired when a UI event occurs (mouse, keyboard, window)
    /// </summary>
    public event EventHandler<UIEventArgs>? UIEvent;

    public SoftwareRenderingBackend(ILogger logger)
    {
        _logger = logger;
    }

    public bool Initialize(int width, int height, string title = "Win32Emu Display")
    {
        lock (_lock)
        {
            if (_initialized)
            {
                return true;
            }

            _width = width;
            _height = height;

            try
            {
                _logger.LogInformation("[Software] Initializing SDL3 software rendering backend ({Width}x{Height})...", width, height);

                // Critical: Set app metadata before any SDL initialization
                // This also handles headless mode detection and configuration
                Sdl3Initializer.EnsureAppMetadataSet();

                // Initialize SDL video subsystem
                if (!SDL.Init(SDL.InitFlags.Video))
                {
                    _logger.LogError("[Software] Failed to initialize SDL video: {Error}", SDL.GetError());
                    return false;
                }

                // Force SDL to use the software rendering driver
                SDL.SetHint("SDL_RENDER_DRIVER", "software");
                // Force SDL to use the software rendering driver
                SDL.SetHint("SDL_RENDER_DRIVER", "software");
                // Create window
                _window = SDL.CreateWindow(title, width, height, SDL.WindowFlags.Hidden);
                if (_window == IntPtr.Zero)
                {
                    _logger.LogError("[Software] Failed to create window: {Error}", SDL.GetError());
                    SDL.Quit();
                    return false;
                }

                // Create software renderer (CPU-only, no GPU acceleration)
                _renderer = SDL.CreateRenderer(_window, null);
                if (_renderer == IntPtr.Zero)
                {
                    _logger.LogError("[Software] Failed to create software renderer: {Error}", SDL.GetError());
                    SDL.DestroyWindow(_window);
                    SDL.Quit();
                    return false;
                }

                _logger.LogInformation("[Software] Created software renderer");

                // Create streaming texture for frame updates (RGBA format)
                _texture = SDL.CreateTexture(_renderer, SDL.PixelFormat.RGBA8888, 
                    SDL.TextureAccess.Streaming, width, height);
                if (_texture == IntPtr.Zero)
                {
                    _logger.LogError("[Software] Failed to create texture: {Error}", SDL.GetError());
                    SDL.DestroyRenderer(_renderer);
                    SDL.DestroyWindow(_window);
                    SDL.Quit();
                    return false;
                }

                // Allocate frame buffer (RGBA format)
                var bufferSize = width * height * 4; // 4 bytes per pixel (RGBA)
                _frameBuffer = new byte[bufferSize];

                // Show the window
                SDL.ShowWindow(_window);

                _initialized = true;
                _logger.LogInformation("[Software] Software rendering backend initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Software] Failed to initialize software rendering backend");
                Cleanup();
                return false;
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

        _logger.LogDebug("[Software] Converting palettized data to RGBA: {Width}x{Height}, pitch={Pitch}", width, height, pitch);

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

        _logger.LogDebug("[Software] Converting 16-bit RGB565 to RGBA: {Width}x{Height}, pitch={Pitch}", width, height, pitch);

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
                var r5 = (pixel >> 11) & 0x1F;
                var g6 = (pixel >> 5) & 0x3F;
                var b5 = pixel & 0x1F;

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

        _logger.LogDebug("[Software] Converting 24-bit RGB to RGBA: {Width}x{Height}, pitch={Pitch}", width, height, pitch);

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

    public bool UpdateFrameBuffer(byte[] data, int pitch)
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                _logger.LogWarning("[Software] Cannot update frame buffer: backend not initialized");
                return false;
            }

            if (_frameBuffer == null)
            {
                _logger.LogWarning("[Software] Cannot update frame buffer: frame buffer is null");
                return false;
            }

            if (data == null)
            {
                _logger.LogWarning("[Software] Cannot update frame buffer: data is null");
                return false;
            }

            try
            {
                // Calculate expected data size
                var expectedSize = _width * _height * 4; // RGBA format

                if (data.Length < expectedSize)
                {
                    _logger.LogWarning("[Software] Data size ({DataSize}) is less than expected size ({ExpectedSize})", 
                        data.Length, expectedSize);
                }

                // Copy data to frame buffer
                var copySize = Math.Min(data.Length, _frameBuffer.Length);
                Array.Copy(data, 0, _frameBuffer, 0, copySize);

                // Update SDL texture with CPU-rendered data (software blit, no GPU)
                fixed (byte* dataPtr = _frameBuffer)
                {
                    if (!SDL.UpdateTexture(_texture, IntPtr.Zero, (IntPtr)dataPtr, _width * 4))
                    {
                        _logger.LogError("[Software] Failed to update texture: {Error}", SDL.GetError());
                        return false;
                    }
                }

                // Render to window using software renderer
                SDL.RenderClear(_renderer);
                SDL.RenderTexture(_renderer, _texture, IntPtr.Zero, IntPtr.Zero);
                SDL.RenderPresent(_renderer);

                _logger.LogDebug("[Software] Frame buffer updated: {Size} bytes copied", copySize);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Software] Failed to update frame buffer");
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

            _logger.LogDebug("[Software] Clearing frame buffer with color ({R}, {G}, {B}, {A})", r, g, b, a);

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
        lock (_lock)
        {
            if (!_initialized || _window == IntPtr.Zero)
            {
                return;
            }

            try
            {
                // Poll SDL events
                SDL.Event evt;
                while (SDL.PollEvent(out evt))
                {
                    switch ((SDL.EventType)evt.Type)
                    {
                        case SDL.EventType.WindowShown:
                        case SDL.EventType.WindowExposed:
                        case SDL.EventType.WindowFocusGained:
                            OnUIEvent(new UIEventArgs
                            {
                                EventType = UIEventType.WindowActivate,
                                WindowHandle = evt.Window.WindowID
                            });
                            break;

                        case SDL.EventType.WindowHidden:
                        case SDL.EventType.WindowMinimized:
                        case SDL.EventType.WindowFocusLost:
                            OnUIEvent(new UIEventArgs
                            {
                                EventType = UIEventType.WindowDeactivate,
                                WindowHandle = evt.Window.WindowID
                            });
                            break;

                        case SDL.EventType.WindowCloseRequested:
                        case SDL.EventType.Quit:
                            OnUIEvent(new UIEventArgs
                            {
                                EventType = UIEventType.WindowClose,
                                WindowHandle = evt.Window.WindowID
                            });
                            break;

                        case SDL.EventType.KeyDown:
                            OnUIEvent(new UIEventArgs
                            {
                                EventType = UIEventType.KeyDown,
                                WindowHandle = evt.Key.WindowID,
                                KeyCode = (int)evt.Key.Key,
                                IsPressed = true
                            });
                            break;

                        case SDL.EventType.KeyUp:
                            OnUIEvent(new UIEventArgs
                            {
                                EventType = UIEventType.KeyUp,
                                WindowHandle = evt.Key.WindowID,
                                KeyCode = (int)evt.Key.Key,
                                IsPressed = false
                            });
                            break;

                        case SDL.EventType.MouseMotion:
                            OnUIEvent(new UIEventArgs
                            {
                                EventType = UIEventType.MouseMove,
                                WindowHandle = evt.Motion.WindowID,
                                MouseX = (int)evt.Motion.X,
                                MouseY = (int)evt.Motion.Y
                            });
                            break;

                        case SDL.EventType.MouseButtonDown:
                            OnUIEvent(new UIEventArgs
                            {
                                EventType = UIEventType.MouseButtonDown,
                                WindowHandle = evt.Button.WindowID,
                                MouseX = (int)evt.Button.X,
                                MouseY = (int)evt.Button.Y,
                                IsPressed = true
                            });
                            break;

                        case SDL.EventType.MouseButtonUp:
                            OnUIEvent(new UIEventArgs
                            {
                                EventType = UIEventType.MouseButtonUp,
                                WindowHandle = evt.Button.WindowID,
                                MouseX = (int)evt.Button.X,
                                MouseY = (int)evt.Button.Y,
                                IsPressed = false
                            });
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Software] Error processing events");
            }
        }
    }

    protected virtual void OnUIEvent(UIEventArgs e)
    {
        UIEvent?.Invoke(this, e);
    }

    public bool IsInitialized => _initialized;

    public int Width => _width;

    public int Height => _height;

    // Hardware-accelerated rendering methods (not supported in software backend)
    
    public void BeginFrame()
    {
        // Software backend doesn't need explicit frame begin
        _logger.LogDebug("[Software] BeginFrame called (no-op for software backend)");
    }

    public void EndFrame()
    {
        // Software backend doesn't need explicit frame end
        _logger.LogDebug("[Software] EndFrame called (no-op for software backend)");
    }

    public void DrawTriangles(Span<Vertex> vertices, Span<ushort> indices)
    {
        _logger.LogWarning("[Software] DrawTriangles not supported in software backend (use UpdateFrameBuffer)");
        // Software backend uses CPU rasterization via UpdateFrameBuffer
        // Hardware acceleration is not available
    }

    public void SetTexture(uint textureId, byte[] data, int width, int height, TextureFormat format)
    {
        _logger.LogWarning("[Software] SetTexture not supported in software backend");
        // Textures are handled via frame buffer updates in software backend
    }

    public void BindTexture(uint textureId)
    {
        _logger.LogWarning("[Software] BindTexture not supported in software backend");
    }

    public void SetRenderState(BlendMode blend, DepthTest depth, CullMode cull)
    {
        _logger.LogWarning("[Software] SetRenderState not supported in software backend");
        // Render state is handled by CPU rasterizer in calling code
    }

    public void DeleteTexture(uint textureId)
    {
        _logger.LogWarning("[Software] DeleteTexture not supported in software backend");
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            _logger.LogInformation("[Software] Disposing software rendering backend");

            Cleanup();
            _disposed = true;

            GC.SuppressFinalize(this);
        }
    }

    private void Cleanup()
    {
        // Clean up SDL resources
        if (_texture != IntPtr.Zero)
        {
            SDL.DestroyTexture(_texture);
            _texture = IntPtr.Zero;
        }

        if (_renderer != IntPtr.Zero)
        {
            SDL.DestroyRenderer(_renderer);
            _renderer = IntPtr.Zero;
        }

        if (_window != IntPtr.Zero)
        {
            SDL.DestroyWindow(_window);
            _window = IntPtr.Zero;
        }

        if (_initialized)
        {
            SDL.Quit();
        }

        _frameBuffer = null;
        _initialized = false;
    }
}
