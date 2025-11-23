namespace Win32Emu.Loader;

public record LoadedImage(
	uint BaseAddress,
	uint EntryPointAddress,
	uint ImageSize,
	Dictionary<uint, (string dll, string name)> ImportAddressMap,
	string FilePath,
	Dictionary<string, uint> ExportsByName,
	Dictionary<uint, uint> ExportsByOrdinal,
	Dictionary<string, string> ForwardedExportsByName,
	Dictionary<uint, string> ForwardedExportsByOrdinal,
	ushort Subsystem,            // PE subsystem type (IMAGE_SUBSYSTEM_WINDOWS_CUI = 3, IMAGE_SUBSYSTEM_WINDOWS_GUI = 2)
	uint HeaderEndRva,           // End of headers (RVA) based on PE: min(SizeOfHeaders, first section RVA)
	uint SizeOfStackReserve,     // From OptionalHeader
	uint SizeOfStackCommit,      // From OptionalHeader
	uint SizeOfHeapReserve,      // From OptionalHeader
	uint SizeOfHeapCommit,       // From OptionalHeader
	uint[] TlsCallbacks,         // TLS callback function addresses (VA)
	PeSection[] Sections,        // PE sections with characteristics (for identifying code/data regions)
	Dictionary<uint, uint> IatEntryMap,  // IAT VA -> expected synthetic address mapping for runtime verification
	Dictionary<string, ExportMetadata> ExportMetadata,  // Export function metadata (calling convention, arg bytes)
	// FileHeader fields
	ushort Machine,              // Architecture type (IMAGE_FILE_MACHINE_I386 = 0x014C)
	uint TimeDateStamp,          // File creation timestamp (seconds since 1970-01-01 UTC)
	ushort Characteristics,      // File characteristics flags (IMAGE_FILE_*)
	// OptionalHeader additional fields
	byte MajorLinkerVersion,     // Linker major version
	byte MinorLinkerVersion,     // Linker minor version
	ushort MajorOperatingSystemVersion,  // Required OS major version
	ushort MinorOperatingSystemVersion,  // Required OS minor version
	ushort MajorImageVersion,    // Image major version
	ushort MinorImageVersion,    // Image minor version
	ushort MajorSubsystemVersion,  // Required subsystem major version
	ushort MinorSubsystemVersion,  // Required subsystem minor version
	ushort DllCharacteristics,   // DLL characteristics flags (IMAGE_DLLCHARACTERISTICS_*)
	uint CheckSum,               // PE checksum (important for drivers and system DLLs)
	uint SectionAlignment,       // Section alignment in memory (bytes)
	uint FileAlignment,          // Section alignment in file (bytes)
	uint BaseOfCode,             // RVA of code section start
	uint BaseOfData,             // RVA of data section start (PE32 only, 0 for PE32+)
	uint SizeOfCode,             // Size of code section(s) in bytes
	uint SizeOfInitializedData,  // Size of initialized data section(s) in bytes
	uint SizeOfUninitializedData // Size of uninitialized data section(s) in bytes
)
{
	/// <summary>
	/// Gets all sections that contain executable code.
	/// </summary>
	public IEnumerable<PeSection> CodeSections => Sections.Where(s => s.IsExecutable);

	/// <summary>
	/// Gets all sections that contain data (initialized or writable).
	/// Note: This may include executable sections that are also writable (e.g., self-modifying code).
	/// Typical data sections have IMAGE_SCN_CNT_INITIALIZED_DATA or IMAGE_SCN_MEM_WRITE flags.
	/// </summary>
	public IEnumerable<PeSection> DataSections => Sections.Where(s => s.IsData || s.IsWritable);

	/// <summary>
	/// Checks if the given virtual address is within an executable section.
	/// </summary>
	public bool IsAddressInCodeSection(uint virtualAddress)
	{
		var rva = virtualAddress - BaseAddress;
		return CodeSections.Any(s => rva >= s.VirtualAddress && rva < s.VirtualAddress + s.VirtualSize);
	}

	/// <summary>
	/// Checks if the given virtual address is within a data section.
	/// </summary>
	public bool IsAddressInDataSection(uint virtualAddress)
	{
		var rva = virtualAddress - BaseAddress;
		return DataSections.Any(s => rva >= s.VirtualAddress && rva < s.VirtualAddress + s.VirtualSize);
	}
};