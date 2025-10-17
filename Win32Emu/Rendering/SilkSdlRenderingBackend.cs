using Microsoft.Extensions.Logging;
using Silk.NET.SDL;
using System.Runtime.InteropServices;

namespace Win32Emu.Rendering;

/// <summary>
/// Silk.NET SDL-based rendering backend for DirectDraw and GDI operations
/// </summary>
public class SilkSdlRenderingBackend : IRenderingBackend
{
    private readonly ILogger _logger;
    private readonly Sdl _sdl;
    private unsafe Window* _window;
    private unsafe Renderer* _renderer;
    private unsafe Texture* _texture;
    private bool _initialized;
    private int _width;
    private int _height;
    private readonly object _lock = new();

    public SilkSdlRenderingBackend(ILogger logger)
    {
        _logger = logger;
        _sdl = Sdl.GetApi();
    }

    public unsafe bool Initialize(int width, int height, string title = "Win32Emu Display")
    {
        lock (_lock)
        {
            if (_initialized)
            {
                return true;
            }

            _width = width;
            _height = height;

            // Initialize SDL video subsystem
            if (_sdl.Init(Sdl.InitVideo) < 0)
            {
                _logger.LogError("[SilkSDL] Failed to initialize: {Error}", Marshal.PtrToStringAnsi((IntPtr)_sdl.GetError()));
                return false;
            }

            // Create window
            _window = _sdl.CreateWindow(title, Sdl.WindowposUndefined, Sdl.WindowposUndefined, width, height, (uint)WindowFlags.Resizable);
            if (_window == null)
            {
                _logger.LogError("[SilkSDL] Failed to create window: {Error}", Marshal.PtrToStringAnsi((IntPtr)_sdl.GetError()));
                _sdl.Quit();
                return false;
            }

            // Create renderer with hardware acceleration
            _renderer = _sdl.CreateRenderer(_window, -1, (uint)RendererFlags.Accelerated);
            if (_renderer == null)
            {
                _logger.LogError("[SilkSDL] Failed to create renderer: {Error}", Marshal.PtrToStringAnsi((IntPtr)_sdl.GetError()));
                _sdl.DestroyWindow(_window);
                _sdl.Quit();
                return false;
            }

            // Create streaming texture for frame buffer updates
            _texture = _sdl.CreateTexture(_renderer, Sdl.PixelformatAbgr8888, (int)TextureAccess.Streaming, width, height);
            if (_texture == null)
            {
                _logger.LogError("[SilkSDL] Failed to create texture: {Error}", Marshal.PtrToStringAnsi((IntPtr)_sdl.GetError()));
                _sdl.DestroyRenderer(_renderer);
                _sdl.DestroyWindow(_window);
                _sdl.Quit();
                return false;
            }

            _initialized = true;
            
            // Clear the window with black to show it's properly initialized
            _sdl.SetRenderDrawColor(_renderer, 0, 0, 0, 255);
            _sdl.RenderClear(_renderer);
            _sdl.RenderPresent(_renderer);
            
            _logger.LogInformation("[SilkSDL] Initialized {Width}x{Height} display", width, height);
            return true;
        }
    }

    public byte[] ConvertPalettizedToRGBA(byte[] indexedData, uint[] palette, int width, int height, int pitch)
    {
        var rgbaData = new byte[width * height * 4];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int srcOffset = y * pitch + x;
                int dstOffset = (y * width + x) * 4;
                
                if (srcOffset < indexedData.Length)
                {
                    byte paletteIndex = indexedData[srcOffset];
                    
                    if (paletteIndex < palette.Length)
                    {
                        uint color = palette[paletteIndex];
                        
                        rgbaData[dstOffset + 0] = (byte)(color & 0xFF);         // R
                        rgbaData[dstOffset + 1] = (byte)((color >> 8) & 0xFF);  // G
                        rgbaData[dstOffset + 2] = (byte)((color >> 16) & 0xFF); // B
                        rgbaData[dstOffset + 3] = 0xFF;                          // A
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
                int srcOffset = y * pitch + x * 2;
                int dstOffset = (y * width + x) * 4;
                
                if (srcOffset + 1 < rgb565Data.Length)
                {
                    ushort pixel = (ushort)(rgb565Data[srcOffset] | (rgb565Data[srcOffset + 1] << 8));
                    
                    byte r5 = (byte)((pixel >> 11) & 0x1F);
                    byte g6 = (byte)((pixel >> 5) & 0x3F);
                    byte b5 = (byte)(pixel & 0x1F);
                    byte r = (byte)((r5 << 3) | (r5 >> 2));
                    byte g = (byte)((g6 << 2) | (g6 >> 4));
                    byte b = (byte)((b5 << 3) | (b5 >> 2));
                    
                    rgbaData[dstOffset + 0] = r;
                    rgbaData[dstOffset + 1] = g;
                    rgbaData[dstOffset + 2] = b;
                    rgbaData[dstOffset + 3] = 0xFF;
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
                int srcOffset = y * pitch + x * 3;
                int dstOffset = (y * width + x) * 4;
                
                if (srcOffset + 2 < rgb24Data.Length)
                {
                    // 24-bit is typically BGR format in Windows
                    rgbaData[dstOffset + 0] = rgb24Data[srcOffset + 2]; // R
                    rgbaData[dstOffset + 1] = rgb24Data[srcOffset + 1]; // G
                    rgbaData[dstOffset + 2] = rgb24Data[srcOffset + 0]; // B
                    rgbaData[dstOffset + 3] = 0xFF;                      // A
                }
            }
        }
        
        return rgbaData;
    }

    public unsafe bool UpdateFrameBuffer(byte[] data, int pitch)
    {
        lock (_lock)
        {
            if (!_initialized || _renderer == null || _texture == null)
            {
                return false;
            }

            // Lock texture and copy data
            void* pixels;
            int texturePitch;
            if (_sdl.LockTexture(_texture, null, &pixels, &texturePitch) < 0)
            {
                _logger.LogError("[SilkSDL] Failed to lock texture: {Error}", Marshal.PtrToStringAnsi((IntPtr)_sdl.GetError()));
                return false;
            }

            // Copy data to texture
            int copySize = Math.Min(data.Length, _height * texturePitch);
            Marshal.Copy(data, 0, (IntPtr)pixels, copySize);

            _sdl.UnlockTexture(_texture);

            // Clear renderer and copy texture
            _sdl.RenderClear(_renderer);
            _sdl.RenderCopy(_renderer, _texture, null, null);
            _sdl.RenderPresent(_renderer);

            return true;
        }
    }

    public unsafe void Clear(byte r, byte g, byte b, byte a = 255)
    {
        lock (_lock)
        {
            if (!_initialized || _renderer == null)
            {
                return;
            }

            _sdl.SetRenderDrawColor(_renderer, r, g, b, a);
            _sdl.RenderClear(_renderer);
            _sdl.RenderPresent(_renderer);
        }
    }

    public unsafe void ProcessEvents()
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                return;
            }

            Event evt;
            while (_sdl.PollEvent(&evt) != 0)
            {
                // Handle events if needed
            }
        }
    }

    public unsafe void Dispose()
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                return;
            }

            if (_texture != null)
            {
                _sdl.DestroyTexture(_texture);
                _texture = null;
            }

            if (_renderer != null)
            {
                _sdl.DestroyRenderer(_renderer);
                _renderer = null;
            }

            if (_window != null)
            {
                _sdl.DestroyWindow(_window);
                _window = null;
            }

            _sdl.Quit();
            _initialized = false;
        }
    }

    public bool IsInitialized => _initialized;
    public int Width => _width;
    public int Height => _height;
}
