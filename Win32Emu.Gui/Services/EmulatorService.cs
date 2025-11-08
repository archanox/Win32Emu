using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Gui.Models;
using Win32Emu.VirtualFileSystem;

namespace Win32Emu.Gui.Services;

public class EmulatorService
{
    private readonly EmulatorConfiguration _configuration;
    private readonly IEmulatorHost? _host;
    private readonly ILogger _logger;
    private readonly VirtualDiskService _virtualDiskService;
    private Emulator? _currentEmulator;

    public EmulatorService(EmulatorConfiguration configuration, IEmulatorHost? host = null, ILogger? logger = null)
    {
        _configuration = configuration;
        _host = host;
        _logger = logger ?? NullLogger.Instance;
        _virtualDiskService = new VirtualDiskService(configuration, logger);
    }

    /// <summary>
    /// Get the currently running emulator instance, or null if not running
    /// </summary>
    public Emulator? CurrentEmulator => _currentEmulator;

    /// <summary>
    /// Stop the currently running emulator
    /// </summary>
    public void StopEmulator()
    {
        if (_currentEmulator != null)
        {
            _currentEmulator.Stop();
            _logger.LogInformation("Emulator stop requested");
        }
    }

    /// <summary>
    /// Launch game using the in-process emulator API
    /// </summary>
    /// <param name="game">The game to launch, including its executable path.</param>
    /// <param name="programArgs">An array of command-line arguments to pass to the emulator when launching the game. These arguments are parsed and provided to the emulated program as if they were passed on the command line.</param>
    public async Task LaunchGame(Game game, string[]? programArgs = null)
    {
        // Check if the game has a VHD path, otherwise check if the original executable exists
        if (string.IsNullOrEmpty(game.VirtualDiskPath) && !File.Exists(game.ExecutablePath))
        {
            throw new FileNotFoundException($"Game executable not found: {game.ExecutablePath}");
        }

        // Get per-game settings
        var gameHash = HashUtility.ComputeSha256(game.ExecutablePath);
        var gameSettings = _configuration.PerGameSettings.GetValueOrDefault(gameHash);

        await Task.Run(async () =>
        {
            try
            {
                // Set the rendering backend from configuration
                if (Enum.TryParse<Rendering.BackendType>(_configuration.RenderingBackend, ignoreCase: true, out var backendType))
                {
                    Rendering.BackendFactory.CurrentBackendType = backendType;
                    _logger.LogInformation("Set rendering backend to: {Backend}", backendType);
                }
                
                // Get the global telemetry service if enabled
                var telemetryService = App.TelemetryService;
                
                // Determine which CPU backend to use based on configuration
                var useJitCpu = _configuration.CpuBackend == "JitCPU";
                var useUnicornCpu = _configuration.CpuBackend == "Unicorn";
                
                // Create and configure the emulator
                _currentEmulator = new Emulator(_host, _logger, telemetryService);
                
                // Determine the virtual disk path to use
                string? virtualDiskPath = null;
                if (!string.IsNullOrEmpty(game.VirtualDiskPath) && File.Exists(game.VirtualDiskPath))
                {
                    virtualDiskPath = game.VirtualDiskPath;
                    _logger.LogInformation("[EmulatorService] Using existing virtual disk: {DiskPath}", virtualDiskPath);
                }
                else if (!string.IsNullOrEmpty(game.VirtualDiskPath))
                {
                    // Create a new VHD for this game (shouldn't happen if AddGame was used, but handle it as fallback)
                    _logger.LogWarning("[EmulatorService] Game does not have a VHD path, creating one now. This should have been done during AddGame.");
                    virtualDiskPath = _virtualDiskService.GetOrCreateVirtualDisk(game, gameSettings);
                    
                    // Update the game object with the new VHD path
                    game.VirtualDiskPath = virtualDiskPath;
                    
                    _logger.LogInformation("[EmulatorService] Created new virtual disk: {DiskPath}", virtualDiskPath);
                }
                
                // Determine which executable path to use
                // If game has a VHD executable path, use that (e.g., "C:\ignition\ign_teas.exe")
                // Otherwise, use the original host path (for backwards compatibility)
                var executablePath = !string.IsNullOrEmpty(game.VhdExecutablePath) 
                    ? game.VhdExecutablePath 
                    : game.ExecutablePath;
                
                _logger.LogInformation("[EmulatorService] Loading executable: {ExecutablePath}", executablePath);
                
                // Load the executable with configured memory size and GDB server settings
                // The virtual disk path is passed to LoadExecutable so the VFS can be initialized
                // after the emulator environment is ready but before file access is needed
                _currentEmulator.LoadExecutable(
                    executablePath,
                    programArgs,
                    _configuration.EnableDebugMode,
                    false, // Interactive debug mode not supported in GUI
                    _configuration.ReservedMemoryMb,
                    _configuration.EnableGdbServer,
                    _configuration.GdbServerPort,
                    _configuration.EnableInstructionAnalyzer,
                    _configuration.EnableLegacyInstructionDecoding,
                    useJitCpu,
                    useUnicornCpu,
                    virtualDiskPath); // Pass the virtual disk path
                
                // Run the emulator
                _currentEmulator.Run();
                
                // Check if there was an unhandled exception during emulation
                if (_currentEmulator.LastException != null)
                {
                    _logger.LogError(_currentEmulator.LastException, "Unhandled exception during emulation");
                    _host?.OnDebugOutput($"Unhandled exception: {_currentEmulator.LastException.Message}", DebugLevel.Error);
                    
                    // Show exception dialog on UI thread
                    if (_host != null)
                    {
                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            try
                            {
                                var mainWindow = Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                                    ? desktop.MainWindow
                                    : null;
                                
                                await Views.ExceptionDialog.ShowExceptionDialogAsync(mainWindow, _currentEmulator.LastException!, "Emulation");
                            }
                            catch (Exception dialogEx) when (dialogEx is not OutOfMemoryException && dialogEx is not StackOverflowException)
                            {
                                _logger.LogError(dialogEx, "Failed to show exception dialog");
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Emulator error: {Message}", ex.Message);
                _host?.OnDebugOutput($"Emulator error: {ex.Message}", DebugLevel.Error);
                throw;
            }
            finally
            {
                _currentEmulator = null;
            }
        });
    }
}
