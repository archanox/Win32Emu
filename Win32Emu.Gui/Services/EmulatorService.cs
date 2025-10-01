using Win32Emu.Gui.Models;

namespace Win32Emu.Gui.Services;

public class EmulatorService
{
    private readonly EmulatorConfiguration _configuration;
    private readonly Win32Emu.IEmulatorHost? _host;

    public EmulatorService(EmulatorConfiguration configuration, Win32Emu.IEmulatorHost? host = null)
    {
        _configuration = configuration;
        _host = host;
    }

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
                var emulator = new Win32Emu.Emulator(_host);
                
                // Load the executable
                emulator.LoadExecutable(game.ExecutablePath, _configuration.EnableDebugMode);
                
                // Run the emulator
                emulator.Run();
            }
            catch (Exception ex)
            {
                _host?.OnDebugOutput($"Emulator error: {ex.Message}", Win32Emu.DebugLevel.Error);
                throw;
            }
        });
    }
}
