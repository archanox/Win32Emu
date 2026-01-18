using Xunit;
using Win32Emu.Tests.Infrastructure;
using System.Text;
using Xunit.Abstractions;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Integration test for child process execution workflow.
/// Tests the complete scenario of one executable launching another.
/// </summary>
[Trait("Category", "Integration")]
public sealed class ChildProcessIntegrationTests
{
	private readonly ITestOutputHelper _output;

	public ChildProcessIntegrationTests(ITestOutputHelper output)
	{
		_output = output;
	}

	[Fact]
	public void ChildProcessWorkflow_SimulatesAutorunLaunchingSetup()
	{
		// This test simulates the autorun.exe → setup.exe scenario
		// by creating a minimal test executable that calls WinExec
		
		_output.WriteLine("=== Child Process Integration Test ===");
		_output.WriteLine("Simulating autorun.exe launching setup.exe");
		_output.WriteLine("");

		// Create a test environment that can generate and run x86 code
		using var testEnv = new TestEnvironment(initializeDispatcher: true);
		
		// Simulate calling WinExec("C:\\Install\\setup.exe", SW_SHOW)
		var setupPath = @"C:\Install\setup.exe";
		var setupPathAddr = testEnv.WriteString(setupPath);
		var swShow = 1u; // SW_SHOWNORMAL

		_output.WriteLine($"1. Calling WinExec(\"{setupPath}\", {swShow})");
		
		// Act: Call WinExec
		var result = testEnv.CallKernel32Api("WINEXEC", setupPathAddr, swShow);

		// Assert: WinExec returns success
		Assert.Equal(33u, result); // SE_ERR_SUCCESS
		_output.WriteLine($"   Result: {result} (success)");
		_output.WriteLine("");

		// Assert: Child process request was created
		var request = testEnv.ProcessEnv.PendingChildProcessRequest;
		Assert.NotNull(request);
		
		_output.WriteLine("2. Child Process Request Created:");
		_output.WriteLine($"   ExecutablePath: {request.ExecutablePath}");
		_output.WriteLine($"   CommandLine: {request.CommandLine}");
		_output.WriteLine($"   WorkingDirectory: {request.WorkingDirectory}");
		_output.WriteLine($"   ShowCommand: {request.ShowCommand}");
		_output.WriteLine("");

		// Verify request details
		Assert.Equal(setupPath, request.ExecutablePath);
		Assert.Equal(setupPath, request.CommandLine);
		Assert.Equal(@"C:\", request.WorkingDirectory); // Default current directory
		Assert.Equal((int)swShow, request.ShowCommand);

		_output.WriteLine("3. Verification Complete:");
		_output.WriteLine("   ✓ WinExec returned success");
		_output.WriteLine("   ✓ Child process request created");
		_output.WriteLine("   ✓ Request contains correct information");
		_output.WriteLine("");
		_output.WriteLine("=== Test Passed ===");
		_output.WriteLine("");
		_output.WriteLine("In a real scenario:");
		_output.WriteLine("1. The emulator would exit its main loop");
		_output.WriteLine("2. The caller would check GetPendingChildProcessRequest()");
		_output.WriteLine("3. The caller would load and run C:\\Install\\setup.exe");
	}

	[Fact]
	public void ChildProcessWorkflow_WithRelativePath_ResolvesCorrectly()
	{
		// Test the scenario where autorun.exe is in C:\Install
		// and calls WinExec("setup.exe", ...)
		
		_output.WriteLine("=== Relative Path Resolution Test ===");

		using var testEnv = new TestEnvironment(initializeDispatcher: true);
		
		// Set current directory to C:\Install (where autorun.exe would be)
		testEnv.ProcessEnv.CurrentDirectory = @"C:\Install";
		_output.WriteLine($"Current Directory: {testEnv.ProcessEnv.CurrentDirectory}");
		_output.WriteLine("");

		// Call WinExec with relative path
		var relativePath = "setup.exe";
		var relativePathAddr = testEnv.WriteString(relativePath);
		var swShow = 1u;

		_output.WriteLine($"Calling WinExec(\"{relativePath}\", {swShow})");
		
		var result = testEnv.CallKernel32Api("WINEXEC", relativePathAddr, swShow);

		Assert.Equal(33u, result);
		_output.WriteLine($"Result: {result} (success)");
		_output.WriteLine("");

		var request = testEnv.ProcessEnv.PendingChildProcessRequest;
		Assert.NotNull(request);

		_output.WriteLine("Resolved Path:");
		_output.WriteLine($"  {request.ExecutablePath}");
		_output.WriteLine("");

		// Verify the relative path was resolved correctly
		Assert.Equal(@"C:\Install\setup.exe", request.ExecutablePath);
		
		_output.WriteLine("✓ Relative path resolved correctly");
		_output.WriteLine("=== Test Passed ===");
	}

	[Fact]
	public void ChildProcessWorkflow_WithShellExecuteA_WorksAsExpected()
	{
		// Test ShellExecuteA API for launching executables
		
		_output.WriteLine("=== ShellExecuteA Integration Test ===");

		using var testEnv = new TestEnvironment(initializeDispatcher: true);
		
		// Register Shell32 module
		var shell32 = new Win32.Modules.Shell32Module(testEnv.ProcessEnv, 0x00500000);
		testEnv.Dispatcher!.RegisterModule(shell32);

		// Prepare ShellExecuteA parameters
		var hwnd = 0u;
		var operation = "open";
		var file = @"C:\Games\game.exe";
		var parameters = "/fullscreen";
		var directory = @"C:\Games";
		var nShowCmd = 5u; // SW_SHOW

		var operationAddr = testEnv.WriteString(operation);
		var fileAddr = testEnv.WriteString(file);
		var parametersAddr = testEnv.WriteString(parameters);
		var directoryAddr = testEnv.WriteString(directory);

		_output.WriteLine("Calling ShellExecuteA:");
		_output.WriteLine($"  Operation: {operation}");
		_output.WriteLine($"  File: {file}");
		_output.WriteLine($"  Parameters: {parameters}");
		_output.WriteLine($"  Directory: {directory}");
		_output.WriteLine($"  ShowCmd: {nShowCmd}");
		_output.WriteLine("");

		// Act
		var result = testEnv.CallShell32Api("SHELLEXECUTEA", hwnd, operationAddr, fileAddr, parametersAddr, directoryAddr, nShowCmd);

		// Assert
		Assert.Equal(33u, result); // Success
		_output.WriteLine($"Result: {result} (success)");
		_output.WriteLine("");

		var request = testEnv.ProcessEnv.PendingChildProcessRequest;
		Assert.NotNull(request);

		_output.WriteLine("Child Process Request:");
		_output.WriteLine($"  ExecutablePath: {request.ExecutablePath}");
		_output.WriteLine($"  CommandLine: {request.CommandLine}");
		_output.WriteLine($"  WorkingDirectory: {request.WorkingDirectory}");
		_output.WriteLine($"  ShowCommand: {request.ShowCommand}");
		_output.WriteLine("");

		Assert.Equal(file, request.ExecutablePath);
		Assert.Equal($"{file} {parameters}", request.CommandLine);
		Assert.Equal(directory, request.WorkingDirectory);
		Assert.Equal((int)nShowCmd, request.ShowCommand);

		_output.WriteLine("✓ ShellExecuteA created correct request");
		_output.WriteLine("=== Test Passed ===");
	}
}
