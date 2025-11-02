# CHD Disc Image Support

## Overview

Win32Emu now includes **full support** for CHD (Compressed Hunks of Data) disc image files. CHD is a lossless compression format developed for MAME that is commonly used for CD-ROM based games with Redbook audio.

## Current Implementation Status

### ✅ Fully Implemented

- **CHD File Detection**: The emulator can recognize and validate CHD files
- **CHDSharp Integration**: Includes the CHDSharp library (MIT licensed) for CHD format support
- **File Format Validation**: Validates CHD file headers and versions (1-5)
- **Virtual File System Integration**: CHD files can be mounted and read like ISO files
- **Block Decompression**: On-demand decompression of CHD blocks with caching
- **ISO Filesystem Extraction**: Full support for reading ISO 9660 filesystems from CHD
- **Track Metadata Reading**: Parses CD-ROM track information from CHD metadata
- **Redbook Audio Support**: Can read and extract CD-DA audio tracks from CHD files
- **Multi-Track Support**: Full support for multi-track CD images with mixed data/audio

### ⏳ Pending Implementation

- **CD-ROM IOCTL Handlers**: DeviceIoControl handlers for advanced CD-ROM operations:
  - `IOCTL_CDROM_READ_TOC` (stub - requires handle tracking)
  - `IOCTL_CDROM_GET_LAST_SESSION` (stub - requires handle tracking)
  - `IOCTL_CDROM_RAW_READ` (stub - can be implemented via ChdDiscReader.ReadAudioTrack)

## Usage

### Mounting CHD Files

CHD files are automatically detected and mounted when passed to the `DiskVirtualFileSystem`:

```csharp
using Win32Emu.VirtualFileSystem;

// CHD files are mounted and can be read like ISO files
var vfs = new DiskVirtualFileSystem("game.chd", logger);

// CHD files are always read-only
Console.WriteLine($"Read-only: {vfs.IsReadOnly}"); // True

// Access files in the CHD just like any other disc image
if (vfs.FileExists("/setup.exe"))
{
    using var stream = vfs.OpenFile("/setup.exe", VfsFileMode.Open, VfsFileAccess.Read);
    // Read the file...
}
```

### Using ChdDiscReader Directly

For more control over CHD files, you can use `ChdDiscReader` directly:

```csharp
using Win32Emu.VirtualFileSystem;

// Open and validate a CHD file
using var reader = new ChdDiscReader("disc.chd", logger);

if (reader.IsValid)
{
    Console.WriteLine($"CHD Version: {reader.Version}");
    
    // Access track information
    if (reader.Toc != null)
    {
        Console.WriteLine($"Tracks: {reader.Toc.Tracks.Count}");
        foreach (var track in reader.Toc.Tracks)
        {
            Console.WriteLine($"  {track}");
        }
    }
    
    // Get the ISO filesystem
    var cdReader = reader.TryGetIsoFileSystem();
    if (cdReader != null)
    {
        // Read files from the ISO filesystem
        var files = cdReader.GetFiles("/");
        foreach (var file in files)
        {
            Console.WriteLine($"File: {file}");
        }
    }
    
    // Read audio track data
    if (reader.Toc?.Tracks.Any(t => t.TrackType == CdTrackType.Audio) == true)
    {
        var audioTrack = reader.Toc.Tracks.First(t => t.TrackType == CdTrackType.Audio);
        // Read 75 frames (1 second) of audio from the start of the track
        byte[]? audioData = reader.ReadAudioTrack(audioTrack.TrackNumber, 0, 75);
        if (audioData != null)
        {
            // Process raw CD-DA audio (16-bit stereo PCM at 44.1kHz)
            // Each frame is 2352 bytes
            Console.WriteLine($"Read {audioData.Length} bytes of audio");
        }
    }
}
```

### Accessing Raw CHD Data

You can also get a stream to read the raw decompressed data:

