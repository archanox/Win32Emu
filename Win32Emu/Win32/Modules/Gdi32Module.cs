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

		// Default font metrics for stub implementations
		private const int DefaultCharWidth = 8;
		private const int DefaultFontHeight = 16;

		// Default window dimensions for BeginPaint when window info is unavailable
		private const int DefaultWindowWidth = 640;
		private const int DefaultWindowHeight = 480;

		// GDI object type constants (from GetObjectType)
		private enum GdiObjectTypeId : uint
		{
			OBJ_PEN = 1,
			OBJ_BRUSH = 2,
			OBJ_DC = 3,
			OBJ_METADC = 4,
			OBJ_PAL = 5,
			OBJ_FONT = 6,
			OBJ_BITMAP = 7,
			OBJ_REGION = 8,
			OBJ_METAFILE = 9,
			OBJ_MEMDC = 10,
			OBJ_EXTPEN = 11
		}

		// Raster operation codes for BitBlt, PatBlt, and StretchBlt
		private enum RasterOperation : uint
		{
			BLACKNESS = 0x00000042,
			NOTSRCERASE = 0x001100A6,
			NOTSRCCOPY = 0x00330008,
			SRCERASE = 0x00440328,
			DSTINVERT = 0x00550009,
			PATINVERT = 0x005A0049,
			SRCINVERT = 0x00660046,
			SRCAND = 0x008800C6,
			MERGEPAINT = 0x00BB0226,
			MERGECOPY = 0x00C000CA,
			SRCCOPY = 0x00CC0020,
			SRCPAINT = 0x00EE0086,
			PATCOPY = 0x00F00021,
			PATPAINT = 0x00FB0A09,
			WHITENESS = 0x00FF0062
		}

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
				case "GETTEXTALIGN":
					returnValue = GetTextAlign(a.UInt32(0));
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
				case "STRETCHBLT":
					returnValue = StretchBlt(a.UInt32(0), a.Int32(1), a.Int32(2), a.Int32(3), a.Int32(4), a.UInt32(5), a.Int32(6), a.Int32(7), a.Int32(8), a.Int32(9), a.UInt32(10));
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
				case "DRAWTEXT": // Unicode version - stub
					returnValue = (uint)DrawText(a.UInt32(0), a.UInt32(1), a.Int32(2), a.UInt32(3), a.UInt32(4));
					return true;
				case "DRAWTEXTA": // ANSI version - implemented
					returnValue = (uint)DrawTextA(a.UInt32(0), a.LpcStr(1), a.Int32(2), a.UInt32(3), a.UInt32(4));
					return true;
				case "FRAMERECT":
					returnValue = FrameRect(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "INVERTRECT":
					returnValue = InvertRect(a.UInt32(0), a.UInt32(1));
					return true;

				case "ABORTDOC":
					returnValue = AbortDoc(a.UInt32(0));
					return true;

				case "CREATEPATTERNBRUSH":
					returnValue = CreatePatternBrush(a.UInt32(0));
					return true;

				case "EXCLUDECLIPRECT":
					returnValue = ExcludeClipRect(a.UInt32(0), a.Int32(1), a.Int32(2), a.Int32(3), a.Int32(4));
					return true;

				case "SELECTCLIPRGN":
					returnValue = SelectClipRgn(a.UInt32(0), a.UInt32(1));
					return true;

				case "SETABORTPROC":
					returnValue = SetAbortProc(a.UInt32(0), a.UInt32(1));
					return true;

				// Metafile functions
				case "GETWINMETAFILEBITS":
					returnValue = GetWinMetaFileBits(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.Int32(3), a.UInt32(4));
					return true;
				case "SETMETAFILEBITSEX":
					returnValue = SetMetaFileBitsEx(a.UInt32(0), a.UInt32(1));
					return true;
				case "SETWINMETAFILEBITS":
					returnValue = SetWinMetaFileBits(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;
				case "PLAYENHMETAFILE":
					returnValue = PlayEnhMetaFile(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "TRANSLATECHARSETINFO":
					returnValue = TranslateCharsetInfo(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				// Missing functions from issue
				case "SETSTRETCHBLTMODE":
					returnValue = (uint)SetStretchBltMode(a.UInt32(0), a.Int32(1));
					return true;
				case "STRETCHDIBITS":
					returnValue = (uint)StretchDIBits(a.UInt32(0), a.Int32(1), a.Int32(2), a.Int32(3), a.Int32(4), a.Int32(5), a.Int32(6), a.Int32(7), a.Int32(8), a.UInt32(9), a.UInt32(10), a.UInt32(11));
					return true;
				case "SETOBJECTOWNER":
					SetObjectOwner(a.UInt32(0), a.UInt32(1));
					returnValue = 1; // Assume success
					return true;
				case "GETOBJECTTYPE":
					returnValue = GetObjectType(a.UInt32(0));
					return true;
				case "ENUMFONTFAMILIESEXA":
					returnValue = EnumFontFamiliesExA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3), a.UInt32(4));
					return true;
				case "GETNEARESTCOLOR":
					returnValue = GetNearestColor(a.UInt32(0), a.UInt32(1));
					return true;
				case "RESIZEPALETTE":
					returnValue = ResizePalette(a.UInt32(0), a.UInt32(1));
					return true;
				case "EXTESCAPE":
					returnValue = (uint)ExtEscape(a.UInt32(0), a.Int32(1), a.Int32(2), a.UInt32(3), a.Int32(4), a.UInt32(5));
					return true;
				case "GETDEVICEGAMMARAMP":
					returnValue = GetDeviceGammaRamp(a.UInt32(0), a.UInt32(1));
					return true;
				case "SETDEVICEGAMMARAMP":
					returnValue = SetDeviceGammaRamp(a.UInt32(0), a.UInt32(1));
					return true;
				case "SETSYSTEMPALETTEUSE":
					returnValue = SetSystemPaletteUse(a.UInt32(0), a.UInt32(1));
					return true;

				// Region functions
				case "COMBINERGN":
					returnValue = (uint)CombineRgn(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.Int32(3));
					return true;
				case "CREATERECTRGNINDIRECT":
					returnValue = CreateRectRgnIndirect(a.UInt32(0));
					return true;
				case "EQUALRGN":
					returnValue = EqualRgn(a.UInt32(0), a.UInt32(1));
					return true;
				case "GETCLIPRGN":
					returnValue = (uint)GetClipRgn(a.UInt32(0), a.UInt32(1));
					return true;
				case "GETRANDOMRGN":
					returnValue = (uint)GetRandomRgn(a.UInt32(0), a.UInt32(1), a.Int32(2));
					return true;
				case "GETRGNBOX":
					returnValue = (uint)GetRgnBox(a.UInt32(0), a.UInt32(1));
					return true;
				case "SETRECTRGN":
					returnValue = SetRectRgn(a.UInt32(0), a.Int32(1), a.Int32(2), a.Int32(3), a.Int32(4));
					return true;
				case "FILLRGN":
					returnValue = FillRgn(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "GETROP2":
					returnValue = (uint)GetROP2(a.UInt32(0));
					return true;
				case "SETROP2":
					returnValue = (uint)SetROP2(a.UInt32(0), a.Int32(1));
					return true;
				case "SETPIXELV":
					returnValue = SetPixelV(a.UInt32(0), a.Int32(1), a.Int32(2), a.UInt32(3));
					return true;
				case "GETOBJECTW":
					returnValue = (uint)GetObjectW(a.UInt32(0), a.Int32(1), a.UInt32(2));
					return true;
				case "ENUMFONTFAMILIESA":
					returnValue = EnumFontFamiliesA(a.UInt32(0), a.LpcStr(1), a.UInt32(2), a.UInt32(3));
					return true;
				case "GDIGETCHARDIMENSIONS":
					returnValue = (uint)GdiGetCharDimensions(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;
				case "GDIGETCODEPAGE":
					returnValue = GdiGetCodePage(a.UInt32(0));
					return true;
				case "GETCHARABCWIDTHSA":
					returnValue = GetCharABCWidthsA(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;
				case "GETCHARABCWIDTHSW":
					returnValue = GetCharABCWidthsW(a.UInt32(0), a.UInt32(1), a.UInt32(2), a.UInt32(3));
					return true;
				case "GETTEXTCHARSETINFO":
					returnValue = GdiGetCodePage(a.UInt32(0)); // Simplified: reuse GdiGetCodePage
					return true;
				case "GETTEXTMETRICSW":
					returnValue = GetTextMetricsW(a.UInt32(0), a.UInt32(1));
					return true;
				case "CREATEMETAFILEA":
					returnValue = CreateMetaFileA(a.LpcStr(0));
					return true;
				case "CLOSEMETAFILE":
					returnValue = CloseMetaFile(a.UInt32(0));
					return true;
				case "CREATEENHMETAFILEA":
					returnValue = CreateEnhMetaFileA(a.UInt32(0), a.LpcStr(1), a.UInt32(2), a.LpcStr(3));
					return true;
				case "CLOSEENHMETAFILE":
					returnValue = CloseEnhMetaFile(a.UInt32(0));
					return true;
				case "GETENHMETAFILEBITS":
					returnValue = GetEnhMetaFileBits(a.UInt32(0), a.UInt32(1), a.UInt32(2));
					return true;

				// Information context (IC) functions
				case "CREATEICA":
					returnValue = CreateICA(a.LpcStr(0), a.LpcStr(1), a.LpcStr(2), a.UInt32(3));
					return true;
				case "CREATEICW":
					returnValue = CreateICW(a.LpcWStr(0), a.LpcWStr(1), a.LpcWStr(2), a.UInt32(3));
					return true;

				// Device context (DC) creation - Unicode
				case "CREATEDCW":
					returnValue = CreateDCW(a.LpcWStr(0), a.LpcWStr(1), a.LpcWStr(2), a.UInt32(3));
					return true;

				// Font creation - Unicode
				case "CREATEFONTINDIRECTW":
					returnValue = CreateFontIndirectW(a.UInt32(0));
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
			if (stockObjectId is < (int)NativeTypes.StockObject.WHITE_BRUSH or > (int)NativeTypes.StockObject.DC_PEN)
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

			// Get window dimensions (default to 640x480 if window not found)
			var width = DefaultWindowWidth;
			var height = DefaultWindowHeight;
			var windowInfo = _env.GetWindow(hwnd);
			if (windowInfo.HasValue)
			{
				width = windowInfo.Value.Width;
				height = windowInfo.Value.Height;
			}

			// Create or get a bitmap for this window
			var bitmapHandle = CreateCompatibleBitmap(0, width, height);

			// Create a device context for this paint session
			var hdc = _nextDcHandle++;
			var dc = new DeviceContext
			{
				Handle = hdc,
				WindowHandle = hwnd,
				SelectedBitmap = bitmapHandle,
				OwnsSelectedBitmap = true // Mark that this DC owns the bitmap for cleanup
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
				_env.MemWrite32(lpPaint + 16, (uint)width); // rcPaint.right
				_env.MemWrite32(lpPaint + 20, (uint)height); // rcPaint.bottom
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

				// Get the device context
				if (_deviceContexts.TryGetValue(hdc, out var dc))
				{
					// Get the bitmap that was drawn to
					if (dc.SelectedBitmap != 0 && _gdiObjects.TryGetValue(dc.SelectedBitmap, out var bitmapObj) && bitmapObj.Bitmap != null)
					{
						var bitmap = bitmapObj.Bitmap;
						if (bitmap.Bits != null && _env.Host != null)
						{
							// Calculate stride
							var bytesPerPixel = (int)(bitmap.BitCount / 8);
							if (bytesPerPixel == 0)
							{
								bytesPerPixel = 1;
							}
							var stride = ((bitmap.Width * bytesPerPixel + 3) / 4) * 4;

							// Send the bitmap to the host for display
							_logger.LogInformation("[Gdi32] EndPaint: Sending display update for HWND=0x{Hwnd:X8}, {Width}x{Height}", 
								hwnd, bitmap.Width, bitmap.Height);

							_env.Host.OnDisplayUpdate(new DisplayUpdateInfo
							{
								FrameBuffer = bitmap.Bits,
								Width = bitmap.Width,
								Height = bitmap.Height,
								Stride = stride
							});
						}

						// Clean up the bitmap if it was created by BeginPaint
						if (dc.OwnsSelectedBitmap)
						{
							_gdiObjects.Remove(dc.SelectedBitmap);
						}
					}

					// Remove the device context
					_deviceContexts.Remove(hdc);
				}
			}

			return 1; // TRUE
		}

		[DllModuleExport(1)]
		private uint FillRect(uint hdc, uint lpRect, uint hBrush)
		{
			if (lpRect != 0)
			{
				var rect = new RectRef(_env.Memory, lpRect);
				_logger.LogInformation("[Gdi32] FillRect(HDC=0x{Hdc:X8}, rect=({Left},{Top},{Right},{Bottom}), hBrush=0x{HBrush:X8})",
					hdc, rect.left, rect.top, rect.right, rect.bottom, hBrush);
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

		/// <summary>
		/// Retrieves the current text-alignment setting for the specified device context.
		/// </summary>
		/// <param name="hdc">Handle to the device context.</param>
		/// <returns>
		/// If the function succeeds, the return value is the status of the text-alignment flags.
		/// The return value is a combination of one or more of the following flags:
		/// TA_BASELINE (24), TA_BOTTOM (8), TA_TOP (0) for vertical alignment
		/// TA_CENTER (6), TA_LEFT (0), TA_RIGHT (2) for horizontal alignment
		/// TA_NOUPDATECP (0), TA_RTLREADING (256), TA_UPDATECP (1) for update flags
		/// If the function fails, the return value is GDI_ERROR.
		/// </returns>
		[DllModuleExport(4, IsStub = true)]
		private uint GetTextAlign(uint hdc)
		{
			_logger.LogInformation("[Gdi32] GetTextAlign(HDC=0x{Hdc:X8})", hdc);
			// Return default alignment (TA_LEFT | TA_TOP | TA_NOUPDATECP = 0)
			return (uint)NativeTypes.TextAlignFlags.TA_NOUPDATECP;
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

			// BitBlt is essentially StretchBlt with 1:1 scaling
			// Just call StretchBlt with the same source and destination sizes
			return StretchBlt(hdcDest, x, y, cx, cy, hdcSrc, x1, y1, cx, cy, rop);
		}

		/// <summary>
		/// Copies a bitmap from a source rectangle into a destination rectangle, stretching or compressing the bitmap as necessary.
		/// BOOL StretchBlt(
		///   [in] HDC   hdcDest,
		///   [in] int   xDest,
		///   [in] int   yDest,
		///   [in] int   wDest,
		///   [in] int   hDest,
		///   [in] HDC   hdcSrc,
		///   [in] int   xSrc,
		///   [in] int   ySrc,
		///   [in] int   wSrc,
		///   [in] int   hSrc,
		///   [in] DWORD rop
		/// );
		/// </summary>
		[DllModuleExport(44)]
		private uint StretchBlt(uint hdcDest, int xDest, int yDest, int wDest, int hDest, uint hdcSrc, int xSrc, int ySrc, int wSrc, int hSrc, uint rop)
		{
			_logger.LogInformation("[Gdi32] StretchBlt(hdcDest=0x{HdcDest:X8}, dest=({XDest},{YDest}), destSize=({WDest}x{HDest}), hdcSrc=0x{HdcSrc:X8}, src=({XSrc},{YSrc}), srcSize=({WSrc}x{HSrc}), rop=0x{Rop:X})",
				hdcDest, xDest, yDest, wDest, hDest, hdcSrc, xSrc, ySrc, wSrc, hSrc, rop);

			// StretchBlt stretches or compresses a bitmap to fit the destination rectangle
			// Common raster operation codes (rop):
			// SRCCOPY (0x00CC0020) - Copy source to destination
			// SRCPAINT (0x00EE0086) - OR source and destination
			// SRCAND (0x008800C6) - AND source and destination
			// SRCINVERT (0x00660046) - XOR source and destination
			// NOTSRCCOPY (0x00330008) - Copy inverted source to destination
			// BLACKNESS (0x00000042) - Fill destination with black
			// WHITENESS (0x00FF0062) - Fill destination with white

			// Validate destination device context
			if (!_deviceContexts.TryGetValue(hdcDest, out var destDc))
			{
				_logger.LogWarning("[Gdi32] StretchBlt: Invalid destination DC 0x{HdcDest:X8}", hdcDest);
				return 0; // FALSE
			}

			// For operations that don't require source DC (BLACKNESS, WHITENESS, etc.)
			if (rop == 0x00000042 || rop == 0x00FF0062)
			{
				// BLACKNESS or WHITENESS - no source required
				_logger.LogInformation("[Gdi32] StretchBlt: Raster operation doesn't require source DC");

				// Get destination bitmap if selected
				if (destDc.SelectedBitmap != 0 && _gdiObjects.TryGetValue(destDc.SelectedBitmap, out var destBitmapObj) && destBitmapObj.Bitmap != null)
				{
					// Fill destination rectangle with color
					var fillColor = (byte)(rop == 0x00000042 ? 0x00 : 0xFF);
					FillBitmapRect(destBitmapObj.Bitmap, xDest, yDest, wDest, hDest, fillColor);
				}

				return 1; // TRUE
			}

			// Validate source device context for operations that require it
			if (!_deviceContexts.TryGetValue(hdcSrc, out var srcDc))
			{
				_logger.LogWarning("[Gdi32] StretchBlt: Invalid source DC 0x{HdcSrc:X8}", hdcSrc);
				return 0; // FALSE
			}

			// Validate dimensions
			if (wDest <= 0 || hDest <= 0 || wSrc <= 0 || hSrc <= 0)
			{
				_logger.LogWarning("[Gdi32] StretchBlt: Invalid dimensions - dest({WDest}x{HDest}), src({WSrc}x{HSrc})",
					wDest, hDest, wSrc, hSrc);
				return 0; // FALSE
			}

			// Full implementation:
			// 1. Get the bitmap selected into the source DC
			if (srcDc.SelectedBitmap == 0 || !_gdiObjects.TryGetValue(srcDc.SelectedBitmap, out var srcObj) || srcObj.Bitmap == null)
			{
				_logger.LogInformation("[Gdi32] StretchBlt: No bitmap selected in source DC, operation is a no-op");
				return 1; // TRUE - operation succeeded but had no visible effect
			}

			// 2. Get the bitmap selected into the destination DC (if any)
			BitmapData? destBitmap = null;
			if (destDc.SelectedBitmap != 0 && _gdiObjects.TryGetValue(destDc.SelectedBitmap, out var destObj))
			{
				destBitmap = destObj.Bitmap;
			}

			if (destBitmap == null)
			{
				_logger.LogInformation("[Gdi32] StretchBlt: No destination bitmap selected, operation is a no-op");
				return 1; // TRUE - operation succeeded but had no visible effect
			}

			// 3. Scale the bitmap from source to destination size
			var srcBitmap = srcObj.Bitmap;

			// Perform the stretch blit operation
			PerformStretchBlt(srcBitmap, xSrc, ySrc, wSrc, hSrc, destBitmap, xDest, yDest, wDest, hDest, rop);

			_logger.LogInformation("[Gdi32] StretchBlt: Operation completed successfully");
			return 1; // TRUE
		}

		/// <summary>
		/// Performs the actual stretch blit operation with scaling and ROP application
		/// </summary>
		private void PerformStretchBlt(BitmapData src, int xSrc, int ySrc, int wSrc, int hSrc,
			BitmapData dest, int xDest, int yDest, int wDest, int hDest, uint rop)
		{
			if (src.Bits == null || dest.Bits == null)
			{
				return;
			}

			var srcBytesPerPixel = (int)(src.BitCount / 8);
			if (srcBytesPerPixel == 0)
			{
				srcBytesPerPixel = 1;
			}

			var srcStride = ((src.Width * srcBytesPerPixel + 3) / 4) * 4;

			var destBytesPerPixel = (int)(dest.BitCount / 8);
			if (destBytesPerPixel == 0)
			{
				destBytesPerPixel = 1;
			}

			var destStride = ((dest.Width * destBytesPerPixel + 3) / 4) * 4;

			// Use bilinear interpolation for scaling
			for (var dy = 0; dy < hDest; dy++)
			{
				for (var dx = 0; dx < wDest; dx++)
				{
					// Calculate destination pixel position
					var destX = xDest + dx;
					var destY = yDest + dy;

					// Skip if out of bounds
					if (destX < 0 || destX >= dest.Width || destY < 0 || destY >= dest.Height)
					{
						continue;
					}

					// Calculate source pixel position using nearest neighbor
					var sx = xSrc + (dx * wSrc) / wDest;
					var sy = ySrc + (dy * hSrc) / hDest;

					// Skip if source is out of bounds
					if (sx < 0 || sx >= src.Width || sy < 0 || sy >= src.Height)
					{
						continue;
					}

					// Get source pixel
					var srcOffset = sy * srcStride + sx * srcBytesPerPixel;
					var destOffset = destY * destStride + destX * destBytesPerPixel;

					// Apply raster operation
					switch (rop)
					{
						case (uint)RasterOperation.SRCCOPY:
							CopyPixel(src.Bits, srcOffset, srcBytesPerPixel, dest.Bits, destOffset, destBytesPerPixel);
							break;

						case (uint)RasterOperation.SRCPAINT:
							OrPixel(src.Bits, srcOffset, srcBytesPerPixel, dest.Bits, destOffset, destBytesPerPixel);
							break;

						case (uint)RasterOperation.SRCAND:
							AndPixel(src.Bits, srcOffset, srcBytesPerPixel, dest.Bits, destOffset, destBytesPerPixel);
							break;

						case (uint)RasterOperation.SRCINVERT:
							XorPixel(src.Bits, srcOffset, srcBytesPerPixel, dest.Bits, destOffset, destBytesPerPixel);
							break;

						case (uint)RasterOperation.NOTSRCCOPY:
							NotCopyPixel(src.Bits, srcOffset, srcBytesPerPixel, dest.Bits, destOffset, destBytesPerPixel);
							break;

						default:
							// Default to SRCCOPY for unknown operations
							CopyPixel(src.Bits, srcOffset, srcBytesPerPixel, dest.Bits, destOffset, destBytesPerPixel);
							break;
					}
				}
			}
		}

		/// <summary>
		/// Fills a rectangle in a bitmap with a solid color
		/// </summary>
		private void FillBitmapRect(BitmapData bitmap, int x, int y, int w, int h, byte fillValue)
		{
			if (bitmap.Bits == null)
			{
				return;
			}

			var bytesPerPixel = (int)(bitmap.BitCount / 8);
			if (bytesPerPixel == 0)
			{
				bytesPerPixel = 1;
			}

			var stride = ((bitmap.Width * bytesPerPixel + 3) / 4) * 4;

			for (var dy = 0; dy < h; dy++)
			{
				var py = y + dy;
				if (py < 0 || py >= bitmap.Height)
				{
					continue;
				}

				for (var dx = 0; dx < w; dx++)
				{
					var px = x + dx;
					if (px < 0 || px >= bitmap.Width)
					{
						continue;
					}

					var offset = py * stride + px * bytesPerPixel;
					for (var b = 0; b < bytesPerPixel && offset + b < bitmap.Bits.Length; b++)
					{
						bitmap.Bits[offset + b] = fillValue;
					}
				}
			}
		}

		/// <summary>
		/// Fills a rectangle in a bitmap with a COLORREF color value
		/// </summary>
		private void FillBitmapRectWithColor(BitmapData bitmap, int x, int y, int w, int h, uint color)
		{
			if (bitmap.Bits == null)
			{
				return;
			}

			var bytesPerPixel = (int)(bitmap.BitCount / 8);
			if (bytesPerPixel == 0)
			{
				bytesPerPixel = 1;
			}

			var stride = ((bitmap.Width * bytesPerPixel + 3) / 4) * 4;

			// Extract RGB components from COLORREF (0x00BBGGRR in memory, where bits 0-7: R, 8-15: G, 16-23: B)
			var r = (byte)(color & 0xFF);
			var g = (byte)((color >> 8) & 0xFF);
			var b = (byte)((color >> 16) & 0xFF);

			for (var dy = 0; dy < h; dy++)
			{
				var py = y + dy;
				if (py < 0 || py >= bitmap.Height)
				{
					continue;
				}

				for (var dx = 0; dx < w; dx++)
				{
					var px = x + dx;
					if (px < 0 || px >= bitmap.Width)
					{
						continue;
					}

					var offset = py * stride + px * bytesPerPixel;
					if (offset < bitmap.Bits.Length)
					{
						// Write color in appropriate format
						if (bytesPerPixel >= 3)
						{
							// RGB or RGBA format
							if (offset + 2 < bitmap.Bits.Length)
							{
								bitmap.Bits[offset] = b;     // Blue
								bitmap.Bits[offset + 1] = g; // Green
								bitmap.Bits[offset + 2] = r; // Red
								if (bytesPerPixel == 4 && offset + 3 < bitmap.Bits.Length)
								{
									bitmap.Bits[offset + 3] = 0xFF; // Alpha
								}
							}
						}
						else
						{
							// Grayscale - use simple average
							bitmap.Bits[offset] = (byte)((r + g + b) / 3);
						}
					}
				}
			}
		}

		/// <summary>
		/// Inverts colors in a rectangle in a bitmap
		/// </summary>
		private void InvertBitmapRect(BitmapData bitmap, int x, int y, int w, int h)
		{
			if (bitmap.Bits == null)
			{
				return;
			}

			var bytesPerPixel = (int)(bitmap.BitCount / 8);
			if (bytesPerPixel == 0)
			{
				bytesPerPixel = 1;
			}

			var stride = ((bitmap.Width * bytesPerPixel + 3) / 4) * 4;

			for (var dy = 0; dy < h; dy++)
			{
				var py = y + dy;
				if (py < 0 || py >= bitmap.Height)
				{
					continue;
				}

				for (var dx = 0; dx < w; dx++)
				{
					var px = x + dx;
					if (px < 0 || px >= bitmap.Width)
					{
						continue;
					}

					var offset = py * stride + px * bytesPerPixel;
					for (var b = 0; b < bytesPerPixel && offset + b < bitmap.Bits.Length; b++)
					{
						bitmap.Bits[offset + b] = (byte)~bitmap.Bits[offset + b];
					}
				}
			}
		}

		/// <summary>
		/// XORs a rectangle in a bitmap with a color
		/// </summary>
		private void XorBitmapRectWithColor(BitmapData bitmap, int x, int y, int w, int h, uint color)
		{
			if (bitmap.Bits == null)
			{
				return;
			}

			var bytesPerPixel = (int)(bitmap.BitCount / 8);
			if (bytesPerPixel == 0)
			{
				bytesPerPixel = 1;
			}

			var stride = ((bitmap.Width * bytesPerPixel + 3) / 4) * 4;

			// Extract RGB components from COLORREF (0x00BBGGRR in memory, where bits 0-7: R, 8-15: G, 16-23: B)
			var r = (byte)(color & 0xFF);
			var g = (byte)((color >> 8) & 0xFF);
			var b = (byte)((color >> 16) & 0xFF);

			for (var dy = 0; dy < h; dy++)
			{
				var py = y + dy;
				if (py < 0 || py >= bitmap.Height)
				{
					continue;
				}

				for (var dx = 0; dx < w; dx++)
				{
					var px = x + dx;
					if (px < 0 || px >= bitmap.Width)
					{
						continue;
					}

					var offset = py * stride + px * bytesPerPixel;
					if (offset < bitmap.Bits.Length)
					{
						// XOR color in appropriate format
						if (bytesPerPixel >= 3)
						{
							// RGB or RGBA format
							if (offset + 2 < bitmap.Bits.Length)
							{
								bitmap.Bits[offset] ^= b;     // Blue
								bitmap.Bits[offset + 1] ^= g; // Green
								bitmap.Bits[offset + 2] ^= r; // Red
							}
						}
						else
						{
							// Grayscale - XOR with average
							var avg = (byte)((r + g + b) / 3);
							bitmap.Bits[offset] ^= avg;
						}
					}
				}
			}
		}

		/// <summary>
		/// Copy pixel from source to destination
		/// </summary>
		private void CopyPixel(byte[] src, int srcOffset, int srcBpp, byte[] dest, int destOffset, int destBpp)
		{
			var bytesToCopy = Math.Min(srcBpp, destBpp);
			for (var i = 0; i < bytesToCopy && srcOffset + i < src.Length && destOffset + i < dest.Length; i++)
			{
				dest[destOffset + i] = src[srcOffset + i];
			}
		}

		/// <summary>
		/// OR pixel from source with destination
		/// </summary>
		private void OrPixel(byte[] src, int srcOffset, int srcBpp, byte[] dest, int destOffset, int destBpp)
		{
			var bytesToProcess = Math.Min(srcBpp, destBpp);
			for (var i = 0; i < bytesToProcess && srcOffset + i < src.Length && destOffset + i < dest.Length; i++)
			{
				dest[destOffset + i] |= src[srcOffset + i];
			}
		}

		/// <summary>
		/// AND pixel from source with destination
		/// </summary>
		private void AndPixel(byte[] src, int srcOffset, int srcBpp, byte[] dest, int destOffset, int destBpp)
		{
			var bytesToProcess = Math.Min(srcBpp, destBpp);
			for (var i = 0; i < bytesToProcess && srcOffset + i < src.Length && destOffset + i < dest.Length; i++)
			{
				dest[destOffset + i] &= src[srcOffset + i];
			}
		}

		/// <summary>
		/// XOR pixel from source with destination
		/// </summary>
		private void XorPixel(byte[] src, int srcOffset, int srcBpp, byte[] dest, int destOffset, int destBpp)
		{
			var bytesToProcess = Math.Min(srcBpp, destBpp);
			for (var i = 0; i < bytesToProcess && srcOffset + i < src.Length && destOffset + i < dest.Length; i++)
			{
				dest[destOffset + i] ^= src[srcOffset + i];
			}
		}

		/// <summary>
		/// Copy inverted pixel from source to destination
		/// </summary>
		private void NotCopyPixel(byte[] src, int srcOffset, int srcBpp, byte[] dest, int destOffset, int destBpp)
		{
			var bytesToCopy = Math.Min(srcBpp, destBpp);
			for (var i = 0; i < bytesToCopy && srcOffset + i < src.Length && destOffset + i < dest.Length; i++)
			{
				dest[destOffset + i] = (byte)~src[srcOffset + i];
			}
		}

		[DllModuleExport(20)]
		private uint CreateBitmap(int nWidth, int nHeight, uint nPlanes, uint nBitCount, uint lpBits)
		{
			_logger.LogInformation("[Gdi32] CreateBitmap(width={NWidth}, height={NHeight}, planes={NPlanes}, bitCount={NBitCount}, lpBits=0x{LpBits:X8})",
				nWidth, nHeight, nPlanes, nBitCount, lpBits);

			var handle = _nextGdiObjectHandle++;
			var bitmapData = new BitmapData
			{
				Width = nWidth,
				Height = nHeight,
				Planes = nPlanes,
				BitCount = nBitCount
			};

			// If bits are provided, copy them
			if (lpBits != 0 && nWidth > 0 && nHeight > 0)
			{
				var bytesPerPixel = (int)(nBitCount / 8);
				if (bytesPerPixel == 0)
				{
					bytesPerPixel = 1; // At least 1 byte per pixel
				}

				var stride = ((nWidth * bytesPerPixel + 3) / 4) * 4; // Align to 4 bytes
				var size = stride * nHeight;
				bitmapData.Bits = new byte[size];

				for (var i = 0; i < size; i++)
				{
					bitmapData.Bits[i] = _env.MemRead8(lpBits + (uint)i);
				}
			}
			else
			{
				// Create empty bitmap
				var bytesPerPixel = (int)(nBitCount / 8);
				if (bytesPerPixel == 0)
				{
					bytesPerPixel = 1;
				}

				var stride = ((nWidth * bytesPerPixel + 3) / 4) * 4;
				var size = stride * Math.Max(nHeight, 1);
				bitmapData.Bits = new byte[size];
			}

			_gdiObjects[handle] = new GdiObject { Type = GdiObjectType.Bitmap, Bitmap = bitmapData };
			return handle;
		}

		[DllModuleExport(12)]
		private uint CreateCompatibleBitmap(uint hdc, int cx, int cy)
		{
			_logger.LogInformation("[Gdi32] CreateCompatibleBitmap(hdc=0x{Hdc:X8}, cx={Cx}, cy={Cy})", hdc, cx, cy);

			var handle = _nextGdiObjectHandle++;
			var bitmapData = new BitmapData
			{
				Width = cx,
				Height = cy,
				Planes = 1,
				BitCount = 32 // Default to 32-bit RGBA
			};

			// Create empty bitmap buffer
			var bytesPerPixel = 4; // 32-bit
			var stride = ((cx * bytesPerPixel + 3) / 4) * 4; // Align to 4 bytes
			var size = stride * Math.Max(cy, 1);
			bitmapData.Bits = new byte[size];

			_gdiObjects[handle] = new GdiObject { Type = GdiObjectType.Bitmap, Bitmap = bitmapData };
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

			// Track object selection in DC
			if (_deviceContexts.TryGetValue(hdc, out var dc) && _gdiObjects.TryGetValue(hObject, out var obj))
			{
				switch (obj.Type)
				{
					case GdiObjectType.Bitmap:
					{
						var previousBitmap = dc.SelectedBitmap;
						dc.SelectedBitmap = hObject;
						_logger.LogInformation("[Gdi32] SelectObject: Selected bitmap 0x{HObject:X8} into DC 0x{Hdc:X8}", hObject, hdc);
						return previousBitmap; // Return previous bitmap
					}
					case GdiObjectType.Brush:
					{
						var previousBrush = dc.SelectedBrush;
						dc.SelectedBrush = hObject;
						_logger.LogInformation("[Gdi32] SelectObject: Selected brush 0x{HObject:X8} into DC 0x{Hdc:X8}", hObject, hdc);
						return previousBrush; // Return previous brush
					}
					case GdiObjectType.Pen:
					{
						var previousPen = dc.SelectedPen;
						dc.SelectedPen = hObject;
						_logger.LogInformation("[Gdi32] SelectObject: Selected pen 0x{HObject:X8} into DC 0x{Hdc:X8}", hObject, hdc);
						return previousPen; // Return previous pen
					}
				}
			}

			return hObject; // Return previous object (stub for non-tracked objects)
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
				_env.MemWrite32(psizl, (uint)(c * DefaultCharWidth)); // cx
				_env.MemWrite32(psizl + 4, DefaultFontHeight); // cy
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
			_gdiObjects[handle] = new GdiObject { Type = GdiObjectType.Brush, BrushColor = color };
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

			if (lpdi != 0)
			{
				var docInfo = new DocInfoARef(_env.Memory, lpdi);

				if (docInfo.lpszDocName != 0)
				{
					var docName = _env.ReadAnsiString(docInfo.lpszDocName);
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

			// Validate device context
			if (!_deviceContexts.TryGetValue(hdc, out var dc))
			{
				_logger.LogWarning("[Gdi32] PatBlt: Invalid DC 0x{Hdc:X8}", hdc);
				return 0; // FALSE
			}

			// Get destination bitmap if selected
			BitmapData? destBitmap = null;
			if (dc.SelectedBitmap != 0 && _gdiObjects.TryGetValue(dc.SelectedBitmap, out var destObj))
			{
				destBitmap = destObj.Bitmap;
			}

			if (destBitmap == null)
			{
				_logger.LogInformation("[Gdi32] PatBlt: No destination bitmap selected, operation is a no-op");
				return 1; // TRUE - operation succeeded but had no visible effect
			}

			// Get the selected brush color (used by pattern operations)
			var brushColor = dc.BkColor; // Default to background color
			if (dc.SelectedBrush != 0 && _gdiObjects.TryGetValue(dc.SelectedBrush, out var brushObj))
			{
				brushColor = brushObj.BrushColor;
			}

			// Handle different raster operations
			switch (rop)
			{
				case (uint)RasterOperation.BLACKNESS:
					FillBitmapRect(destBitmap, x, y, w, h, 0x00);
					break;

				case (uint)RasterOperation.WHITENESS:
					FillBitmapRect(destBitmap, x, y, w, h, 0xFF);
					break;

				case (uint)RasterOperation.PATCOPY:
					FillBitmapRectWithColor(destBitmap, x, y, w, h, brushColor);
					break;

				case (uint)RasterOperation.DSTINVERT:
					InvertBitmapRect(destBitmap, x, y, w, h);
					break;

				case (uint)RasterOperation.PATINVERT:
					XorBitmapRectWithColor(destBitmap, x, y, w, h, brushColor);
					break;

				default:
					_logger.LogWarning("[Gdi32] PatBlt: Unsupported ROP 0x{Rop:X8}, using PATCOPY as fallback", rop);
					// Default to PATCOPY
					FillBitmapRectWithColor(destBitmap, x, y, w, h, brushColor);
					break;
			}

			_logger.LogInformation("[Gdi32] PatBlt: Operation completed successfully");
			return 1; // TRUE
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

			// Stub: Return a default size
			if (lpsz != 0)
			{
				var width = c * DefaultCharWidth;
				var height = DefaultFontHeight;
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
		/// <remarks>
		/// This is the Unicode version of DrawText. It is intentionally stubbed as most
		/// Win32 applications use the ANSI version (DrawTextA). If Unicode support is needed,
		/// this function should be implemented similar to DrawTextA.
		/// </remarks>
		[DllModuleExport(20, IsStub = true)]
		private int DrawText(uint hdc, uint lpchText, int cchText, uint lprc, uint format)
		{
			return 0;
		}

		/// <summary>
		/// Draws formatted text in the specified rectangle (ANSI version).
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
				var rect = new RectRef(_env.Memory, lprc);
				left = rect.left;
				top = rect.top;
				right = rect.right;
				bottom = rect.bottom;
			}

			_logger.LogInformation("[Gdi32] DrawTextA(hdc=0x{Hdc:X8}, text=\"{Text}\", rect=({Left},{Top},{Right},{Bottom}), format=0x{Format:X})",
				hdc, text, left, top, right, bottom, format);

			// Calculate text height (stub implementation)
			// DT_CALCRECT (0x400) means calculate the rectangle needed
			if ((format & 0x400) != 0 && lprc != 0)
			{
				// Update rectangle with calculated size
				var textLength = cchText < 0 ? text.Length : Math.Min(cchText, text.Length);
				var rect = new RectRef(_env.Memory, lprc);
				rect.right = rect.left + textLength * DefaultCharWidth;
				rect.bottom = rect.top + DefaultFontHeight;
			}

			// Return height of text drawn
			return DefaultFontHeight;
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
				var rect = new RectRef(_env.Memory, lprc);
				_logger.LogInformation("[Gdi32] FrameRect(hdc=0x{Hdc:X8}, rect=({Left},{Top},{Right},{Bottom}), hbr=0x{Hbr:X8})",
					hdc, rect.left, rect.top, rect.right, rect.bottom, hbr);
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
				var rect = new RectRef(_env.Memory, lprc);
				_logger.LogInformation("[Gdi32] InvertRect(hdc=0x{Hdc:X8}, rect=({Left},{Top},{Right},{Bottom}))",
					hdc, rect.left, rect.top, rect.right, rect.bottom);
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
			public BitmapData? Bitmap { get; set; }
			public uint BrushColor { get; set; } = 0x00000000; // For solid brushes
		}

		private class BitmapData
		{
			public int Width { get; set; }
			public int Height { get; set; }
			public uint Planes { get; set; }
			public uint BitCount { get; set; }
			public byte[]? Bits { get; set; }
		}

		private class DeviceContext
		{
			public uint Handle { get; set; }
			public uint WindowHandle { get; set; }
			public int BkMode { get; set; } = 2; // OPAQUE
			public uint TextColor { get; set; } = 0x00000000; // Black
			public uint BkColor { get; set; } = 0x00FFFFFF; // White background
			public uint SelectedBitmap { get; set; } = 0; // Currently selected bitmap
			public uint SelectedBrush { get; set; } = 0; // Currently selected brush
			public uint SelectedPen { get; set; } = 0; // Currently selected pen
			public bool OwnsSelectedBitmap { get; set; } = false; // True if bitmap was created by BeginPaint
			public bool IsInfoContext { get; set; } = false; // True if this is an information context (IC) rather than a device context (DC)
		}

		/// <summary>
		/// Stops the current print job and erases everything drawn since the last call to StartDoc.
		/// </summary>
		[DllModuleExport(4, IsStub = true)]
		private uint AbortDoc(uint hdc)
		{
			_logger.LogInformation("[Gdi32] AbortDoc(hdc=0x{Hdc:X8})", hdc);
			return 1; // Success
		}

		/// <summary>
		/// Creates a logical brush with the specified bitmap pattern.
		/// </summary>
		[DllModuleExport(4, IsStub = true)]
		private uint CreatePatternBrush(uint hbmp)
		{
			_logger.LogInformation("[Gdi32] CreatePatternBrush(hbmp=0x{Hbmp:X8})", hbmp);
			return _nextGdiObjectHandle++; // Return unique brush handle
		}

		/// <summary>
		/// Creates a new clipping region by excluding the specified rectangle.
		/// </summary>
		[DllModuleExport(20)]
		private uint ExcludeClipRect(uint hdc, int left, int top, int right, int bottom)
		{
			_logger.LogInformation("[Gdi32] ExcludeClipRect(hdc=0x{Hdc:X8}, left={Left}, top={Top}, right={Right}, bottom={Bottom})",
			hdc, left, top, right, bottom);
			return 1; // SIMPLEREGION
		}

		/// <summary>
		/// Selects a region as the current clipping region for the specified device context.
		/// </summary>
		[DllModuleExport(8)]
		private uint SelectClipRgn(uint hdc, uint hrgn)
		{
			_logger.LogInformation("[Gdi32] SelectClipRgn(hdc=0x{Hdc:X8}, hrgn=0x{Hrgn:X8})", hdc, hrgn);
			return hrgn == 0 ? 1u : 2u; // NULLREGION (1) if null, SIMPLEREGION (2) if non-null
		}

		/// <summary>
		/// Sets the application-defined abort function that allows a print job to be cancelled during printing.
		/// </summary>
		[DllModuleExport(8, IsStub = true)]
		private uint SetAbortProc(uint hdc, uint lpAbortProc)
		{
			_logger.LogInformation("[Gdi32] SetAbortProc(hdc=0x{Hdc:X8}, lpAbortProc=0x{LpAbortProc:X8})", hdc, lpAbortProc);
			return 1; // Success
		}

		/// <summary>
		/// Converts an enhanced metafile into a Windows-format metafile.
		/// UINT GetWinMetaFileBits(
		///   [in]  HENHMETAFILE hemf,
		///   [in]  UINT         cbData16,
		///   [out] LPBYTE       pData16,
		///   [in]  INT          iMapMode,
		///   [in]  HDC          hdcRef
		/// );
		/// </summary>
		[DllModuleExport(20, IsStub = true)]
		private uint GetWinMetaFileBits(uint hemf, uint cbData16, uint pData16, int iMapMode, uint hdcRef)
		{
			_logger.LogInformation("[Gdi32] GetWinMetaFileBits(hemf=0x{Hemf:X8}, cbData16={CbData16}, pData16=0x{PData16:X8}, iMapMode={IMapMode}, hdcRef=0x{HdcRef:X8})",
				hemf, cbData16, pData16, iMapMode, hdcRef);
			// Stub: Return 0 to indicate failure (metafiles not supported)
			return 0;
		}

		/// <summary>
		/// Creates a memory-based metafile from metafile data.
		/// HMETAFILE SetMetaFileBitsEx(
		///   [in] UINT    cbBuffer,
		///   [in] const BYTE *lpData
		/// );
		/// </summary>
		[DllModuleExport(8, IsStub = true)]
		private uint SetMetaFileBitsEx(uint cbBuffer, uint lpData)
		{
			_logger.LogInformation("[Gdi32] SetMetaFileBitsEx(cbBuffer={CbBuffer}, lpData=0x{LpData:X8})",
				cbBuffer, lpData);
			// Stub: Return NULL handle (metafiles not supported)
			return 0;
		}

		/// <summary>
		/// Converts a metafile from the older Windows format to the enhanced format.
		/// HENHMETAFILE SetWinMetaFileBits(
		///   [in] UINT         nSize,
		///   [in] const BYTE   *lpMeta16Data,
		///   [in] HDC          hdcRef,
		///   [in] const METAFILEPICT *lpMFP
		/// );
		/// </summary>
		[DllModuleExport(16, IsStub = true)]
		private uint SetWinMetaFileBits(uint nSize, uint lpMeta16Data, uint hdcRef, uint lpMFP)
		{
			_logger.LogInformation("[Gdi32] SetWinMetaFileBits(nSize={NSize}, lpMeta16Data=0x{LpMeta16Data:X8}, hdcRef=0x{HdcRef:X8}, lpMFP=0x{LpMFP:X8})",
				nSize, lpMeta16Data, hdcRef, lpMFP);
			// Stub: Return NULL handle (metafiles not supported)
			return 0;
		}

		/// <summary>
		/// Displays an enhanced metafile by playing its records.
		/// BOOL PlayEnhMetaFile(
		///   [in] HDC          hdc,
		///   [in] HENHMETAFILE hemf,
		///   [in] const RECT   *lprect
		/// );
		/// </summary>
		[DllModuleExport(12, IsStub = true)]
		private uint PlayEnhMetaFile(uint hdc, uint hemf, uint lprect)
		{
			_logger.LogInformation("[Gdi32] PlayEnhMetaFile(hdc=0x{Hdc:X8}, hemf=0x{Hemf:X8}, lprect=0x{Lprect:X8})",
				hdc, hemf, lprect);
			// Stub: Return FALSE (metafiles not supported)
			return 0;
		}

		/// <summary>
		/// Translates character set information and sets it in a CHARSETINFO structure.
		/// BOOL TranslateCharsetInfo(
		///   [in, out] DWORD       *lpSrc,
		///   [out]     LPCHARSETINFO lpCs,
		///   [in]      DWORD       dwFlags
		/// );
		/// </summary>
		[DllModuleExport(12)]
		private uint TranslateCharsetInfo(uint lpSrc, uint lpCs, uint dwFlags)
		{
			_logger.LogInformation("[Gdi32] TranslateCharsetInfo(lpSrc=0x{LpSrc:X8}, lpCs=0x{LpCs:X8}, dwFlags=0x{DwFlags:X8})",
				lpSrc, lpCs, dwFlags);

			const uint TCI_SRCCHARSET = 1;
			const uint TCI_SRCCODEPAGE = 2;
			const uint TCI_SRCFONTSIG = 3;

			// Default ANSI charset information
			const uint ANSI_CHARSET = 0;
			const uint ANSI_CODEPAGE = 1252;

			if (lpCs == 0)
			{
				return 0; // FALSE
			}

			uint charset = ANSI_CHARSET;
			uint codepage = ANSI_CODEPAGE;

			// Determine charset based on flags
			if (dwFlags == TCI_SRCCHARSET)
			{
				// lpSrc is a charset value
				charset = _env.MemRead32(lpSrc);
				// Map common charsets to codepages
				codepage = charset switch
				{
					0 => 1252,    // ANSI_CHARSET
					128 => 932,   // SHIFTJIS_CHARSET
					129 => 949,   // HANGUL_CHARSET
					130 => 1361,  // JOHAB_CHARSET
					134 => 936,   // GB2312_CHARSET
					136 => 950,   // CHINESEBIG5_CHARSET
					161 => 1253,  // GREEK_CHARSET
					162 => 1254,  // TURKISH_CHARSET
					163 => 1258,  // VIETNAMESE_CHARSET
					177 => 1255,  // HEBREW_CHARSET
					178 => 1256,  // ARABIC_CHARSET
					186 => 1257,  // BALTIC_CHARSET
					204 => 1251,  // RUSSIAN_CHARSET
					238 => 1250,  // EASTEUROPE_CHARSET
					_ => 1252     // Default to ANSI_CODEPAGE
				};
			}
			else if (dwFlags == TCI_SRCCODEPAGE)
			{
				// lpSrc is a codepage value
				codepage = _env.MemRead32(lpSrc);
				// Map common codepages to charsets
				charset = codepage switch
				{
					1252 => 0,   // ANSI_CHARSET
					932 => 128,  // SHIFTJIS_CHARSET
					936 => 134,  // GB2312_CHARSET
					949 => 129,  // HANGUL_CHARSET
					950 => 136,  // CHINESEBIG5_CHARSET
					1361 => 130, // JOHAB_CHARSET
					1250 => 238, // EASTEUROPE_CHARSET
					1251 => 204, // RUSSIAN_CHARSET
					1253 => 161, // GREEK_CHARSET
					1254 => 162, // TURKISH_CHARSET
					1255 => 177, // HEBREW_CHARSET
					1256 => 178, // ARABIC_CHARSET
					1257 => 186, // BALTIC_CHARSET
					1258 => 163, // VIETNAMESE_CHARSET
					_ => 0       // Default to ANSI_CHARSET
				};
			}
			else if (dwFlags == TCI_SRCFONTSIG)
			{
				// lpSrc points to a FONTSIGNATURE structure - not implemented
				_logger.LogWarning("[Gdi32] TranslateCharsetInfo: TCI_SRCFONTSIG not fully implemented");
				charset = ANSI_CHARSET;
			}

			// Fill CHARSETINFO structure
			// typedef struct tagCHARSETINFO {
			//   UINT      ciCharset;   // +0
			//   UINT      ciACP;       // +4
			//   FONTSIGNATURE fs;      // +8 (24 bytes)
			// } CHARSETINFO;
			_env.MemWrite32(lpCs, charset);     // ciCharset
			_env.MemWrite32(lpCs + 4, codepage); // ciACP
			// fs (FONTSIGNATURE) - stub: zero out
			for (var i = 0; i < 24; i += 4)
			{
				_env.MemWrite32(lpCs + 8 + (uint)i, 0);
			}

			return 1; // TRUE
		}

		/// <summary>
		/// Sets the bitmap stretching mode in the specified device context.
		/// </summary>
		[DllModuleExport(8, IsStub = true)]
		private int SetStretchBltMode(uint hdc, int stretchMode)
		{
			_logger.LogInformation("[Gdi32] SetStretchBltMode(hdc=0x{Hdc:X8}, stretchMode={StretchMode})", hdc, stretchMode);
			// Return previous mode (stub: assume COLORONCOLOR = 3)
			return 3;
		}

		/// <summary>
		/// Copies DIB color data from a source rectangle to a destination rectangle, stretching or compressing as needed.
		/// </summary>
		[DllModuleExport(48, IsStub = true)]
		private int StretchDIBits(uint hdc, int xDest, int yDest, int destWidth, int destHeight,
			int xSrc, int ySrc, int srcWidth, int srcHeight, uint lpBits, uint lpbmi, uint usage)
		{
			_logger.LogInformation("[Gdi32] StretchDIBits(hdc=0x{Hdc:X8}, xDest={XDest}, yDest={YDest}, destWidth={DestWidth}, destHeight={DestHeight}, xSrc={XSrc}, ySrc={YSrc}, srcWidth={SrcWidth}, srcHeight={SrcHeight}, lpBits=0x{LpBits:X8}, lpbmi=0x{Lpbmi:X8}, usage={Usage})",
				hdc, xDest, yDest, destWidth, destHeight, xSrc, ySrc, srcWidth, srcHeight, lpBits, lpbmi, usage);
			// Stub: return success
			return srcHeight;
		}

		/// <summary>
		/// Associates an owner process with a GDI object (16-bit compatibility function).
		/// </summary>
		[DllModuleExport(8, IsStub = true)]
		private void SetObjectOwner(uint hGdiObj, uint hOwner)
		{
			_logger.LogInformation("[Gdi32] SetObjectOwner(hGdiObj=0x{HGdiObj:X8}, hOwner=0x{HOwner:X8}) - 16-bit compatibility, no-op", hGdiObj, hOwner);
			// This function exists for 16-bit compatibility and is typically a no-op in Win32
		}

		/// <summary>
		/// Retrieves the type of the specified object.
		/// </summary>
		[DllModuleExport(4, IsStub = true)]
		private uint GetObjectType(uint h)
		{
			_logger.LogInformation("[Gdi32] GetObjectType(h=0x{H:X8})", h);
			
			// Check if it's a known GDI object
			if (_gdiObjects.TryGetValue(h, out var obj))
			{
				// Return appropriate type based on object
				return obj.Type switch
				{
					GdiObjectType.Pen => (uint)GdiObjectTypeId.OBJ_PEN,
					GdiObjectType.Brush => (uint)GdiObjectTypeId.OBJ_BRUSH,
					GdiObjectType.Font => (uint)GdiObjectTypeId.OBJ_FONT,
					GdiObjectType.Bitmap => (uint)GdiObjectTypeId.OBJ_BITMAP,
					GdiObjectType.Palette => (uint)GdiObjectTypeId.OBJ_PAL,
					GdiObjectType.Region => (uint)GdiObjectTypeId.OBJ_REGION,
					_ => 0
				};
			}
			
			// Check if it's a DC
			if (_deviceContexts.ContainsKey(h))
			{
				return (uint)GdiObjectTypeId.OBJ_DC;
			}
			
			// Unknown object
			return 0;
		}

		/// <summary>
		/// Retrieves the color value of the color that is closest to the specified color value.
		/// </summary>
		[DllModuleExport(8, IsStub = true)]
		private uint GetNearestColor(uint hdc, uint color)
		{
			_logger.LogInformation("[Gdi32] GetNearestColor(hdc=0x{Hdc:X8}, color=0x{Color:X8})", hdc, color);
			// Stub: return the same color (assumes true color display)
			return color;
		}

		/// <summary>
		/// Increases or decreases the size of a logical palette.
		/// </summary>
		[DllModuleExport(8, IsStub = true)]
		private uint ResizePalette(uint hPalette, uint nEntries)
		{
			_logger.LogInformation("[Gdi32] ResizePalette(hPalette=0x{HPalette:X8}, nEntries={NEntries})", hPalette, nEntries);
			// Stub: return success
			return 1;
		}

		/// <summary>
		/// Allows applications to access device capabilities that are not available through GDI.
		/// </summary>
		[DllModuleExport(24, IsStub = true)]
		private int ExtEscape(uint hdc, int escape, int cbInput, uint lpInData, int cbOutput, uint lpOutData)
		{
			_logger.LogInformation("[Gdi32] ExtEscape(hdc=0x{Hdc:X8}, escape={Escape}, cbInput={CbInput}, lpInData=0x{LpInData:X8}, cbOutput={CbOutput}, lpOutData=0x{LpOutData:X8})",
				hdc, escape, cbInput, lpInData, cbOutput, lpOutData);
			// Stub: return error (escape not supported)
			return 0;
		}

		/// <summary>
		/// Gets the gamma ramp for the display device context.
		/// </summary>
		[DllModuleExport(8, IsStub = true)]
		private uint GetDeviceGammaRamp(uint hdc, uint lpRamp)
		{
			_logger.LogInformation("[Gdi32] GetDeviceGammaRamp(hdc=0x{Hdc:X8}, lpRamp=0x{LpRamp:X8})", hdc, lpRamp);
			
			// Gamma ramp is an array of 3 * 256 WORD values (R, G, B ramps)
			// Each ramp is 256 WORDs (512 bytes), total = 1536 bytes
			// Stub: Set linear gamma ramp (identity mapping)
			if (lpRamp != 0)
			{
				for (uint i = 0; i < 256; i++)
				{
					// Linear ramp: 0->0, 255->65535 (255 * 257 = 65535 fits in ushort)
					var value = (ushort)(i * 257);
					// Red ramp
					_env.MemWrite16(lpRamp + i * 2, value);
					// Green ramp
					_env.MemWrite16(lpRamp + 512 + i * 2, value);
					// Blue ramp
					_env.MemWrite16(lpRamp + 1024 + i * 2, value);
				}
			}
			
			return 1; // Success
		}

		/// <summary>
		/// Sets the gamma ramp for the display device context.
		/// </summary>
		[DllModuleExport(8, IsStub = true)]
		private uint SetDeviceGammaRamp(uint hdc, uint lpRamp)
		{
			_logger.LogInformation("[Gdi32] SetDeviceGammaRamp(hdc=0x{Hdc:X8}, lpRamp=0x{LpRamp:X8})", hdc, lpRamp);
			// Stub: return success (don't actually apply gamma)
			return 1;
		}

		/// <summary>
		/// Sets the system palette use for the specified device context.
		/// </summary>
		[DllModuleExport(8, IsStub = true)]
		private uint SetSystemPaletteUse(uint hdc, uint use)
		{
			_logger.LogInformation("[Gdi32] SetSystemPaletteUse(hdc=0x{Hdc:X8}, use={Use})", hdc, use);
			// SYSPAL_NOSTATIC = 2, SYSPAL_STATIC = 1, SYSPAL_ERROR = 0
			// Stub: return previous value (assume SYSPAL_STATIC)
			return 1;
		}

		/// <summary>
		/// Enumerates all uniquely-named fonts in the system that match the font characteristics specified.
		/// int EnumFontFamiliesExA(
		///   [in] HDC           hdc,
		///   [in] LPLOGFONTA    lpLogfont,
		///   [in] FONTENUMPROCA lpProc,
		///   [in] LPARAM        lParam,
		///   [in] DWORD         dwFlags
		/// );
		/// Returns: The last value returned by the callback function. If no fonts match, returns 1.
		/// </summary>
		[DllModuleExport(20, IsStub = true)]
		private uint EnumFontFamiliesExA(uint hdc, uint lpLogfont, uint lpProc, uint lParam, uint dwFlags)
		{
			_logger.LogInformation("[Gdi32] EnumFontFamiliesExA(hdc=0x{Hdc:X8}, lpLogfont=0x{LpLogfont:X8}, lpProc=0x{LpProc:X8}, lParam=0x{LParam:X8}, dwFlags=0x{DwFlags:X8})",
				hdc, lpLogfont, lpProc, lParam, dwFlags);
			// Stub implementation: return 1 (no fonts enumerated)
			return 1;
		}

		/// <summary>
		/// Combines two regions and stores the result in a destination region.
		/// int CombineRgn(
		///   [in] HRGN hrgnDst,
		///   [in] HRGN hrgnSrc1,
		///   [in] HRGN hrgnSrc2,
		///   [in] int  iMode
		/// );
		/// </summary>
		[DllModuleExport(12, IsStub = true)]
		private int CombineRgn(uint hrgnDst, uint hrgnSrc1, uint hrgnSrc2, int iMode)
		{
			_logger.LogInformation("[Gdi32] CombineRgn(hrgnDst=0x{HrgnDst:X8}, hrgnSrc1=0x{HrgnSrc1:X8}, hrgnSrc2=0x{HrgnSrc2:X8}, iMode={IMode})",
				hrgnDst, hrgnSrc1, hrgnSrc2, iMode);
			// Return SIMPLEREGION - stub assumes result is a simple region
			return (int)NativeTypes.RegionComplexity.SIMPLEREGION;
		}

		/// <summary>
		/// Fills a region by using the specified brush.
		/// BOOL FillRgn(
		///   [in] HDC   hdc,
		///   [in] HRGN  hrgn,
		///   [in] HBRUSH hbr
		/// );
		/// </summary>
		[DllModuleExport(4, IsStub = true)]
		private uint FillRgn(uint hdc, uint hrgn, uint hbr)
		{
			_logger.LogInformation("[Gdi32] FillRgn(hdc=0x{Hdc:X8}, hrgn=0x{Hrgn:X8}, hbr=0x{Hbr:X8})",
				hdc, hrgn, hbr);
			// Return success (1) - stub implementation
			return 1;
		}

		/// <summary>
		/// Creates an information context for the specified device (ANSI version).
		/// HDC CreateICA(
		///   [in] LPCSTR lpszDriver,
		///   [in] LPCSTR lpszDevice,
		///   [in] LPCSTR lpszOutput,
		///   [in] const DEVMODEA *lpdvmInit
		/// );
		/// </summary>
		[DllModuleExport(20)]
		private uint CreateICA(in LpcStr lpszDriver, in LpcStr lpszDevice, in LpcStr lpszOutput, uint lpdvmInit)
		{
			var driver = lpszDriver.ToString() ?? string.Empty;
			var device = lpszDevice.ToString() ?? string.Empty;
			var output = lpszOutput.ToString() ?? string.Empty;
			
			_logger.LogInformation("[Gdi32] CreateICA(lpszDriver=\"{Driver}\", lpszDevice=\"{Device}\", lpszOutput=\"{Output}\", lpdvmInit=0x{LpdvmInit:X8})",
				driver, device, output, lpdvmInit);

			// Create a new information context handle
			var handle = _nextDcHandle++;
			_deviceContexts[handle] = new DeviceContext
			{
				IsInfoContext = true
			};

			return handle;
		}

		/// <summary>
		/// Creates an information context for the specified device (Unicode version).
		/// HDC CreateICW(
		///   [in] LPCWSTR lpszDriver,
		///   [in] LPCWSTR lpszDevice,
		///   [in] LPCWSTR lpszOutput,
		///   [in] const DEVMODEW *lpdvmInit
		/// );
		/// </summary>
		[DllModuleExport(20)]
		private uint CreateICW(in LpcWStr lpszDriver, in LpcWStr lpszDevice, in LpcWStr lpszOutput, uint lpdvmInit)
		{
			var driver = lpszDriver.ToString() ?? string.Empty;
			var device = lpszDevice.ToString() ?? string.Empty;
			var output = lpszOutput.ToString() ?? string.Empty;
			
			_logger.LogInformation("[Gdi32] CreateICW(lpszDriver=\"{Driver}\", lpszDevice=\"{Device}\", lpszOutput=\"{Output}\", lpdvmInit=0x{LpdvmInit:X8})",
				driver, device, output, lpdvmInit);

			// Create a new information context handle
			var handle = _nextDcHandle++;
			_deviceContexts[handle] = new DeviceContext
			{
				IsInfoContext = true
			};

			return handle;
		}

		/// <summary>
		/// Creates a device context for a device (Unicode version).
		/// HDC CreateDCW(
		///   [in] LPCWSTR lpszDriver,
		///   [in] LPCWSTR lpszDevice,
		///   [in] LPCWSTR lpszOutput,
		///   [in] const DEVMODEW *lpdvmInit
		/// );
		/// </summary>
		[DllModuleExport(20)]
		private uint CreateDCW(in LpcWStr lpszDriver, in LpcWStr lpszDevice, in LpcWStr lpszOutput, uint lpdvmInit)
		{
			var driver = lpszDriver.ToString() ?? string.Empty;
			var device = lpszDevice.ToString() ?? string.Empty;
			var output = lpszOutput.ToString() ?? string.Empty;
			
			_logger.LogInformation("[Gdi32] CreateDCW(lpszDriver=\"{Driver}\", lpszDevice=\"{Device}\", lpszOutput=\"{Output}\", lpdvmInit=0x{LpdvmInit:X8})",
				driver, device, output, lpdvmInit);

			// Create a new device context handle
			var handle = _nextDcHandle++;
			_deviceContexts[handle] = new DeviceContext();

			return handle;
		}

		/// <summary>
		/// Creates a logical font from a LOGFONTW structure (Unicode version).
		/// HFONT CreateFontIndirectW(
		///   [in] const LOGFONTW *lplf
		/// );
		/// </summary>
		[DllModuleExport(4)]
		private uint CreateFontIndirectW(uint lplf)
		{
			_logger.LogInformation("[Gdi32] CreateFontIndirectW(lplf=0x{Lplf:X8})", lplf);

			// Create a new font handle
			var fontHandle = _nextGdiObjectHandle++;
			_gdiObjects[fontHandle] = new GdiObject
			{
				Type = GdiObjectType.Font
			};

			return fontHandle;
		}

		// Region functions
		[DllModuleExport(4)]
		private uint CreateRectRgnIndirect(uint lprc)
		{
			_logger.LogInformation("[Gdi32] CreateRectRgnIndirect(lprc=0x{Lprc:X8})", lprc);
			
			if (lprc == 0)
			{
				return 0; // NULL pointer
			}
			
			// Read RECT structure (left, top, right, bottom - each 4 bytes)
			var left = (int)_env.MemRead32(lprc);
			var top = (int)_env.MemRead32(lprc + 4);
			var right = (int)_env.MemRead32(lprc + 8);
			var bottom = (int)_env.MemRead32(lprc + 12);
			
			// Create a region handle
			var regionHandle = _nextGdiObjectHandle++;
			_gdiObjects[regionHandle] = new GdiObject { Type = GdiObjectType.Region };
			
			_logger.LogInformation("[Gdi32] CreateRectRgnIndirect -> 0x{Handle:X8} ({Left},{Top})-({Right},{Bottom})",
				regionHandle, left, top, right, bottom);
			return regionHandle;
		}

		[DllModuleExport(8, IsStub = true)]
		private uint EqualRgn(uint hSrcRgn1, uint hSrcRgn2)
		{
			_logger.LogInformation("[Gdi32] EqualRgn(hSrcRgn1=0x{HSrcRgn1:X8}, hSrcRgn2=0x{HSrcRgn2:X8})",
				hSrcRgn1, hSrcRgn2);
			
			// Stub: return FALSE (regions are not equal)
			return 0;
		}

		[DllModuleExport(8, IsStub = true)]
		private int GetClipRgn(uint hdc, uint hrgn)
		{
			_logger.LogInformation("[Gdi32] GetClipRgn(hdc=0x{Hdc:X8}, hrgn=0x{Hrgn:X8})", hdc, hrgn);
			
			// Stub: return 0 (no clipping region)
			return 0;
		}

		[DllModuleExport(12, IsStub = true)]
		private int GetRandomRgn(uint hdc, uint hrgn, int iNum)
		{
			_logger.LogInformation("[Gdi32] GetRandomRgn(hdc=0x{Hdc:X8}, hrgn=0x{Hrgn:X8}, iNum={INum})",
				hdc, hrgn, iNum);
			
			// Stub: return -1 (error)
			return -1;
		}

		[DllModuleExport(8, IsStub = true)]
		private int GetRgnBox(uint hrgn, uint lprc)
		{
			_logger.LogInformation("[Gdi32] GetRgnBox(hrgn=0x{Hrgn:X8}, lprc=0x{Lprc:X8})", hrgn, lprc);
			
			// Stub: return NULLREGION (1)
			if (lprc != 0)
			{
				// Write empty rectangle (all zeros)
				_env.MemWrite32(lprc, 0);     // left
				_env.MemWrite32(lprc + 4, 0); // top
				_env.MemWrite32(lprc + 8, 0); // right
				_env.MemWrite32(lprc + 12, 0); // bottom
			}
			return 1; // NULLREGION
		}

		[DllModuleExport(20, IsStub = true)]
		private uint SetRectRgn(uint hrgn, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect)
		{
			_logger.LogInformation("[Gdi32] SetRectRgn(hrgn=0x{Hrgn:X8}, left={Left}, top={Top}, right={Right}, bottom={Bottom})",
				hrgn, nLeftRect, nTopRect, nRightRect, nBottomRect);
			
			// Stub: return TRUE
			return 1;
		}

		[DllModuleExport(4, IsStub = true)]
		private int GetROP2(uint hdc)
		{
			_logger.LogInformation("[Gdi32] GetROP2(hdc=0x{Hdc:X8})", hdc);
			
			// Stub: return R2_COPYPEN (13) - default ROP2 mode
			return 13;
		}

		[DllModuleExport(8, IsStub = true)]
		private int SetROP2(uint hdc, int fnDrawMode)
		{
			_logger.LogInformation("[Gdi32] SetROP2(hdc=0x{Hdc:X8}, fnDrawMode={FnDrawMode})", hdc, fnDrawMode);
			
			// Stub: return previous mode (R2_COPYPEN)
			return 13;
		}

		[DllModuleExport(16, IsStub = true)]
		private uint SetPixelV(uint hdc, int x, int y, uint crColor)
		{
			_logger.LogInformation("[Gdi32] SetPixelV(hdc=0x{Hdc:X8}, x={X}, y={Y}, crColor=0x{CrColor:X8})",
				hdc, x, y, crColor);
			
			// Stub: return TRUE
			return 1;
		}

		[DllModuleExport(12, IsStub = true)]
		private int GetObjectW(uint hgdiobj, int cbBuffer, uint lpvObject)
		{
			_logger.LogInformation("[Gdi32] GetObjectW(hgdiobj=0x{Hgdiobj:X8}, cbBuffer={CbBuffer}, lpvObject=0x{LpvObject:X8})",
				hgdiobj, cbBuffer, lpvObject);
			
			// Stub: return 0 (failure)
			return 0;
		}

		[DllModuleExport(16, IsStub = true)]
		private uint EnumFontFamiliesA(uint hdc, uint lpszFamily, uint lpEnumFontFamProc, uint lParam)
		{
			_logger.LogInformation("[Gdi32] EnumFontFamiliesA(hdc=0x{Hdc:X8}, lpszFamily=0x{LpszFamily:X8}, lpEnumFontFamProc=0x{LpEnumFontFamProc:X8}, lParam=0x{LParam:X8})",
				hdc, lpszFamily, lpEnumFontFamProc, lParam);
			
			// Stub: return 1 (success, but no fonts enumerated)
			return 1;
		}

		[DllModuleExport(12, IsStub = true)]
		private int GdiGetCharDimensions(uint hdc, uint lptm, uint lpAvgCharWidth)
		{
			_logger.LogInformation("[Gdi32] GdiGetCharDimensions(hdc=0x{Hdc:X8}, lptm=0x{Lptm:X8}, lpAvgCharWidth=0x{LpAvgCharWidth:X8})",
				hdc, lptm, lpAvgCharWidth);
			
			// Stub: return default font height
			if (lpAvgCharWidth != 0)
			{
				_env.MemWrite32(lpAvgCharWidth, (uint)DefaultCharWidth);
			}
			return DefaultFontHeight;
		}

		[DllModuleExport(4, IsStub = true)]
		private uint GdiGetCodePage(uint hdc)
		{
			_logger.LogInformation("[Gdi32] GdiGetCodePage(hdc=0x{Hdc:X8})", hdc);
			
			// Stub: return CP_ACP (0) - ANSI code page
			return 0;
		}

		[DllModuleExport(16, IsStub = true)]
		private uint GetCharABCWidthsA(uint hdc, uint uFirstChar, uint uLastChar, uint lpabc)
		{
			_logger.LogInformation("[Gdi32] GetCharABCWidthsA(hdc=0x{Hdc:X8}, uFirstChar={UFirstChar}, uLastChar={ULastChar}, lpabc=0x{Lpabc:X8})",
				hdc, uFirstChar, uLastChar, lpabc);
			
			// Stub: return FALSE
			return 0;
		}

		[DllModuleExport(16, IsStub = true)]
		private uint GetCharABCWidthsW(uint hdc, uint uFirstChar, uint uLastChar, uint lpabc)
		{
			_logger.LogInformation("[Gdi32] GetCharABCWidthsW(hdc=0x{Hdc:X8}, uFirstChar={UFirstChar}, uLastChar={ULastChar}, lpabc=0x{Lpabc:X8})",
				hdc, uFirstChar, uLastChar, lpabc);
			
			// Stub: return FALSE
			return 0;
		}

		[DllModuleExport(8, IsStub = true)]
		private uint GetTextMetricsW(uint hdc, uint lptm)
		{
			_logger.LogInformation("[Gdi32] GetTextMetricsW(hdc=0x{Hdc:X8}, lptm=0x{Lptm:X8})", hdc, lptm);
			
			// Stub: fill in default metrics
			if (lptm != 0)
			{
				// TEXTMETRICW structure (56 bytes)
				_env.MemWrite32(lptm, (uint)DefaultFontHeight);     // tmHeight
				_env.MemWrite32(lptm + 4, 0);                       // tmAscent
				_env.MemWrite32(lptm + 8, 0);                       // tmDescent
				_env.MemWrite32(lptm + 12, 0);                      // tmInternalLeading
				_env.MemWrite32(lptm + 16, 0);                      // tmExternalLeading
				_env.MemWrite32(lptm + 20, (uint)DefaultCharWidth); // tmAveCharWidth
				_env.MemWrite32(lptm + 24, (uint)DefaultCharWidth); // tmMaxCharWidth
			}
			return 1; // TRUE
		}

		// Metafile functions
		[DllModuleExport(4, IsStub = true)]
		private uint CreateMetaFileA(in LpcStr lpszFile)
		{
			var file = lpszFile.ToString() ?? string.Empty;
			_logger.LogInformation("[Gdi32] CreateMetaFileA(lpszFile=\"{File}\")", file);
			
			// Stub: return a dummy metafile handle
			var metaHandle = _nextGdiObjectHandle++;
			return metaHandle;
		}

		[DllModuleExport(4, IsStub = true)]
		private uint CloseMetaFile(uint hmf)
		{
			_logger.LogInformation("[Gdi32] CloseMetaFile(hmf=0x{Hmf:X8})", hmf);
			
			// Stub: return the metafile handle
			return hmf;
		}

		[DllModuleExport(16, IsStub = true)]
		private uint CreateEnhMetaFileA(uint hdcRef, in LpcStr lpFilename, uint lpRect, in LpcStr lpDescription)
		{
			var filename = lpFilename.ToString() ?? string.Empty;
			var description = lpDescription.ToString() ?? string.Empty;
			_logger.LogInformation("[Gdi32] CreateEnhMetaFileA(hdcRef=0x{HdcRef:X8}, lpFilename=\"{Filename}\", lpRect=0x{LpRect:X8}, lpDescription=\"{Description}\")",
				hdcRef, filename, lpRect, description);
			
			// Stub: return a dummy enhanced metafile handle
			var emfHandle = _nextGdiObjectHandle++;
			return emfHandle;
		}

		[DllModuleExport(4, IsStub = true)]
		private uint CloseEnhMetaFile(uint hdc)
		{
			_logger.LogInformation("[Gdi32] CloseEnhMetaFile(hdc=0x{Hdc:X8})", hdc);
			
			// Stub: return a dummy enhanced metafile handle
			var emfHandle = _nextGdiObjectHandle++;
			return emfHandle;
		}

		[DllModuleExport(12, IsStub = true)]
		private uint GetEnhMetaFileBits(uint hemf, uint cbBuffer, uint lpbBuffer)
		{
			_logger.LogInformation("[Gdi32] GetEnhMetaFileBits(hemf=0x{Hemf:X8}, cbBuffer={CbBuffer}, lpbBuffer=0x{LpbBuffer:X8})",
				hemf, cbBuffer, lpbBuffer);
			
			// Stub: return 0 (no bits copied)
			return 0;
		}
	}
}