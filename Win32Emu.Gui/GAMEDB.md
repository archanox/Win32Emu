# GameDB Documentation

## Overview

The GameDB system provides a readonly game database that ships with the Win32Emu emulator. It contains metadata about games including executables, ratings, developers, and more. Users can create their own overrides in a separate file if they want to customize or add entries.

## Architecture

The GameDB system consists of:

1. **Readonly Database** (`gamedb.json`) - Ships with the emulator in the application directory
2. **User Overrides** (`gamedb-overrides.json`) - User-editable file in the AppData folder
3. **GameDbService** - Service that loads and queries both databases, with user overrides taking precedence

## File Locations

- **Readonly Database**: `<app_directory>/gamedb.json`
- **User Overrides**: 
  - Windows: `%APPDATA%/Win32Emu/gamedb-overrides.json`
  - macOS: `~/Library/Application Support/Win32Emu/gamedb-overrides.json`
  - Linux: `~/.config/Win32Emu/gamedb-overrides.json`

## Schema

### GameDatabase (Root Object)

```json
{
  "Version": "1.0",
  "Games": [ /* array of GameDbEntry objects */ ]
}
```

### GameDbEntry

A complete game entry with all metadata:

```json
{
  "Id": "Q2411602",
  "WikidataKey": "Q2411602",
  "Title": "Ignition",
  "Genres": ["Racing"],
  "Developers": ["Unique Development Studios"],
  "Publishers": ["Virgin Interactive", "UDS"],
  "ReleaseDate": "1997-03-31T00:00:00",
  "Description": "Game description here",
  "Languages": ["en"],
  "LogoUrl": "https://example.com/logo.png",
  "Ratings": {
    "Esrb": "E",
    "Pegi": "3",
    "Usk": "0",
    "Acb": "G"
  },
  "Executables": [
    {
      "Name": "game.exe",
      "Md5": "42aeaf49af6191400fa18ba3e3c47e48",
      "Sha1": "eda557a84013bcf42100c3dd43e40263cb8d3353",
      "Sha256": "52b0c3a95cc70eb909b46d5f872a6779eb228b1925274c9da072463934ff2099"
    }
  ],
  "ExternalUrls": {
    "IGDB": "https://www.igdb.com/games/...",
    "Wikidata": "https://www.wikidata.org/wiki/..."
  },
  "DataSource": "user_submitted"
}
```

