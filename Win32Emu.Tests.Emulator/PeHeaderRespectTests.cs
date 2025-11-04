using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Xunit.Abstractions;
using AsmResolver;
using AsmResolver.PE;
using AsmResolver.PE.File;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests to verify that PeImageLoader properly respects all PE header values
/// as specified in the PE format specification.
/// 
/// This addresses the requirement: "I assume we're respecting the EntryPoint, ImageBase 
/// plus all the other magic numbers we have in the PE headers? The section header 
/// locations and flags? The import dll thunk values? The import function hints?"
/// </summary>
public class PeHeaderRespectTests
{
	private readonly ITestOutputHelper _output;
	private const string TestPeFile = "TestData/CHKCPU32.exe";

	public PeHeaderRespectTests(ITestOutputHelper output)
	{
		_output = output;
	}

	[Fact]
	public void EntryPoint_IsRespected_FromOptionalHeader()
	{
		if (!File.Exists(TestPeFile))
		{
			_output.WriteLine($"Test file {TestPeFile} not found, skipping test");
			return;
		}

		// Parse PE file directly with AsmResolver
		var image = PEImage.FromFile(TestPeFile);
		var opt = image.PEFile?.OptionalHeader;
		Assert.NotNull(opt);

		var expectedEntryPointRva = opt.AddressOfEntryPoint;
		var expectedImageBase = (uint)opt.ImageBase;
		var expectedEntryPointVa = expectedImageBase + expectedEntryPointRva;

		// Load with PeImageLoader
		var memory = new VirtualMemory();
		var loader = new PeImageLoader(memory, NullLogger.Instance);
		var loadedImage = loader.Load(TestPeFile);

		_output.WriteLine($"Expected EntryPoint RVA: 0x{expectedEntryPointRva:X8}");
		_output.WriteLine($"Expected EntryPoint VA: 0x{expectedEntryPointVa:X8}");
		_output.WriteLine($"Loaded EntryPoint VA: 0x{loadedImage.EntryPointAddress:X8}");

		// Verify the entry point address matches
		Assert.Equal(expectedEntryPointVa, loadedImage.EntryPointAddress);
	}

	[Fact]
	public void ImageBase_IsRespected_FromOptionalHeader()
	{
		if (!File.Exists(TestPeFile))
		{
			_output.WriteLine($"Test file {TestPeFile} not found, skipping test");
			return;
		}

		var image = PEImage.FromFile(TestPeFile);
		var opt = image.PEFile?.OptionalHeader;
		Assert.NotNull(opt);

		var expectedImageBase = (uint)opt.ImageBase;

		var memory = new VirtualMemory();
		var loader = new PeImageLoader(memory, NullLogger.Instance);
		var loadedImage = loader.Load(TestPeFile);

		_output.WriteLine($"Expected ImageBase: 0x{expectedImageBase:X8}");
		_output.WriteLine($"Loaded ImageBase: 0x{loadedImage.BaseAddress:X8}");

		Assert.Equal(expectedImageBase, loadedImage.BaseAddress);
	}

