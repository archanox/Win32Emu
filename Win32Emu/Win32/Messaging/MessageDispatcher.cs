using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Win32Emu.Win32.Messaging;

/// <summary>
/// Dispatches Win32 messages to registered async handlers
/// Inspired by DispatchR/MediatR pattern for zero-allocation, type-safe message handling
/// Fully asynchronous with cancellation token support
/// Thread-safe implementation using ConcurrentDictionary
/// </summary>
public class MessageDispatcher
{
	private readonly ConcurrentDictionary<uint, List<Func<IMessage, CancellationToken, Task<uint>>>> _handlers = new();
	private readonly ILogger _logger;

	public MessageDispatcher(ILogger? logger = null)
	{
		_logger = logger ?? NullLogger.Instance;
	}

	/// <summary>
	/// Register an async message handler for a specific message type
	/// </summary>
	/// <typeparam name="TMessage">The message type</typeparam>
	/// <param name="messageId">The Win32 message ID</param>
	/// <param name="handler">The async handler instance</param>
	public void RegisterHandler<TMessage>(uint messageId, IMessageHandler<TMessage> handler) 
		where TMessage : IMessage
	{
		var handlerList = _handlers.GetOrAdd(messageId, _ => new List<Func<IMessage, CancellationToken, Task<uint>>>());
		
		lock (handlerList)
		{
			// Wrap the strongly-typed handler in a function that accepts IMessage
			handlerList.Add(async (msg, ct) =>
			{
				if (msg is TMessage typedMsg)
				{
					return await handler.HandleAsync(typedMsg, ct);
				}
				_logger.LogWarning("[MessageDispatcher] Message type mismatch for message ID 0x{MessageId:X4}", messageId);
				return 0;
			});
		}

		_logger.LogDebug("[MessageDispatcher] Registered async handler for message ID 0x{MessageId:X4}", messageId);
	}

	/// <summary>
	/// Register an async lambda handler for a specific message type
	/// </summary>
	/// <param name="messageId">The Win32 message ID</param>
	/// <param name="handler">The async handler function</param>
	public void RegisterHandler(uint messageId, Func<IMessage, CancellationToken, Task<uint>> handler)
	{
		var handlerList = _handlers.GetOrAdd(messageId, _ => new List<Func<IMessage, CancellationToken, Task<uint>>>());
		
		lock (handlerList)
		{
			handlerList.Add(handler);
		}
		
		_logger.LogDebug("[MessageDispatcher] Registered async lambda handler for message ID 0x{MessageId:X4}", messageId);
	}

	/// <summary>
	/// Dispatch a message to all registered async handlers
	/// </summary>
	/// <param name="message">The message to dispatch</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result from the last handler, or 0 if no handlers</returns>
	public async Task<uint> DispatchAsync(IMessage message, CancellationToken cancellationToken = default)
	{
		if (!_handlers.TryGetValue(message.Message, out var handlers))
		{
			_logger.LogTrace("[MessageDispatcher] No handlers for message ID 0x{Message:X4}", message.Message);
			return 0;
		}

		uint result = 0;
		List<Func<IMessage, CancellationToken, Task<uint>>> handlersCopy;
		lock (handlers)
		{
			handlersCopy = new List<Func<IMessage, CancellationToken, Task<uint>>>(handlers);
		}

		foreach (var handler in handlersCopy)
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
		if (_handlers.TryRemove(messageId, out _))
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
		_logger.LogDebug("[MessageDispatcher] Cleared all handlers for {Count} message IDs", count);
	}
}
