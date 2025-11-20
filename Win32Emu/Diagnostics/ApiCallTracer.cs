using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Diagnostics;

/// <summary>
/// Comprehensive API call tracer for diagnosing emulation issues.
/// Logs all Win32 API calls, DirectX COM methods, and execution flow.
/// </summary>
public class ApiCallTracer : IDisposable
{
	private readonly ILogger _logger;
	private readonly bool _enableTracing;
	private readonly bool _enableDetailedParameters;
	private readonly bool _enableExecutionFlow;
	private readonly string? _outputPath;
	private StreamWriter? _traceWriter;
	private readonly ConcurrentQueue<ApiCallRecord> _callQueue = new();
	private readonly ConcurrentDictionary<string, ApiCallStats> _callStats = new();
	private readonly Stopwatch _sessionStopwatch = Stopwatch.StartNew();
	private readonly int _maxQueueSize;
	private long _totalCalls;
	private long _droppedCalls;

	public ApiCallTracer(
		ILogger? logger = null,
		bool enableTracing = true,
		bool enableDetailedParameters = true,
		bool enableExecutionFlow = false,
		string? outputPath = null,
		int maxQueueSize = 10000)
	{
		_logger = logger ?? NullLogger.Instance;
		_enableTracing = enableTracing;
		_enableDetailedParameters = enableDetailedParameters;
		_enableExecutionFlow = enableExecutionFlow;
		_outputPath = outputPath;
		_maxQueueSize = maxQueueSize;

		if (_enableTracing && !string.IsNullOrEmpty(_outputPath))
		{
			try
			{
				var directory = Path.GetDirectoryName(_outputPath);
				if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}
				_traceWriter = new StreamWriter(_outputPath, append: false) { AutoFlush = true };
				_traceWriter.WriteLine($"Win32Emu API Call Trace - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
				_traceWriter.WriteLine("=" + new string('=', 79));
				_traceWriter.WriteLine();
			}
			catch (DirectoryNotFoundException ex)
			{
				_logger.LogWarning(ex, "Failed to create trace output file (directory not found): {Path}", _outputPath);
			}
			catch (UnauthorizedAccessException ex)
			{
				_logger.LogWarning(ex, "Failed to create trace output file (unauthorized): {Path}", _outputPath);
			}
			catch (IOException ex)
			{
				_logger.LogWarning(ex, "Failed to create trace output file (IO error): {Path}", _outputPath);
			}
		}

		if (_enableTracing)
		{
			_logger.LogInformation("[ApiTracer] API call tracing enabled (detailed params: {Detailed}, execution flow: {Flow})",
				_enableDetailedParameters, _enableExecutionFlow);
		}
	}

	/// <summary>
	/// Log a Win32 API call
	/// </summary>
	public void LogApiCall(
		string moduleName,
		string functionName,
		Dictionary<string, object>? parameters = null,
		object? returnValue = null,
		uint eip = 0,
		long? durationUs = null)
	{
		if (!_enableTracing)
		{
			return;
		}

		var callNumber = Interlocked.Increment(ref _totalCalls);
		var timestamp = _sessionStopwatch.Elapsed;

		var record = new ApiCallRecord
		{
			CallNumber = callNumber,
			Timestamp = timestamp,
			ModuleName = moduleName,
			FunctionName = functionName,
			Parameters = parameters ?? new Dictionary<string, object>(),
			ReturnValue = returnValue,
			Eip = eip,
			DurationMicroseconds = durationUs
		};

		_callQueue.Enqueue(record);
		
		// Prevent unbounded growth by limiting queue size
		// Note: This is an approximate check for performance. Between checking Count and TryDequeue,
		// other threads might modify the queue, but this is acceptable for our use case.
		while (true)
		{
			// Check if the queue is over the size limit
			if (_callQueue.Count <= _maxQueueSize)
			{
				break;
			}
			// Try to dequeue an item if possible
			if (_callQueue.TryDequeue(out _))
			{
				Interlocked.Increment(ref _droppedCalls);
			}
			else
			{
				// If we failed to dequeue, exit to avoid spinning
				break;
			}
		}

		// Update statistics
		var key = $"{moduleName}.{functionName}";
		_callStats.AddOrUpdate(key,
			_ => new ApiCallStats { FunctionName = key, Count = 1, TotalDurationUs = durationUs ?? 0 },
			(_, stats) =>
			{
				stats.Count++;
				stats.TotalDurationUs += durationUs ?? 0;
				return stats;
			});

		// Write to trace file
		if (_traceWriter != null)
		{
			WriteTraceRecord(record);
		}

		// Log to console (at debug level to avoid spam)
		if (_logger.IsEnabled(LogLevel.Debug))
		{
			_logger.LogDebug("[API] {Module}.{Function}{Params} = {Return}",
				moduleName, functionName,
				FormatParametersShort(parameters),
				returnValue ?? "void");
		}
	}

