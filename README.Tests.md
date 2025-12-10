# Win32Emu Test Strategy

This document outlines the comprehensive testing strategy for Win32Emu, organized into multiple test projects for different aspects of the emulator.

## Test Project Structure

### 1. Win32Emu.Tests.Kernel32 ✅ COMPLETED
**Purpose**: Tests the Kernel32.dll API emulation  
**Status**: 101/101 tests passing (100% success rate)  
**Coverage**: 
- Basic functions (GetVersion, error handling, process functions, performance counters, code pages)
- Memory management (GlobalAlloc, HeapAlloc, VirtualAlloc)
- File I/O operations (CreateFileA, handles, standard I/O)
- Module/process functions (GetModuleHandleA, LoadLibraryA)
- Environment and command-line functions
- CPU and memory interaction
- Debugging functionality
- Win32 dispatcher integration

### 2. Win32Emu.Tests.User32 📋 TEMPLATE CREATED
**Purpose**: Tests the User32.dll API emulation  
**Status**: Template ready for implementation  
**Future Coverage**:
- Window management (CreateWindow, ShowWindow, DestroyWindow)
- Message handling (PostMessage, SendMessage, message loops)
- Input handling (keyboard, mouse, GetAsyncKeyState)
- Drawing/GDI integration

### 3. Win32Emu.Tests.Emulator ✅ COMPLETED
**Purpose**: Tests the x86 CPU emulator conformance  
**Status**: Core tests passing, extensive conformance tests available  
**Coverage**:
- **CPU Emulation**: Instruction execution, register management, flag handling (CF, ZF, SF, OF, PF, AF)
- **Basic Instructions** (8086/286/386): ADD, SUB, XOR, AND, OR, TEST, CMP, INC, DEC, SHL, SHR
- **486 Instructions**: BSWAP, CMPXCHG, XADD, INVD, WBINVD, INVLPG
- **Pentium Instructions**: RDTSC, CPUID, CMPXCHG8B, RDMSR, WRMSR, RSM, MMX
- **Advanced Features**: JIT compilation, async CPU operations, state suspend/resume
- **Memory & Addressing**: Virtual memory, segment handling, boundary conditions
- **Backend Integration**: SDL3, Software rendering, DirectDraw, DirectInput
- **SingleStepTests Conformance Suite**: 941 hardware-generated CPU tests (optional, not blocking CI)
  - Tests validate CPU implementation against real 386 hardware behavior
  - Run with: `dotnet test --filter "Category=ConformanceTests"`
  - See: https://github.com/SingleStepTests/80386

### 4. Win32Emu.Tests.Integration 🔄 PLANNED
**Purpose**: End-to-end testing with real Win32 executables  
**Status**: Ready for implementation  
**Future Coverage**:
- Sample Win32 programs execution
- Regression testing for PR validation
- Performance benchmarking
- Compatibility testing with different executable types

### 5. Win32Emu.Tests.ReactOS 📋 PLANNED
**Purpose**: Leverage ReactOS test suite for Win32 API validation  
**Status**: Design complete, ready for implementation  
**Future Coverage**:
- Kernel32.dll API tests (~60 test executables)
- User32.dll API tests (~80 test executables)
- GDI32.dll API tests
- Additional Win32 module tests from ReactOS
- See: <https://github.com/reactos/reactos/tree/master/modules/rostests/apitests>

**Approach**: Run ReactOS test executables (compiled to PE format) directly in Win32Emu, parse Wine test framework output, and report results via xUnit.

**Documentation**:
- [ReactOS Test Integration Research](docs/research/REACTOS_TEST_INTEGRATION.md) - Comprehensive analysis and strategy
- [Implementation Plan](docs/implementation/REACTOS_TEST_INTEGRATION_PLAN.md) - Developer guide for using ReactOS tests

## Test Infrastructure Components

### Core Testing Classes
- **MockCpu**: Simulates CPU for API testing without full emulation
- **TestEnvironment**: Provides complete test setup (memory, CPU, process environment)
- **Memory utilities**: String handling, allocation, memory access helpers

### Testing Patterns
1. **API Function Testing**: Direct testing of Win32 API implementations
2. **Integration Testing**: Testing interactions between components
3. **Behavior Documentation**: Tests that document current vs. expected behavior
4. **Error Condition Testing**: Validation of error handling and edge cases

## Usage

### Run All Tests (excluding conformance tests)
```bash
dotnet test --filter "Category!=ConformanceTests"
```

### Run All Tests Including Conformance Tests
```bash
dotnet test
```

