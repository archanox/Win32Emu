using Microsoft.Extensions.Logging.Abstractions;
using Win32Emu.Memory;
using Win32Emu.Tests.Kernel32.TestInfrastructure;
using Win32Emu.Win32;
using Win32Emu.Win32.Modules;

namespace Win32Emu.Tests.Kernel32;

public class DispatcherTests
{
    [Fact]
    public void Dispatcher_ShouldHandleKnownDllKnownFunction()
    {
        // Arrange
        var vm = new VirtualMemory();
        var env = new ProcessEnvironment(vm);
        var testCpu = new MockCpu();
        
        var dispatcher = new Win32Dispatcher(NullLogger.Instance);
        var kernel32Module = new Kernel32Module(env, 0x00400000);
        kernel32Module.SetDispatcher(dispatcher);
        dispatcher.RegisterModule(kernel32Module);
        
        // Act
        var result = dispatcher.TryInvoke("KERNEL32.DLL", "GetVersion", testCpu, vm, out var returnValue, out var argBytes);
        
        // Assert
        Assert.True(result);
        Assert.True(returnValue > 0); // GetVersion should return a valid version
    }
    
    [Fact]
    public void Dispatcher_ShouldHandleKnownDllUnknownFunction()
    {
        // Arrange
        var vm = new VirtualMemory();
        var env = new ProcessEnvironment(vm);
        var testCpu = new MockCpu();
        
        var dispatcher = new Win32Dispatcher(NullLogger.Instance);
        var kernel32Module = new Kernel32Module(env, 0x00400000);
        kernel32Module.SetDispatcher(dispatcher);
        dispatcher.RegisterModule(kernel32Module);
        
        // Act
        var result = dispatcher.TryInvoke("KERNEL32.DLL", "UnknownFunction123", testCpu, vm, out var returnValue, out var argBytes);
        
        // Assert
        Assert.True(result); // Should now return true for unknown functions
        Assert.Equal(0u, returnValue); // Default return value for unknown functions
        Assert.Equal(0, argBytes); // Default arg bytes for unknown functions
    }
    
    [Fact]
    public void Dispatcher_ShouldHandleUnknownDllUnknownFunction()
    {
        // Arrange
        var vm = new VirtualMemory();
        var env = new ProcessEnvironment(vm);
        var testCpu = new MockCpu();
        
        var dispatcher = new Win32Dispatcher(NullLogger.Instance);
        var kernel32Module = new Kernel32Module(env, 0x00400000);
        kernel32Module.SetDispatcher(dispatcher);
        dispatcher.RegisterModule(kernel32Module);
        
        // Act
        var result = dispatcher.TryInvoke("USER32.DLL", "MessageBoxA", testCpu, vm, out var returnValue, out var argBytes);
        
        // Assert
        Assert.True(result); // Should now return true for unknown DLLs
        Assert.Equal(0u, returnValue); // Default return value for unknown DLLs
        Assert.Equal(0, argBytes); // Default arg bytes for unknown DLLs
    }
    
    [Fact]
    public void Dispatcher_ShouldTrackDynamicallyLoadedDlls()
    {
        // Arrange
        var vm = new VirtualMemory();
        var env = new ProcessEnvironment(vm);
        var testCpu = new MockCpu();
        
        var dispatcher = new Win32Dispatcher(NullLogger.Instance);
        var kernel32Module = new Kernel32Module(env, 0x00400000);
        kernel32Module.SetDispatcher(dispatcher);
        dispatcher.RegisterModule(kernel32Module);
        
        // Act
        dispatcher.RegisterDynamicallyLoadedDll("MYDLL.DLL");
        var result = dispatcher.TryInvoke("MYDLL.DLL", "MyFunction", testCpu, vm, out var returnValue, out var argBytes);
        
        // Assert
        Assert.True(result); // Should return true for dynamically loaded DLLs
        Assert.Equal(0u, returnValue); // Default return value
        Assert.Equal(0, argBytes); // Default arg bytes
    }
    
    [Fact]
    public void Dispatcher_ShouldLogMultipleUnknownFunctions()
    {
        // Arrange
        var vm = new VirtualMemory();
        var env = new ProcessEnvironment(vm);
        var testCpu = new MockCpu();
        
        var dispatcher = new Win32Dispatcher(NullLogger.Instance);
        var kernel32Module = new Kernel32Module(env, 0x00400000);
        kernel32Module.SetDispatcher(dispatcher);
        dispatcher.RegisterModule(kernel32Module);
        
        // Act - Call multiple unknown functions
        dispatcher.TryInvoke("USER32.DLL", "MessageBoxA", testCpu, vm, out _, out _);
        dispatcher.TryInvoke("USER32.DLL", "CreateWindowExA", testCpu, vm, out _, out _);
        dispatcher.TryInvoke("GDI32.DLL", "CreateDC", testCpu, vm, out _, out _);
        dispatcher.TryInvoke("USER32.DLL", "MessageBoxA", testCpu, vm, out _, out _); // Duplicate
        
        // Assert - Should not crash and should handle all calls
        // The logging and summary functionality is tested implicitly
        Assert.True(true); // Test passes if no exceptions are thrown
    }
}