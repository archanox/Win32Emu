# SETUP.EXE Decompilation Analysis

## Executive Summary

After analyzing multiple decompilation outputs (Hex-Rays IDA, Reko, Binary Ninja, RetDec, etc.), we have identified the complete functionality of the Ignition game installer (SETUP.EXE).

**Key Finding: SETUP.EXE is a standard Windows installer that requires several Shell32 APIs and COM support that are likely missing or incomplete in the Win32Emu emulator.**

The installer:
1. ✅ Uses standard Win32 dialog APIs (likely supported)
2. ❌ Uses Shell32 APIs for folder browsing and file operations (likely NOT supported)
3. ❌ Uses COM for creating desktop shortcuts (requires COM infrastructure)
4. ❌ Calls DirectXSetupA from DSETUP.DLL (not emulated)
5. ✅ Uses registry APIs (likely supported)

## Program Structure

### Entry Point: WinMain (0x4022E0)

The `WinMain` function (located at 0x4022E0 in hexrays.cpp line 1551) is straightforward:

```cpp
int __stdcall WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nShowCmd)
{
  CHAR *v4; // esi
  LPSTR v5; // eax

  v4 = &sz;
  hInst = hInstance;
  
  // Get the directory where setup.exe is located
  GetModuleFileNameA(hInstance, &sz, 0x104u);
  v5 = &sz;
  if ( sz )
  {
    do
    {
      if ( *v5 == 92 || *v5 == 47 )  // Find last \ or /
        v4 = v5;
      v5 = CharNextA(v5);
    }
    while ( *v5 );
  }
  *v4 = 0;  // Null-terminate to get directory path
  
  // Initialize COM (for shell link creation later)
  CoInitialize(0);
  
  // Show the main installer dialog
  DialogBoxParamA(hInstance, TemplateName, 0, DialogFunc, 0);
  
  // Cleanup COM
  CoUninitialize();
  return 0;
}
```

**Analysis:**
- Simple entry point that initializes COM and shows a dialog
- Gets the setup.exe's directory to find game files
- All work is done in the dialog procedure

**Emulator Requirements:**
- ✅ `GetModuleFileNameA` - Standard API, likely implemented
- ✅ `CharNextA` - Standard string API
- ✅ `CoInitialize` / `CoUninitialize` - Basic COM initialization (should be stubbed)
- ✅ `DialogBoxParamA` - Dialog creation (should be implemented)

### Main Dialog Procedure: DialogFunc (0x401130)

This is the heart of the installer. It's a standard Windows dialog procedure that handles:

#### WM_INITDIALOG (0x110) - Dialog Initialization

```cpp
case 0x110u:
  dword_40B808 = 0;  // Not busy
  dword_40B804 = 0;  // Not in final state
  
  // Set dialog title from string resource
  ModuleHandleA = GetModuleHandleA(0);
  LoadStringA(ModuleHandleA, 0x65u, ::Caption, 512);
  SetWindowTextA(hWnd, ::Caption);
  
  // Disable Install and Cancel buttons initially
  DlgItem = GetDlgItem(hWnd, 1008);
  EnableWindow(DlgItem, 0);
  v8 = GetDlgItem(hWnd, 1007);
  EnableWindow(v8, 0);
  
  // Set static text
  v9 = GetModuleHandleA(0);
  LoadStringA(v9, 0x76u, Buffer, 512);
  sub_401050(hWnd, 1002, Buffer, 175, 195);
  
  // Set default installation path in edit control
  SendDlgItemMessageA(hWnd, 1000, 0xC5u, 0x104u, 0);  // EM_LIMITTEXT
  v10 = GetModuleHandleA(0);
  LoadStringA(v10, 0x64u, Buffer, 512);
  SetDlgItemTextA(hWnd, 1000, Buffer);
  
  // Set edit control selection
  SendDlgItemMessageA(hWnd, 1000, 0xB1u, 0, 16777472);  // EM_SETSEL
  
  v11 = GetDlgItem(hWnd, 1000);
  SetFocus(v11);
  return 0;
```

**Analysis:**
- Standard dialog initialization
- Loads UI strings from resources
- Sets default installation directory
- All standard Win32 APIs

