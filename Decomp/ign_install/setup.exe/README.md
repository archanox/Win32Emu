# SETUP.EXE Decompilation Files

## Overview

This directory contains decompilation outputs from various decompilers analyzing the SETUP.EXE file from the Ignition game installer. Each decompiler has different strengths and may recover different aspects of the code more accurately.

## Files

### Decompiler Outputs

- **`hexrays.cpp`** - Hex-Rays IDA Pro decompilation
  - Generally considered the gold standard for decompilation quality
  - Best for understanding high-level program flow
  - Good function name recovery and type inference
  - Size: ~55 KB, 1608 lines
  - Most readable output for installer logic

- **`reko.cpp`** - Reko decompiler output
  - Open-source decompiler
  - Focuses on portability and multiple architectures
  - Size: ~56 KB, 2464 lines
  - Good for cross-referencing API calls

- **`recstudio.cpp`** - Rec Studio decompilation
  - Commercial decompiler
  - Alternative perspective on the code
  - Size: ~71 KB, 2383 lines

- **`binaryninja.cpp`** - Binary Ninja decompilation
  - Modern commercial decompiler
  - Clean, readable output
  - Good intermediate representation
  - Size: ~158 KB, 5542 lines

- **`retdec.cpp`** - RetDec decompilation
  - Machine-learning enhanced decompilation
  - Can recover some patterns other tools miss
  - Size: ~285 KB, 9107 lines

- **`rev.ng.cpp`** - Rev.ng decompiler output
  - Research-oriented decompiler
  - Size: ~328 KB, 8186 lines

- **`snowman.cpp`** - Snowman decompiler output
  - Part of the radare2 ecosystem
  - Good for comparing against other tools
  - Size: ~297 KB, 7272 lines

- **`boomerang.cpp`** - Boomerang decompiler output
  - Research-oriented decompiler
  - Experimental but can provide insights
  - Size: ~620 KB, 13187 lines

### Analysis Documents

- **`ANALYSIS.md`** - Comprehensive analysis of the decompilation results
  - Identifies the installer's functionality
  - Documents required Win32 APIs
  - Explains COM usage for shortcut creation
  - Identifies DirectX setup integration
  - Provides recommendations for emulator support

- **`INDEX.md`** - Quick reference index
  - Function offsets and names
  - API call locations
  - Key data structures

## Key Findings

After analyzing these decompilation files, we identified that SETUP.EXE:

1. **Is a Windows installer application** using a dialog-based UI
2. **Uses COM** for creating shell shortcuts (IShellLink interface)
3. **Calls DirectXSetupA** to install DirectX runtime components
4. **Uses advanced Shell APIs** for file operations and folder browsing
5. **Modifies the registry** to store installation path
6. **Creates start menu shortcuts** using COM automation

The critical functionality:
```
WinMain() 
  → CoInitialize() ✅
  → DialogBoxParamA() - Main installer UI
      → WM_INITDIALOG - Initialize dialog ✅
      → WM_COMMAND - Button clicks
          → SHBrowseForFolderA() - Browse for install path ❌ NOT IMPLEMENTED
          → SHFileOperationA() - Copy files ❌ NOT IMPLEMENTED
          → DirectXSetupA() - Install DirectX ❌ NOT IMPLEMENTED
          → RegCreateKeyExA() - Create registry key ✅ (likely)
          → CoCreateInstance() - Create shell link COM object ❌ NEEDS COM SUPPORT
          → IShellLink vtable methods ❌ NEEDS COM VTABLES
  → CoUninitialize() ✅
```

## How to Use These Files

### For Understanding Program Flow

1. Start with `hexrays.cpp` - it has the cleanest high-level view
2. Look for the `WinMain` function (line 1551)
3. Follow to `DialogFunc` (line 1061) - the main dialog procedure
4. Track the WM_COMMAND handler for button 1 (Install button)

### For Finding Specific APIs

Use grep to search across all files:
```bash
grep -n "DirectXSetupA" *.cpp
grep -n "CoCreateInstance" *.cpp
grep -n "SHFileOperationA" *.cpp
grep -n "SHBrowseForFolderA" *.cpp
```

### For Understanding COM Method Calls

Look for patterns like:
```cpp
CoCreateInstance(&rclsid, 0, 1u, &riid, &ppv);
(*(void (__stdcall **)(LPVOID, int))(*(_DWORD *)ppv + 80))(ppv, a1);
```

This indicates:
- Creating a COM object via CoCreateInstance
- Dereferencing the object pointer to get vtable
- Calling methods at specific vtable offsets
- These are IShellLink interface method calls

### Cross-Referencing

When in doubt about a function's behavior:
1. Check the same function in multiple decompilers
2. Compare their interpretations
3. The commonalities are likely correct

## Notable Functions

### WinMain (0x4022E0)
- Entry point
- Gets module filename and extracts directory
- Calls `CoInitialize()`
- Shows main installer dialog via `DialogBoxParamA()`
- Calls `CoUninitialize()` on exit

