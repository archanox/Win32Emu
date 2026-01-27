using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Win32Emu.Gui.Models;

namespace Win32Emu.Gui.Services;

/// <summary>
/// Host service for MCP (Model Context Protocol) debugging server.
/// Provides AI assistants with tools to inspect and control the emulator.
/// </summary>
public class McpServerHost : IDisposable
{
	// NOTE: The MCP debug server is intentionally bound to localhost only.
	// This is a security measure to prevent remote access to emulator internals.
	// Do NOT make this configurable without carefully considering the risk of exposing
	// powerful debugging and control tools over the network.
	private const string McpBindAddress = "127.0.0.1";
	
	private readonly IHost? _host;
	private readonly ILogger _logger;
	private readonly EmulatorRuntimeService? _emulatorRuntime;
	private readonly EmulatorConfiguration _config;
	private bool _isRunning;

	public McpServerHost(EmulatorConfiguration config, EmulatorRuntimeService? emulatorRuntime, ILogger logger)
	{
		_config = config;
		_emulatorRuntime = emulatorRuntime;
		_logger = logger;
		_isRunning = false;

		try
		{
			if (_config.McpUseHttpTransport)
			{
				// Use HTTP transport - requires ASP.NET Core WebApplication
				var port = _config.McpHttpPort;
				var url = $"http://{McpBindAddress}:{port}";
				_logger.LogInformation("[MCP] Configuring HTTP transport at {Url}", url);

				// Create builder with specific URL
				var args = new[] { $"--urls={url}" };
				var builder = WebApplication.CreateBuilder(args);
				
				// Configure logging and MCP services
				ConfigureLogging(builder.Logging, logger);
				ConfigureMcpServices(builder.Services, config, emulatorRuntime, logger, useHttpTransport: true);
				
				var app = builder.Build();
				
				// Map MCP endpoints at root.
				// The MCP HTTP transport uses routes like POST `/initialize`, `/tools/list`, `/tools/call`.
				// Mapping at an extra prefix can cause clients to hit the wrong route (e.g., POST `/`) and get 404.
				app.MapMcp("");
				
				_host = app;
			}
			else
			{
				// Use STDIO transport for command-line AI tools
				_logger.LogInformation("[MCP] Configuring STDIO transport");
				
				var builder = Host.CreateApplicationBuilder();

				// Configure logging and MCP services
				ConfigureLogging(builder.Logging, logger);
			ConfigureMcpServices(builder.Services, config, emulatorRuntime, logger, useHttpTransport: false);

				_host = builder.Build();
			}
			
			_logger.LogInformation("[MCP] Server initialized and ready");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[MCP] Failed to initialize server");
			_host = null;
		}
	}

	/// <summary>
	/// Configure logging to use the emulator's logger
	/// </summary>
	private static void ConfigureLogging(ILoggingBuilder loggingBuilder, ILogger logger)
	{
		loggingBuilder.ClearProviders();
		loggingBuilder.AddProvider(new McpLoggerProvider(logger));
		loggingBuilder.SetMinimumLevel(LogLevel.Information);
	}

	/// <summary>
	/// Configure MCP services and register debug tools
	/// </summary>
	private static void ConfigureMcpServices(IServiceCollection services, EmulatorConfiguration config, EmulatorRuntimeService? emulatorRuntime, ILogger logger, bool useHttpTransport)
	{
		// Register the MCP debug tools
		services.AddSingleton(emulatorRuntime ?? new EmulatorRuntimeService(config, logger));
		services.AddSingleton<ILogger>(logger);  // Register with explicit interface type
		services.AddSingleton<McpDebugTools>();
		
		// Configure MCP server with appropriate transport
		var mcpBuilder = services.AddMcpServer();
		
		if (useHttpTransport)
		{
			mcpBuilder.WithHttpTransport();
		}
		else
		{
			mcpBuilder.WithStdioServerTransport();
		}
		
		mcpBuilder.WithTools<McpDebugTools>();  // Use WithTools for instance-based tools with DI
	}

	/// <summary>
	/// Start the MCP server
	/// </summary>
	public async Task StartAsync(CancellationToken cancellationToken = default)
	{
		if (_host == null)
		{
			_logger.LogWarning("[MCP] Cannot start server - initialization failed");
			return;
		}

		if (_isRunning)
		{
			_logger.LogWarning("[MCP] Server already running");
			return;
		}

		try
		{
			_logger.LogInformation("[MCP] Starting debugging server...");
			_logger.LogInformation("[MCP] AI assistants can now connect to inspect and control the emulator");
			_logger.LogInformation("[MCP] Available tools: GetEmulatorState, ReadMemory, SetBreakpoint, ContinueExecution, StepInstruction, and more");

			await _host.StartAsync(cancellationToken);
			_isRunning = true;

			_logger.LogInformation("[MCP] Server started successfully");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[MCP] Failed to start server");
			_isRunning = false;
		}
	}

	/// <summary>
	/// Stop the MCP server
	/// </summary>
	public async Task StopAsync(CancellationToken cancellationToken = default)
	{
		if (_host == null || !_isRunning)
		{
			return;
		}

		try
		{
			_logger.LogInformation("[MCP] Stopping debugging server...");
			await _host.StopAsync(cancellationToken);
			_isRunning = false;
			_logger.LogInformation("[MCP] Server stopped");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "[MCP] Error stopping server");
		}
	}

	/// <summary>
	/// Check if the MCP server is running
	/// </summary>
	public bool IsRunning => _isRunning;

	public void Dispose()
	{
		if (_isRunning)
		{
			StopAsync().GetAwaiter().GetResult();
		}
		_host?.Dispose();
	}

	/// <summary>
	/// Custom logger provider that forwards logs to the emulator's logger
	/// </summary>
	private class McpLoggerProvider : ILoggerProvider
	{
		private readonly ILogger _logger;

		public McpLoggerProvider(ILogger logger)
		{
			_logger = logger;
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new McpLogger(_logger, categoryName);
		}

		public void Dispose()
		{
		}
	}

	/// <summary>
	/// Custom logger that forwards to the emulator's logger
	/// </summary>
	private class McpLogger : ILogger
	{
		private readonly ILogger _logger;
		private readonly string _categoryName;

		public McpLogger(ILogger logger, string categoryName)
		{
			_logger = logger;
			_categoryName = categoryName;
		}

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		{
			return _logger.BeginScope(state);
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return _logger.IsEnabled(logLevel);
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			var message = $"[MCP:{_categoryName}] {formatter(state, exception)}";
			_logger.Log(logLevel, eventId, message, exception, (s, e) => s);
		}
	}
}
