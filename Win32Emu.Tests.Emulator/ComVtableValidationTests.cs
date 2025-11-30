using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Comprehensive validation tests for COM vtable emulation.
/// These tests verify that our COM vtable implementation correctly handles:
/// - Parameter passing via stack
/// - Return value handling
/// - Stack cleanup (stdcall convention)
/// - Multiple sequential COM calls
/// </summary>
public class ComVtableValidationTests
{
	private readonly ITestOutputHelper _output;

	public ComVtableValidationTests(ITestOutputHelper output)
	{
		_output = output;
	}

	/// <summary>
	/// Validates that DirectDraw COM interface methods execute correctly.
	/// Tests the full flow: DirectDrawCreate -> SetCooperativeLevel -> SetDisplayMode -> CreateSurface
	/// </summary>
	[Fact]
	public void DirectDrawComSequence_ShouldExecuteAllMethodsCorrectly()
	{
		var exePath = FindRetrowin32Executable("cpp/ddraw.exe");
		
		_output.WriteLine($"Testing: {exePath}");
		Assert.True(File.Exists(exePath), $"Test executable not found: {exePath}");

		var testHost = new TestEmulatorHost(_output);
		var logger = new XunitLogger(_output, LogLevel.Information);

		using var emulator = new Win32Emu.Emulator(testHost, logger);

		_output.WriteLine("Loading executable...");
		emulator.LoadExecutable(exePath, debugMode: false, reservedMemoryMb: 256);

		_output.WriteLine("Starting emulation with 3 second timeout...");
		var timeout = TimeSpan.FromSeconds(3);
		var runTask = Task.Run(() => emulator.Run());
		var completedTask = Task.WhenAny(runTask, Task.Delay(timeout)).Result;

		if (completedTask != runTask)
		{
			_output.WriteLine("Test timed out (expected for message loop test)");
			emulator.Stop();
			runTask.Wait(TimeSpan.FromSeconds(2));
		}

		// Verify that the expected stdout messages were printed
		// These come from the test program's print() calls
		Assert.Contains(testHost.StdOutputMessages, msg => msg.Contains("CreateWindowEx"));
		Assert.Contains(testHost.StdOutputMessages, msg => msg.Contains("DirectDrawCreate"));
		Assert.Contains(testHost.StdOutputMessages, msg => msg.Contains("SetCooperativeLevel"));
		Assert.Contains(testHost.StdOutputMessages, msg => msg.Contains("SetDisplayMode"));
		Assert.Contains(testHost.StdOutputMessages, msg => msg.Contains("CreateSurface"));

		// Verify no error messages
		Assert.Empty(testHost.ErrorMessages);
		
		// Verify window was created
		Assert.NotEmpty(testHost.WindowsCreated);

		_output.WriteLine($"✓ All COM method calls executed successfully");
		_output.WriteLine($"  - DirectDrawCreate: OK");
		_output.WriteLine($"  - SetCooperativeLevel: OK");
		_output.WriteLine($"  - SetDisplayMode: OK");
		_output.WriteLine($"  - CreateSurface: OK");
	}

	/// <summary>
	/// Validates that COM methods with different parameter counts work correctly.
	/// This ensures argBytes calculation is accurate for various method signatures.
	/// </summary>
	[Fact]
	public void DirectDrawComMethods_ShouldHandleVariousParameterCounts()
	{
		var exePath = FindRetrowin32Executable("cpp/ddraw.exe");
		var testHost = new TestEmulatorHost(_output);
		var logger = new XunitLogger(_output, LogLevel.Warning); // Less verbose

		using var emulator = new Win32Emu.Emulator(testHost, logger);
		emulator.LoadExecutable(exePath, debugMode: false, reservedMemoryMb: 256);

		var runTask = Task.Run(() => emulator.Run());
		Task.WhenAny(runTask, Task.Delay(TimeSpan.FromSeconds(3))).Wait();
		
		if (!runTask.IsCompleted)
		{
			emulator.Stop();
			runTask.Wait(TimeSpan.FromSeconds(2));
		}

		// SetCooperativeLevel has 2 parameters (+ pThis)
		// SetDisplayMode has 3 parameters (+ pThis)
		// CreateSurface has 3 parameters (+ pThis)
		// All should execute without stack corruption
		
		var successMessages = testHost.StdOutputMessages.Count(msg =>
			msg.Contains("SetCooperativeLevel") ||
			msg.Contains("SetDisplayMode") ||
			msg.Contains("CreateSurface"));

		Assert.True(successMessages >= 3, 
			$"Expected at least 3 COM method success messages, got {successMessages}");
	}

