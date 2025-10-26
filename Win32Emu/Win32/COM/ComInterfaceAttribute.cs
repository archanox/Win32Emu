using System;
using System.Runtime.InteropServices;

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

/// <summary>
/// Helper class to calculate argument bytes for COM interface methods defined as delegates.
/// </summary>
public static class ComDelegateHelper
{
	/// <summary>
	/// Calculate the number of bytes of arguments for a delegate type.
	/// This matches the stdcall calling convention where the callee cleans up the stack.
	/// </summary>
	/// <param name="delegateType">The delegate type (must have UnmanagedFunctionPointer attribute)</param>
	/// <returns>Number of bytes of arguments on the stack</returns>
	public static int GetArgBytes(Type delegateType)
	{
		if (!typeof(Delegate).IsAssignableFrom(delegateType))
		{
			throw new ArgumentException($"Type {delegateType.Name} is not a delegate type", nameof(delegateType));
		}
		
		var invokeMethod = delegateType.GetMethod("Invoke");
		if (invokeMethod == null)
		{
			throw new InvalidOperationException($"Could not find Invoke method for delegate type {delegateType.Name}");
		}
		
		int totalBytes = 0;
		foreach (var param in invokeMethod.GetParameters())
		{
			// All parameters on x86 stack are at least 4 bytes (including pointers)
			// Larger types are passed by value and take more space
			var paramType = param.ParameterType;
			
			// Handle pointers and references
			if (paramType.IsByRef || paramType.IsPointer || paramType == typeof(IntPtr) || paramType == typeof(UIntPtr))
			{
				totalBytes += 4; // Pointers are always 4 bytes on x86
			}
			else if (paramType == typeof(byte) || paramType == typeof(sbyte) || 
			         paramType == typeof(short) || paramType == typeof(ushort) ||
			         paramType == typeof(int) || paramType == typeof(uint) ||
			         paramType == typeof(bool))
			{
				totalBytes += 4; // All these types take 4 bytes on the stack (pushed as dwords)
			}
			else if (paramType == typeof(long) || paramType == typeof(ulong) || paramType == typeof(double))
			{
				totalBytes += 8; // 64-bit types take 8 bytes
			}
			else if (paramType == typeof(float))
			{
				totalBytes += 4; // Float is 4 bytes
			}
			else if (paramType.IsValueType)
			{
				// Structs are passed by value - calculate their size
				totalBytes += Marshal.SizeOf(paramType);
			}
			else
			{
				// Reference types (classes) are passed as pointers
				totalBytes += 4;
			}
		}
		
		return totalBytes;
	}
	
	/// <summary>
	/// Verify that a delegate type has the correct UnmanagedFunctionPointer attribute.
	/// </summary>
	public static bool HasStdCallConvention(Type delegateType)
	{
		var attr = delegateType.GetCustomAttributes(typeof(UnmanagedFunctionPointerAttribute), false);
		if (attr.Length == 0)
		{
			return false;
		}
		
		var unmanagedAttr = (UnmanagedFunctionPointerAttribute)attr[0];
		return unmanagedAttr.CallingConvention == CallingConvention.StdCall;
	}
}
