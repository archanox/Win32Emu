using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules
{
	public class Gdi32Module : IWin32ModuleUnsafe
	{
		private readonly ProcessEnvironment _env;
		private readonly uint _imageBase;
		private readonly PeImageLoader? _peLoader;
		private readonly ILogger _logger;

		public Gdi32Module(ProcessEnvironment env, uint imageBase, PeImageLoader? peLoader = null, ILogger? logger = null)
		{
			_env = env;
			_imageBase = imageBase;
			_peLoader = peLoader;
			_logger = logger ?? NullLogger.Instance;
		}

		public string Name => "GDI32.DLL";

		// Stock object handles - these are pseudo-handles that don't require cleanup
		private readonly Dictionary<int, uint> _stockObjects = new();
		private uint _nextStockObjectHandle = 0x80000000; // Start with high address to distinguish from regular handles

		// Device contexts
		private readonly Dictionary<uint, DeviceContext> _deviceContexts = new();
		private uint _nextDcHandle = 0x81000000;

		public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			returnValue = 0;
			var a = new StackArgs(cpu, memory);

			switch (export.ToUpperInvariant())
			{
				case "GETSTOCKOBJECT":
					returnValue = GetStockObject(a.Int32(0));
					return true;

				case "BEGINPAINT":
					returnValue = BeginPaint(a.UInt32(0), a.UInt32(1));
					return true;

				case "ENDPAINT":
					returnValue = EndPaint(a.UInt32(0), a.UInt32(1));
					return true;

				case "FILLRECT":
					returnValue = FillRect(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				case "TEXTOUT":
				case "TEXTOUTA":
					returnValue = TextOutA(a.UInt32(0), a.Int32(1), a.Int32(2), a.UInt32(3), a.Int32(4));
					return true;

				case "SETBKMODE":
					returnValue = SetBkMode(a.UInt32(0), a.Int32(1));
					return true;

				case "SETTEXTCOLOR":
					returnValue = SetTextColor(a.UInt32(0), a.UInt32(1));
					return true;

				case "GETDEVICECAPS":
					returnValue = (uint)GetDeviceCaps(a.UInt32(0), a.Int32(1));
					return true;

				default:
					_logger.LogInformation($"[Gdi32] Unimplemented export: {export}");
					return false;
			}
		}

		private unsafe uint GetStockObject(int stockObjectId)
		{
			// Validate stock object ID
			if (stockObjectId < NativeTypes.StockObject.WHITE_BRUSH ||
			    stockObjectId > NativeTypes.StockObject.DC_PEN)
			{
				_logger.LogInformation($"[Gdi32] GetStockObject: Invalid stock object ID {stockObjectId}");
				return 0;
			}

			// Return cached handle or create a new one
			if (_stockObjects.TryGetValue(stockObjectId, out var handle))
			{
				return handle;
			}

			// Create a pseudo-handle for this stock object
			handle = _nextStockObjectHandle++;
			_stockObjects[stockObjectId] = handle;

			_logger.LogInformation($"[Gdi32] GetStockObject({stockObjectId}) -> 0x{handle:X8}");
			return handle;
		}

		private unsafe uint BeginPaint(uint hwnd, uint lpPaint)
		{
			_logger.LogInformation($"[Gdi32] BeginPaint(HWND=0x{hwnd:X8}, lpPaint=0x{lpPaint:X8})");

			// Create a device context for this paint session
			var hdc = _nextDcHandle++;
			var dc = new DeviceContext
			{
				Handle = hdc,
				WindowHandle = hwnd
			};
			_deviceContexts[hdc] = dc;

			// Fill PAINTSTRUCT if provided
			if (lpPaint != 0)
			{
				// PAINTSTRUCT layout:
				// HDC hdc
				// BOOL fErase
				// RECT rcPaint
				// BOOL fRestore
				// BOOL fIncUpdate
				// BYTE rgbReserved[32]
				_env.MemWrite32(lpPaint, hdc); // hdc
				_env.MemWrite32(lpPaint + 4, 1); // fErase = TRUE
				_env.MemWrite32(lpPaint + 8, 0); // rcPaint.left
				_env.MemWrite32(lpPaint + 12, 0); // rcPaint.top
				_env.MemWrite32(lpPaint + 16, 640); // rcPaint.right
				_env.MemWrite32(lpPaint + 20, 480); // rcPaint.bottom
			}

			return hdc;
		}

		private unsafe uint EndPaint(uint hwnd, uint lpPaint)
		{
			if (lpPaint != 0)
			{
				var hdc = _env.MemRead32(lpPaint);
				_logger.LogInformation($"[Gdi32] EndPaint(HWND=0x{hwnd:X8}, HDC=0x{hdc:X8})");

				// Remove the device context
				_deviceContexts.Remove(hdc);
			}

			return 1; // TRUE
		}

		private unsafe uint FillRect(uint hdc, uint lpRect, uint hBrush)
		{
			if (lpRect != 0)
			{
				var left = _env.MemRead32(lpRect);
				var top = _env.MemRead32(lpRect + 4);
				var right = _env.MemRead32(lpRect + 8);
				var bottom = _env.MemRead32(lpRect + 12);
				_logger.LogInformation($"[Gdi32] FillRect(HDC=0x{hdc:X8}, rect=({left},{top},{right},{bottom}), hBrush=0x{hBrush:X8})");
			}

			return 1; // Non-zero on success
		}

		private unsafe uint TextOutA(uint hdc, int x, int y, uint lpString, int cbString)
		{
			if (lpString != 0 && cbString > 0)
			{
				var text = _env.ReadAnsiString(lpString, cbString);
				_logger.LogInformation($"[Gdi32] TextOutA(HDC=0x{hdc:X8}, x={x}, y={y}, text=\"{text}\")");
			}

			return 1; // TRUE
		}

		private unsafe uint SetBkMode(uint hdc, int mode)
		{
			_logger.LogInformation($"[Gdi32] SetBkMode(HDC=0x{hdc:X8}, mode={mode})");
			if (_deviceContexts.TryGetValue(hdc, out var dc))
			{
				var previous = dc.BkMode;
				dc.BkMode = mode;
				return (uint)previous;
			}

			return 0; // Default: TRANSPARENT
		}

		private unsafe uint SetTextColor(uint hdc, uint color)
		{
			_logger.LogInformation($"[Gdi32] SetTextColor(HDC=0x{hdc:X8}, color=0x{color:X8})");
			return 0x00000000; // Previous color (black)
		}

		private unsafe int GetDeviceCaps(uint hdc, int nIndex)
		{
			_logger.LogInformation($"[Gdi32] GetDeviceCaps(HDC=0x{hdc:X8}, nIndex={nIndex})");

			// Return common device capabilities
			return nIndex switch
			{
				8 => 1920, // HORZRES - Horizontal resolution in pixels
				10 => 1080, // VERTRES - Vertical resolution in pixels
				12 => 32, // BITSPIXEL - Color bits per pixel
				88 => 96, // LOGPIXELSX - Logical pixels/inch in X
				90 => 96, // LOGPIXELSY - Logical pixels/inch in Y
				2 => 8, // TECHNOLOGY - DT_RASDISPLAY (raster display)
				_ => 0
			};
		}

		private class DeviceContext
		{
			public uint Handle { get; set; }
			public uint WindowHandle { get; set; }
			public int BkMode { get; set; } = 2; // OPAQUE
			public uint TextColor { get; set; } = 0x00000000; // Black
		}

		public Dictionary<string, uint> GetExportOrdinals()
		{
			// Export ordinals for Gdi32 - alphabetically ordered
			var exports = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)
			{
				{ "BEGINPAINT", 1 },
				{ "ENDPAINT", 2 },
				{ "FILLRECT", 3 },
				{ "GETDEVICECAPS", 4 },
				{ "GETSTOCKOBJECT", 5 },
				{ "SETBKMODE", 6 },
				{ "SETTEXTCOLOR", 7 },
				{ "TEXTOUT", 8 },
				{ "TEXTOUTA", 9 }
			};
			return exports;
		}
	}
}