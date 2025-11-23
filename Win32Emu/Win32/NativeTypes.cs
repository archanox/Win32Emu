using System.Diagnostics.CodeAnalysis;

namespace Win32Emu.Win32;

public static class NativeTypes
{
	public readonly struct HModule(uint value)
	{
		public readonly uint Value = value;
		public bool IsNull => Value == 0;
		public static implicit operator uint(HModule h) => h.Value;
	}

	public readonly unsafe struct Pvoid(void* v)
	{
		public readonly void* Value = v;
		public static implicit operator void*(Pvoid p) => p.Value;
		public static implicit operator Pvoid(void* v) => new(v);
	}

	public readonly unsafe struct Handle(void* v) : IEquatable<Handle>
	{
		public readonly void* Value = v;
		public static implicit operator void*(Handle h) => h.Value;
		public static implicit operator Handle(void* v) => new(v);

		public override bool Equals([NotNullWhen(true)] object? obj)
		{
			return obj is Handle other && Equals(other);
		}

		public bool Equals(Handle other)
		{
			return Value == other.Value;
		}

		public override int GetHashCode()
		{
			return unchecked((int)(long)Value);
		}

		public static bool operator ==(Handle left, Handle right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Handle left, Handle right)
		{
			return !left.Equals(right);
		}
	}

	public readonly unsafe struct Hinstance(void* v)
	{
		public readonly void* Value = v;
		public static implicit operator void*(Hinstance h) => h.Value;
		public static implicit operator Hinstance(void* v) => new(v);
	}

	// DWORD is a 32-bit unsigned integer
	public struct Dword(uint v)
	{
		public uint Value = v;
		public static implicit operator uint(Dword d) => d.Value;
		public static implicit operator Dword(uint v) => new(v);
	}

	// CPINFO structure for GetCPInfo function
	// Total size: 20 bytes (4 + 2 + 12 + 2 padding)
	public struct Cpinfo
	{
		public uint MaxCharSize;           // Maximum length, in bytes, of a character in the code page
		public unsafe fixed byte DefaultChar[2];  // Default character used when translating to the specific code page
		public unsafe fixed byte LeadByte[12];    // Lead byte ranges for double-byte character sets (DBCS)
	}

	// RTL_CRITICAL_SECTION structure (Windows XP/2000)
	// Total size: 24 bytes
	public struct CriticalSection
	{
		public uint DebugInfo;      // PRTL_CRITICAL_SECTION_DEBUG - offset 0, 4 bytes
		public int LockCount;       // LONG - offset 4, 4 bytes (starts at -1)
		public int RecursionCount;  // LONG - offset 8, 4 bytes (starts at 0)
		public uint OwningThread;   // HANDLE - offset 12, 4 bytes (starts at NULL)
		public uint LockSemaphore;  // HANDLE - offset 16, 4 bytes (starts at NULL)
		public uint SpinCount;      // ULONG_PTR - offset 20, 4 bytes (starts at 0)
	}

	// SYSTEM_INFO structure
	// Total size: 36 bytes (0x24)
	public struct SystemInfo
	{
		public ushort ProcessorArchitecture;  // WORD - offset 0, 2 bytes
		public ushort Reserved;               // WORD - offset 2, 2 bytes
		public uint PageSize;                 // DWORD - offset 4, 4 bytes
		public uint MinimumApplicationAddress; // LPVOID - offset 8, 4 bytes
		public uint MaximumApplicationAddress; // LPVOID - offset 12, 4 bytes
		public uint ActiveProcessorMask;      // DWORD_PTR - offset 16, 4 bytes
		public uint NumberOfProcessors;       // DWORD - offset 20, 4 bytes
		public uint ProcessorType;            // DWORD - offset 24, 4 bytes
		public uint AllocationGranularity;    // DWORD - offset 28, 4 bytes
		public ushort ProcessorLevel;         // WORD - offset 32, 2 bytes
		public ushort ProcessorRevision;      // WORD - offset 34, 2 bytes
	}

	// Pointer to CPINFO structure
	public readonly unsafe struct Lpcpinfo(Cpinfo* v)
	{
		public readonly Cpinfo* Value = v;
		public static implicit operator Cpinfo*(Lpcpinfo p) => p.Value;
		public static implicit operator Lpcpinfo(Cpinfo* v) => new(v);
	}

	// Windows error codes
	public enum Win32Error : uint
	{
		ERROR_SUCCESS = 0,
		ERROR_INVALID_FUNCTION = 1,
		ERROR_FILE_NOT_FOUND = 2,
		ERROR_PATH_NOT_FOUND = 3,
		ERROR_ACCESS_DENIED = 5,
		ERROR_INVALID_HANDLE = 6,
		ERROR_NO_MORE_FILES = 18,
		ERROR_FILE_EXISTS = 80,
		ERROR_INVALID_PARAMETER = 87,
		ERROR_INSUFFICIENT_BUFFER = 122,
		ERROR_MOD_NOT_FOUND = 126,
		ERROR_PROC_NOT_FOUND = 127,
		ERROR_ALREADY_EXISTS = 183,
		ERROR_MORE_DATA = 234,
		ERROR_NO_MORE_ITEMS = 259,
		ERROR_NOT_OWNER = 288,
		ERROR_IO_INCOMPLETE = 996,
		ERROR_IO_PENDING = 997,
		ERROR_RESOURCE_TYPE_NOT_FOUND = 1813
	}

	// Windows BOOL values
	public enum Win32Bool : uint
	{
		FALSE = 0,
		TRUE = 1
	}

	// Win95/98 PDB (Process Database) flags
	[Flags]
	public enum ProcessFlags : uint
	{
		None = 0,
		PDB32_CONSOLE_PROC = 0x01,   // Process has console
		PDB32_FILE_APIS_OEM = 0x02,  // File APIs use OEM character set
		PDB32_DEBUGGED = 0x04        // Process is being debugged
	}

	// Exception handling return values for UnhandledExceptionFilter
	public enum ExceptionHandling : uint
	{
		EXCEPTION_CONTINUE_SEARCH = 0,      // Continue searching for a handler
		EXCEPTION_EXECUTE_HANDLER = 1,      // Terminate the process
		EXCEPTION_CONTINUE_EXECUTION = unchecked((uint)-1) // Continue execution (-1 as uint)
	}
  
	// Windows handle values
	public enum Win32Handle : uint
	{
		NULL = 0,
		INVALID_HANDLE_VALUE = 0xFFFFFFFF
	}

