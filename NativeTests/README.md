# Native Win32 API Test Executables

This directory contains simple C test programs for testing Win32 API functions in Win32Emu. These tests can be compiled with Visual C++ and run on both real Windows and in the Win32Emu emulator.

## Purpose

These test executables are designed to:
- Test specific Win32 API functions that are problematic or require validation
- Compare behavior between real Windows and Win32Emu implementation
- Provide simple, focused tests that are easier to debug than complex applications
- Serve as integration tests that can be run manually on both platforms

## Test Programs

### test_getmodulefilename.exe
Tests the `GetModuleFileNameA` function with various scenarios:
- Getting current module filename with NULL handle
- Testing buffer size limitations
- Getting kernel32.dll module filename
- Testing invalid handles
- Edge cases with zero buffer sizes

### test_environment.exe
Tests environment variable functions:
- `GetEnvironmentVariableA` - Reading environment variables
- `SetEnvironmentVariableA` - Setting and updating environment variables
- Testing non-existent variables
- Testing buffer size handling
- Deleting environment variables with NULL values

### test_heap.exe
Tests heap memory management functions (as used by ign_teas.exe):
- `HeapCreate` - Creating heaps with various flags (including HEAP_NO_SERIALIZE)
- `HeapAlloc` - Allocating memory with HEAP_ZERO_MEMORY flag
- `HeapFree` - Freeing allocated memory blocks
- `HeapDestroy` - Destroying heaps
- Multiple allocation and deallocation scenarios
- Writing to and reading from heap memory

### test_virtualalloc.exe
Tests virtual memory functions (as used by ign_teas.exe):
- `VirtualAlloc` - Reserving and committing memory
- `VirtualFree` - Releasing memory
- `VirtualQuery` - Querying memory information
- MEM_RESERVE and MEM_COMMIT operations
- Different page protection levels
- Writing to and reading from committed memory

### test_fileio.exe
Tests file I/O functions (as used by ign_teas.exe):
- `CreateFileA` - Creating and opening files
- `ReadFile` - Reading data from files
- `WriteFile` - Writing data to files
- `SetFilePointer` - Seeking in files (FILE_BEGIN, FILE_CURRENT, FILE_END)
- `GetFileType` - Determining file type
- `CloseHandle` - Closing file handles
- Error handling for invalid files and handles

### test_version.exe
Tests system version and code page functions (as used by ign_teas.exe):
- `GetVersion` - Getting Windows version information
- `GetACP` - Getting active code page
- `GetCPInfo` - Getting code page information for CP_ACP and CP_UTF8
- `SetHandleCount` - Legacy function for setting max handles
- `GetStdHandle` - Getting standard input/output/error handles
- `GetFileType` - Testing file type of standard handles

### test_commandline.exe
Tests command line and startup info functions (as used by ign_teas.exe):
- `GetCommandLineA` - Getting command line string
- `GetStartupInfoA` - Getting process startup information
- STARTF_USESTDHANDLES flag checking
- STARTF_USESHOWWINDOW flag checking
- Standard handle validation
- Consistency checks across multiple calls

### test_procaddress.exe
Tests module and function address functions (as used by ign_teas.exe):
- `GetModuleHandleA` - Getting module handles (KERNEL32, USER32, current module)
- `GetProcAddress` - Getting function addresses from modules
- `IsProcessorFeaturePresent` - Checking CPU features (FPU, MMX, etc.)
- Case-insensitive module name handling
- Function pointer invocation
- Error handling for invalid modules and functions

### test_messages.exe
Tests window message functions (as used by ign_teas.exe):
- `RegisterClassA` - Registering window classes
- `CreateWindowExA` - Creating message-only windows
- `PostMessageA` - Posting messages to windows
- `PeekMessageA` - Peeking at messages with PM_REMOVE and PM_NOREMOVE
- `GetMessageA` - Getting messages from queue
- `TranslateMessage` - Translating keyboard messages
- `DispatchMessageA` - Dispatching messages to window procedures
- `PostQuitMessage` - Posting quit messages
- WM_QUIT message handling

