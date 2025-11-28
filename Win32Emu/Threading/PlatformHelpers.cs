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

	/// <summary>
	/// Asynchronously delays for the specified amount of time.
	/// This is the preferred approach for delays in async code paths.
	/// </summary>
	/// <param name="milliseconds">The number of milliseconds to delay</param>
	/// <param name="cancellationToken">Optional cancellation token</param>
	public static Task DelayAsync(int milliseconds, CancellationToken cancellationToken = default)
	{
		if (milliseconds <= 0)
		{
			return Task.CompletedTask;
		}
		
		return Task.Delay(milliseconds, cancellationToken);
	}

	/// <summary>
	/// Performs a brief yield/delay. On native platforms, uses Thread.Yield for efficiency.
	/// On WASM, this is a no-op since yielding is handled by the async runtime.
	/// </summary>
	public static void YieldOrSpin()
	{
		if (IsWasm)
		{
			// No-op in WASM - the async model handles yielding
			return;
		}
		
		Thread.Yield();
	}

	/// <summary>
	/// Checks if blocking operations are supported on the current platform.
	/// Returns false for WASM environments.
	/// </summary>
	public static bool SupportsBlockingOperations => !IsWasm;
}
