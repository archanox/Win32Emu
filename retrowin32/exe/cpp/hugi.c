/*
 * DirectDraw Example from Hugi 16 Article
 * Original: https://hugi.scene.org/online/coding/hugi%2016%20-%20coddraw.htm
 * Simplified version for Win32Emu testing
 */

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <ddraw.h>
#include <stdlib.h>

static IDirectDraw          *dd        = 0;
static IDirectDraw2         *dd2       = 0;
static IDirectDrawSurface   *ddSurface = 0;
static DWORD                 ThreadId  = 0;
static HWND                  WindowHandle;
static CRITICAL_SECTION      cr;

DWORD WINAPI mymain (void * argument);

void CloseDirectDraw (void)
{
	EnterCriticalSection (&cr);
	if (ddSurface)  ddSurface->lpVtbl->Release(ddSurface);
	if (dd2)        dd2->lpVtbl->Release(dd2);
	if (dd)         dd->lpVtbl->Release(dd);
	LeaveCriticalSection (&cr);
}

int InitDirectDraw (HWND Window, int width, int height, int bits)
{
	EnterCriticalSection (&cr);

	if (DirectDrawCreate (NULL, &dd, 0)!= DD_OK) goto error;

	if (dd->lpVtbl->SetCooperativeLevel
		(dd, Window, DDSCL_EXCLUSIVE | DDSCL_FULLSCREEN |
		 DDSCL_ALLOWREBOOT)!=DD_OK) goto error;

	if (dd->lpVtbl->QueryInterface
		(dd, &IID_IDirectDraw2, (void **) &dd2)!=DD_OK) goto error;

	if (dd2->lpVtbl->SetDisplayMode (dd2, width, height, bits, 0, 0)!=DD_OK)
		goto error;

	DDSURFACEDESC Surface;
	memset (&Surface, 0, sizeof (Surface));
	Surface.dwSize            = sizeof( Surface );
	Surface.dwFlags           = DDSD_CAPS | DDSD_BACKBUFFERCOUNT;
	Surface.dwBackBufferCount = 1;
	Surface.ddsCaps.dwCaps    = DDSCAPS_PRIMARYSURFACE |
								DDSCAPS_FLIP |
								DDSCAPS_COMPLEX;

	if (dd2->lpVtbl->CreateSurface(dd2, &Surface, &ddSurface, 0)!=DD_OK) goto error;
	LeaveCriticalSection (&cr);
	return 1;

error:
	LeaveCriticalSection (&cr);
	CloseDirectDraw();
	return 0;
}

long CALLBACK WindowProc( HWND hWnd, UINT message,
						  WPARAM wParam, LPARAM lParam )
{
	switch (message)
	{
		case WM_ACTIVATE:
		  if (wParam== WA_ACTIVE)   ShowCursor (0);
		  if (wParam== WA_INACTIVE) ShowCursor (1);
		break;

		case WM_PAINT:
		  if (!dd)
		  {
			  if (!InitDirectDraw (hWnd, 320, 240, 8))
			  {
				  MessageBox (hWnd,
					  "Failed to initialize DirectDraw.",
					  "Error",
					  MB_OK | MB_ICONERROR);
				  PostQuitMessage (1);
				  break;
			  }

			  HANDLE threadHandle = CreateThread (0, 0, mymain, 0, 0, &ThreadId);
			  if (threadHandle == NULL)
			  {
				  MessageBox (hWnd,
					  "Failed to create render thread.",
					  "Error",
					  MB_OK | MB_ICONERROR);
				  CloseDirectDraw ();
				  PostQuitMessage (1);
			  }
			  else
			  {
				  CloseHandle (threadHandle);
			  }
		  }
		break;

		case WM_DESTROY:
		case WM_KEYDOWN:
			PostQuitMessage( 0 );
		break;
	}
	return DefWindowProc( hWnd, message, wParam, lParam );
}