### Run Only Conformance Tests
```bash
dotnet test --filter "Category=ConformanceTests"
```

### Run Specific Test Project
```bash
dotnet test Win32Emu.Tests.Kernel32
```

### Run Test Category
```bash
dotnet test --filter "BasicFunctionsTests"
```

### CI/CD Integration
Tests are categorized to support different CI/CD requirements:
- **Core Tests**: Must pass for CI/CD success (basic functions, memory management)
- **Compatibility Tests**: May fail but provide feedback (file I/O edge cases)
- **Performance Tests**: Track performance regressions

## Adding New Tests

### For New Win32 APIs:
1. Add tests to appropriate DLL test project (Kernel32, User32, etc.)
2. Follow existing naming conventions
3. Use TestEnvironment for consistent setup
4. Document any known behavioral differences

### For New DLL Modules:
1. Create new test project: `Win32Emu.Tests.{DllName}`
2. Copy and adapt test infrastructure from Kernel32 tests
3. Add project to solution and configure properly
4. Follow established patterns for test organization

### For Emulator Features:
1. Add tests to Win32Emu.Tests.Emulator
2. Focus on CPU instruction accuracy and edge cases
3. Include performance validation where relevant

## Test Quality Guidelines

### Requirements
- Each test should be independent and isolated
- Use descriptive test names that explain the scenario
- Include both positive and negative test cases
- Document any implementation limitations or known issues
- Provide clear assertions with meaningful error messages

### Documentation
- Comment complex test scenarios
- Explain any deviations from expected Win32 behavior
- Include references to Win32 API documentation where helpful
- Mark tests that document current behavior vs. ideal behavior

## Current Status

**Total Tests**: 1076+ (135 core + 941 conformance)  
**Test Coverage**: 
- Kernel32 functionality complete (101 tests)
- CPU Emulator basic/486/Pentium instructions (34 tests)
- CPU Conformance tests (941 SingleStepTests - optional)  
**Infrastructure**: Fully functional and extensible  
**Ready for**: User32 testing, integration testing, additional instruction coverage

## CI Test Behavior

### Test Execution Policy
Tests are categorized using xUnit traits to control CI behavior:

- **Core Tests**: Required - failures will block PRs
  - CPU emulator, memory management, GUI, code generation, etc.
  - Any tests NOT marked with DllModuleTests or ConformanceTests traits
  
- **DLL Module Tests** (`[assembly: Trait("Category", "DllModuleTests")]`): Optional - won't block PRs
  - Win32 DLL API tests: Kernel32, User32, Gdi32, DDraw, DInput, WinMM, DSound, DPlayX
  - Allows test-driven development for Win32 API implementation
  - Run locally with: `dotnet test --filter "Category=DllModuleTests"`
  
- **Conformance Tests** (`[Trait("Category", "ConformanceTests")]`): Optional - informational only
  - 941 hardware-generated CPU instruction tests from SingleStepTests/80386 suite
  - Validates CPU implementation against real 386 hardware behavior
  - Run locally with: `dotnet test --filter "Category=ConformanceTests"`

- **ReactOS Tests** (`[Trait("Category", "ReactOSTests")]`): Optional - when implemented
  - Comprehensive Win32 API validation using ReactOS test suite
  - Tests compiled from ReactOS source run as PE executables in Win32Emu
  - Validates API implementations match Windows behavior
  - Run locally with: `dotnet test --filter "Category=ReactOSTests"`

### Purpose
This policy allows developers to:
1. Add tests for unimplemented Win32 DLL functionality without breaking CI
2. Use test-driven development approach for new Win32 modules
3. Still see test results and regressions in CI output
4. Keep core emulator functionality (CPU, memory) stable and tested
5. Track CPU instruction conformance without blocking development

### Adding Tests for New Modules
When creating tests for Win32 DLL modules:
1. Create test project: `Win32Emu.Tests.{ModuleName}`
2. Add `AssemblyInfo.cs` with: `[assembly: Trait("Category", "DllModuleTests")]`
3. Tests will automatically be treated as optional (non-blocking) in CI
4. Implement the functionality to make tests pass
5. Tests provide documentation of expected API behavior

When creating tests for core emulator features (CPU, memory, etc.):
1. Tests will be required and block PRs if they fail
2. These tests ensure the fundamental emulation engine remains stable
3. Critical for maintaining emulator correctness and performance

The failing tests document current implementation differences and serve as targets for future improvements while ensuring the test suite captures the actual behavior of the emulator.