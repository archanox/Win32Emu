namespace Win32Emu.Cpu.Jit
{
	/// <summary>
	/// Container for serialized cache data
	/// </summary>
	public class JitCacheData
	{
		public int Version { get; set; }
		public string ExecutablePath { get; set; } = string.Empty;
		public DateTime Timestamp { get; set; }
		public List<BlockMetadata> Blocks { get; set; } = new();
	}
}