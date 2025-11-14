using Xunit;
using Xunit.Abstractions;
using Iced.Intel;

namespace Win32Emu.Tests.Emulator;

public class TestInstructionDecode
{
    private readonly ITestOutputHelper _output;
    
    public TestInstructionDecode(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void DecodeFirstInstruction()
    {
        var bytes = new byte[] { 0x00, 0x5E, 0x60, 0xF4 };
        
        var decoder = Decoder.Create(16, bytes);
        decoder.IP = 0x72A0;
        
        var insn = decoder.Decode();
        
        _output.WriteLine($"Mnemonic: {insn.Mnemonic}");
        _output.WriteLine($"OpCount: {insn.OpCount}");
        _output.WriteLine($"Length: {insn.Length}");
        _output.WriteLine($"ToString: {insn}");
        
        for (int i = 0; i < insn.OpCount; i++)
        {
            _output.WriteLine($"Op{i}: Kind={insn.GetOpKind(i)}");
            if (insn.GetOpKind(i) == OpKind.Memory)
            {
                _output.WriteLine($"  MemoryBase: {insn.MemoryBase}");
                _output.WriteLine($"  MemoryIndex: {insn.MemoryIndex}");
                _output.WriteLine($"  MemoryDisplacement: 0x{insn.MemoryDisplacement32:X}");
                _output.WriteLine($"  MemoryDisplSize: {insn.MemoryDisplSize}");
                _output.WriteLine($"  SegmentPrefix: {insn.SegmentPrefix}");
            }
            if (insn.GetOpKind(i) == OpKind.Register)
            {
                _output.WriteLine($"  Register: {insn.GetOpRegister(i)}");
            }
        }
    }
}
