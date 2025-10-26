using System;
using System.Runtime.InteropServices;

namespace Win32Emu.Win32.COM;

/// <summary>
/// COM interface definitions for IDirectDrawClipper that match MSDN documentation.
/// </summary>
public static class IDirectDrawClipper
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int QueryInterface(IntPtr pThis, IntPtr riid, IntPtr ppvObject);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate uint AddRef(IntPtr pThis);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate uint Release(IntPtr pThis);
	
	/// <summary>
	/// HRESULT GetClipList(LPRECT lpRect, LPRGNDATA lpClipList, LPDWORD lpdwSize);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetClipList(IntPtr pThis, IntPtr lpRect, IntPtr lpClipList, IntPtr lpdwSize);
	
	/// <summary>
	/// HRESULT GetHWnd(HWND *lphWnd);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetHWnd(IntPtr pThis, IntPtr lphWnd);
	
	/// <summary>
	/// HRESULT Initialize(LPDIRECTDRAW lpDD, DWORD dwFlags);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Initialize(IntPtr pThis, IntPtr lpDD, uint dwFlags);
	
	/// <summary>
	/// HRESULT IsClipListChanged(BOOL *lpbChanged);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int IsClipListChanged(IntPtr pThis, IntPtr lpbChanged);
	
	/// <summary>
	/// HRESULT SetClipList(LPRGNDATA lpClipList, DWORD dwFlags);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetClipList(IntPtr pThis, IntPtr lpClipList, uint dwFlags);
	
	/// <summary>
	/// HRESULT SetHWnd(DWORD dwFlags, HWND hWnd);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetHWnd(IntPtr pThis, uint dwFlags, IntPtr hWnd);
}
