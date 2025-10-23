# JIT Cache Usage Examples

This document provides practical examples of using the JIT cache feature in Win32Emu.

## Example 1: Basic Usage with Automatic Caching

The simplest way to use JIT caching is to enable it when creating the JitCpu:

```csharp
using Win32Emu.Cpu.Jit;
using Win32Emu.Memory;

// Create memory and CPU
var memory = new VirtualMemory(64 * 1024 * 1024); // 64MB
var cpu = new JitCpu(memory);

// Set the executable path to enable caching
cpu.SetExecutablePath("/games/doom/doom.exe");

// Load existing cache from previous runs
await cpu.LoadCacheAsync();

// Execute your program
// Blocks are automatically compiled and cached
while (true)
{
    var result = await cpu.ExecuteBlockAsync(memory);
    
    if (result.IsCall)
    {
        // Handle Win32 API calls
        break;
    }
}

// Save cache for next run
await cpu.SaveCacheAsync();

// Check statistics
var stats = cpu.GetCacheStatistics();
Console.WriteLine($"Cached {stats.TotalBlocks} blocks with {stats.TotalInstructions} instructions");
```

## Example 2: Custom Cache Directory

You can specify a custom location for the cache:

```csharp
// Use a game-specific cache directory
var gameName = "MyFavoriteGame";
var cacheDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "Win32Emu",
    "GameCaches",
    gameName
);

var cpu = new JitCpu(memory, logger: null, cacheDirectory: cacheDir);
cpu.SetExecutablePath("/games/game.exe");

await cpu.LoadCacheAsync();
// ... run game ...
await cpu.SaveCacheAsync();
```

## Example 3: Precompilation for Faster Startup

Precompile code to reduce JIT overhead during gameplay:

```csharp
var cpu = new JitCpu(memory);
cpu.SetExecutablePath("/games/quake/quake.exe");

// Load cache
await cpu.LoadCacheAsync();

// Option 1: Precompile all cached blocks
Console.WriteLine("Precompiling all cached blocks...");
var cachedCompiled = await cpu.PrecompileFromCacheAsync(memory);
Console.WriteLine($"Precompiled {cachedCompiled} blocks from cache");

// Option 2: Precompile specific address range (e.g., .text section)
var textSectionStart = 0x00401000u;
var textSectionEnd = 0x00450000u;

Console.WriteLine("Precompiling code range...");
var rangeCompiled = await cpu.PrecompileRangeAsync(memory, textSectionStart, textSectionEnd);
Console.WriteLine($"Precompiled {rangeCompiled} blocks in range");

// Now execute - precompiled blocks will run immediately
await cpu.ExecuteBlockAsync(memory);

// Save updated cache
await cpu.SaveCacheAsync();
```

## Example 4: Cache Management and Cleanup

```csharp
var cpu = new JitCpu(memory);
cpu.SetExecutablePath("/games/game.exe");

// Load cache
await cpu.LoadCacheAsync();

// Get statistics before execution
var beforeStats = cpu.GetCacheStatistics();
Console.WriteLine($"Cache started with {beforeStats.TotalBlocks} blocks");

// Execute game
// ... game runs ...

// Get statistics after execution
var afterStats = cpu.GetCacheStatistics();
Console.WriteLine($"Cache now has {afterStats.TotalBlocks} blocks");
Console.WriteLine($"New blocks: {afterStats.TotalBlocks - beforeStats.TotalBlocks}");

// Save cache
await cpu.SaveCacheAsync();
Console.WriteLine($"Cache saved to: {afterStats.CacheDirectory}");
```

## Example 5: Integration with Emulator Class

If you're using the higher-level `Emulator` class, you can access the JIT cache through the CPU:

```csharp
// Assuming the emulator is configured to use JitCpu
var emulator = new Emulator(executablePath);

// Get the CPU (if it's a JitCpu)
if (emulator.Cpu is JitCpu jitCpu)
{
    // Set executable path
    jitCpu.SetExecutablePath(executablePath);
    
    // Load cache
    await jitCpu.LoadCacheAsync();
    
    // Run emulator
    await emulator.RunAsync();
    
    // Save cache when done
    await jitCpu.SaveCacheAsync();
    
    // Print statistics
    var stats = jitCpu.GetCacheStatistics();
    Console.WriteLine($"JIT Cache Statistics:");
    Console.WriteLine($"  Total Blocks: {stats.TotalBlocks}");
    Console.WriteLine($"  Total Instructions: {stats.TotalInstructions}");
    Console.WriteLine($"  Cache Location: {stats.CacheDirectory}");
}
```

