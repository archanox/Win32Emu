using System.Diagnostics.CodeAnalysis;

namespace Win32Emu.Win32
{
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

		// Windows error codes
		public static class Win32Error
		{
			public const uint ERROR_SUCCESS = 0;
			public const uint ERROR_INVALID_FUNCTION = 1;
			public const uint ERROR_FILE_NOT_FOUND = 2;
			public const uint ERROR_INVALID_PARAMETER = 87;
			public const uint ERROR_INSUFFICIENT_BUFFER = 122;
			public const uint ERROR_INVALID_HANDLE = 6;
			public const uint ERROR_PROC_NOT_FOUND = 127;
			public const uint ERROR_MOD_NOT_FOUND = 126;
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
	}
}