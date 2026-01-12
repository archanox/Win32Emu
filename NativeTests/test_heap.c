/*
 * Test executable for Heap functions (HeapCreate, HeapAlloc, HeapFree)
 * This can be compiled with Visual C++ and run on both real Windows and Win32Emu
 */

#include <windows.h>
#include <stdio.h>
#include <string.h>

int main(void)
{
	HANDLE hHeap;
	LPVOID pMem1, pMem2, pMem3;
	DWORD lastError;
	BOOL success;

	printf("Testing Heap Functions\n");
	printf("======================\n\n");

	/* Test 1: Create a heap with default settings */
	printf("Test 1: HeapCreate with default settings\n");
	hHeap = HeapCreate(0, 4096, 0);
	lastError = GetLastError();
	printf("  Result: 0x%p\n", hHeap);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", hHeap != NULL ? "PASS" : "FAIL");

	if (hHeap == NULL) {
		printf("Failed to create heap, cannot continue tests.\n");
		return 1;
	}

	/* Test 2: Allocate memory from the heap */
	printf("Test 2: HeapAlloc - allocate 1024 bytes\n");
	pMem1 = HeapAlloc(hHeap, 0, 1024);
	lastError = GetLastError();
	printf("  Result: 0x%p\n", pMem1);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", pMem1 != NULL ? "PASS" : "FAIL");

	/* Test 3: Allocate memory with HEAP_ZERO_MEMORY flag */
	printf("Test 3: HeapAlloc with HEAP_ZERO_MEMORY (2048 bytes)\n");
	pMem2 = HeapAlloc(hHeap, HEAP_ZERO_MEMORY, 2048);
	lastError = GetLastError();
	printf("  Result: 0x%p\n", pMem2);
	printf("  LastError: %lu\n", lastError);
	
	/* Verify memory is zeroed */
	if (pMem2 != NULL) {
		unsigned char *ptr = (unsigned char *)pMem2;
		int isZeroed = 1;
		for (int i = 0; i < 2048; i++) {
			if (ptr[i] != 0) {
				isZeroed = 0;
				break;
			}
		}
		printf("  Memory is zeroed: %s\n", isZeroed ? "YES" : "NO");
		printf("  Status: %s\n\n", (pMem2 != NULL && isZeroed) ? "PASS" : "FAIL");
	} else {
		printf("  Status: FAIL\n\n");
	}

	/* Test 4: Write to and read from allocated memory */
	printf("Test 4: Write to and read from allocated memory\n");
	if (pMem1 != NULL) {
		const char *testString = "Hello from Win32Emu heap test!";
		strcpy((char *)pMem1, testString);
		int match = strcmp((char *)pMem1, testString) == 0;
		printf("  Written: %s\n", testString);
		printf("  Read: %s\n", (char *)pMem1);
		printf("  Status: %s\n\n", match ? "PASS" : "FAIL");
	} else {
		printf("  Status: SKIP (no memory allocated)\n\n");
	}

	/* Test 5: Allocate multiple blocks */
	printf("Test 5: Allocate multiple blocks (8416 bytes each)\n");
	pMem3 = HeapAlloc(hHeap, 0, 8416);
	printf("  Block 1: 0x%p\n", pMem1);
	printf("  Block 2: 0x%p\n", pMem2);
	printf("  Block 3: 0x%p\n", pMem3);
	printf("  Status: %s\n\n", 
		(pMem1 != NULL && pMem2 != NULL && pMem3 != NULL) ? "PASS" : "FAIL");

	/* Test 6: Free allocated memory */
	printf("Test 6: HeapFree - free allocated blocks\n");
	success = TRUE;
	
	if (pMem1 != NULL) {
		BOOL result = HeapFree(hHeap, 0, pMem1);
		printf("  Free block 1: %s\n", result ? "SUCCESS" : "FAILED");
		success = success && result;
	}
	
	if (pMem2 != NULL) {
		BOOL result = HeapFree(hHeap, 0, pMem2);
		printf("  Free block 2: %s\n", result ? "SUCCESS" : "FAILED");
		success = success && result;
	}
	
	if (pMem3 != NULL) {
		BOOL result = HeapFree(hHeap, 0, pMem3);
		printf("  Free block 3: %s\n", result ? "SUCCESS" : "FAILED");
		success = success && result;
	}
	
	printf("  Status: %s\n\n", success ? "PASS" : "FAIL");

	/* Test 7: Create heap with HEAP_NO_SERIALIZE flag (as used by ign_teas) */
	printf("Test 7: HeapCreate with HEAP_NO_SERIALIZE flag\n");
	HANDLE hHeap2 = HeapCreate(HEAP_NO_SERIALIZE, 4096, 0);
	lastError = GetLastError();
	printf("  Result: 0x%p\n", hHeap2);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", hHeap2 != NULL ? "PASS" : "FAIL");

	/* Test 8: Allocate from HEAP_NO_SERIALIZE heap */
	if (hHeap2 != NULL) {
		printf("Test 8: HeapAlloc from HEAP_NO_SERIALIZE heap\n");
		LPVOID pMem4 = HeapAlloc(hHeap2, 0, 4096);
		printf("  Result: 0x%p\n", pMem4);
		printf("  Status: %s\n\n", pMem4 != NULL ? "PASS" : "FAIL");
		
		if (pMem4 != NULL) {
			HeapFree(hHeap2, 0, pMem4);
		}
		
		HeapDestroy(hHeap2);
	}

	/* Test 9: Destroy the heap */
	printf("Test 9: HeapDestroy\n");
	success = HeapDestroy(hHeap);
	lastError = GetLastError();
	printf("  Result: %s\n", success ? "TRUE" : "FALSE");
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", success ? "PASS" : "FAIL");

	/* Test 10: Try to allocate from destroyed heap (should fail) */
	printf("Test 10: HeapAlloc from destroyed heap (should fail)\n");
	pMem1 = HeapAlloc(hHeap, 0, 1024);
	lastError = GetLastError();
	printf("  Result: 0x%p (should be NULL)\n", pMem1);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", pMem1 == NULL ? "PASS" : "FAIL");

	printf("All heap function tests completed.\n");
	return 0;
}
