namespace Win32Emu.NeParser
{
	/// <summary>
	/// Represents a parsed NE (New Executable) file.
	/// </summary>
	public class NeExecutable
	{
	/// <summary>
	/// The NE header information.
	/// </summary>
	public required NeHeader Header { get; init; }
	
	/// <summary>
	/// Array of segments in the executable.
	/// </summary>
	public required NeSegment[] Segments { get; init; }
	
	/// <summary>
	/// Dictionary of entry points by ordinal.
	/// </summary>
	public required Dictionary<ushort, NeEntryPoint> EntryPoints { get; init; }
	
	/// <summary>
	/// Dictionary of resident names to ordinals.
	/// </summary>
	public required Dictionary<string, ushort> ResidentNames { get; init; }
	
	/// <summary>
	/// Dictionary of non-resident names to ordinals.
	/// </summary>
	public required Dictionary<string, ushort> NonResidentNames { get; init; }
	
	/// <summary>
	/// List of imported module names.
	/// </summary>
	public required List<string> ImportModules { get; init; }
	
	/// <summary>
	/// Dictionary mapping module names to their imported functions.
	/// </summary>
	public required Dictionary<string, List<NeImportedFunction>> Imports { get; init; }
}

/// <summary>
/// NE (New Executable) header structure.
/// Based on MS-DOS Encyclopedia Appendix K and https://www.fileformat.info/format/exe/corion-ne.htm
/// </summary>
public class NeHeader
{
	public ushort Signature { get; init; }                    // 0x00-0x01: "NE" signature (0x454E)
	public byte MajorLinkerVersion { get; init; }             // 0x02: Linker major version
	public byte MinorLinkerVersion { get; init; }             // 0x03: Linker minor version (revision)
	public ushort EntryTableOffset { get; init; }             // 0x04-0x05: Offset to entry table (relative to NE header)
	public ushort EntryTableLength { get; init; }             // 0x06-0x07: Length of entry table in bytes
	public uint CrcChecksum { get; init; }                    // 0x08-0x0B: File load CRC checksum
	public ushort ProgramFlags { get; init; }                 // 0x0C-0x0D: Module flag word (DGROUP type, DLL flag, etc.)
	public ushort AutoDataSegment { get; init; }              // 0x0E-0x0F: Automatic data segment number (DGROUP)
	public ushort InitHeapSize { get; init; }                 // 0x10-0x11: Initial local heap size
	public ushort InitStackSize { get; init; }                // 0x12-0x13: Initial stack size
	public ushort EntryPointOffset { get; init; }             // 0x14-0x15: Entry point offset (IP register)
	public ushort EntryPointSegment { get; init; }            // 0x16-0x17: Entry point segment (CS register, 1-based index)
	public ushort InitStackPointer { get; init; }             // 0x18-0x19: Initial stack pointer (SP register)
	public ushort InitStackSegment { get; init; }             // 0x1A-0x1B: Initial stack segment (SS register, 1-based index)
	public ushort SegmentCount { get; init; }                 // 0x1C-0x1D: Number of entries in segment table
	public ushort ModuleReferenceCount { get; init; }         // 0x1E-0x1F: Number of entries in module reference table
	public ushort NonResidentNameTableSize { get; init; }     // 0x20-0x21: Size of non-resident names table in bytes
	public ushort SegmentTableOffset { get; init; }           // 0x22-0x23: Offset to segment table (relative to NE header)
	public ushort ResourceTableOffset { get; init; }          // 0x24-0x25: Offset to resource table (relative to NE header)
	public ushort ResidentNameTableOffset { get; init; }      // 0x26-0x27: Offset to resident names table (relative to NE header)
	public ushort ModuleReferenceTableOffset { get; init; }   // 0x28-0x29: Offset to module reference table (relative to NE header)
	public ushort ImportedNamesTableOffset { get; init; }     // 0x2A-0x2B: Offset to imported names table (relative to NE header)
	public uint NonResidentNameTableOffset { get; init; }     // 0x2C-0x2F: Offset to non-resident names table (absolute file offset)
	public ushort MovableEntryCount { get; init; }            // 0x30-0x31: Number of movable entry points
	public ushort SectorAlignmentShift { get; init; }         // 0x32-0x33: Sector alignment shift count (0 = 9 = 512-byte pages)
	public ushort ResourceSegmentCount { get; init; }         // 0x34-0x35: Number of resource table entries
	public byte TargetOS { get; init; }                       // 0x36: Target operating system
	public byte OtherFlags { get; init; }                     // 0x37: Other OS/2 executable flags
	public ushort ReturnThunksOffset { get; init; }           // 0x38-0x39: Offset to return thunks (or start of gang load area)
	public ushort SegmentReferenceThunksOffset { get; init; } // 0x3A-0x3B: Offset to segment reference thunks (or gang load area length)
	public ushort SwapCodeSize { get; init; }                 // 0x3C-0x3D: Minimum code swap area size
	public ushort ExpectedWindowsVersion { get; init; }       // 0x3E-0x3F: Expected Windows version (minor in low byte, major in high)
	public int BaseOffset { get; init; }                      // Base offset of NE header in file
	
