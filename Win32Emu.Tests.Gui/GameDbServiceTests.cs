using System.Text.Json;
using Win32Emu.Gui.Models;
using Win32Emu.Gui.Services;

namespace Win32Emu.Tests.Gui;

public class GameDbServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _gameDbPath;
    private readonly string _overridesPath;
    private readonly string _testExecutablePath;

    public GameDbServiceTests()
    {
        // Create temporary directory for test files
        _tempDir = Path.Combine(Path.GetTempPath(), "Win32EmuTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);

        _gameDbPath = Path.Combine(_tempDir, "gamedb.json");
        _overridesPath = Path.Combine(_tempDir, "gamedb-overrides.json");
        _testExecutablePath = Path.Combine(_tempDir, "test.exe");

        // Create a test executable with known content
        File.WriteAllText(_testExecutablePath, "Test executable content");
        
        // Clean up any existing test files in app directories
        CleanupAppDirectories();
    }
    
    private static void CleanupAppDirectories()
    {
        // Clean up gamedb.json from app directory
        var appDir = AppContext.BaseDirectory;
        var gameDbPath = Path.Combine(appDir, "gamedb.json");
        if (File.Exists(gameDbPath))
        {
            File.Delete(gameDbPath);
        }
        
        // Clean up user overrides from AppData
        var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var configDir = Path.Combine(appDataDir, "Win32Emu");
        var overridesPath = Path.Combine(configDir, "gamedb-overrides.json");
        if (File.Exists(overridesPath))
        {
            File.Delete(overridesPath);
        }
    }

    public void Dispose()
    {
        // Clean up temporary directory
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
        
        // Clean up app directories
        CleanupAppDirectories();
        
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void FindGameByExecutable_ShouldReturnNull_WhenNoDatabaseExists()
    {
        // Arrange
        var service = CreateServiceWithCustomPaths();

        // Act
        var result = service.FindGameByExecutable(_testExecutablePath);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindGameByExecutable_ShouldMatchBySha256()
    {
        // Arrange
        var sha256 = HashUtility.ComputeSha256(_testExecutablePath);
        CreateGameDatabase(new GameDatabase
        {
            Games = new List<GameDbEntry>
            {
                new GameDbEntry
                {
                    Id = "test-game",
                    Title = "Test Game",
                    Executables = new List<GameExecutable>
                    {
                        new GameExecutable
                        {
                            Name = "test.exe",
                            Sha256 = sha256
                        }
                    }
                }
            }
        });

        var service = CreateServiceWithCustomPaths();

        // Act
        var result = service.FindGameByExecutable(_testExecutablePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-game", result.Id);
        Assert.Equal("Test Game", result.Title);
    }

    [Fact]
    public void FindGameByExecutable_ShouldMatchByMd5()
    {
        // Arrange
        var md5 = HashUtility.ComputeMd5(_testExecutablePath);
        CreateGameDatabase(new GameDatabase
        {
            Games = new List<GameDbEntry>
            {
                new GameDbEntry
                {
                    Id = "test-game",
                    Title = "Test Game",
                    Executables = new List<GameExecutable>
                    {
                        new GameExecutable
                        {
                            Name = "test.exe",
                            Md5 = md5
                        }
                    }
                }
            }
        });

        var service = CreateServiceWithCustomPaths();

        // Act
        var result = service.FindGameByExecutable(_testExecutablePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-game", result.Id);
    }

    [Fact]
    public void FindGameByExecutable_ShouldMatchBySha1()
    {
        // Arrange
        var sha1 = HashUtility.ComputeSha1(_testExecutablePath);
        CreateGameDatabase(new GameDatabase
        {
            Games = new List<GameDbEntry>
            {
                new GameDbEntry
                {
                    Id = "test-game",
                    Title = "Test Game",
                    Executables = new List<GameExecutable>
                    {
                        new GameExecutable
                        {
                            Name = "test.exe",
                            Sha1 = sha1
                        }
                    }
                }
            }
        });

        var service = CreateServiceWithCustomPaths();

        // Act
        var result = service.FindGameByExecutable(_testExecutablePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-game", result.Id);
    }

    [Fact]
    public void FindGameByExecutable_ShouldPrioritizeUserOverrides()
    {
        // Arrange
        var sha256 = HashUtility.ComputeSha256(_testExecutablePath);
        
        // Create readonly database
        CreateGameDatabase(new GameDatabase
        {
            Games = new List<GameDbEntry>
            {
                new GameDbEntry
                {
                    Id = "test-game",
                    Title = "Original Title",
                    Executables = new List<GameExecutable>
                    {
                        new GameExecutable { Name = "test.exe", Sha256 = sha256 }
                    }
                }
            }
        });

        // Create user overrides
        CreateUserOverrides(new GameDatabase
        {
            Games = new List<GameDbEntry>
            {
                new GameDbEntry
                {
                    Id = "test-game",
                    Title = "Overridden Title",
                    Executables = new List<GameExecutable>
                    {
                        new GameExecutable { Name = "test.exe", Sha256 = sha256 }
                    }
                }
            }
        });

        var service = CreateServiceWithCustomPaths();

        // Act
        var result = service.FindGameByExecutable(_testExecutablePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Overridden Title", result.Title);
    }

    [Fact]
    public void GetAllGames_ShouldReturnEmptyList_WhenNoDatabaseExists()
    {
        // Arrange
        var service = CreateServiceWithCustomPaths();

        // Act
        var result = service.GetAllGames();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetAllGames_ShouldReturnAllGames()
    {
        // Arrange
        CreateGameDatabase(new GameDatabase
        {
            Games = new List<GameDbEntry>
            {
                new GameDbEntry { Id = "game1", Title = "Game 1" },
                new GameDbEntry { Id = "game2", Title = "Game 2" }
            }
        });

        var service = CreateServiceWithCustomPaths();

        // Act
        var result = service.GetAllGames().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, g => g.Id == "game1");
        Assert.Contains(result, g => g.Id == "game2");
    }

    [Fact]
    public void GetAllGames_ShouldMergeReadonlyAndOverrides()
    {
        // Arrange
        CreateGameDatabase(new GameDatabase
        {
            Games = new List<GameDbEntry>
            {
                new GameDbEntry { Id = "game1", Title = "Game 1" },
                new GameDbEntry { Id = "game2", Title = "Original Game 2" }
            }
        });

        CreateUserOverrides(new GameDatabase
        {
            Games = new List<GameDbEntry>
            {
                new GameDbEntry { Id = "game2", Title = "Overridden Game 2" },
                new GameDbEntry { Id = "game3", Title = "Game 3" }
            }
        });

        var service = CreateServiceWithCustomPaths();

        // Act
        var result = service.GetAllGames().ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, g => g.Id == "game1" && g.Title == "Game 1");
        Assert.Contains(result, g => g.Id == "game2" && g.Title == "Overridden Game 2");
        Assert.Contains(result, g => g.Id == "game3" && g.Title == "Game 3");
    }

    [Fact]
    public void GetGameById_ShouldReturnGame_WhenExists()
    {
        // Arrange
        CreateGameDatabase(new GameDatabase
        {
            Games = new List<GameDbEntry>
            {
                new GameDbEntry { Id = "test-game", Title = "Test Game" }
            }
        });

        var service = CreateServiceWithCustomPaths();

        // Act
        var result = service.GetGameById("test-game");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Game", result.Title);
    }

    [Fact]
    public void GetGameById_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        CreateGameDatabase(new GameDatabase
        {
            Games = new List<GameDbEntry>
            {
                new GameDbEntry { Id = "test-game", Title = "Test Game" }
            }
        });

        var service = CreateServiceWithCustomPaths();

        // Act
        var result = service.GetGameById("nonexistent");

        // Assert
        Assert.Null(result);
    }

    private void CreateGameDatabase(GameDatabase database)
    {
        var json = JsonSerializer.Serialize(database, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_gameDbPath, json);
    }

    private void CreateUserOverrides(GameDatabase database)
    {
        var json = JsonSerializer.Serialize(database, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_overridesPath, json);
    }

    private GameDbService CreateServiceWithCustomPaths()
    {
        // We need to use reflection or create a test-friendly constructor
        // For now, we'll use a workaround by creating the files in the expected locations
        
        // Copy test files to expected locations
        var appDir = AppContext.BaseDirectory;
        var gameDbDestPath = Path.Combine(appDir, "gamedb.json");
        
        if (File.Exists(_gameDbPath))
        {
            File.Copy(_gameDbPath, gameDbDestPath, overwrite: true);
        }

        var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var configDir = Path.Combine(appDataDir, "Win32Emu");
        Directory.CreateDirectory(configDir);
        var overridesDestPath = Path.Combine(configDir, "gamedb-overrides.json");
        
        if (File.Exists(_overridesPath))
        {
            File.Copy(_overridesPath, overridesDestPath, overwrite: true);
        }

        return new GameDbService();
    }
}
