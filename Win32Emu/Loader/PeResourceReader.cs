using System;
using System.Collections.Generic;
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
	public const uint RT_CURSOR = 1;
	public const uint RT_BITMAP = 2;
	public const uint RT_ICON = 3;
	public const uint RT_MENU = 4;
	public const uint RT_DIALOG = 5;
	public const uint RT_STRING = 6;
	public const uint RT_FONTDIR = 7;
	public const uint RT_FONT = 8;
	public const uint RT_ACCELERATOR = 9;
	public const uint RT_RCDATA = 10;
	public const uint RT_MESSAGETABLE = 11;
	public const uint RT_GROUP_CURSOR = 12;
	public const uint RT_GROUP_ICON = 14;
	public const uint RT_VERSION = 16;
	public const uint RT_DLGINCLUDE = 17;
	public const uint RT_PLUGPLAY = 19;
	public const uint RT_VXD = 20;
	public const uint RT_ANICURSOR = 21;
	public const uint RT_ANIICON = 22;
	public const uint RT_HTML = 23;
	public const uint RT_MANIFEST = 24;

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
}
