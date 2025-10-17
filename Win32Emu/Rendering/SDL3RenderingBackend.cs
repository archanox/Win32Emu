using Microsoft.Extensions.Logging;
using SDL3;

namespace Win32Emu.Rendering;

/// <summary>
/// SDL3-based rendering backend for DirectDraw and GDI operations.
/// Uses SDL3 GPU API for hardware-accelerated rendering with Metal (macOS), Vulkan (Linux), and DirectX (Windows).
/// </summary>
public class Sdl3RenderingBackend(ILogger logger) : IDisposable
{
    private IntPtr _window;
    private IntPtr _gpuDevice;
    private IntPtr _gpuTexture;
    private bool _initialized;
    private int _width;
    private int _height;
    private readonly Lock _lock = new();
    private byte[]? _frameBuffer;

    /// <summary>
    /// Initialize SDL3 with specified dimensions using GPU API for hardware acceleration.
    /// On macOS uses Metal, on Linux uses Vulkan, on Windows uses DirectX 12.
    /// </summary>
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

            // Set app metadata before creating GPU device
            SDL.SetAppMetadata(title, "1.0", "com.win32emu.display");

            // Create GPU device with auto-selected driver (Metal/Vulkan/DirectX)
            // The second parameter 'true' enables debug mode for better error reporting
            _gpuDevice = SDL.CreateGPUDevice(
                SDL.GPUShaderFormat.SPIRV | SDL.GPUShaderFormat.MSL | SDL.GPUShaderFormat.DXIL,
                true,
                null);

            if (_gpuDevice == IntPtr.Zero)
            {
                logger.LogError("[SDL3] Failed to create GPU device: {GetError}", SDL.GetError());
                return false;
            }

            var driverName = SDL.GetGPUDeviceDriver(_gpuDevice);
            logger.LogInformation("[SDL3] Created GPU device with driver: {Driver}", driverName);

            // Create window
            _window = SDL.CreateWindow(title, width, height, SDL.WindowFlags.Resizable);
            if (_window == IntPtr.Zero)
            {
                logger.LogError("[SDL3] Failed to create window: {GetError}", SDL.GetError());
                SDL.DestroyGPUDevice(_gpuDevice);
                return false;
            }

            // Claim window for GPU device
            if (!SDL.ClaimWindowForGPUDevice(_gpuDevice, _window))
            {
                logger.LogError("[SDL3] Failed to claim window for GPU device: {GetError}", SDL.GetError());
                SDL.DestroyWindow(_window);
                SDL.DestroyGPUDevice(_gpuDevice);
                return false;
            }

            // Create GPU texture for frame buffer
            var textureCreateInfo = new SDL.GPUTextureCreateInfo
            {
                Type = SDL.GPUTextureType.Texturetype2D,
                Format = SDL.GPUTextureFormat.R8G8B8A8Unorm,
                Usage = SDL.GPUTextureUsageFlags.Sampler | SDL.GPUTextureUsageFlags.ColorTarget,
                Width = (uint)width,
                Height = (uint)height,
                LayerCountOrDepth = 1,
                NumLevels = 1,
                SampleCount = SDL.GPUSampleCount.SampleCount1
            };

            _gpuTexture = SDL.CreateGPUTexture(_gpuDevice, textureCreateInfo);
            if (_gpuTexture == IntPtr.Zero)
            {
                logger.LogError("[SDL3] Failed to create GPU texture: {GetError}", SDL.GetError());
                SDL.ReleaseWindowFromGPUDevice(_gpuDevice, _window);
                SDL.DestroyWindow(_window);
                SDL.DestroyGPUDevice(_gpuDevice);
                return false;
            }

            // Allocate CPU-side frame buffer
            _frameBuffer = new byte[width * height * 4]; // RGBA format

