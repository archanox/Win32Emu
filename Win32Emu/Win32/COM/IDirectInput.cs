using System;
using System.Runtime.InteropServices;

namespace Win32Emu.Win32.COM;

/// <summary>
/// COM interface definitions for IDirectInput that match MSDN documentation.
/// These delegates define the proper stdcall signatures for type safety and automatic argument size calculation.
/// </summary>
public static class IDirectInput
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
	
	// IDirectInput methods
	
	/// <summary>
	/// HRESULT CreateDevice(REFGUID rguid, LPDIRECTINPUTDEVICE *lplpDirectInputDevice, LPUNKNOWN pUnkOuter);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int CreateDevice(IntPtr pThis, IntPtr rguid, IntPtr lplpDirectInputDevice, IntPtr pUnkOuter);
	
	/// <summary>
	/// HRESULT EnumDevices(DWORD dwDevType, LPDIENUMDEVICESCALLBACK lpCallback, LPVOID pvRef, DWORD dwFlags);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int EnumDevices(IntPtr pThis, uint dwDevType, IntPtr lpCallback, IntPtr pvRef, uint dwFlags);
	
	/// <summary>
	/// HRESULT GetDeviceStatus(REFGUID rguidInstance);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetDeviceStatus(IntPtr pThis, IntPtr rguidInstance);
	
	/// <summary>
	/// HRESULT RunControlPanel(HWND hwndOwner, DWORD dwFlags);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int RunControlPanel(IntPtr pThis, IntPtr hwndOwner, uint dwFlags);
	
	/// <summary>
	/// HRESULT Initialize(HINSTANCE hinst, DWORD dwVersion);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Initialize(IntPtr pThis, IntPtr hinst, uint dwVersion);
}
