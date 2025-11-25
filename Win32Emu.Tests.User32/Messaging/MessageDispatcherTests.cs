using Xunit;
using Win32Emu.Win32.Messaging;
using Win32Emu.Win32.Messaging.Handlers;

namespace Win32Emu.Tests.User32.Messaging;

/// <summary>
/// Tests for the MessageDispatcher system
/// </summary>
[Trait("Category", "DllModuleTests")]
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
	public async Task RegisterHandler_ShouldAllowHandlerRegistration()
	{
		// Arrange
		var handler = new TestMessageHandler();

		// Act
		_dispatcher.RegisterHandler((uint)WM.PAINT, handler);

		// Assert
		Assert.True(_dispatcher.HasHandlers((uint)WM.PAINT));
	}

	[Fact]
	public async Task Dispatch_WithRegisteredHandler_ShouldInvokeHandler()
	{
		// Arrange
		var handler = new TestMessageHandler();
		_dispatcher.RegisterHandler((uint)WM.PAINT, handler);
		var message = new PaintMessage(0x00010000);

		// Act
		var result = await _dispatcher.DispatchAsync(message);

		// Assert
		Assert.Equal(42u, result); // TestMessageHandler returns 42
		Assert.True(handler.WasCalled);
	}

	[Fact]
	public async Task Dispatch_WithNoHandler_ShouldReturnZero()
	{
		// Arrange
		var message = new PaintMessage(0x00010000);

		// Act
		var result = await _dispatcher.DispatchAsync(message);

		// Assert
		Assert.Equal(0u, result);
	}

	[Fact]
	public async Task RegisterHandler_WithLambda_ShouldWork()
	{
		// Arrange
		var callCount = 0;
		_dispatcher.RegisterHandler((uint)WM.COMMAND, async (msg, ct) =>
		{
			callCount++;
			return 123;
		});

		var message = new CommandMessage(0x00010000, 0x00020003, 0x00040000);

		// Act
		var result = await _dispatcher.DispatchAsync(message);

		// Assert
		Assert.Equal(123u, result);
		Assert.Equal(1, callCount);
	}

	[Fact]
	public async Task Dispatch_WithMultipleHandlers_ShouldInvokeAllHandlers()
	{
		// Arrange
		var callCount = 0;
		_dispatcher.RegisterHandler((uint)WM.CLOSE, async (msg, ct) => { callCount++; return 1; });
		_dispatcher.RegisterHandler((uint)WM.CLOSE, async (msg, ct) => { callCount++; return 2; });
		_dispatcher.RegisterHandler((uint)WM.CLOSE, async (msg, ct) => { callCount++; return 3; });

		var message = new CloseMessage(0x00010000);

		// Act
		var result = await _dispatcher.DispatchAsync(message);

		// Assert
		Assert.Equal(3u, result); // Last handler's return value
		Assert.Equal(3, callCount); // All three handlers called
	}

	[Fact]
	public async Task UnregisterHandlers_ShouldRemoveAllHandlers()
	{
		// Arrange
		_dispatcher.RegisterHandler((uint)WM.PAINT, new TestMessageHandler());
		Assert.True(_dispatcher.HasHandlers((uint)WM.PAINT));

		// Act
		_dispatcher.UnregisterHandlers((uint)WM.PAINT);

		// Assert
		Assert.False(_dispatcher.HasHandlers((uint)WM.PAINT));
	}

	[Fact]
	public async Task Clear_ShouldRemoveAllHandlers()
	{
		// Arrange
		_dispatcher.RegisterHandler((uint)WM.PAINT, new TestMessageHandler());
		_dispatcher.RegisterHandler((uint)WM.CLOSE, async (msg, ct) => 0);
		_dispatcher.RegisterHandler((uint)WM.COMMAND, async (msg, ct) => 0);
		
		Assert.True(_dispatcher.HasHandlers((uint)WM.PAINT));
		Assert.True(_dispatcher.HasHandlers((uint)WM.CLOSE));
		Assert.True(_dispatcher.HasHandlers((uint)WM.COMMAND));

		// Act
		_dispatcher.Clear();

		// Assert
		Assert.False(_dispatcher.HasHandlers((uint)WM.PAINT));
		Assert.False(_dispatcher.HasHandlers((uint)WM.CLOSE));
		Assert.False(_dispatcher.HasHandlers((uint)WM.COMMAND));
	}

	[Fact]
	public async Task CommandMessage_ShouldParsewParamCorrectly()
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
	public async Task LButtonDownMessage_ShouldParseCoordinatesCorrectly()
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
	public async Task KeyDownMessage_ShouldParseVirtualKeyCode()
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
	public async Task MessageFactory_ShouldCreateTypedMessages()
	{
		// Act
		var paintMsg = MessageFactory.CreateMessage(0x00010000, (uint)WM.PAINT, 0, 0);
		var closeMsg = MessageFactory.CreateMessage(0x00010000, (uint)WM.CLOSE, 0, 0);
		var commandMsg = MessageFactory.CreateMessage(0x00010000, (uint)WM.COMMAND, 0x00020001, 0x00030000);

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

		public async Task<uint> HandleAsync(PaintMessage message, CancellationToken cancellationToken = default)
		{
			WasCalled = true;
			return 42;
		}
	}
}
