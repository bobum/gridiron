using Microsoft.Extensions.Logging;
using System;

namespace GridironConsole;

/// <summary>
/// A simple console logger that only outputs the message without any metadata
/// (no log level, no category, no event ID, no timestamp)
/// </summary>
public class CleanConsoleLogger : ILogger
{
    private readonly string _categoryName;
    private readonly LogLevel _minLevel;

    public CleanConsoleLogger(string categoryName, LogLevel minLevel = LogLevel.Information)
    {
        _categoryName = categoryName;
        _minLevel = minLevel;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        if (!string.IsNullOrEmpty(message))
        {
            Console.WriteLine(message);
        }

        if (exception != null)
        {
            Console.WriteLine(exception.ToString());
        }
    }
}

/// <summary>
/// Logger provider for CleanConsoleLogger
/// </summary>
public class CleanConsoleLoggerProvider : ILoggerProvider
{
    private readonly LogLevel _minLevel;

    public CleanConsoleLoggerProvider(LogLevel minLevel = LogLevel.Information)
    {
        _minLevel = minLevel;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new CleanConsoleLogger(categoryName, _minLevel);
    }

    public void Dispose() { }
}

/// <summary>
/// Extension methods for adding CleanConsoleLogger
/// </summary>
public static class CleanConsoleLoggerExtensions
{
    public static ILoggingBuilder AddCleanConsole(this ILoggingBuilder builder, LogLevel minLevel = LogLevel.Information)
    {
        builder.AddProvider(new CleanConsoleLoggerProvider(minLevel));
        return builder;
    }
}
