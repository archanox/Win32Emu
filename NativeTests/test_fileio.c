/*
 * Test executable for File I/O functions (CreateFileA, ReadFile, SetFilePointer, CloseHandle)
 * This can be compiled with Visual C++ and run on both real Windows and Win32Emu
 */

#include <windows.h>
#include <stdio.h>
#include <string.h>

int main(void)
{
	HANDLE hFile;
	DWORD bytesRead;
	DWORD lastError;
	DWORD fileType;
	DWORD newPos;
	BOOL success;
	char buffer[1024];
	const char *testFileName = "test_fileio_temp.txt";

	printf("Testing File I/O Functions\n");
	printf("===========================\n\n");

	/* Test 1: CreateFileA - create a new file for writing */
	printf("Test 1: CreateFileA - create new file\n");
	hFile = CreateFileA(testFileName, GENERIC_WRITE, 0, NULL, 
		CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
	lastError = GetLastError();
	printf("  Result: 0x%p\n", hFile);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", hFile != INVALID_HANDLE_VALUE ? "PASS" : "FAIL");

	/* Test 2: Write data to file */
	if (hFile != INVALID_HANDLE_VALUE) {
		printf("Test 2: WriteFile - write test data\n");
		const char *testData = "This is test data for Win32Emu file I/O testing.\n";
		DWORD bytesWritten;
		success = WriteFile(hFile, testData, (DWORD)strlen(testData), &bytesWritten, NULL);
		lastError = GetLastError();
		printf("  Bytes to write: %lu\n", (DWORD)strlen(testData));
		printf("  Bytes written: %lu\n", bytesWritten);
		printf("  LastError: %lu\n", lastError);
		printf("  Status: %s\n\n", 
			(success && bytesWritten == strlen(testData)) ? "PASS" : "FAIL");
		
		CloseHandle(hFile);
	} else {
		printf("Test 2: SKIP (file not created)\n\n");
	}

	/* Test 3: CreateFileA - open existing file for reading */
	printf("Test 3: CreateFileA - open existing file for reading\n");
	hFile = CreateFileA(testFileName, GENERIC_READ, FILE_SHARE_READ, NULL, 
		OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
	lastError = GetLastError();
	printf("  Result: 0x%p\n", hFile);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", hFile != INVALID_HANDLE_VALUE ? "PASS" : "FAIL");

	/* Test 4: GetFileType */
	if (hFile != INVALID_HANDLE_VALUE) {
		printf("Test 4: GetFileType\n");
		fileType = GetFileType(hFile);
		lastError = GetLastError();
		printf("  Result: %lu (FILE_TYPE_DISK=%d)\n", fileType, FILE_TYPE_DISK);
		printf("  LastError: %lu\n", lastError);
		printf("  Status: %s\n\n", fileType == FILE_TYPE_DISK ? "PASS" : "FAIL");
	} else {
		printf("Test 4: SKIP (file not opened)\n\n");
	}

	/* Test 5: ReadFile - read data from file */
	if (hFile != INVALID_HANDLE_VALUE) {
		printf("Test 5: ReadFile - read data from file\n");
		memset(buffer, 0, sizeof(buffer));
		success = ReadFile(hFile, buffer, sizeof(buffer) - 1, &bytesRead, NULL);
		lastError = GetLastError();
		printf("  Bytes read: %lu\n", bytesRead);
		printf("  LastError: %lu\n", lastError);
		printf("  Data read: %s", buffer);
		printf("  Status: %s\n\n", (success && bytesRead > 0) ? "PASS" : "FAIL");
	} else {
		printf("Test 5: SKIP (file not opened)\n\n");
	}

	/* Test 6: SetFilePointer - move to beginning */
	if (hFile != INVALID_HANDLE_VALUE) {
		printf("Test 6: SetFilePointer - move to beginning (FILE_BEGIN)\n");
		newPos = SetFilePointer(hFile, 0, NULL, FILE_BEGIN);
		lastError = GetLastError();
		printf("  New position: %lu (should be 0)\n", newPos);
		printf("  LastError: %lu\n", lastError);
		printf("  Status: %s\n\n", newPos == 0 ? "PASS" : "FAIL");
	} else {
		printf("Test 6: SKIP (file not opened)\n\n");
	}

	/* Test 7: SetFilePointer - move forward */
	if (hFile != INVALID_HANDLE_VALUE) {
		printf("Test 7: SetFilePointer - move forward 10 bytes (FILE_CURRENT)\n");
		newPos = SetFilePointer(hFile, 10, NULL, FILE_CURRENT);
		lastError = GetLastError();
		printf("  New position: %lu (should be 10)\n", newPos);
		printf("  LastError: %lu\n", lastError);
		printf("  Status: %s\n\n", newPos == 10 ? "PASS" : "FAIL");
	} else {
		printf("Test 7: SKIP (file not opened)\n\n");
	}

	/* Test 8: ReadFile from current position */
	if (hFile != INVALID_HANDLE_VALUE) {
		printf("Test 8: ReadFile from position 10\n");
		memset(buffer, 0, sizeof(buffer));
		success = ReadFile(hFile, buffer, 20, &bytesRead, NULL);
		lastError = GetLastError();
		printf("  Bytes read: %lu\n", bytesRead);
		printf("  Data read: %.20s\n", buffer);
		printf("  Status: %s\n\n", (success && bytesRead > 0) ? "PASS" : "FAIL");
	} else {
		printf("Test 8: SKIP (file not opened)\n\n");
	}

	/* Test 9: SetFilePointer to end */
	if (hFile != INVALID_HANDLE_VALUE) {
		printf("Test 9: SetFilePointer - move to end (FILE_END)\n");
		newPos = SetFilePointer(hFile, 0, NULL, FILE_END);
		lastError = GetLastError();
		printf("  File size: %lu bytes\n", newPos);
		printf("  LastError: %lu\n", lastError);
		printf("  Status: %s\n\n", newPos > 0 ? "PASS" : "FAIL");
	} else {
		printf("Test 9: SKIP (file not opened)\n\n");
	}

	/* Test 10: CloseHandle */
	if (hFile != INVALID_HANDLE_VALUE) {
		printf("Test 10: CloseHandle - close file handle\n");
		success = CloseHandle(hFile);
		lastError = GetLastError();
		printf("  Result: %s\n", success ? "TRUE" : "FALSE");
		printf("  LastError: %lu\n", lastError);
		printf("  Status: %s\n\n", success ? "PASS" : "FAIL");
	} else {
		printf("Test 10: SKIP (file not opened)\n\n");
	}

	/* Test 11: CreateFileA with invalid filename */
	printf("Test 11: CreateFileA - open non-existent file\n");
	hFile = CreateFileA("nonexistent_file_12345.txt", GENERIC_READ, 0, NULL, 
		OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
	lastError = GetLastError();
	printf("  Result: 0x%p (should be INVALID_HANDLE_VALUE)\n", hFile);
	printf("  LastError: %lu (ERROR_FILE_NOT_FOUND=%d)\n", lastError, ERROR_FILE_NOT_FOUND);
	printf("  Status: %s\n\n", 
		(hFile == INVALID_HANDLE_VALUE && lastError == ERROR_FILE_NOT_FOUND) ? "PASS" : "FAIL");

	/* Test 12: GetFileType with NULL handle */
	printf("Test 12: GetFileType with NULL handle\n");
	fileType = GetFileType(NULL);
	lastError = GetLastError();
	printf("  Result: %lu (FILE_TYPE_UNKNOWN=%d)\n", fileType, FILE_TYPE_UNKNOWN);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", fileType == FILE_TYPE_UNKNOWN ? "PASS" : "FAIL");

	/* Cleanup: delete test file */
	printf("Cleanup: Deleting test file\n");
	success = DeleteFileA(testFileName);
	printf("  DeleteFileA result: %s\n", success ? "SUCCESS" : "FAILED");
	printf("\n");

	printf("All file I/O tests completed.\n");
	return 0;
}
