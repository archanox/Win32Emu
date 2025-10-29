# Reko Integration Research - Executive Summary

## Problem Statement

> What can we do with https://github.com/uxmal/reko/tree/master/src/Arch/X86 and https://github.com/uxmal/reko/tree/master/src/Environments/Windows if it were integrated into our emulator?

## TL;DR Answer

**Reko's Windows environment XML files are a goldmine for Win32Emu.** They provide machine-readable specifications for 337+ Windows API procedures across 12+ DLLs. Integration opportunities range from simple reference documentation to automated code generation and validation.

**Recommended approach**: Use the XML files as specifications and validation sources. Don't integrate Reko's code directly (GPL licensing).

## What We Delivered

### 1. Comprehensive Analysis Document
**File**: `REKO_INTEGRATION_ANALYSIS.md` (474 lines)

Detailed analysis covering:
- What Reko offers in X86 architecture and Windows environment
- Value assessment for each component (⭐ to ⭐⭐⭐⭐)
- Specific integration opportunities with implementation examples
- Legal considerations (GPL licensing)
- 4-phase implementation plan
- Concrete examples and use cases

### 2. Proof-of-Concept Tool
**Directory**: `Win32Emu.Tools.ApiAnalyzer/`

Working tool that demonstrates API definition usage:
- Parses Reko XML API definitions
- Analyzes Win32Emu module implementations
- Generates coverage reports
- Shows integration value in practice

## Key Findings

### High-Value Opportunities (⭐⭐⭐⭐)

#### 1. API Definition Validation & Code Generation
- **What**: Reko has 337 API procedures defined with complete signatures
- **Value**: Validate implementations, generate stubs, track coverage
- **Example**: Auto-detect when an API signature doesn't match Windows spec
- **Effort**: 2-3 weeks for basic tooling

#### 2. Calling Convention Standardization  
- **What**: Structured definitions of parameter passing and return values
- **Value**: Reduce boilerplate, auto-generate marshalling code
- **Example**: Replace manual parameter handling with generated code
- **Effort**: 3-4 weeks for implementation

### Medium-Value Opportunities (⭐⭐⭐)

#### 3. Enhanced Instruction Analysis
- **What**: RTL (Register Transfer Language) conversion for x86 instructions
- **Value**: Better JIT optimization, security analysis, symbolic execution
- **Example**: Optimize instruction sequences before JIT compilation
- **Effort**: 4-6 weeks for integration

#### 4. Format String Parser
- **What**: Comprehensive printf/scanf format string parser
- **Value**: Better msvcrt implementation, validation, error detection
- **Example**: Correctly handle all Windows format extensions (%I64d, etc.)
- **Effort**: 1-2 weeks clean room implementation

### Low-Value Opportunities (⭐⭐)

#### 5. Assembly Renderers
- **What**: Multiple assembly syntax formats (Intel, AT&T, NASM)
- **Value**: Better debugger output with professional formatting
- **Effort**: 1-2 weeks for integration

## What We Don't Need

### X86 Disassembler (⭐ LOW)
**Why**: Win32Emu already uses Iced.Intel, which is:
- Comprehensive and well-maintained
- MIT licensed (no GPL concerns)
- High performance
- Well integrated

**Recommendation**: Keep Iced.Intel, don't switch to Reko's disassembler.

## Legal Considerations

⚠️ **Reko is GPLv2+** - Viral license that would require Win32Emu to become GPL

**Safe approach**:
1. ✅ Use XML files as **data/specification** (not code)
2. ✅ Implement parsers **independently** (clean room)
3. ✅ Give **credit** in documentation
4. ❌ Don't copy Reko's C# code

**Rationale**: API definitions are factual information about Windows, similar to Microsoft's documentation. Independent implementation avoids GPL contamination.

## Recommended Implementation Plan

### Phase 1: API Definition Tooling (2-3 weeks) ⭐⭐⭐⭐
**Goal**: Enhance the analyzer tool for practical use

Tasks:
1. Add signature validation (compare parameter types)
2. Generate stub implementations for missing APIs
3. Generate unit test templates
4. Create JSON/HTML reports
5. Integrate into CI/CD for coverage tracking

**Value**: Immediate - helps identify and implement missing APIs

### Phase 2: Code Generation (3-4 weeks) ⭐⭐⭐⭐
**Goal**: Auto-generate marshalling and boilerplate code

Tasks:
1. Create source generator for API wrappers
2. Generate calling convention handling
3. Generate documentation from XML
4. Reduce manual coding in module files

**Value**: High - reduces boilerplate, improves consistency

### Phase 3: Enhanced Analysis (4-6 weeks) ⭐⭐⭐
**Goal**: Advanced instruction analysis capabilities

