using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Win32Emu.Gui.Views;

namespace Win32Emu.Tests.Gui;

public class MessageBoxWindowTests
{
	// Win32 MessageBox type constants
	private const uint MB_OK = 0x00000000;
	private const uint MB_OKCANCEL = 0x00000001;
	private const uint MB_ABORTRETRYIGNORE = 0x00000002;
	private const uint MB_YESNOCANCEL = 0x00000003;
	private const uint MB_YESNO = 0x00000004;
	private const uint MB_RETRYCANCEL = 0x00000005;

	// Icon constants
	private const uint MB_ICONERROR = 0x00000010;
	private const uint MB_ICONQUESTION = 0x00000020;
	private const uint MB_ICONWARNING = 0x00000030;
	private const uint MB_ICONINFORMATION = 0x00000040;

	[AvaloniaFact]
	public void MessageBox_WithMB_OK_ShouldHaveOneButton()
	{
		// Arrange & Act
		var messageBox = new MessageBoxWindow("Test Title", "Test Message", MB_OK);
		var buttonPanel = messageBox.FindControl<StackPanel>("ButtonPanel");

		// Assert
		Assert.NotNull(buttonPanel);
		var button = Assert.Single(buttonPanel.Children);
		Assert.IsType<Button>(button);
		Assert.Equal("OK", ((Button)button).Content);
	}

	[AvaloniaFact]
	public void MessageBox_WithMB_OKCANCEL_ShouldHaveTwoButtons()
	{
		// Arrange & Act
		var messageBox = new MessageBoxWindow("Test Title", "Test Message", MB_OKCANCEL);
		var buttonPanel = messageBox.FindControl<StackPanel>("ButtonPanel");

		// Assert
		Assert.NotNull(buttonPanel);
		Assert.Equal(2, buttonPanel.Children.Count);
		Assert.Equal("OK", ((Button)buttonPanel.Children[0]).Content);
		Assert.Equal("Cancel", ((Button)buttonPanel.Children[1]).Content);
	}

	[AvaloniaFact]
	public void MessageBox_WithMB_YESNO_ShouldHaveTwoButtons()
	{
		// Arrange & Act
		var messageBox = new MessageBoxWindow("Test Title", "Test Message", MB_YESNO);
		var buttonPanel = messageBox.FindControl<StackPanel>("ButtonPanel");

		// Assert
		Assert.NotNull(buttonPanel);
		Assert.Equal(2, buttonPanel.Children.Count);
		Assert.Equal("Yes", ((Button)buttonPanel.Children[0]).Content);
		Assert.Equal("No", ((Button)buttonPanel.Children[1]).Content);
	}

	[AvaloniaFact]
	public void MessageBox_WithMB_YESNOCANCEL_ShouldHaveThreeButtons()
	{
		// Arrange & Act
		var messageBox = new MessageBoxWindow("Test Title", "Test Message", MB_YESNOCANCEL);
		var buttonPanel = messageBox.FindControl<StackPanel>("ButtonPanel");

		// Assert
		Assert.NotNull(buttonPanel);
		Assert.Equal(3, buttonPanel.Children.Count);
		Assert.Equal("Yes", ((Button)buttonPanel.Children[0]).Content);
		Assert.Equal("No", ((Button)buttonPanel.Children[1]).Content);
		Assert.Equal("Cancel", ((Button)buttonPanel.Children[2]).Content);
	}

	[AvaloniaFact]
	public void MessageBox_WithMB_ABORTRETRYIGNORE_ShouldHaveThreeButtons()
	{
		// Arrange & Act
		var messageBox = new MessageBoxWindow("Test Title", "Test Message", MB_ABORTRETRYIGNORE);
		var buttonPanel = messageBox.FindControl<StackPanel>("ButtonPanel");

		// Assert
		Assert.NotNull(buttonPanel);
		Assert.Equal(3, buttonPanel.Children.Count);
		Assert.Equal("Abort", ((Button)buttonPanel.Children[0]).Content);
		Assert.Equal("Retry", ((Button)buttonPanel.Children[1]).Content);
		Assert.Equal("Ignore", ((Button)buttonPanel.Children[2]).Content);
	}

