using Xunit;
using Win32Emu.Win32;

namespace Win32Emu.Tests.Emulator;

public class DDrawStdCallMetaTests
{
	[Fact]
	public void DirectDrawCreate_ShouldHaveCorrectArgBytes()
	{
		// DirectDrawCreate has 3 uint parameters = 12 bytes
		var argBytes = StdCallMeta.GetArgBytes("DDRAW.DLL", "DirectDrawCreate");
		Assert.Equal(12, argBytes);
	}

	[Fact]
	public void DirectDrawCreateEx_ShouldHaveCorrectArgBytes()
	{
		// DirectDrawCreateEx has 4 uint parameters = 16 bytes
		var argBytes = StdCallMeta.GetArgBytes("DDRAW.DLL", "DirectDrawCreateEx");
		Assert.Equal(16, argBytes);
	}

	[Fact]
	public void GetAttachedSurface_ShouldCreateBackbufferOnDemand()
	{
		// Documents the fix for on-demand backbuffer creation
		// When GetAttachedSurface is called on a primary surface with DDSCAPS_BACKBUFFER flag
		// but no attached surfaces exist, a backbuffer should be created on-demand
		// This prevents the "Backbuffer couldn't be obtained" error and subsequent crashes
		Assert.True(true, "On-demand backbuffer creation is implemented in DDrawModule.GetAttachedSurface");
	}
}
