using System.Reflection;

namespace Win32Emu.Win32;

/// <summary>
/// Helper class to query metadata about DLL module exports at runtime.
/// </summary>
public static class DllModuleExportInfo
{
	/// <summary>
	/// Checks if a given export function is implemented in a module type.
	/// </summary>
	/// <param name="moduleType">The type of the module (e.g., typeof(DPlayXModule))</param>
	/// <param name="exportName">The export function name to check</param>
	/// <param name="version">Optional version string to match. If null, checks if any version is implemented.</param>
	/// <returns>True if the export is implemented, false otherwise</returns>
	public static bool IsExportImplemented(Type moduleType, string exportName, string? version = null)
	{
		var methods = moduleType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
		
		foreach (var method in methods)
		{
			// Check if method name matches (case-insensitive)
			if (!string.Equals(method.Name, exportName, StringComparison.OrdinalIgnoreCase))
				continue;

			// Check if it has the DllModuleExport attribute
			var attributes = method.GetCustomAttributes<DllModuleExportAttribute>();
			if (!attributes.Any())
				continue;

			// If no version specified, we found a match
			if (version == null)
				return true;

			// Check if any attribute matches the version
			if (attributes.Any(attr => attr.Version == null || attr.Version == version))
				return true;
		}

		return false;
	}

	/// <summary>
	/// Gets all export metadata for a specific function in a module.
	/// </summary>
	/// <param name="moduleType">The type of the module</param>
	/// <param name="exportName">The export function name</param>
	/// <returns>List of export metadata for all versions of the function</returns>
	public static List<DllModuleExportAttribute> GetExportAttributes(Type moduleType, string exportName)
	{
		var methods = moduleType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
		
		foreach (var method in methods)
		{
			if (string.Equals(method.Name, exportName, StringComparison.OrdinalIgnoreCase))
			{
				var attributes = method.GetCustomAttributes<DllModuleExportAttribute>();
				return attributes.ToList();
			}
		}

		return new List<DllModuleExportAttribute>();
	}

	/// <summary>
	/// Gets all exports implemented in a module.
	/// </summary>
	/// <param name="moduleType">The type of the module</param>
	/// <returns>Dictionary mapping export names to their ordinals (first version found)</returns>
	public static Dictionary<string, uint> GetAllExports(Type moduleType)
	{
		var exports = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
		var methods = moduleType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

		foreach (var method in methods)
		{
			var attributes = method.GetCustomAttributes<DllModuleExportAttribute>();
			var firstAttr = attributes.FirstOrDefault();
			if (firstAttr != null && !exports.ContainsKey(method.Name))
			{
				exports[method.Name] = firstAttr.Ordinal;
			}
		}

		return exports;
	}
}
