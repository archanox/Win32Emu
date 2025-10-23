using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Cpu.Jit;

/// <summary>
/// Manages JIT compilation cache for x86 code blocks.
/// Provides disk persistence for compiled block metadata and precompilation support.
/// </summary>
public class JitCache
{
	private readonly ILogger _logger;
	private readonly string _cacheDirectory;
	private readonly ConcurrentDictionary<uint, BlockMetadata> _blockCache = new();
	private readonly ConcurrentDictionary<string, uint> _hashToAddress = new();
	
	public JitCache(string? cacheDirectory = null, ILogger? logger = null)
	{
		_logger = logger ?? NullLogger.Instance;
		_cacheDirectory = cacheDirectory ?? Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"Win32Emu",
			"JitCache"
		);
		
		Directory.CreateDirectory(_cacheDirectory);
		_logger.LogInformation("[JitCache] Initialized with cache directory: {CacheDirectory}", _cacheDirectory);
	}
	
	/// <summary>
	/// Gets metadata for a cached block if it exists
	/// </summary>
	public bool TryGetBlockMetadata(uint address, out BlockMetadata? metadata)
	{
		return _blockCache.TryGetValue(address, out metadata);
	}
	
	/// <summary>
	/// Adds block metadata to the cache
	/// </summary>
	public void AddBlockMetadata(uint address, BlockMetadata metadata)
	{
		_blockCache[address] = metadata;
		_hashToAddress[metadata.CodeHash] = address;
	}
	
	/// <summary>
	/// Loads the cache from disk for a specific executable
	/// </summary>
	public async Task LoadCacheAsync(string executablePath)
	{
		var cacheFileName = GetCacheFileName(executablePath);
		var cacheFilePath = Path.Combine(_cacheDirectory, cacheFileName);
		
		if (!File.Exists(cacheFilePath))
		{
			_logger.LogInformation("[JitCache] No cache file found for {ExecutablePath}", executablePath);
			return;
		}
		
		try
		{
			var json = await File.ReadAllTextAsync(cacheFilePath);
			var cacheData = JsonSerializer.Deserialize<JitCacheData>(json);
			
			if (cacheData?.Blocks == null)
			{
				_logger.LogWarning("[JitCache] Invalid cache file format");
				return;
			}
			
			foreach (var block in cacheData.Blocks)
			{
				_blockCache[block.StartAddress] = block;
				_hashToAddress[block.CodeHash] = block.StartAddress;
			}
			
			_logger.LogInformation("[JitCache] Loaded {Count} blocks from cache", cacheData.Blocks.Count);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[JitCache] Failed to load cache from {CacheFilePath}", cacheFilePath);
		}
	}
	
	/// <summary>
	/// Saves the current cache to disk for a specific executable
	/// </summary>
	public async Task SaveCacheAsync(string executablePath)
	{
		var cacheFileName = GetCacheFileName(executablePath);
		var cacheFilePath = Path.Combine(_cacheDirectory, cacheFileName);
		
		try
		{
			var cacheData = new JitCacheData
			{
				Version = 1,
				ExecutablePath = executablePath,
				Timestamp = DateTime.UtcNow,
				Blocks = _blockCache.Values.ToList()
			};
			
			var json = JsonSerializer.Serialize(cacheData, new JsonSerializerOptions
			{
				WriteIndented = true
			});
			
			await File.WriteAllTextAsync(cacheFilePath, json);
			_logger.LogInformation("[JitCache] Saved {Count} blocks to cache", cacheData.Blocks.Count);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[JitCache] Failed to save cache to {CacheFilePath}", cacheFilePath);
		}
	}
	
	/// <summary>
	/// Clears all cached blocks
	/// </summary>
	public void Clear()
	{
		_blockCache.Clear();
		_hashToAddress.Clear();
		_logger.LogInformation("[JitCache] Cache cleared");
	}
	
	/// <summary>
	/// Gets the cache file name for an executable
	/// </summary>
	private static string GetCacheFileName(string executablePath)
	{
		// Use SHA256 hash of the executable path for the cache file name
		var hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(executablePath));
		var hashString = Convert.ToHexString(hash)[..16]; // Use first 16 characters
		return $"jit_cache_{hashString}.json";
	}
	
	/// <summary>
	/// Computes a hash for a block of x86 code
	/// </summary>
	public static string ComputeCodeHash(ReadOnlySpan<byte> code)
	{
		var hash = SHA256.HashData(code);
		return Convert.ToHexString(hash);
	}
	
	/// <summary>
	/// Gets statistics about the cache
	/// </summary>
	public CacheStatistics GetStatistics()
	{
		return new CacheStatistics
		{
			TotalBlocks = _blockCache.Count,
			TotalInstructions = _blockCache.Values.Sum(b => b.InstructionCount),
			CacheDirectory = _cacheDirectory
		};
	}
}

/// <summary>
/// Metadata about a compiled x86 code block
/// </summary>
public class BlockMetadata
{
	/// <summary>
	/// Starting address (EIP) of the block
	/// </summary>
	public uint StartAddress { get; set; }
	
	/// <summary>
	/// Number of instructions in the block
	/// </summary>
	public int InstructionCount { get; set; }
	
	/// <summary>
	/// Length of the block in bytes
	/// </summary>
	public int ByteLength { get; set; }
	
	/// <summary>
	/// SHA256 hash of the x86 code bytes
	/// </summary>
	public string CodeHash { get; set; } = string.Empty;
	
	/// <summary>
	/// Timestamp when this block was first compiled
	/// </summary>
	public DateTime FirstCompiled { get; set; }
	
	/// <summary>
	/// Number of times this block has been executed
	/// </summary>
	public long ExecutionCount { get; set; }
	
	/// <summary>
	/// Whether this block ends with a call instruction
	/// </summary>
	public bool EndsWithCall { get; set; }
	
	/// <summary>
	/// Whether this block ends with a return instruction
	/// </summary>
	public bool EndsWithReturn { get; set; }
	
	/// <summary>
	/// Target address if this block ends with a direct jump/call
	/// </summary>
	public uint? DirectTarget { get; set; }
}

/// <summary>
/// Container for serialized cache data
/// </summary>
internal class JitCacheData
{
	public int Version { get; set; }
	public string ExecutablePath { get; set; } = string.Empty;
	public DateTime Timestamp { get; set; }
	public List<BlockMetadata> Blocks { get; set; } = new();
}

/// <summary>
/// Statistics about the JIT cache
/// </summary>
public class CacheStatistics
{
	public int TotalBlocks { get; set; }
	public int TotalInstructions { get; set; }
	public string CacheDirectory { get; set; } = string.Empty;
}