	// GDI32 stock objects
	public enum StockObject
	{
		WHITE_BRUSH = 0,
		LTGRAY_BRUSH = 1,
		GRAY_BRUSH = 2,
		DKGRAY_BRUSH = 3,
		BLACK_BRUSH = 4,
		NULL_BRUSH = 5,
		HOLLOW_BRUSH = NULL_BRUSH,
		WHITE_PEN = 6,
		BLACK_PEN = 7,
		NULL_PEN = 8,
		OEM_FIXED_FONT = 10,
		ANSI_FIXED_FONT = 11,
		ANSI_VAR_FONT = 12,
		SYSTEM_FONT = 13,
		DEVICE_DEFAULT_FONT = 14,
		DEFAULT_PALETTE = 15,
		SYSTEM_FIXED_FONT = 16,
		DEFAULT_GUI_FONT = 17,
		DC_BRUSH = 18,
		DC_PEN = 19
	}

	// User32 window class constants
	[Flags]
	public enum WindowClass : uint
	{
		CS_VREDRAW = 0x0001,
		CS_HREDRAW = 0x0002,
		CS_DBLCLKS = 0x0008,
		CS_OWNDC = 0x0020,
		CS_CLASSDC = 0x0040,
		CS_PARENTDC = 0x0080,
		CS_NOCLOSE = 0x0200,
		CS_SAVEBITS = 0x0800,
		CS_BYTEALIGNCLIENT = 0x1000,
		CS_BYTEALIGNWINDOW = 0x2000,
		CS_GLOBALCLASS = 0x4000
	}

	// Window styles
	[Flags]
	public enum WindowStyle : uint
	{
		WS_OVERLAPPED = 0x00000000,
		WS_TABSTOP = 0x00010000,
		WS_MAXIMIZEBOX = 0x00010000,
		WS_GROUP = 0x00020000,
		WS_MINIMIZEBOX = 0x00020000,
		WS_THICKFRAME = 0x00040000,
		WS_SYSMENU = 0x00080000,
		WS_HSCROLL = 0x00100000,
		WS_VSCROLL = 0x00200000,
		WS_DLGFRAME = 0x00400000,
		WS_BORDER = 0x00800000,
		WS_CAPTION = 0x00C00000,
		WS_MAXIMIZE = 0x01000000,
		WS_CLIPCHILDREN = 0x02000000,
		WS_CLIPSIBLINGS = 0x04000000,
		WS_DISABLED = 0x08000000,
		WS_VISIBLE = 0x10000000,
		WS_MINIMIZE = 0x20000000,
		WS_CHILD = 0x40000000,
		WS_POPUP = 0x80000000,
		WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX
	}

	// Color constants
	public enum ColorConstants
	{
		COLOR_SCROLLBAR = 0,
		COLOR_BACKGROUND = 1,
		COLOR_ACTIVECAPTION = 2,
		COLOR_INACTIVECAPTION = 3,
		COLOR_MENU = 4,
		COLOR_WINDOW = 5,
		COLOR_WINDOWFRAME = 6,
		COLOR_MENUTEXT = 7,
		COLOR_WINDOWTEXT = 8,
		COLOR_CAPTIONTEXT = 9,
		COLOR_ACTIVEBORDER = 10,
		COLOR_INACTIVEBORDER = 11,
		COLOR_APPWORKSPACE = 12,
		COLOR_HIGHLIGHT = 13,
		COLOR_HIGHLIGHTTEXT = 14,
		COLOR_BTNFACE = 15,
		COLOR_BTNSHADOW = 16,
		COLOR_GRAYTEXT = 17,
		COLOR_BTNTEXT = 18,
		COLOR_INACTIVECAPTIONTEXT = 19,
		COLOR_BTNHIGHLIGHT = 20
	}

	// Window Long (GetWindowLongA/SetWindowLongA) indices
	public enum WindowLong
	{
		GWL_USERDATA = -21,    // User data (32-bit value)
		GWL_EXSTYLE = -20,     // Extended window style
		GWL_STYLE = -16,       // Window style
		GWL_ID = -12,          // Control ID (for child windows)
		GWL_HWNDPARENT = -8,   // Parent window handle
		GWL_HINSTANCE = -6,    // Application instance handle
		GWL_WNDPROC = -4       // Window procedure address
	}

	// WNDCLASSA structure (40 bytes)
	// Used with RegisterClassA
	public struct WNDCLASSA
	{
		public uint style;         // Offset 0
		public uint lpfnWndProc;   // Offset 4
		public int cbClsExtra;     // Offset 8
		public int cbWndExtra;     // Offset 12
		public uint hInstance;     // Offset 16
		public uint hIcon;         // Offset 20
		public uint hCursor;       // Offset 24
		public uint hbrBackground; // Offset 28
		public uint lpszMenuName;  // Offset 32 (pointer to string)
		public uint lpszClassName; // Offset 36 (pointer to string)
	}

	// WNDCLASSEXA structure (48 bytes)
	// Used with RegisterClassExA
	public struct WNDCLASSEXA
	{
		public uint cbSize;        // Offset 0
		public uint style;         // Offset 4
		public uint lpfnWndProc;   // Offset 8
		public int cbClsExtra;     // Offset 12
		public int cbWndExtra;     // Offset 16
		public uint hInstance;     // Offset 20
		public uint hIcon;         // Offset 24
		public uint hCursor;       // Offset 28
		public uint hbrBackground; // Offset 32
		public uint lpszMenuName;  // Offset 36 (pointer to string)
		public uint lpszClassName; // Offset 40 (pointer to string)
		public uint hIconSm;       // Offset 44
	}

	// MSG structure (28 bytes)
	// Used with GetMessage, PeekMessage, DispatchMessage
	public struct MSG
	{
		public uint hwnd;      // Offset 0
		public uint message;   // Offset 4
		public uint wParam;    // Offset 8
		public uint lParam;    // Offset 12
		public uint time;      // Offset 16
		public int ptX;        // Offset 20
		public int ptY;        // Offset 24
	}

	// POINT structure (8 bytes)
	public struct POINT
	{
		public int x;  // Offset 0
		public int y;  // Offset 4
	}

	// RECT structure (16 bytes)
	public struct RECT
	{
		public int left;    // Offset 0
		public int top;     // Offset 4
		public int right;   // Offset 8
		public int bottom;  // Offset 12
	}

	// MSGBOXPARAMS structure
	public struct MSGBOXPARAMS
	{
		public uint cbSize;           // Offset 0
		public uint hwndOwner;        // Offset 4
		public uint hInstance;        // Offset 8
		public uint lpszText;         // Offset 12 (pointer to string)
		public uint lpszCaption;      // Offset 16 (pointer to string)
		public uint dwStyle;          // Offset 20
		public uint lpszIcon;         // Offset 24 (pointer to string)
		public uint dwContextHelpId;  // Offset 28
		public uint lpfnMsgBoxCallback; // Offset 32 (pointer to function)
		public uint dwLanguageId;     // Offset 36
	}

