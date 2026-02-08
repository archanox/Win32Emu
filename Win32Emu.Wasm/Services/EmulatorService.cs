using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Win32Emu.VirtualFileSystem;
using Win32Emu.Wasm.Backend;

namespace Win32Emu.Wasm.Services;

/// <summary>
/// Service that manages the Win32Emu emulator lifecycle in a WASM environment.
/// Provides methods to load executables, start/stop emulation, and communicate
/// with the emulator from Blazor components.
/// </summary>
public class EmulatorService : IDisposable
{
	private readonly IJSRuntime _jsRuntime;
	private readonly ILoggerFactory _loggerFactory;
	private readonly ILogger<EmulatorService> _logger;
	private readonly VhdStorageService _vhdStorage;
	
	private Emulator? _emulator;
	private readonly WasmEmulatorHost _emulatorHost;
	private WasmBackendFactory? _backendFactory;
	private string? _currentVhdPath;
	private string? _currentExecutableVhdPath;
	private string? _currentVhdName;
	private int _vhdFileCount;
	private CancellationTokenSource? _emulationCts;
	private Task? _emulationTask;
	
	// Store event handlers for proper unsubscription
	private EventHandler<string>? _debugOutputHandler;
	private EventHandler<string>? _stdOutputHandler;
	
	// State
	private bool _isRunning;
	private bool _isPaused;
	private string? _loadedExecutableName;
	
	// Maximum depth for child process chains to prevent infinite loops
	private const int MaxChildProcessRecursionDepth = 10;
	private const long DefaultVhdSizeBytes = 512L * 1024 * 1024;
	
	// Events for UI updates
	public event EventHandler<string>? OnStdOutput;
	public event EventHandler<string>? OnDebugOutput;
	public event EventHandler<EmulatorStateChangedEventArgs>? OnStateChanged;
	
	public bool IsRunning => _isRunning;
	public bool IsPaused => _isPaused;
	public bool IsExecutableLoaded => _emulator?.LoadedImage != null;
	public string? LoadedExecutableName => _loadedExecutableName;
	
	/// <summary>
	/// Gets the WASM emulator host instance for registering to events.
	/// The host is initialized in the constructor and is never null.
	/// </summary>
	public WasmEmulatorHost EmulatorHost => _emulatorHost;
	
	/// <summary>
	/// Gets the number of instructions executed by the emulator.
	/// Note: In WASM mode, telemetry is not enabled, so this will return 0.
	/// This is a placeholder for future implementation.
	/// </summary>
	public ulong InstructionsExecuted => 0;
	
	/// <summary>
	/// Gets the number of files in the virtual file system.
	/// </summary>
	public int VfsFileCount => _vhdFileCount;
	
	/// <summary>
	/// Gets whether cache is enabled and loaded
	/// </summary>
	public bool IsCacheEnabled => _emulator?.Cpu is Win32Emu.Cpu.Jit.JitCpu jitCpu && jitCpu.SupportsJit;
	
	/// <summary>
	/// Gets the current CPU backend name
	/// </summary>
	public string CpuBackend
	{
		get
		{
			if (_emulator?.Cpu == null) return "None";
			return _emulator.Cpu switch
			{
				Win32Emu.Cpu.Jit.JitCpu => "JitCpu (Interpreter in WASM)",
				_ => _emulator.Cpu.GetType().Name
			};
		}
	}
	
	public EmulatorService(IJSRuntime jsRuntime, ILoggerFactory loggerFactory, VhdStorageService vhdStorage)
	{
		_jsRuntime = jsRuntime;
		_loggerFactory = loggerFactory;
		_logger = loggerFactory.CreateLogger<EmulatorService>();
		_vhdStorage = vhdStorage;
		
		// Initialize EmulatorHost early to ensure it's never null for event subscriptions
		_emulatorHost = new WasmEmulatorHost(_loggerFactory.CreateLogger<WasmEmulatorHost>());
		
		// Wire up host events to forward to service events
		_debugOutputHandler = (sender, message) => EmitDebugOutput(message);
		_stdOutputHandler = (sender, message) => EmitStdOutput(message);
		
		_emulatorHost.DebugOutputReceived += _debugOutputHandler;
		_emulatorHost.StdOutputReceived += _stdOutputHandler;
	}
	