### test_multimedia.exe
Tests multimedia timer functions (as used by ign_teas.exe):
- `timeGetDevCaps` - Getting timer capabilities
- `timeBeginPeriod` - Setting timer resolution to 1ms
- `timeEndPeriod` - Restoring timer resolution
- `timeGetTime` - Getting system time in milliseconds
- Nested timeBeginPeriod/timeEndPeriod calls
- Timer consistency and accuracy testing
- Error handling for invalid timer periods

## Building on Windows

### Using Visual Studio
1. Open the solution file `Win32Emu.slnx` in Visual Studio
2. The projects are configured to build for Win32 (x86)
3. Build configuration: Debug or Release
4. Output directory: `EXEs/NativeTests/Debug/` or `EXEs/NativeTests/Release/`

### Using Visual Studio Command Prompt
```cmd
cd NativeTests
cl test_getmodulefilename.c /Fe:test_getmodulefilename.exe
cl test_environment.c /Fe:test_environment.exe
cl test_heap.c /Fe:test_heap.exe
cl test_virtualalloc.c /Fe:test_virtualalloc.exe
cl test_fileio.c /Fe:test_fileio.exe
cl test_version.c /Fe:test_version.exe
cl test_commandline.c /Fe:test_commandline.exe
cl test_procaddress.c /Fe:test_procaddress.exe
cl test_messages.c /Fe:test_messages.exe /link user32.lib
cl test_multimedia.c /Fe:test_multimedia.exe /link winmm.lib
```

### Using MinGW-w64 on Windows
```cmd
cd NativeTests
gcc -o test_getmodulefilename.exe test_getmodulefilename.c -lkernel32
gcc -o test_environment.exe test_environment.c -lkernel32
gcc -o test_heap.exe test_heap.c -lkernel32
gcc -o test_virtualalloc.exe test_virtualalloc.c -lkernel32
gcc -o test_fileio.exe test_fileio.c -lkernel32
gcc -o test_version.exe test_version.c -lkernel32
gcc -o test_commandline.exe test_commandline.c -lkernel32
gcc -o test_procaddress.exe test_procaddress.c -lkernel32
gcc -o test_messages.exe test_messages.c -lkernel32 -luser32
gcc -o test_multimedia.exe test_multimedia.c -lkernel32 -lwinmm
```

## Building on Linux (Cross-compilation)

You can cross-compile these tests on Linux using MinGW-w64:

```bash
cd NativeTests
i686-w64-mingw32-gcc -o test_getmodulefilename.exe test_getmodulefilename.c -lkernel32
i686-w64-mingw32-gcc -o test_environment.exe test_environment.c -lkernel32
i686-w64-mingw32-gcc -o test_heap.exe test_heap.c -lkernel32
i686-w64-mingw32-gcc -o test_virtualalloc.exe test_virtualalloc.c -lkernel32
i686-w64-mingw32-gcc -o test_fileio.exe test_fileio.c -lkernel32
i686-w64-mingw32-gcc -o test_version.exe test_version.c -lkernel32
i686-w64-mingw32-gcc -o test_commandline.exe test_commandline.c -lkernel32
i686-w64-mingw32-gcc -o test_procaddress.exe test_procaddress.c -lkernel32
i686-w64-mingw32-gcc -o test_messages.exe test_messages.c -lkernel32 -luser32
i686-w64-mingw32-gcc -o test_multimedia.exe test_multimedia.c -lkernel32 -lwinmm
```

Or use the provided Makefile:
```bash
cd NativeTests
make
```

## Running the Tests

### On Real Windows
```cmd
cd EXEs\NativeTests\Release
test_getmodulefilename.exe
test_environment.exe
test_heap.exe
test_virtualalloc.exe
test_fileio.exe
test_version.exe
test_commandline.exe
test_procaddress.exe
test_messages.exe
test_multimedia.exe
```

