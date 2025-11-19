using Win32Emu.Cpu;
using Win32Emu.Memory;
using Win32Emu.Win32;
using Win32Emu.Win32.Modules;
using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Integration tests for ordinal-based imports
/// These tests simulate what happens when a PE file imports a function by ordinal
/// </summary>
public class OrdinalImportIntegrationTests
{
	[Fact]
	public void Dispatcher_ShouldResolveOrdinalAndInvokeFunction()
	{
		// Arrange - Setup a dispatcher with DPlayXModule
		var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
		var logger = loggerFactory.CreateLogger<Win32Dispatcher>();
		var dispatcher = new Win32Dispatcher(logger);
		
		// Create a test environment and module
		var memory = new VirtualMemory();
		var cpu = new MockCpu();
		var processEnv = new ProcessEnvironment(memory, 0x00400000);
		
		// Register DPlayX module
		var dplayxModule = new DPlayXModule(processEnv, 0x10000000, null, loggerFactory.CreateLogger<DPlayXModule>());
		dispatcher.RegisterModule(dplayxModule);
		
		// Act - Try to invoke using ordinal-based export name (as PE loader would provide)
		var success = dispatcher.TryInvoke("DPLAYX.DLL", "ORDINAL_1", cpu, memory, out var returnValue, out var argBytes);
		
		// Assert - Should successfully resolve and invoke DirectPlayCreate
		Assert.True(success, "Dispatcher should successfully invoke function by ordinal");
		Assert.Equal(12, argBytes); // DirectPlayCreate has 3 uint32 parameters = 12 bytes
	}

	[Fact]
	public void Dispatcher_ShouldLogCorrectFunctionName_ForOrdinalImport()
	{
		// Arrange - Setup a dispatcher with logging
		var logMessages = new List<string>();
		var loggerFactory = LoggerFactory.Create(builder =>
		{
			builder.AddProvider(new TestLoggerProvider((category, level, message) =>
			{
				logMessages.Add($"[{level}] {message}");
			}));
			builder.SetMinimumLevel(LogLevel.Debug);
		});
		var logger = loggerFactory.CreateLogger<Win32Dispatcher>();
		var dispatcher = new Win32Dispatcher(logger);
		
		var memory = new VirtualMemory();
		var cpu = new MockCpu();
		var processEnv = new ProcessEnvironment(memory, 0x00400000);
		
		var dplayxModule = new DPlayXModule(processEnv, 0x10000000, null, loggerFactory.CreateLogger<DPlayXModule>());
		dispatcher.RegisterModule(dplayxModule);
		
		// Act - Invoke using ordinal
		dispatcher.TryInvoke("DPLAYX.DLL", "ORDINAL_1", cpu, memory, out _, out _);
		
		// Assert - Should log the resolved function name
		Assert.Contains(logMessages, msg => msg.Contains("Resolved ordinal export ORDINAL_1 to DirectPlayCreate"));
		Assert.Contains(logMessages, msg => msg.Contains("DPLAYX.DLL!ORDINAL_1 returned"));
	}

	[Fact]
	public void Module_ShouldReceiveResolvedFunctionName()
	{
		// Arrange
		var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
		var logger = loggerFactory.CreateLogger<Win32Dispatcher>();
		var dispatcher = new Win32Dispatcher(logger);
		
		var memory = new VirtualMemory();
		var cpu = new MockCpu();
		
		// Use a custom test module to verify what export name it receives
		var testModule = new TestOrdinalModule();
		dispatcher.RegisterModule(testModule);
		
		// Act - Invoke using ordinal for DPLAYX.DLL ordinal 1 which should resolve to DirectPlayCreate
		dispatcher.TryInvoke("DPLAYX.DLL", "ORDINAL_1", cpu, memory, out _, out _);
		
		// Assert - Module should receive the resolved name (PascalCase as defined in the method name)
		Assert.Equal("DirectPlayCreate", testModule.LastInvokedExport);
	}

	// Helper test module to capture invoked export names
	private class TestOrdinalModule : IWin32ModuleUnsafe
	{
		public string Name => "DPLAYX.DLL";
		public string? LastInvokedExport { get; private set; }

		public bool TryInvokeUnsafe(string export, ICpu cpu, VirtualMemory memory, out uint returnValue)
		{
			LastInvokedExport = export;
			returnValue = 0;
			return true;
		}
	}

	// Helper test logger provider
	private class TestLoggerProvider : ILoggerProvider
	{
		private readonly Action<string, LogLevel, string> _logAction;

		public TestLoggerProvider(Action<string, LogLevel, string> logAction)
		{
			_logAction = logAction;
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new TestLogger(categoryName, _logAction);
		}

		public void Dispose() { }
	}

	private class TestLogger : ILogger
	{
		private readonly string _categoryName;
		private readonly Action<string, LogLevel, string> _logAction;

		public TestLogger(string categoryName, Action<string, LogLevel, string> logAction)
		{
			_categoryName = categoryName;
			_logAction = logAction;
		}

		public IDisposable BeginScope<TState>(TState state) => null!;
		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			_logAction(_categoryName, logLevel, formatter(state, exception));
		}
	}
}
