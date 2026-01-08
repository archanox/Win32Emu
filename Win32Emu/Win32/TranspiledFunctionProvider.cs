using System;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Win32Emu.Win32;

/// <summary>
/// Provides access to transpiled C# functions loaded from a generated assembly.
/// Enables hybrid execution where frequently-called or problematic functions
/// can be executed as pre-compiled C# instead of JIT compiling x86 code.
/// </summary>
public class TranspiledFunctionProvider
{
	private readonly ILogger _logger;
	private readonly Dictionary<uint, Func<ProcessEnvironment, object>> _functions = new();
	private readonly HashSet<uint> _availableAddresses = new();
	
	public TranspiledFunctionProvider(ILogger logger)
	{
		_logger = logger;
	}
	
	/// <summary>
	/// Load transpiled functions from an assembly
	/// </summary>
	public void LoadFromAssembly(string assemblyPath)
	{
		try
		{
			var assembly = Assembly.LoadFrom(assemblyPath);
			_logger.LogInformation("[TranspiledFunctions] Loading transpiled functions from {AssemblyPath}", assemblyPath);
			
			// Find all function classes (e.g., Function_004032A0)
			var functionTypes = assembly.GetTypes()
				.Where(t => t.Name.StartsWith("Function_") && t.Namespace != null)
				.ToArray();
			
			foreach (var functionType in functionTypes)
			{
				// Extract address from class name (e.g., "Function_004032A0" -> 0x004032A0)
				var addressStr = functionType.Name.Substring("Function_".Length);
				if (!uint.TryParse(addressStr, System.Globalization.NumberStyles.HexNumber, null, out var address))
				{
					_logger.LogWarning("[TranspiledFunctions] Could not parse address from type name: {TypeName}", functionType.Name);
					continue;
				}
				
				// Create factory function that instantiates the transpiled function and calls Execute()
				_functions[address] = (env) =>
				{
					// Create instance with constructor that takes ProcessEnvironment
					var instance = Activator.CreateInstance(functionType, env);
					if (instance == null)
					{
						_logger.LogError("[TranspiledFunctions] Failed to create instance of {TypeName}", functionType.Name);
						return 0;
					}
					
					// Find and invoke Execute() method
					var executeMethod = functionType.GetMethod("Execute");
					if (executeMethod == null)
					{
						_logger.LogError("[TranspiledFunctions] No Execute() method found in {TypeName}", functionType.Name);
						return 0;
					}
					
					var result = executeMethod.Invoke(instance, null);
					return result ?? 0;
				};
				
				_availableAddresses.Add(address);
				_logger.LogDebug("[TranspiledFunctions] Registered function at address 0x{Address:X8} ({TypeName})", address, functionType.Name);
			}
			
			_logger.LogInformation("[TranspiledFunctions] Loaded {Count} transpiled functions", _functions.Count);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[TranspiledFunctions] Failed to load transpiled functions from {AssemblyPath}", assemblyPath);
		}
	}
	
	/// <summary>
	/// Check if a transpiled function is available at the given address
	/// </summary>
	public bool HasFunction(uint address)
	{
		return _availableAddresses.Contains(address);
	}
	
	/// <summary>
	/// Try to execute a transpiled function at the given address
	/// </summary>
	/// <returns>True if function was executed, false if not available</returns>
	public bool TryExecuteFunction(uint address, ProcessEnvironment env, out object? result)
	{
		result = null;
		
		if (!_functions.TryGetValue(address, out var func))
		{
			return false;
		}
		
		try
		{
			_logger.LogDebug("[TranspiledFunctions] Executing transpiled function at 0x{Address:X8}", address);
			result = func(env);
			_logger.LogDebug("[TranspiledFunctions] Transpiled function at 0x{Address:X8} returned: {Result}", address, result);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[TranspiledFunctions] Error executing transpiled function at 0x{Address:X8}", address);
			return false;
		}
	}
	
	/// <summary>
	/// Get all addresses that have transpiled functions available
	/// </summary>
	public IEnumerable<uint> GetAvailableAddresses()
	{
		return _availableAddresses;
	}
}
