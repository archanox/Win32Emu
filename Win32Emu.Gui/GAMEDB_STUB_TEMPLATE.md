# Game Database Stub Template

This file contains a minimal template for submitting a game to the Win32Emu game database.

## How to Submit a Game

1. Copy this template
2. Fill in the required fields (marked with `*`)
3. Fill in as many optional fields as possible
4. Submit a pull request with your stub file to the `stubs/` directory
5. CI/CD will automatically enrich the entry with scraped data

## Stub Template

```json
{
  "Id": "*REQUIRED - Wikidata ID (e.g., Q2411602) or unique identifier",
  "WikidataKey": "Wikidata key for automatic data scraping (e.g., Q2411602)",
  "Title": "*REQUIRED - Official game title",
  "Genres": [
    "Optional - List of genres"
  ],
  "Developers": [
    "Optional - List of developers"
  ],
  "Publishers": [
    "Optional - List of publishers"
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
  "DataSource": "user_submitted"
}
```

## Example: Ignition

Here's a complete example for the game "Ignition":

```json
{
  "Id": "Q2411602",
  "WikidataKey": "Q2411602",
  "Title": "Ignition",
  "Genres": ["Racing"],
  "Developers": ["Unique Development Studios"],
  "Publishers": ["Virgin Interactive", "UDS"],
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

## Data Sources

The CI/CD pipeline will attempt to scrape additional metadata from:

1. **Wikidata** (https://www.wikidata.org/) - Primary source for structured data
   - Properties used:
     - P136: genre
     - P178: developer
     - P123: publisher
     - P577: publication date
     - P407: language of work
     - And many more...

2. **IGDB** (https://www.igdb.com/) - Internet Game Database
   - Requires API key
   - Provides comprehensive game data

3. **MobyGames** (https://www.mobygames.com/) - Community-driven game database
   - Historical game information
   - Screenshots and covers

4. **TheGamesDB** (https://thegamesdb.net/) - Community game database
   - Game metadata and artwork

5. **LaunchBox** (https://gamesdb.launchbox-app.com/) - Frontend database
   - High-quality logos and artwork

6. **GameFAQs** (https://gamefaqs.gamespot.com/) - Game guides and info
   - Platform information and reviews

## Notes

- **Wikidata Key**: If you provide a Wikidata key, many fields can be auto-populated
- **Multiple Executables**: Games can have multiple executables (e.g., different versions, patches)
- **Hash Accuracy**: At least one hash is required, but providing all three (MD5, SHA1, SHA256) is recommended
- **Ratings**: Ratings are optional but helpful for content filtering
- **Logo URL**: Prefer transparent PNG images for logos
- **External URLs**: These help with cross-referencing and verification

## CI/CD Process

Once your stub is merged:

1. CI/CD detects the new stub file
2. Scrapes data from Wikidata (if key provided)
3. Attempts to scrape from other sources
4. Merges scraped data with your stub
5. Validates the complete entry
6. Adds to the main `gamedb.json`
7. Deploys updated database with next release

## Questions?

If you have questions about submitting games, please:
- Open an issue in the repository
- Check existing stubs for examples
- Review the GAMEDB.md documentation
