using AsmResolver.PE;
using System;
using System.Linq;

var exePath = "EXEs/ign_teas/IGN_TEAS.EXE";
Console.WriteLine($"Loading PE file: {exePath}");

var image = PEImage.FromFile(exePath);
var imports = image.Imports;

Console.WriteLine($"\nFound {imports.Count()} import modules:\n");

foreach (var module in imports)
{
    var dll = module.Name ?? "<unknown>";
    var symbolCount = module.Symbols.Count();
    Console.WriteLine($"  {dll} ({symbolCount} imports)");
    
    // Show first few imports from each DLL
    var samples = module.Symbols.Take(3);
    foreach (var sym in samples)
    {
        var name = sym.Name ?? $"Ordinal_{sym.Ordinal}";
        var rva = sym.AddressTableEntry?.Rva;
        Console.WriteLine($"    - {name} (IAT RVA: 0x{rva:X})");
    }
    if (symbolCount > 3)
    {
        Console.WriteLine($"    ... and {symbolCount - 3} more");
    }
    Console.WriteLine();
}
