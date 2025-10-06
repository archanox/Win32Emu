# API Implementation Status for Ignition (1997)

This document provides a detailed status of which DLL imports required by Ignition (1997) are implemented in the Win32Emu codebase.

## Summary

- **Total APIs**: 100
- **Implemented**: 74
- **Not Implemented**: 26
- **Implementation Rate**: 74%

## Detailed Status by DLL

### ✅ kernel32.dll - 35/50 (70%)

#### Implemented (35)
- [x] CloseHandle
- [x] ExitProcess
- [x] FlushFileBuffers
- [x] GetCommandLineA
- [x] GetCurrentProcess
- [x] GetFileType
- [x] GetLastError
- [x] GetModuleFileNameA
- [x] GetModuleHandleA
- [x] GetOEMCP
- [x] GetProcAddress
- [x] GetStartupInfoA
- [x] GetStdHandle
- [x] GetStringTypeW
- [x] GetVersion
- [x] GlobalAlloc
- [x] GlobalFree
- [x] HeapAlloc
- [x] HeapCreate
- [x] HeapDestroy
- [x] HeapFree
- [x] LCMapStringA
- [x] LCMapStringW
- [x] MultiByteToWideChar
- [x] QueryPerformanceFrequency
- [x] RaiseException
- [x] ReadFile
- [x] SetEndOfFile
- [x] SetFilePointer
- [x] SetHandleCount
- [x] SetStdHandle
- [x] TerminateProcess
- [x] VirtualAlloc
- [x] VirtualFree
- [x] WriteFile

#### Not Implemented (15)
- [ ] CompareStringA
- [ ] CompareStringW
- [ ] DeleteFileA
- [ ] FileTimeToLocalFileTime
- [ ] FileTimeToSystemTime
- [ ] FindClose
- [ ] FindFirstFileA
- [ ] FindNextFileA
- [ ] GetTimeZoneInformation
- [ ] GlobalHandle
- [ ] GlobalLock
- [ ] GlobalUnlock
- [ ] HeapReAlloc
- [ ] MoveFileA
- [ ] SetEnvironmentVariableA

### ✅ user32.dll - 30/36 (83%)

#### Implemented (30)
- [x] AdjustWindowRectEx
- [x] ClientToScreen
- [x] CreateWindowExA
- [x] DefWindowProcA
- [x] DestroyWindow
- [x] DispatchMessageA
- [x] GetClientRect
- [x] GetDC
- [x] GetMenu
- [x] GetMessageA
- [x] GetSystemMetrics
- [x] GetWindowLongA
- [x] GetWindowRect
- [x] LoadCursorA
- [x] LoadIconA
- [x] MessageBoxA
- [x] PeekMessageA
- [x] PostMessageA
- [x] PostQuitMessage
- [x] RegisterClassA
- [x] ReleaseDC
- [x] SetCursor
- [x] SetFocus
- [x] SetRect
- [x] SetWindowLongA
- [x] SetWindowPos
- [x] ShowWindow
- [x] SystemParametersInfoA
- [x] TranslateMessage
- [x] UpdateWindow

#### Not Implemented (6)
- [ ] DialogBoxParamA
- [ ] EnableWindow
- [ ] EndDialog
- [ ] GetDlgItem
- [ ] GetDlgItemTextA
- [ ] SendDlgItemMessageA

### ✅ gdi32.dll - 2/2 (100%)

#### Implemented (2)
- [x] GetDeviceCaps
- [x] GetStockObject

### ✅ WINMM.dll - 4/8 (50%)

#### Implemented (4)
- [x] timeBeginPeriod
- [x] timeEndPeriod
- [x] timeGetTime
- [x] timeKillEvent

#### Not Implemented (4)
- [ ] joyGetDevCapsA
- [ ] joyGetPosEx
- [ ] mciSendStringA
- [ ] timeSetEvent

### ✅ DINPUT.dll - 1/1 (100%)

#### Implemented (1)
- [x] DirectInputCreateA

### ✅ DDRAW.dll - 1/1 (100%)

#### Implemented (1)
- [x] DirectDrawCreate

### ✅ DSOUND.dll - 1/1 (100%)

#### Implemented (1)
- [x] DirectSoundCreate

### ❌ DPLAYX.dll - 0/2 (0%)

#### Not Implemented (2)
- [ ] Ordinal_1
- [ ] Ordinal_2

## Notes

- Most APIs are implemented in the `TryInvokeUnsafe` method of each module
- Some APIs have `[DllModuleExport]` attributes, while others are only in the switch case
- The DPLAYX ordinal exports (Ordinal_1 and Ordinal_2) likely map to DirectPlayCreate and DirectPlayEnumerateA functions

## Next Steps

To fully support Ignition (1997), the following APIs should be prioritized:

1. **File I/O**: FindFirstFileA, FindNextFileA, FindClose, DeleteFileA, MoveFileA
2. **Time Functions**: FileTimeToLocalFileTime, FileTimeToSystemTime, GetTimeZoneInformation  
3. **Dialog Functions**: DialogBoxParamA, EndDialog, GetDlgItem, GetDlgItemTextA, SendDlgItemMessageA, EnableWindow
4. **DirectPlay**: Ordinal_1, Ordinal_2 (DirectPlayCreate, DirectPlayEnumerateA)
5. **String Comparison**: CompareStringA, CompareStringW
6. **Multimedia**: timeSetEvent, joyGetPosEx, joyGetDevCapsA, mciSendStringA
7. **Memory**: HeapReAlloc, GlobalLock, GlobalUnlock, GlobalHandle
8. **Environment**: SetEnvironmentVariableA