```csharp
using var reader = new ChdDiscReader("disc.chd", logger);
if (reader.IsValid)
{
    var dataStream = reader.GetDataStream();
    if (dataStream != null)
    {
        // Read raw decompressed data
        byte[] buffer = new byte[2048];
        dataStream.Seek(0, SeekOrigin.Begin);
        int bytesRead = dataStream.Read(buffer, 0, buffer.Length);
        Console.WriteLine($"Read {bytesRead} bytes");
    }
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

## Implementation Details

### Block Decompression

CHD files store data in compressed blocks. The implementation uses on-demand block decompression:

- **Streaming Access**: `ChdBlockStream` provides a `Stream` interface for reading decompressed data
- **Block Caching**: Frequently accessed blocks are cached to improve performance
- **Compression Support**: All CHD compression methods (zlib, lzma, huff, flac, avhuff) are supported
- **Self-Referencing Blocks**: Blocks that reference other blocks are handled efficiently

### Track Metadata

CD-ROM track information is extracted from CHD metadata:

- **Track Types**: Supports all CD-ROM track types (Mode1, Mode2, Audio, etc.)
- **Frame Information**: Each track includes start frame, frame count, and frame size
- **Pregap/Postgap**: Track gaps are properly parsed and stored
- **Table of Contents (TOC)**: Complete TOC is built from metadata

### Audio Track Support

Redbook CD-DA audio tracks can be read directly:

- **Format**: Raw CD-DA format (2352 bytes per frame)
- **Sample Rate**: 44.1 kHz stereo, 16-bit PCM
- **Frame-Based**: Audio is read in CD frames (1/75th of a second each)
- **Track Selection**: Audio can be read from specific tracks by track number

## Known Limitations

### DeviceIoControl Support

The following CD-ROM IOCTLs are recognized but return failure (require handle-to-CHD tracking):

- `IOCTL_CDROM_READ_TOC`: Read table of contents (track list)
  - *Workaround*: Use `ChdDiscReader.Toc` property directly
- `IOCTL_CDROM_GET_LAST_SESSION`: Get last session information  
  - *Workaround*: CHD files are single-session; use track count from TOC
- `IOCTL_CDROM_RAW_READ`: Read raw sectors from disc
  - *Workaround*: Use `ChdDiscReader.GetDataStream()` or `ReadAudioTrack()`

Implementing these would require tracking which file handles correspond to CHD devices and routing IOCTL calls to the appropriate `ChdDiscReader` instance.

## Future Enhancements

### Potential Improvements

- **DeviceIoControl Integration**: Map file handles to CHD readers for full IOCTL support
- **Audio Playback**: Integrate with audio backend for CD-DA playback during gameplay
- **Multi-Session Support**: Support for multi-session CD-ROMs (currently single-session only)
- **Subchannel Data**: Extract and provide access to CD subchannel information
- **Performance Optimization**: Further optimize block caching strategies

## Technical Details

### CHD Library

Win32Emu uses [CHDSharp](https://github.com/RomVault/CHDSharp) by RomVault:

- **License**: MIT
- **Language**: C# (.NET 9.0)
- **Versions Supported**: CHD v1-5
- **Compression Support**: All CHD compression methods

### Integration Points

1. **VirtualFileSystem/ChdBlockStream.cs**
   - Stream wrapper for on-demand block decompression
   - Handles all CHD compression types
   - Implements efficient block caching

2. **VirtualFileSystem/ChdDiscReader.cs**
   - Main CHD reader interface
   - Parses CHD headers and metadata
   - Extracts CD-ROM track information
   - Provides ISO filesystem access
   - Supports audio track reading

3. **VirtualFileSystem/CdTrackInfo.cs**
   - Data structures for CD-ROM track information
   - Table of Contents (TOC) representation
   - Track type and format definitions

4. **VirtualFileSystem/DiskVirtualFileSystem.cs**
   - Recognizes `.chd` extension
   - Mounts CHD files as virtual discs
   - Provides transparent file access

5. **Modules/Kernel32Module.cs**
   - DeviceIoControl with CD-ROM IOCTL stubs
   - Logs when CD-ROM operations are attempted

### CHD File Structure

CHD files consist of:

1. **Header**: Version, compression info, block size, total blocks
2. **Block Map**: Offset and compression type for each block
3. **Compressed Blocks**: Data compressed with various codecs
4. **Metadata**: CD-ROM track info, SHA1/MD5 hashes, etc.

The implementation reads the header, builds the block map, and decompresses blocks on-demand as they're accessed.

## Testing

Tests are located in `Win32Emu.Tests.Kernel32/ChdDiscReaderTests.cs`:

```bash
# Run CHD-specific tests
dotnet test --filter "FullyQualifiedName~ChdDiscReaderTests"
```

Test coverage includes:
- ✅ Non-existent file handling
- ✅ Invalid CHD file detection
- ✅ VFS integration with full block decompression
- ✅ ISO filesystem extraction
- ✅ Track metadata parsing

## Contributing

If you'd like to help improve CHD support:

1. **DeviceIoControl Implementation**: Implement handle tracking to route IOCTL calls to CHD readers
   - Track file handles opened for CHD-backed devices
   - Route IOCTL_CDROM_* calls to appropriate `ChdDiscReader` instance
   - Implement TOC response structures for Windows API compatibility

2. **Audio Backend Integration**: Connect audio track reading to audio playback
   - Integrate `ReadAudioTrack()` with audio subsystem
   - Implement CD-DA streaming during gameplay
   - Support seamless track transitions

3. **Performance Optimization**: Improve block caching strategies
   - Implement LRU cache for blocks
   - Pre-fetch adjacent blocks for sequential reads
   - Memory-mapped file support for better performance

4. **Multi-Session Support**: Add support for multi-session CDs
   - Parse session information from metadata
   - Handle session boundaries correctly
   - Update TOC generation for multi-session discs

## References

- [CHD Format Documentation](https://docs.mamedev.org/tools/chdman.html)
- [CHDSharp Library](https://github.com/RomVault/CHDSharp)
- [MAME chdman Tool](https://www.mamedev.org/)
- [Recalbox CHD Guide](https://wiki.recalbox.com/en/tutorials/utilities/rom-conversion/chdman)
