# Window Dialog Implementation Summary

## Overview

This implementation adds support for basic Windows dialog and window management functionality to Win32Emu, specifically targeting the functions used in the GDI test application mentioned in the issue.

## Functions Implemented

### GDI32.DLL

#### GetStockObject
```c
HGDIOBJ GetStockObject(int i);
```
Returns a handle to one of the stock graphics objects (pens, brushes, fonts, etc.) that Windows provides by default.

**Implementation Details:**
- Supports all standard stock objects (WHITE_BRUSH, BLACK_BRUSH, NULL_BRUSH, WHITE_PEN, BLACK_PEN, DEFAULT_GUI_FONT, etc.)
- Stock object handles are cached and reused for the same object ID
- Returns pseudo-handles starting at 0x80000000 to distinguish them from regular GDI objects
- Validates stock object IDs and returns 0 for invalid IDs

**Test Coverage:**
- Valid stock object retrieval
- Handle caching (same object returns same handle)
- Different objects return different handles
- Invalid stock object ID handling

### USER32.DLL

#### RegisterClassA
```c
ATOM RegisterClassA(const WNDCLASSA *lpWndClass);
```
Registers a window class for subsequent use in calls to CreateWindow or CreateWindowEx.

**Implementation Details:**
- Parses WNDCLASSA structure from guest memory
- Stores window class information in ProcessEnvironment
- Returns a non-zero ATOM (16-bit hash value) on success
- Prevents duplicate class registration
- Validates input parameters (NULL pointer, NULL class name)

**WNDCLASSA Structure Support:**
- style: Window class styles
- lpfnWndProc: Window procedure callback
- cbClsExtra: Extra class memory
- cbWndExtra: Extra window memory  
- hInstance: Instance handle
- hIcon: Icon handle
- hCursor: Cursor handle
- hbrBackground: Background brush
- lpszMenuName: Menu name
- lpszClassName: Class name

**Test Coverage:**
- Valid class registration
- Duplicate registration prevention
- NULL parameter validation
- Return value verification

#### CreateWindowExA
```c
HWND CreateWindowExA(
    DWORD dwExStyle,
    LPCSTR lpClassName,
    LPCSTR lpWindowName,
    DWORD dwStyle,
    int X,
    int Y,
    int nWidth,
    int nHeight,
    HWND hWndParent,
    HMENU hMenu,
    HINSTANCE hInstance,
    LPVOID lpParam
);
```
Creates an overlapped, pop-up, or child window with extended style.

**Implementation Details:**
- Validates that window class is registered before creating window
- Handles CW_USEDEFAULT (0x80000000) for default positioning/sizing
- Allocates unique window handles (starting at 0x00010000)
- Stores window information in ProcessEnvironment for later retrieval
- Supports parent-child window relationships
- Validates input parameters (NULL class name, unregistered classes)

**Window Information Tracked:**
- Handle (HWND)
- Class name
- Window title
- Style flags
- Extended style flags
- Position (x, y)
- Size (width, height)
- Parent window handle
- Menu handle
- Instance handle
- Creation parameter

**Test Coverage:**
- Window creation with registered class
- Unregistered class handling
- NULL parameter validation
- Multiple window creation (unique handles)
- CW_USEDEFAULT parameter handling

## Architecture Changes

### ProcessEnvironment Extensions

Added window management capabilities:
```csharp
// Window class registry
private Dictionary<string, WindowClassInfo> _windowClasses;

// Window tracking
private Dictionary<uint, WindowInfo> _windows;
private uint _nextWindowHandle;

// Public methods
bool RegisterWindowClass(string className, WindowClassInfo classInfo)
bool IsWindowClassRegistered(string className)
WindowClassInfo? GetWindowClass(string className)
uint CreateWindow(...)
WindowInfo? GetWindow(uint hwnd)
bool DestroyWindow(uint hwnd)
```

### NativeTypes Extensions

