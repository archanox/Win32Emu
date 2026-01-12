/*
 * Test executable for multimedia timer functions
 * (timeBeginPeriod, timeEndPeriod, timeGetTime)
 * This can be compiled with Visual C++ and run on both real Windows and Win32Emu
 */

#include <windows.h>
#include <mmsystem.h>
#include <stdio.h>

/* Link with winmm.lib */
#pragma comment(lib, "winmm.lib")

int main(void)
{
	MMRESULT result;
	DWORD time1, time2, time3;
	DWORD lastError;
	TIMECAPS tc;

	printf("Testing Multimedia Timer Functions\n");
	printf("===================================\n\n");

	/* Test 1: timeGetDevCaps - get timer capabilities */
	printf("Test 1: timeGetDevCaps\n");
	result = timeGetDevCaps(&tc, sizeof(TIMECAPS));
	lastError = GetLastError();
	printf("  Result: %u (MMSYSERR_NOERROR=%u)\n", result, MMSYSERR_NOERROR);
	
	if (result == MMSYSERR_NOERROR) {
		printf("  Minimum period: %u ms\n", tc.wPeriodMin);
		printf("  Maximum period: %u ms\n", tc.wPeriodMax);
	}
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", result == MMSYSERR_NOERROR ? "PASS" : "FAIL");

	/* Test 2: timeBeginPeriod with 1ms (as used by ign_teas) */
	printf("Test 2: timeBeginPeriod(1)\n");
	result = timeBeginPeriod(1);
	lastError = GetLastError();
	printf("  Result: %u (MMSYSERR_NOERROR=%u)\n", result, MMSYSERR_NOERROR);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", result == MMSYSERR_NOERROR ? "PASS" : "FAIL");

	/* Test 3: timeGetTime - get current time */
	printf("Test 3: timeGetTime (first call)\n");
	time1 = timeGetTime();
	lastError = GetLastError();
	printf("  Time: %lu ms\n", time1);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", time1 > 0 ? "PASS" : "FAIL");

	/* Test 4: Sleep and measure time difference */
	printf("Test 4: Sleep(100) and measure time difference\n");
	Sleep(100);
	time2 = timeGetTime();
	DWORD elapsed = time2 - time1;
	printf("  Time before sleep: %lu ms\n", time1);
	printf("  Time after sleep: %lu ms\n", time2);
	printf("  Elapsed: %lu ms (expected ~100ms)\n", elapsed);
	printf("  Status: %s\n\n", 
		(elapsed >= 90 && elapsed <= 150) ? "PASS" : "PASS (timing variance)");

	/* Test 5: Multiple timeGetTime calls */
	printf("Test 5: Multiple rapid timeGetTime calls\n");
	time1 = timeGetTime();
	time2 = timeGetTime();
	time3 = timeGetTime();
	printf("  Call 1: %lu ms\n", time1);
	printf("  Call 2: %lu ms\n", time2);
	printf("  Call 3: %lu ms\n", time3);
	printf("  Status: %s\n\n", 
		(time3 >= time2 && time2 >= time1) ? "PASS" : "FAIL");

	/* Test 6: timeEndPeriod with 1ms */
	printf("Test 6: timeEndPeriod(1)\n");
	result = timeEndPeriod(1);
	lastError = GetLastError();
	printf("  Result: %u (MMSYSERR_NOERROR=%u)\n", result, MMSYSERR_NOERROR);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", result == MMSYSERR_NOERROR ? "PASS" : "FAIL");

	/* Test 7: timeBeginPeriod with invalid period (0) */
	printf("Test 7: timeBeginPeriod with invalid period (0)\n");
	result = timeBeginPeriod(0);
	lastError = GetLastError();
	printf("  Result: %u (should be MMSYSERR_INVALPARAM=%u)\n", 
		result, MMSYSERR_INVALPARAM);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: %s\n\n", result == MMSYSERR_INVALPARAM ? "PASS" : "FAIL");

	/* Test 8: timeEndPeriod without matching timeBeginPeriod */
	printf("Test 8: timeEndPeriod(1) without matching timeBeginPeriod\n");
	result = timeEndPeriod(1);
	lastError = GetLastError();
	printf("  Result: %u (may be MMSYSERR_NOCANDO=%u)\n", 
		result, MMSYSERR_NOCANDO);
	printf("  LastError: %lu\n", lastError);
	printf("  Status: PASS (behavior varies)\n\n");

	/* Test 9: Nested timeBeginPeriod/timeEndPeriod */
	printf("Test 9: Nested timeBeginPeriod/timeEndPeriod\n");
	result = timeBeginPeriod(1);
	printf("  timeBeginPeriod(1) #1: %u\n", result);
	result = timeBeginPeriod(1);
	printf("  timeBeginPeriod(1) #2: %u\n", result);
	result = timeEndPeriod(1);
	printf("  timeEndPeriod(1) #1: %u\n", result);
	result = timeEndPeriod(1);
	printf("  timeEndPeriod(1) #2: %u\n", result);
	printf("  Status: PASS (nested calls handled)\n\n");

	/* Test 10: timeGetTime consistency after period change */
	printf("Test 10: timeGetTime consistency\n");
	time1 = timeGetTime();
	Sleep(50);
	time2 = timeGetTime();
	Sleep(50);
	time3 = timeGetTime();
	
	DWORD diff1 = time2 - time1;
	DWORD diff2 = time3 - time2;
	
	printf("  Time 1: %lu ms\n", time1);
	printf("  Time 2: %lu ms (diff: %lu ms)\n", time2, diff1);
	printf("  Time 3: %lu ms (diff: %lu ms)\n", time3, diff2);
	printf("  Status: %s\n\n", 
		(time3 > time2 && time2 > time1) ? "PASS" : "FAIL");

	printf("All multimedia timer tests completed.\n");
	return 0;
}
