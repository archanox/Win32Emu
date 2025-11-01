using Microsoft.Extensions.Logging;
using SharpMetal.Metal;
using SharpMetal.Foundation;
using System.Runtime.Versioning;

namespace Win32Emu.Rendering;

/// <summary>
/// Manages custom Metal shaders for advanced rendering effects
/// </summary>
[SupportedOSPlatform("macos")]
public unsafe class MetalShaderManager : IDisposable
{
    private readonly ILogger _logger;
    private readonly MTLDevice _device;
    private readonly Dictionary<string, MTLFunction> _shaderFunctions;
    private readonly Dictionary<string, MTLLibrary> _libraries;
    private bool _disposed;

    public MetalShaderManager(ILogger logger, MTLDevice device)
    {
        _logger = logger;
        _device = device;
        _shaderFunctions = new Dictionary<string, MTLFunction>();
        _libraries = new Dictionary<string, MTLLibrary>();
    }

    /// <summary>
    /// Loads and compiles a Metal shader from source code
    /// </summary>
    /// <param name="name">Unique name for the shader</param>
    /// <param name="source">Metal shader source code</param>
    /// <returns>True if compilation succeeded</returns>
    public bool LoadShaderFromSource(string name, string source)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MetalShaderManager));
        }

        try
        {
            var compileOptions = new MTLCompileOptions(IntPtr.Zero);
            var shaderSourceNS = NSString.String(source);
            NSError error = default;
            var library = _device.NewLibrary(shaderSourceNS, compileOptions, ref error);

            if ((IntPtr)library == IntPtr.Zero || (IntPtr)error != IntPtr.Zero)
            {
                _logger.LogError("[MetalShaderManager] Failed to compile shader '{Name}': {Error}",
                    name, (IntPtr)error != IntPtr.Zero ? error.LocalizedDescription.ToString() : "Unknown error");
                return false;
            }

            _libraries[name] = library;
            _logger.LogInformation("[MetalShaderManager] Successfully compiled shader library '{Name}'", name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MetalShaderManager] Failed to load shader '{Name}'", name);
            return false;
        }
    }

    /// <summary>
    /// Gets a shader function from a loaded library
    /// </summary>
    /// <param name="libraryName">Name of the shader library</param>
    /// <param name="functionName">Name of the function to retrieve</param>
    /// <returns>The Metal function or null if not found</returns>
    public MTLFunction? GetFunction(string libraryName, string functionName)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MetalShaderManager));
        }

        var cacheKey = $"{libraryName}::{functionName}";
        if (_shaderFunctions.TryGetValue(cacheKey, out var cachedFunction))
        {
            return cachedFunction;
        }

        if (!_libraries.TryGetValue(libraryName, out var library))
        {
            _logger.LogWarning("[MetalShaderManager] Library '{LibraryName}' not found", libraryName);
            return null;
        }

        var function = library.NewFunction(NSString.String(functionName));
        if ((IntPtr)function == IntPtr.Zero)
        {
            _logger.LogWarning("[MetalShaderManager] Function '{FunctionName}' not found in library '{LibraryName}'",
                functionName, libraryName);
            return null;
        }

        _shaderFunctions[cacheKey] = function;
        return function;
    }

    /// <summary>
    /// Creates a render pipeline state with custom shaders
    /// </summary>
    public MTLRenderPipelineState CreateRenderPipeline(
        string vertexLibrary, string vertexFunction,
        string fragmentLibrary, string fragmentFunction,
        MTLPixelFormat colorFormat)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MetalShaderManager));
        }

        var vertFunc = GetFunction(vertexLibrary, vertexFunction);
        var fragFunc = GetFunction(fragmentLibrary, fragmentFunction);

        if (vertFunc == null || fragFunc == null)
        {
            throw new InvalidOperationException("Failed to get shader functions");
        }

        var pipelineDescriptor = new MTLRenderPipelineDescriptor();
        pipelineDescriptor.VertexFunction = vertFunc.Value;
        pipelineDescriptor.FragmentFunction = fragFunc.Value;

        var colorAttachment = pipelineDescriptor.ColorAttachments.Object(0);
        colorAttachment.PixelFormat = colorFormat;
        pipelineDescriptor.ColorAttachments.SetObject(colorAttachment, 0);

        NSError error = default;
        var pipelineState = _device.NewRenderPipelineState(pipelineDescriptor, ref error);

        if ((IntPtr)pipelineState == IntPtr.Zero || (IntPtr)error != IntPtr.Zero)
        {
            var errorMsg = (IntPtr)error != IntPtr.Zero ? error.LocalizedDescription.ToString() : "Unknown error";
            throw new InvalidOperationException($"Failed to create render pipeline: {errorMsg}");
        }

        pipelineDescriptor.Dispose();
        return pipelineState;
    }

    /// <summary>
    /// Creates a compute pipeline state with a custom compute shader
    /// </summary>
    public MTLComputePipelineState CreateComputePipeline(string libraryName, string functionName)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MetalShaderManager));
        }

        var computeFunc = GetFunction(libraryName, functionName);
        if (computeFunc == null)
        {
            throw new InvalidOperationException($"Failed to get compute function '{functionName}' from '{libraryName}'");
        }

        NSError error = default;
        var pipelineState = _device.NewComputePipelineState(computeFunc.Value, ref error);

        if ((IntPtr)pipelineState == IntPtr.Zero || (IntPtr)error != IntPtr.Zero)
        {
            var errorMsg = (IntPtr)error != IntPtr.Zero ? error.LocalizedDescription.ToString() : "Unknown error";
            throw new InvalidOperationException($"Failed to create compute pipeline: {errorMsg}");
        }

        return pipelineState;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var function in _shaderFunctions.Values.Where(f => (IntPtr)f != IntPtr.Zero))
        {
            function.Dispose();
        }
        _shaderFunctions.Clear();

        foreach (var library in _libraries.Values.Where(l => (IntPtr)l != IntPtr.Zero))
        {
            library.Dispose();
        }
        _libraries.Clear();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
