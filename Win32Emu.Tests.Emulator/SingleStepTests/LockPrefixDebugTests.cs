using Xunit;
using Xunit.Abstractions;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;
using Iced.Intel;

namespace Win32Emu.Tests.Emulator.SingleStepTests;

/// <summary>
/// Debug tests to understand LOCK prefix handling
/// </summary>
public class LockPrefixDebugTests
{
	private readonly ITestOutputHelper _output;
	
	public LockPrefixDebugTests(ITestOutputHelper output)
	{
		_output = output;
	}
	
	[Fact]
	public void LockAddRegisterToRegister_ShouldExecute()
	{
		// Test LOCK ADD DH, BH - the failing instruction from test 177 of 00.MOO.gz
		// Opcode: F0 02 F7 (LOCK prefix + ADD DH, BH)
		
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, decoderOptions: DecoderOptions.NoInvalidCheck, bitness: 16);
		
		// Set up registers
		cpu.SetRegister("EDX", 0xFFFD1CFF); // DH = 0x1C
		cpu.SetRegister("EBX", 0x1F70E300); // BH = 0xE3
		cpu.SetRegister("CS", 0x1000);
		cpu.SetEip(0x0000);
		cpu.SetRegister("ESP", 0x0002);
		cpu.SetRegister("EFLAGS", 0xFFFC0413);
		
		// Write instruction: F0 02 F7 F4 (LOCK ADD DH,BH + HLT)
		var address = (uint)((0x1000 << 4) + 0x0000); // CS:IP = 1000:0000 = physical 0x10000
		memory.Write8(address + 0, 0xF0); // LOCK prefix
		memory.Write8(address + 1, 0x02); // ADD opcode
		memory.Write8(address + 2, 0xF7); // ModRM: DH, BH
		memory.Write8(address + 3, 0xF4); // HLT
		
		_output.WriteLine($"Before: EDX=0x{cpu.GetRegister("EDX"):X8}, EIP=0x{cpu.GetEip():X4}, ESP=0x{cpu.GetRegister("ESP"):X8}");
		
		// Execute the LOCK ADD instruction
		var result = cpu.SingleStep(memory);
		
		var edx = cpu.GetRegister("EDX");
		var eip = cpu.GetEip();
		var esp = cpu.GetRegister("ESP");
		
		_output.WriteLine($"After:  EDX=0x{edx:X8}, EIP=0x{eip:X4}, ESP=0x{esp:X8}");
		
		// DH should be 0x1C + 0xE3 = 0xFF
		// EDX should be 0xFFFDFFFF
		var expectedEdx = 0xFFFDFFFF;
		var expectedEip = 0x0003; // Should advance by 3 bytes (F0 02 F7)
		var expectedEsp = 0x0002; // Should not change
		
		_output.WriteLine($"Expected: EDX=0x{expectedEdx:X8}, EIP=0x{expectedEip:X4}, ESP=0x{expectedEsp:X8}");
		
		Assert.True(expectedEdx == edx, $"EDX mismatch: expected 0x{expectedEdx:X8}, got 0x{edx:X8}");
		Assert.True(expectedEip == eip, $"EIP mismatch: expected 0x{expectedEip:X4}, got 0x{eip:X4}");
		Assert.True(expectedEsp == esp, $"ESP mismatch: expected 0x{expectedEsp:X8}, got 0x{esp:X8}");
	}
	
	[Fact]
	public void AddRegisterToRegister_WithoutLock_ShouldExecute()
	{
		// Test ADD DH, BH without LOCK prefix as a control
		// Opcode: 02 F7
		
		var memory = new VirtualMemory();
		var cpu = new IcedCpu(memory, decoderOptions: DecoderOptions.NoInvalidCheck, bitness: 16);
		
		// Set up registers
		cpu.SetRegister("EDX", 0xFFFD1CFF); // DH = 0x1C
		cpu.SetRegister("EBX", 0x1F70E300); // BH = 0xE3
		cpu.SetRegister("CS", 0x1000);
		cpu.SetEip(0x0000);
		cpu.SetRegister("ESP", 0x0002);
		
		// Write instruction: 02 F7 F4 (ADD DH,BH + HLT)
		var address = (uint)((0x1000 << 4) + 0x0000);
		memory.Write8(address + 0, 0x02); // ADD opcode
		memory.Write8(address + 1, 0xF7); // ModRM: DH, BH
		memory.Write8(address + 2, 0xF4); // HLT
		
		_output.WriteLine($"Before: EDX=0x{cpu.GetRegister("EDX"):X8}, EIP=0x{cpu.GetEip():X4}");
		
		// Execute the ADD instruction
		cpu.SingleStep(memory);
		
		var edx = cpu.GetRegister("EDX");
		var eip = cpu.GetEip();
		
		_output.WriteLine($"After:  EDX=0x{edx:X8}, EIP=0x{eip:X4}");
		
		// DH should be 0x1C + 0xE3 = 0xFF
		var expectedEdx = 0xFFFDFFFF;
		var expectedEip = 0x0002; // Should advance by 2 bytes (02 F7)
		
		_output.WriteLine($"Expected: EDX=0x{expectedEdx:X8}, EIP=0x{expectedEip:X4}");
		
		Assert.True(expectedEdx == edx, $"EDX mismatch: expected 0x{expectedEdx:X8}, got 0x{edx:X8}");
		Assert.True(expectedEip == eip, $"EIP mismatch: expected 0x{expectedEip:X4}, got 0x{eip:X4}");
	}
}