**Emulator Requirements:**
- ✅ All standard dialog control APIs

#### WM_COMMAND (0x111) - Button Clicks

This is where the main installation logic resides.

##### Button 1011: Browse Button

```cpp
case 1011:
  if (!dword_40B804)  // If not in final state
  {
    bi.hwndOwner = hWnd;
    bi.pidlRoot = 0;
    bi.pszDisplayName = (LPSTR)String;
    bi.lpszTitle = 0;
    bi.ulFlags = 1;  // BIF_RETURNONLYFSDIRS
    memset(&bi.lpfn, 0, 12);
    
    v68 = SHBrowseForFolderA(&bi);  // ❌ NOT IMPLEMENTED
    if ( v68 )
    {
      SHGetPathFromIDListA(v68, (LPSTR)String);  // ❌ NOT IMPLEMENTED
      SetDlgItemTextA(hWnd, 1000, (LPCSTR)String);
      sub_4010F0(v68);  // Free PIDL via SHGetMalloc
    }
  }
  return 0;
```

**Analysis:**
- Shows folder browse dialog
- Updates installation path text box
- Requires Shell32 APIs

**Emulator Requirements:**
- ❌ `SHBrowseForFolderA` - Folder browse dialog (NOT IMPLEMENTED)
- ❌ `SHGetPathFromIDListA` - Convert PIDL to path string (NOT IMPLEMENTED)
- ❌ `SHGetMalloc` and `IMalloc` interface - Memory management (NOT IMPLEMENTED)

##### Button 1: Install Button - Main Installation

This is the most complex part. The installation proceeds in several phases:

###### Phase 1: Validation and Preparation

```cpp
// Increment busy flag
++dword_40B808;

// Disable Install and Cancel buttons
v13 = GetDlgItem(hWnd, 1);
EnableWindow(v13, 0);
v14 = GetDlgItem(hWnd, 2);
EnableWindow(v14, 0);

// Set busy cursor
v15 = LoadCursorA(0, (LPCSTR)0x7F02);  // IDC_WAIT
SetCursor(v15);

// Get installation path
v16 = GetDlgItem(hWnd, 1000);
GetWindowTextA(v16, &::String, 262);
_strupr(&::String);

// Ensure path ends with backslash
if ( !strstr(&::String, SubStr) )
{
  strcat(&::String, SubStr);  // Add backslash
  strcat(&::String, byte_40A54C);  // Add subdirectory
}
```

###### Phase 2: Create SMAG.INI (Configuration File)

```cpp
// Copy WIN.INI to installation directory as SMAG.INI
GetWindowsDirectoryA(v86, 0x104u);
sub_401000(v86, aWinIni);  // Append "\\win.ini"
lstrcpyA(String1, &::String);
sub_401000(String1, aSmagIni);  // Append "\\smag.ini"

FileOp.hwnd = hWnd;
FileOp.pFrom = v86;
FileOp.pTo = String1;
FileOp.wFunc = 2;  // FO_COPY
FileOp.fFlags = 20;  // FOF_SILENT | FOF_NOCONFIRMATION

if ( SHFileOperationA(&FileOp) )  // ❌ NOT IMPLEMENTED
{
  // Show error
  sub_4010B0(hWnd, byte_40A534);
  // Re-enable buttons and return
  ...
}

// Delete the copied file (just used as template)
FileOp.hwnd = hWnd;
FileOp.pFrom = String1;
FileOp.wFunc = 3;  // FO_DELETE
FileOp.pTo = 0;
FileOp.fFlags = 20;
SHFileOperationA(&FileOp);  // ❌ NOT IMPLEMENTED
```

**Analysis:**
- Creates a configuration file by copying WIN.INI
- Uses Shell file operations API
- Error handling if copy fails

**Emulator Requirements:**
- ❌ `SHFileOperationA` - Batch file operations (NOT IMPLEMENTED)
- ✅ `GetWindowsDirectoryA` - Get Windows directory (should be implemented)

###### Phase 3: DirectX Installation (Optional)

