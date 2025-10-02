using SDL3;
using Win32Emu.Memory;

namespace Win32Emu.Rendering;

/// <summary>
/// SDL3-based rendering backend for DirectDraw and GDI operations
/// </summary>
public class SDL3RenderingBackend : IDisposable
{
    private IntPtr _window;
    private IntPtr _renderer;
    private IntPtr _texture;
    private bool _initialized;
    private int _width;
    private int _height;
    private readonly object _lock = new();

    /// <summary>
    /// Initialize SDL3 with specified dimensions
    /// </summary>
    public bool Initialize(int width, int height, string title = "Win32Emu Display")
    {
        lock (_lock)
        {
            if (_initialized)
                return true;

            _width = width;
            _height = height;

            // Initialize SDL3 video subsystem
            // Note: Other subsystems (Audio, Gamepad, Joystick) are initialized separately
            // by their respective backends
            if (!SDL.Init(SDL.InitFlags.Video))
            {
                Console.WriteLine($"[SDL3] Failed to initialize video: {SDL.GetError()}");
                return false;
            }

            // Create window
            _window = SDL.CreateWindow(title, width, height, SDL.WindowFlags.Resizable);
            if (_window == IntPtr.Zero)
            {
                Console.WriteLine($"[SDL3] Failed to create window: {SDL.GetError()}");
                SDL.Quit();
                return false;
            }

            // Create renderer
            _renderer = SDL.CreateRenderer(_window, null);
            if (_renderer == IntPtr.Zero)
            {
                Console.WriteLine($"[SDL3] Failed to create renderer: {SDL.GetError()}");
                SDL.DestroyWindow(_window);
                SDL.Quit();
                return false;
            }

            // Create texture for rendering
            _texture = SDL.CreateTexture(_renderer, 
                SDL.PixelFormat.ARGB8888,
                SDL.TextureAccess.Streaming, 
                width, height);
                
            if (_texture == IntPtr.Zero)
            {
                Console.WriteLine($"[SDL3] Failed to create texture: {SDL.GetError()}");
                SDL.DestroyRenderer(_renderer);
                SDL.DestroyWindow(_window);
                SDL.Quit();
                return false;
            }

            _initialized = true;
            Console.WriteLine($"[SDL3] Initialized {width}x{height} display");
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
                return false;

            // Update texture with new data
            unsafe
            {
                fixed (byte* ptr = data)
                {
                    if (!SDL.UpdateTexture(_texture, IntPtr.Zero, (IntPtr)ptr, pitch))
                    {
                        Console.WriteLine($"[SDL3] Failed to update texture: {SDL.GetError()}");
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
                return;

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
                return;

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
                return;

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

            // Only quit video subsystem - other subsystems managed by their backends
            SDL.QuitSubSystem(SDL.InitFlags.Video);
            _initialized = false;
        }
    }

    public bool IsInitialized => _initialized;
    public int Width => _width;
    public int Height => _height;
}
