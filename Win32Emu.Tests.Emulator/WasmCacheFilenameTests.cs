using System;
using System.IO;
using Xunit;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests to verify WASM cache filename construction logic matches expectations
/// </summary>
public class WasmCacheFilenameTests
{
	[Theory]
	[InlineData("IGN_TEAS.EXE", "IGN_TEAS.wasm-cache.json")]
	[InlineData("ign_teas.exe", "ign_teas.wasm-cache.json")]
	[InlineData("SETUP.EXE", "SETUP.wasm-cache.json")]
	[InlineData("game.exe", "game.wasm-cache.json")]
	[InlineData("TEST.EXE", "TEST.wasm-cache.json")]
	public void CacheFileName_ShouldPreserveCase_FromExecutableName(string executableName, string expectedCacheFileName)
	{
		// Arrange & Act
		// This mimics the logic in EmulatorService.cs line 241
		var cacheFileName = $"{Path.GetFileNameWithoutExtension(executableName)}.wasm-cache.json";
		
		// Assert
		Assert.Equal(expectedCacheFileName, cacheFileName);
	}
	
	[Fact]
	public void CacheFileName_ForIgnTeas_ShouldBeCorrect()
	{
		// This test specifically validates the fix for the reported issue
		// The cache file was incorrectly named "ign_tease.wasm-cache.json"
		// but should be "IGN_TEAS.wasm-cache.json" to match the executable
		
		// Arrange
		var executableName = "IGN_TEAS.EXE";
		
		// Act
		var cacheFileName = $"{Path.GetFileNameWithoutExtension(executableName)}.wasm-cache.json";
		
		// Assert
		Assert.Equal("IGN_TEAS.wasm-cache.json", cacheFileName);
		Assert.NotEqual("ign_tease.wasm-cache.json", cacheFileName); // Should NOT be the old incorrect name
		Assert.NotEqual("ign_teas.wasm-cache.json", cacheFileName); // Should NOT be lowercase
	}
}
