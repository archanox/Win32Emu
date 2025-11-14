using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Iced.Intel;
using Lokad.ILPack;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Rtl;

/// <summary>
/// RTL-based JIT cache with readable C# code generation and assembly persistence.
/// Implements the full pipeline: x86 → RTL → Optimized RTL → C# → Assembly
/// Uses Lokad.ILPack to save assemblies with decompilable C# source.
/// </summary>
public class RtlJitCache
{
    private readonly ILogger _logger;
    private readonly string _cacheDirectory;
    private readonly string _sourceDirectory;
    private readonly X86ToRtlConverter _rtlConverter;
    private readonly RtlOptimizer _optimizer;
    private readonly RtlToCSharpGenerator _codeGenerator;
    private readonly Dictionary<uint, RtlCompiledBlock> _compiledBlocks = new();
    private readonly string _instanceId; // Unique ID for this cache instance to avoid assembly name conflicts
    
    public static string DefaultCacheDirectory => Path.Combine(
        Path.GetTempPath(),
        "Win32Emu",
        "RtlJitCache"
    );
    
    public static string DefaultSourceDirectory => Path.Combine(
        Path.GetTempPath(),
        "Win32Emu",
        "RtlJitCache",
        "Source"
    );
    
    public RtlJitCache(string? cacheDirectory = null, ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;
        _cacheDirectory = cacheDirectory ?? DefaultCacheDirectory;
        _sourceDirectory = Path.Combine(_cacheDirectory, "Source");
        
        // Generate unique instance ID to avoid assembly name conflicts when multiple tests run
        _instanceId = Guid.NewGuid().ToString("N")[..8];
        
        Directory.CreateDirectory(_cacheDirectory);
        Directory.CreateDirectory(_sourceDirectory);
        
        _rtlConverter = new X86ToRtlConverter(_logger);
        _optimizer = new RtlOptimizer();
        _codeGenerator = new RtlToCSharpGenerator();
        
        _logger.LogInformation("[RtlJitCache] Initialized RTL-based JIT cache at {Directory}", _cacheDirectory);
        _logger.LogInformation("[RtlJitCache] C# source code will be saved to {SourceDir}", _sourceDirectory);
    }
    
    /// <summary>
    /// Compile an x86 code block through the RTL pipeline
    /// </summary>
    public RtlCompiledBlock CompileBlock(uint startAddress, List<Instruction> instructions)
    {
        if (_compiledBlocks.TryGetValue(startAddress, out var cached))
        {
            _logger.LogDebug("[RtlJitCache] Using cached block at 0x{Address:X8}", startAddress);
            return cached;
        }
        
        _logger.LogInformation("[RtlJitCache] Compiling block at 0x{Address:X8} ({Count} instructions)",
            startAddress, instructions.Count);
        
        // Step 1: x86 → RTL
        var rtlBlock = _rtlConverter.Convert(startAddress, instructions);
        _logger.LogDebug("[RtlJitCache] Generated RTL with {BlockCount} basic blocks", rtlBlock.BasicBlocks.Count);
        
        // Step 2: Optimize RTL
        rtlBlock = _optimizer.Optimize(rtlBlock);
        _logger.LogDebug("[RtlJitCache] Optimized RTL");
        
        // Step 3: RTL → C# Code
        var className = $"JitBlock_{_instanceId}_{startAddress:X8}";
        var methodName = "Execute";
        var csharpCode = _codeGenerator.GenerateCSharpCode(rtlBlock, className, methodName);
        
        // Save C# source for inspection
        var sourceFile = Path.Combine(_sourceDirectory, $"{className}.cs");
        File.WriteAllText(sourceFile, csharpCode);
        _logger.LogInformation("[RtlJitCache] Saved C# source to {SourceFile}", sourceFile);
        
        // Step 4: C# → Compiled Assembly
        var assembly = CompileCSharpToAssembly(csharpCode, className);
        
        // Step 5: Save assembly to disk with Lokad.ILPack
        var assemblyFile = Path.Combine(_cacheDirectory, $"{className}.dll");
        SaveAssemblyToDisk(assembly, assemblyFile);
        
        var compiled = new RtlCompiledBlock
        {
            StartAddress = startAddress,
            RtlCode = rtlBlock,
            CSharpSource = csharpCode,
            Assembly = assembly,
            ClassName = className,
            MethodName = methodName
        };
        
        _compiledBlocks[startAddress] = compiled;
        
        _logger.LogInformation("[RtlJitCache] Successfully compiled block at 0x{Address:X8}", startAddress);
        return compiled;
    }
    
