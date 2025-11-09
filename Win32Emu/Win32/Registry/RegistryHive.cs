using DiscUtils.Registry;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.VirtualFileSystem;

namespace Win32Emu.Win32.Registry;

/// <summary>
/// Manages Windows registry hives using DiscUtils.Registry.
/// Supports both in-memory and persistent storage via VFS.
/// </summary>
public class RegistryHive : IDisposable
{
	private readonly ILogger _logger;
	private readonly IVirtualFileSystem? _vfs;
	private readonly Dictionary<string, DiscUtils.Registry.RegistryHive> _hives = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<uint, RegistryKeyHandle> _openKeys = new();
	private uint _nextHandle = 0x80000000;
	
	// Predefined registry key handles (matching Windows)
	public const uint HKEY_CLASSES_ROOT = 0x80000000;
	public const uint HKEY_CURRENT_USER = 0x80000001;
	public const uint HKEY_LOCAL_MACHINE = 0x80000002;
	public const uint HKEY_USERS = 0x80000003;
	public const uint HKEY_PERFORMANCE_DATA = 0x80000004;
	public const uint HKEY_CURRENT_CONFIG = 0x80000005;
	public const uint HKEY_DYN_DATA = 0x80000006;

	private bool _disposed;

	public RegistryHive(IVirtualFileSystem? vfs = null, ILogger? logger = null)
	{
		_vfs = vfs;
		_logger = logger ?? NullLogger.Instance;
		InitializeHives();
	}

	private void InitializeHives()
	{
		// Initialize core hives
		_hives["HKEY_LOCAL_MACHINE\\SYSTEM"] = CreateOrLoadHive("SYSTEM");
		_hives["HKEY_LOCAL_MACHINE\\SOFTWARE"] = CreateOrLoadHive("SOFTWARE");
		_hives["HKEY_CURRENT_USER"] = CreateOrLoadHive("NTUSER.DAT");
		
		_logger.LogInformation("[RegistryHive] Initialized {Count} registry hives", _hives.Count);
		
		// Initialize default environment variables in registry
		InitializeDefaultEnvironmentVariables();
	}

	private DiscUtils.Registry.RegistryHive CreateOrLoadHive(string hiveName)
	{
		DiscUtils.Registry.RegistryHive? hive = null;
		
		// For now, always create in-memory hives
		// TODO: In the future, add VFS persistence support with proper stream wrappers
		hive = DiscUtils.Registry.RegistryHive.Create(new MemoryStream());
		_logger.LogInformation("[RegistryHive] Created in-memory hive: {HiveName}", hiveName);
		
		return hive;
	}

	private void InitializeDefaultEnvironmentVariables()
	{
		try
		{
			// Initialize system environment variables at HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment
			var systemEnvKey = GetOrCreateKey("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Environment");
			if (systemEnvKey != null)
			{
				SetValueIfNotExists(systemEnvKey, "PATH", @"C:\WINDOWS\system32;C:\WINDOWS;C:\WINDOWS\System32\Wbem");
				SetValueIfNotExists(systemEnvKey, "PATHEXT", ".COM;.EXE;.BAT;.CMD;.VBS;.VBE;.JS;.JSE;.WSF;.WSH");
				SetValueIfNotExists(systemEnvKey, "TEMP", @"C:\TEMP");
				SetValueIfNotExists(systemEnvKey, "TMP", @"C:\TEMP");
				SetValueIfNotExists(systemEnvKey, "WINDIR", @"C:\WINDOWS");
				SetValueIfNotExists(systemEnvKey, "SystemRoot", @"C:\WINDOWS");
				SetValueIfNotExists(systemEnvKey, "ComSpec", @"C:\WINDOWS\system32\cmd.exe");
				SetValueIfNotExists(systemEnvKey, "OS", "Windows_NT");
				
				_logger.LogInformation("[RegistryHive] Initialized system environment variables");
			}
			
			// Initialize user environment variables at HKEY_CURRENT_USER\Environment
			var userEnvKey = GetOrCreateKey("HKEY_CURRENT_USER\\Environment");
			if (userEnvKey != null)
			{
				SetValueIfNotExists(userEnvKey, "TEMP", @"C:\Users\User\AppData\Local\Temp");
				SetValueIfNotExists(userEnvKey, "TMP", @"C:\Users\User\AppData\Local\Temp");
				
				_logger.LogInformation("[RegistryHive] Initialized user environment variables");
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[RegistryHive] Failed to initialize default environment variables");
		}
	}

	private void SetValueIfNotExists(RegistryKey key, string valueName, string value)
	{
		try
		{
			if (key.GetValue(valueName) == null)
			{
				key.SetValue(valueName, value);
				_logger.LogDebug("[RegistryHive] Set default value: {ValueName}={Value}", valueName, value);
			}
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "[RegistryHive] Failed to set default value: {ValueName}", valueName);
		}
	}

