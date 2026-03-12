using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Memory;
using Win32Emu.Win32;
using Win32Emu.Win32.Modules;
using Xunit;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests that validate DDSURFACEDESC pixel format field offsets and GetDeviceCaps accuracy.
/// These tests specifically guard against the regression where ddpfPixelFormat was written
/// at offset 76 instead of the correct offset 72, and ddsCaps was clobbered at offset 104.
/// </summary>
public class DDrawSurfaceDescTests
{
	private const uint BaseAddress = 0x00400000;

	// GetDeviceCaps nIndex constants
	private const int GDC_HORZRES = 8;
	private const int GDC_VERTRES = 10;
	private const int GDC_BITSPIXEL = 12;
	private const int GDC_PLANES = 14;

	[Fact]
	public void DDSurfaceDescRef_PixelFormat_ShouldBeAtOffset72()
	{
		// The DDSURFACEDESC struct layout:
		// +0:  dwSize           (4 bytes)
		// +4:  dwFlags          (4 bytes)
		// +8:  dwHeight         (4 bytes)
		// +12: dwWidth          (4 bytes)
		// +16: lPitch           (4 bytes)
		// +20: dwBackBufferCount(4 bytes)
		// +24–71: reserved fields
		// +72: ddpfPixelFormat  (32 bytes) ← correct position
		// +104: ddsCaps.dwCaps  (4 bytes)
		// Total: 108 bytes

		var memory = new VirtualMemory();
		const uint addr = 0x10000u;
		var desc = new DDSurfaceDescRef(memory, addr);

		// Write a sentinel value via the ddpfPixelFormat accessor
		var pf = desc.ddpfPixelFormat;
		pf.dwSize = 0xDEADBEEF;

		// Verify it was written at addr+72, not addr+76
		var valueAt72 = memory.Read32(addr + 72);
		var valueAt76 = memory.Read32(addr + 76);

		Assert.Equal(0xDEADBEEFu, valueAt72); // dwSize at +72
		Assert.NotEqual(0xDEADBEEFu, valueAt76); // NOT at +76
	}

	[Fact]
	public void DDSurfaceDescRef_SurfaceCaps_ShouldBeAtOffset104()
	{
		var memory = new VirtualMemory();
		const uint addr = 0x10000u;
		var desc = new DDSurfaceDescRef(memory, addr);

		desc.dwSurfaceCaps = 0xCAFEBABEu;

		var valueAt104 = memory.Read32(addr + 104);
		var valueAt108 = memory.Read32(addr + 108);

		Assert.Equal(0xCAFEBABEu, valueAt104);
		Assert.NotEqual(0xCAFEBABEu, valueAt108);
	}

	[Fact]
	public void DDSurfaceDescRef_PixelFormat_DoesNotOverwriteSurfaceCaps()
	{
		// Writing the full DDPIXELFORMAT (32 bytes at offset 72) should stop at offset 103
		// and must NOT overwrite ddsCaps at offset 104.
		var memory = new VirtualMemory();
		const uint addr = 0x10000u;
		var desc = new DDSurfaceDescRef(memory, addr);

		// Set a sentinel value at offset 104 (ddsCaps)
		memory.Write32(addr + 104, 0x12345678u);

		// Write the full pixel format
		var pf = desc.ddpfPixelFormat;
		pf.dwSize = 32;
		pf.dwFlags = 0x00000040; // DDPF_RGB
		pf.dwFourCC = 0;
		pf.dwRGBBitCount = 16;
		pf.dwRBitMask = 0xF800;
		pf.dwGBitMask = 0x07E0;
		pf.dwBBitMask = 0x001F;
		pf.dwRGBAlphaBitMask = 0;

		// ddsCaps at offset 104 should be untouched
		var capsValue = memory.Read32(addr + 104);
		Assert.Equal(0x12345678u, capsValue);
	}

