using Win32Emu.Cpu;
using Win32Emu.Cpu.Jit;
using Win32Emu.Memory;
using Xunit;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for interpreter batch execution (InterpretInstructionBatch) exit conditions.
/// These tests use forceInterpreterMode to exercise the WASM/interpreter batch path
/// in ExecuteBlockAsync, verifying that batches stop correctly on syscalls, DOS interrupts,
/// special memory ranges, callback markers, and thread exit markers.
/// </summary>
public class InterpreterBatchTests
{
	/// <summary>
	/// Helper to create a JitCpu in forced interpreter mode (same path as WASM).
	/// </summary>
	private static JitCpu CreateInterpreterCpu(VirtualMemory mem)
	{
		return new JitCpu(mem, logger: null, forceInterpreterMode: true);
	}

	[Fact]
	public async Task Batch_ShouldStopOnSyscall_Int80()
	{
		// Arrange - 3 NOPs then INT 0x80 (syscall), then more NOPs
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = CreateInterpreterCpu(mem);

		cpu.SetEip(0x10000);
		cpu.SetRegister("ESP", 0x80000);
		mem.Write8(0x10000, 0x90); // NOP
		mem.Write8(0x10001, 0x90); // NOP
		mem.Write8(0x10002, 0x90); // NOP
		mem.Write8(0x10003, 0xCD); // INT
		mem.Write8(0x10004, 0x80); // 0x80 (syscall)
		mem.Write8(0x10005, 0x90); // NOP (should not execute)

		// Act
		var result = await cpu.ExecuteBlockAsync(mem);

		// Assert
		Assert.True(result.IsSyscall);
		Assert.False(result.IsDosInterrupt);
		Assert.Equal(4, result.InstructionsExecuted); // 3 NOPs + INT 0x80
	}

	[Fact]
	public async Task Batch_ShouldStopOnDosInterrupt_Int21()
	{
		// Arrange - 2 NOPs then INT 0x21 (DOS), then more NOPs
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = CreateInterpreterCpu(mem);

		cpu.SetEip(0x10000);
		cpu.SetRegister("ESP", 0x80000);
		mem.Write8(0x10000, 0x90); // NOP
		mem.Write8(0x10001, 0x90); // NOP
		mem.Write8(0x10002, 0xCD); // INT
		mem.Write8(0x10003, 0x21); // 0x21 (DOS)
		mem.Write8(0x10004, 0x90); // NOP (should not execute)

		// Act
		var result = await cpu.ExecuteBlockAsync(mem);

		// Assert
		Assert.True(result.IsDosInterrupt);
		Assert.False(result.IsSyscall);
		Assert.Equal(3, result.InstructionsExecuted); // 2 NOPs + INT 0x21
	}

	[Fact]
	public async Task Batch_ShouldStopOnSpecialRange_ImportHook()
	{
		// Arrange - 2 NOPs then JMP to import hook range (0x0F000000+)
		var mem = new VirtualMemory(256 * 1024 * 1024); // 256MB to cover import hook range
		var cpu = CreateInterpreterCpu(mem);

		cpu.SetEip(0x10000);
		cpu.SetRegister("ESP", 0x80000);
		mem.Write8(0x10000, 0x90); // NOP
		mem.Write8(0x10001, 0x90); // NOP
		// JMP rel32 to import hook range: E9 <rel32>
		// Target = 0x0F000000, current EIP after decode = 0x10007
		// rel32 = 0x0F000000 - 0x10007 = 0x0EFEFF9
		mem.Write8(0x10002, 0xE9); // JMP rel32
		var target = MemoryRegions.ImportHookBase;
		var rel32 = (int)(target - (0x10002u + 5)); // rel32 = target - (insn_addr + insn_length)
		mem.Write32(0x10003, (uint)rel32);

		// Act
		var result = await cpu.ExecuteBlockAsync(mem);

		// Assert - batch should stop because EIP is now in special range
		Assert.Equal(target, cpu.GetEip());
		Assert.Equal(3, result.InstructionsExecuted); // 2 NOPs + JMP
	}

	[Fact]
	public async Task Batch_ShouldStopOnThreadExitMarker()
	{
		// Arrange - 2 NOPs then JMP to 0xFFFFFFFF (thread exit marker)
		// We can't JMP to 0xFFFFFFFF directly with rel32 from low addresses,
		// so use RET with the marker as return address
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = CreateInterpreterCpu(mem);

		cpu.SetEip(0x10000);
		cpu.SetRegister("ESP", 0x80000);
		mem.Write32(0x80000, 0xFFFFFFFF); // Return address = thread exit marker
		mem.Write8(0x10000, 0x90); // NOP
		mem.Write8(0x10001, 0x90); // NOP
		mem.Write8(0x10002, 0xC3); // RET - pops 0xFFFFFFFF into EIP

		// Act
		var result = await cpu.ExecuteBlockAsync(mem);

		// Assert - batch should stop because EIP is now thread exit marker
		Assert.Equal(0xFFFFFFFFu, cpu.GetEip());
		Assert.Equal(3, result.InstructionsExecuted); // 2 NOPs + RET
	}