### Field Descriptions

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Id` | string | Yes | Unique identifier (typically Wikidata ID) |
| `WikidataKey` | string | No | Wikidata key for scraping (e.g., "Q2411602") |
| `Title` | string | Yes | Official game title |
| `Genres` | string[] | No | Game genres |
| `Developers` | string[] | No | Game developers |
| `Publishers` | string[] | No | Game publishers |
| `ReleaseDate` | DateTime | No | Release date (ISO 8601 format) |
| `Description` | string | No | Game description/synopsis |
| `Languages` | string[] | No | ISO 639 language codes (from Wikidata P407) |
| `LogoUrl` | string | No | URL to logo with transparency |
| `Ratings` | GameRating | No | Game ratings object |
| `Executables` | GameExecutable[] | Yes | Known executables for this game |
| `ExternalUrls` | Dictionary | No | Links to external databases |
| `DataSource` | string | No | Source of data (e.g., "wikidata", "igdb", "user_submitted") |

### GameRating

```json
{
  "Esrb": "E",     // ESRB: EC, E, E10+, T, M, AO, RP
  "Pegi": "3",     // PEGI: 3, 7, 12, 16, 18
  "Usk": "0",      // USK: 0, 6, 12, 16, 18
  "Acb": "G"       // ACB: G, PG, M, MA15+, R18+, RC
}
```

### GameExecutable

Represents a known executable file with hashes for identification:

```json
{
  "Name": "ign_teas.exe",
  "Md5": "42aeaf49af6191400fa18ba3e3c47e48",
  "Sha1": "eda557a84013bcf42100c3dd43e40263cb8d3353",
  "Sha256": "52b0c3a95cc70eb909b46d5f872a6779eb228b1925274c9da072463934ff2099"
}
```

**Notes:**
- At least one hash should be provided (MD5, SHA1, or SHA256)
- Multiple hashes improve matching reliability
- Hashes are case-insensitive and should be lowercase hexadecimal

## Executable Identification

The GameDB identifies executables using file content hashes rather than file names, as users may rename executables. The matching process:

1. When a game executable is added, all three hashes (MD5, SHA1, SHA256) are computed
2. These hashes are matched against the GameDB entries
3. A match on **any** hash (MD5, SHA1, or SHA256) identifies the game
4. User overrides are checked first, then the readonly database

## Multiple Executables Per Game

Games can have multiple executables. For example, "Ignition" has:
- `ign_win.exe` - Windows version
- `ign_dos.exe` - DOS version
- `ign_3dfx.exe` - 3dfx Glide version
- `ign_teas.exe` - Tease/demo version

Each executable has its own hashes but all belong to the same game entry.

## Creating User Overrides

To create custom entries or override existing ones:

1. Create or edit `gamedb-overrides.json` in your AppData folder
2. Use the same schema as the readonly database
3. Entries with matching IDs will override readonly database entries
4. New IDs will add new games to the database

Example override file:

```json
{
  "Version": "1.0",
  "Games": [
    {
      "Id": "custom-game-001",
      "Title": "My Custom Game",
      "Executables": [
        {
          "Name": "mygame.exe",
          "Sha256": "abc123..."
        }
      ]
    }
  ]
}
```

## Submitting Games to the Database

Users can submit games to be included in the official database:

### Stub Format

Create a minimal stub with this information:
- Wikidata key (to enable automatic scraping)
- Game title
- Genre
- Developer
- Publisher
- Release date
- Description
- Languages (from Wikidata Property P407)
- Logo URL (transparent PNG recommended)
- Ratings (ESRB, PEGI, USK, ACB)
- Executable hashes (MD5, SHA1, SHA256)

### Automatic Data Population

Once a stub is submitted:
1. CI/CD pipeline scrapes metadata from sources
2. Missing fields are filled in from:
   - Wikidata
   - IGDB
   - MobyGames
   - TheGamesDB
   - LaunchBox
   - GameFAQs
3. Data source is recorded for each field
4. Updated entry is added to gamedb.json

## Data Sources

The GameDB can aggregate data from multiple sources:

- **Wikidata** - Free, open metadata (https://www.wikidata.org/)
- **IGDB** - Internet Game Database (https://www.igdb.com/)
- **MobyGames** - Game database (https://www.mobygames.com/)
- **TheGamesDB** - Community game database (https://thegamesdb.net/)
- **LaunchBox** - Frontend database (https://gamesdb.launchbox-app.com/)
- **GameFAQs** - Game guides and info (https://gamefaqs.gamespot.com/)

**Note:** Not all games exist in all databases.

## API Usage

### Finding a Game by Executable

```csharp
var gameDbService = new GameDbService();
var entry = gameDbService.FindGameByExecutable(@"C:\Games\ign_teas.exe");

if (entry != null)
{
    Console.WriteLine($"Found: {entry.Title}");
    Console.WriteLine($"Developer: {string.Join(", ", entry.Developers)}");
    Console.WriteLine($"Released: {entry.ReleaseDate}");
}
```

### Getting All Games

```csharp
var gameDbService = new GameDbService();
var allGames = gameDbService.GetAllGames();

foreach (var game in allGames)
{
    Console.WriteLine($"{game.Title} - {string.Join(", ", game.Developers)}");
}
```

### Getting a Game by ID

```csharp
var gameDbService = new GameDbService();
var entry = gameDbService.GetGameById("Q2411602");

if (entry != null)
{
    Console.WriteLine($"Title: {entry.Title}");
}
```

## Integration with Game Library

The `Game` model has an optional `GameDbId` field that can reference a GameDB entry:

```csharp
var game = new Game
{
    Title = "Ignition",
    ExecutablePath = @"C:\Games\ign_teas.exe",
    GameDbId = "Q2411602" // Links to GameDB entry
};
```

This allows the application to:
- Display rich metadata from GameDB
- Show game logos and artwork
- Display ratings and release information
- Link to external databases

## Future Enhancements

Potential future additions to the GameDB system:
- Automatic download of game logos
- Cover art and screenshots
- Save state compatibility information
- Known issues and workarounds per game
- Recommended emulator settings per game
- Community ratings and reviews
