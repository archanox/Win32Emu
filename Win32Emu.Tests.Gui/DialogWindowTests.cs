using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Win32Emu.Gui.Views;
using Win32Emu.Win32;

namespace Win32Emu.Tests.Gui;

public class DialogWindowTests
{
    // Win32 Window Styles and Button Styles constants
    private const uint WS_DISABLED = 0x08000000;
    private const uint BS_PUSHBUTTON = 0x00000000;
    private const uint BS_DEFPUSHBUTTON = 0x00000001;
    private const uint BS_CHECKBOX = 0x00000002;
    private const uint BS_RADIOBUTTON = 0x00000004;
    private const uint ES_LEFT = 0x0000;
    private const uint SS_LEFT = 0x00000000;

    [AvaloniaFact]
    public void Button_WithWS_DISABLED_ShouldBeDisabled()
    {
        // Arrange
        
        var template = new DialogTemplate
        {
            Title = "Test Dialog",
            Width = 200,
            Height = 100,
            Items = new List<DialogItem>
            {
                new()
                {
                    Id = 1,
                    WindowClass = "BUTTON",
                    Title = "Disabled Button",
                    X = 10,
                    Y = 10,
                    Width = 50,
                    Height = 14,
                    Style = BS_PUSHBUTTON | WS_DISABLED
                }
            }
        };

        // Act
        var dialogWindow = new DialogWindow(template);
        var button = dialogWindow.GetControlById(1) as Button;

        // Assert
        Assert.NotNull(button);
        Assert.False(button.IsEnabled);
        Assert.Equal("Disabled Button", button.Content);
    }

    [AvaloniaFact]
    public void Button_WithoutWS_DISABLED_ShouldBeEnabled()
    {
        // Arrange
        var template = new DialogTemplate
        {
            Title = "Test Dialog",
            Width = 200,
            Height = 100,
            Items = new List<DialogItem>
            {
                new()
                {
                    Id = 1,
                    WindowClass = "BUTTON",
                    Title = "Enabled Button",
                    X = 10,
                    Y = 10,
                    Width = 50,
                    Height = 14,
                    Style = BS_PUSHBUTTON
                }
            }
        };

        // Act
        var dialogWindow = new DialogWindow(template);
        var button = dialogWindow.GetControlById(1) as Button;

        // Assert
        Assert.NotNull(button);
        Assert.True(button.IsEnabled);
        Assert.Equal("Enabled Button", button.Content);
    }

    [AvaloniaFact]
    public void CheckBox_WithWS_DISABLED_ShouldBeDisabled()
    {
        // Arrange
        var template = new DialogTemplate
        {
            Title = "Test Dialog",
            Width = 200,
            Height = 100,
            Items = new List<DialogItem>
            {
                new()
                {
                    Id = 2,
                    WindowClass = "BUTTON",
                    Title = "Disabled Checkbox",
                    X = 10,
                    Y = 30,
                    Width = 70,
                    Height = 14,
                    Style = BS_CHECKBOX | WS_DISABLED
                }
            }
        };

        // Act
        var dialogWindow = new DialogWindow(template);
        var checkbox = dialogWindow.GetControlById(2) as CheckBox;

        // Assert
        Assert.NotNull(checkbox);
        Assert.False(checkbox.IsEnabled);
        Assert.Equal("Disabled Checkbox", checkbox.Content);
    }

    [AvaloniaFact]
    public void CheckBox_WithoutWS_DISABLED_ShouldBeEnabled()
    {
        // Arrange
        var template = new DialogTemplate
        {
            Title = "Test Dialog",
            Width = 200,
            Height = 100,
            Items = new List<DialogItem>
            {
                new()
                {
                    Id = 2,
                    WindowClass = "BUTTON",
                    Title = "Enabled Checkbox",
                    X = 10,
                    Y = 30,
                    Width = 70,
                    Height = 14,
                    Style = BS_CHECKBOX
                }
            }
        };

        // Act
        var dialogWindow = new DialogWindow(template);
        var checkbox = dialogWindow.GetControlById(2) as CheckBox;

        // Assert
        Assert.NotNull(checkbox);
        Assert.True(checkbox.IsEnabled);
        Assert.Equal("Enabled Checkbox", checkbox.Content);
    }

    [AvaloniaFact]
    public void RadioButton_WithWS_DISABLED_ShouldBeDisabled()
    {
        // Arrange
        var template = new DialogTemplate
        {
            Title = "Test Dialog",
            Width = 200,
            Height = 100,
            Items = new List<DialogItem>
            {
                new()
                {
                    Id = 3,
                    WindowClass = "BUTTON",
                    Title = "Disabled Radio",
                    X = 10,
                    Y = 50,
                    Width = 70,
                    Height = 14,
                    Style = BS_RADIOBUTTON | WS_DISABLED
                }
            }
        };

        // Act
        var dialogWindow = new DialogWindow(template);
        var radioButton = dialogWindow.GetControlById(3) as RadioButton;

        // Assert
        Assert.NotNull(radioButton);
        Assert.False(radioButton.IsEnabled);
        Assert.Equal("Disabled Radio", radioButton.Content);
    }

    [AvaloniaFact]
    public void RadioButton_WithoutWS_DISABLED_ShouldBeEnabled()
    {
        // Arrange
        var template = new DialogTemplate
        {
            Title = "Test Dialog",
            Width = 200,
            Height = 100,
            Items = new List<DialogItem>
            {
                new()
                {
                    Id = 3,
                    WindowClass = "BUTTON",
                    Title = "Enabled Radio",
                    X = 10,
                    Y = 50,
                    Width = 70,
                    Height = 14,
                    Style = BS_RADIOBUTTON
                }
            }
        };

        // Act
        var dialogWindow = new DialogWindow(template);
        var radioButton = dialogWindow.GetControlById(3) as RadioButton;

        // Assert
        Assert.NotNull(radioButton);
        Assert.True(radioButton.IsEnabled);
        Assert.Equal("Enabled Radio", radioButton.Content);
    }

