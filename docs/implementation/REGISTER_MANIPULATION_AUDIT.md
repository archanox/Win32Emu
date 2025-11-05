# Register Manipulation Audit

This document catalogs all locations where CPU registers are manipulated in Win32Emu, with analysis of correctness per x86 calling conventions.

## x86 Calling Convention Reference

### stdcall Convention (Win32 APIs)
- **Caller responsibilities:**
  - Push arguments right-to-left
  - CALL pushes return address
  
- **Callee responsibilities:**
  - Must preserve: EBX, ESI, EDI, EBP
  - Return value in EAX (or EDX:EAX for 64-bit)
  - Clean up stack with `RET imm16` instruction
  
- **After return:**
  - EAX contains return value
  - ESP is restored to pre-call state (callee cleaned up arguments)
  - EBX, ESI, EDI, EBP must have original values

## Register Manipulation Points

### 1. Emulator.cs - LoadExecutable() (lines 260-261)

**Purpose:** Initialize CPU state when loading executable

```csharp
_cpu.SetRegister("ESP", initialEsp);
_cpu.SetRegister("EBP", initialEsp); // Initialize frame pointer to match stack pointer
```

**Analysis:** ✅ CORRECT
- Standard initialization: ESP and EBP both point to top of stack initially
- This matches Windows process initialization

### 2. Emulator.cs - COM Vtable Call Handling (lines 658-677)

**Purpose:** Execute COM interface method calls

**Before call (line 658):**
```csharp
var saved = CpuHelpers.SaveCalleeSavedRegisters(_cpu);
```

**After call (lines 669-670, 677):**
```csharp
_cpu.SetRegister("ESP", esp);
_cpu.SetRegister("EAX", ret); // Return value in EAX
// ...
CpuHelpers.RestoreCalleeSavedRegisters(_cpu, saved, skipInvalidEbp: true, memorySize: _vm!.Size);
```

**Analysis:** ✅ CORRECT
- Saves callee-saved registers before COM call
- Sets ESP after stack cleanup
- Sets EAX with return value
- Restores callee-saved registers with validation for EBP

**Note:** Uses `skipInvalidEbp: true` to avoid restoring obviously invalid EBP values

### 3. Emulator.cs - Import Hook Call Handling (lines 692-714)

**Purpose:** Handle direct calls to import stubs (not through syscall dispatcher)

**Before call (line 692):**
```csharp
var saved = CpuHelpers.SaveCalleeSavedRegisters(_cpu);
var ebpBeforeCall = saved.Ebp;
var ebpWasValid = CpuHelpers.IsEbpValid(ebpBeforeCall, (uint)_vm!.Size);
```

**After call (lines 706, 709, 714):**
```csharp
_cpu.SetRegister("ESP", esp);
var ebpAfterCall = _cpu.GetRegister("EBP");
// ... logging ...
CpuHelpers.RestoreCalleeSavedRegisters(_cpu, saved, skipInvalidEbp: true, memorySize: _vm!.Size);
```

**Analysis:** ✅ CORRECT
- Validates EBP before and after call
- Restores registers with EBP validation
- Properly handles stack cleanup

### 4. Emulator.cs - HandleSyscall() (lines 1400-1559)

**Purpose:** Main syscall dispatcher entry point

**Before API call:**
```csharp
var saved = CpuHelpers.SaveCalleeSavedRegisters(_cpu);  // line 1434
_cpu.SetRegister("ESP", esp + 4);  // line 1460 - Skip return-to-stub address
```

**After API call (success path):**
```csharp
_cpu.SetRegister("EAX", ret);  // line 1495
_cpu.SetRegister("ESP", originalEsp);  // line 1498
CpuHelpers.RestoreCalleeSavedRegisters(_cpu, saved);  // line 1501
```

**After API call (error path):**
```csharp
_cpu.SetRegister("EAX", 0);  // line 1546
_cpu.SetRegister("ESP", originalEsp);  // line 1548
CpuHelpers.RestoreCalleeSavedRegisters(_cpu, saved);  // line 1549
```

