namespace Win32Emu.Tests.ABExample;

/// <summary>
/// Shared Windows API constants used across A/B tests.
/// Centralizes constant definitions to avoid duplication and ensure consistency.
/// </summary>
public static class Win32Constants
{
	// File access constants
	public const uint GENERIC_READ = 0x80000000;
	public const uint GENERIC_WRITE = 0x40000000;
	
	// File creation disposition constants
	public const uint CREATE_NEW = 1;
	public const uint CREATE_ALWAYS = 2;
	public const uint OPEN_EXISTING = 3;
	public const uint OPEN_ALWAYS = 4;
	public const uint TRUNCATE_EXISTING = 5;
	
	// File attributes
	public const uint FILE_ATTRIBUTE_NORMAL = 0x80;
	public const uint FILE_ATTRIBUTE_READONLY = 0x01;
	public const uint FILE_ATTRIBUTE_HIDDEN = 0x02;
	public const uint FILE_ATTRIBUTE_SYSTEM = 0x04;
	
	// Memory allocation types
	public const uint MEM_COMMIT = 0x1000;
	public const uint MEM_RESERVE = 0x2000;
	public const uint MEM_RESET = 0x80000;
	public const uint MEM_LARGE_PAGES = 0x20000000;
	public const uint MEM_PHYSICAL = 0x00400000;
	public const uint MEM_TOP_DOWN = 0x00100000;
	
	// Memory free types
	public const uint MEM_DECOMMIT = 0x4000;
	public const uint MEM_RELEASE = 0x8000;
	
	// Memory protection constants
	public const uint PAGE_NOACCESS = 0x01;
	public const uint PAGE_READONLY = 0x02;
	public const uint PAGE_READWRITE = 0x04;
	public const uint PAGE_WRITECOPY = 0x08;
	public const uint PAGE_EXECUTE = 0x10;
	public const uint PAGE_EXECUTE_READ = 0x20;
	public const uint PAGE_EXECUTE_READWRITE = 0x40;
	public const uint PAGE_EXECUTE_WRITECOPY = 0x80;
	public const uint PAGE_GUARD = 0x100;
	public const uint PAGE_NOCACHE = 0x200;
	public const uint PAGE_WRITECOMBINE = 0x400;
	
	// Invalid handle value
	public static readonly nint INVALID_HANDLE_VALUE = -1;
}
