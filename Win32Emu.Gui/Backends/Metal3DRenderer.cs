using Microsoft.Extensions.Logging;
using Win32Emu.Rendering;
using SharpMetal.Metal;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Win32Emu.Gui.Backends;
/// <summary>
/// Provides 3D graphics emulation capabilities using Metal
/// </summary>
[SupportedOSPlatform("macos")]
public unsafe class Metal3DRenderer : IDisposable
{
    private readonly ILogger _logger;
    private readonly MTLDevice _device;
    private MTLBuffer? _vertexBuffer;
    private MTLBuffer? _indexBuffer;
    private MTLBuffer? _uniformBuffer;
    private MTLDepthStencilState? _depthStencilState;
    private bool _disposed;
    public Metal3DRenderer(ILogger logger, MTLDevice device)
    {
        _logger = logger;
        _device = device;
    }
    /// <summary>
    /// Vertex structure for 3D rendering
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex3D
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoord;
        public Vector4 Color;
        public Vertex3D(Vector3 position, Vector3 normal, Vector2 texCoord, Vector4 color)
        {
            Position = position;
            Normal = normal;
            TexCoord = texCoord;
            Color = color;
        }
    /// Uniform buffer for transformation matrices
    public struct Uniforms3D
        public Matrix4x4 ModelViewProjection;
        public Matrix4x4 Model;
        public Matrix4x4 View;
        public Matrix4x4 Projection;
        public Vector4 LightPosition;
        public Vector4 LightColor;
    /// Creates or updates the vertex buffer with 3D vertex data
    public bool UpdateVertexBuffer(Vertex3D[] vertices)
        if (_disposed)
            throw new ObjectDisposedException(nameof(Metal3DRenderer));
        try
            var bufferSize = (ulong)(vertices.Length * Marshal.SizeOf<Vertex3D>());
            
            // Dispose old buffer if it exists and is too small
            if (_vertexBuffer.HasValue && _vertexBuffer.Value.Length < bufferSize)
            {
                _vertexBuffer.Value.Dispose();
                _vertexBuffer = null;
            }
            // Create new buffer if needed
            if (!_vertexBuffer.HasValue)
                _vertexBuffer = _device.NewBuffer(bufferSize, MTLResourceOptions.ResourceStorageModeManaged);
                
