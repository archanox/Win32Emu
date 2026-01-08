# Using DecompToCS with ign_teas - Practical Guide

This guide demonstrates how to use `Win32Emu.Tools.DecompToCS` to investigate initialization issues in the ign_teas game.

## Quick Start

### Step 1: Generate C# Skeleton from Decompilation

```bash
cd /path/to/Win32Emu

# Generate C# code from Hex-Rays decompilation
dotnet run --project Win32Emu.Tools.DecompToCS -- \
    Decomp/ign_teas/hexrays.cpp \
    --output ./IgNTeasCS \
    --exe EXEs/ign_teas.exe \
    --namespace IgNTeas.Generated \
    --verbose
```

**Expected output:**
```
Win32Emu Decompilation to C# Transpiler
Input: Decomp/ign_teas/hexrays.cpp
Output: ./IgNTeasCS
Namespace: IgNTeas.Generated
Detected decompiler format: hexrays
Read 12555 bytes from decompilation file
Parsed 156 functions
Generated 156 C# files

Transpilation complete!
Output directory: /path/to/IgNTeasCS

Next steps:
  1. Build: cd /path/to/IgNTeasCS && dotnet build
  2. Debug: Open in Visual Studio and set breakpoints in Function_*.cs files
```

### Step 2: Examine Generated Files

The tool generates a C# project with skeleton classes:

```bash
cd IgNTeasCS
ls -la
```

**Contents:**
```
IgNTeas.Generated.csproj
Function_00401000.cs
Function_00401080.cs
Function_004011A0.cs
Function_00403070.cs   # WinMain
Function_004032A0.cs   # Initialization function
...
```

### Step 3: Examine a Key Function

Let's look at the initialization function that causes issues:

**From `hexrays.cpp` (line ~4850):**

```cpp
int sub_4032A0()
{
    HINSTANCE hInstance = (HINSTANCE)dword_41C7AC;
    
    // Register window class
    WNDCLASSA wc;
    wc.style = CS_HREDRAW | CS_VREDRAW;
    wc.lpfnWndProc = sub_403340;  // Window procedure
    wc.cbClsExtra = 0;
    wc.cbWndExtra = 0;
    wc.hInstance = hInstance;
    wc.hIcon = LoadIconA(hInstance, (LPCSTR)0x80);
    wc.hCursor = LoadCursorA(0, IDC_ARROW);
    wc.hbrBackground = (HBRUSH)GetStockObject(BLACK_BRUSH);
    wc.lpszMenuName = 0;
    wc.lpszClassName = "IGN_TEAS";
    
    if (!RegisterClassA(&wc))
        return 0;
    
    // Create window
    HWND hWnd = CreateWindowExA(
        0,
        "IGN_TEAS",
        "IGN Teas",
        WS_POPUP,
        0, 0,
        GetSystemMetrics(SM_CXSCREEN),
        GetSystemMetrics(SM_CYSCREEN),
        0, 0,
        hInstance,
        0
    );
    
    if (!hWnd)
        return 0;
    
    dword_41C7B0 = (DWORD)hWnd;
    
    ShowWindow(hWnd, SW_SHOW);
    UpdateWindow(hWnd);
    
    // Initialize DirectX components
    if (!sub_403540())  // DirectDraw init
        return 0;
        
    if (!sub_403560())  // DirectInput init
        return 0;
        
    if (!sub_403570())  // DirectSound init
        return 0;
    
    return 1;
}
```

**Generated C# skeleton** (`Function_004032A0.cs`):

```csharp
using System;
using Win32Emu;

namespace IgNTeas.Generated
{
    /// <summary>
    /// Function at 0x004032A0
    /// Original name: sub_4032A0
    /// Note: Main initialization function
    /// Decompiled from C++ and transpiled to C#
    /// </summary>
    public class Function_004032A0
    {
        private readonly EmulatorEnvironment _env;
        
        public Function_004032A0(EmulatorEnvironment env)
        {
            _env = env;
        }
        
        /// <summary>
        /// Execute function at 0x004032A0
        /// </summary>
        [OriginalAddress(0x004032A0)]
        public int Execute()
        {
            // TODO: Implementation needs to be extracted from decompilation
            // This is a placeholder for manual implementation
            throw new NotImplementedException("Function implementation not yet transpiled");
        }
    }
}
```

## Manual Implementation Example

Now we can manually implement the initialization function using the decompiled C++ as a reference:

**Manually enhanced** `Function_004032A0.cs`:

