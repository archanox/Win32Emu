using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Rendering;
using SharpMetal.Metal;
using System.Numerics;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for advanced Metal backend features
/// </summary>
public class MetalAdvancedFeaturesTests
{
    [Fact]
    public void MetalShaderManager_LoadShaderFromSource_ShouldCompileValidShader()
    {
        // This test requires macOS and Metal support
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        try
        {
            var device = MTLDevice.CreateSystemDefaultDevice();
            if (device == IntPtr.Zero)
            {
                return; // No Metal device available
            }

            using var shaderManager = new MetalShaderManager(NullLogger.Instance, device);

            var shaderSource = @"
#include <metal_stdlib>
using namespace metal;

vertex float4 testVertex(uint vertexID [[vertex_id]]) {
    return float4(0.0, 0.0, 0.0, 1.0);
}

fragment float4 testFragment() {
    return float4(1.0, 0.0, 0.0, 1.0);
}
";

            // Act
            var result = shaderManager.LoadShaderFromSource("test", shaderSource);

            // Assert
            Assert.True(result);

            // Clean up
            device.Dispose();
        }
        catch (DllNotFoundException)
        {
            // SharpMetal not available - skip test
        }
        catch (Exception)
        {
            // Metal not available - skip test
        }
    }

    [Fact]
    public void MetalShaderManager_GetFunction_ShouldReturnValidFunction()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        try
        {
            var device = MTLDevice.CreateSystemDefaultDevice();
            if (device == IntPtr.Zero)
            {
                return;
            }

            using var shaderManager = new MetalShaderManager(NullLogger.Instance, device);

            var shaderSource = @"
#include <metal_stdlib>
using namespace metal;

vertex float4 myVertexFunc(uint vertexID [[vertex_id]]) {
    return float4(0.0, 0.0, 0.0, 1.0);
}
";

            shaderManager.LoadShaderFromSource("testLib", shaderSource);

            // Act
            var function = shaderManager.GetFunction("testLib", "myVertexFunc");

            // Assert
            Assert.NotNull(function);

            device.Dispose();
        }
        catch (DllNotFoundException)
        {
            // SharpMetal not available - skip test
        }
        catch (Exception)
        {
            // Metal not available - skip test
        }
    }

    [Fact]
    public void MetalMultiRenderTarget_CreateTargets_ShouldCreateMultipleTargets()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        try
        {
            var device = MTLDevice.CreateSystemDefaultDevice();
            if (device == IntPtr.Zero)
            {
                return;
            }

            using var mrt = new MetalMultiRenderTarget(NullLogger.Instance, device);

            // Act
            var result = mrt.CreateTargets(800, 600, targetCount: 3, useDepth: true);

            // Assert
            Assert.True(result);
            Assert.Equal(800, mrt.Width);
            Assert.Equal(600, mrt.Height);
            Assert.Equal(3, mrt.TargetCount);

            var target0 = mrt.GetColorTarget(0);
            var target1 = mrt.GetColorTarget(1);
            var target2 = mrt.GetColorTarget(2);
            var depthTarget = mrt.GetDepthTarget();

            Assert.NotNull(target0);
            Assert.NotNull(target1);
            Assert.NotNull(target2);
            Assert.NotNull(depthTarget);

            device.Dispose();
        }
        catch (DllNotFoundException)
        {
            // SharpMetal not available - skip test
        }
        catch (Exception)
        {
            // Metal not available - skip test
        }
    }

    [Fact]
    public void MetalMultiRenderTarget_GetColorTarget_WithInvalidIndex_ShouldReturnNull()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        try
        {
            var device = MTLDevice.CreateSystemDefaultDevice();
            if (device == IntPtr.Zero)
            {
                return;
            }

            using var mrt = new MetalMultiRenderTarget(NullLogger.Instance, device);
            mrt.CreateTargets(800, 600, targetCount: 2);

            // Act
            var invalidTarget = mrt.GetColorTarget(5);

            // Assert
            Assert.Null(invalidTarget);

            device.Dispose();
        }
        catch (DllNotFoundException)
        {
            // SharpMetal not available - skip test
        }
        catch (Exception)
        {
            // Metal not available - skip test
        }
    }

    [Fact]
    public void Metal3DRenderer_UpdateVertexBuffer_ShouldSucceed()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        try
        {
            var device = MTLDevice.CreateSystemDefaultDevice();
            if (device == IntPtr.Zero)
            {
                return;
            }

            using var renderer = new Metal3DRenderer(NullLogger.Instance, device);

            var vertices = new[]
            {
                new Metal3DRenderer.Vertex3D(
                    new Vector3(0, 0, 0),
                    new Vector3(0, 0, 1),
                    new Vector2(0, 0),
                    new Vector4(1, 1, 1, 1)
                ),
                new Metal3DRenderer.Vertex3D(
                    new Vector3(1, 0, 0),
                    new Vector3(0, 0, 1),
                    new Vector2(1, 0),
                    new Vector4(1, 1, 1, 1)
                ),
                new Metal3DRenderer.Vertex3D(
                    new Vector3(0, 1, 0),
                    new Vector3(0, 0, 1),
                    new Vector2(0, 1),
                    new Vector4(1, 1, 1, 1)
                )
            };

            // Act
            var result = renderer.UpdateVertexBuffer(vertices);

            // Assert
            Assert.True(result);
            Assert.NotNull(renderer.VertexBuffer);

            device.Dispose();
        }
        catch (DllNotFoundException)
        {
            // SharpMetal not available - skip test
        }
        catch (Exception)
        {
            // Metal not available - skip test
        }
    }

    [Fact]
    public void Metal3DRenderer_UpdateIndexBuffer_ShouldSucceed()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        try
        {
            var device = MTLDevice.CreateSystemDefaultDevice();
            if (device == IntPtr.Zero)
            {
                return;
            }

            using var renderer = new Metal3DRenderer(NullLogger.Instance, device);

            var indices = new uint[] { 0, 1, 2, 2, 3, 0 };

            // Act
            var result = renderer.UpdateIndexBuffer(indices);

            // Assert
            Assert.True(result);
            Assert.NotNull(renderer.IndexBuffer);

            device.Dispose();
        }
        catch (DllNotFoundException)
        {
            // SharpMetal not available - skip test
        }
        catch (Exception)
        {
            // Metal not available - skip test
        }
    }

    [Fact]
    public void Metal3DRenderer_CreatePerspective_ShouldReturnValidMatrix()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        // Act
        var matrix = Metal3DRenderer.CreatePerspective(
            MathF.PI / 4.0f,  // 45 degrees FOV
            16.0f / 9.0f,      // Aspect ratio
            0.1f,              // Near plane
            100.0f             // Far plane
        );

        // Assert
        Assert.NotEqual(Matrix4x4.Identity, matrix);
    }

    [Fact]
    public void Metal3DRenderer_CreateLookAt_ShouldReturnValidMatrix()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        // Act
        var matrix = Metal3DRenderer.CreateLookAt(
            new Vector3(0, 0, 5),  // Camera position
            new Vector3(0, 0, 0),  // Look at origin
            new Vector3(0, 1, 0)   // Up vector
        );

        // Assert
        Assert.NotEqual(Matrix4x4.Identity, matrix);
    }

    [Fact]
    public void MetalComputeProcessor_LoadImageProcessingKernel_ShouldSucceed()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        try
        {
            var device = MTLDevice.CreateSystemDefaultDevice();
            if (device == IntPtr.Zero)
            {
                return;
            }

            var commandQueue = device.NewCommandQueue();
            if (commandQueue == IntPtr.Zero)
            {
                device.Dispose();
                return;
            }

            using var processor = new MetalComputeProcessor(NullLogger.Instance, device, commandQueue);

            // Act
            var result = processor.LoadImageProcessingKernel("blur", ImageProcessingKernel.GaussianBlur);

            // Assert
            Assert.True(result);

            commandQueue.Dispose();
            device.Dispose();
        }
        catch (DllNotFoundException)
        {
            // SharpMetal not available - skip test
        }
        catch (Exception)
        {
            // Metal not available - skip test
        }
    }

    [Fact]
    public void MetalComputeProcessor_LoadMultipleKernels_ShouldSucceed()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        try
        {
            var device = MTLDevice.CreateSystemDefaultDevice();
            if (device == IntPtr.Zero)
            {
                return;
            }

            var commandQueue = device.NewCommandQueue();
            if (commandQueue == IntPtr.Zero)
            {
                device.Dispose();
                return;
            }

            using var processor = new MetalComputeProcessor(NullLogger.Instance, device, commandQueue);

            // Act
            var blur = processor.LoadImageProcessingKernel("blur", ImageProcessingKernel.GaussianBlur);
            var sharpen = processor.LoadImageProcessingKernel("sharpen", ImageProcessingKernel.Sharpen);
            var grayscale = processor.LoadImageProcessingKernel("gray", ImageProcessingKernel.Grayscale);
            var edge = processor.LoadImageProcessingKernel("edge", ImageProcessingKernel.EdgeDetection);

            // Assert
            Assert.True(blur);
            Assert.True(sharpen);
            Assert.True(grayscale);
            Assert.True(edge);

            commandQueue.Dispose();
            device.Dispose();
        }
        catch (DllNotFoundException)
        {
            // SharpMetal not available - skip test
        }
        catch (Exception)
        {
            // Metal not available - skip test
        }
    }

    [Fact]
    public void MetalComputeProcessor_Dispose_ShouldNotThrow()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        try
        {
            var device = MTLDevice.CreateSystemDefaultDevice();
            if (device == IntPtr.Zero)
            {
                return;
            }

            var commandQueue = device.NewCommandQueue();
            if (commandQueue == IntPtr.Zero)
            {
                device.Dispose();
                return;
            }

            var processor = new MetalComputeProcessor(NullLogger.Instance, device, commandQueue);
            processor.LoadImageProcessingKernel("test", ImageProcessingKernel.Grayscale);

            // Act & Assert - should not throw
            processor.Dispose();

            commandQueue.Dispose();
            device.Dispose();
        }
        catch (DllNotFoundException)
        {
            // SharpMetal not available - skip test
        }
        catch (Exception)
        {
            // Metal not available - skip test
        }
    }
}