	/// <summary>
	/// Load an executable from a byte array
	/// </summary>
	/// <param name="executableBytes">The raw bytes of the executable</param>
	/// <param name="fileName">The name of the executable file</param>
	/// <param name="additionalFiles">Optional dictionary of additional files (path -> bytes) for the VFS</param>
	/// <param name="programArgs">Optional command-line arguments for the program</param>
	/// <param name="force32BitStackOps">Force 32-bit operand size for stack operations in 32-bit mode</param>
	/// <param name="useCache">Enable cache loading from wwwroot/cache/ directory</param>
	/// <param name="enableInstructionAnalyzer">Enable instruction analyzer for debugging (runs in interpreter mode)</param>
	/// <param name="enableLegacyInstructionDecoding">Enable legacy instruction decoding (MPX, Cyrix, ALTINST, etc.)</param>
	/// <returns>True if loading succeeded</returns>
	public async Task<bool> LoadExecutableAsync(
		byte[] executableBytes, 
		string fileName,
		Dictionary<string, byte[]>? additionalFiles = null,
		string[]? programArgs = null,
		bool force32BitStackOps = true,
		bool useCache = true,
		bool enableInstructionAnalyzer = false,
		bool enableLegacyInstructionDecoding = false,
		uint? ansiCodePage = null,
		uint? oemCodePage = null)
	{
		try
		{
			await StopAsync();

			EmitDebugOutput($"Loading executable: {fileName} ({executableBytes.Length} bytes)");

			if (programArgs != null && programArgs.Length > 0)
			{
				EmitDebugOutput($"Program arguments: {string.Join(" ", programArgs)}");
			}

			_backendFactory ??= new WasmBackendFactory(_jsRuntime, _loggerFactory);

			var vfsFiles = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
			var exePath = $"WASM\\{fileName}";
			var normalizedExePath = NormalizeVfsPath(exePath);
			vfsFiles[normalizedExePath] = executableBytes;

			if (additionalFiles != null && additionalFiles.Count > 0)
			{
				EmitDebugOutput($"Adding {additionalFiles.Count} additional files to VHD...");

				var normalizedPaths = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
				foreach (var kvp in additionalFiles)
				{
					var normalizedKey = kvp.Key.Replace('/', '\\');
					if (!normalizedPaths.TryAdd(normalizedKey, kvp.Value))
					{
						EmitDebugOutput($"Warning: Duplicate file path detected (case-insensitive): {normalizedKey}");
					}
				}

				string? commonPrefix = null;
				var hasTopLevelFiles = false;

				foreach (var path in normalizedPaths.Keys)
				{
					var firstSlash = path.IndexOf('\\');
					if (firstSlash > 0)
					{
						var prefix = path[..firstSlash];
						if (commonPrefix == null)
						{
							commonPrefix = prefix;
						}
						else if (!string.Equals(commonPrefix, prefix, StringComparison.OrdinalIgnoreCase))
						{
							commonPrefix = null;
							break;
						}
					}
					else
					{
						hasTopLevelFiles = true;
					}
				}

				if (hasTopLevelFiles)
				{
					commonPrefix = null;
				}

				foreach (var kvp in normalizedPaths)
				{
					var vfsPath = kvp.Key;

					if (commonPrefix != null && vfsPath.StartsWith(commonPrefix + "\\", StringComparison.OrdinalIgnoreCase))
					{
						vfsPath = $"WASM{vfsPath.Substring(commonPrefix.Length)}";
					}
					else if (!vfsPath.StartsWith("WASM\\", StringComparison.OrdinalIgnoreCase))
					{
						vfsPath = $"WASM\\{vfsPath}";
					}

					vfsFiles[NormalizeVfsPath(vfsPath)] = kvp.Value;
				}

				EmitDebugOutput($"VHD file set initialized with {vfsFiles.Count} files");
			}

			var vhdName = Path.GetFileNameWithoutExtension(fileName);
			var vhdPath = await CreateVhdFromFilesAsync(vhdName, vfsFiles, normalizedExePath);

			_emulator?.Dispose();
			_emulator = new Emulator(_emulatorHost, _loggerFactory.CreateLogger<Emulator>(), null, _backendFactory);

			_loadedExecutableName = fileName;

			_emulator.LoadExecutable(
				$"C:{normalizedExePath}",
				programArgs,
				debugMode: false,
				reservedMemoryMb: 256,
				forceInterpreterMode: true,
				virtualDiskPath: vhdPath,
				preloadedBytes: null,
				customVirtualFileSystem: null,
				force32BitStackOps: force32BitStackOps,
				enableInstructionAnalyzer: enableInstructionAnalyzer,
				enableLegacyInstructionDecoding: enableLegacyInstructionDecoding,
				ansiCodePage: ansiCodePage,
				oemCodePage: oemCodePage);

			if (useCache)
			{
				_logger.LogInformation("[WASM] JitCpu runs in interpreter mode - cache loading not needed");
				EmitDebugOutput("[Cache] JitCpu uses interpreter mode in WASM - no cache needed");
			}

			await PersistCurrentVhdAsync();

			EmitDebugOutput($"Successfully loaded: {fileName}");
			EmitDebugOutput($"Entry point: 0x{_emulator.LoadedImage?.EntryPointAddress:X8}");
			EmitDebugOutput($"Image base: 0x{_emulator.LoadedImage?.BaseAddress:X8}");

			OnStateChanged?.Invoke(this, new EmulatorStateChangedEventArgs
			{
				IsLoaded = true,
				IsRunning = false,
				ExecutableName = fileName
			});

			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to load executable: {FileName}", fileName);
			EmitDebugOutput($"Failed to load executable: {ex.Message}");
			return false;
		}
	}