                if ((IntPtr)_vertexBuffer.Value == IntPtr.Zero)
                {
                    _logger.LogError("[Metal3D] Failed to create vertex buffer");
                    return false;
                }
            // Copy vertex data
            var span = new Span<Vertex3D>(vertices);
            var bytes = MemoryMarshal.AsBytes(span);
            fixed (byte* bytesPtr = bytes)
                Buffer.MemoryCopy(bytesPtr, _vertexBuffer.Value.Contents.ToPointer(), 
                    (long)_vertexBuffer.Value.Length, bytes.Length);
            _vertexBuffer.Value.DidModifyRange(new SharpMetal.Foundation.NSRange 
            { 
                location = 0, 
                length = bufferSize 
            });
            _logger.LogDebug("[Metal3D] Updated vertex buffer with {Count} vertices", vertices.Length);
            return true;
        catch (Exception ex)
            _logger.LogError(ex, "[Metal3D] Failed to update vertex buffer");
            return false;
    /// Creates or updates the index buffer for indexed rendering
    public bool UpdateIndexBuffer(uint[] indices)
            var bufferSize = (ulong)(indices.Length * sizeof(uint));
            if (_indexBuffer.HasValue && _indexBuffer.Value.Length < bufferSize)
                _indexBuffer.Value.Dispose();
                _indexBuffer = null;
            if (!_indexBuffer.HasValue)
                _indexBuffer = _device.NewBuffer(bufferSize, MTLResourceOptions.ResourceStorageModeManaged);
                if ((IntPtr)_indexBuffer.Value == IntPtr.Zero)
                    _logger.LogError("[Metal3D] Failed to create index buffer");
            // Copy index data
            var bytes = MemoryMarshal.Cast<uint, byte>(indices);
                Buffer.MemoryCopy(bytesPtr, _indexBuffer.Value.Contents.ToPointer(), 
                    (long)_indexBuffer.Value.Length, bytes.Length);
            _indexBuffer.Value.DidModifyRange(new SharpMetal.Foundation.NSRange 
            _logger.LogDebug("[Metal3D] Updated index buffer with {Count} indices", indices.Length);
            _logger.LogError(ex, "[Metal3D] Failed to update index buffer");
    /// Updates the uniform buffer with transformation matrices
    public bool UpdateUniforms(Uniforms3D uniforms)
            var bufferSize = (ulong)Marshal.SizeOf<Uniforms3D>();
            // Create uniform buffer if it doesn't exist
            if (!_uniformBuffer.HasValue)
                _uniformBuffer = _device.NewBuffer(bufferSize, MTLResourceOptions.ResourceStorageModeManaged);
                if ((IntPtr)_uniformBuffer.Value == IntPtr.Zero)
                    _logger.LogError("[Metal3D] Failed to create uniform buffer");
            // Copy uniform data
            Marshal.StructureToPtr(uniforms, _uniformBuffer.Value.Contents, false);
            _uniformBuffer.Value.DidModifyRange(new SharpMetal.Foundation.NSRange 
            _logger.LogError(ex, "[Metal3D] Failed to update uniforms");
    /// Creates a depth stencil state for depth testing
    /// <param name="depthCompareFunction">The depth comparison function to use</param>
    public bool CreateDepthStencilState(MTLCompareFunction depthCompareFunction = MTLCompareFunction.Less)
            if (_depthStencilState.HasValue)
                _depthStencilState.Value.Dispose();
            var descriptor = new MTLDepthStencilDescriptor();
            descriptor.DepthCompareFunction = depthCompareFunction;
            _depthStencilState = _device.NewDepthStencilState(descriptor);
            if ((IntPtr)_depthStencilState.Value == IntPtr.Zero)
                _logger.LogError("[Metal3D] Failed to create depth stencil state");
                descriptor.Dispose();
                return false;
            descriptor.Dispose();
            _logger.LogInformation("[Metal3D] Created depth stencil state with compare function: {Function}", 
                depthCompareFunction);
            _logger.LogError(ex, "[Metal3D] Failed to create depth stencil state");
    /// Configures a render command encoder for 3D rendering
    public void Configure3DRenderEncoder(MTLRenderCommandEncoder encoder)
        if (_vertexBuffer.HasValue)
            encoder.SetVertexBuffer(_vertexBuffer.Value, 0, 0);
        if (_uniformBuffer.HasValue)
            encoder.SetVertexBuffer(_uniformBuffer.Value, 0, 1);
            encoder.SetFragmentBuffer(_uniformBuffer.Value, 0, 1);
        if (_depthStencilState.HasValue)
            encoder.SetDepthStencilState(_depthStencilState.Value);
    /// Draws indexed primitives
    public void DrawIndexed(MTLRenderCommandEncoder encoder, int indexCount, 
        MTLPrimitiveType primitiveType = MTLPrimitiveType.Triangle)
        if (!_indexBuffer.HasValue)
            _logger.LogWarning("[Metal3D] Cannot draw indexed: index buffer not set");
            return;
        encoder.DrawIndexedPrimitives(primitiveType, (ulong)indexCount, MTLIndexType.UInt32, 
            _indexBuffer.Value, 0);
    /// Creates a perspective projection matrix
    [SupportedOSPlatform("macos")]
    public static Matrix4x4 CreatePerspective(float fovRadians, float aspectRatio, 
        float nearPlane, float farPlane)
        return Matrix4x4.CreatePerspectiveFieldOfView(fovRadians, aspectRatio, nearPlane, farPlane);
    /// Creates an orthographic projection matrix
    public static Matrix4x4 CreateOrthographic(float width, float height, 
        return Matrix4x4.CreateOrthographic(width, height, nearPlane, farPlane);
    /// Creates a view matrix from position, target, and up vector
    public static Matrix4x4 CreateLookAt(Vector3 cameraPosition, Vector3 target, Vector3 up)
        return Matrix4x4.CreateLookAt(cameraPosition, target, up);
    public MTLBuffer? VertexBuffer => _vertexBuffer;
    public MTLBuffer? IndexBuffer => _indexBuffer;
    public MTLBuffer? UniformBuffer => _uniformBuffer;
    public void Dispose()
        if (_vertexBuffer.HasValue && (IntPtr)_vertexBuffer.Value != IntPtr.Zero)
            _vertexBuffer.Value.Dispose();
            _vertexBuffer = null;
        if (_indexBuffer.HasValue && (IntPtr)_indexBuffer.Value != IntPtr.Zero)
            _indexBuffer.Value.Dispose();
            _indexBuffer = null;
        if (_uniformBuffer.HasValue && (IntPtr)_uniformBuffer.Value != IntPtr.Zero)
            _uniformBuffer.Value.Dispose();
            _uniformBuffer = null;
        if (_depthStencilState.HasValue && (IntPtr)_depthStencilState.Value != IntPtr.Zero)
            _depthStencilState.Value.Dispose();
            _depthStencilState = null;
        _disposed = true;
        GC.SuppressFinalize(this);
}
