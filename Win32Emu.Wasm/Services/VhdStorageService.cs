using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Win32Emu.Wasm.Services;

public class VhdStorageService
{
	private readonly IJSRuntime _jsRuntime;
	private readonly ILogger<VhdStorageService> _logger;

	public VhdStorageService(IJSRuntime jsRuntime, ILogger<VhdStorageService> logger)
	{
		_jsRuntime = jsRuntime;
		_logger = logger;
	}

	public async Task<bool> SaveAsync(string name, string executablePath, byte[] data)
	{
		try
		{
			_logger.LogInformation("[VHD Storage] Saving VHD {Name} ({Size} bytes)", name, data.Length);
			return await _jsRuntime.InvokeAsync<bool>("saveVhdImage", name, executablePath, data);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[VHD Storage] Failed to save VHD {Name}", name);
			return false;
		}
	}

	public async Task<VhdImage?> LoadAsync(string name)
	{
		try
		{
			var dto = await _jsRuntime.InvokeAsync<VhdImageDto?>("loadVhdImage", name);
			if (dto?.Bytes == null)
			{
				_logger.LogWarning("[VHD Storage] VHD not found: {Name}", name);
				return null;
			}

			var metadata = new VhdImage
			{
				Name = dto.Name ?? name,
				ExecutablePath = dto.ExecutablePath ?? string.Empty,
				Size = dto.Bytes.LongLength,
				Data = dto.Bytes,
				UpdatedAt = ParseTimestamp(dto.UpdatedAt)
			};

			return metadata;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[VHD Storage] Failed to load VHD {Name}", name);
			return null;
		}
	}

	public async Task<List<VhdMetadata>> ListAsync()
	{
		try
		{
			var items = await _jsRuntime.InvokeAsync<List<VhdMetadataDto>?>("listVhdImages");
			var result = new List<VhdMetadata>();
			if (items == null)
			{
				return result;
			}

			foreach (var item in items)
			{
				result.Add(new VhdMetadata
				{
					Name = item.Name ?? string.Empty,
					ExecutablePath = item.ExecutablePath ?? string.Empty,
					Size = item.Size,
					UpdatedAt = ParseTimestamp(item.UpdatedAt)
				});
			}

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[VHD Storage] Failed to list VHD images");
			return new List<VhdMetadata>();
		}
	}

	public async Task<bool> DeleteAsync(string name)
	{
		try
		{
			return await _jsRuntime.InvokeAsync<bool>("deleteVhdImage", name);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[VHD Storage] Failed to delete VHD {Name}", name);
			return false;
		}
	}

	private static DateTime ParseTimestamp(string? updatedAt)
	{
		if (DateTime.TryParse(updatedAt, out var parsed))
		{
			return parsed;
		}

		return DateTime.UtcNow;
	}

	private class VhdImageDto
	{
		public string? Name { get; set; }
		public string? ExecutablePath { get; set; }
		public string? UpdatedAt { get; set; }
		public byte[]? Bytes { get; set; }
	}

	private class VhdMetadataDto
	{
		public string? Name { get; set; }
		public string? ExecutablePath { get; set; }
		public long Size { get; set; }
		public string? UpdatedAt { get; set; }
	}
}

public class VhdMetadata
{
	public string Name { get; set; } = string.Empty;
	public string ExecutablePath { get; set; } = string.Empty;
	public long Size { get; set; }
	public DateTime UpdatedAt { get; set; }
}

public class VhdImage : VhdMetadata
{
	public byte[] Data { get; set; } = Array.Empty<byte>();
}
