/*
 * Simple DirectDraw Example for Win32Emu
 * Tests basic DirectDraw initialization and surface creation
 * Simpler than Hugi example - no threading
 */

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <ddraw.h>

static IDirectDraw *dd = NULL;
static IDirectDraw2 *dd2 = NULL;
static IDirectDrawSurface *ddPrimarySurface = NULL;
static HWND g_hwnd = NULL;
static int g_frame = 0;

void Cleanup(void)
{
	if (ddPrimarySurface) {
		ddPrimarySurface->lpVtbl->Release(ddPrimarySurface);
		ddPrimarySurface = NULL;
	}
	if (dd2) {
		dd2->lpVtbl->Release(dd2);
		dd2 = NULL;
	}
	if (dd) {
		dd->lpVtbl->Release(dd);
		dd = NULL;
	}
}

BOOL InitDirectDraw(HWND hwnd)
{
	HRESULT hr;
	DDSURFACEDESC ddsd;

	// Create DirectDraw object
	hr = DirectDrawCreate(NULL, &dd, NULL);
	if (FAILED(hr)) {
		MessageBox(hwnd, "DirectDrawCreate failed", "Error", MB_OK);
		return FALSE;
	}

	// Set cooperative level
	hr = dd->lpVtbl->SetCooperativeLevel(dd, hwnd, DDSCL_NORMAL);
	if (FAILED(hr)) {
		MessageBox(hwnd, "SetCooperativeLevel failed", "Error", MB_OK);
		Cleanup();
		return FALSE;
	}

	// Query for IDirectDraw2 interface
	hr = dd->lpVtbl->QueryInterface(dd, &IID_IDirectDraw2, (void**)&dd2);
	if (FAILED(hr)) {
		MessageBox(hwnd, "QueryInterface failed", "Error", MB_OK);
		Cleanup();
		return FALSE;
	}

	// Create primary surface
	memset(&ddsd, 0, sizeof(ddsd));
	ddsd.dwSize = sizeof(ddsd);
	ddsd.dwFlags = DDSD_CAPS;
	ddsd.ddsCaps.dwCaps = DDSCAPS_PRIMARYSURFACE;

	hr = dd2->lpVtbl->CreateSurface(dd2, &ddsd, &ddPrimarySurface, NULL);
	if (FAILED(hr)) {
		MessageBox(hwnd, "CreateSurface failed", "Error", MB_OK);
		Cleanup();
		return FALSE;
	}

	return TRUE;
}

void DrawFrame(void)
{
	DDSURFACEDESC ddsd;
	HRESULT hr;
	int x, y;
	unsigned char *dest;

	if (!ddPrimarySurface)
		return;

	// Check if surface is lost
	if (ddPrimarySurface->lpVtbl->IsLost(ddPrimarySurface) != DD_OK) {
		ddPrimarySurface->lpVtbl->Restore(ddPrimarySurface);
	}

	// Lock the surface
	memset(&ddsd, 0, sizeof(ddsd));
	ddsd.dwSize = sizeof(ddsd);
	hr = ddPrimarySurface->lpVtbl->Lock(ddPrimarySurface, NULL, &ddsd, 
		DDLOCK_SURFACEMEMORYPTR | DDLOCK_WAIT, NULL);
	
	if (FAILED(hr))
		return;

	// Draw a simple animated pattern
	dest = (unsigned char*)ddsd.lpSurface;
	for (y = 0; y < 200 && y < (int)ddsd.dwHeight; y++) {
		unsigned char *row = dest + y * ddsd.lPitch;
		for (x = 0; x < 320 && x < (int)ddsd.dwWidth; x++) {
			// Create an animated pattern
			row[x] = (unsigned char)((x + g_frame) ^ y);
		}
	}

	// Unlock the surface
	ddPrimarySurface->lpVtbl->Unlock(ddPrimarySurface, NULL);
	
	g_frame++;
}

LRESULT CALLBACK WindowProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg) {
		case WM_CREATE:
			g_hwnd = hwnd;
			if (!InitDirectDraw(hwnd)) {
				return -1;
			}
			SetTimer(hwnd, 1, 50, NULL); // 20 FPS timer
			break;

		case WM_TIMER:
			DrawFrame();
			break;

		case WM_KEYDOWN:
			if (wParam == VK_ESCAPE) {
				PostQuitMessage(0);
			}
			break;

		case WM_DESTROY:
			KillTimer(hwnd, 1);
			Cleanup();
			PostQuitMessage(0);
			break;

		default:
			return DefWindowProc(hwnd, msg, wParam, lParam);
	}
	return 0;
}

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance,
				   LPSTR lpCmdLine, int nCmdShow)
{
	WNDCLASS wc;
	HWND hwnd;
	MSG msg;

	// Register window class
	memset(&wc, 0, sizeof(wc));
	wc.lpfnWndProc = WindowProc;
	wc.hInstance = hInstance;
	wc.hbrBackground = (HBRUSH)GetStockObject(BLACK_BRUSH);
	wc.lpszClassName = "SimpleDirectDraw";
	wc.hCursor = LoadCursor(NULL, IDC_ARROW);

	if (!RegisterClass(&wc)) {
		MessageBox(NULL, "RegisterClass failed", "Error", MB_OK);
		return 1;
	}

	// Create window
	hwnd = CreateWindowEx(
		0,
		"SimpleDirectDraw",
		"Simple DirectDraw Test",
		WS_OVERLAPPEDWINDOW,
		CW_USEDEFAULT, CW_USEDEFAULT,
		640, 480,
		NULL, NULL,
		hInstance,
		NULL
	);

	if (!hwnd) {
		MessageBox(NULL, "CreateWindow failed", "Error", MB_OK);
		return 1;
	}

	ShowWindow(hwnd, nCmdShow);
	UpdateWindow(hwnd);

	// Message loop
	while (GetMessage(&msg, NULL, 0, 0)) {
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

	return (int)msg.wParam;
}
