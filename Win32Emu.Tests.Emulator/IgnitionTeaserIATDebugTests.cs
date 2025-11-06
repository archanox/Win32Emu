using Microsoft.Extensions.Logging;
using Win32Emu.Loader;
using Win32Emu.Memory;
using Xunit;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Detailed integration test for IGN_TEAS.EXE that steps through execution
/// and validates IAT (Import Address Table) entries at each stage.
/// 
/// Purpose: Debug the IAT corruption issue where LoadIconA IAT entry at 0x004552F8
/// contains 0x001FEF10 (stack address) instead of 0x0F000060 (synthetic import stub).
/// </summary>
public class IgnitionTeaserIATDebugTests
{
	private readonly ITestOutputHelper _output;

	public IgnitionTeaserIATDebugTests(ITestOutputHelper output)
	{
		_output = output;
	}

	[Fact]
	public void IgnitionTeaser_ValidateIATAtEachStep()
	{
		var exePath = FindExecutable("IGN_TEAS.EXE");
		if (!File.Exists(exePath))
		{
			_output.WriteLine($"Test executable not found: {exePath}");
			_output.WriteLine("Skipping test - executable is required for this debug test");
			return;
		}

		_output.WriteLine("=== IGN_TEAS.EXE IAT Validation Test ===\n");
		_output.WriteLine($"Testing executable: {exePath}\n");

		var testHost = new TestEmulatorHost(_output);
		var logger = new XunitLogger(_output, LogLevel.Debug);

		using var emulator = new Win32Emu.Emulator(testHost, logger);

		_output.WriteLine("=== Phase 1: Loading Executable ===");
		emulator.LoadExecutable(exePath, debugMode: false, reservedMemoryMb: 256);
		_output.WriteLine("Executable loaded successfully\n");

		// Get access to internal state for validation
		var cpu = emulator.GetType().GetField("_cpu", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(emulator);
		var memory = emulator.Environment?.Memory;
		var loadedImage = emulator.LoadedImage;

		Assert.NotNull(memory);
		Assert.NotNull(loadedImage);

		_output.WriteLine("=== Phase 2: Validate IAT After Load ===");
		ValidateIATEntries(memory, loadedImage, "After initial load");

		_output.WriteLine("\n=== Phase 3: Execute Until LoadCursorA Call ===");
		// We'll execute step-by-step and validate IAT before the problematic LoadIconA call
		
		var stepCount = 0;
		var maxSteps = 10000; // Safety limit
		Exception? caughtException = null;

		try
		{
			// Start execution
			var runTask = Task.Run(() =>
			{
				try
				{
					emulator.Run();
				}
				catch (Exception ex)
				{
					caughtException = ex;
				}
			});

			// Give it time to execute
			var timeout = TimeSpan.FromSeconds(2);
			var completedTask = Task.WhenAny(runTask, Task.Delay(timeout)).Result;

			if (completedTask != runTask)
			{
				_output.WriteLine("\nExecution reached timeout - stopping");
				emulator.Stop();
				runTask.Wait(TimeSpan.FromSeconds(2));
			}
		}
		catch (Exception ex)
		{
			caughtException = ex;
		}

		_output.WriteLine("\n=== Phase 4: Final IAT Validation ===");
		ValidateIATEntries(memory, loadedImage, "After execution attempt");

		_output.WriteLine("\n=== Test Summary ===");
		if (caughtException != null)
		{
			_output.WriteLine($"Exception: {caughtException.GetType().Name}");
			_output.WriteLine($"Message: {caughtException.Message}");
			
			// Check if it's the expected IAT corruption error
			if (caughtException.Message.Contains("0x001FEF10") && caughtException.Message.Contains("0x004552F8"))
			{
				_output.WriteLine("\n!!! CONFIRMED: IAT corruption detected !!!");
				_output.WriteLine("LoadIconA IAT entry at 0x004552F8 was corrupted");
				
				// Now validate what's actually in memory at that address
				var iatValue = memory.Read32(0x004552F8);
				_output.WriteLine($"Current value at 0x004552F8: 0x{iatValue:X8}");
				_output.WriteLine($"Expected value: 0x0F000060");
				
				if (iatValue == 0x001FEF10)
				{
					_output.WriteLine("ERROR: Runtime IAT protection did NOT fix the corruption!");
				}
				else if (iatValue == 0x0F000060)
				{
					_output.WriteLine("UNEXPECTED: IAT value is correct, but error was still thrown");
				}
				else
				{
					_output.WriteLine($"UNEXPECTED: IAT value is neither corrupted value nor expected value");
				}
			}
		}
		else
		{
			_output.WriteLine("No exception thrown - execution completed or timed out");
		}

		_output.WriteLine($"\nDebug messages: {testHost.DebugMessages.Count}");
		_output.WriteLine($"Error messages: {testHost.ErrorMessages.Count}");
	}

	private void ValidateIATEntries(VirtualMemory memory, LoadedImage loadedImage, string phase)
	{
		_output.WriteLine($"\n--- IAT Validation: {phase} ---");

		// Key IAT entries we care about
		var criticalIATEntries = new Dictionary<uint, (string name, uint expectedValue)>
		{
			{ 0x004552E0, ("ClientToScreen", 0x0F000000) },
			{ 0x004552F8, ("LoadIconA", 0x0F000060) },
			{ 0x004552FC, ("LoadCursorA", 0x0F000070) }
		};

		var corruptedCount = 0;
		foreach (var (iatAddress, (name, expectedValue)) in criticalIATEntries)
		{
			var actualValue = memory.Read32(iatAddress);
			var status = actualValue == expectedValue ? "✓ OK" : "✗ CORRUPTED";
			
			_output.WriteLine($"  0x{iatAddress:X8} ({name}): 0x{actualValue:X8} (expected 0x{expectedValue:X8}) {status}");
			
			if (actualValue != expectedValue)
			{
				corruptedCount++;
				
				// Check if it's a stack address
				if (actualValue >= 0x00100000 && actualValue <= 0x00200000)
				{
					_output.WriteLine($"    WARNING: Value points to stack region!");
				}
			}
		}

		if (corruptedCount > 0)
		{
			_output.WriteLine($"\n  !!! {corruptedCount} IAT entries are corrupted !!!");
		}
		else
		{
			_output.WriteLine($"\n  All critical IAT entries are valid");
		}
	}

	private string FindExecutable(string exeName)
	{
		var currentDir = Directory.GetCurrentDirectory();
		var repoRoot = currentDir;

		while (repoRoot != null && !File.Exists(Path.Combine(repoRoot, "Win32Emu.slnx")))
		{
			var parent = Directory.GetParent(repoRoot);
			if (parent == null) break;
			repoRoot = parent.FullName;
		}

		var possiblePaths = new[]
		{
			Path.Combine(repoRoot!, "EXEs", "ign_teas", exeName),
			Path.Combine(repoRoot!, "EXEs", exeName),
		};

		foreach (var path in possiblePaths)
		{
			if (File.Exists(path)) return path;
		}

		return possiblePaths[0];
	}

	private class TestEmulatorHost : IEmulatorHost
	{
		private readonly ITestOutputHelper _output;
		public List<string> DebugMessages { get; } = new();
		public List<string> ErrorMessages { get; } = new();

		public TestEmulatorHost(ITestOutputHelper output)
		{
			_output = output;
		}

		public void OnDebugOutput(string message, DebugLevel level)
		{
			var prefix = level switch
			{
				DebugLevel.Error => "[ERROR] ",
				DebugLevel.Warning => "[WARN]  ",
				_ => "[DEBUG] "
			};

			if (level == DebugLevel.Error)
			{
				ErrorMessages.Add(message);
				_output.WriteLine($"{prefix}{message}");
			}
			else
			{
				DebugMessages.Add(message);
			}
		}

		public void OnStdOutput(string output) { }
		public void OnWindowCreate(WindowCreateInfo windowInfo) { }
		public Task<int> OnDialogCreate(DialogCreateInfo info) => Task.FromResult(2);
		public void OnDialogEnd(uint dialogHandle, int result) { }
		public int OnMessageBox(MessageBoxInfo info) => 1;
		public void OnDialogControlTextChanged(uint dialogHandle, int controlId, string text) { }
		public void OnDialogControlBitmapChanged(uint dialogHandle, int controlId, byte[] bitmapData) { }
		public void OnDisplayUpdate(DisplayUpdateInfo info) { }
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

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			if (!IsEnabled(logLevel)) return;

			var message = formatter(state, exception);
			
			// Only log errors and critical information to reduce noise
			if (logLevel >= LogLevel.Warning)
			{
				var prefix = logLevel switch
				{
					LogLevel.Critical => "[CRITICAL]",
					LogLevel.Error => "[ERROR]   ",
					LogLevel.Warning => "[WARNING] ",
					_ => "[INFO]    "
				};

				_output.WriteLine($"{prefix} {message}");
				
				if (exception != null)
				{
					_output.WriteLine($"          Exception: {exception}");
				}
			}
		}
	}
}
