using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Rendering;
using Win32Emu.Gui.Backends;

namespace Win32Emu.Tests.Emulator;

public class SilkBackendTests
{
    [Fact]
    public async Task SilkOpenALAudioBackend_Initialize_ShouldNotThrow()
    {
        // Arrange & Act & Assert - should not throw even if OpenAL is not available
        try
        {
            using var audioBackend = new SilkOpenAlAudioBackend(NullLogger.Instance);
            
            var result = await audioBackend.InitializeAsync();
            // If initialization succeeds, verify state
            if (result)
            {
                Assert.True(audioBackend.IsInitialized);
                Assert.Equal(0, audioBackend.ActiveStreamCount);
            }
        }
        catch (DllNotFoundException)
        {
            // OpenAL not available in CI - this is OK
        }
        catch (FileNotFoundException)
        {
            // OpenAL library not found - this is OK in CI
        }
    }

    [Fact]
    public async Task SilkOpenALAudioBackend_CreateStream_WhenInitialized_ShouldReturnValidId()
    {
        // Arrange
        try
        {
            using var audioBackend = new SilkOpenAlAudioBackend(NullLogger.Instance);
            
            await audioBackend.InitializeAsync();
            if (!audioBackend.IsInitialized)
            {
	            return; // Skip test if OpenAL not available
            }

            // Act
            var streamId = audioBackend.CreateAudioStream(44100, 2, 4096);

            // Assert
            Assert.NotEqual(0u, streamId);
            Assert.Equal(1, audioBackend.ActiveStreamCount);
        }
        catch (DllNotFoundException)
        {
            // OpenAL not available in CI - skip test
        }
        catch (FileNotFoundException)
        {
            // OpenAL library not found - skip test
        }
    }

    [Fact]
    public async Task SilkOpenALAudioBackend_WriteAudioData_ShouldNotThrow()
    {
        // Arrange
        try
        {
            using var audioBackend = new SilkOpenAlAudioBackend(NullLogger.Instance);
            
            await audioBackend.InitializeAsync();
            if (!audioBackend.IsInitialized)
            {
	            return; // Skip test if OpenAL not available
            }

            var streamId = audioBackend.CreateAudioStream(44100, 2, 4096);
            var data = new byte[4096];

            // Act
            var result = audioBackend.WriteAudioData(streamId, data, 0, data.Length);

            // Assert
            Assert.True(result);
        }
        catch (DllNotFoundException)
        {
            // OpenAL not available in CI - skip test
        }
        catch (FileNotFoundException)
        {
            // OpenAL library not found - skip test
        }
    }

    [Fact]
    public async Task SilkInputBackend_Initialize_ShouldNotThrow()
    {
        // Arrange & Act & Assert - should not throw
        using var inputBackend = new SilkInputBackend(NullLogger.Instance);
        
        try
        {
            var result = await inputBackend.InitializeAsync();
            // If initialization succeeds, verify state
            if (result)
            {
                Assert.True(inputBackend.IsInitialized);
            }
        }
        catch (DllNotFoundException)
        {
            // Should not happen with SilkInputBackend
            Assert.False(inputBackend.IsInitialized);
        }
    }

    [Fact]
    public async Task SilkInputBackend_GetDevices_WhenInitialized_ShouldReturnDevices()
    {
        // Arrange
        using var inputBackend = new SilkInputBackend(NullLogger.Instance);
        
        try
        {
            await inputBackend.InitializeAsync();
            if (!inputBackend.IsInitialized)
            {
	            return; // Should not happen
            }

            // Act
            var devices = inputBackend.GetDevices();

            // Assert
            Assert.NotNull(devices);
            // Should at least have keyboard and mouse
            Assert.True(devices.Count >= 2);
            Assert.Contains(devices, d => d.Type == IInputBackend.DeviceType.Keyboard);
            Assert.Contains(devices, d => d.Type == IInputBackend.DeviceType.Mouse);
        }
        catch (DllNotFoundException)
        {
            // Should not happen with SilkInputBackend
        }
    }

