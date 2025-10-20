using Microsoft.Extensions.Logging;
using Silk.NET.GLFW;
using Silk.NET.OpenGL;
using System.Runtime.InteropServices;

namespace Win32Emu.Rendering;

/// <summary>
/// Silk.NET GLFW-based rendering backend for DirectDraw and GDI operations
/// </summary>
public unsafe class SilkGlfwRenderingBackend : IRenderingBackend
{
    private readonly ILogger _logger;
    private readonly Glfw _glfw;
    private WindowHandle* _window;
    private GL? _gl;
    private uint _textureId;
    private bool _initialized;
    private int _width;
    private int _height;
    private readonly object _lock = new();

    /// <summary>
    /// Event fired when a UI event occurs (mouse, keyboard, window)
    /// </summary>
    public event EventHandler<UIEventArgs>? UIEvent;

    public SilkGlfwRenderingBackend(ILogger logger)
    {
        _logger = logger;
        _glfw = Glfw.GetApi();
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

            // Initialize GLFW
            if (!_glfw.Init())
            {
                _logger.LogError("[SilkGLFW] Failed to initialize GLFW");
                return false;
            }

            // Set window hints
            _glfw.WindowHint(WindowHintInt.ContextVersionMajor, 3);
            _glfw.WindowHint(WindowHintInt.ContextVersionMinor, 3);
            _glfw.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
            _glfw.WindowHint(WindowHintBool.Resizable, true);

            // Create window
            _window = _glfw.CreateWindow(width, height, title, null, null);
            if (_window == null)
            {
                _logger.LogError("[SilkGLFW] Failed to create window");
                _glfw.Terminate();
                return false;
            }

            // Make context current and load OpenGL
            _glfw.MakeContextCurrent(_window);
            _gl = GL.GetApi(_glfw.GetProcAddress);

            if (_gl == null)
            {
                _logger.LogError("[SilkGLFW] Failed to load OpenGL");
                _glfw.DestroyWindow(_window);
                _glfw.Terminate();
                return false;
            }

            // Create texture for frame buffer
            _textureId = _gl.GenTexture();
            _gl.BindTexture(TextureTarget.Texture2D, _textureId);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);

            // Allocate texture storage
            _gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba8, (uint)width, (uint)height, 
                          0, PixelFormat.Rgba, PixelType.UnsignedByte, null);

            // Set up window callbacks for lifecycle events
            _glfw.SetWindowFocusCallback(_window, (window, focused) =>
            {
                if (focused)
                {
                    _logger.LogDebug("[SilkGLFW] Window gained focus, firing WindowActivate event");
                    OnUIEvent(new UIEventArgs
                    {
                        EventType = UIEventType.WindowActivate,
                        WindowHandle = 0 // Will be resolved by ProcessEnvironment
                    });
                }
                else
                {
                    _logger.LogDebug("[SilkGLFW] Window lost focus, firing WindowDeactivate event");
                    OnUIEvent(new UIEventArgs
                    {
                        EventType = UIEventType.WindowDeactivate,
                        WindowHandle = 0
                    });
                }
            });

            _initialized = true;
            _logger.LogInformation("[SilkGLFW] Initialized {Width}x{Height} display", width, height);
            return true;
        }
    }

    public byte[] ConvertPalettizedToRGBA(byte[] indexedData, uint[] palette, int width, int height, int pitch)
    {
        var rgbaData = new byte[width * height * 4];
        
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var srcOffset = y * pitch + x;
                var dstOffset = (y * width + x) * 4;
                
                if (srcOffset < indexedData.Length)
                {
                    var paletteIndex = indexedData[srcOffset];
                    
                    if (paletteIndex < palette.Length)
                    {
                        var color = palette[paletteIndex];
                        
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
        
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var srcOffset = y * pitch + x * 2;
                var dstOffset = (y * width + x) * 4;
                
                if (srcOffset + 1 < rgb565Data.Length)
                {
                    var pixel = (ushort)(rgb565Data[srcOffset] | (rgb565Data[srcOffset + 1] << 8));
                    
                    var r5 = (byte)((pixel >> 11) & 0x1F);
                    var g6 = (byte)((pixel >> 5) & 0x3F);
                    var b5 = (byte)(pixel & 0x1F);
                    var r = (byte)((r5 << 3) | (r5 >> 2));
                    var g = (byte)((g6 << 2) | (g6 >> 4));
                    var b = (byte)((b5 << 3) | (b5 >> 2));
                    
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
        
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var srcOffset = y * pitch + x * 3;
                var dstOffset = (y * width + x) * 4;
                
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

    public bool UpdateFrameBuffer(byte[] data, int pitch)
    {
        lock (_lock)
        {
            if (!_initialized || _gl == null || _window == null)
            {
                return false;
            }

            _glfw.MakeContextCurrent(_window);

            // Update texture with new data
            _gl.BindTexture(TextureTarget.Texture2D, _textureId);
            fixed (byte* ptr = data)
            {
                _gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, (uint)_width, (uint)_height, 
                                 PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
            }

            // Clear and render
            _gl.Clear(ClearBufferMask.ColorBufferBit);
            
            // Note: A full implementation would use shaders and VAOs for proper rendering
            // TODO: Implement full OpenGL rendering pipeline (use shaders and VAOs) for proper rendering.
            // For now, just update the texture - actual rendering requires more OpenGL setup
            _glfw.SwapBuffers(_window);

            return true;
        }
    }

    public void Clear(byte r, byte g, byte b, byte a = 255)
    {
        lock (_lock)
        {
            if (!_initialized || _gl == null || _window == null)
            {
                return;
            }

            _glfw.MakeContextCurrent(_window);
            _gl.ClearColor(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
            _gl.Clear(ClearBufferMask.ColorBufferBit);
            _glfw.SwapBuffers(_window);
        }
    }

    public void ProcessEvents()
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                return;
            }

            _glfw.PollEvents();
            // Note: GLFW event handling would typically be set up via callbacks
            // in Initialize() using SetMouseButtonCallback, SetKeyCallback, etc.
            // For now, we poll but don't translate events to UI events.
            // This would need callback registration to work properly.
        }
    }

    /// <summary>
    /// Helper method to raise UI events
    /// </summary>
    protected virtual void OnUIEvent(UIEventArgs e)
    {
        UIEvent?.Invoke(this, e);
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                return;
            }

            if (_gl != null && _textureId != 0)
            {
                _gl.DeleteTexture(_textureId);
                _textureId = 0;
            }

            if (_window != null)
            {
                _glfw.DestroyWindow(_window);
                _window = null;
            }

            _glfw.Terminate();
            _gl?.Dispose();
            _gl = null;
            _initialized = false;
        }
    }

    public bool IsInitialized => _initialized;
    public int Width => _width;
    public int Height => _height;
}
