using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using Win32Emu.Cpu;
using Win32Emu.Cpu.Iced;
using Win32Emu.Debugging;
using Win32Emu.Diagnostics;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.Rendering;
using Win32Emu.Threading;
using Win32Emu.Win32;
using Win32Emu.Win32.Modules;

namespace Win32Emu;

public sealed class Emulator : IDisposable
{
    private readonly IEmulatorHost? _host;
    private readonly ILogger _logger;
    private readonly Telemetry.TelemetryService? _telemetryService;
    private readonly Telemetry.EmulatorMetrics? _metrics;
    private readonly IBackendFactory? _backendFactory;
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
    private Exception? _lastException;
    private byte[]? _executableBytes; // Store executable bytes for resource reading when loaded from VHD
    
    // Actual memory layout from PE headers
    private uint _stackBase;
    private uint _stackLimit; // Bottom of stack (lowest address)
    private uint _heapBase;
    
    // Progress logging interval for emulation loop
    private const ulong PROGRESS_LOG_INTERVAL = 10000;
    
    // Logging throttle interval when stuck at same EIP (reduce spam)
    // Log a warning every 1M iterations to avoid excessive log spam during legitimate tight loops
    private const ulong STUCK_EIP_LOG_INTERVAL = 1000000;
    
    // WASM yield interval - yield to browser event loop every N iterations
    // This prevents the browser from freezing when emulating tight loops.
    // Set to 100 for better responsiveness on WASM - yields every ~0.1-1ms on modern hardware.
    // Lower values improve UI responsiveness but reduce emulation throughput.
    private const ulong WASM_YIELD_INTERVAL = 100;
    
    // Instruction tracing for debugging BasicDD crash
    private int _instructionTraceCount = 0;
    private const int MAX_TRACE_INSTRUCTIONS = 1000; // Trace 1000 instructions to find stack corruption
    private bool _traceEnabled = false;
    
    // BasicDD.exe workaround configuration
    private const uint BASICDD_EPILOGUE_PATCH_ADDRESS = 0x00401412u;
    private const byte BASICDD_ORIGINAL_STACK_ADJUSTMENT = 0x8C;  // 140 bytes
    private const byte BASICDD_CORRECTED_STACK_ADJUSTMENT = 0x94; // 148 bytes (adds 8 bytes)
    private uint? _traceTriggerStartAddress = null;
    private uint? _traceTriggerEndAddress = null;
    private string? _traceTriggerDll = null;
    private string? _traceTriggerFunction = null;
    
    /// <summary>
    /// Enable instruction-level tracing for the next N instructions.
    /// Used for debugging crashes and understanding execution flow.
    /// This method is intended for use by interactive debuggers or diagnostic tools
    /// to manually trigger instruction tracing at specific points in execution.
    /// </summary>
    /// <param name="instructionCount">Number of instructions to trace (default: MAX_TRACE_INSTRUCTIONS)</param>
    public void EnableInstructionTracing(int instructionCount = MAX_TRACE_INSTRUCTIONS)
    {
        _instructionTraceCount = instructionCount;
        _traceEnabled = true;
        _logger.LogWarning("[TRACE] Instruction tracing enabled for next {Count} instructions", instructionCount);
    }

