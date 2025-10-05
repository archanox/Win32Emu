using AsmResolver;
using AsmResolver.PE;
using AsmResolver.PE.Win32Resources;

namespace Win32Emu.Loader;

/// <summary>
/// Utility class for extracting icon resources from PE files
/// </summary>
public static class PeIconExtractor
{
	// RT_ICON and RT_GROUP_ICON resource type constants
	private const uint RT_ICON = 3;
	private const uint RT_GROUP_ICON = 14;

	/// <summary>
	/// Represents an ICONDIR header structure
	/// </summary>
	private struct IconDir
	{
		public ushort Reserved;   // Reserved (must be 0)
		public ushort Type;       // Resource type (1 for icons)
		public ushort Count;      // Number of images
	}

	/// <summary>
	/// Represents an ICONDIRENTRY structure
	/// </summary>
	private struct IconDirEntry
	{
		public byte Width;          // Width in pixels
		public byte Height;         // Height in pixels
		public byte ColorCount;     // Number of colors (0 if >= 8bpp)
		public byte Reserved;       // Reserved (must be 0)
		public ushort Planes;       // Color planes
		public ushort BitCount;     // Bits per pixel
		public uint BytesInRes;     // Size of image data
		public uint ImageOffset;    // Offset to image data in file
	}

	/// <summary>
	/// Represents an GRPICONDIRENTRY structure (resource version)
	/// </summary>
	private struct GrpIconDirEntry
	{
		public byte Width;
		public byte Height;
		public byte ColorCount;
		public byte Reserved;
		public ushort Planes;
		public ushort BitCount;
		public uint BytesInRes;
		public ushort Id;  // Resource ID instead of offset
	}

	/// <summary>
	/// Extracts the first icon from a PE file and saves it to the specified output path
	/// </summary>
	/// <param name="pePath">Path to the PE executable</param>
	/// <param name="outputPath">Path where the icon file should be saved</param>
	/// <returns>True if an icon was extracted successfully, false otherwise</returns>
	public static bool TryExtractIcon(string pePath, string outputPath)
	{
		try
		{
			if (!File.Exists(pePath))
			{
				return false;
			}

			var image = PEImage.FromFile(pePath);
			var resources = image.Resources;
			
			if (resources == null)
			{
				return false;
			}

			// Look for RT_GROUP_ICON (resource type 14)
			var iconGroupEntry = resources.Entries.FirstOrDefault(e => e.Id == RT_GROUP_ICON);
			if (iconGroupEntry is not ResourceDirectory iconGroupDir)
			{
				return false;
			}

			// Get the first icon group
			var firstIconGroup = iconGroupDir.Entries.FirstOrDefault();
			if (firstIconGroup is not ResourceDirectory firstIconGroupDir)
			{
				return false;
			}

			// Get the language-specific entry
			var iconGroupData = firstIconGroupDir.Entries.FirstOrDefault();
			if (iconGroupData is not ResourceData iconGroupDataEntry)
			{
				return false;
			}

			// Read the icon group data
			var grpData = iconGroupDataEntry.Contents.WriteIntoArray();
			if (grpData.Length < 6)
			{
				return false;
			}

			// Parse ICONDIR header
			var reserved = BitConverter.ToUInt16(grpData, 0);
			var type = BitConverter.ToUInt16(grpData, 2);
			var count = BitConverter.ToUInt16(grpData, 4);

			if (type != 1 || count == 0)
			{
				return false;
			}

			// Look for RT_ICON (resource type 3)
			var iconEntry = resources.Entries.FirstOrDefault(e => e.Id == RT_ICON);
			if (iconEntry is not ResourceDirectory iconDir)
			{
				return false;
			}

			// Collect icon entries and data
			var iconEntries = new List<(GrpIconDirEntry entry, byte[] data)>();
			var offset = 6; // After ICONDIR header

			for (int i = 0; i < count && offset + 14 <= grpData.Length; i++)
			{
				var grpEntry = new GrpIconDirEntry
				{
					Width = grpData[offset],
					Height = grpData[offset + 1],
					ColorCount = grpData[offset + 2],
					Reserved = grpData[offset + 3],
					Planes = BitConverter.ToUInt16(grpData, offset + 4),
					BitCount = BitConverter.ToUInt16(grpData, offset + 6),
					BytesInRes = BitConverter.ToUInt32(grpData, offset + 8),
					Id = BitConverter.ToUInt16(grpData, offset + 12)
				};

				// Find the corresponding icon data by ID
				var iconDataEntry = iconDir.Entries.FirstOrDefault(e => e.Id == grpEntry.Id);
				if (iconDataEntry is ResourceDirectory iconDataDir)
				{
					var iconData = iconDataDir.Entries.FirstOrDefault();
					if (iconData is ResourceData iconDataResource)
					{
						var imageData = iconDataResource.Contents.WriteIntoArray();
						iconEntries.Add((grpEntry, imageData));
					}
				}

				offset += 14; // Size of GRPICONDIRENTRY
			}

			if (iconEntries.Count == 0)
			{
				return false;
			}

			// Write .ico file
			using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
			using var writer = new BinaryWriter(stream);

			// Write ICONDIR header
			writer.Write((ushort)0);  // Reserved
			writer.Write((ushort)1);  // Type (1 = icon)
			writer.Write((ushort)iconEntries.Count);  // Count

			// Calculate and write icon directory entries
			uint currentOffset = (uint)(6 + iconEntries.Count * 16); // Header + all entries
			foreach (var (entry, data) in iconEntries)
			{
				writer.Write(entry.Width);
				writer.Write(entry.Height);
				writer.Write(entry.ColorCount);
				writer.Write((byte)0); // Reserved
				writer.Write(entry.Planes);
				writer.Write(entry.BitCount);
				writer.Write((uint)data.Length); // Size
				writer.Write(currentOffset); // Offset
				currentOffset += (uint)data.Length;
			}

			// Write icon image data
			foreach (var (_, data) in iconEntries)
			{
				writer.Write(data);
			}

			return true;
		}
		catch
		{
			// If extraction fails for any reason, return false
			try
			{
				if (File.Exists(outputPath))
				{
					File.Delete(outputPath);
				}
			}
			catch
			{
				// Ignore cleanup errors
			}
			return false;
		}
	}

	/// <summary>
	/// Extracts the first icon from a PE file to a temporary file and returns its path
	/// </summary>
	/// <param name="pePath">Path to the PE executable</param>
	/// <returns>Path to the extracted icon file, or null if extraction failed</returns>
	public static string? ExtractIconToTemp(string pePath)
	{
		try
		{
			var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.ico");
			if (TryExtractIcon(pePath, tempPath))
			{
				return tempPath;
			}
			return null;
		}
		catch
		{
			return null;
		}
	}
}
