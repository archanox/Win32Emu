namespace Win32Emu.CallingConvention;

/// <summary>
/// Represents different Win32 calling conventions for API marshalling.
/// Based on Reko's calling convention analysis for standardized parameter handling.
/// </summary>
public enum Win32CallingConvention
{
    /// <summary>
    /// Standard call convention - callee cleans stack, arguments pushed right-to-left
    /// Used by most Win32 APIs. Return value in EAX.
    /// Stack cleanup: RET N (where N = argument bytes)
    /// </summary>
    Stdcall,
    
    /// <summary>
    /// C declaration convention - caller cleans stack, arguments pushed right-to-left
    /// Used by variadic functions like printf. Return value in EAX.
    /// Stack cleanup: Caller adds ESP after RET
    /// </summary>
    Cdecl,
    
    /// <summary>
    /// Fast call convention - first two arguments in ECX/EDX, rest on stack, callee cleans stack
    /// Used for performance-critical APIs. Return value in EAX.
    /// Stack cleanup: RET N (where N = stack argument bytes only)
    /// </summary>
    Fastcall,
    
    /// <summary>
    /// This call convention - first argument (this pointer) in ECX, rest on stack, callee cleans stack
    /// Used by C++ member functions. Return value in EAX.
    /// Stack cleanup: RET N (where N = stack argument bytes only, excluding ECX)
    /// </summary>
    Thiscall,
    
    /// <summary>
    /// Pascal calling convention - callee cleans stack, arguments pushed left-to-right
    /// Used by Win16 applications and Pascal compilers. Return value in AL/AX/EAX.
    /// Stack cleanup: RET N (where N = argument bytes)
    /// Note: Argument order is REVERSED compared to stdcall (left-to-right vs right-to-left)
    /// </summary>
    Pascal
}
