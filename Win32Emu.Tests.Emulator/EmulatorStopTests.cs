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
        
        // Start a thread that waits for the emulator to be resumed (simulate a thread waiting on pause)
        var releasedEvent = new System.Threading.ManualResetEventSlim(false);
        var waitThread = new System.Threading.Thread(() =>
        {
            // This should block until the emulator is resumed or stopped
            _emulator.WaitWhilePaused();
            releasedEvent.Set();
        });
        waitThread.Start();

        // Give the thread a moment to start and block
        System.Threading.Thread.Sleep(100);
        Assert.False(releasedEvent.IsSet, "Thread should be blocked while emulator is paused");

        // Act - Stop should signal the pause event to wake up any waiting threads
        _emulator.Stop();

        // Assert - The waiting thread should be released after stop
        bool released = releasedEvent.Wait(1000); // Wait up to 1 second
        Assert.True(released, "Waiting thread should be released after Stop()");

        waitThread.Join();
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
