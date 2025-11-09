using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Win32;
using DiscUtils.Registry;

namespace Win32Emu.Gui.ViewModels;

/// <summary>
/// ViewModel for the Registry Viewer window
/// </summary>
public partial class RegistryViewerViewModel : ViewModelBase
{
	[ObservableProperty]
	private ObservableCollection<RegistryKeyNode> _rootKeys = new();

	[ObservableProperty]
	private RegistryKeyNode? _selectedKey;

	[ObservableProperty]
	private ObservableCollection<RegistryValueItem> _values = new();

	[ObservableProperty]
	private RegistryValueItem? _selectedValue;

	[ObservableProperty]
	private string _statusText = "Ready";

	private readonly ProcessEnvironment? _processEnv;
	private readonly ILogger _logger;

	public RegistryViewerViewModel(ProcessEnvironment? processEnv = null, ILogger? logger = null)
	{
		_processEnv = processEnv;
		_logger = logger ?? NullLogger.Instance;
		
		InitializeRootKeys();
	}

	partial void OnSelectedKeyChanged(RegistryKeyNode? value)
	{
		if (value != null)
		{
			LoadValues(value);
		}
	}

	private void InitializeRootKeys()
	{
		// Create root keys
		RootKeys.Add(new RegistryKeyNode("HKEY_LOCAL_MACHINE", "HKEY_LOCAL_MACHINE", true));
		RootKeys.Add(new RegistryKeyNode("HKEY_CURRENT_USER", "HKEY_CURRENT_USER", true));
		RootKeys.Add(new RegistryKeyNode("HKEY_CLASSES_ROOT", "HKEY_CLASSES_ROOT", true));
		RootKeys.Add(new RegistryKeyNode("HKEY_USERS", "HKEY_USERS", true));
		
		// Load first level if registry is available
		if (_processEnv?.RegistryHive != null)
		{
			foreach (var rootKey in RootKeys)
			{
				LoadSubKeys(rootKey);
			}
		}
	}