## Example 6: Logging and Debugging

Enable logging to see what the JIT cache is doing:

```csharp
using Microsoft.Extensions.Logging;

// Create a logger
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Debug);
});

var logger = loggerFactory.CreateLogger<JitCpu>();

// Create CPU with logger
var cpu = new JitCpu(memory, logger);
cpu.SetExecutablePath("/games/game.exe");

// You'll see log messages like:
// [JitCpu] Initialized JIT CPU backend with caching
// [JitCache] Initialized with cache directory: /path/to/cache
// [JitCache] Loaded 150 blocks from cache
// [JitCpu] Compiling block at EIP=0x00401000
// [JitCache] Saved 175 blocks to cache
```

## Example 7: Performance Comparison

Measure the performance benefit of caching:

```csharp
using System.Diagnostics;

var cpu = new JitCpu(memory);
cpu.SetExecutablePath("/games/benchmark.exe");

// First run - no cache
var sw = Stopwatch.StartNew();
await cpu.ExecuteBlockAsync(memory);
sw.Stop();
Console.WriteLine($"First run (no cache): {sw.ElapsedMilliseconds}ms");

// Save cache
await cpu.SaveCacheAsync();

// Second run - with cache
var cpu2 = new JitCpu(memory);
cpu2.SetExecutablePath("/games/benchmark.exe");
await cpu2.LoadCacheAsync();

sw.Restart();
await cpu2.ExecuteBlockAsync(memory);
sw.Stop();
Console.WriteLine($"Second run (with cache): {sw.ElapsedMilliseconds}ms");

var stats = cpu2.GetCacheStatistics();
Console.WriteLine($"Cache utilized {stats.TotalBlocks} blocks");
```

## Cache File Location

By default, cache files are stored in:

**Windows:**
```
%LOCALAPPDATA%\Win32Emu\JitCache\jit_cache_<hash>.json
```

**Linux:**
```
~/.local/share/Win32Emu/JitCache/jit_cache_<hash>.json
```

**macOS:**
```
~/Library/Application Support/Win32Emu/JitCache/jit_cache_<hash>.json
```

Where `<hash>` is a 16-character hash derived from the executable path.

## Best Practices

1. **Always call SaveCacheAsync**: Save the cache after execution to benefit future runs
2. **Handle exceptions**: Cache operations can fail (disk full, permissions, etc.)
3. **Version control**: If your emulator changes significantly, consider clearing old caches
4. **Monitor cache size**: Large executables may create large cache files
5. **Use precompilation judiciously**: Only precompile known hot paths to avoid overhead

## Troubleshooting

### Cache not loading

```csharp
// Check if cache directory exists
var stats = cpu.GetCacheStatistics();
Console.WriteLine($"Cache directory: {stats.CacheDirectory}");
Console.WriteLine($"Directory exists: {Directory.Exists(stats.CacheDirectory)}");

// Check if executable path is set
cpu.SetExecutablePath("/correct/path/to/game.exe");
await cpu.LoadCacheAsync();
```

### Cache not providing speedup

The cache stores metadata, not compiled code. The actual compilation still happens at runtime, but:
- Block analysis is faster (boundaries are known)
- Instruction decoding is skipped for known blocks
- Hot paths can be identified and prioritized

The speedup is most noticeable for:
- Complex control flow analysis
- Large executables with many code paths
- Repeated executions of the same code

## See Also

- [JIT_CACHE_IMPLEMENTATION.md](JIT_CACHE_IMPLEMENTATION.md) - Technical implementation details
- [ASYNC_JIT_ARCHITECTURE.md](ASYNC_JIT_ARCHITECTURE.md) - JIT CPU architecture
- Win32Emu.Tests.Emulator/JitCacheTests.cs - Unit tests demonstrating usage
