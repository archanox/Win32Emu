# OLE32.DLL Implementation

## Overview
This document describes the implementation of OLE32.DLL functions for the Win32Emu emulator.

## Implemented Functions

### CoInitialize
```c
HRESULT CoInitialize([in, optional] LPVOID pvReserved);
```

Initializes the COM library on the current thread. This is a simplified implementation that tracks whether COM has been initialized.

**Parameters:**
- `pvReserved`: Reserved; must be NULL

**Return Values:**
- `S_OK (0x00000000)`: Success - COM library was initialized successfully
- `S_FALSE (0x00000001)`: COM library is already initialized on this thread

**Implementation Notes:**
- Tracks initialization state per Ole32Module instance
- Multiple calls return S_FALSE after the first successful initialization
- Always returns success to avoid breaking applications

### CoUninitialize
```c
void CoUninitialize();
```

Closes the COM library on the current thread, unloads all DLLs loaded by the thread, frees any other resources that the thread maintains, and forces all RPC connections on the thread to close.

**Return Value:**
- None (void function, returns 0)

**Implementation Notes:**
- Tracks and updates initialization state
- Gracefully handles being called without prior CoInitialize
- Logs warning if called without prior initialization

## Testing
Comprehensive tests are available in `Win32Emu.Tests.Emulator/Ole32ModuleTests.cs`:
- CoInitialize basic success test
- CoInitialize multiple calls test (S_FALSE behavior)
- CoUninitialize success test
- CoUninitialize without initialization test
- Unknown export test

All tests pass successfully.

## Usage in Emulator
The Ole32Module is automatically registered during emulator initialization in `Emulator.cs`:

```csharp
_dispatcher.RegisterModule(new Ole32Module(_env, _image.BaseAddress, loader, _logger));
```

## Stdcall Metadata
The functions use the `[DllModuleExport]` attribute for automatic stdcall argument bytes generation:
- CoInitialize: 4 bytes (1 parameter: LPVOID pvReserved)
- CoUninitialize: 0 bytes (void function)

## Related Issues
This implementation resolves the "Unknown DLL function call" errors for OLE32.DLL!CoInitialize and OLE32.DLL!CoUninitialize reported in issue logs.
