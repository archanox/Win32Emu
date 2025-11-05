namespace Win32Emu.Loader;

/// <summary>
/// PE section characteristics flags from winnt.h (IMAGE_SCN_* constants).
/// These flags indicate the properties and permissions of a PE section.
/// </summary>
[Flags]
public enum PeSectionCharacteristics : uint
{
	/// <summary>No flags set</summary>
	None = 0x00000000,
	
	/// <summary>Section contains executable code (IMAGE_SCN_CNT_CODE)</summary>
	ContainsCode = 0x00000020,
	
	/// <summary>Section contains initialized data (IMAGE_SCN_CNT_INITIALIZED_DATA)</summary>
	ContainsInitializedData = 0x00000040,
	
	/// <summary>Section contains uninitialized data (.bss) (IMAGE_SCN_CNT_UNINITIALIZED_DATA)</summary>
	ContainsUninitializedData = 0x00000080,
	
	/// <summary>Reserved for future use (IMAGE_SCN_LNK_OTHER)</summary>
	LinkOther = 0x00000100,
	
	/// <summary>Section contains comments or other information (IMAGE_SCN_LNK_INFO)</summary>
	LinkInfo = 0x00000200,
	
	/// <summary>Section will not become part of the image (IMAGE_SCN_LNK_REMOVE)</summary>
	LinkRemove = 0x00000800,
	
	/// <summary>Section contains COMDAT data (IMAGE_SCN_LNK_COMDAT)</summary>
	LinkComdat = 0x00001000,
	
	/// <summary>Section contains data referenced through global pointer (IMAGE_SCN_GPREL)</summary>
	GpRel = 0x00008000,
	
	/// <summary>Reserved (IMAGE_SCN_MEM_PURGEABLE)</summary>
	MemPurgeable = 0x00020000,
	
	/// <summary>Reserved (IMAGE_SCN_MEM_LOCKED)</summary>
	MemLocked = 0x00040000,
	
	/// <summary>Reserved (IMAGE_SCN_MEM_PRELOAD)</summary>
	MemPreload = 0x00080000,
	
	/// <summary>Align data on 1-byte boundary (IMAGE_SCN_ALIGN_1BYTES)</summary>
	Align1Bytes = 0x00100000,
	
	/// <summary>Align data on 2-byte boundary (IMAGE_SCN_ALIGN_2BYTES)</summary>
	Align2Bytes = 0x00200000,
	
	/// <summary>Align data on 4-byte boundary (IMAGE_SCN_ALIGN_4BYTES)</summary>
	Align4Bytes = 0x00300000,
	
	/// <summary>Align data on 8-byte boundary (IMAGE_SCN_ALIGN_8BYTES)</summary>
	Align8Bytes = 0x00400000,
	
	/// <summary>Align data on 16-byte boundary (IMAGE_SCN_ALIGN_16BYTES)</summary>
	Align16Bytes = 0x00500000,
	
	/// <summary>Align data on 32-byte boundary (IMAGE_SCN_ALIGN_32BYTES)</summary>
	Align32Bytes = 0x00600000,
	
	/// <summary>Align data on 64-byte boundary (IMAGE_SCN_ALIGN_64BYTES)</summary>
	Align64Bytes = 0x00700000,
	
	/// <summary>Align data on 128-byte boundary (IMAGE_SCN_ALIGN_128BYTES)</summary>
	Align128Bytes = 0x00800000,
	
	/// <summary>Align data on 256-byte boundary (IMAGE_SCN_ALIGN_256BYTES)</summary>
	Align256Bytes = 0x00900000,
	
	/// <summary>Align data on 512-byte boundary (IMAGE_SCN_ALIGN_512BYTES)</summary>
	Align512Bytes = 0x00A00000,
	
	/// <summary>Align data on 1024-byte boundary (IMAGE_SCN_ALIGN_1024BYTES)</summary>
	Align1024Bytes = 0x00B00000,
	
	/// <summary>Align data on 2048-byte boundary (IMAGE_SCN_ALIGN_2048BYTES)</summary>
	Align2048Bytes = 0x00C00000,
	
	/// <summary>Align data on 4096-byte boundary (IMAGE_SCN_ALIGN_4096BYTES)</summary>
	Align4096Bytes = 0x00D00000,
	
	/// <summary>Align data on 8192-byte boundary (IMAGE_SCN_ALIGN_8192BYTES)</summary>
	Align8192Bytes = 0x00E00000,
	
	/// <summary>Section contains extended relocations (IMAGE_SCN_LNK_NRELOC_OVFL)</summary>
	LinkNRelocOvfl = 0x01000000,
	
	/// <summary>Section can be discarded as needed (IMAGE_SCN_MEM_DISCARDABLE)</summary>
	MemDiscardable = 0x02000000,
	
	/// <summary>Section cannot be cached (IMAGE_SCN_MEM_NOT_CACHED)</summary>
	MemNotCached = 0x04000000,
	
	/// <summary>Section is not pageable (IMAGE_SCN_MEM_NOT_PAGED)</summary>
	MemNotPaged = 0x08000000,
	
	/// <summary>Section can be shared in memory (IMAGE_SCN_MEM_SHARED)</summary>
	MemShared = 0x10000000,
	
	/// <summary>Section can be executed as code (IMAGE_SCN_MEM_EXECUTE)</summary>
	MemExecute = 0x20000000,
	
	/// <summary>Section can be read (IMAGE_SCN_MEM_READ)</summary>
	MemRead = 0x40000000,
	
	/// <summary>Section can be written to (IMAGE_SCN_MEM_WRITE)</summary>
	MemWrite = 0x80000000,
}

/// <summary>
/// Represents information about a PE section including its location, size, and characteristics.
/// </summary>
public record PeSection(
	string Name,                           // Section name (e.g., ".text", ".data", ".bss")
	uint VirtualAddress,                   // Virtual address (RVA) where section is loaded
	uint VirtualSize,                      // Size of section in memory
	uint RawSize,                          // Size of section in file
	PeSectionCharacteristics Characteristics // Section flags (executable, readable, writable, etc.)
)
{
	/// <summary>
	/// Returns true if this section contains executable code (has MemExecute or ContainsCode flag).
	/// </summary>
	public bool IsExecutable => (Characteristics & (PeSectionCharacteristics.MemExecute | PeSectionCharacteristics.ContainsCode)) != 0;

	/// <summary>
	/// Returns true if this section contains initialized data (has ContainsInitializedData flag).
	/// </summary>
	public bool IsData => (Characteristics & PeSectionCharacteristics.ContainsInitializedData) != 0;

	/// <summary>
	/// Returns true if this section is writable (has MemWrite flag).
	/// </summary>
	public bool IsWritable => (Characteristics & PeSectionCharacteristics.MemWrite) != 0;

	/// <summary>
	/// Returns true if this section is readable (has MemRead flag).
	/// </summary>
	public bool IsReadable => (Characteristics & PeSectionCharacteristics.MemRead) != 0;
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