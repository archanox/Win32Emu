namespace Win32Emu.Diagnostics
{
	/// <summary>
	/// Statistics for a specific API call
	/// </summary>
	public class ApiCallStats
	{
		public string FunctionName { get; init; } = string.Empty;
		public long Count { get; set; }
		public long TotalDurationUs { get; set; }
	}
}