	public async Task<bool> LoadVhdFromLibraryAsync(string name, string[]? programArgs = null, bool force32BitStackOps = true, bool enableInstructionAnalyzer = false, bool enableLegacyInstructionDecoding = false, uint? ansiCodePage = null, uint? oemCodePage = null)
	{
		try
		{
			var image = await _vhdStorage.LoadAsync(name);
			if (image == null)
			{
				EmitDebugOutput($"[VHD] VHD not found: {name}");
				return false;
			}

			await StopAsync();

			var vhdDir = Path.Combine(Path.GetTempPath(), "Win32Emu_VHDs");
			Directory.CreateDirectory(vhdDir);
			var vhdPath = Path.Combine(vhdDir, $"{name}.vhd");
			await File.WriteAllBytesAsync(vhdPath, image.Data);

			_currentVhdPath = vhdPath;
			_currentExecutableVhdPath = image.ExecutablePath;
			_currentVhdName = name;
			_vhdFileCount = 0;

			_backendFactory ??= new WasmBackendFactory(_jsRuntime, _loggerFactory);

			_emulator?.Dispose();
			_emulator = new Emulator(_emulatorHost, _loggerFactory.CreateLogger<Emulator>(), null, _backendFactory);
			_loadedExecutableName = Path.GetFileName(image.ExecutablePath);

			_emulator.LoadExecutable(
				image.ExecutablePath,
				programArgs,
				debugMode: false,
				reservedMemoryMb: 256,
				forceInterpreterMode: true,
				virtualDiskPath: vhdPath,
				preloadedBytes: null,
				customVirtualFileSystem: null,
				force32BitStackOps: force32BitStackOps,
				enableInstructionAnalyzer: enableInstructionAnalyzer,
				enableLegacyInstructionDecoding: enableLegacyInstructionDecoding,
				ansiCodePage: ansiCodePage,
				oemCodePage: oemCodePage);

			await PersistCurrentVhdAsync(name);

			OnStateChanged?.Invoke(this, new EmulatorStateChangedEventArgs
			{
				IsLoaded = true,
				IsRunning = false,
				ExecutableName = _loadedExecutableName
			});

			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[VHD] Failed to load VHD {Name}", name);
			EmitDebugOutput($"[VHD] Failed to load {name}: {ex.Message}");
			return false;
		}
	}

