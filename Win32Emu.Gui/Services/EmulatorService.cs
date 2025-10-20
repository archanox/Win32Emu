using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Gui.Models;

namespace Win32Emu.Gui.Services;

public class EmulatorService
{
    private readonly EmulatorConfiguration _configuration;
    private readonly IEmulatorHost? _host;
    private readonly ILogger _logger;
    private Emulator? _currentEmulator;

    public EmulatorService(EmulatorConfiguration configuration, IEmulatorHost? host = null, ILogger? logger = null)
    {
        _configuration = configuration;
        _host = host;
        _logger = logger ?? NullLogger.Instance;
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
        if (!File.Exists(game.ExecutablePath))
        {
            throw new FileNotFoundException($"Game executable not found: {game.ExecutablePath}");
        }

        await Task.Run(() =>
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
                
                // Create and configure the emulator
                _currentEmulator = new Emulator(_host, _logger, telemetryService);
                
                // Load the executable with configured memory size and GDB server settings
                _currentEmulator.LoadExecutable(
                    game.ExecutablePath,
                    programArgs,
                    _configuration.EnableDebugMode,
                    false, // Interactive debug mode not supported in GUI
                    _configuration.ReservedMemoryMb,
                    _configuration.EnableGdbServer,
                    _configuration.GdbServerPort,
                    _configuration.EnableInstructionAnalyzer,
                    _configuration.EnableLegacyInstructionDecoding);
                
                // Run the emulator
                _currentEmulator.Run();
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
