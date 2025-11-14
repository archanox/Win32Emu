using Xunit;
using Xunit.Abstractions;
using Win32Emu.Tests.Emulator.SingleStepTests;
using System.Linq;

namespace Win32Emu.Tests.Emulator;

public class TestMooOrdering
{
    private readonly ITestOutputHelper _output;
    
    public TestMooOrdering(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void ShowFirstFewTests()
    {
        var testFile = FindTestFile("00.MOO.gz");
        if (testFile == null)
        {
            _output.WriteLine("Test file not found");
            return;
        }
        
        var mooFile = MooFileParser.Parse(testFile);
        
        _output.WriteLine($"Total tests in file: {mooFile.Tests.Count}");
        _output.WriteLine($"\nFirst 10 tests:");
        
        for (int i = 0; i < Math.Min(10, mooFile.Tests.Count); i++)
        {
            var test = mooFile.Tests[i];
            _output.WriteLine($"  [{i}] {test.Name}");
            _output.WriteLine($"      Instruction bytes: {BitConverter.ToString(test.InstructionBytes)}");
            
            // Check FINA memory
            var initialAddresses = test.InitialState.Memory.Select(m => m.Address).ToHashSet();
            var finalAddresses = test.FinalState.Memory.Select(m => m.Address).ToHashSet();
            var newAddresses = finalAddresses.Except(initialAddresses).ToList();
            
            if (newAddresses.Any())
            {
                _output.WriteLine($"      FINA memory writes:");
                foreach (var addr in newAddresses.Take(3))
                {
                    var value = test.FinalState.Memory.First(m => m.Address == addr).Value;
                    _output.WriteLine($"        [0x{addr:X8}] = 0x{value:X2}");
                }
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
