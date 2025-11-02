using System.Text;

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

/// <summary>
/// CD track types
/// </summary>
public enum CdTrackType
{
	/// <summary>
	/// Audio track (CD-DA)
	/// </summary>
	Audio,
	
	/// <summary>
	/// Mode 1 data track
	/// </summary>
	Mode1,
	
	/// <summary>
	/// Mode 1 data track (raw)
	/// </summary>
	Mode1Raw,
	
	/// <summary>
	/// Mode 2 data track
	/// </summary>
	Mode2,
	
	/// <summary>
	/// Mode 2 formless data track
	/// </summary>
	Mode2Formless,
	
	/// <summary>
	/// Mode 2 Form 1 data track
	/// </summary>
	Mode2Form1,
	
	/// <summary>
	/// Mode 2 Form 2 data track
	/// </summary>
	Mode2Form2,
	
	/// <summary>
	/// Mode 2 data track (raw)
	/// </summary>
	Mode2Raw
}

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

/// <summary>
/// Table of Contents for a CD-ROM disc
/// </summary>
public class CdToc
{
	/// <summary>
	/// List of tracks on the disc
	/// </summary>
	public List<CdTrackInfo> Tracks { get; } = new();
	
	/// <summary>
	/// First track number (usually 1)
	/// </summary>
	public int FirstTrack => Tracks.Count > 0 ? Tracks[0].TrackNumber : 0;
	
	/// <summary>
	/// Last track number
	/// </summary>
	public int LastTrack => Tracks.Count > 0 ? Tracks[^1].TrackNumber : 0;
	
	/// <summary>
	/// Total number of frames on the disc
	/// </summary>
	public int TotalFrames { get; set; }

	public override string ToString()
	{
		var sb = new StringBuilder();
		sb.AppendLine($"CD TOC: {Tracks.Count} tracks, {TotalFrames} total frames");
		foreach (var track in Tracks)
		{
			sb.AppendLine($"  {track}");
		}
		return sb.ToString();
	}
}