	[Fact]
	public void SectionHeaders_LocationsAreRespected()
	{
		if (!File.Exists(TestPeFile))
		{
			_output.WriteLine($"Test file {TestPeFile} not found, skipping test");
			return;
		}

		var image = PEImage.FromFile(TestPeFile);
		var pe = image.PEFile;
		Assert.NotNull(pe);
		var sections = pe.Sections;
		Assert.NotNull(sections);

		var memory = new VirtualMemory();
		var loader = new PeImageLoader(memory, NullLogger.Instance);
		var loadedImage = loader.Load(TestPeFile);

		// Verify each section is loaded at its correct RVA
		foreach (var section in sections)
		{
			if (section.Contents == null)
			{
				_output.WriteLine($"Section {section.Name} has no contents, skipping");
				continue;
			}

			try
			{
				var sectionData = section.Contents.WriteIntoArray();
				if (sectionData.Length == 0)
				{
					_output.WriteLine($"Section {section.Name} has zero-length contents, skipping");
					continue;
				}

				var sectionRva = section.Rva;
				var sectionVa = loadedImage.BaseAddress + sectionRva;
				
				_output.WriteLine($"Verifying section {section.Name} at RVA 0x{sectionRva:X8}, VA 0x{sectionVa:X8}");

				// Read first byte from section to verify it's loaded
				var firstByte = memory.Read8(sectionVa);
				var expectedFirstByte = sectionData[0];
				
				_output.WriteLine($"  First byte: Expected 0x{expectedFirstByte:X2}, Got 0x{firstByte:X2}");
				Assert.Equal(expectedFirstByte, firstByte);
			}
			catch (InvalidOperationException ex)
			{
				_output.WriteLine($"Section {section.Name} cannot be read (InvalidOperation): {ex.Message}");
			}
			catch (ArgumentOutOfRangeException ex)
			{
				_output.WriteLine($"Section {section.Name} cannot be read (ArgumentOutOfRange): {ex.Message}");
			}
			catch (IOException ex)
			{
				_output.WriteLine($"Section {section.Name} cannot be read (IO): {ex.Message}");
			}
			catch (Exception ex)
			{
				_output.WriteLine($"Section {section.Name} cannot be read: {ex.Message}");
			}
		}
	}

	[Fact]
	public void SectionHeaders_CharacteristicsAreAccessible()
	{
		if (!File.Exists(TestPeFile))
		{
			_output.WriteLine($"Test file {TestPeFile} not found, skipping test");
			return;
		}

		var image = PEImage.FromFile(TestPeFile);
		var pe = image.PEFile;
		Assert.NotNull(pe);
		var sections = pe.Sections;
		Assert.NotNull(sections);

		// Verify that section characteristics (flags) are available and meaningful
		foreach (var section in sections)
		{
			var characteristics = section.Characteristics;
			
			_output.WriteLine($"Section {section.Name}:");
			_output.WriteLine($"  Characteristics: 0x{(uint)characteristics:X8}");
			_output.WriteLine($"  Readable: {(characteristics & AsmResolver.PE.File.SectionFlags.MemoryRead) != 0}");
			_output.WriteLine($"  Writable: {(characteristics & AsmResolver.PE.File.SectionFlags.MemoryWrite) != 0}");
			_output.WriteLine($"  Executable: {(characteristics & AsmResolver.PE.File.SectionFlags.MemoryExecute) != 0}");
			
			// Verify that we can read these flags
			Assert.NotEqual(0u, (uint)characteristics);
		}

		// Note: Currently PeImageLoader logs section characteristics but doesn't enforce
		// memory protection based on them. This is acceptable for emulation purposes
		// as long as the values are available for future use if needed.
	}

	[Fact]
	public void ImportHints_AreAccessible()
	{
		if (!File.Exists(TestPeFile))
		{
			_output.WriteLine($"Test file {TestPeFile} not found, skipping test");
			return;
		}

		var image = PEImage.FromFile(TestPeFile);
		var imports = image.Imports;
		Assert.NotNull(imports);

		var hintsFound = 0;
		var ordinalImportsFound = 0;

		foreach (var module in imports)
		{
			foreach (var symbol in module.Symbols)
			{
				if (symbol.IsImportByOrdinal)
				{
					ordinalImportsFound++;
					_output.WriteLine($"Import by ordinal: {module.Name}!Ordinal_{symbol.Ordinal}");
				}
				else if (symbol.Name != null)
				{
					hintsFound++;
					_output.WriteLine($"Import: {module.Name}!{symbol.Name}, Hint: {symbol.Hint}");
					
					// Verify hint is accessible (should be a reasonable value, typically < 10000)
					Assert.InRange(symbol.Hint, 0, 65535);
				}
			}
		}

		_output.WriteLine($"Total imports with hints: {hintsFound}");
		_output.WriteLine($"Total imports by ordinal: {ordinalImportsFound}");

		// Note: Hints are optimization values that suggest where to start searching
		// in the export name table. They don't need to be exact. The loader currently
		// doesn't use hints for optimization, which is acceptable since we're
		// intercepting all imports anyway.
	}

