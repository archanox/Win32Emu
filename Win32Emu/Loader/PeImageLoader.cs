using AsmResolver;
using AsmResolver.PE;
using AsmResolver.PE.File;
using Win32Emu.Memory;

namespace Win32Emu.Loader;

/// <summary>
/// PE loader using a single PEImage load. Maps section raw data and replaces IAT entries with synthetic
/// addresses for interception via an import map. Relocations not yet handled.
/// </summary>
public class PeImageLoader(VirtualMemory vm)
{
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
		var (exportsByName, exportsByOrdinal) = BuildExportMaps(image, imageBase);
		return new LoadedImage(imageBase, entryPoint, imageSize, importMap, path, exportsByName, exportsByOrdinal);
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

	private (Dictionary<string, uint> byName, Dictionary<uint, uint> byOrdinal) BuildExportMaps(PEImage image, uint imageBase)
	{
		var byName = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
		var byOrdinal = new Dictionary<uint, uint>();

		if (image.Exports == null)
		{
			return (byName, byOrdinal);
		}

		foreach (var export in image.Exports.Entries)
		{
			// Skip forwarded exports (they have no RVA)
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

		return (byName, byOrdinal);
	}
}