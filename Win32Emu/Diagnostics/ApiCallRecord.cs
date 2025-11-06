namespace Win32Emu.Diagnostics
{
	/// <summary>
	/// Record of a single API call
	/// </summary>
	public class ApiCallRecord
	{
		public long CallNumber { get; init; }
		public TimeSpan Timestamp { get; init; }
		public string ModuleName { get; init; } = string.Empty;
		public string FunctionName { get; init; } = string.Empty;
		public Dictionary<string, object> Parameters { get; init; } = new();
		public object? ReturnValue { get; init; }
		public uint Eip { get; init; }
		public long? DurationMicroseconds { get; init; }
	}
}