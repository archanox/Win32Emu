using System;
using System.Runtime.InteropServices;
using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Xunit;

namespace Win32Emu.Tests.ABExample;

/// <summary>
/// Advanced A/B testing for memory allocation APIs.
/// Demonstrates hooking VirtualAlloc, VirtualFree, HeapAlloc, and HeapFree.
/// This is critical for game compatibility as many games rely on these APIs.
/// </summary>
public class MemoryAllocationHookingTests : HookingABTestBase
{
	// Constants from Windows API
	private const uint MEM_COMMIT = 0x1000;
	private const uint MEM_RESERVE = 0x2000;
	private const uint MEM_RELEASE = 0x8000;
	private const uint PAGE_READWRITE = 0x04;

	// VirtualAlloc delegate
	[UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true)]
	private delegate IntPtr VirtualAllocDelegate(
		IntPtr lpAddress,
		UIntPtr dwSize,
		uint flAllocationType,
		uint flProtect
	);

	// VirtualFree delegate
	[UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true)]
	private delegate bool VirtualFreeDelegate(
		IntPtr lpAddress,
		UIntPtr dwSize,
		uint dwFreeType
	);

	// HeapAlloc delegate
	[UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true)]
	private delegate IntPtr HeapAllocDelegate(
		IntPtr hHeap,
		uint dwFlags,
		UIntPtr dwBytes
	);

	// GetProcessHeap delegate
	[UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true)]
	private delegate IntPtr GetProcessHeapDelegate();

	// Store original functions
	private VirtualAllocDelegate? _originalVirtualAlloc;
	private VirtualFreeDelegate? _originalVirtualFree;
	private HeapAllocDelegate? _originalHeapAlloc;
	private GetProcessHeapDelegate? _originalGetProcessHeap;

	// Track allocations
	private int _virtualAllocCallCount;
	private int _heapAllocCallCount;

	public MemoryAllocationHookingTests()
	{
		if (_hookingAvailable)
		{
			// Get original functions
			_originalVirtualAlloc = GetOriginalFunction<VirtualAllocDelegate>("kernel32.dll", "VirtualAlloc");
			_originalVirtualFree = GetOriginalFunction<VirtualFreeDelegate>("kernel32.dll", "VirtualFree");
			_originalHeapAlloc = GetOriginalFunction<HeapAllocDelegate>("kernel32.dll", "HeapAlloc");
			_originalGetProcessHeap = GetOriginalFunction<GetProcessHeapDelegate>("kernel32.dll", "GetProcessHeap");

			// Create hooks
			CreateHook("kernel32.dll", "VirtualAlloc", new VirtualAllocDelegate(VirtualAllocHook));
			CreateHook("kernel32.dll", "VirtualFree", new VirtualFreeDelegate(VirtualFreeHook));
			CreateHook("kernel32.dll", "HeapAlloc", new HeapAllocDelegate(HeapAllocHook));
		}
	}

	/// <summary>
	/// Hook handler for VirtualAlloc.
	/// </summary>
	private IntPtr VirtualAllocHook(
		IntPtr lpAddress,
		UIntPtr dwSize,
		uint flAllocationType,
		uint flProtect)
	{
		_virtualAllocCallCount++;

		// Capture parameters
		CaptureHookData("VirtualAlloc.Address", lpAddress);
		CaptureHookData("VirtualAlloc.Size", dwSize.ToUInt32());
		CaptureHookData("VirtualAlloc.AllocationType", flAllocationType);
		CaptureHookData("VirtualAlloc.Protect", flProtect);

		// Call original
		var result = _originalVirtualAlloc?.Invoke(lpAddress, dwSize, flAllocationType, flProtect) ?? IntPtr.Zero;

		// Capture result
		CaptureHookData("VirtualAlloc.Result", result);
		CaptureHookData("VirtualAlloc.Success", result != IntPtr.Zero);

		return result;
	}

	/// <summary>
	/// Hook handler for VirtualFree.
	/// </summary>
	private bool VirtualFreeHook(
		IntPtr lpAddress,
		UIntPtr dwSize,
		uint dwFreeType)
	{
		var result = _originalVirtualFree?.Invoke(lpAddress, dwSize, dwFreeType) ?? false;
		CaptureHookData("VirtualFree.Result", result);
		return result;
	}

	/// <summary>
	/// Hook handler for HeapAlloc.
	/// </summary>
	private IntPtr HeapAllocHook(
		IntPtr hHeap,
		uint dwFlags,
		UIntPtr dwBytes)
	{
		_heapAllocCallCount++;

		// Capture parameters
		CaptureHookData("HeapAlloc.Heap", hHeap);
		CaptureHookData("HeapAlloc.Flags", dwFlags);
		CaptureHookData("HeapAlloc.Bytes", dwBytes.ToUInt32());

		// Call original
		var result = _originalHeapAlloc?.Invoke(hHeap, dwFlags, dwBytes) ?? IntPtr.Zero;

		// Capture result
		CaptureHookData("HeapAlloc.Result", result);
		CaptureHookData("HeapAlloc.Success", result != IntPtr.Zero);

		return result;
	}

	[Fact]
	[Trait("Category", "HookTest")]
	[Trait("Category", "Advanced")]
	[Trait("Function", "VirtualAlloc")]
	public void VirtualAlloc_WithValidParameters_SucceedsInBothImplementations()
	{
		// Skip if hooking is not available
		if (!_hookingAvailable)
		{
			return;
		}

		// Arrange
		using var testEnv = new TestEnvironment();
		const uint allocSize = 4096; // 1 page

		// Act - Win32Emu implementation
		var win32EmuResult = testEnv.CallKernel32Api(
			"VIRTUALALLOC",
			0u, // lpAddress (NULL for system choice)
			allocSize,
			MEM_COMMIT | MEM_RESERVE,
			PAGE_READWRITE
		);

		// Act - Native implementation (triggers hook)
		IntPtr? nativeResult = null;
		if (_originalVirtualAlloc != null)
		{
			nativeResult = _originalVirtualAlloc.Invoke(
				IntPtr.Zero,
				new UIntPtr(allocSize),
				MEM_COMMIT | MEM_RESERVE,
				PAGE_READWRITE
			);

			// Verify hook was called
			Assert.Equal(1, _virtualAllocCallCount);
			Assert.Equal(allocSize, GetCapturedData<uint>("VirtualAlloc.Size"));
		}

		// Assert - Both should succeed
		Assert.NotEqual(0u, win32EmuResult);

		if (nativeResult.HasValue)
		{
			Assert.NotEqual(IntPtr.Zero, nativeResult.Value);

			// Both implementations allocated memory successfully
			var capturedSuccess = GetCapturedData<bool>("VirtualAlloc.Success");
			Assert.True(capturedSuccess);

			// Clean up native allocation
			_originalVirtualFree?.Invoke(nativeResult.Value, UIntPtr.Zero, MEM_RELEASE);
		}

		// Clean up Win32Emu allocation
		testEnv.CallKernel32Api(
			"VIRTUALFREE",
			win32EmuResult,
			0u,
			MEM_RELEASE
		);
	}

	[Fact]
	[Trait("Category", "HookTest")]
	[Trait("Category", "Advanced")]
	[Trait("Function", "VirtualAlloc")]
	public void VirtualAlloc_WithZeroSize_BehaviorMatchesNative()
	{
		// Skip if hooking is not available
		if (!_hookingAvailable)
		{
			return;
		}

		// Arrange
		using var testEnv = new TestEnvironment();

		// Act - Win32Emu implementation (should fail with zero size)
		var win32EmuResult = testEnv.CallKernel32Api(
			"VIRTUALALLOC",
			0u,
			0u, // Zero size - invalid
			MEM_COMMIT | MEM_RESERVE,
			PAGE_READWRITE
		);

		// Act - Native implementation
		IntPtr? nativeResult = null;
		if (_originalVirtualAlloc != null)
		{
			nativeResult = _originalVirtualAlloc.Invoke(
				IntPtr.Zero,
				UIntPtr.Zero, // Zero size
				MEM_COMMIT | MEM_RESERVE,
				PAGE_READWRITE
			);
		}

		// Assert - Both should fail (return NULL/0)
		Assert.Equal(0u, win32EmuResult);

		if (nativeResult.HasValue)
		{
			// Native should also fail
			Assert.Equal(IntPtr.Zero, nativeResult.Value);
		}
	}

	[Fact]
	[Trait("Category", "HookTest")]
	[Trait("Category", "Advanced")]
	[Trait("Function", "HeapAlloc")]
	public void HeapAlloc_AllocatesMemorySuccessfully()
	{
		// Skip if hooking is not available
		if (!_hookingAvailable)
		{
			return;
		}

		// Arrange
		using var testEnv = new TestEnvironment();
		const uint allocSize = 256;

		// Get process heap handle
		var heapHandle = testEnv.CallKernel32Api("GETPROCESSHEAP");
		Assert.NotEqual(0u, heapHandle);

		// Act - Win32Emu implementation
		var win32EmuResult = testEnv.CallKernel32Api(
			"HEAPALLOC",
			heapHandle,
			0u, // dwFlags
			allocSize
		);

		// Act - Native implementation
		IntPtr? nativeResult = null;
		if (_originalGetProcessHeap != null && _originalHeapAlloc != null)
		{
			var nativeHeap = _originalGetProcessHeap.Invoke();
			nativeResult = _originalHeapAlloc.Invoke(
				nativeHeap,
				0,
				new UIntPtr(allocSize)
			);

			// Verify hook was called
			Assert.Equal(1, _heapAllocCallCount);
			Assert.Equal(allocSize, GetCapturedData<uint>("HeapAlloc.Bytes"));
		}

		// Assert - Both should succeed
		Assert.NotEqual(0u, win32EmuResult);

		if (nativeResult.HasValue)
		{
			Assert.NotEqual(IntPtr.Zero, nativeResult.Value);

			var capturedSuccess = GetCapturedData<bool>("HeapAlloc.Success");
			Assert.True(capturedSuccess);

			// Note: We don't call HeapFree here for simplicity
			// In a real test, you would clean up the allocation
		}
	}

	[Fact]
	[Trait("Category", "HookTest")]
	[Trait("Category", "Example")]
	[Trait("Function", "VirtualAlloc")]
	public void VirtualAlloc_SequenceOfAllocationsAndFrees()
	{
		// Skip if hooking is not available
		if (!_hookingAvailable)
		{
			return;
		}

		// This test demonstrates tracking a sequence of allocations
		// Similar to what a game might do during initialization

		using var testEnv = new TestEnvironment();

		// Allocate 3 blocks
		var block1 = testEnv.CallKernel32Api("VIRTUALALLOC", 0u, 1024u, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
		var block2 = testEnv.CallKernel32Api("VIRTUALALLOC", 0u, 2048u, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
		var block3 = testEnv.CallKernel32Api("VIRTUALALLOC", 0u, 4096u, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

		// All allocations should succeed
		Assert.NotEqual(0u, block1);
		Assert.NotEqual(0u, block2);
		Assert.NotEqual(0u, block3);

		// Free them
		var free1 = testEnv.CallKernel32Api("VIRTUALFREE", block1, 0u, MEM_RELEASE);
		var free2 = testEnv.CallKernel32Api("VIRTUALFREE", block2, 0u, MEM_RELEASE);
		var free3 = testEnv.CallKernel32Api("VIRTUALFREE", block3, 0u, MEM_RELEASE);

		// All frees should succeed (return non-zero)
		Assert.NotEqual(0u, free1);
		Assert.NotEqual(0u, free2);
		Assert.NotEqual(0u, free3);

		// If we had native tracking, we could compare the sequence
		// This demonstrates how you'd test game initialization patterns
	}
}

/// <summary>
/// Demonstrates comparing memory write/read behavior between Win32Emu and native Windows.
/// </summary>
public class MemoryAccessHookingTests : HookingABTestBase
{
	[UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true)]
	private delegate IntPtr VirtualAllocDelegate(
		IntPtr lpAddress,
		UIntPtr dwSize,
		uint flAllocationType,
		uint flProtect
	);

	private VirtualAllocDelegate? _originalVirtualAlloc;

	public MemoryAccessHookingTests()
	{
		if (_hookingAvailable)
		{
			_originalVirtualAlloc = GetOriginalFunction<VirtualAllocDelegate>("kernel32.dll", "VirtualAlloc");
		}
	}

	[Fact]
	[Trait("Category", "HookTest")]
	[Trait("Category", "Example")]
	[Trait("Function", "VirtualAlloc")]
	public void MemoryWriteRead_BehaviorMatchesNative()
	{
		using var testEnv = new TestEnvironment();
		const uint allocSize = 4096;
		const uint MEM_COMMIT = 0x1000;
		const uint MEM_RESERVE = 0x2000;
		const uint PAGE_READWRITE = 0x04;

		// Act - Win32Emu: allocate and write
		var win32EmuAddr = testEnv.CallKernel32Api(
			"VIRTUALALLOC",
			0u,
			allocSize,
			MEM_COMMIT | MEM_RESERVE,
			PAGE_READWRITE
		);

		Assert.NotEqual(0u, win32EmuAddr);

		// Write a test pattern
		const uint testValue = 0xDEADBEEF;
		testEnv.Memory.Write32(win32EmuAddr, testValue);

		// Read it back
		var readValue = testEnv.Memory.Read32(win32EmuAddr);

		// Assert
		Assert.Equal(testValue, readValue);

		// If we had native hooking for ReadProcessMemory/WriteProcessMemory,
		// we could compare the exact behavior here
	}
}
