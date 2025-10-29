# Reko Integration Analysis for Win32Emu

## Executive Summary

[Reko](https://github.com/uxmal/reko) is an open-source decompiler that includes comprehensive X86 architecture support and Windows environment definitions. This document analyzes what Win32Emu could gain from integrating Reko's components.

## What is Reko?

Reko is a machine code decompiler supporting multiple CPU architectures and operating system platforms. It's designed for reverse engineering and analysis of binary executables. The project is licensed under GPLv2+, which has implications for integration (see Legal Considerations below).

## Key Components Relevant to Win32Emu

### 1. X86 Architecture Components (`src/Arch/X86/`)

Reko provides extensive X86 support including:

#### Disassembler
- **Location**: `src/Arch/X86/Disassembler/`
- **Features**:
  - Complete x86/x86-64 instruction decoding
  - Support for legacy, SSE, AVX, AVX2, AVX-512, VEX, EVEX encodings
  - x87 FPU instruction support
  - Multiple instruction set variations (OneByte, TwoByte, 0F38, 0F3A prefixes)
  - APX (Advanced Performance Extensions) support
  - Comprehensive opcode group handling

**Current Win32Emu Status**: Uses Iced.Intel (v1.21.0) for disassembly, which is also comprehensive and MIT-licensed.

**Integration Value**: ⭐ LOW - Iced.Intel already provides excellent disassembly. No need to switch.

#### Instruction Rewriter
- **Location**: `src/Arch/X86/Rewriter/`
- **Features**:
  - Converts x86 instructions to RTL (Register Transfer Language) intermediate representation
  - Separate modules for ALU, Control Flow, FPU, SSE operations
  - String intrinsics handling
  - Mask register operations (AVX-512)
  
**Current Win32Emu Status**: Has basic `InstructionAnalyzer` for control flow analysis but no full RTL conversion.

**Integration Value**: ⭐⭐⭐ MEDIUM-HIGH - Could enhance static analysis capabilities for:
- Better JIT optimization
- Dead code elimination
- Control flow graph generation
- Security analysis (e.g., identifying potentially malicious patterns)

#### Assembly Renderers
- **Location**: `X86AssemblyRenderer.cs`, `IntelAssemblyRenderer.cs`, `AttAssemblyRenderer.cs`, `NasmAssemblyRenderer.cs`
- **Features**:
  - Multiple assembly syntax formats (Intel, AT&T, NASM)
  - Pretty-printing with proper operand formatting
  
**Current Win32Emu Status**: Limited instruction formatting in debugger.

**Integration Value**: ⭐⭐ MEDIUM - Would improve debugging experience with better disassembly formatting.

#### Calling Conventions
- **Location**: `X86CallingConvention.cs`, `FastcallConvention.cs`, `ThisCallConvention.cs`
- **Features**:
  - Standardized calling convention implementations
  - Register allocation for different conventions (stdcall, cdecl, fastcall, thiscall)
  - Return value handling
  
**Current Win32Emu Status**: Calling conventions handled manually in each Win32 API wrapper.

**Integration Value**: ⭐⭐⭐ HIGH - Could standardize and automate calling convention handling, reducing boilerplate code.

### 2. Windows Environment Components (`src/Environments/Windows/`)

Reko provides comprehensive Windows platform support:

#### Win32 API Definitions (XML Files)
- **Key Files**:
  - `kernel32.xml` (64 procedures defined)
  - `user32.xml` (60 procedures)
  - `gdi32.xml`
  - `advapi32.xml`
  - `shell32.xml`
  - `ole32.xml`, `oleaut32.xml`
  - `wsock32.xml`
  - `wininet.xml`
  - And many more...

- **Format**: Structured XML with:
  ```xml
  <procedure name="CreateWindowExA">
    <signature>
      <return>
        <type>HWND</type>
        <reg>eax</reg>
      </return>
      <arg name="dwExStyle">
        <type>DWORD</type>
        <stack size="4" />
      </arg>
      <!-- ... more args ... -->
    </signature>
  </procedure>
  ```

**Current Win32Emu Status**: Manually implemented C# methods with attributes like:
```csharp
[DllModuleExport]
public uint CreateWindowExA(/* manual params */)
```

**Integration Value**: ⭐⭐⭐⭐ VERY HIGH - These XML definitions could be used to:
1. **Validate existing implementations** - Cross-check parameter types and counts
2. **Auto-generate stub implementations** - Create skeleton code for missing APIs
3. **Documentation** - Auto-generate API documentation
4. **API coverage reporting** - Identify which APIs are implemented vs. missing
5. **Type safety** - Ensure proper marshalling of Windows types

#### Windows Platform Classes
- **Location**: `Win32Platform.cs`, `Win_x86_64_Platform.cs`, etc.
- **Features**:
  - System call handling (INT 3, INT 0x29)
  - Platform-specific characteristics
  - Exception handling
  - Calling convention selection based on platform

**Integration Value**: ⭐⭐ MEDIUM - Some architectural patterns could be adopted.

#### Utility Classes
- **`MsMangledNameParser.cs`**: Parses MSVC name mangling
- **`MsPrintfFormatParser.cs`**: Parses printf/scanf format strings
- **`CodePages.cs`**: Windows code page support
- **`LocaleIds.cs`**: Windows locale ID definitions
- **`SignatureGuesser.cs`**: Heuristically determines function signatures

**Integration Value**: ⭐⭐⭐ HIGH for format parsers - Could improve string formatting API implementations.

#### Win32 Characteristics Files
- **Files**: `win32characteristics.xml`, `win64characteristics.xml`, `win16characteristics.xml`
- **Content**: Procedural characteristics like whether APIs terminate, allocate memory, etc.

**Integration Value**: ⭐⭐ MEDIUM - Could help with optimization and analysis.

## Specific Integration Opportunities

### 1. API Definition Validation and Code Generation (HIGH PRIORITY)

**Opportunity**: Use Reko's XML API definitions to create a validation and code generation tool.

**Implementation Approach**:
```csharp
// Tool: Win32Emu.Tools.ApiValidator
// 1. Parse Reko XML files
// 2. Parse Win32Emu module files (Kernel32Module.cs, User32Module.cs, etc.)
// 3. Compare signatures
// 4. Generate report of:
//    - Missing APIs
//    - Signature mismatches
//    - Stub generation opportunities
```

**Benefits**:
- Ensures API compatibility
- Identifies gaps in implementation
- Reduces manual coding errors
- Facilitates adding new API support

**Example Output**:
```
API Coverage Report for Kernel32:
✓ GetModuleHandleA - Implemented and matches signature
✓ GetProcAddress - Implemented and matches signature
⚠ CreateFileA - Implemented but signature differs (param 3 type mismatch)
✗ CreateFileMappingA - Not implemented
✗ MapViewOfFile - Not implemented

Suggestions:
- Review CreateFileA parameter 3 (expected LPSECURITY_ATTRIBUTES, found uint)
- Consider implementing CreateFileMappingA (used by 15% of analyzed games)
```

### 2. Enhanced Instruction Analysis (MEDIUM-HIGH PRIORITY)

**Opportunity**: Integrate Reko's X86 Rewriter for advanced static analysis.

**Use Cases**:
1. **JIT Optimization**: Convert x86 to RTL, optimize, then generate native code
2. **Security Analysis**: Detect suspicious instruction patterns (shellcode, anti-debugging)
3. **Compatibility Analysis**: Identify use of specific CPU features
4. **Symbolic Execution**: Enable better debugging with symbolic state tracking

**Implementation Approach**:
```csharp
// New component: Win32Emu.Analysis
// - X86ToRtlConverter (wraps Reko rewriter)
// - RtlOptimizer
// - ControlFlowAnalyzer (enhanced version)
// - SecurityPatternDetector
```

### 3. Improved Debugging Output (MEDIUM PRIORITY)

**Opportunity**: Use Reko's assembly renderers for better disassembly formatting.

**Benefits**:
- Multiple syntax options (Intel/AT&T/NASM)
- Professional-grade formatting
- Better debugger experience

**Example**:
```
Current:  CALL 0x401000
Enhanced: call    dword ptr [kernel32!GetModuleHandleA]
          ; Calls GetModuleHandleA(lpModuleName=0x403000 "user32.dll")
```

### 4. Calling Convention Standardization (HIGH PRIORITY)

**Opportunity**: Use Reko's calling convention classes to reduce boilerplate.

**Current Problem**: Each API wrapper manually handles parameters:
```csharp
public uint MessageBoxA(uint hwnd, uint text, uint caption, uint type)
{
    var textStr = _mem.ReadCString(text);
    var captionStr = _mem.ReadCString(caption);
    // Manual marshalling...
}
```

**With Reko Integration**:
```csharp
// Auto-generated from XML + calling convention
[Win32Api("user32.dll", CallingConvention.Stdcall)]
public partial class User32Module
{
    // Signature from XML, implementation guided by convention
    [ApiDefinition("user32.xml", "MessageBoxA")]
    public partial HWND MessageBoxA(HWND hwnd, LPCSTR text, LPCSTR caption, UINT type);
}
```

### 5. Printf/Format String Support (MEDIUM PRIORITY)

**Opportunity**: Use `MsPrintfFormatParser` for better printf/sprintf/scanf implementations.

**Current Status**: Win32Emu has basic printf support in msvcrt.

**Enhancement**: Parse format strings more accurately:
- Validate format specifiers
- Correctly handle width, precision, flags
- Support all Windows-specific format extensions (%I64d, %S, etc.)

## Legal Considerations

⚠️ **IMPORTANT**: Reko is licensed under **GPLv2 or later**.

**Implications**:
- **Direct code integration**: Would require Win32Emu to become GPL-licensed (viral license)
- **XML data usage**: Data files might not be covered by GPL (consult legal counsel)
- **Clean room implementation**: Can use XML as specification, implement independently
- **Tool-based approach**: Generate code from XML as build step (generated code may not be derivative work)

**Recommendation**: 
1. **Use XML files as specification only** - The API definitions are factual data about Windows APIs
2. **Implement parsers and tools independently** - Don't copy Reko's code
3. **Give credit** - Acknowledge Reko project in documentation
4. **Consult legal if uncertain** - Especially for any potential commercial use

## Recommended Implementation Plan

### Phase 1: API Definition Tooling (2-3 weeks)
1. Create `Win32Emu.Tools.ApiAnalyzer` project
2. Implement XML parser for Reko API definitions
3. Implement Win32Emu module analyzer (parse existing .cs files)
4. Generate coverage report
5. Integrate into CI/CD to track API coverage over time

### Phase 2: Code Generation (3-4 weeks)
1. Extend tool to generate stub implementations
2. Generate unit test templates for each API
3. Generate documentation from XML
4. Create source generator for marshalling code

### Phase 3: Enhanced Analysis (4-6 weeks)
1. Implement RTL converter wrapper (using Reko as optional dependency)
2. Add control flow graph visualization
3. Integrate into debugger
4. Add pattern detection for common game engine techniques

### Phase 4: Format String Parser (1-2 weeks)
1. Implement printf format parser (clean room, using Reko's as reference)
2. Enhance msvcrt printf family functions
3. Add validation and warnings for incorrect format strings

## Alternative: Minimal Integration

If full integration is too complex, consider this minimal approach:

### 1. One-Time API Audit
- Download Reko XML files
- Run comparison against Win32Emu modules
- Create issue tickets for missing APIs
- Manually implement based on XML specs

### 2. Reference Implementation
- Keep Reko XMLs in `docs/api-reference/`
- Use as documentation when implementing new APIs
- No code generation, just manual reference

### 3. Test Oracle
- Use XML definitions to generate test cases
- Validate Win32Emu behavior matches expected signatures

## Conclusion

Reko provides valuable resources for Win32Emu, particularly:

1. **API Definitions (⭐⭐⭐⭐)**: Comprehensive, structured specifications of Windows APIs
2. **Calling Conventions (⭐⭐⭐)**: Standardized handling that could reduce boilerplate
3. **Instruction Analysis (⭐⭐⭐)**: Advanced capabilities for optimization and security
4. **Format Parsers (⭐⭐⭐)**: Better string formatting support
5. **Assembly Rendering (⭐⭐)**: Improved debugging output

**Primary Value**: The XML API definitions are the "killer feature" - they provide a comprehensive, machine-readable specification of Windows APIs that could significantly improve Win32Emu's development process.

**Key Recommendation**: Start with **Phase 1** (API Definition Tooling) as it:
- Provides immediate value (coverage analysis)
- Requires minimal code
- Has no licensing concerns (XML as data)
- Enables informed decisions about future phases

## Examples

### Example 1: API Coverage Report

```bash
$ dotnet run --project Win32Emu.Tools.ApiAnalyzer

Win32 API Coverage Analysis
============================

Kernel32 (kernel32.xml):
  Total APIs in Reko: 64
  Implemented in Win32Emu: 45 (70.3%)
  Missing: 19
  
  Top missing APIs by usage frequency:
  1. CreateFileMappingA - Used by 23% of analyzed binaries
  2. MapViewOfFile - Used by 23% of analyzed binaries
  3. UnmapViewOfFile - Used by 18% of analyzed binaries
  4. VirtualProtect - Used by 15% of analyzed binaries
  5. LoadLibraryExA - Used by 12% of analyzed binaries

User32 (user32.xml):
  Total APIs in Reko: 60
  Implemented in Win32Emu: 52 (86.7%)
  Missing: 8
  
Overall Coverage: 78.2% (97/124 APIs)
```

### Example 2: Generated Stub

From Reko XML:
```xml
<procedure name="CreateFileMappingA">
  <signature>
    <return>
      <type>HANDLE</type>
      <reg>eax</reg>
    </return>
    <arg name="hFile">
      <type>HANDLE</type>
      <stack size="4" />
    </arg>
    <arg name="lpFileMappingAttributes">
      <type>LPSECURITY_ATTRIBUTES</type>
      <stack size="4" />
    </arg>
    <arg name="flProtect">
      <type>DWORD</type>
      <stack size="4" />
    </arg>
    <arg name="dwMaximumSizeHigh">
      <type>DWORD</type>
      <stack size="4" />
    </arg>
    <arg name="dwMaximumSizeLow">
      <type>DWORD</type>
      <stack size="4" />
    </arg>
    <arg name="lpName">
      <type>LPCSTR</type>
      <stack size="4" />
    </arg>
  </signature>
</procedure>
```

Auto-generated stub:
```csharp
/// <summary>
/// Creates or opens a named or unnamed file mapping object for a specified file.
/// </summary>
/// <param name="hFile">A handle to the file from which to create a file mapping object.</param>
/// <param name="lpFileMappingAttributes">A pointer to a SECURITY_ATTRIBUTES structure.</param>
/// <param name="flProtect">Specifies the page protection of the file mapping object.</param>
/// <param name="dwMaximumSizeHigh">The high-order DWORD of the maximum size of the file mapping object.</param>
/// <param name="dwMaximumSizeLow">The low-order DWORD of the maximum size of the file mapping object.</param>
/// <param name="lpName">The name of the file mapping object.</param>
/// <returns>If the function succeeds, the return value is a handle to the newly created file mapping object.</returns>
/// <remarks>
/// Auto-generated from kernel32.xml
/// TODO: Implement actual functionality
/// </remarks>
[DllModuleExport]
public uint CreateFileMappingA(
    uint hFile,
    uint lpFileMappingAttributes,
    uint flProtect,
    uint dwMaximumSizeHigh,
    uint dwMaximumSizeLow,
    uint lpName)
{
    _logger.LogWarning("CreateFileMappingA called but not implemented");
    _logger.LogDebug("  hFile: 0x{HFile:X8}", hFile);
    _logger.LogDebug("  lpFileMappingAttributes: 0x{Attrs:X8}", lpFileMappingAttributes);
    _logger.LogDebug("  flProtect: 0x{Protect:X8}", flProtect);
    _logger.LogDebug("  dwMaximumSizeHigh: 0x{High:X8}", dwMaximumSizeHigh);
    _logger.LogDebug("  dwMaximumSizeLow: 0x{Low:X8}", dwMaximumSizeLow);
    _logger.LogDebug("  lpName: 0x{Name:X8}", lpName);
    
    // TODO: Implement CreateFileMappingA
    return 0; // Return NULL handle to indicate failure
}
```

### Example 3: Type Safety Enhancement

Current unsafe approach:
```csharp
public uint GetSystemMetrics(uint nIndex)
{
    // Easy to pass wrong type, hard to validate
}
```

With Reko XML validation:
```csharp
// Tool detects: nIndex should be 'int', not 'uint'
// Build warning: Parameter type mismatch in GetSystemMetrics
public int GetSystemMetrics(int nIndex) // Corrected
{
    // ...
}
```

## Resources

- [Reko GitHub Repository](https://github.com/uxmal/reko)
- [Reko Documentation](https://github.com/uxmal/reko/wiki)
- [Win32 API Documentation](https://learn.microsoft.com/en-us/windows/win32/)
- [Iced x86 Disassembler](https://github.com/icedland/iced) (Current Win32Emu dependency)

## Next Steps

1. Review this analysis with the team
2. Decide on integration approach (full, minimal, or reference-only)
3. If proceeding: Start with Phase 1 (API Definition Tooling)
4. Create epic/issues for chosen approach
5. Allocate development resources

---

**Document Version**: 1.0  
**Date**: October 27, 2025  
**Author**: Win32Emu Development Team  
**Status**: Proposal / Analysis
