using Microsoft.Extensions.Logging;
using Silk.NET.OpenAL;
using System.Runtime.InteropServices;

namespace Win32Emu.Gui.Backends;
using Win32Emu.Rendering;
/// <summary>
/// Silk.NET OpenAL-based audio backend for DirectSound operations
/// </summary>
public unsafe class SilkOpenAlAudioBackend : IAudioBackend
{
    private readonly ILogger _logger;
    private readonly AL _al;
    private readonly ALContext _alc;
    private Device* _device;
    private Context* _context;
    private bool _initialized;
    private readonly Lock _lock = new();
    private readonly Dictionary<uint, AudioStream> _audioStreams = new();
    private uint _nextStreamId = 1;

    private class AudioStream
    {
        public uint Id { get; set; }
        public uint Source { get; set; }
        public List<uint> Buffers { get; set; } = new();
        public int Frequency { get; set; }
        public int Channels { get; set; }
        public int BufferSize { get; set; }
    }

    public SilkOpenAlAudioBackend(ILogger logger)
    {
        _logger = logger;
        _al = AL.GetApi();
        _alc = ALContext.GetApi();
    }

    public Task<bool> InitializeAsync()
    {
        lock (_lock)
        {
            if (_initialized)
            {
                return Task.FromResult(true);
            }

            // Open default audio device
            _device = _alc.OpenDevice("");
            if (_device == null)
            {
                _logger.LogError("[SilkOpenAL] Failed to open audio device");
                return Task.FromResult(false);
            }

            // Create context
            _context = _alc.CreateContext(_device, null);
            if (_context == null)
            {
                _logger.LogError("[SilkOpenAL] Failed to create audio context");
                _alc.CloseDevice(_device);
                return Task.FromResult(false);
            }

            if (!_alc.MakeContextCurrent(_context))
            {
                _logger.LogError("[SilkOpenAL] Failed to make context current");
                _alc.DestroyContext(_context);
                _alc.CloseDevice(_device);
                return Task.FromResult(false);
            }

            _initialized = true;
            _logger.LogInformation("[SilkOpenAL] AudioBackend initialized");
            return Task.FromResult(true);
        }
    }

    public uint CreateAudioStream(int frequency, int channels, int bufferSize)
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                _logger.LogError("[SilkOpenAL] Not initialized");
                return 0;
            }

            var streamId = _nextStreamId++;
            
            // Generate OpenAL source
            var source = _al.GenSource();
            
            var stream = new AudioStream
            {
                Id = streamId,
                Source = source,
                Frequency = frequency,
                Channels = channels,
                BufferSize = bufferSize
            };

            // Generate some buffers for streaming
            for (var i = 0; i < 4; i++)
            {
                var buffer = _al.GenBuffer();
                stream.Buffers.Add(buffer);
            }

            _audioStreams[streamId] = stream;
            _logger.LogInformation("[SilkOpenAL] Created audio stream {StreamId}: {Frequency}Hz, {Channels}ch, {BufferSize} bytes", 
                                  streamId, frequency, channels, bufferSize);
            return streamId;
        }
    }

    public bool WriteAudioData(uint streamId, byte[] data, int offset, int length)
    {
        lock (_lock)
        {
            if (!_initialized || !_audioStreams.TryGetValue(streamId, out var stream))
            {
                _logger.LogError("[SilkOpenAL] Invalid stream {StreamId}", streamId);
                return false;
            }

            // Get a free buffer
            _al.GetSourceProperty(stream.Source, GetSourceInteger.BuffersProcessed, out var processed);
            
            uint bufferId;
            if (processed > 0)
            {
                // Unqueue a processed buffer
                uint tempBuffer = 0;
                _al.SourceUnqueueBuffers(stream.Source, 1, &tempBuffer);
                bufferId = tempBuffer;
            }
            else if (stream.Buffers.Count > 0)
            {
                // Use an unused buffer
                bufferId = stream.Buffers[0];
                stream.Buffers.RemoveAt(0);
            }
            else
            {
                // No buffers available
                return false;
            }

            // Determine format based on channels
            var format = stream.Channels == 2 ? BufferFormat.Stereo16 : BufferFormat.Mono16;

            // Copy audio data to buffer
            fixed (byte* ptr = &data[offset])
            {
                _al.BufferData(bufferId, format, ptr, length, stream.Frequency);
            }

            // Queue buffer
            var bufferPtr = &bufferId;
            _al.SourceQueueBuffers(stream.Source, 1, bufferPtr);

            // Start playing if not already
            _al.GetSourceProperty(stream.Source, GetSourceInteger.SourceState, out var state);
            if (state != (int)SourceState.Playing)
            {
                _al.SourcePlay(stream.Source);
            }

            return true;
        }
    }

    public bool DestroyAudioStream(uint streamId)
    {
        lock (_lock)
        {
            if (!_audioStreams.TryGetValue(streamId, out var stream))
            {
                return false;
            }

            // Stop source
            _al.SourceStop(stream.Source);

            // Delete buffers
            foreach (var buffer in stream.Buffers)
            {
                _al.DeleteBuffer(buffer);
            }

            // Delete source
            _al.DeleteSource(stream.Source);

            _audioStreams.Remove(streamId);
            _logger.LogInformation("[SilkOpenAL] Destroyed audio stream {StreamId}", streamId);
            return true;
        }
    }

    public bool SetStreamVolume(uint streamId, float volume)
    {
        lock (_lock)
        {
            if (!_audioStreams.TryGetValue(streamId, out var stream))
            {
                return false;
            }

            _al.SetSourceProperty(stream.Source, SourceFloat.Gain, volume);
            _logger.LogInformation("[SilkOpenAL] Stream {StreamId}: Set volume to {Volume}", streamId, volume);
            return true;
        }
    }

    public bool SetStreamPaused(uint streamId, bool paused)
    {
        lock (_lock)
        {
            if (!_audioStreams.TryGetValue(streamId, out var stream))
            {
                return false;
            }

            if (paused)
            {
                _al.SourcePause(stream.Source);
            }
            else
            {
                _al.SourcePlay(stream.Source);
            }

            _logger.LogInformation("[SilkOpenAL] Stream {StreamId}: {State}", streamId, paused ? "Paused" : "Resumed");
            return true;
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                return;
            }

            // Destroy all audio streams
            foreach (var stream in _audioStreams.Values.ToList())
            {
                DestroyAudioStream(stream.Id);
            }

            _audioStreams.Clear();

            // Destroy context and close device
            if (_context != null)
            {
                _alc.MakeContextCurrent(null);
                _alc.DestroyContext(_context);
                _context = null;
            }

            if (_device != null)
            {
                _alc.CloseDevice(_device);
                _device = null;
            }

            _initialized = false;
            _logger.LogInformation("[SilkOpenAL] Audio subsystem disposed");
        }
    }

    public bool IsInitialized => _initialized;
    public int ActiveStreamCount => _audioStreams.Count;
}
