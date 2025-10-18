using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.Rendering;
using Win32Emu.Win32.COM;
using Win32Emu.VirtualFileSystem;

namespace Win32Emu.Win32;

public class ProcessEnvironment
{
	private readonly VirtualMemory _vm;
	private readonly IEmulatorHost? _host;
	private readonly ILogger _logger;
	private uint _allocPtr;
	private bool _exitRequested;
	private string _executablePath = string.Empty;
	private string _currentDirectory = @"C:\"; // Default to C:\ root

	// COM vtable dispatcher
	private ComVtableDispatcher? _comDispatcher;

	// Virtual File System
	private IVirtualFileSystem? _vfs;
	
	// Expose VirtualMemory for use by Win32 API implementations
	public VirtualMemory Memory => _vm;
	
	public ProcessEnvironment(VirtualMemory vm, uint heapBase = 0x01000000, IEmulatorHost? host = null, ILogger? logger = null)
	{
		_vm = vm;
		_host = host;
		_logger = logger ?? NullLogger.Instance;
		_allocPtr = heapBase;
		_comDispatcher = new ComVtableDispatcher(this, _logger);
		
		// Pre-register standard Windows control classes
		RegisterStandardControlClasses();
	}
	
	// COM vtable dispatcher access
	public ComVtableDispatcher ComDispatcher => _comDispatcher ?? throw new InvalidOperationException("COM dispatcher not initialized");

	// Virtual File System access
	/// <summary>
	/// Gets the current virtual file system instance for this process environment.
	/// </summary>
	/// <remarks>
	/// Returns <c>null</c> if the virtual file system has not been initialized.
	/// </remarks>
	public IVirtualFileSystem? VirtualFileSystem => _vfs;

