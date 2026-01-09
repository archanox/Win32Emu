/*
 * Test executable for environment variable functions
 * This can be compiled with Visual C++ and run on both real Windows and Win32Emu
 */

#include <windows.h>
#include <stdio.h>
#include <string.h>

int main(void)
{
	char buffer[1024];
	DWORD result;
	DWORD lastError;
	BOOL success;

	printf("Testing Environment Variable Functions\n");
	printf("======================================\n\n");

	/* Test 1: Get existing environment variable (PATH) */
	printf("Test 1: GetEnvironmentVariableA for PATH\n");
	memset(buffer, 0, sizeof(buffer));
	result = GetEnvironmentVariableA("PATH", buffer, sizeof(buffer));
	lastError = GetLastError();
	printf("  Result: %lu\n", result);
	printf("  Buffer: %.100s%s\n", buffer, result > 100 ? "..." : "");
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", result > 0 ? "PASS" : "FAIL");

	/* Test 2: Get non-existent environment variable */
	printf("Test 2: GetEnvironmentVariableA for non-existent variable\n");
	memset(buffer, 0, sizeof(buffer));
	result = GetEnvironmentVariableA("NONEXISTENT_VAR_12345", buffer, sizeof(buffer));
	lastError = GetLastError();
	printf("  Result: %lu (should be 0)\n", result);
	printf("  LastError: %lu (should be %lu for ERROR_ENVVAR_NOT_FOUND)\n", 
		lastError, (DWORD)ERROR_ENVVAR_NOT_FOUND);
	printf("  Status: %s\n\n", (result == 0 && lastError == ERROR_ENVVAR_NOT_FOUND) ? "PASS" : "FAIL");

	/* Test 3: Set and get a new environment variable */
	printf("Test 3: SetEnvironmentVariableA and GetEnvironmentVariableA\n");
	success = SetEnvironmentVariableA("TEST_VAR_123", "TestValue456");
	lastError = GetLastError();
	printf("  SetEnvironmentVariableA Result: %d\n", success);
	printf("  LastError after set: %lu\n", lastError);
	
	if (success) {
		memset(buffer, 0, sizeof(buffer));
		result = GetEnvironmentVariableA("TEST_VAR_123", buffer, sizeof(buffer));
		lastError = GetLastError();
		printf("  GetEnvironmentVariableA Result: %lu\n", result);
		printf("  Buffer: %s\n", buffer);
		printf("  LastError after get: %lu\n", lastError);
		printf("  Status: %s\n\n", 
			(result > 0 && strcmp(buffer, "TestValue456") == 0) ? "PASS" : "FAIL");
	} else {
		printf("  Status: FAIL (SetEnvironmentVariableA failed)\n\n");
	}

	/* Test 4: Update existing environment variable */
	printf("Test 4: Update existing environment variable\n");
	success = SetEnvironmentVariableA("TEST_VAR_123", "UpdatedValue789");
	lastError = GetLastError();
	printf("  SetEnvironmentVariableA Result: %d\n", success);
	
	if (success) {
		memset(buffer, 0, sizeof(buffer));
		result = GetEnvironmentVariableA("TEST_VAR_123", buffer, sizeof(buffer));
		printf("  GetEnvironmentVariableA Result: %lu\n", result);
		printf("  Buffer: %s\n", buffer);
		printf("  Status: %s\n\n", 
			(result > 0 && strcmp(buffer, "UpdatedValue789") == 0) ? "PASS" : "FAIL");
	} else {
		printf("  Status: FAIL\n\n");
	}

	/* Test 5: Delete environment variable */
	printf("Test 5: Delete environment variable with NULL value\n");
	success = SetEnvironmentVariableA("TEST_VAR_123", NULL);
	lastError = GetLastError();
	printf("  SetEnvironmentVariableA(NULL) Result: %d\n", success);
	
	if (success) {
		memset(buffer, 0, sizeof(buffer));
		result = GetEnvironmentVariableA("TEST_VAR_123", buffer, sizeof(buffer));
		lastError = GetLastError();
		printf("  GetEnvironmentVariableA Result: %lu (should be 0)\n", result);
		printf("  LastError: %lu (should be %lu for ERROR_ENVVAR_NOT_FOUND)\n", 
			lastError, (DWORD)ERROR_ENVVAR_NOT_FOUND);
		printf("  Status: %s\n\n", 
			(result == 0 && lastError == ERROR_ENVVAR_NOT_FOUND) ? "PASS" : "FAIL");
	} else {
		printf("  Status: FAIL\n\n");
	}

	/* Test 6: Get environment variable with insufficient buffer */
	printf("Test 6: GetEnvironmentVariableA with buffer size 5\n");
	memset(buffer, 0, sizeof(buffer));
	result = GetEnvironmentVariableA("PATH", buffer, 5);
	lastError = GetLastError();
	printf("  Result: %lu (should be required buffer size)\n", result);
	printf("  Buffer: %s\n", buffer);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", result > 5 ? "PASS" : "FAIL");

	/* Test 7: Get environment variable with NULL buffer to query size */
	printf("Test 7: GetEnvironmentVariableA with NULL buffer\n");
	result = GetEnvironmentVariableA("PATH", NULL, 0);
	lastError = GetLastError();
	printf("  Result: %lu (required buffer size including null terminator)\n", result);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", result > 0 ? "PASS" : "FAIL");

	/* Test 8: Set environment variable with empty name */
	printf("Test 8: SetEnvironmentVariableA with empty name\n");
	success = SetEnvironmentVariableA("", "SomeValue");
	lastError = GetLastError();
	printf("  Result: %d (should be 0/FALSE)\n", success);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", success == 0 ? "PASS" : "FAIL");

	printf("All environment variable tests completed.\n");
	return 0;
}
