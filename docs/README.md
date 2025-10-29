# Win32Emu Documentation

This directory contains all the documentation for the Win32Emu project, organized by category.

## Guides

User guides and how-to documentation:

- [DEBUGGING_GUIDE.md](guides/DEBUGGING_GUIDE.md) - Enhanced debugging mode
- [INTERACTIVE_DEBUGGER_GUIDE.md](guides/INTERACTIVE_DEBUGGER_GUIDE.md) - Interactive debugger
- [GDB_SERVER_GUIDE.md](guides/GDB_SERVER_GUIDE.md) - GDB server for Ghidra/IDA integration
- [GDB_SERVER_QUICK_REFERENCE.md](guides/GDB_SERVER_QUICK_REFERENCE.md) - Quick reference for GDB server
- [DEBUGGER_QUICK_REFERENCE.md](guides/DEBUGGER_QUICK_REFERENCE.md) - Debugger quick reference
- [GHIDRA_DEBUGGING_FAQ.md](guides/GHIDRA_DEBUGGING_FAQ.md) - Troubleshooting "no debugging symbols" and debugging tips
- [VFS_DOCUMENTATION.md](guides/VFS_DOCUMENTATION.md) - Virtual File System for game file isolation
- [OPENTELEMETRY_USAGE.md](guides/OPENTELEMETRY_USAGE.md) - OpenTelemetry for logging, metrics, and profiling
- [RIDER_OPENTELEMETRY_SETUP.md](guides/RIDER_OPENTELEMETRY_SETUP.md) - JetBrains Rider integration guide
- [JIT_CACHE_QUICK_START.md](guides/JIT_CACHE_QUICK_START.md) - JIT cache quick start
- [THREE_WAY_TESTING.md](guides/THREE_WAY_TESTING.md) - Three-way testing methodology

## Implementation

Technical implementation details and architecture:

- [SILK_NET_MIGRATION.md](implementation/SILK_NET_MIGRATION.md) - Backend system and configuration
- [INTRINSICS.md](implementation/INTRINSICS.md) - Hardware intrinsics and CPU acceleration
- [MESSAGE_DISPATCHER_IMPLEMENTATION.md](implementation/MESSAGE_DISPATCHER_IMPLEMENTATION.md) - Message dispatcher implementation
- [JIT_CACHE_IMPLEMENTATION.md](implementation/JIT_CACHE_IMPLEMENTATION.md) - JIT caching to disk for faster emulation
- [DIRECTDRAW_SDL3_IMPLEMENTATION.md](implementation/DIRECTDRAW_SDL3_IMPLEMENTATION.md) - DirectDraw SDL3 implementation
- [DIRECTINPUT_IMPLEMENTATION.md](implementation/DIRECTINPUT_IMPLEMENTATION.md) - DirectInput implementation
- [DIRECTSOUND_IMPLEMENTATION.md](implementation/DIRECTSOUND_IMPLEMENTATION.md) - DirectSound implementation
- [SDL3_IMPLEMENTATION_SUMMARY.md](implementation/SDL3_IMPLEMENTATION_SUMMARY.md) - SDL3 implementation summary
- [SDL3_INTEGRATION.md](implementation/SDL3_INTEGRATION.md) - SDL3 integration
- [MULTITHREADING_IMPLEMENTATION.md](implementation/MULTITHREADING_IMPLEMENTATION.md) - Multithreading implementation
- [OLE32_IMPLEMENTATION.md](implementation/OLE32_IMPLEMENTATION.md) - OLE32 implementation
- [VFS_IMPLEMENTATION_SUMMARY.md](implementation/VFS_IMPLEMENTATION_SUMMARY.md) - VFS implementation summary

See the [implementation](implementation/) directory for the complete list.

## Examples

Practical examples and usage patterns:

- [TELEMETRY_EXAMPLE.md](examples/TELEMETRY_EXAMPLE.md) - Practical examples of using OpenTelemetry
- [JIT_CACHE_EXAMPLES.md](examples/JIT_CACHE_EXAMPLES.md) - JIT cache usage examples and best practices
- [ENHANCED_LOGGING_EXAMPLE.md](examples/ENHANCED_LOGGING_EXAMPLE.md) - Enhanced logging examples
- [INTERACTIVE_DEBUGGER_EXAMPLE.md](examples/INTERACTIVE_DEBUGGER_EXAMPLE.md) - Interactive debugger examples
- [INSTRUCTION_ANALYZER_EXAMPLE.md](examples/INSTRUCTION_ANALYZER_EXAMPLE.md) - Instruction analyzer examples
- [MESSAGEBOX_VISUAL_EXAMPLES.md](examples/MESSAGEBOX_VISUAL_EXAMPLES.md) - MessageBox visual examples
- [QUIT_BUTTON_EXAMPLE.md](examples/QUIT_BUTTON_EXAMPLE.md) - Quit button example
- [SDL3_USAGE_EXAMPLES.md](examples/SDL3_USAGE_EXAMPLES.md) - SDL3 usage examples
- [REMOTE_FILE_IO_QUICK_REFERENCE.md](examples/REMOTE_FILE_IO_QUICK_REFERENCE.md) - Remote file I/O quick reference

## Fixes

Bug fixes and issue resolutions:

See the [fixes](fixes/) directory for detailed information about bug fixes and issue resolutions.

## Analysis

Technical analysis and investigations:

See the [analysis](analysis/) directory for issue analysis and investigations.

## Diagrams

Visual diagrams and flow charts:

- [BUTTON_MESSAGE_FLOW.md](diagrams/BUTTON_MESSAGE_FLOW.md) - Button message flow
- [DIALOG_MESSAGE_LOOP.md](diagrams/DIALOG_MESSAGE_LOOP.md) - Dialog message loop
- [MESSAGEBOX_FLOW.md](diagrams/MESSAGEBOX_FLOW.md) - MessageBox flow
- [STACK_LAYOUT_DIAGRAM.md](diagrams/STACK_LAYOUT_DIAGRAM.md) - Stack layout diagram
- [SDL3_VISUAL_SUMMARY.md](diagrams/SDL3_VISUAL_SUMMARY.md) - SDL3 visual summary
- [COM_DELEGATE_PATTERN.md](diagrams/COM_DELEGATE_PATTERN.md) - COM delegate pattern
- [INT_INT3_FUNCTION_HOOKING.md](diagrams/INT_INT3_FUNCTION_HOOKING.md) - INT/INT3 function hooking

## Testing

Test documentation and coverage:

- [OPENTELEMETRY_TEST_RESULTS.md](testing/OPENTELEMETRY_TEST_RESULTS.md) - OpenTelemetry test results
- [THREEWAY_TEST_COVERAGE_IMPROVEMENTS.md](testing/THREEWAY_TEST_COVERAGE_IMPROVEMENTS.md) - Three-way test coverage improvements
- [UNIT_TEST_STATUS_REPORT.md](testing/UNIT_TEST_STATUS_REPORT.md) - Unit test status report
- [FIX_VERIFICATION.md](testing/FIX_VERIFICATION.md) - Fix verification

## GUI

GUI-related documentation:

See the [gui](gui/) directory for GUI-specific documentation.

## Tests

Test-specific documentation:

See the [tests](tests/) directory for test documentation.

## Tools

Tool-specific documentation:

See the [tools](tools/) directory for tool documentation.

## PR Summaries

Pull request summaries and change logs:

See the [pr-summaries](pr-summaries/) directory for PR summaries.
