using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Diagnostics;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.Rendering;
using Win32Emu.Threading;
using Win32Emu.Win32.COM;
using Win32Emu.Win32.Messaging;
using Win32Emu.VirtualFileSystem;

namespace Win32Emu.Win32;

public class ProcessEnvironment
{
	private readonly IEmulatorHost? _host;
	private readonly ILogger _logger;
	private uint _allocPtr;
	private string _currentDirectory = @"C:\"; // Default to C:\ root

	// Free-list allocator for VirtualAlloc/VirtualFree
	private readonly List<MemoryBlock> _freeList = new();
	private readonly Dictionary<uint, MemoryBlock> _allocatedBlocks = new();

	// COM vtable dispatcher
	private readonly ComVtableDispatcher? _comDispatcher;
	
	// Message dispatcher for event-driven messaging
	private readonly MessageDispatcher _messageDispatcher;
	
	// API call tracer for diagnostics
	private ApiCallTracer? _apiCallTracer;
	
	// Track subscribed backends to prevent duplicate event subscriptions
	private readonly HashSet<IRenderingBackend> _subscribedRenderingBackends = new();
	private readonly HashSet<IInputBackend> _subscribedInputBackends = new();

	// Virtual File System

	// Threading infrastructure

	// Expose VirtualMemory for use by Win32 API implementations
	public VirtualMemory Memory { get; }

	// Expose ThreadScheduler for use by Emulator
	public ThreadScheduler? ThreadScheduler { get; }

	// Expose SynchronizationManager for use by Win32 APIs
	public SynchronizationManager? SynchronizationManager { get; }
	
	// Expose MessageDispatcher for use by Win32 modules
	public MessageDispatcher MessageDispatcher => _messageDispatcher;
	
	// Expose API call tracer for diagnostic purposes
	/// <summary>
	/// Gets the API call tracer for this process environment, if enabled.
	/// </summary>
	public ApiCallTracer? ApiCallTracer => _apiCallTracer;

	public ProcessEnvironment(VirtualMemory vm, uint heapBase = 0x01000000, IEmulatorHost? host = null, ILogger? logger = null)
	{
		Memory = vm;
		_host = host;
		_logger = logger ?? NullLogger.Instance;
		_allocPtr = heapBase;
		_comDispatcher = new ComVtableDispatcher(this, _logger);
		_messageDispatcher = new MessageDispatcher(_logger);
		ThreadScheduler = new ThreadScheduler(_logger);
		SynchronizationManager = new SynchronizationManager(_logger);
		
		// Pre-register standard Windows control classes
		RegisterStandardControlClasses();
	}
	
	// COM vtable dispatcher access
	public ComVtableDispatcher ComDispatcher => _comDispatcher ?? throw new InvalidOperationException("COM dispatcher not initialized");

	// Host interface access
	/// <summary>
	/// Gets the emulator host interface for GUI callbacks
	/// </summary>
	public IEmulatorHost? Host => _host;

	// Virtual File System access
	/// <summary>
	/// Gets the current virtual file system instance for this process environment.
	/// </summary>
	/// <remarks>
	/// Returns <c>null</c> if the virtual file system has not been initialized.
	/// </remarks>
	public IVirtualFileSystem? VirtualFileSystem { get; private set; }

	/// <summary>
	/// Initializes the virtual file system with the specified base directory.
	/// </summary>
	/// <param name="baseDirectory">Base directory containing game files (read-only)</param>
	/// <param name="overlayDirectory">Optional overlay directory for writable files. If null, a temporary directory is used.</param>
	public void InitializeVirtualFileSystem(string baseDirectory, string? overlayDirectory = null)
	{
		VirtualFileSystem = new LayeredVirtualFileSystem(baseDirectory, overlayDirectory, _logger);
		_logger.LogInformation("[ProcessEnv] Virtual File System initialized with base: {BaseDirectory}", baseDirectory);
		VirtualizeExecutablePath();
	}

	/// <summary>
	/// Initializes the virtual file system with a virtual disk file (VHD/VMDK/VHDX/ISO).
	/// </summary>
	/// <param name="diskPath">Path to the virtual disk file</param>
	public void InitializeVirtualFileSystemWithDisk(string diskPath)
	{
		VirtualFileSystem = new DiskVirtualFileSystem(diskPath, _logger);
		_logger.LogInformation("[ProcessEnv] Virtual File System initialized with disk: {DiskPath}", diskPath);
		VirtualizeExecutablePath();
	}

	/// <summary>
	/// Initializes the virtual file system with an existing IVirtualFileSystem instance.
	/// </summary>
	/// <param name="vfs">The virtual file system instance to use</param>
	public void InitializeVirtualFileSystem(IVirtualFileSystem vfs)
	{
		VirtualFileSystem = vfs;
		_logger.LogInformation("[ProcessEnv] Virtual File System initialized with custom instance");
		VirtualizeExecutablePath();
	}
	
	/// <summary>
	/// Enables comprehensive API call tracing for diagnostic purposes.
	/// </summary>
	/// <param name="outputPath">Optional path to write trace log file. If null, logs only to console.</param>
	/// <param name="enableDetailedParameters">Whether to log detailed parameter values (default: true)</param>
	/// <param name="enableExecutionFlow">Whether to log execution flow markers (default: false)</param>
	public void EnableApiTracing(string? outputPath = null, bool enableDetailedParameters = true, bool enableExecutionFlow = false)
	{
		if (_apiCallTracer != null)
		{
			_logger.LogWarning("[ProcessEnv] API tracing already enabled");
			return;
		}

		_apiCallTracer = new ApiCallTracer(
			_logger,
			enableTracing: true,
			enableDetailedParameters: enableDetailedParameters,
			enableExecutionFlow: enableExecutionFlow,
			outputPath: outputPath);

		_logger.LogInformation("[ProcessEnv] API call tracing enabled (output: {Output})", 
			outputPath ?? "console only");
	}
	
	/// <summary>
	/// Disables API call tracing and generates a final diagnostic report.
	/// </summary>
	/// <returns>Diagnostic report summarizing all traced calls</returns>
	public string? DisableApiTracing()
	{
		if (_apiCallTracer == null)
		{
			return null;
		}

		var report = _apiCallTracer.GenerateDiagnosticReport();
		_apiCallTracer.Dispose();
		_apiCallTracer = null;

		_logger.LogInformation("[ProcessEnv] API call tracing disabled");
		return report;
	}

	private void VirtualizeExecutablePath()
	{
		// If executable path is already set, virtualize it to Windows-style path
		if (string.IsNullOrEmpty(ExecutablePath))
		{
			return;
		}

		var virtualizedPath = VirtualFileSystem.ToWindowsPath(ExecutablePath);
		if (virtualizedPath == ExecutablePath)
		{
			return;
		}

		_logger.LogInformation("[ProcessEnv] Virtualizing executable path: {Original} -> {Virtualized}", 
			ExecutablePath, virtualizedPath);
				
		// Update the executable path and module file name
		ExecutablePath = virtualizedPath;
		ModuleFileNamePtr = WriteAnsiString(virtualizedPath + '\0');
		ModuleFileNameLength = (uint)virtualizedPath.Length;
		
		// Update current directory to match the virtualized executable directory
		var directory = Path.GetDirectoryName(virtualizedPath);
		if (!string.IsNullOrEmpty(directory))
		{
			CurrentDirectory = directory;
			_logger.LogInformation("[ProcessEnv] Updated current directory to: {CurrentDirectory}", CurrentDirectory);
		}
				
		// Also update command line if it was already set
		if (CommandLinePtr == 0)
		{
			return;
		}

		// Re-read the old command line to extract args
		var oldCmdLine = ReadAnsiString(CommandLinePtr);
		// Parse to extract args (skip the first quoted part which is the exe path)
		var args = new List<string>();
		var inQuote = false;
		var current = new System.Text.StringBuilder();
		var skipFirst = true;
					
		foreach (var ch in oldCmdLine)
		{
			switch (ch)
			{
				case '"':
				{
					inQuote = !inQuote;
					if (inQuote || !skipFirst)
					{
						continue;
					}

					skipFirst = false;
					current.Clear();
					break;
				}
				case ' ' when !inQuote:
				{
					if (current.Length <= 0 || skipFirst)
					{
						continue;
					}

					args.Add(current.ToString());
					current.Clear();
					break;
				}
				default:
				{
					if (!skipFirst)
					{
						current.Append(ch);
					}

					break;
				}
			}
		}
					
		if (current.Length > 0 && !skipFirst)
		{
			args.Add(current.ToString());
		}
					
		// Rebuild command line with virtualized path
		var newCmdLine = args.Count > 0 
			? $"\"{virtualizedPath}\" {string.Join(" ", args)}"
			: $"\"{virtualizedPath}\"";
		CommandLinePtr = WriteAnsiString(newCmdLine + '\0');
	}

	// Backends for audio and input
	public IAudioBackend? AudioBackend { get; set; }
	public IInputBackend? InputBackend { get; set; }

	public uint CommandLinePtr { get; private set; }
	public uint CommandLinePtrW { get; set; }
	public uint ModuleFileNamePtr { get; private set; }
	public uint ModuleFileNameLength { get; private set; }
	public bool ExitRequested { get; private set; }

	public string ExecutablePath { get; private set; } = string.Empty;

	public string CurrentDirectory
	{
		get => _currentDirectory;
		set => _currentDirectory = value ?? @"C:\";
	}

	// Console state
	public bool HasConsole { get; private set; }

	// Default standard handles (NULL for GUI apps without console)
	// Console apps would set these to actual handles via AllocConsole/AttachConsole
	public uint StdInputHandle { get; set; } // NULL - no console by default
	public uint StdOutputHandle { get; set; } // NULL - no console by default
	public uint StdErrorHandle { get; set; } // NULL - no console by default

	// Simple handle table for host resources (files etc.)
	private readonly Dictionary<uint, object> _handles = new();
	private uint _nextHandle = 0x00001000; // avoid low values used as sentinels

	// Loaded modules tracking
	private readonly Dictionary<string, uint> _loadedModules = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<string, LoadedImage> _loadedImages = new(StringComparer.OrdinalIgnoreCase);
	private uint _nextModuleHandle = 0x10000000;
	private string? _mainExecutableName; // Track the main executable's name for reliable lookup

	// Syscall dispatcher address - shared with PeImageLoader
	private const uint SYSCALL_DISPATCHER_ADDRESS = 0x0E000000;
	
	// Synthetic exports now use syscall mechanism (CALL/RET stubs) starting at 0x0F000000 range
	// They are stored in the main executable's ImportAddressMap for unified handling
	private uint _nextSyntheticExport = 0x0F800000; // Synthetic export stub base address (distinct from import stubs at 0x0F000000)

	// Standard control window procedure marker address range
	// Window procedures in this range (0x0D000000 - 0x0DFFFFFF) are markers for standard controls
	// These addresses signal to User32Module to route messages through StandardControlHandler
	public const uint STANDARD_CONTROL_WNDPROC_BASE = 0x0D000000;
	public const uint STANDARD_CONTROL_WNDPROC_END = 0x0DFFFFFF;

	// Window management
	private readonly Dictionary<uint, WindowInfo> _windows = new();
	private readonly Dictionary<string, WindowClassInfo> _windowClasses = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<uint, string> _atomToClassName = new(); // Maps atoms to class names
	private uint _nextWindowHandle = 0x00010000; // Window handles typically start low
	
	// Window property storage for SetWindowLongA/GetWindowLongA
	// Key: (hwnd, index), Value: property value
	private readonly Dictionary<(uint, int), uint> _windowProperties = new();

	// Registered window messages (RegisterWindowMessageA)
	// Registered messages are allocated in the range 0xC000-0xFFFF
	private readonly Dictionary<string, uint> _registeredMessages = new(StringComparer.OrdinalIgnoreCase);
	private uint _nextRegisteredMessage = 0xC000;

	// Message queue management
	private bool _hasQuitMessage;
	private int _quitExitCode;

	// Dialog state management
	private readonly Dictionary<uint, DialogState> _dialogStates = new();

