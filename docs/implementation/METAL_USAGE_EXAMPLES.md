# Metal Advanced Features Usage Examples

This document provides practical examples of using the advanced Metal backend features in Win32Emu.

## Example 1: Basic Custom Shader Effect

Apply a custom sepia tone shader to the framebuffer:

```csharp
using var backend = new SharpMetalRenderingBackend(logger);
backend.Initialize(1920, 1080, "Sepia Effect Demo");

var device = MTLDevice.CreateSystemDefaultDevice();
using var shaderManager = new MetalShaderManager(logger, device);

// Define sepia tone shader
var sepiaShader = @"
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
    out.texCoord = float2((out.position.x + 1.0) * 0.5, (1.0 - out.position.y) * 0.5);
    return out;
}

fragment float4 fragmentShader(VertexOut in [[stage_in]],
                               texture2d<float> colorTexture [[texture(0)]]) {
    constexpr sampler textureSampler(mag_filter::linear, min_filter::linear);
    float4 color = colorTexture.sample(textureSampler, in.texCoord);
    
    // Sepia tone conversion
    float3 sepia;
    sepia.r = dot(color.rgb, float3(0.393, 0.769, 0.189));
    sepia.g = dot(color.rgb, float3(0.349, 0.686, 0.168));
    sepia.b = dot(color.rgb, float3(0.272, 0.534, 0.131));
    
    return float4(sepia, color.a);
}
";

shaderManager.LoadShaderFromSource("sepia", sepiaShader);
var pipeline = shaderManager.CreateRenderPipeline(
    "sepia", "vertexShader",
    "sepia", "fragmentShader",
    MTLPixelFormat.RGBA8Unorm
);
```

## Example 2: Deferred Rendering with Multiple Render Targets

Implement a basic deferred rendering pipeline:

```csharp
using var mrt = new MetalMultiRenderTarget(logger, device);

// Create G-buffer with 3 targets: albedo, normals, position
mrt.CreateTargets(
    width: 1920,
    height: 1080,
    targetCount: 3,
    useDepth: true,
    format: MTLPixelFormat.RGBA16Float
);

// Geometry pass: render scene to G-buffer
var commandBuffer = commandQueue.CommandBuffer();
var renderPassDesc = new MTLRenderPassDescriptor();
mrt.ConfigureRenderPass(renderPassDesc, clearTargets: true);

var encoder = commandBuffer.RenderCommandEncoder(renderPassDesc);
encoder.SetRenderPipelineState(geometryPipeline);

// Render all geometry to MRT
RenderGeometry(encoder);
encoder.EndEncoding();

// Lighting pass: read from G-buffer and compute lighting
var albedo = mrt.GetColorTarget(0);
var normals = mrt.GetColorTarget(1);
var positions = mrt.GetColorTarget(2);

var lightingEncoder = commandBuffer.RenderCommandEncoder(lightingPassDesc);
lightingEncoder.SetFragmentTexture(albedo, 0);
lightingEncoder.SetFragmentTexture(normals, 1);
lightingEncoder.SetFragmentTexture(positions, 2);
RenderFullscreenQuad(lightingEncoder);
lightingEncoder.EndEncoding();

commandBuffer.Commit();
```

## Example 3: 3D Cube Rendering

Render a spinning 3D cube:

```csharp
using var renderer3D = new Metal3DRenderer(logger, device);

// Define cube vertices
var vertices = new[]
{
    // Front face
    new Metal3DRenderer.Vertex3D(new Vector3(-1, -1, 1), new Vector3(0, 0, 1), new Vector2(0, 0), new Vector4(1, 0, 0, 1)),
    new Metal3DRenderer.Vertex3D(new Vector3(1, -1, 1), new Vector3(0, 0, 1), new Vector2(1, 0), new Vector4(0, 1, 0, 1)),
    new Metal3DRenderer.Vertex3D(new Vector3(1, 1, 1), new Vector3(0, 0, 1), new Vector2(1, 1), new Vector4(0, 0, 1, 1)),
    new Metal3DRenderer.Vertex3D(new Vector3(-1, 1, 1), new Vector3(0, 0, 1), new Vector2(0, 1), new Vector4(1, 1, 0, 1)),
    // ... define other faces
};

var indices = new uint[]
{
    0, 1, 2, 2, 3, 0,  // Front
    // ... define other faces
};

renderer3D.UpdateVertexBuffer(vertices);
renderer3D.UpdateIndexBuffer(indices);

// Set up camera and projection
var projection = Metal3DRenderer.CreatePerspective(
    MathF.PI / 3.0f,    // 60° FOV
    16.0f / 9.0f,       // Aspect ratio
    0.1f, 100.0f        // Near/far planes
);

var view = Metal3DRenderer.CreateLookAt(
    new Vector3(0, 0, 5),    // Camera at (0,0,5)
    new Vector3(0, 0, 0),    // Looking at origin
    new Vector3(0, 1, 0)     // Up is Y
);

// Animation loop
float rotation = 0;
while (running)
{
    rotation += 0.01f;
    var model = Matrix4x4.CreateRotationY(rotation) * Matrix4x4.CreateRotationX(rotation * 0.5f);
    
    var uniforms = new Metal3DRenderer.Uniforms3D
    {
        ModelViewProjection = model * view * projection,
        Model = model,
        View = view,
        Projection = projection,
        LightPosition = new Vector4(5, 5, 5, 1),
        LightColor = new Vector4(1, 1, 1, 1)
    };
    
    renderer3D.UpdateUniforms(uniforms);
    renderer3D.CreateDepthStencilState();
    
    // Render
    var encoder = commandBuffer.RenderCommandEncoder(renderPassDesc);
    renderer3D.Configure3DRenderEncoder(encoder);
    renderer3D.DrawIndexed(encoder, indices.Length);
    encoder.EndEncoding();
}
```

