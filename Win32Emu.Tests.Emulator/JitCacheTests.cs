using Win32Emu.Cpu.Jit;
using Win32Emu.Memory;
using Xunit;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for JIT cache persistence and precompilation functionality
/// </summary>
public class JitCacheTests
{
	[Fact]
	public void JitCache_ShouldInitializeWithDefaultDirectory()
	{
		// Arrange & Act
		var cache = new JitCache();
		var stats = cache.GetStatistics();
		
		// Assert
		Assert.NotNull(stats);
		Assert.NotNull(stats.CacheDirectory);
		Assert.Equal(0, stats.TotalBlocks);
	}
	
	[Fact]
	public void JitCache_ShouldInitializeWithCustomDirectory()
	{
		// Arrange
		var tempDir = Path.Combine(Path.GetTempPath(), "Win32Emu_Test_" + Guid.NewGuid());
		
		// Act
		var cache = new JitCache(tempDir);
		var stats = cache.GetStatistics();
		
		// Assert
		Assert.Equal(tempDir, stats.CacheDirectory);
		Assert.True(Directory.Exists(tempDir));
		
		// Cleanup
		Directory.Delete(tempDir, true);
	}
	
	[Fact]
	public void AddBlockMetadata_ShouldStoreMetadata()
	{
		// Arrange
		var cache = new JitCache();
		var metadata = new BlockMetadata
		{
			StartAddress = 0x1000,
			InstructionCount = 5,
			ByteLength = 15,
			CodeHash = "ABC123",
			FirstCompiled = DateTime.UtcNow,
			ExecutionCount = 0
		};
		
		// Act
		cache.AddBlockMetadata(0x1000, metadata);
		var retrieved = cache.TryGetBlockMetadata(0x1000, out var result);
		
		// Assert
		Assert.True(retrieved);
		Assert.NotNull(result);
		Assert.Equal(0x1000u, result.StartAddress);
		Assert.Equal(5, result.InstructionCount);
		Assert.Equal("ABC123", result.CodeHash);
	}
	
	[Fact]
	public async Task SaveAndLoadCache_ShouldPersistBlocks()
	{
		// Arrange
		var tempDir = Path.Combine(Path.GetTempPath(), "Win32Emu_Test_" + Guid.NewGuid());
		var cache = new JitCache(tempDir);
		var executablePath = "/test/program.exe";
		
		// Add some blocks
		cache.AddBlockMetadata(0x1000, new BlockMetadata
		{
			StartAddress = 0x1000,
			InstructionCount = 3,
			ByteLength = 10,
			CodeHash = "HASH1",
			FirstCompiled = DateTime.UtcNow
		});
		
		cache.AddBlockMetadata(0x2000, new BlockMetadata
		{
			StartAddress = 0x2000,
			InstructionCount = 7,
			ByteLength = 20,
			CodeHash = "HASH2",
			FirstCompiled = DateTime.UtcNow
		});
		
		// Act - Save
		await cache.SaveCacheAsync(executablePath);
		
		// Create new cache and load
		var newCache = new JitCache(tempDir);
		await newCache.LoadCacheAsync(executablePath);
		
		// Assert
		var stats = newCache.GetStatistics();
		Assert.Equal(2, stats.TotalBlocks);
		Assert.Equal(10, stats.TotalInstructions);
		
		Assert.True(newCache.TryGetBlockMetadata(0x1000, out var block1));
		Assert.NotNull(block1);
		Assert.Equal(3, block1.InstructionCount);
		Assert.Equal("HASH1", block1.CodeHash);
		
		Assert.True(newCache.TryGetBlockMetadata(0x2000, out var block2));
		Assert.NotNull(block2);
		Assert.Equal(7, block2.InstructionCount);
		Assert.Equal("HASH2", block2.CodeHash);
		
		// Cleanup
		Directory.Delete(tempDir, true);
	}
	
	[Fact]
	public void ComputeCodeHash_ShouldBeConsistent()
	{
		// Arrange
		byte[] code1 = { 0x90, 0x90, 0xC3 }; // NOP NOP RET
		byte[] code2 = { 0x90, 0x90, 0xC3 }; // Same code
		byte[] code3 = { 0x90, 0xC3 }; // Different code
		
		// Act
		var hash1 = JitCache.ComputeCodeHash(code1);
		var hash2 = JitCache.ComputeCodeHash(code2);
		var hash3 = JitCache.ComputeCodeHash(code3);
		
		// Assert
		Assert.Equal(hash1, hash2); // Same code should produce same hash
		Assert.NotEqual(hash1, hash3); // Different code should produce different hash
	}
	
