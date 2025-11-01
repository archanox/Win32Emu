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
		ERROR_FILE_EXISTS = 80,
		ERROR_INVALID_PARAMETER = 87,
		ERROR_INSUFFICIENT_BUFFER = 122,
		ERROR_MOD_NOT_FOUND = 126,
		ERROR_PROC_NOT_FOUND = 127,
		ERROR_ALREADY_EXISTS = 183,
		ERROR_NOT_OWNER = 288
	}

	// Windows BOOL values
	public enum Win32Bool : uint
	{
		FALSE = 0,
		TRUE = 1
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

	// DDSURFACEDESC structure (108 bytes minimum)
	// Used in DirectDraw for surface description
	public struct DDSURFACEDESC
	{
		public uint dwSize;           // Offset 0
		public uint dwFlags;          // Offset 4
		public uint dwWidth;          // Offset 8
		public uint dwHeight;         // Offset 12
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
}