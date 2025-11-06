namespace Win32Emu.Cpu.Jit
{
	/// <summary>
	/// Statistics about the JIT cache
	/// </summary>
	public class CacheStatistics
	{
		public int TotalBlocks { get; set; }
		public int TotalInstructions { get; set; }
		public string CacheDirectory { get; set; } = string.Empty;
	}
}