	[AvaloniaFact]
	public void MessageBox_WithMB_RETRYCANCEL_ShouldHaveTwoButtons()
	{
		// Arrange & Act
		var messageBox = new MessageBoxWindow("Test Title", "Test Message", MB_RETRYCANCEL);
		var buttonPanel = messageBox.FindControl<StackPanel>("ButtonPanel");

		// Assert
		Assert.NotNull(buttonPanel);
		Assert.Equal(2, buttonPanel.Children.Count);
		Assert.Equal("Retry", ((Button)buttonPanel.Children[0]).Content);
		Assert.Equal("Cancel", ((Button)buttonPanel.Children[1]).Content);
	}

	[AvaloniaFact]
	public void MessageBox_WithMB_ICONERROR_ShouldShowErrorIcon()
	{
		// Arrange & Act
		var messageBox = new MessageBoxWindow("Test Title", "Test Message", MB_OK | MB_ICONERROR);
		var iconText = messageBox.FindControl<TextBlock>("IconText");

		// Assert
		Assert.NotNull(iconText);
		Assert.True(iconText.IsVisible);
		Assert.Equal("❌", iconText.Text);
	}

	[AvaloniaFact]
	public void MessageBox_WithMB_ICONWARNING_ShouldShowWarningIcon()
	{
		// Arrange & Act
		var messageBox = new MessageBoxWindow("Test Title", "Test Message", MB_OK | MB_ICONWARNING);
		var iconText = messageBox.FindControl<TextBlock>("IconText");

		// Assert
		Assert.NotNull(iconText);
		Assert.True(iconText.IsVisible);
		Assert.Equal("⚠️", iconText.Text);
	}

	[AvaloniaFact]
	public void MessageBox_WithMB_ICONINFORMATION_ShouldShowInfoIcon()
	{
		// Arrange & Act
		var messageBox = new MessageBoxWindow("Test Title", "Test Message", MB_OK | MB_ICONINFORMATION);
		var iconText = messageBox.FindControl<TextBlock>("IconText");

		// Assert
		Assert.NotNull(iconText);
		Assert.True(iconText.IsVisible);
		Assert.Equal("ℹ️", iconText.Text);
	}

	[AvaloniaFact]
	public void MessageBox_WithMB_ICONQUESTION_ShouldShowQuestionIcon()
	{
		// Arrange & Act
		var messageBox = new MessageBoxWindow("Test Title", "Test Message", MB_OK | MB_ICONQUESTION);
		var iconText = messageBox.FindControl<TextBlock>("IconText");

		// Assert
		Assert.NotNull(iconText);
		Assert.True(iconText.IsVisible);
		Assert.Equal("❓", iconText.Text);
	}

	[AvaloniaFact]
	public void MessageBox_WithNoIcon_ShouldHideIcon()
	{
		// Arrange & Act
		var messageBox = new MessageBoxWindow("Test Title", "Test Message", MB_OK);
		var iconText = messageBox.FindControl<TextBlock>("IconText");

		// Assert
		Assert.NotNull(iconText);
		Assert.False(iconText.IsVisible);
	}

	[AvaloniaFact]
	public void MessageBox_ShouldSetTitleAndMessage()
	{
		// Arrange
		const string expectedTitle = "Test Title";
		const string expectedMessage = "Test Message";

		// Act
		var messageBox = new MessageBoxWindow(expectedTitle, expectedMessage, MB_OK);
		var messageText = messageBox.FindControl<TextBlock>("MessageText");

		// Assert
		Assert.Equal(expectedTitle, messageBox.Title);
		Assert.NotNull(messageText);
		Assert.Equal(expectedMessage, messageText.Text);
	}
}
