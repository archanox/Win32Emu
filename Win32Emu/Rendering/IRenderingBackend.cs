using System.Numerics;

namespace Win32Emu.Rendering;

/// <summary>
/// Vertex structure for hardware-accelerated rendering
/// </summary>
public struct Vertex
{
	/// <summary>
	/// Position in screen space (x, y, z)
	/// </summary>
	public Vector3 Position;

	/// <summary>
	/// Vertex color (r, g, b, a) in range [0, 1]
	/// </summary>
	public Vector4 Color;

	/// <summary>
	/// Texture coordinates (u, v)
	/// </summary>
	public Vector2 TexCoord;

	/// <summary>
	/// 1/w for perspective-correct interpolation
	/// </summary>
	public float Oow;
}

/// <summary>
/// Blend mode for alpha blending
/// </summary>
public enum BlendMode
{
	/// <summary>No blending</summary>
	Disabled,
	/// <summary>Alpha blending (src_alpha, 1-src_alpha)</summary>
	Alpha,
	/// <summary>Additive blending</summary>
	Additive,
	/// <summary>Multiplicative blending</summary>
	Multiplicative
}

/// <summary>
/// Depth test function
/// </summary>
public enum DepthTest
{
	/// <summary>Depth test disabled</summary>
	Disabled,
	/// <summary>Always pass</summary>
	Always,
	/// <summary>Pass if less than</summary>
	Less,
	/// <summary>Pass if less than or equal</summary>
	LessEqual,
	/// <summary>Pass if greater than</summary>
	Greater,
	/// <summary>Pass if greater than or equal</summary>
	GreaterEqual,
	/// <summary>Pass if equal</summary>
	Equal,
	/// <summary>Pass if not equal</summary>
	NotEqual
}

/// <summary>
/// Cull mode for triangle culling
/// </summary>
public enum CullMode
{
	/// <summary>No culling</summary>
	None,
	/// <summary>Cull front-facing triangles</summary>
	Front,
	/// <summary>Cull back-facing triangles</summary>
	Back
}

/// <summary>
/// Texture format
/// </summary>
public enum TextureFormat
{
	/// <summary>8-bit RGBA format</summary>
	RGBA8,
	/// <summary>16-bit RGB565 format</summary>
	RGB565,
	/// <summary>Palettized 8-bit format</summary>
	Palettized8,
	/// <summary>24-bit RGB format</summary>
	RGB24
}

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

	// Hardware-accelerated rendering methods

	/// <summary>
	/// Begin a new rendering frame (optional, for backends that need it)
	/// </summary>
	void BeginFrame();

	/// <summary>
	/// End the current rendering frame and present to screen
	/// </summary>
	void EndFrame();

	/// <summary>
	/// Draw a batch of triangles with hardware acceleration
	/// </summary>
	/// <param name="vertices">Vertex data</param>
	/// <param name="indices">Index data (triangles, 3 indices per triangle)</param>
	void DrawTriangles(Span<Vertex> vertices, Span<ushort> indices);

	/// <summary>
	/// Upload texture data to GPU
	/// </summary>
	/// <param name="textureId">Unique texture identifier</param>
	/// <param name="data">Texture data</param>
	/// <param name="width">Texture width</param>
	/// <param name="height">Texture height</param>
	/// <param name="format">Texture format</param>
	void SetTexture(uint textureId, byte[] data, int width, int height, TextureFormat format);

	/// <summary>
	/// Set the current active texture for rendering
	/// </summary>
	/// <param name="textureId">Texture identifier (0 for no texture)</param>
	void BindTexture(uint textureId);

	/// <summary>
	/// Set rendering state for subsequent draw calls
	/// </summary>
	/// <param name="blend">Blend mode</param>
	/// <param name="depth">Depth test function</param>
	/// <param name="cull">Cull mode</param>
	void SetRenderState(BlendMode blend, DepthTest depth, CullMode cull);

	/// <summary>
	/// Delete a texture from GPU memory
	/// </summary>
	/// <param name="textureId">Texture identifier</param>
	void DeleteTexture(uint textureId);
}