```cpp
// Ask user if they want to install DirectX
v20 = GetModuleHandleA(0);
LoadStringA(v20, 0x6Eu, Text, 512);
v21 = GetModuleHandleA(0);
LoadStringA(v21, 0x6Fu, Caption, 512);
v23 = MessageBoxA(hWnd, Text, Caption, 4u);  // MB_YESNO

if ( v23 == 6 )  // IDYES
{
  // Check disk space for DirectX
  // ... disk space calculation code ...
  
  // Install DirectX
  v23 = DirectXSetupA(hWnd, 0, 268438079);  // ❌ NOT IMPLEMENTED
  
  if ( v23 < 0 )
  {
    // DirectX setup failed
    if ( v23 == -13 )
      v71 = 128;  // Error message resource ID
    else
      v71 = 123;
    v29 = GetModuleHandleA(0);
    LoadStringA(v29, v71, Buffer, 512);
    MessageBoxA(hWnd, Buffer, ::Caption, 0);
    EndDialog(hWnd, v23);
    return 0;
  }
  
  dword_40BC28 = v23 == 0;  // Remember if DirectX was installed
}
```

**Analysis:**
- Optionally installs DirectX runtime
- Calls DirectXSetupA from DSETUP.DLL
- Handles errors if DirectX setup fails

**Emulator Requirements:**
- ❌ `DirectXSetupA` - DirectX installer API from DSETUP.DLL (NOT IMPLEMENTED)
- Note: This could be stubbed to return success since emulator doesn't need DirectX

###### Phase 4: Copy Game Files

```cpp
// Loop through all files in the game
v38 = (LPCSTR *)off_40A050;  // Array of 294 filenames
do
{
  // Build source path: setup.exe directory + filename
  lstrcpyA(v86, &sz);
  sub_401000(v86, *v38);
  
  // Build destination path: install directory + filename
  lstrcpyA(String1, &::String);
  sub_401000(String1, *v38);
  
  // Update status message
  v39 = GetModuleHandleA(0);
  LoadStringA(v39, 0x7Du, Buffer, 512);
  sub_4010B0(hWnd, Buffer, *v38);
  
  // Copy file
  FileOp.hwnd = hWnd;
  FileOp.pFrom = v86;
  FileOp.pTo = String1;
  FileOp.fFlags = 532;  // FOF_NOCONFIRMATION | FOF_MULTIDESTFILES | etc
  FileOp.wFunc = 2;  // FO_COPY
  
  while ( SHFileOperationA(&FileOp) )  // ❌ NOT IMPLEMENTED
  {
    // If copy failed, ask to retry or cancel
    v40 = GetModuleHandleA(0);
    LoadStringA(v40, 0x7Eu, Buffer, 512);
    wsprintfA((LPSTR)String, Buffer, *v38);
    if ( MessageBoxA(hWnd, (LPCSTR)String, ::Caption, 5u) == 2 )  // IDCANCEL
      goto LABEL_37;
  }
  
  // Set file attributes
  SetFileAttributesA(String1, 0x80u);  // FILE_ATTRIBUTE_NORMAL
  
LABEL_37:
  ++v38;
} while ( v38 < (LPCSTR *)&hKey );  // Loop for all 294 files
```

**Analysis:**
- Copies all 294 game files from CD/source to installation directory
- Shows progress for each file
- Allows retry on error
- Critical functionality requiring SHFileOperationA

**Emulator Requirements:**
- ❌ `SHFileOperationA` - Essential for file copying (NOT IMPLEMENTED)
- ✅ `SetFileAttributesA` - Set file attributes (should be implemented)
- ✅ `wsprintfA` - Format string (should be implemented)

###### Phase 5: Registry Configuration

```cpp
// Create registry key for game
String[0] = RegCreateKeyExA(
  HKEY_LOCAL_MACHINE, 
  SubKey,  // "Software\\Ignition\\InstallPath" or similar
  0, 0, 0, 
  0xF003Fu,  // KEY_ALL_ACCESS
  0, 
  &hKey, 
  &dwDisposition);

if ( String[0] )
  exit(1);

// Write installation path to registry
strcpy((char *)Data, &::String);
RegSetValueExA(hKey, ValueName, 0, 1u, Data, strlen((const char *)Data));
RegCloseKey(hKey);
```

