using Microsoft.Extensions.Logging;
using Win32Emu.Rendering;
using Silk.NET.GLFW;
using SharpMetal.Metal;
using SharpMetal.QuartzCore;
using SharpMetal.Foundation;
using SharpMetal.ObjectiveCCore;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Win32Emu.Gui.Backends;
#if MACOS
// P/Invoke for GLFW native Cocoa functions
internal static partial class GlfwNative
{
    [LibraryImport("glfw3", EntryPoint = "glfwGetCocoaWindow")]
    public static partial IntPtr GetCocoaWindow(IntPtr window);
}
#endif
/// <summary>
/// SharpMetal-based rendering backend for DirectDraw and GDI operations on macOS
/// </summary>
[SupportedOSPlatform("macos")]
public unsafe class SharpMetalRenderingBackend : IRenderingBackend
    private readonly ILogger _logger;
    private readonly Glfw _glfw;
    private WindowHandle* _window;
    private MTLDevice _device;
    private MTLCommandQueue _commandQueue;
    private CAMetalLayer _metalLayer;
    private MTLTexture _frameTexture;
    private MTLRenderPipelineState _pipelineState;
    private MTLBuffer _vertexBuffer;
    private bool _initialized;
    private int _width;
    private int _height;
    private readonly Lock _lock = new();
    private GlfwCallbacks.ErrorCallback? _errorCallback;
    /// <summary>
    /// Event fired when a UI event occurs (mouse, keyboard, window)
    /// </summary>
    public event EventHandler<UIEventArgs>? UIEvent;
    public SharpMetalRenderingBackend(ILogger logger)
    {
        _logger = logger;
        _glfw = Glfw.GetApi();
    }
    public bool Initialize(int width, int height, string title = "Win32Emu Display")
        lock (_lock)
        {
            if (_initialized)
            {
                return true;
            }
            _width = width;
            _height = height;
            try
                // Link Metal frameworks
                ObjectiveC.LinkMetal();
                ObjectiveC.LinkCoreGraphics();
                // Set up GLFW error callback
                _errorCallback = (Silk.NET.GLFW.ErrorCode error, string description) =>
                {
                    _logger.LogError("[SharpMetal] GLFW Error {ErrorCode}: {Description}", error, description);
                };
                _glfw.SetErrorCallback(_errorCallback);
                // Initialize GLFW
                _logger.LogInformation("[SharpMetal] Initializing GLFW...");
                if (!_glfw.Init())
                    _logger.LogError("[SharpMetal] Failed to initialize GLFW");
                    return false;
                }
                _logger.LogInformation("[SharpMetal] GLFW initialized successfully");
                // Set window hints for Cocoa (macOS)
                _logger.LogInformation("[SharpMetal] Setting window hints for Metal rendering...");
                _glfw.WindowHint(WindowHintClientApi.ClientApi, ClientApi.NoApi);
                _glfw.WindowHint(WindowHintBool.Resizable, true);
                // Create window
                _logger.LogInformation("[SharpMetal] Creating window: {Width}x{Height} - '{Title}'", width, height, title);
                _window = _glfw.CreateWindow(width, height, title, null, null);
                if (_window == null)
                    _logger.LogError("[SharpMetal] Failed to create window");
                    _glfw.Terminate();
                _logger.LogInformation("[SharpMetal] Window created successfully");
                // Create Metal device
                _logger.LogInformation("[SharpMetal] Creating Metal device...");
                _device = MTLDevice.CreateSystemDefaultDevice();
                if (_device == null)
                    _logger.LogError("[SharpMetal] Failed to create Metal device");
                    _glfw.DestroyWindow(_window);
                _logger.LogInformation("[SharpMetal] Metal device created: {DeviceName}", 
                    (IntPtr)_device.Name != IntPtr.Zero ? _device.Name.ToString() : "Unknown");
                // Create command queue
                _commandQueue = _device.NewCommandQueue();
                if (_commandQueue == IntPtr.Zero)
                    _logger.LogError("[SharpMetal] Failed to create command queue");
                // Get the NSView from GLFW window and attach Metal layer
                if (!SetupMetalLayer())
                    _logger.LogError("[SharpMetal] Failed to set up Metal layer");
                    _commandQueue.Dispose();
                    _device.Dispose();
                // Create frame texture
                if (!CreateFrameTexture())
                    _logger.LogError("[SharpMetal] Failed to create frame texture");
                    if ((IntPtr)_metalLayer != IntPtr.Zero)
                    {
	                    _metalLayer.Dispose();
                    }
                // Set up rendering pipeline
                if (!SetupRenderingPipeline())
                    _logger.LogError("[SharpMetal] Failed to set up rendering pipeline");
                    if ((IntPtr)_frameTexture != IntPtr.Zero)
	                    _frameTexture.Dispose();
                // Set up window callbacks for lifecycle events
                _glfw.SetWindowFocusCallback(_window, (window, focused) =>
                    if (focused)
                        _logger.LogDebug("[SharpMetal] Window gained focus, firing WindowActivate event");
                        OnUIEvent(new UIEventArgs
                        {
                            EventType = UIEventType.WindowActivate,
                            WindowHandle = 0
                        });
                    else
                        _logger.LogDebug("[SharpMetal] Window lost focus, firing WindowDeactivate event");
                            EventType = UIEventType.WindowDeactivate,
                });
                _initialized = true;
                _logger.LogInformation("[SharpMetal] Initialized {Width}x{Height} Metal display", width, height);
            catch (Exception ex)
                _logger.LogError(ex, "[SharpMetal] Failed to initialize Metal backend");
                return false;
        }
    private bool SetupMetalLayer()
        if (_window == null)
            return false;
        try
            // Get the NSView from GLFW window (macOS specific)
            var nsView = GlfwNative.GetCocoaWindow((IntPtr)_window);
            if (nsView == IntPtr.Zero)
                _logger.LogError("[SharpMetal] Failed to get NSView from GLFW window");
            // Create CAMetalLayer using ObjectiveC
            var metalLayerClass = new ObjectiveCClass("CAMetalLayer");
            var metalLayerPtr = metalLayerClass.Alloc();
            metalLayerPtr = ObjectiveC.IntPtr_objc_msgSend(metalLayerPtr, "init");
            _metalLayer = new CAMetalLayer(metalLayerPtr);
            
            // Set device
            _metalLayer.Device = _device;
            _metalLayer.PixelFormat = MTLPixelFormat.RGBA8Unorm;
            _metalLayer.FramebufferOnly = true;
            // Set drawable size using NSMakeSize
            // DrawableSize expects an IntPtr that represents a CGSize struct
            // CGSize is two doubles: width and height
            var sizeBytes = new byte[16]; // 2 * sizeof(double)
            BitConverter.GetBytes((double)_width).CopyTo(sizeBytes, 0);
            BitConverter.GetBytes((double)_height).CopyTo(sizeBytes, 8);
            unsafe
                fixed (byte* sizePtr = sizeBytes)
                    _metalLayer.DrawableSize = (IntPtr)sizePtr;
            // Set the Metal layer as the layer of the NSView
            ObjectiveC.objc_msgSend(nsView, "setWantsLayer:", true);
            ObjectiveC.objc_msgSend(nsView, "setLayer:", (IntPtr)_metalLayer);
            _logger.LogInformation("[SharpMetal] Metal layer attached to window view");
            return true;
