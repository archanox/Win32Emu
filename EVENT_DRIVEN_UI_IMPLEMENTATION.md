# Event-Driven UI Implementation

## Overview

This implementation adds event-driven UI message handling to Win32Emu, solving the fundamental issue where GUI applications create UI elements, listen for events, but miss those events due to blocking timeouts or lack of event processing.

## Problem Statement

The emulator had several critical issues with UI event handling:

1. **No event processing loop**: The main emulation loop never called `ProcessEvents()` on rendering/input backends
2. **Events never reached message queue**: UI backend events (mouse clicks, key presses) were never translated to Win32 messages
3. **Blocking message loops**: Applications using `GetMessageA` would wait for messages that never arrived
4. **Timing issues**: Event listeners would timeout before events could be delivered

## Solution Architecture

### 1. C# Event System

**Created UIEventArgs class** (`Win32Emu/Rendering/UIEventArgs.cs`):
```csharp
public class UIEventArgs : EventArgs
{
    public UIEventType EventType { get; set; }
    public uint WindowHandle { get; set; }
    public uint WParam { get; set; }
    public uint LParam { get; set; }
    public int MouseX { get; set; }
    public int MouseY { get; set; }
    public int KeyCode { get; set; }
    public bool IsPressed { get; set; }
}

public enum UIEventType
{
    MouseMove,
    MouseButtonDown,
    MouseButtonUp,
    KeyDown,
    KeyUp,
    WindowResize,
    WindowClose,
    WindowActivate,
    WindowDeactivate
}
```

### 2. Backend Event Integration

**Updated Backend Interfaces**:
- Added `event EventHandler<UIEventArgs>? UIEvent;` to `IRenderingBackend`
- Added `event EventHandler<UIEventArgs>? UIEvent;` to `IInputBackend`

**Implemented Event Translation in SilkSdlRenderingBackend**:
```csharp
public unsafe void ProcessEvents()
{
    lock (_lock)
    {
        Event evt;
        while (_sdl.PollEvent(&evt) != 0)
        {
            // Translate SDL events to UI events
            UIEventArgs? uiEvent = null;
            
            switch ((EventType)evt.Type)
            {
                case EventType.Mousemotion:
                    uiEvent = new UIEventArgs
                    {
                        EventType = UIEventType.MouseMove,
                        MouseX = evt.Motion.X,
                        MouseY = evt.Motion.Y
                    };
                    break;
                    
                case EventType.Mousebuttondown:
                    uiEvent = new UIEventArgs
                    {
                        EventType = UIEventType.MouseButtonDown,
                        MouseX = evt.Button.X,
                        MouseY = evt.Button.Y,
                        WParam = evt.Button.Button
                    };
                    break;
                    
                // ... more event types
            }
            
            if (uiEvent != null)
            {
                OnUIEvent(uiEvent);
            }
        }
    }
}
```

### 3. ProcessEnvironment Event Handler

**Added UI Event Subscription** (`ProcessEnvironment.cs`):
```csharp
public void SubscribeToUIEvents(IRenderingBackend? renderingBackend, IInputBackend? inputBackend)
{
    if (renderingBackend != null)
    {
        renderingBackend.UIEvent += OnUIEvent;
    }
    
    if (inputBackend != null)
    {
        inputBackend.UIEvent += OnUIEvent;
    }
}

private void OnUIEvent(object? sender, UIEventArgs e)
{
    // Get target window
    var targetHwnd = e.WindowHandle != 0 ? e.WindowHandle : _windows.Keys.FirstOrDefault();
    
    // Translate UI event to Win32 message
    uint message, wParam, lParam;
    
    switch (e.EventType)
    {
        case UIEventType.MouseMove:
            message = 0x0200; // WM_MOUSEMOVE
            lParam = (uint)((e.MouseY << 16) | (e.MouseX & 0xFFFF));
            break;
            
        case UIEventType.MouseButtonDown:
            message = e.WParam switch
            {
                1 => 0x0201, // WM_LBUTTONDOWN
                2 => 0x0204, // WM_RBUTTONDOWN
                3 => 0x0207, // WM_MBUTTONDOWN
                _ => 0x0201
            };
            lParam = (uint)((e.MouseY << 16) | (e.MouseX & 0xFFFF));
            break;
            
        case UIEventType.KeyDown:
            message = 0x0100; // WM_KEYDOWN
            wParam = (uint)e.KeyCode;
            lParam = 0x00000001;
            break;
            
        // ... more message types
    }
    
    // Post translated message to Win32 message queue
    PostMessage(targetHwnd, message, wParam, lParam);
}
```

