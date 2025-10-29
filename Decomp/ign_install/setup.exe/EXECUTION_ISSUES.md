# SETUP.EXE Execution Issues

## Current Status

Setup.exe is **partially working** but crashes during WM_INITDIALOG handling, causing the dialog to close immediately.

### What Works ✅
- CoInitialize() - COM initialization
- DialogBoxParamA() - Dialog creation and template parsing
- Dialog control creation (all 11 controls created successfully)
- Initial call to dialog procedure with WM_INITDIALOG
- Dialog window is created and shown to user

### What Fails ❌
- Dialog procedure execution crashes at NULL address (0x00000000)
- Crash occurs after ~1,043,322 instruction steps
- This typically indicates calling through a NULL function pointer
- **Dialog closes immediately after the crash, before user can interact**
- User sees the dialog flash on screen and disappear

## Error Details

```
fail: [User32] CallDialogProcedureAsync: Execution jumped to NULL address (0x00000000) at step 1043322
fail: [User32] CallDialogProcedureAsync: This typically means the code called a NULL function pointer
fail: [User32] CallDialogProcedureAsync: ESP=0x001FFF44 EBP=0x001FFFFC
fail: [User32] CallDialogProcedureAsync: Stack: 0x001FE6AF 0x00402346 0x00400000 0x0040A55C 0x00000000 0x00401130 0x00000000 0x00000000
```

## WM_INITDIALOG Handler Analysis

From hexrays.cpp lines 1163-1183, the WM_INITDIALOG handler performs:

```cpp
case 0x110u:  // WM_INITDIALOG
  dword_40B808 = 0;  // Clear busy flag
  dword_40B804 = 0;  // Clear state flag
  
  // 1. Set dialog title
  ModuleHandleA = GetModuleHandleA(0);
  LoadStringA(ModuleHandleA, 0x65u, ::Caption, 512);
  SetWindowTextA(hWnd, ::Caption);
  
  // 2. Disable Back and Help buttons
  DlgItem = GetDlgItem(hWnd, 1008);
  EnableWindow(DlgItem, 0);
  v8 = GetDlgItem(hWnd, 1007);
  EnableWindow(v8, 0);
  
  // 3. Load and display installer image (IGNPIC resource)
  v9 = GetModuleHandleA(0);
  LoadStringA(v9, 0x76u, Buffer, 512);
  sub_401050(hWnd, 1002, Buffer, 175, 195);  // ⚠️ POTENTIAL ISSUE HERE
  
  // 4. Set installation path text box
  SendDlgItemMessageA(hWnd, 1000, 0xC5u, 0x104u, 0);  // EM_LIMITTEXT
  v10 = GetModuleHandleA(0);
  LoadStringA(v10, 0x64u, Buffer, 512);
  SetDlgItemTextA(hWnd, 1000, Buffer);
  SendDlgItemMessageA(hWnd, 1000, 0xB1u, 0, 16777472);  // EM_SETSEL
  
  // 5. Set focus to path text box
  v11 = GetDlgItem(hWnd, 1000);
  SetFocus(v11);
  
  return 0;
```

## sub_401050: Image Loading Function

This helper function loads an image resource and displays it in a static control:

```cpp
HWND __cdecl sub_401050(HWND hDlg, int nIDDlgItem, LPCSTR name, int a4, int cy)
{
  HWND result;
  HWND v6;

  result = GetDlgItem(hDlg, nIDDlgItem);  // Get control handle
  v6 = result;
  if ( result )
  {
    // Load image resource (IGNPIC) with size 175x195
    result = (HWND)LoadImageA(hInst, name, 0, a4, cy, 0x3020u);
    if ( result )
    {
      // Send STM_SETIMAGE (0x172) to static control
      result = (HWND)SendMessageA(v6, 0x172u, 0, (LPARAM)result);
      if ( result )
        // Delete old image if one was returned
        return (HWND)DeleteObject(result);
    }
  }
  return result;
}
```

Parameters when called:
- `hDlg` = Dialog window handle (0x00001000)
- `nIDDlgItem` = 1002 (static control ID)
- `name` = String loaded from resource 0x76 (likely "IGNPIC")
- `a4` = 175 (width)
- `cy` = 195 (height)

## API Implementation Status

All APIs called in WM_INITDIALOG are implemented:

| API | Module | Status |
|-----|--------|--------|
| GetModuleHandleA | KERNEL32 | ✅ Implemented |
| LoadStringA | USER32 | ✅ Implemented |
| SetWindowTextA | USER32 | ✅ Implemented |
| GetDlgItem | USER32 | ✅ Implemented |
| EnableWindow | USER32 | ✅ Implemented |
| LoadImageA | USER32 | ✅ Implemented (stub) |
| SendMessageA | USER32 | ✅ Implemented |
| DeleteObject | GDI32 | ✅ Implemented |
| SendDlgItemMessageA | USER32 | ✅ Implemented |
| SetDlgItemTextA | USER32 | ✅ Implemented |
| SetFocus | USER32 | ✅ Implemented |

## Root Cause Hypotheses

