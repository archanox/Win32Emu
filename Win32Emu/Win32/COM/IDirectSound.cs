using System;
using System.Runtime.InteropServices;

namespace Win32Emu.Win32.COM;

/// <summary>
/// COM interface definitions for IDirectSound that match MSDN documentation.
/// </summary>
public static class IDirectSound
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int QueryInterface(IntPtr pThis, IntPtr riid, IntPtr ppvObject);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate uint AddRef(IntPtr pThis);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate uint Release(IntPtr pThis);
	
	/// <summary>
	/// HRESULT CreateSoundBuffer(LPCDSBUFFERDESC pcDSBufferDesc, LPDIRECTSOUNDBUFFER *ppDSBuffer, LPUNKNOWN pUnkOuter);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int CreateSoundBuffer(IntPtr pThis, IntPtr pcDSBufferDesc, IntPtr ppDSBuffer, IntPtr pUnkOuter);
	
	/// <summary>
	/// HRESULT GetCaps(LPDSCAPS pDSCaps);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetCaps(IntPtr pThis, IntPtr pDSCaps);
	
	/// <summary>
	/// HRESULT DuplicateSoundBuffer(LPDIRECTSOUNDBUFFER pDSBufferOriginal, LPDIRECTSOUNDBUFFER *ppDSBufferDuplicate);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int DuplicateSoundBuffer(IntPtr pThis, IntPtr pDSBufferOriginal, IntPtr ppDSBufferDuplicate);
	
	/// <summary>
	/// HRESULT SetCooperativeLevel(HWND hwnd, DWORD dwLevel);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetCooperativeLevel(IntPtr pThis, IntPtr hwnd, uint dwLevel);
	
	/// <summary>
	/// HRESULT Compact();
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Compact(IntPtr pThis);
	
	/// <summary>
	/// HRESULT GetSpeakerConfig(LPDWORD pdwSpeakerConfig);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetSpeakerConfig(IntPtr pThis, IntPtr pdwSpeakerConfig);
	
	/// <summary>
	/// HRESULT SetSpeakerConfig(DWORD dwSpeakerConfig);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetSpeakerConfig(IntPtr pThis, uint dwSpeakerConfig);
	
	/// <summary>
	/// HRESULT Initialize(LPCGUID pcGuidDevice);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Initialize(IntPtr pThis, IntPtr pcGuidDevice);
}
