# Game Database Stub Template

This file contains a minimal template for submitting a game to the Win32Emu game database.

## How to Submit a Game

1. Copy this template
2. Fill in the required fields (marked with `*`)
3. Fill in as many optional fields as possible
4. Submit a pull request with your stub file to the `stubs/` directory
5. CI/CD will automatically enrich the entry with scraped data from Wikidata

## Stub Template

```json
{
  "Id": "*REQUIRED - UUID in the format xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx (will be auto-generated if not provided or invalid)",
  "WikidataKey": "Optional - Wikidata key for reference (e.g., Q2411602)",
  "Title": "*REQUIRED - Official game title",
  "GenreIds": [
    "Optional - List of genre UUIDs (leave empty if unknown, can be populated later)"
  ],
  "DeveloperIds": [
    "Optional - List of developer UUIDs (leave empty if unknown, can be populated later)"
  ],
  "PublisherIds": [
    "Optional - List of publisher UUIDs (leave empty if unknown, can be populated later)"
  ],
  "ReleaseDate": "Optional - ISO 8601 date format (e.g., 1997-03-31T00:00:00)",
  "Description": "Optional - Game description or synopsis",
  "Languages": [
    "Optional - ISO 639 language codes (e.g., en, fr, de)"
  ],
  "LogoUrl": "Optional - URL to game logo with transparency (PNG preferred)",
  "Ratings": {
    "Esrb": "Optional - ESRB rating (EC, E, E10+, T, M, AO, RP)",
    "Pegi": "Optional - PEGI rating (3, 7, 12, 16, 18)",
    "Usk": "Optional - USK rating (0, 6, 12, 16, 18)",
    "Acb": "Optional - ACB rating (G, PG, M, MA15+, R18+, RC)"
  },
  "Executables": [
    {
      "Name": "*REQUIRED - Executable filename (e.g., game.exe)",
      "Md5": "*REQUIRED - At least one hash must be provided",
      "Sha1": "Optional - SHA-1 hash of the executable",
      "Sha256": "Optional - SHA-256 hash of the executable"
    }
  ],
  "ExternalUrls": {
    "IGDB": "Optional - https://www.igdb.com/games/...",
    "Wikidata": "Optional - https://www.wikidata.org/wiki/...",
    "MobyGames": "Optional - https://www.mobygames.com/game/...",
    "TheGamesDB": "Optional - https://thegamesdb.net/game.php?id=...",
    "LaunchBox": "Optional - https://gamesdb.launchbox-app.com/games/details/...",
    "GameFAQs": "Optional - https://gamefaqs.gamespot.com/..."
  },
  "DataSource": "Optional - Source of data (defaults to 'user_submitted' if not provided)"
}
```

## Example: Ignition

Here's a complete example for the game "Ignition":

```json
{
  "Id": "e7c3f2a1-8b4d-4e6f-9a2c-1d5b8e4a7c9f",
  "WikidataKey": "Q2411602",
  "Title": "Ignition",
  "GenreIds": [
    "a1b2c3d4-e5f6-4789-a0b1-c2d3e4f5a6b7"
  ],
  "DeveloperIds": [
    "d4e5f6a7-b8c9-4012-d3e4-f5a6b7c8d9e0"
  ],
  "PublisherIds": [
    "e5f6a7b8-c9d0-4123-e4f5-a6b7c8d9e0f1",
    "f6a7b8c9-d0e1-4234-f5a6-b7c8d9e0f1a2"
  ],
  "ReleaseDate": "1997-03-31T00:00:00",
  "Description": "Ignition is a racing game developed by Unique Development Studios and published by Virgin Interactive for PC, Sega Saturn and PlayStation in 1997. The game features overhead-view racing with power-ups and weapons.",
  "Languages": ["en"],
  "LogoUrl": "https://images.launchbox-app.com//90a26b92-bc4d-4862-9b07-dc36b88536e0.png",
  "Ratings": {
    "Esrb": "E",
    "Pegi": null,
    "Usk": null,
    "Acb": null
  },
  "Executables": [
    {
      "Name": "ign_teas.exe",
      "Md5": "42aeaf49af6191400fa18ba3e3c47e48",
      "Sha1": "eda557a84013bcf42100c3dd43e40263cb8d3353",
      "Sha256": "52b0c3a95cc70eb909b46d5f872a6779eb228b1925274c9da072463934ff2099"
    }
  ],
  "ExternalUrls": {
    "IGDB": "https://www.igdb.com/games/ignition",
    "Wikidata": "https://www.wikidata.org/wiki/Q2411602",
    "MobyGames": "https://www.mobygames.com/game/807/ignition/",
    "TheGamesDB": "https://thegamesdb.net/game.php?id=16400",
    "LaunchBox": "https://gamesdb.launchbox-app.com/games/details/168516-ignition",
    "GameFAQs": "https://gamefaqs.gamespot.com/pc/197611-ignition"
  },
  "DataSource": "user_submitted"
}
```

## How to Compute Hashes

You can compute hashes using various tools:

### PowerShell (Windows)
```powershell
# MD5
Get-FileHash -Algorithm MD5 "path\to\game.exe"

# SHA-1
Get-FileHash -Algorithm SHA1 "path\to\game.exe"

# SHA-256
Get-FileHash -Algorithm SHA256 "path\to\game.exe"
```

### Linux/macOS
```bash
# MD5
md5sum game.exe

# SHA-1
sha1sum game.exe

# SHA-256
sha256sum game.exe
```

### Online Tools
- Use reputable online hash calculators (be cautious about uploading executables to third-party sites)

## Notes

- **UUID Format**: The Id field should be a valid UUID. If not provided or invalid, the CI/CD pipeline will auto-generate one
- **WikidataKey**: If provided, the CI/CD pipeline will automatically fetch and populate missing fields (Title, Description, ReleaseDate, Languages) from Wikidata
- **Multiple Executables**: Games can have multiple executables (e.g., different versions, patches)
- **Hash Accuracy**: At least one hash is required, but providing all three (MD5, SHA1, SHA256) is recommended
- **Ratings**: Ratings are optional but helpful for content filtering
- **Logo URL**: Prefer transparent PNG images for logos
- **External URLs**: These help with cross-referencing and verification
- **Genre/Developer/Publisher IDs**: Leave these arrays empty if you don't know the IDs. They can be populated later

## CI/CD Process

Once your stub is merged to the main or develop branch:

1. CI/CD detects the new stub file in the `stubs/` directory
2. Validates the JSON syntax
3. Checks for required fields (Title and at least one Executable with a hash)
4. **Enriches from Wikidata** (if WikidataKey is provided):
   - Fetches Title (if not provided)
   - Fetches Description (if not provided)
   - Fetches ReleaseDate (if not provided)
   - Fetches Languages (if not provided or empty)
   - Adds Wikidata URL to ExternalUrls
   - Logs genre, developer, and publisher information (not auto-mapped to IDs yet)
5. Generates a UUID for the Id field if not provided or invalid
6. Sets DataSource to "user_submitted" if not specified
7. Merges the stub into the main `gamedb.json`
8. Removes the stub file from the `stubs/` directory
9. Commits the updated `gamedb.json` back to the repository

**Note**: The Wikidata integration currently enriches Title, Description, ReleaseDate, and Languages. Genre/Developer/Publisher information is logged but not automatically mapped to UUIDs (this is a future enhancement).

## Questions?

If you have questions about submitting games, please:
- Open an issue in the repository
- Check existing stubs for examples
- Review the GAMEDB.md documentation