	private async Task<string> CreateVhdFromFilesAsync(string vhdName, Dictionary<string, byte[]> files, string executablePath)
	{
		var vhdDir = Path.Combine(Path.GetTempPath(), "Win32Emu_VHDs");
		Directory.CreateDirectory(vhdDir);

		var vhdPath = Path.Combine(vhdDir, $"{vhdName}.vhd");
		if (File.Exists(vhdPath))
		{
			File.Delete(vhdPath);
		}

		using (var vfs = DiskVirtualFileSystem.Create(vhdPath, DiskFormat.Vhd, DefaultVhdSizeBytes, _logger))
		{
			foreach (var kvp in files)
			{
				var normalizedPath = NormalizeVfsPath(kvp.Key);
				EnsureDirectories(vfs, normalizedPath);

				var handle = vfs.OpenFile(normalizedPath, VfsFileMode.Create, VfsFileAccess.Write);
				if (handle == null)
				{
					_logger.LogWarning("[VHD] Failed to open {Path} for writing", normalizedPath);
					continue;
				}

				using (handle)
				{
					handle.Write(kvp.Value, 0, kvp.Value.Length);
				}
			}
		}

		_currentVhdPath = vhdPath;
		_currentExecutableVhdPath = $"C:{executablePath}";
		_currentVhdName = vhdName;
		_vhdFileCount = files.Count;

		return vhdPath;
	}

	private static string NormalizeVfsPath(string path)
	{
		var normalized = path.Replace('/', '\\');

		if (normalized.Length >= 2 && normalized[1] == ':')
		{
			normalized = normalized.Substring(2);
		}

		if (!normalized.StartsWith('\\'))
		{
			normalized = "\\" + normalized;
		}

		if (normalized.Length > 1 && normalized.EndsWith('\\'))
		{
			normalized = normalized.TrimEnd('\\');
		}

		return System.Text.RegularExpressions.Regex.Replace(normalized, @"\\+", "\\");
	}

	private static void EnsureDirectories(DiskVirtualFileSystem vfs, string normalizedPath)
	{
		var lastBackslash = normalizedPath.LastIndexOf('\\');
		if (lastBackslash <= 0)
		{
			return;
		}

		var directory = normalizedPath.Substring(0, lastBackslash);
		if (string.IsNullOrEmpty(directory))
		{
			return;
		}

		var parts = directory.Split('\\', StringSplitOptions.RemoveEmptyEntries);
		var current = "";
		foreach (var part in parts)
		{
			current += "\\" + part;
			vfs.CreateDirectory(current);
		}
	}

	public async Task<bool> SaveCurrentVhdAsync(string? nameOverride = null)
	{
		await PersistCurrentVhdAsync(nameOverride);
		return true;
	}

