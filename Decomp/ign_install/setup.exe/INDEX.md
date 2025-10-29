# SETUP.EXE Quick Reference Index

## Function Map

| Address | Name | Description | File:Line |
|---------|------|-------------|-----------|
| 0x401000 | sub_401000 | Append filename to path | hexrays.cpp:16 |
| 0x401050 | sub_401050 | Create/position static text control | hexrays.cpp:17 |
| 0x4010B0 | sub_4010B0 | Update status text in dialog | hexrays.cpp:18 |
| 0x4010F0 | sub_4010F0 | Free shell PIDL (via SHGetMalloc) | hexrays.cpp:19 |
| 0x401130 | DialogFunc | Main dialog procedure | hexrays.cpp:20, 1061 |
| 0x4022C0 | sub_4022C0 | Enable/disable dialog control | hexrays.cpp:21, 1542 |
| 0x4022E0 | WinMain | Program entry point | hexrays.cpp:22, 1551 |
| 0x402360 | sub_402360 | Create Windows shortcut via COM | hexrays.cpp:23, 1577 |

## Dialog Controls

| ID | Type | Description |
|----|------|-------------|
| 1 | Button | Install / OK / Finish button |
| 2 | Button | Cancel / Exit button |
| 1000 | Edit | Installation path text box |
| 1001 | Static | Label or image |
| 1002 | Static | Status text / progress indicator |
| 1005 | Static | Success message 1 |
| 1006 | Static | Success message 2 |
| 1007 | Button | Disabled button |
| 1008 | Button | Disabled button |
| 1009 | Static | Label |
| 1011 | Button | Browse folder button |

## Message Handlers

| Message | Value | Handler Location | Description |
|---------|-------|------------------|-------------|
| WM_SETCURSOR | 0x20 | Line 1157 | Set cursor to wait/arrow |
| WM_INITDIALOG | 0x110 | Line 1163 | Initialize dialog controls |
| WM_COMMAND | 0x111 | Line 1184 | Handle button clicks |
| WM_SYSCOMMAND | 0x112 | Line 1514 | Handle system menu (exit confirmation) |

## String Resources

| ID (hex) | ID (dec) | Description |
|----------|----------|-------------|
| 0x64 | 100 | Default installation path |
| 0x65 | 101 | Dialog title |
| 0x66 | 102 | Start menu folder name |
| 0x67 | 103 | Main game shortcut name |
| 0x68 | 104 | Uninstaller shortcut name |
| 0x69 | 105 | Readme shortcut name |
| 0x6A | 106 | Exit confirmation text |
| 0x6B | 107 | Exit confirmation caption |
| 0x6C | 108 | Insufficient disk space (CD) message |
| 0x6D | 109 | Insufficient disk space (CD) caption |
| 0x6E | 110 | DirectX install prompt text |
| 0x6F | 111 | DirectX install prompt caption |
| 0x70 | 112 | Insufficient disk space (HD) message |
| 0x71 | 113 | Insufficient disk space (HD) caption |
| 0x74 | 116 | Button text (Finish) |
| 0x75 | 117 | Button text (Exit) |
| 0x76 | 118 | Initial static text |
| 0x77 | 119 | Completion static text |
| 0x78 | 120 | Status: Preparing |
| 0x79 | 121 | Status: Installing files |
| 0x7A | 122 | Status: Installing DirectX |
| 0x7B | 123 | DirectX error message |
| 0x7C | 124 | Status: Copying files |
| 0x7D | 125 | Status: Copying file %s |
| 0x7E | 126 | Copy error: %s |
| 0x7F | 127 | Status: Creating shortcuts |
| 0x80 | 128 | DirectX reboot required message |
| 0x81 | 129 | Required CD space (in MB) |
| 0x82 | 130 | Required HD space (in MB) |
| 0x83 | 131 | Exit button text |
| 0x3EB | 1003 | Success message 1 |
| 0x3EC | 1004 | Success message 2 |
| 0x3ED | 1005 | Reboot required message |

## Global Variables

| Address | Name | Type | Description |
|---------|------|------|-------------|
| 0x40A048 | dword_40A048 | int | DirectX prompt shown flag |
| 0x40A050 | off_40A050 | char*[294] | Array of filenames to install |
| 0x40B804 | dword_40B804 | int | Installation state (0=initial, 1=complete) |
| 0x40B808 | dword_40B808 | int | Busy flag |
| 0x40BA10 | hInst | HINSTANCE | Module instance handle |
| 0x40BA18 | sz | char[260] | Setup.exe directory path |
| 0x40BB20 | String | char[260] | Installation target path |
| 0x40BC28 | dword_40BC28 | int | DirectX installed flag |

## File List (294 files)

Located at `off_40A050`, includes:

### Executables
- Ign_win.exe (Main game)
- remove.exe (Uninstaller)
- xruds137.exe (Unknown)
- DOS4GW.EXE (DOS extender)
- DOS_INST.EXE (DOS installer)
- AUTORUN.EXE (CD autorun)

### DirectX Runtime
- DSETUP.DLL
- DSETUP6E.DLL
- DSETUP6J.DLL
- DSETUPE.DLL
- DSETUPJ.DLL

### Data Files
- readme.txt
- Graphics: *.PIC files (H_pan1.pic, H_pan2.pic, etc.)
- Data: *.DAT, *.COL, *.TAB files
- Fonts and levels in subdirectories

### Directories Created
- Baltazar\
- CARS\
- DIRECTX\
- FONTS\
- GENERAL\
- GHOSTS\
- LEVELS\

(See README.md lines 40-100 for partial list)

## API Calls

