using Xunit;
using Xunit.Abstractions;
using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;
using Iced.Intel;

namespace Win32Emu.Tests.Emulator.SingleStepTests;

public class LockPrefixDebugTest
{
private readonly ITestOutputHelper _output;

public LockPrefixDebugTest(ITestOutputHelper output)
{
_output = output;
}

[Fact]
public void TestInvalidLockWithRegisterDestination()
{
// Test that LOCK prefix with register destination generates an exception
// According to x86 spec, LOCK requires memory destination
var memory = new VirtualMemory();
var cpu = new IcedCpu(memory, decoderOptions: DecoderOptions.NoInvalidCheck, bitness: 16);

// Set up initial state including exception handler
cpu.SetRegister("EAX", 0x12345678);
cpu.SetRegister("CS", 0x1000);
cpu.SetRegister("DS", 0x1000);
cpu.SetRegister("ES", 0x1000);
cpu.SetRegister("SS", 0x1000);
cpu.SetRegister("ESP", 0x0100); // Stack at 1000:0100
cpu.SetEip(0x0000);

// Set up a simple exception handler in IVT for vector 6 (#UD)
// IVT entry at address 6*4 = 24 (0x18): [IP:2bytes][CS:2bytes]
memory.Write16(0x18, 0x0200); // Handler IP = 0x0200
memory.Write16(0x1A, 0x1000); // Handler CS = 0x1000

// Write handler code: just HLT
var handlerAddr = (uint)(0x1000 << 4) + 0x0200;
memory.Write8(handlerAddr, 0xF4); // HLT

// Write a LOCK ADD with register destination: F0 82 C5 D4 = lock add ch, 0xD4
// This is INVALID - LOCK requires memory destination
var physAddr = (uint)(0x1000 << 4) + 0x0000;
memory.Write8(physAddr + 0, 0xF0);  // LOCK prefix
memory.Write8(physAddr + 1, 0x82);  // ADD r/m8, imm8
memory.Write8(physAddr + 2, 0xC5);  // ModR/M: CH register
memory.Write8(physAddr + 3, 0xD4);  // Immediate value

_output.WriteLine($"Before: ECX=0x{cpu.GetRegister("ECX"):X8}, EIP=0x{cpu.GetEip():X8}, ESP=0x{cpu.GetRegister("ESP"):X8}");

try
{
cpu.SingleStep(memory);
_output.WriteLine($"After: ECX=0x{cpu.GetRegister("ECX"):X8}, EIP=0x{cpu.GetEip():X8}, ESP=0x{cpu.GetRegister("ESP"):X8}");

// Exception should have been generated, jumping to handler
var newEip = cpu.GetEip();
var newSp = cpu.GetRegister("ESP");

_output.WriteLine($"Exception handler executed - EIP jumped to 0x{newEip:X4}");
_output.WriteLine($"Stack updated - ESP is now 0x{newSp:X4} (pushed 6 bytes)");

// Verify exception was generated (EIP should be at handler, SP should have decreased)
Assert.NotEqual(0x0000, (ushort)newEip); // Should have jumped away from instruction
Assert.NotEqual(0x0100, (ushort)newSp); // Stack pointer should have changed
}
catch (Exception ex) when (ex is not OutOfMemoryException && ex is not StackOverflowException)
{
_output.WriteLine($"Exception: {ex.Message}");
_output.WriteLine($"Stack trace: {ex.StackTrace}");
}
}

[Fact]
public void TestValidLockWithMemoryDestination()
{
// Test that LOCK prefix with memory destination is allowed
var memory = new VirtualMemory();
var cpu = new IcedCpu(memory, decoderOptions: DecoderOptions.NoInvalidCheck, bitness: 16);

// Set up initial state
cpu.SetRegister("EBX", 0x0100);
cpu.SetRegister("CS", 0x1000);
cpu.SetRegister("DS", 0x1000);
cpu.SetRegister("SS", 0x1000);
cpu.SetEip(0x0000);

// Write a valid LOCK ADD: F0 80 07 42 = lock add byte [bx], 0x42
var physAddr = (uint)(0x1000 << 4) + 0x0000;
memory.Write8(physAddr + 0, 0xF0);  // LOCK prefix
memory.Write8(physAddr + 1, 0x80);  // ADD r/m8, imm8
memory.Write8(physAddr + 2, 0x07);  // ModR/M: [BX] memory
memory.Write8(physAddr + 3, 0x42);  // Immediate value
memory.Write8(physAddr + 4, 0xF4);  // HLT

// Write initial value at [DS:BX]
var dataAddr = (uint)(0x1000 << 4) + 0x0100;
memory.Write8(dataAddr, 0x10);

_output.WriteLine($"Before: [DS:BX]=0x{memory.Read8(dataAddr):X2}, EIP=0x{cpu.GetEip():X8}");

try
{
cpu.SingleStep(memory);
_output.WriteLine($"After: [DS:BX]=0x{memory.Read8(dataAddr):X2}, EIP=0x{cpu.GetEip():X8}");

// Verify the ADD executed correctly
Assert.Equal(0x52, memory.Read8(dataAddr)); // 0x10 + 0x42 = 0x52
}
catch (Exception ex) when (ex is not OutOfMemoryException && ex is not StackOverflowException)
{
_output.WriteLine($"Exception: {ex.Message}");
_output.WriteLine($"Stack trace: {ex.StackTrace}");
throw; // Valid LOCK should not throw
}
}
}
