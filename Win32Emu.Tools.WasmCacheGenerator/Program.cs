using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using Iced.Intel;
using Microsoft.Extensions.Logging;
using Win32Emu.Cpu.Jit;

namespace Win32Emu.Tools.WasmCacheGenerator;

/// <summary>
/// Generates WASM-compatible JIT cache metadata for x86 executables.
/// Pre-analyzes code blocks and stores metadata in JSON format for fast loading in WASM.
/// Does NOT use Roslyn compilation - only generates block metadata for IcedCpu.
/// </summary>
class Program
{
	// PE format constants
	private const int PE_HEADER_OFFSET_LOCATION = 0x3C; // Offset in DOS header to PE header offset
	private const int PE_HEADER_OFFSET_SIZE = 4; // Size of PE header offset field
	private const int PE_IMAGE_BASE_OFFSET = 0x34; // Offset in PE header to image base (PE32)
	private const int PE_ENTRY_POINT_RVA_OFFSET = 0x28; // Offset in PE header to entry point RVA
	
	// Default addresses
	private const uint DEFAULT_IMAGE_BASE = 0x400000; // Standard Win32 image base
	private const uint DEFAULT_ENTRY_POINT = 0x401000; // Default entry point if parsing fails
	private const uint COMMON_CODE_SECTION_OFFSET = 0x1000; // Common code section RVA
	
	// Block analysis constants
	private const int MAX_INSTRUCTIONS_PER_BLOCK = 50; // Maximum instructions to decode per block
	private const int MAX_BYTES_TO_DECODE = 1024; // Maximum bytes to decode per block
	