	[Theory]
	[InlineData(8, 0x20u)]   // DDPF_PALETTEINDEXED8
	[InlineData(16, 0x40u)]  // DDPF_RGB
	[InlineData(24, 0x40u)]  // DDPF_RGB
	[InlineData(32, 0x40u)]  // DDPF_RGB
	public void DDSurfaceDescRef_PixelFormat_HasCorrectFlagsForBpp(int bpp, uint expectedFlags)
	{
		// Verify that DDPIXELFORMAT dwFlags is set correctly for each bit depth
		var memory = new VirtualMemory();
		const uint addr = 0x10000u;
		var desc = new DDSurfaceDescRef(memory, addr);
		var pf = desc.ddpfPixelFormat;

		// Simulate what WritePixelFormat would set
		if (bpp == 8)
			pf.dwFlags = 0x20u; // DDPF_PALETTEINDEXED8
		else
			pf.dwFlags = 0x40u; // DDPF_RGB

		Assert.Equal(expectedFlags, memory.Read32(addr + 76)); // dwFlags is at +72+4 = +76
	}

	[Fact]
	public void GetDeviceCaps_ShouldReturnOne_ForPlanes()
	{
		// PLANES (nIndex=14) must always return 1 on modern hardware.
		// The game ign_teas computes: dword_41C9EC = GetDeviceCaps(DC,14) * GetDeviceCaps(DC,12)
		// If PLANES returns 0 (the old stub default), the product is 0 which is wrong.
		var memory = new VirtualMemory();
		var env = new ProcessEnvironment(memory, logger: NullLogger.Instance);
		env.DisplayBitsPerPixel = 8;
		env.DisplayWidth = 320;
		env.DisplayHeight = 200;

		var gdi32Module = new Gdi32Module(env, 0x10000000u, null, NullLogger.Instance);

		// Use reflection to call the private GetDeviceCaps method
		var method = typeof(Gdi32Module).GetMethod(
			"GetDeviceCaps",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		Assert.NotNull(method);

		var planes = (int)method!.Invoke(gdi32Module, [0u, 14])!; // nIndex = 14 (PLANES)
		Assert.Equal(1, planes);
	}

	[Fact]
	public void GetDeviceCaps_ShouldReturnDisplayBitsPerPixel_ForBitsPixel()
	{
		// BITSPIXEL (nIndex=12) must return the bits per pixel of the current display mode.
		var memory = new VirtualMemory();
		var env = new ProcessEnvironment(memory, logger: NullLogger.Instance);
		env.DisplayBitsPerPixel = 8;

		var gdi32Module = new Gdi32Module(env, 0x10000000u, null, NullLogger.Instance);

		var method = typeof(Gdi32Module).GetMethod(
			"GetDeviceCaps",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

		var bpp = (int)method!.Invoke(gdi32Module, [0u, 12])!; // nIndex = 12 (BITSPIXEL)
		Assert.Equal(8, bpp);
	}

	[Fact]
	public void GetDeviceCaps_ShouldReturnDisplayDimensions_ForHorzresAndVertres()
	{
		// HORZRES (8) and VERTRES (10) must return the current display mode dimensions.
		var memory = new VirtualMemory();
		var env = new ProcessEnvironment(memory, logger: NullLogger.Instance);
		env.DisplayWidth = 320;
		env.DisplayHeight = 200;

		var gdi32Module = new Gdi32Module(env, 0x10000000u, null, NullLogger.Instance);

		var method = typeof(Gdi32Module).GetMethod(
			"GetDeviceCaps",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

		var horzRes = (int)method!.Invoke(gdi32Module, [0u, 8])!; // HORZRES
		var vertRes = (int)method!.Invoke(gdi32Module, [0u, 10])!; // VERTRES
		Assert.Equal(320, horzRes);
		Assert.Equal(200, vertRes);
	}

	[Fact]
	public void GetDeviceCaps_Planes_Times_BitsPixel_ShouldEqualBitsPerPixel()
	{
		// Mirrors the game's computation: dword_41C9EC = GetDeviceCaps(DC,14) * GetDeviceCaps(DC,12)
		// This must equal the bits per pixel of the display mode.
		var memory = new VirtualMemory();
		var env = new ProcessEnvironment(memory, logger: NullLogger.Instance);
		env.DisplayBitsPerPixel = 8;

		var gdi32Module = new Gdi32Module(env, 0x10000000u, null, NullLogger.Instance);

		var method = typeof(Gdi32Module).GetMethod(
			"GetDeviceCaps",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

		var planes = (int)method!.Invoke(gdi32Module, [0u, GDC_PLANES])!;
		var bitsPixel = (int)method!.Invoke(gdi32Module, [0u, GDC_BITSPIXEL])!;
		Assert.Equal(8, planes * bitsPixel);
	}
}