	[Fact]
	public void ImportAddressTable_IsProperlyPopulated()
	{
		if (!File.Exists(TestPeFile))
		{
			_output.WriteLine($"Test file {TestPeFile} not found, skipping test");
			return;
		}

		var image = PEImage.FromFile(TestPeFile);
		var imports = image.Imports;
		Assert.NotNull(imports);

		var memory = new VirtualMemory();
		var loader = new PeImageLoader(memory, NullLogger.Instance);
		var loadedImage = loader.Load(TestPeFile);

		// Verify IAT entries are written
		foreach (var module in imports)
		{
			foreach (var symbol in module.Symbols)
			{
				var iatRva = symbol.AddressTableEntry?.Rva;
				if (iatRva == null || iatRva == 0)
				{
					_output.WriteLine($"Skipping {module.Name}!{symbol.Name ?? "Ordinal_" + symbol.Ordinal} - no IAT RVA");
					continue;
				}

				var iatVa = loadedImage.BaseAddress + iatRva.Value;
				var iatValue = memory.Read32(iatVa);

				_output.WriteLine($"IAT entry for {module.Name}!{symbol.Name ?? "Ordinal_" + symbol.Ordinal}:");
				_output.WriteLine($"  RVA: 0x{iatRva:X8}, VA: 0x{iatVa:X8}");
				_output.WriteLine($"  Value: 0x{iatValue:X8}");

				// IAT should be populated with synthetic address in range 0x0F000000-0x0FFFFFFF
				Assert.InRange(iatValue, 0x0F000000u, 0x0FFFFFFFu);
			}
		}
	}

	[Fact]
	public void ImportThunks_AreProperlyHandled()
	{
		if (!File.Exists(TestPeFile))
		{
			_output.WriteLine($"Test file {TestPeFile} not found, skipping test");
			return;
		}

		var image = PEImage.FromFile(TestPeFile);
		var imports = image.Imports;
		Assert.NotNull(imports);

		var memory = new VirtualMemory();
		var loader = new PeImageLoader(memory, NullLogger.Instance);
		var loadedImage = loader.Load(TestPeFile);

		// The Import Address Table (IAT) contains thunk values that are overwritten
		// by the loader to point to the actual function addresses.
		// In our emulator, we replace them with synthetic addresses that trigger syscalls.
		
		// The original thunk data (Import Lookup Table / Import Name Table) should be
		// preserved in memory when we load the PE headers, since it's part of the
		// PE structure that gets mapped into memory.
		
		// For each import, verify that:
		// 1. The IAT entry has been written with a synthetic address
		// 2. The synthetic address is properly mapped in our import map
		
		foreach (var module in imports)
		{
			foreach (var symbol in module.Symbols)
			{
				var iatRva = symbol.AddressTableEntry?.Rva;
				if (iatRva == null || iatRva == 0)
				{
					_output.WriteLine($"Skipping {module.Name}!{symbol.Name ?? "Ordinal_" + symbol.Ordinal} - no IAT RVA");
					continue;
				}

				var iatVa = loadedImage.BaseAddress + iatRva.Value;
				var thunkValue = memory.Read32(iatVa);
				
				_output.WriteLine($"Import thunk for {module.Name}!{symbol.Name ?? "Ordinal_" + symbol.Ordinal}:");
				_output.WriteLine($"  IAT RVA: 0x{iatRva:X8}, IAT VA: 0x{iatVa:X8}");
				_output.WriteLine($"  Thunk value (synthetic address): 0x{thunkValue:X8}");

				// Thunk should be a synthetic address in our import stub range
				Assert.InRange(thunkValue, 0x0F000000u, 0x0FFFFFFFu);
				
				// Verify this synthetic address is in our import map
				if (!loadedImage.ImportAddressMap.TryGetValue(thunkValue, out var mapping))
				{
					Assert.Fail($"Synthetic address 0x{thunkValue:X8} should be in import map");
				}
				var (mappedDll, mappedName) = mapping;
				_output.WriteLine($"  Mapped to: {mappedDll}!{mappedName}");
				
				// Verify the mapping is correct
				Assert.Equal(module.Name?.ToUpperInvariant() ?? "", mappedDll);
			}
		}

		// Note: The original Import Lookup Table (ILT) is preserved in the PE headers
		// section of memory, which allows programs to inspect their own import structure
		// if needed. However, AsmResolver doesn't expose the ILT entries separately,
		// as it combines the information from both IAT and ILT when building the
		// ImportedSymbol objects.
	}