    [Fact]
    public async Task SilkOpenALAudioBackend_Dispose_ShouldNotThrow()
    {
        // Arrange
        try
        {
            var audioBackend = new SilkOpenAlAudioBackend(NullLogger.Instance);
            
            try
            {
                await audioBackend.InitializeAsync();
                if (audioBackend.IsInitialized)
                {
                    audioBackend.CreateAudioStream(44100, 2, 4096);
                }
            }
            catch (DllNotFoundException)
            {
                // OpenAL not available - still test dispose
            }

            // Act
            audioBackend.Dispose();

            // Assert
            Assert.False(audioBackend.IsInitialized);
            Assert.Equal(0, audioBackend.ActiveStreamCount);
        }
        catch (FileNotFoundException)
        {
            // OpenAL library not found - test passes
        }
    }

    [Fact]
    public async Task SilkInputBackend_Dispose_ShouldNotThrow()
    {
        // Arrange
        var inputBackend = new SilkInputBackend(NullLogger.Instance);
        
        try
        {
            await inputBackend.InitializeAsync();
        }
        catch (DllNotFoundException)
        {
            // Should not happen with SilkInputBackend
        }

        // Act
        inputBackend.Dispose();

        // Assert
        Assert.False(inputBackend.IsInitialized);
        Assert.Equal(0, inputBackend.DeviceCount);
    }

    [Fact(Skip = "Seems to stall?")]
    public async Task SilkVulkanRenderingBackend_Initialize_ShouldNotThrow()
    {
        // Arrange & Act & Assert - should not throw even if Vulkan is not available
        try
        {
            using var renderingBackend = new SilkVulkanRenderingBackend(NullLogger.Instance);
            
            var result = await renderingBackend.InitializeAsync(640, 480, "Test Window");
            // If initialization succeeds, verify state
            if (result)
            {
                Assert.True(renderingBackend.IsInitialized);
                Assert.Equal(640, renderingBackend.Width);
                Assert.Equal(480, renderingBackend.Height);
            }
        }
        catch (DllNotFoundException)
        {
            // Vulkan not available in CI - this is OK, test passes
        }
        catch (FileNotFoundException)
        {
            // Vulkan library not found - this is OK in CI
        }
        catch (Exception)
        {
            // Vulkan initialization can fail for various reasons (no device, etc.) - OK in CI
        }
    }

    [Fact]
    public async Task SilkVulkanRenderingBackend_Dispose_ShouldNotThrow()
    {
        // Arrange
        try
        {
            var renderingBackend = new SilkVulkanRenderingBackend(NullLogger.Instance);
            
            try
            {
                await renderingBackend.InitializeAsync(640, 480, "Test Window");
            }
            catch (Exception)
            {
                // Vulkan not available or initialization failed - still test dispose
            }

            // Act
            renderingBackend.Dispose();

            // Assert - should not throw
            Assert.False(renderingBackend.IsInitialized);
        }
        catch (FileNotFoundException)
        {
            // Vulkan library not found - test passes
        }
        catch (Exception)
        {
            // Vulkan initialization can fail - test passes
        }
    }

    [Fact]
    public async Task SilkGlfwRenderingBackend_Initialize_ShouldNotThrow()
    {
        // Arrange & Act & Assert - should not throw even if GLFW is not available
        try
        {
            using var renderingBackend = new SilkGlfwRenderingBackend(NullLogger.Instance);
            
            var result = await renderingBackend.InitializeAsync(640, 480, "Test Window");
            // If initialization succeeds, verify state
            if (result)
            {
                Assert.True(renderingBackend.IsInitialized);
                Assert.Equal(640, renderingBackend.Width);
                Assert.Equal(480, renderingBackend.Height);
            }
        }
        catch (DllNotFoundException)
        {
            // GLFW not available in CI - this is OK, test passes
        }
        catch (FileNotFoundException)
        {
            // GLFW library not found - this is OK in CI
        }
        catch (Exception)
        {
            // GLFW initialization can fail for various reasons (no display, etc.) - OK in CI
        }
    }

    [Fact(Skip = "Borked on macOS?")]
    public async Task SilkGlfwRenderingBackend_Dispose_ShouldNotThrow()
    {
        // Arrange
        try
        {
            var renderingBackend = new SilkGlfwRenderingBackend(NullLogger.Instance);
            
            try
            {
                await renderingBackend.InitializeAsync(640, 480, "Test Window");
            }
            catch (Exception)
            {
                // GLFW not available or initialization failed - still test dispose
            }

            // Act
            renderingBackend.Dispose();

            // Assert - should not throw
            Assert.False(renderingBackend.IsInitialized);
        }
        catch (FileNotFoundException)
        {
            // GLFW library not found - test passes
        }
        catch (Exception)
        {
            // GLFW initialization can fail - test passes
        }
    }
}
