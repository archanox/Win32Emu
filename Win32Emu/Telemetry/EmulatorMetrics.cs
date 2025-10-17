using System.Diagnostics.Metrics;

namespace Win32Emu.Telemetry;

/// <summary>
/// Provides metrics instrumentation for the Win32 emulator
/// </summary>
public sealed class EmulatorMetrics
{
	private readonly Counter<long> _instructionCounter;
	private readonly Counter<long> _apiCallCounter;
	private readonly Counter<long> _memoryAllocationCounter;
	private readonly Histogram<double> _apiCallDuration;
	private readonly ObservableGauge<long> _memoryUsage;
	private readonly Counter<long> _exceptionCounter;
	
	private long _currentMemoryUsage;

	public EmulatorMetrics(Meter meter)
	{
		if (meter == null)
		{
			throw new ArgumentNullException(nameof(meter));
		}
		
		// Counter for total instructions executed
		_instructionCounter = meter.CreateCounter<long>(
			"win32emu.instructions.executed",
			description: "Total number of x86 instructions executed");
		
		// Counter for API calls
		_apiCallCounter = meter.CreateCounter<long>(
			"win32emu.api.calls",
			description: "Total number of Win32 API calls");
		
		// Counter for memory allocations
		_memoryAllocationCounter = meter.CreateCounter<long>(
			"win32emu.memory.allocations",
			description: "Total number of memory allocations");
		
		// Histogram for API call duration
		_apiCallDuration = meter.CreateHistogram<double>(
			"win32emu.api.duration",
			unit: "ms",
			description: "Duration of Win32 API calls in milliseconds");
		
		// Observable gauge for current memory usage
		_memoryUsage = meter.CreateObservableGauge<long>(
			"win32emu.memory.usage",
			() => _currentMemoryUsage,
			unit: "bytes",
			description: "Current memory usage of the emulator");
		
		// Counter for exceptions
		_exceptionCounter = meter.CreateCounter<long>(
			"win32emu.exceptions",
			description: "Total number of exceptions encountered");
	}
	
	/// <summary>
	/// Record executed instructions
	/// </summary>
	public void RecordInstructionsExecuted(long count = 1)
	{
		_instructionCounter.Add(count);
	}
	
	/// <summary>
	/// Record an API call
	/// </summary>
	public void RecordApiCall(string dll, string function)
	{
		_apiCallCounter.Add(1, new KeyValuePair<string, object?>("dll", dll), new KeyValuePair<string, object?>("function", function));
	}
	
	/// <summary>
	/// Record memory allocation
	/// </summary>
	public void RecordMemoryAllocation(long bytes)
	{
		_memoryAllocationCounter.Add(1);
		Interlocked.Add(ref _currentMemoryUsage, bytes);
	}
	
	/// <summary>
	/// Record memory deallocation
	/// </summary>
	public void RecordMemoryDeallocation(long bytes)
	{
		Interlocked.Add(ref _currentMemoryUsage, -bytes);
	}
	
	/// <summary>
	/// Record API call duration
	/// </summary>
	public void RecordApiDuration(string dll, string function, double durationMs)
	{
		_apiCallDuration.Record(durationMs, new KeyValuePair<string, object?>("dll", dll), new KeyValuePair<string, object?>("function", function));
	}
	
	/// <summary>
	/// Record an exception
	/// </summary>
	public void RecordException(string type)
	{
		_exceptionCounter.Add(1, new KeyValuePair<string, object?>("type", type));
	}
}
