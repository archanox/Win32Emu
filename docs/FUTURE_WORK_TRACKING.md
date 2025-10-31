# Future Work Tracking

This document consolidates all "what's next" and future work items mentioned across the documentation. Use this as a basis for creating GitHub issues to track implementation progress.

---

## 🎯 High Priority Items

### GUI API Integration - Phase 4
**Source**: [docs/gui/API_INTEGRATION.md](gui/API_INTEGRATION.md)

**GitHub Issue Template**:
```
Title: Implement Window Procedure Callbacks and Message Queue (Phase 4)

Description:
Complete Phase 4 of the GUI API integration by implementing:
- Window procedure callbacks
- Real message queue with input routing

This builds on the completed Phase 3 (message loop infrastructure) and will enable full window message handling.

Related: docs/gui/API_INTEGRATION.md
```

### Window Implementation Enhancement
**Source**: [docs/implementation/WINDOW_IMPLEMENTATION.md](implementation/WINDOW_IMPLEMENTATION.md)

**Create separate issues for**:
1. **Message Queue and Processing**
   - Implement message queue infrastructure
   - Add support for window procedure callbacks
   
2. **Visual Rendering Integration**
   - Integrate with Avalonia UI for visual rendering (as per API_INTEGRATION.md)
   
3. **Window Management Functions**
   - Implement ShowWindow, UpdateWindow, DestroyWindow
   
4. **System Window Classes**
   - Register system window classes (BUTTON, EDIT, LISTBOX, etc.)
   
5. **GDI Drawing Functions**
   - Add BeginPaint, EndPaint, and other GDI drawing functions
   - Implement DefWindowProc for default message handling

---

## 🔧 Core API Enhancements

### GetProcAddress Enhancement
**Source**: [docs/implementation/GETPROCADDRESS_IMPLEMENTATION.md](implementation/GETPROCADDRESS_IMPLEMENTATION.md)

**GitHub Issue Template**:
```
Title: Enhance GetProcAddress with Forwarded Exports and LoadLibrary Integration

Description:
Current limitations:
- Forwarded exports are not supported
- LoadLibraryA doesn't use PeImageLoader properly
- Missing integration tests

Tasks:
- [ ] Implement forwarded export resolution
- [ ] Enhance LoadLibraryA to use PeImageLoader for DLL loading
- [ ] Add integration tests with actual PE files containing exports
- [ ] Support export name hints for optimization

Related: docs/implementation/GETPROCADDRESS_IMPLEMENTATION.md
```

### Multithreading API Completion
**Source**: [docs/implementation/MULTITHREADING_IMPLEMENTATION.md](implementation/MULTITHREADING_IMPLEMENTATION.md)

**GitHub Issue Template**:
```
Title: Implement Missing Threading APIs

Description:
Complete the multithreading implementation by adding:
- [ ] WaitForMultipleObjects
- [ ] TerminateThread
- [ ] GetExitCodeThread
- [ ] SetThreadPriority / GetThreadPriority
- [ ] InterlockedIncrement / InterlockedDecrement
- [ ] InterlockedCompareExchange

Related: docs/implementation/MULTITHREADING_IMPLEMENTATION.md
```

### DirectDraw Enhancement
**Source**: [docs/implementation/DDRAW_STUBS_IMPLEMENTATION.md](implementation/DDRAW_STUBS_IMPLEMENTATION.md)

**GitHub Issue Template**:
```
Title: Enhance DirectDraw with Enumeration and Advanced Features

Description:
Improve DirectDraw support by implementing:
- [ ] Enumeration functions (EnumDisplayModes, EnumSurfaces) for mode selection
- [ ] Full IDirectDrawClipper support
- [ ] Full overlay surface support (if needed by applications)
- [ ] Real device context creation for GetDC/ReleaseDC
- [ ] Track COM object addresses for surfaces to enable GetGDISurface

Related: docs/implementation/DDRAW_STUBS_IMPLEMENTATION.md
```

---

## 🚀 CPU and JIT Improvements

### Pentium JIT Implementation - Priority 1: Common Instructions
**Source**: [docs/implementation/PENTIUM_JIT_STUBS.md](implementation/PENTIUM_JIT_STUBS.md)

**GitHub Issue Template**:
```
Title: Implement Priority 1 Pentium Instructions (Common Instructions)

Description:
Implement frequently-used instructions essential for control flow and bit manipulation:
- [ ] Conditional jumps (JE, JNE, JA, JG, etc.) - essential for control flow
- [ ] Bit test (BT, BTS, BTR, BTC) - commonly used in bit manipulation
- [ ] Double shifts (SHLD, SHRD) - used in advanced bit operations

Priority: High
Related: docs/implementation/PENTIUM_JIT_STUBS.md
```

