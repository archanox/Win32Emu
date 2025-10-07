using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace Win32Emu.Gui.Converters;

/// <summary>
/// Converts a thumbnail file path to a Bitmap for display
/// Returns null if path is null/empty or file doesn't exist
/// </summary>
public class ThumbnailPathConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string path && !string.IsNullOrEmpty(path) && File.Exists(path))
        {
            try
            {
                return new Bitmap(path);
            }
            catch
            {
                // If loading fails, return null to show fallback
                return null;
            }
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
