using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Win32Emu.Wasm.Backend;
using Win32Emu.Wasm.VirtualFileSystem;

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
	
	private Emulator? _emulator;
	private WasmEmulatorHost? _emulatorHost;
	private WasmBackendFactory? _backendFactory;
	private BrowserVirtualFileSystem? _browserVfs;
	private CancellationTokenSource? _emulationCts;
	private Task? _emulationTask;
	
	// Store event handlers for proper unsubscription
	private EventHandler<string>? _debugOutputHandler;
	private EventHandler<string>? _stdOutputHandler;
	
	// State
	private bool _isRunning;
	private bool _isPaused;
	private string? _loadedExecutableName;
	
	// Events for UI updates
	public event EventHandler<string>? OnStdOutput;
	public event EventHandler<string>? OnDebugOutput;
	public event EventHandler<EmulatorStateChangedEventArgs>? OnStateChanged;
	
	public bool IsRunning => _isRunning;
	public bool IsPaused => _isPaused;
	public bool IsExecutableLoaded => _emulator?.LoadedImage != null;
	public string? LoadedExecutableName => _loadedExecutableName;
	
	/// <summary>
	/// Gets the number of instructions executed by the emulator.
	/// Note: In WASM mode, telemetry is not enabled, so this will return 0.
	/// This is a placeholder for future implementation.
	/// </summary>
	public ulong InstructionsExecuted => 0;
	
	/// <summary>
	/// Gets the number of files in the virtual file system.
	/// </summary>
	public int VfsFileCount => _browserVfs?.FileCount ?? 0;
	
	public EmulatorService(IJSRuntime jsRuntime, ILoggerFactory loggerFactory)
	{
		_jsRuntime = jsRuntime;
		_loggerFactory = loggerFactory;
		_logger = loggerFactory.CreateLogger<EmulatorService>();
	}
	
	/// <summary>
	/// Load an executable from a byte array
	/// </summary>
	/// <param name="executableBytes">The raw bytes of the executable</param>
	/// <param name="fileName">The name of the executable file</param>
	/// <param name="additionalFiles">Optional dictionary of additional files (path -> bytes) for the VFS</param>
	/// <returns>True if loading succeeded</returns>
	public async Task<bool> LoadExecutableAsync(
		byte[] executableBytes, 
		string fileName,
		Dictionary<string, byte[]>? additionalFiles = null)
	{
		try
		{
			// Stop any running emulation before loading a new executable
			if (_isRunning)
			{
				await StopAsync();
			}
			
			EmitDebugOutput($"Loading executable: {fileName} ({executableBytes.Length} bytes)");
			
			// Create backend factory if not already created
			_backendFactory ??= new WasmBackendFactory(_jsRuntime, _loggerFactory);
			
			// Create emulator host once and reuse it across multiple loads
			// This avoids memory leaks from orphaned event handlers
			if (_emulatorHost == null)
			{
				_emulatorHost = new WasmEmulatorHost(_loggerFactory.CreateLogger<WasmEmulatorHost>());
				
				// Wire up host events to forward to service events (only once)
				// Store handlers so we can unsubscribe later
				_debugOutputHandler = (sender, message) => EmitDebugOutput(message);
				_stdOutputHandler = (sender, message) => EmitStdOutput(message);
				
				_emulatorHost.DebugOutputReceived += _debugOutputHandler;
				_emulatorHost.StdOutputReceived += _stdOutputHandler;
			}
			
			// Create browser-based virtual file system
			_browserVfs?.Dispose();
			_browserVfs = new BrowserVirtualFileSystem(_loggerFactory.CreateLogger<BrowserVirtualFileSystem>());
			
			// Add the main executable to VFS
			var exePath = $"WASM\\{fileName}";
			_browserVfs.AddFile(exePath, executableBytes);
			EmitDebugOutput($"Added executable to VFS: \\{exePath}");
			
			// Add additional files to VFS if provided (for folder uploads)
			if (additionalFiles != null && additionalFiles.Count > 0)
			{
				EmitDebugOutput($"Adding {additionalFiles.Count} additional files to VFS...");
				
				// webkitRelativePath gives paths like "folderName/subdir/file.txt"
				// The browser's folder upload API (webkitRelativePath) includes the top-level folder name
				// in all paths. The emulator expects all files to be under the "WASM" directory (C:\WASM).
				// We detect the common folder prefix so we can replace it with "WASM", ensuring the VFS
				// structure matches the emulator's working directory and avoids mismatches between
				// uploaded folder names and the expected VFS root.
				var normalizedPaths = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
				foreach (var kvp in additionalFiles)
				{
					var normalizedKey = kvp.Key.Replace('/', '\\');
					if (!normalizedPaths.TryAdd(normalizedKey, kvp.Value))
					{
						EmitDebugOutput($"Warning: Duplicate file path detected (case-insensitive): {normalizedKey}");
					}
				}
				
				// Detect the common folder prefix from the uploaded files
				string? commonPrefix = null;
				foreach (var path in normalizedPaths.Keys)
				{
					var firstSlash = path.IndexOf('\\');
					if (firstSlash > 0)
					{
						var prefix = path.Substring(0, firstSlash);
						if (commonPrefix == null)
						{
							commonPrefix = prefix;
						}
						else if (!string.Equals(commonPrefix, prefix, StringComparison.OrdinalIgnoreCase))
						{
							// Mixed prefixes - don't try to normalize
							commonPrefix = null;
							break;
						}
					}
				}
				
				foreach (var kvp in normalizedPaths)
				{
					// Replace the folder prefix with WASM to match emulator's working directory (C:\WASM)
					var vfsPath = kvp.Key;
					
					if (commonPrefix != null && vfsPath.StartsWith(commonPrefix + "\\", StringComparison.OrdinalIgnoreCase))
					{
						// Replace the original folder prefix with WASM
						vfsPath = $"WASM{vfsPath.Substring(commonPrefix.Length)}";
					}
					else if (!vfsPath.Contains('\\'))
					{
						// Single file without folder structure - add to WASM folder
						vfsPath = $"WASM\\{vfsPath}";
					}
					
					_browserVfs.AddFile(vfsPath, kvp.Value);
				}
				EmitDebugOutput($"VFS initialized with {_browserVfs.FileCount} files");
			}
			
			// Dispose old emulator if it exists to prevent memory leaks when loading multiple executables
			_emulator?.Dispose();
			
			// Create emulator with WASM backend factory AND emulator host for output
			var emulatorLogger = _loggerFactory.CreateLogger<Emulator>();
			_emulator = new Emulator(_emulatorHost, emulatorLogger, null, _backendFactory);
			
			// Load the executable from bytes using the Emulator's built-in method
			// with the browser VFS for file operations
			_emulator.LoadExecutableFromBytes(executableBytes, fileName, null, false, 256, _browserVfs);
			
			_loadedExecutableName = fileName;
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
		
		try
		{
			_isRunning = true;
			_isPaused = false;
			_emulationCts = new CancellationTokenSource();
			
			EmitDebugOutput("Starting emulation...");
			EmitStdOutput("Win32Emu WASM Starting\n");
			
			OnStateChanged?.Invoke(this, new EmulatorStateChangedEventArgs
			{
				IsLoaded = true,
				IsRunning = true,
				ExecutableName = _loadedExecutableName
			});
			
			// Run emulation on a background task
			// Note: Emulator.RunAsync() doesn't accept a CancellationToken - it uses Stop() method
			// for cancellation. The token here is used to cancel Task.Run() startup, while
			// _emulator.Stop() (called in StopAsync) stops the actual emulation loop.
			_emulationTask = Task.Run(async () =>
			{
				try
				{
					await _emulator.RunAsync();
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Emulation error");
					EmitDebugOutput($"Emulation error: {ex.Message}");
				}
				finally
				{
					_isRunning = false;
					OnStateChanged?.Invoke(this, new EmulatorStateChangedEventArgs
					{
						IsLoaded = true,
						IsRunning = false,
						ExecutableName = _loadedExecutableName
					});
				}
			}, _emulationCts.Token);
			
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
	}
	
	/// <summary>
	/// Get SIMD capabilities string for display
	/// </summary>
	public string GetSimdCapabilities()
	{
		return Win32Emu.Win32.DirectDraw.OptimizedBlitter.GetSimdCapabilities();
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
		_browserVfs?.Dispose();
		
		// Clear references to allow garbage collection
		_emulatorHost = null;
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
