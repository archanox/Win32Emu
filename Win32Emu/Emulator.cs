using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Cpu.Iced;
using Win32Emu.Debugging;
using Win32Emu.Diagnostics;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.Win32;
using Win32Emu.Win32.Modules;

namespace Win32Emu;

public sealed class Emulator : IDisposable
{
    private readonly IEmulatorHost? _host;
    private readonly ILogger _logger;
    private VirtualMemory? _vm;
    private IcedCpu? _cpu;
    private ProcessEnvironment? _env;
    private Win32Dispatcher? _dispatcher;
    private LoadedImage? _image;
    private bool _debugMode;
    private bool _interactiveDebugMode;
    private volatile bool _stopRequested;
    private readonly ManualResetEvent _pauseEvent;

    public Emulator(IEmulatorHost? host = null, ILogger? logger = null)
    {
        _host = host;
        _logger = logger ?? NullLogger.Instance;
        _stopRequested = false;
        _pauseEvent = new ManualResetEvent(true); // Initially not paused (signaled)
        
        // Set the logger for Diagnostics class
        Diagnostics.Diagnostics.SetLogger(_logger);
    }

    /// <summary>
    /// Request the emulator to stop execution
    /// </summary>
    public void Stop()
    {
        _stopRequested = true;
        _pauseEvent.Set(); // Signal the pause event to wake up any waiting threads
        LogDebug("[Emulator] Stop requested");
    }

    /// <summary>
    /// Request the emulator to pause execution
    /// </summary>
    public void Pause()
    {
        _pauseEvent.Reset(); // Set event to non-signaled (paused)
        LogDebug("[Emulator] Pause requested");
    }

    /// <summary>
    /// Resume emulator execution from pause
    /// </summary>
    public void Resume()
    {
        _pauseEvent.Set(); // Set event to signaled (running)
        LogDebug("[Emulator] Resume requested");
    }

    /// <summary>
    /// Check if emulator is currently paused
    /// </summary>
    public bool IsPaused => !_pauseEvent.WaitOne(0);

    /// <summary>
    /// Post a message to the Win32 message queue (for GUI-to-emulator communication)
    /// </summary>
    public bool PostMessage(uint hwnd, uint message, uint wParam, uint lParam)
    {
        if (_env == null)
        {
            LogDebug("[Emulator] PostMessage called but environment not initialized");
            return false;
        }
        
        return _env.PostMessage(hwnd, message, wParam, lParam);
    }

    public void LoadExecutable(string path, bool debugMode = false, bool interactiveDebugMode = false, int reservedMemoryMb = 256)
    {
        _debugMode = debugMode;
        _interactiveDebugMode = interactiveDebugMode;

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File not found: {path}");
        }

        LogDebug($"[Loader] Loading PE: {path}");
        // Convert MB to bytes for VirtualMemory constructor
        var memorySizeBytes = (ulong)reservedMemoryMb * 1024 * 1024;
        _vm = new VirtualMemory(memorySizeBytes);
        var loader = new PeImageLoader(_vm);
        _image = loader.Load(path);
        LogDebug($"[Loader] Image base=0x{_image.BaseAddress:X8} EntryPoint=0x{_image.EntryPointAddress:X8} Size=0x{_image.ImageSize:X}");
        LogDebug($"[Loader] Imports mapped: {_image.ImportAddressMap.Count}");

        _env = new ProcessEnvironment(_vm, 0x01000000, _host, _logger);
        _env.InitializeStrings(path, Array.Empty<string>());

        _cpu = new IcedCpu(_vm, _logger);
        _cpu.SetEip(_image.EntryPointAddress);
        _cpu.SetRegister("ESP", 0x00200000);
        _cpu.SetRegister("EBP", 0x00200000); // Initialize frame pointer to match stack pointer

        _dispatcher = new Win32Dispatcher(_logger);

        var kernel32Module = new Kernel32Module(_env, _image.BaseAddress, loader, _logger);
        kernel32Module.SetDispatcher(_dispatcher);
        _dispatcher.RegisterModule(kernel32Module);
        // Register KERNELBASE for forwarded exports from KERNEL32
        _dispatcher.RegisterModule(new KernelBaseModule(_env, _image.BaseAddress, loader, _logger));

