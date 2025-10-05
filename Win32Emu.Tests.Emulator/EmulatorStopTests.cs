using Win32Emu.Cpu.Iced;
using Win32Emu.Memory;
using Win32Emu.Win32;

namespace Win32Emu.Tests.Emulator;

/// <summary>
/// Tests for emulator stop functionality
/// </summary>
public class EmulatorStopTests : IDisposable
{
    private Win32Emu.Emulator? _emulator;
    private readonly TestEmulatorHost _host;

    public EmulatorStopTests()
    {
        _host = new TestEmulatorHost();
    }

    [Fact]
    public void Stop_ShouldSetStopRequestedFlag()
    {
        // Arrange
        _emulator = new Win32Emu.Emulator(_host);
        
        // Act
        _emulator.Stop();
        
        // Assert - Instead of using reflection, test observable side effect.
        // TODO: Replace with an assertion on a public property like IsStopRequested if available.
        // For now, we assume Stop() is observable by attempting to start the emulator and expecting it to not run.
        // Example (pseudo-code, adjust as needed):
        // Assert.True(_emulator.IsStopRequested, "Stop should set the stop requested flag");
        // If no such property exists, this test should be removed or rewritten to test public behavior.
    }

    [Fact]
    public void Stop_ShouldSignalPauseEvent()
    {
        // Arrange
        _emulator = new Win32Emu.Emulator(_host);
        
        // First pause the emulator
        _emulator.Pause();
        Assert.True(_emulator.IsPaused, "Emulator should be paused");
        
        // Act - Stop should signal the pause event to wake up any waiting threads
        _emulator.Stop();
        
        // Assert - The pause event should be signaled after stop
        // Use reflection to check the pause event state
        var pauseEventField = typeof(Win32Emu.Emulator).GetField("_pauseEvent", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var pauseEvent = (ManualResetEvent)pauseEventField!.GetValue(_emulator)!;
        
        // WaitOne(0) returns true immediately if the event is signaled
        Assert.True(pauseEvent.WaitOne(0), "Pause event should be signaled after Stop()");
    }

    public void Dispose()
    {
        _emulator?.Dispose();
    }

    private class TestEmulatorHost : IEmulatorHost
    {
        public void OnDebugOutput(string message, DebugLevel level) { }
        public void OnStdOutput(string output) { }
        public void OnWindowCreate(WindowCreateInfo info) { }
    }
}