	[Fact]
	public void Clear_ShouldRemoveAllBlocks()
	{
		// Arrange
		var cache = new JitCache();
		cache.AddBlockMetadata(0x1000, new BlockMetadata
		{
			StartAddress = 0x1000,
			InstructionCount = 5,
			ByteLength = 15,
			CodeHash = "ABC",
			FirstCompiled = DateTime.UtcNow
		});
		
		// Act
		cache.Clear();
		
		// Assert
		var stats = cache.GetStatistics();
		Assert.Equal(0, stats.TotalBlocks);
		Assert.False(cache.TryGetBlockMetadata(0x1000, out _));
	}
	
	[Fact]
	public async Task JitCpu_ShouldIntegrateWithCache()
	{
		// Arrange
		var tempDir = Path.Combine(Path.GetTempPath(), "Win32Emu_Test_" + Guid.NewGuid());
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, logger: null, cacheDirectory: tempDir);
		var execPath = "/test/program.exe";
		
		// Setup some simple code
		cpu.SetEip(0x1000);
		mem.Write8(0x1000, 0x90); // NOP
		mem.Write8(0x1001, 0x90); // NOP
		mem.Write8(0x1002, 0xC3); // RET
		
		cpu.SetRegister("ESP", 0x10000);
		mem.Write32(0x10000, 0x2000); // Return address
		
		// Act - Set executable path and execute block (which will compile and cache it)
		cpu.SetExecutablePath(execPath);
		await cpu.ExecuteBlockAsync(mem);
		
		// Save cache
		await cpu.SaveCacheAsync();
		
		// Assert - Check cache was created
		var stats = cpu.GetCacheStatistics();
		Assert.True(stats.TotalBlocks > 0);
		
		// Cleanup
		Directory.Delete(tempDir, true);
	}
	
	[Fact]
	public async Task JitCpu_LoadCache_ShouldLoadExistingCache()
	{
		// Arrange
		var tempDir = Path.Combine(Path.GetTempPath(), "Win32Emu_Test_" + Guid.NewGuid());
		var mem = new VirtualMemory(1024 * 1024);
		var execPath = "/test/program.exe";
		
		// Create first CPU and save cache
		var cpu1 = new JitCpu(mem, logger: null, cacheDirectory: tempDir);
		cpu1.SetExecutablePath(execPath);
		
		// Write some code and compile it
		mem.Write8(0x1000, 0x90); // NOP
		mem.Write8(0x1001, 0xC3); // RET
		cpu1.SetEip(0x1000);
		cpu1.SetRegister("ESP", 0x10000);
		mem.Write32(0x10000, 0x2000);
		
		await cpu1.ExecuteBlockAsync(mem);
		await cpu1.SaveCacheAsync();
		
		var stats1 = cpu1.GetCacheStatistics();
		
		// Act - Create new CPU and load cache
		var cpu2 = new JitCpu(mem, logger: null, cacheDirectory: tempDir);
		cpu2.SetExecutablePath(execPath);
		await cpu2.LoadCacheAsync();
		
		var stats2 = cpu2.GetCacheStatistics();
		
		// Assert
		Assert.Equal(stats1.TotalBlocks, stats2.TotalBlocks);
		
		// Cleanup
		Directory.Delete(tempDir, true);
	}
	
	[Fact]
	public async Task GetCacheStatistics_ShouldReturnCorrectCounts()
	{
		// Arrange
		var tempDir = Path.Combine(Path.GetTempPath(), "Win32Emu_Test_" + Guid.NewGuid());
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, logger: null, cacheDirectory: tempDir);
		
		// Act
		var stats = cpu.GetCacheStatistics();
		
		// Assert
		Assert.NotNull(stats);
		Assert.Equal(0, stats.TotalBlocks);
		Assert.Equal(tempDir, stats.CacheDirectory);
		
		// Cleanup
		Directory.Delete(tempDir, true);
	}
}
