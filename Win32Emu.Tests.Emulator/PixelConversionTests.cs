using Win32Emu.Rendering;

namespace Win32Emu.Tests.Emulator;

public class PixelConversionTests
{
	[Fact]
	public void WriteRgb565ToRgba_ShouldExpandWhiteToFullIntensity()
	{
		var rgbaData = new byte[4];

		PixelConversion.WriteRgb565ToRgba(0xFFFF, rgbaData, 0);

		Assert.Equal(new byte[] { 255, 255, 255, 255 }, rgbaData);
	}

	[Fact]
	public void WriteRgb565ToRgba_ShouldAllowLastValidOffset()
	{
		var rgbaData = new byte[8];

		PixelConversion.WriteRgb565ToRgba(0x001F, rgbaData, 4);

		Assert.Equal(new byte[] { 0, 0, 0, 0, 0, 0, 255, 255 }, rgbaData);
	}

	[Fact]
	public void WriteRgb565ToRgba_ShouldValidateArguments()
	{
		Assert.Throws<ArgumentNullException>(() => PixelConversion.WriteRgb565ToRgba(0xFFFF, null!, 0));
		Assert.Throws<ArgumentOutOfRangeException>(() => PixelConversion.WriteRgb565ToRgba(0xFFFF, new byte[4], -1));
		Assert.Throws<ArgumentOutOfRangeException>(() => PixelConversion.WriteRgb565ToRgba(0xFFFF, new byte[4], 1));
	}
}
