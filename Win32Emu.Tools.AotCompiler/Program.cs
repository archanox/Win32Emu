using System.Diagnostics;
using Iced.Intel;
using Microsoft.Extensions.Logging;
using Win32Emu.Rtl;
using Win32Emu;

namespace Win32Emu.Tools.AotCompiler;

/// <summary>
/// Ahead-of-Time (AoT) compiler for Win32Emu.
/// Pre-compiles executable code blocks to C# and assemblies for faster startup.
/// Enables debugging game code in the scope of the emulator with readable C# source.
/// </summary>
class Program
{
    static async Task<int> Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Win32Emu AoT Compiler - Ahead-of-Time JIT Compilation");
            Console.WriteLine();
            Console.WriteLine("Usage: Win32Emu.Tools.AotCompiler <executable.exe> [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --output <dir>        Output directory for compiled cache (default: ./AotCache)");
            Console.WriteLine("  --advanced-opt        Enable advanced optimizations (loop unrolling, SIMD, inlining)");
            Console.WriteLine("  --start-address <hex> Starting address to scan from (default: entry point)");
            Console.WriteLine("  --max-blocks <n>      Maximum number of blocks to compile (default: unlimited)");
            Console.WriteLine("  --verbose             Enable verbose logging");
            Console.WriteLine();
            Console.WriteLine("Output:");
            Console.WriteLine("  - C# source files in <output>/Source/");
            Console.WriteLine("  - Compiled assemblies in <output>/");
            Console.WriteLine("  - Debug symbols for stepping through game code");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("  Win32Emu.Tools.AotCompiler game.exe --output ./GameCache --advanced-opt");
            Console.WriteLine();
            return 1;
        }

        var exePath = args[0];
        var outputDir = GetArgument(args, "--output", "./AotCache");
        var advancedOpt = args.Contains("--advanced-opt");
        var startAddressStr = GetArgument(args, "--start-address", null);
        var maxBlocksStr = GetArgument(args, "--max-blocks", null);
        var verbose = args.Contains("--verbose");

        if (!File.Exists(exePath))
        {
            Console.Error.WriteLine($"Error: File not found: {exePath}");
            return 1;
        }

        // Setup logging
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(verbose ? LogLevel.Debug : LogLevel.Information);
        });

        var logger = loggerFactory.CreateLogger<Program>();

        logger.LogInformation("Win32Emu AoT Compiler");
        logger.LogInformation("Executable: {Path}", exePath);
        logger.LogInformation("Output: {Dir}", outputDir);
        logger.LogInformation("Advanced Optimizations: {Enabled}", advancedOpt ? "Enabled" : "Disabled");

        try
        {
            // Load the executable
            var exeBytes = await File.ReadAllBytesAsync(exePath);
            logger.LogInformation("Loaded {Size} bytes from executable", exeBytes.Length);

            // Parse PE header to find entry point
            var entryPoint = ParsePEEntryPoint(exeBytes);
            logger.LogInformation("Entry point: 0x{EntryPoint:X8}", entryPoint);

            var startAddress = startAddressStr != null 
                ? Convert.ToUInt32(startAddressStr, 16) 
                : entryPoint;

            var maxBlocks = maxBlocksStr != null ? int.Parse(maxBlocksStr) : int.MaxValue;

            // Create RTL JIT cache
            var rtlCache = new RtlJitCache(outputDir, logger);

            // Scan and compile code blocks
            var compiler = new AotCompiler(rtlCache, exeBytes, logger, advancedOpt);
            var compiledCount = await compiler.CompileAllBlocksAsync(startAddress, maxBlocks);

            logger.LogInformation("Successfully compiled {Count} blocks", compiledCount);
            logger.LogInformation("Output written to: {Dir}", outputDir);
            logger.LogInformation("C# source available in: {SourceDir}", Path.Combine(outputDir, "Source"));
            logger.LogInformation("");
            logger.LogInformation("To debug:");
            logger.LogInformation("  1. Open assemblies in dnSpy or Visual Studio");
            logger.LogInformation("  2. Set breakpoints in generated Execute methods");
            logger.LogInformation("  3. Run Win32Emu with --cache-dir {Dir}", outputDir);
            logger.LogInformation("  4. Step through game code in debugger");

            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Compilation failed");
            return 1;
        }
    }

    static string? GetArgument(string[] args, string name, string? defaultValue)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
            {
                return args[i + 1];
            }
        }
        return defaultValue;
    }

    static uint ParsePEEntryPoint(byte[] exeBytes)
    {
        // Simple PE parser - find entry point from PE header
        if (exeBytes.Length < 0x3C + 4)
            return 0x401000; // Default

        var peOffset = BitConverter.ToInt32(exeBytes, 0x3C);
        if (peOffset < 0 || peOffset + 0x28 >= exeBytes.Length)
            return 0x401000;

        // Entry point RVA is at PE header + 0x28
        var entryPointRva = BitConverter.ToUInt32(exeBytes, peOffset + 0x28);
        
        // Add image base (typically 0x400000 for 32-bit)
        return 0x400000 + entryPointRva;
    }
}

