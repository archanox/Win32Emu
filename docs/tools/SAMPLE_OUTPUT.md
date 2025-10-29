# Sample Output from Calling Convention Demo Tool

This file shows example output from running:
```bash
dotnet run --project Win32Emu.Tools.CallingConventionDemo -- \
    /tmp/reko/src/Environments/Windows/user32.xml MessageBoxA
```

## Output:

```
Win32Emu Calling Convention Standardization Demo
==================================================

This tool demonstrates Integration Opportunity #4:
Using Reko's XML definitions to standardize calling conventions
and auto-generate parameter marshalling code.

Parsing: user32.xml

Found 58 API signatures

================================================================================
API: MessageBoxA
DLL: user32.dll
Calling Convention: Stdcall
Return: INT (in eax)
Parameters: 4
  - hWnd: HWND (stack)
  - lpText: LPCSTR (stack)
  - lpCaption: LPCSTR (stack)
  - uType: UINT (stack)

Generated Parameter Reader:
        // Auto-generated parameter reader for MessageBoxA
        // Convention: Stdcall
        // Stdcall: All parameters on stack, right-to-left push
        var hWnd = a.UInt32(0); // HWND
        var lpText = a.Lpstr(1); // LPCSTR
        var lpCaption = a.Lpstr(2); // LPCSTR
        var uType = a.UInt32(3); // UINT

Generated Wrapper Method:
        /// <summary>
        /// MessageBoxA - Auto-generated wrapper from user32.dll
        /// Calling convention: Stdcall
        /// </summary>
        public uint MessageBoxA(uint hWnd, uint lpText, uint lpCaption, uint uType)
        {
            // Parameter extraction and validation
            // hWnd: HWND (value={hWnd})
            // lpText: LPCSTR (ANSI string pointer)
            var lpTextStr = _memory?.ReadCString(lpText) ?? string.Empty;
            // lpCaption: LPCSTR (ANSI string pointer)
            var lpCaptionStr = _memory?.ReadCString(lpCaption) ?? string.Empty;
            // uType: UINT (value={uType})

            _logger.LogWarning("MessageBoxA called but not fully implemented");

            // TODO: Implement actual API logic
            return 0; // FALSE
        }

Benefits of Calling Convention Standardization:
================================================
✓ Reduces boilerplate - No manual parameter extraction
✓ Type safety - Proper type mapping from Windows types
✓ Convention awareness - Handles stdcall, fastcall, thiscall correctly
✓ Automatic string marshalling - Detects and handles LPSTR/LPWSTR
✓ Consistency - All APIs follow same pattern
✓ Maintainability - Easy to update when Reko definitions change

Next Steps:
1. Integrate into Win32Emu module generation pipeline
2. Create source generators for automatic code generation
3. Add validation to detect signature mismatches
4. Extend to support COM interfaces and callbacks
```

## Example with Multiple APIs

Running without specifying an API name shows summaries of multiple APIs:

```bash
dotnet run --project Win32Emu.Tools.CallingConventionDemo -- \
    /tmp/reko/src/Environments/Windows/kernel32.xml
```

Output:
```
Found 63 API signatures

================================================================================
API: GetModuleHandleA
DLL: kernel32.dll
Calling Convention: Stdcall
Return: HMODULE (in eax)
Parameters: 1
  - lpModuleName: LPCSTR (stack)

Generated Parameter Reader:
        // Auto-generated parameter reader for GetModuleHandleA
        // Convention: Stdcall
        // Stdcall: All parameters on stack, right-to-left push
        var lpModuleName = a.Lpstr(0); // LPCSTR

================================================================================
API: LoadLibraryA
DLL: kernel32.dll
Calling Convention: Stdcall
Return: HMODULE (in eax)
Parameters: 1
  - lpLibFileName: LPCSTR (stack)

Generated Parameter Reader:
        // Auto-generated parameter reader for LoadLibraryA
        // Convention: Stdcall
        // Stdcall: All parameters on stack, right-to-left push
        var lpLibFileName = a.Lpstr(0); // LPCSTR

================================================================================
API: GetProcAddress
DLL: kernel32.dll
Calling Convention: Stdcall
Return: FARPROC (in eax)
Parameters: 2
  - hModule: HMODULE (stack)
  - lpProcName: LPCSTR (stack)

Generated Parameter Reader:
        // Auto-generated parameter reader for GetProcAddress
        // Convention: Stdcall
        // Stdcall: All parameters on stack, right-to-left push
        var hModule = a.UInt32(0); // HMODULE
        var lpProcName = a.Lpstr(1); // LPCSTR

... and 60 more APIs

Tip: Specify an API name to see full wrapper code generation
```

## Value Demonstration

This output demonstrates:

1. **Automatic signature extraction** from Reko XML
2. **Calling convention detection** (Stdcall in these examples)
3. **Type-aware parameter extraction** (LPCSTR → Lpstr accessor)
4. **Auto-generated wrapper code** with proper string marshalling
5. **Reduced boilerplate** - Compare to manual implementation in existing modules

## Comparison

### Manual Implementation (Current)
```csharp
case "MESSAGEBOXA":
    returnValue = MessageBoxA(
        a.UInt32(0), // hwnd
        a.UInt32(1), // text
        a.UInt32(2), // caption
        a.UInt32(3)  // type
    );
    return true;

public uint MessageBoxA(uint hwnd, uint text, uint caption, uint type)
{
    var textStr = _mem.ReadCString(text);
    var captionStr = _mem.ReadCString(caption);
    // ... manual implementation
}
```

### Auto-Generated (With This Tool)
```csharp
// Generated from Reko XML
var hWnd = a.UInt32(0); // HWND
var lpText = a.Lpstr(1); // LPCSTR (auto string extraction)
var lpCaption = a.Lpstr(2); // LPCSTR (auto string extraction)
var uType = a.UInt32(3); // UINT

// Wrapper with type-safe parameters and auto-marshalling
public uint MessageBoxA(uint hWnd, uint lpText, uint lpCaption, uint uType)
{
    var lpTextStr = _memory?.ReadCString(lpText) ?? string.Empty;
    var lpCaptionStr = _memory?.ReadCString(lpCaption) ?? string.Empty;
    // ... implementation
}
```

Benefits:
- ✓ Proper parameter names from Windows documentation
- ✓ Type annotations in comments
- ✓ Automatic string pointer detection and marshalling
- ✓ Consistent pattern across all APIs
- ✓ Easy to regenerate when definitions change
