# Metal Backend Advanced Features

## Overview

This document describes the advanced features added to the macOS Metal rendering backend for Win32Emu. These enhancements provide powerful GPU-accelerated capabilities for custom effects, multi-target rendering, 3D graphics, and compute-based image processing.

## Features

### 1. Custom Shader Support

The `MetalShaderManager` class enables loading and managing custom Metal shaders for advanced rendering effects.

#### Key Capabilities

- **Shader Compilation**: Compile Metal shaders from source code at runtime
- **Function Management**: Cache and retrieve vertex/fragment/compute functions
- **Pipeline Creation**: Automatically create render and compute pipelines from shaders

#### Usage Example

```csharp
using var shaderManager = new MetalShaderManager(logger, device);

// Define custom shader source
var shaderSource = @"
#include <metal_stdlib>
using namespace metal;

vertex float4 myVertex(uint vertexID [[vertex_id]],
                       constant float4 *positions [[buffer(0)]]) {
    return positions[vertexID];
}

fragment float4 myFragment(float4 position [[stage_in]]) {
    return float4(1.0, 0.0, 0.0, 1.0); // Red color
}
";

// Load and compile shader
shaderManager.LoadShaderFromSource("customShader", shaderSource);

// Create render pipeline
var pipeline = shaderManager.CreateRenderPipeline(
    "customShader", "myVertex",
    "customShader", "myFragment",
    MTLPixelFormat.RGBA8Unorm
);
```

### 2. Multiple Render Targets (MRT)

The `MetalMultiRenderTarget` class allows rendering to multiple textures simultaneously, enabling advanced rendering techniques like deferred shading.

#### Key Capabilities

- **Multi-Target Creation**: Create up to 8 color render targets
- **Depth Buffer Support**: Optional depth/stencil buffer
- **Render Pass Configuration**: Automatic setup of render pass descriptors
- **Texture Readback**: Read pixel data from any render target

#### Usage Example

```csharp
using var mrt = new MetalMultiRenderTarget(logger, device);

// Create 3 color targets with depth buffer
mrt.CreateTargets(
    width: 1920,
    height: 1080,
    targetCount: 3,
    useDepth: true,
    format: MTLPixelFormat.RGBA8Unorm
);

// Configure render pass
var renderPassDescriptor = new MTLRenderPassDescriptor();
mrt.ConfigureRenderPass(renderPassDescriptor, clearTargets: true);

// Render to multiple targets
var encoder = commandBuffer.RenderCommandEncoder(renderPassDescriptor);
// ... render operations ...
encoder.EndEncoding();

// Read back results from a specific target
var targetData = mrt.ReadTarget(0);
```

### 3. 3D Graphics Emulation

The `Metal3DRenderer` class provides comprehensive 3D rendering capabilities using GPU acceleration.

#### Key Capabilities

- **Vertex/Index Buffers**: Manage 3D geometry data
- **Transformation Matrices**: Support for model-view-projection transforms
- **Depth Testing**: Configurable depth/stencil states
- **Indexed Drawing**: Efficient rendering with index buffers
- **Projection Matrices**: Helper methods for perspective and orthographic projection

#### Vertex Structure

```csharp
public struct Vertex3D
{
    public Vector3 Position;   // 3D position
    public Vector3 Normal;     // Surface normal
    public Vector2 TexCoord;   // Texture coordinates
    public Vector4 Color;      // Vertex color
}
```

#### Usage Example

```csharp
using var renderer = new Metal3DRenderer(logger, device);

// Create vertex data for a triangle
var vertices = new[]
{
    new Metal3DRenderer.Vertex3D(
        new Vector3(-0.5f, -0.5f, 0),
        new Vector3(0, 0, 1),
        new Vector2(0, 0),
        new Vector4(1, 0, 0, 1)
    ),
    new Metal3DRenderer.Vertex3D(
        new Vector3(0.5f, -0.5f, 0),
        new Vector3(0, 0, 1),
        new Vector2(1, 0),
        new Vector4(0, 1, 0, 1)
    ),
    new Metal3DRenderer.Vertex3D(
        new Vector3(0, 0.5f, 0),
        new Vector3(0, 0, 1),
        new Vector2(0.5f, 1),
        new Vector4(0, 0, 1, 1)
    )
};

// Upload vertex data
renderer.UpdateVertexBuffer(vertices);

// Create index buffer
var indices = new uint[] { 0, 1, 2 };
renderer.UpdateIndexBuffer(indices);

// Set up transformation matrices
var projection = Metal3DRenderer.CreatePerspective(
    MathF.PI / 4.0f,  // 45° FOV
    16.0f / 9.0f,     // Aspect ratio
    0.1f,             // Near plane
    100.0f            // Far plane
);

var view = Metal3DRenderer.CreateLookAt(
    new Vector3(0, 0, 5),   // Camera position
    new Vector3(0, 0, 0),   // Look at origin
    new Vector3(0, 1, 0)    // Up vector
);

var model = Matrix4x4.Identity;
var mvp = model * view * projection;

var uniforms = new Metal3DRenderer.Uniforms3D
{
    ModelViewProjection = mvp,
    Model = model,
    View = view,
    Projection = projection,
    LightPosition = new Vector4(5, 5, 5, 1),
    LightColor = new Vector4(1, 1, 1, 1)
};

renderer.UpdateUniforms(uniforms);

// Enable depth testing
renderer.CreateDepthStencilState(
    depthWriteEnabled: true,
    depthCompareFunction: MTLCompareFunction.Less
);

// Configure render encoder
renderer.Configure3DRenderEncoder(encoder);

// Draw the triangle
renderer.DrawIndexed(encoder, indexCount: 3, MTLPrimitiveType.Triangle);
```

