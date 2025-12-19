using System;
using System.IO;
using Win32Emu.Win32;
using Win32Emu.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Win32Emu.Tests.Kernel32;

public class ConsoleOutputTests
{
    [Fact]
    public void WriteToStdOutput_WithoutHost_ShouldWriteToConsole()
    {
        // Arrange
        var memory = new VirtualMemory(16 * 1024 * 1024);
        var env = new ProcessEnvironment(memory, host: null, logger: NullLogger.Instance);
        
        // Redirect Console.Out to capture output
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        
        try
        {
            // Act
            env.WriteToStdOutput("Test Output");
            
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
        // Arrange
        var memory = new VirtualMemory(16 * 1024 * 1024);
        var env = new ProcessEnvironment(memory, host: null, logger: NullLogger.Instance);
        
        // Redirect Console.Error to capture output
        var originalError = Console.Error;
        using var writer = new StringWriter();
        Console.SetError(writer);
        
        try
        {
            // Act
            env.WriteToStdError("Test Error");
            
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
        // Arrange
        var memory = new VirtualMemory(16 * 1024 * 1024);
        var env = new ProcessEnvironment(memory, host: null, logger: NullLogger.Instance);
        
        // Redirect Console.Out to capture output
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        
        try
        {
            // Act
            var bytes = System.Text.Encoding.ASCII.GetBytes("Byte Array Test");
            env.WriteToStdOutput(bytes);
            
            // Assert
            var output = writer.ToString();
            Assert.Contains("Byte Array Test", output);
        }
        finally
        {
            // Restore original Console.Out
            Console.SetOut(originalOut);
        }
    }
}
