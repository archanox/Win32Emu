using System.Security.Cryptography;
using Win32Emu.Gui.Services;

namespace Win32Emu.Tests.Gui;

public class HashUtilityTests : IDisposable
{
    private readonly string _tempFilePath;

    public HashUtilityTests()
    {
        // Create a temporary file for testing
        _tempFilePath = Path.GetTempFileName();
        File.WriteAllText(_tempFilePath, "Hello, World!");
    }

    public void Dispose()
    {
        // Clean up temporary file
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void ComputeMd5_ShouldReturnCorrectHash()
    {
        // Arrange
        var expected = "65a8e27d8879283831b664bd8b7f0ad4"; // MD5 of "Hello, World!"

        // Act
        var result = HashUtility.ComputeMd5(_tempFilePath);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ComputeSha1_ShouldReturnCorrectHash()
    {
        // Arrange
        var expected = "0a0a9f2a6772942557ab5355d76af442f8f65e01"; // SHA1 of "Hello, World!"

        // Act
        var result = HashUtility.ComputeSha1(_tempFilePath);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ComputeSha256_ShouldReturnCorrectHash()
    {
        // Arrange
        var expected = "dffd6021bb2bd5b0af676290809ec3a53191dd81c7f70a4b28688a362182986f"; // SHA256 of "Hello, World!"

        // Act
        var result = HashUtility.ComputeSha256(_tempFilePath);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ComputeAllHashes_ShouldReturnAllThreeHashes()
    {
        // Arrange
        var expectedMd5 = "65a8e27d8879283831b664bd8b7f0ad4";
        var expectedSha1 = "0a0a9f2a6772942557ab5355d76af442f8f65e01";
        var expectedSha256 = "dffd6021bb2bd5b0af676290809ec3a53191dd81c7f70a4b28688a362182986f";

        // Act
        var (md5, sha1, sha256) = HashUtility.ComputeAllHashes(_tempFilePath);

        // Assert
        Assert.Equal(expectedMd5, md5);
        Assert.Equal(expectedSha1, sha1);
        Assert.Equal(expectedSha256, sha256);
    }

    [Fact]
    public void ComputeMd5_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
    {
        // Arrange
        var nonExistentFile = "nonexistent.exe";

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => HashUtility.ComputeMd5(nonExistentFile));
    }

    [Fact]
    public void ComputeSha1_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
    {
        // Arrange
        var nonExistentFile = "nonexistent.exe";

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => HashUtility.ComputeSha1(nonExistentFile));
    }

    [Fact]
    public void ComputeSha256_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
    {
        // Arrange
        var nonExistentFile = "nonexistent.exe";

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => HashUtility.ComputeSha256(nonExistentFile));
    }

    [Fact]
    public void HashesAreCaseInsensitive()
    {
        // Arrange & Act
        var hash = HashUtility.ComputeMd5(_tempFilePath);

        // Assert
        Assert.Equal(hash.ToLowerInvariant(), hash);
        Assert.DoesNotContain(hash, c => char.IsUpper(c));
    }
}
