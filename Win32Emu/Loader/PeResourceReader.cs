using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsmResolver;
using AsmResolver.PE;
using AsmResolver.PE.File;
using AsmResolver.PE.Win32Resources;
using Win32Emu.Memory;

namespace Win32Emu.Loader;

/// <summary>
/// Reads PE resources from a loaded PE image.
/// Supports reading resource directory structures and extracting resource data.
/// </summary>
public class PeResourceReader
{
	private readonly PEImage _image;
	private readonly uint _imageBase;
	private readonly VirtualMemory _memory;

	// Resource type constants
	public enum ResourceType : uint
	{
		RT_CURSOR = 1,
		RT_BITMAP = 2,
		RT_ICON = 3,
		RT_MENU = 4,
		RT_DIALOG = 5,
		RT_STRING = 6,
		RT_FONTDIR = 7,
		RT_FONT = 8,
		RT_ACCELERATOR = 9,
		RT_RCDATA = 10,
		RT_MESSAGETABLE = 11,
		RT_GROUP_CURSOR = 12,
		RT_GROUP_ICON = 14,
		RT_VERSION = 16,
		RT_DLGINCLUDE = 17,
		RT_PLUGPLAY = 19,
		RT_VXD = 20,
		RT_ANICURSOR = 21,
		RT_ANIICON = 22,
		RT_HTML = 23,
		RT_MANIFEST = 24
	}

	public PeResourceReader(PEImage image, uint imageBase, VirtualMemory memory)
	{
		_image = image ?? throw new ArgumentNullException(nameof(image));
		_imageBase = imageBase;
		_memory = memory ?? throw new ArgumentNullException(nameof(memory));
	}

	/// <summary>
	/// Finds a resource by type, name, and language.
	/// </summary>
	/// <param name="lpType">Resource type (can be integer ID or string name)</param>
	/// <param name="lpName">Resource name (can be integer ID or string name)</param>
	/// <param name="wLanguage">Language ID (0 for default, 0x0409 for English US)</param>
	/// <returns>Handle to the resource information block, or 0 if not found</returns>
	public uint FindResource(uint lpType, uint lpName, ushort wLanguage = 0)
	{
		// Get the resource directory from the PE image
		var resources = _image.Resources;
		if (resources == null)
		{
			return 0;
		}

		// Determine if lpType is an ID or a string
		var typeId = IsIntResource(lpType) ? (uint?)GetIntResource(lpType) : null;
		var typeName = typeId == null ? ReadResourceString(lpType) : null;

		// Determine if lpName is an ID or a string
		var nameId = IsIntResource(lpName) ? (uint?)GetIntResource(lpName) : null;
		var nameNameStr = nameId == null ? ReadResourceString(lpName) : null;

		// If no language specified, default to English (US)
		if (wLanguage == 0)
		{
			wLanguage = 0x0409; // LANG_ENGLISH, SUBLANG_ENGLISH_US
		}
		
		// Navigate the resource directory tree: Type -> Name -> Language
		// For now, we'll create a synthetic handle that encodes the type, name, and language
		// The handle will be used by LoadResource to retrieve the actual data
		
		// Create a resource handle (synthetic identifier)
		// Format: 0x80000000 | (type << 16) | name | (language << 0)
		// Note: We don't encode language in the handle as we'll use the stored _preferredLanguage
		var resourceHandle = 0x80000000u | ((typeId ?? 0) << 16) | (nameId ?? 0);
		
		// Store the preferred language for this lookup
		_preferredLanguage = wLanguage;
		
		return resourceHandle;
	}
	
	private ushort _preferredLanguage = 0x0409; // Default to English (US)

	/// <summary>
	/// Loads a resource into memory.
	/// </summary>
	/// <param name="hModule">Handle to the module containing the resource</param>
	/// <param name="hResInfo">Handle to the resource (from FindResource)</param>
	/// <returns>Handle to the loaded resource data, or 0 if failed</returns>
	public uint LoadResource(uint hModule, uint hResInfo)
	{
		if (hResInfo == 0)
		{
			return 0;
		}

		// Get the resource directory from the PE image
		var resources = _image.Resources;
		if (resources == null)
		{
			return 0;
		}

		// Extract type and name from the resource handle
		var typeId = (hResInfo >> 16) & 0x7FFF;
		var nameId = hResInfo & 0xFFFF;

		// Try to find the resource data by navigating the directory tree
		var resourceData = FindResourceData(resources, typeId, nameId);
		if (resourceData == null || resourceData.Length == 0)
		{
			return 0;
		}

		// Allocate resource in a safe memory range
		// Use 0x0D000000 - 0x0E000000 range for resources (208-224 MB range, before imports at 0x0F000000)
		var resourceAddress = 0x0D000000u + (uint)(_resourceCache.Count * 0x10000);
		_resourceCache[resourceAddress] = resourceData;

		return resourceAddress;
	}