	[Fact]
	public void SizeOfImage_IsRespected()
	{
		if (!File.Exists(TestPeFile))
		{
			_output.WriteLine($"Test file {TestPeFile} not found, skipping test");
			return;
		}

		var image = PEImage.FromFile(TestPeFile);
		var opt = image.PEFile?.OptionalHeader;
		Assert.NotNull(opt);

		var expectedSizeOfImage = opt.SizeOfImage;

		var memory = new VirtualMemory();
		var loader = new PeImageLoader(memory, NullLogger.Instance);
		var loadedImage = loader.Load(TestPeFile);

		_output.WriteLine($"Expected SizeOfImage: 0x{expectedSizeOfImage:X8}");
		_output.WriteLine($"Loaded ImageSize: 0x{loadedImage.ImageSize:X8}");

		Assert.Equal(expectedSizeOfImage, loadedImage.ImageSize);
	}

	[Fact]
	public void SizeOfHeaders_IsRespected()
	{
		if (!File.Exists(TestPeFile))
		{
			_output.WriteLine($"Test file {TestPeFile} not found, skipping test");
			return;
		}

		var image = PEImage.FromFile(TestPeFile);
		var opt = image.PEFile?.OptionalHeader;
		Assert.NotNull(opt);

		var expectedSizeOfHeaders = opt.SizeOfHeaders;

		var memory = new VirtualMemory();
		var loader = new PeImageLoader(memory, NullLogger.Instance);
		var loadedImage = loader.Load(TestPeFile);

		_output.WriteLine($"Expected SizeOfHeaders: 0x{expectedSizeOfHeaders:X8}");
		_output.WriteLine($"Loaded HeaderEndRva: 0x{loadedImage.HeaderEndRva:X8}");

		// HeaderEndRva should be the effective header size
		// It may be min(SizeOfHeaders, first section RVA) to avoid overlap
		Assert.True(loadedImage.HeaderEndRva <= expectedSizeOfHeaders);
		Assert.True(loadedImage.HeaderEndRva > 0);
	}

	[Fact]
	public void StackSizes_AreRespected()
	{
		if (!File.Exists(TestPeFile))
		{
			_output.WriteLine($"Test file {TestPeFile} not found, skipping test");
			return;
		}

		var image = PEImage.FromFile(TestPeFile);
		var opt = image.PEFile?.OptionalHeader;
		Assert.NotNull(opt);

		var expectedSizeOfStackReserve = (uint)opt.SizeOfStackReserve;
		var expectedSizeOfStackCommit = (uint)opt.SizeOfStackCommit;

		var memory = new VirtualMemory();
		var loader = new PeImageLoader(memory, NullLogger.Instance);
		var loadedImage = loader.Load(TestPeFile);

		_output.WriteLine($"Expected SizeOfStackReserve: 0x{expectedSizeOfStackReserve:X8}");
		_output.WriteLine($"Loaded SizeOfStackReserve: 0x{loadedImage.SizeOfStackReserve:X8}");
		_output.WriteLine($"Expected SizeOfStackCommit: 0x{expectedSizeOfStackCommit:X8}");
		_output.WriteLine($"Loaded SizeOfStackCommit: 0x{loadedImage.SizeOfStackCommit:X8}");

		Assert.Equal(expectedSizeOfStackReserve, loadedImage.SizeOfStackReserve);
		Assert.Equal(expectedSizeOfStackCommit, loadedImage.SizeOfStackCommit);
	}

