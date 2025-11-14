using Xunit;
using Xunit.Abstractions;
using Win32Emu.Tests.Emulator.SingleStepTests;

namespace Win32Emu.Tests.Emulator;

public class TestSegmentValues
{
    private readonly ITestOutputHelper _output;
    
    public TestSegmentValues(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void ShowSegmentRegisters()
    {
        var testFile = FindTestFile("00.MOO.gz");
        if (testFile == null)
        {
            _output.WriteLine("Test file not found");
            return;
        }
        
        var mooFile = MooFileParser.Parse(testFile);
        
        // Test 0: add [ss:bp+60h],bl
        var test0 = mooFile.Tests[0];
        _output.WriteLine($"Test 0: {test0.Name}");
        _output.WriteLine($"  SS = 0x{test0.InitialState.Registers.Ss:X4}");
        _output.WriteLine($"  DS = 0x{test0.InitialState.Registers.Ds:X4}");
        _output.WriteLine($"  CS = 0x{test0.InitialState.Registers.Cs:X4}");
        _output.WriteLine($"  ES = 0x{test0.InitialState.Registers.Es:X4}");
        _output.WriteLine($"  BP = 0x{test0.InitialState.Registers.Ebp:X8} (16-bit: 0x{test0.InitialState.Registers.Ebp & 0xFFFF:X4})");
        _output.WriteLine($"  Offset = 0x60");
        
        var bp16 = (ushort)(test0.InitialState.Registers.Ebp & 0xFFFF);
        var offset = (ushort)((bp16 + 0x60) & 0xFFFF);
        _output.WriteLine($"  Effective offset = (BP + 0x60) & 0xFFFF = 0x{offset:X4}");
        
        var ss = test0.InitialState.Registers.Ss;
        var linearAddr = (ss << 4) + offset;
        _output.WriteLine($"  Linear address = (SS << 4) + offset = (0x{ss:X4} << 4) + 0x{offset:X4} = 0x{linearAddr:X8}");
        _output.WriteLine($"  Expected FINA address: 0x00000001");
        
        if (linearAddr == 0x00000001)
        {
            _output.WriteLine($"  MATCH!");
        }
        else
        {
            _output.WriteLine($"  MISMATCH!");
            _output.WriteLine($"  ");
            _output.WriteLine($"  Maybe SS should be 0 for flat addressing?");
            var flatAddr = offset;
            _output.WriteLine($"  If SS=0: Linear address = 0x{flatAddr:X8}");
            if (flatAddr == 0x0001)
            {
                _output.WriteLine($"    MISMATCH - still wrong!");
            }
        }
    }
    
    private string? FindTestFile(string fileName)
    {
        var searchPaths = new[]
        {
            Path.Combine("TestData", "SingleStepTests", fileName),
            Path.Combine("SingleStepTests", fileName),
            Path.Combine("..", "TestData", "SingleStepTests", fileName),
            fileName
        };
        
        foreach (var path in searchPaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }
        
        return null;
    }
}