### In Win32Emu
```bash
dotnet run --project Win32Emu.Gui --no-build -- --nogui EXEs/NativeTests/Release/test_getmodulefilename.exe
dotnet run --project Win32Emu.Gui --no-build -- --nogui EXEs/NativeTests/Release/test_environment.exe
dotnet run --project Win32Emu.Gui --no-build -- --nogui EXEs/NativeTests/Release/test_heap.exe
dotnet run --project Win32Emu.Gui --no-build -- --nogui EXEs/NativeTests/Release/test_virtualalloc.exe
dotnet run --project Win32Emu.Gui --no-build -- --nogui EXEs/NativeTests/Release/test_fileio.exe
dotnet run --project Win32Emu.Gui --no-build -- --nogui EXEs/NativeTests/Release/test_version.exe
dotnet run --project Win32Emu.Gui --no-build -- --nogui EXEs/NativeTests/Release/test_commandline.exe
dotnet run --project Win32Emu.Gui --no-build -- --nogui EXEs/NativeTests/Release/test_procaddress.exe
dotnet run --project Win32Emu.Gui --no-build -- --nogui EXEs/NativeTests/Release/test_messages.exe
dotnet run --project Win32Emu.Gui --no-build -- --nogui EXEs/NativeTests/Release/test_multimedia.exe
```

## Expected Behavior

### GetModuleFileNameA Tests
- All tests should PASS on real Windows
- Win32Emu behavior should match Windows behavior:
  - NULL handle returns current module path
  - Small buffers return truncated paths with ERROR_INSUFFICIENT_BUFFER
  - Invalid handles return 0 with ERROR_INVALID_PARAMETER
  - Valid module handles return full paths

### Environment Variable Tests
- All tests should PASS on real Windows
- Win32Emu should use virtualized environment variables:
  - Variables set in the emulator don't affect the host OS
  - GetEnvironmentVariableA/SetEnvironmentVariableA work within the emulated environment
  - Non-existent variables return ERROR_ENVVAR_NOT_FOUND
  - Null value deletes the variable

### Heap Tests
- All tests should PASS on real Windows
- Win32Emu should properly manage heap allocations:
  - HeapCreate returns valid heap handles
  - HeapAlloc with HEAP_ZERO_MEMORY returns zeroed memory
  - Multiple allocations from same heap work correctly
  - HeapFree successfully frees allocated memory
  - HeapDestroy cleans up heap resources
  - HEAP_NO_SERIALIZE flag is properly supported (as used by ign_teas)

### VirtualAlloc Tests
- All tests should PASS on real Windows
- Win32Emu should handle virtual memory operations:
  - MEM_RESERVE reserves address space
  - MEM_COMMIT commits reserved pages
  - Combined MEM_RESERVE | MEM_COMMIT works in one call
  - Memory can be written to and read from after commit
  - VirtualFree properly releases memory
  - VirtualQuery returns correct memory information

### File I/O Tests
- All tests should PASS on real Windows
- Win32Emu should virtualize file operations:
  - CreateFileA creates and opens files
  - ReadFile and WriteFile transfer data correctly
  - SetFilePointer moves file pointer (BEGIN, CURRENT, END)
  - GetFileType returns correct file types
  - CloseHandle closes file handles
  - Error codes are set correctly for invalid operations

### Version and System Tests
- All tests should PASS on real Windows
- Win32Emu should return appropriate values:
  - GetVersion returns emulated Windows version (e.g., Windows 95 or XP)
  - GetACP returns active code page (typically 1252 or 65001 for UTF-8)
  - GetCPInfo provides code page information
  - SetHandleCount returns at least the requested count
  - GetStdHandle returns valid standard handles (or NULL if not available)
  - GetFileType correctly identifies standard handle types