### DialogFunc (0x401130)
- Main dialog procedure
- Handles WM_INITDIALOG (0x110) - dialog initialization
- Handles WM_COMMAND (0x111) - button clicks
  - Button 1: Install button - main installation logic
  - Button 2: Cancel button
  - Button 1011: Browse button - folder selection
- Handles WM_SYSCOMMAND (0x112) - system menu commands

### sub_402360 (0x402360)
- Creates shell shortcuts using COM
- Calls `CoCreateInstance()` to create IShellLink object
- Uses vtable method calls at offsets:
  - +80: SetPath (set target executable)
  - +28: SetWorkingDirectory
  - +36: SetDescription
  - +0: QueryInterface (to get IPersistFile)
  - +24 (IPersistFile): Save (saves .lnk file)

### Installation Flow (in DialogFunc WM_COMMAND handler for button 1)
1. Validate installation directory
2. Check disk space requirements
3. Call `DirectXSetupA()` to install DirectX
4. Copy game files using `SHFileOperationA()`
5. Create registry entries
6. Create start menu shortcuts via COM
7. Show completion message

## Common Patterns

### File List Pattern
```cpp
char *off_40A050[294] = {
  "Ign_win.exe",
  "remove.exe",
  "readme.txt",
  // ... 291 more files
};
```
This array contains all files to be copied during installation.

### String Resource Loading
```cpp
LoadStringA(GetModuleHandleA(0), 0x65u, Caption, 512);
```
All UI strings are stored as resources and loaded dynamically.

### Shell File Operations
```cpp
FileOp.hwnd = hWnd;
FileOp.pFrom = sourceFile;
FileOp.pTo = destFile;
FileOp.wFunc = 2;  // FO_COPY
FileOp.fFlags = 532;  // FOF_NOCONFIRMATION | FOF_MULTIDESTFILES | etc
SHFileOperationA(&FileOp);
```

### COM Shortcut Creation
```cpp
CoCreateInstance(&rclsid, 0, 1, &riid, &ppv);
// Call SetPath, SetWorkingDirectory, SetDescription via vtable
// Call QueryInterface to get IPersistFile
// Call IPersistFile::Save to write .lnk file
```

## Required Win32 APIs

### Implemented (likely)
- `CoInitialize` / `CoUninitialize`
- `DialogBoxParamA`
- `GetDlgItem`, `EnableWindow`, `ShowWindow`
- `SetWindowTextA`, `GetWindowTextA`
- `LoadStringA`, `GetModuleHandleA`
- `MessageBoxA`
- `RegCreateKeyExA`, `RegSetValueExA`, `RegCloseKey`
- `CreateDirectoryA`
- `GetWindowsDirectoryA`, `GetDiskFreeSpaceA`
- `SetFileAttributesA`

### Not Implemented (critical for installer)
- `SHBrowseForFolderA` - Folder browse dialog
- `SHGetPathFromIDListA` - Convert PIDL to path
- `SHFileOperationA` - Batch file operations (copy/move/delete)
- `SHGetMalloc` / `IMalloc::Free` / `IMalloc::Release` - Shell memory management
- `SHGetSpecialFolderLocation` - Get special folder PIDLs
- `SHChangeNotify` - Notify shell of file system changes
- `DirectXSetupA` - DirectX installer API (from DSETUP.DLL)
- `CoCreateInstance` - COM object creation (partial support needed)
- IShellLink COM interface - For creating shortcuts
- IPersistFile COM interface - For saving shortcuts

## COM Interfaces Used

### IShellLink (CLSID {00021401-0000-0000-C000-000000000046})
- QueryInterface (offset +0)
- AddRef (offset +4)
- Release (offset +8)
- SetPath (offset +80)
- SetWorkingDirectory (offset +28)
- SetDescription (offset +36)

### IPersistFile (IID {0000010B-0000-0000-C000-000000000046})
- Save (offset +24)

## Limitations of Decompilation

These files are **approximations** of the original source code:
- Variable names are guessed (v1, v2, etc.)
- Some types may be incorrect
- Optimizations may obscure intent
- Inlined functions are not always obvious
- Some constructs may be artifacts of compilation

Always cross-reference with:
- Multiple decompilers (provided here)
- Win32 API documentation
- Shell API documentation
- Actual emulator behavior

## Related Files

- `/EXEs/ign_install/SETUP.EXE` - The original executable
- `/EXEs/ign_install/DSETUP.DLL` - DirectX setup library
- `/Win32Emu/Win32/Modules/Shell32Module.cs` - Shell32 API emulation (if exists)
- `/Win32Emu/Win32/Modules/Ole32Module.cs` - COM/OLE emulation (if exists)

## Next Steps

See `ANALYSIS.md` for detailed recommendations on implementing:
1. Shell32 APIs for folder browsing and file operations
2. COM infrastructure for CoCreateInstance and interface support
3. IShellLink and IPersistFile COM interfaces for shortcut creation
4. DirectX setup stub (or skip with mock implementation)

## Contributing

When analyzing the decompilation:
1. Document your findings in `ANALYSIS.md`
2. Update function comments with better names if you identify them
3. Note any discrepancies between decompilers
4. Reference Win32/Shell API documentation
