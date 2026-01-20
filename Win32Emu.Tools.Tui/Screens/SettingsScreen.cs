using Hex1b;
using Hex1b.Widgets;
using Win32Emu.Rendering;
using Win32Emu.Tools.Tui.Models;

namespace Win32Emu.Tools.Tui.Screens;

/// <summary>
/// Settings screen for configuring emulator options
/// </summary>
public static class SettingsScreen
{
	public static Hex1bWidget Build(IHex1bContext ctx, AppState state)
	{
		var config = state.Configuration;
		
		var settingsList = new[]
		{
			$"Default Backend: {config.DefaultBackend}",
			$"Debug Mode: {(config.EnableDebugMode ? "Enabled" : "Disabled")}",
			$"Interactive Debugger: {(config.EnableInteractiveDebugger ? "Enabled" : "Disabled")}",
			$"GDB Server: {(config.EnableGdbServer ? "Enabled" : "Disabled")}",
			$"GDB Server Port: {config.GdbServerPort}",
			$"File Logging: {(config.EnableFileLogging ? "Enabled" : "Disabled")}"
		};

		return ctx.VStack([
			// Header
			ctx.Border().WithTitle("Settings")
				.Child(ctx.Text("Configure emulator options")),
			
			new SeparatorWidget(),
			
			// Settings list
			ctx.List(settingsList)
				.Fill()
				.OnItemActivated(index => ToggleSetting(config, index)),
			
			// Footer
			ctx.InfoBar("Arrow keys: Navigate | Enter: Toggle/Change | ESC: Back")
		]);
	}

	private static void ToggleSetting(ConfigurationService config, int index)
	{
		switch (index)
		{
			case 0: // Backend - cycle through options
				var backends = Enum.GetValues<BackendType>();
				var currentIndex = Array.IndexOf(backends, config.DefaultBackend);
				config.DefaultBackend = backends[(currentIndex + 1) % backends.Length];
				break;
			case 1: // Debug Mode
				config.EnableDebugMode = !config.EnableDebugMode;
				break;
			case 2: // Interactive Debugger
				config.EnableInteractiveDebugger = !config.EnableInteractiveDebugger;
				break;
			case 3: // GDB Server
				config.EnableGdbServer = !config.EnableGdbServer;
				break;
			case 4: // GDB Port - increment by 1
				config.GdbServerPort = (config.GdbServerPort % 65535) + 1;
				if (config.GdbServerPort < 1024)
					config.GdbServerPort = 1234;
				break;
			case 5: // File Logging
				config.EnableFileLogging = !config.EnableFileLogging;
				break;
		}
	}
}