	// Message queue with Channels
	private readonly Channel<QueuedMessage> _messageQueue = Channel.CreateUnbounded<QueuedMessage>();
	
	// Message queue wait token - used by thread scheduler to identify threads waiting for messages
	private readonly object _messageQueueWaitToken = new object();
	
	// Message structure for queueing
	public record struct QueuedMessage(
		uint Hwnd,
		uint Message,
		uint WParam,
		uint LParam,
		uint Time,
		uint PtX,
		uint PtY
	);

	// Environment variables (emulated, not from system)
	private readonly Dictionary<string, string> _environmentVariables = new();

	// Thread management
	private uint _nextThreadId = 1;
	private uint _currentThreadId = 1; // Main thread ID is always 1
	public uint TebAddress { get; private set; }

	// TLS (Thread Local Storage) support
	private readonly Dictionary<uint, Dictionary<uint, uint>> _threadLocalStorage = new(); // threadId -> (tlsIndex -> value)
	private readonly HashSet<uint> _allocatedTlsIndices = [];
	private uint _nextTlsIndex;

	// Virtual Registry support
	private readonly Dictionary<uint, VirtualRegistryKey> _registryKeys = new(); // handle -> key
	private uint _nextRegistryHandle = 0x80000000; // Registry handles typically use high values
	
