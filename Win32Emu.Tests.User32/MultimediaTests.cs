using Win32Emu.Tests.User32.TestInfrastructure;

namespace Win32Emu.Tests.User32;

/// <summary>
/// Tests for multimedia APIs (DirectSound, DirectInput, WinMM)
/// </summary>
public class MultimediaTests : IDisposable
{
    private readonly TestEnvironment _testEnv;

    public MultimediaTests()
    {
        _testEnv = new TestEnvironment();
    }

    [Fact]
    public void DirectSoundCreate_ShouldReturnSuccess()
    {
        // Arrange
        var lplpDs = _testEnv.AllocateMemory(4);

        // Act
        var result = _testEnv.CallDSoundApi("DIRECTSOUNDCREATE", 0u, lplpDs, 0u);

        // Assert
        Assert.Equal(0u, result); // DS_OK
        var dsHandle = _testEnv.Memory.Read32(lplpDs);
        Assert.NotEqual(0u, dsHandle);
    }

    [Fact]
    public void DirectInputCreateA_ShouldReturnSuccess()
    {
        // Arrange
        var lplpDirectInput = _testEnv.AllocateMemory(4);

        // Act
        var result = _testEnv.CallDInputApi("DIRECTINPUTCREATEA", 0u, 0x0300u, lplpDirectInput, 0u);

        // Assert
        Assert.Equal(0u, result); // DI_OK
        var diHandle = _testEnv.Memory.Read32(lplpDirectInput);
        Assert.NotEqual(0u, diHandle);
    }

    [Fact]
    public void TimeGetTime_ShouldReturnIncreasingValues()
    {
        // Act
        var time1 = _testEnv.CallWinMmApi("TIMEGETTIME");
        Thread.Sleep(10); // Sleep 10ms
        var time2 = _testEnv.CallWinMmApi("TIMEGETTIME");

        // Assert
        Assert.True(time2 >= time1); // Time should not go backwards
    }

    [Fact]
    public void TimeBeginPeriod_ShouldReturnSuccess()
    {
        // Act
        var result = _testEnv.CallWinMmApi("TIMEBEGINPERIOD", 1u);

        // Assert
        Assert.Equal(0u, result); // TIMERR_NOERROR
    }

    [Fact]
    public void TimeEndPeriod_ShouldReturnSuccess()
    {
        // Act
        var result = _testEnv.CallWinMmApi("TIMEENDPERIOD", 1u);

        // Assert
        Assert.Equal(0u, result); // TIMERR_NOERROR
    }

    [Fact]
    public void TimeKillEvent_ShouldReturnSuccess()
    {
        // Act
        var result = _testEnv.CallWinMmApi("TIMEKILLEVENT", 1u);

        // Assert
        Assert.Equal(0u, result); // TIMERR_NOERROR
    }

    [Fact]
    public void GetDeviceCaps_ShouldReturnResolution()
    {
        // Arrange
        var hdc = 0x81000000;

        // Act - HORZRES = 8
        var width = _testEnv.CallGdi32Api("GETDEVICECAPS", hdc, 8);

        // Assert
        Assert.True(width > 0);
    }

    public void Dispose()
    {
        _testEnv?.Dispose();
    }
}
