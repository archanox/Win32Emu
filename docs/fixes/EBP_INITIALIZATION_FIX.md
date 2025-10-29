# Fix for Address Wraparound Issue (0xFFFFFFF5)

## Problem

When running `winapi.exe`, the emulator crashed with the following error:

```
fail: Win32Emu.Emulator[0]
      Calculated memory address out of range: 0xFFFFFFF5 (EIP=0x00401002) 
      size=0x10000000; ESP=0x00200000 EBP=0x00000000 EAX=0x00000002
fail: Win32Emu.Emulator[0]
      Instruction bytes at EIP: FF 15 58 20 40 00 31 C9
fail: Win32Emu.Emulator[0]
      Emulator error: Calculated memory address out of range: 0xFFFFFFF5 (EIP=0x00401002)
      System.IndexOutOfRangeException: Calculated memory address out of range: 0xFFFFFFF5 (EIP=0x00401002)
```

The error occurred after `WriteFile` successfully executed (the "hello" message appeared in the UI), 
when the program tried to execute an ADD instruction with a frame-pointer-relative memory operand.

## Root Cause

The address `0xFFFFFFF5` is the result of unsigned integer wraparound when calculating a memory 
address with:
- Base register (EBP) = 0x00000000 (uninitialized)
- Displacement = -11 (0xFFFFFFF5 in two's complement)

The calculation: `0x00000000 + 0xFFFFFFF5 = 0xFFFFFFF5` produces a very large positive number
(4,294,967,285 decimal) which exceeds the emulator's memory size.

The root cause was that the emulator did not initialize the EBP (frame pointer) register when
loading executables. In `Emulator.cs`, line 93 set ESP but not EBP, leaving EBP at its default
value of 0.

## Solution

Initialize EBP to match ESP when loading an executable:

```csharp
_cpu = new IcedCpu(_vm, _logger);
_cpu.SetEip(_image.EntryPointAddress);
_cpu.SetRegister("ESP", 0x00200000);
_cpu.SetRegister("EBP", 0x00200000); // Initialize frame pointer to match stack pointer
```

### Rationale

1. **Prevents wraparound**: With EBP initialized to a valid stack address (0x00200000), 
   frame-relative accesses like `[EBP-11]` will calculate valid addresses within the stack 
   region instead of wrapping around to huge positive values.

2. **Standard practice**: In x86 architecture, the frame pointer typically points to the base
   of the current stack frame. Before the first function sets up its frame, having EBP equal
   to ESP is a reasonable default.

3. **Defensive programming**: Even if user code properly sets up stack frames (via `PUSH EBP; 
   MOV EBP, ESP`), initializing EBP prevents crashes if code attempts frame-relative accesses
   before stack frame setup.

4. **Consistent with stack semantics**: The address 0x00200000 is a valid stack location, so
   any accesses via `[EBP±offset]` will target the stack region (which has allocated memory).

## Testing

Added `EbpInitializationTests.cs` with two tests:

1. **IcedCpu_WithInitializedEBP_ShouldAllowFrameRelativeAccess**: Verifies that with EBP 
   initialized to ESP, frame-relative instructions execute successfully.

2. **IcedCpu_WithUninitializedEBP_ShouldThrowOnFrameRelativeAccess**: Verifies that with 
   EBP=0, the same instructions throw `IndexOutOfRangeException` with address `0xFFFFFFF5`.

Both tests pass, confirming the fix resolves the issue.

## Impact

- **Minimal change**: Only one line of code added
- **No breaking changes**: All existing tests continue to pass (64/64 in Win32Emu.Tests.Emulator,
  11/11 in CpuMemoryAccessTests, plus 2 new tests)
- **Pre-existing test failures**: Some tests in `ModuleProcessTests` were already failing due to
  unrelated issues (AccessViolationException in GetModuleHandleA), which are not affected by
  this change
