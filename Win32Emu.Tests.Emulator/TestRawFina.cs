using Xunit;
using Xunit.Abstractions;
using Win32Emu.Tests.Emulator.SingleStepTests;
using System.Linq;

namespace Win32Emu.Tests.Emulator;

public class TestRawFina
{
    private readonly ITestOutputHelper _output;
    
    public TestRawFina(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void ShowRawFinalState()
    {
        var testFile = FindTestFile("00.MOO.gz");
        if (testFile == null)
        {
            _output.WriteLine("Test file not found");
            return;
        }
        
        // Parse the file but examine the raw state BEFORE merging
        var data = System.IO.File.ReadAllBytes(testFile);
        using var gzipStream = new System.IO.Compression.GZipStream(new System.IO.MemoryStream(data), System.IO.Compression.CompressionMode.Decompress);
        using var memoryStream = new System.IO.MemoryStream();
        gzipStream.CopyTo(memoryStream);
        var uncompressed = memoryStream.ToArray();
        
        // Now parse it
        var mooFile = MooFileParser.ParseBytes(uncompressed);
        var test = mooFile.Tests[0];
        
        _output.WriteLine($"Test: {test.Name}");
        _output.WriteLine($"\nInitial state memory count: {test.InitialState.Memory.Count}");
        _output.WriteLine($"Final state memory count (after merge): {test.FinalState.Memory.Count}");
        
        // The issue is that MergeFinalStateWithInitial has already been called
        // We can't see the raw FINA data here
        // But we can see what addresses are in the final list
        
        var initialAddresses = test.InitialState.Memory.Select(m => m.Address).ToHashSet();
        var finalAddresses = test.FinalState.Memory.Select(m => m.Address).ToHashSet();
        
        var newAddresses = finalAddresses.Except(initialAddresses).ToList();
        _output.WriteLine($"\nAddresses that are NEW in final state (came from FINA):");
        foreach (var addr in newAddresses.OrderBy(a => a))
        {
            var value = test.FinalState.Memory.First(m => m.Address == addr).Value;
            _output.WriteLine($"  [0x{addr:X8}] = 0x{value:X2}");
        }
        
        _output.WriteLine($"\nNow let me check the PresenceMask for final registers:");
        _output.WriteLine($"  PresenceMask: 0x{test.FinalState.Registers.PresenceMask:X8}");
        
        // Check which registers were present in FINA
        for (int i = 0; i < 20; i++)
        {
            if (test.FinalState.Registers.HasRegister(i))
            {
                string regName = i switch
                {
                    2 => "EAX",
                    3 => "EBX",
                    4 => "ECX",
                    5 => "EDX",
                    6 => "ESI",
                    7 => "EDI",
                    8 => "EBP",
                    9 => "ESP",
                    10 => "CS",
                    11 => "DS",
                    12 => "ES",
                    13 => "FS",
                    14 => "GS",
                    15 => "SS",
                    16 => "EIP",
                    17 => "EFLAGS",
                    _ => $"Reg{i}"
                };
                _output.WriteLine($"  Register {i} ({regName}) was present in FINA");
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