	/// <summary>
	/// Initializes the virtual file system with the specified base directory.
	/// </summary>
	/// <param name="baseDirectory">Base directory containing game files (read-only)</param>
	/// <param name="overlayDirectory">Optional overlay directory for writable files. If null, a temporary directory is used.</param>
	public void InitializeVirtualFileSystem(string baseDirectory, string? overlayDirectory = null)
	{
		_vfs = new LayeredVirtualFileSystem(baseDirectory, overlayDirectory, _logger);
		_logger.LogInformation("[ProcessEnv] Virtual File System initialized with base: {BaseDirectory}", baseDirectory);
		
		// If executable path is already set, virtualize it to Windows-style path
		if (!string.IsNullOrEmpty(_executablePath))
		{
			var virtualizedPath = _vfs.ToWindowsPath(_executablePath);
			if (virtualizedPath != _executablePath)
			{
				_logger.LogInformation("[ProcessEnv] Virtualizing executable path: {Original} -> {Virtualized}", 
					_executablePath, virtualizedPath);
				
				// Update the executable path and module file name
				_executablePath = virtualizedPath;
				ModuleFileNamePtr = WriteAnsiString(virtualizedPath + '\0');
				ModuleFileNameLength = (uint)virtualizedPath.Length;
				
				// Also update command line if it was already set
				if (CommandLinePtr != 0)
				{
					// Re-read the old command line to extract args
					var oldCmdLine = ReadAnsiString(CommandLinePtr);
					// Parse to extract args (skip the first quoted part which is the exe path)
					var args = new List<string>();
					var inQuote = false;
					var current = new System.Text.StringBuilder();
					var skipFirst = true;
					
					foreach (var ch in oldCmdLine)
					{
						if (ch == '"')
						{
							inQuote = !inQuote;
							if (!inQuote && skipFirst)
							{
								skipFirst = false;
								current.Clear();
								continue;
							}
						}
						else if (ch == ' ' && !inQuote)
						{
							if (current.Length > 0 && !skipFirst)
							{
								args.Add(current.ToString());
								current.Clear();
							}
						}
						else if (!skipFirst)
						{
							current.Append(ch);
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
			}
		}
	}

	// SDL3 backends for audio and input
	public IAudioBackend? AudioBackend { get; set; }
	public IInputBackend? InputBackend { get; set; }

	public uint CommandLinePtr { get; private set; }
	public uint ModuleFileNamePtr { get; private set; }
	public uint ModuleFileNameLength { get; private set; }
	public bool ExitRequested => _exitRequested;
	public string ExecutablePath => _executablePath;
	public string CurrentDirectory
	{
		get => _currentDirectory;
		set => _currentDirectory = value ?? @"C:\";
	}

	// Console state
	private bool _hasConsole = false;
	public bool HasConsole => _hasConsole;

	// Default standard handles (NULL for GUI apps without console)
	// Console apps would set these to actual handles via AllocConsole/AttachConsole
	public uint StdInputHandle { get; set; } = 0x00000000; // NULL - no console by default
	public uint StdOutputHandle { get; set; } = 0x00000000; // NULL - no console by default
	public uint StdErrorHandle { get; set; } = 0x00000000; // NULL - no console by default

	// Simple handle table for host resources (files etc.)
	private readonly Dictionary<uint, object> _handles = new();
	private uint _nextHandle = 0x00001000; // avoid low values used as sentinels

	// Loaded modules tracking
	private readonly Dictionary<string, uint> _loadedModules = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<string, LoadedImage> _loadedImages = new(StringComparer.OrdinalIgnoreCase);
	private uint _nextModuleHandle = 0x10000000;

	// Emulated module exports tracking (for GetProcAddress on system DLLs)
	private readonly Dictionary<uint, (string module, string export)> _syntheticExports = new();
	private uint _nextSyntheticExport = 0x0E000000; // Synthetic export base address

	// Window management
	private readonly Dictionary<uint, WindowInfo> _windows = new();
	private readonly Dictionary<string, WindowClassInfo> _windowClasses = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<uint, string> _atomToClassName = new(); // Maps atoms to class names
	private uint _nextWindowHandle = 0x00010000; // Window handles typically start low
	
	// Window property storage for SetWindowLongA/GetWindowLongA
	// Key: (hwnd, index), Value: property value
	private readonly Dictionary<(uint, int), uint> _windowProperties = new();

	// Message queue management
	private bool _hasQuitMessage;
	private int _quitExitCode;

	// Dialog state management
	private readonly Dictionary<uint, DialogState> _dialogStates = new();

	// Message queue with Channels
	private readonly Channel<QueuedMessage> _messageQueue = Channel.CreateUnbounded<QueuedMessage>();
	
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
	private uint _tebAddress;
	public uint TebAddress => _tebAddress;

	// TLS (Thread Local Storage) support
	private readonly Dictionary<uint, Dictionary<uint, uint>> _threadLocalStorage = new(); // threadId -> (tlsIndex -> value)
	private readonly HashSet<uint> _allocatedTlsIndices = new();
	private uint _nextTlsIndex = 0;

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
		if (_vfs != null)
		{
			effectivePath = _vfs.ToWindowsPath(exePath);
			if (effectivePath != exePath)
			{
				_logger.LogInformation("[ProcessEnv] Virtualizing executable path: {Original} -> {Virtualized}", 
					exePath, effectivePath);
			}
		}
		
		_executablePath = effectivePath;
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
		_tebAddress = SimpleAlloc(0x1000); // Allocate 4KB for TEB
		MemZero(_tebAddress, 0x1000);
		_logger.LogInformation("[ProcessEnv] TEB allocated at 0x{TebAddress:X8}", _tebAddress);

		// The TEB contains a self-referential pointer at offset 0x18
		// This is the linear address of the TEB
		MemWrite32(_tebAddress + 0x18, _tebAddress);

		// Allocate a dummy PEB (Process Environment Block)
		var pebAddress = SimpleAlloc(0x1000);
		MemZero(pebAddress, 0x1000);
		_logger.LogInformation("[ProcessEnv] PEB allocated at 0x{PebAddress:X8}", pebAddress);
        
		// The TEB points to the PEB at offset 0x30
		MemWrite32(_tebAddress + 0x30, pebAddress);
        
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

	public void RequestExit() => _exitRequested = true;

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
		_vm.WriteBytes(addr, bytes);
		Diagnostics.Diagnostics.LogMemWrite(addr, bytes.Length, bytes);
		return addr;
	}

	public uint WriteUnicodeString(string s)
	{
		var bytes = Encoding.Unicode.GetBytes(s);
		var addr = SimpleAlloc((uint)bytes.Length);
		_vm.WriteBytes(addr, bytes);
		Diagnostics.Diagnostics.LogMemWrite(addr, bytes.Length, bytes);
		return addr;
	}

	public void WriteAnsiStringAt(uint addr, string s, bool nullTerminate = true)
	{
		var bytes = Encoding.ASCII.GetBytes(nullTerminate ? s + "\0" : s);
		_vm.WriteBytes(addr, bytes);
		Diagnostics.Diagnostics.LogMemWrite(addr, bytes.Length, bytes);
	}

	public string ReadAnsiString(uint addr)
	{
		var buf = new List<byte>();
		var p = addr;
		for (;;)
		{
			var b = _vm.Read8(p++);
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
			buf[i] = _vm.Read8(addr + (uint)i);
		}

		var result = Encoding.ASCII.GetString(buf);
		_logger.LogDebug("[ProcessEnv] ReadAnsiString addr=0x{Addr:X8} length={MaxLength} result='{Result}'", addr, maxLength, result);
		return result;
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
		_vm.WriteBytes(addr, bytes);
		
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
		_vm.WriteBytes(addr, bytes);
		
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

	public byte[] MemReadBytes(uint addr, int count) => _vm.GetSpan(addr, count);
	public byte MemRead8(uint addr) => _vm.Read8(addr);
	public void MemWriteBytes(uint addr, ReadOnlySpan<byte> data)
	{
		_vm.WriteBytes(addr, data);
		try { Diagnostics.Diagnostics.LogMemWrite(addr, data.Length, data.ToArray()); } catch { }
	}
	public uint MemRead32(uint addr) => _vm.Read32(addr);
	public void MemWrite32(uint addr, uint value) => _vm.Write32(addr, value);
	public void MemWriteBytes(uint addr, byte[] bytes) => _vm.WriteBytes(addr, bytes);
	public void MemWrite16(uint addr, ushort value) => _vm.Write16(addr, value);
	public void MemWrite8(uint addr, byte value) => _vm.Write8(addr, value);
	public ushort MemRead16(uint addr) => _vm.Read16(addr);
	public void MemWrite64(uint addr, ulong value) => _vm.Write64(addr, value);
	public void MemZero(uint addr, uint size) => _vm.WriteBytes(addr, new byte[size]);

	// Write an unmanaged struct to emulated memory
	public unsafe void MemWriteStruct<T>(uint addr, ref T value) where T : unmanaged
	{
		var size = sizeof(T);
		var bytes = new byte[size];
		fixed (T* ptr = &value)
		{
			Marshal.Copy((nint)ptr, bytes, 0, size);
		}
		_vm.WriteBytes(addr, bytes);
		try { Diagnostics.Diagnostics.LogMemWrite(addr, bytes.Length, bytes); } catch { }
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
		_logger.LogInformation("[ProcessEnv] Registered main executable: {ImagePath} at 0x{BaseAddress:X8}", imagePath, image.BaseAddress);
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
		foreach (var kvp in _loadedImages)
		{
			if (kvp.Value.BaseAddress == moduleHandle)
			{
				return kvp.Value.FilePath;
			}
		}

		// Search loaded modules for a matching handle and return normalized name
		foreach (var kvp in _loadedModules)
		{
			if (kvp.Value == moduleHandle)
			{
				return kvp.Key; // normalized name
			}
		}

		// If not found, return null
		return null;
	}

	/// <summary>
	/// Try to get a loaded PE image by its module handle.
	/// </summary>
	public bool TryGetLoadedImage(uint moduleHandle, out LoadedImage? image)
	{
		// Search loaded images by base address
		foreach (var kvp in _loadedImages)
		{
			if (kvp.Value.BaseAddress == moduleHandle)
			{
				image = kvp.Value;
				return true;
			}
		}

		image = null;
		return false;
	}

	/// <summary>
	/// Register a synthetic export for an emulated module.
	/// Returns a synthetic address that can be used to call this export.
	/// </summary>
	public uint RegisterSyntheticExport(string moduleName, string exportName)
	{
		var address = _nextSyntheticExport;
		_nextSyntheticExport += 0x10;
		_syntheticExports[address] = (moduleName.ToUpperInvariant(), exportName.ToUpperInvariant());
		
		// Create a stub at the synthetic address (INT3 for breakpoint interception)
		var stub = new byte[] { 0xCC, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 };
		_vm.WriteBytes(address, stub);
		
		return address;
	}

	/// <summary>
	/// Try to get the module and export name for a synthetic export address.
	/// </summary>
	public bool TryGetSyntheticExport(uint address, out string moduleName, out string exportName)
	{
		if (_syntheticExports.TryGetValue(address, out var export))
		{
			moduleName = export.module;
			exportName = export.export;
			return true;
		}

		moduleName = string.Empty;
		exportName = string.Empty;
		return false;
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
			var addr = SimpleAlloc(dwBytes);
			_heapAllocationSizes[addr] = dwBytes;
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

		var fallbackAddr = SimpleAlloc(dwBytes);
		_heapAllocationSizes[fallbackAddr] = dwBytes;
		return fallbackAddr;
	}

	public uint HeapFree(uint hHeap, uint lpMem)
	{
		// Remove allocation size tracking
		_heapAllocationSizes.Remove(lpMem);
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
		if (_hasConsole)
		{
			_logger.LogWarning("[ProcessEnvironment] AllocConsole called but console already exists");
			return false; // Console already exists
		}

		_hasConsole = true;
		
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
		if (!_hasConsole)
		{
			_logger.LogWarning("[ProcessEnvironment] FreeConsole called but no console exists");
			return false;
		}

		_hasConsole = false;
		
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

	// VirtualAlloc
	public uint VirtualAlloc(uint lpAddress, uint dwSize, uint flAllocationType, uint flProtect)
	{
		var size = AlignUp(dwSize == 0 ? 1u : dwSize, 0x1000);
		if (lpAddress != 0)
		{
			if (lpAddress + size <= _vm.Size)
			{
				_vm.WriteBytes(lpAddress, new byte[size]);
			}

			return lpAddress;
		}

		var addr = AlignUp(_allocPtr, 0x1000);
		_allocPtr = addr + size;
		_vm.WriteBytes(addr, new byte[size]);
		return addr;
	}

	private static uint AlignUp(uint value, uint align) => (value + (align - 1)) & ~(align - 1);

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
		// Common window class names from Windows
		var standardClasses = new[]
		{
			"BUTTON",
			"EDIT",
			"STATIC",
			"LISTBOX",
			"COMBOBOX",
			"SCROLLBAR"
		};

		foreach (var className in standardClasses)
		{
			var classInfo = new WindowClassInfo(
				ClassName: className,
				Style: 0,
				WndProc: 0, // Standard controls have their own internal window procedures
				ClsExtra: 0,
				WndExtra: 0,
				HInstance: 0,
				HIcon: 0,
				HCursor: 0,
				HbrBackground: 0,
				MenuName: null
			);

			_windowClasses.TryAdd(className, classInfo);
			_logger.LogInformation("[ProcessEnv] Pre-registered standard control class: {ClassName}", className);
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
			return true;
		}

		_logger.LogWarning("[ProcessEnv] PostMessage: failed to queue MSG=0x{Message:X4}", message);
		return false;
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
	/// Try to get a message from the queue (blocking, synchronous with timeout)
	/// </summary>
	public QueuedMessage? GetMessageBlocking(uint hwnd, uint msgFilterMin, uint msgFilterMax, int timeoutMs = 100)
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

			// Wait for a message with timeout
			using var cts = new CancellationTokenSource(timeoutMs);
			var readTask = _messageQueue.Reader.ReadAsync(cts.Token).AsTask();
			
			// Wait for the task to complete (successfully or canceled)
			try
			{
				readTask.Wait(timeoutMs);
			}
			catch (AggregateException)
			{
				// Task was canceled or faulted, which is expected on timeout
			}
			
			if (readTask.IsCompletedSuccessfully)
			{
				message = readTask.Result;
				
				// Apply filters if specified
				if (hwnd != 0 && message.Hwnd != hwnd)
				{
					_messageQueue.Writer.TryWrite(message);
					return null;
				}

				if (msgFilterMin != 0 || msgFilterMax != 0)
				{
					if (message.Message < msgFilterMin || message.Message > msgFilterMax)
					{
						_messageQueue.Writer.TryWrite(message);
						return null;
					}
				}

				return message;
			}

			return null;
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "[ProcessEnv] GetMessageBlocking");
			return null;
		}
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

	// Thread management methods
	public uint GetCurrentThreadId()
	{
		return _currentThreadId;
	}

	public uint CreateThread()
	{
		// Simple thread emulation - just return a new thread ID
		// In this emulation, we don't actually create real threads
		var threadId = _nextThreadId++;
		
		// Initialize TLS storage for this thread
		_threadLocalStorage[threadId] = new Dictionary<uint, uint>();
		
		_logger.LogInformation("[ProcessEnv] CreateThread: new thread ID={ThreadId}", threadId);
		return threadId;
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
				NativeTypes.WindowLong.GWL_STYLE => windowInfo.Style,
				NativeTypes.WindowLong.GWL_EXSTYLE => windowInfo.ExStyle,
				NativeTypes.WindowLong.GWL_HWNDPARENT => windowInfo.Parent,
				NativeTypes.WindowLong.GWL_HINSTANCE => windowInfo.Instance,
				NativeTypes.WindowLong.GWL_ID => windowInfo.Menu, // For child windows, this is the control ID
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
	/// Internal class to track dialog state.
	/// </summary>
	private class DialogState
	{
		public bool IsEnded { get; set; }
		public uint Result { get; set; }
		// Storage for dialog control text: Key = control ID, Value = text
		public Dictionary<int, string> ControlText { get; } = new();
	}
}
