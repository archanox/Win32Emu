/*
 * Test executable for window message functions
 * (GetMessageA, PeekMessageA, TranslateMessage, DispatchMessageA, PostMessageA, PostQuitMessage)
 * This can be compiled with Visual C++ and run on both real Windows and Win32Emu
 */

#include <windows.h>
#include <stdio.h>

LRESULT CALLBACK TestWndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);

static int g_customMessageReceived = 0;
static int g_quitMessageReceived = 0;

#define WM_CUSTOM_TEST (WM_USER + 100)

int main(void)
{
	WNDCLASSA wc = {0};
	HWND hwnd;
	MSG msg;
	BOOL result;
	DWORD lastError;

	printf("Testing Window Message Functions\n");
	printf("=================================\n\n");

	/* Test 1: Register a window class */
	printf("Test 1: RegisterClassA\n");
	wc.lpfnWndProc = TestWndProc;
	wc.hInstance = GetModuleHandleA(NULL);
	wc.lpszClassName = "TestMessageWindowClass";
	wc.hCursor = LoadCursorA(NULL, IDC_ARROW);
	
	ATOM classAtom = RegisterClassA(&wc);
	lastError = GetLastError();
	printf("  Result: 0x%04x\n", classAtom);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", classAtom != 0 ? "PASS" : "FAIL");

	if (classAtom == 0) {
		printf("Failed to register window class, cannot continue tests.\n");
		return 1;
	}

	/* Test 2: CreateWindowExA - create a message-only window */
	printf("Test 2: CreateWindowExA (message-only window)\n");
	hwnd = CreateWindowExA(0, "TestMessageWindowClass", "Test", 
		0, 0, 0, 0, 0, HWND_MESSAGE, NULL, wc.hInstance, NULL);
	lastError = GetLastError();
	printf("  Result: 0x%p\n", hwnd);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", hwnd != NULL ? "PASS" : "FAIL");

	if (hwnd == NULL) {
		printf("Failed to create window, cannot continue tests.\n");
		return 1;
	}

	/* Test 3: PostMessageA - post a custom message */
	printf("Test 3: PostMessageA (custom message WM_CUSTOM_TEST)\n");
	result = PostMessageA(hwnd, WM_CUSTOM_TEST, 123, 456);
	lastError = GetLastError();
	printf("  Result: %s\n", result ? "TRUE" : "FALSE");
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", result ? "PASS" : "FAIL");

	/* Test 4: PeekMessageA - check for message without removing */
	printf("Test 4: PeekMessageA (PM_NOREMOVE)\n");
	result = PeekMessageA(&msg, hwnd, 0, 0, PM_NOREMOVE);
	lastError = GetLastError();
	printf("  Result: %s (message available)\n", result ? "TRUE" : "FALSE");
	if (result) {
		printf("  Message: 0x%04x\n", msg.message);
		printf("  wParam: 0x%lx\n", (DWORD)msg.wParam);
		printf("  lParam: 0x%lx\n", msg.lParam);
	}
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", result ? "PASS" : "FAIL");

	/* Test 5: PeekMessageA - retrieve and remove message */
	printf("Test 5: PeekMessageA (PM_REMOVE)\n");
	result = PeekMessageA(&msg, hwnd, 0, 0, PM_REMOVE);
	lastError = GetLastError();
	printf("  Result: %s\n", result ? "TRUE" : "FALSE");
	if (result) {
		printf("  Message: 0x%04x (WM_CUSTOM_TEST=0x%04x)\n", 
			msg.message, WM_CUSTOM_TEST);
		printf("  wParam: %lu (expected 123)\n", (DWORD)msg.wParam);
		printf("  lParam: %lu (expected 456)\n", msg.lParam);
		
		/* Test 6: TranslateMessage */
		printf("\nTest 6: TranslateMessage\n");
		BOOL translated = TranslateMessage(&msg);
		printf("  Result: %s\n", translated ? "TRUE" : "FALSE");
		printf("  Status: PASS (TranslateMessage called)\n\n");
		
		/* Test 7: DispatchMessageA */
		printf("Test 7: DispatchMessageA\n");
		LRESULT dispatchResult = DispatchMessageA(&msg);
		printf("  Result: %ld\n", dispatchResult);
		printf("  Custom message received in WndProc: %s\n", 
			g_customMessageReceived ? "YES" : "NO");
		printf("  Status: %s\n\n", g_customMessageReceived ? "PASS" : "FAIL");
	} else {
		printf("  Status: FAIL (no message retrieved)\n\n");
		printf("Test 6: SKIP\n\n");
		printf("Test 7: SKIP\n\n");
	}

	/* Test 8: PostQuitMessage */
	printf("Test 8: PostQuitMessage\n");
	PostQuitMessage(42);
	printf("  Posted quit message with exit code 42\n");
	printf("  Status: PASS (call successful)\n\n");

	/* Test 9: PeekMessageA - retrieve quit message */
	printf("Test 9: PeekMessageA - retrieve WM_QUIT\n");
	result = PeekMessageA(&msg, NULL, 0, 0, PM_REMOVE);
	lastError = GetLastError();
	printf("  Result: %s\n", result ? "TRUE" : "FALSE");
	if (result) {
		printf("  Message: 0x%04x (WM_QUIT=0x%04x)\n", msg.message, WM_QUIT);
		printf("  wParam (exit code): %lu (expected 42)\n", (DWORD)msg.wParam);
		printf("  Status: %s\n\n", 
			(msg.message == WM_QUIT && msg.wParam == 42) ? "PASS" : "FAIL");
	} else {
		printf("  Status: FAIL (no message)\n\n");
	}

	/* Test 10: PeekMessageA with no messages (should return FALSE) */
	printf("Test 10: PeekMessageA with no messages available\n");
	result = PeekMessageA(&msg, hwnd, 0, 0, PM_REMOVE);
	lastError = GetLastError();
	printf("  Result: %s (should be FALSE)\n", result ? "TRUE" : "FALSE");
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", !result ? "PASS" : "FAIL");

	/* Test 11: PostMessageA to invalid window */
	printf("Test 11: PostMessageA to invalid window\n");
	result = PostMessageA((HWND)0xDEADBEEF, WM_CUSTOM_TEST, 0, 0);
	lastError = GetLastError();
	printf("  Result: %s (should be FALSE)\n", result ? "TRUE" : "FALSE");
	printf("  LastError: %lu (ERROR_INVALID_WINDOW_HANDLE=%d)\n", 
		lastError, ERROR_INVALID_WINDOW_HANDLE);
	printf("  Status: %s\n\n", !result ? "PASS" : "FAIL");

	/* Test 12: Broadcast message with PostMessageA */
	printf("Test 12: PostMessageA broadcast (HWND_BROADCAST)\n");
	result = PostMessageA(HWND_BROADCAST, WM_NULL, 0, 0);
	lastError = GetLastError();
	printf("  Result: %s\n", result ? "TRUE" : "FALSE");
	printf("  LastError: %lu\n", lastError);
	printf("  Status: PASS (broadcast attempted)\n\n");

	/* Cleanup */
	printf("Cleanup: Destroying window\n");
	DestroyWindow(hwnd);
	printf("\n");

	printf("All message function tests completed.\n");
	printf("Summary:\n");
	printf("  Custom messages received: %d\n", g_customMessageReceived);
	printf("  Quit messages received: %d\n", g_quitMessageReceived);
	
	return 0;
}

LRESULT CALLBACK TestWndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg) {
		case WM_CUSTOM_TEST:
			g_customMessageReceived = 1;
			return 0;
		
		case WM_QUIT:
			g_quitMessageReceived = 1;
			return 0;
		
		default:
			return DefWindowProcA(hwnd, msg, wParam, lParam);
	}
}
