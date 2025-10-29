# Ignition Game - Installation Files Analysis

## Overview

This directory contains decompilation analysis for the Ignition game installer components.

## Contents

### setup.exe/
Complete analysis of the SETUP.EXE installer executable with decompilations from 8 different tools.

**See:** [setup.exe/SUMMARY.md](setup.exe/SUMMARY.md) for executive summary

**Key files:**
- [setup.exe/README.md](setup.exe/README.md) - Overview and usage guide
- [setup.exe/ANALYSIS.md](setup.exe/ANALYSIS.md) - Detailed technical analysis
- [setup.exe/INDEX.md](setup.exe/INDEX.md) - Quick reference
- [setup.exe/EXECUTION_ISSUES.md](setup.exe/EXECUTION_ISSUES.md) - Current runtime problems

## What is Ignition?

Ignition is a racing game from the mid-1990s that includes:
- **IGN_WIN.EXE** - Main Windows game executable (DirectX-based)
- **SETUP.EXE** - Windows installer for the game
- **DOS version** - Alternative DOS-based version
- **294 data files** - Graphics, sounds, levels, etc.

## Installer Analysis Summary

**SETUP.EXE** is a standard Windows 95 installer that:
1. Shows dialog-based UI for installation options
2. Browses for installation directory
3. Optionally installs DirectX runtime
4. Copies 294 game files from CD to hard drive
5. Creates Start Menu shortcuts
6. Writes registry entries

**Current Status in Win32Emu:**
- ✅ Dialog creation works
- ✅ Window appears on screen
- ❌ **Crashes during WM_INITDIALOG** (jumps to NULL address)
- ❌ Dialog closes before user can interact
- ⚠️ Shell32 file operations stubbed but not fully implemented
- ❌ COM shortcuts not implemented

**Priority:** Medium (game itself is higher priority)

## Required Win32 APIs

### Critical for Installer
1. **Fix dialog crash** - Unknown cause, needs debugging
2. **SHFileOperationA** - File copying (exists but stubbed)
3. **SHBrowseForFolderA** - Folder selection (exists but stubbed)

### Important for Full Installation
4. **IShellLink COM** - Create shortcuts
5. **IPersistFile COM** - Save .lnk files
6. **DirectXSetupA** - Can be trivially stubbed

### Already Implemented
- All standard Win32 dialog and window APIs
- Registry APIs
- Basic COM initialization
- Resource loading (stubbed)

## Relationship to Game

The **installer** and **game** have different requirements:

| Component | SETUP.EXE | IGN_TEAS.EXE / IGN_WIN.EXE |
|-----------|-----------|--------------|
| **Type** | Installer utility | Game executable |
| **UI** | Standard dialogs | DirectDraw rendering |
| **File I/O** | Heavy writes (copy files) | Read-only (load data) |
| **COM** | IShellLink for shortcuts | DirectX COM vtables |
| **Priority** | Medium | High |
| **Workaround** | Manual install possible | None |

**Note:** IGN_TEAS.EXE is the game teaser/demo, IGN_WIN.EXE is the full game. Both use DirectX and have similar COM requirements.

**See:** `../ign_teas/ANALYSIS.md` for game teaser analysis

## Implementation Recommendations

### Strategy 1: Focus on Game First (Recommended)
1. Fix IGN_TEAS.EXE DirectX COM vtable issues
2. Get the game playable
3. Use manual installation as workaround
4. Come back to installer later if needed

**Rationale:**
- Game is the end goal, installer is just a utility
- Manual installation is straightforward (just copy files)
- Users care more about playing the game than using the installer

### Strategy 2: Complete Installer Support
1. Debug and fix dialog crash in SETUP.EXE
2. Implement SHFileOperationA for file copying
3. Implement folder browsing APIs
4. Add COM support for shortcuts
5. Test full installation flow

**Effort:** ~1-2 weeks
**Benefit:** Demonstrates emulator can run real Windows apps (not just games)

### Strategy 3: Hybrid Approach
1. Fix the dialog crash only (enables basic installer UI)
2. Implement SHFileOperationA (enables file copying)
3. Stub remaining features (shortcuts, folder browse)
4. This gives a "mostly working" installer with minimal effort

