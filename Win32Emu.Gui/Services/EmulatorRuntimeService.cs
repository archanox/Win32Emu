using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using Win32Emu.Gui.Backends;
using Win32Emu.Gui.Models;
using Win32Emu.Rendering;

namespace Win32Emu.Gui.Services;

public sealed class EmulatorRuntimeService
{
	private readonly EmulatorConfiguration _configuration;
	private readonly ILogger _logger;
	private readonly VirtualDiskService _virtualDiskService;

	private readonly Lock _gate = new();
	private Emulator? _emulator;
	private Task? _runTask;
	private int _runId;

	public EmulatorRuntimeService(EmulatorConfiguration configuration, ILogger logger)
	{
		ArgumentNullException.ThrowIfNull(configuration);
		ArgumentNullException.ThrowIfNull(logger);

		_configuration = configuration;
		_logger = logger;
		_virtualDiskService = new VirtualDiskService(configuration, logger);
	}

	public Emulator? CurrentEmulator
	{
		get
		{
			lock (_gate)
			{
				return _emulator;
			}
		}
	}

	public bool IsRunning
	{
		get
		{
			lock (_gate)
			{
				return _runTask is { IsCompleted: false };
			}
		}
	}

	public int CurrentRunId
	{
		get
		{
			lock (_gate)
			{
				return _runId;
			}
		}
	}

	public Task LaunchGameAsync(Game game, IEmulatorHost host, string[]? programArgs = null)
	{
		ArgumentNullException.ThrowIfNull(game);
		ArgumentNullException.ThrowIfNull(host);

		lock (_gate)
		{
			if (_runTask is { IsCompleted: false })
			{
				throw new InvalidOperationException("An emulation session is already running.");
			}

			_runId++;
			_runTask = Task.Run(() => RunGameWorkerAsync(game, host, programArgs, _runId));
			return _runTask;
		}
	}

	public void Stop()
	{
		Emulator? emulator;
		lock (_gate)
		{
			emulator = _emulator;
		}

		emulator?.Stop();
		_logger.LogInformation("[EmulatorRuntime] Stop requested");
	}

	private async Task RunGameWorkerAsync(Game game, IEmulatorHost host, string[]? programArgs, int runId)
	{
		try
		{
			if (string.IsNullOrEmpty(game.VirtualDiskPath) && !File.Exists(game.ExecutablePath))
			{
				throw new FileNotFoundException($"Game executable not found: {game.ExecutablePath}");
			}

			var gameHash = HashUtility.ComputeSha256(game.ExecutablePath);
			var gameSettings = _configuration.PerGameSettings.GetValueOrDefault(gameHash);

			var backendFactory = new BackendFactory();
			if (Enum.TryParse<BackendType>(_configuration.RenderingBackend, ignoreCase: true, out var backendType))
			{
				backendFactory.CurrentBackendType = backendType;
				_logger.LogInformation("[EmulatorRuntime] Set rendering backend to: {Backend}", backendType);
			}
			else
			{
				_logger.LogInformation("[EmulatorRuntime] Using default rendering backend: {Backend}", backendFactory.CurrentBackendType);
			}

			var telemetryService = App.TelemetryService;

			var emulator = new Emulator(host, _logger, telemetryService, backendFactory);
			lock (_gate)
			{
				// Ensure we only overwrite state for the active run.
				if (_runId != runId)
				{
					emulator.Dispose();
					throw new InvalidOperationException("Emulator run was superseded before start.");
				}
				_emulator = emulator;
			}

			string? virtualDiskPath = null;
			if (!string.IsNullOrEmpty(game.VirtualDiskPath) && File.Exists(game.VirtualDiskPath))
			{
				virtualDiskPath = game.VirtualDiskPath;
				_logger.LogInformation("[EmulatorRuntime] Using existing virtual disk: {DiskPath}", virtualDiskPath);
			}
			else if (!string.IsNullOrEmpty(game.VirtualDiskPath))
			{
				_logger.LogWarning("[EmulatorRuntime] Game does not have a VHD path, creating one now. This should have been done during AddGame.");
				virtualDiskPath = _virtualDiskService.GetOrCreateVirtualDisk(game, gameSettings);
				game.VirtualDiskPath = virtualDiskPath;
				_logger.LogInformation("[EmulatorRuntime] Created new virtual disk: {DiskPath}", virtualDiskPath);
			}

			var executablePath = !string.IsNullOrEmpty(game.VhdExecutablePath)
				? game.VhdExecutablePath
				: game.ExecutablePath;

			_logger.LogInformation("[EmulatorRuntime] Loading executable: {ExecutablePath}", executablePath);

			emulator.LoadExecutable(
				executablePath,
				programArgs,
				_configuration.EnableDebugMode,
				false,
				_configuration.ReservedMemoryMb,
				_configuration.EnableGdbServer,
				_configuration.GdbServerPort,
				_configuration.EnableInstructionAnalyzer,
				_configuration.EnableLegacyInstructionDecoding,
				_configuration.ForceInterpreterMode,
				virtualDiskPath,
				preloadedBytes: null,
				customVirtualFileSystem: null,
				force32BitStackOps: _configuration.Force32BitStackOps,
				ansiCodePage: _configuration.DefaultAnsiCodePage,
				oemCodePage: _configuration.DefaultOemCodePage);

			emulator.Run();

			if (emulator.LastException != null)
			{
				_logger.LogError(emulator.LastException, "Unhandled exception during emulation");
				host.OnDebugOutput($"Unhandled exception: {emulator.LastException.Message}", DebugLevel.Error);
			}
		}
		finally
		{
			Emulator? toDispose = null;
			lock (_gate)
			{
				if (_runId == runId)
				{
					toDispose = _emulator;
					_emulator = null;
					_runTask = null;
				}
			}

			try
			{
				toDispose?.Dispose();
			}
			catch (Exception ex) when (ex is not OutOfMemoryException && ex is not StackOverflowException)
			{
				_logger.LogWarning(ex, "[EmulatorRuntime] Failed to dispose emulator cleanly");
			}
		}
	}
}
