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
    private readonly Telemetry.TelemetryService? _telemetryService;
    private readonly Telemetry.EmulatorMetrics? _metrics;
    private VirtualMemory? _vm;
    private IcedCpu? _cpu;
    private ProcessEnvironment? _env;
    private Win32Dispatcher? _dispatcher;
    private LoadedImage? _image;
    private bool _debugMode;
    private bool _interactiveDebugMode;
    private bool _gdbServerMode;
    private int _gdbServerPort;
    private volatile bool _stopRequested;
    private readonly ManualResetEvent _pauseEvent;
    private Task? _eventProcessingTask;
    private CancellationTokenSource? _eventProcessingCts;

    public Emulator(IEmulatorHost? host = null, ILogger? logger = null, Telemetry.TelemetryService? telemetryService = null)
    {
        _host = host;
        _logger = logger ?? NullLogger.Instance;
        _telemetryService = telemetryService;
        _stopRequested = false;
        _pauseEvent = new ManualResetEvent(true); // Initially not paused (signaled)
        
        // Set the logger for Diagnostics class
        Diagnostics.Diagnostics.SetLogger(_logger);
        
        // Initialize metrics if telemetry is enabled
        if (_telemetryService != null)
        {
            _metrics = new Telemetry.EmulatorMetrics(_telemetryService.Meter);
        }
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
    /// Get the emulator metrics (may be null if telemetry is not enabled)
    /// </summary>
    public Telemetry.EmulatorMetrics? Metrics => _metrics;

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

    /// <summary>
    /// Subscribe the process environment to UI events from rendering and input backends.
    /// This enables event-driven UI message handling. Should be called after LoadExecutable
    /// when backends are initialized.
    /// </summary>
    /// <param name="renderingBackend">The rendering backend to subscribe to (optional)</param>
    /// <param name="inputBackend">The input backend to subscribe to (optional)</param>
    public void SubscribeToUIEvents(Rendering.IRenderingBackend? renderingBackend = null, Rendering.IInputBackend? inputBackend = null)
    {
        if (_env == null)
        {
            LogDebug("[Emulator] SubscribeToUIEvents called but environment not initialized");
            return;
        }

        // Use backends from parameters if provided, otherwise use from environment
        var renderBackend = renderingBackend;
        var inputBackendToUse = inputBackend ?? _env.InputBackend;

        _env.SubscribeToUIEvents(renderBackend, inputBackendToUse);
        LogDebug("[Emulator] Subscribed to UI events from backends");
    }

    public void LoadExecutable(string path, string[]? programArgs = null, bool debugMode = false, bool interactiveDebugMode = false, int reservedMemoryMb = 256, bool gdbServerMode = false, int gdbServerPort = 1234, bool enableInstructionAnalyzer = false, bool enableLegacyInstructionDecoding = false)
    {
        _debugMode = debugMode;
        _interactiveDebugMode = interactiveDebugMode;
        _gdbServerMode = gdbServerMode;
        _gdbServerPort = gdbServerPort;

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
        LogDebug($"[Loader] Subsystem: {_image.Subsystem} (2=GUI, 3=CUI)");

        _env = new ProcessEnvironment(_vm, 0x01000000, _host, _logger);
        // Register the main executable so GetModuleFileNameA can find it
        _env.RegisterMainExecutable(_image, path);
        // Convert path to Windows-style backslashes for proper parsing by C runtime
        _env.InitializeStrings(path, programArgs ?? []);
        _env.InitializeTebAndPeb(_image.BaseAddress);
        
        // Initialize console based on PE subsystem type
        _env.InitializeConsoleForSubsystem(_image.Subsystem);

        // Determine decoder options for legacy instruction support
        var decoderOptions = Iced.Intel.DecoderOptions.None;
        if (enableLegacyInstructionDecoding)
        {
            decoderOptions = Iced.Intel.DecoderOptions.MPX | 
                           Iced.Intel.DecoderOptions.MovTr | 
                           Iced.Intel.DecoderOptions.Cyrix | 
                           Iced.Intel.DecoderOptions.Cyrix_DMI | 
                           Iced.Intel.DecoderOptions.ALTINST;
            LogDebug("[Loader] Legacy instruction decoding enabled (MPX, Cyrix, ALTINST, etc.)");
        }

        _cpu = new IcedCpu(_vm, _logger, decoderOptions, enableInstructionAnalyzer);
        if (enableInstructionAnalyzer)
        {
            LogDebug("[Loader] Instruction analyzer enabled");
        }
        _cpu.SetEip(_image.EntryPointAddress);
        _cpu.SetRegister("ESP", 0x00200000);
        _cpu.SetRegister("EBP", 0x00200000); // Initialize frame pointer to match stack pointer

        _dispatcher = new Win32Dispatcher(_logger);

        var kernel32Module = new Kernel32Module(_env, _image.BaseAddress, loader, _logger);
        kernel32Module.SetDispatcher(_dispatcher);
        
        // Create resource reader for PE resources (dialogs, icons, etc.)
        var peImage = AsmResolver.PE.PEImage.FromFile(path);
        var resourceReader = new PeResourceReader(peImage, _image.BaseAddress, _vm);
        kernel32Module.SetResourceReader(resourceReader);
        
        _dispatcher.RegisterModule(kernel32Module);
        // Register KERNELBASE for forwarded exports from KERNEL32
        _dispatcher.RegisterModule(new KernelBaseModule(_env, _image.BaseAddress, loader, _logger));

        _dispatcher.RegisterModule(new Advapi32Module(_env, _image.BaseAddress, loader, _logger));
        
        var user32Module = new User32Module(_env, _image.BaseAddress, loader, _logger);
        user32Module.SetDispatcher(_dispatcher);
        user32Module.SetLoadedImage(_image);
        user32Module.SetResourceReader(resourceReader); // Set resource reader for dialog loading
        user32Module.SetHost(_host); // Set host for dialog UI callbacks
        _dispatcher.RegisterModule(user32Module);
        
        _dispatcher.RegisterModule(new Gdi32Module(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new DDrawModule(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new DSoundModule(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new DInputModule(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new WinMmModule(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new Glide2XModule(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new DPlayXModule(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new Ole32Module(_env, _image.BaseAddress, loader, _logger));

        // Initialize the main thread in the thread scheduler
        _env.InitializeMainThread(_cpu);
        LogDebug("[Loader] Main thread initialized");
    }

    public async Task RunAsync()
    {
        if (_cpu == null || _vm == null || _env == null || _dispatcher == null || _image == null)
        {
            throw new InvalidOperationException("Executable not loaded. Call LoadExecutable first.");
        }

        // Start tracing activity
        using var activity = _telemetryService?.StartActivity("Emulator.Run");
        activity?.SetTag("executable", _image.FilePath);
        activity?.SetTag("debug_mode", _debugMode);

        _stopRequested = false;
        _pauseEvent.Set(); // Ensure we start in running state

        // Subscribe to UI events from backends (if available)
        if (_env.InputBackend != null || _env.AudioBackend != null)
        {
            // Note: We need access to rendering backend too, but it's not stored in _env
            // For now, we'll need to ensure this is called from the host/GUI when backends are set up
            LogDebug("[Emulator] UI backends detected - event processing will be available");
        }

        // Start background UI event processing thread
        StartEventProcessing();

        try
        {
            if (_gdbServerMode)
            {
                await RunWithGdbServer(_gdbServerPort);
            }
            else if (_interactiveDebugMode)
            {
                await RunWithInteractiveDebuggerAsync();
            }
            else if (_debugMode)
            {
                await RunWithEnhancedDebuggingAsync();
            }
            else
            {
                await RunNormalAsync();
            }
        }
        finally
        {
            // Stop event processing thread
            StopEventProcessing();
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

    /// <summary>
    /// Synchronous wrapper for RunAsync for backward compatibility
    /// </summary>
    public void Run()
    {
        RunAsync().GetAwaiter().GetResult();
    }

    private async Task RunNormalAsync()
    {
        var scheduler = _env!.ThreadScheduler;

        // Run indefinitely until stop/exit requested or no threads running
        while (!_stopRequested && !_env!.ExitRequested)
        {
            // Check pause state periodically without blocking
            if (!_pauseEvent.WaitOne(0))
            {
                // Paused - yield and check again
                await Task.Delay(100);
                continue;
            }

            if (_stopRequested)
            {
	            break;
            }

            // Check if we have any runnable threads
            if (scheduler != null && !scheduler.HasRunningThreads())
            {
                LogDebug("[Emulator] No more runnable threads, stopping execution");
                break;
            }

            // Process wait timeouts
            scheduler?.ProcessWaitTimeouts();

            // Check if we should context switch
            if (scheduler != null && scheduler.ShouldContextSwitch())
            {
                var nextThread = scheduler.ContextSwitch(_cpu!);
                if (nextThread != null)
                {
                    LogDebug($"[Emulator] Context switched to thread {nextThread.ThreadId}");
                }
                else
                {
                    // No runnable threads - yield to prevent busy-waiting
                    await Task.Delay(1);
                    continue;
                }
            }

            var step = _cpu!.SingleStep(_vm!);
            
            // Record instruction execution
            _metrics?.RecordInstructionsExecuted();
            
            // Check for thread exit (return address is 0xFFFFFFFF)
            var eip = _cpu!.GetEip();
            if (eip == 0xFFFFFFFF && scheduler != null)
            {
                var currentThread = scheduler.CurrentThread;
                if (currentThread != null && currentThread.ThreadId != 1) // Not the main thread
                {
                    var exitCode = _cpu.GetRegister("EAX"); // Return value in EAX
                    scheduler.TerminateThread(currentThread.ThreadId, exitCode);
                    LogDebug($"[Emulator] Thread {currentThread.ThreadId} terminated with exit code {exitCode}");
                    
                    // Switch to another thread
                    scheduler.ContextSwitch(_cpu);
                    continue;
                }
            }
            
            // Check for COM vtable method calls
            if (step.IsCall && _env.ComDispatcher.IsComVtableAddress(step.CallTarget))
            {
                _logger.LogInformation("[COM] Vtable method call at address 0x{CallTarget:X8}", step.CallTarget);
                
                // Save callee-saved registers (EBX, ESI, EDI) per x86 calling convention
                // Note: We do NOT save EBP here because some calling code uses EBP to hold the function
                // pointer for indirect calls (e.g., MOV EBP, [IAT_Entry]; CALL EBP). If we preserve
                // the EBP value at the time of the call, we'll restore the function pointer value
                // instead of the original frame pointer, causing crashes.
                var saved = CpuHelpers.SaveCalleeSavedRegisters(_cpu);
                
                if (_env.ComDispatcher.TryInvoke(step.CallTarget, _cpu, _vm!, out var ret, out var comArgBytes))
                {
                    LogDebug($"[COM] Method returned 0x{ret:X8}");
                    var esp = _cpu.GetRegister("ESP");
                    var retEip = _vm!.Read32(esp);
                    // COM methods use stdcall convention - callee cleans up the stack
                    esp += 4 + (uint)comArgBytes; // Pop return address + arguments
                    _cpu.SetRegister("ESP", esp);
                    _cpu.SetRegister("EAX", ret); // Return value in EAX
                    _cpu.SetEip(retEip);
                    
                    // Restore callee-saved registers (except EBP - see above)
                    CpuHelpers.RestoreCalleeSavedRegisters(_cpu, saved);
                    
                    // Restore EBP from stack to handle indirect call cases
                    RestoreEbpFromStack(esp);
                }
            }
            else if (step.IsCall && _image!.ImportAddressMap.TryGetValue(step.CallTarget, out var imp))
            {
                var dll = imp.dll.ToUpperInvariant();
                var name = imp.name;
                _logger.LogInformation("[Import] Hooked function: {Dll}!{Name} at address 0x{CallTarget:X8}", dll, name, step.CallTarget);
                
                // Save callee-saved registers (EBX, ESI, EDI) per x86 calling convention
                // Note: We do NOT save EBP here because some calling code uses EBP to hold the function
                // pointer for indirect calls (e.g., MOV EBP, [IAT_Entry]; CALL EBP). If we preserve
                // the EBP value at the time of the call, we'll restore the function pointer value
                // instead of the original frame pointer, causing crashes.
                var saved = CpuHelpers.SaveCalleeSavedRegisters(_cpu);
                
                if (_dispatcher!.TryInvoke(dll, name, _cpu, _vm!, out var ret, out var argBytes))
                {
                    LogDebug($"[Import] Returned 0x{ret:X8}");
                    var esp = _cpu.GetRegister("ESP");
                    var retEip = _vm!.Read32(esp);
                    
                    esp += 4 + (uint)argBytes;
                    
                    _cpu.SetRegister("ESP", esp);
                    _cpu.SetEip(retEip);
                    
                    // Restore callee-saved registers (except EBP - see above)
                    CpuHelpers.RestoreCalleeSavedRegisters(_cpu, saved);
                    
                    // Restore EBP from stack to handle indirect call cases
                    RestoreEbpFromStack(esp);
                }
            }
            else if (step.IsCall)
            {
	            _logger.LogInformation("[Call] Call method at 0x{CallTarget:X8}", step.CallTarget);
            }
            
        }
    }

    /// <summary>
    /// Synchronous wrapper for backward compatibility
    /// </summary>
    private void RunNormal()
    {
        RunNormalAsync().GetAwaiter().GetResult();
    }

    private Task RunWithEnhancedDebuggingAsync()
    {
        // Enhanced debugging is inherently synchronous due to the debugger's design
        RunWithEnhancedDebugging();
        return Task.CompletedTask;
    }

    private Task RunWithInteractiveDebuggerAsync()
    {
        // Interactive debugger is inherently synchronous due to console I/O
        RunWithInteractiveDebugger();
        return Task.CompletedTask;
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

            var currentEip = _cpu!.GetEip();

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
                // Log the first few occurrences and then periodically
                if (i < 500 || i % 10000 == 101)
                {
                    var esp = _cpu.GetRegister("ESP");
                    var ebp = _cpu.GetRegister("EBP");
                    var eip = _cpu!.GetEip();
                    LogDebug($"[Debug] [Instruction {i}] Suspicious registers: EIP=0x{eip:X8} ESP=0x{esp:X8} EBP=0x{ebp:X8}");
                }
                else if (i is > 100 and <= 500)
                {
                    LogDebug($"[Debug] [Instruction {i}] Suspicious registers detected");
                }
            }

            // Add detailed logging for the problematic address range
            if (currentEip >= 0x004123B8 && currentEip <= 0x004125A0 && i < 2000)
            {
                var esp = _cpu.GetRegister("ESP");
                var ebp = _cpu.GetRegister("EBP");
                var eax = _cpu.GetRegister("EAX");
                LogDebug($"[CRT] Instruction {i} at EIP=0x{currentEip:X8} ESP=0x{esp:X8} EBP=0x{ebp:X8} EAX=0x{eax:X8}");
            }
            
            // Log when we see the same EIP range repeatedly (likely a loop)
            if (i % 10000 == 0 && i > 0)
            {
                var eip = _cpu!.GetEip();
                LogDebug($"[Loop Check] Instruction {i}: EIP=0x{eip:X8}");
                
                // Warn the user if execution seems stuck after many instructions
                if (i % 100000 == 0)
                {
                    _logger.LogWarning("[Loop Detection] Emulator has executed {InstructionCount} instructions and may be stuck in a loop. EIP=0x{Eip:X8}", i, eip);
                    _logger.LogWarning("[Loop Detection] If the program is not responding, you may need to stop it. Check the documentation for known issues with this executable.");
                }
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
                    _logger.LogInformation("[COM] Vtable method call at address 0x{CallTarget:X8}", step.CallTarget);
                    
                    // Save callee-saved registers (EBX, ESI, EDI, EBP) per x86 calling convention
                    var savedEbx = _cpu.GetRegister("EBX");
                    var savedEsi = _cpu.GetRegister("ESI");
                    var savedEdi = _cpu.GetRegister("EDI");
                    var savedEbp = _cpu.GetRegister("EBP");
                    
                    if (_env.ComDispatcher.TryInvoke(step.CallTarget, _cpu, _vm!, out var ret, out var comArgBytes))
                    {
                        LogDebug($"[COM] Method returned 0x{ret:X8}");
                        var esp = _cpu.GetRegister("ESP");
                        var retEip = _vm!.Read32(esp);
                        // COM methods use stdcall convention - callee cleans up the stack
                        esp += 4 + (uint)comArgBytes; // Pop return address + arguments
                        _cpu.SetRegister("ESP", esp);
                        _cpu.SetRegister("EAX", ret); // Return value in EAX
                        _cpu.SetEip(retEip);
                        
                        // Restore callee-saved registers
                        _cpu.SetRegister("EBX", savedEbx);
                        _cpu.SetRegister("ESI", savedEsi);
                        _cpu.SetRegister("EDI", savedEdi);
                        _cpu.SetRegister("EBP", savedEbp);
                    }
                }
                else if (step.IsCall && _image!.ImportAddressMap.TryGetValue(step.CallTarget, out var imp))
                {
                    var dll = imp.dll.ToUpperInvariant();
                    var name = imp.name;
                    _logger.LogInformation("[Import] Hooked function: {Dll}!{Name} at address 0x{CallTarget:X8}", dll, name, step.CallTarget);
                    
                    // Save callee-saved registers (EBX, ESI, EDI, EBP) per x86 calling convention
                    var savedEbx = _cpu.GetRegister("EBX");
                    var savedEsi = _cpu.GetRegister("ESI");
                    var savedEdi = _cpu.GetRegister("EDI");
                    var savedEbp = _cpu.GetRegister("EBP");
                    
                    if (_dispatcher!.TryInvoke(dll, name, _cpu, _vm!, out var ret, out var argBytes))
                    {
                        LogDebug($"[Import] Returned 0x{ret:X8}, argBytes={argBytes}");
                        var esp = _cpu.GetRegister("ESP");
                        LogDebug($"[Import] ESP before return: 0x{esp:X8}");
                        
                        // Read first 4 stack values for debugging
                        try
                        {
                            var stack0 = _vm!.Read32(esp);
                            var stack4 = _vm!.Read32(esp + 4);
                            var stack8 = _vm!.Read32(esp + 8);
                            var stack12 = _vm!.Read32(esp + 12);
                            LogDebug($"[Import] Stack: [ESP+0]=0x{stack0:X8} [ESP+4]=0x{stack4:X8} [ESP+8]=0x{stack8:X8} [ESP+12]=0x{stack12:X8}");
                        }
                        catch { }
                        
                        var retEip = _vm!.Read32(esp);
                        LogDebug($"[Import] Return address from stack: 0x{retEip:X8}");
                        esp += 4 + (uint)argBytes;
                        LogDebug($"[Import] ESP after cleanup: 0x{esp:X8} (added {4 + argBytes} bytes)");
                        _cpu.SetRegister("ESP", esp);
                        _cpu.SetEip(retEip);
                        LogDebug($"[Import] Set EIP to 0x{retEip:X8}");
                        
                        // Restore callee-saved registers
                        _cpu.SetRegister("EBX", savedEbx);
                        _cpu.SetRegister("ESI", savedEsi);
                        _cpu.SetRegister("EDI", savedEdi);
                        _cpu.SetRegister("EBP", savedEbp);
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
	            _logger.LogDebug(ex, "[Debug] Unexpected exception at instruction {i}: {ex}", i, ex.Message);
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
            currentEip = _cpu!.GetEip();
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
                _logger.LogInformation("[COM] Vtable method call at address 0x{CallTarget:X8}", step.CallTarget);
                
                // Save callee-saved registers (EBX, ESI, EDI, EBP) per x86 calling convention
                var savedEbx = _cpu.GetRegister("EBX");
                var savedEsi = _cpu.GetRegister("ESI");
                var savedEdi = _cpu.GetRegister("EDI");
                var savedEbp = _cpu.GetRegister("EBP");
                
                if (_env.ComDispatcher.TryInvoke(step.CallTarget, _cpu, _vm!, out var ret, out var comArgBytes))
                {
                    LogDebug($"[COM] Method returned 0x{ret:X8}");
                    var esp = _cpu.GetRegister("ESP");
                    var retEip = _vm!.Read32(esp);
                    // COM methods use stdcall convention - callee cleans up the stack
                    esp += 4 + (uint)comArgBytes; // Pop return address + arguments
                    _cpu.SetRegister("ESP", esp);
                    _cpu.SetRegister("EAX", ret); // Return value in EAX
                    _cpu.SetEip(retEip);
                    
                    // Restore callee-saved registers
                    _cpu.SetRegister("EBX", savedEbx);
                    _cpu.SetRegister("ESI", savedEsi);
                    _cpu.SetRegister("EDI", savedEdi);
                    _cpu.SetRegister("EBP", savedEbp);
                }
            }
            else if (step.IsCall && _image!.ImportAddressMap.TryGetValue(step.CallTarget, out var imp))
            {
                var dll = imp.dll.ToUpperInvariant();
                var name = imp.name;
                _logger.LogInformation("[Import] Hooked function: {Dll}!{Name} at address 0x{CallTarget:X8}", dll, name, step.CallTarget);
                
                // Save callee-saved registers (EBX, ESI, EDI, EBP) per x86 calling convention
                var savedEbx = _cpu.GetRegister("EBX");
                var savedEsi = _cpu.GetRegister("ESI");
                var savedEdi = _cpu.GetRegister("EDI");
                var savedEbp = _cpu.GetRegister("EBP");
                
                if (_dispatcher!.TryInvoke(dll, name, _cpu, _vm!, out var ret, out var argBytes))
                {
                    LogDebug($"[Import] Returned 0x{ret:X8}");
                    var esp = _cpu.GetRegister("ESP");
                    var retEip = _vm!.Read32(esp);
                    esp += 4 + (uint)argBytes;
                    _cpu.SetRegister("ESP", esp);
                    _cpu.SetEip(retEip);
                    
                    // Restore callee-saved registers
                    _cpu.SetRegister("EBX", savedEbx);
                    _cpu.SetRegister("ESI", savedEsi);
                    _cpu.SetRegister("EDI", savedEdi);
                    _cpu.SetRegister("EBP", savedEbp);
                }
            }
        }

        Console.WriteLine("\nInteractive debugger session ended");
    }

    private async Task RunWithGdbServer(int port)
    {
        var breakpoints = new BreakpointManager();
        var gdbServer = new GdbServer(_cpu!, _vm!, breakpoints, _logger, port, _env!.VirtualFileSystem, _env);
        
        // Add symbols from the loaded image for better debugging experience
        if (_image != null)
        {
            var moduleName = Path.GetFileNameWithoutExtension(_image.FilePath).ToUpperInvariant();
            gdbServer.AddSymbolsFromLoadedImage(_image, moduleName);
        }
        
        try
        {
            await gdbServer.StartAsync();
            
            // Break at entry point
            var currentEip = _cpu!.GetEip();
            if (!await gdbServer.HandleBreakAsync(currentEip, "Stopped at entry point"))
            {
                return; // Client disconnected or quit
            }

            // Run indefinitely until stop/exit requested
            while (!_stopRequested && !_env!.ExitRequested)
            {
                // Check if GDB wants to break
                currentEip = _cpu!.GetEip();
                if (gdbServer.ShouldBreak(currentEip) && !await gdbServer.HandleBreakAsync(currentEip))
                {
	                break; // Client disconnected or quit
                }

                // Execute one instruction
                var step = _cpu.SingleStep(_vm!);
                
                // Check for COM vtable method calls
                if (step.IsCall && _env.ComDispatcher.IsComVtableAddress(step.CallTarget))
                {
                    _logger.LogInformation("[COM] Vtable method call at address 0x{CallTarget:X8}", step.CallTarget);
                    
                    // Save callee-saved registers (EBX, ESI, EDI, EBP) per x86 calling convention
                    var savedEbx = _cpu.GetRegister("EBX");
                    var savedEsi = _cpu.GetRegister("ESI");
                    var savedEdi = _cpu.GetRegister("EDI");
                    var savedEbp = _cpu.GetRegister("EBP");
                    
                    if (_env.ComDispatcher.TryInvoke(step.CallTarget, _cpu, _vm!, out var ret, out var comArgBytes))
                    {
                        LogDebug($"[COM] Method returned 0x{ret:X8}");
                        var esp = _cpu.GetRegister("ESP");
                        var retEip = _vm!.Read32(esp);
                        // COM methods use stdcall convention - callee cleans up the stack
                        esp += 4 + (uint)comArgBytes; // Pop return address + arguments
                        _cpu.SetRegister("ESP", esp);
                        _cpu.SetRegister("EAX", ret); // Return value in EAX
                        _cpu.SetEip(retEip);
                        
                        // Restore callee-saved registers
                        _cpu.SetRegister("EBX", savedEbx);
                        _cpu.SetRegister("ESI", savedEsi);
                        _cpu.SetRegister("EDI", savedEdi);
                        _cpu.SetRegister("EBP", savedEbp);
                    }
                }
                else if (step.IsCall && _image!.ImportAddressMap.TryGetValue(step.CallTarget, out var imp))
                {
                    var dll = imp.dll.ToUpperInvariant();
                    var name = imp.name;
                    _logger.LogInformation("[Import] Hooked function: {Dll}!{Name} at address 0x{CallTarget:X8}", dll, name, step.CallTarget);
                    
                    // Save callee-saved registers (EBX, ESI, EDI, EBP) per x86 calling convention
                    var savedEbx = _cpu.GetRegister("EBX");
                    var savedEsi = _cpu.GetRegister("ESI");
                    var savedEdi = _cpu.GetRegister("EDI");
                    var savedEbp = _cpu.GetRegister("EBP");
                    
                    if (_dispatcher!.TryInvoke(dll, name, _cpu, _vm!, out var ret, out var argBytes))
                    {
                        LogDebug($"[Import] Returned 0x{ret:X8}");
                        var esp = _cpu.GetRegister("ESP");
                        var retEip = _vm!.Read32(esp);
                        esp += 4 + (uint)argBytes;
                        _cpu.SetRegister("ESP", esp);
                        _cpu.SetEip(retEip);
                        
                        // Restore callee-saved registers
                        _cpu.SetRegister("EBX", savedEbx);
                        _cpu.SetRegister("ESI", savedEsi);
                        _cpu.SetRegister("EDI", savedEdi);
                        _cpu.SetRegister("EBP", savedEbp);
                    }
                }
            }
        }
        finally
        {
            gdbServer.Dispose();
        }
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

    /// <summary>
    /// Attempts to restore EBP from the stack after an emulated API call.
    /// This handles cases where the calling code used EBP to hold the function pointer for an indirect call.
    /// </summary>
    private void RestoreEbpFromStack(uint esp)
    {
        try
        {
            var ebpFromStack = _vm!.Read32(esp);

            // Define plausible stack region (for example, 1MB stack)
            // Assume stack grows down, so stack base is the highest address, stack limit is lowest
            // Here, we use current ESP as the top of the stack, and allow up to 1MB below
            const uint STACK_SIZE = 0x100000; // 1MB
            var stackBottom = (esp > STACK_SIZE) ? (esp - STACK_SIZE) : 0x00100000; // Don't go below 1MB

            var inStackRegion = (ebpFromStack >= stackBottom) && (ebpFromStack <= esp);
            var isAligned = (ebpFromStack & 0x3) == 0;

            // Optionally, check that the memory at ebpFromStack is readable and contains a plausible saved EBP
            var savedEbpValid = false;
            if (inStackRegion && isAligned)
            {
                try
                {
                    var savedEbp = _vm!.Read32(ebpFromStack);
                    // Check that savedEbp is also within stack region (optional, but plausible)
                    savedEbpValid = (savedEbp >= stackBottom) && (savedEbp <= esp);
                }
                catch
                {
                    savedEbpValid = false;
                }
            }

            if (inStackRegion && isAligned && savedEbpValid)
            {
                _cpu!.SetRegister("EBP", ebpFromStack);
                _logger.LogDebug("[Emulator] Restored EBP from stack: 0x{EBP:X8}", ebpFromStack);
            }
            else
            {
                _logger.LogDebug("[Emulator] Skipped restoring EBP from stack: 0x{EBP:X8} (not a valid frame pointer)", ebpFromStack);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "[Emulator] Failed to restore EBP from stack");
        }
    }

    /// <summary>
    /// Start background UI event processing loop.
    /// This runs on a separate thread to continuously poll for events from rendering and input backends.
    /// </summary>
    private void StartEventProcessing()
    {
        if (_env == null)
        {
            return;
        }

        // Create cancellation token source for event processing
        _eventProcessingCts = new CancellationTokenSource();

        // Start the event processing task
        _eventProcessingTask = Task.Run(async () =>
        {
            LogDebug("[EventProcessing] Starting UI event processing loop");

            try
            {
                while (!_eventProcessingCts.Token.IsCancellationRequested && !_stopRequested)
                {
                    // Process events from input backend
                    try
                    {
                        _env.InputBackend?.ProcessEvents();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[EventProcessing] Error processing input backend events");
                    }

                    // Small delay to avoid busy-waiting (60 FPS event processing)
                    await Task.Delay(16, _eventProcessingCts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                LogDebug("[EventProcessing] UI event processing cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EventProcessing] Unexpected error in UI event processing loop");
            }

            LogDebug("[EventProcessing] UI event processing loop stopped");
        }, _eventProcessingCts.Token);
    }

    /// <summary>
    /// Stop the background UI event processing loop.
    /// </summary>
    private void StopEventProcessing()
    {
        if (_eventProcessingCts != null)
        {
            _eventProcessingCts.Cancel();
            _eventProcessingCts.Dispose();
            _eventProcessingCts = null;
        }

        if (_eventProcessingTask != null)
        {
            try
            {
                // Wait for the task to complete (with timeout)
                _eventProcessingTask.Wait(TimeSpan.FromSeconds(1));
            }
            catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
            {
                // Expected
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[EventProcessing] Error waiting for event processing task to complete");
            }
            finally
            {
                _eventProcessingTask?.Dispose();
                _eventProcessingTask = null;
            }
        }
    }

    public void Dispose()
    {
        // Stop event processing if running
        StopEventProcessing();
        _pauseEvent.Dispose();
    }
}

public interface IEmulatorHost
{
    void OnDebugOutput(string message, DebugLevel level);
    void OnStdOutput(string output);
    void OnWindowCreate(WindowCreateInfo info);
    Task<int> OnDialogCreate(DialogCreateInfo info);
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

public class DialogCreateInfo
{
    public required uint Handle { get; init; }
    public required Win32.DialogTemplate Template { get; init; }
    public uint ParentHandle { get; init; }
    public uint DialogProcAddress { get; init; }
    public uint InitParam { get; init; }
}
