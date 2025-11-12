using Microsoft.Extensions.Logging;
using SDL3;
using System.Collections.Concurrent;

namespace Win32Emu.Rendering;

/// <summary>
/// SDL3 audio backend for DirectSound operations
/// </summary>
public class Sdl3AudioBackend(ILogger logger) : IAudioBackend
{
    private readonly ILogger _logger = logger;
    private readonly ConcurrentDictionary<uint, AudioStreamInfo> _audioStreams = new();
    private uint _nextStreamId = 1;
    private bool _initialized;
    private readonly Lock _lock = new();

    private class AudioStreamInfo
    {
        public IntPtr Stream { get; set; }
        public int SampleRate { get; set; }
        public int Channels { get; set; }
        public int BufferSize { get; set; }
    }

    public bool Initialize()
    {
        lock (_lock)
        {
            if (_initialized)
            {
                return true;
            }

            try
            {
                _logger.LogInformation("[SDL3Audio] Initializing SDL3 audio backend...");

                // Critical: Set app metadata before any SDL initialization
                Sdl3Initializer.EnsureAppMetadataSet();

                // Initialize SDL audio subsystem
                if (!SDL.Init(SDL.InitFlags.Audio))
                {
                    _logger.LogError("[SDL3Audio] Failed to initialize SDL audio: {Error}", SDL.GetError());
                    return false;
                }

                _initialized = true;
                _logger.LogInformation("[SDL3Audio] Audio backend initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SDL3Audio] Failed to initialize audio backend");
                return false;
            }
        }
    }

    public uint CreateAudioStream(int sampleRate, int channels, int bufferSize)
    {
        if (!_initialized)
        {
            _logger.LogWarning("[SDL3Audio] Cannot create audio stream - backend not initialized");
            return 0;
        }

        lock (_lock)
        {
            try
            {
                // Create SDL audio stream spec
                var spec = new SDL.AudioSpec
                {
                    Freq = sampleRate,
                    Format = SDL.AudioFormat.AudioS16LE,
                    Channels = channels
                };

                // Open audio stream (0 = default playback device)
                var stream = SDL.OpenAudioDeviceStream(0, in spec, callback: null, userdata: IntPtr.Zero);
                if (stream == IntPtr.Zero)
                {
                    _logger.LogError("[SDL3Audio] Failed to open audio stream: {Error}", SDL.GetError());
                    return 0;
                }

                // Resume stream (start playing)
                SDL.ResumeAudioStreamDevice(stream);

                var streamId = _nextStreamId++;
                _audioStreams[streamId] = new AudioStreamInfo
                {
                    Stream = stream,
                    SampleRate = sampleRate,
                    Channels = channels,
                    BufferSize = bufferSize
                };

                _logger.LogInformation("[SDL3Audio] Created audio stream {StreamId}: {SampleRate}Hz, {Channels} channels, {BufferSize} bytes",
                    streamId, sampleRate, channels, bufferSize);

                return streamId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SDL3Audio] Failed to create audio stream");
                return 0;
            }
        }
    }

    public bool WriteAudioData(uint streamId, byte[] data, int offset, int length)
    {
        if (!_initialized)
        {
            return false;
        }

        if (!_audioStreams.TryGetValue(streamId, out var streamInfo))
        {
            _logger.LogWarning("[SDL3Audio] Audio stream {StreamId} not found", streamId);
            return false;
        }

        try
        {
            // Put audio data into stream
            var dataToWrite = new byte[length];
            Array.Copy(data, offset, dataToWrite, 0, length);

            unsafe
            {
                fixed (byte* ptr = dataToWrite)
                {
                    if (!SDL.PutAudioStreamData(streamInfo.Stream, (IntPtr)ptr, length))
                    {
                        _logger.LogError("[SDL3Audio] Failed to write audio data: {Error}", SDL.GetError());
                        return false;
                    }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SDL3Audio] Failed to write audio data to stream {StreamId}", streamId);
            return false;
        }
    }

    public bool DestroyAudioStream(uint streamId)
    {
        if (!_initialized)
        {
            return false;
        }

        if (_audioStreams.TryRemove(streamId, out var streamInfo))
        {
            try
            {
                SDL.DestroyAudioStream(streamInfo.Stream);
                _logger.LogInformation("[SDL3Audio] Destroyed audio stream {StreamId}", streamId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SDL3Audio] Failed to destroy audio stream {StreamId}", streamId);
                return false;
            }
        }

        return false;
    }

    public bool SetStreamVolume(uint streamId, float volume)
    {
        if (!_initialized)
        {
            return false;
        }

        if (!_audioStreams.TryGetValue(streamId, out var streamInfo))
        {
            _logger.LogWarning("[SDL3Audio] Audio stream {StreamId} not found", streamId);
            return false;
        }

        try
        {
            // Set audio stream gain (volume)
            if (!SDL.SetAudioStreamGain(streamInfo.Stream, volume))
            {
                _logger.LogError("[SDL3Audio] Failed to set volume: {Error}", SDL.GetError());
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SDL3Audio] Failed to set volume for stream {StreamId}", streamId);
            return false;
        }
    }

    public bool SetStreamPaused(uint streamId, bool paused)
    {
        if (!_initialized)
        {
            return false;
        }

        if (!_audioStreams.TryGetValue(streamId, out var streamInfo))
        {
            _logger.LogWarning("[SDL3Audio] Audio stream {StreamId} not found", streamId);
            return false;
        }

        try
        {
            if (paused)
            {
                SDL.PauseAudioStreamDevice(streamInfo.Stream);
            }
            else
            {
                SDL.ResumeAudioStreamDevice(streamInfo.Stream);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SDL3Audio] Failed to set pause state for stream {StreamId}", streamId);
            return false;
        }
    }

    public bool IsInitialized => _initialized;

    public int ActiveStreamCount => _audioStreams.Count;

    public void Dispose()
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                return;
            }

            // Destroy all audio streams
            foreach (var streamId in _audioStreams.Keys.ToArray())
            {
                DestroyAudioStream(streamId);
            }

            SDL.Quit();
            _initialized = false;
            _logger.LogInformation("[SDL3Audio] Audio backend disposed");
        }

        GC.SuppressFinalize(this);
    }
}
