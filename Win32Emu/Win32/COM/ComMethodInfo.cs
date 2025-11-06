using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Win32.COM
{
	/// <summary>
	/// Metadata for a COM method including handler and argument information.
	/// </summary>
	/// <remarks>
	/// DEPRECATED: Use ComVtableDispatcher.FromDelegate&lt;T&gt;() instead for automatic argBytes calculation.
	/// Manual argBytes specification is error-prone and can lead to stack corruption.
	/// </remarks>
	[Obsolete("Use ComVtableDispatcher.FromDelegate<T>() to automatically calculate argBytes from delegate signatures", false)]
	public record ComMethodInfo(
		Func<ICpu, VirtualMemory, uint> Handler,
		int ArgBytes = 0  // Argument byte count for stdcall stack cleanup (deprecated - use FromDelegate<T>())
	);
}