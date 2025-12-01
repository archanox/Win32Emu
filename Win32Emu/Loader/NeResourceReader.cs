using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Memory;

namespace Win32Emu.Loader;

/// <summary>
/// Reads resources from NE (New Executable) format files.
/// NE resources are organized in a resource table with type and name information blocks.
/// 
/// NE Resource Table Format:
/// - Alignment shift count (WORD): Used to calculate actual offsets
/// - Resource type blocks: One per resource type
///   - Type ID or name offset (WORD)
///   - Count of resources of this type (WORD)
///   - Reserved (DWORD)
///   - Resource entries:
///     - Offset (WORD): Shifted by alignment shift count
///     - Length (WORD): Shifted by alignment shift count
///     - Flags (WORD)
///     - Resource ID (WORD)
///     - Reserved (DWORD)
/// - Terminator: 0x0000 (WORD)
/// </summary>
public class NeResourceReader : IResourceReader
{
	private readonly byte[] _fileBytes;
	private readonly VirtualMemory _memory;
	private readonly ILogger _logger;
	private readonly Dictionary<uint, byte[]> _resourceCache = new();
	private readonly Dictionary<uint, uint> _resourceHandleToAddress = new(); // Maps hResInfo -> memory address
	private readonly List<NeResource> _resources = new();
	private ushort _alignmentShift;
	private int _neHeaderOffset;
	private uint _nextResourceAddress = 0x10000000u; // Starting address for resource allocation (outside COM vtable region)
	
	// DOS header constant
	private const int DOS_HEADER_NE_OFFSET = 0x3C;
	
	// NE header resource table offset location
	private const int NE_RESOURCE_TABLE_OFFSET_LOCATION = 36;
	
	public NeResourceReader(byte[] fileBytes, VirtualMemory memory, ILogger? logger = null)
	{
		_fileBytes = fileBytes ?? throw new ArgumentNullException(nameof(fileBytes));
		_memory = memory ?? throw new ArgumentNullException(nameof(memory));
		_logger = logger ?? NullLogger.Instance;
		
		ParseResourceTable();
	}
	
