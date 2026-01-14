using Iced.Intel;

namespace Win32Emu.Rtl;

/// <summary>
/// Interface for pluggable decompiler adapters.
/// Allows different decompiler backends (custom RTL, Reko, etc.) to be used interchangeably.
/// </summary>
public interface IDecompilerAdapter
{
	/// <summary>
	/// Name of the decompiler adapter (e.g., "CustomRTL", "Reko")
	/// </summary>
	string Name { get; }
	
	/// <summary>
	/// Decompile a sequence of x86 instructions to C# source code
	/// </summary>
	/// <param name="startAddress">Starting address of the code block</param>
	/// <param name="instructions">x86 instructions to decompile</param>
	/// <param name="className">Name for the generated class</param>
	/// <returns>Generated C# source code</returns>
	Task<string> DecompileToCSharpAsync(uint startAddress, List<Instruction> instructions, string className);
	
	/// <summary>
	/// Whether this adapter is available and properly configured
	/// </summary>
	bool IsAvailable { get; }
	
	/// <summary>
	/// License information for this adapter
	/// </summary>
	string LicenseInfo { get; }
}
