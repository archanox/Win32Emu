/*
 * Test executable for VirtualAlloc function
 * This can be compiled with Visual C++ and run on both real Windows and Win32Emu
 */

#include <windows.h>
#include <stdio.h>
#include <string.h>

int main(void)
{
	LPVOID pMem1, pMem2, pMem3;
	DWORD lastError;
	BOOL success;
	MEMORY_BASIC_INFORMATION memInfo;

	printf("Testing VirtualAlloc Function\n");
	printf("==============================\n\n");

	/* Test 1: Reserve memory (as used by ign_teas) */
	printf("Test 1: VirtualAlloc - reserve 4MB (MEM_RESERVE)\n");
	pMem1 = VirtualAlloc(NULL, 4194304, MEM_RESERVE, PAGE_READWRITE);
	lastError = GetLastError();
	printf("  Result: 0x%p\n", pMem1);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", pMem1 != NULL ? "PASS" : "FAIL");

	/* Test 2: Commit memory from reserved region (as used by ign_teas) */
	if (pMem1 != NULL) {
		printf("Test 2: VirtualAlloc - commit 64KB from reserved region\n");
		pMem2 = VirtualAlloc(pMem1, 65536, MEM_COMMIT, PAGE_READWRITE);
		lastError = GetLastError();
		printf("  Result: 0x%p\n", pMem2);
		printf("  Expected: 0x%p (same as reserved address)\n", pMem1);
		printf("  LastError: %lu\n", lastError);
		printf("  Status: %s\n\n", pMem2 != NULL ? "PASS" : "FAIL");
	} else {
		printf("Test 2: SKIP (no reserved memory)\n\n");
	}

	/* Test 3: Query memory information */
	if (pMem2 != NULL) {
		printf("Test 3: VirtualQuery on committed memory\n");
		SIZE_T result = VirtualQuery(pMem2, &memInfo, sizeof(memInfo));
		printf("  Result: %lu bytes\n", (DWORD)result);
		printf("  BaseAddress: 0x%p\n", memInfo.BaseAddress);
		printf("  AllocationBase: 0x%p\n", memInfo.AllocationBase);
		printf("  AllocationProtect: 0x%lx\n", memInfo.AllocationProtect);
		printf("  RegionSize: %lu bytes\n", (DWORD)memInfo.RegionSize);
		printf("  State: 0x%lx (MEM_COMMIT=0x%x)\n", memInfo.State, MEM_COMMIT);
		printf("  Protect: 0x%lx (PAGE_READWRITE=0x%x)\n", memInfo.Protect, PAGE_READWRITE);
		printf("  Type: 0x%lx (MEM_PRIVATE=0x%x)\n", memInfo.Type, MEM_PRIVATE);
		printf("  Status: %s\n\n", 
			(result > 0 && memInfo.State == MEM_COMMIT) ? "PASS" : "FAIL");
	} else {
		printf("Test 3: SKIP (no committed memory)\n\n");
	}

	/* Test 4: Write to and read from committed memory */
	if (pMem2 != NULL) {
		printf("Test 4: Write to and read from committed memory\n");
		const char *testString = "VirtualAlloc test data - Win32Emu";
		strcpy((char *)pMem2, testString);
		int match = strcmp((char *)pMem2, testString) == 0;
		printf("  Written: %s\n", testString);
		printf("  Read: %s\n", (char *)pMem2);
		printf("  Status: %s\n\n", match ? "PASS" : "FAIL");
	} else {
		printf("Test 4: SKIP (no committed memory)\n\n");
	}

	/* Test 5: Reserve and commit in one call */
	printf("Test 5: VirtualAlloc - reserve and commit in one call (1MB)\n");
	pMem3 = VirtualAlloc(NULL, 1048576, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
	lastError = GetLastError();
	printf("  Result: 0x%p\n", pMem3);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", pMem3 != NULL ? "PASS" : "FAIL");

	/* Test 6: Write to memory allocated with reserve+commit */
	if (pMem3 != NULL) {
		printf("Test 6: Write to memory allocated with reserve+commit\n");
		memset(pMem3, 0xAB, 1024);
		unsigned char *ptr = (unsigned char *)pMem3;
		int allMatch = 1;
		for (int i = 0; i < 1024; i++) {
			if (ptr[i] != 0xAB) {
				allMatch = 0;
				break;
			}
		}
		printf("  Written: 1024 bytes of 0xAB\n");
		printf("  Verified: %s\n", allMatch ? "ALL BYTES MATCH" : "MISMATCH");
		printf("  Status: %s\n\n", allMatch ? "PASS" : "FAIL");
	} else {
		printf("Test 6: SKIP (no memory allocated)\n\n");
	}

	/* Test 7: Free committed memory with VirtualFree */
	if (pMem3 != NULL) {
		printf("Test 7: VirtualFree - free committed memory\n");
		success = VirtualFree(pMem3, 0, MEM_RELEASE);
		lastError = GetLastError();
		printf("  Result: %s\n", success ? "TRUE" : "FALSE");
		printf("  LastError: %lu\n", lastError);
		printf("  Status: %s\n\n", success ? "PASS" : "FAIL");
	} else {
		printf("Test 7: SKIP (no memory to free)\n\n");
	}

	/* Test 8: Free reserved memory with VirtualFree */
	if (pMem1 != NULL) {
		printf("Test 8: VirtualFree - free reserved memory\n");
		success = VirtualFree(pMem1, 0, MEM_RELEASE);
		lastError = GetLastError();
		printf("  Result: %s\n", success ? "TRUE" : "FALSE");
		printf("  LastError: %lu\n", lastError);
		printf("  Status: %s\n\n", success ? "PASS" : "FAIL");
	} else {
		printf("Test 8: SKIP (no memory to free)\n\n");
	}

	/* Test 9: Allocate with specific address (should succeed if available) */
	printf("Test 9: VirtualAlloc with specific address hint\n");
	LPVOID pDesired = (LPVOID)0x0b900000;
	LPVOID pMem4 = VirtualAlloc(pDesired, 65536, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
	lastError = GetLastError();
	printf("  Requested address: 0x%p\n", pDesired);
	printf("  Actual address: 0x%p\n", pMem4);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", pMem4 != NULL ? "PASS" : "FAIL");
	
	if (pMem4 != NULL) {
		VirtualFree(pMem4, 0, MEM_RELEASE);
	}

	/* Test 10: Test allocation with PAGE_EXECUTE_READWRITE */
	printf("Test 10: VirtualAlloc with PAGE_EXECUTE_READWRITE\n");
	LPVOID pMem5 = VirtualAlloc(NULL, 4096, MEM_RESERVE | MEM_COMMIT, PAGE_EXECUTE_READWRITE);
	lastError = GetLastError();
	printf("  Result: 0x%p\n", pMem5);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", pMem5 != NULL ? "PASS" : "FAIL");
	
	if (pMem5 != NULL) {
		VirtualFree(pMem5, 0, MEM_RELEASE);
	}

	printf("All VirtualAlloc tests completed.\n");
	return 0;
}