    [AvaloniaFact]
    public void TextBox_WithWS_DISABLED_ShouldBeDisabled()
    {
        // Arrange
        var template = new DialogTemplate
        {
            Title = "Test Dialog",
            Width = 200,
            Height = 100,
            Items = new List<DialogItem>
            {
                new()
                {
                    Id = 4,
                    WindowClass = "EDIT",
                    Title = "Initial Text",
                    X = 10,
                    Y = 70,
                    Width = 100,
                    Height = 14,
                    Style = ES_LEFT | WS_DISABLED
                }
            }
        };

        // Act
        var dialogWindow = new DialogWindow(template);
        var textBox = dialogWindow.GetControlById(4) as TextBox;

        // Assert
        Assert.NotNull(textBox);
        Assert.False(textBox.IsEnabled);
        Assert.Equal("Initial Text", textBox.Text);
    }

    [AvaloniaFact]
    public void TextBox_WithoutWS_DISABLED_ShouldBeEnabled()
    {
        // Arrange
        var template = new DialogTemplate
        {
            Title = "Test Dialog",
            Width = 200,
            Height = 100,
            Items = new List<DialogItem>
            {
                new()
                {
                    Id = 4,
                    WindowClass = "EDIT",
                    Title = "Initial Text",
                    X = 10,
                    Y = 70,
                    Width = 100,
                    Height = 14,
                    Style = ES_LEFT
                }
            }
        };

        // Act
        var dialogWindow = new DialogWindow(template);
        var textBox = dialogWindow.GetControlById(4) as TextBox;

        // Assert
        Assert.NotNull(textBox);
        Assert.True(textBox.IsEnabled);
        Assert.Equal("Initial Text", textBox.Text);
    }

    [AvaloniaFact]
    public void DefaultPushButton_ShouldHaveIsDefaultTrue()
    {
        // Arrange
        var template = new DialogTemplate
        {
            Title = "Test Dialog",
            Width = 200,
            Height = 100,
            Items = new List<DialogItem>
            {
                new()
                {
                    Id = 1,
                    WindowClass = "BUTTON",
                    Title = "OK",
                    X = 10,
                    Y = 10,
                    Width = 50,
                    Height = 14,
                    Style = BS_DEFPUSHBUTTON
                }
            }
        };

        // Act
        var dialogWindow = new DialogWindow(template);
        var button = dialogWindow.GetControlById(1) as Button;

        // Assert
        Assert.NotNull(button);
        Assert.True(button.IsDefault);
        Assert.True(button.IsEnabled);
    }

    [AvaloniaFact]
    public void MultipleControls_WithMixedDisabledStates_ShouldRespectIndividualStates()
    {
        // Arrange
        var template = new DialogTemplate
        {
            Title = "Test Dialog",
            Width = 200,
            Height = 150,
            Items = new List<DialogItem>
            {
                new()
                {
                    Id = 1,
                    WindowClass = "BUTTON",
                    Title = "Enabled Button",
                    X = 10,
                    Y = 10,
                    Width = 50,
                    Height = 14,
                    Style = BS_PUSHBUTTON
                },
                new()
                {
                    Id = 2,
                    WindowClass = "BUTTON",
                    Title = "Disabled Button",
                    X = 10,
                    Y = 30,
                    Width = 50,
                    Height = 14,
                    Style = BS_PUSHBUTTON | WS_DISABLED
                },
                new()
                {
                    Id = 3,
                    WindowClass = "BUTTON",
                    Title = "Enabled Checkbox",
                    X = 10,
                    Y = 50,
                    Width = 70,
                    Height = 14,
                    Style = BS_CHECKBOX
                },
                new()
                {
                    Id = 4,
                    WindowClass = "BUTTON",
                    Title = "Disabled Checkbox",
                    X = 10,
                    Y = 70,
                    Width = 70,
                    Height = 14,
                    Style = BS_CHECKBOX | WS_DISABLED
                }
            }
        };

        // Act
        var dialogWindow = new DialogWindow(template);
        var enabledButton = dialogWindow.GetControlById(1) as Button;
        var disabledButton = dialogWindow.GetControlById(2) as Button;
        var enabledCheckbox = dialogWindow.GetControlById(3) as CheckBox;
        var disabledCheckbox = dialogWindow.GetControlById(4) as CheckBox;

        // Assert
        Assert.NotNull(enabledButton);
        Assert.True(enabledButton.IsEnabled);
        
        Assert.NotNull(disabledButton);
        Assert.False(disabledButton.IsEnabled);
        
        Assert.NotNull(enabledCheckbox);
        Assert.True(enabledCheckbox.IsEnabled);
        
        Assert.NotNull(disabledCheckbox);
        Assert.False(disabledCheckbox.IsEnabled);
    }

    [AvaloniaFact]
    public void StaticText_ShouldBeCreatedCorrectly()
    {
        // Arrange
        var template = new DialogTemplate
        {
            Title = "Test Dialog",
            Width = 200,
            Height = 100,
            Items = new List<DialogItem>
            {
                new()
                {
                    Id = 5,
                    WindowClass = "STATIC",
                    Title = "Label Text",
                    X = 10,
                    Y = 10,
                    Width = 80,
                    Height = 14,
                    Style = SS_LEFT
                }
            }
        };

        // Act
        var dialogWindow = new DialogWindow(template);
        var staticControl = dialogWindow.GetControlById(5) as TextBlock;

        // Assert
        Assert.NotNull(staticControl);
        Assert.Equal("Label Text", staticControl.Text);
    }
}
