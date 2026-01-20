using Microsoft.Extensions.Logging;
using Win32Emu.Rendering;

namespace Win32Emu.Tools.Tui.Services;

/// <summary>
/// Service for managing emulator configuration
/// </summary>
public class ConfigurationService
{
	private readonly ILogger _logger;
	
	public BackendType DefaultBackend { get; set; } = BackendType.Software;
	public bool EnableDebugMode { get; set; } = false;
	public bool EnableInteractiveDebugger { get; set; } = false;
	public bool EnableGdbServer { get; set; } = false;
	public int GdbServerPort { get; set; } = 1234;
	public bool EnableFileLogging { get; set; } = false;

	public ConfigurationService(ILogger logger)
	{
		_logger = logger;
	}

	public string[] BuildEmulatorArgs(string executablePath)
	{
		var args = new List<string> { executablePath, "--nogui" };

		// Backend selection
		args.Add("--backend");
		args.Add(DefaultBackend.ToString());

		// Debug options
		if (EnableDebugMode)
		{
			args.Add("--debug");
		}

		if (EnableInteractiveDebugger)
		{
			args.Add("--interactive-debug");
		}

		if (EnableGdbServer)
		{
			args.Add("--gdb-server");
			args.Add(GdbServerPort.ToString());
		}

		if (EnableFileLogging)
		{
			args.Add("--log-file");
		}

		return args.ToArray();
	}
}