	// PAINTSTRUCT structure (64 bytes)
	public struct PAINTSTRUCT
	{
		public uint hdc;            // Offset 0
		public uint fErase;         // Offset 4
		public int rcPaintLeft;     // Offset 8
		public int rcPaintTop;      // Offset 12
		public int rcPaintRight;    // Offset 16
		public int rcPaintBottom;   // Offset 20
		public uint fRestore;       // Offset 24
		public uint fIncUpdate;     // Offset 28
		public unsafe fixed byte rgbReserved[32]; // Offset 32
	}

	// DOCINFO structure (20 bytes)
	// Used with StartDocA in GDI32
	public struct DOCINFOA
	{
		public int cbSize;        // Offset 0
		public uint lpszDocName;  // Offset 4 (pointer to string)
		public uint lpszOutput;   // Offset 8 (pointer to string)
		public uint lpszDatatype; // Offset 12 (pointer to string)
		public uint fwType;       // Offset 16
	}

	// SCROLLINFO structure (28 bytes)
	// Used with SetScrollInfo/GetScrollInfo in User32
	public struct SCROLLINFO
	{
		public uint cbSize;      // Offset 0
		public uint fMask;       // Offset 4
		public int nMin;         // Offset 8
		public int nMax;         // Offset 12
		public uint nPage;       // Offset 16
		public int nPos;         // Offset 20
		public int nTrackPos;    // Offset 24
	}

	/// <summary>
	/// Flags for DDSURFACEDESC dwFlags field
	/// </summary>
	[Flags]
	public enum DDSD : uint
	{
		CAPS = 0x00000001,           // dwCaps field is valid
		HEIGHT = 0x00000002,         // dwHeight field is valid
		WIDTH = 0x00000004,          // dwWidth field is valid
		PITCH = 0x00000008,          // lPitch field is valid
		BACKBUFFERCOUNT = 0x00000020, // dwBackBufferCount field is valid
		ZBUFFERBITDEPTH = 0x00000040, // dwZBufferBitDepth field is valid
		ALPHABITDEPTH = 0x00000080,  // dwAlphaBitDepth field is valid
		LPSURFACE = 0x00000800,      // lpSurface field is valid
		PIXELFORMAT = 0x00001000,    // ddpfPixelFormat field is valid
		CKDESTOVERLAY = 0x00002000,  // ddckCKDestOverlay field is valid
		CKDESTBLT = 0x00004000,      // ddckCKDestBlt field is valid
		CKSRCOVERLAY = 0x00008000,   // ddckCKSrcOverlay field is valid
		CKSRCBLT = 0x00010000,       // ddckCKSrcBlt field is valid
		MIPMAPCOUNT = 0x00020000,    // dwMipMapCount field is valid
		REFRESHRATE = 0x00040000,    // dwRefreshRate field is valid
		LINEARSIZE = 0x00080000,     // dwLinearSize field is valid
		TEXTURESTAGE = 0x00100000,   // dwTextureStage field is valid
		FVF = 0x00200000,            // dwFVF field is valid
		SRCVBHANDLE = 0x00400000,    // dwSrcVBHandle field is valid
		DEPTH = 0x00800000,          // dwDepth field is valid
		ALL = 0x00FFF9EE,            // All fields are valid
	}

	// DDSURFACEDESC structure (108 bytes minimum)
	// Used in DirectDraw for surface description
	public struct DDSURFACEDESC
	{
		public uint dwSize;           // Offset 0
		public DDSD dwFlags;          // Offset 4
		public uint dwHeight;         // Offset 8
		public uint dwWidth;          // Offset 12
		public uint lPitch;           // Offset 16
		public uint dwBackBufferCount;// Offset 20
		// Additional fields exist but these are the most commonly used
		// dwSurfaceCaps is at offset 108
	}

	// DIPROPHEADER structure (16 bytes)
	// Used in DirectInput for property headers
	public struct DIPROPHEADER
	{
		public uint dwSize;       // Offset 0
		public uint dwHeaderSize; // Offset 4
		public uint dwObj;        // Offset 8
		public uint dwHow;        // Offset 12
	}

	// DIDATAFORMAT structure (24 bytes)
	// Used in DirectInput for data format specification
	public struct DIDATAFORMAT
	{
		public uint dwSize;      // Offset 0
		public uint dwObjSize;   // Offset 4
		public uint dwFlags;     // Offset 8
		public uint dwDataSize;  // Offset 12
		public uint dwNumObjs;   // Offset 16
		public uint rgodf;       // Offset 20 (pointer to array)
	}

	// FILETIME structure (8 bytes)
	// 64-bit value representing the number of 100-nanosecond intervals since January 1, 1601 (UTC)
	public struct FILETIME
	{
		public uint dwLowDateTime;  // Offset 0
		public uint dwHighDateTime; // Offset 4
	}

	// SYSTEMTIME structure (16 bytes)
	// Specifies a date and time using individual members for month, day, year, weekday, hour, minute, second, and millisecond
	public struct SYSTEMTIME
	{
		public ushort wYear;         // Offset 0
		public ushort wMonth;        // Offset 2
		public ushort wDayOfWeek;    // Offset 4
		public ushort wDay;          // Offset 6
		public ushort wHour;         // Offset 8
		public ushort wMinute;       // Offset 10
		public ushort wSecond;       // Offset 12
		public ushort wMilliseconds; // Offset 14
	}

	// TIMECAPS structure (8 bytes)
	// Defines the minimum and maximum period values for timer resolution
	public struct TIMECAPS
	{
		public uint wPeriodMin;  // Offset 0 - Minimum period supported (milliseconds)
		public uint wPeriodMax;  // Offset 4 - Maximum period supported (milliseconds)
	}

	// WAVEFORMATEX structure (18 bytes minimum)
	// Defines the format of waveform-audio data
	public struct WAVEFORMATEX
	{
		public ushort wFormatTag;      // Offset 0 - Format type
		public ushort nChannels;       // Offset 2 - Number of channels
		public uint nSamplesPerSec;    // Offset 4 - Sample rate
		public uint nAvgBytesPerSec;   // Offset 8 - For buffer estimation
		public ushort nBlockAlign;     // Offset 12 - Block alignment
		public ushort wBitsPerSample;  // Offset 14 - Bits per sample
		public ushort cbSize;          // Offset 16 - Size of extra format information
	}

	// WAVEHDR structure for wave audio buffers (32 bytes)
	public struct WAVEHDR
	{
		public uint lpData;           // Offset 0 - Pointer to the waveform buffer
		public uint dwBufferLength;   // Offset 4 - Length of the buffer in bytes
		public uint dwBytesRecorded;  // Offset 8 - Bytes recorded (for input)
		public uint dwUser;           // Offset 12 - User data
		public uint dwFlags;          // Offset 16 - Flags
		public uint dwLoops;          // Offset 20 - Number of times to play (for output)
		public uint lpNext;           // Offset 24 - Reserved
		public uint reserved;         // Offset 28 - Reserved
	}

