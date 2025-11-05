using System;
using Win32Emu.Memory;
using Xunit;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for VirtualMemory class to verify Memory&lt;T&gt; and Span&lt;T&gt; functionality
/// </summary>
public class VirtualMemoryTests
{
	[Fact]
	public void ReadBytes_WithSpan_ReadsCorrectData()
	{
		// Arrange
		var mem = new VirtualMemory();
		var testData = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
		mem.WriteBytes(0x1000, testData);
		
		// Act
		Span<byte> result = stackalloc byte[5];
		mem.ReadBytes(0x1000, result);
		
		// Assert
		Assert.Equal(testData, result.ToArray());
	}
	
	[Fact]
	public void ReadBytes_WithSpan_SpanningPages_ReadsCorrectData()
	{
		// Arrange
		var mem = new VirtualMemory();
		
		// Write data that spans two pages (page size is 4KB = 4096 bytes)
		// Write at offset 4094 so it crosses page boundary
		var testData = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };
		mem.WriteBytes(4094, testData);
		
		// Act
		Span<byte> result = stackalloc byte[6];
		mem.ReadBytes(4094, result);
		
		// Assert
		Assert.Equal(testData, result.ToArray());
	}
	
	[Fact]
	public void ReadBytes_UnallocatedPage_ReturnsZeros()
	{
		// Arrange
		var mem = new VirtualMemory();
		
		// Act - read from unallocated memory
		Span<byte> result = stackalloc byte[10];
		mem.ReadBytes(0x5000, result);
		
		// Assert - should all be zeros
		for (int i = 0; i < result.Length; i++)
		{
			Assert.Equal(0, result[i]);
		}
	}
	
	[Fact]
	public void GetMemory_ReturnsCorrectData()
	{
		// Arrange
		var mem = new VirtualMemory();
		var testData = new byte[] { 0x11, 0x22, 0x33, 0x44 };
		mem.WriteBytes(0x2000, testData);
		
		// Act
		var result = mem.GetMemory(0x2000, 4);
		
		// Assert
		Assert.Equal(testData, result.ToArray());
	}
	
	[Fact]
	public void TryGetPageMemory_WithinSinglePage_Succeeds()
	{
		// Arrange
		var mem = new VirtualMemory();
		var testData = new byte[] { 0x01, 0x02, 0x03, 0x04 };
		mem.WriteBytes(0x1000, testData);
		
		// Act
		bool success = mem.TryGetPageMemory(0x1000, 4, out var result);
		
		// Assert
		Assert.True(success);
		Assert.Equal(testData, result.ToArray());
	}
	
	[Fact]
	public void TryGetPageMemory_SpanningPages_Fails()
	{
		// Arrange
		var mem = new VirtualMemory();
		var testData = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD };
		mem.WriteBytes(4094, testData); // Spans page boundary
		
		// Act
		bool success = mem.TryGetPageMemory(4094, 4, out var result);
		
		// Assert
		Assert.False(success);
		Assert.True(result.IsEmpty);
	}
	
	[Fact]
	public void TryGetPageMemory_UnallocatedPage_Fails()
	{
		// Arrange
		var mem = new VirtualMemory();
		
		// Act - try to get memory from unallocated page
		bool success = mem.TryGetPageMemory(0x5000, 4, out var result);
		
		// Assert
		Assert.False(success);
		Assert.True(result.IsEmpty);
	}
	
	[Fact]
	public void GetSpan_BackwardCompatibility_StillWorks()
	{
		// Arrange
		var mem = new VirtualMemory();
		var testData = new byte[] { 0xFF, 0xEE, 0xDD, 0xCC };
		mem.WriteBytes(0x3000, testData);
		
		// Act - use the old GetSpan method for backward compatibility
		var result = mem.GetSpan(0x3000, 4);
		
		// Assert
		Assert.Equal(testData, result);
	}
	
	[Fact]
	public void ReadBytes_PartialPages_ReadsCorrectly()
	{
		// Arrange
		var mem = new VirtualMemory();
		
		// Write data across 3 pages
		var testData = new byte[8200]; // More than 2 pages (4096 * 2 = 8192)
		for (int i = 0; i < testData.Length; i++)
		{
			testData[i] = (byte)(i % 256);
		}
		mem.WriteBytes(100, testData);
		
		// Act
		var result = new byte[8200];
		mem.ReadBytes(100, result);
		
		// Assert
		Assert.Equal(testData, result);
	}
	
	[Fact]
	public void ReadBytes_EmptySpan_DoesNotThrow()
	{
		// Arrange
		var mem = new VirtualMemory();
		
		// Act & Assert - should not throw
		Span<byte> empty = Span<byte>.Empty;
		mem.ReadBytes(0x1000, empty);
	}
}
