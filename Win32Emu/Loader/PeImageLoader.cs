using AsmResolver;
using AsmResolver.PE;
using AsmResolver.PE.Exports;
using AsmResolver.PE.File;
using Microsoft.Extensions.Logging;
using Win32Emu.Memory;

namespace Win32Emu.Loader;

/// <summary>
/// PE loader using a single PEImage load. Maps section raw data and replaces IAT entries with synthetic
/// addresses for interception via an import map. Relocations not yet handled.
/// </summary>
public class PeImageLoader(VirtualMemory vm, ILogger? logger = null)
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
		var subsystem = (ushort)opt.SubSystem;

		// Map sections (raw contents only; uninitialized data left zeroed).
		foreach (var section in pe.Sections)
		{
			if (section.Contents is null)
			{
				continue;
			}

			try
			{
				vm.WriteBytes(imageBase + section.Rva, section.Contents.WriteIntoArray());
			}
			catch (Exception ex) when (ex is System.IO.EndOfStreamException or ArgumentException)
			{
				// Skip corrupted sections that extend beyond file boundaries
				// This can happen with malformed PE files where section headers indicate
				// sizes that don't match actual file data
				logger?.LogWarning("Skipping corrupted section at RVA {SectionRva:X8}: {ErrorMessage}", section.Rva, ex.Message);
			}
		}

		var importMap = BuildImportMap(image, imageBase);
		var (exportsByName, exportsByOrdinal, forwardedByName, forwardedByOrdinal) = BuildExportMaps(image, imageBase);
		return new LoadedImage(imageBase, entryPoint, imageSize, importMap, path, exportsByName, exportsByOrdinal, forwardedByName, forwardedByOrdinal, (ushort)subsystem);
	}

	// Syscall dispatcher address - this is where all import stubs will call into
	private const uint SYSCALL_DISPATCHER_ADDRESS = 0x0E000000;
	
	private Dictionary<uint, (string dll, string name)> BuildImportMap(PEImage image, uint imageBase)
	{
		var map = new Dictionary<uint, (string dll, string name)>();
		var imports = image.Imports; // IEnumerable<ImportModule>
		var synth = 0;
		
		// First, create the syscall dispatcher stub at a fixed address
		// This stub will be hit by all import calls and will trigger our syscall handler
		// Format: INT 0x80 (syscall); RET (RET executes after the syscall handler returns control to the CPU/emulator)
		var syscallStub = new byte[]
		{
			0xCD, 0x80, // INT 0x80 - triggers syscall
			0xC3        // RET - return (won't execute in normal flow)
		};
		vm.WriteBytes(SYSCALL_DISPATCHER_ADDRESS, syscallStub);
		logger?.LogInformation("[Loader] Created syscall dispatcher at 0x{Address:X8}", SYSCALL_DISPATCHER_ADDRESS);
		
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
				
				// Create import stub using retrowin32-style approach:
				// CALL [syscall_dispatcher]; RET N
				// This allows the CPU to naturally execute and return without manual EIP manipulation
				//
				// Format:
				// - CALL to syscall dispatcher (5 bytes: E8 + rel32 offset)
				// - RET (1 byte: C3) - CPU will execute this to return naturally after syscall
				// 
				// Calculate relative offset from stub address to syscall dispatcher
				var stubAddr = synthetic;
				var callOffset = (int)(SYSCALL_DISPATCHER_ADDRESS - (stubAddr + 5)); // +5 for size of CALL instruction
				
				var stub = new byte[]
				{
					0xE8, // CALL rel32
					(byte)(callOffset & 0xFF),
					(byte)((callOffset >> 8) & 0xFF),
					(byte)((callOffset >> 16) & 0xFF),
					(byte)((callOffset >> 24) & 0xFF),
					0xC3, // RET - CPU will execute this to return naturally
					// Padding to maintain 16-byte alignment
					0x90, 0x90, 0x90, 0x90,
					0x90, 0x90, 0x90, 0x90, 0x90, 0x90
				};
				vm.WriteBytes(synthetic, stub);
				
				var name = sym.Name ?? ($"Ordinal_{sym.Hint}");
				map[synthetic] = (dll.ToUpperInvariant(), name);
				logger?.LogTrace("[Loader] Mapped import #{Index}: {Dll}!{Name} at 0x{Synthetic:X8} -> syscall at 0x{Syscall:X8}", 
					synth - 1, dll.ToUpperInvariant(), name, synthetic, SYSCALL_DISPATCHER_ADDRESS);
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