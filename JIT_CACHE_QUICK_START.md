# JIT Cache Quick Start Guide

## What is JIT Caching?

JIT (Just-In-Time) caching saves information about compiled x86 code blocks to disk, making subsequent executions faster by avoiding redundant analysis and compilation work.

## Quick Start

### 1. Enable JIT Caching (Automatic)

```csharp
using Win32Emu.Cpu.Jit;
using Win32Emu.Memory;

var memory = new VirtualMemory(64 * 1024 * 1024);
var cpu = new JitCpu(memory);

// Set executable path - enables automatic caching
cpu.SetExecutablePath("/path/to/your/game.exe");

// Load cache from previous runs
await cpu.LoadCacheAsync();

// Run your game
await cpu.ExecuteBlockAsync(memory);

// Save cache for next time
await cpu.SaveCacheAsync();
```

### 2. Check Cache Statistics

```csharp
var stats = cpu.GetCacheStatistics();
Console.WriteLine($"Cached {stats.TotalBlocks} blocks");
Console.WriteLine($"Cache location: {stats.CacheDirectory}");
```

### 3. Use Precompilation (Optional)

For even faster startup, precompile code before running:

```csharp
// Precompile a specific address range
await cpu.PrecompileRangeAsync(memory, 
    startAddress: 0x00401000,  // Start of .text section
    endAddress: 0x00450000);    // End of .text section
```

## Cache Location

Caches are automatically stored in:

- **Windows**: `%LOCALAPPDATA%\Win32Emu\JitCache\`
- **Linux**: `~/.local/share/Win32Emu/JitCache/`
- **macOS**: `~/Library/Application Support/Win32Emu/JitCache/`

## Performance Tips

1. **Always save the cache** after running - `await cpu.SaveCacheAsync()`
2. **Load cache on startup** - `await cpu.LoadCacheAsync()`
3. **Use precompilation for hot paths** - speeds up frequently executed code
4. **Clear cache if emulator is updated** - ensures compatibility

## Common Patterns

### Pattern 1: Game Launcher
```csharp
async Task RunGame(string gamePath)
{
    var cpu = new JitCpu(memory);
    cpu.SetExecutablePath(gamePath);
    await cpu.LoadCacheAsync();
    
    // Run game
    await RunGameLoop(cpu, memory);
    
    await cpu.SaveCacheAsync();
}
```

### Pattern 2: Benchmark/Test
```csharp
// First run - builds cache
var cpu1 = new JitCpu(memory);
cpu1.SetExecutablePath(testExe);
await cpu1.ExecuteBlockAsync(memory);
await cpu1.SaveCacheAsync();

// Second run - uses cache (faster)
var cpu2 = new JitCpu(memory);
cpu2.SetExecutablePath(testExe);
await cpu2.LoadCacheAsync();
await cpu2.ExecuteBlockAsync(memory);  // Faster!
```

## Troubleshooting

### Q: Cache not loading?
**A**: Ensure `SetExecutablePath()` is called before `LoadCacheAsync()`

### Q: Not seeing performance improvement?
**A**: 
- Make sure you're calling `SaveCacheAsync()` after execution
- Check cache directory exists and has write permissions
- Performance gain is most noticeable on complex executables

### Q: How to clear cache?
**A**: Delete files in the cache directory, or programmatically:
```csharp
var stats = cpu.GetCacheStatistics();
var cacheDir = stats.CacheDirectory;
if (Directory.Exists(cacheDir))
    Directory.Delete(cacheDir, recursive: true);
```

## Next Steps

- See [JIT_CACHE_EXAMPLES.md](JIT_CACHE_EXAMPLES.md) for detailed examples
- Read [JIT_CACHE_IMPLEMENTATION.md](JIT_CACHE_IMPLEMENTATION.md) for technical details
- Check test files in `Win32Emu.Tests.Emulator/JitCacheTests.cs` for more patterns

## API Reference

### Essential Methods

```csharp
// JitCpu class
void SetExecutablePath(string path)
Task LoadCacheAsync()
Task SaveCacheAsync()
Task<int> PrecompileRangeAsync(VirtualMemory mem, uint start, uint end)
CacheStatistics GetCacheStatistics()
```

### CacheStatistics Properties

```csharp
int TotalBlocks          // Number of cached blocks
int TotalInstructions    // Total instructions in cache
string CacheDirectory    // Cache storage location
```

That's it! You're now ready to use JIT caching in Win32Emu.