### 4. Background Event Processing Thread

**Added to Emulator** (`Emulator.cs`):
```csharp
private Task? _eventProcessingTask;
private CancellationTokenSource? _eventProcessingCts;

private void StartEventProcessing()
{
    _eventProcessingCts = new CancellationTokenSource();
    
    _eventProcessingTask = Task.Run(async () =>
    {
        while (!_eventProcessingCts.Token.IsCancellationRequested)
        {
            // Process events from input backend
            _env.InputBackend?.ProcessEvents();
            
            // 60 FPS event processing (16ms delay)
            await Task.Delay(16, _eventProcessingCts.Token);
        }
    }, _eventProcessingCts.Token);
}

private void StopEventProcessing()
{
    _eventProcessingCts?.Cancel();
    _eventProcessingTask?.Wait(TimeSpan.FromSeconds(1));
    _eventProcessingCts?.Dispose();
    _eventProcessingTask?.Dispose();
}
```

**Integrated with RunAsync**:
```csharp
public async Task RunAsync()
{
    // Start background UI event processing thread
    StartEventProcessing();
    
    try
    {
        // Run emulation (debug mode, GDB server, or normal)
        if (_gdbServerMode)
            await RunWithGdbServer(_gdbServerPort);
        else if (_debugMode)
            await RunWithEnhancedDebuggingAsync();
        else
            await RunNormalAsync();
    }
    finally
    {
        // Stop event processing thread
        StopEventProcessing();
    }
}
```

## Event Flow

```
┌─────────────────────────────────────────────────┐
│  Native UI Backend (SDL/GLFW/Vulkan Window)     │
│  - Mouse clicks, keyboard input, window events  │
└────────────────┬────────────────────────────────┘
                 │
                 │ ProcessEvents() (polled at 60 FPS)
                 │
                 v
┌─────────────────────────────────────────────────┐
│  Rendering/Input Backend                        │
│  - ProcessEvents() polls native events          │
│  - Translates to UIEventArgs                    │
│  - Raises UIEvent C# event                      │
└────────────────┬────────────────────────────────┘
                 │
                 │ event EventHandler<UIEventArgs> UIEvent
                 │
                 v
┌─────────────────────────────────────────────────┐
│  ProcessEnvironment.OnUIEvent()                 │
│  - Subscribes to backend UIEvent                │
│  - Translates UIEventArgs to Win32 message      │
│  - Calls PostMessage()                          │
└────────────────┬────────────────────────────────┘
                 │
                 │ PostMessage(hwnd, WM_*, wParam, lParam)
                 │
                 v
┌─────────────────────────────────────────────────┐
│  Message Queue (System.Threading.Channels)      │
│  - Thread-safe channel for Win32 messages       │
└────────────────┬────────────────────────────────┘
                 │
                 │ GetMessageA() / PeekMessageA()
                 │
                 v
┌─────────────────────────────────────────────────┐
│  Win32 Application Message Loop                 │
│  - GetMessageA waits for messages               │
│  - DispatchMessageA calls window procedure      │
│  - Window procedure processes messages          │
└─────────────────────────────────────────────────┘
```

## Threading Model

### Event Processing Thread (Background)
- Runs on separate thread via `Task.Run()`
- Polls backends at 60 FPS (16ms intervals)
- Uses `CancellationToken` for cooperative cancellation
- Started by `StartEventProcessing()` in `RunAsync()`
- Stopped by `StopEventProcessing()` when emulation ends

### Main Emulation Thread
- Executes CPU instructions
- Processes Win32 API calls
- Handles message dispatching via `DispatchMessageA`
- Context switches between emulated threads

### Synchronization
- Message queue uses `System.Threading.Channels` (thread-safe)
- Backend event processing is single-threaded (no locking needed)
- `PostMessage()` is thread-safe via channel writes
- `GetMessageAsync()` properly uses async/await for cooperative multitasking

## Usage

### For Emulator Users

The event system is automatically integrated when you run an application:

```csharp
var emulator = new Emulator();
emulator.LoadExecutable("app.exe");

// Subscribe backends to event system (if using custom backends)
emulator.SubscribeToUIEvents(renderingBackend, inputBackend);

// Run normally - events are processed automatically
await emulator.RunAsync();
```

### For Backend Implementers

To implement a new backend with event support:

