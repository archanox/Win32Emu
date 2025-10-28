using Microsoft.Extensions.Logging;
using Silk.NET.GLFW;
using Silk.NET.OpenGL;
using System.Runtime.InteropServices;

namespace Win32Emu.Rendering;

/// <summary>
/// Software (CPU-based) rendering backend for DirectDraw operations.
/// This backend uses CPU-based rendering with a windowed display using Silk.NET.GLFW.
/// Suitable for macOS and other platforms where GPU acceleration may not be available or desired.
/// </summary>
public unsafe class SoftwareRenderingBackend : IRenderingBackend
{
    private readonly ILogger _logger;
    private readonly Glfw _glfw;
    private WindowHandle* _window;
    private GL? _gl;
    private uint _textureId;
    private int _width;
    private int _height;
    private bool _initialized;
    private readonly object _lock = new();
    private bool _disposed;
    private byte[]? _frameBuffer;
    private GlfwCallbacks.ErrorCallback? _errorCallback;
    
    // OpenGL rendering pipeline components
    private uint _shaderProgram;
    private uint _vao;
    private uint _vbo;

    /// <summary>
    /// Event fired when a UI event occurs (mouse, keyboard, window)
    /// </summary>
    public event EventHandler<UIEventArgs>? UIEvent;

    public SoftwareRenderingBackend(ILogger logger)
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

            try
            {
                _logger.LogInformation("[Software] Initializing software rendering backend ({Width}x{Height})...", width, height);

                // Set up GLFW error callback
                _errorCallback = (Silk.NET.GLFW.ErrorCode error, string description) =>
                {
                    _logger.LogError("[Software] GLFW Error {ErrorCode}: {Description}", error, description);
                };
                _glfw.SetErrorCallback(_errorCallback);

                // Initialize GLFW
                if (!_glfw.Init())
                {
                    _logger.LogError("[Software] Failed to initialize GLFW");
                    return false;
                }

                // Set window hints for OpenGL 3.2 Core
                _glfw.WindowHint(WindowHintInt.ContextVersionMajor, 3);
                _glfw.WindowHint(WindowHintInt.ContextVersionMinor, 2);
                _glfw.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
                _glfw.WindowHint(WindowHintBool.Resizable, true);
                _glfw.WindowHint(WindowHintBool.OpenGLForwardCompat, true); // Required for macOS

                // Create window
                _window = _glfw.CreateWindow(width, height, title, null, null);
                if (_window == null)
                {
                    _logger.LogError("[Software] Failed to create window");
                    _glfw.Terminate();
                    return false;
                }

                // Make context current and load OpenGL
                _glfw.MakeContextCurrent(_window);
                _gl = GL.GetApi(_glfw.GetProcAddress);

                if (_gl == null)
                {
                    _logger.LogError("[Software] Failed to load OpenGL");
                    _glfw.DestroyWindow(_window);
                    _glfw.Terminate();
                    return false;
                }

                // Set up window callbacks for event processing
                SetupWindowCallbacks();

                // Initialize OpenGL rendering pipeline
                InitializeOpenGL();

                // Allocate frame buffer (RGBA format)
                var bufferSize = width * height * 4; // 4 bytes per pixel (RGBA)
                _frameBuffer = new byte[bufferSize];

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

    private void SetupWindowCallbacks()
    {
        // Window focus callback
        _glfw.SetWindowFocusCallback(_window, (window, focused) =>
        {
            if (focused)
            {
                OnUIEvent(new UIEventArgs
                {
                    EventType = UIEventType.WindowActivate,
                    WindowHandle = (uint)(nint)window
                });
            }
            else
            {
                OnUIEvent(new UIEventArgs
                {
                    EventType = UIEventType.WindowDeactivate,
                    WindowHandle = (uint)(nint)window
                });
            }
        });

        // Keyboard callback
        _glfw.SetKeyCallback(_window, (window, key, scancode, action, mods) =>
        {
            OnUIEvent(new UIEventArgs
            {
                EventType = action == InputAction.Press ? UIEventType.KeyDown : UIEventType.KeyUp,
                WindowHandle = (uint)(nint)window,
                KeyCode = (int)key,
                IsPressed = action == InputAction.Press
            });
        });

        // Mouse button callback
        _glfw.SetMouseButtonCallback(_window, (window, button, action, mods) =>
        {
            _glfw.GetCursorPos(window, out double xPos, out double yPos);
            OnUIEvent(new UIEventArgs
            {
                EventType = action == InputAction.Press ? UIEventType.MouseButtonDown : UIEventType.MouseButtonUp,
                WindowHandle = (uint)(nint)window,
                MouseX = (int)xPos,
                MouseY = (int)yPos,
                IsPressed = action == InputAction.Press
            });
        });

        // Mouse move callback
        _glfw.SetCursorPosCallback(_window, (window, xPos, yPos) =>
        {
            OnUIEvent(new UIEventArgs
            {
                EventType = UIEventType.MouseMove,
                WindowHandle = (uint)(nint)window,
                MouseX = (int)xPos,
                MouseY = (int)yPos
            });
        });

        // Window close callback
        _glfw.SetWindowCloseCallback(_window, (window) =>
        {
            OnUIEvent(new UIEventArgs
            {
                EventType = UIEventType.WindowClose,
                WindowHandle = (uint)(nint)window
            });
        });
    }

    private void InitializeOpenGL()
    {
        if (_gl == null) return;

        // Create and compile vertex shader
        const string vertexShaderSource = @"
            #version 330 core
            layout (location = 0) in vec2 aPosition;
            layout (location = 1) in vec2 aTexCoord;
            out vec2 TexCoord;
            void main()
            {
                gl_Position = vec4(aPosition, 0.0, 1.0);
                TexCoord = aTexCoord;
            }
        ";

        const string fragmentShaderSource = @"
            #version 330 core
            in vec2 TexCoord;
            out vec4 FragColor;
            uniform sampler2D uTexture;
            void main()
            {
                FragColor = texture(uTexture, TexCoord);
            }
        ";

        var vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, vertexShaderSource);
        _gl.CompileShader(vertexShader);
        CheckShaderCompilation(vertexShader, "vertex");

        var fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, fragmentShaderSource);
        _gl.CompileShader(fragmentShader);
        CheckShaderCompilation(fragmentShader, "fragment");