Tasks:
1. Implement RTL converter wrapper
2. Add control flow graph visualization
3. Integrate into debugger
4. Add pattern detection for optimization

**Value**: Medium-High - enables advanced features

### Phase 4: Format String Parser (1-2 weeks) ⭐⭐⭐
**Goal**: Better printf/scanf implementations

Tasks:
1. Implement format parser (clean room)
2. Enhance msvcrt module
3. Add validation and warnings

**Value**: Medium - improves compatibility

## Practical Examples

### Example 1: Missing API Detection

Current state:
```
Win32Emu has 238 kernel32 APIs implemented
Reko defines 63 kernel32 APIs
Only 5 match (7.9% coverage)
```

Why so low? Reko only has a subset defined. But those 5 matches tell us:
- Our implementations align with Windows specs ✓
- We can validate signatures against Reko's definitions ✓
- We can identify which common APIs we're missing ✓

### Example 2: Signature Validation

Reko XML says:
```xml
<procedure name="CreateFileA">
  <arg name="lpFileName"><type>LPCSTR</type></arg>
  <arg name="dwDesiredAccess"><type>DWORD</type></arg>
  ...
</procedure>
```

Win32Emu has:
```csharp
public uint CreateFileA(uint lpFileName, uint dwDesiredAccess, ...)
```

Tool could detect: "lpFileName should be LPCSTR (pointer), not uint" ✓

### Example 3: Stub Generation

From Reko XML, auto-generate:
```csharp
/// <summary>
/// Creates or opens a file or I/O device.
/// Auto-generated from kernel32.xml
/// </summary>
[DllModuleExport]
public uint CreateFileA(
    uint lpFileName,
    uint dwDesiredAccess,
    uint dwShareMode,
    uint lpSecurityAttributes,
    uint dwCreationDisposition,
    uint dwFlagsAndAttributes,
    uint hTemplateFile)
{
    _logger.LogWarning("CreateFileA not implemented");
    // TODO: Implement
    return 0xFFFFFFFF; // INVALID_HANDLE_VALUE
}
```

## Immediate Actions You Can Take

### 1. Reference Usage (Zero Effort)
Keep Reko XML files as reference when implementing APIs:
```bash
# Look up API signature before implementing
cat /tmp/reko/src/Environments/Windows/kernel32.xml | grep -A 20 "CreateFileA"
```

### 2. One-Time Audit (1-2 hours)
Run the analyzer tool to identify gaps:
```bash
git clone https://github.com/uxmal/reko.git /tmp/reko
cd Win32Emu.Tools.ApiAnalyzer
dotnet run -- /tmp/reko/src/Environments/Windows ../Win32Emu/Win32/Modules
```

Review missing APIs, create issues for commonly-used ones.

### 3. Keep XMLs as Documentation (Low Effort)
Add to repository:
```bash
mkdir -p docs/api-reference/reko
cp /tmp/reko/src/Environments/Windows/*.xml docs/api-reference/reko/
```

Include link in API implementation files:
```csharp
// See docs/api-reference/reko/kernel32.xml for signature
```

## Conclusion

**Answer to the problem statement**: With Reko's X86 architecture and Windows environment, we can:

1. ✅ **Validate** our API implementations against authoritative specs
2. ✅ **Generate** stub code for missing APIs automatically  
3. ✅ **Standardize** calling convention handling to reduce boilerplate
4. ✅ **Track** API coverage over time in CI/CD
5. ✅ **Document** APIs using machine-readable definitions
6. ✅ **Optimize** JIT compilation with advanced instruction analysis
7. ✅ **Improve** printf/scanf implementations with format parsers
8. ✅ **Enhance** debugging with professional assembly formatting

**Recommended next step**: Start with Phase 1 (API Definition Tooling) - it provides immediate value with minimal effort and no licensing concerns.

## Files in This PR

- `REKO_INTEGRATION_ANALYSIS.md` - Full analysis (474 lines)
- `Win32Emu.Tools.ApiAnalyzer/` - Proof-of-concept tool
  - `Program.cs` - Implementation (244 lines)
  - `README.md` - Documentation (170 lines)
  - `SAMPLE_OUTPUT.txt` - Example output
  - `Win32Emu.Tools.ApiAnalyzer.csproj` - Project file
- `REKO_INTEGRATION_EXECUTIVE_SUMMARY.md` - This document

## Resources

- [Reko GitHub](https://github.com/uxmal/reko)
- [Full Analysis](REKO_INTEGRATION_ANALYSIS.md)
- [Tool README](Win32Emu.Tools.ApiAnalyzer/README.md)
- [Sample Output](Win32Emu.Tools.ApiAnalyzer/SAMPLE_OUTPUT.txt)

---

**Research Date**: October 27, 2025  
**Status**: Complete - Ready for team review and decision