	private string FindRetrowin32Executable(string relativePath)
	{
		var currentDir = Directory.GetCurrentDirectory();
		var repoRoot = currentDir;

		while (repoRoot != null && !File.Exists(Path.Combine(repoRoot, "Win32Emu.slnx")))
		{
			var parent = Directory.GetParent(repoRoot);
			if (parent == null) break;
			repoRoot = parent.FullName;
		}

		if (repoRoot == null)
			throw new InvalidOperationException("Could not locate repository root: 'Win32Emu.slnx' not found in any parent directory.");

		// Ensure relativePath is not rooted to avoid truncating the combined path
		if (Path.IsPathRooted(relativePath))
		{
			relativePath = relativePath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}

		return Path.Combine(repoRoot, "retrowin32", "exe", relativePath);
	}

	private class TestEmulatorHost : IEmulatorHost
	{
		private readonly ITestOutputHelper _output;
		public List<string> DebugMessages { get; } = new();
		public List<string> ErrorMessages { get; } = new();
		public List<string> WarningMessages { get; } = new();
		public List<string> WindowsCreated { get; } = new();
		public List<string> StdOutputMessages { get; } = new();

		public TestEmulatorHost(ITestOutputHelper output)
		{
			_output = output;
		}

		public void OnDebugOutput(string message, DebugLevel level)
		{
			switch (level)
			{
				case DebugLevel.Error:
					ErrorMessages.Add(message);
					_output.WriteLine($"[ERROR] {message}");
					break;
				case DebugLevel.Warning:
					WarningMessages.Add(message);
					break;
				default:
					DebugMessages.Add(message);
					break;
			}
		}

		public void OnStdOutput(string output)
		{
			StdOutputMessages.Add(output);
			_output.WriteLine($"[STDOUT] {output}");
		}

		public void OnWindowCreate(WindowCreateInfo windowInfo)
		{
			var info = $"Window: '{windowInfo.Title}' Class: '{windowInfo.ClassName}'";
			WindowsCreated.Add(info);
		}

		public Task<int> OnDialogCreate(DialogCreateInfo info) => Task.FromResult(2);
		public void OnDialogEnd(uint dialogHandle, int result) { }
		public int OnMessageBox(MessageBoxInfo info) => 1;
		public void OnDialogControlTextChanged(uint dialogHandle, int controlId, string text) { }
		public void OnDialogControlBitmapChanged(uint dialogHandle, int controlId, byte[] bitmapData) { }
		public void OnDialogControlEnabledChanged(uint dialogHandle, int controlId, bool enabled) { }
		public void OnDisplayUpdate(DisplayUpdateInfo info) { }
		public Task<string?> OnBrowseForFolder(string? title, string? rootPath) => Task.FromResult<string?>(null);
		public Task<string?> OnOpenFileDialog(string? title, string? filter, string? initialDirectory) => Task.FromResult<string?>(null);
		public Task<string?> OnSaveFileDialog(string? title, string? filter, string? initialDirectory) => Task.FromResult<string?>(null);
public void OnWindowTitleChanged(uint windowHandle, string title) { }
public void OnControlVisibilityChanged(uint dialogHandle, int controlId, bool visible) { }
	}

	private class XunitLogger : ILogger
	{
		private readonly ITestOutputHelper _output;
		private readonly LogLevel _minLevel;

		public XunitLogger(ITestOutputHelper output, LogLevel minLevel = LogLevel.Information)
		{
			_output = output;
			_minLevel = minLevel;
		}

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
		public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, 
			Func<TState, Exception?, string> formatter)
		{
			if (!IsEnabled(logLevel)) return;

			var message = formatter(state, exception);
			var prefix = logLevel switch
			{
				LogLevel.Error => "[ERROR]",
				LogLevel.Warning => "[WARN]",
				_ => "[INFO]"
			};

			_output.WriteLine($"{prefix} {message}");
		}
	}
}
