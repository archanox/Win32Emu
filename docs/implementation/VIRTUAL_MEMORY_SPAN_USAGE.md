# VirtualMemory Memory\<T\> and Span\<T\> Usage Guide

## Overview

The `VirtualMemory` class has been enhanced to leverage .NET's modern `Memory<T>` and `Span<T>` types for improved performance and reduced allocations. This guide explains how to use these new APIs effectively.

## Why Memory\<T\> and Span\<T\>?

According to [Microsoft's documentation](https://learn.microsoft.com/en-us/dotnet/api/system.memory-1?view=net-9.0):

- **`Span<T>`**: A stack-allocated type that provides a view over a contiguous region of memory without allocations
- **`Memory<T>`**: A heap-allocated type that can be stored in fields and used in async methods
- Both types avoid copying data, improving performance in hot paths

## New APIs

### ReadBytes with Span\<byte\>

**Use this for zero-allocation reads when you can allocate the buffer on the stack or reuse an existing buffer.**

```csharp
// Stack allocation (best for small, fixed-size reads)
Span<byte> buffer = stackalloc byte[16];
memory.ReadBytes(0x1000, buffer);

// Reuse existing buffer (good for loops)
byte[] reusableBuffer = new byte[256];
for (int i = 0; i < 100; i++)
{
    memory.ReadBytes(baseAddr + (uint)i * 256, reusableBuffer);
    // Process buffer
}
```

**Benefits:**
- Zero heap allocations when using `stackalloc`
- Reduced GC pressure
- Better cache locality

### GetMemory

**Use this when you need a `Memory<byte>` for async operations or to store in a field.**

```csharp
Memory<byte> data = memory.GetMemory(0x2000, 100);
// Can be stored, passed to async methods, etc.
await ProcessDataAsync(data);
```

**Note:** This allocates a new array internally. For zero-copy access, use `TryGetPageMemory` when possible.

### TryGetPageMemory

**Use this for zero-copy access to single-page regions.**

```csharp
if (memory.TryGetPageMemory(0x1000, 64, out Memory<byte> pageData))
{
    // Direct access to the page without copying
    // This is the most efficient option when it works
    Span<byte> span = pageData.Span;
    // Process span
}
else
{
    // Region spans multiple pages or page not allocated
    // Fall back to GetSpan or ReadBytes
    var data = memory.GetSpan(0x1000, 64);
}
```

**Benefits:**
- Zero allocations
- Zero copy overhead
- Direct access to internal page buffer

**Limitations:**
- Only works when the requested region fits within a single 4KB page
- Returns false for unallocated pages

### GetSpan (Backward Compatible)

**This method is maintained for backward compatibility.**

```csharp
byte[] data = memory.GetSpan(0x3000, 100);
// Returns a newly allocated array
```

**When to use:**
- Existing code that expects `byte[]`
- When you need to store the result long-term
- When the calling code is not performance-critical

## Performance Guidelines

### Choose the Right API

1. **Hot path, small fixed-size reads**: Use `ReadBytes` with `stackalloc`
2. **Hot path, single-page access**: Use `TryGetPageMemory` 
3. **Async operations**: Use `GetMemory`
4. **Compatibility/convenience**: Use `GetSpan`

### Example: Instruction Decoding (Hot Path)

```csharp
// Before: Allocates a new array every time
byte[] instrBytes = memory.GetSpan(eip, 8);
decoder.Decode(instrBytes);

// After: Zero allocations
Span<byte> instrBytes = stackalloc byte[8];
memory.ReadBytes(eip, instrBytes);
decoder.Decode(instrBytes);
```

### Example: Processing Large Buffers

```csharp
// Before: Multiple allocations
for (int i = 0; i < pageCount; i++)
{
    var page = memory.GetSpan(baseAddr + i * 4096, 4096);
    ProcessPage(page);
}

// After: Single allocation, reused
byte[] pageBuffer = new byte[4096];
for (int i = 0; i < pageCount; i++)
{
    memory.ReadBytes(baseAddr + i * 4096, pageBuffer);
    ProcessPage(pageBuffer);
}

// Best: Zero allocations with TryGetPageMemory
for (int i = 0; i < pageCount; i++)
{
    if (memory.TryGetPageMemory(baseAddr + i * 4096, 4096, out var pageData))
    {
        ProcessPage(pageData.Span);
    }
}
```

## Page-Based Architecture

`VirtualMemory` uses 4KB pages internally. Understanding this helps optimize performance:

- **Page size**: 4096 bytes (4KB)
- **Page alignment**: Addresses are divided into page index and offset
- **Sparse allocation**: Only allocated pages consume memory

### Page Boundary Considerations

```csharp
// This fits in a single page (assuming aligned address)
memory.TryGetPageMemory(0x1000, 100, out var data); // Success

// This spans two pages - will fail
memory.TryGetPageMemory(0x1FFE, 100, out var data); // Fails (crosses boundary)

// Use ReadBytes for cross-page reads
Span<byte> buffer = stackalloc byte[100];
memory.ReadBytes(0x1FFE, buffer); // Works, handles page boundary
```

## Migration from GetSpan

To migrate existing code for better performance:

1. **Identify hot paths** using profiling
2. **Replace GetSpan with ReadBytes** in hot paths
3. **Use stackalloc for small buffers** (< 256 bytes typically)
4. **Try TryGetPageMemory first** for page-aligned access
5. **Keep GetSpan** in cold paths for simplicity

## Best Practices

1. **Use `Span<byte>` for synchronous APIs** (Rule #1 from Microsoft guidelines)
2. **Use `Memory<byte>` for async APIs** (Rule #10 from Microsoft guidelines)
3. **Prefer `stackalloc` for buffers < 256 bytes**
4. **Reuse buffers in loops** instead of allocating repeatedly
5. **Check page boundaries** when optimizing with `TryGetPageMemory`

## See Also

- [Microsoft Memory\<T\> Usage Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/memory-t-usage-guidelines)
- [Memory-related and span types](https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/)
- [What's new in .NET 9 - Spans](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/libraries#spans)
