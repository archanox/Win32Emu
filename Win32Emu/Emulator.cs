using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Runtime.InteropServices;
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
    private IAsyncCpu? _cpu;
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
    private readonly HashSet<uint> _patchedImportStubs = new();
    
    // Progress logging interval for emulation loop
    private const ulong PROGRESS_LOG_INTERVAL = 10000;
    
    // Logging throttle interval when stuck at same EIP (reduce spam)
    private const ulong STUCK_EIP_LOG_INTERVAL = 100000;

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
    /// Get the process environment (may be null if not initialized)
    /// </summary>
    public ProcessEnvironment? Environment => _env;
    
    /// <summary>
    /// Get the Win32 API dispatcher (may be null if not initialized)
    /// </summary>
    public Win32Dispatcher? Win32Dispatcher => _dispatcher;

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

    public void LoadExecutable(string path, string[]? programArgs = null, bool debugMode = false, bool interactiveDebugMode = false, int reservedMemoryMb = 256, bool gdbServerMode = false, int gdbServerPort = 1234, bool enableInstructionAnalyzer = false, bool enableLegacyInstructionDecoding = false, bool useJitCpu = false, bool useUnicornCpu = false)
    {
        _debugMode = debugMode;
        _interactiveDebugMode = interactiveDebugMode;
        _gdbServerMode = gdbServerMode;
        _gdbServerPort = gdbServerPort;

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File not found: {path}");
        }

        // Log system information
        var osDescription = RuntimeInformation.OSDescription;
        var processArchitecture = RuntimeInformation.ProcessArchitecture;
        _logger.LogInformation("[Loader] Host OS: {OSDescription}", osDescription);
        _logger.LogInformation("[Loader] Host Architecture: {ProcessArchitecture}", processArchitecture);

        LogDebug($"[Loader] Loading PE: {path}");
        // Convert MB to bytes for VirtualMemory constructor
        var memorySizeBytes = (ulong)reservedMemoryMb * 1024 * 1024;
        _vm = new VirtualMemory(memorySizeBytes);
        
        var configuredSizeMB = _vm.ConfiguredSize / (1024 * 1024);
        var addressSpaceSizeMB = _vm.Size / (1024 * 1024);
        _logger.LogInformation("[Memory] Configured size: {ConfiguredMB} MB, Address space: {AddressSpaceMB} MB (sparse, pages allocated on-demand)", 
            configuredSizeMB, addressSpaceSizeMB);
        var loader = new PeImageLoader(_vm, _logger);
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

        // Create CPU based on backend preference
        if (useUnicornCpu)
        {
            try
            {
                _cpu = new Cpu.Unicorn.UnicornCpu(_vm, _logger);
                LogDebug("[Loader] Unicorn CPU backend enabled (reference implementation)");
            }
            catch (ApplicationException ex) when (ex.Message.Contains("Control Flow Guard"))
            {
                // Unicorn cannot run with CFG enabled on Windows
                _logger.LogWarning("[Loader] Unicorn CPU backend is not compatible with Control Flow Guard (CFG). Falling back to IcedCpu.");
                _logger.LogWarning("[Loader] To use Unicorn, disable CFG in project properties or build without CFG.");
                CreateFallbackIcedCpu(decoderOptions, enableInstructionAnalyzer);
            }
            catch (Exception ex)
            {
                // Handle any other Unicorn initialization failures
                _logger.LogWarning(ex, "[Loader] Failed to initialize Unicorn CPU backend: {Message}. Falling back to IcedCpu.", ex.Message);
                CreateFallbackIcedCpu(decoderOptions, enableInstructionAnalyzer);
            }
        }
        else if (useJitCpu)
        {
            _cpu = new Cpu.Jit.JitCpu(_vm, _logger);
            LogDebug("[Loader] JIT CPU backend enabled (async-capable)");
        }
        else
        {
            _cpu = new IcedCpu(_vm, _logger, decoderOptions, enableInstructionAnalyzer);
            if (enableInstructionAnalyzer)
            {
                LogDebug("[Loader] Instruction analyzer enabled");
            }
        }
        
        // Log the actual CPU backend being used (after initialization and potential fallback)
        var actualCpuBackend = _cpu switch
        {
            Cpu.Unicorn.UnicornCpu => "Unicorn",
            Cpu.Jit.JitCpu => "JitCpu",
            IcedCpu => "IcedCpu",
            _ => "Unknown"
        };
        _logger.LogInformation("[Loader] Selected CPU Emulator: {CpuBackend}", actualCpuBackend);
        
        _cpu.SetEip(_image.EntryPointAddress);
        // Initialize stack using PE-provided SizeOfStackCommit when available
        var stackBase = 0x00200000u; // keep existing base location for now
        var commitSize = _image.SizeOfStackCommit;
        if (commitSize == 0)
        {
            commitSize = 0x8000; // sensible default if PE doesn't specify
        }
        if (commitSize >= stackBase)
        {
            // Avoid underflow; keep at least one page
            commitSize = stackBase - 0x1000;
        }
        var initialEsp = stackBase - commitSize;
        _cpu.SetRegister("ESP", initialEsp);
        _cpu.SetRegister("EBP", initialEsp); // Initialize frame pointer to match stack pointer

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
        _dispatcher.RegisterModule(new Comdlg32Module(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new DDrawModule(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new DSoundModule(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new DInputModule(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new WinMmModule(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new Msacm32Module(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new Glide2XModule(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new DPlayXModule(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new Ole32Module(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new Shell32Module(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new DsetupModule(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new MsvcrtModule(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new Wsock32Module(_env, _image.BaseAddress, loader, _logger));
        _dispatcher.RegisterModule(new Wavmix32Module(_env, _image.BaseAddress, loader, _logger));

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
        var iterationCount = 0ul;
        var lastLogTime = DateTime.UtcNow;
        
        // Infinite loop detection - track EIP to detect stuck loops
        var lastProgressEip = 0u;
        var sameEipCount = 0ul;
        // Stop emulation after 1M iterations at same EIP
        // This threshold allows legitimate tight loops (spinlocks, busy waits) to run
        // but catches applications stuck in true infinite loops (e.g., message pump with no messages)
        const ulong MAX_SAME_EIP_ITERATIONS = 1000000;

        // Run indefinitely until stop/exit requested or no threads running
        while (!_stopRequested && !_env!.ExitRequested)
        {
            iterationCount++;
            
            // Infinite loop detection - check every PROGRESS_LOG_INTERVAL iterations
            if (_logger.IsEnabled(LogLevel.Debug) && iterationCount % PROGRESS_LOG_INTERVAL == 0)
            {
                var now = DateTime.UtcNow;
                var elapsed = (now - lastLogTime).TotalMilliseconds;
                var progressEip = _cpu!.GetEip();
                var progressEsp = _cpu.GetRegister("ESP");
                
                // Check if we're stuck at the same EIP
                if (progressEip == lastProgressEip)
                {
                    sameEipCount += PROGRESS_LOG_INTERVAL;
                    
                    // Only log every STUCK_EIP_LOG_INTERVAL iterations when stuck to reduce spam
                    if (sameEipCount % STUCK_EIP_LOG_INTERVAL == 0)
                    {
                        _logger.LogWarning("[Emulator] Possible infinite loop: {SameEipCount} iterations at EIP=0x{Eip:X8}, ESP=0x{Esp:X8}", 
                            sameEipCount, progressEip, progressEsp);
                    }
                    
                    // Stop execution if we've been stuck too long
                    if (sameEipCount >= MAX_SAME_EIP_ITERATIONS)
                    {
                        _logger.LogError("[Emulator] INFINITE LOOP DETECTED: Stuck at EIP=0x{Eip:X8} for {Iterations} iterations. Stopping emulation.", 
                            progressEip, sameEipCount);
                        break;
                    }
                }
                else
                {
                    // EIP changed - reset counter and log progress
                    sameEipCount = 0;
                    _logger.LogDebug("[Emulator] Progress: {Iterations} iterations ({Elapsed:F2}ms), EIP=0x{Eip:X8}, ESP=0x{Esp:X8}", 
                        iterationCount, elapsed, progressEip, progressEsp);
                }
                
                lastProgressEip = progressEip;
                lastLogTime = now;
            }
            
            // DEBUG: Log EIP at start of each iteration to catch when it gets corrupted
            var eipAtLoopStart = _cpu!.GetEip();
            if (eipAtLoopStart >= 0x01000000 && eipAtLoopStart < 0x02000000)
            {
                var esp = _cpu.GetRegister("ESP");
                _logger.LogWarning("[Emulator] LOOP START: EIP=0x{Eip:X8} is already in suspicious range at loop start! ESP=0x{Esp:X8}", eipAtLoopStart, esp);
            }
            
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

            // Process wait timeouts BEFORE checking for runnable threads
            // This ensures sleeping threads are woken up before we check if any threads are runnable
            scheduler?.ProcessWaitTimeouts();

            // Check if we have any runnable threads
            if (scheduler != null && !scheduler.HasRunningThreads())
            {
                LogDebug("[Emulator] No more runnable threads, stopping execution");
                break;
            }

            // Check if we should context switch
            if (scheduler != null && scheduler.ShouldContextSwitch())
            {
                var eipBeforeSwitch = _cpu!.GetEip();
                var nextThread = scheduler.ContextSwitch(_cpu!);
                var eipAfterSwitch = _cpu!.GetEip();
                
                if (eipBeforeSwitch != eipAfterSwitch)
                {
                    _logger.LogWarning("[Emulator] Context switch changed EIP from 0x{Before:X8} to 0x{After:X8}",  eipBeforeSwitch, eipAfterSwitch);
                }
                
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

            // Defensive check: Detect and fix obviously invalid EBP before execution
            // EBP should generally point to a stack frame, not be 0 or very small values
            // This prevents crashes when code tries to access [EBP+offset] with invalid EBP
            ValidateAndFixEbp();

            // Check if EIP is in the import stub range but not properly mapped
            // This can happen if code returns to or jumps to an unmapped import address
            var currentEip = _cpu!.GetEip();
            if (currentEip >= 0x0F000000 && currentEip < 0x10000000)
            {
                // EIP is in the import stub address range
                // Import stubs are aligned to 16-byte boundaries (0x10)
                // We need to align down to check if this is a valid stub
                var alignedEip = currentEip & IMPORT_STUB_ALIGNMENT_MASK;
                
                // Get the current main executable (may have been updated with synthetic exports)
                var currentImage = _env!.GetMainExecutable() ?? _image!;
                
                if (!currentImage.ImportAddressMap.ContainsKey(alignedEip))
                {
                    // This import address is not mapped - simulate a return with error
                    var esp = _cpu.GetRegister("ESP");
                    _logger.LogError("[Import] Attempted to execute unmapped import stub at address 0x{Eip:X8} (aligned: 0x{AlignedEip:X8}, not in ImportAddressMap). ESP=0x{Esp:X8}, attempting to read return address from stack", currentEip, alignedEip, esp);
                    
                    try
                    {
                        // Read return address from stack and return
                        var retEip = _vm!.Read32(esp);
                        esp += 4; // Pop return address only
                        _cpu.SetRegister("ESP", esp);
                        _cpu.SetRegister("EAX", 0); // Return 0 as a safe default
                        _cpu.SetEip(retEip);
                        
                        _logger.LogWarning("[Import] Simulated return to 0x{RetEip:X8} with EAX=0", retEip);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[Import] Failed to simulate return - stack may be corrupted");
                        throw; // Re-throw if we can't recover
                    }
                    continue; // Skip to next iteration
                }
            }
            
            // Validate that EIP points to valid/mapped memory before execution
            // This catches bad jumps/returns early before they cause cascading errors
            const uint SUSPICIOUS_MEMORY_RANGE_START = 0x01000000;
            const uint SUSPICIOUS_MEMORY_RANGE_END = 0x02000000;
            
            var eipBeforeStep = _cpu!.GetEip();
            
            // Guard: detect execution in PE header region (e.g., 0x00400000–0x00401000)
            // This typically indicates a corrupted return address or bad jump target
            var imgForHeaderCheck = _env!.GetMainExecutable() ?? _image!;
            if (imgForHeaderCheck != null)
            {
                var imageBase = imgForHeaderCheck.BaseAddress;
                var headerEndVa = imageBase + imgForHeaderCheck.HeaderEndRva;
                if (eipBeforeStep >= imageBase && eipBeforeStep < headerEndVa)
                {
                    _logger.LogError("[Emulator] EIP=0x{Eip:X8} is in PE header region [0x{Base:X8}-0x{End:X8}). Attempting to recover by simulating a return.", eipBeforeStep, imageBase, headerEndVa);
                    if (TrySimulateReturn("PE header EIP recovery", eipBeforeStep))
                    {
                        continue;
                    }
                    else
                    {
                        throw new InvalidOperationException($"EIP=0x{eipBeforeStep:X8} in PE header region. Failed to auto-recover.");
                    }
                }
            }
            
            if (eipBeforeStep >= SUSPICIOUS_MEMORY_RANGE_START && eipBeforeStep < SUSPICIOUS_MEMORY_RANGE_END)
            {
                // EIP in range 0x01000000-0x01FFFFFF is suspicious - likely executing data or unmapped memory
                // This range is typically used for data segments, not code
                _logger.LogWarning("[Emulator] EIP=0x{Eip:X8} is in suspicious memory range (0x{Start:X8}-0x{End:X8}). This may indicate a bad jump or return address. Attempting to verify memory is mapped...", 
                    eipBeforeStep, SUSPICIOUS_MEMORY_RANGE_START, SUSPICIOUS_MEMORY_RANGE_END - 1);
                
                try
                {
                    // Try to read a few bytes to check if memory is mapped and accessible
                    // Reading 4 bytes validates enough memory for a typical instruction
                    _ = _vm!.Read32(eipBeforeStep);
                }
                catch (IndexOutOfRangeException ex)
                {
                    var esp = _cpu.GetRegister("ESP");
                    _logger.LogError(ex, "[Emulator] EIP=0x{Eip:X8} points to unmapped memory. ESP=0x{Esp:X8}. Execution cannot continue.", eipBeforeStep, esp);
                    throw new InvalidOperationException($"EIP=0x{eipBeforeStep:X8} points to unmapped memory. Likely a bad jump or corrupted return address.", ex);
                }
            }

            CpuStepResult step;
            try
            {
                step = _cpu!.SingleStep(_vm!);
            }
            catch (Exception ex)
            {
                var esp = _cpu!.GetRegister("ESP");
                var ebp = _cpu.GetRegister("EBP");
                _logger.LogError(ex, "[Emulator] Exception during SingleStep at EIP=0x{Eip:X8}, ESP=0x{Esp:X8}, EBP=0x{Ebp:X8}: {Message}", 
                    eipBeforeStep, esp, ebp, ex.Message);
                throw; // Re-throw to stop emulation
            }
            
            // Record instruction execution
            _metrics?.RecordInstructionsExecuted();
            
            // Validate EIP after execution to catch bad jumps/returns early
            var eipAfterStep = _cpu.GetEip();
            if (eipAfterStep > 0 && eipAfterStep < 0x00010000)
            {
                // EIP in low memory range (0x1-0xFFFF) is highly suspicious
                // This usually indicates a corrupted return address or bad function pointer
                var esp = _cpu.GetRegister("ESP");
                var ebp = _cpu.GetRegister("EBP");
                _logger.LogError("[Emulator] EIP=0x{Eip:X8} is in suspicious low memory range. Previous EIP=0x{PrevEip:X8}, ESP=0x{Esp:X8}, EBP=0x{Ebp:X8}. Likely corrupted return address or indirect jump.", 
                    eipAfterStep, eipBeforeStep, esp, ebp);
            }
            
            // Check for syscall (INT 0x80 from import stubs)
            // This is the retrowin32-style approach where import stubs CALL syscall dispatcher
            // The syscall dispatcher triggers INT 0x80, we handle it, then CPU executes RET naturally
            if (step.IsSyscall)
            {
                HandleSyscall();
                continue; // Continue to next iteration, let CPU execute RET
            }
            
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
                
                // Use consolidated helper for register preservation and stdcall convention
                CpuHelpers.InvokeWithRegisterPreservation(
                    _cpu,
                    _vm!,
                    () => {
                        var success = _env.ComDispatcher.TryInvoke(step.CallTarget, _cpu, _vm!, out var returnValue, out var argBytes);
                        return (success, returnValue, argBytes);
                    },
                    _vm!.Size,
                    _logger,
                    "COM vtable");
            }
            // OLD IMPORT HANDLING CODE - DISABLED
            // Import stubs now use CALL/RET and syscall mechanism (INT 0x80)
            // This old code intercepted calls to import stub addresses and manually manipulated EIP/ESP
            // which caused the infinite loop bug. Import handling now happens via syscall mechanism.
            /* 
            else if (step.IsCall && !IsImportStubAddress(step.CallTarget) && _image!.ImportAddressMap.TryGetValue(step.CallTarget, out var imp))
            {
                var dll = imp.dll.ToUpperInvariant();
                var name = imp.name;
                _logger.LogInformation("[Import] Hooked function: {Dll}!{Name} at address 0x{CallTarget:X8}", dll, name, step.CallTarget);
                
                // Save callee-saved registers (EBX, ESI, EDI, EBP) per x86 calling convention
                var saved = CpuHelpers.SaveCalleeSavedRegisters(_cpu);
                var ebpBeforeCall = saved.Ebp;
                var ebpWasValid = CpuHelpers.IsEbpValid(ebpBeforeCall, (uint)_vm!.Size);
                
                _logger.LogDebug("[Import] Before {Dll}!{Name}: EBP=0x{Ebp:X8} (valid={Valid})", dll, name, ebpBeforeCall, ebpWasValid);
                
                if (_dispatcher!.TryInvoke(dll, name, _cpu, _vm!, out var ret, out var argBytes))
                {
                    LogDebug($"[Import] Returned 0x{ret:X8}");
                    var esp = _cpu.GetRegister("ESP");
                    var retEip = _vm!.Read32(esp);
                    
                    esp += 4 + (uint)argBytes;
                    
                    _cpu.SetRegister("ESP", esp);
                    _cpu.SetEip(retEip);
                    
                    var ebpAfterCall = _cpu.GetRegister("EBP");
                    _logger.LogDebug("[Import] After {Dll}!{Name}: EBP=0x{Ebp:X8}", dll, name, ebpAfterCall);
                    
                    // Restore callee-saved registers, but skip EBP if it was invalid when saved
                    // This prevents restoring corrupted EBP values
                    CpuHelpers.RestoreCalleeSavedRegisters(_cpu, saved, skipInvalidEbp: true, memorySize: _vm!.Size);
                    
                    var ebpAfterRestore = _cpu.GetRegister("EBP");
                    if (ebpAfterRestore != ebpBeforeCall)
                    {
                        _logger.LogDebug("[Import] EBP selective restore: 0x{Before:X8} -> 0x{After:X8} (skipped invalid)", ebpBeforeCall, ebpAfterRestore);
                    }
                }
            }
            */
            else if (step.IsCall)
            {
	            // TODO: wire up to native program function overrides in c#
	            // _logger.LogInformation("[Call] Call method at 0x{CallTarget:X8}", step.CallTarget);
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

            // Check for extremely low EIP values that indicate corruption
            // Skip NULL (0) as that's handled separately
            // Exclude valid synthetic address ranges (COM vtables, syscalls, imports)
            var isValidSyntheticRange = currentEip >= COM_VTABLE_BASE && currentEip < IMPORT_HOOK_LIMIT;
            if (currentEip > 0 && currentEip < 0x00400000 && !isValidSyntheticRange)
            {
                _logger.LogWarning("[Emulator] EIP=0x{Eip:X8} is suspiciously low (< 0x00400000) at instruction {Instruction}. This likely indicates a corrupted function pointer, bad jump, or API returning invalid address.", currentEip, i);
                
                if (TrySimulateReturn("Low EIP recovery", currentEip))
                {
                    i++; // Count this as an instruction
                    continue; // Skip to next iteration
                }
                else
                {
                    throw new InvalidOperationException($"Failed to recover from corrupted EIP=0x{currentEip:X8}");
                }
            }

            if (currentEip is >= 0x0F000000 and < 0x10000000)
            {
                LogDebug("\n[Debug] *** CPU TRYING TO EXECUTE SYNTHETIC IMPORT ADDRESS! ***");
                LogDebug($"[Debug] EIP=0x{currentEip:X8} at instruction {i}");

                // Get the current main executable (may have been updated with synthetic exports)
                var currentImage = _env!.GetMainExecutable() ?? _image!;
                
                // Import stubs are aligned to 16-byte boundaries (0x10)
                // We need to align down to check if this is a valid stub
                var alignedEip = currentEip & IMPORT_STUB_ALIGNMENT_MASK;
                
                if (currentImage.ImportAddressMap.TryGetValue(alignedEip, out var importInfo))
                {
                    LogDebug($"[Debug] This is import: {importInfo.dll}!{importInfo.name}");
                    LogDebug("[Debug] This should now execute an INT3 stub that will be handled as an import call");
                }
                else
                {
                    // This import address is not mapped - simulate a return
                    LogDebug("[Debug] Unknown synthetic address - not in import map");
                    _logger.LogWarning("[Import] Attempted to execute unmapped import stub at address 0x{Eip:X8} (aligned: 0x{AlignedEip:X8}, not in ImportAddressMap). ESP=0x{Esp:X8}", 
                        currentEip, alignedEip, _cpu.GetRegister("ESP"));
                    
                    if (TrySimulateReturn("Unmapped import recovery", currentEip))
                    {
                        i++; // Count this as an instruction
                        continue; // Skip to next iteration
                    }
                    else
                    {
                        throw new InvalidOperationException($"Failed to recover from unmapped import at 0x{currentEip:X8}");
                    }
                }
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
                var step = debugger.SafeSingleStep();

                // Check for direct calls to import stubs
                // In some cases, the game may call import stubs directly instead of through the syscall mechanism
                if (step.IsCall && step.CallTarget >= 0x0F000000 && step.CallTarget < 0x10000000)
                {
                    if (HandleDirectImportCall(step.CallTarget))
                    {
                        i++;
                        continue;
                    }
                }

                // Check for syscall (INT 0x80 from import stubs)
                // This is the retrowin32-style approach where import stubs CALL syscall dispatcher
                // The syscall dispatcher triggers INT 0x80, we handle it, then CPU executes RET naturally
                if (step.IsSyscall)
                {
                    if (HandleSyscall())
                    {
                        i++;
                        continue;
                    }
                }

                // Check for COM vtable method calls
                if (step.IsCall && _env.ComDispatcher.IsComVtableAddress(step.CallTarget))
                {
                    _logger.LogInformation("[COM] Vtable method call at address 0x{CallTarget:X8}", step.CallTarget);
                    
                    // Use consolidated helper for register preservation and stdcall convention
                    CpuHelpers.InvokeWithRegisterPreservation(
                        _cpu,
                        _vm!,
                        () => {
                            var success = _env.ComDispatcher.TryInvoke(step.CallTarget, _cpu, _vm!, out var returnValue, out var argBytes);
                            if (success)
                            {
                                LogDebug($"[COM] Method returned 0x{returnValue:X8}");
                            }
                            return (success, returnValue, argBytes);
                        },
                        _vm!.Size,
                        _logger,
                        "COM vtable");
                }
                else if (step.IsCall && !IsImportStubAddress(step.CallTarget))
                {
                    // Get the current main executable (may have been updated with synthetic exports)
                    var currentImage = _env!.GetMainExecutable() ?? _image!;
                    
                    if (currentImage.ImportAddressMap.TryGetValue(step.CallTarget, out var imp))
                    {
                        var dll = imp.dll.ToUpperInvariant();
                        var name = imp.name;
                        _logger.LogInformation("[Import] Hooked function: {Dll}!{Name} at address 0x{CallTarget:X8}", dll, name, step.CallTarget);
                        
                        // Debug logging before invocation
                        var espBefore = _cpu.GetRegister("ESP");
                        LogDebug($"[Import] ESP before call: 0x{espBefore:X8}");
                        
                        // Use consolidated helper for register preservation and stdcall convention
                        var success = CpuHelpers.InvokeWithRegisterPreservation(
                            _cpu,
                            _vm!,
                            () => {
                                var invokeSuccess = _dispatcher!.TryInvoke(dll, name, _cpu, _vm!, out var returnValue, out var argBytes);
                                if (invokeSuccess)
                                {
                                    LogDebug($"[Import] Returned 0x{returnValue:X8}, argBytes={argBytes}");
                                }
                                return (invokeSuccess, returnValue, argBytes);
                            },
                            _vm!.Size,
                            _logger,
                            $"Import {dll}!{name}");
                        
                        if (!success)
                        {
                            _logger.LogError("[Import] Dispatcher failed to invoke {Dll}!{Name} at address 0x{CallTarget:X8}", dll, name, step.CallTarget);
                            _logger.LogError("[Import] This import is not implemented in the emulator");
                            _logger.LogWarning("[Import] Simulated return with EAX=0 (this may cause incorrect behavior)");
                        }
                        else
                        {
                            // Log final state after import return
                            LogDebug($"[Import] After return: EIP=0x{_cpu.GetEip():X8} ESP=0x{_cpu.GetRegister("ESP"):X8} EBP=0x{_cpu.GetRegister("EBP"):X8}");
                        }
                    }
                }
                else if (step.IsCall && step.CallTarget >= 0x0F000000 && step.CallTarget < 0x10000000)
                {
                    // This is a call to an address in the import stub range, but it's not in the ImportAddressMap
                    // This typically means the program is trying to call an import that wasn't loaded
                    _logger.LogError("[Import] Attempted to call unmapped import stub at address 0x{CallTarget:X8}", step.CallTarget);
                    _logger.LogError("[Import] This address is in the import stub range but not in the ImportAddressMap");
                    _logger.LogError("[Import] EIP=0x{Eip:X8} ESP=0x{Esp:X8}", _cpu.GetEip(), _cpu.GetRegister("ESP"));
                    
                    // To prevent executing into uninitialized memory (NOP padding after INT3),
                    // we need to simulate a return from this call
                    var esp = _cpu.GetRegister("ESP");
                    var retEip = _vm!.Read32(esp);
                    esp += 4; // Pop return address
                    _cpu.SetRegister("ESP", esp);
                    _cpu.SetRegister("EAX", 0); // Return 0 as a safe default
                    _cpu.SetEip(retEip);
                    
                    _logger.LogWarning("[Import] Simulated return to 0x{RetEip:X8} with EAX=0", retEip);
                }
            }
            catch (IndexOutOfRangeException ex) when (ex.Message.Contains("0xFFFFFFFD") || ex.Message.Contains("0xFFFFFFFF"))
            {
                LogDebug($"\n[Debug] *** CAUGHT MEMORY ACCESS VIOLATION AT INSTRUCTION {i} ***");
                LogDebug($"[Debug] Exception: {ex.Message}");

                if (currentEip is >= 0x0F000000 and < 0x10000000)
                {
                    LogDebug($"[Debug] ERROR CAUSE: Trying to execute synthetic import address 0x{currentEip:X8}");
                    
                    // Get the current main executable (may have been updated with synthetic exports)
                    var currentImage = _env!.GetMainExecutable() ?? _image!;
                    
                    if (currentImage.ImportAddressMap.TryGetValue(currentEip, out var importInfo))
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
            else if (step.IsCall && !IsImportStubAddress(step.CallTarget))
            {
                // Get the current main executable (may have been updated with synthetic exports)
                var currentImage = _env!.GetMainExecutable() ?? _image!;
                
                if (currentImage.ImportAddressMap.TryGetValue(step.CallTarget, out var imp))
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
                    
                    // Use consolidated helper for register preservation and stdcall convention
                    CpuHelpers.InvokeWithRegisterPreservation(
                        _cpu,
                        _vm!,
                        () => {
                            var success = _env.ComDispatcher.TryInvoke(step.CallTarget, _cpu, _vm!, out var returnValue, out var argBytes);
                            if (success)
                            {
                                LogDebug($"[COM] Method returned 0x{returnValue:X8}");
                            }
                            return (success, returnValue, argBytes);
                        },
                        _vm!.Size,
                        _logger,
                        "COM vtable (GDB)");
                }
                else if (step.IsCall && !IsImportStubAddress(step.CallTarget))
                {
                    // Get the current main executable (may have been updated with synthetic exports)
                    var currentImage = _env!.GetMainExecutable() ?? _image!;
                    
                    if (currentImage.ImportAddressMap.TryGetValue(step.CallTarget, out var imp))
                    {
                        var dll = imp.dll.ToUpperInvariant();
                        var name = imp.name;
                        _logger.LogInformation("[Import] Hooked function: {Dll}!{Name} at address 0x{CallTarget:X8}", dll, name, step.CallTarget);
                        
                        // Use consolidated helper for register preservation and stdcall convention
                        var success = CpuHelpers.InvokeWithRegisterPreservation(
                            _cpu,
                            _vm!,
                            () => {
                                var invokeSuccess = _dispatcher!.TryInvoke(dll, name, _cpu, _vm!, out var returnValue, out var argBytes);
                                if (invokeSuccess)
                                {
                                    LogDebug($"[Import] Returned 0x{returnValue:X8}");
                                }
                                return (invokeSuccess, returnValue, argBytes);
                            },
                            _vm!.Size,
                            _logger,
                            $"Import {dll}!{name} (GDB)");
                        
                        if (!success)
                        {
                            _logger.LogError("[Import] Dispatcher failed to invoke {Dll}!{Name}", dll, name);
                        }
                    }
                }
            }
        }
        finally
        {
            gdbServer.Dispose();
        }
    }

    private static bool WillBeCall(ICpu cpu, VirtualMemory vm)
    {
        try
        {
            var eip = cpu.GetEip();
            var opcode = vm.Read8(eip);

            if (opcode == 0xE8 || (opcode == 0xFF && IsCallVariant(vm, eip)))
            {
                return true;
            }

            // Check for INT3 breakpoints in emulation stub ranges (COM vtables, synthetic exports, import hooks)
            if (opcode == 0xCC && eip is >= COM_VTABLE_BASE and < IMPORT_HOOK_LIMIT)
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

    /// <summary>
    /// Checks if an address is in the import stub range (0x0F000000-0x10000000).
    /// Import stubs now use CALL/RET and syscall mechanism, so they should not be
    /// intercepted as direct import calls.
    /// </summary>
    private static bool IsImportStubAddress(uint address)
    {
        return address >= 0x0F000000 && address < 0x10000000;
    }

    /// <summary>
    /// Handles direct calls to import stub addresses.
    /// Returns true if the call was handled and execution should continue to next instruction.
    /// </summary>
    private bool HandleDirectImportCall(uint callTarget)
    {
        var currentImage = _env!.GetMainExecutable() ?? _image!;
        
        // Align the call target to 16-byte boundary (import stubs are aligned)
        var alignedTarget = callTarget & IMPORT_STUB_ALIGNMENT_MASK;
        
        if (currentImage.ImportAddressMap.TryGetValue(alignedTarget, out var imp))
        {
            var dll = imp.dll.ToUpperInvariant();
            var name = imp.name;
            _logger.LogInformation("[Import] Direct call to {Dll}!{Name} at 0x{CallTarget:X8}", dll, name, callTarget);
            
            // Use consolidated helper for register preservation and stdcall convention
            var success = CpuHelpers.InvokeWithRegisterPreservation(
                _cpu,
                _vm!,
                () => {
                    var invokeSuccess = _dispatcher!.TryInvoke(dll, name, _cpu, _vm!, out var returnValue, out var argBytes);
                    return (invokeSuccess, returnValue, argBytes);
                },
                _vm!.Size,
                _logger,
                $"Import {dll}!{name}");
            
            if (!success)
            {
                _logger.LogError("[Import] Dispatcher failed to invoke {Dll}!{Name}", dll, name);
            }
            else
            {
                _logger.LogDebug("[Import] Successfully invoked {Dll}!{Name}", dll, name);
            }
            
            return true;
        }
        else
        {
            _logger.LogError("[Import] Attempted to call unmapped import stub at address 0x{CallTarget:X8}", callTarget);
            _logger.LogError("[Import] This address is in the import stub range but not in the ImportAddressMap");
            _logger.LogError("[Import] EIP=0x{Eip:X8} ESP=0x{Esp:X8}", _cpu.GetEip(), _cpu.GetRegister("ESP"));
            
            // Simulate return
            var esp = _cpu.GetRegister("ESP");
            var retEip = _vm!.Read32(esp);
            esp += 4;
            _cpu.SetRegister("ESP", esp);
            _cpu.SetRegister("EAX", 0);
            _cpu.SetEip(retEip);
            
            _logger.LogWarning("[Import] Simulated return to 0x{RetEip:X8} with EAX=0", retEip);
            
            return true;
        }
    }

    /// <summary>
    /// Handles syscall (INT 0x80) from import stubs.
    /// Returns true if the syscall was handled and execution should continue to next instruction.
    /// </summary>
    private bool HandleSyscall()
    {
        // The stack looks like:
        // [ESP+0] = return address to import stub (points to RET instruction after CALL)
        // [ESP+4+] = function arguments (pushed by original caller)
        
        var esp = _cpu.GetRegister("ESP");
        
        // Validate ESP is in a reasonable range before attempting to read from stack
        if (esp < 0x00010000)
        {
            _logger.LogError("[Syscall] ESP=0x{Esp:X8} is suspiciously low (< 0x10000). Skipping syscall.", esp);
            _cpu.SetRegister("EAX", 0); // Return 0 as error
            return true;
        }
        
        // Read the return address - this points to the RET instruction in the import stub
        var retToStub = _vm!.Read32(esp);
        
        // The import stub address is 5 bytes before the return address
        // (5 bytes for CALL instruction, then RET is at +5, which is what retToStub points to)
        var importStubAddr = retToStub - 5;
        
        // Get the current main executable (may have been updated with synthetic exports)
        var currentImage = _env!.GetMainExecutable() ?? _image!;
        
        // Look up which import this is
        if (currentImage.ImportAddressMap.TryGetValue(importStubAddr, out var imp))
        {
            var dll = imp.dll.ToUpperInvariant();
            var name = imp.name;
            _logger.LogInformation("[Syscall] {Dll}!{Name} from stub at 0x{Stub:X8}", dll, name, importStubAddr);
            
            // Save callee-saved registers (EBX, ESI, EDI, EBP per stdcall convention)
            var saved = CpuHelpers.SaveCalleeSavedRegisters(_cpu);
            
            // Temporarily adjust ESP to skip the return-to-stub address on the stack.
            // This allows StackArgs to read arguments at the correct offsets.
            //
            // Current stack layout:
            //   [ESP+0] = return address to import stub (after CALL to syscall dispatcher)
            //   [ESP+4] = return address to caller (after CALL to import stub)
            //   [ESP+8] = arg1
            //   [ESP+12] = arg2, etc.
            //
            // After adjustment (ESP += 4):
            //   [ESP+0] = return address to caller  
            //   [ESP+4] = arg1  (StackArgs reads this for index 0)
            //   [ESP+8] = arg2  (StackArgs reads this for index 1)
            //
            // This matches retrowin32's approach where they pass stack_args = esp + 8
            // (skipping both return addresses) to their generated wrapper functions.
            // We restore ESP before returning so the CPU can execute RET naturally.
            var originalEsp = esp;
            
            // DEBUG: Log stack contents before API call (only in normal execution path for performance)
            var returnToCallerAddr = originalEsp + 4;
            var returnToCaller = _vm!.Read32(returnToCallerAddr);
            _logger.LogInformation("[Syscall] BEFORE API: Return address at 0x{Addr:X8} = 0x{RetAddr:X8}", returnToCallerAddr, returnToCaller);
            
            _cpu.SetRegister("ESP", esp + 4);
            
            if (_dispatcher!.TryInvoke(dll, name, _cpu, _vm!, out var ret, out var argBytes))
            {
                // DEBUG: Log stack contents after API call
                var returnToCallerAfter = _vm!.Read32(returnToCallerAddr);
                _logger.LogInformation("[Syscall] AFTER API: Return address at 0x{Addr:X8} = 0x{RetAddr:X8}", returnToCallerAddr, returnToCallerAfter);
                
                // VALIDATION: Detect stack corruption by checking if return address changed during API call
                if (returnToCaller != returnToCallerAfter)
                {
                    _logger.LogError("[Syscall] STACK CORRUPTION DETECTED: Return address changed from 0x{Before:X8} to 0x{After:X8} during {Dll}!{Name} call. This indicates the API corrupted the stack.", 
                        returnToCaller, returnToCallerAfter, dll, name);
                    
                    // Additional diagnostic: Check if the new return address is in unmapped import range
                    if (returnToCallerAfter >= 0x0F000000 && returnToCallerAfter < 0x10000000)
                    {
                        var alignedAddr = returnToCallerAfter & 0xFFFFFFF0u;
                        var isMapped = currentImage.ImportAddressMap.ContainsKey(alignedAddr);
                        _logger.LogError("[Syscall] Corrupted return address 0x{Addr:X8} is in import stub range. Aligned: 0x{Aligned:X8}, Mapped: {Mapped}", 
                            returnToCallerAfter, alignedAddr, isMapped);
                        
                        if (!isMapped)
                        {
                            var importCount = currentImage.ImportAddressMap.Count;
                            // Calculate import index using aligned address to ensure correct calculation
                            var wouldBeIndex = (alignedAddr - 0x0F000000) / 0x10;
                            _logger.LogError("[Syscall] This would be import index {Index} but only {Count} imports exist (indices 0-{MaxIndex}). " +
                                "This is likely a C runtime bug with uninitialized function pointer or array bounds issue.",
                                wouldBeIndex, importCount, importCount - 1);
                        }
                    }
                }
                
                // Set return value in EAX (stdcall convention)
                _cpu.SetRegister("EAX", ret);
                
                // Restore ESP to original value so CPU can execute RET instructions naturally
                _cpu.SetRegister("ESP", originalEsp);
                
                // Restore callee-saved registers (with EBP validation to prevent corruption)
                CpuHelpers.RestoreCalleeSavedRegisters(_cpu, saved, skipInvalidEbp: true, memorySize: _vm!.Size);
                
                // Validate register state after syscall (helps diagnose corruption issues)
                // Uses logging level check instead of debug mode to allow selective enablement
                CpuHelpers.ValidateRegisterState(_cpu, saved, _vm!.Size, _logger, $"Syscall {dll}!{name}", LogLevel.Debug);
                
                _logger.LogDebug("[Syscall] Returned 0x{Ret:X8}, argBytes={ArgBytes}, CPU will execute RET naturally", ret, argBytes);
                
                // Patch the import stub's RET instruction with the correct argBytes value for stdcall cleanup
                // The stub RET instruction is at importStubAddr + 5 (after the 5-byte CALL instruction)
                // Format: RET imm16 = 0xC2 <low_byte> <high_byte>
                // Only patch if not already patched to avoid redundant memory writes
                if (argBytes <= 0xFFFF && !_patchedImportStubs.Contains(importStubAddr))
                {
                    var retInstrAddr = importStubAddr + 5;
                    var opcode = _vm!.Read8(retInstrAddr);
                    if (opcode == 0xC2)
                    {
                        _vm!.Write8(retInstrAddr + 1, (byte)(argBytes & 0xFF));
                        _vm!.Write8(retInstrAddr + 2, (byte)((argBytes >> 8) & 0xFF));
                        _patchedImportStubs.Add(importStubAddr);
                        _logger.LogDebug("[Syscall] Patched RET at 0x{RetAddr:X8} with argBytes={ArgBytes}", retInstrAddr, argBytes);
                    }
                    else
                    {
                        _logger.LogWarning("[Syscall] Expected RET imm16 (0xC2) at 0x{RetAddr:X8} but found 0x{Opcode:X2}. Skipping patch.", retInstrAddr, opcode);
                    }
                }
                
                // The CPU will now execute the RET instruction in the syscall dispatcher,
                // which returns to the import stub, which then executes its RET imm16
                // to return to the original caller with proper stack cleanup!
                
                // Validate that the return-to-stub address looks reasonable
                if (retToStub < 0x0F000000 || retToStub >= 0x10000000)
                {
                    _logger.LogWarning("[Syscall] Return-to-stub address 0x{RetToStub:X8} is outside import stub range [0x0F000000-0x10000000). This may indicate stack corruption.", retToStub);
                }
                
                // Validate ESP is in a reasonable range (not extremely small)
                var restoredEsp = _cpu.GetRegister("ESP");
                if (restoredEsp < 0x00010000)
                {
                    _logger.LogError("[Syscall] ESP=0x{Esp:X8} after syscall return is suspiciously low. This indicates possible stack corruption.", restoredEsp);
                }
            }
            else
            {
                _logger.LogError("[Syscall] Dispatcher failed to invoke {Dll}!{Name}", dll, name);
                _cpu.SetRegister("EAX", 0);
                // Restore ESP to original value
                _cpu.SetRegister("ESP", originalEsp);
                // Restore callee-saved registers (with EBP validation)
                CpuHelpers.RestoreCalleeSavedRegisters(_cpu, saved, skipInvalidEbp: true, memorySize: _vm!.Size);
            }
        }
        else
        {
            _logger.LogError("[Syscall] Unknown import stub at 0x{Stub:X8} (retAddr=0x{RetAddr:X8})", importStubAddr, retToStub);
            _cpu.SetRegister("EAX", 0);
        }
        
        return true;
    }

    private static uint GetCallTarget(ICpu cpu, VirtualMemory vm)
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

            // INT3 breakpoints in emulation stub ranges return their own address as the target
            if (opcode == 0xCC && eip is >= COM_VTABLE_BASE and < IMPORT_HOOK_LIMIT)
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

    private void CreateFallbackIcedCpu(Iced.Intel.DecoderOptions decoderOptions, bool enableInstructionAnalyzer)
    {
        // _vm is guaranteed to be non-null here as this method is only called from LoadExecutable
        // after _vm has been initialized
        if (_vm == null)
        {
            throw new InvalidOperationException("Virtual memory must be initialized before creating fallback CPU.");
        }
        _cpu = new IcedCpu(_vm, _logger, decoderOptions, enableInstructionAnalyzer);
        LogDebug("[Loader] IcedCpu backend enabled (fallback from Unicorn)");
    }

    /// <summary>
    /// Attempts to restore EBP from the stack after an emulated API call.
    /// This handles cases where the calling code used EBP to hold the function pointer for an indirect call.
    /// </summary>
    
    // Memory address ranges for various emulation stubs (all use INT3/0xCC for interception)
    private const uint COM_VTABLE_BASE = 0x0D000000;      // COM interface vtable methods
    private const uint COM_VTABLE_LIMIT = 0x0E000000;
    private const uint SYNTHETIC_EXPORT_BASE = 0x0E000000; // Dynamically resolved exports (e.g., GetProcAddress)
    private const uint SYNTHETIC_EXPORT_LIMIT = 0x0F000000;
    private const uint IMPORT_HOOK_BASE = 0x0F000000;      // Static import table hooks
    private const uint IMPORT_HOOK_LIMIT = 0x10000000;
    private const uint IMPORT_STUB_ALIGNMENT_MASK = 0xFFFFFFF0u; // 16-byte alignment for import stubs
    
    // Constants for EBP validation
    private const uint HEAP_BASE = 0x01000000;            // Start of heap region
    private const uint HEAP_LIMIT = 0x70000000;           // End of heap region
    private const uint MIN_VALID_EBP = 0x1000;            // Minimum valid EBP (4KB)
    private const uint DEFAULT_STACK_BOTTOM = 0x00100000; // Default stack bottom (1MB)
    private const uint STACK_SIZE = 0x100000;             // Assumed stack size (1MB)
    private const uint STACK_SLACK_BYTES = 0x1000;        // Stack slack above ESP (4KB)
    
    /// <summary>
    /// Attempts to recover from a corrupted EIP by simulating a return instruction.
    /// Pops the return address from the stack, sets EAX to 0, and updates EIP.
    /// Validates that the return address is plausible before using it.
    /// </summary>
    /// <param name="reason">Description of why recovery is needed (for logging)</param>
    /// <param name="corruptedEip">The corrupted EIP value that triggered recovery</param>
    /// <returns>True if recovery succeeded, false otherwise</returns>
    private bool TrySimulateReturn(string reason, uint corruptedEip)
    {
        try
        {
            var esp = _cpu!.GetRegister("ESP");
            var retEip = _vm!.Read32(esp);
            
            // Validate the return address is plausible
            // Reject obviously invalid addresses: NULL or extremely high (kernel space)
            if (retEip == 0 || retEip >= 0x80000000)
            {
                _logger.LogWarning("[Emulator] {Reason} at 0x{CorruptedEip:X8}: Return address 0x{RetEip:X8} from stack is invalid (NULL or kernel space), recovery aborted", 
                    reason, corruptedEip, retEip);
                return false;
            }
            
            // Warn if return address looks suspicious but allow recovery to proceed
            var isInImageRange = retEip >= 0x00400000 && retEip < 0x80000000;
            var isInSyntheticRange = retEip >= COM_VTABLE_BASE && retEip < IMPORT_HOOK_LIMIT;
            if (!isInImageRange && !isInSyntheticRange)
            {
                _logger.LogWarning("[Emulator] {Reason} at 0x{CorruptedEip:X8}: Return address 0x{RetEip:X8} is outside typical code ranges but attempting recovery anyway", 
                    reason, corruptedEip, retEip);
            }
            
            esp += 4; // Pop return address only
            _cpu.SetRegister("ESP", esp);
            _cpu.SetRegister("EAX", 0); // Return 0 as a safe default
            _cpu.SetEip(retEip);
            
            _logger.LogDebug("[Emulator] {Reason} at 0x{CorruptedEip:X8}: Simulated return to 0x{RetEip:X8} with EAX=0", 
                reason, corruptedEip, retEip);
            return true;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "[Emulator] {Reason} at 0x{CorruptedEip:X8}: Failed to simulate return - invalid argument", 
                reason, corruptedEip);
            return false;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "[Emulator] {Reason} at 0x{CorruptedEip:X8}: Failed to simulate return - invalid operation", 
                reason, corruptedEip);
            return false;
        }
        catch (IndexOutOfRangeException ex)
        {
            _logger.LogError(ex, "[Emulator] {Reason} at 0x{CorruptedEip:X8}: Failed to simulate return - stack may be corrupted", 
                reason, corruptedEip);
            return false;
        }
    }

    /// <summary>
    /// Validates EBP register and fixes it if it contains an obviously invalid value.
    /// This prevents crashes when code tries to access [EBP+offset] with invalid EBP.
    /// </summary>
    private void ValidateAndFixEbp()
    {
        var ebp = _cpu!.GetRegister("EBP");
        var esp = _cpu!.GetRegister("ESP");
        
        // Define plausible stack region
        var stackBottom = (esp >= STACK_SIZE) ? (esp - STACK_SIZE) : DEFAULT_STACK_BOTTOM;
        
        // Check if EBP is within reasonable stack range
        var ebpInStackRegion = (ebp >= stackBottom) && (ebp <= esp + STACK_SLACK_BYTES);
        
        // Check if EBP is aligned (should be 4-byte aligned)
        var ebpAligned = (ebp & 0x3) == 0;
        
        // Check for obviously invalid values
        var ebpIsZero = (ebp == 0);
        var ebpIsVerySmall = (ebp < MIN_VALID_EBP);
        var ebpIsImportHook = (ebp >= IMPORT_HOOK_BASE && ebp < IMPORT_HOOK_LIMIT);
        var ebpIsBeyondMemory = (ebp >= _vm!.Size);
        
        // Check if EBP looks like a COM/heap pointer being used for special purposes
        var ebpIsHeapPointer = (ebp >= HEAP_BASE && ebp < HEAP_LIMIT) && !ebpInStackRegion;
        
        // If EBP is clearly invalid and not a special-purpose pointer, fix it
        if ((ebpIsZero || ebpIsVerySmall) && !ebpIsHeapPointer)
        {
            _cpu!.SetRegister("EBP", esp);
            _logger.LogTrace("[Emulator] Reset invalid EBP 0x{OldEBP:X8} to ESP 0x{NewEBP:X8} (zero/too small)", ebp, esp);
        }
        else if (ebpIsImportHook)
        {
            // EBP contains an import hook address - reset to ESP
            _cpu!.SetRegister("EBP", esp);
            _logger.LogTrace("[Emulator] Reset EBP from import hook 0x{OldEBP:X8} to ESP 0x{NewEBP:X8}", ebp, esp);
        }
        else if (ebpIsBeyondMemory)
        {
            // EBP is beyond valid memory range - reset to ESP
            _cpu!.SetRegister("EBP", esp);
            _logger.LogTrace("[Emulator] Reset EBP beyond memory 0x{OldEBP:X8} to ESP 0x{NewEBP:X8} (size=0x{Size:X})", ebp, esp, _vm!.Size);
        }
        else if (!ebpAligned && ebpInStackRegion)
        {
            // EBP is unaligned but in stack region - this is clearly wrong
            _cpu!.SetRegister("EBP", esp);
            _logger.LogTrace("[Emulator] Reset unaligned EBP 0x{OldEBP:X8} to ESP 0x{NewEBP:X8}", ebp, esp);
        }
        // Otherwise, leave EBP alone - it might be a valid heap pointer or special-purpose value
    }
    
    private void RestoreEbpFromStack(uint esp)
    {
        CpuHelpers.RestoreEbpFromStack(_cpu!, _vm!, esp, _logger, "Emulator");
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
                    // Process events from all subscribed rendering and input backends
                    // This includes GLFW window events which are critical for window responsiveness
                    try
                    {
                        _env.ProcessAllBackendEvents();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[EventProcessing] Error processing backend events");
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
    void OnDialogEnd(uint dialogHandle, int result);
    int OnMessageBox(MessageBoxInfo info);
    void OnDialogControlTextChanged(uint dialogHandle, int controlId, string text);
    void OnDialogControlBitmapChanged(uint dialogHandle, int controlId, byte[] bitmapData);
    void OnDisplayUpdate(DisplayUpdateInfo info);
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
    public Dictionary<int, uint> ControlHandles { get; init; } = new();
}

public class MessageBoxInfo
{
    public uint ParentHandle { get; init; }
    public required string Text { get; init; }
    public required string Caption { get; init; }
    public uint Type { get; init; }
}

public class DisplayUpdateInfo
{
    public required byte[] FrameBuffer { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int Stride { get; init; }
}
