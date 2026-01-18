# Child Process Execution Support

This document describes how to use the child process execution feature in Win32Emu.

## Overview

Win32Emu now supports basic child process execution through the `WinExec` and `ShellExecuteA` Win32 API functions. When a program requests to launch another executable, the emulator:

1. Resolves the executable path (absolute or relative)
2. Creates a `ChildProcessRequest` object
3. Exits the main execution loop
4. Returns control to the caller

The caller (CLI or GUI host) can then check for pending child process requests and handle them appropriately.

## Use Cases

This feature is designed for scenarios where:
- An installer (autorun.exe) launches a setup program (setup.exe)
- A launcher executable starts the main application
- A program chain-executes another program and doesn't need to continue running

## API Functions Supported

### WinExec
```c
UINT WinExec(
  LPCSTR lpCmdLine,  // Command line string
  UINT   uCmdShow    // Window show state
);
```

The function parses the command line to extract:
- Executable path (with quoted path support)
- Command line arguments
- Resolves relative paths to current directory

### ShellExecuteA
```c
HINSTANCE ShellExecuteA(
  HWND   hwnd,        // Window handle (ignored)
  LPCSTR lpOperation, // Operation to perform ("open", etc.)
  LPCSTR lpFile,      // File to execute
  LPCSTR lpParameters,// Parameters/arguments
  LPCSTR lpDirectory, // Working directory
  INT    nShowCmd     // Window show state
);
```

Only the "open" operation is implemented for executable files. Other operations return success without action.

## Usage Pattern

### 1. Check for Pending Requests

After `emulator.Run()` completes, check if a child process was requested:

```csharp
using var emulator = new Emulator(host, logger);
emulator.LoadExecutable(exePath);
emulator.Run();

// Check for child process request
var childRequest = emulator.GetPendingChildProcessRequest();
if (childRequest != null)
{
    Console.WriteLine($"Child process requested: {childRequest.ExecutablePath}");
    Console.WriteLine($"Command line: {childRequest.CommandLine}");
    Console.WriteLine($"Working directory: {childRequest.WorkingDirectory}");
    Console.WriteLine($"Show command: {childRequest.ShowCommand}");
    
    // Handle the child process...
}
```

### 2. Load and Run Child Process

You have several options for handling child process requests:

#### Option A: Sequential Execution (Recommended for autorun→setup scenario)

```csharp
var childRequest = emulator.GetPendingChildProcessRequest();
if (childRequest != null)
{
    // Dispose the parent emulator
    emulator.Dispose();
    
    // Create new emulator for child process
    using var childEmulator = new Emulator(host, logger);
    
    // Resolve child executable path in VFS
    var childExePath = ResolvePathInVfs(childRequest.ExecutablePath);
    
    // Load and run child
    childEmulator.LoadExecutable(childExePath);
    childEmulator.Run();
}
```

#### Option B: Recursive Execution

For automated testing or batch processing, you can recursively handle child processes:

```csharp
void RunWithChildProcessSupport(string exePath)
{
    using var emulator = new Emulator(host, logger);
    emulator.LoadExecutable(exePath);
    emulator.Run();
    
    var childRequest = emulator.GetPendingChildProcessRequest();
    if (childRequest != null)
    {
        // Recursively run the child process
        var childExePath = ResolvePathInVfs(childRequest.ExecutablePath);
        RunWithChildProcessSupport(childExePath);
    }
}
```

#### Option C: User Prompt (GUI Applications)

```csharp
var childRequest = emulator.GetPendingChildProcessRequest();
if (childRequest != null)
{
    var result = MessageBox.Show(
        $"The program wants to launch:\n{childRequest.ExecutablePath}\n\nContinue?",
        "Child Process Request",
        MessageBoxButtons.YesNo);
        
    if (result == DialogResult.Yes)
    {
        // Load and run child process
        LoadAndRunChildProcess(childRequest);
    }
}
```

## Path Resolution

The implementation automatically handles:

### Absolute Paths
```c
WinExec("C:\\Windows\\System32\\notepad.exe", SW_SHOW);
// Resolved to: C:\Windows\System32\notepad.exe
```

### Relative Paths
```c
// Current directory: C:\Install
WinExec("setup.exe", SW_SHOW);
// Resolved to: C:\Install\setup.exe
```

### Quoted Paths with Arguments
```c
WinExec("\"C:\\Program Files\\MyApp\\app.exe\" arg1 arg2", SW_SHOW);
// Executable: C:\Program Files\MyApp\app.exe
// Full command line: "C:\Program Files\MyApp\app.exe" arg1 arg2
```

