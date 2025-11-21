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
        var handle = _testEnv.CallGdi32Api("GETSTOCKOBJECT", (int)NativeTypes.StockObject.WHITE_BRUSH);

        // Assert
        Assert.NotEqual(0u, handle);
    }

    [Fact]
    public void GetStockObject_BlackBrush_ShouldReturnValidHandle()
    {
        // Act
        var handle = _testEnv.CallGdi32Api("GETSTOCKOBJECT", (int)NativeTypes.StockObject.BLACK_BRUSH);

        // Assert
        Assert.NotEqual(0u, handle);
    }

    [Fact]
    public void GetStockObject_DefaultGuiFont_ShouldReturnValidHandle()
    {
        // Act
        var handle = _testEnv.CallGdi32Api("GETSTOCKOBJECT", (int)NativeTypes.StockObject.DEFAULT_GUI_FONT);

        // Assert
        Assert.NotEqual(0u, handle);
    }

    [Fact]
    public void GetStockObject_SystemFont_ShouldReturnValidHandle()
    {
        // Act
        var handle = _testEnv.CallGdi32Api("GETSTOCKOBJECT", (int)NativeTypes.StockObject.SYSTEM_FONT);

        // Assert
        Assert.NotEqual(0u, handle);
    }

    [Fact]
    public void GetStockObject_NullBrush_ShouldReturnValidHandle()
    {
        // Act
        var handle = _testEnv.CallGdi32Api("GETSTOCKOBJECT", (int)NativeTypes.StockObject.NULL_BRUSH);

        // Assert
        Assert.NotEqual(0u, handle);
    }

    [Fact]
    public void GetStockObject_CalledTwice_ShouldReturnSameHandle()
    {
        // Act
        var handle1 = _testEnv.CallGdi32Api("GETSTOCKOBJECT", (int)NativeTypes.StockObject.DEFAULT_GUI_FONT);
        var handle2 = _testEnv.CallGdi32Api("GETSTOCKOBJECT", (int)NativeTypes.StockObject.DEFAULT_GUI_FONT);

        // Assert - same stock object should return same handle
        Assert.Equal(handle1, handle2);
    }

    [Fact]
    public void GetStockObject_DifferentObjects_ShouldReturnDifferentHandles()
    {
        // Act
        var handle1 = _testEnv.CallGdi32Api("GETSTOCKOBJECT", (int)NativeTypes.StockObject.WHITE_BRUSH);
        var handle2 = _testEnv.CallGdi32Api("GETSTOCKOBJECT", (int)NativeTypes.StockObject.BLACK_BRUSH);

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

    [Fact]
    public void BeginPaint_ShouldReturnValidHDC()
    {
        // Arrange
        uint hwnd = 0x00010000;
        var lpPaint = _testEnv.AllocateMemory(64); // PAINTSTRUCT size

        // Act
        var hdc = _testEnv.CallGdi32Api("BEGINPAINT", hwnd, lpPaint);

        // Assert
        Assert.NotEqual(0u, hdc);
        
        // Verify PAINTSTRUCT was filled
        var hdcFromStruct = _testEnv.Memory.Read32(lpPaint);
        Assert.Equal(hdc, hdcFromStruct);
    }

    [Fact]
    public void EndPaint_ShouldReturnTrue()
    {
        // Arrange
        uint hwnd = 0x00010000;
        var lpPaint = _testEnv.AllocateMemory(64);
        _testEnv.CallGdi32Api("BEGINPAINT", hwnd, lpPaint);

        // Act
        var result = _testEnv.CallGdi32Api("ENDPAINT", hwnd, lpPaint);

        // Assert
        Assert.Equal(1u, result); // TRUE
    }

    [Fact]
    public void FillRect_ShouldReturnSuccess()
    {
        // Arrange
        var hdc = 0x81000000;
        var lpRect = _testEnv.AllocateMemory(16);
        _testEnv.Memory.Write32(lpRect, 10);      // left
        _testEnv.Memory.Write32(lpRect + 4, 10);  // top
        _testEnv.Memory.Write32(lpRect + 8, 100); // right
        _testEnv.Memory.Write32(lpRect + 12, 100); // bottom
        var hBrush = 0x80000000;

        // Act
        var result = _testEnv.CallGdi32Api("FILLRECT", hdc, lpRect, hBrush);

        // Assert
        Assert.NotEqual(0u, result); // Non-zero on success
    }

    [Fact]
    public void TextOutA_ShouldReturnTrue()
    {
        // Arrange
        var hdc = 0x81000000;
        var text = "Hello, World!";
        var lpString = _testEnv.WriteString(text);

        // Act
        var result = _testEnv.CallGdi32Api("TEXTOUTA", hdc, 10, 20, lpString, (uint)text.Length);

        // Assert
        Assert.Equal(1u, result); // TRUE
    }

    [Fact]
    public void SetBkMode_ShouldReturnPreviousMode()
    {
        // Arrange
        uint hwnd = 0x00010000;
        var lpPaint = _testEnv.AllocateMemory(64);
        var hdc = _testEnv.CallGdi32Api("BEGINPAINT", hwnd, lpPaint);
        var transparent = 1;

        // Act
        var result = _testEnv.CallGdi32Api("SETBKMODE", hdc, (uint)transparent);

        // Assert
        Assert.NotEqual(0u, result); // Should return previous mode (OPAQUE = 2)
        
        // Cleanup
        _testEnv.CallGdi32Api("ENDPAINT", hwnd, lpPaint);
    }

    [Fact]
    public void SetTextColor_ShouldReturnPreviousColor()
    {
        // Arrange
        var hdc = 0x81000000;
        uint rgbRed = 0x000000FF;

        // Act
        var result = _testEnv.CallGdi32Api("SETTEXTCOLOR", hdc, rgbRed);

        // Assert - should return previous color (black = 0x00000000)
        Assert.Equal(0u, result);
    }

    [Fact]
    public void Ellipse_ShouldReturnTrue()
    {
        // Arrange
        var hdc = 0x81000000u;

        // Act
        var result = _testEnv.CallGdi32Api("ELLIPSE", hdc, 10, 10, 100, 100);

        // Assert
        Assert.Equal(1u, result); // TRUE
    }

    [Fact]
    public void Arc_ShouldReturnTrue()
    {
        // Arrange
        var hdc = 0x81000000u;

        // Act
        var result = _testEnv.CallGdi32Api("ARC", hdc, 10, 10, 100, 100, 50, 10, 10, 50);

        // Assert
        Assert.Equal(1u, result); // TRUE
    }

    [Fact]
    public void Polyline_ShouldReturnTrue()
    {
        // Arrange
        var hdc = 0x81000000u;
        var points = _testEnv.AllocateMemory(16); // 2 POINT structures (8 bytes each)
        _testEnv.Memory.Write32(points, 10);      // x1
        _testEnv.Memory.Write32(points + 4, 10);  // y1
        _testEnv.Memory.Write32(points + 8, 100); // x2
        _testEnv.Memory.Write32(points + 12, 100); // y2

        // Act
        var result = _testEnv.CallGdi32Api("POLYLINE", hdc, points, 2);

        // Assert
        Assert.Equal(1u, result); // TRUE
    }

    [Fact]
    public void DrawTextA_ShouldReturnHeight()
    {
        // Arrange
        var hdc = 0x81000000u;
        var text = "Hello, World!";
        var lpString = _testEnv.WriteString(text);
        var lpRect = _testEnv.AllocateMemory(16);
        _testEnv.Memory.Write32(lpRect, 10);      // left
        _testEnv.Memory.Write32(lpRect + 4, 10);  // top
        _testEnv.Memory.Write32(lpRect + 8, 200); // right
        _testEnv.Memory.Write32(lpRect + 12, 200); // bottom
        uint format = 0; // DT_LEFT | DT_TOP

        // Act
        var result = _testEnv.CallGdi32Api("DRAWTEXTA", hdc, lpString, (uint)text.Length, lpRect, format);

        // Assert
        Assert.NotEqual(0u, result); // Should return height of text
    }

    [Fact]
    public void FrameRect_ShouldReturnSuccess()
    {
        // Arrange
        var hdc = 0x81000000;
        var lpRect = _testEnv.AllocateMemory(16);
        _testEnv.Memory.Write32(lpRect, 10);      // left
        _testEnv.Memory.Write32(lpRect + 4, 10);  // top
        _testEnv.Memory.Write32(lpRect + 8, 100); // right
        _testEnv.Memory.Write32(lpRect + 12, 100); // bottom
        var hBrush = 0x80000000;

        // Act
        var result = _testEnv.CallGdi32Api("FRAMERECT", hdc, lpRect, hBrush);

        // Assert
        Assert.NotEqual(0u, result); // Non-zero on success
    }

    [Fact]
    public void InvertRect_ShouldReturnTrue()
    {
        // Arrange
        var hdc = 0x81000000;
        var lpRect = _testEnv.AllocateMemory(16);
        _testEnv.Memory.Write32(lpRect, 10);      // left
        _testEnv.Memory.Write32(lpRect + 4, 10);  // top
        _testEnv.Memory.Write32(lpRect + 8, 100); // right
        _testEnv.Memory.Write32(lpRect + 12, 100); // bottom

        // Act
        var result = _testEnv.CallGdi32Api("INVERTRECT", hdc, lpRect);

        // Assert
        Assert.Equal(1u, result); // TRUE
    }

    [Fact]
    public void StretchBlt_WithValidDCs_ShouldReturnTrue()
    {
        // Arrange - Create source and destination DCs
        var hdcDest = _testEnv.CallGdi32Api("CREATECOMPATIBLEDC", 0u);
        var hdcSrc = _testEnv.CallGdi32Api("CREATECOMPATIBLEDC", 0u);
        uint rop = 0x00CC0020; // SRCCOPY

        // Act
        var result = _testEnv.CallGdi32Api("STRETCHBLT", 
            hdcDest, 0, 0, 100, 100,  // Destination: (0, 0), size 100x100
            hdcSrc, 0, 0, 50, 50,     // Source: (0, 0), size 50x50
            rop);

        // Assert
        Assert.Equal(1u, result); // TRUE

        // Cleanup
        _testEnv.CallGdi32Api("DELETEDC", hdcDest);
        _testEnv.CallGdi32Api("DELETEDC", hdcSrc);
    }

    [Fact]
    public void StretchBlt_WithInvalidDestDC_ShouldReturnFalse()
    {
        // Arrange - Create only source DC
        var hdcSrc = _testEnv.CallGdi32Api("CREATECOMPATIBLEDC", 0u);
        uint invalidHdc = 0xDEADBEEF;
        uint rop = 0x00CC0020; // SRCCOPY

        // Act
        var result = _testEnv.CallGdi32Api("STRETCHBLT",
            invalidHdc, 0, 0, 100, 100,
            hdcSrc, 0, 0, 50, 50,
            rop);

        // Assert
        Assert.Equal(0u, result); // FALSE

        // Cleanup
        _testEnv.CallGdi32Api("DELETEDC", hdcSrc);
    }

    [Fact]
    public void StretchBlt_WithInvalidSrcDC_ShouldReturnFalse()
    {
        // Arrange - Create only destination DC
        var hdcDest = _testEnv.CallGdi32Api("CREATECOMPATIBLEDC", 0u);
        uint invalidHdc = 0xDEADBEEF;
        uint rop = 0x00CC0020; // SRCCOPY

        // Act
        var result = _testEnv.CallGdi32Api("STRETCHBLT",
            hdcDest, 0, 0, 100, 100,
            invalidHdc, 0, 0, 50, 50,
            rop);

        // Assert
        Assert.Equal(0u, result); // FALSE

        // Cleanup
        _testEnv.CallGdi32Api("DELETEDC", hdcDest);
    }

    [Fact]
    public void StretchBlt_WithBlackness_ShouldNotRequireSourceDC()
    {
        // Arrange - Create only destination DC
        var hdcDest = _testEnv.CallGdi32Api("CREATECOMPATIBLEDC", 0u);
        uint rop = 0x00000042; // BLACKNESS - doesn't require source

        // Act
        var result = _testEnv.CallGdi32Api("STRETCHBLT",
            hdcDest, 0, 0, 100, 100,
            0u, 0, 0, 0, 0,  // Source DC and dimensions are ignored for BLACKNESS
            rop);

        // Assert
        Assert.Equal(1u, result); // TRUE - should succeed without valid source DC

        // Cleanup
        _testEnv.CallGdi32Api("DELETEDC", hdcDest);
    }

    [Fact]
    public void StretchBlt_WithWhiteness_ShouldNotRequireSourceDC()
    {
        // Arrange - Create only destination DC
        var hdcDest = _testEnv.CallGdi32Api("CREATECOMPATIBLEDC", 0u);
        uint rop = 0x00FF0062; // WHITENESS - doesn't require source

        // Act
        var result = _testEnv.CallGdi32Api("STRETCHBLT",
            hdcDest, 0, 0, 100, 100,
            0u, 0, 0, 0, 0,  // Source DC and dimensions are ignored for WHITENESS
            rop);

        // Assert
        Assert.Equal(1u, result); // TRUE - should succeed without valid source DC

        // Cleanup
        _testEnv.CallGdi32Api("DELETEDC", hdcDest);
    }

    [Fact]
    public void StretchBlt_WithZeroDestWidth_ShouldReturnFalse()
    {
        // Arrange
        var hdcDest = _testEnv.CallGdi32Api("CREATECOMPATIBLEDC", 0u);
        var hdcSrc = _testEnv.CallGdi32Api("CREATECOMPATIBLEDC", 0u);
        uint rop = 0x00CC0020; // SRCCOPY

        // Act
        var result = _testEnv.CallGdi32Api("STRETCHBLT",
            hdcDest, 0, 0, 0, 100,  // Invalid: width = 0
            hdcSrc, 0, 0, 50, 50,
            rop);

        // Assert
        Assert.Equal(0u, result); // FALSE

        // Cleanup
        _testEnv.CallGdi32Api("DELETEDC", hdcDest);
        _testEnv.CallGdi32Api("DELETEDC", hdcSrc);
    }

    [Fact]
    public void StretchBlt_WithNegativeDestHeight_ShouldReturnFalse()
    {
        // Arrange
        var hdcDest = _testEnv.CallGdi32Api("CREATECOMPATIBLEDC", 0u);
        var hdcSrc = _testEnv.CallGdi32Api("CREATECOMPATIBLEDC", 0u);
        uint rop = 0x00CC0020; // SRCCOPY

        // Act
        var result = _testEnv.CallGdi32Api("STRETCHBLT",
            hdcDest, 0, 0, 100, unchecked((uint)-50),  // Invalid: height < 0
            hdcSrc, 0, 0, 50, 50,
            rop);

        // Assert
        Assert.Equal(0u, result); // FALSE

        // Cleanup
        _testEnv.CallGdi32Api("DELETEDC", hdcDest);
        _testEnv.CallGdi32Api("DELETEDC", hdcSrc);
    }

    [Fact]
    public void StretchBlt_WithZeroSourceDimensions_ShouldReturnFalse()
    {
        // Arrange
        var hdcDest = _testEnv.CallGdi32Api("CREATECOMPATIBLEDC", 0u);
        var hdcSrc = _testEnv.CallGdi32Api("CREATECOMPATIBLEDC", 0u);
        uint rop = 0x00CC0020; // SRCCOPY

        // Act
        var result = _testEnv.CallGdi32Api("STRETCHBLT",
            hdcDest, 0, 0, 100, 100,
            hdcSrc, 0, 0, 0, 0,  // Invalid: source dimensions = 0
            rop);

        // Assert
        Assert.Equal(0u, result); // FALSE

        // Cleanup
        _testEnv.CallGdi32Api("DELETEDC", hdcDest);
        _testEnv.CallGdi32Api("DELETEDC", hdcSrc);
    }

    [Fact]
    public void StretchBlt_WithDifferentRasterOps_ShouldReturnTrue()
    {
        // Arrange
        var hdcDest = _testEnv.CallGdi32Api("CREATECOMPATIBLEDC", 0u);
        var hdcSrc = _testEnv.CallGdi32Api("CREATECOMPATIBLEDC", 0u);

        // Test various raster operations
        uint[] rasterOps = {
            0x00CC0020, // SRCCOPY
            0x00EE0086, // SRCPAINT
            0x008800C6, // SRCAND
            0x00660046  // SRCINVERT
        };

        foreach (var rop in rasterOps)
        {
            // Act
            var result = _testEnv.CallGdi32Api("STRETCHBLT",
                hdcDest, 0, 0, 100, 100,
                hdcSrc, 0, 0, 50, 50,
                rop);

            // Assert
            Assert.Equal(1u, result); // TRUE for all valid raster ops
        }

        // Cleanup
        _testEnv.CallGdi32Api("DELETEDC", hdcDest);
        _testEnv.CallGdi32Api("DELETEDC", hdcSrc);
    }

    [Fact]
    public void StretchBlt_WithActualBitmaps_ShouldScaleAndCopy()
    {
        // Arrange - Create DCs
        var hdcDest = _testEnv.CallGdi32Api("CREATECOMPATIBLEDC", 0u);
        var hdcSrc = _testEnv.CallGdi32Api("CREATECOMPATIBLEDC", 0u);

        // Create source bitmap (10x10)
        var srcBitmap = _testEnv.CallGdi32Api("CREATECOMPATIBLEBITMAP", hdcSrc, 10, 10);
        
        // Create destination bitmap (20x20)
        var destBitmap = _testEnv.CallGdi32Api("CREATECOMPATIBLEBITMAP", hdcDest, 20, 20);

        // Select bitmaps into DCs
        _testEnv.CallGdi32Api("SELECTOBJECT", hdcSrc, srcBitmap);
        _testEnv.CallGdi32Api("SELECTOBJECT", hdcDest, destBitmap);

        // Act - Stretch from 10x10 to 20x20
        var result = _testEnv.CallGdi32Api("STRETCHBLT",
            hdcDest, 0, 0, 20, 20,  // Destination: (0, 0), size 20x20
            hdcSrc, 0, 0, 10, 10,   // Source: (0, 0), size 10x10
            0x00CC0020);            // SRCCOPY

        // Assert
        Assert.Equal(1u, result); // TRUE

        // Cleanup
        _testEnv.CallGdi32Api("DELETEOBJECT", srcBitmap);
        _testEnv.CallGdi32Api("DELETEOBJECT", destBitmap);
        _testEnv.CallGdi32Api("DELETEDC", hdcDest);
        _testEnv.CallGdi32Api("DELETEDC", hdcSrc);
    }

    [Fact]
    public void StretchBlt_WithScalingDown_ShouldWork()
    {
        // Arrange - Create DCs
        var hdcDest = _testEnv.CallGdi32Api("CREATECOMPATIBLEDC", 0u);
        var hdcSrc = _testEnv.CallGdi32Api("CREATECOMPATIBLEDC", 0u);

        // Create source bitmap (100x100)
        var srcBitmap = _testEnv.CallGdi32Api("CREATECOMPATIBLEBITMAP", hdcSrc, 100, 100);
        
        // Create destination bitmap (50x50)
        var destBitmap = _testEnv.CallGdi32Api("CREATECOMPATIBLEBITMAP", hdcDest, 50, 50);

        // Select bitmaps into DCs
        _testEnv.CallGdi32Api("SELECTOBJECT", hdcSrc, srcBitmap);
        _testEnv.CallGdi32Api("SELECTOBJECT", hdcDest, destBitmap);

        // Act - Scale down from 100x100 to 50x50
        var result = _testEnv.CallGdi32Api("STRETCHBLT",
            hdcDest, 0, 0, 50, 50,   // Destination: (0, 0), size 50x50
            hdcSrc, 0, 0, 100, 100,  // Source: (0, 0), size 100x100
            0x00CC0020);             // SRCCOPY

        // Assert
        Assert.Equal(1u, result); // TRUE

        // Cleanup
        _testEnv.CallGdi32Api("DELETEOBJECT", srcBitmap);
        _testEnv.CallGdi32Api("DELETEOBJECT", destBitmap);
        _testEnv.CallGdi32Api("DELETEDC", hdcDest);
        _testEnv.CallGdi32Api("DELETEDC", hdcSrc);
    }

    [Fact]
    public void StretchBlt_WithPartialSourceRect_ShouldWork()
    {
        // Arrange - Create DCs
        var hdcDest = _testEnv.CallGdi32Api("CREATECOMPATIBLEDC", 0u);
        var hdcSrc = _testEnv.CallGdi32Api("CREATECOMPATIBLEDC", 0u);

        // Create source bitmap (100x100)
        var srcBitmap = _testEnv.CallGdi32Api("CREATECOMPATIBLEBITMAP", hdcSrc, 100, 100);
        
        // Create destination bitmap (50x50)
        var destBitmap = _testEnv.CallGdi32Api("CREATECOMPATIBLEBITMAP", hdcDest, 50, 50);

        // Select bitmaps into DCs
        _testEnv.CallGdi32Api("SELECTOBJECT", hdcSrc, srcBitmap);
        _testEnv.CallGdi32Api("SELECTOBJECT", hdcDest, destBitmap);

        // Act - Copy partial source rect (25,25)-(75,75) to full dest (0,0)-(50,50)
        var result = _testEnv.CallGdi32Api("STRETCHBLT",
            hdcDest, 0, 0, 50, 50,   // Destination: (0, 0), size 50x50
            hdcSrc, 25, 25, 50, 50,  // Source: (25, 25), size 50x50
            0x00CC0020);             // SRCCOPY

        // Assert
        Assert.Equal(1u, result); // TRUE

        // Cleanup
        _testEnv.CallGdi32Api("DELETEOBJECT", srcBitmap);
        _testEnv.CallGdi32Api("DELETEOBJECT", destBitmap);
        _testEnv.CallGdi32Api("DELETEDC", hdcDest);
        _testEnv.CallGdi32Api("DELETEDC", hdcSrc);
    }

    [Fact]
    public void GetWinMetaFileBits_ShouldReturnZero_WhenNotSupported()
    {
        // Arrange
        uint hemf = 0x12345678;
        uint cbData16 = 256;
        var pData16 = _testEnv.AllocateMemory(cbData16);
        uint iMapMode = 1; // MM_TEXT
        uint hdcRef = 0;  // NULL reference DC

        // Act
        var result = _testEnv.CallGdi32Api("GETWINMETAFILEBITS", hemf, cbData16, pData16, iMapMode, hdcRef);

        // Assert
        Assert.Equal(0u, result); // Metafiles not supported, should return 0
    }

    [Fact]
    public void SetMetaFileBitsEx_ShouldReturnNull_WhenNotSupported()
    {
        // Arrange
        uint cbBuffer = 128;
        var lpData = _testEnv.AllocateMemory(cbBuffer);

        // Act
        var result = _testEnv.CallGdi32Api("SETMETAFILEBITSEX", cbBuffer, lpData);

        // Assert
        Assert.Equal(0u, result); // Metafiles not supported, should return NULL handle
    }

    [Fact]
    public void SetWinMetaFileBits_ShouldReturnNull_WhenNotSupported()
    {
        // Arrange
        uint nSize = 256;
        var lpMeta16Data = _testEnv.AllocateMemory(nSize);
        uint hdcRef = 0;
        uint lpMFP = 0;

        // Act
        var result = _testEnv.CallGdi32Api("SETWINMETAFILEBITS", nSize, lpMeta16Data, hdcRef, lpMFP);

        // Assert
        Assert.Equal(0u, result); // Metafiles not supported, should return NULL handle
    }

    [Fact]
    public void PlayEnhMetaFile_ShouldReturnFalse_WhenNotSupported()
    {
        // Arrange
        uint hdc = 0x81000000;
        uint hemf = 0x12345678;
        var lprect = _testEnv.AllocateMemory(16);
        _testEnv.Memory.Write32(lprect, 0);      // left
        _testEnv.Memory.Write32(lprect + 4, 0);  // top
        _testEnv.Memory.Write32(lprect + 8, 100); // right
        _testEnv.Memory.Write32(lprect + 12, 100); // bottom

        // Act
        var result = _testEnv.CallGdi32Api("PLAYENHMETAFILE", hdc, hemf, lprect);

        // Assert
        Assert.Equal(0u, result); // Metafiles not supported, should return FALSE
    }

    [Fact]
    public void TranslateCharsetInfo_ShouldReturnTrue_ForAnsiCharset()
    {
        // Arrange
        const uint TCI_SRCCHARSET = 1;
        var lpSrc = _testEnv.AllocateMemory(4);
        _testEnv.Memory.Write32(lpSrc, 0); // ANSI_CHARSET
        var lpCs = _testEnv.AllocateMemory(32); // CHARSETINFO structure

        // Act
        var result = _testEnv.CallGdi32Api("TRANSLATECHARSETINFO", lpSrc, lpCs, TCI_SRCCHARSET);

        // Assert
        Assert.Equal(1u, result); // TRUE
        var charset = _testEnv.Memory.Read32(lpCs);
        var codepage = _testEnv.Memory.Read32(lpCs + 4);
        Assert.Equal(0u, charset);   // ANSI_CHARSET
        Assert.Equal(1252u, codepage); // ANSI codepage
    }

    [Fact]
    public void TranslateCharsetInfo_ShouldReturnTrue_ForShiftJis()
    {
        // Arrange
        const uint TCI_SRCCODEPAGE = 2;
        var lpSrc = _testEnv.AllocateMemory(4);
        _testEnv.Memory.Write32(lpSrc, 932); // Shift-JIS codepage
        var lpCs = _testEnv.AllocateMemory(32); // CHARSETINFO structure

        // Act
        var result = _testEnv.CallGdi32Api("TRANSLATECHARSETINFO", lpSrc, lpCs, TCI_SRCCODEPAGE);

        // Assert
        Assert.Equal(1u, result); // TRUE
        var charset = _testEnv.Memory.Read32(lpCs);
        var codepage = _testEnv.Memory.Read32(lpCs + 4);
        Assert.Equal(128u, charset); // SHIFTJIS_CHARSET
        Assert.Equal(932u, codepage);
    }

    [Fact]
    public void TranslateCharsetInfo_ShouldReturnFalse_WhenNullPointer()
    {
        // Arrange
        const uint TCI_SRCCHARSET = 1;
        var lpSrc = _testEnv.AllocateMemory(4);
        _testEnv.Memory.Write32(lpSrc, 0);

        // Act
        var result = _testEnv.CallGdi32Api("TRANSLATECHARSETINFO", lpSrc, 0u, TCI_SRCCHARSET);

        // Assert
        Assert.Equal(0u, result); // FALSE - null lpCs pointer
    }

    public void Dispose()
    {
        _testEnv?.Dispose();
    }
}