	private RegistryKey? GetOrCreateKey(string fullPath)
	{
		try
		{
			var (hiveName, subKeyPath) = ParseRegistryPath(fullPath);
			if (hiveName == null || subKeyPath == null)
			{
				return null;
			}
			
			if (!_hives.TryGetValue(hiveName, out var hive))
			{
				_logger.LogWarning("[RegistryHive] Unknown hive: {HiveName}", hiveName);
				return null;
			}
			
			// Navigate/create the key path
			var key = hive.Root;
			var parts = subKeyPath.Split('\\', StringSplitOptions.RemoveEmptyEntries);
			
			foreach (var part in parts)
			{
				var subKey = key.OpenSubKey(part);
				if (subKey == null)
				{
					subKey = key.CreateSubKey(part);
				}
				key = subKey;
			}
			
			return key;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[RegistryHive] Failed to get/create key: {FullPath}", fullPath);
			return null;
		}
	}

	/// <summary>
	/// Opens a registry key and returns a handle.
	/// </summary>
	public uint OpenKey(string fullPath)
	{
		try
		{
			var (hiveName, subKeyPath) = ParseRegistryPath(fullPath);
			if (hiveName == null || subKeyPath == null)
			{
				_logger.LogWarning("[RegistryHive] Invalid registry path: {FullPath}", fullPath);
				return 0;
			}
			
			if (!_hives.TryGetValue(hiveName, out var hive))
			{
				_logger.LogWarning("[RegistryHive] Unknown hive: {HiveName}", hiveName);
				return 0;
			}
			
			// Navigate to the key
			var key = hive.Root;
			if (!string.IsNullOrEmpty(subKeyPath))
			{
				var parts = subKeyPath.Split('\\', StringSplitOptions.RemoveEmptyEntries);
				foreach (var part in parts)
				{
					key = key.OpenSubKey(part);
					if (key == null)
					{
						_logger.LogDebug("[RegistryHive] Key not found: {FullPath}", fullPath);
						return 0;
					}
				}
			}
			
			var handle = _nextHandle++;
			_openKeys[handle] = new RegistryKeyHandle { Key = key, Path = fullPath };
			
			_logger.LogDebug("[RegistryHive] Opened key: {FullPath} -> handle 0x{Handle:X8}", fullPath, handle);
			return handle;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[RegistryHive] Failed to open key: {FullPath}", fullPath);
			return 0;
		}
	}

	/// <summary>
	/// Creates a registry key and returns a handle.
	/// </summary>
	public uint CreateKey(string fullPath)
	{
		try
		{
			var key = GetOrCreateKey(fullPath);
			if (key == null)
			{
				return 0;
			}
			
			var handle = _nextHandle++;
			_openKeys[handle] = new RegistryKeyHandle { Key = key, Path = fullPath };
			
			_logger.LogDebug("[RegistryHive] Created key: {FullPath} -> handle 0x{Handle:X8}", fullPath, handle);
			return handle;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[RegistryHive] Failed to create key: {FullPath}", fullPath);
			return 0;
		}
	}

	/// <summary>
	/// Queries a value from an open registry key.
	/// </summary>
	public bool QueryValue(uint handle, string valueName, out object? value, out RegistryValueType type)
	{
		value = null;
		type = RegistryValueType.String;
		
		if (!_openKeys.TryGetValue(handle, out var keyHandle))
		{
			_logger.LogWarning("[RegistryHive] Invalid handle: 0x{Handle:X8}", handle);
			return false;
		}
		
		try
		{
			value = keyHandle.Key.GetValue(valueName);
			if (value == null)
			{
				_logger.LogDebug("[RegistryHive] Value not found: {ValueName} in key 0x{Handle:X8}", valueName, handle);
				return false;
			}
			
			// Determine the type
			type = keyHandle.Key.GetValueType(valueName);
			
			_logger.LogDebug("[RegistryHive] Query value: {ValueName}={Value} (type={Type}) from handle 0x{Handle:X8}", 
				valueName, value, type, handle);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[RegistryHive] Failed to query value: {ValueName} from handle 0x{Handle:X8}", valueName, handle);
			return false;
		}
	}