	// WAVEINCAPS structure for wave input device capabilities (52 bytes for ANSI)
	public struct WAVEINCAPSA
	{
		public ushort wMid;                  // Offset 0 - Manufacturer ID
		public ushort wPid;                  // Offset 2 - Product ID
		public uint vDriverVersion;          // Offset 4 - Driver version
		public unsafe fixed byte szPname[32]; // Offset 8 - Product name (32 chars)
		public uint dwFormats;               // Offset 40 - Supported formats
		public ushort wChannels;             // Offset 44 - Number of channels supported
		public ushort wReserved1;            // Offset 46 - Reserved
	}

	// WAVEOUTCAPS structure for wave output device capabilities (52 bytes for ANSI)
	public struct WAVEOUTCAPSA
	{
		public ushort wMid;                  // Offset 0 - Manufacturer ID
		public ushort wPid;                  // Offset 2 - Product ID
		public uint vDriverVersion;          // Offset 4 - Driver version
		public unsafe fixed byte szPname[32]; // Offset 8 - Product name (32 chars)
		public uint dwFormats;               // Offset 40 - Supported formats
		public ushort wChannels;             // Offset 44 - Number of channels supported
		public ushort wReserved1;            // Offset 46 - Padding
		public uint dwSupport;               // Offset 48 - Optional functionality
	}

	// MMTIME structure for multimedia time (12 bytes)
	public struct MMTIME
	{
		public uint wType;     // Offset 0 - Time format type
		public uint u;         // Offset 4 - Time value (union - we use simple uint)
		public uint padding;   // Offset 8 - Padding for union size
	}

	// MIXERLINE structure (168 bytes for ANSI version)
	public struct MIXERLINEA
	{
		public uint cbStruct;                // Offset 0 - Size of structure
		public uint dwDestination;           // Offset 4 - Destination index
		public uint dwSource;                // Offset 8 - Source index
		public uint dwLineID;                // Offset 12 - Line identifier
		public uint fdwLine;                 // Offset 16 - Line flags
		public uint dwUser;                  // Offset 20 - User data
		public uint dwComponentType;         // Offset 24 - Component type
		public uint cChannels;               // Offset 28 - Number of channels
		public uint cConnections;            // Offset 32 - Number of connections
		public uint cControls;               // Offset 36 - Number of controls
		public unsafe fixed byte szShortName[16]; // Offset 40 - Short name
		public unsafe fixed byte szName[64];      // Offset 56 - Full name
		public uint dwType;                  // Offset 120 - Target type
		public uint dwDeviceID;              // Offset 124 - Device ID
		public ushort wMid;                  // Offset 128 - Manufacturer ID
		public ushort wPid;                  // Offset 130 - Product ID
		public uint vDriverVersion;          // Offset 132 - Driver version
		public unsafe fixed byte szPname[32]; // Offset 136 - Product name
	}

	// MIXERCONTROL structure (148 bytes for ANSI version)
	public struct MIXERCONTROLA
	{
		public uint cbStruct;                // Offset 0 - Size of structure
		public uint dwControlID;             // Offset 4 - Control identifier
		public uint dwControlType;           // Offset 8 - Control type
		public uint fdwControl;              // Offset 12 - Control flags
		public uint cMultipleItems;          // Offset 16 - Multiple items count
		public unsafe fixed byte szShortName[16]; // Offset 20 - Short name
		public unsafe fixed byte szName[64];      // Offset 36 - Full name
		public uint lMinimum;                // Offset 100 - Minimum value (signed as uint)
		public uint lMaximum;                // Offset 104 - Maximum value (signed as uint)
		public unsafe fixed uint reserved[10];    // Offset 108 - Reserved (40 bytes)
	}

	// MIXERLINECONTROLS structure (24 bytes for ANSI version)
	public struct MIXERLINECONTROLSA
	{
		public uint cbStruct;           // Offset 0 - Size of structure
		public uint dwLineID;           // Offset 4 - Line identifier
		public uint dwControlID;        // Offset 8 - Control identifier (input)
		public uint dwControlType;      // Offset 12 - Control type (input)
		public uint cControls;          // Offset 16 - Number of controls
		public uint cbmxctrl;           // Offset 20 - Size of MIXERCONTROL structure
		public uint pamxctrl;           // Offset 24 - Pointer to MIXERCONTROL array (should be offset 20, error in comment)
	}

	/// <summary>
	/// DirectSound cooperative level flags
	/// </summary>
	public enum DSSCL : uint
	{
		NORMAL = 0x00000001,       // Normal level - can play, but not change format
		PRIORITY = 0x00000002,     // Priority level - can play and change format
		EXCLUSIVE = 0x00000003,    // Exclusive level - exclusive control of device
		WRITEPRIMARY = 0x00000004  // Write primary - can write directly to primary buffer
	}

	/// <summary>
	/// DirectSound buffer capability flags
	/// </summary>
	[Flags]
	public enum DSBCapsFlags : uint
	{
		PRIMARYBUFFER = 0x00000001,        // Buffer is a primary buffer
		STATIC = 0x00000002,               // Buffer is in system memory
		LOCHARDWARE = 0x00000004,          // Buffer is in hardware memory
		LOCSOFTWARE = 0x00000008,          // Buffer is in software memory
		CTRL3D = 0x00000010,               // Buffer has 3D control
		CTRLFREQUENCY = 0x00000020,        // Buffer has frequency control
		CTRLPAN = 0x00000040,              // Buffer has pan control
		CTRLVOLUME = 0x00000080,           // Buffer has volume control
		CTRLPOSITIONNOTIFY = 0x00000100,   // Buffer has position notify
		CTRLFX = 0x00000200,               // Buffer has effects control
		STICKYFOCUS = 0x00004000,          // Buffer has sticky focus
		GLOBALFOCUS = 0x00008000,          // Buffer has global focus
		GETCURRENTPOSITION2 = 0x00010000,  // More accurate position
		MUTE3DATMAXDISTANCE = 0x00020000,  // Mute 3D at max distance
		LOCDEFER = 0x00040000              // Defer location assignment
	}