	private readonly Dictionary<uint, byte[]> _resourceCache = new();

	/// <summary>
	/// Gets the size of a resource.
	/// </summary>
	/// <param name="hModule">Handle to the module containing the resource</param>
	/// <param name="hResInfo">Handle to the resource</param>
	/// <returns>Size of the resource in bytes, or 0 if failed</returns>
	public uint SizeofResource(uint hModule, uint hResInfo)
	{
		if (hResInfo == 0)
		{
			return 0;
		}

		var resources = _image.Resources;
		if (resources == null)
		{
			return 0;
		}

		var typeId = (hResInfo >> 16) & 0x7FFF;
		var nameId = hResInfo & 0xFFFF;

		var resourceData = FindResourceData(resources, typeId, nameId);
		return resourceData != null ? (uint)resourceData.Length : 0;
	}

	/// <summary>
	/// Locks a resource into memory (no-op in our implementation as resources are already loaded).
	/// </summary>
	/// <param name="hResData">Handle to the resource data</param>
	/// <returns>Pointer to the resource data</returns>
	public uint LockResource(uint hResData)
	{
		// In Win32, LockResource returns a pointer to the resource data
		// Since LoadResource returns a handle that's already an address, just return it
		// But first copy the data to actual memory if it's in our cache
		if (_resourceCache.TryGetValue(hResData, out var data))
		{
			// Write the resource data to memory at this address
			_memory.WriteBytes(hResData, data);
		}
		return hResData;
	}

	private byte[]? FindResourceData(ResourceDirectory directory, uint typeId, uint nameId)
	{
		// Navigate: Type -> Name -> Language
		foreach (var typeEntry in directory.Entries)
		{
			// Check if this is the type we're looking for
			var typeMatch = false;
			if (typeEntry.Id == typeId)
			{
				typeMatch = true;
			}

			if (!typeMatch)
			{
				continue;
			}

			// Type matched, now look for the name
			if (typeEntry is not ResourceDirectory typeDir)
			{
				continue;
			}

			foreach (var nameEntry in typeDir.Entries)
			{
				var nameMatch = false;
				if (nameEntry.Id == nameId)
				{
					nameMatch = true;
				}

				if (!nameMatch)
				{
					continue;
				}

				// Name matched, now get the language entry
				if (nameEntry is not ResourceDirectory nameDir)
				{
					continue;
				}

				// Try to find the preferred language first
				ResourceData? preferredData = null;
				ResourceData? fallbackData = null;
				
				foreach (var langEntry in nameDir.Entries)
				{
					if (langEntry is ResourceData data && data.Contents != null)
					{
						// Check if this is the preferred language
						if (langEntry.Id == _preferredLanguage)
						{
							preferredData = data;
							break; // Found exact match
						}
						
						// Keep the first entry as fallback
						fallbackData ??= data;
					}
				}
				
				// Use preferred language if found, otherwise use fallback
				var selectedData = preferredData ?? fallbackData;
				if (selectedData?.Contents != null)
				{
					return selectedData.Contents.WriteIntoArray();
				}
			}
		}

		return null;
	}

	private bool IsIntResource(uint ptr)
	{
		// In Win32, if the high word is 0, it's an integer resource ID
		return (ptr & 0xFFFF0000) == 0;
	}

	private uint GetIntResource(uint ptr)
	{
		return ptr & 0xFFFF;
	}

	private string? ReadResourceString(uint ptr)
	{
		if (ptr == 0)
		{
			return null;
		}

		// Read a null-terminated ASCII string from memory
		var sb = new StringBuilder();
		var offset = 0u;
		while (true)
		{
			var b = _memory.Read8(ptr + offset);
			if (b == 0)
			{
				break;
			}
			sb.Append((char)b);
			offset++;
			if (offset > 256) // Safety limit
			{
				break;
			}
		}
		return sb.ToString();
	}

