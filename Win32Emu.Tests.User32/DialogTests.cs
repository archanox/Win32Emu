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

	public void Dispose()
	{
		_testEnv.Dispose();
	}
}
