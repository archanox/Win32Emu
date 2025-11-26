using Microsoft.Extensions.Logging;
using Silk.NET.GLFW;
using Silk.NET.OpenGL;
using System.Runtime.InteropServices;
using System.Buffers;

namespace Win32Emu.Gui.Backends;
using Win32Emu.Rendering;
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
    private readonly Lock _lock = new();
    private GlfwCallbacks.ErrorCallback? _errorCallback;
    
    // OpenGL rendering pipeline components
    private uint _shaderProgram;
    private uint _vao;
    private uint _vbo;
    private uint _ebo; // Element buffer object for indices
    
    // Hardware acceleration state
    private uint _hwAccelShaderProgram;
    private uint _hwAccelVao;
    private uint _hwAccelVbo;
    private uint _hwAccelEbo;
    private readonly Dictionary<uint, uint> _textures = new(); // Texture ID -> GL texture
    private uint _currentBoundTexture;
    private BlendMode _currentBlendMode = BlendMode.Disabled;
    private DepthTest _currentDepthTest = DepthTest.Disabled;
    private CullMode _currentCullMode = CullMode.None;

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
            
            // On macOS, forward compatibility must be enabled for OpenGL 3.2 Core Profile
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

            // Set up rendering pipeline for legacy frame buffer mode
            if (!SetupRenderingPipeline())
            {
                _logger.LogError("[SilkGLFW] Failed to set up rendering pipeline");
                _gl.DeleteTexture(_textureId);
                _glfw.DestroyWindow(_window);
                _glfw.Terminate();
                return false;
            }

            // Set up hardware acceleration pipeline for Glide2x
            if (!SetupHardwareAccelerationPipeline())
            {
                _logger.LogError("[SilkGLFW] Failed to set up hardware acceleration pipeline");
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

            // Set up keyboard callback
            _glfw.SetKeyCallback(_window, (window, key, scancode, action, mods) =>
            {
                bool pressed = action == InputAction.Press || action == InputAction.Repeat;
                SilkInputBackend.UpdateKeyState((int)key, pressed);
            });

            // Set up mouse button callback
            _glfw.SetMouseButtonCallback(_window, (window, button, action, mods) =>
            {
                bool pressed = action == InputAction.Press;
                SilkInputBackend.UpdateMouseButton((int)button, pressed);
            });

            // Set up cursor position callback
            _glfw.SetCursorPosCallback(_window, (window, xpos, ypos) =>
            {
                SilkInputBackend.UpdateMousePosition((int)xpos, (int)ypos);
            });

            // Set up scroll callback
            _glfw.SetScrollCallback(_window, (window, xoffset, yoffset) =>
            {
                SilkInputBackend.UpdateMouseWheel((int)yoffset);
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

    private bool SetupHardwareAccelerationPipeline()
    {
        if (_gl == null)
        {
            return false;
        }

        // Vertex shader for hardware-accelerated rendering (Glide2x)
        const string hwAccelVertexShaderSource = @"
#version 330 core
layout (location = 0) in vec3 aPos;     // Position (x, y, z)
layout (location = 1) in vec4 aColor;   // Color (r, g, b, a)
layout (location = 2) in vec2 aTexCoord;// Texture coordinates (u, v)
layout (location = 3) in float aOow;    // 1/w for perspective correction

out vec4 Color;
out vec2 TexCoord;
out float Oow;

uniform mat4 projection;

void main()
{
    // Transform to normalized device coordinates
    // Convert from screen space to NDC: x: [0, width] -> [-1, 1], y: [0, height] -> [1, -1]
    gl_Position = projection * vec4(aPos, 1.0);
    Color = aColor;
    TexCoord = aTexCoord;
    Oow = aOow;
}
";

        // Fragment shader for hardware-accelerated rendering
        const string hwAccelFragmentShaderSource = @"
#version 330 core
out vec4 FragColor;

in vec4 Color;
in vec2 TexCoord;
in float Oow;

uniform sampler2D texture1;
uniform bool useTexture;

void main()
{
    if (useTexture)
    {
        // Perspective-correct texture sampling
        vec2 correctedTexCoord = TexCoord * Oow;
        FragColor = texture(texture1, correctedTexCoord) * Color;
    }
    else
    {
        // Just use vertex color (Gouraud shading)
        FragColor = Color;
    }
}
";

        // Compile vertex shader
        var vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, hwAccelVertexShaderSource);
        _gl.CompileShader(vertexShader);

        // Check for vertex shader compile errors
        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out var success);
        if (success == 0)
        {
            var infoLog = _gl.GetShaderInfoLog(vertexShader);
            _logger.LogError("[SilkGLFW] HW Accel vertex shader compilation failed: {InfoLog}", infoLog);
            return false;
        }

        // Compile fragment shader
        var fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, hwAccelFragmentShaderSource);
        _gl.CompileShader(fragmentShader);

        // Check for fragment shader compile errors
        _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out success);
        if (success == 0)
        {
            var infoLog = _gl.GetShaderInfoLog(fragmentShader);
            _logger.LogError("[SilkGLFW] HW Accel fragment shader compilation failed: {InfoLog}", infoLog);
            _gl.DeleteShader(vertexShader);
            return false;
        }

        // Link shaders into a program
        _hwAccelShaderProgram = _gl.CreateProgram();
        _gl.AttachShader(_hwAccelShaderProgram, vertexShader);
        _gl.AttachShader(_hwAccelShaderProgram, fragmentShader);
        _gl.LinkProgram(_hwAccelShaderProgram);

        // Check for linking errors
        _gl.GetProgram(_hwAccelShaderProgram, ProgramPropertyARB.LinkStatus, out success);
        if (success == 0)
        {
            var infoLog = _gl.GetProgramInfoLog(_hwAccelShaderProgram);
            _logger.LogError("[SilkGLFW] HW Accel shader program linking failed: {InfoLog}", infoLog);
            _gl.DeleteShader(vertexShader);
            _gl.DeleteShader(fragmentShader);
            return false;
        }

        // Clean up shaders
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        // Create VAO, VBO, and EBO for hardware-accelerated rendering
        _hwAccelVao = _gl.GenVertexArray();
        _hwAccelVbo = _gl.GenBuffer();
        _hwAccelEbo = _gl.GenBuffer();

        _gl.BindVertexArray(_hwAccelVao);

        // Bind VBO (will be filled dynamically during rendering)
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _hwAccelVbo);

        // Vertex format: position (3 floats), color (4 floats), texcoord (2 floats), oow (1 float)
        // Total: 10 floats per vertex
        var stride = 10 * sizeof(float);

        // Position attribute (location = 0)
        _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)stride, (void*)0);
        _gl.EnableVertexAttribArray(0);

        // Color attribute (location = 1)
        _gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, (uint)stride, (void*)(3 * sizeof(float)));
        _gl.EnableVertexAttribArray(1);

        // Texture coordinate attribute (location = 2)
        _gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, (uint)stride, (void*)(7 * sizeof(float)));
        _gl.EnableVertexAttribArray(2);

        // Oow attribute (location = 3)
        _gl.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, (uint)stride, (void*)(9 * sizeof(float)));
        _gl.EnableVertexAttribArray(3);

        // Bind EBO to the VAO (will be filled dynamically during rendering)
        // Note: Binding the EBO after the VAO is already bound will associate it with the VAO.
        // This association is persistent - the VAO "remembers" which EBO is bound to it.
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _hwAccelEbo);

        // Unbind
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindVertexArray(0);

        _logger.LogInformation("[SilkGLFW] Hardware acceleration pipeline set up successfully");
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
            //_logger.LogDebug("[SilkGLFW] Events polled");

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
                // Clean up legacy frame buffer resources
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

                // Clean up hardware acceleration resources
                if (_hwAccelVao != 0)
                {
                    _gl.DeleteVertexArray(_hwAccelVao);
                    _hwAccelVao = 0;
                }
                
                if (_hwAccelVbo != 0)
                {
                    _gl.DeleteBuffer(_hwAccelVbo);
                    _hwAccelVbo = 0;
                }
                
                if (_hwAccelEbo != 0)
                {
                    _gl.DeleteBuffer(_hwAccelEbo);
                    _hwAccelEbo = 0;
                }
                
                if (_hwAccelShaderProgram != 0)
                {
                    _gl.DeleteProgram(_hwAccelShaderProgram);
                    _hwAccelShaderProgram = 0;
                }

                // Clean up all textures
                foreach (var glTextureId in _textures.Values)
                {
                    _gl.DeleteTexture(glTextureId);
                }
                _textures.Clear();
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

    // Hardware-accelerated rendering methods (OpenGL implementation)
    
    public void BeginFrame()
    {
        if (_gl == null || !_initialized)
        {
            return;
        }

        lock (_lock)
        {
            _logger.LogDebug("[SilkGLFW] BeginFrame: Clearing buffers");
            
            // Clear the color and depth buffers
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            // Use hardware acceleration shader program
            _gl.UseProgram(_hwAccelShaderProgram);
            
            // Set up projection matrix to convert from screen space to NDC
            // Screen space: x: [0, width], y: [0, height]
            // NDC: x: [-1, 1], y: [1, -1] (Y inverted)
            var projectionMatrix = new float[16];
            projectionMatrix[0] = 2.0f / _width;   // Scale X
            projectionMatrix[5] = -2.0f / _height; // Scale Y (inverted)
            projectionMatrix[10] = 1.0f;           // Scale Z
            projectionMatrix[12] = -1.0f;          // Translate X
            projectionMatrix[13] = 1.0f;           // Translate Y
            projectionMatrix[15] = 1.0f;           // W

            var projectionLoc = _gl.GetUniformLocation(_hwAccelShaderProgram, "projection");
            if (projectionLoc >= 0)
            {
                _gl.UniformMatrix4(projectionLoc, 1, false, projectionMatrix);
            }
        }
    }

    public void EndFrame()
    {
        if (_gl == null || !_initialized || _window == null)
        {
            return;
        }

        lock (_lock)
        {
            _logger.LogDebug("[SilkGLFW] EndFrame: Swapping buffers");
            
            // Swap front and back buffers
            _glfw.SwapBuffers(_window);
        }
    }

    public void DrawTriangles(Span<Vertex> vertices, Span<ushort> indices)
    {
        if (_gl == null || !_initialized)
        {
            _logger.LogWarning("[SilkGLFW] DrawTriangles called but not initialized");
            return;
        }

        lock (_lock)
        {
            _logger.LogDebug("[SilkGLFW] DrawTriangles: {VertexCount} vertices, {IndexCount} indices", 
                vertices.Length, indices.Length);

            // Use hardware acceleration shader program
            _gl.UseProgram(_hwAccelShaderProgram);

            // Bind VAO
            _gl.BindVertexArray(_hwAccelVao);

            // Convert vertices to float array
            // Format: position (3), color (4), texcoord (2), oow (1) = 10 floats per vertex
            // Use ArrayPool to avoid repeated allocations
            var vertexDataLength = vertices.Length * 10;
            var vertexData = ArrayPool<float>.Shared.Rent(vertexDataLength);
            
            try
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    var v = vertices[i];
                    var offset = i * 10;
                    
                    // Position
                    vertexData[offset + 0] = v.Position.X;
                    vertexData[offset + 1] = v.Position.Y;
                    vertexData[offset + 2] = v.Position.Z;
                    
                    // Color
                    vertexData[offset + 3] = v.Color.X;
                    vertexData[offset + 4] = v.Color.Y;
                    vertexData[offset + 5] = v.Color.Z;
                    vertexData[offset + 6] = v.Color.W;
                    
                    // Texture coordinates
                    vertexData[offset + 7] = v.TexCoord.X;
                    vertexData[offset + 8] = v.TexCoord.Y;
                    
                    // Oow (1/w)
                    vertexData[offset + 9] = v.Oow;
                }

                // Upload vertex data
                _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _hwAccelVbo);
                fixed (float* vData = vertexData)
                {
                    _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertexDataLength * sizeof(float)), 
                                  vData, BufferUsageARB.DynamicDraw);
                }

                // Upload index data
                _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _hwAccelEbo);
                fixed (ushort* iData = indices)
                {
                    _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(ushort)), 
                                  iData, BufferUsageARB.DynamicDraw);
                }

                // Set texture usage uniform
                var useTextureLoc = _gl.GetUniformLocation(_hwAccelShaderProgram, "useTexture");
                if (useTextureLoc >= 0)
                {
                    _gl.Uniform1(useTextureLoc, _currentBoundTexture != 0 ? 1 : 0);
                }

                // Draw triangles
                _gl.DrawElements(PrimitiveType.Triangles, (uint)indices.Length, DrawElementsType.UnsignedShort, null);
            }
            finally
            {
                // Return the rented array to the pool
                ArrayPool<float>.Shared.Return(vertexData);
            }
        }
    }

    public void SetTexture(uint textureId, byte[] data, int width, int height, TextureFormat format)
    {
        if (_gl == null || !_initialized)
        {
            _logger.LogWarning("[SilkGLFW] SetTexture called but not initialized");
            return;
        }

        lock (_lock)
        {
            _logger.LogDebug("[SilkGLFW] SetTexture: ID={TextureId}, Size={Width}x{Height}, Format={Format}", 
                textureId, width, height, format);

            // Create or update texture
            if (!_textures.TryGetValue(textureId, out var glTextureId))
            {
                // Create new texture
                glTextureId = _gl.GenTexture();
                _textures[textureId] = glTextureId;
            }

            _gl.BindTexture(TextureTarget.Texture2D, glTextureId);

            // Set texture parameters
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);

            // Upload texture data based on format
            fixed (byte* dataPtr = data)
            {
                switch (format)
                {
                    case TextureFormat.RGBA8:
                        _gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba8, 
                                      (uint)width, (uint)height, 0, PixelFormat.Rgba, 
                                      PixelType.UnsignedByte, dataPtr);
                        break;

                    case TextureFormat.RGB565:
                        _gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgb, 
                                      (uint)width, (uint)height, 0, PixelFormat.Rgb, 
                                      PixelType.UnsignedShort565, dataPtr);
                        break;

                    case TextureFormat.RGB24:
                        _gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgb8, 
                                      (uint)width, (uint)height, 0, PixelFormat.Rgb, 
                                      PixelType.UnsignedByte, dataPtr);
                        break;

                    case TextureFormat.Palettized8:
                        _logger.LogWarning("[SilkGLFW] Palettized8 format not supported, convert to RGBA first");
                        break;
                }
            }

            _gl.BindTexture(TextureTarget.Texture2D, 0);
        }
    }

    public void BindTexture(uint textureId)
    {
        if (_gl == null || !_initialized)
        {
            return;
        }

        lock (_lock)
        {
            _logger.LogDebug("[SilkGLFW] BindTexture: ID={TextureId}", textureId);

            _currentBoundTexture = textureId;

            if (textureId == 0)
            {
                // Unbind texture
                _gl.BindTexture(TextureTarget.Texture2D, 0);
            }
            else if (_textures.TryGetValue(textureId, out var glTextureId))
            {
                // Bind texture
                _gl.ActiveTexture(TextureUnit.Texture0);
                _gl.BindTexture(TextureTarget.Texture2D, glTextureId);
            }
            else
            {
                _logger.LogWarning("[SilkGLFW] Texture ID {TextureId} not found", textureId);
            }
        }
    }

    public void SetRenderState(BlendMode blend, DepthTest depth, CullMode cull)
    {
        if (_gl == null || !_initialized)
        {
            return;
        }

        lock (_lock)
        {
            _logger.LogDebug("[SilkGLFW] SetRenderState: Blend={Blend}, Depth={Depth}, Cull={Cull}", 
                blend, depth, cull);

            // Set blend mode
            if (blend != _currentBlendMode)
            {
                switch (blend)
                {
                    case BlendMode.Disabled:
                        _gl.Disable(EnableCap.Blend);
                        break;

                    case BlendMode.Alpha:
                        _gl.Enable(EnableCap.Blend);
                        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                        break;

                    case BlendMode.Additive:
                        _gl.Enable(EnableCap.Blend);
                        _gl.BlendFunc(BlendingFactor.One, BlendingFactor.One);
                        break;

                    case BlendMode.Multiplicative:
                        _gl.Enable(EnableCap.Blend);
                        _gl.BlendFunc(BlendingFactor.DstColor, BlendingFactor.Zero);
                        break;
                }
                _currentBlendMode = blend;
            }

            // Set depth test
            if (depth != _currentDepthTest)
            {
                switch (depth)
                {
                    case DepthTest.Disabled:
                        _gl.Disable(EnableCap.DepthTest);
                        break;

                    case DepthTest.Always:
                        _gl.Enable(EnableCap.DepthTest);
                        _gl.DepthFunc(DepthFunction.Always);
                        break;

                    case DepthTest.Less:
                        _gl.Enable(EnableCap.DepthTest);
                        _gl.DepthFunc(DepthFunction.Less);
                        break;

                    case DepthTest.LessEqual:
                        _gl.Enable(EnableCap.DepthTest);
                        _gl.DepthFunc(DepthFunction.Lequal);
                        break;

                    case DepthTest.Greater:
                        _gl.Enable(EnableCap.DepthTest);
                        _gl.DepthFunc(DepthFunction.Greater);
                        break;

                    case DepthTest.GreaterEqual:
                        _gl.Enable(EnableCap.DepthTest);
                        _gl.DepthFunc(DepthFunction.Gequal);
                        break;

                    case DepthTest.Equal:
                        _gl.Enable(EnableCap.DepthTest);
                        _gl.DepthFunc(DepthFunction.Equal);
                        break;

                    case DepthTest.NotEqual:
                        _gl.Enable(EnableCap.DepthTest);
                        _gl.DepthFunc(DepthFunction.Notequal);
                        break;
                }
                _currentDepthTest = depth;
            }

            // Set cull mode
            if (cull != _currentCullMode)
            {
                switch (cull)
                {
                    case CullMode.None:
                        _gl.Disable(EnableCap.CullFace);
                        break;

                    case CullMode.Front:
                        _gl.Enable(EnableCap.CullFace);
                        _gl.CullFace(TriangleFace.Front);
                        break;

                    case CullMode.Back:
                        _gl.Enable(EnableCap.CullFace);
                        _gl.CullFace(TriangleFace.Back);
                        break;
                }
                _currentCullMode = cull;
            }
        }
    }

    public void DeleteTexture(uint textureId)
    {
        if (_gl == null || !_initialized)
        {
            return;
        }

        lock (_lock)
        {
            _logger.LogDebug("[SilkGLFW] DeleteTexture: ID={TextureId}", textureId);

            if (_textures.TryGetValue(textureId, out var glTextureId))
            {
                _gl.DeleteTexture(glTextureId);
                _textures.Remove(textureId);
            }
        }
    }
}
