using System;
using System.IO;
using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Win32Emu.VirtualFileSystem;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for registry persistence to virtual disk
/// </summary>
public class RegistryPersistenceTests : IDisposable
{
	private readonly string _tempVhdPath;
	
	public RegistryPersistenceTests()
	{
		// Create a temporary VHD for testing
		_tempVhdPath = Path.Combine(Path.GetTempPath(), $"test_registry_{Guid.NewGuid()}.vhd");
	}
	
	[Fact]
	public void Registry_ShouldPersistToVirtualDisk()
	{
		// Arrange - Create a VHD and initialize registry with VFS
		const long vhdSize = 50 * 1024 * 1024; // 50 MB
		using (var diskVfs = DiskVirtualFileSystem.Create(_tempVhdPath, DiskFormat.Vhd, vhdSize))
		{
			// Create a test environment with VFS
			using var testEnv = new TestEnvironment();
			testEnv.ProcessEnv.InitializeVirtualFileSystem(diskVfs);
			
			// Act - Set an environment variable which should update the registry
			var testName = "TEST_PERSIST_VAR";
			var testValue = "PersistValue456";
			testEnv.ProcessEnv.SetEnvironmentVariable(testName, testValue);
			
			// Trigger registry save by cleaning up
			testEnv.ProcessEnv.Cleanup();
			
			// Assert - Registry files should exist in VFS
			Assert.True(diskVfs.FileExists(@"C:\Windows\System32\Config\SYSTEM"), 
				"SYSTEM registry hive should exist in VFS");
			Assert.True(diskVfs.FileExists(@"C:\Windows\System32\Config\SOFTWARE"), 
				"SOFTWARE registry hive should exist in VFS");
			Assert.True(diskVfs.FileExists(@"C:\Users\User\NTUSER.DAT"), 
				"NTUSER.DAT registry hive should exist in VFS");
		}
		
		// Clean up the VHD after test
		File.Delete(_tempVhdPath);
	}
	
	[Fact]
	public void Registry_ShouldLoadFromExistingVirtualDisk()
	{
		// Arrange - Create and populate a VHD
		const long vhdSize = 50 * 1024 * 1024; // 50 MB
		const string testName = "TEST_RELOAD_VAR";
		const string testValue = "ReloadValue789";
		
		// Step 1: Create VHD and save registry
		using (var diskVfs = DiskVirtualFileSystem.Create(_tempVhdPath, DiskFormat.Vhd, vhdSize))
		{
			using var testEnv1 = new TestEnvironment();
			testEnv1.ProcessEnv.InitializeVirtualFileSystem(diskVfs);
			
			// Set a value
			testEnv1.ProcessEnv.SetEnvironmentVariable(testName, testValue);
			
			// Save
			testEnv1.ProcessEnv.Cleanup();
		}
		
		// Step 2: Re-open VHD and verify registry is loaded
		using (var diskVfs = new DiskVirtualFileSystem(_tempVhdPath))
		{
			using var testEnv2 = new TestEnvironment();
			testEnv2.ProcessEnv.InitializeVirtualFileSystem(diskVfs);
			
			// Act - Get the environment variable
			var retrievedValue = testEnv2.ProcessEnv.GetEnvironmentVariable(testName);
			
			// Assert - Should retrieve the persisted value
			Assert.Equal(testValue, retrievedValue);
		}
		
		// Clean up
		File.Delete(_tempVhdPath);
	}
	
	[Fact]
	public void Registry_ShouldNotFailWhenVFSNotAvailable()
	{
		// Arrange - Create test environment without VFS
		using var testEnv = new TestEnvironment();
		
		// Act - Set an environment variable (should work in-memory)
		var testName = "TEST_NOMEM_VAR";
		var testValue = "InMemoryValue";
		testEnv.ProcessEnv.SetEnvironmentVariable(testName, testValue);
		
		// Trigger cleanup (should not throw even without VFS)
		var exception = Record.Exception(() => testEnv.ProcessEnv.Cleanup());
		
		// Assert
		Assert.Null(exception);
		
		// Verify the value is still accessible in memory
		var retrievedValue = testEnv.ProcessEnv.GetEnvironmentVariable(testName);
		Assert.Equal(testValue, retrievedValue);
	}
	
	public void Dispose()
	{
		// Clean up temporary VHD if it still exists
		if (File.Exists(_tempVhdPath))
		{
			try
			{
				File.Delete(_tempVhdPath);
			}
			catch
			{
				// Ignore cleanup errors
			}
		}
	}
}