Added constants for window operations:
- `StockObject`: Stock object IDs (WHITE_BRUSH, DEFAULT_GUI_FONT, etc.)
- `WindowClass`: Window class styles (CS_VREDRAW, CS_HREDRAW, etc.)
- `WindowStyle`: Window styles (WS_OVERLAPPED, WS_POPUP, WS_CHILD, etc.)
- `ColorConstants`: System color indices

## Test Infrastructure

Created comprehensive test infrastructure in `Win32Emu.Tests.User32`:

### TestInfrastructure/
- **MockCpu.cs**: Mock CPU implementation for testing API calls
- **TestEnvironment.cs**: Complete test environment with helper methods
  - `CallUser32Api()`: Invoke User32 functions
  - `CallGdi32Api()`: Invoke GDI32 functions
  - `WriteWndClassA()`: Write WNDCLASSA structure to memory
  - `WriteString()`: Write null-terminated strings

### Test Files
- **Gdi32Tests.cs**: 8 tests for GetStockObject
- **WindowTests.cs**: 9 tests for RegisterClassA and CreateWindowExA
- **IntegrationTests.cs**: 2 integration tests demonstrating complete workflow

**Total Test Coverage: 19 tests, all passing**

## Usage Example

The following demonstrates the workflow from the C++ gdi.exe test program:

```csharp
// 1. Get stock object (font)
var hfont = GetStockObject(DEFAULT_GUI_FONT);

// 2. Register window class
WNDCLASSA wc = {
    .lpfnWndProc = wndproc,
    .lpszClassName = "gdi"
};
RegisterClassA(&wc);

// 3. Create window
HWND hwnd = CreateWindowExA(
    0,                      // dwExStyle
    "gdi",                  // lpClassName
    "title",                // lpWindowName
    WS_OVERLAPPED,          // dwStyle
    CW_USEDEFAULT,          // x
    CW_USEDEFAULT,          // y
    400,                    // nWidth
    300,                    // nHeight
    NULL,                   // hWndParent
    NULL,                   // hMenu
    NULL,                   // hInstance
    NULL                    // lpParam
);

// 4. Create child button
HWND button = CreateWindowExA(
    0,
    "BUTTON",
    "quit",
    WS_TABSTOP | WS_VISIBLE | WS_CHILD,
    10, 10, 100, 30,
    hwnd,                   // parent
    (HMENU)1,               // button ID
    NULL,
    NULL
);
```

## Limitations and Future Work

### Current Limitations
1. **No Message Loop**: Message processing (GetMessage, DispatchMessage) not implemented
2. **No Window Procedures**: Window procedure callbacks not invoked
3. **No Visual Rendering**: Windows created but not visually displayed
4. **System Classes**: Common control classes (BUTTON, EDIT, etc.) not pre-registered
5. **No GDI Drawing**: Paint operations not supported

### Future Enhancements
1. Implement message queue and message processing
2. Add support for window procedure callbacks
3. Integrate with Avalonia UI for visual rendering (as per API_INTEGRATION.md)
4. Register system window classes (BUTTON, EDIT, LISTBOX, etc.)
5. Implement ShowWindow, UpdateWindow, DestroyWindow
6. Add BeginPaint, EndPaint, and other GDI drawing functions
7. Implement DefWindowProc for default message handling

## Testing

All tests pass successfully:
```
Total tests: 153 (17 User32 + 102 Kernel32 + 34 Emulator)
     Passed: 153
     Failed: 0
   Duration: ~6 seconds
```

### Test Categories
- **Unit Tests**: Individual function behavior
- **Integration Tests**: Multi-function workflows
- **Validation Tests**: Error handling and edge cases

## References

- [Windows API Documentation - GetStockObject](https://docs.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-getstockobject)
- [Windows API Documentation - RegisterClassA](https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerclassa)
- [Windows API Documentation - CreateWindowExA](https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-createwindowexa)
- Project: `Win32Emu.Gui/API_INTEGRATION.md` - Phase 2 implementation plan
