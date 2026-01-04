# WASM Cache Directory

This directory contains pre-analyzed JIT cache files for executables.

## Purpose

When running in WebAssembly, the emulator cannot use Roslyn for JIT compilation. Instead, we pre-analyze executables during CI builds and store the metadata here as JSON files.

## File Format

Files follow the naming convention: `<executable>.wasm-cache.json`

Each file contains:
- Block metadata (addresses, sizes, instruction counts)
- Code hashes for verification
- Control flow information

## Usage

The WASM frontend automatically loads these cache files when available, providing:
- Faster startup (no runtime analysis)
- Better performance hints for IcedCpu
- Reduced CPU usage during initialization

## Generation

Cache files are generated in CI using:
```bash
dotnet run --project Win32Emu.Tools.WasmCacheGenerator -- \
  path/to/executable.exe \
  --output wwwroot/cache/executable.wasm-cache.json
```

See `.github/workflows/cpu-test-results.yml` for the CI integration.