#else
            _logger.LogError("[SharpMetal] SharpMetal backend is only supported on macOS");
        catch (Exception ex)
            _logger.LogError(ex, "[SharpMetal] Failed to set up Metal layer");
    private bool CreateFrameTexture()
            var descriptor = new MTLTextureDescriptor();
            descriptor.TextureType = MTLTextureType.Type2D;
            descriptor.PixelFormat = MTLPixelFormat.RGBA8Unorm;
            descriptor.Width = (ulong)_width;
            descriptor.Height = (ulong)_height;
            descriptor.Usage = MTLTextureUsage.ShaderRead | MTLTextureUsage.RenderTarget;
            _frameTexture = _device.NewTexture(descriptor);
            // Check if creation succeeded by verifying the NativePtr
            if ((IntPtr)_frameTexture == IntPtr.Zero)
                _logger.LogError("[SharpMetal] Failed to create frame texture");
            _logger.LogInformation("[SharpMetal] Frame texture created: {Width}x{Height}", _width, _height);
            _logger.LogError(ex, "[SharpMetal] Failed to create frame texture");
    private bool SetupRenderingPipeline()
            // Create a simple shader for texture rendering
            const string shaderSource = @"
#include <metal_stdlib>
using namespace metal;
struct VertexOut {
    float4 position [[position]];
    float2 texCoord;
};
vertex VertexOut vertexShader(uint vertexID [[vertex_id]],
                              constant float4 *positions [[buffer(0)]]) {
    VertexOut out;
    out.position = positions[vertexID];
    
    // Calculate texture coordinates based on position
    out.texCoord = float2((out.position.x + 1.0) * 0.5, (1.0 - out.position.y) * 0.5);
    return out;
fragment float4 fragmentShader(VertexOut in [[stage_in]],
                               texture2d<float> colorTexture [[texture(0)]]) {
    constexpr sampler textureSampler(mag_filter::linear, min_filter::linear);
    return colorTexture.sample(textureSampler, in.texCoord);
";
            var compileOptions = new MTLCompileOptions(IntPtr.Zero);
            var shaderSourceNS = NSString.String(shaderSource);
            NSError error = default;
            var library = _device.NewLibrary(shaderSourceNS, compileOptions, ref error);
            if ((IntPtr)library == IntPtr.Zero || (IntPtr)error != IntPtr.Zero)
                _logger.LogError("[SharpMetal] Failed to compile shaders: {Error}", 
                    (IntPtr)error != IntPtr.Zero ? error.LocalizedDescription.ToString() : "Unknown error");
            var vertexFunction = library.NewFunction(NSString.String("vertexShader"));
            var fragmentFunction = library.NewFunction(NSString.String("fragmentShader"));
            if ((IntPtr)vertexFunction == IntPtr.Zero || (IntPtr)fragmentFunction == IntPtr.Zero)
                _logger.LogError("[SharpMetal] Failed to get shader functions");
                library.Dispose();
            // Create pipeline state descriptor
            var pipelineDescriptor = new MTLRenderPipelineDescriptor();
            pipelineDescriptor.VertexFunction = vertexFunction;
            pipelineDescriptor.FragmentFunction = fragmentFunction;
            var colorAttachment = pipelineDescriptor.ColorAttachments.Object(0);
            colorAttachment.PixelFormat = MTLPixelFormat.RGBA8Unorm;
            pipelineDescriptor.ColorAttachments.SetObject(colorAttachment, 0);
            NSError pipelineError = default;
            _pipelineState = _device.NewRenderPipelineState(pipelineDescriptor, ref pipelineError);
            if ((IntPtr)_pipelineState == IntPtr.Zero || (IntPtr)pipelineError != IntPtr.Zero)
                _logger.LogError("[SharpMetal] Failed to create pipeline state: {Error}", 
                    (IntPtr)pipelineError != IntPtr.Zero ? pipelineError.LocalizedDescription.ToString() : "Unknown error");
                vertexFunction.Dispose();
                fragmentFunction.Dispose();
            // Create vertex buffer for fullscreen quad
            var vertices = new float[]
                // Position (x, y, z, w)
                -1.0f,  1.0f, 0.0f, 1.0f,  // Top-left
                -1.0f, -1.0f, 0.0f, 1.0f,  // Bottom-left
                 1.0f, -1.0f, 0.0f, 1.0f,  // Bottom-right
                
                 1.0f,  1.0f, 0.0f, 1.0f   // Top-right
            };
            var bufferSize = (ulong)(vertices.Length * sizeof(float));
            _vertexBuffer = _device.NewBuffer(bufferSize, MTLResourceOptions.ResourceStorageModeManaged);
            if ((IntPtr)_vertexBuffer == IntPtr.Zero)
                _logger.LogError("[SharpMetal] Failed to create vertex buffer");
                pipelineDescriptor.Dispose();
            // Copy vertex data to buffer
            Marshal.Copy(vertices, 0, _vertexBuffer.Contents, vertices.Length);
            _vertexBuffer.DidModifyRange(new NSRange { location = 0, length = _vertexBuffer.Length });
            // Clean up temporary objects
            library.Dispose();
            vertexFunction.Dispose();
            fragmentFunction.Dispose();
            pipelineDescriptor.Dispose();
            _logger.LogInformation("[SharpMetal] Rendering pipeline set up successfully");
            _logger.LogError(ex, "[SharpMetal] Failed to set up rendering pipeline");
    public byte[] ConvertPalettizedToRGBA(byte[] indexedData, uint[] palette, int width, int height, int pitch)
        var rgbaData = new byte[width * height * 4];
        
        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
                var srcOffset = y * pitch + x;
                var dstOffset = (y * width + x) * 4;
                if (srcOffset < indexedData.Length)
                    var paletteIndex = indexedData[srcOffset];
                    
                    if (paletteIndex < palette.Length)
                        var color = palette[paletteIndex];
                        
                        rgbaData[dstOffset + 0] = (byte)(color & 0xFF);         // R
                        rgbaData[dstOffset + 1] = (byte)((color >> 8) & 0xFF);  // G
                        rgbaData[dstOffset + 2] = (byte)((color >> 16) & 0xFF); // B
                        rgbaData[dstOffset + 3] = 0xFF;                          // A
        return rgbaData;
    public byte[] Convert16BitToRGBA(byte[] rgb565Data, int width, int height, int pitch)
                var srcOffset = y * pitch + x * 2;
                if (srcOffset + 1 < rgb565Data.Length)
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
    public byte[] Convert24BitToRGBA(byte[] rgb24Data, int width, int height, int pitch)
                var srcOffset = y * pitch + x * 3;
                if (srcOffset + 2 < rgb24Data.Length)
                    // 24-bit is typically BGR format in Windows
                    rgbaData[dstOffset + 0] = rgb24Data[srcOffset + 2]; // R
                    rgbaData[dstOffset + 1] = rgb24Data[srcOffset + 1]; // G
                    rgbaData[dstOffset + 2] = rgb24Data[srcOffset + 0]; // B
                    rgbaData[dstOffset + 3] = 0xFF;                      // A
    public bool UpdateFrameBuffer(byte[] data, int pitch)
            if (!_initialized || (IntPtr)_commandQueue == IntPtr.Zero || (IntPtr)_metalLayer == IntPtr.Zero || 
                (IntPtr)_frameTexture == IntPtr.Zero || (IntPtr)_pipelineState == IntPtr.Zero)
                _logger.LogWarning("[SharpMetal] UpdateFrameBuffer called but backend not initialized");
                // Update frame texture with new data
                var region = new MTLRegion
                    origin = new MTLOrigin { x = 0, y = 0, z = 0 },
                    size = new MTLSize { width = (ulong)_width, height = (ulong)_height, depth = 1 }
                fixed (byte* dataPtr = data)
                    _frameTexture.ReplaceRegion(region, 0, (IntPtr)dataPtr, (ulong)(_width * 4));
                // Get next drawable from layer
                var drawable = _metalLayer.NextDrawable;
                if ((IntPtr)drawable == IntPtr.Zero)
                    _logger.LogWarning("[SharpMetal] Failed to get next drawable");
                // Create command buffer
                var commandBuffer = _commandQueue.CommandBuffer();
                if ((IntPtr)commandBuffer == IntPtr.Zero)
                    _logger.LogWarning("[SharpMetal] Failed to create command buffer");
                // Create render pass descriptor
                var renderPassDescriptor = new MTLRenderPassDescriptor();
                var colorAttachment = renderPassDescriptor.ColorAttachments.Object(0);
                colorAttachment.Texture = drawable.Texture;
                colorAttachment.LoadAction = MTLLoadAction.Clear;
                colorAttachment.StoreAction = MTLStoreAction.Store;
                colorAttachment.ClearColor = new MTLClearColor { red = 0.0, green = 0.0, blue = 0.0, alpha = 1.0 };
                renderPassDescriptor.ColorAttachments.SetObject(colorAttachment, 0);
                // Create render command encoder
                var encoder = commandBuffer.RenderCommandEncoder(renderPassDescriptor);
                if ((IntPtr)encoder == IntPtr.Zero)
                    _logger.LogWarning("[SharpMetal] Failed to create render command encoder");
                    renderPassDescriptor.Dispose();
                    commandBuffer.Dispose();
                    drawable.Dispose();
                // Set pipeline state and render
                encoder.SetRenderPipelineState(_pipelineState);
                encoder.SetVertexBuffer(_vertexBuffer, 0, 0);
                encoder.SetFragmentTexture(_frameTexture, 0);
                encoder.DrawPrimitives(MTLPrimitiveType.Triangle, 0, 6);
                encoder.EndEncoding();
                // Present drawable and commit
                commandBuffer.PresentDrawable(drawable);
                commandBuffer.Commit();
                // Clean up
                encoder.Dispose();
                renderPassDescriptor.Dispose();
                commandBuffer.Dispose();
                drawable.Dispose();
                _logger.LogDebug("[SharpMetal] Frame buffer updated and rendered to screen");
                _logger.LogError(ex, "[SharpMetal] Failed to update frame buffer");
    public void Clear(byte r, byte g, byte b, byte a = 255)
            if (!_initialized || (IntPtr)_commandQueue == IntPtr.Zero || (IntPtr)_metalLayer == IntPtr.Zero)
                return;
                    return;
                colorAttachment.ClearColor = new MTLClearColor 
                { 
                    red = r / 255.0, 
                    green = g / 255.0, 
                    blue = b / 255.0, 
                    alpha = a / 255.0 
                if ((IntPtr)encoder != IntPtr.Zero)
                    encoder.EndEncoding();
                    encoder.Dispose();
                _logger.LogDebug("[SharpMetal] Screen cleared to color ({R}, {G}, {B}, {A})", r, g, b, a);
                _logger.LogError(ex, "[SharpMetal] Failed to clear screen");
    public void ProcessEvents()
            if (!_initialized)
                _logger.LogDebug("[SharpMetal] ProcessEvents called but backend not initialized");
            _glfw.PollEvents();
            _logger.LogDebug("[SharpMetal] Events polled");
    protected virtual void OnUIEvent(UIEventArgs e)
        UIEvent?.Invoke(this, e);
    public void Dispose()
            if ((IntPtr)_vertexBuffer != IntPtr.Zero)
	            _vertexBuffer.Dispose();
            if ((IntPtr)_pipelineState != IntPtr.Zero)
	            _pipelineState.Dispose();
            if ((IntPtr)_frameTexture != IntPtr.Zero)
	            _frameTexture.Dispose();
            if ((IntPtr)_metalLayer != IntPtr.Zero)
	            _metalLayer.Dispose();
            if ((IntPtr)_commandQueue != IntPtr.Zero)
	            _commandQueue.Dispose();
            if ((IntPtr)_device != IntPtr.Zero)
	            _device.Dispose();
            if (_window != null)
                _glfw.DestroyWindow(_window);
                _window = null;
            _glfw.Terminate();
            _initialized = false;
    public bool IsInitialized => _initialized;
    public int Width => _width;
    public int Height => _height;
    // Hardware-accelerated rendering methods (stub implementations for now)
    public void BeginFrame()
        _logger.LogDebug("[Metal] BeginFrame called (not yet implemented)");
        // TODO: Implement Metal frame begin
    public void EndFrame()
        _logger.LogDebug("[Metal] EndFrame called (not yet implemented)");
        // TODO: Implement Metal frame end and present
    public void DrawTriangles(Span<Vertex> vertices, Span<ushort> indices)
        _logger.LogWarning("[Metal] DrawTriangles not yet implemented");
        // TODO: Implement Metal triangle rendering
    public void SetTexture(uint textureId, byte[] data, int width, int height, TextureFormat format)
        _logger.LogWarning("[Metal] SetTexture not yet implemented");
        // TODO: Implement Metal texture upload
    public void BindTexture(uint textureId)
        _logger.LogWarning("[Metal] BindTexture not yet implemented");
        // TODO: Implement Metal texture binding
    public void SetRenderState(BlendMode blend, DepthTest depth, CullMode cull)
        _logger.LogWarning("[Metal] SetRenderState not yet implemented");
        // TODO: Implement Metal render state management
    public void DeleteTexture(uint textureId)
        _logger.LogWarning("[Metal] DeleteTexture not yet implemented");
        // TODO: Implement Metal texture deletion