            _initialized = true;
            logger.LogInformation("[SDL3] Initialized {Width}x{Height} display with GPU backend ({Driver})", 
                width, height, driverName);
            return true;
        }
    }

    /// <summary>
    /// Convert palettized (8-bit indexed) surface to RGBA format
    /// </summary>
    public byte[] ConvertPalettizedToRGBA(byte[] indexedData, uint[] palette, int width, int height, int pitch)
    {
        var rgbaData = new byte[width * height * 4];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int srcOffset = y * pitch + x;
                int dstOffset = (y * width + x) * 4;
                
                if (srcOffset < indexedData.Length)
                {
                    byte paletteIndex = indexedData[srcOffset];
                    
                    if (paletteIndex < palette.Length)
                    {
                        uint color = palette[paletteIndex];
                        
                        // PALETTEENTRY format: RGBQUAD (r, g, b, flags)
                        rgbaData[dstOffset + 0] = (byte)(color & 0xFF);         // R
                        rgbaData[dstOffset + 1] = (byte)((color >> 8) & 0xFF);  // G
                        rgbaData[dstOffset + 2] = (byte)((color >> 16) & 0xFF); // B
                        rgbaData[dstOffset + 3] = 0xFF;                          // A (opaque)
                    }
                }
            }
        }
        
        return rgbaData;
    }

    /// <summary>
    /// Convert 16-bit RGB565 surface to RGBA format
    /// </summary>
    public byte[] Convert16BitToRGBA(byte[] rgb565Data, int width, int height, int pitch)
    {
        var rgbaData = new byte[width * height * 4];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int srcOffset = y * pitch + x * 2;
                int dstOffset = (y * width + x) * 4;
                
                if (srcOffset + 1 < rgb565Data.Length)
                {
                    ushort pixel = (ushort)(rgb565Data[srcOffset] | (rgb565Data[srcOffset + 1] << 8));
                    
                    // RGB565 format: RRRRRGGGGGGBBBBB
                    byte r5 = (byte)((pixel >> 11) & 0x1F);
                    byte g6 = (byte)((pixel >> 5) & 0x3F);
                    byte b5 = (byte)(pixel & 0x1F);
                    byte r = (byte)((r5 << 3) | (r5 >> 2));
                    byte g = (byte)((g6 << 2) | (g6 >> 4));
                    byte b = (byte)((b5 << 3) | (b5 >> 2));
                    
                    rgbaData[dstOffset + 0] = r;
                    rgbaData[dstOffset + 1] = g;
                    rgbaData[dstOffset + 2] = b;
                    rgbaData[dstOffset + 3] = 0xFF; // Alpha (opaque)
                }
            }
        }
        
        return rgbaData;
    }

    /// <summary>
    /// Update the display with new frame buffer data using GPU API
    /// </summary>
    public bool UpdateFrameBuffer(byte[] data, int pitch)
    {
        lock (_lock)
        {
            if (!_initialized || _gpuDevice == IntPtr.Zero || _gpuTexture == IntPtr.Zero)
            {
                return false;
            }

            // Copy data to our frame buffer
            if (_frameBuffer != null && data.Length <= _frameBuffer.Length)
            {
                Array.Copy(data, _frameBuffer, data.Length);
            }
            else
            {
                logger.LogError("[SDL3] Frame buffer size mismatch");
                return false;
            }

            // Acquire command buffer for GPU operations
            var commandBuffer = SDL.AcquireGPUCommandBuffer(_gpuDevice);
            if (commandBuffer == IntPtr.Zero)
            {
                logger.LogError("[SDL3] Failed to acquire GPU command buffer: {GetError}", SDL.GetError());
                return false;
            }

            // Acquire swapchain texture to render to
            IntPtr swapchainTexture;
            uint swapchainWidth, swapchainHeight;
            if (!SDL.AcquireGPUSwapchainTexture(commandBuffer, _window, out swapchainTexture, 
                out swapchainWidth, out swapchainHeight))
            {
                logger.LogError("[SDL3] Failed to acquire swapchain texture: {GetError}", SDL.GetError());
                return false;
            }

            if (swapchainTexture == IntPtr.Zero)
            {
                // Window is minimized or occluded, skip rendering
                return true;
            }

            // For now, we'll use a simple copy pass to blit the texture
            // In a full implementation, we'd upload the frame buffer data to the GPU texture
            // and use a render pass to draw it to the swapchain

            // Begin copy pass to upload frame buffer data
            var copyPass = SDL.BeginGPUCopyPass(commandBuffer);

            // Create transfer buffer for uploading data
            var transferBufferCreateInfo = new SDL.GPUTransferBufferCreateInfo
            {
                Usage = SDL.GPUTransferBufferUsage.Upload,
                Size = (uint)(_width * _height * 4)
            };

            var transferBuffer = SDL.CreateGPUTransferBuffer(_gpuDevice, transferBufferCreateInfo);
            if (transferBuffer != IntPtr.Zero)
            {
                // Map transfer buffer and copy data
                var mappedData = SDL.MapGPUTransferBuffer(_gpuDevice, transferBuffer, false);
                if (mappedData != IntPtr.Zero && _frameBuffer != null)
                {
                    unsafe
                    {
                        fixed (byte* srcPtr = _frameBuffer)
                        {
                            Buffer.MemoryCopy(srcPtr, mappedData.ToPointer(), 
                                _frameBuffer.Length, _frameBuffer.Length);
                        }
                    }
                    SDL.UnmapGPUTransferBuffer(_gpuDevice, transferBuffer);

                    // Upload to GPU texture
                    var textureTransferInfo = new SDL.GPUTextureTransferInfo
                    {
                        TransferBuffer = transferBuffer,
                        Offset = 0,
                        PixelsPerRow = (uint)_width,
                        RowsPerLayer = (uint)_height
                    };

                    var textureRegion = new SDL.GPUTextureRegion
                    {
                        Texture = _gpuTexture,
                        MipLevel = 0,
                        Layer = 0,
                        X = 0,
                        Y = 0,
                        Z = 0,
                        W = (uint)_width,
                        H = (uint)_height,
                        D = 1
                    };

                    SDL.UploadToGPUTexture(copyPass, textureTransferInfo, textureRegion, false);
                }

                SDL.ReleaseGPUTransferBuffer(_gpuDevice, transferBuffer);
            }

            SDL.EndGPUCopyPass(copyPass);

            // Now blit our texture to the swapchain
            var blitInfo = new SDL.GPUBlitInfo
            {
                Source = new SDL.GPUBlitRegion
                {
                    Texture = _gpuTexture,
                    MipLevel = 0,
                    LayerOrDepthPlane = 0,
                    X = 0,
                    Y = 0,
                    W = (uint)_width,
                    H = (uint)_height
                },
                Destination = new SDL.GPUBlitRegion
                {
                    Texture = swapchainTexture,
                    MipLevel = 0,
                    LayerOrDepthPlane = 0,
                    X = 0,
                    Y = 0,
                    W = swapchainWidth,
                    H = swapchainHeight
                },
                LoadOp = SDL.GPULoadOp.Clear,
                ClearColor = new SDL.FColor { R = 0, G = 0, B = 0, A = 1 },
                Filter = SDL.GPUFilter.Linear,
                FlipMode = 0
            };

            SDL.BlitGPUTexture(commandBuffer, blitInfo);

            // Submit command buffer
            SDL.SubmitGPUCommandBuffer(commandBuffer);

            return true;
        }
    }

    /// <summary>
    /// Clear the display with specified color using GPU API
    /// </summary>
    public void Clear(byte r, byte g, byte b, byte a = 255)
    {
        lock (_lock)
        {
            if (!_initialized || _gpuDevice == IntPtr.Zero)
            {
                return;
            }

            // Acquire command buffer
            var commandBuffer = SDL.AcquireGPUCommandBuffer(_gpuDevice);
            if (commandBuffer == IntPtr.Zero)
            {
                return;
            }

            // Acquire swapchain texture
            IntPtr swapchainTexture;
            uint swapchainWidth, swapchainHeight;
            if (!SDL.AcquireGPUSwapchainTexture(commandBuffer, _window, out swapchainTexture,
                out swapchainWidth, out swapchainHeight) || swapchainTexture == IntPtr.Zero)
            {
                return;
            }

            // Begin render pass with clear operation
            var colorTargetInfo = new SDL.GPUColorTargetInfo
            {
                Texture = swapchainTexture,
                MipLevel = 0,
                LayerOrDepthPlane = 0,
                ClearColor = new SDL.FColor { R = r / 255.0f, G = g / 255.0f, B = b / 255.0f, A = a / 255.0f },
                LoadOp = SDL.GPULoadOp.Clear,
                StoreOp = SDL.GPUStoreOp.Store,
                ResolveTexture = IntPtr.Zero,
                ResolveMipLevel = 0,
                ResolveLayer = 0,
                CycleResolveTexture = 0
            };

            var colorTargets = new[] { colorTargetInfo };
            var renderPass = SDL.BeginGPURenderPass(commandBuffer, colorTargets, 1, IntPtr.Zero);
            
            // End render pass (clear happens automatically)
            SDL.EndGPURenderPass(renderPass);

            // Submit command buffer
            SDL.SubmitGPUCommandBuffer(commandBuffer);
        }
    }

    /// <summary>
    /// Process SDL events (call periodically)
    /// </summary>
    public void ProcessEvents()
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                return;
            }

            SDL.Event evt;
            while (SDL.PollEvent(out evt))
            {
                switch ((SDL.EventType)evt.Type)
                {
                    case SDL.EventType.Quit:
                        // Handle quit event
                        break;
                    case SDL.EventType.WindowResized:
                        // Handle window resize
                        break;
                }
            }
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                return;
            }

            // Wait for GPU to finish
            if (_gpuDevice != IntPtr.Zero)
            {
                SDL.WaitForGPUIdle(_gpuDevice);
            }

            // Release GPU texture
            if (_gpuTexture != IntPtr.Zero && _gpuDevice != IntPtr.Zero)
            {
                SDL.ReleaseGPUTexture(_gpuDevice, _gpuTexture);
                _gpuTexture = IntPtr.Zero;
            }

            // Release window from GPU device
            if (_window != IntPtr.Zero && _gpuDevice != IntPtr.Zero)
            {
                SDL.ReleaseWindowFromGPUDevice(_gpuDevice, _window);
            }

            // Destroy window
            if (_window != IntPtr.Zero)
            {
                SDL.DestroyWindow(_window);
                _window = IntPtr.Zero;
            }

            // Destroy GPU device
            if (_gpuDevice != IntPtr.Zero)
            {
                SDL.DestroyGPUDevice(_gpuDevice);
                _gpuDevice = IntPtr.Zero;
            }

            _frameBuffer = null;
            _initialized = false;
        }
    }

    public bool IsInitialized => _initialized;
    public int Width => _width;
    public int Height => _height;
}
