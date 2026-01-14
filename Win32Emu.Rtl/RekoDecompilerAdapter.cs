using System.ComponentModel.Design;
using System.Text;
using Iced.Intel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Reko.Arch.X86;
using Reko.Core;
using Reko.Core.Memory;

namespace Win32Emu.Rtl;

/// <summary>
/// Decompiler adapter using Reko.Decompiler.Runtime.
/// 
/// PROOF OF CONCEPT - GPL licensing concerns waived for demonstration purposes.
/// This implementation directly uses Reko packages to decompile x86 instructions to RTL.
/// 
/// Note: Reko is licensed under GPLv2. This is a proof of concept implementation.
/// </summary>
public class RekoDecompilerAdapter : IDecompilerAdapter
{
	private readonly ILogger _logger;
	
	public string Name => "Reko";
	
	public bool IsAvailable => true; // Always available since we have direct dependency
	
	public string LicenseInfo => "GPLv2 - Reko Decompiler (Proof of Concept)";
	
	public RekoDecompilerAdapter(ILogger? logger = null)
	{
		_logger = logger ?? NullLogger.Instance;
		_logger.LogInformation("[RekoAdapter] Reko decompiler initialized (Proof of Concept - GPL licensing)");
	}
	
	public async Task<string> DecompileToCSharpAsync(uint startAddress, List<Instruction> instructions, string className)
	{
		try
		{
			_logger.LogInformation("[RekoAdapter] Decompiling {Count} instructions at 0x{Address:X8}", 
				instructions.Count, startAddress);
			
			var csharpCode = await DecompileUsingRekoAsync(startAddress, instructions, className);
			return csharpCode;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[RekoAdapter] Error during Reko decompilation");
			return GenerateFallbackStub(startAddress, instructions, className, ex.Message);
		}
	}
	
	private async Task<string> DecompileUsingRekoAsync(uint startAddress, List<Instruction> instructions, string className)
	{
		// Convert Iced.Intel instructions to byte array for Reko
		var instructionBytes = ConvertInstructionsToBytes(instructions);
		
		_logger.LogDebug("[RekoAdapter] Converted {Count} instructions to {ByteCount} bytes", 
			instructions.Count, instructionBytes.Length);
		
		// Create Reko components directly (no reflection!)
		var address = Address.Ptr32(startAddress);
		var memoryArea = new ByteMemoryArea(address, instructionBytes);
		var serviceContainer = new ServiceContainer();
		var arch = new X86ArchitectureFlat32(serviceContainer, "x86-protected-32", new Dictionary<string, object>());
		
		_logger.LogDebug("[RekoAdapter] Created Reko architecture: {Arch}", arch.Name);
		
		// Create ImageReader
		var imageReader = arch.CreateImageReader(memoryArea, address);
		
		// Create processor state (required for CreateRewriter)
		var state = arch.CreateProcessorState();
		
		// Create rewriter with required parameters
		// Note: We use null for IStorageBinder and IRewriterHost for simplicity in POC
		var rewriter = arch.CreateRewriter(imageReader, state, null!, null!);
		
		// Collect RTL instructions from rewriter
		var rtlInstructions = new List<string>();
		int instructionCount = 0;
		int maxInstructions = instructions.Count * 5; // Allow up to 5 RTL instructions per x86 instruction
		
		foreach (var rtlCluster in rewriter)
		{
			if (instructionCount >= maxInstructions)
				break;
				
			if (rtlCluster != null)
			{
				// Each cluster contains multiple RTL instructions
				var clusterStr = rtlCluster.ToString();
				if (!string.IsNullOrWhiteSpace(clusterStr))
				{
					rtlInstructions.Add(clusterStr);
					instructionCount++;
				}
			}
		}
		
		_logger.LogInformation("[RekoAdapter] Generated {Count} RTL instructions", rtlInstructions.Count);
		
		// Generate C# code from RTL instructions
		return await Task.FromResult(GenerateCSharpFromRtl(startAddress, rtlInstructions, className));
	}
	
