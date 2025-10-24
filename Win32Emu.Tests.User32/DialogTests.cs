using Win32Emu.Tests.User32.TestInfrastructure;

namespace Win32Emu.Tests.User32;

/// <summary>
/// Tests for User32 dialog functions
/// </summary>
public class DialogTests : IDisposable
{
	private readonly TestEnvironment _testEnv;

	public DialogTests()
	{
		_testEnv = new TestEnvironment();
	}

	[Fact]
	public void DialogState_InitializeAndEnd_ShouldWorkCorrectly()
	{
		// Arrange
		const uint hDlg = 0x00010000;

		// Act - Initialize dialog state
		_testEnv.ProcessEnv.InitializeDialogState(hDlg);

		// Assert - Dialog should not be ended initially
		Assert.False(_testEnv.ProcessEnv.IsDialogEnded(hDlg));

		// Act - Set dialog result
		const uint expectedResult = 42;
		var success = _testEnv.ProcessEnv.SetDialogResult(hDlg, expectedResult);

		// Assert - Should succeed and dialog should be ended
		Assert.True(success);
		Assert.True(_testEnv.ProcessEnv.IsDialogEnded(hDlg));
		Assert.Equal(expectedResult, _testEnv.ProcessEnv.GetDialogResult(hDlg));

		// Act - Cleanup
		_testEnv.ProcessEnv.CleanupDialogState(hDlg);

		// Assert - Dialog should no longer be tracked
		Assert.False(_testEnv.ProcessEnv.IsDialogEnded(hDlg));
	}

	[Fact]
	public void EndDialog_WithValidDialog_ShouldReturnTrue()
	{
		// Arrange
		const uint hDlg = 0x00010000;
		_testEnv.ProcessEnv.InitializeDialogState(hDlg);
		const uint expectedResult = 1; // IDOK

		// Act
		var result = _testEnv.CallUser32Api("ENDDIALOG", hDlg, expectedResult);

		// Assert
		Assert.Equal(1u, result); // TRUE
		Assert.True(_testEnv.ProcessEnv.IsDialogEnded(hDlg));
		Assert.Equal(expectedResult, _testEnv.ProcessEnv.GetDialogResult(hDlg));
	}

	[Fact]
	public void EndDialog_WithInvalidDialog_ShouldReturnFalse()
	{
		// Arrange
		const uint hDlg = 0xDEADBEEF; // Non-existent dialog
		const uint result = 1;

		// Act
		var returnValue = _testEnv.CallUser32Api("ENDDIALOG", hDlg, result);

		// Assert
		Assert.Equal(0u, returnValue); // FALSE
	}

	[Fact]
	public void GetDlgItem_ShouldReturnSyntheticHandle()
	{
		// Arrange
		const uint hDlg = 0x00010000;
		const int controlId = 101;

		// Act
		var result = _testEnv.CallUser32Api("GETDLGITEM", hDlg, (uint)controlId);

		// Assert
		Assert.NotEqual(0u, result); // Should return a non-zero handle
		Assert.Equal(hDlg + (uint)controlId, result); // Should be dialog handle + control ID
	}

	[Fact]
	public void SetDlgItemTextA_ShouldStoreText()
	{
		// Arrange
		const uint hDlg = 0x00010000;
		const int controlId = 101;
		_testEnv.ProcessEnv.InitializeDialogState(hDlg);
		
		var textPtr = _testEnv.WriteString("Test Control Text");

		// Act
		var result = _testEnv.CallUser32Api("SETDLGITEMTEXTA", hDlg, (uint)controlId, textPtr);

		// Assert
		Assert.Equal(1u, result); // TRUE
		
		// Verify text was stored
		var storedText = _testEnv.ProcessEnv.GetDialogControlText(hDlg, controlId);
		Assert.Equal("Test Control Text", storedText);
	}

	[Fact]
	public void GetDlgItemTextA_WithNoText_ShouldReturnEmptyString()
	{
		// Arrange
		const uint hDlg = 0x00010000;
		const int controlId = 101;
		_testEnv.ProcessEnv.InitializeDialogState(hDlg);
		
		var buffer = _testEnv.AllocateMemory(256);

		// Act
		var length = _testEnv.CallUser32Api("GETDLGITEMTEXTA", hDlg, (uint)controlId, buffer, 256u);

		// Assert
		Assert.Equal(0u, length); // No text
		Assert.Equal(0, _testEnv.Memory.Read8(buffer)); // Null terminator
	}

