using Microsoft.Extensions.Logging;

namespace Win32Emu.Rendering;

/// <summary>
/// Software (CPU-based) rendering backend for DirectDraw operations.
/// This backend does not require GPU acceleration and is suitable for macOS and other platforms
/// where hardware-accelerated rendering may not be available or desired.
/// </summary>
public class SoftwareRenderingBackend : IRenderingBackend
{
    private readonly ILogger _logger;
    private int _width;
    private int _height;
    private bool _initialized;
    private readonly object _lock = new();
    private bool _disposed;
    private byte[]? _frameBuffer;

    /// <summary>
    /// Event fired when a UI event occurs (mouse, keyboard, window)
    /// Note: Software backend does not generate UI events as it has no window
    /// </summary>
    public event EventHandler<UIEventArgs>? UIEvent;

    public SoftwareRenderingBackend(ILogger logger)
    {
        _logger = logger;
    }

    public bool Initialize(int width, int height, string title = "Win32Emu Display")
    {
        lock (_lock)
        {
            if (_initialized)
            {
                return true;
            }

            _width = width;
            _height = height;

            try
            {
                _logger.LogInformation("[Software] Initializing software rendering backend ({Width}x{Height})...", width, height);

                // Allocate frame buffer (RGBA format)
                var bufferSize = width * height * 4; // 4 bytes per pixel (RGBA)
                _frameBuffer = new byte[bufferSize];

                _initialized = true;
                _logger.LogInformation("[Software] Software rendering backend initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Software] Failed to initialize software rendering backend");
                return false;
            }
        }
    }

    public byte[] ConvertPalettizedToRGBA(byte[] indexedData, uint[] palette, int width, int height, int pitch)
    {
        if (indexedData == null)
            throw new ArgumentNullException(nameof(indexedData));
        if (palette == null)
            throw new ArgumentNullException(nameof(palette));

        _logger.LogDebug("[Software] Converting palettized data to RGBA: {Width}x{Height}, pitch={Pitch}", width, height, pitch);

        var rgbaData = new byte[width * height * 4];
        var rgbaIndex = 0;

        for (var y = 0; y < height; y++)
        {
            var rowOffset = y * pitch;
            for (var x = 0; x < width; x++)
            {
                var paletteIndex = indexedData[rowOffset + x];
                var color = palette[paletteIndex];

                // Extract RGBA components from palette entry (format: 0xAABBGGRR or 0x00BBGGRR)
                var r = (byte)(color & 0xFF);
                var g = (byte)((color >> 8) & 0xFF);
                var b = (byte)((color >> 16) & 0xFF);
                var a = (byte)((color >> 24) & 0xFF);

                // If alpha is 0, assume fully opaque
                if (a == 0)
                    a = 0xFF;

                rgbaData[rgbaIndex++] = r;
                rgbaData[rgbaIndex++] = g;
                rgbaData[rgbaIndex++] = b;
                rgbaData[rgbaIndex++] = a;
            }
        }

        return rgbaData;
    }

    public byte[] Convert16BitToRGBA(byte[] rgb565Data, int width, int height, int pitch)
    {
        if (rgb565Data == null)
            throw new ArgumentNullException(nameof(rgb565Data));

        _logger.LogDebug("[Software] Converting 16-bit RGB565 to RGBA: {Width}x{Height}, pitch={Pitch}", width, height, pitch);

        var rgbaData = new byte[width * height * 4];
        var rgbaIndex = 0;

        for (var y = 0; y < height; y++)
        {
            var rowOffset = y * pitch;
            for (var x = 0; x < width; x++)
            {
                var pixelOffset = rowOffset + (x * 2);
                var pixel = (ushort)(rgb565Data[pixelOffset] | (rgb565Data[pixelOffset + 1] << 8));

                // Extract RGB565 components
                var r5 = (pixel >> 11) & 0x1F;
                var g6 = (pixel >> 5) & 0x3F;
                var b5 = pixel & 0x1F;

                // Convert to 8-bit values
                var r = (byte)((r5 << 3) | (r5 >> 2));
                var g = (byte)((g6 << 2) | (g6 >> 4));
                var b = (byte)((b5 << 3) | (b5 >> 2));

                rgbaData[rgbaIndex++] = r;
                rgbaData[rgbaIndex++] = g;
                rgbaData[rgbaIndex++] = b;
                rgbaData[rgbaIndex++] = 0xFF; // Fully opaque
            }
        }

        return rgbaData;
    }

    public byte[] Convert24BitToRGBA(byte[] rgb24Data, int width, int height, int pitch)
    {
        if (rgb24Data == null)
            throw new ArgumentNullException(nameof(rgb24Data));

        _logger.LogDebug("[Software] Converting 24-bit RGB to RGBA: {Width}x{Height}, pitch={Pitch}", width, height, pitch);

        var rgbaData = new byte[width * height * 4];
        var rgbaIndex = 0;

        for (var y = 0; y < height; y++)
        {
            var rowOffset = y * pitch;
            for (var x = 0; x < width; x++)
            {
                var pixelOffset = rowOffset + (x * 3);

                // 24-bit is typically BGR format in Windows
                var b = rgb24Data[pixelOffset];
                var g = rgb24Data[pixelOffset + 1];
                var r = rgb24Data[pixelOffset + 2];

                rgbaData[rgbaIndex++] = r;
                rgbaData[rgbaIndex++] = g;
                rgbaData[rgbaIndex++] = b;
                rgbaData[rgbaIndex++] = 0xFF; // Fully opaque
            }
        }

        return rgbaData;
    }

    public bool UpdateFrameBuffer(byte[] data, int pitch)
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                _logger.LogWarning("[Software] Cannot update frame buffer: backend not initialized");
                return false;
            }

            if (_frameBuffer == null)
            {
                _logger.LogWarning("[Software] Cannot update frame buffer: frame buffer is null");
                return false;
            }

            if (data == null)
            {
                _logger.LogWarning("[Software] Cannot update frame buffer: data is null");
                return false;
            }

            try
            {
                // Calculate expected data size
                var expectedSize = _width * _height * 4; // RGBA format

                if (data.Length < expectedSize)
                {
                    _logger.LogWarning("[Software] Data size ({DataSize}) is less than expected size ({ExpectedSize})", 
                        data.Length, expectedSize);
                }

                // Copy data to frame buffer
                var copySize = Math.Min(data.Length, _frameBuffer.Length);
                Array.Copy(data, 0, _frameBuffer, 0, copySize);

                _logger.LogDebug("[Software] Frame buffer updated: {Size} bytes copied", copySize);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Software] Failed to update frame buffer");
                return false;
            }
        }
    }

    public void Clear(byte r, byte g, byte b, byte a = 255)
    {
        lock (_lock)
        {
            if (!_initialized || _frameBuffer == null)
            {
                return;
            }

            _logger.LogDebug("[Software] Clearing frame buffer with color ({R}, {G}, {B}, {A})", r, g, b, a);

            // Fill frame buffer with specified color
            for (var i = 0; i < _frameBuffer.Length; i += 4)
            {
                _frameBuffer[i] = r;
                _frameBuffer[i + 1] = g;
                _frameBuffer[i + 2] = b;
                _frameBuffer[i + 3] = a;
            }
        }
    }

    public void ProcessEvents()
    {
        // Software backend has no window, so no events to process
        // This method is a no-op but required by the interface
    }

    public bool IsInitialized => _initialized;

    public int Width => _width;

    public int Height => _height;

    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            _logger.LogInformation("[Software] Disposing software rendering backend");

            _frameBuffer = null;
            _initialized = false;
            _disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}
