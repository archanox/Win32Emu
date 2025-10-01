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

### 3. Win32Emu.Tests.Emulator 🔄 PLANNED
**Purpose**: Tests the x86 CPU emulator conformance  
**Status**: Ready for implementation  
**Future Coverage**:
- CPU instruction execution accuracy
- Register state management
- Memory addressing modes
- Flag handling and arithmetic operations
- Control flow (jumps, calls, returns)

### 4. Win32Emu.Tests.Integration 🔄 PLANNED
**Purpose**: End-to-end testing with real Win32 executables  
**Status**: Ready for implementation  
**Future Coverage**:
- Sample Win32 programs execution
- Regression testing for PR validation
- Performance benchmarking
- Compatibility testing with different executable types

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

### Run All Tests
```bash
dotnet test
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

**Total Tests**: 101 (all passing)  
**Test Coverage**: Comprehensive Kernel32 functionality complete  
**Infrastructure**: Fully functional and extensible  
**Ready for**: Additional DLL testing, emulator testing, integration testing

## CI Test Behavior

### Test Execution Policy
- **Core Emulator Tests**: Required - failures will block PRs (CPU, memory, instruction execution tests)
- **Win32 DLL Module Tests**: Optional - failures won't block PRs (allows test-driven development)
  - Kernel32, User32, Gdi32, DDraw, DInput, WinMM, DSound, DPlayX tests

### Purpose
This policy allows developers to:
1. Add tests for unimplemented Win32 DLL functionality without breaking CI
2. Use test-driven development approach for new Win32 modules
3. Still see test results and regressions in CI output
4. Keep core emulator functionality (CPU, memory) stable and tested

### Adding Tests for New Modules
When creating tests for Win32 DLL modules:
1. Tests will automatically be treated as optional (non-blocking)
2. Implement the functionality to make tests pass
3. Tests provide documentation of expected API behavior
4. CI will show test status without blocking development

When creating tests for core emulator features (CPU, memory, etc.):
1. Tests will be required and block PRs if they fail
2. These tests ensure the fundamental emulation engine remains stable
3. Critical for maintaining emulator correctness and performance

The failing tests document current implementation differences and serve as targets for future improvements while ensuring the test suite captures the actual behavior of the emulator.