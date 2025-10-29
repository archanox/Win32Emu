# API Call Sequence Comparison: ApiMon vs Emulator

## Purpose
Compare the actual API call sequence from a real Windows run (ApiMon logs) with what the emulator is calling to identify where the game diverges.

## Expected Sequence (from ApiMon logs)

The game calls these APIs in this order:

1. **GetVersion** - Check Windows version
2. **HeapCreate** - Create heap
3. **VirtualAlloc** (x2) - Reserve and commit memory
4. **GetStartupInfoA** - Get startup info
5. **GetStdHandle** (x3) - Get stdin, stdout, stderr
6. **GetFileType** (x3) - Check handle types
7. **SetHandleCount** - Set handle count
8. **GetACP** - Get ANSI code page ⚠️
9. **GetCPInfo** - Get code page info
10. **GetCommandLineA** - Get command line
11. **GetEnvironmentStringsW** - Get environment
12. **WideCharToMultiByte** (x2) - Convert strings
13. **HeapAlloc** - Allocate from heap
14. **FreeEnvironmentStringsW** - Free environment
15. **GetModuleFileNameA** - Get module path
16. **HeapAlloc**, **HeapFree** - Heap operations
17. **GetModuleHandleA** ("KERNEL32") - Get KERNEL32 handle
18. **GetProcAddress** ("IsProcessorFeaturePresent") - Get function address
19. **IsProcessorFeaturePresent** - Check processor feature
20. **HeapAlloc** - Allocate heap
21. **GetStartupInfoA** (2nd call) - Get startup info again
22. **GetModuleHandleA** (NULL) - Get current module
23. **LoadCursorA** - Load cursor ✅ EMULATOR REACHES HERE
24. **LoadIconA** - Load icon ❌ EMULATOR STOPS
25. **GetStockObject** - Get stock brush
26. **RegisterClassA** - Register window class
27. **timeBeginPeriod** - Set timer resolution
28. **HeapAlloc** (x7) - Multiple heap allocations
29. **GetSystemMetrics** (SM_CYSCREEN, SM_CXSCREEN) - Get screen size
30. **CreateWindowExA** - Create window
31. ... continues with window messages ...

## Analysis

### Where Emulator Stops
The emulator successfully calls **LoadCursorA** (API #23) but then enters an infinite loop without calling:
- LoadIconA
- GetStockObject
- RegisterClassA
- timeBeginPeriod

### Possible Issues

#### 1. LoadCursorA Return Value
**ApiMon**: Returns `0x00010003`
**Emulator**: Returns `0x00017F00`

The return values are different! This could cause the game to behave differently.

#### 2. Missing GetACP Call
Looking at the sequence:
- ApiMon shows GetACP is called AFTER SetHandleCount
- Need to verify emulator calls GetACP

#### 3. GetProcAddress + Dynamic Call
The game calls `GetProcAddress` to get `IsProcessorFeaturePresent` dynamically, then calls it.
- This creates a synthetic export at address 0x0E000000
- Need to verify this works correctly

#### 4. Infinite Loop Hypothesis
The game likely:
1. Calls LoadCursorA
2. Checks the return value
3. If invalid, enters error handling loop
4. Never proceeds to LoadIconA

## Next Steps for Investigation

### 1. Verify GetACP is Called
```bash
# Check if GetACP appears in emulator output
grep "GetACP" /tmp/emulator_output.log
```

### 2. Check LoadCursorA Return Value
The cursor handle might matter. The game expects a specific format.

### 3. Check GetProcAddress/IsProcessorFeaturePresent
Verify the dynamic function call works:
```csharp
// Should create synthetic export at 0x0E000000
// Should be callable via that address
```

### 4. Add Breakpoint After LoadCursorA
Use interactive debugger to see what the game does after LoadCursorA returns.

## Detailed Sequence Comparison Needed

Create a tool to:
1. Extract all API calls from emulator run (with logging enabled)
2. Compare side-by-side with ApiMon sequence
3. Find exact point of divergence
4. Identify the problematic API or return value

## Commands to Run

```bash
# Run emulator with full logging
cd /home/runner/work/Win32Emu/Win32Emu
dotnet run --project Win32Emu/Win32Emu.csproj -- ./EXEs/ign_teas/IGN_TEAS.EXE 2>&1 | \
    grep -E "Dispatching|returned" | \
    head -50 > /tmp/emulator_sequence.log

# Extract API sequence from emulator
awk '/Dispatching/ {
    match($0, /Dispatching ([^!]+)!([^ ]+)/, m);
    print m[1] "." m[2]
}' /tmp/emulator_sequence.log

# Compare with ApiMon
# Line by line comparison to find divergence point
```
