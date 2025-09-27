using Microsoft.Extensions.Logging;

namespace Win32Emu.Logging;

/// <summary>
/// Interface for Win32Emu structured logging with OpenTelemetry support
/// </summary>
public interface IWin32Logger
{
    /// <summary>
    /// Log information message
    /// </summary>
    void LogInformation(string message, params object[] args);

    /// <summary>
    /// Log debug message
    /// </summary>
    void LogDebug(string message, params object[] args);

    /// <summary>
    /// Log warning message
    /// </summary>
    void LogWarning(string message, params object[] args);

    /// <summary>
    /// Log error message
    /// </summary>
    void LogError(string message, params object[] args);

    /// <summary>
    /// Log error message with exception
    /// </summary>
    void LogError(Exception ex, string message, params object[] args);

    /// <summary>
    /// Begin a new logging scope for dependency chain tracking
    /// </summary>
    IDisposable BeginScope(string scopeName);

    /// <summary>
    /// Begin a new logging scope with additional properties
    /// </summary>
    IDisposable BeginScope<T>(T state);
}