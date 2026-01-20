using Hex1b;
using Hex1b.Widgets;
using Win32Emu.Tools.Tui.Models;

namespace Win32Emu.Tools.Tui.Screens;

/// <summary>
/// Screen builder using Hex1b fluent API
/// Routes to different screens based on app state
/// </summary>
public static class ScreenBuilder
{
	public static Hex1bWidget BuildScreen(IIHex1bContext ctx, AppState state, CancellationTokenSource cts)
	{
		return state.CurrentView switch
		{
			ViewMode.MainMenu => MainMenuScreen.Build(ctx, state, cts),
			ViewMode.GameLibrary => GameLibraryScreen.Build(ctx, state),
			ViewMode.AddGame => AddGameScreen.Build(ctx, state),
			ViewMode.Settings => SettingsScreen.Build(ctx, state),
			ViewMode.Help => HelpScreen.Build(ctx, state),
			ViewMode.Debugger => DebuggerScreen.Build(ctx, state),
			_ => ctx.Text("Unknown view")
		};
	}
}