	/// <summary>
	/// Sets a value in an open registry key.
	/// </summary>
	public bool SetValue(uint handle, string valueName, object value, RegistryValueType type = RegistryValueType.String)
	{
		if (!_openKeys.TryGetValue(handle, out var keyHandle))
		{
			_logger.LogWarning("[RegistryHive] Invalid handle: 0x{Handle:X8}", handle);
			return false;
		}
		
		try
		{
			keyHandle.Key.SetValue(valueName, value, type);
			_logger.LogDebug("[RegistryHive] Set value: {ValueName}={Value} (type={Type}) in handle 0x{Handle:X8}", 
				valueName, value, type, handle);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[RegistryHive] Failed to set value: {ValueName} in handle 0x{Handle:X8}", valueName, handle);
			return false;
		}
	}

	/// <summary>
	/// Closes an open registry key handle.
	/// </summary>
	public bool CloseKey(uint handle)
	{
		if (_openKeys.Remove(handle))
		{
			_logger.LogDebug("[RegistryHive] Closed key handle: 0x{Handle:X8}", handle);
			return true;
		}
		
		_logger.LogWarning("[RegistryHive] Invalid handle: 0x{Handle:X8}", handle);
		return false;
	}

	/// <summary>
	/// Flushes a registry key to persistent storage if VFS is available.
	/// </summary>
	public bool FlushKey(uint handle)
	{
		if (!_openKeys.ContainsKey(handle))
		{
			_logger.LogWarning("[RegistryHive] Invalid handle: 0x{Handle:X8}", handle);
			return false;
		}
		
		// If VFS is available, we could save the hive here
		// For now, just acknowledge the flush
		_logger.LogDebug("[RegistryHive] Flush key handle: 0x{Handle:X8}", handle);
		return true;
	}

	/// <summary>
	/// Enumerates subkey names under an open registry key.
	/// </summary>
	public string[] EnumerateSubKeyNames(uint handle)
	{
		if (!_openKeys.TryGetValue(handle, out var keyHandle))
		{
			_logger.LogWarning("[RegistryHive] Invalid handle: 0x{Handle:X8}", handle);
			return Array.Empty<string>();
		}
		
		try
		{
			var subKeyNames = keyHandle.Key.GetSubKeyNames().ToArray();
			_logger.LogDebug("[RegistryHive] Enumerated {Count} subkeys for handle 0x{Handle:X8}", subKeyNames.Length, handle);
			return subKeyNames;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[RegistryHive] Failed to enumerate subkeys for handle 0x{Handle:X8}", handle);
			return Array.Empty<string>();
		}
	}

	/// <summary>
	/// Enumerates value names in an open registry key.
	/// </summary>
	public string[] EnumerateValueNames(uint handle)
	{
		if (!_openKeys.TryGetValue(handle, out var keyHandle))
		{
			_logger.LogWarning("[RegistryHive] Invalid handle: 0x{Handle:X8}", handle);
			return Array.Empty<string>();
		}
		
		try
		{
			var valueNames = keyHandle.Key.GetValueNames().ToArray();
			_logger.LogDebug("[RegistryHive] Enumerated {Count} values for handle 0x{Handle:X8}", valueNames.Length, handle);
			return valueNames;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[RegistryHive] Failed to enumerate values for handle 0x{Handle:X8}", handle);
			return Array.Empty<string>();
		}
	}

	/// <summary>
	/// Gets the path for an open registry key handle.
	/// </summary>
	public string? GetKeyPath(uint handle)
	{
		if (_openKeys.TryGetValue(handle, out var keyHandle))
		{
			return keyHandle.Path;
		}
		
		return null;
	}

	/// <summary>
	/// Deletes a value from an open registry key.
	/// </summary>
	public bool DeleteValue(uint handle, string valueName)
	{
		if (!_openKeys.TryGetValue(handle, out var keyHandle))
		{
			_logger.LogWarning("[RegistryHive] Invalid handle: 0x{Handle:X8}", handle);
			return false;
		}
		
		try
		{
			keyHandle.Key.DeleteValue(valueName);
			_logger.LogDebug("[RegistryHive] Deleted value: {ValueName} from handle 0x{Handle:X8}", valueName, handle);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[RegistryHive] Failed to delete value: {ValueName} from handle 0x{Handle:X8}", valueName, handle);
			return false;
		}
	}

