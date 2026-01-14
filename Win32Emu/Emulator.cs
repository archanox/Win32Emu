using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using Win32Emu.Cpu;
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
    
    // Event processing interval - process backend events and post synthetic messages
    // Set to 1000 iterations for responsive message handling
    // Lower than PROGRESS_LOG_INTERVAL to ensure apps waiting for messages don't block for long
    // Note: Based on instruction count, not real time - actual latency depends on CPU performance
    private const ulong EVENT_PROCESSING_INTERVAL = 1000;
    
    // x86 instruction opcodes for import stub patching
    private const byte RET_OPCODE = 0xC3;           // RET - return (no stack cleanup)
    private const byte RET_IMM16_OPCODE = 0xC2;     // RET imm16 - return with stack cleanup
    private const byte NOP_OPCODE = 0x90;           // NOP - no operation
    
    // Logging throttle interval when stuck at same EIP (reduce spam)
    // Log a warning every 1M iterations to avoid excessive log spam during legitimate tight loops
    private const ulong STUCK_EIP_LOG_INTERVAL = 1000000;
    
    // WASM yield interval - yield to browser event loop every N iterations
    // This prevents the browser from freezing when emulating tight loops.
    // Set to 10 for maximum responsiveness on WASM - yields every ~0.01-0.1ms on modern hardware.
    // Lower values improve UI responsiveness but reduce emulation throughput slightly.
    // Reduced from 100 to 10 to prevent browser tab freezing during DirectDraw initialization.
    private const ulong WASM_YIELD_INTERVAL = 10;
    
    // Emergency yield threshold - force yield if more than this many milliseconds pass without yielding
    // This is a safety net to prevent browser freezes in pathological cases
    private const int EMERGENCY_YIELD_THRESHOLD_MS = 100;
    
    // Infinite loop detection thresholds - WASM uses lower values to prevent browser freeze
    // These thresholds are selected based on runtime cost and expected legitimate workloads:
    // - WASM needs to remain responsive to the browser event loop
    // - Native can tolerate longer delays since emulation runs on a dedicated thread
    
    // Max iterations at same EIP before treating as stuck
    // 307K iterations are needed for typical screen buffer initialization (640x480)
    // Set to 500K to avoid false positives for legitimate screen buffer operations in WASM
    private const ulong MAX_SAME_EIP_ITERATIONS_WASM = 500000;     // WASM: 500K (~0.5-5 seconds)
    private const ulong MAX_SAME_EIP_ITERATIONS_NATIVE = 50000000; // Native: 50M iterations
    
    // Max iterations without a syscall (Win32 API call) before treating as stuck
    // WASM: Increased to 5M to allow ign_teas texture loading loop to complete (needs ~260K+ iterations)
    // Native: Increased to 500M to allow setup.exe CharNextA path parsing loop to complete (needs ~100M+ iterations)
    // Games and installers may have long-running initialization loops that don't call Win32 APIs
    private const ulong MAX_ITERATIONS_WITHOUT_SYSCALL_WASM = 5000000;      // WASM: 5M (~5-50 seconds)
    private const ulong MAX_ITERATIONS_WITHOUT_SYSCALL_NATIVE = 500000000;  // Native: 500M instructions (~5-10 seconds on modern CPUs)
    
    // Max consecutive heap executions before stopping emulation
    // If we've been executing in heap memory for more than 10 iterations,
    // this is definitely wrong - stop execution to prevent infinite loops
    // (10 iterations at ~2 bytes each = executing about 20 bytes of heap memory)
    private const ulong MAX_CONSECUTIVE_HEAP_EXECUTIONS = 10;
    
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
    
    // DOS interrupt constants
    private const byte DOS_INTERRUPT = 0x21;
    private const byte SYSCALL_INTERRUPT = 0x80;
    private const byte DOS_SPACE_CHAR = 0x20;
    private const byte DOS_STRING_TERMINATOR = 0x24; // '$' character
    private const byte DOS_NO_INPUT = 0x00;
    private const byte DOS_INPUT_READY = 0xFF;
    private const byte DOS_DRIVE_C = 0x02; // 0=default, 1=A, 2=B, 3=C
    private const ushort DOS_VERSION_MAJOR = 6;
    private const ushort DOS_VERSION_MINOR = 22;
    private const ushort DOS_VERSION_6_22 = 0x1606; // 6.22 in DOS format (AL=minor, AH=major)
    private const ushort DOS_PARAGRAPH_SIZE = 16; // DOS memory paragraphs are 16 bytes
    private const ushort DOS_STDOUT_HANDLE = 1;
    private const ushort DOS_STDERR_HANDLE = 2;
    private const ushort DOS_DUMMY_FILE_HANDLE = 0x0005;
    private const ushort DOS_FILE_ATTR_ARCHIVE = 0x0020;
    private const uint DOS_ERROR_INVALID_FUNCTION = 0xFFFFFFFF;
    private const int MAX_DOS_STRING_LENGTH = 1024;

    private const int DOS_MAX_CURRENT_DIR_LENGTH = 63;
    
    // IGN_TEAS texture loop tracking
    private ulong _ignTeasLoopIterations = 0;
    private ulong _ignTeasLastLoopLogIteration = 0;
    private const ulong IGN_TEAS_LOOP_LOG_INTERVAL = 10000; // Log every 10K iterations in the problematic loop
    
    // IGN_TEAS CRT startup loop tracking
    private ulong _ignTeasCrtLoopIterations = 0;
    private ulong _ignTeasCrtLastLogIteration = 0;
    private const ulong IGN_TEAS_CRT_LOG_INTERVAL = 100; // Log every 100 iterations in CRT loop (lowered to capture shorter loops)
    private bool _ignTeasCrtStringLogged = false; // Only log string content once
    private int _ignTeasCrtLoopExitCount = 0; // Track how many times we've exited the CRT loop
    private ulong _ignTeasPostCrtExecutionCount = 0; // Track instructions executed after final CRT exit
    private uint _ignTeasLastPostCrtEipForLoop = 0; // Track last EIP after CRT for loop detection  
    private const ulong IGN_TEAS_POST_FINAL_CRT_LOG_INTERVAL = 10000; // Log every 10K instructions after final CRT
    
    // IGN_TEAS diagnostic constants - function addresses from Ghidra decompilation
    private const uint IGN_TEAS_MAIN_INIT_ADDR = 0x004023F0;          // Main initialization (calls texture loading, DirectDraw setup)
    private const uint IGN_TEAS_HEAP_INIT_ADDR = 0x00402540;          // Heap/memory initialization
    private const uint IGN_TEAS_TEXTURE_LOADING_ADDR = 0x004025D0;    // Texture loading (contains problematic loop)
    private const uint IGN_TEAS_GAME_LOGIC_ADDR = 0x00402410;         // Game logic update
    private const uint IGN_TEAS_CLEANUP_ADDR = 0x00402520;            // Cleanup function
    private const uint IGN_TEAS_DDRAW_INIT_ADDR = 0x004027D0;         // DirectDraw/rendering initialization
    private const uint IGN_TEAS_WINMAIN_ADDR = 0x00403140;            // WinMain entry point (message loop setup)
    private const uint IGN_TEAS_DDRAW_CREATE_ADDR = 0x00403510;       // DirectDraw creation
    private const uint IGN_TEAS_MAIN_TICK_ADDR = 0x004032A0;          // Main game tick
    
    // IGN_TEAS texture loop address range (problematic arithmetic loop)
    private const uint IGN_TEAS_TEXTURE_LOOP_START = 0x004027A2;
    private const uint IGN_TEAS_TEXTURE_LOOP_END = 0x004027B4;
    
    // IGN_TEAS CRT startup loop address ranges
    private const uint IGN_TEAS_CRT_ENTRY_1 = 0x00411060;             // CRT entry point 1
    private const uint IGN_TEAS_CRT_ENTRY_2 = 0x00412620;             // CRT entry point 2
    private const uint IGN_TEAS_CRT_LOOP_START = 0x00412422;          // CRT parsing loop start
    private const uint IGN_TEAS_CRT_LOOP_END = 0x00412676;            // CRT parsing loop end
    
    // IGN_TEAS game state memory addresses
    private const uint IGN_TEAS_GAME_STATE_ADDR = 0x0041c7a8;         // Game state: 0=init, 1=running, 2=cleanup
    private const uint IGN_TEAS_INIT_FLAG_ADDR = 0x0041c828;          // Initialization complete flag
    private const uint IGN_TEAS_EXIT_FLAG_ADDR = 0x0041c82c;          // Exit/cleanup flag
    
    // IGN_TEAS execution ranges for progress tracking
    private const uint IGN_TEAS_POST_CRT_START = 0x00413000;          // Post-CRT but pre-entry code start
    private const uint IGN_TEAS_POST_CRT_END = 0x00420000;            // Post-CRT code end
    private const uint IGN_TEAS_WINMAIN_RANGE_START = 0x00403000;     // WinMain area start
    private const uint IGN_TEAS_WINMAIN_RANGE_END = 0x00404000;       // WinMain area end
    private const uint IGN_TEAS_LIMBO_START = 0x00411000;             // Post-CRT "limbo" area start
    private const uint IGN_TEAS_LIMBO_END = 0x00413000;               // Post-CRT "limbo" area end
    
    // IGN_TEAS diagnostic thresholds
    private const ulong MIN_SIGNIFICANT_TEXTURE_LOOP_ITERATIONS = 100;     // Minimum iterations before logging texture loop
    private const ulong EXCESSIVE_TEXTURE_LOOP_THRESHOLD = 1000;           // Threshold indicating arithmetic bug in texture loop
    private const ulong CRT_LOOP_STUCK_THRESHOLD = 5000;                   // Threshold indicating CRT loop is stuck
    private const ulong CRT_LOOP_PARSING_BUG_THRESHOLD = 1000;             // Threshold indicating potential CRT parsing bug
    private const ulong IGN_TEAS_POST_CRT_LOG_INTERVAL = 1000;             // Log every N instructions in post-CRT range
    private const ulong IGN_TEAS_LIMBO_LOG_INTERVAL = 5000;                // Log every N instructions in "limbo" range
    
    // IGN_TEAS memory validation constants
    private const uint IGN_TEAS_VALID_MEMORY_START = 0x00400000;           // Valid memory range start
    private const uint IGN_TEAS_VALID_MEMORY_END = 0x00500000;             // Valid memory range end
    private const int IGN_TEAS_STRING_BUFFER_SIZE = 256;                   // String buffer size for diagnostics
    private const int IGN_TEAS_HEX_DUMP_MAX_LENGTH = 150;                  // Maximum hex dump string length
    private const int IGN_TEAS_ASCII_STRING_MAX_LENGTH = 100;              // Maximum ASCII string length
    private const byte ASCII_PRINTABLE_MIN = 32;                           // Minimum ASCII printable character
    private const byte ASCII_PRINTABLE_MAX = 127;                          // Maximum ASCII printable character (exclusive)
    
    // IGN_TEAS post-CRT progress tracking
    private ulong _ignTeasPostCrtInstructions = 0;
    private ulong _ignTeasLimboInstructions = 0;
    
    /// <summary>
    /// DOS INT 21h function numbers
    /// </summary>
    private enum DosFunction : byte
    {
        Terminate = 0x00,
        CharInputWithEcho = 0x01,
        CharOutput = 0x02,
        DirectConsoleIO = 0x06,
        DirectCharInputNoEcho = 0x07,
        CharInputNoEcho = 0x08,
        WriteString = 0x09,
        BufferedInput = 0x0A,
        CheckStdinStatus = 0x0B,
        GetCurrentDrive = 0x19,
        SetInterruptVector = 0x25,
        GetSystemDate = 0x2A,
        SetSystemDate = 0x2B,
        GetSystemTime = 0x2C,
        SetSystemTime = 0x2D,
        GetDosVersion = 0x30,
        GetSetCtrlBreak = 0x33,
        GetInterruptVector = 0x35,
        CreateFile = 0x3C,
        OpenFile = 0x3D,
        CloseFile = 0x3E,
        ReadFile = 0x3F,
        WriteFile = 0x40,
        SeekFile = 0x42,
        GetSetFileAttributes = 0x43,
        GetCurrentDirectory = 0x47,
        AllocateMemory = 0x48,
        FreeMemory = 0x49,
        ResizeMemory = 0x4A,
        TerminateWithReturnCode = 0x4C,
        GetReturnCode = 0x4D
    }
    
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
    /// Gets the current CPU backend instance (JitCpu with JIT/interpreter support)
    /// </summary>
    public IAsyncCpu? Cpu => _cpu;
    
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
    public void LoadExecutableFromBytes(byte[] executableBytes, string executableName, string[]? programArgs = null, bool debugMode = false, int reservedMemoryMb = 256, bool force32BitStackOps = true)
    {
        LoadExecutableFromBytes(executableBytes, executableName, programArgs, debugMode, reservedMemoryMb, virtualFileSystem: null, force32BitStackOps: force32BitStackOps);
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
    /// <param name="force32BitStackOps">Force 32-bit operand size for stack operations in 32-bit mode</param>
    /// <param name="forceInterpreterMode">Force interpreter mode even on native platforms (disables JIT compilation)</param>
    /// <param name="enableInstructionAnalyzer">Enable instruction analyzer for debugging (requires forceInterpreterMode in WASM)</param>
    /// <param name="enableLegacyInstructionDecoding">Enable legacy instruction decoding (MPX, Cyrix, ALTINST, etc.)</param>
    /// <param name="ansiCodePage">Default ANSI code page (CP_ACP). If null, uses UTF-8 (65001)</param>
    /// <param name="oemCodePage">Default OEM code page (CP_OEMCP). If null, uses OEM US (437)</param>
    public void LoadExecutableFromBytes(byte[] executableBytes, string executableName, string[]? programArgs, bool debugMode, int reservedMemoryMb, VirtualFileSystem.IVirtualFileSystem? virtualFileSystem, bool force32BitStackOps = true, bool forceInterpreterMode = false, bool enableInstructionAnalyzer = false, bool enableLegacyInstructionDecoding = false, uint? ansiCodePage = null, uint? oemCodePage = null)
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
            enableInstructionAnalyzer: enableInstructionAnalyzer, 
            enableLegacyInstructionDecoding: enableLegacyInstructionDecoding, 
            forceInterpreterMode: forceInterpreterMode,
            virtualDiskPath: null,
            preloadedBytes: executableBytes,
            customVirtualFileSystem: virtualFileSystem,
            force32BitStackOps: force32BitStackOps,
            ansiCodePage: ansiCodePage,
            oemCodePage: oemCodePage);
    }

    public void LoadExecutable(string path, string[]? programArgs = null, bool debugMode = false, bool interactiveDebugMode = false, int reservedMemoryMb = 256, bool gdbServerMode = false, int gdbServerPort = 1234, bool enableInstructionAnalyzer = false, bool enableLegacyInstructionDecoding = false, bool forceInterpreterMode = false, string? virtualDiskPath = null, byte[]? preloadedBytes = null, VirtualFileSystem.IVirtualFileSystem? customVirtualFileSystem = null, bool force32BitStackOps = true, uint? ansiCodePage = null, uint? oemCodePage = null)
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

        _env = new ProcessEnvironment(_vm, CalculateHeapBase(_image), _host, _logger, _backendFactory);
        
        // Set codepage configuration if specified
        if (ansiCodePage.HasValue)
        {
            _env.AnsiCodePage = (CodePage)ansiCodePage.Value;
            _logger.LogInformation("[Loader] ANSI code page set to: {CodePage} ({CodePageValue})", _env.AnsiCodePage, ansiCodePage.Value);
        }
        if (oemCodePage.HasValue)
        {
            _env.OemCodePage = (CodePage)oemCodePage.Value;
            _logger.LogInformation("[Loader] OEM code page set to: {CodePage} ({CodePageValue})", _env.OemCodePage, oemCodePage.Value);
        }
        
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

        // Create unified CPU backend (JitCpu with interpreter mode)
        // JitCpu uses JIT compilation when available (native platforms) and falls back to
        // interpreter mode in WASM or when forceInterpreterMode is true
        var jitCpu = new Cpu.Jit.JitCpu(_vm, _logger, decoderOptions, enableInstructionAnalyzer, _image.BaseAddress, stackLimit, stackBase, bitness: 32, force32BitStackOps: force32BitStackOps, forceInterpreterMode: forceInterpreterMode);
        _cpu = jitCpu;
        
        // Wire up ProcessEnvironment to JitCpu for transpiled function support
        jitCpu.SetProcessEnvironment(_env);
        
        // Load transpiled functions if available (for ign_win.exe / ign_teas)
        if (path.Contains("ign_win", StringComparison.OrdinalIgnoreCase) || path.Contains("ign_teas", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var transpiledDllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Generated", "IgNTeas", "bin", "Release", "net10.0", "IgNTeas.Generated.dll");
                if (File.Exists(transpiledDllPath))
                {
                    _env.TranspiledFunctionProvider?.LoadFromAssembly(transpiledDllPath);
                    _logger.LogInformation("[Loader] Loaded transpiled functions from {Path}", transpiledDllPath);
                }
                else
                {
                    _logger.LogDebug("[Loader] Transpiled functions not found at {Path} (this is optional)", transpiledDllPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Loader] Failed to load transpiled functions (non-fatal)");
            }
        }
        
        _logger.LogInformation("[Loader] Unified JitCpu backend enabled (JIT compilation: {JitEnabled}, Interpreter mode: {InterpreterEnabled})", 
            jitCpu.SupportsJit, !jitCpu.SupportsJit);
        
        // Initialize JIT cache for pre-compiled blocks (only when JIT is supported)
        if (jitCpu.SupportsJit)
        {
            jitCpu.SetExecutablePath(path);
            _logger.LogInformation("[Loader] JIT cache: Set executable path to {Path}", path);
            
            // Load existing cache using async API and wait for completion
            // We still wait synchronously here to ensure cache loading completes before execution
            try
            {
                Task.Run(() => jitCpu.LoadCacheAsync()).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Loader] Failed to load JIT cache (non-fatal)");
            }
        }
        
        if (enableInstructionAnalyzer)
        {
            LogDebug("[Loader] Instruction analyzer requested");
        }
        
        // Log the actual CPU backend being used (after initialization and potential fallback)
        var actualCpuBackend = _cpu?.GetType().Name ?? "None";
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
        
        // CRITICAL: Zero-initialize the ENTIRE stack reserve region for Win95 compatibility
        // Win95-era CRT expects stack memory to be zero-filled (similar to VirtualAlloc with MEM_COMMIT)
        // Without this, REPNZ SCASB instructions scanning for null terminators will read garbage data
        // and loop indefinitely (e.g., ign_teas CRT initialization at 0x004122CF)
        // NOTE: We zero the entire reserve (not just commit) because the stack grows during CRT init
        _logger.LogInformation("[Loader] Zero-initializing entire stack reserve region (0x{Start:X8} - 0x{End:X8}, {Size} bytes)",
            _stackLimit, _stackBase, stackReserve);
        for (uint addr = _stackLimit; addr < _stackBase; addr++)
        {
            _vm!.Write8(addr, 0);
        }
        
        // Store heap base for use in checks
        _heapBase = CalculateHeapBase(_image);
        
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
        
        // Create resource reader for resources (dialogs, icons, etc.)
        // Use stored bytes instead of path, as path is a Windows path inside the VHD (e.g., C:\ign_teas\IGN_TEAS.EXE)
        // which doesn't exist on the host file system
        IResourceReader? resourceReader = null;
        if (format == ExecutableFormat.PE32)
        {
            var peImage = AsmResolver.PE.PEImage.FromBytes(_executableBytes!);
            resourceReader = new PeResourceReader(peImage, _image.BaseAddress, _vm, _logger);
            kernel32Module.SetResourceReader(resourceReader);
        }
        else if (format == ExecutableFormat.NE)
        {
            // Use NE resource reader for Win16 executables
            resourceReader = new NeResourceReader(_executableBytes!, _vm, _logger);
            kernel32Module.SetResourceReader(resourceReader);
            _logger.LogInformation("[Loader] NE resource reader initialized for Win16 executable");
        }
        else
        {
            _logger.LogWarning("[Loader] Resource reader not available for {Format} format. Dialog and resource loading may not work.", format);
        }
        
        _dispatcher.RegisterModule(kernel32Module);
        // Register KERNELBASE for forwarded exports from KERNEL32
        _dispatcher.RegisterModule(new KernelBaseModule(_env, _image.BaseAddress, peLoader, _logger));

        _dispatcher.RegisterModule(new Advapi32Module(_env, _image.BaseAddress, peLoader, _logger));
        
        var user32Module = new User32Module(_env, _image.BaseAddress, peLoader, _logger);
        user32Module.SetDispatcher(_dispatcher);
        user32Module.SetLoadedImage(_image);
        if (resourceReader != null)
        {
            user32Module.SetResourceReader(resourceReader); // Set resource reader for dialog loading
        }
        user32Module.SetHost(_host); // Set host for dialog UI callbacks
        _dispatcher.RegisterModule(user32Module);
        
        var gdi32Module = new Gdi32Module(_env, _image.BaseAddress, peLoader, _logger, user32Module);
        _dispatcher.RegisterModule(gdi32Module);
        _dispatcher.RegisterModule(new Comdlg32Module(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new DDrawModule(_env, _image.BaseAddress, peLoader, _logger, gdi32Module));
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
        
        var msvcrtModule = new MsvcrtModule(_env, _image.BaseAddress, peLoader, _logger);
        msvcrtModule.SetDispatcher(_dispatcher);
        msvcrtModule.SetLoadedImage(_image);
        _dispatcher.RegisterModule(msvcrtModule);
        
        _dispatcher.RegisterModule(new Wsock32Module(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new Wavmix32Module(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new Comctl32Module(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new DInput8Module(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new VersionModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new Lz32Module(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new WinspoolModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new OledlgModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new Olepro32Module(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new Imm32Module(_env, _image.BaseAddress, peLoader, _logger));
        
        // Additional system DLLs
        _dispatcher.RegisterModule(new NtdllModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new ShlwapiModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new WininetModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new UcrtbaseModule(_env, _image.BaseAddress, peLoader, _logger));
        _dispatcher.RegisterModule(new Vcruntime140Module(_env, _image.BaseAddress, peLoader, _logger));

        // Register Win16 thunking modules for NE format executables
        if (format == ExecutableFormat.NE)
        {
            _logger.LogInformation("[Loader] Registering Win16 thunking modules for NE format executable");
            
            // Get the Win32 modules we need to wrap
            var kernel32 = _dispatcher.TryGetModule("KERNEL32.DLL", out var k32Module) ? k32Module! : throw new InvalidOperationException("KERNEL32.DLL not found");
            var user32 = _dispatcher.TryGetModule("USER32.DLL", out var u32Module) ? u32Module! : throw new InvalidOperationException("USER32.DLL not found");
            var gdi32 = _dispatcher.TryGetModule("GDI32.DLL", out var g32Module) ? g32Module! : throw new InvalidOperationException("GDI32.DLL not found");
            var winmm = _dispatcher.TryGetModule("WINMM.DLL", out var wmmModule) ? wmmModule! : throw new InvalidOperationException("WINMM.DLL not found");
            
            // Register Win16 thunking modules that wrap Win32 modules
            _dispatcher.RegisterModule(new Win32.Win16.Win16KernelModule(kernel32, _logger));
            _dispatcher.RegisterModule(new Win32.Win16.Win16UserModule(user32, _logger));
            _dispatcher.RegisterModule(new Win32.Win16.Win16GdiModule(gdi32, _logger));
            _dispatcher.RegisterModule(new Win32.Win16.Win16KeyboardModule(user32, _logger));
            _dispatcher.RegisterModule(new Win32.Win16.Win16SystemModule(kernel32, _logger));
            _dispatcher.RegisterModule(new Win32.Win16.Win16SoundModule(winmm, _logger));
            
            _logger.LogInformation("[Loader] Win16 thunking modules registered successfully");
        }

        // Initialize the main thread in the thread scheduler
        _env.InitializeMainThread(_cpu);
        LogDebug("[Loader] Main thread initialized");

        // Fix up data imports after all modules are registered
        // Some imports like _iob are data exports, not functions, and need special handling
        FixupDataImports();

        // Execute TLS callbacks if present
        // TLS callbacks must be executed AFTER all modules are registered but BEFORE the main entry point
        // Use synchronous wrapper for compatibility with synchronous LoadExecutable
        ExecuteTlsCallbacksAsync().GetAwaiter().GetResult();
        
        // Apply executable-specific workarounds after TLS but before main entry point
        ApplyExecutableWorkarounds();
    }

    /// <summary>
    /// Executes TLS (Thread Local Storage) callbacks for process attach.
    /// TLS callbacks are invoked before the main entry point with DLL_PROCESS_ATTACH reason.
    /// </summary>
    private async Task ExecuteTlsCallbacksAsync()
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
                
                // Execute the callback until it returns using async SingleStep
                // We'll detect return when EIP reaches our RETURN_MARKER
                // Note: Unlike the main emulation loop, TLS callbacks run to completion without instruction limits
                // to match Windows behavior. If a callback never returns, the emulator will hang.
                var stepCount = 0;
                while (_cpu.GetEip() != RETURN_MARKER)
                {
                    await _cpu.SingleStepAsync(_vm).ConfigureAwait(false);
                    
                    // Yield periodically on WASM to keep browser responsive
                    // Use Task.Delay(1) to actually return control to browser, not Task.Yield()
                    if (PlatformHelpers.IsWasm && ++stepCount % (int)WASM_YIELD_INTERVAL == 0)
                    {
                        await Task.Delay(1);
                    }
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
    /// <summary>
    /// Fixes up IAT entries for data imports after all modules are registered.
    /// Some imports like _iob, __mb_cur_max are data exports, not functions.
    /// The IAT entry should point to the actual data, not a function stub.
    /// </summary>
    private void FixupDataImports()
    {
        if (_image == null || _vm == null || _dispatcher == null)
        {
            return;
        }

        _logger.LogInformation("[Loader] Fixing up data imports");

        // Get the import map from the image
        var importMap = _image.ImportAddressMap;
        if (importMap == null || importMap.Count == 0)
        {
            _logger.LogDebug("[Loader] No imports to fix up");
            return;
        }

        var fixupCount = 0;
        
        // Iterate through all imports and check if they are data imports
        foreach (var kvp in importMap)
        {
            var synthetic = kvp.Key;
            var (dll, name) = kvp.Value;
            
            // Check if this is a known data import
            if (PeImageLoader.IsKnownDataImport(dll, name))
            {
                _logger.LogInformation("[Loader] Fixing up data import: {Dll}!{Name} (synthetic=0x{Synthetic:X8})", dll, name, synthetic);
                
                // Get the actual data address from the module
                var dataAddress = GetDataImportAddress(dll, name);
                if (dataAddress != 0)
                {
                    // Find the IAT entry that points to this synthetic address and update it
                    // The IAT entry currently points to the synthetic function stub
                    // We need to replace it with the actual data address
                    var iatEntryMap = _image.IatEntryMap;
                    if (iatEntryMap != null)
                    {
                        foreach (var iatKvp in iatEntryMap)
                        {
                            var iatVa = iatKvp.Key;
                            var expectedSynthetic = iatKvp.Value;
                            
                            if (expectedSynthetic == synthetic)
                            {
                                _logger.LogInformation("[Loader] Patching IAT entry at 0x{IatVa:X8}: 0x{Old:X8} -> 0x{New:X8}", 
                                    iatVa, synthetic, dataAddress);
                                _vm.Write32(iatVa, dataAddress);
                                fixupCount++;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("[Loader] Failed to get data address for {Dll}!{Name}", dll, name);
                }
            }
        }
        
        _logger.LogInformation("[Loader] Fixed up {Count} data import(s)", fixupCount);
    }

    /// <summary>
    /// Gets the actual data address for a data import by calling the appropriate module function
    /// </summary>
    private uint GetDataImportAddress(string dll, string name)
    {
        var upperDll = dll.ToUpperInvariant();
        var upperName = name.ToUpperInvariant();
        
        // Handle MSVCRT data imports
        if ((upperDll == "MSVCRT.DLL" || upperDll == "MSVCRT") &&
            _dispatcher!.TryGetModule("MSVCRT.DLL", out var msvcrtModule) &&
            msvcrtModule != null)
        {
            switch (upperName)
            {
                case "_IOB":
                {
                    // Call __p__iob() to get the array address
                    if (msvcrtModule.TryInvokeUnsafe("__P__IOB", _cpu!, _vm, out var address))
                    {
                        _logger.LogDebug("[Loader] Got _iob address: 0x{Address:X8}", address);
                        return address;
                    }
                    break;
                }
                case "__MB_CUR_MAX":
                {
                    // Call __p__mb_cur_max() to get the address
                    if (msvcrtModule.TryInvokeUnsafe("__MB_CUR_MAX", _cpu!, _vm, out var address))
                    {
                        _logger.LogDebug("[Loader] Got __mb_cur_max address: 0x{Address:X8}", address);
                        return address;
                    }
                    break;
                }
                case "__INITENV":
                {
                    // Call __p___initenv() to get the address
                    if (msvcrtModule.TryInvokeUnsafe("__P___INITENV", _cpu!, _vm, out var address))
                    {
                        _logger.LogDebug("[Loader] Got __initenv address: 0x{Address:X8}", address);
                        return address;
                    }
                    break;
                }
                case "__WINITENV":
                {
                    // Call __p___winitenv() to get the address
                    if (msvcrtModule.TryInvokeUnsafe("__P___WINITENV", _cpu!, _vm, out var address))
                    {
                        _logger.LogDebug("[Loader] Got __winitenv address: 0x{Address:X8}", address);
                        return address;
                    }
                    break;
                }
                case "_FMODE":
                {
                    // Call __p__fmode() to get the address
                    if (msvcrtModule.TryInvokeUnsafe("__P__FMODE", _cpu!, _vm, out var address))
                    {
                        _logger.LogDebug("[Loader] Got _fmode address: 0x{Address:X8}", address);
                        return address;
                    }
                    break;
                }
                case "_COMMODE":
                {
                    // Call __p__commode() to get the address
                    if (msvcrtModule.TryInvokeUnsafe("__P__COMMODE", _cpu!, _vm, out var address))
                    {
                        _logger.LogDebug("[Loader] Got _commode address: 0x{Address:X8}", address);
                        return address;
                    }
                    break;
                }
                case "_ACMDLN":
                {
                    // Call __p__acmdln() to get the address
                    if (msvcrtModule.TryInvokeUnsafe("__P__ACMDLN", _cpu!, _vm, out var address))
                    {
                        _logger.LogDebug("[Loader] Got _acmdln address: 0x{Address:X8}", address);
                        return address;
                    }
                    break;
                }
                case "_WCMDLN":
                {
                    // Call __p__wcmdln() to get the address
                    if (msvcrtModule.TryInvokeUnsafe("__P__WCMDLN", _cpu!, _vm, out var address))
                    {
                        _logger.LogDebug("[Loader] Got _wcmdln address: 0x{Address:X8}", address);
                        return address;
                    }
                    break;
                }
                case "_ENVIRON":
                {
                    // Call _environ() to get the address
                    if (msvcrtModule.TryInvokeUnsafe("_ENVIRON", _cpu!, _vm, out var address))
                    {
                        _logger.LogDebug("[Loader] Got _environ address: 0x{Address:X8}", address);
                        return address;
                    }
                    break;
                }
                case "__ARGC":
                case "__ARGV":
                case "__WARGV":
                case "_PGMPTR":
                case "_WPGMPTR":
                {
                    // These are set up by __getmainargs/__wgetmainargs and don't have __p__ accessors
                    // For now, allocate placeholder memory
                    var placeholder = _env!.HeapAlloc(0, 4);
                    _logger.LogDebug("[Loader] Allocated placeholder for {Name} at 0x{Address:X8}", upperName, placeholder);
                    return placeholder;
                }
            }
        }
        
        return 0;
    }

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
        
        // ign_teas.exe: Enable debug environment variable by default
        // This prevents log spam from GetEnvironmentVariable when the debug code checks for IGN_TEAS_DEBUG
        if (exeNameFromImage == "IGN_TEAS.EXE" || exeNameFromEnv == "IGN_TEAS.EXE" || 
            exeNameFromImage.Contains("IGN_TEAS") || exeNameFromEnv.Contains("IGN_TEAS"))
        {
            _env.SetEnvironmentVariable("IGN_TEAS_DEBUG", "1");
            _logger.LogInformation("[Emulator] IGN_TEAS.EXE detected - enabled IGN_TEAS_DEBUG environment variable");
        }
        
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
    
    /// <summary>
    /// Handles executable-specific function calls by providing C# overrides for diagnostics and debugging.
    /// Returns true if the call was handled, false otherwise.
    /// </summary>
    private bool TryHandleExecutableSpecificCall(uint callTarget)
    {
	    if (_image == null || _vm == null || _env == null || _cpu == null)
	    {
		    return false;
	    }
	    
	    var exeNameFromImage = Path.GetFileName(_image.FilePath ?? "").ToUpperInvariant();
	    var exeNameFromEnv = Path.GetFileName(_env.ExecutablePath ?? "").ToUpperInvariant();
	    
	    // IGN_TEAS.EXE function overrides for debugging initialization flow
	    if (exeNameFromImage == "IGN_TEAS.EXE" || exeNameFromEnv == "IGN_TEAS.EXE" ||
	        exeNameFromImage.Contains("IGN_TEAS") || exeNameFromEnv.Contains("IGN_TEAS"))
	    {
		    return HandleIgnTeasFunctionCall(callTarget);
	    }
	    
	    return false;
    }
    
    /// <summary>
    /// Handles IGN_TEAS.EXE specific function calls for debugging.
    /// Key functions based on Ghidra decompilation:
    /// - 0x004023F0: Main initialization (calls texture loading and DirectDraw setup)
    /// - 0x00402540: Heap/memory initialization
    /// - 0x004025D0: Texture loading (contains the problematic loop at 0x004027A2-0x004027B4)
    /// - 0x004027D0: DirectDraw/rendering initialization
    /// - 0x00403140: WinMain (message loop setup)
    /// - 0x00403510: DirectDraw creation and mode setup
    /// - 0x004032A0: Main game tick function
    /// - 0x00402410: Game logic update
    /// </summary>
    private bool HandleIgnTeasFunctionCall(uint callTarget)
    {
	    switch (callTarget)
	    {
		    case IGN_TEAS_MAIN_INIT_ADDR: // Main initialization function
			    _logger.LogWarning("[IGN_TEAS] Entering FUN_004023F0 (Main Initialization)");
			    _logger.LogWarning("[IGN_TEAS]   This function calls: FUN_00402540, FUN_004025D0 (texture loading), FUN_004027D0, FUN_004011A0");
			    return false; // Let it execute normally but we've logged it
			    
		    case IGN_TEAS_HEAP_INIT_ADDR: // Heap/memory initialization
			    _logger.LogWarning("[IGN_TEAS] Entering FUN_00402540 (Heap/Memory Initialization)");
			    _logger.LogWarning("[IGN_TEAS]   Allocates memory regions for game data");
			    return false; // Let it execute normally
			    
		    case IGN_TEAS_TEXTURE_LOADING_ADDR: // Texture loading function (contains the problematic loop)
			    _logger.LogWarning("[IGN_TEAS] Entering FUN_004025D0 (Texture Loading - PROBLEMATIC FUNCTION)");
			    _logger.LogWarning("[IGN_TEAS]   This function contains the texture data processing loop");
			    _logger.LogWarning("[IGN_TEAS]   Loop at 0x004027A2-0x004027B4 calculates: uVar8 = sVar3 + 0xffff >> 0x10");
			    _logger.LogWarning("[IGN_TEAS]   Expected iterations: ~16 per 1MB texture file");
			    _logger.LogWarning("[IGN_TEAS]   In WASM, this loop may iterate millions of times due to arithmetic bug");
			    return false; // Let it execute normally
			    
		    case IGN_TEAS_DDRAW_INIT_ADDR: // DirectDraw/rendering initialization
			    _logger.LogWarning("[IGN_TEAS] Entering FUN_004027D0 (DirectDraw/Rendering Initialization)");
			    _logger.LogWarning("[IGN_TEAS]   This should initialize DirectDraw surfaces and rendering");
			    return false; // Let it execute normally
			    
		    case IGN_TEAS_WINMAIN_ADDR: // WinMain
			    _logger.LogWarning("[IGN_TEAS] Entering FUN_00403140 (WinMain - Main Entry Point)");
			    _logger.LogWarning("[IGN_TEAS]   Registers window class, creates window, starts message loop");
			    return false; // Let it execute normally
			    
		    case IGN_TEAS_DDRAW_CREATE_ADDR: // DirectDraw creation and mode setup
			    _logger.LogWarning("[IGN_TEAS] Entering FUN_00403510 (DirectDraw Creation)");
			    _logger.LogWarning("[IGN_TEAS]   This should call DirectDrawCreate and set display mode");
			    return false; // Let it execute normally
			    
		    case IGN_TEAS_MAIN_TICK_ADDR: // Main game tick function
			    var eip = _cpu.GetEip();
			    _logger.LogDebug("[IGN_TEAS] Entering FUN_004032A0 (Main Game Tick) - EIP=0x{Eip:X8}", eip);
			    _logger.LogDebug("[IGN_TEAS]   State check: DAT_0041c7a8 (game state), DAT_0041c828 (init flag)");
			    // Log game state variables
			    try
			    {
				    var gameState = _vm.Read32(IGN_TEAS_GAME_STATE_ADDR);
				    var initFlag = _vm.Read32(IGN_TEAS_INIT_FLAG_ADDR);
				    var exitFlag = _vm.Read32(IGN_TEAS_EXIT_FLAG_ADDR);
				    _logger.LogDebug("[IGN_TEAS]   Game State: DAT_0041c7a8={GameState}, DAT_0041c828={InitFlag}, DAT_0041c82c={ExitFlag}", 
					    gameState, initFlag, exitFlag);
			    }
			    catch (Exception ex)
			    {
				    _logger.LogDebug(ex, "[IGN_TEAS]   Could not read game state variables");
			    }
			    return false; // Let it execute normally
			    
		    case IGN_TEAS_GAME_LOGIC_ADDR: // Game logic update
			    _logger.LogDebug("[IGN_TEAS] Entering FUN_00402410 (Game Logic Update)");
			    return false; // Let it execute normally
			    
		    case IGN_TEAS_CLEANUP_ADDR: // Cleanup
			    _logger.LogWarning("[IGN_TEAS] Entering FUN_00402520 (Cleanup)");
			    return false; // Let it execute normally
			    
		    default:
			    return false; // Not a function we're tracking
	    }
    }
    
    /// <summary>
    /// Tracks execution within the IGN_TEAS texture loading loop to diagnose infinite loop issues.
    /// The loop at 0x004027A2-0x004027B4 in FUN_004025D0 should iterate ~16 times per texture file.
    /// In WASM, it can iterate millions of times due to arithmetic operation bugs.
    /// </summary>
    private void TrackIgnTeasTextureLoop(uint eip)
    {
	    if (_image == null)
	    {
		    return;
	    }
	    
	    var exeNameFromImage = Path.GetFileName(_image.FilePath ?? "").ToUpperInvariant();
	    var exeNameFromEnv = Path.GetFileName(_env?.ExecutablePath ?? "").ToUpperInvariant();
	    
	    // Only track for IGN_TEAS.EXE
	    if (!(exeNameFromImage == "IGN_TEAS.EXE" || exeNameFromEnv == "IGN_TEAS.EXE" ||
	          exeNameFromImage.Contains("IGN_TEAS") || exeNameFromEnv.Contains("IGN_TEAS")))
	    {
		    return;
	    }
	    
	    // Track the problematic loop: 0x004027A2 through 0x004027B4
	    // This is the "do { *puVar10 = pvVar6; puVar10 = puVar10 + 1; pvVar6 = (void *)((int)pvVar6 + 0x10000); uVar8 = uVar8 - 1; } while (uVar8 != 0);"
	    if (eip >= IGN_TEAS_TEXTURE_LOOP_START && eip <= IGN_TEAS_TEXTURE_LOOP_END)
	    {
		    _ignTeasLoopIterations++;
		    
		    // Log periodically to track progress without spamming
		    if (_ignTeasLoopIterations - _ignTeasLastLoopLogIteration >= IGN_TEAS_LOOP_LOG_INTERVAL)
		    {
			    _ignTeasLastLoopLogIteration = _ignTeasLoopIterations;
			    
			    // Read loop counter and pointer variables
			    try
			    {
				    if (_cpu != null)
				    {
					    var eax = _cpu.GetRegister("EAX");
					    var ebx = _cpu.GetRegister("EBX");
					    var ecx = _cpu.GetRegister("ECX");
					    var edx = _cpu.GetRegister("EDX");
					    var esi = _cpu.GetRegister("ESI");
					    var edi = _cpu.GetRegister("EDI");
					    
					    _logger.LogWarning("[IGN_TEAS] Texture loop iteration {Iterations} at EIP=0x{Eip:X8}", _ignTeasLoopIterations, eip);
					    _logger.LogWarning("[IGN_TEAS]   Registers: EAX=0x{Eax:X8} EBX=0x{Ebx:X8} ECX=0x{Ecx:X8} EDX=0x{Edx:X8} ESI=0x{Esi:X8} EDI=0x{Edi:X8}",
						    eax, ebx, ecx, edx, esi, edi);
					    _logger.LogWarning("[IGN_TEAS]   Expected: ~16-32 iterations per 1MB texture file");
					    _logger.LogWarning("[IGN_TEAS]   If this count keeps growing, we're in the WASM arithmetic bug");
				    }
			    }
			    catch (Exception ex)
			    {
				    _logger.LogDebug(ex, "[IGN_TEAS] Could not read registers during loop tracking");
			    }
		    }
	    }
	    // Reset counter when we exit the loop
	    else if (_ignTeasLoopIterations > 0)
	    {
		    if (_ignTeasLoopIterations > MIN_SIGNIFICANT_TEXTURE_LOOP_ITERATIONS) // Only log if significant iterations occurred
		    {
			    _logger.LogWarning("[IGN_TEAS] Exited texture loop after {Iterations} total iterations", _ignTeasLoopIterations);
			    if (_ignTeasLoopIterations > EXCESSIVE_TEXTURE_LOOP_THRESHOLD)
			    {
				    _logger.LogError("[IGN_TEAS] ⚠️ Loop iterated {Iterations} times - this is excessive and indicates the WASM arithmetic bug!", _ignTeasLoopIterations);
			    }
		    }
		    _ignTeasLoopIterations = 0;
		    _ignTeasLastLoopLogIteration = 0;
	    }
    }
    
    /// <summary>
    /// Tracks execution within the IGN_TEAS CRT startup loop to diagnose the infinite loop.
    /// The loop at 0x00411060 → 0x00412620 → 0x004124C3-0x00412611 is CharNext-style string parsing.
    /// Logs memory contents and register state to identify why termination condition is never met.
    /// </summary>
    private void TrackIgnTeasCrtLoop(uint eip)
    {
	    if (_image == null || _vm == null || _cpu == null)
	    {
		    return;
	    }
	    
	    var exeNameFromImage = Path.GetFileName(_image.FilePath ?? "").ToUpperInvariant();
	    var exeNameFromEnv = Path.GetFileName(_env?.ExecutablePath ?? "").ToUpperInvariant();
	    
	    // Only track for IGN_TEAS.EXE
	    if (!(exeNameFromImage == "IGN_TEAS.EXE" || exeNameFromEnv == "IGN_TEAS.EXE" ||
	          exeNameFromImage.Contains("IGN_TEAS") || exeNameFromEnv.Contains("IGN_TEAS")))
	    {
		    return;
	    }
	    
	    // Track the CRT startup loop: entry points and main parsing loop
	    bool inCrtLoop = (eip == IGN_TEAS_CRT_ENTRY_1 || eip == IGN_TEAS_CRT_ENTRY_2 || 
	                      (eip >= IGN_TEAS_CRT_LOOP_START && eip <= IGN_TEAS_CRT_LOOP_END));
	    
	    if (inCrtLoop)
	    {
		    _ignTeasCrtLoopIterations++;
		    
		    // Log periodically with detailed information
		    if (_ignTeasCrtLoopIterations - _ignTeasCrtLastLogIteration >= IGN_TEAS_CRT_LOG_INTERVAL)
		    {
			    _ignTeasCrtLastLogIteration = _ignTeasCrtLoopIterations;
			    
			    try
			    {
				    var eax = _cpu.GetRegister("EAX");
				    var ebx = _cpu.GetRegister("EBX");
				    var ecx = _cpu.GetRegister("ECX");
				    var edx = _cpu.GetRegister("EDX");
				    var esi = _cpu.GetRegister("ESI");
				    var edi = _cpu.GetRegister("EDI");
				    var esp = _cpu.GetRegister("ESP");
				    var ebp = _cpu.GetRegister("EBP");
				    
				    _logger.LogWarning("[IGN_TEAS CRT] Loop iteration {Iterations} at EIP=0x{Eip:X8}", _ignTeasCrtLoopIterations, eip);
				    _logger.LogWarning("[IGN_TEAS CRT]   Registers: EAX=0x{Eax:X8} EBX=0x{Ebx:X8} ECX=0x{Ecx:X8} EDX=0x{Edx:X8}", eax, ebx, ecx, edx);
				    _logger.LogWarning("[IGN_TEAS CRT]   ESI=0x{Esi:X8} (iterator) EDI=0x{Edi:X8} ESP=0x{Esp:X8} EBP=0x{Ebp:X8}", esi, edi, esp, ebp);
				    
				    // Log string buffer content once to see what's being parsed
				    if (!_ignTeasCrtStringLogged && ecx != 0 && ecx >= IGN_TEAS_VALID_MEMORY_START && ecx < IGN_TEAS_VALID_MEMORY_END)
				    {
					    try
					    {
						    _logger.LogWarning("[IGN_TEAS CRT] String buffer at ECX=0x{Ecx:X8}:", ecx);
						    var bytes = new byte[IGN_TEAS_STRING_BUFFER_SIZE];
						    for (int i = 0; i < IGN_TEAS_STRING_BUFFER_SIZE; i++)
						    {
							    bytes[i] = _vm.Read8(ecx + (uint)i);
						    }
						    
						    // Log as hex dump
						    var hex = BitConverter.ToString(bytes).Replace("-", " ");
						    _logger.LogWarning("[IGN_TEAS CRT]   Hex: {Hex}", hex.Substring(0, Math.Min(IGN_TEAS_HEX_DUMP_MAX_LENGTH, hex.Length)));
						    
						    // Try to interpret as ASCII string
						    var str = System.Text.Encoding.ASCII.GetString(bytes).Replace("\0", "\\0").Replace("\r", "\\r").Replace("\n", "\\n");
						    _logger.LogWarning("[IGN_TEAS CRT]   ASCII: {Str}", str.Substring(0, Math.Min(IGN_TEAS_ASCII_STRING_MAX_LENGTH, str.Length)));
						    
						    _ignTeasCrtStringLogged = true;
					    }
					    catch (Exception ex)
					    {
						    _logger.LogWarning(ex, "[IGN_TEAS CRT] Failed to read string buffer at ECX");
					    }
				    }
				    
				    // Log current byte being examined at ESI
				    if (esi >= IGN_TEAS_VALID_MEMORY_START && esi < IGN_TEAS_VALID_MEMORY_END)
				    {
					    try
					    {
						    var currentByte = _vm.Read8(esi);
						    var nextByte = _vm.Read8(esi + 1);
						    var prevByte = esi > IGN_TEAS_VALID_MEMORY_START ? _vm.Read8(esi - 1) : (byte)0;
						    
						    _logger.LogWarning("[IGN_TEAS CRT]   Current position ESI=0x{Esi:X8}: prev=0x{Prev:X2} current=0x{Current:X2} ('{CurrentChar}') next=0x{Next:X2}", 
							    esi, prevByte, currentByte, (char)(currentByte >= ASCII_PRINTABLE_MIN && currentByte < ASCII_PRINTABLE_MAX ? (char)currentByte : '.'), nextByte);
					    }
					    catch (Exception ex)
					    {
						    _logger.LogDebug(ex, "[IGN_TEAS CRT] Failed to read byte at ESI");
					    }
				    }
				    
				    if (_ignTeasCrtLoopIterations > CRT_LOOP_STUCK_THRESHOLD)
				    {
					    _logger.LogError("[IGN_TEAS CRT] ⚠️ CRT loop has iterated {Iterations} times - definitely stuck!", _ignTeasCrtLoopIterations);
				    }
			    }
			    catch (Exception ex)
			    {
				    _logger.LogDebug(ex, "[IGN_TEAS CRT] Could not read registers during CRT loop tracking");
			    }
		    }
	    }
	    // Reset counter when we exit the loop
	    else if (_ignTeasCrtLoopIterations > 0)
	    {
		    _ignTeasCrtLoopExitCount++;
		    _logger.LogWarning("[IGN_TEAS CRT] Exited CRT loop after {Iterations} total iterations (exit #{ExitCount})", _ignTeasCrtLoopIterations, _ignTeasCrtLoopExitCount);
		    if (_ignTeasCrtLoopIterations > CRT_LOOP_PARSING_BUG_THRESHOLD)
		    {
			    _logger.LogError("[IGN_TEAS CRT] ⚠️ CRT loop iterated {Iterations} times - this indicates a parsing bug!", _ignTeasCrtLoopIterations);
		    }
		    _ignTeasCrtLoopIterations = 0;
		    _ignTeasCrtLastLogIteration = 0;
		    _ignTeasCrtStringLogged = false;
	    }
	    // Track execution after CRT exits (after 4th exit, start logging to see where it goes)
	    else if (_ignTeasCrtLoopExitCount >= 4)
	    {
		    _ignTeasPostCrtExecutionCount++;
		    
		    // Dump buffer once at start to see what was written
		    if (_ignTeasPostCrtExecutionCount == 1 && _vm != null)
		    {
			    _logger.LogWarning("[IGN_TEAS POST-CRT] Dumping environment buffer at 0x00480200 (bytes 0-99, 320-349):");
			    try
			    {
				    // First 100 bytes
				    var buffer = new byte[100];
				    for (int i = 0; i < 100; i++)
				    {
					    buffer[i] = _vm.Read8(0x00480200 + (uint)i);
				    }
				    var hex = BitConverter.ToString(buffer).Replace("-", " ");
				    _logger.LogWarning("[IGN_TEAS POST-CRT]   Bytes 0-99 Hex: {Hex}", hex);
				    
				    // Last 30 bytes around 329 (0x149)
				    var endBuffer = new byte[30];
				    for (int i = 0; i < 30; i++)
				    {
					    endBuffer[i] = _vm.Read8(0x00480200 + 320 + (uint)i);
				    }
				    var endHex = BitConverter.ToString(endBuffer).Replace("-", " ");
				    _logger.LogWarning("[IGN_TEAS POST-CRT]   Bytes 320-349 Hex: {Hex}", endHex);
				    _logger.LogWarning("[IGN_TEAS POST-CRT]   (Byte 329 is at index 329, should be double-null at 327-328 or 328-329)");
				    
				    // Check what's at EBP (frame pointer) - CRT may expect environment pointer there
				    var ebp = _cpu!.GetRegister("EBP");
				    var ptrAtEbp = _vm.Read32(ebp);
				    _logger.LogError("[IGN_TEAS POST-CRT] ⚠️  EBP=0x{Ebp:X8}, value at [EBP]=0x{PtrAtEbp:X8}", ebp, ptrAtEbp);
				    
				    // Check what's at that pointer (if it looks valid)
				    if (ptrAtEbp >= 0x00400000 && ptrAtEbp < 0x10000000)
				    {
					    try
					    {
						    var firstBytes = new byte[32];
						    for (int i = 0; i < 32; i++)
						    {
							    firstBytes[i] = _vm.Read8(ptrAtEbp + (uint)i);
						    }
						    var ptrHex = BitConverter.ToString(firstBytes).Replace("-", " ");
						    _logger.LogError("[IGN_TEAS POST-CRT] Data at [EBP] pointer 0x{Ptr:X8}: {Hex}", ptrAtEbp, ptrHex);
					    }
					    catch (Exception ex)
					    {
						    _logger.LogError(ex, "[IGN_TEAS POST-CRT] Failed to read from pointer at [EBP]");
					    }
				    }
				    else
				    {
					    _logger.LogError("[IGN_TEAS POST-CRT] Pointer at [EBP] looks invalid (0x{Ptr:X8}) - should point to environment", ptrAtEbp);
				    }
			    }
			    catch (Exception ex)
			    {
				    _logger.LogError(ex, "[IGN_TEAS POST-CRT] Failed to dump buffer");
			    }
		    }
		    
		    // Check if we're at the REPNZ SCAS instruction at 0x004122CF
		    // This instruction scans [ESP+0x14] for a null byte
		    if (eip == 0x004122CF)
		    {
			    try
			    {
				    var esp = _cpu!.GetRegister("ESP");
				    var edi = _cpu!.GetRegister("EDI");
				    var ecx = _cpu!.GetRegister("ECX");
				    var stackBufAddr = esp + 0x14;
				    
				    // Log every 100000th time we hit this instruction
				    if (_ignTeasPostCrtExecutionCount % 100000 == 0)
				    {
					    _logger.LogError("[IGN_TEAS SCAS] At REPNZ SCAS (0x004122CF): ESP=0x{Esp:X8}, EDI=0x{Edi:X8}, ECX=0x{Ecx:X8}", esp, edi, ecx);
					    _logger.LogError("[IGN_TEAS SCAS] Stack buffer at [ESP+0x14]=0x{Addr:X8}", stackBufAddr);
					    
					    // Dump first 64 bytes of stack buffer
					    try
					    {
						    var stackBuf = new byte[64];
						    for (int i = 0; i < 64; i++)
						    {
							    stackBuf[i] = _vm!.Read8(stackBufAddr + (uint)i);
						    }
						    var hex = BitConverter.ToString(stackBuf).Replace("-", " ");
						    var ascii = new System.Text.StringBuilder();
						    for (int i = 0; i < 64; i++)
						    {
							    var b = stackBuf[i];
							    ascii.Append(b >= 32 && b < 127 ? (char)b : '.');
						    }
						    _logger.LogError("[IGN_TEAS SCAS] Stack buffer: {Hex}", hex);
						    _logger.LogError("[IGN_TEAS SCAS] Stack ASCII: {Ascii}", ascii.ToString());
					    }
					    catch (Exception ex)
					    {
						    _logger.LogError(ex, "[IGN_TEAS SCAS] Failed to read stack buffer");
					    }
				    }
			    }
			    catch (Exception ex)
			    {
				    _logger.LogError(ex, "[IGN_TEAS SCAS] Failed to read registers");
			    }
		    }
		    
		    // Log periodically to see where we are
		    if (_ignTeasPostCrtExecutionCount % IGN_TEAS_POST_FINAL_CRT_LOG_INTERVAL == 0)
		    {
			    try
			    {
				    var eax = _cpu!.GetRegister("EAX");
				    var ebx = _cpu!.GetRegister("EBX");
				    var ecx = _cpu!.GetRegister("ECX");
				    var edx = _cpu!.GetRegister("EDX");
				    var esi = _cpu!.GetRegister("ESI");
				    var edi = _cpu!.GetRegister("EDI");
				    var esp = _cpu!.GetRegister("ESP");
				    var ebp = _cpu!.GetRegister("EBP");
				    
				    _logger.LogWarning("[IGN_TEAS POST-CRT] Executing after final CRT exit: EIP=0x{Eip:X8} ({Count} instructions since exit)", eip, _ignTeasPostCrtExecutionCount);
				    _logger.LogWarning("[IGN_TEAS POST-CRT]   Registers: EAX=0x{Eax:X8} EBX=0x{Ebx:X8} ECX=0x{Ecx:X8} EDX=0x{Edx:X8}", eax, ebx, ecx, edx);
				    _logger.LogWarning("[IGN_TEAS POST-CRT]   ESI=0x{Esi:X8} EDI=0x{Edi:X8} ESP=0x{Esp:X8} EBP=0x{Ebp:X8}", esi, edi, esp, ebp);
				    
				    // Check if stuck in tight loop
				    if (_ignTeasLastPostCrtEipForLoop == eip)
				    {
					    _logger.LogError("[IGN_TEAS POST-CRT] ⚠️ Stuck at same EIP=0x{Eip:X8} - infinite loop detected!", eip);
				    }
				    _ignTeasLastPostCrtEipForLoop = eip;
			    }
			    catch (Exception ex)
			    {
				    _logger.LogDebug(ex, "[IGN_TEAS POST-CRT] Could not read registers");
				    _logger.LogWarning("[IGN_TEAS POST-CRT] Executing after final CRT exit: EIP=0x{Eip:X8} ({Count} instructions since exit)", eip, _ignTeasPostCrtExecutionCount);
			    }
		    }
	    }
    }
    
    // IGN_TEAS.EXE: Track if we ever reach key addresses after CRT
    private bool _ignTeasReachedBeyondCrt = false;
    private uint _ignTeasLastPostCrtEip = 0;
    
    private void TrackIgnTeasProgress(uint eip)
    {
	    if (_image == null)
	    {
		    return;
	    }
	    
	    var exeNameFromImage = Path.GetFileName(_image.FilePath ?? "").ToUpperInvariant();
	    
	    // Only track for IGN_TEAS.EXE
	    if (!(exeNameFromImage == "IGN_TEAS.EXE" || exeNameFromImage.Contains("IGN_TEAS")))
	    {
		    return;
	    }
	    
	    // Track ALL execution after CRT range to see where it goes
	    if (eip >= IGN_TEAS_POST_CRT_START && eip < IGN_TEAS_POST_CRT_END)
	    {
		    _ignTeasPostCrtInstructions++;
		    
		    // Log periodically in this range
		    if (_ignTeasPostCrtInstructions % IGN_TEAS_POST_CRT_LOG_INTERVAL == 0)
		    {
			    _logger.LogWarning("[IGN_TEAS POST-CRT] Executing in 0x00413XXX-0x0041XXXX range: EIP=0x{Eip:X8} ({Count} instructions)", eip, _ignTeasPostCrtInstructions);
		    }
		    
		    // Track if stuck in tight loop
		    if (_ignTeasLastPostCrtEip == eip)
		    {
			    _logger.LogError("[IGN_TEAS POST-CRT] ⚠️ Stuck at EIP=0x{Eip:X8} - same address consecutively!", eip);
		    }
		    _ignTeasLastPostCrtEip = eip;
	    }
	    
	    // Check if we've reached addresses beyond CRT initialization (WinMain area)
	    if (!_ignTeasReachedBeyondCrt && eip >= IGN_TEAS_WINMAIN_RANGE_START && eip < IGN_TEAS_WINMAIN_RANGE_END)
	    {
		    _ignTeasReachedBeyondCrt = true;
		    _logger.LogWarning("[IGN_TEAS PROGRESS] ✅ Reached address 0x{Eip:X8} - BEYOND CRT initialization!", eip);
		    _logger.LogWarning("[IGN_TEAS PROGRESS] This means CRT completed successfully and we're in game code");
	    }
	    
	    // Known key addresses
	    if (eip == IGN_TEAS_WINMAIN_ADDR)
	    {
		    _logger.LogWarning("[IGN_TEAS PROGRESS] ✅✅ Reached WinMain at 0x00403140!");
	    }
	    else if (eip == IGN_TEAS_MAIN_INIT_ADDR)
	    {
		    _logger.LogWarning("[IGN_TEAS PROGRESS] ✅✅ Reached Main Init at 0x004023F0!");
	    }
	    
	    // Track if execution is stuck in "limbo" range (between CRT and entry)
	    if (eip >= IGN_TEAS_LIMBO_START && eip < IGN_TEAS_LIMBO_END)
	    {
		    _ignTeasLimboInstructions++;
		    // Log periodically to see if stuck here
		    if (_ignTeasLimboInstructions % IGN_TEAS_LIMBO_LOG_INTERVAL == 0)
		    {
			    _logger.LogWarning("[IGN_TEAS LIMBO] Still in post-CRT range 0x00411XXX-0x00413XXX at EIP=0x{Eip:X8}", eip);
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
            
            // Save JIT cache if using JitCpu
            if (_cpu is Cpu.Jit.JitCpu jitCpu)
            {
                try
                {
                    await jitCpu.SaveCacheAsync().ConfigureAwait(false);
                    _logger.LogInformation("[Emulator] JIT cache saved successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[Emulator] Failed to save JIT cache (non-fatal)");
                }
            }
            
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
        
        // Throttle noisy warning logs to reduce spam
        var lastHeapEipWarning = 0u;
        var consecutiveHeapExecutions = 0ul; // Track consecutive executions in heap to detect stuck execution
        
        // WASM emergency yield: Track last yield time to prevent prolonged browser freezes
        // If more than 100ms passes without yielding, force an emergency yield
        var lastYieldTime = DateTime.UtcNow;

        // Run indefinitely until stop/exit requested or no threads running
        while (!_stopRequested && !_env!.ExitRequested)
        {
            iterationCount++;
            
            // Debug: Log EIP at start of iteration to track changes (trace-only to avoid hot-path overhead)
            if (iterationCount <= 40 && _logger.IsEnabled(LogLevel.Trace))
            {
                var eipAtLoopStart = _cpu!.GetEip();
                var espAtLoopStart = _cpu.GetRegister("ESP");
                _logger.LogTrace("[Emulator] Iteration {Count} START: EIP=0x{Eip:X8}, ESP=0x{Esp:X8}", iterationCount, eipAtLoopStart, espAtLoopStart);
            }
            
            // Log progress periodically for debugging
            if (iterationCount % PROGRESS_LOG_INTERVAL == 0)
            {
                var now = DateTime.UtcNow;
                var elapsed = (now - lastLogTime).TotalMilliseconds;
                var progressEip = _cpu!.GetEip();
                var progressEsp = _cpu.GetRegister("ESP");
                
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("[Emulator] Progress: {Iterations} iterations ({Elapsed:F2}ms), EIP=0x{Eip:X8}, ESP=0x{Esp:X8}", 
                        iterationCount, elapsed, progressEip, progressEsp);
                }
                
                lastLogTime = now;
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
            // IMPORTANT: Task.Yield() doesn't work in WASM - it only yields to .NET scheduler
            // but doesn't return control to browser. Use Task.Delay(1) instead to allow
            // the browser's event loop to process events.
            // Note: PlatformHelpers.IsWasm is a static readonly field that JIT can constant-fold.
            // On non-WASM platforms, the && short-circuits and the modulo is never evaluated.
            if (PlatformHelpers.IsWasm)
            {
                var needsYield = iterationCount % WASM_YIELD_INTERVAL == 0;
                
                // Emergency yield: If more than EMERGENCY_YIELD_THRESHOLD_MS has passed since last yield, force a yield
                // This prevents the browser from freezing even if we're stuck in a tight loop
                // Check every 100 iterations to reduce DateTime.UtcNow overhead
                if (!needsYield && iterationCount % 100 == 0)
                {
                    var timeSinceLastYield = (DateTime.UtcNow - lastYieldTime).TotalMilliseconds;
                    if (timeSinceLastYield > EMERGENCY_YIELD_THRESHOLD_MS)
                    {
                        needsYield = true;
                        _logger.LogWarning("[Emulator] Emergency yield after {Ms}ms without yielding (iteration {Count})", timeSinceLastYield, iterationCount);
                    }
                }
                
                if (needsYield)
                {
                    // Use Task.Delay(1) instead of Task.Yield() to actually return control to browser
                    // Task.Delay schedules a JavaScript timer which allows the browser event loop to run
                    await Task.Delay(1);
                    lastYieldTime = DateTime.UtcNow;
                }
            }

            if (_stopRequested)
            {
	            break;
            }

            // Process wait timeouts BEFORE checking for runnable threads
            // This ensures sleeping threads are woken up before we check if any threads are runnable
            scheduler?.ProcessWaitTimeouts();

            // Process events from rendering and input backends periodically
            // This is essential for applications that wait for window messages (WM_PAINT, WM_TIMER, etc.)
            // and ensures GetMessageA doesn't block forever when no DirectDraw rendering is happening
            // Process events every EVENT_PROCESSING_INTERVAL iterations for responsive message handling
            if (iterationCount % EVENT_PROCESSING_INTERVAL == 0)
            {
                _env?.ProcessAllBackendEvents();
                
                // Post a synthetic WM_PAINT message to keep the message queue active
                // This is especially important in headless mode where SDL may not generate any events
                // DirectDraw does the same thing after Flip - we need it for apps that don't use DirectDraw
                var firstWindow = _env?.GetAllWindowHandles().FirstOrDefault() ?? 0;
                if (firstWindow != 0)
                {
                    _env?.PostMessage(firstWindow, (uint)Win32.Messaging.WM.PAINT, 0, 0);
                }
            }

            // Check if we have any runnable threads
            // However, if threads are blocked waiting for messages and we have timers or the event processing
            // loop running, we should continue execution to allow timers to fire and wake up blocked threads
            if (scheduler != null && !scheduler.HasRunningThreads())
            {
                // Check if there are any waiting threads that might be woken up by events/timers
                var hasWaitingThreads = scheduler.GetAllThreads().Any(t => t.State == Threading.ThreadState.Waiting);
                
                if (!hasWaitingThreads)
                {
                    // No threads at all - stop execution
                    LogDebug("[Emulator] No more runnable threads, stopping execution");
                    break;
                }
                
                // We have waiting threads - give the event processing loop a chance to post messages/fire timers
                // by yielding briefly. This prevents busy-waiting when all threads are blocked.
                await Task.Delay(1).ConfigureAwait(false);
                continue;
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
            var isExecutingInHeapRange = eipBeforeStep >= _heapBase && eipBeforeStep < HEAP_LIMIT;
            var isExecutingInSpecialRange = MemoryRegions.IsInSpecialRange(eipBeforeStep);
            if (isExecutingInHeapRange && !isExecutingInSpecialRange)
            {
                consecutiveHeapExecutions++;
                
                // Log warning only when EIP changes to reduce log noise
                if (eipBeforeStep != lastHeapEipWarning)
                {
                    _logger.LogWarning("[Emulator] EIP=0x{Eip:X8} is in heap memory range (0x{HeapBase:X8}-0x{HeapLimit:X8}). This may indicate a bad jump or return address. Consecutive heap executions: {Count}", 
                        eipBeforeStep, _heapBase, HEAP_LIMIT - 1, consecutiveHeapExecutions);
                    lastHeapEipWarning = eipBeforeStep;
                }
                
                if (consecutiveHeapExecutions >= MAX_CONSECUTIVE_HEAP_EXECUTIONS)
                {
                    var esp = _cpu.GetRegister("ESP");
                    _logger.LogError("[Emulator] HEAP EXECUTION DETECTED: EIP has been in heap memory range for {Count} consecutive iterations. EIP=0x{Eip:X8}, ESP=0x{Esp:X8}. Stopping emulation.", 
                        consecutiveHeapExecutions, eipBeforeStep, esp);
                    _logger.LogError("[Emulator] This indicates the program jumped into data memory (likely due to a bug or corrupted return address). Normal programs should never execute code from the heap region.");
                    break; // Stop emulation
                }
                
                // Verify memory is mapped before attempting to execute
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
            else
            {
                // Reset counter when we're not in heap range
                consecutiveHeapExecutions = 0;
            }

            // Check for callback return marker addresses (0xDEADBEEF and similar)
            // These should never be executed - they're return address markers for callbacks
            if (eipBeforeStep >= 0xDEAD0000 && eipBeforeStep <= 0xDEADFFFF)
            {
                _logger.LogError("[Emulator] EIP=0x{Eip:X8} is in callback marker range (0xDEAD0000-0xDEADFFFF). This should never be executed!", eipBeforeStep);
                _logger.LogError("[Emulator] This indicates a problem with callback return handling or stack corruption.");
                var esp = _cpu.GetRegister("ESP");
                var ebp = _cpu.GetRegister("EBP");
                _logger.LogError("[Emulator] ESP=0x{Esp:X8}, EBP=0x{Ebp:X8}", esp, ebp);
                
                // Try to read stack to diagnose the issue
                try
                {
                    var retAddr = _vm!.Read32(esp);
                    _logger.LogError("[Emulator] Top of stack [ESP]=0x{RetAddr:X8}", retAddr);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "[Emulator] Failed to read top of stack at ESP=0x{Esp:X8} while diagnosing callback marker execution.",
                        esp);
                }
                
                throw new InvalidOperationException($"EIP=0x{eipBeforeStep:X8} is in callback marker range. Callback return handling failed.");
            }
            
            // IGN_TEAS.EXE: Track the problematic texture loading loop
            TrackIgnTeasTextureLoop(eipBeforeStep);
            
            // IGN_TEAS.EXE: Track the CRT startup loop (earlier hang point)
            TrackIgnTeasCrtLoop(eipBeforeStep);
            
            // IGN_TEAS.EXE: Track progress beyond CRT
            TrackIgnTeasProgress(eipBeforeStep);

            CpuStepResult step;
            try
            {
                // Use async SingleStep to enable cooperative multitasking on WASM
                // This allows the browser event loop to remain responsive during emulation
                step = await _cpu!.SingleStepAsync(_vm!).ConfigureAwait(false);
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
            if (eipAfterStep < MemoryRegions.MinValidUserAddress)
            {
                // EIP in low memory range (0x0-0xFFFF) is highly suspicious
                // This usually indicates a corrupted return address or bad function pointer
                var esp = _cpu.GetRegister("ESP");
                var ebp = _cpu.GetRegister("EBP");
                _logger.LogError("[Emulator] EIP=0x{Eip:X8} is in suspicious low memory range. Previous EIP=0x{PrevEip:X8}, ESP=0x{Esp:X8}, EBP=0x{Ebp:X8}. Likely corrupted return address or indirect jump.", 
                    eipAfterStep, eipBeforeStep, esp, ebp);
                
                // Halt execution to prevent infinite loop of corruption
                // This allows tests to fail fast instead of timing out
                throw new InvalidOperationException(
                    $"Memory corruption detected: EIP=0x{eipAfterStep:X8} is in suspicious low memory range. " +
                    $"Previous EIP=0x{eipBeforeStep:X8}, ESP=0x{esp:X8}, EBP=0x{ebp:X8}. " +
                    "This indicates a corrupted return address or bad function pointer.");
            }
            
            // Check for syscall (INT 0x80 from import stubs)
            // This is the retrowin32-style approach where import stubs CALL syscall dispatcher
            // The syscall dispatcher triggers INT 0x80, we handle it, then CPU executes RET naturally
            if (step.IsSyscall)
            {
                // Use async syscall handler to support async Win32 API implementations
                // This is required for WASM where blocking operations are not supported
                await HandleSyscallAsync().ConfigureAwait(false);
                var espAfterSyscall = _cpu.GetRegister("ESP");
                _logger.LogDebug("[Emulator] Iteration {Iter}: Syscall handled, continuing to next iteration. EIP=0x{Eip:X8}, ESP=0x{Esp:X8}", iterationCount, _cpu.GetEip(), espAfterSyscall);
                continue; // Continue to next iteration, let CPU execute RET
            }

            // Check for DOS interrupt (INT 21h from Win16 NE executables)
            if (step.IsDosInterrupt)
            {
                await HandleDosInterruptAsync().ConfigureAwait(false);
                continue; // Continue to next iteration
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
                // Logging now handled by ComVtableDispatcher.TryInvokeAsync
                
                var espBefore = _cpu.GetRegister("ESP");
                var eipBefore = _cpu.GetEip();
                
                // Use async version of consolidated helper for register preservation and stdcall convention
                await CpuHelpers.InvokeWithRegisterPreservationAsync(
                    _cpu,
                    _vm!,
                    async () => {
                        return await _env.ComDispatcher.TryInvokeAsync(step.CallTarget, _cpu, _vm!).ConfigureAwait(false);
                    },
                    _vm!.Size,
                    _logger,
                    "COM vtable",
                    _image).ConfigureAwait(false);
                
                var espAfter = _cpu.GetRegister("ESP");
                var eipAfter = _cpu.GetEip();
                _logger.LogInformation("[COM] After async vtable call: ESP changed from 0x{EspBefore:X8} to 0x{EspAfter:X8} (delta={Delta}), Call site EIP=0x{EipBefore:X8}, Return EIP=0x{EipAfter:X8}", 
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
	            // Check for executable-specific function overrides
	            if (TryHandleExecutableSpecificCall(step.CallTarget))
	            {
		            continue; // Function was handled by override
	            }
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
            
            // Loop detection removed to improve performance
            // The every-10k-instruction check was causing significant overhead
            // Applications with legitimate long-running initialization (like ign_teas)
            // were being slowed down by the GetEip() calls and modulo operations

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

                // Check for DOS interrupt (INT 21h from Win16 NE executables)
                if (step.IsDosInterrupt)
                {
                    HandleDosInterruptAsync().GetAwaiter().GetResult();
                    i++;
                    continue;
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

                // Execute one instruction using async SingleStep
                var step = await _cpu.SingleStepAsync(_vm!).ConfigureAwait(false);
                
                // Check for COM vtable method calls
                if (step.IsCall && _env.ComDispatcher.IsComVtableAddress(step.CallTarget))
                {
                    // Logging now handled by ComVtableDispatcher.TryInvokeAsync
                    
                    // Use async version of consolidated helper for register preservation and stdcall convention
                    await CpuHelpers.InvokeWithRegisterPreservationAsync(
                        _cpu,
                        _vm!,
                        async () => {
                            return await _env.ComDispatcher.TryInvokeAsync(step.CallTarget, _cpu, _vm!).ConfigureAwait(false);
                        },
                        _vm!.Size,
                        _logger,
                        "COM vtable (GDB)",
                        _image).ConfigureAwait(false);
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
        // Advance EIP past the INT 0x80 instruction (2 bytes: CD 80)
        // The JitCpu INT handler resets EIP to point AT the INT instruction for us to handle
        // We need to advance it so execution continues at the RET instruction after the INT
        var eipAtInt = _cpu!.GetEip();
        _cpu.SetEip(eipAtInt + 2); // INT 0x80 is 2 bytes
        _logger.LogDebug("[Syscall] Advanced EIP past INT 0x80: 0x{EipBefore:X8} -> 0x{EipAfter:X8}", eipAtInt, _cpu.GetEip());
        
        // The stack looks like:
        // [ESP+0] = return address to import stub (points to RET instruction after CALL)
        // [ESP+4+] = function arguments (pushed by original caller)
        
        var esp = _cpu.GetRegister("ESP");
        
        // Validate ESP is in a reasonable range before attempting to read from stack
        if (esp < MemoryRegions.MinValidUserAddress)
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
            
            // CRITICAL: Win95/Win32 ABI requires direction flag to be CLEARED on entry to Win32 API functions
            // Per Microsoft documentation: "The direction flag must be cleared (set to 0) before calling any Win32 API function"
            // If the CRT or application code sets DF=1 (via STD instruction), we must clear it here
            // to ensure string operations inside the API (and after return) work correctly.
            // This fixes ign_teas CRT infinite loop where string scanning goes in wrong direction.
            const uint FLAG_DF = 1u << 10;  // Direction Flag at bit 10
            var eflags = _cpu.GetRegister("EFLAGS");
            if ((eflags & FLAG_DF) != 0)
            {
                _logger.LogWarning("[Syscall] Direction Flag (DF) was SET before API call - clearing per Win32 ABI (EFLAGS=0x{Eflags:X8})", eflags);
                _cpu.SetRegister("EFLAGS", eflags & ~FLAG_DF);
            }
            
            // Use async dispatcher to support async Win32 API implementations (required for WASM)
            var (success, ret, argBytes, callingConvention) = await _dispatcher!.TryInvokeAsync(dll, name, _cpu, _vm!, cancellationToken).ConfigureAwait(false);
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
                
                // CRITICAL: Win95/Win32 ABI requires direction flag to be CLEARED on exit from Win32 API functions
                // Even though we cleared it on entry, the API implementation might have used string operations
                // that could modify it. Per Win32 ABI, we must ensure DF=0 when returning to application code.
                var eflagsAfter = _cpu.GetRegister("EFLAGS");
                if ((eflagsAfter & FLAG_DF) != 0)
                {
                    _logger.LogWarning("[Syscall] Direction Flag (DF) was SET after API call - clearing per Win32 ABI (EFLAGS=0x{Eflags:X8})", eflagsAfter);
                    _cpu.SetRegister("EFLAGS", eflagsAfter & ~FLAG_DF);
                }
                
                // Restore ESP to original value so CPU can execute RET instructions naturally
                _cpu.SetRegister("ESP", originalEsp);
                
                // Restore callee-saved registers (with EBP validation to prevent corruption)
                CpuHelpers.RestoreCalleeSavedRegisters(_cpu, saved, skipInvalidEbp: true, memorySize: _vm!.Size);
                
                // Validate register state after syscall (helps diagnose corruption issues)
                // Uses logging level check instead of debug mode to allow selective enablement
                CpuHelpers.ValidateRegisterState(_cpu, saved, _vm!.Size, _logger, $"Syscall {dll}!{name}", LogLevel.Debug);
                
                // Log warning if calling convention is unknown to aid debugging
                if (callingConvention == null)
                {
                    _logger.LogWarning("[Syscall] Calling convention unknown for {Dll}!{Name}, defaulting to stdcall. This may cause stack corruption if function uses cdecl.",
                        dll, name);
                }
                
                _logger.LogDebug("[Syscall] Returned 0x{Ret:X8}, argBytes={ArgBytes}, calling convention={Convention}, CPU will execute RET naturally", 
                    ret, argBytes, callingConvention?.ToString() ?? "unknown");
                
                // Patch the import stub's RET instruction based on calling convention
                // For stdcall/fastcall/thiscall/pascal: Use RET imm16 to clean up stack (callee cleanup)
                // For cdecl: Use RET (0xC3) with no cleanup (caller cleanup)
                // Only patch if not already patched to avoid redundant memory writes
                if (!_patchedImportStubs.Contains(importStubAddr))
                {
                    var retInstrAddr = importStubAddr + 5;
                    var opcode = _vm!.Read8(retInstrAddr);
                    
                    // Determine if this calling convention requires callee stack cleanup
                    var requiresCalleeCleanup = callingConvention switch
                    {
                        Loader.CallingConvention.Stdcall => true,
                        Loader.CallingConvention.Fastcall => true,
                        Loader.CallingConvention.Thiscall => true,
                        Loader.CallingConvention.Pascal => true,
                        Loader.CallingConvention.Cdecl => false,
                        // Default to stdcall for unknown/null conventions (preserves existing behavior, warning logged above)
                        _ => true
                    };
                    
                    if (requiresCalleeCleanup)
                    {
                        // Stdcall-style functions: patch RET imm16 to clean up stack
                        // Note: Even functions with 0 arguments need consistent RET instruction
                        
                        if (argBytes > 0xFFFF)
                        {
                            // RET imm16 can only handle up to 65535 bytes (0xFFFF)
                            // This is an extremely rare edge case (64KB+ of arguments)
                            _logger.LogError("[Syscall] {Dll}!{Name} has argBytes={ArgBytes} which exceeds RET imm16 maximum (65535). " +
                                "Cannot patch import stub for stack cleanup. This will likely cause stack corruption.",
                                dll, name, argBytes);
                            _patchedImportStubs.Add(importStubAddr);
                        }
                        else if (opcode == RET_IMM16_OPCODE)
                        {
                            _vm!.Write8(retInstrAddr + 1, (byte)(argBytes & 0xFF));
                            _vm!.Write8(retInstrAddr + 2, (byte)((argBytes >> 8) & 0xFF));
                            _patchedImportStubs.Add(importStubAddr);
                            _logger.LogDebug("[Syscall] Patched RET at 0x{RetAddr:X8} with argBytes={ArgBytes} for {Convention} calling convention", 
                                retInstrAddr, argBytes, callingConvention?.ToString() ?? "unknown");
                        }
                        else if (opcode == RET_OPCODE)
                        {
                            // Already a plain RET, mark as patched to avoid redundant warnings
                            _patchedImportStubs.Add(importStubAddr);
                            _logger.LogDebug("[Syscall] RET at 0x{RetAddr:X8} already plain RET (0x{RetOpcode:X2}) for {Convention} calling convention", 
                                retInstrAddr, RET_OPCODE, callingConvention?.ToString() ?? "unknown");
                        }
                        else
                        {
                            _logger.LogWarning("[Syscall] Expected RET imm16 (0x{RetImm16Opcode:X2}) or RET (0x{RetOpcode:X2}) at 0x{RetAddr:X8} but found 0x{Opcode:X2}. Skipping patch.", 
                                RET_IMM16_OPCODE, RET_OPCODE, retInstrAddr, opcode);
                        }
                    }
                    else
                    {
                        // Cdecl functions: ensure RET has no cleanup (caller will clean up)
                        if (opcode == RET_IMM16_OPCODE)
                        {
                            // Change RET imm16 (0xC2 xx xx) to RET (0xC3) for cdecl
                            // We need to replace the entire 3-byte instruction with a single-byte RET
                            // followed by NOPs to avoid instruction boundary issues
                            _vm!.Write8(retInstrAddr, RET_OPCODE);      // RET
                            _vm!.Write8(retInstrAddr + 1, NOP_OPCODE);  // NOP
                            _vm!.Write8(retInstrAddr + 2, NOP_OPCODE);  // NOP
                            _patchedImportStubs.Add(importStubAddr);
                            _logger.LogDebug("[Syscall] Patched RET imm16 at 0x{RetAddr:X8} to RET+NOP+NOP (0x{RetOpcode:X2} 0x{NopOpcode:X2} 0x{NopOpcode:X2}) for cdecl calling convention (caller cleanup)", 
                                retInstrAddr, RET_OPCODE, NOP_OPCODE, NOP_OPCODE);
                        }
                        else if (opcode == RET_OPCODE)
                        {
                            // Already a plain RET, mark as patched
                            _patchedImportStubs.Add(importStubAddr);
                            _logger.LogDebug("[Syscall] RET at 0x{RetAddr:X8} already plain RET (0x{RetOpcode:X2}) for cdecl calling convention", 
                                retInstrAddr, RET_OPCODE);
                        }
                        else
                        {
                            _logger.LogWarning("[Syscall] Expected RET (0x{RetOpcode:X2}) or RET imm16 (0x{RetImm16Opcode:X2}) at 0x{RetAddr:X8} but found 0x{Opcode:X2}. Skipping patch.", 
                                RET_OPCODE, RET_IMM16_OPCODE, retInstrAddr, opcode);
                        }
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
                if (restoredEsp < MemoryRegions.MinValidUserAddress)
                {
                    _logger.LogError("[Syscall] ESP=0x{Esp:X8} after syscall return is suspiciously low. This indicates possible stack corruption.", restoredEsp);
                }
                
                // Log CPU state after syscall for debugging
                var eax = _cpu.GetRegister("EAX");
                _logger.LogDebug("[Syscall] CPU state after {Dll}!{Name}: EAX=0x{Eax:X8} ESP=0x{Esp:X8}", dll, name, eax, restoredEsp);
                
                // Enhanced diagnostics: Validate stack contents after syscall to catch potential corruption early
                // This helps diagnose issues where a function returns successfully but leaves corrupted data on the stack
                // that will cause problems later when the caller tries to use it
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    try
                    {
                        // Check the stack region that will be used after RET cleanup
                        // After the import stub executes RET with cleanup, ESP will advance past the arguments
                        var futureEsp = restoredEsp + 4 + (uint)argBytes; // After dispatcher RET + import stub RET cleanup
                        
                        // Read a few DWORDs from the future stack position to check for suspicious values
                        var stackDump = new System.Text.StringBuilder();
                        stackDump.Append($"\n[Syscall] Stack validation after {dll}!{name}:");
                        
                        for (int offset = -8; offset <= 16; offset += 4)
                        {
                            var addr = (uint)(futureEsp + offset);
                            if (addr >= MemoryRegions.MinValidUserAddress && addr < _vm!.Size - 4) // Validate address is in reasonable range
                            {
                                var val = _vm!.Read32(addr);
                                var marker = offset == 0 ? " <-- Future ESP" : "";
                                stackDump.Append($"\n  [ESP+{offset:+0;-0;+0}] = 0x{addr:X8}: 0x{val:X8}{marker}");
                            }
                        }
                        
                        _logger.LogDebug("{StackDump}", stackDump.ToString());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "[Syscall] Failed to perform stack validation");
                    }
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

    /// <summary>
    /// Handles DOS interrupt (INT 21h) for Win16 NE executables and DOS programs.
    /// </summary>
    private async Task HandleDosInterruptAsync(CancellationToken cancellationToken = default)
    {
        // Advance EIP past the INT 0x21 instruction (2 bytes: CD 21)
        // The JitCpu INT handler resets EIP to point AT the INT instruction for us to handle
        // We need to advance it so execution continues after the INT
        var eipAtInt = _cpu!.GetEip();
        _cpu.SetEip(eipAtInt + 2); // INT 0x21 is 2 bytes
        _logger.LogDebug("[DOS INT 21h] Advanced EIP past INT 0x21: 0x{EipBefore:X8} -> 0x{EipAfter:X8}", eipAtInt, _cpu.GetEip());
        
        // DOS services are accessed via INT 21h with function number in AH
        var ah = (_cpu!.GetRegister("EAX") >> 8) & 0xFF;
        var al = _cpu.GetRegister("EAX") & 0xFF;
        var bx = _cpu.GetRegister("EBX") & 0xFFFF;
        var cx = _cpu.GetRegister("ECX") & 0xFFFF;
        var dx = _cpu.GetRegister("EDX") & 0xFFFF;

        _logger.LogDebug("[DOS INT 21h] Function AH=0x{Ah:X2}, AL=0x{Al:X2}", ah, al);

        // Implement common DOS functions
        switch ((DosFunction)ah)
        {
            case DosFunction.Terminate:
                _logger.LogInformation("[DOS INT 21h] Program termination requested (AH=0x00)");
                _stopRequested = true;
                break;

            case DosFunction.CharInputWithEcho:
                {
                    _logger.LogDebug("[DOS INT 21h] Read character from stdin (AH=0x01)");
                    // Return a dummy character (space) for now
                    _cpu.SetRegister("EAX", (_cpu.GetRegister("EAX") & 0xFFFFFF00) | DOS_SPACE_CHAR);
                }
                break;

            case DosFunction.CharOutput:
                {
                    var dl = (_cpu.GetRegister("EDX") & 0xFF);
                    var ch = (char)dl;
                    _env!.WriteToStdOutput(ch.ToString());
                    _logger.LogDebug("[DOS INT 21h] Print character: '{Char}' (0x{Dl:X2})", ch, dl);
                }
                break;

            case DosFunction.DirectConsoleIO:
                {
                    var dl = (_cpu.GetRegister("EDX") & 0xFF);
                    if (dl == DOS_INPUT_READY)
                    {
                        // Input - return dummy character or 0 if no input available
                        _logger.LogDebug("[DOS INT 21h] Direct console input (AH=0x06)");
                        _cpu.SetRegister("EAX", (_cpu.GetRegister("EAX") & 0xFFFFFF00) | DOS_NO_INPUT);
                    }
                    else
                    {
                        // Output
                        var ch = (char)dl;
                        _env!.WriteToStdOutput(ch.ToString());
                        _logger.LogDebug("[DOS INT 21h] Direct console output: '{Char}'", ch);
                    }
                }
                break;

            case DosFunction.DirectCharInputNoEcho:
            case DosFunction.CharInputNoEcho:
                {
                    _logger.LogDebug("[DOS INT 21h] Read character without echo (AH=0x{Ah:X2})", ah);
                    // Return a dummy character (space)
                    _cpu.SetRegister("EAX", (_cpu.GetRegister("EAX") & 0xFFFFFF00) | DOS_SPACE_CHAR);
                }
                break;

            case DosFunction.WriteString:
                {
                    try
                    {
                        var sb = new System.Text.StringBuilder();
                        var offset = 0u;
                        while (true)
                        {
                            var ch = (char)_vm!.Read8(dx + offset);
                            if (ch == (char)DOS_STRING_TERMINATOR) break;
                            if (offset > MAX_DOS_STRING_LENGTH) break; // Safety limit
                            sb.Append(ch);
                            offset++;
                        }
                        var text = sb.ToString();
                        _env!.WriteToStdOutput(text);
                        _logger.LogDebug("[DOS INT 21h] Print string: {Text}", text);
                        // Return DL (last character) in AL
                        _cpu.SetRegister("EAX", (_cpu.GetRegister("EAX") & 0xFFFFFF00) | DOS_STRING_TERMINATOR);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[DOS INT 21h] Failed to read string at address 0x{Address:X8}", dx);
                    }
                }
                break;

            case DosFunction.BufferedInput:
                {
                    _logger.LogDebug("[DOS INT 21h] Buffered input (AH=0x0A) - not fully implemented");
                    // DS:DX points to buffer: first byte = max chars, second byte = actual chars read
                    // For now, just return empty input
                    if (dx != 0)
                    {
                        _vm!.Write8(dx + 1, 0); // No characters read
                    }
                }
                break;

            case DosFunction.CheckStdinStatus:
                {
                    _logger.LogDebug("[DOS INT 21h] Check stdin status (AH=0x0B)");
                    // Return 0 = no character available, 0xFF = character available
                    _cpu.SetRegister("EAX", (_cpu.GetRegister("EAX") & 0xFFFFFF00) | DOS_NO_INPUT);
                }
                break;

            case DosFunction.GetCurrentDrive:
                {
                    _logger.LogDebug("[DOS INT 21h] Get current drive (AH=0x19)");
                    // Return drive 2 (C:) - drives are 0=A, 1=B, 2=C, etc.
                    _cpu.SetRegister("EAX", (_cpu.GetRegister("EAX") & 0xFFFFFF00) | DOS_DRIVE_C);
                }
                break;

            case DosFunction.SetInterruptVector:
                _logger.LogDebug("[DOS INT 21h] Set interrupt vector AL=0x{Al:X2} (ignored)", al);
                break;

            case DosFunction.GetSystemDate:
                {
                    var now = DateTime.Now;
                    _logger.LogDebug("[DOS INT 21h] Get system date (AH=0x2A): {Date}", now.ToShortDateString());
                    // CX = year, DH = month, DL = day, AL = day of week (0=Sunday)
                    _cpu.SetRegister("ECX", (_cpu.GetRegister("ECX") & 0xFFFF0000) | (uint)now.Year);
                    _cpu.SetRegister("EDX", (_cpu.GetRegister("EDX") & 0xFFFF0000) | ((uint)now.Month << 8) | (uint)now.Day);
                    _cpu.SetRegister("EAX", (_cpu.GetRegister("EAX") & 0xFFFFFF00) | (uint)now.DayOfWeek);
                }
                break;

            case DosFunction.SetSystemDate:
                {
                    var year = cx;
                    var month = (dx >> 8) & 0xFF;
                    var day = dx & 0xFF;
                    _logger.LogDebug("[DOS INT 21h] Set system date (AH=0x2B): {Year}-{Month:D2}-{Day:D2} (ignored)", year, month, day);
                    // Return AL=0 for success (but we don't actually set it)
                    _cpu.SetRegister("EAX", (_cpu.GetRegister("EAX") & 0xFFFFFF00) | 0x00);
                }
                break;

            case DosFunction.GetSystemTime:
                {
                    var now = DateTime.Now;
                    _logger.LogDebug("[DOS INT 21h] Get system time (AH=0x2C): {Time}", now.ToLongTimeString());
                    // CH = hour, CL = minute, DH = second, DL = hundredths
                    _cpu.SetRegister("ECX", (_cpu.GetRegister("ECX") & 0xFFFF0000) | ((uint)now.Hour << 8) | (uint)now.Minute);
                    _cpu.SetRegister("EDX", (_cpu.GetRegister("EDX") & 0xFFFF0000) | ((uint)now.Second << 8) | (uint)(now.Millisecond / 10));
                }
                break;

            case DosFunction.SetSystemTime:
                {
                    var hour = (cx >> 8) & 0xFF;
                    var minute = cx & 0xFF;
                    var second = (dx >> 8) & 0xFF;
                    _logger.LogDebug("[DOS INT 21h] Set system time (AH=0x2D): {Hour:D2}:{Minute:D2}:{Second:D2} (ignored)", hour, minute, second);
                    // Return AL=0 for success
                    _cpu.SetRegister("EAX", (_cpu.GetRegister("EAX") & 0xFFFFFF00) | 0x00);
                }
                break;

            case DosFunction.GetDosVersion:
                {
                    _logger.LogDebug("[DOS INT 21h] Get DOS version (AH=0x30)");
                    // Return version 6.22 (DOS 6.22): AL=major (6), AH=minor (22)
                    _cpu.SetRegister("EAX", (_cpu.GetRegister("EAX") & 0xFFFF0000) | DOS_VERSION_6_22);
                    // BH = 0xFF (DOS is in HMA), BL:CX = 0 (serial number)
                    _cpu.SetRegister("EBX", (_cpu.GetRegister("EBX") & 0xFFFF0000) | 0xFF00);
                    _cpu.SetRegister("ECX", 0);
                }
                break;

            case DosFunction.GetSetCtrlBreak:
                {
                    if (al == 0x00)
                    {
                        // Get Ctrl-Break flag
                        _logger.LogDebug("[DOS INT 21h] Get Ctrl-Break flag (AH=0x33, AL=0x00)");
                        _cpu.SetRegister("EDX", (_cpu.GetRegister("EDX") & 0xFFFFFF00) | 0x01); // Enabled
                    }
                    else if (al == 0x01)
                    {
                        // Set Ctrl-Break flag
                        _logger.LogDebug("[DOS INT 21h] Set Ctrl-Break flag (AH=0x33, AL=0x01, DL={Dl})", dx & 0xFF);
                    }
                }
                break;

            case DosFunction.GetInterruptVector:
                {
                    _logger.LogDebug("[DOS INT 21h] Get interrupt vector AL=0x{Al:X2} (returning dummy)", al);
                    _cpu.SetRegister("EBX", 0x0000);
                    // ES segment register not accessible through ICpu interface, so we skip setting it
                }
                break;

            case DosFunction.CreateFile:
                {
                    var filename = Win32.MemoryHelpers.ReadNullTerminatedString(_vm!, dx, _logger, maxLength: 256);
                    _logger.LogDebug("[DOS INT 21h] Create file: {Filename} (AH=0x3C)", filename);
                    // Return dummy file handle in AX
                    _cpu.SetRegister("EAX", (_cpu.GetRegister("EAX") & 0xFFFF0000) | DOS_DUMMY_FILE_HANDLE);
                }
                break;

            case DosFunction.OpenFile:
                {
                    var filename = Win32.MemoryHelpers.ReadNullTerminatedString(_vm!, dx, _logger, maxLength: 256);
                    var accessMode = al & 0x03; // 0=read, 1=write, 2=read/write
                    _logger.LogDebug("[DOS INT 21h] Open file: {Filename}, mode={Mode} (AH=0x3D)", filename, accessMode);
                    // Return dummy file handle in AX
                    _cpu.SetRegister("EAX", (_cpu.GetRegister("EAX") & 0xFFFF0000) | DOS_DUMMY_FILE_HANDLE);
                }
                break;

            case DosFunction.CloseFile:
                {
                    var handle = bx;
                    _logger.LogDebug("[DOS INT 21h] Close file handle: 0x{Handle:X4} (AH=0x3E)", handle);
                    // Return success (carry flag clear, but we can't set it)
                }
                break;

            case DosFunction.ReadFile:
                {
                    var handle = bx;
                    var count = cx;
                    var buffer = dx;
                    _logger.LogDebug("[DOS INT 21h] Read from file handle 0x{Handle:X4}, {Count} bytes to 0x{Buffer:X8} (AH=0x3F)", handle, count, buffer);
                    // Return 0 bytes read (EOF)
                    _cpu.SetRegister("EAX", (_cpu.GetRegister("EAX") & 0xFFFF0000) | 0x0000);
                }
                break;

            case DosFunction.WriteFile:
                {
                    var handle = bx;
                    var count = cx;
                    var buffer = dx;
                    _logger.LogDebug("[DOS INT 21h] Write to handle 0x{Handle:X4}, {Count} bytes from 0x{Buffer:X8} (AH=0x40)", handle, count, buffer);
                    
                    // If handle is stdout (1) or stderr (2), write to console
                    if (handle == DOS_STDOUT_HANDLE || handle == DOS_STDERR_HANDLE)
                    {
                        try
                        {
                            var data = new byte[count];
                            for (var i = 0; i < count; i++)
                            {
                                data[i] = _vm!.Read8(buffer + (uint)i);
                            }
                            var text = System.Text.Encoding.ASCII.GetString(data);
                            _env!.WriteToStdOutput(text);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "[DOS INT 21h] Failed to write to console");
                        }
                    }
                    
                    // Return bytes written in AX
                    _cpu.SetRegister("EAX", (_cpu.GetRegister("EAX") & 0xFFFF0000) | (uint)count);
                }
                break;

            case DosFunction.SeekFile:
                {
                    var handle = bx;
                    var method = al; // 0=from start, 1=from current, 2=from end
                    var offset = ((uint)cx << 16) | dx;
                    _logger.LogDebug("[DOS INT 21h] Seek in file handle 0x{Handle:X4}, method={Method}, offset=0x{Offset:X8} (AH=0x42)", handle, method, offset);
                    // Return new position in DX:AX (just return the offset)
                    _cpu.SetRegister("EDX", (_cpu.GetRegister("EDX") & 0xFFFF0000) | ((offset >> 16) & 0xFFFF));
                    _cpu.SetRegister("EAX", (_cpu.GetRegister("EAX") & 0xFFFF0000) | (offset & 0xFFFF));
                }
                break;

            case DosFunction.GetSetFileAttributes:
                {
                    var filename = Win32.MemoryHelpers.ReadNullTerminatedString(_vm!, dx, _logger, maxLength: 256);
                    if (al == 0x00)
                    {
                        // Get attributes
                        _logger.LogDebug("[DOS INT 21h] Get file attributes: {Filename} (AH=0x43, AL=0x00)", filename);
                        // Return normal file attribute in CX
                        _cpu.SetRegister("ECX", (_cpu.GetRegister("ECX") & 0xFFFF0000) | DOS_FILE_ATTR_ARCHIVE);
                    }
                    else if (al == 0x01)
                    {
                        // Set attributes
                        _logger.LogDebug("[DOS INT 21h] Set file attributes: {Filename}, attrs=0x{Attrs:X4} (AH=0x43, AL=0x01)", filename, cx);
                    }
                }
                break;

            case DosFunction.GetCurrentDirectory:
                {
                    var drive = dx & 0xFF; // 0=default, 1=A, 2=B, 3=C, etc.
                    var buffer = (_cpu.GetRegister("ESI") & 0xFFFF); // DS:SI points to buffer
                    _logger.LogDebug("[DOS INT 21h] Get current directory, drive={Drive} (AH=0x47)", drive);
                    
                    // Write current directory to buffer (e.g., "WINDOWS\SYSTEM32")
                    var path = _env!.CurrentDirectory.TrimStart('C', ':', '\\');
                    if (string.IsNullOrEmpty(path)) path = "";
                    
                    try
                    {
                        for (var i = 0; i < path.Length && i < DOS_MAX_CURRENT_DIR_LENGTH; i++)
                        {
                            _vm!.Write8(buffer + (uint)i, (byte)path[i]);
                        }
                        _vm!.Write8(buffer + (uint)path.Length, 0); // Null terminator
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[DOS INT 21h] Failed to write current directory");
                    }
                }
                break;

            case DosFunction.AllocateMemory:
                {
                    var paragraphs = bx; // Size in 16-byte paragraphs
                    var bytes = paragraphs * DOS_PARAGRAPH_SIZE;
                    _logger.LogDebug("[DOS INT 21h] Allocate memory: {Paragraphs} paragraphs ({Bytes} bytes) (AH=0x48)", paragraphs, bytes);
                    
                    // Use SimpleAlloc to allocate memory
                    var address = _env!.SimpleAlloc(bytes);
                    var segment = address >> 4; // Convert to segment
                    
                    // Return segment in AX
                    _cpu.SetRegister("EAX", (_cpu.GetRegister("EAX") & 0xFFFF0000) | (segment & 0xFFFF));
                }
                break;

            case DosFunction.FreeMemory:
                {
                    var segment = (_cpu.GetRegister("ES") & 0xFFFF); // ES = segment to free
                    _logger.LogDebug("[DOS INT 21h] Free memory: segment=0x{Segment:X4} (AH=0x49)", segment);
                    // We don't actually free it since we don't have a proper memory manager
                }
                break;

            case DosFunction.ResizeMemory:
                {
                    var segment = (_cpu.GetRegister("ES") & 0xFFFF);
                    var newParagraphs = bx;
                    _logger.LogDebug("[DOS INT 21h] Resize memory: segment=0x{Segment:X4}, new size={NewSize} paragraphs (AH=0x4A)", segment, newParagraphs);
                    // Return success
                }
                break;

            case DosFunction.TerminateWithReturnCode:
                {
                    var exitCode = al;
                    _logger.LogInformation("[DOS INT 21h] Program termination with exit code {ExitCode} (AH=0x4C)", exitCode);
                    _stopRequested = true;
                }
                break;

            case DosFunction.GetReturnCode:
                {
                    _logger.LogDebug("[DOS INT 21h] Get return code (AH=0x4D)");
                    // Return 0 in AX
                    _cpu.SetRegister("EAX", (_cpu.GetRegister("EAX") & 0xFFFF0000) | 0x0000);
                }
                break;

            default:
                _logger.LogWarning("[DOS INT 21h] Unimplemented function AH=0x{Ah:X2}", ah);
                // Return error value in AX
                _cpu.SetRegister("EAX", DOS_ERROR_INVALID_FUNCTION);
                break;
        }

        await Task.CompletedTask;
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
    /// Returns the heap base address for memory allocation.
    /// The heap base is calculated as the image base address plus the image size,
    /// aligned to a 64KB boundary (standard Windows allocation granularity).
    /// This ensures the heap region starts after the loaded PE image to avoid
    /// false positives in heap execution detection.
    /// The PE header's SizeOfHeapReserve value is available in LoadedImage but not used
    /// to determine heap placement - it's available for the memory allocator to manage heap growth.
    /// </summary>
    /// <param name="image">The loaded image containing base address and size information</param>
    /// <returns>The heap base address to use for memory allocation</returns>
    private static uint CalculateHeapBase(LoadedImage image)
    {
        // Calculate heap base as image base + image size
        var heapBase = image.BaseAddress + image.ImageSize;
        
        // Align to 64KB boundary (0x10000) - standard Windows allocation granularity
        // This ensures proper alignment for VirtualAlloc and similar operations
        const uint ALLOCATION_GRANULARITY = 0x10000;
        heapBase = (heapBase + ALLOCATION_GRANULARITY - 1) & ~(ALLOCATION_GRANULARITY - 1);
        
        return heapBase;
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

        // Capture the cancellation token to avoid race conditions when StopEventProcessing() sets _eventProcessingCts to null
        var cancellationToken = _eventProcessingCts.Token;

        // Start the event processing task
        _eventProcessingTask = Task.Run(async () =>
        {
            LogDebug("[EventProcessing] Starting UI event processing loop");

            try
            {
                while (!cancellationToken.IsCancellationRequested && !_stopRequested)
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

                    // Process Win32 timers (SetTimer API)
                    try
                    {
                        if (_dispatcher != null && _dispatcher.TryGetModule("USER32.DLL", out var user32Module) && user32Module is User32Module user32)
                        {
                            await user32.ProcessTimersAsync(cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[EventProcessing] Error processing timers");
                    }

                    // Small delay to avoid busy-waiting (60 FPS event processing)
                    await Task.Delay(16, cancellationToken);
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
        }, cancellationToken);
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