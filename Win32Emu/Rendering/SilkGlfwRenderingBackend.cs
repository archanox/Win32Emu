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

            _initialized = true;
            _logger.LogInformation("[SilkGLFW] Initialized {Width}x{Height} display", width, height);
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
