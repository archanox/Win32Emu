using Win32Emu.Win32.Messaging;
using Win32Emu.Win32.Messaging.Handlers;

namespace Win32Emu.Tests.User32.Messaging;

/// <summary>
/// Tests for the MessageDispatcher system
/// </summary>
public class MessageDispatcherTests : IDisposable
{
	private readonly MessageDispatcher _dispatcher;
	private uint _lastHandledMessage;
	private uint _handlerCallCount;

	public MessageDispatcherTests()
	{
		_dispatcher = new MessageDispatcher();
		_lastHandledMessage = 0;
		_handlerCallCount = 0;
	}

	[Fact]
	public void RegisterHandler_ShouldAllowHandlerRegistration()
	{
		// Arrange
		var handler = new TestMessageHandler();

		// Act
		_dispatcher.RegisterHandler(WM.PAINT, handler);

		// Assert
		Assert.True(_dispatcher.HasHandlers(WM.PAINT));
	}

	[Fact]
	public void Dispatch_WithRegisteredHandler_ShouldInvokeHandler()
	{
		// Arrange
		var handler = new TestMessageHandler();
		_dispatcher.RegisterHandler(WM.PAINT, handler);
		var message = new PaintMessage(0x00010000);

		// Act
		var result = _dispatcher.Dispatch(message);

		// Assert
		Assert.Equal(42u, result); // TestMessageHandler returns 42
		Assert.True(handler.WasCalled);
	}

	[Fact]
	public void Dispatch_WithNoHandler_ShouldReturnZero()
	{
		// Arrange
		var message = new PaintMessage(0x00010000);

		// Act
		var result = _dispatcher.Dispatch(message);

		// Assert
		Assert.Equal(0u, result);
	}

	[Fact]
	public void RegisterHandler_WithLambda_ShouldWork()
	{
		// Arrange
		var callCount = 0;
		_dispatcher.RegisterHandler(WM.COMMAND, msg =>
		{
			callCount++;
			return 123;
		});

		var message = new CommandMessage(0x00010000, 0x00020003, 0x00040000);

		// Act
		var result = _dispatcher.Dispatch(message);

		// Assert
		Assert.Equal(123u, result);
		Assert.Equal(1, callCount);
	}

	[Fact]
	public void Dispatch_WithMultipleHandlers_ShouldInvokeAllHandlers()
	{
		// Arrange
		var callCount = 0;
		_dispatcher.RegisterHandler(WM.CLOSE, msg => { callCount++; return 1; });
		_dispatcher.RegisterHandler(WM.CLOSE, msg => { callCount++; return 2; });
		_dispatcher.RegisterHandler(WM.CLOSE, msg => { callCount++; return 3; });

		var message = new CloseMessage(0x00010000);

		// Act
		var result = _dispatcher.Dispatch(message);

		// Assert
		Assert.Equal(3u, result); // Last handler's return value
		Assert.Equal(3, callCount); // All three handlers called
	}

	[Fact]
	public void UnregisterHandlers_ShouldRemoveAllHandlers()
	{
		// Arrange
		_dispatcher.RegisterHandler(WM.PAINT, new TestMessageHandler());
		Assert.True(_dispatcher.HasHandlers(WM.PAINT));

		// Act
		_dispatcher.UnregisterHandlers(WM.PAINT);

		// Assert
		Assert.False(_dispatcher.HasHandlers(WM.PAINT));
	}

	[Fact]
	public void Clear_ShouldRemoveAllHandlers()
	{
		// Arrange
		_dispatcher.RegisterHandler(WM.PAINT, new TestMessageHandler());
		_dispatcher.RegisterHandler(WM.CLOSE, msg => 0);
		_dispatcher.RegisterHandler(WM.COMMAND, msg => 0);
		
		Assert.True(_dispatcher.HasHandlers(WM.PAINT));
		Assert.True(_dispatcher.HasHandlers(WM.CLOSE));
		Assert.True(_dispatcher.HasHandlers(WM.COMMAND));

		// Act
		_dispatcher.Clear();

		// Assert
		Assert.False(_dispatcher.HasHandlers(WM.PAINT));
		Assert.False(_dispatcher.HasHandlers(WM.CLOSE));
		Assert.False(_dispatcher.HasHandlers(WM.COMMAND));
	}

	[Fact]
	public void CommandMessage_ShouldParsewParamCorrectly()
	{
		// Arrange
		var controlId = 0x0001u;
		var notificationCode = 0x0002u;
		var wParam = (notificationCode << 16) | controlId;
		var lParam = 0x00030000u;

		// Act
		var message = new CommandMessage(0x00010000, wParam, lParam);

		// Assert
		Assert.Equal(controlId, message.ControlId);
		Assert.Equal(notificationCode, message.NotificationCode);
		Assert.Equal(lParam, message.ControlHandle);
	}

	[Fact]
	public void LButtonDownMessage_ShouldParseCoordinatesCorrectly()
	{
		// Arrange
		var x = (short)100;
		var y = (short)200;
		var lParam = (uint)(((y & 0xFFFF) << 16) | (x & 0xFFFF));

		// Act
		var message = new LButtonDownMessage(0x00010000, 0, lParam);

		// Assert
		Assert.Equal(x, message.X);
		Assert.Equal(y, message.Y);
	}

	[Fact]
	public void KeyDownMessage_ShouldParseVirtualKeyCode()
	{
		// Arrange
		var virtualKeyCode = 0x41u; // 'A' key
		var repeatCount = 1u;
		var scanCode = 0x1Eu;
		var lParam = (scanCode << 16) | repeatCount;

		// Act
		var message = new KeyDownMessage(0x00010000, virtualKeyCode, lParam);

		// Assert
		Assert.Equal(virtualKeyCode, message.VirtualKeyCode);
		Assert.Equal(repeatCount, message.RepeatCount);
		Assert.Equal(scanCode, message.ScanCode);
	}

	[Fact]
	public void MessageFactory_ShouldCreateTypedMessages()
	{
		// Act
		var paintMsg = MessageFactory.CreateMessage(0x00010000, WM.PAINT, 0, 0);
		var closeMsg = MessageFactory.CreateMessage(0x00010000, WM.CLOSE, 0, 0);
		var commandMsg = MessageFactory.CreateMessage(0x00010000, WM.COMMAND, 0x00020001, 0x00030000);

		// Assert
		Assert.IsType<PaintMessage>(paintMsg);
		Assert.IsType<CloseMessage>(closeMsg);
		Assert.IsType<CommandMessage>(commandMsg);
	}

	public void Dispose()
	{
		_dispatcher.Clear();
	}

	// Test message handler for testing purposes
	private class TestMessageHandler : IMessageHandler<PaintMessage>
	{
		public bool WasCalled { get; private set; }

		public uint Handle(PaintMessage message)
		{
			WasCalled = true;
			return 42;
		}
	}
}
