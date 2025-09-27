using Microsoft.Extensions.Logging;

namespace Win32Emu.Logging;

/// <summary>
/// Implementation of IWin32Logger using Microsoft.Extensions.Logging and OpenTelemetry
/// </summary>
public class Win32Logger : IWin32Logger
{
    private readonly ILogger _logger;

    public Win32Logger(ILogger<Win32Logger> logger)
    {
        _logger = logger;
    }

    public void LogInformation(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
    }

    public void LogDebug(string message, params object[] args)
    {
        _logger.LogDebug(message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
    }

    public void LogError(string message, params object[] args)
    {
        _logger.LogError(message, args);
    }

    public void LogError(Exception ex, string message, params object[] args)
    {
        _logger.LogError(ex, message, args);
    }

    public IDisposable BeginScope(string scopeName)
    {
        return _logger.BeginScope(scopeName);
    }

    public IDisposable BeginScope<T>(T state)
    {
        return _logger.BeginScope(state);
    }
}