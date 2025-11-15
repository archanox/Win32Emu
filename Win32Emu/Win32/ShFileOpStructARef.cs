using Win32Emu.Memory;

namespace Win32Emu.Win32;

/// <summary>
/// Ref struct wrapper for SHFILEOPSTRUCTA that provides direct memory access via properties.
/// Properties automatically read from and write to the underlying memory address.
/// </summary>
public readonly ref struct ShFileOpStructARef
{
	private readonly VirtualMemory _memory;
	private readonly uint _address;

	public ShFileOpStructARef(VirtualMemory memory, uint address)
	{
		_memory = memory;
		_address = address;
	}

	public uint Address => _address;

	/// <summary>
	/// Handle of the dialog box to display information about the status of the file operation.
	/// </summary>
	public uint hwnd
	{
		get => _memory.Read32(_address + 0);
		set => _memory.Write32(_address + 0, value);
	}

	/// <summary>
	/// Value that indicates which operation to perform (FO_COPY, FO_DELETE, FO_MOVE, FO_RENAME).
	/// </summary>
	public uint wFunc
	{
		get => _memory.Read32(_address + 4);
		set => _memory.Write32(_address + 4, value);
	}

	/// <summary>
	/// Pointer to one or more source file names (null-terminated, double-null terminated list).
	/// </summary>
	public uint pFrom
	{
		get => _memory.Read32(_address + 8);
		set => _memory.Write32(_address + 8, value);
	}

	/// <summary>
	/// Pointer to the destination file or directory name (null-terminated, double-null terminated list).
	/// </summary>
	public uint pTo
	{
		get => _memory.Read32(_address + 12);
		set => _memory.Write32(_address + 12, value);
	}

	/// <summary>
	/// Flags that control the file operation.
	/// </summary>
	public uint fFlags
	{
		get => _memory.Read32(_address + 16);
		set => _memory.Write32(_address + 16, value);
	}

	/// <summary>
	/// Value that receives TRUE if the user aborted any file operations before they were completed.
	/// </summary>
	public uint fAnyOperationsAborted
	{
		get => _memory.Read32(_address + 20);
		set => _memory.Write32(_address + 20, value);
	}

	/// <summary>
	/// Handle to a name mapping object that contains an array of SHNAMEMAPPING structures.
	/// </summary>
	public uint hNameMappings
	{
		get => _memory.Read32(_address + 24);
		set => _memory.Write32(_address + 24, value);
	}

	/// <summary>
	/// Pointer to a null-terminated string used as the title of the progress dialog box.
	/// </summary>
	public uint lpszProgressTitle
	{
		get => _memory.Read32(_address + 28);
		set => _memory.Write32(_address + 28, value);
	}
}
