using Microsoft.Extensions.Logging;
using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Win32.Win16;

/// <summary>
/// Win16 GDI module thunking layer - maps to GDI32.DLL.
/// Provides 16-bit to 32-bit thunking for common GDI functions.
/// </summary>
/// <remarks>
/// This is a simplified thunking layer that handles common Win16 GDI functions
/// where parameter sizes and semantics are compatible with Win32 equivalents.
/// Device context handles (HDC) and GDI object handles (HPEN, HBRUSH, etc.) 
/// are often 16-bit in Win16 but are extended to 32-bit in Win32.
/// </remarks>
internal class Win16GdiModule : Win16ThunkingLayer, IWin32ModuleUnsafe
{
	public Win16GdiModule(IWin32ModuleUnsafe gdi32Module, ILogger logger)
		: base(gdi32Module, logger)
	{
	}

	public string Name => "GDI";

	public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		return TryInvokeWin16(export, cpu, memory, out returnValue);
	}

	public override bool TryInvokeWin16(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
	{
		returnValue = 0;
		var exportUpper = export.ToUpperInvariant();

		// For most Win16 GDI functions, we can forward directly to the Win32 equivalent
		// Handle parameters are typically compatible (16-bit handles work as 32-bit)
		switch (exportUpper)
		{
			// Device context functions
			case "CREATEDC":
			case "CREATEDCA":
			case "CREATECOMPATIBLEDC":
			case "DELETEDC":
			case "SAVEDC":
			case "RESTOREDC":
			case "GETDEVICECAPS":
			case "SETMAPMODE":
			case "GETMAPMODE":
			case "SETVIEWPORTORG":
			case "GETVIEWPORTORG":
			case "SETWINDOWORG":
			case "GETWINDOWORG":
				LogWin16Call(export, "forwarding to GDI32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			// Drawing functions
			case "MOVETO":
			case "LINETO":
			case "RECTANGLE":
			case "ELLIPSE":
			case "POLYGON":
			case "POLYLINE":
			case "ARC":
			case "PIE":
			case "CHORD":
			case "ROUNDRECT":
			case "SETPIXEL":
			case "GETPIXEL":
			case "FLOODFILL":
			case "EXTFLOODFILL":
				LogWin16Call(export, "forwarding to GDI32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			// Text output functions
			case "TEXTOUT":
			case "TEXTOUTA":
			case "EXTTEXTOUT":
			case "EXTTEXTOUTA":
			case "DRAWTEXT":
			case "DRAWTEXTA":
			case "TABBEDTEXTOUT":
			case "TABBEDTEXTOUTA":
			case "GETTEXTEXTENT":
			case "GETTEXTEXTENTA":
			case "GETTEXTMETRICS":
			case "GETTEXTMETRICSA":
			case "SETTEXTCOLOR":
			case "GETTEXTCOLOR":
			case "SETBKCOLOR":
			case "GETBKCOLOR":
			case "SETBKMODE":
			case "GETBKMODE":
			case "SETTEXTALIGN":
			case "GETTEXTALIGN":
				LogWin16Call(export, "forwarding to GDI32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			// Pen and brush functions
			case "CREATEPEN":
			case "CREATEPENINDIRECT":
			case "CREATEBRUSHINDIRECT":
			case "CREATESOLIDBRUSH":
			case "CREATEHATCHBRUSH":
			case "CREATEPATTERNBRUSH":
			case "SELECTOBJECT":
			case "DELETEOBJECT":
			case "GETOBJECT":
			case "GETSTOCKOBJECT":
				LogWin16Call(export, "forwarding to GDI32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			// Bitmap functions
			case "CREATEBITMAP":
			case "CREATEBITMAPINDIRECT":
			case "CREATECOMPATIBLEBITMAP":
			case "CREATEDIBSECTION":
			case "BITBLT":
			case "STRETCHBLT":
			case "PATBLT":
			case "SETDIBITS":
			case "GETDIBITS":
			case "SETDIBITSTODEVICE":
			case "STRETCHDIBITS":
				LogWin16Call(export, "forwarding to GDI32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			// Font functions
			case "CREATEFONT":
			case "CREATEFONTA":
			case "CREATEFONTINDIRECT":
			case "CREATEFONTINDIRECTA":
			case "ENUMFONTS":
			case "ENUMFONTSA":
			case "ENUMFONTFAMILIES":
			case "ENUMFONTFAMILIESA":
			case "GETTEXTFACE":
			case "GETTEXTFACEA":
				LogWin16Call(export, "forwarding to GDI32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			// Region functions
			case "CREATERECTRGNINDIRECT":
			case "CREATERECTRGNINDIRECTA":
			case "CREATEELLIPTICRGN":
			case "CREATEELLIPTICRGNINDIRECT":
			case "CREATEPOLYGONRGN":
			case "CREATEROUNDRECTRGN":
			case "COMBINERGN":
			case "EQUALRGN":
			case "OFFSETRGN":
			case "GETREGIONDATA":
			case "PTINREGION":
			case "RECTINREGION":
			case "FILLRGN":
			case "FRAMERGN":
			case "INVERTRGN":
			case "PAINTRGN":
				LogWin16Call(export, "forwarding to GDI32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			// Palette functions
			case "CREATEPALETTE":
			case "SELECTPALETTE":
			case "REALIZEPALETTE":
			case "GETPALETTEENTRIES":
			case "SETPALETTEENTRIES":
			case "GETNEARESTCOLOR":
			case "GETNEARESTPALETTEINDEX":
			case "ANIMATEPALETTE":
			case "UPDATECOLORS":
				LogWin16Call(export, "forwarding to GDI32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			// Clipping functions
			case "SELECTCLIPRGN":
			case "GETCLIPBOX":
			case "EXCLUDECLIPRECT":
			case "INTERSECTCLIPRECT":
			case "OFFSETCLIPRGN":
				LogWin16Call(export, "forwarding to GDI32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			// Miscellaneous functions
			case "GETRVALUE":
			case "GETGVALUE":
			case "GETBVALUE":
			case "RGB":
			case "GETROP2":
			case "SETROP2":
			case "SETSTRETCHBLTMODE":
			case "GETSTRETCHBLTMODE":
			case "UNREALIZEOBJECT":
				LogWin16Call(export, "forwarding to GDI32");
				return Win32Module.TryInvokeUnsafe(export, cpu, memory, out returnValue);

			default:
				// Unknown Win16 GDI function
				Logger.LogWarning("[Win16 Thunk] Unknown Win16 GDI function: {Export}", export);
				return false;
		}
	}
}
