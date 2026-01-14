using System.Text;
using Iced.Intel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Rtl;

/// <summary>
/// Decompiler adapter using Reko.Decompiler.Runtime.
/// 
/// IMPORTANT LICENSING NOTICE:
/// This adapter uses Reko, which is licensed under GPLv2.
/// Using this adapter may require your application to comply with GPLv2.
/// 
/// By default, this adapter is NOT used unless explicitly enabled.
/// The default CustomRtlDecompilerAdapter is MIT-licensed and compatible with Win32Emu's license.
/// 
/// To use this adapter:
/// 1. Add NuGet reference: Reko.Decompiler.Runtime
/// 2. Set environment variable: WIN32EMU_USE_REKO=true
/// 3. Ensure your project complies with GPLv2 licensing requirements
/// 
/// See: https://github.com/uxmal/reko for Reko licensing details
/// </summary>
public class RekoDecompilerAdapter : IDecompilerAdapter
{
	private readonly ILogger _logger;
	private readonly bool _isEnabled;
	private readonly bool _rekoAvailable;
	
	public string Name => "Reko";
	
	public bool IsAvailable => _isEnabled && _rekoAvailable;
	
	public string LicenseInfo => "GPLv2 - Reko Decompiler (https://github.com/uxmal/reko)";
	
	public RekoDecompilerAdapter(ILogger? logger = null)
	{
		_logger = logger ?? NullLogger.Instance;
		
		// Check if explicitly enabled via environment variable
		var enableReko = Environment.GetEnvironmentVariable("WIN32EMU_USE_REKO");
		_isEnabled = enableReko?.ToLowerInvariant() == "true";
		
		// Check if Reko assemblies are available
		_rekoAvailable = CheckRekoAvailability();
		
		if (_isEnabled && !_rekoAvailable)
		{
			_logger.LogWarning(
				"[RekoAdapter] Reko decompiler is enabled but Reko.Decompiler.Runtime package is not available. " +
				"Add NuGet package: Reko.Decompiler.Runtime");
		}
		else if (_isEnabled && _rekoAvailable)
		{
			_logger.LogInformation(
				"[RekoAdapter] Reko decompiler is enabled. " +
				"Note: Reko is GPLv2 licensed. Ensure your project complies with GPL requirements.");
		}
	}
	
	public async Task<string> DecompileToCSharpAsync(uint startAddress, List<Instruction> instructions, string className)
	{
		if (!IsAvailable)
		{
			throw new InvalidOperationException(
				"Reko decompiler adapter is not available. " +
				"Set WIN32EMU_USE_REKO=true and add Reko.Decompiler.Runtime NuGet package.");
		}
		
		try
		{
			// This is where Reko integration would go
			// For now, provide a placeholder that explains the integration approach
			return await GenerateRekoIntegrationStubAsync(startAddress, instructions, className);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[RekoAdapter] Failed to decompile using Reko");
			throw;
		}
	}
	
	private bool CheckRekoAvailability()
	{
		try
		{
			// Try to load Reko assemblies using reflection
			// This way we don't require a hard dependency on Reko
			var rekoCore = Type.GetType("Reko.Core.Address, Reko.Core");
			var rekoArch = Type.GetType("Reko.Arch.X86.X86ArchitectureFlat32, Reko.Arch.X86");
			
			return rekoCore != null && rekoArch != null;
		}
		catch
		{
			return false;
		}
	}
	
	private async Task<string> GenerateRekoIntegrationStubAsync(uint startAddress, List<Instruction> instructions, string className)
	{
		try
		{
			// Use reflection to call Reko API without hard dependency
			var csharpCode = await DecompileUsingRekoAsync(startAddress, instructions, className);
			return csharpCode;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[RekoAdapter] Error during Reko decompilation, falling back to stub");
			return GenerateFallbackStub(startAddress, instructions, className, ex.Message);
		}
	}
	
	private async Task<string> DecompileUsingRekoAsync(uint startAddress, List<Instruction> instructions, string className)
	{
		// Convert Iced.Intel instructions to byte array for Reko
		var instructionBytes = ConvertInstructionsToBytes(instructions);
		
		// Use reflection to call Reko API
		var addressType = Type.GetType("Reko.Core.Address, Reko.Core") 
			?? throw new InvalidOperationException("Reko.Core.Address type not found");
		var memoryAreaType = Type.GetType("Reko.Core.Memory.ByteMemoryArea, Reko.Core") 
			?? throw new InvalidOperationException("Reko.Core.Memory.ByteMemoryArea type not found");
		var archType = Type.GetType("Reko.Arch.X86.X86ArchitectureFlat32, Reko.Arch.X86") 
			?? throw new InvalidOperationException("Reko.Arch.X86.X86ArchitectureFlat32 type not found");
		var serviceContainerType = Type.GetType("System.ComponentModel.Design.ServiceContainer, System.ComponentModel.TypeConverter") 
			?? throw new InvalidOperationException("ServiceContainer type not found");
		
		// Create Address instance: Address.Ptr32(startAddress)
		var ptr32Method = addressType.GetMethod("Ptr32", new[] { typeof(uint) })
			?? throw new InvalidOperationException("Address.Ptr32 method not found");
		var address = ptr32Method.Invoke(null, new object[] { startAddress });
		
		// Create ByteMemoryArea: new ByteMemoryArea(address, bytes)
		var memoryAreaCtor = memoryAreaType.GetConstructor(new[] { addressType, typeof(byte[]) })
			?? throw new InvalidOperationException("ByteMemoryArea constructor not found");
		var memoryArea = memoryAreaCtor.Invoke(new[] { address, instructionBytes });
		
		// Create ServiceContainer
		var serviceContainer = Activator.CreateInstance(serviceContainerType)
			?? throw new InvalidOperationException("Failed to create ServiceContainer");
		
		// Create X86ArchitectureFlat32: new X86ArchitectureFlat32(serviceContainer, "x86-protected-32")
		var archCtor = archType.GetConstructor(new[] { serviceContainerType, typeof(string) })
			?? throw new InvalidOperationException("X86ArchitectureFlat32 constructor not found");
		var arch = archCtor.Invoke(new[] { serviceContainer, "x86-protected-32" });
		
		// Create ImageReader: arch.CreateImageReader(memoryArea, address)
		var createImageReaderMethod = archType.GetMethod("CreateImageReader", new[] { memoryAreaType, addressType })
			?? throw new InvalidOperationException("CreateImageReader method not found");
		var imageReader = createImageReaderMethod.Invoke(arch, new[] { memoryArea, address });
		
		// Create Rewriter: arch.CreateRewriter(imageReader)
		var createRewriterMethod = archType.GetMethod("CreateRewriter", new[] { imageReader!.GetType().GetInterfaces()[0] })
			?? throw new InvalidOperationException("CreateRewriter method not found");
		var rewriter = createRewriterMethod.Invoke(arch, new[] { imageReader });
		
		// Collect RTL instructions from rewriter
		var rtlInstructions = new List<string>();
		var enumerator = ((System.Collections.IEnumerable)rewriter!).GetEnumerator();
		int instructionCount = 0;
		while (enumerator.MoveNext() && instructionCount < instructions.Count * 3) // Limit to avoid infinite loops
		{
			var rtlCluster = enumerator.Current;
			if (rtlCluster != null)
			{
				rtlInstructions.Add(rtlCluster.ToString() ?? "");
				instructionCount++;
			}
		}
		
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
