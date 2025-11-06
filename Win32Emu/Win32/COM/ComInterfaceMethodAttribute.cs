using System;

namespace Win32Emu.Win32.COM;

/// <summary>
/// Marks a delegate as a COM interface method with stdcall calling convention.
/// This attribute enables automatic calculation of argument byte sizes for proper stack cleanup.
/// </summary>
[AttributeUsage(AttributeTargets.Delegate, AllowMultiple = false)]
public sealed class ComInterfaceMethodAttribute : Attribute
{
	/// <summary>
	/// Gets or sets the method index in the vtable (0-based).
	/// </summary>
	public int VtableIndex { get; set; }
	
	/// <summary>
	/// Gets or sets the method name for logging and debugging.
	/// </summary>
	public string? MethodName { get; set; }
}