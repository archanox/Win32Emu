using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Memory;
using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Win32Emu.Win32;
using Win32Emu.Win32.Modules;

namespace Win32Emu.Tests.Kernel32;

public class DispatcherIntegrationTests
{
    [Fact]
    public void Dispatcher_LoadLibraryIntegration_ShouldRegisterDynamicallyLoadedDlls()
    {
        // Arrange
        var vm = new VirtualMemory();
        var env = new ProcessEnvironment(vm);
        
        // Initialize environment with a fake exe path so LoadLibraryA works
        env.InitializeStrings("/tmp/test.exe", Array.Empty<string>());
        
        var testCpu = new MockCpu();
        
        var dispatcher = new Win32Dispatcher(NullLogger.Instance);
        var kernel32Module = new Kernel32Module(env, 0x00400000);
        kernel32Module.SetDispatcher(dispatcher);
        dispatcher.RegisterModule(kernel32Module);
        
        // Act - First register a DLL as dynamically loaded
        dispatcher.RegisterDynamicallyLoadedDll("USER32.DLL");
        
        // Now try to call a function from the loaded DLL
        var result = dispatcher.TryInvoke("USER32.DLL", "MessageBoxA", testCpu, vm, out var returnValue, out var argBytes);
        
        // Assert
        Assert.True(result); // MessageBoxA call should be handled
        Assert.Equal(0u, returnValue); // Default return for unknown function
        Assert.Equal(0, argBytes); // Default arg bytes
    }
    
    [Fact] 
    public void Dispatcher_PrintSummary_ShouldShowUnknownFunctions()
    {
        // Arrange
        var vm = new VirtualMemory();
        var env = new ProcessEnvironment(vm);
        var testCpu = new MockCpu();
        
        var dispatcher = new Win32Dispatcher(NullLogger.Instance);
        var kernel32Module = new Kernel32Module(env, 0x00400000);
        kernel32Module.SetDispatcher(dispatcher);
        dispatcher.RegisterModule(kernel32Module);
        
        // Act - Call various unknown functions
        dispatcher.TryInvoke("USER32.DLL", "MessageBoxA", testCpu, vm, out _, out _);
        dispatcher.TryInvoke("USER32.DLL", "CreateWindowExA", testCpu, vm, out _, out _);
        dispatcher.TryInvoke("GDI32.DLL", "CreateDC", testCpu, vm, out _, out _);
        dispatcher.TryInvoke("KERNEL32.DLL", "UnknownFunction", testCpu, vm, out _, out _);
        
        // Capture console output
        var originalOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        
        dispatcher.PrintUnknownFunctionsSummary();
        
        Console.SetOut(originalOut);
        var output = stringWriter.ToString();
        
        // Assert
        Assert.Contains("USER32.DLL", output);
        Assert.Contains("GDI32.DLL", output); 
        Assert.Contains("KERNEL32.DLL", output);
        Assert.Contains("MessageBoxA", output);
        Assert.Contains("CreateWindowExA", output);
        Assert.Contains("CreateDC", output);
        Assert.Contains("UnknownFunction", output);
    }
}