	private byte[] ConvertInstructionsToBytes(List<Instruction> instructions)
	{
		// Use Iced.Intel's Encoder to convert instructions back to bytes
		var codeWriter = new CodeWriterImpl();
		var encoder = Iced.Intel.Encoder.Create(32, codeWriter); // 32-bit mode
		
		foreach (var instruction in instructions)
		{
			// Encode the instruction to bytes
			encoder.Encode(instruction, instruction.IP);
		}
		
		return codeWriter.ToArray();
	}
	
	// Helper class for Iced.Intel encoding
	private class CodeWriterImpl : Iced.Intel.CodeWriter
	{
		private readonly List<byte> _bytes = new();
		
		public override void WriteByte(byte value) => _bytes.Add(value);
		
		public byte[] ToArray() => _bytes.ToArray();
	}
	
	private string GenerateCSharpFromRtl(uint startAddress, List<string> rtlInstructions, string className)
	{
		var sb = new StringBuilder();
		
		// File header
		sb.AppendLine("using System;");
		sb.AppendLine("using System.Threading.Tasks;");
		sb.AppendLine();
		sb.AppendLine("namespace Win32Emu.Generated");
		sb.AppendLine("{");
		sb.AppendLine($"\t// Decompiled using Reko (GPLv2) - {LicenseInfo}");
		sb.AppendLine($"\t// Note: This code is subject to GPLv2 licensing requirements");
		sb.AppendLine($"\tpublic class {className}");
		sb.AppendLine("\t{");
		sb.AppendLine($"\t\t// Block at 0x{startAddress:X8}");
		sb.AppendLine($"\t\t// Reko RTL instructions: {rtlInstructions.Count}");
		sb.AppendLine();
		sb.AppendLine("\t\tpublic async Task<dynamic> Execute(dynamic cpu, dynamic mem)");
		sb.AppendLine("\t\t{");
		
		// Add RTL instructions as comments and attempt basic conversion
		sb.AppendLine("\t\t\t// Reko RTL representation:");
		foreach (var rtl in rtlInstructions.Take(50)) // Limit for readability
		{
			sb.AppendLine($"\t\t\t// {rtl}");
		}
		
		if (rtlInstructions.Count > 50)
		{
			sb.AppendLine($"\t\t\t// ... and {rtlInstructions.Count - 50} more RTL instructions");
		}
		
		sb.AppendLine();
		sb.AppendLine("\t\t\t// TODO: Convert Reko RTL to executable C# code");
		sb.AppendLine("\t\t\t// This requires mapping RTL operations to CPU state modifications");
		sb.AppendLine("\t\t\tthrow new NotImplementedException(\"RTL to C# conversion not yet implemented\");");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t}");
		sb.AppendLine("}");
		
		return sb.ToString();
	}
	
	private string GenerateFallbackStub(uint startAddress, List<Instruction> instructions, string className, string errorMessage)
	{
		var sb = new StringBuilder();
		sb.AppendLine("using System;");
		sb.AppendLine("using System.Threading.Tasks;");
		sb.AppendLine();
		sb.AppendLine("namespace Win32Emu.Generated");
		sb.AppendLine("{");
		sb.AppendLine($"\t// Decompilation attempted using Reko (GPLv2) but failed");
		sb.AppendLine($"\t// Error: {errorMessage}");
		sb.AppendLine($"\tpublic class {className}");
		sb.AppendLine("\t{");
		sb.AppendLine($"\t\t// Block at 0x{startAddress:X8}");
		sb.AppendLine($"\t\t// Contains {instructions.Count} x86 instructions");
		sb.AppendLine();
		sb.AppendLine("\t\tpublic async Task<dynamic> Execute(dynamic cpu, dynamic mem)");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\t// Reko decompilation failed, fallback needed");
		sb.AppendLine("\t\t\tthrow new NotImplementedException(\"Reko integration failed\");");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t}");
		sb.AppendLine("}");
		
		return sb.ToString();
	}
}
