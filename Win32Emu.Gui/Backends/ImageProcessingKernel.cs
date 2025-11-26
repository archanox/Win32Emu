namespace Win32Emu.Gui.Backends;
using Win32Emu.Rendering;

/// <summary>
/// Built-in image processing kernels
/// </summary>
public enum ImageProcessingKernel
{
	GaussianBlur,
	Sharpen,
	EdgeDetection,
	Grayscale,
	BrightnessContrast
}