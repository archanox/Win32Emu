using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Win32Emu.Rendering;

namespace Win32Emu.Win32;

public class ProcessEnvironment
{
	private readonly VirtualMemory _vm;
	private readonly IEmulatorHost? _host;
	private readonly ILogger _logger;
	private uint _allocPtr;
	private bool _exitRequested;
	private string _executablePath = string.Empty;

	public ProcessEnvironment(VirtualMemory vm, uint heapBase = 0x01000000, IEmulatorHost? host = null, ILogger? logger = null)
	{
		_vm = vm;
		_host = host;
		_logger = logger ?? NullLogger.Instance;
		_allocPtr = heapBase;
	}

	// SDL3 backends for audio and input
	public Sdl3AudioBackend? AudioBackend { get; set; }
	public Sdl3InputBackend? InputBackend { get; set; }

	public uint CommandLinePtr { get; private set; }
	public uint ModuleFileNamePtr { get; private set; }
	public uint ModuleFileNameLength { get; private set; }
	public bool ExitRequested => _exitRequested;
	public string ExecutablePath => _executablePath;

	// Default standard handles (pseudo values)
	public uint StdInputHandle { get; set; } = 0x00000001;
	public uint StdOutputHandle { get; set; } = 0x00000002;
	public uint StdErrorHandle { get; set; } = 0x00000003;

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
	private uint _nextWindowHandle = 0x00010000; // Window handles typically start low

	// Message queue management
	private bool _hasQuitMessage;
	private int _quitExitCode;

	// Environment variables (emulated, not from system)
	private readonly Dictionary<string, string> _environmentVariables = new();

	public void InitializeStrings(string exePath, string[] args)
	{
		_executablePath = exePath;
		var cmdLine = string.Join(" ", new[] { exePath }.Concat(args.Skip(1)));
		CommandLinePtr = WriteAnsiString(cmdLine + '\0');
		ModuleFileNamePtr = WriteAnsiString(exePath + '\0');
		ModuleFileNameLength = (uint)exePath.Length;

		// Initialize with some default environment variables
		InitializeDefaultEnvironmentVariables();
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
		Diagnostics.Diagnostics.LogDebug($"ReadAnsiString addr=0x{addr:X8} result='{result}'");
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
		Diagnostics.Diagnostics.LogDebug($"ReadAnsiString addr=0x{addr:X8} length={maxLength} result='{result}'");
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

	public byte[] MemReadBytes(uint addr, int count) => _vm.GetSpan(addr, count);
	public byte MemRead8(uint addr) => _vm.Read8(addr);
	public void MemWriteBytes(uint addr, ReadOnlySpan<byte> data)
	{
		_vm.WriteBytes(addr, data);
		try { Diagnostics.Diagnostics.LogMemWrite(addr, data.Length, data.ToArray()); } catch { }
	}
	public uint MemRead32(uint addr) => _vm.Read32(addr);
	public void MemWrite32(uint addr, uint value) => _vm.Write32(addr, value);
	public void MemWrite16(uint addr, ushort value) => _vm.Write16(addr, value);
	public ushort MemRead16(uint addr) => _vm.Read16(addr);
	public void MemWrite64(uint addr, ulong value) => _vm.Write64(addr, value);
	public void MemZero(uint addr, uint size) => _vm.WriteBytes(addr, new byte[size]);

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
			
			Console.WriteLine($"[ProcessEnv] Loaded PE image: {imagePath} at 0x{handle:X8}");
			return handle;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[ProcessEnv] Failed to load PE image {imagePath}: {ex.Message}");
			// Fall back to tracking without loading
			return LoadModule(normalizedName);
		}
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
			return SimpleAlloc(dwBytes);
		}

		var size = AlignUp(dwBytes == 0 ? 1u : dwBytes, 16);
		if (hs.Current + size <= hs.Limit)
		{
			var ptr = hs.Current;
			hs = hs with { Current = hs.Current + size };
			_heaps[hHeap] = hs;
			return ptr;
		}

		return SimpleAlloc(dwBytes);
	}

	public uint HeapFree(uint hHeap, uint lpMem) => 1;

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

	public bool RegisterWindowClass(string className, WindowClassInfo classInfo)
	{
		if (_windowClasses.ContainsKey(className))
		{
			Console.WriteLine($"[ProcessEnv] Window class '{className}' already registered");
			return false;
		}

		_windowClasses[className] = classInfo;
		Console.WriteLine($"[ProcessEnv] Registered window class: {className}");
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

	public uint CreateWindow(string className, string windowName, uint style, uint exStyle,
		int x, int y, int width, int height, uint parent, uint menu, uint instance, uint param)
	{
		if (!_windowClasses.ContainsKey(className))
		{
			Console.WriteLine($"[ProcessEnv] CreateWindow failed: Window class '{className}' not registered");
			return 0;
		}

		var handle = _nextWindowHandle;
		_nextWindowHandle += 4;

		var windowInfo = new WindowInfo(
			handle, className, windowName, style, exStyle,
			x, y, width, height, parent, menu, instance, param
		);

		_windows[handle] = windowInfo;
		Console.WriteLine($"[ProcessEnv] Created window: HWND=0x{handle:X8} Class='{className}' Title='{windowName}'");

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
			Parent = parent
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
			Console.WriteLine($"[ProcessEnv] Destroyed window: HWND=0x{hwnd:X8}");
			return true;
		}
		return false;
	}

	// Message queue management
	public void PostQuitMessage(int exitCode)
	{
		_hasQuitMessage = true;
		_quitExitCode = exitCode;
		Console.WriteLine($"[ProcessEnv] PostQuitMessage: exitCode={exitCode}");
	}

	public bool HasQuitMessage()
	{
		return _hasQuitMessage;
	}

	public int GetQuitExitCode()
	{
		return _quitExitCode;
	}
}