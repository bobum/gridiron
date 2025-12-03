using System.Text;
using Microsoft.Extensions.Logging;

namespace Gridiron.WebApi.Services;

/// <summary>
/// A logger implementation that captures all logged messages to a string buffer
/// Used to capture play-by-play output during game simulation for persistence.
/// </summary>
public class StringCaptureLogger<T> : ILogger<T>
{
    private readonly StringBuilder _logBuffer = new ();
    private readonly object _lock = new ();

    /// <summary>
    /// Gets the captured log text with diagnostic messages filtered out
    /// Removes state transition messages and other debugging content.
    /// </summary>
    /// <returns></returns>
    public string GetCapturedLog()
    {
        lock (_lock)
        {
            var fullLog = _logBuffer.ToString();

            // Filter out diagnostic messages that aren't play-by-play content
            var lines = fullLog.Split('\n');
            var playByPlayLines = lines.Where(line =>
                !line.TrimStart().StartsWith("State transition:", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(line));

            return string.Join(Environment.NewLine, playByPlayLines);
        }
    }

    /// <summary>
    /// Clears the captured log buffer.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _logBuffer.Clear();
        }
    }

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        // No-op for scope - we're just capturing flat text
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        // Capture all log levels
        return true;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (formatter == null)
        {
            return;
        }

        var message = formatter(state, exception);

        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        lock (_lock)
        {
            _logBuffer.AppendLine(message);
        }
    }
}
