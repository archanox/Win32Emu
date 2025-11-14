using Xunit;
using Xunit.Abstractions;
using Win32Emu.Tests.Emulator.SingleStepTests;

namespace Win32Emu.Tests.Emulator;

public class TestRegValues
{
    private readonly ITestOutputHelper _output;
    
    public TestRegValues(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void ShowRegisterValuesForTests()
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
        _output.WriteLine($"  BP = 0x{test0.InitialState.Registers.Ebp:X8} (16-bit: 0x{test0.InitialState.Registers.Ebp & 0xFFFF:X4})");
        _output.WriteLine($"  Offset = 0x60");
        _output.WriteLine($"  Expected address: BP + 0x60 = 0x{((test0.InitialState.Registers.Ebp & 0xFFFF) + 0x60) & 0xFFFF:X4}");
        _output.WriteLine($"  Actual FINA address: 0x00000001");
        _output.WriteLine($"  Mismatch!");
        _output.WriteLine("");
        
        // Test 3: add [ds:bx+si],al
        var test3 = mooFile.Tests[3];
        _output.WriteLine($"Test 3: {test3.Name}");
        _output.WriteLine($"  BX = 0x{test3.InitialState.Registers.Ebx:X8} (16-bit: 0x{test3.InitialState.Registers.Ebx & 0xFFFF:X4})");
        _output.WriteLine($"  SI = 0x{test3.InitialState.Registers.Esi:X8} (16-bit: 0x{test3.InitialState.Registers.Esi & 0xFFFF:X4})");
        _output.WriteLine($"  Expected address: BX + SI = 0x{((test3.InitialState.Registers.Ebx & 0xFFFF) + (test3.InitialState.Registers.Esi & 0xFFFF)) & 0xFFFF:X4}");
        _output.WriteLine($"  Actual FINA address: 0x00000001");
        
        var calcAddr3 = ((test3.InitialState.Registers.Ebx & 0xFFFF) + (test3.InitialState.Registers.Esi & 0xFFFF)) & 0xFFFF;
        if (calcAddr3 == 0x0001)
        {
            _output.WriteLine($"  MATCH!");
        }
        else
        {
            _output.WriteLine($"  Mismatch!");
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
        
        return searchPaths.FirstOrDefault(File.Exists);
    }
}
