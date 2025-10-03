using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Win32;

public interface IWin32ModuleUnsafe
{
	string Name { get; }

	// Optional extension: modules can implement pointer-based stubs for selected exports.
	bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue);

	// Get the export table for this module (export name -> ordinal)
	// This allows GetProcAddress to work with emulated modules
	Dictionary<string, uint> GetExportOrdinals();
}