    public Emulator(IEmulatorHost? host = null, ILogger? logger = null, Telemetry.TelemetryService? telemetryService = null, IBackendFactory? backendFactory = null)
    {
        _host = host;
        _logger = logger ?? NullLogger.Instance;
        _telemetryService = telemetryService;
        _backendFactory = backendFactory;
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
    /// Get the last exception that occurred during emulation (may be null if no exception occurred)
    /// </summary>
    public Exception? LastException => _lastException;

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

    public LoadedImage? LoadedImage => _image;
    
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

    /// <summary>
    /// Load an executable directly from a byte array without requiring file system access.
    /// This is the primary entry point for WASM and other sandboxed environments.
    /// </summary>
    /// <param name="executableBytes">The raw bytes of the PE executable</param>
    /// <param name="executableName">Display name for the executable (e.g., "game.exe")</param>
    /// <param name="programArgs">Optional command-line arguments</param>
    /// <param name="debugMode">Enable enhanced debugging</param>
    /// <param name="reservedMemoryMb">Memory to reserve for emulation (default: 256 MB)</param>
    public void LoadExecutableFromBytes(byte[] executableBytes, string executableName, string[]? programArgs = null, bool debugMode = false, int reservedMemoryMb = 256)
    {
        LoadExecutableFromBytes(executableBytes, executableName, programArgs, debugMode, reservedMemoryMb, virtualFileSystem: null);
    }

    /// <summary>
    /// Loads an executable from bytes with an optional custom virtual file system.
    /// This overload is useful for WASM scenarios where the VFS is browser-based.
    /// </summary>
    /// <param name="executableBytes">The executable bytes to load</param>
    /// <param name="executableName">The name of the executable file</param>
    /// <param name="programArgs">Optional program arguments</param>
    /// <param name="debugMode">Enable debug mode</param>
    /// <param name="reservedMemoryMb">Reserved memory in megabytes</param>
    /// <param name="virtualFileSystem">Optional custom virtual file system for file operations</param>
    public void LoadExecutableFromBytes(byte[] executableBytes, string executableName, string[]? programArgs, bool debugMode, int reservedMemoryMb, VirtualFileSystem.IVirtualFileSystem? virtualFileSystem)
    {
        // Use a synthetic path for internal tracking
        var syntheticPath = $"C:\\WASM\\{executableName}";
        
        // Call the main LoadExecutable with pre-loaded bytes
        LoadExecutable(
            path: syntheticPath, 
            programArgs: programArgs, 
            debugMode: debugMode, 
            interactiveDebugMode: false, 
            reservedMemoryMb: reservedMemoryMb, 
            gdbServerMode: false, 
            gdbServerPort: 1234, 
            enableInstructionAnalyzer: false, 
            enableLegacyInstructionDecoding: false, 
            useJitCpu: false, 
            virtualDiskPath: null,
            preloadedBytes: executableBytes,
            customVirtualFileSystem: virtualFileSystem);
    }

    public void LoadExecutable(string path, string[]? programArgs = null, bool debugMode = false, bool interactiveDebugMode = false, int reservedMemoryMb = 256, bool gdbServerMode = false, int gdbServerPort = 1234, bool enableInstructionAnalyzer = false, bool enableLegacyInstructionDecoding = false, bool useJitCpu = false, string? virtualDiskPath = null, byte[]? preloadedBytes = null, VirtualFileSystem.IVirtualFileSystem? customVirtualFileSystem = null)
    {
        _debugMode = debugMode;
        _interactiveDebugMode = interactiveDebugMode;
        _gdbServerMode = gdbServerMode;
        _gdbServerPort = gdbServerPort;

        // When using a virtual disk, extract the executable from VFS to memory
        byte[]? executableBytes = preloadedBytes;
        
        if (executableBytes != null)
        {
            // Bytes were pre-loaded (e.g., from WASM), skip file system access
            _logger.LogInformation("[Loader] Using pre-loaded executable bytes ({Size} bytes)", executableBytes.Length);
        }
        else if (!string.IsNullOrEmpty(virtualDiskPath))
        {
            _logger.LogInformation("[Loader] Extracting executable from virtual disk: {VfsPath}", path);
            
            // Open the VFS temporarily to extract the executable
            using (var diskVfs = new VirtualFileSystem.DiskVirtualFileSystem(virtualDiskPath, _logger))
            {
                // Convert Windows path to VFS path (e.g., C:\ign_teas\IGN_TEAS.EXE -> \ign_teas\IGN_TEAS.EXE)
                var vfsPath = path;
                if (vfsPath.Length >= 2 && vfsPath[1] == ':')
                {
                    // Remove drive letter (e.g., "C:\foo" -> "\foo")
                    vfsPath = vfsPath.Substring(2);
                }
                
                // Open and read the file from VFS
                var fileHandle = diskVfs.OpenFile(vfsPath, VirtualFileSystem.VfsFileMode.Open, VirtualFileSystem.VfsFileAccess.Read);
                if (fileHandle == null)
                {
                    throw new FileNotFoundException($"File not found in virtual disk: {vfsPath}");
                }
                
                using (fileHandle)
                {
                    // Get file length by seeking to the end
                    var fileLength = fileHandle.Seek(0, SeekOrigin.End);
                    fileHandle.Seek(0, SeekOrigin.Begin); // Reset to beginning
                    
                    executableBytes = new byte[fileLength];
                    fileHandle.Read(executableBytes, 0, (int)fileLength);
                    
                    _logger.LogInformation("[Loader] Extracted {FileSize} bytes from virtual disk", fileLength);
                }
            }
        }
        else if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File not found: {path}");
        }

        // Log system information
        _logger.LogInformation("[Loader] Host OS: {OSDescription}", RuntimeInformation.OSDescription);
		_logger.LogInformation("[Loader] Host OS Architecture: {OSArchitecture}", RuntimeInformation.OSArchitecture);
		_logger.LogInformation("[Loader] Host Process Architecture: {ProcessArchitecture}", RuntimeInformation.ProcessArchitecture);
		_logger.LogInformation("[Loader] Host Framework: {FrameworkDescription}", RuntimeInformation.FrameworkDescription);
		_logger.LogInformation("[Loader] Host Runtime Identifier: {RuntimeIdentifier}", RuntimeInformation.RuntimeIdentifier);

		LogDebug($"[Loader] Loading executable: {path}");
        
        // Convert MB to bytes for VirtualMemory constructor
        var memorySizeBytes = (ulong)reservedMemoryMb * 1024 * 1024;
        _vm = new VirtualMemory(memorySizeBytes, _logger);
        
        var configuredSizeMB = _vm.ConfiguredSize / (1024 * 1024);
        var addressSpaceSizeMB = _vm.Size / (1024 * 1024);
        _logger.LogInformation("[Memory] Configured size: {ConfiguredMB} MB, Address space: {AddressSpaceMB} MB (sparse, pages allocated on-demand)", 
            configuredSizeMB, addressSpaceSizeMB);
        
        // Store executable bytes for resource reading
        // If we loaded from VHD, we already have the bytes. Otherwise, read from file.
        _executableBytes = executableBytes ?? File.ReadAllBytes(path);
        
        // Detect executable format
        var format = PeImageLoader.DetectFormat(_executableBytes);
        _logger.LogInformation("[Loader] Detected format: {Format}", format);
        
        // Load executable based on format
        // Note: peLoader is null for NE format. Modules accept nullable PeImageLoader
        // since not all modules require PE-specific functionality (e.g., resource reading).
        PeImageLoader? peLoader = null; // For PE format only
        switch (format)
        {
            case ExecutableFormat.PE32:
            {
                peLoader = new PeImageLoader(_vm, _logger);
                _image = executableBytes != null
                    ? peLoader.LoadFromBytes(executableBytes)
                    : peLoader.Load(path);
                break;
            }
            
            case ExecutableFormat.NE:
            {
                var neLoader = new NeImageLoader(_vm, _logger);
                _image = neLoader.LoadFromBytes(_executableBytes, path);
                _logger.LogWarning("[Loader] Win16 NE format support is experimental. Some features may not work correctly.");
                // peLoader remains null for NE format - modules will use LoadedImage data instead
                break;
            }
            
            default:
                throw new NotSupportedException($"Unsupported executable format: {format}. Only PE32 (Win32) and NE (Win16) formats are supported.");
        }
        
        LogDebug($"[Loader] Image base=0x{_image.BaseAddress:X8} EntryPoint=0x{_image.EntryPointAddress:X8} Size=0x{_image.ImageSize:X}");
        LogDebug($"[Loader] Imports mapped: {_image.ImportAddressMap.Count}");
        LogDebug($"[Loader] Subsystem: {_image.Subsystem} (2=GUI, 3=CUI)");
        LogDebug($"[Loader] Stack: Reserve=0x{_image.SizeOfStackReserve:X} Commit=0x{_image.SizeOfStackCommit:X}");
        LogDebug($"[Loader] Heap: Reserve=0x{_image.SizeOfHeapReserve:X} Commit=0x{_image.SizeOfHeapCommit:X}");
        LogDebug($"[Loader] Sections loaded: {_image.Sections.Length}");
        
        // Log section information for debugging data/instruction ranges
        foreach (var section in _image.Sections)
        {
            var flags = new List<string>();
            if (section.IsExecutable)
            {
	            flags.Add("EXEC");
            }

            if (section.IsData)
            {
	            flags.Add("DATA");
            }

            if (section.IsReadable)
            {
	            flags.Add("READ");
            }

            if (section.IsWritable)
            {
	            flags.Add("WRITE");
            }

            LogDebug($"[Loader]   Section '{section.Name}': RVA=0x{section.VirtualAddress:X8} Size=0x{section.VirtualSize:X8} Flags=[{string.Join(",", flags)}]");
        }

        _env = new ProcessEnvironment(_vm, CalculateHeapBase(), _host, _logger, _backendFactory);
        
        // Initialize virtual file system - prioritize custom VFS, then disk path
        if (customVirtualFileSystem != null)
        {
            _logger.LogInformation("[Loader] Initializing virtual file system with custom instance");
            _env.InitializeVirtualFileSystem(customVirtualFileSystem);
            _logger.LogInformation("[Loader] Virtual file system initialized successfully (custom)");
        }
        else if (!string.IsNullOrEmpty(virtualDiskPath))
        {
            _logger.LogInformation("[Loader] Initializing virtual file system with disk: {DiskPath}", virtualDiskPath);
            _env.InitializeVirtualFileSystemWithDisk(virtualDiskPath);
            _logger.LogInformation("[Loader] Virtual file system initialized successfully (disk)");
        }
        else
        {
            _logger.LogWarning("[Loader] No virtual disk provided - VFS not initialized. File operations will fail.");
        }
        
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

        // Calculate stack bounds from PE header before creating CPU
        // This allows CPU validation to use actual stack bounds
        var stackReserve = _image.SizeOfStackReserve;
        if (stackReserve == 0)
        {
            stackReserve = 0x00100000; // 1MB default reserve if not specified
        }
        
        // Stack grows downward, so place it below the typical heap area
        // but above low memory. We'll use 0x00100000 + stackReserve as the stack top.
        var stackBase = 0x00100000u + stackReserve;
        var stackLimit = 0x00100000u; // Bottom of stack (lowest valid address)

        // Create CPU based on backend preference
        if (useJitCpu)
        {
            _cpu = new Cpu.Jit.JitCpu(_vm, _logger);
            LogDebug("[Loader] JIT CPU backend enabled (async-capable)");
        }
        else
        {
            _cpu = new IcedCpu(_vm, _logger, decoderOptions, enableInstructionAnalyzer, _image.BaseAddress, stackLimit, stackBase);
            if (enableInstructionAnalyzer)
            {
                LogDebug("[Loader] Instruction analyzer enabled");
            }
        }
        
        // Log the actual CPU backend being used (after initialization and potential fallback)
        var actualCpuBackend = _cpu switch
        {
            Cpu.Jit.JitCpu => "JitCpu",
            IcedCpu => "IcedCpu",
            _ => "Unknown"
        };
        _logger.LogInformation("[Loader] Selected CPU Emulator: {CpuBackend}", actualCpuBackend);
        
        _cpu.SetEip(_image.EntryPointAddress);
        
        // Use the stack bounds calculated earlier (before CPU creation)
        _stackBase = stackBase;
        _stackLimit = stackLimit;
        
        // Initialize stack using PE-provided SizeOfStackCommit
        var commitSize = _image.SizeOfStackCommit;
        if (commitSize == 0)
        {
            commitSize = 0x8000; // 32KB default commit if not specified
        }
        if (commitSize > stackReserve)
        {
            // Commit size cannot exceed reserve size
            commitSize = stackReserve;
        }
        
        var initialEsp = _stackBase - commitSize;
        _cpu.SetRegister("ESP", initialEsp);
        _cpu.SetRegister("EBP", initialEsp); // Initialize frame pointer to match stack pointer
        
        // Store heap base for use in checks
        _heapBase = CalculateHeapBase();
        
        // Store memory layout in ProcessEnvironment for use by Win32 modules
        _env.StackBase = _stackBase;
        _env.StackLimit = _stackLimit;
        
        _logger.LogInformation("[Loader] Stack initialized: Base=0x{StackBase:X8} Limit=0x{StackLimit:X8} ESP=0x{ESP:X8} Reserve=0x{Reserve:X} Commit=0x{Commit:X}",
            _stackBase, _stackLimit, initialEsp, stackReserve, commitSize);
        _logger.LogInformation("[Loader] Heap base: 0x{HeapBase:X8}", _heapBase);

        _dispatcher = new Win32Dispatcher(_logger);
        _dispatcher.SetProcessEnvironment(_env);

        var kernel32Module = new Kernel32Module(_env, _image.BaseAddress, peLoader, _logger);
        kernel32Module.SetDispatcher(_dispatcher);
        
        // Create resource reader for PE resources (dialogs, icons, etc.)
        // Use stored bytes instead of path, as path is a Windows path inside the VHD (e.g., C:\ign_teas\IGN_TEAS.EXE)
        // which doesn't exist on the host file system
        var peImage = AsmResolver.PE.PEImage.FromBytes(_executableBytes!);
        var resourceReader = new PeResourceReader(peImage, _image.BaseAddress, _vm, _logger);
        kernel32Module.SetResourceReader(resourceReader);
        
        _dispatcher.RegisterModule(kernel32Module);
        // Register KERNELBASE for forwarded exports from KERNEL32
        _dispatcher.RegisterModule(new KernelBaseModule(_env, _image.BaseAddress, peLoader, _logger));

        _dispatcher.RegisterModule(new Advapi32Module(_env, _image.BaseAddress, peLoader, _logger));
        
        var user32Module = new User32Module(_env, _image.BaseAddress, peLoader, _logger);
        user32Module.SetDispatcher(_dispatcher);
        user32Module.SetLoadedImage(_image);
        user32Module.SetResourceReader(resourceReader); // Set resource reader for dialog loading
        user32Module.SetHost(_host); // Set host for dialog UI callbacks
        _dispatcher.RegisterModule(user32Module);
        
        _dispatcher.RegisterModule(new Gdi32Module(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new Comdlg32Module(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new DDrawModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new DSoundModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new DInputModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new WinMmModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new Msacm32Module(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new Glide2XModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new RedlineModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new VeriteModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new DPlayXModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new Ole32Module(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new Oleaut32Module(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new Shell32Module(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new DsetupModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new MsvcrtModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new Wsock32Module(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new Wavmix32Module(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new Comctl32Module(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new DInput8Module(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new VersionModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new Lz32Module(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new WinspoolModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new OledlgModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new Olepro32Module(_env, _image.BaseAddress, peLoader, _logger));
        
        // Additional system DLLs
        _dispatcher.RegisterModule(new NtdllModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new ShlwapiModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new WininetModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new UcrtbaseModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new Vcruntime140Module(_env, _image.BaseAddress, peLoader, _logger));

        // Initialize the main thread in the thread scheduler
        _env.InitializeMainThread(_cpu);
        LogDebug("[Loader] Main thread initialized");

        // Execute TLS callbacks if present
        // TLS callbacks must be executed AFTER all modules are registered but BEFORE the main entry point
        ExecuteTlsCallbacks();
        
        // Apply executable-specific workarounds after TLS but before main entry point
        ApplyExecutableWorkarounds();
    }

    /// <summary>
    /// Executes TLS (Thread Local Storage) callbacks for process attach.
    /// TLS callbacks are invoked before the main entry point with DLL_PROCESS_ATTACH reason.
    /// </summary>
    private void ExecuteTlsCallbacks()
    {
        if (_image == null || _cpu == null || _vm == null || _env == null)
        {
            return;
        }

        if (_image.TlsCallbacks == null || _image.TlsCallbacks.Length == 0)
        {
            _logger.LogDebug("[Emulator] No TLS callbacks to execute");
            return;
        }

        _logger.LogInformation("[Emulator] Executing {Count} TLS callbacks", _image.TlsCallbacks.Length);

        // TLS callback signature: void NTAPI TlsCallback(PVOID DllHandle, DWORD Reason, PVOID Reserved)
        // Parameters:
        //   DllHandle: Base address of the image (hModule)
        //   Reason: DLL_PROCESS_ATTACH (1) for process initialization
        //   Reserved: NULL (0)
        const uint DLL_PROCESS_ATTACH = 1;

        var originalEip = _cpu.GetEip();
        var originalEsp = _cpu.GetRegister("ESP");

        for (var i = 0; i < _image.TlsCallbacks.Length; i++)
        {
            var callbackAddress = _image.TlsCallbacks[i];
            _logger.LogInformation("[Emulator] Executing TLS callback #{Index} at 0x{Address:X8}", i, callbackAddress);

            try
            {
                // Set up the stack for the callback call (stdcall convention)
                // TLS callback signature: void NTAPI TlsCallback(PVOID DllHandle, DWORD Reason, PVOID Reserved)
                // Parameters pushed right-to-left: Reserved, Reason, DllHandle
                // Each callback starts with a fresh stack from originalEsp
                var esp = originalEsp;
                
                // Push Reserved (last parameter, NULL)
                esp -= 4;
                _vm.Write32(esp, 0);
                
                // Push Reason (second parameter, DLL_PROCESS_ATTACH)
                esp -= 4;
                _vm.Write32(esp, DLL_PROCESS_ATTACH);
                
                // Push DllHandle (first parameter, image base address)
                esp -= 4;
                _vm.Write32(esp, _image.BaseAddress);
                
                // Push return address (we'll use a special marker to detect return)
                // Use an address in the NULL page that will cause a fault if executed
                const uint RETURN_MARKER = 0x00000001;
                esp -= 4;
                _vm.Write32(esp, RETURN_MARKER);
                
                // Update ESP and set EIP to callback
                _cpu.SetRegister("ESP", esp);
                _cpu.SetEip(callbackAddress);
                
                // Execute the callback until it returns
                // We'll detect return when EIP reaches our RETURN_MARKER
                // Note: Unlike the main emulation loop, TLS callbacks run to completion without instruction limits
                // to match Windows behavior. If a callback never returns, the emulator will hang.
                while (_cpu.GetEip() != RETURN_MARKER)
                {
                    _cpu.SingleStep(_vm);
                }
                
                _logger.LogDebug("[Emulator] TLS callback #{Index} returned successfully", i);
            }
            catch (Exception ex)
            {
                // Re-throw critical exceptions that should not be caught
                if (ex is OutOfMemoryException || ex is StackOverflowException)
                {
                    throw;
                }
                
                // For other exceptions, restore CPU state and abort TLS callbacks execution
                // since the process state is now undefined
                _logger.LogCritical(ex, "[Emulator] Error executing TLS callback #{Index} at 0x{Address:X8}", 
                    i, callbackAddress);
                _cpu.SetEip(originalEip);
                _cpu.SetRegister("ESP", originalEsp);
                _logger.LogCritical("[Emulator] Aborting TLS callbacks execution due to exception in callback #{Index}", i);
                break;
            }
        }

        // Restore original EIP and ESP
        _cpu.SetEip(originalEip);
        _cpu.SetRegister("ESP", originalEsp);
        
        _logger.LogInformation("[Emulator] TLS callbacks execution complete");
    }

    /// <summary>
    /// Applies executable-specific workarounds for known bugs in legacy applications.
    /// These patches address issues like stack misalignment, incorrect function epilogues,
    /// or other compatibility problems that prevent the application from running correctly.
    /// </summary>
    private void ApplyExecutableWorkarounds()
    {
        if (_image == null || _vm == null || _env == null)
        {
            return;
        }

        // Try to get executable name from multiple sources
        var exeNameFromImage = Path.GetFileName(_image.FilePath ?? "").ToUpperInvariant();
        var exeNameFromEnv = Path.GetFileName(_env.ExecutablePath ?? "").ToUpperInvariant();
        
        _logger.LogInformation("[Emulator] Checking for executable-specific workarounds");
        _logger.LogDebug("[Emulator] Image FilePath: {ImagePath}", _image.FilePath);
        _logger.LogDebug("[Emulator] Env ExecutablePath: {EnvPath}", _env.ExecutablePath);
        _logger.LogDebug("[Emulator] Extracted names: {ImageName} / {EnvName}", exeNameFromImage, exeNameFromEnv);
        
        // BasicDD.exe: Fix stack misalignment in FUN_00401310
        // The function's epilogue does ADD ESP,0x8C but should do ADD ESP,0x94 due to
        // CRT stack management bug where 5 parameters are pushed to WinMain but only
        // 4 are cleaned up, accumulating an 8-byte offset.
        if (exeNameFromImage == "BASICDD.EXE" || exeNameFromEnv == "BASICDD.EXE" || 
            exeNameFromImage.Contains("BASICDD") || exeNameFromEnv.Contains("BASICDD"))
        {
            // Validate patch address is within image bounds
            if (BASICDD_EPILOGUE_PATCH_ADDRESS < _image.BaseAddress || 
                BASICDD_EPILOGUE_PATCH_ADDRESS >= _image.BaseAddress + _image.ImageSize)
            {
                _logger.LogWarning("[Emulator] BasicDD.exe detected but patch address 0x{Address:X8} is outside image bounds (0x{Base:X8}-0x{Limit:X8})", 
                    BASICDD_EPILOGUE_PATCH_ADDRESS, _image.BaseAddress, _image.BaseAddress + _image.ImageSize);
                return;
            }
            
            // Patch the epilogue at 0x00401412 to change ADD ESP,0x8C to ADD ESP,0x94
            // Original bytes: 81 C4 8C 00 00 00 (ADD ESP, 0x8C)
            // Patched bytes:  81 C4 94 00 00 00 (ADD ESP, 0x94)
            var originalByte = _vm.Read8(BASICDD_EPILOGUE_PATCH_ADDRESS);
            
            if (originalByte == BASICDD_ORIGINAL_STACK_ADJUSTMENT)
            {
                _vm.Write8(BASICDD_EPILOGUE_PATCH_ADDRESS, BASICDD_CORRECTED_STACK_ADJUSTMENT);
                _logger.LogWarning("[Emulator] Applied BasicDD.exe workaround: Patched function epilogue at 0x{Address:X8} (0x{Original:X2} -> 0x{Corrected:X2})", 
                    BASICDD_EPILOGUE_PATCH_ADDRESS, BASICDD_ORIGINAL_STACK_ADJUSTMENT, BASICDD_CORRECTED_STACK_ADJUSTMENT);
                _logger.LogWarning("[Emulator] This fixes an 8-byte stack misalignment caused by CRT startup bug");
                
                // Configure tracing trigger points for this executable
                _traceTriggerStartAddress = 0x00401329u;  // Return from DirectDrawCreateEx
                _traceTriggerEndAddress = 0x0040132Bu;
                _traceTriggerDll = "DDRAW.DLL";
                _traceTriggerFunction = "DirectDrawCreateEx";
            }
            else
            {
                _logger.LogWarning("[Emulator] BasicDD.exe detected but patch not applied - byte at 0x{Address:X8} is 0x{Byte:X2} (expected 0x{Expected:X2})", 
                    BASICDD_EPILOGUE_PATCH_ADDRESS, originalByte, BASICDD_ORIGINAL_STACK_ADJUSTMENT);
            }
        }
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
        catch (Exception ex) when (ex is not OutOfMemoryException && ex is not StackOverflowException && ex is not System.Threading.ThreadAbortException)
        {
            // Log the unhandled exception
            _logger.LogError(ex, "[Emulator] Unhandled exception during emulation");
            
            // Store exception for later reporting (GUI will handle display)
            _lastException = ex;
        }
        finally
        {
            // Stop event processing thread
            StopEventProcessing();
            
            // Always print exit message and summary, even if there was an exception
            string exitMessage;
            if (_lastException != null)
            {
                exitMessage = $"[Exit] Emulation terminated due to unhandled exception: {_lastException.GetType().Name}";
                LogDebug(exitMessage);
                
                // Log additional exception details for debugging
                LogDebug($"[Exit] Exception message: {_lastException.Message}");
                
                var inner = _lastException.InnerException;
                var level = 1;
                while (inner != null)
                {
                    LogDebug($"[Exit] Inner exception ({level}): {inner.GetType().Name}: {inner.Message}");
                    if (inner.StackTrace != null)
                    {
                        LogDebug("[Exit] Inner exception stack trace:");
                        foreach (var trimmedLine in inner.StackTrace
                            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                            .Select(line => line.Trim())
                            .Where(trimmedLine => !string.IsNullOrEmpty(trimmedLine)))
                        {
                            LogDebug($"[Exit]   {trimmedLine}");
                        }
                    }
                    inner = inner.InnerException;
                    level++;
                }
                
                // Log stack trace to help identify the source of the exception
                if (_lastException.StackTrace != null)
                {
                    LogDebug("[Exit] Stack trace:");
                    // Split on both \r and \n to handle different line endings across platforms
                    foreach (var trimmedLine in _lastException.StackTrace
                        .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                        .Select(line => line.Trim())
                        .Where(trimmedLine => !string.IsNullOrEmpty(trimmedLine)))
                    {
                        LogDebug($"[Exit]   {trimmedLine}");
                    }
                }
            }
            else if (_stopRequested)
            {
                exitMessage = "[Exit] Stop requested by user.";
                LogDebug(exitMessage);
            }
            else if (_env.ExitRequested)
            {
                exitMessage = "[Exit] Process requested exit.";
                LogDebug(exitMessage);
            }
            else
            {
                exitMessage = "[Exit] Execution completed.";
                LogDebug(exitMessage);
            }
            
            LogDebug("=== Unknown Function Summary ===");
            _dispatcher.PrintUnknownFunctionsSummary();
        }
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
        // Stop emulation after N iterations at same EIP
        // WASM uses a much lower threshold (500K) to prevent browser freeze
        // Native uses 50M to allow legitimate tight loops (memory initialization, large data processing)
        // Note: 307K iterations are needed for typical screen buffer initialization (640x480)
        var maxSameEipIterations = PlatformHelpers.IsWasm 
            ? 500000ul   // WASM: 500K iterations (~0.5-5 seconds) - prevents browser freeze
            : 50000000ul; // Native: 50M iterations - allows complex initialization
        
        // Secondary infinite loop detection - track iterations since last syscall
        // This catches loops that cycle through multiple instructions but never call Win32 APIs
        // WASM uses a lower threshold (1M) to keep the browser responsive
        // Native uses 100M to allow complex initialization routines (lookup tables, data structures)
        var iterationsSinceLastSyscall = 0ul;
        var maxIterationsWithoutSyscall = PlatformHelpers.IsWasm
            ? 1000000ul    // WASM: 1M instructions (~1-10 seconds) - prevents browser freeze
            : 100000000ul; // Native: 100M instructions - allows complex initialization
        
        // Throttle noisy warning logs to reduce spam
        var lastSuspiciousEipWarning = 0u;
        var lastHeapEipWarning = 0u;

        // Run indefinitely until stop/exit requested or no threads running
        while (!_stopRequested && !_env!.ExitRequested)
        {
            iterationCount++;
            iterationsSinceLastSyscall++;
            
            // Infinite loop detection - check every PROGRESS_LOG_INTERVAL iterations
            // This check runs regardless of log level since it affects emulation behavior
            if (iterationCount % PROGRESS_LOG_INTERVAL == 0)
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
                    if (sameEipCount >= maxSameEipIterations)
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
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("[Emulator] Progress: {Iterations} iterations ({Elapsed:F2}ms), EIP=0x{Eip:X8}, ESP=0x{Esp:X8}", 
                            iterationCount, elapsed, progressEip, progressEsp);
                    }
                }
                
                // Check if we've been running too long without making a Win32 API call
                // This catches infinite loops that cycle through multiple instructions
                if (iterationsSinceLastSyscall >= maxIterationsWithoutSyscall)
                {
                    _logger.LogError("[Emulator] INFINITE LOOP DETECTED: {Iterations} iterations without a syscall. EIP=0x{Eip:X8}, ESP=0x{Esp:X8}. Stopping emulation.", 
                        iterationsSinceLastSyscall, progressEip, progressEsp);
                    break;
                }
                
                lastProgressEip = progressEip;
                lastLogTime = now;
            }
            
            // DEBUG: Log EIP at start of each iteration to catch when it gets corrupted
            var eipAtLoopStart = _cpu!.GetEip();
            // Check if EIP is in heap area (likely executing data)
            // Exclude special emulator infrastructure ranges (syscall dispatcher, import stubs, COM vtables)
            // Throttle: Only log this warning when EIP changes to reduce log noise
            var isInHeapRange = eipAtLoopStart >= _heapBase && eipAtLoopStart < HEAP_LIMIT;
            var isInSpecialRange = MemoryRegions.IsInSpecialRange(eipAtLoopStart);
            if (isInHeapRange && !isInSpecialRange && eipAtLoopStart != lastSuspiciousEipWarning)
            {
                var esp = _cpu.GetRegister("ESP");
                _logger.LogWarning("[Emulator] LOOP START: EIP=0x{Eip:X8} is already in suspicious range at loop start! ESP=0x{Esp:X8}", eipAtLoopStart, esp);
                lastSuspiciousEipWarning = eipAtLoopStart;
            }
            
            // Check pause state periodically without blocking
            if (!_pauseEvent.WaitOne(0))
            {
                // Paused - yield and check again
                await Task.Delay(100);
                continue;
            }
            
            // WASM: Yield to browser event loop periodically to prevent freezing
            // In WebAssembly, Task.Run doesn't create real threads, so we must yield
            // control back to the JavaScript event loop to keep the UI responsive.
            // Note: PlatformHelpers.IsWasm is a static readonly field that JIT can constant-fold.
            // On non-WASM platforms, the && short-circuits and the modulo is never evaluated.
            if (PlatformHelpers.IsWasm && iterationCount % WASM_YIELD_INTERVAL == 0)
            {
                await Task.Yield();
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
            // DISABLED: This was causing false positives when EBP holds import stub addresses for indirect calls
            // Example: mov ebp,[IAT_entry]; call ebp - EBP legitimately contains 0x0F000060 (import stub)
            // ValidateAndFixEbp() would reset this to ESP, breaking the indirect call
            // EBP validation is already handled properly in CpuHelpers.RestoreCalleeSavedRegisters after syscalls
            // ValidateAndFixEbp();

            // Check if EIP is in the import stub range but not properly mapped
            // This can happen if code returns to or jumps to an unmapped import address
            var currentEip = _cpu!.GetEip();
            if (MemoryRegions.IsInImportHookRange(currentEip))
            {
                // EIP is in the import stub address range
                // Import stubs are aligned to 16-byte boundaries (0x10)
                // We need to align down to check if this is a valid stub
                var alignedEip = currentEip & MemoryRegions.ImportStubAlignmentMask;
                
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
            
            // Guard: detect execution in heap memory (likely executing data)
            // Exclude special emulator infrastructure ranges (syscall dispatcher, import stubs, COM vtables)
            // Throttle: Only log this warning when EIP changes to reduce log noise
            var isExecutingInHeapRange = eipBeforeStep >= _heapBase && eipBeforeStep < HEAP_LIMIT;
            var isExecutingInSpecialRange = MemoryRegions.IsInSpecialRange(eipBeforeStep);
            if (isExecutingInHeapRange && !isExecutingInSpecialRange && eipBeforeStep != lastHeapEipWarning)
            {
                // EIP in heap range is suspicious - likely executing data or unmapped memory
                // This range is typically used for data segments, not code
                _logger.LogWarning("[Emulator] EIP=0x{Eip:X8} is in heap memory range (0x{HeapBase:X8}-0x{HeapLimit:X8}). This may indicate a bad jump or return address. Attempting to verify memory is mapped...", 
                    eipBeforeStep, _heapBase, HEAP_LIMIT - 1);
                lastHeapEipWarning = eipBeforeStep;
                
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
            
            // Instruction-level tracing for debugging
            if (_instructionTraceCount > 0)
            {
                _instructionTraceCount--;
                var eipAfter = _cpu.GetEip();
                var esp = _cpu.GetRegister("ESP");
                var ebp = _cpu.GetRegister("EBP");
                var eax = _cpu.GetRegister("EAX");
                var ebx = _cpu.GetRegister("EBX");
                var ecx = _cpu.GetRegister("ECX");
                var edx = _cpu.GetRegister("EDX");
                var esi = _cpu.GetRegister("ESI");
                var edi = _cpu.GetRegister("EDI");
                
                // Try to read stack values (expanded to 36 DWORDs = 144 bytes)
                // This covers ESP+0x8C (140 bytes) where BasicDD.exe stack corruption occurs,
                // plus additional space to see the correct return address at ESP+0x94 (148 bytes)
                var stackVals = new List<string>();
                try
                {
                    for (int i = 0; i < 36; i++)
                    {
                        var addr = esp + (uint)(i * 4);
                        var val = _vm!.Read32(addr);
                        stackVals.Add($"[ESP+{i*4:X2}]=0x{val:X8}");
                    }
                }
                catch { }
                
                // Try to read instruction bytes at EIP before execution
                var instrBytes = "";
                try
                {
                    var bytes = new List<byte>();
                    for (int i = 0; i < 8; i++)
                    {
                        bytes.Add(_vm!.Read8(eipBeforeStep + (uint)i));
                    }
                    instrBytes = $"| Bytes: {BitConverter.ToString(bytes.ToArray()).Replace("-", " ")}";
                }
                catch { }
                
                // Check for EBP corruption - EBP should point to stack, not code/data
                // Use actual stack bounds for detection
                if (ebp < _stackLimit || ebp >= _stackBase)
                {
                    _logger.LogError("[TRACE] ⚠️ EBP CORRUPTION DETECTED! EBP=0x{Ebp:X8} is outside stack bounds (stack 0x{StackLimit:X8}-0x{StackBase:X8}). EIP: 0x{EipBefore:X8} -> 0x{EipAfter:X8}", 
                        ebp, _stackLimit, _stackBase, eipBeforeStep, eipAfter);
                }
                
                // Check for suspicious stack values (import stub addresses with wrong offsets)
                try
                {
                    var topOfStack = _vm!.Read32(esp);
                    if (topOfStack >= MemoryRegions.ImportHookBase && topOfStack < MemoryRegions.ImportHookLimit)
                    {
                        // This is in the import stub range
                        var alignedAddr = topOfStack & MemoryRegions.ImportStubAlignmentMask;
                        var offset = topOfStack - alignedAddr;
                        if (offset != 0 && offset != MemoryRegions.ImportStubSize) // Valid import stub entry points are at 0x0 or after full stub (0x10)
                        {
                            _logger.LogError("[TRACE] ⚠️ STACK CORRUPTION! Top of stack [ESP]=0x{Val:X8} points into middle of import stub (offset 0x{Offset:X2}). EIP: 0x{EipBefore:X8}", 
                                topOfStack, offset, eipBeforeStep);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "[TRACE] Exception during stack diagnostic check at EIP=0x{EipBefore:X8}", eipBeforeStep);
                }
                
                _logger.LogWarning("[TRACE] EIP: 0x{EipBefore:X8} -> 0x{EipAfter:X8} | ESP=0x{Esp:X8} EBP=0x{Ebp:X8} | EAX=0x{Eax:X8} EBX=0x{Ebx:X8} ECX=0x{Ecx:X8} EDX=0x{Edx:X8} ESI=0x{Esi:X8} EDI=0x{Edi:X8} | Stack: {Stack} {InstrBytes} | Remaining: {Count}",
                    eipBeforeStep, eipAfter, esp, ebp, eax, ebx, ecx, edx, esi, edi, string.Join(" ", stackVals), instrBytes, _instructionTraceCount);
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
                // Use async syscall handler to support async Win32 API implementations
                // This is required for WASM where blocking operations are not supported
                await HandleSyscallAsync().ConfigureAwait(false);
                iterationsSinceLastSyscall = 0; // Reset counter on syscall
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
                // Logging now handled by ComVtableDispatcher.TryInvoke
                
                var espBefore = _cpu.GetRegister("ESP");
                var eipBefore = _cpu.GetEip();
                
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
                    "COM vtable",
                    _image);
                
                var espAfter = _cpu.GetRegister("ESP");
                var eipAfter = _cpu.GetEip();
                _logger.LogInformation("[COM] After vtable call: ESP changed from 0x{EspBefore:X8} to 0x{EspAfter:X8} (delta={Delta}), Call site EIP=0x{EipBefore:X8}, Return EIP=0x{EipAfter:X8}", 
                    espBefore, espAfter, (int)espAfter - (int)espBefore, eipBefore, eipAfter);
                
                // Enable instruction tracing based on configured trigger points
                if (!_traceEnabled && _traceTriggerStartAddress.HasValue && _traceTriggerEndAddress.HasValue &&
                    eipAfter >= _traceTriggerStartAddress.Value && eipAfter <= _traceTriggerEndAddress.Value)
                {
                    _logger.LogWarning("[TRACE] Trigger address 0x{EipAfter:X8} reached, enabling instruction tracing for next {Count} instructions", eipAfter, MAX_TRACE_INSTRUCTIONS);
                    _instructionTraceCount = MAX_TRACE_INSTRUCTIONS;
                    _traceEnabled = true;
                }
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
            // In WASM, WaitOne with timeout may not be supported, so we use non-blocking check
            if (PlatformHelpers.IsWasm)
            {
                // Non-blocking check for WASM - use WaitOne(0) which is always supported
                if (!_pauseEvent.WaitOne(0))
                {
                    // If paused, add a small delay to prevent busy-waiting
                    PlatformHelpers.Sleep(1);
                    continue;
                }
            }
            else
            {
                _pauseEvent.WaitOne(100);
            }

            if (_stopRequested)
            {
	            break;
            }

            var currentEip = _cpu!.GetEip();

            // Check for extremely low EIP values that indicate corruption
            // Skip NULL (0) as that's handled separately
            // Exclude valid synthetic address ranges (COM vtables, syscalls, imports)
            var isValidSyntheticRange = MemoryRegions.IsInSpecialRange(currentEip);
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

            if (MemoryRegions.IsInImportHookRange(currentEip))
            {
                LogDebug("\n[Debug] *** CPU TRYING TO EXECUTE SYNTHETIC IMPORT ADDRESS! ***");
                LogDebug($"[Debug] EIP=0x{currentEip:X8} at instruction {i}");

                // Get the current main executable (may have been updated with synthetic exports)
                var currentImage = _env!.GetMainExecutable() ?? _image!;
                
                // Import stubs are aligned to 16-byte boundaries (0x10)
                // We need to align down to check if this is a valid stub
                var alignedEip = currentEip & MemoryRegions.ImportStubAlignmentMask;
                
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
                if (step.IsCall && MemoryRegions.IsInImportHookRange(step.CallTarget))
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
                    // Logging now handled by ComVtableDispatcher.TryInvoke
                    
                    // Use consolidated helper for register preservation and stdcall convention
                    CpuHelpers.InvokeWithRegisterPreservation(
                        _cpu,
                        _vm!,
                        () => {
                            var success = _env.ComDispatcher.TryInvoke(step.CallTarget, _cpu, _vm!, out var returnValue, out var argBytes);
                            // Return logging now handled by ComVtableDispatcher.TryInvoke
                            return (success, returnValue, argBytes);
                        },
                        _vm!.Size,
                        _logger,
                        "COM vtable",
                        _image);
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
                            $"Import {dll}!{name}",
                            _image);
                        
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
                else if (step.IsCall && MemoryRegions.IsInImportHookRange(step.CallTarget))
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

                if (MemoryRegions.IsInImportHookRange(currentEip))
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
                // Logging now handled by ComVtableDispatcher.TryInvoke
                
                // Save callee-saved registers (EBX, ESI, EDI, EBP) per x86 calling convention
                var savedEbx = _cpu.GetRegister("EBX");
                var savedEsi = _cpu.GetRegister("ESI");
                var savedEdi = _cpu.GetRegister("EDI");
                var savedEbp = _cpu.GetRegister("EBP");
                
                if (_env.ComDispatcher.TryInvoke(step.CallTarget, _cpu, _vm!, out var ret, out var comArgBytes))
                {
                    // Return logging now handled by ComVtableDispatcher.TryInvoke
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
                    // Logging now handled by ComVtableDispatcher.TryInvoke
                    
                    // Use consolidated helper for register preservation and stdcall convention
                    CpuHelpers.InvokeWithRegisterPreservation(
                        _cpu,
                        _vm!,
                        () => {
                            var success = _env.ComDispatcher.TryInvoke(step.CallTarget, _cpu, _vm!, out var returnValue, out var argBytes);
                            // Return logging now handled by ComVtableDispatcher.TryInvoke
                            return (success, returnValue, argBytes);
                        },
                        _vm!.Size,
                        _logger,
                        "COM vtable (GDB)",
                        _image);
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
                            $"Import {dll}!{name} (GDB)",
                            _image);
                        
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
            if (opcode == 0xCC && MemoryRegions.IsInSpecialRange(eip))
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
    /// Checks if an address is in the import stub range.
    /// Import stubs now use CALL/RET and syscall mechanism, so they should not be
    /// intercepted as direct import calls.
    /// </summary>
    private static bool IsImportStubAddress(uint address)
    {
        return MemoryRegions.IsInImportHookRange(address);
    }

    /// <summary>
    /// Handles direct calls to import stub addresses.
    /// Returns true if the call was handled and execution should continue to next instruction.
    /// </summary>
    private bool HandleDirectImportCall(uint callTarget)
    {
        var currentImage = _env!.GetMainExecutable() ?? _image!;
        
        // Align the call target to 16-byte boundary (import stubs are aligned)
        var alignedTarget = callTarget & MemoryRegions.ImportStubAlignmentMask;
        
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
                $"Import {dll}!{name}",
                _image);
            
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
    /// <summary>
    /// Async version of HandleSyscall that supports async Win32 API implementations.
    /// This is required for WASM where blocking operations (like Task.Wait()) are not supported.
    /// On WASM, this enables proper async execution of APIs like UpdateWindow that need to call
    /// back into emulated code (e.g., window procedures).
    /// </summary>
    private async Task<bool> HandleSyscallAsync(CancellationToken cancellationToken = default)
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
            
            // Use async dispatcher to support async Win32 API implementations (required for WASM)
            var (success, ret, argBytes, _) = await _dispatcher!.TryInvokeAsync(dll, name, _cpu, _vm!, cancellationToken).ConfigureAwait(false);
            if (success)
            {
                // DEBUG: Log stack contents after API call
                var returnToCallerAfter = _vm!.Read32(returnToCallerAddr);
                _logger.LogInformation("[Syscall] AFTER API: Return address at 0x{Addr:X8} = 0x{RetAddr:X8}", returnToCallerAddr, returnToCallerAfter);
                
                // Enable tracing based on configured trigger points
                if (!_traceEnabled && _traceTriggerDll != null && 
                    dll == _traceTriggerDll && name == _traceTriggerFunction)
                {
                    _logger.LogWarning("[TRACE] {Dll}!{Function} returned, enabling instruction tracing for next {Count} instructions", dll, name, MAX_TRACE_INSTRUCTIONS);
                    _instructionTraceCount = MAX_TRACE_INSTRUCTIONS;
                    _traceEnabled = true;
                }
                
                // VALIDATION: Detect stack corruption by checking if return address changed during API call
                if (returnToCaller != returnToCallerAfter)
                {
                    _logger.LogError("[Syscall] STACK CORRUPTION DETECTED: Return address changed from 0x{Before:X8} to 0x{After:X8} during {Dll}!{Name} call. This indicates the API corrupted the stack.", 
                        returnToCaller, returnToCallerAfter, dll, name);
                    
                    // Additional diagnostic: Check if the new return address is in unmapped import range
                    if (MemoryRegions.IsInImportHookRange(returnToCallerAfter))
                    {
                        var alignedAddr = returnToCallerAfter & MemoryRegions.ImportStubAlignmentMask;
                        var isMapped = currentImage.ImportAddressMap.ContainsKey(alignedAddr);
                        _logger.LogError("[Syscall] Corrupted return address 0x{Addr:X8} is in import stub range. Aligned: 0x{Aligned:X8}, Mapped: {Mapped}", 
                            returnToCallerAfter, alignedAddr, isMapped);
                        
                        if (!isMapped)
                        {
                            var importCount = currentImage.ImportAddressMap.Count;
                            // Calculate import index using aligned address to ensure correct calculation
                            var wouldBeIndex = (alignedAddr - MemoryRegions.ImportHookBase) / MemoryRegions.ImportStubSize;
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
                if (!MemoryRegions.IsInImportHookRange(retToStub))
                {
                    _logger.LogWarning("[Syscall] Return-to-stub address 0x{RetToStub:X8} is outside import stub range. This may indicate stack corruption.", retToStub);
                }
                
                // Validate ESP is in a reasonable range (not extremely small)
                var restoredEsp = _cpu.GetRegister("ESP");
                if (restoredEsp < 0x00010000)
                {
                    _logger.LogError("[Syscall] ESP=0x{Esp:X8} after syscall return is suspiciously low. This indicates possible stack corruption.", restoredEsp);
                }
                
                // Log CPU state after syscall for debugging
                var eax = _cpu.GetRegister("EAX");
                _logger.LogDebug("[Syscall] CPU state after {Dll}!{Name}: EAX=0x{Eax:X8} ESP=0x{Esp:X8}", dll, name, eax, restoredEsp);
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

    /// <summary>
    /// Synchronous wrapper for HandleSyscallAsync for use in non-WASM execution modes
    /// (enhanced debugging) where blocking is acceptable.
    /// </summary>
    /// <remarks>
    /// WARNING: This method uses GetAwaiter().GetResult() which can cause deadlocks
    /// in UI or ASP.NET contexts. It should ONLY be called from:
    /// - RunWithEnhancedDebugging (inherently synchronous, not used on WASM)
    /// On WASM, the async RunNormalAsync calls HandleSyscallAsync directly.
    /// </remarks>
    private bool HandleSyscall()
    {
        // Use GetAwaiter().GetResult() for sync contexts (non-WASM)
        // This is safe on desktop/server runtimes in these specific contexts where
        // there's no synchronization context that could cause deadlock
        return HandleSyscallAsync().GetAwaiter().GetResult();
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
            if (opcode == 0xCC && MemoryRegions.IsInSpecialRange(eip))
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
    
    /// <summary>
    /// Returns the default heap base address for memory allocation.
    /// The heap base is always set to 0x01000000 for compatibility.
    /// The PE header's SizeOfHeapReserve value is available in LoadedImage but not used
    /// to determine heap placement - it's available for the memory allocator to manage heap growth.
    /// </summary>
    /// <returns>The heap base address to use for memory allocation</returns>
    private static uint CalculateHeapBase()
    {
        const uint DEFAULT_HEAP_BASE = 0x01000000;
        return DEFAULT_HEAP_BASE;
    }
    
    // Constants for memory validation
    private const uint HEAP_LIMIT = 0x70000000;           // End of heap region (conservative upper limit)
    private const uint MIN_VALID_EBP = 0x1000;            // Minimum valid EBP (4KB)
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
            var isInSyntheticRange = MemoryRegions.IsInSpecialRange(retEip);
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
        
        // Use actual stack region from PE headers
        var stackBottom = _stackLimit;
        
        // Check if EBP is within reasonable stack range
        var ebpInStackRegion = (ebp >= stackBottom) && (ebp <= _stackBase + STACK_SLACK_BYTES);
        
        // Check if EBP is aligned (should be 4-byte aligned)
        var ebpAligned = (ebp & 0x3) == 0;
        
        // Check for obviously invalid values
        var ebpIsZero = (ebp == 0);
        var ebpIsVerySmall = (ebp < MIN_VALID_EBP);
        var ebpIsImportHook = MemoryRegions.IsInImportHookRange(ebp);
        var ebpIsBeyondMemory = (ebp >= _vm!.Size);
        
        // Check if EBP looks like a COM/heap pointer being used for special purposes
        var ebpIsHeapPointer = (ebp >= _heapBase && ebp < HEAP_LIMIT) && !ebpInStackRegion;
        
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
                // Only dispose if the task has completed to avoid "Task_Dispose_NotCompleted" error
                // This is especially important in WASM environments where Task.Run doesn't create
                // actual background threads and tasks may never complete in the traditional sense.
                // According to Microsoft guidance, disposing Tasks is rarely necessary - the GC will
                // clean up the resources eventually.
                if (_eventProcessingTask?.IsCompleted == true)
                {
                    _eventProcessingTask.Dispose();
                }
                _eventProcessingTask = null;
            }
        }
    }

    public void Dispose()
    {
        // Stop event processing if running
        StopEventProcessing();
        _pauseEvent.Dispose();
        
        // Cleanup process environment (saves registry, etc.)
        _env?.Cleanup();
    }
}