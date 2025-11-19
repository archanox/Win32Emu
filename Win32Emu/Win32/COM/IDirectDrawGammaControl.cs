using System;
using System.Runtime.InteropServices;

namespace Win32Emu.Win32.COM;

/// <summary>
/// COM interface definitions for IDirectDrawGammaControl
/// Used to adjust gamma ramp levels of the primary surface
/// Reference: https://learn.microsoft.com/en-us/windows/win32/api/ddraw/nn-ddraw-idirectdrawgammacontrol
/// </summary>
public static class IDirectDrawGammaControl
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
	
	// IDirectDrawGammaControl methods (3-4)
	
	/// <summary>
	/// HRESULT GetGammaRamp(DWORD dwFlags, LPDDGAMMARAMP lpRampData);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetGammaRamp(IntPtr pThis, uint dwFlags, IntPtr lpRampData);
	
	/// <summary>
	/// HRESULT SetGammaRamp(DWORD dwFlags, LPDDGAMMARAMP lpRampData);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetGammaRamp(IntPtr pThis, uint dwFlags, IntPtr lpRampData);
}
