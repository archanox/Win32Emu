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
		
		// Collect RTL instruction clusters from rewriter
		var rtlClusters = new List<object>();
		int clusterCount = 0;
		int maxClusters = instructions.Count * 2; // Allow up to 2 clusters per x86 instruction
		
		foreach (var rtlCluster in rewriter)
		{
			if (clusterCount >= maxClusters)
				break;
				
			if (rtlCluster != null)
			{
				rtlClusters.Add(rtlCluster);
				clusterCount++;
			}
		}
		
		_logger.LogInformation("[RekoAdapter] Generated {Count} RTL clusters", rtlClusters.Count);
		
		// Generate C# code from RTL clusters
		return await Task.FromResult(GenerateCSharpFromRtlClusters(startAddress, rtlClusters, className));
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
	
	private string GenerateCSharpFromRtlClusters(uint startAddress, List<object> rtlClusters, string className)
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
		sb.AppendLine($"\t\t// Reko RTL clusters: {rtlClusters.Count}");
		sb.AppendLine();
		sb.AppendLine("\t\tpublic async Task<dynamic> Execute(dynamic cpu, dynamic mem)");
		sb.AppendLine("\t\t{");
		
		// Initialize local variables for CPU registers
		sb.AppendLine("\t\t\t// Initialize CPU register state");
		sb.AppendLine("\t\t\tuint eax = cpu.EAX;");
		sb.AppendLine("\t\t\tuint ebx = cpu.EBX;");
		sb.AppendLine("\t\t\tuint ecx = cpu.ECX;");
		sb.AppendLine("\t\t\tuint edx = cpu.EDX;");
		sb.AppendLine("\t\t\tuint esi = cpu.ESI;");
		sb.AppendLine("\t\t\tuint edi = cpu.EDI;");
		sb.AppendLine("\t\t\tuint esp = cpu.ESP;");
		sb.AppendLine("\t\t\tuint ebp = cpu.EBP;");
		sb.AppendLine("\t\t\tuint eip = cpu.EIP;");
		sb.AppendLine("\t\t\tbool CF = cpu.CF;");
		sb.AppendLine("\t\t\tbool ZF = cpu.ZF;");
		sb.AppendLine("\t\t\tbool SF = cpu.SF;");
		sb.AppendLine("\t\t\tbool OF = cpu.OF;");
		sb.AppendLine("\t\t\tbool PF = cpu.PF;");
		sb.AppendLine();
		
		// Convert RTL clusters to C# code
		int clusterIndex = 0;
		foreach (var cluster in rtlClusters)
		{
			sb.AppendLine($"\t\t\t// RTL Cluster {clusterIndex++}");
			
			// Use reflection to access cluster's Instructions property
			var clusterType = cluster.GetType();
			var instructionsProperty = clusterType.GetProperty("Instructions");
			
			if (instructionsProperty != null)
			{
				var instructions = instructionsProperty.GetValue(cluster) as System.Collections.IEnumerable;
				if (instructions != null)
				{
					foreach (var rtlInstruction in instructions)
					{
						var convertedCode = ConvertRtlInstructionToCSharp(rtlInstruction);
						if (!string.IsNullOrWhiteSpace(convertedCode))
						{
							sb.AppendLine($"\t\t\t{convertedCode}");
						}
					}
				}
			}
			else
			{
				// Fallback: just add as comment
				sb.AppendLine($"\t\t\t// {cluster.ToString()}");
			}
			
			sb.AppendLine();
		}
		
		// Write back CPU register state
		sb.AppendLine("\t\t\t// Write back CPU register state");
		sb.AppendLine("\t\t\tcpu.EAX = eax;");
		sb.AppendLine("\t\t\tcpu.EBX = ebx;");
		sb.AppendLine("\t\t\tcpu.ECX = ecx;");
		sb.AppendLine("\t\t\tcpu.EDX = edx;");
		sb.AppendLine("\t\t\tcpu.ESI = esi;");
		sb.AppendLine("\t\t\tcpu.EDI = edi;");
		sb.AppendLine("\t\t\tcpu.ESP = esp;");
		sb.AppendLine("\t\t\tcpu.EBP = ebp;");
		sb.AppendLine("\t\t\tcpu.EIP = eip;");
		sb.AppendLine("\t\t\tcpu.CF = CF;");
		sb.AppendLine("\t\t\tcpu.ZF = ZF;");
		sb.AppendLine("\t\t\tcpu.SF = SF;");
		sb.AppendLine("\t\t\tcpu.OF = OF;");
		sb.AppendLine("\t\t\tcpu.PF = PF;");
		sb.AppendLine();
		sb.AppendLine("\t\t\treturn await Task.FromResult<dynamic>(new { IsCall = false });");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t}");
		sb.AppendLine("}");
		
		return sb.ToString();
	}
	
	private string ConvertRtlInstructionToCSharp(object rtlInstruction)
	{
		if (rtlInstruction == null)
			return string.Empty;
			
		var instrString = rtlInstruction.ToString() ?? "";
		
		// Parse common RTL patterns and convert to C#
		// Pattern: "dst = src" (assignment)
		if (instrString.Contains(" = "))
		{
			var parts = instrString.Split(new[] { " = " }, StringSplitOptions.None);
			if (parts.Length == 2)
			{
				var dst = parts[0].Trim();
				var src = parts[1].Trim();
				
				// Convert Reko register names to lowercase variables
				dst = ConvertRekoOperandToCSharp(dst);
				src = ConvertRekoOperandToCSharp(src);
				
				return $"{dst} = {src};";
			}
		}
		
		// Pattern: "branch target (condition)" - control flow
		if (instrString.Contains("branch") || instrString.Contains("goto"))
		{
			return $"// Control flow: {instrString}";
		}
		
		// Pattern: "call target" - function call
		if (instrString.Contains("call"))
		{
			return $"// Function call: {instrString}";
		}
		
		// Pattern: "return" - function return
		if (instrString.Contains("return"))
		{
			return "// return;";
		}
		
		// Fallback: add as comment for manual review
		return $"// {instrString}";
	}
	
	private string ConvertRekoOperandToCSharp(string operand)
	{
		// Remove Reko-specific syntax
		operand = operand.Trim();
		
		// Handle memory access: Mem0[address:type] -> mem.Read32(address)
		if (operand.StartsWith("Mem") && operand.Contains("["))
		{
			var match = System.Text.RegularExpressions.Regex.Match(operand, @"Mem\d+\[([^:]+):(\w+)\]");
			if (match.Success)
			{
				var address = ConvertRekoOperandToCSharp(match.Groups[1].Value);
				var type = match.Groups[2].Value;
				
				// Map Reko types to memory read operations
				return type.ToLower() switch
				{
					"word32" => $"mem.Read32({address})",
					"word16" => $"mem.Read16({address})",
					"byte" => $"mem.Read8({address})",
					_ => $"mem.Read32({address})" // Default to 32-bit
				};
			}
		}
		
		// Handle constants: 0x00000004<32> -> 0x00000004u
		if (operand.Contains("<") && operand.Contains(">"))
		{
			var constMatch = System.Text.RegularExpressions.Regex.Match(operand, @"(0x[0-9A-Fa-f]+)<\d+>");
			if (constMatch.Success)
			{
				return constMatch.Groups[1].Value + "u";
			}
		}
		
		// Handle register names (lowercase them for C# variables)
		operand = operand.ToLower();
		
		// Handle arithmetic operations
		operand = operand.Replace(" + ", " + ");
		operand = operand.Replace(" - ", " - ");
		operand = operand.Replace(" * ", " * ");
		operand = operand.Replace(" / ", " / ");
		
		return operand;
	}
	
	private string GenerateCSharpFromRtl(uint startAddress, List<string> rtlInstructions, string className)
	{
		// This method is kept for backward compatibility but is no longer the primary method
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
		
		// Add RTL instructions as comments
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
		sb.AppendLine("\t\t\t// Note: Use GenerateCSharpFromRtlClusters for executable code generation");
		sb.AppendLine("\t\t\tthrow new NotImplementedException(\"Use cluster-based generation\");");
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
