using System.Diagnostics;
using Win32Emu.Gui.Models;

namespace Win32Emu.Gui.Services;

public class EmulatorService
{
    private readonly EmulatorConfiguration _configuration;
    private readonly IEmulatorHost? _host;

    public EmulatorService(EmulatorConfiguration configuration, IEmulatorHost? host = null)
    {
        _configuration = configuration;
        _host = host;
    }

    /// <summary>
    /// Launch game using the process-based approach (legacy)
    /// This will be replaced with the in-process API in future updates
    /// </summary>
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

        // TODO: Replace this with in-process emulator API
        // For now, launch as a separate process
        await LaunchAsProcess(args);
    }

    private async Task LaunchAsProcess(List<string> args)
    {
        // Launch the Win32Emu process
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "Win32Emu",
            Arguments = string.Join(" ", args.Select(arg => $"\"{arg}\"")),
            UseShellExecute = false,
            CreateNoWindow = false,
            RedirectStandardOutput = _host != null,
            RedirectStandardError = _host != null
        };

        await Task.Run(() =>
        {
            using var process = Process.Start(processStartInfo);
            if (process != null && _host != null)
            {
                // If we have a host, redirect output
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        _host.OnStdOutput(e.Data);
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        _host.OnDebugOutput(e.Data, DebugLevel.Error);
                    }
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }

            process?.WaitForExit();
        });
    }

    // Future API-based methods will be added here:
    // public async Task LaunchGameInProcess(Game game) { ... }
    // public void SetDisplayOutput(Action<DisplayUpdateInfo> callback) { ... }
    // public void SetDebugOutput(Action<string, DebugLevel> callback) { ... }
}
