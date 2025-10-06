using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Win32Emu.Gui.Converters;

/// <summary>
/// Converts a boolean to "Implemented" or "Not Implemented" text
/// </summary>
public class BoolToImplementedConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isImplemented)
        {
            return isImplemented ? "✓ Yes" : "✗ No";
        }
        return "Unknown";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a boolean to a color (green for true, red for false)
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isImplemented)
        {
            return isImplemented 
                ? new SolidColorBrush(Color.Parse("#28A745")) 
                : new SolidColorBrush(Color.Parse("#DC3545"));
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