	private void ParseResourceTable()
	{
		// Get NE header offset from DOS header
		if (_fileBytes.Length < DOS_HEADER_NE_OFFSET + 4)
		{
			_logger.LogWarning("[NE Resources] File too small to contain NE header offset");
			return;
		}
		
		_neHeaderOffset = (int)BitConverter.ToUInt32(_fileBytes, DOS_HEADER_NE_OFFSET);
		
		// Get resource table offset from NE header
		if (_neHeaderOffset + NE_RESOURCE_TABLE_OFFSET_LOCATION + 2 > _fileBytes.Length)
		{
			_logger.LogWarning("[NE Resources] Invalid NE header offset");
			return;
		}
		
		var resourceTableOffset = BitConverter.ToUInt16(_fileBytes, _neHeaderOffset + NE_RESOURCE_TABLE_OFFSET_LOCATION);
		if (resourceTableOffset == 0)
		{
			_logger.LogDebug("[NE Resources] No resource table in this NE file");
			return;
		}
		
		// Calculate absolute offset of resource table
		var tableOffset = _neHeaderOffset + resourceTableOffset;
		if (tableOffset + 2 > _fileBytes.Length)
		{
			_logger.LogWarning("[NE Resources] Resource table offset extends beyond file");
			return;
		}
		
		// Read alignment shift count
		_alignmentShift = BitConverter.ToUInt16(_fileBytes, tableOffset);
		_logger.LogDebug("[NE Resources] Alignment shift: {Shift}", _alignmentShift);
		
		var offset = tableOffset + 2;
		
		// Parse resource type blocks
		while (offset + 8 <= _fileBytes.Length)
		{
			// Read type ID (0x0000 marks end of resource table)
			var typeId = BitConverter.ToUInt16(_fileBytes, offset);
			if (typeId == 0)
			{
				break;
			}
			
			// Read count of resources of this type
			var count = BitConverter.ToUInt16(_fileBytes, offset + 2);
			// Skip reserved DWORD
			offset += 8;
			
			// Determine the actual type value
			// If high bit is set (0x8000), the low bits are the type ID
			// Otherwise, it's an offset to a type name string
			ushort actualTypeId;
			string? typeName = null;
			if ((typeId & 0x8000) != 0)
			{
				actualTypeId = (ushort)(typeId & 0x7FFF);
			}
			else
			{
				// Type name is at offset relative to resource table start
				typeName = ReadResourceName(tableOffset + typeId);
				actualTypeId = 0; // Named type
			}
			
			_logger.LogDebug("[NE Resources] Found resource type {TypeId} ({TypeName}) with {Count} resources",
				actualTypeId, typeName ?? GetResourceTypeName(actualTypeId), count);
			
			// Parse individual resources
			for (var i = 0; i < count; i++)
			{
				if (offset + 12 > _fileBytes.Length)
				{
					_logger.LogWarning("[NE Resources] Resource entry extends beyond file");
					break;
				}
				
				var resourceOffset = (uint)BitConverter.ToUInt16(_fileBytes, offset) << _alignmentShift;
				var resourceLength = (uint)BitConverter.ToUInt16(_fileBytes, offset + 2) << _alignmentShift;
				var flags = BitConverter.ToUInt16(_fileBytes, offset + 4);
				var resourceId = BitConverter.ToUInt16(_fileBytes, offset + 6);
				// Skip reserved DWORD (offset + 8)
				
				// Determine the actual resource ID/name
				ushort actualResourceId;
				string? resourceName = null;
				if ((resourceId & 0x8000) != 0)
				{
					actualResourceId = (ushort)(resourceId & 0x7FFF);
				}
				else
				{
					// Resource name is at offset relative to resource table start
					resourceName = ReadResourceName(tableOffset + resourceId);
					actualResourceId = 0; // Named resource
				}
				
				var resource = new NeResource
				{
					TypeId = actualTypeId,
					TypeName = typeName,
					ResourceId = actualResourceId,
					ResourceName = resourceName,
					FileOffset = resourceOffset,
					Length = resourceLength,
					Flags = flags
				};
				
				_resources.Add(resource);
				
				_logger.LogDebug("[NE Resources] Resource: Type={TypeId}, ID={ResourceId} ({Name}), Offset=0x{Offset:X}, Length={Length}",
					actualTypeId, actualResourceId, resourceName ?? actualResourceId.ToString(), resourceOffset, resourceLength);
				
				offset += 12;
			}
		}
		
		_logger.LogInformation("[NE Resources] Parsed {Count} resources", _resources.Count);
	}
	
	private string? ReadResourceName(int offset)
	{
		if (offset >= _fileBytes.Length)
		{
			return null;
		}
		
		// NE resource names are Pascal strings (length byte followed by string)
		var length = _fileBytes[offset];
		if (length == 0 || offset + 1 + length > _fileBytes.Length)
		{
			return null;
		}
		
		return Encoding.ASCII.GetString(_fileBytes, offset + 1, length);
	}
	