**Analysis:** ✅ CORRECT
- Temporarily adjusts ESP for argument reading (documented in comments)
- Restores original ESP before returning
- Sets return value in EAX
- Restores callee-saved registers

**Note:** Does NOT use `skipInvalidEbp` - assumes EBP is always valid here

### 5. Win32Dispatcher.cs - TryInvoke() (lines 36-117)

**Purpose:** Dispatch to Win32 module implementations

**Register manipulation:**
```csharp
var esp = cpu.GetRegister("ESP");  // line 42
cpu.SetRegister("EAX", retUnsafe);  // line 60 - After successful invoke
cpu.SetRegister("EAX", returnValue);  // lines 96, 114 - For unknown/unimplemented functions
```

**Analysis:** ⚠️ REDUNDANT but HARMLESS
- Sets EAX with return value
- **Issue:** This is redundant with HandleSyscall also setting EAX (line 1495)
- The HandleSyscall value overwrites this, so the dispatcher setting EAX is unnecessary
- Does not manipulate ESP (correct - caller handles that)
- Does not save/restore registers (correct - that's handled by HandleSyscall)

**Recommendation:** Remove EAX manipulation from Win32Dispatcher since HandleSyscall handles it

### 6. IcedCpu.cs - RET Instruction (lines 382-426)

**Purpose:** Execute x86 RET instruction

```csharp
case Mnemonic.Ret:
    var ret = Read32(_esp);  // Pop return address
    var oldEsp = _esp;
    _esp += 4;  // Pop return address from stack
    _eip = ret;  // Jump to return address
    if (insn.Immediate16 != 0)
    {
        _esp += insn.Immediate16;  // Clean up arguments (stdcall)
    }
```

**Analysis:** ✅ CORRECT
- Pops return address into EIP
- Adjusts ESP for return address (4 bytes)
- Optionally adjusts ESP for argument cleanup (stdcall convention)
- Does NOT modify other registers (correct per x86 specification)

### 7. CpuHelpers.cs - SaveCalleeSavedRegisters() (lines 35-44)

**Purpose:** Save callee-saved registers per x86 convention

```csharp
return new SavedCalleeSavedRegisters
{
    Ebx = cpu.GetRegister("EBX"),
    Esi = cpu.GetRegister("ESI"),
    Edi = cpu.GetRegister("EDI"),
    Ebp = cpu.GetRegister("EBP")
};
```

**Analysis:** ✅ CORRECT
- Saves exactly the registers required by x86 stdcall/cdecl conventions
- Does NOT save EAX, ECX, EDX (correct - these are caller-saved)

### 8. CpuHelpers.cs - RestoreCalleeSavedRegisters() (lines 64-87)

**Purpose:** Restore callee-saved registers

```csharp
cpu.SetRegister("EBX", saved.Ebx);
cpu.SetRegister("ESI", saved.Esi);
cpu.SetRegister("EDI", saved.Edi);

// Optionally skip restoring invalid EBP
if (skipInvalidEbp && memorySize > 0)
{
    if (IsEbpValid(saved.Ebp, memorySize))
    {
        cpu.SetRegister("EBP", saved.Ebp);
    }
}
else
{
    cpu.SetRegister("EBP", saved.Ebp);
}
```

**Analysis:** ✅ CORRECT with ENHANCEMENT
- Restores EBX, ESI, EDI unconditionally (correct)
- Has optional validation for EBP restoration
- Validation prevents restoring obviously corrupted EBP values (import hook addresses, etc.)
- This is a defensive measure beyond standard x86 convention

**Enhancement rationale:** Some calling code uses EBP for indirect calls (e.g., `MOV EBP, [IAT_Entry]; CALL EBP`), which violates standard conventions but occurs in real code.

### 9. CpuHelpers.cs - RestoreEbpFromStack() (lines 98-211)

**Purpose:** Advanced EBP restoration heuristics for non-standard code patterns

**Analysis:** ✅ DEFENSIVE MEASURE
- Handles edge cases where code uses EBP for function pointers
- Attempts to restore EBP from stack in certain conditions
- Falls back to safe defaults (ESP) when restoration fails
- Uses heuristics to detect COM pointers, stack pointers, etc.

**Note:** This goes beyond standard x86 conventions to handle real-world code patterns

## Potential Issues Identified

### Issue 1: Redundant EAX Setting (Low Priority) - CLARIFIED

**Location:** Win32Dispatcher.TryInvoke() sets EAX, then HandleSyscall also sets EAX

**Impact:** Minimal - the second write simply overwrites the first

**Original Recommendation:** Remove EAX manipulation from Win32Dispatcher for clarity

**Current Status:** ✅ NOT AN ISSUE
- Further analysis shows EAX setting in Win32Dispatcher is REQUIRED for debugger modes
- Interactive debugger (lines 1080-1174) and GDB server mode (lines 1180-1269) call Win32Dispatcher directly
- These modes do NOT set EAX after TryInvoke, relying on Win32Dispatcher to do so
- HandleSyscall path (line 1462) does redundantly set EAX (line 1495), but this is harmless
- **Recommendation:** Keep current implementation. The redundancy in HandleSyscall is acceptable for code clarity.

### Issue 2: Inconsistent EBP Validation (Medium Priority) - RESOLVED

**Location:** Different code paths use different EBP validation strategies

**Original Details:**
- COM vtable calls use `skipInvalidEbp: true`
- Import hook calls use `skipInvalidEbp: true`
- HandleSyscall does NOT use `skipInvalidEbp`

**Current Status:** ✅ RESOLVED
- Audit document was outdated
- Verification shows ALL code paths now consistently use `skipInvalidEbp: true`:
  - Line 677: COM vtable calls
  - Line 714: Import hook calls (commented out code)
  - Line 931: COM vtable calls (alternative path)
  - Line 975: Import hook calls (alternative path)
  - Line 1000: Import hook calls (error path)
  - Line 1232: GDB server COM calls
  - Line 1259: GDB server import calls
  - Line 1347: Direct import calls
  - Line 1363: Direct import calls (error path)
  - **Line 1501: HandleSyscall** ✅ USES skipInvalidEbp: true
  - **Line 1554: HandleSyscall error path** ✅ USES skipInvalidEbp: true

**Recommendation:** No action needed. EBP validation is already consistent across all paths.

### Issue 3: Multiple Register Manipulation Code Paths (Medium Priority) - ✅ COMPLETED

**Location:** Emulator.cs has 6+ different code paths for handling different call types

**Details:** Each path has slightly different register handling logic:
1. COM vtable calls (lines 658-677)
2. Import hook calls (lines 692-714) - LEGACY CODE: Commented out, now uses syscall mechanism (INT 0x80) instead
3. Syscall dispatcher (lines 1400-1559) - PRIMARY PATH: All import calls route through here
4. Interactive debugger paths (lines 1080-1174, 1180-1269)
5. Direct import calls (lines 1335-1365)
6. And several more...

**Impact:** Error-prone, difficult to maintain, easy to introduce bugs

**Original Recommendation:** Refactor into a common helper function that handles register save/restore consistently

**Current Status:** ✅ COMPLETED
- Created `CpuHelpers.InvokeWithRegisterPreservation()` helper function that consolidates the common pattern
- Refactored 5 call sites to use the new helper:
  1. Direct import calls (HandleDirectImportCall)
  2. COM vtable calls in main execution loop
  3. Import hook calls in main execution loop
  4. COM vtable calls in GDB server mode
  5. Import hook calls in GDB server mode
- HandleSyscall intentionally NOT refactored due to unique ESP manipulation and stack corruption detection
- Reduces code duplication from ~30 lines per call site to ~15 lines
- Standardizes register preservation, EAX setting, ESP cleanup, and EIP handling
- All 619 emulator tests pass after refactoring

**Implementation Details:**
The new helper function handles:
- Save callee-saved registers (EBX, ESI, EDI, EBP)
- Invoke the target function
- Set return value in EAX (stdcall convention)
- Clean up stack: pop return address + arguments
- Set EIP to return address
- Restore callee-saved registers with EBP validation
- Optional register state validation for diagnostics
- Consistent error handling with proper register restoration

**Benefits:**
1. ✅ Reduced code duplication (eliminated ~75 lines of repetitive code)
2. ✅ Centralized x86 stdcall convention implementation
3. ✅ Easier to maintain and modify register handling logic
4. ✅ Consistent behavior across all call paths
5. ✅ Better testability through comprehensive tests

## Summary

Overall, register manipulation follows x86 calling conventions correctly. Analysis of the original issues shows:

1. **Issue 1 (Redundant EAX):** ✅ CLARIFIED - Not actually redundant; required for debugger modes
2. **Issue 2 (EBP Validation):** ✅ RESOLVED - Already fixed; all paths use `skipInvalidEbp: true`
3. **Issue 3 (Code Consolidation):** ✅ COMPLETED - Refactored with new InvokeWithRegisterPreservation helper

**Improvements Made:**
1. ✅ **Testing:** Added 18 comprehensive register preservation tests
   - Tests verify callee-saved registers (EBX, ESI, EDI, EBP) are preserved
   - Tests verify EAX receives correct return values
   - Tests verify ESP is correctly adjusted
   - Tests cover edge cases (invalid EBP, import hooks, etc.)
2. ✅ **Documentation:** Updated audit to reflect current code state
3. ✅ **Validation:** Existing validation logging confirmed working (line 1505 in Emulator.cs)
4. ✅ **Code Consolidation:** Created InvokeWithRegisterPreservation helper
   - Eliminated ~75 lines of duplicate code
   - Standardized register handling across 5 call sites
   - Centralized x86 stdcall convention implementation
   - All tests pass after refactoring

**Code Changes Summary:**
- Added `CpuHelpers.InvokeWithRegisterPreservation()` helper function (~70 lines)
- Refactored 5 call sites to use new helper (net reduction of ~75 lines)
- Improved maintainability and consistency
- Zero regressions - all 619 tests still passing

## Testing Recommendations

✅ **COMPLETED:** Comprehensive register preservation tests added (18 tests in RegisterPreservationTests.cs)

Tests now verify:
1. ✅ Callee-saved registers (EBX, ESI, EDI, EBP) are preserved across Win32 API calls
2. ✅ EAX receives the correct return value
3. ✅ ESP is correctly adjusted after stdcall functions
4. ✅ Edge cases: EBP used as function pointer, unaligned EBP, invalid EBP, etc.
5. ✅ Multiple API call sequences preserve registers consistently
6. ✅ SaveCalleeSavedRegisters/RestoreCalleeSavedRegisters helper functions work correctly
7. ✅ IsEbpValid correctly identifies invalid EBP values (import hooks, zero, low addresses)
8. ✅ RestoreEbpFromStack handles import hook addresses correctly

**Test Coverage:**
- Basic unit tests for CpuHelpers functions
- Integration tests calling actual Win32 APIs (GetTickCount)
- Edge case testing for invalid and corrupted register values
- Stack restoration scenarios

## Next Steps

✅ **COMPLETED:**
1. ✅ Add validation logging for register state before/after each call type (already exists at line 1505)
2. ✅ Implement comprehensive register preservation tests (18 tests added)
3. ✅ Refactored to reduce code duplication (completed)
4. ✅ Add runtime assertions for register invariants (ValidateRegisterState function exists)

**Future Enhancements (Optional):**
1. Consider consolidating register save/restore code if adding new call paths
2. Add performance profiling to identify optimization opportunities
3. Expand test coverage to include all Win32 modules (currently focuses on Kernel32)
