using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Loader;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Modules
{
	internal class Gdi32Module : IWin32ModuleUnsafe
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
		private uint _nextGdiObjectHandle = 0x82000000;
		private readonly Dictionary<uint, GdiObject> _gdiObjects = new();

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
				case "RECTANGLE":
					returnValue = Rectangle(a.UInt32(0), a.Int32(1), a.Int32(2), a.Int32(3), a.Int32(4));
					return true;
				case "TEXTOUT":
					returnValue = TextOut(a.UInt32(0), a.Int32(1), a.Int32(2), a.UInt32(3), a.Int32(4));
					return true;
				case "TEXTOUTA":
					returnValue = TextOutA(a.UInt32(0), a.Int32(1), a.Int32(2), a.UInt32(3), a.Int32(4));
					return true;
				case "SETBKMODE":
					returnValue = SetBkMode(a.UInt32(0), a.Int32(1));
					return true;
				case "SETTEXTCOLOR":
					returnValue = SetTextColor(a.UInt32(0), a.UInt32(1));
					return true;
				case "SETBKCOLOR":
					returnValue = SetBkColor(a.UInt32(0), a.UInt32(1));
					return true;
				case "SETTEXTALIGN":
					returnValue = SetTextAlign(a.UInt32(0), a.UInt32(1));
					return true;
				case "GETDEVICECAPS":
					returnValue = (uint)GetDeviceCaps(a.UInt32(0), a.Int32(1));
					return true;
				case "DELETEOBJECT":
					returnValue = DeleteObject(a.UInt32(0));
					return true;

				// Bitmap functions
				case "BITBLT":
					returnValue = BitBlt(a.UInt32(0), a.Int32(1), a.Int32(2), a.Int32(3), a.Int32(4), a.UInt32(5), a.Int32(6), a.Int32(7), a.UInt32(8));
					return true;
				case "CREATEBITMAP":
					returnValue = CreateBitmap(a.Int32(0), a.Int32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;
				case "CREATEDIBITMAP":
					returnValue = CreateDIBitmap(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5));
					return true;
				case "CREATECOMPATIBLEBITMAP":
					returnValue = CreateCompatibleBitmap(a.UInt32(0), a.Int32(1), a.Int32(2));
					return true;
				case "GETDIBITS":
					returnValue = GetDIBits(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5), a.UInt32(6));
					return true;
				case "SETDIBITSTODEVICE":
					returnValue = (uint)SetDIBitsToDevice(a.UInt32(0), a.Int32(1), a.Int32(2), a.UInt32(3), a.UInt32(4), a.Int32(5), a.Int32(6), a.UInt32(7), a.UInt32(8), a.UInt32(9), a.UInt32(10), a.UInt32(11));
					return true;

				// DC functions
				case "CREATECOMPATIBLEDC":
					returnValue = CreateCompatibleDC(a.UInt32(0));
					return true;
				case "CREATEDCA":
					returnValue = CreateDCA(a.LpcStr(0), a.LpcStr(1), a.LpcStr(2), a.UInt32(3));
					return true;
				case "DELETEDC":
					returnValue = DeleteDC(a.UInt32(0));
					return true;
				case "SAVEDC":
					returnValue = (uint)SaveDC(a.UInt32(0));
					return true;
				case "RESTOREDC":
					returnValue = RestoreDC(a.UInt32(0), a.Int32(1));
					return true;
				case "SELECTOBJECT":
					returnValue = SelectObject(a.UInt32(0), a.UInt32(1));
					return true;
				case "GETCURRENTOBJECT":
					returnValue = GetCurrentObject(a.UInt32(0), a.UInt32(1));
					return true;
				case "GETOBJECTA":
					returnValue = (uint)GetObjectA(a.UInt32(0), a.Int32(1), a.UInt32(2));
					return true;

				// Drawing functions
				case "LINETO":
					returnValue = LineTo(a.UInt32(0), a.Int32(1), a.Int32(2));
					return true;
				case "MOVETOEX":
					returnValue = MoveToEx(a.UInt32(0), a.Int32(1), a.Int32(2), a.UInt32(3));
					return true;
				case "SETPIXEL":
					returnValue = SetPixel(a.UInt32(0), a.Int32(1), a.Int32(2), a.UInt32(3));
					return true;
				case "GETPIXEL":
					returnValue = GetPixel(a.UInt32(0), a.Int32(1), a.Int32(2));
					return true;

				// Font and text functions
				case "CREATEFONTA":
					returnValue = CreateFontA(a.Int32(0), a.Int32(1), a.Int32(2), a.Int32(3), a.Int32(4), a.UInt32(5), a.UInt32(6), a.UInt32(7), a.UInt32(8), a.UInt32(9), a.UInt32(10), a.UInt32(11), a.UInt32(12), a.LpcStr(13));
					return true;
				case "CREATEFONTINDIRECTA":
					returnValue = CreateFontIndirectA(a.UInt32(0));
					return true;
				case "GETTEXTEXTENTPOINT32A":
					returnValue = GetTextExtentPoint32A(a.UInt32(0), a.LpcStr(1), a.Int32(2), a.UInt32(3));
					return true;
				case "EXTTEXTOUTA":
					returnValue = ExtTextOutA(a.UInt32(0), a.Int32(1), a.Int32(2), a.UInt32(3), a.UInt32(4), a.LpcStr(5), a.UInt32(6), a.UInt32(7));
					return true;

				// Pen and brush functions
				case "CREATEPEN":
					returnValue = CreatePen(a.Int32(0), a.Int32(1), a.UInt32(2));
					return true;
				case "CREATESOLIDBRUSH":
					returnValue = CreateSolidBrush(a.UInt32(0));
					return true;

				// Palette functions
				case "CREATEPALETTE":
					returnValue = CreatePalette(a.UInt32(0));
					return true;
				case "SELECTPALETTE":
					returnValue = SelectPalette(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "REALIZEPALETTE":
					returnValue = RealizePalette(a.UInt32(0));
					return true;
				case "GETSYSTEMPALETTEENTRIES":
					returnValue = GetSystemPaletteEntries(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;

				// Viewport and mapping functions
				case "SETMAPMODE":
					returnValue = (uint)SetMapMode(a.UInt32(0), a.Int32(1));
					return true;
				case "SETVIEWPORTEXTEX":
					returnValue = SetViewportExtEx(a.UInt32(0), a.Int32(1), a.Int32(2), a.UInt32(3));
					return true;
				case "SETVIEWPORTORGEX":
					returnValue = SetViewportOrgEx(a.UInt32(0), a.Int32(1), a.Int32(2), a.UInt32(3));
					return true;
				case "SETWINDOWEXTEX":
					returnValue = SetWindowExtEx(a.UInt32(0), a.Int32(1), a.Int32(2), a.UInt32(3));
					return true;
				case "OFFSETVIEWPORTORGEX":
					returnValue = OffsetViewportOrgEx(a.UInt32(0), a.Int32(1), a.Int32(2), a.UInt32(3));
					return true;
				case "SCALEVIEWPORTEXTEX":
					returnValue = ScaleViewportExtEx(a.UInt32(0), a.Int32(1), a.Int32(2), a.Int32(3), a.Int32(4), a.UInt32(5));
					return true;
				case "SCALEWINDOWEXTEX":
					returnValue = ScaleWindowExtEx(a.UInt32(0), a.Int32(1), a.Int32(2), a.Int32(3), a.Int32(4), a.UInt32(5));
					return true;

				// Clipping functions
				case "GETCLIPBOX":
					returnValue = (uint)GetClipBox(a.UInt32(0), a.UInt32(1));
					return true;
				case "INTERSECTCLIPRECT":
					returnValue = (uint)IntersectClipRect(a.UInt32(0), a.Int32(1), a.Int32(2), a.Int32(3), a.Int32(4));
					return true;
				case "PTVISIBLE":
					returnValue = PtVisible(a.UInt32(0), a.Int32(1), a.Int32(2));
					return true;
				case "RECTVISIBLE":
					returnValue = RectVisible(a.UInt32(0), a.UInt32(1));
					return true;

				// Additional bitmap functions
				case "GETBITMAPBITS":
					returnValue = (uint)GetBitmapBits(a.UInt32(0), a.Int32(1), a.UInt32(2));
					return true;
				case "SETBITMAPBITS":
					returnValue = (uint)SetBitmapBits(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "CREATEDIBSECTION":
					returnValue = CreateDIBSection(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4), a.UInt32(5));
					return true;
				case "SETDIBCOLORTABLE":
					returnValue = SetDIBColorTable(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;

				// Palette functions
				case "ANIMATEPALETTE":
					returnValue = AnimatePalette(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;
				case "CREATEHALFTONEPALETTE":
					returnValue = CreateHalftonePalette(a.UInt32(0));
					return true;
				case "GETNEARESTPALETTEINDEX":
					returnValue = GetNearestPaletteIndex(a.UInt32(0), a.UInt32(1));
					return true;
				case "GETPALETTEENTRIES":
					returnValue = GetPaletteEntries(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;
				case "SETPALETTEENTRIES":
					returnValue = SetPaletteEntries(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;
				case "UNREALIZEOBJECT":
					returnValue = UnrealizeObject(a.UInt32(0));
					return true;

				// Text functions
				case "GETTEXTMETRICSA":
					returnValue = GetTextMetricsA(a.UInt32(0), a.UInt32(1));
					return true;
				case "GETTEXTEXTENTPOINTA":
					returnValue = GetTextExtentPointA(a.UInt32(0), a.LpcStr(1), a.Int32(2), a.UInt32(3));
					return true;
				case "GETTEXTCOLOR":
					returnValue = GetTextColor(a.UInt32(0));
					return true;
				case "GETBKCOLOR":
					returnValue = GetBkColor(a.UInt32(0));
					return true;

				// Coordinate transformation functions
				case "DPTOLP":
					returnValue = DPtoLP(a.UInt32(0), a.UInt32(1), a.Int32(2));
					return true;
				case "LPTODP":
					returnValue = LPtoDP(a.UInt32(0), a.UInt32(1), a.Int32(2));
					return true;
				case "GETVIEWPORTEXTEX":
					returnValue = GetViewportExtEx(a.UInt32(0), a.UInt32(1));
					return true;
				case "GETWINDOWEXTEX":
					returnValue = GetWindowExtEx(a.UInt32(0), a.UInt32(1));
					return true;
				case "GETMAPMODE":
					returnValue = (uint)GetMapMode(a.UInt32(0));
					return true;

				// Advanced drawing functions
				case "PATBLT":
					returnValue = PatBlt(a.UInt32(0), a.Int32(1), a.Int32(2), a.Int32(3), a.Int32(4), a.UInt32(5));
					return true;
				case "CHORD":
					returnValue = Chord(a.UInt32(0), a.Int32(1), a.Int32(2), a.Int32(3), a.Int32(4), a.Int32(5), a.Int32(6), a.Int32(7), a.Int32(8));
					return true;
				case "PIE":
					returnValue = Pie(a.UInt32(0), a.Int32(1), a.Int32(2), a.Int32(3), a.Int32(4), a.Int32(5), a.Int32(6), a.Int32(7), a.Int32(8));
					return true;
				case "POLYGON":
					returnValue = Polygon(a.UInt32(0), a.UInt32(1), a.Int32(2));
					return true;
				case "ROUNDRECT":
					returnValue = RoundRect(a.UInt32(0), a.Int32(1), a.Int32(2), a.Int32(3), a.Int32(4), a.Int32(5), a.Int32(6));
					return true;

				// Escape function
				case "ESCAPE":
					returnValue = (uint)Escape(a.UInt32(0), a.Int32(1), a.Int32(2), a.UInt32(3), a.UInt32(4));
					return true;

				// Printing functions
				case "STARTDOCA":
					returnValue = (uint)StartDocA(a.UInt32(0), a.UInt32(1));
					return true;
				case "ENDDOC":
					returnValue = (uint)EndDoc(a.UInt32(0));
					return true;
				case "STARTPAGE":
					returnValue = (uint)StartPage(a.UInt32(0));
					return true;
				case "ENDPAGE":
					returnValue = (uint)EndPage(a.UInt32(0));
					return true;

				// GDI utility functions
				case "GDIFLUSH":
					returnValue = GdiFlush();
					return true;
				case "GETSYSTEMPALETTEUSE":
					returnValue = GetSystemPaletteUse(a.UInt32(0));
					return true;

				// Region functions
				case "CREATERECTRGN":
					returnValue = CreateRectRgn(a.Int32(0), a.Int32(1), a.Int32(2), a.Int32(3));
					return true;
				case "GETREGIONDATA":
					returnValue = GetRegionData(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				// Additional drawing functions
				case "ELLIPSE":
					returnValue = Ellipse(a.UInt32(0), a.Int32(1), a.Int32(2), a.Int32(3), a.Int32(4));
					return true;
				case "ARC":
					returnValue = Arc(a.UInt32(0), a.Int32(1), a.Int32(2), a.Int32(3), a.Int32(4), a.Int32(5), a.Int32(6), a.Int32(7), a.Int32(8));
					return true;
				case "POLYLINE":
					returnValue = Polyline(a.UInt32(0), a.UInt32(1), a.Int32(2));
					return true;
				case "DRAWTEXT":
					returnValue = (uint)DrawText(a.UInt32(0), a.UInt32(1), a.Int32(2), a.UInt32(3), a.UInt32(4));
					return true;
				case "DRAWTEXTA":
					returnValue = (uint)DrawTextA(a.UInt32(0), a.LpcStr(1), a.Int32(2), a.UInt32(3), a.UInt32(4));
					return true;
				case "FRAMERECT":
					returnValue = FrameRect(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "INVERTRECT":
					returnValue = InvertRect(a.UInt32(0), a.UInt32(1));
					return true;

				default:
					_logger.LogInformation("[Gdi32] Unimplemented export: {Export}", export);
					return false;
			}
		}

		[DllModuleExport(1)]
		private uint GetStockObject(int stockObjectId)
		{
			// Validate stock object ID
			if (stockObjectId is < NativeTypes.StockObject.WHITE_BRUSH or > NativeTypes.StockObject.DC_PEN)
			{
				_logger.LogInformation("[Gdi32] GetStockObject: Invalid stock object ID {StockObjectId}", stockObjectId);
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

			_logger.LogInformation("[Gdi32] GetStockObject({StockObjectId}) -> 0x{Handle:X8}", stockObjectId, handle);
			return handle;
		}

		[DllModuleExport(1)]
		private uint BeginPaint(uint hwnd, uint lpPaint)
		{
			_logger.LogInformation("[Gdi32] BeginPaint(HWND=0x{Hwnd:X8}, lpPaint=0x{LpPaint:X8})", hwnd, lpPaint);

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

		[DllModuleExport(1)]
		private uint EndPaint(uint hwnd, uint lpPaint)
		{
			if (lpPaint != 0)
			{
				var hdc = _env.MemRead32(lpPaint);
				_logger.LogInformation("[Gdi32] EndPaint(HWND=0x{Hwnd:X8}, HDC=0x{Hdc:X8})", hwnd, hdc);

				// Remove the device context
				_deviceContexts.Remove(hdc);
			}

			return 1; // TRUE
		}

		[DllModuleExport(1)]
		private uint FillRect(uint hdc, uint lpRect, uint hBrush)
		{
			if (lpRect != 0)
			{
				var left = _env.MemRead32(lpRect);
				var top = _env.MemRead32(lpRect + 4);
				var right = _env.MemRead32(lpRect + 8);
				var bottom = _env.MemRead32(lpRect + 12);
				_logger.LogInformation("[Gdi32] FillRect(HDC=0x{Hdc:X8}, rect=({Left},{Top},{Right},{Bottom}), hBrush=0x{HBrush:X8})", hdc, left, top, right, bottom, hBrush);
			}

			return 1; // Non-zero on success
		}

		[DllModuleExport(20)]
		private uint Rectangle(uint hdc, int left, int top, int right, int bottom)
		{
			_logger.LogInformation("[Gdi32] Rectangle(HDC=0x{Hdc:X8}, left={Left}, top={Top}, right={Right}, bottom={Bottom})",
				hdc, left, top, right, bottom);

			// Rectangle draws a rectangle outline using the current pen
			// For stub implementation, we just log and return success
			return 1; // Non-zero on success
		}

		[DllModuleExport(1, IsStub = true)]
		private uint TextOut(uint hdc, int x, int y, uint lpString, int cbString)
		{
			return 0;
		}

		[DllModuleExport(1)]
		private uint TextOutA(uint hdc, int x, int y, uint lpString, int cbString)
		{
			if (lpString != 0 && cbString > 0)
			{
				var text = _env.ReadAnsiString(lpString, cbString);
				_logger.LogInformation("[Gdi32] TextOutA(HDC=0x{Hdc:X8}, x={I}, y={I1}, text=\"{Text}\")", hdc, x, y, text);
			}

			return 1; // TRUE
		}

		[DllModuleExport(1)]
		private uint SetBkMode(uint hdc, int mode)
		{
			_logger.LogInformation("[Gdi32] SetBkMode(HDC=0x{Hdc:X8}, mode={Mode})", hdc, mode);
			if (_deviceContexts.TryGetValue(hdc, out var dc))
			{
				var previous = dc.BkMode;
				dc.BkMode = mode;
				return (uint)previous;
			}

			return 0; // Default: TRANSPARENT
		}

		[DllModuleExport(1)]
		private uint SetTextColor(uint hdc, uint color)
		{
			_logger.LogInformation("[Gdi32] SetTextColor(HDC=0x{Hdc:X8}, color=0x{Color:X8})", hdc, color);
			return 0x00000000; // Previous color (black)
		}

		[DllModuleExport(8)]
		private uint SetTextAlign(uint hdc, uint align)
		{
			_logger.LogInformation("[Gdi32] SetTextAlign(HDC=0x{Hdc:X8}, align=0x{Align:X8})", hdc, align);
			// SetTextAlign sets the text alignment flags
			// Common flags:
			// TA_LEFT = 0, TA_RIGHT = 2, TA_CENTER = 6
			// TA_TOP = 0, TA_BOTTOM = 8, TA_BASELINE = 24
			// TA_NOUPDATECP = 0, TA_UPDATECP = 1
			// Return previous alignment (default: TA_LEFT | TA_TOP | TA_NOUPDATECP = 0)
			return 0; // Previous alignment
		}

		[DllModuleExport(1)]
		private int GetDeviceCaps(uint hdc, int nIndex)
		{
			_logger.LogInformation("[Gdi32] GetDeviceCaps(HDC=0x{Hdc:X8}, nIndex={NIndex})", hdc, nIndex);

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

		[DllModuleExport(8)]
		private uint SetBkColor(uint hdc, uint color)
		{
			_logger.LogInformation("[Gdi32] SetBkColor(HDC=0x{Hdc:X8}, color=0x{Color:X8})", hdc, color);
			return 0x00FFFFFF; // Previous color (white)
		}

		[DllModuleExport(4)]
		private uint DeleteObject(uint hObject)
		{
			_logger.LogInformation("[Gdi32] DeleteObject(hObject=0x{HObject:X8})", hObject);

			// Stock objects should not be deleted
			if (hObject >= 0x80000000 && hObject < 0x81000000)
			{
				_logger.LogInformation("[Gdi32] DeleteObject: Cannot delete stock object");
				return 0; // FALSE
			}

			// Remove device context if it exists
			if (_deviceContexts.Remove(hObject))
			{
				_logger.LogInformation("[Gdi32] DeleteObject: Deleted device context");
				return 1; // TRUE
			}

			// Remove GDI object if it exists
			if (_gdiObjects.Remove(hObject))
			{
				_logger.LogInformation("[Gdi32] DeleteObject: Deleted GDI object");
				return 1; // TRUE
			}

			// For other objects, just acknowledge the deletion
			_logger.LogInformation("[Gdi32] DeleteObject: Object deleted (stub)");
			return 1; // TRUE
		}

		// Bitmap functions
		[DllModuleExport(36)]
		private uint BitBlt(uint hdcDest, int x, int y, int cx, int cy, uint hdcSrc, int x1, int y1, uint rop)
		{
			_logger.LogInformation("[Gdi32] BitBlt(hdcDest=0x{HdcDest:X8}, dest=({X},{Y}), size=({Cx},{Cy}), hdcSrc=0x{HdcSrc:X8}, src=({X1},{Y1}), rop=0x{Rop:X})",
				hdcDest, x, y, cx, cy, hdcSrc, x1, y1, rop);
			return 1; // TRUE
		}

		[DllModuleExport(20)]
		private uint CreateBitmap(int nWidth, int nHeight, uint nPlanes, uint nBitCount, uint lpBits)
		{
			_logger.LogInformation("[Gdi32] CreateBitmap(width={NWidth}, height={NHeight}, planes={NPlanes}, bitCount={NBitCount}, lpBits=0x{LpBits:X8})",
				nWidth, nHeight, nPlanes, nBitCount, lpBits);
			var handle = _nextGdiObjectHandle++;
			_gdiObjects[handle] = new GdiObject { Type = GdiObjectType.Bitmap };
			return handle;
		}

		[DllModuleExport(12)]
		private uint CreateCompatibleBitmap(uint hdc, int cx, int cy)
		{
			_logger.LogInformation("[Gdi32] CreateCompatibleBitmap(hdc=0x{Hdc:X8}, cx={Cx}, cy={Cy})", hdc, cx, cy);
			var handle = _nextGdiObjectHandle++;
			_gdiObjects[handle] = new GdiObject { Type = GdiObjectType.Bitmap };
			return handle;
		}

		[DllModuleExport(28)]
		private uint GetDIBits(uint hdc, uint hbm, uint start, uint cLines, uint lpvBits, uint lpbmi, uint usage)
		{
			_logger.LogInformation("[Gdi32] GetDIBits(stub)");
			return 0; // 0 indicates error
		}

		[DllModuleExport(48)]
		private int SetDIBitsToDevice(uint hdc, int xDest, int yDest, uint width, uint height, int xSrc, int ySrc, uint startScan, uint scanLines, uint lpvBits, uint lpbmi, uint colorUse)
		{
			_logger.LogInformation("[Gdi32] SetDIBitsToDevice(hdc=0x{Hdc:X8}, dest=({XDest},{YDest}), size=({Width}x{Height}), src=({XSrc},{YSrc}), startScan={StartScan}, scanLines={ScanLines}, lpvBits=0x{LpvBits:X8}, lpbmi=0x{Lpbmi:X8}, colorUse={ColorUse})",
				hdc, xDest, yDest, width, height, xSrc, ySrc, startScan, scanLines, lpvBits, lpbmi, colorUse);
			// Return the number of scan lines copied (stub implementation)
			return (int)scanLines;
		}

		// DC functions
		[DllModuleExport(4)]
		private uint CreateCompatibleDC(uint hdc)
		{
			_logger.LogInformation("[Gdi32] CreateCompatibleDC(hdc=0x{Hdc:X8})", hdc);
			var handle = _nextDcHandle++;
			_deviceContexts[handle] = new DeviceContext { Handle = handle };
			return handle;
		}

		[DllModuleExport(1)]
		private uint CreateDCA(in LpcStr lpszDriver, in LpcStr lpszDevice, in LpcStr lpszOutput, uint lpInitData)
		{
			var driver = lpszDriver.ToString() ?? string.Empty;
			var device = lpszDevice.ToString() ?? string.Empty;
			var output = lpszOutput.ToString() ?? string.Empty;

			_logger.LogInformation("[Gdi32] CreateDCA(lpszDriver=\"{Driver}\", lpszDevice=\"{Device}\", lpszOutput=\"{Output}\")",
				driver, device, output);

			// Create a device context handle
			var handle = _nextDcHandle++;
			_deviceContexts[handle] = new DeviceContext { Handle = handle };
			return handle;
		}

		[DllModuleExport(4)]
		private uint DeleteDC(uint hdc)
		{
			_logger.LogInformation("[Gdi32] DeleteDC(hdc=0x{Hdc:X8})", hdc);
			return _deviceContexts.Remove(hdc) ? 1u : 0u;
		}

		[DllModuleExport(4)]
		private int SaveDC(uint hdc)
		{
			_logger.LogInformation("[Gdi32] SaveDC(hdc=0x{Hdc:X8})", hdc);
			return 1; // Return saved DC identifier
		}

		[DllModuleExport(8)]
		private uint RestoreDC(uint hdc, int nSavedDC)
		{
			_logger.LogInformation("[Gdi32] RestoreDC(hdc=0x{Hdc:X8}, nSavedDC={NSavedDC})", hdc, nSavedDC);
			return 1; // TRUE
		}

		[DllModuleExport(8)]
		private uint SelectObject(uint hdc, uint hObject)
		{
			_logger.LogInformation("[Gdi32] SelectObject(hdc=0x{Hdc:X8}, hObject=0x{HObject:X8})", hdc, hObject);
			return hObject; // Return previous object (stub)
		}

		[DllModuleExport(8)]
		private uint GetCurrentObject(uint hdc, uint type)
		{
			_logger.LogInformation("[Gdi32] GetCurrentObject(hdc=0x{Hdc:X8}, type={Type})", hdc, type);
			return 0; // Return NULL (no current object)
		}

		[DllModuleExport(12)]
		private int GetObjectA(uint hObject, int c, uint pv)
		{
			_logger.LogInformation("[Gdi32] GetObjectA(hObject=0x{HObject:X8}, c={C}, pv=0x{Pv:X8})", hObject, c, pv);
			return 0; // Return 0 (no data copied)
		}

		// Drawing functions
		[DllModuleExport(12)]
		private uint LineTo(uint hdc, int x, int y)
		{
			_logger.LogInformation("[Gdi32] LineTo(hdc=0x{Hdc:X8}, x={X}, y={Y})", hdc, x, y);
			return 1; // TRUE
		}

		[DllModuleExport(16)]
		private uint MoveToEx(uint hdc, int x, int y, uint lppt)
		{
			_logger.LogInformation("[Gdi32] MoveToEx(hdc=0x{Hdc:X8}, x={X}, y={Y}, lppt=0x{Lppt:X8})", hdc, x, y, lppt);
			return 1; // TRUE
		}

		[DllModuleExport(16)]
		private uint SetPixel(uint hdc, int x, int y, uint color)
		{
			_logger.LogInformation("[Gdi32] SetPixel(hdc=0x{Hdc:X8}, x={X}, y={Y}, color=0x{Color:X8})", hdc, x, y, color);
			return color; // Return the color set
		}

		[DllModuleExport(12)]
		private uint GetPixel(uint hdc, int x, int y)
		{
			_logger.LogInformation("[Gdi32] GetPixel(hdc=0x{Hdc:X8}, x={X}, y={Y})", hdc, x, y);
			return 0x00000000; // Return black
		}

		// Font and text functions
		[DllModuleExport(56)]
		private uint CreateFontA(int cHeight, int cWidth, int cEscapement, int cOrientation, int cWeight, uint bItalic, uint bUnderline, uint bStrikeOut, uint iCharSet, uint iOutPrecision, uint iClipPrecision, uint iQuality, uint iPitchAndFamily, in LpcStr pszFaceName)
		{
			var faceName = pszFaceName.ToString() ?? "Arial";
			_logger.LogInformation("[Gdi32] CreateFontA(height={CHeight}, weight={CWeight}, faceName=\"{FaceName}\")", cHeight, cWeight, faceName);
			var handle = _nextGdiObjectHandle++;
			_gdiObjects[handle] = new GdiObject { Type = GdiObjectType.Font };
			return handle;
		}

		[DllModuleExport(4)]
		private uint CreateFontIndirectA(uint lplf)
		{
			_logger.LogInformation("[Gdi32] CreateFontIndirectA(lplf=0x{Lplf:X8})", lplf);
			var handle = _nextGdiObjectHandle++;
			_gdiObjects[handle] = new GdiObject { Type = GdiObjectType.Font };
			return handle;
		}

		[DllModuleExport(16)]
		private uint GetTextExtentPoint32A(uint hdc, in LpcStr lpString, int c, uint psizl)
		{
			var str = lpString.ToString() ?? string.Empty;
			_logger.LogInformation("[Gdi32] GetTextExtentPoint32A(hdc=0x{Hdc:X8}, string=\"{Str}\", c={C})", hdc, str, c);
			if (psizl != 0)
			{
				_env.MemWrite32(psizl, (uint)(c * 8)); // cx
				_env.MemWrite32(psizl + 4, 16); // cy
			}
			return 1; // TRUE
		}

		[DllModuleExport(32)]
		private uint ExtTextOutA(uint hdc, int x, int y, uint options, uint lprect, in LpcStr lpString, uint c, uint lpDx)
		{
			var str = lpString.ToString() ?? string.Empty;
			_logger.LogInformation("[Gdi32] ExtTextOutA(hdc=0x{Hdc:X8}, pos=({X},{Y}), string=\"{Str}\")", hdc, x, y, str);
			return 1; // TRUE
		}

		// Pen and brush functions
		[DllModuleExport(12)]
		private uint CreatePen(int iStyle, int cWidth, uint color)
		{
			_logger.LogInformation("[Gdi32] CreatePen(style={IStyle}, width={CWidth}, color=0x{Color:X8})", iStyle, cWidth, color);
			var handle = _nextGdiObjectHandle++;
			_gdiObjects[handle] = new GdiObject { Type = GdiObjectType.Pen };
			return handle;
		}

		[DllModuleExport(4)]
		private uint CreateSolidBrush(uint color)
		{
			_logger.LogInformation("[Gdi32] CreateSolidBrush(color=0x{Color:X8})", color);
			var handle = _nextGdiObjectHandle++;
			_gdiObjects[handle] = new GdiObject { Type = GdiObjectType.Brush };
			return handle;
		}

		// Palette functions
		[DllModuleExport(4)]
		private uint CreatePalette(uint plpal)
		{
			_logger.LogInformation("[Gdi32] CreatePalette(plpal=0x{Plpal:X8})", plpal);
			var handle = _nextGdiObjectHandle++;
			_gdiObjects[handle] = new GdiObject { Type = GdiObjectType.Palette };
			return handle;
		}

		[DllModuleExport(12)]
		private uint SelectPalette(uint hdc, uint hPal, uint bForceBkgd)
		{
			_logger.LogInformation("[Gdi32] SelectPalette(hdc=0x{Hdc:X8}, hPal=0x{HPal:X8}, bForceBkgd={BForceBkgd})", hdc, hPal, bForceBkgd);
			return hPal; // Return previous palette (stub)
		}

		[DllModuleExport(4)]
		private uint RealizePalette(uint hdc)
		{
			_logger.LogInformation("[Gdi32] RealizePalette(hdc=0x{Hdc:X8})", hdc);
			return 0; // Return 0 (no palette entries changed)
		}

		[DllModuleExport(16)]
		private uint GetSystemPaletteEntries(uint hdc, uint iStart, uint cEntries, uint pPalEntries)
		{
			_logger.LogInformation("[Gdi32] GetSystemPaletteEntries(hdc=0x{Hdc:X8}, iStart={IStart}, cEntries={CEntries})", hdc, iStart, cEntries);
			return 0; // Return 0 (no entries retrieved)
		}

		// Viewport and mapping functions
		[DllModuleExport(8)]
		private int SetMapMode(uint hdc, int iMode)
		{
			_logger.LogInformation("[Gdi32] SetMapMode(hdc=0x{Hdc:X8}, iMode={IMode})", hdc, iMode);
			return 1; // MM_TEXT (previous mode)
		}

		[DllModuleExport(16)]
		private uint SetViewportExtEx(uint hdc, int x, int y, uint lpsz)
		{
			_logger.LogInformation("[Gdi32] SetViewportExtEx(hdc=0x{Hdc:X8}, x={X}, y={Y})", hdc, x, y);
			return 1; // TRUE
		}

		[DllModuleExport(16)]
		private uint SetViewportOrgEx(uint hdc, int x, int y, uint lppt)
		{
			_logger.LogInformation("[Gdi32] SetViewportOrgEx(hdc=0x{Hdc:X8}, x={X}, y={Y})", hdc, x, y);
			return 1; // TRUE
		}

		[DllModuleExport(16)]
		private uint SetWindowExtEx(uint hdc, int x, int y, uint lpsz)
		{
			_logger.LogInformation("[Gdi32] SetWindowExtEx(hdc=0x{Hdc:X8}, x={X}, y={Y})", hdc, x, y);
			return 1; // TRUE
		}

		[DllModuleExport(16)]
		private uint OffsetViewportOrgEx(uint hdc, int x, int y, uint lppt)
		{
			_logger.LogInformation("[Gdi32] OffsetViewportOrgEx(hdc=0x{Hdc:X8}, x={X}, y={Y})", hdc, x, y);
			return 1; // TRUE
		}

		[DllModuleExport(24)]
		private uint ScaleViewportExtEx(uint hdc, int xNum, int xDenom, int yNum, int yDenom, uint lpsz)
		{
			_logger.LogInformation("[Gdi32] ScaleViewportExtEx(hdc=0x{Hdc:X8}, xScale={XNum}/{XDenom}, yScale={YNum}/{YDenom})",
				hdc, xNum, xDenom, yNum, yDenom);
			return 1; // TRUE
		}

		[DllModuleExport(24)]
		private uint ScaleWindowExtEx(uint hdc, int xNum, int xDenom, int yNum, int yDenom, uint lpsz)
		{
			_logger.LogInformation("[Gdi32] ScaleWindowExtEx(hdc=0x{Hdc:X8}, xScale={XNum}/{XDenom}, yScale={YNum}/{YDenom})",
				hdc, xNum, xDenom, yNum, yDenom);
			return 1; // TRUE
		}

		// Clipping functions
		[DllModuleExport(8)]
		private int GetClipBox(uint hdc, uint lprect)
		{
			_logger.LogInformation("[Gdi32] GetClipBox(hdc=0x{Hdc:X8}, lprect=0x{Lprect:X8})", hdc, lprect);
			if (lprect != 0)
			{
				_env.MemWrite32(lprect, 0); // left
				_env.MemWrite32(lprect + 4, 0); // top
				_env.MemWrite32(lprect + 8, 640); // right
				_env.MemWrite32(lprect + 12, 480); // bottom
			}
			return 1; // SIMPLEREGION
		}

		[DllModuleExport(12)]
		private uint PtVisible(uint hdc, int x, int y)
		{
			_logger.LogInformation("[Gdi32] PtVisible(hdc=0x{Hdc:X8}, x={X}, y={Y})", hdc, x, y);
			return 1; // TRUE
		}

		[DllModuleExport(8)]
		private uint RectVisible(uint hdc, uint lprect)
		{
			_logger.LogInformation("[Gdi32] RectVisible(hdc=0x{Hdc:X8}, lprect=0x{Lprect:X8})", hdc, lprect);
			return 1; // TRUE
		}

		// Escape function
		[DllModuleExport(20)]
		private int Escape(uint hdc, int iEscape, int cjIn, uint pvIn, uint pvOut)
		{
			_logger.LogInformation("[Gdi32] Escape(hdc=0x{Hdc:X8}, iEscape={IEscape}, cjIn={CjIn})", hdc, iEscape, cjIn);
			return 0; // Return 0 (not supported)
		}

		// Printing functions
		/// <summary>
		/// Starts a print job.
		/// int StartDocA(
		///   [in] HDC            hdc,
		///   [in] const DOCINFOA *lpdi
		/// );
		/// </summary>
		[DllModuleExport(8)]
		private int StartDocA(uint hdc, uint lpdi)
		{
			_logger.LogInformation("[Gdi32] StartDocA(hdc=0x{Hdc:X8}, lpdi=0x{Lpdi:X8})", hdc, lpdi);

			// DOCINFO structure (simplified):
			// int cbSize;
			// LPCSTR lpszDocName;
			// LPCSTR lpszOutput;
			// LPCSTR lpszDatatype;
			// DWORD fwType;

			if (lpdi != 0)
			{
				var cbSize = (int)_env.MemRead32(lpdi + 0);
				var lpszDocName = _env.MemRead32(lpdi + 4);

				if (lpszDocName != 0)
				{
					var docName = _env.ReadAnsiString(lpszDocName);
					_logger.LogInformation("[Gdi32] StartDocA: Document name=\"{DocName}\"", docName);
				}
			}

			// Return a positive job identifier
			return 1;
		}

		/// <summary>
		/// Ends a print job.
		/// int EndDoc(
		///   [in] HDC hdc
		/// );
		/// </summary>
		[DllModuleExport(4)]
		private int EndDoc(uint hdc)
		{
			_logger.LogInformation("[Gdi32] EndDoc(hdc=0x{Hdc:X8})", hdc);

			// EndDoc ends the current print job
			// Return success
			return 1;
		}

		/// <summary>
		/// Prepares the printer driver to accept data.
		/// int StartPage(
		///   [in] HDC hdc
		/// );
		/// </summary>
		[DllModuleExport(4)]
		private int StartPage(uint hdc)
		{
			_logger.LogInformation("[Gdi32] StartPage(hdc=0x{Hdc:X8})", hdc);

			// StartPage prepares the printer driver to accept data
			// Return success (greater than zero)
			return 1;
		}

		/// <summary>
		/// Notifies the device that the application has finished writing to a page.
		/// int EndPage(
		///   [in] HDC hdc
		/// );
		/// </summary>
		[DllModuleExport(4)]
		private int EndPage(uint hdc)
		{
			_logger.LogInformation("[Gdi32] EndPage(hdc=0x{Hdc:X8})", hdc);

			// EndPage notifies the device that the application has finished writing to a page
			// Return success (greater than zero)
			return 1;
		}

		// GDI utility functions
		/// <summary>
		/// Flushes the calling thread's current batch.
		/// BOOL GdiFlush();
		/// </summary>
		[DllModuleExport(0)]
		private uint GdiFlush()
		{
			_logger.LogInformation("[Gdi32] GdiFlush()");

			// GdiFlush forces all pending GDI operations to complete
			// For emulation, we don't have batching, so just return success
			return 1; // TRUE
		}

		/// <summary>
		/// Retrieves the current system palette use for the specified device context.
		/// UINT GetSystemPaletteUse(
		///   [in] HDC hdc
		/// );
		/// </summary>
		[DllModuleExport(4)]
		private uint GetSystemPaletteUse(uint hdc)
		{
			_logger.LogInformation("[Gdi32] GetSystemPaletteUse(hdc=0x{Hdc:X8})", hdc);

			// GetSystemPaletteUse returns the palette mode for the device context
			// SYSPAL_NOSTATIC (2) - System palette contains no static colors except black and white
			// SYSPAL_STATIC (1) - System palette contains static colors (default)
			// SYSPAL_ERROR (0) - Error

			// Return SYSPAL_STATIC (default mode)
			return 1;
		}

		// Additional bitmap functions
		[DllModuleExport(12)]
		private int GetBitmapBits(uint hBitmap, int cb, uint lpvBits)
		{
			_logger.LogInformation("[Gdi32] GetBitmapBits(hBitmap=0x{HBitmap:X8}, cb={Cb}, lpvBits=0x{LpvBits:X8})", hBitmap, cb, lpvBits);
			// Stub - return 0 (no bits copied)
			return 0;
		}

		[DllModuleExport(12)]
		private int SetBitmapBits(uint hBitmap, uint cb, uint pvBits)
		{
			_logger.LogInformation("[Gdi32] SetBitmapBits(hBitmap=0x{HBitmap:X8}, cb={Cb}, pvBits=0x{PvBits:X8})", hBitmap, cb, pvBits);
			// Stub - return cb (all bits set)
			return (int)cb;
		}

		[DllModuleExport(24)]
		private uint CreateDIBSection(uint hdc, uint pbmi, uint usage, uint ppvBits, uint hSection, uint offset)
		{
			_logger.LogInformation("[Gdi32] CreateDIBSection(hdc=0x{Hdc:X8}, pbmi=0x{Pbmi:X8}, usage={Usage})", hdc, pbmi, usage);

			// Create a dummy bitmap handle
			var bitmapHandle = _nextGdiObjectHandle++;
			_gdiObjects[bitmapHandle] = new GdiObject { Type = GdiObjectType.Bitmap };

			// If ppvBits is provided, write a dummy pointer
			if (ppvBits != 0)
			{
				_env.MemWrite32(ppvBits, 0x90000000); // Dummy bits pointer
			}

			return bitmapHandle;
		}

		[DllModuleExport(16)]
		private uint SetDIBColorTable(uint hdc, uint iStart, uint cEntries, uint prgbq)
		{
			_logger.LogInformation("[Gdi32] SetDIBColorTable(hdc=0x{Hdc:X8}, iStart={IStart}, cEntries={CEntries}, prgbq=0x{Prgbq:X8})",
				hdc, iStart, cEntries, prgbq);
			// Stub - return number of entries set
			return cEntries;
		}

		// Palette functions
		[DllModuleExport(16)]
		private uint AnimatePalette(uint hPal, uint iStartIndex, uint cEntries, uint ppe)
		{
			_logger.LogInformation("[Gdi32] AnimatePalette(hPal=0x{HPal:X8}, iStartIndex={IStartIndex}, cEntries={CEntries})",
				hPal, iStartIndex, cEntries);
			// Stub - return TRUE (success)
			return 1;
		}

		[DllModuleExport(4)]
		private uint CreateHalftonePalette(uint hdc)
		{
			_logger.LogInformation("[Gdi32] CreateHalftonePalette(hdc=0x{Hdc:X8})", hdc);

			// Create a dummy palette handle
			var paletteHandle = _nextGdiObjectHandle++;
			_gdiObjects[paletteHandle] = new GdiObject { Type = GdiObjectType.Palette };

			return paletteHandle;
		}

		[DllModuleExport(8)]
		private uint GetNearestPaletteIndex(uint hPal, uint color)
		{
			_logger.LogInformation("[Gdi32] GetNearestPaletteIndex(hPal=0x{HPal:X8}, color=0x{Color:X8})", hPal, color);
			// Stub - return 0 (first palette entry)
			return 0;
		}

		[DllModuleExport(16)]
		private uint GetPaletteEntries(uint hPal, uint iStart, uint cEntries, uint pPalEntries)
		{
			_logger.LogInformation("[Gdi32] GetPaletteEntries(hPal=0x{HPal:X8}, iStart={IStart}, cEntries={CEntries}, pPalEntries=0x{PPalEntries:X8})",
				hPal, iStart, cEntries, pPalEntries);
			// Stub - return 0 (no entries)
			return 0;
		}

		// Text functions
		[DllModuleExport(8)]
		private uint GetTextMetricsA(uint hdc, uint lptm)
		{
			_logger.LogInformation("[Gdi32] GetTextMetricsA(hdc=0x{Hdc:X8}, lptm=0x{Lptm:X8})", hdc, lptm);

			if (lptm != 0)
			{
				// Fill in TEXTMETRIC structure with default values
				_env.MemWrite32(lptm, 16);       // tmHeight
				_env.MemWrite32(lptm + 4, 14);   // tmAscent
				_env.MemWrite32(lptm + 8, 2);    // tmDescent
				_env.MemWrite32(lptm + 12, 0);   // tmInternalLeading
				_env.MemWrite32(lptm + 16, 0);   // tmExternalLeading
				_env.MemWrite32(lptm + 20, 8);   // tmAveCharWidth
				_env.MemWrite32(lptm + 24, 8);   // tmMaxCharWidth
				_env.MemWrite32(lptm + 28, 400); // tmWeight
												 // NOTE: The TEXTMETRIC structure has additional fields beyond tmWeight (such as tmItalic, tmUnderlined, tmStruckOut, tmFirstChar, etc.)
												 // that are not initialized here. The structure is only partially initialized; additional fields may need to be filled
												 // if required by the application.
			}

			return 1; // TRUE
		}

		// Advanced drawing functions
		[DllModuleExport(24)]
		private uint PatBlt(uint hdc, int x, int y, int w, int h, uint rop)
		{
			_logger.LogInformation("[Gdi32] PatBlt(hdc=0x{Hdc:X8}, x={X}, y={Y}, w={W}, h={H}, rop=0x{Rop:X8})",
				hdc, x, y, w, h, rop);
			// Stub - return TRUE (success)
			return 1;
		}

		[DllModuleExport(36)]
		private uint Chord(uint hdc, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4)
		{
			_logger.LogInformation("[Gdi32] Chord(hdc=0x{Hdc:X8}, x1={X1}, y1={Y1}, x2={X2}, y2={Y2}, x3={X3}, y3={Y3}, x4={X4}, y4={Y4})",
				hdc, x1, y1, x2, y2, x3, y3, x4, y4);
			// Stub - return TRUE (success)
			return 1;
		}

		[DllModuleExport(36)]
		private uint Pie(uint hdc, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4)
		{
			_logger.LogInformation("[Gdi32] Pie(hdc=0x{Hdc:X8}, x1={X1}, y1={Y1}, x2={X2}, y2={Y2}, x3={X3}, y3={Y3}, x4={X4}, y4={Y4})",
				hdc, x1, y1, x2, y2, x3, y3, x4, y4);
			// Stub - return TRUE (success)
			return 1;
		}

		[DllModuleExport(12)]
		private uint Polygon(uint hdc, uint apt, int cpt)
		{
			_logger.LogInformation("[Gdi32] Polygon(hdc=0x{Hdc:X8}, apt=0x{Apt:X8}, cpt={Cpt})", hdc, apt, cpt);
			// Stub - return TRUE (success)
			return 1;
		}

		[DllModuleExport(28)]
		private uint RoundRect(uint hdc, int left, int top, int right, int bottom, int width, int height)
		{
			_logger.LogInformation("[Gdi32] RoundRect(hdc=0x{Hdc:X8}, left={Left}, top={Top}, right={Right}, bottom={Bottom}, width={Width}, height={Height})",
				hdc, left, top, right, bottom, width, height);
			// Stub - return TRUE (success)
			return 1;
		}

		/// <summary>
		/// Determines whether a specified string fits within a specified rectangle.
		/// BOOL GetTextExtentPointA(HDC hdc, LPCSTR lpString, int c, LPSIZE lpsz);
		/// </summary>
		[DllModuleExport(16)]
		private uint GetTextExtentPointA(uint hdc, in LpcStr lpString, int c, uint lpsz)
		{
			var str = lpString.ToString() ?? string.Empty;
			_logger.LogInformation("[Gdi32] GetTextExtentPointA(hdc=0x{Hdc:X8}, lpString=\"{Str}\", c={C}, lpsz=0x{Lpsz:X8})",
				hdc, str, c, lpsz);

			// Stub: Return a default size (8x16 per character)
			if (lpsz != 0)
			{
				var width = c * 8;
				var height = 16;
				_env.MemWrite32(lpsz, (uint)width);      // cx
				_env.MemWrite32(lpsz + 4, (uint)height); // cy
			}

			return 1; // TRUE
		}

		/// <summary>
		/// Retrieves the current text color.
		/// COLORREF GetTextColor(HDC hdc);
		/// </summary>
		[DllModuleExport(4)]
		private uint GetTextColor(uint hdc)
		{
			_logger.LogInformation("[Gdi32] GetTextColor(hdc=0x{Hdc:X8})", hdc);

			if (_deviceContexts.TryGetValue(hdc, out var dc))
			{
				return dc.TextColor;
			}

			// Return default black color
			return 0x00000000;
		}

		/// <summary>
		/// Retrieves the current background color.
		/// COLORREF GetBkColor(HDC hdc);
		/// </summary>
		[DllModuleExport(4)]
		private uint GetBkColor(uint hdc)
		{
			_logger.LogInformation("[Gdi32] GetBkColor(hdc=0x{Hdc:X8})", hdc);

			// Stub: Return white background
			return 0x00FFFFFF;
		}

		/// <summary>
		/// Converts device points to logical points.
		/// BOOL DPtoLP(HDC hdc, LPPOINT lppt, int c);
		/// </summary>
		[DllModuleExport(12)]
		private uint DPtoLP(uint hdc, uint lppt, int c)
		{
			_logger.LogInformation("[Gdi32] DPtoLP(hdc=0x{Hdc:X8}, lppt=0x{Lppt:X8}, c={C})", hdc, lppt, c);

			// Stub: In MM_TEXT mode (default), device points = logical points, so no conversion needed
			return 1; // TRUE
		}

		/// <summary>
		/// Converts logical points to device points.
		/// BOOL LPtoDP(HDC hdc, LPPOINT lppt, int c);
		/// </summary>
		[DllModuleExport(12)]
		private uint LPtoDP(uint hdc, uint lppt, int c)
		{
			_logger.LogInformation("[Gdi32] LPtoDP(hdc=0x{Hdc:X8}, lppt=0x{Lppt:X8}, c={C})", hdc, lppt, c);

			// Stub: In MM_TEXT mode (default), logical points = device points, so no conversion needed
			return 1; // TRUE
		}

		/// <summary>
		/// Retrieves the viewport extent.
		/// BOOL GetViewportExtEx(HDC hdc, LPSIZE lpsize);
		/// </summary>
		[DllModuleExport(8)]
		private uint GetViewportExtEx(uint hdc, uint lpsize)
		{
			_logger.LogInformation("[Gdi32] GetViewportExtEx(hdc=0x{Hdc:X8}, lpsize=0x{Lpsize:X8})", hdc, lpsize);

			// Stub: Return default extent (1:1 mapping)
			if (lpsize != 0)
			{
				_env.MemWrite32(lpsize, 1);     // cx
				_env.MemWrite32(lpsize + 4, 1); // cy
			}

			return 1; // TRUE
		}

		/// <summary>
		/// Retrieves the window extent.
		/// BOOL GetWindowExtEx(HDC hdc, LPSIZE lpsize);
		/// </summary>
		[DllModuleExport(8)]
		private uint GetWindowExtEx(uint hdc, uint lpsize)
		{
			_logger.LogInformation("[Gdi32] GetWindowExtEx(hdc=0x{Hdc:X8}, lpsize=0x{Lpsize:X8})", hdc, lpsize);

			// Stub: Return default extent (1:1 mapping)
			if (lpsize != 0)
			{
				_env.MemWrite32(lpsize, 1);     // cx
				_env.MemWrite32(lpsize + 4, 1); // cy
			}

			return 1; // TRUE
		}

		/// <summary>
		/// Retrieves the current mapping mode.
		/// int GetMapMode(HDC hdc);
		/// </summary>
		[DllModuleExport(4)]
		private int GetMapMode(uint hdc)
		{
			_logger.LogInformation("[Gdi32] GetMapMode(hdc=0x{Hdc:X8})", hdc);

			// Stub: Return MM_TEXT (default mapping mode)
			return 1; // MM_TEXT
		}

		/// <summary>
		/// Creates a device-dependent bitmap (DDB) from a device-independent bitmap (DIB).
		/// HBITMAP CreateDIBitmap(HDC hdc, const BITMAPINFOHEADER *pbmih, DWORD flInit, const VOID *pjBits, const BITMAPINFO *pbmi, UINT iUsage);
		/// </summary>
		[DllModuleExport(24)]
		private uint CreateDIBitmap(uint hdc, uint pbmih, uint flInit, uint pjBits, uint pbmi, uint iUsage)
		{
			_logger.LogInformation("[Gdi32] CreateDIBitmap(hdc=0x{Hdc:X8}, pbmih=0x{Pbmih:X8}, flInit=0x{FlInit:X}, pjBits=0x{PjBits:X8}, pbmi=0x{Pbmi:X8}, iUsage=0x{IUsage:X})",
				hdc, pbmih, flInit, pjBits, pbmi, iUsage);

			// Stub: Return a fake bitmap handle
			var handle = _nextGdiObjectHandle++;
			_gdiObjects[handle] = new GdiObject { Type = GdiObjectType.Bitmap };
			return handle;
		}

		/// <summary>
		/// Creates a rectangular clipping region that is the intersection of the current clipping region and the specified rectangle.
		/// int IntersectClipRect(HDC hdc, int left, int top, int right, int bottom);
		/// </summary>
		[DllModuleExport(20)]
		private int IntersectClipRect(uint hdc, int left, int top, int right, int bottom)
		{
			_logger.LogInformation("[Gdi32] IntersectClipRect(hdc=0x{Hdc:X8}, left={Left}, top={Top}, right={Right}, bottom={Bottom})",
				hdc, left, top, right, bottom);

			// Stub: Return SIMPLEREGION (simple rectangular region)
			return 2; // SIMPLEREGION
		}

		/// <summary>
		/// Sets RGB values and flags in a range of entries in a logical palette.
		/// UINT SetPaletteEntries(
		///   [in] HPALETTE       hpal,
		///   [in] UINT           iStart,
		///   [in] UINT           cEntries,
		///   [in] const PALETTEENTRY *pPalEntries
		/// );
		/// </summary>
		[DllModuleExport(16)]
		private uint SetPaletteEntries(uint hpal, uint iStart, uint cEntries, uint pPalEntries)
		{
			_logger.LogInformation("[Gdi32] SetPaletteEntries(hpal=0x{Hpal:X8}, iStart={IStart}, cEntries={CEntries}, pPalEntries=0x{PPalEntries:X8})",
				hpal, iStart, cEntries, pPalEntries);

			// PALETTEENTRY structure is 4 bytes: peRed, peGreen, peBlue, peFlags
			// For a stub implementation, we just acknowledge the entries were set
			// In a real implementation, we would store the palette data

			if (pPalEntries == 0 || cEntries == 0)
			{
				return 0; // Return 0 if invalid parameters
			}

			// Return the number of entries set (stub)
			return cEntries;
		}

		/// <summary>
		/// Resets the origin of a brush or resets a logical palette.
		/// BOOL UnrealizeObject(
		///   [in] HGDIOBJ h
		/// );
		/// </summary>
		[DllModuleExport(4)]
		private uint UnrealizeObject(uint hgdiobj)
		{
			_logger.LogInformation("[Gdi32] UnrealizeObject(hgdiobj=0x{Hgdiobj:X8})", hgdiobj);

			// UnrealizeObject is used primarily for palette objects
			// It indicates that the palette should be completely remapped next time it's selected
			// For brushes, it resets the brush origin

			// Check if this is a known GDI object
			if (_gdiObjects.TryGetValue(hgdiobj, out var obj))
			{
				_logger.LogInformation("[Gdi32] UnrealizeObject: Object type is {Type}", obj.Type);

				// For palettes, this marks them as needing to be realized again
				// For other object types, this typically does nothing
				// Return TRUE to indicate success
				return 1;
			}

			// If object is not in our tracking, still return success
			// as this may be a stock object or system object
			_logger.LogInformation("[Gdi32] UnrealizeObject: Object not tracked, returning success");
			return 1; // TRUE
		}

		/// <summary>
		/// Creates a rectangular region.
		/// HRGN CreateRectRgn(
		///   int x1,
		///   int y1,
		///   int x2,
		///   int y2
		/// );
		/// </summary>
		[DllModuleExport(0)]
		private uint CreateRectRgn(int x1, int y1, int x2, int y2)
		{
			_logger.LogInformation("[Gdi32] CreateRectRgn(x1={X1}, y1={Y1}, x2={X2}, y2={Y2})", x1, y1, x2, y2);

			// Create a new region object
			var regionHandle = _nextGdiObjectHandle++;
			_gdiObjects[regionHandle] = new GdiObject
			{
				Type = GdiObjectType.Region
			};

			return regionHandle;
		}

		/// <summary>
		/// Retrieves data for a specified region.
		/// DWORD GetRegionData(
		///   HRGN   hrgn,
		///   DWORD  nCount,
		///   LPRGNDATA lpRgnData
		/// );
		/// </summary>
		[DllModuleExport(0)]
		private uint GetRegionData(uint hrgn, uint nCount, uint lpRgnData)
		{
			_logger.LogInformation("[Gdi32] GetRegionData(hrgn=0x{Hrgn:X8}, nCount={NCount}, lpRgnData=0x{LpRgnData:X8})",
				hrgn, nCount, lpRgnData);

			if (!_gdiObjects.ContainsKey(hrgn))
			{
				_logger.LogWarning("[Gdi32] GetRegionData: Invalid region handle");
				return 0;
			}

			// Return a minimal RGNDATA structure
			// RGNDATA header is 32 bytes:
			// - dwSize: 32 (DWORD)
			// - iType: 1 (RDH_RECTANGLES) (DWORD)
			// - nCount: 1 (DWORD)
			// - nRgnSize: 16 (DWORD)
			// - rcBound: RECT (16 bytes)
			const uint headerSize = 32;
			const uint rectSize = 16;
			const uint totalSize = headerSize + rectSize;

			if (lpRgnData == 0)
			{
				// Return required buffer size
				return totalSize;
			}

			if (nCount < totalSize)
			{
				// Buffer too small
				_logger.LogWarning("[Gdi32] GetRegionData: Buffer too small");
				return 0;
			}

			// Write RGNDATA structure
			_env.MemWrite32(lpRgnData + 0, headerSize);      // dwSize
			_env.MemWrite32(lpRgnData + 4, 1);               // iType (RDH_RECTANGLES)
			_env.MemWrite32(lpRgnData + 8, 1);               // nCount
			_env.MemWrite32(lpRgnData + 12, rectSize);       // nRgnSize
															 // rcBound (RECT)
			_env.MemWrite32(lpRgnData + 16, 0);              // left
			_env.MemWrite32(lpRgnData + 20, 0);              // top
			_env.MemWrite32(lpRgnData + 24, 100);            // right
			_env.MemWrite32(lpRgnData + 28, 100);            // bottom
															 // Rectangle data
			_env.MemWrite32(lpRgnData + 32, 0);              // left
			_env.MemWrite32(lpRgnData + 36, 0);              // top
			_env.MemWrite32(lpRgnData + 40, 100);            // right
			_env.MemWrite32(lpRgnData + 44, 100);            // bottom

			return totalSize;
		}

		/// <summary>
		/// Draws an ellipse.
		/// BOOL Ellipse(
		///   HDC hdc,
		///   int left,
		///   int top,
		///   int right,
		///   int bottom
		/// );
		/// </summary>
		[DllModuleExport(20)]
		private uint Ellipse(uint hdc, int left, int top, int right, int bottom)
		{
			_logger.LogInformation("[Gdi32] Ellipse(hdc=0x{Hdc:X8}, left={Left}, top={Top}, right={Right}, bottom={Bottom})",
				hdc, left, top, right, bottom);
			// Stub - return TRUE (success)
			return 1;
		}

		/// <summary>
		/// Draws an elliptical arc.
		/// BOOL Arc(
		///   HDC hdc,
		///   int x1, int y1,
		///   int x2, int y2,
		///   int x3, int y3,
		///   int x4, int y4
		/// );
		/// </summary>
		[DllModuleExport(36)]
		private uint Arc(uint hdc, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4)
		{
			_logger.LogInformation("[Gdi32] Arc(hdc=0x{Hdc:X8}, x1={X1}, y1={Y1}, x2={X2}, y2={Y2}, x3={X3}, y3={Y3}, x4={X4}, y4={Y4})",
				hdc, x1, y1, x2, y2, x3, y3, x4, y4);
			// Stub - return TRUE (success)
			return 1;
		}

		/// <summary>
		/// Draws a series of line segments.
		/// BOOL Polyline(
		///   HDC hdc,
		///   const POINT *apt,
		///   int cpt
		/// );
		/// </summary>
		[DllModuleExport(12)]
		private uint Polyline(uint hdc, uint apt, int cpt)
		{
			_logger.LogInformation("[Gdi32] Polyline(hdc=0x{Hdc:X8}, apt=0x{Apt:X8}, cpt={Cpt})", hdc, apt, cpt);
			// Stub - return TRUE (success)
			return 1;
		}

		/// <summary>
		/// Draws formatted text (stub for UNICODE version).
		/// int DrawText(
		///   HDC hdc,
		///   LPCWSTR lpchText,
		///   int cchText,
		///   LPRECT lprc,
		///   UINT format
		/// );
		/// </summary>
		[DllModuleExport(20, IsStub = true)]
		private int DrawText(uint hdc, uint lpchText, int cchText, uint lprc, uint format)
		{
			return 0;
		}

		/// <summary>
		/// Draws formatted text in the specified rectangle.
		/// int DrawTextA(
		///   HDC hdc,
		///   LPCSTR lpchText,
		///   int cchText,
		///   LPRECT lprc,
		///   UINT format
		/// );
		/// </summary>
		[DllModuleExport(20)]
		private int DrawTextA(uint hdc, in LpcStr lpchText, int cchText, uint lprc, uint format)
		{
			var text = lpchText.ToString() ?? string.Empty;
			
			// Read rectangle if provided
			int left = 0, top = 0, right = 0, bottom = 0;
			if (lprc != 0)
			{
				left = (int)_env.MemRead32(lprc);
				top = (int)_env.MemRead32(lprc + 4);
				right = (int)_env.MemRead32(lprc + 8);
				bottom = (int)_env.MemRead32(lprc + 12);
			}

			_logger.LogInformation("[Gdi32] DrawTextA(hdc=0x{Hdc:X8}, text=\"{Text}\", rect=({Left},{Top},{Right},{Bottom}), format=0x{Format:X})",
				hdc, text, left, top, right, bottom, format);

			// Calculate text height (stub implementation)
			// DT_CALCRECT (0x400) means calculate the rectangle needed
			if ((format & 0x400) != 0 && lprc != 0)
			{
				// Update rectangle with calculated size
				var textLength = cchText < 0 ? text.Length : Math.Min(cchText, text.Length);
				var height = 16; // Default font height
				_env.MemWrite32(lprc + 8, (uint)(left + textLength * 8)); // right
				_env.MemWrite32(lprc + 12, (uint)(top + height)); // bottom
			}

			// Return height of text drawn
			return 16;
		}

		/// <summary>
		/// Draws a border around a rectangle.
		/// int FrameRect(
		///   HDC hdc,
		///   const RECT *lprc,
		///   HBRUSH hbr
		/// );
		/// </summary>
		[DllModuleExport(12)]
		private uint FrameRect(uint hdc, uint lprc, uint hbr)
		{
			if (lprc != 0)
			{
				var left = (int)_env.MemRead32(lprc);
				var top = (int)_env.MemRead32(lprc + 4);
				var right = (int)_env.MemRead32(lprc + 8);
				var bottom = (int)_env.MemRead32(lprc + 12);
				_logger.LogInformation("[Gdi32] FrameRect(hdc=0x{Hdc:X8}, rect=({Left},{Top},{Right},{Bottom}), hbr=0x{Hbr:X8})",
					hdc, left, top, right, bottom, hbr);
			}
			// Stub - return non-zero on success
			return 1;
		}

		/// <summary>
		/// Inverts the colors in a rectangle.
		/// BOOL InvertRect(
		///   HDC hdc,
		///   const RECT *lprc
		/// );
		/// </summary>
		[DllModuleExport(8)]
		private uint InvertRect(uint hdc, uint lprc)
		{
			if (lprc != 0)
			{
				var left = (int)_env.MemRead32(lprc);
				var top = (int)_env.MemRead32(lprc + 4);
				var right = (int)_env.MemRead32(lprc + 8);
				var bottom = (int)_env.MemRead32(lprc + 12);
				_logger.LogInformation("[Gdi32] InvertRect(hdc=0x{Hdc:X8}, rect=({Left},{Top},{Right},{Bottom}))",
					hdc, left, top, right, bottom);
			}
			// Stub - return TRUE (success)
			return 1;
		}

		private enum GdiObjectType
		{
			Pen,
			Brush,
			Font,
			Bitmap,
			Palette,
			Region
		}

		private class GdiObject
		{
			public GdiObjectType Type { get; set; }
		}

		private class DeviceContext
		{
			public uint Handle { get; set; }
			public uint WindowHandle { get; set; }
			public int BkMode { get; set; } = 2; // OPAQUE
			public uint TextColor { get; set; } = 0x00000000; // Black
		}
	}
}