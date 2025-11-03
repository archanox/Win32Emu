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

### Issue 1: Redundant EAX Setting (Low Priority)

**Location:** Win32Dispatcher.TryInvoke() sets EAX, then HandleSyscall also sets EAX

**Impact:** Minimal - the second write simply overwrites the first

**Recommendation:** Remove EAX manipulation from Win32Dispatcher for clarity

### Issue 2: Inconsistent EBP Validation (Medium Priority)

**Location:** Different code paths use different EBP validation strategies

**Details:**
- COM vtable calls use `skipInvalidEbp: true`
- Import hook calls use `skipInvalidEbp: true`
- HandleSyscall does NOT use `skipInvalidEbp`

**Recommendation:** Standardize on using `skipInvalidEbp: true` for all paths

### Issue 3: Multiple Register Manipulation Code Paths (Medium Priority)

**Location:** Emulator.cs has 6+ different code paths for handling different call types

**Details:** Each path has slightly different register handling logic:
1. COM vtable calls (lines 658-677)
2. Import hook calls (lines 692-714)
3. Syscall dispatcher (lines 1400-1559)
4. Synthetic export calls (lines 917-1000)
5. And several more...

**Impact:** Error-prone, difficult to maintain, easy to introduce bugs

**Recommendation:** Refactor into a common helper function that handles register save/restore consistently

## Summary

Overall, register manipulation follows x86 calling conventions correctly. The main areas for improvement are:

1. **Code consolidation:** Reduce duplication across different call handling paths
2. **Consistency:** Use consistent EBP validation across all paths
3. **Clarity:** Remove redundant operations (like duplicate EAX setting)
4. **Testing:** Add explicit tests for register preservation across syscalls

## Testing Recommendations

1. Test that callee-saved registers (EBX, ESI, EDI, EBP) are preserved across Win32 API calls
2. Test that EAX receives the correct return value
3. Test that ESP is correctly adjusted after stdcall functions
4. Test edge cases: EBP used as function pointer, unaligned EBP, etc.
5. Test all different call paths: COM, import hooks, syscall dispatcher, synthetic exports

## Next Steps

1. Add validation logging for register state before/after each call type
2. Implement comprehensive register preservation tests
3. Consider refactoring to reduce code duplication
4. Add runtime assertions for register invariants
