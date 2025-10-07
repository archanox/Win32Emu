namespace Win32Emu.Win32;

/// <summary>
/// Marks a method as a DLL module export with associated metadata.
/// Multiple attributes can be applied to support different DLL versions.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class DllModuleExportAttribute : Attribute
{
	/// <summary>
	/// Used to indicate that this export is a stub implementation.
	/// </summary>
	public bool IsStub { get; init; } = false;
	
	/// <summary>
	/// The export ordinal number for this function.
	/// </summary>
	public uint Ordinal { get; }

	/// <summary>
	/// The entry point address for this function (optional).
	/// </summary>
	public uint? EntryPoint { get; }

	/// <summary>
	/// The DLL version this export applies to (optional).
	/// If not specified, the export applies to all versions.
	/// Example: "5.3.2600.5512" for Windows XP version of DDRAW.DLL
	/// </summary>
	public string? Version { get; init; }

	/// <summary>
	/// The forwarding target for this export (optional).
	/// If specified, this export forwards to another DLL's export.
	/// Example: "KERNELBASE.GetVersion" to forward to GetVersion in KERNELBASE.DLL
	/// Format: "DLL.ExportName" where DLL can optionally include .DLL extension
	/// </summary>
	public string? ForwardedTo { get; init; }

	public DllModuleExportAttribute(uint ordinal)
	{
		Ordinal = ordinal;
	}
	
	public DllModuleExportAttribute(uint ordinal,
		uint entryPoint)
	{
		Ordinal = ordinal;
		EntryPoint = entryPoint;
	}
}
