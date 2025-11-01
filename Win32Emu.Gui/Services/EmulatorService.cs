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
        if (!File.Exists(game.ExecutablePath))
        {
            throw new FileNotFoundException($"Game executable not found: {game.ExecutablePath}");
        }

        // Get per-game settings
        var gameHash = HashUtility.ComputeSha256(game.ExecutablePath);
        var gameSettings = _configuration.PerGameSettings.GetValueOrDefault(gameHash);

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
                
                // Determine which CPU backend to use based on configuration
                var useJitCpu = _configuration.CpuBackend == "JitCPU";
                var useUnicornCpu = _configuration.CpuBackend == "Unicorn";
                
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
                    _configuration.EnableLegacyInstructionDecoding,
                    useJitCpu,
                    useUnicornCpu);
                
                // Initialize virtual file system
                InitializeVirtualFileSystem(game, gameSettings);
                
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

    private void InitializeVirtualFileSystem(Game game, GameSettings? gameSettings)
    {
        if (_currentEmulator?.Environment == null)
        {
            _logger.LogWarning("[EmulatorService] Cannot initialize VFS: emulator environment not ready");
            return;
        }

        var useVirtualDisk = _virtualDiskService.ShouldUseVirtualDisk(game, gameSettings);

        if (useVirtualDisk)
        {
            // Use virtual disk (VHD/VMDK/VHDX)
            try
            {
                var diskPath = _virtualDiskService.GetOrCreateVirtualDisk(game, gameSettings);
                
                // Check if disk exists, if not warn user
                if (!File.Exists(diskPath))
                {
                    _logger.LogWarning("[EmulatorService] Virtual disk does not exist: {Path}", diskPath);
                    _logger.LogWarning("[EmulatorService] Please create it manually using: qemu-img create -f vhd \"{Path}\" {SizeMb}M", 
                        diskPath, gameSettings?.VirtualDiskSizeMb ?? _configuration.DefaultVirtualDiskSizeMb);
                    _host?.OnDebugOutput($"Virtual disk not found. Create manually: qemu-img create -f vhd \"{diskPath}\" {gameSettings?.VirtualDiskSizeMb ?? _configuration.DefaultVirtualDiskSizeMb}M", DebugLevel.Warning);
                    
                    // Fall back to layered VFS
                    InitializeLayeredVFS(game);
                    return;
                }
                
                _currentEmulator.Environment.InitializeVirtualFileSystemWithDisk(diskPath);
                _logger.LogInformation("[EmulatorService] Using virtual disk for game: {Title}", game.Title);
                
                // Copy source directory into disk if specified and disk is writable
                if (!string.IsNullOrEmpty(gameSettings?.VirtualDiskSourceDirectory) && 
                    Directory.Exists(gameSettings.VirtualDiskSourceDirectory))
                {
                    try
                    {
                        var vfs = _currentEmulator.Environment.VirtualFileSystem;
                        if (vfs is DiskVirtualFileSystem diskVfs && !diskVfs.IsReadOnly)
                        {
                            _logger.LogInformation("[EmulatorService] Copying source directory into virtual disk: {SourceDir}", 
                                gameSettings.VirtualDiskSourceDirectory);
                            diskVfs.CopyDirectoryIn(gameSettings.VirtualDiskSourceDirectory, "/");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[EmulatorService] Failed to copy source directory into virtual disk");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EmulatorService] Failed to initialize virtual disk, falling back to layered VFS");
                InitializeLayeredVFS(game);
            }
        }
        else
        {
            // Use traditional layered VFS
            InitializeLayeredVFS(game);
        }
    }

    private void InitializeLayeredVFS(Game game)
    {
        if (_currentEmulator?.Environment == null)
        {
            return;
        }

        // Use game directory as base, with temp overlay
        var baseDir = Path.GetDirectoryName(game.ExecutablePath) ?? Directory.GetCurrentDirectory();
        _currentEmulator.Environment.InitializeVirtualFileSystem(baseDir);
        _logger.LogInformation("[EmulatorService] Using layered VFS for game: {Title}", game.Title);
    }
}
