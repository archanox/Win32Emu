using System.Collections.ObjectModel;
using Win32Emu.Gui.ViewModels;

namespace Win32Emu.Tests.Gui;

public class EmulatorWindowViewModelTests
{
	[Fact]
	public void GetDebugOutputText_ReturnsFormattedOutput()
	{
		// Arrange
		var viewModel = new EmulatorWindowViewModel();
		
		// Directly add messages to the collection to bypass UI thread requirement
		viewModel.DebugMessages.Add(new DebugMessage { Timestamp = DateTime.Now, Level = DebugLevel.Info, Message = "Test message 1" });
		viewModel.DebugMessages.Add(new DebugMessage { Timestamp = DateTime.Now, Level = DebugLevel.Warning, Message = "Test message 2" });
		viewModel.DebugMessages.Add(new DebugMessage { Timestamp = DateTime.Now, Level = DebugLevel.Error, Message = "Test message 3" });
		
		// Act - Use reflection to call private method
		var method = typeof(EmulatorWindowViewModel).GetMethod("GetDebugOutputText", 
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		Assert.NotNull(method);
		
		var result = method.Invoke(viewModel, null) as string;
		
		// Assert
		Assert.NotNull(result);
		Assert.Contains("Win32Emu Debug Output", result);
		Assert.Contains("Total Messages: 3", result);
		Assert.Contains("Test message 1", result);
		Assert.Contains("Test message 2", result);
		Assert.Contains("Test message 3", result);
		Assert.Contains("[Info]", result);
		Assert.Contains("[Warning]", result);
		Assert.Contains("[Error]", result);
	}
	
	[Fact]
	public void GetDebugOutputText_TruncatesAt65535Characters_ShowsLastMessages()
	{
		// Arrange
		var viewModel = new EmulatorWindowViewModel();
		
		// Add many messages to exceed the limit
		var longMessage = new string('A', 1000);
		for (int i = 0; i < 100; i++)
		{
			viewModel.DebugMessages.Add(new DebugMessage 
			{ 
				Timestamp = DateTime.Now, 
				Level = DebugLevel.Info, 
				Message = $"Message {i}: {longMessage}" 
			});
		}
		
		// Act
		var method = typeof(EmulatorWindowViewModel).GetMethod("GetDebugOutputText", 
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		Assert.NotNull(method);
		
		var result = method.Invoke(viewModel, null) as string;
		
		// Assert
		Assert.NotNull(result);
		Assert.True(result.Length <= 65535, $"Output length {result.Length} exceeds 65535 characters");
		Assert.Contains("showing last 65535 characters", result);
		
		// Verify it shows the LAST messages (higher message numbers should be present)
		Assert.Contains("Message 99:", result); // Last message should be there
		// First messages should NOT be there when truncated
		Assert.DoesNotContain("Message 0:", result);
	}
	
	[Fact]
	public void GetDebugOutputText_IncludesTimestampAndHeader()
	{
		// Arrange
		var viewModel = new EmulatorWindowViewModel();
		viewModel.DebugMessages.Add(new DebugMessage { Timestamp = DateTime.Now, Level = DebugLevel.Debug, Message = "Test message" });
		
		// Act
		var method = typeof(EmulatorWindowViewModel).GetMethod("GetDebugOutputText", 
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		Assert.NotNull(method);
		
		var result = method.Invoke(viewModel, null) as string;
		
		// Assert
		Assert.NotNull(result);
		Assert.Contains("=== Win32Emu Debug Output ===", result);
		Assert.Contains("Timestamp:", result);
		Assert.Contains("Total Messages:", result);
	}
	
	[Fact]
	public void CopyDebugOutputCommand_Exists()
	{
		// Arrange
		var viewModel = new EmulatorWindowViewModel();
		
		// Assert
		Assert.NotNull(viewModel.CopyDebugOutputCommand);
	}
	
	[Fact]
	public void GetDebugOutputText_WithNoMessages_ReturnsHeaderOnly()
	{
		// Arrange
		var viewModel = new EmulatorWindowViewModel();
		
		// Act
		var method = typeof(EmulatorWindowViewModel).GetMethod("GetDebugOutputText", 
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		Assert.NotNull(method);
		
		var result = method.Invoke(viewModel, null) as string;
		
		// Assert
		Assert.NotNull(result);
		Assert.Contains("Win32Emu Debug Output", result);
		Assert.Contains("Total Messages: 0", result);
	}
}