	/// <summary>
	/// Gets the application type from the module flag word high byte.
	/// This corresponds to offset 0x0D in the NE header (application flags).
	/// Bits 0-2: Application type (1=full screen, 2=Windows compatible, 3=uses Windows API)
	/// Bit 3: OS/2 family application
	/// Bit 7: Library module (DLL/driver)
	/// </summary>
	public ushort ApplicationType => (ushort)((ProgramFlags >> 8) & 0xFF);
	
	// Computed properties for display
	public string LinkerVersion => $"{MajorLinkerVersion}.{MinorLinkerVersion}";
	public string ExpectedWindowsVersionFormatted => $"{ExpectedWindowsVersion >> 8}.{ExpectedWindowsVersion & 0xFF}";
	public string EntryPointFormatted => $"{EntryPointSegment:X4}:{EntryPointOffset:X4}";
	public string InitialStackFormatted => $"{InitStackSegment:X4}:{InitStackPointer:X4}";
	public string TargetOsName => TargetOS switch
	{
		0x00 => "Unknown",
		0x01 => "OS/2",
		0x02 => "Windows",
		0x03 => "European MS-DOS 4.x",
		0x04 => "Windows/386",
		0x05 => "BOSS (Borland Operating System Services)",
		_ => $"Unknown (0x{TargetOS:X2})"
	};
	public string DGroupType => (ProgramFlags & 0x03) switch
	{
		0 => "None",
		1 => "Single Data",
		2 => "Multiple Data",
		3 => "Reserved",
		_ => "Unknown"
	};
	public List<string> ProgramFlagsList
	{
		get
		{
			var flags = new List<string>();
			if ((ProgramFlags & 0x04) != 0) flags.Add("Global initialization");
			if ((ProgramFlags & 0x08) != 0) flags.Add("Protected mode only");
			if ((ProgramFlags & 0x10) != 0) flags.Add("8086 instructions");
			if ((ProgramFlags & 0x20) != 0) flags.Add("80286 instructions");
			if ((ProgramFlags & 0x40) != 0) flags.Add("80386 instructions");
			if ((ProgramFlags & 0x80) != 0) flags.Add("8087 instructions");
			return flags;
		}
	}
	public List<string> ApplicationFlagsList
	{
		get
		{
			var flags = new List<string>();
			var appType = ApplicationType & 0x03;
			if (appType == 0x01) flags.Add("Full screen (not aware of Windows/P.M. API)");
			else if (appType == 0x02) flags.Add("Compatible with Windows/P.M. API");
			else if (appType == 0x03) flags.Add("Uses Windows/P.M. API");
			
			if ((ApplicationType & 0x08) != 0) flags.Add("First segment has code that loads application");
			if ((ApplicationType & 0x20) != 0) flags.Add("Errors in image/executable");
			if ((ApplicationType & 0x80) != 0) flags.Add("Library module");
			return flags;
		}
	}
}

/// <summary>
/// NE segment table entry.
/// </summary>
public class NeSegment
{
	public int SegmentNumber { get; init; }    // Segment number (1-based)
	public uint FileOffset { get; init; }      // File offset to segment data (shifted)
	public uint Length { get; init; }          // Length of segment in file
	public ushort Flags { get; init; }         // Segment flags
	public ushort MinAllocation { get; init; } // Minimum allocation size in memory
}

/// <summary>
/// NE segment flags.
/// </summary>
[Flags]
public enum NeSegmentFlags : ushort
{
	Data = 0x0001,          // Segment contains data (vs code)
	Allocated = 0x0002,     // Memory allocated for segment
	Loaded = 0x0004,        // Segment is loaded
	Iterated = 0x0008,      // Segment data is iterated (compressed)
	Movable = 0x0010,       // Segment is movable
	Shareable = 0x0020,     // Segment is shareable (pure)
	Preload = 0x0040,       // Segment should be preloaded
	ExecuteOnly = 0x0080,   // Code segment is execute-only
	ReadOnly = 0x0080,      // Data segment is read-only
	HasRelocations = 0x0100,// Segment has relocation data
	Conforming = 0x0200,    // Code segment is conforming
	PrivilegeLevel = 0x0C00,// Privilege level (DPL)
	Discardable = 0x1000,   // Segment is discardable
	Is32Bit = 0x2000,       // 32-bit segment
	Huge = 0x4000,          // Huge segment (>64KB)
}

/// <summary>
/// NE entry point.
/// </summary>
public class NeEntryPoint
{
	public ushort Ordinal { get; init; }  // Ordinal number
	public byte Segment { get; init; }    // Segment number (0 = movable)
	public ushort Offset { get; init; }   // Offset within segment
	public byte Flags { get; init; }      // Entry flags (exported, shared data, etc.)
}

/// <summary>
/// Represents an imported function in a NE (Win16) executable.
/// </summary>
public class NeImportedFunction
{
	public string Name { get; set; } = "";
	public ushort? Ordinal { get; set; }
	public bool ImportedByOrdinal { get; set; }
}

/// <summary>
/// NE program flags (offset 0x0C in NE header).
/// </summary>
[Flags]
public enum NeProgramFlags : ushort
{
	/// <summary>DGROUP type mask (bits 0-1)</summary>
	DGroupTypeMask = 0x0003,
	/// <summary>No DGROUP (DGROUP = NONE)</summary>
	DGroupNone = 0x0000,
	/// <summary>Single DGROUP (DGROUP = GROUP)</summary>
	DGroupSingle = 0x0001,
	/// <summary>Multiple DGROUP (DGROUP = MULTIPLE)</summary>
	DGroupMultiple = 0x0002,
	/// <summary>DGROUP is null (for library)</summary>
	DGroupNull = 0x0003,
	/// <summary>Global initialization required</summary>
	GlobalInit = 0x0004,
	/// <summary>Protected mode only</summary>
	ProtectedModeOnly = 0x0008,
	/// <summary>8086 instructions used</summary>
	Has8086 = 0x0010,
	/// <summary>80286 instructions used</summary>
	Has80286 = 0x0020,
	/// <summary>80386 instructions used</summary>
	Has80386 = 0x0040,
	/// <summary>80x87 (FPU) instructions used</summary>
	HasFpu = 0x0080,
}

/// <summary>
/// NE application type flags (offset 0x0E in NE header).
/// </summary>
[Flags]
public enum NeApplicationType : ushort
{
	/// <summary>Full screen application (not aware of Windows)</summary>
	FullScreen = 0x0001,
	/// <summary>Aware of Windows/Presentation Manager</summary>
	WindowsAware = 0x0002,
	/// <summary>Uses Windows/Presentation Manager API (GUI application)</summary>
	WindowsApi = 0x0003,
	/// <summary>Application type mask (bits 0-1)</summary>
	TypeMask = 0x0003,
	/// <summary>OS/2 family application (runs on OS/2 and DOS)</summary>
	FamilyApp = 0x0008,
	/// <summary>Self-loading application (has self-loading prolog)</summary>
	SelfLoading = 0x0800,
	/// <summary>Linker errors occurred (file may be invalid)</summary>
	LinkerErrors = 0x2000,
	/// <summary>Module is a library (DLL)</summary>
	Library = 0x8000,
}

/// <summary>
/// NE target operating system values (offset 0x38 in NE header).
/// </summary>
public enum NeTargetOS : byte
{
	/// <summary>Unknown operating system</summary>
	Unknown = 0,
	/// <summary>OS/2</summary>
	OS2 = 1,
	/// <summary>Windows</summary>
	Windows = 2,
	/// <summary>DOS 4.x</summary>
	DOS4 = 3,
	/// <summary>Windows 386 (enhanced mode)</summary>
	Windows386 = 4,
	/// <summary>Borland Operating System Services</summary>
	BOSS = 5,
}

/// <summary>
/// NE relocation record.
/// </summary>
public class NeRelocation
{
	public byte SourceType { get; init; }      // Source type (fixup type)
	public byte TargetFlags { get; init; }     // Target flags and type
	public ushort SourceOffset { get; init; }  // Offset within segment to fixup
	public ushort TargetSegment { get; init; } // Target segment (or module index)
	public ushort TargetOffset { get; init; }  // Target offset (or ordinal/name offset)
}

/// <summary>
/// NE relocation source types.
/// </summary>
public enum NeRelocationSourceType : byte
{
	LoByte = 0,         // Low byte fixup
	Selector = 2,       // 16-bit selector fixup
	Pointer32 = 3,      // 32-bit far pointer fixup (seg:off)
	Offset16 = 5,       // 16-bit offset fixup
	Pointer48 = 11,     // 48-bit far pointer fixup (seg:off32)
	Offset32 = 13,      // 32-bit offset fixup
}

/// <summary>
/// NE relocation target types.
/// </summary>
public enum NeRelocationTargetType : byte
{
	InternalRef = 0,    // Internal reference (within this module)
	ImportOrdinal = 1,  // Import by ordinal
	ImportName = 2,     // Import by name
	OsFixup = 3,        // Operating system fixup
	Additive = 4,       // Additive fixup (add, don't replace)
}
}
