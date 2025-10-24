using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Win32.Messaging;

/// <summary>
/// Dispatches Win32 messages to registered handlers
/// Inspired by DispatchR/MediatR pattern for zero-allocation, type-safe message handling
/// </summary>
public class MessageDispatcher
{
	private readonly Dictionary<uint, List<Func<IMessage, uint>>> _handlers = new();
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
	/// Check if any handlers are registered for a message
	/// </summary>
	/// <param name="messageId">The Win32 message ID</param>
	/// <returns>True if handlers exist</returns>
	public bool HasHandlers(uint messageId)
	{
		return _handlers.ContainsKey(messageId) && _handlers[messageId].Count > 0;
	}

	/// <summary>
	/// Unregister all handlers for a specific message
	/// </summary>
	/// <param name="messageId">The Win32 message ID</param>
	public void UnregisterHandlers(uint messageId)
	{
		if (_handlers.Remove(messageId))
		{
			_logger.LogDebug("[MessageDispatcher] Unregistered all handlers for message ID 0x{MessageId:X4}", messageId);
		}
	}

	/// <summary>
	/// Clear all registered handlers
	/// </summary>
	public void Clear()
	{
		var count = _handlers.Count;
		_handlers.Clear();
		_logger.LogDebug("[MessageDispatcher] Cleared all {Count} registered message handlers", count);
	}
}
