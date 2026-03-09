using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Gui.Backends;

namespace Win32Emu.Tests.Emulator;

public class HeadlessRenderingBackendTests
{
	[Fact]
	public void Convert16BitToRGBA_ShouldExpandRgb565WhiteToFullIntensity()
	{
		using var backend = new HeadlessRenderingBackend(NullLogger.Instance);
		var rgb565Data = new byte[] { 0xFF, 0xFF };

		var rgbaData = backend.Convert16BitToRGBA(rgb565Data, 1, 1, 2);

		Assert.Equal(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, rgbaData);
	}
}