        _dispatcher.RegisterModule(new User32Module(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new Gdi32Module(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new DDrawModule(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new DSoundModule(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new DInputModule(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new WinMmModule(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new Glide2XModule(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new DPlayXModule(_env, _image.BaseAddress, loader, _logger));
    }

    public void Run()
    {
        if (_cpu == null || _vm == null || _env == null || _dispatcher == null || _image == null)
        {
            throw new InvalidOperationException("Executable not loaded. Call LoadExecutable first.");
        }

        _stopRequested = false;
        _pauseEvent.Set(); // Ensure we start in running state

        if (_interactiveDebugMode)
        {
            RunWithInteractiveDebugger();
        }
        else if (_debugMode)
        {
            RunWithEnhancedDebugging();
        }
        else
        {
            RunNormal();
        }

        string exitMessage;
        if (_stopRequested)
        {
	        exitMessage = "[Exit] Stop requested by user.";
        }
        else
        {
	        if (_env.ExitRequested)
	        {
		        exitMessage = "[Exit] Process requested exit.";
	        }
	        else
	        {
		        exitMessage = "[Exit] Execution completed.";
	        }
        }

        LogDebug(exitMessage);

        LogDebug("=== Unknown Function Summary ===");
        _dispatcher.PrintUnknownFunctionsSummary();
    }

    private void RunNormal()
    {
        // Run indefinitely until stop/exit requested
        while (!_stopRequested && !_env!.ExitRequested)
        {
            // Wait for pause event to be signaled (running state)
            // Using a timeout allows us to check _stopRequested periodically
            _pauseEvent.WaitOne(100);

            if (_stopRequested)
            {
	            break;
            }

            var step = _cpu!.SingleStep(_vm!);
            
            // Check for COM vtable method calls
            if (step.IsCall && _env.ComDispatcher.IsComVtableAddress(step.CallTarget))
            {
                LogDebug($"[COM] Vtable method call at 0x{step.CallTarget:X8}");
                if (_env.ComDispatcher.TryInvoke(step.CallTarget, _cpu, _vm, out var ret))
                {
                    LogDebug($"[COM] Method returned 0x{ret:X8}");
                    var esp = _cpu.GetRegister("ESP");
                    var retEip = _vm.Read32(esp);
                    // COM methods use stdcall convention - they clean up their own stack
                    // For now, we'll let the method handler manage the stack
                    esp += 4; // Pop return address
                    _cpu.SetRegister("ESP", esp);
                    _cpu.SetRegister("EAX", ret); // Return value in EAX
                    _cpu.SetEip(retEip);
                }
            }
            else if (step.IsCall && _image!.ImportAddressMap.TryGetValue(step.CallTarget, out var imp))
            {
                var dll = imp.dll.ToUpperInvariant();
                var name = imp.name;
                LogDebug($"[Import] {dll}!{name}");
                if (_dispatcher!.TryInvoke(dll, name, _cpu, _vm, out var ret, out var argBytes))
                {
                    LogDebug($"[Import] Returned 0x{ret:X8}");
                    var esp = _cpu.GetRegister("ESP");
                    var retEip = _vm.Read32(esp);
                    esp += 4 + (uint)argBytes;
                    _cpu.SetRegister("ESP", esp);
                    _cpu.SetEip(retEip);
                }
            }
        }
    }

    private void RunWithEnhancedDebugging()
    {
        var debugger = _cpu!.CreateDebugger(_vm!);
        debugger.EnableSuspiciousRegisterDetection = true;
        debugger.LogToConsole = true;
        debugger.LogAllInstructions = false;
        debugger.SuspiciousThreshold = 0x1000;

        LogDebug("[Debug] Enhanced debugging enabled - will catch 0xFFFFFFFD errors");
        LogDebug("[Debug] Monitoring for suspicious register values");

        // Run indefinitely until stop/exit requested
        var i = 0;
        while (!_stopRequested && !_env!.ExitRequested)
        {
            // Wait for pause event to be signaled (running state)
            // Using a timeout allows us to check _stopRequested periodically
            _pauseEvent.WaitOne(100);

            if (_stopRequested)
            {
	            break;
            }

            var currentEip = _cpu.GetEip();

            if (currentEip is >= 0x0F000000 and < 0x10000000)
            {
                LogDebug("\n[Debug] *** CPU TRYING TO EXECUTE SYNTHETIC IMPORT ADDRESS! ***");
                LogDebug($"[Debug] EIP=0x{currentEip:X8} at instruction {i}");

                if (_image!.ImportAddressMap.TryGetValue(currentEip, out var importInfo))
                {
                    LogDebug($"[Debug] This is import: {importInfo.dll}!{importInfo.name}");
                }
                else
                {
                    LogDebug("[Debug] Unknown synthetic address - not in import map");
                }

                LogDebug("[Debug] This should now execute an INT3 stub that will be handled as an import call");
            }

            if (debugger.IsProblematicEip())
            {
                LogDebug($"\n[Debug] *** FOUND PROBLEMATIC EIP AT INSTRUCTION {i} ***");
                debugger.HandleProblematicEip();
                LogDebug("[Debug] Stopping execution to prevent crash");
                break;
            }

            if (_cpu.HasSuspiciousRegisters() && i > 100)
            {
                LogDebug($"[Debug] [Instruction {i}] Suspicious registers detected");
            }

            try
            {
                var wasCall = WillBeCall(_cpu, _vm!);
                var callTarget = wasCall ? GetCallTarget(_cpu, _vm!) : 0u;

                debugger.SafeSingleStep();

                var step = new CpuStepResult(wasCall, callTarget);

                // Check for COM vtable method calls
                if (step.IsCall && _env.ComDispatcher.IsComVtableAddress(step.CallTarget))
                {
                    LogDebug($"[COM] Vtable method call at 0x{step.CallTarget:X8}");
                    if (_env.ComDispatcher.TryInvoke(step.CallTarget, _cpu, _vm, out var ret))
                    {
                        LogDebug($"[COM] Method returned 0x{ret:X8}");
                        var esp = _cpu.GetRegister("ESP");
                        var retEip = _vm.Read32(esp);
                        esp += 4; // Pop return address
                        _cpu.SetRegister("ESP", esp);
                        _cpu.SetRegister("EAX", ret); // Return value in EAX
                        _cpu.SetEip(retEip);
                    }
                }
                else if (step.IsCall && _image!.ImportAddressMap.TryGetValue(step.CallTarget, out var imp))
                {
                    var dll = imp.dll.ToUpperInvariant();
                    var name = imp.name;
                    LogDebug($"[Import] {dll}!{name}");
                    if (_dispatcher!.TryInvoke(dll, name, _cpu, _vm, out var ret, out var argBytes))
                    {
                        LogDebug($"[Import] Returned 0x{ret:X8}");
                        var esp = _cpu.GetRegister("ESP");
                        var retEip = _vm.Read32(esp);
                        esp += 4 + (uint)argBytes;
                        _cpu.SetRegister("ESP", esp);
                        _cpu.SetEip(retEip);
                    }
                }
            }
            catch (IndexOutOfRangeException ex) when (ex.Message.Contains("0xFFFFFFFD") || ex.Message.Contains("0xFFFFFFFF"))
            {
                LogDebug($"\n[Debug] *** CAUGHT MEMORY ACCESS VIOLATION AT INSTRUCTION {i} ***");
                LogDebug($"[Debug] Exception: {ex.Message}");

                if (currentEip is >= 0x0F000000 and < 0x10000000)
                {
                    LogDebug($"[Debug] ERROR CAUSE: Trying to execute synthetic import address 0x{currentEip:X8}");
                    if (_image!.ImportAddressMap.TryGetValue(currentEip, out var importInfo))
                    {
                        LogDebug($"[Debug] This is import: {importInfo.dll}!{importInfo.name}");
                    }
                    LogDebug("[Debug] SOLUTION: The program should CALL THROUGH the IAT, not execute the import address directly");
                }

                var trace = debugger.GetExecutionTrace();
                var suspiciousStates = debugger.FindSuspiciousStates();

                LogDebug($"[Debug] Execution trace has {trace.Count} entries");
                LogDebug($"[Debug] Found {suspiciousStates.Count} suspicious register states");

                if (suspiciousStates.Count > 0)
                {
                    var first = suspiciousStates[0];
                    LogDebug("[Debug] First suspicious state occurred at:");
                    LogDebug($"[Debug]   EIP=0x{first.Eip:X8} EBP=0x{first.Ebp:X8} ESP=0x{first.Esp:X8}");
                }

                throw;
            }
            catch (Exception ex)
            {
                LogDebug($"[Debug] Unexpected exception at instruction {i}: {ex}");
                throw;
            }

            i++;
        }

        var finalTrace = debugger.GetExecutionTrace();
        var finalSuspicious = debugger.FindSuspiciousStates();
        LogDebug("[Debug] Final execution summary:");
        LogDebug($"[Debug]   Total traced instructions: {finalTrace.Count}");
        LogDebug($"[Debug]   Suspicious register states: {finalSuspicious.Count}");
    }

    private void RunWithInteractiveDebugger()
    {
        var debugger = new InteractiveDebugger(_cpu!, _vm!);
        
        Console.WriteLine("=== Interactive Debugger Mode ===");
        Console.WriteLine("Type 'help' for available commands");
        Console.WriteLine("The debugger will break at the entry point");
        Console.WriteLine();

        // Break at entry point
        var currentEip = _cpu!.GetEip();
        if (!debugger.HandleBreak(currentEip, "Stopped at entry point"))
        {
            return; // User quit
        }

        // Run indefinitely until stop/exit requested
        while (!_stopRequested && !_env!.ExitRequested && !debugger.ShouldStop)
        {
            // Check if debugger wants to break
            currentEip = _cpu.GetEip();
            if (debugger.ShouldBreak(currentEip))
            {
                if (!debugger.HandleBreak(currentEip))
                {
                    break; // User quit
                }
            }

            // Execute one instruction
            var step = _cpu.SingleStep(_vm!);
            
            // Check for COM vtable method calls
            if (step.IsCall && _env.ComDispatcher.IsComVtableAddress(step.CallTarget))
            {
                LogDebug($"[COM] Vtable method call at 0x{step.CallTarget:X8}");
                if (_env.ComDispatcher.TryInvoke(step.CallTarget, _cpu, _vm, out var ret))
                {
                    LogDebug($"[COM] Method returned 0x{ret:X8}");
                    var esp = _cpu.GetRegister("ESP");
                    var retEip = _vm.Read32(esp);
                    esp += 4; // Pop return address
                    _cpu.SetRegister("ESP", esp);
                    _cpu.SetRegister("EAX", ret); // Return value in EAX
                    _cpu.SetEip(retEip);
                }
            }
            else if (step.IsCall && _image!.ImportAddressMap.TryGetValue(step.CallTarget, out var imp))
            {
                var dll = imp.dll.ToUpperInvariant();
                var name = imp.name;
                LogDebug($"[Import] {dll}!{name}");
                if (_dispatcher!.TryInvoke(dll, name, _cpu, _vm, out var ret, out var argBytes))
                {
                    LogDebug($"[Import] Returned 0x{ret:X8}");
                    var esp = _cpu.GetRegister("ESP");
                    var retEip = _vm.Read32(esp);
                    esp += 4 + (uint)argBytes;
                    _cpu.SetRegister("ESP", esp);
                    _cpu.SetEip(retEip);
                }
            }
        }

        Console.WriteLine("\nInteractive debugger session ended");
    }

    private static bool WillBeCall(IcedCpu cpu, VirtualMemory vm)
    {
        try
        {
            var eip = cpu.GetEip();
            var opcode = vm.Read8(eip);

            if (opcode == 0xE8 || (opcode == 0xFF && IsCallVariant(vm, eip)))
            {
                return true;
            }

            if (opcode == 0xCC && eip is >= 0x0F000000 and < 0x10000000)
            {
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsCallVariant(VirtualMemory vm, uint eip)
    {
        try
        {
            var modRm = vm.Read8(eip + 1);
            var reg = (modRm >> 3) & 0x07;
            return reg == 2;
        }
        catch
        {
            return false;
        }
    }

    private static uint GetCallTarget(IcedCpu cpu, VirtualMemory vm)
    {
        try
        {
            var eip = cpu.GetEip();
            var opcode = vm.Read8(eip);

            if (opcode == 0xE8)
            {
                var displacement = vm.Read32(eip + 1);
                return (uint)(eip + 5 + (int)displacement);
            }

            if (opcode == 0xCC && eip is >= 0x0F000000 and < 0x10000000)
            {
	            return eip;
            }
        }
        catch
        {
            // If we can't decode, return 0
        }

        return 0;
    }

    private void LogDebug(string message)
    {
        _logger.LogDebug(message);
        if (_host != null)
        {
            _host.OnDebugOutput(message, DebugLevel.Debug);
        }
    }

    public void Dispose()
    {
	    _pauseEvent.Dispose();
    }
}

public interface IEmulatorHost
{
    void OnDebugOutput(string message, DebugLevel level);
    void OnStdOutput(string output);
    void OnWindowCreate(WindowCreateInfo info);
}

public enum DebugLevel
{
    Trace,
    Debug,
    Info,
    Warning,
    Error
}

public class WindowCreateInfo
{
    public required uint Handle { get; init; }
    public required string Title { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public required string ClassName { get; init; }
    public uint Style { get; init; }
    public uint ExStyle { get; init; }
    public uint Parent { get; init; }
    public uint Menu { get; init; }
}
