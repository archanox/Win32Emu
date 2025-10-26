using System;
using System.Runtime.InteropServices;

namespace Win32Emu.Win32.COM;

/// <summary>
/// COM interface definitions for IDirectDraw that match MSDN documentation.
/// These delegates define the proper stdcall signatures for type safety and automatic argument size calculation.
/// </summary>
public static class IDirectDraw
{
	// IUnknown methods (inherited by all COM interfaces)
	
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
	
	// IDirectDraw methods
	
	/// <summary>
	/// HRESULT Compact();
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Compact(IntPtr pThis);
	
	/// <summary>
	/// HRESULT CreateClipper(DWORD dwFlags, LPDIRECTDRAWCLIPPER *lplpDDClipper, IUnknown *pUnkOuter);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int CreateClipper(IntPtr pThis, uint dwFlags, IntPtr lplpDDClipper, IntPtr pUnkOuter);
	
	/// <summary>
	/// HRESULT CreatePalette(DWORD dwFlags, LPPALETTEENTRY lpColorTable, LPDIRECTDRAWPALETTE *lplpDDPalette, IUnknown *pUnkOuter);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int CreatePalette(IntPtr pThis, uint dwFlags, IntPtr lpColorTable, IntPtr lplpDDPalette, IntPtr pUnkOuter);
	
	/// <summary>
	/// HRESULT CreateSurface(LPDDSURFACEDESC lpDDSurfaceDesc, LPDIRECTDRAWSURFACE *lplpDDSurface, IUnknown *pUnkOuter);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int CreateSurface(IntPtr pThis, IntPtr lpDDSurfaceDesc, IntPtr lplpDDSurface, IntPtr pUnkOuter);
	
	/// <summary>
	/// HRESULT DuplicateSurface(LPDIRECTDRAWSURFACE lpDDSurface, LPDIRECTDRAWSURFACE *lplpDupDDSurface);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int DuplicateSurface(IntPtr pThis, IntPtr lpDDSurface, IntPtr lplpDupDDSurface);
	
	/// <summary>
	/// HRESULT EnumDisplayModes(DWORD dwFlags, LPDDSURFACEDESC lpDDSurfaceDesc, LPVOID lpContext, LPDDENUMMODESCALLBACK lpEnumModesCallback);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int EnumDisplayModes(IntPtr pThis, uint dwFlags, IntPtr lpDDSurfaceDesc, IntPtr lpContext, IntPtr lpEnumModesCallback);
	
	/// <summary>
	/// HRESULT EnumSurfaces(DWORD dwFlags, LPDDSURFACEDESC lpDDSD, LPVOID lpContext, LPDDENUMSURFACESCALLBACK lpEnumSurfacesCallback);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int EnumSurfaces(IntPtr pThis, uint dwFlags, IntPtr lpDDSD, IntPtr lpContext, IntPtr lpEnumSurfacesCallback);
	
	/// <summary>
	/// HRESULT FlipToGDISurface();
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int FlipToGDISurface(IntPtr pThis);
	
	/// <summary>
	/// HRESULT GetCaps(LPDDCAPS lpDDDriverCaps, LPDDCAPS lpDDHELCaps);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetCaps(IntPtr pThis, IntPtr lpDDDriverCaps, IntPtr lpDDHELCaps);
	
	/// <summary>
	/// HRESULT GetDisplayMode(LPDDSURFACEDESC lpDDSurfaceDesc);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetDisplayMode(IntPtr pThis, IntPtr lpDDSurfaceDesc);
	
	/// <summary>
	/// HRESULT GetFourCCCodes(LPDWORD lpNumCodes, LPDWORD lpCodes);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetFourCCCodes(IntPtr pThis, IntPtr lpNumCodes, IntPtr lpCodes);
	
	/// <summary>
	/// HRESULT GetGDISurface(LPDIRECTDRAWSURFACE *lplpGDIDDSurface);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetGDISurface(IntPtr pThis, IntPtr lplpGDIDDSurface);
	
	/// <summary>
	/// HRESULT GetMonitorFrequency(LPDWORD lpdwFrequency);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetMonitorFrequency(IntPtr pThis, IntPtr lpdwFrequency);
	
	/// <summary>
	/// HRESULT GetScanLine(LPDWORD lpdwScanLine);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetScanLine(IntPtr pThis, IntPtr lpdwScanLine);
	
	/// <summary>
	/// HRESULT GetVerticalBlankStatus(LPBOOL lpbIsInVB);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetVerticalBlankStatus(IntPtr pThis, IntPtr lpbIsInVB);
	
	/// <summary>
	/// HRESULT Initialize(GUID *lpGUID);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Initialize(IntPtr pThis, IntPtr lpGUID);
	
	/// <summary>
	/// HRESULT RestoreDisplayMode();
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int RestoreDisplayMode(IntPtr pThis);
	
	/// <summary>
	/// HRESULT SetCooperativeLevel(HWND hWnd, DWORD dwFlags);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetCooperativeLevel(IntPtr pThis, IntPtr hWnd, uint dwFlags);
	
	/// <summary>
	/// HRESULT SetDisplayMode(DWORD dwWidth, DWORD dwHeight, DWORD dwBPP);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetDisplayMode(IntPtr pThis, uint dwWidth, uint dwHeight, uint dwBPP);
	
	/// <summary>
	/// HRESULT WaitForVerticalBlank(DWORD dwFlags, HANDLE hEvent);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int WaitForVerticalBlank(IntPtr pThis, uint dwFlags, IntPtr hEvent);
}
