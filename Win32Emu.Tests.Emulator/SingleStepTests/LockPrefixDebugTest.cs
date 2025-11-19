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
public void TestLockMov()
{
var memory = new VirtualMemory();
var cpu = new IcedCpu(memory, decoderOptions: DecoderOptions.NoInvalidCheck, bitness: 16);

// Set up initial state
cpu.SetRegister("EAX", 0x12345678);
cpu.SetRegister("CS", 0x1000);
cpu.SetRegister("DS", 0x1000);
cpu.SetRegister("ES", 0x1000);
cpu.SetRegister("SS", 0x1000);
cpu.SetEip(0x0000);

// Write a LOCK MOV instruction: F0 A1 00 00 = lock mov ax,[0000]
var physAddr = (uint)(0x1000 << 4) + 0x0000;  // CS:IP = 1000:0000
memory.Write8(physAddr + 0, 0xF0);  // LOCK prefix
memory.Write8(physAddr + 1, 0xA1);  // MOV AX, [moffs]
memory.Write8(physAddr + 2, 0x00);  // offset low
memory.Write8(physAddr + 3, 0x00);  // offset high
memory.Write8(physAddr + 4, 0xF4);  // HLT

// Write some data to read
memory.Write16(0x10000, 0xABCD);

_output.WriteLine($"Before: EAX=0x{cpu.GetRegister("EAX"):X8}, EIP=0x{cpu.GetEip():X8}");

try
{
cpu.SingleStep(memory);
_output.WriteLine($"After: EAX=0x{cpu.GetRegister("EAX"):X8}, EIP=0x{cpu.GetEip():X8}");

// Check if AX (lower 16 bits of EAX) was updated
var eax = cpu.GetRegister("EAX");
var ax = (ushort)(eax & 0xFFFF);
_output.WriteLine($"AX value: 0x{ax:X4}, expected: 0xABCD");
}
catch (Exception ex)
{
_output.WriteLine($"Exception: {ex.Message}");
_output.WriteLine($"Stack trace: {ex.StackTrace}");
}
}
}
