using Hex1b;
using Hex1b.Widgets;
using Win32Emu.Tools.Tui.Models;

namespace Win32Emu.Tools.Tui.Screens;

/// <summary>
/// Help screen with usage information
/// </summary>
public static class HelpScreen
{
	public static Hex1bWidget Build(IHex1bContext ctx, AppState state)
	{
		return ctx.VStack([
			// Header
			ctx.Border().WithTitle("Win32Emu TUI - Help")
				.Child(ctx.Text("Terminal User Interface for Win32 Emulation")),
			
			new SeparatorWidget(),
			
			// Help content
			ctx.VStack([
				ctx.Text("OVERVIEW:"),
				ctx.Text("  Win32Emu TUI provides terminal-based interface for managing"),
				ctx.Text("  and running classic Windows games. Optimized for 80-column mode."),
				ctx.Text(""),
				ctx.Text("FEATURES:"),
				ctx.Text("  • Game library management (browse, add, delete games)"),
				ctx.Text("  • Interactive debugger integration"),
				ctx.Text("  • Multiple rendering backends (SDL, GLFW, Vulkan, Metal, Software)"),
				ctx.Text("  • 80-column display for mobile SSH access"),
				ctx.Text("  • Play statistics tracking"),
				ctx.Text(""),
				ctx.Text("NAVIGATION:"),
				ctx.Text("  • Arrow keys: Move up/down in lists"),
				ctx.Text("  • Enter: Select item or confirm action"),
				ctx.Text("  • ESC: Go back to previous screen"),
				ctx.Text("  • Tab: Move between form fields"),
				ctx.Text("  • Q: Quit (from main menu)"),
				ctx.Text(""),
				ctx.Text("GAME LIBRARY:"),
				ctx.Text("  Browse your game collection, view details, and launch games"),
				ctx.Text("  with one keypress. Statistics are automatically tracked."),
				ctx.Text(""),
				ctx.Text("INTERACTIVE DEBUGGER:"),
				ctx.Text("  Set breakpoints, step through instructions, inspect registers,"),
				ctx.Text("  and examine memory. Full GDB-style debugging integrated."),
			]),
			
			// Footer
			ctx.InfoBar("Press ESC to return to main menu")
		]);
	}
}
