using Microsoft.Extensions.Logging;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Win32Emu.Rendering;

/// <summary>
/// Silk.NET Vulkan-based rendering backend for DirectDraw and GDI operations
/// Uses MoltenVK on macOS for Metal interoperability
/// </summary>
public unsafe class SilkVulkanRenderingBackend : IRenderingBackend
{
    private readonly ILogger _logger;
    private IWindow? _window;
    private Vk? _vk;
    private Instance _instance;
    private PhysicalDevice _physicalDevice;
    private Device _device;
    private Queue _graphicsQueue;
    private SurfaceKHR _surface;
    private SwapchainKHR _swapchain;
    private Image[] _swapchainImages = Array.Empty<Image>();
    private ImageView[] _swapchainImageViews = Array.Empty<ImageView>();
    private Image _stagingImage;
    private DeviceMemory _stagingMemory;
    private CommandPool _commandPool;
    private CommandBuffer _commandBuffer;
    private Silk.NET.Vulkan.Semaphore _imageAvailableSemaphore;
    private Silk.NET.Vulkan.Semaphore _renderFinishedSemaphore;
    private Fence _inFlightFence;
    private bool _initialized;
    private int _width;
    private int _height;
    private readonly object _lock = new();
    private KhrSurface? _khrSurface;
    private KhrSwapchain? _khrSwapchain;
    private uint _graphicsQueueFamilyIndex;

    public SilkVulkanRenderingBackend(ILogger logger)
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
                // Create window
                var options = WindowOptions.DefaultVulkan with
                {
                    Size = new Silk.NET.Maths.Vector2D<int>(width, height),
                    Title = title,
                    API = new GraphicsAPI(ContextAPI.Vulkan, ContextProfile.Core, ContextFlags.Default, new APIVersion(1, 0))
                };

                _window = Window.Create(options);
                _window.Initialize();

                // Initialize Vulkan
                _vk = Vk.GetApi();

                // Create Vulkan instance
                if (!CreateInstance())
                {
                    _logger.LogError("[Vulkan] Failed to create instance");
                    return false;
                }

                // Create surface
                _surface = _window.VkSurface!.Create<AllocationCallbacks>(_instance.ToHandle(), null).ToSurface();

                // Select physical device
                if (!SelectPhysicalDevice())
                {
                    _logger.LogError("[Vulkan] Failed to select physical device");
                    return false;
                }

                // Create logical device
                if (!CreateLogicalDevice())
                {
                    _logger.LogError("[Vulkan] Failed to create logical device");
                    return false;
                }

                // Create swapchain
                if (!CreateSwapchain())
                {
                    _logger.LogError("[Vulkan] Failed to create swapchain");
                    return false;
                }

                // Create staging image for frame buffer updates
                if (!CreateStagingImage())
                {
                    _logger.LogError("[Vulkan] Failed to create staging image");
                    return false;
                }

                // Create command pool and buffers
                if (!CreateCommandResources())
                {
                    _logger.LogError("[Vulkan] Failed to create command resources");
                    return false;
                }

                // Create synchronization objects
                if (!CreateSyncObjects())
                {
                    _logger.LogError("[Vulkan] Failed to create sync objects");
                    return false;
                }