1. **Implement the `UIEvent` event**:
```csharp
public class MyRenderingBackend : IRenderingBackend
{
    public event EventHandler<UIEventArgs>? UIEvent;
    
    protected virtual void OnUIEvent(UIEventArgs e)
    {
        UIEvent?.Invoke(this, e);
    }
}
```

2. **Translate native events in ProcessEvents()**:
```csharp
public void ProcessEvents()
{
    // Poll native events
    while (PollNativeEvent(out var nativeEvent))
    {
        // Translate to UIEventArgs
        var uiEvent = new UIEventArgs
        {
            EventType = TranslateEventType(nativeEvent),
            MouseX = nativeEvent.X,
            MouseY = nativeEvent.Y,
            // ... other properties
        };
        
        // Raise event
        OnUIEvent(uiEvent);
    }
}
```

## Benefits

### Before
❌ UI events were never processed  
❌ Message queue never received input events  
❌ Applications would hang waiting for events  
❌ No background event processing  
❌ Tight coupling between backends and message queue  

### After
✅ Event-driven architecture with C# events  
✅ Background thread processes UI events continuously  
✅ Events automatically translated to Win32 messages  
✅ Async/await for cooperative multitasking  
✅ Loose coupling via event subscription  
✅ 60 FPS event processing rate  
✅ Proper threading with cancellation support  
✅ Thread-safe message queue via Channels  

## Testing

### Manual Testing
1. Run any Win32 GUI application that uses message loops
2. Click buttons, move mouse, press keys
3. Verify events are received and processed

### Integration Testing
Create tests that:
1. Mock a rendering backend
2. Raise UIEvent with test data
3. Verify Win32 messages appear in queue
4. Verify correct message translation

Example:
```csharp
[Fact]
public void UIEvent_MouseClick_PostsWM_LBUTTONDOWN()
{
    // Arrange
    var backend = new MockRenderingBackend();
    var env = new ProcessEnvironment(vm);
    env.SubscribeToUIEvents(backend, null);
    
    // Act
    backend.RaiseMouseClick(10, 20);
    
    // Assert
    var msg = env.TryPeekMessage(out var message, 0, 0, 0, false);
    Assert.True(msg);
    Assert.Equal(0x0201u, message.Message); // WM_LBUTTONDOWN
}
```

## Future Enhancements

1. **GLFW Event Callbacks**: Implement proper callback-based event handling for GLFW backend
2. **Vulkan Window Events**: Hook into Silk.NET.Windowing events for Vulkan backend
3. **Touch Events**: Add support for touch input on mobile/tablet platforms
4. **Gamepad Events**: Translate gamepad input to DirectInput messages
5. **Window Focus Management**: Better handling of multiple windows and focus
6. **Event Filtering**: Allow applications to filter events before posting to queue
7. **Performance Metrics**: Track event processing latency and throughput
8. **Adaptive Polling Rate**: Adjust event processing rate based on activity

## Related Files

- `Win32Emu/Rendering/UIEventArgs.cs` - Event argument definitions
- `Win32Emu/Rendering/IInputBackend.cs` - Input backend interface with UIEvent
- `Win32Emu/Rendering/IRenderingBackend.cs` - Rendering backend interface with UIEvent
- `Win32Emu/Rendering/SilkSdlRenderingBackend.cs` - SDL event translation implementation
- `Win32Emu/Rendering/SilkGlfwRenderingBackend.cs` - GLFW backend (stub)
- `Win32Emu/Rendering/SilkVulkanRenderingBackend.cs` - Vulkan backend (stub)
- `Win32Emu/Rendering/SilkInputBackend.cs` - Input backend base implementation
- `Win32Emu/Win32/ProcessEnvironment.cs` - Event subscription and Win32 message translation
- `Win32Emu/Emulator.cs` - Background event processing thread integration
- `ASYNC_THREADING_IMPLEMENTATION.md` - Related async/threading documentation
- `MESSAGE_QUEUE_IMPLEMENTATION.md` - Message queue documentation

## Summary

This implementation transforms Win32Emu from a polling-based system to an event-driven architecture using C# events, async/await, and proper threading. UI events from rendering backends are now automatically translated to Win32 messages and posted to the message queue, enabling GUI applications to receive and respond to user input in real-time.

The key innovation is the **background event processing thread** that continuously polls backends at 60 FPS, combined with **C# event subscription** that loosely couples backends to the message queue. This ensures events are never missed due to timing issues or blocking message loops.
