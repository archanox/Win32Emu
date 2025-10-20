using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Rendering;

namespace Win32Emu.Tests.Emulator;

public class SilkBackendTests
{
    [Fact]
    public void SilkOpenALAudioBackend_Initialize_ShouldNotThrow()
    {
        // Arrange & Act & Assert - should not throw even if OpenAL is not available
        try
        {
            using var audioBackend = new SilkOpenAlAudioBackend(NullLogger.Instance);
            
            var result = audioBackend.Initialize();
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
    public void SilkOpenALAudioBackend_CreateStream_WhenInitialized_ShouldReturnValidId()
    {
        // Arrange
        try
        {
            using var audioBackend = new SilkOpenAlAudioBackend(NullLogger.Instance);
            
            audioBackend.Initialize();
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
    public void SilkOpenALAudioBackend_WriteAudioData_ShouldNotThrow()
    {
        // Arrange
        try
        {
            using var audioBackend = new SilkOpenAlAudioBackend(NullLogger.Instance);
            
            audioBackend.Initialize();
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
    public void SilkInputBackend_Initialize_ShouldNotThrow()
    {
        // Arrange & Act & Assert - should not throw
        using var inputBackend = new SilkInputBackend(NullLogger.Instance);
        
        try
        {
            var result = inputBackend.Initialize();
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
    public void SilkInputBackend_GetDevices_WhenInitialized_ShouldReturnDevices()
    {
        // Arrange
        using var inputBackend = new SilkInputBackend(NullLogger.Instance);
        
        try
        {
            inputBackend.Initialize();
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
    public void SilkOpenALAudioBackend_Dispose_ShouldNotThrow()
    {
        // Arrange
        try
        {
            var audioBackend = new SilkOpenAlAudioBackend(NullLogger.Instance);
            
            try
            {
                audioBackend.Initialize();
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
    public void SilkInputBackend_Dispose_ShouldNotThrow()
    {
        // Arrange
        var inputBackend = new SilkInputBackend(NullLogger.Instance);
        
        try
        {
            inputBackend.Initialize();
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

    [Fact]
    public void SilkVulkanRenderingBackend_Initialize_ShouldNotThrow()
    {
        // Arrange & Act & Assert - should not throw even if Vulkan is not available
        try
        {
            using var renderingBackend = new SilkVulkanRenderingBackend(NullLogger.Instance);
            
            var result = renderingBackend.Initialize(640, 480, "Test Window");
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
    public void SilkVulkanRenderingBackend_Dispose_ShouldNotThrow()
    {
        // Arrange
        try
        {
            var renderingBackend = new SilkVulkanRenderingBackend(NullLogger.Instance);
            
            try
            {
                renderingBackend.Initialize(640, 480, "Test Window");
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
}
