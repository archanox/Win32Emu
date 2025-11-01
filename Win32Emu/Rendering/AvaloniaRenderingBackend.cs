using Microsoft.Extensions.Logging;

namespace Win32Emu.Rendering;

/// <summary>
/// Rendering backend that integrates with Avalonia UI.
/// Instead of creating separate SDL windows, this backend routes frame buffer updates
/// to the IEmulatorHost interface which displays them in Avalonia UI controls.
/// </summary>
public class AvaloniaRenderingBackend : IRenderingBackend
{
    private readonly ILogger _logger;
    private readonly IEmulatorHost _host;
    private int _width;
    private int _height;
    private bool _initialized;
    private readonly object _lock = new();
    private bool _disposed;

    /// <summary>
    /// Event fired when a UI event occurs (mouse, keyboard, window)
    /// Note: For Avalonia integration, UI events come from Avalonia controls,
    /// so this event is not used by this backend.
    /// </summary>
    public event EventHandler<UIEventArgs>? UIEvent;

    public AvaloniaRenderingBackend(ILogger logger, IEmulatorHost host)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _host = host ?? throw new ArgumentNullException(nameof(host));
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

            _logger.LogInformation("[Avalonia] Initializing Avalonia rendering backend ({Width}x{Height})...", width, height);

            _initialized = true;
            _logger.LogInformation("[Avalonia] Avalonia rendering backend initialized successfully");
            return true;
        }
    }

    public byte[] ConvertPalettizedToRGBA(byte[] indexedData, uint[] palette, int width, int height, int pitch)
    {
        if (indexedData == null)
            throw new ArgumentNullException(nameof(indexedData));
        if (palette == null)
            throw new ArgumentNullException(nameof(palette));

        _logger.LogDebug("[Avalonia] Converting palettized data to RGBA: {Width}x{Height}, pitch={Pitch}", width, height, pitch);

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

        _logger.LogDebug("[Avalonia] Converting 16-bit RGB565 to RGBA: {Width}x{Height}, pitch={Pitch}", width, height, pitch);

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

        _logger.LogDebug("[Avalonia] Converting 24-bit RGB to RGBA: {Width}x{Height}, pitch={Pitch}", width, height, pitch);

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
                _logger.LogWarning("[Avalonia] Cannot update frame buffer: backend not initialized");
                return false;
            }

            if (data == null)
            {
                _logger.LogWarning("[Avalonia] Cannot update frame buffer: data is null");
                return false;
            }

            try
            {
                // Calculate expected data size
                var expectedSize = _width * _height * 4; // RGBA format

                if (data.Length < expectedSize)
                {
                    _logger.LogWarning("[Avalonia] Data size ({DataSize}) is less than expected size ({ExpectedSize})", 
                        data.Length, expectedSize);
                }

                // Copy data to a new array to avoid modification after passing to GUI
                var bufferCopy = new byte[expectedSize];
                var copySize = Math.Min(data.Length, bufferCopy.Length);
                Array.Copy(data, 0, bufferCopy, 0, copySize);

                // Notify the host that display has been updated
                _host.OnDisplayUpdate(new DisplayUpdateInfo
                {
                    FrameBuffer = bufferCopy,
                    Width = _width,
                    Height = _height,
                    Stride = pitch
                });

                _logger.LogTrace("[Avalonia] Frame buffer updated: {Size} bytes, {Width}x{Height}", copySize, _width, _height);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Avalonia] Failed to update frame buffer");
                return false;
            }
        }
    }

    public void Clear(byte r, byte g, byte b, byte a = 255)
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                return;
            }

            _logger.LogDebug("[Avalonia] Clearing with color ({R}, {G}, {B}, {A})", r, g, b, a);

            // Create a solid color buffer
            var bufferSize = _width * _height * 4;
            var clearBuffer = new byte[bufferSize];
            
            for (var i = 0; i < bufferSize; i += 4)
            {
                clearBuffer[i] = r;
                clearBuffer[i + 1] = g;
                clearBuffer[i + 2] = b;
                clearBuffer[i + 3] = a;
            }

            // Update the display with the clear color
            UpdateFrameBuffer(clearBuffer, _width * 4);
        }
    }

    public void ProcessEvents()
    {
        // For Avalonia integration, event processing is handled by Avalonia itself
        // through the EmulatorWindowViewModel which captures Avalonia UI events
        // and routes them to the Win32 message queue via PostMessage.
        // This method is a no-op for this backend.
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

            _logger.LogInformation("[Avalonia] Disposing Avalonia rendering backend");
            _initialized = false;
            _disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}
