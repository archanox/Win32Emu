using Microsoft.Extensions.Logging;

namespace Win32Emu.Rendering;

/// <summary>
/// Null audio backend that provides no audio output.
/// Used for headless mode or when audio is not available.
/// </summary>
public class NullAudioBackend : IAudioBackend
{
	private readonly ILogger _logger;
	private bool _isInitialized;
	private uint _nextStreamId = 1;

	public bool IsInitialized => _isInitialized;
	public int ActiveStreamCount => 0;

	public NullAudioBackend(ILogger logger)
	{
		_logger = logger;
	}

	public bool Initialize()
	{
		_logger.LogInformation("[NullAudio] Audio backend initialized (no audio output)");
		_isInitialized = true;
		return true;
	}

	public uint CreateAudioStream(int frequency, int channels, int bufferSize)
	{
		_logger.LogDebug("[NullAudio] CreateAudioStream(frequency={Frequency}, channels={Channels}, bufferSize={BufferSize})",
			frequency, channels, bufferSize);
		return _nextStreamId++;
	}

	public bool WriteAudioData(uint streamId, byte[] data, int offset, int length)
	{
		// Silently discard audio data
		return true;
	}

	public bool DestroyAudioStream(uint streamId)
	{
		_logger.LogDebug("[NullAudio] DestroyAudioStream(streamId={StreamId})", streamId);
		return true;
	}

	public bool SetStreamVolume(uint streamId, float volume)
	{
		// No-op
		return true;
	}

	public bool SetStreamPaused(uint streamId, bool paused)
	{
		// No-op
		return true;
	}

	public void Dispose()
	{
		_logger.LogInformation("[NullAudio] Audio backend disposed");
		_isInitialized = false;
	}
}