**Analysis:**
- Stores installation path in registry
- Standard registry APIs
- Likely already implemented

**Emulator Requirements:**
- ✅ `RegCreateKeyExA` - Create registry key (likely implemented)
- ✅ `RegSetValueExA` - Write registry value (likely implemented)
- ✅ `RegCloseKey` - Close registry key (likely implemented)

###### Phase 6: Create Start Menu Shortcuts

```cpp
// Get Start Menu Programs folder
hKey = 0;
v42 = GetModuleHandleA(0);
LoadStringA(v42, 0x66u, v95, 512);
SHGetSpecialFolderLocation(0, 2, &ppidl);  // CSIDL_PROGRAMS ❌
SHGetPathFromIDListA(ppidl, pszPath);  // ❌
wsprintfA(PathName, "%s\\%s", pszPath, v95);  // e.g., "Programs\\Ignition"
strcat(PathName, SrcStr);
CreateDirectoryA(PathName, 0);
SHChangeNotify(8, 1u, PathName, 0);  // SHCNE_MKDIR ❌

// Create shortcut for main game (Ign_win.exe)
wsprintfA(v89, "%s\\%s", &::String, off_40A050[0]);  // Source EXE
v43 = GetModuleHandleA(0);
LoadStringA(v43, 0x67u, v87, 260);
wsprintfA(MultiByteStr, "%s\\%s.lnk", PathName, v87);  // Shortcut path
sub_402360((int)v89, MultiByteStr, (int)byte_40A534, (int)&::String);  // ❌

// Create shortcut for removal tool (remove.exe)
wsprintfA(v89, "%s\\%s", &::String, off_40A054[0]);
v44 = GetModuleHandleA(0);
LoadStringA(v44, 0x68u, v87, 260);
wsprintfA(MultiByteStr, "%s\\%s.lnk", PathName, v87);
sub_402360((int)v89, MultiByteStr, (int)byte_40A534, (int)&::String);  // ❌

// Create shortcut for readme (readme.txt)
wsprintfA(v89, "%s\\%s", &::String, off_40A058[0]);
v45 = GetModuleHandleA(0);
LoadStringA(v45, 0x69u, v87, 260);
wsprintfA(MultiByteStr, "%s\\%s.lnk", PathName, v87);
sub_402360((int)v89, MultiByteStr, (int)byte_40A534, (int)byte_40A534);  // ❌
```

**Analysis:**
- Creates Start Menu folder for game
- Creates 3 shortcuts (game, uninstaller, readme)
- Requires Shell APIs and COM

**Emulator Requirements:**
- ❌ `SHGetSpecialFolderLocation` - Get special folder PIDL (NOT IMPLEMENTED)
- ❌ `SHGetPathFromIDListA` - Convert PIDL to path (NOT IMPLEMENTED)
- ❌ `SHChangeNotify` - Notify shell of changes (NOT IMPLEMENTED)
- ✅ `CreateDirectoryA` - Create directory (should be implemented)

### Shortcut Creation: sub_402360 (0x402360)

This function creates Windows shortcuts using COM:

