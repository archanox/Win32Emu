using Win32Emu.Loader;
using Win32Emu.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

public class PeLoaderIntegrationTests
{
	private readonly ITestOutputHelper _output;
	
	public PeLoaderIntegrationTests(ITestOutputHelper output)
	{
		_output = output;
	}
	
	[Fact]
	public void PeLoader_LoadsRealPeFile_WithSectionAndHeapInfo()
	{
		// Arrange
		var vm = new VirtualMemory();
		var loader = new PeImageLoader(vm, NullLogger.Instance);
		var testExe = "./retrowin32/exe/cpp/thread.exe";
		
		// Skip if test file doesn't exist
		if (!System.IO.File.Exists(testExe))
		{
			_output.WriteLine($"Test PE file not found: {testExe} - skipping test");
			return;
		}
		
		// Act
		var image = loader.Load(testExe);
		
		// Assert
		Assert.NotNull(image);
		Assert.True(image.BaseAddress > 0);
		Assert.True(image.EntryPointAddress > 0);
		Assert.NotNull(image.Sections);
		Assert.NotEmpty(image.Sections);
		
		// Check that we have at least one code section
		Assert.NotEmpty(image.CodeSections);
		
		_output.WriteLine($"Successfully loaded PE: {testExe}");
		_output.WriteLine($"  Base: 0x{image.BaseAddress:X8}");
		_output.WriteLine($"  Entry: 0x{image.EntryPointAddress:X8}");
		_output.WriteLine($"  Stack Reserve: 0x{image.SizeOfStackReserve:X8}");
		_output.WriteLine($"  Stack Commit: 0x{image.SizeOfStackCommit:X8}");
		_output.WriteLine($"  Heap Reserve: 0x{image.SizeOfHeapReserve:X8}");
		_output.WriteLine($"  Heap Commit: 0x{image.SizeOfHeapCommit:X8}");
		_output.WriteLine($"  Sections: {image.Sections.Length}");
		_output.WriteLine($"  Code Sections: {image.CodeSections.Count()}");
		_output.WriteLine($"  Data Sections: {image.DataSections.Count()}");
	}
}
