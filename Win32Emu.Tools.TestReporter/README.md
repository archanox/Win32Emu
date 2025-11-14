# SingleStep CPU Test Results

This directory contains the test reporter tool that generates GitHub Pages reports for SingleStep CPU conformance tests.

## Overview

The Win32Emu emulator includes comprehensive CPU conformance tests based on the [SingleStepTests/80386](https://github.com/SingleStepTests/80386) test suite. These are hardware-generated tests that validate CPU implementation against real 386 behavior.

This tool generates a report showing pass/fail status for each test file, published automatically to GitHub Pages.

## Report Contents

The generated report includes:

- **Summary Statistics**: Total files tested, tests run, pass/fail counts, and success rate
- **Per-File Results**: Pass/fail breakdown for each MOO test file
- **Detailed Data**: Full test results in JSON format for analysis
- **Search Functionality**: Filter test files by name

## Usage

### Running Locally

```bash
# Build the tool
dotnet build Win32Emu.Tools.TestReporter --configuration Release

# Generate report
dotnet run --project Win32Emu.Tools.TestReporter --configuration Release -- \
  <test-data-path> \
  <output-path> \
  <max-tests-per-file>
```

Example:
```bash
dotnet run --project Win32Emu.Tools.TestReporter --configuration Release -- \
  Win32Emu.Tests.Emulator/TestData/SingleStepTests \
  test-results \
  100
```

### Parameters

1. **test-data-path**: Path to the directory containing MOO test files (default: `Win32Emu.Tests.Emulator/TestData/SingleStepTests`)
2. **output-path**: Directory where report files will be generated (default: `test-results`)
3. **max-tests-per-file**: Maximum number of tests to run per MOO file (default: `100`)

### Output Files

- `index.html`: Interactive HTML report with test results
- `test-results.json`: Complete test data in JSON format

## GitHub Actions

The report is automatically generated and deployed to GitHub Pages:

- **Schedule**: Weekly on Mondays at 00:00 UTC
- **Manual**: Can be triggered via workflow_dispatch
- **Automatic**: On pushes to main that modify CPU or test code

### Viewing the Report

Once deployed, the reports are available at:
```
https://archanox.github.io/Win32Emu/         # Main landing page
https://archanox.github.io/Win32Emu/cpu-tests/   # CPU test results
https://archanox.github.io/Win32Emu/api-status.html  # API status
```

## Test Files

The SingleStep test suite contains 941 MOO files, each with hundreds to thousands of individual tests. Each MOO file tests specific x86 instructions or instruction variants.

Test file naming convention:
- `00.MOO.gz` - `FF.MOO.gz`: Basic x86 instructions (opcode-based)
- `0F80.MOO.gz` - `0F9F.MOO.gz`: Extended instructions (0F prefix)
- `66XX.MOO.gz`: 32-bit operand size prefix tests
- `67XX.MOO.gz`: 32-bit address size prefix tests

## Understanding Test Results

### Success Criteria

A test passes when the emulated CPU state exactly matches the expected state from real hardware after executing a single instruction:

- All register values match (EAX, EBX, ECX, EDX, ESI, EDI, EBP, ESP, EIP, EFLAGS)
- All modified memory locations match
- No execution errors occur

### Common Failure Types

- **EIP only**: Instruction length calculation issue
- **EFLAGS only**: Flag calculation error
- **Register value error**: Incorrect instruction execution
- **Memory error**: Memory write mismatch
- **Execution Error**: Instruction not implemented or crash

## Contributing

To improve CPU emulation accuracy:

1. Run the test reporter to identify failing tests
2. Analyze failure patterns in the JSON output
3. Fix CPU implementation in `Win32Emu/Cpu/Iced/IcedCpu.cs`
4. Run tests again to verify fixes
5. Submit a PR with improvements

## Technical Details

### Architecture

The test reporter:
1. Discovers all MOO.gz test files in the specified directory
2. For each file:
   - Parses the MOO file format (compressed test cases)
   - Runs up to N tests per file using `SingleStepTestRunner`
   - Collects pass/fail results and detailed mismatch information
3. Generates summary statistics
4. Outputs HTML report with interactive UI
5. Outputs JSON data for programmatic analysis

### Performance

- Processing ~94,000 tests (100 per file × 941 files) takes approximately 2-5 minutes
- JSON output is ~40MB (detailed test data)
- HTML output is ~500KB (summary only)

## Related Files

- `Win32Emu.Tests.Emulator/SingleStepTests/SingleStepConformanceTests.cs` - xUnit test class
- `Win32Emu.Tests.Emulator/SingleStepTests/SingleStepTestRunner.cs` - Test execution engine
- `Win32Emu.Tests.Emulator/SingleStepTests/MooFileParser.cs` - MOO file format parser
- `.github/workflows/cpu-test-results.yml` - GitHub Actions workflow
