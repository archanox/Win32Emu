# Diagnosing ign_teas.exe Emulation Issues - Minimizing Unknowns

This guide explains how to systematically diagnose and fix emulation issues by removing or minimizing unknowns through comprehensive diagnostic tools.

## Problem Statement

When diagnosing issues with `ign_teas.exe` (or any emulated program), we face many unknowns:
- What API calls are being made during execution?
- What parameters are passed to these calls?
- Which DirectX methods are actually invoked?
- Where does execution diverge from expected behavior?
- What's the state of the emulator at failure points?

This makes it difficult to precisely identify and fix the root cause.

## Solution: Comprehensive Diagnostic Toolset

Win32Emu now includes several tools to eliminate these unknowns:

### 1. API Call Tracing (`--trace-api`)

**Purpose**: Logs every Win32 API and DirectX COM method call in real-time.

**Usage**:
```bash
# Basic tracing (console output only)
Win32Emu.Gui --nogui ./EXEs/ign_teas/IGN_TEAS.EXE --trace-api

# Save trace to file for analysis
Win32Emu.Gui --nogui ./EXEs/ign_teas/IGN_TEAS.EXE --trace-api trace_ign_teas.log
```

**Output**: Creates a detailed log showing:
- Call number and timestamp
- Module and function name
- Parameters (optional: full or count only)
- Return values
- Execution time (in microseconds)
- CPU state (EIP)

**Example Output**:
```
[     125]   2.450123s EIP=0x00401234 KERNEL32.GetVersion() = 0x23F00218 [15μs]
[     126]   2.450145s EIP=0x00401239 KERNEL32.HeapCreate(HEAP_NO_SERIALIZE, 4096, 0) = 0x0A0E0000 [75μs]
[     127]   2.450231s EIP=0x00401245 COM.IDirectDraw::CreateSurface(...) = 0x00000000 [3590μs]
```

**Benefits**:
- See exactly what the program is doing in real-time
- Identify which APIs are called most frequently
- Find slow operations that may be causing performance issues
- Spot unexpected API calls that indicate bugs

### 2. API Monitor Comparison (`--compare-apimon`)

**Purpose**: Compares emulated behavior against real Windows execution.

**Prerequisites**: 
1. Run the game on real Windows with API Monitor
2. Export the log as CSV (available in `ApiMon Logs/ign_teas/ign_teas.exe.csv`)

**Usage**:
```bash
Win32Emu.Gui --nogui ./EXEs/ign_teas/IGN_TEAS.EXE \
  --trace-api \
  --compare-apimon "ApiMon Logs/ign_teas/ign_teas.exe.csv"
```

**Output**: Generates a comparison report showing:
- Point where behavior diverges
- API call frequency differences
- Missing APIs (called in real Windows but not in emulator)
- Extra APIs (called in emulator but not in real Windows)

**Example Report**:
```
API Behavior Comparison Report
================================================================================

Expected API calls (API Monitor): 5,715
Actual API calls (Emulated):     4,892

Behavior diverges at call #4893:
--------------------------------------------------------------------------------
Expected: DINPUT.DLL.IDirectInputDevice::GetDeviceState(...) = 0x00000000
Actual:   (emulation stopped)

API Call Frequency Comparison:
--------------------------------------------------------------------------------
API                                      Expected      Actual        Diff
--------------------------------------------------------------------------------
KERNEL32.GetVersion                            1            1           0
KERNEL32.HeapCreate                            1            1           0
DDRAW.DirectDrawCreate                         1            1           0
DINPUT.DirectInputCreateA                      1            1           0
DINPUT.IDirectInputDevice::GetDeviceState   1247            0       -1247
```

**Benefits**:
- Pinpoints exactly where emulation diverges
- Identifies missing functionality
- Validates that implemented APIs are called correctly
- Shows performance differences (timing)

### 3. Diagnostic Report Generation

**Purpose**: Automatically summarizes the entire emulation session.

**Generated Automatically**: When tracing is enabled, a diagnostic report is generated at the end of execution.

**Report Sections**:
1. **Session Overview**: Total calls, duration
2. **Top Called APIs**: Most frequently called functions
3. **Slowest APIs**: Operations taking the most time
4. **DirectX COM Calls**: Breakdown of DirectX method usage