### 4. Compute Passes for Image Processing

The `MetalComputeProcessor` class provides GPU-accelerated image processing using compute shaders.

#### Built-in Kernels

The processor includes several pre-built image processing kernels:

1. **Gaussian Blur**: Smooth blurring effect with 5x5 kernel
2. **Sharpen**: Edge enhancement
3. **Edge Detection**: Sobel edge detection
4. **Grayscale**: Convert to grayscale using luminance
5. **Brightness/Contrast**: Adjust image brightness and contrast

#### Usage Example

```csharp
using var processor = new MetalComputeProcessor(logger, device, commandQueue);

// Load a built-in kernel
processor.LoadImageProcessingKernel("blur", ImageProcessingKernel.GaussianBlur);

// Create input and output textures
var inputTexture = CreateTexture(width, height);
var outputTexture = CreateTexture(width, height);

// Process the texture
processor.ProcessTexture("blur", inputTexture, outputTexture);

// For kernels with parameters (e.g., brightness/contrast)
processor.LoadImageProcessingKernel("adjust", ImageProcessingKernel.BrightnessContrast);
var parameters = new Dictionary<string, object>
{
    { "brightness", 0.1f },
    { "contrast", 1.2f }
};
processor.ProcessTexture("adjust", inputTexture, outputTexture, parameters);
```

#### Custom Compute Shaders

You can also create custom compute pipelines:

```csharp
var customComputeShader = @"
#include <metal_stdlib>
using namespace metal;

kernel void customKernel(texture2d<float, access::read> input [[texture(0)]],
                        texture2d<float, access::write> output [[texture(1)]],
                        uint2 gid [[thread_position_in_grid]])
{
    if (gid.x >= output.get_width() || gid.y >= output.get_height())
        return;
    
    float4 color = input.read(gid);
    // Apply custom processing
    color.r = 1.0 - color.r; // Invert red channel
    output.write(color, gid);
}
";

processor.CreateComputePipeline("custom", customComputeShader, "customKernel");
processor.ProcessTexture("custom", inputTexture, outputTexture);
```

## Performance Considerations

### Shader Management

- Shaders are compiled on first load and cached for subsequent use
- Compile shaders during initialization to avoid runtime overhead
- Dispose of the shader manager when no longer needed to free resources

### Multiple Render Targets

- Use MRT for techniques like deferred shading to minimize render passes
- Larger textures consume more GPU memory; consider resolution tradeoffs
- Depth buffers add memory overhead; only create when needed

### 3D Rendering

- Use indexed rendering to reduce vertex data duplication
- Batch geometry updates to minimize buffer transfers
- Update uniform buffers efficiently; they're updated frequently

### Compute Processing

- Compute shaders run in parallel on the GPU; ideal for image processing
- Thread group sizes are optimized at 16x16 for texture operations
- Chain multiple compute passes by using output as next input

## Integration with Existing Backend

These advanced features integrate seamlessly with the existing `SharpMetalRenderingBackend`:

```csharp
// Create rendering backend
using var backend = new SharpMetalRenderingBackend(logger);
backend.Initialize(1920, 1080, "Advanced Metal Demo");

// Access Metal device for advanced features
var device = backend._device; // Note: Would need to expose this property

// Use advanced features
using var shaderManager = new MetalShaderManager(logger, device);
using var mrt = new MetalMultiRenderTarget(logger, device);
using var renderer3D = new Metal3DRenderer(logger, device);
using var computeProcessor = new MetalComputeProcessor(logger, device, commandQueue);
```

## Testing

Comprehensive tests are provided in `MetalAdvancedFeaturesTests.cs`:

- Shader compilation and management tests
- Multi-target creation and configuration tests
- 3D rendering buffer management tests
- Compute pipeline creation tests
- Platform-specific guard clauses for non-macOS environments

All tests handle the absence of Metal gracefully and skip when not available.

## Requirements

- **Platform**: macOS only
- **Package**: SharpMetal v1.0.0
- **Runtime**: .NET 9.0
- **GPU**: Metal-capable Mac (most Macs since 2012)

## Future Enhancements

Potential future improvements could include:

- Tessellation shader support
- Geometry shaders (when available in Metal)
- Ray tracing capabilities
- Enhanced material system
- Animation and skinning support
- Post-processing effect chains

## See Also

- [MACOS_METAL_FIX.md](../fixes/MACOS_METAL_FIX.md) - Original Metal backend implementation
- [METAL_BACKEND.md](METAL_BACKEND.md) - Basic Metal backend documentation
- [METAL_USAGE_EXAMPLES.md](METAL_USAGE_EXAMPLES.md) - Practical code examples
- [SharpMetal Repository](https://github.com/IsaacMarovitz/SharpMetal)
- [Apple Metal Documentation](https://developer.apple.com/metal/)