	/// <summary>
	/// Log a DirectX COM method call
	/// </summary>
	public void LogComCall(
		string interfaceName,
		string methodName,
		Dictionary<string, object>? parameters = null,
		object? returnValue = null,
		uint thisPtr = 0,
		uint eip = 0,
		long? durationUs = null)
	{
		var fullName = $"{interfaceName}::{methodName}";
		LogApiCall("COM", fullName, parameters, returnValue, eip, durationUs);
	}

	/// <summary>
	/// Log an execution flow marker (e.g., entering a specific code region)
	/// </summary>
	public void LogExecutionFlow(string marker, uint eip, Dictionary<string, object>? context = null)
	{
		if (!_enableTracing || !_enableExecutionFlow)
		{
			return;
		}

		LogApiCall("FLOW", marker, context, null, eip);
	}

	/// <summary>
	/// Generate a diagnostic report summarizing all traced calls
	/// </summary>
	public string GenerateDiagnosticReport()
	{
		var sb = new StringBuilder();
		sb.AppendLine("API Call Diagnostic Report");
		sb.AppendLine("=" + new string('=', 79));
		sb.AppendLine();
		sb.AppendLine($"Session Duration: {_sessionStopwatch.Elapsed:hh\\:mm\\:ss\\.fff}");
		sb.AppendLine($"Total API Calls: {_totalCalls:N0}");
		if (_droppedCalls > 0)
		{
			sb.AppendLine($"Dropped Calls (queue full): {_droppedCalls:N0}");
			sb.AppendLine($"  (Queue size limit: {_maxQueueSize:N0} - increase if needed)");
		}
		sb.AppendLine();

		// Top called APIs
		sb.AppendLine("Top 20 Most Called APIs:");
		sb.AppendLine("-" + new string('-', 79));
		var topCalls = _callStats.Values
			.OrderByDescending(s => s.Count)
			.Take(20)
			.ToList();

		if (topCalls.Any())
		{
			sb.AppendLine($"{"Function",-50} {"Count",10} {"Avg Time (μs)",15}");
			sb.AppendLine(new string('-', 80));
			foreach (var stat in topCalls)
			{
				var avgTime = stat.Count > 0 ? stat.TotalDurationUs / stat.Count : 0;
				sb.AppendLine($"{stat.FunctionName,-50} {stat.Count,10:N0} {avgTime,15:N1}");
			}
		}
		else
		{
			sb.AppendLine("  (No API calls recorded)");
		}

		sb.AppendLine();

		// Slowest APIs
		sb.AppendLine("Top 20 Slowest APIs (by total time):");
		sb.AppendLine("-" + new string('-', 79));
		var slowestCalls = _callStats.Values
			.Where(s => s.TotalDurationUs > 0)
			.OrderByDescending(s => s.TotalDurationUs)
			.Take(20)
			.ToList();

		if (slowestCalls.Any())
		{
			sb.AppendLine($"{"Function",-50} {"Total Time (ms)",17} {"Calls",8}");
			sb.AppendLine(new string('-', 80));
			foreach (var stat in slowestCalls)
			{
				var totalTimeMs = stat.TotalDurationUs / 1000.0;
				sb.AppendLine($"{stat.FunctionName,-50} {totalTimeMs,17:N2} {stat.Count,8:N0}");
			}
		}
		else
		{
			sb.AppendLine("  (No timing data available)");
		}

		sb.AppendLine();

		// DirectX/COM calls breakdown
		var comCalls = _callStats.Where(kvp => kvp.Key.StartsWith("COM.")).ToList();
		if (comCalls.Any())
		{
			sb.AppendLine("DirectX COM Calls:");
			sb.AppendLine("-" + new string('-', 79));
			sb.AppendLine($"{"Method",-50} {"Count",10}");
			sb.AppendLine(new string('-', 80));
			foreach (var (key, stats) in comCalls.OrderByDescending(kvp => kvp.Value.Count).Take(20))
			{
				var methodName = key.Substring(4); // Remove "COM." prefix
				sb.AppendLine($"{methodName,-50} {stats.Count,10:N0}");
			}
			sb.AppendLine();
		}

		return sb.ToString();
	}

