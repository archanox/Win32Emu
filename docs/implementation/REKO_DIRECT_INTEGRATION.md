# Reko Direct Integration - Proof of Concept

## Overview

This document describes the **direct integration** of Reko.Decompiler.Runtime NuGet package into Win32Emu, removing the reflection-based approach in favor of direct API usage.

## Changes Made

### 1. Added Reko Package Dependency

**File**: `Win32Emu.Rtl/Win32Emu.Rtl.csproj`

Added direct package reference:
```xml
<PackageReference Include="Reko.Decompiler.Runtime" Version="0.11.6" />
```

### 2. Rewritten RekoDecompilerAdapter

**File**: `Win32Emu.Rtl/RekoDecompilerAdapter.cs`

**Changes**:
- Removed reflection-based API calls
- Added direct `using` statements for Reko namespaces:
  - `using Reko.Arch.X86;`
  - `using Reko.Core;`
  - `using Reko.Core.Memory;`
  - `using System.ComponentModel.Design;`
- Simplified implementation using direct API calls
- Removed environment variable check (always available now)
- Updated licensing notice to "Proof of Concept"

**Key improvements:**
- No more reflection overhead
- Cleaner, more readable code
- Direct use of Reko types and methods
- Proper error handling maintained

## Usage

Now Reko is always available since it's a direct dependency. To use Reko:

```bash
# Set environment variable to select Reko adapter
export WIN32EMU_USE_REKO=true

# Run emulator
dotnet run --project Win32Emu.Gui -- --nogui game.exe
```

Without the environment variable, it defaults to CustomRTL (MIT-licensed).

Now let me commit these changes and reply to the user:

<function_calls>
<invoke name="report_progress">
<parameter name="commitMessage">Add Reko.Decompiler.Runtime as direct dependency and remove reflection-based loading