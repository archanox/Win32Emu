/*
 * Test executable for GetModuleFileNameA
 * This can be compiled with Visual C++ and run on both real Windows and Win32Emu
 */

#include <windows.h>
#include <stdio.h>
#include <string.h>

int main(void)
{
	char buffer[MAX_PATH];
	DWORD result;
	DWORD lastError;

	printf("Testing GetModuleFileNameA\n");
	printf("==========================\n\n");

	/* Test 1: Get current module filename with NULL handle */
	printf("Test 1: GetModuleFileNameA with NULL handle\n");
	memset(buffer, 0, sizeof(buffer));
	result = GetModuleFileNameA(NULL, buffer, MAX_PATH);
	lastError = GetLastError();
	printf("  Result: %lu\n", result);
	printf("  Buffer: %s\n", buffer);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", result > 0 ? "PASS" : "FAIL");

	/* Test 2: Get current module filename with buffer size too small */
	printf("Test 2: GetModuleFileNameA with buffer size 10\n");
	memset(buffer, 0, sizeof(buffer));
	result = GetModuleFileNameA(NULL, buffer, 10);
	lastError = GetLastError();
	printf("  Result: %lu\n", result);
	printf("  Buffer: %s\n", buffer);
	printf("  LastError: %lu (should be %lu for ERROR_INSUFFICIENT_BUFFER)\n", 
		lastError, (DWORD)ERROR_INSUFFICIENT_BUFFER);
	printf("  Status: %s\n\n", (result > 0 && result < 10) ? "PASS" : "FAIL");

	/* Test 3: Get kernel32.dll module handle and filename */
	printf("Test 3: GetModuleFileNameA for kernel32.dll\n");
	HMODULE hKernel32 = GetModuleHandleA("kernel32.dll");
	if (hKernel32 != NULL) {
		memset(buffer, 0, sizeof(buffer));
		result = GetModuleFileNameA(hKernel32, buffer, MAX_PATH);
		lastError = GetLastError();
		printf("  Kernel32 Handle: 0x%p\n", hKernel32);
		printf("  Result: %lu\n", result);
		printf("  Buffer: %s\n", buffer);
		printf("  LastError: %lu\n", lastError);
		printf("  Status: %s\n\n", result > 0 ? "PASS" : "FAIL");
	} else {
		printf("  Failed to get kernel32.dll handle\n");
		printf("  Status: FAIL\n\n");
	}

	/* Test 4: Get module filename with zero buffer size */
	printf("Test 4: GetModuleFileNameA with buffer size 0\n");
	result = GetModuleFileNameA(NULL, buffer, 0);
	lastError = GetLastError();
	printf("  Result: %lu (should be 0)\n", result);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", result == 0 ? "PASS" : "FAIL");

	/* Test 5: Invalid module handle */
	printf("Test 5: GetModuleFileNameA with invalid handle 0xFFFFFFFF\n");
	memset(buffer, 0, sizeof(buffer));
	result = GetModuleFileNameA((HMODULE)0xFFFFFFFF, buffer, MAX_PATH);
	lastError = GetLastError();
	printf("  Result: %lu (should be 0)\n", result);
	printf("  LastError: %lu (should be %lu for ERROR_INVALID_PARAMETER)\n", 
		lastError, (DWORD)ERROR_INVALID_PARAMETER);
	printf("  Status: %s\n\n", (result == 0 && lastError == ERROR_INVALID_PARAMETER) ? "PASS" : "FAIL");

	printf("All GetModuleFileNameA tests completed.\n");
	return 0;
}