## Example 4: Image Processing Chain

Apply multiple image processing effects in sequence:

```csharp
using var processor = new MetalComputeProcessor(logger, device, commandQueue);

// Load multiple kernels
processor.LoadImageProcessingKernel("blur", ImageProcessingKernel.GaussianBlur);
processor.LoadImageProcessingKernel("sharpen", ImageProcessingKernel.Sharpen);
processor.LoadImageProcessingKernel("edge", ImageProcessingKernel.EdgeDetection);

// Create intermediate textures
var original = CreateTextureFromFramebuffer(width, height);
var blurred = CreateEmptyTexture(width, height);
var sharpened = CreateEmptyTexture(width, height);
var edges = CreateEmptyTexture(width, height);

// Process: original -> blur -> sharpen -> edge detection
processor.ProcessTexture("blur", original, blurred);
processor.ProcessTexture("sharpen", blurred, sharpened);
processor.ProcessTexture("edge", sharpened, edges);

// Copy result back to framebuffer
CopyTextureToFramebuffer(edges);
```

## Example 5: Brightness/Contrast Adjustment

Interactively adjust image brightness and contrast:

```csharp
using var processor = new MetalComputeProcessor(logger, device, commandQueue);

processor.LoadImageProcessingKernel("adjust", ImageProcessingKernel.BrightnessContrast);

var inputTexture = CreateTextureFromFramebuffer(width, height);
var outputTexture = CreateEmptyTexture(width, height);

// Allow user to adjust via slider or keyboard
float brightness = 0.0f;  // -1.0 to 1.0
float contrast = 1.0f;     // 0.0 to 2.0

while (running)
{
    HandleInput(ref brightness, ref contrast);
    
    var parameters = new Dictionary<string, object>
    {
        { "brightness", brightness },
        { "contrast", contrast }
    };
    
    processor.ProcessTexture("adjust", inputTexture, outputTexture, parameters);
    CopyTextureToFramebuffer(outputTexture);
}
```

## Example 6: Custom Compute Shader for Color Inversion

Create a custom compute shader for effect not included in built-ins:

```csharp
using var processor = new MetalComputeProcessor(logger, device, commandQueue);

var invertShader = @"
#include <metal_stdlib>
using namespace metal;

kernel void invertColors(texture2d<float, access::read> input [[texture(0)]],
                        texture2d<float, access::write> output [[texture(1)]],
                        uint2 gid [[thread_position_in_grid]])
{
    if (gid.x >= output.get_width() || gid.y >= output.get_height())
        return;
    
    float4 color = input.read(gid);
    color.rgb = 1.0 - color.rgb;  // Invert RGB, keep alpha
    output.write(color, gid);
}
";

processor.CreateComputePipeline("invert", invertShader, "invertColors");

var inputTex = GetCurrentFrame();
var outputTex = CreateEmptyTexture(width, height);
processor.ProcessTexture("invert", inputTex, outputTex);
```

## Example 7: Combining 3D and Post-Processing

Render 3D scene and apply post-processing:

```csharp
// Set up 3D renderer
using var renderer3D = new Metal3DRenderer(logger, device);
using var processor = new MetalComputeProcessor(logger, device, commandQueue);

// Create render target for 3D scene
var sceneTexture = CreateEmptyTexture(width, height);
var processedTexture = CreateEmptyTexture(width, height);

// Render 3D scene to texture
Render3DSceneToTexture(renderer3D, sceneTexture);

// Apply post-processing effects
processor.LoadImageProcessingKernel("blur", ImageProcessingKernel.GaussianBlur);
processor.ProcessTexture("blur", sceneTexture, processedTexture);

// Display result
DisplayTexture(processedTexture);
```

## Performance Tips

1. **Shader Compilation**: Compile shaders during initialization, not in the render loop
2. **Buffer Updates**: Batch vertex/uniform updates to minimize GPU transfers
3. **Texture Reuse**: Reuse textures when possible instead of creating new ones each frame
4. **Command Buffer Batching**: Group multiple render/compute passes into single command buffer
5. **Thread Group Optimization**: Compute shaders use 16x16 thread groups; align texture sizes when possible

## Common Pitfalls

1. **Memory Leaks**: Always dispose of Metal objects (textures, pipelines, buffers)
2. **Platform Guards**: Wrap Metal code with `OperatingSystem.IsMacOS()` checks
3. **Null Checks**: Check that Metal objects are not null/zero before using
4. **Coordinate Systems**: Metal uses different coordinate systems than DirectX/OpenGL
5. **Shader Syntax**: Metal Shading Language has specific syntax requirements

## See Also

- [METAL_ADVANCED_FEATURES.md](METAL_ADVANCED_FEATURES.md) - Complete API documentation
- [METAL_BACKEND.md](METAL_BACKEND.md) - Basic Metal backend overview
- [Apple Metal Best Practices](https://developer.apple.com/metal/Metal-Best-Practices-Guide.pdf)