### Path with Subdirectories
```c
// Current directory: C:\Install
WinExec("tools\\setup.exe", SW_SHOW);
// Resolved to: C:\Install\tools\setup.exe
```

## Virtual File System Integration

When using a Virtual File System (VFS), you need to resolve paths correctly:

```csharp
string ResolvePathInVfs(string windowsPath)
{
    // If using DiskVirtualFileSystem with a VHD/ISO
    if (vfs != null && vfs.FileExists(windowsPath))
    {
        // Path exists in VFS, extract to temp location or handle in VFS
        return ExtractFromVfs(windowsPath);
    }
    
    // Otherwise, might be a native file path
    return windowsPath;
}
```

## Example: Autorun.exe → Setup.exe

Here's a complete example for handling the autorun→setup scenario:

```csharp
public void RunInstaller(string autorunPath)
{
    var logger = CreateLogger();
    var host = CreateEmulatorHost();
    
    // Run autorun.exe
    using var emulator = new Emulator(host, logger);
    emulator.LoadExecutable(autorunPath);
    emulator.Run();
    
    // Check if autorun wants to launch setup.exe
    var childRequest = emulator.GetPendingChildProcessRequest();
    if (childRequest != null)
    {
        logger.LogInformation("Autorun.exe requested to launch: {Path}", 
            childRequest.ExecutablePath);
        
        // Typically setup.exe is in same directory as autorun.exe
        var setupPath = Path.Combine(
            Path.GetDirectoryName(autorunPath),
            Path.GetFileName(childRequest.ExecutablePath));
        
        if (File.Exists(setupPath))
        {
            // Run setup.exe
            using var setupEmulator = new Emulator(host, logger);
            setupEmulator.LoadExecutable(setupPath);
            setupEmulator.Run();
        }
        else
        {
            logger.LogError("Setup executable not found: {Path}", setupPath);
        }
    }
}
```

## Limitations

### Current Implementation

1. **No Parallel Execution**: Child processes don't run in parallel with the parent. The parent must exit before the child starts.

2. **No Parent-Child Communication**: Child processes don't inherit handles or have any communication with the parent process.

3. **Single Child Only**: Only one child process request can be pending at a time. The `ChildProcessRequest` is replaced if multiple calls are made.

4. **No Process Enumeration**: Tools like Task Manager that enumerate running processes won't see child processes as separate entities.

### Not Yet Implemented

- `CreateProcessA` / `CreateProcessW` (stub returns ERROR_ACCESS_DENIED)
- Handle inheritance (`bInheritHandles` parameter)
- Process wait functions (`WaitForSingleObject` on process handles)
- Process exit codes from child processes
- Environment block inheritance
- True multi-process execution with context switching

## Error Handling

### Invalid Paths

If a non-existent executable is requested:
- The function returns success (per Win32 API behavior)
- The caller attempts to load the executable
- `LoadExecutable` will throw `FileNotFoundException`

```csharp
try
{
    var childRequest = emulator.GetPendingChildProcessRequest();
    if (childRequest != null)
    {
        childEmulator.LoadExecutable(childRequest.ExecutablePath);
        childEmulator.Run();
    }
}
catch (FileNotFoundException ex)
{
    logger.LogError("Child executable not found: {Path}", ex.FileName);
    // Handle error appropriately
}
```

### Null Command Lines

`WinExec` with a null command line returns `ERROR_FILE_NOT_FOUND` and doesn't create a child process request.

## Testing

The implementation includes comprehensive unit tests in `Win32Emu.Tests.Kernel32/ChildProcessTests.cs`:

```bash
# Run child process tests
dotnet test --filter "FullyQualifiedName~ChildProcessTests"
```

Tests cover:
- Path resolution (absolute, relative, quoted)
- Parameter parsing
- Error handling
- ShellExecuteA operations
- Cross-platform path separators

## Future Enhancements

Potential improvements for full multi-process support:

1. **Process Manager**: Track multiple process instances with PIDs
2. **Handle Inheritance**: Support `DuplicateHandle` and inherited handles
3. **Context Switching**: Allow parent process to continue while child runs
4. **IPC**: Implement pipes, shared memory, or other IPC mechanisms
5. **Exit Codes**: Track and return child process exit codes
6. **Environment Inheritance**: Copy parent environment to child

## See Also

- [README.md](../README.md) - Main project documentation
- [Win32 API Documentation](https://docs.microsoft.com/en-us/windows/win32/api/) - Official Win32 API reference
- [ProcessEnvironment.cs](../Win32Emu/Win32/ProcessEnvironment.cs) - Process environment implementation
- [Emulator.cs](../Win32Emu/Emulator.cs) - Main emulator class
