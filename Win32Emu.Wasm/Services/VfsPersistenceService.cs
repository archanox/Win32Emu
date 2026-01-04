using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Win32Emu.Wasm.Services;

/// <summary>
/// Service for persisting VFS state to browser IndexedDB.
/// Enables saving and loading of virtual file system snapshots between sessions.
/// </summary>
public class VfsPersistenceService : IDisposable
{
	private readonly IJSRuntime _jsRuntime;
	private readonly ILogger<VfsPersistenceService> _logger;
	private bool _disposed;

	public VfsPersistenceService(IJSRuntime jsRuntime, ILogger<VfsPersistenceService> logger)
	{
		_jsRuntime = jsRuntime;
		_logger = logger;
	}

	/// <summary>
	/// Save VFS state to IndexedDB.
	/// </summary>
	/// <param name="stateName">Name/ID for this save state</param>
	/// <param name="executableName">Name of the executable this state is for</param>
	/// <param name="files">Dictionary of VFS files (path -> byte array)</param>
	/// <returns>True if save succeeded</returns>
	public async Task<bool> SaveVfsStateAsync(string stateName, string? executableName, IReadOnlyDictionary<string, byte[]> files)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		try
		{
			_logger.LogInformation("[VfsPersistence] Saving VFS state: {StateName} ({FileCount} files)", 
				stateName, files.Count);

			// Convert byte arrays to base64 for JSON serialization
			var filesDict = new Dictionary<string, string>();
			foreach (var kvp in files)
			{
				filesDict[kvp.Key] = Convert.ToBase64String(kvp.Value);
			}

			var filesJson = JsonSerializer.Serialize(filesDict);
			var success = await _jsRuntime.InvokeAsync<bool>("saveVfsState", stateName, executableName ?? "Unknown", filesJson);

			if (success)
			{
				_logger.LogInformation("[VfsPersistence] Successfully saved VFS state: {StateName}", stateName);
			}
			else
			{
				_logger.LogWarning("[VfsPersistence] Failed to save VFS state: {StateName}", stateName);
			}

			return success;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[VfsPersistence] Error saving VFS state: {StateName}", stateName);
			return false;
		}
	}

	/// <summary>
	/// Load VFS state from IndexedDB.
	/// </summary>
	/// <param name="stateName">Name/ID of the state to load</param>
	/// <returns>Dictionary of VFS files or null if not found</returns>
	public async Task<Dictionary<string, byte[]>?> LoadVfsStateAsync(string stateName)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		try
		{
			_logger.LogInformation("[VfsPersistence] Loading VFS state: {StateName}", stateName);

			var filesJson = await _jsRuntime.InvokeAsync<string?>("loadVfsState", stateName);
			if (string.IsNullOrEmpty(filesJson))
			{
				_logger.LogInformation("[VfsPersistence] VFS state not found: {StateName}", stateName);
				return null;
			}

			// Deserialize and convert base64 back to byte arrays
			var filesDict = JsonSerializer.Deserialize<Dictionary<string, string>>(filesJson);
			if (filesDict == null)
			{
				_logger.LogWarning("[VfsPersistence] Failed to deserialize VFS state: {StateName}", stateName);
				return null;
			}

			var result = new Dictionary<string, byte[]>();
			foreach (var kvp in filesDict)
			{
				result[kvp.Key] = Convert.FromBase64String(kvp.Value);
			}

			_logger.LogInformation("[VfsPersistence] Successfully loaded VFS state: {StateName} ({FileCount} files)", 
				stateName, result.Count);

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[VfsPersistence] Error loading VFS state: {StateName}", stateName);
			return null;
		}
	}

	/// <summary>
	/// List all saved VFS states.
	/// </summary>
	/// <returns>List of VFS state metadata</returns>
	public async Task<List<VfsStateMetadata>> ListVfsStatesAsync()
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		try
		{
			_logger.LogDebug("[VfsPersistence] Listing VFS states");

			var metadataJson = await _jsRuntime.InvokeAsync<string>("listVfsStates");
			if (string.IsNullOrEmpty(metadataJson))
			{
				return new List<VfsStateMetadata>();
			}

			var metadata = JsonSerializer.Deserialize<List<VfsStateMetadata>>(metadataJson);
			return metadata ?? new List<VfsStateMetadata>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[VfsPersistence] Error listing VFS states");
			return new List<VfsStateMetadata>();
		}
	}

	/// <summary>
	/// Delete a saved VFS state.
	/// </summary>
	/// <param name="stateName">Name/ID of the state to delete</param>
	/// <returns>True if deletion succeeded</returns>
	public async Task<bool> DeleteVfsStateAsync(string stateName)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		try
		{
			_logger.LogInformation("[VfsPersistence] Deleting VFS state: {StateName}", stateName);

			var success = await _jsRuntime.InvokeAsync<bool>("deleteVfsState", stateName);
			if (success)
			{
				_logger.LogInformation("[VfsPersistence] Successfully deleted VFS state: {StateName}", stateName);
			}
			else
			{
				_logger.LogWarning("[VfsPersistence] Failed to delete VFS state: {StateName}", stateName);
			}

			return success;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[VfsPersistence] Error deleting VFS state: {StateName}", stateName);
			return false;
		}
	}

	/// <summary>
	/// Clear all saved VFS states.
	/// </summary>
	/// <returns>True if clearing succeeded</returns>
	public async Task<bool> ClearAllVfsStatesAsync()
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		try
		{
			_logger.LogInformation("[VfsPersistence] Clearing all VFS states");

			var success = await _jsRuntime.InvokeAsync<bool>("clearAllVfsStates");
			if (success)
			{
				_logger.LogInformation("[VfsPersistence] Successfully cleared all VFS states");
			}
			else
			{
				_logger.LogWarning("[VfsPersistence] Failed to clear all VFS states");
			}

			return success;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[VfsPersistence] Error clearing all VFS states");
			return false;
		}
	}

	/// <summary>
	/// Get storage usage information.
	/// </summary>
	/// <returns>Storage usage information</returns>
	public async Task<StorageUsageInfo> GetStorageUsageAsync()
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		try
		{
			var usageJson = await _jsRuntime.InvokeAsync<string>("getVfsStorageUsage");
			if (string.IsNullOrEmpty(usageJson))
			{
				return new StorageUsageInfo();
			}

			var usage = JsonSerializer.Deserialize<StorageUsageInfo>(usageJson);
			return usage ?? new StorageUsageInfo();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[VfsPersistence] Error getting storage usage");
			return new StorageUsageInfo();
		}
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
	}
}

/// <summary>
/// Metadata about a saved VFS state.
/// </summary>
public class VfsStateMetadata
{
	public required string Id { get; set; }
	public required string ExecutableName { get; set; }
	public long Timestamp { get; set; }
	public int FileCount { get; set; }

	public DateTime GetDateTime() => DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).LocalDateTime;
}

/// <summary>
/// Storage usage information.
/// </summary>
public class StorageUsageInfo
{
	[JsonPropertyName("usage")]
	public long Usage { get; set; }
	
	[JsonPropertyName("quota")]
	public long Quota { get; set; }
	
	[JsonPropertyName("usagePercent")]
	public double UsagePercent { get; set; }

	public string FormatUsage() => FormatBytes(Usage);
	public string FormatQuota() => FormatBytes(Quota);

	private static string FormatBytes(long bytes)
	{
		string[] sizes = { "B", "KB", "MB", "GB" };
		double len = bytes;
		int order = 0;
		while (len >= 1024 && order < sizes.Length - 1)
		{
			order++;
			len = len / 1024;
		}
		return $"{len:0.##} {sizes[order]}";
	}
}