### Command Line and Startup Info Tests
- All tests should PASS on real Windows
- Win32Emu should provide process information:
  - GetCommandLineA returns the command line used to launch the executable
  - GetStartupInfoA fills in startup information structure
  - Standard handles are available if STARTF_USESTDHANDLES is set
  - Information is consistent across multiple calls

### Module and Process Address Tests
- All tests should PASS on real Windows
- Win32Emu should support dynamic function loading:
  - GetModuleHandleA returns handles for loaded modules (KERNEL32, USER32, etc.)
  - GetProcAddress returns function pointers for exported functions
  - Function pointers can be called successfully
  - IsProcessorFeaturePresent reports CPU features correctly
  - Error handling for invalid modules and functions

### Message Tests
- All tests should PASS on real Windows
- Win32Emu should implement the message loop:
  - RegisterClassA registers window classes
  - CreateWindowExA creates windows (including message-only windows)
  - PostMessageA posts messages to windows
  - PeekMessageA retrieves messages without blocking
  - GetMessageA blocks until a message is available
  - TranslateMessage and DispatchMessageA process messages
  - PostQuitMessage posts WM_QUIT to terminate message loop

### Multimedia Tests
- All tests should PASS on real Windows
- Win32Emu should support multimedia timers:
  - timeGetDevCaps returns timer capabilities
  - timeBeginPeriod sets timer resolution (typically 1ms for games)
  - timeEndPeriod restores timer resolution
  - timeGetTime returns monotonically increasing time values
  - Nested timeBeginPeriod/timeEndPeriod calls are handled
  - Timer is consistent and accurate

## Comparing Results

To compare results between Windows and Win32Emu:

1. Run the tests on Windows and save output:
```cmd
test_getmodulefilename.exe > windows_getmodulefilename.txt
test_environment.exe > windows_environment.txt
```

2. Run the tests in Win32Emu and save output:
```bash
dotnet run --no-build --project Win32Emu.Gui -- --nogui test_getmodulefilename.exe > emu_getmodulefilename.txt
dotnet run --no-build --project Win32Emu.Gui -- --nogui test_environment.exe > emu_environment.txt
```

3. Compare the outputs to identify any discrepancies

## Known Differences

### Path Separators
- Windows uses backslashes (`\`) in paths
- Win32Emu may use forward slashes (`/`) or backslashes depending on configuration
- This is expected and not considered a failure

### Environment Variables
- Win32Emu uses a virtualized environment
- Default variables (like PATH, WINDIR) will have emulated values
- This is by design to isolate the emulated environment from the host

## Adding New Tests

To add a new test:
1. Create a new `.c` file in this directory
2. Create a corresponding `.vcxproj` file (copy and modify an existing one)
3. Add the project to the solution if desired
4. Update this README with test description
5. Ensure the test can run on both Windows and Win32Emu

## Related Issues

These tests were created to help validate the implementation of functions used by the ign_teas.exe application (Ignition TEAS game), particularly:
- Heap management functions (HeapCreate, HeapAlloc, HeapFree) - heavily used by the game
- Virtual memory functions (VirtualAlloc) - for large memory allocations
- File I/O functions (CreateFileA, ReadFile, SetFilePointer) - for loading game assets
- Version and code page functions (GetVersion, GetACP, GetCPInfo) - for initialization
- Command line and startup functions - for process initialization
- Module and function address functions - for dynamic API loading
- Window message functions - for the game's message loop
- Multimedia timer functions - for game timing and frame rate control

See also:
- `ApiMon Logs/ign_teas/ign_teas.exe.csv` - API call trace from the actual game
- `Win32Emu.Tests.Kernel32/IgnTeasRequiredFunctionsTests.cs` - C# unit tests for these functions
- `Win32Emu.Tests.ReactOS/IGN_TEAS_TESTS.md` - ReactOS/Wine test coverage documentation
