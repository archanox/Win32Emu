using Win32Emu.Cpu;
using Win32Emu.Cpu.Jit;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;
using Xunit;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for CPU state suspend/resume functionality across async boundaries
/// </summary>
public class CpuStateSuspendResumeTests
{
	[Fact]
	public void SuspendExecution_WithJitCpu_ShouldSaveCpuState()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetRegister("EAX", 0x12345678);
		cpu.SetRegister("EBX", 0xABCDEF00);
		cpu.SetRegister("ESP", 0x00200000);
		cpu.SetEip(0x00401000);
		
		// Act
		var state = CpuHelpers.SuspendExecution(cpu);
		
		// Assert
		Assert.NotNull(state);
		Assert.Equal(0x12345678u, state.Eax);
		Assert.Equal(0xABCDEF00u, state.Ebx);
		Assert.Equal(0x00200000u, state.Esp);
		Assert.Equal(0x00401000u, state.Eip);
	}
	
	[Fact]
	public void ResumeExecution_WithJitCpu_ShouldRestoreCpuState()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		var originalState = new CpuState
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
		CpuHelpers.ResumeExecution(cpu, originalState);
		
		// Assert
		Assert.Equal(0x11111111u, cpu.GetRegister("EAX"));
		Assert.Equal(0x22222222u, cpu.GetRegister("EBX"));
		Assert.Equal(0x33333333u, cpu.GetRegister("ECX"));
		Assert.Equal(0x44444444u, cpu.GetRegister("EDX"));
		Assert.Equal(0x55555555u, cpu.GetRegister("ESI"));
		Assert.Equal(0x66666666u, cpu.GetRegister("EDI"));
		Assert.Equal(0x77777777u, cpu.GetRegister("EBP"));
		Assert.Equal(0x88888888u, cpu.GetRegister("ESP"));
		Assert.Equal(0x99999999u, cpu.GetEip());
	}
	
	[Fact]
	public void SuspendAndResume_WithStateModification_ShouldRestoreOriginalState()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetRegister("EAX", 0x12345678);
		cpu.SetRegister("EBX", 0xABCDEF00);
		
		// Act - Suspend
		var state = CpuHelpers.SuspendExecution(cpu);
		
		// Modify CPU state
		cpu.SetRegister("EAX", 0xFFFFFFFF);
		cpu.SetRegister("EBX", 0x00000000);
		
		// Act - Resume
		CpuHelpers.ResumeExecution(cpu, state);
		
		// Assert - Original state restored
		Assert.Equal(0x12345678u, cpu.GetRegister("EAX"));
		Assert.Equal(0xABCDEF00u, cpu.GetRegister("EBX"));
	}
	
	[Fact]
	public void SuspendExecution_WithIcedCpu_ShouldSaveCpuState()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new IcedCpu(mem);
		
		cpu.SetRegister("EAX", 0x12345678);
		cpu.SetRegister("EBX", 0xABCDEF00);
		
		// Act
		var state = CpuHelpers.SuspendExecution(cpu);
		
		// Assert
		Assert.NotNull(state);
		Assert.Equal(0x12345678u, state.Eax);
		Assert.Equal(0xABCDEF00u, state.Ebx);
	}
	
	[Fact]
	public async Task SuspendAndResume_AcrossAsyncBoundary_ShouldPreserveState()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetRegister("EAX", 0x12345678);
		cpu.SetRegister("EBX", 0xABCDEF00);
		cpu.SetRegister("ESP", 0x00200000);
		cpu.SetEip(0x00401000);
		
		// Act - Simulate async callback pattern
		var state = CpuHelpers.SuspendExecution(cpu);
		
		// Simulate async operation
		await Task.Delay(1);
		
		// Modify CPU state during async operation (simulating other code running)
		cpu.SetRegister("EAX", 0xFFFFFFFF);
		cpu.SetRegister("EBX", 0x00000000);
		
		// Resume execution
		CpuHelpers.ResumeExecution(cpu, state);
		
		// Assert - Original state restored
		Assert.Equal(0x12345678u, cpu.GetRegister("EAX"));
		Assert.Equal(0xABCDEF00u, cpu.GetRegister("EBX"));
		Assert.Equal(0x00200000u, cpu.GetRegister("ESP"));
		Assert.Equal(0x00401000u, cpu.GetEip());
	}
	
	[Fact]
	public void SuspendExecution_WithNullCpu_ShouldReturnNull()
	{
		// Arrange
		ICpu? cpu = null;
		
		// Act
		var state = CpuHelpers.SuspendExecution(cpu!);
		
		// Assert
		Assert.Null(state);
	}
	
	[Fact]
	public void ResumeExecution_WithNullState_ShouldNotThrow()
	{
		// Arrange
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = new JitCpu(mem);
		
		cpu.SetRegister("EAX", 0x12345678);
		
		// Act & Assert - Should not throw
		CpuHelpers.ResumeExecution(cpu, null);
		
		// State should remain unchanged
		Assert.Equal(0x12345678u, cpu.GetRegister("EAX"));
	}
}
