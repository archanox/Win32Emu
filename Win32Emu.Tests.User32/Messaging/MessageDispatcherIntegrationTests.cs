using Win32Emu.Memory;
using Win32Emu.Tests.User32.TestInfrastructure;
using Win32Emu.Win32;
using Win32Emu.Win32.Messaging;
using Win32Emu.Win32.Messaging.Handlers;

namespace Win32Emu.Tests.User32.Messaging;

/// <summary>
/// Integration tests showing how MessageDispatcher works with ProcessEnvironment
/// </summary>
public class MessageDispatcherIntegrationTests : IDisposable
{
	private readonly TestEnvironment _testEnv;
	private readonly ProcessEnvironment _env;
	private uint _handlerCallCount;
	private uint _lastHandledHwnd;

	public MessageDispatcherIntegrationTests()
	{
		_testEnv = new TestEnvironment();
		_env = _testEnv.ProcessEnv;
		_handlerCallCount = 0;
		_lastHandledHwnd = 0;
	}

	[Fact]
	public void MessageDispatcher_ShouldBeAccessibleFromProcessEnvironment()
	{
		// Arrange & Act
		var dispatcher = _env.MessageDispatcher;

		// Assert
		Assert.NotNull(dispatcher);
	}

	[Fact]
	public void MessageDispatcher_ShouldHandleRegisteredMessages()
	{
		// Arrange
		var hwnd = 0x00010000u;
		_env.MessageDispatcher.RegisterHandler(WM.PAINT, msg =>
		{
			_handlerCallCount++;
			_lastHandledHwnd = msg.Hwnd;
			return 42;
		});

		// Act
		var message = new PaintMessage(hwnd);
		var result = _env.MessageDispatcher.Dispatch(message);

		// Assert
		Assert.Equal(42u, result);
		Assert.Equal(1u, _handlerCallCount);
		Assert.Equal(hwnd, _lastHandledHwnd);
	}

	[Fact]
	public void MessageDispatcher_WithTypedHandler_ShouldProvideStrongTyping()
	{
		// Arrange
		var hwnd = 0x00010000u;
		var handler = new TestCloseHandler(_env);
		_env.MessageDispatcher.RegisterHandler(WM.CLOSE, handler);

		// Act
		var message = new CloseMessage(hwnd);
		var result = _env.MessageDispatcher.Dispatch(message);

		// Assert
		Assert.Equal(123u, result);
		Assert.True(handler.WasCalled);
		Assert.Equal(hwnd, handler.HandledHwnd);
	}

	[Fact]
	public void MessageFactory_ShouldCreateTypedMessagesForDispatch()
	{
		// Arrange
		var hwnd = 0x00010000u;
		var controlId = 0x0001u;
		var notificationCode = 0x0002u;
		var wParam = (notificationCode << 16) | controlId;
		var lParam = 0x00020000u;

		var commandHandlerCalled = false;
		_env.MessageDispatcher.RegisterHandler(WM.COMMAND, msg =>
		{
			if (msg is CommandMessage cmdMsg)
			{
				Assert.Equal(controlId, cmdMsg.ControlId);
				Assert.Equal(notificationCode, cmdMsg.NotificationCode);
				Assert.Equal(lParam, cmdMsg.ControlHandle);
				commandHandlerCalled = true;
			}
			return 0;
		});

		// Act
		var message = MessageFactory.CreateMessage(hwnd, WM.COMMAND, wParam, lParam);
		_env.MessageDispatcher.Dispatch(message);

		// Assert
		Assert.IsType<CommandMessage>(message);
		Assert.True(commandHandlerCalled);
	}

	[Fact]
	public void MessageDispatcher_WithCommonHandlers_ShouldWorkWithProcessEnvironment()
	{
		// Arrange
		var hwnd = 0x00010000u;
		
		// Register common handlers
		_env.MessageDispatcher.RegisterHandler(WM.PAINT, new PaintMessageHandler(_env));
		_env.MessageDispatcher.RegisterHandler(WM.CLOSE, new CloseMessageHandler(_env));
		_env.MessageDispatcher.RegisterHandler(WM.COMMAND, new CommandMessageHandler());

		// Act - Dispatch various messages
		var paintResult = _env.MessageDispatcher.Dispatch(new PaintMessage(hwnd));
		var closeResult = _env.MessageDispatcher.Dispatch(new CloseMessage(hwnd));
		var cmdResult = _env.MessageDispatcher.Dispatch(new CommandMessage(hwnd, 0x00010002, 0x00020000));

		// Assert - All handlers should execute without error
		Assert.Equal(0u, paintResult);
		Assert.Equal(0u, closeResult);
		Assert.Equal(0u, cmdResult);
	}

	[Fact]
	public void MessageDispatcher_MultipleHandlers_ShouldExecuteInOrder()
	{
		// Arrange
		var executionOrder = new List<int>();
		
		_env.MessageDispatcher.RegisterHandler(WM.PAINT, msg => { executionOrder.Add(1); return 1; });
		_env.MessageDispatcher.RegisterHandler(WM.PAINT, msg => { executionOrder.Add(2); return 2; });
		_env.MessageDispatcher.RegisterHandler(WM.PAINT, msg => { executionOrder.Add(3); return 3; });

		// Act
		var result = _env.MessageDispatcher.Dispatch(new PaintMessage(0x00010000));

		// Assert
		Assert.Equal(3u, result); // Last handler's return value
		Assert.Equal(new[] { 1, 2, 3 }, executionOrder);
	}

	[Fact]
	public void MessageDispatcher_CanBeCleared_AfterRegistration()
	{
		// Arrange
		_env.MessageDispatcher.RegisterHandler(WM.PAINT, msg => 42);
		_env.MessageDispatcher.RegisterHandler(WM.CLOSE, msg => 43);
		Assert.True(_env.MessageDispatcher.HasHandlers(WM.PAINT));
		Assert.True(_env.MessageDispatcher.HasHandlers(WM.CLOSE));

		// Act
		_env.MessageDispatcher.Clear();

		// Assert
		Assert.False(_env.MessageDispatcher.HasHandlers(WM.PAINT));
		Assert.False(_env.MessageDispatcher.HasHandlers(WM.CLOSE));
	}

	public void Dispose()
	{
		_env.MessageDispatcher.Clear();
		_testEnv.Dispose();
	}

	// Test handler for integration testing
	private class TestCloseHandler : IMessageHandler<CloseMessage>
	{
		private readonly ProcessEnvironment _env;
		public bool WasCalled { get; private set; }
		public uint HandledHwnd { get; private set; }

		public TestCloseHandler(ProcessEnvironment env)
		{
			_env = env;
		}

		public uint Handle(CloseMessage message)
		{
			WasCalled = true;
			HandledHwnd = message.Hwnd;
			// Don't actually destroy window in test
			return 123;
		}
	}
}