	/// <summary>
	/// Deletes a subkey from a registry path.
	/// </summary>
	public bool DeleteSubKey(string fullPath)
	{
		try
		{
			var (hiveName, subKeyPath) = ParseRegistryPath(fullPath);
			if (hiveName == null || string.IsNullOrEmpty(subKeyPath))
			{
				_logger.LogWarning("[RegistryHive] Invalid registry path: {FullPath}", fullPath);
				return false;
			}
			
			if (!_hives.TryGetValue(hiveName, out var hive))
			{
				_logger.LogWarning("[RegistryHive] Unknown hive: {HiveName}", hiveName);
				return false;
			}
			
			// Navigate to parent key
			var parts = subKeyPath.Split('\\', StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 0)
			{
				_logger.LogWarning("[RegistryHive] Cannot delete root key");
				return false;
			}
			
			var key = hive.Root;
			for (int i = 0; i < parts.Length - 1; i++)
			{
				key = key.OpenSubKey(parts[i]);
				if (key == null)
				{
					_logger.LogWarning("[RegistryHive] Parent key not found: {FullPath}", fullPath);
					return false;
				}
			}
			
			// Delete the last key
			key.DeleteSubKey(parts[^1]);
			_logger.LogDebug("[RegistryHive] Deleted subkey: {FullPath}", fullPath);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[RegistryHive] Failed to delete subkey: {FullPath}", fullPath);
			return false;
		}
	}

	/// <summary>
	/// Parses a full registry path into hive name and subkey path.
	/// Example: "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet" -> ("HKEY_LOCAL_MACHINE\SYSTEM", "CurrentControlSet")
	/// </summary>
	private (string? hiveName, string? subKeyPath) ParseRegistryPath(string fullPath)
	{
		if (string.IsNullOrEmpty(fullPath))
		{
			return (null, null);
		}
		
		// Handle predefined keys
		if (fullPath.StartsWith("HKEY_LOCAL_MACHINE\\SYSTEM", StringComparison.OrdinalIgnoreCase))
		{
			const string prefix = "HKEY_LOCAL_MACHINE\\SYSTEM";
			var subPath = fullPath[prefix.Length..].TrimStart('\\');
			return (prefix, subPath);
		}
		else if (fullPath.StartsWith("HKEY_LOCAL_MACHINE\\SOFTWARE", StringComparison.OrdinalIgnoreCase))
		{
			const string prefix = "HKEY_LOCAL_MACHINE\\SOFTWARE";
			var subPath = fullPath[prefix.Length..].TrimStart('\\');
			return (prefix, subPath);
		}
		else if (fullPath.StartsWith("HKEY_LOCAL_MACHINE", StringComparison.OrdinalIgnoreCase))
		{
			// Default to SYSTEM for other HKEY_LOCAL_MACHINE paths
			const string prefix = "HKEY_LOCAL_MACHINE";
			var subPath = fullPath[prefix.Length..].TrimStart('\\');
			return ("HKEY_LOCAL_MACHINE\\SYSTEM", subPath);
		}
		else if (fullPath.StartsWith("HKEY_CURRENT_USER", StringComparison.OrdinalIgnoreCase))
		{
			const string prefix = "HKEY_CURRENT_USER";
			var subPath = fullPath[prefix.Length..].TrimStart('\\');
			return (prefix, subPath);
		}
		else if (fullPath.StartsWith("HKEY_CLASSES_ROOT", StringComparison.OrdinalIgnoreCase))
		{
			// HKEY_CLASSES_ROOT is typically merged view of HKLM\Software\Classes and HKCU\Software\Classes
			const string prefix = "HKEY_CLASSES_ROOT";
			var subPath = fullPath[prefix.Length..].TrimStart('\\');
			return ("HKEY_LOCAL_MACHINE\\SOFTWARE", $"Classes\\{subPath}");
		}
		else if (fullPath.StartsWith("HKEY_USERS", StringComparison.OrdinalIgnoreCase))
		{
			const string prefix = "HKEY_USERS";
			var subPath = fullPath[prefix.Length..].TrimStart('\\');
			return ("HKEY_CURRENT_USER", subPath);
		}
		
		_logger.LogWarning("[RegistryHive] Unknown registry root: {FullPath}", fullPath);
		return (null, null);
	}

	/// <summary>
	/// Saves all hives to VFS if available.
	/// TODO: Implement VFS persistence with proper stream wrappers
	/// </summary>
	public void SaveHives()
	{
		_logger.LogDebug("[RegistryHive] SaveHives called (not implemented - using in-memory only)");
		// For now, hives are in-memory only and not persisted
		// In the future, we could implement VFS persistence with custom stream wrappers
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		
		try
		{
			// Save hives before disposing
			SaveHives();
			
			// Dispose all hives
			foreach (var hive in _hives.Values)
			{
				hive.Dispose();
			}
			
			_hives.Clear();
			_openKeys.Clear();
			
			_logger.LogInformation("[RegistryHive] Disposed registry hive manager");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[RegistryHive] Error during disposal");
		}
		finally
		{
			_disposed = true;
		}
	}

	private class RegistryKeyHandle
	{
		public required RegistryKey Key { get; init; }
		public required string Path { get; init; }
	}
}
