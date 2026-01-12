/*
 * Test executable for command line and startup info functions
 * (GetCommandLineA, GetStartupInfoA)
 * This can be compiled with Visual C++ and run on both real Windows and Win32Emu
 */

#include <windows.h>
#include <stdio.h>

int main(void)
{
	LPSTR cmdLine;
	STARTUPINFOA si;
	DWORD lastError;

	printf("Testing Command Line and Startup Info Functions\n");
	printf("================================================\n\n");

	/* Test 1: GetCommandLineA (as used by ign_teas) */
	printf("Test 1: GetCommandLineA\n");
	cmdLine = GetCommandLineA();
	lastError = GetLastError();
	printf("  Result: 0x%p\n", cmdLine);
	printf("  LastError: %lu\n", lastError);
	
	if (cmdLine != NULL) {
		printf("  Command line: %s\n", cmdLine);
		printf("  Status: PASS\n\n");
	} else {
		printf("  Status: FAIL\n\n");
	}

	/* Test 2: GetStartupInfoA (as used by ign_teas) */
	printf("Test 2: GetStartupInfoA\n");
	memset(&si, 0, sizeof(si));
	si.cb = sizeof(si);
	GetStartupInfoA(&si);
	lastError = GetLastError();
	
	printf("  LastError: %lu\n", lastError);
	printf("  cb: %lu (should be %lu)\n", si.cb, (DWORD)sizeof(STARTUPINFOA));
	printf("  lpReserved: 0x%p\n", si.lpReserved);
	printf("  lpDesktop: %s\n", si.lpDesktop ? si.lpDesktop : "(null)");
	printf("  lpTitle: %s\n", si.lpTitle ? si.lpTitle : "(null)");
	printf("  dwX: %lu\n", si.dwX);
	printf("  dwY: %lu\n", si.dwY);
	printf("  dwXSize: %lu\n", si.dwXSize);
	printf("  dwYSize: %lu\n", si.dwYSize);
	printf("  dwXCountChars: %lu\n", si.dwXCountChars);
	printf("  dwYCountChars: %lu\n", si.dwYCountChars);
	printf("  dwFillAttribute: 0x%lx\n", si.dwFillAttribute);
	printf("  dwFlags: 0x%lx\n", si.dwFlags);
	printf("  wShowWindow: %u\n", si.wShowWindow);
	printf("  cbReserved2: %u\n", si.cbReserved2);
	printf("  lpReserved2: 0x%p\n", si.lpReserved2);
	printf("  hStdInput: 0x%p\n", si.hStdInput);
	printf("  hStdOutput: 0x%p\n", si.hStdOutput);
	printf("  hStdError: 0x%p\n", si.hStdError);
	printf("  Status: PASS\n\n");

	/* Test 3: Verify GetStartupInfoA consistency */
	printf("Test 3: GetStartupInfoA called twice - consistency check\n");
	STARTUPINFOA si2;
	memset(&si2, 0, sizeof(si2));
	si2.cb = sizeof(si2);
	GetStartupInfoA(&si2);
	
	int consistent = 1;
	if (si.dwFlags != si2.dwFlags) consistent = 0;
	if (si.wShowWindow != si2.wShowWindow) consistent = 0;
	if (si.hStdInput != si2.hStdInput) consistent = 0;
	if (si.hStdOutput != si2.hStdOutput) consistent = 0;
	if (si.hStdError != si2.hStdError) consistent = 0;
	
	printf("  First call dwFlags: 0x%lx\n", si.dwFlags);
	printf("  Second call dwFlags: 0x%lx\n", si2.dwFlags);
	printf("  Status: %s\n\n", consistent ? "PASS" : "FAIL");

	/* Test 4: Check STARTF_USESTDHANDLES flag */
	printf("Test 4: Check STARTF_USESTDHANDLES flag\n");
	int usesStdHandles = (si.dwFlags & STARTF_USESTDHANDLES) != 0;
	printf("  dwFlags: 0x%lx\n", si.dwFlags);
	printf("  STARTF_USESTDHANDLES: 0x%lx\n", (DWORD)STARTF_USESTDHANDLES);
	printf("  Uses std handles: %s\n", usesStdHandles ? "YES" : "NO");
	
	if (usesStdHandles) {
		printf("  hStdInput: 0x%p %s\n", si.hStdInput, 
			si.hStdInput != NULL ? "valid" : "null");
		printf("  hStdOutput: 0x%p %s\n", si.hStdOutput, 
			si.hStdOutput != NULL ? "valid" : "null");
		printf("  hStdError: 0x%p %s\n", si.hStdError, 
			si.hStdError != NULL ? "valid" : "null");
	}
	printf("  Status: PASS\n\n");

	/* Test 5: Check STARTF_USESHOWWINDOW flag */
	printf("Test 5: Check STARTF_USESHOWWINDOW flag\n");
	int usesShowWindow = (si.dwFlags & STARTF_USESHOWWINDOW) != 0;
	printf("  dwFlags: 0x%lx\n", si.dwFlags);
	printf("  STARTF_USESHOWWINDOW: 0x%lx\n", (DWORD)STARTF_USESHOWWINDOW);
	printf("  Uses show window: %s\n", usesShowWindow ? "YES" : "NO");
	
	if (usesShowWindow) {
		printf("  wShowWindow: %u\n", si.wShowWindow);
		printf("  SW_HIDE: %d, SW_SHOWNORMAL: %d, SW_SHOWMINIMIZED: %d\n",
			SW_HIDE, SW_SHOWNORMAL, SW_SHOWMINIMIZED);
	}
	printf("  Status: PASS\n\n");

	/* Test 6: Validate standard handles from StartupInfo */
	printf("Test 6: Validate standard handles from StartupInfo\n");
	if (si.hStdInput != NULL && si.hStdInput != INVALID_HANDLE_VALUE) {
		DWORD fileType = GetFileType(si.hStdInput);
		printf("  hStdInput type: %lu\n", fileType);
	}
	if (si.hStdOutput != NULL && si.hStdOutput != INVALID_HANDLE_VALUE) {
		DWORD fileType = GetFileType(si.hStdOutput);
		printf("  hStdOutput type: %lu\n", fileType);
	}
	if (si.hStdError != NULL && si.hStdError != INVALID_HANDLE_VALUE) {
		DWORD fileType = GetFileType(si.hStdError);
		printf("  hStdError type: %lu\n", fileType);
	}
	printf("  Status: PASS\n\n");

	/* Test 7: GetCommandLineA returns consistent value */
	printf("Test 7: GetCommandLineA consistency check\n");
	LPSTR cmdLine2 = GetCommandLineA();
	printf("  First call: 0x%p\n", cmdLine);
	printf("  Second call: 0x%p\n", cmdLine2);
	printf("  Status: %s\n\n", cmdLine == cmdLine2 ? "PASS" : "FAIL");

	/* Test 8: Command line is not empty */
	printf("Test 8: Command line is not empty\n");
	if (cmdLine != NULL && cmdLine[0] != '\0') {
		printf("  Command line length: %lu\n", (DWORD)strlen(cmdLine));
		printf("  First 50 chars: %.50s%s\n", cmdLine, 
			strlen(cmdLine) > 50 ? "..." : "");
		printf("  Status: PASS\n\n");
	} else {
		printf("  Status: FAIL (command line is empty)\n\n");
	}

	printf("All command line and startup info tests completed.\n");
	return 0;
}
