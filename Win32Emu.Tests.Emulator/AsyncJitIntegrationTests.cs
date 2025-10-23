using Win32Emu.Cpu;
using Win32Emu.Cpu.Jit;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;
using Win32Emu.Win32;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Integration tests demonstrating async JIT capabilities with Win32 dispatcher
/// </summary>
public class AsyncJitIntegrationTests
{
	[Fact]
	public async Task JitCpu_WithAsyncDispatcher_ShouldHandleImportCalls()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		var dispatcher = new Win32Dispatcher(NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("ESP", 0x10000); // Use lower address within 1MB range
		
		// Write a simple call instruction
		mem.Write8(0x1000, 0xE8); // CALL rel32
		mem.Write32(0x1001, 0x00000005); // displacement
		
		// Act - Execute using async dispatcher
		var result = await cpu.SingleStepAsync(mem);
		var (success, retVal, argBytes) = await dispatcher.TryInvokeAsync(
			"KERNEL32.DLL", 
			"GetCurrentThreadId", 
			cpu, 
			mem
		);
		
		// Assert
		Assert.True(result.IsCall);
		Assert.True(success);
	}
	
	[Fact]
	public async Task IcedCpu_WithAsyncDispatcher_ShouldMaintainBackwardCompatibility()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new IcedCpu(mem, NullLogger.Instance);
		var dispatcher = new Win32Dispatcher(NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("ESP", 0x00200000);
		
		// Write a NOP
		mem.Write8(0x1000, 0x90);
		
		// Act - Both sync and async should work
		var syncResult = cpu.SingleStep(mem);
		cpu.SetEip(0x1000); // Reset
		var asyncResult = await cpu.SingleStepAsync(mem);
		
		// Assert - Results should be identical
		Assert.Equal(syncResult.IsCall, asyncResult.IsCall);
		Assert.Equal(syncResult.CallTarget, asyncResult.CallTarget);
	}
	
	[Fact]
	public async Task CpuStatePreservation_AcrossAsyncBoundaries()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		// Set up initial state
		cpu.SetRegister("EAX", 0x12345678);
		cpu.SetRegister("EBX", 0xABCDEF00);
		cpu.SetRegister("ECX", 0xFEDCBA98);
		cpu.SetRegister("ESP", 0x00200000);
		cpu.SetEip(0x00401000);
		
		// Act - Save state, simulate async operation, restore
		var savedState = cpu.SaveState();
		
		// Simulate an async operation that modifies CPU state
		await Task.Delay(1);
		cpu.SetRegister("EAX", 0xFFFFFFFF);
		cpu.SetRegister("EBX", 0x00000000);
		cpu.SetEip(0xDEADBEEF);
		
		// Restore the saved state
		cpu.RestoreState(savedState);
		
		// Assert - State should be restored exactly
		Assert.Equal(0x12345678u, cpu.GetRegister("EAX"));
		Assert.Equal(0xABCDEF00u, cpu.GetRegister("EBX"));
		Assert.Equal(0xFEDCBA98u, cpu.GetRegister("ECX"));
		Assert.Equal(0x00200000u, cpu.GetRegister("ESP"));
		Assert.Equal(0x00401000u, cpu.GetEip());
	}
	
	[Fact]
	public async Task AsyncBlockExecution_WithStatePreservation()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem, NullLogger.Instance);
		
		cpu.SetEip(0x1000);
		cpu.SetRegister("EAX", 100);
		
		// Write a sequence of NOPs followed by a RET
		for (uint i = 0; i < 5; i++)
		{
			mem.Write8(0x1000 + i, 0x90); // NOP
		}
		mem.Write8(0x1005, 0xC3); // RET
		
		// Write return address on stack
		cpu.SetRegister("ESP", 0x10000); // Use lower address within 1MB range
		mem.Write32(0x10000, 0x00002000);
		
		// Act - Execute block asynchronously
		var initialState = cpu.SaveState();
		var result = await cpu.ExecuteBlockAsync(mem, 10);
		
		// Assert
		Assert.NotNull(result);
		// EAX should be unchanged (NOPs don't modify registers)
		Assert.Equal(100u, cpu.GetRegister("EAX"));
	}
	
	[Fact]
	public async Task DifferentCpuBackends_ShouldProduceSimilarResults()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var icedCpu = new IcedCpu(mem, NullLogger.Instance);
		var jitCpu = new JitCpu(mem, NullLogger.Instance);
		
		// Set same initial state
		foreach (var cpu in new ICpu[] { icedCpu, jitCpu })
		{
			cpu.SetEip(0x1000);
			cpu.SetRegister("EAX", 42);
			cpu.SetRegister("ESP", 0x00200000);
		}
		
		// Write a simple instruction (NOP)
		mem.Write8(0x1000, 0x90);
		
		// Act - Execute on both backends
		var icedResult = await icedCpu.SingleStepAsync(mem);
		jitCpu.SetEip(0x1000); // Reset for JIT
		var jitResult = await jitCpu.SingleStepAsync(mem);
		
		// Assert - Both should produce same result
		Assert.Equal(icedResult.IsCall, jitResult.IsCall);
		Assert.Equal(icedResult.CallTarget, jitResult.CallTarget);
		Assert.Equal(0x1001u, icedCpu.GetEip()); // NOP advances by 1
		Assert.Equal(0x1001u, jitCpu.GetEip());
	}
	
	[Fact]
	public void JitCpu_ShouldReportCorrectCapabilities()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var icedCpu = new IcedCpu(mem);
		var jitCpu = new JitCpu(mem);
		
		// Assert - Check SupportsJit property
		Assert.False(icedCpu.SupportsJit, "IcedCpu should not report JIT support");
		Assert.True(jitCpu.SupportsJit, "JitCpu should report JIT support");
	}
}
