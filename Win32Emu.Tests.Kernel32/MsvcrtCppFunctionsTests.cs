using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Win32Emu.Win32.Modules;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for MSVCRT C++ functions (mangled names)
/// </summary>
public sealed class MsvcrtCppFunctionsTests : IDisposable
{
	private readonly TestEnvironment _testEnv;
	private readonly MsvcrtModule _msvcrt;

	public MsvcrtCppFunctionsTests()
	{
		_testEnv = new TestEnvironment();
		_msvcrt = new MsvcrtModule(_testEnv.ProcessEnv, 0x00400000, _testEnv.PeLoader, NullLogger.Instance);
		_testEnv.Dispatcher.RegisterModule(_msvcrt);
	}

	[Fact]
	public void TypeInfoDestructor_ShouldSucceed()
	{
		// Arrange - allocate a fake type_info object
		var typeInfoPtr = _testEnv.ProcessEnv.HeapAlloc(0, 16);

		// Act - call type_info destructor (??1type_info@@UAE@XZ)
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, typeInfoPtr);
		var success = _msvcrt.TryInvokeUnsafe("??1type_info@@UAE@XZ", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "type_info destructor should be implemented");
		Assert.Equal(0u, returnValue); // Destructor returns void (0)
		
		// Cleanup
		_testEnv.ProcessEnv.HeapFree(0, typeInfoPtr);
	}

	[Fact]
	public void OperatorDelete_WithValidPointer_ShouldSucceed()
	{
		// Arrange - allocate memory
		var ptr = _testEnv.ProcessEnv.HeapAlloc(0, 32);
		Assert.NotEqual(0u, ptr);

		// Act - call operator delete (??3@YAXPAX@Z)
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, ptr);
		var success = _msvcrt.TryInvokeUnsafe("??3@YAXPAX@Z", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert
		Assert.True(success, "operator delete should be implemented");
		Assert.Equal(0u, returnValue); // delete returns void (0)
	}

	[Fact]
	public void OperatorDelete_WithNullPointer_ShouldSucceed()
	{
		// Arrange - null pointer
		var nullPtr = 0u;

		// Act - call operator delete (??3@YAXPAX@Z) with null
		_testEnv.Cpu.SetupStackArgs(_testEnv.Memory, nullPtr);
		var success = _msvcrt.TryInvokeUnsafe("??3@YAXPAX@Z", _testEnv.Cpu, _testEnv.Memory, out var returnValue);

		// Assert - should succeed and do nothing (null pointer is safe to delete)
		Assert.True(success, "operator delete should handle null pointers");
		Assert.Equal(0u, returnValue);
	}

	public void Dispose()
	{
		_testEnv.Dispose();
	}
}