	static async Task<int> Main(string[] args)
	{
		if (args.Length < 1)
		{
			Console.WriteLine("Win32Emu WASM Cache Generator - Pre-analyze executables for WASM");
			Console.WriteLine();
			Console.WriteLine("Usage: Win32Emu.Tools.WasmCacheGenerator <executable.exe> [options]");
			Console.WriteLine();
			Console.WriteLine("Options:");
			Console.WriteLine("  --output <file>       Output JSON file (default: <exe>.wasm-cache.json)");
			Console.WriteLine("  --max-blocks <n>      Maximum number of blocks to analyze (default: 10000)");
			Console.WriteLine("  --verbose             Enable verbose logging");
			Console.WriteLine();
			Console.WriteLine("Output:");
			Console.WriteLine("  JSON file containing block metadata (addresses, sizes, hashes)");
			Console.WriteLine("  Compatible with WASM - no compilation, just metadata");
			Console.WriteLine();
			Console.WriteLine("Example:");
			Console.WriteLine("  Win32Emu.Tools.WasmCacheGenerator IGN_TEAS.EXE --output ign_teas.wasm-cache.json");
			Console.WriteLine();
			return 1;
		}

		var exePath = args[0];
		var outputFile = GetArgument(args, "--output", Path.ChangeExtension(exePath, ".wasm-cache.json"));
		var maxBlocksStr = GetArgument(args, "--max-blocks", "10000");
		var verbose = args.Contains("--verbose");

		// Create logger for error reporting
		using var loggerFactory = LoggerFactory.Create(builder =>
		{
			builder.AddConsole();
			if (verbose)
			{
				builder.SetMinimumLevel(LogLevel.Debug);
			}
		});
		var logger = loggerFactory.CreateLogger<Program>();

		if (!File.Exists(exePath))
		{
			logger.LogError("File not found: {ExePath}", exePath);
			Console.Error.WriteLine($"Error: File not found: {exePath}");
			return 1;
		}

		// Setup logging
		logger.LogInformation("Win32Emu WASM Cache Generator");
		logger.LogInformation("Executable: {Path}", exePath);
		logger.LogInformation("Output: {Output}", outputFile);

		try
		{
			var maxBlocks = int.Parse(maxBlocksStr);
			
			// Load the executable
			var exeBytes = await File.ReadAllBytesAsync(exePath);
			logger.LogInformation("Loaded {Size} bytes from executable", exeBytes.Length);

			// Parse PE header to find entry point
			var entryPoint = ParsePEEntryPoint(exeBytes);
			logger.LogInformation("Entry point: 0x{EntryPoint:X8}", entryPoint);

			// Analyze code blocks
			var analyzer = new BlockAnalyzer(exeBytes, logger);
			var blocks = await analyzer.AnalyzeBlocksAsync(entryPoint, maxBlocks);

			logger.LogInformation("Analyzed {Count} code blocks", blocks.Count);

			// Create cache data
			var cacheData = new JitCacheData
			{
				Version = 1,
				ExecutablePath = Path.GetFileName(exePath),
				Timestamp = DateTime.UtcNow,
				Blocks = blocks
			};

			// Serialize to JSON
			var options = new JsonSerializerOptions
			{
				WriteIndented = true,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};
			
			var json = JsonSerializer.Serialize(cacheData, options);
			await File.WriteAllTextAsync(outputFile, json);

			logger.LogInformation("Successfully wrote cache to: {Output}", outputFile);
			logger.LogInformation("File size: {Size} bytes", new FileInfo(outputFile).Length);
			logger.LogInformation("");
			logger.LogInformation("To use in WASM:");
			logger.LogInformation("  1. Copy {Output} to Win32Emu.Wasm/wwwroot/cache/", Path.GetFileName(outputFile));
			logger.LogInformation("  2. Emulator will auto-load cache for faster startup");

			return 0;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Analysis failed");
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
		if (exeBytes.Length < PE_HEADER_OFFSET_LOCATION + PE_HEADER_OFFSET_SIZE)
			return DEFAULT_ENTRY_POINT;

		var peOffset = BitConverter.ToInt32(exeBytes, PE_HEADER_OFFSET_LOCATION);
		if (peOffset < 0 || peOffset + PE_IMAGE_BASE_OFFSET >= exeBytes.Length)
			return DEFAULT_ENTRY_POINT;

		// Image base is at PE header + 0x34 (for PE32)
		var imageBase = BitConverter.ToUInt32(exeBytes, peOffset + PE_IMAGE_BASE_OFFSET);
		
		// Entry point RVA is at PE header + 0x28
		var entryPointRva = BitConverter.ToUInt32(exeBytes, peOffset + PE_ENTRY_POINT_RVA_OFFSET);
		
		// Return image base + entry point RVA
		return imageBase + entryPointRva;
	}
}

/// <summary>
/// Analyzes x86 code blocks and generates metadata without compilation
/// </summary>
class BlockAnalyzer
{
	private readonly byte[] _exeBytes;
	private readonly ILogger _logger;
	private readonly HashSet<uint> _analyzedAddresses = new();
	private readonly Queue<uint> _addressesToAnalyze = new();

	public BlockAnalyzer(byte[] exeBytes, ILogger logger)
	{
		_exeBytes = exeBytes;
		_logger = logger;
	}

