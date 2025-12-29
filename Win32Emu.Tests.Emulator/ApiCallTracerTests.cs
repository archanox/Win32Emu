using System.Linq;
using Win32Emu.Diagnostics;
using Xunit;

namespace Win32Emu.Tests.Emulator;

public class ApiCallTracerTests
{
	[Fact]
	public void GetRecentCalls_ReturnsLastEntriesInOrder()
	{
		using var tracer = new ApiCallTracer(enableTracing: true, enableDetailedParameters: false);

		for (var i = 0; i < 50; i++)
		{
			tracer.LogApiCall("Test", $"Func{i}");
		}

		var recent = tracer.GetRecentCalls(10);

		Assert.Equal(10, recent.Count);
		Assert.Equal(Enumerable.Range(41, 10).Select(i => (long)i), recent.Select(r => r.CallNumber));
	}

	[Fact]
	public void GetRecentCalls_WithNonPositiveCount_ReturnsEmpty()
	{
		using var tracer = new ApiCallTracer(enableTracing: true, enableDetailedParameters: false);

		tracer.LogApiCall("Test", "Func");

		Assert.Empty(tracer.GetRecentCalls(0));
		Assert.Empty(tracer.GetRecentCalls(-5));
	}
}
