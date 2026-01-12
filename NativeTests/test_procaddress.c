/*
 * Test executable for module and process address functions
 * (GetModuleHandleA, GetProcAddress, IsProcessorFeaturePresent)
 * This can be compiled with Visual C++ and run on both real Windows and Win32Emu
 */

#include <windows.h>
#include <stdio.h>

/* Function pointer types for testing GetProcAddress */
typedef DWORD (WINAPI *GetVersionFunc)(void);
typedef HANDLE (WINAPI *GetStdHandleFunc)(DWORD);

int main(void)
{
	HMODULE hKernel32, hUser32, hModule;
	FARPROC procAddr;
	DWORD lastError;
	BOOL result;

	printf("Testing Module and Process Address Functions\n");
	printf("=============================================\n\n");

	/* Test 1: GetModuleHandleA for KERNEL32 (as used by ign_teas) */
	printf("Test 1: GetModuleHandleA(\"KERNEL32\")\n");
	hKernel32 = GetModuleHandleA("KERNEL32");
	lastError = GetLastError();
	printf("  Result: 0x%p\n", hKernel32);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", hKernel32 != NULL ? "PASS" : "FAIL");

	/* Test 2: GetModuleHandleA for current module (NULL) */
	printf("Test 2: GetModuleHandleA(NULL) - current module\n");
	hModule = GetModuleHandleA(NULL);
	lastError = GetLastError();
	printf("  Result: 0x%p\n", hModule);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", hModule != NULL ? "PASS" : "FAIL");

	/* Test 3: GetModuleHandleA with case-insensitive name */
	printf("Test 3: GetModuleHandleA(\"kernel32.dll\") - case insensitive\n");
	HMODULE hKernel32Lower = GetModuleHandleA("kernel32.dll");
	lastError = GetLastError();
	printf("  Result: 0x%p\n", hKernel32Lower);
	printf("  Expected: 0x%p (same as KERNEL32)\n", hKernel32);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", hKernel32Lower == hKernel32 ? "PASS" : "FAIL");

	/* Test 4: GetProcAddress for known function */
	if (hKernel32 != NULL) {
		printf("Test 4: GetProcAddress(KERNEL32, \"GetVersion\")\n");
		procAddr = GetProcAddress(hKernel32, "GetVersion");
		lastError = GetLastError();
		printf("  Result: 0x%p\n", procAddr);
		printf("  LastError: %lu\n", lastError);
		printf("  Status: %s\n\n", procAddr != NULL ? "PASS" : "FAIL");
		
		/* Test 5: Call the function pointer */
		if (procAddr != NULL) {
			printf("Test 5: Call GetVersion through function pointer\n");
			GetVersionFunc getVersionPtr = (GetVersionFunc)procAddr;
			DWORD version = getVersionPtr();
			printf("  Version: 0x%08lx\n", version);
			printf("  Status: %s\n\n", version > 0 ? "PASS" : "FAIL");
		} else {
			printf("Test 5: SKIP (no function pointer)\n\n");
		}
	} else {
		printf("Test 4: SKIP (KERNEL32 not loaded)\n\n");
		printf("Test 5: SKIP\n\n");
	}

	/* Test 6: GetProcAddress for IsProcessorFeaturePresent (as used by ign_teas) */
	if (hKernel32 != NULL) {
		printf("Test 6: GetProcAddress(KERNEL32, \"IsProcessorFeaturePresent\")\n");
		procAddr = GetProcAddress(hKernel32, "IsProcessorFeaturePresent");
		lastError = GetLastError();
		printf("  Result: 0x%p\n", procAddr);
		printf("  LastError: %lu\n", lastError);
		printf("  Status: %s\n\n", procAddr != NULL ? "PASS" : "FAIL");
	} else {
		printf("Test 6: SKIP\n\n");
	}

	/* Test 7: IsProcessorFeaturePresent for various features */
	printf("Test 7: IsProcessorFeaturePresent(PF_FLOATING_POINT_EMULATED)\n");
	result = IsProcessorFeaturePresent(PF_FLOATING_POINT_EMULATED);
	lastError = GetLastError();
	printf("  Result: %s\n", result ? "TRUE" : "FALSE");
	printf("  LastError: %lu\n", lastError);
	printf("  Status: PASS\n\n");

	/* Test 8: IsProcessorFeaturePresent for floating point precision errata */
	printf("Test 8: IsProcessorFeaturePresent(PF_FLOATING_POINT_PRECISION_ERRATA)\n");
	result = IsProcessorFeaturePresent(PF_FLOATING_POINT_PRECISION_ERRATA);
	lastError = GetLastError();
	printf("  Result: %s\n", result ? "TRUE" : "FALSE");
	printf("  LastError: %lu\n", lastError);
	printf("  Status: PASS\n\n");

	/* Test 9: IsProcessorFeaturePresent for MMX */
	printf("Test 9: IsProcessorFeaturePresent(PF_MMX_INSTRUCTIONS_AVAILABLE)\n");
	result = IsProcessorFeaturePresent(PF_MMX_INSTRUCTIONS_AVAILABLE);
	lastError = GetLastError();
	printf("  Result: %s\n", result ? "TRUE" : "FALSE");
	printf("  LastError: %lu\n", lastError);
	printf("  Status: PASS\n\n");

	/* Test 10: GetProcAddress with invalid function name */
	if (hKernel32 != NULL) {
		printf("Test 10: GetProcAddress with invalid function name\n");
		procAddr = GetProcAddress(hKernel32, "NonExistentFunction12345");
		lastError = GetLastError();
		printf("  Result: 0x%p (should be NULL)\n", procAddr);
		printf("  LastError: %lu (ERROR_PROC_NOT_FOUND=%d)\n", 
			lastError, ERROR_PROC_NOT_FOUND);
		printf("  Status: %s\n\n", procAddr == NULL ? "PASS" : "FAIL");
	} else {
		printf("Test 10: SKIP\n\n");
	}

	/* Test 11: GetModuleHandleA for non-loaded module */
	printf("Test 11: GetModuleHandleA for non-loaded module\n");
	hModule = GetModuleHandleA("NonExistentModule12345.dll");
	lastError = GetLastError();
	printf("  Result: 0x%p (should be NULL)\n", hModule);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", hModule == NULL ? "PASS" : "FAIL");

	/* Test 12: GetModuleHandleA for USER32 */
	printf("Test 12: GetModuleHandleA(\"USER32\")\n");
	hUser32 = GetModuleHandleA("USER32");
	lastError = GetLastError();
	printf("  Result: 0x%p\n", hUser32);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", hUser32 != NULL ? "PASS" : "FAIL");

	/* Test 13: GetProcAddress from USER32 */
	if (hUser32 != NULL) {
		printf("Test 13: GetProcAddress(USER32, \"MessageBoxA\")\n");
		procAddr = GetProcAddress(hUser32, "MessageBoxA");
		lastError = GetLastError();
		printf("  Result: 0x%p\n", procAddr);
		printf("  LastError: %lu\n", lastError);
		printf("  Status: %s\n\n", procAddr != NULL ? "PASS" : "FAIL");
	} else {
		printf("Test 13: SKIP (USER32 not loaded)\n\n");
	}

	/* Test 14: GetProcAddress with invalid module handle */
	printf("Test 14: GetProcAddress with invalid module handle\n");
	procAddr = GetProcAddress((HMODULE)0xDEADBEEF, "GetVersion");
	lastError = GetLastError();
	printf("  Result: 0x%p (should be NULL)\n", procAddr);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", procAddr == NULL ? "PASS" : "FAIL");

	printf("All module and process address tests completed.\n");
	return 0;
}
