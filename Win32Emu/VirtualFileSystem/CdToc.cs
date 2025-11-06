using System.Text;

namespace Win32Emu.VirtualFileSystem
{
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
}