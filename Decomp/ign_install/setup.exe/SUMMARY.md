# Setup.exe Decompilation Analysis - Summary

## Overview

This directory contains comprehensive analysis of the SETUP.EXE installer from the Ignition game CD, including decompilation outputs from 8 different decompilers and detailed documentation.

## Quick Links

- **[README.md](README.md)** - Overview of decompiler outputs and how to use them
- **[ANALYSIS.md](ANALYSIS.md)** - Detailed technical analysis of installer functionality
- **[INDEX.md](INDEX.md)** - Quick reference for functions, APIs, and data structures
- **[EXECUTION_ISSUES.md](EXECUTION_ISSUES.md)** - Current runtime problems and debugging info
- **[APIMON_LOG_ANALYSIS.md](APIMON_LOG_ANALYSIS.md)** - Runtime API call analysis from real Windows execution

## Key Findings

### What SETUP.EXE Does

SETUP.EXE is a Windows 95-era installer application that:

1. **Shows a dialog-based UI** for user interaction
2. **Browses for installation directory** using Shell32 folder picker
3. **Optionally installs DirectX** via DirectXSetupA from DSETUP.DLL
4. **Copies 294 game files** from CD to hard drive using SHFileOperationA
5. **Creates registry entries** to store installation path
6. **Creates Start Menu shortcuts** using COM (IShellLink/IPersistFile interfaces)

### Current Status in Win32Emu

| Component | Status | Notes |
|-----------|--------|-------|
| **Basic Win32 APIs** | ✅ Working | Dialog creation, controls, messages all work |
| **COM Initialization** | ✅ Working | CoInitialize/CoUninitialize implemented |
| **Dialog Display** | ✅ Working | Dialog window appears on screen |
| **WM_INITDIALOG** | ❌ **CRASHES** | Jumps to NULL after ~1M instructions |
| **User Interaction** | ❌ Blocked | Dialog closes before user can click anything |
| **Shell32 File Ops** | ⚠️ Stubbed | SHFileOperationA exists but not fully implemented |
| **Shell32 Folder Browse** | ⚠️ Stubbed | SHBrowseForFolderA exists but not fully implemented |
| **COM Shortcuts** | ❌ Not Implemented | IShellLink/IPersistFile COM interfaces missing |
| **DirectX Setup** | ❌ Not Implemented | DirectXSetupA not emulated |

### Critical Issue: Dialog Crashes During Initialization

**Problem:** The installer dialog crashes during WM_INITDIALOG message handling, causing it to close immediately.

**Symptoms:**
- Dialog window appears briefly on screen
- Crashes after executing ~1,043,322 instructions
- Code jumps to address 0x00000000 (NULL pointer)
- Dialog closes before user can interact
- Program exits

**Impact:** Installer is completely unusable.

**See:** [EXECUTION_ISSUES.md](EXECUTION_ISSUES.md) for detailed analysis.

## Required Win32 APIs

### Already Implemented ✅
- Standard dialog APIs (DialogBoxParamA, GetDlgItem, EnableWindow, etc.)
- String loading (LoadStringA)
- Window management (SetWindowTextA, SetFocus, etc.)
- Message handling (SendMessageA, SendDlgItemMessageA)
- Resource loading (LoadImageA - stubbed)
- Registry APIs (RegCreateKeyExA, RegSetValueExA, etc.)

### Partially Implemented ⚠️
- **SHFileOperationA** - Exists as stub, needs full implementation for file copying
- **SHBrowseForFolderA** - Exists as stub, needs implementation for folder selection
- **SHGetPathFromIDListA** - Exists as stub
- **SHGetSpecialFolderLocation** - Exists as stub
- **SHChangeNotify** - Exists as stub
- **SHGetMalloc** - Exists as stub

### Not Implemented ❌
- **DirectXSetupA** - DirectX installer API (can be stubbed)
- **IShellLink COM interface** - For creating shortcuts
- **IPersistFile COM interface** - For saving .lnk files
- **CoCreateInstance** - Needs support for CLSID_ShellLink

## Implementation Priorities

### Priority 1: CRITICAL - Fix Dialog Crash
**Goal:** Get the dialog to stay open and be responsive

**Tasks:**
1. Debug the NULL pointer jump in WM_INITDIALOG
2. Identify which API call or instruction is causing it
3. Fix the missing function or emulator bug
4. Ensure dialog remains open for user interaction

**Estimated Effort:** 1-2 days

### Priority 2: HIGH - Shell32 File Operations
**Goal:** Enable file copying during installation

**Tasks:**
1. Implement SHFileOperationA for FO_COPY and FO_DELETE operations
2. Integrate with emulator's VFS for file I/O
3. Test copying the 294 game files

**Estimated Effort:** 2-3 days

**Impact:** Without this, no files get installed.

### Priority 3: MEDIUM - Shell32 Folder Browsing
**Goal:** Allow user to select installation directory

**Tasks:**
1. Implement SHBrowseForFolderA to show folder picker
2. Implement SHGetPathFromIDListA to convert result to path
3. Option: Use native folder dialog or accept command-line path

**Estimated Effort:** 1-2 days

**Impact:** User can't choose where to install. Could use default path as workaround.

### Priority 4: LOW - COM Shortcuts
**Goal:** Create Start Menu shortcuts