**Example Report**:
```
API Call Diagnostic Report
================================================================================

Session Duration: 00:00:05.234
Total API Calls: 4,892

Top 20 Most Called APIs:
--------------------------------------------------------------------------------
Function                                           Count    Avg Time (μs)
--------------------------------------------------------------------------------
COM.IDirectDrawSurface::Lock                       1,234          125.3
KERNEL32.GetTickCount                                892            2.1
USER32.PeekMessageA                                  567           15.7
COM.IDirectDrawSurface::Unlock                     1,234           45.2

Top 20 Slowest APIs (by total time):
--------------------------------------------------------------------------------
Function                                       Total Time (ms)    Calls
--------------------------------------------------------------------------------
DDRAW.DirectDrawCreate                                17.03        1
COM.IDirectDraw::SetDisplayMode                      255.87        1
COM.IDirectDrawSurface::Lock                         154.56    1,234
```

**Benefits**:
- Quick overview of program behavior
- Identify performance bottlenecks
- See which APIs need optimization
- Understand API usage patterns

## Workflow for Diagnosing Issues

### Step 1: Capture Baseline Behavior

First, understand what the program should be doing:

```bash
# Check if there's already an API Monitor log
ls -la "ApiMon Logs/ign_teas/"

# If not, review the decompilation
cat Decomp/ign_teas/ANALYSIS.md
cat docs/archive/IGN_TEAS_MISSING_FEATURES.md
```

### Step 2: Run with API Tracing

Execute the program with comprehensive tracing:

```bash
Win32Emu.Gui --nogui ./EXEs/ign_teas/IGN_TEAS.EXE \
  --trace-api trace_ign_teas.log \
  --debug
```

### Step 3: Analyze the Trace Log

Review the trace log to identify issues:

```bash
# Check the last few API calls before program stopped
tail -n 100 trace_ign_teas.log

# Search for error returns
grep "= 0xFFFFFFFF\|= ERROR" trace_ign_teas.log

# Find specific APIs
grep "DirectInput" trace_ign_teas.log
grep "GetDeviceState" trace_ign_teas.log

# Check the diagnostic report at the end
tail -n 200 trace_ign_teas.log
```

### Step 4: Compare Against Real Windows

If you have an API Monitor log from real Windows:

```bash
Win32Emu.Gui --nogui ./EXEs/ign_teas/IGN_TEAS.EXE \
  --trace-api trace_ign_teas.log \
  --compare-apimon "ApiMon Logs/ign_teas/ign_teas.exe.csv" \
  --debug
```

Review the comparison report (at the end of trace_ign_teas.log) to find:
- Where behavior diverges
- Which APIs are missing
- Which APIs behave differently

### Step 5: Identify Root Cause

Based on the analysis:

1. **Missing APIs**: The comparison shows APIs called in real Windows but not implemented
   - Example: `IDirectInputDevice::GetDeviceState` is called 1,247 times but returns error

2. **Stubbed Methods**: Trace shows calls returning success (0) but not doing actual work
   - Example: `IDirectInputDevice::SetDataFormat` logs "stub" message

3. **Incorrect Behavior**: API is called but returns wrong values
   - Example: `GetVersion` returns wrong Windows version

4. **Execution Stops**: Program stops calling APIs at a specific point
   - Example: Game enters infinite loop waiting for a condition that never becomes true

### Step 6: Fix and Verify

After implementing fixes:

```bash
# Run again with tracing to verify the fix
Win32Emu.Gui --nogui ./EXEs/ign_teas/IGN_TEAS.EXE \
  --trace-api trace_ign_teas_fixed.log \
  --compare-apimon "ApiMon Logs/ign_teas/ign_teas.exe.csv"

# Compare before and after
diff trace_ign_teas.log trace_ign_teas_fixed.log
```

## Case Study: Diagnosing ign_teas.exe

### Known Issues from Analysis

From `docs/archive/IGN_TEAS_MISSING_FEATURES.md`:

1. **DirectInput device methods are stubs**:
   - `SetDataFormat` - Doesn't parse input format
   - `Acquire` - Doesn't begin capturing input
   - `GetDeviceState` - Returns zeroed buffer (no actual input)
   - `GetDeviceData` - Returns nothing (stub)

2. **DirectSound buffer methods are stubs**:
   - `Lock` / `Unlock` - Don't manage audio buffers
   - `Play` - Doesn't play audio
   - `SetFormat` - Doesn't configure audio

### Using Tracing to Confirm

