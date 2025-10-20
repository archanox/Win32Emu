namespace Win32Emu.Rendering;

/// <summary>
/// Interface for rendering backends (SDL, GLFW, etc.)
/// </summary>
public interface IRenderingBackend : IDisposable
{
    /// <summary>
    /// Initialize the rendering backend with specified dimensions
    /// </summary>
    bool Initialize(int width, int height, string title = "Win32Emu Display");

    /// <summary>
    /// Convert palettized (8-bit indexed) surface to RGBA format
    /// </summary>
    byte[] ConvertPalettizedToRGBA(byte[] indexedData, uint[] palette, int width, int height, int pitch);

    /// <summary>
    /// Convert 16-bit RGB565 surface to RGBA format
    /// </summary>
    byte[] Convert16BitToRGBA(byte[] rgb565Data, int width, int height, int pitch);

    /// <summary>
    /// Convert 24-bit RGB/BGR surface to RGBA format
    /// </summary>
    byte[] Convert24BitToRGBA(byte[] rgb24Data, int width, int height, int pitch);

    /// <summary>
    /// Update the display with new frame buffer data
    /// </summary>
    bool UpdateFrameBuffer(byte[] data, int pitch);

    /// <summary>
    /// Clear the display with specified color
    /// </summary>
    void Clear(byte r, byte g, byte b, byte a = 255);

    /// <summary>
    /// Process events (call periodically)
    /// </summary>
    void ProcessEvents();

    /// <summary>
    /// Event fired when a UI event occurs (mouse, keyboard, window)
    /// </summary>
    event EventHandler<UIEventArgs>? UIEvent;

    /// <summary>
    /// Gets whether the backend is initialized
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Gets the width of the display
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the height of the display
    /// </summary>
    int Height { get; }
}