**Effort:** ~3-4 days
**Benefit:** Usable installer without full feature parity

## Current Blockers

### Blocker 1: Dialog Crash (CRITICAL)
**Issue:** SETUP.EXE crashes during WM_INITDIALOG message handling
- Code jumps to NULL address (0x00000000) after ~1M instruction steps
- Dialog closes immediately before user can interact
- Installer is completely unusable

**Root Cause:** Unknown - needs debugging
- All Win32 APIs are implemented
- Likely a missing CRT function or emulator execution bug
- Could be stack corruption or indirect call issue

**Next Steps:**
1. Add instruction-level logging
2. Identify the exact jump to NULL
3. Check for missing string/memory functions
4. Fix the issue or modify DialogBoxParamAsync to keep dialog open

### Blocker 2: File Operations (HIGH)
**Issue:** SHFileOperationA exists but is stubbed
- Returns success without actually copying files
- Installation completes but no files are installed

**Impact:** Without this, manual file copying is required

**Effort to fix:** 2-3 days

## Testing Approach

### Minimal Test (Dialog Only)
```bash
Win32Emu EXEs/ign_install/SETUP.EXE --debug
```
**Expected:** Dialog appears and stays open (after fixing crash)
**Success:** User can click Cancel to exit cleanly

### Partial Test (File Operations)
```bash
Win32Emu EXEs/ign_install/SETUP.EXE --debug
```
1. Click "Install" button
2. Verify SHFileOperationA is called
3. Check if files are copied

**Success:** Files copied to installation directory

### Full Test (Complete Installation)
```bash
Win32Emu EXEs/ign_install/SETUP.EXE
```
1. Complete installation flow
2. Verify all 294 files copied
3. Check registry entries created
4. Check shortcuts created
5. Launch installed game

**Success:** Can play game from installed location

## Related Files

### Executables
- `../../EXEs/ign_install/SETUP.EXE` - Installer (analyzed here)
- `../../EXEs/ign_install/IGN_WIN.EXE` - Main game (full version)
- `../../EXEs/ign_install/REMOVE.EXE` - Uninstaller
- `../../EXEs/ign_teas/IGN_TEAS.EXE` - Game teaser/demo (separate from IGN_WIN.EXE)

### Decompilations
- `setup.exe/*.cpp` - 8 decompiler outputs for SETUP.EXE installer
- `../ign_teas/*.cpp` - Decompilations for IGN_TEAS.EXE game teaser

### Emulator Code
- `../../Win32Emu/Win32/Modules/Shell32Module.cs` - Shell APIs
- `../../Win32Emu/Win32/Modules/Ole32Module.cs` - COM APIs
- `../../Win32Emu/Win32/Modules/User32Module.cs` - Dialog APIs

## Quick Reference

**Want to understand installer logic?**
→ Read [setup.exe/README.md](setup.exe/README.md)

**Want detailed technical analysis?**
→ Read [setup.exe/ANALYSIS.md](setup.exe/ANALYSIS.md)

**Want to debug the crash?**
→ Read [setup.exe/EXECUTION_ISSUES.md](setup.exe/EXECUTION_ISSUES.md)

**Want to see function offsets and API calls?**
→ Read [setup.exe/INDEX.md](setup.exe/INDEX.md)

**Want high-level summary?**
→ Read [setup.exe/SUMMARY.md](setup.exe/SUMMARY.md)

## Conclusion

The Ignition installer (SETUP.EXE) is **mostly implementable** with the Win32Emu emulator. The main challenges are:

1. ✅ **Feasible:** Standard Win32 dialog APIs (already working)
2. ⚠️ **Partially done:** Shell32 file operations (stubbed)
3. ⚠️ **Needs work:** COM for shortcuts (similar to game's DirectX COM)
4. ❌ **Blocked:** Current dialog crash needs debugging

**Recommendation:** Focus on the game first, use manual installation for now.

The infrastructure built for the game (COM support, file I/O) will make the installer easier to implement later. The installer is valuable but not critical - users can manually copy files and create shortcuts as a workaround.

**Total effort for complete installer support:** 1-2 weeks (after game is working)
