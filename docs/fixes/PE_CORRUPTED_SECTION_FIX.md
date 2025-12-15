# PE Loader: Corrupted Section Handling Fix

## Problem

When attempting to load certain older Windows executables (e.g., `calc.exe` from Windows ME), the PE loader would crash with an `EndOfStreamException` during the section extraction phase. The error occurred specifically in the `ExtractSectionInfo` method when trying to read section metadata.

### Error Details
```
System.IO.EndOfStreamException: Offset and address reach outside of the boundaries of the data source.
   at AsmResolver.IO.BinaryStreamReader..ctor(IDataSource , UInt64 , UInt32 , UInt32 )
   at AsmResolver.DataSourceSegment.CreateReader(UInt64 , UInt32 )
   at AsmResolver.DataSourceSegment.Write(BinaryStreamWriter )
   at AsmResolver.VirtualSegment.Write(BinaryStreamWriter )
   at AsmResolver.Extensions.WriteIntoArray(ISegment )
   at Win32Emu.Loader.PeImageLoader.ExtractSectionInfo(PEFile pe, ILogger logger)
```

## Root Cause

The issue was an inconsistency in error handling between two parts of the PE loader:

1. **Section Loading Code (lines 230-289)**: Had proper exception handling for corrupted sections that extend beyond file boundaries
2. **ExtractSectionInfo Method (lines 950-973)**: Did NOT have exception handling when calling `WriteIntoArray()` to get section metadata

When the loader encountered the corrupted `.rsrc` section in `calc.exe`:
- First pass (section loading): The exception was caught and logged as a warning, execution continued
- Second pass (section info extraction): The same exception was NOT caught, causing the entire loader to fail

## Solution

Added exception handling to the `ExtractSectionInfo` method to match the behavior of the section loading code:

```csharp
foreach (var section in pe.Sections)
{
	try
	{
		var name = section.Name ?? string.Empty;
		var rva = section.Rva;
		var virtualSize = section.Contents?.GetVirtualSize() ?? 0;
		var rawSize = (uint)(section.Contents?.WriteIntoArray().Length ?? 0);
		var characteristics = (PeSectionCharacteristics)(uint)section.Characteristics;

		sections.Add(new PeSection(name, rva, virtualSize, rawSize, characteristics));

		logger?.LogDebug("[Loader] Section {Name}: RVA=0x{Rva:X8}, VirtualSize=0x{VSize:X8}, RawSize=0x{RawSize:X8}, Characteristics=0x{Chars:X8}",
			name, rva, virtualSize, rawSize, (uint)characteristics);
	}
	catch (Exception ex) when (ex is System.IO.EndOfStreamException or ArgumentException)
	{
		// Skip corrupted sections that extend beyond file boundaries during info extraction
		var sectionName = section.Name ?? string.Empty;
		logger?.LogWarning("Skipping corrupted section {SectionName} at RVA {SectionRva:X8} during info extraction: {ErrorMessage}", 
			sectionName, section.Rva, ex.Message);
	}
}
```

## Impact

- **Before Fix**: PE files with corrupted sections (like Windows ME `calc.exe`) would fail to load with an `EndOfStreamException`
- **After Fix**: Corrupted sections are gracefully skipped during metadata extraction, allowing the loader to continue processing valid sections

## Testing

Added test case `LoadFromBytes_HandlesCorruptedSections_WithoutCrashing` to verify that:
1. The loader doesn't crash when encountering corrupted sections
2. Non-corrupted sections are still loaded successfully
3. The behavior is consistent between section loading and metadata extraction

## Related Files

- `Win32Emu/Loader/PeImageLoader.cs`: Main fix location
- `Win32Emu.Tests.Emulator/PeImageLoaderTests.cs`: Test coverage

## Notes

This fix follows the principle of robustness for handling malformed PE files. Many older executables from Windows 95/98/ME era have section headers that don't perfectly match modern PE specifications, but the executables still functioned on their original platforms. The emulator should handle these gracefully rather than failing outright.
