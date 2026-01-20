using Hex1b;
using Hex1b.Widgets;
using Win32Emu.Tools.Tui.Models;

namespace Win32Emu.Tools.Tui.Screens;

/// <summary>
/// Main menu screen - entry point for the TUI
/// Optimized for 80-column mode
/// </summary>
public static class MainMenuScreen
{
	public static Hex1bWidget Build(IHex1bContext ctx, AppState state, CancellationTokenSource cts)
	{
		var menuItems = new[]
		{
			"Game Library",
			"Add Game",
			"Settings",
			"Interactive Debugger",
			"Help",
			"Exit"
		};

		return ctx.VStack([
			// Header
			ctx.Border()
				.WithTitle("Win32Emu - TUI Edition")
				.Child(ctx.Text("Terminal User Interface for Win32 Emulation")),
			
			new SeparatorWidget(),
			
			// Menu
			ctx.Border()
				.WithTitle("Main Menu")
				.Child(
					ctx.List(menuItems)
						.Fill()
						.OnItemActivated(index =>
						{
							switch (index)
							{
								case 0: // Game Library
									state.CurrentView = ViewMode.GameLibrary;
									break;
								case 1: // Add Game
									state.CurrentView = ViewMode.AddGame;
									break;
								case 2: // Settings
									state.CurrentView = ViewMode.Settings;
									break;
								case 3: // Interactive Debugger
									state.CurrentView = ViewMode.Debugger;
									break;
								case 4: // Help
									state.CurrentView = ViewMode.Help;
									break;
								case 5: // Exit
									cts.Cancel();
									break;
							}
						})
				),
			
			// Footer
			ctx.InfoBar("Arrow keys to navigate | Enter to select | Q to quit")
		]);
	}
}
