# DOS Interrupt Handling Implementation

## Overview

This document describes the implementation of DOS interrupt (INT 21h) handling in Win32Emu to support Win16 NE executables that use DOS services.

## Problem Statement

Win16 NE executables like `mine.exe` use DOS interrupts (specifically INT 21h) to access system services. Previously, the emulator would:

1. Log a warning when encountering an unhandled interrupt
2. **Not advance the instruction pointer (EIP)**
3. Cause an infinite loop by re-executing the same INT instruction

This made it impossible to run certain Win16 applications that rely on DOS services.

## Solution

### 1. CPU-Level Changes

#### IcedCpu and JitCpu Interrupt Handling

Both CPU emulators now:

- **Detect INT 21h** (DOS services interrupt)
- **Advance EIP** past the INT instruction to prevent infinite loops
- **Signal DOS interrupt** via the `IsDosInterrupt` flag in `CpuStepResult`
- **Log the DOS function number** (AH register) for debugging

For all other unhandled interrupts, EIP is now also advanced to prevent infinite loops.

**Changes:**
- Added `isDosInterrupt` flag to CPU step logic
- Modified INT instruction handling to detect 0x21 (DOS services)
- Ensured EIP advances for all interrupt types

#### CpuStepResult Extension

Added `IsDosInterrupt` parameter to the `CpuStepResult` record struct:

```csharp
public readonly record struct CpuStepResult(
    bool IsCall, 
    uint CallTarget, 
    bool IsSyscall = false, 
    bool IsDosInterrupt = false
);
```

### 2. Emulator-Level Changes

#### DOS Interrupt Handler

Added `HandleDosInterruptAsync` method in the `Emulator` class to handle DOS INT 21h services:

**Supported DOS Functions:**

| AH | Function | Implementation |
|----|----------|----------------|
| 0x00 | Terminate program | Sets `_stopRequested` flag |
| 0x4C | Terminate with return code | Sets `_stopRequested` flag, logs exit code |
| 0x09 | Write string to stdout | Reads '$'-terminated string and logs it |
| 0x02 | Write character to stdout | Logs single character |
| 0x25 | Set interrupt vector | Acknowledged but not implemented |
| 0x35 | Get interrupt vector | Returns dummy address |
| Other | Unimplemented functions | Returns error value (0xFFFFFFFF in EAX) |

**Design Notes:**

- Uses flat memory model (no segment register operations)
- Reads strings using DX as a direct pointer
- Logs output instead of writing to actual console
- Returns error values for unimplemented functions

#### Main Execution Loop Integration

Both async and sync execution loops now check for DOS interrupts:

```csharp
// Check for DOS interrupt (INT 21h from Win16 NE executables)
if (step.IsDosInterrupt)
{
    await HandleDosInterruptAsync().ConfigureAwait(false);
    continue; // Continue to next iteration
}
```

## Usage

### For Win16 Applications

Win16 NE executables can now:

1. **Terminate gracefully** using INT 21h, AH=0x00 or AH=0x4C
2. **Print output** using INT 21h, AH=0x09 (strings) or AH=0x02 (characters)
3. **Continue execution** past DOS service calls without hanging

### Logging

DOS interrupt calls are logged at different levels:

- **Debug level**: Function calls (AH parameter values)
- **Info level**: String/character output, program termination
- **Warning level**: Unimplemented function calls

Example log output:
```
[DBG] [IcedCpu] INT 0x21 DOS services at 0x00039E14, AH=0x09
[INF] [DOS INT 21h] Print string: Hello, World!
[INF] [DOS INT 21h] Program termination with exit code 0 (AH=0x4C)
```

## Limitations

### Current Limitations

1. **Limited Function Support**: Only basic DOS functions are implemented
2. **No File I/O**: File operations (open, read, write, close) not yet supported
3. **No Real-Mode Segmentation**: Assumes flat memory model
4. **No Interrupt Vector Table**: Interrupt vectors not actually stored or used
5. **Logging Only**: Console output is logged, not written to actual console

### Future Enhancements

To support more complex DOS applications, consider implementing:

1. **File I/O Functions**:
   - AH=0x3C: Create file
   - AH=0x3D: Open file
   - AH=0x3E: Close file
   - AH=0x3F: Read from file
   - AH=0x40: Write to file

2. **Memory Management**:
   - AH=0x48: Allocate memory
   - AH=0x49: Free memory
   - AH=0x4A: Resize memory block

3. **Directory Operations**:
   - AH=0x39: Create directory
   - AH=0x3A: Remove directory
   - AH=0x3B: Change directory
   - AH=0x47: Get current directory

4. **Date/Time Functions**:
   - AH=0x2A: Get system date
   - AH=0x2B: Set system date
   - AH=0x2C: Get system time
   - AH=0x2D: Set system time

5. **Process Management**:
   - AH=0x4B: Load and execute program
   - AH=0x4D: Get return code

## Testing

### Regression Tests

All existing emulator tests pass with these changes:
- CPU instruction conformance tests: Pass (no new failures)
- Win16 thunking tests: Pass
- Core emulator tests: Pass

### Manual Testing

To test DOS interrupt handling:

1. Run a Win16 application that uses DOS services
2. Check logs for DOS interrupt calls
3. Verify application doesn't hang in infinite loop
4. Confirm proper program termination

Example:
```bash
dotnet run --project Win32Emu -- path/to/mine.exe --debug
```

## References

### DOS INT 21h Documentation

- [DOS Interrupt 21h Function Reference](http://www.ctyme.com/intr/int-21.htm)
- [Ralf Brown's Interrupt List](http://www.ctyme.com/rbrown.htm)
- Win16 Programming Guide (Microsoft MSDN Archives)

### Related Implementation Documents

- `docs/implementation/WIN16_THUNKING_IMPLEMENTATION.md` - Win16 API thunking
- `docs/implementation/NE_LOADER_IMPLEMENTATION.md` - NE executable loader
- `README.md` - Main project documentation

## Contributing

To add support for additional DOS functions:

1. Add case to switch statement in `HandleDosInterruptAsync`
2. Implement function behavior using available CPU/memory APIs
3. Add appropriate logging
4. Test with real Win16 applications
5. Update this documentation with new function support

Example:

```csharp
case 0x3D: // Open file
    var filename = ReadStringFromMemory(dx);
    _logger.LogDebug("[DOS INT 21h] Open file: {Filename}", filename);
    // TODO: Implement file opening via VFS
    // Return file handle in AX
    _cpu.SetRegister("EAX", fileHandle);
    break;
```
