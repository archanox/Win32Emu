using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Gui.ViewModels;
using DiscUtils.Registry;
using Win32EmuRegistryHive = Win32Emu.Win32.Registry.RegistryHive;

namespace Win32Emu.Tests.Gui;

/// <summary>
/// Tests for RegistryViewerViewModel
/// </summary>
public class RegistryViewerViewModelTests : IDisposable
{
	private readonly Win32EmuRegistryHive _registryHive;

	public RegistryViewerViewModelTests()
	{
		// Create a test registry hive (without VFS for simplicity)
		_registryHive = new Win32EmuRegistryHive(null, NullLogger.Instance);
		
		// Create some test keys
		SetupTestRegistry();
	}

	private void SetupTestRegistry()
	{
		// Create HKEY_CURRENT_USER\Environment with some values
		var envKey = _registryHive.CreateKey("HKEY_CURRENT_USER\\Environment");
		_registryHive.SetValue(envKey, "TEST_VAR", "test_value", RegistryValueType.String);
		_registryHive.CloseKey(envKey);

		// Create HKEY_CURRENT_USER\Software (parent key for TestApp)
		var softwareKey = _registryHive.CreateKey("HKEY_CURRENT_USER\\Software");
		_registryHive.CloseKey(softwareKey);

		// Create HKEY_CURRENT_USER\Software\TestApp
		var testAppKey = _registryHive.CreateKey("HKEY_CURRENT_USER\\Software\\TestApp");
		_registryHive.SetValue(testAppKey, "Version", "1.0", RegistryValueType.String);
		_registryHive.CloseKey(testAppKey);
	}

	[Fact]
	public void Constructor_InitializesRootKeys()
	{
		// Act
		var viewModel = new RegistryViewerViewModel(_registryHive, NullLogger.Instance);

		// Assert
		Assert.NotNull(viewModel.RootKeys);
		Assert.Equal(4, viewModel.RootKeys.Count);
		Assert.Contains(viewModel.RootKeys, k => k.Name == "HKEY_LOCAL_MACHINE");
		Assert.Contains(viewModel.RootKeys, k => k.Name == "HKEY_CURRENT_USER");
		Assert.Contains(viewModel.RootKeys, k => k.Name == "HKEY_CLASSES_ROOT");
		Assert.Contains(viewModel.RootKeys, k => k.Name == "HKEY_USERS");
	}

	[Fact]
	public void RootKeys_InitiallyLoadSubKeys()
	{
		// Act
		var viewModel = new RegistryViewerViewModel(_registryHive, NullLogger.Instance);
		var hkcu = viewModel.RootKeys.First(k => k.Name == "HKEY_CURRENT_USER");

		// Assert - Should have actual children, not just "Loading..."
		Assert.NotEmpty(hkcu.Children);
		Assert.DoesNotContain(hkcu.Children, c => c.Name == "Loading...");
		Assert.Contains(hkcu.Children, c => c.Name == "Environment");
		Assert.Contains(hkcu.Children, c => c.Name == "Software");
	}

	[Fact]
	public void ExpandingNode_LoadsSubKeys_AndReplacesLoadingPlaceholder()
	{
		// Arrange
		var viewModel = new RegistryViewerViewModel(_registryHive, NullLogger.Instance);
		var hkcu = viewModel.RootKeys.First(k => k.Name == "HKEY_CURRENT_USER");
		var softwareNode = hkcu.Children.First(c => c.Name == "Software");

		// Verify initial state - should have "Loading..." placeholder
		Assert.Single(softwareNode.Children);
		Assert.Equal("Loading...", softwareNode.Children[0].Name);

		// Act - Expand the node
		softwareNode.IsExpanded = true;

		// Assert - "Loading..." should be replaced with actual children
		Assert.NotEmpty(softwareNode.Children);
		Assert.DoesNotContain(softwareNode.Children, c => c.Name == "Loading...");
		Assert.Contains(softwareNode.Children, c => c.Name == "TestApp");
	}

	[Fact]
	public void ExpandingNodeTwice_DoesNotReloadChildren()
	{
		// Arrange
		var viewModel = new RegistryViewerViewModel(_registryHive, NullLogger.Instance);
		var hkcu = viewModel.RootKeys.First(k => k.Name == "HKEY_CURRENT_USER");
		var softwareNode = hkcu.Children.First(c => c.Name == "Software");

		// Act - Expand the node twice
		softwareNode.IsExpanded = true;
		var firstChildCount = softwareNode.Children.Count;
		
		softwareNode.IsExpanded = false; // Collapse
		softwareNode.IsExpanded = true;  // Expand again

		// Assert - Should not reload, children count should remain the same
		Assert.Equal(firstChildCount, softwareNode.Children.Count);
		Assert.DoesNotContain(softwareNode.Children, c => c.Name == "Loading...");
	}

	[Fact]
	public void SelectedKey_LoadsValues()
	{
		// Arrange
		var viewModel = new RegistryViewerViewModel(_registryHive, NullLogger.Instance);
		var hkcu = viewModel.RootKeys.First(k => k.Name == "HKEY_CURRENT_USER");
		var envNode = hkcu.Children.First(c => c.Name == "Environment");

		// Act
		viewModel.SelectedKey = envNode;

		// Assert
		Assert.NotEmpty(viewModel.Values);
		Assert.Contains(viewModel.Values, v => v.Name == "TEST_VAR");
	}

	[Fact]
	public void EmptyKey_DoesNotShowLoadingPlaceholder()
	{
		// Arrange
		var viewModel = new RegistryViewerViewModel(_registryHive, NullLogger.Instance);
		var hkcu = viewModel.RootKeys.First(k => k.Name == "HKEY_CURRENT_USER");
		var envNode = hkcu.Children.First(c => c.Name == "Environment");

		// Act - Expand a leaf node (no children)
		envNode.IsExpanded = true;

		// Assert - Should have no children, not even "Loading..."
		Assert.Empty(envNode.Children);
	}

	public void Dispose()
	{
		// Clean up registry hive
		_registryHive.Dispose();
		GC.SuppressFinalize(this);
	}
}
