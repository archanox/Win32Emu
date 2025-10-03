using Win32Emu.Rendering;

namespace Win32Emu.Tests.Emulator;

public class Sdl3BackendTests
{
    [Fact]
    public void SDL3AudioBackend_Initialize_ShouldNotThrow()
    {
        // Arrange & Act & Assert - should not throw even if SDL3 is not available
        using var audioBackend = new Sdl3AudioBackend();
        
        try
        {
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
            Assert.False(audioBackend.IsInitialized);
        }
    }

    [Fact]
    public void SDL3AudioBackend_CreateStream_WhenInitialized_ShouldReturnValidId()
    {
        // Arrange
        using var audioBackend = new Sdl3AudioBackend();
        
        try
        {
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
    }

    [Fact]
    public void SDL3AudioBackend_WriteAudioData_ShouldNotThrow()
    {
        // Arrange
        using var audioBackend = new Sdl3AudioBackend();
        
        try
        {
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
    }

    [Fact]
    public void SDL3InputBackend_Initialize_ShouldNotThrow()
    {
        // Arrange & Act & Assert - should not throw even if SDL3 is not available
        using var inputBackend = new Sdl3InputBackend();
        
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
            // SDL3 not available in CI - this is OK
            Assert.False(inputBackend.IsInitialized);
        }
    }

    [Fact]
    public void SDL3InputBackend_GetDevices_WhenInitialized_ShouldReturnDevices()
    {
        // Arrange
        using var inputBackend = new Sdl3InputBackend();
        
        try
        {
            inputBackend.Initialize();
            if (!inputBackend.IsInitialized)
            {
	            return; // Skip test if SDL3 not available
            }

            // Act
            var devices = inputBackend.GetDevices();

            // Assert
            Assert.NotNull(devices);
            // Should at least have keyboard and mouse
            Assert.True(devices.Count >= 2);
            Assert.Contains(devices, d => d.Type == Sdl3InputBackend.DeviceType.Keyboard);
            Assert.Contains(devices, d => d.Type == Sdl3InputBackend.DeviceType.Mouse);
        }
        catch (DllNotFoundException)
        {
            // SDL3 not available in CI - skip test
        }
    }

    [Fact]
    public void SDL3AudioBackend_Dispose_ShouldNotThrow()
    {
        // Arrange
        var audioBackend = new Sdl3AudioBackend();
        
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

    [Fact]
    public void SDL3InputBackend_Dispose_ShouldNotThrow()
    {
        // Arrange
        var inputBackend = new Sdl3InputBackend();
        
        try
        {
            inputBackend.Initialize();
        }
        catch (DllNotFoundException)
        {
            // SDL3 not available - still test dispose
        }

        // Act
        inputBackend.Dispose();

        // Assert
        Assert.False(inputBackend.IsInitialized);
        Assert.Equal(0, inputBackend.DeviceCount);
    }
}
