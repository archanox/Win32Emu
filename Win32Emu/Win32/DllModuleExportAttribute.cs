namespace Win32Emu.Win32;

/// <summary>
/// Calling conventions for DLL export functions.
/// Used by DllModuleExportAttribute to specify how functions handle stack cleanup and parameter passing.
/// </summary>
public enum DllCallingConvention
{
	/// <summary>
	/// Default/unspecified - infer from name decoration or use stdcall.
	/// </summary>
	Default = 0,
	
	/// <summary>
	/// Standard call convention - callee cleans stack.
	/// Used by most Win32 APIs.
	/// </summary>
	Stdcall = 1,
	
	/// <summary>
	/// C declaration convention - caller cleans stack.
	/// Used by variadic functions (printf, sprintf, etc.) and standard C library functions.
	/// </summary>
	Cdecl = 2,
	
	/// <summary>
	/// Fast call convention - first two arguments in registers, callee cleans stack.
	/// </summary>
	Fastcall = 3,
	
	/// <summary>
	/// This call convention - first argument (this pointer) in ECX, callee cleans stack.
	/// Used by C++ member functions.
	/// </summary>
	Thiscall = 4,
	
	/// <summary>
	/// Pascal calling convention - callee cleans stack, arguments pushed left-to-right.
	/// </summary>
	Pascal = 5
}

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
	public bool IsStub { get; init; }
	
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

	/// <summary>
	/// The original export name from the DLL (optional).
	/// Used when the export name is not a valid C# method name.
	/// Example: "_grDepthBufferMode@4" from glide2x.dll
	/// This will be displayed in the Game Info -> DLL Imports screen.
	/// </summary>
	public string? ExportName { get; init; }

	/// <summary>
	/// The calling convention used by this export (optional).
	/// If not specified (value is Default), defaults based on name decoration:
	/// - Decorated names (e.g., "Function@N") infer stdcall/fastcall/thiscall
	/// - Undecorated names default to stdcall for backward compatibility
	/// Use Cdecl to explicitly specify cdecl for C runtime functions.
	/// </summary>
	public DllCallingConvention CallingConvention { get; init; } = DllCallingConvention.Default;

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
