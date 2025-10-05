# Game Database Stubs

This directory contains game database stub files submitted by users.

## How to Submit a Game

1. Copy the template from `Win32Emu.Gui/GAMEDB_STUB_TEMPLATE.md`
2. Fill in the required fields (marked with `*`)
3. Fill in as many optional fields as possible
4. Save your stub as a JSON file in this directory (e.g., `my-game.json`)
5. Submit a pull request
6. CI/CD will automatically validate and merge your stub into `gamedb.json`

## File Naming

- Use descriptive names for your stub files (e.g., `ignition.json`, `doom.json`)
- Only `.json` files in this directory will be processed
- File names should be lowercase and use hyphens instead of spaces

## Validation

The CI/CD pipeline will:
- Validate JSON syntax
- Check for required fields (Title, at least one Executable with hash)
- **Enrich from Wikidata** if WikidataKey is provided (automatically fetches Title, Description, ReleaseDate, Languages)
- Merge stub data into the main `gamedb.json`
- Set DataSource to "user_submitted" if not specified

## Questions?

See `Win32Emu.Gui/GAMEDB_STUB_TEMPLATE.md` and `Win32Emu.Gui/GAMEDB.md` for more information.
