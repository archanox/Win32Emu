using System;
using System.Runtime.InteropServices;

namespace Win32Emu.Win32.COM;

/// <summary>
/// COM interface definitions for IDirectDrawSurface2 that extends IDirectDrawSurface
/// IDirectDrawSurface2 adds GetDDInterface, PageLock, and PageUnlock methods
/// Reference: https://learn.microsoft.com/en-us/windows/win32/api/ddraw/nn-ddraw-idirectdrawsurface2
/// </summary>
public static class IDirectDrawSurface2
{
	// IUnknown methods (0-2)
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int QueryInterface(IntPtr pThis, IntPtr riid, IntPtr ppvObject);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate uint AddRef(IntPtr pThis);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate uint Release(IntPtr pThis);
	
	// IDirectDrawSurface methods (3-35)
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int AddAttachedSurface(IntPtr pThis, IntPtr lpDDSAttachedSurface);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int AddOverlayDirtyRect(IntPtr pThis, IntPtr lpRect);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Blt(IntPtr pThis, IntPtr lpDestRect, IntPtr lpDDSrcSurface, IntPtr lpSrcRect, uint dwFlags, IntPtr lpDDBltFx);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int BltBatch(IntPtr pThis, IntPtr lpDDBltBatch, uint dwCount, uint dwFlags);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int BltFast(IntPtr pThis, uint dwX, uint dwY, IntPtr lpDDSrcSurface, IntPtr lpSrcRect, uint dwTrans);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int DeleteAttachedSurface(IntPtr pThis, uint dwFlags, IntPtr lpDDSAttachedSurface);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int EnumAttachedSurfaces(IntPtr pThis, IntPtr lpContext, IntPtr lpEnumSurfacesCallback);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int EnumOverlayZOrders(IntPtr pThis, uint dwFlags, IntPtr lpContext, IntPtr lpfnCallback);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Flip(IntPtr pThis, IntPtr lpDDSurfaceTargetOverride, uint dwFlags);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetAttachedSurface(IntPtr pThis, IntPtr lpDDSCaps, IntPtr lplpDDAttachedSurface);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetBltStatus(IntPtr pThis, uint dwFlags);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetCaps(IntPtr pThis, IntPtr lpDDSCaps);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetClipper(IntPtr pThis, IntPtr lplpDDClipper);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetColorKey(IntPtr pThis, uint dwFlags, IntPtr lpDDColorKey);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetDC(IntPtr pThis, IntPtr lphDC);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetFlipStatus(IntPtr pThis, uint dwFlags);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetOverlayPosition(IntPtr pThis, IntPtr lplX, IntPtr lplY);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetPalette(IntPtr pThis, IntPtr lplpDDPalette);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetPixelFormat(IntPtr pThis, IntPtr lpDDPixelFormat);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetSurfaceDesc(IntPtr pThis, IntPtr lpDDSurfaceDesc);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Initialize(IntPtr pThis, IntPtr lpDD, IntPtr lpDDSurfaceDesc);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int IsLost(IntPtr pThis);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Lock(IntPtr pThis, IntPtr lpDestRect, IntPtr lpDDSurfaceDesc, uint dwFlags, IntPtr hEvent);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int ReleaseDC(IntPtr pThis, IntPtr hDC);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Restore(IntPtr pThis);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetClipper(IntPtr pThis, IntPtr lpDDClipper);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetColorKey(IntPtr pThis, uint dwFlags, IntPtr lpDDColorKey);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetOverlayPosition(IntPtr pThis, int lX, int lY);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetPalette(IntPtr pThis, IntPtr lpDDPalette);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Unlock(IntPtr pThis, IntPtr lpSurfaceData);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int UpdateOverlay(IntPtr pThis, IntPtr lpSrcRect, IntPtr lpDDDestSurface, IntPtr lpDestRect, uint dwFlags, IntPtr lpDDOverlayFx);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int UpdateOverlayDisplay(IntPtr pThis, uint dwFlags);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int UpdateOverlayZOrder(IntPtr pThis, uint dwFlags, IntPtr lpDDSReference);
	
	// IDirectDrawSurface2 new methods (36-38)
	
	/// <summary>
	/// HRESULT GetDDInterface(LPVOID *lplpDD);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetDDInterface(IntPtr pThis, IntPtr lplpDD);
	
	/// <summary>
	/// HRESULT PageLock(DWORD dwFlags);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int PageLock(IntPtr pThis, uint dwFlags);
	
	/// <summary>
	/// HRESULT PageUnlock(DWORD dwFlags);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int PageUnlock(IntPtr pThis, uint dwFlags);
}
