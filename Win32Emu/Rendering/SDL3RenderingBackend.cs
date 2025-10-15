using Microsoft.Extensions.Logging;
using SDL3;

namespace Win32Emu.Rendering;

/// <summary>
/// SDL3-based rendering backend for DirectDraw and GDI operations
/// </summary>
public class Sdl3RenderingBackend(ILogger logger) : IDisposable
{
    private IntPtr _window;
    private IntPtr _renderer;
    private IntPtr _texture;
    private bool _initialized;
    private int _width;
    private int _height;
    private readonly Lock _lock = new();

    /// <summary>
    /// Initialize SDL3 with specified dimensions
    /// </summary>
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

            // Set app metadata, similar to the C++ example
            SDL.SetAppMetadata(title, "1.0", "com.win32emu.display");

            // Initialize SDL3 video subsystem
            if (!SDL.Init(SDL.InitFlags.Video))
            {
                logger.LogError("[SDL3] Failed to initialize video: {GetError}", SDL.GetError());
                return false;
            }

            // Create window and renderer
            if (!SDL.CreateWindowAndRenderer(title, width, height, SDL.WindowFlags.Resizable, out _window, out _renderer))
            {
                logger.LogError("[SDL3] Failed to create window and renderer: {GetError}", SDL.GetError());
                SDL.Quit();
                return false;
            }

            SDL.SetRenderLogicalPresentation(_renderer, width, height, SDL.RendererLogicalPresentation.Letterbox);

            // Create texture for rendering
            _texture = SDL.CreateTexture(_renderer,
                SDL.PixelFormat.ARGB8888,
                SDL.TextureAccess.Streaming,
                width, height);

            if (_texture == IntPtr.Zero)
            {
                logger.LogError("[SDL3] Failed to create texture: {GetError}", SDL.GetError());
                SDL.DestroyRenderer(_renderer);
                SDL.DestroyWindow(_window);
                SDL.Quit();
                return false;
            }

            _initialized = true;
            logger.LogInformation("[SDL3] Initialized {Width}x{Height} display", width, height);
            return true;
        }
    }

    /// <summary>
    /// Update the display with new frame buffer data
    /// </summary>
    public bool UpdateFrameBuffer(byte[] data, int pitch)
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                return false;
            }

            // Update texture with new data
            unsafe
            {
                fixed (byte* ptr = data)
                {
                    if (!SDL.UpdateTexture(_texture, IntPtr.Zero, (IntPtr)ptr, pitch))
                    {
                        logger.LogError("[SDL3] Failed to update texture: {GetError}", SDL.GetError());
                        return false;
                    }
                }
            }

            // Clear and render
            SDL.RenderClear(_renderer);
            SDL.RenderTexture(_renderer, _texture, IntPtr.Zero, IntPtr.Zero);
            SDL.RenderPresent(_renderer);

            return true;
        }
    }

    /// <summary>
    /// Clear the display with specified color
    /// </summary>
    public void Clear(byte r, byte g, byte b, byte a = 255)
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                return;
            }

            SDL.SetRenderDrawColor(_renderer, r, g, b, a);
            SDL.RenderClear(_renderer);
            SDL.RenderPresent(_renderer);
        }
    }

    /// <summary>
    /// Process SDL events (call periodically)
    /// </summary>
    public void ProcessEvents()
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                return;
            }

            SDL.Event evt;
            while (SDL.PollEvent(out evt))
            {
                switch ((SDL.EventType)evt.Type)
                {
                    case SDL.EventType.Quit:
                        // Handle quit event
                        break;
                    case SDL.EventType.WindowResized:
                        // Handle window resize
                        break;
                }
            }
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                return;
            }

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

            SDL.Quit();
            _initialized = false;
        }
    }

    public bool IsInitialized => _initialized;
    public int Width => _width;
    public int Height => _height;
}
