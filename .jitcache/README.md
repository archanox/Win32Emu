# JIT Compilation Cache

This directory contains the JIT compilation cache for Win32Emu.

## Purpose

The JIT cache stores metadata about compiled x86 code blocks to improve performance on subsequent runs by:
- Eliminating recompilation overhead for previously seen code
- Storing block metadata (start address, size, instruction count, code hash)
- Enabling faster startup for frequently-run executables

## Structure

- `jit_cache_*.json` - Cache metadata files (one per executable, named by hash of executable path)
- `Source/` - Generated C# source code for inspecting JIT-compiled blocks (RTL cache)

## How It Works

1. **First Run**: JIT compiler translates x86 blocks to native code and records metadata
2. **Cache Save**: On graceful exit, metadata is saved to `jit_cache_<hash>.json`
3. **Subsequent Runs**: Pre-compiled blocks are loaded from cache
4. **Performance**: Eliminates JIT compilation overhead for cached blocks

## Populating the Cache

The cache is automatically populated during emulator execution. To build the cache for ign_teas:

```bash
# Run the emulator (let it run for at least 30-60 seconds to build cache)
cd Win32Emu.Gui/bin/Release/net10.0/
./Win32Emu.Gui --nogui --backend Software ../../../../EXEs/ign_teas/IGN_TEAS.EXE

# Stop with Ctrl+C (graceful shutdown triggers cache save)
```

## Cache Files

Cache files are named using SHA256 hash of the executable path:
- Format: `jit_cache_<first_16_chars_of_sha256>.json`
- Content: JSON with block metadata (address, size, instruction count, code hash)

## Notes

- Cache is persistent across runs
- Cache files should be committed to enable faster CI/testing runs
- Invalid/outdated cache entries are automatically ignored
- Cache directory is automatically created if it doesn't exist
