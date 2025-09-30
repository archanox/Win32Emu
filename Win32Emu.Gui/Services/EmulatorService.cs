using System.Diagnostics;
using Win32Emu.Gui.Models;

namespace Win32Emu.Gui.Services;

public class EmulatorService
{
    private readonly EmulatorConfiguration _configuration;

    public EmulatorService(EmulatorConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task LaunchGame(Game game)
    {
        if (!File.Exists(game.ExecutablePath))
        {
            throw new FileNotFoundException($"Game executable not found: {game.ExecutablePath}");
        }

        // Build command line arguments
        var args = new List<string>
        {
            game.ExecutablePath
        };

        if (_configuration.EnableDebugMode)
        {
            args.Add("--debug");
        }

        // Launch the Win32Emu process
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "Win32Emu",
            Arguments = string.Join(" ", args.Select(arg => $"\"{arg}\"")),
            UseShellExecute = false,
            CreateNoWindow = false
        };

        await Task.Run(() =>
        {
            using var process = Process.Start(processStartInfo);
            process?.WaitForExit();
        });
    }
}