                _initialized = true;
                _logger.LogInformation("[Vulkan] Initialized {Width}x{Height} display", width, height);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Vulkan] Initialization failed: {Message}", ex.Message);
                return false;
            }
        }
    }

    private bool CreateInstance()
    {
        var appInfo = new ApplicationInfo
        {
            SType = StructureType.ApplicationInfo,
            PApplicationName = (byte*)Marshal.StringToHGlobalAnsi("Win32Emu"),
            ApplicationVersion = new Version32(1, 0, 0),
            PEngineName = (byte*)Marshal.StringToHGlobalAnsi("Win32Emu"),
            EngineVersion = new Version32(1, 0, 0),
            ApiVersion = Vk.Version10
        };

        // Get required extensions from window
        var extensions = _window!.VkSurface!.GetRequiredExtensions(out var extCount);

        var createInfo = new InstanceCreateInfo
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &appInfo,
            EnabledExtensionCount = extCount,
            PpEnabledExtensionNames = extensions,
            EnabledLayerCount = 0
        };

        var result = _vk!.CreateInstance(&createInfo, null, out _instance);

        // Free allocated strings
        Marshal.FreeHGlobal((IntPtr)appInfo.PApplicationName);
        Marshal.FreeHGlobal((IntPtr)appInfo.PEngineName);

        if (result != Result.Success)
        {
            _logger.LogError("[Vulkan] Failed to create instance: {Result}", result);
            return false;
        }

        // Load surface extension
        if (!_vk.TryGetInstanceExtension(_instance, out _khrSurface))
        {
            _logger.LogError("[Vulkan] Failed to load KHR_surface extension");
            return false;
        }

        return true;
    }

    private bool SelectPhysicalDevice()
    {
        uint deviceCount = 0;
        _vk!.EnumeratePhysicalDevices(_instance, &deviceCount, null);

        if (deviceCount == 0)
        {
            _logger.LogError("[Vulkan] No Vulkan-capable devices found");
            return false;
        }

        var devices = stackalloc PhysicalDevice[(int)deviceCount];
        _vk.EnumeratePhysicalDevices(_instance, &deviceCount, devices);

        // Select first device that supports graphics
        for (uint i = 0; i < deviceCount; i++)
        {
            _physicalDevice = devices[i];

            uint queueFamilyCount = 0;
            _vk.GetPhysicalDeviceQueueFamilyProperties(_physicalDevice, &queueFamilyCount, null);

            var queueFamilies = stackalloc QueueFamilyProperties[(int)queueFamilyCount];
            _vk.GetPhysicalDeviceQueueFamilyProperties(_physicalDevice, &queueFamilyCount, queueFamilies);

            for (uint j = 0; j < queueFamilyCount; j++)
            {
                if (queueFamilies[j].QueueFlags.HasFlag(QueueFlags.GraphicsBit))
                {
                    _graphicsQueueFamilyIndex = j;
                    
                    PhysicalDeviceProperties properties;
                    _vk.GetPhysicalDeviceProperties(_physicalDevice, &properties);
                    var deviceName = Marshal.PtrToStringAnsi((IntPtr)properties.DeviceName);
                    _logger.LogInformation("[Vulkan] Selected device: {DeviceName}", deviceName);
                    
                    return true;
                }
            }
        }

        return false;
    }

    private bool CreateLogicalDevice()
    {
        var queuePriority = 1.0f;
        var queueCreateInfo = new DeviceQueueCreateInfo
        {
            SType = StructureType.DeviceQueueCreateInfo,
            QueueFamilyIndex = _graphicsQueueFamilyIndex,
            QueueCount = 1,
            PQueuePriorities = &queuePriority
        };

        // Enable swapchain extension
        var swapchainExtensionName = KhrSwapchain.ExtensionName;
        var extensionNamePtr = (byte*)SilkMarshal.StringToPtr(swapchainExtensionName);

        var deviceCreateInfo = new DeviceCreateInfo
        {
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = 1,
            PQueueCreateInfos = &queueCreateInfo,
            EnabledExtensionCount = 1,
            PpEnabledExtensionNames = &extensionNamePtr
        };

        var result = _vk!.CreateDevice(_physicalDevice, &deviceCreateInfo, null, out _device);
        SilkMarshal.Free((nint)extensionNamePtr);

        if (result != Result.Success)
        {
            _logger.LogError("[Vulkan] Failed to create logical device: {Result}", result);
            return false;
        }

        _vk.GetDeviceQueue(_device, _graphicsQueueFamilyIndex, 0, out _graphicsQueue);

        // Load swapchain extension
        if (!_vk.TryGetDeviceExtension(_instance, _device, out _khrSwapchain))
        {
            _logger.LogError("[Vulkan] Failed to load KHR_swapchain extension");
            return false;
        }

        return true;
    }

    private bool CreateSwapchain()
    {
        // Query surface capabilities
        SurfaceCapabilitiesKHR capabilities;
        _khrSurface!.GetPhysicalDeviceSurfaceCapabilities(_physicalDevice, _surface, &capabilities);

        // Choose format
        uint formatCount = 0;
        _khrSurface.GetPhysicalDeviceSurfaceFormats(_physicalDevice, _surface, &formatCount, null);
        var formats = stackalloc SurfaceFormatKHR[(int)formatCount];
        _khrSurface.GetPhysicalDeviceSurfaceFormats(_physicalDevice, _surface, &formatCount, formats);

        var surfaceFormat = formats[0]; // Use first available format

        // Choose present mode
        uint presentModeCount = 0;
        _khrSurface.GetPhysicalDeviceSurfacePresentModes(_physicalDevice, _surface, &presentModeCount, null);
        var presentModes = stackalloc PresentModeKHR[(int)presentModeCount];
        _khrSurface.GetPhysicalDeviceSurfacePresentModes(_physicalDevice, _surface, &presentModeCount, presentModes);

        var presentMode = PresentModeKHR.FifoKhr; // Default to FIFO (vsync)

        // Determine extent
        var extent = capabilities.CurrentExtent;
        if (extent.Width == uint.MaxValue)
        {
            extent.Width = (uint)_width;
            extent.Height = (uint)_height;
        }

        var imageCount = capabilities.MinImageCount + 1;
        if (capabilities.MaxImageCount > 0 && imageCount > capabilities.MaxImageCount)
        {
            imageCount = capabilities.MaxImageCount;
        }

        var createInfo = new SwapchainCreateInfoKHR
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = _surface,
            MinImageCount = imageCount,
            ImageFormat = surfaceFormat.Format,
            ImageColorSpace = surfaceFormat.ColorSpace,
            ImageExtent = extent,
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit | ImageUsageFlags.TransferDstBit,
            ImageSharingMode = SharingMode.Exclusive,
            PreTransform = capabilities.CurrentTransform,
            CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
            PresentMode = presentMode,
            Clipped = true
        };

        var result = _khrSwapchain!.CreateSwapchain(_device, &createInfo, null, out _swapchain);
        if (result != Result.Success)
        {
            _logger.LogError("[Vulkan] Failed to create swapchain: {Result}", result);
            return false;
        }

        // Get swapchain images
        uint swapchainImageCount = 0;
        _khrSwapchain.GetSwapchainImages(_device, _swapchain, &swapchainImageCount, null);
        _swapchainImages = new Image[swapchainImageCount];
        fixed (Image* imagesPtr = _swapchainImages)
        {
            _khrSwapchain.GetSwapchainImages(_device, _swapchain, &swapchainImageCount, imagesPtr);
        }

        // Create image views
        _swapchainImageViews = new ImageView[swapchainImageCount];
        for (var i = 0; i < swapchainImageCount; i++)
        {
            var viewCreateInfo = new ImageViewCreateInfo
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = _swapchainImages[i],
                ViewType = ImageViewType.Type2D,
                Format = surfaceFormat.Format,
                Components = new ComponentMapping
                {
                    R = ComponentSwizzle.Identity,
                    G = ComponentSwizzle.Identity,
                    B = ComponentSwizzle.Identity,
                    A = ComponentSwizzle.Identity
                },
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1
                }
            };

            result = _vk!.CreateImageView(_device, &viewCreateInfo, null, out _swapchainImageViews[i]);
            if (result != Result.Success)
            {
                _logger.LogError("[Vulkan] Failed to create image view: {Result}", result);
                return false;
            }
        }

        return true;
    }

    private bool CreateStagingImage()
    {
        // Create staging image for CPU->GPU transfers
        var imageCreateInfo = new ImageCreateInfo
        {
            SType = StructureType.ImageCreateInfo,
            ImageType = ImageType.Type2D,
            Format = Format.R8G8B8A8Unorm,
            Extent = new Extent3D((uint)_width, (uint)_height, 1),
            MipLevels = 1,
            ArrayLayers = 1,
            Samples = SampleCountFlags.Count1Bit,
            Tiling = ImageTiling.Linear,
            Usage = ImageUsageFlags.TransferSrcBit,
            SharingMode = SharingMode.Exclusive,
            InitialLayout = ImageLayout.Preinitialized
        };

        var result = _vk!.CreateImage(_device, &imageCreateInfo, null, out _stagingImage);
        if (result != Result.Success)
        {
            _logger.LogError("[Vulkan] Failed to create staging image: {Result}", result);
            return false;
        }

        // Allocate memory
        MemoryRequirements memRequirements;
        _vk.GetImageMemoryRequirements(_device, _stagingImage, &memRequirements);

        var allocInfo = new MemoryAllocateInfo
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = memRequirements.Size,
            MemoryTypeIndex = FindMemoryType(memRequirements.MemoryTypeBits, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit)
        };

        result = _vk.AllocateMemory(_device, &allocInfo, null, out _stagingMemory);
        if (result != Result.Success)
        {
            _logger.LogError("[Vulkan] Failed to allocate staging memory: {Result}", result);
            return false;
        }

        _vk.BindImageMemory(_device, _stagingImage, _stagingMemory, 0);
        return true;
    }

    private uint FindMemoryType(uint typeFilter, MemoryPropertyFlags properties)
    {
        PhysicalDeviceMemoryProperties memProperties;
        _vk!.GetPhysicalDeviceMemoryProperties(_physicalDevice, &memProperties);

        for (uint i = 0; i < memProperties.MemoryTypeCount; i++)
        {
            if ((typeFilter & (1 << (int)i)) != 0 &&
                (memProperties.MemoryTypes[(int)i].PropertyFlags & properties) == properties)
            {
                return i;
            }
        }

        return 0;
    }

    private bool CreateCommandResources()
    {
        var poolInfo = new CommandPoolCreateInfo
        {
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = _graphicsQueueFamilyIndex,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit
        };

        var result = _vk!.CreateCommandPool(_device, &poolInfo, null, out _commandPool);
        if (result != Result.Success)
        {
            _logger.LogError("[Vulkan] Failed to create command pool: {Result}", result);
            return false;
        }

        var allocInfo = new CommandBufferAllocateInfo
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = _commandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = 1
        };

        result = _vk.AllocateCommandBuffers(_device, &allocInfo, out _commandBuffer);
        if (result != Result.Success)
        {
            _logger.LogError("[Vulkan] Failed to allocate command buffer: {Result}", result);
            return false;
        }

        return true;
    }

    private bool CreateSyncObjects()
    {
        var semaphoreInfo = new SemaphoreCreateInfo
        {
            SType = StructureType.SemaphoreCreateInfo
        };

        var fenceInfo = new FenceCreateInfo
        {
            SType = StructureType.FenceCreateInfo,
            Flags = FenceCreateFlags.SignaledBit
        };

        var result = _vk!.CreateSemaphore(_device, &semaphoreInfo, null, out _imageAvailableSemaphore);
        if (result != Result.Success) return false;

        result = _vk.CreateSemaphore(_device, &semaphoreInfo, null, out _renderFinishedSemaphore);
        if (result != Result.Success) return false;

        result = _vk.CreateFence(_device, &fenceInfo, null, out _inFlightFence);
        if (result != Result.Success) return false;

        return true;
    }

    public byte[] ConvertPalettizedToRGBA(byte[] indexedData, uint[] palette, int width, int height, int pitch)
    {
        var rgbaData = new byte[width * height * 4];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var srcOffset = y * pitch + x;
                var dstOffset = (y * width + x) * 4;

                if (srcOffset < indexedData.Length)
                {
                    var paletteIndex = indexedData[srcOffset];

                    if (paletteIndex < palette.Length)
                    {
                        var color = palette[paletteIndex];

                        rgbaData[dstOffset + 0] = (byte)(color & 0xFF);         // R
                        rgbaData[dstOffset + 1] = (byte)((color >> 8) & 0xFF);  // G
                        rgbaData[dstOffset + 2] = (byte)((color >> 16) & 0xFF); // B
                        rgbaData[dstOffset + 3] = 0xFF;                          // A
                    }
                }
            }
        }

        return rgbaData;
    }

    public byte[] Convert16BitToRGBA(byte[] rgb565Data, int width, int height, int pitch)
    {
        var rgbaData = new byte[width * height * 4];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var srcOffset = y * pitch + x * 2;
                var dstOffset = (y * width + x) * 4;

                if (srcOffset + 1 < rgb565Data.Length)
                {
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
                }
            }
        }

        return rgbaData;
    }

    public byte[] Convert24BitToRGBA(byte[] rgb24Data, int width, int height, int pitch)
    {
        var rgbaData = new byte[width * height * 4];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var srcOffset = y * pitch + x * 3;
                var dstOffset = (y * width + x) * 4;

                if (srcOffset + 2 < rgb24Data.Length)
                {
                    // 24-bit is typically BGR format in Windows
                    rgbaData[dstOffset + 0] = rgb24Data[srcOffset + 2]; // R
                    rgbaData[dstOffset + 1] = rgb24Data[srcOffset + 1]; // G
                    rgbaData[dstOffset + 2] = rgb24Data[srcOffset + 0]; // B
                    rgbaData[dstOffset + 3] = 0xFF;                      // A
                }
            }
        }

        return rgbaData;
    }

    public bool UpdateFrameBuffer(byte[] data, int pitch)
    {
        lock (_lock)
        {
            if (!_initialized || _vk == null)
            {
                return false;
            }

            try
            {
                // Wait for previous frame
                _vk.WaitForFences(_device, 1, in _inFlightFence, true, ulong.MaxValue);
                _vk.ResetFences(_device, 1, in _inFlightFence);

                // Acquire next image
                uint imageIndex = 0;
                var result = _khrSwapchain!.AcquireNextImage(_device, _swapchain, ulong.MaxValue, _imageAvailableSemaphore, default, &imageIndex);
                if (result != Result.Success)
                {
                    _logger.LogWarning("[Vulkan] Failed to acquire next image: {Result}", result);
                    return false;
                }

                // Copy data to staging image
                void* mappedData;
                _vk.MapMemory(_device, _stagingMemory, 0, (ulong)(data.Length), 0, &mappedData);
                fixed (byte* dataPtr = data)
                {
                    Unsafe.CopyBlock(mappedData, dataPtr, (uint)data.Length);
                }
                _vk.UnmapMemory(_device, _stagingMemory);

                // Record and submit command buffer
                RecordCommandBuffer(imageIndex);

                var waitSemaphore = _imageAvailableSemaphore;
                var signalSemaphore = _renderFinishedSemaphore;
                var waitStage = PipelineStageFlags.ColorAttachmentOutputBit;
                var commandBuffer = _commandBuffer;

                var submitInfo = new SubmitInfo
                {
                    SType = StructureType.SubmitInfo,
                    WaitSemaphoreCount = 1,
                    PWaitSemaphores = &waitSemaphore,
                    PWaitDstStageMask = &waitStage,
                    CommandBufferCount = 1,
                    PCommandBuffers = &commandBuffer,
                    SignalSemaphoreCount = 1,
                    PSignalSemaphores = &signalSemaphore
                };

                result = _vk.QueueSubmit(_graphicsQueue, 1, &submitInfo, _inFlightFence);
                if (result != Result.Success)
                {
                    _logger.LogWarning("[Vulkan] Failed to submit queue: {Result}", result);
                    return false;
                }

                // Present
                var swapchain = _swapchain;
                var presentInfo = new PresentInfoKHR
                {
                    SType = StructureType.PresentInfoKhr,
                    WaitSemaphoreCount = 1,
                    PWaitSemaphores = &signalSemaphore,
                    SwapchainCount = 1,
                    PSwapchains = &swapchain,
                    PImageIndices = &imageIndex
                };

                result = _khrSwapchain.QueuePresent(_graphicsQueue, &presentInfo);
                if (result != Result.Success && result != Result.SuboptimalKhr)
                {
                    _logger.LogWarning("[Vulkan] Failed to present: {Result}", result);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Vulkan] Update frame buffer failed: {Message}", ex.Message);
                return false;
            }
        }
    }

    private void RecordCommandBuffer(uint imageIndex)
    {
        var beginInfo = new CommandBufferBeginInfo
        {
            SType = StructureType.CommandBufferBeginInfo
        };

        _vk!.BeginCommandBuffer(_commandBuffer, &beginInfo);

        // Transition staging image layout
        var stagingBarrier = new ImageMemoryBarrier
        {
            SType = StructureType.ImageMemoryBarrier,
            SrcAccessMask = AccessFlags.HostWriteBit,
            DstAccessMask = AccessFlags.TransferReadBit,
            OldLayout = ImageLayout.Preinitialized,
            NewLayout = ImageLayout.TransferSrcOptimal,
            Image = _stagingImage,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                LevelCount = 1,
                LayerCount = 1
            }
        };

        _vk.CmdPipelineBarrier(_commandBuffer, PipelineStageFlags.HostBit, PipelineStageFlags.TransferBit,
            0, 0, null, 0, null, 1, &stagingBarrier);

        // Transition swapchain image to transfer dst
        var swapchainBarrier = new ImageMemoryBarrier
        {
            SType = StructureType.ImageMemoryBarrier,
            SrcAccessMask = 0,
            DstAccessMask = AccessFlags.TransferWriteBit,
            OldLayout = ImageLayout.Undefined,
            NewLayout = ImageLayout.TransferDstOptimal,
            Image = _swapchainImages[imageIndex],
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                LevelCount = 1,
                LayerCount = 1
            }
        };

        _vk.CmdPipelineBarrier(_commandBuffer, PipelineStageFlags.TopOfPipeBit, PipelineStageFlags.TransferBit,
            0, 0, null, 0, null, 1, &swapchainBarrier);

        // Copy from staging to swapchain
        var imageCopy = new ImageCopy
        {
            SrcSubresource = new ImageSubresourceLayers
            {
                AspectMask = ImageAspectFlags.ColorBit,
                LayerCount = 1
            },
            DstSubresource = new ImageSubresourceLayers
            {
                AspectMask = ImageAspectFlags.ColorBit,
                LayerCount = 1
            },
            Extent = new Extent3D((uint)_width, (uint)_height, 1)
        };

        _vk.CmdCopyImage(_commandBuffer, _stagingImage, ImageLayout.TransferSrcOptimal,
            _swapchainImages[imageIndex], ImageLayout.TransferDstOptimal, 1, &imageCopy);

        // Transition swapchain image to present
        swapchainBarrier.SrcAccessMask = AccessFlags.TransferWriteBit;
        swapchainBarrier.DstAccessMask = 0;
        swapchainBarrier.OldLayout = ImageLayout.TransferDstOptimal;
        swapchainBarrier.NewLayout = ImageLayout.PresentSrcKhr;

        _vk.CmdPipelineBarrier(_commandBuffer, PipelineStageFlags.TransferBit, PipelineStageFlags.BottomOfPipeBit,
            0, 0, null, 0, null, 1, &swapchainBarrier);

        _vk.EndCommandBuffer(_commandBuffer);
    }

    public void Clear(byte r, byte g, byte b, byte a = 255)
    {
        // Create clear color data
        var clearData = new byte[_width * _height * 4];
        for (var i = 0; i < _width * _height; i++)
        {
            clearData[i * 4 + 0] = r;
            clearData[i * 4 + 1] = g;
            clearData[i * 4 + 2] = b;
            clearData[i * 4 + 3] = a;
        }

        UpdateFrameBuffer(clearData, _width * 4);
    }

    public void ProcessEvents()
    {
        lock (_lock)
        {
            if (!_initialized || _window == null)
            {
                return;
            }

            _window.DoEvents();
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (!_initialized || _vk == null)
            {
                return;
            }

            _vk.DeviceWaitIdle(_device);

            _vk.DestroySemaphore(_device, _imageAvailableSemaphore, null);
            _vk.DestroySemaphore(_device, _renderFinishedSemaphore, null);
            _vk.DestroyFence(_device, _inFlightFence, null);
            _vk.DestroyCommandPool(_device, _commandPool, null);
            _vk.DestroyImage(_device, _stagingImage, null);
            _vk.FreeMemory(_device, _stagingMemory, null);

            foreach (var imageView in _swapchainImageViews)
            {
                _vk.DestroyImageView(_device, imageView, null);
            }

            _khrSwapchain?.DestroySwapchain(_device, _swapchain, null);
            _khrSurface?.DestroySurface(_instance, _surface, null);
            _vk.DestroyDevice(_device, null);
            _vk.DestroyInstance(_instance, null);

            _window?.Dispose();
            _vk.Dispose();

            _initialized = false;
            _logger.LogInformation("[Vulkan] Rendering backend disposed");
        }
    }

    public bool IsInitialized => _initialized;
    public int Width => _width;
    public int Height => _height;
}
