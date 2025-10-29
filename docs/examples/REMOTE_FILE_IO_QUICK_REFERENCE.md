# Remote File I/O Quick Reference

## Overview

The GDB server now supports remote file I/O operations, allowing Ghidra and other debuggers to access files in Win32Emu's Virtual File System.

## Quick Start

### 1. Enable VFS and Start GDB Server

```csharp
// In your emulator setup
var processEnv = new ProcessEnvironment(virtualMemory);

// Initialize VFS with game directory
processEnv.InitializeVirtualFileSystem(
    baseDirectory: @"C:\Games\MyGame",
    overlayDirectory: @"C:\Users\Me\AppData\Local\Win32Emu\MyGame"
);

// Start with GDB server
emulator.LoadExecutable("game.exe", gdbServerMode: true);
```

### 2. Connect from Ghidra

```
Debugger → Connect to Target → gdb
Host: localhost
Port: 1234
```

### 3. Use File I/O from Ghidra Scripts

```python
# Example: Read game config file
import gdb

# Convert filename to hex
filename_hex = "config.ini".encode('utf-8').hex()

# Open file (flags: O_RDONLY=0, mode: 0644=0x1a4)
result = gdb.execute(f"monitor vFile:open:{filename_hex},0,1a4", to_string=True)
fd = int(result.split('F')[1], 16)  # Extract file descriptor

# Read 1024 bytes from offset 0
result = gdb.execute(f"monitor vFile:pread:{fd:x},400,0", to_string=True)
parts = result.split(';')
hex_data = parts[1] if len(parts) > 1 else ""
data = bytes.fromhex(hex_data).decode('utf-8', errors='ignore')

print(f"File contents:\n{data}")

# Close file
gdb.execute(f"monitor vFile:close:{fd:x}", to_string=True)
```

## Supported Commands

| Command | Format | Description |
|---------|--------|-------------|
| vFile:open | `vFile:open:filename_hex,flags,mode` | Open a file |
| vFile:close | `vFile:close:fd_hex` | Close a file |
| vFile:pread | `vFile:pread:fd_hex,count_hex,offset_hex` | Read from file |
| vFile:pwrite | `vFile:pwrite:fd_hex,offset_hex,data_hex` | Write to file |
| vFile:fstat | `vFile:fstat:fd_hex` | Get file info |
| vFile:unlink | `vFile:unlink:filename_hex` | Delete a file |

## Common Use Cases

### Read a Configuration File

```python
def read_file(filename):
    import gdb
    filename_hex = filename.encode('utf-8').hex()
    result = gdb.execute(f"monitor vFile:open:{filename_hex},0,1a4", to_string=True)
    fd = int(result.split('F')[1], 16)
    
    # Read entire file (assuming < 4KB)
    result = gdb.execute(f"monitor vFile:pread:{fd:x},1000,0", to_string=True)
    parts = result.split(';')
    data = bytes.fromhex(parts[1]).decode('utf-8', errors='ignore')
    
    gdb.execute(f"monitor vFile:close:{fd:x}", to_string=True)
    return data

# Usage
config = read_file("config.ini")
print(config)
```

### Extract a Game Asset

```python
def extract_file(source_file, dest_file):
    import gdb
    
    # Open source file
    filename_hex = source_file.encode('utf-8').hex()
    result = gdb.execute(f"monitor vFile:open:{filename_hex},0,1a4", to_string=True)
    fd = int(result.split('F')[1], 16)
    
    # Get file size
    result = gdb.execute(f"monitor vFile:fstat:{fd:x}", to_string=True)
    parts = result.split(';')
    stat_data = bytes.fromhex(parts[1])
    file_size = int.from_bytes(stat_data[28:36], byteorder='little')
    
    # Read file in chunks
    chunk_size = 4096
    offset = 0
    data = b''
    
    while offset < file_size:
        to_read = min(chunk_size, file_size - offset)
        result = gdb.execute(f"monitor vFile:pread:{fd:x},{to_read:x},{offset:x}", to_string=True)
        parts = result.split(';')
        chunk = bytes.fromhex(parts[1])
        data += chunk
        offset += len(chunk)
    
    gdb.execute(f"monitor vFile:close:{fd:x}", to_string=True)
    
    # Write to local file
    with open(dest_file, 'wb') as f:
        f.write(data)
    
    return len(data)

# Usage
bytes_extracted = extract_file("assets/texture.dds", "extracted_texture.dds")
print(f"Extracted {bytes_extracted} bytes")
```

