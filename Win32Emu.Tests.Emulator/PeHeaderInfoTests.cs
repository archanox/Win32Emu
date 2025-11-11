using Win32Emu.Loader;
using Xunit;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for reading PE header information including sections, heap, and stack sizes
/// </summary>
public class PeHeaderInfoTests
{
	[Fact]
	public void PeSection_IsExecutable_DetectsCodeSections()
	{
		// Arrange - Create a section with executable flag
		var codeSection = new PeSection(".text", 0x1000, 0x2000, 0x2000, (PeSectionCharacteristics)0x20000020);
		
		// Act & Assert
		Assert.True(codeSection.IsExecutable);
	}
	
	[Fact]
	public void PeSection_IsData_DetectsDataSections()
	{
		// Arrange - Create a section with initialized data flag
		var dataSection = new PeSection(".data", 0x3000, 0x1000, 0x1000, (PeSectionCharacteristics)0x40000040);
		
		// Act & Assert
		Assert.True(dataSection.IsData);
	}
	
	[Fact]
	public void LoadedImage_CodeSections_ReturnsExecutableSections()
	{
		// Arrange
		var sections = new[]
		{
			new PeSection(".text", 0x1000, 0x2000, 0x2000, (PeSectionCharacteristics)0x20000020), // Executable
			new PeSection(".data", 0x3000, 0x1000, 0x1000, (PeSectionCharacteristics)0xC0000040), // Data, readable, writable
			new PeSection(".rdata", 0x4000, 0x500, 0x500, (PeSectionCharacteristics)0x40000040),  // Data, readable
		};
		
		var loadedImage = new LoadedImage(
			0x00400000,
			0x00401000,
			0x00010000,
			new Dictionary<uint, (string dll, string name)>(),
			"test.exe",
			new Dictionary<string, uint>(),
			new Dictionary<uint, uint>(),
			new Dictionary<string, string>(),
			new Dictionary<uint, string>(),
			3,
			0x1000,
			0x100000,
			0x10000,
			0x100000,
			0x10000,
			[],
			sections,
			new Dictionary<uint, uint>(), // IatEntryMap (empty)
			// FileHeader fields
			Machine: 0x014C, // Intel 386
			TimeDateStamp: 0x00000000,
			Characteristics: 0x010E, // EXECUTABLE_IMAGE | 32BIT_MACHINE | LINE_NUMS_STRIPPED | LOCAL_SYMS_STRIPPED
			// OptionalHeader additional fields
			MajorLinkerVersion: 14,
			MinorLinkerVersion: 0,
			MajorOperatingSystemVersion: 4,
			MinorOperatingSystemVersion: 0,
			MajorImageVersion: 0,
			MinorImageVersion: 0,
			MajorSubsystemVersion: 4,
			MinorSubsystemVersion: 0,
			DllCharacteristics: 0x0000,
			CheckSum: 0x00000000,
			SectionAlignment: 0x1000,
			FileAlignment: 0x0200,
			BaseOfCode: 0x1000,
			BaseOfData: 0x3000,
			SizeOfCode: 0x2000,
			SizeOfInitializedData: 0x1500,
			SizeOfUninitializedData: 0x0000
		);
		
		// Act
		var codeSections = loadedImage.CodeSections.ToList();
		
		// Assert
		Assert.Single(codeSections);
		Assert.Equal(".text", codeSections[0].Name);
	}
	
	[Fact]
	public void LoadedImage_IsAddressInCodeSection_DetectsCodeAddresses()
	{
		// Arrange
		var sections = new[]
		{
			new PeSection(".text", 0x1000, 0x2000, 0x2000, (PeSectionCharacteristics)0x20000020), // Executable at RVA 0x1000-0x3000
		};
		
		var loadedImage = new LoadedImage(
			0x00400000,
			0x00401000,
			0x00010000,
			new Dictionary<uint, (string dll, string name)>(),
			"test.exe",
			new Dictionary<string, uint>(),
			new Dictionary<uint, uint>(),
			new Dictionary<string, string>(),
			new Dictionary<uint, string>(),
			3,
			0x1000,
			0x100000,
			0x10000,
			0x100000,
			0x10000,
			[],
			sections,
			new Dictionary<uint, uint>(), // IatEntryMap (empty)
			// FileHeader fields
			Machine: 0x014C, // Intel 386
			TimeDateStamp: 0x00000000,
			Characteristics: 0x010E,
			// OptionalHeader additional fields
			MajorLinkerVersion: 14,
			MinorLinkerVersion: 0,
			MajorOperatingSystemVersion: 4,
			MinorOperatingSystemVersion: 0,
			MajorImageVersion: 0,
			MinorImageVersion: 0,
			MajorSubsystemVersion: 4,
			MinorSubsystemVersion: 0,
			DllCharacteristics: 0x0000,
			CheckSum: 0x00000000,
			SectionAlignment: 0x1000,
			FileAlignment: 0x0200,
			BaseOfCode: 0x1000,
			BaseOfData: 0x3000,
			SizeOfCode: 0x2000,
			SizeOfInitializedData: 0x1500,
			SizeOfUninitializedData: 0x0000
		);
		
		// Act & Assert
		Assert.True(loadedImage.IsAddressInCodeSection(0x00401000));  // VA in .text section
		Assert.True(loadedImage.IsAddressInCodeSection(0x00402FFF));  // Last byte of .text
		Assert.False(loadedImage.IsAddressInCodeSection(0x00403000)); // Beyond .text
		Assert.False(loadedImage.IsAddressInCodeSection(0x00400000)); // Before .text (in headers)
	}
	
	[Fact]
	public void LoadedImage_StoresHeapSizes_FromPeHeaders()
	{
		// Arrange & Act
		var loadedImage = new LoadedImage(
			0x00400000,
			0x00401000,
			0x00010000,
			new Dictionary<uint, (string dll, string name)>(),
			"test.exe",
			new Dictionary<string, uint>(),
			new Dictionary<uint, uint>(),
			new Dictionary<string, string>(),
			new Dictionary<uint, string>(),
			3,
			0x1000,
			0x200000,  // Stack reserve
			0x10000,   // Stack commit
			0x100000,  // Heap reserve
			0x8000,    // Heap commit
			[],
			[],
			new Dictionary<uint, uint>(), // IatEntryMap (empty)
			// FileHeader fields
			Machine: 0x014C,
			TimeDateStamp: 0x00000000,
			Characteristics: 0x010E,
			// OptionalHeader additional fields
			MajorLinkerVersion: 14,
			MinorLinkerVersion: 0,
			MajorOperatingSystemVersion: 4,
			MinorOperatingSystemVersion: 0,
			MajorImageVersion: 0,
			MinorImageVersion: 0,
			MajorSubsystemVersion: 4,
			MinorSubsystemVersion: 0,
			DllCharacteristics: 0x0000,
			CheckSum: 0x00000000,
			SectionAlignment: 0x1000,
			FileAlignment: 0x0200,
			BaseOfCode: 0x1000,
			BaseOfData: 0x3000,
			SizeOfCode: 0x2000,
			SizeOfInitializedData: 0x1500,
			SizeOfUninitializedData: 0x0000
		);
		
		// Assert
		Assert.Equal(0x200000u, loadedImage.SizeOfStackReserve);
		Assert.Equal(0x10000u, loadedImage.SizeOfStackCommit);
		Assert.Equal(0x100000u, loadedImage.SizeOfHeapReserve);
		Assert.Equal(0x8000u, loadedImage.SizeOfHeapCommit);
	}
}
