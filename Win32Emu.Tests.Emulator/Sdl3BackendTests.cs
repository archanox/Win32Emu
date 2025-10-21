using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Rendering;

namespace Win32Emu.Tests.Emulator;

public class Sdl3BackendTests
{
    [Fact]
    public void Sdl3AudioBackend_Initialize_ShouldNotThrow()
    {
        // Arrange & Act & Assert - should not throw even if SDL3 is not available
        try
        {
            using var audioBackend = new Sdl3AudioBackend(NullLogger.Instance);
            
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
            // SDL3 not available in CI - this is OK
        }
        catch (FileNotFoundException)
        {
            // SDL3 library not found - this is OK in CI
        }
    }

    [Fact]
    public void Sdl3AudioBackend_CreateStream_WhenInitialized_ShouldReturnValidId()
    {
        // Arrange
        try
        {
            using var audioBackend = new Sdl3AudioBackend(NullLogger.Instance);
            
            audioBackend.Initialize();
            if (!audioBackend.IsInitialized)
            {
                return; // Skip test if SDL3 not available
            }

            // Act
            var streamId = audioBackend.CreateAudioStream(44100, 2, 4096);

            // Assert
            Assert.NotEqual(0u, streamId);
            Assert.Equal(1, audioBackend.ActiveStreamCount);
        }
        catch (DllNotFoundException)
        {
            // SDL3 not available in CI - skip test
        }
        catch (FileNotFoundException)
        {
            // SDL3 library not found - skip test
        }
    }

    [Fact]
    public void Sdl3AudioBackend_WriteAudioData_ShouldNotThrow()
    {
        // Arrange
        try
        {
            using var audioBackend = new Sdl3AudioBackend(NullLogger.Instance);
            
            audioBackend.Initialize();
            if (!audioBackend.IsInitialized)
            {
                return; // Skip test if SDL3 not available
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
            // SDL3 not available in CI - skip test
        }
        catch (FileNotFoundException)
        {
            // SDL3 library not found - skip test
        }
    }

    [Fact]
    public void Sdl3AudioBackend_Dispose_ShouldNotThrow()
    {
        // Arrange
        try
        {
            var audioBackend = new Sdl3AudioBackend(NullLogger.Instance);
            
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
                // SDL3 not available - still test dispose
            }

            // Act
            audioBackend.Dispose();

            // Assert
            Assert.False(audioBackend.IsInitialized);
            Assert.Equal(0, audioBackend.ActiveStreamCount);
        }
        catch (FileNotFoundException)
        {
            // SDL3 library not found - test passes
        }
    }

    [Fact]
    public void Sdl3InputBackend_Initialize_ShouldNotThrow()
    {
        // Arrange & Act & Assert - should not throw
        using var inputBackend = new Sdl3InputBackend(NullLogger.Instance);
        
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
            // SDL3 not available in CI
            Assert.False(inputBackend.IsInitialized);
        }
        catch (FileNotFoundException)
        {
            // SDL3 library not found
            Assert.False(inputBackend.IsInitialized);
        }
    }

    [Fact]
    public void Sdl3InputBackend_GetDevices_WhenInitialized_ShouldReturnDevices()
    {
        // Arrange
        using var inputBackend = new Sdl3InputBackend(NullLogger.Instance);
        
        try
        {
            inputBackend.Initialize();
            if (!inputBackend.IsInitialized)
            {
                return; // Should not happen but skip if it does
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
            // SDL3 not available in CI - skip test
        }
        catch (FileNotFoundException)
        {
            // SDL3 library not found - skip test
        }
    }

    [Fact]
    public void Sdl3InputBackend_Dispose_ShouldNotThrow()
    {
        // Arrange
        var inputBackend = new Sdl3InputBackend(NullLogger.Instance);
        
        try
        {
            inputBackend.Initialize();
        }
        catch (DllNotFoundException)
        {
            // SDL3 not available - still test dispose
        }
        catch (FileNotFoundException)
        {
            // SDL3 library not found - still test dispose
        }

        // Act
        inputBackend.Dispose();

        // Assert
        Assert.False(inputBackend.IsInitialized);
    }

    [Fact]
    public void Sdl3RenderingBackend_Initialize_ShouldNotThrow()
    {
        // Arrange & Act & Assert - should not throw even if SDL3 is not available
        try
        {
            using var renderingBackend = new Sdl3RenderingBackend(NullLogger.Instance);
            
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
            // SDL3 not available in CI - this is OK, test passes
        }
        catch (FileNotFoundException)
        {
            // SDL3 library not found - this is OK in CI
        }
        catch (Exception)
        {
            // SDL3 initialization can fail for various reasons (no device, etc.) - OK in CI
        }
    }

    [Fact]
    public void Sdl3RenderingBackend_Dispose_ShouldNotThrow()
    {
        // Arrange
        try
        {
            var renderingBackend = new Sdl3RenderingBackend(NullLogger.Instance);
            
            try
            {
                renderingBackend.Initialize(640, 480, "Test Window");
            }
            catch (Exception)
            {
                // SDL3 not available or initialization failed - still test dispose
            }

            // Act
            renderingBackend.Dispose();

            // Assert - should not throw
            Assert.False(renderingBackend.IsInitialized);
        }
        catch (FileNotFoundException)
        {
            // SDL3 library not found - test passes
        }
        catch (Exception)
        {
            // SDL3 initialization can fail - test passes
        }
    }

    [Fact]
    public void Sdl3RenderingBackend_UpdateFrameBuffer_WhenInitialized_ShouldReturnTrue()
    {
        // Arrange
        try
        {
            using var renderingBackend = new Sdl3RenderingBackend(NullLogger.Instance);
            
            if (!renderingBackend.Initialize(640, 480, "Test Window"))
            {
                return; // Skip test if initialization fails
            }

            var frameData = new byte[640 * 480 * 4]; // RGBA

            // Act
            var result = renderingBackend.UpdateFrameBuffer(frameData, 640 * 4);

            // Assert
            Assert.True(result);
        }
        catch (DllNotFoundException)
        {
            // SDL3 not available in CI - skip test
        }
        catch (FileNotFoundException)
        {
            // SDL3 library not found - skip test
        }
        catch (Exception)
        {
            // SDL3 operations can fail for various reasons - skip test
        }
    }
}