```bash
Win32Emu.Gui --nogui ./EXEs/ign_teas/IGN_TEAS.EXE --trace-api trace_dinput.log
```

Look for these patterns in the trace:
```
COM.IDirectInputDevice::SetDataFormat(this=0x..., lpdf=0x...) - stub = 0x00000000
COM.IDirectInputDevice::Acquire(this=0x...) - stub = 0x00000000
COM.IDirectInputDevice::GetDeviceState(this=0x..., cbData=256, lpvData=0x...) - stub = 0x00000000
```

The "stub" messages confirm these methods aren't implemented.

### Verification After Implementation

After implementing `GetDeviceState` properly:
```bash
Win32Emu.Gui --nogui ./EXEs/ign_teas/IGN_TEAS.EXE --trace-api trace_dinput_fixed.log
```

The trace should now show:
```
COM.IDirectInputDevice::GetDeviceState(this=0x..., cbData=256, lpvData=0x...) = 0x00000000 [23μs]
  # No more "stub" message, and faster execution (actual implementation)
```

## Advanced Techniques

### Filtering Trace Output

When traces get very large, filter for specific information:

```bash
# Only show DirectInput calls
grep "DINPUT\|IDirectInput" trace_ign_teas.log > dinput_calls.log

# Only show errors (non-zero return values from APIs that return HRESULT)
grep "COM\." trace_ign_teas.log | grep -v "= 0x00000000" > com_errors.log

# Show timing statistics
grep "μs]" trace_ign_teas.log | sort -t'[' -k2 -n > slow_calls.log
```

### Combining with Other Debug Tools

Use tracing alongside other debugging features:

```bash
# Trace + Interactive Debugger
Win32Emu.Gui --nogui ./EXEs/ign_teas/IGN_TEAS.EXE \
  --trace-api trace.log \
  --interactive-debug

# Trace + GDB Server (for use with Ghidra/IDA)
Win32Emu.Gui --nogui ./EXEs/ign_teas/IGN_TEAS.EXE \
  --trace-api trace.log \
  --gdb-server 1234

# Trace + OpenTelemetry (for distributed tracing)
Win32Emu.Gui --nogui ./EXEs/ign_teas/IGN_TEAS.EXE \
  --trace-api trace.log \
  --telemetry-console
```

### Automated Analysis Scripts

Create scripts to analyze traces:

```python
# analyze_trace.py
import re
from collections import Counter

# Count API calls by module
api_counts = Counter()
with open('trace_ign_teas.log') as f:
    for line in f:
        m = re.search(r'(\w+)\.(\w+)\(', line)
        if m:
            api_counts[m.group(1)] += 1

print("API calls by module:")
for module, count in api_counts.most_common():
    print(f"  {module}: {count}")
```

## Best Practices

1. **Always trace when diagnosing issues**: Don't guess what APIs are being called - trace them!

2. **Compare against real Windows**: Use API Monitor logs as ground truth

3. **Start with high-level view**: Look at the diagnostic report first, then drill down

4. **Focus on divergence point**: The exact call where behavior differs is most important

5. **Verify fixes with tracing**: Re-run with tracing after implementing fixes

6. **Keep traces organized**: Save traces with descriptive names (e.g., `trace_ign_teas_before_dinput_fix.log`)

7. **Document findings**: Update documentation with what you learned

## Conclusion

By using comprehensive API tracing and comparison tools, we eliminate the unknowns that make debugging difficult:

- ✅ **Know what APIs are called**: Full trace shows every call
- ✅ **Know the parameters**: Detailed parameter logging
- ✅ **Know the return values**: See what the emulator returns
- ✅ **Know where it diverges**: Comparison pinpoints the exact call
- ✅ **Know the performance**: Timing data shows bottlenecks
- ✅ **Know the frequency**: Statistics show usage patterns

This transforms debugging from guesswork into a systematic, data-driven process.

## See Also

- [DEBUGGING_GUIDE.md](./DEBUGGING_GUIDE.md) - General debugging guide
- [IGN_TEAS_MISSING_FEATURES.md](../archive/IGN_TEAS_MISSING_FEATURES.md) - Known missing features
- [THREE_WAY_TESTING.md](./THREE_WAY_TESTING.md) - CPU instruction validation
- [OPENTELEMETRY_USAGE.md](./OPENTELEMETRY_USAGE.md) - Distributed tracing
