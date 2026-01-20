using Microsoft.Extensions.Logging;
using System.Text.Json;
using Win32Emu.Tools.Tui.Models;

namespace Win32Emu.Tools.Tui.Services;

/// <summary>
/// Service for managing the game library
/// Loads and saves game entries from/to JSON file
/// </summary>
public class GameLibraryService
{
	private readonly ILogger _logger;
	private readonly string _libraryPath;
	private List<GameEntry> _games = new();

	public IReadOnlyList<GameEntry> Games => _games.AsReadOnly();

	public GameLibraryService(ILogger logger, string? libraryPath = null)
	{
		_logger = logger;
		_libraryPath = libraryPath ?? Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			"Win32Emu",
			"game-library.json"
		);
	}

	public async Task LoadLibraryAsync()
	{
		try
		{
			if (File.Exists(_libraryPath))
			{
				var json = await File.ReadAllTextAsync(_libraryPath);
				_games = JsonSerializer.Deserialize<List<GameEntry>>(json) ?? new List<GameEntry>();
				_logger.LogInformation("Loaded {Count} games from library", _games.Count);
			}
			else
			{
				_logger.LogInformation("No existing library found, starting with empty library");
				_games = new List<GameEntry>();
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to load game library");
			_games = new List<GameEntry>();
		}
	}

	public async Task SaveLibraryAsync()
	{
		try
		{
			var directory = Path.GetDirectoryName(_libraryPath);
			if (directory != null && !Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			var json = JsonSerializer.Serialize(_games, new JsonSerializerOptions
			{
				WriteIndented = true
			});
			
			await File.WriteAllTextAsync(_libraryPath, json);
			_logger.LogInformation("Saved library with {Count} games", _games.Count);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to save game library");
		}
	}

	public void AddGame(GameEntry game)
	{
		game.Id = Guid.NewGuid().ToString();
		game.AddedDate = DateTime.Now;
		_games.Add(game);
		_logger.LogInformation("Added game: {Title}", game.Title);
	}

	public void RemoveGame(string id)
	{
		var game = _games.FirstOrDefault(g => g.Id == id);
		if (game != null)
		{
			_games.Remove(game);
			_logger.LogInformation("Removed game: {Title}", game.Title);
		}
	}

	public void UpdateGameStats(string id)
	{
		var game = _games.FirstOrDefault(g => g.Id == id);
		if (game != null)
		{
			game.LastPlayed = DateTime.Now;
			game.PlayCount++;
		}
	}
}
