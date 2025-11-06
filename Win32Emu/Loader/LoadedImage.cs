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
	Dictionary<uint, uint> IatEntryMap  // IAT VA -> expected synthetic address mapping for runtime verification
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