	private static string GetResourceTypeName(ushort typeId)
	{
		return typeId switch
		{
			(ushort)IResourceReader.ResourceType.RT_CURSOR => "RT_CURSOR",
			(ushort)IResourceReader.ResourceType.RT_BITMAP => "RT_BITMAP",
			(ushort)IResourceReader.ResourceType.RT_ICON => "RT_ICON",
			(ushort)IResourceReader.ResourceType.RT_MENU => "RT_MENU",
			(ushort)IResourceReader.ResourceType.RT_DIALOG => "RT_DIALOG",
			(ushort)IResourceReader.ResourceType.RT_STRING => "RT_STRING",
			(ushort)IResourceReader.ResourceType.RT_FONTDIR => "RT_FONTDIR",
			(ushort)IResourceReader.ResourceType.RT_FONT => "RT_FONT",
			(ushort)IResourceReader.ResourceType.RT_ACCELERATOR => "RT_ACCELERATOR",
			(ushort)IResourceReader.ResourceType.RT_RCDATA => "RT_RCDATA",
			(ushort)IResourceReader.ResourceType.RT_GROUP_CURSOR => "RT_GROUP_CURSOR",
			(ushort)IResourceReader.ResourceType.RT_GROUP_ICON => "RT_GROUP_ICON",
			(ushort)IResourceReader.ResourceType.RT_VERSION => "RT_VERSION",
			_ => $"Unknown({typeId})"
		};
	}
	
	/// <inheritdoc />
	public uint FindResource(uint lpType, uint lpName, ushort wLanguage = 0)
	{
		var typeId = (ushort)(lpType & 0xFFFF);
		var nameId = (ushort)(lpName & 0xFFFF);
		
		// Find matching resource
		var matchingResource = _resources.FirstOrDefault(r => r.TypeId == typeId && r.ResourceId == nameId);
		if (matchingResource != null)
		{
			// Create a resource handle encoding type and name
			var handle = 0x80000000u | ((uint)typeId << 16) | nameId;
			return handle;
		}
		
		_logger.LogDebug("[NE Resources] Resource not found: Type={TypeId}, Name={NameId}", typeId, nameId);
		return 0;
	}
	
	/// <inheritdoc />
	public uint LoadResource(uint hModule, uint hResInfo)
	{
		if (hResInfo == 0)
		{
			return 0;
		}
		
		// Check if already loaded (return cached address)
		if (_resourceHandleToAddress.TryGetValue(hResInfo, out var cachedAddress))
		{
			return cachedAddress;
		}
		
		var typeId = (ushort)((hResInfo >> 16) & 0x7FFF);
		var nameId = (ushort)(hResInfo & 0xFFFF);
		
		// Find the resource
		var resource = _resources.FirstOrDefault(r => r.TypeId == typeId && r.ResourceId == nameId);
		if (resource != null)
		{
			// Read resource data from file
			if (resource.FileOffset + resource.Length > _fileBytes.Length)
			{
				_logger.LogWarning("[NE Resources] Resource data extends beyond file");
				return 0;
			}
			
			var data = new byte[resource.Length];
			Array.Copy(_fileBytes, resource.FileOffset, data, 0, resource.Length);
			
			// Allocate memory for the resource at a unique address
			var address = _nextResourceAddress;
			if (_nextResourceAddress > 0xFFC00000u)
			{
				throw new OutOfMemoryException("Resource allocation limit exceeded");
			}
			_nextResourceAddress += 0x10000; // 64KB per resource allocation slot
			
			// Store in both caches
			_resourceCache[address] = data;
			_resourceHandleToAddress[hResInfo] = address;
			
			_logger.LogDebug("[NE Resources] Loaded resource Type={TypeId}, ID={NameId} at 0x{Address:X8} ({Length} bytes)",
				typeId, nameId, address, data.Length);
			
			return address;
		}
		
		return 0;
	}
	
	/// <inheritdoc />
	public uint SizeofResource(uint hModule, uint hResInfo)
	{
		if (hResInfo == 0)
		{
			return 0;
		}
		
		var typeId = (ushort)((hResInfo >> 16) & 0x7FFF);
		var nameId = (ushort)(hResInfo & 0xFFFF);
		
		var resource = _resources.FirstOrDefault(r => r.TypeId == typeId && r.ResourceId == nameId);
		return resource?.Length ?? 0;
	}
	
	/// <inheritdoc />
	public uint LockResource(uint hResData)
	{
		if (_resourceCache.TryGetValue(hResData, out var data))
		{
			_memory.WriteBytes(hResData, data);
		}
		return hResData;
	}
	
