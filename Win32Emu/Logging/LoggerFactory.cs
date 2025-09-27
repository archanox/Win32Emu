using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Win32Emu.Logging;

/// <summary>
/// Factory for creating loggers with proper OpenTelemetry configuration
/// </summary>
public static class LoggerFactory
{
    private static IServiceProvider? _serviceProvider;
    private static IWin32Logger? _logger;

    /// <summary>
    /// Initialize the logging system
    /// </summary>
    public static void Initialize(bool enableHttpExport = false, string? otlpEndpoint = null)
    {
        var services = new ServiceCollection();
        services.ConfigureOpenTelemetry(enableHttpExport, otlpEndpoint);
        
        _serviceProvider = services.BuildServiceProvider();
        _logger = _serviceProvider.GetRequiredService<IWin32Logger>();
    }

    /// <summary>
    /// Get the configured logger instance
    /// </summary>
    public static IWin32Logger GetLogger()
    {
        if (_logger == null)
        {
            throw new InvalidOperationException("Logger not initialized. Call LoggerFactory.Initialize() first.");
        }
        return _logger;
    }

    /// <summary>
    /// Create a scoped logger for a specific component
    /// </summary>
    public static IScopedLogger CreateScopedLogger(string componentName)
    {
        return new ScopedLogger(GetLogger(), componentName);
    }

    /// <summary>
    /// Dispose of the service provider
    /// </summary>
    public static void Dispose()
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
        _serviceProvider = null;
        _logger = null;
    }
}

/// <summary>
/// Interface for scoped loggers
/// </summary>
public interface IScopedLogger : IWin32Logger
{
    string ComponentName { get; }
}

/// <summary>
/// Implementation of a scoped logger that automatically creates scopes
/// </summary>
public class ScopedLogger : IScopedLogger
{
    private readonly IWin32Logger _logger;

    public string ComponentName { get; }

    public ScopedLogger(IWin32Logger logger, string componentName)
    {
        _logger = logger;
        ComponentName = componentName;
    }

    public void LogInformation(string message, params object[] args)
    {
        using var scope = _logger.BeginScope(ComponentName);
        _logger.LogInformation(message, args);
    }

    public void LogDebug(string message, params object[] args)
    {
        using var scope = _logger.BeginScope(ComponentName);
        _logger.LogDebug(message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        using var scope = _logger.BeginScope(ComponentName);
        _logger.LogWarning(message, args);
    }

    public void LogError(string message, params object[] args)
    {
        using var scope = _logger.BeginScope(ComponentName);
        _logger.LogError(message, args);
    }

    public void LogError(Exception ex, string message, params object[] args)
    {
        using var scope = _logger.BeginScope(ComponentName);
        _logger.LogError(ex, message, args);
    }

    public IDisposable BeginScope(string scopeName)
    {
        return _logger.BeginScope($"{ComponentName}.{scopeName}");
    }

    public IDisposable BeginScope<T>(T state)
    {
        return _logger.BeginScope(state);
    }
}