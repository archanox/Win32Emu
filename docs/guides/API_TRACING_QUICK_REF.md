# API Call Tracing - Quick Reference

## Quick Start

```bash
# Enable API tracing with console output
Win32Emu.Gui --nogui game.exe --trace-api

# Save trace to file for analysis
Win32Emu.Gui --nogui game.exe --trace-api trace_output.log

# Compare against real Windows behavior
Win32Emu.Gui --nogui game.exe --trace-api trace.log --compare-apimon "ApiMon Logs/game.csv"
```

## What Gets Traced

- **Win32 API calls**: All calls to Kernel32, User32, GDI32, etc.
- **DirectX COM methods**: DirectDraw, DirectInput, DirectSound interfaces
- **Return values**: What each API returns
- **CPU state**: EIP (instruction pointer) for each call
- **Timing data**: Execution time in microseconds (when available)

## Trace Output Format

```
[     125]   2.450123s EIP=0x00401234 KERNEL32.GetVersion() = 0x23F00218
[     126]   2.450145s EIP=0x00401239 KERNEL32.HeapCreate(3 params) = 0x0A0E0000 [75μs]
[     127]   2.450231s EIP=0x00401245 COM.IDirectDraw::CreateSurface(...) = 0x00000000 [3590μs]
```

- `[125]`: Call number (sequential)
- `2.450123s`: Timestamp from session start
- `EIP=0x00401234`: Instruction pointer when call was made
- `KERNEL32.GetVersion()`: Module and function name
- `= 0x23F00218`: Return value (in hex)
- `[75μs]`: Execution time in microseconds (optional)

## Diagnostic Report

At the end of execution, a comprehensive report is automatically generated:

### Top Called APIs
Shows the 20 most frequently called APIs and their average execution time.

### Slowest APIs
Lists the 20 APIs that consumed the most total execution time.

### DirectX COM Calls
Breakdown of DirectX method calls (DirectDraw, DirectInput, DirectSound).

## Example: Diagnosing ign_teas.exe

```bash
# Run with tracing
Win32Emu.Gui --nogui ./EXEs/ign_teas/IGN_TEAS.EXE \
  --trace-api ign_teas_trace.log \
  --compare-apimon "ApiMon Logs/ign_teas/ign_teas.exe.csv" \
  --debug

# After execution, check the trace
tail -n 200 ign_teas_trace.log
```

### Finding Issues

**1. Missing APIs**: Search for APIs that should be called but aren't:
```bash
grep "DirectInputDevice" ign_teas_trace.log
# If GetDeviceState is missing, that's the problem!
```

**2. Stubbed Methods**: Look for calls that complete suspiciously fast:
```bash
grep "stub" ign_teas_trace.log
# Shows which methods are not actually implemented
```

**3. Divergence Point**: Check the comparison report:
```
Behavior diverges at call #4893:
Expected: DINPUT.DLL.IDirectInputDevice::GetDeviceState(...) = 0x00000000
Actual:   (emulation stopped)
```
This tells you exactly where the emulator stops behaving like Windows.

## Advanced Usage

### Filtering Large Traces

```bash
# Only show DirectInput calls
grep "DINPUT\|IDirectInput" ign_teas_trace.log > dinput_only.log

# Only show errors (non-zero returns for APIs that return HRESULT)
grep "COM\." ign_teas_trace.log | grep -v "= 0x00000000"

# Show slow calls (over 100μs)
grep "μs]" ign_teas_trace.log | awk -F'[][]' '$2 > 100'
```

### Combining with Other Debug Tools

```bash
# Trace + Interactive Debugger
Win32Emu.Gui --nogui game.exe --trace-api --interactive-debug

# Trace + GDB Server
Win32Emu.Gui --nogui game.exe --trace-api --gdb-server

# Trace + OpenTelemetry
Win32Emu.Gui --nogui game.exe --trace-api --telemetry-console
```

## Programmatic Access

```csharp
// Enable tracing from code
emulator.Environment?.EnableApiTracing("trace.log", enableDetailedParameters: true);

// Wire to dispatcher
if (emulator.Win32Dispatcher != null && emulator.Environment?.ApiCallTracer != null)
{
    emulator.Win32Dispatcher.SetApiCallTracer(emulator.Environment.ApiCallTracer);
}

// Get report later
var report = emulator.Environment?.DisableApiTracing();
Console.WriteLine(report);
```

## Common Issues

### "API call tracing already enabled"
You tried to enable tracing twice. This is harmless but unnecessary.

### "No API calls recorded"
The program didn't make it far enough to call any Win32 APIs. Check if it crashed during PE loading.

### Trace file is huge
Large games may generate millions of API calls. Consider:
- Running for a shorter time
- Using `--compare-apimon` to focus on divergence point
- Filtering the output with grep/awk

## Performance Impact

API tracing adds minimal overhead:
- **Console logging**: ~1-5μs per call
- **File logging**: ~5-10μs per call
- **Total impact**: Usually <1% for normal games

For performance-critical analysis, use `--telemetry-otlp` instead, which has lower overhead.

## See Also

- [DIAGNOSING_UNKNOWN_ISSUES.md](./DIAGNOSING_UNKNOWN_ISSUES.md) - Complete diagnostic workflow
- [DEBUGGING_GUIDE.md](./DEBUGGING_GUIDE.md) - General debugging guide
- [OPENTELEMETRY_USAGE.md](./OPENTELEMETRY_USAGE.md) - Distributed tracing alternative
