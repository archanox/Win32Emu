# GameDB Implementation Summary

## Overview

This implementation introduces a comprehensive GameDB system for Win32Emu that allows shipping a readonly game database with the emulator while enabling users to create their own overrides.

## What Was Implemented

### 1. Data Models

Created complete data models to represent game metadata:

- **`GameRating.cs`** - Represents ratings from ESRB, PEGI, USK, and ACB
- **`GameExecutable.cs`** - Represents an executable with MD5, SHA1, and SHA256 hashes
- **`GameDbEntry.cs`** - Complete game entry with all metadata fields
- **`GameDatabase.cs`** - Root database object containing version and games list
- Updated **`Game.cs`** - Added optional `GameDbId` field to link to database entries

### 2. Hash Computation Utilities

Created **`HashUtility.cs`** with static methods to compute file hashes:
- `ComputeMd5(string filePath)` - MD5 hash
- `ComputeSha1(string filePath)` - SHA-1 hash
- `ComputeSha256(string filePath)` - SHA-256 hash
- `ComputeAllHashes(string filePath)` - All three hashes in one pass

### 3. GameDB Service

Created **`GameDbService.cs`** implementing **`IGameDbService`** interface:

**Key Features:**
- Loads readonly `gamedb.json` from application directory
- Loads user overrides from `gamedb-overrides.json` in AppData
- User overrides take precedence over readonly database
- Identifies games by matching any hash (MD5, SHA1, or SHA256)
- Supports multiple executables per game
- Automatic database merging (readonly + overrides)

**Methods:**
- `FindGameByExecutable(string executablePath)` - Find game by hash matching
- `GetAllGames()` - Get merged list of all games
- `GetGameById(string id)` - Get specific game by ID
- `Reload()` - Reload databases from disk

### 4. GUI Integration

Updated the GUI to use GameDB when adding games:

- **`MainWindowViewModel.cs`** - Instantiates `GameDbService` and passes to view models
- **`GameLibraryViewModel.cs`** - Enhanced to automatically enrich games with GameDB metadata
- When a game is added (via file picker or folder scan), it's automatically matched against GameDB
- If found, the game's title, description, and metadata are enriched from the database

### 5. Example Database

Created **`gamedb.json`** with "Ignition" as example:
- Includes all 4 known executables (ign_win.exe, ign_dos.exe, ign_3dfx.exe, ign_teas.exe)
- Full metadata: developers, publishers, release date, description, ratings
- External URLs to IGDB, Wikidata, MobyGames, etc.
- Configured in `.csproj` to copy to output directory

### 6. Comprehensive Testing

Created **`Win32Emu.Tests.Gui`** test project:

**`HashUtilityTests.cs`** - 8 tests:
- Hash computation correctness (MD5, SHA1, SHA256)
- All hashes computation
- File not found exception handling
- Case insensitivity verification

**`GameDbServiceTests.cs`** - 10 tests:
- Finding games by executable (all hash types)
- User override precedence
- Database merging
- Get all games
- Get game by ID
- Null handling

**All 18 tests pass successfully.**

### 7. Documentation

Created comprehensive documentation:

**`GAMEDB.md`** - 7,891 characters covering:
- Architecture and file locations
- Complete schema documentation
- Field descriptions and examples
- Executable identification process
- Multiple executables per game
- User override creation
- Game submission process
- Data sources
- API usage examples
- Future enhancements

**`GAMEDB_STUB_TEMPLATE.md`** - 6,006 characters covering:
- Template for submitting games
- Example stub (Ignition)
- Hash computation instructions
- Data source descriptions
- CI/CD process overview

## Architecture Decisions

### Hash-Based Identification

Games are identified by file content hashes rather than filenames:
- Supports MD5, SHA1, and SHA256
- Any hash match identifies the game
- Allows executables to be renamed without losing metadata
- Multiple hashes provide redundancy

### User Override System

User overrides stored in AppData:
- Readonly database ships with emulator
- User can't accidentally corrupt the readonly database
- Overrides take precedence over readonly entries
- Same ID overrides, different ID extends

### Automatic Enrichment

Games are automatically enriched when added:
- Transparent to user
- No additional steps required
- Falls back gracefully if not found in database
- Optional - GameDbService can be null

## File Locations

### Readonly Database
- **Path**: `<app_directory>/gamedb.json`
- **Managed by**: Emulator developers
- **Updated**: With each release

### User Overrides
- **Windows**: `%APPDATA%/Win32Emu/gamedb-overrides.json`
- **macOS**: `~/Library/Application Support/Win32Emu/gamedb-overrides.json`
- **Linux**: `~/.config/Win32Emu/gamedb-overrides.json`
- **Managed by**: End users
- **Format**: Same as readonly database

## Future Enhancements (Out of Scope)

While the core infrastructure is complete, these features could be added later:

1. **CI/CD Integration**
   - Automatic scraping from Wikidata, IGDB, MobyGames, etc.
   - Stub file processing
   - Database validation and merging

2. **Logo/Artwork Download**
   - Automatic logo download from LogoUrl
   - Cache management
   - Thumbnail generation

3. **Advanced Metadata**
   - Save state compatibility information
   - Known issues and workarounds
   - Recommended emulator settings per game
   - Controller profiles per game

4. **Search and Filter**
   - Search games by title, developer, genre
   - Filter by rating, release date
   - Sort by various criteria

5. **Community Features**
   - Community ratings and reviews
   - User-submitted screenshots
   - Compatibility reports

## Testing Results

All tests pass successfully:
- **HashUtilityTests**: 8/8 passed
- **GameDbServiceTests**: 10/10 passed
- **Total**: 18/18 passed

Existing test suite also passes (with 1 pre-existing failure in Kernel32 unrelated to this change):
- Win32Emu.Tests.Kernel32: 105/106 passed (1 pre-existing failure)
- Win32Emu.Tests.User32: 53/53 passed
- Win32Emu.Tests.Emulator: 64/65 passed (1 skipped)
- Win32Emu.Tests.Gui: 18/18 passed

## Breaking Changes

None. This is a purely additive feature:
- All existing code continues to work
- GameDbService is optional in GameLibraryViewModel
- No changes to existing configuration files
- No changes to existing models (only addition of optional field)

## Performance Considerations

- **Hash Computation**: Only performed when adding games (not on every launch)
- **Database Loading**: Loaded once on startup
- **Memory Footprint**: Minimal - JSON is deserialized once
- **Disk I/O**: Two small JSON files read on startup

## Security Considerations

- **Hash Verification**: Hashes are for identification only, not security
- **User Overrides**: Users can modify their own overrides but not the readonly database
- **No Code Execution**: Database contains only metadata, no executable code
- **Input Validation**: JSON deserialization is safe with System.Text.Json

## Conclusion

The GameDB system is fully implemented with:
- ✅ Complete data models
- ✅ Hash computation utilities
- ✅ Service layer with readonly + override support
- ✅ GUI integration with automatic enrichment
- ✅ Example database (Ignition)
- ✅ Comprehensive testing (18 tests)
- ✅ Extensive documentation

The system is production-ready and provides a solid foundation for future enhancements like CI/CD integration, automatic scraping, and community features.