	/// <inheritdoc />
	public string? LoadString(uint stringId)
	{
		// String resources are organized in blocks of 16 strings
		var blockId = (ushort)((stringId / 16) + 1);
		var indexInBlock = (int)(stringId % 16);
		
		// Find the string table resource
		var resource = _resources.FirstOrDefault(r => 
			r.TypeId == (ushort)IResourceReader.ResourceType.RT_STRING && r.ResourceId == blockId);
		
		if (resource == null)
		{
			return null;
		}
		
		if (resource.FileOffset + resource.Length > _fileBytes.Length)
		{
			return null;
		}
		
		// Parse string table
		var offset = (int)resource.FileOffset;
		var endOffset = offset + (int)resource.Length;
		
		for (var i = 0; i <= indexInBlock && offset < endOffset; i++)
		{
			if (offset + 1 > endOffset)
			{
				return null;
			}
			
			// String length in bytes (NE uses Pascal strings, not WCHAR)
			var length = _fileBytes[offset];
			offset++;
			
			if (i == indexInBlock)
			{
				if (length == 0)
				{
					return string.Empty;
				}
				
				if (offset + length > endOffset)
				{
					return null;
				}
				
				// NE string resources are typically ANSI, not Unicode
				return Encoding.ASCII.GetString(_fileBytes, offset, length);
			}
			
			offset += length;
		}
		
		return null;
	}
	
	/// <inheritdoc />
	public byte[]? LoadBitmap(uint bitmapId)
	{
		var resource = _resources.FirstOrDefault(r => 
			r.TypeId == (ushort)IResourceReader.ResourceType.RT_BITMAP && r.ResourceId == (ushort)bitmapId);
		
		if (resource == null)
		{
			return null;
		}
		
		if (resource.FileOffset + resource.Length > _fileBytes.Length)
		{
			return null;
		}
		
		var data = new byte[resource.Length];
		Array.Copy(_fileBytes, resource.FileOffset, data, 0, resource.Length);
		return data;
	}
	
	/// <inheritdoc />
	public byte[]? LoadBitmapByName(string bitmapName)
	{
		var resource = _resources.FirstOrDefault(r => 
			r.TypeId == (ushort)IResourceReader.ResourceType.RT_BITMAP && 
			string.Equals(r.ResourceName, bitmapName, StringComparison.OrdinalIgnoreCase));
		
		if (resource == null)
		{
			return null;
		}
		
		if (resource.FileOffset + resource.Length > _fileBytes.Length)
		{
			return null;
		}
		
		var data = new byte[resource.Length];
		Array.Copy(_fileBytes, resource.FileOffset, data, 0, resource.Length);
		return data;
	}
	
	/// <inheritdoc />
	public IEnumerable<uint>? EnumerateResourceNames(uint lpType)
	{
		var typeId = (ushort)(lpType & 0xFFFF);
		var names = _resources
			.Where(r => r.TypeId == typeId && r.ResourceName == null)
			.Select(r => (uint)r.ResourceId)
			.ToList();
		
		return names.Count > 0 ? names : null;
	}
	
	/// <summary>
	/// Internal resource information structure.
	/// </summary>
	private class NeResource
	{
		public ushort TypeId { get; init; }
		public string? TypeName { get; init; }
		public ushort ResourceId { get; init; }
		public string? ResourceName { get; init; }
		public uint FileOffset { get; init; }
		public uint Length { get; init; }
		public ushort Flags { get; init; }
	}
}

/// <summary>
/// NE resource flags.
/// </summary>
[Flags]
internal enum NeResourceFlags : ushort
{
	/// <summary>No flags set</summary>
	None = 0x0000,
	/// <summary>Resource is moveable in memory</summary>
	Moveable = 0x0010,
	/// <summary>Resource is pure (shareable)</summary>
	Pure = 0x0020,
	/// <summary>Resource should be preloaded</summary>
	Preload = 0x0040,
}
