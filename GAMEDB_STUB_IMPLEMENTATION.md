# Game Database Stub System Implementation

## Overview

This implementation adds support for community-contributed game database entries through a stub system. Users can submit JSON stub files via pull requests, and the CI/CD pipeline will automatically process and merge them into the main game database.

## Files Added

### `/stubs/` Directory
- **Purpose**: Contains user-submitted game database stub files
- **`.gitkeep`**: Ensures the directory is tracked in git
- **`README.md`**: Documentation for users on how to submit stubs
- **`example-game.json`**: Example stub showing the correct format

### CI/CD Workflow Changes
- **`.github/workflows/ci.yml`**: Added `process-stubs` job that:
  - Detects stub files in the `stubs/` directory
  - Validates JSON syntax
  - Checks for required fields (Title and at least one Executable with a hash)
  - Auto-generates UUIDs for Id field if not provided or invalid
  - Sets DataSource to "user_submitted" if not specified
  - Merges stubs into `Win32Emu.Gui/gamedb.json`
  - Removes processed stub files
  - Commits changes back to the repository

### Documentation Updates
- **`Win32Emu.Gui/GAMEDB_STUB_TEMPLATE.md`**: Updated to:
  - Use correct field names (GenreIds, DeveloperIds, PublisherIds instead of Genres, Developers, Publishers)
  - Match the actual GameDbEntry model schema
  - Reflect the minimal implementation (no automatic scraping yet)
  - Clarify that genre/developer/publisher IDs can be left empty

## How It Works

1. **User Submission**:
   - User creates a JSON file following the template in `Win32Emu.Gui/GAMEDB_STUB_TEMPLATE.md`
   - User submits a pull request with the stub file in the `stubs/` directory
   - Pull request is reviewed and merged to main or develop branch

2. **CI/CD Processing** (runs only on push to main/develop):
   - Workflow checks for `.json` files in `stubs/` directory
   - Each stub is validated:
     - JSON syntax must be valid
     - Must have a `Title` field
     - Must have at least one `Executable` with a hash (Md5, Sha1, or Sha256)
   - Valid stubs are processed:
     - UUID is auto-generated if Id is missing or invalid
     - DataSource is set to "user_submitted" if not provided
     - Stub is merged into the Games array in `gamedb.json`
     - Duplicate IDs are skipped
   - Processed stub files are removed from `stubs/` directory
   - Changes are committed and pushed back to the repository

3. **Result**:
   - Updated `gamedb.json` is deployed with the application
   - Game metadata is available to all users

## Schema

The stub must match the `GameDbEntry` model structure:

```json
{
  "Id": "UUID (auto-generated if not provided)",
  "WikidataKey": "Optional Wikidata key for reference",
  "Title": "REQUIRED - Game title",
  "GenreIds": ["Optional array of genre UUIDs"],
  "DeveloperIds": ["Optional array of developer UUIDs"],
  "PublisherIds": ["Optional array of publisher UUIDs"],
  "ReleaseDate": "Optional ISO 8601 date",
  "Description": "Optional game description",
  "Languages": ["Optional array of ISO 639 language codes"],
  "LogoUrl": "Optional logo URL",
  "Ratings": {
    "Esrb": "Optional ESRB rating",
    "Pegi": "Optional PEGI rating",
    "Usk": "Optional USK rating",
    "Acb": "Optional ACB rating"
  },
  "Executables": [
    {
      "Name": "REQUIRED - Executable filename",
      "Md5": "Optional MD5 hash",
      "Sha1": "Optional SHA-1 hash",
      "Sha256": "Optional SHA-256 hash"
    }
  ],
  "ExternalUrls": {
    "IGDB": "Optional IGDB URL",
    "Wikidata": "Optional Wikidata URL",
    ...
  },
  "DataSource": "Optional - defaults to 'user_submitted'"
}
```

## Future Enhancements

The current implementation is minimal and functional. Future enhancements could include:

1. **Automatic Metadata Scraping**:
   - Scrape Wikidata using WikidataKey
   - Fetch additional metadata from IGDB, MobyGames, etc.
   - Auto-populate missing fields

2. **Genre/Developer/Publisher Management**:
   - Create separate tables for genres, developers, and publishers
   - Auto-generate IDs and maintain relationships
   - Allow string submissions and convert to IDs automatically

3. **Enhanced Validation**:
   - Verify hash formats
   - Check for duplicate games by hash
   - Validate URLs

4. **Testing**:
   - Add unit tests for stub validation logic
   - Add integration tests for the merge process

## Wikidata Integration

The stub processing now includes automatic enrichment from Wikidata:

### What Gets Enriched

When a stub includes a `WikidataKey` field (e.g., "Q2411602"), the system automatically fetches and populates:

- **Title** - Game title (if not already provided)
- **Description** - Short game description (if not already provided)
- **ReleaseDate** - Publication date in ISO 8601 format (if not already provided)
- **Languages** - ISO 639-1 language codes (if not already provided or empty)
- **ExternalUrls.Wikidata** - Link to the Wikidata page

### What Gets Logged

The following information is fetched and logged but not automatically mapped (requires manual review or future enhancement):

- **Genres** - Game genres (Wikidata property P136)
- **Developers** - Game developers (Wikidata property P178)
- **Publishers** - Game publishers (Wikidata property P123)

These are logged because they would need to be mapped to UUIDs in the database, which requires a proper genre/developer/publisher management system.

### How It Works

The enrichment is performed by the Python script `.github/scripts/enrich_from_wikidata.py` which:

1. Checks if the stub has a `WikidataKey` field
2. Calls the Wikidata MediaWiki API to fetch entity data
3. Extracts relevant properties (P136, P178, P123, P577, P407)
4. Enriches only the fields that are missing in the original stub
5. Preserves any user-provided values

### Supported Languages

The script includes mappings for common languages including: English, French, German, Spanish, Italian, Portuguese, Dutch, Swedish, Czech, Polish, Chinese, Japanese, Korean, and Russian.

## Testing

The implementation has been tested locally:
- JSON validation works correctly
- Required field checking works
- UUID generation works
- Stub merging into gamedb.json works
- Duplicate detection works

The full CI/CD workflow will be tested when:
- A stub file is submitted via pull request
- The pull request is merged to main or develop branch
- The workflow runs and processes the stub

## Notes

- The workflow only runs on pushes to main or develop branches to avoid processing stubs on every PR
- Stubs are removed after processing to keep the directory clean
- The workflow uses `github-actions[bot]` as the committer for automated commits
- Existing build errors in `Win32Emu.Gui` are unrelated to this implementation
