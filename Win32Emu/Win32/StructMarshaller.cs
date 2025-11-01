using Win32Emu.Memory;

namespace Win32Emu.Win32;

/// <summary>
/// Provides methods for reading and writing Win32 structures to/from emulated memory.
/// </summary>
public static class StructMarshaller
{
	/// <summary>
	/// Reads a WNDCLASSA structure from memory.
	/// </summary>
	public static NativeTypes.WNDCLASSA ReadWNDCLASSA(VirtualMemory memory, uint address)
	{
		return new NativeTypes.WNDCLASSA
		{
			style = memory.Read32(address + 0),
			lpfnWndProc = memory.Read32(address + 4),
			cbClsExtra = (int)memory.Read32(address + 8),
			cbWndExtra = (int)memory.Read32(address + 12),
			hInstance = memory.Read32(address + 16),
			hIcon = memory.Read32(address + 20),
			hCursor = memory.Read32(address + 24),
			hbrBackground = memory.Read32(address + 28),
			lpszMenuName = memory.Read32(address + 32),
			lpszClassName = memory.Read32(address + 36)
		};
	}

	/// <summary>
	/// Reads a WNDCLASSEXA structure from memory.
	/// </summary>
	public static NativeTypes.WNDCLASSEXA ReadWNDCLASSEXA(VirtualMemory memory, uint address)
	{
		return new NativeTypes.WNDCLASSEXA
		{
			cbSize = memory.Read32(address + 0),
			style = memory.Read32(address + 4),
			lpfnWndProc = memory.Read32(address + 8),
			cbClsExtra = (int)memory.Read32(address + 12),
			cbWndExtra = (int)memory.Read32(address + 16),
			hInstance = memory.Read32(address + 20),
			hIcon = memory.Read32(address + 24),
			hCursor = memory.Read32(address + 28),
			hbrBackground = memory.Read32(address + 32),
			lpszMenuName = memory.Read32(address + 36),
			lpszClassName = memory.Read32(address + 40),
			hIconSm = memory.Read32(address + 44)
		};
	}

	/// <summary>
	/// Writes a MSG structure to memory.
	/// </summary>
	public static void WriteMSG(VirtualMemory memory, uint address, NativeTypes.MSG msg)
	{
		memory.Write32(address + 0, msg.hwnd);
		memory.Write32(address + 4, msg.message);
		memory.Write32(address + 8, msg.wParam);
		memory.Write32(address + 12, msg.lParam);
		memory.Write32(address + 16, msg.time);
		memory.Write32(address + 20, (uint)msg.ptX);
		memory.Write32(address + 24, (uint)msg.ptY);
	}

	/// <summary>
	/// Reads a MSG structure from memory.
	/// </summary>
	public static NativeTypes.MSG ReadMSG(VirtualMemory memory, uint address)
	{
		return new NativeTypes.MSG
		{
			hwnd = memory.Read32(address + 0),
			message = memory.Read32(address + 4),
			wParam = memory.Read32(address + 8),
			lParam = memory.Read32(address + 12),
			time = memory.Read32(address + 16),
			ptX = (int)memory.Read32(address + 20),
			ptY = (int)memory.Read32(address + 24)
		};
	}

	/// <summary>
	/// Reads a POINT structure from memory.
	/// </summary>
	public static NativeTypes.POINT ReadPOINT(VirtualMemory memory, uint address)
	{
		return new NativeTypes.POINT
		{
			x = (int)memory.Read32(address + 0),
			y = (int)memory.Read32(address + 4)
		};
	}

	/// <summary>
	/// Writes a POINT structure to memory.
	/// </summary>
	public static void WritePOINT(VirtualMemory memory, uint address, NativeTypes.POINT point)
	{
		memory.Write32(address + 0, (uint)point.x);
		memory.Write32(address + 4, (uint)point.y);
	}

	/// <summary>
	/// Reads a RECT structure from memory.
	/// </summary>
	public static NativeTypes.RECT ReadRECT(VirtualMemory memory, uint address)
	{
		return new NativeTypes.RECT
		{
			left = (int)memory.Read32(address + 0),
			top = (int)memory.Read32(address + 4),
			right = (int)memory.Read32(address + 8),
			bottom = (int)memory.Read32(address + 12)
		};
	}

	/// <summary>
	/// Writes a RECT structure to memory.
	/// </summary>
	public static void WriteRECT(VirtualMemory memory, uint address, NativeTypes.RECT rect)
	{
		memory.Write32(address + 0, (uint)rect.left);
		memory.Write32(address + 4, (uint)rect.top);
		memory.Write32(address + 8, (uint)rect.right);
		memory.Write32(address + 12, (uint)rect.bottom);
	}

	/// <summary>
	/// Reads a PAINTSTRUCT structure from memory.
	/// </summary>
	public static NativeTypes.PAINTSTRUCT ReadPAINTSTRUCT(VirtualMemory memory, uint address)
	{
		var ps = new NativeTypes.PAINTSTRUCT
		{
			hdc = memory.Read32(address + 0),
			fErase = memory.Read32(address + 4),
			rcPaintLeft = (int)memory.Read32(address + 8),
			rcPaintTop = (int)memory.Read32(address + 12),
			rcPaintRight = (int)memory.Read32(address + 16),
			rcPaintBottom = (int)memory.Read32(address + 20),
			fRestore = memory.Read32(address + 24),
			fIncUpdate = memory.Read32(address + 28)
		};

		// Read reserved bytes (32 bytes starting at offset 32)
		unsafe
		{
			for (int i = 0; i < 32; i++)
			{
				ps.rgbReserved[i] = memory.Read8(address + 32 + (uint)i);
			}
		}

		return ps;
	}

	/// <summary>
	/// Writes a PAINTSTRUCT structure to memory.
	/// </summary>
	public static void WritePAINTSTRUCT(VirtualMemory memory, uint address, NativeTypes.PAINTSTRUCT ps)
	{
		memory.Write32(address + 0, ps.hdc);
		memory.Write32(address + 4, ps.fErase);
		memory.Write32(address + 8, (uint)ps.rcPaintLeft);
		memory.Write32(address + 12, (uint)ps.rcPaintTop);
		memory.Write32(address + 16, (uint)ps.rcPaintRight);
		memory.Write32(address + 20, (uint)ps.rcPaintBottom);
		memory.Write32(address + 24, ps.fRestore);
		memory.Write32(address + 28, ps.fIncUpdate);

		// Write reserved bytes (32 bytes starting at offset 32)
		unsafe
		{
			for (int i = 0; i < 32; i++)
			{
				memory.Write8(address + 32 + (uint)i, ps.rgbReserved[i]);
			}
		}
	}

	/// <summary>
	/// Reads a DOCINFOA structure from memory.
	/// </summary>
	public static NativeTypes.DOCINFOA ReadDOCINFOA(VirtualMemory memory, uint address)
	{
		return new NativeTypes.DOCINFOA
		{
			cbSize = (int)memory.Read32(address + 0),
			lpszDocName = memory.Read32(address + 4),
			lpszOutput = memory.Read32(address + 8),
			lpszDatatype = memory.Read32(address + 12),
			fwType = memory.Read32(address + 16)
		};
	}
}
