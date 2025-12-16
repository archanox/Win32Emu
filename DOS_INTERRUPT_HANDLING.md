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

| AH   | Function Name                  | Implementation Status                        |
|------|-------------------------------|----------------------------------------------|
| 0x00 | Terminate Program              | Sets `_stopRequested` flag                   |
| 0x01 | Read Character from STDIN      | Returns dummy character (space)              |
| 0x02 | Write Character to STDOUT      | Writes to StdOut via `ProcessEnvironment`    |
| 0x06 | Direct Console I/O             | Handles input/output, writes to StdOut       |
| 0x07 | Direct Char Input (no echo)    | Returns dummy character (space)              |
| 0x08 | Char Input (no echo)           | Returns dummy character (space)              |
| 0x09 | Write String to STDOUT         | Writes '$'-terminated string to StdOut       |
| 0x0A | Buffered Keyboard Input        | Stub - returns empty input                   |
| 0x0B | Check Keyboard Status          | Returns no input available                   |
| 0x19 | Get Current Drive              | Returns C: drive (0x02)                      |
| 0x25 | Set Interrupt Vector           | Acknowledged but not implemented             |
| 0x2A | Get System Date                | Returns real system date                     |
| 0x2B | Set System Date                | Stub - accepts but doesn't set              |
| 0x2C | Get System Time                | Returns real system time                     |
| 0x2D | Set System Time                | Stub - accepts but doesn't set              |
| 0x30 | Get DOS Version                | Returns version 6.22                         |
| 0x33 | Get/Set Ctrl-Break             | Get returns enabled, Set is acknowledged     |
| 0x35 | Get Interrupt Vector           | Returns dummy address                        |
| 0x3C | Create File                    | Returns dummy file handle (0x0005)           |
| 0x3D | Open File                      | Returns dummy file handle (0x0005)           |
| 0x3E | Close File                     | Acknowledged (no-op)                         |
| 0x3F | Read from File                 | Returns 0 bytes (EOF)                        |
| 0x40 | Write to File/Device           | Writes to StdOut for handles 1-2             |
| 0x42 | Move File Pointer (Lseek)      | Returns requested offset                     |
| 0x43 | Get/Set File Attributes        | Get returns archive bit, Set acknowledged   |
| 0x47 | Get Current Directory          | Returns current directory from ProcessEnv    |
| 0x48 | Allocate Memory Block          | Allocates memory via `SimpleAlloc`           |
| 0x49 | Free Memory Block              | Acknowledged (no-op)                         |
| 0x4A | Resize Memory Block            | Acknowledged (no-op)                         |
| 0x4C | Terminate with Return Code     | Sets `_stopRequested` flag, logs exit code   |
| 0x4D | Get Return Code                | Returns 0                                    |
| Other | Unimplemented functions       | Returns error value (0xFFFFFFFF in EAX)      |

**Design Notes:**

- Uses flat memory model (no segment register operations)
- Reads strings using DX as a direct pointer
- **Console output routed to ProcessEnvironment.WriteToStdOutput()** - Output is sent to the host/UI, not just logged
- File operations return dummy handles or success values (actual I/O not implemented)
- Memory allocation uses existing SimpleAlloc mechanism
- Date/time functions return real system values
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

1. **File I/O Stubbed**: File operations return dummy handles or success values but don't perform actual I/O
   - File handles are dummy values (0x0005)
   - Read operations return EOF
   - Write operations return success but don't write to actual files
   - Consider integrating with VFS for real file operations

2. **No Real-Mode Segmentation**: Assumes flat memory model
   - Segment registers (DS, ES) not accessible through ICpu interface
   - Works for protected mode Win16 applications
   - Real-mode DOS programs may have issues

3. **No Interrupt Vector Table**: Interrupt vectors not actually stored or used
   - Get/Set interrupt vector operations are acknowledged but don't maintain a table
   - Programs relying on actual interrupt hooking won't work correctly

4. **Input Operations Return Dummy Data**: Character input functions return space character
   - No actual keyboard input handling
   - Programs requiring interactive input won't work properly

5. **Memory Management Simplified**: Memory allocation uses SimpleAlloc
   - Free and resize operations are acknowledged but don't actually free memory
   - May lead to memory exhaustion for programs that allocate/free repeatedly

### Improvements Made

- ✅ **Console Output Routed to Host/UI**: Output now uses `ProcessEnvironment.WriteToStdOutput()` which calls `_host?.OnStdOutput(text)`
- ✅ **File Operations Recognized**: Create, open, close, read, write, seek operations implemented (stubbed)
- ✅ **Date/Time Functions**: Return real system date and time values
- ✅ **Memory Allocation**: Uses existing memory allocator
- ✅ **Comprehensive Function Coverage**: 30+ DOS functions implemented

### Future Enhancements

To support more complex DOS applications, consider implementing:

1. **Real File I/O**: Integrate with VirtualFileSystem
   - Map DOS file handles to VFS files
   - Implement actual read/write operations
   - Support file seeking and attribute management

2. **Interactive Input**: Implement actual keyboard input
   - Buffer keyboard events
   - Return real characters from input functions
   - Support buffered input (AH=0x0A)

3. **Process Management**:
   - AH=0x4B: Load and execute program
   - AH=0x4D: Get return code (currently returns 0)
   - Support for spawning child processes

4. **Directory Operations**:
   - AH=0x39: Create directory
   - AH=0x3A: Remove directory
   - AH=0x3B: Change directory (integrate with ProcessEnvironment.CurrentDirectory)

5. **Proper Memory Management**:
   - Implement actual memory free and resize
   - Track allocated blocks properly
   - Prevent memory leaks from repeated allocations

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