### Hypothesis 1: Missing Import or NULL Function Pointer
- Some Win32 API function that's called indirectly is not in the import table
- The code calls through the import table entry which is NULL
- After many instructions, it eventually executes the NULL pointer

### Hypothesis 2: String API or Helper Function
- One of the string manipulation functions (lstrlenA, lstrcatA, etc.) might be missing
- Or a runtime library function (memset, etc.)

### Hypothesis 3: Resource Loading Issue
- LoadStringA or LoadImageA returns success but with invalid data
- Subsequent code that processes the resource data crashes
- The resource name "IGNPIC" might not exist or be malformed

### Hypothesis 4: Stack Corruption
- An API is not cleaning up the stack correctly (__stdcall vs __cdecl mismatch)
- Stack gets corrupted over many function calls
- Eventually returns to address 0x00000000

### Hypothesis 5: Emulator Execution Bug
- The emulator's instruction stepping has a bug
- It's not correctly handling certain instructions (like indirect calls)
- Eventually gets into an invalid state

## Diagnostic Steps

### Step 1: Check All String APIs
Verify these are implemented and work correctly:
- lstrlenA
- lstrcatA
- lstrcpyA
- wvsprintfA (used in sub_4010B0)

### Step 2: Check Runtime Library Functions
Verify CRT functions that might be called:
- memset
- memcpy
- _strupr (used later in installation)
- va_start / va_end (for variable args)

### Step 3: Examine Import Table
- Dump the import table of setup.exe
- Check if all imported functions are present
- Look for any NULL entries

### Step 4: Add More Detailed Logging
- Log every function entry/exit in the dialog procedure
- Log register state at each step
- Try to identify the exact instruction that jumps to NULL

### Step 5: Disassemble Around the Crash Point
- Look at the last valid EIP before the NULL jump
- Disassemble that area to see what instruction caused the jump
- Check if it's a `call`, `jmp`, or `ret` instruction

## Recommended Fixes

### Critical Fix 1: Handle Dialog Procedure Failures Gracefully
**Problem:** When the dialog procedure crashes, DialogBoxParamAsync immediately closes the dialog.

**Solution:** Modify DialogBoxParamAsync to:
1. Catch execution failures during WM_INITDIALOG
2. Instead of closing the dialog, keep it open
3. Log the error but let the user interact with the dialog
4. This allows debugging and may let some functionality work

Example:
```csharp
// In User32Module.DialogBoxParamAsync
try {
    var initResult = await CallDialogProcedureAsync(...);
} catch (Exception ex) {
    _logger.LogError("Dialog procedure failed during WM_INITDIALOG: {Ex}", ex);
    // Don't close the dialog - keep it open for user interaction
    // The user can still click Cancel to exit
}
```

### Critical Fix 2: Identify and Fix the NULL Pointer Call
**Problem:** Code is jumping to address 0x00000000 after many instruction steps.

**Investigation needed:**
1. Add instruction-level logging before the crash
2. Log the last 10-20 instructions before hitting NULL
3. Identify which API call or indirect call is causing the jump
4. Check if it's a missing import or a runtime library function

### Quick Fix: Skip Image Loading
Modify sub_401050 to skip the image loading:
- Make LoadImageA return NULL (0)
- This causes sub_401050 to return early without crash
- Dialog can still initialize without the image

### Medium Fix: Implement Proper Resource Loading
- LoadStringA needs to actually load strings from resources
- LoadImageA needs to actually load images from resources
- Or at minimum return valid stub data that won't cause crashes

### Long Fix: Debug the Root Cause
- Use enhanced debugging mode to trace execution
- Find the exact NULL function pointer being called
- Implement the missing function or fix the caller

## Impact

This crash **blocks the installer from running** properly. The dialog window appears briefly but closes immediately before the user can interact with it.

**Sequence of events:**
1. Dialog window is created and displayed ✅
2. WM_INITDIALOG is sent to initialize the dialog
3. Dialog procedure crashes during initialization ❌
4. DialogBoxParamAsync detects the failure
5. Dialog is immediately cleaned up and closed
6. Program exits
7. User sees dialog flash on screen and disappear

**Result:** The installer is completely unusable - the user never gets a chance to click "Install" or interact with the UI.

Priority: **CRITICAL** - Without fixing this, setup.exe cannot be used at all.

## Workaround

For now, **manual installation** is required:
1. Extract files from CD manually
2. Copy to desired location
3. Create shortcuts manually
4. Edit registry manually (if needed)

Or use the game directly without installer (if IGN_WIN.EXE can run standalone).

## Next Steps

1. **Enable verbose logging** - Add more detailed trace logging in User32Module dialog handling
2. **Check missing APIs** - Verify all string and resource APIs are implemented
3. **Test with simpler dialog** - Create a minimal test dialog to isolate the issue
4. **Review emulator execution** - Check if there's a bug in how indirect calls are handled

## Related Files

- `/Decomp/ign_install/setup.exe/ANALYSIS.md` - Full installer analysis
- `/Win32Emu/Win32/Modules/User32Module.cs` - Dialog handling implementation
- `/Win32Emu/Win32/Modules/Kernel32Module.cs` - Resource loading
- `/EXEs/ign_install/SETUP.EXE` - Original executable
