using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;
using Win32Emu.Win32;
using Win32Emu.Win32.Modules;
using Win32Emu.Win32.Win16;
using Xunit;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for Win16 thunking layer that converts Win16 API calls to Win32 equivalents.
/// </summary>
public class Win16ThunkingTests
{
	[Fact]
	public void Win16KernelModule_GetVersion_ForwardsToKernel32()
	{
		// Arrange
		var vm = new VirtualMemory(32 * 1024 * 1024);
		var cpu = new IcedCpu(vm);
		var env = new ProcessEnvironment(vm, heapBase: 0x01000000);
		var kernel32 = new Kernel32Module(env, 0x10000000, null, NullLogger.Instance);
		var win16Kernel = new Win16KernelModule(kernel32, NullLogger.Instance);

		// Act
		var result = win16Kernel.TryInvokeUnsafe("GETVERSION", cpu, vm, out var returnValue);

		// Assert
		Assert.True(result, "Win16 KERNEL.GETVERSION should be handled");
		Assert.NotEqual(0u, returnValue); // Should return Windows version
	}

	[Fact]
	public void Win16UserModule_MessageBeep_ForwardsToUser32()
	{
		// Arrange
		var vm = new VirtualMemory(32 * 1024 * 1024);
		var cpu = new IcedCpu(vm);
		var env = new ProcessEnvironment(vm, heapBase: 0x01000000);
		var user32 = new User32Module(env, 0x10000000, null, NullLogger.Instance);
		var win16User = new Win16UserModule(user32, NullLogger.Instance);

		// Set up stack with beep type parameter
		var esp = 0x00100000u;
		cpu.SetRegister("ESP", esp);
		vm.Write32(esp, 0); // MB_OK

		// Act
		var result = win16User.TryInvokeUnsafe("MESSAGEBEEP", cpu, vm, out var returnValue);

		// Assert
		Assert.True(result, "Win16 USER.MESSAGEBEEP should be handled");
	}

	[Fact]
	public void Win16GdiModule_GetDeviceCaps_ForwardsToGdi32()
	{
		// Arrange
		var vm = new VirtualMemory(32 * 1024 * 1024);
		var cpu = new IcedCpu(vm);
		var env = new ProcessEnvironment(vm, heapBase: 0x01000000);
		var gdi32 = new Gdi32Module(env, 0x10000000, null, NullLogger.Instance);
		var win16Gdi = new Win16GdiModule(gdi32, NullLogger.Instance);

		// Set up stack with HDC and capability index
		var esp = 0x00100000u;
		cpu.SetRegister("ESP", esp);
		vm.Write32(esp, 0x1234); // HDC (fake handle)
		vm.Write32(esp + 4, 8);  // BITSPIXEL capability

		// Act
		var result = win16Gdi.TryInvokeUnsafe("GETDEVICECAPS", cpu, vm, out var returnValue);

		// Assert
		Assert.True(result, "Win16 GDI.GETDEVICECAPS should be handled");
		// returnValue would be the bits per pixel (implementation dependent)
	}

	[Fact]
	public void Win16KeyboardModule_GetKeyState_ForwardsToUser32()
	{
		// Arrange
		var vm = new VirtualMemory(32 * 1024 * 1024);
		var cpu = new IcedCpu(vm);
		var env = new ProcessEnvironment(vm, heapBase: 0x01000000);
		var user32 = new User32Module(env, 0x10000000, null, NullLogger.Instance);
		var win16Keyboard = new Win16KeyboardModule(user32, NullLogger.Instance);

		// Set up stack with virtual key code
		var esp = 0x00100000u;
		cpu.SetRegister("ESP", esp);
		vm.Write32(esp, 0x20); // VK_SPACE

		// Act
		var result = win16Keyboard.TryInvokeUnsafe("GETKEYSTATE", cpu, vm, out var returnValue);

		// Assert
		Assert.True(result, "Win16 KEYBOARD.GETKEYSTATE should be handled");
	}

