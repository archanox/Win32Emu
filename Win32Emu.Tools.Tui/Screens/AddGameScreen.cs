using Hex1b;
using Hex1b.Widgets;
using Win32Emu.Tools.Tui.Models;

namespace Win32Emu.Tools.Tui.Screens;

/// <summary>
/// Add game screen with form fields
/// </summary>
public static class AddGameScreen
{
	public static Hex1bWidget Build(IHex1bContext ctx, AppState state)
	{
		return ctx.VStack([
			// Header
			ctx.Border().WithTitle("Add New Game")
				.Child(ctx.Text("Enter game information (Tab to move between fields)")),
			
			new SeparatorWidget(),
			
			// Form fields
			ctx.VStack([
				ctx.HStack([
					ctx.Text("Title (required):").Fixed(25),
					ctx.TextBox()
						.OnTextChanged(text => state.NewGameTitle = text)
						.Fill()
				]),
				
				ctx.HStack([
					ctx.Text("Executable Path:").Fixed(25),
					ctx.TextBox()
						.OnTextChanged(text => state.NewGamePath = text)
						.Fill()
				]),
				
				ctx.HStack([
					ctx.Text("Developer:").Fixed(25),
					ctx.TextBox()
						.OnTextChanged(text => state.NewGameDeveloper = text)
						.Fill()
				]),
				
				ctx.HStack([
					ctx.Text("Publisher:").Fixed(25),
					ctx.TextBox()
						.OnTextChanged(text => state.NewGamePublisher = text)
						.Fill()
				]),
				
				ctx.HStack([
					ctx.Text("Genre:").Fixed(25),
					ctx.TextBox()
						.OnTextChanged(text => state.NewGameGenre = text)
						.Fill()
				]),
				
				ctx.HStack([
					ctx.Text("Release Year:").Fixed(25),
					ctx.TextBox()
						.OnTextChanged(text => state.NewGameYear = text)
						.Fill()
				]),
				
				new SeparatorWidget(),
				
				ctx.Button("Save Game", () => SaveGame(state))
			]),
			
			// Footer
			ctx.InfoBar("Tab: Next field | Enter: Save | ESC: Cancel")
		]);
	}

	private static void SaveGame(AppState state)
	{
		// Validate required fields
		if (string.IsNullOrWhiteSpace(state.NewGameTitle) || 
		    string.IsNullOrWhiteSpace(state.NewGamePath))
		{
			state.Logger.LogWarning("Title and Executable Path are required");
			return;
		}

		// Validate file exists
		if (!File.Exists(state.NewGamePath))
		{
			state.Logger.LogWarning("Executable not found: {Path}", state.NewGamePath);
			return;
		}

		// Parse year
		int? releaseYear = null;
		if (!string.IsNullOrWhiteSpace(state.NewGameYear) && 
		    int.TryParse(state.NewGameYear, out var year))
		{
			releaseYear = year;
		}

		// Create game entry
		var game = new GameEntry
		{
			Title = state.NewGameTitle,
			ExecutablePath = state.NewGamePath,
			Developer = string.IsNullOrWhiteSpace(state.NewGameDeveloper) ? null : state.NewGameDeveloper,
			Publisher = string.IsNullOrWhiteSpace(state.NewGamePublisher) ? null : state.NewGamePublisher,
			Genre = string.IsNullOrWhiteSpace(state.NewGameGenre) ? null : state.NewGameGenre,
			ReleaseYear = releaseYear
		};

		state.GameLibrary.AddGame(game);
		_ = state.GameLibrary.SaveLibraryAsync();

		// Clear form fields
		state.NewGameTitle = string.Empty;
		state.NewGamePath = string.Empty;
		state.NewGameDeveloper = string.Empty;
		state.NewGamePublisher = string.Empty;
		state.NewGameGenre = string.Empty;
		state.NewGameYear = string.Empty;

		// Return to library
		state.CurrentView = ViewMode.GameLibrary;
	}
}
