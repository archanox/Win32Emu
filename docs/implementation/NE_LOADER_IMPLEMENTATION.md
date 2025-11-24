# NE (New Executable) Loader Implementation

## Overview

Win32Emu now supports Win16 NE (New Executable) format applications in addition to PE32 (Win32) executables. This enables running 16-bit Windows installers and applications common for Windows 9x games.

## Architecture

### Format Detection

The emulator automatically detects the executable format before loading:

```csharp
var format = PeImageLoader.DetectFormat(bytes);
switch (format)
{
    case ExecutableFormat.PE32:
        // Load as Win32 PE executable
        break;
    case ExecutableFormat.NE:
        // Load as Win16 NE executable
        break;
    default:
        throw new NotSupportedException($"Unsupported format: {format}");
}
```

Detection is based on the signature at the DOS header offset:
- **MZ Signature**: 0x5A4D (little-endian for 'M', 'Z')
- **NE Signature**: 0x454E (little-endian for 'N', 'E')
- **PE Signature**: 0x4550 (little-endian for 'P', 'E')

### NE File Structure

The NE format consists of:

1. **DOS Stub** (MZ header)
   - Offset 0x3C contains pointer to NE header

2. **NE Header** (at variable offset)
   - Linker version
   - Entry table offset and length
   - Segment table offset and count
   - Resource table offset
   - Name table offsets (resident and non-resident)
   - Module reference table
   - Entry point (segment:offset)
   - Target OS and Windows version

3. **Segment Table**
   - Fixed and movable segments
   - File offset, length, flags
   - Memory allocation requirements

4. **Entry Table**
   - Exported function ordinals
   - Segment and offset for each entry
   - Bundles for efficient storage

5. **Name Tables**
   - **Resident**: Module name and frequently-used exports
   - **Non-resident**: Less frequently-used exports

6. **Module Reference Table**
   - List of imported DLL names

7. **Imported Names Table**
   - Names of imported functions

8. **Resource Data**
   - Icons, dialogs, strings, etc.

## Implementation Details

### Segment Loading

NE executables use a segmented memory model:

```csharp
// Segments start at 0x10000 (64KB) to avoid NULL pointer conflicts
uint baseAddress = 0x00010000;

// Align each segment to paragraph boundary (16 bytes)
currentAddress = (currentAddress + 0xF) & 0xFFFFFFF0;

// Load segment data
segmentMap[segmentNumber] = (currentAddress, length);
vm.WriteBytes(currentAddress, segmentData);
```

#### Segment Flags

NE segment flags are converted to PE section characteristics:

| NE Flag | Meaning | PE Characteristic |
|---------|---------|-------------------|
| 0x0001 | Data segment | ContainsInitializedData |
| 0x0000 | Code segment | ContainsCode + MemExecute |
| 0x0008 | Read-only | (None - writable by default) |

### Entry Point Translation

NE entry points use segment:offset addressing:

```csharp
// Entry point: Segment 1, Offset 0x0100
// Translate to linear address
if (segmentMap.TryGetValue(entrySegment, out var segment))
{
    entryPointAddress = segment.address + entryOffset;
}
```

### Import/Export Handling

#### Exports

Exports are parsed from resident and non-resident name tables:

```csharp
// Name table format: length-prefixed string + 16-bit ordinal
var nameLength = bytes[offset];
var name = Encoding.ASCII.GetString(bytes, offset + 1, nameLength);
var ordinal = BitConverter.ToUInt16(bytes, offset + nameLength + 1);
```

#### Imports

Win16 module names are mapped to Win32 equivalents:

| Win16 Module | Win32 Module | Purpose |
|--------------|--------------|---------|
| KERNEL | KERNEL32.DLL | Memory, files, modules |
| USER | USER32.DLL | Windows, messages |
| GDI | GDI32.DLL | Graphics device interface |
| KEYBOARD | USER32.DLL | Keyboard input |
| SOUND | WINMM.DLL | Audio playback |
| SYSTEM | KERNEL32.DLL | System information |

### Compatibility with PE Infrastructure

The NE loader returns a `LoadedImage` structure compatible with the existing PE loader:

```csharp
return new LoadedImage(
    baseAddress,
    entryPointAddress,
    imageSize,
    importMap,
    sourcePath,
    exportsByName,
    exportsByOrdinal,
    // ... PE-compatible fields with sensible defaults
);
```

This allows Win16 executables to use the same emulator infrastructure as Win32 executables.

## Limitations and Future Work

### Current Limitations

