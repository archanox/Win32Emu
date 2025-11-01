// EXAMPLE: How to use the new ref struct wrappers for seamless memory access
// 
// The ref struct wrappers provide automatic memory read/write via properties,
// eliminating the need for explicit StructMarshaller.Read/Write calls.

namespace Win32Emu.Win32.Examples;

using Win32Emu.Memory;

public class StructWrapperExamples
{
	private VirtualMemory _memory = null!;

	// BEFORE: Manual approach with StructMarshaller
	public void OldWay_RegisterClassA(uint lpWndClass)
	{
		// Had to explicitly read the entire struct from memory
		var wndClass = StructMarshaller.ReadWNDCLASSA(_memory, lpWndClass);
		
		// Access fields from the value struct
		uint style = wndClass.style;
		uint wndProc = wndClass.lpfnWndProc;
		
		// If we needed to modify and write back:
		// wndClass.style = 0x0001;
		// StructMarshaller.WriteWNDCLASSA(_memory, lpWndClass, wndClass);
	}

	// AFTER: Seamless approach with ref struct wrapper
	public void NewWay_RegisterClassA(uint lpWndClass)
	{
		// Create a ref struct wrapper - no explicit read needed
		var wndClass = new WndClassARef(_memory, lpWndClass);
		
		// Property access automatically reads from memory
		uint style = wndClass.style;
		uint wndProc = wndClass.lpfnWndProc;
		
		// Property assignment automatically writes to memory - no explicit write call needed!
		wndClass.style = 0x0001;
		// The change is immediately written to memory at lpWndClass
	}

	// EXAMPLE: Writing to a MSG structure
	public void NewWay_GetMessageA(uint lpMsg)
	{
		// Create ref struct wrapper
		var msg = new MsgRef(_memory, lpMsg);
		
		// All property assignments automatically write to memory
		msg.hwnd = 0;
		msg.message = 0x0012; // WM_QUIT
		msg.wParam = 0;
		msg.lParam = 0;
		msg.time = 0;
		msg.ptX = 0;
		msg.ptY = 0;
		
		// No need for StructMarshaller.WriteMSG() - it's already written!
	}

	// EXAMPLE: Working with StackArgs (even cleaner!)
	public void UsingStackArgs(Cpu.ICpu cpu)
	{
		var args = new StackArgs(cpu, _memory);
		
		// Get a WNDCLASSA from stack argument 0
		var wndClass = args.WndClassA(0);
		
		// Access properties - automatic memory read
		uint style = wndClass.style;
		
		// Modify properties - automatic memory write
		wndClass.style = 0x0001;
		
		// Get a MSG from stack argument 1
		var msg = args.Msg(1);
		msg.hwnd = 0x1234;
		msg.message = 0x0010; // WM_CLOSE
		
		// Everything is automatically synchronized with memory!
	}

	// EXAMPLE: Reading a RECT
	public void WorkingWithRect(uint lpRect)
	{
		var rect = new RectRef(_memory, lpRect);
		
		// Read coordinates
		int width = rect.right - rect.left;
		int height = rect.bottom - rect.top;
		
		// Modify coordinates - automatically written to memory
		rect.left -= 10;
		rect.top -= 10;
		rect.right += 10;
		rect.bottom += 10;
		
		// No explicit write call needed!
	}

	// NEW: Implicit cast to value struct
	public void ImplicitCastExample(uint lpWndClass)
	{
		// Create ref struct wrapper
		var wndClassRef = new WndClassARef(_memory, lpWndClass);
		
		// Implicit cast to value struct - creates a snapshot
		NativeTypes.WNDCLASSA snapshot = wndClassRef;
		
		// Can also pass directly where value struct is expected
		ProcessValueStruct(wndClassRef); // Implicitly casts to NativeTypes.WNDCLASSA
		
		// This is cleaner than calling .ToStruct() explicitly
		// var snapshot = wndClassRef.ToStruct(); // Old way (still available)
	}

	private void ProcessValueStruct(NativeTypes.WNDCLASSA wndClass)
	{
		// Work with the value struct snapshot
		uint style = wndClass.style;
	}
}
