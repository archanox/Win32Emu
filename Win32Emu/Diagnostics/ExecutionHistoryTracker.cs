using System.Collections.Concurrent;

namespace Win32Emu.Diagnostics;

/// <summary>
/// Tracks recent CPU instruction execution history for debugging purposes.
/// Uses a circular buffer approach to maintain last N executed instructions.
/// </summary>
public class ExecutionHistoryTracker
{
	private readonly ConcurrentQueue<ExecutionRecord> _history = new();
	private readonly int _maxSize;
	private long _totalInstructions;

	public ExecutionHistoryTracker(int maxSize = 1000)
	{
		_maxSize = maxSize;
	}

	/// <summary>
	/// Record an executed instruction
	/// </summary>
	public void RecordExecution(uint eip, byte[] instructionBytes, string? disassembly = null)
	{
		Interlocked.Increment(ref _totalInstructions);

		var record = new ExecutionRecord
		{
			InstructionNumber = _totalInstructions,
			Eip = eip,
			InstructionBytes = instructionBytes,
			Disassembly = disassembly,
			Timestamp = DateTime.UtcNow
		};

		_history.Enqueue(record);

		// Maintain max size by dequeuing old entries
		while (_history.Count > _maxSize)
		{
			_history.TryDequeue(out _);
		}
	}

	/// <summary>
	/// Get recent execution history
	/// </summary>
	public List<ExecutionRecord> GetRecentHistory(int count)
	{
		if (count <= 0)
		{
			return new List<ExecutionRecord>(0);
		}

		var snapshot = _history.ToArray();
		if (snapshot.Length == 0)
		{
			return new List<ExecutionRecord>(0);
		}

		var takeCount = Math.Min(count, snapshot.Length);
		var startIndex = snapshot.Length - takeCount;

		return new ArraySegment<ExecutionRecord>(snapshot, startIndex, takeCount).ToList();
	}

	/// <summary>
	/// Get total number of instructions executed
	/// </summary>
	public long TotalInstructions => _totalInstructions;
}

/// <summary>
/// Record of a single executed instruction
/// </summary>
public class ExecutionRecord
{
	public long InstructionNumber { get; init; }
	public uint Eip { get; init; }
	public byte[] InstructionBytes { get; init; } = Array.Empty<byte>();
	public string? Disassembly { get; init; }
	public DateTime Timestamp { get; init; }
}