```cpp
int __cdecl sub_402360(int a1, LPCCH lpMultiByteStr, int a3, int a4)
{
  HRESULT v4;
  int result;
  LPVOID ppv;
  int v7;
  WCHAR WideCharStr[260];

  // Create IShellLink COM object
  v4 = CoCreateInstance(&rclsid, 0, 1u, &riid, &ppv);  // ❌ NEEDS COM SUPPORT
  if ( v4 >= 0 )
  {
    // Call IShellLink::SetPath (vtable offset +80)
    (*(void (__stdcall **)(LPVOID, int))(*(_DWORD *)ppv + 80))(ppv, a1);  // ❌
    
    // Call IShellLink::SetWorkingDirectory (vtable offset +28)
    (*(void (__stdcall **)(LPVOID, int))(*(_DWORD *)ppv + 28))(ppv, a3);  // ❌
    
    // Call IShellLink::SetDescription (vtable offset +36)
    (*(void (__stdcall **)(LPVOID, int))(*(_DWORD *)ppv + 36))(ppv, a4);  // ❌
    
    // Query for IPersistFile interface (vtable offset +0 = QueryInterface)
    v4 = (**(int (__stdcall ***)(LPVOID, void *, int *))ppv)(ppv, &unk_407360, &v7);  // ❌
    if ( v4 >= 0 )
    {
      // Convert filename to Unicode
      MultiByteToWideChar(0, 0, lpMultiByteStr, -1, WideCharStr, 260);
      
      // Call IPersistFile::Save (vtable offset +24)
      result = (*(int (__stdcall **)(int, WCHAR *, int))(*(_DWORD *)v7 + 24))(v7, WideCharStr, 1);  // ❌
      v4 = result;
      if ( result < 0 )
        return result;
        
      // Release IPersistFile (vtable offset +8)
      (*(void (__stdcall **)(int))(*(_DWORD *)v7 + 8))(v7);  // ❌
    }
    
    // Release IShellLink (vtable offset +8)
    (*(void (__stdcall **)(LPVOID))(*(_DWORD *)ppv + 8))(ppv);  // ❌
  }
  return v4;
}
```

**Analysis:**
- Creates Windows .lnk shortcut files
- Uses COM interfaces IShellLink and IPersistFile
- Requires full COM support with vtable dispatch
- Similar to the DirectX COM issue in ign_teas.exe

**Emulator Requirements:**
- ❌ `CoCreateInstance` with CLSID_ShellLink support (NOT IMPLEMENTED)
- ❌ IShellLink COM interface with vtable (NOT IMPLEMENTED)
- ❌ IPersistFile COM interface with vtable (NOT IMPLEMENTED)
- ✅ `MultiByteToWideChar` - String conversion (should be implemented)

## Summary of Missing APIs

### Critical (Installation Cannot Proceed Without)

1. **`SHFileOperationA`** - Used to copy all 294 game files
   - Priority: **HIGH**
   - Without this, no files get installed
   - Could potentially be replaced with custom file copy loop

2. **`SHBrowseForFolderA`** - Folder selection dialog
   - Priority: **MEDIUM**
   - Could use default path or command-line argument as workaround
   
3. **`SHGetPathFromIDListA`** - Convert folder PIDL to path string
   - Priority: **MEDIUM**
   - Required if SHBrowseForFolderA is implemented

### Important (Shortcuts Won't Work Without)

4. **`CoCreateInstance`** - COM object creation
   - Priority: **MEDIUM**
   - Needed for shortcut creation
   - Requires full COM infrastructure

5. **IShellLink COM interface** - Create shortcut properties
   - Priority: **MEDIUM**
   - Part of COM support
   - Can be skipped if shortcuts aren't needed

6. **IPersistFile COM interface** - Save shortcut to disk
   - Priority: **MEDIUM**
   - Part of COM support

### Optional (Can Be Stubbed)

