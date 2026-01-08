using System;
using Microsoft.Extensions.Logging;
using Win32Emu;
using Win32Emu.Win32;
using Win32Emu.Memory;
using IgNTeas.Generated;

namespace IgNTeas.TestHarness
{
	/// <summary>
	/// Test harness for running transpiled ign_teas functions
	/// </summary>
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("=== ign_teas Transpiled Code Test Harness ===\n");
			
			// Create logger
			using var loggerFactory = LoggerFactory.Create(builder =>
			{
				builder.AddConsole();
				builder.SetMinimumLevel(LogLevel.Debug);
			});
			var logger = loggerFactory.CreateLogger<Program>();
			
			// Create mock environment
			var mockEnv = new MockProcessEnvironment(logger);
			
			// Test the initialization function
			Console.WriteLine("Testing Function_004032A0 (Initialization Function)");
			Console.WriteLine("====================================================\n");
			
			try
			{
				var initFunction = new Function_004032A0(mockEnv);
				
				Console.WriteLine("Calling Execute()...\n");
				var result = initFunction.Execute();
				
				Console.WriteLine($"\n✅ Function completed successfully!");
				Console.WriteLine($"Return value: {result}");
				Console.WriteLine($"\nGlobal variable states:");
				Console.WriteLine($"  dword_41C7A8 = {mockEnv.GetGlobal("dword_41C7A8")}");
				Console.WriteLine($"  dword_41C828 = {mockEnv.GetGlobal("dword_41C828")}");
				Console.WriteLine($"  dword_41C7B0 = {mockEnv.GetGlobal("dword_41C7B0")}");
				Console.WriteLine($"  dword_41C82C = {mockEnv.GetGlobal("dword_41C82C")}");
				
				Console.WriteLine($"\nFunction calls made:");
				foreach (var call in mockEnv.GetFunctionCalls())
				{
					Console.WriteLine($"  CallFunction(0x{call:X8})");
				}
				
				Console.WriteLine("\n=== Test Complete ===");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"\n❌ Error running function: {ex.Message}");
				Console.WriteLine($"Stack trace:\n{ex.StackTrace}");
			}
		}
	}
	
	/// <summary>
	/// Mock ProcessEnvironment for testing transpiled code
	/// </summary>
	class MockProcessEnvironment : ProcessEnvironment
	{
		private readonly Dictionary<string, uint> _globalVariables = new();
		private readonly List<uint> _functionCalls = new();
		private readonly ILogger _logger;
		
		public MockProcessEnvironment(ILogger logger) 
			: base(new VirtualMemory(16 * 1024 * 1024, logger), 0x01000000, new MockEmulatorHost(), logger)
		{
			_logger = logger;
			
			// Initialize global variables to 0
			_globalVariables["dword_41C7A8"] = 0;
			_globalVariables["dword_41C828"] = 0;
			_globalVariables["dword_41C7B0"] = 0;
			_globalVariables["dword_41C82C"] = 0;
			
			Console.WriteLine("Mock environment initialized");
			Console.WriteLine($"Initial global states: All variables = 0\n");
		}
		
		public uint GetGlobal(string name)
		{
			return _globalVariables.TryGetValue(name, out var value) ? value : 0;
		}
		
		public void SetGlobal(string name, uint value)
		{
			var oldValue = GetGlobal(name);
			_globalVariables[name] = value;
			Console.WriteLine($"  [GLOBAL] {name}: {oldValue} → {value}");
		}
		
		public void RecordFunctionCall(uint address)
		{
			_functionCalls.Add(address);
			Console.WriteLine($"  [CALL] Function at 0x{address:X8}");
		}
		
		public List<uint> GetFunctionCalls() => new List<uint>(_functionCalls);
	}
	
	/// <summary>
	/// Mock host for testing - implements IEmulatorHost with stub implementations
	/// </summary>
	class MockEmulatorHost : IEmulatorHost
	{
		public void OnDebugOutput(string message, DebugLevel level) { }
		public void OnStdOutput(string output) => Console.Write(output);
		public void OnWindowCreate(WindowCreateInfo info) { }
		public Task<int> OnDialogCreate(DialogCreateInfo info) => Task.FromResult(0);
		public void OnDialogEnd(uint dialogHandle, int result) { }
		public int OnMessageBox(MessageBoxInfo info) => 0;
		public void OnDialogControlTextChanged(uint dialogHandle, int controlId, string text) { }
		public void OnDialogControlBitmapChanged(uint dialogHandle, int controlId, byte[] bitmapData) { }
		public void OnDialogControlEnabledChanged(uint dialogHandle, int controlId, bool enabled) { }
		public void OnDisplayUpdate(DisplayUpdateInfo info) { }
		public Task<string?> OnBrowseForFolder(string? title, string? rootPath) => Task.FromResult<string?>(null);
		public Task<string?> OnOpenFileDialog(string? title, string? filter, string? initialDirectory) => Task.FromResult<string?>(null);
		public Task<string?> OnSaveFileDialog(string? title, string? filter, string? initialDirectory) => Task.FromResult<string?>(null);
		public void OnControlVisibilityChanged(uint dialogHandle, int controlId, bool visible) { }
		public void OnWindowTitleChanged(uint hwnd, string title) { }
	}
}
