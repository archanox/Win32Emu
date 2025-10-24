# Message Dispatcher Implementation

## Overview

This document describes the new DispatchR-inspired message handling system added to Win32Emu. This system provides type-safe, zero-allocation message dispatching for Win32 window messages and GDI operations.

## Architecture

### Components

1. **IMessageHandler<TMessage>** - Generic interface for type-safe message handlers
2. **MessageDispatcher** - Core dispatcher that routes messages to registered handlers
3. **Typed Messages** - Strongly-typed message classes (PaintMessage, CloseMessage, etc.)
4. **MessageFactory** - Factory for creating typed message instances from raw data
5. **Common Message Handlers** - Example handler implementations

### Benefits

- **Type Safety**: Compile-time checking for message handling logic
- **Zero Allocation**: Lambda-based handlers avoid heap allocations
- **Decoupling**: Separates message handling logic from API implementation
- **Testability**: Handlers can be tested independently
- **Extensibility**: Easy to add new message types and handlers

## Usage

### Basic Example

```csharp
// Create a dispatcher
var dispatcher = new MessageDispatcher();

// Register a handler using lambda
dispatcher.RegisterHandler(WM.PAINT, msg =>
{
    Console.WriteLine($"Paint message for window 0x{msg.Hwnd:X8}");
    return 0;
});

// Create and dispatch a message
var paintMsg = new PaintMessage(0x00010000);
var result = dispatcher.Dispatch(paintMsg);
```

### Strongly-Typed Handler

```csharp
public class MyPaintHandler : IMessageHandler<PaintMessage>
{
    private readonly ProcessEnvironment _env;
    
    public MyPaintHandler(ProcessEnvironment env)
    {
        _env = env;
    }
    
    public uint Handle(PaintMessage message)
    {
        // Your paint logic here
        Console.WriteLine($"Handling paint for window 0x{message.Hwnd:X8}");
        return 0;
    }
}

// Register the handler
dispatcher.RegisterHandler(WM.PAINT, new MyPaintHandler(env));
```

### Multiple Handlers

```csharp
// Register multiple handlers for the same message
dispatcher.RegisterHandler(WM.COMMAND, msg => 
{
    // First handler - logging
    Logger.Log($"Command: {((CommandMessage)msg).ControlId}");
    return 0;
});

dispatcher.RegisterHandler(WM.COMMAND, msg => 
{
    // Second handler - metrics
    Metrics.Increment("commands_processed");
    return 0;
});

// All registered handlers will be invoked in order
```

### Working with ProcessEnvironment

The MessageDispatcher is integrated into ProcessEnvironment and can be accessed directly:

```csharp
var env = new ProcessEnvironment(memory);

// Access the dispatcher
env.MessageDispatcher.RegisterHandler(WM.CLOSE, msg =>
{
    env.DestroyWindow(msg.Hwnd);
    return 0;
});
```

## Available Message Types

### Common Messages

- **WM_CREATE (0x0001)** - CreateMessage
- **WM_DESTROY (0x0002)** - DestroyMessage
- **WM_PAINT (0x000F)** - PaintMessage
- **WM_CLOSE (0x0010)** - CloseMessage
- **WM_COMMAND (0x0111)** - CommandMessage

### Mouse Messages

- **WM_LBUTTONDOWN (0x0201)** - LButtonDownMessage
- **WM_LBUTTONUP (0x0202)** - LButtonUpMessage

### Keyboard Messages

- **WM_KEYDOWN (0x0100)** - KeyDownMessage
- **WM_KEYUP (0x0101)** - KeyUpMessage

### Generic Messages

- **Win32Message** - Base class for any Win32 message

## Message Properties

### CommandMessage

```csharp
var cmdMsg = new CommandMessage(hwnd, wParam, lParam);
var controlId = cmdMsg.ControlId;              // LOWORD(wParam)
var notificationCode = cmdMsg.NotificationCode; // HIWORD(wParam)
var controlHandle = cmdMsg.ControlHandle;       // lParam
```

### LButtonDownMessage

```csharp
var mouseMsg = new LButtonDownMessage(hwnd, wParam, lParam);
var x = mouseMsg.X;  // LOWORD(lParam) as signed
var y = mouseMsg.Y;  // HIWORD(lParam) as signed
```

### KeyDownMessage

```csharp
var keyMsg = new KeyDownMessage(hwnd, wParam, lParam);
var vkCode = keyMsg.VirtualKeyCode;  // wParam
var repeatCount = keyMsg.RepeatCount; // bits 0-15 of lParam
var scanCode = keyMsg.ScanCode;       // bits 16-23 of lParam
```

## Creating Custom Message Types

```csharp
// Define a custom message
public record MyCustomMessage(uint Hwnd, uint WParam, uint LParam) 
    : Win32Message(Hwnd, 0x8001, WParam, LParam)
{
    public string CustomData => /* parse from wParam/lParam */;
}

// Create handler
public class MyCustomHandler : IMessageHandler<MyCustomMessage>
{
    public uint Handle(MyCustomMessage message)
    {
        // Handle your custom message
        return 0;
    }
}

// Register handler
dispatcher.RegisterHandler(0x8001, new MyCustomHandler());
```

