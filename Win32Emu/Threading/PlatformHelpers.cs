using System.Runtime.InteropServices;

namespace Win32Emu.Threading;

/// <summary>
/// Provides platform-aware threading utilities that work correctly on both
/// native platforms and WebAssembly (WASM).
/// 
/// In WASM environments, blocking operations like Thread.Sleep and Thread.Yield
/// throw PlatformNotSupportedException. This class provides safe alternatives
/// that either use async patterns or become no-ops in WASM.
/// </summary>
public static class PlatformHelpers
{
	/// <summary>
	/// Indicates if the runtime is WebAssembly
	/// </summary>
	public static readonly bool IsWasm = RuntimeInformation.ProcessArchitecture is Architecture.Wasm;

	/// <summary>
	/// Suspends the current thread for the specified amount of time.
	/// In WASM environments, this is a no-op since Thread.Sleep is not supported.
	/// </summary>
	/// <param name="milliseconds">The number of milliseconds to sleep</param>
	public static void Sleep(int milliseconds)
	{
		if (IsWasm)
		{
			// Thread.Sleep is not supported in WASM - skip the sleep
			// The emulator's main loop will still yield via async/await
			return;
		}
		
		Thread.Sleep(milliseconds);
	}

	/// <summary>
	/// Causes the calling thread to yield execution to another thread that is ready to run.
	/// In WASM environments, this is a no-op since Thread.Yield is not supported.
	/// </summary>
	public static void Yield()
	{
		if (IsWasm)
		{
			// Thread.Yield is not supported in WASM - skip
			return;
		}
		
		Thread.Yield();
	}
}
