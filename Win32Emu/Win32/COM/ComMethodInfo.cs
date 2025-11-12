using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Win32.COM
{
	/// <summary>
	/// Metadata for a COM method including handler and argument information.
	/// Use ComVtableDispatcher.FromDelegate&lt;T&gt;() to automatically calculate argBytes from delegate signatures.
	/// </summary>
	public record ComMethodInfo(
		Func<ICpu, VirtualMemory, uint> Handler,
		int ArgBytes = 0  // Argument byte count for stdcall stack cleanup (auto-calculated when using FromDelegate<T>())
	);
}