### Implemented (Standard Win32)
- GetModuleFileNameA
- CharNextA
- LoadStringA
- GetModuleHandleA
- SetWindowTextA / GetWindowTextA
- GetDlgItem
- EnableWindow / ShowWindow
- SetDlgItemTextA
- SendDlgItemMessageA
- MessageBoxA
- DialogBoxParamA
- EndDialog
- GetWindowsDirectoryA
- GetDiskFreeSpaceA
- CreateDirectoryA
- SetFileAttributesA
- RegCreateKeyExA / RegSetValueExA / RegCloseKey
- lstrcpyA / strcat / strcpy / strlen
- _strupr / strstr
- wsprintfA / sprintf
- atoi
- MultiByteToWideChar
- LoadCursorA / SetCursor
- SetFocus

### Not Implemented (Shell32)
- SHBrowseForFolderA
- SHGetPathFromIDListA
- SHFileOperationA
- SHGetMalloc
- SHGetSpecialFolderLocation
- SHChangeNotify

### Not Implemented (COM)
- CoInitialize / CoUninitialize (basic stubs probably exist)
- CoCreateInstance (needs full implementation)

### Not Implemented (DirectX Setup)
- DirectXSetupA

## COM Interfaces

### CLSID_ShellLink
- CLSID: {00021401-0000-0000-C000-000000000046}
- Referenced at: hexrays.cpp:34

### IID_IShellLinkA
- IID: {000214EE-0000-0000-C000-000000000046}
- Referenced at: hexrays.cpp:35

### IShellLink vtable offsets
| Offset | Method | Description |
|--------|--------|-------------|
| 0 | QueryInterface | Get another interface |
| 4 | AddRef | Increment reference count |
| 8 | Release | Decrement reference count |
| ... | ... | ... |
| 28 | SetWorkingDirectory | Set working directory for shortcut |
| 36 | SetDescription | Set shortcut description |
| ... | ... | ... |
| 80 | SetPath | Set target executable path |

### IPersistFile vtable offsets
| Offset | Method | Description |
|--------|--------|-------------|
| 0 | QueryInterface | Get another interface |
| 4 | AddRef | Increment reference count |
| 8 | Release | Decrement reference count |
| ... | ... | ... |
| 24 | Save | Save .lnk file to disk |

## Installation Flow

```
WinMain
│
├─ CoInitialize()
├─ DialogBoxParamA()
│  │
│  └─ DialogFunc()
│     │
│     ├─ WM_INITDIALOG
│     │  ├─ Load strings
│     │  ├─ Set default path
│     │  └─ Initialize controls
│     │
│     ├─ WM_COMMAND (Browse button)
│     │  ├─ SHBrowseForFolderA()
│     │  └─ SHGetPathFromIDListA()
│     │
│     ├─ WM_COMMAND (Install button)
│     │  ├─ Validate path
│     │  ├─ Copy SMAG.INI (SHFileOperationA)
│     │  ├─ Ask about DirectX
│     │  ├─ Check disk space
│     │  ├─ DirectXSetupA() [optional]
│     │  ├─ Check disk space again
│     │  ├─ Copy 294 files (SHFileOperationA loop)
│     │  ├─ Create registry key
│     │  ├─ Get Start Menu path
│     │  ├─ Create shortcuts via sub_402360()
│     │  │  ├─ CoCreateInstance(CLSID_ShellLink)
│     │  │  ├─ IShellLink::SetPath()
│     │  │  ├─ IShellLink::SetWorkingDirectory()
│     │  │  ├─ IShellLink::SetDescription()
│     │  │  ├─ IShellLink::QueryInterface(IID_IPersistFile)
│     │  │  ├─ IPersistFile::Save()
│     │  │  └─ Release interfaces
│     │  └─ Show completion message
│     │
│     └─ WM_SYSCOMMAND (Close/Exit)
│        └─ Exit confirmation
│
└─ CoUninitialize()
```

## Key Decompiler Comparison

| Decompiler | Lines | Quality | Best For |
|------------|-------|---------|----------|
| Hex-Rays | 1,608 | ★★★★★ | Reading program flow, API calls |
| Reko | 2,464 | ★★★☆☆ | Cross-referencing, data structures |
| RecStudio | 2,383 | ★★★☆☆ | Alternative view |
| Binary Ninja | 5,542 | ★★★★☆ | Clean output, good for analysis |
| RetDec | 9,107 | ★★★☆☆ | Detailed but verbose |
| Rev.ng | 8,186 | ★★☆☆☆ | Alternative patterns |
| Snowman | 7,272 | ★★★☆☆ | Good for comparison |
| Boomerang | 13,187 | ★★☆☆☆ | Very verbose, experimental |

**Recommendation:** Start with Hex-Rays for understanding, cross-reference with Binary Ninja for details.

## Search Commands

### Find DirectX calls
```bash
grep -n "DirectX" hexrays.cpp
```

### Find file operations
```bash
grep -n "SHFileOperation\|CopyFile\|CreateDirectory" hexrays.cpp
```

### Find COM calls
```bash
grep -n "CoCreate\|CoInitialize\|QueryInterface" hexrays.cpp
```

### Find all API calls
```bash
grep -o "\b[A-Z][a-zA-Z0-9]*A\?\s*(" hexrays.cpp | sort -u
```

### Find string resource usage
```bash
grep -n "LoadStringA" hexrays.cpp
```

## Related Documentation

- `/Decomp/ign_install/setup.exe/README.md` - This directory's overview
- `/Decomp/ign_install/setup.exe/ANALYSIS.md` - Detailed analysis and recommendations
- `/Decomp/ign_teas/ANALYSIS.md` - COM vtable analysis for games (similar concepts)
- `/EXEs/ign_install/SETUP.EXE` - Original executable
- `/EXEs/ign_install/DSETUP.DLL` - DirectX setup library
