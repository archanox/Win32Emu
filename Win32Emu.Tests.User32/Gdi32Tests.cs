using Win32Emu.Tests.User32.TestInfrastructure;
using Win32Emu.Win32;

namespace Win32Emu.Tests.User32;

/// <summary>
/// Tests for GDI32 functions like GetStockObject
/// </summary>
public class Gdi32Tests : IDisposable
{
    private readonly TestEnvironment _testEnv;

    public Gdi32Tests()
    {
        _testEnv = new TestEnvironment();
    }

    [Fact]
    public void GetStockObject_WhiteBrush_ShouldReturnValidHandle()
    {
        // Act
        var handle = _testEnv.CallGdi32Api("GETSTOCKOBJECT", NativeTypes.StockObject.WHITE_BRUSH);

        // Assert
        Assert.NotEqual(0u, handle);
    }

    [Fact]
    public void GetStockObject_BlackBrush_ShouldReturnValidHandle()
    {
        // Act
        var handle = _testEnv.CallGdi32Api("GETSTOCKOBJECT", NativeTypes.StockObject.BLACK_BRUSH);

        // Assert
        Assert.NotEqual(0u, handle);
    }

    [Fact]
    public void GetStockObject_DefaultGuiFont_ShouldReturnValidHandle()
    {
        // Act
        var handle = _testEnv.CallGdi32Api("GETSTOCKOBJECT", NativeTypes.StockObject.DEFAULT_GUI_FONT);

        // Assert
        Assert.NotEqual(0u, handle);
    }

    [Fact]
    public void GetStockObject_SystemFont_ShouldReturnValidHandle()
    {
        // Act
        var handle = _testEnv.CallGdi32Api("GETSTOCKOBJECT", NativeTypes.StockObject.SYSTEM_FONT);

        // Assert
        Assert.NotEqual(0u, handle);
    }

    [Fact]
    public void GetStockObject_NullBrush_ShouldReturnValidHandle()
    {
        // Act
        var handle = _testEnv.CallGdi32Api("GETSTOCKOBJECT", NativeTypes.StockObject.NULL_BRUSH);

        // Assert
        Assert.NotEqual(0u, handle);
    }

    [Fact]
    public void GetStockObject_CalledTwice_ShouldReturnSameHandle()
    {
        // Act
        var handle1 = _testEnv.CallGdi32Api("GETSTOCKOBJECT", NativeTypes.StockObject.DEFAULT_GUI_FONT);
        var handle2 = _testEnv.CallGdi32Api("GETSTOCKOBJECT", NativeTypes.StockObject.DEFAULT_GUI_FONT);

        // Assert - same stock object should return same handle
        Assert.Equal(handle1, handle2);
    }

    [Fact]
    public void GetStockObject_DifferentObjects_ShouldReturnDifferentHandles()
    {
        // Act
        var handle1 = _testEnv.CallGdi32Api("GETSTOCKOBJECT", NativeTypes.StockObject.WHITE_BRUSH);
        var handle2 = _testEnv.CallGdi32Api("GETSTOCKOBJECT", NativeTypes.StockObject.BLACK_BRUSH);

        // Assert - different stock objects should return different handles
        Assert.NotEqual(handle1, handle2);
    }

    [Fact]
    public void GetStockObject_InvalidStockObject_ShouldReturnNull()
    {
        // Act
        var handle = _testEnv.CallGdi32Api("GETSTOCKOBJECT", 999); // Invalid stock object ID

        // Assert
        Assert.Equal(0u, handle);
    }

    public void Dispose()
    {
        _testEnv?.Dispose();
    }
}
