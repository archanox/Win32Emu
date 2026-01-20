using Hex1b;
using Hex1b.Widgets;
using Microsoft.Extensions.Logging;
using Win32Emu.Tools.Tui.Models;

namespace Win32Emu.Tools.Tui.Screens;

/// <summary>
/// Debugger screen for launching interactive debugger
/// </summary>
public static class DebuggerScreen
{
	private static string _executablePath = string.Empty;

	public static Hex1bWidget Build(IHex1bContext ctx, AppState state)
	{
		return ctx.VStack([
			// Header
			ctx.Border().WithTitle("Interactive Debugger")
				.Child(ctx.VStack([
					ctx.Text("Launch games in debug mode with step-through capabilities."),
					ctx.Text(""),
					ctx.Text("Features:"),
					ctx.Text("  • Set breakpoints at specific addresses"),
					ctx.Text("  • Step through instructions one at a time"),
					ctx.Text("  • Inspect CPU registers and memory"),
					ctx.Text("  • Examine the call stack"),
					ctx.Text("  • Pause and resume execution"),
				])),
			
			new SeparatorWidget(),
			
			// Input field
			ctx.HStack([
				ctx.Text("Executable Path:").Fixed(20),
				ctx.TextBox()
					.OnTextChanged(text => _executablePath = text)
					.Fill()
			]),
			
			new SeparatorWidget(),
			
			ctx.Button("Launch Debugger", () => LaunchDebugger(state)),
			
			// Footer
			ctx.InfoBar("Tab: Next field | Enter: Launch | ESC: Back")
		]);
	}

	private static void LaunchDebugger(AppState state)
	{
		if (string.IsNullOrWhiteSpace(_executablePath))
		{
			state.Logger.LogWarning("Executable path is required");
			return;
		}

		if (!File.Exists(_executablePath))
		{
			state.Logger.LogWarning("Executable not found: {Path}", _executablePath);
			return;
		}

		try
		{
			state.Logger.LogInformation("Launching debugger for: {Path}", _executablePath);
			
			// Build arguments with interactive debugger enabled
			var args = new List<string>
			{
				_executablePath,
				"--nogui",
				"--interactive-debug"
			};

			// Launch the emulator with interactive debugger
			// This will take over the terminal
			var exitCode = EmulatorLauncher.Launch(args.ToArray());
			
			state.Logger.LogInformation("Debugger session ended with code: {ExitCode}", exitCode);
			
			// Clear the path after use
			_executablePath = string.Empty;
		}
		catch (Exception ex)
		{
			state.Logger.LogError(ex, "Failed to launch debugger");
		}
	}
}
