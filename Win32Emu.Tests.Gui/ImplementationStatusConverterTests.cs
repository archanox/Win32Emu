using System.Globalization;
using Avalonia.Media;
using Win32Emu.Gui.Converters;
using Win32Emu.Gui.ViewModels;

namespace Win32Emu.Tests.Gui;

public class ImplementationStatusConverterTests
{
    [Fact]
    public void ImplementationStatusToTextConverter_WithImplemented_ReturnsCorrectText()
    {
        // Arrange
        var converter = new ImplementationStatusToTextConverter();

        // Act
        var result = converter.Convert(ImplementationStatus.Implemented, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("✓ Implemented", result);
    }

    [Fact]
    public void ImplementationStatusToTextConverter_WithPartial_ReturnsCorrectText()
    {
        // Arrange
        var converter = new ImplementationStatusToTextConverter();

        // Act
        var result = converter.Convert(ImplementationStatus.Partial, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("⚠ Partial", result);
    }

    [Fact]
    public void ImplementationStatusToTextConverter_WithNotImplemented_ReturnsCorrectText()
    {
        // Arrange
        var converter = new ImplementationStatusToTextConverter();

        // Act
        var result = converter.Convert(ImplementationStatus.NotImplemented, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("✗ Not Implemented", result);
    }

    [Fact]
    public void ImplementationStatusToTextConverter_WithInvalidValue_ReturnsUnknown()
    {
        // Arrange
        var converter = new ImplementationStatusToTextConverter();

        // Act
        var result = converter.Convert("invalid", typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("Unknown", result);
    }

    [Fact]
    public void ImplementationStatusToColorConverter_WithImplemented_ReturnsGreen()
    {
        // Arrange
        var converter = new ImplementationStatusToColorConverter();

        // Act
        var result = converter.Convert(ImplementationStatus.Implemented, typeof(SolidColorBrush), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<SolidColorBrush>(result);
        var brush = (SolidColorBrush)result;
        Assert.Equal(Color.Parse("#28A745"), brush.Color);
    }

    [Fact]
    public void ImplementationStatusToColorConverter_WithPartial_ReturnsYellow()
    {
        // Arrange
        var converter = new ImplementationStatusToColorConverter();

        // Act
        var result = converter.Convert(ImplementationStatus.Partial, typeof(SolidColorBrush), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<SolidColorBrush>(result);
        var brush = (SolidColorBrush)result;
        Assert.Equal(Color.Parse("#FFC107"), brush.Color);
    }

    [Fact]
    public void ImplementationStatusToColorConverter_WithNotImplemented_ReturnsRed()
    {
        // Arrange
        var converter = new ImplementationStatusToColorConverter();

        // Act
        var result = converter.Convert(ImplementationStatus.NotImplemented, typeof(SolidColorBrush), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<SolidColorBrush>(result);
        var brush = (SolidColorBrush)result;
        Assert.Equal(Color.Parse("#DC3545"), brush.Color);
    }

    [Fact]
    public void ImplementationStatusToColorConverter_WithInvalidValue_ReturnsGray()
    {
        // Arrange
        var converter = new ImplementationStatusToColorConverter();

        // Act
        var result = converter.Convert("invalid", typeof(SolidColorBrush), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<SolidColorBrush>(result);
        var brush = (SolidColorBrush)result;
        Assert.Equal(Colors.Gray, brush.Color);
    }

    [Fact]
    public void ImplementationStatusToTextConverter_ConvertBack_ThrowsNotImplementedException()
    {
        // Arrange
        var converter = new ImplementationStatusToTextConverter();

        // Act & Assert
        Assert.Throws<NotImplementedException>(() =>
            converter.ConvertBack(null, typeof(ImplementationStatus), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ImplementationStatusToColorConverter_ConvertBack_ThrowsNotImplementedException()
    {
        // Arrange
        var converter = new ImplementationStatusToColorConverter();

        // Act & Assert
        Assert.Throws<NotImplementedException>(() =>
            converter.ConvertBack(null, typeof(ImplementationStatus), null, CultureInfo.InvariantCulture));
    }
}
