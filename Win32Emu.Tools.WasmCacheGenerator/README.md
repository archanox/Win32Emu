# Win32Emu.Tools.WasmCacheGenerator

Generates WASM-compatible JIT cache metadata for x86 executables. Pre-analyzes code blocks and stores metadata in JSON format for fast loading in WebAssembly environments.

## Overview

Unlike the `AotCompiler` which generates compiled assemblies using Roslyn, this tool:
- **Does NOT compile** - Only analyzes and stores block metadata
- **WASM-compatible** - Output is JSON that can be loaded in browser
- **No Roslyn** - Works entirely through disassembly and analysis
- **Lightweight** - Metadata-only approach suitable for CI/CD

## Purpose

In WASM environments, dynamic compilation (Roslyn) is not available. This tool pre-analyzes executables during CI builds and generates lightweight metadata that can be:
1. Embedded in the WASM build
2. Loaded instantly without runtime analysis
3. Used by IcedCpu for optimized execution paths

## Usage

### Basic

```bash
dotnet run --project Win32Emu.Tools.WasmCacheGenerator -- game.exe
```

### With Options

```bash
dotnet run --project Win32Emu.Tools.WasmCacheGenerator -- \
  EXEs/ign_teas/IGN_TEAS.EXE \
  --output ign_teas.wasm-cache.json \
  --max-blocks 10000 \
  --verbose
```

### Options

- `--output <file>` - Output JSON file (default: `<exe>.wasm-cache.json`)
- `--max-blocks <n>` - Maximum number of blocks to analyze (default: 10000)
- `--verbose` - Enable verbose logging

## Output Format

The tool generates a JSON file with the following structure:

```json
{
  "version": 1,
  "executablePath": "IGN_TEAS.EXE",
  "timestamp": "2025-12-29T04:00:00Z",
  "blocks": [
    {
      "startAddress": 4198400,
      "instructionCount": 15,
      "byteLength": 42,
      "codeHash": "A1B2C3D4...",
      "firstCompiled": "2025-12-29T04:00:00Z",
      "executionCount": 0,
      "endsWithCall": false,
      "endsWithReturn": true,
      "directTarget": null
    }
  ]
}
```

## CI Integration

Add to `.github/workflows/cpu-test-results.yml`:

```yaml
- name: Generate WASM cache for ign_teas
  run: |
    dotnet run --project Win32Emu.Tools.WasmCacheGenerator -- \
      EXEs/ign_teas/IGN_TEAS.EXE \
      --output Win32Emu.Wasm/wwwroot/cache/ign_teas.wasm-cache.json \
      --max-blocks 5000
```

## WASM Usage

In the WASM frontend, the cache will be automatically loaded:

```csharp
// EmulatorService automatically checks for cache files
await EmulatorService.LoadExecutableAsync(exeBytes, "IGN_TEAS.EXE");
// If ign_teas.wasm-cache.json exists in wwwroot/cache/, it's loaded
```

## Benefits

1. **Faster Startup** - No runtime analysis needed, instant block metadata
2. **No Compilation** - Bypasses Roslyn requirement in WASM
3. **Version Control** - Cache files can be committed to repo
4. **CI Caching** - Generated once in CI, used by all WASM sessions
5. **Bandwidth** - Small JSON files (typically < 1MB for large games)

## Comparison

| Feature | AotCompiler | WasmCacheGenerator |
|---------|-------------|-------------------|
| **Target** | Native platforms | WASM platforms |
| **Output** | Compiled assemblies | JSON metadata |
| **Uses Roslyn** | ✅ Yes | ❌ No |
| **WASM Compatible** | ❌ No | ✅ Yes |
| **Size** | Large (50KB+ per block) | Small (<1KB per block) |
| **Execution** | Native code | Interpreted |
| **Startup** | Very fast (native) | Fast (metadata) |

## Implementation Details

### What Gets Analyzed

For each code block:
- Start address (EIP)
- Instruction count
- Byte length
- SHA256 hash of code bytes
- Control flow (calls, returns, jumps)
- Branch targets

### What Does NOT Get Stored

- Compiled code (no Roslyn)
- IL or machine code
- Optimizations (done at runtime)
- Source code generation

### Limitations

- Maximum 50 instructions per block
- Only reaches reachable code from entry point
- 32-bit PE executables only
- No dynamic code generation detection

## See Also

- [Win32Emu.Tools.AotCompiler](../Win32Emu.Tools.AotCompiler/README.md) - For native platforms
- [JIT_CPU_WASM_COMPATIBILITY.md](../docs/implementation/JIT_CPU_WASM_COMPATIBILITY.md) - WASM architecture
- [JIT_CACHE_IMPLEMENTATION.md](../docs/implementation/JIT_CACHE_IMPLEMENTATION.md) - Cache system

---

**Created**: December 29, 2025  
**Status**: ✅ Ready for use  
**License**: Same as Win32Emu
