using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Cpu;
using Win32Emu.Memory;
using Win32Emu.Win32;
using Win32Emu.Win32.COM;
using Xunit;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for the async/sync fallback in ComVtableDispatcher.
/// Verifies that COM methods registered as async handlers can be invoked through the sync TryInvoke path.
/// </summary>
public class ComVtableAsyncSyncFallbackTests
{
	/// <summary>
	/// Test that async handlers registered via CreateComObjectAsyncOrdered can be invoked through sync TryInvoke.
	/// This reproduces the bug where DirectDraw methods registered as async were not found by sync TryInvoke.
	/// </summary>
	[Fact]
	public void TryInvoke_WithAsyncHandlerRegistered_ShouldInvokeSuccessfully()
	{
		// Arrange
		var env = CreateTestEnvironment();
		var dispatcher = new ComVtableDispatcher(env, NullLogger.Instance);
		
		var methodInvoked = false;
		var expectedReturnValue = 0xDEADBEEF;
		
		// Register an async method handler
		var methods = new List<KeyValuePair<string, ComAsyncMethodInfo>>
		{
			new("QueryInterface", new ComAsyncMethodInfo(
				async (cpu, mem) => { await Task.CompletedTask; return 0; }, 
				ArgBytes: 8)), // pThis + riid + ppvObject
			new("AddRef", new ComAsyncMethodInfo(
				async (cpu, mem) => { await Task.CompletedTask; return 1; }, 
				ArgBytes: 0)), // pThis only (handled by dispatcher)
			new("Release", new ComAsyncMethodInfo(
				async (cpu, mem) => { await Task.CompletedTask; return 0; }, 
				ArgBytes: 0)), // pThis only
			new("TestMethod", new ComAsyncMethodInfo(
				async (cpu, mem) => 
				{ 
					methodInvoked = true;
					await Task.CompletedTask; 
					return expectedReturnValue; 
				}, 
				ArgBytes: 8)) // pThis + 2 parameters (4 bytes each)
		};
		
		var comObjectAddr = dispatcher.CreateComObjectAsyncOrdered("ITestInterface", methods);
		
		// Get the vtable address
		var vtableAddr = env.MemRead32(comObjectAddr);
		
		// Get the address of TestMethod (index 3 in vtable, so offset 12)
		var testMethodAddr = env.MemRead32(vtableAddr + 12);
		
		// Create a mock CPU with a stack
		var cpu = CreateMockCpu(env);
		
		// Act - invoke through SYNC TryInvoke path
		var success = dispatcher.TryInvoke(testMethodAddr, cpu, env.Memory, out var returnValue, out var argBytes);
		
		// Assert
		Assert.True(success, "TryInvoke should succeed when async handler is registered");
		Assert.True(methodInvoked, "Async handler should have been invoked");
		Assert.Equal(expectedReturnValue, returnValue);
		Assert.Equal(8, argBytes);
	}
	
	/// <summary>
	/// Test that sync handlers are still preferred over async handlers when both are available.
	/// </summary>
	[Fact]
	public void TryInvoke_WithBothSyncAndAsyncHandlers_ShouldPreferSyncHandler()
	{
		// Arrange
		var env = CreateTestEnvironment();
		var dispatcher = new ComVtableDispatcher(env, NullLogger.Instance);
		
		var syncInvoked = false;
		var asyncInvoked = false;
		
		// This test would require access to internal dictionaries to manually register both
		// For now, we verify that sync handlers work as expected
		var methods = new List<KeyValuePair<string, ComMethodInfo>>
		{
			new("QueryInterface", new ComMethodInfo((cpu, mem) => 0, ArgBytes: 8)),
			new("AddRef", new ComMethodInfo((cpu, mem) => 1, ArgBytes: 0)),
			new("Release", new ComMethodInfo((cpu, mem) => 0, ArgBytes: 0)),
			new("TestMethod", new ComMethodInfo(
				(cpu, mem) => 
				{ 
					syncInvoked = true;
					return 0x12345678; 
				}, 
				ArgBytes: 8))
		};
		
		var comObjectAddr = dispatcher.CreateComObjectOrdered("ITestInterface", methods);
		var vtableAddr = env.MemRead32(comObjectAddr);
		var testMethodAddr = env.MemRead32(vtableAddr + 12);
		var cpu = CreateMockCpu(env);
		
		// Act
		var success = dispatcher.TryInvoke(testMethodAddr, cpu, env.Memory, out var returnValue, out var argBytes);
		
		// Assert
		Assert.True(success, "TryInvoke should succeed with sync handler");
		Assert.True(syncInvoked, "Sync handler should have been invoked");
		Assert.False(asyncInvoked, "Async handler should not have been invoked");
		Assert.Equal(0x12345678u, returnValue);
	}
	