int PASCAL WinMain(HINSTANCE hInstance, HINSTANCE hPrevInst,
				   LPSTR lpCmdLine, int nCmdShow)
{
	InitializeCriticalSection (&cr);

	HINSTANCE instance = hInstance;

	WNDCLASS wc;
	memset (&wc, 0, sizeof (wc));
	wc.style         = CS_BYTEALIGNCLIENT;
	wc.lpfnWndProc   = WindowProc;
	wc.hInstance     = instance;
	wc.hbrBackground = (HBRUSH) GetStockObject (BLACK_BRUSH);
	wc.lpszClassName = "HugiExample";
	RegisterClass( &wc );

	WindowHandle =
	   CreateWindowEx( WS_EX_TOPMOST,                 // styleex
					   "HugiExample",                 // classname
					   "Hugi DirectDraw Example",     // caption (title)
					   WS_POPUP,                      // style
					   0,                             // left
					   0,                             // top
					   GetSystemMetrics(SM_CXSCREEN), // right
					   GetSystemMetrics(SM_CYSCREEN), // bottom
					   0,                             // parent window (none)
					   0,                             // menu (none)
					   instance,                      // instance handle
					   0 );                           // useless thing

	if (!WindowHandle) return 1;
	ShowWindow( WindowHandle, SW_SHOW);

	MSG message;
	while ( GetMessage( &message, 0, 0, 0 ) )
	{
	  TranslateMessage( &message );
	  DispatchMessage( &message );
	}

	if (ThreadId)
	{
		HANDLE threadHandle = OpenThread(SYNCHRONIZE, FALSE, ThreadId);
		if (threadHandle)
		{
			WaitForSingleObject(threadHandle, INFINITE);
			CloseHandle(threadHandle);
		}
	}

	CloseDirectDraw();
	UnregisterClass("HugiExample", hInstance);
	return 0;
}

DWORD WINAPI mymain (void * argument)
{
	char * temp = (char*)malloc(320*240);
	if (temp == NULL)
	{
		/* Allocation failed; close window and exit thread gracefully */
		SendMessage (WindowHandle, WM_CLOSE, 0, 0);
		return 0;
	}

	for (int frames=0; frames<3200; frames++)
	{
		// draw an ugly pattern...
		int i=0;
		for (int y=0; y<240; y++)
		for (int x=0; x<320; x++)
		{
			temp[i++] =((x+frames)^y);
		}

		// and show it..
		EnterCriticalSection (&cr);
		if (ddSurface->lpVtbl->IsLost(ddSurface)!=DD_OK)
			ddSurface->lpVtbl->Restore(ddSurface);

		DDSCAPS caps;
		caps.dwCaps = DDSCAPS_BACKBUFFER;
		IDirectDrawSurface * backbuffer = 0;
		HRESULT hr = ddSurface->lpVtbl->GetAttachedSurface(ddSurface, &caps, &backbuffer);
		if (hr != DD_OK || backbuffer == 0)
		{
			LeaveCriticalSection (&cr);
			continue;
		}

		if (backbuffer->lpVtbl->IsLost(backbuffer)!=DD_OK)
		   backbuffer->lpVtbl->Restore(backbuffer);

		DDSURFACEDESC sd;
		memset (&sd, 0, sizeof (DDSURFACEDESC));
		sd.dwSize = sizeof (sd);
		hr = backbuffer->lpVtbl->Lock (backbuffer, 0, &sd, DDLOCK_SURFACEMEMORYPTR
			| DDLOCK_WAIT ,0);

		if (hr == DD_OK && sd.lpSurface)
		{
			char  *source = temp;
			char  *dest   = (char *) sd.lpSurface;

			for (int y=0; y<240; y++)
			{
				memcpy (dest, source, 320);
				dest   += sd.lPitch;
				source += 320;
			}

			backbuffer->lpVtbl->Unlock (backbuffer, sd.lpSurface);
			ddSurface->lpVtbl->Flip (ddSurface, 0, DDFLIP_WAIT );
		}
		
		backbuffer->lpVtbl->Release (backbuffer);
		LeaveCriticalSection (&cr);
	}
	
	free(temp);
	SendMessage (WindowHandle, WM_CLOSE, 0, 0);
	return 0;
}