/// <summary>
/// Core AoT compilation logic
/// </summary>
class AotCompiler
{
    private readonly RtlJitCache _rtlCache;
    private readonly byte[] _exeBytes;
    private readonly ILogger _logger;
    private readonly bool _advancedOptimizations;
    private readonly HashSet<uint> _compiledAddresses = new();
    private readonly Queue<uint> _addressesToCompile = new();

    public AotCompiler(RtlJitCache rtlCache, byte[] exeBytes, ILogger logger, bool advancedOpt)
    {
        _rtlCache = rtlCache;
        _exeBytes = exeBytes;
        _logger = logger;
        _advancedOptimizations = advancedOpt;
    }

    public async Task<int> CompileAllBlocksAsync(uint startAddress, int maxBlocks)
    {
        _addressesToCompile.Enqueue(startAddress);
        int compiledCount = 0;

        while (_addressesToCompile.Count > 0 && compiledCount < maxBlocks)
        {
            var address = _addressesToCompile.Dequeue();
            
            if (_compiledAddresses.Contains(address))
                continue;

            try
            {
                await CompileBlockAsync(address);
                _compiledAddresses.Add(address);
                compiledCount++;

                if (compiledCount % 100 == 0)
                {
                    _logger.LogInformation("Compiled {Count} blocks so far...", compiledCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to compile block at 0x{Address:X8}", address);
            }
        }

        return compiledCount;
    }

    private async Task CompileBlockAsync(uint address)
    {
        // Disassemble instructions at this address
        var instructions = DisassembleBlock(address);
        
        if (instructions.Count == 0)
            return;

        // Compile through RTL pipeline
        _rtlCache.CompileBlock(address, instructions);

        // Queue targets for compilation
        foreach (var insn in instructions)
        {
            if (insn.Mnemonic == Mnemonic.Call || 
                insn.Mnemonic == Mnemonic.Jmp ||
                insn.Mnemonic.ToString().StartsWith("J")) // All conditional jumps
            {
                var target = GetBranchTarget(insn);
                if (target.HasValue && !_compiledAddresses.Contains(target.Value))
                {
                    _addressesToCompile.Enqueue(target.Value);
                }
            }
        }

        await Task.CompletedTask;
    }

    private List<Instruction> DisassembleBlock(uint address)
    {
        var instructions = new List<Instruction>();
        
        // Simple virtual memory - map exe bytes at base address 0x400000
        var offset = address - 0x400000;
        if (offset < 0 || offset >= _exeBytes.Length)
            return instructions;

        var codeReader = new ByteArrayCodeReader(_exeBytes, (int)offset, Math.Min(1024, _exeBytes.Length - (int)offset));
        var decoder = Decoder.Create(32, codeReader);
        decoder.IP = address;

        // Decode until we hit a terminating instruction or max instructions
        const int maxInstructions = 50;
        int count = 0;

        while (count < maxInstructions && decoder.IP < address + 1024)
        {
            decoder.Decode(out var instruction);
            instructions.Add(instruction);
            count++;

            // Stop at block terminators
            if (instruction.Mnemonic == Mnemonic.Ret ||
                instruction.Mnemonic == Mnemonic.Jmp ||
                instruction.Mnemonic == Mnemonic.Int)
            {
                break;
            }

            // Stop at conditional branches (end of basic block)
            if (instruction.Mnemonic.ToString().StartsWith("J") && instruction.Mnemonic != Mnemonic.Jmp)
            {
                break;
            }
        }

        return instructions;
    }

    private uint? GetBranchTarget(Instruction instruction)
    {
        if (instruction.Op0Kind == OpKind.NearBranch32 || instruction.Op0Kind == OpKind.NearBranch16)
        {
            return (uint)instruction.NearBranchTarget;
        }

        return null;
    }
}

/// <summary>
/// Simple byte array code reader for Iced disassembler
/// </summary>
class ByteArrayCodeReader : CodeReader
{
    private readonly byte[] _data;
    private int _offset;
    private readonly int _length;

    public ByteArrayCodeReader(byte[] data, int offset, int length)
    {
        _data = data;
        _offset = offset;
        _length = length;
    }

    public override int ReadByte()
    {
        if (_offset >= _data.Length || _offset - (_offset - _length) >= _length)
            return -1;

        return _data[_offset++];
    }
}
