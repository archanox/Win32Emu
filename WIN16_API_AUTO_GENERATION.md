# Win16 API Status Auto-Generation

## Date
December 4, 2025

## Problem
The Win16 function support list in the PE Analyzer was hardcoded with ~400 functions, making it unmaintainable. When Win16 functions were added or modified in the thunking layers, the hardcoded list had to be manually updated.

## Solution
Replaced the hardcoded function list with an auto-generated JSON file that is extracted from the Win16 module source files.

## Implementation

### 1. Created Generation Script
**File**: `generate-win16-api-status.py`

A Python script that:
- Parses all Win16 module C# files in `Win32Emu/Win32/Win16/`
- Uses regex to extract the module name from `public string Name => "..."` property
- Extracts all case statements from the `TryInvokeWin16` switch statements
- Generates `Win32Emu.Tools.PeAnalyzer.Wasm/wwwroot/win16-api-status.json`

### 2. Generated JSON File
**File**: `Win32Emu.Tools.PeAnalyzer.Wasm/wwwroot/win16-api-status.json`

Contains 325 Win16 functions across 6 modules:
- KERNEL.DLL: 59 functions
- USER.DLL: 123 functions
- GDI.DLL: 117 functions
- KEYBOARD.DLL: 9 functions
- SYSTEM.DLL: 8 functions
- SOUND.DLL: 9 functions

Format:
```json
{
  "modules": [
    {
      "name": "KERNEL.DLL",
      "functions": [
        "GLOBALALLOC",
        "GLOBALFREE",
        ...
      ]
    },
    ...
  ]
}
```

### 3. Updated PeAnalyzer
**File**: `Win32Emu.Tools.PeAnalyzer.Wasm/Pages/PeAnalyzer.razor`

Changes:
- Added `win16ApiStatus` field to store loaded Win16 API data
- Load `win16-api-status.json` in `OnInitializedAsync()`
- Simplified `GetWin16SupportedModules()` to convert JSON data to dictionary
- Added `Win16ApiStatusData` and `Win16ModuleInfo` classes for deserialization

### 4. Updated Documentation
**File**: `Win32Emu.Tools.PeAnalyzer.Wasm/README.md`

Added section explaining how to regenerate the JSON file:
```bash
python3 generate-win16-api-status.py
```

## Benefits

1. **Maintainable**: Function list is always in sync with source code
2. **Automatic**: No manual updates required when Win16 functions change
3. **Accurate**: Extracts actual case statements from switch blocks
4. **Simple**: One command to regenerate the entire list
5. **Version Control**: JSON file is committed, so changes are tracked

## Usage

When adding or modifying Win16 functions:

1. Update the Win16 module source files (e.g., `Win16KernelModule.cs`)
2. Run regeneration script:
   ```bash
   cd /path/to/Win32Emu
   python3 generate-win16-api-status.py
   ```
3. Verify the updated JSON file
4. Commit both the source changes and updated JSON file

## Technical Details

### Regex Pattern
The script uses this pattern to match each module class:
```python
class_pattern = (
    r'internal class (Win16\w+Module).*?'  # Class name
    r'public string Name => "([^"]+)".*?'  # Module name from property
    r'public override bool TryInvokeWin16\(.*?\)\s*\{(.*?)'  # Method body
    r'(?=internal class|$)'  # Stop at next class or end of file
)
```

Then extracts case statements:
```python
case_pattern = r'case\s+"([^"]+)":'
functions = set(re.findall(case_pattern, method_body))
```

### Handling Multiple Classes Per File
The script correctly handles files like `Win16AuxiliaryModules.cs` that contain multiple module classes (KEYBOARD, SYSTEM, SOUND) by using `re.finditer()` to match all classes.

## Comparison

**Before** (Hardcoded):
- 167 lines of hardcoded function strings
- Manual updates required
- Easy to get out of sync with source
- Difficult to maintain

**After** (Auto-Generated):
- 20 lines of code to load and convert JSON
- Automatic extraction from source
- Always in sync with implementation
- Easy to maintain

## Files Changed

- `generate-win16-api-status.py` (NEW) - Generation script
- `Win32Emu.Tools.PeAnalyzer.Wasm/wwwroot/win16-api-status.json` (NEW) - Generated data
- `Win32Emu.Tools.PeAnalyzer.Wasm/Pages/PeAnalyzer.razor` - Load JSON instead of hardcoding
- `Win32Emu.Tools.PeAnalyzer.Wasm/README.md` - Document regeneration process

## Future Enhancements

Potential improvements:
1. Integrate script into build process to auto-regenerate on Win16 module changes
2. Add validation to ensure all modules are found
3. Extract additional metadata (comments, parameter counts, etc.)
4. Create a similar approach for Win32 modules if beneficial
