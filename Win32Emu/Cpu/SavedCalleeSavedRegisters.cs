namespace Win32Emu.Cpu
{
	/// <summary>
	/// Holds saved callee-saved register values (EBX, ESI, EDI, EBP) per x86 calling convention
	/// </summary>
	public readonly struct SavedCalleeSavedRegisters
	{
		public uint Ebx { get; init; }
		public uint Esi { get; init; }
		public uint Edi { get; init; }
		public uint Ebp { get; init; }
	}
}