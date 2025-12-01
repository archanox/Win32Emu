using System.Collections.Generic;

namespace Win32Emu.Loader;

/// <summary>
/// Interface for reading resources from executable files.
/// Supports both PE and NE executable formats.
/// </summary>
public interface IResourceReader
{
	/// <summary>
	/// Resource type constants.
	/// </summary>
	enum ResourceType : uint
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

	/// <summary>
	/// Finds a resource by type, name, and language.
	/// </summary>
	/// <param name="lpType">Resource type (can be integer ID or string name)</param>
	/// <param name="lpName">Resource name (can be integer ID or string name)</param>
	/// <param name="wLanguage">Language ID (0 for default, 0x0409 for English US)</param>
	/// <returns>Handle to the resource information block, or 0 if not found</returns>
	uint FindResource(uint lpType, uint lpName, ushort wLanguage = 0);

	/// <summary>
	/// Loads a resource into memory.
	/// </summary>
	/// <param name="hModule">Handle to the module containing the resource</param>
	/// <param name="hResInfo">Handle to the resource (from FindResource)</param>
	/// <returns>Handle to the loaded resource data, or 0 if failed</returns>
	uint LoadResource(uint hModule, uint hResInfo);

	/// <summary>
	/// Gets the size of a resource.
	/// </summary>
	/// <param name="hModule">Handle to the module containing the resource</param>
	/// <param name="hResInfo">Handle to the resource</param>
	/// <returns>Size of the resource in bytes, or 0 if failed</returns>
	uint SizeofResource(uint hModule, uint hResInfo);

	/// <summary>
	/// Locks a resource into memory.
	/// </summary>
	/// <param name="hResData">Handle to the resource data</param>
	/// <returns>Pointer to the resource data</returns>
	uint LockResource(uint hResData);

	/// <summary>
	/// Loads a string from the string table resource.
	/// </summary>
	/// <param name="stringId">The string resource ID</param>
	/// <returns>The string, or null if not found</returns>
	string? LoadString(uint stringId);

	/// <summary>
	/// Loads a bitmap resource by ID.
	/// </summary>
	/// <param name="bitmapId">The bitmap resource ID</param>
	/// <returns>The bitmap data, or null if not found</returns>
	byte[]? LoadBitmap(uint bitmapId);

	/// <summary>
	/// Loads a bitmap resource by name.
	/// </summary>
	/// <param name="bitmapName">The bitmap resource name</param>
	/// <returns>The bitmap data, or null if not found</returns>
	byte[]? LoadBitmapByName(string bitmapName);

	/// <summary>
	/// Enumerates all resource names for a specified type.
	/// </summary>
	/// <param name="lpType">Resource type (can be integer ID or string name)</param>
	/// <returns>List of resource IDs/names, or null if type not found</returns>
	IEnumerable<uint>? EnumerateResourceNames(uint lpType);
}
