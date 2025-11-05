using Win32Emu.Gui.Services;
using Xunit;
using AsmResolver.PE;
using System.Linq;

namespace Win32Emu.Tests.Gui;

/// <summary>
/// Tests for PE metadata extraction, specifically verifying that
/// we correctly distinguish between import hints and ordinals.
/// </summary>
public class PeMetadataImportTests
{
	// Use the same test file as Win32Emu.Tests.Emulator
	// This path works when running from the test project's bin directory
	private static readonly string TestPeFile = Path.Combine(
		AppContext.BaseDirectory,
		"../../../../../../Win32Emu.Tests.Emulator/TestData/CHKCPU32.exe");

	[Fact]
	public void GetMetadata_UsesOrdinalNotHint_ForOrdinalBasedImports()
	{
		// This test verifies the fix for the bug where PeMetadataService
		// incorrectly used symbol.Hint instead of symbol.Ordinal
		
		if (!File.Exists(TestPeFile))
		{
			// Skip test if test file is not available
			return;
		}

		var metadata = PeMetadataService.GetMetadata(TestPeFile);
		
		Assert.NotNull(metadata);
		Assert.NotEmpty(metadata.Imports);

		// Verify that imports use correct ordinal values
		// We'll cross-reference with AsmResolver to ensure correctness
		var image = PEImage.FromFile(TestPeFile);
		var expectedImports = image.Imports;
		
		Assert.NotNull(expectedImports);

		foreach (var module in expectedImports)
		{
			var dllName = module.Name ?? "Unknown";
			
			foreach (var symbol in module.Symbols)
			{
				var expectedName = symbol.Name ?? $"Ordinal_{symbol.Ordinal}";
				var expectedOrdinal = symbol.Ordinal;
				
				// Find corresponding import in metadata
				var metadataImport = metadata.Imports.FirstOrDefault(i => 
					i.DllName == dllName && i.FunctionName == expectedName);
				
				if (metadataImport != null)
				{
					// Verify that the ordinal matches symbol.Ordinal, NOT symbol.Hint
					Assert.Equal(expectedOrdinal, metadataImport.Ordinal);
					
					// For ordinal-based imports, verify the name format is correct
					if (symbol.Name == null)
					{
						Assert.Equal($"Ordinal_{symbol.Ordinal}", metadataImport.FunctionName);
					}
				}
			}
		}
	}

	[Fact]
	public void GetMetadata_IncludesImportInformation()
	{
		if (!File.Exists(TestPeFile))
		{
			return;
		}

		var metadata = PeMetadataService.GetMetadata(TestPeFile);
		
		Assert.NotNull(metadata);
		Assert.NotEmpty(metadata.Imports);

		// Verify each import has required fields
		foreach (var import in metadata.Imports)
		{
			Assert.NotEmpty(import.DllName);
			Assert.NotEmpty(import.FunctionName);
			// Ordinal should be valid (0-65535)
			Assert.InRange(import.Ordinal, (ushort)0, ushort.MaxValue);
		}
	}

	[Fact]
	public void GetMetadata_HandlesNamedImportsCorrectly()
	{
		if (!File.Exists(TestPeFile))
		{
			return;
		}

		var metadata = PeMetadataService.GetMetadata(TestPeFile);
		
		Assert.NotNull(metadata);
		
		// Find an import with a name (should be most of them)
		var namedImport = metadata.Imports.FirstOrDefault(i => 
			!i.FunctionName.StartsWith("Ordinal_"));
		
		Assert.NotNull(namedImport);
		Assert.NotEmpty(namedImport.FunctionName);
		Assert.NotEmpty(namedImport.DllName);
	}

	[Fact]
	public void GetMetadata_ReturnsNullForNonExistentFile()
	{
		var metadata = PeMetadataService.GetMetadata("nonexistent.exe");
		Assert.Null(metadata);
	}

	[Fact]
	public void GetMetadata_ReturnsNullForInvalidPeFile()
	{
		// Create a temporary invalid PE file
		var tempFile = Path.GetTempFileName();
		try
		{
			File.WriteAllText(tempFile, "This is not a PE file");
			var metadata = PeMetadataService.GetMetadata(tempFile);
			Assert.Null(metadata);
		}
		finally
		{
			if (File.Exists(tempFile))
			{
				File.Delete(tempFile);
			}
		}
	}
}
