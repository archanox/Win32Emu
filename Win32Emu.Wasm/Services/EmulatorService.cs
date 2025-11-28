using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
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
	
	private Emulator? _emulator;
	private WasmBackendFactory? _backendFactory;
	private CancellationTokenSource? _emulationCts;
	private Task? _emulationTask;
	
	// State
	private bool _isRunning;
	private bool _isPaused;
	private ulong _instructionsExecuted;
	private string? _loadedExecutableName;
	
	// Events for UI updates
	public event EventHandler<string>? OnStdOutput;
	public event EventHandler<string>? OnDebugOutput;
	public event EventHandler<EmulatorStateChangedEventArgs>? OnStateChanged;
	public event EventHandler<ulong>? OnInstructionCountUpdated;
	
	public bool IsRunning => _isRunning;
	public bool IsPaused => _isPaused;
	public bool IsExecutableLoaded => _emulator?.LoadedImage != null;
	public string? LoadedExecutableName => _loadedExecutableName;
	public ulong InstructionsExecuted => _instructionsExecuted;
	
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
			EmitDebugOutput($"Loading executable: {fileName} ({executableBytes.Length} bytes)");
			
			// Create backend factory if not already created
			_backendFactory ??= new WasmBackendFactory(_jsRuntime, _loggerFactory);
			
			// Create emulator with WASM backend factory
			var emulatorLogger = _loggerFactory.CreateLogger<Emulator>();
			_emulator = new Emulator(null, emulatorLogger, null, _backendFactory);
			
			// Load the executable from bytes using the Emulator's built-in method
			// which handles synthetic path generation internally
			_emulator.LoadExecutableFromBytes(executableBytes, fileName);
			
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
		_emulationCts?.Cancel();
		_emulationCts?.Dispose();
		_emulator?.Dispose();
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
