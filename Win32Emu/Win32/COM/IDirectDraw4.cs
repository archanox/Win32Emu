using System;
using System.Runtime.InteropServices;

namespace Win32Emu.Win32.COM;

/// <summary>
/// COM interface definitions for IDirectDraw4 that extends IDirectDraw2
/// IDirectDraw4 adds GetSurfaceFromDC, RestoreAllSurfaces, TestCooperativeLevel, and GetDeviceIdentifier methods
/// Reference: https://learn.microsoft.com/en-us/windows/win32/api/ddraw/nn-ddraw-idirectdraw4
/// </summary>
public static class IDirectDraw4
{
	// IUnknown methods (0-2)
	
	/// <summary>
	/// HRESULT QueryInterface(REFIID riid, void **ppvObject);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int QueryInterface(IntPtr pThis, IntPtr riid, IntPtr ppvObject);
	
	/// <summary>
	/// ULONG AddRef();
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate uint AddRef(IntPtr pThis);
	
	/// <summary>
	/// ULONG Release();
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate uint Release(IntPtr pThis);
	
	// IDirectDraw methods (3-22) - inherited from IDirectDraw
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Compact(IntPtr pThis);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int CreateClipper(IntPtr pThis, uint dwFlags, IntPtr lplpDDClipper, IntPtr pUnkOuter);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int CreatePalette(IntPtr pThis, uint dwFlags, IntPtr lpColorTable, IntPtr lplpDDPalette, IntPtr pUnkOuter);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int CreateSurface(IntPtr pThis, IntPtr lpDDSurfaceDesc, IntPtr lplpDDSurface, IntPtr pUnkOuter);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int DuplicateSurface(IntPtr pThis, IntPtr lpDDSurface, IntPtr lplpDupDDSurface);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int EnumDisplayModes(IntPtr pThis, uint dwFlags, IntPtr lpDDSurfaceDesc, IntPtr lpContext, IntPtr lpEnumModesCallback);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int EnumSurfaces(IntPtr pThis, uint dwFlags, IntPtr lpDDSD, IntPtr lpContext, IntPtr lpEnumSurfacesCallback);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int FlipToGDISurface(IntPtr pThis);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetCaps(IntPtr pThis, IntPtr lpDDDriverCaps, IntPtr lpDDHELCaps);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetDisplayMode(IntPtr pThis, IntPtr lpDDSurfaceDesc);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetFourCCCodes(IntPtr pThis, IntPtr lpNumCodes, IntPtr lpCodes);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetGDISurface(IntPtr pThis, IntPtr lplpGDIDDSurface);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetMonitorFrequency(IntPtr pThis, IntPtr lpdwFrequency);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetScanLine(IntPtr pThis, IntPtr lpdwScanLine);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetVerticalBlankStatus(IntPtr pThis, IntPtr lpbIsInVB);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Initialize(IntPtr pThis, IntPtr lpGUID);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int RestoreDisplayMode(IntPtr pThis);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetCooperativeLevel(IntPtr pThis, IntPtr hWnd, uint dwFlags);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetDisplayMode(IntPtr pThis, uint dwWidth, uint dwHeight, uint dwBPP);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int WaitForVerticalBlank(IntPtr pThis, uint dwFlags, IntPtr hEvent);
	
	// IDirectDraw2 method (23)
	
	/// <summary>
	/// HRESULT GetAvailableVidMem(LPDDSCAPS lpDDSCaps, LPDWORD lpdwTotal, LPDWORD lpdwFree);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetAvailableVidMem(IntPtr pThis, IntPtr lpDDSCaps, IntPtr lpdwTotal, IntPtr lpdwFree);
	
	// IDirectDraw4 new methods (24-27)
	
	/// <summary>
	/// HRESULT GetSurfaceFromDC(HDC hdc, LPDIRECTDRAWSURFACE4 *lpDDS);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetSurfaceFromDC(IntPtr pThis, IntPtr hdc, IntPtr lpDDS);
	
	/// <summary>
	/// HRESULT RestoreAllSurfaces();
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int RestoreAllSurfaces(IntPtr pThis);
	
	/// <summary>
	/// HRESULT TestCooperativeLevel();
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int TestCooperativeLevel(IntPtr pThis);
	
	/// <summary>
	/// HRESULT GetDeviceIdentifier(LPDDDEVICEIDENTIFIER lpdddi, DWORD dwFlags);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetDeviceIdentifier(IntPtr pThis, IntPtr lpdddi, uint dwFlags);
}