7. **`DirectXSetupA`** - Install DirectX runtime
   - Priority: **LOW**
   - Can return success immediately (emulator doesn't need DirectX)
   - Could show warning that DirectX installation is skipped

8. **`SHGetSpecialFolderLocation`** - Get Start Menu folder
   - Priority: **LOW**
   - Can use hardcoded path like "C:\\Users\\Public\\Start Menu"

9. **`SHChangeNotify`** - Notify shell of file system changes
   - Priority: **LOW**
   - Can be a no-op stub

10. **`SHGetMalloc` / `IMalloc`** - Shell memory allocator
    - Priority: **LOW**
    - Can use regular malloc/free

## Comparison with IGN_TEAS.EXE

| Aspect | IGN_TEAS.EXE | SETUP.EXE |
|--------|--------------|-----------|
| **Type** | Game executable | Installer |
| **UI** | Custom DirectDraw rendering | Standard Windows dialogs |
| **DirectX** | Requires DirectDraw/DirectInput/DirectSound COM vtables | Calls DirectXSetupA (installer) |
| **COM Usage** | DirectX COM interfaces for gameplay | Shell COM for shortcuts |
| **File I/O** | Reads game data files | Copies files from CD |
| **Priority** | High (game should run) | Medium (installation utility) |
| **Complexity** | High (full COM vtable emulation) | Medium (file operations + basic COM) |

## Recommendations

### Immediate Actions

1. **Implement `SHFileOperationA`**
   - Most critical API for installation to work
   - Should support FO_COPY, FO_DELETE operations
   - Can be a thin wrapper around emulator's VFS file operations
   - Estimated effort: Medium (1-2 days)

2. **Implement `SHBrowseForFolderA`**
   - Shows folder selection dialog
   - For emulator, could:
     - Option A: Show actual native folder dialog
     - Option B: Use command-line argument for install path
     - Option C: Return a default path without showing UI
   - Estimated effort: Low-Medium (0.5-1 day for option C, more for A/B)

3. **Stub `DirectXSetupA`**
   - Return S_OK (0) immediately
   - Add logging message that DirectX installation is skipped
   - Estimated effort: Trivial (1 hour)

### Medium-Term Goals

4. **Implement Shell32 helper APIs**
   - `SHGetPathFromIDListA`
   - `SHGetSpecialFolderLocation`
   - These are relatively simple string/path manipulation
   - Estimated effort: Low (0.5 day)

5. **Implement basic COM infrastructure for IShellLink**
   - Similar to DirectX COM vtables from ign_teas analysis
   - Create ShellLinkObject with vtable
   - Implement IShellLink and IPersistFile methods
   - Can actually create .lnk files or just log the calls
   - Estimated effort: Medium (2-3 days if reusing DirectX COM infrastructure)

### Long-Term / Optional

6. **Full COM infrastructure**
   - Generic CoCreateInstance with CLSID registry
   - Support for multiple COM interfaces
   - This would benefit both setup.exe and ign_teas.exe
   - Estimated effort: High (1-2 weeks)

## Testing Strategy

### Phase 1: Basic Dialog
- Run setup.exe in emulator
- Verify dialog appears with correct title
- Verify default installation path is shown
- Test: Cancel button should exit

### Phase 2: File Operations
- After implementing SHFileOperationA
- Test: Click Install button
- Verify: Files are copied to installation directory
- Verify: All 294 files are present

### Phase 3: Shortcuts (Optional)
- After implementing COM support
- Verify: Start Menu folder is created
- Verify: 3 .lnk files are created
- Test: Shortcuts can be opened (even if they just log)

### Phase 4: Complete Install
- Full installation flow
- Registry entries created
- Game can be launched from installed location

## Files to Modify

Based on repository structure:

1. **`Win32Emu/Win32/Modules/Shell32Module.cs`** (create if doesn't exist)
   - Add SHFileOperationA
   - Add SHBrowseForFolderA
   - Add SHGetPathFromIDListA
   - Add SHGetSpecialFolderLocation
   - Add SHChangeNotify
   - Add SHGetMalloc

2. **`Win32Emu/Win32/Modules/Ole32Module.cs`** (enhance if exists)
   - Enhance CoCreateInstance for CLSID_ShellLink
   - Add COM infrastructure from ign_teas recommendations

3. **`Win32Emu/Win32/COM/`** (create new directory)
   - `IShellLink.cs` - IShellLink interface implementation
   - `IPersistFile.cs` - IPersistFile interface implementation
   - `ShellLinkObject.cs` - COM object implementation

4. **`Win32Emu/Win32/Modules/DirectXSetupModule.cs`** (create)
   - Stub for DirectXSetupA

## Conclusion

SETUP.EXE is a standard Windows installer that primarily requires:
1. **Shell32 file operation APIs** (critical)
2. **Shell32 folder/path APIs** (important)
3. **COM support for shortcuts** (optional but nice)
4. **DirectX setup stub** (trivial)

Unlike ign_teas.exe which requires complex DirectX COM vtable emulation for gameplay, setup.exe's requirements are more straightforward to implement. The most critical piece is `SHFileOperationA` for file copying, which could potentially be implemented in a few days.

The COM shortcut creation is similar in concept to the DirectX COM issues in ign_teas.exe, so implementing a general COM framework would benefit both executables.

**Priority Recommendation:** Implement Shell32 file operations first (allows installation to work), then add COM support for shortcuts (makes installation complete).
