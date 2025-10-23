using Win32Emu.Cpu;
using Win32Emu.Cpu.Jit;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;
using Xunit;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for async JIT CPU backend functionality
/// </summary>
public class AsyncJitCpuTests
{
	[Fact]
	public void JitCpu_ShouldImplementIAsyncCpu()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		
		// Act
		var cpu = new JitCpu(mem);
		
		// Assert
		Assert.IsAssignableFrom<IAsyncCpu>(cpu);
		Assert.IsAssignableFrom<ICpu>(cpu);
	}

	[Fact]
	public void JitCpu_ShouldReportJitSupport()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		// Assert
		Assert.True(cpu.SupportsJit);
	}

	[Fact]
	public void IcedCpu_ShouldImplementIAsyncCpu()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		
		// Act
		var cpu = new IcedCpu(mem);
		
		// Assert
		Assert.IsAssignableFrom<IAsyncCpu>(cpu);
		Assert.IsAssignableFrom<ICpu>(cpu);
	}

	[Fact]
	public void IcedCpu_ShouldNotReportJitSupport()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new IcedCpu(mem);
		
		// Assert
		Assert.False(cpu.SupportsJit);
	}

	[Fact]
	public async Task SingleStepAsync_ShouldExecuteInstruction()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		// Write a NOP instruction at address 0x1000
		cpu.SetEip(0x1000);
		mem.Write8(0x1000, 0x90); // NOP
		
		// Act
		var result = await cpu.SingleStepAsync(mem);
		
		// Assert
		Assert.False(result.IsCall);
		Assert.Equal(0x1001u, cpu.GetEip()); // EIP should advance by 1
	}

	[Fact]
	public async Task ExecuteBlockAsync_ShouldExecuteMultipleInstructions()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetEip(0x1000);
		// Write multiple NOP instructions followed by a RET to terminate the block
		for (uint i = 0; i < 5; i++)
		{
			mem.Write8(0x1000 + i, 0x90); // NOP
		}
		mem.Write8(0x1005, 0xC3); // RET - terminates the block
		
		// Setup stack for RET
		cpu.SetRegister("ESP", 0x10000);
		mem.Write32(0x10000, 0x00002000); // Return address
		
		// Act
		var result = await cpu.ExecuteBlockAsync(mem);
		
		// Assert - execution should stop at RET
		Assert.NotNull(result);
	}

	[Fact]
	public void SaveState_ShouldCaptureAllRegisters()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetRegister("EAX", 0x12345678);
		cpu.SetRegister("EBX", 0xABCDEF00);
		cpu.SetRegister("ESP", 0x00200000);
		cpu.SetRegister("EIP", 0x00401000);
		
		// Act
		var state = cpu.SaveState();
		
		// Assert
		Assert.Equal(0x12345678u, state.Eax);
		Assert.Equal(0xABCDEF00u, state.Ebx);
		Assert.Equal(0x00200000u, state.Esp);
		Assert.Equal(0x00401000u, state.Eip);
	}

	[Fact]
	public void RestoreState_ShouldRestoreAllRegisters()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		var state = new CpuState
		{
			Eax = 0x11111111,
			Ebx = 0x22222222,
			Ecx = 0x33333333,
			Edx = 0x44444444,
			Esi = 0x55555555,
			Edi = 0x66666666,
			Ebp = 0x77777777,
			Esp = 0x88888888,
			Eip = 0x99999999,
			Eflags = 0xAAAAAAAA
		};
		
		// Act
		cpu.RestoreState(state);
		
		// Assert
		Assert.Equal(0x11111111u, cpu.GetRegister("EAX"));
		Assert.Equal(0x22222222u, cpu.GetRegister("EBX"));
		Assert.Equal(0x33333333u, cpu.GetRegister("ECX"));
		Assert.Equal(0x44444444u, cpu.GetRegister("EDX"));
		Assert.Equal(0x55555555u, cpu.GetRegister("ESI"));
		Assert.Equal(0x66666666u, cpu.GetRegister("EDI"));
		Assert.Equal(0x77777777u, cpu.GetRegister("EBP"));
		Assert.Equal(0x88888888u, cpu.GetRegister("ESP"));
		Assert.Equal(0x99999999u, cpu.GetRegister("EIP"));
		Assert.Equal(0xAAAAAAAAu, cpu.GetRegister("EFLAGS"));
	}

	[Fact]
	public void SaveAndRestore_ShouldMaintainCpuState()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetRegister("EAX", 0xDEADBEEF);
		cpu.SetRegister("ESP", 0x00200000);
		cpu.SetEip(0x00401234);
		
		// Act
		var savedState = cpu.SaveState();
		cpu.SetRegister("EAX", 0x00000000); // Modify state
		cpu.RestoreState(savedState);
		
		// Assert
		Assert.Equal(0xDEADBEEFu, cpu.GetRegister("EAX"));
		Assert.Equal(0x00200000u, cpu.GetRegister("ESP"));
		Assert.Equal(0x00401234u, cpu.GetEip());
	}

	[Fact]
	public async Task IcedCpu_SingleStepAsync_ShouldWorkLikeSynchronousVersion()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new IcedCpu(mem);
		
		cpu.SetEip(0x1000);
		mem.Write8(0x1000, 0x90); // NOP
		
		// Act
		var asyncResult = await cpu.SingleStepAsync(mem);
		cpu.SetEip(0x1000);
		var syncResult = cpu.SingleStep(mem);
		
		// Assert
		Assert.Equal(syncResult.IsCall, asyncResult.IsCall);
		Assert.Equal(syncResult.CallTarget, asyncResult.CallTarget);
	}

	[Fact]
	public async Task IcedCpu_ExecuteBlockAsync_ShouldExecuteMultipleSteps()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new IcedCpu(mem);
		
		cpu.SetEip(0x1000);
		// Write multiple NOP instructions followed by a CALL to terminate the block
		for (uint i = 0; i < 5; i++)
		{
			mem.Write8(0x1000 + i, 0x90); // NOP
		}
		// Write a CALL instruction to terminate the block
		mem.Write8(0x1005, 0xE8); // CALL rel32
		mem.Write32(0x1006, 0x00000100); // displacement
		
		// Setup stack for CALL
		cpu.SetRegister("ESP", 0x10000);
		
		var initialEip = cpu.GetEip();
		
		// Act
		var result = await cpu.ExecuteBlockAsync(mem);
		
		// Assert
		Assert.NotNull(result);
		Assert.True(result.IsCall); // Should stop at CALL
		// EIP should have advanced by executing instructions
		Assert.NotEqual(initialEip, cpu.GetEip());
	}
}
