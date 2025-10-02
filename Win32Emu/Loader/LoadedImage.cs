namespace Win32Emu.Loader;

public record LoadedImage(
	uint BaseAddress,
	uint EntryPointAddress,
	uint ImageSize,
	Dictionary<uint, (string dll, string name)> ImportAddressMap,
	string FilePath,
	Dictionary<string, uint> ExportsByName,
	Dictionary<uint, uint> ExportsByOrdinal
);