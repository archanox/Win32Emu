/*
 * Test executable for version and code page functions
 * (GetVersion, GetACP, GetCPInfo, SetHandleCount, GetStdHandle)
 * This can be compiled with Visual C++ and run on both real Windows and Win32Emu
 */

#include <windows.h>
#include <stdio.h>

int main(void)
{
	DWORD version;
	DWORD lastError;
	UINT acp;
	CPINFO cpInfo;
	BOOL success;
	UINT handleCount;
	HANDLE hStdIn, hStdOut, hStdErr;

	printf("Testing Version and System Functions\n");
	printf("=====================================\n\n");

	/* Test 1: GetVersion (as used by ign_teas) */
	printf("Test 1: GetVersion\n");
	version = GetVersion();
	lastError = GetLastError();
	
	DWORD majorVersion = (DWORD)(LOBYTE(LOWORD(version)));
	DWORD minorVersion = (DWORD)(HIBYTE(LOWORD(version)));
	DWORD buildNumber = 0;
	
	if (version < 0x80000000) {
		buildNumber = (DWORD)(HIWORD(version));
	}
	
	printf("  Raw version: 0x%08lx\n", version);
	printf("  Major version: %lu\n", majorVersion);
	printf("  Minor version: %lu\n", minorVersion);
	printf("  Build number: %lu\n", buildNumber);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", majorVersion > 0 ? "PASS" : "FAIL");

	/* Test 2: GetACP - get active code page */
	printf("Test 2: GetACP (Active Code Page)\n");
	acp = GetACP();
	lastError = GetLastError();
	printf("  Active Code Page: %u\n", acp);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", acp > 0 ? "PASS" : "FAIL");

	/* Test 3: GetCPInfo for current code page */
	printf("Test 3: GetCPInfo for current code page\n");
	success = GetCPInfo(CP_ACP, &cpInfo);
	lastError = GetLastError();
	printf("  Result: %s\n", success ? "TRUE" : "FALSE");
	printf("  LastError: %lu\n", lastError);
	
	if (success) {
		printf("  MaxCharSize: %u\n", cpInfo.MaxCharSize);
		printf("  DefaultChar: [");
		for (int i = 0; i < MAX_DEFAULTCHAR; i++) {
			printf("%02X", (unsigned char)cpInfo.DefaultChar[i]);
			if (i < MAX_DEFAULTCHAR - 1) printf(" ");
		}
		printf("]\n");
		printf("  LeadByte: [");
		for (int i = 0; i < MAX_LEADBYTES; i++) {
			printf("%02X", (unsigned char)cpInfo.LeadByte[i]);
			if (i < MAX_LEADBYTES - 1 && cpInfo.LeadByte[i] != 0) printf(" ");
			if (cpInfo.LeadByte[i] == 0) break;
		}
		printf("]\n");
	}
	printf("  Status: %s\n\n", success ? "PASS" : "FAIL");

	/* Test 4: GetCPInfo for UTF-8 (CP_UTF8) */
	printf("Test 4: GetCPInfo for UTF-8 (CP_UTF8)\n");
	success = GetCPInfo(CP_UTF8, &cpInfo);
	lastError = GetLastError();
	printf("  Result: %s\n", success ? "TRUE" : "FALSE");
	printf("  LastError: %lu\n", lastError);
	
	if (success) {
		printf("  MaxCharSize: %u (should be 4 for UTF-8)\n", cpInfo.MaxCharSize);
	}
	printf("  Status: %s\n\n", success ? "PASS" : "FAIL");

	/* Test 5: SetHandleCount (legacy function as used by ign_teas) */
	printf("Test 5: SetHandleCount (legacy function)\n");
	handleCount = SetHandleCount(32);
	lastError = GetLastError();
	printf("  Requested: 32\n");
	printf("  Result: %u\n", handleCount);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", handleCount >= 32 ? "PASS" : "FAIL");

	/* Test 6: GetStdHandle - get standard input handle */
	printf("Test 6: GetStdHandle(STD_INPUT_HANDLE)\n");
	hStdIn = GetStdHandle(STD_INPUT_HANDLE);
	lastError = GetLastError();
	printf("  Result: 0x%p\n", hStdIn);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", hStdIn != INVALID_HANDLE_VALUE ? "PASS" : "FAIL");

	/* Test 7: GetStdHandle - get standard output handle */
	printf("Test 7: GetStdHandle(STD_OUTPUT_HANDLE)\n");
	hStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
	lastError = GetLastError();
	printf("  Result: 0x%p\n", hStdOut);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", hStdOut != INVALID_HANDLE_VALUE ? "PASS" : "FAIL");

	/* Test 8: GetStdHandle - get standard error handle */
	printf("Test 8: GetStdHandle(STD_ERROR_HANDLE)\n");
	hStdErr = GetStdHandle(STD_ERROR_HANDLE);
	lastError = GetLastError();
	printf("  Result: 0x%p\n", hStdErr);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", hStdErr != INVALID_HANDLE_VALUE ? "PASS" : "FAIL");

	/* Test 9: GetFileType on standard handles */
	printf("Test 9: GetFileType on standard output handle\n");
	if (hStdOut != NULL && hStdOut != INVALID_HANDLE_VALUE) {
		DWORD fileType = GetFileType(hStdOut);
		lastError = GetLastError();
		printf("  Result: %lu\n", fileType);
		printf("  Type: ");
		switch (fileType) {
			case FILE_TYPE_UNKNOWN: printf("FILE_TYPE_UNKNOWN"); break;
			case FILE_TYPE_DISK: printf("FILE_TYPE_DISK"); break;
			case FILE_TYPE_CHAR: printf("FILE_TYPE_CHAR"); break;
			case FILE_TYPE_PIPE: printf("FILE_TYPE_PIPE"); break;
			default: printf("UNKNOWN(%lu)", fileType); break;
		}
		printf("\n");
		printf("  LastError: %lu\n", lastError);
		printf("  Status: PASS\n\n");
	} else {
		printf("  Status: SKIP (no valid handle)\n\n");
	}

	/* Test 10: GetCPInfo with invalid code page */
	printf("Test 10: GetCPInfo with invalid code page (99999)\n");
	success = GetCPInfo(99999, &cpInfo);
	lastError = GetLastError();
	printf("  Result: %s (should be FALSE)\n", success ? "TRUE" : "FALSE");
	printf("  LastError: %lu (ERROR_INVALID_PARAMETER=%d)\n", 
		lastError, ERROR_INVALID_PARAMETER);
	printf("  Status: %s\n\n", !success ? "PASS" : "FAIL");

	printf("All version and system function tests completed.\n");
	return 0;
}
