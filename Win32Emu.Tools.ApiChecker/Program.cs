using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Win32Emu.Win32;

// Helper class to check if an export is in TryInvokeUnsafe
static class ApiChecker
{
    private static readonly Dictionary<string, string> ModuleFiles = new()
    {
        ["KERNEL32.DLL"] = "Kernel32Module.cs",
        ["USER32.DLL"] = "User32Module.cs",
        ["GDI32.DLL"] = "Gdi32Module.cs",
        ["WINMM.DLL"] = "WinMMModule.cs",
        ["DINPUT.DLL"] = "DInputModule.cs",
        ["DDRAW.DLL"] = "DDrawModule.cs",
        ["DSOUND.DLL"] = "DSoundModule.cs",
        ["DPLAYX.DLL"] = "DPlayXModule.cs"
    };
    
    public static bool IsImplemented(string dllName, string apiName)
    {
        var dllUpper = dllName.ToUpperInvariant();
        
        // First check if it has a DllModuleExport attribute
        if (DllModuleExportInfo.IsExportImplemented(dllUpper, apiName))
            return true;
        
        // Check if the module file exists
        if (!ModuleFiles.TryGetValue(dllUpper, out var moduleFile))
            return false;
        
        // Find the source file
        var currentDir = Directory.GetCurrentDirectory();
        var sourceFile = Path.Combine(currentDir, "..", "Win32Emu", "Win32", "Modules", moduleFile);
        
        if (!File.Exists(sourceFile))
        {
            // Try alternative path
            sourceFile = Path.Combine(currentDir, "..", "..", "..", "Win32Emu", "Win32", "Modules", moduleFile);
            if (!File.Exists(sourceFile))
                return false;
        }
        
        // Read the source file and check if the API is in a case statement
        var sourceCode = File.ReadAllText(sourceFile);
        var caseStatement = $"case \"{apiName.ToUpperInvariant()}\":";
        
        return sourceCode.Contains(caseStatement);
    }
}

class Program
{
    static void Main()
    {
        // Parse the issue checklist and check implementation status
        var apis = new Dictionary<string, List<string>>
        {
        ["kernel32.dll"] = new List<string>
        {
            "QueryPerformanceFrequency", "GetFileType", "GetStdHandle", "ReadFile",
            "HeapDestroy", "RaiseException", "LCMapStringW", "LCMapStringA",
            "HeapReAlloc", "SetEnvironmentVariableA", "CompareStringW", "CompareStringA",
            "GlobalFree", "GlobalUnlock", "GlobalHandle", "GlobalLock", "GlobalAlloc",
            "HeapAlloc", "HeapFree", "ExitProcess", "TerminateProcess", "GetCurrentProcess",
            "GetModuleHandleA", "GetStartupInfoA", "GetCommandLineA", "GetVersion",
            "GetLastError", "DeleteFileA", "MoveFileA", "FindFirstFileA", "FindNextFileA",
            "FindClose", "FileTimeToSystemTime", "FileTimeToLocalFileTime", "CloseHandle",
            "GetTimeZoneInformation", "SetEndOfFile", "HeapCreate", "VirtualFree",
            "VirtualAlloc", "GetProcAddress", "GetModuleFileNameA", "MultiByteToWideChar",
            "GetOEMCP", "SetHandleCount", "SetStdHandle", "WriteFile", "SetFilePointer",
            "FlushFileBuffers", "GetStringTypeW"
        },
        ["user32.dll"] = new List<string>
        {
            "PostQuitMessage", "SetRect", "ClientToScreen", "GetClientRect",
            "PostMessageA", "GetSystemMetrics", "ReleaseDC", "GetDC",
            "TranslateMessage", "DispatchMessageA", "DialogBoxParamA", "EndDialog",
            "SendDlgItemMessageA", "EnableWindow", "GetDlgItem", "GetDlgItemTextA",
            "ShowWindow", "MessageBoxA", "GetWindowRect", "SystemParametersInfoA",
            "SetWindowPos", "AdjustWindowRectEx", "GetMenu", "SetWindowLongA",
            "GetWindowLongA", "LoadCursorA", "DestroyWindow", "SetFocus",
            "UpdateWindow", "CreateWindowExA", "SetCursor", "DefWindowProcA",
            "PeekMessageA", "GetMessageA", "LoadIconA", "RegisterClassA"
        },
        ["gdi32.dll"] = new List<string>
        {
            "GetDeviceCaps", "GetStockObject"
        },
        ["WINMM.dll"] = new List<string>
        {
            "joyGetPosEx", "joyGetDevCapsA", "mciSendStringA", "timeKillEvent",
            "timeBeginPeriod", "timeSetEvent", "timeEndPeriod", "timeGetTime"
        },
        ["DINPUT.dll"] = new List<string>
        {
            "DirectInputCreateA"
        },
        ["DDRAW.dll"] = new List<string>
        {
            "DirectDrawCreate"
        },
        ["DSOUND.dll"] = new List<string>
        {
            "DirectSoundCreate"
        },
        ["DPLAYX.dll"] = new List<string>
        {
            "Ordinal_2", "Ordinal_1"
        }
    };

    Console.WriteLine("# API Implementation Status\n");
    Console.WriteLine("Checking which APIs are implemented in the codebase...\n");

    foreach (var dll in apis)
    {
        Console.WriteLine($"## {dll.Key}");
        Console.WriteLine();
        
        var implemented = new List<string>();
        var notImplemented = new List<string>();
        
        foreach (var api in dll.Value)
        {
            var isImplemented = ApiChecker.IsImplemented(dll.Key, api);
            if (isImplemented)
                implemented.Add(api);
            else
                notImplemented.Add(api);
        }
        
        Console.WriteLine($"**Implemented: {implemented.Count}/{dll.Value.Count}**");
        Console.WriteLine();
        
        if (implemented.Count > 0)
        {
            Console.WriteLine("✅ Implemented:");
            foreach (var api in implemented.OrderBy(x => x))
                Console.WriteLine($"  - {api}");
            Console.WriteLine();
        }
        
        if (notImplemented.Count > 0)
        {
            Console.WriteLine("❌ Not Implemented:");
            foreach (var api in notImplemented.OrderBy(x => x))
                Console.WriteLine($"  - {api}");
            Console.WriteLine();
        }
        
        Console.WriteLine();
    }
    }
}
