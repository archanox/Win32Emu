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
    private GlfwCallbacks.ErrorCallback? _errorCallback;
    
    // OpenGL rendering pipeline components
    private uint _shaderProgram;
    private uint _vao;
    private uint _vbo;

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

            // Set up GLFW error callback to route errors through ILogger
            _errorCallback = (Silk.NET.GLFW.ErrorCode error, string description) =>
            {
                _logger.LogError("[SilkGLFW] GLFW Error {ErrorCode}: {Description}", error, description);
            };
            _glfw.SetErrorCallback(_errorCallback);

            // Initialize GLFW
            _logger.LogInformation("[SilkGLFW] Initializing GLFW...");
            if (!_glfw.Init())
            {
                _logger.LogError("[SilkGLFW] Failed to initialize GLFW");
                return false;
            }
            _logger.LogInformation("[SilkGLFW] GLFW initialized successfully");

            // Set window hints
            _logger.LogInformation("[SilkGLFW] Setting window hints for OpenGL 3.2 Core...");
            _glfw.WindowHint(WindowHintInt.ContextVersionMajor, 3);
            _glfw.WindowHint(WindowHintInt.ContextVersionMinor, 2);
            _glfw.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
            _glfw.WindowHint(WindowHintBool.Resizable, true);
            
            // On macOS, forward compatibility must be enabled for OpenGL 3.3 Core Profile
            _glfw.WindowHint(WindowHintBool.OpenGLForwardCompat, true);

            // Create window
            _logger.LogInformation("[SilkGLFW] Creating window: {Width}x{Height} - '{Title}'", width, height, title);
            _window = _glfw.CreateWindow(width, height, title, null, null);
            if (_window == null)
            {
                _logger.LogError("[SilkGLFW] Failed to create window");
                _glfw.Terminate();
                return false;
            }
            _logger.LogInformation("[SilkGLFW] Window created successfully");

            // Make context current and load OpenGL
            _logger.LogInformation("[SilkGLFW] Making context current and loading OpenGL...");
            _glfw.MakeContextCurrent(_window);
            _gl = GL.GetApi(_glfw.GetProcAddress);

            if (_gl == null)
            {
                _logger.LogError("[SilkGLFW] Failed to load OpenGL");
                _glfw.DestroyWindow(_window);
                _glfw.Terminate();
                return false;
            }
            _logger.LogInformation("[SilkGLFW] OpenGL loaded successfully");

            // Log OpenGL version information
            var glVersionPtr = _gl.GetString(StringName.Version);
            var glVendorPtr = _gl.GetString(StringName.Vendor);
            var glRendererPtr = _gl.GetString(StringName.Renderer);
            var glVersion = Marshal.PtrToStringAnsi((IntPtr)glVersionPtr) ?? "Unknown";
            var glVendor = Marshal.PtrToStringAnsi((IntPtr)glVendorPtr) ?? "Unknown";
            var glRenderer = Marshal.PtrToStringAnsi((IntPtr)glRendererPtr) ?? "Unknown";
            _logger.LogInformation("[SilkGLFW] OpenGL Version: {Version}, Vendor: {Vendor}, Renderer: {Renderer}", 
                glVersion, glVendor, glRenderer);

            // Create texture for frame buffer
            _logger.LogInformation("[SilkGLFW] Creating frame buffer texture...");
            _textureId = _gl.GenTexture();
            _gl.BindTexture(TextureTarget.Texture2D, _textureId);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);

            // Allocate texture storage
            _gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba8, (uint)width, (uint)height, 
                          0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
            _logger.LogInformation("[SilkGLFW] Frame buffer texture created: ID={TextureId}", _textureId);

            // Set up rendering pipeline
            if (!SetupRenderingPipeline())
            {
                _logger.LogError("[SilkGLFW] Failed to set up rendering pipeline");
                _gl.DeleteTexture(_textureId);
                _glfw.DestroyWindow(_window);
                _glfw.Terminate();
                return false;
            }

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

    private bool SetupRenderingPipeline()
    {
        if (_gl == null)
        {
            return false;
        }

        // Vertex shader source - simple passthrough with texture coordinates
        const string vertexShaderSource = @"
#version 330 core
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTexCoord;

out vec2 TexCoord;

void main()
{
    gl_Position = vec4(aPos.x, aPos.y, 0.0, 1.0);
    TexCoord = aTexCoord;
}
";

        // Fragment shader source - sample texture
        const string fragmentShaderSource = @"
#version 330 core
out vec4 FragColor;

in vec2 TexCoord;

uniform sampler2D texture1;

void main()
{
    FragColor = texture(texture1, TexCoord);
}
";

        // Compile vertex shader
        var vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, vertexShaderSource);
        _gl.CompileShader(vertexShader);

        // Check for vertex shader compile errors
        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out var success);
        if (success == 0)
        {
            var infoLog = _gl.GetShaderInfoLog(vertexShader);
            _logger.LogError("[SilkGLFW] Vertex shader compilation failed: {InfoLog}", infoLog);
            return false;
        }

        // Compile fragment shader
        var fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, fragmentShaderSource);
        _gl.CompileShader(fragmentShader);

        // Check for fragment shader compile errors
        _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out success);
        if (success == 0)
        {
            var infoLog = _gl.GetShaderInfoLog(fragmentShader);
            _logger.LogError("[SilkGLFW] Fragment shader compilation failed: {InfoLog}", infoLog);
            _gl.DeleteShader(vertexShader);
            return false;
        }

        // Link shaders into a program
        _shaderProgram = _gl.CreateProgram();
        _gl.AttachShader(_shaderProgram, vertexShader);
        _gl.AttachShader(_shaderProgram, fragmentShader);
        _gl.LinkProgram(_shaderProgram);

        // Check for linking errors
        _gl.GetProgram(_shaderProgram, ProgramPropertyARB.LinkStatus, out success);
        if (success == 0)
        {
            var infoLog = _gl.GetProgramInfoLog(_shaderProgram);
            _logger.LogError("[SilkGLFW] Shader program linking failed: {InfoLog}", infoLog);
            _gl.DeleteShader(vertexShader);
            _gl.DeleteShader(fragmentShader);
            return false;
        }

        // Clean up shaders (they're now linked into the program)
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        // Set up vertex data for a fullscreen quad
        // Two triangles forming a quad covering the entire screen
        // Format: x, y, texX, texY
        float[] vertices = new float[]
        {
            // Position      // TexCoords
            -1.0f,  1.0f,    0.0f, 0.0f,  // Top-left (Y tex flipped)
            -1.0f, -1.0f,    0.0f, 1.0f,  // Bottom-left (Y tex flipped)
             1.0f, -1.0f,    1.0f, 1.0f,  // Bottom-right (Y tex flipped)

            -1.0f,  1.0f,    0.0f, 0.0f,  // Top-left (Y tex flipped)
             1.0f, -1.0f,    1.0f, 1.0f,  // Bottom-right (Y tex flipped)
             1.0f,  1.0f,    1.0f, 0.0f   // Top-right (Y tex flipped)
        };

        // Create and configure VAO and VBO
        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();

        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        // Upload vertex data
        fixed (float* v = vertices)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
        }

        // Position attribute
        _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*)0);
        _gl.EnableVertexAttribArray(0);

        // Texture coordinate attribute
        _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*)(2 * sizeof(float)));
        _gl.EnableVertexAttribArray(1);

        // Unbind
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindVertexArray(0);

        _logger.LogInformation("[SilkGLFW] Rendering pipeline set up successfully");
        return true;
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
                _logger.LogWarning("[SilkGLFW] UpdateFrameBuffer called but backend not initialized");
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

            // Clear the screen
            _gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            _gl.Clear(ClearBufferMask.ColorBufferBit);
            
            // Use our shader program
            _gl.UseProgram(_shaderProgram);
            
            // Set the 'texture1' uniform to use texture unit 0
            int textureUniformLocation = _gl.GetUniformLocation(_shaderProgram, "texture1");
            _gl.Uniform1(textureUniformLocation, 0);
            
            // Bind texture
            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2D, _textureId);
            
            // Render the quad
            _gl.BindVertexArray(_vao);
            _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
            _gl.BindVertexArray(0);
            
            // Swap buffers
            _glfw.SwapBuffers(_window);
            
            _logger.LogDebug("[SilkGLFW] Frame buffer updated and rendered to screen");

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
            
            _logger.LogDebug("[SilkGLFW] Screen cleared to color ({R}, {G}, {B}, {A})", r, g, b, a);
        }
    }

    public void ProcessEvents()
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                _logger.LogDebug("[SilkGLFW] ProcessEvents called but backend not initialized");
                return;
            }

            _glfw.PollEvents();
            _logger.LogDebug("[SilkGLFW] Events polled");
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

            if (_gl != null)
            {
                if (_textureId != 0)
                {
                    _gl.DeleteTexture(_textureId);
                    _textureId = 0;
                }
                
                if (_vao != 0)
                {
                    _gl.DeleteVertexArray(_vao);
                    _vao = 0;
                }
                
                if (_vbo != 0)
                {
                    _gl.DeleteBuffer(_vbo);
                    _vbo = 0;
                }
                
                if (_shaderProgram != 0)
                {
                    _gl.DeleteProgram(_shaderProgram);
                    _shaderProgram = 0;
                }
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
