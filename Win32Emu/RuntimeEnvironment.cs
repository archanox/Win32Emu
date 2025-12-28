using System.Runtime.InteropServices;

namespace Win32Emu;

/// <summary>
/// Provides utilities for detecting the runtime environment (WASM, native, etc.)
/// </summary>
public static class RuntimeEnvironment
{
	private static bool? _isWasm;
	
	/// <summary>
	/// Returns true if running in a WebAssembly environment
	/// </summary>
	public static bool IsWasm
	{
		get
		{
			if (_isWasm.HasValue)
				return _isWasm.Value;
			
			// In WASM, RuntimeInformation.OSArchitecture is Wasm
			// and RuntimeInformation.IsOSPlatform returns false for all platforms
			_isWasm = RuntimeInformation.OSArchitecture == Architecture.Wasm ||
			          RuntimeInformation.ProcessArchitecture == Architecture.Wasm;
			
			return _isWasm.Value;
		}
	}
	
	/// <summary>
	/// Returns true if running on a native platform (not WASM)
	/// </summary>
	public static bool IsNative => !IsWasm;
}
