namespace Win32Emu.Loader
{
	/// <summary>
	/// Represents information about a PE section including its location, size, and characteristics.
	/// </summary>
	public record PeSection(
		string Name,                           // Section name (e.g., ".text", ".data", ".bss")
		uint VirtualAddress,                   // Virtual address (RVA) where section is loaded
		uint VirtualSize,                      // Size of section in memory
		uint RawSize,                          // Size of section in file
		PeSectionCharacteristics Characteristics // Section flags (executable, readable, writable, etc.)
	)
	{
		/// <summary>
		/// Returns true if this section contains executable code (has MemExecute or ContainsCode flag).
		/// </summary>
		public bool IsExecutable => (Characteristics & (PeSectionCharacteristics.MemExecute | PeSectionCharacteristics.ContainsCode)) != 0;

		/// <summary>
		/// Returns true if this section contains initialized data (has ContainsInitializedData flag).
		/// </summary>
		public bool IsData => (Characteristics & PeSectionCharacteristics.ContainsInitializedData) != 0;

		/// <summary>
		/// Returns true if this section is writable (has MemWrite flag).
		/// </summary>
		public bool IsWritable => (Characteristics & PeSectionCharacteristics.MemWrite) != 0;

		/// <summary>
		/// Returns true if this section is readable (has MemRead flag).
		/// </summary>
		public bool IsReadable => (Characteristics & PeSectionCharacteristics.MemRead) != 0;
	};
}