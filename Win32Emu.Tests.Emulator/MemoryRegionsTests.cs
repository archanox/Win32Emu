using Win32Emu.Memory;
using Xunit;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for MemoryRegions utility class to verify special range detection
/// </summary>
public class MemoryRegionsTests
{
	[Theory]
	[InlineData(0x0D000000, true)]  // Start of COM vtable range
	[InlineData(0x0D800000, true)]  // Middle of COM vtable range
	[InlineData(0x0DFFFFFF, true)]  // End of COM vtable range
	[InlineData(0x0E000000, true)]  // Start of syscall dispatcher range
	[InlineData(0x0E800000, true)]  // Middle of syscall dispatcher range
	[InlineData(0x0EFFFFFF, true)]  // End of syscall dispatcher range
	[InlineData(0x0F000000, true)]  // Start of import hook range
	[InlineData(0x0F800000, true)]  // Middle of import hook range (synthetic exports)
	[InlineData(0x0FFFFFFF, true)]  // End of import hook range
	[InlineData(0x00400000, false)] // Typical PE image base
	[InlineData(0x01000000, false)] // Heap base
	[InlineData(0x10000000, false)] // Beyond special ranges
	[InlineData(0x00000000, false)] // Null pointer
	public void IsInSpecialRange_ShouldIdentifySpecialRanges(uint address, bool expected)
	{
		// Act
		var result = MemoryRegions.IsInSpecialRange(address);
		
		// Assert
		Assert.Equal(expected, result);
	}
	
	[Theory]
	[InlineData(0x0D000000, true)]  // Start of COM vtable range
	[InlineData(0x0DFFFFFF, true)]  // End of COM vtable range
	[InlineData(0x0CFFFFFF, false)] // Just before COM vtable range
	[InlineData(0x0E000000, false)] // Just after COM vtable range
	public void IsInComVtableRange_ShouldIdentifyComVtableRange(uint address, bool expected)
	{
		// Act
		var result = MemoryRegions.IsInComVtableRange(address);
		
		// Assert
		Assert.Equal(expected, result);
	}
	
	[Theory]
	[InlineData(0x0E000000, true)]
	[InlineData(0x0EFFFFFF, true)]
	[InlineData(0x0F000000, false)]
	public void IsInSyscallRange_ShouldIdentifySyscallRange(uint address, bool expected)
	{
		// Act
		var result = MemoryRegions.IsInSyscallRange(address);
		
		// Assert
		Assert.Equal(expected, result);
	}
	
	[Theory]
	[InlineData(0x0F000000, true)]
	[InlineData(0x0FFFFFFF, true)]
	[InlineData(0x10000000, false)]
	public void IsInImportHookRange_ShouldIdentifyImportHookRange(uint address, bool expected)
	{
		// Act
		var result = MemoryRegions.IsInImportHookRange(address);
		
		// Assert
		Assert.Equal(expected, result);
	}
}
