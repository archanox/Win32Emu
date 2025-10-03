using Win32Emu.Gui.Models;

namespace Win32Emu.Gui.Services;

public class EmulatorService
{
    private readonly EmulatorConfiguration _configuration;
    private readonly Win32Emu.IEmulatorHost? _host;
    private Win32Emu.Emulator? _currentEmulator;

    public EmulatorService(EmulatorConfiguration configuration, Win32Emu.IEmulatorHost? host = null)
    {
        _configuration = configuration;
        _host = host;
    }

    /// <summary>
    /// Get the currently running emulator instance, or null if not running
    /// </summary>
    public Win32Emu.Emulator? CurrentEmulator => _currentEmulator;

    /// <summary>
    /// Launch game using the in-process emulator API
    /// </summary>
    public async Task LaunchGame(Game game)
    {
        if (!File.Exists(game.ExecutablePath))
        {
            throw new FileNotFoundException($"Game executable not found: {game.ExecutablePath}");
        }

        await Task.Run(() =>
        {
            try
            {
                // Create and configure the emulator
                _currentEmulator = new Win32Emu.Emulator(_host);
                
                // Load the executable with configured memory size
                _currentEmulator.LoadExecutable(
                    game.ExecutablePath, 
                    _configuration.EnableDebugMode,
                    _configuration.ReservedMemoryMB);
                
                // Run the emulator
                _currentEmulator.Run();
            }
            catch (Exception ex)
            {
                _host?.OnDebugOutput($"Emulator error: {ex.Message}", Win32Emu.DebugLevel.Error);
                throw;
            }
            finally
            {
                _currentEmulator = null;
            }
        });
    }
}
