using System;
using System.Runtime.InteropServices;

namespace Win32Emu.Win32.COM;

/// <summary>
/// COM interface definitions for IDirectSoundBuffer that match MSDN documentation.
/// </summary>
public static class IDirectSoundBuffer
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int QueryInterface(IntPtr pThis, IntPtr riid, IntPtr ppvObject);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate uint AddRef(IntPtr pThis);
	
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate uint Release(IntPtr pThis);
	
	/// <summary>
	/// HRESULT GetCaps(LPDSBCAPS pDSBufferCaps);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetCaps(IntPtr pThis, IntPtr pDSBufferCaps);
	
	/// <summary>
	/// HRESULT GetCurrentPosition(LPDWORD pdwCurrentPlayCursor, LPDWORD pdwCurrentWriteCursor);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetCurrentPosition(IntPtr pThis, IntPtr pdwCurrentPlayCursor, IntPtr pdwCurrentWriteCursor);
	
	/// <summary>
	/// HRESULT GetFormat(LPWAVEFORMATEX pwfxFormat, DWORD dwSizeAllocated, LPDWORD pdwSizeWritten);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetFormat(IntPtr pThis, IntPtr pwfxFormat, uint dwSizeAllocated, IntPtr pdwSizeWritten);
	
	/// <summary>
	/// HRESULT GetVolume(LPLONG plVolume);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetVolume(IntPtr pThis, IntPtr plVolume);
	
	/// <summary>
	/// HRESULT GetPan(LPLONG plPan);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetPan(IntPtr pThis, IntPtr plPan);
	
	/// <summary>
	/// HRESULT GetFrequency(LPDWORD pdwFrequency);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetFrequency(IntPtr pThis, IntPtr pdwFrequency);
	
	/// <summary>
	/// HRESULT GetStatus(LPDWORD pdwStatus);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int GetStatus(IntPtr pThis, IntPtr pdwStatus);
	
	/// <summary>
	/// HRESULT Initialize(LPDIRECTSOUND pDirectSound, LPCDSBUFFERDESC pcDSBufferDesc);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Initialize(IntPtr pThis, IntPtr pDirectSound, IntPtr pcDSBufferDesc);
	
	/// <summary>
	/// HRESULT Lock(DWORD dwOffset, DWORD dwBytes, LPVOID *ppvAudioPtr1, LPDWORD pdwAudioBytes1, LPVOID *ppvAudioPtr2, LPDWORD pdwAudioBytes2, DWORD dwFlags);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Lock(IntPtr pThis, uint dwOffset, uint dwBytes, IntPtr ppvAudioPtr1, IntPtr pdwAudioBytes1, IntPtr ppvAudioPtr2, IntPtr pdwAudioBytes2, uint dwFlags);
	
	/// <summary>
	/// HRESULT Play(DWORD dwReserved1, DWORD dwPriority, DWORD dwFlags);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Play(IntPtr pThis, uint dwReserved1, uint dwPriority, uint dwFlags);
	
	/// <summary>
	/// HRESULT SetCurrentPosition(DWORD dwNewPosition);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetCurrentPosition(IntPtr pThis, uint dwNewPosition);
	
	/// <summary>
	/// HRESULT SetFormat(LPCWAVEFORMATEX pcfxFormat);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetFormat(IntPtr pThis, IntPtr pcfxFormat);
	
	/// <summary>
	/// HRESULT SetVolume(LONG lVolume);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetVolume(IntPtr pThis, int lVolume);
	
	/// <summary>
	/// HRESULT SetPan(LONG lPan);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetPan(IntPtr pThis, int lPan);
	
	/// <summary>
	/// HRESULT SetFrequency(DWORD dwFrequency);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int SetFrequency(IntPtr pThis, uint dwFrequency);
	
	/// <summary>
	/// HRESULT Stop();
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Stop(IntPtr pThis);
	
	/// <summary>
	/// HRESULT Unlock(LPVOID pvAudioPtr1, DWORD dwAudioBytes1, LPVOID pvAudioPtr2, DWORD dwAudioBytes2);
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Unlock(IntPtr pThis, IntPtr pvAudioPtr1, uint dwAudioBytes1, IntPtr pvAudioPtr2, uint dwAudioBytes2);
	
	/// <summary>
	/// HRESULT Restore();
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int Restore(IntPtr pThis);
}
