namespace Win32Emu.VirtualFileSystem
{
	/// <summary>
	/// CD track subtypes
	/// </summary>
	public enum CdTrackSubType
	{
		/// <summary>
		/// No subcode data
		/// </summary>
		None,
	
		/// <summary>
		/// Raw subcode data (96 bytes)
		/// </summary>
		Raw,
	
		/// <summary>
		/// Cooked subcode data
		/// </summary>
		Cooked
	}
}