### Pentium JIT Implementation - Priority 2: Compatibility
**Source**: [docs/implementation/PENTIUM_JIT_STUBS.md](implementation/PENTIUM_JIT_STUBS.md)

**GitHub Issue Template**:
```
Title: Implement Priority 2 Pentium Instructions (Compatibility)

Description:
Implement instructions for FPU state management and segment operations:
- [ ] FPU control instructions (FNINIT, FNCLEX, FSTSW) - needed for FPU state management
- [ ] Segment loads (LDS, LES, LFS, LGS, LSS) - for segment register operations
- [ ] BOUND, ENTER - for stack frame management

Priority: Medium
Related: docs/implementation/PENTIUM_JIT_STUBS.md
```

### Pentium JIT Implementation - Priority 3: Performance
**Source**: [docs/implementation/PENTIUM_JIT_STUBS.md](implementation/PENTIUM_JIT_STUBS.md)

**GitHub Issue Template**:
```
Title: Implement Priority 3 Pentium Instructions (Performance)

Description:
Implement performance-oriented instructions for multimedia and modern optimizations:
- [ ] MMX instructions - for multimedia applications
- [ ] Conditional moves (CMOV*) - modern compiler optimization

Priority: Low
Related: docs/implementation/PENTIUM_JIT_STUBS.md
```

---

## 🎮 Graphics, Audio, and Input

### COM Backend Implementation
**Source**: [docs/fixes/COM_VTABLE_FIX_SUMMARY.md](fixes/COM_VTABLE_FIX_SUMMARY.md)

**GitHub Issue Template**:
```
Title: Implement Real Backends for DirectDraw, DirectInput, and DirectSound

Description:
The COM infrastructure is complete. Implement actual backends:
- [ ] Implement actual rendering - DirectDraw surface operations need real graphics backend
- [ ] Implement actual input - DirectInput device state needs real input backend
- [ ] Implement actual audio - DirectSound buffers need real audio backend

Note: Games should start and run without crashing, but need these backends for full functionality.

Related: docs/fixes/COM_VTABLE_FIX_SUMMARY.md
```

### macOS Metal Enhancement
**Source**: [docs/fixes/MACOS_METAL_FIX.md](fixes/MACOS_METAL_FIX.md)

**GitHub Issue Template**:
```
Title: Enhance macOS Metal Backend with Advanced Features

Description:
While the current Metal implementation provides a solid foundation, add:
- [ ] Custom shader support for advanced effects
- [ ] Multiple render targets (multi-texture)
- [ ] 3D graphics emulation using GPU API
- [ ] Compute passes for image processing

Priority: Enhancement
Platform: macOS only
Related: docs/fixes/MACOS_METAL_FIX.md
```

---

## 🔨 Developer Tools

### Calling Convention Standardization
**Source**: [docs/implementation/CALLING_CONVENTION_STANDARDIZATION.md](implementation/CALLING_CONVENTION_STANDARDIZATION.md)

**GitHub Issue Template**:
```
Title: Enhance Code Generation with Advanced Features

Description:
Improve the calling convention and code generation system:
- [ ] Add struct definitions - Parse typedef elements from XML
- [ ] Generate callback wrappers - Generate delegate types for callbacks
- [ ] Add COM interface generation - Auto-generate vtable dispatch
- [ ] Add validation mode - Check existing code against XML
- [ ] Add documentation generation - Extract MSDN-style docs
- [ ] Add unit test generation - Create test templates

Related: docs/implementation/CALLING_CONVENTION_STANDARDIZATION.md
```

---

## 🐛 Bug Investigations

### Unmapped Import Investigation
**Source**: [docs/fixes/UNMAPPED_IMPORT_FIX.md](fixes/UNMAPPED_IMPORT_FIX.md)

**GitHub Issue Template**:
```
Title: Investigate Unmapped Import Stack Corruption

Description:
Investigate and resolve stack corruption related to unmapped imports:
- [ ] Investigate why return address 0x0F000530 appears on the stack
- [ ] Determine if there's an IAT entry that shouldn't be there
- [ ] Check if the C runtime initialization is correct
- [ ] Add validation of the stack after each import call to detect corruption early

Type: Bug Investigation
Related: docs/fixes/UNMAPPED_IMPORT_FIX.md
```

---

## 📋 How to Use This Document

1. **Review the items above** and prioritize based on project needs
2. **Create GitHub issues** using the provided templates
3. **Add labels** such as `enhancement`, `bug`, `priority:high`, `area:graphics`, etc.
4. **Link related issues** when creating dependencies
5. **Update this document** as items are completed or priorities change

## 📝 Notes

- All items are sourced from active documentation (not archived)
- Archived documentation items were intentionally excluded as they represent completed historical work
- Some items may overlap and should be combined when creating issues
- Priority levels are suggestions based on documentation context
