using System.Security.Cryptography;

namespace Win32Emu.Gui.Services;

/// <summary>
/// Utility class for computing file hashes
/// </summary>
public static class HashUtility
{
    /// <summary>
    /// Compute MD5 hash of a file
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <returns>Hexadecimal MD5 hash string (lowercase)</returns>
    public static string ComputeMd5(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        using var stream = File.OpenRead(filePath);
        var hashBytes = MD5.HashData(stream);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <summary>
    /// Compute SHA-1 hash of a file
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <returns>Hexadecimal SHA-1 hash string (lowercase)</returns>
    public static string ComputeSha1(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        using var stream = File.OpenRead(filePath);
        var hashBytes = SHA1.HashData(stream);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <summary>
    /// Compute SHA-256 hash of a file
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <returns>Hexadecimal SHA-256 hash string (lowercase)</returns>
    public static string ComputeSha256(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        using var stream = File.OpenRead(filePath);
        var hashBytes = SHA256.HashData(stream);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <summary>
    /// Compute all supported hashes for a file
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <returns>Tuple containing (MD5, SHA1, SHA256) hashes</returns>
    public static (string Md5, string Sha1, string Sha256) ComputeAllHashes(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        using var stream = File.OpenRead(filePath);
        
        // Read file once into memory for multiple hash calculations
        var fileBytes = new byte[stream.Length];
        stream.ReadExactly(fileBytes);

        var md5 = Convert.ToHexString(MD5.HashData(fileBytes)).ToLowerInvariant();
        var sha1 = Convert.ToHexString(SHA1.HashData(fileBytes)).ToLowerInvariant();
        var sha256 = Convert.ToHexString(SHA256.HashData(fileBytes)).ToLowerInvariant();

        return (md5, sha1, sha256);
    }
}
