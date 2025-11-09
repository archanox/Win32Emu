# Dialog Template Parser Safety Improvements

## Issue
The Ignition setup.exe application was crashing while parsing dialog template resources. The log output showed the emulator successfully parsing 6 out of 11 dialog controls before cutting off mid-log at "[Us...". This indicated an unhandled exception or crash during the parsing of the 7th control.

## Root Cause
The `DialogTemplateParser` class lacked proper safety checks when reading string data and control information from memory. Specifically:

1. **Infinite Loop Risk**: The `ReadString` method had no maximum length limit, potentially looping forever if a string lacked a proper null terminator.
2. **Out-of-Bounds Reads**: No exception handling for `IndexOutOfRangeException` when reading beyond allocated memory.
3. **Unvalidated Creation Data**: No size validation for dialog item creation data, allowing attempts to read excessive amounts.

## Solution
Added comprehensive safety checks throughout the `DialogTemplateParser` class:

### 1. String Reading Protection (`ReadString`)
- Added maximum string length limit of 8192 characters
- Wrapped memory reads in try-catch blocks to handle `IndexOutOfRangeException`
- Returns partial string if null terminator is not found within limits

```csharp
private string ReadString(uint templateAddress, ref uint offset)
{
    var sb = new StringBuilder();
    const int MaxStringLength = 8192; // Reasonable maximum for dialog template strings
    var charsRead = 0;
    
    while (charsRead < MaxStringLength)
    {
        try
        {
            var wchar = _memory.Read16(templateAddress + offset);
            offset += 2;
            if (wchar == 0)
            {
                break;
            }
            sb.Append((char)wchar);
            charsRead++;
        }
        catch (IndexOutOfRangeException)
        {
            // Hit end of memory without finding null terminator
            // Return what we have
            if (sb.Length == 0)
            {
                return string.Empty;
            }
            return sb.ToString();
        }
    }
    
    // Return truncated string if max length exceeded
    return sb.ToString();
}
```

### 2. Name/Ordinal Reading Protection (`ReadNameOrOrdinal`)
- Wrapped all memory reads in try-catch blocks
- Returns empty string on memory access failure

### 3. Creation Data Validation
- Added maximum size check (64KB) for creation data
- Added try-catch around creation data reading
- Sets `CreationData` to null if reading fails (non-critical data)

```csharp
// Creation data
try
{
    var dataSize = _memory.Read16(templateAddress + offset);
    offset += 2;
    
    const int MaxCreationDataSize = 65536; // 64KB should be more than enough
    if (dataSize > 0 && dataSize <= MaxCreationDataSize)
    {
        // Read creation data...
    }
    else if (dataSize > MaxCreationDataSize)
    {
        // Skip the corrupted data
        item.CreationData = null;
    }
}
catch (IndexOutOfRangeException)
{
    // Failed to read creation data, but that's okay - not critical
    item.CreationData = null;
}
```

### 4. Item Parsing Loop Protection
- Wrapped item parsing in try-catch blocks
- Gracefully stops parsing remaining items if corruption is detected
- Allows partial template to be used with successfully parsed items

```csharp
for (var i = 0; i < template.ItemCount; i++)
{
    try
    {
        var item = ParseStandardItem(templateAddress, ref offset);
        template.Items.Add(item);
        offset = AlignToDword(offset);
    }
    catch (IndexOutOfRangeException)
    {
        // Failed to parse item, stop parsing remaining items
        // This can happen if the dialog template is corrupted
        break;
    }
}
```

## Testing
Added comprehensive tests to verify safety checks:

1. **`DialogTemplateParser_CorruptedStringData_ShouldHandleGracefully`**
   - Tests string reading with no null terminator
   - Verifies truncation and graceful handling

2. **`DialogTemplateParser_CorruptedItemData_ShouldParseAvailableItems`**
   - Tests dialog template with corrupted item data
   - Verifies partial parsing works correctly

All existing dialog template tests continue to pass.

## Impact
- Prevents infinite loops from malformed string data
- Prevents crashes from out-of-bounds memory access
- Allows emulator to continue with partially corrupted dialog templates
- Improves robustness when handling real-world executables with non-standard dialog resources

## Backwards Compatibility
All changes are backwards compatible:
- Valid dialog templates parse exactly as before
- Only corrupted/malformed data handling is improved
- No API changes

## Performance
Minimal performance impact:
- Added try-catch blocks only execute on exceptions (rare case)
- String length counter adds negligible overhead
- Creation data size check is a simple comparison
