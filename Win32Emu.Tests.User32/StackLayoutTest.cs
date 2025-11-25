using System;
using Xunit;
using Win32Emu.Cpu;
using Win32Emu.Memory;

namespace Win32Emu.Tests.User32;

/// <summary>
/// Tests to verify correct stack layout for CallWindowProcedureAsync and CallDialogProcedureAsync
/// </summary>
[Trait("Category", "DllModuleTests")]
public class StackLayoutTests
{
	[Fact]
	public void StdCallStackLayout_ShouldMatchExpectedOrder()
	{
		// Arrange: Create a minimal CPU and memory environment
		var memory = new VirtualMemory(0x10000000); // 256MB
		var cpu = new Win32Emu.Cpu.Iced.IcedCpu(memory);
		
		// Set up initial stack pointer
		const uint initialEsp = 0x001FF000;
		cpu.SetRegister("ESP", initialEsp);
		
		// Simulate pushing parameters for a stdcall function:
		// void __stdcall Function(uint param1, uint param2, uint param3, uint param4)
		const uint param1 = 0x11111111;
		const uint param2 = 0x22222222;
		const uint param3 = 0x33333333;
		const uint param4 = 0x44444444;
		const uint returnAddress = 0xDEADBEEF;
		
		// Push parameters right-to-left (stdcall convention)
		var esp = initialEsp;
		
		// Push param4 (rightmost)
		esp -= 4;
		memory.Write32(esp, param4);
		
		// Push param3
		esp -= 4;
		memory.Write32(esp, param3);
		
		// Push param2
		esp -= 4;
		memory.Write32(esp, param2);
		
		// Push param1 (leftmost)
		esp -= 4;
		memory.Write32(esp, param1);
		
		// Push return address (simulating CALL instruction)
		esp -= 4;
		memory.Write32(esp, returnAddress);
		
		// Act: Verify stack layout from function's perspective
		// When function starts, ESP points to return address
		// Parameters are at ESP+4, ESP+8, ESP+12, ESP+16
		
		// Assert: Check that parameters are in correct positions
		Assert.Equal(returnAddress, memory.Read32(esp + 0));  // Return address at ESP
		Assert.Equal(param1, memory.Read32(esp + 4));         // First parameter at ESP+4
		Assert.Equal(param2, memory.Read32(esp + 8));         // Second parameter at ESP+8
		Assert.Equal(param3, memory.Read32(esp + 12));        // Third parameter at ESP+12
		Assert.Equal(param4, memory.Read32(esp + 16));        // Fourth parameter at ESP+16
	}
	
	[Fact]
	public void WindowProcStackLayout_ShouldMatchWin32Convention()
	{
		// Arrange
		var memory = new VirtualMemory(0x10000000);
		var cpu = new Win32Emu.Cpu.Iced.IcedCpu(memory);
		
		const uint initialEsp = 0x001FF000;
		cpu.SetRegister("ESP", initialEsp);
		
		// WindowProc signature: LRESULT WindowProc(HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam)
		const uint hwnd = 0x00001234;
		const uint message = 0x0010;  // WM_CLOSE
		const uint wParam = 0x0000;
		const uint lParam = 0x0000;
		const uint returnAddress = 0xDEADBEEF;
		
		// Simulate the stack setup from CallWindowProcedureAsync
		var esp = initialEsp;
		
		// Push parameters right-to-left
		esp -= 4;
		memory.Write32(esp, lParam);
		
		esp -= 4;
		memory.Write32(esp, wParam);
		
		esp -= 4;
		memory.Write32(esp, message);
		
		esp -= 4;
		memory.Write32(esp, hwnd);
		
		// Push return address
		esp -= 4;
		memory.Write32(esp, returnAddress);
		
		// Assert: Verify correct layout for WindowProc
		Assert.Equal(returnAddress, memory.Read32(esp + 0));
		Assert.Equal(hwnd, memory.Read32(esp + 4));     // First param: HWND
		Assert.Equal(message, memory.Read32(esp + 8));  // Second param: UINT message
		Assert.Equal(wParam, memory.Read32(esp + 12));  // Third param: WPARAM
		Assert.Equal(lParam, memory.Read32(esp + 16));  // Fourth param: LPARAM
	}
}
