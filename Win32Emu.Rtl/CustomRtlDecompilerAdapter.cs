using Iced.Intel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Rtl;

/// <summary>
/// Decompiler adapter using Win32Emu's custom RTL pipeline.
/// This is the default, MIT-licensed decompiler implementation.
/// </summary>
public class CustomRtlDecompilerAdapter : IDecompilerAdapter
{
	private readonly ILogger _logger;
	
	public string Name => "CustomRTL";
	
	public bool IsAvailable => true; // Always available
	
	public string LicenseInfo => "MIT License - Part of Win32Emu";
	
	public CustomRtlDecompilerAdapter(ILogger? logger = null)
	{
		_logger = logger ?? NullLogger.Instance;
	}
	
	public async Task<string> DecompileToCSharpAsync(uint startAddress, List<Instruction> instructions, string className)
	{
		// Use existing RTL pipeline
		var converter = new X86ToRtlConverter(_logger);
		var rtlBlock = converter.Convert(startAddress, instructions);
		
		var optimizer = new RtlOptimizer();
		var optimizedBlock = optimizer.Optimize(rtlBlock);
		
		var generator = new RtlToCSharpGenerator();
		var csharpCode = generator.GenerateCSharpCode(optimizedBlock, className, "Execute");
		
		return await Task.FromResult(csharpCode);
	}
}