	/// <summary>
	/// Get recent API calls (last N calls)
	/// </summary>
	public List<ApiCallRecord> GetRecentCalls(int count = 100)
	{
		return _callQueue.Reverse().Take(count).Reverse().ToList();
	}

	/// <summary>
	/// Get call statistics for a specific API
	/// </summary>
	public ApiCallStats? GetCallStats(string moduleName, string functionName)
	{
		var key = $"{moduleName}.{functionName}";
		return _callStats.TryGetValue(key, out var stats) ? stats : null;
	}

	private void WriteTraceRecord(ApiCallRecord record)
	{
		if (_traceWriter == null)
		{
			return;
		}

		var sb = new StringBuilder();
		sb.Append($"[{record.CallNumber,8}] ");
		sb.Append($"{record.Timestamp.TotalSeconds,10:F6}s ");
		sb.Append($"EIP=0x{record.Eip:X8} ");
		sb.Append($"{record.ModuleName}.{record.FunctionName}");

		if (_enableDetailedParameters && record.Parameters.Any())
		{
			sb.Append("(");
			var first = true;
			foreach (var (name, value) in record.Parameters)
			{
				if (!first)
				{
					sb.Append(", ");
				}

				sb.Append($"{name}={FormatValue(value)}");
				first = false;
			}
			sb.Append(")");
		}
		else if (record.Parameters.Any())
		{
			sb.Append($"({record.Parameters.Count} params)");
		}
		else
		{
			sb.Append("()");
		}

		if (record.ReturnValue != null)
		{
			sb.Append($" = {FormatValue(record.ReturnValue)}");
		}

		if (record.DurationMicroseconds.HasValue)
		{
			sb.Append($" [{record.DurationMicroseconds.Value:N0}μs]");
		}

		_traceWriter.WriteLine(sb.ToString());
	}

	private static string FormatParametersShort(Dictionary<string, object>? parameters)
	{
		if (parameters == null || !parameters.Any())
		{
			return "()";
		}

		return $"({parameters.Count} params)";
	}

	private static string FormatValue(object? value)
	{
		return value switch
		{
			null => "null",
			string s => $"\"{s}\"",
			uint u => $"0x{u:X}",
			int i when i < 0 || i > 1000 => $"0x{i:X}",
			bool b => b.ToString().ToLower(),
			byte[] bytes => $"[{bytes.Length} bytes]",
			_ => value.ToString() ?? "null"
		};
	}

	public void Dispose()
	{
		if (_traceWriter != null)
		{
			try
			{
				_traceWriter.WriteLine();
				_traceWriter.WriteLine("=" + new string('=', 79));
				_traceWriter.WriteLine($"Trace ended at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
				_traceWriter.WriteLine($"Total calls: {_totalCalls:N0}");
				_traceWriter.WriteLine();
				_traceWriter.WriteLine(GenerateDiagnosticReport());
			}
			catch (IOException ex)
			{
				_logger.LogError(ex, "[ApiTracer] IO error during Dispose while writing trace file");
			}
			catch (ObjectDisposedException ex)
			{
				_logger.LogError(ex, "[ApiTracer] Object disposed error during Dispose");
			}
			finally
			{
				_traceWriter?.Dispose();
				_traceWriter = null;
			}
		}

		_logger.LogInformation("[ApiTracer] Session complete - {TotalCalls} API calls traced", _totalCalls);
	}
}