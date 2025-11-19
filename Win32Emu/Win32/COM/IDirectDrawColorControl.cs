using System;
using System.Runtime.InteropServices;

namespace Win32Emu.Win32.COM;

/// <summary>
/// COM interface definitions for IDirectDrawColorControl
/// Used to get and set color controls for overlay or primary surfaces
/// Reference: https://learn.microsoft.com/en-us/windows/win32/api/ddraw/nn-ddraw-idirectdrawcolorcontrol
/// </summary>
public static class IDirectDrawColorControl
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
	
	// IDirectDrawColorControl methods (3-4)
	
	/// <summary>
	/// HRESULT GetColorControls(LPDDCOLORCONTROL lpColorControl);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetColorControls(IntPtr pThis, IntPtr lpColorControl);
	
	/// <summary>
	/// HRESULT SetColorControls(LPDDCOLORCONTROL lpColorControl);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetColorControls(IntPtr pThis, IntPtr lpColorControl);
}
