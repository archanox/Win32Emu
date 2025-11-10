using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Win32.COM
{
	/// <summary>
	/// Metadata for an async COM method including async handler and argument information.
	/// </summary>
	public record ComAsyncMethodInfo(
		Func<ICpu, VirtualMemory, Task<uint>> AsyncHandler,
		int ArgBytes = 0  // Argument byte count for stdcall stack cleanup
	);
}