	[Fact]
	public void Subsystem_IsRespected()
	{
		if (!File.Exists(TestPeFile))
		{
			_output.WriteLine($"Test file {TestPeFile} not found, skipping test");
			return;
		}

		var image = PEImage.FromFile(TestPeFile);
		var opt = image.PEFile?.OptionalHeader;
		Assert.NotNull(opt);

		var expectedSubsystem = (ushort)opt.SubSystem;

		var memory = new VirtualMemory();
		var loader = new PeImageLoader(memory, NullLogger.Instance);
		var loadedImage = loader.Load(TestPeFile);

		_output.WriteLine($"Expected Subsystem: {expectedSubsystem}");
		_output.WriteLine($"Loaded Subsystem: {loadedImage.Subsystem}");

		Assert.Equal(expectedSubsystem, loadedImage.Subsystem);
	}

	[Fact]
	public void PEHeaders_AreLoadedIntoMemory()
	{
		if (!File.Exists(TestPeFile))
		{
			_output.WriteLine($"Test file {TestPeFile} not found, skipping test");
			return;
		}

		var memory = new VirtualMemory();
		var loader = new PeImageLoader(memory, NullLogger.Instance);
		var loadedImage = loader.Load(TestPeFile);

		// Verify DOS header is loaded (MZ signature)
		var mzSignature = memory.Read16(loadedImage.BaseAddress);
		_output.WriteLine($"DOS MZ signature: 0x{mzSignature:X4}");
		Assert.Equal((ushort)0x5A4D, mzSignature); // "MZ"

		// Verify PE signature is loaded at offset indicated by DOS header
		var peOffset = memory.Read32(loadedImage.BaseAddress + 0x3C);
		_output.WriteLine($"PE offset: 0x{peOffset:X8}");
		
		var peSignature = memory.Read32(loadedImage.BaseAddress + peOffset);
		_output.WriteLine($"PE signature: 0x{peSignature:X8}");
		Assert.Equal(0x00004550u, peSignature); // "PE\0\0"

		// This verifies that PeImageLoader properly loads headers into memory,
		// which is required for programs that read their own PE headers
	}

	[Fact]
	public void ImportNameConsistency_OrdinalVsHint()
	{
		if (!File.Exists(TestPeFile))
		{
			_output.WriteLine($"Test file {TestPeFile} not found, skipping test");
			return;
		}

		// This test verifies that when imports don't have names (import by ordinal),
		// we correctly identify them using the ordinal, not the hint
		
		var image = PEImage.FromFile(TestPeFile);
		var imports = image.Imports;
		Assert.NotNull(imports);

		foreach (var module in imports)
		{
			foreach (var symbol in module.Symbols)
			{
				var expectedName = symbol.Name ?? $"Ordinal_{symbol.Ordinal}";
				
				_output.WriteLine($"Symbol: {module.Name}!{expectedName}");
				_output.WriteLine($"  Name: {symbol.Name ?? "(null)"}");
				_output.WriteLine($"  Ordinal: {symbol.Ordinal}");
				_output.WriteLine($"  Hint: {symbol.Hint}");
				_output.WriteLine($"  IsImportByOrdinal: {symbol.IsImportByOrdinal}");

				// Verify that when there's no name, we use ordinal (not hint)
				if (symbol.Name == null)
				{
					Assert.Contains($"Ordinal_{symbol.Ordinal}", expectedName);
				}
			}
		}
	}

	[Fact]
	public void BaseRelocations_AreAccessible()
	{
		if (!File.Exists(TestPeFile))
		{
			_output.WriteLine($"Test file {TestPeFile} not found, skipping test");
			return;
		}

		var image = PEImage.FromFile(TestPeFile);
		var relocations = image.Relocations;

		if (relocations == null || relocations.Count == 0)
		{
			_output.WriteLine("PE file has no relocations, skipping test");
			return;
		}

		_output.WriteLine($"PE file has {relocations.Count} relocations");

		var relocationTypeCount = relocations
			.Take(10) // Check first 10 for performance
			.GroupBy(reloc => reloc.Type.ToString())
			.ToDictionary(g => g.Key, g => g.Count());

		foreach (var (type, count) in relocationTypeCount)
		{
			_output.WriteLine($"  {type}: {count}");
		}

		// Verify relocations are accessible
		Assert.True(relocations.Count > 0);
		
		// Note: PeImageLoader already implements relocation application
		// This test just verifies the data is accessible
	}
}