    /// <summary>
    /// Compile C# code to an in-memory assembly using Roslyn
    /// </summary>
    private Assembly CompileCSharpToAssembly(string csharpCode, string assemblyName)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(csharpCode);
        
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
            MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location), // For 'dynamic'
            MetadataReference.CreateFromFile(typeof(System.Linq.Expressions.Expression).Assembly.Location), // For DynamicAttribute
        };
        
        var compilation = CSharpCompilation.Create(
            assemblyName,
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
        
        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);
        
        if (!result.Success)
        {
            var errors = string.Join("\n", result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString()));
            
            _logger.LogError("[RtlJitCache] Compilation failed:\n{Errors}", errors);
            throw new InvalidOperationException($"C# compilation failed: {errors}");
        }
        
        ms.Seek(0, SeekOrigin.Begin);
        return Assembly.Load(ms.ToArray());
    }
    
    /// <summary>
    /// Save assembly to disk using Lokad.ILPack (preserves debugging info)
    /// </summary>
    private void SaveAssemblyToDisk(Assembly assembly, string path)
    {
        try
        {
            var generator = new AssemblyGenerator();
            generator.GenerateAssembly(assembly, path);
            _logger.LogInformation("[RtlJitCache] Saved assembly to {Path}", path);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[RtlJitCache] Failed to save assembly with ILPack (non-critical)");
        }
    }
    
    /// <summary>
    /// Load cached assemblies from disk
    /// </summary>
    public void LoadCachedAssemblies(string executablePath)
    {
        var metadataFile = GetMetadataFileName(executablePath);
        if (!File.Exists(metadataFile))
        {
            _logger.LogInformation("[RtlJitCache] No cache metadata found for {Executable}", executablePath);
            return;
        }
        
        try
        {
            var json = File.ReadAllText(metadataFile);
            var metadata = JsonSerializer.Deserialize<CacheMetadata>(json);
            
            if (metadata?.Blocks == null)
                return;
            
            foreach (var blockInfo in metadata.Blocks)
            {
                var assemblyFile = Path.Combine(_cacheDirectory, $"{blockInfo.ClassName}.dll");
                var sourceFile = Path.Combine(_sourceDirectory, $"{blockInfo.ClassName}.cs");
                
                if (File.Exists(assemblyFile) && File.Exists(sourceFile))
                {
                    var assembly = Assembly.LoadFrom(assemblyFile);
                    var source = File.ReadAllText(sourceFile);
                    
                    _compiledBlocks[blockInfo.StartAddress] = new RtlCompiledBlock
                    {
                        StartAddress = blockInfo.StartAddress,
                        CSharpSource = source,
                        Assembly = assembly,
                        ClassName = blockInfo.ClassName,
                        MethodName = blockInfo.MethodName
                    };
                }
            }
            
            _logger.LogInformation("[RtlJitCache] Loaded {Count} cached blocks", _compiledBlocks.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RtlJitCache] Failed to load cache metadata");
        }
    }
    
    /// <summary>
    /// Save cache metadata to disk
    /// </summary>
    public void SaveCacheMetadata(string executablePath)
    {
        var metadataFile = GetMetadataFileName(executablePath);
        
        var metadata = new CacheMetadata
        {
            ExecutablePath = executablePath,
            Timestamp = DateTime.UtcNow,
            Blocks = _compiledBlocks.Values.Select(b => new BlockInfo
            {
                StartAddress = b.StartAddress,
                ClassName = b.ClassName,
                MethodName = b.MethodName
            }).ToList()
        };
        
        var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(metadataFile, json);
        
        _logger.LogInformation("[RtlJitCache] Saved metadata for {Count} blocks", metadata.Blocks.Count);
    }
    
    /// <summary>
    /// Get statistics about the cache
    /// </summary>
    public RtlCacheStatistics GetStatistics()
    {
        return new RtlCacheStatistics
        {
            TotalBlocks = _compiledBlocks.Count,
            CacheDirectory = _cacheDirectory,
            SourceDirectory = _sourceDirectory
        };
    }
    
    /// <summary>
    /// Clear all cached data
    /// </summary>
    public void PurgeCache()
    {
        _compiledBlocks.Clear();
        
        if (Directory.Exists(_cacheDirectory))
        {
            Directory.Delete(_cacheDirectory, true);
            Directory.CreateDirectory(_cacheDirectory);
            Directory.CreateDirectory(_sourceDirectory);
        }
        
        _logger.LogInformation("[RtlJitCache] Cache purged");
    }
    
    private string GetMetadataFileName(string executablePath)
    {
        var hash = ComputeHash(executablePath);
        return Path.Combine(_cacheDirectory, $"metadata_{hash}.json");
    }
    
    private string ComputeHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes)[..16];
    }
}

/// <summary>
/// Represents a compiled RTL block
/// </summary>
public class RtlCompiledBlock
{
    public uint StartAddress { get; set; }
    public RtlCodeBlock? RtlCode { get; set; }
    public string CSharpSource { get; set; } = "";
    public Assembly? Assembly { get; set; }
    public string ClassName { get; set; } = "";
    public string MethodName { get; set; } = "";
}

/// <summary>
/// Cache metadata for persistence
/// </summary>
public class CacheMetadata
{
    public string ExecutablePath { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public List<BlockInfo> Blocks { get; set; } = new();
}

public class BlockInfo
{
    public uint StartAddress { get; set; }
    public string ClassName { get; set; } = "";
    public string MethodName { get; set; } = "";
}

public class RtlCacheStatistics
{
    public int TotalBlocks { get; set; }
    public string CacheDirectory { get; set; } = "";
    public string SourceDirectory { get; set; } = "";
}
