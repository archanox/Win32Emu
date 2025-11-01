using Microsoft.Extensions.Logging;
using SharpMetal.Metal;
using SharpMetal.Foundation;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Win32Emu.Rendering;

/// <summary>
/// Provides compute shader capabilities for image processing and general computation
/// </summary>
[SupportedOSPlatform("macos")]
public unsafe class MetalComputeProcessor : IDisposable
{
    private readonly ILogger _logger;
    private readonly MTLDevice _device;
    private readonly MTLCommandQueue _commandQueue;
    private readonly Dictionary<string, MTLComputePipelineState> _pipelines;
    private bool _disposed;

    public MetalComputeProcessor(ILogger logger, MTLDevice device, MTLCommandQueue commandQueue)
    {
        _logger = logger;
        _device = device;
        _commandQueue = commandQueue;
        _pipelines = new Dictionary<string, MTLComputePipelineState>();
    }

    /// <summary>
    /// Creates a compute pipeline from shader source
    /// </summary>
    public bool CreateComputePipeline(string name, string shaderSource, string functionName)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MetalComputeProcessor));
        }

        try
        {
            // Compile shader
            var compileOptions = new MTLCompileOptions(IntPtr.Zero);
            var shaderSourceNS = NSString.String(shaderSource);
            NSError error = default;
            var library = _device.NewLibrary(shaderSourceNS, compileOptions, ref error);

            if ((IntPtr)library == IntPtr.Zero || (IntPtr)error != IntPtr.Zero)
            {
                _logger.LogError("[MetalCompute] Failed to compile compute shader '{Name}': {Error}",
                    name, (IntPtr)error != IntPtr.Zero ? error.LocalizedDescription.ToString() : "Unknown error");
                return false;
            }

            // Get compute function
            var function = library.NewFunction(NSString.String(functionName));
            if ((IntPtr)function == IntPtr.Zero)
            {
                _logger.LogError("[MetalCompute] Failed to get function '{Function}' from shader '{Name}'",
                    functionName, name);
                library.Dispose();
                return false;
            }

            // Create pipeline state
            NSError pipelineError = default;
            var pipelineState = _device.NewComputePipelineState(function, ref pipelineError);

            if ((IntPtr)pipelineState == IntPtr.Zero || (IntPtr)pipelineError != IntPtr.Zero)
            {
                _logger.LogError("[MetalCompute] Failed to create pipeline state for '{Name}': {Error}",
                    name, (IntPtr)pipelineError != IntPtr.Zero ? pipelineError.LocalizedDescription.ToString() : "Unknown error");
                function.Dispose();
                library.Dispose();
                return false;
            }

            // Store pipeline
            if (_pipelines.ContainsKey(name))
            {
                _pipelines[name].Dispose();
            }
            _pipelines[name] = pipelineState;

            function.Dispose();
            library.Dispose();

            _logger.LogInformation("[MetalCompute] Created compute pipeline '{Name}'", name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MetalCompute] Failed to create compute pipeline '{Name}'", name);
            return false;
        }
    }

    /// <summary>
    /// Loads a built-in image processing kernel
    /// </summary>
    public bool LoadImageProcessingKernel(string name, ImageProcessingKernel kernel)
    {
        string shaderSource = kernel switch
        {
            ImageProcessingKernel.GaussianBlur => GetGaussianBlurShader(),
            ImageProcessingKernel.Sharpen => GetSharpenShader(),
            ImageProcessingKernel.EdgeDetection => GetEdgeDetectionShader(),
            ImageProcessingKernel.Grayscale => GetGrayscaleShader(),
            ImageProcessingKernel.BrightnessContrast => GetBrightnessContrastShader(),
            _ => throw new ArgumentException($"Unknown kernel type: {kernel}")
        };

        return CreateComputePipeline(name, shaderSource, "computeMain");
    }

    /// <summary>
    /// Executes a compute pass on a texture
    /// </summary>
    public bool ProcessTexture(string pipelineName, MTLTexture inputTexture, MTLTexture outputTexture,
        Dictionary<string, object>? parameters = null)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MetalComputeProcessor));
        }

        if (!_pipelines.TryGetValue(pipelineName, out var pipeline))
        {
            _logger.LogError("[MetalCompute] Pipeline '{Name}' not found", pipelineName);
            return false;
        }

        try
        {
            var commandBuffer = _commandQueue.CommandBuffer();
            if ((IntPtr)commandBuffer == IntPtr.Zero)
            {
                _logger.LogError("[MetalCompute] Failed to create command buffer");
                return false;
            }

            var computeEncoder = commandBuffer.ComputeCommandEncoder();
            if ((IntPtr)computeEncoder == IntPtr.Zero)
            {
                _logger.LogError("[MetalCompute] Failed to create compute encoder");
                commandBuffer.Dispose();
                return false;
            }

            computeEncoder.SetComputePipelineState(pipeline);
            computeEncoder.SetTexture(inputTexture, 0);
            computeEncoder.SetTexture(outputTexture, 1);

            // Set parameters if provided
            if (parameters != null)
            {
                SetComputeParameters(computeEncoder, parameters);
            }

            // Calculate thread group sizes
            var width = (int)inputTexture.Width;
            var height = (int)inputTexture.Height;
            var threadGroupSize = new MTLSize { width = 16, height = 16, depth = 1 };
            var threadGroups = new MTLSize
            {
                width = (ulong)((width + 15) / 16),
                height = (ulong)((height + 15) / 16),
                depth = 1
            };

            computeEncoder.DispatchThreadgroups(threadGroups, threadGroupSize);
            computeEncoder.EndEncoding();

            commandBuffer.Commit();
            commandBuffer.WaitUntilCompleted();

            computeEncoder.Dispose();
            commandBuffer.Dispose();

            _logger.LogDebug("[MetalCompute] Processed texture with pipeline '{Name}'", pipelineName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MetalCompute] Failed to process texture with pipeline '{Name}'", pipelineName);
            return false;
        }
    }

    private void SetComputeParameters(MTLComputeCommandEncoder encoder, Dictionary<string, object> parameters)
    {
        int bufferIndex = 2; // Start after textures
        
        foreach (var param in parameters)
        {
            if (param.Value is float floatValue)
            {
                var buffer = _device.NewBuffer((ulong)sizeof(float), MTLResourceOptions.ResourceStorageModeManaged);
                Marshal.StructureToPtr(floatValue, buffer.Contents, false);
                encoder.SetBuffer(buffer, 0, (ulong)bufferIndex++);
            }
            else if (param.Value is int intValue)
            {
                var buffer = _device.NewBuffer((ulong)sizeof(int), MTLResourceOptions.ResourceStorageModeManaged);
                Marshal.StructureToPtr(intValue, buffer.Contents, false);
                encoder.SetBuffer(buffer, 0, (ulong)bufferIndex++);
            }
        }
    }

    private static string GetGaussianBlurShader()
    {
        return @"
#include <metal_stdlib>
using namespace metal;

kernel void computeMain(texture2d<float, access::read> inputTexture [[texture(0)]],
                       texture2d<float, access::write> outputTexture [[texture(1)]],
                       uint2 gid [[thread_position_in_grid]])
{
    if (gid.x >= outputTexture.get_width() || gid.y >= outputTexture.get_height())
        return;
    
    float4 sum = float4(0.0);
    float kernel[5][5] = {
        {1,  4,  7,  4, 1},
        {4, 16, 26, 16, 4},
        {7, 26, 41, 26, 7},
        {4, 16, 26, 16, 4},
        {1,  4,  7,  4, 1}
    };
    
    float kernelSum = 273.0;
    
    for (int y = -2; y <= 2; y++) {
        for (int x = -2; x <= 2; x++) {
            int2 coord = int2(gid) + int2(x, y);
            coord = clamp(coord, int2(0), int2(inputTexture.get_width() - 1, inputTexture.get_height() - 1));
            sum += inputTexture.read(uint2(coord)) * kernel[y + 2][x + 2];
        }
    }
    
    outputTexture.write(sum / kernelSum, gid);
}
";
    }

    private static string GetSharpenShader()
    {
        return @"
#include <metal_stdlib>
using namespace metal;

kernel void computeMain(texture2d<float, access::read> inputTexture [[texture(0)]],
                       texture2d<float, access::write> outputTexture [[texture(1)]],
                       uint2 gid [[thread_position_in_grid]])
{
    if (gid.x >= outputTexture.get_width() || gid.y >= outputTexture.get_height())
        return;
    
    float4 center = inputTexture.read(gid);
    float4 sum = center * 5.0;
    
    sum -= inputTexture.read(uint2(gid.x - 1, gid.y));
    sum -= inputTexture.read(uint2(gid.x + 1, gid.y));
    sum -= inputTexture.read(uint2(gid.x, gid.y - 1));
    sum -= inputTexture.read(uint2(gid.x, gid.y + 1));
    
    outputTexture.write(clamp(sum, 0.0, 1.0), gid);
}
";
    }

    private static string GetEdgeDetectionShader()
    {
        return @"
#include <metal_stdlib>
using namespace metal;

kernel void computeMain(texture2d<float, access::read> inputTexture [[texture(0)]],
                       texture2d<float, access::write> outputTexture [[texture(1)]],
                       uint2 gid [[thread_position_in_grid]])
{
    if (gid.x >= outputTexture.get_width() || gid.y >= outputTexture.get_height())
        return;
    
    float4 gx = float4(0.0);
    float4 gy = float4(0.0);
    
    // Sobel operator
    gx += inputTexture.read(uint2(gid.x - 1, gid.y - 1)) * -1.0;
    gx += inputTexture.read(uint2(gid.x - 1, gid.y + 1)) * 1.0;
    gx += inputTexture.read(uint2(gid.x + 1, gid.y - 1)) * -1.0;
    gx += inputTexture.read(uint2(gid.x + 1, gid.y + 1)) * 1.0;
    
    gy += inputTexture.read(uint2(gid.x - 1, gid.y - 1)) * -1.0;
    gy += inputTexture.read(uint2(gid.x + 1, gid.y - 1)) * 1.0;
    gy += inputTexture.read(uint2(gid.x - 1, gid.y + 1)) * -1.0;
    gy += inputTexture.read(uint2(gid.x + 1, gid.y + 1)) * 1.0;
    
    float4 magnitude = sqrt(gx * gx + gy * gy);
    outputTexture.write(magnitude, gid);
}
";
    }

    private static string GetGrayscaleShader()
    {
        return @"
#include <metal_stdlib>
using namespace metal;

kernel void computeMain(texture2d<float, access::read> inputTexture [[texture(0)]],
                       texture2d<float, access::write> outputTexture [[texture(1)]],
                       uint2 gid [[thread_position_in_grid]])
{
    if (gid.x >= outputTexture.get_width() || gid.y >= outputTexture.get_height())
        return;
    
    float4 color = inputTexture.read(gid);
    float gray = dot(color.rgb, float3(0.299, 0.587, 0.114));
    outputTexture.write(float4(gray, gray, gray, color.a), gid);
}
";
    }

    private static string GetBrightnessContrastShader()
    {
        return @"
#include <metal_stdlib>
using namespace metal;

kernel void computeMain(texture2d<float, access::read> inputTexture [[texture(0)]],
                       texture2d<float, access::write> outputTexture [[texture(1)]],
                       constant float &brightness [[buffer(2)]],
                       constant float &contrast [[buffer(3)]],
                       uint2 gid [[thread_position_in_grid]])
{
    if (gid.x >= outputTexture.get_width() || gid.y >= outputTexture.get_height())
        return;
    
    float4 color = inputTexture.read(gid);
    color.rgb = (color.rgb - 0.5) * contrast + 0.5 + brightness;
    color.rgb = clamp(color.rgb, 0.0, 1.0);
    outputTexture.write(color, gid);
}
";
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var pipeline in _pipelines.Values)
        {
            if ((IntPtr)pipeline != IntPtr.Zero)
            {
                pipeline.Dispose();
            }
        }
        _pipelines.Clear();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Built-in image processing kernels
/// </summary>
public enum ImageProcessingKernel
{
    GaussianBlur,
    Sharpen,
    EdgeDetection,
    Grayscale,
    BrightnessContrast
}