```csharp
using System;
using Win32Emu;

namespace IgNTeas.Generated
{
    /// <summary>
    /// Function at 0x004032A0 - Main initialization
    /// Registers window class, creates window, initializes DirectX
    /// </summary>
    public class Function_004032A0
    {
        private readonly EmulatorEnvironment _env;
        
        public Function_004032A0(EmulatorEnvironment env)
        {
            _env = env;
        }
        
        [OriginalAddress(0x004032A0)]
        public int Execute()
        {
            _env.Logger.LogInformation("[ign_teas] Initialization starting at 0x004032A0");
            
            uint hInstance = _env.MemRead32(0x0041C7AC);  // Global variable dword_41C7AC
            
            // Register window class
            _env.Logger.LogDebug("[ign_teas] Registering window class");
            
            var wndClassResult = _env.CallWin32Api("User32.RegisterClassA", new 
            {
                style = 0x0003u,  // CS_HREDRAW | CS_VREDRAW
                lpfnWndProc = 0x00403340u,  // Window procedure address
                cbClsExtra = 0,
                cbWndExtra = 0,
                hInstance = hInstance,
                hIcon = _env.CallWin32Api<uint>("User32.LoadIconA", hInstance, 0x80u),
                hCursor = _env.CallWin32Api<uint>("User32.LoadCursorA", 0u, 32512u), // IDC_ARROW
                hbrBackground = _env.CallWin32Api<uint>("Gdi32.GetStockObject", 4), // BLACK_BRUSH
                lpszMenuName = 0u,
                lpszClassName = "IGN_TEAS"
            });
            
            if (wndClassResult == 0)
            {
                _env.Logger.LogError("[ign_teas] RegisterClassA failed");
                return 0;
            }
            
            // Create window
            _env.Logger.LogDebug("[ign_teas] Creating window");
            
            uint screenWidth = _env.CallWin32Api<uint>("User32.GetSystemMetrics", 0);  // SM_CXSCREEN
            uint screenHeight = _env.CallWin32Api<uint>("User32.GetSystemMetrics", 1); // SM_CYSCREEN
            
            uint hWnd = _env.CallWin32Api<uint>("User32.CreateWindowExA",
                0u,                    // dwExStyle
                "IGN_TEAS",           // lpClassName
                "IGN Teas",           // lpWindowName
                0x80000000u,          // dwStyle = WS_POPUP
                0, 0,                 // x, y
                screenWidth,          // width
                screenHeight,         // height
                0u,                   // hWndParent
                0u,                   // hMenu
                hInstance,            // hInstance
                0u                    // lpParam
            );
            
            if (hWnd == 0)
            {
                _env.Logger.LogError("[ign_teas] CreateWindowExA failed");
                return 0;
            }
            
            // Store window handle in global variable
            _env.MemWrite32(0x0041C7B0, hWnd);  // dword_41C7B0 = hWnd
            
            // Show window
            _env.CallWin32Api("User32.ShowWindow", hWnd, 1); // SW_SHOW
            _env.CallWin32Api("User32.UpdateWindow", hWnd);
            
            // Initialize DirectX components
            _env.Logger.LogDebug("[ign_teas] Initializing DirectDraw");
            var ddResult = CallFunction(0x00403540);  // DirectDraw init
            if (ddResult == 0)
            {
                _env.Logger.LogError("[ign_teas] DirectDraw initialization failed");
                return 0;
            }
            
            _env.Logger.LogDebug("[ign_teas] Initializing DirectInput");
            var diResult = CallFunction(0x00403560);  // DirectInput init
            if (diResult == 0)
            {
                _env.Logger.LogError("[ign_teas] DirectInput initialization failed");
                return 0;
            }
            
            _env.Logger.LogDebug("[ign_teas] Initializing DirectSound");
            var dsResult = CallFunction(0x00403570);  // DirectSound init
            if (dsResult == 0)
            {
                _env.Logger.LogError("[ign_teas] DirectSound initialization failed");
                return 0;
            }
            
            _env.Logger.LogInformation("[ign_teas] Initialization completed successfully");
            return 1;
        }
        
        private int CallFunction(uint address)
        {
            // This would call into the emulator to execute the function at the given address
            // For now, we log and return success
            _env.Logger.LogDebug($"[ign_teas] Would call function at 0x{address:X8}");
            return 1; // Assume success for demonstration
        }
    }
}
```

## Debugging Workflow

### Step 1: Build the C# Project

```bash
cd IgNTeasCS
dotnet build
```

### Step 2: Create Test Harness

Create a test program that uses the generated functions:

**`TestInitialization.cs`:**

```csharp
using IgNTeas.Generated;
using Win32Emu;

class TestInitialization
{
    static void Main()
    {
        // Create mock emulator environment
        var env = CreateMockEnvironment();
        
        // Create and execute initialization function
        var initFunc = new Function_004032A0(env);
        
        Console.WriteLine("Testing ign_teas initialization...");
        var result = initFunc.Execute();
        
        if (result == 1)
        {
            Console.WriteLine("✓ Initialization succeeded");
        }
        else
        {
            Console.WriteLine("✗ Initialization failed");
        }
    }
    
    static EmulatorEnvironment CreateMockEnvironment()
    {
        // Create a mock environment that logs all Win32 API calls
        // This helps us see what the game is trying to do
        // ... implementation details ...
    }
}
```

