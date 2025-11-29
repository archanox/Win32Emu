using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Win32Emu.Rendering;

namespace Win32Emu.Wasm.Backend;

/// <summary>
/// WASM-compatible audio backend using Web Audio API and JavaScript interop
/// </summary>
public class WasmAudioBackend : IAudioBackend
{
	private readonly IJSRuntime _jsRuntime;
	private readonly ILogger<WasmAudioBackend> _logger;
	private bool _initialized;
	private readonly Dictionary<uint, AudioStreamInfo> _streams = new();
	private uint _nextStreamId = 1;

	public bool IsInitialized => _initialized;
	public int ActiveStreamCount => _streams.Count;

	private class AudioStreamInfo
	{
		public int Frequency { get; set; }
		public int Channels { get; set; }
		public int BufferSize { get; set; }
		public float Volume { get; set; } = 1.0f;
		public bool Paused { get; set; }
	}

	public WasmAudioBackend(IJSRuntime jsRuntime, ILogger<WasmAudioBackend> logger)
	{
		_jsRuntime = jsRuntime;
		_logger = logger;
	}

	/// <summary>
	/// Initializes the audio backend. In WASM, this always returns true because:
	/// 1. We cannot use blocking calls (WASM is single-threaded)
	/// 2. Browser audio requires user interaction to start (autoplay policies)
	/// 3. The Blazor component (Home.razor) handles proper async initialization with user interaction
	/// </summary>
	/// <returns>Always true in WASM. Actual audio availability depends on browser state.</returns>
	public bool Initialize()
	{
		if (_initialized)
		{
			return true;
		}

		try
		{
			_logger.LogInformation("[WASM] Initializing audio backend");
			
			// Initialize Web Audio API through JavaScript using fire-and-forget pattern.
			// In WASM, we cannot use blocking calls like .Wait() or .Result because
			// WebAssembly runs on a single thread and doesn't support Monitor.Wait.
			// Audio initialization in browsers requires user interaction anyway, so
			// the Blazor component handles this properly with await before emulation.
			// Note: This always returns true - actual audio availability depends on
			// browser state and user interaction.
			_ = _jsRuntime.InvokeVoidAsync("initializeAudio");
			
			_initialized = true;
			_logger.LogInformation("[WASM] Audio backend initialized (actual availability depends on browser)");
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[WASM] Failed to initialize audio backend");
			return false;
		}
	}

	public uint CreateAudioStream(int frequency, int channels, int bufferSize)
	{
		if (!_initialized)
		{
			_logger.LogWarning("[WASM] Cannot create audio stream: backend not initialized");
			return 0;
		}

		try
		{
			var streamId = _nextStreamId++;
			var streamInfo = new AudioStreamInfo
			{
				Frequency = frequency,
				Channels = channels,
				BufferSize = bufferSize
			};

			_streams[streamId] = streamInfo;
			
			_logger.LogInformation("[WASM] Created audio stream {StreamId} ({Frequency}Hz, {Channels} channels, {BufferSize} buffer)", 
				streamId, frequency, channels, bufferSize);
			
			// Notify JavaScript about the new audio stream
			_jsRuntime.InvokeVoidAsync("createAudioStream", streamId, frequency, channels, bufferSize);
			
			return streamId;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[WASM] Failed to create audio stream");
			return 0;
		}
	}

	public bool WriteAudioData(uint streamId, byte[] data, int offset, int length)
	{
		if (!_initialized || !_streams.ContainsKey(streamId))
		{
			return false;
		}

		try
		{
			// Convert byte data to base64 for transfer to JavaScript
			var audioData = new byte[length];
			Array.Copy(data, offset, audioData, 0, length);
			var base64Data = Convert.ToBase64String(audioData);
			
			// Send audio data to JavaScript for playback
			_jsRuntime.InvokeVoidAsync("writeAudioData", streamId, base64Data);
			
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[WASM] Failed to write audio data for stream {StreamId}", streamId);
			return false;
		}
	}

	public bool DestroyAudioStream(uint streamId)
	{
		if (!_streams.ContainsKey(streamId))
		{
			return false;
		}

		try
		{
			_streams.Remove(streamId);
			_logger.LogInformation("[WASM] Destroyed audio stream {StreamId}", streamId);
			
			// Notify JavaScript
			_jsRuntime.InvokeVoidAsync("destroyAudioStream", streamId);
			
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[WASM] Failed to destroy audio stream {StreamId}", streamId);
			return false;
		}
	}

	public bool SetStreamVolume(uint streamId, float volume)
	{
		if (!_streams.TryGetValue(streamId, out var streamInfo))
		{
			return false;
		}

		try
		{
			streamInfo.Volume = Math.Clamp(volume, 0.0f, 1.0f);
			_jsRuntime.InvokeVoidAsync("setStreamVolume", streamId, streamInfo.Volume);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[WASM] Failed to set volume for stream {StreamId}", streamId);
			return false;
		}
	}

	public bool SetStreamPaused(uint streamId, bool paused)
	{
		if (!_streams.TryGetValue(streamId, out var streamInfo))
		{
			return false;
		}

		try
		{
			streamInfo.Paused = paused;
			_jsRuntime.InvokeVoidAsync("setStreamPaused", streamId, paused);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[WASM] Failed to set paused state for stream {StreamId}", streamId);
			return false;
		}
	}

	public void Dispose()
	{
		if (_initialized)
		{
			_logger.LogInformation("[WASM] Disposing audio backend");
			
			// Destroy all streams
			foreach (var streamId in _streams.Keys.ToList())
			{
				DestroyAudioStream(streamId);
			}
			
			_streams.Clear();
			_initialized = false;
		}
	}
}
