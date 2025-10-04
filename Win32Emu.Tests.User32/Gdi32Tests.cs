using Win32Emu.Tests.User32.TestInfrastructure;
using Win32Emu.Win32;

namespace Win32Emu.Tests.User32
{
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

		public void Dispose()
		{
			_testEnv?.Dispose();
		}
	}
}