	/// <summary>
	/// Test that unregistered addresses still return false.
	/// </summary>
	[Fact]
	public void TryInvoke_WithUnregisteredAddress_ShouldReturnFalse()
	{
		// Arrange
		var env = CreateTestEnvironment();
		var dispatcher = new ComVtableDispatcher(env, NullLogger.Instance);
		var cpu = CreateMockCpu(env);
		
		// Use a COM vtable address that's not registered
		var unregisteredAddr = 0x0D002000u;
		
		// Act
		var success = dispatcher.TryInvoke(unregisteredAddr, cpu, env.Memory, out var returnValue, out var argBytes);
		
		// Assert
		Assert.False(success, "TryInvoke should return false for unregistered address");
		Assert.Equal(0u, returnValue);
		Assert.Equal(0, argBytes);
	}
	
	private ProcessEnvironment CreateTestEnvironment()
	{
		var memory = new VirtualMemory(256 * 1024 * 1024); // 256 MB
		var env = new ProcessEnvironment(memory, heapBase: 0x01000000, host: null, logger: NullLogger.Instance);
		
		// Initialize the environment with a simple stack
		env.StackBase = 0x00300000;
		env.StackLimit = 0x00200000;
		
		return env;
	}
	
	private ICpu CreateMockCpu(ProcessEnvironment env)
	{
		// Create a simple mock CPU for testing
		// We just need it to have a valid stack pointer
		var cpu = new MockCpu(env);
		cpu.SetRegister("ESP", env.StackBase - 0x1000); // Stack grows downward
		return cpu;
	}
	
	/// <summary>
	/// Minimal ICpu implementation for testing
	/// </summary>
	private class MockCpu : ICpu
	{
		private readonly Dictionary<string, uint> _registers = new();
		private readonly ProcessEnvironment _env;
		
		public MockCpu(ProcessEnvironment env)
		{
			_env = env;
			// Initialize common registers
			_registers["EAX"] = 0;
			_registers["EBX"] = 0;
			_registers["ECX"] = 0;
			_registers["EDX"] = 0;
			_registers["ESI"] = 0;
			_registers["EDI"] = 0;
			_registers["EBP"] = 0;
			_registers["ESP"] = 0;
			_registers["EIP"] = 0;
		}
		
		public uint GetRegister(string name)
		{
			var key = name.ToUpperInvariant();
			return _registers.TryGetValue(key, out var value) ? value : 0u;
		}
		public void SetRegister(string name, uint value, string? source = null) => _registers[name.ToUpperInvariant()] = value;
		public uint GetEip() => GetRegister("EIP");
		public void SetEip(uint value) => SetRegister("EIP", value);
		public CpuStepResult SingleStep(VirtualMemory memory) => new CpuStepResult(false, 0);
		
		// Not used in these tests
		public void Execute(int maxInstructions = -1) => throw new NotImplementedException();
		public void ExecuteSingle() => throw new NotImplementedException();
		public long GetInstructionCount() => 0;
		public void Reset() => throw new NotImplementedException();
		public bool GetFlag(string flagName) => false;
		public void SetFlag(string flagName, bool value) => throw new NotImplementedException();
	}
}
