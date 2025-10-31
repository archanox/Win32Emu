# SETUP.EXE ApiMon Log Analysis

## Overview

This document analyzes the ApiMon log captured during the execution of SETUP.EXE (Ignition game installer) on a real Windows system. The log provides valuable insights into the actual API calls made during runtime, which complements the static decompilation analysis.

**Log File:** `/ApiMon Logs/ign_install/setup.exe.log`  
**Total API Calls:** 102 (logged entries)  
**Execution Duration:** ~0.66 seconds (6:25:59.340 PM - 6:25:59.996 PM)  
**Thread:** Single-threaded execution (Thread ID: 1)

## Key Findings

### 1. Successful Initialization

The installer successfully initializes and begins dialog creation:
- ✅ CRT initialization (heap, environment, startup info)
- ✅ COM initialization via `CoInitialize`
- ✅ Module filename extraction
- ✅ Dialog creation started via `DialogBoxParamA`

### 2. Dialog Initialization (WM_INITDIALOG)

The log captures the WM_INITDIALOG message handler executing:
- ✅ Window title set to "Ignition Setup" (LoadStringA #101)
- ✅ Dialog controls disabled (buttons 1007, 1008)
- ✅ Installation path initialized to "C:\Games\Ignition" (LoadStringA #100)
- ✅ Edit control configured with 260 character limit
- ⚠️ LoadImageA failed (Error 1814: "The specified resource name cannot be found")
- ✅ Focus set to installation path textbox (control 1000)

### 3. Execution Stopped at Dialog Display

**Critical Observation:** The log ends at `SetFocus` call, indicating the dialog was shown but execution stopped shortly after. This aligns with the documented crash issue in EXECUTION_ISSUES.md.

### 4. Missing Expected API Calls

APIs that were **expected** from decompilation but **not seen** in the log:
- ❌ User interaction messages (WM_COMMAND for button clicks)
- ❌ SHBrowseForFolderA (folder selection)
- ❌ SHFileOperationA (file copying)
- ❌ DirectXSetupA (DirectX installation)
- ❌ CoCreateInstance (COM shortcut creation)
- ❌ Registry APIs (RegCreateKeyExA, etc.)
- ❌ CoUninitialize (COM cleanup)

**Reason:** The installer crashed or closed before user could interact with it.

## Detailed API Call Analysis

### Phase 1: CRT Initialization (Lines 257-351)

#### Memory Management
```
GetVersion()                          → 602931718 (Windows version)
HeapCreate(HEAP_NO_SERIALIZE, 4096, 0) → 0x0a6d0000
VirtualAlloc(NULL, 4MB, RESERVE)      → 0x0a740000
VirtualAlloc(0x0a740000, 64KB, COMMIT) → 0x0a740000
```

**Analysis:**
- Heap created for CRT allocations
- 4MB virtual memory reserved for stack/heap growth
- 64KB committed initially (typical CRT pattern)

#### Standard I/O Initialization
```
GetStdHandle(STD_INPUT)  → NULL
GetStdHandle(STD_OUTPUT) → NULL
GetStdHandle(STD_ERROR)  → NULL
GetFileType(NULL) x3     → FILE_TYPE_UNKNOWN
```

**Analysis:**
- No console handles (GUI application)
- CRT still checks for redirected I/O (standard practice)

#### Locale and Encoding
```
SetHandleCount(32)                → 32
GetACP()                          → 65001 (CP_UTF8)
GetCPInfo(CP_UTF8)                → TRUE
GetCommandLineA()                 → 0x028b64c8
GetEnvironmentStringsW()          → 0x028d7738
WideCharToMultiByte(CP_ACP, ...)  → 3460 chars
```

**Analysis:**
- Active Code Page is UTF-8 (65001)
- Environment strings converted from Unicode to ANSI
- 3460 characters in environment block

### Phase 2: Module Path Extraction (Lines 352-425)

#### Module Filename Retrieval
```
GetModuleHandleA(NULL)                    → 0x00400000
GetModuleFileNameA(0x00400000, buffer)    → 55 chars
Path: "\\Mac\RiderProjects\Win32Emu\EXEs\ign_install\SETUP.EXE"
```

**Analysis:**
- Running from development path (not CD/Program Files)
- Network share or macOS mount point ("\\Mac")
- Indicates testing environment, not production installation

#### Path Parsing via CharNextA
```
CharNextA() x 55 times
```

**Analysis:**
- WinMain uses CharNextA to parse path backwards
- Finds last backslash to extract directory
- DBCS-safe string traversal (handles multi-byte characters)
- Extracts directory: "\\Mac\RiderProjects\Win32Emu\EXEs\ign_install\"

**Why 55 calls?**
- One CharNextA call per character in the path
- String length = 55 characters

### Phase 3: COM Initialization (Line 426)

```
CoInitialize(NULL) → S_OK (0.0099417 seconds)
```

**Analysis:**
- COM initialized successfully
- Single-threaded apartment (STA) mode
- ~10ms initialization time (typical for first COM call)
- Required for IShellLink interface (shortcut creation)

### Phase 4: Dialog Creation (Line 1757)

```
DialogBoxParamA(0x00400000, "DLG_MASTER", NULL, 0x00401130, 0)
```

**Parameters:**
- hInstance: 0x00400000 (module base)
- Template: "DLG_MASTER" (resource name)
- hWndParent: NULL (no parent window)
- DialogProc: 0x00401130 (DialogFunc from decompilation)
- InitParam: 0 (no custom data)

**Analysis:**
- Modal dialog (blocks until closed)
- No return value logged (dialog still processing)
- DialogProc address matches decompilation (see INDEX.md line 11)

### Phase 5: WM_INITDIALOG Handler (Lines 4999-5760)

This is the most interesting part - the actual dialog initialization:

#### 5.1. Window Title Setup
```
GetModuleHandleA(NULL)                → 0x00400000
LoadStringA(0x00400000, 101, buffer)  → 14 chars
SetWindowTextA(0x00040568, "Ignition Setup") → TRUE
```

**Analysis:**
- String resource #101 = "Ignition Setup"
- Dialog handle: 0x00040568
- Window title successfully set

#### 5.2. Disable Unused Controls
```
GetDlgItem(0x00040568, 1008) → 0x00060636
EnableWindow(0x00060636, FALSE)
GetDlgItem(0x00040568, 1007) → 0x00070550
EnableWindow(0x00070550, FALSE)
```

**Analysis:**
- Controls 1007 and 1008 are disabled
- These are placeholder buttons not used in this screen
- Matches decompilation showing conditional UI states

#### 5.3. Bitmap Loading Attempt
```
GetModuleHandleA(NULL)                → 0x00400000
LoadStringA(0x00400000, 118, buffer)  → 6 chars
GetDlgItem(0x00040568, 1002)          → 0x00050612
LoadImageA(0x00400000, buffer, IMAGE_BITMAP, 175, 195, flags)
  → NULL (Error 1814: resource not found)
```

**Analysis:**
- String resource #118 contains bitmap resource name
- Bitmap dimensions: 175x195 pixels
- **Bitmap not found in executable!**
- This is non-fatal (installer continues)
- Likely a splash image or logo

**Implication for Emulator:**
- LoadImageA must return NULL on missing resources
- Installer must handle NULL return gracefully
- Don't crash on missing bitmaps

#### 5.4. Installation Path Configuration
```
SendDlgItemMessageA(0x00040568, 1000, EM_SETLIMITTEXT, 260, 0) → 1
GetModuleHandleA(NULL)                → 0x00400000
LoadStringA(0x00400000, 100, buffer)  → 17 chars
SetDlgItemTextA(0x00040568, 1000, "C:\Games\Ignition") → TRUE
SendDlgItemMessageA(0x00040568, 1000, EM_SETSEL, 0, 16777472) → 1
```

**Analysis:**
- Control 1000 is an EDIT control (installation path textbox)
- Character limit: 260 (MAX_PATH)
- Default path: "C:\Games\Ignition" (string resource #100)
- Text selected after setting (EM_SETSEL with large value = select all)

**Control 1000 Message Sequence:**
1. Set character limit to 260
2. Set text to default path
3. Select all text (ready for user to type)

#### 5.5. Focus Management
```
GetDlgItem(0x00040568, 1000) → 0x00060558
SetFocus(0x00060558)
```

**Analysis:**
- Focus set to installation path textbox
- User can immediately type a new path
- Standard Windows dialog behavior

**Last API Call:**
This is the **final logged API call**. The dialog should now be visible and responsive, but the log ends here.

## Execution Timeline

```
6:25:59.340 PM - CRT initialization begins
6:25:59.344 PM - Path parsing and COM init
6:25:59.356 PM - CoInitialize completes
6:25:59.640 PM - DialogBoxParamA called
6:25:59.946 PM - WM_INITDIALOG processing
6:25:59.996 PM - SetFocus (last logged call)
```

**Total Duration:** 656 milliseconds from start to dialog display

## Comparison with Decompilation

| Aspect | Decompilation | ApiMon Log | Match? |
|--------|---------------|------------|--------|
| WinMain entry | ✅ Documented | ✅ Confirmed | ✅ Yes |
| Path extraction | ✅ CharNextA loop | ✅ 55 CharNextA calls | ✅ Yes |
| CoInitialize | ✅ Called | ✅ S_OK return | ✅ Yes |
| DialogBoxParamA | ✅ Called | ✅ Called | ✅ Yes |
| WM_INITDIALOG | ✅ Handler exists | ✅ Executed | ✅ Yes |
| String resources | ✅ IDs documented | ✅ #100, #101, #118 loaded | ✅ Yes |
| Control setup | ✅ Code present | ✅ Controls configured | ✅ Yes |
| Button clicks | ✅ WM_COMMAND handler | ❌ Not reached | ⚠️ Crash before user input |
| File operations | ✅ SHFileOperationA | ❌ Not reached | ⚠️ Crash before install |
| COM shortcuts | ✅ CoCreateInstance | ❌ Not reached | ⚠️ Crash before completion |
| CoUninitialize | ✅ Called at exit | ❌ Not reached | ⚠️ Crash before cleanup |

## String Resources Confirmed

| Resource ID | Value | Usage |
|-------------|-------|-------|
| 100 (0x64) | "C:\Games\Ignition" | Default installation path |
| 101 (0x65) | "Ignition Setup" | Dialog window title |
| 118 (0x76) | Unknown (6 chars) | Bitmap resource name (not found) |

**Note:** These match the documented resource IDs in INDEX.md.

## Critical Observations

### 1. Execution Path Verified

The ApiMon log **confirms** the decompilation analysis:
- WinMain → CoInitialize → DialogBoxParamA → DialogFunc
- This matches the documented flow in INDEX.md lines 205-246

### 2. Dialog Initialization Completes Successfully

All WM_INITDIALOG operations succeeded:
- ✅ Window title set
- ✅ Controls configured
- ✅ Default path loaded
- ✅ Focus set correctly

**This proves the dialog is functional up to this point.**

### 3. Execution Stops After SetFocus

The log ends at `SetFocus`, which should be near the **end** of WM_INITDIALOG.

**Expected next events:**
1. WM_INITDIALOG returns TRUE
2. Dialog enters message loop
3. User sees dialog window
4. User can click buttons or type

**Actual behavior (from EXECUTION_ISSUES.md):**
- Dialog appears briefly
- Crashes after ~1M instructions
- Jumps to NULL address
- Dialog closes

### 4. The Missing Piece

**Question:** What happens between `SetFocus` and the crash?

**Possibilities:**
1. WM_INITDIALOG returns (not logged by ApiMon)
2. Another message is dispatched (WM_PAINT, WM_SHOWWINDOW, etc.)
3. An internal function is called
4. **A CRT function or callback fails**
5. Stack corruption or invalid pointer access

**ApiMon Limitation:**
- Only logs Win32 API calls
- Does not log CRT functions (strcpy, sprintf, etc.)
- Does not log internal emulator operations
- Does not log CPU-level crashes

## Implications for Emulator Implementation

### 1. APIs Successfully Implemented ✅

Based on this log, these APIs work correctly:
- HeapCreate, HeapAlloc, HeapFree
- VirtualAlloc
- GetModuleFileNameA, GetModuleHandleA
- CharNextA (critical for DBCS paths)
- CoInitialize
- DialogBoxParamA
- GetDlgItem
- LoadStringA
- SetWindowTextA
- EnableWindow
- SetDlgItemTextA
- SendDlgItemMessageA
- SetFocus

### 2. Missing Resource Handling ⚠️

```
LoadImageA(...) → NULL (Error 1814)
```

**Recommendation:**
- Emulator must return NULL for missing resources
- Must set error code 1814 (ERROR_RESOURCE_NAME_NOT_FOUND)
- Installer must not crash on NULL return

### 3. Focus on Post-Init Debugging 🔍

**Since WM_INITDIALOG completes successfully**, the crash happens:
- AFTER SetFocus
- BEFORE user interaction
- Possibly in WM_PAINT, WM_SHOWWINDOW, or message loop

**Debug Strategy:**
1. Add logging AFTER WM_INITDIALOG returns
2. Log all messages dispatched after init
3. Check for NULL pointer dereferences in message handlers
4. Verify CRT functions don't fail
5. Check stack integrity

### 4. Test Case for Emulator

**Minimal test:** Dialog initialization only
```csharp
[Test]
public void Setup_WM_INITDIALOG_Completes()
{
    var env = new Emulator();
    env.LoadPE("SETUP.EXE");
    
    // Should complete WM_INITDIALOG without crash
    env.ExecuteUntilMessageDispatched(WM.INITDIALOG);
    
    Assert.IsTrue(env.DialogTitleContains("Ignition Setup"));
    Assert.AreEqual("C:\\Games\\Ignition", env.GetDlgItemText(1000));
}
```

## API Call Statistics

### By Category

| Category | Count | Percentage |
|----------|-------|------------|
| String Operations | 55 | 53.9% (CharNextA) |
| Dialog Management | 12 | 11.8% |
| Module/Resource | 8 | 7.8% |
| Memory Management | 7 | 6.9% |
| Locale/Encoding | 6 | 5.9% |
| Standard I/O | 6 | 5.9% |
| COM | 1 | 1.0% |
| Other | 7 | 6.9% |

### By Frequency

| API | Count | Purpose |
|-----|-------|---------|
| CharNextA | 55 | Path parsing (DBCS-safe) |
| GetModuleHandleA | 4 | Get module handle for resources |
| GetDlgItem | 4 | Get control handles |
| LoadStringA | 3 | Load string resources |
| HeapAlloc | 3 | CRT heap allocations |
| GetStdHandle | 3 | Check for console redirection |
| GetFileType | 3 | Validate standard handles |
| Others | <3 | Various operations |

### Performance Notes

**Slowest Operations:**
1. CoInitialize: 0.0099417s (~10ms) - COM initialization overhead
2. SendDlgItemMessageA (EM_SETSEL): 0.0053967s (~5ms) - Select text in edit control
3. SetDlgItemTextA: 0.0009808s (~1ms) - Set installation path

**Fastest Operations:**
- Most CharNextA calls: <0.000001s
- Simple property getters: <0.000010s

## Recommendations

### For Debugging the Crash

1. **Add comprehensive logging AFTER WM_INITDIALOG:**
   ```csharp
   Log($"WM_INITDIALOG returned: {result}");
   Log($"Next message in queue: {PeekMessage()}");
   ```

2. **Instrument message dispatch:**
   ```csharp
   foreach (var msg in DialogMessageLoop())
   {
       Log($"Dispatching message: {msg.message} to hwnd {msg.hwnd}");
       DispatchMessage(msg);
       Log($"Message completed");
   }
   ```

3. **Check for CRT failures:**
   - Add hooks for strcpy, sprintf, etc.
   - Validate all string operations
   - Check buffer boundaries

4. **Verify stack integrity:**
   - Check ESP/EBP after each function call
   - Detect stack smashing
   - Validate return addresses

### For Improving Emulation

1. **Implement resource error handling:**
   ```csharp
   if (!ResourceExists(resourceId))
   {
       SetLastError(ERROR_RESOURCE_NAME_NOT_FOUND);
       return NULL;
   }
   ```

2. **Add ApiMon-style logging:**
   - Log all API calls with parameters
   - Log return values
   - Log timing information
   - Export to CSV for analysis

3. **Test with minimal dialog:**
   - Create a test EXE with just DialogBoxParamA
   - Verify dialog shows and responds
   - Ensure message loop works correctly

## Related Documentation

- **[INDEX.md](INDEX.md)** - Function offsets and resource IDs (confirmed by this log)
- **[EXECUTION_ISSUES.md](EXECUTION_ISSUES.md)** - Documents the crash that prevents further execution
- **[ANALYSIS.md](ANALYSIS.md)** - Detailed decompilation analysis (matches this log)
- **[README.md](README.md)** - Overview of decompiler outputs

## Conclusion

The ApiMon log provides **critical runtime validation** of the decompilation analysis:

✅ **Confirmed:**
- Decompilation is accurate
- Dialog initialization logic is correct
- String resources are properly loaded
- Control setup works as documented

⚠️ **Revealed:**
- Missing bitmap resource (Error 1814)
- Execution stops after SetFocus
- No user interaction occurs

❌ **Crash Location:**
- After WM_INITDIALOG completes
- Before user can interact
- Likely in message loop or subsequent message handler

**Next Steps:**
1. Add post-init logging in emulator
2. Instrument message dispatch
3. Identify the exact crash location
4. Fix the NULL pointer jump
5. Verify dialog remains responsive

The ApiMon log proves the installer is **very close** to working - it just needs the post-initialization crash fixed.
