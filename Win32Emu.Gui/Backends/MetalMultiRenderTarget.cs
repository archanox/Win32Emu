using Microsoft.Extensions.Logging;
using Win32Emu.Rendering;
using SharpMetal.Metal;
using System.Runtime.Versioning;

namespace Win32Emu.Gui.Backends;
/// <summary>
/// Manages multiple render targets for advanced rendering techniques
/// </summary>
[SupportedOSPlatform("macos")]
public unsafe class MetalMultiRenderTarget : IDisposable
{
    private readonly ILogger _logger;
    private readonly MTLDevice _device;
    private MTLTexture[] _colorTargets;
    private MTLTexture? _depthTarget;
    private int _width;
    private int _height;
    private bool _disposed;
    public int Width => _width;
    public int Height => _height;
    public int TargetCount => _colorTargets.Length;
    public MetalMultiRenderTarget(ILogger logger, MTLDevice device)
    {
        _logger = logger;
        _device = device;
        _colorTargets = Array.Empty<MTLTexture>();
    }
    /// <summary>
    /// Creates multiple color render targets with optional depth buffer
    /// </summary>
    /// <param name="width">Width of render targets</param>
    /// <param name="height">Height of render targets</param>
    /// <param name="targetCount">Number of color targets (1-8)</param>
    /// <param name="useDepth">Whether to create a depth buffer</param>
    /// <param name="format">Pixel format for color targets</param>
    public bool CreateTargets(int width, int height, int targetCount = 2, bool useDepth = true, 
        MTLPixelFormat format = MTLPixelFormat.RGBA8Unorm)
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MetalMultiRenderTarget));
        }
        if (targetCount < 1 || targetCount > 8)
            _logger.LogError("[MetalMRT] Target count must be between 1 and 8, got {Count}", targetCount);
            return false;
        try
            _width = width;
            _height = height;
            // Dispose existing targets
            DisposeTargets();
            // Create color targets
            _colorTargets = new MTLTexture[targetCount];
            for (int i = 0; i < targetCount; i++)
            {
                var descriptor = new MTLTextureDescriptor();
                descriptor.TextureType = MTLTextureType.Type2D;
                descriptor.PixelFormat = format;
                descriptor.Width = (ulong)width;
                descriptor.Height = (ulong)height;
                descriptor.Usage = MTLTextureUsage.ShaderRead | MTLTextureUsage.RenderTarget;
                _colorTargets[i] = _device.NewTexture(descriptor);
                
                if ((IntPtr)_colorTargets[i] == IntPtr.Zero)
                {
                    _logger.LogError("[MetalMRT] Failed to create color target {Index}", i);
                    DisposeTargets();
                    return false;
                }
                descriptor.Dispose();
            }
            // Create depth target if requested
            if (useDepth)
                var depthDescriptor = new MTLTextureDescriptor();
                depthDescriptor.TextureType = MTLTextureType.Type2D;
                depthDescriptor.PixelFormat = MTLPixelFormat.Depth32Float;
                depthDescriptor.Width = (ulong)width;
                depthDescriptor.Height = (ulong)height;
                depthDescriptor.Usage = MTLTextureUsage.RenderTarget;
                _depthTarget = _device.NewTexture(depthDescriptor);
                if ((IntPtr)_depthTarget.Value == IntPtr.Zero)
                    _logger.LogWarning("[MetalMRT] Failed to create depth target");
                    _depthTarget = null;
                depthDescriptor.Dispose();
            _logger.LogInformation("[MetalMRT] Created {Count} render target(s) at {Width}x{Height} with depth={HasDepth}",
                targetCount, width, height, _depthTarget.HasValue);
            
            return true;
        catch (Exception ex)
            _logger.LogError(ex, "[MetalMRT] Failed to create render targets");
    /// Gets a specific color target texture
    public MTLTexture? GetColorTarget(int index)
        if (index < 0 || index >= _colorTargets.Length)
            return null;
        return _colorTargets[index];
    /// Gets the depth target texture if it exists
    public MTLTexture? GetDepthTarget()
        return _depthTarget;
    /// Configures a render pass descriptor with all render targets
    public void ConfigureRenderPass(MTLRenderPassDescriptor descriptor, bool clearTargets = true)
        for (int i = 0; i < _colorTargets.Length; i++)
            var attachment = descriptor.ColorAttachments.Object((ulong)i);
            attachment.Texture = _colorTargets[i];
            attachment.LoadAction = clearTargets ? MTLLoadAction.Clear : MTLLoadAction.Load;
            attachment.StoreAction = MTLStoreAction.Store;
            attachment.ClearColor = new MTLClearColor { red = 0.0, green = 0.0, blue = 0.0, alpha = 1.0 };
            descriptor.ColorAttachments.SetObject(attachment, (ulong)i);
        if (_depthTarget.HasValue)
            var depthAttachment = descriptor.DepthAttachment;
            depthAttachment.Texture = _depthTarget.Value;
            depthAttachment.LoadAction = clearTargets ? MTLLoadAction.Clear : MTLLoadAction.Load;
            depthAttachment.StoreAction = MTLStoreAction.Store;
            depthAttachment.ClearDepth = 1.0;
    /// Reads back pixel data from a specific color target
    public byte[] ReadTarget(int targetIndex)
        if (targetIndex < 0 || targetIndex >= _colorTargets.Length)
            throw new ArgumentOutOfRangeException(nameof(targetIndex));
        var texture = _colorTargets[targetIndex];
        var bytesPerPixel = 4; // Assuming RGBA8
        var bytesPerRow = _width * bytesPerPixel;
        var data = new byte[_height * bytesPerRow];
        var region = new MTLRegion
            origin = new MTLOrigin { x = 0, y = 0, z = 0 },
            size = new MTLSize { width = (ulong)_width, height = (ulong)_height, depth = 1 }
        };
        unsafe
            fixed (byte* dataPtr = data)
                texture.GetBytes((IntPtr)dataPtr, (ulong)bytesPerRow, region, 0);
        return data;
    private void DisposeTargets()
        foreach (var target in _colorTargets.Where(t => (IntPtr)t != IntPtr.Zero))
            target.Dispose();
        if (_depthTarget.HasValue && (IntPtr)_depthTarget.Value != IntPtr.Zero)
            _depthTarget.Value.Dispose();
            _depthTarget = null;
    public void Dispose()
            return;
        DisposeTargets();
        _disposed = true;
        GC.SuppressFinalize(this);
}
