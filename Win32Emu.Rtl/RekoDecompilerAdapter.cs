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
		// This is a placeholder showing how Reko integration would work
		// Actual implementation would use Reko's API to:
		// 1. Create Reko.Core.Architecture instance for x86
		// 2. Load instructions into Reko's MemoryArea
		// 3. Use Reko's Rewriter to convert to RTL
		// 4. Use Reko's Decompiler to generate high-level code
		// 5. Convert Reko's output (typically C) to C# format
		
		var sb = new StringBuilder();
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
		sb.AppendLine($"\t\t// Contains {instructions.Count} x86 instructions");
		sb.AppendLine();
		sb.AppendLine("\t\tpublic async Task<dynamic> Execute(dynamic cpu, dynamic mem)");
		sb.AppendLine("\t\t{");
		sb.AppendLine("\t\t\t// TODO: Integrate Reko's decompilation output here");
		sb.AppendLine("\t\t\t// See RekoDecompilerAdapter implementation for integration details");
		sb.AppendLine("\t\t\tthrow new NotImplementedException(\"Reko integration requires additional implementation\");");
		sb.AppendLine("\t\t}");
		sb.AppendLine("\t}");
		sb.AppendLine("}");
		
		return await Task.FromResult(sb.ToString());
	}
}
