# Archived Documentation

This directory contains historical documentation that has been archived for reference purposes. These documents describe completed features, historical bug fixes, and development analysis that may be useful for understanding the project's evolution but are not actively maintained.

## Contents

### Historical Implementation Summaries
- `IMPLEMENTATION_COMPLETE.md` - CPU Backend UI setting completion summary
- `IMPLEMENTATION_SUMMARY.md` - Original Avalonia GUI implementation summary
- `PHASE2_IMPLEMENTATION.md` - Phase 2: Window Management implementation
- `PHASE2_SUMMARY.md` - Phase 2: Pentium CPU instructions (60 instructions total)
- `PHASE3_IMPLEMENTATION.md` - Phase 3: Message Loop and Window Display
- `PHASE3_COMPLETION.md` - Phase 3: JitCpu Pentium Implementation (109 instructions)

### Game-Specific Analysis
Documents related to specific game compatibility testing and debugging:
- **IGN_TEAS.EXE** - Ignition game analysis (8 documents)
- **Issue #17** - Ignition (1997) DLL import tracking (4 documents)
- **CPU-Z** - CPU-Z compatibility analysis (3 documents)

### Development Tool Analysis
- `APIMON_*.md` - API Monitor log analysis and cross-referencing
- `DECOMPILATION_*.md` - Decompilation findings and reviews

### Historical Bug Fixes
Detailed documentation of specific bug fixes that have been resolved:
- Stack and register management fixes (EBP, ESP, stack corruption)
- DirectDraw rendering fixes
- Game-specific compatibility fixes
- API implementation fixes

### Investigations
- `ARGBYTES_INVESTIGATION.md` - Stack cleanup and argument bytes investigation
- `HEAPALLOC_INVESTIGATION.md` - Heap allocation analysis
- `THREAD_EXE_INVESTIGATION.md` - Threading investigation
- `INVESTIGATION_SUMMARY.md` - Overall investigation summary

## Why These Are Archived

These documents were created during active development to track:
- Specific PR implementations
- Bug fix investigations
- Game compatibility debugging
- Progressive feature development (phases)

While valuable for historical context, they are not actively maintained and may reference code that has since evolved. For current documentation, see the main [docs/](../) directory.

## Usage

These documents can be useful for:
- Understanding the project's development history
- Reference when debugging similar issues
- Learning about past architectural decisions
- Historical context for current features