### Step 3: Debug in Visual Studio

1. Open `IgNTeasCS.sln` in Visual Studio
2. Set breakpoint in `Function_004032A0.Execute()` at line: `_env.CallWin32Api("User32.RegisterClassA", ...)`
3. Press F5 to run with debugging
4. When breakpoint hits, inspect:
   - `hInstance` value
   - Watch window to see what parameters are being passed
   - Call stack to see how we got here
5. Step through (F10) to see each API call
6. Compare with emulator logs to find discrepancies

### Step 4: Compare with Emulator Execution

Run the actual game in the emulator with debug logging:

```bash
Win32Emu.Gui --nogui EXEs/ign_teas.exe --debug --log-file ign_teas_debug.log
```

Then compare:
1. **C# execution**: What parameters are passed to Win32 APIs
2. **Emulator execution**: What the emulator sees in the log
3. **Find differences**: Where do they diverge?

## Key Functions to Implement

Based on the ign_teas investigation, these functions are most important:

1. **`Function_004032A0`** - Main initialization (shown above)
2. **`Function_00403540`** - DirectDraw initialization
3. **`Function_00403560`** - DirectInput initialization
4. **`Function_00403570`** - DirectSound initialization
5. **`Function_00403070`** - WinMain entry point
6. **`Function_00403340`** - Window procedure (message handler)

## Benefits of This Approach

### What We Learn

1. **Initialization sequence**
   - Exact order of Win32 API calls
   - Parameters being passed
   - Expected return values

2. **Global state**
   - Memory addresses of global variables
   - Values stored at critical points
   - State dependencies

3. **Error handling**
   - What errors the game checks for
   - How it handles failures
   - What success looks like

### How It Helps Debugging

1. **Set targeted breakpoints**
   - Stop right before problematic API call
   - Inspect parameters before call
   - Step into emulator implementation

2. **Compare behavior**
   - Run C# version to see expected behavior
   - Run emulator to see actual behavior
   - Find the difference

3. **Fix issues**
   - Once difference is identified, fix emulator
   - Re-run C# test to verify fix
   - Confirm game works in emulator

## Example: Finding a Bug

Let's say the game fails during initialization. Using this approach:

**Step 1: Run C# version**
```
[ign_teas] Initialization starting at 0x004032A0
[ign_teas] Registering window class
[ign_teas] Creating window
[ign_teas] Window handle: 0x00010001
[ign_teas] Initializing DirectDraw
[ign_teas] DirectDraw initialization failed
✗ Initialization failed
```

**Step 2: Set breakpoint in `Function_00403540` (DirectDraw init)**

```csharp
public int Execute()
{
    // Breakpoint here
    var ddResult = _env.CallWin32Api<uint>("DirectDrawCreate", 
        0u,           // lpGUID (NULL)
        out lpDD,     // &lpDD
        0u            // pUnkOuter
    );
    
    if (ddResult != 0)
    {
        _env.Logger.LogError($"DirectDrawCreate failed: 0x{ddResult:X8}");
        return 0;
    }
    // ...
}
```

**Step 3: Inspect at breakpoint**
- Parameters look correct
- But emulator's DirectDrawCreate returns error code

**Step 4: Check emulator implementation**
- Look at `DDrawModule.cs` implementation
- Find that it's not properly initializing the COM object
- Fix the bug

**Step 5: Verify**
- Re-run C# test - now succeeds
- Run actual game - now works!

## Limitations and Workarounds

### Current Limitations

1. **Manual implementation required** - Function bodies must be manually written
2. **No real Win32 APIs** - Need to mock or use emulator environment
3. **Memory access is fake** - Not accessing real x86 memory

### Workarounds

1. **Start with key functions** - Don't implement everything, focus on problem areas
2. **Use emulator environment** - Integrate with actual emulator for real behavior
3. **Iterative approach** - Implement one function at a time, test, refine

## Conclusion

The `DecompToCS` tool provides a way to convert decompiled game code into debuggable C# code. While full automation isn't ready yet, manually implementing key functions based on the generated skeletons is highly valuable for:

- Understanding initialization flows
- Debugging emulator issues
- Comparing expected vs actual behavior
- Fixing Win32 API implementations

For ign_teas specifically, this approach allows us to:
1. See exactly what the game tries to do during initialization
2. Identify where the emulator's behavior differs
3. Fix the issues and get the game working

---

**Next Steps:**
1. Generate C# skeletons for ign_teas
2. Manually implement `Function_004032A0` (initialization)
3. Implement DirectX initialization functions
4. Test and compare with emulator
5. Fix emulator bugs found during comparison
