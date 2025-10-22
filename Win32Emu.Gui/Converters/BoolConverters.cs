using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Win32Emu.Gui.ViewModels;

namespace Win32Emu.Gui.Converters;

/// <summary>
/// Converts ImplementationStatus to display text
/// </summary>
public class ImplementationStatusToTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ImplementationStatus status)
        {
            return status switch
            {
                ImplementationStatus.Implemented => "✓ Implemented",
                ImplementationStatus.Partial => "⚠ Partial",
                ImplementationStatus.NotImplemented => "✗ Not Implemented",
                _ => "Unknown"
            };
        }
        return "Unknown";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts ImplementationStatus to color
/// </summary>
public class ImplementationStatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ImplementationStatus status)
        {
            return status switch
            {
                ImplementationStatus.Implemented => new SolidColorBrush(Color.Parse("#28A745")),
                ImplementationStatus.Partial => new SolidColorBrush(Color.Parse("#FFC107")),
                ImplementationStatus.NotImplemented => new SolidColorBrush(Color.Parse("#DC3545")),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

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