	[Fact]
	public void Win16SystemModule_GetTickCount_ForwardsToKernel32()
	{
		// Arrange
		var vm = new VirtualMemory(32 * 1024 * 1024);
		var cpu = new IcedCpu(vm);
		var env = new ProcessEnvironment(vm, heapBase: 0x01000000);
		var kernel32 = new Kernel32Module(env, 0x10000000, null, NullLogger.Instance);
		var win16System = new Win16SystemModule(kernel32, NullLogger.Instance);

		// Act
		var result = win16System.TryInvokeUnsafe("GETTICKCOUNT", cpu, vm, out var returnValue);

		// Assert
		Assert.True(result, "Win16 SYSTEM.GETTICKCOUNT should be handled");
		// returnValue would be the tick count (always > 0 typically)
	}

	[Fact]
	public void Win16SoundModule_SndPlaySound_ForwardsToWinMM()
	{
		// Arrange
		var vm = new VirtualMemory(32 * 1024 * 1024);
		var cpu = new IcedCpu(vm);
		var env = new ProcessEnvironment(vm, heapBase: 0x01000000);
		var winmm = new WinMmModule(env, 0x10000000, null, NullLogger.Instance);
		var win16Sound = new Win16SoundModule(winmm, NullLogger.Instance);

		// Set up stack with sound name and flags
		var esp = 0x00100000u;
		cpu.SetRegister("ESP", esp);
		vm.Write32(esp, 0); // NULL sound name (stop playing)
		vm.Write32(esp + 4, 0); // Flags

		// Act
		var result = win16Sound.TryInvokeUnsafe("SNDPLAYSOUND", cpu, vm, out var returnValue);

		// Assert
		Assert.True(result, "Win16 SOUND.SNDPLAYSOUND should be handled");
	}

	[Fact]
	public void Win16KernelModule_UnknownFunction_ReturnsFalse()
	{
		// Arrange
		var vm = new VirtualMemory(32 * 1024 * 1024);
		var cpu = new IcedCpu(vm);
		var env = new ProcessEnvironment(vm, heapBase: 0x01000000);
		var kernel32 = new Kernel32Module(env, 0x10000000, null, NullLogger.Instance);
		var win16Kernel = new Win16KernelModule(kernel32, NullLogger.Instance);

		// Act
		var result = win16Kernel.TryInvokeUnsafe("UNKNOWNFUNCTION", cpu, vm, out var returnValue);

		// Assert
		Assert.False(result, "Unknown Win16 function should return false");
		Assert.Equal(0u, returnValue);
	}

	[Fact]
	public void Win16Modules_HaveCorrectNames()
	{
		// Arrange
		var vm = new VirtualMemory(32 * 1024 * 1024);
		var env = new ProcessEnvironment(vm, heapBase: 0x01000000);
		var kernel32 = new Kernel32Module(env, 0x10000000, null, NullLogger.Instance);
		var user32 = new User32Module(env, 0x10000000, null, NullLogger.Instance);
		var gdi32 = new Gdi32Module(env, 0x10000000, null, NullLogger.Instance);
		var winmm = new WinMmModule(env, 0x10000000, null, NullLogger.Instance);

		// Act
		var win16Kernel = new Win16KernelModule(kernel32, NullLogger.Instance);
		var win16User = new Win16UserModule(user32, NullLogger.Instance);
		var win16Gdi = new Win16GdiModule(gdi32, NullLogger.Instance);
		var win16Keyboard = new Win16KeyboardModule(user32, NullLogger.Instance);
		var win16System = new Win16SystemModule(kernel32, NullLogger.Instance);
		var win16Sound = new Win16SoundModule(winmm, NullLogger.Instance);

		// Assert
		Assert.Equal("KERNEL", win16Kernel.Name);
		Assert.Equal("USER", win16User.Name);
		Assert.Equal("GDI", win16Gdi.Name);
		Assert.Equal("KEYBOARD", win16Keyboard.Name);
		Assert.Equal("SYSTEM", win16System.Name);
		Assert.Equal("SOUND", win16Sound.Name);
	}
}
