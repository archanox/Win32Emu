# Testing Documentation

This directory contains documentation related to Win32Emu testing strategies and evaluations.

## Quick Start

**Want to validate CPU emulation?** → [CPU Testing Quick Reference](CPU_TESTING_GUIDE.md)

## Documents

### [CPU_TESTING_GUIDE.md](CPU_TESTING_GUIDE.md) 🚀 START HERE
Quick reference guide for running CPU tests and understanding test suite options.

**Key Points:**
- Commands to run CPU tests
- Why SingleStepTests/80386 is used (not test386.asm)
- Quick comparison table
- FAQ for common questions

### [TEST386_EVALUATION.md](TEST386_EVALUATION.md)
Comprehensive evaluation of the test386.asm CPU test suite, explaining why it's not suitable for Win32Emu and what alternatives are available.

**Key Topics:**
- What test386.asm is and what it tests
- Architecture requirements and why they don't fit Win32Emu
- Detailed comparison with SingleStepTests/80386 (integrated solution)
- Instructions for external usage if needed

**Quick Answer**: Win32Emu uses SingleStepTests/80386 (already integrated) which provides better instruction-level validation with ~2.3M test cases from real hardware.

## Test Suite Decision Summary

| Test Suite | Status | Purpose | Test Count |
|------------|--------|---------|------------|
| **SingleStepTests/80386** | ✅ Integrated | Hardware-accurate instruction validation | ~2.3M tests |
| **Core CPU Tests** | ✅ Integrated | Basic instruction validation | ~135 tests |
| **test386.asm** | ❌ Not Integrated | Bare-metal BIOS testing | ~40 categories |
| **ReactOS Tests** | 📋 Planned | Win32 API validation | Thousands |

## See Also

- [Test Strategy Overview](../../README.Tests.md) - Main testing documentation
- [SingleStepTests Integration](../../Win32Emu.Tests.Emulator/SingleStepTests/README.md) - How to use the integrated conformance tests
- [ReactOS Test Integration Research](../research/REACTOS_TEST_INTEGRATION.md) - Future Win32 API testing
