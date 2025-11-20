using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Win32Emu.VirtualFileSystem;

namespace Win32Emu.Gui.Views;

/// <summary>
/// Avalonia dialog for browsing folders in the Virtual File System.
/// </summary>
public partial class FolderBrowserDialog : Window
{
	private readonly IVirtualFileSystem? _vfs;
	private string? _selectedPath;
	private readonly string _rootPath;
	private readonly TaskCompletionSource<string?> _resultTcs = new();

	public FolderBrowserDialog()
	{
		InitializeComponent();
		_rootPath = "C:\\";
	}

	public FolderBrowserDialog(IVirtualFileSystem? vfs, string? title = null, string? rootPath = null)
	{
		InitializeComponent();
		_vfs = vfs;
		_rootPath = rootPath ?? "C:\\";

		// Set title if provided
		if (!string.IsNullOrEmpty(title))
		{
			var titleText = this.FindControl<TextBlock>("TitleText");
			if (titleText != null)
			{
				titleText.Text = title;
			}
		}

		// Set up event handlers
		var okButton = this.FindControl<Button>("OkButton");
		if (okButton != null)
		{
			okButton.Click += OkButton_Click;
		}

		var cancelButton = this.FindControl<Button>("CancelButton");
		if (cancelButton != null)
		{
			cancelButton.Click += CancelButton_Click;
		}

		var treeView = this.FindControl<TreeView>("FolderTreeView");
		if (treeView != null)
		{
			treeView.SelectionChanged += TreeView_SelectionChanged;
		}

		// Populate tree view
		PopulateTreeView();
	}

	private void PopulateTreeView()
	{
		var treeView = this.FindControl<TreeView>("FolderTreeView");
		if (treeView == null) return;

		// Create root node
		var rootNode = new FolderTreeItem
		{
			Path = _rootPath,
			DisplayName = _rootPath,
			IsExpanded = true
		};

		// Add child directories
		PopulateChildren(rootNode);

		treeView.ItemsSource = new[] { rootNode };

		// Select root by default
		treeView.SelectedItem = rootNode;
		_selectedPath = _rootPath;
		UpdateSelectedPathDisplay();
	}

	private void PopulateChildren(FolderTreeItem parent)
	{
		if (_vfs == null) return;

		try
		{
			// Get all files/directories in this path
			var entries = _vfs.GetFiles(parent.Path, "*");

			// Group entries to find directories
			var directories = new HashSet<string>();
			
			foreach (var entry in entries)
			{
				// Extract directory from full path
				var relativePath = entry.Substring(parent.Path.Length).TrimStart('\\');
				var firstSlash = relativePath.IndexOf('\\');
				
				if (firstSlash > 0)
				{
					// This is a subdirectory entry
					var dirName = relativePath.Substring(0, firstSlash);
					directories.Add(dirName);
				}
			}

			// Also check if directories exist using DirectoryExists
			// Common Windows directories
			var commonDirs = new[] { "Program Files", "Program Files (x86)", "Windows", "Users", "ProgramData", "Temp" };
			foreach (var dir in commonDirs)
			{
				var fullPath = System.IO.Path.Combine(parent.Path, dir);
				if (_vfs.DirectoryExists(fullPath))
				{
					directories.Add(dir);
				}
			}

			// Create tree items for each directory
			foreach (var dirName in directories.OrderBy(d => d))
			{
				var fullPath = System.IO.Path.Combine(parent.Path, dirName);
				var childNode = new FolderTreeItem
				{
					Path = fullPath,
					DisplayName = dirName,
					IsExpanded = false
				};

				// Add placeholder to show expand arrow
				childNode.Children.Add(new FolderTreeItem { Path = "", DisplayName = "Loading..." });

				parent.Children.Add(childNode);
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error populating children for {parent.Path}: {ex.Message}");
		}
	}

	private void TreeView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		var treeView = sender as TreeView;
		if (treeView?.SelectedItem is FolderTreeItem item)
		{
			_selectedPath = item.Path;
			UpdateSelectedPathDisplay();

			// Lazy load children if not yet loaded
			if (item.Children.Count == 1 && item.Children[0].DisplayName == "Loading...")
			{
				item.Children.Clear();
				PopulateChildren(item);
			}
		}
	}

	private void UpdateSelectedPathDisplay()
	{
		var textBox = this.FindControl<TextBox>("SelectedPathTextBox");
		if (textBox != null)
		{
			textBox.Text = _selectedPath ?? "";
		}
	}

	private void OkButton_Click(object? sender, RoutedEventArgs e)
	{
		Close(_selectedPath);
	}

	private void CancelButton_Click(object? sender, RoutedEventArgs e)
	{
		Close(null);
	}

	/// <summary>
	/// Shows the dialog modally and returns the selected path, or null if cancelled.
	/// </summary>
	public async Task<string?> ShowDialogAsync(Window? owner)
	{
		if (owner != null)
		{
			return await ShowDialog<string?>(owner);
		}
		else
		{
			Show();
			return await _resultTcs.Task;
		}
	}

	protected override void OnClosed(EventArgs e)
	{
		base.OnClosed(e);
		_resultTcs.TrySetResult(_selectedPath);
	}
}

/// <summary>
/// Tree item representing a folder in the VFS.
/// </summary>
public class FolderTreeItem
{
	public required string Path { get; set; }
	public required string DisplayName { get; set; }
	public bool IsExpanded { get; set; }
	public ObservableCollection<FolderTreeItem> Children { get; set; } = new();
}