	[Fact]
	public async Task Batch_ShouldStopOnCallbackReturnMarker()
	{
		// Arrange - 2 NOPs then RET to callback marker range (0xDEAD0000-0xDEADFFFF)
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = CreateInterpreterCpu(mem);

		cpu.SetEip(0x10000);
		cpu.SetRegister("ESP", 0x80000);
		mem.Write32(0x80000, 0xDEAD0042); // Return address in callback marker range
		mem.Write8(0x10000, 0x90); // NOP
		mem.Write8(0x10001, 0x90); // NOP
		mem.Write8(0x10002, 0xC3); // RET - pops 0xDEAD0042 into EIP

		// Act
		var result = await cpu.ExecuteBlockAsync(mem);

		// Assert - batch should stop because EIP is in callback marker range
		Assert.Equal(0xDEAD0042u, cpu.GetEip());
		Assert.Equal(3, result.InstructionsExecuted); // 2 NOPs + RET
	}

	[Fact]
	public async Task Batch_ShouldStopOnLowMemoryEip()
	{
		// Arrange - 2 NOPs then RET to address below MinValidUserAddress (0x00010000)
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = CreateInterpreterCpu(mem);

		cpu.SetEip(0x10000);
		cpu.SetRegister("ESP", 0x80000);
		mem.Write32(0x80000, 0x00000001); // Return address in low memory
		mem.Write8(0x10000, 0x90); // NOP
		mem.Write8(0x10001, 0x90); // NOP
		mem.Write8(0x10002, 0xC3); // RET - pops 0x00000001 into EIP

		// Act
		var result = await cpu.ExecuteBlockAsync(mem);

		// Assert - batch should stop because EIP is in low memory
		Assert.Equal(0x00000001u, cpu.GetEip());
		Assert.Equal(3, result.InstructionsExecuted); // 2 NOPs + RET
	}

	[Fact]
	public async Task Batch_InstructionsExecuted_ShouldCountCorrectly()
	{
		// Arrange - Write exactly 10 NOPs followed by INT 0x80
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = CreateInterpreterCpu(mem);

		cpu.SetEip(0x10000);
		cpu.SetRegister("ESP", 0x80000);
		for (uint i = 0; i < 10; i++)
		{
			mem.Write8(0x10000 + i, 0x90); // NOP
		}
		mem.Write8(0x1000A, 0xCD); // INT
		mem.Write8(0x1000B, 0x80); // 0x80

		// Act
		var result = await cpu.ExecuteBlockAsync(mem);

		// Assert
		Assert.True(result.IsSyscall);
		Assert.Equal(11, result.InstructionsExecuted); // 10 NOPs + INT 0x80
	}

	[Fact]
	public async Task Batch_ShouldReturnDefaultInstructionsExecuted_ForSingleStep()
	{
		// Arrange - SingleStepAsync should always return InstructionsExecuted = 1
		var mem = new VirtualMemory(1024 * 1024);
		var cpu = CreateInterpreterCpu(mem);

		cpu.SetEip(0x10000);
		mem.Write8(0x10000, 0x90); // NOP

		// Act
		var result = await cpu.SingleStepAsync(mem);

		// Assert
		Assert.Equal(1, result.InstructionsExecuted);
		Assert.Equal(0x10001u, cpu.GetEip());
	}

	[Fact]
	public async Task Batch_ShouldStopOnComVtableRange()
	{
		// Arrange - 1 NOP then JMP to COM vtable range (0x0D000000+)
		var mem = new VirtualMemory(256 * 1024 * 1024); // 256MB to cover COM vtable range
		var cpu = CreateInterpreterCpu(mem);

		cpu.SetEip(0x10000);
		cpu.SetRegister("ESP", 0x80000);
		mem.Write8(0x10000, 0x90); // NOP
		// JMP rel32 to COM vtable range
		mem.Write8(0x10001, 0xE9); // JMP rel32
		var target = MemoryRegions.ComVtableBase;
		var rel32 = (int)(target - (0x10001u + 5));
		mem.Write32(0x10002, (uint)rel32);

		// Act
		var result = await cpu.ExecuteBlockAsync(mem);

		// Assert - batch should stop because EIP is in COM vtable range (special range)
		Assert.Equal(target, cpu.GetEip());
		Assert.Equal(2, result.InstructionsExecuted); // 1 NOP + JMP
	}
}
