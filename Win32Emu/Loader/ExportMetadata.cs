namespace Win32Emu.Loader;

/// <summary>
/// Represents the calling convention used by a function.
/// Matches Win32CallingConvention from Win32Emu.CallingConvention project.
/// </summary>
public enum CallingConvention
{
	/// <summary>
	/// Standard call convention - callee cleans stack, arguments pushed right-to-left, all on stack.
	/// Used by most Win32 APIs. Return value in EAX.
	/// Stack cleanup: RET N (where N = argument bytes)
	/// </summary>
	Stdcall,

	/// <summary>
	/// C declaration convention - caller cleans stack, arguments pushed right-to-left, all on stack.
	/// Used by variadic functions like printf.  Return value in EAX.
	/// Stack cleanup: Caller adds ESP after RET
	/// </summary>
	Cdecl,

	/// <summary>
	/// Fast call convention - first two arguments in ECX/EDX, rest on stack, callee cleans stack.
	/// Used for performance-critical APIs. Return value in EAX.
	/// Stack cleanup: RET N (where N = stack argument bytes only)
	/// </summary>
	Fastcall,

	/// <summary>
	/// This call convention - first argument (this pointer) in ECX, rest on stack, callee cleans stack.
	/// Used by C++ member functions. Return value in EAX.
	/// Stack cleanup: RET N (where N = stack argument bytes only, excluding ECX)
	/// </summary>
	Thiscall
}

/// <summary>
/// Metadata about a DLL export function, including calling convention and argument information.
/// </summary>
public record ExportMetadata
{
	/// <summary>
	/// The calling convention used by this export function.
	/// </summary>
	public CallingConvention Convention { get; init; } = CallingConvention.Stdcall;

	/// <summary>
	/// Total number of argument bytes on the stack (does NOT include register-passed arguments).
	/// For stdcall/cdecl: all argument bytes
	/// For fastcall: argument bytes excluding first 2 register args
	/// For thiscall: argument bytes excluding 'this' in ECX
	/// </summary>
	public int StackArgBytes { get; init; }

	/// <summary>
	/// Original export name from the PE file (may include decoration like @, _, etc.)
	/// </summary>
	public string? OriginalName { get; init; }

	/// <summary>
	/// Whether this metadata was inferred from name decoration or explicitly configured.
	/// </summary>
	public bool IsInferred { get; init; }

	/// <summary>
	/// Creates default metadata assuming stdcall with no arguments.
	/// This is used as a safe default for exports without explicit metadata.
	/// </summary>
	public static ExportMetadata Default { get; } = new()
	{
		Convention = CallingConvention.Stdcall,
		StackArgBytes = 0,
		IsInferred = true
	};

	/// <summary>
	/// Parses calling convention and argument bytes from a decorated export name.
	/// Supports stdcall decoration (FunctionName@N where N = stack bytes).
	/// </summary>
	/// <param name="exportName">The export name, potentially decorated</param>
	/// <returns>ExportMetadata parsed from decoration, or null if no decoration found</returns>
	public static ExportMetadata? FromDecoratedName(string exportName)
	{
		if (string.IsNullOrEmpty(exportName))
			return null;

		// Stdcall decoration: FunctionName@N where N is the number of stack bytes
		// Example: MessageBoxA@16 means stdcall with 16 bytes of arguments
		var atIndex = exportName.IndexOf('@');
		if (atIndex > 0 && atIndex < exportName.Length - 1)
		{
			var bytesStr = exportName.Substring(atIndex + 1);
			if (int.TryParse(bytesStr, out var stackBytes) && stackBytes >= 0)
			{
				return new ExportMetadata
				{
					Convention = CallingConvention.Stdcall,
					StackArgBytes = stackBytes,
					OriginalName = exportName,
					IsInferred = true
				};
			}
		}

		// Fastcall decoration: @FunctionName@N
		// Example: @FastFunction@8 means fastcall (first 2 args in registers, 8 bytes on stack)
		if (exportName.StartsWith("@"))
		{
			atIndex = exportName.IndexOf('@', 1);
			if (atIndex > 1 && atIndex < exportName.Length - 1)
			{
				var bytesStr = exportName.Substring(atIndex + 1);
				if (int.TryParse(bytesStr, out var stackBytes) && stackBytes >= 0)
				{
					return new ExportMetadata
					{
						Convention = CallingConvention.Fastcall,
						StackArgBytes = stackBytes,
						OriginalName = exportName,
						IsInferred = true
					};
				}
			}
		}

		// C++ thiscall decoration: ?FunctionName@@... (more complex, often has ? prefix)
		// For now, we don't parse full C++ mangling, but detect the pattern
		if (exportName.StartsWith("?") && exportName.Contains("@@"))
		{
			// Assume thiscall for C++ member functions
			return new ExportMetadata
			{
				Convention = CallingConvention.Thiscall,
				StackArgBytes = 0, // Cannot determine from mangling alone
				OriginalName = exportName,
				IsInferred = true
			};
		}

		// No decoration found
		return null;
	}
}