1. **No 16-bit API emulation yet**: Win16 API calls are not yet implemented
2. **No thunking layer**: No translation between 16-bit and 32-bit calling conventions
3. **No segment relocations**: Assumes segments load at fixed addresses
4. **No resource loading**: NE resources not yet accessible
5. **No DLL loading**: Cannot load Win16 DLLs dynamically

### Planned Enhancements

1. **16-bit Address Translation**
   - Implement segment:offset to linear address conversion
   - Handle far pointers (segment + offset)
   - Support segment register manipulation

2. **Win16 API Thunking**
   - Map Win16 APIs to Win32 equivalents
   - Handle 16-bit calling conventions (Pascal, stdcall)
   - Translate data structures (WORD vs DWORD)

3. **Win16 API Stubs**
   - KERNEL: GlobalAlloc, LocalAlloc, LoadLibrary16
   - USER: CreateWindow16, GetMessage16, DispatchMessage16
   - GDI: CreateDC16, SelectObject16, TextOut16

4. **Dynamic DLL Loading**
   - Load Win16 DLLs on-demand
   - Resolve imports at runtime
   - Handle DLL initialization

5. **Resource Support**
   - Parse NE resource directory
   - Load dialogs, menus, icons
   - Support 16-bit resource APIs

## Testing

The NE loader includes comprehensive unit tests:

```bash
dotnet test --filter "FullyQualifiedName~NeImageLoaderTests"
```

Test coverage:
- ✓ Format detection (file and byte array)
- ✓ Invalid format rejection
- ✓ Minimal NE file loading
- ✓ Signature validation
- ✓ Header parsing

## References

### Win16 Emulation Projects

- [winevdm](https://github.com/otya128/winevdm) - Wine VDM for running Win16 on Win64
- [win16ne](https://github.com/qnighy/win16ne) - Rust NE format parser
- [semblance](https://github.com/zfigura/semblance) - C++ 16-bit Windows emulator

### NE Format Specifications

- [Microsoft PE/COFF Specification](https://learn.microsoft.com/en-us/windows/win32/debug/pe-format)
- [NE File Format Wiki](https://wiki.osdev.org/NE)
- [Windows Executable File Format](http://www.program-transformation.org/Transform/ProgramDatabaseExeFormat)

### Win16 API Documentation

- [Windows 3.1 SDK Documentation](https://archive.org/details/windows-3.1-sdk-documentation)
- [Win16 API Reference](https://www.pcjs.org/software/pcx86/sys/windows/3.10/)

## Code Examples

### Loading a Win16 Executable

```csharp
using Win32Emu;
using Win32Emu.Loader;

// Automatic format detection
var emulator = new Emulator();
emulator.LoadExecutable("game16.exe");
emulator.Run();
```

### Manual Format Detection

```csharp
var format = PeImageLoader.DetectFormat("installer.exe");
if (format == ExecutableFormat.NE)
{
    Console.WriteLine("Win16 NE executable detected");
}
```

### Parsing NE Headers

```csharp
var vm = new VirtualMemory(256 * 1024 * 1024);
var loader = new NeImageLoader(vm);
var image = loader.Load("win16app.exe");

Console.WriteLine($"Base: 0x{image.BaseAddress:X8}");
Console.WriteLine($"Entry: 0x{image.EntryPointAddress:X8}");
Console.WriteLine($"Segments: {image.Sections.Length}");
```

## Debugging Win16 Executables

### Common Issues

1. **Invalid NE Signature**
   - Verify file is actually NE format (not PE or LE)
   - Check DOS stub offset at 0x3C

2. **Segment Loading Failures**
   - Ensure file offsets are valid
   - Check segment alignment requirements

3. **Entry Point Errors**
   - Verify segment number is valid
   - Check offset is within segment bounds

### Debug Logging

Enable verbose logging to trace NE loading:

```csharp
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole().SetMinimumLevel(LogLevel.Debug);
});

var emulator = new Emulator(null, loggerFactory.CreateLogger<Emulator>());
emulator.LoadExecutable("win16app.exe", debugMode: true);
```

Sample output:
```
[NE Loader] Loading NE executable from game.exe
[NE Loader] NE version: 5.10
[NE Loader] Target OS: 2 (Windows)
[NE Loader] Loaded 3 segments
[NE Loader] Found 12 entry points
[NE Loader] Entry point: Segment 1, Offset 0x0100 -> VA 0x00010100
```

## Contributing

To contribute Win16 support improvements:

1. Study the NE format specification
2. Add tests for new functionality
3. Follow existing coding standards
4. Update this documentation
5. Test with real Win16 executables

See [CONTRIBUTING.md](../../CONTRIBUTING.md) for general contribution guidelines.