        _shaderProgram = _gl.CreateProgram();
        _gl.AttachShader(_shaderProgram, vertexShader);
        _gl.AttachShader(_shaderProgram, fragmentShader);
        _gl.LinkProgram(_shaderProgram);
        CheckProgramLinking(_shaderProgram);

        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        // Create fullscreen quad
        float[] vertices = new float[]
        {
            // positions   // texCoords
            -1.0f,  1.0f,  0.0f, 1.0f, // top left
            -1.0f, -1.0f,  0.0f, 0.0f, // bottom left
             1.0f, -1.0f,  1.0f, 0.0f, // bottom right
            -1.0f,  1.0f,  0.0f, 1.0f, // top left
             1.0f, -1.0f,  1.0f, 0.0f, // bottom right
             1.0f,  1.0f,  1.0f, 1.0f  // top right
        };

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        
        // Use fixed pointer for BufferData
        fixed (float* verticesPtr = vertices)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), verticesPtr, BufferUsageARB.StaticDraw);
        }

        _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*)0);
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*)(2 * sizeof(float)));
        _gl.EnableVertexAttribArray(1);

        // Create texture
        _textureId = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, _textureId);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        // Allocate texture storage
        _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)_width, (uint)_height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);

        _gl.BindVertexArray(0);
    }

    private void CheckShaderCompilation(uint shader, string type)
    {
        if (_gl == null) return;
        
        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int success);
        if (success == 0)
        {
            var infoLog = _gl.GetShaderInfoLog(shader);
            _logger.LogError("[Software] {Type} shader compilation failed: {InfoLog}", type, infoLog);
        }
    }

    private void CheckProgramLinking(uint program)
    {
        if (_gl == null) return;
        
        _gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int success);
        if (success == 0)
        {
            var infoLog = _gl.GetProgramInfoLog(program);
            _logger.LogError("[Software] Shader program linking failed: {InfoLog}", infoLog);
        }
    }

    protected virtual void OnUIEvent(UIEventArgs e)
    {
        UIEvent?.Invoke(this, e);
    }

    public byte[] ConvertPalettizedToRGBA(byte[] indexedData, uint[] palette, int width, int height, int pitch)
    {
        if (indexedData == null)
            throw new ArgumentNullException(nameof(indexedData));
        if (palette == null)
            throw new ArgumentNullException(nameof(palette));

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
                    a = 0xFF;

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
            throw new ArgumentNullException(nameof(rgb565Data));

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
            throw new ArgumentNullException(nameof(rgb24Data));

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

                // Upload to OpenGL texture for display
                if (_gl != null && _textureId != 0)
                {
                    _gl.BindTexture(TextureTarget.Texture2D, _textureId);
                    
                    fixed (byte* dataPtr = _frameBuffer)
                    {
                        _gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, (uint)_width, (uint)_height, 
                            PixelFormat.Rgba, PixelType.UnsignedByte, dataPtr);
                    }

                    // Render the texture to the window
                    RenderFrame();
                }

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

    private void RenderFrame()
    {
        if (_gl == null || _window == null) return;

        _gl.Clear(ClearBufferMask.ColorBufferBit);
        
        _gl.UseProgram(_shaderProgram);
        _gl.BindVertexArray(_vao);
        _gl.BindTexture(TextureTarget.Texture2D, _textureId);
        _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
        
        _glfw.SwapBuffers(_window);
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
            if (!_initialized || _window == null)
            {
                return;
            }

            try
            {
                _glfw.PollEvents();
                
                // Check if window should close
                if (_glfw.WindowShouldClose(_window))
                {
                    OnUIEvent(new UIEventArgs
                    {
                        EventType = UIEventType.WindowClose,
                        WindowHandle = (uint)(nint)_window
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Software] Error processing events");
            }
        }
    }

    public bool IsInitialized => _initialized;

    public int Width => _width;

    public int Height => _height;

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
        // Clean up OpenGL resources
        if (_gl != null)
        {
            if (_textureId != 0)
            {
                _gl.DeleteTexture(_textureId);
                _textureId = 0;
            }

            if (_vbo != 0)
            {
                _gl.DeleteBuffer(_vbo);
                _vbo = 0;
            }

            if (_vao != 0)
            {
                _gl.DeleteVertexArray(_vao);
                _vao = 0;
            }

            if (_shaderProgram != 0)
            {
                _gl.DeleteProgram(_shaderProgram);
                _shaderProgram = 0;
            }

            _gl = null;
        }

        // Clean up GLFW resources
        if (_window != null)
        {
            _glfw.DestroyWindow(_window);
            _window = null;
        }

        if (_initialized)
        {
            _glfw.Terminate();
        }

        _frameBuffer = null;
        _initialized = false;
    }
}