**Tasks:**
1. Implement CoCreateInstance for CLSID_ShellLink
2. Create IShellLink COM interface with vtable
3. Create IPersistFile COM interface with vtable
4. Generate .lnk files or log the calls

**Estimated Effort:** 2-3 days

**Impact:** No shortcuts created, but game can still run. Manual shortcut creation is easy workaround.

### Priority 5: TRIVIAL - DirectX Setup Stub
**Goal:** Skip DirectX installation gracefully

**Tasks:**
1. Implement DirectXSetupA to return S_OK immediately
2. Log that DirectX installation is skipped
3. Emulator doesn't need DirectX anyway

**Estimated Effort:** 1 hour

**Impact:** Minimal. DirectX is not needed by the emulator.

## Comparison with IGN_TEAS.EXE

| Aspect | Setup.exe | Ign_teas.exe |
|--------|-----------|--------------|
| **Type** | Installer utility | Game executable |
| **Priority** | Medium | High |
| **UI** | Standard dialogs | DirectDraw rendering |
| **File I/O** | Heavy (294 files) | Read-only (game data) |
| **COM Usage** | IShellLink for shortcuts | DirectX COM vtables for gameplay |
| **Blocking Issue** | Dialog crash | Missing DirectX COM vtables |
| **Workaround** | Manual install | None |

**Recommendation:** Fix IGN_TEAS.EXE first (it's the actual game), then use manual installation for now. Come back to SETUP.EXE later if needed.

## Testing Strategy

### Phase 1: Fix Dialog Crash
1. Add detailed instruction logging
2. Identify NULL pointer source
3. Fix the issue
4. Verify dialog stays open and is responsive

**Success Criteria:** User can see and interact with dialog (click Cancel to exit).

### Phase 2: Test File Operations
1. Click "Install" button in dialog
2. Verify SHFileOperationA is called
3. Verify files are copied to destination
4. Check all 294 files are present

**Success Criteria:** Files are successfully copied.

### Phase 3: Test Complete Installation
1. Run full installation flow
2. Verify registry entries created
3. Verify shortcuts created (if implemented)
4. Launch installed game

**Success Criteria:** Game can be launched from installed location.

## Documentation Structure

```
Decomp/ign_install/setup.exe/
├── README.md                 # Overview and how to use decomps
├── ANALYSIS.md               # Detailed technical analysis
├── INDEX.md                  # Quick reference guide
├── EXECUTION_ISSUES.md       # Current runtime problems
├── SUMMARY.md                # This file
├── APIMON_LOG_ANALYSIS.md    # Runtime API call analysis
├── binaryninja.cpp           # Binary Ninja decompilation (5,542 lines)
├── boomerang.cpp             # Boomerang decompilation (13,187 lines)
├── hexrays.cpp               # IDA Hex-Rays decompilation (1,608 lines) ⭐ BEST
├── recstudio.cpp             # RecStudio decompilation (2,383 lines)
├── reko.cpp                  # Reko decompilation (2,464 lines)
├── retdec.cpp                # RetDec decompilation (9,107 lines)
├── rev.ng.cpp                # Rev.ng decompilation (8,186 lines)
└── snowman.cpp               # Snowman decompilation (7,272 lines)
```

**⭐ Recommended:** Start with `hexrays.cpp` for cleanest, most readable code. Use `APIMON_LOG_ANALYSIS.md` to validate with actual runtime behavior.

## Related Files

- `/EXEs/ign_install/SETUP.EXE` - Original executable
- `/EXEs/ign_install/*.DLL` - DirectX setup DLLs
- `/EXEs/ign_install/*.*` - 294 game files to be installed
- `/ApiMon Logs/ign_install/setup.exe.log` - Runtime API call log from real Windows
- `/Decomp/ign_teas/ANALYSIS.md` - Similar COM analysis for game exe
- `/Win32Emu/Win32/Modules/Shell32Module.cs` - Shell32 API implementation
- `/Win32Emu/Win32/Modules/Ole32Module.cs` - COM implementation
- `/Win32Emu/Win32/Modules/User32Module.cs` - Dialog implementation

## Contributing

When working on SETUP.EXE support:

1. **Read EXECUTION_ISSUES.md first** - Understand the current crash
2. **Use hexrays.cpp** - Cleanest decompilation for understanding code
3. **Cross-reference with other decompilers** - Verify unclear sections
4. **Update documentation** - Add findings to ANALYSIS.md or EXECUTION_ISSUES.md
5. **Test incrementally** - Fix one issue at a time, verify each fix

## Conclusion

SETUP.EXE is a **standard Windows installer** that is mostly within reach for the emulator. The main challenges are:

1. **Immediate:** Fix the dialog crash (CRITICAL)
2. **Short-term:** Implement Shell32 file operations (HIGH)
3. **Long-term:** Add COM support for shortcuts (LOW)

The installer is **less critical than the game itself** (IGN_TEAS.EXE), so it's reasonable to:
- Focus on fixing the game first
- Use manual installation as a workaround
- Come back to the installer later if desired

However, fixing the dialog crash would be valuable as it **demonstrates the emulator can run real-world Windows applications** beyond just games, which is good for the project's credibility and usefulness.

**Total effort to make installer work:** ~1-2 weeks
**Total effort to make it work well:** ~2-3 weeks

The Shell32 and COM infrastructure built for the installer would also benefit other applications, making it a worthwhile investment if time permits.
