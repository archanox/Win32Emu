# Reko Integration Example

This example demonstrates how to use the pluggable decompiler adapter pattern in Win32Emu.

## Default Behavior (CustomRTL)

```csharp
using Win32Emu.Rtl;
using Microsoft.Extensions.Logging;
using Iced.Intel;

// Create logger
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

// Create JIT cache (uses CustomRTL by default)
var cache = new RtlJitCache(cacheDir: "./cache", logger: logger);
// To check which adapter is being used, inspect logs:
// Output: [RtlJitCache] Using decompiler: CustomRTL (MIT License - Part of Win32Emu)

// Compile x86 instructions
var instructions = new List<Instruction> {
    /* ... x86 instructions ... */
};
var block = await cache.CompileBlockAsync(0x401000, instructions);

// Generated C# source is saved to ./cache/Source/JitBlock_<id>_00401000.cs
Console.WriteLine($"C# source: {block.CSharpSource}");
```

## With Reko (Opt-In)

### Step 1: Add Reko NuGet Package

```bash
dotnet add package Reko.Decompiler.Runtime --version 0.11.6
```

### Step 2: Enable Reko via Environment Variable

```bash
export WIN32EMU_USE_REKO=true
```

### Step 3: Run Your Application

```csharp
using Win32Emu.Rtl;
using Microsoft.Extensions.Logging;

// Create logger
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

// RtlJitCache automatically detects Reko is enabled and available
var cache = new RtlJitCache(cacheDir: "./cache", logger: logger);
// Output: [RtlJitCache] Using decompiler: Reko (GPLv2 - Reko Decompiler)
// Output: [RekoAdapter] Reko decompiler is enabled. Note: Reko is GPLv2 licensed.

// Use it the same way - interface is identical
var instructions = new List<Instruction> { /* ... */ };
var block = await cache.CompileBlockAsync(0x401000, instructions);
```

## Explicit Adapter Selection

```csharp
using Win32Emu.Rtl;
using Microsoft.Extensions.Logging;

// Create logger
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

// Option 1: Use CustomRTL explicitly
var customAdapter = new CustomRtlDecompilerAdapter(logger);
var cache1 = new RtlJitCache(cacheDir: "./cache1", logger: logger, 
                             decompilerAdapter: customAdapter);

// Option 2: Try Reko, fall back to CustomRTL
var rekoAdapter = new RekoDecompilerAdapter(logger);
if (rekoAdapter.IsAvailable)
{
    logger.LogInformation("Using Reko decompiler");
    var cache2 = new RtlJitCache(cacheDir: "./cache2", logger: logger, 
                                 decompilerAdapter: rekoAdapter);
}
else
{
    logger.LogInformation("Reko not available, using CustomRTL");
    var cache2 = new RtlJitCache(cacheDir: "./cache2", logger: logger);
}

// Option 3: Compare both adapters
var instructions = new List<Instruction> { /* ... */ };

var customCode = await customAdapter.DecompileToCSharpAsync(0x401000, instructions, "Test_Custom");
Console.WriteLine("CustomRTL output:");
Console.WriteLine(customCode);

if (rekoAdapter.IsAvailable)
{
    var rekoCode = await rekoAdapter.DecompileToCSharpAsync(0x401000, instructions, "Test_Reko");
    Console.WriteLine("\nReko output:");
    Console.WriteLine(rekoCode);
}
```

## Checking Adapter Status

```csharp
using Win32Emu.Rtl;

// Check what adapters are available
var customAdapter = new CustomRtlDecompilerAdapter();
Console.WriteLine($"CustomRTL available: {customAdapter.IsAvailable}"); // Always true
Console.WriteLine($"License: {customAdapter.LicenseInfo}");

var rekoAdapter = new RekoDecompilerAdapter();
Console.WriteLine($"Reko available: {rekoAdapter.IsAvailable}"); // true if enabled + package installed
Console.WriteLine($"License: {rekoAdapter.LicenseInfo}");

// List available adapters
var adapters = new IDecompilerAdapter[] { customAdapter, rekoAdapter };
foreach (var adapter in adapters.Where(a => a.IsAvailable))
{
    Console.WriteLine($"- {adapter.Name}: {adapter.LicenseInfo}");
}
```

## Testing Different Adapters

