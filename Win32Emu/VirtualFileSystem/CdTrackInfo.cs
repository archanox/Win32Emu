namespace Win32Emu.VirtualFileSystem;

/// <summary>
/// Represents CD-ROM track information from a CHD disc image.
/// </summary>
public class CdTrackInfo
{
	/// <summary>
	/// Track number (1-based)
	/// </summary>
	public int TrackNumber { get; set; }
	
	/// <summary>
	/// Track type (data, audio, etc.)
	/// </summary>
	public CdTrackType TrackType { get; set; }
	
	/// <summary>
	/// Track subtype (raw, cooked, etc.)
	/// </summary>
	public CdTrackSubType SubType { get; set; }
	
	/// <summary>
	/// Starting frame number
	/// </summary>
	public int StartFrame { get; set; }
	
	/// <summary>
	/// Number of frames in this track
	/// </summary>
	public int FrameCount { get; set; }
	
	/// <summary>
	/// Bytes per frame (2352 for raw, 2048 for cooked data)
	/// </summary>
	public int FrameSize { get; set; }
	
	/// <summary>
	/// Track pregap in frames
	/// </summary>
	public int Pregap { get; set; }
	
	/// <summary>
	/// Track postgap in frames
	/// </summary>
	public int Postgap { get; set; }

	public override string ToString()
	{
		return $"Track {TrackNumber}: {TrackType}/{SubType}, Frames {StartFrame}-{StartFrame + FrameCount - 1} ({FrameCount} frames, {FrameSize} bytes/frame)";
	}
}