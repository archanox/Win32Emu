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
	public static class Win32Error
	{
		public const uint ERROR_SUCCESS = 0;
		public const uint ERROR_INVALID_FUNCTION = 1;
		public const uint ERROR_FILE_NOT_FOUND = 2;
		public const uint ERROR_PATH_NOT_FOUND = 3;
		public const uint ERROR_ACCESS_DENIED = 5;
		public const uint ERROR_INVALID_PARAMETER = 87;
		public const uint ERROR_INSUFFICIENT_BUFFER = 122;
		public const uint ERROR_INVALID_HANDLE = 6;
		public const uint ERROR_PROC_NOT_FOUND = 127;
		public const uint ERROR_MOD_NOT_FOUND = 126;
		public const uint ERROR_ALREADY_EXISTS = 183;
		public const uint ERROR_FILE_EXISTS = 80;
		public const uint ERROR_NOT_OWNER = 288;
	}

	// Windows BOOL values
	public static class Win32Bool
	{
		public const uint FALSE = 0;
		public const uint TRUE = 1;
	}

	// Exception handling return values for UnhandledExceptionFilter
	public static class ExceptionHandling
	{
		public const uint EXCEPTION_EXECUTE_HANDLER = 1;      // Terminate the process
		public const uint EXCEPTION_CONTINUE_SEARCH = 0;      // Continue searching for a handler
		public const uint EXCEPTION_CONTINUE_EXECUTION = unchecked((uint)-1); // Continue execution (-1 as uint)
  }
  
	// Windows handle values
	public static class Win32Handle
	{
		public const uint INVALID_HANDLE_VALUE = 0xFFFFFFFF;
	}

	// GDI32 stock objects
	public static class StockObject
	{
		public const int WHITE_BRUSH = 0;
		public const int LTGRAY_BRUSH = 1;
		public const int GRAY_BRUSH = 2;
		public const int DKGRAY_BRUSH = 3;
		public const int BLACK_BRUSH = 4;
		public const int NULL_BRUSH = 5;
		public const int HOLLOW_BRUSH = NULL_BRUSH;
		public const int WHITE_PEN = 6;
		public const int BLACK_PEN = 7;
		public const int NULL_PEN = 8;
		public const int OEM_FIXED_FONT = 10;
		public const int ANSI_FIXED_FONT = 11;
		public const int ANSI_VAR_FONT = 12;
		public const int SYSTEM_FONT = 13;
		public const int DEVICE_DEFAULT_FONT = 14;
		public const int DEFAULT_PALETTE = 15;
		public const int SYSTEM_FIXED_FONT = 16;
		public const int DEFAULT_GUI_FONT = 17;
		public const int DC_BRUSH = 18;
		public const int DC_PEN = 19;
	}

	// User32 window class constants
	public static class WindowClass
	{
		public const uint CS_VREDRAW = 0x0001;
		public const uint CS_HREDRAW = 0x0002;
		public const uint CS_DBLCLKS = 0x0008;
		public const uint CS_OWNDC = 0x0020;
		public const uint CS_CLASSDC = 0x0040;
		public const uint CS_PARENTDC = 0x0080;
		public const uint CS_NOCLOSE = 0x0200;
		public const uint CS_SAVEBITS = 0x0800;
		public const uint CS_BYTEALIGNCLIENT = 0x1000;
		public const uint CS_BYTEALIGNWINDOW = 0x2000;
		public const uint CS_GLOBALCLASS = 0x4000;
	}

	// Window styles
	public static class WindowStyle
	{
		public const uint WS_OVERLAPPED = 0x00000000;
		public const uint WS_POPUP = 0x80000000;
		public const uint WS_CHILD = 0x40000000;
		public const uint WS_MINIMIZE = 0x20000000;
		public const uint WS_VISIBLE = 0x10000000;
		public const uint WS_DISABLED = 0x08000000;
		public const uint WS_CLIPSIBLINGS = 0x04000000;
		public const uint WS_CLIPCHILDREN = 0x02000000;
		public const uint WS_MAXIMIZE = 0x01000000;
		public const uint WS_CAPTION = 0x00C00000;
		public const uint WS_BORDER = 0x00800000;
		public const uint WS_DLGFRAME = 0x00400000;
		public const uint WS_VSCROLL = 0x00200000;
		public const uint WS_HSCROLL = 0x00100000;
		public const uint WS_SYSMENU = 0x00080000;
		public const uint WS_THICKFRAME = 0x00040000;
		public const uint WS_GROUP = 0x00020000;
		public const uint WS_TABSTOP = 0x00010000;
		public const uint WS_MINIMIZEBOX = 0x00020000;
		public const uint WS_MAXIMIZEBOX = 0x00010000;
		public const uint WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;
	}

	// Color constants
	public static class ColorConstants
	{
		public const int COLOR_SCROLLBAR = 0;
		public const int COLOR_BACKGROUND = 1;
		public const int COLOR_ACTIVECAPTION = 2;
		public const int COLOR_INACTIVECAPTION = 3;
		public const int COLOR_MENU = 4;
		public const int COLOR_WINDOW = 5;
		public const int COLOR_WINDOWFRAME = 6;
		public const int COLOR_MENUTEXT = 7;
		public const int COLOR_WINDOWTEXT = 8;
		public const int COLOR_CAPTIONTEXT = 9;
		public const int COLOR_ACTIVEBORDER = 10;
		public const int COLOR_INACTIVEBORDER = 11;
		public const int COLOR_APPWORKSPACE = 12;
		public const int COLOR_HIGHLIGHT = 13;
		public const int COLOR_HIGHLIGHTTEXT = 14;
		public const int COLOR_BTNFACE = 15;
		public const int COLOR_BTNSHADOW = 16;
		public const int COLOR_GRAYTEXT = 17;
		public const int COLOR_BTNTEXT = 18;
		public const int COLOR_INACTIVECAPTIONTEXT = 19;
		public const int COLOR_BTNHIGHLIGHT = 20;
	}

	// Window Long (GetWindowLongA/SetWindowLongA) indices
	public static class WindowLong
	{
		public const int GWL_WNDPROC = -4;      // Window procedure address
		public const int GWL_HINSTANCE = -6;    // Application instance handle
		public const int GWL_HWNDPARENT = -8;   // Parent window handle
		public const int GWL_ID = -12;          // Control ID (for child windows)
		public const int GWL_STYLE = -16;       // Window style
		public const int GWL_EXSTYLE = -20;     // Extended window style
		public const int GWL_USERDATA = -21;    // User data (32-bit value)
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
}