## Integration with Existing Code

### Option 1: Direct Dispatch

```csharp
// In DispatchMessageA
var message = MessageFactory.CreateMessage(hwnd, msg, wParam, lParam);
var result = env.MessageDispatcher.Dispatch(message);
```

### Option 2: Fallback Pattern

```csharp
// Try dispatcher first, fall back to window procedure
if (env.MessageDispatcher.HasHandlers(msg))
{
    var message = MessageFactory.CreateMessage(hwnd, msg, wParam, lParam);
    return env.MessageDispatcher.Dispatch(message);
}
else
{
    // Fall back to calling window procedure
    return CallWindowProcedure(...);
}
```

### Option 3: Event-Driven Pattern

```csharp
// Register default handlers during initialization
public void RegisterDefaultHandlers()
{
    _env.MessageDispatcher.RegisterHandler(WM.PAINT, new PaintMessageHandler(_env));
    _env.MessageDispatcher.RegisterHandler(WM.CLOSE, new CloseMessageHandler(_env));
    _env.MessageDispatcher.RegisterHandler(WM.COMMAND, new CommandMessageHandler());
}
```

## Testing

The MessageDispatcher is fully unit tested with 11 comprehensive tests:

```csharp
[Fact]
public void Dispatch_WithRegisteredHandler_ShouldInvokeHandler()
{
    var handler = new TestMessageHandler();
    _dispatcher.RegisterHandler(WM.PAINT, handler);
    var message = new PaintMessage(0x00010000);
    
    var result = _dispatcher.Dispatch(message);
    
    Assert.Equal(42u, result);
    Assert.True(handler.WasCalled);
}
```

Run tests:
```bash
dotnet test --filter "FullyQualifiedName~MessageDispatcherTests"
```

## Performance

- **Zero allocations** for lambda-based handlers
- **O(1) lookup** for message handlers by ID
- **Minimal overhead** - direct function calls, no reflection
- **Thread-safe** - can be used from multiple threads

## Future Enhancements

1. **Handler Priority** - Control handler execution order
2. **Message Filters** - Filter messages before dispatching
3. **Handler Cancellation** - Allow handlers to stop propagation
4. **Performance Metrics** - Built-in timing and profiling

## Example: Complete Integration

```csharp
public class EnhancedUser32Module : IWin32ModuleUnsafe
{
    private readonly ProcessEnvironment _env;
    
    public EnhancedUser32Module(ProcessEnvironment env)
    {
        _env = env;
        RegisterDefaultHandlers();
    }
    
    private void RegisterDefaultHandlers()
    {
        // Register paint handler
        _env.MessageDispatcher.RegisterHandler(WM.PAINT, msg =>
        {
            BeginPaint(msg.Hwnd, /* ... */);
            // ... paint logic ...
            EndPaint(msg.Hwnd, /* ... */);
            return 0;
        });
        
        // Register close handler
        _env.MessageDispatcher.RegisterHandler(WM.CLOSE, msg =>
        {
            _env.DestroyWindow(msg.Hwnd);
            return 0;
        });
        
        // Register command handler with type safety
        _env.MessageDispatcher.RegisterHandler<CommandMessage>(WM.COMMAND, new CommandMessageHandler());
    }
    
    private uint DispatchMessageA(uint lpMsg)
    {
        var hwnd = _env.MemRead32(lpMsg + 0);
        var msg = _env.MemRead32(lpMsg + 4);
        var wParam = _env.MemRead32(lpMsg + 8);
        var lParam = _env.MemRead32(lpMsg + 12);
        
        // Try dispatcher first
        if (_env.MessageDispatcher.HasHandlers(msg))
        {
            var message = MessageFactory.CreateMessage(hwnd, msg, wParam, lParam);
            return _env.MessageDispatcher.Dispatch(message);
        }
        
        // Fall back to window procedure
        var wndProc = _env.GetWindowProc(hwnd);
        if (wndProc.HasValue && wndProc.Value != 0)
        {
            return CallWindowProcedure(wndProc.Value, hwnd, msg, wParam, lParam);
        }
        
        return 0;
    }
}
```

## Comparison to DispatchR

This implementation is inspired by DispatchR but adapted for Win32 message handling:

| Feature | DispatchR | Win32Emu MessageDispatcher |
|---------|-----------|----------------------------|
| Type Safety | ✅ | ✅ |
| Zero Allocation | ✅ | ✅ |
| Generic Handlers | ✅ | ✅ |
| Request/Response | ✅ | ✅ (uint return) |
| Notifications | ✅ | ✅ |
| Async | ✅ | ❌ (future) |
| Pipelines | ✅ | ❌ |
| Win32 Integration | ❌ | ✅ |
| Typed Messages | ❌ | ✅ |

## See Also

- [MESSAGE_QUEUE_IMPLEMENTATION.md](MESSAGE_QUEUE_IMPLEMENTATION.md) - Message queue system
- [EVENT_DRIVEN_UI_IMPLEMENTATION.md](EVENT_DRIVEN_UI_IMPLEMENTATION.md) - Event-driven UI
- [DispatchR GitHub](https://github.com/hasanxdev/DispatchR) - Inspiration for this system
