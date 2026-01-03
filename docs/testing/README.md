# Testing Documentation

This directory contains documentation related to Win32Emu testing strategies and evaluations.

## Documents

### [TEST386_EVALUATION.md](TEST386_EVALUATION.md)
Comprehensive evaluation of the test386.asm CPU test suite, explaining why it's not suitable for Win32Emu and what alternatives are available.

**Key Topics:**
- What test386.asm is and what it tests
- Architecture requirements and why they don't fit Win32Emu
- Comparison with SingleStepTests/80386 (integrated solution)
- Instructions for external usage if needed

**Quick Answer**: Win32Emu uses SingleStepTests/80386 (already integrated) which provides better instruction-level validation with ~2.3M test cases from real hardware.

## See Also

- [Test Strategy Overview](../../README.Tests.md) - Main testing documentation
- [SingleStepTests Integration](../../Win32Emu.Tests.Emulator/SingleStepTests/README.md) - How to use the integrated conformance tests
- [ReactOS Test Integration Research](../research/REACTOS_TEST_INTEGRATION.md) - Future Win32 API testing