	public async Task<List<BlockMetadata>> AnalyzeBlocksAsync(uint startAddress, int maxBlocks)
	{
		var blocks = new List<BlockMetadata>();
		_addressesToAnalyze.Enqueue(startAddress);

		while (_addressesToAnalyze.Count > 0 && blocks.Count < maxBlocks)
		{
			var address = _addressesToAnalyze.Dequeue();
			
			if (_analyzedAddresses.Contains(address))
				continue;

			try
			{
				var metadata = AnalyzeBlock(address);
				if (metadata != null)
				{
					blocks.Add(metadata);
					_analyzedAddresses.Add(address);

					if (blocks.Count % 100 == 0)
					{
						_logger.LogInformation("Analyzed {Count} blocks so far...", blocks.Count);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Failed to analyze block at 0x{Address:X8}", address);
			}
		}

		return await Task.FromResult(blocks);
	}

	private BlockMetadata? AnalyzeBlock(uint address)
	{
		// Disassemble instructions at this address
		var instructions = DisassembleBlock(address);
		
		if (instructions.Count == 0)
			return null;

		// Calculate byte length
		int byteLength = 0;
		foreach (var insn in instructions)
		{
			byteLength += insn.Length;
		}

		// Compute hash of the code bytes
		// Try to map from virtual address to file offset
		// For simplicity, assume image base 0x400000 and RVA = file offset for code section
		long offset = address - DEFAULT_IMAGE_BASE;
		if (offset < 0 || offset + byteLength > _exeBytes.Length)
		{
			// Try alternative: address might already be an RVA
			offset = address - COMMON_CODE_SECTION_OFFSET; // Common code section start
			if (offset < 0 || offset + byteLength > _exeBytes.Length)
				return null;
		}

		var codeBytes = new byte[byteLength];
		Array.Copy(_exeBytes, (int)offset, codeBytes, 0, byteLength);
		var hash = ComputeHash(codeBytes);

		// Check for terminating instructions
		var lastInsn = instructions[^1];
		bool endsWithCall = lastInsn.Mnemonic == Mnemonic.Call;
		bool endsWithReturn = lastInsn.Mnemonic == Mnemonic.Ret;
		uint? directTarget = null;

		// Queue targets for analysis - explicitly filter conditional jumps
		foreach (var insn in instructions.Where(i => 
			i.Mnemonic == Mnemonic.Call || 
			i.Mnemonic == Mnemonic.Jmp ||
			i.Mnemonic.ToString().StartsWith("J"))) // All conditional jumps
		{
			var target = GetBranchTarget(insn);
			if (target.HasValue && !_analyzedAddresses.Contains(target.Value))
			{
				_addressesToAnalyze.Enqueue(target.Value);
				
				if (insn == lastInsn && (insn.Mnemonic == Mnemonic.Call || insn.Mnemonic == Mnemonic.Jmp))
				{
					directTarget = target.Value;
				}
			}
		}

		return new BlockMetadata
		{
			StartAddress = address,
			InstructionCount = instructions.Count,
			ByteLength = byteLength,
			CodeHash = hash,
			FirstCompiled = DateTime.UtcNow,
			ExecutionCount = 0,
			EndsWithCall = endsWithCall,
			EndsWithReturn = endsWithReturn,
			DirectTarget = directTarget
		};
	}

	private List<Instruction> DisassembleBlock(uint address)
	{
		var instructions = new List<Instruction>();
		
		// Try to map from virtual address to file offset
		long offset = address - DEFAULT_IMAGE_BASE;
		if (offset < 0 || offset >= _exeBytes.Length)
		{
			// Try alternative mapping
			offset = address - COMMON_CODE_SECTION_OFFSET;
			if (offset < 0 || offset >= _exeBytes.Length)
				return instructions;
		}

		var codeReader = new ByteArrayCodeReader(_exeBytes, (int)offset, Math.Min(Program.MAX_BYTES_TO_DECODE, _exeBytes.Length - (int)offset));
		var decoder = Decoder.Create(32, codeReader);
		decoder.IP = address;

		// Decode until we hit a terminating instruction or max instructions
		int count = 0;

		while (count < Program.MAX_INSTRUCTIONS_PER_BLOCK && decoder.IP < address + Program.MAX_BYTES_TO_DECODE)
		{
			decoder.Decode(out var instruction);
			
			if (instruction.IsInvalid)
				break;
				
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

	private static string ComputeHash(byte[] bytes)
	{
		using var sha256 = SHA256.Create();
		var hashBytes = sha256.ComputeHash(bytes);
		return Convert.ToHexString(hashBytes);
	}
}

/// <summary>
/// Simple byte array code reader for Iced disassembler
/// </summary>
class ByteArrayCodeReader : CodeReader
{
	private readonly byte[] _data;
	private readonly int _startOffset;
	private int _offset;
	private readonly int _length;

	public ByteArrayCodeReader(byte[] data, int offset, int length)
	{
		_data = data;
		_startOffset = offset;
		_offset = offset;
		_length = length;
	}

	public override int ReadByte()
	{
		if (_offset >= _data.Length || _offset >= _startOffset + _length)
			return -1;

		return _data[_offset++];
	}
}
