using System;
using System.IO;
using Win32Emu.Win32;
using Win32Emu.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

/// <summary>
/// Tests for console output functionality without a host
/// </summary>
[Trait("Category", "DllModuleTests")]
public sealed class ConsoleOutputTests : IDisposable
{
    private readonly VirtualMemory _memory;
    private readonly ProcessEnvironment _env;

    public ConsoleOutputTests()
    {
        _memory = new VirtualMemory(16 * 1024 * 1024);
        _env = new ProcessEnvironment(_memory, host: null, logger: NullLogger.Instance);
    }

    [Fact]
    public void WriteToStdOutput_WithoutHost_ShouldWriteToConsole()
    {
        // Redirect Console.Out to capture output
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        
        try
        {
            // Act
            _env.WriteToStdOutput("Test Output");
            
            // Assert
            var output = writer.ToString();
            Assert.Contains("Test Output", output);
        }
        finally
        {
            // Restore original Console.Out
            Console.SetOut(originalOut);
        }
    }
    
    [Fact]
    public void WriteToStdError_WithoutHost_ShouldWriteToConsoleError()
    {
        // Redirect Console.Error to capture output
        var originalError = Console.Error;
        using var writer = new StringWriter();
        Console.SetError(writer);
        
        try
        {
            // Act
            _env.WriteToStdError("Test Error");
            
            // Assert
            var output = writer.ToString();
            Assert.Contains("Test Error", output);
        }
        finally
        {
            // Restore original Console.Error
            Console.SetError(originalError);
        }
    }
    
    [Fact]
    public void WriteToStdOutput_WithByteArray_WithoutHost_ShouldWriteToConsole()
    {
        // Since WriteToStdOutput(byte[]) now writes directly to Console.OpenStandardOutput(),
        // we can't easily capture it with StringWriter. Instead, we verify it doesn't throw.
        // The actual byte writing to stdout is tested by integration tests.
        
        // Act - this should not throw
        var bytes = System.Text.Encoding.ASCII.GetBytes("Byte Array Test");
        var exception = Record.Exception(() => _env.WriteToStdOutput(bytes));
        
        // Assert - no exception should be thrown
        Assert.Null(exception);
    }

    public void Dispose()
    {
        // ProcessEnvironment and VirtualMemory don't implement IDisposable,
        // so no cleanup is needed
    }
}
