namespace Win32Emu.Diagnostics
{
	/// <summary>
	/// Record from API Monitor CSV log
	/// </summary>
	public class ApiMonRecord
	{
		public int CallNumber { get; init; }
		public string TimeOfDay { get; init; } = string.Empty;
		public string Thread { get; init; } = string.Empty;
		public string Module { get; init; } = string.Empty;
		public string Api { get; init; } = string.Empty;
		public string? ReturnValue { get; init; }
		public string? Error { get; init; }
		public string? Duration { get; init; }
	}
}