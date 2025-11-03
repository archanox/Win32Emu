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
	ushort Subsystem, // PE subsystem type (IMAGE_SUBSYSTEM_WINDOWS_CUI = 3, IMAGE_SUBSYSTEM_WINDOWS_GUI = 2)
	uint HeaderEndRva,           // End of headers (RVA) based on PE: min(SizeOfHeaders, first section RVA)
	uint SizeOfStackReserve,     // From OptionalHeader
	uint SizeOfStackCommit       // From OptionalHeader
);