	public class VirtualRegistryKey
	{
		public string Path { get; set; } = string.Empty;
		public Dictionary<string, object> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);
	}

	public void InitializeStrings(string exePath, string[] args)
	{
		Debug.Assert(exePath != null, nameof(exePath) + " != null");
		
		// If VFS is initialized, virtualize the executable path to Windows-style
		var effectivePath = exePath;
		if (VirtualFileSystem != null)
		{
			effectivePath = VirtualFileSystem.ToWindowsPath(exePath);
			if (effectivePath != exePath)
			{
				_logger.LogInformation("[ProcessEnv] Virtualizing executable path: {Original} -> {Virtualized}", 
					exePath, effectivePath);
			}
		}
		
		ExecutablePath = effectivePath;
		
		// Set current directory to the directory containing the executable
		// This ensures relative paths are resolved relative to the executable's location
		var directory = Path.GetDirectoryName(effectivePath);
		if (!string.IsNullOrEmpty(directory))
		{
			CurrentDirectory = directory;
			_logger.LogInformation("[ProcessEnv] Set current directory to: {CurrentDirectory}", CurrentDirectory);
		}
		
		// Build command line: quoted exe path + space + args (if any)
		var cmdLine = args.Length > 0 
			? $"\"{effectivePath}\" {string.Join(" ", args)}"
			: $"\"{effectivePath}\"";
		CommandLinePtr = WriteAnsiString(cmdLine + '\0');
		ModuleFileNamePtr = WriteAnsiString(effectivePath + '\0');
		ModuleFileNameLength = (uint)effectivePath.Length;

		// Initialize with some default environment variables
		InitializeDefaultEnvironmentVariables();
	}

	public void InitializeTebAndPeb(uint imageBaseAddress)
	{
		// Allocate memory for the TEB (Thread Environment Block)
		TebAddress = SimpleAlloc(0x1000); // Allocate 4KB for TEB
		MemZero(TebAddress, 0x1000);
		_logger.LogInformation("[ProcessEnv] TEB allocated at 0x{TebAddress:X8}", TebAddress);

		// The TEB contains a self-referential pointer at offset 0x18
		// This is the linear address of the TEB
		MemWrite32(TebAddress + 0x18, TebAddress);

		// Allocate a dummy PEB (Process Environment Block)
		var pebAddress = SimpleAlloc(0x1000);
		MemZero(pebAddress, 0x1000);
		_logger.LogInformation("[ProcessEnv] PEB allocated at 0x{PebAddress:X8}", pebAddress);
        
		// The TEB points to the PEB at offset 0x30
		MemWrite32(TebAddress + 0x30, pebAddress);
        
		// The PEB contains a pointer to itself at offset 0x0
		MemWrite32(pebAddress, pebAddress);
		
		// Populate minimal PEB fields
		MemWrite8(pebAddress + 0x2, 1); // BeingDebugged = TRUE
		MemWrite32(pebAddress + 0x8, imageBaseAddress); // ImageBaseAddress
	}

	private void InitializeDefaultEnvironmentVariables()
	{
		// Set some common Windows environment variables for emulation
		_environmentVariables["PATH"] = @"C:\WINDOWS\system32;C:\WINDOWS;C:\WINDOWS\System32\Wbem";
		_environmentVariables["WINDIR"] = @"C:\WINDOWS";
		_environmentVariables["SYSTEMROOT"] = @"C:\WINDOWS";
		_environmentVariables["TEMP"] = @"C:\TEMP";
		_environmentVariables["TMP"] = @"C:\TEMP";
		_environmentVariables["COMPUTERNAME"] = "WIN32EMU";
		_environmentVariables["USERNAME"] = "User";
		_environmentVariables["USERDOMAIN"] = "WIN32EMU";
	}

	public uint SimpleAlloc(uint size)
	{
		if (size == 0)
		{
			size = 1;
		}

		var addr = _allocPtr;
		_allocPtr = AlignUp(_allocPtr + size, 16);
		return addr;
	}

	public void RequestExit() => ExitRequested = true;

	/// <summary>
	/// Writes output to the standard output stream, notifying the host if available.
	/// </summary>
	public void WriteToStdOutput(byte[] data)
	{
		// Convert bytes to string (assuming ANSI/ASCII encoding)
		var text = Encoding.ASCII.GetString(data);
		
		// Log to console for debugging
		_logger.LogInformation("[ProcessEnvironment] StdOutput: {Text}", text);
		
		// Notify host if available (for GUI display)
		_host?.OnStdOutput(text);
	}

	// Guest memory helpers
	public uint WriteAnsiString(string s)
	{
		var bytes = Encoding.ASCII.GetBytes(s);
		var addr = SimpleAlloc((uint)bytes.Length);
		Memory.WriteBytes(addr, bytes);
		Diagnostics.Diagnostics.LogMemWrite(addr, bytes.Length, bytes);
		return addr;
	}

	public uint WriteUnicodeString(string s)
	{
		var bytes = Encoding.Unicode.GetBytes(s);
		var addr = SimpleAlloc((uint)bytes.Length);
		Memory.WriteBytes(addr, bytes);
		Diagnostics.Diagnostics.LogMemWrite(addr, bytes.Length, bytes);
		return addr;
	}

	public void WriteAnsiStringAt(uint addr, string s, bool nullTerminate = true)
	{
		var bytes = Encoding.ASCII.GetBytes(nullTerminate ? s + "\0" : s);
		Memory.WriteBytes(addr, bytes);
		Diagnostics.Diagnostics.LogMemWrite(addr, bytes.Length, bytes);
	}

	public string ReadAnsiString(uint addr)
	{
		var buf = new List<byte>();
		var p = addr;
		while (true)
		{
			var b = Memory.Read8(p++);
			if (b == 0)
			{
				break;
			}

			buf.Add(b);
		}

		var result = Encoding.ASCII.GetString(buf.ToArray());
		_logger.LogDebug("[ProcessEnv] ReadAnsiString addr=0x{Addr:X8} result='{Result}'", addr, result);
		return result;
	}

	public string ReadAnsiString(uint addr, int maxLength)
	{
		var buf = new byte[maxLength];
		for (var i = 0; i < maxLength; i++)
		{
			buf[i] = Memory.Read8(addr + (uint)i);
		}

		var result = Encoding.ASCII.GetString(buf);
		_logger.LogDebug("[ProcessEnv] ReadAnsiString addr=0x{Addr:X8} length={MaxLength} result='{Result}'", addr, maxLength, result);
		return result;
	}

	public string ReadUnicodeString(uint addr)
	{
		const int chunkSize = 256; // Read 256 bytes at a time
		var bytes = new List<byte>();
		var offset = 0u;
		
		while (true)
		{
			// Read a chunk of memory
			var chunk = new byte[chunkSize];
			for (var i = 0; i < chunkSize; i++)
			{
				chunk[i] = Memory.Read8(addr + offset + (uint)i);
			}
			
			// Find the null terminator (two consecutive zero bytes for Unicode)
			for (var i = 0; i < chunkSize - 1; i += 2)
			{
				if (chunk[i] == 0 && chunk[i + 1] == 0)
				{
					// Found null terminator, add remaining bytes and return
					for (var j = 0; j < i; j++)
					{
						bytes.Add(chunk[j]);
					}
					var result = Encoding.Unicode.GetString(bytes.ToArray());
					_logger.LogDebug("[ProcessEnv] ReadUnicodeString addr=0x{Addr:X8} result='{Result}'", addr, result);
					return result;
				}
			}
			
			// No null terminator found in this chunk, add all bytes and continue
			bytes.AddRange(chunk);
			offset += chunkSize;
			
			// Safety check to prevent infinite loops on malformed strings
			if (offset > 65536) // Max 64KB string
			{
				_logger.LogWarning("[ProcessEnv] ReadUnicodeString exceeded maximum length at addr=0x{Addr:X8}", addr);
				var safeResult = Encoding.Unicode.GetString(bytes.ToArray());
				return safeResult;
			}
		}
	}

	public uint GetEnvironmentStringsW()
	{
		var envBlock = new StringBuilder();
		
		// Add each environment variable as "NAME=VALUE\0"
		foreach (var kvp in _environmentVariables.OrderBy(x => x.Key))
		{
			envBlock.Append($"{kvp.Key}={kvp.Value}");
			envBlock.Append('\0'); // null terminate each string
		}
		
		// Add final null terminator for the block
		envBlock.Append('\0');
		
		// Convert to bytes and allocate memory
		var bytes = Encoding.Unicode.GetBytes(envBlock.ToString());
		var addr = SimpleAlloc((uint)bytes.Length);
		Memory.WriteBytes(addr, bytes);
		
		return addr;
	}

	/// <summary>
	/// Creates a Windows-format environment strings block in ANSI.
	/// Returns a pointer to a double-null-terminated block of null-terminated strings.
	/// Format: "VAR1=value1\0VAR2=value2\0...VARn=valuen\0\0"
	/// </summary>
	public uint GetEnvironmentStringsA()
	{
		var envBlock = new StringBuilder();
		
		// Add each environment variable as "NAME=VALUE\0"
		foreach (var kvp in _environmentVariables.OrderBy(x => x.Key))
		{
			envBlock.Append($"{kvp.Key}={kvp.Value}");
			envBlock.Append('\0'); // null terminate each string
		}
		
		// Add final null terminator for the block
		envBlock.Append('\0');
		
		// Convert to bytes and allocate memory
		var bytes = Encoding.ASCII.GetBytes(envBlock.ToString());
		var addr = SimpleAlloc((uint)bytes.Length);
		Memory.WriteBytes(addr, bytes);
		
		return addr;
	}

	/// <summary>
	/// Frees environment strings memory allocated by GetEnvironmentStringsW.
	/// In a real implementation, this would free the memory block, but since we use
	/// a simple allocator that doesn't support freeing, this is a no-op.
	/// Returns TRUE (1) to indicate success.
	/// </summary>
	public uint FreeEnvironmentStringsW(uint lpszEnvironmentBlock)
	{
		// In our simple memory model, we don't actually free memory
		// Just return success (TRUE)
		return 1;
	}

	/// <summary>
	/// Frees environment strings memory allocated by GetEnvironmentStringsA.
	/// In a real implementation, this would free the memory block, but since we use
	/// a simple allocator that doesn't support freeing, this is a no-op.
	/// Returns TRUE (1) to indicate success.
	/// </summary>
	public uint FreeEnvironmentStringsA(uint lpszEnvironmentBlock)
	{
		// In our simple memory model, we don't actually free memory
		// Just return success (TRUE)
		return 1;
	}

	/// <summary>
	/// Sets a virtualized environment variable.
	/// </summary>
	/// <param name="name">The name of the environment variable</param>
	/// <param name="value">The value to set, or null to delete the variable</param>
	public void SetEnvironmentVariable(string name, string? value)
	{
		if (value == null)
		{
			_environmentVariables.Remove(name);
			_logger.LogDebug("[ProcessEnv] SetEnvironmentVariable: Deleted '{Name}'", name);
		}
		else
		{
			_environmentVariables[name] = value;
			_logger.LogDebug("[ProcessEnv] SetEnvironmentVariable: Set '{Name}'='{Value}'", name, value);
		}
	}

	/// <summary>
	/// Gets a virtualized environment variable value.
	/// </summary>
	/// <param name="name">The name of the environment variable</param>
	/// <returns>The value of the environment variable, or null if not found</returns>
	public string? GetEnvironmentVariable(string name)
	{
		if (_environmentVariables.TryGetValue(name, out var value))
		{
			_logger.LogDebug("[ProcessEnv] GetEnvironmentVariable: '{Name}'='{Value}'", name, value);
			return value;
		}

		_logger.LogDebug("[ProcessEnv] GetEnvironmentVariable: '{Name}' not found", name);
		return null;
	}

	/// <summary>
	/// Write text to standard output via the host callback
	/// </summary>
	public void WriteToStdOutput(string text)
	{
		// Log to console for debugging
		_logger.LogInformation("[ProcessEnv] StdOutput: {Text}", text);
		
		// Notify host if available (for GUI display)
		_host?.OnStdOutput(text);
	}

	/// <summary>
	/// Write text to standard error via the host callback (currently same as stdout)
	/// </summary>
	public void WriteToStdError(string text)
	{
		// Log to console for debugging
		_logger.LogError("[ProcessEnv] StdOutput: {Text}", text);
		
		// For now, treat stderr the same as stdout
		_host?.OnStdOutput(text);
	}

	public byte[] MemReadBytes(uint addr, int count) => Memory.GetSpan(addr, count);
	public byte MemRead8(uint addr) => Memory.Read8(addr);
	public void MemWriteBytes(uint addr, ReadOnlySpan<byte> data)
	{
		Memory.WriteBytes(addr, data);
		try { Diagnostics.Diagnostics.LogMemWrite(addr, data.Length, data.ToArray()); } catch { }
	}
	public uint MemRead32(uint addr) => Memory.Read32(addr);
	public void MemWrite32(uint addr, uint value) => Memory.Write32(addr, value);
	public void MemWriteBytes(uint addr, byte[] bytes) => Memory.WriteBytes(addr, bytes);
	public void MemWrite16(uint addr, ushort value) => Memory.Write16(addr, value);
	public void MemWrite8(uint addr, byte value) => Memory.Write8(addr, value);
	public ushort MemRead16(uint addr) => Memory.Read16(addr);
	public void MemWrite64(uint addr, ulong value) => Memory.Write64(addr, value);
	public void MemZero(uint addr, uint size) => Memory.WriteBytes(addr, new byte[size]);

	// Write an unmanaged struct to emulated memory
	public unsafe void MemWriteStruct<T>(uint addr, ref T value) where T : unmanaged
	{
		var size = sizeof(T);
		var bytes = new byte[size];
		fixed (T* ptr = &value)
		{
			Marshal.Copy((nint)ptr, bytes, 0, size);
		}
		Memory.WriteBytes(addr, bytes);
		try { Diagnostics.Diagnostics.LogMemWrite(addr, bytes.Length, bytes); } catch { }
	}

	// Read an unmanaged struct from emulated memory
	public unsafe T MemReadStruct<T>(uint addr) where T : unmanaged
	{
		var size = sizeof(T);
		var bytes = Memory.GetSpan(addr, size);
		T value;
		fixed (byte* ptr = bytes)
		{
			value = *(T*)ptr;
		}
		return value;
	}

	// Handle table ops
	public uint RegisterHandle(object obj)
	{
		var h = _nextHandle;
		_nextHandle += 4;
		_handles[h] = obj;
		return h;
	}

	public bool TryGetHandle<T>(uint handle, out T? value) where T : class
	{
		if (_handles.TryGetValue(handle, out var obj) && obj is T t)
		{
			value = t;
			return true;
		}

		value = null;
		return false;
	}

	public bool CloseHandle(uint handle) => _handles.Remove(handle);

	// Module loading tracking
	public uint LoadModule(string moduleName)
	{
		var normalizedName = Path.GetFileName(moduleName).ToUpperInvariant();
		if (_loadedModules.TryGetValue(normalizedName, out var existingHandle))
		{
			return existingHandle;
		}

		var handle = _nextModuleHandle;
		_nextModuleHandle += 0x1000;
		_loadedModules[normalizedName] = handle;
		return handle;
	}

	public uint LoadPeImage(string imagePath, PeImageLoader peLoader)
	{
		var normalizedName = Path.GetFileName(imagePath).ToUpperInvariant();
		if (_loadedModules.TryGetValue(normalizedName, out var existingHandle))
		{
			return existingHandle;
		}

		try
		{
			var loadedImage = peLoader.Load(imagePath);
			var handle = loadedImage.BaseAddress;
			
			_loadedModules[normalizedName] = handle;
			_loadedImages[normalizedName] = loadedImage;
			
			_logger.LogInformation("[ProcessEnv] Loaded PE image: {ImagePath} at 0x{Handle:X8}", imagePath, handle);
			return handle;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[ProcessEnv] Failed to load PE image {ImagePath}: {ExMessage}", imagePath, ex.Message);
			// Fall back to tracking without loading
			return LoadModule(normalizedName);
		}
	}

	/// <summary>
	/// Registers the main executable's LoadedImage so it can be found by GetModuleFileNameA.
	/// This should be called after loading the main executable.
	/// </summary>
	public void RegisterMainExecutable(LoadedImage image, string imagePath)
	{
		var normalizedName = Path.GetFileName(imagePath).ToUpperInvariant();
		_loadedModules[normalizedName] = image.BaseAddress;
		_loadedImages[normalizedName] = image;
		_mainExecutableName = normalizedName; // Track the main executable for synthetic exports
		_logger.LogInformation("[ProcessEnv] Registered main executable: {ImagePath} at 0x{BaseAddress:X8}", imagePath, image.BaseAddress);
	}

	/// <summary>
	/// Get the current main executable's LoadedImage (may have been updated with synthetic exports).
	/// </summary>
	public LoadedImage? GetMainExecutable()
	{
		if (_mainExecutableName != null && _loadedImages.TryGetValue(_mainExecutableName, out var mainExeImage))
		{
			return mainExeImage;
		}
		return null;
	}

	public bool IsModuleLoaded(string moduleName)
	{
		var normalizedName = Path.GetFileName(moduleName).ToUpperInvariant();
		return _loadedModules.ContainsKey(normalizedName);
	}

	/// <summary>
	/// Try to resolve a module handle to a module filename (normalized). Returns null if unknown.
	/// </summary>
	public string? GetModuleFileNameForHandle(uint moduleHandle)
	{
		// Search loaded images first to return full path
		foreach (var kvp in _loadedImages.Where(kvp => kvp.Value.BaseAddress == moduleHandle))
		{
			return kvp.Value.FilePath;
		}

		// Search loaded modules for a matching handle and return normalized name
		return (from kvp in _loadedModules where kvp.Value == moduleHandle select kvp.Key).FirstOrDefault();

		// If not found, return null
	}

	/// <summary>
	/// Try to get a loaded PE image by its module handle.
	/// </summary>
	public bool TryGetLoadedImage(uint moduleHandle, out LoadedImage? image)
	{
		// Search loaded images by base address
		foreach (var kvp in _loadedImages.Where(kvp => kvp.Value.BaseAddress == moduleHandle))
		{
			image = kvp.Value;
			return true;
		}

		image = null;
		return false;
	}

	/// <summary>
	/// Register a synthetic export for an emulated module.
	/// Returns a synthetic address that can be used to call this export.
	/// Uses the syscall mechanism (CALL/RET stub) for unified handling with import stubs.
	/// </summary>
	public uint RegisterSyntheticExport(string moduleName, string exportName)
	{
		var address = _nextSyntheticExport;
		_nextSyntheticExport += 0x10;
		
		// Create import stub using syscall mechanism (same as regular imports):
		// CALL [syscall_dispatcher]; RET argBytes
		// The RET instruction will be patched at runtime with the correct argBytes value
		
		// Calculate relative offset from stub address to syscall dispatcher
		var stubAddr = address;
		var callOffset = (int)(SYSCALL_DISPATCHER_ADDRESS - (stubAddr + 5)); // +5 for size of CALL instruction
		
		var stub = new byte[]
		{
			0xE8, // CALL rel32
			(byte)(callOffset & 0xFF),
			(byte)((callOffset >> 8) & 0xFF),
			(byte)((callOffset >> 16) & 0xFF),
			(byte)((callOffset >> 24) & 0xFF),
			0xC2, 0x00, 0x00, // RET 0 - will be patched at runtime with actual argBytes
			// Padding to maintain 16-byte alignment
			0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90
		};
		Memory.WriteBytes(address, stub);
		
		// Add to the main executable's import map so syscall handler can find it
		// Get the main executable's LoadedImage using the tracked name
		if (_mainExecutableName != null && _loadedImages.TryGetValue(_mainExecutableName, out var mainExeImage))
		{
			// Create a new ImportAddressMap with the synthetic export added (respecting record immutability)
			var newImportAddressMap = new Dictionary<uint, (string dll, string name)>(mainExeImage.ImportAddressMap)
			{
				[address] = (moduleName.ToUpperInvariant(), exportName)
			};
			
			// Create a new LoadedImage with the updated ImportAddressMap
			var updatedMainExeImage = mainExeImage with { ImportAddressMap = newImportAddressMap };
			
			// Replace the main executable's LoadedImage in the dictionary
			_loadedImages[_mainExecutableName] = updatedMainExeImage;
			
			_logger.LogInformation("[ProcessEnv] Registered synthetic export: {Module}!{Export} at 0x{Address:X8}", moduleName, exportName, address);
		}
		else
		{
			_logger.LogWarning("[ProcessEnv] Could not register synthetic export {Module}!{Export} - no main executable loaded", moduleName, exportName);
		}
		
		return address;
	}

	// Heaps
	private readonly Dictionary<uint, HeapState> _heaps = new();
	private readonly Dictionary<uint, uint> _heapAllocationSizes = new(); // Track allocation sizes (address -> size)

	public uint HeapCreate(uint flOptions, uint dwInitialSize, uint dwMaximumSize)
	{
		var init = AlignUp(dwInitialSize == 0 ? 0x10000u : dwInitialSize, 0x1000);
		var max = dwMaximumSize == 0 ? init : AlignUp(dwMaximumSize, 0x1000);
		var baseAddr = SimpleAlloc(init);
		_heaps[baseAddr] = new HeapState(baseAddr, baseAddr, baseAddr + max);
		return baseAddr;
	}

	public uint HeapAlloc(uint hHeap, uint dwBytes)
	{
		if (!_heaps.TryGetValue(hHeap, out var hs))
		{
			// No heap with this handle - use VirtualAlloc for fallback allocation
			// This allows the memory to be properly freed later
			const uint MEM_COMMIT = 0x00001000;
			const uint PAGE_READWRITE = 0x04;
			var addr = VirtualAlloc(0, dwBytes, MEM_COMMIT, PAGE_READWRITE);
			if (addr != 0)
			{
				_heapAllocationSizes[addr] = dwBytes;
			}
			return addr;
		}

		var size = AlignUp(dwBytes == 0 ? 1u : dwBytes, 16);
		if (hs.Current + size <= hs.Limit)
		{
			var ptr = hs.Current;
			hs = hs with { Current = hs.Current + size };
			_heaps[hHeap] = hs;
			_heapAllocationSizes[ptr] = dwBytes; // Track the requested size, not aligned
			return ptr;
		}

		// Heap is full - use VirtualAlloc for fallback allocation
		// This allows the memory to be properly freed later
		const uint MEM_COMMIT_2 = 0x00001000;
		const uint PAGE_READWRITE_2 = 0x04;
		var fallbackAddr = VirtualAlloc(0, dwBytes, MEM_COMMIT_2, PAGE_READWRITE_2);
		if (fallbackAddr != 0)
		{
			_heapAllocationSizes[fallbackAddr] = dwBytes;
		}
		return fallbackAddr;
	}

	public uint HeapFree(uint hHeap, uint lpMem)
	{
		if (lpMem == 0)
		{
			_logger.LogWarning("[ProcessEnv] HeapFree: Invalid address 0x00000000");
			return 0;
		}

		// Check if this allocation has a tracked size
		if (!_heapAllocationSizes.TryGetValue(lpMem, out var size))
		{
			_logger.LogWarning("[ProcessEnv] HeapFree: Address 0x{Address:X8} not found in allocation tracking", lpMem);
			return 0;
		}

		// Remove from tracking
		_heapAllocationSizes.Remove(lpMem);

		// Check if this allocation is within a heap's memory pool
		if (_heaps.TryGetValue(hHeap, out var hs))
		{
			// Check if the address is within the heap's range
			if (lpMem >= hs.Base && lpMem < hs.Limit)
			{
				// This is a heap pool allocation - don't free it individually
				// The memory will be freed when the heap is destroyed
				_logger.LogDebug("[ProcessEnv] HeapFree: Address 0x{Address:X8} is within heap pool, not freeing individually", lpMem);
				return 1;
			}
		}

		// This is a fallback allocation (from VirtualAlloc) - free it properly
		const uint MEM_RELEASE = 0x8000;
		var success = VirtualFree(lpMem, 0, MEM_RELEASE);
		if (!success)
		{
			_logger.LogWarning("[ProcessEnv] HeapFree: VirtualFree failed for address 0x{Address:X8}", lpMem);
			return 0;
		}

		_logger.LogDebug("[ProcessEnv] HeapFree: Freed fallback allocation at 0x{Address:X8}, size=0x{Size:X}", lpMem, size);
		return 1;
	}

	public uint HeapSize(uint hHeap, uint lpMem)
	{
		// Return the size of the allocated block, or 0 if not found
		return _heapAllocationSizes.TryGetValue(lpMem, out var size) ? size : 0;
	}

	// Console management
	/// <summary>
	/// Allocate a console for the process and initialize standard handles.
	/// </summary>
	/// <returns>True if console was allocated successfully</returns>
	public bool AllocateConsole()
	{
		if (HasConsole)
		{
			_logger.LogWarning("[ProcessEnvironment] AllocConsole called but console already exists");
			return false; // Console already exists
		}

		HasConsole = true;
		
		// Initialize standard handles to valid values
		// Use simple sequential handle values for console handles
		StdInputHandle = 0x00000001;
		StdOutputHandle = 0x00000002;
		StdErrorHandle = 0x00000003;
		
		_logger.LogInformation("[ProcessEnvironment] Console allocated - stdin=0x{StdIn:X8}, stdout=0x{StdOut:X8}, stderr=0x{StdErr:X8}", 
			StdInputHandle, StdOutputHandle, StdErrorHandle);
		
		return true;
	}

	/// <summary>
	/// Free the console and reset standard handles to NULL.
	/// </summary>
	/// <returns>True if console was freed successfully</returns>
	public bool FreeConsole()
	{
		if (!HasConsole)
		{
			_logger.LogWarning("[ProcessEnvironment] FreeConsole called but no console exists");
			return false;
		}

		HasConsole = false;
		
		// Reset standard handles to NULL
		StdInputHandle = 0x00000000;
		StdOutputHandle = 0x00000000;
		StdErrorHandle = 0x00000000;
		
		_logger.LogInformation("[ProcessEnvironment] Console freed");
		
		return true;
	}

	/// <summary>
	/// Initialize standard handles based on PE subsystem type.
	/// </summary>
	/// <param name="subsystem">PE subsystem value (2=GUI, 3=CUI)</param>
	public void InitializeConsoleForSubsystem(ushort subsystem)
	{
		// IMAGE_SUBSYSTEM_WINDOWS_CUI = 3 (Console app)
		// IMAGE_SUBSYSTEM_WINDOWS_GUI = 2 (GUI app)
		const ushort IMAGE_SUBSYSTEM_WINDOWS_GUI = 2;
		const ushort IMAGE_SUBSYSTEM_WINDOWS_CUI = 3;
		
		if (subsystem == IMAGE_SUBSYSTEM_WINDOWS_CUI)
		{
			// Console application - allocate console automatically
			_logger.LogInformation("[ProcessEnvironment] Detected console subsystem, allocating console");
			AllocateConsole();
		}
		else if (subsystem == IMAGE_SUBSYSTEM_WINDOWS_GUI)
		{
			// GUI application - no console by default (handles remain NULL)
			_logger.LogInformation("[ProcessEnvironment] Detected GUI subsystem, no console allocated");
		}
		else
		{
			// Unknown subsystem - default to GUI behavior (no console)
			_logger.LogWarning("[ProcessEnvironment] Unknown subsystem type {Subsystem}, defaulting to GUI behavior", subsystem);
		}
	}

	/// <summary>
	/// Reserves and/or commits virtual memory following Windows semantics:
	/// MEM_RESERVE aligns base/size to 64KB and does not touch memory; MEM_COMMIT aligns to 4KB.
	/// Bottom-up search by default; honors MEM_TOP_DOWN when scanning free list; avoids emulator internal ranges.
	/// Returns 0 on failure instead of throwing; normal failures do not write to memory.
	/// </summary>
	public uint VirtualAlloc(uint lpAddress, uint dwSize, uint flAllocationType, uint flProtect)
	{
		// Flags
		const uint MEM_COMMIT   = 0x00001000;
		const uint MEM_RESERVE  = 0x00002000;
		const uint MEM_TOP_DOWN = 0x00100000;
		const uint PAGE_SIZE    = 0x1000;   // 4KB
		const uint ALLOC_GRAN   = 0x10000;  // 64KB
		const uint SPECIAL_MIN  = 0x0D000000; // emulator special ranges (COM/syscall/import)
		const uint SPECIAL_MAX  = 0x10000000;
		
		bool reserve = (flAllocationType & MEM_RESERVE) != 0;
		bool commit  = (flAllocationType & MEM_COMMIT) != 0;
		bool topDown = (flAllocationType & MEM_TOP_DOWN) != 0;
		
		_logger.LogInformation("[ProcessEnv] VirtualAlloc(lp=0x{Lp:X8}, size=0x{Size:X8}, alloc=0x{Alloc:X8}, prot=0x{Prot:X8})",
			lpAddress, dwSize, flAllocationType, flProtect);
		
		if (!reserve && !commit)
		{
			_logger.LogWarning("[ProcessEnv] VirtualAlloc: neither MEM_RESERVE nor MEM_COMMIT specified");
			return 0;
		}
		
		uint requested = dwSize == 0 ? 1u : dwSize;
		// Align sizes respecting semantics
		uint reserveSize = reserve ? AlignUp(requested, ALLOC_GRAN) : 0u;
		uint commitSize  = commit  ? AlignUp(requested, PAGE_SIZE)  : 0u;
		uint effectiveSize = reserve ? reserveSize : commitSize;
		if (effectiveSize == 0)
		{
			// Should not happen because requested >= 1 and either reserve or commit is true
			return 0;
		}
		
		// If specific address provided
		if (lpAddress != 0)
		{
			uint alignedBase = reserve ? AlignDown(lpAddress, ALLOC_GRAN) : AlignDown(lpAddress, PAGE_SIZE);
			if (!RangeFits(alignedBase, effectiveSize, Memory.Size))
			{
				_logger.LogWarning("[ProcessEnv] VirtualAlloc: requested range overflows address space (base=0x{Base:X8}, size=0x{Size:X8})", alignedBase, effectiveSize);
				return 0;
			}
			// Avoid emulator special ranges
			if (!(alignedBase + effectiveSize <= SPECIAL_MIN || alignedBase >= SPECIAL_MAX))
			{
				_logger.LogWarning("[ProcessEnv] VirtualAlloc: requested range overlaps emulator special range [0x{Min:X8}-0x{Max:X8}), failing", SPECIAL_MIN, SPECIAL_MAX);
				return 0;
			}
			return AllocateAtSpecificAddress(alignedBase, effectiveSize, reserve, commit);
		}
		
		// No specific address - find a suitable block or allocate from end
		var addr = AllocateFromFreeList(effectiveSize, reserve, commit, topDown, SPECIAL_MIN, SPECIAL_MAX);
		if (addr != 0)
		{
			_logger.LogInformation("[ProcessEnv] VirtualAlloc: allocated at 0x{Addr:X8}, size=0x{Size:X8}, reserve={Reserve}, commit={Commit}, topDown={TopDown}",
				addr, effectiveSize, reserve, commit, topDown);
			return addr;
		}
		
		_logger.LogWarning("[ProcessEnv] VirtualAlloc: allocation failed (size=0x{Size:X8})", effectiveSize);
		return 0;
	}

	/// <summary>
	/// Allocates memory at a specific address.
	/// </summary>
	private uint AllocateAtSpecificAddress(uint lpAddress, uint size, bool reserve, bool commit)
	{
		// Bounds check: ensure [lpAddress, lpAddress+size) fits in 32-bit address space
		ulong end64 = (ulong)lpAddress + (ulong)size;
		if (end64 < lpAddress || end64 > Memory.Size)
		{
			_logger.LogWarning("[ProcessEnv] AllocateAtSpecificAddress: range overflow (base=0x{Base:X8}, size=0x{Size:X8})", lpAddress, size);
			return 0;
		}
		uint endOfAllocation = (uint)end64;
		
		// Check if this exact region is already allocated (re-commit scenario)
		bool alreadyAllocated = _allocatedBlocks.TryGetValue(lpAddress, out var existingBlock)
			&& existingBlock.Size >= size;
		
		if (alreadyAllocated)
		{
			_logger.LogInformation("[ProcessEnv] VirtualAlloc: Re-committing already allocated region at 0x{Address:X8}", lpAddress);
		}
		else
		{
			// Verify the requested range is available
			if (!IsRangeAvailable(lpAddress, size))
			{
				_logger.LogWarning("[ProcessEnv] VirtualAlloc: Requested address range 0x{Address:X8}-0x{EndAddress:X8} is not available",
					lpAddress, endOfAllocation);
				return 0; // Failed to allocate
			}
			
			// Mark the range as allocated
			MarkRangeAsAllocated(lpAddress, size);
		}
		
		// Do not touch memory for MEM_RESERVE. For MEM_COMMIT we can rely on sparse pages being zeroed by default.
		// Therefore we skip any bulk writes here to avoid boundary issues and unnecessary work.
		
		// Update _allocPtr if the allocation extends beyond it
		if (endOfAllocation > _allocPtr)
		{
			_allocPtr = endOfAllocation;
		}
		
		_logger.LogInformation("[ProcessEnv] VirtualAlloc: Allocated 0x{Size:X} bytes at specific address 0x{Address:X8} (reserve={Reserve}, commit={Commit})",
			size, lpAddress, reserve, commit);
		
		return lpAddress;
	}

	/// <summary>
	/// Allocates memory from the free list.
	/// </summary>
	private uint AllocateFromFreeList(uint size, bool reserve, bool commit, bool topDown, uint avoidStart, uint avoidEnd)
	{
		const uint PAGE_SIZE = 0x1000;
		const uint ALLOC_GRAN = 0x10000;
		uint align = reserve ? ALLOC_GRAN : PAGE_SIZE;
		
		// Helper to validate a candidate range and avoid reserved internal ranges
		bool IsCandidateValid(uint baseAddr, uint span)
		{
			if (!RangeFits(baseAddr, span, Memory.Size)) return false;
			// reject if overlaps [avoidStart, avoidEnd)
			ulong start = baseAddr;
			ulong end = start + span;
			return (end <= avoidStart) || (start >= avoidEnd);
		}
		
		// Search free list according to direction
		if (_freeList.Count > 0)
		{
			if (topDown)
			{
				for (int i = _freeList.Count - 1; i >= 0; i--)
				{
					var block = _freeList[i];
					// choose the highest aligned start within this block where [addr, addr+size) fits
					uint blockStart = block.Address;
					uint blockEnd = block.EndAddress;
					if (block.Size < size) continue;
					uint maxStart = blockEnd - size;
					uint cand = AlignDown(maxStart, align);
					if (cand < blockStart) cand = blockStart; // clamp
					// Also ensure alignment
					cand = AlignUp(cand, align);
					if (cand < blockStart || cand + size > blockEnd) continue;
					if (!IsCandidateValid(cand, size)) continue;
					
					// Carve this allocation from the end of the block
					uint usedStart = cand;
					uint usedEnd = cand + size;
					// Adjust free block(s)
					if (block.Address == usedStart && block.EndAddress == usedEnd)
					{
						_freeList.RemoveAt(i);
					}
					else if (block.Address == usedStart)
					{
						block.Address = usedEnd;
						block.Size = blockEnd - usedEnd;
					}
					else if (block.EndAddress == usedEnd)
					{
						block.Size = usedStart - block.Address;
					}
					else
					{
						// Split into two blocks
						var after = new MemoryBlock(usedEnd, blockEnd - usedEnd, true);
						block.Size = usedStart - block.Address;
						_freeList.Add(after);
					}
					
					_allocatedBlocks[usedStart] = new MemoryBlock(usedStart, size, false);
					return usedStart;
				}
			}
			else
			{
				for (int i = 0; i < _freeList.Count; i++)
				{
					var block = _freeList[i];
					if (block.Size < size) continue;
					uint cand = AlignUp(block.Address, align);
					if (cand + size > block.EndAddress) continue;
					if (!IsCandidateValid(cand, size))
					{
						// Try to move to after avoidEnd if it splits inside this block
						if (block.Address < avoidStart && block.EndAddress > avoidStart)
						{
							cand = AlignUp(avoidEnd, align);
							if (cand + size > block.EndAddress) continue;
							if (!IsCandidateValid(cand, size)) continue;
						}
						else continue;
					}
					// Carve from start
					uint usedStart = cand;
					uint usedEnd = cand + size;
					if (block.Address == usedStart && block.EndAddress == usedEnd)
					{
						_freeList.RemoveAt(i);
					}
					else if (block.Address == usedStart)
					{
						block.Address = usedEnd;
						block.Size = block.EndAddress - usedEnd;
					}
					else if (block.EndAddress == usedEnd)
					{
						block.Size = usedStart - block.Address;
					}
					else
					{
						var after = new MemoryBlock(usedEnd, block.EndAddress - usedEnd, true);
						block.Size = usedStart - block.Address;
						_freeList.Add(after);
					}
					_allocatedBlocks[usedStart] = new MemoryBlock(usedStart, size, false);
					return usedStart;
				}
			}
		}
		
		// No suitable free block found - allocate from the end (bump pointer)
		uint addr = AlignUp(_allocPtr, align);
		// Skip over avoid range if we would overlap it
		if (!(addr + size <= avoidStart || addr >= avoidEnd))
		{
			addr = AlignUp(avoidEnd, align);
		}
		if (!RangeFits(addr, size, Memory.Size))
		{
			_logger.LogWarning("[ProcessEnv] VirtualAlloc: bump-pointer allocation would overflow (addr=0x{Addr:X8}, size=0x{Size:X8})", addr, size);
			return 0;
		}
		_allocPtr = addr + size;
		_allocatedBlocks[addr] = new MemoryBlock(addr, size, false);
		return addr;
	}

	/// <summary>
	/// Checks if a memory range is available (not already allocated).
	/// </summary>
	private bool IsRangeAvailable(uint address, uint size)
	{
		// Calculate end address with overflow protection
		ulong endAddress64 = (ulong)address + size;
		uint endAddress = endAddress64 > uint.MaxValue ? uint.MaxValue : (uint)endAddress64;
		
		// Check against all allocated blocks
		foreach (var block in _allocatedBlocks.Values)
		{
			if (!(endAddress <= block.Address || address >= block.EndAddress))
			{
				// Ranges overlap
				return false;
			}
		}
		
		return true;
	}

	/// <summary>
	/// Marks a memory range as allocated by removing it from free list and adding to allocated list.
	/// </summary>
	private void MarkRangeAsAllocated(uint address, uint size)
	{
		// Calculate end address with overflow protection
		ulong endAddress64 = (ulong)address + size;
		uint endAddress = endAddress64 > uint.MaxValue ? uint.MaxValue : (uint)endAddress64;
		
		// Remove or split any free blocks that overlap with this range
		for (int i = _freeList.Count - 1; i >= 0; i--)
		{
			var block = _freeList[i];
			
			// Check if this free block overlaps with the requested range
			if (block.Address < endAddress && block.EndAddress > address)
			{
				// There's an overlap - we need to handle it
				if (block.Address >= address && block.EndAddress <= endAddress)
				{
					// Free block is completely contained - remove it
					_freeList.RemoveAt(i);
				}
				else if (block.Address < address && block.EndAddress > endAddress)
				{
					// Free block contains the requested range - split into two
					var afterBlock = new MemoryBlock(endAddress, block.EndAddress - endAddress, true);
					block.Size = address - block.Address;
					_freeList.Add(afterBlock);
				}
				else if (block.Address < address)
				{
					// Free block overlaps at the end - trim it
					block.Size = address - block.Address;
				}
				else
				{
					// Free block overlaps at the start - trim it
					uint newStart = endAddress;
					block.Size = block.EndAddress - newStart;
					block.Address = newStart;
				}
			}
		}
		
		// Add to allocated blocks
		_allocatedBlocks[address] = new MemoryBlock(address, size, false);
	}

	/// <summary>
	/// Frees allocated virtual memory.
	/// </summary>
	public bool VirtualFree(uint lpAddress, uint dwSize, uint dwFreeType)
	{
		const uint memRelease = 0x8000;
		
		// Validate parameters
		if (lpAddress == 0)
		{
			_logger.LogWarning("[ProcessEnv] VirtualFree: Invalid address 0x00000000");
			return false;
		}
		
		// When using MEM_RELEASE, dwSize must be 0
		if ((dwFreeType & memRelease) != 0 && dwSize != 0)
		{
			_logger.LogWarning("[ProcessEnv] VirtualFree: MEM_RELEASE requires dwSize to be 0");
			return false;
		}
		
		// Find the allocated block
		if (!_allocatedBlocks.TryGetValue(lpAddress, out var blockToFree))
		{
			_logger.LogWarning("[ProcessEnv] VirtualFree: Address 0x{Address:X8} not found in allocated blocks",
				lpAddress);
			return false;
		}
		
		// Remove from allocated blocks
		_allocatedBlocks.Remove(lpAddress);
		
		// Add to free list
		var freedBlock = new MemoryBlock(blockToFree.Address, blockToFree.Size, true);
		_freeList.Add(freedBlock);
		
		// Merge adjacent free blocks
		MergeAdjacentFreeBlocks();
		
		_logger.LogInformation("[ProcessEnv] VirtualFree: Freed 0x{Size:X} bytes at 0x{Address:X8}",
			blockToFree.Size, lpAddress);
		
		return true;
	}

	/// <summary>
	/// Merges adjacent free blocks to reduce fragmentation.
	/// </summary>
	private void MergeAdjacentFreeBlocks()
	{
		if (_freeList.Count <= 1)
		{
			return;
		}
		
		// Sort free list by address
		_freeList.Sort((a, b) => a.Address.CompareTo(b.Address));
		
		// Merge adjacent blocks
		for (int i = 0; i < _freeList.Count - 1; )
		{
			var current = _freeList[i];
			var next = _freeList[i + 1];
			
			if (current.EndAddress == next.Address)
			{
				// Adjacent blocks - merge them
				current.Size += next.Size;
				_freeList.RemoveAt(i + 1);
				
				_logger.LogDebug("[ProcessEnv] Merged adjacent free blocks at 0x{Address:X8} (new size: 0x{Size:X})",
					current.Address, current.Size);
			}
			else
			{
				i++;
			}
		}
	}

	private static uint AlignUp(uint value, uint align) => (value + (align - 1)) & ~(align - 1);
	private static uint AlignDown(uint value, uint align) => value & ~(align - 1);
	private static bool RangeFits(uint baseAddr, uint size, ulong limit)
	{
		ulong start = baseAddr;
		ulong end = start + size;
		return end >= start && end <= limit;
	}

	private record struct HeapState(uint Base, uint Current, uint Limit);

	// Window management structures and methods
	public record struct WindowClassInfo(
		string ClassName,
		uint Style,
		uint WndProc,
		int ClsExtra,
		int WndExtra,
		uint HInstance,
		uint HIcon,
		uint HCursor,
		uint HbrBackground,
		string? MenuName
	);

	public record struct WindowInfo(
		uint Handle,
		string ClassName,
		string WindowName,
		uint Style,
		uint ExStyle,
		int X,
		int Y,
		int Width,
		int Height,
		uint Parent,
		uint Menu,
		uint Instance,
		uint Param
	);

	/// <summary>
	/// Pre-registers standard Windows control classes that are built into the OS.
	/// These classes are available without explicit registration in real Windows.
	/// </summary>
	private void RegisterStandardControlClasses()
	{
		// Standard system window class names from Windows
		// These are the predefined window classes available to all processes
		// See: https://learn.microsoft.com/en-us/windows/win32/winmsg/about-window-classes#system-classes
		var standardClasses = new[]
		{
			"BUTTON",
			"EDIT",
			"STATIC",
			"LISTBOX",
			"COMBOBOX",
			"SCROLLBAR",
			"MDICLIENT"
		};

		uint index = 0;
		foreach (var className in standardClasses)
		{
			// Each standard control class gets a unique window procedure address
			// This allows code to get a non-NULL wndProc, while User32Module can detect
			// these special addresses and route messages to StandardControlHandler
			// Using a simple counter ensures no collisions (unlike GetHashCode)
			var wndProcAddress = STANDARD_CONTROL_WNDPROC_BASE + index;
			
			var classInfo = new WindowClassInfo(
				ClassName: className,
				Style: 0,
				WndProc: wndProcAddress, // Use special marker address for standard controls
				ClsExtra: 0,
				WndExtra: 0,
				HInstance: 0,
				HIcon: 0,
				HCursor: 0,
				HbrBackground: 0,
				MenuName: null
			);

			_windowClasses.TryAdd(className, classInfo);
			_logger.LogInformation("[ProcessEnv] Pre-registered standard control class: {ClassName} with WndProc=0x{WndProc:X8}", className, wndProcAddress);
			index++;
		}
	}

	public bool RegisterWindowClass(string className, WindowClassInfo classInfo)
	{
		if (!_windowClasses.TryAdd(className, classInfo))
		{
			_logger.LogError("[ProcessEnv] Window class '{ClassName}' already registered", className);
			return false;
		}

		_logger.LogInformation("[ProcessEnv] Registered window class: {ClassName}", className);
		return true;
	}

	public bool IsWindowClassRegistered(string className)
	{
		return _windowClasses.ContainsKey(className);
	}

	public WindowClassInfo? GetWindowClass(string className)
	{
		return _windowClasses.TryGetValue(className, out var classInfo) ? classInfo : null;
	}

	public void RegisterAtom(uint atom, string className)
	{
		_atomToClassName[atom] = className;
	}

	public string? GetClassNameFromAtom(uint atom)
	{
		return _atomToClassName.GetValueOrDefault(atom);
	}

	/// <summary>
	/// Registers a new window message that is guaranteed to be unique throughout the system.
	/// This implements the behavior of RegisterWindowMessageA.
	/// </summary>
	/// <param name="messageName">The message string to register</param>
	/// <returns>The registered message identifier in the range 0xC000 through 0xFFFF</returns>
	public uint RegisterWindowMessage(string messageName)
	{
		// If the message is already registered, return the existing ID
		if (_registeredMessages.TryGetValue(messageName, out var existingId))
		{
			_logger.LogDebug("[ProcessEnv] RegisterWindowMessage: '{MessageName}' already registered as 0x{ExistingId:X4}", messageName, existingId);
			return existingId;
		}

		// Ensure we don't overflow the registered message range
		if (_nextRegisteredMessage > 0xFFFF)
		{
			_logger.LogError("[ProcessEnv] RegisterWindowMessage: Registered message range exhausted! Cannot register '{MessageName}'", messageName);
			return 0; // Return 0 to indicate failure
		}

		// Allocate a new message ID in the registered message range (0xC000-0xFFFF)
		var messageId = _nextRegisteredMessage;
		_nextRegisteredMessage++;

		_registeredMessages[messageName] = messageId;
		_logger.LogInformation("[ProcessEnv] RegisterWindowMessage: '{MessageName}' registered as 0x{MessageId:X4}", messageName, messageId);
		return messageId;
	}

	/// <summary>
	/// Gets the message ID for a registered window message.
	/// </summary>
	/// <param name="messageName">The message string to look up</param>
	/// <returns>The message ID if found, or 0 if not registered</returns>
	public uint GetRegisteredMessage(string messageName)
	{
		if (_registeredMessages.TryGetValue(messageName, out var messageId))
		{
			return messageId;
		}
		return 0;
	}

	public uint CreateWindow(string className, string windowName, uint style, uint exStyle,
		int x, int y, int width, int height, uint parent, uint menu, uint instance, uint param)
	{
		if (!_windowClasses.ContainsKey(className))
		{
			_logger.LogError("[ProcessEnv] CreateWindow failed: Window class '{ClassName}' not registered", className);
			return 0;
		}

		var handle = _nextWindowHandle;
		_nextWindowHandle += 4;

		var windowInfo = new WindowInfo(
			handle, className, windowName, style, exStyle,
			x, y, width, height, parent, menu, instance, param
		);

		_windows[handle] = windowInfo;
		_logger.LogInformation("[ProcessEnv] Created window: HWND=0x{Handle:X8} Class='{ClassName}' Title='{WindowName}'", handle, className, windowName);

		// Notify host about window creation (Phase 2: Window Management)
		// The GUI will create an actual Avalonia window when this is called
		_host?.OnWindowCreate(new WindowCreateInfo
		{
			Handle = handle,
			Title = windowName,
			Width = width,
			Height = height,
			X = x,
			Y = y,
			ClassName = className,
			Style = style,
			ExStyle = exStyle,
			Parent = parent,
			Menu = menu
		});

		// Send WM_CREATE message to the window
		// WM_CREATE = 0x0001
		SendMessageToWindow(handle, 0x0001, 0, param);
		_logger.LogDebug("[ProcessEnv] Sent WM_CREATE to window 0x{Handle:X8}", handle);

		// Send WM_SIZE message to the window
		// WM_SIZE = 0x0005, wParam = SIZE_RESTORED (0), lParam = MAKELONG(width, height)
		uint sizeParam = ((uint)height << 16) | ((uint)width & 0xFFFF);
		SendMessageToWindow(handle, 0x0005, 0, sizeParam);
		_logger.LogDebug("[ProcessEnv] Sent WM_SIZE to window 0x{Handle:X8} (width={Width}, height={Height})", handle, width, height);

		// Send WM_MOVE message to the window
		// WM_MOVE = 0x0003, wParam = 0, lParam = MAKELONG(x, y)
		uint moveParam = ((uint)y << 16) | ((uint)x & 0xFFFF);
		SendMessageToWindow(handle, 0x0003, 0, moveParam);
		_logger.LogDebug("[ProcessEnv] Sent WM_MOVE to window 0x{Handle:X8} (x={X}, y={Y})", handle, x, y);

		return handle;
	}

	public WindowInfo? GetWindow(uint hwnd)
	{
		return _windows.TryGetValue(hwnd, out var windowInfo) ? windowInfo : null;
	}

	public bool DestroyWindow(uint hwnd)
	{
		if (_windows.Remove(hwnd))
		{
			_logger.LogInformation("[ProcessEnv] Destroyed window: HWND=0x{Hwnd:X8}", hwnd);
			return true;
		}
		return false;
	}

	// Message queue management
	public void PostQuitMessage(int exitCode)
	{
		_hasQuitMessage = true;
		_quitExitCode = exitCode;
		_logger.LogInformation("[ProcessEnv] PostQuitMessage: exitCode={ExitCode}", exitCode);
	}

	public bool HasQuitMessage()
	{
		return _hasQuitMessage;
	}

	public int GetQuitExitCode()
	{
		return _quitExitCode;
	}

	/// <summary>
	/// Post a message to the message queue asynchronously
	/// </summary>
	public bool PostMessage(uint hwnd, uint message, uint wParam, uint lParam)
	{
		var timestamp = (uint)Environment.TickCount;
		var queuedMsg = new QueuedMessage(hwnd, message, wParam, lParam, timestamp, 0, 0);
		
		// Try to write to the channel
		if (_messageQueue.Writer.TryWrite(queuedMsg))
		{
			_logger.LogInformation("[ProcessEnv] PostMessage: queued MSG=0x{Message:X4} HWND=0x{Hwnd:X8}", message, hwnd);
			
			// Wake any threads waiting for messages in GetMessage
			WakeThreadsWaitingForMessages();
			
			return true;
		}

		_logger.LogWarning("[ProcessEnv] PostMessage: failed to queue MSG=0x{Message:X4}", message);
		return false;
	}
	
	/// <summary>
	/// Wake all threads that are waiting for messages (blocked in GetMessage)
	/// </summary>
	private void WakeThreadsWaitingForMessages()
	{
		if (ThreadScheduler == null)
			return;
			
		// Find all threads waiting on the message queue and wake them
		var allThreads = ThreadScheduler.GetAllThreads();
		foreach (var thread in allThreads.Where(t => t.State == Threading.ThreadState.Waiting && ReferenceEquals(t.WaitingOn, _messageQueueWaitToken)))
		{
			ThreadScheduler.WakeThread(thread.ThreadId);
			_logger.LogDebug("[ProcessEnv] Woke thread {ThreadId} waiting for messages", thread.ThreadId);
		}
	}

	/// <summary>
	/// Send a message directly to a window (synchronous) by posting it to the queue.
	/// For system messages during window creation/lifecycle, we post them so they can be processed
	/// in the normal message loop.
	/// </summary>
	public void SendMessageToWindow(uint hwnd, uint message, uint wParam, uint lParam)
	{
		_logger.LogDebug("[ProcessEnv] SendMessageToWindow: posting MSG=0x{Message:X4} to HWND=0x{Hwnd:X8}", message, hwnd);
		PostMessage(hwnd, message, wParam, lParam);
	}

	/// <summary>
	/// Try to get a message from the queue (blocking)
	/// </summary>
	public async Task<QueuedMessage?> GetMessageAsync(uint hwnd, uint msgFilterMin, uint msgFilterMax)
	{
		try
		{
			// Read from the channel (will block until message available)
			var message = await _messageQueue.Reader.ReadAsync();
			
			// Apply filters if specified
			if (hwnd != 0 && message.Hwnd != hwnd)
			{
				// Re-queue messages that don't match the window filter
				_messageQueue.Writer.TryWrite(message);
				return null;
			}

			if (msgFilterMin != 0 || msgFilterMax != 0)
			{
				if (message.Message < msgFilterMin || message.Message > msgFilterMax)
				{
					// Re-queue messages outside the filter range
					_messageQueue.Writer.TryWrite(message);
					return null;
				}
			}

			return message;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"[ProcessEnv] GetMessageAsync error");
			return null;
		}
	}

	/// <summary>
	/// Try to get a message from the queue (async with timeout) - preferred method for new code
	/// </summary>
	public async Task<QueuedMessage?> GetMessageAsync(uint hwnd, uint msgFilterMin, uint msgFilterMax, int timeoutMs = 100)
	{
		try
		{
			// Try to read from the channel with a timeout
			if (_messageQueue.Reader.TryRead(out var message))
			{
				// Apply filters if specified
				if (hwnd != 0 && message.Hwnd != hwnd)
				{
					// Re-queue messages that don't match the window filter
					await _messageQueue.Writer.WriteAsync(message);
					return null;
				}

				if (msgFilterMin != 0 || msgFilterMax != 0)
				{
					if (message.Message < msgFilterMin || message.Message > msgFilterMax)
					{
						// Re-queue messages outside the filter range
						await _messageQueue.Writer.WriteAsync(message);
						return null;
					}
				}

				return message;
			}

			// Wait for a message with timeout
			using var cts = new CancellationTokenSource(timeoutMs);
			
			try
			{
				// Use async waiting which properly yields to other tasks
				message = await _messageQueue.Reader.ReadAsync(cts.Token);
				
				// Apply filters if specified
				if (hwnd != 0 && message.Hwnd != hwnd)
				{
					await _messageQueue.Writer.WriteAsync(message);
					return null;
				}

				if (msgFilterMin != 0 || msgFilterMax != 0)
				{
					if (message.Message < msgFilterMin || message.Message > msgFilterMax)
					{
						await _messageQueue.Writer.WriteAsync(message);
						return null;
					}
				}

				return message;
			}
			catch (OperationCanceledException)
			{
				// Timeout occurred, which is expected
				return null;
			}
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "[ProcessEnv] GetMessageAsync");
			return null;
		}
	}

	/// <summary>
	/// Try to get a message from the queue (blocking, synchronous with timeout)
	/// Note: This is a synchronous wrapper around GetMessageAsync. For better performance and
	/// cooperative multitasking, consider using GetMessageAsync directly where possible.
	/// </summary>
	public QueuedMessage? GetMessageBlocking(uint hwnd, uint msgFilterMin, uint msgFilterMax, int timeoutMs = 100)
	{
		try
		{
			// Use the async version with GetAwaiter().GetResult() for synchronous contexts
			// This is more efficient than Task.Wait() and doesn't wrap exceptions in AggregateException
			return GetMessageAsync(hwnd, msgFilterMin, msgFilterMax, timeoutMs).GetAwaiter().GetResult();
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "[ProcessEnv] GetMessageBlocking");
			return null;
		}
	}
	
	/// <summary>
	/// Get the message queue wait token for thread scheduler integration
	/// </summary>
	public object GetMessageQueueWaitToken()
	{
		return _messageQueueWaitToken;
	}
	
	/// <summary>
	/// Try to get a message from the queue without blocking (polls once)
	/// Returns null if no message is immediately available
	/// </summary>
	public QueuedMessage? TryGetMessageNonBlocking(uint hwnd, uint msgFilterMin, uint msgFilterMax)
	{
		// Use a temporary list to hold messages that don't match the filter
		List<QueuedMessage> requeueList = null;

		while (_messageQueue.Reader.TryRead(out var message))
		{
			bool match = true;
			// Apply filters if specified
			if (hwnd != 0 && message.Hwnd != hwnd)
			{
				match = false;
			}

			if (match && (msgFilterMin != 0 || msgFilterMax != 0) &&
			    (message.Message < msgFilterMin || message.Message > msgFilterMax))
			{
				match = false;
			}

			if (match)
			{
				// Found a matching message, re-queue any held messages and return
				if (requeueList != null)
				{
					foreach (var msgToRequeue in requeueList)
					{
						_messageQueue.Writer.TryWrite(msgToRequeue);
					}
				}
				return message;
			}
			else
			{
				// Message does not match, hold it for re-queueing
				requeueList ??= new List<QueuedMessage>();
				requeueList.Add(message);
			}
		}

		// No matching message found, re-queue all held messages
		if (requeueList != null)
		{
			foreach (var msgToRequeue in requeueList)
			{
				_messageQueue.Writer.TryWrite(msgToRequeue);
			}
		}

		return null;
	}

	/// <summary>
	/// Try to peek at a message from the queue (non-blocking)
	/// </summary>
	public bool TryPeekMessage(out QueuedMessage message, uint hwnd, uint msgFilterMin, uint msgFilterMax, bool remove)
	{
		message = default;

		if (_messageQueue.Reader.TryRead(out var queuedMsg))
		{
			// Apply filters if specified
			if (hwnd != 0 && queuedMsg.Hwnd != hwnd)
			{
				// Re-queue and return false
				_messageQueue.Writer.TryWrite(queuedMsg);
				return false;
			}

			if (msgFilterMin != 0 || msgFilterMax != 0)
			{
				if (queuedMsg.Message < msgFilterMin || queuedMsg.Message > msgFilterMax)
				{
					// Re-queue and return false
					_messageQueue.Writer.TryWrite(queuedMsg);
					return false;
				}
			}

			message = queuedMsg;
			
			// If remove is false (PM_NOREMOVE), put the message back
			if (!remove)
			{
				_messageQueue.Writer.TryWrite(queuedMsg);
			}

			return true;
		}

		return false;
	}

	/// <summary>
	/// Get the window procedure address for a given window handle
	/// </summary>
	public uint? GetWindowProc(uint hwnd)
	{
		if (_windows.TryGetValue(hwnd, out var windowInfo))
		{
			var className = windowInfo.ClassName;
			if (_windowClasses.TryGetValue(className, out var classInfo))
			{
				return classInfo.WndProc;
			}
		}
		return null;
	}

	/// <summary>
	/// Check if a window procedure address is a marker for a standard control.
	/// Standard controls use addresses in the range 0x0D000000 - 0x0DFFFFFF.
	/// These are not actual executable code, but markers to route messages through StandardControlHandler.
	/// </summary>
	public static bool IsStandardControlWndProc(uint wndProcAddress)
	{
		return wndProcAddress >= STANDARD_CONTROL_WNDPROC_BASE && 
		       wndProcAddress <= STANDARD_CONTROL_WNDPROC_END;
	}

	// Thread management methods
	public uint GetCurrentThreadId()
	{
		// Use ThreadScheduler if available, otherwise fall back to simple ID
		if (ThreadScheduler != null)
		{
			var currentThread = ThreadScheduler.CurrentThread;
			return currentThread?.ThreadId ?? _currentThreadId;
		}
		return _currentThreadId;
	}

	/// <summary>
	/// Initialize the main thread in the thread scheduler
	/// </summary>
	public void InitializeMainThread(ICpu cpu)
	{
		if (ThreadScheduler != null)
		{
			var mainThread = ThreadScheduler.InitializeMainThread(cpu, Memory);
			_currentThreadId = mainThread.ThreadId;
			
			// Initialize TLS storage for main thread
			_threadLocalStorage[_currentThreadId] = new Dictionary<uint, uint>();
			
			_logger.LogInformation("[ProcessEnv] Main thread initialized with ID={ThreadId}", _currentThreadId);
		}
	}

	public uint CreateThread(uint entryPoint, uint parameter, uint stackSize, bool suspended = false)
	{
		if (ThreadScheduler != null)
		{
			// Use ThreadScheduler to create a proper thread with its own stack and context
			var thread = ThreadScheduler.CreateThread(entryPoint, parameter, stackSize, Memory, suspended);
			
			// Initialize TLS storage for this thread
			_threadLocalStorage[thread.ThreadId] = new Dictionary<uint, uint>();
			
			_logger.LogInformation("[ProcessEnv] CreateThread: new thread ID={ThreadId} handle=0x{Handle:X8} entry=0x{EntryPoint:X8}",
				thread.ThreadId, thread.Handle, entryPoint);
			
			return thread.Handle; // Return the handle, not the ID
		}
		else
		{
			// Fall back to simple thread emulation (legacy path)
			var threadId = _nextThreadId++;
			
			// Initialize TLS storage for this thread
			_threadLocalStorage[threadId] = new Dictionary<uint, uint>();
			
			_logger.LogInformation("[ProcessEnv] CreateThread: new thread ID={ThreadId} (legacy mode)", threadId);
			return threadId;
		}
	}

	public uint CreateThread()
	{
		// Legacy overload for compatibility - creates a thread with default parameters
		return CreateThread(0, 0, 0x8000, false); // 32KB default stack, running (matches Win32 default)
	}

	// TLS (Thread Local Storage) methods
	public uint TlsAlloc()
	{
		// Find next available TLS index
		var index = _nextTlsIndex++;
		_allocatedTlsIndices.Add(index);
		
		_logger.LogInformation("[ProcessEnv] TlsAlloc: allocated index={Index}", index);
		return index;
	}

	public bool TlsSetValue(uint tlsIndex, uint value)
	{
		if (!_allocatedTlsIndices.Contains(tlsIndex))
		{
			_logger.LogWarning("[ProcessEnv] TlsSetValue: invalid TLS index={TlsIndex}", tlsIndex);
			return false;
		}

		// Get or create TLS storage for current thread
		if (!_threadLocalStorage.TryGetValue(_currentThreadId, out var threadTls))
		{
			threadTls = new Dictionary<uint, uint>();
			_threadLocalStorage[_currentThreadId] = threadTls;
		}

		threadTls[tlsIndex] = value;
		_logger.LogInformation("[ProcessEnv] TlsSetValue: threadId={CurrentThreadId} index={TlsIndex} value=0x{Value:X8}", _currentThreadId, tlsIndex, value);
		return true;
	}

	public uint TlsGetValue(uint tlsIndex)
	{
		if (!_allocatedTlsIndices.Contains(tlsIndex))
		{
			_logger.LogWarning("[ProcessEnv] TlsGetValue: invalid TLS index={TlsIndex}", tlsIndex);
			return 0;
		}

		// Get TLS storage for current thread
		if (_threadLocalStorage.TryGetValue(_currentThreadId, out var threadTls) &&
		    threadTls.TryGetValue(tlsIndex, out var value))
		{
			_logger.LogInformation("[ProcessEnv] TlsGetValue: threadId={CurrentThreadId} index={TlsIndex} value=0x{Value:X8}", _currentThreadId, tlsIndex, value);
			return value;
		}

		_logger.LogInformation("[ProcessEnv] TlsGetValue: threadId={CurrentThreadId} index={TlsIndex} not set, returning 0", _currentThreadId, tlsIndex);
		return 0;
	}

	public bool TlsFree(uint tlsIndex)
	{
		if (!_allocatedTlsIndices.Remove(tlsIndex))
		{
			_logger.LogWarning("[ProcessEnv] TlsFree: invalid TLS index={TlsIndex}", tlsIndex);
			return false;
		}

		// Remove from all threads
		foreach (var threadTls in _threadLocalStorage.Values)
		{
			threadTls.Remove(tlsIndex);
		}

		_logger.LogInformation("[ProcessEnv] TlsFree: freed index={TlsIndex}", tlsIndex);
		return true;
	}

	// Registry support methods
	public uint RegOpenKey(string path)
	{
		var handle = _nextRegistryHandle++;
		var key = new VirtualRegistryKey { Path = path };
		_registryKeys[handle] = key;
		_logger.LogInformation("[ProcessEnv] RegOpenKey: path=\"{Path}\" handle=0x{Handle:X8}", path, handle);
		return handle;
	}

	public bool RegQueryValue(uint handle, string valueName, out object? value)
	{
		value = null;
		if (!_registryKeys.TryGetValue(handle, out var key))
		{
			_logger.LogWarning("[ProcessEnv] RegQueryValue: invalid handle=0x{Handle:X8}", handle);
			return false;
		}

		if (key.Values.TryGetValue(valueName, out value))
		{
			_logger.LogInformation("[ProcessEnv] RegQueryValue: handle=0x{Handle:X8} name=\"{ValueName}\" value={Value}",
				handle, valueName, value);
			return true;
		}

		_logger.LogInformation("[ProcessEnv] RegQueryValue: handle=0x{Handle:X8} name=\"{ValueName}\" not found",
			handle, valueName);
		return false;
	}

	public bool RegCloseKey(uint handle)
	{
		if (_registryKeys.Remove(handle))
		{
			_logger.LogInformation("[ProcessEnv] RegCloseKey: handle=0x{Handle:X8}", handle);
			return true;
		}

		_logger.LogWarning("[ProcessEnv] RegCloseKey: invalid handle=0x{Handle:X8}", handle);
		return false;
	}

	// Dialog state management methods

	/// <summary>
	/// Initializes dialog state for a dialog box.
	/// </summary>
	public void InitializeDialogState(uint hDlg)
	{
		_dialogStates[hDlg] = new DialogState();
		_logger.LogDebug("[ProcessEnv] InitializeDialogState: hDlg=0x{HDlg:X8}", hDlg);
	}

	/// <summary>
	/// Sets the result for a dialog and marks it as ended.
	/// Called by EndDialog.
	/// </summary>
	public bool SetDialogResult(uint hDlg, uint result)
	{
		if (_dialogStates.TryGetValue(hDlg, out var state))
		{
			state.Result = result;
			state.IsEnded = true;
			_logger.LogInformation("[ProcessEnv] SetDialogResult: hDlg=0x{HDlg:X8} result={Result}", hDlg, result);
			return true;
		}

		_logger.LogWarning("[ProcessEnv] SetDialogResult: unknown dialog hDlg=0x{HDlg:X8}", hDlg);
		return false;
	}

	/// <summary>
	/// Checks if a dialog has been ended via EndDialog.
	/// </summary>
	public bool IsDialogEnded(uint hDlg)
	{
		return _dialogStates.TryGetValue(hDlg, out var state) && state.IsEnded;
	}

	/// <summary>
	/// Gets the result set by EndDialog.
	/// </summary>
	public uint GetDialogResult(uint hDlg)
	{
		if (_dialogStates.TryGetValue(hDlg, out var state))
		{
			return state.Result;
		}

		return 0;
	}

	/// <summary>
	/// Cleans up dialog state after the dialog is destroyed.
	/// </summary>
	public void CleanupDialogState(uint hDlg)
	{
		if (_dialogStates.Remove(hDlg))
		{
			_logger.LogDebug("[ProcessEnv] CleanupDialogState: hDlg=0x{HDlg:X8}", hDlg);
		}
	}

	/// <summary>
	/// Stores control information for a dialog.
	/// </summary>
	public void StoreControlInfo(uint hDlg, int controlId, uint controlHandle, Win32.DialogItem controlInfo)
	{
		if (_dialogStates.TryGetValue(hDlg, out var state))
		{
			state.ControlHandles[controlId] = controlHandle;
			state.ControlInfo[controlId] = controlInfo;
			_logger.LogDebug("[ProcessEnv] StoreControlInfo: hDlg=0x{HDlg:X8} controlId={ControlId} handle=0x{ControlHandle:X8}", hDlg, controlId, controlHandle);
		}
	}

	/// <summary>
	/// Gets a control handle by its ID for a dialog.
	/// </summary>
	public uint GetDialogControlHandle(uint hDlg, int controlId)
	{
		if (_dialogStates.TryGetValue(hDlg, out var state) && state.ControlHandles.TryGetValue(controlId, out var handle))
		{
			_logger.LogDebug("[ProcessEnv] GetDialogControlHandle: hDlg=0x{HDlg:X8} controlId={ControlId} -> 0x{Handle:X8}", hDlg, controlId, handle);
			return handle;
		}
		_logger.LogDebug("[ProcessEnv] GetDialogControlHandle: hDlg=0x{HDlg:X8} controlId={ControlId} -> NOT FOUND", hDlg, controlId);
		return 0;
	}

	/// <summary>
	/// Sets a window property value for SetWindowLongA.
	/// </summary>
	public void SetWindowProperty(uint hwnd, int index, uint value)
	{
		_windowProperties[(hwnd, index)] = value;
		_logger.LogDebug("[ProcessEnv] SetWindowProperty: HWND=0x{Hwnd:X8} index={Index} value=0x{Value:X8}", hwnd, index, value);
	}

	/// <summary>
	/// Gets a window property value for GetWindowLongA.
	/// </summary>
	public uint GetWindowProperty(uint hwnd, int index)
	{
		if (_windowProperties.TryGetValue((hwnd, index), out var value))
		{
			_logger.LogDebug("[ProcessEnv] GetWindowProperty: HWND=0x{Hwnd:X8} index={Index} -> 0x{Value:X8}", hwnd, index, value);
			return value;
		}

		// Return appropriate default values based on index
		// Common GWL_* constants are defined in NativeTypes.WindowLong

		if (_windows.TryGetValue(hwnd, out var windowInfo))
		{
			return index switch
			{
				(int)NativeTypes.WindowLong.GWL_STYLE => windowInfo.Style,
				(int)NativeTypes.WindowLong.GWL_EXSTYLE => windowInfo.ExStyle,
				(int)NativeTypes.WindowLong.GWL_HWNDPARENT => windowInfo.Parent,
				(int)NativeTypes.WindowLong.GWL_HINSTANCE => windowInfo.Instance,
				(int)NativeTypes.WindowLong.GWL_ID => windowInfo.Menu, // For child windows, this is the control ID
				_ => 0
			};
		}

		_logger.LogDebug("[ProcessEnv] GetWindowProperty: HWND=0x{Hwnd:X8} index={Index} -> 0 (default)", hwnd, index);
		return 0;
	}

	/// <summary>
	/// Sets text for a dialog control.
	/// </summary>
	public void SetDialogControlText(uint hDlg, int controlId, string text)
	{
		if (_dialogStates.TryGetValue(hDlg, out var state))
		{
			state.ControlText[controlId] = text;
			_logger.LogDebug("[ProcessEnv] SetDialogControlText: hDlg=0x{HDlg:X8} controlId={ControlId} text='{Text}'", hDlg, controlId, text);
		}
	}

	/// <summary>
	/// Gets text for a dialog control.
	/// </summary>
	public string? GetDialogControlText(uint hDlg, int controlId)
	{
		if (_dialogStates.TryGetValue(hDlg, out var state))
		{
			if (state.ControlText.TryGetValue(controlId, out var text))
			{
				_logger.LogDebug("[ProcessEnv] GetDialogControlText: hDlg=0x{HDlg:X8} controlId={ControlId} -> '{Text}'", hDlg, controlId, text);
				return text;
			}
		}

		_logger.LogDebug("[ProcessEnv] GetDialogControlText: hDlg=0x{HDlg:X8} controlId={ControlId} -> null", hDlg, controlId);
		return null;
	}

	/// <summary>
	/// Subscribe to UI events from rendering and input backends.
	/// This method should be called after backends are initialized to enable event-driven UI.
	/// Automatically prevents duplicate subscriptions by tracking subscribed backends.
	/// </summary>
	/// <param name="renderingBackend">The rendering backend to subscribe to</param>
	/// <param name="inputBackend">The input backend to subscribe to</param>
	public void SubscribeToUIEvents(IRenderingBackend? renderingBackend, IInputBackend? inputBackend)
	{
		if (renderingBackend != null && !_subscribedRenderingBackends.Contains(renderingBackend))
		{
			renderingBackend.UIEvent += OnUIEvent;
			_subscribedRenderingBackends.Add(renderingBackend);
			_logger.LogInformation("[ProcessEnv] Subscribed to rendering backend UI events");
		}

		if (inputBackend != null && !_subscribedInputBackends.Contains(inputBackend))
		{
			inputBackend.UIEvent += OnUIEvent;
			_subscribedInputBackends.Add(inputBackend);
			_logger.LogInformation("[ProcessEnv] Subscribed to input backend UI events");
		}
	}

	/// <summary>
	/// Unsubscribe from UI events from rendering and input backends.
	/// </summary>
	/// <param name="renderingBackend">The rendering backend to unsubscribe from</param>
	/// <param name="inputBackend">The input backend to unsubscribe from</param>
	public void UnsubscribeFromUIEvents(IRenderingBackend? renderingBackend, IInputBackend? inputBackend)
	{
		if (renderingBackend != null && _subscribedRenderingBackends.Contains(renderingBackend))
		{
			renderingBackend.UIEvent -= OnUIEvent;
			_subscribedRenderingBackends.Remove(renderingBackend);
		}

		if (inputBackend != null && _subscribedInputBackends.Contains(inputBackend))
		{
			inputBackend.UIEvent -= OnUIEvent;
			_subscribedInputBackends.Remove(inputBackend);
		}
	}

	/// <summary>
	/// Process events from all subscribed rendering and input backends.
	/// This should be called regularly to keep windows responsive and process input.
	/// </summary>
	public void ProcessAllBackendEvents()
	{
		// Process events from all subscribed rendering backends (e.g., GLFW windows)
		foreach (var renderingBackend in _subscribedRenderingBackends)
		{
			try
			{
				renderingBackend.ProcessEvents();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[ProcessEnv] Error processing rendering backend events");
			}
		}

		// Process events from all subscribed input backends
		foreach (var inputBackend in _subscribedInputBackends)
		{
			try
			{
				inputBackend.ProcessEvents();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[ProcessEnv] Error processing input backend events");
			}
		}

		// Also process the legacy InputBackend property if set and not already in subscribed list
		if (InputBackend != null && !_subscribedInputBackends.Contains(InputBackend))
		{
			try
			{
				InputBackend.ProcessEvents();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[ProcessEnv] Error processing input backend events");
			}
		}
	}

	/// <summary>
	/// Handle UI events from rendering/input backends and translate them to Win32 messages.
	/// This is the event handler that gets called when backends raise UI events.
	/// </summary>
	private void OnUIEvent(object? sender, UIEventArgs e)
	{
		// Get the target window handle (use first window if not specified)
		var targetHwnd = e.WindowHandle;
		if (targetHwnd == 0 && _windows.Count > 0)
		{
			// Default to the first created window
			targetHwnd = _windows.Keys.First();
		}

		if (targetHwnd == 0)
		{
			// No window to send message to
			_logger.LogDebug("[ProcessEnv] OnUIEvent: No target window for event {EventType}", e.EventType);
			return;
		}

		// Translate UI event to Win32 message
		uint message;
		uint wParam;
		uint lParam;

		switch (e.EventType)
		{
			case UIEventType.MouseMove:
				message = 0x0200; // WM_MOUSEMOVE
				wParam = 0; // No button flags for now
				// Pack coordinates into lParam: LOWORD = x, HIWORD = y
				// Handle signed coordinates properly by masking to 16 bits
				lParam = (uint)(((e.MouseY & 0xFFFF) << 16) | (e.MouseX & 0xFFFF));
				break;

			case UIEventType.MouseButtonDown:
				// Translate button ID to Win32 message
				message = e.WParam switch
				{
					1 => 0x0201, // WM_LBUTTONDOWN
					2 => 0x0204, // WM_RBUTTONDOWN
					3 => 0x0207, // WM_MBUTTONDOWN
					_ => 0x0201  // Default to left button
				};
				wParam = 0x0001; // MK_LBUTTON flag
				// Pack coordinates: LOWORD = x, HIWORD = y
				lParam = (uint)(((e.MouseY & 0xFFFF) << 16) | (e.MouseX & 0xFFFF));
				break;

			case UIEventType.MouseButtonUp:
				message = e.WParam switch
				{
					1 => 0x0202, // WM_LBUTTONUP
					2 => 0x0205, // WM_RBUTTONUP
					3 => 0x0208, // WM_MBUTTONUP
					_ => 0x0202
				};
				wParam = 0;
				// Pack coordinates: LOWORD = x, HIWORD = y
				lParam = (uint)(((e.MouseY & 0xFFFF) << 16) | (e.MouseX & 0xFFFF));
				break;

			case UIEventType.KeyDown:
				message = 0x0100; // WM_KEYDOWN
				wParam = (uint)e.KeyCode;
				// lParam encoding for WM_KEYDOWN (simplified):
				// Bits 0-15: Repeat count (1)
				// Bits 16-23: Scan code (0 for now, would need virtual key to scan code translation)
				// Bit 24: Extended key flag (0)
				// Bits 25-28: Reserved (0)
				// Bit 29: Context code (0 = not ALT key)
				// Bit 30: Previous key state (0 = was up)
				// Bit 31: Transition state (0 = being pressed)
				lParam = 0x00000001; // Simplified: repeat count = 1, rest = 0
				break;

			case UIEventType.KeyUp:
				message = 0x0101; // WM_KEYUP
				wParam = (uint)e.KeyCode;
				// lParam encoding for WM_KEYUP:
				// Bits 0-15: Repeat count (1)
				// Bit 30: Previous key state (1 = was down)
				// Bit 31: Transition state (1 = being released)
				lParam = 0xC0000001; // Repeat=1, Previous=1, Transition=1
				break;

			case UIEventType.WindowResize:
				message = 0x0005; // WM_SIZE
				wParam = 0; // SIZE_RESTORED
				lParam = (e.LParam << 16) | e.WParam; // HIWORD=height, LOWORD=width
				break;

			case UIEventType.WindowClose:
				message = 0x0010; // WM_CLOSE
				wParam = 0;
				lParam = 0;
				break;

			case UIEventType.WindowActivate:
				// Send WM_ACTIVATE first
				message = 0x0006; // WM_ACTIVATE
				wParam = 0x0001; // WA_ACTIVE
				lParam = 0;
				PostMessage(targetHwnd, message, wParam, lParam);
				
				// Then send WM_ACTIVATEAPP for application-level activation
				message = 0x001C; // WM_ACTIVATEAPP
				wParam = 1; // TRUE - activating
				lParam = GetCurrentThreadId(); // Thread ID of the thread being activated
				break;

			case UIEventType.WindowDeactivate:
				// Send WM_ACTIVATE first
				message = 0x0006; // WM_ACTIVATE
				wParam = 0x0000; // WA_INACTIVE
				lParam = 0;
				PostMessage(targetHwnd, message, wParam, lParam);
				
				// Then send WM_ACTIVATEAPP for application-level deactivation
				message = 0x001C; // WM_ACTIVATEAPP
				wParam = 0; // FALSE - deactivating
				lParam = GetCurrentThreadId(); // Thread ID of the thread being deactivated
				break;

			default:
				_logger.LogWarning("[ProcessEnv] OnUIEvent: Unknown event type {EventType}", e.EventType);
				return;
		}

		// Post the translated message to the queue
		var success = PostMessage(targetHwnd, message, wParam, lParam);
		if (success)
		{
			_logger.LogDebug("[ProcessEnv] OnUIEvent: Translated {EventType} to WM_{Message:X4} for HWND=0x{TargetHwnd:X8}",
				e.EventType, message, targetHwnd);
		}
		else
		{
			_logger.LogWarning("[ProcessEnv] OnUIEvent: Failed to post message WM_{Message:X4} for event {EventType}",
				message, e.EventType);
		}
	}

	/// <summary>
	/// Internal class to track dialog state.
	/// </summary>
	private class DialogState
	{
		public bool IsEnded { get; set; }
		public uint Result { get; set; }
		// Storage for dialog control text: Key = control ID, Value = text
		public Dictionary<int, string> ControlText { get; } = new();
		// Storage for dialog control handles: Key = control ID, Value = handle
		public Dictionary<int, uint> ControlHandles { get; } = new();
		// Storage for dialog control info: Key = control ID, Value = DialogItem
		public Dictionary<int, Win32.DialogItem> ControlInfo { get; } = new();
	}

	/// <summary>
	/// Represents a memory block for the free-list allocator.
	/// </summary>
	private class MemoryBlock
	{
		public uint Address { get; set; }
		public uint Size { get; set; }
		
		/// <summary>
		/// Indicates whether this block is free or allocated.
		/// Currently used for debugging and validation purposes.
		/// In the future, this could be used to maintain a single unified list
		/// instead of separate _freeList and _allocatedBlocks lists.
		/// </summary>
		public bool IsFree { get; set; }

		public MemoryBlock(uint address, uint size, bool isFree = true)
		{
			Address = address;
			Size = size;
			IsFree = isFree;
		}

		public uint EndAddress
		{
			get
			{
				ulong end = (ulong)Address + (ulong)Size;
				return end > uint.MaxValue ? uint.MaxValue : (uint)end;
			}
		}
	}
}
