namespace Win32Emu.CallingConvention;

/// <summary>
/// Represents different Win32 calling conventions for API marshalling.
/// Based on Reko's calling convention analysis for standardized parameter handling.
/// </summary>
public enum Win32CallingConvention
{
    /// <summary>
    /// Standard call convention - callee cleans stack, arguments pushed right-to-left
    /// Used by most Win32 APIs
    /// </summary>
    Stdcall,
    
    /// <summary>
    /// C declaration convention - caller cleans stack, arguments pushed right-to-left
    /// Used by variadic functions like printf
    /// </summary>
    Cdecl,
    
    /// <summary>
    /// Fast call convention - first two arguments in ECX/EDX, rest on stack
    /// Used for performance-critical APIs
    /// </summary>
    Fastcall,
    
    /// <summary>
    /// This call convention - first argument (this pointer) in ECX, rest on stack
    /// Used by C++ member functions
    /// </summary>
    Thiscall
}
