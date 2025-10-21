using Microsoft.Extensions.Logging;
using SDL3;
using System.Runtime.InteropServices;

namespace Win32Emu.Rendering;

/// <summary>
/// SDL3 GPU-based rendering backend for DirectDraw and GDI operations.
/// Uses Metal on macOS, Vulkan on Linux, and DirectX 12 on Windows.
/// </summary>
public unsafe class Sdl3RenderingBackend : IRenderingBackend
{
    private readonly ILogger _logger;
    private IntPtr _window;
    private IntPtr _gpuDevice;
    private IntPtr _frameTexture;
    private IntPtr _transferBuffer;
    private int _width;
    private int _height;
    private bool _initialized;
    private readonly object _lock = new();
    private bool _disposed;

    /// <summary>
    /// Event fired when a UI event occurs (mouse, keyboard, window)
    /// </summary>
    public event EventHandler<UIEventArgs>? UIEvent;

    public Sdl3RenderingBackend(ILogger logger)
    {
        _logger = logger;
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

            try
            {
                _logger.LogInformation("[SDL3] Initializing SDL3 GPU rendering backend...");

                // Critical: Set app metadata before any SDL initialization
                Sdl3Initializer.EnsureAppMetadataSet();

                // Initialize SDL video subsystem
                if (!SDL.Init(SDL_InitFlags.Video))
                {
                    _logger.LogError("[SDL3] Failed to initialize SDL video: {Error}", SDL.GetError());
                    return false;
                }

                // Create GPU device with shader format support for all platforms
                var shaderFormats = SDL_GPUShaderFormat.SPIRV | SDL_GPUShaderFormat.MSL | SDL_GPUShaderFormat.DXIL;
                _gpuDevice = SDL.CreateGPUDevice(shaderFormats, debug: false, name: null);
                
                if (_gpuDevice == IntPtr.Zero)
                {
                    _logger.LogError("[SDL3] Failed to create GPU device: {Error}", SDL.GetError());
                    SDL.Quit();
                    return false;
                }

                // Get the driver name
                var driverName = SDL.GetGPUDeviceDriver(_gpuDevice);
                _logger.LogInformation("[SDL3] Created GPU device with driver: {Driver}", driverName);

                // Create window
                _window = SDL.CreateWindow(title, width, height, SDL_WindowFlags.Hidden);
                if (_window == IntPtr.Zero)
                {
                    _logger.LogError("[SDL3] Failed to create window: {Error}", SDL.GetError());
                    SDL.DestroyGPUDevice(_gpuDevice);
                    SDL.Quit();
                    return false;
                }

                // Claim window for GPU device
                if (!SDL.ClaimWindowForGPUDevice(_gpuDevice, _window))
                {
                    _logger.LogError("[SDL3] Failed to claim window for GPU device: {Error}", SDL.GetError());
                    SDL.DestroyWindow(_window);
                    SDL.DestroyGPUDevice(_gpuDevice);
                    SDL.Quit();
                    return false;
                }

                // Create frame texture
                var textureCreateInfo = new SDL_GPUTextureCreateInfo
                {
                    type = SDL_GPUTextureType.TwoDimensional,
                    format = SDL_GPUTextureFormat.R8G8B8A8Unorm,
                    usage = SDL_GPUTextureUsageFlags.Sampler | SDL_GPUTextureUsageFlags.ColorTarget,
                    width = (uint)width,
                    height = (uint)height,
                    layer_count_or_depth = 1,
                    num_levels = 1,
                    sample_count = SDL_GPUSampleCount.One,
                    @props = 0
                };

                _frameTexture = SDL.CreateGPUTexture(_gpuDevice, textureCreateInfo);
                if (_frameTexture == IntPtr.Zero)
                {
                    _logger.LogError("[SDL3] Failed to create frame texture: {Error}", SDL.GetError());
                    SDL.ReleaseWindowFromGPUDevice(_gpuDevice, _window);
                    SDL.DestroyWindow(_window);
                    SDL.DestroyGPUDevice(_gpuDevice);
                    SDL.Quit();
                    return false;
                }

                // Create transfer buffer for uploading frame data
                var transferBufferCreateInfo = new SDL_GPUTransferBufferCreateInfo
                {
                    usage = SDL_GPUTransferBufferUsage.Upload,
                    size = (uint)(width * height * 4), // RGBA format
                    @props = 0
                };

                _transferBuffer = SDL.CreateGPUTransferBuffer(_gpuDevice, transferBufferCreateInfo);
                if (_transferBuffer == IntPtr.Zero)
                {
                    _logger.LogError("[SDL3] Failed to create transfer buffer: {Error}", SDL.GetError());
                    SDL.ReleaseGPUTexture(_gpuDevice, _frameTexture);
                    SDL.ReleaseWindowFromGPUDevice(_gpuDevice, _window);
                    SDL.DestroyWindow(_window);
                    SDL.DestroyGPUDevice(_gpuDevice);
                    SDL.Quit();
                    return false;
                }

                // Show window
                SDL.ShowWindow(_window);

                _initialized = true;
                _logger.LogInformation("[SDL3] Initialized {Width}x{Height} display with GPU backend ({Driver})", 
                    width, height, driverName);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SDL3] Failed to initialize rendering backend");
                Cleanup();
                return false;
            }
        }
    }

    public byte[] ConvertPalettizedToRGBA(byte[] indexedData, uint[] palette, int width, int height, int pitch)
    {
        var rgbaData = new byte[width * height * 4];
        var rgbaIndex = 0;

        for (var y = 0; y < height; y++)
        {
            var rowOffset = y * pitch;
            for (var x = 0; x < width; x++)
            {
                var paletteIndex = indexedData[rowOffset + x];
                var color = palette[paletteIndex];

                rgbaData[rgbaIndex++] = (byte)(color & 0xFF);         // R
                rgbaData[rgbaIndex++] = (byte)((color >> 8) & 0xFF);  // G
                rgbaData[rgbaIndex++] = (byte)((color >> 16) & 0xFF); // B
                rgbaData[rgbaIndex++] = (byte)((color >> 24) & 0xFF); // A
            }
        }

        return rgbaData;
    }

    public byte[] Convert16BitToRGBA(byte[] rgb565Data, int width, int height, int pitch)
    {
        var rgbaData = new byte[width * height * 4];
        var rgbaIndex = 0;

        for (var y = 0; y < height; y++)
        {
            var rowOffset = y * pitch;
            for (var x = 0; x < width; x++)
            {
                var pixelOffset = rowOffset + (x * 2);
                var pixel = (ushort)(rgb565Data[pixelOffset] | (rgb565Data[pixelOffset + 1] << 8));

                // RGB565 to RGBA8888
                var r = (byte)(((pixel >> 11) & 0x1F) * 255 / 31);
                var g = (byte)(((pixel >> 5) & 0x3F) * 255 / 63);
                var b = (byte)((pixel & 0x1F) * 255 / 31);

                rgbaData[rgbaIndex++] = r;
                rgbaData[rgbaIndex++] = g;
                rgbaData[rgbaIndex++] = b;
                rgbaData[rgbaIndex++] = 255; // A
            }
        }

        return rgbaData;
    }

    public byte[] Convert24BitToRGBA(byte[] rgb24Data, int width, int height, int pitch)
    {
        var rgbaData = new byte[width * height * 4];
        var rgbaIndex = 0;

        for (var y = 0; y < height; y++)
        {
            var rowOffset = y * pitch;
            for (var x = 0; x < width; x++)
            {
                var pixelOffset = rowOffset + (x * 3);

                rgbaData[rgbaIndex++] = rgb24Data[pixelOffset];     // R
                rgbaData[rgbaIndex++] = rgb24Data[pixelOffset + 1]; // G
                rgbaData[rgbaIndex++] = rgb24Data[pixelOffset + 2]; // B
                rgbaData[rgbaIndex++] = 255;                         // A
            }
        }

        return rgbaData;
    }

    public bool UpdateFrameBuffer(byte[] data, int pitch)
    {
        if (!_initialized)
        {
            return false;
        }

        lock (_lock)
        {
            try
            {
                // Map transfer buffer
                var mappedPtr = SDL.MapGPUTransferBuffer(_gpuDevice, _transferBuffer, cycle: false);
                if (mappedPtr == IntPtr.Zero)
                {
                    _logger.LogError("[SDL3] Failed to map transfer buffer");
                    return false;
                }

                // Copy frame data to transfer buffer
                var expectedSize = _width * _height * 4;
                if (data.Length >= expectedSize)
                {
                    Marshal.Copy(data, 0, mappedPtr, expectedSize);
                }
                else
                {
                    // Handle pitch differences
                    for (var y = 0; y < _height; y++)
                    {
                        var srcOffset = y * pitch;
                        var dstOffset = y * (_width * 4);
                        var rowSize = Math.Min(pitch, _width * 4);
                        Marshal.Copy(data, srcOffset, mappedPtr + dstOffset, rowSize);
                    }
                }

                // Unmap buffer
                SDL.UnmapGPUTransferBuffer(_gpuDevice, _transferBuffer);

                // Acquire command buffer
                var commandBuffer = SDL.AcquireGPUCommandBuffer(_gpuDevice);
                if (commandBuffer == IntPtr.Zero)
                {
                    _logger.LogError("[SDL3] Failed to acquire command buffer");
                    return false;
                }

                // Create copy pass
                var copyPass = SDL.BeginGPUCopyPass(commandBuffer);
                if (copyPass == IntPtr.Zero)
                {
                    _logger.LogError("[SDL3] Failed to begin copy pass");
                    return false;
                }

                // Upload from transfer buffer to texture
                var source = new SDL_GPUTextureTransferInfo
                {
                    transfer_buffer = _transferBuffer,
                    offset = 0,
                    pixels_per_row = (uint)_width,
                    rows_per_layer = (uint)_height
                };

                var destination = new SDL_GPUTextureRegion
                {
                    texture = _frameTexture,
                    mip_level = 0,
                    layer = 0,
                    x = 0,
                    y = 0,
                    z = 0,
                    w = (uint)_width,
                    h = (uint)_height,
                    d = 1
                };

                SDL.UploadToGPUTexture(copyPass, source, destination, cycle: false);
                SDL.EndGPUCopyPass(copyPass);

                // Acquire swapchain texture
                IntPtr swapchainTexture;
                uint swapchainWidth, swapchainHeight;
                if (!SDL.AcquireGPUSwapchainTexture(commandBuffer, _window, out swapchainTexture, 
                    out swapchainWidth, out swapchainHeight))
                {
                    _logger.LogError("[SDL3] Failed to acquire swapchain texture");
                    return false;
                }

                if (swapchainTexture != IntPtr.Zero)
                {
                    // Blit frame texture to swapchain
                    var blitInfo = new SDL_GPUBlitInfo
                    {
                        source = new SDL_GPUBlitRegion
                        {
                            texture = _frameTexture,
                            mip_level = 0,
                            layer_or_depth_plane = 0,
                            x = 0,
                            y = 0,
                            w = (uint)_width,
                            h = (uint)_height
                        },
                        destination = new SDL_GPUBlitRegion
                        {
                            texture = swapchainTexture,
                            mip_level = 0,
                            layer_or_depth_plane = 0,
                            x = 0,
                            y = 0,
                            w = swapchainWidth,
                            h = swapchainHeight
                        },
                        load_op = SDL_GPULoadOp.DontCare,
                        clear_color = default,
                        flip_mode = SDL_FlipMode.None,
                        filter = SDL_GPUFilter.Linear,
                        cycle = false
                    };

                    SDL.BlitGPUTexture(commandBuffer, blitInfo);
                }

                // Submit command buffer
                SDL.SubmitGPUCommandBuffer(commandBuffer);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SDL3] Failed to update frame buffer");
                return false;
            }
        }
    }

    public void Clear(byte r, byte g, byte b, byte a = 255)
    {
        if (!_initialized)
        {
            return;
        }

        lock (_lock)
        {
            try
            {
                // Create a solid color buffer
                var clearData = new byte[_width * _height * 4];
                for (var i = 0; i < clearData.Length; i += 4)
                {
                    clearData[i] = r;
                    clearData[i + 1] = g;
                    clearData[i + 2] = b;
                    clearData[i + 3] = a;
                }

                UpdateFrameBuffer(clearData, _width * 4);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SDL3] Failed to clear display");
            }
        }
    }

    public void ProcessEvents()
    {
        if (!_initialized)
        {
            return;
        }

        SDL_Event evt;
        while (SDL.PollEvent(out evt))
        {
            switch (evt.type)
            {
                case SDL_EventType.Quit:
                    OnUIEvent(new UIEventArgs
                    {
                        EventType = UIEventType.Quit,
                        WindowHandle = 0
                    });
                    break;

                case SDL_EventType.WindowCloseRequested:
                    OnUIEvent(new UIEventArgs
                    {
                        EventType = UIEventType.Quit,
                        WindowHandle = 0
                    });
                    break;

                case SDL_EventType.WindowFocusGained:
                    OnUIEvent(new UIEventArgs
                    {
                        EventType = UIEventType.WindowActivate,
                        WindowHandle = 0
                    });
                    break;

                case SDL_EventType.WindowFocusLost:
                    OnUIEvent(new UIEventArgs
                    {
                        EventType = UIEventType.WindowDeactivate,
                        WindowHandle = 0
                    });
                    break;
            }
        }
    }

    protected virtual void OnUIEvent(UIEventArgs e)
    {
        UIEvent?.Invoke(this, e);
    }

    public bool IsInitialized => _initialized;
    public int Width => _width;
    public int Height => _height;

    private void Cleanup()
    {
        if (_transferBuffer != IntPtr.Zero)
        {
            SDL.ReleaseGPUTransferBuffer(_gpuDevice, _transferBuffer);
            _transferBuffer = IntPtr.Zero;
        }

        if (_frameTexture != IntPtr.Zero)
        {
            SDL.ReleaseGPUTexture(_gpuDevice, _frameTexture);
            _frameTexture = IntPtr.Zero;
        }

        if (_window != IntPtr.Zero)
        {
            SDL.ReleaseWindowFromGPUDevice(_gpuDevice, _window);
            SDL.DestroyWindow(_window);
            _window = IntPtr.Zero;
        }

        if (_gpuDevice != IntPtr.Zero)
        {
            SDL.WaitForGPUIdle(_gpuDevice);
            SDL.DestroyGPUDevice(_gpuDevice);
            _gpuDevice = IntPtr.Zero;
        }

        SDL.Quit();
        _initialized = false;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            Cleanup();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}
