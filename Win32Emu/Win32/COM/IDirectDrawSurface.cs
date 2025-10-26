using System;
using System.Runtime.InteropServices;

namespace Win32Emu.Win32.COM;

/// <summary>
/// COM interface definitions for IDirectDrawSurface that match MSDN documentation.
/// </summary>
public static class IDirectDrawSurface
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int QueryInterface(IntPtr pThis, IntPtr riid, IntPtr ppvObject);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate uint AddRef(IntPtr pThis);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate uint Release(IntPtr pThis);
	
	/// <summary>
	/// HRESULT AddAttachedSurface(LPDIRECTDRAWSURFACE lpDDSAttachedSurface);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int AddAttachedSurface(IntPtr pThis, IntPtr lpDDSAttachedSurface);
	
	/// <summary>
	/// HRESULT AddOverlayDirtyRect(LPRECT lpRect);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int AddOverlayDirtyRect(IntPtr pThis, IntPtr lpRect);
	
	/// <summary>
	/// HRESULT Blt(LPRECT lpDestRect, LPDIRECTDRAWSURFACE lpDDSrcSurface, LPRECT lpSrcRect, DWORD dwFlags, LPDDBLTFX lpDDBltFx);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Blt(IntPtr pThis, IntPtr lpDestRect, IntPtr lpDDSrcSurface, IntPtr lpSrcRect, uint dwFlags, IntPtr lpDDBltFx);
	
	/// <summary>
	/// HRESULT BltBatch(LPDDBLTBATCH lpDDBltBatch, DWORD dwCount, DWORD dwFlags);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int BltBatch(IntPtr pThis, IntPtr lpDDBltBatch, uint dwCount, uint dwFlags);
	
	/// <summary>
	/// HRESULT BltFast(DWORD dwX, DWORD dwY, LPDIRECTDRAWSURFACE lpDDSrcSurface, LPRECT lpSrcRect, DWORD dwTrans);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int BltFast(IntPtr pThis, uint dwX, uint dwY, IntPtr lpDDSrcSurface, IntPtr lpSrcRect, uint dwTrans);
	
	/// <summary>
	/// HRESULT DeleteAttachedSurface(DWORD dwFlags, LPDIRECTDRAWSURFACE lpDDSAttachedSurface);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int DeleteAttachedSurface(IntPtr pThis, uint dwFlags, IntPtr lpDDSAttachedSurface);
	
	/// <summary>
	/// HRESULT EnumAttachedSurfaces(LPVOID lpContext, LPDDENUMSURFACESCALLBACK lpEnumSurfacesCallback);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int EnumAttachedSurfaces(IntPtr pThis, IntPtr lpContext, IntPtr lpEnumSurfacesCallback);
	
	/// <summary>
	/// HRESULT EnumOverlayZOrders(DWORD dwFlags, LPVOID lpContext, LPDDENUMSURFACESCALLBACK lpfnCallback);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int EnumOverlayZOrders(IntPtr pThis, uint dwFlags, IntPtr lpContext, IntPtr lpfnCallback);
	
	/// <summary>
	/// HRESULT Flip(LPDIRECTDRAWSURFACE lpDDSurfaceTargetOverride, DWORD dwFlags);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Flip(IntPtr pThis, IntPtr lpDDSurfaceTargetOverride, uint dwFlags);
	
	/// <summary>
	/// HRESULT GetAttachedSurface(LPDDSCAPS lpDDSCaps, LPDIRECTDRAWSURFACE *lplpDDAttachedSurface);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetAttachedSurface(IntPtr pThis, IntPtr lpDDSCaps, IntPtr lplpDDAttachedSurface);
	
	/// <summary>
	/// HRESULT GetBltStatus(DWORD dwFlags);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetBltStatus(IntPtr pThis, uint dwFlags);
	
	/// <summary>
	/// HRESULT GetCaps(LPDDSCAPS lpDDSCaps);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetCaps(IntPtr pThis, IntPtr lpDDSCaps);
	
	/// <summary>
	/// HRESULT GetClipper(LPDIRECTDRAWCLIPPER *lplpDDClipper);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetClipper(IntPtr pThis, IntPtr lplpDDClipper);
	
	/// <summary>
	/// HRESULT GetColorKey(DWORD dwFlags, LPDDCOLORKEY lpDDColorKey);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetColorKey(IntPtr pThis, uint dwFlags, IntPtr lpDDColorKey);
	
	/// <summary>
	/// HRESULT GetDC(HDC *lphDC);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetDC(IntPtr pThis, IntPtr lphDC);
	
	/// <summary>
	/// HRESULT GetFlipStatus(DWORD dwFlags);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetFlipStatus(IntPtr pThis, uint dwFlags);
	
	/// <summary>
	/// HRESULT GetOverlayPosition(LPLONG lplX, LPLONG lplY);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetOverlayPosition(IntPtr pThis, IntPtr lplX, IntPtr lplY);
	
	/// <summary>
	/// HRESULT GetPalette(LPDIRECTDRAWPALETTE *lplpDDPalette);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetPalette(IntPtr pThis, IntPtr lplpDDPalette);
	
	/// <summary>
	/// HRESULT GetPixelFormat(LPDDPIXELFORMAT lpDDPixelFormat);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetPixelFormat(IntPtr pThis, IntPtr lpDDPixelFormat);
	
	/// <summary>
	/// HRESULT GetSurfaceDesc(LPDDSURFACEDESC lpDDSurfaceDesc);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetSurfaceDesc(IntPtr pThis, IntPtr lpDDSurfaceDesc);
	
	/// <summary>
	/// HRESULT Initialize(LPDIRECTDRAW lpDD, LPDDSURFACEDESC lpDDSurfaceDesc);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Initialize(IntPtr pThis, IntPtr lpDD, IntPtr lpDDSurfaceDesc);
	
	/// <summary>
	/// HRESULT IsLost();
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int IsLost(IntPtr pThis);
	
	/// <summary>
	/// HRESULT Lock(LPRECT lpDestRect, LPDDSURFACEDESC lpDDSurfaceDesc, DWORD dwFlags, HANDLE hEvent);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Lock(IntPtr pThis, IntPtr lpDestRect, IntPtr lpDDSurfaceDesc, uint dwFlags, IntPtr hEvent);
	
	/// <summary>
	/// HRESULT ReleaseDC(HDC hDC);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int ReleaseDC(IntPtr pThis, IntPtr hDC);
	
	/// <summary>
	/// HRESULT Restore();
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Restore(IntPtr pThis);
	
	/// <summary>
	/// HRESULT SetClipper(LPDIRECTDRAWCLIPPER lpDDClipper);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetClipper(IntPtr pThis, IntPtr lpDDClipper);
	
	/// <summary>
	/// HRESULT SetColorKey(DWORD dwFlags, LPDDCOLORKEY lpDDColorKey);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetColorKey(IntPtr pThis, uint dwFlags, IntPtr lpDDColorKey);
	
	/// <summary>
	/// HRESULT SetOverlayPosition(LONG lX, LONG lY);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetOverlayPosition(IntPtr pThis, int lX, int lY);
	
	/// <summary>
	/// HRESULT SetPalette(LPDIRECTDRAWPALETTE lpDDPalette);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetPalette(IntPtr pThis, IntPtr lpDDPalette);
	
	/// <summary>
	/// HRESULT Unlock(LPVOID lpSurfaceData);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Unlock(IntPtr pThis, IntPtr lpSurfaceData);
	
	/// <summary>
	/// HRESULT UpdateOverlay(LPRECT lpSrcRect, LPDIRECTDRAWSURFACE lpDDDestSurface, LPRECT lpDestRect, DWORD dwFlags, LPDDOVERLAYFX lpDDOverlayFx);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int UpdateOverlay(IntPtr pThis, IntPtr lpSrcRect, IntPtr lpDDDestSurface, IntPtr lpDestRect, uint dwFlags, IntPtr lpDDOverlayFx);
	
	/// <summary>
	/// HRESULT UpdateOverlayDisplay(DWORD dwFlags);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int UpdateOverlayDisplay(IntPtr pThis, uint dwFlags);
	
	/// <summary>
	/// HRESULT UpdateOverlayZOrder(DWORD dwFlags, LPDIRECTDRAWSURFACE lpDDSReference);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int UpdateOverlayZOrder(IntPtr pThis, uint dwFlags, IntPtr lpDDSReference);
}