	private void LoadSubKeys(RegistryKeyNode node)
	{
		if (_processEnv?.RegistryHive == null)
			return;

		try
		{
			var handle = _processEnv.RegistryHive.OpenKey(node.FullPath);
			if (handle != 0)
			{
				var subKeyNames = _processEnv.RegistryHive.EnumerateSubKeyNames(handle);
				node.Children.Clear();
				
				foreach (var subKeyName in subKeyNames.OrderBy(s => s))
				{
					var childPath = $"{node.FullPath}\\{subKeyName}";
					var childNode = new RegistryKeyNode(subKeyName, childPath, true);
					node.Children.Add(childNode);
				}
				
				_processEnv.RegistryHive.CloseKey(handle);
				StatusText = $"Loaded {subKeyNames.Length} subkeys from {node.Name}";
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[RegistryViewer] Failed to load subkeys for {Path}", node.FullPath);
			StatusText = $"Error loading subkeys: {ex.Message}";
		}
	}

	private void LoadValues(RegistryKeyNode node)
	{
		Values.Clear();

		if (_processEnv?.RegistryHive == null)
			return;

		try
		{
			var handle = _processEnv.RegistryHive.OpenKey(node.FullPath);
			if (handle != 0)
			{
				var valueNames = _processEnv.RegistryHive.EnumerateValueNames(handle);
				
				foreach (var valueName in valueNames.OrderBy(s => s))
				{
					if (_processEnv.RegistryHive.QueryValue(handle, valueName, out var value, out var type))
					{
						var displayValue = FormatValue(value, type);
						Values.Add(new RegistryValueItem
						{
							Name = string.IsNullOrEmpty(valueName) ? "(Default)" : valueName,
							Type = type.ToString(),
							Value = displayValue,
							RawValue = value
						});
					}
				}
				
				_processEnv.RegistryHive.CloseKey(handle);
				StatusText = $"Loaded {valueNames.Length} values from {node.FullPath}";
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[RegistryViewer] Failed to load values for {Path}", node.FullPath);
			StatusText = $"Error loading values: {ex.Message}";
		}
	}

	private static string FormatValue(object? value, RegistryValueType type)
	{
		if (value == null)
			return "(null)";

		return type switch
		{
			RegistryValueType.Dword => $"0x{value:X8} ({value})",
			RegistryValueType.Binary when value is byte[] bytes => 
				BitConverter.ToString(bytes).Replace("-", " "),
			_ => value.ToString() ?? "(empty)"
		};
	}

	[RelayCommand]
	private void Refresh()
	{
		if (SelectedKey != null)
		{
			LoadSubKeys(SelectedKey);
			LoadValues(SelectedKey);
		}
	}

	[RelayCommand]
	private void AddKey()
	{
		if (SelectedKey == null)
		{
			StatusText = "Please select a parent key first";
			return;
		}

		// This would open a dialog to get key name
		// For now, just show status
		StatusText = "Add key functionality - to be implemented with dialog";
	}

	[RelayCommand]
	private void DeleteKey()
	{
		if (SelectedKey == null)
		{
			StatusText = "Please select a key to delete";
			return;
		}

		if (_processEnv?.RegistryHive == null)
			return;

		try
		{
			if (_processEnv.RegistryHive.DeleteSubKey(SelectedKey.FullPath))
			{
				StatusText = $"Deleted key: {SelectedKey.Name}";
				// Reload parent
				Refresh();
			}
			else
			{
				StatusText = "Failed to delete key";
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[RegistryViewer] Failed to delete key");
			StatusText = $"Error: {ex.Message}";
		}
	}

	[RelayCommand]
	private void AddValue()
	{
		if (SelectedKey == null)
		{
			StatusText = "Please select a key first";
			return;
		}

		// This would open a dialog to get value details
		StatusText = "Add value functionality - to be implemented with dialog";
	}

	[RelayCommand]
	private void EditValue()
	{
		if (SelectedValue == null)
		{
			StatusText = "Please select a value to edit";
			return;
		}

		// This would open a dialog to edit the value
		StatusText = $"Edit value: {SelectedValue.Name} - to be implemented with dialog";
	}

	[RelayCommand]
	private void DeleteValue()
	{
		if (SelectedValue == null || SelectedKey == null)
		{
			StatusText = "Please select a value to delete";
			return;
		}

		if (_processEnv?.RegistryHive == null)
			return;

		try
		{
			var handle = _processEnv.RegistryHive.OpenKey(SelectedKey.FullPath);
			if (handle != 0)
			{
				var valueName = SelectedValue.Name == "(Default)" ? "" : SelectedValue.Name;
				if (_processEnv.RegistryHive.DeleteValue(handle, valueName))
				{
					StatusText = $"Deleted value: {SelectedValue.Name}";
					LoadValues(SelectedKey);
				}
				else
				{
					StatusText = "Failed to delete value";
				}
				_processEnv.RegistryHive.CloseKey(handle);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[RegistryViewer] Failed to delete value");
			StatusText = $"Error: {ex.Message}";
		}
	}
}

/// <summary>
/// Represents a registry key node in the tree view
/// </summary>
public class RegistryKeyNode : ObservableObject
{
	private bool _isExpanded;
	
	public string Name { get; set; }
	public string FullPath { get; set; }
	public ObservableCollection<RegistryKeyNode> Children { get; set; } = new();

	public bool IsExpanded
	{
		get => _isExpanded;
		set => SetProperty(ref _isExpanded, value);
	}

	public RegistryKeyNode(string name, string fullPath, bool hasChildren = false)
	{
		Name = name;
		FullPath = fullPath;
		
		// Add dummy child if this key has children (for lazy loading)
		if (hasChildren)
		{
			Children.Add(new RegistryKeyNode("Loading...", "", false));
		}
	}
}

/// <summary>
/// Represents a registry value item in the list
/// </summary>
public class RegistryValueItem
{
	public string Name { get; set; } = string.Empty;
	public string Type { get; set; } = string.Empty;
	public string Value { get; set; } = string.Empty;
	public object? RawValue { get; set; }
}
