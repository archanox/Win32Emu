namespace Win32Emu.Rendering;

/// <summary>
/// Interface for audio backends
/// </summary>
public interface IAudioBackend : IDisposable
{
    /// <summary>
    /// Initialize the audio backend synchronously
    /// </summary>
    bool Initialize();

    /// <summary>
    /// Initialize the audio backend asynchronously.
    /// Default implementation calls the synchronous Initialize method.
    /// </summary>
    Task<bool> InitializeAsync() => Task.FromResult(Initialize());

    /// <summary>
    /// Create an audio stream with specified parameters
    /// </summary>
    uint CreateAudioStream(int frequency, int channels, int bufferSize);

    /// <summary>
    /// Write audio data to a stream
    /// </summary>
    bool WriteAudioData(uint streamId, byte[] data, int offset, int length);

    /// <summary>
    /// Destroy an audio stream
    /// </summary>
    bool DestroyAudioStream(uint streamId);

    /// <summary>
    /// Set volume for an audio stream (0.0 to 1.0)
    /// </summary>
    bool SetStreamVolume(uint streamId, float volume);

    /// <summary>
    /// Pause or resume an audio stream
    /// </summary>
    bool SetStreamPaused(uint streamId, bool paused);

    /// <summary>
    /// Gets whether the backend is initialized
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Gets the number of active audio streams
    /// </summary>
    int ActiveStreamCount { get; }
}