### List Files Being Accessed

```python
# Monitor file opens by setting breakpoints on CreateFileA
import gdb

class FileOpenBreakpoint(gdb.Breakpoint):
    def __init__(self):
        super().__init__("CreateFileA")
    
    def stop(self):
        # Read filename from stack (first parameter)
        esp = int(gdb.parse_and_eval("$esp"))
        filename_ptr = int(gdb.parse_and_eval(f"*(uint32_t*)({esp}+4)"))
        
        # Read null-terminated string
        filename = ""
        offset = 0
        while True:
            byte = int(gdb.parse_and_eval(f"*(uint8_t*)({filename_ptr}+{offset})"))
            if byte == 0:
                break
            filename += chr(byte)
            offset += 1
        
        print(f"[File Access] Opening: {filename}")
        return False  # Don't stop execution

# Set up monitoring
FileOpenBreakpoint()
gdb.execute("continue")
```

## POSIX Flags Reference

### Open Flags

| Flag | Value (hex) | Description |
|------|-------------|-------------|
| O_RDONLY | 0x0000 | Read-only |
| O_WRONLY | 0x0001 | Write-only |
| O_RDWR | 0x0002 | Read-write |
| O_CREAT | 0x0100 | Create if doesn't exist |
| O_TRUNC | 0x0200 | Truncate to zero length |
| O_EXCL | 0x0400 | Fail if file exists (with O_CREAT) |
| O_APPEND | 0x0800 | Append mode (not implemented) |

### File Modes

Common mode values:
- `0x1a4` (644) - rw-r--r-- (owner can read/write, others can read)
- `0x1b6` (666) - rw-rw-rw- (everyone can read/write)
- `0x1ed` (755) - rwxr-xr-x (owner can execute)

## Error Codes

| Error | Code | Description |
|-------|------|-------------|
| EPERM | 1 | Operation not permitted |
| ENOENT | 2 | No such file or directory |
| EIO | 5 | I/O error |
| EBADF | 9 | Bad file descriptor |
| EINVAL | 22 | Invalid argument |

Error responses are formatted as: `F-1,<errno_hex>`

## Response Formats

### Success Responses

- `F<value_hex>` - Operation succeeded, value returned
- `F<length_hex>;<hex_data>` - Data returned with length

### Error Responses

- `F-1,<errno_hex>` - Operation failed with error code

### Examples

```
# Successful open (fd=3)
F3

# Successful read (16 bytes)
F10;48656c6c6f20576f726c64210a

# File not found error
F-1,2

# Bad file descriptor error
F-1,9
```

## Troubleshooting

### "Empty response" from vFile commands

- Ensure VFS is initialized before starting GDB server
- Check that files exist in the base directory
- Verify file paths are relative to VFS base directory

### File changes not reflected

- Remember VFS uses copy-on-write
- Writes go to overlay directory
- Check overlay directory for modified files

### Cannot write to files

- Ensure file is opened with write flags (O_WRONLY or O_RDWR)
- Check file permissions in overlay directory

### File operations hang

- File I/O is synchronous - large files may take time
- Consider reading/writing in smaller chunks
- Check network connectivity to GDB server

## See Also

- [GDB_SERVER_GUIDE.md](../guides/GDB_SERVER_GUIDE.md) - Complete GDB server guide
- [VFS_DOCUMENTATION.md](../guides/VFS_DOCUMENTATION.md) - Virtual File System documentation
- [REMOTE_FILE_IO_IMPLEMENTATION.md](../implementation/REMOTE_FILE_IO_IMPLEMENTATION.md) - Implementation details
