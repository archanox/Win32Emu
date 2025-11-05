namespace Win32Emu.Loader;

/// <summary>
/// Represents information about a PE section including its location, size, and characteristics.
/// </summary>
public record PeSection(
	string Name,                 // Section name (e.g., ".text", ".data", ".bss")
	uint VirtualAddress,         // Virtual address (RVA) where section is loaded
	uint VirtualSize,            // Size of section in memory
	uint RawSize,                // Size of section in file
	uint Characteristics         // Section flags (executable, readable, writable, etc.)
)
{
	// PE section characteristics flags (from winnt.h)
	private const uint IMAGE_SCN_CNT_CODE = 0x00000020;              // Section contains executable code
	private const uint IMAGE_SCN_CNT_INITIALIZED_DATA = 0x00000040;  // Section contains initialized data
	private const uint IMAGE_SCN_CNT_UNINITIALIZED_DATA = 0x00000080;// Section contains uninitialized data (.bss)
	private const uint IMAGE_SCN_MEM_EXECUTE = 0x20000000;           // Section can be executed as code
	private const uint IMAGE_SCN_MEM_READ = 0x40000000;              // Section can be read
	private const uint IMAGE_SCN_MEM_WRITE = 0x80000000;             // Section can be written to

	/// <summary>
	/// Returns true if this section contains executable code (has IMAGE_SCN_MEM_EXECUTE or IMAGE_SCN_CNT_CODE flag).
	/// </summary>
	public bool IsExecutable => (Characteristics & (IMAGE_SCN_MEM_EXECUTE | IMAGE_SCN_CNT_CODE)) != 0;

	/// <summary>
	/// Returns true if this section contains initialized data (has IMAGE_SCN_CNT_INITIALIZED_DATA flag).
	/// </summary>
	public bool IsData => (Characteristics & IMAGE_SCN_CNT_INITIALIZED_DATA) != 0;

	/// <summary>
	/// Returns true if this section is writable (has IMAGE_SCN_MEM_WRITE flag).
	/// </summary>
	public bool IsWritable => (Characteristics & IMAGE_SCN_MEM_WRITE) != 0;

	/// <summary>
	/// Returns true if this section is readable (has IMAGE_SCN_MEM_READ flag).
	/// </summary>
	public bool IsReadable => (Characteristics & IMAGE_SCN_MEM_READ) != 0;
};

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
	PeSection[] Sections         // PE sections with characteristics (for identifying code/data regions)
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