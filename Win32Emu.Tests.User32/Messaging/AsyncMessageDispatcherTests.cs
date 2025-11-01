using Win32Emu.Memory;
using Win32Emu.Tests.User32.TestInfrastructure;
using Win32Emu.Win32;
using Win32Emu.Win32.Messaging;

namespace Win32Emu.Tests.User32.Messaging;

/// <summary>
/// Tests for async message handling
/// </summary>
public class AsyncMessageDispatcherTests : IDisposable
{
	private readonly TestEnvironment _testEnv;
	private readonly ProcessEnvironment _env;

	public AsyncMessageDispatcherTests()
	{
		_testEnv = new TestEnvironment();
		_env = _testEnv.ProcessEnv;
	}

	[Fact]
	public async Task RegisterHandler_ShouldAllowAsyncHandlerRegistration()
	{
		// Arrange
		var handler = new TestAsyncMessageHandler();

		// Act
		_env.MessageDispatcher.RegisterHandler((uint)WM.PAINT, handler);

		// Assert
		Assert.True(_env.MessageDispatcher.HasHandlers((uint)WM.PAINT));
	}

	[Fact]
	public async Task DispatchAsync_WithRegisteredAsyncHandler_ShouldInvokeHandler()
	{
		// Arrange
		var handler = new TestAsyncMessageHandler();
		_env.MessageDispatcher.RegisterHandler((uint)WM.PAINT, handler);
		var message = new PaintMessage(0x00010000);

		// Act
		var result = await _env.MessageDispatcher.DispatchAsync(message);

		// Assert
		Assert.Equal(42u, result); // TestAsyncMessageHandler returns 42
		Assert.True(handler.WasCalled);
	}

	[Fact]
	public async Task RegisterHandler_WithLambda_ShouldWork()
	{
		// Arrange
		var callCount = 0;
		_env.MessageDispatcher.RegisterHandler((uint)WM.COMMAND, async (msg, ct) =>
		{
			await Task.Delay(10, ct); // Simulate async work
			callCount++;
			return 123;
		});

		var message = new CommandMessage(0x00010000, 0x00020003, 0x00040000);

		// Act
		var result = await _env.MessageDispatcher.DispatchAsync(message);

		// Assert
		Assert.Equal(123u, result);
		Assert.Equal(1, callCount);
	}

	[Fact]
	public async Task DispatchAsync_WithMultipleAsyncHandlers_ShouldInvokeAllHandlers()
	{
		// Arrange
		var executionOrder = new List<int>();

		_env.MessageDispatcher.RegisterHandler((uint)WM.CLOSE, async (msg, ct) =>
		{
			await Task.Delay(5, ct);
			executionOrder.Add(1);
			return 1;
		});
		
		_env.MessageDispatcher.RegisterHandler((uint)WM.CLOSE, async (msg, ct) =>
		{
			await Task.Delay(5, ct);
			executionOrder.Add(2);
			return 2;
		});
		
		_env.MessageDispatcher.RegisterHandler((uint)WM.CLOSE, async (msg, ct) =>
		{
			await Task.Delay(5, ct);
			executionOrder.Add(3);
			return 3;
		});

		var message = new CloseMessage(0x00010000);

		// Act
		var result = await _env.MessageDispatcher.DispatchAsync(message);

		// Assert
		Assert.Equal(3u, result); // Last handler's return value
		Assert.Equal(new[] { 1, 2, 3 }, executionOrder);
	}

	[Fact]
	public async Task DispatchAsync_WithCancellation_ShouldThrowOperationCancelledException()
	{
		// Arrange
		var cts = new CancellationTokenSource();
		_env.MessageDispatcher.RegisterHandler((uint)WM.PAINT, async (msg, ct) =>
		{
			await Task.Delay(100, ct); // Long operation
			return 0;
		});

		var message = new PaintMessage(0x00010000);
		cts.Cancel(); // Cancel before dispatch

		// Act & Assert
		// TaskCanceledException is a subclass of OperationCanceledException
		await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
		{
			await _env.MessageDispatcher.DispatchAsync(message, cts.Token);
		});
	}

	[Fact]
	public async Task DispatchAsync_WithMultipleAsyncHandlers_ShouldInvokeBoth()
	{
		// Arrange
		var firstCalled = false;
		var secondCalled = false;

		_env.MessageDispatcher.RegisterHandler((uint)WM.COMMAND, async (msg, ct) =>
		{
			firstCalled = true;
			return 10;
		});

		_env.MessageDispatcher.RegisterHandler((uint)WM.COMMAND, async (msg, ct) =>
		{
			await Task.Delay(5, ct);
			secondCalled = true;
			return 20;
		});

		var message = new CommandMessage(0x00010000, 0x00020003, 0x00040000);

		// Act
		var result = await _env.MessageDispatcher.DispatchAsync(message);

		// Assert
		Assert.True(firstCalled);
		Assert.True(secondCalled);
		Assert.Equal(20u, result); // Second handler's return value
	}

	[Fact]
	public async Task TypedAsyncHandler_ShouldProvideStrongTyping()
	{
		// Arrange
		var handler = new TestTypedAsyncCommandHandler();
		_env.MessageDispatcher.RegisterHandler((uint)WM.COMMAND, handler);

		var controlId = 0x0001u;
		var notificationCode = 0x0002u;
		var wParam = (notificationCode << 16) | controlId;
		var message = new CommandMessage(0x00010000, wParam, 0x00030000);

		// Act
		var result = await _env.MessageDispatcher.DispatchAsync(message);

		// Assert
		Assert.True(handler.WasCalled);
		Assert.Equal(controlId, handler.ReceivedControlId);
		Assert.Equal(notificationCode, handler.ReceivedNotificationCode);
	}

	public void Dispose()
	{
		_env.MessageDispatcher.Clear();
		_testEnv.Dispose();
	}

	// Test async message handler
	private class TestAsyncMessageHandler : IMessageHandler<PaintMessage>
	{
		public bool WasCalled { get; private set; }

		public async Task<uint> HandleAsync(PaintMessage message, CancellationToken cancellationToken = default)
		{
			await Task.Delay(10, cancellationToken); // Simulate async work
			WasCalled = true;
			return 42;
		}
	}

	// Test typed async handler
	private class TestTypedAsyncCommandHandler : IMessageHandler<CommandMessage>
	{
		public bool WasCalled { get; private set; }
		public uint ReceivedControlId { get; private set; }
		public uint ReceivedNotificationCode { get; private set; }

		public async Task<uint> HandleAsync(CommandMessage message, CancellationToken cancellationToken = default)
		{
			await Task.Delay(5, cancellationToken);
			WasCalled = true;
			ReceivedControlId = message.ControlId;
			ReceivedNotificationCode = message.NotificationCode;
			return 0;
		}
	}
}
