using AsmResolver;
using AsmResolver.PE;
using AsmResolver.PE.Exports;
using AsmResolver.PE.File;
using Win32Emu.Memory;

namespace Win32Emu.Loader;

/// <summary>
/// PE loader using a single PEImage load. Maps section raw data and replaces IAT entries with synthetic
/// addresses for interception via an import map. Relocations not yet handled.
/// </summary>
public class PeImageLoader(VirtualMemory vm)
{
	/// <summary>
	/// Validates if a file is a valid PE32 executable by parsing the PE structure (and may map sections into memory).
	/// </summary>
	/// <param name="path">Path to the executable file</param>
	/// <returns>True if the file is a valid PE32 executable, false otherwise</returns>
	public static bool IsPE32(string path)
	{
		try
		{
			var image = PEImage.FromFile(path);
			var pe = image.PEFile;
			if (pe == null)
			{
				return false;
			}

			var opt = pe.OptionalHeader;
			if (opt == null)
			{
				return false;
			}

			return opt.Magic == OptionalHeaderMagic.PE32;
		}
		catch
		{
			return false;
		}
	}

	public LoadedImage Load(string path)
	{
		var image = PEImage.FromFile(path);
		var pe = image.PEFile ?? throw new InvalidOperationException("PEImage missing PEFile.");
		var opt = pe.OptionalHeader ?? throw new InvalidOperationException("Missing optional header.");

		if(opt.Magic != OptionalHeaderMagic.PE32)
		{
			throw new NotSupportedException("Only PE32 format is supported.");
		}

		var imageBase = (uint)opt.ImageBase;
		var entryPoint = imageBase + opt.AddressOfEntryPoint;
		var imageSize = opt.SizeOfImage;

		// Map sections (raw contents only; uninitialized data left zeroed).
		foreach (var section in pe.Sections)
		{
			if (section.Contents is null)
			{
				continue;
			}

			vm.WriteBytes(imageBase + section.Rva, section.Contents.WriteIntoArray());
		}

		var importMap = BuildImportMap(image, imageBase);
		var (exportsByName, exportsByOrdinal, forwardedByName, forwardedByOrdinal) = BuildExportMaps(image, imageBase);
		return new LoadedImage(imageBase, entryPoint, imageSize, importMap, path, exportsByName, exportsByOrdinal, forwardedByName, forwardedByOrdinal);
	}

	private Dictionary<uint, (string dll, string name)> BuildImportMap(PEImage image, uint imageBase)
	{
		var map = new Dictionary<uint, (string dll, string name)>();
		var imports = image.Imports; // IEnumerable<ImportModule>
		var synth = 0;
		foreach (var module in imports)
		{
			var dll = module.Name ?? string.Empty;
			foreach (var sym in module.Symbols)
			{
				// Prefer IAT entry RVA when available.
				var rva = sym.AddressTableEntry?.Rva; // fallback
				if (rva is null or 0)
				{
					continue;
				}

				var va = imageBase + rva.Value;
				var synthetic = 0x0F000000u + (uint)(synth++ * 0x10u);
				
				// Write the synthetic address to the IAT entry
				vm.Write32(va, synthetic);
				
				// Create an executable stub at the synthetic address
				// This stub will be a simple INT3 (breakpoint) that we can intercept
				// INT3 = 0xCC, followed by padding
				var stub = new byte[] 
				{ 
					0xCC, // INT3 - breakpoint instruction
					0x90, 0x90, 0x90, // NOP padding
					0x90, 0x90, 0x90, 0x90,
					0x90, 0x90, 0x90, 0x90,
					0x90, 0x90, 0x90, 0x90
				};
				vm.WriteBytes(synthetic, stub);
				
				var name = sym.Name ?? ($"Ordinal_{sym.Hint}");
				map[synthetic] = (dll.ToUpperInvariant(), name);
			}
		}

		return map;
	}

	private (Dictionary<string, uint> byName, Dictionary<uint, uint> byOrdinal, Dictionary<string, string> forwardedByName, Dictionary<uint, string> forwardedByOrdinal) BuildExportMaps(PEImage image, uint imageBase)
	{
		var byName = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
		var byOrdinal = new Dictionary<uint, uint>();
		var forwardedByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		var forwardedByOrdinal = new Dictionary<uint, string>();

		if (image.Exports == null)
		{
			return (byName, byOrdinal, forwardedByName, forwardedByOrdinal);
		}

		foreach (var export in image.Exports.Entries)
		{
			// Check if this is a forwarded export
			if (export.IsForwarder)
			{
				// Store forwarded export information
				forwardedByOrdinal[export.Ordinal] = export.ForwarderName;
				
				if (!string.IsNullOrEmpty(export.Name))
				{
					forwardedByName[export.Name] = export.ForwarderName;
				}
				continue;
			}

			// Skip exports with no RVA (shouldn't happen for non-forwarded exports)
			if (export.Address == null || !export.Address.IsBounded)
			{
				continue;
			}

			var rva = export.Address.Rva;
			var va = imageBase + rva;

			// Add by ordinal
			byOrdinal[export.Ordinal] = va;

			// Add by name if it has one
			if (!string.IsNullOrEmpty(export.Name))
			{
				byName[export.Name] = va;
			}
		}

		return (byName, byOrdinal, forwardedByName, forwardedByOrdinal);
	}
}