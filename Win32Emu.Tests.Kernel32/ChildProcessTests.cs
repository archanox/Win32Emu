using Xunit;
using Win32Emu.Tests.Infrastructure;
using Win32Emu.Win32.Modules;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for child process execution functions (WinExec, ShellExecuteA)
/// </summary>
[Trait("Category", "DllModuleTests")]
public sealed class ChildProcessTests : IDisposable
{
	private readonly TestEnvironment _testEnv;
	private readonly Shell32Module _shell32;

	public ChildProcessTests()
	{
		_testEnv = new TestEnvironment(initializeDispatcher: true);
		
		// Register Shell32 module for ShellExecuteA tests
		_shell32 = new Shell32Module(_testEnv.ProcessEnv, 0x00500000);
		_testEnv.Dispatcher!.RegisterModule(_shell32);
	}

	[Fact]
	public void WinExec_WithAbsolutePath_ShouldRequestChildProcess()
	{
		// Arrange
		var cmdLine = @"C:\Windows\System32\notepad.exe";
		var cmdLineAddr = _testEnv.WriteString(cmdLine);
		var uCmdShow = 5u; // SW_SHOW

		// Act
		var result = _testEnv.CallKernel32Api("WINEXEC", cmdLineAddr, uCmdShow);

		// Assert
		Assert.Equal(33u, result); // SE_ERR_SUCCESS (value > 31 indicates success)
		
		// Verify child process request was created
		var request = _testEnv.ProcessEnv.PendingChildProcessRequest;
		Assert.NotNull(request);
		Assert.Equal(cmdLine, request.ExecutablePath);
		Assert.Equal(cmdLine, request.CommandLine);
		Assert.Equal((int)uCmdShow, request.ShowCommand);
	}

	[Fact]
	public void WinExec_WithRelativePath_ShouldResolveAndRequestChildProcess()
	{
		// Arrange
		var relativePath = "setup.exe";
		var currentDir = @"C:\Install";
		_testEnv.ProcessEnv.CurrentDirectory = currentDir;
		
		var cmdLineAddr = _testEnv.WriteString(relativePath);
		var uCmdShow = 1u; // SW_SHOWNORMAL

		// Act
		var result = _testEnv.CallKernel32Api("WINEXEC", cmdLineAddr, uCmdShow);

		// Assert
		Assert.Equal(33u, result); // SE_ERR_SUCCESS
		
		// Verify child process request was created with resolved path
		var request = _testEnv.ProcessEnv.PendingChildProcessRequest;
		Assert.NotNull(request);
		Assert.Equal(@"C:\Install\setup.exe", request.ExecutablePath);
		Assert.Equal(relativePath, request.CommandLine);
		Assert.Equal((int)uCmdShow, request.ShowCommand);
	}

	[Fact]
	public void WinExec_WithQuotedPath_ShouldParseCorrectly()
	{
		// Arrange
		var exePath = @"C:\Program Files\MyApp\app.exe";
		var args = "arg1 arg2";
		var cmdLine = $"\"{exePath}\" {args}";
		var cmdLineAddr = _testEnv.WriteString(cmdLine);
		var uCmdShow = 1u;

		// Act
		var result = _testEnv.CallKernel32Api("WINEXEC", cmdLineAddr, uCmdShow);

		// Assert
		Assert.Equal(33u, result); // SE_ERR_SUCCESS
		
		// Verify child process request was created
		var request = _testEnv.ProcessEnv.PendingChildProcessRequest;
		Assert.NotNull(request);
		Assert.Equal(exePath, request.ExecutablePath);
		Assert.Equal(cmdLine, request.CommandLine);
	}

	[Fact]
	public void WinExec_WithNullCommandLine_ShouldReturnError()
	{
		// Arrange - null pointer (address 0)
		var cmdLineAddr = 0u;
		var uCmdShow = 1u;

		// Act
		var result = _testEnv.CallKernel32Api("WINEXEC", cmdLineAddr, uCmdShow);

		// Assert
		Assert.Equal(2u, result); // ERROR_FILE_NOT_FOUND
		
		// Verify no child process request was created
		var request = _testEnv.ProcessEnv.PendingChildProcessRequest;
		Assert.Null(request);
	}

	[Fact]
	public void ShellExecuteA_WithOpenOperation_ShouldRequestChildProcess()
	{
		// Arrange
		var hwnd = 0u;
		var operation = "open";
		var file = @"C:\Test\program.exe";
		var parameters = "/silent";
		var directory = @"C:\Test";
		var nShowCmd = 1;

		var operationAddr = _testEnv.WriteString(operation);
		var fileAddr = _testEnv.WriteString(file);
		var parametersAddr = _testEnv.WriteString(parameters);
		var directoryAddr = _testEnv.WriteString(directory);

		// Act
		var result = _testEnv.CallShell32Api("SHELLEXECUTEA", hwnd, operationAddr, fileAddr, parametersAddr, directoryAddr, (uint)nShowCmd);

		// Assert
		Assert.Equal(33u, result); // Success (> 32)
		
		// Verify child process request was created
		var request = _testEnv.ProcessEnv.PendingChildProcessRequest;
		Assert.NotNull(request);
		Assert.Equal(file, request.ExecutablePath);
		Assert.Equal($"{file} {parameters}", request.CommandLine);
		Assert.Equal(directory, request.WorkingDirectory);
		Assert.Equal(nShowCmd, request.ShowCommand);
	}

	[Fact]
	public void ShellExecuteA_WithEmptyOperation_ShouldDefaultToOpen()
	{
		// Arrange
		var hwnd = 0u;
		var operation = ""; // Empty string defaults to "open"
		var file = @"C:\setup.exe";
		var parameters = "";
		var directory = "";
		var nShowCmd = 1;

		var operationAddr = _testEnv.WriteString(operation);
		var fileAddr = _testEnv.WriteString(file);
		var parametersAddr = _testEnv.WriteString(parameters);
		var directoryAddr = _testEnv.WriteString(directory);

		// Act
		var result = _testEnv.CallShell32Api("SHELLEXECUTEA", hwnd, operationAddr, fileAddr, parametersAddr, directoryAddr, (uint)nShowCmd);

		// Assert
		Assert.Equal(33u, result); // Success
		
		// Verify child process request was created
		var request = _testEnv.ProcessEnv.PendingChildProcessRequest;
		Assert.NotNull(request);
		Assert.Equal(file, request.ExecutablePath);
	}

	[Fact]
	public void ShellExecuteA_WithRelativeFile_ShouldResolve()
	{
		// Arrange
		var hwnd = 0u;
		var operation = "open";
		var file = "install.exe";
		var parameters = "";
		var directory = @"C:\MyApp";
		var nShowCmd = 1;

		var operationAddr = _testEnv.WriteString(operation);
		var fileAddr = _testEnv.WriteString(file);
		var parametersAddr = _testEnv.WriteString(parameters);
		var directoryAddr = _testEnv.WriteString(directory);

		// Act
		var result = _testEnv.CallShell32Api("SHELLEXECUTEA", hwnd, operationAddr, fileAddr, parametersAddr, directoryAddr, (uint)nShowCmd);

		// Assert
		Assert.Equal(33u, result); // Success
		
		// Verify child process request was created with resolved path
		var request = _testEnv.ProcessEnv.PendingChildProcessRequest;
		Assert.NotNull(request);
		Assert.Equal(@"C:\MyApp\install.exe", request.ExecutablePath);
	}

	public void Dispose()
	{
		_testEnv.Dispose();
	}
}
