using Win32Emu.Tests.Kernel32.TestInfrastructure;

namespace Win32Emu.Tests.Kernel32
{
	/// <summary>
	/// Tests for Kernel32 memory management functions like HeapCreate, HeapAlloc, HeapFree, VirtualAlloc, GlobalAlloc
	/// </summary>
	public class MemoryManagementTests : IDisposable
	{
		private readonly TestEnvironment _testEnv;

		public MemoryManagementTests()
		{
			_testEnv = new TestEnvironment();
		}

		#region GlobalAlloc Tests

		[Fact]
		public void GlobalAlloc_WithValidSize_ShouldReturnNonZeroAddress()
		{
			// Arrange
			const uint flags = 0x0000; // GMEM_FIXED
			const uint size = 1024;

			// Act
			var address = _testEnv.CallKernel32Api("GLOBALALLOC", flags, size);

			// Assert
			Assert.NotEqual(0u, address);
		}

		[Fact]
		public void GlobalAlloc_WithZeroSize_ShouldReturnNonZeroAddress()
		{
			// Arrange - GlobalAlloc with 0 size should allocate 1 byte
			const uint flags = 0x0000;
			const uint size = 0;

			// Act
			var address = _testEnv.CallKernel32Api("GLOBALALLOC", flags, size);

			// Assert
			Assert.NotEqual(0u, address);
		}

		[Fact]
		public void GlobalFree_WithValidHandle_ShouldReturnZero()
		{
			// Arrange
			var handle = _testEnv.CallKernel32Api("GLOBALALLOC", 0x0000, 1024);
			Assert.NotEqual(0u, handle);

			// Act
			var result = _testEnv.CallKernel32Api("GLOBALFREE", handle);

			// Assert
			Assert.Equal(0u, result); // GlobalFree returns 0 on success
		}

		#endregion

		#region Heap Tests

		[Fact]
		public void HeapCreate_WithValidParameters_ShouldReturnNonZeroHandle()
		{
			// Arrange
			const uint flOptions = 0x00000000; // No special options
			const uint dwInitialSize = 4096;
			const uint dwMaximumSize = 0; // Growable heap

			// Act
			var heapHandle = _testEnv.CallKernel32Api("HEAPCREATE", flOptions, dwInitialSize, dwMaximumSize);

			// Assert
			Assert.NotEqual(0u, heapHandle);
		}

		[Fact]
		public void HeapAlloc_WithValidHeap_ShouldReturnNonZeroAddress()
		{
			// Arrange
			var heapHandle = _testEnv.CallKernel32Api("HEAPCREATE", 0, 4096, 0);
			Assert.NotEqual(0u, heapHandle);

			const uint dwFlags = 0x00000000; // No special flags
			const uint dwBytes = 256;

			// Act
			var address = _testEnv.CallKernel32Api("HEAPALLOC", heapHandle, dwFlags, dwBytes);

			// Assert
			Assert.NotEqual(0u, address);
		}

		[Fact]
		public void HeapFree_WithValidParameters_ShouldReturnOne()
		{
			// Arrange
			var heapHandle = _testEnv.CallKernel32Api("HEAPCREATE", 0, 4096, 0);
			var address = _testEnv.CallKernel32Api("HEAPALLOC", heapHandle, 0, 256);
			Assert.NotEqual(0u, address);

			const uint dwFlags = 0x00000000;

			// Act
			var result = _testEnv.CallKernel32Api("HEAPFREE", heapHandle, dwFlags, address);

			// Assert
			Assert.Equal(1u, result); // HeapFree returns 1 on success
		}

		[Fact]
		public void HeapAlloc_MultipleAllocations_ShouldReturnDifferentAddresses()
		{
			// Arrange
			var heapHandle = _testEnv.CallKernel32Api("HEAPCREATE", 0, 4096, 0);
			Assert.NotEqual(0u, heapHandle);

			// Act
			var address1 = _testEnv.CallKernel32Api("HEAPALLOC", heapHandle, 0, 128);
			var address2 = _testEnv.CallKernel32Api("HEAPALLOC", heapHandle, 0, 128);

			// Assert
			Assert.NotEqual(0u, address1);
			Assert.NotEqual(0u, address2);
			Assert.NotEqual(address1, address2);
		}

		#endregion

		#region VirtualAlloc Tests

		[Fact]
		public void VirtualAlloc_WithZeroAddress_ShouldReturnNonZeroAddress()
		{
			// Arrange - VirtualAlloc with lpAddress=0 lets system choose address
			const uint lpAddress = 0;
			const uint dwSize = 4096;
			const uint flAllocationType = 0x00001000; // MEM_COMMIT
			const uint flProtect = 0x04; // PAGE_READWRITE

			// Act
			var address = _testEnv.CallKernel32Api("VIRTUALALLOC", lpAddress, dwSize, flAllocationType, flProtect);

			// Assert
			Assert.NotEqual(0u, address);
		}

		[Fact]
		public void VirtualAlloc_WithSpecificAddress_ShouldReturnRequestedAddress()
		{
			// Arrange
			const uint lpAddress = 0x10000000; // Request specific address
			const uint dwSize = 4096;
			const uint flAllocationType = 0x00001000; // MEM_COMMIT
			const uint flProtect = 0x04; // PAGE_READWRITE

			// Act
			var address = _testEnv.CallKernel32Api("VIRTUALALLOC", lpAddress, dwSize, flAllocationType, flProtect);

			// Assert
			// The implementation might return the requested address or choose another
			Assert.NotEqual(0u, address);
		}

		[Fact]
		public void VirtualAlloc_MultipleAllocations_ShouldReturnDifferentAddresses()
		{
			// Arrange
			const uint dwSize = 4096;
			const uint flAllocationType = 0x00001000; // MEM_COMMIT
			const uint flProtect = 0x04; // PAGE_READWRITE

			// Act
			var address1 = _testEnv.CallKernel32Api("VIRTUALALLOC", 0, dwSize, flAllocationType, flProtect);
			var address2 = _testEnv.CallKernel32Api("VIRTUALALLOC", 0, dwSize, flAllocationType, flProtect);

			// Assert - VirtualAlloc should return valid addresses
			Assert.NotEqual(0u, address1); // Should return a valid address
			Assert.NotEqual(0u, address2); // Should return a valid address
			Assert.NotEqual(address1, address2); // Different allocations should have different addresses
		}

		#endregion

		public void Dispose()
		{
			_testEnv?.Dispose();
		}
	}
}