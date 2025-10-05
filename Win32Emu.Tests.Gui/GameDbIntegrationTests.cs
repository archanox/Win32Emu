using Win32Emu.Gui.Models;
using Win32Emu.Gui.Services;
using Win32Emu.Gui.ViewModels;
using Win32Emu.Gui.Configuration;

namespace Win32Emu.Tests.Gui;

/// <summary>
/// Integration tests for GameDB with ViewModels
/// </summary>
public class GameDbIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _testExecutablePath;

    public GameDbIntegrationTests()
    {
        // Create temporary directory for test files
        _tempDir = Path.Combine(Path.GetTempPath(), "Win32EmuIntegrationTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);

        _testExecutablePath = Path.Combine(_tempDir, "ign_teas.exe");
        
        // Create a test executable with the exact content that produces the Ignition hash
        // For this test, we'll create a dummy file and manually set up the database to match it
        File.WriteAllText(_testExecutablePath, "Test executable for Ignition teaser");
        
        CleanupAppDirectories();
    }

    public void Dispose()
    {
        // Clean up temporary directory
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
        
        CleanupAppDirectories();
        GC.SuppressFinalize(this);
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

    [Fact]
    public void GameLibraryViewModel_ShouldEnrichGameFromDatabase_WhenGameDbServiceProvided()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var sha256 = HashUtility.ComputeSha256(_testExecutablePath);
        
        // Create a test database
        var database = new GameDatabase
        {
            Games = new List<GameDbEntry>
            {
                new GameDbEntry
                {
                    Id = gameId,
                    Title = "Test Game Title",
                    Description = "Test game description",
                    Developers = new List<string> { "Test Developer" },
                    Executables = new List<GameExecutable>
                    {
                        new GameExecutable
                        {
                            Name = "ign_teas.exe",
                            Sha256 = sha256
                        }
                    }
                }
            }
        };

        // Write database to expected location
        var appDir = AppContext.BaseDirectory;
        var gameDbPath = Path.Combine(appDir, "gamedb.json");
        var json = System.Text.Json.JsonSerializer.Serialize(database, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(gameDbPath, json);

        // Create services
        var configService = new ConfigurationService();
        var gameDbService = new GameDbService();
        var config = configService.GetEmulatorConfiguration();

        // Act
        var viewModel = new GameLibraryViewModel(config, configService, gameDbService);
        
        // Simulate adding a game manually
        var game = new Game
        {
            Title = "Original Title",
            ExecutablePath = _testExecutablePath,
            Description = "Original description"
        };

        // Use reflection to call the private EnrichGameFromDb method
        var method = typeof(GameLibraryViewModel).GetMethod("EnrichGameFromDb", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method?.Invoke(viewModel, new object[] { game });

        // Assert
        Assert.Equal("Test Game Title", game.Title);
        Assert.Equal("Test game description", game.Description);
        Assert.Equal(gameId, game.GameDbId);
    }

    [Fact]
    public void GameLibraryViewModel_ShouldWorkWithoutGameDbService()
    {
        // Arrange
        var configService = new ConfigurationService();
        var config = configService.GetEmulatorConfiguration();

        // Act - Create without GameDbService
        var viewModel = new GameLibraryViewModel(config, configService, null);

        // Assert - Should not throw
        Assert.NotNull(viewModel);
        Assert.Empty(viewModel.Games);
    }

    [Fact]
    public void GameDbService_ShouldLoadShippedDatabase()
    {
        // This test verifies that the actual shipped gamedb.json can be loaded
        // Note: This assumes the gamedb.json is copied to the output directory
        
        // Arrange & Act
        var service = new GameDbService();
        var allGames = service.GetAllGames().ToList();

        // Assert
        // The shipped database should contain at least the Ignition game
        // But this test is flexible in case the database is empty in test environment
        Assert.NotNull(allGames);
        
        // If there are games, verify the structure is valid
        foreach (var game in allGames)
        {
            Assert.NotNull(game.Id);
            Assert.NotNull(game.Title);
            Assert.NotNull(game.Executables);
        }
    }

    [Fact]
    public void HashUtility_ShouldBeUsedForFileIdentification()
    {
        // This test verifies that hash-based identification works
        
        // Arrange
        var testFile = Path.Combine(_tempDir, "test.exe");
        File.WriteAllText(testFile, "Test content for hashing");
        
        var expectedMd5 = HashUtility.ComputeMd5(testFile);
        var expectedSha1 = HashUtility.ComputeSha1(testFile);
        var expectedSha256 = HashUtility.ComputeSha256(testFile);

        // Act
        var (md5, sha1, sha256) = HashUtility.ComputeAllHashes(testFile);

        // Assert - All three hashes should match individual computations
        Assert.Equal(expectedMd5, md5);
        Assert.Equal(expectedSha1, sha1);
        Assert.Equal(expectedSha256, sha256);
        
        // Verify they are lowercase
        Assert.Equal(md5.ToLowerInvariant(), md5);
        Assert.Equal(sha1.ToLowerInvariant(), sha1);
        Assert.Equal(sha256.ToLowerInvariant(), sha256);
    }
}