	[Fact]
	public void GetDlgItemTextA_WithText_ShouldReturnText()
	{
		// Arrange
		const uint hDlg = 0x00010000;
		const int controlId = 101;
		_testEnv.ProcessEnv.InitializeDialogState(hDlg);
		
		// Set text first
		var textPtr = _testEnv.WriteString("Hello World");
		_testEnv.CallUser32Api("SETDLGITEMTEXTA", hDlg, (uint)controlId, textPtr);
		
		// Allocate buffer for retrieval
		var buffer = _testEnv.AllocateMemory(256);

		// Act
		var length = _testEnv.CallUser32Api("GETDLGITEMTEXTA", hDlg, (uint)controlId, buffer, 256u);

		// Assert
		Assert.Equal(11u, length); // Length of "Hello World"
		
		// Read the text from the buffer
		var retrievedText = _testEnv.ReadString(buffer);
		Assert.Equal("Hello World", retrievedText);
	}

	[Fact]
	public void GetDlgItemTextA_WithSmallBuffer_ShouldTruncateText()
	{
		// Arrange
		const uint hDlg = 0x00010000;
		const int controlId = 101;
		_testEnv.ProcessEnv.InitializeDialogState(hDlg);
		
		// Set text first
		var textPtr = _testEnv.WriteString("This is a very long text");
		_testEnv.CallUser32Api("SETDLGITEMTEXTA", hDlg, (uint)controlId, textPtr);
		
		// Allocate small buffer
		var buffer = _testEnv.AllocateMemory(10);

		// Act
		var length = _testEnv.CallUser32Api("GETDLGITEMTEXTA", hDlg, (uint)controlId, buffer, 10u);

		// Assert
		Assert.Equal(9u, length); // 10 - 1 (for null terminator)
		
		// Read the text from the buffer
		var retrievedText = _testEnv.ReadString(buffer);
		Assert.Equal("This is a", retrievedText); // Truncated
	}

	[Fact]
	public void SetDlgItemTextA_MultipleControls_ShouldStoreIndependently()
	{
		// Arrange
		const uint hDlg = 0x00010000;
		_testEnv.ProcessEnv.InitializeDialogState(hDlg);
		
		var text1Ptr = _testEnv.WriteString("Control 1 Text");
		var text2Ptr = _testEnv.WriteString("Control 2 Text");
		var text3Ptr = _testEnv.WriteString("Control 3 Text");

		// Act - Set text for multiple controls
		_testEnv.CallUser32Api("SETDLGITEMTEXTA", hDlg, 101u, text1Ptr);
		_testEnv.CallUser32Api("SETDLGITEMTEXTA", hDlg, 102u, text2Ptr);
		_testEnv.CallUser32Api("SETDLGITEMTEXTA", hDlg, 103u, text3Ptr);

		// Assert - Each control should have its own text
		Assert.Equal("Control 1 Text", _testEnv.ProcessEnv.GetDialogControlText(hDlg, 101));
		Assert.Equal("Control 2 Text", _testEnv.ProcessEnv.GetDialogControlText(hDlg, 102));
		Assert.Equal("Control 3 Text", _testEnv.ProcessEnv.GetDialogControlText(hDlg, 103));
	}

	[Fact]
	public void DialogControlHandle_StoreAndRetrieve_ShouldWorkCorrectly()
	{
		// Arrange
		const uint hDlg = 0x00010000;
		const ushort controlId = 101;
		const uint controlHandle = 0x00020000;
		_testEnv.ProcessEnv.InitializeDialogState(hDlg);
		
		var controlInfo = new Win32.DialogItem
		{
			Id = controlId,
			WindowClass = "BUTTON",
			Title = "OK",
			Style = 0,
			ExtendedStyle = 0
		};

		// Act
		_testEnv.ProcessEnv.StoreControlInfo(hDlg, controlId, controlHandle, controlInfo);
		var retrievedHandle = _testEnv.ProcessEnv.GetDialogControlHandle(hDlg, controlId);

		// Assert
		Assert.Equal(controlHandle, retrievedHandle);
	}

	[Fact]
	public void GetDialogControlHandle_WithNonexistentControl_ShouldReturnZero()
	{
		// Arrange
		const uint hDlg = 0x00010000;
		const int controlId = 999;
		_testEnv.ProcessEnv.InitializeDialogState(hDlg);

		// Act
		var handle = _testEnv.ProcessEnv.GetDialogControlHandle(hDlg, controlId);

		// Assert
		Assert.Equal(0u, handle);
	}

	public void Dispose()
	{
		_testEnv.Dispose();
	}
}
