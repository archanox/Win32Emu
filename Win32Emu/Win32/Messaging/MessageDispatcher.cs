using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Win32.Messaging;

/// <summary>
/// Dispatches Win32 messages to registered handlers
/// Inspired by DispatchR/MediatR pattern for zero-allocation, type-safe message handling
/// Supports both synchronous and asynchronous handlers
/// </summary>
public class MessageDispatcher
{
	private readonly Dictionary<uint, List<Func<IMessage, uint>>> _handlers = new();
	private readonly Dictionary<uint, List<Func<IMessage, CancellationToken, Task<uint>>>> _asyncHandlers = new();
	private readonly ILogger _logger;

	public MessageDispatcher(ILogger? logger = null)
	{
		_logger = logger ?? NullLogger.Instance;
	}

	/// <summary>
	/// Register a message handler for a specific message type
	/// </summary>
	/// <typeparam name="TMessage">The message type</typeparam>
	/// <param name="messageId">The Win32 message ID</param>
	/// <param name="handler">The handler instance</param>
	public void RegisterHandler<TMessage>(uint messageId, IMessageHandler<TMessage> handler) 
		where TMessage : IMessage
	{
		if (!_handlers.ContainsKey(messageId))
		{
			_handlers[messageId] = new List<Func<IMessage, uint>>();
		}

		// Wrap the strongly-typed handler in a function that accepts IMessage
		_handlers[messageId].Add(msg =>
		{
			if (msg is TMessage typedMsg)
			{
				return handler.Handle(typedMsg);
			}
			_logger.LogWarning("[MessageDispatcher] Message type mismatch for message ID 0x{MessageId:X4}", messageId);
			return 0;
		});

		_logger.LogDebug("[MessageDispatcher] Registered handler for message ID 0x{MessageId:X4}", messageId);
	}

	/// <summary>
	/// Register an async message handler for a specific message type
	/// </summary>
	/// <typeparam name="TMessage">The message type</typeparam>
	/// <param name="messageId">The Win32 message ID</param>
	/// <param name="handler">The async handler instance</param>
	public void RegisterAsyncHandler<TMessage>(uint messageId, IAsyncMessageHandler<TMessage> handler) 
		where TMessage : IMessage
	{
		if (!_asyncHandlers.ContainsKey(messageId))
		{
			_asyncHandlers[messageId] = new List<Func<IMessage, CancellationToken, Task<uint>>>();
		}

		// Wrap the strongly-typed handler in a function that accepts IMessage
		_asyncHandlers[messageId].Add(async (msg, ct) =>
		{
			if (msg is TMessage typedMsg)
			{
				return await handler.HandleAsync(typedMsg, ct);
			}
			_logger.LogWarning("[MessageDispatcher] Message type mismatch for message ID 0x{MessageId:X4}", messageId);
			return 0;
		});

		_logger.LogDebug("[MessageDispatcher] Registered async handler for message ID 0x{MessageId:X4}", messageId);
	}

	/// <summary>
	/// Register a lambda handler for a specific message type
	/// </summary>
	/// <param name="messageId">The Win32 message ID</param>
	/// <param name="handler">The handler function</param>
	public void RegisterHandler(uint messageId, Func<IMessage, uint> handler)
	{
		if (!_handlers.ContainsKey(messageId))
		{
			_handlers[messageId] = new List<Func<IMessage, uint>>();
		}

		_handlers[messageId].Add(handler);
		_logger.LogDebug("[MessageDispatcher] Registered lambda handler for message ID 0x{MessageId:X4}", messageId);
	}

	/// <summary>
	/// Register an async lambda handler for a specific message type
	/// </summary>
	/// <param name="messageId">The Win32 message ID</param>
	/// <param name="handler">The async handler function</param>
	public void RegisterAsyncHandler(uint messageId, Func<IMessage, CancellationToken, Task<uint>> handler)
	{
		if (!_asyncHandlers.ContainsKey(messageId))
		{
			_asyncHandlers[messageId] = new List<Func<IMessage, CancellationToken, Task<uint>>>();
		}

		_asyncHandlers[messageId].Add(handler);
		_logger.LogDebug("[MessageDispatcher] Registered async lambda handler for message ID 0x{MessageId:X4}", messageId);
	}

	/// <summary>
	/// Dispatch a message to all registered handlers
	/// </summary>
	/// <param name="message">The message to dispatch</param>
	/// <returns>The result from the last handler, or 0 if no handlers</returns>
	public uint Dispatch(IMessage message)
	{
		if (!_handlers.TryGetValue(message.Message, out var handlers))
		{
			_logger.LogTrace("[MessageDispatcher] No handlers for message ID 0x{Message:X4}", message.Message);
			return 0;
		}

		uint result = 0;
		foreach (var handler in handlers)
		{
			try
			{
				result = handler(message);
				_logger.LogTrace("[MessageDispatcher] Handler for message 0x{Message:X4} returned 0x{Result:X8}", 
					message.Message, result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[MessageDispatcher] Error in handler for message 0x{Message:X4}", 
					message.Message);
			}
		}

		return result;
	}

	/// <summary>
	/// Dispatch a message to all registered async handlers
	/// </summary>
	/// <param name="message">The message to dispatch</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result from the last handler, or 0 if no handlers</returns>
	public async Task<uint> DispatchAsync(IMessage message, CancellationToken cancellationToken = default)
	{
		// First execute synchronous handlers
		uint result = Dispatch(message);

		// Then execute async handlers
		if (_asyncHandlers.TryGetValue(message.Message, out var asyncHandlers))
		{
			foreach (var handler in asyncHandlers)
			{
				try
				{
					result = await handler(message, cancellationToken);
					_logger.LogTrace("[MessageDispatcher] Async handler for message 0x{Message:X4} returned 0x{Result:X8}", 
						message.Message, result);
				}
				catch (OperationCanceledException)
				{
					_logger.LogDebug("[MessageDispatcher] Async handler cancelled for message 0x{Message:X4}", message.Message);
					throw;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "[MessageDispatcher] Error in async handler for message 0x{Message:X4}", 
						message.Message);
				}
			}
		}

		return result;
	}

	/// <summary>
	/// Check if any handlers are registered for a message
	/// </summary>
	/// <param name="messageId">The Win32 message ID</param>
	/// <returns>True if handlers exist</returns>
	public bool HasHandlers(uint messageId)
	{
		return (_handlers.ContainsKey(messageId) && _handlers[messageId].Count > 0) ||
		       (_asyncHandlers.ContainsKey(messageId) && _asyncHandlers[messageId].Count > 0);
	}

	/// <summary>
	/// Unregister all handlers for a specific message
	/// </summary>
	/// <param name="messageId">The Win32 message ID</param>
	public void UnregisterHandlers(uint messageId)
	{
		var removed = _handlers.Remove(messageId);
		removed |= _asyncHandlers.Remove(messageId);
		
		if (removed)
		{
			_logger.LogDebug("[MessageDispatcher] Unregistered all handlers for message ID 0x{MessageId:X4}", messageId);
		}
	}

	/// <summary>
	/// Clear all registered handlers
	/// </summary>
	public void Clear()
	{
		var count = _handlers.Count + _asyncHandlers.Count;
		_handlers.Clear();
		_asyncHandlers.Clear();
		_logger.LogDebug("[MessageDispatcher] Cleared all {Count} registered message handlers", count);
	}
}
