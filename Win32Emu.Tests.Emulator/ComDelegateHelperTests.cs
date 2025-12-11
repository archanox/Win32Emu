using System;
using System.Runtime.InteropServices;
using Win32Emu.Win32.COM;
using Xunit;

namespace Win32Emu.Tests.Emulator;

// Test delegate declarations (must be at file/namespace level, not in method)
[UnmanagedFunctionPointer(CallingConvention.StdCall)]
delegate int SimpleMethod(IntPtr pThis);

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
delegate int QueryInterfaceDelegate(IntPtr pThis, IntPtr riid, IntPtr ppvObject);

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
delegate int SetCooperativeLevelDelegate(IntPtr pThis, IntPtr hwnd, uint dwFlags);

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
delegate int EnumDevicesDelegate(IntPtr pThis, uint dwDevType, IntPtr lpCallback, IntPtr pvRef, uint dwFlags);

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
delegate int GetDeviceStateDelegate(IntPtr pThis, uint cbData, IntPtr lpvData);

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
delegate int InitializeDelegate(IntPtr pThis, IntPtr hinst, uint dwVersion, IntPtr rguid);

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
delegate int StdCallMethod(IntPtr pThis);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
delegate int CdeclMethod(IntPtr pThis);

delegate int RegularDelegate(IntPtr pThis); // No attribute

// Void return type delegates - to test that return type doesn't affect argBytes
[UnmanagedFunctionPointer(CallingConvention.StdCall)]
delegate void VoidNoParamsDelegate();

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
delegate void VoidOneParamDelegate(IntPtr pThis);

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
delegate void VoidTwoParamsDelegate(IntPtr pThis, uint dwFlags);

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
delegate void VoidFiveParamsDelegate(IntPtr pThis, uint param1, IntPtr param2, uint param3, IntPtr param4);

public class ComDelegateHelperTests
{
	[Fact]
	public void GetArgBytes_SimpleMethod_ThisPointerOnly()
	{
		// Arrange - method with just 'this' pointer (IntPtr pThis)
		// Act
		var argBytes = ComDelegateHelper.GetArgBytes(typeof(SimpleMethod));
		
		// Assert
		Assert.Equal(4, argBytes); // 1 pointer parameter = 4 bytes on x86
	}
	
	[Fact]
	public void GetArgBytes_MethodWithMultipleIntPtrParams()
	{
		// Arrange - QueryInterface: this + riid + ppvObject (3 pointers)
		// Act
		var argBytes = ComDelegateHelper.GetArgBytes(typeof(QueryInterfaceDelegate));
		
		// Assert
		Assert.Equal(12, argBytes); // 3 pointer parameters = 12 bytes on x86
	}
	
	[Fact]
	public void GetArgBytes_MethodWithIntPtrAndUint()
	{
		// Arrange - SetCooperativeLevel: this + hwnd + dwFlags
		// Act
		var argBytes = ComDelegateHelper.GetArgBytes(typeof(SetCooperativeLevelDelegate));
		
		// Assert
		Assert.Equal(12, argBytes); // 2 pointers (8 bytes) + 1 uint (4 bytes) = 12 bytes
	}
	
	[Fact]
	public void GetArgBytes_MethodWithMultipleUints()
	{
		// Arrange - EnumDevices: this + dwDevType + lpCallback + pvRef + dwFlags
		// Act
		var argBytes = ComDelegateHelper.GetArgBytes(typeof(EnumDevicesDelegate));
		
		// Assert
		Assert.Equal(20, argBytes); // 3 pointers (12 bytes) + 2 uints (8 bytes) = 20 bytes
	}
	
	[Fact]
	public void GetArgBytes_GetDeviceState_ThisPlusCbDataPlusLpvData()
	{
		// Arrange - GetDeviceState: this + cbData + lpvData
		// Act
		var argBytes = ComDelegateHelper.GetArgBytes(typeof(GetDeviceStateDelegate));
		
		// Assert
		Assert.Equal(12, argBytes); // 2 pointers (8 bytes) + 1 uint (4 bytes) = 12 bytes
	}
	
	[Fact]
	public void GetArgBytes_Initialize_ComplexMethod()
	{
		// Arrange - Initialize: this + hinst + dwVersion + rguid
		// Act
		var argBytes = ComDelegateHelper.GetArgBytes(typeof(InitializeDelegate));
		
		// Assert
		Assert.Equal(16, argBytes); // 3 pointers (12 bytes) + 1 uint (4 bytes) = 16 bytes
	}
	
	[Fact]
	public void HasStdCallConvention_ReturnsTrue_ForStdCallDelegate()
	{
		// Arrange & Act
		var hasStdCall = ComDelegateHelper.HasStdCallConvention(typeof(StdCallMethod));
		
		// Assert
		Assert.True(hasStdCall);
	}
	
	[Fact]
	public void HasStdCallConvention_ReturnsFalse_ForCdeclDelegate()
	{
		// Arrange & Act
		var hasStdCall = ComDelegateHelper.HasStdCallConvention(typeof(CdeclMethod));
		
		// Assert
		Assert.False(hasStdCall);
	}
	
