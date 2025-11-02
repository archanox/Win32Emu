# CHD Disc Image Support

## Overview

Win32Emu now includes support for detecting and validating CHD (Compressed Hunks of Data) disc image files. CHD is a lossless compression format developed for MAME that is commonly used for CD-ROM based games with Redbook audio.

## Current Implementation Status

### ✅ Implemented

- **CHD File Detection**: The emulator can recognize and validate CHD files
- **CHDSharp Integration**: Includes the CHDSharp library (MIT licensed) for CHD format support
- **File Format Validation**: Validates CHD file headers and versions (1-5)
- **Virtual File System Integration**: CHD files are recognized by the VFS layer
- **CD-ROM IOCTL Stubs**: Basic DeviceIoControl handlers for CD-ROM operations:
  - `IOCTL_CDROM_READ_TOC` (stub)
  - `IOCTL_CDROM_GET_LAST_SESSION` (stub)
  - `IOCTL_CDROM_RAW_READ` (stub)

### ⏳ Pending Implementation

- **Block Decompression**: Full CHD block decompression to extract ISO filesystem
- **Track Metadata Reading**: Reading CD track information from CHD metadata
- **Redbook Audio Support**: Playing audio tracks from CHD files
- **Multi-Track Support**: Handling multi-track CD images with mixed data/audio

## Usage

### Detecting CHD Files

CHD files are automatically detected when passed to the `DiskVirtualFileSystem`:

```csharp
using Win32Emu.VirtualFileSystem;

// CHD files will be detected and validated
var vfs = new DiskVirtualFileSystem("game.chd", logger);

// Check if the CHD is valid
if (!vfs.IsReadOnly)
{
    // This should never happen for CHD files
    throw new Exception("CHD files should always be read-only");
}
```

### Using ChdDiscReader Directly

```csharp
using Win32Emu.VirtualFileSystem;

// Open and validate a CHD file
using var reader = new ChdDiscReader("disc.chd", logger);

if (reader.IsValid)
{
    Console.WriteLine($"CHD Version: {reader.Version}");
}
else
{
    Console.WriteLine("Invalid CHD file");
}
```

## CHD File Format

CHD files are compressed disc images that preserve:

- **Track Layout**: All CD tracks (data and audio)
- **Redbook Audio**: CD audio tracks with lossless compression
- **Disc Metadata**: Track offsets, types, and other disc information

The format supports multiple compression methods:
- zlib
- lzma
- huff (Huffman)
- avhuff (Audio/Video Huffman)
- flac (for audio data)

## Creating CHD Files

CHD files are created using the `chdman` tool from MAME:

```bash
# Convert CUE/BIN to CHD
chdman createcd -i game.cue -o game.chd

# Convert GDI (Dreamcast) to CHD
chdman createcd -i game.gdi -o game.chd

# View CHD information
chdman info -i game.chd
```

## Known Limitations

### File System Access

Currently, CHD files are detected and validated, but **file system access is not yet available**. This means:

- ❌ Cannot read files from CHD disc images
- ❌ Cannot mount CHD as a virtual CD-ROM drive
- ❌ Cannot play Redbook audio from CHD files

To use game files from a CHD:

1. **Option 1**: Extract the CHD to CUE/BIN using `chdman extractcd`
2. **Option 2**: Convert to ISO format if the game doesn't require audio tracks

### DeviceIoControl Support

The following CD-ROM IOCTLs are recognized but return failure:

- `IOCTL_CDROM_READ_TOC`: Read table of contents (track list)
- `IOCTL_CDROM_GET_LAST_SESSION`: Get last session information
- `IOCTL_CDROM_RAW_READ`: Read raw sectors from disc

These will be implemented when full CHD decompression is added.

## Future Enhancements

### Phase 1: Block Decompression
- Implement CHD block reading and decompression
- Extract ISO 9660 filesystem from CHD
- Enable file reading from CHD disc images

### Phase 2: Track Support
- Read CHD metadata for track information
- Implement TOC (Table of Contents) generation
- Support multi-track disc images

### Phase 3: Audio Playback
- Decode Redbook audio tracks
- Implement CD-DA (Digital Audio) playback
- Support for games requiring inserted audio CDs

## Technical Details

### CHD Library

Win32Emu uses [CHDSharp](https://github.com/RomVault/CHDSharp) by RomVault:

- **License**: MIT
- **Language**: C# (.NET 9.0)
- **Versions Supported**: CHD v1-5
- **Compression Support**: All CHD compression methods

### Integration Points

1. **VirtualFileSystem/DiskVirtualFileSystem.cs**
   - Recognizes `.chd` extension
   - Creates `ChdDiscReader` for validation

2. **VirtualFileSystem/ChdDiscReader.cs**
   - Wraps CHDSharp library
   - Validates CHD files
   - Placeholder for ISO extraction

3. **Modules/Kernel32Module.cs**
   - DeviceIoControl with CD-ROM IOCTL stubs
   - Logs when CD-ROM operations are attempted

## Testing

Tests are located in `Win32Emu.Tests.Kernel32/ChdDiscReaderTests.cs`:

```bash
# Run CHD-specific tests
dotnet test --filter "FullyQualifiedName~ChdDiscReaderTests"
```

Test coverage includes:
- ✅ Non-existent file handling
- ✅ Invalid CHD file detection
- ✅ VFS integration error handling

## Contributing

If you'd like to help implement full CHD support:

1. **Block Decompression**: Implement `ChdDiscReader.TryGetIsoFileSystem()`
   - Use CHDSharp's block reading APIs
   - Decompress blocks on-demand
   - Create a Stream wrapper for ISO filesystem

2. **Track Metadata**: Parse CHD metadata tags
   - Implement track list generation
   - Extract track offsets and types
   - Support CD-TEXT metadata

3. **Audio Support**: Decode and play audio tracks
   - FLAC decoding for audio tracks
   - PCM audio output
   - Integration with audio backend

## References

- [CHD Format Documentation](https://docs.mamedev.org/tools/chdman.html)
- [CHDSharp Library](https://github.com/RomVault/CHDSharp)
- [MAME chdman Tool](https://www.mamedev.org/)
- [Recalbox CHD Guide](https://wiki.recalbox.com/en/tutorials/utilities/rom-conversion/chdman)
