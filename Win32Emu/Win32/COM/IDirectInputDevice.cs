using System;
using System.Runtime.InteropServices;

namespace Win32Emu.Win32.COM;

/// <summary>
/// COM interface definitions for IDirectInputDevice that match MSDN documentation.
/// These delegates define the proper stdcall signatures for type safety and automatic argument size calculation.
/// </summary>
public static class IDirectInputDevice
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
	
	// IDirectInputDevice methods
	
	/// <summary>
	/// HRESULT GetCapabilities(LPDIDEVCAPS lpDIDevCaps);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetCapabilities(IntPtr pThis, IntPtr lpDIDevCaps);
	
	/// <summary>
	/// HRESULT EnumObjects(LPDIENUMDEVICEOBJECTSCALLBACK lpCallback, LPVOID pvRef, DWORD dwFlags);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int EnumObjects(IntPtr pThis, IntPtr lpCallback, IntPtr pvRef, uint dwFlags);
	
	/// <summary>
	/// HRESULT GetProperty(REFGUID rguidProp, LPDIPROPHEADER pdiph);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetProperty(IntPtr pThis, IntPtr rguidProp, IntPtr pdiph);
	
	/// <summary>
	/// HRESULT SetProperty(REFGUID rguidProp, LPCDIPROPHEADER lpdiph);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetProperty(IntPtr pThis, IntPtr rguidProp, IntPtr lpdiph);
	
	/// <summary>
	/// HRESULT Acquire();
	/// Obtains access to the input device.
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Acquire(IntPtr pThis);
	
	/// <summary>
	/// HRESULT Unacquire();
	/// Releases access to the input device.
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Unacquire(IntPtr pThis);
	
	/// <summary>
	/// HRESULT GetDeviceState(DWORD cbData, LPVOID lpvData);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetDeviceState(IntPtr pThis, uint cbData, IntPtr lpvData);
	
	/// <summary>
	/// HRESULT GetDeviceData(DWORD cbObjectData, LPDIDEVICEOBJECTDATA rgdod, LPDWORD pdwInOut, DWORD dwFlags);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetDeviceData(IntPtr pThis, uint cbObjectData, IntPtr rgdod, IntPtr pdwInOut, uint dwFlags);
	
	/// <summary>
	/// HRESULT SetDataFormat(LPCDIDATAFORMAT lpdf);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetDataFormat(IntPtr pThis, IntPtr lpdf);
	
	/// <summary>
	/// HRESULT SetEventNotification(HANDLE hEvent);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetEventNotification(IntPtr pThis, IntPtr hEvent);
	
	/// <summary>
	/// HRESULT SetCooperativeLevel(HWND hwnd, DWORD dwFlags);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetCooperativeLevel(IntPtr pThis, IntPtr hwnd, uint dwFlags);
	
	/// <summary>
	/// HRESULT GetObjectInfo(LPDIDEVICEOBJECTINSTANCE pdidoi, DWORD dwObj, DWORD dwHow);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetObjectInfo(IntPtr pThis, IntPtr pdidoi, uint dwObj, uint dwHow);
	
	/// <summary>
	/// HRESULT GetDeviceInfo(LPDIDEVICEINSTANCE pdidi);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetDeviceInfo(IntPtr pThis, IntPtr pdidi);
	
	/// <summary>
	/// HRESULT RunControlPanel(HWND hwndOwner, DWORD dwFlags);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int RunControlPanel(IntPtr pThis, IntPtr hwndOwner, uint dwFlags);
	
	/// <summary>
	/// HRESULT Initialize(HINSTANCE hinst, DWORD dwVersion, REFGUID rguid);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Initialize(IntPtr pThis, IntPtr hinst, uint dwVersion, IntPtr rguid);
}