```csharp
using Win32Emu.Rtl;
using System.Diagnostics;

// Benchmark decompilation performance
var instructions = /* large block of x86 instructions */;

// Test CustomRTL
var sw = Stopwatch.StartNew();
var customAdapter = new CustomRtlDecompilerAdapter();
var customCode = await customAdapter.DecompileToCSharpAsync(0x401000, instructions, "Benchmark");
sw.Stop();
Console.WriteLine($"CustomRTL: {sw.ElapsedMilliseconds}ms, {customCode.Length} chars");

// Test Reko (if available)
var rekoAdapter = new RekoDecompilerAdapter();
if (rekoAdapter.IsAvailable)
{
    sw.Restart();
    var rekoCode = await rekoAdapter.DecompileToCSharpAsync(0x401000, instructions, "Benchmark");
    sw.Stop();
    Console.WriteLine($"Reko: {sw.ElapsedMilliseconds}ms, {rekoCode.Length} chars");
}
```

## CLI Integration

```bash
# Use default decompiler (CustomRTL)
Win32Emu.Gui --nogui game.exe

# Use Reko decompiler (requires package + env var)
WIN32EMU_USE_REKO=true Win32Emu.Gui --nogui game.exe

# Future: Command-line option (not yet implemented)
# Win32Emu.Gui --nogui game.exe --decompiler Reko
```

## Output Comparison

### CustomRTL Output (Default)

```csharp
using System;
using System.Threading.Tasks;

namespace Win32Emu.Generated
{
    public class JitBlock_abc12345_00401000
    {
        public async Task<dynamic> Execute(dynamic cpu, dynamic mem)
        {
            // Load CPU state
            uint EAX = cpu.GetRegister("EAX");
            uint EBX = cpu.GetRegister("EBX");
            
            // Block instructions
            EAX = 0x5u; // @0x401000
            EBX = EAX + 0x3u; // @0x401002
            mem.Write32(0x403000u, EBX); // @0x401005
            
            // Save CPU state
            cpu.SetRegister("EAX", EAX);
            cpu.SetRegister("EBX", EBX);
            
            return new { IsCall = false };
        }
    }
}
```

### Reko Output (When Implemented)

```csharp
using System;
using System.Threading.Tasks;

namespace Win32Emu.Generated
{
    // Decompiled using Reko (GPLv2) - GPLv2 - Reko Decompiler (https://github.com/uxmal/reko)
    // Note: This code is subject to GPLv2 licensing requirements
    public class JitBlock_abc12345_00401000
    {
        // Block at 0x00401000
        // Contains 5 x86 instructions
        
        public async Task<dynamic> Execute(dynamic cpu, dynamic mem)
        {
            // TODO: Integrate Reko's decompilation output here
            // See RekoDecompilerAdapter implementation for integration details
            throw new NotImplementedException("Reko integration requires additional implementation");
        }
    }
}
```

## License Compliance

### CustomRTL (Default)
- **License**: MIT
- **Compatible with**: Any project (open source or proprietary)
- **Requirements**: None
- **Source**: Part of Win32Emu

### Reko (Opt-In)
- **License**: GPLv2
- **Compatible with**: GPLv2 or GPLv3 projects only
- **Requirements**: 
  - Must release source code if distributed
  - Derivative works must also be GPLv2
  - Cannot use in proprietary software
- **Source**: https://github.com/uxmal/reko

When using Reko, ensure your project complies with GPLv2 requirements. The adapter logs warnings to remind you.

## Troubleshooting

### "Reko is enabled but not available"

```
[RekoAdapter] Reko decompiler is enabled but Reko.Decompiler.Runtime package is not available.
```

**Solution**: Add the NuGet package:
```bash
dotnet add package Reko.Decompiler.Runtime
```

### "Using CustomRTL instead of Reko"

Even with `WIN32EMU_USE_REKO=true`, RtlJitCache falls back to CustomRTL if Reko isn't available.

**Check**:
1. Is Reko package installed?
2. Is environment variable set correctly?
3. Are Reko assemblies loadable?

### License Concerns

If you're unsure about GPLv2 compliance:
- **For personal use**: No restrictions
- **For internal use**: Generally okay (not distributing)
- **For distribution**: Must release source code under GPLv2
- **Consult legal counsel** if uncertain

**Safe option**: Use CustomRTL (MIT-licensed, no restrictions)

## Future Plans

### Adapter Enhancements
- Complete Reko API integration
- Add Ghidra adapter (via pyhidra)
- Add IDA Pro adapter (via IDA SDK)
- Add Binary Ninja adapter

### Quality Metrics
- Compare decompiler outputs
- Measure readability scores
- Performance benchmarks
- Correctness testing

### Configuration
- CLI option: `--decompiler <name>`
- Config file support
- Per-function adapter selection

## See Also

- [REKO_INTEGRATION_GUIDE.md](../implementation/REKO_INTEGRATION_GUIDE.md) - Full implementation details
- [RTL_JIT_IMPLEMENTATION.md](../implementation/RTL_JIT_IMPLEMENTATION.md) - RTL pipeline architecture
- [Reko Documentation](https://github.com/uxmal/reko/wiki) - Official Reko docs
