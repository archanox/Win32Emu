using Win32Emu.Memory;

namespace Win32Emu.Win32;

/// <summary>
/// Ref struct wrapper for BROWSEINFOA that provides direct memory access via properties.
/// Properties automatically read from and write to the underlying memory address.
/// </summary>
public readonly ref struct BrowseInfoARef
{
	private readonly VirtualMemory _memory;
	private readonly uint _address;

	public BrowseInfoARef(VirtualMemory memory, uint address)
	{
		_memory = memory;
		_address = address;
	}

	public uint Address => _address;

	/// <summary>
	/// Handle of the owner window for the dialog box.
	/// </summary>
	public uint hwndOwner
	{
		get => _memory.Read32(_address + 0);
		set => _memory.Write32(_address + 0, value);
	}

	/// <summary>
	/// Pointer to an item identifier list (PIDL) specifying the location of the root folder.
	/// </summary>
	public uint pidlRoot
	{
		get => _memory.Read32(_address + 4);
		set => _memory.Write32(_address + 4, value);
	}

	/// <summary>
	/// Pointer to a buffer to receive the display name of the folder selected by the user.
	/// </summary>
	public uint pszDisplayName
	{
		get => _memory.Read32(_address + 8);
		set => _memory.Write32(_address + 8, value);
	}

	/// <summary>
	/// Pointer to a null-terminated string that is displayed above the tree view control.
	/// </summary>
	public uint lpszTitle
	{
		get => _memory.Read32(_address + 12);
		set => _memory.Write32(_address + 12, value);
	}

	/// <summary>
	/// Flags that specify the options for the dialog box.
	/// </summary>
	public uint ulFlags
	{
		get => _memory.Read32(_address + 16);
		set => _memory.Write32(_address + 16, value);
	}

	/// <summary>
	/// Pointer to an application-defined function that the dialog box calls when an event occurs.
	/// </summary>
	public uint lpfn
	{
		get => _memory.Read32(_address + 20);
		set => _memory.Write32(_address + 20, value);
	}

	/// <summary>
	/// Application-defined value that the dialog box passes to the callback function.
	/// </summary>
	public uint lParam
	{
		get => _memory.Read32(_address + 24);
		set => _memory.Write32(_address + 24, value);
	}

	/// <summary>
	/// Variable to receive the image associated with the selected folder.
	/// </summary>
	public int iImage
	{
		get => (int)_memory.Read32(_address + 28);
		set => _memory.Write32(_address + 28, (uint)value);
	}
}