	/// <summary>
	/// DSBCAPS structure (20 bytes)
	/// Describes the capabilities of a DirectSound buffer
	/// </summary>
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 20)]
	[GenerateMemoryRef]
	public struct DSBCAPS
	{
		[System.Runtime.InteropServices.FieldOffset(0)]
		public uint dwSize;                    // Offset 0 - Size of structure (20)
		
		[System.Runtime.InteropServices.FieldOffset(4)]
		public uint dwFlags;                   // Offset 4 - Capability flags
		
		[System.Runtime.InteropServices.FieldOffset(8)]
		public uint dwBufferBytes;             // Offset 8 - Size of buffer in bytes
		
		[System.Runtime.InteropServices.FieldOffset(12)]
		public uint dwUnlockTransferRate;      // Offset 12 - Unlock transfer rate (obsolete)
		
		[System.Runtime.InteropServices.FieldOffset(16)]
		public uint dwPlayCpuOverhead;         // Offset 16 - Play CPU overhead (obsolete)
	}

	// DDCOLORKEY structure (8 bytes)
	// Specifies a color key for DirectDraw surfaces
	public struct DDCOLORKEY
	{
		public uint dwColorSpaceLowValue;  // Offset 0 - Low boundary of color space
		public uint dwColorSpaceHighValue; // Offset 4 - High boundary of color space
	}

	// ACMSTREAMHEADER structure (used for ACM audio conversion)
	public struct ACMSTREAMHEADER
	{
		public uint cbStruct;       // Offset 0 - Size of structure
		public uint fdwStatus;      // Offset 4 - Flags
		public uint dwUser;         // Offset 8 - User data
		public uint pbSrc;          // Offset 12 - Source buffer pointer
		public uint cbSrcLength;    // Offset 16 - Source buffer length
		public uint cbSrcLengthUsed;// Offset 20 - Source bytes used
		public uint dwSrcUser;      // Offset 24 - Source user data
		public uint pbDst;          // Offset 28 - Destination buffer pointer
		public uint cbDstLength;    // Offset 32 - Destination buffer length
		public uint cbDstLengthUsed;// Offset 36 - Destination bytes used
		public uint dwDstUser;      // Offset 40 - Destination user data
	}

	// DDPIXELFORMAT structure (32 bytes)
	// Describes the pixel format of a DirectDraw surface
	public struct DDPIXELFORMAT
	{
		public uint dwSize;             // Offset 0 - Size of structure (32)
		public uint dwFlags;            // Offset 4 - Pixel format flags
		public uint dwFourCC;           // Offset 8 - FourCC code
		public uint dwRGBBitCount;      // Offset 12 - RGB bit count
		public uint dwRBitMask;         // Offset 16 - Red bit mask
		public uint dwGBitMask;         // Offset 20 - Green bit mask
		public uint dwBBitMask;         // Offset 24 - Blue bit mask
		public uint dwRGBAlphaBitMask;  // Offset 28 - Alpha bit mask
	}

	// STARTUPINFOA structure (68 bytes)
	// Specifies startup information for a process
	public struct STARTUPINFOA
	{
		public uint cb;              // Offset 0 - Size of structure
		public uint lpReserved;      // Offset 4
		public uint lpDesktop;       // Offset 8
		public uint lpTitle;         // Offset 12
		public uint dwX;             // Offset 16
		public uint dwY;             // Offset 20
		public uint dwXSize;         // Offset 24
		public uint dwYSize;         // Offset 28
		public uint dwXCountChars;   // Offset 32
		public uint dwYCountChars;   // Offset 36
		public uint dwFillAttribute; // Offset 40
		public uint dwFlags;         // Offset 44
		public ushort wShowWindow;   // Offset 48
		public ushort cbReserved2;   // Offset 50
		public uint lpReserved2;     // Offset 52
		public uint hStdInput;       // Offset 56
		public uint hStdOutput;      // Offset 60
		public uint hStdError;       // Offset 64
	}

	// EXCEPTION_POINTERS structure (8 bytes)
	// Contains exception record and context pointers
	public struct EXCEPTION_POINTERS
	{
		public uint ExceptionRecord; // Offset 0 - Pointer to EXCEPTION_RECORD
		public uint ContextRecord;   // Offset 4 - Pointer to CONTEXT
	}

	// Toolhelp32 snapshot flags
	[Flags]
	public enum Th32SnapshotFlags : uint
	{
		TH32CS_SNAPHEAPLIST = 0x00000001,  // Include all heaps of the process in the snapshot
		TH32CS_SNAPPROCESS = 0x00000002,   // Include all processes in the system
		TH32CS_SNAPTHREAD = 0x00000004,    // Include all threads in the system
		TH32CS_SNAPMODULE = 0x00000008,    // Include all modules of the process
		TH32CS_SNAPMODULE32 = 0x00000010,  // Include all 32-bit modules
		TH32CS_SNAPALL = TH32CS_SNAPHEAPLIST | TH32CS_SNAPPROCESS | TH32CS_SNAPTHREAD | TH32CS_SNAPMODULE,
		TH32CS_INHERIT = 0x80000000        // Inherit from parent process
	}

	// PROCESSENTRY32 structure (296 bytes)
	// Describes an entry from a list of the processes residing in the system address space
	public unsafe struct PROCESSENTRY32
	{
		public uint dwSize;              // Offset 0 - Size of the structure (296 bytes)
		public uint cntUsage;            // Offset 4 - Reference count (no longer used, always 0)
		public uint th32ProcessID;       // Offset 8 - Process identifier
		public uint th32DefaultHeapID;   // Offset 12 - Default heap ID (not used)
		public uint th32ModuleID;        // Offset 16 - Module identifier (not used)
		public uint cntThreads;          // Offset 20 - Number of execution threads
		public uint th32ParentProcessID; // Offset 24 - Parent process identifier
		public int pcPriClassBase;       // Offset 28 - Base priority of threads
		public uint dwFlags;             // Offset 32 - Reserved (not used)
		public fixed byte szExeFile[260]; // Offset 36 - Path and filename of executable (MAX_PATH)
		
		public const int Size = 296;
	}

	// THREADENTRY32 structure (28 bytes)
	// Describes an entry from a list of the threads executing in the system
	public struct THREADENTRY32
	{
		public uint dwSize;              // Offset 0 - Size of the structure (28 bytes)
		public uint cntUsage;            // Offset 4 - Reference count (no longer used, always 0)
		public uint th32ThreadID;        // Offset 8 - Thread identifier
		public uint th32OwnerProcessID;  // Offset 12 - Identifier of the process that created the thread
		public int tpBasePri;            // Offset 16 - Base priority level
		public int tpDeltaPri;           // Offset 20 - Delta priority value
		public uint dwFlags;             // Offset 24 - Reserved (not used)
		
		public const int Size = 28;
	}

	// MODULEENTRY32 structure (548 bytes)
	// Describes an entry from a list of the modules belonging to a process
	public unsafe struct MODULEENTRY32
	{
		public uint dwSize;              // Offset 0 - Size of the structure (548 bytes)
		public uint th32ModuleID;        // Offset 4 - Module identifier (not used)
		public uint th32ProcessID;       // Offset 8 - Process identifier
		public uint GlblcntUsage;        // Offset 12 - Global usage count (not used)
		public uint ProccntUsage;        // Offset 16 - Module usage count (not used)
		public uint modBaseAddr;         // Offset 20 - Base address of module
		public uint modBaseSize;         // Offset 24 - Size of module in bytes
		public uint hModule;             // Offset 28 - Module handle
		public fixed byte szModule[256]; // Offset 32 - Module name (MAX_MODULE_NAME32 + 1)
		public fixed byte szExePath[260]; // Offset 288 - Module path (MAX_PATH)
		
		public const int Size = 548;
	}

	// EXCEPTION_RECORD structure (partial - 20 bytes minimum)
	// Describes an exception
	public struct EXCEPTION_RECORD
	{
		public uint ExceptionCode;       // Offset 0
		public uint ExceptionFlags;      // Offset 4
		public uint ExceptionRecord;     // Offset 8 - Pointer to nested record
		public uint ExceptionAddress;    // Offset 12
		public uint NumberParameters;    // Offset 16
		// ExceptionInformation array follows...
	}

	#region DirectDraw Constants and Enums

	/// <summary>
	/// DirectDraw error codes
	/// </summary>
	public enum DDResult : uint
	{
		DD_OK = 0,
		DDERR_GENERIC = 1,
		DDERR_INVALIDPARAMS = 0x80070057,
		DDERR_NOTFOUND = 0x887601C2,
		DDERR_INVALIDOBJECT = 0x88760066,
		DDERR_NOPALETTEATTACHED = 0x88760165,
		DDERR_NOCOLORKEY = 0x88760168,
		DDERR_NOCLIPPERATTACHED = 0x88760169,
		DDERR_NOTAOVERLAYSURFACE = 0x88760177,
		DDERR_SURFACEBUSY = 0x8877000A,
		DDERR_NOTLOCKED = 0x88770010,
		DDERR_SURFACEALREADYATTACHED = 0x88760109,
		DDERR_SURFACENOTATTACHED = 0x88760108,
		DDERR_NOCLIPLIST = 0x887601F6,
		E_NOINTERFACE = 0x80004002,
		CLASS_E_NOAGGREGATION = 0x80040110,
	}

	/// <summary>
	/// DirectDraw surface capabilities flags
	/// Based on Microsoft DirectX SDK and Olde-Skuul DirectDraw headers
	/// Reference: https://learn.microsoft.com/en-us/windows/win32/api/ddraw/ns-ddraw-ddscaps
	/// Reference: https://github.com/apitrace/dxsdk/blob/master/Include/ddraw.h
	/// </summary>
	[Flags]
	public enum DDSCaps : uint
	{
		DDSCAPS_RESERVED1 = 0x00000001,          // Reserved (was DDSCAPS_3DDEVICE in some docs)
		DDSCAPS_ALPHA = 0x00000002,              // Surface contains alpha information only
		DDSCAPS_BACKBUFFER = 0x00000004,         // Back buffer of a surface flipping structure
		DDSCAPS_COMPLEX = 0x00000008,            // Complex surface (more than one surface)
		DDSCAPS_FLIP = 0x00000010,               // Part of a surface flipping structure
		DDSCAPS_FRONTBUFFER = 0x00000020,        // Front buffer of a surface flipping structure
		DDSCAPS_OFFSCREENPLAIN = 0x00000040,     // Offscreen plain surface
		DDSCAPS_OVERLAY = 0x00000080,            // Surface is an overlay
		DDSCAPS_PALETTE = 0x00000100,            // Allows unique DirectDrawPalette objects
		DDSCAPS_PRIMARYSURFACE = 0x00000200,     // Primary surface (what user sees)
		DDSCAPS_SYSTEMMEMORY = 0x00000800,       // Surface memory allocated from system memory
		DDSCAPS_TEXTURE = 0x00001000,            // Can be used as a 3D texture
		DDSCAPS_3DDEVICE = 0x00002000,           // Surface can be used for 3D rendering
		DDSCAPS_VIDEOMEMORY = 0x00004000,        // Surface exists in display memory
		DDSCAPS_VISIBLE = 0x00008000,            // Changes are immediately visible
		DDSCAPS_WRITEONLY = 0x00010000,          // Only write access permitted
		DDSCAPS_ZBUFFER = 0x00020000,            // Z-buffer with depth information
		DDSCAPS_OWNDC = 0x00040000,              // Surface will have a DC association for a long period
		DDSCAPS_LIVEVIDEO = 0x00080000,          // Can receive live video
		DDSCAPS_HWCODEC = 0x00100000,            // Can have stream decompressed to it by hardware
		DDSCAPS_MODEX = 0x00200000,              // 320x200 or 320x240 Mode X surface
		DDSCAPS_MIPMAP = 0x00400000,             // One level of a mipmap
		DDSCAPS_ALLOCONLOAD = 0x04000000,        // Memory allocated on texture load
		DDSCAPS_VIDEOPORT = 0x08000000,          // Can receive data from a video port
		DDSCAPS_LOCALVIDMEM = 0x10000000,        // Surface exists in true local display memory
		DDSCAPS_NONLOCALVIDMEM = 0x20000000,     // Surface exists in non-local display memory
		DDSCAPS_STANDARDVGAMODE = 0x40000000,    // Standard VGA mode surface
		DDSCAPS_OPTIMIZED = 0x80000000,          // Surface is optimized (not currently implemented)
	}

	/// <summary>
	/// DirectDraw general capabilities flags
	/// </summary>
	[Flags]
	public enum DDCaps : uint
	{
		DDCAPS_BLT = 0x00000001,
		DDCAPS_BLTCOLORFILL = 0x00000002,
		DDCAPS_BLTQUEUE = 0x00000004,
		DDCAPS_BLTSTRETCH = 0x00000040,
		DDCAPS_COLORKEY = 0x00000100,
		DDCAPS_GDI = 0x00000800,
		DDCAPS_PALETTE = 0x00002000,
		DDCAPS_PALETTEVSYNC = 0x00010000,
	}

	/// <summary>
	/// DirectDraw extended capabilities flags (caps2)
	/// Based on DirectX 7 SDK and Olde-Skuul DirectDraw headers
	/// </summary>
	[Flags]
	public enum DDCaps2 : uint
	{
		DDCAPS2_CERTIFIED = 0x00000001,              // Driver is certified by Microsoft
		DDCAPS2_CANRENDERWINDOWED = 0x00000040,      // Can render in windowed mode
		DDCAPS2_WIDESURFACES = 0x00000100,           // Supports surfaces wider than primary
		DDCAPS2_CANBOBHARDWARE = 0x00001000,         // Hardware can bob in overlay
		DDCAPS2_FLIPINTERVAL = 0x00200000,           // Supports DDFLIP_INTERVAL flags
		DDCAPS2_FLIPNOVSYNC = 0x00400000,            // Supports DDFLIP_NOVSYNC
		DDCAPS2_CANMANAGETEXTURE = 0x00800000,       // Device can manage textures
		DDCAPS2_TEXMANINNONLOCALVIDMEM = 0x01000000, // Texture manager uses non-local video memory
		DDCAPS2_STEREO = 0x02000000,                 // Stereo driver
		DDCAPS2_SYSTONONLOCAL_AS_SYSTOLOCAL = 0x04000000, // Systolocal blt uses same path as systononlocal
	}

	/// <summary>
	/// DirectDraw color key capabilities flags
	/// </summary>
	[Flags]
	public enum DDCKeyCaps : uint
	{
		DDCKEYCAPS_DESTBLT = 0x00000001,
		DDCKEYCAPS_DESTBLTCLRSPACE = 0x00000002,
		DDCKEYCAPS_SRCBLT = 0x00000010,
		DDCKEYCAPS_SRCBLTCLRSPACE = 0x00000020,
	}

	/// <summary>
	/// DirectDraw FX capabilities flags
	/// </summary>
	[Flags]
	public enum DDFXCaps : uint
	{
		DDFXCAPS_BLTARITHSTRETCHY = 0x00000001,
		DDFXCAPS_BLTARITHSTRETCHYN = 0x00000002,
		DDFXCAPS_BLTMIRRORLEFTRIGHT = 0x00000010,
		DDFXCAPS_BLTMIRRORUPDOWN = 0x00000020,
		DDFXCAPS_BLTROTATION = 0x00000040,
		DDFXCAPS_BLTSHRINKX = 0x00000100,
		DDFXCAPS_BLTSHRINKY = 0x00000400,
		DDFXCAPS_BLTSTRETCHX = 0x00001000,
		DDFXCAPS_BLTSTRETCHY = 0x00004000,
	}

	/// <summary>
	/// DirectDraw palette capabilities flags
	/// Reference: https://doxygen.reactos.org/d7/de9/sdk_2include_2psdk_2ddraw_8h_source.html
	/// </summary>
	[Flags]
	public enum DDPCaps : uint
	{
		DDPCAPS_4BIT = 0x00000001,
		DDPCAPS_8BITENTRIES = 0x00000002,
		DDPCAPS_8BIT = 0x00000004,
		DDPCAPS_INITIALIZE = 0x00000008,
		DDPCAPS_PRIMARYSURFACE = 0x00000010,
		DDPCAPS_PRIMARYSURFACELEFT = 0x00000020,
		DDPCAPS_ALLOW256 = 0x00000040,
		DDPCAPS_VSYNC = 0x00000080,
		DDPCAPS_1BIT = 0x00000100,
		DDPCAPS_2BIT = 0x00000200,
		DDPCAPS_ALPHA = 0x00000400,
	}

	/// <summary>
	/// DirectDraw pixel format flags
	/// </summary>
	[Flags]
	public enum DDPFFlags : uint
	{
		DDPF_PALETTEINDEXED8 = 0x00000020,
		DDPF_RGB = 0x00000040,
	}

	/// <summary>
	/// DirectDraw blt flags
	/// </summary>
	[Flags]
	public enum DDBlt : uint
	{
		DDBLT_COLORFILL = 0x00000400,
		DDBLT_KEYSRC = 0x00008000,
	}

	/// <summary>
	/// DirectDraw surface video capabilities
	/// </summary>
	[Flags]
	public enum DDSVCaps : uint
	{
		DDSVCAPS_RESERVED1 = 0x00000001,
	}

	/// <summary>
	/// DirectDraw surface lock flags
	/// Based on IDirectDrawSurface7::Lock documentation
	/// Reference: https://learn.microsoft.com/en-us/windows/win32/api/ddraw/nf-ddraw-idirectdrawsurface7-lock
	/// </summary>
	[Flags]
	public enum DDLock : uint
	{
		/// <summary>
		/// Default behavior - return a valid memory pointer to the top of the surface
		/// </summary>
		DDLOCK_SURFACEMEMORYPTR = 0x00000000,
		
		/// <summary>
		/// Not currently implemented
		/// </summary>
		DDLOCK_EVENT = 0x00000002,
		
		/// <summary>
		/// Indicates that the surface being locked can only be read
		/// </summary>
		DDLOCK_READONLY = 0x00000010,
		
		/// <summary>
		/// Indicates that the surface being locked is write-enabled
		/// </summary>
		DDLOCK_WRITEONLY = 0x00000020,
		
		/// <summary>
		/// Do not take the Win16Mutex (also known as Win16Lock)
		/// This flag is ignored when locking the primary surface
		/// </summary>
		DDLOCK_NOSYSLOCK = 0x00000800,
		
		/// <summary>
		/// If a lock cannot be obtained because a blit operation is in progress,
		/// Lock retries until a lock is obtained or another error occurs
		/// </summary>
		DDLOCK_WAIT = 0x00001000,
		
		/// <summary>
		/// DirectX 7.0+ - Used only with Direct3D vertex-buffer locks
		/// Indicates that no vertices referred to in a draw operation since the start
		/// of the frame are modified during the lock
		/// </summary>
		DDLOCK_NOOVERWRITE = 0x00001000, // Same value as WAIT - context-dependent
		
		/// <summary>
		/// DirectX 7.0+ - Used only with Direct3D vertex-buffer locks
		/// Indicates that no assumptions are made about the contents of the vertex buffer
		/// This enables Direct3D or the driver to provide an alternative memory area
		/// </summary>
		DDLOCK_DISCARDCONTENTS = 0x00002000,
		
		/// <summary>
		/// Override default DDLOCK_WAIT behavior
		/// If you want to use time when the accelerator is busy (DDERR_WASSTILLDRAWING),
		/// use DDLOCK_DONOTWAIT
		/// </summary>
		DDLOCK_DONOTWAIT = 0x00004000,
		
		/// <summary>
		/// Obsolete - replaced by DDLOCK_DISCARDCONTENTS
		/// </summary>
		DDLOCK_OKTOSWAP = 0x00002000, // Same value as DISCARDCONTENTS
	}

	/// <summary>
	/// DirectDraw extended surface capabilities flags (dwCaps2 in DDSCAPS2)
	/// Based on Microsoft DirectX SDK and Olde-Skuul DirectDraw headers
	/// Reference: https://learn.microsoft.com/en-us/windows/win32/api/ddraw/ns-ddraw-ddscaps2
	/// </summary>
	[Flags]
	public enum DDSCaps2Flags : uint
	{
		DDSCAPS2_HARDWAREDEINTERLACE = 0x00000000,      // Reserved
		DDSCAPS2_HINTDYNAMIC = 0x00000004,              // Surface updated frequently
		DDSCAPS2_HINTSTATIC = 0x00000008,               // Surface updated infrequently
		DDSCAPS2_TEXTUREMANAGE = 0x00000010,            // Managed by driver/D3D
		DDSCAPS2_RESERVED1 = 0x00000020,                // Reserved
		DDSCAPS2_RESERVED2 = 0x00000040,                // Reserved
		DDSCAPS2_OPAQUE = 0x00000080,                   // Never locked/blitted/updated
		DDSCAPS2_HINTANTIALIASING = 0x00000100,         // Surface uses antialiasing
		DDSCAPS2_CUBEMAP = 0x00000200,                  // Cubic environment map
		DDSCAPS2_CUBEMAP_POSITIVEX = 0x00000400,        // +X face of cube map
		DDSCAPS2_CUBEMAP_NEGATIVEX = 0x00000800,        // -X face of cube map
		DDSCAPS2_CUBEMAP_POSITIVEY = 0x00001000,        // +Y face of cube map
		DDSCAPS2_CUBEMAP_NEGATIVEY = 0x00002000,        // -Y face of cube map
		DDSCAPS2_CUBEMAP_POSITIVEZ = 0x00004000,        // +Z face of cube map
		DDSCAPS2_CUBEMAP_NEGATIVEZ = 0x00008000,        // -Z face of cube map
		DDSCAPS2_CUBEMAP_ALLFACES = 0x0000FC00,         // All 6 faces of cube map
		DDSCAPS2_MIPMAPSUBLEVEL = 0x00010000,           // Mipmap sublevel
		DDSCAPS2_D3DTEXTUREMANAGE = 0x00020000,         // Managed by Direct3D
		DDSCAPS2_DONOTPERSIST = 0x00040000,             // Can be safely lost
		DDSCAPS2_STEREOSURFACELEFT = 0x00080000,        // Left stereo surface
		DDSCAPS2_VOLUME = 0x00200000,                   // Volume texture
		DDSCAPS2_NOTUSERLOCKABLE = 0x00400000,          // Surface cannot be locked
		DDSCAPS2_POINTS = 0x00800000,                   // Can render points/point sprites
		DDSCAPS2_RTPATCHES = 0x01000000,                // Can render RT patches
		DDSCAPS2_NPATCHES = 0x02000000,                 // Can render N patches
		DDSCAPS2_RESERVED3 = 0x04000000,                // Reserved
		DDSCAPS2_DISCARDBACKBUFFER = 0x10000000,        // Back buffer preservation not required
		DDSCAPS2_ENABLEALPHACHANNEL = 0x20000000,       // Enable alpha channel
		DDSCAPS2_EXTENDEDFORMATPRIMARY = 0x40000000,    // Non-standard display mode
		DDSCAPS2_ADDITIONALPRIMARY = 0x80000000,        // Additional primary surface
	}

	/// <summary>
	/// DirectDraw additional surface capabilities flags (dwCaps3 in DDSCAPS2)
	/// Reference: https://learn.microsoft.com/en-us/windows/win32/api/ddraw/ns-ddraw-ddscaps2
	/// </summary>
	[Flags]
	public enum DDSCaps3Flags : uint
	{
		DDSCAPS3_MULTISAMPLE_MASK = 0x0000001F,         // Bits 0-4: multisample type
		DDSCAPS3_MULTISAMPLE_QUALITY_MASK = 0x000000E0, // Bits 5-7: multisample quality
		DDSCAPS3_RESERVED1 = 0x00000100,                // Reserved
		DDSCAPS3_VIDEO = 0x00000200,                    // Contains video data
		DDSCAPS3_LIGHTWEIGHTMIPMAP = 0x00000400,        // Has lightweight mip levels
		DDSCAPS3_AUTOGENMIPMAP = 0x00000800,            // Mip sublevels auto-generated
		DDSCAPS3_DMAP = 0x00001000,                     // Displacement map texture
	}

	#endregion

	#region DirectDraw Structures

	/// <summary>
	/// DDSCAPS structure (4 bytes)
	/// Defines the capabilities of a DirectDraw surface object
	/// Reference: https://learn.microsoft.com/en-us/windows/win32/api/ddraw/ns-ddraw-ddscaps
	/// </summary>
	public struct DDSCAPS
	{
		public DDSCaps dwCaps;  // Surface capability flags
	}

	/// <summary>
	/// DDSCAPS2 structure (16 bytes)
	/// Defines additional capabilities of a DirectDraw surface object
	/// Reference: https://learn.microsoft.com/en-us/windows/win32/api/ddraw/ns-ddraw-ddscaps2
	/// </summary>
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct DDSCAPS2
	{
		public DDSCaps dwCaps;           // Base surface capability flags
		public DDSCaps2Flags dwCaps2;    // Extended surface capability flags
		public DDSCaps3Flags dwCaps3;    // Additional surface capability flags
		public uint dwCaps4;             // Volume depth or additional flags
	}

	#endregion

	#region File System

	/// <summary>
	/// MoveFileEx flags
	/// </summary>
	[Flags]
	public enum MoveFileFlags : uint
	{
		MOVEFILE_REPLACE_EXISTING = 0x00000001,
		MOVEFILE_COPY_ALLOWED = 0x00000002,
		MOVEFILE_DELAY_UNTIL_REBOOT = 0x00000004,
		MOVEFILE_WRITE_THROUGH = 0x00000008,
	}

	#endregion

	#region Common Controls

	/// <summary>
	/// Image types for LoadImage
	/// </summary>
	public enum ImageType : uint
	{
		IMAGE_BITMAP = 0,
		IMAGE_ICON = 1,
		IMAGE_CURSOR = 2,
	}

	/// <summary>
	/// LoadImage flags
	/// </summary>
	[Flags]
	public enum LoadImageFlags : uint
	{
		LR_DEFAULTCOLOR = 0x00000000,
		LR_MONOCHROME = 0x00000001,
		LR_COLOR = 0x00000002,
		LR_SHARED = 0x00008000,
	}

	#endregion

	#region Menu

	/// <summary>
	/// Menu item flags
	/// </summary>
	[Flags]
	public enum MenuFlags : uint
	{
		MF_STRING = 0x00000000,
		MF_BITMAP = 0x00000004,
		MF_POPUP = 0x00000010,
		MF_SEPARATOR = 0x00000800,
	}

	/// <summary>
	/// TrackPopupMenu flags
	/// </summary>
	[Flags]
	public enum TrackPopupMenuFlags : uint
	{
		TPM_LEFTBUTTON = 0x0000,
		TPM_RIGHTBUTTON = 0x0002,
		TPM_LEFTALIGN = 0x0000,
		TPM_CENTERALIGN = 0x0004,
		TPM_RIGHTALIGN = 0x0008,
		TPM_RETURNCMD = 0x0100,
	}

	#endregion
}