	private async Task PersistCurrentVhdAsync(string? nameOverride = null)
	{
		if (string.IsNullOrEmpty(_currentVhdPath) || string.IsNullOrEmpty(_currentExecutableVhdPath))
		{
			return;
		}

		if (!File.Exists(_currentVhdPath))
		{
			_logger.LogWarning("[VHD] Current VHD path missing: {Path}", _currentVhdPath);
			return;
		}

		try
		{
			var name = nameOverride ?? _currentVhdName ?? Path.GetFileNameWithoutExtension(_currentVhdPath);
			var data = await File.ReadAllBytesAsync(_currentVhdPath);
			var saved = await _vhdStorage.SaveAsync(name, _currentExecutableVhdPath, data);

			if (saved)
			{
				EmitDebugOutput($"[VHD] Persisted {name} ({data.LongLength} bytes)");
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[VHD] Failed to persist VHD {Path}", _currentVhdPath);
		}
	}

	/// <summary>
	/// Start emulation
	/// </summary>
	public async Task<bool> StartAsync()
	{
		if (_emulator == null || _emulator.LoadedImage == null)
		{
			EmitDebugOutput("Cannot start: No executable loaded");
			return false;
		}
		
		if (_isRunning)
		{
			EmitDebugOutput("Emulation already running");
			return true;
		}
		
		// Ensure there is no still-running emulation task before starting a new one
		if (_emulationTask != null && !_emulationTask.IsCompleted)
		{
			EmitDebugOutput("Cannot start: Previous emulation task is still running");
			return false;
		}
		
		try
		{
			_isRunning = true;
			_isPaused = false;
			
			// Dispose old cancellation token source only after confirming previous task has completed
			_emulationCts?.Dispose();
			_emulationCts = new CancellationTokenSource();
			
			EmitDebugOutput("Starting emulation...");
			EmitStdOutput("Win32Emu WASM Starting\n");
			
			OnStateChanged?.Invoke(this, new EmulatorStateChangedEventArgs
			{
				IsLoaded = true,
				IsRunning = true,
				ExecutableName = _loadedExecutableName
			});
			
			// Run emulation directly as an async task without Task.Run
			// In WebAssembly, Task.Run doesn't create a real background thread - it just queues work
			// on the same thread. Calling RunAsync() directly allows it to properly yield to the
			// browser event loop at await points. Task.Run can actually cause issues because it
			// wraps the async work in a way that may block the UI thread until the first await.
			// Note: Emulator.RunAsync() doesn't accept a CancellationToken - it uses Stop() method
			// for cancellation. The cancellation token parameter is kept for potential future use.
			_emulationTask = RunEmulationLoopAsync(_emulationCts.Token);
			
			// Wait a brief moment to allow the emulation loop to start and log its first message
			// This helps with debugging by ensuring we see the "[EmulationLoop] Starting..." message
			// before returning from StartAsync. In WASM, this yields control to let the async task begin.
			await Task.Delay(10);
			
			EmitDebugOutput("StartAsync completed, emulation task is running");
			
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to start emulation");
			EmitDebugOutput($"Failed to start: {ex.Message}");
			_isRunning = false;
			return false;
		}
	}
	
	/// <summary>
	/// Pause emulation
	/// </summary>
	public void Pause()
	{
		if (_emulator != null && _isRunning && !_isPaused)
		{
			_emulator.Pause();
			_isPaused = true;
			EmitDebugOutput("Emulation paused");
			
			OnStateChanged?.Invoke(this, new EmulatorStateChangedEventArgs
			{
				IsLoaded = true,
				IsRunning = true,
				IsPaused = true,
				ExecutableName = _loadedExecutableName
			});
		}
	}
	
	/// <summary>
	/// Resume emulation
	/// </summary>
	public void Resume()
	{
		if (_emulator != null && _isRunning && _isPaused)
		{
			_emulator.Resume();
			_isPaused = false;
			EmitDebugOutput("Emulation resumed");
			
			OnStateChanged?.Invoke(this, new EmulatorStateChangedEventArgs
			{
				IsLoaded = true,
				IsRunning = true,
				IsPaused = false,
				ExecutableName = _loadedExecutableName
			});
		}
	}
	
	/// <summary>
	/// Stop emulation
	/// </summary>
	public async Task StopAsync()
	{
		if (_emulator != null && _isRunning)
		{
			EmitDebugOutput("Stopping emulation...");
			
			_emulator.Stop();
			_emulationCts?.Cancel();
			
			if (_emulationTask != null)
			{
				try
				{
					await _emulationTask.WaitAsync(TimeSpan.FromSeconds(5));
				}
				catch (TimeoutException)
				{
					_logger.LogWarning("Emulation task did not stop within timeout");
				}
				catch (OperationCanceledException)
				{
					// Expected
				}
			}
			
			_isRunning = false;
			_isPaused = false;
			EmitDebugOutput("Emulation stopped");
			EmitStdOutput("Emulator stopped\n");
			
			OnStateChanged?.Invoke(this, new EmulatorStateChangedEventArgs
			{
				IsLoaded = true,
				IsRunning = false,
				ExecutableName = _loadedExecutableName
			});
		}
		
		// Clean up emulation task and cancellation token source regardless of _isRunning state
		// This ensures proper cleanup even if the emulation was already stopped or failed
		_emulationCts?.Dispose();
		_emulationCts = null;
		_emulationTask = null;
	}
	
	/// <summary>
	/// Get SIMD capabilities string for display
	/// </summary>
	public string GetSimdCapabilities()
	{
		return Win32Emu.Win32.DirectDraw.OptimizedBlitter.GetSimdCapabilities();
	}
	
	/// <summary>
	/// Gets all files in the virtual file system.
	/// Returns a read-only dictionary of file paths to file contents.
	/// </summary>
	public IReadOnlyDictionary<string, byte[]>? GetVfsFiles()
	{
		return null;
	}
	
	/// <summary>
	/// Load VFS files from a saved state.
	/// This replaces the current VFS contents with the loaded state.
	/// </summary>
	/// <param name="files">Dictionary of VFS files to load</param>
	public void LoadVfsFiles(Dictionary<string, byte[]> files)
	{
		EmitDebugOutput("VFS load skipped: browser VFS replaced by VHD-backed storage");
	}
	
	private void EmitStdOutput(string message)
	{
		OnStdOutput?.Invoke(this, message);
	}
	
	private void EmitDebugOutput(string message)
	{
		var timestampedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
		_logger.LogDebug(message);
		OnDebugOutput?.Invoke(this, timestampedMessage);
	}
	
	/// <summary>
	/// Helper method to run emulation loop with proper error handling and state management.
	/// This is extracted to a separate method to allow calling RunAsync directly without Task.Run,
	/// which is important for WASM where Task.Run doesn't create real background threads.
	/// Automatically handles child process requests by recursively loading and running child executables.
	/// </summary>
	/// <param name="cancellationToken">
	/// Cancellation token that is checked between child process launches to allow cooperative
	/// cancellation of the entire emulation and child-process chain, in addition to _emulator.Stop().
	/// </param>
	private async Task RunEmulationLoopAsync(CancellationToken cancellationToken)
	{
		// Track recursion depth to prevent infinite loops (e.g., A calls B, B calls A)
		var recursionDepth = 0;
		
		while (recursionDepth < MaxChildProcessRecursionDepth && !cancellationToken.IsCancellationRequested)
		{
			try
			{
				// Log that we're starting the emulation loop
				if (recursionDepth == 0)
				{
					EmitDebugOutput("[EmulationLoop] Starting emulator.RunAsync()...");
				}
				else
				{
					EmitDebugOutput($"[EmulationLoop] Starting child process (depth {recursionDepth})...");
				}
				
				// Note: cancellationToken is not used here because Emulator.RunAsync() doesn't support
				// cancellation tokens. Cancellation is handled via _emulator.Stop() in StopAsync().
				await _emulator!.RunAsync();
				
				// If we get here, emulation completed normally
				EmitDebugOutput("[EmulationLoop] Emulation completed successfully");
				
				// Check if a child process was requested
				var childRequest = _emulator.GetPendingChildProcessRequest();
				if (childRequest != null)
				{
					EmitDebugOutput($"[ChildProcess] Child process requested: {childRequest.ExecutablePath}");
					EmitDebugOutput($"[ChildProcess] Command line: {childRequest.CommandLine}");
					EmitDebugOutput($"[ChildProcess] Working directory: {childRequest.WorkingDirectory}");
					EmitDebugOutput($"[ChildProcess] Show command: {childRequest.ShowCommand}");
					
					// Resolve the child executable path in VFS
					// The path is already resolved by WinExec, but we need to convert it to VFS format
					var childPath = childRequest.ExecutablePath;
					
					// Convert Windows path (C:\WASM\setup.exe) to VFS path (\WASM\setup.exe)
					// Note: BrowserVirtualFileSystem normalizes paths to include a leading backslash
					if (childPath.StartsWith(@"C:\", StringComparison.OrdinalIgnoreCase))
					{
						childPath = childPath.Substring(2); // Remove "C:" prefix, keep the leading backslash
					}
					else if (childPath.StartsWith(@"C:", StringComparison.OrdinalIgnoreCase))
					{
						// No leading backslash after C:, add it
						childPath = "\\" + childPath.Substring(2);
					}
					else if (!childPath.StartsWith("\\"))
					{
						// Ensure leading backslash for paths without drive letter
						childPath = "\\" + childPath;
					}
					
					if (string.IsNullOrEmpty(_currentVhdPath) || !File.Exists(_currentVhdPath))
					{
						EmitDebugOutput("[ChildProcess] ERROR: No virtual disk available for child process");
						EmitStdOutput("ERROR: Virtual disk missing for child process\n");
						break;
					}

					var normalizedChildPath = NormalizeVfsPath(childPath);
					using (var diskVfs = new DiskVirtualFileSystem(_currentVhdPath, _loggerFactory.CreateLogger<DiskVirtualFileSystem>()))
					{
						if (!diskVfs.FileExists(normalizedChildPath))
						{
							EmitDebugOutput($"[ChildProcess] ERROR: Child executable not found in VHD: {normalizedChildPath}");
							EmitStdOutput($"ERROR: Child executable not found: {normalizedChildPath}\n");
							break;
						}
					}
					
					EmitDebugOutput($"[ChildProcess] Found child executable in VHD: {normalizedChildPath}");
					
					// Parse command line to extract arguments (if any)
					var cmdLine = childRequest.CommandLine;
					var args = Array.Empty<string>();
					
					// TODO: Implement proper command line parsing (split by spaces, respecting quotes)
					// For now, just pass empty args - the child executable will receive the full command line
					if (!string.IsNullOrEmpty(cmdLine) && cmdLine != childRequest.ExecutablePath)
					{
						// There are arguments after the executable path
						EmitDebugOutput($"[ChildProcess] Command line arguments: {cmdLine}");
					}
					
					// Dispose current emulator
					EmitDebugOutput("[ChildProcess] Disposing parent emulator...");
					_emulator.Dispose();
					_emulator = null;
					
					// Create new emulator for child process
					EmitDebugOutput("[ChildProcess] Creating new emulator for child process...");
					_emulator = new Emulator(_emulatorHost, _loggerFactory.CreateLogger<Emulator>(), null, _backendFactory);
					
					// Load child executable with VFS
					// Extract filename manually instead of using System.IO.Path.GetFileName
					// because in WASM/browser environments, System.IO.Path may not correctly handle
					// Windows-style backslash paths, returning the entire path instead of just the filename
					var lastBackslash = childPath.LastIndexOf('\\');
					var childFileName = lastBackslash >= 0 ? childPath.Substring(lastBackslash + 1) : childPath;
					EmitDebugOutput($"[ChildProcess] Loading child executable: {childFileName}");
					_loadedExecutableName = childFileName;
					_emulator.LoadExecutable(
						$"C:{normalizedChildPath}", 
						args, 
						debugMode: false, 
						reservedMemoryMb: 256, 
						virtualDiskPath: _currentVhdPath, 
						preloadedBytes: null, 
						customVirtualFileSystem: null, 
						force32BitStackOps: true, 
						forceInterpreterMode: true);
					
					EmitStdOutput($"\n=== Launching child process: {childFileName} ===\n");
					
					// Increment recursion depth and continue the loop to run the child
					recursionDepth++;
					continue;
				}
				
				// No child process requested, we're done
				EmitDebugOutput("[EmulationLoop] No child process requested, emulation complete");
				break;
			}
			catch (OperationCanceledException)
			{
				// Expected when cancellation is requested
				EmitDebugOutput("[EmulationLoop] Emulation cancelled");
				break;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[EmulationLoop] Emulation error");
				EmitDebugOutput($"[EmulationLoop] Emulation error: {ex.GetType().Name}: {ex.Message}");
				
				// Log stack trace for debugging
				if (ex.StackTrace != null)
				{
					EmitDebugOutput($"[EmulationLoop] Stack trace: {ex.StackTrace}");
				}
				break;
			}
		}
		
		// Check if we hit the recursion limit
		if (recursionDepth >= MaxChildProcessRecursionDepth)
		{
			EmitDebugOutput($"[EmulationLoop] WARNING: Maximum child process recursion depth ({MaxChildProcessRecursionDepth}) reached");
			EmitStdOutput($"ERROR: Maximum child process chain depth exceeded\n");
		}
		
		// Final cleanup
		_isRunning = false;
		EmitDebugOutput("[EmulationLoop] Emulation stopped, updating state...");
		
		OnStateChanged?.Invoke(this, new EmulatorStateChangedEventArgs
		{
			IsLoaded = true,
			IsRunning = false,
			ExecutableName = _loadedExecutableName
		});
		
		EmitDebugOutput("[EmulationLoop] State updated");
	}
	
	public void Dispose()
	{
		// Unsubscribe from emulator host events to prevent memory leaks
		if (_emulatorHost != null)
		{
			if (_debugOutputHandler != null)
			{
				_emulatorHost.DebugOutputReceived -= _debugOutputHandler;
				_debugOutputHandler = null;
			}
			if (_stdOutputHandler != null)
			{
				_emulatorHost.StdOutputReceived -= _stdOutputHandler;
				_stdOutputHandler = null;
			}
		}
		
		_emulationCts?.Cancel();
		_emulationCts?.Dispose();
		_emulator?.Dispose();
		
		// Clear references to allow garbage collection
		// Note: _emulatorHost is readonly and managed separately
		_backendFactory = null;
	}
}

/// <summary>
/// Event args for emulator state changes
/// </summary>
public class EmulatorStateChangedEventArgs : EventArgs
{
	public bool IsLoaded { get; set; }
	public bool IsRunning { get; set; }
	public bool IsPaused { get; set; }
	public string? ExecutableName { get; set; }
}