	/// <summary>
	/// Loads a string from the string table resource.
	/// String resources are stored in blocks of 16 strings per resource.
	/// </summary>
	/// <param name="stringId">The string resource ID</param>
	/// <returns>The string, or null if not found</returns>
	public string? LoadString(uint stringId)
	{
		// String resources are organized in blocks of 16 strings
		// The block ID is (stringId / 16) + 1
		// The index within the block is stringId % 16
		var blockId = (stringId / 16) + 1;
		var indexInBlock = (int)(stringId % 16);

		// Find the string table resource
		var resources = _image.Resources;
		if (resources == null)
		{
			return null;
		}
		
		var resourceData = FindResourceData(resources, (uint)ResourceType.RT_STRING, blockId);
		if (resourceData == null || resourceData.Length == 0)
		{
			return null;
		}

		// String table format:
		// Each entry is a length-prefixed WCHAR string (length is in WCHARs, not bytes)
		// Length is stored as a WORD (2 bytes), followed by that many WCHARs
		var offset = 0;
		for (var i = 0; i <= indexInBlock; i++)
		{
			if (offset + 2 > resourceData.Length)
			{
				return null; // Not enough data
			}

			// Read the length (in WCHARs) - already validated offset is in bounds
			var length = BitConverter.ToUInt16(resourceData, offset);
			offset += 2;

			if (i == indexInBlock)
			{
				// This is the string we want
				if (length == 0)
				{
					return string.Empty;
				}

				if (offset + (length * 2) > resourceData.Length)
				{
					return null; // Not enough data
				}

				// Read the Unicode string
				var stringBytes = new byte[length * 2];
				Array.Copy(resourceData, offset, stringBytes, 0, length * 2);
				return Encoding.Unicode.GetString(stringBytes);
			}

			// Skip this string
			offset += length * 2;
		}

		return null;
	}

	/// <summary>
	/// Loads a bitmap resource and returns the raw bitmap data.
	/// </summary>
	/// <param name="bitmapId">The bitmap resource ID or name</param>
	/// <returns>The bitmap data (DIB format), or null if not found</returns>
	public byte[]? LoadBitmap(uint bitmapId)
	{
		// Try to find the bitmap resource
		var resources = _image.Resources;
		if (resources == null)
		{
			return null;
		}
		
		var resourceData = FindResourceData(resources, (uint)ResourceType.RT_BITMAP, bitmapId);
		return resourceData;
	}

	/// <summary>
	/// Loads a bitmap resource by name.
	/// </summary>
	/// <param name="bitmapName">The bitmap resource name</param>
	/// <returns>The bitmap data (DIB format), or null if not found</returns>
	public byte[]? LoadBitmapByName(string bitmapName)
	{
		// Find bitmap by name
		var resources = _image.Resources;
		if (resources == null)
		{
			return null;
		}

		// Navigate: Type (RT_BITMAP) -> Name -> Language
		var typeEntry = resources.Entries.Where(e => e.Id == (uint)ResourceType.RT_BITMAP).FirstOrDefault();
		if (typeEntry is ResourceDirectory typeDir)
		{
			// Try case-sensitive match first, then case-insensitive
			// Note: If multiple resources exist with names differing only in case,
			// the case-insensitive search will return the first match found
			var nameEntry = typeDir.Entries.Where(e => e.Name == bitmapName).FirstOrDefault();
			if (nameEntry == null)
			{
				nameEntry = typeDir.Entries.Where(e => string.Equals(e.Name, bitmapName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
			}
			
			if (nameEntry is ResourceDirectory nameDir)
			{
				// Get first language version
				var langEntry = nameDir.Entries.OfType<ResourceData>().Where(d => d.Contents != null).FirstOrDefault();
				if (langEntry?.Contents != null)
				{
					return langEntry.Contents.WriteIntoArray();
				}
			}
		}

		return null;
	}

	/// <summary>
	/// Enumerates all resource names for a specified type
	/// </summary>
	/// <param name="lpType">Resource type (can be integer ID or string name)</param>
	/// <returns>List of resource IDs/names, or null if type not found</returns>
	public IEnumerable<uint>? EnumerateResourceNames(uint lpType)
	{
		var resources = _image.Resources;
		if (resources == null)
		{
			return null;
		}

		// Determine if lpType is an ID or a string
		var typeId = IsIntResource(lpType) ? (uint?)GetIntResource(lpType) : null;
		var typeName = typeId == null ? ReadResourceString(lpType) : null;

		// Find the type directory
		IResourceEntry? typeEntry = null;
		if (typeId.HasValue)
		{
			typeEntry = resources.Entries.Where(e => e.Id == typeId.Value).FirstOrDefault();
		}
		else if (typeName != null)
		{
			typeEntry = resources.Entries.Where(e => e.Name == typeName).FirstOrDefault();
		}

		if (typeEntry is not ResourceDirectory typeDir)
		{
			return null;
		}

		// Collect all resource names/IDs
		var resourceNames = new List<uint>();
		foreach (var nameEntry in typeDir.Entries)
		{
			// Return the ID if it's a numeric ID
			// Check if it has an ID (non-zero means it's an ID resource)
			if (nameEntry.Id != 0)
			{
				resourceNames.Add(nameEntry.Id);
			}
			else if (nameEntry.Name != null)
			{
				// For string names, we'd need to allocate memory for the string
				// For now, just skip string names or use a hash as a pseudo-ID
				// This is a limitation of the simple stub implementation
			}
		}

		return resourceNames;
	}
}