	[Fact]
	public void HasStdCallConvention_ReturnsFalse_ForDelegateWithoutAttribute()
	{
		// Arrange - delegate without UnmanagedFunctionPointer attribute
		// Act
		var hasStdCall = ComDelegateHelper.HasStdCallConvention(typeof(RegularDelegate));
		
		// Assert
		Assert.False(hasStdCall);
	}
	
	[Fact]
	public void FromDelegate_CreatesComMethodInfoWithCorrectArgBytes()
	{
		// Arrange
		static uint TestHandler(Cpu.ICpu cpu, Memory.VirtualMemory mem) => 0;
		
		// Act
		var methodInfo = ComVtableDispatcher.FromDelegate<IDirectInputDevice.Acquire>(TestHandler);
		
		// Assert
		Assert.Equal(4, methodInfo.ArgBytes); // Acquire has only 'this' pointer
		Assert.NotNull(methodInfo.Handler);
	}
	
	[Fact]
	public void FromDelegate_ThrowsException_ForNonStdCallDelegate()
	{
		// Arrange
		static uint TestHandler(Cpu.ICpu cpu, Memory.VirtualMemory mem) => 0;
		
		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => 
			ComVtableDispatcher.FromDelegate<CdeclMethod>(TestHandler));
	}
	
	[Fact]
	public void IDirectInput_Delegates_HaveCorrectSignatures()
	{
		// Verify that all IDirectInput delegate signatures compute to the expected argBytes
		Assert.Equal(12, ComDelegateHelper.GetArgBytes(typeof(IDirectInput.QueryInterface)));
		Assert.Equal(4, ComDelegateHelper.GetArgBytes(typeof(IDirectInput.AddRef)));
		Assert.Equal(4, ComDelegateHelper.GetArgBytes(typeof(IDirectInput.Release)));
		Assert.Equal(16, ComDelegateHelper.GetArgBytes(typeof(IDirectInput.CreateDevice)));
		Assert.Equal(20, ComDelegateHelper.GetArgBytes(typeof(IDirectInput.EnumDevices)));
		Assert.Equal(8, ComDelegateHelper.GetArgBytes(typeof(IDirectInput.GetDeviceStatus)));
		Assert.Equal(12, ComDelegateHelper.GetArgBytes(typeof(IDirectInput.RunControlPanel))); // this + hwndOwner + dwFlags = 12
		Assert.Equal(12, ComDelegateHelper.GetArgBytes(typeof(IDirectInput.Initialize))); // this + hinst + dwVersion = 12
	}
	
	[Fact]
	public void IDirectInputDevice_Delegates_HaveCorrectSignatures()
	{
		// Verify that all IDirectInputDevice delegate signatures compute to the expected argBytes
		Assert.Equal(12, ComDelegateHelper.GetArgBytes(typeof(IDirectInputDevice.QueryInterface)));
		Assert.Equal(4, ComDelegateHelper.GetArgBytes(typeof(IDirectInputDevice.AddRef)));
		Assert.Equal(4, ComDelegateHelper.GetArgBytes(typeof(IDirectInputDevice.Release)));
		Assert.Equal(8, ComDelegateHelper.GetArgBytes(typeof(IDirectInputDevice.GetCapabilities)));
		Assert.Equal(16, ComDelegateHelper.GetArgBytes(typeof(IDirectInputDevice.EnumObjects)));
		Assert.Equal(12, ComDelegateHelper.GetArgBytes(typeof(IDirectInputDevice.GetProperty)));
		Assert.Equal(12, ComDelegateHelper.GetArgBytes(typeof(IDirectInputDevice.SetProperty)));
		Assert.Equal(4, ComDelegateHelper.GetArgBytes(typeof(IDirectInputDevice.Acquire)));
		Assert.Equal(4, ComDelegateHelper.GetArgBytes(typeof(IDirectInputDevice.Unacquire)));
		Assert.Equal(12, ComDelegateHelper.GetArgBytes(typeof(IDirectInputDevice.GetDeviceState)));
		Assert.Equal(20, ComDelegateHelper.GetArgBytes(typeof(IDirectInputDevice.GetDeviceData)));
		Assert.Equal(8, ComDelegateHelper.GetArgBytes(typeof(IDirectInputDevice.SetDataFormat)));
		Assert.Equal(8, ComDelegateHelper.GetArgBytes(typeof(IDirectInputDevice.SetEventNotification)));
		Assert.Equal(12, ComDelegateHelper.GetArgBytes(typeof(IDirectInputDevice.SetCooperativeLevel)));
		Assert.Equal(16, ComDelegateHelper.GetArgBytes(typeof(IDirectInputDevice.GetObjectInfo))); // this + pdidoi + dwObj + dwHow = 16
		Assert.Equal(8, ComDelegateHelper.GetArgBytes(typeof(IDirectInputDevice.GetDeviceInfo)));
		Assert.Equal(12, ComDelegateHelper.GetArgBytes(typeof(IDirectInputDevice.RunControlPanel)));
		Assert.Equal(16, ComDelegateHelper.GetArgBytes(typeof(IDirectInputDevice.Initialize)));
	}
	
	/// <summary>
	/// Tests that void-returning functions have the same argBytes as int-returning functions
	/// with the same parameters. This validates that return type does NOT affect stack cleanup.
	/// 
	/// In stdcall convention:
	/// - Return values are in EAX register, not on stack
	/// - RET N instruction pops N bytes of parameters (not return value)
	/// - void vs int return type should not change argBytes
	/// </summary>
	[Fact]
	public void GetArgBytes_VoidReturnType_ShouldOnlyCountParameters()
	{
		// Arrange & Act - void functions with different parameter counts
		var noParams = ComDelegateHelper.GetArgBytes(typeof(VoidNoParamsDelegate));
		var oneParam = ComDelegateHelper.GetArgBytes(typeof(VoidOneParamDelegate));
		var twoParams = ComDelegateHelper.GetArgBytes(typeof(VoidTwoParamsDelegate));
		var fiveParams = ComDelegateHelper.GetArgBytes(typeof(VoidFiveParamsDelegate));
		
		// Assert - argBytes should match parameter count * 4, regardless of void return
		Assert.Equal(0, noParams);   // 0 parameters = 0 bytes
		Assert.Equal(4, oneParam);   // 1 parameter (IntPtr) = 4 bytes
		Assert.Equal(8, twoParams);  // 2 parameters (IntPtr + uint) = 8 bytes
		Assert.Equal(20, fiveParams); // 5 parameters (IntPtr + uint + IntPtr + uint + IntPtr) = 20 bytes
	}
	
	[Fact]
	public void GetArgBytes_VoidVsIntReturnType_ShouldBeIdentical()
	{
		// Arrange - compare void and int delegates with same parameters
		// Both have just IntPtr pThis parameter
		var voidMethod = ComDelegateHelper.GetArgBytes(typeof(VoidOneParamDelegate));
		var intMethod = ComDelegateHelper.GetArgBytes(typeof(SimpleMethod));
		
		// Assert - return type should NOT affect argBytes
		Assert.Equal(voidMethod, intMethod);
		Assert.Equal(4, voidMethod); // Both should be 4 bytes (one IntPtr parameter)
	}
	
	[Fact]
	public void GetArgBytes_VoidWithTwoParams_MatchesIntWithTwoParams()
	{
		// Arrange - VoidTwoParamsDelegate has IntPtr + uint (same as some int-returning methods)
		var voidTwoParams = ComDelegateHelper.GetArgBytes(typeof(VoidTwoParamsDelegate));
		
		// Assert - 2 parameters = 8 bytes, regardless of return type
		Assert.Equal(8, voidTwoParams); // IntPtr (4) + uint (4) = 8 bytes
	}
	
	[Fact]
	public void FromAsyncDelegate_CreatesComAsyncMethodInfoWithCorrectArgBytes()
	{
		// Arrange
		static async Task<uint> TestAsyncHandler(Cpu.ICpu cpu, Memory.VirtualMemory mem)
		{
			await Task.CompletedTask;
			return 0;
		}
		
		// Act
		var methodInfo = ComVtableDispatcher.FromAsyncDelegate<IDirectInputDevice.Acquire>(TestAsyncHandler);
		
		// Assert
		Assert.Equal(4, methodInfo.ArgBytes); // Acquire has only 'this' pointer
		Assert.NotNull(methodInfo.AsyncHandler);
	}
	
	[Fact]
	public void FromAsyncDelegate_ThrowsException_ForNonStdCallDelegate()
	{
		// Arrange
		static async Task<uint> TestAsyncHandler(Cpu.ICpu cpu, Memory.VirtualMemory mem)
		{
			await Task.CompletedTask;
			return 0;
		}
		
		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => 
			ComVtableDispatcher.FromAsyncDelegate<CdeclMethod>(TestAsyncHandler));
	}
	
	[Fact]
	public void FromAsyncDelegate_AndFromDelegate_ProduceSameArgBytes()
	{
		// Arrange
		static uint TestSyncHandler(Cpu.ICpu cpu, Memory.VirtualMemory mem) => 0;
		static async Task<uint> TestAsyncHandler(Cpu.ICpu cpu, Memory.VirtualMemory mem)
		{
			await Task.CompletedTask;
			return 0;
		}
		
		// Act
		var syncMethodInfo = ComVtableDispatcher.FromDelegate<IDirectInput.QueryInterface>(TestSyncHandler);
		var asyncMethodInfo = ComVtableDispatcher.FromAsyncDelegate<IDirectInput.QueryInterface>(TestAsyncHandler);
		
		// Assert - Both should calculate the same argBytes for the same delegate type
		Assert.Equal(syncMethodInfo.ArgBytes, asyncMethodInfo.ArgBytes);
		Assert.Equal(12, asyncMethodInfo.ArgBytes); // QueryInterface: this + riid + ppvObject = 12 bytes
	}
}
